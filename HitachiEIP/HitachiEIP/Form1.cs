using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace HitachiEIP {
   public partial class Form1 : Form {

      #region Data declarations

      string IPAddress;
      int port;

      EIP EIP;

      eipAccessCode[] AccessCodes;
      eipClassCode[] ClassCodes;
      uint[] Attributes;

      // Traffic/Log files
      string TrafficFilename;
      StreamWriter TrafficFileStream = null;

      string LogFilename;
      StreamWriter LogFileStream = null;

      ResizeInfo R;
      bool initComplete = false;

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
      //Attributes<eipUser_pattern> userPatAttr;      // 0x6B

      #endregion

      #region Constructors and Destructors

      public Form1() {
         InitializeComponent();
         VerifyAddressAndPort();
         EIP = new EIP(txtIPAddress.Text, port);
         EIP.Log += EIP_Log;
      }

      private void EIP_Log(EIP sender, string msg) {
         LogFileStream.WriteLine(msg);
      }

      #endregion

      #region Form Level events

      private void Form1_Load(object sender, EventArgs e) {
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
         calAttr = new Attributes<eipCalendar>(this, EIP, tabPrintFormat, eipClassCode.Calendar);
         sRulesAttr = new Attributes<eipSubstitution_rules>(this, EIP, tabSubstitution, eipClassCode.Substitution_rules);
         countAttr = new Attributes<eipCount>(this, EIP, tabCount, eipClassCode.Count);
         unitInfoAttr = new Attributes<eipUnit_Information>(this, EIP, tabUnitInformation, eipClassCode.Unit_Information);
         envirAttr = new Attributes<eipEnviroment_setting>(this, EIP, tabEnviroment, eipClassCode.Enviroment_setting);
         mgmtAttr = new Attributes<eipOperation_management>(this, EIP, tabOpMgmt, eipClassCode.Operation_management);
         //userPatAttr = new Attributes<eipUser_pattern>(this, EIP, tabUserPattern, eipClassCode.User_pattern);

         SetButtonEnables();

         initComplete = true;
         Form1_Resize(null, null);
      }

      private void Form1_FormClosing(object sender, FormClosingEventArgs e) {

         EIP.Log -= EIP_Log;

         CloseTrafficFile(false);
         CloseLogFile(false);
      }

      private void Form1_Resize(object sender, EventArgs e) {
         //
         // Avoid resize on screen minimize
         if (initComplete && ClientRectangle.Height > 0) {
            //
            this.SuspendLayout();
            // Build local parameters
            R = Utils.InitializeResize(this, 47, 35, true);

            #region Left Column

            Utils.ResizeObject(ref R, lblIPAddress, 1, 1, 2, 3);
            Utils.ResizeObject(ref R, txtIPAddress, 1, 4, 2, 3);
            Utils.ResizeObject(ref R, lblPort, 3, 1, 2, 3);
            Utils.ResizeObject(ref R, txtPort, 3, 4, 2, 3);
            Utils.ResizeObject(ref R, lblSessionID, 5, 1, 2, 3);
            Utils.ResizeObject(ref R, txtSessionID, 5, 4, 2, 3);

            Utils.ResizeObject(ref R, btnConnect, 8, 0.5f, 2, 3);
            Utils.ResizeObject(ref R, btnDisconnect, 8, 4, 2, 3);
            Utils.ResizeObject(ref R, btnStartSession, 10.5f, 0.5f, 2, 3);
            Utils.ResizeObject(ref R, btnEndSession, 10.5f, 4, 2, 3);
            Utils.ResizeObject(ref R, btnForwardOpen, 13, 0.5f, 2, 3);
            Utils.ResizeObject(ref R, btnForwardClose, 13, 4, 2, 3);

            Utils.ResizeObject(ref R, lblAccessCode, 16, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, cbAccessCode, 18, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, lblClassCode, 20, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, cbClassCode, 22, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, lblFunction, 24, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, cbFunction, 26, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, btnIssueRequest, 28, 0.5f, 2, 6.5f);

            Utils.ResizeObject(ref R, lblStatus, 30, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, txtStatus, 32, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, lbldata, 34, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, txtData, 36, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, txtDataDec, 38, 0.5f, 2, 6.5f);

            Utils.ResizeObject(ref R, lblSaveFolder, 40, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, txtSaveFolder, 42, 0.5f, 2, 6.5f);
            Utils.ResizeObject(ref R, btnBrowse, 44, 0.5f, 2, 6.5f);

            #endregion

            #region  Classes

            Utils.ResizeObject(ref R, tclClasses, 1, 8, 42, 26);

            #endregion

            if (initComplete) {
               indexAttr.ResizeControls(ref R); // 0x7A
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
            }

            #endregion

            #region Bottom Row

            Utils.ResizeObject(ref R, btnViewTraffic, 44, 15, 2, 4);
            Utils.ResizeObject(ref R, btnViewLog, 44, 20, 2, 4);
            Utils.ResizeObject(ref R, btnReadAll, 44, 25, 2, 4);
            Utils.ResizeObject(ref R, btnExit, 44, 30, 2, 4);

            #endregion

            this.Refresh();
            this.ResumeLayout();
         }
      }

      #region Form control events

      private void btnConnect_Click(object sender, EventArgs e) {
         VerifyAddressAndPort();
         EIP.Connect(IPAddress, port);
         SetButtonEnables();
      }

      private void btnDisconnect_Click(object sender, EventArgs e) {
         EIP.Disconnect();
         SetButtonEnables();
      }

      private void btnStartSession_Click(object sender, EventArgs e) {
         EIP.StartSession();
         txtSessionID.Text = EIP.SessionID.ToString();
         SetButtonEnables();
      }

      private void btnEndSession_Click(object sender, EventArgs e) {
         EIP.EndSession();
         txtSessionID.Text = string.Empty;
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
         if (EIP.IsConnected) {
            btnDisconnect_Click(null, null);
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
                     Success = EIP.WriteOneAttribute(ClassCodes[cbClassCode.SelectedIndex], (byte)Attributes[cbFunction.SelectedIndex], Data);
                     break;
                  case eipAccessCode.Get:
                     Success = EIP.ReadOneAttribute(ClassCodes[cbClassCode.SelectedIndex], (byte)Attributes[cbFunction.SelectedIndex], out string val, DataFormats.Bytes);
                     break;
                  case eipAccessCode.Service:
                     break;
               }
               string trafficText = $"{(int)EIP.Access:X2} {(int)EIP.Class & 0xFF:X2} {(int)EIP.Instance:X2} {(int)EIP.Attribute & 0xFF:X2}\t";
               trafficText += $"{EIP.Access }\t{EIP.Class}\t{EIP.Instance}\t{EIP.GetAttributeName(EIP.Class, Attributes[cbFunction.SelectedIndex])}\t";
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

            }
         }
      }

      private void cbAccessCode_SelectedIndexChanged(object sender, EventArgs e) {
         cbClassCode_SelectedIndexChanged(null, null);
         SetButtonEnables();
      }

      private void cbClassCode_SelectedIndexChanged(object sender, EventArgs e) {
         cbFunction.Items.Clear();
         Attributes = null;
         if (cbAccessCode.SelectedIndex >= 0 && cbClassCode.SelectedIndex >= 0) {
            int n = 0;
            switch (ClassCodes[cbClassCode.SelectedIndex]) {
               case eipClassCode.Index:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipIndex), cbFunction, out Attributes);
                  break;
               case eipClassCode.Print_data_management:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipPrint_Data_Management), cbFunction, out Attributes);
                  break;
               case eipClassCode.Print_format:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipPrint_format), cbFunction, out Attributes);
                  break;
               case eipClassCode.Print_specification:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipPrint_specification), cbFunction, out Attributes);
                  break;
               case eipClassCode.Calendar:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipCalendar), cbFunction, out Attributes);
                  break;
               case eipClassCode.User_pattern:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipUser_pattern), cbFunction, out Attributes);
                  break;
               case eipClassCode.Substitution_rules:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipSubstitution_rules), cbFunction, out Attributes);
                  break;
               case eipClassCode.Enviroment_setting:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipEnviroment_setting), cbFunction, out Attributes);
                  break;
               case eipClassCode.Unit_Information:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipUnit_Information), cbFunction, out Attributes);
                  break;
               case eipClassCode.Operation_management:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipOperation_management), cbFunction, out Attributes);
                  break;
               case eipClassCode.IJP_operation:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipIJP_operation), cbFunction, out Attributes);
                  break;
               case eipClassCode.Count:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipCount), cbFunction, out Attributes);
                  break;
               default:
                  break;
            }
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
         // Establish the connection
         btnConnect_Click(null, null);
         btnStartSession_Click(null, null);
         btnForwardOpen_Click(null, null);

         // Read add attributes from the printer
         cbAccessCode.Text = eipAccessCode.Get.ToString();

         for (int i = 0; i < cbClassCode.Items.Count; i++) {
            cbClassCode.SelectedIndex = i;
            this.Refresh();
            for (int j = 0; j < cbFunction.Items.Count; j++) {
               cbFunction.SelectedIndex = j;
               this.Refresh();
               btnIssueRequest_Click(null, null);
            }
         }

         // Close out the connection
         btnForwardClose_Click(null, null);
         btnEndSession_Click(null, null);
         btnDisconnect_Click(null, null);

      }

      private void tclClasses_SelectedIndexChanged(object sender, EventArgs e) {
         Form1_Resize(null, null);
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

      void SetButtonEnables() {
         btnConnect.Enabled = !EIP.IsConnected;
         btnDisconnect.Enabled = EIP.IsConnected;
         btnStartSession.Enabled = EIP.IsConnected && !EIP.SessionIsOpen;
         btnEndSession.Enabled = EIP.IsConnected && EIP.SessionIsOpen;
         btnForwardOpen.Enabled = EIP.IsConnected && EIP.SessionIsOpen && !EIP.ForwardIsOpen;
         btnForwardClose.Enabled = EIP.IsConnected && EIP.SessionIsOpen && EIP.ForwardIsOpen;
         btnIssueRequest.Enabled = EIP.IsConnected && EIP.SessionIsOpen && EIP.ForwardIsOpen && EIP.ForwardIsOpen
            && cbAccessCode.SelectedIndex >= 0 && cbClassCode.SelectedIndex >= 0 && cbFunction.SelectedIndex >= 0;
      }

      #endregion

   }
}
