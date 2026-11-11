using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace Tablo
{
	// Token: 0x0200001D RID: 29
	public partial class SettingForm : Form
	{
		// Token: 0x06000122 RID: 290 RVA: 0x00010C98 File Offset: 0x0000EE98
		private void InitVars()
		{
			// О-2 (аудит 2026-11): TConfig.Instance теперь кэшируется, поэтому при открытии
			// окна настроек конфигурация принудительно перечитывается с диска — форма всегда
			// показывает актуальные значения config.xml.
			TConfig.Reload();
			this.cfg = TConfig.Instance;
			this.int_cfg = (TConfig)this.cfg.Clone();
			if (this.cfg != null)
			{
				try
				{
					this.pB_BackColor.BackColor = this.cfg.BackColor;
					this.tB_Left.Text = this.cfg.Left.ToString();
					this.pB_PromptColor.BackColor = this.cfg.PromptColor;
					this.tB_Top.Text = this.cfg.Top.ToString();
					this.chB_TopWindow.Checked = this.cfg.TopWindow;
					this.chB_AutoStart.Checked = this.cfg.AutoStart;
					this.cB_Port.Text = this.cfg.Port;
					this.cB_BaudRate.Text = this.cfg.BaudRate;
					this.cB_Parity.Text = this.cfg.Parity;
					this.cB_DataBit.Text = this.cfg.DataBit;
					this.cB_StopBits.Text = this.cfg.StopBits;
					this.tB_FontFile.Text = this.cfg.FontFile;
					this.pB_BeforeStartTextColor.BackColor = this.cfg.BeforeStartTextColor;
					this.dTP_TimeBeforeStart.Value = this.dTP_TimeBeforeStart.MinDate + this.cfg.TimeBeforeStart;
					this.dTP_TimeBeforeFlash.Value = this.dTP_TimeBeforeFlash.MinDate + this.cfg.TimeBeforeFlash;
					this.dTP_TimeFirstBeep.Value = this.dTP_TimeFirstBeep.MinDate + this.cfg.TimeFirstBeep;
					this.dTP_TimeSecondBeep.Value = this.dTP_TimeSecondBeep.MinDate + this.cfg.TimeSecondBeep;
					this.rB_ForwardStrokeBeforeStart.Checked = this.cfg.ForwardStrokeBeforeStart;
					this.rB_ReverceStrokeBeforeStart.Checked = this.cfg.ReverceStrokeBeforeStart;
					this.tB_BeepFile.Text = this.cfg.BeepFile;
					this.dTP_TimeoutTime.Value = this.dTP_TimeoutTime.MinDate + this.cfg.TimeoutTime;
					this.chB_TimeoutVideoEnable.Checked = this.cfg.TimeoutVideoEnable;
					this.dTP_BreakTime.Value = this.dTP_BreakTime.MinDate + this.cfg.BreakTime;
					this.chB_BreakVideoEnable.Checked = this.cfg.BreakVideoEnable;
					this.pB_FontColor.BackColor = this.cfg.FontColor;
					this.dTP_GameTime.Value = this.dTP_GameTime.MinDate + this.cfg.GameTime;
					this.rB_ForwardStrokeGame.Checked = this.cfg.ForwardStrokeGame;
					this.rB_ReverceStrokeGame.Checked = this.cfg.ReverceStrokeGame;
					this.tB_StartGameBeepFile.Text = this.cfg.StartGameBeepFile;
					this.tB_GetGateBeepFile.Text = this.cfg.GetGateBeepFile;
					this.tB_WhistleBeepFile.Text = this.cfg.WhistleBeepFile;
					this.chB_AutoWhistleEnable.Checked = this.cfg.AutoWhistleEnable;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Ошибка при чтении конфигурации");
				}
			}
			// БЛОК 4, F8-09: GC.Collect() удалён — вызывался при КАЖДОМ открытии окна
			// настроек и замораживал интерфейс на сотни миллисекунд.
		}

		// Token: 0x06000123 RID: 291 RVA: 0x0000286B File Offset: 0x00000A6B
		public SettingForm()
		{
			this.InitializeComponent();
			this.InitVars();
			this.Patch_InitToolTips();
		}

		// БЛОК 4, F6-12: существующий компонент toolTip1 наконец задействован —
		// у ключевых полей настроек появились понятные подсказки (раньше поле
		// объявлялось, но не использовалось: предупреждение CS0414).
		// БЛОК 7 (аудит), Б15: перечитать реальный список COM-портов, НЕ трогая текущий выбор.
		private bool _patchPortHooked;
		private void Patch_RefreshPortList()
		{
			try
			{
				string cur = this.cB_Port.Text;
				string[] names = SerialPort.GetPortNames();
				Array.Sort(names, StringComparer.OrdinalIgnoreCase);
				this.cB_Port.BeginUpdate();
				try
				{
					this.cB_Port.Items.Clear();
					this.cB_Port.Items.AddRange(names);
				}
				finally { this.cB_Port.EndUpdate(); }
				// прежнее значение сохраняем в любом случае: даже если такого порта сейчас
				// нет в системе, конфиг пульта не должен измениться «сам».
				this.cB_Port.Text = cur;
			}
			catch (Exception ex) { TabloPatch.Diag.Note("Обновление списка COM-портов", ex); }
		}

		// =====================================================================
		// БЛОК 7 (аудит), Б12/Б16 (сборка 5): УДАЛЕНО по решению пользователя:
		//   1) Patch_PreviewSound - обработчики кнопок «Прослушать» (треугольник);
		//   2) Patch_AddSynthCheckboxes / Patch_OneSynthCb - флажки резервного синтеза;
		//   3) сам резервный синтез SynthSound - на слух не похож на сирену.
		// Звук играется ТОЛЬКО из файлов во время матча (frmMain.Patch_PlayAudio).
		// =====================================================================

		private void Patch_InitToolTips()
		{
			try
			{
				if (this.toolTip1 == null)
				{
					return;
				}
				this.toolTip1.SetToolTip(this.chB_TopWindow, "Окна табло и заставки будут поверх всех остальных окон (для захвата видеокартой оставьте включённым)");
				this.toolTip1.SetToolTip(this.chB_AutoStart, "Запускать Табло автоматически при входе в Windows (запись в реестре текущего пользователя)");
				this.toolTip1.SetToolTip(this.cB_Port, "COM-порт физического пульта. Не открывается? Нажмите Ctrl+F7 в главном окне — переподключение");
				this.toolTip1.SetToolTip(this.cB_BaudRate, "Скорость порта пульта. Должна совпадать со настройкой пульта (обычно 115200)");
				this.toolTip1.SetToolTip(this.tB_FontFile, "Файл шрифта табло (.fdat). Без корректного шрифта табло будет пустым");
				this.toolTip1.SetToolTip(this.dTP_GameTime, "Длина ОДНОГО периода основного времени (например, 20:00)");
				this.toolTip1.SetToolTip(this.dTP_BreakTime, "Длина перерыва между периодами (например, 15:00)");
				this.toolTip1.SetToolTip(this.dTP_TimeoutTime, "Длина тренерского тайм-аута (например, 00:30)");
				this.toolTip1.SetToolTip(this.chB_AutoWhistleEnable, "Звуковой сигнал автоматически в конце периода и по окончании перерыва/тайм-аута");
				this.toolTip1.SetToolTip(this.rB_ForwardStrokeGame, "Время периода идёт ВПЕРЁД (00:00 -> 20:00)");
				this.toolTip1.SetToolTip(this.rB_ReverceStrokeGame, "Время периода идёт в обратную сторону (20:00 -> 00:00) — обычный режим для хоккея");
			}
			catch
			{
			}
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00011048 File Offset: 0x0000F248
		private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.int_cfg != null && base.DialogResult == DialogResult.OK)
			{
				// О-3 (аудит 2026-11): БЫЛО — жёстко зашитые дефолты: если элемент управления не
				// найден (вкладка не построена), tablo_stream.txt и tablo_overtime.txt перезаписывались
				// дефолтами и значения оператора молча терялись. СТАЛО — стартовое значение = текущее
				// значение из файла; элемент не найден -> значение сохраняется как было (потерь нет).
				string text = Patch_ReadCfgValue("tablo_stream.txt", "VmixApi", "0");
				string text2 = Patch_ReadCfgValue("tablo_stream.txt", "VmixHost", "192.168.1.100:8088");
				string text3 = Patch_ReadCfgValue("tablo_stream.txt", "VmixInput", "Title.xaml");
				string text4 = Patch_ReadCfgValue("tablo_stream.txt", "VmixField", "TimeText");
				string text5 = Patch_ReadCfgValue("tablo_stream.txt", "Udp", "0");
				string text6 = Patch_ReadCfgValue("tablo_stream.txt", "UdpHost", "192.168.1.100:9999");
				string text7 = Patch_ReadCfgValue("tablo_stream.txt", "File", "0");
				string str = Patch_ReadOvertimeDefault();
				foreach (object obj in this.tabControl_MainSetting.TabPages)
				{
					foreach (object obj2 in ((TabPage)obj).Controls)
					{
						Control control = (Control)obj2;
						if (control.Name == "cbAutoBreak")
						{
							this.int_cfg.WhistleMode = ((CheckBox)control).Checked;
						}
						if (control.Name == "dtpPenaltyTime")
						{
							this.int_cfg.TimeBeforeFlash = ((DateTimePicker)control).Value.TimeOfDay;
						}
						if (control.Name == "cbxVmixApi")
						{
							text = (((CheckBox)control).Checked ? "1" : "0");
						}
						if (control.Name == "tbVmixHost")
						{
							text2 = control.Text;
						}
						if (control.Name == "tbVmixInput")
						{
							text3 = control.Text;
						}
						if (control.Name == "tbVmixField")
						{
							text4 = control.Text;
						}
						if (control.Name == "cbxUdp")
						{
							text5 = (((CheckBox)control).Checked ? "1" : "0");
						}
						if (control.Name == "tbUdpHost")
						{
							text6 = control.Text;
						}
						if (control.Name == "cbRemoteLog")
						{
							this.int_cfg.RemoteLogEnable = ((CheckBox)control).Checked;
						}
						if (control.Name == "cbxFile")
						{
							text7 = (((CheckBox)control).Checked ? "1" : "0");
						}
						if (control.Name == "dtpOvertimeTime")
						{
							str = ((DateTimePicker)control).Value.TimeOfDay.ToString();
						}
					}
				}
								Control[] foundMatch = this.tabControl_MainSetting.Controls.Find("dtpMatchStart", true);
				if (foundMatch.Length != 0)
				{
					this.int_cfg.MatchStart = ((DateTimePicker)foundMatch[0]).Value.ToString("HH:mm");
				}
								Control[] foundPre1 = this.tabControl_MainSetting.Controls.Find("tbPreMark1", true);
				if (foundPre1.Length != 0)
				{
					this.int_cfg.PreMark1 = ((TextBox)foundPre1[0]).Text.Trim();
				}
				Control[] foundPre2 = this.tabControl_MainSetting.Controls.Find("tbPreMark2", true);
				if (foundPre2.Length != 0)
				{
					this.int_cfg.PreMark2 = ((TextBox)foundPre2[0]).Text.Trim();
				}
				Control[] foundPre3 = this.tabControl_MainSetting.Controls.Find("tbPreMark3", true);
				if (foundPre3.Length != 0)
				{
					this.int_cfg.PreMark3 = ((TextBox)foundPre3[0]).Text.Trim();
				}
				Control[] foundPre4 = this.tabControl_MainSetting.Controls.Find("tbPreMark4", true);
				if (foundPre4.Length != 0)
				{
					this.int_cfg.PreMark4 = ((TextBox)foundPre4[0]).Text.Trim();
				}
				Control[] foundBrk1 = this.tabControl_MainSetting.Controls.Find("tbBrkMark1", true);
				if (foundBrk1.Length != 0)
				{
					this.int_cfg.BrkMark1 = ((TextBox)foundBrk1[0]).Text.Trim();
				}
								// БЛОК 5: строка «Пароль оператора» убрана с вкладки «Прочие» - пароль
				// веб-оператора задаётся и сбрасывается в окне разработчика (группы
				// «Пароль оператора веб-панели» и сброс супер-паролем). Здесь остаётся
				// только синхронизация рабочего хэша веб-сервера из настроек.
				this.cfg = (TConfig)this.int_cfg.Clone();
				this.cfg.Save();
				// О-2 (аудит 2026-11): после сохранения сбрасываем кэш синглтона, чтобы потребители
				// TConfig.Instance (веб-сервер и др.) перечитали обновлённый config.xml. Раньше это
				// неявно делал Clone() подменой _instance — теперь Clone без побочного эффекта.
				TConfig.Reload();
				TabloPatch.WebServer.PassHash = this.cfg.WebPassHash;
				try
				{
					File.WriteAllText(Path.Combine(Application.StartupPath, "tablo_stream.txt"), string.Concat(new string[]
					{
						"VmixApi=",
						text,
						"\r\nVmixHost=",
						text2,
						"\r\nVmixInput=",
						text3,
						"\r\nVmixField=",
						text4,
						"\r\nUdp=",
						text5,
						"\r\nUdpHost=",
						text6,
						"\r\nFile=",
						text7,
						"\r\n"
					}));
				}
				catch (Exception exStream)
				{
					// О-19 (аудит 2026-11): пустой catch скрывал ошибку записи tablo_stream.txt.
					TabloPatch.Diag.Note("SettingForm: запись tablo_stream.txt", exStream);
				}
				try
				{
					File.WriteAllText(Path.Combine(Application.StartupPath, "tablo_overtime.txt"), str + "\r\n");
				}
				catch (Exception exOt)
				{
					// О-19 (аудит 2026-11): пустой catch скрывал ошибку записи tablo_overtime.txt.
					TabloPatch.Diag.Note("SettingForm: запись tablo_overtime.txt", exOt);
				}
			}
		}

		// О-3 (аудит 2026-11): чтение текущего значения ключа из файла настроек вида «Key=value»
		// (по строке на ключ). Файла/ключа нет или ошибка чтения -> fallback (прежний дефолт).
		private static string Patch_ReadCfgValue(string fileName, string key, string fallback)
		{
			try
			{
				string path = Path.Combine(Application.StartupPath, fileName);
				if (!File.Exists(path))
				{
					return fallback;
				}
				string[] lines = File.ReadAllLines(path);
				for (int i = 0; i < lines.Length; i++)
				{
					int eq = lines[i].IndexOf('=');
					if (eq > 0 && lines[i].Substring(0, eq).Trim() == key)
					{
						return lines[i].Substring(eq + 1).Trim();
					}
				}
			}
			catch (Exception ex)
			{
				TabloPatch.Diag.Note("SettingForm: чтение " + fileName, ex);
			}
			return fallback;
		}

		// О-3 (аудит 2026-11): текущее значение времени овертайма из tablo_overtime.txt
		// (первая непустая строка); файла нет или ошибка -> прежний дефолт 00:05:00.
		private static string Patch_ReadOvertimeDefault()
		{
			try
			{
				string path = Path.Combine(Application.StartupPath, "tablo_overtime.txt");
				if (File.Exists(path))
				{
					string[] lines = File.ReadAllLines(path);
					for (int i = 0; i < lines.Length; i++)
					{
						if (lines[i].Trim().Length > 0)
						{
							return lines[i].Trim();
						}
					}
				}
			}
			catch (Exception ex)
			{
				TabloPatch.Diag.Note("SettingForm: чтение tablo_overtime.txt", ex);
			}
			return "00:05:00";
		}

		// Token: 0x06000126 RID: 294 RVA: 0x000113EC File Offset: 0x0000F5EC
		private void SettingForm_Shown(object sender, EventArgs e)
		{
			// БЛОК 7 (аудит), Б15: список COM-портов ОБНОВЛЯЕТСЯ (раньше AddRange без Clear
			// выполнялся один раз за сеанс: воткнутый позже USB-COM-переходник в списке не
			// появлялся, а вписать порт руками нельзя — cB_Port_KeyPress глушит все клавиши).
			// Выбранный/сохранённый порт НЕ трогаем: управление пультом не должно сломаться.
			this.Patch_RefreshPortList();
			if (!this._patchPortHooked)
			{
				this._patchPortHooked = true;
				this.cB_Port.DropDown += delegate(object sD, EventArgs eD) { this.Patch_RefreshPortList(); };
			}
			ComboBox.ObjectCollection items3 = this.cB_Parity.Items;
			// БЛОК 7, hotfix build4: в оригинале переменные array/items2 объявлялись в
			// удалённом блоке заполнения списка портов (теперь это Patch_RefreshPortList),
			// поэтому объявления возвращены сюда - иначе CS0103.
			object[] array = Enum.GetNames(typeof(Parity));
			object[] items2 = array;
			items3.AddRange(items2);
			ComboBox.ObjectCollection items4 = this.cB_StopBits.Items;
			array = Enum.GetNames(typeof(StopBits));
			items2 = array;
			items4.AddRange(items2);
			this.InitVars();
			// ОП-6 (аудит 2026-11): построение вкладки «Прочие» (~550 строк императивной
			// верстки) вынесено в Patch_BuildOtherTab() — код перенесён БЕЗ изменений координат
			// и порядка создания контролов (слепая перекладка пикселей дала бы визуальные
			// регрессии); метод-строитель теперь можно править отдельно от обработчика Shown.
			this.Patch_BuildOtherTab();
		}

		// ОП-6 (аудит 2026-11): построение вкладки «Прочие». Вынесено из SettingForm_Shown
		// без изменения поведения. Порядок строк: группы gbPre/gbBrk и поля создаются
// (группа «Дополнительный вывод» 2026-11 перенесена в окно режима разработчика).
		// сверху вниз; финальный блок Location-ов (метка «БЛОК 5: поднято») — компактная
		// раскладка: правьте координаты ТАМ, а не в scattered Location выше.
		private void Patch_BuildOtherTab()
		{
			if (!this.tabControl_MainSetting.TabPages.ContainsKey("tpOther"))
			{
				TabPage tabPage = new TabPage("Прочие");
				tabPage.Name = "tpOther";
								tabPage.AutoScroll = true;
				CheckBox checkBox = new CheckBox();
				checkBox.Name = "cbAutoBreak";
				checkBox.Text = "Автоматический запуск перерыва и увеличение числа периода";
				checkBox.Location = new Point(10, 10);
				checkBox.Size = new Size(430, 24);
				checkBox.Checked = this.cfg.WhistleMode;
				tabPage.Controls.Add(checkBox);
				this.l_TimeBeforeFlash.Visible = false;
				this.dTP_TimeBeforeFlash.Visible = false;
				Label label = new Label();
				label.Name = "lPenaltyTime";
				label.Text = "Время удаленного игрока:";
				label.Size = new Size(150, 20);
				label.Location = new Point(10, 42);
				DateTimePicker dateTimePicker = new DateTimePicker();
				dateTimePicker.Name = "dtpPenaltyTime";
				dateTimePicker.Format = DateTimePickerFormat.Time;
				dateTimePicker.ShowUpDown = true;
				dateTimePicker.CustomFormat = "HH:mm:ss";
				try
				{
					dateTimePicker.Value = DateTime.Today.Add(this.cfg.TimeBeforeFlash);
				}
				catch
				{
					dateTimePicker.Value = DateTime.Today.Add(TimeSpan.FromMinutes(2.0));
				}
				dateTimePicker.Size = new Size(100, 20);
				dateTimePicker.Location = new Point(165, 40);
				tabPage.Controls.Add(label);
				tabPage.Controls.Add(dateTimePicker);
				CheckBox checkBox2 = new CheckBox();
				checkBox2.Name = "cbxVmixApi";
				checkBox2.Text = "Отдача времени в vMix (API HTTP)";
				checkBox2.Size = new Size(260, 20);
				checkBox2.Location = new Point(10, 70);
				Label label2 = new Label();
				label2.Text = "Адрес vMix (IP:порт):";
				label2.Size = new Size(140, 20);
				label2.Location = new Point(10, 95);
				TextBox textBox = new TextBox();
				textBox.Name = "tbVmixHost";
				textBox.Size = new Size(140, 20);
				textBox.Location = new Point(155, 93);
				Label label3 = new Label();
				label3.Text = "Титр (Input):";
				label3.Size = new Size(90, 20);
				label3.Location = new Point(10, 120);
				TextBox textBox2 = new TextBox();
				textBox2.Name = "tbVmixInput";
				textBox2.Size = new Size(140, 20);
				textBox2.Location = new Point(155, 118);
				Label label4 = new Label();
				label4.Text = "Поле (SelectedName):";
				label4.Size = new Size(140, 20);
				label4.Location = new Point(10, 145);
				TextBox textBox3 = new TextBox();
				textBox3.Name = "tbVmixField";
				textBox3.Size = new Size(140, 20);
				textBox3.Location = new Point(155, 143);
								label4.Visible = false;
				textBox3.Visible = false;
				CheckBox checkBox3 = new CheckBox();
				checkBox3.Name = "cbxUdp";
				checkBox3.Text = "Отдача времени по UDP (JSON)";
				checkBox3.Size = new Size(260, 20);
				checkBox3.Location = new Point(10, 145);
				Label label5 = new Label();
				label5.Text = "Адрес UDP (IP:порт):";
				label5.Size = new Size(140, 20);
				label5.Location = new Point(10, 170);
				TextBox textBox4 = new TextBox();
				textBox4.Name = "tbUdpHost";
				textBox4.Size = new Size(140, 20);
				textBox4.Location = new Point(155, 168);
				CheckBox checkBox4 = new CheckBox();
				checkBox4.Name = "cbxFile";
				checkBox4.Text = "Писать время в файл tablo_time.txt";
				checkBox4.Size = new Size(260, 20);
				checkBox4.Location = new Point(10, 195);
				Label label6 = new Label();
				label6.Text = "Время овертайма (4-й период):";
				label6.Size = new Size(170, 20);
				label6.Location = new Point(10, 220);
				DateTimePicker dateTimePicker2 = new DateTimePicker();
				dateTimePicker2.Name = "dtpOvertimeTime";
				dateTimePicker2.Format = DateTimePickerFormat.Time;
				dateTimePicker2.ShowUpDown = true;
				dateTimePicker2.CustomFormat = "HH:mm:ss";
				dateTimePicker2.Size = new Size(100, 20);
				dateTimePicker2.Location = new Point(185, 218);
				string a = "0";
				string text = "192.168.1.100:8088";
				string text2 = "Title.xaml";
				string text3 = "TimeText";
				string a2 = "0";
				string text4 = "192.168.1.100:9999";
				string a3 = "0";
				string s = "00:05:00";
				string path = Path.Combine(Application.StartupPath, "tablo_stream.txt");
				if (File.Exists(path))
				{
					foreach (string text5 in File.ReadAllLines(path))
					{
						int num = text5.IndexOf('=');
						if (num >= 0)
						{
							string a4 = text5.Substring(0, num).Trim();
							string text6 = text5.Substring(num + 1).Trim();
							if (a4 == "VmixApi")
							{
								a = text6;
							}
							else if (a4 == "VmixHost")
							{
								text = text6;
							}
							else if (a4 == "VmixInput")
							{
								text2 = text6;
							}
							else if (a4 == "VmixField")
							{
								text3 = text6;
							}
							else if (a4 == "Udp")
							{
								a2 = text6;
							}
							else if (a4 == "UdpHost")
							{
								text4 = text6;
							}
							else if (a4 == "File")
							{
								a3 = text6;
							}
							else if (a4 == "Overtime")
							{
								s = text6;
							}
						}
					}
				}
				string path2 = Path.Combine(Application.StartupPath, "tablo_overtime.txt");
				if (File.Exists(path2))
				{
					try
					{
						string text7 = File.ReadAllText(path2).Trim();
						if (text7.Length > 0)
						{
							s = text7;
						}
					}
					catch
					{
					}
				}
				checkBox2.Checked = (a == "1");
				textBox.Text = text;
				textBox2.Text = text2;
				textBox3.Text = text3;
				checkBox3.Checked = (a2 == "1");
				textBox4.Text = text4;
				checkBox4.Checked = (a3 == "1");
				try
				{
					dateTimePicker2.Value = DateTime.Today.Add(TimeSpan.Parse(s));
				}
				catch
				{
					dateTimePicker2.Value = DateTime.Today.Add(TimeSpan.FromMinutes(5.0));
				}
				tabPage.Controls.Add(checkBox2);
				tabPage.Controls.Add(label2);
				tabPage.Controls.Add(textBox);
				tabPage.Controls.Add(label3);
				tabPage.Controls.Add(textBox2);
				tabPage.Controls.Add(label4);
				tabPage.Controls.Add(textBox3);
				tabPage.Controls.Add(checkBox3);
				tabPage.Controls.Add(label5);
				tabPage.Controls.Add(textBox4);
				tabPage.Controls.Add(checkBox4);
				tabPage.Controls.Add(label6);
				tabPage.Controls.Add(dateTimePicker2);
				CheckBox checkBoxLog = new CheckBox();
				checkBoxLog.Name = "cbRemoteLog";
				checkBoxLog.Text = "Вести лог пульта remote_com_log.txt";
				checkBoxLog.Size = new Size(260, 20);
				checkBoxLog.Location = new Point(10, 245);
				checkBoxLog.Checked = this.cfg.RemoteLogEnable;
				checkBoxLog.CheckedChanged += delegate
				{
					TabloPatch.SerialPortLog.Enabled = checkBoxLog.Checked;
				};
				tabPage.Controls.Add(checkBoxLog);
				TabloPatch.TimeMemorySettingsControl tmctl = new TabloPatch.TimeMemorySettingsControl(TabloPatch.PatchEntry.Core);
				tmctl.Location = new Point(10, 270);
				tmctl.Size = new Size(470, 80);
				tabPage.Controls.Add(tmctl);
								tabPage.AutoScroll = true;
								GroupBox gbPre = new GroupBox();
				gbPre.Name = "gbPreMarks";
				gbPre.Text = "Предматчевый таймер (метки сирен)";
				gbPre.Location = new Point(10, 355);   // 2026-11: поднято после переноса «Дополнительного вывода» в режим разработчика
				gbPre.Size = new Size(470, 150);
				string[] preLabels = new string[]
				{
					"Сирен. предматч 1 (все):",
					"Сирен. предматч 2 (все):",
					"Сирен. предматч 3 (все):",
					"Сирен. предматч 4 (все):"
				};
				string[] preVals = new string[]
				{
					this.cfg.PreMark1,
					this.cfg.PreMark2,
					this.cfg.PreMark3,
					this.cfg.PreMark4
				};
				for (int ip = 0; ip < 4; ip++)
				{
					Label lbPre = new Label();
					lbPre.Text = preLabels[ip];
					lbPre.Location = new Point(10, 22 + ip * 26);
					lbPre.Size = new Size(190, 20);
					TextBox tbPre = new TextBox();
					tbPre.Name = "tbPreMark" + (ip + 1).ToString();
					tbPre.Text = preVals[ip];
					tbPre.Location = new Point(205, 20 + ip * 26);
					tbPre.Size = new Size(60, 20);
					Button btPre = new Button();
					btPre.Name = "btPreTest" + (ip + 1).ToString();
					btPre.Text = "Тест-сигнал";
					btPre.Size = new Size(90, 23);
					btPre.Location = new Point(275, 18 + ip * 26);
					btPre.Tag = ip + 1;
					btPre.Click += delegate(object sT, EventArgs eT)
					{
						int n = (int)((Button)sT).Tag;
						string[] tg = new string[] { "A", "A", "A", "A" };
						TabloPatch.ExtOut.TestFire(tg[n - 1], "ПРЕДМАТЧ: ручная проверка, метка " + n);
					};
					gbPre.Controls.Add(lbPre);
					gbPre.Controls.Add(tbPre);
					gbPre.Controls.Add(btPre);
				}
				tabPage.Controls.Add(gbPre);
				GroupBox gbBrk = new GroupBox();
				gbBrk.Name = "gbBrkMarks";
				gbBrk.Text = "Перерыв (метки сирен)";
				gbBrk.Location = new Point(10, 510);   // 2026-11: поднято вслед за gbPre
				gbBrk.Size = new Size(470, 80);
				Label lbB1 = new Label();
				lbB1.Text = "Сирен. пер 1 (все, до конца):";
				lbB1.Location = new Point(10, 22);
				lbB1.Size = new Size(190, 20);
				TextBox tbB1 = new TextBox();
				tbB1.Name = "tbBrkMark1";
				tbB1.Text = this.cfg.BrkMark1;
				tbB1.Location = new Point(205, 20);
				tbB1.Size = new Size(60, 20);
				Button btB1 = new Button();
				btB1.Name = "btBrkTest1";
				btB1.Text = "Тест-сигнал";
				btB1.Size = new Size(90, 23);
				btB1.Location = new Point(275, 18);
				btB1.Click += delegate
				{
					TabloPatch.ExtOut.TestFire("A", "ПЕРЕРЫВ: ручная проверка, метка 1");
				};
				Label lbB2 = new Label();
				lbB2.Text = "Сирен. пер 2 (все): по окончании перерыва, автоматически";
				lbB2.Location = new Point(10, 48);
				lbB2.Size = new Size(360, 20);
				gbBrk.Controls.Add(lbB1);
				gbBrk.Controls.Add(tbB1);
				gbBrk.Controls.Add(btB1);
				gbBrk.Controls.Add(lbB2);
				tabPage.Controls.Add(gbBrk);
				TabPage tpBefore = null;
				foreach (object objT in this.tabControl_MainSetting.TabPages)
				{
					TabPage tpT = (TabPage)objT;
					if (tpT.Text == "Предстартовый таймер")
					{
						tpBefore = tpT;
						break;
					}
				}
				if (tpBefore != null)
				{
					int yBase = 10;
					foreach (Control ctl in tpBefore.Controls)
					{
						if (ctl.Bottom > yBase)
						{
							yBase = ctl.Bottom;
						}
					}
					yBase += 10;
					Label lbMatchStart = new Label();
					lbMatchStart.Text = "Время начала матча:";
					lbMatchStart.Location = new Point(10, yBase + 3);
					lbMatchStart.Size = new Size(140, 20);
					DateTimePicker dtpMatchStart = new DateTimePicker();
					dtpMatchStart.Name = "dtpMatchStart";
					dtpMatchStart.Format = DateTimePickerFormat.Custom;
					dtpMatchStart.CustomFormat = "HH:mm";
					dtpMatchStart.ShowUpDown = true;
					dtpMatchStart.Location = new Point(155, yBase);
					dtpMatchStart.Size = new Size(70, 20);
					try
					{
						dtpMatchStart.Value = DateTime.ParseExact(this.cfg.MatchStart, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
					}
					catch
					{
						dtpMatchStart.Value = new DateTime(2000, 1, 1, 18, 30, 0);
					}
					Button butEnterStart = new Button();
					butEnterStart.Name = "butEnterStart";
					butEnterStart.Text = "Ввести время";
					butEnterStart.Location = new Point(235, yBase - 1);
					butEnterStart.Size = new Size(120, 23);
					butEnterStart.Click += delegate
					{
						DateTime nowDt = DateTime.Now;
						DateTime startDt = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day, dtpMatchStart.Value.Hour, dtpMatchStart.Value.Minute, 0);
						TimeSpan diffTs = startDt - nowDt;
						if (diffTs < TimeSpan.Zero)
						{
							diffTs = diffTs + TimeSpan.FromDays(1.0);
						}
						TimeSpan cutTs = TimeSpan.FromSeconds((double)(int)Math.Ceiling(diffTs.TotalSeconds));
						this.cfg.TimeBeforeStart = cutTs;
						this.cfg.MatchStart = dtpMatchStart.Value.ToString("HH:mm");
						this.cfg.Save();
						TConfig.Reload();   // О-2 (аудит 2026-11): кэш синглтона — в синхронно-сохранённое состояние
						if (TabloPatch.ExtOut.BeforeTotalSetter != null)
						{
							TabloPatch.ExtOut.BeforeTotalSetter(cutTs);
						}
					};
					tpBefore.Controls.Add(lbMatchStart);
					tpBefore.Controls.Add(dtpMatchStart);
					tpBefore.Controls.Add(butEnterStart);
				}
				checkBox2.Visible = false;
				label2.Visible = false;
				textBox.Visible = false;
				label3.Visible = false;
				textBox2.Visible = false;
				checkBox3.Visible = false;
				label5.Visible = false;
				textBox4.Visible = false;
				checkBox4.Visible = false;
				checkBoxLog.Visible = false;
				label6.Location = new Point(10, 70);
				dateTimePicker2.Location = new Point(185, 68);
				tmctl.Location = new Point(10, 100);
				gbPre.Location = new Point(10, 190);   // 2026-11: поднято (компактная верстка)
				gbBrk.Location = new Point(10, 345);   // 2026-11: поднято (компактная верстка)
				this.tabControl_MainSetting.TabPages.Add(tabPage);
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00011C28 File Offset: 0x0000FE28
		private Color SelColorDialog()
		{
			ColorDialog colorDialog = new ColorDialog();
			Color result = Color.Black;
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				result = colorDialog.Color;
			}
			return result;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00011C54 File Offset: 0x0000FE54
		private string SelAudioFile()
		{
			string result = string.Empty;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (this.tB_BeepFile.Text != string.Empty)
			{
				openFileDialog.FileName = this.tB_BeepFile.Text;
			}
			openFileDialog.Filter = "Media Files|*.wav;*.mp3;*.mp2;*.wma|All Files|*.*";
			openFileDialog.Title = "Выбор аудиофайла";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				result = openFileDialog.FileName;
			}
			return result;
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00011CBC File Offset: 0x0000FEBC
		private string SelFontFile()
		{
			string result = string.Empty;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Font Files|*.fdat|All Files|*.*";   // БЛОК 4, F4-12: лишняя «;» в фильтре убрана
			openFileDialog.Title = "Выбор файла шрифта";
			if (DialogResult.OK == openFileDialog.ShowDialog())
			{
				result = openFileDialog.FileName;
			}
			return result;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x000028A1 File Offset: 0x00000AA1
		private void but_SelBackColor_Click(object sender, EventArgs e)
		{
			this.pB_BackColor.BackColor = this.SelColorDialog();
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00011CFC File Offset: 0x0000FEFC
		private void pB_BackColor_BackColorChanged(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.BackColor = pictureBox.BackColor;
			}
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00011D2C File Offset: 0x0000FF2C
		private void tB_Left_TextChanged(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.Left = Convert.ToInt32(textBox.Text);
			}
		}

		// Token: 0x0600012D RID: 301 RVA: 0x000028B4 File Offset: 0x00000AB4
		private void tB_Left_KeyPress(object sender, KeyPressEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (!char.IsControl(e.KeyChar) && !char.IsNumber(e.KeyChar))
			{
				e.KeyChar = '\0';
				e.Handled = true;
			}
		}

		// Token: 0x0600012E RID: 302 RVA: 0x000028E5 File Offset: 0x00000AE5
		private void but_PromptColor_Click(object sender, EventArgs e)
		{
			this.pB_PromptColor.BackColor = this.SelColorDialog();
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00011D60 File Offset: 0x0000FF60
		private void pB_PromptColor_BackColorChanged(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.PromptColor = pictureBox.BackColor;
			}
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00011D90 File Offset: 0x0000FF90
		private void tB_Top_TextChanged(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.Top = Convert.ToInt32(textBox.Text);
			}
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00011DC4 File Offset: 0x0000FFC4
		private void chB_TopWindow_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.TopWindow = checkBox.Checked;
			}
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00011DF4 File Offset: 0x0000FFF4
		private void chB_AutoStart_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.AutoStart = checkBox.Checked;
			}
		}

		// Token: 0x06000133 RID: 307 RVA: 0x000028F8 File Offset: 0x00000AF8
		private void cB_Port_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = true;
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00011E24 File Offset: 0x00010024
		private void cB_Port_TextChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.Port = comboBox.Text;
			}
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00011E54 File Offset: 0x00010054
		private void cB_BaudRate_TextChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.BaudRate = comboBox.Text;
			}
		}

		// Token: 0x06000136 RID: 310 RVA: 0x00011E84 File Offset: 0x00010084
		private void cB_Parity_TextChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.Parity = comboBox.Text;
			}
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00011EB4 File Offset: 0x000100B4
		private void cB_DataBit_TextChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.DataBit = comboBox.Text;
			}
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00011EE4 File Offset: 0x000100E4
		private void cB_StopBits_TextChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.StopBits = comboBox.Text;
			}
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00002901 File Offset: 0x00000B01
		private void but_SelFontFile_Click(object sender, EventArgs e)
		{
			this.tB_FontFile.Text = this.SelFontFile();
		}

		// Token: 0x0600013A RID: 314 RVA: 0x00011F14 File Offset: 0x00010114
		private void tB_FontFile_TextChanged(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.FontFile = textBox.Text;
			}
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00002914 File Offset: 0x00000B14
		private void but_SelBeforeStartTextColor_Click(object sender, EventArgs e)
		{
			this.pB_BeforeStartTextColor.BackColor = this.SelColorDialog();
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00011F44 File Offset: 0x00010144
		private void pB_BeforeStartTextColor_BackColorChanged(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.BeforeStartTextColor = pictureBox.BackColor;
			}
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00011F74 File Offset: 0x00010174
		private void dTP_TimeBeforeStart_ValueChanged(object sender, EventArgs e)
		{
			DateTimePicker dateTimePicker = (DateTimePicker)sender;
			TimeSpan timeBeforeStart = TimeSpan.FromHours((double)dateTimePicker.Value.Hour) + TimeSpan.FromMinutes((double)dateTimePicker.Value.Minute) + TimeSpan.FromSeconds((double)dateTimePicker.Value.Second);
			if (this.int_cfg != null)
			{
				this.int_cfg.TimeBeforeStart = timeBeforeStart;
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00011FE4 File Offset: 0x000101E4
		private void dTP_TimeBeforeFlash_ValueChanged(object sender, EventArgs e)
		{
			DateTimePicker dateTimePicker = (DateTimePicker)sender;
			TimeSpan timeBeforeFlash = TimeSpan.FromHours((double)dateTimePicker.Value.Hour) + TimeSpan.FromMinutes((double)dateTimePicker.Value.Minute) + TimeSpan.FromSeconds((double)dateTimePicker.Value.Second);
			if (this.int_cfg != null)
			{
				this.int_cfg.TimeBeforeFlash = timeBeforeFlash;
			}
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00012054 File Offset: 0x00010254
		private void dTP_TimeFirstBeep_ValueChanged(object sender, EventArgs e)
		{
			DateTimePicker dateTimePicker = (DateTimePicker)sender;
			TimeSpan timeFirstBeep = TimeSpan.FromHours((double)dateTimePicker.Value.Hour) + TimeSpan.FromMinutes((double)dateTimePicker.Value.Minute) + TimeSpan.FromSeconds((double)dateTimePicker.Value.Second);
			if (this.int_cfg != null)
			{
				this.int_cfg.TimeFirstBeep = timeFirstBeep;
			}
		}

		// Token: 0x06000140 RID: 320 RVA: 0x000120C4 File Offset: 0x000102C4
		private void dTP_TimeSecondBeep_ValueChanged(object sender, EventArgs e)
		{
			DateTimePicker dateTimePicker = (DateTimePicker)sender;
			TimeSpan timeSecondBeep = TimeSpan.FromHours((double)dateTimePicker.Value.Hour) + TimeSpan.FromMinutes((double)dateTimePicker.Value.Minute) + TimeSpan.FromSeconds((double)dateTimePicker.Value.Second);
			if (this.int_cfg != null)
			{
				this.int_cfg.TimeSecondBeep = timeSecondBeep;
			}
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00012134 File Offset: 0x00010334
		private void rB_ForwardStrokeBeforeStart_CheckedChanged(object sender, EventArgs e)
		{
			RadioButton radioButton = (RadioButton)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.ForwardStrokeBeforeStart = radioButton.Checked;
			}
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00002927 File Offset: 0x00000B27
		private void but_SelBeepFile_Click(object sender, EventArgs e)
		{
			this.tB_BeepFile.Text = this.SelAudioFile();
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00012164 File Offset: 0x00010364
		private void tB_BeepFile_TextChanged(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.BeepFile = textBox.Text;
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x000121F4 File Offset: 0x000103F4
		private void dTP_TimeoutTime_ValueChanged(object sender, EventArgs e)
		{
			DateTimePicker dateTimePicker = (DateTimePicker)sender;
			TimeSpan timeoutTime = TimeSpan.FromHours((double)dateTimePicker.Value.Hour) + TimeSpan.FromMinutes((double)dateTimePicker.Value.Minute) + TimeSpan.FromSeconds((double)dateTimePicker.Value.Second);
			if (this.int_cfg != null)
			{
				this.int_cfg.TimeoutTime = timeoutTime;
			}
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00012264 File Offset: 0x00010464
		private void chB_TimeoutVideoEnable_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.TimeoutVideoEnable = checkBox.Checked;
			}
		}

		// Token: 0x06000147 RID: 327 RVA: 0x00012294 File Offset: 0x00010494
		private void dTP_BreakTime_ValueChanged(object sender, EventArgs e)
		{
			DateTimePicker dateTimePicker = (DateTimePicker)sender;
			TimeSpan breakTime = TimeSpan.FromHours((double)dateTimePicker.Value.Hour) + TimeSpan.FromMinutes((double)dateTimePicker.Value.Minute) + TimeSpan.FromSeconds((double)dateTimePicker.Value.Second);
			if (this.int_cfg != null)
			{
				this.int_cfg.BreakTime = breakTime;
			}
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00012304 File Offset: 0x00010504
		private void dTP_GameTime_ValueChanged(object sender, EventArgs e)
		{
			DateTimePicker dateTimePicker = (DateTimePicker)sender;
			TimeSpan gameTime = TimeSpan.FromHours((double)dateTimePicker.Value.Hour) + TimeSpan.FromMinutes((double)dateTimePicker.Value.Minute) + TimeSpan.FromSeconds((double)dateTimePicker.Value.Second);
			if (this.int_cfg != null)
			{
				this.int_cfg.GameTime = gameTime;
			}
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00012374 File Offset: 0x00010574
		private void chB_BreakVideoEnable_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.BreakVideoEnable = checkBox.Checked;
			}
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0000293A File Offset: 0x00000B3A
		private void but_SelFontColor_Click(object sender, EventArgs e)
		{
			this.pB_FontColor.BackColor = this.SelColorDialog();
		}

		// Token: 0x0600014B RID: 331 RVA: 0x000123A4 File Offset: 0x000105A4
		private void pB_FontColor_BackColorChanged(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.FontColor = pictureBox.BackColor;
			}
		}

		// Token: 0x0600014C RID: 332 RVA: 0x000123D4 File Offset: 0x000105D4
		private void rB_ForwardStrokeGame_CheckedChanged(object sender, EventArgs e)
		{
			RadioButton radioButton = (RadioButton)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.ForwardStrokeGame = radioButton.Checked;
			}
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0000294D File Offset: 0x00000B4D
		private void but_SelStartGameBeepFile_Click(object sender, EventArgs e)
		{
			this.tB_StartGameBeepFile.Text = this.SelAudioFile();
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00012404 File Offset: 0x00010604
		private void tB_StartGameBeepFile_TextChanged(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.StartGameBeepFile = textBox.Text;
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00002960 File Offset: 0x00000B60
		private void but_SelGetGateBeepFile_Click(object sender, EventArgs e)
		{
			this.tB_GetGateBeepFile.Text = this.SelAudioFile();
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00012434 File Offset: 0x00010634
		private void tB_GetGateBeepFile_TextChanged(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.GetGateBeepFile = textBox.Text;
			}
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00002973 File Offset: 0x00000B73
		private void but_SelWhistleBeepFile_Click(object sender, EventArgs e)
		{
			this.tB_WhistleBeepFile.Text = this.SelAudioFile();
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00012524 File Offset: 0x00010724
		private void tB_WhistleBeepFile_TextChanged(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.WhistleBeepFile = textBox.Text;
			}
		}

		// Token: 0x06000156 RID: 342 RVA: 0x000125B4 File Offset: 0x000107B4
		private void chB_AutoWhistleEnable_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			if (this.int_cfg != null)
			{
				this.int_cfg.AutoWhistleEnable = checkBox.Checked;
			}
		}

		// БЛОК 7 (аудит), Б16 (сборка 5): поле _AudioPlayer удалено вместе с кнопками
		// «Прослушать» - в окне настроек звук больше не воспроизводится.

		// Token: 0x04000108 RID: 264
		public TConfig cfg;

		// Token: 0x04000109 RID: 265
		private TConfig int_cfg;
	}
}
