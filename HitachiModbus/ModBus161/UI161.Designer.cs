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
         this.tabObject = new System.Windows.Forms.TabPage();
         this.tabLog = new System.Windows.Forms.TabPage();
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
         this.cmLog.SuspendLayout();
         this.tclViews.SuspendLayout();
         this.tabXML.SuspendLayout();
         this.tabIndented.SuspendLayout();
         this.tabLog.SuspendLayout();
         this.SuspendLayout();
         // 
         // lblIPAddress
         // 
         this.lblIPAddress.Location = new System.Drawing.Point(30, 61);
         this.lblIPAddress.Name = "lblIPAddress";
         this.lblIPAddress.Size = new System.Drawing.Size(107, 22);
         this.lblIPAddress.TabIndex = 0;
         this.lblIPAddress.Text = "IP Address";
         this.lblIPAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblIPPort
         // 
         this.lblIPPort.Location = new System.Drawing.Point(30, 89);
         this.lblIPPort.Name = "lblIPPort";
         this.lblIPPort.Size = new System.Drawing.Size(107, 28);
         this.lblIPPort.TabIndex = 1;
         this.lblIPPort.Text = "IP Port";
         this.lblIPPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtIPAddress
         // 
         this.txtIPAddress.Location = new System.Drawing.Point(143, 61);
         this.txtIPAddress.Name = "txtIPAddress";
         this.txtIPAddress.Size = new System.Drawing.Size(137, 22);
         this.txtIPAddress.TabIndex = 2;
         this.txtIPAddress.Text = "192.168.168.100";
         this.txtIPAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // txtIPPort
         // 
         this.txtIPPort.Location = new System.Drawing.Point(143, 89);
         this.txtIPPort.Name = "txtIPPort";
         this.txtIPPort.Size = new System.Drawing.Size(137, 22);
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
         this.cmdDisconnect.Location = new System.Drawing.Point(286, 102);
         this.cmdDisconnect.Name = "cmdDisconnect";
         this.cmdDisconnect.Size = new System.Drawing.Size(105, 35);
         this.cmdDisconnect.TabIndex = 9;
         this.cmdDisconnect.Text = "Disconnect";
         this.cmdDisconnect.UseVisualStyleBackColor = true;
         this.cmdDisconnect.Click += new System.EventHandler(this.cmdDisconnect_Click);
         // 
         // cmdConnect
         // 
         this.cmdConnect.Location = new System.Drawing.Point(286, 61);
         this.cmdConnect.Name = "cmdConnect";
         this.cmdConnect.Size = new System.Drawing.Size(105, 35);
         this.cmdConnect.TabIndex = 10;
         this.cmdConnect.Text = "Connect";
         this.cmdConnect.UseVisualStyleBackColor = true;
         this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
         // 
         // cmdExit
         // 
         this.cmdExit.Location = new System.Drawing.Point(675, 102);
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
         this.cmdComOff.Location = new System.Drawing.Point(392, 102);
         this.cmdComOff.Name = "cmdComOff";
         this.cmdComOff.Size = new System.Drawing.Size(105, 35);
         this.cmdComOff.TabIndex = 16;
         this.cmdComOff.Text = "Com Off";
         this.cmdComOff.UseVisualStyleBackColor = true;
         this.cmdComOff.Click += new System.EventHandler(this.cmdComOff_Click);
         // 
         // cmdComOn
         // 
         this.cmdComOn.Location = new System.Drawing.Point(392, 61);
         this.cmdComOn.Name = "cmdComOn";
         this.cmdComOn.Size = new System.Drawing.Size(105, 35);
         this.cmdComOn.TabIndex = 17;
         this.cmdComOn.Text = "Com On";
         this.cmdComOn.UseVisualStyleBackColor = true;
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
         this.txtDataLength.Location = new System.Drawing.Point(416, 611);
         this.txtDataLength.Name = "txtDataLength";
         this.txtDataLength.Size = new System.Drawing.Size(81, 22);
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
         this.lblDataLength.Location = new System.Drawing.Point(323, 611);
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
         this.txtData.Location = new System.Drawing.Point(416, 643);
         this.txtData.Name = "txtData";
         this.txtData.Size = new System.Drawing.Size(222, 22);
         this.txtData.TabIndex = 24;
         // 
         // lblData
         // 
         this.lblData.Location = new System.Drawing.Point(323, 643);
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
         this.optInputRegister.Location = new System.Drawing.Point(504, 612);
         this.optInputRegister.Name = "optInputRegister";
         this.optInputRegister.Size = new System.Drawing.Size(134, 21);
         this.optInputRegister.TabIndex = 26;
         this.optInputRegister.Text = "Input Register";
         this.optInputRegister.UseVisualStyleBackColor = true;
         // 
         // tclViews
         // 
         this.tclViews.Controls.Add(this.tabXML);
         this.tclViews.Controls.Add(this.tabIndented);
         this.tclViews.Controls.Add(this.tabObject);
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
         this.txtIndentedView.Location = new System.Drawing.Point(15, 6);
         this.txtIndentedView.Multiline = true;
         this.txtIndentedView.Name = "txtIndentedView";
         this.txtIndentedView.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtIndentedView.Size = new System.Drawing.Size(728, 376);
         this.txtIndentedView.TabIndex = 0;
         // 
         // tabObject
         // 
         this.tabObject.Location = new System.Drawing.Point(4, 25);
         this.tabObject.Name = "tabObject";
         this.tabObject.Size = new System.Drawing.Size(768, 393);
         this.tabObject.TabIndex = 2;
         this.tabObject.Text = "Object View";
         this.tabObject.UseVisualStyleBackColor = true;
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
         // cmdRetrieve
         // 
         this.cmdRetrieve.Location = new System.Drawing.Point(19, 701);
         this.cmdRetrieve.Name = "cmdRetrieve";
         this.cmdRetrieve.Size = new System.Drawing.Size(105, 40);
         this.cmdRetrieve.TabIndex = 28;
         this.cmdRetrieve.Text = "Retrieve";
         this.cmdRetrieve.UseVisualStyleBackColor = true;
         this.cmdRetrieve.Click += new System.EventHandler(this.cmdRetrieve_Click);
         // 
         // cmdReformat
         // 
         this.cmdReformat.Enabled = false;
         this.cmdReformat.Location = new System.Drawing.Point(675, 59);
         this.cmdReformat.Name = "cmdReformat";
         this.cmdReformat.Size = new System.Drawing.Size(105, 35);
         this.cmdReformat.TabIndex = 29;
         this.cmdReformat.Text = "Reformat";
         this.cmdReformat.UseVisualStyleBackColor = true;
         this.cmdReformat.Click += new System.EventHandler(this.cmdReformat_Click);
         // 
         // cmdSaveAs
         // 
         this.cmdSaveAs.Location = new System.Drawing.Point(130, 701);
         this.cmdSaveAs.Name = "cmdSaveAs";
         this.cmdSaveAs.Size = new System.Drawing.Size(105, 40);
         this.cmdSaveAs.TabIndex = 30;
         this.cmdSaveAs.Text = "Save As";
         this.cmdSaveAs.UseVisualStyleBackColor = true;
         this.cmdSaveAs.Click += new System.EventHandler(this.cmdSaveAs_Click);
         // 
         // cmdOpen
         // 
         this.cmdOpen.Location = new System.Drawing.Point(241, 701);
         this.cmdOpen.Name = "cmdOpen";
         this.cmdOpen.Size = new System.Drawing.Size(105, 40);
         this.cmdOpen.TabIndex = 32;
         this.cmdOpen.Text = "Open";
         this.cmdOpen.UseVisualStyleBackColor = true;
         this.cmdOpen.Click += new System.EventHandler(this.cmdOpen_Click);
         // 
         // cmdSend
         // 
         this.cmdSend.Location = new System.Drawing.Point(352, 701);
         this.cmdSend.Name = "cmdSend";
         this.cmdSend.Size = new System.Drawing.Size(105, 40);
         this.cmdSend.TabIndex = 31;
         this.cmdSend.Text = "Send";
         this.cmdSend.UseVisualStyleBackColor = true;
         this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);
         // 
         // lblMessageFolder
         // 
         this.lblMessageFolder.Location = new System.Drawing.Point(16, 27);
         this.lblMessageFolder.Name = "lblMessageFolder";
         this.lblMessageFolder.Size = new System.Drawing.Size(121, 22);
         this.lblMessageFolder.TabIndex = 33;
         this.lblMessageFolder.Text = "Message Folder";
         this.lblMessageFolder.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtMessageFolder
         // 
         this.txtMessageFolder.Location = new System.Drawing.Point(143, 24);
         this.txtMessageFolder.Name = "txtMessageFolder";
         this.txtMessageFolder.Size = new System.Drawing.Size(518, 22);
         this.txtMessageFolder.TabIndex = 34;
         this.txtMessageFolder.Text = "192.168.168.100";
         // 
         // cmdBrowse
         // 
         this.cmdBrowse.Location = new System.Drawing.Point(675, 18);
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
         this.cmdExperiment.Location = new System.Drawing.Point(653, 701);
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
         // UI161
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(800, 753);
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
         this.Name = "UI161";
         this.Text = "Modbus for Hitachi UX Model 161 Printer";
         this.cmLog.ResumeLayout(false);
         this.tclViews.ResumeLayout(false);
         this.tabXML.ResumeLayout(false);
         this.tabIndented.ResumeLayout(false);
         this.tabIndented.PerformLayout();
         this.tabLog.ResumeLayout(false);
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
      private System.Windows.Forms.TabPage tabObject;
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
   }
}

