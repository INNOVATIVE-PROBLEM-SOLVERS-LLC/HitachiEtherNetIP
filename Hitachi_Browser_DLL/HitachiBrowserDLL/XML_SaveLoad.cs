using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EIP_Lib {

   public partial class XML {

      #region XML Driver Routines

      private void cbAvailableTests_SelectedIndexChanged(object sender, EventArgs e) {
         if (cbAvailableXmlTests.SelectedIndex >= 0) {
            try {
               string fileName = Path.Combine(parent.MessageFolder, cbAvailableXmlTests.Text + ".XML");
               XMLFileName = fileName;
               ProcessLabel(File.ReadAllText(fileName));
            } catch {
               Clear_Click(null, null);
            }
            SetButtonEnables();
         }
      }

      #endregion

      #region XML to/from Printer Routines

      // Send xlmDoc from display to printer
      private void SendDisplayToPrinter_Click(object sender, EventArgs e) {
         xmlDoc = new XmlDocument() { PreserveWhitespace = true };
         xmlDoc.LoadXml(txtIndentedView.Text);
         SendFileToPrinter_Click(null, null);
      }

      // Send xlmDoc from file to printer
      private void SendFileToPrinter_Click(object sender, EventArgs e) {
         bool success = true;
         // Need a XMP Document to continue
         if (xmlDoc == null) {
            Open_Click(null, null);
         }
         if (xmlDoc != null) {
            if (chkSerialize.Checked) {
               success = EIP.SendFileAsSerialization(XMLFileName, chkAutoReflect.Checked);
            } else {
               success = EIP.SendXmlToPrinter(xmlDoc, chkAutoReflect.Checked);
            }
         }
         if (success) {
            EIP.LogIt("Load Successful!");
         } else {
            EIP.LogIt("Load Failed!");
         }
      }

      // Generate an XML Doc from the printer contents
      private void Retrieve_Click(object sender, EventArgs e) {
         if (chkSerialize.Checked) {
            XMLText = EIP.RetrieveXMLAsSerialization();
         } else {
            XMLText = EIP.RetrieveXML();
         }
         ProcessLabel(XMLText);
         SetButtonEnables();
      }

      #endregion

      #region Display XML in Tree and Indented forms

      // Process an XML Label
      private bool ProcessLabel(string xml) {
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
                  txtIndentedView.Text = xml;

                  tvXML.Nodes.Clear();
                  tvXML.Nodes.Add(new TreeNode(xmlDoc.DocumentElement.Name));
                  TreeNode tNode = new TreeNode();
                  tNode = tvXML.Nodes[0];

                  AddNode(xmlDoc.DocumentElement, tNode);
                  tvXML.ExpandAll();

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

   }

}
