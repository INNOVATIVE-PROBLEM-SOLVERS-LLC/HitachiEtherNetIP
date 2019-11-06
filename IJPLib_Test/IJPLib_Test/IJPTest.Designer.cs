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
         this.cmdComOnOff = new System.Windows.Forms.Button();
         this.cmdConnect = new System.Windows.Forms.Button();
         this.ipAddressTextBox = new System.Windows.Forms.TextBox();
         this.tclIJPLib = new System.Windows.Forms.TabControl();
         this.tabTreeView = new System.Windows.Forms.TabPage();
         this.label1 = new System.Windows.Forms.Label();
         this.tvMessage = new System.Windows.Forms.TreeView();
         this.tabIndentedView = new System.Windows.Forms.TabPage();
         this.ivMessage = new System.Windows.Forms.TextBox();
         this.tabXMLIndented = new System.Windows.Forms.TabPage();
         this.txtXML = new System.Windows.Forms.TextBox();
         this.cmdGetViews = new System.Windows.Forms.Button();
         this.cmdGetXML = new System.Windows.Forms.Button();
         this.tabXMLTree = new System.Windows.Forms.TabPage();
         this.tvXML = new System.Windows.Forms.TreeView();
         this.tclIJPLib.SuspendLayout();
         this.tabTreeView.SuspendLayout();
         this.tabIndentedView.SuspendLayout();
         this.tabXMLIndented.SuspendLayout();
         this.tabXMLTree.SuspendLayout();
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
         this.cmdComOnOff.Click += new System.EventHandler(this.cmdComOn_Click);
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
         this.tabTreeView.Text = "IJPLib Tree View";
         this.tabTreeView.UseVisualStyleBackColor = true;
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
         // tvMessage
         // 
         this.tvMessage.Location = new System.Drawing.Point(29, 66);
         this.tvMessage.Name = "tvMessage";
         this.tvMessage.Size = new System.Drawing.Size(658, 489);
         this.tvMessage.TabIndex = 0;
         // 
         // tabIndentedView
         // 
         this.tabIndentedView.Controls.Add(this.ivMessage);
         this.tabIndentedView.Location = new System.Drawing.Point(4, 25);
         this.tabIndentedView.Name = "tabIndentedView";
         this.tabIndentedView.Size = new System.Drawing.Size(714, 573);
         this.tabIndentedView.TabIndex = 2;
         this.tabIndentedView.Text = "IJPLib Indented View";
         this.tabIndentedView.UseVisualStyleBackColor = true;
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
         // tabXMLIndented
         // 
         this.tabXMLIndented.Controls.Add(this.txtXML);
         this.tabXMLIndented.Location = new System.Drawing.Point(4, 25);
         this.tabXMLIndented.Name = "tabXMLIndented";
         this.tabXMLIndented.Size = new System.Drawing.Size(714, 573);
         this.tabXMLIndented.TabIndex = 3;
         this.tabXMLIndented.Text = "XML Indented";
         this.tabXMLIndented.UseVisualStyleBackColor = true;
         // 
         // txtXML
         // 
         this.txtXML.Location = new System.Drawing.Point(22, 20);
         this.txtXML.Multiline = true;
         this.txtXML.Name = "txtXML";
         this.txtXML.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.txtXML.Size = new System.Drawing.Size(670, 533);
         this.txtXML.TabIndex = 1;
         // 
         // cmdGetViews
         // 
         this.cmdGetViews.Location = new System.Drawing.Point(476, 22);
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
         this.cmdGetXML.Location = new System.Drawing.Point(599, 22);
         this.cmdGetXML.Margin = new System.Windows.Forms.Padding(4);
         this.cmdGetXML.Name = "cmdGetXML";
         this.cmdGetXML.Size = new System.Drawing.Size(100, 31);
         this.cmdGetXML.TabIndex = 18;
         this.cmdGetXML.Text = "Get XML";
         this.cmdGetXML.UseVisualStyleBackColor = true;
         this.cmdGetXML.Click += new System.EventHandler(this.cmdGetXML_Click);
         // 
         // tabXMLTree
         // 
         this.tabXMLTree.Controls.Add(this.tvXML);
         this.tabXMLTree.Location = new System.Drawing.Point(4, 25);
         this.tabXMLTree.Name = "tabXMLTree";
         this.tabXMLTree.Size = new System.Drawing.Size(714, 573);
         this.tabXMLTree.TabIndex = 4;
         this.tabXMLTree.Text = "XML Tree";
         this.tabXMLTree.UseVisualStyleBackColor = true;
         // 
         // tvXML
         // 
         this.tvXML.Location = new System.Drawing.Point(28, 42);
         this.tvXML.Name = "tvXML";
         this.tvXML.Size = new System.Drawing.Size(658, 489);
         this.tvXML.TabIndex = 1;
         // 
         // IJPTest
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(753, 675);
         this.Controls.Add(this.cmdGetXML);
         this.Controls.Add(this.cmdGetViews);
         this.Controls.Add(this.tclIJPLib);
         this.Controls.Add(this.cmdComOnOff);
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
         this.tabXMLIndented.ResumeLayout(false);
         this.tabXMLIndented.PerformLayout();
         this.tabXMLTree.ResumeLayout(false);
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
      private System.Windows.Forms.TreeView tvMessage;
      private System.Windows.Forms.TextBox ivMessage;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TabPage tabXMLIndented;
      private System.Windows.Forms.TextBox txtXML;
      private System.Windows.Forms.Button cmdGetXML;
      private System.Windows.Forms.TabPage tabXMLTree;
      private System.Windows.Forms.TreeView tvXML;
   }
}

