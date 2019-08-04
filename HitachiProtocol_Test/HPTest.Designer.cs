namespace HitachiProtocol_Test {
   partial class HPTest {
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
         this.ConfigureConnection = new System.Windows.Forms.TabControl();
         this.tabEthernet = new System.Windows.Forms.TabPage();
         this.tbPrinterPort = new System.Windows.Forms.TextBox();
         this.tbPrinterIPAddress = new System.Windows.Forms.TextBox();
         this.lblPrinterPort = new System.Windows.Forms.Label();
         this.lblPrinterIPAddress = new System.Windows.Forms.Label();
         this.tabSerial = new System.Windows.Forms.TabPage();
         this.lblPrinterPortName = new System.Windows.Forms.Label();
         this.cbPrinterPortName = new System.Windows.Forms.ComboBox();
         this.cbPrinterStopBits = new System.Windows.Forms.ComboBox();
         this.cbPrinterParity = new System.Windows.Forms.ComboBox();
         this.cbPrinterDataBits = new System.Windows.Forms.ComboBox();
         this.cbPrinterBaudRate = new System.Windows.Forms.ComboBox();
         this.lblPrinterStopBits = new System.Windows.Forms.Label();
         this.lblPrinterParity = new System.Windows.Forms.Label();
         this.lblPrinterDataBits = new System.Windows.Forms.Label();
         this.lblPrinterBaudRate = new System.Windows.Forms.Label();
         this.cmdConnect = new System.Windows.Forms.Button();
         this.cmdDisconnect = new System.Windows.Forms.Button();
         this.cmdSend = new System.Windows.Forms.Button();
         this.cmdExit = new System.Windows.Forms.Button();
         this.lbTraffic = new System.Windows.Forms.ListBox();
         this.TrafficMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cmTraffic = new System.Windows.Forms.ToolStripMenuItem();
         this.cmLoadInNotepad = new System.Windows.Forms.ToolStripMenuItem();
         this.tabSimulated = new System.Windows.Forms.TabPage();
         this.label1 = new System.Windows.Forms.Label();
         this.ConfigureConnection.SuspendLayout();
         this.tabEthernet.SuspendLayout();
         this.tabSerial.SuspendLayout();
         this.TrafficMenu.SuspendLayout();
         this.tabSimulated.SuspendLayout();
         this.SuspendLayout();
         // 
         // ConfigureConnection
         // 
         this.ConfigureConnection.Controls.Add(this.tabEthernet);
         this.ConfigureConnection.Controls.Add(this.tabSerial);
         this.ConfigureConnection.Controls.Add(this.tabSimulated);
         this.ConfigureConnection.Location = new System.Drawing.Point(21, 12);
         this.ConfigureConnection.Name = "ConfigureConnection";
         this.ConfigureConnection.SelectedIndex = 0;
         this.ConfigureConnection.Size = new System.Drawing.Size(264, 199);
         this.ConfigureConnection.TabIndex = 0;
         this.ConfigureConnection.SelectedIndexChanged += new System.EventHandler(this.SetButtonEnables);
         // 
         // tabEthernet
         // 
         this.tabEthernet.Controls.Add(this.tbPrinterPort);
         this.tabEthernet.Controls.Add(this.tbPrinterIPAddress);
         this.tabEthernet.Controls.Add(this.lblPrinterPort);
         this.tabEthernet.Controls.Add(this.lblPrinterIPAddress);
         this.tabEthernet.Location = new System.Drawing.Point(4, 25);
         this.tabEthernet.Name = "tabEthernet";
         this.tabEthernet.Padding = new System.Windows.Forms.Padding(3);
         this.tabEthernet.Size = new System.Drawing.Size(256, 170);
         this.tabEthernet.TabIndex = 0;
         this.tabEthernet.Text = "Ethernet";
         this.tabEthernet.UseVisualStyleBackColor = true;
         // 
         // tbPrinterPort
         // 
         this.tbPrinterPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.tbPrinterPort.Location = new System.Drawing.Point(109, 52);
         this.tbPrinterPort.Margin = new System.Windows.Forms.Padding(4);
         this.tbPrinterPort.Name = "tbPrinterPort";
         this.tbPrinterPort.Size = new System.Drawing.Size(140, 22);
         this.tbPrinterPort.TabIndex = 33;
         this.tbPrinterPort.Leave += new System.EventHandler(this.SetButtonEnables);
         // 
         // tbPrinterIPAddress
         // 
         this.tbPrinterIPAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.tbPrinterIPAddress.Location = new System.Drawing.Point(109, 18);
         this.tbPrinterIPAddress.Margin = new System.Windows.Forms.Padding(4);
         this.tbPrinterIPAddress.Name = "tbPrinterIPAddress";
         this.tbPrinterIPAddress.Size = new System.Drawing.Size(140, 22);
         this.tbPrinterIPAddress.TabIndex = 32;
         this.tbPrinterIPAddress.Leave += new System.EventHandler(this.SetButtonEnables);
         // 
         // lblPrinterPort
         // 
         this.lblPrinterPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.lblPrinterPort.Location = new System.Drawing.Point(16, 49);
         this.lblPrinterPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.lblPrinterPort.Name = "lblPrinterPort";
         this.lblPrinterPort.Size = new System.Drawing.Size(85, 25);
         this.lblPrinterPort.TabIndex = 31;
         this.lblPrinterPort.Text = "Port #";
         this.lblPrinterPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblPrinterIPAddress
         // 
         this.lblPrinterIPAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.lblPrinterIPAddress.Location = new System.Drawing.Point(16, 15);
         this.lblPrinterIPAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.lblPrinterIPAddress.Name = "lblPrinterIPAddress";
         this.lblPrinterIPAddress.Size = new System.Drawing.Size(85, 25);
         this.lblPrinterIPAddress.TabIndex = 30;
         this.lblPrinterIPAddress.Text = "IP Address";
         this.lblPrinterIPAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // tabSerial
         // 
         this.tabSerial.Controls.Add(this.lblPrinterPortName);
         this.tabSerial.Controls.Add(this.cbPrinterPortName);
         this.tabSerial.Controls.Add(this.cbPrinterStopBits);
         this.tabSerial.Controls.Add(this.cbPrinterParity);
         this.tabSerial.Controls.Add(this.cbPrinterDataBits);
         this.tabSerial.Controls.Add(this.cbPrinterBaudRate);
         this.tabSerial.Controls.Add(this.lblPrinterStopBits);
         this.tabSerial.Controls.Add(this.lblPrinterParity);
         this.tabSerial.Controls.Add(this.lblPrinterDataBits);
         this.tabSerial.Controls.Add(this.lblPrinterBaudRate);
         this.tabSerial.Location = new System.Drawing.Point(4, 25);
         this.tabSerial.Name = "tabSerial";
         this.tabSerial.Padding = new System.Windows.Forms.Padding(3);
         this.tabSerial.Size = new System.Drawing.Size(256, 170);
         this.tabSerial.TabIndex = 1;
         this.tabSerial.Text = "Serial";
         this.tabSerial.UseVisualStyleBackColor = true;
         // 
         // lblPrinterPortName
         // 
         this.lblPrinterPortName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.lblPrinterPortName.Location = new System.Drawing.Point(19, 14);
         this.lblPrinterPortName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.lblPrinterPortName.Name = "lblPrinterPortName";
         this.lblPrinterPortName.Size = new System.Drawing.Size(99, 25);
         this.lblPrinterPortName.TabIndex = 65;
         this.lblPrinterPortName.Text = "Port Name";
         this.lblPrinterPortName.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // cbPrinterPortName
         // 
         this.cbPrinterPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbPrinterPortName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.cbPrinterPortName.FormattingEnabled = true;
         this.cbPrinterPortName.Location = new System.Drawing.Point(125, 12);
         this.cbPrinterPortName.Margin = new System.Windows.Forms.Padding(4);
         this.cbPrinterPortName.Name = "cbPrinterPortName";
         this.cbPrinterPortName.Size = new System.Drawing.Size(124, 24);
         this.cbPrinterPortName.TabIndex = 64;
         this.cbPrinterPortName.SelectedIndexChanged += new System.EventHandler(this.SetButtonEnables);
         // 
         // cbPrinterStopBits
         // 
         this.cbPrinterStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbPrinterStopBits.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.cbPrinterStopBits.FormattingEnabled = true;
         this.cbPrinterStopBits.Items.AddRange(new object[] {
            "One",
            "Two"});
         this.cbPrinterStopBits.Location = new System.Drawing.Point(125, 135);
         this.cbPrinterStopBits.Margin = new System.Windows.Forms.Padding(4);
         this.cbPrinterStopBits.Name = "cbPrinterStopBits";
         this.cbPrinterStopBits.Size = new System.Drawing.Size(124, 24);
         this.cbPrinterStopBits.TabIndex = 63;
         this.cbPrinterStopBits.SelectedIndexChanged += new System.EventHandler(this.SetButtonEnables);
         // 
         // cbPrinterParity
         // 
         this.cbPrinterParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbPrinterParity.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.cbPrinterParity.FormattingEnabled = true;
         this.cbPrinterParity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even",
            "Mark",
            "Space"});
         this.cbPrinterParity.Location = new System.Drawing.Point(125, 105);
         this.cbPrinterParity.Margin = new System.Windows.Forms.Padding(4);
         this.cbPrinterParity.Name = "cbPrinterParity";
         this.cbPrinterParity.Size = new System.Drawing.Size(124, 24);
         this.cbPrinterParity.TabIndex = 62;
         this.cbPrinterParity.SelectedIndexChanged += new System.EventHandler(this.SetButtonEnables);
         // 
         // cbPrinterDataBits
         // 
         this.cbPrinterDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbPrinterDataBits.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.cbPrinterDataBits.FormattingEnabled = true;
         this.cbPrinterDataBits.Items.AddRange(new object[] {
            "7",
            "8"});
         this.cbPrinterDataBits.Location = new System.Drawing.Point(125, 74);
         this.cbPrinterDataBits.Margin = new System.Windows.Forms.Padding(4);
         this.cbPrinterDataBits.Name = "cbPrinterDataBits";
         this.cbPrinterDataBits.Size = new System.Drawing.Size(124, 24);
         this.cbPrinterDataBits.TabIndex = 61;
         this.cbPrinterDataBits.SelectedIndexChanged += new System.EventHandler(this.SetButtonEnables);
         // 
         // cbPrinterBaudRate
         // 
         this.cbPrinterBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbPrinterBaudRate.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.cbPrinterBaudRate.FormattingEnabled = true;
         this.cbPrinterBaudRate.Items.AddRange(new object[] {
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "38400",
            "56000",
            "57600",
            "115200"});
         this.cbPrinterBaudRate.Location = new System.Drawing.Point(125, 43);
         this.cbPrinterBaudRate.Margin = new System.Windows.Forms.Padding(4);
         this.cbPrinterBaudRate.Name = "cbPrinterBaudRate";
         this.cbPrinterBaudRate.Size = new System.Drawing.Size(124, 24);
         this.cbPrinterBaudRate.TabIndex = 60;
         this.cbPrinterBaudRate.SelectedIndexChanged += new System.EventHandler(this.SetButtonEnables);
         // 
         // lblPrinterStopBits
         // 
         this.lblPrinterStopBits.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.lblPrinterStopBits.Location = new System.Drawing.Point(19, 135);
         this.lblPrinterStopBits.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.lblPrinterStopBits.Name = "lblPrinterStopBits";
         this.lblPrinterStopBits.Size = new System.Drawing.Size(99, 25);
         this.lblPrinterStopBits.TabIndex = 59;
         this.lblPrinterStopBits.Text = "Stop Bits";
         this.lblPrinterStopBits.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblPrinterParity
         // 
         this.lblPrinterParity.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.lblPrinterParity.Location = new System.Drawing.Point(19, 106);
         this.lblPrinterParity.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.lblPrinterParity.Name = "lblPrinterParity";
         this.lblPrinterParity.Size = new System.Drawing.Size(99, 25);
         this.lblPrinterParity.TabIndex = 58;
         this.lblPrinterParity.Text = "Parity";
         this.lblPrinterParity.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblPrinterDataBits
         // 
         this.lblPrinterDataBits.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.lblPrinterDataBits.Location = new System.Drawing.Point(19, 75);
         this.lblPrinterDataBits.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.lblPrinterDataBits.Name = "lblPrinterDataBits";
         this.lblPrinterDataBits.Size = new System.Drawing.Size(99, 25);
         this.lblPrinterDataBits.TabIndex = 57;
         this.lblPrinterDataBits.Text = "Data Bits";
         this.lblPrinterDataBits.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblPrinterBaudRate
         // 
         this.lblPrinterBaudRate.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.lblPrinterBaudRate.Location = new System.Drawing.Point(19, 43);
         this.lblPrinterBaudRate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.lblPrinterBaudRate.Name = "lblPrinterBaudRate";
         this.lblPrinterBaudRate.Size = new System.Drawing.Size(99, 25);
         this.lblPrinterBaudRate.TabIndex = 56;
         this.lblPrinterBaudRate.Text = "Baud Rate";
         this.lblPrinterBaudRate.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // cmdConnect
         // 
         this.cmdConnect.Location = new System.Drawing.Point(317, 37);
         this.cmdConnect.Name = "cmdConnect";
         this.cmdConnect.Size = new System.Drawing.Size(163, 39);
         this.cmdConnect.TabIndex = 1;
         this.cmdConnect.Text = "Connect";
         this.cmdConnect.UseVisualStyleBackColor = true;
         this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
         // 
         // cmdDisconnect
         // 
         this.cmdDisconnect.Location = new System.Drawing.Point(317, 82);
         this.cmdDisconnect.Name = "cmdDisconnect";
         this.cmdDisconnect.Size = new System.Drawing.Size(163, 39);
         this.cmdDisconnect.TabIndex = 2;
         this.cmdDisconnect.Text = "Disconnect";
         this.cmdDisconnect.UseVisualStyleBackColor = true;
         this.cmdDisconnect.Click += new System.EventHandler(this.cmdDisconnect_Click);
         // 
         // cmdSend
         // 
         this.cmdSend.Location = new System.Drawing.Point(317, 127);
         this.cmdSend.Name = "cmdSend";
         this.cmdSend.Size = new System.Drawing.Size(163, 39);
         this.cmdSend.TabIndex = 3;
         this.cmdSend.Text = "Send";
         this.cmdSend.UseVisualStyleBackColor = true;
         this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);
         // 
         // cmdExit
         // 
         this.cmdExit.Location = new System.Drawing.Point(317, 172);
         this.cmdExit.Name = "cmdExit";
         this.cmdExit.Size = new System.Drawing.Size(163, 39);
         this.cmdExit.TabIndex = 4;
         this.cmdExit.Text = "Exit";
         this.cmdExit.UseVisualStyleBackColor = true;
         this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
         // 
         // lbTraffic
         // 
         this.lbTraffic.ContextMenuStrip = this.TrafficMenu;
         this.lbTraffic.FormattingEnabled = true;
         this.lbTraffic.ItemHeight = 16;
         this.lbTraffic.Location = new System.Drawing.Point(21, 222);
         this.lbTraffic.Name = "lbTraffic";
         this.lbTraffic.Size = new System.Drawing.Size(459, 164);
         this.lbTraffic.TabIndex = 5;
         // 
         // TrafficMenu
         // 
         this.TrafficMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.TrafficMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmTraffic,
            this.cmLoadInNotepad});
         this.TrafficMenu.Name = "TrafficMenu";
         this.TrafficMenu.Size = new System.Drawing.Size(189, 52);
         // 
         // cmTraffic
         // 
         this.cmTraffic.Name = "cmTraffic";
         this.cmTraffic.Size = new System.Drawing.Size(188, 24);
         this.cmTraffic.Text = "Clear";
         this.cmTraffic.Click += new System.EventHandler(this.cmTraffic_Click);
         // 
         // cmLoadInNotepad
         // 
         this.cmLoadInNotepad.Name = "cmLoadInNotepad";
         this.cmLoadInNotepad.Size = new System.Drawing.Size(188, 24);
         this.cmLoadInNotepad.Text = "Load In NotePad";
         this.cmLoadInNotepad.Click += new System.EventHandler(this.cmLoadInNotepad_Click);
         // 
         // tabSimulated
         // 
         this.tabSimulated.Controls.Add(this.label1);
         this.tabSimulated.Location = new System.Drawing.Point(4, 25);
         this.tabSimulated.Name = "tabSimulated";
         this.tabSimulated.Size = new System.Drawing.Size(256, 170);
         this.tabSimulated.TabIndex = 2;
         this.tabSimulated.Text = "Simulated";
         this.tabSimulated.UseVisualStyleBackColor = true;
         // 
         // label1
         // 
         this.label1.Location = new System.Drawing.Point(21, 22);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(217, 23);
         this.label1.TabIndex = 6;
         this.label1.Text = "No parameters needed!";
         // 
         // HPTest
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(505, 402);
         this.Controls.Add(this.lbTraffic);
         this.Controls.Add(this.cmdExit);
         this.Controls.Add(this.cmdSend);
         this.Controls.Add(this.cmdDisconnect);
         this.Controls.Add(this.cmdConnect);
         this.Controls.Add(this.ConfigureConnection);
         this.Name = "HPTest";
         this.Text = "Test Hitachi Printer DLL";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HPTest_FormClosing);
         this.Load += new System.EventHandler(this.HPTest_Load);
         this.ConfigureConnection.ResumeLayout(false);
         this.tabEthernet.ResumeLayout(false);
         this.tabEthernet.PerformLayout();
         this.tabSerial.ResumeLayout(false);
         this.TrafficMenu.ResumeLayout(false);
         this.tabSimulated.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TabControl ConfigureConnection;
      private System.Windows.Forms.TabPage tabEthernet;
      private System.Windows.Forms.TextBox tbPrinterPort;
      private System.Windows.Forms.TextBox tbPrinterIPAddress;
      private System.Windows.Forms.Label lblPrinterPort;
      private System.Windows.Forms.Label lblPrinterIPAddress;
      private System.Windows.Forms.TabPage tabSerial;
      private System.Windows.Forms.Label lblPrinterPortName;
      private System.Windows.Forms.ComboBox cbPrinterPortName;
      private System.Windows.Forms.ComboBox cbPrinterStopBits;
      private System.Windows.Forms.ComboBox cbPrinterParity;
      private System.Windows.Forms.ComboBox cbPrinterDataBits;
      private System.Windows.Forms.ComboBox cbPrinterBaudRate;
      private System.Windows.Forms.Label lblPrinterStopBits;
      private System.Windows.Forms.Label lblPrinterParity;
      private System.Windows.Forms.Label lblPrinterDataBits;
      private System.Windows.Forms.Label lblPrinterBaudRate;
      private System.Windows.Forms.Button cmdConnect;
      private System.Windows.Forms.Button cmdDisconnect;
      private System.Windows.Forms.Button cmdSend;
      private System.Windows.Forms.Button cmdExit;
      private System.Windows.Forms.ListBox lbTraffic;
      private System.Windows.Forms.ContextMenuStrip TrafficMenu;
      private System.Windows.Forms.ToolStripMenuItem cmTraffic;
      private System.Windows.Forms.ToolStripMenuItem cmLoadInNotepad;
      private System.Windows.Forms.TabPage tabSimulated;
      private System.Windows.Forms.Label label1;
   }
}

