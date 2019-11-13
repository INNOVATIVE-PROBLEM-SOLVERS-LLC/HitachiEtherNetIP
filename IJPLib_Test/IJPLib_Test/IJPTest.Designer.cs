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
         this.components = new System.ComponentModel.Container();
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IJPTest));
         this.cmdComOnOff = new System.Windows.Forms.Button();
         this.cmdConnect = new System.Windows.Forms.Button();
         this.ipAddressTextBox = new System.Windows.Forms.TextBox();
         this.tclIJPLib = new System.Windows.Forms.TabControl();
         this.tabIndentedView = new System.Windows.Forms.TabPage();
         this.txtIjpIndented = new System.Windows.Forms.TextBox();
         this.tabTreeView = new System.Windows.Forms.TabPage();
         this.tvIJPLibTree = new System.Windows.Forms.TreeView();
         this.tabXMLIndented = new System.Windows.Forms.TabPage();
         this.txtXMLIndented = new System.Windows.Forms.TextBox();
         this.tabXMLTree = new System.Windows.Forms.TabPage();
         this.tvXMLTree = new System.Windows.Forms.TreeView();
         this.cmdGetViews = new System.Windows.Forms.Button();
         this.cmdGetXML = new System.Windows.Forms.Button();
         this.cmdSaveAs = new System.Windows.Forms.Button();
         this.lblSelectHardCodedTest = new System.Windows.Forms.Label();
         this.cbSelectHardCodedTest = new System.Windows.Forms.ComboBox();
         this.cmdRunHardCodedTest = new System.Windows.Forms.Button();
         this.lstLogs = new System.Windows.Forms.ListBox();
         this.cmErrLog = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cmErrLogToNotepad = new System.Windows.Forms.ToolStripMenuItem();
         this.cmErrLogClearlog = new System.Windows.Forms.ToolStripMenuItem();
         this.lblSelectXMLTest = new System.Windows.Forms.Label();
         this.cbSelectXMLTest = new System.Windows.Forms.ComboBox();
         this.cmdRunXMLTest = new System.Windows.Forms.Button();
         this.lblMessageFolder = new System.Windows.Forms.Label();
         this.txtMessageFolder = new System.Windows.Forms.TextBox();
         this.cmdBrowse = new System.Windows.Forms.Button();
         this.cmdClear = new System.Windows.Forms.Button();
         this.cmdSend = new System.Windows.Forms.Button();
         this.tclIJPLib.SuspendLayout();
         this.tabIndentedView.SuspendLayout();
         this.tabTreeView.SuspendLayout();
         this.tabXMLIndented.SuspendLayout();
         this.tabXMLTree.SuspendLayout();
         this.cmErrLog.SuspendLayout();
         this.SuspendLayout();
         // 
         // cmdComOnOff
         // 
         this.cmdComOnOff.Location = new System.Drawing.Point(358, 22);
         this.cmdComOnOff.Margin = new System.Windows.Forms.Padding(4);
         this.cmdComOnOff.Name = "cmdComOnOff";
         this.cmdComOnOff.Size = new System.Drawing.Size(100, 31);
         this.cmdComOnOff.TabIndex = 11;
         this.cmdComOnOff.Text = "COM On";
         this.cmdComOnOff.UseVisualStyleBackColor = true;
         this.cmdComOnOff.Click += new System.EventHandler(this.cmdComOnOff_Click);
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
         this.tclIJPLib.Controls.Add(this.tabIndentedView);
         this.tclIJPLib.Controls.Add(this.tabTreeView);
         this.tclIJPLib.Controls.Add(this.tabXMLIndented);
         this.tclIJPLib.Controls.Add(this.tabXMLTree);
         this.tclIJPLib.Location = new System.Drawing.Point(18, 76);
         this.tclIJPLib.Name = "tclIJPLib";
         this.tclIJPLib.SelectedIndex = 0;
         this.tclIJPLib.Size = new System.Drawing.Size(675, 195);
         this.tclIJPLib.TabIndex = 16;
         this.tclIJPLib.SelectedIndexChanged += new System.EventHandler(this.tclIJPLib_SelectedIndexChanged);
         // 
         // tabIndentedView
         // 
         this.tabIndentedView.Controls.Add(this.txtIjpIndented);
         this.tabIndentedView.Location = new System.Drawing.Point(4, 25);
         this.tabIndentedView.Name = "tabIndentedView";
         this.tabIndentedView.Size = new System.Drawing.Size(667, 166);
         this.tabIndentedView.TabIndex = 2;
         this.tabIndentedView.Text = "IJPLib Indented View";
         this.tabIndentedView.UseVisualStyleBackColor = true;
         // 
         // txtIjpIndented
         // 
         this.txtIjpIndented.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.txtIjpIndented.Location = new System.Drawing.Point(12, 25);
         this.txtIjpIndented.Multiline = true;
         this.txtIjpIndented.Name = "txtIjpIndented";
         this.txtIjpIndented.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtIjpIndented.Size = new System.Drawing.Size(614, 118);
         this.txtIjpIndented.TabIndex = 0;
         // 
         // tabTreeView
         // 
         this.tabTreeView.Controls.Add(this.tvIJPLibTree);
         this.tabTreeView.Location = new System.Drawing.Point(4, 25);
         this.tabTreeView.Name = "tabTreeView";
         this.tabTreeView.Padding = new System.Windows.Forms.Padding(3);
         this.tabTreeView.Size = new System.Drawing.Size(667, 166);
         this.tabTreeView.TabIndex = 1;
         this.tabTreeView.Text = "IJPLib Tree View";
         this.tabTreeView.UseVisualStyleBackColor = true;
         // 
         // tvIJPLibTree
         // 
         this.tvIJPLibTree.Location = new System.Drawing.Point(36, 21);
         this.tvIJPLibTree.Name = "tvIJPLibTree";
         this.tvIJPLibTree.Size = new System.Drawing.Size(602, 116);
         this.tvIJPLibTree.TabIndex = 0;
         // 
         // tabXMLIndented
         // 
         this.tabXMLIndented.Controls.Add(this.txtXMLIndented);
         this.tabXMLIndented.Location = new System.Drawing.Point(4, 25);
         this.tabXMLIndented.Name = "tabXMLIndented";
         this.tabXMLIndented.Size = new System.Drawing.Size(667, 166);
         this.tabXMLIndented.TabIndex = 3;
         this.tabXMLIndented.Text = "XML Indented";
         this.tabXMLIndented.UseVisualStyleBackColor = true;
         // 
         // txtXMLIndented
         // 
         this.txtXMLIndented.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.txtXMLIndented.Location = new System.Drawing.Point(22, 20);
         this.txtXMLIndented.Multiline = true;
         this.txtXMLIndented.Name = "txtXMLIndented";
         this.txtXMLIndented.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtXMLIndented.Size = new System.Drawing.Size(609, 117);
         this.txtXMLIndented.TabIndex = 1;
         // 
         // tabXMLTree
         // 
         this.tabXMLTree.Controls.Add(this.tvXMLTree);
         this.tabXMLTree.Location = new System.Drawing.Point(4, 25);
         this.tabXMLTree.Name = "tabXMLTree";
         this.tabXMLTree.Size = new System.Drawing.Size(667, 166);
         this.tabXMLTree.TabIndex = 4;
         this.tabXMLTree.Text = "XML Tree";
         this.tabXMLTree.UseVisualStyleBackColor = true;
         // 
         // tvXMLTree
         // 
         this.tvXMLTree.Location = new System.Drawing.Point(27, 18);
         this.tvXMLTree.Name = "tvXMLTree";
         this.tvXMLTree.Size = new System.Drawing.Size(603, 119);
         this.tvXMLTree.TabIndex = 1;
         // 
         // cmdGetViews
         // 
         this.cmdGetViews.Location = new System.Drawing.Point(721, 197);
         this.cmdGetViews.Margin = new System.Windows.Forms.Padding(4);
         this.cmdGetViews.Name = "cmdGetViews";
         this.cmdGetViews.Size = new System.Drawing.Size(100, 31);
         this.cmdGetViews.TabIndex = 17;
         this.cmdGetViews.Text = "Get Views";
         this.cmdGetViews.UseVisualStyleBackColor = true;
         this.cmdGetViews.Click += new System.EventHandler(this.cmdGetViews_Click);
         // 
         // cmdGetXML
         // 
         this.cmdGetXML.Location = new System.Drawing.Point(721, 158);
         this.cmdGetXML.Margin = new System.Windows.Forms.Padding(4);
         this.cmdGetXML.Name = "cmdGetXML";
         this.cmdGetXML.Size = new System.Drawing.Size(100, 31);
         this.cmdGetXML.TabIndex = 18;
         this.cmdGetXML.Text = "Get XML";
         this.cmdGetXML.UseVisualStyleBackColor = true;
         this.cmdGetXML.Click += new System.EventHandler(this.cmdGetXML_Click);
         // 
         // cmdSaveAs
         // 
         this.cmdSaveAs.Location = new System.Drawing.Point(721, 236);
         this.cmdSaveAs.Margin = new System.Windows.Forms.Padding(4);
         this.cmdSaveAs.Name = "cmdSaveAs";
         this.cmdSaveAs.Size = new System.Drawing.Size(100, 31);
         this.cmdSaveAs.TabIndex = 19;
         this.cmdSaveAs.Text = "Save As";
         this.cmdSaveAs.UseVisualStyleBackColor = true;
         this.cmdSaveAs.Click += new System.EventHandler(this.ccmdSaveAs_Click);
         // 
         // lblSelectHardCodedTest
         // 
         this.lblSelectHardCodedTest.Location = new System.Drawing.Point(326, 292);
         this.lblSelectHardCodedTest.Name = "lblSelectHardCodedTest";
         this.lblSelectHardCodedTest.Size = new System.Drawing.Size(167, 28);
         this.lblSelectHardCodedTest.TabIndex = 2;
         this.lblSelectHardCodedTest.Text = "Select Hard Coded Test";
         this.lblSelectHardCodedTest.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
         // 
         // cbSelectHardCodedTest
         // 
         this.cbSelectHardCodedTest.FormattingEnabled = true;
         this.cbSelectHardCodedTest.Location = new System.Drawing.Point(321, 323);
         this.cbSelectHardCodedTest.Name = "cbSelectHardCodedTest";
         this.cbSelectHardCodedTest.Size = new System.Drawing.Size(172, 24);
         this.cbSelectHardCodedTest.TabIndex = 1;
         this.cbSelectHardCodedTest.SelectedIndexChanged += new System.EventHandler(this.cbSelectTest_SelectedIndexChanged);
         // 
         // cmdRunHardCodedTest
         // 
         this.cmdRunHardCodedTest.Location = new System.Drawing.Point(321, 353);
         this.cmdRunHardCodedTest.Name = "cmdRunHardCodedTest";
         this.cmdRunHardCodedTest.Size = new System.Drawing.Size(171, 51);
         this.cmdRunHardCodedTest.TabIndex = 0;
         this.cmdRunHardCodedTest.Text = "Run Hard Coded Test";
         this.cmdRunHardCodedTest.UseVisualStyleBackColor = true;
         this.cmdRunHardCodedTest.Click += new System.EventHandler(this.cmdRunTest_Click);
         // 
         // lstLogs
         // 
         this.lstLogs.ContextMenuStrip = this.cmErrLog;
         this.lstLogs.FormattingEnabled = true;
         this.lstLogs.ItemHeight = 16;
         this.lstLogs.Location = new System.Drawing.Point(12, 292);
         this.lstLogs.Name = "lstLogs";
         this.lstLogs.Size = new System.Drawing.Size(291, 116);
         this.lstLogs.TabIndex = 3;
         // 
         // cmErrLog
         // 
         this.cmErrLog.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.cmErrLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmErrLogToNotepad,
            this.cmErrLogClearlog});
         this.cmErrLog.Name = "cmErrLog";
         this.cmErrLog.Size = new System.Drawing.Size(190, 52);
         // 
         // cmErrLogToNotepad
         // 
         this.cmErrLogToNotepad.Name = "cmErrLogToNotepad";
         this.cmErrLogToNotepad.Size = new System.Drawing.Size(189, 24);
         this.cmErrLogToNotepad.Text = "View In Notepad";
         this.cmErrLogToNotepad.Click += new System.EventHandler(this.cmErrLogToNotepad_Click);
         // 
         // cmErrLogClearlog
         // 
         this.cmErrLogClearlog.Name = "cmErrLogClearlog";
         this.cmErrLogClearlog.Size = new System.Drawing.Size(189, 24);
         this.cmErrLogClearlog.Text = "Clear Log";
         this.cmErrLogClearlog.Click += new System.EventHandler(this.cmErrLogClearlog_Click);
         // 
         // lblSelectXMLTest
         // 
         this.lblSelectXMLTest.Location = new System.Drawing.Point(526, 292);
         this.lblSelectXMLTest.Name = "lblSelectXMLTest";
         this.lblSelectXMLTest.Size = new System.Drawing.Size(167, 28);
         this.lblSelectXMLTest.TabIndex = 22;
         this.lblSelectXMLTest.Text = "Select XML Test";
         this.lblSelectXMLTest.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
         // 
         // cbSelectXMLTest
         // 
         this.cbSelectXMLTest.FormattingEnabled = true;
         this.cbSelectXMLTest.Location = new System.Drawing.Point(521, 323);
         this.cbSelectXMLTest.Name = "cbSelectXMLTest";
         this.cbSelectXMLTest.Size = new System.Drawing.Size(172, 24);
         this.cbSelectXMLTest.TabIndex = 21;
         this.cbSelectXMLTest.SelectedIndexChanged += new System.EventHandler(this.cbSelectXMLTest_SelectedIndexChanged);
         // 
         // cmdRunXMLTest
         // 
         this.cmdRunXMLTest.Location = new System.Drawing.Point(521, 353);
         this.cmdRunXMLTest.Name = "cmdRunXMLTest";
         this.cmdRunXMLTest.Size = new System.Drawing.Size(171, 51);
         this.cmdRunXMLTest.TabIndex = 20;
         this.cmdRunXMLTest.Text = "Run XML Test";
         this.cmdRunXMLTest.UseVisualStyleBackColor = true;
         this.cmdRunXMLTest.Click += new System.EventHandler(this.cmdRunXMLTest_Click);
         // 
         // lblMessageFolder
         // 
         this.lblMessageFolder.AutoSize = true;
         this.lblMessageFolder.Location = new System.Drawing.Point(482, 26);
         this.lblMessageFolder.Name = "lblMessageFolder";
         this.lblMessageFolder.Size = new System.Drawing.Size(109, 17);
         this.lblMessageFolder.TabIndex = 23;
         this.lblMessageFolder.Text = "Message Folder";
         this.lblMessageFolder.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
         // 
         // txtMessageFolder
         // 
         this.txtMessageFolder.Location = new System.Drawing.Point(625, 25);
         this.txtMessageFolder.Name = "txtMessageFolder";
         this.txtMessageFolder.Size = new System.Drawing.Size(106, 22);
         this.txtMessageFolder.TabIndex = 24;
         // 
         // cmdBrowse
         // 
         this.cmdBrowse.Location = new System.Drawing.Point(761, 26);
         this.cmdBrowse.Name = "cmdBrowse";
         this.cmdBrowse.Size = new System.Drawing.Size(84, 34);
         this.cmdBrowse.TabIndex = 25;
         this.cmdBrowse.Text = "Browse";
         this.cmdBrowse.UseVisualStyleBackColor = true;
         this.cmdBrowse.Click += new System.EventHandler(this.cmdBrowse_Click);
         // 
         // cmdClear
         // 
         this.cmdClear.Location = new System.Drawing.Point(721, 119);
         this.cmdClear.Margin = new System.Windows.Forms.Padding(4);
         this.cmdClear.Name = "cmdClear";
         this.cmdClear.Size = new System.Drawing.Size(100, 31);
         this.cmdClear.TabIndex = 26;
         this.cmdClear.Text = "Clear";
         this.cmdClear.UseVisualStyleBackColor = true;
         this.cmdClear.Click += new System.EventHandler(this.cmdClear_Click);
         // 
         // cmdSend
         // 
         this.cmdSend.Location = new System.Drawing.Point(721, 275);
         this.cmdSend.Margin = new System.Windows.Forms.Padding(4);
         this.cmdSend.Name = "cmdSend";
         this.cmdSend.Size = new System.Drawing.Size(100, 31);
         this.cmdSend.TabIndex = 27;
         this.cmdSend.Text = "Send";
         this.cmdSend.UseVisualStyleBackColor = true;
         this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);
         // 
         // IJPTest
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(926, 437);
         this.Controls.Add(this.cmdSend);
         this.Controls.Add(this.cmdClear);
         this.Controls.Add(this.cmdSaveAs);
         this.Controls.Add(this.tclIJPLib);
         this.Controls.Add(this.cmdGetViews);
         this.Controls.Add(this.cmdBrowse);
         this.Controls.Add(this.cmdGetXML);
         this.Controls.Add(this.txtMessageFolder);
         this.Controls.Add(this.lblMessageFolder);
         this.Controls.Add(this.lblSelectXMLTest);
         this.Controls.Add(this.cbSelectXMLTest);
         this.Controls.Add(this.cmdRunXMLTest);
         this.Controls.Add(this.lblSelectHardCodedTest);
         this.Controls.Add(this.lstLogs);
         this.Controls.Add(this.cbSelectHardCodedTest);
         this.Controls.Add(this.cmdRunHardCodedTest);
         this.Controls.Add(this.cmdComOnOff);
         this.Controls.Add(this.cmdConnect);
         this.Controls.Add(this.ipAddressTextBox);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Name = "IJPTest";
         this.Text = "Test IJP Interface";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IJPTest_FormClosing);
         this.Load += new System.EventHandler(this.IJPTest_Load);
         this.Resize += new System.EventHandler(this.IJPTest_Resize);
         this.tclIJPLib.ResumeLayout(false);
         this.tabIndentedView.ResumeLayout(false);
         this.tabIndentedView.PerformLayout();
         this.tabTreeView.ResumeLayout(false);
         this.tabXMLIndented.ResumeLayout(false);
         this.tabXMLIndented.PerformLayout();
         this.tabXMLTree.ResumeLayout(false);
         this.cmErrLog.ResumeLayout(false);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion
      private System.Windows.Forms.Button cmdComOnOff;
      private System.Windows.Forms.Button cmdConnect;
      private System.Windows.Forms.TextBox ipAddressTextBox;
      private System.Windows.Forms.TabControl tclIJPLib;
      private System.Windows.Forms.TabPage tabTreeView;
      private System.Windows.Forms.TabPage tabIndentedView;
      private System.Windows.Forms.Button cmdGetViews;
      private System.Windows.Forms.TreeView tvIJPLibTree;
      private System.Windows.Forms.TextBox txtIjpIndented;
      private System.Windows.Forms.TabPage tabXMLIndented;
      private System.Windows.Forms.TextBox txtXMLIndented;
      private System.Windows.Forms.Button cmdGetXML;
      private System.Windows.Forms.TabPage tabXMLTree;
      private System.Windows.Forms.TreeView tvXMLTree;
      private System.Windows.Forms.Label lblSelectHardCodedTest;
      private System.Windows.Forms.ComboBox cbSelectHardCodedTest;
      private System.Windows.Forms.Button cmdRunHardCodedTest;
      private System.Windows.Forms.ListBox lstLogs;
      private System.Windows.Forms.ContextMenuStrip cmErrLog;
      private System.Windows.Forms.ToolStripMenuItem cmErrLogToNotepad;
      private System.Windows.Forms.ToolStripMenuItem cmErrLogClearlog;
      private System.Windows.Forms.Button cmdSaveAs;
      private System.Windows.Forms.Label lblSelectXMLTest;
      private System.Windows.Forms.ComboBox cbSelectXMLTest;
      private System.Windows.Forms.Button cmdRunXMLTest;
      private System.Windows.Forms.Label lblMessageFolder;
      private System.Windows.Forms.TextBox txtMessageFolder;
      private System.Windows.Forms.Button cmdBrowse;
      private System.Windows.Forms.Button cmdClear;
      private System.Windows.Forms.Button cmdSend;
   }
}

