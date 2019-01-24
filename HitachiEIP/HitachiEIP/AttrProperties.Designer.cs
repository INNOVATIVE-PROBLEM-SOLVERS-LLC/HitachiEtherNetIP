namespace HitachiEIP {
   partial class AttrProperties {
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
         this.lblClassCode = new System.Windows.Forms.Label();
         this.cbClassCode = new System.Windows.Forms.ComboBox();
         this.cbAttribute = new System.Windows.Forms.ComboBox();
         this.lblAttribute = new System.Windows.Forms.Label();
         this.chkHasGet = new System.Windows.Forms.CheckBox();
         this.chkHasService = new System.Windows.Forms.CheckBox();
         this.chkHasSet = new System.Windows.Forms.CheckBox();
         this.lblLength = new System.Windows.Forms.Label();
         this.txtLength = new System.Windows.Forms.TextBox();
         this.txtMax = new System.Windows.Forms.TextBox();
         this.lblMax = new System.Windows.Forms.Label();
         this.txtMin = new System.Windows.Forms.TextBox();
         this.lblMin = new System.Windows.Forms.Label();
         this.cbFormat = new System.Windows.Forms.ComboBox();
         this.lblFormat = new System.Windows.Forms.Label();
         this.chkLockUp = new System.Windows.Forms.CheckBox();
         this.btnIssueService = new System.Windows.Forms.Button();
         this.btnIssueSet = new System.Windows.Forms.Button();
         this.btnIssueGet = new System.Windows.Forms.Button();
         this.txtStatus = new System.Windows.Forms.TextBox();
         this.lblStatus = new System.Windows.Forms.Label();
         this.txtData = new System.Windows.Forms.TextBox();
         this.lblData = new System.Windows.Forms.Label();
         this.txtRawData = new System.Windows.Forms.TextBox();
         this.lblRawData = new System.Windows.Forms.Label();
         this.btnExit = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // lblClassCode
         // 
         this.lblClassCode.Location = new System.Drawing.Point(29, 24);
         this.lblClassCode.Name = "lblClassCode";
         this.lblClassCode.Size = new System.Drawing.Size(141, 23);
         this.lblClassCode.TabIndex = 0;
         this.lblClassCode.Text = "Class Code";
         this.lblClassCode.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // cbClassCode
         // 
         this.cbClassCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbClassCode.FormattingEnabled = true;
         this.cbClassCode.Location = new System.Drawing.Point(196, 21);
         this.cbClassCode.Name = "cbClassCode";
         this.cbClassCode.Size = new System.Drawing.Size(274, 24);
         this.cbClassCode.TabIndex = 1;
         this.cbClassCode.SelectedIndexChanged += new System.EventHandler(this.cbClassCode_SelectedIndexChanged);
         // 
         // cbAttribute
         // 
         this.cbAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbAttribute.FormattingEnabled = true;
         this.cbAttribute.Location = new System.Drawing.Point(196, 66);
         this.cbAttribute.Name = "cbAttribute";
         this.cbAttribute.Size = new System.Drawing.Size(274, 24);
         this.cbAttribute.TabIndex = 3;
         this.cbAttribute.SelectedIndexChanged += new System.EventHandler(this.cbAttribute_SelectedIndexChanged);
         // 
         // lblAttribute
         // 
         this.lblAttribute.Location = new System.Drawing.Point(29, 69);
         this.lblAttribute.Name = "lblAttribute";
         this.lblAttribute.Size = new System.Drawing.Size(141, 23);
         this.lblAttribute.TabIndex = 2;
         this.lblAttribute.Text = "Attribute";
         this.lblAttribute.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // chkHasGet
         // 
         this.chkHasGet.AutoSize = true;
         this.chkHasGet.Location = new System.Drawing.Point(71, 115);
         this.chkHasGet.Name = "chkHasGet";
         this.chkHasGet.Size = new System.Drawing.Size(82, 21);
         this.chkHasGet.TabIndex = 4;
         this.chkHasGet.Text = "Has Get";
         this.chkHasGet.UseVisualStyleBackColor = true;
         this.chkHasGet.CheckedChanged += new System.EventHandler(this.chkHasGet_CheckedChanged);
         // 
         // chkHasService
         // 
         this.chkHasService.AutoSize = true;
         this.chkHasService.Location = new System.Drawing.Point(71, 193);
         this.chkHasService.Name = "chkHasService";
         this.chkHasService.Size = new System.Drawing.Size(106, 21);
         this.chkHasService.TabIndex = 5;
         this.chkHasService.Text = "Has Service";
         this.chkHasService.UseVisualStyleBackColor = true;
         this.chkHasService.CheckedChanged += new System.EventHandler(this.chkHasService_CheckedChanged);
         // 
         // chkHasSet
         // 
         this.chkHasSet.AutoSize = true;
         this.chkHasSet.Location = new System.Drawing.Point(71, 154);
         this.chkHasSet.Name = "chkHasSet";
         this.chkHasSet.Size = new System.Drawing.Size(80, 21);
         this.chkHasSet.TabIndex = 6;
         this.chkHasSet.Text = "Has Set";
         this.chkHasSet.UseVisualStyleBackColor = true;
         this.chkHasSet.CheckedChanged += new System.EventHandler(this.chkHasSet_CheckedChanged);
         // 
         // lblLength
         // 
         this.lblLength.Location = new System.Drawing.Point(228, 121);
         this.lblLength.Name = "lblLength";
         this.lblLength.Size = new System.Drawing.Size(115, 23);
         this.lblLength.TabIndex = 7;
         this.lblLength.Text = "Length";
         this.lblLength.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtLength
         // 
         this.txtLength.Location = new System.Drawing.Point(349, 118);
         this.txtLength.Name = "txtLength";
         this.txtLength.Size = new System.Drawing.Size(121, 22);
         this.txtLength.TabIndex = 8;
         this.txtLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // txtMax
         // 
         this.txtMax.Location = new System.Drawing.Point(349, 199);
         this.txtMax.Name = "txtMax";
         this.txtMax.Size = new System.Drawing.Size(121, 22);
         this.txtMax.TabIndex = 10;
         this.txtMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblMax
         // 
         this.lblMax.Location = new System.Drawing.Point(228, 197);
         this.lblMax.Name = "lblMax";
         this.lblMax.Size = new System.Drawing.Size(115, 23);
         this.lblMax.TabIndex = 9;
         this.lblMax.Text = "Max Value";
         this.lblMax.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtMin
         // 
         this.txtMin.Location = new System.Drawing.Point(349, 152);
         this.txtMin.Name = "txtMin";
         this.txtMin.Size = new System.Drawing.Size(121, 22);
         this.txtMin.TabIndex = 12;
         this.txtMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblMin
         // 
         this.lblMin.Location = new System.Drawing.Point(228, 155);
         this.lblMin.Name = "lblMin";
         this.lblMin.Size = new System.Drawing.Size(115, 23);
         this.lblMin.TabIndex = 11;
         this.lblMin.Text = "Min Value";
         this.lblMin.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // cbFormat
         // 
         this.cbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbFormat.FormattingEnabled = true;
         this.cbFormat.Location = new System.Drawing.Point(349, 233);
         this.cbFormat.Name = "cbFormat";
         this.cbFormat.Size = new System.Drawing.Size(121, 24);
         this.cbFormat.TabIndex = 14;
         // 
         // lblFormat
         // 
         this.lblFormat.Location = new System.Drawing.Point(259, 233);
         this.lblFormat.Name = "lblFormat";
         this.lblFormat.Size = new System.Drawing.Size(84, 23);
         this.lblFormat.TabIndex = 13;
         this.lblFormat.Text = "Format";
         this.lblFormat.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // chkLockUp
         // 
         this.chkLockUp.AutoSize = true;
         this.chkLockUp.Location = new System.Drawing.Point(71, 230);
         this.chkLockUp.Name = "chkLockUp";
         this.chkLockUp.Size = new System.Drawing.Size(173, 21);
         this.chkLockUp.TabIndex = 17;
         this.chkLockUp.Text = "Causes Printer Lockup";
         this.chkLockUp.UseVisualStyleBackColor = true;
         // 
         // btnIssueService
         // 
         this.btnIssueService.Location = new System.Drawing.Point(212, 425);
         this.btnIssueService.Name = "btnIssueService";
         this.btnIssueService.Size = new System.Drawing.Size(137, 22);
         this.btnIssueService.TabIndex = 42;
         this.btnIssueService.Text = "Service";
         this.btnIssueService.UseVisualStyleBackColor = true;
         this.btnIssueService.Visible = false;
         // 
         // btnIssueSet
         // 
         this.btnIssueSet.Location = new System.Drawing.Point(105, 425);
         this.btnIssueSet.Name = "btnIssueSet";
         this.btnIssueSet.Size = new System.Drawing.Size(76, 22);
         this.btnIssueSet.TabIndex = 41;
         this.btnIssueSet.Text = "Set";
         this.btnIssueSet.UseVisualStyleBackColor = true;
         this.btnIssueSet.Visible = false;
         this.btnIssueSet.Click += new System.EventHandler(this.btnIssueSet_Click);
         // 
         // btnIssueGet
         // 
         this.btnIssueGet.Location = new System.Drawing.Point(41, 425);
         this.btnIssueGet.Name = "btnIssueGet";
         this.btnIssueGet.Size = new System.Drawing.Size(58, 22);
         this.btnIssueGet.TabIndex = 40;
         this.btnIssueGet.Text = "Get";
         this.btnIssueGet.UseVisualStyleBackColor = true;
         this.btnIssueGet.Visible = false;
         this.btnIssueGet.Click += new System.EventHandler(this.btnIssueGet_Click);
         // 
         // txtStatus
         // 
         this.txtStatus.Location = new System.Drawing.Point(388, 324);
         this.txtStatus.Name = "txtStatus";
         this.txtStatus.Size = new System.Drawing.Size(82, 22);
         this.txtStatus.TabIndex = 44;
         this.txtStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblStatus
         // 
         this.lblStatus.Location = new System.Drawing.Point(309, 327);
         this.lblStatus.Name = "lblStatus";
         this.lblStatus.Size = new System.Drawing.Size(34, 23);
         this.lblStatus.TabIndex = 43;
         this.lblStatus.Text = "Status";
         this.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtData
         // 
         this.txtData.Location = new System.Drawing.Point(174, 324);
         this.txtData.Name = "txtData";
         this.txtData.Size = new System.Drawing.Size(121, 22);
         this.txtData.TabIndex = 46;
         this.txtData.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblData
         // 
         this.lblData.Location = new System.Drawing.Point(93, 323);
         this.lblData.Name = "lblData";
         this.lblData.Size = new System.Drawing.Size(66, 23);
         this.lblData.TabIndex = 45;
         this.lblData.Text = "Data";
         this.lblData.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtRawData
         // 
         this.txtRawData.Location = new System.Drawing.Point(174, 364);
         this.txtRawData.Name = "txtRawData";
         this.txtRawData.Size = new System.Drawing.Size(296, 22);
         this.txtRawData.TabIndex = 48;
         this.txtRawData.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblRawData
         // 
         this.lblRawData.Location = new System.Drawing.Point(53, 367);
         this.lblRawData.Name = "lblRawData";
         this.lblRawData.Size = new System.Drawing.Size(115, 23);
         this.lblRawData.TabIndex = 47;
         this.lblRawData.Text = "Raw Data";
         this.lblRawData.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // btnExit
         // 
         this.btnExit.Location = new System.Drawing.Point(370, 425);
         this.btnExit.Name = "btnExit";
         this.btnExit.Size = new System.Drawing.Size(76, 22);
         this.btnExit.TabIndex = 49;
         this.btnExit.Text = "Exit";
         this.btnExit.UseVisualStyleBackColor = true;
         this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
         // 
         // AttrProperties
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(566, 525);
         this.Controls.Add(this.btnExit);
         this.Controls.Add(this.txtRawData);
         this.Controls.Add(this.lblRawData);
         this.Controls.Add(this.txtData);
         this.Controls.Add(this.lblData);
         this.Controls.Add(this.txtStatus);
         this.Controls.Add(this.lblStatus);
         this.Controls.Add(this.btnIssueService);
         this.Controls.Add(this.btnIssueSet);
         this.Controls.Add(this.btnIssueGet);
         this.Controls.Add(this.chkLockUp);
         this.Controls.Add(this.cbFormat);
         this.Controls.Add(this.lblFormat);
         this.Controls.Add(this.txtMin);
         this.Controls.Add(this.lblMin);
         this.Controls.Add(this.txtMax);
         this.Controls.Add(this.lblMax);
         this.Controls.Add(this.txtLength);
         this.Controls.Add(this.lblLength);
         this.Controls.Add(this.chkHasSet);
         this.Controls.Add(this.chkHasService);
         this.Controls.Add(this.chkHasGet);
         this.Controls.Add(this.cbAttribute);
         this.Controls.Add(this.lblAttribute);
         this.Controls.Add(this.cbClassCode);
         this.Controls.Add(this.lblClassCode);
         this.Name = "AttrProperties";
         this.Text = "View properties of an attribute";
         this.Load += new System.EventHandler(this.Properties_Load);
         this.Resize += new System.EventHandler(this.AttrProperties_Resize);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lblClassCode;
      private System.Windows.Forms.ComboBox cbClassCode;
      private System.Windows.Forms.ComboBox cbAttribute;
      private System.Windows.Forms.Label lblAttribute;
      private System.Windows.Forms.CheckBox chkHasGet;
      private System.Windows.Forms.CheckBox chkHasService;
      private System.Windows.Forms.CheckBox chkHasSet;
      private System.Windows.Forms.Label lblLength;
      private System.Windows.Forms.TextBox txtLength;
      private System.Windows.Forms.TextBox txtMax;
      private System.Windows.Forms.Label lblMax;
      private System.Windows.Forms.TextBox txtMin;
      private System.Windows.Forms.Label lblMin;
      private System.Windows.Forms.ComboBox cbFormat;
      private System.Windows.Forms.Label lblFormat;
      private System.Windows.Forms.CheckBox chkLockUp;
      private System.Windows.Forms.Button btnIssueService;
      private System.Windows.Forms.Button btnIssueSet;
      private System.Windows.Forms.Button btnIssueGet;
      private System.Windows.Forms.TextBox txtStatus;
      private System.Windows.Forms.Label lblStatus;
      private System.Windows.Forms.TextBox txtData;
      private System.Windows.Forms.Label lblData;
      private System.Windows.Forms.TextBox txtRawData;
      private System.Windows.Forms.Label lblRawData;
      private System.Windows.Forms.Button btnExit;
   }
}