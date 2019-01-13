using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace HitachiEIP {
   public partial class Form1 : Form {

      #region Data declarations

      IPAddress IPAddress;
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

      #endregion

      #region Constructors and Destructors

      public Form1() {
         InitializeComponent();
         EIP = new EIP();
         EIP.Log += EIP_Log;
         SetButtonEnables();
      }

      private void EIP_Log(EIP sender, string msg) {
         LogFileStream.WriteLine(msg);
      }

      #endregion

      #region Form Level events

      private void Form1_Load(object sender, EventArgs e) {
         cbAccessCode.Items.Clear();
         cbAccessCode.Items.AddRange(Enum.GetNames(typeof(eipAccessCode)));
         AccessCodes = (eipAccessCode[])Enum.GetValues(typeof(eipAccessCode));

         cbClassCode.Items.Clear();
         cbClassCode.Items.AddRange(Enum.GetNames(typeof(eipClassCode)));
         ClassCodes = (eipClassCode[])Enum.GetValues(typeof(eipClassCode));

         BuildTrafficFile();
         BuildLogFile();

      }

      private void Form1_FormClosing(object sender, FormClosingEventArgs e) {

         EIP.Log -= EIP_Log;

         CloseTrafficFile(false);
         CloseLogFile(false);
      }

      #endregion

      #region Form control events

      private void btnConnect_Click(object sender, EventArgs e) {
         if (!Int32.TryParse(txtPort.Text, out port)) {
            port = 44818;
            txtPort.Text = port.ToString();
         }
         if (!System.Net.IPAddress.TryParse(txtIPAddress.Text, out IPAddress)) {
            txtIPAddress.Text = "192.168.0.1";
            IPAddress = IPAddress.Parse(txtIPAddress.Text);
         }
         EIP.Connect(txtIPAddress.Text, port);
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
         if(EIP.ForwardIsOpen) {
            btnForwardClose_Click(null, null);
         }
         if (EIP.SessionIsOpen) {
            btnEndSession_Click(null, null);
         }
         if(EIP.IsConnected) {
            btnDisconnect_Click(null, null);
         }
         this.Close();
      }

      private void btnIssueRequest_Click(object sender, EventArgs e) {
         if (cbAccessCode.SelectedIndex >= 0
            && cbClassCode.SelectedIndex >= 0
            && cbFunction.SelectedIndex >= 0) {
            EIP.Access = AccessCodes[cbAccessCode.SelectedIndex];
            EIP.Class = ClassCodes[cbClassCode.SelectedIndex];
            EIP.Instance = 0x01;
            EIP.Attribute = (byte)Attributes[cbFunction.SelectedIndex];
            if(EIP.Access == eipAccessCode.Get) {
               EIP.Data = 0;
               EIP.DataLength = 0;
            } else {
               // Got some work to do here
               EIP.Data = 1;
               EIP.DataLength = 1;
            }
            string trafficText = $"{(int)EIP.Access:X2} {(int)EIP.Class & 0xFF:X2} {(int)EIP.Instance:X2} {(int)EIP.Attribute & 0xFF:X2}\t";
            trafficText += $"{EIP.Access }\t{EIP.Class}\t{EIP.Instance}\t{EIP.GetAttributeName(EIP.Class, Attributes[cbFunction.SelectedIndex])}\t";
            try {
               byte[] ed = EIP.EIP_Hitachi(EIP_Type.SendUnitData, AccessCodes[cbAccessCode.SelectedIndex]);
               EIP.Write(ed, 0, ed.Length);

               byte[] data;
               Int32 bytes;
               if (EIP.Read(out data, out bytes)) {
                  int status = (int)Utils.Get(data, 48, 2, mem.LittleEndian);
                  string text = "Unknown!";
                  switch (status) {
                     case 0:
                        text = "O.K.";
                        break;
                     case 0x14:
                        text = "Attribute Not Supported!";
                        break;
                  }
                  trafficText += text + "\t";
                  txtStatus.Text = $"{status:X2} -- {text} -- {(int)EIP.Access:X2} {(int)EIP.Class & 0xFF:X2} {(int)EIP.Instance:X2} {(int)EIP.Attribute:X2}";
                  switch (EIP.Access) {
                     case eipAccessCode.Set:
                     case eipAccessCode.Service:

                        break;
                     case eipAccessCode.Get:
                        string s = string.Empty;
                        for (int i = 50; i < bytes; i++) {
                           s += $"{data[i]:X2} ";
                        }
                        txtData.Text = s;
                        txtDataDec.Text = "?";
                        if (bytes > 50) {
                           if (bytes < 55) {
                              int x = (int)Utils.Get(data, 50, bytes - 50, mem.BigEndian);
                              txtDataDec.Text = x.ToString();
                           } else {
                              s = string.Empty;
                              for (int i = 50; i < bytes; i++) {
                                 if (data[i] > 0x1f && data[i] < 0x80) {
                                    s += (char)data[i];
                                 } else {
                                    s += $"<{data[i]:X2}>";
                                 }
                              }
                              //txtDataDec.Text = s;
                           }
                        }
                        trafficText += $"{txtDataDec.Text}\t{txtData.Text}";
                        break;
                  }
               }
            } catch (Exception e2) {

            }
            TrafficFileStream.WriteLine(trafficText);
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
               case eipClassCode.Index_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipIndex_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Print_data_management_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipPrint_Data_Management_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Print_format_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipPrint_format_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Print_specification_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipPrint_specification_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Calendar_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipCalendar_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.User_pattern_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipUser_pattern_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Substitution_rules_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipSubstitution_rules_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Enviroment_setting_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipEnviroment_setting_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Unit_Information_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipUnit_Information_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Operation_management_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipOperation_management_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.IJP_operation_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipIJP_operation_function), cbFunction, out Attributes);
                  break;
               case eipClassCode.Count_function:
                  n = EIP.GetDropDowns(AccessCodes[cbAccessCode.SelectedIndex], typeof(eipCount_function), cbFunction, out Attributes);
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
         Thread.Sleep(1000);
         btnStartSession_Click(null, null);
         Thread.Sleep(1000);
         btnForwardOpen_Click(null, null);
         Thread.Sleep(1000);

         // Read add attributes from the printer
         cbAccessCode.Text = eipAccessCode.Get.ToString();

         for (int i = 0; i < cbClassCode.Items.Count; i++) {
            cbClassCode.SelectedIndex = i;
            this.Refresh();
            for (int j = 0; j < cbFunction.Items.Count; j++) {
               cbFunction.SelectedIndex = j;
               this.Refresh();
               btnIssueRequest_Click(null, null);
               this.Refresh();
            }
         }

         // Close out the connection
         Thread.Sleep(1000);
         btnForwardClose_Click(null, null);
         Thread.Sleep(1000);
         btnEndSession_Click(null, null);
         Thread.Sleep(1000);
         btnDisconnect_Click(null, null);

      }

      #endregion

      #region Service Routines

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

      private void BuildTrafficFile() {
         TrafficFilename = Utils.CreateTimestampLogFileName(txtSaveFolder.Text, "Traffic");
         TrafficFileStream = new StreamWriter(TrafficFilename, false);
      }

      private void BuildLogFile() {
         LogFilename = Utils.CreateTimestampLogFileName(txtSaveFolder.Text, "Log");
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

      #endregion

   }
}
