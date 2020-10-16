using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using HIES.IJP.RX;
using HIES.ModbusTcp;
using IJPLibXML;

namespace IJPLib_Test {


   public partial class IJPTest : Form {

      #region Data Declarations

      ResizeInfo R;
      bool initComplete = false;

      IJPLib_XML IX;

      Status StatusArea = new Status();

      Properties.Settings p;

      // Variables for Save All files in printer directory
      bool SA_SavingAll;
      string SA_NickName;
      ushort SA_PrinterID;
      byte SA_GroupID;
      int SA_DirectoryID;

      // Variables for Load All files into printer directory
      bool LA_LoadingAll;
      string LA_NickName;
      ushort LA_PrinterID;
      byte LA_GroupID;
      int LA_DirectoryID;

      // Variables for retrieving the printer's directory
      bool GD_GettingDirectory;
      int GD_FirstMsg;
      int GD_DirCount;

      // Variables for retrieving Alarm Codes
      bool GA_GettingAlarms;
      int GA_CurrentAlarm;
      int GA_AlarmCount;
      int GA_TotalAlarms;

      // Cancel for multi step processes
      bool Cancel = false;

      #endregion

      #region Constructors and Destructors

      public IJPTest() {
         InitializeComponent();
         initComplete = true;
         p = Properties.Settings.Default;
         IX = new IJPLib_XML(this);
         IX.Complete += IX_Complete;
         IX.Log += IX_Log;
      }

      #endregion

      #region Form level events

      // Initialize position and load all dropdowns
      private void IJPTest_Load(object sender, EventArgs e) {
         // Center the form on the screen
         Utils.PositionForm(this, 0.6f, 0.9f);
         BuildTestFileList();
         cbMiscOps.Items.AddRange(Enum.GetNames(typeof(IJPLib_XML.ReqSubType)));
         SetButtonEnables();
      }

      // Allow the form to run on any screen size and resolution
      private void IJPTest_Resize(object sender, EventArgs e) {
         //
         // Avoid resize before Program Load has run or on screen minimize
         if (initComplete && ClientRectangle.Height > 0) {
            //
            this.SuspendLayout();
            // Build local parameter
            R = Utils.InitializeResize(this, 60, 40, true);

            Utils.ResizeObject(ref R, ipAddressTextBox, 1, 1, 2, 5);
            Utils.ResizeObject(ref R, cmdConnect, 1, 6.5f, 2, 3.5f);
            Utils.ResizeObject(ref R, cmdComOn, 1, 10.5f, 2, 3.5f);
            Utils.ResizeObject(ref R, cmdComOff, 1, 14.5f, 2, 3.5f);

            Utils.ResizeObject(ref R, lblMessageFolder, 0.5f, 18, 2, 5);
            Utils.ResizeObject(ref R, txtMessageFolder, 0.5f, 23, 2, 11);
            Utils.ResizeObject(ref R, cmdMsgBrowse, 0.5f, 35, 2, 4);

            Utils.ResizeObject(ref R, lblLogFolder, 3, 18, 2, 5);
            Utils.ResizeObject(ref R, txtLogFolder, 3, 23, 2, 11);
            Utils.ResizeObject(ref R, cmdLogBrowse, 3, 35, 2, 4);

            Utils.ResizeObject(ref R, tclIJPLib, 5, 1, 43, 33);
            Utils.ResizeObject(ref R, cmdClear, 7, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdNewMessage, 10, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdGetMessage, 13, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdGetXML, 16, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdGetViews, 19, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdSaveAs, 22, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdSend, 25, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdCancel, 28, 35, 2, 4);

            Utils.ResizeObject(ref R, txtIjpIndented, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, tvIJPLibTree, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, txtXMLIndented, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, tvXMLTree, 1, 1, 38, 31);

            Utils.ResizeObject(ref R, lblFolderDirectory, 1, 1, 2, 8);
            Utils.ResizeObject(ref R, dgFolder, 3, 1, 36, 8);
            Utils.ResizeObject(ref R, cmdSaveInPrinter, 3, 9.5f, 2, 4);

            Utils.ResizeObject(ref R, lblPrinterDirectory, 1, 14, 2, 14);
            Utils.ResizeObject(ref R, dgDirectory, 3, 14, 36, 14);
            Utils.ResizeObject(ref R, cmdGetDirectory, 3, 28.5f, 2, 4);
            Utils.ResizeObject(ref R, cmdGetOne, 6, 28.5f, 2, 4);
            Utils.ResizeObject(ref R, cmdGetAll, 9, 28.5f, 2, 4);

            Utils.ResizeObject(ref R, lblAlarms, 1, 1, 2, 27);
            Utils.ResizeObject(ref R, dgAlarms, 3, 1, 36, 27);
            Utils.ResizeObject(ref R, cmdGetAlarms, 3, 28.5f, 2, 4);

            Utils.ResizeObject(ref R, lblMiscIndented, 1, 1, 2, 13);
            Utils.ResizeObject(ref R, txtMisc, 3, 1, 36, 13);
            Utils.ResizeObject(ref R, lblMiscTreeView, 1, 15, 2, 13);
            Utils.ResizeObject(ref R, tvMisc, 3, 15, 36, 13);
            Utils.ResizeObject(ref R, lblSelect, 1, 28.5f, 2, 4);
            Utils.ResizeObject(ref R, cbMiscOps, 3, 28.5f, 2, 4);
            Utils.ResizeObject(ref R, cmdGetMisc, 6, 28.5f, 2, 4);

            Utils.ResizeObject(ref R, lstLogs, 50, 1, 9, 15);

            Utils.ResizeObject(ref R, cmdTest1, 50, 17, 3, 5);
            Utils.ResizeObject(ref R, cmdTest2, 55, 17, 3, 5);

            Utils.ResizeObject(ref R, lblSelectXMLTest, 50, 24, 2, 10);
            Utils.ResizeObject(ref R, cbSelectXMLTest, 52, 24, 2, 10);
            Utils.ResizeObject(ref R, cmdRunXMLTest, 55, 24, 3, 10);

            this.ResumeLayout();
         }
      }

      // All done, shut down the other process
      private void IJPTest_FormClosing(object sender, FormClosingEventArgs e) {
         // Force other thread to close.
         IX?.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.Exit));
      }

      #endregion

      #region Form Control Events

      // Set the enables to reflect the selected tab
      private void IJPLib_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // Connect and turn com on, or turn com off and Disconnect
      private void Connect_Click(object sender, EventArgs e) {
         if (!IX.IsConnected) {
            IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.Connect) { IpAddress = ipAddressTextBox.Text });
         } else {
            IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.Disconnect));
         }
         SetButtonEnables();
      }

      // Set COM ON
      private void ComOn_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetComStatus) { ComStatus = IJPOnlineStatus.Online });
         SetButtonEnables();
      }

      // Set COM OFF
      private void ComOff_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetComStatus) { ComStatus = IJPOnlineStatus.Offline });
         SetButtonEnables();
      }

      // Clear message and all view displays
      private void Clear_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.ClearMessage));
         ClearViews();      // Clear the four screens
      }

      // Create a new message with only the default settings
      private void NewMessage_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.NewMessage));
         ClearViews();      // Clear the four screens
      }

      // Get indented and tree views of message object settings
      private void GetViews_Click(object sender, EventArgs e) {
         txtIjpIndented.Text = string.Empty;
         tvIJPLibTree.Nodes.Clear();
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetObjectSettings));
         SetButtonEnables();
      }

      // Get current message from the printer
      private void GetMessage_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMessage));
      }

      // Get indented and tree views of message object XML
      private void GetXML_Click(object sender, EventArgs e) {
         //IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.ClearMessage));
         //IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMessage));
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetXML));
      }

      // Save indented view of message object or message XML
      private void SaveAs_Click(object sender, EventArgs e) {
         string fileName = string.Empty;
         string fileText = string.Empty;
         ;
         using (SaveFileDialog sfd = new SaveFileDialog()) {
            switch (tclIJPLib.SelectedIndex) {
               case 0:
                  fileName = "IJPIndented.txt";
                  fileText = txtIjpIndented.Text;
                  sfd.DefaultExt = "txt";
                  sfd.Filter = "Text|*.txt";
                  sfd.Title = "Save Printer Image to Text file";
                  break;
               case 2:
                  fileName = "XMLIndented.HML";
                  fileText = txtXMLIndented.Text;
                  sfd.DefaultExt = "hml";
                  sfd.Filter = "HML|*.hml";
                  sfd.Title = "Save Printer Image to HML file";
                  break;
               default:
                  break;
            }
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.InitialDirectory = txtMessageFolder.Text;
            sfd.FileName = fileName;
            if (sfd.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty(sfd.FileName)) {
               fileName = Path.Combine(txtMessageFolder.Text, sfd.FileName);
               File.WriteAllText(fileName, fileText);
            }
         }
         BuildTestFileList();
         SetButtonEnables();
      }

      // Send current message to the printer
      private void Send_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.NewMessage));
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetXML) { XML = txtXMLIndented.Text });
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetMessage));
      }

      // Update enables when the selected test changes
      private void SelectTest_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // View the task log in NotePad
      private void ErrLogToNotepad_Click(object sender, EventArgs e) {
         string ViewFilename = @"c:\Temp\Err.txt";
         File.WriteAllLines(ViewFilename, lstLogs.Items.Cast<string>().ToArray());
         Process.Start("notepad.exe", ViewFilename);
      }

      // Clear the task log
      private void ErrLogClearlog_Click(object sender, EventArgs e) {
         lstLogs.Items.Clear();
      }

      // Browse for new message folder
      private void Browse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = txtMessageFolder.Text };
         if (dlg.ShowDialog() == DialogResult.OK) {
            txtMessageFolder.Text = dlg.SelectedPath;
            BuildTestFileList();
         }
      }

      // Browse for new Log Folder
      private void LogBrowse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = txtLogFolder.Text };
         if (dlg.ShowDialog() == DialogResult.OK) {
            txtLogFolder.Text = dlg.SelectedPath;
         }
      }

      // Load a new XML file and populate the four XML and Object views
      private void SelectXMLTest_SelectedIndexChanged(object sender, EventArgs e) {
         string fileName = Path.Combine(txtMessageFolder.Text, cbSelectXMLTest.Text + ".hml");
         ClearViews();
         string xml = File.ReadAllText(fileName);
         IX.ProcessLabel(xml, out string indentedXML, out TreeNode tnXML);
         txtXMLIndented.Text = indentedXML;
         tvXMLTree.Nodes.Clear();
         tvXMLTree.Nodes.Add(tnXML);
         tvXMLTree.ExpandAll();
         tclIJPLib.SelectedTab = tabXMLIndented;
         SetButtonEnables();
      }

      // Send text from the XML Indented View to the printer
      private void RunXMLTest_Click(object sender, EventArgs e) {
         Send_Click(null, null);
      }

      // Get the directory five entries at a time.  Quit if 0 items retyrned.
      private void GetDirectory_Click(object sender, EventArgs e) {
         dgDirectory.Rows.Clear();
         GD_GettingDirectory = true;
         GD_FirstMsg = 1;
         GD_DirCount = 5;
         Cancel = false;
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetDirectory) { Start = GD_FirstMsg, End = GD_FirstMsg + GD_DirCount - 1 });
         SetButtonEnables();
      }

      // Set flag to terminate an iterative process
      private void Cancel_Click(object sender, EventArgs e) {
         Cancel = true;
      }

      // Set button enables to reflect changes in the selected dictionary entries
      private void Directory_SelectionChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // Set button enables to reflect changes in the selected folder entries
      private void Folder_SelectionChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // Retrieve one message for the printers directory 
      private void GetOne_Click(object sender, EventArgs e) {
         int n = dgDirectory.SelectedRows[0].Index;
         ushort msgNo = Convert.ToUInt16(dgDirectory.Rows[n].Cells[0].Value);
         ushort PrinterID = Convert.ToUInt16(dgDirectory.Rows[n].Cells[0].Value);
         byte GroupID = Convert.ToByte(dgDirectory.Rows[n].Cells[1].Value);
         string NickName = dgDirectory.Rows[n].Cells[2].Value.ToString();
         // Call the message into the printer
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.CallMessage) { MessageNumber = PrinterID });
         // Get message from the printer 
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMessage));
         // Generate the two XML Views
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetXML) { MessageInfo = new IJPMessageInfo(PrinterID, GroupID, NickName) });
         // Generate the two Object views
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetObjectSettings));
      }

      // Initialize the iterative process for retrieving and saving all messages
      private void GetAll_Click(object sender, EventArgs e) {
         SA_SavingAll = true;
         Cancel = false;
         GetNextMessageToSave();
      }

      // Find the next message to process
      private void GetNextMessageToSave() {
         for (int i = 0; i < dgDirectory.Rows.Count; i++) {
            if (dgDirectory.Rows[i].Selected) {
               SA_DirectoryID = i;
               SA_PrinterID = Convert.ToUInt16(dgDirectory.Rows[i].Cells[0].Value);
               SA_GroupID = Convert.ToByte(dgDirectory.Rows[i].Cells[1].Value);
               SA_NickName = dgDirectory.Rows[i].Cells[2].Value.ToString();
               IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.CallMessage) { MessageNumber = SA_PrinterID });
               IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMessage));
               IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetXMLOnly) { MessageInfo = new IJPMessageInfo(SA_PrinterID, SA_GroupID, SA_NickName) });
               return;
            }
         }
         SA_SavingAll = false;
      }

      // Initialize the iterative process for sending and saving all messages into the printer
      private void SaveInPrinter_Click(object sender, EventArgs e) {
         LA_LoadingAll = true;
         Cancel = false;
         GetNextMessageToLoad();
      }

      // Find the next message to process
      private void GetNextMessageToLoad() {
         for (int i = 0; i < dgFolder.Rows.Count; i++) {
            if (dgFolder.Rows[i].Selected) {
               LA_DirectoryID = i;
               string fileName = Path.Combine(txtMessageFolder.Text, dgFolder.Rows[i].Cells[0].Value.ToString() + ".hml");
               if (File.Exists(fileName)) {
                  string xml = File.ReadAllText(fileName);
                  XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = true };
                  xmlDoc.LoadXml(xml);
                  XmlNode msg = xmlDoc.SelectSingleNode("Label/Message");
                  if (msg != null) {
                     if (ushort.TryParse(GetXmlAttr(msg, "Registration"), out LA_PrinterID)
                        && byte.TryParse(GetXmlAttr(msg, "GroupNumber"), out LA_GroupID)) {
                        LA_NickName = GetXmlAttr(msg, "Name");
                        IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.NewMessage));
                        IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetXML) { XML = xml });
                        IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetMessage));
                        IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SaveMessage) { MessageInfo = new IJPMessageInfo(LA_PrinterID, LA_GroupID, LA_NickName) });
                        IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.RenameMessage) { MessageNumber = LA_PrinterID, MessageName = LA_NickName });
                        return;
                     }
                  }
               }
            }
         }
         SA_SavingAll = false;
      }

      private void cmdGetAlarms_Click(object sender, EventArgs e) {
         GA_TotalAlarms = IX.GetAlarmCount();
         GA_CurrentAlarm = 1;
         GA_AlarmCount = 5;
         GA_GettingAlarms = true;
         dgAlarms.Rows.Clear();
         GetNextAlarmToLoad();
      }

      private void GetNextAlarmToLoad() {
         int n = Math.Min(GA_AlarmCount, GA_TotalAlarms - GA_CurrentAlarm + 1);
         if (n > 0) {
            IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetAlarms) { Start = GA_CurrentAlarm, End = n });
         }
      }

      private void cbMiscOps_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      private void cmdGetMisc_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMisc) { SubType = (IJPLib_XML.ReqSubType)cbMiscOps.SelectedIndex });
      }

      #endregion

      #region Completion Methods

      // Receive notification as each task completes
      private void IX_Complete(object sender, IX_EventArgs evArgs) {
         switch (evArgs.Type) {
            case IJPLib_XML.ReqType.Connect:
               break;
            case IJPLib_XML.ReqType.Disconnect:
               break;
            case IJPLib_XML.ReqType.GetMessage:
               break;
            case IJPLib_XML.ReqType.GetDirectory:
               if (GD_GettingDirectory) {
                  if (evArgs.Mi != null && evArgs.Mi.Length > 0) {
                     for (int i = 0; i < evArgs.Mi.Length; i++) {
                        dgDirectory.Rows.Add(new string[]
                        { evArgs.Mi[i].RegistrationNumber.ToString(),  evArgs.Mi[i].GroupNumber.ToString(), evArgs.Mi[i].Nickname });
                     }
                     if (Cancel) {
                        GD_GettingDirectory = false;
                     } else {
                        GD_FirstMsg += GD_DirCount;
                        IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetDirectory) { Start = GD_FirstMsg, End = GD_FirstMsg + GD_DirCount - 1 });
                     }
                     dgDirectory.FirstDisplayedScrollingRowIndex = dgDirectory.Rows.Count - 1;
                  } else {
                     GD_GettingDirectory = false;
                  }
               }
               break;
            case IJPLib_XML.ReqType.GetXML:
               txtXMLIndented.Text = evArgs.Indented;
               tvXMLTree.Nodes.Clear();
               if (evArgs.TreeNode != null) {
                  tvXMLTree.Nodes.Add(evArgs.TreeNode);
                  tvXMLTree.ExpandAll();
               }
               tclIJPLib.SelectedTab = tabXMLIndented;
               break;
            case IJPLib_XML.ReqType.GetXMLOnly:
               if (SA_SavingAll) {
                  dgDirectory.Rows[SA_DirectoryID].Selected = false;
                  if (Directory.Exists(txtMessageFolder.Text)) {
                     File.WriteAllText(Path.Combine(txtMessageFolder.Text, $"{SA_NickName}.hml"), evArgs.Indented);
                     Log($"File \"{SA_NickName}\" saved");
                  } else {
                     Log($"Directory \"{txtMessageFolder.Text}\" does not exits!");
                     Cancel = true;
                  }
                  // need to do a save here
                  if (!Cancel) {
                     GetNextMessageToSave();
                  }
               }
               break;
            case IJPLib_XML.ReqType.GetObjectSettings:
               txtIjpIndented.Text = evArgs.Indented;
               if (evArgs.TreeNode != null) {
                  tvIJPLibTree.Nodes.Add(evArgs.TreeNode);
                  tvIJPLibTree.ExpandAll();
               }
               tclIJPLib.SelectedTab = tabIndentedView;
               break;
            case IJPLib_XML.ReqType.SetComStatus:
               break;
            case IJPLib_XML.ReqType.SaveMessage:
               if (LA_LoadingAll) {
                  dgFolder.Rows[LA_DirectoryID].Selected = false;
                  if (!Cancel) {
                     GetNextMessageToLoad();
                  }
               }
               break;
            case IJPLib_XML.ReqType.GetAlarms:
               for (int i = 0; i < evArgs.AlarmHistory.Length; i++) {
                  string status = StatusArea.TranslateStatus(Status.Area.Operation, (char)evArgs.AlarmHistory[i].ErrorCode);
                  dgAlarms.Rows.Add(new string[] { evArgs.AlarmHistory[i].DateTime.ToString(), evArgs.AlarmHistory[i].ErrorCode.ToString(), status });
               }
               GA_CurrentAlarm += evArgs.AlarmHistory.Length;
               if (!Cancel) {
                  GetNextAlarmToLoad();
               }
               break;
            case IJPLib_XML.ReqType.GetMisc:
               lblMiscIndented.Text = $"{evArgs.SubType.ToString()} Indented View";
               txtMisc.Text = evArgs.Indented;
               lblMiscTreeView.Text = $"{evArgs.SubType.ToString()} Tree View";
               tvMisc.Nodes.Clear();
               tvMisc.Nodes.Add(evArgs.TreeNode);
               tvMisc.ExpandAll();
               break;
            case IJPLib_XML.ReqType.Exit:
               break;
            default:
               break;
         }
         Log($"{evArgs.ToString()} Complete!");
         SetButtonEnables();
      }

      #endregion

      #region Service routines

      // Get XML Attribute Value
      private string GetXmlAttr(XmlNode node, string AttrName) {
         XmlNode n;
         if (node != null && (n = node.Attributes[AttrName]) != null) {
            return n.Value;
         } else {
            return "N_A";
         }
      }

      // Clear XML and Object Indented and Tree View displays
      private void ClearViews() {
         tvIJPLibTree.Nodes.Clear();
         tvXMLTree.Nodes.Clear();
         txtIjpIndented.Text = string.Empty;
         txtXMLIndented.Text = string.Empty;
      }

      // Populate dropdown with XML files in Messages folder
      private void BuildTestFileList() {
         cbSelectXMLTest.Items.Clear();
         dgFolder.Rows.Clear();
         if (Directory.Exists(txtMessageFolder.Text)) {
            string[] FileNames = Directory.GetFiles(txtMessageFolder.Text, "*.HML");
            Array.Sort(FileNames);
            for (int i = 0; i < FileNames.Length; i++) {
               string fileName = Path.GetFileNameWithoutExtension(FileNames[i]);
               cbSelectXMLTest.Items.Add(fileName);
               dgFolder.Rows.Add(fileName);
            }
         } else {
            Log($"The Directory \"{txtMessageFolder.Text}\" does not exist!");
         }
      }

      // Log requests from IX Object
      private void IX_Log(object sender, string msg) {
         Log(msg);
      }

      // Log messages
      public void Log(string s) {
         lstLogs.Items.Add(s);
         lstLogs.Update();
         lstLogs.SelectedIndex = lstLogs.Items.Count - 1;
      }

      // Enable buttons to reflect the current environment
      private void SetButtonEnables() {
         bool connected = IX != null && IX.IsConnected;
         bool IOPossible = connected && IX.ComStatus == IJPOnlineStatus.Online;
         bool msgExists = IX.MessageExists;
         bool msgDirExists = Directory.Exists(txtMessageFolder.Text);
         // These must connect first
         cmdComOn.Enabled = connected;
         cmdComOff.Enabled = connected;
         cmdConnect.Text = connected ? "Disconnect" : "Connect";
         //cmdComOn.Text = IX.ComStatus == IJPOnlineStatus.Online ? "Com Off" : "Com On";
         cmdGetViews.Enabled = IOPossible || msgExists;
         cmdGetXML.Enabled = IOPossible || msgExists;
         cmdGetDirectory.Enabled = IOPossible;
         cmdGetOne.Enabled = IOPossible && dgDirectory.SelectedRows.Count > 0;
         cmdGetAll.Enabled = IOPossible && dgDirectory.SelectedRows.Count > 0 && msgDirExists;
         cmdSaveInPrinter.Enabled = IOPossible && dgFolder.SelectedRows.Count > 0 && msgDirExists;
         cmdGetAlarms.Enabled = IOPossible;
         cmdGetMisc.Enabled = IOPossible && cbMiscOps.SelectedIndex >= 0;
         cmdRunXMLTest.Enabled = IOPossible && cbSelectXMLTest.SelectedIndex >= 0;

         switch (tclIJPLib.SelectedIndex) {
            case 0:
               cmdSaveAs.Enabled = txtIjpIndented.Text.Length > 0;
               break;
            case 2:
               cmdSaveAs.Enabled = txtXMLIndented.Text.Length > 0;
               break;
            default:
               cmdSaveAs.Enabled = false;
               break;
         }
      }

      #endregion

      private void cmdTest1_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetComStatus) { ComStatus = IJPOnlineStatus.Online });
      }

      private void cmdTest2_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetComStatus) { ComStatus = IJPOnlineStatus.Offline });
      }
   }
}
