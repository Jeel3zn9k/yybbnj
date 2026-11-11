namespace Tablo
{
	// Token: 0x02000008 RID: 8
	public partial class frmMain : global::System.Windows.Forms.Form
	{
		// Token: 0x0600002F RID: 47 RVA: 0x0000227B File Offset: 0x0000047B
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003138 File Offset: 0x00001338
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.gB_Tablo = new global::System.Windows.Forms.GroupBox();
			this.groupBox4 = new global::System.Windows.Forms.GroupBox();
			this.but_BeforeStart = new global::System.Windows.Forms.Button();
			this.gB_GameTime = new global::System.Windows.Forms.GroupBox();
			this.dTP_GameTime = new global::System.Windows.Forms.DateTimePicker();
			this.but_ShowPrompt = new global::System.Windows.Forms.Button();
			this.but_TimeReset = new global::System.Windows.Forms.Button();
			this.but_ResetGame = new global::System.Windows.Forms.Button();
			this.but_BreakGame = new global::System.Windows.Forms.Button();
			this.but_Beep = new global::System.Windows.Forms.Button();
			this.but_Timeout = new global::System.Windows.Forms.Button();
			this.but_StopGame = new global::System.Windows.Forms.Button();
			this.but_StartGame = new global::System.Windows.Forms.Button();
			this.but_ScriptPlay = new global::System.Windows.Forms.Button();
			this.statusStrip1 = new global::System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel2 = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel3 = new global::System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainer1 = new global::System.Windows.Forms.SplitContainer();
			this.gBox_VideoFileList = new global::System.Windows.Forms.GroupBox();
			this.tabControl_VideoFileList = new global::System.Windows.Forms.TabControl();
			this.tabPage_BreakList = new global::System.Windows.Forms.TabPage();
			this.cLB_VideoFiles = new global::System.Windows.Forms.ListBox();
			this.panel_VideoButtons = new global::System.Windows.Forms.Panel();
			this.but_DeleteVideo = new global::System.Windows.Forms.Button();
			this.but_VideoDown = new global::System.Windows.Forms.Button();
			this.but_VideoUp = new global::System.Windows.Forms.Button();
			this.but_AddVideo = new global::System.Windows.Forms.Button();
			this.tabPage_TimeOutList = new global::System.Windows.Forms.TabPage();
			this.cLB_TimeoutVideoFiles = new global::System.Windows.Forms.ListBox();
			this.panel2 = new global::System.Windows.Forms.Panel();
			this.but_TimeoutVideoFiles_Delete = new global::System.Windows.Forms.Button();
			this.but_TimeoutVideoFiles_Down = new global::System.Windows.Forms.Button();
			this.but_TimeoutVideoFiles_Up = new global::System.Windows.Forms.Button();
			this.gBox_ScenList = new global::System.Windows.Forms.GroupBox();
			this.cLB_ScriptFiles = new global::System.Windows.Forms.ListBox();
			this.panel_ScriptButtons = new global::System.Windows.Forms.Panel();
			this.but_DeleteScript = new global::System.Windows.Forms.Button();
			this.but_ScriptDown = new global::System.Windows.Forms.Button();
			this.but_ScriptUp = new global::System.Windows.Forms.Button();
			this.but_AddScript = new global::System.Windows.Forms.Button();
			this.Main_menu = new global::System.Windows.Forms.MenuStrip();
			this.файлToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.загрузитьСписокВидеоToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.сохранитьСписокВидеоToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.загToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.сохранитьСписокШаблоновToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.печатьСписковToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.выходToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.настройкиToolStripMenuItem = new global::System.Windows.Forms.ToolStripMenuItem();
			this.toolTip1 = new global::System.Windows.Forms.ToolTip(this.components);
			this.nUD_Score_2 = new global::System.Windows.Forms.NumericUpDown();
			this.nUD_Period = new global::System.Windows.Forms.NumericUpDown();
			this.nUD_Score_1 = new global::System.Windows.Forms.NumericUpDown();
			this.panel1 = new global::System.Windows.Forms.Panel();
			this.groupBox2 = new global::System.Windows.Forms.GroupBox();
			this.but_DelPeople_B_Add = new global::System.Windows.Forms.Button();
			this.mTB_DelPeople_B = new global::System.Windows.Forms.MaskedTextBox();
			this.lB_DelPeople_B = new global::System.Windows.Forms.ListBox();
			this.groupBox1 = new global::System.Windows.Forms.GroupBox();
			this.but_DelPeople_A_Add = new global::System.Windows.Forms.Button();
			this.mTB_DelPeople_A = new global::System.Windows.Forms.MaskedTextBox();
			this.lB_DelPeople_A = new global::System.Windows.Forms.ListBox();
			this.label4 = new global::System.Windows.Forms.Label();
			this.mtb_TownCommand_B = new global::System.Windows.Forms.MaskedTextBox();
			this.label3 = new global::System.Windows.Forms.Label();
			this.mtb_Command_B = new global::System.Windows.Forms.MaskedTextBox();
			this.label2 = new global::System.Windows.Forms.Label();
			this.mtb_TownCommand_A = new global::System.Windows.Forms.MaskedTextBox();
			this.label1 = new global::System.Windows.Forms.Label();
			this.mtb_Command_A = new global::System.Windows.Forms.MaskedTextBox();
			global::System.Windows.Forms.Button button = new global::System.Windows.Forms.Button();
			this.gB_Tablo.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.gB_GameTime.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.gBox_VideoFileList.SuspendLayout();
			this.tabControl_VideoFileList.SuspendLayout();
			this.tabPage_BreakList.SuspendLayout();
			this.panel_VideoButtons.SuspendLayout();
			this.tabPage_TimeOutList.SuspendLayout();
			this.panel2.SuspendLayout();
			this.gBox_ScenList.SuspendLayout();
			this.panel_ScriptButtons.SuspendLayout();
			this.Main_menu.SuspendLayout();
			((global::System.ComponentModel.ISupportInitialize)this.nUD_Score_2).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.nUD_Period).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.nUD_Score_1).BeginInit();
			this.panel1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			base.SuspendLayout();
			button.Location = new global::System.Drawing.Point(6, 3);
			button.Name = "but_TimeoutVideoFiles_Add";
			button.Size = new global::System.Drawing.Size(65, 23);
			button.TabIndex = 0;
			button.Text = "Добавить";
			button.UseVisualStyleBackColor = true;
			button.Click += new global::System.EventHandler(this.but_TimeoutVideoFiles_Add_Click);
			this.gB_Tablo.Controls.Add(this.groupBox4);
			this.gB_Tablo.Controls.Add(this.gB_GameTime);
			this.gB_Tablo.Controls.Add(this.but_ShowPrompt);
			this.gB_Tablo.Controls.Add(this.but_TimeReset);
			this.gB_Tablo.Controls.Add(this.but_ResetGame);
			this.gB_Tablo.Controls.Add(this.but_BreakGame);
			this.gB_Tablo.Controls.Add(this.but_Beep);
			this.gB_Tablo.Controls.Add(this.but_Timeout);
			this.gB_Tablo.Controls.Add(this.but_StopGame);
			this.gB_Tablo.Controls.Add(this.but_StartGame);
			this.gB_Tablo.Controls.Add(this.but_ScriptPlay);
			this.gB_Tablo.Dock = global::System.Windows.Forms.DockStyle.Right;
			this.gB_Tablo.Location = new global::System.Drawing.Point(890, 24);
			this.gB_Tablo.Name = "gB_Tablo";
			this.gB_Tablo.Size = new global::System.Drawing.Size(138, 339);
			this.gB_Tablo.TabIndex = 16;
			this.gB_Tablo.TabStop = false;
			this.gB_Tablo.Text = "Кнопки";
			this.groupBox4.Controls.Add(this.but_BeforeStart);
			this.groupBox4.Location = new global::System.Drawing.Point(5, 12);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new global::System.Drawing.Size(131, 48);
			this.groupBox4.TabIndex = 35;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Предстартовый т-р";
			this.but_BeforeStart.Location = new global::System.Drawing.Point(6, 19);
			this.but_BeforeStart.Name = "but_BeforeStart";
			this.but_BeforeStart.Size = new global::System.Drawing.Size(120, 23);
			this.but_BeforeStart.TabIndex = 23;
			this.but_BeforeStart.Text = "Запустить";
			this.but_BeforeStart.UseVisualStyleBackColor = true;
			this.but_BeforeStart.Click += new global::System.EventHandler(this.but_BeforeStart_Click);
			this.gB_GameTime.Controls.Add(this.dTP_GameTime);
			this.gB_GameTime.Location = new global::System.Drawing.Point(7, 292);
			this.gB_GameTime.Name = "gB_GameTime";
			this.gB_GameTime.Size = new global::System.Drawing.Size(121, 44);
			this.gB_GameTime.TabIndex = 34;
			this.gB_GameTime.TabStop = false;
			this.gB_GameTime.Text = "Время периода";
			this.gB_GameTime.Visible = false;
			this.dTP_GameTime.CustomFormat = "HH:mm:ss";
			this.dTP_GameTime.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.dTP_GameTime.Format = global::System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dTP_GameTime.Location = new global::System.Drawing.Point(6, 14);
			this.dTP_GameTime.Name = "dTP_GameTime";
			this.dTP_GameTime.ShowUpDown = true;
			this.dTP_GameTime.Size = new global::System.Drawing.Size(109, 26);
			this.dTP_GameTime.TabIndex = 0;
			this.dTP_GameTime.ValueChanged += new global::System.EventHandler(this.dTP_GameTime_ValueChanged);
			this.but_ShowPrompt.Location = new global::System.Drawing.Point(10, 66);
			this.but_ShowPrompt.Name = "but_ShowPrompt";
			this.but_ShowPrompt.Size = new global::System.Drawing.Size(120, 23);
			this.but_ShowPrompt.TabIndex = 33;
			this.but_ShowPrompt.Text = "Показать заставку";
			this.but_ShowPrompt.UseVisualStyleBackColor = true;
			this.but_ShowPrompt.Click += new global::System.EventHandler(this.but_ShowPrompt_Click);
			this.but_TimeReset.Location = new global::System.Drawing.Point(9, 268);
			this.but_TimeReset.Name = "but_TimeReset";
			this.but_TimeReset.Size = new global::System.Drawing.Size(120, 23);
			this.but_TimeReset.TabIndex = 30;
			this.but_TimeReset.Text = "Сброс времени";
			this.but_TimeReset.UseVisualStyleBackColor = true;
			this.but_TimeReset.Click += new global::System.EventHandler(this.but_TimeReset_Click);
			this.but_ResetGame.Location = new global::System.Drawing.Point(9, 243);
			this.but_ResetGame.Name = "but_ResetGame";
			this.but_ResetGame.Size = new global::System.Drawing.Size(120, 23);
			this.but_ResetGame.TabIndex = 29;
			this.but_ResetGame.Text = "Сброс матча";
			this.but_ResetGame.UseVisualStyleBackColor = true;
			this.but_ResetGame.Click += new global::System.EventHandler(this.but_ResetGame_Click);
			this.but_BreakGame.Location = new global::System.Drawing.Point(9, 218);
			this.but_BreakGame.Name = "but_BreakGame";
			this.but_BreakGame.Size = new global::System.Drawing.Size(120, 23);
			this.but_BreakGame.TabIndex = 28;
			this.but_BreakGame.Text = "Перерыв";
			this.but_BreakGame.UseVisualStyleBackColor = true;
			this.but_BreakGame.Click += new global::System.EventHandler(this.but_BreakGame_Click);
			this.but_Beep.Location = new global::System.Drawing.Point(9, 193);
			this.but_Beep.Name = "but_Beep";
			this.but_Beep.Size = new global::System.Drawing.Size(120, 23);
			this.but_Beep.TabIndex = 27;
			this.but_Beep.Text = "Свисток";
			this.but_Beep.UseVisualStyleBackColor = true;
			this.but_Beep.Click += new global::System.EventHandler(this.but_Beep_Click);
			this.but_Timeout.Location = new global::System.Drawing.Point(9, 168);
			this.but_Timeout.Name = "but_Timeout";
			this.but_Timeout.Size = new global::System.Drawing.Size(120, 23);
			this.but_Timeout.TabIndex = 26;
			this.but_Timeout.Text = "Таймаут";
			this.but_Timeout.UseVisualStyleBackColor = true;
			this.but_Timeout.Click += new global::System.EventHandler(this.but_Timeout_Click);
			this.but_StopGame.Location = new global::System.Drawing.Point(9, 143);
			this.but_StopGame.Name = "but_StopGame";
			this.but_StopGame.Size = new global::System.Drawing.Size(120, 23);
			this.but_StopGame.TabIndex = 25;
			this.but_StopGame.Text = "Стоп";
			this.but_StopGame.UseVisualStyleBackColor = true;
			this.but_StopGame.Click += new global::System.EventHandler(this.but_StopGame_Click);
			this.but_StartGame.Location = new global::System.Drawing.Point(9, 118);
			this.but_StartGame.Name = "but_StartGame";
			this.but_StartGame.Size = new global::System.Drawing.Size(120, 23);
			this.but_StartGame.TabIndex = 24;
			this.but_StartGame.Text = "Старт";
			this.but_StartGame.UseVisualStyleBackColor = true;
			this.but_StartGame.Click += new global::System.EventHandler(this.but_StartGame_Click);
			this.but_ScriptPlay.Location = new global::System.Drawing.Point(9, 93);
			this.but_ScriptPlay.Name = "but_ScriptPlay";
			this.but_ScriptPlay.Size = new global::System.Drawing.Size(121, 23);
			this.but_ScriptPlay.TabIndex = 21;
			this.but_ScriptPlay.Text = "Запуск сценария";
			this.but_ScriptPlay.UseVisualStyleBackColor = true;
			this.but_ScriptPlay.Click += new global::System.EventHandler(this.but_ScriptPlay_Click);
			this.statusStrip1.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.toolStripStatusLabel1,
				this.toolStripStatusLabel2,
				this.toolStripStatusLabel3
			});
			this.statusStrip1.Location = new global::System.Drawing.Point(0, 363);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new global::System.Drawing.Size(1028, 22);
			this.statusStrip1.TabIndex = 15;
			this.statusStrip1.Text = "statusStrip1";
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new global::System.Drawing.Size(0, 17);
			this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
			this.toolStripStatusLabel2.Size = new global::System.Drawing.Size(17, 17);
			this.toolStripStatusLabel2.Text = " | ";
			this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
			this.toolStripStatusLabel3.Size = new global::System.Drawing.Size(0, 17);
			this.splitContainer1.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new global::System.Drawing.Point(0, 24);
			this.splitContainer1.MinimumSize = new global::System.Drawing.Size(586, 266);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Panel1.Controls.Add(this.gBox_VideoFileList);
			this.splitContainer1.Panel2.Controls.Add(this.gBox_ScenList);
			this.splitContainer1.Size = new global::System.Drawing.Size(661, 339);
			this.splitContainer1.SplitterDistance = 326;
			this.splitContainer1.TabIndex = 22;
			this.gBox_VideoFileList.Controls.Add(this.tabControl_VideoFileList);
			this.gBox_VideoFileList.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.gBox_VideoFileList.Location = new global::System.Drawing.Point(0, 0);
			this.gBox_VideoFileList.Name = "gBox_VideoFileList";
			this.gBox_VideoFileList.Size = new global::System.Drawing.Size(326, 339);
			this.gBox_VideoFileList.TabIndex = 0;
			this.gBox_VideoFileList.TabStop = false;
			this.gBox_VideoFileList.Text = "Список видео файлов";
			this.tabControl_VideoFileList.Alignment = global::System.Windows.Forms.TabAlignment.Left;
			this.tabControl_VideoFileList.Controls.Add(this.tabPage_BreakList);
			this.tabControl_VideoFileList.Controls.Add(this.tabPage_TimeOutList);
			this.tabControl_VideoFileList.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.tabControl_VideoFileList.Location = new global::System.Drawing.Point(3, 16);
			this.tabControl_VideoFileList.Multiline = true;
			this.tabControl_VideoFileList.Name = "tabControl_VideoFileList";
			this.tabControl_VideoFileList.SelectedIndex = 0;
			this.tabControl_VideoFileList.Size = new global::System.Drawing.Size(320, 320);
			this.tabControl_VideoFileList.TabIndex = 1;
			this.tabPage_BreakList.Controls.Add(this.cLB_VideoFiles);
			this.tabPage_BreakList.Controls.Add(this.panel_VideoButtons);
			this.tabPage_BreakList.Location = new global::System.Drawing.Point(23, 4);
			this.tabPage_BreakList.Name = "tabPage_BreakList";
			this.tabPage_BreakList.Padding = new global::System.Windows.Forms.Padding(3);
			this.tabPage_BreakList.Size = new global::System.Drawing.Size(293, 312);
			this.tabPage_BreakList.TabIndex = 0;
			this.tabPage_BreakList.Text = "Перерыв";
			this.tabPage_BreakList.UseVisualStyleBackColor = true;
			this.cLB_VideoFiles.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.cLB_VideoFiles.DrawMode = global::System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.cLB_VideoFiles.FormattingEnabled = true;
			this.cLB_VideoFiles.Location = new global::System.Drawing.Point(3, 3);
			this.cLB_VideoFiles.Name = "cLB_VideoFiles";
			this.cLB_VideoFiles.Size = new global::System.Drawing.Size(287, 264);
			this.cLB_VideoFiles.TabIndex = 25;
			this.cLB_VideoFiles.DrawItem += new global::System.Windows.Forms.DrawItemEventHandler(this.cLB_VideoFiles_DrawItem);
			this.cLB_VideoFiles.SelectedIndexChanged += new global::System.EventHandler(this.cLB_VideoFiles_SelectedIndexChanged);
			this.panel_VideoButtons.Controls.Add(this.but_DeleteVideo);
			this.panel_VideoButtons.Controls.Add(this.but_VideoDown);
			this.panel_VideoButtons.Controls.Add(this.but_VideoUp);
			this.panel_VideoButtons.Controls.Add(this.but_AddVideo);
			this.panel_VideoButtons.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel_VideoButtons.Location = new global::System.Drawing.Point(3, 274);
			this.panel_VideoButtons.Name = "panel_VideoButtons";
			this.panel_VideoButtons.Size = new global::System.Drawing.Size(287, 35);
			this.panel_VideoButtons.TabIndex = 24;
			this.but_DeleteVideo.Location = new global::System.Drawing.Point(219, 3);
			this.but_DeleteVideo.Name = "but_DeleteVideo";
			this.but_DeleteVideo.Size = new global::System.Drawing.Size(65, 23);
			this.but_DeleteVideo.TabIndex = 0;
			this.but_DeleteVideo.Text = "Удалить";
			this.but_DeleteVideo.UseVisualStyleBackColor = true;
			this.but_DeleteVideo.Click += new global::System.EventHandler(this.but_DeleteVideo_Click);
			this.but_VideoDown.Location = new global::System.Drawing.Point(148, 3);
			this.but_VideoDown.Name = "but_VideoDown";
			this.but_VideoDown.Size = new global::System.Drawing.Size(65, 23);
			this.but_VideoDown.TabIndex = 0;
			this.but_VideoDown.Text = "Вниз";
			this.but_VideoDown.UseVisualStyleBackColor = true;
			this.but_VideoDown.Click += new global::System.EventHandler(this.but_VideoDown_Click);
			this.but_VideoUp.Location = new global::System.Drawing.Point(77, 3);
			this.but_VideoUp.Name = "but_VideoUp";
			this.but_VideoUp.Size = new global::System.Drawing.Size(65, 23);
			this.but_VideoUp.TabIndex = 0;
			this.but_VideoUp.Text = "Вверх";
			this.but_VideoUp.UseVisualStyleBackColor = true;
			this.but_VideoUp.Click += new global::System.EventHandler(this.but_VideoUp_Click);
			this.but_AddVideo.Location = new global::System.Drawing.Point(6, 3);
			this.but_AddVideo.Name = "but_AddVideo";
			this.but_AddVideo.Size = new global::System.Drawing.Size(65, 23);
			this.but_AddVideo.TabIndex = 0;
			this.but_AddVideo.Text = "Добавить";
			this.but_AddVideo.UseVisualStyleBackColor = true;
			this.but_AddVideo.Click += new global::System.EventHandler(this.but_AddVideo_Click);
			this.tabPage_TimeOutList.Controls.Add(this.cLB_TimeoutVideoFiles);
			this.tabPage_TimeOutList.Controls.Add(this.panel2);
			this.tabPage_TimeOutList.Location = new global::System.Drawing.Point(23, 4);
			this.tabPage_TimeOutList.Name = "tabPage_TimeOutList";
			this.tabPage_TimeOutList.Padding = new global::System.Windows.Forms.Padding(3);
			this.tabPage_TimeOutList.Size = new global::System.Drawing.Size(293, 312);
			this.tabPage_TimeOutList.TabIndex = 1;
			this.tabPage_TimeOutList.Text = "Таймаут";
			this.tabPage_TimeOutList.UseVisualStyleBackColor = true;
			this.cLB_TimeoutVideoFiles.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.cLB_TimeoutVideoFiles.DrawMode = global::System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.cLB_TimeoutVideoFiles.FormattingEnabled = true;
			this.cLB_TimeoutVideoFiles.Location = new global::System.Drawing.Point(3, 3);
			this.cLB_TimeoutVideoFiles.Name = "cLB_TimeoutVideoFiles";
			this.cLB_TimeoutVideoFiles.Size = new global::System.Drawing.Size(287, 264);
			this.cLB_TimeoutVideoFiles.TabIndex = 25;
			this.cLB_TimeoutVideoFiles.DrawItem += new global::System.Windows.Forms.DrawItemEventHandler(this.cLB_TimeoutVideoFiles_DrawItem);
			this.cLB_TimeoutVideoFiles.SelectedIndexChanged += new global::System.EventHandler(this.cLB_TimeoutVideoFiles_SelectedIndexChanged);
			this.panel2.Controls.Add(this.but_TimeoutVideoFiles_Delete);
			this.panel2.Controls.Add(this.but_TimeoutVideoFiles_Down);
			this.panel2.Controls.Add(this.but_TimeoutVideoFiles_Up);
			this.panel2.Controls.Add(button);
			this.panel2.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new global::System.Drawing.Point(3, 274);
			this.panel2.Name = "panel2";
			this.panel2.Size = new global::System.Drawing.Size(287, 35);
			this.panel2.TabIndex = 24;
			this.but_TimeoutVideoFiles_Delete.Location = new global::System.Drawing.Point(219, 3);
			this.but_TimeoutVideoFiles_Delete.Name = "but_TimeoutVideoFiles_Delete";
			this.but_TimeoutVideoFiles_Delete.Size = new global::System.Drawing.Size(65, 23);
			this.but_TimeoutVideoFiles_Delete.TabIndex = 0;
			this.but_TimeoutVideoFiles_Delete.Text = "Удалить";
			this.but_TimeoutVideoFiles_Delete.UseVisualStyleBackColor = true;
			this.but_TimeoutVideoFiles_Delete.Click += new global::System.EventHandler(this.but_TimeoutVideoFiles_Delete_Click);
			this.but_TimeoutVideoFiles_Down.Location = new global::System.Drawing.Point(148, 3);
			this.but_TimeoutVideoFiles_Down.Name = "but_TimeoutVideoFiles_Down";
			this.but_TimeoutVideoFiles_Down.Size = new global::System.Drawing.Size(65, 23);
			this.but_TimeoutVideoFiles_Down.TabIndex = 0;
			this.but_TimeoutVideoFiles_Down.Text = "Вниз";
			this.but_TimeoutVideoFiles_Down.UseVisualStyleBackColor = true;
			this.but_TimeoutVideoFiles_Down.Click += new global::System.EventHandler(this.but_TimeoutVideoFiles_Down_Click);
			this.but_TimeoutVideoFiles_Up.Location = new global::System.Drawing.Point(77, 3);
			this.but_TimeoutVideoFiles_Up.Name = "but_TimeoutVideoFiles_Up";
			this.but_TimeoutVideoFiles_Up.Size = new global::System.Drawing.Size(65, 23);
			this.but_TimeoutVideoFiles_Up.TabIndex = 0;
			this.but_TimeoutVideoFiles_Up.Text = "Вверх";
			this.but_TimeoutVideoFiles_Up.UseVisualStyleBackColor = true;
			this.but_TimeoutVideoFiles_Up.Click += new global::System.EventHandler(this.but_TimeoutVideoFiles_Up_Click);
			this.gBox_ScenList.Controls.Add(this.cLB_ScriptFiles);
			this.gBox_ScenList.Controls.Add(this.panel_ScriptButtons);
			this.gBox_ScenList.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.gBox_ScenList.Location = new global::System.Drawing.Point(0, 0);
			this.gBox_ScenList.Name = "gBox_ScenList";
			this.gBox_ScenList.Size = new global::System.Drawing.Size(331, 339);
			this.gBox_ScenList.TabIndex = 0;
			this.gBox_ScenList.TabStop = false;
			this.gBox_ScenList.Text = "Список сценариев";
			this.cLB_ScriptFiles.Dock = global::System.Windows.Forms.DockStyle.Fill;
			this.cLB_ScriptFiles.DrawMode = global::System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.cLB_ScriptFiles.FormattingEnabled = true;
			this.cLB_ScriptFiles.Location = new global::System.Drawing.Point(3, 16);
			this.cLB_ScriptFiles.Name = "cLB_ScriptFiles";
			this.cLB_ScriptFiles.Size = new global::System.Drawing.Size(325, 277);
			this.cLB_ScriptFiles.TabIndex = 26;
			this.cLB_ScriptFiles.DrawItem += new global::System.Windows.Forms.DrawItemEventHandler(this.cLB_ScriptFiles_DrawItem);
			this.cLB_ScriptFiles.SelectedIndexChanged += new global::System.EventHandler(this.cLB_ScriptFiles_SelectedIndexChanged);
			this.panel_ScriptButtons.Controls.Add(this.but_DeleteScript);
			this.panel_ScriptButtons.Controls.Add(this.but_ScriptDown);
			this.panel_ScriptButtons.Controls.Add(this.but_ScriptUp);
			this.panel_ScriptButtons.Controls.Add(this.but_AddScript);
			this.panel_ScriptButtons.Dock = global::System.Windows.Forms.DockStyle.Bottom;
			this.panel_ScriptButtons.Location = new global::System.Drawing.Point(3, 301);
			this.panel_ScriptButtons.Name = "panel_ScriptButtons";
			this.panel_ScriptButtons.Size = new global::System.Drawing.Size(325, 35);
			this.panel_ScriptButtons.TabIndex = 25;
			this.but_DeleteScript.Location = new global::System.Drawing.Point(219, 3);
			this.but_DeleteScript.Name = "but_DeleteScript";
			this.but_DeleteScript.Size = new global::System.Drawing.Size(65, 23);
			this.but_DeleteScript.TabIndex = 0;
			this.but_DeleteScript.Text = "Удалить";
			this.but_DeleteScript.UseVisualStyleBackColor = true;
			this.but_DeleteScript.Click += new global::System.EventHandler(this.but_DeleteScript_Click);
			this.but_ScriptDown.Location = new global::System.Drawing.Point(148, 3);
			this.but_ScriptDown.Name = "but_ScriptDown";
			this.but_ScriptDown.Size = new global::System.Drawing.Size(65, 23);
			this.but_ScriptDown.TabIndex = 0;
			this.but_ScriptDown.Text = "Вниз";
			this.but_ScriptDown.UseVisualStyleBackColor = true;
			this.but_ScriptDown.Click += new global::System.EventHandler(this.but_ScriptDown_Click);
			this.but_ScriptUp.Location = new global::System.Drawing.Point(77, 3);
			this.but_ScriptUp.Name = "but_ScriptUp";
			this.but_ScriptUp.Size = new global::System.Drawing.Size(65, 23);
			this.but_ScriptUp.TabIndex = 0;
			this.but_ScriptUp.Text = "Вверх";
			this.but_ScriptUp.UseVisualStyleBackColor = true;
			this.but_ScriptUp.Click += new global::System.EventHandler(this.but_ScriptUp_Click);
			this.but_AddScript.Location = new global::System.Drawing.Point(6, 3);
			this.but_AddScript.Name = "but_AddScript";
			this.but_AddScript.Size = new global::System.Drawing.Size(65, 23);
			this.but_AddScript.TabIndex = 0;
			this.but_AddScript.Text = "Добавить";
			this.but_AddScript.UseVisualStyleBackColor = true;
			this.but_AddScript.Click += new global::System.EventHandler(this.but_AddScript_Click);
			this.Main_menu.Items.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.файлToolStripMenuItem,
				this.настройкиToolStripMenuItem
			});
			this.Main_menu.Location = new global::System.Drawing.Point(0, 0);
			this.Main_menu.Name = "Main_menu";
			this.Main_menu.Size = new global::System.Drawing.Size(1028, 24);
			this.Main_menu.TabIndex = 23;
			this.Main_menu.Text = "menuStrip1";
			this.файлToolStripMenuItem.DropDownItems.AddRange(new global::System.Windows.Forms.ToolStripItem[]
			{
				this.загрузитьСписокВидеоToolStripMenuItem,
				this.сохранитьСписокВидеоToolStripMenuItem,
				this.загToolStripMenuItem,
				this.сохранитьСписокШаблоновToolStripMenuItem,
				this.печатьСписковToolStripMenuItem,
				this.выходToolStripMenuItem
			});
			this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
			this.файлToolStripMenuItem.Size = new global::System.Drawing.Size(45, 20);
			this.файлToolStripMenuItem.Text = "Файл";
			this.загрузитьСписокВидеоToolStripMenuItem.Name = "загрузитьСписокВидеоToolStripMenuItem";
			this.загрузитьСписокВидеоToolStripMenuItem.Size = new global::System.Drawing.Size(219, 22);
			this.загрузитьСписокВидеоToolStripMenuItem.Text = "Загрузить список видео";
			this.загрузитьСписокВидеоToolStripMenuItem.Click += new global::System.EventHandler(this.загрузитьСписокВидеоToolStripMenuItem_Click);
			this.сохранитьСписокВидеоToolStripMenuItem.Name = "сохранитьСписокВидеоToolStripMenuItem";
			this.сохранитьСписокВидеоToolStripMenuItem.Size = new global::System.Drawing.Size(219, 22);
			this.сохранитьСписокВидеоToolStripMenuItem.Text = "Сохранить список видео";
			this.сохранитьСписокВидеоToolStripMenuItem.Click += new global::System.EventHandler(this.сохранитьСписокВидеоToolStripMenuItem_Click);
			this.загToolStripMenuItem.Name = "загToolStripMenuItem";
			this.загToolStripMenuItem.Size = new global::System.Drawing.Size(219, 22);
			this.загToolStripMenuItem.Text = "Загрузить список шаблонов";
			this.загToolStripMenuItem.Click += new global::System.EventHandler(this.загToolStripMenuItem_Click);
			this.сохранитьСписокШаблоновToolStripMenuItem.Name = "сохранитьСписокШаблоновToolStripMenuItem";
			this.сохранитьСписокШаблоновToolStripMenuItem.Size = new global::System.Drawing.Size(219, 22);
			this.сохранитьСписокШаблоновToolStripMenuItem.Text = "Сохранить список шаблонов";
			this.сохранитьСписокШаблоновToolStripMenuItem.Click += new global::System.EventHandler(this.сохранитьСписокШаблоновToolStripMenuItem_Click);
			this.печатьСписковToolStripMenuItem.Name = "печатьСписковToolStripMenuItem";
			this.печатьСписковToolStripMenuItem.Size = new global::System.Drawing.Size(219, 22);
			this.печатьСписковToolStripMenuItem.Text = "Печать списков";
			this.печатьСписковToolStripMenuItem.Click += new global::System.EventHandler(this.печатьСписковToolStripMenuItem_Click);
			this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
			this.выходToolStripMenuItem.Size = new global::System.Drawing.Size(219, 22);
			this.выходToolStripMenuItem.Text = "Выход";
			this.выходToolStripMenuItem.Click += new global::System.EventHandler(this.выходToolStripMenuItem_Click);
			this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
			this.настройкиToolStripMenuItem.Size = new global::System.Drawing.Size(73, 20);
			this.настройкиToolStripMenuItem.Text = "Настройки";
			this.настройкиToolStripMenuItem.Click += new global::System.EventHandler(this.настройкиToolStripMenuItem_Click);
			this.nUD_Score_2.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.nUD_Score_2.Location = new global::System.Drawing.Point(176, 185);
			global::System.Windows.Forms.NumericUpDown numericUpDown = this.nUD_Score_2;
			int[] array = new int[4];
			array[0] = 99;
			numericUpDown.Maximum = new decimal(array);
			this.nUD_Score_2.Name = "nUD_Score_2";
			this.nUD_Score_2.Size = new global::System.Drawing.Size(47, 26);
			this.nUD_Score_2.TabIndex = 28;
			this.toolTip1.SetToolTip(this.nUD_Score_2, "Увеличение/уменьшение счета команды Б");
			this.nUD_Score_2.ValueChanged += new global::System.EventHandler(this.nUD_Score_2_ValueChanged);
			this.nUD_Score_2.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.nUD_Score_2_KeyPress);
			this.nUD_Period.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.nUD_Period.Location = new global::System.Drawing.Point(92, 185);
			global::System.Windows.Forms.NumericUpDown numericUpDown2 = this.nUD_Period;
			int[] array2 = new int[4];
			array2[0] = 3;
			numericUpDown2.Maximum = new decimal(array2);
			global::System.Windows.Forms.NumericUpDown numericUpDown3 = this.nUD_Period;
			int[] array3 = new int[4];
			array3[0] = 1;
			numericUpDown3.Minimum = new decimal(array3);
			this.nUD_Period.Name = "nUD_Period";
			this.nUD_Period.Size = new global::System.Drawing.Size(47, 26);
			this.nUD_Period.TabIndex = 27;
			this.toolTip1.SetToolTip(this.nUD_Period, "Увеличение/уменьшение периода");
			global::System.Windows.Forms.NumericUpDown numericUpDown4 = this.nUD_Period;
			int[] array4 = new int[4];
			array4[0] = 1;
			numericUpDown4.Value = new decimal(array4);
			this.nUD_Period.ValueChanged += new global::System.EventHandler(this.nUD_Period_ValueChanged);
			this.nUD_Period.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.nUD_Period_KeyPress);
			this.nUD_Score_1.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.nUD_Score_1.Location = new global::System.Drawing.Point(8, 185);
			global::System.Windows.Forms.NumericUpDown numericUpDown5 = this.nUD_Score_1;
			int[] array5 = new int[4];
			array5[0] = 99;
			numericUpDown5.Maximum = new decimal(array5);
			this.nUD_Score_1.Name = "nUD_Score_1";
			this.nUD_Score_1.Size = new global::System.Drawing.Size(47, 26);
			this.nUD_Score_1.TabIndex = 26;
			this.toolTip1.SetToolTip(this.nUD_Score_1, "Увеличение/уменьшение счета команды А");
			this.nUD_Score_1.ValueChanged += new global::System.EventHandler(this.nUD_Score_1_ValueChanged);
			this.nUD_Score_1.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.nUD_Score_1_KeyPress);
			this.panel1.Controls.Add(this.groupBox2);
			this.panel1.Controls.Add(this.groupBox1);
			this.panel1.Controls.Add(this.nUD_Score_2);
			this.panel1.Controls.Add(this.nUD_Period);
			this.panel1.Controls.Add(this.nUD_Score_1);
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.mtb_TownCommand_B);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.mtb_Command_B);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.mtb_TownCommand_A);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.mtb_Command_A);
			this.panel1.Dock = global::System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new global::System.Drawing.Point(661, 24);
			this.panel1.Name = "panel1";
			this.panel1.Size = new global::System.Drawing.Size(229, 339);
			this.panel1.TabIndex = 24;
			this.groupBox2.Controls.Add(this.but_DelPeople_B_Add);
			this.groupBox2.Controls.Add(this.mTB_DelPeople_B);
			this.groupBox2.Controls.Add(this.lB_DelPeople_B);
			this.groupBox2.Location = new global::System.Drawing.Point(127, 215);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new global::System.Drawing.Size(96, 122);
			this.groupBox2.TabIndex = 35;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Команда Б";
			this.but_DelPeople_B_Add.Location = new global::System.Drawing.Point(6, 99);
			this.but_DelPeople_B_Add.Name = "but_DelPeople_B_Add";
			this.but_DelPeople_B_Add.Size = new global::System.Drawing.Size(84, 20);
			this.but_DelPeople_B_Add.TabIndex = 36;
			this.but_DelPeople_B_Add.Text = "Добавить";
			this.but_DelPeople_B_Add.UseVisualStyleBackColor = true;
			this.but_DelPeople_B_Add.Click += new global::System.EventHandler(this.but_DelPeople_B_Add_Click);
			this.mTB_DelPeople_B.InsertKeyMode = global::System.Windows.Forms.InsertKeyMode.Overwrite;
			this.mTB_DelPeople_B.Location = new global::System.Drawing.Point(6, 73);
			this.mTB_DelPeople_B.Mask = "00 00:00";
			this.mTB_DelPeople_B.Name = "mTB_DelPeople_B";
			this.mTB_DelPeople_B.PromptChar = ' ';
			this.mTB_DelPeople_B.Size = new global::System.Drawing.Size(84, 20);
			this.mTB_DelPeople_B.TabIndex = 35;
			this.mTB_DelPeople_B.Text = "000000";
			this.mTB_DelPeople_B.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.mTB_DelPeople_B_KeyPress);
			this.lB_DelPeople_B.FormattingEnabled = true;
			this.lB_DelPeople_B.Location = new global::System.Drawing.Point(6, 17);
			this.lB_DelPeople_B.Name = "lB_DelPeople_B";
			this.lB_DelPeople_B.Size = new global::System.Drawing.Size(84, 56);
			this.lB_DelPeople_B.TabIndex = 34;
			this.lB_DelPeople_B.Click += new global::System.EventHandler(this.lB_DelPeople_B_Click);
			this.groupBox1.Controls.Add(this.but_DelPeople_A_Add);
			this.groupBox1.Controls.Add(this.mTB_DelPeople_A);
			this.groupBox1.Controls.Add(this.lB_DelPeople_A);
			this.groupBox1.Location = new global::System.Drawing.Point(8, 214);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new global::System.Drawing.Size(96, 122);
			this.groupBox1.TabIndex = 34;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Команда А";
			this.but_DelPeople_A_Add.Location = new global::System.Drawing.Point(6, 99);
			this.but_DelPeople_A_Add.Name = "but_DelPeople_A_Add";
			this.but_DelPeople_A_Add.Size = new global::System.Drawing.Size(84, 20);
			this.but_DelPeople_A_Add.TabIndex = 36;
			this.but_DelPeople_A_Add.Text = "Добавить";
			this.but_DelPeople_A_Add.UseVisualStyleBackColor = true;
			this.but_DelPeople_A_Add.Click += new global::System.EventHandler(this.but_DelPeople_A_Add_Click);
			this.mTB_DelPeople_A.InsertKeyMode = global::System.Windows.Forms.InsertKeyMode.Overwrite;
			this.mTB_DelPeople_A.Location = new global::System.Drawing.Point(6, 73);
			this.mTB_DelPeople_A.Mask = "00 00:00";
			this.mTB_DelPeople_A.Name = "mTB_DelPeople_A";
			this.mTB_DelPeople_A.PromptChar = ' ';
			this.mTB_DelPeople_A.Size = new global::System.Drawing.Size(84, 20);
			this.mTB_DelPeople_A.TabIndex = 35;
			this.mTB_DelPeople_A.Text = "000000";
			this.mTB_DelPeople_A.KeyPress += new global::System.Windows.Forms.KeyPressEventHandler(this.mTB_DelPeople_A_KeyPress);
			this.lB_DelPeople_A.FormattingEnabled = true;
			this.lB_DelPeople_A.Location = new global::System.Drawing.Point(6, 17);
			this.lB_DelPeople_A.Name = "lB_DelPeople_A";
			this.lB_DelPeople_A.Size = new global::System.Drawing.Size(84, 56);
			this.lB_DelPeople_A.TabIndex = 34;
			this.lB_DelPeople_A.Click += new global::System.EventHandler(this.lB_DelPeople_A_Click);
			this.label4.AutoSize = true;
			this.label4.Location = new global::System.Drawing.Point(8, 137);
			this.label4.Name = "label4";
			this.label4.Size = new global::System.Drawing.Size(96, 13);
			this.label4.TabIndex = 24;
			this.label4.Text = "Город команды Б";
			this.mtb_TownCommand_B.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.mtb_TownCommand_B.Location = new global::System.Drawing.Point(8, 153);
			this.mtb_TownCommand_B.Name = "mtb_TownCommand_B";
			this.mtb_TownCommand_B.Size = new global::System.Drawing.Size(215, 26);
			this.mtb_TownCommand_B.TabIndex = 23;
			this.mtb_TownCommand_B.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Right;
			this.mtb_TownCommand_B.TextChanged += new global::System.EventHandler(this.mtb_TownCommand_B_TextChanged);
			this.label3.AutoSize = true;
			this.label3.Location = new global::System.Drawing.Point(8, 93);
			this.label3.Name = "label3";
			this.label3.Size = new global::System.Drawing.Size(62, 13);
			this.label3.TabIndex = 22;
			this.label3.Text = "Команда Б";
			this.mtb_Command_B.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.mtb_Command_B.Location = new global::System.Drawing.Point(8, 109);
			this.mtb_Command_B.Name = "mtb_Command_B";
			this.mtb_Command_B.Size = new global::System.Drawing.Size(215, 26);
			this.mtb_Command_B.TabIndex = 21;
			this.mtb_Command_B.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Right;
			this.mtb_Command_B.TextChanged += new global::System.EventHandler(this.mtb_Command_B_TextChanged);
			this.label2.AutoSize = true;
			this.label2.Location = new global::System.Drawing.Point(8, 47);
			this.label2.Name = "label2";
			this.label2.Size = new global::System.Drawing.Size(96, 13);
			this.label2.TabIndex = 20;
			this.label2.Text = "Город команды А";
			this.mtb_TownCommand_A.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.mtb_TownCommand_A.Location = new global::System.Drawing.Point(8, 63);
			this.mtb_TownCommand_A.Name = "mtb_TownCommand_A";
			this.mtb_TownCommand_A.Size = new global::System.Drawing.Size(215, 26);
			this.mtb_TownCommand_A.TabIndex = 19;
			this.mtb_TownCommand_A.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Right;
			this.mtb_TownCommand_A.TextChanged += new global::System.EventHandler(this.mtb_TownCommand_A_TextChanged);
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(8, 1);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(62, 13);
			this.label1.TabIndex = 18;
			this.label1.Text = "Команда А";
			this.mtb_Command_A.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 204);
			this.mtb_Command_A.Location = new global::System.Drawing.Point(8, 17);
			this.mtb_Command_A.Name = "mtb_Command_A";
			this.mtb_Command_A.Size = new global::System.Drawing.Size(215, 26);
			this.mtb_Command_A.TabIndex = 17;
			this.mtb_Command_A.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Right;
			this.mtb_Command_A.TextChanged += new global::System.EventHandler(this.mtb_Command_A_TextChanged);
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new global::System.Drawing.Size(1028, 385);
			base.Controls.Add(this.splitContainer1);
			base.Controls.Add(this.panel1);
			base.Controls.Add(this.gB_Tablo);
			base.Controls.Add(this.Main_menu);
			base.Controls.Add(this.statusStrip1);
			base.IsMdiContainer = true;
			base.Name = "frmMain";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Табло";
			base.Load += new global::System.EventHandler(this.frmMain_Load);
			base.Paint += new global::System.Windows.Forms.PaintEventHandler(this.frmMain_Paint);
			base.FormClosed += new global::System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.gB_Tablo.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.gB_GameTime.ResumeLayout(false);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.gBox_VideoFileList.ResumeLayout(false);
			this.tabControl_VideoFileList.ResumeLayout(false);
			this.tabPage_BreakList.ResumeLayout(false);
			this.panel_VideoButtons.ResumeLayout(false);
			this.tabPage_TimeOutList.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.gBox_ScenList.ResumeLayout(false);
			this.panel_ScriptButtons.ResumeLayout(false);
			this.Main_menu.ResumeLayout(false);
			this.Main_menu.PerformLayout();
			((global::System.ComponentModel.ISupportInitialize)this.nUD_Score_2).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.nUD_Period).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.nUD_Score_1).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000017 RID: 23
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000018 RID: 24
		private global::System.Windows.Forms.GroupBox gB_Tablo;

		// Token: 0x04000019 RID: 25
		private global::System.Windows.Forms.StatusStrip statusStrip1;

		// Token: 0x0400001A RID: 26
		private global::System.Windows.Forms.SplitContainer splitContainer1;

		// Token: 0x0400001B RID: 27
		private global::System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;

		// Token: 0x0400001C RID: 28
		private global::System.Windows.Forms.Button but_ScriptPlay;

		// Token: 0x0400001D RID: 29
		private global::System.Windows.Forms.MenuStrip Main_menu;

		// Token: 0x0400001E RID: 30
		private global::System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;

		// Token: 0x0400001F RID: 31
		private global::System.Windows.Forms.ToolStripMenuItem загрузитьСписокВидеоToolStripMenuItem;

		// Token: 0x04000020 RID: 32
		private global::System.Windows.Forms.ToolStripMenuItem сохранитьСписокВидеоToolStripMenuItem;

		// Token: 0x04000021 RID: 33
		private global::System.Windows.Forms.ToolStripMenuItem загToolStripMenuItem;

		// Token: 0x04000022 RID: 34
		private global::System.Windows.Forms.ToolStripMenuItem сохранитьСписокШаблоновToolStripMenuItem;

		// Token: 0x04000023 RID: 35
		private global::System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;

		// Token: 0x04000024 RID: 36
		private global::System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;

		// Token: 0x04000025 RID: 37
		private global::System.Windows.Forms.ToolTip toolTip1;

		// Token: 0x04000026 RID: 38
		private global::System.Windows.Forms.Panel panel1;

		// Token: 0x04000027 RID: 39
		private global::System.Windows.Forms.Label label4;

		// Token: 0x04000028 RID: 40
		private global::System.Windows.Forms.MaskedTextBox mtb_TownCommand_B;

		// Token: 0x04000029 RID: 41
		private global::System.Windows.Forms.Label label3;

		// Token: 0x0400002A RID: 42
		private global::System.Windows.Forms.MaskedTextBox mtb_Command_B;

		// Token: 0x0400002B RID: 43
		private global::System.Windows.Forms.Label label2;

		// Token: 0x0400002C RID: 44
		private global::System.Windows.Forms.MaskedTextBox mtb_TownCommand_A;

		// Token: 0x0400002D RID: 45
		private global::System.Windows.Forms.Label label1;

		// Token: 0x0400002E RID: 46
		private global::System.Windows.Forms.MaskedTextBox mtb_Command_A;

		// Token: 0x0400002F RID: 47
		private global::System.Windows.Forms.GroupBox gBox_ScenList;

		// Token: 0x04000030 RID: 48
		private global::System.Windows.Forms.ListBox cLB_ScriptFiles;

		// Token: 0x04000031 RID: 49
		private global::System.Windows.Forms.Panel panel_ScriptButtons;

		// Token: 0x04000032 RID: 50
		private global::System.Windows.Forms.Button but_DeleteScript;

		// Token: 0x04000033 RID: 51
		private global::System.Windows.Forms.Button but_ScriptDown;

		// Token: 0x04000034 RID: 52
		private global::System.Windows.Forms.Button but_ScriptUp;

		// Token: 0x04000035 RID: 53
		private global::System.Windows.Forms.Button but_AddScript;

		// Token: 0x04000036 RID: 54
		private global::System.Windows.Forms.GroupBox gBox_VideoFileList;

		// Token: 0x04000037 RID: 55
		private global::System.Windows.Forms.TabControl tabControl_VideoFileList;

		// Token: 0x04000038 RID: 56
		private global::System.Windows.Forms.TabPage tabPage_BreakList;

		// Token: 0x04000039 RID: 57
		private global::System.Windows.Forms.ListBox cLB_VideoFiles;

		// Token: 0x0400003A RID: 58
		private global::System.Windows.Forms.Panel panel_VideoButtons;

		// Token: 0x0400003B RID: 59
		private global::System.Windows.Forms.Button but_DeleteVideo;

		// Token: 0x0400003C RID: 60
		private global::System.Windows.Forms.Button but_VideoDown;

		// Token: 0x0400003D RID: 61
		private global::System.Windows.Forms.Button but_VideoUp;

		// Token: 0x0400003E RID: 62
		private global::System.Windows.Forms.Button but_AddVideo;

		// Token: 0x0400003F RID: 63
		private global::System.Windows.Forms.TabPage tabPage_TimeOutList;

		// Token: 0x04000040 RID: 64
		private global::System.Windows.Forms.ListBox cLB_TimeoutVideoFiles;

		// Token: 0x04000041 RID: 65
		private global::System.Windows.Forms.Panel panel2;

		// Token: 0x04000042 RID: 66
		private global::System.Windows.Forms.Button but_TimeoutVideoFiles_Delete;

		// Token: 0x04000043 RID: 67
		private global::System.Windows.Forms.Button but_TimeoutVideoFiles_Down;

		// Token: 0x04000044 RID: 68
		private global::System.Windows.Forms.Button but_TimeoutVideoFiles_Up;

		// Token: 0x04000045 RID: 69
		private global::System.Windows.Forms.Button but_Timeout;

		// Token: 0x04000046 RID: 70
		private global::System.Windows.Forms.Button but_StopGame;

		// Token: 0x04000047 RID: 71
		private global::System.Windows.Forms.Button but_StartGame;

		// Token: 0x04000048 RID: 72
		private global::System.Windows.Forms.Button but_BeforeStart;

		// Token: 0x04000049 RID: 73
		private global::System.Windows.Forms.NumericUpDown nUD_Score_2;

		// Token: 0x0400004A RID: 74
		private global::System.Windows.Forms.NumericUpDown nUD_Period;

		// Token: 0x0400004B RID: 75
		private global::System.Windows.Forms.NumericUpDown nUD_Score_1;

		// Token: 0x0400004C RID: 76
		private global::System.Windows.Forms.Button but_Beep;

		// Token: 0x0400004D RID: 77
		private global::System.Windows.Forms.Button but_ResetGame;

		// Token: 0x0400004E RID: 78
		private global::System.Windows.Forms.Button but_BreakGame;

		// Token: 0x0400004F RID: 79
		private global::System.Windows.Forms.ToolStripMenuItem печатьСписковToolStripMenuItem;

		// Token: 0x04000050 RID: 80
		private global::System.Windows.Forms.Button but_TimeReset;

		// Token: 0x04000051 RID: 81
		private global::System.Windows.Forms.Button but_ShowPrompt;

		// Token: 0x04000052 RID: 82
		private global::System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;

		// Token: 0x04000053 RID: 83
		private global::System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;

		// Token: 0x04000054 RID: 84
		private global::System.Windows.Forms.GroupBox groupBox1;

		// Token: 0x04000055 RID: 85
		private global::System.Windows.Forms.Button but_DelPeople_A_Add;

		// Token: 0x04000056 RID: 86
		private global::System.Windows.Forms.MaskedTextBox mTB_DelPeople_A;

		// Token: 0x04000057 RID: 87
		private global::System.Windows.Forms.ListBox lB_DelPeople_A;

		// Token: 0x04000058 RID: 88
		private global::System.Windows.Forms.GroupBox groupBox2;

		// Token: 0x04000059 RID: 89
		private global::System.Windows.Forms.Button but_DelPeople_B_Add;

		// Token: 0x0400005A RID: 90
		private global::System.Windows.Forms.MaskedTextBox mTB_DelPeople_B;

		// Token: 0x0400005B RID: 91
		private global::System.Windows.Forms.ListBox lB_DelPeople_B;

		// Token: 0x0400005C RID: 92
		private global::System.Windows.Forms.GroupBox gB_GameTime;

		// Token: 0x0400005D RID: 93
		private global::System.Windows.Forms.DateTimePicker dTP_GameTime;

		// Token: 0x0400005E RID: 94
		private global::System.Windows.Forms.GroupBox groupBox4;
	}
}
