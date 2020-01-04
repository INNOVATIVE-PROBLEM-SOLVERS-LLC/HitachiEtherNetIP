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

      // Nozzle selection for Twin-Nozzle printers
      public enum Nozzle {
         Printer = 0,
         Nozzle1 = 1,
         Nozzle2 = 2,
         Both = 3,
      }

      // Single instance of the printer
      private Modbus p;

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

      #region Constructors an destructors

      // Constructor
      public UI161() {
         InitializeComponent();

         // Instantiate Modbus printer and register for log events
         p = new Modbus();
         p.Log += Modbus_Log;

         // Initialize all dropdowns
         ccNames = Enum.GetNames(typeof(ClassCode));
         ccNamesSorted = Enum.GetNames(typeof(ClassCode));
         Array.Sort(ccNamesSorted);
         ccValues = (int[])Enum.GetValues(typeof(ClassCode));
         cbClass.Items.AddRange(ccNamesSorted);

         // Ready to go
         SetButtonEnables();
      }

      #endregion

      #region Form Control Events

      // Connect to printer and turn COM on
      private void cmdConnect_Click(object sender, EventArgs e) {
         p.TwinNozzle = chkTwinNozzle.Checked;
         if (p.Connect(txtIPAddress.Text, txtIPPort.Text)) {

         }
         SetButtonEnables();
      }

      // Disconnect from the printer
      private void cmdDisconnect_Click(object sender, EventArgs e) {
         p.Disconnect();
         SetButtonEnables();
      }

      // Turn com on
      private void cmdComOn_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Online_Offline, 1);
         SetButtonEnables();
      }

      // Turn com off
      private void cmdComOff_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Online_Offline, 0);
         SetButtonEnables();
      }

      private void cmdReset_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.ClearFault);
         SetButtonEnables();
      }

      private void cmdGetStatus_Click(object sender, EventArgs e) {

         SetButtonEnables();
      }

      private void cmdShutDown_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.Stop);
         SetButtonEnables();
      }

      private void cmdStartUp_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.Start);
         SetButtonEnables();
      }

      private void cmdStandby_Click(object sender, EventArgs e) {
         p.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.StandBy);
         SetButtonEnables();
      }

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
         lstMessages.Items.Clear();
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
         // Place any test code here
      }

      // Show I/O packets in Log File.
      private void chkLogIO_CheckedChanged(object sender, EventArgs e) {
         p.LogIOs = chkLogIO.Checked;
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
         string s = "";
         for (int i = 0; i < msg.Length;i++) {
            char c = msg[i];
            if (c >= 0x100) {
               s += $"<{c >> 8:X2}><{c & 0xFF:X2}>";
            } else {
               s += msg.Substring(i, 1);
            }
         }
         lstMessages.Items.Add(s);
         lstMessages.SelectedIndex = lstMessages.Items.Count - 1;
         lstMessages.Update();
      }

      // Log messages generated by modbus
      private void Modbus_Log(Modbus sender, string msg) {
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

      // Avoid extra tests by enabling only the buttons that can be used
      private void SetButtonEnables() {
         int addr;
         int len;
         bool isConnected = p == null ? false : p.IsConnected;
         bool comIsOn = isConnected && p.ComIsOn;
         cmdConnect.Enabled = !isConnected;
         cmdDisconnect.Enabled = isConnected;
         cmdComOff.Enabled = comIsOn;
         cmdComOn.Enabled = isConnected && !comIsOn;
         cmdStartUp.Enabled = comIsOn;
         cmdShutDown.Enabled = comIsOn;
         cmdReady.Enabled = comIsOn;
         cmdStandby.Enabled = comIsOn;
         cmdGetStatus.Enabled = comIsOn;
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
      }

      #endregion

   }

}
