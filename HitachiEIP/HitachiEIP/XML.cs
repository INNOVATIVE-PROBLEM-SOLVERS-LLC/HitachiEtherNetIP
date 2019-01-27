﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace HitachiEIP {

   public class XML {

      #region Data Declarations

      HitachiBrowser parent;
      EIP EIP;
      TabPage tab;

      string LoadedFileName;

      // Controls
      TabControl tclViewXML;
      TabPage tabTreeView;
      TabPage tabIndented;

      TreeView tvXML;
      TextBox txtIndentedView;

      Button cmdOpen;
      Button cmdClear;
      Button cmdGenerate;
      Button cmdSaveAs;
      Button cmdSendToPrinter;

      string XMLText= string.Empty;

      #endregion

      #region Constructors and destructors

      public XML(HitachiBrowser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;

         tclViewXML = new TabControl() { Name = "tclViewXML" };
         tabTreeView = new TabPage() { Name = "tabTreeView", Text = "Tree View" };
         tabIndented = new TabPage() { Name = "tabIndented", Text = "Indented View" };

         tvXML = new TreeView() { Name = "tvXML" };
         txtIndentedView = new TextBox() { Name = "txtIndentedView", Multiline = true, ScrollBars = ScrollBars.Both };

         cmdOpen = new Button() { Text = "Open" };
         cmdOpen.Click += Open_Click;

         cmdClear = new Button() { Text = "Clear" };
         cmdClear.Click += Clear_Click;

         cmdGenerate = new Button() { Text = "Generate" };
         cmdGenerate.Click += Generate_Click;

         cmdSaveAs = new Button() { Text = "Save As" };
         cmdSaveAs.Click += SaveAs_Click;

         cmdSendToPrinter = new Button() { Text = "Send To Printer", Enabled = false };
         cmdSendToPrinter.Click += SendToPrinter_Click;

         tab.Controls.Add(tclViewXML);

         tclViewXML.Controls.Add(tabTreeView);
         tclViewXML.Controls.Add(tabIndented);

         tabTreeView.Controls.Add(tvXML);
         tabIndented.Controls.Add(txtIndentedView);

         tab.Controls.Add(cmdOpen);
         tab.Controls.Add(cmdClear);
         tab.Controls.Add(cmdGenerate);
         tab.Controls.Add(cmdSaveAs);
         tab.Controls.Add(cmdSendToPrinter);
      }

      #endregion

      #region Form Control Events

      private void Open_Click(object sender, EventArgs e) {
         DialogResult dlgResult = DialogResult.Retry;
         string fileName = String.Empty;
         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.InitialDirectory = LoadedFileName;
            dlg.Title = "Select XML formatted file!";
            dlg.Filter = "XML (*.xml)|*.xml|All (*.*)|*.*";
            dlg.DefaultExt = "txt";
            dlg.FilterIndex = 1;
            dlgResult = DialogResult.Retry;
            while (dlgResult == DialogResult.Retry) {
               dlgResult = dlg.ShowDialog();
               if (dlgResult == DialogResult.OK) {
                  try {
                     ProcessLabel(File.ReadAllText(dlg.FileName));
                  } catch (Exception ex) {
                     MessageBox.Show(parent, ex.Message, "Cannot load XML File!");
                  }
               }
            }
         }
      }

      private void Clear_Click(object sender, EventArgs e) {
         txtIndentedView.Text = string.Empty;
         tvXML.Nodes.Clear();
         XMLText = string.Empty;
         SetButtonEnables();
      }

      private void Generate_Click(object sender, EventArgs e) {
         XMLText = ConvertLayoutToXML();
         ProcessLabel(XMLText);
         SetButtonEnables();
      }

      private void SaveAs_Click(object sender, EventArgs e) {
         DialogResult dlgResult;
         string filename = this.LoadedFileName;

         using (SaveFileDialog saveFileDialog1 = new SaveFileDialog()) {
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "xml";
            saveFileDialog1.Filter = "xml|*.xml";
            saveFileDialog1.Title = "Save Printer Layout to XML file";
            saveFileDialog1.FileName = filename;
            dlgResult = saveFileDialog1.ShowDialog();
            if (dlgResult == DialogResult.OK && !String.IsNullOrEmpty(saveFileDialog1.FileName)) {
               filename = saveFileDialog1.FileName;
               this.SaveLayoutToXML(filename);
            }
         }
         SetButtonEnables();
      }

      private void SaveLayoutToXML(string filename) {
         Stream outfs = null;
         outfs = new FileStream(filename, FileMode.Create);
         outfs.Write(EIP.encode.GetBytes(XMLText),0,XMLText.Length);
         outfs.Flush();
         outfs.Close();
         SetButtonEnables();
      }

      private void SendToPrinter_Click(object sender, EventArgs e) {
      }

      #endregion

      #region XML Routines

      private string ConvertLayoutToXML() {
         bool OpenCloseForward = !EIP.ForwardIsOpen;
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
               OpenCloseForward = !EIP.ForwardIsOpen;
               if (OpenCloseForward) {
                  EIP.ForwardOpen();
               }
               writer.WriteStartElement("Label"); // Start Label

               //writer.WriteAttributeString("ClockSystem", this.ClockSystem);
               //writer.WriteAttributeString("Registration", this.Registration.ToString());
               //writer.WriteAttributeString("GroupNumber", this.MessageGroupNumber);
               //writer.WriteAttributeString("GroupName", this.MessageGroup);
               //writer.WriteAttributeString("Name", this.MessageName);
               //writer.WriteAttributeString("BeRestrictive", this.BeRestrictive.ToString());
               //writer.WriteAttributeString("UseHalfSpace", this.UseHalfSpace.ToString());
               //writer.WriteAttributeString("Format", MessageStyle.ToString());

               writer.WriteAttributeString("Version", "3");
               WritePrinterSettings(writer);

               writer.WriteStartElement("Objects"); // Start Objects
               if (EIP.ForwardIsOpen && OpenCloseForward) {
                  EIP.ForwardClose();
               }

               int itemCount = GetDecimalAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Print_Item);
               for (int i = 0; i < itemCount; i++) {
                  OpenCloseForward = !EIP.ForwardIsOpen;
                  if (OpenCloseForward) {
                     EIP.ForwardOpen();
                  }

                  SetAttribute(eipClassCode.Index, (byte)eipIndex.Item_Count, i + 1);
                  writer.WriteStartElement("Object"); // Start Object

                  //   writer.WriteAttributeString("Type", Enum.GetName(typeof(TPB.ItemTypes), p.ItemType));

                  writer.WriteStartElement("Font"); // Start Font
                  writer.WriteAttributeString("HumanReadableFont", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Readable_Code));
                  writer.WriteAttributeString("EANPrefix", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Prefix_Code));
                  writer.WriteAttributeString("BarCode", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Barcode_Type));
                  writer.WriteAttributeString("IncreasedWidth", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Character_Bold));
                  writer.WriteAttributeString("InterLineSpace", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Line_Spacing));
                  writer.WriteAttributeString("InterCharacterSpace", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.InterCharacter_Space));
                  writer.WriteString(GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Dot_Matrix));
                  writer.WriteEndElement(); // End Font

                  writer.WriteStartElement("Location"); // Start Location
                  writer.WriteAttributeString("ItemNumber", (i + 1).ToString());
                  //   writer.WriteAttributeString("Column", p.Column.ToString());
                  //   writer.WriteAttributeString("Row", p.Row.ToString());
                  //   writer.WriteAttributeString("Height", p.ItemHeight.ToString());
                  //   writer.WriteAttributeString("Width", (p.ItemWidth * p.IncreasedWidth).ToString());
                  //   writer.WriteAttributeString("Left", p.X.ToString());
                  //   writer.WriteAttributeString("Top", (p.Y + p.ScaledImage.Height).ToString());
                  writer.WriteEndElement(); // End Location
                  writer.WriteElementString("Text", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Print_Character_String));

                  //   switch (p.ItemType) {
                  //      case TPB.ItemTypes.Counter:
                  //         writer.WriteStartElement("Counter"); // Start Counter
                  //         writer.WriteAttributeString("Reset", p.CtReset);
                  //         writer.WriteAttributeString("ExternalSignal", p.CtExternalSignal);
                  //         writer.WriteAttributeString("ResetSignal", p.CtResetSignal);
                  //         writer.WriteAttributeString("Variable", p.WlxVariableName);
                  //         writer.WriteAttributeString("CountUp", p.CtDirection.ToString());
                  //         writer.WriteAttributeString("Increment", p.CtIncrement);
                  //         writer.WriteAttributeString("JumpTo", p.CtJumpTo);
                  //         writer.WriteAttributeString("JumpFrom", p.CtJumpFrom);
                  //         writer.WriteAttributeString("UpdateUnit", p.CtUpdateUnit);
                  //         writer.WriteAttributeString("UpdateIP", p.CtUpdateIP);
                  //         writer.WriteAttributeString("Range2", p.CtRangeEnd);
                  //         writer.WriteAttributeString("Range1", p.CtRangeStart);
                  //         writer.WriteAttributeString("InitialValue", p.CtInitialValue);
                  //         writer.WriteAttributeString("Format", p.RawText);
                  //         writer.WriteAttributeString("Multiplier", p.CtMultiplier);
                  //         writer.WriteAttributeString("ZeroSuppression", p.CtZeroSuppression.ToString());
                  //         writer.WriteEndElement(); //  End Counter

                  //         writer.WriteElementString("Text", p.RawText);
                  //         break;
                  //      case TPB.ItemTypes.Date:
                  //         writer.WriteStartElement("Date"); // Start Date
                  //         writer.WriteAttributeString("Variable", p.WlxVariableName);
                  //         writer.WriteAttributeString("WindowsFormat", (!p.DtWillettFormat).ToString());
                  //         //writer.WriteAttributeString("Format", p.RawText);

                  //         writer.WriteStartElement("Offset"); // Start Offset
                  //         writer.WriteAttributeString("Minute", p.DtMinuteOffset);
                  //         writer.WriteAttributeString("Hour", p.DtHourOffset);
                  //         writer.WriteAttributeString("Day", p.DtDayOffset);
                  //         writer.WriteAttributeString("Month", p.DtMonthOffset);
                  //         writer.WriteAttributeString("Year", p.DtYearOffset);
                  //         writer.WriteEndElement(); // End Offset

                  //         writer.WriteStartElement("ZeroSuppress"); // Start ZeroSuppress
                  //         writer.WriteAttributeString("DayOfWeek", p.DtDayOfWeekZS.ToString());
                  //         writer.WriteAttributeString("Week", p.DtWeekZS.ToString());
                  //         writer.WriteAttributeString("Minute", p.DtMinuteZS.ToString());
                  //         writer.WriteAttributeString("Hour", p.DtHourZS.ToString());
                  //         writer.WriteAttributeString("Day", p.DtDayZS.ToString());
                  //         writer.WriteAttributeString("Month", p.DtMonthZS.ToString());
                  //         writer.WriteAttributeString("Year", p.DtYearZS.ToString());
                  //         writer.WriteEndElement(); // End ZeroSuppress

                  //         writer.WriteStartElement("EnableSubstitution"); // Start ZeroSuppress
                  //         writer.WriteAttributeString("SubstitutionRule", p.DTSubRule);
                  //         writer.WriteAttributeString("DayOfWeek", p.DtDayOfWeekSub.ToString());
                  //         writer.WriteAttributeString("Week", p.DtWeekSub.ToString());
                  //         writer.WriteAttributeString("Minute", p.DtMinuteSub.ToString());
                  //         writer.WriteAttributeString("Hour", p.DtHourSub.ToString());
                  //         writer.WriteAttributeString("Day", p.DtDaySub.ToString());
                  //         writer.WriteAttributeString("Month", p.DtMonthSub.ToString());
                  //         writer.WriteAttributeString("Year", p.DtYearSub.ToString());
                  //         writer.WriteEndElement(); // End EnableSubstitution

                  //         writer.WriteEndElement(); // End Date

                  //         writer.WriteElementString("Text", p.RawText);
                  //         break;
                  //      case TPB.ItemTypes.Logo:
                  //         writer.WriteStartElement("Logo"); // Start Logo
                  //         writer.WriteAttributeString("Variable", p.WlxVariableName);
                  //         writer.WriteAttributeString("HAlignment", Enum.GetName(typeof(Utils.HAlignment), p.LogoHAlignment));
                  //         writer.WriteAttributeString("VAlignment", Enum.GetName(typeof(Utils.VAlignment), p.LogoVAlignment));
                  //         writer.WriteAttributeString("Filter", p.LogoFilter.ToString());
                  //         writer.WriteAttributeString("ReverseVideo", p.LogoReverseVideo.ToString());
                  //         writer.WriteAttributeString("Source", p.LogoSource);
                  //         writer.WriteAttributeString("LogoLength", p.LogoLength.ToString());
                  //         writer.WriteAttributeString("Registration", p.LogoRegistration.ToString());

                  //         using (MemoryStream ms2 = new MemoryStream()) {
                  //            // bm.Save does not like transparent pixels so make them white
                  //            Bitmap bm2 = new Bitmap(p.ItemWidth, p.ItemHeight);
                  //            using (Graphics g = Graphics.FromImage(bm2)) {
                  //               g.Clear(Color.White);
                  //               for (int x = 0; x < p.ItemWidth; x++) {
                  //                  for (int y = 0; y < p.ItemHeight; y++) {
                  //                     if (p.ScaledImage.GetPixel(x, y).ToArgb() != 0) {
                  //                        bm2.SetPixel(x, y, Color.Black);
                  //                     }
                  //                  }
                  //               }
                  //            }
                  //            Bitmap bm = Utils.BitmapTo1Bpp(bm2);
                  //            bm.Save(ms2, ImageFormat.Bmp);
                  //            using (BinaryReader br = new BinaryReader(ms2)) {
                  //               ms2.Position = 0;
                  //               byte[] b = br.ReadBytes((int)ms2.Length);
                  //               int length = Utils.LittleEndian(b, 2, 4);
                  //               string data = "";
                  //               writer.WriteAttributeString("size", length.ToString());
                  //               for (int j = 0; j < length; j++) {
                  //                  data = data + string.Format("{0:X2} ", b[j]);
                  //               }
                  //               writer.WriteStartElement("Data"); // Start Data
                  //               writer.WriteAttributeString("Length", length.ToString());
                  //               writer.WriteString(data.TrimEnd());
                  //               writer.WriteEndElement(); // End Data
                  //            }
                  //         }
                  //         writer.WriteEndElement(); // End Logo
                  //         string LogoText = "";
                  //         //for (int j = 0; j < p.ItemText.Length; j++) {
                  //         //   LogoText += ((short)p.ItemText[j]).ToString("X4");
                  //         //}
                  //         writer.WriteElementString("Text", LogoText);
                  //         break;
                  //      case TPB.ItemTypes.Text:
                  //         writer.WriteElementString("Text", p.RawText);
                  //         break;
                  //      case TPB.ItemTypes.Link:
                  //         writer.WriteStartElement("Link"); // Start Link
                  //         writer.WriteAttributeString("Variable", p.WlxVariableName);
                  //         writer.WriteAttributeString("Location", p.FileLocation);
                  //         writer.WriteEndElement(); // End Link
                  //         writer.WriteElementString("Text", p.RawText);
                  //         break;
                  //      case TPB.ItemTypes.Prompt:
                  //         writer.WriteStartElement("Prompt"); // Start Prompt
                  //         writer.WriteAttributeString("Variable", p.WlxVariableName);
                  //         writer.WriteAttributeString("Message", p.PromptMessage);
                  //         writer.WriteAttributeString("Responses", p.PromptResponses);
                  //         writer.WriteAttributeString("AssumedResponse", p.PromptAssumedResponse);
                  //         writer.WriteEndElement(); // End Prompt
                  //         writer.WriteElementString("Text", p.RawText);
                  //         break;
                  //   }
                  writer.WriteEndElement(); // End Object

                  if (EIP.ForwardIsOpen && OpenCloseForward) {
                     EIP.ForwardClose();
                  }
               }

               writer.WriteEndElement(); // End Objects
               writer.WriteEndElement(); // End Label

               writer.WriteEndDocument();
               writer.Flush();
               ms.Position = 0;
               return new StreamReader(ms).ReadToEnd();
            }
         }
      }

      private void WritePrinterSettings(XmlTextWriter writer) {

         writer.WriteStartElement("Printer");
         // Write it out but do not load it back
         //writer.WriteAttributeString("Model", this.PXRType);
         //writer.WriteAttributeString("Make", "Hitachi");

         //writer.WriteStartElement("PrintHead");
         //writer.WriteAttributeString("Orientation", this.CharacterOrientation);
         //writer.WriteEndElement(); // PrintHead

         //writer.WriteStartElement("ContinuousPrinting");
         //writer.WriteAttributeString("RepeatInterval", this.RepeatInterval);
         //writer.WriteAttributeString("PrintsPerTrigger", this.PrintsPerTrigger);
         //writer.WriteEndElement(); // ContinuousPrinting

         //writer.WriteStartElement("TargetSensor");
         //writer.WriteAttributeString("Filter", this.TargetSensorFilter);
         //writer.WriteAttributeString("SetupValue", this.TargetSensorSetupValue);
         //writer.WriteAttributeString("Timer", this.TargetSensorTimer);
         //writer.WriteEndElement(); // TargetSensor

         //writer.WriteStartElement("CharacterSize");
         //writer.WriteAttributeString("Height", this.CharacterHeight);
         //writer.WriteAttributeString("Width", this.CharacterWidth);
         //writer.WriteEndElement(); // CharacterSize

         //writer.WriteStartElement("PrintStartDelay");
         //writer.WriteAttributeString("Reverse", this.ReverseDelay);
         //writer.WriteAttributeString("Forward", this.ForwardDelay);
         //writer.WriteEndElement(); // PrintStartDelay

         //writer.WriteStartElement("EncoderSettings");
         //writer.WriteAttributeString("HighSpeedPrinting", this.HighSpeedPrinting);
         //writer.WriteAttributeString("Divisor", this.Divisor);
         //writer.WriteAttributeString("ExternalEncoder", this.ExternalEncoder.ToString());
         //writer.WriteEndElement(); // EncoderSettings

         //writer.WriteStartElement("InkStream");
         //writer.WriteAttributeString("InkDropUse", this.InkDropUse);
         //writer.WriteAttributeString("ChargeRule", this.InkDropChargeRule); // InkDropChargeRule presents as ChargeRule in message .xml file
         //writer.WriteEndElement(); // InkStream

         //writer.WriteStartElement("TwinNozzle");
         //writer.WriteAttributeString("LeadingCharControl", this.LeadingCharacterControl.ToString());
         //writer.WriteAttributeString("LeadingCharControlWidth1", this.LeadingCharacterControlWidth1.ToString());
         //writer.WriteAttributeString("LeadingCharControlWidth2", this.LeadingCharacterControlWidth2.ToString());
         //writer.WriteAttributeString("NozzleSpaceAlignment", this.NozzleSpaceAlignment.ToString());
         //writer.WriteEndElement(); // TwinNozzle


         writer.WriteEndElement(); // Printer
      }

      private bool ProcessLabel(string xml) {
         bool result = false;
         try {
            int i = xml.IndexOf("<Label");
            if (i == -1) {
               xml = File.ReadAllText(xml);
               i = xml.IndexOf("<Label");
            }
            i = xml.IndexOf("<Label");
            int j = xml.IndexOf("</Label>", i + 7);
            if (j > 0)
               xml = xml.Substring(i, j - i + 8);
            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;
            dom.LoadXml(xml);
            xml = ToIndentedString(xml);
            i = xml.IndexOf("<Label");
            if (i > 0) {
               xml = xml.Substring(i);
               txtIndentedView.Text = xml;

               tvXML.Nodes.Clear();
               tvXML.Nodes.Add(new TreeNode(dom.DocumentElement.Name));
               TreeNode tNode = new TreeNode();
               tNode = tvXML.Nodes[0];

               AddNode(dom.DocumentElement, tNode);
               tvXML.ExpandAll();

               result = true;
            }
         } catch {

         }
         return result;
      }

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

      private string GetNameAttr(XmlNode n) {
         string result = n.Name;
         if (n.Attributes.Count > 0) {
            foreach (XmlAttribute attribute in n.Attributes) {
               result += " " + attribute.Name + "=\"" + attribute.Value + "\"";
            }
         }
         return result;
      }

      #endregion

      #region Service Routines

      public void ResizeControls(ref ResizeInfo R) {
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         int tclWidth = (int)(tab.ClientSize.Width / R.W);
         float offset = (int)(tab.ClientSize.Height - tclHeight * R.H);
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         R.offset = offset;
         Utils.ResizeObject(ref R, tclViewXML, 0, 1, tclHeight - 4, tclWidth - 1);
         {
            Utils.ResizeObject(ref R, tvXML, 1, 1, tclHeight - 9, tclWidth - 3);

            Utils.ResizeObject(ref R, txtIndentedView, 1, 1, tclHeight - 9, tclWidth - 3);

            Utils.ResizeObject(ref R, cmdOpen, tclHeight - 3, 1, 2, 6);
            Utils.ResizeObject(ref R, cmdClear, tclHeight - 3, 8, 2, 6);
            Utils.ResizeObject(ref R, cmdGenerate, tclHeight - 3, 15, 2, 6);
            Utils.ResizeObject(ref R, cmdSaveAs, tclHeight - 3, 22, 2, 6);
            Utils.ResizeObject(ref R, cmdSendToPrinter, tclHeight - 3, 29, 2, 6);
         }
         R.offset = 0;
      }

      private string GetAttribute(eipClassCode Class, byte Attribute) {
         bool successful = EIP.ReadOneAttribute(Class, Attribute, out string val, Data.AttrDict[(byte)Class, Attribute].Fmt);
         return val;
      }

      private int GetDecimalAttribute(eipClassCode Class, byte Attribute) {
         bool successful = EIP.ReadOneAttribute(Class, Attribute, out string val, DataFormats.Decimal);
         return EIP.GetDecValue;
      }

      private int SetAttribute(eipClassCode Class, byte Attribute, int n) {
         bool successful = EIP.WriteOneAttribute(Class, Attribute, EIP.ToBytes((uint)n, Data.AttrDict[(byte)Class, Attribute].Len));
         return EIP.GetDecValue;
      }

      public void SetButtonEnables() {
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         cmdSaveAs.Enabled = XMLText.Length > 0;
      }

      #endregion

   }

}
