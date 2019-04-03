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
         this.SuspendLayout();
         // 
         // cmdSendToPrinter
         // 
         this.cmdSendToPrinter.Location = new System.Drawing.Point(12, 12);
         this.cmdSendToPrinter.Name = "cmdSendToPrinter";
         this.cmdSendToPrinter.Size = new System.Drawing.Size(173, 45);
         this.cmdSendToPrinter.TabIndex = 0;
         this.cmdSendToPrinter.Text = "Run Test";
         this.cmdSendToPrinter.UseVisualStyleBackColor = true;
         this.cmdSendToPrinter.Click += new System.EventHandler(this.cmdTest_Click);
         // 
         // cmdViewTraffic
         // 
         this.cmdViewTraffic.Location = new System.Drawing.Point(12, 77);
         this.cmdViewTraffic.Name = "cmdViewTraffic";
         this.cmdViewTraffic.Size = new System.Drawing.Size(173, 45);
         this.cmdViewTraffic.TabIndex = 1;
         this.cmdViewTraffic.Text = "View Traffic";
         this.cmdViewTraffic.UseVisualStyleBackColor = true;
         this.cmdViewTraffic.Click += new System.EventHandler(this.cmdViewTraffic_Click);
         // 
         // cmdStartBrowser
         // 
         this.cmdStartBrowser.Location = new System.Drawing.Point(200, 12);
         this.cmdStartBrowser.Name = "cmdStartBrowser";
         this.cmdStartBrowser.Size = new System.Drawing.Size(173, 45);
         this.cmdStartBrowser.TabIndex = 2;
         this.cmdStartBrowser.Text = "Start Browser";
         this.cmdStartBrowser.UseVisualStyleBackColor = true;
         this.cmdStartBrowser.Click += new System.EventHandler(this.cmdStartBrowser_Click);
         // 
         // cmdExit
         // 
         this.cmdExit.Location = new System.Drawing.Point(200, 77);
         this.cmdExit.Name = "cmdExit";
         this.cmdExit.Size = new System.Drawing.Size(173, 45);
         this.cmdExit.TabIndex = 3;
         this.cmdExit.Text = "Exit";
         this.cmdExit.UseVisualStyleBackColor = true;
         this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
         // 
         // TestEIP
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(394, 147);
         this.Controls.Add(this.cmdExit);
         this.Controls.Add(this.cmdStartBrowser);
         this.Controls.Add(this.cmdViewTraffic);
         this.Controls.Add(this.cmdSendToPrinter);
         this.Name = "TestEIP";
         this.Text = "Test Driver";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestEIP_FormClosing);
         this.Load += new System.EventHandler(this.TestEIP_Load);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Button cmdSendToPrinter;
      private System.Windows.Forms.Button cmdViewTraffic;
      private System.Windows.Forms.Button cmdStartBrowser;
      private System.Windows.Forms.Button cmdExit;
   }
}

