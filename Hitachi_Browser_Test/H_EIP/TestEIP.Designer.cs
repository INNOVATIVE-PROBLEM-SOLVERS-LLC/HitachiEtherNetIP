namespace H_EIP {
   partial class TestEIP {
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
         this.cmdSendToPrinter = new System.Windows.Forms.Button();
         this.cmdViewTraffic = new System.Windows.Forms.Button();
         this.cmdStartBrowser = new System.Windows.Forms.Button();
         this.cmdExit = new System.Windows.Forms.Button();
         this.lblIPAddress = new System.Windows.Forms.Label();
         this.lblPort = new System.Windows.Forms.Label();
         this.txtIPAddress = new System.Windows.Forms.TextBox();
         this.txtPort = new System.Windows.Forms.TextBox();
         this.lblMessageFolder = new System.Windows.Forms.Label();
         this.txtMessageFolder = new System.Windows.Forms.TextBox();
         this.cmdBrowse = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // cmdSendToPrinter
         // 
         this.cmdSendToPrinter.Location = new System.Drawing.Point(24, 123);
         this.cmdSendToPrinter.Name = "cmdSendToPrinter";
         this.cmdSendToPrinter.Size = new System.Drawing.Size(173, 34);
         this.cmdSendToPrinter.TabIndex = 0;
         this.cmdSendToPrinter.Text = "Run Test";
         this.cmdSendToPrinter.UseVisualStyleBackColor = true;
         this.cmdSendToPrinter.Click += new System.EventHandler(this.cmdTest_Click);
         // 
         // cmdViewTraffic
         // 
         this.cmdViewTraffic.Location = new System.Drawing.Point(24, 174);
         this.cmdViewTraffic.Name = "cmdViewTraffic";
         this.cmdViewTraffic.Size = new System.Drawing.Size(173, 34);
         this.cmdViewTraffic.TabIndex = 1;
         this.cmdViewTraffic.Text = "View Traffic";
         this.cmdViewTraffic.UseVisualStyleBackColor = true;
         this.cmdViewTraffic.Click += new System.EventHandler(this.cmdViewTraffic_Click);
         // 
         // cmdStartBrowser
         // 
         this.cmdStartBrowser.Location = new System.Drawing.Point(212, 123);
         this.cmdStartBrowser.Name = "cmdStartBrowser";
         this.cmdStartBrowser.Size = new System.Drawing.Size(173, 34);
         this.cmdStartBrowser.TabIndex = 2;
         this.cmdStartBrowser.Text = "Start Browser";
         this.cmdStartBrowser.UseVisualStyleBackColor = true;
         this.cmdStartBrowser.Click += new System.EventHandler(this.cmdStartBrowser_Click);
         // 
         // cmdExit
         // 
         this.cmdExit.Location = new System.Drawing.Point(212, 174);
         this.cmdExit.Name = "cmdExit";
         this.cmdExit.Size = new System.Drawing.Size(173, 34);
         this.cmdExit.TabIndex = 3;
         this.cmdExit.Text = "Exit";
         this.cmdExit.UseVisualStyleBackColor = true;
         this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
         // 
         // lblIPAddress
         // 
         this.lblIPAddress.Location = new System.Drawing.Point(24, 8);
         this.lblIPAddress.Name = "lblIPAddress";
         this.lblIPAddress.Size = new System.Drawing.Size(170, 22);
         this.lblIPAddress.TabIndex = 4;
         this.lblIPAddress.Text = "IP Address";
         this.lblIPAddress.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
         // 
         // lblPort
         // 
         this.lblPort.Location = new System.Drawing.Point(209, 8);
         this.lblPort.Name = "lblPort";
         this.lblPort.Size = new System.Drawing.Size(173, 22);
         this.lblPort.TabIndex = 5;
         this.lblPort.Text = "Port";
         this.lblPort.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
         // 
         // txtIPAddress
         // 
         this.txtIPAddress.Location = new System.Drawing.Point(24, 33);
         this.txtIPAddress.Name = "txtIPAddress";
         this.txtIPAddress.Size = new System.Drawing.Size(170, 22);
         this.txtIPAddress.TabIndex = 6;
         this.txtIPAddress.Text = "10.0.0.100";
         this.txtIPAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // txtPort
         // 
         this.txtPort.Location = new System.Drawing.Point(209, 33);
         this.txtPort.Name = "txtPort";
         this.txtPort.ReadOnly = true;
         this.txtPort.Size = new System.Drawing.Size(173, 22);
         this.txtPort.TabIndex = 7;
         this.txtPort.Text = "44818";
         this.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblMessageFolder
         // 
         this.lblMessageFolder.Location = new System.Drawing.Point(24, 58);
         this.lblMessageFolder.Name = "lblMessageFolder";
         this.lblMessageFolder.Size = new System.Drawing.Size(170, 22);
         this.lblMessageFolder.TabIndex = 8;
         this.lblMessageFolder.Text = "Message Folder";
         // 
         // txtMessageFolder
         // 
         this.txtMessageFolder.Location = new System.Drawing.Point(27, 83);
         this.txtMessageFolder.Name = "txtMessageFolder";
         this.txtMessageFolder.Size = new System.Drawing.Size(269, 22);
         this.txtMessageFolder.TabIndex = 9;
         this.txtMessageFolder.Text = "C:\\Temp\\EIP";
         // 
         // cmdBrowse
         // 
         this.cmdBrowse.Location = new System.Drawing.Point(302, 75);
         this.cmdBrowse.Name = "cmdBrowse";
         this.cmdBrowse.Size = new System.Drawing.Size(81, 31);
         this.cmdBrowse.TabIndex = 10;
         this.cmdBrowse.Text = " Browse";
         this.cmdBrowse.UseVisualStyleBackColor = true;
         this.cmdBrowse.Click += new System.EventHandler(this.cmdBrowse_Click);
         // 
         // TestEIP
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(394, 230);
         this.Controls.Add(this.cmdBrowse);
         this.Controls.Add(this.txtMessageFolder);
         this.Controls.Add(this.lblMessageFolder);
         this.Controls.Add(this.txtPort);
         this.Controls.Add(this.txtIPAddress);
         this.Controls.Add(this.lblPort);
         this.Controls.Add(this.lblIPAddress);
         this.Controls.Add(this.cmdExit);
         this.Controls.Add(this.cmdStartBrowser);
         this.Controls.Add(this.cmdViewTraffic);
         this.Controls.Add(this.cmdSendToPrinter);
         this.Name = "TestEIP";
         this.Text = "Test Driver";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestEIP_FormClosing);
         this.Load += new System.EventHandler(this.TestEIP_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button cmdSendToPrinter;
      private System.Windows.Forms.Button cmdViewTraffic;
      private System.Windows.Forms.Button cmdStartBrowser;
      private System.Windows.Forms.Button cmdExit;
      private System.Windows.Forms.Label lblIPAddress;
      private System.Windows.Forms.Label lblPort;
      public System.Windows.Forms.TextBox txtIPAddress;
      public System.Windows.Forms.TextBox txtPort;
      private System.Windows.Forms.Label lblMessageFolder;
      public System.Windows.Forms.TextBox txtMessageFolder;
      private System.Windows.Forms.Button cmdBrowse;
   }
}

