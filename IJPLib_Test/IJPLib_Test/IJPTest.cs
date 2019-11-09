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

namespace IJPLib_Test {
   public partial class IJPTest : Form {

      #region Data Declarations

      ResizeInfo R;
      bool initComplete = false;

      IJPMessage message = null;

      IJPOnlineStatus comOn = IJPOnlineStatus.Offline;

      IJPLib_Test.Properties.Settings p;

      #endregion

      #region Constructors and Destructors

      private IJP ijp;

      public IJPTest() {
         InitializeComponent();
         initComplete = true;
         p = IJPLib_Test.Properties.Settings.Default;
         ipAddressTextBox.Text = p.IPAddress;
         txtMessageFolder.Text = p.MessageFolder;
      }

      ~IJPTest() {

      }

      #endregion

      #region Form level events

      private void IJPTest_Load(object sender, EventArgs e) {
         // Center the form on the screen
         Utils.PositionForm(this, 0.5f, 0.9f);
         cbSelectHardCodedTest.Items.AddRange(AvailableTests);
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

            Utils.ResizeObject(ref R, lblMessageFolder, 1, 24, 2, 10);
            Utils.ResizeObject(ref R, txtMessageFolder, 3, 24, 2, 10);
            Utils.ResizeObject(ref R, cmdBrowse, 1, 35, 4, 4);

            Utils.ResizeObject(ref R, tclIJPLib, 5, 1, 43, 33);
            Utils.ResizeObject(ref R, cmdClear, 7, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdGetXML, 10, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdGetViews, 13, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdSaveAs, 16, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdSend, 19, 35, 2, 4);

            Utils.ResizeObject(ref R, txtIjpIndented, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, tvIJPLibTree, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, txtXMLIndented, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, tvXMLTree, 1, 1, 38, 31);

            Utils.ResizeObject(ref R, lstLogs, 50, 1, 9, 15);

            Utils.ResizeObject(ref R, lblSelectHardCodedTest, 50, 17, 2, 10);
            Utils.ResizeObject(ref R, cbSelectHardCodedTest, 52, 17, 2, 10);
            Utils.ResizeObject(ref R, cmdRunHardCodedTest, 55, 17, 3, 10);

            Utils.ResizeObject(ref R, lblSelectXMLTest, 50, 28, 2, 10);
            Utils.ResizeObject(ref R, cbSelectXMLTest, 52, 28, 2, 10);
            Utils.ResizeObject(ref R, cmdRunXMLTest, 55, 28, 3, 10);

            this.ResumeLayout();
         }
      }

      private void IJPTest_FormClosing(object sender, FormClosingEventArgs e) {
         p.IPAddress = ipAddressTextBox.Text;
         p.MessageFolder = txtMessageFolder.Text;
         p.Save();
      }

      #endregion

      #region Form Control Events

      private void tclIJPLib_SelectedIndexChanged(object sender, EventArgs e) {
         setButtonEnables();
      }

      private void cmdConnect_Click(object sender, EventArgs e) {
         if (null == this.ijp) {
            // Connect to the printer
            ConnectIJP();
            // Get com setting
            comOn = ijp.GetComPort();
            if (comOn == IJPOnlineStatus.Offline) {
               cmdComOnOff_Click(null, null);
            }
            // Set Caption
            this.cmdConnect.Text = "Disconnect";
         } else {
            // Turn com off
            cmdComOnOff_Click(null, null);
            // Disconnect from printer
            DisconnectIJP();
            // Set caption
            this.cmdConnect.Text = "Connect";
         }
         setButtonEnables();
      }

      private void cmdComOnOff_Click(object sender, EventArgs e) {
         switch (comOn) {
            case IJPOnlineStatus.Offline:
               this.ijp.SetComPort(IJPOnlineStatus.Online);
               comOn = IJPOnlineStatus.Online;
               break;
            case IJPOnlineStatus.Online:
               this.ijp.SetComPort(IJPOnlineStatus.Offline);
               comOn = IJPOnlineStatus.Offline;
               break;
            default:
               break;
         }
         setButtonEnables();
      }

      private void cmdClear_Click(object sender, EventArgs e) {
         message = null;    // Force retrieval of next message
         ClearViews();      // Clear the four screens
      }

      private void cmdGetViews_Click(object sender, EventArgs e) {
         Log("Get View Starting");
         //  Set hour glass
         Cursor.Current = Cursors.WaitCursor;
         // Out with the old
         txtIjpIndented.Text = string.Empty;
         tvIJPLibTree.Nodes.Clear();
         // In with the new
         GetCurrentMessage();
         // Generate the views
         ObjectDumper od = new ObjectDumper(2);
         string indentedView;
         TreeNode treeNode;
         od.Dump(message, out indentedView, out treeNode);
         // Display the viewd
         txtIjpIndented.Text = indentedView;
         tvIJPLibTree.Nodes.Add(treeNode);
         tvIJPLibTree.ExpandAll();
         tclIJPLib.SelectedTab = tabIndentedView;
         // Restore cursor
         Cursor.Current = Cursors.Arrow;
         setButtonEnables();
         Log("Get View Complete");
      }

      private void cmdGetXML_Click(object sender, EventArgs e) {
         Log("Get XML Starting");                      // Show start
         Cursor.Current = Cursors.WaitCursor;          // Set hour glass
         ClearViews();                                 // Out with the old
         GetCurrentMessage();                         // Use current or new message
         MsgToXml mtx = new MsgToXml(message);
         txtXMLIndented.Text = mtx.GetXML();           // Display the indented XML as it is.
         ProcessLabel(txtXMLIndented.Text);            // Build XML Tree and display it
         tclIJPLib.SelectedTab = tabXMLIndented;       // Make XML Indented tab visible
         Cursor.Current = Cursors.Arrow;               // Restore cursor
         setButtonEnables();                           // Enable the correct buttons
         Log("Get XML Complete");                      // Show Completion
      }

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

      private void cmdSend_Click(object sender, EventArgs e) {
         try {
            Log("Send Message Starting");
            cmdRunXMLTest_Click(null, null);
            ijp.SetMessage(message);
         } catch (Exception e2) {
            Log($"Send Message: {e2.Message}\r\n{e2.StackTrace}");
         } finally {
            Log("Send Message Complete");
         }
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

      private void cmdBrowse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() 
            { ShowNewFolderButton = true, SelectedPath = txtMessageFolder.Text };
         if (dlg.ShowDialog() == DialogResult.OK) {
            txtMessageFolder.Text = dlg.SelectedPath;
            BuildTestFileList();
         }
      }

      private void cbSelectXMLTest_SelectedIndexChanged(object sender, EventArgs e) {
         string fileName = Path.Combine(txtMessageFolder.Text, cbSelectXMLTest.Text + ".xml");
         ClearViews();
         string xml = File.ReadAllText(fileName);
         ProcessLabel(xml);
         setButtonEnables();
      }

      private void cmdRunXMLTest_Click(object sender, EventArgs e) {
         XmlToMsg xtm = new XmlToMsg(txtXMLIndented.Text);
         xtm.Log += Log;
         message = xtm.GetMessage();
         xtm.Log -= Log;
      }

      #endregion

      #region XML Formatting

      // Process an XML Label
      private bool ProcessLabel(string xml) {
         XmlDocument xmlDoc;
         bool result = false;
         int xmlStart;
         int xmlEnd;
         try {
            // Can be called with a Filename or XML text
            xmlStart = xml.IndexOf("<Label");
            if (xmlStart == -1) {
               xml = File.ReadAllText(xml);
               xmlStart = xml.IndexOf("<Label");
            }
            // No label found, exit
            if (xmlStart == -1) {
               return result;
            }
            xmlEnd = xml.IndexOf("</Label>", xmlStart + 7);
            if (xmlEnd > 0) {
               xml = xml.Substring(xmlStart, xmlEnd - xmlStart + 8);
               xmlDoc = new XmlDocument() { PreserveWhitespace = true };
               xmlDoc.LoadXml(xml);
               xml = ToIndentedString(xml);
               xmlStart = xml.IndexOf("<Label");
               if (xmlStart > 0) {
                  xml = xml.Substring(xmlStart);
                  txtXMLIndented.Text = xml;

                  tvXMLTree.Nodes.Clear();
                  tvXMLTree.Nodes.Add(new TreeNode(xmlDoc.DocumentElement.Name));
                  TreeNode tNode = new TreeNode();
                  tNode = tvXMLTree.Nodes[0];

                  AddNode(xmlDoc.DocumentElement, tNode);
                  tvXMLTree.ExpandAll();

                  result = true;
               }
            }
         } catch {

         }
         return result;
      }

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

      #endregion

      #region Service routines

      private void ConnectIJP() {
         ConnectIJP(this.ipAddressTextBox.Text, 5000, 5);
      }

      private void ConnectIJP(string ipAddress, int timeout, int retry) {
         if (null != this.ijp) {
            DisconnectIJP();
            this.ijp = null;
         }
         try {

            // Create the IJP object.
            this.ijp = new IJP();

            // Set parameters.
            this.ijp.IPAddress = ipAddress;
            this.ijp.Timeout = timeout;
            this.ijp.Retry = retry;

            // Connect the Ink jet printer.
            this.ijp.Connect();

         } catch (Exception e) {
            Log($"ConnectIJP: {e.Message} \n{e.StackTrace}");
         }
         setButtonEnables();
      }

      private void DisconnectIJP() {
         if (null != this.ijp) {
            this.ijp.Disconnect();
            this.ijp = null;
         }
         setButtonEnables();
      }

      private void GetCurrentMessage() {
         try {
            if (message == null) {
               // Get the current message.
               message = (IJPMessage)this.ijp.GetMessage();
            }
         } catch (Exception e) {

         }
         setButtonEnables();
      }

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

      private void Log(string s) {
         lstLogs.Items.Add(s);
         lstLogs.Update();
      }

      private void setButtonEnables() {
         bool connected = ijp != null;
         // These must connect first
         cmdComOnOff.Enabled = connected;
         cmdComOnOff.Text = comOn == IJPOnlineStatus.Online ? "Com Off" : "Com On";
         cmdGetViews.Enabled = connected && comOn == IJPOnlineStatus.Online || message != null;
         cmdGetXML.Enabled = connected && comOn == IJPOnlineStatus.Online || message != null;

         cmdRunHardCodedTest.Enabled = comOn == IJPOnlineStatus.Online && cbSelectHardCodedTest.SelectedIndex >= 0;
         cmdRunXMLTest.Enabled = comOn == IJPOnlineStatus.Online && cbSelectXMLTest.SelectedIndex >= 0;

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

      #region Test Routines

      string[] AvailableTests = new string[]
         { "New Message", "Retrieve Message", "Send Message", "Clear Screen",
           "Create Message", "Create Complex Message", "Echo" };

      private void cmdRunTest_Click(object sender, EventArgs e) {
         try {
            Cursor.Current = Cursors.WaitCursor;
            Log($"{cbSelectHardCodedTest.Text} Starting");
            switch (cbSelectHardCodedTest.SelectedIndex) {
               case 0:
                  NewMessage();
                  break;
               case 1:
                  RetrieveMessage();
                  break;
               case 2:
                  SendMessage();
                  break;
               case 3:
                  ClearDisplay();
                  break;
               case 4:
                  CreateMessage();
                  break;
               case 5:
                  CreateComplex();
                  break;
               case 6:
                  Echo();
                  break;
               default:
                  break;
            }
         } catch (Exception e2) {
            Log($"Run Test: {e2.Message}\r\n{e2.StackTrace}");
         } finally {
            Log($"{cbSelectHardCodedTest.Text} Complete");
            Cursor.Current = Cursors.Arrow;
         }
      }

      private void CreateComplex() {
         ClearViews();
         message = new IJPMessage();
         message.AddColumn();
         message.SetRow(0, 3);
         message.AddColumn();
         message.SetRow(1, 2);
         for (int i = 0; i < message.Items.Count; i++) {
            message.Items[i].Text = $"Item #{i + 1}";
         }
         message.InkDropUse = 2; // Missing from documentation example
         // Set to IJP.
      }

      private void CreateMessage() {
         ClearViews();
         message = new IJPMessage();
         message.AddColumn();
         message.Items[0].Text = "ABC";
         //message.Items[0].Bold = 5;
         message.InkDropUse = 2; // Missing from documentation example
         // Set to IJP.
         ijp.SetMessage(message);
      }

      private void NewMessage() {
         message = new IJPMessage();
         ClearViews();
      }

      private void ClearViews() {
         tvIJPLibTree.Nodes.Clear();
         tvXMLTree.Nodes.Clear();
         txtIjpIndented.Text = string.Empty;
         txtXMLIndented.Text = string.Empty;
      }

      private void RetrieveMessage() {
         ClearViews();
         message = (IJPMessage)ijp.GetMessage();
      }

      private void SendMessage() {
         ijp.SetMessage(message);
      }

      private void Echo() {
         message = (IJPMessage)this.ijp.GetMessage();
         Log("Message Retrieved");
         ijp.SetMessage(message);
      }

      private void ClearDisplay() {
         ClearViews();
         message = new IJPMessage();
         message.InkDropUse = 2; // Missing from documentation example
         message.AddColumn();
         message.AddItem();
         message.Items[0].Text = "X";
         //ijp.SetMessage(message);
      }

      #endregion

   }
}
