// ============================================================
// ФАЙЛ: TabloPatch.cs  (версия под .NET Framework 3.5, C# 3.0/4.0)
// Класть целиком в папку проекта Tablo, рядом с frmMain.cs
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace TabloPatch
{
    public static class TimeFmt
    {
        public static string Fmt(int totalSec)
        {
            if (totalSec < 0) totalSec = 0;
            return string.Format("{0:D2}:{1:D2}", totalSec / 60, totalSec % 60);
        }

        public static bool TryParse(string s, out int sec)
        {
            sec = 0;
            if (s == null) return false;
            string t = s.Trim();
            if (t.Length == 0) return false;
            string[] p = t.Split(':');
            int m, ss;
            if (p.Length == 2 && int.TryParse(p[0], out m) && int.TryParse(p[1], out ss))
            { sec = m * 60 + ss; return true; }
            if (p.Length == 1 && int.TryParse(p[0], out m))
            { sec = m * 60; return true; }
            return false;
        }
    }

    public class PenaltyInfo
    {
        public bool HomeTeam;
        public int PlayerNumber;
        public int SecondsTotal;
        public int SecondsLeft;

        public override string ToString()
        {
            return (HomeTeam ? "ХОЗ" : "ГОС") + " №" + PlayerNumber + " " + TimeFmt.Fmt(SecondsLeft);
        }
    }

    public class GameClockCore
    {
        // существующие настройки программы
        public int PeriodTimeSec { get; set; }
        public int OvertimeTimeSec { get; set; }
        public bool MainClockCountDown { get; set; }    // ФУНКЦИЯ 1: по умолчанию обратный ход
        public bool PrestartCountDown { get; set; }     // БАГ 3: по умолчанию обратный ход

        // НОВЫЕ ЭТАЛОН-ПЕРЕМЕННЫЕ (защита времени)
        private int savedMainPeriodTimeSec = -1;        // переменная 1: основное время
        private int savedOvertimeTimeSec = -1;          // переменная 2: овертайм
        public int SavedMainPeriodTimeSec { get { return savedMainPeriodTimeSec; } }
        public int SavedOvertimeTimeSec { get { return savedOvertimeTimeSec; } }
        public bool MainTimeMemoryLocked { get; private set; }
        public bool OtTimeMemoryLocked { get; private set; }

        private readonly Dictionary<int, int> periodInitialTimeSec = new Dictionary<int, int>();
        private bool periodTimeWasCorrected;
        private int periodEndRestoreDoneFor = -1;       // восстановление ОДИН раз в конце периода

        public int CurrentPeriod { get; private set; }
        public int ClockDisplaySec { get; private set; }
        public int CurrentPeriodLengthSec { get; private set; }
        public bool Running { get; private set; }
        public int HomeScore { get; private set; }
        public int AwayScore { get; private set; }
        private readonly List<PenaltyInfo> penalties = new List<PenaltyInfo>();
        public List<PenaltyInfo> Penalties { get { return penalties; } }

        public GameClockCore()
        {
            CurrentPeriod = 1;
            PeriodTimeSec = 20 * 60;
            OvertimeTimeSec = 5 * 60;
            MainClockCountDown = true;
            PrestartCountDown = true;
            CurrentPeriodLengthSec = PeriodTimeSec;
            ClockDisplaySec = PeriodTimeSec;
        }

        public bool IsOvertime { get { return CurrentPeriod >= 4; } }
        public bool IsCountDownNow { get { return IsOvertime ? true : MainClockCountDown; } }

        public int DomainSettingSec
        {
            get { return IsOvertime ? OvertimeTimeSec : PeriodTimeSec; }
            set { if (IsOvertime) OvertimeTimeSec = value; else PeriodTimeSec = value; }
        }
        public int DomainSavedSec { get { return IsOvertime ? savedOvertimeTimeSec : savedMainPeriodTimeSec; } }

        public event Action<string> Log;
        public event Action StateChanged;
        private void Raise() { if (StateChanged != null) StateChanged(); }
        private void LogIt(string s) { if (Log != null) Log(s); }

        // кнопка настроек "Запомнить время периодов"
        public void RememberPeriodTimesFromSettings()
        {
            if (MainTimeMemoryLocked && OtTimeMemoryLocked)
            {
                LogIt("ЗАПОМИНАНИЕ: эталон уже запомнен. Разблокировка - мастер-сброс или ручная правка эталона.");
                return;
            }
            savedMainPeriodTimeSec = PeriodTimeSec; MainTimeMemoryLocked = true;
            savedOvertimeTimeSec = OvertimeTimeSec; OtTimeMemoryLocked = true;
            LogIt("ЗАПОМИНАНИЕ ЭТАЛОНА: осн.=" + TimeFmt.Fmt(savedMainPeriodTimeSec) +
                  ", овертайм=" + TimeFmt.Fmt(savedOvertimeTimeSec) + " (заблокировано)");
            Raise();
        }

        // аварийная ручная правка эталона (только из отдельных полей настроек)
        public void ManualEditSavedMainTime(int sec)
        {
            savedMainPeriodTimeSec = sec; MainTimeMemoryLocked = true;
            LogIt("РУЧНАЯ ПРАВКА ЭТАЛОНА осн.: " + TimeFmt.Fmt(sec)); Raise();
        }
        public void ManualEditSavedOtTime(int sec)
        {
            savedOvertimeTimeSec = sec; OtTimeMemoryLocked = true;
            LogIt("РУЧНАЯ ПРАВКА ЭТАЛОНА овертайма: " + TimeFmt.Fmt(sec)); Raise();
        }

        private void EnsureMemoryLocked()
        {
            // триггер: первый старт периода сам запоминает эталон
            if (IsOvertime)
            {
                if (savedOvertimeTimeSec < 0)
                {
                    savedOvertimeTimeSec = OvertimeTimeSec; OtTimeMemoryLocked = true;
                    LogIt("ЗАПОМИНАНИЕ ПРИ СТАРТЕ: овертайм=" + TimeFmt.Fmt(savedOvertimeTimeSec));
                }
            }
            else
            {
                if (savedMainPeriodTimeSec < 0)
                {
                    savedMainPeriodTimeSec = PeriodTimeSec; MainTimeMemoryLocked = true;
                    LogIt("ЗАПОМИНАНИЕ ПРИ СТАРТЕ: осн.=" + TimeFmt.Fmt(savedMainPeriodTimeSec));
                }
            }
        }

        private void PrepareClock()
        {
            EnsureMemoryLocked();
            int len;
            if (!periodInitialTimeSec.TryGetValue(CurrentPeriod, out len))
            { len = DomainSettingSec; periodInitialTimeSec[CurrentPeriod] = len; }
            CurrentPeriodLengthSec = len;
            ClockDisplaySec = IsCountDownNow ? len : 0;
            periodTimeWasCorrected = false;
            periodEndRestoreDoneFor = -1;
        }

        public void StartClock()
        {
            PrepareClock(); Running = true;
            LogIt("СТАРТ периода " + CurrentPeriod + ": табло=" + TimeFmt.Fmt(ClockDisplaySec) +
                  ", ход=" + (IsCountDownNow ? "обратный" : "прямой"));
            Raise();
        }

        public void StopClock()
        {
            Running = false;
            LogIt("СТОП времени, на табло " + TimeFmt.Fmt(ClockDisplaySec));
            Raise();
        }

        // БЛОК 7 (аудит), М7: метод Tick1Sec() УДАЛЁН — он не вызывался НИ ОТКУДА
        // (проверено по всему проекту). Настоящий отсчёт времени живёт в frmMain
        // (_timerGame_Tick + Patch_GameTimeRefresh), удаления — в списках lB_DelPeople_*.
        // Мёртвый метод вводил в заблуждение: казалось, что GameClockCore сам считает
        // секунды и штрафы, хотя он хранит только ЭТАЛОН длины периода/овертайма.

        public void PeriodNext()
        {
            OnPeriodEnd();
            if (CurrentPeriod < 9) CurrentPeriod++;
            PrepareClock();
            LogIt("ПЕРИОД -> " + CurrentPeriod + (IsOvertime ? " (овертайм)" : ""));
            Raise();
        }

        public void PeriodPrev()
        {
            if (CurrentPeriod > 1) CurrentPeriod--;
            PrepareClock();
            LogIt("ПЕРИОД -> " + CurrentPeriod);
            Raise();
        }
              public void SetCurrentPeriod(int period)
        {
            if (period != CurrentPeriod)
            {
                periodEndRestoreDoneFor = -1;
            }
            CurrentPeriod = period;
        }


        // конец периода: сверка настройки с эталоном, восстановление ОДИН раз
        public void OnPeriodEnd()
        {
            if (periodEndRestoreDoneFor == CurrentPeriod) return;
            periodEndRestoreDoneFor = CurrentPeriod;
            Running = false;
            int saved = DomainSavedSec;
            if (periodTimeWasCorrected && saved > 0 && DomainSettingSec != saved)
            {
                string broken = TimeFmt.Fmt(DomainSettingSec);
                DomainSettingSec = saved;
                LogIt("КОНЕЦ периода " + CurrentPeriod + ": ВРЕМЯ НЕ СОВПАДАЛО (было " + broken +
                      ") -> восстановлено из эталона " + TimeFmt.Fmt(saved));
            }
            else
            {
                LogIt("КОНЕЦ периода " + CurrentPeriod + ": настройка и эталон совпадают");
            }
            periodTimeWasCorrected = false;
            Raise();
        }

        // БАГ 4 + ФУНКЦИЯ 3: корректировка применяется СРАЗУ и ТОЧНО, эталон не трогается
        public void ApplyManualTimeCorrection(int sec)
        {
            if (sec < 0) sec = 0;
            ClockDisplaySec = sec;
            DomainSettingSec = sec;
            periodTimeWasCorrected = true;
            periodEndRestoreDoneFor = -1;
            LogIt("КОРРЕКТИРОВКА: выставлено ТОЧНО " + TimeFmt.Fmt(sec) + " (без вычитания прошедшего)");
            Raise();
        }

        // кнопка "Сброс времени" оставлена рабочей: возврат к эталону
        public void ResetClockToSavedTime()
        {
            int saved = DomainSavedSec;
            ClockDisplaySec = (saved > 0) ? saved : DomainSettingSec;
            CurrentPeriodLengthSec = ClockDisplaySec;
            Running = false;
            LogIt("СБРОС ВРЕМЕНИ: возвращено к эталону " + TimeFmt.Fmt(ClockDisplaySec));
            Raise();
        }

        // БАГИ 1 и 2: единый путь удаления для периодов 1..9
        public void AddPenalty(bool home, int playerNumber, int minutes)
        {
            int sec = minutes * 60;
            PenaltyInfo existing = null;
            for (int i = 0; i < penalties.Count; i++)
            {
                if (penalties[i].HomeTeam == home && penalties[i].PlayerNumber == playerNumber)
                { existing = penalties[i]; break; }
            }
            if (existing != null)
            {
                existing.SecondsTotal = sec; existing.SecondsLeft = sec;
                LogIt("УДАЛЕНИЕ ОБНОВЛЕНО: " + existing.ToString());
            }
            else
            {
                PenaltyInfo p = new PenaltyInfo();
                p.HomeTeam = home; p.PlayerNumber = playerNumber;
                p.SecondsTotal = sec; p.SecondsLeft = sec;
                penalties.Add(p);
                LogIt("УДАЛЕНИЕ ДОБАВЛЕНО: " + p.ToString() + " (период " + CurrentPeriod + ")");
            }
            Raise();
        }

        public void RemovePenalizedPlayer(int playerNumber)
        {
            int before = penalties.Count;
            for (int i = penalties.Count - 1; i >= 0; i--)
            {
                if (penalties[i].PlayerNumber == playerNumber) penalties.RemoveAt(i);
            }
            if (penalties.Count < before)
                LogIt("УДАЛЕНИЕ СНЯТО: игрок №" + playerNumber + " убран с табло немедленно (период " + CurrentPeriod + ")");
            else
                LogIt("УДАЛЕНИЕ СНЯТО: игрок №" + playerNumber + " не найден среди удалённых");
            Raise();
        }

        public void ChangeScore(bool home, int delta)
        {
            if (home) HomeScore = Math.Max(0, HomeScore + delta);
            else AwayScore = Math.Max(0, AwayScore + delta);
            LogIt("СЧЁТ: " + HomeScore + ":" + AwayScore);
            Raise();
        }

        // МАСТЕР-СБРОС: единственная полная разблокировка эталонов
        public void MasterReset()
        {
            Running = false;
            CurrentPeriod = 1;
            HomeScore = 0; AwayScore = 0;
            penalties.Clear();
            savedMainPeriodTimeSec = -1; savedOvertimeTimeSec = -1;
            MainTimeMemoryLocked = false; OtTimeMemoryLocked = false;
            periodInitialTimeSec.Clear();
            periodTimeWasCorrected = false;
            periodEndRestoreDoneFor = -1;
            CurrentPeriodLengthSec = PeriodTimeSec;
            ClockDisplaySec = MainClockCountDown ? PeriodTimeSec : 0;
            LogIt("МАСТЕР-СБРОС: эталоны разблокированы и очищены");
            Raise();
        }
    }

    // БАГ 3: прямой ход заставки реально идёт вверх (00:00 -> total)
    public static class SplashClock
    {
        public static int DisplaySec(int elapsedSec, int totalSec, bool countDown)
        {
            if (countDown)
            {
                int v = totalSec - elapsedSec;
                return v < 0 ? 0 : v;
            }
            return elapsedSec > totalSec ? totalSec : elapsedSec;
        }
    }

    public static class SplashFormPatch
    {
        public static string PrestartDisplay(int elapsedSec, int totalSec, bool countDown)
        {
            return TimeFmt.Fmt(SplashClock.DisplaySec(elapsedSec, totalSec, countDown));
        }
    }

    public enum RemoteCmd
    {
        Start, Stop, PeriodNext, PeriodPrev,
        TimeSetSave, TimeResetToSaved,
        PeriodNextForce, PeriodPrevForce,
        PenaltyAdd, PenaltyRemovePlayer,
        ScoreHomePlus, ScoreHomeMinus, ScoreAwayPlus, ScoreAwayMinus,
        MasterReset, SplashShow, SplashHide,
        TimeoutRequest, AdvertPause, Intermission,   // недоделаны: безопасный no-op
        TimeoutHome, TimeoutAway,
        CounterSet, ScoreSet, WhistleBeep, WhistleStart, WhistleStop,
        BreakTimeSet, TimeoutTimeSet, ScriptPlay, ScriptStop,
        SetPeriodValue, TimeResetOnly, DelGamerPacket,
        // БЛОК 7 (аудит), М1: команды, которых не хватало, чтобы физический пульт шёл
        // через ТОТ ЖЕ диспетчер, что веб-панель и эмулятор (раньше у пульта были свои
        // 15 обработчиков со своей валидацией — отсюда Б3/Б4/Б6: в веб-пути проверка
        // была, в пультовом нет).
        TimeoutCountersSet, BreakModeSet, NameSet
    }

    public struct RemoteCommand
    {
        public RemoteCmd Cmd;
        public int TimeSec;
        public int TimeMs;   // точное время в миллисекундах (0 = не задано, брать TimeSec)
        public bool HomeTeam;
        public int PlayerNumber;
        public int Minutes;
        // ===== БЛОК 7 (аудит), М1: поля, нужные для полного описания кадров пульта =====
        // Значения по умолчанию (false/0/null) сохраняют прежнее поведение веб-пути и
        // эмулятора: они эти поля не заполняют.
        public bool UseStrokeInfo;    // TimeSetSave: брать ход часов и авто-свисток из кадра, а не из настроек
        public bool ForwardStroke;    //   значимо при UseStrokeInfo == true
        public bool AutoWhistle;      //   значимо при UseStrokeInfo == true
        public bool ShowVideo;        // TimeoutCountersSet / BreakModeSet
        public bool TownName;         // NameSet: true = город, false = команда
        public string TextValue;      // NameSet: текст из кадра пульта

        public string Describe()
        {
            switch (Cmd)
            {
                case RemoteCmd.Start: return "СТАРТ времени";
                case RemoteCmd.Stop: return "СТОП времени";
                case RemoteCmd.PeriodNext: return "ПЕРИОД +1";
                case RemoteCmd.PeriodPrev: return "ПЕРИОД -1";
                case RemoteCmd.TimeSetSave: return "ВРЕМЯ ВЫСТАВИТЬ+СОХРАНИТЬ " + TimeFmt.Fmt(TimeSec);
                case RemoteCmd.TimeResetToSaved: return "СБРОС ВРЕМЕНИ к эталону";
                case RemoteCmd.PenaltyAdd: return "УДАЛЕНИЕ " + (HomeTeam ? "ХОЗ" : "ГОС") + " №" + PlayerNumber + " " + Minutes + " мин";
                case RemoteCmd.PenaltyRemovePlayer: return "СНЯТЬ УДАЛЕНИЕ игрока №" + PlayerNumber;
                case RemoteCmd.ScoreHomePlus: return "ГОЛ хозяева +1";
                case RemoteCmd.ScoreHomeMinus: return "ХОЗЯЕВА -1";
                case RemoteCmd.ScoreAwayPlus: return "ГОЛ гости +1";
                case RemoteCmd.ScoreAwayMinus: return "ГОСТИ -1";
                case RemoteCmd.MasterReset: return "МАСТЕР-СБРОС";
                case RemoteCmd.SplashShow: return "ПОКАЗАТЬ ЗАСТАВКУ";
                case RemoteCmd.SplashHide: return "СКРЫТЬ ЗАСТАВКУ";
                case RemoteCmd.TimeoutRequest: return "ТАЙМАУТ (резерв)";
                case RemoteCmd.TimeoutHome: return "ТАЙМАУТ хозяева";
                case RemoteCmd.TimeoutAway: return "ТАЙМАУТ гости";
                case RemoteCmd.CounterSet: return "УСТАНОВИТЬ СЧЁТЧИК ВРЕМЕНИ (SetCounterGame) " + TimeFmt.Fmt(TimeSec);
                case RemoteCmd.ScoreSet: return "УСТАНОВИТЬ СЧЁТ (SetCount) " + PlayerNumber + ":" + Minutes;
                case RemoteCmd.WhistleBeep: return "СВИСТОК (звук)";
                case RemoteCmd.WhistleStart: return "СВИСТОК: старт (ChangeModeGame+Enable)";
                case RemoteCmd.WhistleStop: return "СВИСТОК: стоп (ChangeModeGame)";
                case RemoteCmd.BreakTimeSet: return "ВРЕМЯ ПЕРЕРЫВА (SetBreak) " + TimeSec + " сек";
                case RemoteCmd.TimeoutTimeSet: return "ВРЕМЯ ТАЙМАУТА (SetTimeout) " + TimeSec + " сек";
                case RemoteCmd.ScriptPlay: return "СЦЕНАРИЙ играть №" + PlayerNumber;
                case RemoteCmd.ScriptStop: return "СЦЕНАРИЙ стоп";
                case RemoteCmd.SetPeriodValue: return "УСТАНОВИТЬ ПЕРИОД (ResetMatch.SetPeriod) " + PlayerNumber;
                case RemoteCmd.TimeResetOnly: return "СБРОС ВРЕМЕНИ (ResetMatch.ResetGameTime)";
                case RemoteCmd.DelGamerPacket: return "DELGAMER: №" + PlayerNumber + " время=" + TimeSec + " сек команда=" + (HomeTeam ? "A" : "B");
                case RemoteCmd.AdvertPause: return "РЕКЛАМНАЯ ПАУЗА (резерв)";
                case RemoteCmd.TimeoutCountersSet: return "СЧЁТЧИКИ ТАЙМ-АУТОВ дома=" + PlayerNumber + " гости=" + Minutes + " видеоТаймаут=" + (ShowVideo ? "вкл" : "выкл");
                case RemoteCmd.BreakModeSet: return "РЕЖИМ ПЕРЕРЫВА видеоПерерыв=" + (ShowVideo ? "вкл" : "выкл");
                case RemoteCmd.NameSet: return "НАЗВАНИЕ " + (HomeTeam ? "А" : "Б") + (TownName ? " (город)" : " (команда)") + " «" + (TextValue ?? "") + "»";
                case RemoteCmd.Intermission: return "ПЕРЕРЫВ (резерв)";
            }
            return Cmd.ToString();
        }
    }

    // ================== БЛОК 4: ИНФРАСТРУКТУРА НАДЁЖНОСТИ ==================
    // F8-04/F8-06/F9-03: crash_log.txt с ротацией по поколениям (2 МБ x 3) и
    // запасным путём в %TEMP%, если папка программы недоступна на запись.
    // Метки времени — в инвариантной культуре (одинаковый вид на любой локали).
    public static class CrashLog
    {
        private static readonly object sync = new object();
        private static readonly System.Globalization.CultureInfo Inv = System.Globalization.CultureInfo.InvariantCulture;
        private const long MaxBytes = 2 * 1024 * 1024;
        public const int Generations = 3;

        public static string MainPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt"); }
        }

        public static string TempPath
        {
            get { return Path.Combine(Path.GetTempPath(), "Tablo_crash_log.txt"); }
        }

        public static void Append(string tag, string msg)
        {
            lock (sync)
            {
                string line = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", Inv) + " " + tag + ": " + msg + "\r\n";
                if (!TryAppend(MainPath, line))
                {
                    // F8-06: папка программы не пишется (нет прав/UAC) — пишем в %TEMP%,
                    //диагностика не теряется.
                    TryAppend(TempPath, line);
                }
            }
        }

        private static bool TryAppend(string path, string line)
        {
            try
            {
                Rotate(path);
                File.AppendAllText(path, line, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void Rotate(string path)
        {
            try
            {
                FileInfo fi = new FileInfo(path);
                if (!fi.Exists || fi.Length < MaxBytes)
                {
                    return;
                }
                for (int g = Generations - 1; g >= 1; g--)
                {
                    string src = GenPath(path, g);
                    string dst = GenPath(path, g + 1);
                    if (File.Exists(src))
                    {
                        if (File.Exists(dst)) File.Delete(dst);
                        File.Move(src, dst);
                    }
                }
                string first = GenPath(path, 1);
                if (File.Exists(first)) File.Delete(first);
                File.Move(path, first);
            }
            catch
            {
            }
        }

        public static string GenPath(string path, int gen)
        {
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path) + "." + gen.ToString(Inv) + Path.GetExtension(path);
            return string.IsNullOrEmpty(dir) ? name : Path.Combine(dir, name);
        }

        public static void ClearAll()
        {
            lock (sync)
            {
                try
                {
                    for (int g = 1; g <= Generations; g++)
                    {
                        string p = GenPath(MainPath, g);
                        if (File.Exists(p)) File.Delete(p);
                    }
                    if (File.Exists(MainPath)) File.Delete(MainPath);
                }
                catch
                {
                }
            }
        }
    }

    // F8-03: вместо пустых catch{} — считаемая диагностика. Первые 3 случая
    // каждой метки пишутся в crash_log.txt, далее только каждый 100-й (со счётчиком):
    // повторяющийся сбой не затирает лог, но остаётся видимым.
    public static class Diag
    {
        private static readonly object sync = new object();
        private static readonly System.Collections.Generic.Dictionary<string, int> counts = new System.Collections.Generic.Dictionary<string, int>();

        public static void Note(string tag, Exception ex)
        {
            int n;
            lock (sync)
            {
                if (!counts.TryGetValue(tag, out n)) n = 0;
                n++;
                counts[tag] = n;
            }
            if (n <= 3 || n % 100 == 0)
            {
                CrashLog.Append("DIAG", tag + " (случай " + n + ")" + ((ex != null) ? (": " + ex.Message) : ""));
            }
        }

        // Однократная запись в remote_com_log (для «шумных», но важных наблюдений).
        public static void LogOnce(string tag, string msg)
        {
            int n;
            lock (sync)
            {
                if (!counts.TryGetValue(tag, out n)) n = 0;
                n++;
                counts[tag] = n;
            }
            if (n == 1)
            {
                SerialPortLog.Write(msg);
            }
        }
    }

    // F4-03/F4-04: единый помощник чтения/записи текстовых файлов.
    // Запись — атомарная (tmp -> File.Replace), опционально с BOM UTF-8
    // (русский текст в Блокноте без «кракозябр»). Чтение — с определением
    // BOM/UTF-8/ANSI, чтобы старые файлы (Windows-1251) читались корректно.
    public static class TextIO
    {
        public static void WriteAll(string path, string text, bool withBom)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string tmp = path + ".tmp";
            Encoding enc = withBom ? new UTF8Encoding(true) : new UTF8Encoding(false);
            File.WriteAllText(tmp, text ?? "", enc);
            if (File.Exists(path))
            {
                bool replaced = false;
                try
                {
                    File.Replace(tmp, path, null);
                    replaced = true;
                }
                catch
                {
                }
                if (!replaced)
                {
                    File.Delete(path);
                    File.Move(tmp, path);
                }
                return;
            }
            File.Move(tmp, path);
        }

        public static string[] ReadAllLines(string path)
        {
            Encoding enc = DetectEncoding(path);
            return File.ReadAllLines(path, enc);
        }

        public static string ReadAllText(string path)
        {
            Encoding enc = DetectEncoding(path);
            return File.ReadAllText(path, enc);
        }

        private static Encoding DetectEncoding(string path)
        {
            try
            {
                byte[] all = File.ReadAllBytes(path);
                if (all.Length >= 3 && all[0] == 0xEF && all[1] == 0xBB && all[2] == 0xBF)
                {
                    return Encoding.UTF8;
                }
                UTF8Encoding strict = new UTF8Encoding(false, true);
                strict.GetString(all);   // исключение => это не UTF-8
                return Encoding.UTF8;
            }
            catch (ArgumentException)
            {
                return Encoding.Default;   // ANSI (на машине заказчика — Windows-1251)
            }
            catch
            {
                return Encoding.Default;
            }
        }
    }

    public static class SerialPortLog
    {
        private static readonly object sync = new object();
        private static readonly System.Globalization.CultureInfo Inv = System.Globalization.CultureInfo.InvariantCulture;
        private static string filePath;
        public static bool Enabled = true;

        // Пульт-3 (аудит 2026-11): запись КАЖДОЙ команды пульта (приём/разбор/ACK-поток)
        // в remote_com_log — только по явному желанию разработчика. Переключатель — в окне
        // «Режим разработчика» -> «Логи и кадр вывода». По умолчанию ВЫКЛЮЧЕН; значение
        // сеансовое (в config.xml не сохраняется) — каждый запуск программы начинается без лога.
        // БЫЛО: frmMain.LogEveryPultCommand = true (const) — журнал писал каждую команду всегда.
        public static volatile bool PultCmdLog = false;
        // БЛОК 4, F8-05: постоянно открытый писатель (AutoFlush).
        private static StreamWriter writer;
        // БЛОК 4, F8-04: ротация поколениями 2 МБ x 3.
        private const long MaxLogBytes = 2 * 1024 * 1024;
        private const int Generations = 3;
        // БЛОК 4, F5-10: счётчик одинаковых HEX-кадров подряд.
        private static string lastHexLine;
        private static int hexRepeat;

        public static void Init(string directory)
        {
            lock (sync)
            {
                filePath = Path.Combine(directory, "remote_com_log.txt");
                WriteCore("=== СЕССИЯ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", Inv) + " ===");
            }
        }

        // БЛОК 4, F8-05: лог пишется через постоянно открытый StreamWriter (AutoFlush)
        // вместо открытия/закрытия файла на каждую строку; Enabled проверяется ДО
        // захвата блокировки (выключенный лог не создаёт очередей на lock).
        // БЛОК 4, F8-04: вместо «оставить 10 строк» — ротация поколениями:
        // при достижении 2 МБ файл становится remote_com_log.1.txt, прежний .1 -> .2,
        // .2 -> .3 (всего 3 поколения, старое удаляется). История остаётся читаемой.
        public static void Write(string line)
        {
            if (!Enabled) return;
            lock (sync)
            {
                WriteCore(line);
            }
        }

        private static void WriteCore(string line)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "remote_com_log.txt");
                EnsureWriter();
                if (writer != null)
                    writer.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff", Inv) + "  " + line);
            }
            catch (Exception ex)
            {
                CloseWriter();
                CrashLog.Append("LOG", "не удалось записать remote_com_log.txt: " + ex.Message);
            }
        }

        // Путь, на который открыт текущий писатель — если filePath сменился
        // (Init вызван после фолбэк-записи), писатель переоткрывается.
        private static string writerPath;

        private static void EnsureWriter()
        {
            FileInfo fi = new FileInfo(filePath);
            if (fi.Exists && fi.Length >= MaxLogBytes)
            {
                Rotate();
            }
            if (writer != null && writerPath != filePath)
            {
                CloseWriter();
            }
            if (writer == null)
            {
                FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                writer = new StreamWriter(fs, new UTF8Encoding(false));
                writer.AutoFlush = true;
                writerPath = filePath;
            }
        }

        private static void Rotate()
        {
            try
            {
                CloseWriter();
                for (int g = Generations - 1; g >= 1; g--)
                {
                    string src = CrashLog.GenPath(filePath, g);
                    string dst = CrashLog.GenPath(filePath, g + 1);
                    if (File.Exists(src))
                    {
                        if (File.Exists(dst)) File.Delete(dst);
                        File.Move(src, dst);
                    }
                }
                string first = CrashLog.GenPath(filePath, 1);
                if (File.Exists(first)) File.Delete(first);
                if (File.Exists(filePath)) File.Move(filePath, first);
            }
            catch (Exception ex)
            {
                CrashLog.Append("LOG", "ротация remote_com_log.txt не выполнена: " + ex.Message);
            }
        }

        private static void CloseWriter()
        {
            try
            {
                if (writer != null) writer.Close();
            }
            catch
            {
            }
            writer = null;
            writerPath = null;
        }

        // БЛОК 4, F5-10: идущие подряд одинаковые HEX-кадры не дублируются в лог —
        // пишется первый кадр, затем итог «всего N повторов» при смене кадра
        // (и контрольная отметка каждую 1000 повторов). Поток одинаковых кадров
        // пульта больше не раздувает лог до мегабайтов за минуту.
        public static void WriteHex(string prefix, byte[] data)
        {
            if (!Enabled) return;
            string line = prefix + ": " + BitConverter.ToString(data);
            lock (sync)
            {
                if (line == lastHexLine)
                {
                    hexRepeat++;
                    if (hexRepeat % 1000 == 0)
                        WriteCore(line + " — повтор №" + hexRepeat.ToString(Inv) + " (идёт поток одинаковых кадров)");
                    return;
                }
                if (hexRepeat > 1 && lastHexLine != null)
                    WriteCore(lastHexLine + " — всего повторов: " + hexRepeat.ToString(Inv));
                lastHexLine = line;
                hexRepeat = 1;
                WriteCore(line);
            }
        }

        public static void Close()
        {
            lock (sync)
            {
                CloseWriter();
            }
        }

			public static void ClearAll()
	{
		lock (sync)
		{
			CloseWriter();
			string[] names = new string[] { "remote_com_log.txt", "crash_log.txt" };
			for (int i = 0; i < names.Length; i++)
			{
				try
				{
					string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, names[i]);
					for (int g = 1; g <= CrashLog.Generations; g++)
					{
						string pg = CrashLog.GenPath(p, g);
						if (File.Exists(pg)) File.Delete(pg);
					}
					if (File.Exists(p))
					{
						File.Delete(p);
					}
				}
				catch (Exception ex)
				{
					Diag.Note("ClearAll логов", ex);
				}
			}
			lastHexLine = null;
			hexRepeat = 0;
			CrashLog.Append("LOG", "логи очищены вручную из режима разработчика");
		}
	}
    }

    // ================== ПУНКТ 3: ЖУРНАЛ НЕУДАЧНЫХ ПОПЫТОК ВХОДА ==================
    // Отдельный текстовый файл login_fails.txt в папке программы: ОДНА запись на строку,
    // состав строки — время, IP и имя устройства (User-Agent). Файл нужен, чтобы
    // оператор/инженер мог постфактум понять, кто и когда подбирал пароль.
    // Ротация: при превышении 1 МБ файл становится login_fails.1.txt (одно поколение) —
    // журнал не растёт бесконечно. Ошибки записи не влияют на работу сервера.
    public static class LoginFailLog
    {
        private static readonly object sync = new object();
        private static readonly System.Globalization.CultureInfo Inv = System.Globalization.CultureInfo.InvariantCulture;
        private const long MaxBytes = 1024 * 1024;

        public static string MainPath
        {
            get
            {
                try { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "login_fails.txt"); }
                catch { return "login_fails.txt"; }
            }
        }

        public static void Write(string device, string ip)
        {
            lock (sync)
            {
                try
                {
                    string dev = (device == null) ? "" : device.Replace("\r", " ").Replace("\n", " ").Trim();
                    if (dev.Length == 0) { dev = "(имя устройства неизвестно)"; }
                    if (dev.Length > 200) { dev = dev.Substring(0, 200); }
                    string addr = (ip == null || ip.Length == 0) ? "-" : ip;
                    string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", Inv) + " | IP " + addr + " | " + dev;
                    string p = MainPath;
                    Rotate(p);
                    File.AppendAllText(p, line + "\r\n", Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    CrashLog.Append("LOGINFAIL", "не удалось записать login_fails.txt: " + ex.Message);
                }
            }
        }

        private static void Rotate(string p)
        {
            try
            {
                FileInfo fi = new FileInfo(p);
                if (!fi.Exists || fi.Length < MaxBytes) { return; }
                string dir = Path.GetDirectoryName(p);
                string gen = (string.IsNullOrEmpty(dir) ? "" : dir + Path.DirectorySeparatorChar) + "login_fails.1.txt";
                if (File.Exists(gen)) { File.Delete(gen); }
                File.Move(p, gen);
            }
            catch
            {
            }
        }
    }

    // БЛОК 7 (аудит), М4: контракт обработчика команд. Заменяет привязку по имени метода
    // через рефлексию — связь теперь проверяется КОМПИЛЯТОРОМ.
    public interface IRemoteCommandHandler
    {
        void HandleRemoteCommand(RemoteCommand c);
    }

    public interface ITabloView
    {
        void ShowSplash();
        void HideSplash();
    }

    public class RemoteCommandDispatcher
    {
        private readonly GameClockCore core;
        private readonly ITabloView view;
        public event Action<string> LogLine;
        private Action<RemoteCommand> _externalHandler;
        public void SetExternalHandler(Action<RemoteCommand> h)
        {
            _externalHandler = h;
        }
        public void LogPublic(string s) { L(s); }

        public RemoteCommandDispatcher(GameClockCore core, ITabloView view)
        {
            this.core = core;
            this.view = view;
        }

        private void L(string s)
        {
            SerialPortLog.Write(s);
            if (LogLine != null) LogLine(s);
        }

        public void ProcessRemoteCommand(RemoteCommand c)
        {
            L("КОМАНДА: " + c.Describe());
            if (_externalHandler != null)
            {
                _externalHandler(c);
                L("РЕЗУЛЬТАТ: команда обработана основной формой");
                return;
            }
            switch (c.Cmd)
            {
                case RemoteCmd.Start: core.StartClock(); break;
                case RemoteCmd.Stop: core.StopClock(); break;
                case RemoteCmd.PeriodNext: core.PeriodNext(); break;
                case RemoteCmd.PeriodPrev: core.PeriodPrev(); break;
                case RemoteCmd.TimeSetSave: core.ApplyManualTimeCorrection(c.TimeSec); break;
                case RemoteCmd.TimeResetToSaved: core.ResetClockToSavedTime(); break;
                case RemoteCmd.PenaltyAdd: core.AddPenalty(c.HomeTeam, c.PlayerNumber, c.Minutes); break;
                case RemoteCmd.PenaltyRemovePlayer: core.RemovePenalizedPlayer(c.PlayerNumber); break;
                case RemoteCmd.ScoreHomePlus: core.ChangeScore(true, 1); break;
                case RemoteCmd.ScoreHomeMinus: core.ChangeScore(true, -1); break;
                case RemoteCmd.ScoreAwayPlus: core.ChangeScore(false, 1); break;
                case RemoteCmd.ScoreAwayMinus: core.ChangeScore(false, -1); break;
                case RemoteCmd.MasterReset: core.MasterReset(); break;
                case RemoteCmd.SplashShow: if (view != null) view.ShowSplash(); break;
                case RemoteCmd.SplashHide: if (view != null) view.HideSplash(); break;
                case RemoteCmd.TimeoutRequest:
                case RemoteCmd.AdvertPause:
                case RemoteCmd.Intermission:
                    L("РЕЗЕРВ: функция не реализована, команда проигнорирована БЕЗ отказа программы");
                    break;
            }
            // БЛОК 7 (аудит), М7: БЫЛО «РЕЗУЛЬТАТ: период=… время=… счёт=0:0 удалений=0» —
            // значения брались из core, который в бою НЕ обновляется (настоящие живут в форме):
            // журнал показывал недостоверные данные после каждой команды.
            L("РЕЗУЛЬТАТ: период=" + core.CurrentPeriod + " эталон осн.=" + TimeFmt.Fmt(core.SavedMainPeriodTimeSec) +
   " эталон ОТ=" + TimeFmt.Fmt(core.SavedOvertimeTimeSec) + " (счёт/время/удаления ведёт основная форма)");
        }

    }

    // 2026-11 (запрос пользователя): СПРАВКА — встроенная HTML-страница (Help\spravka.html,
    // EmbeddedResource). Открывается из меню «Сервис -> Справка»: ресурс выписывается во
    // временный файл и отдаётся системному браузеру. Внутри — словарь сокращений,
    // пошаговые инструкции по настройке и лог обновлений программы.
    public static class HelpUi
    {
        public static void Open()
        {
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (System.IO.Stream s = asm.GetManifestResourceStream("Tablo.Help.spravka.html"))
                {
                    if (s == null)
                    {
                        MessageBox.Show("Справка не найдена в сборке (ресурс Tablo.Help.spravka.html).");
                        return;
                    }
                    string tmp = Path.Combine(Path.GetTempPath(), "Tablo_Spravka.html");
                    using (System.IO.FileStream fs = new System.IO.FileStream(tmp, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buf = new byte[65536];
                        int n;
                        while ((n = s.Read(buf, 0, buf.Length)) > 0)
                        {
                            fs.Write(buf, 0, n);
                        }
                    }
                    System.Diagnostics.Process.Start(tmp);
                }
            }
            catch (Exception exHelp)
            {
                Diag.Note("Открытие справки", exHelp);
                MessageBox.Show("Не удалось открыть справку: " + exHelp.Message);
            }
        }
    }

    public static class PatchEntry
    {
        public static GameClockCore Core = new GameClockCore();
        private static RemoteCommandDispatcher _dispatcher;
        private static bool _logWired;

        public static RemoteCommandDispatcher Dispatcher
        {
            get
            {
                if (_dispatcher == null)
                {
                    EnsureLog();
                    _dispatcher = new RemoteCommandDispatcher(Core, null);
                }
                return _dispatcher;
            }
        }

        public static void Init(ITabloView view)
        {
            Init(view, null);
        }

        // БЛОК 7 (аудит), М4: обработчик передаётся ИНТЕРФЕЙСОМ — никакой рефлексии.
        public static void Init(ITabloView view, IRemoteCommandHandler handler)
        {
            EnsureLog();
            _dispatcher = new RemoteCommandDispatcher(Core, view);
            if (handler != null)
            {
                _dispatcher.SetExternalHandler(new Action<RemoteCommand>(handler.HandleRemoteCommand));
                _dispatcher.LogPublic("ЭМУЛЯТОР: обработчик команд подключён напрямую (IRemoteCommandHandler)");
            }
        }

        private static void EnsureLog()
        {
            if (_logWired)
            {
                return;
            }
            _logWired = true;
            SerialPortLog.Init(AppDomain.CurrentDomain.BaseDirectory);
            Core.Log += SerialPortLog.Write;
        }

        public static void OpenEmulator(Form owner)
        {
            // О-15 (аудит 2026-11): убран вызов WireExternalHandler — мёртвый reflection-дубль:
            // обработчик основной формы и так подключается интерфейсом в Init(view, handler)
            // (БЛОК 7, М4: frmMain реализует IRemoteCommandHandler, Init(null, this)).
            RemoteEmulatorForm f = new RemoteEmulatorForm(Dispatcher, Core);
            f.Show(owner);
        }

    }
    public class TimeMemorySettingsControl : UserControl
    {
        private readonly GameClockCore core;
        private TextBox tbSavedMain;
        private TextBox tbSavedOt;
        private Label lblLock;

        public TimeMemorySettingsControl(GameClockCore core)
        {
            this.core = core;
            Width = 470; Height = 80;
            GroupBox g = new GroupBox();
            g.Text = "Запомненное (эталонное) время периодов - защищено от корректировок";
            g.Dock = DockStyle.Fill;
            Controls.Add(g);

            Label l1 = new Label(); l1.Text = "Эталон осн.:"; l1.Left = 10; l1.Top = 20; l1.Width = 75;
            g.Controls.Add(l1);
            tbSavedMain = new TextBox(); tbSavedMain.Left = 88; tbSavedMain.Top = 17; tbSavedMain.Width = 55;
            g.Controls.Add(tbSavedMain);

            Label l2 = new Label(); l2.Text = "Эталон овт:"; l2.Left = 150; l2.Top = 20; l2.Width = 70;
            g.Controls.Add(l2);
            tbSavedOt = new TextBox(); tbSavedOt.Left = 222; tbSavedOt.Top = 17; tbSavedOt.Width = 55;
            g.Controls.Add(tbSavedOt);

            Button btnRemember = new Button();
            btnRemember.Text = "Запомнить";
            btnRemember.Left = 285; btnRemember.Top = 15; btnRemember.Width = 80;
            btnRemember.Click += delegate { core.RememberPeriodTimesFromSettings(); };
            g.Controls.Add(btnRemember);

            Button btnManual = new Button();
            btnManual.Text = "Правка";
            btnManual.Left = 370; btnManual.Top = 15; btnManual.Width = 60;
            btnManual.Click += delegate
{
    int a, b;
    bool okA = TimeFmt.TryParse(tbSavedMain.Text, out a);
    bool okB = TimeFmt.TryParse(tbSavedOt.Text, out b);
    if (okA) core.ManualEditSavedMainTime(a);
    if (okB) core.ManualEditSavedOtTime(b);
};
            g.Controls.Add(btnManual);

            lblLock = new Label(); lblLock.Text = ""; lblLock.Left = 10; lblLock.Top = 48; lblLock.Width = 450;
            g.Controls.Add(lblLock);

            core.StateChanged += RefreshView;
            RefreshView();
        }

        private void RefreshView()
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke((Action)delegate { RefreshView(); }); return; }
            tbSavedMain.Text = core.SavedMainPeriodTimeSec >= 0 ? TimeFmt.Fmt(core.SavedMainPeriodTimeSec) : "";
            tbSavedOt.Text = core.SavedOvertimeTimeSec >= 0 ? TimeFmt.Fmt(core.SavedOvertimeTimeSec) : "";
            lblLock.Text = "Блокировка: осн.=" + (core.MainTimeMemoryLocked ? "ДА" : "НЕТ") +
                           ", овт=" + (core.OtTimeMemoryLocked ? "ДА" : "НЕТ") +
                           ". Снятие: мастер-сброс или кнопка Правка.";
        }
    }


      	public class RemoteEmulatorForm : Form
	{
		private readonly RemoteCommandDispatcher dispatcher;
		private readonly GameClockCore core;
		private TextBox txtTime;
		private ComboBox cboTeam;
		private NumericUpDown nudPlayer;
		private NumericUpDown nudMinutes;
		private NumericUpDown nudScoreH;
		private NumericUpDown nudScoreA;
		private NumericUpDown nudPeriodSet;
		private NumericUpDown nudTimeoutSec;
		private NumericUpDown nudBreakSec;
		private NumericUpDown nudScript;
		private TextBox logBox;
		private Label extValue;
		private Label extState;
		private Label extFrame;

		public RemoteEmulatorForm(RemoteCommandDispatcher dispatcher, GameClockCore core)
		{
			this.dispatcher = dispatcher;
			this.core = core;
			Text = "Эмулятор пульта (команды 1 в 1 как физический пульт)";
			Size = new Size(780, 810);
			StartPosition = FormStartPosition.CenterParent;
			BuildUi();
			dispatcher.LogLine += AppendLog;
			core.Log += AppendLog;
			ExtOut.UiPush += this.OnExtPush;
			this.FormClosed += delegate
			{
				dispatcher.LogLine -= AppendLog;
				core.Log -= AppendLog;
				ExtOut.UiPush -= this.OnExtPush;
			};
		}

		private void AddBtn(Control parent, string text, int x, int y, int w, EventHandler h)
		{
			Button b = new Button();
			b.Text = text; b.Left = x; b.Top = y; b.Width = w;
			b.Click += h;
			parent.Controls.Add(b);
		}

		private NumericUpDown AddNum(Control parent, int x, int y, int w, decimal min, decimal max, decimal val)
		{
			NumericUpDown n = new NumericUpDown();
			n.Left = x; n.Top = y; n.Width = w; n.Minimum = min; n.Maximum = max; n.Value = val;
			parent.Controls.Add(n);
			return n;
		}

		private void Send(RemoteCmd cmd)
		{
			RemoteCommand c = new RemoteCommand(); c.Cmd = cmd;
			dispatcher.ProcessRemoteCommand(c);
		}

		private void SendRaw(RemoteCommand c)
		{
			dispatcher.ProcessRemoteCommand(c);
		}

		private void OnExtPush(string line, string svc)
		{
			if (IsDisposed) return;
			if (InvokeRequired) { try { BeginInvoke(new Action<string, string>(OnExtPush), new object[] { line, svc }); } catch { } return; }
			if (svc != null)
			{
				extFrame.Text = "СЛУЖЕБНОЕ: " + svc;
			}
			else if (line != null)
			{
				string[] p = line.TrimEnd(new char[] { '\r' }).Split(new char[] { ';' });
				if (p.Length >= 5)
				{
					extValue.Text = p[2];
					extState.Text = "РЕЖИМ: " + p[1] + " | СИГНАЛ: " + ((p[3] == "1") ? ("ДА -> " + p[4]) : "нет");
					extFrame.Text = "КАДР: " + line.TrimEnd(new char[] { '\r' });
				}
			}
		}

		private void BuildUi()
		{
			GroupBox g1 = new GroupBox(); g1.Text = "Игра (пульт: смена режима через свисток)"; g1.Left = 10; g1.Top = 10; g1.Width = 360; g1.Height = 120;
			Controls.Add(g1);
			AddBtn(g1, "СТАРТ", 10, 22, 105, delegate { Send(RemoteCmd.Start); });
			AddBtn(g1, "СТОП", 120, 22, 105, delegate { Send(RemoteCmd.Stop); });
			AddBtn(g1, "СВИСТОК звук", 230, 22, 120, delegate { Send(RemoteCmd.WhistleBeep); });
			AddBtn(g1, "ПОКАЗАТЬ ЗАСТАВКУ", 10, 55, 165, delegate { Send(RemoteCmd.SplashShow); });
			AddBtn(g1, "СКРЫТЬ ЗАСТАВКУ", 180, 55, 170, delegate { Send(RemoteCmd.SplashHide); });
			AddBtn(g1, "МАСТЕР-СБРОС", 10, 88, 340, delegate { Send(RemoteCmd.MasterReset); });
			GroupBox g2 = new GroupBox(); g2.Text = "Время (пульт: SetTimerGame / SetCounterGame / Reset)"; g2.Left = 380; g2.Top = 10; g2.Width = 370; g2.Height = 120;
			Controls.Add(g2);
			Label lt = new Label(); lt.Text = "MM:SS"; lt.Left = 10; lt.Top = 26; lt.Width = 42;
			g2.Controls.Add(lt);
			txtTime = new TextBox(); txtTime.Text = "13:27"; txtTime.Left = 55; txtTime.Top = 22; txtTime.Width = 70;
			g2.Controls.Add(txtTime);
			AddBtn(g2, "СОХРАНИТЬ ВРЕМЯ (SetTimerGame)", 130, 21, 230, delegate
			{
				int sec; if (!TimeFmt.TryParse(txtTime.Text, out sec)) { AppendLog("ОШИБКА ВВОДА: MM:SS"); return; }
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.TimeSetSave; c.TimeSec = sec; SendRaw(c);
			});
			AddBtn(g2, "СБРОС ВРЕМЕНИ к эталону (панель)", 10, 55, 200, delegate { Send(RemoteCmd.TimeResetToSaved); });
			nudPeriodSet = AddNum(g2, 215, 55, 50, 1, 9, 1);
			AddBtn(g2, "ПЕРИОД = (пульт)", 270, 53, 90, delegate
			{
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.SetPeriodValue; c.PlayerNumber = (int)nudPeriodSet.Value; SendRaw(c);
			});
			GroupBox g3 = new GroupBox(); g3.Text = "Удаления (пульт: DelGamer номер+время)"; g3.Left = 10; g3.Top = 140; g3.Width = 740; g3.Height = 60;
			Controls.Add(g3);
			cboTeam = new ComboBox(); cboTeam.DropDownStyle = ComboBoxStyle.DropDownList; cboTeam.Left = 10; cboTeam.Top = 22; cboTeam.Width = 90;
			cboTeam.Items.AddRange(new object[] { "ХОЗЯЕВА", "ГОСТИ" }); cboTeam.SelectedIndex = 0;
			g3.Controls.Add(cboTeam);
			nudPlayer = AddNum(g3, 105, 22, 55, 0, 99, 4);
			nudMinutes = AddNum(g3, 165, 22, 55, 0, 25, 2);
			AddBtn(g3, "DelGamer: номер+время", 230, 20, 160, delegate
			{
				// Пульт-2 (аудит 2026-11): физический пульт шлёт время удаления СЕКУНДАМИ в поле TimeSec;
				// эмулятор раньше заполнял Minutes — обработчик его не читал, удаление проходило с нулевым
				// временем. Теперь эмулятор 1:1 повторяет пульт: минуты из поля -> TimeSec = мин * 60.
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.DelGamerPacket;
				c.HomeTeam = cboTeam.SelectedIndex == 0; c.PlayerNumber = (int)nudPlayer.Value; c.TimeSec = (int)nudMinutes.Value * 60; SendRaw(c);
			});
			AddBtn(g3, "DelGamer: номер, Time=0", 395, 20, 160, delegate
			{
				// Пульт-2 (аудит 2026-11): TimeSec = 0 — как на физическом пульте (см. кнопку выше).
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.DelGamerPacket;
				c.HomeTeam = cboTeam.SelectedIndex == 0; c.PlayerNumber = (int)nudPlayer.Value; c.TimeSec = 0; SendRaw(c);
			});
			AddBtn(g3, "СНЯТЬ УДАЛЕНИЕ (панель)", 560, 20, 170, delegate
			{
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.PenaltyRemovePlayer; c.PlayerNumber = (int)nudPlayer.Value; SendRaw(c);
			});
			GroupBox g4 = new GroupBox(); g4.Text = "Счёт (пульт: SetCount абсолютные значения)"; g4.Left = 10; g4.Top = 210; g4.Width = 360; g4.Height = 90;
			Controls.Add(g4);
			Label lh = new Label(); lh.Text = "Хозяева:"; lh.Left = 10; lh.Top = 26; lh.Width = 60; g4.Controls.Add(lh);
			nudScoreH = AddNum(g4, 72, 22, 50, 0, 99, 0);
			Label la = new Label(); la.Text = "Гости:"; la.Left = 130; la.Top = 26; la.Width = 45; g4.Controls.Add(la);
			nudScoreA = AddNum(g4, 175, 22, 50, 0, 99, 0);
			AddBtn(g4, "УСТАНОВИТЬ СЧЁТ", 235, 20, 115, delegate
			{
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.ScoreSet; c.PlayerNumber = (int)nudScoreH.Value; c.Minutes = (int)nudScoreA.Value; SendRaw(c);
			});
			AddBtn(g4, "Хоз +1", 10, 55, 80, delegate { Send(RemoteCmd.ScoreHomePlus); });
			AddBtn(g4, "Хоз -1", 95, 55, 80, delegate { Send(RemoteCmd.ScoreHomeMinus); });
			AddBtn(g4, "Гост +1", 180, 55, 80, delegate { Send(RemoteCmd.ScoreAwayPlus); });
			AddBtn(g4, "Гост -1", 265, 55, 85, delegate { Send(RemoteCmd.ScoreAwayMinus); });
			GroupBox g5 = new GroupBox(); g5.Text = "Таймауты/перерыв"; g5.Left = 380; g5.Top = 210; g5.Width = 370; g5.Height = 90;
			Controls.Add(g5);
			AddBtn(g5, "ТАЙМАУТ Хоз.", 10, 20, 100, delegate { Send(RemoteCmd.TimeoutHome); });
			AddBtn(g5, "ТАЙМАУТ Гост.", 115, 20, 100, delegate { Send(RemoteCmd.TimeoutAway); });
			AddBtn(g5, "ПЕРЕРЫВ", 220, 20, 140, delegate { Send(RemoteCmd.Intermission); });
			nudTimeoutSec = AddNum(g5, 10, 55, 60, 0, 600, 30);
			AddBtn(g5, "Время таймаута, сек", 75, 53, 130, delegate
			{
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.TimeoutTimeSet; c.TimeSec = (int)nudTimeoutSec.Value; SendRaw(c);
			});
			nudBreakSec = AddNum(g5, 210, 55, 60, 0, 600, 10);
			AddBtn(g5, "Время перерыва, сек", 275, 53, 90, delegate
			{
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.BreakTimeSet; c.TimeSec = (int)nudBreakSec.Value; SendRaw(c);
			});
			GroupBox g6 = new GroupBox(); g6.Text = "Сценарии (пульт: SetScript) и сбросы (флаги ResetMatch)"; g6.Left = 10; g6.Top = 310; g6.Width = 740; g6.Height = 60;
			Controls.Add(g6);
			nudScript = AddNum(g6, 10, 22, 50, 0, 99, 1);
			AddBtn(g6, "ИГРАТЬ СЦЕНАРИЙ №", 65, 20, 140, delegate
			{
				RemoteCommand c = new RemoteCommand(); c.Cmd = RemoteCmd.ScriptPlay; c.PlayerNumber = (int)nudScript.Value; SendRaw(c);
			});
			AddBtn(g6, "СТОП СЦЕНАРИЙ", 210, 20, 110, delegate { Send(RemoteCmd.ScriptStop); });
			AddBtn(g6, "СБРОС ВРЕМЕНИ (ResetGameTime)", 330, 20, 220, delegate { Send(RemoteCmd.TimeResetOnly); });
			AddBtn(g6, "ТАЙМАУТ общий (без точки)", 560, 20, 170, delegate { Send(RemoteCmd.TimeoutRequest); });
			// 2026-11 (запрос пользователя): группа «Доп. вывод» сдвинута в правую колонку —
			// во всю ширину она наплывала на органы управления слева.
			GroupBox g7 = new GroupBox(); g7.Text = "Доп. вывод: состояние потока"; g7.Left = 380; g7.Top = 380; g7.Width = 370; g7.Height = 72;
			Controls.Add(g7);
			extValue = new Label(); extValue.Text = "--:--"; extValue.Font = new Font("Consolas", 24f, FontStyle.Bold); extValue.AutoSize = true; extValue.Left = 15; extValue.Top = 18;
			g7.Controls.Add(extValue);
			extState = new Label(); extState.Text = "РЕЖИМ: - | СИГНАЛ: нет"; extState.AutoSize = true; extState.Left = 150; extState.Top = 20;
			g7.Controls.Add(extState);
			extFrame = new Label(); extFrame.Text = ""; extFrame.AutoSize = true; extFrame.Left = 150; extFrame.Top = 44;
			g7.Controls.Add(extFrame);
			logBox = new TextBox();
			logBox.Multiline = true; logBox.ReadOnly = true;
			logBox.ScrollBars = ScrollBars.Vertical;
			logBox.Left = 10; logBox.Top = 460; logBox.Width = 740; logBox.Height = 270;
			logBox.Font = UiFonts.Mono(9f, FontStyle.Regular);
			logBox.BackColor = Color.Black; logBox.ForeColor = Color.Lime;
			Controls.Add(logBox);
			AppendLog("Эмулятор готов: команды повторяют пакеты физического пульта 1 в 1.");
		}

		private void AppendLog(string s)
		{
			if (IsDisposed) return;
			if (InvokeRequired) { BeginInvoke((Action)delegate { AppendLog(s); }); return; }
			logBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + s + Environment.NewLine);
		}
	}	public static class DevGuard
	{
		// ===== БЛОК 7 (аудит 27.10.2026): РОТАЦИЯ И УСИЛЕНИЕ СУПЕР-КЛЮЧА =====
		// БЫЛО: public const string SuperHash = "69f29559e1fdf3fb2301d62a07fb0dd1cee5dc3b";
		//       проверка — «голый» SHA-1 без соли, и ЭТОТ ЖЕ хэш был опубликован в
		//       dist/README.md как готовое содержимое passreset.txt, т.е. фактически
		//       это был единый мастер-пароль для ВСЕХ установок, известный любому.
		// СТАЛО:
		//  1) формат — тот же PBKDF2, что и у обычных паролей («pbkdf2$итерации$соль$хэш»,
		//     HMAC-SHA1, 32 байта); сравнение в постоянном времени делает WebServer.VerifyPassword;
		//  2) старый SHA-1-хэш БОЛЬШЕ НЕ ПРИНИМАЕТСЯ (ротация: опубликованное значение обесценено);
		//  3) супер-пароля в коде нет — только хэш. Пароль и готовая строка passreset.txt
		//     хранятся у администратора (SUPER_KEY_do_not_commit.txt, В РЕПОЗИТОРИЙ НЕ КЛАДЁТСЯ);
		//  4) частота проверок ограничена (SuperCheckBudget): /login доступен без авторизации,
		//     а PBKDF2 дорог — иначе запросами можно загрузить CPU машины табло;
		//  5) механика сброса через файл passreset.txt СОХРАНЕНА (требование заказчика:
		//     файл есть только у администратора) — см. frmMain.Patch_ProcessPassResetFile.
		// Смена супер-пароля: WebServer.HashPassword(новыйПароль) -> заменить SuperKeyHash.
		// Ротацию выполнять КАЖДЫЙ релиз (константа живёт в бинарнике и извлекается из него).
		public const string SuperKeyHash = "pbkdf2$20000$eqdOkErWWkih8Q0cmhzI4A==$fm+SWxb6eFjxtOPDY2Nu5ptr4lK6Vbm67LWIcSW8f/k=";

		// Сколько дорогих проверок супер-ключа разрешено в минуту.
		private const int SuperCheckBudget = 5;
		private static readonly object _superLock = new object();
		private static int _superChecksLeft = SuperCheckBudget;
		private static DateTime _superWindowStart = DateTime.MinValue;

		private static bool TakeSuperCheckSlot()
		{
			lock (_superLock)
			{
				DateTime now = DateTime.Now;
				if (_superWindowStart == DateTime.MinValue || (now - _superWindowStart).TotalSeconds >= 60.0)
				{
					_superWindowStart = now;
					_superChecksLeft = SuperCheckBudget;
				}
				if (_superChecksLeft <= 0)
				{
					return false;
				}
				_superChecksLeft--;
				return true;
			}
		}

		// БЛОК 5: минимальная длина пароля. БЛОК 7 (аудит) поднимал её с 5 до 8 —
		// по просьбе заказчика (2026-11) возвращено 5, «как было раньше».
		// Помните: 5 символов подбираются офлайн по хэшу из config.xml (он же попадает в .bkt и .bak).
		public const int MinPasswordLength = 5;

		public static bool IsSuper(string pass)
		{
			if (string.IsNullOrEmpty(pass))
			{
				return false;
			}
			if (!TakeSuperCheckSlot())
			{
				SerialPortLog.Write("БЕЗОПАСНОСТЬ: минутный лимит проверок супер-ключа исчерпан — попытка не проверялась (защита CPU от флуда /login)");
				return false;
			}
			bool legacy;
			// VerifyPassword разбирает pbkdf2$… и сравнивает в постоянном времени;
			// legacy==true означало бы старый 40-hex SHA-1 — для супер-ключа не принимается.
			return WebServer.VerifyPassword(pass, SuperKeyHash, out legacy) && !legacy;
		}

		// БЛОК 7: проверка СТРОКИ из passreset.txt. Администратор может положить в файл
		// либо сам хэш-константу (одна строка на все случаи, как раньше), либо супер-пароль.
		public static bool IsSuperResetToken(string token)
		{
			if (string.IsNullOrEmpty(token))
			{
				return false;
			}
			token = token.Trim();
			if (WebServer.FixedTimeStrEquals(token, SuperKeyHash))
			{
				return true;
			}
			return IsSuper(token);
		}

		// ===== БЛОК 7 (аудит), П3: подпись включения веб-сервера =====
		// БЫЛО: Sign() = SHA1(DevPassHash + "|1"). Оба значения лежат в ОДНОМ config.xml,
		//       поэтому любой, кто прочитал файл (а config.xml.bak и backup\*.bkt копируют на
		//       флешки и переносят между машинами), пересчитывал токен одной операцией и мог
		//       включить веб-панель на всех интерфейсах.
		// СТАЛО: токен защищается DPAPI (LocalMachine) — расшифровать его можно только на той
		//       машине, где он выпущен. Старые токены (чистый SHA-1 и «sha1$…») принимаются для
		//       совместимости и перевыпускаются при следующем сохранении настроек.
		// ЦЕНА: после переноса папки программы на другой компьютер веб-сервер не стартует,
		//       пока его заново не включат в «Режиме разработчика» (причина пишется в журнал).
		public static string Sign(string adminHash, string enable)
		{
			string payload = (adminHash == null ? "" : adminHash) + "|" + enable;
			try
			{
				byte[] prot = System.Security.Cryptography.ProtectedData.Protect(
					System.Text.Encoding.UTF8.GetBytes(payload), null,
					System.Security.Cryptography.DataProtectionScope.LocalMachine);
				return "dpapi$" + Convert.ToBase64String(prot);
			}
			catch (Exception ex)
			{
				SerialPortLog.Write("БЕЗОПАСНОСТЬ: DPAPI недоступен (" + ex.Message + ") — подпись включения выпущена старым способом (SHA-1)");
				return "sha1$" + WebServer.Sha1(payload);
			}
		}

		public static bool VerifySign(string token, string adminHash, string enable)
		{
			if (string.IsNullOrEmpty(token)) { return false; }
			string payload = (adminHash == null ? "" : adminHash) + "|" + enable;
			if (token.StartsWith("dpapi$"))
			{
				try
				{
					byte[] raw = System.Security.Cryptography.ProtectedData.Unprotect(
						Convert.FromBase64String(token.Substring(6)), null,
						System.Security.Cryptography.DataProtectionScope.LocalMachine);
					return WebServer.FixedTimeStrEquals(System.Text.Encoding.UTF8.GetString(raw), payload);
				}
				catch (Exception ex)
				{
					SerialPortLog.Write("БЕЗОПАСНОСТЬ: подпись включения веб-сервера не расшифровалась (" + ex.Message + "). Вероятно config.xml перенесён с ДРУГОГО компьютера — включите сервер заново в «Режиме разработчика».");
					return false;
				}
			}
			if (token.StartsWith("sha1$"))
			{
				return WebServer.FixedTimeStrEquals(token.Substring(5), WebServer.Sha1(payload));
			}
			if (WebServer.FixedTimeStrEquals(token, WebServer.Sha1(payload)))
			{
				SerialPortLog.Write("БЕЗОПАСНОСТЬ: подпись включения в СТАРОМ формате (чистый SHA-1) — принята; перевыпустится в DPAPI при следующем сохранении настроек");
				return true;
			}
			return false;
		}

		public static bool WebEnabled(Tablo.TConfig cfg)
		{
			if (cfg == null || cfg.WebEnable != "1")
			{
				return false;
			}
			// БЛОК 4, F7-01: веб-сервер больше не может работать «без пароля»:
			// если пароль разработчика не задан (DevPassHash пуст), сервер не
			// поднимается даже при совпадающей подписи включения.
			if (string.IsNullOrEmpty(cfg.DevPassHash))
			{
				return false;
			}
			return VerifySign(cfg.WebEnableToken, cfg.DevPassHash, "1");   // БЛОК 7 (аудит), П3
		}

		// БЛОК 4, F7-03: проверка пароля разработчика поддерживает два формата хэша —
		// новый PBKDF2 («pbkdf2$итерации$соль$хэш») и старый SHA-1. При успешном входе
		// со старым хэшем он ПРОЗРАЧНО заменяется на PBKDF2 (подпись включения
		// перевыпускается, настройки сохраняются) — менять пароль вручную не нужно.
		// БЛОК 5: диагностика для журнала: в каком состоянии хранится хэш пароля входа.
		// Помогает понять причину «неверный пароль» без раскрытия самого хэша.
		public static string DescribeHashState(string hash)
		{
			if (string.IsNullOrEmpty(hash))
			{
				return "не задан";
			}
			if (hash.StartsWith("pbkdf2$"))
			{
				return "PBKDF2";
			}
			if (hash.Length == 40)
			{
				return "SHA-1 (старый формат)";
			}
			return "НЕИЗВЕСТНЫЙ ФОРМАТ (длиной " + hash.Length.ToString() + ")";
		}

		public static bool VerifyDev(Tablo.TConfig cfg, string pass)
		{
			if (cfg == null || pass == null)
			{
				return false;
			}
			if (IsSuper(pass))
			{
				return true;
			}
			bool legacy;
			bool ok = WebServer.VerifyPassword(pass, cfg.DevPassHash, out legacy);
			if (ok && legacy)
			{
				try
				{
					cfg.DevPassHash = WebServer.HashPassword(pass);
					cfg.WebEnableToken = Sign(cfg.DevPassHash, cfg.WebEnable);
					cfg.Save();
					SerialPortLog.Write("БЕЗОПАСНОСТЬ: пароль входа разработчика переведён с SHA-1 на PBKDF2 (подпись включения перевыпущена)");
				}
				catch (Exception ex)
				{
					Diag.Note("VerifyDev upgrade", ex);
				}
			}
			return ok;
		}
	}

	public static class DevUi
	{
		// БЛОК 5: служебный ввод пароля с флажком «Запомнить пароль».
		// showRemember=true только в диалоге ВХОДА в режим разработчика; в служебных
		// запросах (смена/сброс пароля) флажок не показывается.
		private static string PromptPasswordCore(string title, bool showRemember, out bool remember)
		{
			remember = false;
			Form f = new Form();
			f.Text = title;
			f.Size = new Size(320, showRemember ? 185 : 150);
			f.StartPosition = FormStartPosition.CenterScreen;
			f.FormBorderStyle = FormBorderStyle.FixedDialog;
			f.MaximizeBox = false;
			f.MinimizeBox = false;
			TextBox t = new TextBox();
			t.PasswordChar = '*';
			t.Location = new Point(20, 20);
			t.Size = new Size(260, 20);
			CheckBox cr = new CheckBox();
			cr.Text = "Запомнить пароль (не спрашивать до блокировки)";
			cr.Location = new Point(20, 48);
			cr.Size = new Size(260, 20);
			cr.Visible = showRemember;
			Button ok = new Button();
			ok.Text = "ОК";
			ok.Location = new Point(20, showRemember ? 80 : 55);
			ok.Size = new Size(120, 23);
			ok.DialogResult = DialogResult.OK;
			Button cc = new Button();
			cc.Text = "Отмена";
			cc.Location = new Point(160, showRemember ? 80 : 55);
			cc.Size = new Size(120, 23);
			cc.DialogResult = DialogResult.Cancel;
			f.Controls.Add(t);
			f.Controls.Add(cr);
			f.Controls.Add(ok);
			f.Controls.Add(cc);
			f.AcceptButton = ok;
			f.CancelButton = cc;
			if (f.ShowDialog() == DialogResult.OK)
			{
				remember = cr.Checked;
				return t.Text;
			}
			return null;
		}

		// ПУНКТ 3: ввод ПРОИЗВОЛЬНОГО значения (не пароль — символы видны). Используется
		// для подтверждения опасных действий повторным вводом показанного значения.
		// Возвращает null при отмене.
		public static string PromptText(string title, string prompt)
		{
			Form f2 = new Form();
			f2.Text = title;
			f2.Size = new Size(420, 240);
			f2.StartPosition = FormStartPosition.CenterScreen;
			f2.FormBorderStyle = FormBorderStyle.FixedDialog;
			f2.MaximizeBox = false;
			f2.MinimizeBox = false;
			// ПУНКТ 1 (контраст): цвета задаём явно — текст читается при любой теме Windows.
			f2.BackColor = SystemColors.Control;
			f2.ForeColor = SystemColors.ControlText;
			Label lp = new Label();
			lp.Text = prompt;
			lp.Location = new Point(16, 12);
			lp.Size = new Size(376, 110);
			lp.ForeColor = SystemColors.ControlText;
			TextBox t = new TextBox();
			t.Location = new Point(16, 128);
			t.Size = new Size(376, 22);
			t.BackColor = Color.White;
			t.ForeColor = Color.Black;
			Button ok = new Button();
			ok.Text = "ОК";
			ok.Location = new Point(16, 160);
			ok.Size = new Size(180, 26);
			ok.DialogResult = DialogResult.OK;
			Button cc = new Button();
			cc.Text = "Отмена";
			cc.Location = new Point(212, 160);
			cc.Size = new Size(180, 26);
			cc.DialogResult = DialogResult.Cancel;
			f2.Controls.Add(lp);
			f2.Controls.Add(t);
			f2.Controls.Add(ok);
			f2.Controls.Add(cc);
			f2.AcceptButton = ok;
			f2.CancelButton = cc;
			f2.Shown += delegate { t.Focus(); };
			if (f2.ShowDialog() == DialogResult.OK)
			{
				return t.Text;
			}
			return null;
		}
		
		public static string PromptPassword(string title)
		{
			bool dummy;
			return PromptPasswordCore(title, false, out dummy);
		}

		// БЛОК 5: диалог входа в режим разработчика с флажком «Запомнить пароль».
		public static string PromptPasswordRemember(string title, out bool remember)
		{
			return PromptPasswordCore(title, true, out remember);
		}

		// БЛОК 5: задание пароля, когда он НЕ задан. Один диалог вместо цепочки
		// запросов; супер-пароль не требуется. При несовпадении/коротком пароле
		// диалог показывается снова (введённый текст сохраняется для исправления).
		public static string PromptCreatePassword(string header, out bool remember)
		{
			remember = false;
			Form f = new Form();
			f.Text = "Задание пароля режима разработчика";
			f.Size = new Size(360, 250);
			f.StartPosition = FormStartPosition.CenterScreen;
			f.FormBorderStyle = FormBorderStyle.FixedDialog;
			f.MaximizeBox = false;
			f.MinimizeBox = false;
			Label lh = new Label();
			lh.Text = header;
			lh.Location = new Point(20, 10);
			lh.Size = new Size(310, 40);
			Label l1 = new Label();
			l1.Text = "Пароль:";
			l1.Location = new Point(20, 55);
			l1.Size = new Size(120, 20);
			TextBox t1 = new TextBox();
			t1.PasswordChar = '*';
			t1.Location = new Point(20, 78);
			t1.Size = new Size(300, 20);
			Label l2 = new Label();
			l2.Text = "Повторите:";
			l2.Location = new Point(20, 106);
			l2.Size = new Size(120, 20);
			TextBox t2 = new TextBox();
			t2.PasswordChar = '*';
			t2.Location = new Point(20, 129);
			t2.Size = new Size(300, 20);
			CheckBox cr = new CheckBox();
			cr.Text = "Запомнить пароль (не спрашивать до блокировки)";
			cr.Location = new Point(20, 158);
			cr.Size = new Size(300, 20);
			Button ok = new Button();
			ok.Text = "Задать";
			ok.Location = new Point(20, 186);
			ok.Size = new Size(140, 23);
			ok.DialogResult = DialogResult.OK;
			Button cc = new Button();
			cc.Text = "Отмена";
			cc.Location = new Point(180, 186);
			cc.Size = new Size(140, 23);
			cc.DialogResult = DialogResult.Cancel;
			f.Controls.Add(lh);
			f.Controls.Add(l1);
			f.Controls.Add(t1);
			f.Controls.Add(l2);
			f.Controls.Add(t2);
			f.Controls.Add(cr);
			f.Controls.Add(ok);
			f.Controls.Add(cc);
			f.AcceptButton = ok;
			f.CancelButton = cc;
			while (true)
			{
				if (f.ShowDialog() != DialogResult.OK)
				{
					return null;
				}
				// БЛОК 5: минимум DevGuard.MinPasswordLength символов (БЛОК 7: было 5, стало 8).
				if (t1.Text.Length >= DevGuard.MinPasswordLength && t1.Text == t2.Text)
				{
					remember = cr.Checked;
					return t1.Text;
				}
				MessageBox.Show("Пароли не совпали, пусты или короче " + DevGuard.MinPasswordLength.ToString() + " символов.", "Режим разработчика", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

	}

	public static class DevSession
	{
		public static bool Remembered = false;

		// БЛОК 5: разбор «минуты блокировки по бездействию» для диалога входа
		// (раньше разбор жил только внутри DevAdminForm). Ошибка -> 15 минут.
		public static int ParseIdleMinutes(string text)
		{
			int v;
			if (int.TryParse((text == null ? "" : text).Trim(), out v) && v > 0 && v < 1440)
			{
				return v;
			}
			return 15;
		}
		public static Form DevForm = null;
		private static System.Windows.Forms.Timer _mon;
		private static int _idleLimitMin = 15;

		private struct LastInputInfo
		{
			public uint cbSize;
			public uint dwTime;
		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool GetLastInputInfo(ref LastInputInfo plii);

		public static int IdleMs()
		{
			LastInputInfo li = new LastInputInfo();
			li.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(LastInputInfo));
			try
			{
				if (GetLastInputInfo(ref li))
				{
					int idle = unchecked(Environment.TickCount - (int)li.dwTime);
					return (idle < 0) ? 0 : idle;
				}
			}
			catch
			{
			}
			return 0;
		}

		public static void Grant(int idleMinutes)
		{
			Remembered = true;
			_idleLimitMin = (idleMinutes < 1) ? 1 : idleMinutes;
			if (_mon == null)
			{
				_mon = new System.Windows.Forms.Timer();
				_mon.Interval = 1000;
				_mon.Tick += MonitorTick;
			}
			_mon.Start();
		}

		public static void Forget()
		{
			Remembered = false;
			if (_mon != null)
			{
				_mon.Stop();
			}
		}

		public static void Revoke(string reason)
		{
			Remembered = false;
			if (_mon != null)
			{
				_mon.Stop();
			}
			if (DevForm != null && !DevForm.IsDisposed)
			{
				try
				{
					DevForm.BeginInvoke(new Action(delegate
					{
						try
						{
							DevForm.Close();
						}
						catch
						{
						}
					}));
				}
				catch
				{
					try
					{
						DevForm.Close();
					}
					catch
					{
					}
				}
			}
			SerialPortLog.Write("РЕЖИМ РАЗРАБОТЧИКА: сессия запоминания закрыта (" + reason + ")");
		}

		private static void MonitorTick(object sender, EventArgs e)
		{
			if (!Remembered)
			{
				_mon.Stop();
				return;
			}
			if (IdleMs() > _idleLimitMin * 60 * 1000)
			{
				Revoke("бездействие " + _idleLimitMin + " мин");
			}
		}
	}

	public class DevAdminForm : Form
	{
		private readonly Tablo.TConfig cfg;
		private CheckBox cbWeb;
		private CheckBox cbLan;
        private CheckBox cbHttps;
		private TextBox tbWebPort;
		private TextBox tbWebIp;
		private CheckBox cbLog;
		private CheckBox cbVmix;
		private CheckBox cbUdp;
		private CheckBox cbFile;
		private TextBox tbVmixHost;
		private TextBox tbVmixInput;
		private CheckBox cbExtD;
		private ComboBox cbExtPortD;
		private ComboBox cbExtBaudD;
		private CheckBox cbNetD;
		private TextBox tbNetHostD;
		private TextBox tbNetPortD;
		private TextBox tbLiveD;
		private TextBox tbUdpHost;
		private TextBox tbFrame;
		private TextBox tbWebPass;
		private CheckBox cbRemember;
		private TextBox tbIdle;

		public DevAdminForm(Tablo.TConfig cfg, Size ownerSize)
		{
			this.cfg = cfg;
			this.Text = "Режим разработчика";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Size = new Size(720, ownerSize.Height);
			Panel bottom = new Panel();
			bottom.Dock = DockStyle.Bottom;
			bottom.Height = 48;
			Button bApply = new Button();
			bApply.Text = "Применить";
			bApply.Size = new Size(120, 26);
			bApply.Location = new Point(10, 10);
			bApply.Click += delegate
			{
				this.SaveAll();
				this.DialogResult = DialogResult.OK;
				this.Close();
			};
			Button bInstant = new Button();
			bInstant.Text = "Момент. применение";
			bInstant.Size = new Size(150, 26);
			bInstant.Location = new Point(140, 10);
			bInstant.Click += delegate
			{
				this.SaveAll();
			};
			Button bCancel = new Button();
			bCancel.Text = "Отменить";
			bCancel.Size = new Size(120, 26);
			bCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			bCancel.Click += delegate
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close();
			};
			bottom.Controls.Add(bApply);
			bottom.Controls.Add(bInstant);
			bottom.Controls.Add(bCancel);
			bCancel.Location = new Point(bottom.ClientSize.Width - 130, 10);
			Panel scroll = new Panel();
			scroll.Dock = DockStyle.Fill;
			scroll.AutoScroll = true;
			int colW = (this.ClientSize.Width - 30) / 2;
			if (colW < 240)
			{
				colW = 240;
			}
			int xR = 10 + colW + 10;
			GroupBox g1 = new GroupBox();
			g1.Text = "Веб-сервер";
			g1.Location = new Point(10, 8);
			g1.Size = new Size(colW, 118);
			cbWeb = new CheckBox();
			cbWeb.Text = "Сервер включен (вывод и оператор)";
			cbWeb.Location = new Point(8, 18);
			cbWeb.Size = new Size(colW - 16, 20);
			cbWeb.Checked = (cfg.WebEnable == "1");
			Label lbPort = new Label();
			lbPort.Text = "Порт:";
			lbPort.Location = new Point(8, 45);
			lbPort.Size = new Size(40, 20);
			tbWebPort = new TextBox();
			tbWebPort.Location = new Point(50, 42);
			tbWebPort.Size = new Size(60, 20);
			tbWebPort.Text = cfg.WebPort;
			Label lbIp = new Label();
			lbIp.Text = "IP:";
			lbIp.Location = new Point(120, 45);
			lbIp.Size = new Size(28, 20);
			tbWebIp = new TextBox();
			tbWebIp.Location = new Point(150, 42);
			tbWebIp.Size = new Size(120, 20);
			tbWebIp.Text = string.IsNullOrEmpty(cfg.WebIp) ? WebServer.DetectLocalIp() : cfg.WebIp;
            			System.Collections.Generic.List<string> ipsNow = WebServer.LocalIpList();
			if (ipsNow.Count > 0 && tbWebIp.Text.Length > 0 && ipsNow.IndexOf(tbWebIp.Text) < 0)
			{
				if (MessageBox.Show("IP-адрес в поле (" + tbWebIp.Text + ") не совпадает ни с одним текущим адресом машины: " + string.Join(", ", ipsNow.ToArray()) + ".\nПодключение по указанному адресу невозможно. Подставить первый актуальный адрес (" + ipsNow[0] + ")?", "Режим разработчика", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					tbWebIp.Text = ipsNow[0];
				}
			}
			cbLan = new CheckBox();
			cbLan.Text = "Доступ из локальной сети";
			cbLan.Location = new Point(8, 68);
			cbLan.Size = new Size(colW - 16, 20);
			cbLan.Checked = (cfg.WebBindAll == "1");
			g1.Controls.Add(cbWeb);
			g1.Controls.Add(lbPort);
			g1.Controls.Add(tbWebPort);
			g1.Controls.Add(lbIp);
			g1.Controls.Add(tbWebIp);
			g1.Controls.Add(cbLan);
             			cbHttps = new CheckBox();
 			cbHttps.Text = "HTTPS (TLS-шифрование)";
 			cbHttps.Location = new Point(8, 92);
 			cbHttps.Size = new Size(colW - 16, 20);
 			cbHttps.Checked = (cfg.WebUseHttps == "1");
 			g1.Controls.Add(cbHttps);
			scroll.Controls.Add(g1);
			GroupBox g2 = new GroupBox();
			g2.Text = "Отдача времени (три способа)";
			g2.Location = new Point(10, 132);
			g2.Size = new Size(colW, 110);
			cbVmix = new CheckBox();
			cbVmix.Text = "В vMix (HTTP API)";
			cbVmix.Location = new Point(8, 16);
			cbVmix.Size = new Size(150, 20);
			tbVmixHost = new TextBox();
			tbVmixHost.Location = new Point(8, 38);
			tbVmixHost.Size = new Size(110, 20);
			tbVmixInput = new TextBox();
			tbVmixInput.Location = new Point(122, 38);
			tbVmixInput.Size = new Size(colW - 130, 20);
			cbUdp = new CheckBox();
			cbUdp.Text = "По UDP (JSON)";
			cbUdp.Location = new Point(8, 62);
			cbUdp.Size = new Size(110, 20);
			tbUdpHost = new TextBox();
			tbUdpHost.Location = new Point(122, 62);
			tbUdpHost.Size = new Size(colW - 130, 20);
			cbFile = new CheckBox();
			cbFile.Text = "Писать время в tablo_time.txt";
			cbFile.Location = new Point(8, 86);
			cbFile.Size = new Size(colW - 16, 20);
			g2.Controls.Add(cbVmix);
			g2.Controls.Add(tbVmixHost);
			g2.Controls.Add(tbVmixInput);
			g2.Controls.Add(cbUdp);
			g2.Controls.Add(tbUdpHost);
			g2.Controls.Add(cbFile);
			scroll.Controls.Add(g2);
			GroupBox g3 = new GroupBox();
			g3.Text = "Логи и кадр вывода";
			g3.Location = new Point(10, 246);
			g3.Size = new Size(colW, 122);   // Пульт-3: +22 px под переключатель лога команд пульта
			cbLog = new CheckBox();
			cbLog.Text = "Вести лог пульта remote_com_log.txt";
			cbLog.Location = new Point(8, 16);
			cbLog.Size = new Size(colW - 16, 20);
			cbLog.Checked = cfg.RemoteLogEnable;
			// Пульт-3 (аудит 2026-11): лог каждой команды пульта — сеансовый переключатель,
			// по умолчанию выключен, действует сразу (кнопка «Применить» не нужна).
			CheckBox cbPultLog = new CheckBox();
			cbPultLog.Text = "Логировать каждую команду с физ. пульта";
			cbPultLog.Location = new Point(8, 38);
			cbPultLog.Size = new Size(colW - 16, 20);
			cbPultLog.Checked = SerialPortLog.PultCmdLog;
			cbPultLog.CheckedChanged += delegate
			{
				SerialPortLog.PultCmdLog = cbPultLog.Checked;
				SerialPortLog.Write("РЕЖИМ РАЗРАБОТЧИКА: лог каждой команды пульта " + (cbPultLog.Checked ? "ВКЛЮЧЕН" : "выключен"));
			};
			Label lf = new Label();
			lf.Text = "Кадр:";
			lf.Location = new Point(8, 66);
			lf.Size = new Size(40, 20);
			tbFrame = new TextBox();
			tbFrame.Location = new Point(50, 63);
			tbFrame.Size = new Size(colW - 58, 20);
			tbFrame.Text = cfg.ExtFrameTemplate;
					Button bClearLogs = new Button();
		bClearLogs.Text = "Очистить логи";
		bClearLogs.Size = new Size(colW - 16, 23);
		bClearLogs.Location = new Point(8, 90);   // Пульт-3: сдвинута под новый переключатель
		bClearLogs.Click += delegate
		{
			if (MessageBox.Show("Удалить содержимое ВСЕХ файлов-логов (remote_com_log.txt, crash_log.txt)? Действие необратимо, текущие логи будут потеряны.", "Режим разработчика", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				SerialPortLog.ClearAll();
				MessageBox.Show("Логи очищены. Ведение лога продолжается с новой строки.", "Режим разработчика", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		};
		g3.Controls.Add(cbLog);
		g3.Controls.Add(cbPultLog);
		g3.Controls.Add(lf);
		g3.Controls.Add(tbFrame);
		g3.Controls.Add(bClearLogs);
		scroll.Controls.Add(g3);
			// 2026-11 (запрос пользователя): функционал «Дополнительный вывод» (RS-485 и сеть)
			// перенесён сюда из «Настройки -> Прочие» — это служебный вывод, место ему в режиме
			// разработчика. Значения читаются ExtOut напрямую из cfg каждый тик, так что
			// «Применить» этого окна сразу включает/выключает отдачу.
			GroupBox g6 = new GroupBox();
			g6.Text = "Дополнительный вывод (время матча наружу)";
			g6.Location = new Point(10, 372);
			g6.Size = new Size(colW, 150);
			cbExtD = new CheckBox();
			cbExtD.Text = "Отдача вывода (RS-485)";
			cbExtD.Location = new Point(8, 16);
			cbExtD.Size = new Size(220, 20);
			cbExtD.Checked = cfg.ExtEnable;
			Label lbExtPortD = new Label();
			lbExtPortD.Text = "Порт:";
			lbExtPortD.Location = new Point(8, 45);
			lbExtPortD.Size = new Size(40, 20);
			cbExtPortD = new ComboBox();
			cbExtPortD.DropDownStyle = ComboBoxStyle.DropDownList;
			cbExtPortD.Location = new Point(50, 42);
			cbExtPortD.Size = new Size(110, 21);
			cbExtPortD.DrawMode = DrawMode.OwnerDrawFixed;
			string pultPortD = cfg.Port;
			foreach (string pnD in System.IO.Ports.SerialPort.GetPortNames())
			{
				cbExtPortD.Items.Add(pnD);
			}
			if (cbExtPortD.Items.Count == 0)
			{
				cbExtPortD.Items.Add("");
			}
			if (!string.IsNullOrEmpty(cfg.ExtPort) && cbExtPortD.Items.Contains(cfg.ExtPort))
			{
				cbExtPortD.SelectedItem = cfg.ExtPort;
			}
			else
			{
				cbExtPortD.SelectedIndex = 0;
			}
			cbExtPortD.Tag = cbExtPortD.SelectedItem;
			cbExtPortD.DrawItem += delegate(object sPortD, DrawItemEventArgs ePortD)
			{
				ePortD.DrawBackground();
				if (ePortD.Index >= 0)
				{
					string itemTextD = (string)cbExtPortD.Items[ePortD.Index];
					bool busyD = (itemTextD == pultPortD);
					Brush brD = busyD ? SystemBrushes.GrayText : SystemBrushes.WindowText;
					ePortD.Graphics.DrawString(busyD ? itemTextD + " (занят пультом)" : itemTextD, ePortD.Font, brD, ePortD.Bounds);
				}
				ePortD.DrawFocusRectangle();
			};
			cbExtPortD.SelectedIndexChanged += delegate
			{
				string selD = cbExtPortD.SelectedItem as string;
				if (selD == pultPortD)
				{
					cbExtPortD.SelectedItem = cbExtPortD.Tag;
				}
				else
				{
					cbExtPortD.Tag = selD;
				}
			};
			Label lbExtBaudD = new Label();
			lbExtBaudD.Text = "Скорость:";
			lbExtBaudD.Location = new Point(168, 45);
			lbExtBaudD.Size = new Size(60, 20);
			cbExtBaudD = new ComboBox();
			cbExtBaudD.DropDownStyle = ComboBoxStyle.DropDownList;
			cbExtBaudD.Location = new Point(230, 42);
			cbExtBaudD.Size = new Size(80, 21);
			cbExtBaudD.Items.AddRange(new object[]
			{
				"9600",
				"19200",
				"38400",
				"57600",
				"115200"
			});
			if (cbExtBaudD.Items.Contains(cfg.ExtBaud))
			{
				cbExtBaudD.SelectedItem = cfg.ExtBaud;
			}
			else
			{
				cbExtBaudD.SelectedItem = "9600";
			}
			cbNetD = new CheckBox();
			cbNetD.Text = "Сетевая отдача (RJ-45, UDP)";
			cbNetD.Location = new Point(8, 68);
			cbNetD.Size = new Size(280, 20);
			cbNetD.Checked = cfg.NetEnable;
			Label lbNetHostD = new Label();
			lbNetHostD.Text = "Адрес:";
			lbNetHostD.Location = new Point(8, 95);
			lbNetHostD.Size = new Size(60, 20);
			tbNetHostD = new TextBox();
			tbNetHostD.Location = new Point(70, 92);
			tbNetHostD.Size = new Size(120, 20);
			tbNetHostD.Text = cfg.NetHost;
			Label lbNetPortD = new Label();
			lbNetPortD.Text = "Порт:";
			lbNetPortD.Location = new Point(200, 95);
			lbNetPortD.Size = new Size(40, 20);
			tbNetPortD = new TextBox();
			tbNetPortD.Location = new Point(242, 92);
			tbNetPortD.Size = new Size(60, 20);
			tbNetPortD.Text = cfg.NetPort;
			Label lbLiveD = new Label();
			lbLiveD.Text = "Живой кадр:";
			lbLiveD.Location = new Point(8, 121);
			lbLiveD.Size = new Size(80, 20);
			tbLiveD = new TextBox();
			tbLiveD.ReadOnly = true;
			tbLiveD.Location = new Point(92, 118);
			tbLiveD.Size = new Size(colW - 108, 20);
			tbLiveD.Text = ExtOut.LastLine.TrimEnd(new char[] { '\r' });
			System.Windows.Forms.Timer liveTD = new System.Windows.Forms.Timer();
			liveTD.Interval = 1000;
			liveTD.Tick += delegate
			{
				if (tbLiveD.IsDisposed)
				{
					liveTD.Stop();
					return;
				}
				tbLiveD.Text = ExtOut.LastLine.TrimEnd(new char[] { '\r' });
			};
			// О-17 (аудит): окно режима разработчика прячется, а не закрывается — таймер
			// «живого кадра» не должен тикать в скрытом окне.
			this.VisibleChanged += delegate
			{
				if (this.Visible && !tbLiveD.IsDisposed)
				{
					liveTD.Start();
				}
				else
				{
					liveTD.Stop();
				}
			};
			liveTD.Start();
			ToolTip tpFrameD = new ToolTip();
			tpFrameD.SetToolTip(lf, "Кодовые слова кадра:\r\n{MODE} - режим: BRK перерыв, PRE отсчёт заставки, GAME матч, CLK часы\r\n{TIME} - время ММ:СС (для CLK - ЧЧ:ММ:СС)\r\n{SIG} - сигнал 0 или 1\r\n{TO} - адресат: J судьи, T команды, A все\r\nПример кадра: EXT;BRK;12:34;1;A");
			g6.Controls.Add(cbExtD);
			g6.Controls.Add(lbExtPortD);
			g6.Controls.Add(cbExtPortD);
			g6.Controls.Add(lbExtBaudD);
			g6.Controls.Add(cbExtBaudD);
			g6.Controls.Add(cbNetD);
			g6.Controls.Add(lbNetHostD);
			g6.Controls.Add(tbNetHostD);
			g6.Controls.Add(lbNetPortD);
			g6.Controls.Add(tbNetPortD);
			g6.Controls.Add(lbLiveD);
			g6.Controls.Add(tbLiveD);
			scroll.Controls.Add(g6);
			GroupBox g4 = new GroupBox();
			g4.Text = "Пароль оператора веб-панели";
			g4.Location = new Point(xR, 8);
			g4.Size = new Size(colW, 56);
			tbWebPass = new TextBox();
			tbWebPass.PasswordChar = '*';
			tbWebPass.Location = new Point(8, 20);
			tbWebPass.Size = new Size(110, 20);
			Button bWeb = new Button();
			bWeb.Text = "Задать пароль";
			bWeb.Location = new Point(122, 18);
			bWeb.Size = new Size(colW - 130, 23);
			bWeb.Click += delegate
			{
				// БЛОК 4, F7-03: хэш PBKDF2 с солью (старый SHA-1 больше не записывается;
				// проверка входа понимает оба формата). БЛОК 5: минимум 5 символов.
				if (tbWebPass.Text.Length < DevGuard.MinPasswordLength)
				{
					MessageBox.Show("Введите новый пароль веб-оператора (не короче " + DevGuard.MinPasswordLength.ToString() + " символов).");
					return;
				}
				cfg.WebPassHash = WebServer.HashPassword(tbWebPass.Text);
				cfg.Save();
				WebServer.PassHash = cfg.WebPassHash;
				tbWebPass.Text = "";
				MessageBox.Show("Пароль веб-оператора обновлён.");
			};
			g4.Controls.Add(tbWebPass);
			g4.Controls.Add(bWeb);
			scroll.Controls.Add(g4);
			GroupBox g5 = new GroupBox();
			g5.Text = "Пароли входа и блокировка по бездействию";
			g5.Location = new Point(xR, 68);
			g5.Size = new Size(colW, 210);   // ПУНКТ 3: +34 px под кнопку сброса счётчика попыток
			Label lbIdle = new Label();
			lbIdle.Text = "Блокировка при бездействии, минут:";
			lbIdle.Location = new Point(8, 18);
			lbIdle.Size = new Size(colW - 60, 20);
			tbIdle = new TextBox();
			tbIdle.Location = new Point(colW - 48, 17);
			tbIdle.Size = new Size(40, 20);
			tbIdle.Text = cfg.DevIdleMinutes;
			cbRemember = new CheckBox();
			cbRemember.Location = new Point(8, 44);
			cbRemember.Size = new Size(colW - 16, 20);
			cbRemember.Text = "Запомнить пароль на " + cfg.DevIdleMinutes + " минут";
			Button bCh = new Button();
			bCh.Text = "Сменить пароль входа";
			bCh.Location = new Point(8, 74);
			bCh.Size = new Size(colW - 16, 23);
			bCh.Click += delegate
			{
				string cur = DevUi.PromptPassword("Текущий пароль входа");
				if (cur == null)
				{
					return;
				}
				// БЛОК 4, F7-03: проверка текущего пароля через VerifyDev (PBKDF2/SHA-1
				// + прозрачный апгрейд хэша при входе).
				if (!DevGuard.VerifyDev(cfg, cur))
				{
					MessageBox.Show("Неверный текущий пароль.");
					return;
				}
				string n1 = DevUi.PromptPassword("Новый пароль входа (не короче " + DevGuard.MinPasswordLength.ToString() + " символов)");
				if (n1 == null || n1.Length == 0)
				{
					return;
				}
				if (n1.Length < DevGuard.MinPasswordLength)   // БЛОК 5: было 6 (F7-06 блока 4)
				{
					MessageBox.Show("Новый пароль короче " + DevGuard.MinPasswordLength.ToString() + " символов — не принят.");
					return;
				}
				string n2 = DevUi.PromptPassword("Повторите новый пароль");
				if (n1 != n2)
				{
					MessageBox.Show("Повтор не совпал.");
					return;
				}
				cfg.DevPassHash = WebServer.HashPassword(n1);   // БЛОК 4, F7-03: PBKDF2
				cfg.WebEnableToken = DevGuard.Sign(cfg.DevPassHash, cfg.WebEnable);
				cfg.Save();
				MessageBox.Show("Пароль входа изменён.");
			};
			Button bRs = new Button();
			bRs.Text = "Сброс пароля входа (супер)";
			bRs.Location = new Point(8, 106);
			bRs.Size = new Size(colW - 16, 23);
			bRs.Click += delegate
			{
				string s = DevUi.PromptPassword("Супер-пароль для сброса");
				if (s == null)
				{
					return;
				}
				if (!DevGuard.IsSuper(s))
				{
					MessageBox.Show("Неверный супер-пароль.");
					return;
				}
				cfg.DevPassHash = "";
				cfg.WebEnable = "0";
				cfg.WebEnableToken = "";
				cfg.Save();
				WebServer.Stop();
				DevSession.Forget();
				MessageBox.Show("Пароль входа сброшен, сервер остановлен.");
			};
			Button bRsW = new Button();
			bRsW.Text = "Сброс пароля веб-оператора (супер)";
			bRsW.Location = new Point(8, 138);
			bRsW.Size = new Size(colW - 16, 23);
			bRsW.Click += delegate
			{
				string s = DevUi.PromptPassword("Супер-пароль для сброса");
				if (s == null)
				{
					return;
				}
				if (!DevGuard.IsSuper(s))
				{
					MessageBox.Show("Неверный супер-пароль.");
					return;
				}
				cfg.WebPassHash = "";
				cfg.Save();
				WebServer.PassHash = "";
				MessageBox.Show("Пароль веб-оператора сброшен.");
			};
			g5.Controls.Add(lbIdle);
			g5.Controls.Add(tbIdle);
			g5.Controls.Add(cbRemember);
			g5.Controls.Add(bCh);
			g5.Controls.Add(bRs);
			// ================== ПУНКТ 3: РУЧНОЙ СБРОС СЧЁТЧИКА НЕУДАЧНЫХ ПОПЫТОК ВХОДА ==================
			// Сбрасывает уровень нарастающей паузы по ВСЕМ устройствам (веб-вход).
			// Подтверждается повторным вводом показанного значения — защита от случайного нажатия.
			Button bRsF = new Button();
			bRsF.Text = "Сброс счётчика попыток входа";
			bRsF.Location = new Point(8, 170);
			bRsF.Size = new Size(colW - 16, 23);
			bRsF.Click += delegate
			{
				int devCnt = 0;
				int maxWait = 0;
				int maxSeries = 0;
				int lvl = WebServer.GetFailStateAll(out devCnt, out maxWait, out maxSeries);
				if (devCnt <= 0 && lvl <= 0)
				{
					MessageBox.Show("Счётчик неудачных попыток входа пуст - сбрасывать нечего.");
					return;
				}
				string msg = "Сбросить счётчик попыток входа по ВСЕМ устройствам?\r\n\r\n"
					+ "Устройств в счётчике: " + devCnt + "\r\n"
					+ "Осталось попыток: " + WebServer.LoginAttemptsLeft(maxSeries) + " из " + WebServer.LoginMaxAttempts + "\r\n"
					+ "Блокировок было: " + lvl + "\r\n"
					+ (maxWait > 0 ? ("Остаток блокировки: " + WebServer.FmtPause(maxWait) + "\r\n") : "")
					+ "\r\nПосле сброса снова 10 попыток, следующая блокировка - 15 сек.\r\n"
					+ "Пароль не требуется - только подтверждение.";
				if (MessageBox.Show(msg, "Сброс счётчика попыток входа",
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
				{
					return;
				}
				int n = WebServer.ResetLoginFails();
				MessageBox.Show("Счётчик неудачных попыток входа сброшен.\r\nУстройств сброшено: " + n + ".\r\nСчётчик снова 10 попыток (следующая блокировка - 15 сек).");
			};
			g5.Controls.Add(bRsW);
			g5.Controls.Add(bRsF);
			scroll.Controls.Add(g5);
			this.Controls.Add(scroll);
			this.Controls.Add(bottom);
			DevSession.DevForm = this;
			cbRemember.Checked = DevSession.Remembered;
			cbRemember.CheckedChanged += delegate
			{
				if (cbRemember.Checked)
				{
					DevSession.Grant(ParseIdle());
				}
				else
				{
					DevSession.Forget();
				}
			};
			tbIdle.TextChanged += delegate
			{
				cbRemember.Text = "Запомнить пароль на " + ParseIdle().ToString() + " минут";
			};
			string vmApi = "0";
			string vmHost = "192.168.1.100:8088";
			string vmInput = "Title.xaml";
			string udApi = "0";
			string udHost = "192.168.1.100:9999";
			string fl = "0";
			// БЛОК 4, F4-04/F8-07: чтение через TextIO (BOM/UTF-8/ANSI), а сбой
			// чтения больше не проглатывается молча — уходит в диагностику.
			try
			{
				string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tablo_stream.txt");
				if (File.Exists(p))
				{
					foreach (string ln in TextIO.ReadAllLines(p))
					{
						int i = ln.IndexOf('=');
						if (i < 0)
						{
							continue;
						}
						string k = ln.Substring(0, i).Trim();
						string v = ln.Substring(i + 1).Trim();
						if (k == "VmixApi")
						{
							vmApi = v;
						}
						else if (k == "VmixHost")
						{
							vmHost = v;
						}
						else if (k == "VmixInput")
						{
							vmInput = v;
						}
						else if (k == "Udp")
						{
							udApi = v;
						}
						else if (k == "UdpHost")
						{
							udHost = v;
						}
						else if (k == "File")
						{
							fl = v;
						}
					}
				}
			}
			catch (Exception exStream)
			{
				Diag.Note("Чтение tablo_stream.txt (DevAdminForm)", exStream);
			}
			cbVmix.Checked = (vmApi == "1");
			tbVmixHost.Text = vmHost;
			tbVmixInput.Text = vmInput;
			cbUdp.Checked = (udApi == "1");
			tbUdpHost.Text = udHost;
			cbFile.Checked = (fl == "1");
		}

		private int ParseIdle()
		{
			int v = 15;
			try
			{
				v = int.Parse(tbIdle.Text.Trim());
			}
			catch
			{
			}
			if (v < 1)
			{
				v = 1;
			}
			if (v > 240)
			{
				v = 240;
			}
			return v;
		}

		private int WebPortNum()
		{
			int p = 8642;
			try
			{
				p = int.Parse(cfg.WebPort);
			}
			catch
			{
			}
			if (p < 1024 || p > 65535)
			{
				p = 8642;
			}
			return p;
		}

		private void SaveAll()
		{
			// БЛОК 6, З10: запоминаем ПРЕЖНИЕ значения параметров веб-сервера.
			// Сервер будет перезапущен, только если хотя бы один из них реально изменился.
			string oldWebEnable = cfg.WebEnable;
			string oldWebPort = cfg.WebPort;
			string oldWebBindAll = cfg.WebBindAll;
			string oldWebUseHttps = cfg.WebUseHttps;
			cfg.WebEnable = cbWeb.Checked ? "1" : "0";
			cfg.WebEnableToken = DevGuard.Sign(cfg.DevPassHash, cfg.WebEnable);
			cfg.RemoteLogEnable = cbLog.Checked;
			SerialPortLog.Enabled = cbLog.Checked;
			cfg.ExtFrameTemplate = tbFrame.Text.Trim();
			// 2026-11: параметры перенесённого сюда «Дополнительного вывода» (RS-485/сеть).
			cfg.ExtEnable = cbExtD.Checked;
			if (cbExtPortD.SelectedItem != null)
			{
				cfg.ExtPort = cbExtPortD.SelectedItem as string;
			}
			if (cbExtBaudD.SelectedItem != null)
			{
				cfg.ExtBaud = cbExtBaudD.SelectedItem as string;
			}
			cfg.NetEnable = cbNetD.Checked;
			cfg.NetHost = tbNetHostD.Text.Trim();
			cfg.NetPort = tbNetPortD.Text.Trim();
			cfg.DevIdleMinutes = ParseIdle().ToString();
			cfg.WebPort = tbWebPort.Text.Trim();
			cfg.WebBindAll = cbLan.Checked ? "1" : "0";
            cfg.WebUseHttps = cbHttps.Checked ? "1" : "0";
			cfg.WebIp = tbWebIp.Text.Trim();
            			System.Collections.Generic.List<string> ipsSave = WebServer.LocalIpList();
			if (ipsSave.Count > 0 && cfg.WebIp.Length > 0 && ipsSave.IndexOf(cfg.WebIp) < 0)
			{
				MessageBox.Show("Внимание: указанный IP (" + cfg.WebIp + ") не является текущим адресом машины (" + string.Join(", ", ipsSave.ToArray()) + "). Сервер слушает все адреса, поэтому подключаться нужно по одному из списка или по localhost; значение сохранено как есть.", "Режим разработчика", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			cfg.Save();
			try
			{
				string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tablo_stream.txt");
				// БЛОК 4, F4-03/F8-09: атомарная запись с BOM (русские названия полей
				// в vMix читаются корректно), сбой больше не проглатывается молча.
				TextIO.WriteAll(p, "VmixApi=" + (cbVmix.Checked ? "1" : "0") + "\r\nVmixHost=" + tbVmixHost.Text + "\r\nVmixInput=" + tbVmixInput.Text + "\r\nVmixField=TimeText\r\nUdp=" + (cbUdp.Checked ? "1" : "0") + "\r\nUdpHost=" + tbUdpHost.Text + "\r\nFile=" + (cbFile.Checked ? "1" : "0") + "\r\n", true);
			}
			catch (Exception exStreamSave)
			{
				Diag.Note("Сохранение tablo_stream.txt", exStreamSave);
				MessageBox.Show("Не удалось сохранить tablo_stream.txt: " + exStreamSave.Message, "Режим разработчика", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			if (cbRemember.Checked)
			{
				DevSession.Grant(ParseIdle());
			}
			// БЛОК 6, З10: перезапуск веб-сервера ТОЛЬКО при реальных изменениях его
			// параметров (включен/порт/доступ из сети/HTTPS). Остальные настройки окна
			// (логи, кадр вывода, vMix/UDP, пароль веб-оператора) применяются БЕЗ обрыва
			// подключений операторов — раньше сервер падал и поднимался при каждом «Применить».
			bool webChanged = (oldWebEnable != cfg.WebEnable) || (oldWebPort != cfg.WebPort) ||
			(oldWebBindAll != cfg.WebBindAll) || (oldWebUseHttps != cfg.WebUseHttps);
			bool wantWeb = DevGuard.WebEnabled(cfg);
			if (webChanged)
			{
				WebServer.Stop();
				if (wantWeb)
				{
					WebServer.Start(WebPortNum(), cfg.WebBindAll == "1", cfg.WebIp, cfg.WebUseHttps == "1");
				}
			}
			else if (wantWeb && !WebServer.IsRunning)
			{
				// параметры не менялись, но сервер почему-то не поднят (например, не стартовал
				// при запуске программы) — поднимаем без остановки.
				WebServer.Start(WebPortNum(), cfg.WebBindAll == "1", cfg.WebIp, cfg.WebUseHttps == "1");
			}
			else if (!wantWeb && WebServer.IsRunning)
			{
				WebServer.Stop();
			}
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			if (DevSession.DevForm == this)
			{
				DevSession.DevForm = null;
			}
			base.OnFormClosed(e);
		}
	}

	 public class WebSession
 {
 	public string Token;
 	public string Finger;
 	public DateTime LastCmd;
 	public bool Super;
 	public string Ip;
 	public string Ua;
 	public DateTime Created;   // БЛОК 4, F7-07: абсолютный срок жизни сессии (8 ч)
 	public string Sid;         // БЛОК 4, F7-11: короткий публичный идентификатор (не токен)
 }

	// ================== РђР’РўРћРџРћРРЎРљ РЎР•Р Р’Р•Р Рђ Р’ Р›РћРљРђР›Р¬РќРћР™ РЎР•РўР ==================
	// Android-РїСЂРёР»РѕР¶РµРЅРёРµ С€Р»С‘С‚ UDP-broadcast В«TABLO_DISCOVER_V1В» РЅР° РїРѕСЂС‚ 8643;
	// РїСЂРѕРіСЂР°РјРјР° РѕС‚РІРµС‡Р°РµС‚ В«TABLO_SERVER_V1|РёРјСЏ|РїРѕСЂС‚|httpsВ». Р Р°Р±РѕС‚Р°РµС‚, РєРѕРіРґР° С‚РµР»РµС„РѕРЅ
	// Рё РєРѕРјРїСЊСЋС‚РµСЂ РІ РѕРґРЅРѕР№ РїРѕРґСЃРµС‚Рё. Р СѓС‡РЅРѕР№ РІРІРѕРґ IP РІ РїСЂРёР»РѕР¶РµРЅРёРё СЃРѕС…СЂР°РЅС‘РЅ.
	public static class LanDiscovery
	{
		public const int UdpPort = 8643;
		private const string Request = "TABLO_DISCOVER_V1";
		private static System.Net.Sockets.UdpClient _udp;
		private static System.Threading.Thread _thread;
		private static volatile bool _run;
		private static int _webPort;
		private static bool _https;

		public static void Start(int webPort, bool https)
		{
			Stop();
			try
			{
				_webPort = webPort;
				_https = https;
				System.Net.Sockets.UdpClient u = new System.Net.Sockets.UdpClient(UdpPort);
				u.EnableBroadcast = true;
				try
				{
					u.Client.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket,
						System.Net.Sockets.SocketOptionName.ReuseAddress, true);
				}
				catch
				{
				}
				_udp = u;
				_run = true;
				System.Threading.Thread t = new System.Threading.Thread(Loop);
				t.IsBackground = true;
				_thread = t;
				t.Start();
				SerialPortLog.Write("WEB: Р°РІС‚РѕРїРѕРёСЃРє СЃРµСЂРІРµСЂР° РІРєР»СЋС‡С‘РЅ (UDP " + UdpPort.ToString()
					+ "). Р•СЃР»Рё С‚РµР»РµС„РѕРЅ РЅРµ РЅР°С…РѕРґРёС‚ С‚Р°Р±Р»Рѕ вЂ” СЂР°Р·СЂРµС€РёС‚Рµ UDP " + UdpPort.ToString() + " РІ Р±СЂР°РЅРґРјР°СѓСЌСЂРµ Windows.");
			}
			catch (Exception ex)
			{
				SerialPortLog.Write("WEB: Р°РІС‚РѕРїРѕРёСЃРє СЃРµСЂРІРµСЂР° РЅРµ Р·Р°РїСѓС‰РµРЅ (UDP " + UdpPort.ToString() + "): " + ex.Message);
			}
		}

		public static void Stop()
		{
			_run = false;
			System.Net.Sockets.UdpClient u = _udp;
			_udp = null;
			_thread = null;
			try
			{
				if (u != null)
				{
					u.Close();
				}
			}
			catch
			{
			}
		}

		private static void Loop()
		{
			while (_run)
			{
				System.Net.Sockets.UdpClient u = _udp;
				if (u == null)
				{
					return;
				}
				System.Net.IPEndPoint ep = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
				try
				{
					byte[] data = u.Receive(ref ep);
					string req = Encoding.UTF8.GetString(data);
					if (req.IndexOf(Request, StringComparison.Ordinal) < 0)
					{
						continue;
					}
					string answer = "TABLO_SERVER_V1|" + Environment.MachineName + "|"
						+ _webPort.ToString() + "|" + (_https ? "1" : "0");
					byte[] resp = Encoding.UTF8.GetBytes(answer);
					u.Send(resp, resp.Length, ep);
				}
				catch
				{
					if (!_run)
					{
						return;
					}
					try
					{
						System.Threading.Thread.Sleep(100);
					}
					catch
					{
					}
				}
			}
		}
	}

	public static class WebServer
	{
		private static System.Net.HttpListener _listener;
		private static System.Threading.Thread _thread;
		private static readonly object _sync = new object();
		public static string StateCache = "{}";
		public static string PubCache = "{}";   // БЛОК 3, Q15: публичный урезанный state для /display и /pub.json
		// БЛОК 3, F3-09: 0=выполнено, 1=неизвестная команда, 2=отклонено защитой Q6, 3=сбой.
		// БЛОК 7 (аудит): добавлен 3-й параметр isSuper — права сессии, чтобы привилегированные
		// команды (god_toggle) нельзя было выполнить с обычным паролем оператора.
		public static Func<string, int, bool, int> CmdProvider;
		public static Func<string, string, bool> CmdTextProvider;
		public static string PassHash = "";
		public static int IdleLimitMin = 30;
		public static int MaxDevices = 2;
		// ===== БЛОК 7 (аудит): «режим бога» больше не публичное изменяемое поле =====
		// БЫЛО: public static bool GodMode = false;  — любой, у кого есть ссылка на класс,
		//       мог снять/поставить флаг; веб-команда god_toggle принималась от ЛЮБОЙ
		//       авторизованной сессии (без s.Super) и отключала сразу три защиты:
		//       запрет тайм-аута и перерыва во время идущего периода (frmMain.cs:5744, 5783)
		//       и подтверждение смены периода (frmMain.cs:7810).
		// СТАЛО: приватное поле, чтение через свойство, переключение — только через
		//       SetGodMode(...) с обязательной записью в журнал (кто и когда) и с индикацией
		//       в статус-строке табло. Право переключения осталось у ОБЕИХ категорий
		//       веб-пользователей (решение заказчика: кнопка специально вынесена в отдельное
		//       окно, случайное нажатие исключено). GodModeTtlMin=0 — авто-выключение отключено.
		private static volatile bool _godMode = false;
		// БЛОК 7: DateTime НЕ может быть volatile (CS0677) — храним тики long и читаем/пишем
		// через Interlocked (атомамрно на 32-битной платформе).
		private static long _godModeUntilTicks = 0L;
		// 0 = авто-выключение ОТКЛЮЧЕНО (поведение как до аудита: флаг держится, пока его
		// не снимут вручную). Поставьте, например, 30 — тогда флаг сам снимется через 30 мин.
		public static int GodModeTtlMin = 0;
		public static bool GodMode
		{
			get
			{
				long untilTicks = System.Threading.Interlocked.Read(ref _godModeUntilTicks);
				if (_godMode && GodModeTtlMin > 0 && untilTicks != 0L && DateTime.Now.Ticks > untilTicks)
				{
					_godMode = false;
					SerialPortLog.Write("WEB: режим бога ВЫКЛЮЧЕН автоматически (прошло " + GodModeTtlMin.ToString(Inv) + " мин)");
				}
				return _godMode;
			}
		}
		// isSuper передаётся ТОЛЬКО для журнала (кто переключил) — отказов по правам нет:
		// по решению заказчика «режим бога» доступен и обычной сессии оператора.
		public static void SetGodMode(bool on, string who, bool isSuper)
		{
			if (_godMode == on)
			{
				SerialPortLog.Write("WEB: режим бога уже " + (on ? "ВКЛЮЧЕН" : "ВЫКЛЮЧЕН") + " — повторное переключение от " + who + " проигнорировано");
				return;
			}
			_godMode = on;
			System.Threading.Interlocked.Exchange(ref _godModeUntilTicks,
				(on && GodModeTtlMin > 0) ? DateTime.Now.AddMinutes((double)GodModeTtlMin).Ticks : 0L);
			SerialPortLog.Write("WEB: режим бога " + (on ? "ВКЛЮЧЕН" : "ВЫКЛЮЧЕН") + " (" + who + ")"
				+ (on && GodModeTtlMin > 0 ? (" авто-выключение через " + GodModeTtlMin.ToString(Inv) + " мин") : ""));
		}
		private static readonly System.Collections.Generic.Dictionary<string, WebSession> _tokens = new System.Collections.Generic.Dictionary<string, WebSession>();

		// БЛОК 4, F9-02: один общий экземпляр SHA1 под блокировкой вместо создания
		// нового объекта на каждый вызов (SHA1 не потокобезопасен — поэтому lock).
		private static readonly System.Security.Cryptography.SHA1 _sha1 = System.Security.Cryptography.SHA1.Create();
		private static readonly object _sha1Lock = new object();
		private static readonly System.Globalization.CultureInfo Inv = System.Globalization.CultureInfo.InvariantCulture;

		public static string Sha1(string s)
		{
			byte[] r;
			lock (_sha1Lock)
			{
				r = _sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s ?? ""));
			}
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for (int i = 0; i < r.Length; i++)
			{
				sb.Append(r[i].ToString("x2"));
			}
			return sb.ToString();
		}

		// ================== БЛОК 4, F7-03: PBKDF2-хэширование паролей ==================
		// Формат сохранённого хэша: "pbkdf2$<итерации>$<соль base64>$<хэш base64>".
		// Старые SHA-1-хэши (40 hex-символов) принимаются для ВХОДА и прозрачно
		// заменяются на PBKDF2 (см. DevGuard.VerifyDev и /login). Сравнение —
		// в постоянном времени (нельзя подобрать по времени ответа).
		public const int Pbkdf2Iterations = 20000;

		public static string HashPassword(string pass)
		{
			byte[] salt = new byte[16];
			// .NET 3.5: RNGCryptoServiceProvider ещё НЕ реализует IDisposable — без using.
			System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
			rng.GetBytes(salt);
			return Pbkdf2(pass, salt, Pbkdf2Iterations);
		}

		private static string Pbkdf2(string pass, byte[] salt, int iters)
		{
			System.Security.Cryptography.Rfc2898DeriveBytes k = new System.Security.Cryptography.Rfc2898DeriveBytes(System.Text.Encoding.UTF8.GetBytes(pass ?? ""), salt, iters);
			byte[] h = k.GetBytes(32);
			return "pbkdf2$" + iters.ToString(Inv) + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(h);
		}

		public static bool VerifyPassword(string pass, string stored, out bool legacySha1)
		{
			legacySha1 = false;
			if (string.IsNullOrEmpty(stored))
			{
				return false;
			}
			if (stored.StartsWith("pbkdf2$"))
			{
				string[] p = stored.Split(new char[] { '$' });
				if (p.Length != 4)
				{
					return false;
				}
				int it;
				byte[] salt;
				byte[] want;
				if (!int.TryParse(p[1], out it) || it < 1000 || it > 500000)
				{
					return false;
				}
				try
				{
					salt = Convert.FromBase64String(p[2]);
					want = Convert.FromBase64String(p[3]);
				}
				catch
				{
					return false;
				}
				if (want.Length == 0)
				{
					return false;
				}
				byte[] got;
				try
				{
					System.Security.Cryptography.Rfc2898DeriveBytes k = new System.Security.Cryptography.Rfc2898DeriveBytes(System.Text.Encoding.UTF8.GetBytes(pass ?? ""), salt, it);
					got = k.GetBytes(want.Length);
				}
				catch
				{
					return false;
				}
				return FixedTimeEquals(got, want);
			}
			// старый формат — «голый» SHA-1 (40 hex)
			legacySha1 = true;
			return FixedTimeStrEquals(Sha1(pass), stored);
		}

		public static bool FixedTimeEquals(byte[] a, byte[] b)
		{
			if (a == null || b == null || a.Length != b.Length)
			{
				return false;
			}
			int diff = 0;
			for (int i = 0; i < a.Length; i++)
			{
				diff |= (int)(a[i] ^ b[i]);
			}
			return diff == 0;
		}

		// О-13 (аудит 2026-11): БЫЛО — ранний выход при несовпадении длин «протекал» длиной
		// токена, а посимвольный XOR шёл по исходным строкам разной длины. Стало — обе строки
		// хэшируются SHA1 и сравниваются побайтово всегда по 40 hex-символам: время сравнения
		// не зависит ни от содержимого, ни от длины входа. Равенство строк сохраняется 1:1
		// (Sha1(x)==Sha1(y) <=> x==y с криптографической точностью).
		public static bool FixedTimeStrEquals(string a, string b)
		{
			if (a == null || b == null)
			{
				return false;
			}
			string ha = Sha1(a);
			string hb = Sha1(b);
			int diff = 0;
			for (int i = 0; i < 40; i++)
			{
				diff |= (int)(ha[i] ^ hb[i]);
			}
			return diff == 0;
		}

		// БЛОК 4, F3-11: вместо Regex.Unescape (он раскрывает ЛЮБЫЕ escape-последовательности)
		// — ограниченный декодер клиентского uenc(): только «обратная косая + |», двойная
		// обратная косая и юникод-эскейпы u+4hex (управляющие символы не пропускаются).
		// Разделитель «|» остаётся защищённым от подделки.
		public static string UnescapeText(string s)
		{
			if (s == null || s.IndexOf('\\') < 0)
			{
				return s;
			}
			System.Text.StringBuilder sb = new System.Text.StringBuilder(s.Length);
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (c == '\\' && i + 1 < s.Length)
				{
					char n = s[i + 1];
					if (n == 'u' && i + 5 < s.Length)
					{
						int code;
						if (int.TryParse(s.Substring(i + 2, 4), System.Globalization.NumberStyles.HexNumber, Inv, out code) && code >= 32 && code <= 0xFFFF)
						{
							sb.Append((char)code);
							i += 5;
							continue;
						}
					}
					if (n == '|' || n == '\\')
					{
						sb.Append(n);
						i++;
						continue;
					}
				}
				sb.Append(c);
			}
			return sb.ToString();
		}

		// БЛОК 4, F7-03: frmMain подписывается сюда, чтобы при прозрачном апгрейде
		// хэша веб-оператора (SHA-1 -> PBKDF2) новый хэш сохранялся в config.xml.
		public static Action<string> PassHashUpgraded;

		// БЛОК 4, F7-08: помним, что сервер поднят по HTTPS (для HSTS-заголовка).
		private static volatile bool _useHttps = false;

		// БЛОК 4, F7-07: заголовки безопасности для всех ответов.
		private static void AddSecurityHeaders(System.Net.HttpListenerContext ctx, bool isHtml)
		{
			try
			{
				ctx.Response.AddHeader("X-Content-Type-Options", "nosniff");
				ctx.Response.AddHeader("X-Frame-Options", "DENY");
				ctx.Response.AddHeader("Referrer-Policy", "no-referrer");
				if (isHtml)
				{
					ctx.Response.AddHeader("Content-Security-Policy", "default-src 'none'; script-src 'unsafe-inline'; style-src 'unsafe-inline'; connect-src 'self'; img-src data:");
				}
				if (_useHttps)
				{
					ctx.Response.AddHeader("Strict-Transport-Security", "max-age=31536000");
				}
			}
			catch
			{
			}
		}

			public static string DetectLocalIp()
	{
		System.Collections.Generic.List<string> l = LocalIpList();
		return (l.Count > 0) ? l[0] : "";
	}

	public static System.Collections.Generic.List<string> LocalIpList()
	{
		System.Collections.Generic.List<string> res = new System.Collections.Generic.List<string>();
		try
		{
			foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
			{
				if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up ||
					ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
				{
					continue;
				}
				System.Net.NetworkInformation.IPInterfaceProperties pp = ni.GetIPProperties();
				bool gw = (pp != null && pp.GatewayAddresses != null && pp.GatewayAddresses.Count > 0);
				System.Collections.Generic.List<string> mine = new System.Collections.Generic.List<string>();
				foreach (System.Net.NetworkInformation.UnicastIPAddressInformation ua in pp.UnicastAddresses)
				{
					if (ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
					{
						mine.Add(ua.Address.ToString());
					}
				}
				if (gw)
				{
					res.InsertRange(0, mine);
				}
				else
				{
					res.AddRange(mine);
				}
			}
		}
		catch
		{
		}
		return res;
	}

		private static string FingerOf(System.Net.HttpListenerContext ctx)
		{
			string ip = "-";
			try
			{
				ip = ctx.Request.RemoteEndPoint.Address.ToString();
			}
			catch
			{
			}
			string ua = ctx.Request.Headers["User-Agent"] ?? "";
			return Sha1(ip + "|" + ua);
		}

		private static void PurgeExpired()
		{
			lock (_sync)
			{
				System.Collections.Generic.List<string> dead = new System.Collections.Generic.List<string>();
				foreach (System.Collections.Generic.KeyValuePair<string, WebSession> kv in _tokens)
				{
					if ((DateTime.Now - kv.Value.LastCmd).TotalMinutes > IdleLimitMin)
					{
						dead.Add(kv.Key);
					}
				}
				foreach (string k in dead)
				{
					_tokens.Remove(k);
				}
			}
		}

				public static void Start(int port, bool bindAll, string ip, bool useHttps)
	{
		lock (_sync)
		{
			if (_thread != null)
			{
				return;
			}
		}
		string scheme = useHttps ? "https" : "http";
		_useHttps = useHttps;   // БЛОК 4, F7-08: для HSTS-заголовка
		System.Collections.Generic.List<string> candidates = new System.Collections.Generic.List<string>();
		if (bindAll)
		{
			candidates.Add(scheme + "://+:" + port + "/");
		}
		candidates.Add(scheme + "://localhost:" + port + "/");
		foreach (string px in candidates)
		{
			// БЛОК 4, F3-15: слушатель объявлен ВНЕ try — при неудаче его нужно закрыть,
			// иначе объект остаётся «полуподнятым» и удерживает порт до перезапуска.
			System.Net.HttpListener l = new System.Net.HttpListener();
			try
			{
				l.Prefixes.Add(px);
				l.Start();
				_listener = l;
				if (px.StartsWith(scheme + "://+"))
				{
					SerialPortLog.Write("WEB: сервер запущен на " + px + " (все адреса: localhost, " + (string.IsNullOrEmpty(ip) ? DetectLocalIp() : ip) + ", имя машины)");
				}
				else
				{
					SerialPortLog.Write("WEB: сервер запущен на " + px + " (только localhost)");
				}
				System.Threading.Thread t = new System.Threading.Thread(Loop);
				t.IsBackground = true;
				_thread = t;
				t.Start();
				if (bindAll)
				{
					LanDiscovery.Start(port, useHttps);
				}
				return;
			}
			catch (Exception ex)
			{
				try { l.Close(); } catch { }
				SerialPortLog.Write("WEB: префикс " + px + " не поднялся: " + ex.Message);
			}
		}
		SerialPortLog.Write("WEB: НЕ УДАЛОСЬ поднять сервер. Для сети один раз от администратора: netsh http add urlacl url=" + scheme + "://+:" + port + "/ user=Everyone");
		if (useHttps)
		{
			SerialPortLog.Write("WEB: для HTTPS привяжите сертификат один раз от администратора: netsh http add sslcert ipport=0.0.0.0:" + port + " certhash=THUMBPRINT appid={00000000-0000-0000-0000-000000000000}");
			// БЛОК 4, F7-08: причина отказа HTTPS объясняется явно, с подсказкой для XP.
			if (Environment.OSVersion.Version.Major < 6)
			{
				SerialPortLog.Write("WEB: HTTPS не поднят. На Windows XP сертификат привязывается командой: httpcfg set ssl -i 0.0.0.0:" + port + " -h THUMBPRINT (httpcfg.exe входит в Windows Support Tools).");
			}
			else
			{
				SerialPortLog.Write("WEB: HTTPS не поднят. Частые причины: сертификат не привязан к порту (netsh http add sslcert ...) или нет прав на префикс (netsh http add urlacl ...). В НЕзашифрованном режиме сервер НЕ запущен — при необходимости переключитесь на HTTP в режиме разработчика.");
			}
		}
	}

		// БЛОК 6, З10: признак «сервер реально поднят» — нужен, чтобы не перезапускать
		// веб-сервер при сохранении настроек, которые его НЕ касаются.
		public static bool IsRunning
		{
			get
			{
				lock (_sync)
				{
					return _listener != null && _thread != null;
				}
			}
		}

		// БЛОК 6, З8: счётчики активных веб-сессий для статус-строки программы:
		// обычные подключения (их ограничивает MaxDevices) и служебные (супер-пароль).
		public static void GetSessionCounts(out int regular, out int superCount)
		{
			regular = 0;
			superCount = 0;
			lock (_sync)
			{
				foreach (System.Collections.Generic.KeyValuePair<string, WebSession> kv in _tokens)
				{
					if (kv.Value.Super)
					{
						superCount++;
					}
					else
					{
						regular++;
					}
				}
			}
		}

		public static void Stop()
		{
			System.Net.HttpListener l;
			lock (_sync)
			{
				l = _listener;
				_listener = null;
				_thread = null;
			}
			try
			{
				if (l != null)
				{
					l.Stop();
					l.Close();
				}
			}
			catch
			{
			}
			LanDiscovery.Stop();
		}

		private static bool Auth(System.Net.HttpListenerContext ctx, out WebSession ses)
		{
			ses = null;
			// БЛОК 4, F7-07: токен передаётся в заголовке X-Tablo-Token (он не попадает
			// в URL, историю браузера и логи прокси). Старый вариант ?token= работает
			// как запасной — открытые в браузере страницы прошлой версии не «отвалятся».
			string tok = ctx.Request.Headers["X-Tablo-Token"];
			if (string.IsNullOrEmpty(tok))
			{
				tok = ctx.Request.QueryString["token"];
			}
			if (string.IsNullOrEmpty(tok))
			{
				return false;
			}
			lock (_sync)
			{
				WebSession s;
				if (!_tokens.TryGetValue(tok, out s))
				{
					return false;
				}
				if ((DateTime.Now - s.LastCmd).TotalMinutes > IdleLimitMin)
				{
					_tokens.Remove(tok);
					return false;
				}
				// О-7 (аудит 2026-11): сессия привязана к ТОКЕНУ, а не к IP+User-Agent.
				// БЫЛО: телефон оператора при роуминге между точками Wi-Fi (или смене DHCP-адреса)
				// терял сессию посреди матча — панель требовала повторный вход, «Запомнить вход»
				// не спасал. СТАЛО: токен — секрет доступа; отпечаток устройства — мягкий сигнал:
				// при его смене пишем в журнал и запоминаем новый, сессию НЕ рвём.
				// Риск угона токена компенсирован уже имеющимися защитами: лимит простоя
				// (IdleLimitMin), абсолютный TTL 8 ч (проверка ниже), MaxDevices=2, передача
				// токена в заголовке X-Tablo-Token (не светится в URL/истории/логах прокси).
				string fingerNow = FingerOf(ctx);
				if (s.Finger != fingerNow)
				{
					s.Finger = fingerNow;
					try
					{
						s.Ip = ctx.Request.RemoteEndPoint.Address.ToString();
					}
					catch
					{
					}
					s.Ua = ctx.Request.Headers["User-Agent"] ?? "";
					SerialPortLog.Write("WEB: сессия " + s.Sid + " — клиент сменил адрес/user-agent (токен действителен, сессия сохранена)");
				}
				// БЛОК 4, F7-07: абсолютный срок жизни сессии — 8 часов, даже если
				// панель активно используется. Дальше — повторный вход по паролю.
				if ((DateTime.Now - s.Created).TotalHours > 8.0)
				{
					_tokens.Remove(tok);
					SerialPortLog.Write("WEB: сессия закрыта - превышен лимит 8 часов, требуется повторный вход");
					return false;
				}
				ses = s;
				return true;
			}
		}

		private static void Touch(WebSession s)
		{
			lock (_sync)
			{
				s.LastCmd = DateTime.Now;
			}
		}
       	// ================== ПУНКТ 3: ПОПЫТКИ ВХОДА И ПРОГРЕССИВНАЯ БЛОКИРОВКА ==================
	// 10 попыток ввода пароля; блокировка назначается после 11-й неудачной:
	//	1 -> 15 с, 2 -> 30 с, 3 -> 45 с, 4 -> 1 мин, 5 -> 2 мин, 6 и далее -> 5 мин (потолок).
	// После разблокировки счётчик снова равен 10 (серия обнуляется в момент блокировки).
	// _failSeries — неудачи ТЕКУЩЕЙ серии (10 = последняя попытка без блокировки).
	// _failLevel  — номер блокировки: сбрасывается ТОЛЬКО явно (веб-панель с повышенными
	//               правами либо меню «Режим разработчика») — см. ResetLoginFails().
	// Счётчик, номер блокировки и время блокировки сохраняются между перезапусками
	// в login_fails_state.txt (см. LoadFailState/SaveFailState).
	// Вход с повышенными правами (супер-пароль) временной заглушкой НЕ ограничивается.
	public const int LoginMaxAttempts = 10;
	private static readonly int[] FailPauseSec = new int[] { 15, 30, 45, 60, 120, 300 };
	private static readonly System.Collections.Generic.Dictionary<string, int> _failSeries = new System.Collections.Generic.Dictionary<string, int>();
	private static readonly System.Collections.Generic.Dictionary<string, int> _failLevel = new System.Collections.Generic.Dictionary<string, int>();
	private static readonly System.Collections.Generic.Dictionary<string, DateTime> _failUntil = new System.Collections.Generic.Dictionary<string, DateTime>();
	// время последней неудачи (для «уборки» устаревших серий) и отметка последней «уборки».
	private static readonly System.Collections.Generic.Dictionary<string, DateTime> _failLast = new System.Collections.Generic.Dictionary<string, DateTime>();
	private static DateTime _lastHouseKeep = DateTime.Now;

	// ===== СОХРАНЕНИЕ СЧЁТЧИКА И ВРЕМЕНИ БЛОКИРОВКИ МЕЖДУ ПЕРЕЗАПУСКАМИ =====
	// Формат строки: ip \t серия \t номер блокировки \t until.Ticks \t last.Ticks
	private const string FailStateFile = "login_fails_state.txt";
	private static readonly object _failFileSync = new object();

	static WebServer()
	{
		LoadFailState();
	}

	private static string FailStatePath()
	{
		return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FailStateFile);
	}

	private static void LoadFailState()
	{
		try
		{
			string p = FailStatePath();
			if (!File.Exists(p))
			{
				return;
			}
			lock (_sync)
			{
				_failSeries.Clear();
				_failLevel.Clear();
				_failUntil.Clear();
				_failLast.Clear();
				foreach (string line in TextIO.ReadAllLines(p))
				{
					if (string.IsNullOrEmpty(line))
					{
						continue;
					}
					string[] a = line.Split('\t');
					if (a.Length < 5 || a[0].Length == 0)
					{
						continue;
					}
					string ip = a[0];
					int ser;
					int lvl;
					long until;
					long last;
					if (!int.TryParse(a[1], out ser)) { continue; }
					if (!int.TryParse(a[2], out lvl)) { continue; }
					if (!long.TryParse(a[3], out until)) { continue; }
					if (!long.TryParse(a[4], out last)) { continue; }
					if (ser < 0) { ser = 0; }
					if (ser > LoginMaxAttempts) { ser = LoginMaxAttempts; }
					if (lvl < 0) { lvl = 0; }
					_failSeries[ip] = ser;
					_failLevel[ip] = lvl;
					if (until > 0) { _failUntil[ip] = new DateTime(until); }
					if (last > 0) { _failLast[ip] = new DateTime(last); }
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Note("Загрузка счётчика попыток входа", ex);
		}
	}

	// Снимок состояния формируется под _sync, запись на диск — вне блокировки.
	private static void SaveFailState()
	{
		try
		{
			// Запись сериализована: снимок и файл не могут быть перепутаны между потоками.
			lock (_failFileSync)
			{
				StringBuilder sb = new StringBuilder();
				lock (_sync)
				{
					System.Collections.Generic.List<string> ips = new System.Collections.Generic.List<string>();
					foreach (string ip in _failLevel.Keys) { ips.Add(ip); }
					foreach (string ip in _failSeries.Keys)
					{
						if (!_failLevel.ContainsKey(ip)) { ips.Add(ip); }
					}
					for (int i = 0; i < ips.Count; i++)
					{
						string ip = ips[i];
						int ser;
						int lvl;
						DateTime until;
						DateTime last;
						if (!_failSeries.TryGetValue(ip, out ser)) { ser = 0; }
						if (!_failLevel.TryGetValue(ip, out lvl)) { lvl = 0; }
						_failUntil.TryGetValue(ip, out until);
						_failLast.TryGetValue(ip, out last);
						sb.Append(ip).Append('\t')
							.Append(ser.ToString(Inv)).Append('\t')
							.Append(lvl.ToString(Inv)).Append('\t')
							.Append(until.Ticks.ToString(Inv)).Append('\t')
							.Append(last.Ticks.ToString(Inv)).Append("\r\n");
					}
				}
				TextIO.WriteAll(FailStatePath(), sb.ToString(), false);
			}
		}
		catch (Exception ex)
		{
			Diag.Note("Сохранение счётчика попыток входа", ex);
		}
	}

	// Сколько попыток осталось в текущей серии (10 в начале, 0 перед блокировкой).
	public static int LoginAttemptsLeft(int series)
	{
		int left = LoginMaxAttempts - series;
		return left < 0 ? 0 : left;
	}

	// Пауза в секундах для уровня level (нумерация с 1). Уровень выше таблицы -> потолок.
	public static int FailPauseForLevel(int level)
	{
		if (level < 1)
		{
			return 0;
		}
		if (level > FailPauseSec.Length)
		{
			level = FailPauseSec.Length;
		}
		return FailPauseSec[level - 1];
	}

	// Человекочитаемое название паузы (для журнала и сообщений).
	public static string FmtPause(int sec)
	{
		if (sec <= 0) return "без паузы";
		if (sec >= 60 && sec % 60 == 0) return (sec / 60).ToString(Inv) + " мин";
		if (sec >= 60) return ((double)sec / 60.0).ToString("0.#", Inv) + " мин";
		return sec.ToString(Inv) + " сек";
	}

	private static DateTime BanUntil(string ip)
	{
		lock (_sync)
		{
			DateTime t;
			if (_failUntil.TryGetValue(ip, out t))
			{
				return t;
			}
			return DateTime.MinValue;
		}
	}

	// Состояние заглушки конкретного устройства: уровень, серия, остаток паузы (сек).
	public static void GetFailState(string ip, out int level, out int series, out int waitSec)
	{
		lock (_sync)
		{
			_failLevel.TryGetValue(ip, out level);
			_failSeries.TryGetValue(ip, out series);
			DateTime t;
			waitSec = 0;
			if (_failUntil.TryGetValue(ip, out t) && t > DateTime.Now)
			{
				waitSec = (int)Math.Ceiling((t - DateTime.Now).TotalSeconds);
			}
		}
	}

	// Сводка по ВСЕМ устройствам — нужна для ручного сброса (сбрасываем всё скопом,
	// оператор не обязан знать IP телефона судьи). Возвращает номер последней блокировки,
	// максимальный остаток паузы и самую длинную текущую серию неудач.
	public static int GetFailStateAll(out int devices, out int maxWaitSec, out int maxSeries)
	{
		lock (_sync)
		{
			int lvl = 0;
			devices = 0;
			maxWaitSec = 0;
			maxSeries = 0;
			foreach (System.Collections.Generic.KeyValuePair<string, int> kv in _failLevel)
			{
				devices++;
				if (kv.Value > lvl)
				{
					lvl = kv.Value;
				}
			}
			foreach (string ip in _failSeries.Keys)
			{
				if (!_failLevel.ContainsKey(ip))
				{
					devices++;
				}
			}
			foreach (System.Collections.Generic.KeyValuePair<string, int> kvS in _failSeries)
			{
				if (kvS.Value > maxSeries)
				{
					maxSeries = kvS.Value;
				}
			}
			foreach (System.Collections.Generic.KeyValuePair<string, DateTime> kv2 in _failUntil)
			{
				if (kv2.Value > DateTime.Now)
				{
					int w = (int)Math.Ceiling((kv2.Value - DateTime.Now).TotalSeconds);
					if (w > maxWaitSec)
					{
						maxWaitSec = w;
					}
				}
			}
			return lvl;
		}
	}

	// Неудачная попытка входа: серия растёт. На 11-й неудаче серия обнуляется
	// (после разблокировки снова 10 попыток), номер блокировки увеличивается и
	// назначается пауза. Запись уходит в журнал и в login_fails.txt.
	private static bool RegisterFail(string ip, string device, out int level, out int sec)
	{
		int series = 0;
		level = 0;
		sec = 0;
		bool locked = false;
		lock (_sync)
		{
			_failLevel.TryGetValue(ip, out level);
			_failSeries.TryGetValue(ip, out series);
			series++;
			_failLast[ip] = DateTime.Now;
			if (series >= LoginMaxAttempts + 1)
			{
				level++;
				_failLevel[ip] = level;
				_failSeries[ip] = 0;   // после разблокировки счётчик снова 10
				sec = FailPauseForLevel(level);
				if (sec > 0)
				{
					_failUntil[ip] = DateTime.Now.AddSeconds((double)sec);
				}
				locked = true;
			}
			else
			{
				_failSeries[ip] = series;
			}
		}
		SaveFailState();
		// Записи вне lock: файловый ввод не держит блокировку сервера.
		if (locked)
		{
			SerialPortLog.Write("WEB: неверный пароль, IP " + ip + " — 11-я неудача, блокировка №"
				+ level.ToString(Inv) + " на " + FmtPause(sec) + "; после разблокировки снова "
				+ LoginMaxAttempts.ToString(Inv) + " попыток");
		}
		else
		{
			SerialPortLog.Write("WEB: неверный пароль, IP " + ip + " — неудача " + series.ToString(Inv)
				+ " из " + LoginMaxAttempts.ToString(Inv) + ", без блокировки (блокировка после 11-й)");
		}
		LoginFailLog.Write(device, ip);
		return locked;
	}

	private static string FmtBan(int sec)
	{
		if (sec >= 86400) return "24 часа";
		if (sec >= 3600) return (sec / 3600).ToString(Inv) + " ч";
		if (sec >= 60) return (sec / 60).ToString(Inv) + " мин";
		return sec.ToString(Inv) + " сек";
	}

	// Периодическая «уборка» — вызывается из HandleContext примерно раз в 60 секунд:
	// истёкшие сессии, закончившиеся баны и СЕРИИ неудач (IP без неудач 24 ч).
	// ВАЖНО (пункт 3): уровень паузы _failLevel уборкой НЕ снимается — только явным сбросом.
	public static void HouseKeep()
	{
		PurgeExpired();
		bool failStateChanged = false;
		lock (_sync)
		{
			System.Collections.Generic.List<string> gone = new System.Collections.Generic.List<string>();
			foreach (System.Collections.Generic.KeyValuePair<string, DateTime> kv in _bans)
			{
				if (kv.Value <= DateTime.Now) gone.Add(kv.Key);
			}
			foreach (string k in gone) _bans.Remove(k);
			gone.Clear();
			foreach (System.Collections.Generic.KeyValuePair<string, DateTime> kv in _failLast)
			{
				if (kv.Value <= DateTime.Now.AddHours(-24.0)) gone.Add(kv.Key);
			}
			foreach (string k in gone)
			{
				_failSeries.Remove(k);
				_failLast.Remove(k);
				_failUntil.Remove(k);
			}
			failStateChanged = gone.Count > 0;
		}
		if (failStateChanged)
		{
			// убираем устаревшие серии и из файла (номер блокировки сохраняется)
			SaveFailState();
		}
	}

	// Успешный вход: серия обнуляется (снова 10 попыток), пауза снимается, номер
	// блокировки остаётся — его сбрасывает только явный ручной сброс.
	private static void ClearFails(string ip)
	{
		lock (_sync)
		{
			_failSeries.Remove(ip);
			_failUntil.Remove(ip);
		}
		SaveFailState();
	}

	// Явный ручной сброс (веб-панель с повышенными правами / меню разработчика):
	// снимает серии, паузы и номера блокировок по всем устройствам — снова 10 попыток.
	// Админские баны (_bans) не трогаются — это отдельная мера, её снимает только срок.
	public static int ResetLoginFails()
	{
		int n;
		lock (_sync)
		{
			n = _failLevel.Count;
			foreach (string ip in _failSeries.Keys)
			{
				if (!_failLevel.ContainsKey(ip))
				{
					n++;
				}
			}
			_failSeries.Clear();
			_failLevel.Clear();
			_failUntil.Clear();
			_failLast.Clear();
		}
		SaveFailState();
		SerialPortLog.Write("WEB: счётчик неудачных попыток входа сброшен вручную (устройств в списке было: " + n.ToString(Inv) + ")");
		CrashLog.Append("SECURITY", "счётчик неудачных попыток веб-входа сброшен вручную (устройств: " + n.ToString(Inv) + ")");
		return n;
	}
 	private static readonly System.Collections.Generic.Dictionary<string, DateTime> _bans = new System.Collections.Generic.Dictionary<string, DateTime>();

 	private static void PurgeBans()
 	{
 		lock (_sync)
 		{
 			System.Collections.Generic.List<string> gone = new System.Collections.Generic.List<string>();
 			foreach (System.Collections.Generic.KeyValuePair<string, DateTime> kv in _bans)
 			{
 				if (kv.Value <= DateTime.Now)
 				{
 					gone.Add(kv.Key);
 				}
 			}
 			foreach (string k in gone)
 			{
 				_bans.Remove(k);
 			}
 		}
 	}

 	private static bool BanActive(string ip)
 	{
 		lock (_sync)
 		{
 			DateTime t;
 			if (_bans.TryGetValue(ip, out t))
 			{
 				return t > DateTime.Now;
 			}
 			return false;
 		}
 	}

 	private static string JEsc(string s)
 	{
 		if (s == null)
 		{
 			return "";
 		}
 		System.Text.StringBuilder sb = new System.Text.StringBuilder();
 		foreach (char c in s)
 		{
 			if (c == '\\' || c == '"')
 			{
 				sb.Append('\\').Append(c);
 			}
 			else if (c < ' ')
 			{
 				sb.Append(' ');
 			}
 			else
 			{
 				sb.Append(c);
 			}
 		}
 		return sb.ToString();
 	}

	// БЛОК 7 (аудит): команды, которые меняют «правила игры», а не состояние матча.
	// Доступны ТОЛЬКО сессии с повышенными правами (вход по супер-ключу).
	// Обычному оператору веб-панели достаточно счёта/времени/удалений.
	// БЛОК 7 (аудит), ред. по решению заказчика: god_toggle здесь НЕТ — «режим бога»
		// доступен обеим категориям веб-пользователей (кнопка специально вынесена в отдельное
		// окно/панель, чтобы исключить случайное нажатие). Список оставлен как механизм:
		// сюда добавляются ключи, которые позже понадобится закрыть супер-ключом.
		private static readonly string[] _superOnlyKeys = new string[0];
	public static bool IsSuperOnlyKey(string key)
	{
		if (string.IsNullOrEmpty(key)) { return false; }
		for (int i = 0; i < _superOnlyKeys.Length; i++) { if (_superOnlyKeys[i] == key) return true; }
		return false;
	}

		// ===== БЛОК 7 (аудит), П5: ограничение одновременной обработки запросов =====
		// БЫЛО: Loop() на каждый запрос без ограничения ставил задачу в ThreadPool.
		//       MaxDevices ограничивает только ЧИСЛО СЕССИЙ, а /pub.json и /display доступны
		//       без авторизации. Флуд запросами истощал пул потоков, а каждый /cmd dodatkowo
		//       делал BeginInvoke в UI-поток: очередь сообщений табло забивалась, и команды
		//       ПУЛЬТА (тоже BeginInvoke) выполнялись с задержкой.
		// СТАЛО: не более MaxConcurrentRequests запросов одновременно; лишние получают 503
		//       сразу, без расхода памяти и без обращения к UI-потоку.
		public const int MaxConcurrentRequests = 8;
		private static readonly System.Threading.Semaphore _slots = new System.Threading.Semaphore(MaxConcurrentRequests, MaxConcurrentRequests);

		private static void Loop()
		{
			while (true)
			{
				System.Net.HttpListener l = _listener;
				if (l == null || !l.IsListening)
				{
					break;
				}
				System.Net.HttpListenerContext ctx = null;
				try
				{
					ctx = l.GetContext();
				}
				catch
				{
					break;
				}
				// БЛОК 3, F2-09: КАЖДЫЙ запрос обрабатывается в своём потоке из пула.
				// Раньше один поток обслуживал запросы по очереди: один "зависший" клиент
				// (телефон уснул посреди ответа) блокировал панель и остальные устройства.
				// Общее состояние (сессии, кэш) защищено lock, команды на форму уходят
				// через BeginInvoke - многопоточность здесь безопасна.
				System.Net.HttpListenerContext ctxCopy = ctx;
				// БЛОК 7 (аудит), П5: берём слот; если все заняты — сразу 503 (без ThreadPool)
				if (!_slots.WaitOne(0, false))
				{
					try
					{
						ctxCopy.Response.StatusCode = 503;
						ctxCopy.Response.ContentType = "text/plain; charset=utf-8";
						byte[] b503 = System.Text.Encoding.UTF8.GetBytes("busy");
						ctxCopy.Response.ContentLength64 = b503.Length;
						ctxCopy.Response.OutputStream.Write(b503, 0, b503.Length);
						ctxCopy.Response.Close();
					}
					catch
					{
					}
					Diag.LogOnce("WEB 503", "WEB: все слоты обработки заняты (" + MaxConcurrentRequests.ToString(Inv) + ") — запрос отклонён с 503. Похоже на флуд или на зависший клиент.");
					continue;
				}
				System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(HandleContextGuarded), ctxCopy);
			}
			SerialPortLog.Write("WEB: поток слушателя завершился");
		}

		// БЛОК 7 (аудит), П5: обёртка, гарантирующая освобождение слота
		private static void HandleContextGuarded(object stateObj)
		{
			try
			{
				HandleContext(stateObj);
			}
			catch (Exception ex)
			{
				Diag.Note("HTTP обработчик", ex);
			}
			finally
			{
				try { _slots.Release(); } catch { }
			}
		}

		private static void HandleContext(object stateObj)
		{
			System.Net.HttpListenerContext ctx = (System.Net.HttpListenerContext)stateObj;
			try
			{
				// БЛОК 4, F7-06: уборка устаревших сессий/банов/счётчиков ~раз в 60 с.
				// Гонка потоков безвредна: максимум уборка выполнится дважды подряд.
				if ((DateTime.Now - _lastHouseKeep).TotalSeconds > 60.0)
				{
					_lastHouseKeep = DateTime.Now;
					HouseKeep();
				}
				string url = ctx.Request.Url.AbsolutePath;
				string body = "";
				string ctype = "text/html; charset=utf-8";
				if (url == "/state.json")
					{
						// БЛОК 3, Q15: полный state теперь ТОЛЬКО для вошедших (token).
						// Публичная витрина /display читает урезанный /pub.json.
						// Сессию этот запрос НЕ продлевает (Q17: 30 мин без КОМАНД = выход).
						ctype = "application/json; charset=utf-8";
						WebSession ss;
						if (!Auth(ctx, out ss))
						{
							ctx.Response.StatusCode = 403;
							body = "{\"ok\":0}";
						}
						else
						{
							body = (StateCache == null || StateCache.Length == 0) ? "{}" : StateCache;
						}
					}
					else if (url == "/pub.json")
					{
						// БЛОК 3, Q15: публичные данные витрины: счёт, время, период,
						// названия, удаления, таймауты. Служебных полей здесь нет.
						ctype = "application/json; charset=utf-8";
						body = (PubCache == null || PubCache.Length == 0) ? "{}" : PubCache;
					}
				else if (url == "/" || url == "/display")
				{
					body = DisplayHtml();
				}
				else if (url == "/passstate")
				{
					ctype = "application/json; charset=utf-8";
					body = "{\"set\":" + (((PassHash != null && PassHash.Length > 0) ? 1 : 0)).ToString() + "}";
				}
				 				else if (url == "/login")
 				{
 					ctype = "application/json; charset=utf-8";
 					string cip = "-";
 					try
 					{
 						cip = ctx.Request.RemoteEndPoint.Address.ToString();
 					}
 					catch
 					{
 					}
 					PurgeBans();
 					// ПУНКТ 3: имя устройства (User-Agent) пишется в отдельный журнал login_fails.txt.
 					string devName = "";
 					try { devName = ctx.Request.Headers["User-Agent"] ?? ""; } catch { }
 					// Админский бан устройства (30 мин) действует для ВСЕХ, включая супер-пароль:
 					// это отдельная мера, а не «временная заглушка» из пункта 3.
 					if (BanActive(cip))
 					{
 						ctx.Response.StatusCode = 403;
 						body = "{\"error\":5}";
 					}
 					else if (ctx.Request.ContentLength64 > 4096)
 					{
 						ctx.Response.StatusCode = 413;
 						body = "{\"error\":3}";
 					}
 					else
 					{
 						string raw = "";
 						const int MaxLoginBody = 4096;
 						// БЛОК 7 (аудит): раньше лимит проверялся только по ContentLength64, а при
 						// Transfer-Encoding: chunked он равен -1 -> ReadToEnd() читал тело любого размера.
 						// Один такой запрос мог съесть память 32-битного процесса и уронить табло.
 						try
 						{
 							using (System.IO.StreamReader sr = new System.IO.StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
 							{
 								char[] cbuf = new char[MaxLoginBody + 1];
 								int got = 0;
 								int nRead;
 								while (got <= MaxLoginBody && (nRead = sr.Read(cbuf, got, MaxLoginBody + 1 - got)) > 0) { got += nRead; }
 								if (got > MaxLoginBody)
 								{
 									ctx.Response.StatusCode = 413;
 									body = "{\"error\":3}";
 									SerialPortLog.Write("WEB: /login — тело больше " + MaxLoginBody.ToString(Inv) + " байт, запрос отклонён (413)");
 								}
 								else { raw = new string(cbuf, 0, got); }
 							}
 						}
 						catch (Exception exBody)
 						{
 							Diag.Note("Чтение тела /login", exBody);
 						}
 						if (ctx.Response.StatusCode == 413)
 						{
 							byte[] b413 = System.Text.Encoding.UTF8.GetBytes(body);
 							ctx.Response.ContentType = ctype;
 							AddSecurityHeaders(ctx, false);
 							ctx.Response.ContentLength64 = b413.Length;
 							ctx.Response.OutputStream.Write(b413, 0, b413.Length);
 							return;
 						}
 						string pass = "";
 						// БЛОК 4, F3-15: мёртвый код удалён — параметр "rem" никогда не использовался
 						// (браузер сам решает, запоминать вход через localStorage).
 						foreach (string part in raw.Split(new char[] { '&' }))
 						{
 							int eq = part.IndexOf('=');
 							if (eq < 0)
 							{
 								continue;
 							}
 							string k = part.Substring(0, eq);
 							string v = System.Uri.UnescapeDataString(part.Substring(eq + 1));
 							if (k == "pass")
 							{
 								pass = v;
 							}
 						}
 						bool isSuper = DevGuard.IsSuper(pass);
 						// ============ ПУНКТ 3: НАРАСТАЮЩАЯ ПАУЗА ПОСЛЕ НЕУДАЧНЫХ ПОПЫТОК ============
 						// Если пауза сработала — обычный вход отклоняется СРАЗУ: пароль даже не проверяется
 						// и новая пауза не назначается. Вход с повышенными правами (супер-пароль) проходит
 						// БЕЗ ограничений — осознанное решение для удобства оператора.
 						// Клиенту отдаётся реальный остаток паузы, текущий уровень и длина серии.
 						bool failBlocked = (!isSuper) && DateTime.Now < BanUntil(cip);
 						if (failBlocked)
 						{
 							int waitSec = (int)Math.Ceiling((BanUntil(cip) - DateTime.Now).TotalSeconds);
 							if (waitSec < 1) { waitSec = 1; }
 							int lvlNow = 0; int serNow = 0; int waitChk = 0;
 							GetFailState(cip, out lvlNow, out serNow, out waitChk);
 							ctx.Response.StatusCode = 403;
 							body = "{\"error\":4,\"wait\":" + waitSec.ToString(Inv)
 								+ ",\"level\":" + lvlNow.ToString(Inv)
 								+ ",\"series\":" + serNow.ToString(Inv)
								+ ",\"attempts\":" + LoginMaxAttempts.ToString(Inv)
								+ ",\"left\":0}";
 						}
 						// БЛОК 4, F7-03: проверка пароля через PBKDF2 (старый SHA-1 тоже
 						// принимается и при успехе прозрачно заменяется на PBKDF2).
 						bool legacyHash = false;   // инициализация обязательна: VerifyPassword может не вызваться
 						bool okPass = (!failBlocked) && PassHash != null && PassHash.Length > 0
 							&& (VerifyPassword(pass, PassHash, out legacyHash) || isSuper);
 						if (okPass && legacyHash && !isSuper && PassHashUpgraded != null)
 						{
 							try
 							{
 								string upgraded = HashPassword(pass);
 								PassHash = upgraded;
 								PassHashUpgraded(upgraded);
 								SerialPortLog.Write("БЕЗОПАСНОСТЬ: пароль веб-оператора переведён с SHA-1 на PBKDF2");
 							}
 							catch (Exception exUp)
 							{
 								Diag.Note("Апгрейд WebPassHash", exUp);
 							}
 						}
 						PurgeExpired();
 						if (okPass)
 						{
 							ClearFails(cip);
 							string finger = FingerOf(ctx);
 							lock (_sync)
 							{
 								System.Collections.Generic.List<string> same = new System.Collections.Generic.List<string>();
 								foreach (System.Collections.Generic.KeyValuePair<string, WebSession> kv in _tokens)
 								{
 									if (kv.Value.Finger == finger)
 									{
 										same.Add(kv.Key);
 									}
 								}
 								foreach (string k in same)
 								{
 									_tokens.Remove(k);
 								}
 								int regular = 0;
 								foreach (System.Collections.Generic.KeyValuePair<string, WebSession> kv in _tokens)
 								{
 									if (!kv.Value.Super)
 									{
 										regular++;
 									}
 								}
 								if (!isSuper && regular >= MaxDevices)
 								{
 									ctx.Response.StatusCode = 403;
 									body = "{\"error\":2}";
 								}
 								else
 								{
 									WebSession ns = new WebSession();
 									ns.Token = System.Guid.NewGuid().ToString("N");
 									ns.Finger = finger;
 									ns.LastCmd = DateTime.Now;
 									ns.Created = DateTime.Now;   // БЛОК 4, F7-07: TTL 8 часов
 									ns.Sid = ns.Token.Substring(0, 8);   // БЛОК 4, F7-11: публичный идентификатор
 									ns.Super = isSuper;
 									ns.Ip = cip;
 									ns.Ua = ctx.Request.Headers["User-Agent"] ?? "";
 									_tokens[ns.Token] = ns;
 									body = "{\"token\":\"" + ns.Token + "\"}";
 								}
 							}
 						}
 						// ПУНКТ 3: неудача регистрируется, только если пароль действительно проверялся.
 						// При сработавшей заглушке ответ уже сформирован выше (error 4) — счётчик не растёт.
 						else if (!failBlocked)
 						{
 							int failLevel = 0;
 							int failSec = 0;
 							bool locked = RegisterFail(cip, devName, out failLevel, out failSec);
 							int left = 0;
 							int lvlNow = 0; int serNow = 0; int waitChk = 0;
 							GetFailState(cip, out lvlNow, out serNow, out waitChk);
 							if (!locked)
 							{
 								left = LoginAttemptsLeft(serNow);
 							}
 							ctx.Response.StatusCode = 403;
 							if (locked)
 							{
 								// 11-я неудача: блокировка с первой паузой, счётчик уже снова 10.
 								body = "{\"error\":4,\"wait\":" + failSec.ToString(Inv)
 									+ ",\"level\":" + failLevel.ToString(Inv)
 									+ ",\"series\":0,\"attempts\":" + LoginMaxAttempts.ToString(Inv)
 									+ ",\"left\":0,\"locked\":1}";
 							}
 							else
 							{
 								body = "{\"error\":1,\"left\":" + left.ToString(Inv)
 									+ ",\"attempts\":" + LoginMaxAttempts.ToString(Inv)
 									+ ",\"level\":" + failLevel.ToString(Inv) + "}";
 							}
 						}
 					}
 				}
				else if (url == "/sessioncheck")
				{
					ctype = "application/json; charset=utf-8";
					WebSession s;
					if (Auth(ctx, out s))
					{
						body = "{\"ok\":1,\"super\":" + (s.Super ? "1" : "0") + "}";
					}
					else
					{
						ctx.Response.StatusCode = 403;
						body = "{\"ok\":0}";
					}
				}
				else if (url == "/logout")
				{
					ctype = "application/json; charset=utf-8";
					// БЛОК 4, F7-07: токен из заголовка, ?token= — запасной вариант.
					string tok = ctx.Request.Headers["X-Tablo-Token"];
					if (string.IsNullOrEmpty(tok))
					{
						tok = ctx.Request.QueryString["token"];
					}
					lock (_sync)
					{
						if (tok != null)
						{
							_tokens.Remove(tok);
						}
					}
					body = "{\"ok\":1}";
				}
				else if (url == "/cmd")
				{
					ctype = "application/json; charset=utf-8";
					WebSession s;
					// О-9 (аудит 2026-11): изменение состояния — только POST. БЫЛО GET: команду
					// мог дёрнуть <img src=...> с любого сайта (CSRF), prefetch браузера или
					// антивирусный прокси; ?token= попадал в историю и логи промежуточных прокси.
					// GET остался только для чтения (/state.json и др.). Параметры — по-прежнему
					// в query (не секрет), токен — заголовок X-Tablo-Token; ?token= сохранён
					// как переходный запасной путь для открытых страниц прошлой версии.
					if (ctx.Request.HttpMethod != "POST")
					{
						ctx.Response.StatusCode = 405;
						body = "{\"ok\":0,\"err\":5}";
					}
					else if (!Auth(ctx, out s))
					{
						ctx.Response.StatusCode = 403;
						body = "{\"ok\":0}";
					}
					else
					{
						string key = ctx.Request.QueryString["key"];
						int p = 0;
						try
						{
							p = int.Parse(ctx.Request.QueryString["p"] ?? "0");
						}
						catch
						{
						}
						// БЛОК 3, F3-09: честный результат: 0=выполнено, 1=неизвестная команда,
							// 2=отклонено защитой (идёт время периода), 3=сбой. Плюс аудит в лог.
							int rc = 3;
							// БЛОК 7 (аудит): привилегированные команды — только для супер-сессии.
							if (IsSuperOnlyKey(key) && !s.Super)
							{
								SerialPortLog.Write("WEB CMD: ОТКАЗ ip=" + s.Ip + " key=" + (key ?? "-") + " — команда доступна только с повышенными правами");
								ctx.Response.StatusCode = 403;
								body = "{\"ok\":0,\"err\":4}";
							}
							else if (CmdProvider != null)
							{
								try
								{
									rc = CmdProvider(key, p, s.Super);
								}
								catch (Exception exc)
								{
									rc = 3;
									SerialPortLog.Write("WEB: исключение при выполнении команды " + key + ": " + exc.Message);
								}
							}
							Touch(s);
							SerialPortLog.Write("WEB CMD: ip=" + s.Ip + " key=" + (key ?? "-") + " p=" + p.ToString() + " -> " + ((rc == 0) ? "выполнено" : ("отказ, код " + rc.ToString())));
							body = (rc == 0) ? "{\"ok\":1}" : ("{\"ok\":0,\"err\":" + rc.ToString() + "}");
					}
				}
				else if (url == "/cmdtext")
				{
					ctype = "application/json; charset=utf-8";
					WebSession s;
					// О-9 (аудит 2026-11): только POST — см. комментарий в ветке /cmd.
					if (ctx.Request.HttpMethod != "POST")
					{
						ctx.Response.StatusCode = 405;
						body = "{\"ok\":0,\"err\":5}";
					}
					else if (!Auth(ctx, out s))
					{
						ctx.Response.StatusCode = 403;
						body = "{\"ok\":0}";
					}
					else
					{
						string key = ctx.Request.QueryString["key"];
						string v = ctx.Request.QueryString["v"] ?? "";
						// БЛОК 4, F3-11: Regex.Unescape раскрывал ЛЮБЫЕ escape-последовательности
						// (включая \n, \t и мусор) — заменён на ограниченный декодер: только
						// «обратная косая + |», двойная обратная косая и юникод-эскейпы u+4hex
						// (управляющие символы не пропускаются).
						v = UnescapeText(v);
						bool ok = false;
						if (CmdTextProvider != null)
						{
							try
							{
								ok = CmdTextProvider(key, v);
							}
							catch (Exception exTxt)
							{
								// БЛОК 4, F8-03: исключение обработчика больше не проглатывается молча
								ok = false;
								SerialPortLog.Write("WEB: исключение при обработке текстовой команды " + (key ?? "-") + ": " + exTxt.Message);
							}
						}
						Touch(s);
							// БЛОК 3, F3-10: аудит текстовых команд (названия, эталон)
							string vlog = (v.Length > 80) ? (v.Substring(0, 80) + "...") : v;
							SerialPortLog.Write("WEB TEXT: ip=" + s.Ip + " key=" + (key ?? "-") + " v=" + vlog + " -> " + (ok ? "выполнено" : "отклонено"));
							body = ok ? "{\"ok\":1}" : "{\"ok\":0}";
					}
				}
				else if (url == "/adminfail" || url == "/adminresetfails")
				{
					// ================== ПУНКТ 3: РУЧНОЙ СБРОС СЧЁТЧИКА ПОПЫТОК ВХОДА ==================
					// Только для сессии с повышенными правами (супер-пароль).
					// /adminfail       — текущее состояние: максимальный уровень паузы, число устройств,
					//                    остаток самой длинной паузы.
					// /adminresetfails — сброс; ОБЯЗАТЕЛЬНО подтверждение повторным вводом значения:
					//                    ?confirm=N, где N — уровень, показанный панелью. Не совпал ->
					//                    сброс не выполняется и панель получает актуальное значение.
					ctype = "application/json; charset=utf-8";
					WebSession sf;
					if (!Auth(ctx, out sf) || !sf.Super)
					{
						ctx.Response.StatusCode = 403;
						body = "{\"ok\":0}";
					}
					else
					{
						Touch(sf);
						int devCnt = 0;
						int maxWait = 0;
						int maxSeries = 0;
						int lvl = GetFailStateAll(out devCnt, out maxWait, out maxSeries);
						if (url == "/adminfail")
						{
							// Во время действующей блокировки попыток не осталось (0); после неё — снова 10.
							int leftNow = (maxWait > 0) ? 0 : LoginAttemptsLeft(maxSeries);
							body = "{\"ok\":1,\"level\":" + lvl.ToString(Inv)
								+ ",\"devices\":" + devCnt.ToString(Inv)
								+ ",\"wait\":" + maxWait.ToString(Inv)
								+ ",\"left\":" + leftNow.ToString(Inv)
								+ ",\"attempts\":" + LoginMaxAttempts.ToString(Inv)
								+ ",\"pause\":" + FailPauseForLevel(lvl + 1).ToString(Inv) + "}";
						}
						else
						{
							// Только подтверждение (модальное окно панели), без ввода пароля/значения.
							int n = ResetLoginFails();
							SerialPortLog.Write("WEB: АДМИН - сброс счётчика неудачных попыток входа подтверждён (ip=" + sf.Ip + ")");
							body = "{\"ok\":1,\"reset\":" + n.ToString(Inv) + ",\"level\":0,\"left\":"
								+ LoginMaxAttempts.ToString(Inv) + "}";
						}
					}
				}
                     				else if (url == "/admin" || url == "/adminkick" || url == "/adminban")
 				{
 					ctype = "application/json; charset=utf-8";
 					WebSession s;
 					if (!Auth(ctx, out s) || !s.Super)
 					{
 						ctx.Response.StatusCode = 403;
 						body = "{\"ok\":0}";
 					}
 					else if (url == "/admin")
 					{
 						System.Text.StringBuilder sb = new System.Text.StringBuilder();
 						sb.Append("[");
 						lock (_sync)
 						{
 							bool first = true;
 							foreach (System.Collections.Generic.KeyValuePair<string, WebSession> kv in _tokens)
 							{
 								if (!first)
 								{
 									sb.Append(",");
 								}
 								first = false;
 								int idle = (int)(DateTime.Now - kv.Value.LastCmd).TotalMinutes;
 								// БЛОК 4, F7-11: полный токен больше НЕ отдаётся в браузер — только
 								// короткий идентификатор sid (8 символов) и признак «это моя сессия».
 								sb.Append("{\"sid\":\"" + JEsc(kv.Value.Sid) + "\",\"ip\":\"" + JEsc(kv.Value.Ip) + "\",\"ua\":\"" + JEsc(kv.Value.Ua) + "\",\"super\":" + (kv.Value.Super ? "1" : "0") + ",\"me\":" + ((kv.Key == s.Token) ? "1" : "0") + ",\"idle\":" + idle.ToString() + "}");
 							}
 						}
 						sb.Append("]");
 						body = sb.ToString();
 					}
 					else
 					{
 						string target = ctx.Request.QueryString["target"] ?? "";
 						bool done = false;
 						lock (_sync)
 						{
 							// БЛОК 4, F7-11: target — это sid (короткий идентификатор) или,
 							// для совместимости, полный токен.
 							string realKey = null;
 							WebSession ts = null;
 							if (_tokens.ContainsKey(target))
 							{
 								realKey = target;
 								ts = _tokens[target];
 							}
 							else
 							{
 								foreach (System.Collections.Generic.KeyValuePair<string, WebSession> kv2 in _tokens)
 								{
 									if (kv2.Value.Sid == target)
 									{
 										realKey = kv2.Key;
 										ts = kv2.Value;
 										break;
 									}
 								}
 							}
 							if (ts != null && realKey != s.Token)
 							{
 								if (url == "/adminban")
 								{
 									if (ts.Ip != s.Ip)
 									{
 										_bans[ts.Ip] = DateTime.Now.AddMinutes(30);
 										SerialPortLog.Write("WEB: АДМИН - бан устройства " + ts.Ip + " на 30 минут");
 										_tokens.Remove(realKey);
 										done = true;
 									}
 								}
 								else
 								{
 									SerialPortLog.Write("WEB: АДМИН - кик сессии устройства " + ts.Ip);
 									_tokens.Remove(realKey);
 									done = true;
 								}
 							}
 						}
 						body = done ? "{\"ok\":1}" : "{\"ok\":0}";
 					}
 				}
                else if (url == "/panel")
 				{
 					body = PanelHtml();
 				}
				else if (url == "/operator")
				{
					body = OperatorHtml();
				}
				else
				{
					ctx.Response.StatusCode = 404;
					body = "not found";
				}
								byte[] buf = System.Text.Encoding.UTF8.GetBytes(body);
				ctx.Response.ContentType = ctype;
				// БЛОК 4, F7-09: единые заголовки безопасности для всех ответов.
				// БЛОК 4, F3-15: HTML-страницам разрешена ревалидация (no-cache — панель
				// после обновления страницы не «залипает» на старой копии), данные — no-store.
				bool isHtml = ctype.StartsWith("text/html");
				AddSecurityHeaders(ctx, isHtml);
				try
				{
					ctx.Response.AddHeader("Cache-Control", isHtml ? "no-cache, must-revalidate" : "no-store, must-revalidate");
					ctx.Response.AddHeader("Pragma", "no-cache");
				}
				catch
				{
				}
				ctx.Response.ContentLength64 = buf.Length;
				ctx.Response.OutputStream.Write(buf, 0, buf.Length);
				ctx.Response.OutputStream.Close();
			}
			catch
			{
			}
			finally
			{
				// БЛОК 3, F2-09: ответ закрывается ВСЕГДА, иначе клиент "висит" до таймаута
				try
				{
					ctx.Response.Close();
				}
				catch
				{
				}
			}
		}

		// ОП-7: загрузка страниц панелей из встроенных ресурсов WebUi\*.html.
		// Ресурс не найден/не читается — отказ НЕ молчит: запись в remote_com_log,
		// счётчик Diag и страница-заглушка с внятным текстом (вместо пустого ответа).
		private static string LoadWebResource(string fileName)
		{
			string fullName = "Tablo.WebUi." + fileName;
			try
			{
				System.Reflection.Assembly asm = typeof(WebServer).Assembly;
				using (System.IO.Stream st = asm.GetManifestResourceStream(fullName))
				{
					if (st == null)
					{
						SerialPortLog.Write("WEB: встроенный ресурс " + fullName + " не найден в сборке");
					}
					else
					{
						using (System.IO.StreamReader sr = new System.IO.StreamReader(st, System.Text.Encoding.UTF8))
						{
							return sr.ReadToEnd();
						}
					}
				}
			}
			catch (Exception ex)
			{
				SerialPortLog.Write("WEB: не удалось загрузить ресурс " + fullName + ": " + ex.Message);
				Diag.Note("WEB: ресурс " + fullName, ex);
			}
			return "<!DOCTYPE html><html><head><meta charset='utf-8'></head><body style='background:#000;color:#f66;font-family:monospace;padding:20px'>Ошибка панели: ресурс " + fullName + " не загружен. Подробности в remote_com_log.txt</body></html>";
		}

		// ОП-7 (аудит 2026-11): HTML страницы витрины /display вынесен во встроенный ресурс
		// WebUi\display.html (EmbeddedResource, см. Tablo.csproj). Правки верстки теперь делаются
		// в нормальном редакторе с подсветкой, а не вслепую внутри C#-строки (основной
		// источник регрессий UI). Загрузка — один раз в статический кэш; содержимое
		// байт-в-байт прежнее.
		private static string _displayCache;
		private static string DisplayHtml()
		{
			if (_displayCache == null)
			{
				_displayCache = LoadWebResource("display.html");
			}
			return _displayCache;
		}
		// ОП-7 (аудит 2026-11): HTML страницы мобильного пульта оператора вынесен во встроенный ресурс
		// WebUi\panel.html (EmbeddedResource, см. Tablo.csproj). Правки верстки теперь делаются
		// в нормальном редакторе с подсветкой, а не вслепую внутри C#-строки (основной
		// источник регрессий UI). Загрузка — один раз в статический кэш; содержимое
		// байт-в-байт прежнее.
		private static string _panelCache;
		private static string PanelHtml()
		{
			if (_panelCache == null)
			{
				_panelCache = LoadWebResource("panel.html");
			}
			return _panelCache;
		}
		// ОП-7 (аудит 2026-11): HTML страницы веб-панели оператора (вид с ПК) вынесен во встроенный ресурс
		// WebUi\operator.html (EmbeddedResource, см. Tablo.csproj). Правки верстки теперь делаются
		// в нормальном редакторе с подсветкой, а не вслепую внутри C#-строки (основной
		// источник регрессий UI). Загрузка — один раз в статический кэш; содержимое
		// байт-в-байт прежнее.
		private static string _operatorCache;
		private static string OperatorHtml()
		{
			if (_operatorCache == null)
			{
				_operatorCache = LoadWebResource("operator.html");
			}
			return _operatorCache;
		}
    }
	public static class ExtOut
	{
		public static Func<Tablo.TConfig> CfgProvider;
		public static Func<string> StateProvider;
		public static Action<TimeSpan> BeforeTotalSetter;
		public static Action<string, string> UiPush;
		private static System.Windows.Forms.Timer _timer;
		private static volatile SerialPort _port;   // БЛОК 3, F2-05: открывается в фоновом потоке
		private static System.Net.Sockets.UdpClient _udp;
		private static bool _netErrLogged = false;
		private static string _lastLine = "";
		private static int _signalTicks = 0;
		private static string _signalTarget = "-";
		private static int _prevBreakRem = -1;
		private static int _prevPreRem = -1;
		private static string _prevMode = "";
		private static string _lastBrkValue = "";
		private static int _openFailCooldown = 0;
		// БЛОК 7 (аудит), Б31: замок и «что сейчас нужно» — порт открывается в фоновом потоке,
		// а ClosePort() выполняется в UI-потоке.
		private static readonly object _portLock = new object();
		private static string _portWantName;
		private static int _portWantBaud;
		private static int _opening = 0;   // БЛОК 3, F2-05: 1 = прямо сейчас идёт фоновое открытие порта
		private static bool _parseErrLogged = false;
		public static string LastLine = "";

		public static void EnsureStarted()
		{
			if (_timer == null)
			{
				_timer = new System.Windows.Forms.Timer();
				_timer.Interval = 1000;
				_timer.Tick += Tick;
			}
			_timer.Start();
		}

		public static void StopAll()
		{
			if (_timer != null)
			{
				_timer.Stop();
			}
			ClosePort();
		}

		private static void ClosePort()
		{
			// БЛОК 7 (аудит), Б31: закрываем под тем же замком, что и присваивание _port
			// в фоновом потоке, и сбрасываем «что нужно открыть» — идущее открытие не примется.
			lock (_portLock)
			{
				_portWantName = null;
				if (_port != null)
				{
					try { _port.Close(); } catch { }
					try { _port.Dispose(); } catch { }
					_port = null;
				}
			}
		}

		private static bool CrossMark(int prev, int cur, int mark)
		{
			return prev >= 0 && cur >= 0 && mark > 0 && prev > mark && cur <= mark;
		}

		private static string ApplyTemplate(string tpl, string mode, string time, string sig, string to)
		{
			string s = (tpl == null || tpl.Trim().Length == 0) ? "EXT;{MODE};{TIME};{SIG};{TO}" : tpl;
			s = s.Replace("{MODE}", mode);
			s = s.Replace("{TIME}", time);
			s = s.Replace("{SIG}", sig);
			s = s.Replace("{TO}", to);
			return s;
		}

		private static int ParseTimeValue(string s)
		{
			try
			{
				string[] p = s.Split(new char[] { ':' });
				if (p.Length == 2)
				{
					return int.Parse(p[0]) * 60 + int.Parse(p[1]);
				}
				if (p.Length == 3)
				{
					return int.Parse(p[0]) * 3600 + int.Parse(p[1]) * 60 + int.Parse(p[2]);
				}
				return -1;
			}
			catch
			{
				return -1;
			}
		}

		private static int ParseMMSS(string s)
		{
			try
			{
				string t = (s == null) ? "" : s.Trim();
				int i = t.IndexOf(':');
				if (i <= 0 || i >= t.Length - 1)
				{
					return -1;
				}
				int mm = int.Parse(t.Substring(0, i));
				int ss = int.Parse(t.Substring(i + 1));
				if (ss < 0 || ss > 59)
				{
					return -1;
				}
				return mm * 60 + ss;
			}
			catch
			{
				return -1;
			}
		}

		private static void PushUi(string line, string svc)
		{
			try
			{
				if (UiPush != null)
				{
					UiPush(line, svc);
				}
			}
			catch
			{
			}
		}

		private static void Svc(string msg)
		{
			SerialPortLog.Write("ДОП.ВЫВОД: " + msg);
			PushUi(null, msg);
		}

		public static void TestFire(string target, string kind)
		{
			Fire(target, kind);
		}

		private static void Fire(string target, string kind)
		{
			_signalTicks = 3;
			_signalTarget = target;
			Svc("СИГНАЛ: " + kind + " -> " + target);
		}

		private static void Tick(object sender, EventArgs e)
		{
			try
			{
				if (CfgProvider == null || StateProvider == null)
				{
					return;
				}
				Tablo.TConfig cfg = CfgProvider();
				if (cfg == null)
				{
					return;
				}
				bool portWanted = cfg.ExtEnable && !string.IsNullOrEmpty(cfg.ExtPort);
				if (!portWanted)
				{
					ClosePort();
				}
				int baud = 9600;
				try
				{
					baud = int.Parse(cfg.ExtBaud);
				}
				catch
				{
				}
				if (portWanted && _port == null && _openFailCooldown > 0)
				{
					_openFailCooldown--;
				}
				else if (portWanted && (_port == null || _port.PortName != cfg.ExtPort || _port.BaudRate != baud))
				{
					ClosePort();
					// БЛОК 3, F2-05: SerialPort.Open() на "подвисшем" USB-COM может блокировать
					// на секунды, а Tick идёт на UI-потоке (интерфейс замерзал бы). Открываем В ФОНЕ;
					// Tick просто продолжит работу на следующем цикле, когда _port появится.
					if (System.Threading.Interlocked.CompareExchange(ref _opening, 1, 0) == 0)
					{
						// БЛОК 7 (аудит), Б31: запоминаем, что именно сейчас нужно открыть
						string pn = cfg.ExtPort;
						int bd = baud;
						_portWantName = pn;
						_portWantBaud = bd;
						System.Threading.ThreadPool.QueueUserWorkItem(delegate(object st)
						{
							SerialPort sp = null;
							try
							{
								sp = new SerialPort(pn, bd, Parity.None, 8, StopBits.One);
								sp.WriteTimeout = 500;
								sp.Open();
							}
							catch (Exception ex)
							{
								try
								{
									if (sp != null)
									{
										sp.Dispose();
									}
								}
								catch
								{
								}
								sp = null;
								System.Threading.Interlocked.Exchange(ref _openFailCooldown, 30);   // БЛОК 7 (аудит), Б31
								Svc("порт " + pn + " не открыт: " + ex.Message + ", повтор через 30 сек");
							}
							if (sp != null)
							{
								// БЛОК 7 (аудит), Б31: БЫЛО _port = sp из потока пула без проверки. Если ClosePort()
								// (смена порта / выключение доп. вывода / StopAll) успевал выполниться между sp.Open()
								// и присваиванием, свежий порт не закрывал НИКТО: COM-порт оставался занятым процессом
								// до перезапуска, а журнал показывал «порт открыт».
								bool taken = false;
								lock (_portLock)
								{
									if (_port == null && _portWantName == pn && _portWantBaud == bd)
									{
										_port = sp; sp = null; taken = true;
									}
								}
								if (taken)
								{
									System.Threading.Interlocked.Exchange(ref _openFailCooldown, 0);
									Svc("порт " + pn + " открыт (в фоне)");
								}
								else
								{
									try { sp.Close(); sp.Dispose(); } catch { }
									Svc("порт " + pn + " был открыт, но уже не нужен (настройки изменились / доп. вывод выключен) — закрыт");
								}
							}
							System.Threading.Interlocked.Exchange(ref _opening, 0);
						});
					}
				}
				string snap = StateProvider();
				// БЛОК 7 (аудит), Б32: БЫЛО parts[1] без проверки длины. Контракт «РЕЖИМ|ЗНАЧЕНИЕ»
				// между frmMain.Patch_ExternalSnapshot и ExtOut нигде не зафиксирован: стоило изменить
				// формат снимка — и Tick падал КАЖДУЮ СЕКУНДУ, внешний COM-порт и UDP переставали
				// обновляться, а журнал забивался «ошибка тика» (ротация 2 МБ x 3 затирала полезную
				// историю, включая записи про пульт).
				if (string.IsNullOrEmpty(snap)) { Svc("снимок состояния пуст — кадр не сформирован"); return; }
				string[] parts = snap.Split(new char[] { '|' });
				if (parts.Length < 2)
				{
					Svc("снимок состояния «" + snap + "» без разделителя '|' — кадр не сформирован");
					return;
				}
				string mode = parts[0];
				string value = parts[1];
				string sig = "0";
				string target = "-";
				if (mode != _prevMode)
				{
					Svc("ПЕРЕХОД РЕЖИМА: " + ((_prevMode == null || _prevMode == "") ? "(старт)" : _prevMode) + " -> " + mode + ", значение " + value);
					if (_prevMode == "BRK" && _lastBrkValue == "00:00")
					{
						Fire("A", "КОНЕЦ ПЕРЕРЫВА");
						sig = "1";
						target = "A";
					}
					_prevMode = mode;
					_prevBreakRem = -1;
					_prevPreRem = -1;
					_lastBrkValue = "";
					_parseErrLogged = false;
				}
				int remSec = -1;
				if (mode == "BRK" || mode == "PRE")
				{
					remSec = ParseTimeValue(value);
					if (remSec < 0 && !_parseErrLogged)
					{
						_parseErrLogged = true;
						Svc("не разобрал значение " + value + ", метки приостановлены до смены режима");
					}
				}
				if (mode == "BRK")
				{
					_lastBrkValue = value;
				}
				if (_signalTicks > 0)
				{
					_signalTicks--;
					sig = "1";
					target = _signalTarget;
				}
				else if (cfg.ExtSignals)
				{
					string fired = null;
					string kind = null;
					if (mode == "BRK")
					{
						int brk1 = ParseMMSS(cfg.BrkMark1);
						if (brk1 < 0)
						{
							brk1 = 180;
						}
						if (CrossMark(_prevBreakRem, remSec, brk1))
						{
							fired = "A";
							kind = "ПЕРЕРЫВ: за " + (brk1 / 60) + " мин до конца";
						}
						_prevBreakRem = remSec;
					}
					else if (mode == "PRE")
					{
						int[] pre = new int[4];
						pre[0] = ParseMMSS(cfg.PreMark1);
						if (pre[0] < 0)
						{
							pre[0] = 3600;
						}
						pre[1] = ParseMMSS(cfg.PreMark2);
						if (pre[1] < 0)
						{
							pre[1] = 2400;
						}
						pre[2] = ParseMMSS(cfg.PreMark3);
						if (pre[2] < 0)
						{
							pre[2] = 1200;
						}
						pre[3] = ParseMMSS(cfg.PreMark4);
						if (pre[3] < 0)
						{
							pre[3] = 120;
						}
						string[] tg = new string[] { "A", "A", "A", "A" };
						for (int i = 0; i < 4; i++)
						{
							if (CrossMark(_prevPreRem, remSec, pre[i]))
							{
								fired = tg[i];
								kind = "ПРЕДМАТЧ: метка " + (i + 1);
								break;
							}
						}
						_prevPreRem = remSec;
					}
					if (fired != null)
					{
						Fire(fired, kind + ", остаток " + value);
						sig = "1";
						target = fired;
					}
				}
				string line = ApplyTemplate(cfg.ExtFrameTemplate, mode, value, sig, target) + "\r";
				LastLine = line;
				if (line != _lastLine || sig == "1")
				{
					_lastLine = line;
					if (_port != null)
					{
						try
						{
							_port.Write(line);
						}
						catch (Exception ex)
						{
							Svc("ошибка записи в порт: " + ex.Message);
						}
					}
				}
				if (cfg.NetEnable)
				{
					try
					{
						if (_udp == null)
						{
							_udp = new System.Net.Sockets.UdpClient();
							_udp.EnableBroadcast = true;
						}
						int netPort = 5005;
						try
						{
							netPort = int.Parse(cfg.NetPort);
						}
						catch
						{
						}
						string host = string.IsNullOrEmpty(cfg.NetHost) ? "255.255.255.255" : cfg.NetHost;
						byte[] data = System.Text.Encoding.UTF8.GetBytes(line);
						_udp.Send(data, data.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), netPort));
						_netErrLogged = false;
					}
					catch (Exception ex)
					{
						if (!_netErrLogged)
						{
							_netErrLogged = true;
							Svc("ошибка сетевой отдачи: " + ex.Message);
						}
					}
				}
				else if (_udp != null)
				{
					try
					{
						_udp.Close();
					}
					catch
					{
					}
					_udp = null;
				}
				PushUi(line, null);
			}
			catch (Exception ex)
			{
				Svc("ошибка тика: " + ex.Message);
			}
		}
	}

	public class ExtOutUiForm : Form
	{
		private Label _value;
		// БЛОК 7 (аудит), Б33: кэш шрифта. БЫЛО: LayoutValue() на каждый пуш (1 раз в секунду)
		// и на каждый ресайз создавал НОВЫЙ Font и присваивал его Label.Font, а прежний Font
		// не освобождался -> утечка GDI-дескрипторов (HFONT). При лимите 10000 GDI-объектов
		// на процесс окно, открытое на время матчей, исчерпывало лимит примерно за 2.8 часа,
		// после чего отрисовка начинала падать ВО ВСЁМ приложении (включая полотно табло).
		private Font _cachedFont;
		private float _cachedFontSize;

		public ExtOutUiForm()
		{
			this.Text = "Доп. табло (Ctrl+F8)";
			this.Width = 480;
			this.Height = 240;
			this.MinimumSize = new Size(200, 120);
			this.StartPosition = FormStartPosition.CenterScreen;
			this.BackColor = Color.Black;
			this._value = new Label();
			this._value.Text = "--:--";
			this._value.ForeColor = Color.White;
			this._value.AutoSize = true;
			this._cachedFont = UiFonts.Mono(40f, FontStyle.Bold);
			this._cachedFontSize = 40f;
			this._value.Font = this._cachedFont;
			this.Controls.Add(this._value);
			this.Resize += delegate
			{
				this.LayoutValue();
			};
			ExtOut.UiPush += this.OnPush;
			this.FormClosed += delegate
			{
				ExtOut.UiPush -= this.OnPush;
				// БЛОК 7 (аудит), Б33: освобождаем кэшированный шрифт
				if (this._cachedFont != null)
				{
					try { this._cachedFont.Dispose(); } catch { }
					this._cachedFont = null;
				}
			};
			this.LayoutValue();
		}

		private void OnPush(string line, string svc)
		{
			if (this.IsDisposed || !this.IsHandleCreated)
			{
				return;   // БЛОК 7 (аудит): окно уже закрыто — InvokeRequired бросил бы ObjectDisposedException
			}
			if (this.InvokeRequired)
			{
				try
				{
					this.BeginInvoke(new Action<string, string>(this.OnPush), new object[] { line, svc });
				}
				catch
				{
				}
				return;
			}
			if (line != null)
			{
				string[] p = line.TrimEnd(new char[] { '\r' }).Split(new char[] { ';' });
				if (p.Length >= 3)
				{
					this._value.Text = p[2];
					this.LayoutValue();
				}
			}
		}

		private void LayoutValue()
		{
			float size = Math.Min(this.ClientSize.Width / 7f, this.ClientSize.Height / 2.2f);
			if (size < 12f)
			{
				size = 12f;
			}
			// БЛОК 7 (аудит), Б33: шрифт пересоздаётся ТОЛЬКО при изменении размера,
			// прежний Font освобождается. Повторные LayoutValue() без изменения размера
			// (это каждый пуш раз в секунду) больше не создают GDI-объектов.
			if (this._cachedFont == null || Math.Abs(size - this._cachedFontSize) > 0.01f)
			{
				Font old = this._cachedFont;
				this._cachedFont = UiFonts.Mono(size, FontStyle.Bold);
				this._cachedFontSize = size;
				this._value.Font = this._cachedFont;
				if (old != null)
				{
					try { old.Dispose(); } catch { }
				}
			}
			this._value.Location = new Point(Math.Max(0, (this.ClientSize.Width - this._value.Width) / 2), Math.Max(0, (this.ClientSize.Height - this._value.Height) / 2));
		}
	}

	public static class UiFonts
	{
		private static string _mono;

		public static string MonoName
		{
			get
			{
				if (_mono == null)
				{
					_mono = "Courier New";
					try
					{
						foreach (System.Drawing.FontFamily f in new System.Drawing.Text.InstalledFontCollection().Families)
						{
							if (f.Name == "Consolas")
							{
								_mono = "Consolas";
								break;
							}
						}
					}
					catch
					{
					}
				}
				return _mono;
			}
		}

		public static Font Mono(float size, FontStyle style)
		{
			return new Font(MonoName, size, style);
		}
	}

	public static class ExtOutUi
	{
		private static ExtOutUiForm _form;

		public static void Open()
		{
			if (_form == null || _form.IsDisposed)
			{
				_form = new ExtOutUiForm();
				_form.Show();
			}
			else
			{
				_form.BringToFront();
			}
		}
	}

	// =====================================================================
	// БЛОК 7 (аудит), Б25/М3: ФОНОВАЯ ЗАПИСЬ ФАЙЛОВ
	// =====================================================================
	// Зачем: config.xml писался с FileOptions.WriteThrough + FlushFileBuffers (физический
	// сброс на диск) ПРЯМО В UI-ПОТОКЕ — в том числе по команде пульта (SetBreak/SetTimeout/
	// SetTimerGame) и каждые 2 с при автосохранении match_state.xml. На HDD это 2-15 мс,
	// на USB-флешке 20-100 мс, при активном антивирусе до 200 мс. Всё это время тик игровых
	// часов (10 мс) не перерисовывался: секунды на табло ЗАМИРАЛИ, а команды пульта,
	// пришедшие в этот момент, выполнялись с задержкой.
	//
	// Как работает: сериализация/формирование содержимого остаётся в UI-потоке (это
	// доли миллисекунды), а сам ввод-вывод уходит в один фоновый поток. Для каждого пути
	// хранится только ПОСЛЕДНЯЯ версия данных («последняя запись побеждает») — промежуточные
	// состояния файла не интересуют ни программу, ни внешних потребителей.
	// Запись остаётся АТОМАРНОЙ: tmp -> File.Replace (на FAT32 откат к Delete+Move).
	// Перед выходом из программы вызывается FlushAll() — всё, что поставлено в очередь,
	// гарантированно оказывается на диске.
	public static class AsyncFileWriter
	{
		private sealed class Item
		{
			public string Path;
			public byte[] Data;
			public bool FlushToDisk;
			public string BackupPath;
		}

		private static readonly object _lock = new object();
		private static readonly System.Collections.Generic.Dictionary<string, Item> _pending =
			new System.Collections.Generic.Dictionary<string, Item>();
		private static System.Threading.AutoResetEvent _signal;
		private static System.Threading.Thread _thread;
		private static volatile bool _stop;
		private static string _lastError;
		// ОП-8 (аудит 2026-11): момент последней ошибки записи (Ticks), 0 = ошибок нет.
		// Обновляется через Interlocked — поток UI безопасно читает возраст ошибки.
		private static long _lastErrorTicks;

		[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FlushFileBuffers(System.Runtime.InteropServices.SafeHandle hFile);

		public static string LastError { get { return _lastError; } }

		// ОП-8 (аудит 2026-11): возраст (сек) последней ошибки фоновой записи; -1 = ошибок нет.
		// Потребители: state.json (поле io_error) и статус-строка (webCacheTimer в frmMain).
		public static int LastErrorAgeSec
		{
			get
			{
				long t = System.Threading.Interlocked.Read(ref _lastErrorTicks);
				if (t == 0L)
				{
					return -1;
				}
				return (int)((DateTime.Now.Ticks - t) / 10000000L);
			}
		}

		public static void Enqueue(string path, byte[] data, bool flushToDisk, string backupPath)
		{
			if (string.IsNullOrEmpty(path) || data == null) { return; }
			lock (_lock)
			{
				Item it = new Item();
				it.Path = path; it.Data = data; it.FlushToDisk = flushToDisk; it.BackupPath = backupPath;
				_pending[path] = it;
				if (_signal == null) { _signal = new System.Threading.AutoResetEvent(false); }
				if (_thread == null)
				{
					_stop = false;
					_thread = new System.Threading.Thread(new System.Threading.ThreadStart(Loop));
					_thread.IsBackground = true;
					_thread.Name = "TabloAsyncFileWriter";
					_thread.Start();
				}
			}
			try { _signal.Set(); } catch { }
		}

		// Ждать, пока очередь опустеет. Возвращает true, если всё записано.
		public static bool FlushAll(int timeoutMs)
		{
			int waited = 0;
			while (waited < timeoutMs)
			{
				lock (_lock) { if (_pending.Count == 0) { return true; } }
				System.Threading.Thread.Sleep(20);
				waited += 20;
			}
			lock (_lock) { return _pending.Count == 0; }
		}

		public static void Shutdown()
		{
			FlushAll(3000);
			_stop = true;
			try { if (_signal != null) { _signal.Set(); } } catch { }
		}

		private static void Loop()
		{
			while (true)
			{
				try { _signal.WaitOne(250, false); } catch { }
				System.Collections.Generic.List<Item> batch;
				lock (_lock)
				{
					if (_pending.Count == 0)
					{
						if (_stop) { return; }
						continue;
					}
					batch = new System.Collections.Generic.List<Item>(_pending.Values);
					_pending.Clear();
				}
				for (int i = 0; i < batch.Count; i++) { WriteOne(batch[i]); }
				if (_stop)
				{
					lock (_lock) { if (_pending.Count == 0) { return; } }
				}
			}
		}

		private static void WriteOne(Item it)
		{
			string tmp = it.Path + ".tmp";
			try
			{
				using (System.IO.FileStream fs = new System.IO.FileStream(tmp, System.IO.FileMode.Create,
						System.IO.FileAccess.Write, System.IO.FileShare.None, 4096,
						it.FlushToDisk ? System.IO.FileOptions.WriteThrough : System.IO.FileOptions.None))
				{
					fs.Write(it.Data, 0, it.Data.Length);
					fs.Flush();
					if (it.FlushToDisk)
					{
						try { FlushFileBuffers(fs.SafeFileHandle); } catch { }
					}
				}
				if (System.IO.File.Exists(it.Path))
				{
					bool replaced = false;
					try
					{
						System.IO.File.Replace(tmp, it.Path, it.BackupPath, true);
						replaced = true;
					}
					catch { }
					if (!replaced)
					{
						System.IO.File.Delete(it.Path);
						System.IO.File.Move(tmp, it.Path);
					}
				}
				else
				{
					System.IO.File.Move(tmp, it.Path);
				}
				_lastError = null;
				System.Threading.Interlocked.Exchange(ref _lastErrorTicks, 0L);   // ОП-8: ошибка снята
			}
			catch (Exception ex)
			{
				_lastError = it.Path + ": " + ex.Message;
				System.Threading.Interlocked.Exchange(ref _lastErrorTicks, DateTime.Now.Ticks);   // ОП-8: запомнили момент ошибки
				try { if (System.IO.File.Exists(tmp)) { System.IO.File.Delete(tmp); } } catch { }
				CrashLog.Append("WRITE", "фоновая запись " + it.Path + " не удалась: " + ex.Message);
				Diag.Note("AsyncFileWriter " + System.IO.Path.GetFileName(it.Path), ex);
			}
		}
	}

	// =====================================================================
	// БЛОК 7 (аудит), Б16: ЕДИНАЯ точка развёртки короткого имени звукового файла
	// =====================================================================
	// Короткое имя («sirena.wav») разворачивается в полный путь к папке программы.
	// БЫЛО: frmMain это делал (Patch_AudioPath), а SettingForm передавал в PrepareSound
	//       короткое имя как есть — File.Exists резолвил его от ТЕКУЩЕГО КАТАЛОГА ПРОЦЕССА.
	//       При запуске из автозагрузки (HKCU\...\Run) рабочий каталог = C:\Windows\System32,
	//       поэтому кнопки «Прослушать» молчали, хотя в матче звук работал.
	public static class Paths
	{
		public static string Audio(string f)
		{
			try
			{
				if (string.IsNullOrEmpty(f)) { return f; }
				if (System.IO.Path.IsPathRooted(f)) { return f; }
				return System.IO.Path.Combine(Application.StartupPath, f);
			}
			catch { return f; }
		}
	}

	// =====================================================================
	// БЛОК 7 (аудит), Б12/Б13 (сборка 5): СИНТЕЗ ЗВУКА УДАЛЁН ПОЛНОСТЬЮ
	// =====================================================================
	// По решению пользователя резервный синтезированный звук удалён: на слух он
	// не похож на сирену. Здесь больше НЕТ класса SynthSound: ни генерации PCM,
	// ни неуправляемых буферов (Marshal.AllocHGlobal), ни DllImport winmm.PlaySound.
	// Звук играется ТОЛЬКО из файлов (TAudioPlayer / DirectShow). Если файла нет или
	// он не готовится - табло честно пишет причину в журнал и остаётся тихим
	// (см. frmMain.Patch_PrepareAudio / Patch_PlayAudio).

    }
