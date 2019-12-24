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
         this.comOn = new System.Windows.Forms.Button();
         this.cmdWriteData = new System.Windows.Forms.Button();
         this.txtDataLength = new System.Windows.Forms.TextBox();
         this.txtDataAddress = new System.Windows.Forms.TextBox();
         this.lblDataLength = new System.Windows.Forms.Label();
         this.lblDataAddress = new System.Windows.Forms.Label();
         this.txtData = new System.Windows.Forms.TextBox();
         this.lblData = new System.Windows.Forms.Label();
         this.optHoldingRegister = new System.Windows.Forms.RadioButton();
         this.optInputRegister = new System.Windows.Forms.RadioButton();
         this.tabControl1 = new System.Windows.Forms.TabControl();
         this.tabXML = new System.Windows.Forms.TabPage();
         this.tvXML = new System.Windows.Forms.TreeView();
         this.tabIndented = new System.Windows.Forms.TabPage();
         this.txtIndentedView = new System.Windows.Forms.TextBox();
         this.tabObject = new System.Windows.Forms.TabPage();
         this.tabLog = new System.Windows.Forms.TabPage();
         this.cmdRetrieve = new System.Windows.Forms.Button();
         this.cmLog.SuspendLayout();
         this.tabControl1.SuspendLayout();
         this.tabXML.SuspendLayout();
         this.tabIndented.SuspendLayout();
         this.tabLog.SuspendLayout();
         this.SuspendLayout();
         // 
         // lblIPAddress
         // 
         this.lblIPAddress.Location = new System.Drawing.Point(28, 12);
         this.lblIPAddress.Name = "lblIPAddress";
         this.lblIPAddress.Size = new System.Drawing.Size(107, 22);
         this.lblIPAddress.TabIndex = 0;
         this.lblIPAddress.Text = "IP Address";
         this.lblIPAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblIPPort
         // 
         this.lblIPPort.Location = new System.Drawing.Point(28, 40);
         this.lblIPPort.Name = "lblIPPort";
         this.lblIPPort.Size = new System.Drawing.Size(107, 28);
         this.lblIPPort.TabIndex = 1;
         this.lblIPPort.Text = "IP Port";
         this.lblIPPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtIPAddress
         // 
         this.txtIPAddress.Location = new System.Drawing.Point(141, 12);
         this.txtIPAddress.Name = "txtIPAddress";
         this.txtIPAddress.Size = new System.Drawing.Size(137, 22);
         this.txtIPAddress.TabIndex = 2;
         this.txtIPAddress.Text = "192.168.168.100";
         this.txtIPAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // txtIPPort
         // 
         this.txtIPPort.Location = new System.Drawing.Point(141, 40);
         this.txtIPPort.Name = "txtIPPort";
         this.txtIPPort.Size = new System.Drawing.Size(137, 22);
         this.txtIPPort.TabIndex = 3;
         this.txtIPPort.Text = "502";
         this.txtIPPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // cmdReadData
         // 
         this.cmdReadData.Location = new System.Drawing.Point(16, 576);
         this.cmdReadData.Name = "cmdReadData";
         this.cmdReadData.Size = new System.Drawing.Size(127, 40);
         this.cmdReadData.TabIndex = 8;
         this.cmdReadData.Text = "Read Data";
         this.cmdReadData.UseVisualStyleBackColor = true;
         this.cmdReadData.Click += new System.EventHandler(this.cmdReadData_Click);
         // 
         // cmdDisconnect
         // 
         this.cmdDisconnect.Location = new System.Drawing.Point(298, 53);
         this.cmdDisconnect.Name = "cmdDisconnect";
         this.cmdDisconnect.Size = new System.Drawing.Size(127, 35);
         this.cmdDisconnect.TabIndex = 9;
         this.cmdDisconnect.Text = "Disconnect";
         this.cmdDisconnect.UseVisualStyleBackColor = true;
         // 
         // cmdConnect
         // 
         this.cmdConnect.Location = new System.Drawing.Point(298, 12);
         this.cmdConnect.Name = "cmdConnect";
         this.cmdConnect.Size = new System.Drawing.Size(127, 35);
         this.cmdConnect.TabIndex = 10;
         this.cmdConnect.Text = "Connect";
         this.cmdConnect.UseVisualStyleBackColor = true;
         this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
         // 
         // cmdExit
         // 
         this.cmdExit.Location = new System.Drawing.Point(617, 17);
         this.cmdExit.Name = "cmdExit";
         this.cmdExit.Size = new System.Drawing.Size(127, 45);
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
         this.lstMessages.Size = new System.Drawing.Size(737, 404);
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
         this.cmdComOff.Location = new System.Drawing.Point(445, 53);
         this.cmdComOff.Name = "cmdComOff";
         this.cmdComOff.Size = new System.Drawing.Size(127, 35);
         this.cmdComOff.TabIndex = 16;
         this.cmdComOff.Text = "Com Off";
         this.cmdComOff.UseVisualStyleBackColor = true;
         this.cmdComOff.Click += new System.EventHandler(this.cmdComOff_Click);
         // 
         // comOn
         // 
         this.comOn.Location = new System.Drawing.Point(445, 12);
         this.comOn.Name = "comOn";
         this.comOn.Size = new System.Drawing.Size(127, 35);
         this.comOn.TabIndex = 17;
         this.comOn.Text = "Com On";
         this.comOn.UseVisualStyleBackColor = true;
         this.comOn.Click += new System.EventHandler(this.comOn_Click);
         // 
         // cmdWriteData
         // 
         this.cmdWriteData.Location = new System.Drawing.Point(16, 631);
         this.cmdWriteData.Name = "cmdWriteData";
         this.cmdWriteData.Size = new System.Drawing.Size(127, 40);
         this.cmdWriteData.TabIndex = 18;
         this.cmdWriteData.Text = "Write Data";
         this.cmdWriteData.UseVisualStyleBackColor = true;
         this.cmdWriteData.Click += new System.EventHandler(this.cmdWriteData_Click);
         // 
         // txtDataLength
         // 
         this.txtDataLength.Location = new System.Drawing.Point(284, 608);
         this.txtDataLength.Name = "txtDataLength";
         this.txtDataLength.Size = new System.Drawing.Size(137, 22);
         this.txtDataLength.TabIndex = 22;
         this.txtDataLength.Text = "1";
         this.txtDataLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // txtDataAddress
         // 
         this.txtDataAddress.Location = new System.Drawing.Point(284, 576);
         this.txtDataAddress.Name = "txtDataAddress";
         this.txtDataAddress.Size = new System.Drawing.Size(137, 22);
         this.txtDataAddress.TabIndex = 21;
         this.txtDataAddress.Text = "0x2490";
         this.txtDataAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblDataLength
         // 
         this.lblDataLength.Location = new System.Drawing.Point(171, 608);
         this.lblDataLength.Name = "lblDataLength";
         this.lblDataLength.Size = new System.Drawing.Size(107, 28);
         this.lblDataLength.TabIndex = 20;
         this.lblDataLength.Text = "Length";
         this.lblDataLength.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblDataAddress
         // 
         this.lblDataAddress.Location = new System.Drawing.Point(171, 576);
         this.lblDataAddress.Name = "lblDataAddress";
         this.lblDataAddress.Size = new System.Drawing.Size(107, 22);
         this.lblDataAddress.TabIndex = 19;
         this.lblDataAddress.Text = "Data Address";
         this.lblDataAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtData
         // 
         this.txtData.Location = new System.Drawing.Point(284, 640);
         this.txtData.Name = "txtData";
         this.txtData.Size = new System.Drawing.Size(296, 22);
         this.txtData.TabIndex = 24;
         // 
         // lblData
         // 
         this.lblData.Location = new System.Drawing.Point(171, 640);
         this.lblData.Name = "lblData";
         this.lblData.Size = new System.Drawing.Size(107, 28);
         this.lblData.TabIndex = 23;
         this.lblData.Text = "Data";
         this.lblData.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // optHoldingRegister
         // 
         this.optHoldingRegister.AutoSize = true;
         this.optHoldingRegister.Checked = true;
         this.optHoldingRegister.Location = new System.Drawing.Point(445, 577);
         this.optHoldingRegister.Name = "optHoldingRegister";
         this.optHoldingRegister.Size = new System.Drawing.Size(134, 21);
         this.optHoldingRegister.TabIndex = 25;
         this.optHoldingRegister.TabStop = true;
         this.optHoldingRegister.Text = "Holding Register";
         this.optHoldingRegister.UseVisualStyleBackColor = true;
         // 
         // optInputRegister
         // 
         this.optInputRegister.AutoSize = true;
         this.optInputRegister.Location = new System.Drawing.Point(445, 609);
         this.optInputRegister.Name = "optInputRegister";
         this.optInputRegister.Size = new System.Drawing.Size(117, 21);
         this.optInputRegister.TabIndex = 26;
         this.optInputRegister.Text = "Input Register";
         this.optInputRegister.UseVisualStyleBackColor = true;
         // 
         // tabControl1
         // 
         this.tabControl1.Controls.Add(this.tabXML);
         this.tabControl1.Controls.Add(this.tabIndented);
         this.tabControl1.Controls.Add(this.tabObject);
         this.tabControl1.Controls.Add(this.tabLog);
         this.tabControl1.Location = new System.Drawing.Point(12, 94);
         this.tabControl1.Name = "tabControl1";
         this.tabControl1.SelectedIndex = 0;
         this.tabControl1.Size = new System.Drawing.Size(776, 476);
         this.tabControl1.TabIndex = 27;
         // 
         // tabXML
         // 
         this.tabXML.Controls.Add(this.tvXML);
         this.tabXML.Location = new System.Drawing.Point(4, 25);
         this.tabXML.Name = "tabXML";
         this.tabXML.Padding = new System.Windows.Forms.Padding(3);
         this.tabXML.Size = new System.Drawing.Size(768, 447);
         this.tabXML.TabIndex = 0;
         this.tabXML.Text = "XML View";
         this.tabXML.UseVisualStyleBackColor = true;
         // 
         // tvXML
         // 
         this.tvXML.Location = new System.Drawing.Point(15, 16);
         this.tvXML.Name = "tvXML";
         this.tvXML.Size = new System.Drawing.Size(727, 425);
         this.tvXML.TabIndex = 0;
         // 
         // tabIndented
         // 
         this.tabIndented.Controls.Add(this.txtIndentedView);
         this.tabIndented.Location = new System.Drawing.Point(4, 25);
         this.tabIndented.Name = "tabIndented";
         this.tabIndented.Padding = new System.Windows.Forms.Padding(3);
         this.tabIndented.Size = new System.Drawing.Size(768, 447);
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
         this.txtIndentedView.Size = new System.Drawing.Size(728, 424);
         this.txtIndentedView.TabIndex = 0;
         // 
         // tabObject
         // 
         this.tabObject.Location = new System.Drawing.Point(4, 25);
         this.tabObject.Name = "tabObject";
         this.tabObject.Size = new System.Drawing.Size(768, 447);
         this.tabObject.TabIndex = 2;
         this.tabObject.Text = "Object View";
         this.tabObject.UseVisualStyleBackColor = true;
         // 
         // tabLog
         // 
         this.tabLog.Controls.Add(this.lstMessages);
         this.tabLog.Location = new System.Drawing.Point(4, 25);
         this.tabLog.Name = "tabLog";
         this.tabLog.Size = new System.Drawing.Size(768, 447);
         this.tabLog.TabIndex = 3;
         this.tabLog.Text = "Log";
         this.tabLog.UseVisualStyleBackColor = true;
         // 
         // cmdRetrieve
         // 
         this.cmdRetrieve.Location = new System.Drawing.Point(617, 577);
         this.cmdRetrieve.Name = "cmdRetrieve";
         this.cmdRetrieve.Size = new System.Drawing.Size(127, 40);
         this.cmdRetrieve.TabIndex = 28;
         this.cmdRetrieve.Text = "Retrieve";
         this.cmdRetrieve.UseVisualStyleBackColor = true;
         this.cmdRetrieve.Click += new System.EventHandler(this.cmdRetrieve_Click);
         // 
         // UI161
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(800, 683);
         this.Controls.Add(this.cmdRetrieve);
         this.Controls.Add(this.tabControl1);
         this.Controls.Add(this.optInputRegister);
         this.Controls.Add(this.optHoldingRegister);
         this.Controls.Add(this.txtData);
         this.Controls.Add(this.lblData);
         this.Controls.Add(this.txtDataLength);
         this.Controls.Add(this.txtDataAddress);
         this.Controls.Add(this.lblDataLength);
         this.Controls.Add(this.lblDataAddress);
         this.Controls.Add(this.cmdWriteData);
         this.Controls.Add(this.comOn);
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
         this.tabControl1.ResumeLayout(false);
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
      private System.Windows.Forms.TextBox txtIPAddress;
      private System.Windows.Forms.TextBox txtIPPort;
      private System.Windows.Forms.Button cmdReadData;
      private System.Windows.Forms.Button cmdDisconnect;
      private System.Windows.Forms.Button cmdConnect;
      private System.Windows.Forms.Button cmdExit;
      private System.Windows.Forms.ListBox lstMessages;
      private System.Windows.Forms.Button cmdComOff;
      private System.Windows.Forms.Button comOn;
      private System.Windows.Forms.Button cmdWriteData;
      private System.Windows.Forms.TextBox txtDataLength;
      private System.Windows.Forms.TextBox txtDataAddress;
      private System.Windows.Forms.Label lblDataLength;
      private System.Windows.Forms.Label lblDataAddress;
      private System.Windows.Forms.TextBox txtData;
      private System.Windows.Forms.Label lblData;
      private System.Windows.Forms.RadioButton optHoldingRegister;
      private System.Windows.Forms.RadioButton optInputRegister;
      private System.Windows.Forms.TabControl tabControl1;
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
   }
}

