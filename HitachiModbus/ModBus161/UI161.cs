using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Modbus_DLL;

namespace ModBus161 {
   public partial class UI161 : Form {

      #region Data Declarations

      ResizeInfo R;
      bool initComplete = false;

      Properties.Settings prop = Properties.Settings.Default;

      // Nozzle selection for Twin-Nozzle printers
      public enum Nozzle {
         Printer = 0,
         Nozzle1 = 1,
         Nozzle2 = 2,
         Both = 3,
      }

      // Single instance of the printer
      private Modbus p;

      // User Pattern
      UserPattern up;

      // Modbus data to send to each nozzle
      string modbusTextN1 = string.Empty;
      string modbusTextN2 = string.Empty;

      // Used to manage dropdowns
      private string[] ccNames;
      private string[] ccNamesSorted;
      private int[] ccValues;

      private string[] attrNames;
      private string[] attrNamesSorted;
      private int[] attValues;

      private AttrData attr;

      // Remote Operations
      private enum RemoteOps {
         Start = 0,
         Stop = 1,
         Ready = 2,
         StandBy = 3,
         ClearFault = 4,
      }



      #endregion

      #region Application data

      TwinApp twinApp;

      #endregion

      #region Constructors an destructors

      // Constructor
      public UI161() {
         InitializeComponent();

         // Instantiate Modbus printer and register for log events
         p = new Modbus();
         p.Log += Modbus_Log;

         // Instantiate the user pattern
         up = new UserPattern(this, p, tabLogo);
         up.Log += Modbus_Log;

      }

      #endregion

      #region Form Level Events

      private void UI161_Load(object sender, EventArgs e) {
         // Get persistant data
         txtIPAddress.Text = prop.IPAddress;
         txtIPPort.Text = prop.IPPort;
         txtMessageFolder.Text = prop.MessageFolder;
         txtDataAddress.Text = prop.HexAddress;
         txtDataLength.Text = prop.Length;
         optHoldingRegister.Checked = prop.HoldingReg;
         chkTwinNozzle.Checked = prop.TwinNozzle;
         cbNozzle.SelectedIndex = prop.Nozzle;
         chkHex.Checked = prop.HexData;
         chkLogIO.Checked = prop.LogIO;
         chkStopOnAllErrors.Checked = prop.StopOnErrors;

         // Initialize all dropdowns
         ccNames = Enum.GetNames(typeof(ClassCode));
         ccNamesSorted = Enum.GetNames(typeof(ClassCode));
         Array.Sort(ccNamesSorted);
         ccValues = (int[])Enum.GetValues(typeof(ClassCode));
         cbClass.Items.AddRange(ccNamesSorted);

         // Initilize the Twin Nozzle Application
         txtAppExcel.Text = prop.AppSpreadsheet;
         if (File.Exists(txtAppExcel.Text)) {
            cmdAppStart_Click(null, null);
            cbAppSpreadsheet.SelectedIndex = prop.AppWorksheet;
            cbAppPrimaryKey.SelectedIndex = prop.AppPrimaryKey;
            cbAppTemplate.SelectedIndex = prop.AppTemplate;
            cbAppMsgSource.SelectedIndex = prop.AppSrc;
            cbAppMsgDestination.SelectedIndex = prop.AppDst;
         }
         p.LogIOs = chkLogIO.Checked;
         p.StopOnAllErrors = chkStopOnAllErrors.Checked;

         cbMessageNumber.Items.Clear();
         for (int i = 1; i <= 48; i++) {
            cbMessageNumber.Items.Add(i.ToString());
         }

         // Ready to go
         initComplete = true;

         // Center the form on the screen
         Utils.PositionForm(this, 0.6f, 0.9f);

         SetButtonEnables();
      }

      private void UI161_FormClosing(object sender, FormClosingEventArgs e) {
         prop.IPAddress = txtIPAddress.Text;
         prop.IPPort = txtIPPort.Text;
         prop.MessageFolder = txtMessageFolder.Text;
         prop.HexAddress = txtDataAddress.Text;
         prop.Length = txtDataLength.Text;
         prop.HoldingReg = optHoldingRegister.Checked;
         prop.TwinNozzle = chkTwinNozzle.Checked;
         prop.Nozzle = cbNozzle.SelectedIndex;
         prop.HexData = chkHex.Checked;
         prop.LogIO = chkLogIO.Checked;
         prop.StopOnErrors = chkStopOnAllErrors.Checked;
         prop.AppSpreadsheet = txtAppExcel.Text;
         prop.AppWorksheet = cbAppSpreadsheet.SelectedIndex;
         prop.AppPrimaryKey = cbAppPrimaryKey.SelectedIndex;
         prop.AppTemplate = cbAppTemplate.SelectedIndex;
         prop.AppWorksheet = cbAppSpreadsheet.SelectedIndex;
         prop.AppPrimaryKey = cbAppPrimaryKey.SelectedIndex;
         prop.AppTemplate = cbAppTemplate.SelectedIndex;
         prop.AppSrc = cbAppMsgSource.SelectedIndex;
         prop.AppDst = cbAppMsgDestination.SelectedIndex;
         prop.Save();
      }

      private void UI161_Resize(object sender, EventArgs e) {
         //
         // Avoid resize before Program Load has run or on screen minimize
         if (initComplete && ClientRectangle.Height > 0) {
            //
            this.SuspendLayout();
            // Build local parameters
            R = Utils.InitializeResize(this, 49, 47, true);

            Utils.ResizeObject(ref R, lblMessageFolder, 1, 1, 2, 6);
            Utils.ResizeObject(ref R, txtMessageFolder, 1, 7, 2, 33);
            Utils.ResizeObject(ref R, cmdBrowse, 1, 41, 2.5f, 5);

            Utils.ResizeObject(ref R, lblIPAddress, 4, 1, 2, 5);
            Utils.ResizeObject(ref R, txtIPAddress, 4, 6, 2, 5);
            Utils.ResizeObject(ref R, lblIPPort, 7, 1, 2, 5);
            Utils.ResizeObject(ref R, txtIPPort, 7, 6, 2, 5);

            Utils.ResizeObject(ref R, cmdConnect, 4, 12, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdDisconnect, 7, 12, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdComOn, 4, 19, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdComOff, 7, 19, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdReady, 4, 26, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdStandby, 7, 26, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdStartUp, 4, 33, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdShutDown, 7, 33, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdGetStatus, 4, 40, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdReset, 7, 40, 2.5f, 6);

            Utils.ResizeObject(ref R, chkLogIO, 9, 1, 2, 5);
            Utils.ResizeObject(ref R, chkStopOnAllErrors, 11, 1, 2, 7);
            Utils.ResizeObject(ref R, chkTwinNozzle, 9, 6, 2, 6);
            Utils.ResizeObject(ref R, lblPrinterStatus, 10, 14, 2, 6);
            Utils.ResizeObject(ref R, txtPrinterStatus, 10, 20, 2, 26);
            Utils.ResizeObject(ref R, lblAnalysis, 12, 14, 2, 6);
            Utils.ResizeObject(ref R, txtAnalysis, 12, 20, 2, 26);

            Utils.ResizeObject(ref R, tclViews, 14, 1, 23, 45);
            {
               Utils.ResizeObject(ref R, dgMessages, 1, 1, 16, 43);
               {
                  Utils.ResizeObject(ref R, lblMessageName, 17.5f, 1, 2, 5);
                  Utils.ResizeObject(ref R, txtMessageName, 17.5f, 6, 2, 10);
                  Utils.ResizeObject(ref R, lblMessageNumber, 19.5f, 1, 2, 5);
                  Utils.ResizeObject(ref R, cbMessageNumber, 19.5f, 6, 2, 5);

                  Utils.ResizeObject(ref R, cmdMessageAdd, 18, 21, 2.5f, 5);
                  Utils.ResizeObject(ref R, cmdMessageDelete, 18, 27, 2.5f, 5);
                  Utils.ResizeObject(ref R, cmdMessageRefresh, 18, 33, 2.5f, 5);
                  Utils.ResizeObject(ref R, cmdMessageLoad, 18, 39, 2.5f, 5);
               }
               Utils.ResizeObject(ref R, dgGroups, 1, 1, 17, 43);
               {
                  Utils.ResizeObject(ref R, cmdGroupRefresh, 18, 38, 2.5f, 6);
               }
               Utils.ResizeObject(ref R, tvXML, 1, 1, 19, 43);
               Utils.ResizeObject(ref R, txtIndentedView, 1, 1, 19, 43);
               Utils.ResizeObject(ref R, lbErrors, 1, 1, 17, 43);
               {
                  Utils.ResizeObject(ref R, cmdErrorRefresh, 18, 31, 2.5f, 6);
                  Utils.ResizeObject(ref R, cmdErrorClear, 18, 38, 2.5f, 6);
               }
               Utils.ResizeObject(ref R, lstMessages, 1, 1, 19, 43);
               // Application Tab
               {
                  Utils.ResizeObject(ref R, lblAppExcel, 1, 1, 2, 6);
                  Utils.ResizeObject(ref R, txtAppExcel, 1, 7, 2, 30);
                  Utils.ResizeObject(ref R, cmdAppBrowse, 0.5f, 38, 2, 6);

                  Utils.ResizeObject(ref R, lblAppN1Readable, 3.5f, 1, 2, 8);
                  Utils.ResizeObject(ref R, txtAppN1Readable, 3.5f, 9, 2, 21);
                  Utils.ResizeObject(ref R, lblAppN1Modbus, 6, 1, 2, 8);
                  Utils.ResizeObject(ref R, txtAppN1Modbus, 6, 9, 2, 21);
                  Utils.ResizeObject(ref R, lblAppN2Readable, 8.5f, 1, 2, 8);
                  Utils.ResizeObject(ref R, txtAppN2Readable, 8.5f, 9, 2, 21);
                  Utils.ResizeObject(ref R, lblAppN2Modbus, 11, 1, 2, 8);
                  Utils.ResizeObject(ref R, txtAppN2Modbus, 11, 9, 2, 21);

                  Utils.ResizeObject(ref R, lblAppParts, 16, 1, 2, 8);
                  Utils.ResizeObject(ref R, cbAppParts, 16, 9, 2, 6);
                  Utils.ResizeObject(ref R, lblAppAuxA, 16, 15, 2, 6);
                  Utils.ResizeObject(ref R, txtAppAuxA, 16, 21, 2, 6);
                  Utils.ResizeObject(ref R, lblAppAuxB, 19, 15, 2, 6);
                  Utils.ResizeObject(ref R, txtAppAuxB, 19, 21, 2, 6);

                  Utils.ResizeObject(ref R, lblAppSpreadsheet, 3.5f, 32, 2, 6);
                  Utils.ResizeObject(ref R, cbAppSpreadsheet, 3.5f, 38, 2, 6);
                  Utils.ResizeObject(ref R, lblAppPrimaryKey, 6, 32, 2, 6);
                  Utils.ResizeObject(ref R, cbAppPrimaryKey, 6, 38, 2, 6);
                  Utils.ResizeObject(ref R, lblAppTemplate, 8.5f, 32, 2, 6);
                  Utils.ResizeObject(ref R, cbAppTemplate, 8.5f, 38, 2, 6);
                  Utils.ResizeObject(ref R, lblAppMsgSource, 11, 32, 2, 6);
                  Utils.ResizeObject(ref R, cbAppMsgSource, 11, 38, 2, 6);
                  Utils.ResizeObject(ref R, lblAppMsgDestination, 13.5f, 32, 2, 6);
                  Utils.ResizeObject(ref R, cbAppMsgDestination, 13.5f, 38, 2, 6);

                  Utils.ResizeObject(ref R, cmdAppStart, 16, 31, 2, 6);
                  Utils.ResizeObject(ref R, cmdAppQuit, 18.5f, 31, 2, 6);
                  Utils.ResizeObject(ref R, cmdAppRefresh, 16, 38, 2, 6);
                  Utils.ResizeObject(ref R, cmdAppToPrinter, 18.5f, 38, 2, 6);
               }
               // Logo Tab
               up.ResizeControls(ref R, 0, 20, 44);

            }

            Utils.ResizeObject(ref R, lblClass, 38, 1, 2, 5);
            Utils.ResizeObject(ref R, cbClass, 38, 6, 2, 5);
            Utils.ResizeObject(ref R, lblAttribute, 40, 1, 2, 5);
            Utils.ResizeObject(ref R, cbAttribute, 40, 6, 2, 5);
            Utils.ResizeObject(ref R, lblInstance, 42, 1, 2, 5);
            Utils.ResizeObject(ref R, cbInstance, 42, 6, 2, 5);

            Utils.ResizeObject(ref R, lblDataAddress, 38, 12, 2, 6);
            Utils.ResizeObject(ref R, txtDataAddress, 38, 18, 2, 6);
            Utils.ResizeObject(ref R, lblNozzle, 40, 12, 2, 6);
            Utils.ResizeObject(ref R, cbNozzle, 40, 18, 2, 6);
            Utils.ResizeObject(ref R, lblDataLength, 42, 12, 2, 6);
            Utils.ResizeObject(ref R, txtDataLength, 42, 18, 2, 6);
            Utils.ResizeObject(ref R, lblData, 44, 12, 2, 6);
            Utils.ResizeObject(ref R, txtData, 44, 18, 2, 12);

            Utils.ResizeObject(ref R, optHoldingRegister, 38, 25, 2, 6);
            Utils.ResizeObject(ref R, optInputRegister, 40, 25, 2, 6);
            Utils.ResizeObject(ref R, chkHex, 42, 25, 2, 6);

            Utils.ResizeObject(ref R, cmdReadData, 38, 31, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdWriteData, 42, 31, 2.5f, 6);

            Utils.ResizeObject(ref R, cmdRetrieve, 46, 1, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdSaveAs, 46, 7, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdOpen, 46, 13, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdSend, 46, 19, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdReformat, 46, 25, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdExperiment, 46, 31, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdExit, 46, 41, 2.5f, 5);

            //this.Refresh();
            this.ResumeLayout();

         }
      }

      #endregion

      #region Form Control Events

      // Connect to printer and turn COM on
      private void cmdConnect_Click(object sender, EventArgs e) {
         p.TwinNozzle = chkTwinNozzle.Checked;
         if (p.Connect(txtIPAddress.Text, txtIPPort.Text)) {
            cmdGetStatus_Click(null, null);
         }
         SetButtonEnables();
      }

      // Disconnect from the printer
      private void cmdDisconnect_Click(object sender, EventArgs e) {
         p.Disconnect();
         txtPrinterStatus.Text = "Unknown";
         txtAnalysis.Text = "Unknown";
         SetButtonEnables();
      }

      // Turn com on
      private void cmdComOn_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Online_Offline, 1);
         cmdGetStatus_Click(null, null);
         SetButtonEnables();
      }

      // Turn com off
      private void cmdComOff_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Online_Offline, 0);
         cmdGetStatus_Click(null, null);
         SetButtonEnables();
      }

      // Reset alarm
      private void cmdReset_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.ClearFault);
         cmdGetStatus_Click(null, null);
         SetButtonEnables();
      }

      // Get printer status
      private void cmdGetStatus_Click(object sender, EventArgs e) {
         string comm = Status.TranslateStatus(Status.StatusAreas.Connection, p.GetDecAttribute(ccUS.Communication_Status));
         string receive = Status.TranslateStatus(Status.StatusAreas.Reception, p.GetDecAttribute(ccUS.Receive_Status));
         string operation = Status.TranslateStatus(Status.StatusAreas.Operation, p.GetDecAttribute(ccUS.Operation_Status));
         string warn = Status.TranslateStatus(Status.StatusAreas.Warning, p.GetDecAttribute(ccUS.Warning_Status));
         string a1 = Status.TranslateStatus(Status.StatusAreas.Analysis1, p.GetDecAttribute(ccUS.Analysis_Info_1));
         string a2 = Status.TranslateStatus(Status.StatusAreas.Analysis2, p.GetDecAttribute(ccUS.Analysis_Info_2));
         string a3 = Status.TranslateStatus(Status.StatusAreas.Analysis3, p.GetDecAttribute(ccUS.Analysis_Info_3));
         string a4 = Status.TranslateStatus(Status.StatusAreas.Analysis4, p.GetDecAttribute(ccUS.Analysis_Info_4));
         txtPrinterStatus.Text = $"{comm}/{receive}/{operation}/{warn}";
         txtAnalysis.Text = $"{a1}/{a2}/{a3}/{a4}";
         SetButtonEnables();
      }

      // Hydralic pump shutdown
      private void cmdShutDown_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.Stop);
         SetButtonEnables();
      }

      // Hydralic pump startup
      private void cmdStartUp_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.Start);
         SetButtonEnables();
      }

      // Printer to standby
      private void cmdStandby_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.StandBy);
         SetButtonEnables();
      }

      // Printer to ready
      private void cmdReady_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.Ready);
         SetButtonEnables();
      }

      // Read data from the printer
      private void cmdReadData_Click(object sender, EventArgs e) {
         if (int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out int addr)
            && int.TryParse(txtDataLength.Text, out int len)) {
            Modbus.FunctionCode fc = optHoldingRegister.Checked ? Modbus.FunctionCode.ReadHolding : Modbus.FunctionCode.ReadInput;
            byte devAddr = GetDevAddr();
            p.GetAttribute(fc, devAddr, addr, len, out byte[] data);
            txtData.Text = p.byte_to_string(data);
         }
         SetButtonEnables();
      }

      // Send data to the printer
      private void cmdWriteData_Click(object sender, EventArgs e) {
         if (int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out int addr)
            && int.TryParse(txtDataLength.Text, out int len)
            && txtData.Text.Length > 0) {
            byte devAddr = GetDevAddr();
            byte[] data;
            if (chkHex.Checked) {
               data = p.string_to_byte(txtData.Text);
            } else {
               data = new byte[len];
               if (int.TryParse(txtData.Text, out int n)) {
                  for (int i = len; i > 0; i--) {
                     data[i - 1] = (byte)n;
                     n >>= 8;
                  }
               }
            }
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(devAddr, addr, data);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }
         SetButtonEnables();
      }

      // Send an XML message to the printer
      private void cmdSend_Click(object sender, EventArgs e) {
         SendRetrieveXML send = new SendRetrieveXML(this, p);
         send.SendXML(txtIndentedView.Text);
         SetButtonEnables();
      }

      // Retrieve message from printer and convert to XML
      private void cmdRetrieve_Click(object sender, EventArgs e) {
         SendRetrieveXML retrieve = new SendRetrieveXML(this, p);
         LoadXmlToDisplay(retrieve.Retrieve());
         SetButtonEnables();
      }

      // Exit the program
      private void cmdExit_Click(object sender, EventArgs e) {
         this.Close();
      }

      // Browse for a new message folder
      private void cmdBrowse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = txtMessageFolder.Text };
         if (dlg.ShowDialog() == DialogResult.OK) {
            txtMessageFolder.Text = dlg.SelectedPath;
         }
         SetButtonEnables();
      }

      // Save indented view as am HML file
      private void cmdSaveAs_Click(object sender, EventArgs e) {
         string fileName = "XMLIndented.HML";
         string fileText = txtIndentedView.Text;
         using (SaveFileDialog sfd = new SaveFileDialog()) {
            sfd.DefaultExt = "hml";
            sfd.Filter = "HML|*.hml";
            sfd.Title = "Save Printer Image to HML file";
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.InitialDirectory = txtMessageFolder.Text;
            sfd.FileName = fileName;
            if (sfd.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty(sfd.FileName)) {
               fileName = Path.Combine(txtMessageFolder.Text, sfd.FileName);
               File.WriteAllText(fileName, fileText);
            }
         }
         SetButtonEnables();
      }

      // Open an HML file for processing
      private void cmdOpen_Click(object sender, EventArgs e) {
         // Clear out any currently loaded file
         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.Title = "Select HML formatted file!";
            dlg.Filter = "HML (*.hml)|*.hml|All (*.*)|*.*";
            DialogResult dlgResult = DialogResult.Retry;
            while (dlgResult == DialogResult.Retry) {
               dlgResult = dlg.ShowDialog();
               if (dlgResult == DialogResult.OK) {
                  try {
                     LoadXmlToDisplay(File.ReadAllText(dlg.FileName));
                     tclViews.SelectedTab = tabIndented;
                  } catch (Exception ex) {
                     MessageBox.Show(this, ex.Message, "Cannot load HML File!");
                  }
               }
            }
         }
         SetButtonEnables();
      }

      // Reformat the main data table after major changes.
      private void cmdReformat_Click(object sender, EventArgs e) {

         string RFN = @"c:\temp\Reformat.txt";
         StreamWriter RFS = new StreamWriter(RFN, false, Encoding.UTF8);

         Modbus.M161.ReformatTables(RFS);

         RFS.Flush();
         RFS.Close();
         Process.Start("notepad.exe", RFN);
         SetButtonEnables();
      }

      // Clear the task log
      private void cmLogClear_Click(object sender, EventArgs e) {
         if (tclViews.SelectedTab == tabLog) {
            lstMessages.Items.Clear();
         } else if (tclViews.SelectedTab == tabIndented) {
            txtIndentedView.Text = string.Empty;
            tvXML.Nodes.Clear();
         }
         SetButtonEnables();
      }

      // View the task log in NotePad
      private void cmLogToNotepad_Click(object sender, EventArgs e) {
         string ViewFilename = @"c:\Temp\Err.txt";
         File.WriteAllLines(ViewFilename, lstMessages.Items.Cast<string>().ToArray());
         Process.Start("notepad.exe", ViewFilename);
         SetButtonEnables();
      }

      // Class selection changed
      private void cbClass_SelectedIndexChanged(object sender, EventArgs e) {
         cbAttribute.Items.Clear();
         cbInstance.Items.Clear();
         if (cbClass.SelectedIndex >= 0) {
            int n = Array.FindIndex(ccNames, x => x == cbClass.Text);
            Type cc = p.ClassCodeAttributes[n];
            attrNames = Enum.GetNames(p.ClassCodeAttributes[n]);
            attrNamesSorted = Enum.GetNames(p.ClassCodeAttributes[n]);
            //Array.Sort(attrNamesSorted);
            cbAttribute.Items.AddRange(attrNamesSorted);
         }

      }

      // Attribute selection changed
      private void cbAttribute_SelectedIndexChanged(object sender, EventArgs e) {
         cbInstance.Items.Clear();
         if (cbAttribute.SelectedIndex >= 0) {
            int n1 = Array.FindIndex(ccNames, x => x == cbClass.Text);
            attValues = (int[])Enum.GetValues(p.ClassCodeAttributes[n1]);
            int n2 = Array.FindIndex(attrNames, x => x == cbAttribute.Text);
            attr = p.GetAttrData(p.ClassCodes[n1], attValues[n2]);
            if (attr.HoldingReg) {
               optHoldingRegister.Checked = true;
            } else {
               optInputRegister.Checked = true;
            }
            switch (attr.Nozzle) {
               case Noz.None:
                  cbNozzle.SelectedIndex = 0;
                  break;
               case Noz.Current:
                  cbNozzle.SelectedIndex = p.Nozzle + 1;
                  break;
               case Noz.Both:
                  cbNozzle.SelectedIndex = 3;
                  break;
               default:
                  break;
            }
            int n = attr.Count;
            for (int i = 0; i < n; i++) {
               cbInstance.Items.Add(i);
            }
            cbInstance.SelectedIndex = 0;
         }
      }

      // Instance selection changed
      private void cbInstance_SelectedIndexChanged(object sender, EventArgs e) {
         if (cbInstance.SelectedIndex >= 0) {
            txtDataAddress.Text = (attr.Val + cbInstance.SelectedIndex * attr.Stride).ToString("X4");
            txtDataLength.Text = attr.Data.Len.ToString();
            txtData.Text = "";
         }
      }

      // Just playing around to see how things work
      private void cmdExperiment_Click(object sender, EventArgs e) {
         p.DeleteMessage(5);
         p.SetAttribute(ccPDR.Recall_Message, 6);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccPDR.MessageName, "TWIN MSG 3  ");
         p.SetAttribute(ccPDR.Message_Number, 5);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         SetButtonEnables();
      }

      // Show I/O packets in Log File.
      private void chkLogIO_CheckedChanged(object sender, EventArgs e) {
         p.LogIOs = chkLogIO.Checked;
      }

      // Retrieve the error log from the printer
      private void cmdErrorRefresh_Click(object sender, EventArgs e) {
         lbErrors.Items.Clear();
         int errCount = p.GetDecAttribute(ccAH.Message_Count);
         lbErrors.Items.Add($"There are {errCount} errors to report!");
         for (int i = 0; i < errCount; i++) {
            int year = p.GetDecAttribute(ccAH.Year, i);
            int month = p.GetDecAttribute(ccAH.Month, i);
            int day = p.GetDecAttribute(ccAH.Day, i);
            int hour = p.GetDecAttribute(ccAH.Hour, i);
            int minute = p.GetDecAttribute(ccAH.Minute, i);
            int second = p.GetDecAttribute(ccAH.Second, i);
            int fault = p.GetDecAttribute(ccAH.Fault_Number, i);
            lbErrors.Items.Add($"{fault:###} {year}/{month:##}/{day:##} {hour:##}:{minute:##}:{second:##}");
            lbErrors.Update();
         }
      }

      // Add a message to the printer's directory
      private void cmdMessageAdd_Click(object sender, EventArgs e) {
         int msgNumber = int.Parse(cbMessageNumber.Text);
         string msgName = txtMessageName.Text.PadRight(12);
         p.DeleteMessage(msgNumber);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccPDR.MessageName, msgName);
         p.SetAttribute(ccPDR.Message_Number, msgNumber);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         SetButtonEnables();
      }

      // Delete a message from the printer's directory
      private void cmdMessageDelete_Click(object sender, EventArgs e) {
         int msgNumber = int.Parse((string)dgMessages.SelectedRows[0].Cells["colMessage"].Value);
         p.DeleteMessage(msgNumber);
         dgMessages.Rows.Remove(dgMessages.SelectedRows[0]);
         SetButtonEnables();
      }

      // Get all messages from the printer and display them in a data view
      private void cmdMessageRefresh_Click(object sender, EventArgs e) {
         string[] s = new string[3];
         dgMessages.Rows.Clear();
         // For now, look at the first 48 only.  Need to implement block read
         AttrData attrCount = p.GetAttrData(ccMM.Registration);
         for (int i = 0; i < Math.Min(3, attrCount.Count); i++) {
            int reg = p.GetDecAttribute(ccMM.Registration, i);
            for (int j = 15; j >= 0; j--) {
               if ((reg & (1 << j)) > 0) {
                  int n = i * 16 - j + 15; // 1-origin
                  p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
                  p.SetAttribute(ccIDX.Message_Number, n + 1);         // Load the message into input registers
                  p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
                  s[0] = p.GetHRAttribute(ccMM.Group_Number);
                  s[1] = p.GetHRAttribute(ccMM.Message_Number);
                  s[2] = p.GetHRAttribute(ccMM.Message_Name);
                  dgMessages.Rows.Add(s);
                  dgMessages.Update();
               }
            }
         }
         SetButtonEnables();
      }

      // Recall a message that has beed stored in the printer
      private void cmdMessageLoad_Click(object sender, EventArgs e) {
         if (int.TryParse((string)dgMessages.SelectedRows[0].Cells[1].Value, out int n)) {
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccPDR.Recall_Message, n);                   // Load the message into input registers
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            SetButtonEnables();
         }
      }

      // Re-evaluate enables when leaving
      private void Data_Leave(object sender, EventArgs e) {
         SetButtonEnables();
      }

      #endregion

      #region Twin Nozzle Application Events

      // Find the excel spreadsheet associated with the application
      private void cmdAppBrowse_Click(object sender, EventArgs e) {
         txtAppExcel.Text = GetExcelFile(txtAppExcel.Text);
         SetButtonEnables();
      }

      // Start the Twin Nozzle Application
      private void cmdAppStart_Click(object sender, EventArgs e) {
         twinApp = new TwinApp();
         cbAppSpreadsheet.Items.Clear();
         cbAppPrimaryKey.Items.Clear();
         cbAppTemplate.Items.Clear();
         if (twinApp.Open(txtAppExcel.Text)) {
            cbAppSpreadsheet.Items.AddRange(twinApp.workSheets.ToArray());
         }
         SetButtonEnables();
      }

      // The selected spreadsheet changed, reload the primary keys and templates
      private void cbAppSpreadsheet_SelectedIndexChanged(object sender, EventArgs e) {
         cbAppPrimaryKey.Items.Clear();
         cbAppTemplate.Items.Clear();
         if (cbAppSpreadsheet.SelectedIndex >= 0) {
            string[] keys = twinApp.workSheetVariables[cbAppSpreadsheet.SelectedIndex];
            cbAppPrimaryKey.Items.AddRange(keys);
            cbAppTemplate.Items.AddRange(keys);
         }
         SetButtonEnables();
      }

      // The primary key changed, reload the parts list
      private void cbAppPrimaryKey_SelectedIndexChanged(object sender, EventArgs e) {
         cbAppParts.Items.Clear();
         if (cbAppPrimaryKey.SelectedIndex >= 0) {
            cbAppParts.Items.AddRange(twinApp.PartNumbers(cbAppSpreadsheet.Text, cbAppPrimaryKey.Text));
         }
         SetButtonEnables();
      }

      // resolve the templates
      private void cbAppParts_SelectedIndexChanged(object sender, EventArgs e) {
         if (cbAppParts.SelectedIndex >= 0) {
            if (twinApp.GetDataRow(cbAppSpreadsheet.Text, cbAppPrimaryKey.Text, cbAppParts.Text)) {
               int n = twinApp.CurrentEdbRow.Table.Columns[cbAppTemplate.Text].Ordinal;

               txtAppN1Readable.Text = twinApp.ResolveReferences(twinApp.GetData(n));
               modbusTextN1 = p.HandleBraces(txtAppN1Readable.Text);
               txtAppN1Modbus.Text = Readable(modbusTextN1);

               txtAppN2Readable.Text = twinApp.ResolveReferences(twinApp.GetData(n + 1));
               modbusTextN2 = p.HandleBraces(txtAppN2Readable.Text);
               txtAppN2Modbus.Text = Readable(modbusTextN2);
            }
         }

      }

      // Force template re-evaluation
      private void cmdAppRefresh_Click(object sender, EventArgs e) {
         cbAppParts_SelectedIndexChanged(null, null);
      }

      // All done.  Close it out
      private void cmdAppQuit_Click(object sender, EventArgs e) {
         twinApp.Close();
         twinApp = null;
      }

      // Clean up the current mesage and load new text
      private void cmdAppToPrinter_Click(object sender, EventArgs e) {
         // Cleanup the current display
         p.DeleteMessage(cbAppMsgDestination.SelectedIndex + 1);
         p.SetAttribute(ccPDR.Recall_Message, cbAppMsgSource.SelectedIndex + 1);
         if (modbusTextN1.Length > 0) {
            p.Nozzle = 0;
            p.DeleteAllButOne();
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccPC.Characters_per_Item, 0, modbusTextN1.Length);
            p.SetAttribute(ccPC.Print_Character_String, 0, modbusTextN1);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccPDR.MessageName, "TWIN MSG 3  ");
         p.SetAttribute(ccPDR.Message_Number, cbAppMsgDestination.SelectedIndex + 1);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         SetButtonEnables();


         //p.DeleteAllButOne();
         //p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         //p.SetAttribute(ccPC.Characters_per_Item, index, s.Length);
         //p.SetAttribute(ccPC.Print_Character_String, charPosition, s);
         //p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
      }

      // Re-evaluate enables if selection changes
      private void cbAppMsgSource_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // Re-evaluate enables if selection changes
      private void cbAppMsgDestination_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      #endregion

      #region Service Routines

      // Convert an XML Document into an indented text string
      private string ToIndentedString(string unformattedXml) {
         string result;
         XmlReaderSettings readeroptions = new XmlReaderSettings { IgnoreWhitespace = true };
         XmlReader reader = XmlReader.Create(new StringReader(unformattedXml), readeroptions);
         StringBuilder sb = new StringBuilder();
         XmlWriterSettings xmlSettingsWithIndentation = new XmlWriterSettings { Indent = true };
         using (XmlWriter writer = XmlWriter.Create(sb, xmlSettingsWithIndentation)) {
            writer.WriteNode(reader, true);
         }
         result = sb.ToString();
         return result;
      }

      // Add a node to the tree view
      private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode) {
         if (inXmlNode is XmlWhitespace)
            return;
         XmlNode xNode;
         XmlNodeList nodeList;
         if (inXmlNode.HasChildNodes) {
            inTreeNode.Text = GetNameAttr(inXmlNode);
            nodeList = inXmlNode.ChildNodes;
            int j = 0;
            for (int i = 0; i < nodeList.Count; i++) {
               xNode = inXmlNode.ChildNodes[i];
               if (xNode is XmlWhitespace)
                  continue;
               if (xNode.Name == "#text") {
                  inTreeNode.Text = inXmlNode.OuterXml.Trim();
               } else {
                  if (!(xNode is XmlWhitespace)) {
                     inTreeNode.Nodes.Add(new TreeNode(GetNameAttr(xNode)));
                     AddNode(xNode, inTreeNode.Nodes[j]);
                  }
               }
               j++;
            }
         } else {
            inTreeNode.Text = inXmlNode.OuterXml.Trim();
         }
      }

      // Get the attributes associated with a node
      private string GetNameAttr(XmlNode n) {
         string result = n.Name;
         if (n.Attributes != null && n.Attributes.Count > 0) {
            foreach (XmlAttribute attribute in n.Attributes) {
               result += $" {attribute.Name}=\"{attribute.Value}\"";
            }
         }
         return result;
      }

      // Enter a message into the log file display
      public void Log(string msg) {
         while (lstMessages.Items.Count > 1000) {
            lstMessages.Items.RemoveAt(0);
         }
         lstMessages.Items.Add(Readable(msg));
         lstMessages.SelectedIndex = lstMessages.Items.Count - 1;
         lstMessages.Update();
      }

      // Log messages generated by modbus
      private void Modbus_Log(object sender, string msg) {
         Log(msg);
      }

      // Load an XML file into the displays
      private void LoadXmlToDisplay(string xml) {
         try {
            // Can be called with a Filename or XML text
            int xmlStart = xml.IndexOf("<Label");
            if (xmlStart == -1) {
               xml = File.ReadAllText(xml);
               xmlStart = xml.IndexOf("<Label");
            }
            // No label found, exit
            if (xmlStart == -1) {
               return;
            }
            int xmlEnd = xml.IndexOf("</Label>", xmlStart + 7);
            if (xmlEnd > 0) {
               xml = xml.Substring(xmlStart, xmlEnd - xmlStart + 8);
               XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = true };
               xmlDoc.LoadXml(xml);
               xml = ToIndentedString(xml);
               xmlStart = xml.IndexOf("<Label");
               if (xmlStart > 0) {
                  xml = xml.Substring(xmlStart);
                  txtIndentedView.Text = xml;

                  tvXML.Nodes.Clear();
                  tvXML.Nodes.Add(new TreeNode(xmlDoc.DocumentElement.Name));
                  TreeNode tNode = new TreeNode();
                  tNode = tvXML.Nodes[0];

                  AddNode(xmlDoc.DocumentElement, tNode);
                  tvXML.ExpandAll();

               }
            }
         } catch {

         }
      }

      // Get device address to use
      private byte GetDevAddr() {
         byte devAddr = 0;
         if (chkTwinNozzle.Checked) {
            switch ((Nozzle)cbNozzle.SelectedIndex) {
               case Nozzle.Printer:
                  devAddr = 1;
                  break;
               case Nozzle.Nozzle1:
                  devAddr = 1;
                  break;
               case Nozzle.Nozzle2:
                  devAddr = 2;
                  break;
               case Nozzle.Both:
                  devAddr = 3;
                  break;
               default:
                  break;
            }
         }
         return devAddr;
      }

      // Get the Excel spreadsheet with part numbers and print specs
      private string GetExcelFile(string fileName) {
         string result = fileName;
         using (OpenFileDialog dlg = new OpenFileDialog() { CheckFileExists = true, CheckPathExists = true, Multiselect = false, ValidateNames = true }) {
            if (File.Exists(fileName)) {
               dlg.InitialDirectory = Path.GetDirectoryName(fileName);
            }
            dlg.Title = "Part List Spread Sheet";
            dlg.Filter = "Parts File|*.xlsx;*.xlsm;*.xlsb;*.xls";
            if (dlg.ShowDialog(this) == DialogResult.OK) {
               result = dlg.FileName;
            }
         }
         return result;
      }

      // Make string readable
      private string Readable(string msg) {
         string s = "";
         for (int i = 0; i < msg.Length; i++) {
            char c = msg[i];
            if (c >= 0x100) {
               s += $"<{c >> 8:X2}><{c & 0xFF:X2}>";
            } else {
               s += msg.Substring(i, 1);
            }
         }
         return s;
      }

      // Avoid extra tests by enabling only the buttons that can be used
      private void SetButtonEnables() {
         int addr;
         int len;
         bool isConnected = p == null ? false : p.IsConnected;
         bool comIsOn = isConnected && p.ComIsOn;
         bool appIsOpen = twinApp != null;

         cmdConnect.Enabled = !isConnected;
         cmdDisconnect.Enabled = isConnected;
         cmdComOff.Enabled = comIsOn;
         cmdComOn.Enabled = isConnected && !comIsOn;
         cmdStartUp.Enabled = comIsOn;
         cmdShutDown.Enabled = comIsOn;
         cmdReady.Enabled = comIsOn;
         cmdStandby.Enabled = comIsOn;
         cmdGetStatus.Enabled = isConnected;
         cmdReset.Enabled = comIsOn;

         cmdReadData.Enabled = comIsOn
            && int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out addr)
            && int.TryParse(txtDataLength.Text, out len);
         cmdWriteData.Enabled = comIsOn
            && int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out addr)
            && int.TryParse(txtDataLength.Text, out len)
            && txtData.Text.Length > 0;

         cmdRetrieve.Enabled = comIsOn;
         cmdSaveAs.Enabled = txtIndentedView.Text.Length > 0;
         cmdOpen.Enabled = true; // For now
         cmdSend.Enabled = comIsOn && txtIndentedView.Text.Length > 0;

         cmdExperiment.Enabled = comIsOn;
         chkTwinNozzle.Enabled = !isConnected;

         cmdErrorRefresh.Enabled = false; // comIsOn;
         cmdErrorClear.Enabled = false; // comIsOn;

         cmdGroupRefresh.Enabled = comIsOn;

         cmdMessageAdd.Enabled = comIsOn && cbMessageNumber.SelectedIndex >= 0 && txtMessageName.Text.Length > 0;
         cmdMessageDelete.Enabled = comIsOn && dgMessages.Rows.Count > 0 && dgMessages.SelectedRows.Count == 1;
         cmdMessageRefresh.Enabled = comIsOn;
         cmdMessageLoad.Enabled = comIsOn && dgMessages.Rows.Count > 0 && dgMessages.SelectedRows.Count == 1;
         //cmdMessageAdd.Enabled = false;
         //cmdMessageDelete.Enabled = false;

         cmdAppStart.Enabled = File.Exists(txtAppExcel.Text);
         cmdAppQuit.Enabled = appIsOpen;
         cbAppSpreadsheet.Enabled = appIsOpen;
         cbAppPrimaryKey.Enabled = appIsOpen;
         cbAppTemplate.Enabled = appIsOpen;
         cbAppParts.Enabled = appIsOpen;

         up.SetButtonEnables(comIsOn);
      }

      #endregion

      private void cmdGroupRefresh_Click(object sender, EventArgs e) {

      }
   }

}
