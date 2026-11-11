namespace Tablo
{
	// Token: 0x0200001D RID: 29
	public partial class SettingForm : global::System.Windows.Forms.Form
	{
		// Token: 0x06000157 RID: 343 RVA: 0x00002986 File Offset: 0x00000B86
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000158 RID: 344 RVA: 0x000125E4 File Offset: 0x000107E4
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Tablo.SettingForm));
			this.tabControl_MainSetting = new global::System.Windows.Forms.TabControl();
			this.tP_MainSetting = new global::System.Windows.Forms.TabPage();
			this.gBox_FontFile = new global::System.Windows.Forms.GroupBox();
			this.tB_FontFile = new global::System.Windows.Forms.TextBox();
			this.but_SelFontFile = new global::System.Windows.Forms.Button();
			this.pB_PromptColor = new global::System.Windows.Forms.PictureBox();
			this.pB_BackColor = new global::System.Windows.Forms.PictureBox();
			this.gBox_WindowPosition = new global::System.Windows.Forms.GroupBox();
			this.tB_Top = new global::System.Windows.Forms.TextBox();
			this.tB_Left = new global::System.Windows.Forms.TextBox();
			this.l_Left = new global::System.Windows.Forms.Label();
			this.gBox_SerialSetting = new global::System.Windows.Forms.GroupBox();
			this.cB_StopBits = new global::System.Windows.Forms.ComboBox();
			this.l_StopBits = new global::System.Windows.Forms.Label();
			this.cB_DataBit = new global::System.Windows.Forms.ComboBox();
			this.l_DataBit = new global::System.Windows.Forms.Label();
			this.cB_Parity = new global::System.Windows.Forms.ComboBox();
			this.l_Parity = new global::System.Windows.Forms.Label();
			this.cB_BaudRate = new global::System.Windows.Forms.ComboBox();
			this.l_BaudRate = new global::System.Windows.Forms.Label();
			this.cB_Port = new global::System.Windows.Forms.ComboBox();
			this.l_Port = new global::System.Windows.Forms.Label();
			this.chB_AutoStart = new global::System.Windows.Forms.CheckBox();
			this.chB_TopWindow = new global::System.Windows.Forms.CheckBox();
			this.but_PromptColor = new global::System.Windows.Forms.Button();
			this.l_PromptColor = new global::System.Windows.Forms.Label();
			this.but_SelBackColor = new global::System.Windows.Forms.Button();
			this.l_BackColor = new global::System.Windows.Forms.Label();
			this.tP_BeforeStart = new global::System.Windows.Forms.TabPage();
			this.gBox_BeepFile = new global::System.Windows.Forms.GroupBox();
			this.tB_BeepFile = new global::System.Windows.Forms.TextBox();
			this.but_SelBeepFile = new global::System.Windows.Forms.Button();
			this.gBox_TimeIntervalSetting = new global::System.Windows.Forms.GroupBox();
			this.dTP_TimeBeforeStart = new global::System.Windows.Forms.DateTimePicker();
			this.l_TimeSecondBeep = new global::System.Windows.Forms.Label();
			this.l_TimeFirstBeep = new global::System.Windows.Forms.Label();
			this.l_TimeBeforeFlash = new global::System.Windows.Forms.Label();
			this.l_TimeBeforeStart = new global::System.Windows.Forms.Label();
			this.gBox_StrokeBeforeStart = new global::System.Windows.Forms.GroupBox();
			this.rB_ReverceStrokeBeforeStart = new global::System.Windows.Forms.RadioButton();
			this.rB_ForwardStrokeBeforeStart = new global::System.Windows.Forms.RadioButton();
			this.pB_BeforeStartTextColor = new global::System.Windows.Forms.PictureBox();
			this.but_SelBeforeStartTextColor = new global::System.Windows.Forms.Button();
			this.l_BeforeStartTextColor = new global::System.Windows.Forms.Label();
			this.tP_MainGame = new global::System.Windows.Forms.TabPage();
			this.dTP_GameTime = new global::System.Windows.Forms.DateTimePicker();
			this.groupBox2 = new global::System.Windows.Forms.GroupBox();
			this.pB_FontColor = new global::System.Windows.Forms.PictureBox();
			this.but_SelFontColor = new global::System.Windows.Forms.Button();
			this.gBox_GetGateBeepFile = new global::System.Windows.Forms.GroupBox();
			this.tB_GetGateBeepFile = new global::System.Windows.Forms.TextBox();
			this.but_SelGetGateBeepFile = new global::System.Windows.Forms.Button();
			this.dTP_BreakTime = new global::System.Windows.Forms.DateTimePicker();
			this.l_GameTime = new global::System.Windows.Forms.Label();
			this.dTP_TimeoutTime = new global::System.Windows.Forms.DateTimePicker();
			this.chB_BreakVideoEnable = new global::System.Windows.Forms.CheckBox();
			this.l_BreakTime = new global::System.Windows.Forms.Label();
			this.chB_TimeoutVideoEnable = new global::System.Windows.Forms.CheckBox();
			this.l_TimeoutTime = new global::System.Windows.Forms.Label();
			this.gBox_StrokeGame = new global::System.Windows.Forms.GroupBox();
			this.rB_ReverceStrokeGame = new global::System.Windows.Forms.RadioButton();
			this.rB_ForwardStrokeGame = new global::System.Windows.Forms.RadioButton();
			this.gBox_StartGameBeepFile = new global::System.Windows.Forms.GroupBox();
			this.tB_StartGameBeepFile = new global::System.Windows.Forms.TextBox();
			this.but_SelStartGameBeepFile = new global::System.Windows.Forms.Button();
			this.gBox_WhistleBeepFile = new global::System.Windows.Forms.GroupBox();
			this.chB_AutoWhistleEnable = new global::System.Windows.Forms.CheckBox();
			this.tB_WhistleBeepFile = new global::System.Windows.Forms.TextBox();
			this.but_SelWhistleBeepFile = new global::System.Windows.Forms.Button();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.but_Cancel = new global::System.Windows.Forms.Button();
			this.but_OK = new global::System.Windows.Forms.Button();
			this.toolTip1 = new global::System.Windows.Forms.ToolTip(this.components);
			this.dTP_TimeBeforeFlash = new global::System.Windows.Forms.DateTimePicker();
			this.dTP_TimeFirstBeep = new global::System.Windows.Forms.DateTimePicker();
			this.dTP_TimeSecondBeep = new global::System.Windows.Forms.DateTimePicker();
			global::System.Windows.Forms.Label label = new global::System.Windows.Forms.Label();
			this.tabControl_MainSetting.SuspendLayout();
			this.tP_MainSetting.SuspendLayout();
			this.gBox_FontFile.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.pB_PromptColor).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.pB_BackColor).BeginInit();
			this.gBox_WindowPosition.SuspendLayout();
			this.gBox_SerialSetting.SuspendLayout();
			this.tP_BeforeStart.SuspendLayout();
			this.gBox_BeepFile.SuspendLayout();
			this.gBox_TimeIntervalSetting.SuspendLayout();
			this.gBox_StrokeBeforeStart.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.pB_BeforeStartTextColor).BeginInit();
			this.tP_MainGame.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.pB_FontColor).BeginInit();
			this.gBox_GetGateBeepFile.SuspendLayout();
			this.gBox_StrokeGame.SuspendLayout();
			this.gBox_StartGameBeepFile.SuspendLayout();
			this.gBox_WhistleBeepFile.SuspendLayout();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			label.AutoSize = true;
			label.Location = new global::System.Drawing.Point(6, 43);
			label.Name = "l_Top";
			label.Size = new global::System.Drawing.Size(135, 13);
			label.TabIndex = 2;
			label.Text = "Отступ от верхнего края:";
			this.tabControl_MainSetting.Controls.Add(this.tP_MainSetting);
			this.tabControl_MainSetting.Controls.Add(this.tP_BeforeStart);
			this.tabControl_MainSetting.Controls.Add(this.tP_MainGame);
			this.tabControl_MainSetting.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.tabControl_MainSetting.Location = new global::System.Drawing.Point(0, 0);
			this.tabControl_MainSetting.Name = "tabControl_MainSetting";
			this.tabControl_MainSetting.SelectedIndex = 0;
			this.tabControl_MainSetting.Size = new global::System.Drawing.Size(471, 308);
			this.tabControl_MainSetting.TabIndex = 0;
			this.tP_MainSetting.Controls.Add(this.gBox_FontFile);
			this.tP_MainSetting.Controls.Add(this.pB_PromptColor);
			this.tP_MainSetting.Controls.Add(this.pB_BackColor);
			this.tP_MainSetting.Controls.Add(this.gBox_WindowPosition);
			this.tP_MainSetting.Controls.Add(this.gBox_SerialSetting);
			this.tP_MainSetting.Controls.Add(this.chB_AutoStart);
			this.tP_MainSetting.Controls.Add(this.chB_TopWindow);
			this.tP_MainSetting.Controls.Add(this.but_PromptColor);
			this.tP_MainSetting.Controls.Add(this.l_PromptColor);
			this.tP_MainSetting.Controls.Add(this.but_SelBackColor);
			this.tP_MainSetting.Controls.Add(this.l_BackColor);
			this.tP_MainSetting.Location = new global::System.Drawing.Point(4, 22);
			this.tP_MainSetting.Name = "tP_MainSetting";
			this.tP_MainSetting.Padding = new global::System.Windows.Forms.Padding(3);
			this.tP_MainSetting.Size = new global::System.Drawing.Size(463, 282);
			this.tP_MainSetting.TabIndex = 0;
			this.tP_MainSetting.Text = "Основные настройки";
			this.tP_MainSetting.UseVisualStyleBackColor = true;
			this.gBox_FontFile.Controls.Add(this.tB_FontFile);
			this.gBox_FontFile.Controls.Add(this.but_SelFontFile);
			this.gBox_FontFile.Location = new global::System.Drawing.Point(8, 202);
			this.gBox_FontFile.Name = "gBox_FontFile";
			this.gBox_FontFile.Size = new global::System.Drawing.Size(445, 49);
			this.gBox_FontFile.TabIndex = 28;
			this.gBox_FontFile.TabStop = false;
			this.gBox_FontFile.Text = "Файл шрифта";
			this.tB_FontFile.Location = new global::System.Drawing.Point(6, 19);
			this.tB_FontFile.Name = "tB_FontFile";
			this.tB_FontFile.Size = new global::System.Drawing.Size(352, 20);
			this.tB_FontFile.TabIndex = 28;
			this.tB_FontFile.TextChanged += new global::System.EventHandler(this.tB_FontFile_TextChanged);
			this.but_SelFontFile.Location = new global::System.Drawing.Point(364, 18);
			this.but_SelFontFile.Name = "but_SelFontFile";
			this.but_SelFontFile.Size = new global::System.Drawing.Size(75, 23);
			this.but_SelFontFile.TabIndex = 27;
			this.but_SelFontFile.Text = "Выбрать";
			this.but_SelFontFile.UseVisualStyleBackColor = true;
			this.but_SelFontFile.Click += new global::System.EventHandler(this.but_SelFontFile_Click);
			this.pB_PromptColor.BackColor = global::System.Drawing.Color.White;
			this.pB_PromptColor.BorderStyle = global::System.Windows.Forms.BorderStyle.FixedSingle;
			this.pB_PromptColor.Location = new global::System.Drawing.Point(99, 48);
			this.pB_PromptColor.Name = "pB_PromptColor";
			this.pB_PromptColor.Size = new global::System.Drawing.Size(25, 25);
			this.pB_PromptColor.TabIndex = 13;
			this.pB_PromptColor.TabStop = false;
			this.pB_PromptColor.BackColorChanged += new global::System.EventHandler(this.pB_PromptColor_BackColorChanged);
			this.pB_BackColor.BackColor = global::System.Drawing.Color.Black;
			this.pB_BackColor.BorderStyle = global::System.Windows.Forms.BorderStyle.FixedSingle;
			this.pB_BackColor.Location = new global::System.Drawing.Point(99, 14);
			this.pB_BackColor.Name = "pB_BackColor";
			this.pB_BackColor.Size = new global::System.Drawing.Size(25, 25);
			this.pB_BackColor.TabIndex = 11;
			this.pB_BackColor.TabStop = false;
			this.pB_BackColor.BackColorChanged += new global::System.EventHandler(this.pB_BackColor_BackColorChanged);
			this.gBox_WindowPosition.Controls.Add(this.tB_Top);
			this.gBox_WindowPosition.Controls.Add(label);
			this.gBox_WindowPosition.Controls.Add(this.tB_Left);
			this.gBox_WindowPosition.Controls.Add(this.l_Left);
			this.gBox_WindowPosition.Location = new global::System.Drawing.Point(8, 125);
			this.gBox_WindowPosition.Name = "gBox_WindowPosition";
			this.gBox_WindowPosition.Size = new global::System.Drawing.Size(204, 71);
			this.gBox_WindowPosition.TabIndex = 9;
			this.gBox_WindowPosition.TabStop = false;
			this.gBox_WindowPosition.Text = "Положение окна";
			this.tB_Top.Location = new global::System.Drawing.Point(145, 40);
			this.tB_Top.Name = "tB_Top";
			this.tB_Top.Size = new global::System.Drawing.Size(53, 20);
			this.tB_Top.TabIndex = 3;
			this.tB_Top.TextChanged += new global::System.EventHandler(this.tB_Top_TextChanged);
			this.tB_Top.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.tB_Left_KeyPress);
			this.tB_Left.Location = new global::System.Drawing.Point(145, 14);
			this.tB_Left.Name = "tB_Left";
			this.tB_Left.Size = new global::System.Drawing.Size(53, 20);
			this.tB_Left.TabIndex = 1;
			this.tB_Left.TextChanged += new global::System.EventHandler(this.tB_Left_TextChanged);
			this.tB_Left.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.tB_Left_KeyPress);
			this.l_Left.AutoSize = true;
			this.l_Left.Location = new global::System.Drawing.Point(6, 17);
			this.l_Left.Name = "l_Left";
			this.l_Left.Size = new global::System.Drawing.Size(124, 13);
			this.l_Left.TabIndex = 0;
			this.l_Left.Text = "Отступ от левого края:";
			this.gBox_SerialSetting.Controls.Add(this.cB_StopBits);
			this.gBox_SerialSetting.Controls.Add(this.l_StopBits);
			this.gBox_SerialSetting.Controls.Add(this.cB_DataBit);
			this.gBox_SerialSetting.Controls.Add(this.l_DataBit);
			this.gBox_SerialSetting.Controls.Add(this.cB_Parity);
			this.gBox_SerialSetting.Controls.Add(this.l_Parity);
			this.gBox_SerialSetting.Controls.Add(this.cB_BaudRate);
			this.gBox_SerialSetting.Controls.Add(this.l_BaudRate);
			this.gBox_SerialSetting.Controls.Add(this.cB_Port);
			this.gBox_SerialSetting.Controls.Add(this.l_Port);
			this.gBox_SerialSetting.Location = new global::System.Drawing.Point(218, 6);
			this.gBox_SerialSetting.Name = "gBox_SerialSetting";
			this.gBox_SerialSetting.Size = new global::System.Drawing.Size(237, 190);
			this.gBox_SerialSetting.TabIndex = 8;
			this.gBox_SerialSetting.TabStop = false;
			this.gBox_SerialSetting.Text = "Порт связи с БОС";
			this.cB_StopBits.FormattingEnabled = true;
			this.cB_StopBits.Location = new global::System.Drawing.Point(110, 111);
			this.cB_StopBits.Name = "cB_StopBits";
			this.cB_StopBits.Size = new global::System.Drawing.Size(121, 21);
			this.cB_StopBits.TabIndex = 9;
			this.cB_StopBits.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.cB_Port_KeyPress);
			this.cB_StopBits.TextChanged += new global::System.EventHandler(this.cB_StopBits_TextChanged);
			this.l_StopBits.AutoSize = true;
			this.l_StopBits.Location = new global::System.Drawing.Point(6, 115);
			this.l_StopBits.Name = "l_StopBits";
			this.l_StopBits.Size = new global::System.Drawing.Size(88, 13);
			this.l_StopBits.TabIndex = 8;
			this.l_StopBits.Text = "Стоповые биты:";
			this.cB_DataBit.FormattingEnabled = true;
			this.cB_DataBit.Items.AddRange(new object[]
			{
				"5",
				"6",
				"7",
				"8"
			});
			this.cB_DataBit.Location = new global::System.Drawing.Point(110, 87);
			this.cB_DataBit.Name = "cB_DataBit";
			this.cB_DataBit.Size = new global::System.Drawing.Size(121, 21);
			this.cB_DataBit.TabIndex = 7;
			this.cB_DataBit.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.cB_Port_KeyPress);
			this.cB_DataBit.TextChanged += new global::System.EventHandler(this.cB_DataBit_TextChanged);
			this.l_DataBit.AutoSize = true;
			this.l_DataBit.Location = new global::System.Drawing.Point(18, 91);
			this.l_DataBit.Name = "l_DataBit";
			this.l_DataBit.Size = new global::System.Drawing.Size(76, 13);
			this.l_DataBit.TabIndex = 6;
			this.l_DataBit.Text = "Биты данных:";
			this.cB_Parity.FormattingEnabled = true;
			this.cB_Parity.Location = new global::System.Drawing.Point(110, 63);
			this.cB_Parity.Name = "cB_Parity";
			this.cB_Parity.Size = new global::System.Drawing.Size(121, 21);
			this.cB_Parity.TabIndex = 5;
			this.cB_Parity.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.cB_Port_KeyPress);
			this.cB_Parity.TextChanged += new global::System.EventHandler(this.cB_Parity_TextChanged);
			this.l_Parity.AutoSize = true;
			this.l_Parity.Location = new global::System.Drawing.Point(36, 67);
			this.l_Parity.Name = "l_Parity";
			this.l_Parity.Size = new global::System.Drawing.Size(58, 13);
			this.l_Parity.TabIndex = 4;
			this.l_Parity.Text = "Четность:";
			this.cB_BaudRate.FormattingEnabled = true;
			this.cB_BaudRate.Items.AddRange(new object[]
			{
				"1200",
				"2400",
				"4800",
				"9600",
				"14400",
				"19200",
				"38400",
				"57600",
				"115200"
			});
			this.cB_BaudRate.Location = new global::System.Drawing.Point(110, 39);
			this.cB_BaudRate.Name = "cB_BaudRate";
			this.cB_BaudRate.Size = new global::System.Drawing.Size(121, 21);
			this.cB_BaudRate.TabIndex = 3;
			this.cB_BaudRate.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.cB_Port_KeyPress);
			this.cB_BaudRate.TextChanged += new global::System.EventHandler(this.cB_BaudRate_TextChanged);
			this.l_BaudRate.AutoSize = true;
			this.l_BaudRate.Location = new global::System.Drawing.Point(36, 43);
			this.l_BaudRate.Name = "l_BaudRate";
			this.l_BaudRate.Size = new global::System.Drawing.Size(58, 13);
			this.l_BaudRate.TabIndex = 2;
			this.l_BaudRate.Text = "Скорость:";
			this.cB_Port.FormattingEnabled = true;
			this.cB_Port.Location = new global::System.Drawing.Point(110, 15);
			this.cB_Port.Name = "cB_Port";
			this.cB_Port.Size = new global::System.Drawing.Size(121, 21);
			this.cB_Port.TabIndex = 1;
			this.cB_Port.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.cB_Port_KeyPress);
			this.cB_Port.TextChanged += new global::System.EventHandler(this.cB_Port_TextChanged);
			this.l_Port.AutoSize = true;
			this.l_Port.Location = new global::System.Drawing.Point(59, 19);
			this.l_Port.Name = "l_Port";
			this.l_Port.Size = new global::System.Drawing.Size(35, 13);
			this.l_Port.TabIndex = 0;
			this.l_Port.Text = "Порт:";
			this.chB_AutoStart.AutoSize = true;
			this.chB_AutoStart.Location = new global::System.Drawing.Point(8, 102);
			this.chB_AutoStart.Name = "chB_AutoStart";
			this.chB_AutoStart.Size = new global::System.Drawing.Size(150, 17);
			this.chB_AutoStart.TabIndex = 7;
			this.chB_AutoStart.Text = "Автозапуск приложения";
			this.chB_AutoStart.UseVisualStyleBackColor = true;
			this.chB_AutoStart.CheckedChanged += new global::System.EventHandler(this.chB_AutoStart_CheckedChanged);
			this.chB_TopWindow.AutoSize = true;
			this.chB_TopWindow.Location = new global::System.Drawing.Point(8, 79);
			this.chB_TopWindow.Name = "chB_TopWindow";
			this.chB_TopWindow.Size = new global::System.Drawing.Size(116, 17);
			this.chB_TopWindow.TabIndex = 6;
			this.chB_TopWindow.Text = "Поверх всех окон";
			this.chB_TopWindow.UseVisualStyleBackColor = true;
			this.chB_TopWindow.CheckedChanged += new global::System.EventHandler(this.chB_TopWindow_CheckedChanged);
			this.but_PromptColor.Location = new global::System.Drawing.Point(137, 49);
			this.but_PromptColor.Name = "but_PromptColor";
			this.but_PromptColor.Size = new global::System.Drawing.Size(75, 23);
			this.but_PromptColor.TabIndex = 5;
			this.but_PromptColor.Text = "Выбрать";
			this.but_PromptColor.UseVisualStyleBackColor = true;
			this.but_PromptColor.Click += new global::System.EventHandler(this.but_PromptColor_Click);
			this.l_PromptColor.AutoSize = true;
			this.l_PromptColor.Location = new global::System.Drawing.Point(8, 54);
			this.l_PromptColor.Name = "l_PromptColor";
			this.l_PromptColor.Size = new global::System.Drawing.Size(85, 13);
			this.l_PromptColor.TabIndex = 4;
			this.l_PromptColor.Text = "Цвет заставки:";
			this.but_SelBackColor.Location = new global::System.Drawing.Point(137, 15);
			this.but_SelBackColor.Name = "but_SelBackColor";
			this.but_SelBackColor.Size = new global::System.Drawing.Size(75, 23);
			this.but_SelBackColor.TabIndex = 1;
			this.but_SelBackColor.Text = "Выбрать";
			this.but_SelBackColor.UseVisualStyleBackColor = true;
			this.but_SelBackColor.Click += new global::System.EventHandler(this.but_SelBackColor_Click);
			this.l_BackColor.AutoSize = true;
			this.l_BackColor.Location = new global::System.Drawing.Point(29, 20);
			this.l_BackColor.Name = "l_BackColor";
			this.l_BackColor.Size = new global::System.Drawing.Size(64, 13);
			this.l_BackColor.TabIndex = 0;
			this.l_BackColor.Text = "Цвет фона:";
			this.tP_BeforeStart.Controls.Add(this.gBox_BeepFile);
			this.tP_BeforeStart.Controls.Add(this.gBox_TimeIntervalSetting);
			this.tP_BeforeStart.Controls.Add(this.gBox_StrokeBeforeStart);
			this.tP_BeforeStart.Controls.Add(this.pB_BeforeStartTextColor);
			this.tP_BeforeStart.Controls.Add(this.but_SelBeforeStartTextColor);
			this.tP_BeforeStart.Controls.Add(this.l_BeforeStartTextColor);
			this.tP_BeforeStart.Location = new global::System.Drawing.Point(4, 22);
			this.tP_BeforeStart.Name = "tP_BeforeStart";
			this.tP_BeforeStart.Padding = new global::System.Windows.Forms.Padding(3);
			this.tP_BeforeStart.Size = new global::System.Drawing.Size(463, 282);
			this.tP_BeforeStart.TabIndex = 1;
			this.tP_BeforeStart.Text = "Предстартовый таймер";
			this.tP_BeforeStart.UseVisualStyleBackColor = true;
			this.gBox_BeepFile.Controls.Add(this.tB_BeepFile);
			this.gBox_BeepFile.Controls.Add(this.but_SelBeepFile);
			this.gBox_BeepFile.Location = new global::System.Drawing.Point(10, 188);
			this.gBox_BeepFile.Name = "gBox_BeepFile";
			this.gBox_BeepFile.Size = new global::System.Drawing.Size(445, 49);
			this.gBox_BeepFile.TabIndex = 27;
			this.gBox_BeepFile.TabStop = false;
			this.gBox_BeepFile.Text = "Звуковой файл";
			this.tB_BeepFile.Location = new global::System.Drawing.Point(6, 19);
			this.tB_BeepFile.Name = "tB_BeepFile";
			this.tB_BeepFile.Size = new global::System.Drawing.Size(321, 20);
			this.tB_BeepFile.TabIndex = 28;
			this.tB_BeepFile.TextChanged += new global::System.EventHandler(this.tB_BeepFile_TextChanged);
			this.but_SelBeepFile.Location = new global::System.Drawing.Point(333, 17);
			this.but_SelBeepFile.Name = "but_SelBeepFile";
			this.but_SelBeepFile.Size = new global::System.Drawing.Size(75, 23);
			this.but_SelBeepFile.TabIndex = 27;
			this.but_SelBeepFile.Text = "Выбрать";
			this.but_SelBeepFile.UseVisualStyleBackColor = true;
			this.but_SelBeepFile.Click += new global::System.EventHandler(this.but_SelBeepFile_Click);
			this.gBox_TimeIntervalSetting.Controls.Add(this.dTP_TimeSecondBeep);
			this.gBox_TimeIntervalSetting.Controls.Add(this.dTP_TimeFirstBeep);
			this.gBox_TimeIntervalSetting.Controls.Add(this.dTP_TimeBeforeFlash);
			this.gBox_TimeIntervalSetting.Controls.Add(this.dTP_TimeBeforeStart);
			this.gBox_TimeIntervalSetting.Controls.Add(this.l_TimeSecondBeep);
			this.gBox_TimeIntervalSetting.Controls.Add(this.l_TimeFirstBeep);
			this.gBox_TimeIntervalSetting.Controls.Add(this.l_TimeBeforeFlash);
			this.gBox_TimeIntervalSetting.Controls.Add(this.l_TimeBeforeStart);
			this.gBox_TimeIntervalSetting.Location = new global::System.Drawing.Point(10, 38);
			this.gBox_TimeIntervalSetting.Name = "gBox_TimeIntervalSetting";
			this.gBox_TimeIntervalSetting.Size = new global::System.Drawing.Size(287, 144);
			this.gBox_TimeIntervalSetting.TabIndex = 24;
			this.gBox_TimeIntervalSetting.TabStop = false;
			this.gBox_TimeIntervalSetting.Text = "Временные интервалы";
			this.dTP_TimeBeforeStart.CustomFormat = "HH:mm:ss";
			this.dTP_TimeBeforeStart.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dTP_TimeBeforeStart.Format = global::System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dTP_TimeBeforeStart.Location = new global::System.Drawing.Point(195, 12);
			this.dTP_TimeBeforeStart.Name = "dTP_TimeBeforeStart";
			this.dTP_TimeBeforeStart.ShowUpDown = true;
			this.dTP_TimeBeforeStart.Size = new global::System.Drawing.Size(86, 26);
			this.dTP_TimeBeforeStart.TabIndex = 43;
			this.dTP_TimeBeforeStart.ValueChanged += new global::System.EventHandler(this.dTP_TimeBeforeStart_ValueChanged);
			this.l_TimeSecondBeep.AutoSize = true;
			this.l_TimeSecondBeep.Location = new global::System.Drawing.Point(6, 115);
			this.l_TimeSecondBeep.Name = "l_TimeSecondBeep";
			this.l_TimeSecondBeep.Size = new global::System.Drawing.Size(168, 13);
			this.l_TimeSecondBeep.TabIndex = 31;
			this.l_TimeSecondBeep.Text = "Время подачи второго сигнала:";
			this.l_TimeFirstBeep.AutoSize = true;
			this.l_TimeFirstBeep.Location = new global::System.Drawing.Point(6, 83);
			this.l_TimeFirstBeep.Name = "l_TimeFirstBeep";
			this.l_TimeFirstBeep.Size = new global::System.Drawing.Size(169, 13);
			this.l_TimeFirstBeep.TabIndex = 29;
			this.l_TimeFirstBeep.Text = "Время подачи первого сигнала:";
			this.l_TimeBeforeFlash.AutoSize = true;
			this.l_TimeBeforeFlash.Location = new global::System.Drawing.Point(6, 51);
			this.l_TimeBeforeFlash.Name = "l_TimeBeforeFlash";
			this.l_TimeBeforeFlash.Size = new global::System.Drawing.Size(61, 13);
			this.l_TimeBeforeFlash.TabIndex = 27;
			this.l_TimeBeforeFlash.Text = "Отсечка 1:";
			this.l_TimeBeforeStart.AutoSize = true;
			this.l_TimeBeforeStart.Location = new global::System.Drawing.Point(6, 19);
			this.l_TimeBeforeStart.Name = "l_TimeBeforeStart";
			this.l_TimeBeforeStart.Size = new global::System.Drawing.Size(173, 13);
			this.l_TimeBeforeStart.TabIndex = 25;
			this.l_TimeBeforeStart.Text = "Время предстартового таймера:";
			this.gBox_StrokeBeforeStart.Controls.Add(this.rB_ReverceStrokeBeforeStart);
			this.gBox_StrokeBeforeStart.Controls.Add(this.rB_ForwardStrokeBeforeStart);
			this.gBox_StrokeBeforeStart.Location = new global::System.Drawing.Point(303, 38);
			this.gBox_StrokeBeforeStart.Name = "gBox_StrokeBeforeStart";
			this.gBox_StrokeBeforeStart.Size = new global::System.Drawing.Size(152, 67);
			this.gBox_StrokeBeforeStart.TabIndex = 15;
			this.gBox_StrokeBeforeStart.TabStop = false;
			this.gBox_StrokeBeforeStart.Text = "Ход отложенного старта";
			this.rB_ReverceStrokeBeforeStart.AutoSize = true;
			this.rB_ReverceStrokeBeforeStart.Location = new global::System.Drawing.Point(22, 42);
			this.rB_ReverceStrokeBeforeStart.Name = "rB_ReverceStrokeBeforeStart";
			this.rB_ReverceStrokeBeforeStart.Size = new global::System.Drawing.Size(74, 17);
			this.rB_ReverceStrokeBeforeStart.TabIndex = 1;
			this.rB_ReverceStrokeBeforeStart.Text = "обратный";
			this.rB_ReverceStrokeBeforeStart.UseVisualStyleBackColor = true;
			this.rB_ForwardStrokeBeforeStart.AutoSize = true;
			this.rB_ForwardStrokeBeforeStart.Checked = true;
			this.rB_ForwardStrokeBeforeStart.Location = new global::System.Drawing.Point(22, 19);
			this.rB_ForwardStrokeBeforeStart.Name = "rB_ForwardStrokeBeforeStart";
			this.rB_ForwardStrokeBeforeStart.Size = new global::System.Drawing.Size(63, 17);
			this.rB_ForwardStrokeBeforeStart.TabIndex = 0;
			this.rB_ForwardStrokeBeforeStart.TabStop = true;
			this.rB_ForwardStrokeBeforeStart.Text = "прямой";
			this.rB_ForwardStrokeBeforeStart.UseVisualStyleBackColor = true;
			this.rB_ForwardStrokeBeforeStart.CheckedChanged += new global::System.EventHandler(this.rB_ForwardStrokeBeforeStart_CheckedChanged);
			this.pB_BeforeStartTextColor.BackColor = global::System.Drawing.Color.White;
			this.pB_BeforeStartTextColor.BorderStyle = global::System.Windows.Forms.BorderStyle.FixedSingle;
			this.pB_BeforeStartTextColor.Location = new global::System.Drawing.Point(236, 8);
			this.pB_BeforeStartTextColor.Name = "pB_BeforeStartTextColor";
			this.pB_BeforeStartTextColor.Size = new global::System.Drawing.Size(25, 25);
			this.pB_BeforeStartTextColor.TabIndex = 14;
			this.pB_BeforeStartTextColor.TabStop = false;
			this.pB_BeforeStartTextColor.BackColorChanged += new global::System.EventHandler(this.pB_BeforeStartTextColor_BackColorChanged);
			this.but_SelBeforeStartTextColor.Location = new global::System.Drawing.Point(271, 9);
			this.but_SelBeforeStartTextColor.Name = "but_SelBeforeStartTextColor";
			this.but_SelBeforeStartTextColor.Size = new global::System.Drawing.Size(75, 23);
			this.but_SelBeforeStartTextColor.TabIndex = 13;
			this.but_SelBeforeStartTextColor.Text = "Выбрать";
			this.but_SelBeforeStartTextColor.UseVisualStyleBackColor = true;
			this.but_SelBeforeStartTextColor.Click += new global::System.EventHandler(this.but_SelBeforeStartTextColor_Click);
			this.l_BeforeStartTextColor.AutoSize = true;
			this.l_BeforeStartTextColor.Location = new global::System.Drawing.Point(8, 14);
			this.l_BeforeStartTextColor.Name = "l_BeforeStartTextColor";
			this.l_BeforeStartTextColor.Size = new global::System.Drawing.Size(225, 13);
			this.l_BeforeStartTextColor.TabIndex = 12;
			this.l_BeforeStartTextColor.Text = "Цвет текста времени отложенного старта:";
			this.tP_MainGame.Controls.Add(this.dTP_GameTime);
			this.tP_MainGame.Controls.Add(this.groupBox2);
			this.tP_MainGame.Controls.Add(this.gBox_GetGateBeepFile);
			this.tP_MainGame.Controls.Add(this.dTP_BreakTime);
			this.tP_MainGame.Controls.Add(this.l_GameTime);
			this.tP_MainGame.Controls.Add(this.dTP_TimeoutTime);
			this.tP_MainGame.Controls.Add(this.chB_BreakVideoEnable);
			this.tP_MainGame.Controls.Add(this.l_BreakTime);
			this.tP_MainGame.Controls.Add(this.chB_TimeoutVideoEnable);
			this.tP_MainGame.Controls.Add(this.l_TimeoutTime);
			this.tP_MainGame.Controls.Add(this.gBox_StrokeGame);
			this.tP_MainGame.Controls.Add(this.gBox_StartGameBeepFile);
			this.tP_MainGame.Controls.Add(this.gBox_WhistleBeepFile);
			this.tP_MainGame.Location = new global::System.Drawing.Point(4, 22);
			this.tP_MainGame.Name = "tP_MainGame";
			this.tP_MainGame.Size = new global::System.Drawing.Size(463, 282);
			this.tP_MainGame.TabIndex = 2;
			this.tP_MainGame.Text = "Основная игра";
			this.tP_MainGame.UseVisualStyleBackColor = true;
			this.dTP_GameTime.CustomFormat = "HH:mm:ss";
			this.dTP_GameTime.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dTP_GameTime.Format = global::System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dTP_GameTime.Location = new global::System.Drawing.Point(102, 3);
			this.dTP_GameTime.Name = "dTP_GameTime";
			this.dTP_GameTime.ShowUpDown = true;
			this.dTP_GameTime.Size = new global::System.Drawing.Size(87, 26);
			this.dTP_GameTime.TabIndex = 45;
			this.dTP_GameTime.ValueChanged += new global::System.EventHandler(this.dTP_GameTime_ValueChanged);
			this.groupBox2.Controls.Add(this.pB_FontColor);
			this.groupBox2.Controls.Add(this.but_SelFontColor);
			this.groupBox2.Location = new global::System.Drawing.Point(195, 51);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new global::System.Drawing.Size(88, 61);
			this.groupBox2.TabIndex = 44;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Цвет текста";
			this.pB_FontColor.BackColor = global::System.Drawing.Color.White;
			this.pB_FontColor.Location = new global::System.Drawing.Point(29, 12);
			this.pB_FontColor.Name = "pB_FontColor";
			this.pB_FontColor.Size = new global::System.Drawing.Size(25, 25);
			this.pB_FontColor.TabIndex = 17;
			this.pB_FontColor.TabStop = false;
			this.but_SelFontColor.Location = new global::System.Drawing.Point(6, 38);
			this.but_SelFontColor.Name = "but_SelFontColor";
			this.but_SelFontColor.Size = new global::System.Drawing.Size(75, 20);
			this.but_SelFontColor.TabIndex = 16;
			this.but_SelFontColor.Text = "Выбрать";
			this.but_SelFontColor.UseVisualStyleBackColor = true;
			this.gBox_GetGateBeepFile.Controls.Add(this.tB_GetGateBeepFile);
			this.gBox_GetGateBeepFile.Controls.Add(this.but_SelGetGateBeepFile);
			this.gBox_GetGateBeepFile.Location = new global::System.Drawing.Point(8, 161);
			this.gBox_GetGateBeepFile.Name = "gBox_GetGateBeepFile";
			this.gBox_GetGateBeepFile.Size = new global::System.Drawing.Size(445, 49);
			this.gBox_GetGateBeepFile.TabIndex = 43;
			this.gBox_GetGateBeepFile.TabStop = false;
			this.gBox_GetGateBeepFile.Text = "Звуковой файл взятие ворот";
			this.tB_GetGateBeepFile.Location = new global::System.Drawing.Point(6, 19);
			this.tB_GetGateBeepFile.Name = "tB_GetGateBeepFile";
			this.tB_GetGateBeepFile.Size = new global::System.Drawing.Size(318, 20);
			this.tB_GetGateBeepFile.TabIndex = 28;
			this.tB_GetGateBeepFile.TextChanged += new global::System.EventHandler(this.tB_GetGateBeepFile_TextChanged);
			this.but_SelGetGateBeepFile.Location = new global::System.Drawing.Point(331, 17);
			this.but_SelGetGateBeepFile.Name = "but_SelGetGateBeepFile";
			this.but_SelGetGateBeepFile.Size = new global::System.Drawing.Size(75, 23);
			this.but_SelGetGateBeepFile.TabIndex = 27;
			this.but_SelGetGateBeepFile.Text = "Выбрать";
			this.but_SelGetGateBeepFile.UseVisualStyleBackColor = true;
			this.but_SelGetGateBeepFile.Click += new global::System.EventHandler(this.but_SelGetGateBeepFile_Click);
			this.dTP_BreakTime.CustomFormat = "HH:mm:ss";
			this.dTP_BreakTime.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dTP_BreakTime.Format = global::System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dTP_BreakTime.Location = new global::System.Drawing.Point(102, 67);
			this.dTP_BreakTime.Name = "dTP_BreakTime";
			this.dTP_BreakTime.ShowUpDown = true;
			this.dTP_BreakTime.Size = new global::System.Drawing.Size(87, 26);
			this.dTP_BreakTime.TabIndex = 42;
			this.dTP_BreakTime.ValueChanged += new global::System.EventHandler(this.dTP_BreakTime_ValueChanged);
			this.l_GameTime.AutoSize = true;
			this.l_GameTime.Location = new global::System.Drawing.Point(3, 10);
			this.l_GameTime.Name = "l_GameTime";
			this.l_GameTime.Size = new global::System.Drawing.Size(88, 13);
			this.l_GameTime.TabIndex = 31;
			this.l_GameTime.Text = "Время периода:";
			this.dTP_TimeoutTime.CustomFormat = "HH:mm:ss";
			this.dTP_TimeoutTime.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dTP_TimeoutTime.Format = global::System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dTP_TimeoutTime.Location = new global::System.Drawing.Point(102, 35);
			this.dTP_TimeoutTime.Name = "dTP_TimeoutTime";
			this.dTP_TimeoutTime.ShowUpDown = true;
			this.dTP_TimeoutTime.Size = new global::System.Drawing.Size(87, 26);
			this.dTP_TimeoutTime.TabIndex = 41;
			this.dTP_TimeoutTime.ValueChanged += new global::System.EventHandler(this.dTP_TimeoutTime_ValueChanged);
			this.chB_BreakVideoEnable.AutoSize = true;
			this.chB_BreakVideoEnable.Location = new global::System.Drawing.Point(195, 29);
			this.chB_BreakVideoEnable.Name = "chB_BreakVideoEnable";
			this.chB_BreakVideoEnable.Size = new global::System.Drawing.Size(251, 17);
			this.chB_BreakVideoEnable.TabIndex = 38;
			this.chB_BreakVideoEnable.Text = "Разрешить показ видео во время перерыва";
			this.chB_BreakVideoEnable.UseVisualStyleBackColor = true;
			this.chB_BreakVideoEnable.CheckedChanged += new global::System.EventHandler(this.chB_BreakVideoEnable_CheckedChanged);
			this.l_BreakTime.AutoSize = true;
			this.l_BreakTime.Location = new global::System.Drawing.Point(3, 74);
			this.l_BreakTime.Name = "l_BreakTime";
			this.l_BreakTime.Size = new global::System.Drawing.Size(96, 13);
			this.l_BreakTime.TabIndex = 37;
			this.l_BreakTime.Text = "Время перерыва:";
			this.chB_TimeoutVideoEnable.AutoSize = true;
			this.chB_TimeoutVideoEnable.Location = new global::System.Drawing.Point(195, 7);
			this.chB_TimeoutVideoEnable.Name = "chB_TimeoutVideoEnable";
			this.chB_TimeoutVideoEnable.Size = new global::System.Drawing.Size(248, 17);
			this.chB_TimeoutVideoEnable.TabIndex = 35;
			this.chB_TimeoutVideoEnable.Text = "Разрешить показ видео во время таймаута";
			this.chB_TimeoutVideoEnable.UseVisualStyleBackColor = true;
			this.chB_TimeoutVideoEnable.CheckedChanged += new global::System.EventHandler(this.chB_TimeoutVideoEnable_CheckedChanged);
			this.l_TimeoutTime.AutoSize = true;
			this.l_TimeoutTime.Location = new global::System.Drawing.Point(3, 42);
			this.l_TimeoutTime.Name = "l_TimeoutTime";
			this.l_TimeoutTime.Size = new global::System.Drawing.Size(93, 13);
			this.l_TimeoutTime.TabIndex = 34;
			this.l_TimeoutTime.Text = "Время таймаута:";
			this.gBox_StrokeGame.Controls.Add(this.rB_ReverceStrokeGame);
			this.gBox_StrokeGame.Controls.Add(this.rB_ForwardStrokeGame);
			this.gBox_StrokeGame.Location = new global::System.Drawing.Point(282, 51);
			this.gBox_StrokeGame.Name = "gBox_StrokeGame";
			this.gBox_StrokeGame.Size = new global::System.Drawing.Size(173, 61);
			this.gBox_StrokeGame.TabIndex = 32;
			this.gBox_StrokeGame.TabStop = false;
			this.gBox_StrokeGame.Text = "Ход основного времени игры";
			this.rB_ReverceStrokeGame.AutoSize = true;
			this.rB_ReverceStrokeGame.Location = new global::System.Drawing.Point(22, 42);
			this.rB_ReverceStrokeGame.Name = "rB_ReverceStrokeGame";
			this.rB_ReverceStrokeGame.Size = new global::System.Drawing.Size(74, 17);
			this.rB_ReverceStrokeGame.TabIndex = 1;
			this.rB_ReverceStrokeGame.Text = "обратный";
			this.rB_ReverceStrokeGame.UseVisualStyleBackColor = true;
			this.rB_ForwardStrokeGame.AutoSize = true;
			this.rB_ForwardStrokeGame.Checked = true;
			this.rB_ForwardStrokeGame.Location = new global::System.Drawing.Point(22, 19);
			this.rB_ForwardStrokeGame.Name = "rB_ForwardStrokeGame";
			this.rB_ForwardStrokeGame.Size = new global::System.Drawing.Size(63, 17);
			this.rB_ForwardStrokeGame.TabIndex = 0;
			this.rB_ForwardStrokeGame.TabStop = true;
			this.rB_ForwardStrokeGame.Text = "прямой";
			this.rB_ForwardStrokeGame.UseVisualStyleBackColor = true;
			this.rB_ForwardStrokeGame.CheckedChanged += new global::System.EventHandler(this.rB_ForwardStrokeGame_CheckedChanged);
			this.gBox_StartGameBeepFile.Controls.Add(this.tB_StartGameBeepFile);
			this.gBox_StartGameBeepFile.Controls.Add(this.but_SelStartGameBeepFile);
			this.gBox_StartGameBeepFile.Location = new global::System.Drawing.Point(8, 111);
			this.gBox_StartGameBeepFile.Name = "gBox_StartGameBeepFile";
			this.gBox_StartGameBeepFile.Size = new global::System.Drawing.Size(445, 49);
			this.gBox_StartGameBeepFile.TabIndex = 29;
			this.gBox_StartGameBeepFile.TabStop = false;
			this.gBox_StartGameBeepFile.Text = "Звуковой файл начала игры";
			this.tB_StartGameBeepFile.Location = new global::System.Drawing.Point(6, 19);
			this.tB_StartGameBeepFile.Name = "tB_StartGameBeepFile";
			this.tB_StartGameBeepFile.Size = new global::System.Drawing.Size(318, 20);
			this.tB_StartGameBeepFile.TabIndex = 28;
			this.tB_StartGameBeepFile.TextChanged += new global::System.EventHandler(this.tB_StartGameBeepFile_TextChanged);
			this.but_SelStartGameBeepFile.Location = new global::System.Drawing.Point(331, 17);
			this.but_SelStartGameBeepFile.Name = "but_SelStartGameBeepFile";
			this.but_SelStartGameBeepFile.Size = new global::System.Drawing.Size(75, 23);
			this.but_SelStartGameBeepFile.TabIndex = 27;
			this.but_SelStartGameBeepFile.Text = "Выбрать";
			this.but_SelStartGameBeepFile.UseVisualStyleBackColor = true;
			this.but_SelStartGameBeepFile.Click += new global::System.EventHandler(this.but_SelStartGameBeepFile_Click);
			this.gBox_WhistleBeepFile.Controls.Add(this.chB_AutoWhistleEnable);
			this.gBox_WhistleBeepFile.Controls.Add(this.tB_WhistleBeepFile);
			this.gBox_WhistleBeepFile.Controls.Add(this.but_SelWhistleBeepFile);
			this.gBox_WhistleBeepFile.Location = new global::System.Drawing.Point(8, 209);
			this.gBox_WhistleBeepFile.Name = "gBox_WhistleBeepFile";
			this.gBox_WhistleBeepFile.Size = new global::System.Drawing.Size(445, 71);
			this.gBox_WhistleBeepFile.TabIndex = 28;
			this.gBox_WhistleBeepFile.TabStop = false;
			this.gBox_WhistleBeepFile.Text = "Свисток / Перерыв / Тайм-аут";
			this.chB_AutoWhistleEnable.AutoSize = true;
			this.chB_AutoWhistleEnable.Location = new global::System.Drawing.Point(6, 45);
			this.chB_AutoWhistleEnable.Name = "chB_AutoWhistleEnable";
			this.chB_AutoWhistleEnable.Size = new global::System.Drawing.Size(356, 17);
			this.chB_AutoWhistleEnable.TabIndex = 29;
			this.chB_AutoWhistleEnable.Text = "Автоматический свисток по окончании основного времени игры, перерыва, тайм-аута";
			this.chB_AutoWhistleEnable.UseVisualStyleBackColor = true;
			this.chB_AutoWhistleEnable.CheckedChanged += new global::System.EventHandler(this.chB_AutoWhistleEnable_CheckedChanged);
			this.tB_WhistleBeepFile.Location = new global::System.Drawing.Point(6, 19);
			this.tB_WhistleBeepFile.Name = "tB_WhistleBeepFile";
			this.tB_WhistleBeepFile.Size = new global::System.Drawing.Size(318, 20);
			this.tB_WhistleBeepFile.TabIndex = 28;
			this.tB_WhistleBeepFile.TextChanged += new global::System.EventHandler(this.tB_WhistleBeepFile_TextChanged);
			this.but_SelWhistleBeepFile.Location = new global::System.Drawing.Point(331, 17);
			this.but_SelWhistleBeepFile.Name = "but_SelWhistleBeepFile";
			this.but_SelWhistleBeepFile.Size = new global::System.Drawing.Size(75, 23);
			this.but_SelWhistleBeepFile.TabIndex = 27;
			this.but_SelWhistleBeepFile.Text = "Выбрать";
			this.but_SelWhistleBeepFile.UseVisualStyleBackColor = true;
			this.but_SelWhistleBeepFile.Click += new global::System.EventHandler(this.but_SelWhistleBeepFile_Click);
			this.panel1.Controls.Add(this.but_Cancel);
			this.panel1.Controls.Add(this.but_OK);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new global::System.Drawing.Point(0, 308);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(471, 33);
			this.panel1.TabIndex = 1;
			this.but_Cancel.Anchor = (global::System.Windows.Forms.AnchorStyles.Top | global::System.Windows.Forms.AnchorStyles.Right);
			this.but_Cancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.but_Cancel.Location = new global::System.Drawing.Point(343, 4);
			this.but_Cancel.Name = "but_Cancel";
			this.but_Cancel.Size = new global::System.Drawing.Size(75, 25);
			this.but_Cancel.TabIndex = 1;
			this.but_Cancel.Text = "Отменить";
			this.but_Cancel.UseVisualStyleBackColor = true;
			this.but_OK.DialogResult = global::System.Windows.Forms.DialogResult.OK;
			this.but_OK.Location = new global::System.Drawing.Point(36, 4);
			this.but_OK.Name = "but_OK";
			this.but_OK.Size = new global::System.Drawing.Size(75, 25);
			this.but_OK.TabIndex = 0;
			this.but_OK.Text = "Применить";
			this.but_OK.UseVisualStyleBackColor = true;
			this.dTP_TimeBeforeFlash.CustomFormat = "HH:mm:ss";
			this.dTP_TimeBeforeFlash.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dTP_TimeBeforeFlash.Format = global::System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dTP_TimeBeforeFlash.Location = new global::System.Drawing.Point(195, 44);
			this.dTP_TimeBeforeFlash.Name = "dTP_TimeBeforeFlash";
			this.dTP_TimeBeforeFlash.ShowUpDown = true;
			this.dTP_TimeBeforeFlash.Size = new global::System.Drawing.Size(86, 26);
			this.dTP_TimeBeforeFlash.TabIndex = 44;
			this.dTP_TimeBeforeFlash.ValueChanged += new global::System.EventHandler(this.dTP_TimeBeforeFlash_ValueChanged);
			this.dTP_TimeFirstBeep.CustomFormat = "HH:mm:ss";
			this.dTP_TimeFirstBeep.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dTP_TimeFirstBeep.Format = global::System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dTP_TimeFirstBeep.Location = new global::System.Drawing.Point(195, 76);
			this.dTP_TimeFirstBeep.Name = "dTP_TimeFirstBeep";
			this.dTP_TimeFirstBeep.ShowUpDown = true;
			this.dTP_TimeFirstBeep.Size = new global::System.Drawing.Size(86, 26);
			this.dTP_TimeFirstBeep.TabIndex = 45;
			this.dTP_TimeFirstBeep.ValueChanged += new global::System.EventHandler(this.dTP_TimeFirstBeep_ValueChanged);
			this.dTP_TimeSecondBeep.CustomFormat = "HH:mm:ss";
			this.dTP_TimeSecondBeep.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dTP_TimeSecondBeep.Format = global::System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dTP_TimeSecondBeep.Location = new global::System.Drawing.Point(195, 108);
			this.dTP_TimeSecondBeep.Name = "dTP_TimeSecondBeep";
			this.dTP_TimeSecondBeep.ShowUpDown = true;
			this.dTP_TimeSecondBeep.Size = new global::System.Drawing.Size(86, 26);
			this.dTP_TimeSecondBeep.TabIndex = 46;
			this.dTP_TimeSecondBeep.ValueChanged += new global::System.EventHandler(this.dTP_TimeSecondBeep_ValueChanged);
			base.AcceptButton = this.but_OK;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.but_Cancel;
			base.ClientSize = new global::System.Drawing.Size(471, 341);
			base.Controls.Add(this.tabControl_MainSetting);
			base.Controls.Add(this.panel1);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.Name = "SettingForm";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Настройки";
			base.Shown += new global::System.EventHandler(this.SettingForm_Shown);
			base.FormClosing += new global::System.Windows.Forms.FormClosingEventHandler(this.SettingForm_FormClosing);
			this.tabControl_MainSetting.ResumeLayout(false);
			this.tP_MainSetting.ResumeLayout(false);
			this.tP_MainSetting.PerformLayout();
			this.gBox_FontFile.ResumeLayout(false);
			this.gBox_FontFile.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.pB_PromptColor).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.pB_BackColor).EndInit();
			this.gBox_WindowPosition.ResumeLayout(false);
			this.gBox_WindowPosition.PerformLayout();
			this.gBox_SerialSetting.ResumeLayout(false);
			this.gBox_SerialSetting.PerformLayout();
			this.tP_BeforeStart.ResumeLayout(false);
			this.tP_BeforeStart.PerformLayout();
			this.gBox_BeepFile.ResumeLayout(false);
			this.gBox_BeepFile.PerformLayout();
			this.gBox_TimeIntervalSetting.ResumeLayout(false);
			this.gBox_TimeIntervalSetting.PerformLayout();
			this.gBox_StrokeBeforeStart.ResumeLayout(false);
			this.gBox_StrokeBeforeStart.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.pB_BeforeStartTextColor).EndInit();
			this.tP_MainGame.ResumeLayout(false);
			this.tP_MainGame.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			((global::System.ComponentModel.ISupportInitialize)this.pB_FontColor).EndInit();
			this.gBox_GetGateBeepFile.ResumeLayout(false);
			this.gBox_GetGateBeepFile.PerformLayout();
			this.gBox_StrokeGame.ResumeLayout(false);
			this.gBox_StrokeGame.PerformLayout();
			this.gBox_StartGameBeepFile.ResumeLayout(false);
			this.gBox_StartGameBeepFile.PerformLayout();
			this.gBox_WhistleBeepFile.ResumeLayout(false);
			this.gBox_WhistleBeepFile.PerformLayout();
			this.panel1.ResumeLayout(false);
			base.ResumeLayout(false);
		}

		// Token: 0x0400010A RID: 266
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400010B RID: 267
		private global::System.Windows.Forms.TabControl tabControl_MainSetting;

		// Token: 0x0400010C RID: 268
		private global::System.Windows.Forms.TabPage tP_MainSetting;

		// Token: 0x0400010D RID: 269
		private global::System.Windows.Forms.TabPage tP_BeforeStart;

		// Token: 0x0400010E RID: 270
		private global::System.Windows.Forms.TabPage tP_MainGame;

		// Token: 0x0400010F RID: 271
		private global::System.Windows.Forms.Label l_BackColor;

		// Token: 0x04000110 RID: 272
		private global::System.Windows.Forms.Button but_SelBackColor;

		// Token: 0x04000111 RID: 273
		private global::System.Windows.Forms.Button but_PromptColor;

		// Token: 0x04000112 RID: 274
		private global::System.Windows.Forms.Label l_PromptColor;

		// Token: 0x04000113 RID: 275
		private global::System.Windows.Forms.GroupBox gBox_SerialSetting;

		// Token: 0x04000114 RID: 276
		private global::System.Windows.Forms.ComboBox cB_StopBits;

		// Token: 0x04000115 RID: 277
		private global::System.Windows.Forms.Label l_StopBits;

		// Token: 0x04000116 RID: 278
		private global::System.Windows.Forms.ComboBox cB_DataBit;

		// Token: 0x04000117 RID: 279
		private global::System.Windows.Forms.Label l_DataBit;

		// Token: 0x04000118 RID: 280
		private global::System.Windows.Forms.ComboBox cB_Parity;

		// Token: 0x04000119 RID: 281
		private global::System.Windows.Forms.Label l_Parity;

		// Token: 0x0400011A RID: 282
		private global::System.Windows.Forms.ComboBox cB_BaudRate;

		// Token: 0x0400011B RID: 283
		private global::System.Windows.Forms.Label l_BaudRate;

		// Token: 0x0400011C RID: 284
		private global::System.Windows.Forms.ComboBox cB_Port;

		// Token: 0x0400011D RID: 285
		private global::System.Windows.Forms.Label l_Port;

		// Token: 0x0400011E RID: 286
		private global::System.Windows.Forms.GroupBox gBox_WindowPosition;

		// Token: 0x0400011F RID: 287
		private global::System.Windows.Forms.Label l_Left;

		// Token: 0x04000120 RID: 288
		private global::System.Windows.Forms.PictureBox pB_PromptColor;

		// Token: 0x04000121 RID: 289
		private global::System.Windows.Forms.PictureBox pB_BackColor;

		// Token: 0x04000122 RID: 290
		private global::System.Windows.Forms.GroupBox gBox_StrokeBeforeStart;

		// Token: 0x04000123 RID: 291
		private global::System.Windows.Forms.PictureBox pB_BeforeStartTextColor;

		// Token: 0x04000124 RID: 292
		private global::System.Windows.Forms.Button but_SelBeforeStartTextColor;

		// Token: 0x04000125 RID: 293
		private global::System.Windows.Forms.Label l_BeforeStartTextColor;

		// Token: 0x04000126 RID: 294
		private global::System.Windows.Forms.RadioButton rB_ReverceStrokeBeforeStart;

		// Token: 0x04000127 RID: 295
		private global::System.Windows.Forms.RadioButton rB_ForwardStrokeBeforeStart;

		// Token: 0x04000128 RID: 296
		private global::System.Windows.Forms.GroupBox gBox_BeepFile;

		// Token: 0x04000129 RID: 297
		private global::System.Windows.Forms.TextBox tB_BeepFile;

		// Token: 0x0400012A RID: 298
		private global::System.Windows.Forms.Button but_SelBeepFile;

		// Token: 0x0400012B RID: 299
		private global::System.Windows.Forms.GroupBox gBox_TimeIntervalSetting;

		// Token: 0x0400012C RID: 300
		private global::System.Windows.Forms.Label l_TimeSecondBeep;

		// Token: 0x0400012D RID: 301
		private global::System.Windows.Forms.Label l_TimeFirstBeep;

		// Token: 0x0400012E RID: 302
		private global::System.Windows.Forms.Label l_TimeBeforeFlash;

		// Token: 0x0400012F RID: 303
		private global::System.Windows.Forms.Label l_TimeBeforeStart;

		// Token: 0x04000130 RID: 304
		private global::System.Windows.Forms.Label l_GameTime;

		// Token: 0x04000131 RID: 305
		private global::System.Windows.Forms.GroupBox gBox_StartGameBeepFile;

		// Token: 0x04000132 RID: 306
		private global::System.Windows.Forms.TextBox tB_StartGameBeepFile;

		// Token: 0x04000133 RID: 307
		private global::System.Windows.Forms.Button but_SelStartGameBeepFile;

		// Token: 0x04000134 RID: 308
		private global::System.Windows.Forms.GroupBox gBox_WhistleBeepFile;

		// Token: 0x04000135 RID: 309
		private global::System.Windows.Forms.CheckBox chB_AutoWhistleEnable;

		// Token: 0x04000136 RID: 310
		private global::System.Windows.Forms.TextBox tB_WhistleBeepFile;

		// Token: 0x04000137 RID: 311
		private global::System.Windows.Forms.Button but_SelWhistleBeepFile;

		// Token: 0x04000138 RID: 312
		private global::System.Windows.Forms.CheckBox chB_TimeoutVideoEnable;

		// Token: 0x04000139 RID: 313
		private global::System.Windows.Forms.Label l_TimeoutTime;

		// Token: 0x0400013A RID: 314
		private global::System.Windows.Forms.GroupBox gBox_StrokeGame;

		// Token: 0x0400013B RID: 315
		private global::System.Windows.Forms.RadioButton rB_ReverceStrokeGame;

		// Token: 0x0400013C RID: 316
		private global::System.Windows.Forms.RadioButton rB_ForwardStrokeGame;

		// Token: 0x0400013D RID: 317
		private global::System.Windows.Forms.CheckBox chB_BreakVideoEnable;

		// Token: 0x0400013E RID: 318
		private global::System.Windows.Forms.Label l_BreakTime;

		// Token: 0x0400013F RID: 319
		private global::System.Windows.Forms.CheckBox chB_AutoStart;

		// Token: 0x04000140 RID: 320
		private global::System.Windows.Forms.CheckBox chB_TopWindow;

		// Token: 0x04000141 RID: 321
		private global::System.Windows.Forms.TextBox tB_Top;

		// Token: 0x04000142 RID: 322
		private global::System.Windows.Forms.TextBox tB_Left;

		// Token: 0x04000143 RID: 323
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x04000144 RID: 324
		private global::System.Windows.Forms.Button but_Cancel;

		// Token: 0x04000145 RID: 325
		private global::System.Windows.Forms.Button but_OK;

		// Token: 0x04000146 RID: 326
		private global::System.Windows.Forms.GroupBox gBox_FontFile;

		// Token: 0x04000147 RID: 327
		private global::System.Windows.Forms.TextBox tB_FontFile;

		// Token: 0x04000148 RID: 328
		private global::System.Windows.Forms.Button but_SelFontFile;

		// Token: 0x04000149 RID: 329
		private global::System.Windows.Forms.ToolTip toolTip1;

		// Token: 0x0400014A RID: 330

		// Token: 0x0400014B RID: 331

		// Token: 0x0400014C RID: 332

		// Token: 0x0400014D RID: 333

		// Token: 0x0400014E RID: 334
		private global::System.Windows.Forms.DateTimePicker dTP_TimeoutTime;

		// Token: 0x0400014F RID: 335
		private global::System.Windows.Forms.DateTimePicker dTP_TimeBeforeStart;

		// Token: 0x04000150 RID: 336
		private global::System.Windows.Forms.DateTimePicker dTP_BreakTime;

		// Token: 0x04000151 RID: 337
		private global::System.Windows.Forms.GroupBox gBox_GetGateBeepFile;

		// Token: 0x04000152 RID: 338

		// Token: 0x04000153 RID: 339
		private global::System.Windows.Forms.TextBox tB_GetGateBeepFile;

		// Token: 0x04000154 RID: 340
		private global::System.Windows.Forms.Button but_SelGetGateBeepFile;

		// Token: 0x04000155 RID: 341
		private global::System.Windows.Forms.GroupBox groupBox2;

		// Token: 0x04000156 RID: 342
		private global::System.Windows.Forms.PictureBox pB_FontColor;

		// Token: 0x04000157 RID: 343
		private global::System.Windows.Forms.Button but_SelFontColor;

		// Token: 0x04000158 RID: 344
		private global::System.Windows.Forms.DateTimePicker dTP_GameTime;

		// Token: 0x04000159 RID: 345
		private global::System.Windows.Forms.DateTimePicker dTP_TimeBeforeFlash;

		// Token: 0x0400015A RID: 346
		private global::System.Windows.Forms.DateTimePicker dTP_TimeFirstBeep;

		// Token: 0x0400015B RID: 347
		private global::System.Windows.Forms.DateTimePicker dTP_TimeSecondBeep;
	}
}
