# -*- coding: utf-8 -*-
"""
cs_struct_check.py — структурная проверка C#-файлов (без компилятора).

Что делает:
  1. Лексирует файл (строки, @"...", символы, комментарии, препроцессор).
  2. Считает баланс {} и () — должен быть 0 в конце файла.
  3. Ищет «сиротские» конструкции: строки кода (не объявления/атрибуты),
     которые встречаются на уровне членов класса (depth == 1) или на уровне
     namespace (depth == 0). Именно так выглядела ошибка CS1519/CS0116
     в TabloPatch.cs (задвоенное тело ClosePort()).

Запуск:  python3 cs_struct_check.py файл1.cs файл2.cs ...
Код возврата: 0 — чисто, 1 — найдены проблемы.
"""
import re
import sys

DECL_HEAD = re.compile(
    r'^\s*(?:\[[^\]]*\]\s*)*'                                   # атрибуты
    r'(?:(?:public|private|protected|internal|static|virtual|override|abstract|'
    r'sealed|readonly|const|volatile|extern|unsafe|partial|new|async|implicit|explicit)\s+)*'
    r'(?:namespace|class|struct|interface|enum|delegate|event|void)\b')

MEMBER = re.compile(
    r'^\s*(?:\[[^\]]*\]\s*)*'
    r'(?:(?:public|private|protected|internal|static|virtual|override|abstract|'
    r'sealed|readonly|const|volatile|extern|unsafe|partial|new|async|implicit|explicit)\s+)+'
    r'[\w<>\[\],\.\?\s]+\s+[\w~]+')          # модификатор обязателен -> похоже на объявление

CLOSE_ONLY = re.compile(r'^\s*[\{\}\(\)\];,]*\s*(//.*)?$')   # только скобки -> не «код»
DIRECTIVE = re.compile(r'^\s*#')


def strip_code(text):
    """Возвращает список строк, где содержимое строк/комментариев заменено пробелами."""
    out = []
    buf = []
    i, n = 0, len(text)
    state = 'code'          # code | line_c | block_c | str | verbatim | char
    while i < n:
        c = text[i]
        nxt = text[i + 1] if i + 1 < n else ''
        if state == 'code':
            if c == '/' and nxt == '/':
                state = 'line_c'; buf.append('  '); i += 2; continue
            if c == '/' and nxt == '*':
                state = 'block_c'; buf.append('  '); i += 2; continue
            if c == '@' and nxt == '"':
                state = 'verbatim'; buf.append('  '); i += 2; continue
            if c == '"':
                state = 'str'; buf.append(' '); i += 1; continue
            if c == "'":
                state = 'char'; buf.append(' '); i += 1; continue
            buf.append(c); i += 1; continue
        if state == 'line_c':
            if c == '\n':
                state = 'code'; buf.append(c)
            else:
                buf.append(' ')
            i += 1; continue
        if state == 'block_c':
            if c == '*' and nxt == '/':
                state = 'code'; buf.append('  '); i += 2; continue
            buf.append(c if c == '\n' else ' '); i += 1; continue
        if state == 'str':
            if c == '\\':
                buf.append('  '); i += 2; continue
            if c == '"':
                state = 'code'; buf.append(' ')
            elif c == '\n':
                state = 'code'; buf.append(c)   # незакрытая строка — не «съедаем» файл
            else:
                buf.append(' ')
            i += 1; continue
        if state == 'verbatim':
            if c == '"' and nxt == '"':
                buf.append('  '); i += 2; continue
            if c == '"':
                state = 'code'; buf.append(' ')
            elif c == '\n':
                buf.append(c)
            else:
                buf.append(' ')
            i += 1; continue
        if state == 'char':
            if c == '\\':
                buf.append('  '); i += 2; continue
            if c == "'":
                state = 'code'; buf.append(' ')
            elif c == '\n':
                state = 'code'; buf.append(c)
            else:
                buf.append(' ')
            i += 1; continue
    return ''.join(buf), state


def check(path):
    raw = open(path, 'rb').read().decode('utf-8-sig', 'replace')
    code, end_state = strip_code(raw)
    problems = []
    if end_state != 'code':
        problems.append((0, 'файл заканчивается внутри лексемы: %s' % end_state))

    # --- проверка «сиротского» открывающего блока -------------------------
    # Одиночное '{' на уровне членов класса допустимо только после заголовка
    # метода/свойства/конструктора. Если предыдущая значимая строка — это '}'
    # (конец предыдущего метода) или что-то, что не может открывать блок,
    # значит тело метода задвоилось / осталось вне класса (ошибки CS1519).
    BLOCK_OPENERS = (')', '=>', 'else', 'try', 'finally', 'do', 'lock',
                     'get', 'set', 'add', 'remove', 'checked', 'unchecked',
                     'unsafe', 'namespace')

    def orphan_check(codelines, rawls):
        """Ищет «сиротские» блоки.

        Одиночное '{' на уровне членов класса допустимо только после заголовка
        метода/свойства/конструктора. Если предыдущая значимая строка — это '}'
        (конец предыдущего метода) или иная строка, не способная открывать блок,
        значит тело метода задвоилось и осталось ВНЕ класса. Компилятор это
        показывает как CS1519 / CS0116 / CS1022 — ровно та ошибка, что была
        в TabloPatch.cs (двойное тело ClosePort()).
        """
        res = []
        d = 0
        prev = ''
        prev_ln = 0
        body_depth = None      # глубина, на которой живут тела методов
        for idx, line in enumerate(codelines):
            s2 = line.strip()
            before = d
            for ch in line:
                if ch == '{':
                    d += 1
                elif ch == '}':
                    d -= 1
            # «namespace X {» / «public class Y {» в одну строку: тела методов
            # живут на глубине before (до этой скобки).
            if '{' in line and DECL_HEAD.match(line):
                body_depth = before
            # заголовок типа на предыдущей строке, '{' — на своей
            elif s2 == '{' and DECL_HEAD.match(prev):
                body_depth = before
            if (s2 == '{' and body_depth is not None and before == body_depth
                    and not DECL_HEAD.match(prev)):
                pt = prev.rstrip()
                tail = pt.rsplit('}', 1)[-1].strip()
                ok = bool(tail) and (
                    tail.endswith(BLOCK_OPENERS)
                    or MEMBER.match(prev)
                    or tail.endswith(':') or tail.endswith(','))
                if not ok:
                    res.append((idx + 1,
                                "сиротский блок '{' после строки %d (%r) — тело метода вне класса?"
                                % (prev_ln, rawls[prev_ln - 1].strip()[:90])))
            if s2 and not DIRECTIVE.match(s2):
                prev = line
                prev_ln = idx + 1
        return res

    depth = 0
    par = 0
    min_depth = 0
    # splitlines() важен: в файле попадаются «голые» CR (перевод строки без LF),
    # иначе две физические строки склеиваются в одну и номера строк «едут».
    lines = code.splitlines()
    rawlines = raw.splitlines()
    for idx, line in enumerate(lines):
        ln = idx + 1
        s = line.strip()
        # запоминаем глубину ПЕРЕД строкой — для поиска «кода на уровне членов»
        depth_before = depth
        for ch in line:
            if ch == '{':
                depth += 1
            elif ch == '}':
                depth -= 1
                if depth < min_depth:
                    min_depth = depth
            elif ch == '(':
                par += 1
            elif ch == ')':
                par -= 1
        if s and not DIRECTIVE.match(s) and not CLOSE_ONLY.match(s):
            if depth_before <= 1:
                looks_decl = (DECL_HEAD.match(line) or MEMBER.match(line)
                              or s.startswith('using ')
                              or s.startswith('[') and s.endswith(']')
                              or s.startswith('case ') or s.startswith('default:')
                              or s.startswith('else') or s.startswith('get')
                              or s.startswith('set') or s.startswith('add')
                              or s.startswith('remove') or s.startswith('assembly:')
                              or ':' in s.split(' ')[0] and DECL_HEAD.match(line))
                if not looks_decl:
                    problems.append((ln, 'код на уровне %s: %s'
                                     % ('namespace' if depth_before == 0 else 'членов класса',
                                        rawlines[idx].strip()[:110])))
    problems.extend(orphan_check(lines, rawlines))
    if depth != 0:
        problems.append((0, 'баланс {} = %+d (не 0)' % depth))
    if par != 0:
        problems.append((0, 'баланс () = %+d (не 0)' % par))
    if min_depth < 0:
        problems.append((0, 'лишние закрывающие }: глубина уходила в %d' % min_depth))
    return problems


def main():
    bad = 0
    for path in sys.argv[1:]:
        probs = check(path)
        if probs:
            bad += 1
            print('=== %s — ПРОБЛЕМЫ (%d) ===' % (path, len(probs)))
            for ln, msg in probs[:40]:
                print('  строка %s: %s' % (ln, msg))
            if len(probs) > 40:
                print('  ... и ещё %d' % (len(probs) - 40))
        else:
            print('OK  %s' % path)
    return 1 if bad else 0


if __name__ == '__main__':
    sys.exit(main())
