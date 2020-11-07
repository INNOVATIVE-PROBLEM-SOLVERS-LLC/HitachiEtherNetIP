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
using Serialization;

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
      private Modbus MB;

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

      private int acks = 0;
      private int naks = 0;

      string LogXML = string.Empty;

      // Multi-Thread interface
      public AsyncIO asyncIO = null;

      DoSubs doSubs;

      #endregion

      #region Application data

      TwinApp twinApp;

      #endregion

      #region Constructors an destructors

      // Constructor
      public UI161() {
         InitializeComponent();

         // Instantiate Modbus printer and register for log events
         MB = new Modbus(this);
         MB.Log += Modbus_Log;
         MB.Complete += P_Complete;

         // Instantiate the user pattern
         up = new UserPattern(this, MB, tabLogo);
         up.Log += Modbus_Log;

         // Start AsyncIO
         asyncIO = new AsyncIO(this, MB);
         asyncIO.Log += Modbus_Log;
         asyncIO.Complete += AsyncIO_Complete;

         doSubs = new DoSubs(this, MB, grpMain);
         doSubs.Subs = new Substitution[2];
         //if (global.DefaultSubRules != null) {
         //   doSubs.Subs[(int)DoSubs.Src.global] = global.DefaultSubRules.Copy();
         //}
         //if (parent.msg != null && parent.msg.Substitution != null) {
         //   doSubs.Subs[(int)DoSubs.Src.msg] = parent.msg.Substitution.Copy();
         //}
         doSubs.BuildControls(prop);


      }

      private void AsyncIO_Complete(object sender, AsyncComplete status) {
         switch (status.Type) {
            case AsyncIO.TaskType.Connect:
               if (status.Success) {
                  cmdGetStatus_Click(null, null);
               }
               break;
            case AsyncIO.TaskType.Disconnect:
               txtPrinterStatus.Text = "Unknown";
               txtAnalysis.Text = "Unknown";
               break;
            case AsyncIO.TaskType.Send:
               DisplayLogTree(status.Resp2);
               break;
            case AsyncIO.TaskType.Retrieve:
               LoadXmlToDisplay(status.Resp1);
               DisplayLogTree(status.Resp2);
               break;
            case AsyncIO.TaskType.WriteData:

               break;
            case AsyncIO.TaskType.ReadData:
               txtData.Text = MB.byte_to_string(status.DataA);
               break;
            case AsyncIO.TaskType.IssueccIJP:

               break;
            case AsyncIO.TaskType.GetStatus:
               txtPrinterStatus.Text = status.Resp1;
               txtAnalysis.Text = status.Resp2;
               break;
            case AsyncIO.TaskType.GetMessages:
               dgMessages.Rows.Clear();
               foreach(string s in status.MultiLine) {
                  dgMessages.Rows.Add(s.Split(','));
               }
               break;
            case AsyncIO.TaskType.GetErrors:
               lbErrors.Items.Clear();
               lbErrors.Items.Add($"There are {status.Value} errors to report!");
               lbErrors.Items.AddRange(status.MultiLine);
               break;
            case AsyncIO.TaskType.Exit:
               break;
            default:
               break;
         }
         SetButtonEnables();
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
         chkLogAsXML.Checked = prop.LogAsXML;

         // Initialize all dropdowns
         ccNames = Enum.GetNames(typeof(ClassCode));
         ccNamesSorted = Enum.GetNames(typeof(ClassCode));
         Array.Sort(ccNamesSorted);
         ccValues = (int[])Enum.GetValues(typeof(ClassCode));
         cbClass.Items.AddRange(ccNamesSorted);

         MB.LogIO = chkLogIO.Checked;
         MB.LogAllIO = chkLogIO.Checked;
         MB.StopOnAllErrors = chkStopOnAllErrors.Checked;

         cbMessageNumber.Items.Clear();
         for (int i = 1; i <= 48; i++) {
            cbMessageNumber.Items.Add(i.ToString());
         }

         // Ready to go
         initComplete = true;

         // Center the form on the screen
         Utils.PositionForm(this, 0.6f, 0.9f);

         doSubs.DoSubs_Load(sender, e);

         SetButtonEnables();
      }

      private void UI161_FormClosing(object sender, FormClosingEventArgs e) {
         // Shutdown AsyncIO
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.Exit));

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
         prop.LogAsXML = chkLogAsXML.Checked;
         prop.StopOnErrors = chkStopOnAllErrors.Checked;
         prop.GlobalFileName = doSubs.txtGlobalFileName.Text;
         prop.MsgFileName = doSubs.txtMsgFileName.Text;
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
            Utils.ResizeObject(ref R, chkTwinNozzle, 9, 6, 2, 6);
            Utils.ResizeObject(ref R, chkLogAsXML, 11, 1, 2, 5);
            Utils.ResizeObject(ref R, chkStopOnAllErrors, 11, 6, 2, 7);
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
               Utils.ResizeObject(ref R, lstMessages, 1, 1, 19, 41);
               // Logo Tab
               up.ResizeControls(ref R, 0, 20, 44);
               // Log as XML
               Utils.ResizeObject(ref R, tvLogAsXML, 1, 1, 19, 43);

               Utils.ResizeObject(ref R, grpMain, 0, 0, 21, 45);
               doSubs?.ResizeControls(ref R, 2, 1);
            }

            Utils.ResizeObject(ref R, lblClass, 38, 1, 2, 4);
            Utils.ResizeObject(ref R, cbClass, 38, 5, 2, 7);
            Utils.ResizeObject(ref R, lblAttribute, 40, 1, 2, 4);
            Utils.ResizeObject(ref R, cbAttribute, 40, 5, 2, 7);
            Utils.ResizeObject(ref R, lblInstance, 42, 1, 2, 4);
            Utils.ResizeObject(ref R, cbInstance, 42, 5, 2, 7);

            Utils.ResizeObject(ref R, lblDataAddress, 38, 13, 2, 5);
            Utils.ResizeObject(ref R, txtDataAddress, 38, 18, 2, 6);
            Utils.ResizeObject(ref R, lblNozzle, 40, 13, 2, 5);
            Utils.ResizeObject(ref R, cbNozzle, 40, 18, 2, 6);
            Utils.ResizeObject(ref R, lblDataLength, 42, 13, 2, 5);
            Utils.ResizeObject(ref R, txtDataLength, 42, 18, 2, 6);
            Utils.ResizeObject(ref R, lblData, 44, 13, 2, 5);
            Utils.ResizeObject(ref R, txtData, 44, 18, 2, 12);

            Utils.ResizeObject(ref R, optHoldingRegister, 38, 25, 2, 6);
            Utils.ResizeObject(ref R, optInputRegister, 40, 25, 2, 6);
            Utils.ResizeObject(ref R, chkHex, 42, 25, 2, 6);

            Utils.ResizeObject(ref R, cmdReadData, 38, 31, 2.5f, 6);
            Utils.ResizeObject(ref R, cmdWriteData, 42, 31, 2.5f, 6);

            Utils.ResizeObject(ref R, lblAcks, 38, 37, 2, 3);
            Utils.ResizeObject(ref R, txtAcks, 38, 41, 2, 4);
            Utils.ResizeObject(ref R, lblNaks, 40, 37, 2, 3);
            Utils.ResizeObject(ref R, txtNaks, 40, 41, 2, 4);

            Utils.ResizeObject(ref R, cmdRetrieve, 46, 1, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdSaveAs, 46, 7, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdOpen, 46, 13, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdSend, 46, 18, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdReformat, 46, 23, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdExperiment, 46, 29, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdResetIOs, 46, 35, 2.5f, 5);
            Utils.ResizeObject(ref R, cmdExit, 46, 41, 2.5f, 5);

            //this.Refresh();
            this.ResumeLayout();

         }
      }

      #endregion

      #region Form Control Events

      // Stop on I/O or data rejected errors.
      private void chkStopOnAllErrors_CheckedChanged(object sender, EventArgs e) {
         if (MB != null) {
            MB.StopOnAllErrors = chkStopOnAllErrors.Checked;
         }
      }

      // Reset the ACK/NAK counts
      private void cmdResetIOs_Click(object sender, EventArgs e) {
         acks = 0;
         txtAcks.Text = "0";
         naks = 0;
         txtNaks.Text = "0";
      }

      // Connect to printer and turn COM on
      private void cmdConnect_Click(object sender, EventArgs e) {
         MB.TwinNozzle = chkTwinNozzle.Checked;
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.Connect) { IpAddress = txtIPAddress.Text, IpPort = txtIPPort.Text });
      }

      // Disconnect from the printer
      private void cmdDisconnect_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.Disconnect));
      }

      // Turn com on
      private void cmdComOn_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.IssueccIJP, ccIJP.Online_Offline, 1));
      }

      // Turn com off
      private void cmdComOff_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.IssueccIJP, ccIJP.Online_Offline, 0));
      }

      // Reset alarm
      private void cmdReset_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.IssueccIJP, ccIJP.Remote_operation, (int)RemoteOps.ClearFault));
      }

      // Get printer status
      private void cmdGetStatus_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.GetStatus));
      }

      // Hydralic pump shutdown
      private void cmdShutDown_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.IssueccIJP, ccIJP.Remote_operation, (int)RemoteOps.Stop));
      }

      // Hydralic pump startup
      private void cmdStartUp_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.IssueccIJP, ccIJP.Remote_operation, (int)RemoteOps.Start));
      }

      // Printer to standby
      private void cmdStandby_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.IssueccIJP, ccIJP.Remote_operation, (int)RemoteOps.StandBy));
      }

      // Printer to ready
      private void cmdReady_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.IssueccIJP, ccIJP.Remote_operation, (int)RemoteOps.Ready));
      }

      // Read data from the printer
      private void cmdReadData_Click(object sender, EventArgs e) {
         if (int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out int addr)
            && int.TryParse(txtDataLength.Text, out int len)) {
            Modbus.FunctionCode fc = optHoldingRegister.Checked ? Modbus.FunctionCode.ReadHolding : Modbus.FunctionCode.ReadInput;
            byte devAddr = GetDevAddr();
            asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.ReadData) { fc = fc, DevAddr = devAddr, Addr = addr, Len = len });
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
               data = MB.string_to_byte(txtData.Text);
            } else {
               data = new byte[len];
               if (int.TryParse(txtData.Text, out int n)) {
                  for (int i = len; i > 0; i--) {
                     data[i - 1] = (byte)n;
                     n >>= 8;
                  }
               }
            }
            asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.WriteData) { DevAddr = devAddr, Addr = addr, DataA = data });
         }
      }

      // Send an XML message to the printer
      private void cmdSend_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.Send, txtIndentedView.Text));
      }

      // Retrieve message from printer and convert to XML
      private void cmdRetrieve_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.Retrieve));
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
         } else if (tclViews.SelectedTab == tabLogAsXML) {
            tvLogAsXML.Nodes.Clear();
         }
         SetButtonEnables();
      }

      // View the task log in NotePad
      private void cmLogToNotepad_Click(object sender, EventArgs e) {
         string ViewFilename = @"c:\Temp\Err.txt";
         if (tclViews.SelectedTab == tabLog) {
            ViewFilename = @"c:\Temp\Log.txt";
            File.WriteAllLines(ViewFilename, lstMessages.Items.Cast<string>().ToArray());
         } else if (tclViews.SelectedTab == tabIndented) {
            ViewFilename = @"c:\Temp\Indented.HML";
            File.WriteAllLines(ViewFilename, txtIndentedView.Text.Replace("\r\n", "\n").Split('\n'));
         } else if (tclViews.SelectedTab == tabLogAsXML) {
            ViewFilename = @"c:\Temp\LogXML.XML";
            File.WriteAllLines(ViewFilename, LogXML.Replace("\r\n", "\n").Split('\n'));
         }
         Process.Start("notepad.exe", ViewFilename);
         SetButtonEnables();
      }

      // Class selection changed
      private void cbClass_SelectedIndexChanged(object sender, EventArgs e) {
         cbAttribute.Items.Clear();
         cbInstance.Items.Clear();
         if (cbClass.SelectedIndex >= 0) {
            int n = Array.FindIndex(ccNames, x => x == cbClass.Text);
            Type cc = MB.ClassCodeAttributes[n];
            attrNames = Enum.GetNames(MB.ClassCodeAttributes[n]);
            attrNamesSorted = Enum.GetNames(MB.ClassCodeAttributes[n]);
            //Array.Sort(attrNamesSorted);
            cbAttribute.Items.AddRange(attrNamesSorted);
         }

      }

      // Attribute selection changed
      private void cbAttribute_SelectedIndexChanged(object sender, EventArgs e) {
         cbInstance.Items.Clear();
         if (cbAttribute.SelectedIndex >= 0) {
            int n1 = Array.FindIndex(ccNames, x => x == cbClass.Text);
            attValues = (int[])Enum.GetValues(MB.ClassCodeAttributes[n1]);
            int n2 = Array.FindIndex(attrNames, x => x == cbAttribute.Text);
            attr = MB.GetAttrData(MB.ClassCodes[n1], attValues[n2]);
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
                  cbNozzle.SelectedIndex = MB.Nozzle + 1;
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

         MB.DeleteAllButOne();

         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         MB.SetAttribute(ccPF.Format_Setup, "FreeLayout");
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         // Set up the first item (0-origin indexing)
         int item = 0;
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         MB.SetAttribute(ccPF.Column, 1);
         MB.SetAttribute(ccPF.Line, 1);
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         MB.SetAttribute(ccPF.Dot_Matrix, item, "12x16");
         MB.SetAttribute(ccPF.InterCharacter_Space, item, 2);
         MB.SetAttribute(ccPF.Character_Bold, item, 1);
         MB.SetAttribute(ccPF.X_Coordinate, item, 8);
         MB.SetAttribute(ccPF.Y_Coordinate, item, 10);
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         MB.SetAttribute(ccPC.Characters_per_Item, item, 11);
         MB.SetAttribute(ccPC.Print_Character_String, item, "HELLO WORLD");
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         SetButtonEnables();
      }

      // Show I/O packets in Log File.
      private void chkLogIO_CheckedChanged(object sender, EventArgs e) {
         MB.LogIO = chkLogIO.Checked;
      }

      // Retrieve the error log from the printer
      private void cmdErrorRefresh_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.GetErrors));
      }

      // Add a message to the printer's directory
      private void cmdMessageAdd_Click(object sender, EventArgs e) {
         int msgNumber = int.Parse(cbMessageNumber.Text);
         string msgName = txtMessageName.Text.PadRight(12);
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.AddMessage) { Data = msgName, Value = msgNumber });
      }

      // Delete a message from the printer's directory
      private void cmdMessageDelete_Click(object sender, EventArgs e) {
         if (dgMessages.SelectedRows.Count > 0) {
            int msgNumber = int.Parse((string)dgMessages.SelectedRows[0].Cells["colMessage"].Value);
            asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.DeleteMessage) { Value = msgNumber });
            dgMessages.Rows.Remove(dgMessages.SelectedRows[0]);
         }
      }

      // Get all messages from the printer and display them in a data view
      private void cmdMessageRefresh_Click(object sender, EventArgs e) {
         asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.GetMessages));
      }

      // Recall a message that has been stored in the printer
      private void cmdMessageLoad_Click(object sender, EventArgs e) {
         if (int.TryParse((string)dgMessages.SelectedRows[0].Cells[1].Value, out int n)) {
            asyncIO.Tasks.Add(new ModbusPkt(AsyncIO.TaskType.RecallMessage) { Value = n });
         }
      }

      // Re-evaluate enables when leaving
      private void Data_Leave(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // No longer checked.
      private void chkLogAsXML_CheckedChanged(object sender, EventArgs e) {
         if (MB != null) {
            MB.LogAsXML = chkLogAsXML.Checked;
         }
      }

      #endregion

      #region Twin Nozzle Application Events

      // All done.  Close it out
      private void cmdAppQuit_Click(object sender, EventArgs e) {
         twinApp.Close();
         twinApp = null;
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
         while (lstMessages.Items.Count > 2000) {
            lstMessages.Items.RemoveAt(0);
         }
         lstMessages.Items.Add(Readable(msg));
         lstMessages.SelectedIndex = lstMessages.Items.Count - 1;
         lstMessages.Update();
      }

      // Log messages generated by modbus
      private void Modbus_Log(object sender, string msg) {
         string[] s = msg.Split('\n');
         for (int i = 0; i < s.Length; i++) {
            Log(s[i]);
         }
      }

      // Record printer I/O completions
      private void P_Complete(object sender, bool Success) {
         if (Success) {
            txtAcks.Text = (++acks).ToString();
            txtAcks.Refresh();
         } else {
            txtNaks.Text = (++naks).ToString();
            txtNaks.Refresh();
         }
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
         byte devAddr = 1;
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

      // Display log file as TreeView
      private void DisplayLogTree(string logXML) {
         LogXML = logXML;
         XmlDocument LogXmlDoc = new XmlDocument() { PreserveWhitespace = true };
         LogXmlDoc.LoadXml(logXML);

         tvLogAsXML.Nodes.Clear();
         tvLogAsXML.Nodes.Add(new TreeNode(LogXmlDoc.DocumentElement.Name));
         TreeNode tNode = new TreeNode();
         tNode = tvLogAsXML.Nodes[0];

         AddNode(LogXmlDoc.DocumentElement, tNode);
         tvLogAsXML.CollapseAll();
         tvLogAsXML.Nodes[0].Expand();

      }

      // Avoid extra tests by enabling only the buttons that can be used
      private void SetButtonEnables() {
         int addr;
         int len;
         bool isConnected = MB == null ? false : MB.IsConnected;
         bool comIsOn = isConnected && MB.ComIsOn;
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

         cmdErrorRefresh.Enabled = comIsOn;
         cmdErrorClear.Enabled = comIsOn;

         cmdGroupRefresh.Enabled = comIsOn;

         cmdMessageAdd.Enabled = comIsOn && cbMessageNumber.SelectedIndex >= 0 && txtMessageName.Text.Length > 0;
         cmdMessageDelete.Enabled = comIsOn && dgMessages.Rows.Count > 0 && dgMessages.SelectedRows.Count == 1;
         cmdMessageRefresh.Enabled = comIsOn;
         cmdMessageLoad.Enabled = comIsOn && dgMessages.Rows.Count > 0 && dgMessages.SelectedRows.Count == 1;
         //cmdMessageAdd.Enabled = false;
         //cmdMessageDelete.Enabled = false;

         up.SetButtonEnables(comIsOn);
      }

      #endregion

   }

}
