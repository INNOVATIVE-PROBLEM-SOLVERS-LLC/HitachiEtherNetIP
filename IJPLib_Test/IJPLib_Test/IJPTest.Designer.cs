namespace IJPLib_Test {
   partial class IJPTest {
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
         this.cmdComOn = new System.Windows.Forms.Button();
         this.cmdComOff = new System.Windows.Forms.Button();
         this.cmdConnect = new System.Windows.Forms.Button();
         this.ipAddressTextBox = new System.Windows.Forms.TextBox();
         this.tclIJPLib = new System.Windows.Forms.TabControl();
         this.tabTreeView = new System.Windows.Forms.TabPage();
         this.tabIndentedView = new System.Windows.Forms.TabPage();
         this.cmdDump = new System.Windows.Forms.Button();
         this.tvMessage = new System.Windows.Forms.TreeView();
         this.ivMessage = new System.Windows.Forms.TextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.tclIJPLib.SuspendLayout();
         this.tabTreeView.SuspendLayout();
         this.tabIndentedView.SuspendLayout();
         this.SuspendLayout();
         // 
         // cmdComOn
         // 
         this.cmdComOn.Location = new System.Drawing.Point(358, 22);
         this.cmdComOn.Margin = new System.Windows.Forms.Padding(4);
         this.cmdComOn.Name = "cmdComOn";
         this.cmdComOn.Size = new System.Drawing.Size(100, 31);
         this.cmdComOn.TabIndex = 11;
         this.cmdComOn.Text = "COM On";
         this.cmdComOn.UseVisualStyleBackColor = true;
         this.cmdComOn.Click += new System.EventHandler(this.cmdComOn_Click);
         // 
         // cmdComOff
         // 
         this.cmdComOff.Location = new System.Drawing.Point(494, 22);
         this.cmdComOff.Margin = new System.Windows.Forms.Padding(4);
         this.cmdComOff.Name = "cmdComOff";
         this.cmdComOff.Size = new System.Drawing.Size(100, 31);
         this.cmdComOff.TabIndex = 10;
         this.cmdComOff.Text = "COM Off";
         this.cmdComOff.UseVisualStyleBackColor = true;
         this.cmdComOff.Click += new System.EventHandler(this.cmdComOff_Click);
         // 
         // cmdConnect
         // 
         this.cmdConnect.Location = new System.Drawing.Point(222, 22);
         this.cmdConnect.Margin = new System.Windows.Forms.Padding(4);
         this.cmdConnect.Name = "cmdConnect";
         this.cmdConnect.Size = new System.Drawing.Size(100, 31);
         this.cmdConnect.TabIndex = 9;
         this.cmdConnect.Text = "Connect";
         this.cmdConnect.UseVisualStyleBackColor = true;
         this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
         // 
         // ipAddressTextBox
         // 
         this.ipAddressTextBox.Location = new System.Drawing.Point(13, 22);
         this.ipAddressTextBox.Margin = new System.Windows.Forms.Padding(4);
         this.ipAddressTextBox.Name = "ipAddressTextBox";
         this.ipAddressTextBox.Size = new System.Drawing.Size(200, 22);
         this.ipAddressTextBox.TabIndex = 8;
         this.ipAddressTextBox.Text = "10.0.0.100";
         // 
         // tclIJPLib
         // 
         this.tclIJPLib.Controls.Add(this.tabTreeView);
         this.tclIJPLib.Controls.Add(this.tabIndentedView);
         this.tclIJPLib.Location = new System.Drawing.Point(12, 57);
         this.tclIJPLib.Name = "tclIJPLib";
         this.tclIJPLib.SelectedIndex = 0;
         this.tclIJPLib.Size = new System.Drawing.Size(722, 602);
         this.tclIJPLib.TabIndex = 16;
         // 
         // tabTreeView
         // 
         this.tabTreeView.Controls.Add(this.label1);
         this.tabTreeView.Controls.Add(this.tvMessage);
         this.tabTreeView.Location = new System.Drawing.Point(4, 25);
         this.tabTreeView.Name = "tabTreeView";
         this.tabTreeView.Padding = new System.Windows.Forms.Padding(3);
         this.tabTreeView.Size = new System.Drawing.Size(714, 573);
         this.tabTreeView.TabIndex = 1;
         this.tabTreeView.Text = "Tree View";
         this.tabTreeView.UseVisualStyleBackColor = true;
         // 
         // tabIndentedView
         // 
         this.tabIndentedView.Controls.Add(this.ivMessage);
         this.tabIndentedView.Location = new System.Drawing.Point(4, 25);
         this.tabIndentedView.Name = "tabIndentedView";
         this.tabIndentedView.Size = new System.Drawing.Size(714, 573);
         this.tabIndentedView.TabIndex = 2;
         this.tabIndentedView.Text = "Indented View";
         this.tabIndentedView.UseVisualStyleBackColor = true;
         // 
         // cmdDump
         // 
         this.cmdDump.Location = new System.Drawing.Point(630, 22);
         this.cmdDump.Margin = new System.Windows.Forms.Padding(4);
         this.cmdDump.Name = "cmdDump";
         this.cmdDump.Size = new System.Drawing.Size(100, 31);
         this.cmdDump.TabIndex = 17;
         this.cmdDump.Text = "Get Views";
         this.cmdDump.UseVisualStyleBackColor = true;
         this.cmdDump.Click += new System.EventHandler(this.cmdDump_Click);
         // 
         // tvMessage
         // 
         this.tvMessage.Location = new System.Drawing.Point(29, 66);
         this.tvMessage.Name = "tvMessage";
         this.tvMessage.Size = new System.Drawing.Size(658, 489);
         this.tvMessage.TabIndex = 0;
         // 
         // ivMessage
         // 
         this.ivMessage.Location = new System.Drawing.Point(17, 19);
         this.ivMessage.Multiline = true;
         this.ivMessage.Name = "ivMessage";
         this.ivMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.ivMessage.Size = new System.Drawing.Size(670, 533);
         this.ivMessage.TabIndex = 0;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(26, 14);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(71, 17);
         this.label1.TabIndex = 1;
         this.label1.Text = "Tree View";
         // 
         // IJPTest
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(753, 675);
         this.Controls.Add(this.cmdDump);
         this.Controls.Add(this.tclIJPLib);
         this.Controls.Add(this.cmdComOn);
         this.Controls.Add(this.cmdComOff);
         this.Controls.Add(this.cmdConnect);
         this.Controls.Add(this.ipAddressTextBox);
         this.Name = "IJPTest";
         this.Text = "Test IJP Interface";
         this.Load += new System.EventHandler(this.IJPTest_Load);
         this.tclIJPLib.ResumeLayout(false);
         this.tabTreeView.ResumeLayout(false);
         this.tabTreeView.PerformLayout();
         this.tabIndentedView.ResumeLayout(false);
         this.tabIndentedView.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion
      private System.Windows.Forms.Button cmdComOn;
      private System.Windows.Forms.Button cmdComOff;
      private System.Windows.Forms.Button cmdConnect;
      private System.Windows.Forms.TextBox ipAddressTextBox;
      private System.Windows.Forms.TabControl tclIJPLib;
      private System.Windows.Forms.TabPage tabTreeView;
      private System.Windows.Forms.TabPage tabIndentedView;
      private System.Windows.Forms.Button cmdDump;
      private System.Windows.Forms.TreeView tvMessage;
      private System.Windows.Forms.TextBox ivMessage;
      private System.Windows.Forms.Label label1;
   }
}

