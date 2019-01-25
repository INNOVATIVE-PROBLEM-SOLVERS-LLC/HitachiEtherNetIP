using System;
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


      #endregion

      #region Constructors and destructors

      public XML(HitachiBrowser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
      }

      #endregion

      #region XML Routines

      internal bool SaveAsXml() {
         bool result = false;
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
               result = true;
            }
         }
         return result;
      }

      internal void SaveLayoutToXML(string filename) {

         int ItemNumber = 1;
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
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

               int itemCount = GetDecimalAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Print_Item);
               for (int i = 0; i < itemCount; i++) {
                  writer.WriteStartElement("Object"); // Start Object

                  //   writer.WriteAttributeString("Type", Enum.GetName(typeof(TPB.ItemTypes), p.ItemType));

                  writer.WriteStartElement("Font"); // Start Font
                  //writer.WriteAttributeString("HumanReadableFont", p.HumanReadableFont);
                  //writer.WriteAttributeString("EANPrefix", p.EANPrefix);
                  //writer.WriteAttributeString("BarCode", p.BarCode);
                  //writer.WriteAttributeString("IncreasedWidth", p.IncreasedWidth.ToString());
                  //writer.WriteAttributeString("InterLineSpace", p.InterLineSpace.ToString());
                  //writer.WriteAttributeString("InterCharacterSpace", p.InterCharacterSpace.ToString());
                  //writer.WriteString(p.ItemFont);
                  writer.WriteEndElement(); // End Font

                  //   writer.WriteStartElement("Location"); // Start Location
                  //   writer.WriteAttributeString("ItemNumber", (ItemNumber++).ToString());
                  //   writer.WriteAttributeString("Column", p.Column.ToString());
                  //   writer.WriteAttributeString("Row", p.Row.ToString());
                  //   writer.WriteAttributeString("Height", p.ItemHeight.ToString());
                  //   writer.WriteAttributeString("Width", (p.ItemWidth * p.IncreasedWidth).ToString());
                  //   writer.WriteAttributeString("Left", p.X.ToString());
                  //   writer.WriteAttributeString("Top", (p.Y + p.ScaledImage.Height).ToString());
                  //   writer.WriteEndElement(); // End Location

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

               }

               writer.WriteEndElement(); // End Objects
               writer.WriteEndElement(); // End Label

               writer.WriteEndDocument();
               writer.Flush();
               Stream outfs = null;
               outfs = new FileStream(filename, FileMode.Create);
               ms.WriteTo(outfs);
               outfs.Flush();
               outfs.Close();
               //Utils.Log("XML File \"" + filename + "\" Saved successfully", Severity.Normal);
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

      private void cmdBrowse_Click(string Filename) {
         DialogResult dlgResult = DialogResult.Retry;
         string fileName = String.Empty;
         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.InitialDirectory = Filename;
            dlg.Title = "Select XML formatted file!";
            dlg.Filter = "TXML (*.xml)|All (*.*)|*.*";
            dlg.DefaultExt = "txt";
            dlg.FilterIndex = 1;
            dlgResult = DialogResult.Retry;
            while (dlgResult == DialogResult.Retry) {
               dlgResult = dlg.ShowDialog();
               if (dlgResult == DialogResult.OK) {
                  try {
                     ProcessGP(File.ReadAllText(dlg.FileName));
                  } catch (Exception ex) {
                     MessageBox.Show(parent, ex.Message, "Cannot load XML File!");
                  }
               }
            }
         }
      }

      public bool ProcessGP(string xml, bool postError = true) {
         bool result = false;
         try {
            int i = xml.IndexOf("<La");
            if (i == -1) {
               xml = File.ReadAllText(xml);
               i = xml.IndexOf("<La");
            }
            i = xml.IndexOf("<La");
            int j = xml.IndexOf("</Label>", i + 3);
            if (j > 0)
               xml = xml.Substring(i, j - i + 5);
            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;
            dom.LoadXml(xml);
            //xml = ToIndentedString(dom);
            xml = ToIndentedString(xml);
            i = xml.IndexOf("<GP");
            if (i > 0) {
               xml = xml.Substring(i);
               parent.txtIndentedView.Text = xml;

               parent.tvXML.Nodes.Clear();
               parent.tvXML.Nodes.Add(new TreeNode(dom.DocumentElement.Name));
               TreeNode tNode = new TreeNode();
               tNode = parent.tvXML.Nodes[0];

               AddNode(dom.DocumentElement, tNode);
               parent.tvXML.ExpandAll();

               result = true;
            }
         } catch {

         } finally {
            if (postError && !result) {
               parent.txtIndentedView.Text = "Invalid <GP> structure!";
            }
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
         int offset = (int)(tab.ClientSize.Height - tclHeight * R.H);
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         R.offset = offset;
         Utils.ResizeObject(ref R, parent.tclViewXML, 0, 0, tclHeight - 4, tclWidth);
         {
            Utils.ResizeObject(ref R, parent.tvXML, 1, 1, tclHeight - 9, tclWidth - 2, 0.9f);

            Utils.ResizeObject(ref R, parent.txtIndentedView, 1, 1, tclHeight - 9, tclWidth - 2, 0.9f);

            Utils.ResizeObject(ref R, parent.cmdBrowse, tclHeight - 3, 1, 2, 5);
            Utils.ResizeObject(ref R, parent.cmdClear, tclHeight - 3, 7, 2, 5);
            Utils.ResizeObject(ref R, parent.cmdSendToPrinter, tclHeight - 3, 13, 2, 6);
         }
         R.offset = 0;
      }

      private string GetAttribute(eipClassCode Class, byte Attribute, DataFormats fmt) {
         bool successful = EIP.ReadOneAttribute(Class, Attribute, out string val, fmt);
         return val;
      }

      private int GetDecimalAttribute(eipClassCode Class, byte Attribute) {
         bool successful = EIP.ReadOneAttribute(Class, Attribute, out string val, DataFormats.Decimal);
         return EIP.GetDecValue;
      }

      public void SetButtonEnables() {
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }

      }

      #endregion

   }

}
