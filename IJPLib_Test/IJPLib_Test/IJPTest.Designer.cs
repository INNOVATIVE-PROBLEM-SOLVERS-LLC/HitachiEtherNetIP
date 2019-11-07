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
         this.tclIJPTests = new System.Windows.Forms.TabControl();
         this.tabMain = new System.Windows.Forms.TabPage();
         this.lblSelectTest = new System.Windows.Forms.Label();
         this.cbSelectTest = new System.Windows.Forms.ComboBox();
         this.cmdRunTest = new System.Windows.Forms.Button();
         this.tabDisplay = new System.Windows.Forms.TabPage();
         this.lstLogs = new System.Windows.Forms.ListBox();
         this.cmErrLog = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cmErrLogToNotepad = new System.Windows.Forms.ToolStripMenuItem();
         this.cmErrLogClearlog = new System.Windows.Forms.ToolStripMenuItem();
         this.tclIJPLib.SuspendLayout();
         this.tabIndentedView.SuspendLayout();
         this.tabTreeView.SuspendLayout();
         this.tabXMLIndented.SuspendLayout();
         this.tabXMLTree.SuspendLayout();
         this.tclIJPTests.SuspendLayout();
         this.tabDisplay.SuspendLayout();
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
         this.tclIJPLib.Location = new System.Drawing.Point(19, 16);
         this.tclIJPLib.Name = "tclIJPLib";
         this.tclIJPLib.SelectedIndex = 0;
         this.tclIJPLib.Size = new System.Drawing.Size(675, 488);
         this.tclIJPLib.TabIndex = 16;
         // 
         // tabIndentedView
         // 
         this.tabIndentedView.Controls.Add(this.txtIjpIndented);
         this.tabIndentedView.Location = new System.Drawing.Point(4, 25);
         this.tabIndentedView.Name = "tabIndentedView";
         this.tabIndentedView.Size = new System.Drawing.Size(667, 573);
         this.tabIndentedView.TabIndex = 2;
         this.tabIndentedView.Text = "IJPLib Indented View";
         this.tabIndentedView.UseVisualStyleBackColor = true;
         // 
         // txtIjpIndented
         // 
         this.txtIjpIndented.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.txtIjpIndented.Location = new System.Drawing.Point(17, 19);
         this.txtIjpIndented.Multiline = true;
         this.txtIjpIndented.Name = "txtIjpIndented";
         this.txtIjpIndented.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtIjpIndented.Size = new System.Drawing.Size(614, 417);
         this.txtIjpIndented.TabIndex = 0;
         // 
         // tabTreeView
         // 
         this.tabTreeView.Controls.Add(this.tvIJPLibTree);
         this.tabTreeView.Location = new System.Drawing.Point(4, 25);
         this.tabTreeView.Name = "tabTreeView";
         this.tabTreeView.Padding = new System.Windows.Forms.Padding(3);
         this.tabTreeView.Size = new System.Drawing.Size(667, 573);
         this.tabTreeView.TabIndex = 1;
         this.tabTreeView.Text = "IJPLib Tree View";
         this.tabTreeView.UseVisualStyleBackColor = true;
         // 
         // tvIJPLibTree
         // 
         this.tvIJPLibTree.Location = new System.Drawing.Point(29, 66);
         this.tvIJPLibTree.Name = "tvIJPLibTree";
         this.tvIJPLibTree.Size = new System.Drawing.Size(602, 390);
         this.tvIJPLibTree.TabIndex = 0;
         // 
         // tabXMLIndented
         // 
         this.tabXMLIndented.Controls.Add(this.txtXMLIndented);
         this.tabXMLIndented.Location = new System.Drawing.Point(4, 25);
         this.tabXMLIndented.Name = "tabXMLIndented";
         this.tabXMLIndented.Size = new System.Drawing.Size(667, 573);
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
         this.txtXMLIndented.Size = new System.Drawing.Size(609, 449);
         this.txtXMLIndented.TabIndex = 1;
         // 
         // tabXMLTree
         // 
         this.tabXMLTree.Controls.Add(this.tvXMLTree);
         this.tabXMLTree.Location = new System.Drawing.Point(4, 25);
         this.tabXMLTree.Name = "tabXMLTree";
         this.tabXMLTree.Size = new System.Drawing.Size(667, 459);
         this.tabXMLTree.TabIndex = 4;
         this.tabXMLTree.Text = "XML Tree";
         this.tabXMLTree.UseVisualStyleBackColor = true;
         // 
         // tvXMLTree
         // 
         this.tvXMLTree.Location = new System.Drawing.Point(28, 42);
         this.tvXMLTree.Name = "tvXMLTree";
         this.tvXMLTree.Size = new System.Drawing.Size(603, 405);
         this.tvXMLTree.TabIndex = 1;
         // 
         // cmdGetViews
         // 
         this.cmdGetViews.Location = new System.Drawing.Point(701, 94);
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
         this.cmdGetXML.Location = new System.Drawing.Point(701, 40);
         this.cmdGetXML.Margin = new System.Windows.Forms.Padding(4);
         this.cmdGetXML.Name = "cmdGetXML";
         this.cmdGetXML.Size = new System.Drawing.Size(100, 31);
         this.cmdGetXML.TabIndex = 18;
         this.cmdGetXML.Text = "Get XML";
         this.cmdGetXML.UseVisualStyleBackColor = true;
         this.cmdGetXML.Click += new System.EventHandler(this.cmdGetXML_Click);
         // 
         // tclIJPTests
         // 
         this.tclIJPTests.Controls.Add(this.tabDisplay);
         this.tclIJPTests.Controls.Add(this.tabMain);
         this.tclIJPTests.Location = new System.Drawing.Point(13, 60);
         this.tclIJPTests.Name = "tclIJPTests";
         this.tclIJPTests.SelectedIndex = 0;
         this.tclIJPTests.Size = new System.Drawing.Size(837, 553);
         this.tclIJPTests.TabIndex = 19;
         // 
         // tabMain
         // 
         this.tabMain.Location = new System.Drawing.Point(4, 25);
         this.tabMain.Name = "tabMain";
         this.tabMain.Padding = new System.Windows.Forms.Padding(3);
         this.tabMain.Size = new System.Drawing.Size(829, 476);
         this.tabMain.TabIndex = 1;
         this.tabMain.Text = "Main";
         this.tabMain.UseVisualStyleBackColor = true;
         // 
         // lblSelectTest
         // 
         this.lblSelectTest.Location = new System.Drawing.Point(403, 619);
         this.lblSelectTest.Name = "lblSelectTest";
         this.lblSelectTest.Size = new System.Drawing.Size(141, 28);
         this.lblSelectTest.TabIndex = 2;
         this.lblSelectTest.Text = "SelectTest";
         this.lblSelectTest.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
         // 
         // cbSelectTest
         // 
         this.cbSelectTest.FormattingEnabled = true;
         this.cbSelectTest.Location = new System.Drawing.Point(406, 650);
         this.cbSelectTest.Name = "cbSelectTest";
         this.cbSelectTest.Size = new System.Drawing.Size(132, 24);
         this.cbSelectTest.TabIndex = 1;
         this.cbSelectTest.SelectedIndexChanged += new System.EventHandler(this.cbSelectTest_SelectedIndexChanged);
         // 
         // cmdRunTest
         // 
         this.cmdRunTest.Location = new System.Drawing.Point(559, 619);
         this.cmdRunTest.Name = "cmdRunTest";
         this.cmdRunTest.Size = new System.Drawing.Size(171, 51);
         this.cmdRunTest.TabIndex = 0;
         this.cmdRunTest.Text = "Run Test";
         this.cmdRunTest.UseVisualStyleBackColor = true;
         this.cmdRunTest.Click += new System.EventHandler(this.cmdRunTest_Click);
         // 
         // tabDisplay
         // 
         this.tabDisplay.Controls.Add(this.tclIJPLib);
         this.tabDisplay.Controls.Add(this.cmdGetViews);
         this.tabDisplay.Controls.Add(this.cmdGetXML);
         this.tabDisplay.Location = new System.Drawing.Point(4, 25);
         this.tabDisplay.Name = "tabDisplay";
         this.tabDisplay.Padding = new System.Windows.Forms.Padding(3);
         this.tabDisplay.Size = new System.Drawing.Size(829, 524);
         this.tabDisplay.TabIndex = 0;
         this.tabDisplay.Text = "Display";
         this.tabDisplay.UseVisualStyleBackColor = true;
         // 
         // lstLogs
         // 
         this.lstLogs.ContextMenuStrip = this.cmErrLog;
         this.lstLogs.FormattingEnabled = true;
         this.lstLogs.ItemHeight = 16;
         this.lstLogs.Location = new System.Drawing.Point(17, 619);
         this.lstLogs.Name = "lstLogs";
         this.lstLogs.Size = new System.Drawing.Size(355, 36);
         this.lstLogs.TabIndex = 3;
         // 
         // cmErrLog
         // 
         this.cmErrLog.ImageScalingSize = new System.Drawing.Size(20, 20);
         this.cmErrLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmErrLogToNotepad,
            this.cmErrLogClearlog});
         this.cmErrLog.Name = "cmErrLog";
         this.cmErrLog.Size = new System.Drawing.Size(211, 80);
         // 
         // cmErrLogToNotepad
         // 
         this.cmErrLogToNotepad.Name = "cmErrLogToNotepad";
         this.cmErrLogToNotepad.Size = new System.Drawing.Size(210, 24);
         this.cmErrLogToNotepad.Text = "View In Notepad";
         this.cmErrLogToNotepad.Click += new System.EventHandler(this.cmErrLogToNotepad_Click);
         // 
         // cmErrLogClearlog
         // 
         this.cmErrLogClearlog.Name = "cmErrLogClearlog";
         this.cmErrLogClearlog.Size = new System.Drawing.Size(210, 24);
         this.cmErrLogClearlog.Text = "Clear Log";
         this.cmErrLogClearlog.Click += new System.EventHandler(this.cmErrLogClearlog_Click);
         // 
         // IJPTest
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(926, 683);
         this.Controls.Add(this.lblSelectTest);
         this.Controls.Add(this.lstLogs);
         this.Controls.Add(this.cbSelectTest);
         this.Controls.Add(this.cmdRunTest);
         this.Controls.Add(this.tclIJPTests);
         this.Controls.Add(this.cmdComOnOff);
         this.Controls.Add(this.cmdConnect);
         this.Controls.Add(this.ipAddressTextBox);
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
         this.tclIJPTests.ResumeLayout(false);
         this.tabDisplay.ResumeLayout(false);
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
      private System.Windows.Forms.TabControl tclIJPTests;
      private System.Windows.Forms.TabPage tabMain;
      private System.Windows.Forms.TabPage tabDisplay;
      private System.Windows.Forms.Label lblSelectTest;
      private System.Windows.Forms.ComboBox cbSelectTest;
      private System.Windows.Forms.Button cmdRunTest;
      private System.Windows.Forms.ListBox lstLogs;
      private System.Windows.Forms.ContextMenuStrip cmErrLog;
      private System.Windows.Forms.ToolStripMenuItem cmErrLogToNotepad;
      private System.Windows.Forms.ToolStripMenuItem cmErrLogClearlog;
   }
}

