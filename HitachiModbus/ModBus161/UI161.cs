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

      Encoding encode = Encoding.GetEncoding("ISO-8859-1");

      Modbus p;

      // Used to manage dropdowns
      string[] ccNames;
      string[] ccNamesSorted;
      int[] ccValues;

      string[] attrNames;
      string[] attrNamesSorted;
      int[] attValues;

      AttrData attr;

      #endregion

      #region Constructors an destructors

      public UI161() {
         InitializeComponent();
         p = new Modbus();
         p.Log += Modbus_Log;
         ccNames = Enum.GetNames(typeof(ClassCode));
         ccNamesSorted = Enum.GetNames(typeof(ClassCode));
         Array.Sort(ccNamesSorted);
         ccValues = (int[])Enum.GetValues(typeof(ClassCode));
         cbClass.Items.AddRange(ccNamesSorted);
         SetButtonEnables();
      }

      private void LoadDropDowns() {
      }

      enum FunctionCode {
         WriteMultiple = 0x10,
         WriteSingle = 0x06,
         ReadHolding = 0x03,
         ReadInput = 0x04,
      }

      #endregion

      #region Form Control Events

      // Connect to printer and turn COM on
      private void cmdConnect_Click(object sender, EventArgs e) {
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

      // Read data from the printer
      private void cmdReadData_Click(object sender, EventArgs e) {
         if (int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out int addr)
            && int.TryParse(txtDataLength.Text, out int len)) {
            p.GetAttribute(addr, len, optHoldingRegister.Checked, out byte[] data);
            txtData.Text = p.byte_to_string(data);
         }
         SetButtonEnables();
      }

      // Send data to the printer
      private void cmdWriteData_Click(object sender, EventArgs e) {
         if (int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out int addr)
            && int.TryParse(txtDataLength.Text, out int len)
            && txtData.Text.Length > 0) {
            byte[] data = p.string_to_byte(txtData.Text);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(addr, data);
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
         lstMessages.Items.Add(msg);
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
      }

      #endregion

      // Just playing around to see how things work
      private void cmdExperiment_Click(object sender, EventArgs e) {
         int lineCount;
         int n = 0;
         string text = "Hello World";
         List<int> cols = new List<int>();            // Holds the number of rows in each column
         List<string> spacing = new List<string>();   // Holds the line spacing
         int itemCount = p.GetDecAttribute(ccIDX.Number_Of_Items);
         while (n < itemCount) {
            cols.Add(lineCount = p.GetDecAttribute(ccPF.Line_Count, n));
            spacing.Add(p.GetHRAttribute(ccPF.Line_Spacing, n));
            n += lineCount;
         }

         for (int i = 0; i < cols.Count - 1; i++) {
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccPF.Delete_Column, cols.Count - i);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }

         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccPF.Column, 1);
         p.SetAttribute(ccPF.Line, 2);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccPF.Add_Column, 2);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccPF.Column, 3);
         p.SetAttribute(ccPF.Line, 1);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         //p.SetAttribute(ccPF.Add_Column, 0);
         p.SetAttribute(ccPF.Column, 5);
         p.SetAttribute(ccPF.Line, 2);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         //p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         //p.SetAttribute(ccPF.Dot_Matrix, "5X7");
         //p.SetAttribute(ccIDX.Characters_per_Item, 0, text.Length);
         //p.SetAttribute(ccPF.Print_Character_String, 0, text);
         //p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         //p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         //p.SetAttribute(ccPF.Line_Count, 0, 3);
         //p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         //p.SetAttribute(ccIDX.Characters_per_Item, 0, text.Length);
         //p.SetAttribute(ccPF.Print_Character_String, 0, text);
         //p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);


      }

   }

}
