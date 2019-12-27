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

      Modbus modbus;

      #endregion

      #region Constructors an destructors

      public UI161() {
         InitializeComponent();
         modbus = new Modbus();
         modbus.Log += Modbus_Log;
      }

      private void Modbus_Log(Modbus sender, string msg) {
         Log(msg);
      }

      enum FunctionCode {
         WriteMultiple = 0x10,
         WriteSingle = 0x06,
         ReadHolding = 0x03,
         ReadInput = 0x04,
      }

      #endregion

      #region Form Level Events

      #endregion

      #region Form Control Events

      // Connect to printer and turn COM on
      private void cmdConnect_Click(object sender, EventArgs e) {
         if (modbus.Connect(txtIPAddress.Text, txtIPPort.Text)) {

         }
      }

      // Turn com on
      private void comOn_Click(object sender, EventArgs e) {
         modbus.SetAttribute(ccIJP.Online_Offline, 1);
      }

      // Turn com off
      private void cmdComOff_Click(object sender, EventArgs e) {
         modbus.SetAttribute(ccIJP.Online_Offline, 0);
      }

      // Read data from the printer
      private void cmdReadData_Click(object sender, EventArgs e) {
         if (int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out int addr)
            && int.TryParse(txtDataLength.Text, out int len)) {
            modbus.GetAttribute(addr, len, optHoldingRegister.Checked, out byte[] data);
         }
      }

      // Send data to the printer
      private void cmdWriteData_Click(object sender, EventArgs e) {
         if (int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out int addr)
            && int.TryParse(txtDataLength.Text, out int len)
            && txtData.Text.Length > 0) {
            byte[] data = modbus.string_to_byte(txtData.Text);
            modbus.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            modbus.SetAttribute(addr, data);
            modbus.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }
      }

      // Retrieve message from printer and convert to XML
      private void cmdRetrieve_Click(object sender, EventArgs e) {
         RetrieveXML retrieve = new RetrieveXML(this, modbus);
         string xml = retrieve.Retrieve();
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

      // Exit the program
      private void cmdExit_Click(object sender, EventArgs e) {
         this.Close();
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

      public void Log(string msg) {
         lstMessages.Items.Add(msg);
         lstMessages.SelectedIndex = lstMessages.Items.Count - 1;
         lstMessages.Update();
      }

      #endregion

      // Clear the task log
      private void cmLogClear_Click(object sender, EventArgs e) {
         lstMessages.Items.Clear();
      }

      // View the task log in NotePad
      private void cmLogToNotepad_Click(object sender, EventArgs e) {
         string ViewFilename = @"c:\Temp\Err.txt";
         File.WriteAllLines(ViewFilename, lstMessages.Items.Cast<string>().ToArray());
         Process.Start("notepad.exe", ViewFilename);
      }

      private void cmdReformat_Click(object sender, EventArgs e) {

         string RFN = @"c:\temp\Reformat.txt";
         StreamWriter RFS = new StreamWriter(RFN, false, Encoding.UTF8);

         Modbus.M161.ReformatTables(RFS);

         RFS.Flush();
         RFS.Close();
         Process.Start("notepad.exe", RFN);
      }
   }

}
