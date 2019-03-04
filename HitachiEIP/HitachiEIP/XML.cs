using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace HitachiEIP {

   public class XML {

      #region Data Declarations

      HitachiBrowser parent;
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
      Button cmdSendToPrinter;

      // Testing Buttons
      Button cmdTest1;
      Button cmdTest2;
      Button cmdTest3;
      Button cmdTest4;
      Button cmdTest5;

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

      #endregion

      #region Constructors and destructors

      // Create class
      public XML(HitachiBrowser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;

         BuildControls();

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
                  outfs.Write(EIP.encode.GetBytes(XMLText), 0, XMLText.Length);
                  outfs.Flush();
                  outfs.Close();
                  SetButtonEnables();
               }
            }
         }
         SetButtonEnables();
      }

      #endregion

      #region XML  Save Routines

      // Generate an XMP Doc form the current printer settings
      private string ConvertLayoutToXML() {
         bool success = true;
         ItemType itemType = ItemType.Text;
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
               if (EIP.StartSession()) {
                  if (EIP.ForwardOpen()) {
                     writer.WriteStartElement("Label"); // Start Label

                     //writer.WriteAttributeString("ClockSystem", this.ClockSystem);
                     //writer.WriteAttributeString("Registration", this.Registration.ToString());
                     //writer.WriteAttributeString("GroupNumber", this.MessageGroupNumber);
                     //writer.WriteAttributeString("GroupName", this.MessageGroup);
                     //writer.WriteAttributeString("Name", this.MessageName);
                     //writer.WriteAttributeString("BeRestrictive", this.BeRestrictive.ToString());
                     //writer.WriteAttributeString("UseHalfSpace", this.UseHalfSpace.ToString());
                     //writer.WriteAttributeString("Format", MessageStyle.ToString());

                     writer.WriteAttributeString("Version", "1");
                     WritePrinterSettings(writer);

                     writer.WriteStartElement("Objects"); // Start Objects
                  }
                  EIP.ForwardClose();
               }
               EIP.EndSession();

               int itemCount = GetDecimalAttribute(ClassCode.Print_format, (byte)ccPF.Number_Of_Items);
               for (int i = 0; i < itemCount; i++) {
                  if (EIP.StartSession()) {
                     if (EIP.ForwardOpen()) {

                        SetAttribute(ClassCode.Index, (byte)ccIDX.Item, i + 1, ref success);
                        string text = GetAttribute(ClassCode.Print_format, (byte)ccPF.Print_Character_String);

                        itemType = GetItemType(text);

                        writer.WriteStartElement("Object"); // Start Object

                        writer.WriteAttributeString("Type", Enum.GetName(typeof(ItemType), itemType));

                        writer.WriteStartElement("Font"); // Start Font
                        {
                           writer.WriteAttributeString("HumanReadableFont", GetAttribute(ClassCode.Print_format, (byte)ccPF.Readable_Code));
                           writer.WriteAttributeString("EANPrefix", GetAttribute(ClassCode.Print_format, (byte)ccPF.Prefix_Code));
                           writer.WriteAttributeString("BarCode", GetAttribute(ClassCode.Print_format, (byte)ccPF.Barcode_Type));
                           writer.WriteAttributeString("IncreasedWidth", GetAttribute(ClassCode.Print_format, (byte)ccPF.Character_Bold));
                           writer.WriteAttributeString("InterLineSpace", GetAttribute(ClassCode.Print_format, (byte)ccPF.Line_Spacing));
                           writer.WriteAttributeString("InterCharacterSpace", GetAttribute(ClassCode.Print_format, (byte)ccPF.InterCharacter_Space));
                           writer.WriteString(GetAttribute(ClassCode.Print_format, (byte)ccPF.Dot_Matrix));
                        }
                        writer.WriteEndElement(); // End Font

                        writer.WriteStartElement("Location"); // Start Location
                        {
                           writer.WriteAttributeString("ItemNumber", (i + 1).ToString());
                           //   writer.WriteAttributeString("Column", p.Column.ToString());
                           //   writer.WriteAttributeString("Row", p.Row.ToString());
                           //   writer.WriteAttributeString("Height", p.ItemHeight.ToString());
                           //   writer.WriteAttributeString("Width", (p.ItemWidth * p.IncreasedWidth).ToString());
                           //   writer.WriteAttributeString("Left", p.X.ToString());
                           //   writer.WriteAttributeString("Top", (p.Y + p.ScaledImage.Height).ToString());
                        }
                        writer.WriteEndElement(); // End Location

                        switch (itemType) {
                           case ItemType.Text:
                              break;
                           case ItemType.Date:
                              WriteCalendarSettings(writer);
                              break;
                           case ItemType.Counter:
                              WriteCounterSettings(writer);
                              break;
                           case ItemType.Logo:
                              WriteUserPatternSettings(writer);
                              break;
                           default:
                              break;
                        }

                        writer.WriteElementString("Text", text);
                        writer.WriteEndElement(); // End Object

                     }
                     EIP.ForwardClose();
                  }
                  EIP.EndSession();
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

      // Output the Counter Settings
      private void WriteCounterSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Counter"); // Start Counter
         writer.WriteAttributeString("Reset", GetAttribute(ClassCode.Count, (byte)ccCount.Reset_Value));
         //writer.WriteAttributeString("ExternalSignal", p.CtExternalSignal);
         //writer.WriteAttributeString("ResetSignal", p.CtResetSignal);
         writer.WriteAttributeString("CountUp", GetAttribute(ClassCode.Count, (byte)ccCount.Direction_Value));
         writer.WriteAttributeString("Increment", GetAttribute(ClassCode.Count, (byte)ccCount.Increment_Value));
         writer.WriteAttributeString("JumpTo", GetAttribute(ClassCode.Count, (byte)ccCount.Jump_To));
         writer.WriteAttributeString("JumpFrom", GetAttribute(ClassCode.Count, (byte)ccCount.Jump_From));
         writer.WriteAttributeString("UpdateUnit", GetAttribute(ClassCode.Count, (byte)ccCount.Update_Unit_Unit));
         writer.WriteAttributeString("UpdateIP", GetAttribute(ClassCode.Count, (byte)ccCount.Update_Unit_Halfway));
         writer.WriteAttributeString("Range2", GetAttribute(ClassCode.Count, (byte)ccCount.Count_Range_2));
         writer.WriteAttributeString("Range1", GetAttribute(ClassCode.Count, (byte)ccCount.Count_Range_1));
         writer.WriteAttributeString("InitialValue", GetAttribute(ClassCode.Count, (byte)ccCount.Initial_Value));
         //writer.WriteAttributeString("Format", p.RawText);
         writer.WriteAttributeString("Multiplier", GetAttribute(ClassCode.Count, (byte)ccCount.Count_Multiplier));
         writer.WriteAttributeString("ZeroSuppression", GetAttribute(ClassCode.Count, (byte)ccCount.Availibility_Of_Zero_Suppression));
         writer.WriteEndElement(); //  End Counter
      }

      // Output the Calendar Settings
      private void WriteCalendarSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Date"); // Start Date
         {
            //writer.WriteAttributeString("Format", p.RawText);

            writer.WriteStartElement("Offset"); // Start Offset
            {
               writer.WriteAttributeString("Minute", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Hour));
               writer.WriteAttributeString("Day", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Day));
               writer.WriteAttributeString("Month", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Month));
               writer.WriteAttributeString("Year", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Year));
            }
            writer.WriteEndElement(); // End Offset

            writer.WriteStartElement("ZeroSuppress"); // Start ZeroSuppress
            {
               writer.WriteAttributeString("DayOfWeek", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Day_Of_Week));
               writer.WriteAttributeString("Week", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Weeks));
               writer.WriteAttributeString("Minute", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Hour));
               writer.WriteAttributeString("Day", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Day));
               writer.WriteAttributeString("Month", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Month));
               writer.WriteAttributeString("Year", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Year));
            }
            writer.WriteEndElement(); // End ZeroSuppress

            writer.WriteStartElement("EnableSubstitution"); // Start EnableSubstitution
            {
               //writer.WriteAttributeString("SubstitutionRule", p.DTSubRule);
               writer.WriteAttributeString("DayOfWeek", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Day_Of_Week));
               writer.WriteAttributeString("Week", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Week));
               writer.WriteAttributeString("Minute", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Hour));
               writer.WriteAttributeString("Day", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Day));
               writer.WriteAttributeString("Month", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Month));
               writer.WriteAttributeString("Year", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Year));
            }
            writer.WriteEndElement(); // End EnableSubstitution

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
               writer.WriteAttributeString("Model", GetAttribute(ClassCode.Unit_Information, (byte)ccUI.Model_Name));
            }
            writer.WriteAttributeString("Make", "Hitachi");

            writer.WriteStartElement("PrintHead");
            {
               writer.WriteAttributeString("Orientation", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Orientation));
            }
            writer.WriteEndElement(); // PrintHead

            writer.WriteStartElement("ContinuousPrinting");
            {
               writer.WriteAttributeString("RepeatInterval", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Repeat_Interval));
               writer.WriteAttributeString("PrintsPerTrigger", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Repeat_Count));
            }
            writer.WriteEndElement(); // ContinuousPrinting

            writer.WriteStartElement("TargetSensor");
            {
               writer.WriteAttributeString("Filter", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Target_Sensor_Filter));
               writer.WriteAttributeString("SetupValue", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Targer_Sensor_Filter_Value));
               writer.WriteAttributeString("Timer", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Target_Sensor_Timer));
            }
            writer.WriteEndElement(); // TargetSensor

            writer.WriteStartElement("CharacterSize");
            {
               writer.WriteAttributeString("Height", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Width));
               writer.WriteAttributeString("Width", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Height));
            }
            writer.WriteEndElement(); // CharacterSize

            writer.WriteStartElement("PrintStartDelay");
            {
               writer.WriteAttributeString("Reverse", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Print_Start_Delay_Forward));
               writer.WriteAttributeString("Forward", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Print_Start_Delay_Reverse));
            }
            writer.WriteEndElement(); // PrintStartDelay

            writer.WriteStartElement("EncoderSettings");
            {
               writer.WriteAttributeString("HighSpeedPrinting", GetAttribute(ClassCode.Print_specification, (byte)ccPS.High_Speed_Print));
               writer.WriteAttributeString("Divisor", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Pulse_Rate_Division_Factor));
               writer.WriteAttributeString("ExternalEncoder", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Product_Speed_Matching));
            }
            writer.WriteEndElement(); // EncoderSettings

            writer.WriteStartElement("InkStream");
            {
               writer.WriteAttributeString("InkDropUse", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Ink_Drop_Use));
               writer.WriteAttributeString("ChargeRule", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Ink_Drop_Charge_Rule));
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
         if (n.Attributes.Count > 0) {
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
               // Send printer wide settings
               SendPrinterSettings(xmlDoc.SelectSingleNode("Label/Printer"));
               // Send the objects one at a time
               SendObjectSettings(xmlDoc.SelectNodes("Label/Objects")[0].ChildNodes);
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      // Send the Printer Wide Settings
      private void SendPrinterSettings(XmlNode pr) {
         bool success = true;
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Orientation, GetAttr(c, "Orientation"), ref success);
                  break;
               case "ContinuousPrinting":
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Repeat_Interval, GetAttr(c, "RepeatInterval"), ref success);
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Repeat_Count, GetAttr(c, "PrintsPerTrigger"), ref success);
                  break;
               case "TargetSensor":
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Target_Sensor_Filter, GetAttr(c, "Filter"), ref success);
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Targer_Sensor_Filter_Value, GetAttr(c, "SetupValue"), ref success);
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Target_Sensor_Timer, GetAttr(c, "Timer"), ref success);
                  break;
               case "CharacterSize":
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Width, GetAttr(c, "Width"), ref success);
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Width, GetAttr(c, "Height"), ref success);
                  break;
               case "PrintStartDelay":
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Print_Start_Delay_Reverse, GetAttr(c, "Reverse"), ref success);
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Print_Start_Delay_Forward, GetAttr(c, "Forward"), ref success);
                  break;
               case "EncoderSettings":
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.High_Speed_Print, GetAttr(c, "HighSpeedPrinting"), ref success);
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Pulse_Rate_Division_Factor, GetAttr(c, "Divisor"), ref success);
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Product_Speed_Matching, GetAttr(c, "ExternalEncoder"), ref success);
                  break;
               case "InkStream":
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Ink_Drop_Use, GetAttr(c, "InkDropUse"), ref success);
                  SetAttribute(ClassCode.Print_specification, (byte)ccPS.Ink_Drop_Charge_Rule, GetAttr(c, "ChargeRule"), ref success);
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
            }
         }
      }

      private void SendSubstitution(XmlNode p) {
         AttrData attr;
         byte[] data;
         string rule = GetAttr(p, "Rule");
         string startYear = GetAttr(p, "StartYear");
         string delimeter = GetAttr(p, "Delimeter");
         if (int.TryParse(rule, out int ruleNumber) && int.TryParse(startYear, out int year) && delimeter.Length == 1) {

            attr = DataII.GetAttrData(ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting);
            data = EIP.FormatOutput(ruleNumber, attr.Set);
            EIP.WriteOneAttribute(ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting, data);

            attr = DataII.GetAttrData(ClassCode.Substitution_rules, (byte)ccSR.Start_Year);
            data = EIP.FormatOutput(year, attr.Set);
            EIP.WriteOneAttribute(ClassCode.Substitution_rules, (byte)ccSR.Start_Year, data);

            foreach (XmlNode c in p.ChildNodes) {
               switch (c.Name) {
                  case "Year":
                     SetSubValues(ccSR.Year, c, delimeter[0], 0);
                     break;
                  case "Month":
                     SetSubValues(ccSR.Month, c, delimeter[0], 1);
                     break;
                  case "Day":
                     SetSubValues(ccSR.Day, c, delimeter[0], 1);
                     break;
                  case "Hour":
                     SetSubValues(ccSR.Hour, c, delimeter[0], 0);
                     break;
                  case "Minute":
                     SetSubValues(ccSR.Minute, c, delimeter[0], 0);
                     break;
                  case "Week":
                     SetSubValues(ccSR.Week, c, delimeter[0], 1);
                     break;
                  case "DayOfWeek":
                     SetSubValues(ccSR.Day_Of_Week, c, delimeter[0], 1);
                     break;
               }
            }
         }
      }

      private void SetSubValues(ccSR attribute, XmlNode c, char delimeter, int n) {
         if (int.TryParse(GetAttr(c, "Base"), out int b)) {
            string[] s = GetValue(c).Split(delimeter);
            for (int i = 0; i < s.Length; i++) {
               byte[] data = EIP.Merge(EIP.ToBytes(n + b + i, 1), EIP.ToBytes(s[i] + "\x00"));
               EIP.WriteOneAttribute(ClassCode.Substitution_rules, (byte)attribute, data);
            }
         }
      }

      // Send the individual objects
      private void SendObjectSettings(XmlNodeList objs) {
         bool success = true;
         ItemType type;
         XmlNode n;
         int count = 1;
         int calendar = 1;
         int item = 1;
         foreach (XmlNode obj in objs) {
            if (!(obj is XmlWhitespace)) {
               // Get the item type
               type = (ItemType)Enum.Parse(typeof(ItemType), GetAttr(obj, "Type"), true);
               // Handle multiple line texts
               string[] text = GetValue(obj.SelectSingleNode("Text"), "").Split(new string[] { "\r\n" }, StringSplitOptions.None);
               for (int i = 0; i < text.Length; i++) {
                  // Printer always has one item
                  if (item > 1) {
                     // Add an item <TODO> Need to add item, not column
                     ServiceAttribute(ClassCode.Print_format, (byte)ccPF.Add_Column, 0, ref success);
                  }

                  // Point to the item
                  SetAttribute(ClassCode.Index, (byte)ccIDX.Item, item, ref success);

                  // Set the common parameters
                  n = obj.SelectSingleNode("Location");
                  //x = GetAttr(n, "Left", 0);
                  //y = GetAttr(n, "Top", 0) - GetAttr(n, "Height", 0);
                  //int r = GetAttr(n, "Row", -1);
                  //int c = GetAttr(n, "Column", -1);

                  n = obj.SelectSingleNode("Font");
                  SetAttribute(ClassCode.Print_format, (byte)ccPF.Dot_Matrix, n.InnerText, ref success);
                  SetAttribute(ClassCode.Print_format, (byte)ccPF.InterCharacter_Space, GetAttr(n, "InterCharacterSpace"), ref success);
                  SetAttribute(ClassCode.Print_format, (byte)ccPF.Line_Spacing, GetAttr(n, "InterLineSpace"), ref success);
                  SetAttribute(ClassCode.Print_format, (byte)ccPF.Character_Bold, GetAttr(n, "IncreasedWidth"), ref success);

                  //p = new TPB(this, type, x, y, F, ICS, ILS, IW);

                  //p.Row = r;
                  //p.Column = c;

                  //p.BarCode = GetAttr(n, "BarCode", "(None)");
                  //p.HumanReadableFont = GetAttr(n, "HumanReadableFont", "(None)");
                  //p.EANPrefix = GetAttr(n, "EANPrefix", "00");

                  switch (type) {
                     case ItemType.Text:
                        SetAttribute(ClassCode.Print_format, (byte)ccPF.Print_Character_String, text[i], ref success);
                        break;
                     case ItemType.Logo:
                        break;
                     case ItemType.Counter:
                        SetAttribute(ClassCode.Index, (byte)ccIDX.Count_Block, count++, ref success);
                        SendCounterSettings(text[i], obj.SelectSingleNode("Counter"), ref success);
                        break;
                     case ItemType.Date:
                        SetAttribute(ClassCode.Index, (byte)ccIDX.Calendar_Block, calendar++, ref success);
                        SendDateSettings(text[i], obj.SelectSingleNode("Date"), ref success);
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
      }

      // Send Date related information
      private void SendDateSettings(string text, XmlNode d, ref bool success) {

         // Send the text
         SetAttribute(ClassCode.Print_format, (byte)ccPF.Print_Character_String, FormatDate(text), ref success);

         XmlNode n = d.SelectSingleNode("Offset");
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Year, GetAttr(n, "Year"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Month, GetAttr(n, "Month"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Day, GetAttr(n, "Day"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Hour, GetAttr(n, "Hour"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Minute, GetAttr(n, "Minute"), ref success);

         n = d.SelectSingleNode("ZeroSuppress");
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Year, GetAttr(n, "Year"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Month, GetAttr(n, "Month"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Day, GetAttr(n, "Day"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Hour, GetAttr(n, "Hour"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Minute, GetAttr(n, "Minute"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Weeks, GetAttr(n, "Week"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Day_Of_Week, GetAttr(n, "DayOfWeek"), ref success);

         n = d.SelectSingleNode("EnableSubstitution");
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Substitute_Year, GetAttr(n, "Year"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Substitute_Month, GetAttr(n, "Month"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Substitute_Day, GetAttr(n, "Day"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Substitute_Hour, GetAttr(n, "Hour"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Substitute_Minute, GetAttr(n, "Minute"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Substitute_Weeks, GetAttr(n, "Week"), ref success);
         SetAttribute(ClassCode.Calendar, (byte)ccCal.Substitute_Day_Of_Week, GetAttr(n, "DayOfWeek"), ref success);
      }

      // Send counter related information
      private void SendCounterSettings(string text, XmlNode n, ref bool success) {

         // Send the text
         SetAttribute(ClassCode.Print_format, (byte)ccPF.Print_Character_String, FormatCounter(text), ref success);

         SetAttribute(ClassCode.Count, (byte)ccCount.Initial_Value, GetAttr(n, "InitialValue"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Count_Range_1, GetAttr(n, "Range1"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Count_Range_2, GetAttr(n, "Range2"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Update_Unit_Halfway, GetAttr(n, "UpdateIP"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Update_Unit_Unit, GetAttr(n, "UpdateUnit"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Jump_From, GetAttr(n, "JumpFrom"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Jump_To, GetAttr(n, "JumpTo"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Increment_Value, GetAttr(n, "Increment"), ref success);
         string s = bool.TryParse(GetAttr(n, "CountUp"), out bool b) && !b ? "DOWN" : "UP";
         SetAttribute(ClassCode.Count, (byte)ccCount.Direction_Value, s, ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Reset_Value, GetAttr(n, "Reset"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Count_Multiplier, GetAttr(n, "Multiplier"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Availibility_Of_Zero_Suppression, GetAttr(n, "ZeroSuppression"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Type_Of_Reset_Signal, GetAttr(n, "ResetSignal"), ref success);
         SetAttribute(ClassCode.Count, (byte)ccCount.Availibility_Of_External_Count, GetAttr(n, "ExternalSignal"), ref success);
      }

      // Convert from cijConnect format to Hitachi format
      private string FormatCounter(string text) {
         string result = text;
         if (text.IndexOf("{{") < 0) {
            int lBrace = text.IndexOf('{');
            int rBrace = text.LastIndexOf('}');
            if (lBrace >= 0 && lBrace < rBrace) {
               result = text.Substring(0, lBrace) + "{{" + new string('C', rBrace - lBrace) + "}}" + text.Substring(rBrace + 1);
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
         cmdTest1 = new Button() { Text = "Delete All" };
         cmdTest2 = new Button() { Text = "Add Text" };
         cmdTest3 = new Button() { Text = "Create Message" };
         cmdTest4 = new Button() { Text = "Test4" };
         cmdTest5 = new Button() { Text = "Test5" };

         cmdTest1.Click += CmdTest1_Click;
         cmdTest2.Click += CmdTest2_Click;
         cmdTest3.Click += CmdTest3_Click;
         cmdTest4.Click += CmdTest4_Click;
         cmdTest4.Click += CmdTest5_Click;


         tab.Controls.Add(cmdTest1);
         tab.Controls.Add(cmdTest2);
         tab.Controls.Add(cmdTest3);
         tab.Controls.Add(cmdTest4);
         tab.Controls.Add(cmdTest5);
      }

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

            Utils.ResizeObject(ref R, cmdTest1, tclHeight - 6, 1, 2, 6);
            Utils.ResizeObject(ref R, cmdTest2, tclHeight - 6, 8, 2, 6);
            Utils.ResizeObject(ref R, cmdTest3, tclHeight - 6, 15, 2, 6);
            Utils.ResizeObject(ref R, cmdTest4, tclHeight - 6, 22, 2, 6);
            Utils.ResizeObject(ref R, cmdTest5, tclHeight - 6, 29, 2, 6);

            Utils.ResizeObject(ref R, cmdOpen, tclHeight - 3, 1, 2, 6);
            Utils.ResizeObject(ref R, cmdClear, tclHeight - 3, 8, 2, 6);
            Utils.ResizeObject(ref R, cmdGenerate, tclHeight - 3, 15, 2, 6);
            Utils.ResizeObject(ref R, cmdSaveAs, tclHeight - 3, 22, 2, 6);
            Utils.ResizeObject(ref R, cmdSendToPrinter, tclHeight - 3, 29, 2, 6);
         }
         R.offset = 0;
      }

      // Get the contents of one attribute
      private string GetAttribute(ClassCode Class, byte Attribute) {
         string val = string.Empty;
         AttrData attr = DataII.AttrDict[Class, Attribute];
         EIP.ReadOneAttribute(Class, Attribute, EIP.Nodata);
         if (attr.Data.Fmt == DataFormats.UTF8) {
            return EIP.FromQuoted(EIP.GetDataValue);
         }
         return EIP.GetDataValue;
      }

      // Get the value of an attribute that is known to be a decimal number
      private int GetDecimalAttribute(ClassCode Class, byte Attribute) {
         GetAttribute(Class, Attribute);
         return EIP.GetDecValue;
      }

      // Only allow buttons if conditions are right to process the request
      public void SetButtonEnables() {
         cmdSaveAs.Enabled = XMLText.Length > 0;
         cmdSendToPrinter.Enabled = xmlDoc != null;
      }

      // Examine the contents of a print message to determine its type
      private ItemType GetItemType(string text) {
         string[] s = text.Split('{');
         for (int i = 0; i < s.Length; i++) {
            int n = s[i].IndexOf('}');
            if (n >= 0) {
               for (int j = 0; j < n; j++) {
                  switch (s[i][j]) {
                     case 'C':
                        // Contains a counter character
                        // "{CCCC}"
                        return ItemType.Counter;
                     case 'Y':
                     case 'M':
                     case 'D':
                     case 'h':
                     case 'm':
                     case 's':
                     case 'T':
                     case 'W':
                     case '7':
                     case 'E':
                     case 'F':
                        // Contains a calendar character
                        // {{MM}/{DD}/{YY} {hh}:[mm]:[ss}}
                        return ItemType.Date;
                     case 'X':
                     case 'Z':
                        // Contains a Fixed or Free layout user pattern
                        // {X/n} or {Z/n} where n is the character position
                        return ItemType.Logo;
                     case ' ':
                     case '\'':
                     case '.':
                     case ';':
                     case ':':
                     case '!':
                     case ',':
                        // Half size characters are treated as text
                        // {'}{.}{:}{,}{;}{!}{ }
                        break;
                  }
               }
            }
         }
         return ItemType.Text;
      }

      private string GetValue(XmlNode node, string DefaultValue = "") {
         try {
            return node.InnerText;
         } catch {
            return DefaultValue;
         }
      }

      private int GetAttr(XmlNode node, string AttrName, int DefaultValue) {
         try {
            return Convert.ToInt32(node.Attributes[AttrName].Value);
         } catch {
            return DefaultValue;
         }
      }

      private bool GetAttr(XmlNode node, string AttrName, bool DefaultValue) {
         bool result = DefaultValue;
         string s;
         try {
            s = node.Attributes[AttrName].Value;
            switch (s) {
               case "0":
                  result = false;
                  break;
               case "1":
                  result = true;
                  break;
               default:
                  result = bool.Parse(s);
                  break;
            }
         } catch {

         }
         return result;
      }

      private string GetAttr(XmlNode node, string AttrName, string DefaultValue = "?") {
         try {
            return node.Attributes[AttrName].Value;
         } catch {
            return DefaultValue;
         }
      }

      #endregion

      #region Test Routines

      // Get the contents of one attribute
      private int GetAttribute(ClassCode Class, byte Attribute, int n, ref bool success) {
         if (success) {
            AttrData attr = DataII.AttrDict[Class, Attribute];
            byte[] data = EIP.ToBytes(n, attr.Get.Len);
            success = EIP.ReadOneAttribute(Class, Attribute, data);
            return EIP.GetDecValue;
         } else {
            return 0;
         }
      }

      // Set one attribute based on the Set Property
      private void SetAttribute(ClassCode Class, byte Attribute, int n, ref bool success) {
         if (success) {
            AttrData attr = DataII.AttrDict[Class, Attribute];
            byte[] data = EIP.ToBytes(n, attr.Set.Len);
            success = EIP.WriteOneAttribute(Class, Attribute, data);
         }
      }

      // Set one attribute based on the Set Property
      private void SetAttribute(ClassCode Class, byte Attribute, string s, ref bool success) {
         if (success) {
            AttrData attr = DataII.AttrDict[Class, Attribute];
            byte[] data = EIP.FormatOutput(s, attr.Set);
            success = EIP.WriteOneAttribute(Class, Attribute, data);
         }
      }

      // Service one attribute based on the Set Property
      private void ServiceAttribute(ClassCode Class, byte Attribute, int n, ref bool success) {
         if (success) {
            AttrData attr = DataII.AttrDict[Class, Attribute];
            byte[] data = EIP.ToBytes(n, attr.Service.Len);
            success = EIP.ServiceAttribute(Class, Attribute, data);
         }
      }

      // Delete all but 1
      private void CmdTest1_Click(object sender, EventArgs e) {
         CleanUpDisplay();
      }

      // Add text to all items
      private void CmdTest2_Click(object sender, EventArgs e) {
         SetText();
      }

      // Create a message
      private void CmdTest3_Click(object sender, EventArgs e) {
         bool success = true;
         string s;
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
                        for (int i = 0; i < 10; i++) {
                           ServiceAttribute(ClassCode.Print_format, (byte)ccPF.Add_Column, 0, ref success);
                        }
                        break;
                     case 2:
                        // Set the text
                        SetText();
                        break;
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void CmdTest4_Click(object sender, EventArgs e) {
      }

      private void CmdTest5_Click(object sender, EventArgs e) {
      }

      private bool CleanUpDisplay() {
         int cols = 0;
         bool success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Get the number of columns
               cols = GetAttribute(ClassCode.Print_format, (byte)ccPF.Number_Of_Columns, 0, ref success);
               // Column number is 0 origin
               while (success && cols > 1) {
                  // Select the column
                  SetAttribute(ClassCode.Index, (byte)ccIDX.Column, cols - 1, ref success);
                  // Delete the column
                  ServiceAttribute(ClassCode.Print_format, (byte)ccPF.Delete_Column, 0, ref success);
                  cols--;
               }
               // Select item 1
               SetAttribute(ClassCode.Index, (byte)ccIDX.Item, 1, ref success);
               // Select column 0
               SetAttribute(ClassCode.Index, (byte)ccIDX.Column, 0, ref success);
               // Set line count to 1. (Need to find out how delete single item works.)
               SetAttribute(ClassCode.Print_format, (byte)ccPF.Line_Count, 1, ref success);
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      private bool SetText() {
         int items = 0;
         bool success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Get the number of items
               items = GetAttribute(ClassCode.Print_format, (byte)ccPF.Number_Of_Items, 0, ref success);
               // Place item number in all of the items for identity
               for (int i = 1; i <= items && success; i++) {
                  // Select the item
                  SetAttribute(ClassCode.Index, (byte)ccIDX.Item, i, ref success);
                  // Set font
                  SetAttribute(ClassCode.Print_format, (byte)ccPF.Dot_Matrix, 3, ref success);
                  // Set ICS to 1
                  SetAttribute(ClassCode.Print_format, (byte)ccPF.InterCharacter_Space, 1, ref success);
                  // Insert the text
                  SetAttribute(ClassCode.Print_format, (byte)ccPF.Print_Character_String, $" {i} ", ref success);
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      #endregion

   }

}
