# -*- coding: utf-8 -*-
"""
dotnet35_compat_scan.py — проверка C#-исходников на вещи, которых НЕТ в .NET Framework 3.5.

Зачем: проект «Табло» должен остаться на .NET 3.5, а писался код в среде, где под рукой
современный компилятор. Именно так в сборку 3 проскочил IntPtr.Add() (это .NET 4.0) —
ошибка CS0117. Этот скрипт ловит такие места ДО компиляции.

Что проверяется:
  1. API, появившиеся в 4.0/4.5/4.6+: IntPtr.Add/Subtract, String.IsNullOrWhiteSpace,
     Enum.HasFlag, Task/async/await, dynamic, Tuple, Lazy<T>, Concurrent*, SortedSet,
     BigInteger, Directory.Enumerate*, File.ReadLines, Marshal.SizeOf<T>,
     Environment.Is64Bit*, System.Numerics, HashAlgorithmName, SpinWait.
  2. Обобщённые делегаты Func<>/Action<> с числом аргументов больше 4 (в 3.5 максимум 4).
  3. string.Join(string, IEnumerable<string>) — 4.0 (вариант с string[] есть с 2.0).
  4. Path.Combine с 3+ аргументами — 4.0 (с двумя есть с 1.0).
  5. Monitor.TryEnter(obj, TimeSpan) — 4.0 (вариант с int-миллисекундами есть с 1.1).
  6. Интерполяция строк $"..." — C# 6 (на 3.5 не нужна, компилятор может быть старым).
  7. Режим --baseline: поиск «удалённых объявлений локальных переменных» — тот самый класс
     ошибки CS0103 из SettingForm.cs (убрали блок кода, а объявления переменных уехали
     вместе с ним, и нижестоящий код перестал компилироваться).

Запуск:
    python3 dotnet35_compat_scan.py каталог_или_файлы...
    python3 dotnet35_compat_scan.py --baseline /path/to/pristine repo/

Код возврата: 0 — чисто, 1 — найдены проблемы, 2 — ошибка запуска.
"""
import io
import os
import re
import sys

# ---------- 1. простые паттерны «этого нет в 3.5» ----------
BAD_API = [
    (r'\bIntPtr\s*\.\s*(?:Add|Subtract)\s*\(',            'IntPtr.Add/Subtract — только .NET 4.0'),
    (r'\b[sS]tring\s*\.\s*IsNullOrWhiteSpace\s*\(',       'String.IsNullOrWhiteSpace — только .NET 4.0'),
    (r'\.HasFlag\s*\(',                                   'Enum.HasFlag — только .NET 4.0'),
    (r'\bTask\s*<|\bTask\s*\.\s*(?:Run|Factory|Delay)\b', 'Task — только .NET 4.5'),
    (r'\basync\s+(?:\w+\s+)*\w+\s*\(|\bawait\s+',         'async/await — только .NET 4.5'),
    (r'\bdynamic\s+\w+',                                  'dynamic — только .NET 4.0'),
    (r'\bTuple\s*<|\bTuple\s*\.\s*Create',                'Tuple — только .NET 4.0'),
    (r'\bLazy\s*<',                                       'Lazy<T> — только .NET 4.0'),
    (r'\bConcurrent(?:Dictionary|Queue|Bag)\b|\bBlockingCollection\b',
                                                          'System.Collections.Concurrent — только .NET 4.0'),
    (r'\bSortedSet\s*<',                                  'SortedSet<T> — только .NET 4.0'),
    (r'\bBigInteger\b',                                   'BigInteger — только .NET 4.0'),
    (r'\bIs64Bit(?:Process|OperatingSystem)\b',           'Environment.Is64Bit* — только .NET 4.0'),
    (r'\bDirectory\s*\.\s*Enumerate\w+',                  'Directory.Enumerate* — только .NET 4.0'),
    (r'\bFile\s*\.\s*ReadLines\b',                        'File.ReadLines — только .NET 4.0'),
    (r'Marshal\s*\.\s*(?:SizeOf|PtrToStructure|StructureToPtr)\s*<',
                                                          'обобщённые Marshal.* — только .NET 4.5.1'),
    (r'\bSystem\s*\.\s*Numerics\b',                       'System.Numerics — только .NET 4.0'),
    (r'\bHashAlgorithmName\b',                            'HashAlgorithmName — только .NET 4.6.1'),
    (r'\bSpinWait\b',                                     'SpinWait — только .NET 4.0'),
    (r'\bSystem\s*\.\s*Reflection\s*\.\s*\w*TypeInfo\b',  'TypeInfo — только .NET 4.5'),
    (r'\bConfigureAwait\s*\(|\bIProgress\s*<|\bCallerMemberName\b',
                                                          '4.5+'),
]

# ---------- вспомогательное: разбить аргументы по запятым верхнего уровня ----------
def split_top(s):
    out, depth, cur = [], 0, ''
    for ch in s:
        if ch in '<([':
            depth += 1
        elif ch in '>)]':
            depth -= 1
        if ch == ',' and depth == 0:
            out.append(cur)
            cur = ''
        else:
            cur += ch
    out.append(cur)
    return [a.strip() for a in out if a.strip()]


def call_args(code, open_paren_pos):
    """Аргументы вызова, начиная с позиции открывающей скобки."""
    depth, end = 0, None
    for j in range(open_paren_pos, len(code)):
        if code[j] == '(':
            depth += 1
        elif code[j] == ')':
            depth -= 1
            if depth == 0:
                end = j
                break
    return code[open_paren_pos + 1:end] if end is not None else code[open_paren_pos + 1:]


def strip_comments_and_strings(line):
    """Убрать // комментарии и СОДЕРЖИМОЕ строк (чтобы "pbkdf2$" не считался интерполяцией)."""
    out, i, n = [], 0, len(line)
    while i < n:
        ch = line[i]
        if ch == '/' and i + 1 < n and line[i + 1] == '/':
            break
        if ch == '"':
            verb = i > 0 and line[i - 1] == '@'
            i += 1
            while i < n:
                if line[i] == '"':
                    if verb and i + 1 < n and line[i + 1] == '"':
                        i += 2
                        continue
                    i += 1
                    break
                i += 1
            out.append('""')
            continue
        if ch == "'":
            i += 1
            while i < n and line[i] != "'":
                i += 1
            i += 1
            out.append("' '")
            continue
        out.append(ch)
        i += 1
    return ''.join(out)


def scan_file(path, raw_lines):
    problems = []
    in_block_comment = False
    for idx, raw in enumerate(raw_lines, 1):
        line = raw
        if in_block_comment:
            if '*/' in line:
                line = line.split('*/', 1)[1]
                in_block_comment = False
            else:
                continue
        if '/*' in line and '*/' not in line:
            line = line.split('/*', 1)[0]
            in_block_comment = True
        code = strip_comments_and_strings(line)

        for pat, label in BAD_API:
            if re.search(pat, code):
                problems.append((idx, '%s  |  %s' % (label, raw.strip()[:110])))

        for m in re.finditer(r'\bFunc\s*<', code):
            args = split_top(call_args(code, code.index('<', m.start())))
            if len(args) > 4:
                problems.append((idx, 'Func<> с %d аргументами — в 3.5 максимум 4' % len(args)))
        for m in re.finditer(r'\bAction\s*<', code):
            args = split_top(call_args(code, code.index('<', m.start())))
            if len(args) > 4:
                problems.append((idx, 'Action<> с %d аргументами — в 3.5 максимум 4' % len(args)))

        for m in re.finditer(r'\b[sS]tring\s*\.\s*Join\s*\(', code):
            inner = call_args(code, m.end() - 1)
            if 'IEnumerable' in inner or re.search(r'=>|\.Select\(|\.Where\(', inner):
                problems.append((idx, 'string.Join(string, IEnumerable<string>) — только .NET 4.0'))
        for m in re.finditer(r'\bPath\s*\.\s*Combine\s*\(', code):
            inner = call_args(code, m.end() - 1)
            if len(split_top(inner)) > 2:
                problems.append((idx, 'Path.Combine с %d аргументами — только .NET 4.0'
                                 % len(split_top(inner))))
        for m in re.finditer(r'\bMonitor\s*\.\s*TryEnter\s*\(', code):
            if 'TimeSpan' in call_args(code, m.end() - 1):
                problems.append((idx, 'Monitor.TryEnter(obj, TimeSpan) — только .NET 4.0'))

        if '$"' in code:
            problems.append((idx, 'интерполяция строк $"..." — C# 6, на .NET 3.5 не нужна'))
    return problems


# ---------- 7. удалённые объявления локальных переменных (CS0103) ----------
DECL = re.compile(
    r'^\s*(?:(?:var|const)\b|(?:(?:[\w\.]+)(?:<[^;=]*?>)?(?:\[\s*\])*)\s+)(\w+)\s*(?:=[^=]|;|\[)')
KW = set('''if else for foreach while switch case return new this base out ref in public private
protected internal static readonly void class struct enum interface delegate try catch finally lock
using do get set add remove throw yield unsafe fixed checked unchecked namespace goto break continue
default where operator event override virtual abstract sealed partial extern async await'''.split())


def removed_declarations(base_text, new_text):
    """Имена, объявленные в удалённых строках, но используемые в новом тексте без объявления."""
    new_lines = set(l.strip() for l in new_text.split('\n'))
    hits = []
    names = {}
    for l in base_text.split('\n'):
        c = re.sub(r'//.*$', '', l).strip()
        if not c or c in new_lines:
            continue
        m = DECL.match(c)
        if not m:
            continue
        n = m.group(1)
        if n not in KW and not n[0].isupper():
            names.setdefault(n, []).append(c[:80])
    for n, srcs in sorted(names.items()):
        decl = re.search(r'(?:\bvar\b|\b[\w\.]+(?:<[^;=]*?>)?(?:\[\s*\])*)\s+' + re.escape(n) +
                         r'\s*(?:=[^=]|;|\[)', new_text)
        used = re.search(r'\b' + re.escape(n) + r'\b', new_text)
        if used and not decl:
            hits.append((n, srcs))
    return hits


def collect_cs(paths):
    out = []
    for p in paths:
        if os.path.isdir(p):
            for root, dirs, files in os.walk(p):
                dirs[:] = [d for d in dirs if d not in
                           ('obj', 'bin', '.git', 'packages', '.vs')]
                for f in sorted(files):
                    if f.endswith('.cs'):
                        out.append(os.path.join(root, f))
        elif p.endswith('.cs'):
            out.append(p)
    return out


def read(p):
    return io.open(p, encoding='utf-8-sig', newline='').read()


def main(argv):
    baseline = None
    if '--baseline' in argv:
        i = argv.index('--baseline')
        baseline = argv[i + 1]
        argv = argv[:i] + argv[i + 2:]
    files = collect_cs(argv)
    if not files:
        print('Не найдено ни одного .cs. Запуск: python3 %s <каталог|файлы...> [--baseline <эталон>]'
              % os.path.basename(argv[0] if argv else __file__))
        return 2

    bad = 0
    print('=' * 78)
    print('1. API и языковые средства, отсутствующие в .NET Framework 3.5')
    print('=' * 78)
    for f in files:
        probs = scan_file(f, read(f).split('\n'))
        for ln, msg in probs:
            bad += 1
            print('  %s:%d  %s' % (f, ln, msg))
    print('  проверено файлов: %d, находок: %d' % (len(files), bad))

    if baseline:
        print()
        print('=' * 78)
        print('2. Удалённые объявления локальных переменных (класс ошибки CS0103)')
        print('=' * 78)
        n2 = 0
        for f in files:
            rel = os.path.relpath(f, argv[0] if os.path.isdir(argv[0]) else os.path.dirname(f) or '.')
            base = os.path.join(baseline, rel.replace(os.sep, '/'))
            if not os.path.exists(base):
                continue
            for name, srcs in removed_declarations(read(base), read(f)):
                n2 += 1
                print('  %s: "%s" — объявление удалено, но имя используется в файле' % (f, name))
                for s in srcs:
                    print('        было: %s' % s)
                print('        -> проверьте вручную: возможно, имя стало параметром метода')
                print('           или встречается только в комментариях (ложное срабатывание)')
        print('  подозрений: %d' % n2)
        bad += 0  # подозрения не считаем фатальными — они требуют ручной проверки
    return 1 if bad else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
