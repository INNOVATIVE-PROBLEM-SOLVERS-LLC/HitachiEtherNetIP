using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EIP_Lib {
   public partial class XML {

      #region Data Declarations

      Browser parent;

      EIP EIP;
      TabPage tab;

      // Tab Controls
      TabControl tclViewXML;
      TabPage tabTreeView;
      TabPage tabIndented;

      TreeView tvXML;
      TextBox txtIndentedView;

      // Operating Buttons
      Button cmdOpen;
      Button cmdClear;
      Button cmdGenerate;
      Button cmdSaveAs;
      Button cmdDeleteAll;
      Button cmdSaveInPrinter;

      // Testing Buttons
      Label lblSelectXmlTest;
      ComboBox cbAvailableXmlTests;
      Button cmdBrowse;
      Button cmdSendToPrinter;

      Label lblSelectHardTest;
      ComboBox cbAvailableHardTests;
      Button cmdRunHardTest;

      // XML Processing
      string XMLText = string.Empty;
      XmlDocument xmlDoc = null;
      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
         Link = 5,     // Not supported in the printer
         Prompt = 6,   // Not supported in the printer
      }

      // Make things easier to read
      Font courier = new Font("Courier New", 9);

      // Flag for Attribute Not Present
      const string N_A = "N!A";

      // Braced Characters (count, date, half-size, logos
      char[] bc = new char[] { 'C', 'Y', 'M', 'D', 'h', 'm', 's', 'T', 'W', '7', 'E', 'F', ' ', '\'', '.', ';', ':', '!', ',', 'X', 'Z' };
      // Attributes of braced characters
      enum ba {
         Count = 1 << 0,
         Year = 1 << 1,
         Month = 1 << 2,
         Day = 1 << 3,
         Hour = 1 << 4,
         Minute = 1 << 5,
         Second = 1 << 6,
         Julian = 1 << 7,
         Week = 1 << 8,
         DayOfWeek = 1 << 9,
         Shift = 1 << 10,
         TimeCount = 1 << 11,
         Space = 1 << 12,
         Quote = 1 << 13,
         Period = 1 << 14,
         SemiColon = 1 << 15,
         Colon = 1 << 16,
         Exclamation = 1 << 17,
         Comma = 1<< 18,
         FixedPattern = 1 << 19,
         FreePattern = 1 << 20,
         Unknown = 1 << 21,
         DateCode = (1 << 12) - 2, // All the date codes combined

      }
      #endregion

      #region Constructors and destructors

      // Create class
      public XML(Browser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;

         BuildControls();

         BuildTestFileList();

         SetButtonEnables();
      }

      #endregion

      #region Form Control Events

      // Open a new XML file
      private void Open_Click(object sender, EventArgs e) {
         // Clear out any currently loaded file
         Clear_Click(null, null);
         DialogResult dlgResult = DialogResult.Retry;
         string fileName = String.Empty;
         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
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
         SetButtonEnables();
      }

      // Clear out the screens
      private void Clear_Click(object sender, EventArgs e) {
         txtIndentedView.Text = string.Empty;
         xmlDoc = null;
         tvXML.Nodes.Clear();
         XMLText = string.Empty;
         SetButtonEnables();
      }

      // Generate an XMP Doc from the printer contents
      private void Generate_Click(object sender, EventArgs e) {
         XMLText = ConvertLayoutToXML();
         ProcessLabel(XMLText);
         SetButtonEnables();
      }

      // Save the generated XML file
      private void SaveAs_Click(object sender, EventArgs e) {
         DialogResult dlgResult;
         string filename = string.Empty;

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
               using (Stream outfs = new FileStream(filename, FileMode.Create)) {
                  // Might have some possibilities here <TODO>
                  outfs.Write(EIP.Encode.GetBytes(XMLText), 0, XMLText.Length);
                  outfs.Flush();
                  outfs.Close();
                  SetButtonEnables();
               }
            }
         }
         SetButtonEnables();
      }

      #endregion

      #region XML Save Routines

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

      // Output the Counter Settings
      private void WriteCounterSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Counter"); // Start Counter
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
         //writer.WriteAttributeString("Format", p.RawText);
         writer.WriteAttributeString("Multiplier", GetAttribute(ccCount.Count_Multiplier));
         writer.WriteAttributeString("ZeroSuppression", GetAttribute(ccCount.Zero_Suppression));
         writer.WriteEndElement(); //  End Counter
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

      #region Send to Printer Routines

      // Send xlmDoc to printer
      private void SendToPrinter_Click(object sender, EventArgs e) {
         // Need a XMP Document to continue
         if (xmlDoc == null) {
            Open_Click(null, null);
            if (xmlDoc == null) {
               return;
            }
         }
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Set to only one item in printer
               CleanUpDisplay();
               XmlNode lab = xmlDoc.SelectSingleNode("Label");
               foreach (XmlNode l in lab.ChildNodes) {
                  if (l is XmlWhitespace)
                     continue;
                  switch (l.Name) {
                     case "Printer":
                        // Send printer wide settings
                        SendPrinterSettings(l);
                        break;
                     case "Objects":
                        // Send the objects one at a time
                        SendObjectSettings(l.ChildNodes);
                        break;
                  }
                  //// Send printer wide settings
                  //SendPrinterSettings(xmlDoc.SelectSingleNode("Label/Printer"));
                  //// Send the objects one at a time
                  //SendObjectSettings(xmlDoc.SelectNodes("Label/Objects")[0].ChildNodes);
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      // Send the Printer Wide Settings
      private void SendPrinterSettings(XmlNode pr) {
         success = true;
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  EIP.SetAttribute(ccPS.Character_Orientation, GetAttr(c, "Orientation"));
                  break;
               case "ContinuousPrinting":
                  EIP.SetAttribute(ccPS.Repeat_Interval, GetAttr(c, "RepeatInterval"));
                  EIP.SetAttribute(ccPS.Repeat_Count, GetAttr(c, "PrintsPerTrigger"));
                  break;
               case "TargetSensor":
                  EIP.SetAttribute(ccPS.Target_Sensor_Filter, GetAttr(c, "Filter"));
                  EIP.SetAttribute(ccPS.Targer_Sensor_Filter_Value, GetAttr(c, "SetupValue"));
                  EIP.SetAttribute(ccPS.Target_Sensor_Timer, GetAttr(c, "Timer"));
                  break;
               case "CharacterSize":
                  EIP.SetAttribute(ccPS.Character_Width, GetAttr(c, "Width"));
                  EIP.SetAttribute(ccPS.Character_Width, GetAttr(c, "Height"));
                  break;
               case "PrintStartDelay":
                  EIP.SetAttribute(ccPS.Print_Start_Delay_Reverse, GetAttr(c, "Reverse"));
                  EIP.SetAttribute(ccPS.Print_Start_Delay_Forward, GetAttr(c, "Forward"));
                  break;
               case "EncoderSettings":
                  EIP.SetAttribute(ccPS.High_Speed_Print, GetAttr(c, "HighSpeedPrinting"));
                  EIP.SetAttribute(ccPS.Pulse_Rate_Division_Factor, GetAttr(c, "Divisor"));
                  EIP.SetAttribute(ccPS.Product_Speed_Matching, GetAttr(c, "ExternalEncoder"));
                  break;
               case "InkStream":
                  EIP.SetAttribute(ccPS.Ink_Drop_Use, GetAttr(c, "InkDropUse"));
                  EIP.SetAttribute(ccPS.Ink_Drop_Charge_Rule, GetAttr(c, "ChargeRule"));
                  break;
               case "TwinNozzle":
                  // Not supported in EtherNet/IP
                  //this.LeadingCharacterControl = GetAttr(c, "LeadingCharControl", 0);
                  //this.LeadingCharacterControlWidth1 = GetAttr(c, "LeadingCharControlWidth1", 32);
                  //this.LeadingCharacterControlWidth1 = GetAttr(c, "LeadingCharControlWidth2", 32);
                  //this.NozzleSpaceAlignment = GetAttr(c, "NozzleSpaceAlignment", 0);
                  break;
               case "Substitution":
                  SendSubstitution(c);
                  break;
               case "Calendar":
                  SendCalendar(c);
                  break;
               case "Count":
                  SendCounter(c);
                  break;
            }
         }
      }

      // Set all the values for a single substitution rule
      private void SendSubstitution(XmlNode p) {
         AttrData attr;
         byte[] data;

         // Get the standard attributes for substitution
         string rule = GetAttr(p, "Rule");
         string startYear = GetAttr(p, "StartYear");
         string delimeter = GetAttr(p, "Delimeter");

         // Avoid user errors
         if (int.TryParse(rule, out int ruleNumber) && int.TryParse(startYear, out int year) && delimeter.Length == 1) {

            // Sub Substitution rule in Index class
            attr = EIP.AttrDict[ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting];
            data = EIP.FormatOutput(attr.Set, ruleNumber);
            EIP.SetAttribute(ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting, data);

            // Set the start year in the substitution rule
            attr = EIP.AttrDict[ClassCode.Index, (byte)ccSR.Start_Year];
            data = EIP.FormatOutput(attr.Set, year);
            EIP.SetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Start_Year, data);

            // Load the individual rules
            foreach (XmlNode c in p.ChildNodes) {
               switch (c.Name) {
                  case "Year":
                     SetSubValues(ccSR.Year, c, delimeter);
                     break;
                  case "Month":
                     SetSubValues(ccSR.Month, c, delimeter);
                     break;
                  case "Day":
                     SetSubValues(ccSR.Day, c, delimeter);
                     break;
                  case "Hour":
                     SetSubValues(ccSR.Hour, c, delimeter);
                     break;
                  case "Minute":
                     SetSubValues(ccSR.Minute, c, delimeter);
                     break;
                  case "Week":
                     SetSubValues(ccSR.Week, c, delimeter);
                     break;
                  case "DayOfWeek":
                     SetSubValues(ccSR.Day_Of_Week, c, delimeter);
                     break;
                  case "Skip":
                     // Do not process these nodes
                     break;
               }
            }
         }
      }

      // Send Date related information
      private void SendCalendar(XmlNode d) {

         XmlNode n = d.SelectSingleNode("Offset");
         if (n != null) {
            EIP.SetAttribute(ccCal.Offset_Year, GetAttr(n, "Year"));
            EIP.SetAttribute(ccCal.Offset_Month, GetAttr(n, "Month"));
            EIP.SetAttribute(ccCal.Offset_Day, GetAttr(n, "Day"));
            EIP.SetAttribute(ccCal.Offset_Hour, GetAttr(n, "Hour"));
            EIP.SetAttribute(ccCal.Offset_Minute, GetAttr(n, "Minute"));
         }

         n = d.SelectSingleNode("ZeroSuppress");
         if (n != null) {
            EIP.SetAttribute(ccCal.Zero_Suppress_Year, GetAttr(n, "Year"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Month, GetAttr(n, "Month"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Day, GetAttr(n, "Day"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Hour, GetAttr(n, "Hour"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Minute, GetAttr(n, "Minute"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Weeks, GetAttr(n, "Week"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Day_Of_Week, GetAttr(n, "DayOfWeek"));
         }

         n = d.SelectSingleNode("EnableSubstitution");
         if (n != null) {
            EIP.SetAttribute(ccCal.Substitute_Year, GetAttr(n, "Year"));
            EIP.SetAttribute(ccCal.Substitute_Month, GetAttr(n, "Month"));
            EIP.SetAttribute(ccCal.Substitute_Day, GetAttr(n, "Day"));
            EIP.SetAttribute(ccCal.Substitute_Hour, GetAttr(n, "Hour"));
            EIP.SetAttribute(ccCal.Substitute_Minute, GetAttr(n, "Minute"));
            EIP.SetAttribute(ccCal.Substitute_Weeks, GetAttr(n, "Week"));
            EIP.SetAttribute(ccCal.Substitute_Day_Of_Week, GetAttr(n, "DayOfWeek"));
         }

         n = d.SelectSingleNode("TimeCount");
         if (n != null) {
            EIP.SetAttribute(ccCal.Time_Count_Start_Value, GetAttr(n, "Start"));
            EIP.SetAttribute(ccCal.Time_Count_End_Value, GetAttr(n, "End"));
            EIP.SetAttribute(ccCal.Time_Count_Reset_Value, GetAttr(n, "Reset"));
            EIP.SetAttribute(ccCal.Reset_Time_Value, GetAttr(n, "ResetTime"));
            EIP.SetAttribute(ccCal.Update_Interval_Value, GetAttr(n, "RenewalPeriod"));
         }

         n = d.SelectSingleNode("Shift");
         if (n != null) {
            EIP.SetAttribute(ccIDX.Item, GetAttr(n, "Number"));
            EIP.SetAttribute(ccCal.Shift_Start_Hour, GetAttr(n, "StartHour"));
            EIP.SetAttribute(ccCal.Shift_Start_Minute, GetAttr(n, "StartMinute"));
            EIP.SetAttribute(ccCal.Shift_End_Hour, GetAttr(n, "EndHour"));
            EIP.SetAttribute(ccCal.Shift_End_Minute, GetAttr(n, "EndMinute"));
         }
      }

      // Send counter related information
      private void SendCounter(XmlNode c) {
         if (c != null) {
            EIP.SetAttribute(ccCount.Initial_Value, GetAttr(c, "InitialValue"));
            EIP.SetAttribute(ccCount.Count_Range_1, GetAttr(c, "Range1"));
            EIP.SetAttribute(ccCount.Count_Range_2, GetAttr(c, "Range2"));
            EIP.SetAttribute(ccCount.Update_Unit_Halfway, GetAttr(c, "UpdateIP"));
            EIP.SetAttribute(ccCount.Update_Unit_Unit, GetAttr(c, "UpdateUnit"));
            EIP.SetAttribute(ccCount.Increment_Value, GetAttr(c, "Increment"));
            string s = bool.TryParse(GetAttr(c, "CountUp"), out bool b) && !b ? "Down" : "Up";
            EIP.SetAttribute(ccCount.Direction_Value, s);
            EIP.SetAttribute(ccCount.Jump_From, GetAttr(c, "JumpFrom"));
            EIP.SetAttribute(ccCount.Jump_To, GetAttr(c, "JumpTo"));
            EIP.SetAttribute(ccCount.Reset_Value, GetAttr(c, "Reset"));
            EIP.SetAttribute(ccCount.Type_Of_Reset_Signal, GetAttr(c, "ResetSignal"));
            EIP.SetAttribute(ccCount.External_Count, GetAttr(c, "ExternalSignal"));
            EIP.SetAttribute(ccCount.Zero_Suppression, GetAttr(c, "ZeroSuppression"));
            EIP.SetAttribute(ccCount.Count_Multiplier, GetAttr(c, "Multiplier"));
            EIP.SetAttribute(ccCount.Count_Skip, GetAttr(c, "Skip"));
         }
      }

      // Set the substitution values for a class
      private void SetSubValues(ccSR attribute, XmlNode c, string delimeter) {
         // Avoid user errors
         if (int.TryParse(GetAttr(c, "Base"), out int b)) {
            Prop prop = EIP.AttrDict[ClassCode.Substitution_rules, (byte)attribute].Set;
            string[] s = GetValue(c).Split(delimeter[0]);
            for (int i = 0; i < s.Length && success; i++) {
               int n = b + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  byte[] data = EIP.FormatOutput(prop, n, 1, s[i]);
                  success = EIP.SetAttribute(ClassCode.Substitution_rules, (byte)attribute, data);
               }
            }
         }
      }

      // Send the individual objects
      private void SendObjectSettings(XmlNodeList objs) {
         success = true;
         ItemType type;
         XmlNode n;
         int count = 1;
         int calendar = 1;
         int item = 1;
         foreach (XmlNode obj in objs) {
            if (obj is XmlWhitespace)
               continue;
            // Get the item type
            type = (ItemType)Enum.Parse(typeof(ItemType), GetAttr(obj, "Type"), true);
            // Handle multiple line texts
            string[] text = GetValue(obj.SelectSingleNode("Text")).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < text.Length; i++) {
               // Printer always has one item
               if (item > 1) {
                  // Add an item <TODO> Need to add item, not column
                  EIP.ServiceAttribute(ccPF.Add_Column, 0);
               }

               // Point to the item
               EIP.SetAttribute(ccIDX.Item, item);

               // Set the common parameters
               n = obj.SelectSingleNode("Location");
               //x = GetAttr(n, "Left", 0);
               //y = GetAttr(n, "Top", 0) - GetAttr(n, "Height", 0);
               //int r = GetAttr(n, "Row", -1);
               //int c = GetAttr(n, "Column", -1);

               n = obj.SelectSingleNode("Font");
               EIP.SetAttribute(ccPF.Dot_Matrix, n.InnerText);
               EIP.SetAttribute(ccPF.InterCharacter_Space, GetAttr(n, "InterCharacterSpace"));
               EIP.SetAttribute(ccPF.Line_Spacing, GetAttr(n, "InterLineSpace"));
               EIP.SetAttribute(ccPF.Character_Bold, GetAttr(n, "IncreasedWidth"));


               //p = new TPB(this, type, x, y, F, ICS, ILS, IW);

               //p.Row = r;
               //p.Column = c;

               //p.BarCode = GetAttr(n, "BarCode", "(None)");
               //p.HumanReadableFont = GetAttr(n, "HumanReadableFont", "(None)");
               //p.EANPrefix = GetAttr(n, "EANPrefix", "00");

               switch (type) {
                  case ItemType.Text:
                     EIP.SetAttribute(ccPF.Print_Character_String, text[i]);
                     break;
                  case ItemType.Counter:
                     EIP.SetAttribute(ccIDX.Count_Block, count++);
                     EIP.SetAttribute(ccPF.Print_Character_String, FormatCounter(text[i]));
                     SendCounter(obj.SelectSingleNode("Counter"));
                     break;
                  case ItemType.Date:
                     EIP.SetAttribute(ccIDX.Calendar_Block, calendar++);
                     EIP.SetAttribute(ccPF.Print_Character_String, FormatDate(text[i]));
                     n = obj.SelectSingleNode("Date");
                     if (n != null) {
                        SendCalendar(n);
                     }
                     break;
                  case ItemType.Logo:
                     break;
                  case ItemType.Link:
                     break;
                  case ItemType.Prompt:
                     break;
                  default:
                     break;
               }
               item++;
            }
         }
      }

      // Convert from cijConnect format to Hitachi format
      private string FormatCounter(string text) {
         string result = text;
         if (text.IndexOf("{{") < 0) {
            int lBrace = text.IndexOf('{');
            int rBrace = text.LastIndexOf('}');
            if (lBrace >= 0 && lBrace < rBrace) {
               result = text.Substring(0, lBrace) + "{{" + new string('C', rBrace - lBrace - 1) + "}}" + text.Substring(rBrace + 1);
            }
         }
         return result;
      }

      // Convert from cijConnect format to Hitachi format
      private string FormatDate(string text) {
         string result = text;
         if (text.IndexOf("{{") < 0) {
            int lBrace = text.IndexOf('{');
            int rBrace = text.LastIndexOf('}');
            if (lBrace >= 0 && lBrace < rBrace) {
               result = text.Substring(0, lBrace) + "{{" + text.Substring(lBrace + 1, rBrace - lBrace - 1) + "}}" + text.Substring(rBrace + 1);
            }
         }
         return result;
      }

      #endregion

      #region Service Routines

      // Build XML page controls
      private void BuildControls() {
         tclViewXML = new TabControl() { Name = "tclViewXML", Font = courier };
         tabTreeView = new TabPage() { Name = "tabTreeView", Text = "Tree View" };
         tabIndented = new TabPage() { Name = "tabIndented", Text = "Indented View" };

         tvXML = new TreeView() { Name = "tvXML", Font = courier };
         txtIndentedView = new TextBox() { Name = "txtIndentedView", Multiline = true, ScrollBars = ScrollBars.Both };

         cmdOpen = new Button() { Text = "Open" };
         cmdOpen.Click += Open_Click;

         cmdClear = new Button() { Text = "Clear" };
         cmdClear.Click += Clear_Click;

         cmdGenerate = new Button() { Text = "Generate" };
         cmdGenerate.Click += Generate_Click;

         cmdSaveAs = new Button() { Text = "Save As" };
         cmdSaveAs.Click += SaveAs_Click;

         cmdSendToPrinter = new Button() { Text = "Send To Printer" };
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

         // Testing controls
         lblSelectXmlTest = new Label() { Text = "Select XML Test", TextAlign = ContentAlignment.BottomCenter };
         cbAvailableXmlTests = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         cbAvailableXmlTests.SelectedIndexChanged += cbAvailableTests_SelectedIndexChanged;
         cmdBrowse = new Button() { Text = "Browse" };
         cmdBrowse.Click += cmdBrowse_Click;

         lblSelectHardTest = new Label() { Text = "Select Hard Test", TextAlign = ContentAlignment.BottomCenter };
         cbAvailableHardTests = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         cbAvailableHardTests.Items.AddRange(
            new string[] { "Reset", "Shift Code", "Month Day SR", "Time Count", "Day of Week etc", "MDY hms", "Multi-Line", "Counter", "???" }
            );
         cbAvailableHardTests.SelectedIndexChanged += CbAvailableHardTests_SelectedIndexChanged;
         cmdRunHardTest = new Button() { Text = "Run Test" };
         cmdRunHardTest.Click += cmdRunHardTest_Click;

         cmdDeleteAll = new Button() { Text = "Delete All" };
         cmdSaveInPrinter = new Button() { Text = "Save In Printer" };

         cmdDeleteAll.Click += cmdDeleteAll_Click;
         cmdSaveInPrinter.Click += cmdSaveToPrinter_Click;

         tab.Controls.Add(lblSelectXmlTest);
         tab.Controls.Add(lblSelectHardTest);
         tab.Controls.Add(cbAvailableXmlTests);
         tab.Controls.Add(cbAvailableHardTests);
         tab.Controls.Add(cmdDeleteAll);
         tab.Controls.Add(cmdBrowse);
         tab.Controls.Add(cmdSaveInPrinter);
         tab.Controls.Add(cmdRunHardTest);
      }

      private void CbAvailableHardTests_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      private void cbAvailableTests_SelectedIndexChanged(object sender, EventArgs e) {
         if (cbAvailableXmlTests.SelectedIndex >= 0) {
            string fileName = Path.Combine(parent.MessageFolder, cbAvailableXmlTests.Text + ".XML");
            ProcessLabel(File.ReadAllText(fileName));
            SetButtonEnables();
         }
      }

      // Called from parent
      public void ResizeControls(ref ResizeInfo R) {
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         int tclWidth = (int)(tab.ClientSize.Width / R.W);
         float offset = (int)(tab.ClientSize.Height - tclHeight * R.H);
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         R.offset = offset;
         Utils.ResizeObject(ref R, tclViewXML, 0, 1, tclHeight - 7, tclWidth - 1);
         {
            Utils.ResizeObject(ref R, tvXML, 1, 1, tclHeight - 12, tclWidth - 3);
            Utils.ResizeObject(ref R, txtIndentedView, 1, 1, tclHeight - 12, tclWidth - 3);

            Utils.ResizeObject(ref R, cmdDeleteAll, tclHeight - 6, 1, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdOpen, tclHeight - 3, 1, 2.5f, 4);

            Utils.ResizeObject(ref R, cmdSaveAs, tclHeight - 6, 5.5f, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdClear, tclHeight - 3, 5.5f, 2.5f, 4);

            Utils.ResizeObject(ref R, cmdGenerate, tclHeight - 6, 10, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdSaveInPrinter, tclHeight - 3, 10, 2.5f, 4);

            Utils.ResizeObject(ref R, lblSelectXmlTest, tclHeight - 6, 14.5f, 1, 6);
            Utils.ResizeObject(ref R, cbAvailableXmlTests, tclHeight - 5, 14.5f, 2, 6);
            Utils.ResizeObject(ref R, cmdBrowse, tclHeight - 3, 14.5f, 2.5f, 6);

            Utils.ResizeObject(ref R, cmdSendToPrinter, tclHeight - 6, 21, 5.5f, 4);

            Utils.ResizeObject(ref R, lblSelectHardTest, tclHeight - 6, 25.5f, 1, 5);
            Utils.ResizeObject(ref R, cbAvailableHardTests, tclHeight - 5, 25.5f, 2, 5);
            Utils.ResizeObject(ref R, cmdRunHardTest, tclHeight - 6, 31, 5.5f, 4);

         }
         R.offset = 0;
      }

      // Only allow buttons if conditions are right to process the request
      public void SetButtonEnables() {
         cmdSaveAs.Enabled = XMLText.Length > 0;
         cmdSendToPrinter.Enabled = xmlDoc != null;
         cmdRunHardTest.Enabled = cbAvailableHardTests.SelectedIndex >= 0;
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
                  if(k >= 0) {
                     mask[l] |= 1 << k;
                  } else {
                     mask[l] |= (int)ba.Unknown;
                  }
               }
            }
            if (s[i].IndexOf('}', n+1) >0) {
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

      private string GetValue(XmlNode node) {
         if (node != null) {
            return node.InnerText;
         } else {
            return N_A;
         }
      }

      private string GetAttr(XmlNode node, string AttrName) {
         XmlNode n;
         if (node != null && (n = node.Attributes[AttrName]) != null) {
            return n.Value;
         } else {
            return N_A;
         }
      }

      private void BuildTestFileList() {
         cbAvailableXmlTests.Items.Clear();
         string[] FileNames = Directory.GetFiles(parent.MessageFolder, "*.XML");
         Array.Sort(FileNames);
         for (int i = 0; i < FileNames.Length; i++) {
            cbAvailableXmlTests.Items.Add(Path.GetFileNameWithoutExtension(FileNames[i]));
         }

      }

      #endregion

      #region Test Routines

      // Success is "Global" so the Get/Set/Service Attributes callers can avoid continuously testing it
      bool success = true;

      // Get the contents of one attribute
      private string GetAttribute<T>(T Attribute) {
         string val = string.Empty;
         ClassCode cc = EIP.ClassCodes[Array.IndexOf(EIP.ClassCodeAttributes, typeof(T))];
         byte at = Convert.ToByte(Attribute);
         AttrData attr = EIP.AttrDict[cc, at];
         if (EIP.GetAttribute(cc, at, EIP.Nodata)) {
            val = EIP.GetDataValue;
            if (attr.Data.Fmt == DataFormats.UTF8) {
               val = EIP.FromQuoted(EIP.GetDataValue);
            } else if (attr.Data.DropDown != fmtDD.None) {
               string[] dd = EIP.DropDowns[(int)attr.Data.DropDown];
               long n = EIP.GetDecValue - attr.Data.Min;
               if (n >= 0 && n < dd.Length) {
                  val = dd[n];
               }
            }
         }
         return val;
      }

      // Get the value of an attribute that is known to be a decimal number
      private int GetDecimalAttribute<T>(T Attribute) where T : Enum {
         AttrData attr = EIP.GetAttrData(Attribute);
         EIP.GetAttribute(attr.Class, attr.Val, EIP.Nodata);
         return EIP.GetDecValue;
      }

      // Delete all but 1
      private void cmdDeleteAll_Click(object sender, EventArgs e) {
         CleanUpDisplay();
      }

      // Add text to all items (Control Deleted)
      private void cmdAddText_Click(object sender, EventArgs e) {
         // Add new logic here
      }

      // Create a message with text only (Control Deleted)
      private void cmdCreateText_Click(object sender, EventArgs e) {
         success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               for (int step = 0; step < 3 && success; step++) {
                  switch (step) {
                     case 0:
                        // Cleanup the display
                        CleanUpDisplay();
                        break;
                     case 1:
                        // Put in some items
                        for (int i = 0; i < 5; i++) {
                           EIP.ServiceAttribute(ccPF.Add_Column, 0);
                        }
                        break;
                     case 2:
                        // Set the text
                        SetText("Hello World");
                        break;
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void cmdSaveToPrinter_Click(object sender, EventArgs e) {
         success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               byte[] data = EIP.Merge(EIP.ToBytes(4, 2), EIP.ToBytes(2, 1), EIP.ToBytes("AAA" + "\x00"));
               AttrData attr = EIP.GetAttrData(ccPDM.Store_Print_Data);
               EIP.SetAttribute(attr.Class, attr.Val, data);
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void cmdBrowse_Click(object sender, EventArgs e) {
         using (FolderBrowserDialog dlg = new FolderBrowserDialog()) {
            dlg.ShowNewFolderButton = true;
            dlg.SelectedPath = parent.MessageFolder;
            if (dlg.ShowDialog() == DialogResult.OK) {
               parent.MessageFolder = dlg.SelectedPath;
               BuildTestFileList();
            }
         }
      }

      #endregion

   }

}
