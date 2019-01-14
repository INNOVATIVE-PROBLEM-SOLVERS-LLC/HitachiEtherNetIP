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

      enum DataFormats {
         Decimal,
         Bytes,
      }

      #endregion

      #region Constructors and Destructors

      public Form1() {
         InitializeComponent();
         EIP = new EIP();
         EIP.Log += EIP_Log;
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

         // Load all the tabbed control data
         indexLoad();
         ijpOpLoad();

         SetButtonEnables();
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
         if (cbAccessCode.SelectedIndex >= 0
            && cbClassCode.SelectedIndex >= 0
            && cbFunction.SelectedIndex >= 0) {
            EIP.Access = AccessCodes[cbAccessCode.SelectedIndex];
            EIP.Class = ClassCodes[cbClassCode.SelectedIndex];
            EIP.Instance = 0x01;
            EIP.Attribute = (byte)Attributes[cbFunction.SelectedIndex];
            if (EIP.Access == eipAccessCode.Get) {
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
                  string hdr = EIP.GetBytes(data, 46, 4);
                  int status = (int)EIP.Get(data, 48, 2, mem.LittleEndian);
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
                        int length = bytes - 50;
                        string s = EIP.GetBytes(data, 50, length);
                        txtData.Text = s;
                        txtDataDec.Text = "N/A";
                        if (bytes > 50) {
                           if (length < 5) {
                              int x = (int)EIP.Get(data, 50, length, mem.BigEndian);
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
                        trafficText += $"{length}\t{txtDataDec.Text}\t{txtData.Text}";
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

      #endregion

      #region Index Tab Controls

      eipIndex[] indexAttributes = new eipIndex[] {
         eipIndex.Start_Stop_Management_Flag,
         eipIndex.Automatic_reflection,
         eipIndex.Item_Count,
         eipIndex.Column,
         eipIndex.Line,
         eipIndex.Character_position,
         eipIndex.Print_Data_Message_Number,
         eipIndex.Print_Data_Group_Data,
         eipIndex.Substitution_Rules_Setting,
         eipIndex.User_Pattern_Size,
         eipIndex.Count_Block,
         eipIndex.Calendar_Block,
      };

      Label[] indexLabel;
      TextBox[] indexText;
      Button[] indexGet;
      Button[] indexSet;

      private int[,] validIndexData = new int[,] {
         {0, 1 }, {0, 1 }, {1, 100 }, {1, 100 }, {1, 6 }, {1, 1000 }, {1, 2000 },
         {1, 99 }, {1, 99 }, {1, 19 }, {1, 8 }, {1, 8 }
      };

      private void indexLoad() {

         indexLabel = new Label[] {
            lblIndex64, lblIndex65, lblIndex66, lblIndex67, lblIndex68, lblIndex69,
            lblIndex6A, lblIndex6B, lblIndex6C, lblIndex6D, lblIndex6E, lblIndex6F
         };

         indexText = new TextBox[] {
            txtIndex64, txtIndex65, txtIndex66, txtIndex67, txtIndex68, txtIndex69,
            txtIndex6A, txtIndex6B, txtIndex6C, txtIndex6D, txtIndex6E, txtIndex6F
         };

         indexGet = new Button[] {
            btnIndexGet64, btnIndexGet65, btnIndexGet66, btnIndexGet67, btnIndexGet68, btnIndexGet69,
            btnIndexGet6A, btnIndexGet6B, btnIndexGet6C, btnIndexGet6D, btnIndexGet6E, btnIndexGet6F
         };

         indexSet = new Button[] {
            btnIndexSet64, btnIndexSet65, btnIndexSet66, btnIndexSet67, btnIndexSet68, btnIndexSet69,
            btnIndexSet6A, btnIndexSet6B, btnIndexSet6C, btnIndexSet6D, btnIndexSet6E, btnIndexSet6F
         };

         for (int i = 0; i < indexLabel.Length; i++) {
            indexLabel[i].Text = EIP.GetAttributeName(eipClassCode.Index, (uint)indexAttributes[i]);
         }
      }

      private void btnIndexGet_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = Convert.ToInt32(b.Tag);
         string val;

         indexText[tag].Text = "Loading";
         if (ReadOneAttribute(eipClassCode.Index, (byte)indexAttributes[tag], out val, DataFormats.Decimal)) {
            indexText[tag].Text = val;
         } else {
            indexText[tag].Text = "#Error";
         }

         SetButtonEnables();
      }

      private void btnIndexSet_Click(object sender, EventArgs e) {

         Button b = (Button)sender;
         int tag = Convert.ToInt32(b.Tag);

         byte[] data;
         Int32 bytes;

         int val;
         if (!int.TryParse(indexText[tag].Text, out val)) {
            val = validIndexData[tag, 0];
         }
         int len = ((int)indexAttributes[tag] & 0xFF0000) >> 16;

         EIP.ForwardOpen();

         EIP.Access = eipAccessCode.Set;
         EIP.Class = eipClassCode.Index;
         EIP.Instance = 0x01;
         EIP.Attribute = (byte)indexAttributes[tag];
         EIP.Data = (ulong)val;
         EIP.DataLength = (byte)len;
         try {
            byte[] ed = EIP.EIP_Hitachi(EIP_Type.SendUnitData, eipAccessCode.Set);
            EIP.Write(ed, 0, ed.Length);

            if (EIP.Read(out data, out bytes)) {

            }
         } catch (Exception e2) {

         }
         btnForwardClose_Click(null, null);

      }

      private void btnGetAll_Click(object sender, EventArgs e) {
         for (int i = 0; i < indexGet.Length; i++) {
            btnIndexGet_Click(indexGet[i], null);
            this.Refresh();
         }
      }

      private void btnSetAll_Click(object sender, EventArgs e) {
         for (int i = 0; i < indexSet.Length; i++) {
            btnIndexSet_Click(indexSet[i], null);
         }
      }

      private void NumbersOnly(object sender, KeyPressEventArgs e) {
         TextBox t = (TextBox)sender;
         e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
      }

      private void txtIndex_Leave(object sender, EventArgs e) {
         TextBox t = (TextBox)sender;
         int tag = Convert.ToInt32(t.Tag);
         int val;
         if (!string.IsNullOrEmpty(t.Text)) {
            if (!int.TryParse(t.Text, out val)) {
               MessageBox.Show($"Invalid Text =>{t.Text}<=");
               t.Text = validIndexData[tag, 0].ToString();
            } else {
               if (val < validIndexData[tag, 0] || val > validIndexData[tag, 1]) {
                  MessageBox.Show($"Invalid Value =>{t.Text}<=", $"Index Data == {indexLabel[tag].Text}", MessageBoxButtons.OK);
                  t.Text = validIndexData[tag, 0].ToString();
               }
            }
         }
         SetIndexButtonEnables();
      }

      private void SetIndexButtonEnables() {
         bool enable = EIP.IsConnected && EIP.SessionIsOpen;
         bool allValid = true;
         for (int i = 0; i < indexGet.Length; i++) {
            bool dataValid = false;
            int val = 0;
            if (int.TryParse(indexText[i].Text, out val)) {
               dataValid = val >= validIndexData[i, 0] && val <= validIndexData[i, 1];
            }
            indexGet[i].Enabled = enable;
            indexSet[i].Enabled = enable && dataValid;
            allValid &= dataValid;
         }
         btnGetAll.Enabled = enable;
         btnSetAll.Enabled = enable && allValid;
      }

      #endregion

      #region IJP Operation Tab Controls

      eipIJP_operation[] ijpOpAttributes = new eipIJP_operation[] {
      eipIJP_operation.Remote_operation_information,
      eipIJP_operation.Fault_and_warning_history,
      eipIJP_operation.Operating_condition,
      eipIJP_operation.Warning_condition,
      eipIJP_operation.Date_and_time_information,
      eipIJP_operation.Error_code,
      eipIJP_operation.Start_Remote_Operation,
      eipIJP_operation.Stop_Remote_Operation,
      eipIJP_operation.Deflection_voltage_control,
      eipIJP_operation.Online_Offline,
      };

      Label[] ijpOpLabel;
      TextBox[] ijpOpText;
      Button[] ijpOpxGet;
      Button[] ijpOpxSet;
      Button[] ijpOpxSvc;

      private int[,] validIjpOpData = new int[,] {
         {0, 0 }, {0, 0 }, {0, 0 }, {0, 0 }, {0, 0 },
         {0, 0 }, {0, 0 }, {0, 0 }, {0, 0 }, {0, 1 }
      };
      private void ijpOpLoad() {

         ijpOpLabel = new Label[] {
            lblIJPOp64, lblIJPOp66, lblIJPOp67, lblIJPOp68, lblIJPOp6A,
            lblIJPOp6B, lblIJPOp6C, lblIJPOp6D, lblIJPOp6E, lblIJPOp6F,
         };

         ijpOpText = new TextBox[] {
            txtIJPOp64, txtIJPOp66, txtIJPOp67, txtIJPOp68, txtIJPOp6A,
            txtIJPOp6B, txtIJPOp6C, txtIJPOp6D, txtIJPOp6E, txtIJPOp6F,
         };

         ijpOpxGet = new Button[] {
            btnIJPOpGet64, btnIJPOpGet66, btnIJPOpGet67, btnIJPOpGet68, btnIJPOpGet6A,
            btnIJPOpGet6B, null, null, null, btnIJPOpGet6F,
         };

         ijpOpxSet = new Button[] {
            null, null, null, null, null,
            null, null, null, null, btnIJPOpSet6F,
         };

         ijpOpxSvc = new Button[] {
            null, null, null, null, null,
            null, btnIJPOpSvc6C, btnIJPOpSvc6D, btnIJPOpSvc6E, null,
         };

         for (int i = 0; i < ijpOpLabel.Length; i++) {
            ijpOpLabel[i].Text = EIP.GetAttributeName(eipClassCode.IJP_operation, (uint)ijpOpAttributes[i]);
         }
      }

      private void btnIJPOpGet_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = Convert.ToInt32(b.Tag);
         string val;

         ijpOpText[tag].Text = "Loading";
         DataFormats fmt = DataFormats.Bytes;
         if (ijpOpAttributes[tag] == eipIJP_operation.Online_Offline) {
            fmt = DataFormats.Decimal;
         }
         if (ReadOneAttribute(eipClassCode.IJP_operation, (byte)ijpOpAttributes[tag], out val, fmt)) {
            ijpOpText[tag].Text = val;
         } else {
            ijpOpText[tag].Text = "#Error";
         }

         SetButtonEnables();
      }

      private void btnIJPOpGetAll_Click(object sender, EventArgs e) {
         for (int i = 0; i < ijpOpxGet.Length; i++) {
            if (ijpOpxGet[i] != null) {
               btnIJPOpGet_Click(ijpOpxGet[i], null);
               this.Refresh();
            }
         }
      }
      private void txtIJPOp6F_Leave(object sender, EventArgs e) {
         TextBox t = (TextBox)sender;
         int tag = Convert.ToInt32(t.Tag);
         int val;
         if (!string.IsNullOrEmpty(t.Text)) {
            if (!int.TryParse(t.Text, out val)) {
               MessageBox.Show($"Invalid Text =>{t.Text}<=");
               t.Text = validIjpOpData[tag, 0].ToString();
            } else {
               if (val < validIjpOpData[tag, 0] || val > validIjpOpData[tag, 1]) {
                  MessageBox.Show($"Invalid Value =>{t.Text}<=", $"IJP Operation Data == {ijpOpLabel[tag].Text}", MessageBoxButtons.OK);
                  t.Text = validIjpOpData[tag, 0].ToString();
               }
            }
         }
         SetIndexButtonEnables();
      }

      #endregion

      #region Service Routines

      private bool ReadOneAttribute(eipClassCode Class, byte Attribute, out string val, DataFormats fmt) {
         bool result = false;
         byte[] data;
         Int32 bytes;

         val = string.Empty;
         EIP.ForwardOpen();

         EIP.Access = eipAccessCode.Get;
         EIP.Class = Class;
         EIP.Instance = 0x01;
         EIP.Attribute = Attribute;
         EIP.Data = 0;
         EIP.DataLength = 0;
         try {

            byte[] ed = EIP.EIP_Hitachi(EIP_Type.SendUnitData, eipAccessCode.Get);
            EIP.Write(ed, 0, ed.Length);

            if (EIP.Read(out data, out bytes)) {
               int status = (int)EIP.Get(data, 48, 2, mem.LittleEndian);
               if (status == 0) {
                  switch (fmt) {
                     case DataFormats.Decimal:
                        val = EIP.Get(data, 50, bytes - 50, mem.BigEndian).ToString();
                        break;
                     case DataFormats.Bytes:
                        val = EIP.GetBytes(data, 50, bytes - 50);
                        break;
                     default:
                        break;
                  }
                  result = true;
               } else {
                  val = "#Error";
               }
            }
         } catch (Exception e2) {

         }

         btnForwardClose_Click(null, null);
         return result;
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
         SetIndexButtonEnables();
      }

      #endregion

   }
}
