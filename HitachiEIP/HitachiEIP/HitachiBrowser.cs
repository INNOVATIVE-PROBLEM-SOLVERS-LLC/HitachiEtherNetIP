using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace HitachiEIP {
   public partial class HitachiBrowser : Form {

      #region Data declarations

      string IPAddress;
      int port;

      EIP EIP;

      eipAccessCode[] AccessCodes;
      eipClassCode[] ClassCodes;
      Type[] ClassCodeAttributes = new Type[] {
            typeof(eipCalendar),                // 0x69
            typeof(eipCount),                   // 0x79
            typeof(eipEnviroment_setting),      // 0x71
            typeof(eipIJP_operation),           // 0x75
            typeof(eipIndex),                   // 0x7A
            typeof(eipOperation_management),    // 0x74
            typeof(eipPrint_Data_Management),   // 0x66
            typeof(eipPrint_format),            // 0x67
            typeof(eipPrint_specification),     // 0x68
            typeof(eipSubstitution_rules),      // 0x6C
            typeof(eipUnit_Information),        // 0x73
            typeof(eipUser_pattern),            // 0x6B
         };
      ulong[] ClassAttr;

      // Traffic/Log files
      string TrafficFilename;
      StreamWriter TrafficFileStream = null;

      string LogFilename;
      StreamWriter LogFileStream = null;

      ResizeInfo R;
      bool initComplete = false;

      public bool AllGood { get; set; } = true;

      // Attribute Screens
      Attributes<eipIndex> indexAttr;               // 0x7A
      Attributes<eipIJP_operation> oprAttr;         // 0x75
      Attributes<eipPrint_Data_Management> pdmAttr; // 0x66
      Attributes<eipPrint_specification> psAttr;    // 0x68
      Attributes<eipPrint_format> pFmtAttr;         // 0x67
      Attributes<eipCalendar> calAttr;              // 0x69
      Attributes<eipSubstitution_rules> sRulesAttr; // 0x6C
      Attributes<eipCount> countAttr;               // 0x79
      Attributes<eipUnit_Information> unitInfoAttr; // 0x73
      Attributes<eipEnviroment_setting> envirAttr;  // 0x71
      Attributes<eipOperation_management> mgmtAttr; // 0x74

      //Attributes<eipUser_pattern> userPatAttr;    // 0x6B Not implemented here

      public bool ComIsOn = false;

      #endregion

      #region Constructors and Destructors

      public HitachiBrowser() {
         InitializeComponent();
         VerifyAddressAndPort();
         EIP = new EIP(txtIPAddress.Text, port);
         EIP.Log += EIP_Log;
         EIP.Error += EIP_Error;
      }

      private void EIP_Error(EIP sender, string msg) {
         AllGood = false;
         lstErrors.Items.Add(msg);
      }

      public void EIP_Log(EIP sender, string msg) {
         LogFileStream.WriteLine(msg);
         lstErrors.Items.Add(msg);
         lstErrors.SelectedIndex = lstErrors.Items.Count - 1;
      }

      #endregion

      #region Form Level events

      private void HitachiBrowser_Load(object sender, EventArgs e) {
         Utils.PositionForm(this, 0.75f, 0.9f);
         cbAccessCode.Items.Clear();
         cbAccessCode.Items.AddRange(Enum.GetNames(typeof(eipAccessCode)));
         AccessCodes = (eipAccessCode[])Enum.GetValues(typeof(eipAccessCode));

         cbClassCode.Items.Clear();
         cbClassCode.Items.AddRange(Enum.GetNames(typeof(eipClassCode)));
         ClassCodes = (eipClassCode[])Enum.GetValues(typeof(eipClassCode));

         BuildTrafficFile();
         BuildLogFile();

         // Load all the tabbed control data
         indexAttr = new Attributes<eipIndex>(this, EIP, tabIndex, eipClassCode.Index);
         oprAttr = new Attributes<eipIJP_operation>(this, EIP, tabIJPOperation, eipClassCode.IJP_operation);
         pdmAttr = new Attributes<eipPrint_Data_Management>(this, EIP, tabPrintManagement, eipClassCode.Print_data_management);
         psAttr = new Attributes<eipPrint_specification>(this, EIP, tabPrintSpec, eipClassCode.Print_specification);
         pFmtAttr = new Attributes<eipPrint_format>(this, EIP, tabPrintFormat, eipClassCode.Print_format);
         calAttr = new Attributes<eipCalendar>(this, EIP, tabCalendar, eipClassCode.Calendar);
         sRulesAttr = new Attributes<eipSubstitution_rules>(this, EIP, tabSubstitution, eipClassCode.Substitution_rules);
         countAttr = new Attributes<eipCount>(this, EIP, tabCount, eipClassCode.Count);
         unitInfoAttr = new Attributes<eipUnit_Information>(this, EIP, tabUnitInformation, eipClassCode.Unit_Information);
         envirAttr = new Attributes<eipEnviroment_setting>(this, EIP, tabEnviroment, eipClassCode.Enviroment_setting);
         mgmtAttr = new Attributes<eipOperation_management>(this, EIP, tabOpMgmt, eipClassCode.Operation_management);
         //userPatAttr = new Attributes<eipUser_pattern>(this, EIP, tabUserPattern, eipClassCode.User_pattern);

         // Force a resize
         initComplete = true;
         HitachiBrowser_Resize(null, null);

         //Start out connected to the printer
         btnStartSession_Click(null, null);

         if (EIP.SessionIsOpen) {
            // COM on is important.  Go get it
            GetComSetting();
         } else {

         }
         SetButtonEnables();
      }

      private void HitachiBrowser_FormClosing(object sender, FormClosingEventArgs e) {

         // Cleanup the open classes
         indexAttr = null;
         oprAttr = null;
         pdmAttr = null;
         psAttr = null;
         pFmtAttr = null;
         calAttr = null;
         sRulesAttr = null;
         countAttr = null;
         unitInfoAttr = null;
         envirAttr = null;
         mgmtAttr = null;
         //userPatAttr = null;

         // Stop logging
         EIP.Log -= EIP_Log;

         // Close log/traffic files
         CloseTrafficFile(false);
         CloseLogFile(false);
      }

      private void HitachiBrowser_Resize(object sender, EventArgs e) {
         //
         // Avoid resize on screen minimize
         if (initComplete && ClientRectangle.Height > 0) {
            //
            this.SuspendLayout();
            // Build local parameters
            R = Utils.InitializeResize(this, 47, 45, true);

            #region Left Column

            Utils.ResizeObject(ref R, lblIPAddress, 1, 1, 2, 3);
            Utils.ResizeObject(ref R, txtIPAddress, 1, 4, 2, 3);
            Utils.ResizeObject(ref R, lblPort, 3, 1, 2, 3);
            Utils.ResizeObject(ref R, txtPort, 3, 4, 2, 3);
            Utils.ResizeObject(ref R, lblSessionID, 5, 1, 2, 3);
            Utils.ResizeObject(ref R, txtSessionID, 5, 4, 2, 3);

            Utils.ResizeObject(ref R, btnStartSession, 7.5f, 0.5f, 2, 3);
            Utils.ResizeObject(ref R, btnEndSession, 7.5f, 4, 2, 3);
            Utils.ResizeObject(ref R, btnForwardOpen, 10, 0.5f, 2, 3);
            Utils.ResizeObject(ref R, btnForwardClose, 10, 4, 2, 3);

            Utils.ResizeObject(ref R, lblAccessCode, 13, 0.5f, 1, 6.5f);
            Utils.ResizeObject(ref R, cbAccessCode, 14, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, lblClassCode, 16, 0.5f, 1, 6.5f);
            Utils.ResizeObject(ref R, cbClassCode, 17, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, lblFunction, 19, 0.5f, 1, 6.5f);
            Utils.ResizeObject(ref R, cbFunction, 20, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, btnIssueRequest, 23, 0.5f, 2, 6.5f);

            Utils.ResizeObject(ref R, lblStatus, 25, 0.5f, 1, 6.5f);
            Utils.ResizeObject(ref R, txtStatus, 26, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, lbldata, 28, 0.5f, 1, 6.5f);
            Utils.ResizeObject(ref R, txtData, 29, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, txtDataDec, 31, 0.5f, 2, 6.5f);

            Utils.ResizeObject(ref R, lblSaveFolder, 33, 0.5f, 1, 6.5f);
            Utils.ResizeObject(ref R, txtSaveFolder, 34, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, btnBrowse, 36, 0.5f, 2, 6.5f);

            Utils.ResizeObject(ref R, lstErrors, 39, 0.5f, 7, 6.5f);

            #endregion

            #region  Classes

            Utils.ResizeObject(ref R, tclClasses, 1, 8, 42, 36);

            indexAttr.ResizeControls(ref R);
            oprAttr.ResizeControls(ref R);
            pdmAttr.ResizeControls(ref R);
            psAttr.ResizeControls(ref R);
            pFmtAttr.ResizeControls(ref R);
            calAttr.ResizeControls(ref R);
            sRulesAttr.ResizeControls(ref R);
            countAttr.ResizeControls(ref R);
            unitInfoAttr.ResizeControls(ref R);
            envirAttr.ResizeControls(ref R);
            mgmtAttr.ResizeControls(ref R);
            //userPatAttr.ResizeControls(ref R);

            #endregion

            #region Bottom Row

            Utils.ResizeObject(ref R, btnCom, 43.5f, 8, 3, 3);
            Utils.ResizeObject(ref R, btnStop, 44, 27.5f, 2, 3);
            Utils.ResizeObject(ref R, btnViewTraffic, 44, 31, 2, 3);
            Utils.ResizeObject(ref R, btnViewLog, 44, 34.5f, 2, 3);
            Utils.ResizeObject(ref R, btnReadAll, 44, 38, 2, 3);
            Utils.ResizeObject(ref R, btnExit, 44, 41.5f, 2, 3);

            #endregion

            this.Refresh();
            this.ResumeLayout();
         }
      }

      #endregion

      #region Form control events

      private void btnStartSession_Click(object sender, EventArgs e) {
         VerifyAddressAndPort();
         //EIP.Connect(IPAddress, port);
         EIP.StartSession();
         txtSessionID.Text = EIP.SessionID.ToString();
         SetButtonEnables();
      }

      private void btnEndSession_Click(object sender, EventArgs e) {
         EIP.EndSession();
         txtSessionID.Text = string.Empty;
         //EIP.Disconnect();
         SetButtonEnables();
      }

      private void btnForwardOpen_Click(object sender, EventArgs e) {
         EIP.ForwardOpen();
         SetButtonEnables();
      }

      private void btnForwardClose_Click(object sender, EventArgs e) {
         EIP.ForwardClose();
         SetButtonEnables();
      }

      private void btnExit_Click(object sender, EventArgs e) {
         if (EIP.ForwardIsOpen) {
            btnForwardClose_Click(null, null);
         }
         if (EIP.SessionIsOpen) {
            btnEndSession_Click(null, null);
         }
         this.Close();
      }

      private void btnIssueRequest_Click(object sender, EventArgs e) {
         bool Success = false;
         if (cbAccessCode.SelectedIndex >= 0
            && cbClassCode.SelectedIndex >= 0
            && cbFunction.SelectedIndex >= 0) {
            try {
               switch (AccessCodes[cbAccessCode.SelectedIndex]) {
                  case eipAccessCode.Set:
                     // Got some work to do here
                     byte[] Data = new byte[] { 1 };
                     Success = EIP.WriteOneAttribute(ClassCodes[cbClassCode.SelectedIndex], (byte)ClassAttr[cbFunction.SelectedIndex], Data);
                     break;
                  case eipAccessCode.Get:
                     Success = EIP.ReadOneAttribute(ClassCodes[cbClassCode.SelectedIndex], (byte)ClassAttr[cbFunction.SelectedIndex], out string val, DataFormats.Bytes);
                     break;
                  case eipAccessCode.Service:
                     break;
               }
               string trafficText = $"{(int)EIP.Access:X2} {(int)EIP.Class & 0xFF:X2} {(int)EIP.Instance:X2} {(int)EIP.Attribute & 0xFF:X2}\t";
               trafficText += $"{EIP.Access }\t{EIP.Class}\t{EIP.Instance}\t{EIP.GetAttributeName(EIP.Class, ClassAttr[cbFunction.SelectedIndex])}\t";
               if (Success) {
                  string hdr = EIP.GetBytes(EIP.ReadData, 46, 4);
                  int status = (int)EIP.Get(EIP.ReadData, 48, 2, mem.LittleEndian);
                  string text = "Unknown!";
                  switch (status) {
                     case 0:
                        text = "O.K.";
                        break;
                     case 0x14:
                        text = "Attribute Not Supported!";
                        break;
                  }
                  trafficText += $"{hdr}\t{text}\t";
                  txtStatus.Text = $"{status:X2} -- {text} -- {(int)EIP.Access:X2} {(int)EIP.Class & 0xFF:X2} {(int)EIP.Instance:X2} {(int)EIP.Attribute:X2}";
                  switch (EIP.Access) {
                     case eipAccessCode.Set:
                     case eipAccessCode.Service:

                        break;
                     case eipAccessCode.Get:
                        int length = EIP.ReadDataLength - 50;
                        string s = EIP.GetBytes(EIP.ReadData, 50, length);
                        txtData.Text = s;
                        txtDataDec.Text = "N/A";
                        if (EIP.ReadDataLength > 50) {
                           if (length < 5) {
                              int x = (int)EIP.Get(EIP.ReadData, 50, length, mem.BigEndian);
                              txtDataDec.Text = x.ToString();
                           } else {
                              s = string.Empty;
                              for (int i = 50; i < EIP.ReadDataLength; i++) {
                                 if (EIP.ReadData[i] > 0x1f && EIP.ReadData[i] < 0x80) {
                                    s += (char)EIP.ReadData[i];
                                 } else {
                                    s += $"<{EIP.ReadData[i]:X2}>";
                                 }
                              }
                              //txtDataDec.Text = s;
                           }
                        }
                        trafficText += $"{length}\t{txtDataDec.Text}\t{txtData.Text}";
                        break;
                  }
               }
               TrafficFileStream.WriteLine(trafficText);
            } catch (Exception e2) {
               AllGood = false;
            }
         }
      }

      private void cbAccessCode_SelectedIndexChanged(object sender, EventArgs e) {
         cbClassCode_SelectedIndexChanged(null, null);
         SetButtonEnables();
      }

      private void cbClassCode_SelectedIndexChanged(object sender, EventArgs e) {
         cbFunction.Items.Clear();
         ClassAttr = null;
         if (cbAccessCode.SelectedIndex >= 0 && cbClassCode.SelectedIndex >= 0) {
            int n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], cbFunction, ClassCodeAttributes[cbClassCode.SelectedIndex], out ClassAttr);
            lblFunction.Text = $"Function Code -- {n} found]";
         }
         SetButtonEnables();
      }

      private void cbFunction_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      private void btnViewTraffic_Click(object sender, EventArgs e) {
         CloseTrafficFile(true);
      }

      private void btnViewLog_Click(object sender, EventArgs e) {
         CloseLogFile(true);
      }

      private void btnReadAll_Click(object sender, EventArgs e) {
         // Read add attributes from the printer
         cbAccessCode.Text = eipAccessCode.Get.ToString();

         for (int i = 0; i < cbClassCode.Items.Count; i++) {
            // Set AllGood for this class
            AllGood = true;
            cbClassCode.SelectedIndex = i;
            this.Refresh();
            // Establish the connection
            btnStartSession_Click(null, null);
            btnForwardOpen_Click(null, null);
            // Issue commands for this group
            for (int j = 0; j < cbFunction.Items.Count && AllGood; j++) {
               cbFunction.SelectedIndex = j;
               this.Refresh();
               btnIssueRequest_Click(null, null);
            }
            // Close out the connection
            btnForwardClose_Click(null, null);
            btnEndSession_Click(null, null);
         }


      }

      private void btnCom_Click(object sender, EventArgs e) {
         if (EIP.SessionIsOpen) {
            int val = ComIsOn ? 0 : 1;
            if (EIP.WriteOneAttribute(eipClassCode.IJP_operation, EIP.GetAttribute((ulong)eipIJP_operation.Online_Offline), EIP.ToBytes((uint)val, 1))) {
               GetComSetting();
            }
         }
         SetButtonEnables();
      }

      private void tclClasses_SelectedIndexChanged(object sender, EventArgs e) {
         HitachiBrowser_Resize(null, null);
      }

      private void cmLogClear_Click(object sender, EventArgs e) {
         lstErrors.Items.Clear();
      }

      private void cmLogView_Click(object sender, EventArgs e) {
         string ViewFilename = CreateFileName(txtSaveFolder.Text, "View");
         StreamWriter ViewStream = new StreamWriter(ViewFilename, false);
         for (int i = 0; i < lstErrors.Items.Count; i++) {
            ViewStream.WriteLine(lstErrors.Items[i].ToString());
         }
         ViewStream.Flush();
         ViewStream.Close();
         Process.Start("notepad.exe", ViewFilename);
      }

      private void lstErrors_MouseDoubleClick(object sender, MouseEventArgs e) {
         cmLogView_Click(null, null);
      }

      private void btnStop_Click(object sender, EventArgs e) {
         AllGood = false;
      }

      #endregion

      #region Service Routines

      private void VerifyAddressAndPort() {
         if (!Int32.TryParse(txtPort.Text, out port)) {
            port = 44818;
            txtPort.Text = port.ToString();
         }
         if (!System.Net.IPAddress.TryParse(txtIPAddress.Text, out System.Net.IPAddress IPAddress)) {
            txtIPAddress.Text = "192.168.0.1";
            IPAddress = IPAddress.Parse(txtIPAddress.Text);
         }
         this.IPAddress = IPAddress.ToString();
      }

      private void BuildTrafficFile() {
         TrafficFilename = CreateFileName(txtSaveFolder.Text, "Traffic");
         TrafficFileStream = new StreamWriter(TrafficFilename, false);
      }

      private void BuildLogFile() {
         LogFilename = CreateFileName(txtSaveFolder.Text, "Log");
         LogFileStream = new StreamWriter(LogFilename, false);
      }

      private void CloseTrafficFile(bool view) {
         TrafficFileStream.Flush();
         TrafficFileStream.Close();
         if (view) {
            Process.Start("notepad.exe", TrafficFilename);
            BuildTrafficFile();
         }
      }

      private void CloseLogFile(bool view) {
         LogFileStream.Flush();
         LogFileStream.Close();
         if (view) {
            Process.Start("notepad.exe", LogFilename);
            BuildLogFile();
         }
      }

      private string CreateFileName(string directory, string s) {
         return Path.Combine(directory, $"{s}{DateTime.Now.ToString("yyMMdd-HHmmss")}.txt");
      }

      private bool GetComSetting() {
         bool result;
         if (EIP.ReadOneAttribute(eipClassCode.IJP_operation, EIP.GetAttribute((ulong)eipIJP_operation.Online_Offline), out string val, DataFormats.Decimal)) {
            if (val == "1") {
               btnCom.Text = "COM = 1";
               btnCom.BackColor = Color.Green;
               ComIsOn = true;
            } else {
               btnCom.Text = "COM = 0";
               btnCom.BackColor = Color.Red;
               ComIsOn = false;
            }
            result = true;
         } else {
            btnCom.Text = "COM = ?";
            btnCom.BackColor = SystemColors.Control;
            ComIsOn = false;
            result = false;
         }
         SetButtonEnables();
         return result;
      }

      void SetButtonEnables() {
         btnStartSession.Enabled = !EIP.SessionIsOpen;
         btnEndSession.Enabled = EIP.SessionIsOpen;
         btnForwardOpen.Enabled = EIP.SessionIsOpen && !EIP.ForwardIsOpen;
         btnForwardClose.Enabled = EIP.SessionIsOpen && EIP.ForwardIsOpen;
         btnIssueRequest.Enabled = EIP.SessionIsOpen && EIP.ForwardIsOpen
            && cbAccessCode.SelectedIndex >= 0 && cbClassCode.SelectedIndex >= 0 && cbFunction.SelectedIndex >= 0;

         if (initComplete) {
            indexAttr.SetButtonEnables();
            oprAttr.SetButtonEnables();
            pdmAttr.SetButtonEnables();
            psAttr.SetButtonEnables();
            pFmtAttr.SetButtonEnables();
            calAttr.SetButtonEnables();
            sRulesAttr.SetButtonEnables();
            countAttr.SetButtonEnables();
            unitInfoAttr.SetButtonEnables();
            envirAttr.SetButtonEnables();
            mgmtAttr.SetButtonEnables();
            //userPatAttr.SetButtonEnables();
         }

      }

      #endregion

   }
}
