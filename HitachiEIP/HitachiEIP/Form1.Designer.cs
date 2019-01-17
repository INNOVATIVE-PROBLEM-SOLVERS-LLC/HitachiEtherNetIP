﻿namespace HitachiEIP {
   partial class Form1 {
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
         this.btnConnect = new System.Windows.Forms.Button();
         this.btnDisconnect = new System.Windows.Forms.Button();
         this.lblIPAddress = new System.Windows.Forms.Label();
         this.lblPort = new System.Windows.Forms.Label();
         this.txtIPAddress = new System.Windows.Forms.TextBox();
         this.txtPort = new System.Windows.Forms.TextBox();
         this.btnEndSession = new System.Windows.Forms.Button();
         this.btnStartSession = new System.Windows.Forms.Button();
         this.txtSessionID = new System.Windows.Forms.TextBox();
         this.lblSessionID = new System.Windows.Forms.Label();
         this.btnExit = new System.Windows.Forms.Button();
         this.btnForwardClose = new System.Windows.Forms.Button();
         this.btnForwardOpen = new System.Windows.Forms.Button();
         this.btnIssueRequest = new System.Windows.Forms.Button();
         this.cbAccessCode = new System.Windows.Forms.ComboBox();
         this.lblAccessCode = new System.Windows.Forms.Label();
         this.lblClassCode = new System.Windows.Forms.Label();
         this.cbClassCode = new System.Windows.Forms.ComboBox();
         this.lblFunction = new System.Windows.Forms.Label();
         this.cbFunction = new System.Windows.Forms.ComboBox();
         this.lbldata = new System.Windows.Forms.Label();
         this.txtData = new System.Windows.Forms.TextBox();
         this.txtStatus = new System.Windows.Forms.TextBox();
         this.lblStatus = new System.Windows.Forms.Label();
         this.txtDataDec = new System.Windows.Forms.TextBox();
         this.lblSaveFolder = new System.Windows.Forms.Label();
         this.txtSaveFolder = new System.Windows.Forms.TextBox();
         this.btnBrowse = new System.Windows.Forms.Button();
         this.btnViewTraffic = new System.Windows.Forms.Button();
         this.btnViewLog = new System.Windows.Forms.Button();
         this.btnReadAll = new System.Windows.Forms.Button();
         this.tclClasses = new System.Windows.Forms.TabControl();
         this.tabIndex = new System.Windows.Forms.TabPage();
         this.tabIJPOperation = new System.Windows.Forms.TabPage();
         this.tabPrintManagement = new System.Windows.Forms.TabPage();
         this.tabPrintSpec = new System.Windows.Forms.TabPage();
         this.tabPrintFormat = new System.Windows.Forms.TabPage();
         this.tabCalendar = new System.Windows.Forms.TabPage();
         this.tabSubstitution = new System.Windows.Forms.TabPage();
         this.tabCount = new System.Windows.Forms.TabPage();
         this.tabUnitInformation = new System.Windows.Forms.TabPage();
         this.tabEnviroment = new System.Windows.Forms.TabPage();
         this.tabOpMgmt = new System.Windows.Forms.TabPage();
         this.tabUserPattern = new System.Windows.Forms.TabPage();
         this.tclClasses.SuspendLayout();
         this.SuspendLayout();
         // 
         // btnConnect
         // 
         this.btnConnect.Location = new System.Drawing.Point(26, 118);
         this.btnConnect.Name = "btnConnect";
         this.btnConnect.Size = new System.Drawing.Size(137, 31);
         this.btnConnect.TabIndex = 0;
         this.btnConnect.Text = "Connect";
         this.btnConnect.UseVisualStyleBackColor = true;
         this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
         // 
         // btnDisconnect
         // 
         this.btnDisconnect.Location = new System.Drawing.Point(179, 118);
         this.btnDisconnect.Name = "btnDisconnect";
         this.btnDisconnect.Size = new System.Drawing.Size(137, 31);
         this.btnDisconnect.TabIndex = 1;
         this.btnDisconnect.Text = "Disconnect";
         this.btnDisconnect.UseVisualStyleBackColor = true;
         this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
         // 
         // lblIPAddress
         // 
         this.lblIPAddress.Location = new System.Drawing.Point(23, 24);
         this.lblIPAddress.Name = "lblIPAddress";
         this.lblIPAddress.Size = new System.Drawing.Size(95, 29);
         this.lblIPAddress.TabIndex = 2;
         this.lblIPAddress.Text = "IP Address";
         this.lblIPAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lblPort
         // 
         this.lblPort.Location = new System.Drawing.Point(23, 56);
         this.lblPort.Name = "lblPort";
         this.lblPort.Size = new System.Drawing.Size(95, 29);
         this.lblPort.TabIndex = 3;
         this.lblPort.Text = "Port";
         this.lblPort.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // txtIPAddress
         // 
         this.txtIPAddress.Location = new System.Drawing.Point(141, 24);
         this.txtIPAddress.Name = "txtIPAddress";
         this.txtIPAddress.Size = new System.Drawing.Size(175, 22);
         this.txtIPAddress.TabIndex = 4;
         this.txtIPAddress.Text = "192.168.0.1";
         this.txtIPAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // txtPort
         // 
         this.txtPort.Location = new System.Drawing.Point(141, 56);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(175, 22);
         this.txtPort.TabIndex = 5;
         this.txtPort.Text = "44818";
         this.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // btnEndSession
         // 
         this.btnEndSession.Location = new System.Drawing.Point(179, 162);
         this.btnEndSession.Name = "btnEndSession";
         this.btnEndSession.Size = new System.Drawing.Size(137, 31);
         this.btnEndSession.TabIndex = 7;
         this.btnEndSession.Text = "End Session";
         this.btnEndSession.UseVisualStyleBackColor = true;
         this.btnEndSession.Click += new System.EventHandler(this.btnEndSession_Click);
         // 
         // btnStartSession
         // 
         this.btnStartSession.Location = new System.Drawing.Point(26, 162);
         this.btnStartSession.Name = "btnStartSession";
         this.btnStartSession.Size = new System.Drawing.Size(137, 31);
         this.btnStartSession.TabIndex = 6;
         this.btnStartSession.Text = "Start session";
         this.btnStartSession.UseVisualStyleBackColor = true;
         this.btnStartSession.Click += new System.EventHandler(this.btnStartSession_Click);
         // 
         // txtSessionID
         // 
         this.txtSessionID.Location = new System.Drawing.Point(141, 90);
         this.txtSessionID.Name = "txtSessionID";
         this.txtSessionID.Size = new System.Drawing.Size(175, 22);
         this.txtSessionID.TabIndex = 9;
         this.txtSessionID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         // 
         // lblSessionID
         // 
         this.lblSessionID.Location = new System.Drawing.Point(23, 90);
         this.lblSessionID.Name = "lblSessionID";
         this.lblSessionID.Size = new System.Drawing.Size(95, 29);
         this.lblSessionID.TabIndex = 8;
         this.lblSessionID.Text = "Session ID";
         this.lblSessionID.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // btnExit
         // 
         this.btnExit.Location = new System.Drawing.Point(1145, 655);
         this.btnExit.Name = "btnExit";
         this.btnExit.Size = new System.Drawing.Size(125, 52);
         this.btnExit.TabIndex = 10;
         this.btnExit.Text = "Exit";
         this.btnExit.UseVisualStyleBackColor = true;
         this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
         // 
         // btnForwardClose
         // 
         this.btnForwardClose.Location = new System.Drawing.Point(179, 203);
         this.btnForwardClose.Name = "btnForwardClose";
         this.btnForwardClose.Size = new System.Drawing.Size(137, 31);
         this.btnForwardClose.TabIndex = 12;
         this.btnForwardClose.Text = "Forward Close";
         this.btnForwardClose.UseVisualStyleBackColor = true;
         this.btnForwardClose.Click += new System.EventHandler(this.btnForwardClose_Click);
         // 
         // btnForwardOpen
         // 
         this.btnForwardOpen.Location = new System.Drawing.Point(26, 203);
         this.btnForwardOpen.Name = "btnForwardOpen";
         this.btnForwardOpen.Size = new System.Drawing.Size(137, 31);
         this.btnForwardOpen.TabIndex = 11;
         this.btnForwardOpen.Text = "Forward Open";
         this.btnForwardOpen.UseVisualStyleBackColor = true;
         this.btnForwardOpen.Click += new System.EventHandler(this.btnForwardOpen_Click);
         // 
         // btnIssueRequest
         // 
         this.btnIssueRequest.Location = new System.Drawing.Point(23, 421);
         this.btnIssueRequest.Name = "btnIssueRequest";
         this.btnIssueRequest.Size = new System.Drawing.Size(293, 22);
         this.btnIssueRequest.TabIndex = 14;
         this.btnIssueRequest.Text = "Issue Request";
         this.btnIssueRequest.UseVisualStyleBackColor = true;
         this.btnIssueRequest.Click += new System.EventHandler(this.btnIssueRequest_Click);
         // 
         // cbAccessCode
         // 
         this.cbAccessCode.FormattingEnabled = true;
         this.cbAccessCode.Location = new System.Drawing.Point(26, 267);
         this.cbAccessCode.Name = "cbAccessCode";
         this.cbAccessCode.Size = new System.Drawing.Size(293, 24);
         this.cbAccessCode.TabIndex = 16;
         this.cbAccessCode.SelectedIndexChanged += new System.EventHandler(this.cbAccessCode_SelectedIndexChanged);
         // 
         // lblAccessCode
         // 
         this.lblAccessCode.Location = new System.Drawing.Point(23, 241);
         this.lblAccessCode.Name = "lblAccessCode";
         this.lblAccessCode.Size = new System.Drawing.Size(293, 22);
         this.lblAccessCode.TabIndex = 17;
         this.lblAccessCode.Text = "Access Code";
         this.lblAccessCode.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
         // 
         // lblClassCode
         // 
         this.lblClassCode.Location = new System.Drawing.Point(20, 297);
         this.lblClassCode.Name = "lblClassCode";
         this.lblClassCode.Size = new System.Drawing.Size(293, 22);
         this.lblClassCode.TabIndex = 19;
         this.lblClassCode.Text = "Class Code";
         this.lblClassCode.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
         // 
         // cbClassCode
         // 
         this.cbClassCode.FormattingEnabled = true;
         this.cbClassCode.Location = new System.Drawing.Point(23, 323);
         this.cbClassCode.Name = "cbClassCode";
         this.cbClassCode.Size = new System.Drawing.Size(293, 24);
         this.cbClassCode.TabIndex = 18;
         this.cbClassCode.SelectedIndexChanged += new System.EventHandler(this.cbClassCode_SelectedIndexChanged);
         // 
         // lblFunction
         // 
         this.lblFunction.Location = new System.Drawing.Point(20, 353);
         this.lblFunction.Name = "lblFunction";
         this.lblFunction.Size = new System.Drawing.Size(293, 22);
         this.lblFunction.TabIndex = 21;
         this.lblFunction.Text = "Function Code";
         this.lblFunction.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
         // 
         // cbFunction
         // 
         this.cbFunction.FormattingEnabled = true;
         this.cbFunction.Location = new System.Drawing.Point(23, 381);
         this.cbFunction.Name = "cbFunction";
         this.cbFunction.Size = new System.Drawing.Size(293, 24);
         this.cbFunction.TabIndex = 20;
         this.cbFunction.SelectedIndexChanged += new System.EventHandler(this.cbFunction_SelectedIndexChanged);
         // 
         // lbldata
         // 
         this.lbldata.Location = new System.Drawing.Point(23, 502);
         this.lbldata.Name = "lbldata";
         this.lbldata.Size = new System.Drawing.Size(293, 22);
         this.lbldata.TabIndex = 22;
         this.lbldata.Text = "Data";
         this.lbldata.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // txtData
         // 
         this.txtData.Location = new System.Drawing.Point(23, 534);
         this.txtData.Name = "txtData";
         this.txtData.ReadOnly = true;
         this.txtData.Size = new System.Drawing.Size(293, 22);
         this.txtData.TabIndex = 23;
         // 
         // txtStatus
         // 
         this.txtStatus.Location = new System.Drawing.Point(23, 477);
         this.txtStatus.Name = "txtStatus";
         this.txtStatus.ReadOnly = true;
         this.txtStatus.Size = new System.Drawing.Size(293, 22);
         this.txtStatus.TabIndex = 25;
         // 
         // lblStatus
         // 
         this.lblStatus.Location = new System.Drawing.Point(23, 446);
         this.lblStatus.Name = "lblStatus";
         this.lblStatus.Size = new System.Drawing.Size(293, 22);
         this.lblStatus.TabIndex = 24;
         this.lblStatus.Text = "Status";
         this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // txtDataDec
         // 
         this.txtDataDec.Location = new System.Drawing.Point(23, 562);
         this.txtDataDec.Name = "txtDataDec";
         this.txtDataDec.ReadOnly = true;
         this.txtDataDec.Size = new System.Drawing.Size(293, 22);
         this.txtDataDec.TabIndex = 26;
         // 
         // lblSaveFolder
         // 
         this.lblSaveFolder.Location = new System.Drawing.Point(23, 597);
         this.lblSaveFolder.Name = "lblSaveFolder";
         this.lblSaveFolder.Size = new System.Drawing.Size(202, 19);
         this.lblSaveFolder.TabIndex = 27;
         this.lblSaveFolder.Text = "Traffic/Log Save Folder";
         this.lblSaveFolder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // txtSaveFolder
         // 
         this.txtSaveFolder.Location = new System.Drawing.Point(26, 628);
         this.txtSaveFolder.Name = "txtSaveFolder";
         this.txtSaveFolder.ReadOnly = true;
         this.txtSaveFolder.Size = new System.Drawing.Size(301, 22);
         this.txtSaveFolder.TabIndex = 28;
         this.txtSaveFolder.Text = "c:\\Temp";
         // 
         // btnBrowse
         // 
         this.btnBrowse.Location = new System.Drawing.Point(231, 590);
         this.btnBrowse.Name = "btnBrowse";
         this.btnBrowse.Size = new System.Drawing.Size(81, 32);
         this.btnBrowse.TabIndex = 29;
         this.btnBrowse.Text = "Browse";
         this.btnBrowse.UseVisualStyleBackColor = true;
         // 
         // btnViewTraffic
         // 
         this.btnViewTraffic.Location = new System.Drawing.Point(708, 655);
         this.btnViewTraffic.Name = "btnViewTraffic";
         this.btnViewTraffic.Size = new System.Drawing.Size(125, 52);
         this.btnViewTraffic.TabIndex = 30;
         this.btnViewTraffic.Text = "View Traffic";
         this.btnViewTraffic.UseVisualStyleBackColor = true;
         this.btnViewTraffic.Click += new System.EventHandler(this.btnViewTraffic_Click);
         // 
         // btnViewLog
         // 
         this.btnViewLog.Location = new System.Drawing.Point(857, 655);
         this.btnViewLog.Name = "btnViewLog";
         this.btnViewLog.Size = new System.Drawing.Size(125, 52);
         this.btnViewLog.TabIndex = 31;
         this.btnViewLog.Text = "View Log";
         this.btnViewLog.UseVisualStyleBackColor = true;
         this.btnViewLog.Click += new System.EventHandler(this.btnViewLog_Click);
         // 
         // btnReadAll
         // 
         this.btnReadAll.Location = new System.Drawing.Point(1005, 655);
         this.btnReadAll.Name = "btnReadAll";
         this.btnReadAll.Size = new System.Drawing.Size(125, 52);
         this.btnReadAll.TabIndex = 32;
         this.btnReadAll.Text = "Read All";
         this.btnReadAll.UseVisualStyleBackColor = true;
         this.btnReadAll.Click += new System.EventHandler(this.btnReadAll_Click);
         // 
         // tclClasses
         // 
         this.tclClasses.Controls.Add(this.tabIndex);
         this.tclClasses.Controls.Add(this.tabIJPOperation);
         this.tclClasses.Controls.Add(this.tabPrintManagement);
         this.tclClasses.Controls.Add(this.tabPrintSpec);
         this.tclClasses.Controls.Add(this.tabPrintFormat);
         this.tclClasses.Controls.Add(this.tabCalendar);
         this.tclClasses.Controls.Add(this.tabSubstitution);
         this.tclClasses.Controls.Add(this.tabCount);
         this.tclClasses.Controls.Add(this.tabUnitInformation);
         this.tclClasses.Controls.Add(this.tabEnviroment);
         this.tclClasses.Controls.Add(this.tabOpMgmt);
         this.tclClasses.Controls.Add(this.tabUserPattern);
         this.tclClasses.Location = new System.Drawing.Point(371, 26);
         this.tclClasses.Multiline = true;
         this.tclClasses.Name = "tclClasses";
         this.tclClasses.SelectedIndex = 0;
         this.tclClasses.Size = new System.Drawing.Size(903, 605);
         this.tclClasses.TabIndex = 33;
         this.tclClasses.SelectedIndexChanged += new System.EventHandler(this.tclClasses_SelectedIndexChanged);
         // 
         // tabIndex
         // 
         this.tabIndex.Location = new System.Drawing.Point(4, 46);
         this.tabIndex.Name = "tabIndex";
         this.tabIndex.Padding = new System.Windows.Forms.Padding(3);
         this.tabIndex.Size = new System.Drawing.Size(895, 555);
         this.tabIndex.TabIndex = 0;
         this.tabIndex.Text = "Index (0x7A)";
         this.tabIndex.UseVisualStyleBackColor = true;
         // 
         // tabIJPOperation
         // 
         this.tabIJPOperation.Location = new System.Drawing.Point(4, 25);
         this.tabIJPOperation.Name = "tabIJPOperation";
         this.tabIJPOperation.Padding = new System.Windows.Forms.Padding(3);
         this.tabIJPOperation.Size = new System.Drawing.Size(895, 576);
         this.tabIJPOperation.TabIndex = 1;
         this.tabIJPOperation.Text = "IJP Operation (0x75)";
         this.tabIJPOperation.UseVisualStyleBackColor = true;
         // 
         // tabPrintManagement
         // 
         this.tabPrintManagement.Location = new System.Drawing.Point(4, 25);
         this.tabPrintManagement.Name = "tabPrintManagement";
         this.tabPrintManagement.Size = new System.Drawing.Size(895, 576);
         this.tabPrintManagement.TabIndex = 4;
         this.tabPrintManagement.Text = "Print Management (066)";
         this.tabPrintManagement.UseVisualStyleBackColor = true;
         // 
         // tabPrintSpec
         // 
         this.tabPrintSpec.Location = new System.Drawing.Point(4, 25);
         this.tabPrintSpec.Name = "tabPrintSpec";
         this.tabPrintSpec.Size = new System.Drawing.Size(895, 576);
         this.tabPrintSpec.TabIndex = 2;
         this.tabPrintSpec.Text = "Print Spec (0x68)";
         this.tabPrintSpec.UseVisualStyleBackColor = true;
         // 
         // tabPrintFormat
         // 
         this.tabPrintFormat.Location = new System.Drawing.Point(4, 25);
         this.tabPrintFormat.Name = "tabPrintFormat";
         this.tabPrintFormat.Size = new System.Drawing.Size(895, 576);
         this.tabPrintFormat.TabIndex = 3;
         this.tabPrintFormat.Text = "Print Format (0x67)";
         this.tabPrintFormat.UseVisualStyleBackColor = true;
         // 
         // tabCalendar
         // 
         this.tabCalendar.Location = new System.Drawing.Point(4, 25);
         this.tabCalendar.Name = "tabCalendar";
         this.tabCalendar.Size = new System.Drawing.Size(895, 576);
         this.tabCalendar.TabIndex = 5;
         this.tabCalendar.Text = "Calendar (0x69)";
         this.tabCalendar.UseVisualStyleBackColor = true;
         // 
         // tabSubstitution
         // 
         this.tabSubstitution.Location = new System.Drawing.Point(4, 25);
         this.tabSubstitution.Name = "tabSubstitution";
         this.tabSubstitution.Size = new System.Drawing.Size(895, 576);
         this.tabSubstitution.TabIndex = 6;
         this.tabSubstitution.Text = "Substitution (0x6C)";
         this.tabSubstitution.UseVisualStyleBackColor = true;
         // 
         // tabCount
         // 
         this.tabCount.Location = new System.Drawing.Point(4, 46);
         this.tabCount.Name = "tabCount";
         this.tabCount.Size = new System.Drawing.Size(895, 555);
         this.tabCount.TabIndex = 7;
         this.tabCount.Text = "Count (0x79)";
         this.tabCount.UseVisualStyleBackColor = true;
         // 
         // tabUnitInformation
         // 
         this.tabUnitInformation.Location = new System.Drawing.Point(4, 46);
         this.tabUnitInformation.Name = "tabUnitInformation";
         this.tabUnitInformation.Size = new System.Drawing.Size(895, 555);
         this.tabUnitInformation.TabIndex = 8;
         this.tabUnitInformation.Text = "Unit Information (0x73)";
         this.tabUnitInformation.UseVisualStyleBackColor = true;
         // 
         // tabEnviroment
         // 
         this.tabEnviroment.Location = new System.Drawing.Point(4, 46);
         this.tabEnviroment.Name = "tabEnviroment";
         this.tabEnviroment.Size = new System.Drawing.Size(895, 555);
         this.tabEnviroment.TabIndex = 9;
         this.tabEnviroment.Text = "Environment Settings (0x71)";
         this.tabEnviroment.UseVisualStyleBackColor = true;
         // 
         // tabOpMgmt
         // 
         this.tabOpMgmt.Location = new System.Drawing.Point(4, 46);
         this.tabOpMgmt.Name = "tabOpMgmt";
         this.tabOpMgmt.Size = new System.Drawing.Size(895, 555);
         this.tabOpMgmt.TabIndex = 10;
         this.tabOpMgmt.Text = "Operation Management (0x74)";
         this.tabOpMgmt.UseVisualStyleBackColor = true;
         // 
         // tabUserPattern
         // 
         this.tabUserPattern.Location = new System.Drawing.Point(4, 46);
         this.tabUserPattern.Name = "tabUserPattern";
         this.tabUserPattern.Size = new System.Drawing.Size(895, 555);
         this.tabUserPattern.TabIndex = 11;
         this.tabUserPattern.Text = "User Pattern (0x6B)";
         this.tabUserPattern.UseVisualStyleBackColor = true;
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1288, 713);
         this.Controls.Add(this.tclClasses);
         this.Controls.Add(this.btnReadAll);
         this.Controls.Add(this.btnViewLog);
         this.Controls.Add(this.btnViewTraffic);
         this.Controls.Add(this.btnBrowse);
         this.Controls.Add(this.txtSaveFolder);
         this.Controls.Add(this.lblSaveFolder);
         this.Controls.Add(this.txtDataDec);
         this.Controls.Add(this.txtStatus);
         this.Controls.Add(this.lblStatus);
         this.Controls.Add(this.txtData);
         this.Controls.Add(this.lbldata);
         this.Controls.Add(this.lblFunction);
         this.Controls.Add(this.cbFunction);
         this.Controls.Add(this.lblClassCode);
         this.Controls.Add(this.cbClassCode);
         this.Controls.Add(this.lblAccessCode);
         this.Controls.Add(this.cbAccessCode);
         this.Controls.Add(this.btnIssueRequest);
         this.Controls.Add(this.btnForwardClose);
         this.Controls.Add(this.btnForwardOpen);
         this.Controls.Add(this.btnExit);
         this.Controls.Add(this.txtSessionID);
         this.Controls.Add(this.lblSessionID);
         this.Controls.Add(this.btnEndSession);
         this.Controls.Add(this.btnStartSession);
         this.Controls.Add(this.txtPort);
         this.Controls.Add(this.txtIPAddress);
         this.Controls.Add(this.lblPort);
         this.Controls.Add(this.lblIPAddress);
         this.Controls.Add(this.btnDisconnect);
         this.Controls.Add(this.btnConnect);
         this.Name = "Form1";
         this.Text = "Form1";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
         this.Load += new System.EventHandler(this.Form1_Load);
         this.Resize += new System.EventHandler(this.Form1_Resize);
         this.tclClasses.ResumeLayout(false);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button btnConnect;
      private System.Windows.Forms.Button btnDisconnect;
      private System.Windows.Forms.Label lblIPAddress;
      private System.Windows.Forms.Label lblPort;
      private System.Windows.Forms.TextBox txtIPAddress;
      private System.Windows.Forms.TextBox txtPort;
      private System.Windows.Forms.Button btnEndSession;
      private System.Windows.Forms.Button btnStartSession;
      private System.Windows.Forms.TextBox txtSessionID;
      private System.Windows.Forms.Label lblSessionID;
      private System.Windows.Forms.Button btnExit;
      private System.Windows.Forms.Button btnForwardClose;
      private System.Windows.Forms.Button btnForwardOpen;
      private System.Windows.Forms.Button btnIssueRequest;
      private System.Windows.Forms.ComboBox cbAccessCode;
      private System.Windows.Forms.Label lblAccessCode;
      private System.Windows.Forms.Label lblClassCode;
      private System.Windows.Forms.ComboBox cbClassCode;
      private System.Windows.Forms.Label lblFunction;
      private System.Windows.Forms.ComboBox cbFunction;
      private System.Windows.Forms.Label lbldata;
      private System.Windows.Forms.TextBox txtData;
      private System.Windows.Forms.TextBox txtStatus;
      private System.Windows.Forms.Label lblStatus;
      private System.Windows.Forms.TextBox txtDataDec;
      private System.Windows.Forms.Label lblSaveFolder;
      private System.Windows.Forms.TextBox txtSaveFolder;
      private System.Windows.Forms.Button btnBrowse;
      private System.Windows.Forms.Button btnViewTraffic;
      private System.Windows.Forms.Button btnViewLog;
      private System.Windows.Forms.Button btnReadAll;
      private System.Windows.Forms.TabControl tclClasses;
      private System.Windows.Forms.TabPage tabIndex;
      private System.Windows.Forms.TabPage tabIJPOperation;
      private System.Windows.Forms.TabPage tabPrintManagement;
      private System.Windows.Forms.TabPage tabPrintSpec;
      private System.Windows.Forms.TabPage tabPrintFormat;
      private System.Windows.Forms.TabPage tabCalendar;
      private System.Windows.Forms.TabPage tabSubstitution;
      private System.Windows.Forms.TabPage tabCount;
      private System.Windows.Forms.TabPage tabUnitInformation;
      private System.Windows.Forms.TabPage tabEnviroment;
      private System.Windows.Forms.TabPage tabOpMgmt;
      private System.Windows.Forms.TabPage tabUserPattern;
   }
}

