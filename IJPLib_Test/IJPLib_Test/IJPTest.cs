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

      //private IJP ijp;

      IJPMessage message = null;

      IJPOnlineStatus comOn = IJPOnlineStatus.Offline;

      Properties.Settings p;

      #endregion

      #region Constructors and Destructors

      public IJPTest() {
         InitializeComponent();
         initComplete = true;
         p = Properties.Settings.Default;
         IX = new IJPLib_XML(this);
         IX.Complete += IX_IOComplete;
         IX.Log += IX_Log;
      }

      ~IJPTest() {

      }

      #endregion

      #region Form level events

      private void IJPTest_Load(object sender, EventArgs e) {
         // Center the form on the screen
         Utils.PositionForm(this, 0.6f, 0.9f);
         BuildTestFileList();
         setButtonEnables();
      }

      private void IJPTest_Resize(object sender, EventArgs e) {
         //
         // Avoid resize before Program Load has run or on screen minimize
         if (initComplete && ClientRectangle.Height > 0) {
            //
            this.SuspendLayout();
            // Build local parameters
            R = Utils.InitializeResize(this, 60, 40, true);

            Utils.ResizeObject(ref R, ipAddressTextBox, 1, 1, 2, 5);
            Utils.ResizeObject(ref R, cmdConnect, 1, 7, 2, 5);
            Utils.ResizeObject(ref R, cmdComOnOff, 1, 13, 2, 5);

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

            Utils.ResizeObject(ref R, dgDirectory, 1, 1, 38, 15);
            Utils.ResizeObject(ref R, cmdGetDirectory, 1, 17, 2, 5);
            Utils.ResizeObject(ref R, cmdGetOne, 4, 17, 2, 5);
            Utils.ResizeObject(ref R, cmdGetAll, 7, 17, 2, 5);

            Utils.ResizeObject(ref R, lstLogs, 50, 1, 9, 15);

            Utils.ResizeObject(ref R, lblSelectXMLTest, 50, 24, 2, 10);
            Utils.ResizeObject(ref R, cbSelectXMLTest, 52, 24, 2, 10);
            Utils.ResizeObject(ref R, cmdRunXMLTest, 55, 24, 3, 10);

            this.ResumeLayout();
         }
      }

      private void IJPTest_FormClosing(object sender, FormClosingEventArgs e) {
         // Force other thread to close.
         IX?.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.Exit));
      }

      #endregion

      #region Form Control Events

      // Set the enables to reflect the selected tab
      private void tclIJPLib_SelectedIndexChanged(object sender, EventArgs e) {
         setButtonEnables();
      }

      // Connect and turn com on, or turn com off and Disconnect
      private void cmdConnect_Click(object sender, EventArgs e) {
         if (!IX.IsConnected) {
            IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.Connect) { ipAddress = ipAddressTextBox.Text });
         } else {
            IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.Disconnect));
         }
         setButtonEnables();
      }

      // Invert the current com setting
      private void cmdComOnOff_Click(object sender, EventArgs e) {
         switch (comOn) {
            case IJPOnlineStatus.Offline:
               IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetComStatus) { ComStatus = IJPOnlineStatus.Online });
               break;
            case IJPOnlineStatus.Online:
               IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetComStatus) { ComStatus = IJPOnlineStatus.Offline });
               break;
         }
         setButtonEnables();
      }

      // Clear message and all view displays
      private void cmdClear_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.ClearMessage));
         message = null;    // Force retrieval of next message
         ClearViews();      // Clear the four screens
      }

      private void cmdNewMessage_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.NewMessage));
         message = null;    // Force retrieval of next message
         ClearViews();      // Clear the four screens
      }

      // Get indented and tree views of message object settings
      private void cmdGetViews_Click(object sender, EventArgs e) {
         txtIjpIndented.Text = string.Empty;
         tvIJPLibTree.Nodes.Clear();
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetObjectSettings));
         setButtonEnables();
      }

      // Get current message from the printer
      private void cmdGetMessage_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMessage));
      }

      // Get indented and tree views of message object XML
      private void cmdGetXML_Click(object sender, EventArgs e) {
         //IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.ClearMessage));
         //IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMessage));
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetXML));
      }

      // Save indented view of message object or message XML
      private void ccmdSaveAs_Click(object sender, EventArgs e) {
         string fileName = string.Empty;
         string fileText = string.Empty; ;
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
                  fileName = "XMLIndented.XML";
                  fileText = txtXMLIndented.Text;
                  sfd.DefaultExt = "xml";
                  sfd.Filter = "XML|*.xml";
                  sfd.Title = "Save Printer Image to XML file";
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
         setButtonEnables();
      }

      // 
      private void cmdSend_Click(object sender, EventArgs e) {
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.NewMessage));
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetXML) { XML = txtXMLIndented.Text });
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.SetMessage));
      }

      private void cbSelectTest_SelectedIndexChanged(object sender, EventArgs e) {
         setButtonEnables();
      }

      private void cmErrLogToNotepad_Click(object sender, EventArgs e) {
         string ViewFilename = @"c:\Temp\Err.txt";
         File.WriteAllLines(ViewFilename, lstLogs.Items.Cast<string>().ToArray());
         Process.Start("notepad.exe", ViewFilename);
      }

      private void cmErrLogClearlog_Click(object sender, EventArgs e) {
         lstLogs.Items.Clear();
      }

      private void cmdMsgBrowse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() 
            { ShowNewFolderButton = true, SelectedPath = txtMessageFolder.Text };
         if (dlg.ShowDialog() == DialogResult.OK) {
            txtMessageFolder.Text = dlg.SelectedPath;
            BuildTestFileList();
         }
      }

      private void cmdLogBrowse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = txtLogFolder.Text };
         if (dlg.ShowDialog() == DialogResult.OK) {
            txtLogFolder.Text = dlg.SelectedPath;
         }
      }

      private void cbSelectXMLTest_SelectedIndexChanged(object sender, EventArgs e) {
         string fileName = Path.Combine(txtMessageFolder.Text, cbSelectXMLTest.Text + ".xml");
         ClearViews();
         string xml = File.ReadAllText(fileName);
         IX.ProcessLabel(xml, out string indentedXML, out TreeNode tnXML);
         txtXMLIndented.Text = indentedXML;
         tvXMLTree.Nodes.Clear();
         tvXMLTree.Nodes.Add(tnXML);
         tvXMLTree.ExpandAll();
         tclIJPLib.SelectedTab = tabXMLIndented;
         setButtonEnables();
      }

      private void cmdRunXMLTest_Click(object sender, EventArgs e) {
         cmdSend_Click(null, null);
      }

      int firstDirMsg;
      int dirCount;
      bool cancel;
      private void cmdGetDirectory_Click(object sender, EventArgs e) {
         Cursor.Current = Cursors.WaitCursor;
         dgDirectory.Rows.Clear();
         firstDirMsg = 1;
         dirCount = 5;
         cancel = false;
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetDirectory) { Start = firstDirMsg, End = firstDirMsg + dirCount - 1 });
         setButtonEnables();
         Cursor.Current = Cursors.Arrow;
      }

      private void cmdCancel_Click(object sender, EventArgs e) {
         cancel = true;
      }

      private void dgDirectory_SelectionChanged(object sender, EventArgs e) {
         setButtonEnables();
      }

      private void cmdGetOne_Click(object sender, EventArgs e) {
         int n = dgDirectory.SelectedRows[0].Index;
         ushort msgNo = Convert.ToUInt16(dgDirectory.Rows[n].Cells[0].Value);
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.CallMessage) { MessageNumber = msgNo });
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMessage));
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetXML));
         IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetObjectSettings));
      }

      bool SavingAll;
      string nickName;
      private void cmdGetAll_Click(object sender, EventArgs e) {
         SavingAll = true;
         cancel = false;
         GetNextMessage();
      }

      private bool GetNextMessage() {
         for (int i = 0; i < dgDirectory.Rows.Count; i++) {
            if (dgDirectory.Rows[i].Selected) {
               ushort msgNo = Convert.ToUInt16(dgDirectory.Rows[i].Cells[0].Value);
               nickName = dgDirectory.Rows[i].Cells[0].Value.ToString();
               IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.CallMessage) { MessageNumber = msgNo });
               IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetMessage));
               IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetXMLOnly));
               dgDirectory.Rows[i].Selected = false;
               return true;
            }
         }
         return false;
      }

      #endregion

      #region Completion Methods

      private void IX_IOComplete(IJPLib_XML sender, IX_EventArgs evArgs) {
         switch (evArgs.Type) {
            case IJPLib_XML.ReqType.Connect:
               comOn = evArgs.ComStatus;
               break;
            case IJPLib_XML.ReqType.Disconnect:
               comOn = IJPOnlineStatus.Offline;
               break;
            case IJPLib_XML.ReqType.GetMessage:
               this.message = evArgs.message;
               comOn = evArgs.ComStatus;
               break;
            case IJPLib_XML.ReqType.GetDirectory:
               if (evArgs.mi.Length > 0) {
                  for (int i = 0; i < evArgs.mi.Length; i++) {
                     dgDirectory.Rows.Add(new string[]
                     { evArgs.mi[i].RegistrationNumber.ToString(),  evArgs.mi[i].GroupNumber.ToString(), evArgs.mi[i].Nickname });
                  }
                  if (!cancel) {
                     firstDirMsg += dirCount;
                     IX.Tasks.Add(new ReqPkt(IJPLib_XML.ReqType.GetDirectory) { Start = firstDirMsg, End = firstDirMsg + dirCount - 1 });
                  }
               } else {
               }
               dgDirectory.FirstDisplayedScrollingRowIndex = dgDirectory.Rows.Count - 1;
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
               // need to do a save here
               if (!cancel) {
                  GetNextMessage();
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
               comOn = evArgs.ComStatus;
               break;
            case IJPLib_XML.ReqType.Exit:
               break;
            default:
               break;
         }
         Log($"{evArgs.Type.ToString()} Complete");
         setButtonEnables();
      }

      #endregion

      #region Service routines

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
         try {
            string[] FileNames = Directory.GetFiles(txtMessageFolder.Text, "*.XML");
            Array.Sort(FileNames);
            for (int i = 0; i < FileNames.Length; i++) {
               cbSelectXMLTest.Items.Add(Path.GetFileNameWithoutExtension(FileNames[i]));
            }
         } catch {

         }
      }

      // Log requests from IX Object
      private void IX_Log(IJPLib_XML sender, string msg) {
         Log(msg);
      }

      // Log messages
      public void Log(string s) {
         lstLogs.Items.Add(s);
         lstLogs.Update();
         lstLogs.SelectedIndex = lstLogs.Items.Count - 1;
      }

      private void setButtonEnables() {
         bool connected = IX != null && IX.IsConnected;
         bool IOPossible = connected && comOn == IJPOnlineStatus.Online;
         // These must connect first
         cmdComOnOff.Enabled = connected;
         cmdConnect.Text = connected ? "Disconnect" : "Connect";
         cmdComOnOff.Text = comOn == IJPOnlineStatus.Online ? "Com Off" : "Com On";
         cmdGetViews.Enabled = IOPossible || message != null;
         cmdGetXML.Enabled = IOPossible || message != null;
         cmdGetDirectory.Enabled = IOPossible;
         cmdGetOne.Enabled = IOPossible && dgDirectory.SelectedRows.Count > 0;
         cmdGetAll.Enabled = IOPossible && dgDirectory.SelectedRows.Count > 0;

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

   }
}
