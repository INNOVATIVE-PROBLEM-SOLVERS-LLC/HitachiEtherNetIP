using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EIP_Lib {

   public partial class XML {

      // Generate an XML Doc from the printer contents
      private void Generate_Click(object sender, EventArgs e) {
         XMLText = ConvertLayoutToXML();
         ProcessLabel(XMLText);
         SetButtonEnables();
      }

      private void cbAvailableTests_SelectedIndexChanged(object sender, EventArgs e) {
         if (cbAvailableXmlTests.SelectedIndex >= 0) {
            try {
               string fileName = Path.Combine(parent.MessageFolder, cbAvailableXmlTests.Text + ".XML");
               ProcessLabel(File.ReadAllText(fileName));
            } catch {
               Clear_Click(null, null);
            }
            SetButtonEnables();
         }
      }

      #region Retrieve Settings from printer as XML

      // Generate an XMP Doc form the current printer settings
      private string ConvertLayoutToXML() {
         success = true;
         ItemType itemType = ItemType.Text;
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
               if (EIP.StartSession()) {
                  if (EIP.ForwardOpen()) {
                     writer.WriteStartElement("Label"); // Start Label
                     {
                        writer.WriteAttributeString("Version", "1");
                        WritePrinterSettings(writer);
                        writer.WriteStartElement("Objects"); // Start Objects
                        {
                           int item = 0;
                           int colCount = GetDecimalAttribute(ccPF.Number_Of_Columns);
                           for (int col = 1; col <= colCount; col++) {
                              success = success && EIP.SetAttribute(ccIDX.Column, col);
                              int LineCount = GetDecimalAttribute(ccPF.Line_Count);
                              for (int row = LineCount; row > 0; row--) {
                                 success = success && EIP.SetAttribute(ccIDX.Item, ++item);
                                 string text = GetAttribute(ccPF.Print_Character_String);
                                 int[] mask = new int[1 + Math.Max(
                                       GetDecimalAttribute(ccCal.Number_of_Calendar_Blocks),
                                       GetDecimalAttribute(ccCount.Number_Of_Count_Blocks))];
                                 itemType = GetItemType(text, ref mask);
                                 writer.WriteStartElement("Object"); // Start Object
                                 {
                                    writer.WriteAttributeString("Type", Enum.GetName(typeof(ItemType), itemType));

                                    WriteFont(writer);

                                    WriteLocation(writer, item, row, col);

                                    switch (itemType) {
                                       case ItemType.Text:
                                          break;
                                       case ItemType.Date:
                                          // Missing multiple calendar block logic
                                          WriteCalendarSettings(writer, mask);
                                          break;
                                       case ItemType.Counter:
                                          // Missing multiple counter block logic
                                          WriteCounterSettings(writer);
                                          break;
                                       case ItemType.Logo:
                                          WriteUserPatternSettings(writer);
                                          break;
                                       default:
                                          break;
                                    }

                                    writer.WriteElementString("Text", text);
                                 }
                                 writer.WriteEndElement(); // End Object
                              }
                           }
                        }
                        writer.WriteEndElement(); // End Objects
                     }
                     writer.WriteEndElement(); // End Label
                  }
                  EIP.ForwardClose();
               }
               EIP.EndSession();
               writer.WriteEndDocument();
               writer.Flush();
               ms.Position = 0;
               return new StreamReader(ms).ReadToEnd();
            }
         }
      }

      // Write the global printyer settings
      private void WritePrinterSettings(XmlTextWriter writer) {

         writer.WriteStartElement("Printer");
         {
            {
               writer.WriteAttributeString("Model", GetAttribute(ccUI.Model_Name));
            }
            writer.WriteAttributeString("Make", "Hitachi");

            writer.WriteStartElement("PrintHead");
            {
               writer.WriteAttributeString("Orientation", GetAttribute(ccPS.Character_Orientation));
            }
            writer.WriteEndElement(); // PrintHead

            writer.WriteStartElement("ContinuousPrinting");
            {
               writer.WriteAttributeString("RepeatInterval", GetAttribute(ccPS.Repeat_Interval));
               writer.WriteAttributeString("PrintsPerTrigger", GetAttribute(ccPS.Repeat_Count));
            }
            writer.WriteEndElement(); // ContinuousPrinting

            writer.WriteStartElement("TargetSensor");
            {
               writer.WriteAttributeString("Filter", GetAttribute(ccPS.Target_Sensor_Filter));
               writer.WriteAttributeString("SetupValue", GetAttribute(ccPS.Targer_Sensor_Filter_Value));
               writer.WriteAttributeString("Timer", GetAttribute(ccPS.Target_Sensor_Timer));
            }
            writer.WriteEndElement(); // TargetSensor

            writer.WriteStartElement("CharacterSize");
            {
               writer.WriteAttributeString("Height", GetAttribute(ccPS.Character_Width));
               writer.WriteAttributeString("Width", GetAttribute(ccPS.Character_Height));
            }
            writer.WriteEndElement(); // CharacterSize

            writer.WriteStartElement("PrintStartDelay");
            {
               writer.WriteAttributeString("Reverse", GetAttribute(ccPS.Print_Start_Delay_Forward));
               writer.WriteAttributeString("Forward", GetAttribute(ccPS.Print_Start_Delay_Reverse));
            }
            writer.WriteEndElement(); // PrintStartDelay

            writer.WriteStartElement("EncoderSettings");
            {
               writer.WriteAttributeString("HighSpeedPrinting", GetAttribute(ccPS.High_Speed_Print));
               writer.WriteAttributeString("Divisor", GetAttribute(ccPS.Pulse_Rate_Division_Factor));
               writer.WriteAttributeString("ExternalEncoder", GetAttribute(ccPS.Product_Speed_Matching));
            }
            writer.WriteEndElement(); // EncoderSettings

            writer.WriteStartElement("InkStream");
            {
               writer.WriteAttributeString("InkDropUse", GetAttribute(ccPS.Ink_Drop_Use));
               writer.WriteAttributeString("ChargeRule", GetAttribute(ccPS.Ink_Drop_Charge_Rule));
            }
            writer.WriteEndElement(); // InkStream

            writer.WriteStartElement("TwinNozzle");
            {
               //writer.WriteAttributeString("LeadingCharControl", this.LeadingCharacterControl.ToString());
               //writer.WriteAttributeString("LeadingCharControlWidth1", this.LeadingCharacterControlWidth1.ToString());
               //writer.WriteAttributeString("LeadingCharControlWidth2", this.LeadingCharacterControlWidth2.ToString());
               //writer.WriteAttributeString("NozzleSpaceAlignment", this.NozzleSpaceAlignment.ToString());
            }
            writer.WriteEndElement(); // TwinNozzle
         }
         writer.WriteEndElement(); // Printer
      }

      // Examine the contents of a print message to determine its type
      private ItemType GetItemType(string text, ref int[] mask) {
         int l = 0;
         mask[l] = 0;
         string[] s = text.Split('{');
         for (int i = 0; i < s.Length; i++) {
            int n = s[i].IndexOf('}');
            if (n >= 0) {
               for (int j = 0; j < n; j++) {
                  int k = Array.IndexOf(bc, s[i][j]);
                  if (k >= 0) {
                     mask[l] |= 1 << k;
                  } else {
                     mask[l] |= (int)ba.Unknown;
                  }
               }
            }
            if (s[i].IndexOf('}', n + 1) > 0) {
               l++;
            }
         }
         // Calendar and Count cannot appear in the same item
         if ((mask[0] & (int)ba.Count) > 0) {
            return ItemType.Counter;
         } else if ((mask[0] & (int)ba.DateCode) > 0) {
            return ItemType.Date;
         } else {
            return ItemType.Text;
         }
      }

      private void WriteFont(XmlTextWriter writer) {
         writer.WriteStartElement("Font"); // Start Font
         {
            string BarCode = GetAttribute(ccPF.Barcode_Type);
            writer.WriteAttributeString("BarCode", BarCode);
            if (BarCode != "None") {
               writer.WriteAttributeString("HumanReadableFont", GetAttribute(ccPF.Readable_Code));
               writer.WriteAttributeString("EANPrefix", GetAttribute(ccPF.Prefix_Code));
            }
            writer.WriteAttributeString("IncreasedWidth", GetAttribute(ccPF.Character_Bold));
            writer.WriteAttributeString("InterLineSpace", GetAttribute(ccPF.Line_Spacing));
            writer.WriteAttributeString("InterCharacterSpace", GetAttribute(ccPF.InterCharacter_Space));
            writer.WriteString(GetAttribute(ccPF.Dot_Matrix));
         }
         writer.WriteEndElement(); // End Font
      }

      private void WriteLocation(XmlTextWriter writer, int item, int row, int col) {
         writer.WriteStartElement("Location"); // Start Location
         {
            writer.WriteAttributeString("ItemNumber", item.ToString());
            writer.WriteAttributeString("Row", row.ToString());
            writer.WriteAttributeString("Column", col.ToString());
            //   writer.WriteAttributeString("Height", p.ItemHeight.ToString());
            //   writer.WriteAttributeString("Width", (p.ItemWidth * p.IncreasedWidth).ToString());
            //   writer.WriteAttributeString("Left", p.X.ToString());
            //   writer.WriteAttributeString("Top", (p.Y + p.ScaledImage.Height).ToString());
         }
         writer.WriteEndElement(); // End Location
      }

      // Output the Calendar Settings
      private void WriteCalendarSettings(XmlTextWriter writer, int[] mask) {
         bool success = true;
         int FirstBlock = 0;
         int BlockCount = 0;
         success = success && EIP.GetAttribute(ccCal.First_Calendar_Block, out FirstBlock);
         success = success && EIP.GetAttribute(ccCal.Number_of_Calendar_Blocks, out BlockCount);
         for (int i = 0; success && i < BlockCount; i++) {
            success = success && EIP.SetAttribute(ccIDX.Calendar_Block, FirstBlock + i);
            writer.WriteStartElement("Date"); // Start Date
            {
               writer.WriteAttributeString("Block", (i + 1).ToString());
               writer.WriteStartElement("Offset"); // Start Offset
               {
                  if ((mask[i] & (int)ba.Year) > 0)
                     writer.WriteAttributeString("Year", GetAttribute(ccCal.Offset_Year));
                  if ((mask[i] & (int)ba.Month) > 0)
                     writer.WriteAttributeString("Month", GetAttribute(ccCal.Offset_Month));
                  if ((mask[i] & (int)ba.Day) > 0)
                     writer.WriteAttributeString("Day", GetAttribute(ccCal.Offset_Day));
                  if ((mask[i] & (int)ba.Hour) > 0)
                     writer.WriteAttributeString("Hour", GetAttribute(ccCal.Offset_Hour));
                  if ((mask[i] & (int)ba.Minute) > 0)
                     writer.WriteAttributeString("Minute", GetAttribute(ccCal.Offset_Minute));
               }
               writer.WriteEndElement(); // End Offset

               writer.WriteStartElement("ZeroSuppress"); // Start ZeroSuppress
               {
                  if ((mask[i] & (int)ba.Year) > 0)
                     writer.WriteAttributeString("Year", GetAttribute(ccCal.Zero_Suppress_Year));
                  if ((mask[i] & (int)ba.Month) > 0)
                     writer.WriteAttributeString("Month", GetAttribute(ccCal.Zero_Suppress_Month));
                  if ((mask[i] & (int)ba.Day) > 0)
                     writer.WriteAttributeString("Day", GetAttribute(ccCal.Zero_Suppress_Day));
                  if ((mask[i] & (int)ba.Hour) > 0)
                     writer.WriteAttributeString("Hour", GetAttribute(ccCal.Zero_Suppress_Hour));
                  if ((mask[i] & (int)ba.Minute) > 0)
                     writer.WriteAttributeString("Minute", GetAttribute(ccCal.Zero_Suppress_Minute));
                  if ((mask[i] & (int)ba.Week) > 0)
                     writer.WriteAttributeString("Week", GetAttribute(ccCal.Zero_Suppress_Weeks));
                  if ((mask[i] & (int)ba.DayOfWeek) > 0)
                     writer.WriteAttributeString("DayOfWeek", GetAttribute(ccCal.Zero_Suppress_Day_Of_Week));
               }
               writer.WriteEndElement(); // End ZeroSuppress

               writer.WriteStartElement("EnableSubstitution"); // Start EnableSubstitution
               {
                  if ((mask[i] & (int)ba.Year) > 0)
                     writer.WriteAttributeString("Year", GetAttribute(ccCal.Substitute_Year));
                  if ((mask[i] & (int)ba.Month) > 0)
                     writer.WriteAttributeString("Month", GetAttribute(ccCal.Substitute_Month));
                  if ((mask[i] & (int)ba.Day) > 0)
                     writer.WriteAttributeString("Day", GetAttribute(ccCal.Substitute_Day));
                  if ((mask[i] & (int)ba.Hour) > 0)
                     writer.WriteAttributeString("Hour", GetAttribute(ccCal.Substitute_Hour));
                  if ((mask[i] & (int)ba.Minute) > 0)
                     writer.WriteAttributeString("Minute", GetAttribute(ccCal.Substitute_Minute));
                  if ((mask[i] & (int)ba.Week) > 0)
                     writer.WriteAttributeString("Week", GetAttribute(ccCal.Substitute_Weeks));
                  if ((mask[i] & (int)ba.DayOfWeek) > 0)
                     writer.WriteAttributeString("DayOfWeek", GetAttribute(ccCal.Substitute_Day_Of_Week));
               }
               writer.WriteEndElement(); // End EnableSubstitution
            }
            writer.WriteEndElement(); // End Date
         }
      }

      // Output the Counter Settings
      private void WriteCounterSettings(XmlTextWriter writer) {
         bool success = true;
         int FirstBlock = 0;
         int BlockCount = 0;
         success = success && EIP.GetAttribute(ccCount.First_Count_Block, out FirstBlock);
         success = success && EIP.GetAttribute(ccCount.Number_Of_Count_Blocks, out BlockCount);
         for (int i = 0; success && i < BlockCount; i++) {
            success = success && EIP.SetAttribute(ccIDX.Count_Block, FirstBlock + i);
            writer.WriteStartElement("Counter"); // Start Counter
            {
               writer.WriteAttributeString("Block", (i + 1).ToString());
               writer.WriteAttributeString("Reset", GetAttribute(ccCount.Reset_Value));
               //writer.WriteAttributeString("ExternalSignal", p.CtExternalSignal);
               //writer.WriteAttributeString("ResetSignal", p.CtResetSignal);
               writer.WriteAttributeString("CountUp", GetAttribute(ccCount.Direction_Value));
               writer.WriteAttributeString("Increment", GetAttribute(ccCount.Increment_Value));
               writer.WriteAttributeString("JumpTo", GetAttribute(ccCount.Jump_To));
               writer.WriteAttributeString("JumpFrom", GetAttribute(ccCount.Jump_From));
               writer.WriteAttributeString("UpdateUnit", GetAttribute(ccCount.Update_Unit_Unit));
               writer.WriteAttributeString("UpdateIP", GetAttribute(ccCount.Update_Unit_Halfway));
               writer.WriteAttributeString("Range2", GetAttribute(ccCount.Count_Range_2));
               writer.WriteAttributeString("Range1", GetAttribute(ccCount.Count_Range_1));
               writer.WriteAttributeString("InitialValue", GetAttribute(ccCount.Initial_Value));
               writer.WriteAttributeString("Multiplier", GetAttribute(ccCount.Count_Multiplier));
               writer.WriteAttributeString("ZeroSuppression", GetAttribute(ccCount.Zero_Suppression));
            }
            writer.WriteEndElement(); //  End Counter
         }
      }

      // Output the User Pattern Settings
      private void WriteUserPatternSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Logo"); // Start Logo
         {
            //writer.WriteAttributeString("Variable", p.WlxVariableName);
            //writer.WriteAttributeString("HAlignment", Enum.GetName(typeof(Utils.HAlignment), p.LogoHAlignment));
            //writer.WriteAttributeString("VAlignment", Enum.GetName(typeof(Utils.VAlignment), p.LogoVAlignment));
            //writer.WriteAttributeString("Filter", p.LogoFilter.ToString());
            //writer.WriteAttributeString("ReverseVideo", p.LogoReverseVideo.ToString());
            //writer.WriteAttributeString("Source", p.LogoSource);
            //writer.WriteAttributeString("LogoLength", p.LogoLength.ToString());
            //writer.WriteAttributeString("Registration", p.LogoRegistration.ToString());

            //using (MemoryStream ms2 = new MemoryStream()) {
            //   // bm.Save does not like transparent pixels so make them white
            //   Bitmap bm2 = new Bitmap(p.ItemWidth, p.ItemHeight);
            //   using (Graphics g = Graphics.FromImage(bm2)) {
            //      g.Clear(Color.White);
            //      for (int x = 0; x < p.ItemWidth; x++) {
            //         for (int y = 0; y < p.ItemHeight; y++) {
            //            if (p.ScaledImage.GetPixel(x, y).ToArgb() != 0) {
            //               bm2.SetPixel(x, y, Color.Black);
            //            }
            //         }
            //      }
            //   }
            //   Bitmap bm = Utils.BitmapTo1Bpp(bm2);
            //   bm.Save(ms2, ImageFormat.Bmp);
            //   using (BinaryReader br = new BinaryReader(ms2)) {
            //      ms2.Position = 0;
            //      byte[] b = br.ReadBytes((int)ms2.Length);
            //      int length = Utils.LittleEndian(b, 2, 4);
            //      string data = "";
            //      writer.WriteAttributeString("size", length.ToString());
            //      for (int j = 0; j < length; j++) {
            //         data = data + string.Format("{0:X2} ", b[j]);
            //      }
            //      writer.WriteStartElement("Data"); // Start Data
            //      writer.WriteAttributeString("Length", length.ToString());
            //      writer.WriteString(data.TrimEnd());
            //      writer.WriteEndElement(); // End Data
            //   }
            //}
         }
         writer.WriteEndElement(); // End Logo
         //string LogoText = "";
         //for (int j = 0; j < p.ItemText.Length; j++) {
         //   LogoText += ((short)p.ItemText[j]).ToString("X4");
         //}
      }

      #endregion

      #region Display XML in Tree and Indented forms

      // Process an XML Label
      private bool ProcessLabel(string xml) {
         bool result = false;
         int xmlStart = 0;
         int xmlEnd = 0;
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
               xmlDoc = new XmlDocument();
               xmlDoc.PreserveWhitespace = true;
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
