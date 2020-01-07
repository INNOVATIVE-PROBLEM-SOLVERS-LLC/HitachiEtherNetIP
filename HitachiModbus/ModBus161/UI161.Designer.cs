namespace ModBus161 {
   partial class UI161 {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent() {
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UI161));
         this.lblIPAddress = new System.Windows.Forms.Label();
         this.lblIPPort = new System.Windows.Forms.Label();
         this.txtIPAddress = new System.Windows.Forms.TextBox();
         this.txtIPPort = new System.Windows.Forms.TextBox();
         this.cmdReadData = new System.Windows.Forms.Button();
         this.cmdDisconnect = new System.Windows.Forms.Button();
         this.cmdConnect = new System.Windows.Forms.Button();
         this.cmdExit = new System.Windows.Forms.Button();
         this.lstMessages = new System.Windows.Forms.ListBox();
         this.cmLog = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cmLogClear = new System.Windows.Forms.ToolStripMenuItem();
         this.cmLogToNotepad = new System.Windows.Forms.ToolStripMenuItem();
         this.cmdComOff = new System.Windows.Forms.Button();
         this.cmdComOn = new System.Windows.Forms.Button();
         this.cmdWriteData = new System.Windows.Forms.Button();
         this.txtDataLength = new System.Windows.Forms.TextBox();
         this.txtDataAddress = new System.Windows.Forms.TextBox();
         this.lblDataLength = new System.Windows.Forms.Label();
         this.lblDataAddress = new System.Windows.Forms.Label();
         this.txtData = new System.Windows.Forms.TextBox();
         this.lblData = new System.Windows.Forms.Label();
         this.optHoldingRegister = new System.Windows.Forms.RadioButton();
         this.optInputRegister = new System.Windows.Forms.RadioButton();
         this.tclViews = new System.Windows.Forms.TabControl();
         this.tabXML = new System.Windows.Forms.TabPage();
         this.tvXML = new System.Windows.Forms.TreeView();
         this.tabIndented = new System.Windows.Forms.TabPage();
         this.txtIndentedView = new System.Windows.Forms.TextBox();
         this.tabLog = new System.Windows.Forms.TabPage();
         this.tabErrors = new System.Windows.Forms.TabPage();
         this.cmdErrorRefresh = new System.Windows.Forms.Button();
         this.cmdErrorClear = new System.Windows.Forms.Button();
         this.lbErrors = new System.Windows.Forms.ListBox();
         this.tabGroups = new System.Windows.Forms.TabPage();
         this.cmdGroupRefresh = new System.Windows.Forms.Button();
         this.dgGroups = new System.Windows.Forms.DataGridView();
         this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.tabMessages = new System.Windows.Forms.TabPage();
         this.dgMessages = new System.Windows.Forms.DataGridView();
         this.colGroup = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.cmdRetrieve = new System.Windows.Forms.Button();
         this.cmdReformat = new System.Windows.Forms.Button();
         this.cmdSaveAs = new System.Windows.Forms.Button();
         this.cmdOpen = new System.Windows.Forms.Button();
         this.cmdSend = new System.Windows.Forms.Button();
         this.lblMessageFolder = new System.Windows.Forms.Label();
         this.txtMessageFolder = new System.Windows.Forms.TextBox();
         this.cmdBrowse = new System.Windows.Forms.Button();
         this.cmdExperiment = new System.Windows.Forms.Button();
         this.lblInstance = new System.Windows.Forms.Label();
         this.lblAttribute = new System.Windows.Forms.Label();
         this.lblClass = new System.Windows.Forms.Label();
         this.cbClass = new System.Windows.Forms.ComboBox();
         this.cbAttribute = new System.Windows.Forms.ComboBox();
         this.cbInstance = new System.Windows.Forms.ComboBox();
         this.lblNozzle = new System.Windows.Forms.Label();
         this.cbNozzle = new System.Windows.Forms.ComboBox();
         this.chkHex = new System.Windows.Forms.CheckBox();
         this.chkTwinNozzle = new System.Windows.Forms.CheckBox();
         this.chkLogIO = new System.Windows.Forms.CheckBox();
         this.cmdReady = new System.Windows.Forms.Button();
         this.cmdStandby = new System.Windows.Forms.Button();
         this.cmdStartUp = new System.Windows.Forms.Button();
         this.cmdShutDown = new System.Windows.Forms.Button();
         this.cmdGetStatus = new System.Windows.Forms.Button();
         this.cmdReset = new System.Windows.Forms.Button();
         this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cmdMessageRefresh = new System.Windows.Forms.Button();
         this.cmdMessageLoad = new System.Windows.Forms.Button();
         this.cmLog.SuspendLayout();
         this.tclViews.SuspendLayout();
         this.tabXML.SuspendLayout();
         this.tabIndented.SuspendLayout();
         this.tabLog.SuspendLayout();
         this.tabErrors.SuspendLayout();
         this.tabGroups.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.dgGroups)).BeginInit();
         this.tabMessages.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.dgMessages)).BeginInit();
         this.SuspendLayout();
         // 
         // lblIPAddress
         // 
         this.lblIPAddress.Location = new System.Drawing.Point(29, 47);
         this.lblIPAddress.Name = "lblIPAddress";
         this.lblIPAddress.Size = new System.Drawing.Size(107, 22);
         this.lblIPAddress.TabIndex = 0;
         this.lblIPAddress.Text = "IP Address";
         this.lblIPAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblIPPort
         // 
         this.lblIPPort.Location = new System.Drawing.Point(29, 75);
         this.lblIPPort.Name = "lblIPPort";
         this.lblIPPort.Size = new System.Drawing.Size(107, 28);
         this.lblIPPort.TabIndex = 1;
         this.lblIPPort.Text = "IP Port";
         this.lblIPPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtIPAddress
         // 
         this.txtIPAddress.Location = new System.Drawing.Point(142, 47);
         this.txtIPAddress.Name = "txtIPAddress";
         this.txtIPAddress.Size = new System.Drawing.Size(110, 22);
         this.txtIPAddress.TabIndex = 2;
         this.txtIPAddress.Text = "192.168.168.100";
         this.txtIPAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // txtIPPort
         // 
         this.txtIPPort.Location = new System.Drawing.Point(142, 75);
         this.txtIPPort.Name = "txtIPPort";
         this.txtIPPort.Size = new System.Drawing.Size(110, 22);
         this.txtIPPort.TabIndex = 3;
         this.txtIPPort.Text = "502";
         this.txtIPPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // cmdReadData
         // 
         this.cmdReadData.Location = new System.Drawing.Point(653, 578);
         this.cmdReadData.Name = "cmdReadData";
         this.cmdReadData.Size = new System.Drawing.Size(127, 40);
         this.cmdReadData.TabIndex = 8;
         this.cmdReadData.Text = "Read Data";
         this.cmdReadData.UseVisualStyleBackColor = true;
         this.cmdReadData.Click += new System.EventHandler(this.cmdReadData_Click);
         // 
         // cmdDisconnect
         // 
         this.cmdDisconnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdDisconnect.Location = new System.Drawing.Point(258, 88);
         this.cmdDisconnect.Name = "cmdDisconnect";
         this.cmdDisconnect.Size = new System.Drawing.Size(98, 35);
         this.cmdDisconnect.TabIndex = 9;
         this.cmdDisconnect.Text = "Disconnect";
         this.cmdDisconnect.UseVisualStyleBackColor = false;
         this.cmdDisconnect.Click += new System.EventHandler(this.cmdDisconnect_Click);
         // 
         // cmdConnect
         // 
         this.cmdConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdConnect.Location = new System.Drawing.Point(258, 47);
         this.cmdConnect.Name = "cmdConnect";
         this.cmdConnect.Size = new System.Drawing.Size(98, 35);
         this.cmdConnect.TabIndex = 10;
         this.cmdConnect.Text = "Connect";
         this.cmdConnect.UseVisualStyleBackColor = false;
         this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
         // 
         // cmdExit
         // 
         this.cmdExit.Location = new System.Drawing.Point(674, 706);
         this.cmdExit.Name = "cmdExit";
         this.cmdExit.Size = new System.Drawing.Size(105, 35);
         this.cmdExit.TabIndex = 11;
         this.cmdExit.Text = "Exit";
         this.cmdExit.UseVisualStyleBackColor = true;
         this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
         // 
         // lstMessages
         // 
         this.lstMessages.ContextMenuStrip = this.cmLog;
         this.lstMessages.FormattingEnabled = true;
         this.lstMessages.ItemHeight = 16;
         this.lstMessages.Location = new System.Drawing.Point(15, 20);
         this.lstMessages.Name = "lstMessages";
         this.lstMessages.Size = new System.Drawing.Size(737, 356);
         this.lstMessages.TabIndex = 12;
         // 
         // cmLog
         // 
         this.cmLog.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.cmLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmLogClear,
            this.cmLogToNotepad});
         this.cmLog.Name = "cmLog";
         this.cmLog.Size = new System.Drawing.Size(189, 52);
         // 
         // cmLogClear
         // 
         this.cmLogClear.Name = "cmLogClear";
         this.cmLogClear.Size = new System.Drawing.Size(188, 24);
         this.cmLogClear.Text = "Clear";
         this.cmLogClear.Click += new System.EventHandler(this.cmLogClear_Click);
         // 
         // cmLogToNotepad
         // 
         this.cmLogToNotepad.Name = "cmLogToNotepad";
         this.cmLogToNotepad.Size = new System.Drawing.Size(188, 24);
         this.cmLogToNotepad.Text = "Load in NotePad";
         this.cmLogToNotepad.Click += new System.EventHandler(this.cmLogToNotepad_Click);
         // 
         // cmdComOff
         // 
         this.cmdComOff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdComOff.Location = new System.Drawing.Point(362, 88);
         this.cmdComOff.Name = "cmdComOff";
         this.cmdComOff.Size = new System.Drawing.Size(98, 35);
         this.cmdComOff.TabIndex = 16;
         this.cmdComOff.Text = "Com Off";
         this.cmdComOff.UseVisualStyleBackColor = false;
         this.cmdComOff.Click += new System.EventHandler(this.cmdComOff_Click);
         // 
         // cmdComOn
         // 
         this.cmdComOn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdComOn.Location = new System.Drawing.Point(362, 47);
         this.cmdComOn.Name = "cmdComOn";
         this.cmdComOn.Size = new System.Drawing.Size(98, 35);
         this.cmdComOn.TabIndex = 17;
         this.cmdComOn.Text = "Com On";
         this.cmdComOn.UseVisualStyleBackColor = false;
         this.cmdComOn.Click += new System.EventHandler(this.cmdComOn_Click);
         // 
         // cmdWriteData
         // 
         this.cmdWriteData.Location = new System.Drawing.Point(653, 629);
         this.cmdWriteData.Name = "cmdWriteData";
         this.cmdWriteData.Size = new System.Drawing.Size(127, 40);
         this.cmdWriteData.TabIndex = 18;
         this.cmdWriteData.Text = "Write Data";
         this.cmdWriteData.UseVisualStyleBackColor = true;
         this.cmdWriteData.Click += new System.EventHandler(this.cmdWriteData_Click);
         // 
         // txtDataLength
         // 
         this.txtDataLength.Location = new System.Drawing.Point(416, 633);
         this.txtDataLength.Name = "txtDataLength";
         this.txtDataLength.Size = new System.Drawing.Size(73, 22);
         this.txtDataLength.TabIndex = 22;
         this.txtDataLength.Text = "1";
         this.txtDataLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // txtDataAddress
         // 
         this.txtDataAddress.Location = new System.Drawing.Point(416, 579);
         this.txtDataAddress.Name = "txtDataAddress";
         this.txtDataAddress.Size = new System.Drawing.Size(73, 22);
         this.txtDataAddress.TabIndex = 21;
         this.txtDataAddress.Text = "2490";
         this.txtDataAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblDataLength
         // 
         this.lblDataLength.Location = new System.Drawing.Point(315, 629);
         this.lblDataLength.Name = "lblDataLength";
         this.lblDataLength.Size = new System.Drawing.Size(87, 28);
         this.lblDataLength.TabIndex = 20;
         this.lblDataLength.Text = "Byte Length";
         this.lblDataLength.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblDataAddress
         // 
         this.lblDataAddress.Location = new System.Drawing.Point(320, 579);
         this.lblDataAddress.Name = "lblDataAddress";
         this.lblDataAddress.Size = new System.Drawing.Size(90, 22);
         this.lblDataAddress.TabIndex = 19;
         this.lblDataAddress.Text = "Hex Address";
         this.lblDataAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtData
         // 
         this.txtData.Location = new System.Drawing.Point(416, 661);
         this.txtData.Name = "txtData";
         this.txtData.Size = new System.Drawing.Size(222, 22);
         this.txtData.TabIndex = 24;
         // 
         // lblData
         // 
         this.lblData.Location = new System.Drawing.Point(315, 661);
         this.lblData.Name = "lblData";
         this.lblData.Size = new System.Drawing.Size(87, 28);
         this.lblData.TabIndex = 23;
         this.lblData.Text = "Data";
         this.lblData.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // optHoldingRegister
         // 
         this.optHoldingRegister.Checked = true;
         this.optHoldingRegister.Location = new System.Drawing.Point(504, 580);
         this.optHoldingRegister.Name = "optHoldingRegister";
         this.optHoldingRegister.Size = new System.Drawing.Size(134, 21);
         this.optHoldingRegister.TabIndex = 25;
         this.optHoldingRegister.TabStop = true;
         this.optHoldingRegister.Text = "Holding Register";
         this.optHoldingRegister.UseVisualStyleBackColor = true;
         // 
         // optInputRegister
         // 
         this.optInputRegister.Location = new System.Drawing.Point(504, 607);
         this.optInputRegister.Name = "optInputRegister";
         this.optInputRegister.Size = new System.Drawing.Size(134, 21);
         this.optInputRegister.TabIndex = 26;
         this.optInputRegister.Text = "Input Register";
         this.optInputRegister.UseVisualStyleBackColor = true;
         // 
         // tclViews
         // 
         this.tclViews.Controls.Add(this.tabMessages);
         this.tclViews.Controls.Add(this.tabGroups);
         this.tclViews.Controls.Add(this.tabXML);
         this.tclViews.Controls.Add(this.tabIndented);
         this.tclViews.Controls.Add(this.tabErrors);
         this.tclViews.Controls.Add(this.tabLog);
         this.tclViews.Location = new System.Drawing.Point(8, 148);
         this.tclViews.Name = "tclViews";
         this.tclViews.SelectedIndex = 0;
         this.tclViews.Size = new System.Drawing.Size(776, 422);
         this.tclViews.TabIndex = 27;
         // 
         // tabXML
         // 
         this.tabXML.Controls.Add(this.tvXML);
         this.tabXML.Location = new System.Drawing.Point(4, 25);
         this.tabXML.Name = "tabXML";
         this.tabXML.Padding = new System.Windows.Forms.Padding(3);
         this.tabXML.Size = new System.Drawing.Size(768, 393);
         this.tabXML.TabIndex = 0;
         this.tabXML.Text = "XML View";
         this.tabXML.UseVisualStyleBackColor = true;
         // 
         // tvXML
         // 
         this.tvXML.Location = new System.Drawing.Point(15, 19);
         this.tvXML.Name = "tvXML";
         this.tvXML.Size = new System.Drawing.Size(727, 353);
         this.tvXML.TabIndex = 0;
         // 
         // tabIndented
         // 
         this.tabIndented.Controls.Add(this.txtIndentedView);
         this.tabIndented.Location = new System.Drawing.Point(4, 25);
         this.tabIndented.Name = "tabIndented";
         this.tabIndented.Padding = new System.Windows.Forms.Padding(3);
         this.tabIndented.Size = new System.Drawing.Size(768, 393);
         this.tabIndented.TabIndex = 1;
         this.tabIndented.Text = "Indented View";
         this.tabIndented.UseVisualStyleBackColor = true;
         // 
         // txtIndentedView
         // 
         this.txtIndentedView.ContextMenuStrip = this.cmLog;
         this.txtIndentedView.Location = new System.Drawing.Point(15, 6);
         this.txtIndentedView.Multiline = true;
         this.txtIndentedView.Name = "txtIndentedView";
         this.txtIndentedView.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtIndentedView.Size = new System.Drawing.Size(728, 376);
         this.txtIndentedView.TabIndex = 0;
         // 
         // tabLog
         // 
         this.tabLog.Controls.Add(this.lstMessages);
         this.tabLog.Location = new System.Drawing.Point(4, 25);
         this.tabLog.Name = "tabLog";
         this.tabLog.Size = new System.Drawing.Size(768, 393);
         this.tabLog.TabIndex = 3;
         this.tabLog.Text = "Log";
         this.tabLog.UseVisualStyleBackColor = true;
         // 
         // tabErrors
         // 
         this.tabErrors.Controls.Add(this.cmdErrorRefresh);
         this.tabErrors.Controls.Add(this.cmdErrorClear);
         this.tabErrors.Controls.Add(this.lbErrors);
         this.tabErrors.Location = new System.Drawing.Point(4, 25);
         this.tabErrors.Name = "tabErrors";
         this.tabErrors.Size = new System.Drawing.Size(768, 393);
         this.tabErrors.TabIndex = 4;
         this.tabErrors.Text = "Errors";
         this.tabErrors.UseVisualStyleBackColor = true;
         // 
         // cmdErrorRefresh
         // 
         this.cmdErrorRefresh.Location = new System.Drawing.Point(492, 341);
         this.cmdErrorRefresh.Name = "cmdErrorRefresh";
         this.cmdErrorRefresh.Size = new System.Drawing.Size(127, 40);
         this.cmdErrorRefresh.TabIndex = 15;
         this.cmdErrorRefresh.Text = "Refresh";
         this.cmdErrorRefresh.UseVisualStyleBackColor = true;
         this.cmdErrorRefresh.Click += new System.EventHandler(this.cmdErrorRefresh_Click);
         // 
         // cmdErrorClear
         // 
         this.cmdErrorClear.Location = new System.Drawing.Point(626, 341);
         this.cmdErrorClear.Name = "cmdErrorClear";
         this.cmdErrorClear.Size = new System.Drawing.Size(127, 40);
         this.cmdErrorClear.TabIndex = 14;
         this.cmdErrorClear.Text = "Clear";
         this.cmdErrorClear.UseVisualStyleBackColor = true;
         // 
         // lbErrors
         // 
         this.lbErrors.ContextMenuStrip = this.cmLog;
         this.lbErrors.FormattingEnabled = true;
         this.lbErrors.ItemHeight = 16;
         this.lbErrors.Location = new System.Drawing.Point(16, 18);
         this.lbErrors.Name = "lbErrors";
         this.lbErrors.Size = new System.Drawing.Size(737, 308);
         this.lbErrors.TabIndex = 13;
         // 
         // tabGroups
         // 
         this.tabGroups.Controls.Add(this.cmdGroupRefresh);
         this.tabGroups.Controls.Add(this.dgGroups);
         this.tabGroups.Location = new System.Drawing.Point(4, 25);
         this.tabGroups.Name = "tabGroups";
         this.tabGroups.Size = new System.Drawing.Size(768, 393);
         this.tabGroups.TabIndex = 6;
         this.tabGroups.Text = "Groups";
         this.tabGroups.UseVisualStyleBackColor = true;
         // 
         // cmdGroupRefresh
         // 
         this.cmdGroupRefresh.Location = new System.Drawing.Point(617, 338);
         this.cmdGroupRefresh.Name = "cmdGroupRefresh";
         this.cmdGroupRefresh.Size = new System.Drawing.Size(127, 40);
         this.cmdGroupRefresh.TabIndex = 9;
         this.cmdGroupRefresh.Text = "Refresh";
         this.cmdGroupRefresh.UseVisualStyleBackColor = true;
         // 
         // dgGroups
         // 
         this.dgGroups.AllowUserToAddRows = false;
         this.dgGroups.AllowUserToDeleteRows = false;
         this.dgGroups.AllowUserToOrderColumns = true;
         this.dgGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
         this.dgGroups.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
         this.dgGroups.Location = new System.Drawing.Point(10, 13);
         this.dgGroups.Name = "dgGroups";
         this.dgGroups.ReadOnly = true;
         this.dgGroups.RowTemplate.Height = 24;
         this.dgGroups.Size = new System.Drawing.Size(734, 295);
         this.dgGroups.TabIndex = 0;
         // 
         // dataGridViewTextBoxColumn1
         // 
         this.dataGridViewTextBoxColumn1.HeaderText = "Group #";
         this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
         this.dataGridViewTextBoxColumn1.ReadOnly = true;
         // 
         // dataGridViewTextBoxColumn2
         // 
         this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
         this.dataGridViewTextBoxColumn2.HeaderText = "Name";
         this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
         this.dataGridViewTextBoxColumn2.ReadOnly = true;
         this.dataGridViewTextBoxColumn2.Width = 74;
         // 
         // tabMessages
         // 
         this.tabMessages.Controls.Add(this.cmdMessageLoad);
         this.tabMessages.Controls.Add(this.cmdMessageRefresh);
         this.tabMessages.Controls.Add(this.dgMessages);
         this.tabMessages.Location = new System.Drawing.Point(4, 25);
         this.tabMessages.Name = "tabMessages";
         this.tabMessages.Size = new System.Drawing.Size(768, 393);
         this.tabMessages.TabIndex = 5;
         this.tabMessages.Text = "Messages";
         this.tabMessages.UseVisualStyleBackColor = true;
         // 
         // dgMessages
         // 
         this.dgMessages.AllowUserToAddRows = false;
         this.dgMessages.AllowUserToDeleteRows = false;
         this.dgMessages.AllowUserToOrderColumns = true;
         this.dgMessages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
         this.dgMessages.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colGroup,
            this.colMessage,
            this.colName});
         this.dgMessages.Location = new System.Drawing.Point(10, 16);
         this.dgMessages.MultiSelect = false;
         this.dgMessages.Name = "dgMessages";
         this.dgMessages.ReadOnly = true;
         this.dgMessages.RowTemplate.Height = 24;
         this.dgMessages.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
         this.dgMessages.Size = new System.Drawing.Size(734, 308);
         this.dgMessages.TabIndex = 1;
         // 
         // colGroup
         // 
         this.colGroup.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
         this.colGroup.HeaderText = "Group #";
         this.colGroup.Name = "colGroup";
         this.colGroup.ReadOnly = true;
         this.colGroup.Width = 89;
         // 
         // colMessage
         // 
         this.colMessage.HeaderText = "Msg #";
         this.colMessage.Name = "colMessage";
         this.colMessage.ReadOnly = true;
         // 
         // colName
         // 
         this.colName.HeaderText = "Name";
         this.colName.Name = "colName";
         this.colName.ReadOnly = true;
         // 
         // cmdRetrieve
         // 
         this.cmdRetrieve.Location = new System.Drawing.Point(19, 701);
         this.cmdRetrieve.Name = "cmdRetrieve";
         this.cmdRetrieve.Size = new System.Drawing.Size(87, 40);
         this.cmdRetrieve.TabIndex = 28;
         this.cmdRetrieve.Text = "Retrieve";
         this.cmdRetrieve.UseVisualStyleBackColor = true;
         this.cmdRetrieve.Click += new System.EventHandler(this.cmdRetrieve_Click);
         // 
         // cmdReformat
         // 
         this.cmdReformat.Enabled = false;
         this.cmdReformat.Location = new System.Drawing.Point(391, 701);
         this.cmdReformat.Name = "cmdReformat";
         this.cmdReformat.Size = new System.Drawing.Size(87, 40);
         this.cmdReformat.TabIndex = 29;
         this.cmdReformat.Text = "Reformat";
         this.cmdReformat.UseVisualStyleBackColor = true;
         this.cmdReformat.Click += new System.EventHandler(this.cmdReformat_Click);
         // 
         // cmdSaveAs
         // 
         this.cmdSaveAs.Location = new System.Drawing.Point(112, 701);
         this.cmdSaveAs.Name = "cmdSaveAs";
         this.cmdSaveAs.Size = new System.Drawing.Size(87, 40);
         this.cmdSaveAs.TabIndex = 30;
         this.cmdSaveAs.Text = "Save As";
         this.cmdSaveAs.UseVisualStyleBackColor = true;
         this.cmdSaveAs.Click += new System.EventHandler(this.cmdSaveAs_Click);
         // 
         // cmdOpen
         // 
         this.cmdOpen.Location = new System.Drawing.Point(205, 701);
         this.cmdOpen.Name = "cmdOpen";
         this.cmdOpen.Size = new System.Drawing.Size(87, 40);
         this.cmdOpen.TabIndex = 32;
         this.cmdOpen.Text = "Open";
         this.cmdOpen.UseVisualStyleBackColor = true;
         this.cmdOpen.Click += new System.EventHandler(this.cmdOpen_Click);
         // 
         // cmdSend
         // 
         this.cmdSend.Location = new System.Drawing.Point(298, 701);
         this.cmdSend.Name = "cmdSend";
         this.cmdSend.Size = new System.Drawing.Size(87, 40);
         this.cmdSend.TabIndex = 31;
         this.cmdSend.Text = "Send";
         this.cmdSend.UseVisualStyleBackColor = true;
         this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);
         // 
         // lblMessageFolder
         // 
         this.lblMessageFolder.Location = new System.Drawing.Point(15, 13);
         this.lblMessageFolder.Name = "lblMessageFolder";
         this.lblMessageFolder.Size = new System.Drawing.Size(121, 22);
         this.lblMessageFolder.TabIndex = 33;
         this.lblMessageFolder.Text = "Message Folder";
         this.lblMessageFolder.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtMessageFolder
         // 
         this.txtMessageFolder.Location = new System.Drawing.Point(136, 10);
         this.txtMessageFolder.Name = "txtMessageFolder";
         this.txtMessageFolder.Size = new System.Drawing.Size(518, 22);
         this.txtMessageFolder.TabIndex = 34;
         this.txtMessageFolder.Text = "192.168.168.100";
         // 
         // cmdBrowse
         // 
         this.cmdBrowse.Location = new System.Drawing.Point(674, 4);
         this.cmdBrowse.Name = "cmdBrowse";
         this.cmdBrowse.Size = new System.Drawing.Size(105, 35);
         this.cmdBrowse.TabIndex = 35;
         this.cmdBrowse.Text = "Browse";
         this.cmdBrowse.UseVisualStyleBackColor = true;
         this.cmdBrowse.Click += new System.EventHandler(this.cmdBrowse_Click);
         // 
         // cmdExperiment
         // 
         this.cmdExperiment.Enabled = false;
         this.cmdExperiment.Location = new System.Drawing.Point(490, 701);
         this.cmdExperiment.Name = "cmdExperiment";
         this.cmdExperiment.Size = new System.Drawing.Size(127, 40);
         this.cmdExperiment.TabIndex = 36;
         this.cmdExperiment.Text = "Experiment";
         this.cmdExperiment.UseVisualStyleBackColor = true;
         this.cmdExperiment.Click += new System.EventHandler(this.cmdExperiment_Click);
         // 
         // lblInstance
         // 
         this.lblInstance.Location = new System.Drawing.Point(19, 646);
         this.lblInstance.Name = "lblInstance";
         this.lblInstance.Size = new System.Drawing.Size(87, 28);
         this.lblInstance.TabIndex = 39;
         this.lblInstance.Text = "Instance";
         this.lblInstance.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblAttribute
         // 
         this.lblAttribute.Location = new System.Drawing.Point(19, 614);
         this.lblAttribute.Name = "lblAttribute";
         this.lblAttribute.Size = new System.Drawing.Size(87, 28);
         this.lblAttribute.TabIndex = 38;
         this.lblAttribute.Text = "Attribute";
         this.lblAttribute.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblClass
         // 
         this.lblClass.Location = new System.Drawing.Point(16, 582);
         this.lblClass.Name = "lblClass";
         this.lblClass.Size = new System.Drawing.Size(90, 22);
         this.lblClass.TabIndex = 37;
         this.lblClass.Text = "Class";
         this.lblClass.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // cbClass
         // 
         this.cbClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbClass.FormattingEnabled = true;
         this.cbClass.Location = new System.Drawing.Point(112, 579);
         this.cbClass.Name = "cbClass";
         this.cbClass.Size = new System.Drawing.Size(184, 24);
         this.cbClass.TabIndex = 40;
         this.cbClass.SelectedIndexChanged += new System.EventHandler(this.cbClass_SelectedIndexChanged);
         // 
         // cbAttribute
         // 
         this.cbAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbAttribute.FormattingEnabled = true;
         this.cbAttribute.Location = new System.Drawing.Point(112, 615);
         this.cbAttribute.Name = "cbAttribute";
         this.cbAttribute.Size = new System.Drawing.Size(184, 24);
         this.cbAttribute.TabIndex = 41;
         this.cbAttribute.SelectedIndexChanged += new System.EventHandler(this.cbAttribute_SelectedIndexChanged);
         // 
         // cbInstance
         // 
         this.cbInstance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbInstance.FormattingEnabled = true;
         this.cbInstance.Location = new System.Drawing.Point(112, 650);
         this.cbInstance.Name = "cbInstance";
         this.cbInstance.Size = new System.Drawing.Size(184, 24);
         this.cbInstance.TabIndex = 42;
         this.cbInstance.SelectedIndexChanged += new System.EventHandler(this.cbInstance_SelectedIndexChanged);
         // 
         // lblNozzle
         // 
         this.lblNozzle.Location = new System.Drawing.Point(315, 601);
         this.lblNozzle.Name = "lblNozzle";
         this.lblNozzle.Size = new System.Drawing.Size(87, 28);
         this.lblNozzle.TabIndex = 43;
         this.lblNozzle.Text = "Nozzle";
         this.lblNozzle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         // 
         // cbNozzle
         // 
         this.cbNozzle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbNozzle.FormattingEnabled = true;
         this.cbNozzle.Items.AddRange(new object[] {
            "Printer",
            "N-1",
            "N-2",
            "Both"});
         this.cbNozzle.Location = new System.Drawing.Point(416, 604);
         this.cbNozzle.Name = "cbNozzle";
         this.cbNozzle.Size = new System.Drawing.Size(73, 24);
         this.cbNozzle.TabIndex = 44;
         // 
         // chkHex
         // 
         this.chkHex.Location = new System.Drawing.Point(504, 635);
         this.chkHex.Name = "chkHex";
         this.chkHex.Size = new System.Drawing.Size(113, 26);
         this.chkHex.TabIndex = 45;
         this.chkHex.Text = "Data is Hex";
         this.chkHex.UseVisualStyleBackColor = true;
         // 
         // chkTwinNozzle
         // 
         this.chkTwinNozzle.Location = new System.Drawing.Point(112, 108);
         this.chkTwinNozzle.Name = "chkTwinNozzle";
         this.chkTwinNozzle.Size = new System.Drawing.Size(140, 20);
         this.chkTwinNozzle.TabIndex = 1;
         this.chkTwinNozzle.Text = "Twin Nozzle";
         this.chkTwinNozzle.UseVisualStyleBackColor = true;
         // 
         // chkLogIO
         // 
         this.chkLogIO.Location = new System.Drawing.Point(12, 99);
         this.chkLogIO.Name = "chkLogIO";
         this.chkLogIO.Size = new System.Drawing.Size(94, 29);
         this.chkLogIO.TabIndex = 46;
         this.chkLogIO.Text = "Log I/O";
         this.chkLogIO.UseVisualStyleBackColor = true;
         this.chkLogIO.CheckedChanged += new System.EventHandler(this.chkLogIO_CheckedChanged);
         // 
         // cmdReady
         // 
         this.cmdReady.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdReady.Location = new System.Drawing.Point(466, 47);
         this.cmdReady.Name = "cmdReady";
         this.cmdReady.Size = new System.Drawing.Size(98, 35);
         this.cmdReady.TabIndex = 48;
         this.cmdReady.Text = "Ready";
         this.cmdReady.UseVisualStyleBackColor = false;
         this.cmdReady.Click += new System.EventHandler(this.cmdReady_Click);
         // 
         // cmdStandby
         // 
         this.cmdStandby.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdStandby.Location = new System.Drawing.Point(466, 88);
         this.cmdStandby.Name = "cmdStandby";
         this.cmdStandby.Size = new System.Drawing.Size(98, 35);
         this.cmdStandby.TabIndex = 47;
         this.cmdStandby.Text = "Standby";
         this.cmdStandby.UseVisualStyleBackColor = false;
         this.cmdStandby.Click += new System.EventHandler(this.cmdStandby_Click);
         // 
         // cmdStartUp
         // 
         this.cmdStartUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdStartUp.Location = new System.Drawing.Point(570, 47);
         this.cmdStartUp.Name = "cmdStartUp";
         this.cmdStartUp.Size = new System.Drawing.Size(98, 35);
         this.cmdStartUp.TabIndex = 50;
         this.cmdStartUp.Text = "Start Up";
         this.cmdStartUp.UseVisualStyleBackColor = false;
         this.cmdStartUp.Click += new System.EventHandler(this.cmdStartUp_Click);
         // 
         // cmdShutDown
         // 
         this.cmdShutDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdShutDown.Location = new System.Drawing.Point(570, 88);
         this.cmdShutDown.Name = "cmdShutDown";
         this.cmdShutDown.Size = new System.Drawing.Size(98, 35);
         this.cmdShutDown.TabIndex = 49;
         this.cmdShutDown.Text = "Shut Down";
         this.cmdShutDown.UseVisualStyleBackColor = false;
         this.cmdShutDown.Click += new System.EventHandler(this.cmdShutDown_Click);
         // 
         // cmdGetStatus
         // 
         this.cmdGetStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdGetStatus.Location = new System.Drawing.Point(674, 47);
         this.cmdGetStatus.Name = "cmdGetStatus";
         this.cmdGetStatus.Size = new System.Drawing.Size(98, 35);
         this.cmdGetStatus.TabIndex = 52;
         this.cmdGetStatus.Text = "Get Status";
         this.cmdGetStatus.UseVisualStyleBackColor = false;
         this.cmdGetStatus.Click += new System.EventHandler(this.cmdGetStatus_Click);
         // 
         // cmdReset
         // 
         this.cmdReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
         this.cmdReset.Location = new System.Drawing.Point(674, 88);
         this.cmdReset.Name = "cmdReset";
         this.cmdReset.Size = new System.Drawing.Size(98, 35);
         this.cmdReset.TabIndex = 51;
         this.cmdReset.Text = "Reset Alarm";
         this.cmdReset.UseVisualStyleBackColor = false;
         this.cmdReset.Click += new System.EventHandler(this.cmdReset_Click);
         // 
         // contextMenuStrip1
         // 
         this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.contextMenuStrip1.Name = "contextMenuStrip1";
         this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
         // 
         // cmdMessageRefresh
         // 
         this.cmdMessageRefresh.Location = new System.Drawing.Point(617, 339);
         this.cmdMessageRefresh.Name = "cmdMessageRefresh";
         this.cmdMessageRefresh.Size = new System.Drawing.Size(127, 40);
         this.cmdMessageRefresh.TabIndex = 10;
         this.cmdMessageRefresh.Text = "Refresh";
         this.cmdMessageRefresh.UseVisualStyleBackColor = true;
         this.cmdMessageRefresh.Click += new System.EventHandler(this.cmdMessageRefresh_Click);
         // 
         // cmdMessageLoad
         // 
         this.cmdMessageLoad.Location = new System.Drawing.Point(478, 339);
         this.cmdMessageLoad.Name = "cmdMessageLoad";
         this.cmdMessageLoad.Size = new System.Drawing.Size(127, 40);
         this.cmdMessageLoad.TabIndex = 11;
         this.cmdMessageLoad.Text = "Load";
         this.cmdMessageLoad.UseVisualStyleBackColor = true;
         this.cmdMessageLoad.Click += new System.EventHandler(this.cmdMessageLoad_Click);
         // 
         // UI161
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(800, 753);
         this.Controls.Add(this.cmdGetStatus);
         this.Controls.Add(this.cmdReset);
         this.Controls.Add(this.cmdStartUp);
         this.Controls.Add(this.cmdShutDown);
         this.Controls.Add(this.cmdReady);
         this.Controls.Add(this.cmdStandby);
         this.Controls.Add(this.chkLogIO);
         this.Controls.Add(this.chkTwinNozzle);
         this.Controls.Add(this.chkHex);
         this.Controls.Add(this.cbNozzle);
         this.Controls.Add(this.lblNozzle);
         this.Controls.Add(this.cbInstance);
         this.Controls.Add(this.cbAttribute);
         this.Controls.Add(this.cbClass);
         this.Controls.Add(this.lblInstance);
         this.Controls.Add(this.lblAttribute);
         this.Controls.Add(this.lblClass);
         this.Controls.Add(this.cmdExperiment);
         this.Controls.Add(this.cmdBrowse);
         this.Controls.Add(this.txtMessageFolder);
         this.Controls.Add(this.lblMessageFolder);
         this.Controls.Add(this.cmdOpen);
         this.Controls.Add(this.cmdSend);
         this.Controls.Add(this.cmdSaveAs);
         this.Controls.Add(this.cmdReformat);
         this.Controls.Add(this.cmdRetrieve);
         this.Controls.Add(this.tclViews);
         this.Controls.Add(this.optInputRegister);
         this.Controls.Add(this.optHoldingRegister);
         this.Controls.Add(this.txtData);
         this.Controls.Add(this.lblData);
         this.Controls.Add(this.txtDataLength);
         this.Controls.Add(this.txtDataAddress);
         this.Controls.Add(this.lblDataLength);
         this.Controls.Add(this.lblDataAddress);
         this.Controls.Add(this.cmdWriteData);
         this.Controls.Add(this.cmdComOn);
         this.Controls.Add(this.cmdComOff);
         this.Controls.Add(this.cmdExit);
         this.Controls.Add(this.cmdConnect);
         this.Controls.Add(this.cmdDisconnect);
         this.Controls.Add(this.cmdReadData);
         this.Controls.Add(this.txtIPPort);
         this.Controls.Add(this.txtIPAddress);
         this.Controls.Add(this.lblIPPort);
         this.Controls.Add(this.lblIPAddress);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Name = "UI161";
         this.Text = "Modbus for Hitachi UX Model 161 Printer";
         this.cmLog.ResumeLayout(false);
         this.tclViews.ResumeLayout(false);
         this.tabXML.ResumeLayout(false);
         this.tabIndented.ResumeLayout(false);
         this.tabIndented.PerformLayout();
         this.tabLog.ResumeLayout(false);
         this.tabErrors.ResumeLayout(false);
         this.tabGroups.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.dgGroups)).EndInit();
         this.tabMessages.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.dgMessages)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lblIPAddress;
      private System.Windows.Forms.Label lblIPPort;
      private System.Windows.Forms.Button cmdReadData;
      private System.Windows.Forms.Button cmdDisconnect;
      private System.Windows.Forms.Button cmdConnect;
      private System.Windows.Forms.Button cmdExit;
      private System.Windows.Forms.ListBox lstMessages;
      private System.Windows.Forms.Button cmdComOff;
      private System.Windows.Forms.Button cmdComOn;
      private System.Windows.Forms.Button cmdWriteData;
      private System.Windows.Forms.Label lblDataLength;
      private System.Windows.Forms.Label lblDataAddress;
      private System.Windows.Forms.Label lblData;
      private System.Windows.Forms.TabControl tclViews;
      private System.Windows.Forms.TabPage tabXML;
      private System.Windows.Forms.TabPage tabIndented;
      private System.Windows.Forms.TabPage tabLog;
      private System.Windows.Forms.Button cmdRetrieve;
      private System.Windows.Forms.TextBox txtIndentedView;
      private System.Windows.Forms.TreeView tvXML;
      private System.Windows.Forms.ContextMenuStrip cmLog;
      private System.Windows.Forms.ToolStripMenuItem cmLogClear;
      private System.Windows.Forms.ToolStripMenuItem cmLogToNotepad;
      private System.Windows.Forms.Button cmdReformat;
      private System.Windows.Forms.Button cmdSaveAs;
      private System.Windows.Forms.Button cmdOpen;
      private System.Windows.Forms.Button cmdSend;
      private System.Windows.Forms.Label lblMessageFolder;
      private System.Windows.Forms.Button cmdBrowse;
      public System.Windows.Forms.TextBox txtIPAddress;
      public System.Windows.Forms.TextBox txtIPPort;
      public System.Windows.Forms.TextBox txtDataLength;
      public System.Windows.Forms.TextBox txtDataAddress;
      public System.Windows.Forms.TextBox txtData;
      public System.Windows.Forms.RadioButton optHoldingRegister;
      public System.Windows.Forms.RadioButton optInputRegister;
      public System.Windows.Forms.TextBox txtMessageFolder;
      private System.Windows.Forms.Button cmdExperiment;
      private System.Windows.Forms.Label lblInstance;
      private System.Windows.Forms.Label lblAttribute;
      private System.Windows.Forms.Label lblClass;
      private System.Windows.Forms.ComboBox cbClass;
      private System.Windows.Forms.ComboBox cbAttribute;
      private System.Windows.Forms.ComboBox cbInstance;
      private System.Windows.Forms.Label lblNozzle;
      public System.Windows.Forms.CheckBox chkTwinNozzle;
      public System.Windows.Forms.ComboBox cbNozzle;
      public System.Windows.Forms.CheckBox chkHex;
      public System.Windows.Forms.CheckBox chkLogIO;
      private System.Windows.Forms.Button cmdReady;
      private System.Windows.Forms.Button cmdStandby;
      private System.Windows.Forms.Button cmdStartUp;
      private System.Windows.Forms.Button cmdShutDown;
      private System.Windows.Forms.Button cmdGetStatus;
      private System.Windows.Forms.Button cmdReset;
      private System.Windows.Forms.TabPage tabErrors;
      private System.Windows.Forms.Button cmdErrorRefresh;
      private System.Windows.Forms.Button cmdErrorClear;
      private System.Windows.Forms.ListBox lbErrors;
      private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
      private System.Windows.Forms.TabPage tabGroups;
      private System.Windows.Forms.Button cmdGroupRefresh;
      private System.Windows.Forms.DataGridView dgGroups;
      private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
      private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
      private System.Windows.Forms.TabPage tabMessages;
      private System.Windows.Forms.DataGridView dgMessages;
      private System.Windows.Forms.DataGridViewTextBoxColumn colGroup;
      private System.Windows.Forms.DataGridViewTextBoxColumn colMessage;
      private System.Windows.Forms.DataGridViewTextBoxColumn colName;
      private System.Windows.Forms.Button cmdMessageLoad;
      private System.Windows.Forms.Button cmdMessageRefresh;
   }
}

