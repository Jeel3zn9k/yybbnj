using System;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace Tablo
{
	// Token: 0x02000020 RID: 32
	[Serializable]
	public class TConfig : ICloneable
	{
				// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600015E RID: 350 RVA: 0x00015A40 File Offset: 0x00013C40
				[XmlIgnore]
		public static TConfig Instance
		{
			get
			{
				lock (TConfig.lockFlag)
				{
					// О-2 (аудит 2026-11): БЫЛО — каждое обращение к TConfig.Instance обнуляло кэш
					// и заново читало config.xml с диска (несколько десериализаций в секунду прямо
					// во время матча). СТАЛО — экземпляр кэшируется; повторное чтение с диска только
					// после Reload() (его вызывает сохранение настроек) или при первом обращении.
					// Логика хоккейного матча не меняется — меняется только источник чтения полей.
					if (TConfig._instance != null)
					{
						return TConfig._instance;
					}
					string newPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
					string oldPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\HockeyBoard\\config.xml";
					if (!File.Exists(newPath) && File.Exists(oldPath))
					{
						try
						{
							File.Copy(oldPath, newPath);
						}
						catch
						{
						}
					}
										bool cfgMissing = !File.Exists(newPath);
					try
					{
						using (FileStream fileStream = new FileStream(newPath, FileMode.Open))
						{
							// БЛОК 4, F9-01: XmlSerializer создаётся ОДИН раз на процесс
							TConfig._instance = (TConfig)TConfig.Serializer.Deserialize(fileStream);
						}
					}
					catch
					{
						if (cfgMissing)
						{
							// БЛОК 1c: файла просто нет (первый запуск) - это штатная ситуация, а не повреждение.
							// Раньше в лог все равно писалась страшилка про config.xml.bad, которого не существует.
							try
							{
								TabloPatch.CrashLog.Append("CONFIG", newPath + " не найден (первый запуск) - создан новый с заводскими значениями");
							}
							catch
							{
							}
						}
						else
						{
							string badPath = null;
							try
							{
								// БЛОК 4, F8-09: ПЕРВЫЙ повреждённый образец не затирается
								// последующими — для диагностики ценен именно он.
								badPath = newPath + ".bad";
								if (File.Exists(badPath))
								{
									badPath = null;
								}
								else
								{
									File.Copy(newPath, badPath, false);
								}
							}
							catch
							{
								badPath = null;
							}
							try
							{
								TabloPatch.CrashLog.Append("CONFIG", newPath + " повреждён или недоступен - загружены заводские значения; " + ((badPath != null) ? ("копия: " + badPath) : (File.Exists(newPath + ".bad") ? "образец config.xml.bad уже есть - сохранён ПЕРВЫЙ повреждённый файл" : "копию config.xml.bad создать НЕ удалось (нет доступа к папке)")));
							}
							catch
							{
							}
						}
						TConfig._instance = new TConfig();
					}
					if (TConfig._instance != null && !TConfig._instance.MigratedV2)
					{
						TConfig._instance._StrokeGame = false;
						TConfig._instance._StrokeBeforeStart = false;
						TConfig._instance._BaudRate = "115200";
						TConfig._instance.MigratedV2 = true;
						TConfig._instance.Save();
					}
					if (TConfig._instance != null && !TConfig._instance.MigratedV3)
					{
						// исправляем «заводской мусор» ПРЕЖНИХ версий, не трогая пользовательские значения
						if (TConfig._instance._GameTime == 60) TConfig._instance._GameTime = 1200;   // 1 мин -> 20 мин
						if (TConfig._instance._BreakTime == 10) TConfig._instance._BreakTime = 900;   // 10 с -> 15 мин
						if (string.IsNullOrEmpty(TConfig._instance._FontFile)) TConfig._instance._FontFile = "16fs_new.fdat";
						TConfig._instance.MigratedV3 = true;
						TConfig._instance.Save();
					}
					if (TConfig._instance != null && !TConfig._instance.MigratedV4)
					{
						// БЛОК 1c (решения заказчика от 14.10.2026): новые заводские значения.
						// Пустые/старозаводские поля заполняются, пользовательские НЕ затираются.
						if (string.IsNullOrEmpty(TConfig._instance._BeepFile)) TConfig._instance._BeepFile = "sirena.wav";
						if (string.IsNullOrEmpty(TConfig._instance._StartGameBeepFile)) TConfig._instance._StartGameBeepFile = "sirena.wav";
						if (string.IsNullOrEmpty(TConfig._instance._WhistleBeepFile)) TConfig._instance._WhistleBeepFile = "sirena.wav";
						// GetGateBeepFile (звук взятия ворот) - НЕ трогаем по указанию заказчика.
						TConfig._instance.AutoWhistleEnable = true;   // авто-свисток по окончании периода (БЛОК 3b: было "1" — FormatException в Convert.ToBoolean)
						if (TConfig._instance._TimeBeforeFlash == 60) TConfig._instance._TimeBeforeFlash = 120;   // удаление по умолчанию 02:00
						if (string.IsNullOrEmpty(TConfig._instance._Command_A)) TConfig._instance._Command_A = "ТЕСТ";
						if (string.IsNullOrEmpty(TConfig._instance._TownCommand_A)) TConfig._instance._TownCommand_A = "ТЕСТ";
						if (string.IsNullOrEmpty(TConfig._instance._Command_B)) TConfig._instance._Command_B = "ТЕСТ";
						if (string.IsNullOrEmpty(TConfig._instance._TownCommand_B)) TConfig._instance._TownCommand_B = "ТЕСТ";
						TConfig._instance.MigratedV4 = true;
						TConfig._instance.Save();
					}
					// ===== БЛОК 3b: самолечение логических полей config.xml =====
					// После ошибочной миграции в файле могло остаться AutoWhistleEnable="1" —
					// такой файл валил программу при КАЖДОМ старте (FormatException).
					// Разово приводим все Convert.ToBoolean-поля к виду "True"/"False"
					// и сохраняем файл. Поведение программы при этом не меняется.
					if (TConfig._instance != null)
					{
						bool healed = false;
						healed |= NormalizeBoolField("TopWindow", ref TConfig._instance._TopWindow, true);
						healed |= NormalizeBoolField("AutoStart", ref TConfig._instance._AutoStart, false);
						healed |= NormalizeBoolField("TimeoutVideoEnable", ref TConfig._instance._TimeoutVideoEnable, false);
						healed |= NormalizeBoolField("BreakVideoEnable", ref TConfig._instance._BreakVideoEnable, false);
						healed |= NormalizeBoolField("AutoWhistleEnable", ref TConfig._instance._AutoWhistleEnable, true);
						healed |= NormalizeBoolField("WhistleMode", ref TConfig._instance._WhistleMode, true);
						if (healed)
						{
							TConfig._instance.Save();
						}
					}
				}
				return TConfig._instance;
			}
		}

		// БЛОК 4, F9-01: один общий XmlSerializer (его генерация — дорогая операция;
		// экземпляр потокобезопасен для Serialize/Deserialize).
		private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(TConfig));

		private static XmlSerializer Serializer
		{
			get
			{
				return TConfig._serializer;
			}
		}

		// ===== БЛОК 3b: толерантный разбор логических значений =====
		// Convert.ToBoolean принимает только строки "True"/"False" и бросает
		// FormatException на "1"/"0"/пустой строке — из-за этого программа падала
		// при старте. Теперь распознаются и "1"/"0" (и прочие понятные человеку
		// варианты), а нераспознанное значение заменяется заводским без падения.
		private static bool ParseBoolSafe(string s, bool def)
		{
			if (s == null)
			{
				return def;
			}
			string t = s.Trim().ToLowerInvariant();
			if (t.Length == 0)
			{
				return def;
			}
			if (t == "true" || t == "1" || t == "yes" || t == "on" || t == "да")
			{
				return true;
			}
			if (t == "false" || t == "0" || t == "no" || t == "off" || t == "нет")
			{
				return false;
			}
			return def;
		}

		// БЛОК 3b: разовое приведение логического поля config.xml к каноническому
		// виду "True"/"False" с записью в crash_log.txt. Возвращает true, если поле
		// изменено (файл надо пересохранить).
		private static bool NormalizeBoolField(string name, ref string field, bool def)
		{
			string canonical = ParseBoolSafe(field, def) ? "True" : "False";
			if (field == canonical)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(field))
			{
				try
				{
					TabloPatch.CrashLog.Append("CONFIG", "значение поля " + name + " [«" + field + "»] не является корректным логическим - заменено на [«" + canonical + "»]");
				}
				catch
				{
				}
			}
			field = canonical;
			return true;
		}

		// Token: 0x0600015F RID: 351 RVA: 0x000029B1 File Offset: 0x00000BB1
		public static void Reload()
		{
			TConfig._instance = null;
		}

		// Token: 0x06000160 RID: 352 RVA: 0x00015AEC File Offset: 0x00013CEC
		// БЛОК 7 (аудит): Save() теперь возвращает результат. БЫЛО: любая ошибка записи
		// глоталась (только строка в crash_log), и вызывающий код — в том числе команды
		// пульта и «ОК» в настройках — считал сохранение успешным. См. также П9 аудита.
				 	public bool Save()
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
			try
			{
				string dir = Path.GetDirectoryName(path);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
				// ===== БЛОК 2, F8-02 + F1-14: АТОМАРНОЕ сохранение config.xml =====
				// Было: Flush только в кэш ОС, затем File.Delete(config.xml) и File.Move —
				// обрыв питания в этом окне оставлял программу БЕЗ настроек; плюс файл
				// переписывался при каждом закрытии и каждой веб-команде.
				// Стало: 1) сериализация в память; 2) если XML не изменился — диск не трогаем;
				// 3) tmp с WriteThrough + Flush(true) (данные физически на диске);
				// 4) атомарная замена File.Replace с копией в config.xml.bak
				//    (на FAT32, где Replace недоступен, — откат к Delete+Move).
				byte[] data;
				using (MemoryStream ms = new MemoryStream())
				{
					TConfig.Serializer.Serialize(ms, this);   // БЛОК 4, F9-01
					data = ms.ToArray();
				}
				string xml = new System.Text.UTF8Encoding(false).GetString(data);
				if (xml == this._lastWrittenXml && File.Exists(path))
				{
					return true;
				}
				// ===== БЛОК 7 (аудит), Б25: ЗАПИСЬ УШЛА В ФОНОВЫЙ ПОТОК =====
				// БЫЛО: tmp с FileOptions.WriteThrough + FlushFileBuffers (физический сброс на
				//       диск) + File.Replace с копией в .bak — ВСЁ ЭТО В UI-ПОТОКЕ, в том числе
				//       по команде пульта (SetBreak/SetTimeout/SetTimerGame) и каждые 2 с при
				//       автосохранении матча. На HDD 2-15 мс, на USB-флешке 20-100 мс, при
				//       активном антивирусе до 200 мс. В это время тик часов (10 мс) не
				//       перерисовывался — СЕКУНДЫ НА ТАБЛО ЗАМИРАЛИ.
				// СТАЛО: сериализация в память остаётся здесь (доли мс), ввод-вывод выполняет
				//       один фоновый поток; атомарность (tmp -> File.Replace -> .bak) сохранена.
				//       Перед выходом из программы вызывается TConfig.FlushNow().
				TabloPatch.AsyncFileWriter.Enqueue(path, data, true, path + ".bak");
				this._lastWrittenXml = xml;
				return true;
			}
			catch (Exception ex)
			{
				try
				{
					TabloPatch.CrashLog.Append("CONFIG SAVE", ex.Message);
				}
				catch
				{
				}
			}
			return false;
		}

		// БЛОК 7 (аудит), Б25: дождаться завершения фоновой записи (вызывать при выходе).
		// Возвращает false, если что-то не успело записаться за 3 с.
		public static bool FlushNow()
		{
			bool ok = TabloPatch.AsyncFileWriter.FlushAll(3000);
			if (!ok)
			{
				try { TabloPatch.CrashLog.Append("CONFIG SAVE", "фоновая запись не завершилась за 3 с: " + TabloPatch.AsyncFileWriter.LastError); } catch { }
			}
			return ok;
		}

		// БЛОК 2, F8-02: последний записанный на диск XML — чтобы не переписывать config.xml,
		// когда ничего не изменилось. Поле private — в сериализации НЕ участвует.
		private string _lastWrittenXml;

		// БЛОК 7 (аудит), Б25: FlushFileBuffers/Patch_FlushToDisk переехали в
		// TabloPatch.AsyncFileWriter (запись теперь фоновая) — отсюда удалены как мёртвые.

		// Token: 0x06000161 RID: 353 RVA: 0x00015B74 File Offset: 0x00013D74
		public object Clone()
		{
			TConfig tconfig = new TConfig();
			tconfig.BackColor = this.BackColor;
			tconfig.PromptColor = this.PromptColor;
			tconfig.Left = this.Left;
			tconfig.Top = this.Top;
			tconfig.TopWindow = this.TopWindow;
			tconfig.AutoStart = this.AutoStart;
			tconfig.Port = this.Port;
			tconfig.BaudRate = this.BaudRate;
			tconfig.Parity = this.Parity;
			tconfig.DataBit = this.DataBit;
			tconfig.StopBits = this.StopBits;
			tconfig.FontFile = this.FontFile;
			tconfig.BeforeStartTextColor = this.BeforeStartTextColor;
			tconfig.TimeBeforeStart = this.TimeBeforeStart;
			tconfig.TimeBeforeFlash = this.TimeBeforeFlash;
			tconfig.TimeFirstBeep = this.TimeFirstBeep;
			tconfig.TimeSecondBeep = this.TimeSecondBeep;
			tconfig.ForwardStrokeBeforeStart = this.ForwardStrokeBeforeStart;
			tconfig.BeepFile = this.BeepFile;
			tconfig.TimeoutTime = this.TimeoutTime;
			tconfig.TimeoutVideoEnable = this.TimeoutVideoEnable;
			tconfig.BreakTime = this.BreakTime;
			tconfig.BreakVideoEnable = this.BreakVideoEnable;
			tconfig.FontColor = this.FontColor;
			tconfig.GameTime = this.GameTime;
			tconfig.ForwardStrokeGame = this.ForwardStrokeGame;
			tconfig.StartGameBeepFile = this.StartGameBeepFile;
			tconfig.GetGateBeepFile = this.GetGateBeepFile;
			tconfig.WhistleBeepFile = this.WhistleBeepFile;
			tconfig.AutoWhistleEnable = this.AutoWhistleEnable;
			tconfig.WhistleMode = this.WhistleMode;
			tconfig._ScriptPlaylistPath = this._ScriptPlaylistPath;
			tconfig._MediaPlaylistPath = this._MediaPlaylistPath;
			tconfig.Command_A = this.Command_A;
			tconfig.TownCommand_A = this.TownCommand_A;
			tconfig.Command_B = this.Command_B;
			tconfig.TownCommand_B = this.TownCommand_B;
			tconfig._MigratedV2 = this._MigratedV2;
			tconfig._MigratedV3 = this._MigratedV3;
			tconfig._MigratedV4 = this._MigratedV4;
			tconfig._RemoteLogEnable = this._RemoteLogEnable;
			tconfig._ExtEnable = this._ExtEnable;
			tconfig._ExtSignals = this._ExtSignals;
			tconfig._ExtPort = this._ExtPort;
			tconfig._ExtBaud = this._ExtBaud;
			tconfig._MatchStart = this._MatchStart;
			tconfig._PreMark1 = this._PreMark1;
			tconfig._PreMark2 = this._PreMark2;
			tconfig._PreMark3 = this._PreMark3;
			tconfig._PreMark4 = this._PreMark4;
			tconfig._BrkMark1 = this._BrkMark1;
			tconfig._NetEnable = this._NetEnable;
			tconfig._NetHost = this._NetHost;
			tconfig._NetPort = this._NetPort;
			tconfig._WebPassHash = this._WebPassHash;
						tconfig._WebPort = this._WebPort;
						
			tconfig._WebBindAll = this._WebBindAll;
			tconfig._WebUseHttps = this._WebUseHttps;
			tconfig._WebIp = this._WebIp;
			tconfig._DevIdleMinutes = this._DevIdleMinutes;
			tconfig._DevPassHash = this._DevPassHash;
			tconfig._WebEnable = this._WebEnable;
			tconfig._WebEnableToken = this._WebEnableToken;
			tconfig._ExtFrameTemplate = this._ExtFrameTemplate;
			// О-2 (аудит 2026-11): Clone() больше НЕ подменяет глобальный синглтон — раньше
			// любой клон становился TConfig.Instance, и последующие чтения могли получать копию
			// с несохранёнными правками. Теперь Clone() — чистая копия без побочного эффекта.
			return tconfig;
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00015D4C File Offset: 0x00013F4C
		public TConfig()
		{
			this.BackColor = Color.Black;
			this.PromptColor = Color.White;
			this.Left = 0;
			this.Top = 0;
			this.TopWindow = true;
			this.AutoStart = false;
			this.Port = "COM1";
			this.BaudRate = "115200";
			this.Parity = "None";
			this.DataBit = "8";
			this.StopBits = "One";
			this.FontFile = "16fs_new.fdat";
			this.BeforeStartTextColor = Color.White;
			this.TimeBeforeStart = TimeSpan.FromMinutes(1.0);
			this.TimeBeforeFlash = TimeSpan.FromMinutes(2.0);   // БЛОК 1c: удаление по умолчанию 02:00 (его использует Add_del)
			this.TimeFirstBeep = TimeSpan.FromMinutes(1.0);
			this.TimeSecondBeep = TimeSpan.FromMinutes(1.0);
			this.ForwardStrokeBeforeStart = false;
			this.BeepFile = "sirena.wav";   // БЛОК 1c: звук по умолчанию (решение заказчика)
			this.TimeoutTime = TimeSpan.FromSeconds(30.0);
			this.TimeoutVideoEnable = false;
			this.BreakTime = TimeSpan.FromMinutes(15.0);
			this.BreakVideoEnable = false;
			this.FontColor = Color.White;
			this.GameTime = TimeSpan.FromMinutes(20.0);
			this.ForwardStrokeGame = false;
			this.StartGameBeepFile = "sirena.wav";   // БЛОК 1c
			this.GetGateBeepFile = string.Empty;
			this.WhistleBeepFile = "sirena.wav";   // БЛОК 1c: сирена конца периода
			this.AutoWhistleEnable = true;   // БЛОК 1c: авто-свисток по окончании периода ВКЛ по умолчанию
			this.WhistleMode = true;
			this._ScriptPlaylistPath = string.Empty;
			this._MediaPlaylistPath = string.Empty;
			this.Command_A = "ТЕСТ";   // БЛОК 1c
			this.TownCommand_A = "ТЕСТ";   // БЛОК 1c
			this.Command_B = "ТЕСТ";   // БЛОК 1c
			this.TownCommand_B = "ТЕСТ";   // БЛОК 1c
			// ===== БЛОК 3b: явные заводские значения «веб/разработческих» полей =====
			// Раньше эти поля оставались null, а XmlSerializer не записывает null в
			// config.xml — из-за этого в новом файле «пропадали» строки, которые были
			// в старых резервных копиях (WebEnable, WebIp, DevPassHash и т.д.).
			// Значения совпадают с запасными значениями геттеров — поведение не меняется.
			this._RemoteLogEnable = "1";         // удалённый лог COM включён (геттер: != "0")
			this._ExtPort = string.Empty;        // дополнительный COM-порт не задан
			this._WebPassHash = string.Empty;    // пароль веб-интерфейса не задан
			this._WebBindAll = "0";              // веб-сервер только на localhost
			this._WebUseHttps = "0";             // без HTTPS
			this._WebIp = string.Empty;          // IP определяется автоматически
			this._DevPassHash = string.Empty;    // пароль режима разработчика не задан
			this._WebEnable = "0";               // веб-интерфейс выключен
			this._WebEnableToken = string.Empty; // подпись включения пуста
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000163 RID: 355 RVA: 0x000029B9 File Offset: 0x00000BB9
		// (set) Token: 0x06000164 RID: 356 RVA: 0x000029C6 File Offset: 0x00000BC6
		[XmlIgnore]
		public Color BackColor
		{
			get
			{
				return Color.FromArgb(this._BackColor);
			}
			set
			{
				this._BackColor = value.ToArgb();
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000165 RID: 357 RVA: 0x000029D5 File Offset: 0x00000BD5
		// (set) Token: 0x06000166 RID: 358 RVA: 0x000029E2 File Offset: 0x00000BE2
		[XmlIgnore]
		public Color PromptColor
		{
			get
			{
				return Color.FromArgb(this._PromptColor);
			}
			set
			{
				this._PromptColor = value.ToArgb();
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000167 RID: 359 RVA: 0x000029F1 File Offset: 0x00000BF1
		// (set) Token: 0x06000168 RID: 360 RVA: 0x000029F9 File Offset: 0x00000BF9
		[XmlIgnore]
		public int Left
		{
			get
			{
				return this._Left;
			}
			set
			{
				this._Left = value;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000169 RID: 361 RVA: 0x00002A02 File Offset: 0x00000C02
		// (set) Token: 0x0600016A RID: 362 RVA: 0x00002A0A File Offset: 0x00000C0A
		[XmlIgnore]
		public int Top
		{
			get
			{
				return this._Top;
			}
			set
			{
				this._Top = value;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600016B RID: 363 RVA: 0x00002A13 File Offset: 0x00000C13
		// (set) Token: 0x0600016C RID: 364 RVA: 0x00002A20 File Offset: 0x00000C20
		[XmlIgnore]
		public bool TopWindow
		{
			get
			{
				return ParseBoolSafe(this._TopWindow, true);   // БЛОК 3b
			}
			set
			{
				this._TopWindow = value.ToString();
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600016D RID: 365 RVA: 0x00002A2F File Offset: 0x00000C2F
		// (set) Token: 0x0600016E RID: 366 RVA: 0x00002A3C File Offset: 0x00000C3C
		[XmlIgnore]
		public bool AutoStart
		{
			get
			{
				return ParseBoolSafe(this._AutoStart, false);   // БЛОК 3b
			}
			set
			{
				this._AutoStart = value.ToString();
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600016F RID: 367 RVA: 0x00002A4B File Offset: 0x00000C4B
		// (set) Token: 0x06000170 RID: 368 RVA: 0x00002A53 File Offset: 0x00000C53
		[XmlIgnore]
		public string Port
		{
			get
			{
				return this._Port;
			}
			set
			{
				this._Port = value;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000171 RID: 369 RVA: 0x00002A5C File Offset: 0x00000C5C
		// (set) Token: 0x06000172 RID: 370 RVA: 0x00002A64 File Offset: 0x00000C64
		[XmlIgnore]
		public string BaudRate
		{
			get
			{
				return this._BaudRate;
			}
			set
			{
				this._BaudRate = value;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000173 RID: 371 RVA: 0x00002A6D File Offset: 0x00000C6D
		// (set) Token: 0x06000174 RID: 372 RVA: 0x00002A75 File Offset: 0x00000C75
		[XmlIgnore]
		public string Parity
		{
			get
			{
				return this._Parity;
			}
			set
			{
				this._Parity = value;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000175 RID: 373 RVA: 0x00002A7E File Offset: 0x00000C7E
		// (set) Token: 0x06000176 RID: 374 RVA: 0x00002A86 File Offset: 0x00000C86
		[XmlIgnore]
		public string DataBit
		{
			get
			{
				return this._DataBit;
			}
			set
			{
				this._DataBit = value;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000177 RID: 375 RVA: 0x00002A8F File Offset: 0x00000C8F
		// (set) Token: 0x06000178 RID: 376 RVA: 0x00002A97 File Offset: 0x00000C97
		[XmlIgnore]
		public string StopBits
		{
			get
			{
				return this._StopBits;
			}
			set
			{
				this._StopBits = value;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000179 RID: 377 RVA: 0x00002AA0 File Offset: 0x00000CA0
		// (set) Token: 0x0600017A RID: 378 RVA: 0x00002AA8 File Offset: 0x00000CA8
		[XmlIgnore]
		public string FontFile
		{
			get
			{
				return this._FontFile;
			}
			set
			{
				this._FontFile = value;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600017B RID: 379 RVA: 0x00002AB1 File Offset: 0x00000CB1
		// (set) Token: 0x0600017C RID: 380 RVA: 0x00002ABE File Offset: 0x00000CBE
		[XmlIgnore]
		public Color BeforeStartTextColor
		{
			get
			{
				return Color.FromArgb(this._BeforeStartTextColor);
			}
			set
			{
				this._BeforeStartTextColor = value.ToArgb();
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600017D RID: 381 RVA: 0x00002ACD File Offset: 0x00000CCD
		// (set) Token: 0x0600017E RID: 382 RVA: 0x00002ADB File Offset: 0x00000CDB
		[XmlIgnore]
		public TimeSpan TimeBeforeStart
		{
			get
			{
				return TimeSpan.FromSeconds((double)this._TimeBeforeStart);
			}
			set
			{
				this._TimeBeforeStart = (int)value.TotalSeconds;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600017F RID: 383 RVA: 0x00002AEB File Offset: 0x00000CEB
		// (set) Token: 0x06000180 RID: 384 RVA: 0x00002AF9 File Offset: 0x00000CF9
		[XmlIgnore]
		public TimeSpan TimeBeforeFlash
		{
			get
			{
				return TimeSpan.FromSeconds((double)this._TimeBeforeFlash);
			}
			set
			{
				this._TimeBeforeFlash = (int)value.TotalSeconds;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000181 RID: 385 RVA: 0x00002B09 File Offset: 0x00000D09
		// (set) Token: 0x06000182 RID: 386 RVA: 0x00002B17 File Offset: 0x00000D17
		[XmlIgnore]
		public TimeSpan TimeFirstBeep
		{
			get
			{
				return TimeSpan.FromSeconds((double)this._TimeFirstBeep);
			}
			set
			{
				this._TimeFirstBeep = (int)value.TotalSeconds;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000183 RID: 387 RVA: 0x00002B27 File Offset: 0x00000D27
		// (set) Token: 0x06000184 RID: 388 RVA: 0x00002B35 File Offset: 0x00000D35
		[XmlIgnore]
		public TimeSpan TimeSecondBeep
		{
			get
			{
				return TimeSpan.FromSeconds((double)this._TimeSecondBeep);
			}
			set
			{
				this._TimeSecondBeep = (int)value.TotalSeconds;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000185 RID: 389 RVA: 0x00002B45 File Offset: 0x00000D45
		// (set) Token: 0x06000186 RID: 390 RVA: 0x00002B4D File Offset: 0x00000D4D
		[XmlIgnore]
		public bool ForwardStrokeBeforeStart
		{
			get
			{
				return this._StrokeBeforeStart;
			}
			set
			{
				this._StrokeBeforeStart = value;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000187 RID: 391 RVA: 0x00002B56 File Offset: 0x00000D56
		// (set) Token: 0x06000188 RID: 392 RVA: 0x00002B61 File Offset: 0x00000D61
		[XmlIgnore]
		public bool ReverceStrokeBeforeStart
		{
			get
			{
				return !this._StrokeBeforeStart;
			}
			set
			{
				this._StrokeBeforeStart = !value;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000189 RID: 393 RVA: 0x00002B6D File Offset: 0x00000D6D
		// (set) Token: 0x0600018A RID: 394 RVA: 0x00002B75 File Offset: 0x00000D75
		[XmlIgnore]
		public string BeepFile
		{
			get
			{
				return this._BeepFile;
			}
			set
			{
				this._BeepFile = value;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600018B RID: 395 RVA: 0x00002B7E File Offset: 0x00000D7E
		// (set) Token: 0x0600018C RID: 396 RVA: 0x00002B8C File Offset: 0x00000D8C
		[XmlIgnore]
		public TimeSpan TimeoutTime
		{
			get
			{
				return TimeSpan.FromSeconds((double)this._TimeoutTime);
			}
			set
			{
				this._TimeoutTime = (int)value.TotalSeconds;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600018D RID: 397 RVA: 0x00002B9C File Offset: 0x00000D9C
		// (set) Token: 0x0600018E RID: 398 RVA: 0x00002BA9 File Offset: 0x00000DA9
		[XmlIgnore]
		public bool TimeoutVideoEnable
		{
			get
			{
				return ParseBoolSafe(this._TimeoutVideoEnable, false);   // БЛОК 3b
			}
			set
			{
				this._TimeoutVideoEnable = value.ToString();
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600018F RID: 399 RVA: 0x00002BB8 File Offset: 0x00000DB8
		// (set) Token: 0x06000190 RID: 400 RVA: 0x00002BC6 File Offset: 0x00000DC6
		[XmlIgnore]
		public TimeSpan BreakTime
		{
			get
			{
				return TimeSpan.FromSeconds((double)this._BreakTime);
			}
			set
			{
				this._BreakTime = (int)value.TotalSeconds;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000191 RID: 401 RVA: 0x00002BD6 File Offset: 0x00000DD6
		// (set) Token: 0x06000192 RID: 402 RVA: 0x00002BE3 File Offset: 0x00000DE3
		[XmlIgnore]
		public bool BreakVideoEnable
		{
			get
			{
				return ParseBoolSafe(this._BreakVideoEnable, false);   // БЛОК 3b
			}
			set
			{
				this._BreakVideoEnable = value.ToString();
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000193 RID: 403 RVA: 0x00002BF2 File Offset: 0x00000DF2
		// (set) Token: 0x06000194 RID: 404 RVA: 0x00002BFF File Offset: 0x00000DFF
		[XmlIgnore]
		public Color FontColor
		{
			get
			{
				return Color.FromArgb(this._FontColor);
			}
			set
			{
				this._FontColor = value.ToArgb();
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000195 RID: 405 RVA: 0x00002C0E File Offset: 0x00000E0E
		// (set) Token: 0x06000196 RID: 406 RVA: 0x00002C1C File Offset: 0x00000E1C
		[XmlIgnore]
		public TimeSpan GameTime
		{
			get
			{
				return TimeSpan.FromSeconds((double)this._GameTime);
			}
			set
			{
				this._GameTime = (int)value.TotalSeconds;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000197 RID: 407 RVA: 0x00002C2C File Offset: 0x00000E2C
		// (set) Token: 0x06000198 RID: 408 RVA: 0x00002C34 File Offset: 0x00000E34
		[XmlIgnore]
		public bool ForwardStrokeGame
		{
			get
			{
				return this._StrokeGame;
			}
			set
			{
				this._StrokeGame = value;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000199 RID: 409 RVA: 0x00002C3D File Offset: 0x00000E3D
		// (set) Token: 0x0600019A RID: 410 RVA: 0x00002C48 File Offset: 0x00000E48
		[XmlIgnore]
		public bool ReverceStrokeGame
		{
			get
			{
				return !this._StrokeGame;
			}
			set
			{
				this._StrokeGame = !value;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600019B RID: 411 RVA: 0x00002C54 File Offset: 0x00000E54
		// (set) Token: 0x0600019C RID: 412 RVA: 0x00002C5C File Offset: 0x00000E5C
		[XmlIgnore]
		public string StartGameBeepFile
		{
			get
			{
				return this._StartGameBeepFile;
			}
			set
			{
				this._StartGameBeepFile = value;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600019D RID: 413 RVA: 0x00002C65 File Offset: 0x00000E65
		// (set) Token: 0x0600019E RID: 414 RVA: 0x00002C6D File Offset: 0x00000E6D
		[XmlIgnore]
		public string GetGateBeepFile
		{
			get
			{
				return this._GetGateBeepFile;
			}
			set
			{
				this._GetGateBeepFile = value;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600019F RID: 415 RVA: 0x00002C76 File Offset: 0x00000E76
		// (set) Token: 0x060001A0 RID: 416 RVA: 0x00002C7E File Offset: 0x00000E7E
		[XmlIgnore]
		public string WhistleBeepFile
		{
			get
			{
				return this._WhistleBeepFile;
			}
			set
			{
				this._WhistleBeepFile = value;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060001A1 RID: 417 RVA: 0x00002C87 File Offset: 0x00000E87
		// (set) Token: 0x060001A2 RID: 418 RVA: 0x00002C94 File Offset: 0x00000E94
		[XmlIgnore]
		public bool AutoWhistleEnable
		{
			get
			{
				return ParseBoolSafe(this._AutoWhistleEnable, true);   // БЛОК 3b
			}
			set
			{
				this._AutoWhistleEnable = value.ToString();
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060001A3 RID: 419 RVA: 0x00002CA3 File Offset: 0x00000EA3
		// (set) Token: 0x060001A4 RID: 420 RVA: 0x00002CB0 File Offset: 0x00000EB0
		[XmlIgnore]
		public bool WhistleMode
		{
			get
			{
				return ParseBoolSafe(this._WhistleMode, true);   // БЛОК 3b
			}
			set
			{
				this._WhistleMode = value.ToString();
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060001A5 RID: 421 RVA: 0x00002CBF File Offset: 0x00000EBF
		// (set) Token: 0x060001A6 RID: 422 RVA: 0x00002CC7 File Offset: 0x00000EC7
		[XmlIgnore]
		public string ScriptPlaylistPath
		{
			get
			{
				return this._ScriptPlaylistPath;
			}
			set
			{
				this._ScriptPlaylistPath = value;
				this.Save();
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060001A7 RID: 423 RVA: 0x00002CD6 File Offset: 0x00000ED6
		// (set) Token: 0x060001A8 RID: 424 RVA: 0x00002CDE File Offset: 0x00000EDE
		[XmlIgnore]
		public string MediaPlaylistPath
		{
			get
			{
				return this._MediaPlaylistPath;
			}
			set
			{
				this._MediaPlaylistPath = value;
				this.Save();
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x00002CED File Offset: 0x00000EED
		// (set) Token: 0x060001AA RID: 426 RVA: 0x00002CF5 File Offset: 0x00000EF5
		[XmlIgnore]
		public string Command_A
		{
			get
			{
				return this._Command_A;
			}
			set
			{
				this._Command_A = value;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060001AB RID: 427 RVA: 0x00002CFE File Offset: 0x00000EFE
		// (set) Token: 0x060001AC RID: 428 RVA: 0x00002D06 File Offset: 0x00000F06
		[XmlIgnore]
		public string TownCommand_A
		{
			get
			{
				return this._TownCommand_A;
			}
			set
			{
				this._TownCommand_A = value;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060001AD RID: 429 RVA: 0x00002D0F File Offset: 0x00000F0F
		// (set) Token: 0x060001AE RID: 430 RVA: 0x00002D17 File Offset: 0x00000F17
		[XmlIgnore]
		public string Command_B
		{
			get
			{
				return this._Command_B;
			}
			set
			{
				this._Command_B = value;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060001AF RID: 431 RVA: 0x00002D20 File Offset: 0x00000F20
		// (set) Token: 0x060001B0 RID: 432 RVA: 0x00002D28 File Offset: 0x00000F28
		[XmlIgnore]
		public string TownCommand_B
		{
			get
			{
				return this._TownCommand_B;
			}
			set
			{
				this._TownCommand_B = value;
			}
		}

		// Token: 0x0400015F RID: 351
		private static object lockFlag = new object();

		// Token: 0x04000160 RID: 352
		private static TConfig _instance;

		// Token: 0x04000161 RID: 353
		[XmlElement("BackColor")]
		public int _BackColor;

		// Token: 0x04000162 RID: 354
		[XmlElement("PromptColor")]
		public int _PromptColor;

		// Token: 0x04000163 RID: 355
		[XmlElement("Left")]
		public int _Left;

		// Token: 0x04000164 RID: 356
		[XmlElement("Top")]
		public int _Top;

		// Token: 0x04000165 RID: 357
		[XmlElement("TopWindow")]
		public string _TopWindow;

		// Token: 0x04000166 RID: 358
		[XmlElement("AutoStart")]
		public string _AutoStart;

		// Token: 0x04000167 RID: 359
		[XmlElement("Port")]
		public string _Port;

		// Token: 0x04000168 RID: 360
		[XmlElement("BaudRate")]
		public string _BaudRate;

		// Token: 0x04000169 RID: 361
		[XmlElement("Parity")]
		public string _Parity;

		// Token: 0x0400016A RID: 362
		[XmlElement("DataBit")]
		public string _DataBit;

		// Token: 0x0400016B RID: 363
		[XmlElement("StopBits")]
		public string _StopBits;

		// Token: 0x0400016C RID: 364
		[XmlElement("FontFile")]
		public string _FontFile;

		// Token: 0x0400016D RID: 365
		[XmlElement("BeforeStartTextColor")]
		public int _BeforeStartTextColor;

		// Token: 0x0400016E RID: 366
		[XmlElement("TimeBeforeStart")]
		public int _TimeBeforeStart;

		// Token: 0x0400016F RID: 367
		[XmlElement("TimeBeforeFlash")]
		public int _TimeBeforeFlash;

		// Token: 0x04000170 RID: 368
		[XmlElement("TimeFirstBeep")]
		public int _TimeFirstBeep;

		// Token: 0x04000171 RID: 369
		[XmlElement("TimeSecondBeep")]
		public int _TimeSecondBeep;

		// Token: 0x04000172 RID: 370
		[XmlElement("StrokeBeforeStart")]
		public bool _StrokeBeforeStart;

		// Token: 0x04000173 RID: 371
		[XmlElement("BeepFile")]
		public string _BeepFile;

		// Token: 0x04000174 RID: 372
		[XmlElement("TimeoutTime")]
		public int _TimeoutTime;

		// Token: 0x04000175 RID: 373
		[XmlElement("TimeoutVideoEnable")]
		public string _TimeoutVideoEnable;

		// Token: 0x04000176 RID: 374
		[XmlElement("BreakTime")]
		public int _BreakTime;

		// Token: 0x04000177 RID: 375
		[XmlElement("BreakVideoEnable")]
		public string _BreakVideoEnable;

		// Token: 0x04000178 RID: 376
		[XmlElement("FontColor")]
		public int _FontColor;

		// Token: 0x04000179 RID: 377
		[XmlElement("GameTime")]
		public int _GameTime;

		// Token: 0x0400017A RID: 378
		[XmlElement("StrokeGame")]
		public bool _StrokeGame;

		// Token: 0x0400017B RID: 379
		[XmlElement("StartGameBeepFile")]
		public string _StartGameBeepFile;

		// Token: 0x0400017C RID: 380
		[XmlElement("GetGateBeepFile")]
		public string _GetGateBeepFile;

		// Token: 0x0400017D RID: 381
		[XmlElement("WhistleBeepFile")]
		public string _WhistleBeepFile;

		// Token: 0x0400017E RID: 382
		[XmlElement("AutoWhistleEnable")]
		public string _AutoWhistleEnable;

		// Token: 0x0400017F RID: 383
		[XmlElement("WhistleMode")]
		public string _WhistleMode;

		// Token: 0x04000180 RID: 384
		[XmlElement("ScriptPlaylistPath")]
		public string _ScriptPlaylistPath;

		// Token: 0x04000181 RID: 385
		[XmlElement("MediaPlaylistPath")]
		public string _MediaPlaylistPath;

		// Token: 0x04000182 RID: 386
		[XmlElement("Command_A")]
		public string _Command_A;

		// Token: 0x04000183 RID: 387
		[XmlElement("TownCommand_A")]
		public string _TownCommand_A;

		// Token: 0x04000184 RID: 388
		[XmlElement("Command_B")]
		public string _Command_B;

		// Token: 0x04000185 RID: 389
		[XmlElement("TownCommand_B")]
		public string _TownCommand_B;
		
		// Token: patch remote log flag
		[XmlElement("RemoteLogEnable")]
		public string _RemoteLogEnable;
		[XmlElement("ExtEnable")]
		public bool ExtEnable
		{
			get
			{
				return this._ExtEnable == "1";
			}
			set
			{
				this._ExtEnable = value ? "1" : "0";
			}
		}

		private string _ExtEnable;

		[XmlElement("ExtSignals")]
		public bool ExtSignals
		{
			get
			{
				return this._ExtSignals != "0";
			}
			set
			{
				this._ExtSignals = value ? "1" : "0";
			}
		}

		private string _ExtSignals = "1";

		[XmlElement("ExtPort")]
		public string ExtPort
		{
			get
			{
				return this._ExtPort;
			}
			set
			{
				this._ExtPort = value;
			}
		}

		private string _ExtPort;

		[XmlElement("ExtBaud")]
		public string ExtBaud
		{
			get
			{
				return string.IsNullOrEmpty(this._ExtBaud) ? "9600" : this._ExtBaud;
			}
			set
			{
				this._ExtBaud = value;
			}
		}

		private string _ExtBaud;

		[XmlElement("MatchStart")]
		public string MatchStart
		{
			get
			{
				return string.IsNullOrEmpty(this._MatchStart) ? "18:30" : this._MatchStart;
			}
			set
			{
				this._MatchStart = value;
			}
		}

		private string _MatchStart;
				[XmlElement("PreMark1")]
		public string PreMark1
		{
			get
			{
				return string.IsNullOrEmpty(this._PreMark1) ? "60:00" : this._PreMark1;
			}
			set
			{
				this._PreMark1 = value;
			}
		}

		private string _PreMark1;

		[XmlElement("PreMark2")]
		public string PreMark2
		{
			get
			{
				return string.IsNullOrEmpty(this._PreMark2) ? "40:00" : this._PreMark2;
			}
			set
			{
				this._PreMark2 = value;
			}
		}

		private string _PreMark2;

		[XmlElement("PreMark3")]
		public string PreMark3
		{
			get
			{
				return string.IsNullOrEmpty(this._PreMark3) ? "20:00" : this._PreMark3;
			}
			set
			{
				this._PreMark3 = value;
			}
		}

		private string _PreMark3;

		[XmlElement("PreMark4")]
		public string PreMark4
		{
			get
			{
				return string.IsNullOrEmpty(this._PreMark4) ? "02:00" : this._PreMark4;
			}
			set
			{
				this._PreMark4 = value;
			}
		}

		private string _PreMark4;

		[XmlElement("BrkMark1")]
		public string BrkMark1
		{
			get
			{
				return string.IsNullOrEmpty(this._BrkMark1) ? "03:00" : this._BrkMark1;
			}
			set
			{
				this._BrkMark1 = value;
			}
		}

		private string _BrkMark1;
				[XmlElement("NetEnable")]
		public bool NetEnable
		{
			get
			{
				return this._NetEnable == "1";
			}
			set
			{
				this._NetEnable = value ? "1" : "0";
			}
		}

		private string _NetEnable;

		[XmlElement("NetHost")]
		public string NetHost
		{
			get
			{
				return string.IsNullOrEmpty(this._NetHost) ? "255.255.255.255" : this._NetHost;
			}
			set
			{
				this._NetHost = value;
			}
		}

		private string _NetHost;

		[XmlElement("NetPort")]
		public string NetPort
		{
			get
			{
				return string.IsNullOrEmpty(this._NetPort) ? "5005" : this._NetPort;
			}
			set
			{
				this._NetPort = value;
			}
		}

		private string _NetPort;
				[XmlElement("WebPassHash")]
		public string WebPassHash
		{
			get
			{
				return this._WebPassHash;
			}
			set
			{
				this._WebPassHash = value;
			}
		}

		private string _WebPassHash;
					[XmlElement("WebPort")]
		public string WebPort
		{
			get
			{
				return string.IsNullOrEmpty(this._WebPort) ? "8642" : this._WebPort;
			}
			set
			{
				this._WebPort = value;
			}
		}

		private string _WebPort;

		[XmlElement("WebBindAll")]
		public string WebBindAll
		{
			get
			{
				return this._WebBindAll;
			}
			set
			{
				this._WebBindAll = value;
			}
		}

		private string _WebBindAll;
		 	[XmlElement("WebUseHttps")]
 	public string WebUseHttps
 	{
 		get
 		{
 			return this._WebUseHttps;
 		}
 		set
 		{
 			this._WebUseHttps = value;
 		}
 	}

 	private string _WebUseHttps;
				[XmlElement("WebIp")]
		public string WebIp
		{
			get
			{
				return this._WebIp;
			}
			set
			{
				this._WebIp = value;
			}
		}

		private string _WebIp;
				[XmlElement("DevIdleMinutes")]
		public string DevIdleMinutes
		{
			get
			{
				return string.IsNullOrEmpty(this._DevIdleMinutes) ? "15" : this._DevIdleMinutes;
			}
			set
			{
				this._DevIdleMinutes = value;
			}
		}

		private string _DevIdleMinutes;
				[XmlElement("DevPassHash")]
		public string DevPassHash
		{
			get
			{
				return this._DevPassHash;
			}
			set
			{
				this._DevPassHash = value;
			}
		}

		private string _DevPassHash;

		[XmlElement("WebEnable")]
		public string WebEnable
		{
			get
			{
				return this._WebEnable;
			}
			set
			{
				this._WebEnable = value;
			}
		}

		private string _WebEnable;

		[XmlElement("WebEnableToken")]
		public string WebEnableToken
		{
			get
			{
				return this._WebEnableToken;
			}
			set
			{
				this._WebEnableToken = value;
			}
		}

		private string _WebEnableToken;
				[XmlElement("ExtFrameTemplate")]
		public string ExtFrameTemplate
		{
			get
			{
				return string.IsNullOrEmpty(this._ExtFrameTemplate) ? "EXT;{MODE};{TIME};{SIG};{TO}" : this._ExtFrameTemplate;
			}
			set
			{
				this._ExtFrameTemplate = value;
			}
		}

		private string _ExtFrameTemplate;

		// БЛОК 7 (аудит), Б12/Б13 (сборка 5): свойства Synth* и поля _Synth* удалены вместе
		// с синтезом. СТАРЫЕ конфиги с элементами <SynthWhistle> и т.п. читаются штатно:
		// XmlSerializer молча пропускает неизвестные элементы, ничего не ломается.

		// Token: patch remote log flag property
		[XmlIgnore]
		public bool RemoteLogEnable
		{
			get
			{
				return this._RemoteLogEnable != "0";
			}
			set
			{
				this._RemoteLogEnable = value ? "1" : "0";
			}
		}
		
		// Token: patch migrated flag
		[XmlElement("MigratedV2")]
		public string _MigratedV2;

		// Token: patch migrated flag property
		[XmlIgnore]
		public bool MigratedV2
		{
			get
			{
				return this._MigratedV2 == "1";
			}
			set
			{
				this._MigratedV2 = value ? "1" : "0";
			}
		}

		// Token: patch migrated flag V3 (хоккейные значения по умолчанию)
		[XmlElement("MigratedV3")]
		public string _MigratedV3;

		[XmlIgnore]
		public bool MigratedV3
		{
			get
			{
				return this._MigratedV3 == "1";
			}
			set
			{
				this._MigratedV3 = value ? "1" : "0";
			}
		}
		// Token: patch migrated flag V4 (звуки sirena.wav, авто-свисток, удаление 02:00, названия ТЕСТ)
		[XmlElement("MigratedV4")]
		public string _MigratedV4;

		[XmlIgnore]
		public bool MigratedV4
		{
			get
			{
				return this._MigratedV4 == "1";
			}
			set
			{
				this._MigratedV4 = value ? "1" : "0";
			}
		}
	}
}
