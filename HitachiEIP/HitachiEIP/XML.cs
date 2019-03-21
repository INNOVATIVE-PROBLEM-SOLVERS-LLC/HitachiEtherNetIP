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
      Button cmdDeleteAll;
      Button cmdAddText;
      Button cmdCreateText;
      Button cmdCreateDate;
      Button cmdCreateCounter;
      Button cmdSaveInPrinter;
      Button cmdTest;

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
         success = true;
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

               int itemCount = GetDecimalAttribute(ccPF.Number_Of_Items);
               for (int i = 0; i < itemCount; i++) {
                  if (EIP.StartSession()) {
                     if (EIP.ForwardOpen()) {

                        SetAttribute(ccIDX.Item, i + 1);
                        string text = GetAttribute(ccPF.Print_Character_String);

                        itemType = GetItemType(text);

                        writer.WriteStartElement("Object"); // Start Object

                        writer.WriteAttributeString("Type", Enum.GetName(typeof(ItemType), itemType));

                        writer.WriteStartElement("Font"); // Start Font
                        {
                           writer.WriteAttributeString("HumanReadableFont", GetAttribute(ccPF.Readable_Code));
                           writer.WriteAttributeString("EANPrefix", GetAttribute(ccPF.Prefix_Code));
                           writer.WriteAttributeString("BarCode", GetAttribute(ccPF.Barcode_Type));
                           writer.WriteAttributeString("IncreasedWidth", GetAttribute(ccPF.Character_Bold));
                           writer.WriteAttributeString("InterLineSpace", GetAttribute(ccPF.Line_Spacing));
                           writer.WriteAttributeString("InterCharacterSpace", GetAttribute(ccPF.InterCharacter_Space));
                           writer.WriteString(GetAttribute(ccPF.Dot_Matrix));
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
      private void WriteCalendarSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Date"); // Start Date
         {
            //writer.WriteAttributeString("Format", p.RawText);

            writer.WriteStartElement("Offset"); // Start Offset
            {
               writer.WriteAttributeString("Minute", GetAttribute(ccCal.Offset_Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ccCal.Offset_Hour));
               writer.WriteAttributeString("Day", GetAttribute(ccCal.Offset_Day));
               writer.WriteAttributeString("Month", GetAttribute(ccCal.Offset_Month));
               writer.WriteAttributeString("Year", GetAttribute(ccCal.Offset_Year));
            }
            writer.WriteEndElement(); // End Offset

            writer.WriteStartElement("ZeroSuppress"); // Start ZeroSuppress
            {
               writer.WriteAttributeString("DayOfWeek", GetAttribute(ccCal.Zero_Suppress_Day_Of_Week));
               writer.WriteAttributeString("Week", GetAttribute(ccCal.Zero_Suppress_Weeks));
               writer.WriteAttributeString("Minute", GetAttribute(ccCal.Zero_Suppress_Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ccCal.Zero_Suppress_Hour));
               writer.WriteAttributeString("Day", GetAttribute(ccCal.Zero_Suppress_Day));
               writer.WriteAttributeString("Month", GetAttribute(ccCal.Zero_Suppress_Month));
               writer.WriteAttributeString("Year", GetAttribute(ccCal.Zero_Suppress_Year));
            }
            writer.WriteEndElement(); // End ZeroSuppress

            writer.WriteStartElement("EnableSubstitution"); // Start EnableSubstitution
            {
               //writer.WriteAttributeString("SubstitutionRule", p.DTSubRule);
               writer.WriteAttributeString("DayOfWeek", GetAttribute(ccSR.Day_Of_Week));
               writer.WriteAttributeString("Week", GetAttribute(ccSR.Week));
               writer.WriteAttributeString("Minute", GetAttribute(ccSR.Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ccSR.Hour));
               writer.WriteAttributeString("Day", GetAttribute(ccSR.Day));
               writer.WriteAttributeString("Month", GetAttribute(ccSR.Month));
               writer.WriteAttributeString("Year", GetAttribute(ccSR.Year));
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
         success = true;
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  SetAttribute(ccPS.Character_Orientation, GetAttr(c, "Orientation"));
                  break;
               case "ContinuousPrinting":
                  SetAttribute(ccPS.Repeat_Interval, GetAttr(c, "RepeatInterval"));
                  SetAttribute(ccPS.Repeat_Count, GetAttr(c, "PrintsPerTrigger"));
                  break;
               case "TargetSensor":
                  SetAttribute(ccPS.Target_Sensor_Filter, GetAttr(c, "Filter"));
                  SetAttribute(ccPS.Targer_Sensor_Filter_Value, GetAttr(c, "SetupValue"));
                  SetAttribute(ccPS.Target_Sensor_Timer, GetAttr(c, "Timer"));
                  break;
               case "CharacterSize":
                  SetAttribute(ccPS.Character_Width, GetAttr(c, "Width"));
                  SetAttribute(ccPS.Character_Width, GetAttr(c, "Height"));
                  break;
               case "PrintStartDelay":
                  SetAttribute(ccPS.Print_Start_Delay_Reverse, GetAttr(c, "Reverse"));
                  SetAttribute(ccPS.Print_Start_Delay_Forward, GetAttr(c, "Forward"));
                  break;
               case "EncoderSettings":
                  SetAttribute(ccPS.High_Speed_Print, GetAttr(c, "HighSpeedPrinting"));
                  SetAttribute(ccPS.Pulse_Rate_Division_Factor, GetAttr(c, "Divisor"));
                  SetAttribute(ccPS.Product_Speed_Matching, GetAttr(c, "ExternalEncoder"));
                  break;
               case "InkStream":
                  SetAttribute(ccPS.Ink_Drop_Use, GetAttr(c, "InkDropUse"));
                  SetAttribute(ccPS.Ink_Drop_Charge_Rule, GetAttr(c, "ChargeRule"));
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
            data = EIP.FormatOutput(ruleNumber, attr.Set);
            EIP.SetAttribute(ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting, data);

            // Set the start year in the substitution rule
            attr = EIP.AttrDict[ClassCode.Index, (byte)ccSR.Start_Year];
            data = EIP.FormatOutput(year, attr.Set);
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
            SetAttribute(ccCal.Offset_Year, GetAttr(n, "Year"));
            SetAttribute(ccCal.Offset_Month, GetAttr(n, "Month"));
            SetAttribute(ccCal.Offset_Day, GetAttr(n, "Day"));
            SetAttribute(ccCal.Offset_Hour, GetAttr(n, "Hour"));
            SetAttribute(ccCal.Offset_Minute, GetAttr(n, "Minute"));
         }

         n = d.SelectSingleNode("ZeroSuppress");
         if (n != null) {
            SetAttribute(ccCal.Zero_Suppress_Year, GetAttr(n, "Year"));
            SetAttribute(ccCal.Zero_Suppress_Month, GetAttr(n, "Month"));
            SetAttribute(ccCal.Zero_Suppress_Day, GetAttr(n, "Day"));
            SetAttribute(ccCal.Zero_Suppress_Hour, GetAttr(n, "Hour"));
            SetAttribute(ccCal.Zero_Suppress_Minute, GetAttr(n, "Minute"));
            SetAttribute(ccCal.Zero_Suppress_Weeks, GetAttr(n, "Week"));
            SetAttribute(ccCal.Zero_Suppress_Day_Of_Week, GetAttr(n, "DayOfWeek"));
         }

         n = d.SelectSingleNode("EnableSubstitution");
         if (n != null) {
            SetAttribute(ccCal.Substitute_Year, GetAttr(n, "Year"));
            SetAttribute(ccCal.Substitute_Month, GetAttr(n, "Month"));
            SetAttribute(ccCal.Substitute_Day, GetAttr(n, "Day"));
            SetAttribute(ccCal.Substitute_Hour, GetAttr(n, "Hour"));
            SetAttribute(ccCal.Substitute_Minute, GetAttr(n, "Minute"));
            SetAttribute(ccCal.Substitute_Weeks, GetAttr(n, "Week"));
            SetAttribute(ccCal.Substitute_Day_Of_Week, GetAttr(n, "DayOfWeek"));
         }

         n = d.SelectSingleNode("TimeCount");
         if (n != null) {
            SetAttribute(ccCal.Time_Count_Start_Value, GetAttr(n, "Start"));
            SetAttribute(ccCal.Time_Count_End_Value, GetAttr(n, "End"));
            SetAttribute(ccCal.Time_Count_Reset_Value, GetAttr(n, "Reset"));
            SetAttribute(ccCal.Reset_Time_Value, GetAttr(n, "ResetTime"));
            SetAttribute(ccCal.Update_Interval_Value, GetAttr(n, "RenewalPeriod"));
         }

         n = d.SelectSingleNode("Shift");
         if (n != null) {
            SetAttribute(ccIDX.Item, GetAttr(n, "Number"));
            SetAttribute(ccCal.Shift_Start_Hour, GetAttr(n, "StartHour"));
            SetAttribute(ccCal.Shift_Start_Minute, GetAttr(n, "StartMinute"));
            SetAttribute(ccCal.Shift_End_Hour, GetAttr(n, "EndHour"));
            SetAttribute(ccCal.Shift_End_Minute, GetAttr(n, "EndMinute"));
         }
      }

      // Send counter related information
      private void SendCounter(XmlNode c) {

         XmlNode n = c.SelectSingleNode("Counter");
         if (n != null) {
            SetAttribute(ccCount.Initial_Value, GetAttr(n, "InitialValue"));
            SetAttribute(ccCount.Count_Range_1, GetAttr(n, "Range1"));
            SetAttribute(ccCount.Count_Range_2, GetAttr(n, "Range2"));
            SetAttribute(ccCount.Update_Unit_Halfway, GetAttr(n, "UpdateIP"));
            SetAttribute(ccCount.Update_Unit_Unit, GetAttr(n, "UpdateUnit"));
            SetAttribute(ccCount.Jump_From, GetAttr(n, "JumpFrom"));
            SetAttribute(ccCount.Jump_To, GetAttr(n, "JumpTo"));
            SetAttribute(ccCount.Increment_Value, GetAttr(n, "Increment"));
            string s = bool.TryParse(GetAttr(n, "CountUp"), out bool b) && !b ? "DOWN" : "UP";
            SetAttribute(ccCount.Direction_Value, s);
            SetAttribute(ccCount.Reset_Value, GetAttr(n, "Reset"));
            SetAttribute(ccCount.Count_Multiplier, GetAttr(n, "Multiplier"));
            SetAttribute(ccCount.Zero_Suppression, GetAttr(n, "ZeroSuppression"));
            SetAttribute(ccCount.Type_Of_Reset_Signal, GetAttr(n, "ResetSignal"));
            SetAttribute(ccCount.External_Count, GetAttr(n, "ExternalSignal"));
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
                  byte[] data = EIP.FormatOutput(n, 1, s[i], prop);
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
            if (!(obj is XmlWhitespace)) {
               // Get the item type
               type = (ItemType)Enum.Parse(typeof(ItemType), GetAttr(obj, "Type"), true);
               // Handle multiple line texts
               string[] text = GetValue(obj.SelectSingleNode("Text")).Split(new string[] { "\r\n" }, StringSplitOptions.None);
               for (int i = 0; i < text.Length; i++) {
                  // Printer always has one item
                  if (item > 1) {
                     // Add an item <TODO> Need to add item, not column
                     ServiceAttribute(ccPF.Add_Column, 0);
                  }

                  // Point to the item
                  SetAttribute(ccIDX.Item, item);

                  // Set the common parameters
                  n = obj.SelectSingleNode("Location");
                  //x = GetAttr(n, "Left", 0);
                  //y = GetAttr(n, "Top", 0) - GetAttr(n, "Height", 0);
                  //int r = GetAttr(n, "Row", -1);
                  //int c = GetAttr(n, "Column", -1);

                  n = obj.SelectSingleNode("Font");
                  SetAttribute(ccPF.Dot_Matrix, n.InnerText);
                  SetAttribute(ccPF.InterCharacter_Space, GetAttr(n, "InterCharacterSpace"));
                  SetAttribute(ccPF.Line_Spacing, GetAttr(n, "InterLineSpace"));
                  SetAttribute(ccPF.Character_Bold, GetAttr(n, "IncreasedWidth"));

                  //p = new TPB(this, type, x, y, F, ICS, ILS, IW);

                  //p.Row = r;
                  //p.Column = c;

                  //p.BarCode = GetAttr(n, "BarCode", "(None)");
                  //p.HumanReadableFont = GetAttr(n, "HumanReadableFont", "(None)");
                  //p.EANPrefix = GetAttr(n, "EANPrefix", "00");

                  switch (type) {
                     case ItemType.Text:
                        SetAttribute(ccPF.Print_Character_String, text[i]);
                        break;
                     case ItemType.Logo:
                        break;
                     case ItemType.Counter:
                        SetAttribute(ccIDX.Count_Block, count++);
                        SendCounter(n);
                        SetAttribute(ccPF.Print_Character_String, FormatCounter(text[i]));
                        break;
                     case ItemType.Date:
                        SetAttribute(ccIDX.Calendar_Block, calendar++);
                        n = obj.SelectSingleNode("Date");
                        if (n != null) {
                           SendCalendar(n);
                        }
                        SetAttribute(ccPF.Print_Character_String, FormatDate(text[i]));
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
         cmdDeleteAll = new Button() { Text = "Delete All" };
         cmdAddText = new Button() { Text = "Add Text" };
         cmdCreateText = new Button() { Text = "Create Text" };
         cmdCreateDate = new Button() { Text = "Create Date" };
         cmdCreateCounter = new Button() { Text = "Create Counter" };
         cmdSaveInPrinter = new Button() { Text = "Save In Printer" };
         cmdTest = new Button() { Text = "Test" };

         cmdDeleteAll.Click += cmdDeleteAll_Click;
         cmdAddText.Click += cmdAddText_Click;
         cmdCreateText.Click += cmdCreateText_Click;
         cmdCreateDate.Click += cmdCreateDate_Click;
         cmdCreateCounter.Click += cmdCreateCounter_Click;
         cmdSaveInPrinter.Click += cmdSaveToPrinter_Click;
         cmdTest.Click += cmdTest_Click;


         tab.Controls.Add(cmdDeleteAll);
         tab.Controls.Add(cmdAddText);
         tab.Controls.Add(cmdCreateText);
         tab.Controls.Add(cmdCreateDate);
         tab.Controls.Add(cmdCreateCounter);
         tab.Controls.Add(cmdSaveInPrinter);
         tab.Controls.Add(cmdTest);
      }

      private void cmdTest_Click(object sender, EventArgs e) {

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

            Utils.ResizeObject(ref R, cmdDeleteAll, tclHeight - 6, 1, 2, 5);
            Utils.ResizeObject(ref R, cmdOpen, tclHeight - 3, 1, 2, 5);

            Utils.ResizeObject(ref R, cmdSaveAs, tclHeight - 6, 6.5f, 2, 5);
            Utils.ResizeObject(ref R, cmdClear, tclHeight - 3, 6.5f, 2, 5);

            Utils.ResizeObject(ref R, cmdGenerate, tclHeight - 6, 12, 2, 5);
            Utils.ResizeObject(ref R, cmdSendToPrinter, tclHeight - 3, 12, 2, 5);

            Utils.ResizeObject(ref R, cmdAddText, tclHeight - 6, 17.5f, 2, 5);
            Utils.ResizeObject(ref R, cmdCreateDate, tclHeight - 3, 17.5f, 2, 5);

            Utils.ResizeObject(ref R, cmdCreateText, tclHeight - 6, 23, 2, 5);
            Utils.ResizeObject(ref R, cmdCreateCounter, tclHeight - 3, 23, 2, 5);

            Utils.ResizeObject(ref R, cmdSaveInPrinter, tclHeight - 6, 28.5f, 2, 5);
            Utils.ResizeObject(ref R, cmdTest, tclHeight - 3, 28.5f, 2, 5);

         }
         R.offset = 0;
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

      #endregion

      #region Test Routines

      // Success is "Global" so the Get/Set/Service Attributes callers can avoid continuously testing it
      bool success = true;

      // Get the contents of one attribute
      private string GetAttribute<T>(T Attribute) {
         ClassCode cc = EIP.ClassCodes[Array.IndexOf(EIP.ClassCodeAttributes, typeof(T))];
         byte at = Convert.ToByte(Attribute);
         string val = string.Empty;
         AttrData attr = EIP.AttrDict[cc, at];
         EIP.GetAttribute(cc, at, EIP.Nodata);
         if (attr.Data.Fmt == DataFormats.UTF8) {
            return EIP.FromQuoted(EIP.GetDataValue);
         }
         return EIP.GetDataValue;
      }

      // Get the value of an attribute that is known to be a decimal number
      private int GetDecimalAttribute<T>(T Attribute) where T : Enum {
         AttrData attr = EIP.GetAttrData(Attribute);
         EIP.GetAttribute(attr.Class, attr.Val, EIP.Nodata);
         return EIP.GetDecValue;
      }

      // Get one attribute based on the Data Property
      private int GetAttribute<T>(T Attribute, int n) where T : Enum {
         AttrData attr = EIP.GetAttrData(Attribute);
         byte[] data = EIP.FormatOutput(n, attr.Get);
         EIP.GetAttribute(attr.Class, attr.Val, data);
         return EIP.GetDecValue;
      }

      // Set one attribute based on the Set Property
      private void SetAttribute<T>(T Attribute, int n) where T : Enum {
         if (success) {
            byte[] data;
            AttrData attr = EIP.GetAttrData(Attribute);
            if (attr.Set.Fmt == DataFormats.UTF8) {
               data = EIP.FormatOutput(n.ToString(), attr.Set);
            } else {
               data = EIP.FormatOutput(n, attr.Set);
            }
            success = EIP.SetAttribute(attr.Class, attr.Val, data);
         }
      }

      // Set one attribute based on the Set Property
      private void SetAttribute<T>(T Attribute, string s) where T : Enum {
         if (success && s != N_A) {
            AttrData attr = EIP.GetAttrData(Attribute);
            byte[] data = EIP.FormatOutput(s, attr.Set);
            success = EIP.SetAttribute(attr.Class, attr.Val, data);
         }
      }

      // Set one attribute based on the Set Property
      private void SetAttribute<T>(T Attribute, int item, string s) {
         if (success) {
            ClassCode cc = EIP.ClassCodes[Array.IndexOf(EIP.ClassCodeAttributes, typeof(T))];
            byte at = Convert.ToByte(Attribute);
            AttrData attr = EIP.AttrDict[cc, at];
            byte[] data = EIP.FormatOutput(item, 1, s, attr.Set);
            success = EIP.SetAttribute(cc, at, data);
         }
      }

      // Service one attribute based on the Set Property
      private void ServiceAttribute<T>(T Attribute, int n) {
         if (success) {
            ClassCode cc = EIP.ClassCodes[Array.IndexOf(EIP.ClassCodeAttributes, typeof(T))];
            byte at = Convert.ToByte(Attribute);
            AttrData attr = EIP.AttrDict[cc, at];
            byte[] data = EIP.ToBytes(n, attr.Service.Len);
            success = EIP.ServiceAttribute(cc, at, data);
         }
      }

      // Delete all but 1
      private void cmdDeleteAll_Click(object sender, EventArgs e) {
         CleanUpDisplay();
      }

      // Add text to all items
      private void cmdAddText_Click(object sender, EventArgs e) {
         SetText();
      }

      // Create a message with text only
      private void cmdCreateText_Click(object sender, EventArgs e) {
         success = true;
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
                        for (int i = 0; i < 5; i++) {
                           ServiceAttribute(ccPF.Add_Column, 0);
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

      // Create a message containing a counter
      private void cmdCreateCounter_Click(object sender, EventArgs e) {
         success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Clean up the display
               CleanUpDisplay();
               SetText();

               // Set to first item
               int item = 1;

               // Select item #1
               SetAttribute(ccIDX.Item, item);

               // Set item number in count block
               SetAttribute(ccIDX.Count_Block, item);

               // Set font, ICS, and Text is a 4 digit counter
               SetAttribute(ccPF.Dot_Matrix, "5x8");
               SetAttribute(ccPF.InterCharacter_Space, 1);
               SetAttribute(ccPF.Print_Character_String, "{{CCCC}}");

               // Set <Counter InitialValue="0001" Range1="0000" Range2="9999" JumpFrom="6666" JumpTo ="7777"
               //      Increment="1" Direction="Up" ZeroSuppression="Enable" UpdateIP="0" UpdateUnit="1"
               //      Multiplier ="2" CountSkip="0" Reset="0001" ExternalSignal="Disable" ResetSignal="Signal 1" />
               SetAttribute(ccCount.Initial_Value, "0001");
               SetAttribute(ccCount.Count_Range_1, "0000");
               SetAttribute(ccCount.Count_Range_2, "9999");
               SetAttribute(ccCount.Jump_From, "6666");
               SetAttribute(ccCount.Jump_To, "7777");
               SetAttribute(ccCount.Increment_Value, 1);
               SetAttribute(ccCount.Direction_Value, "Up");
               SetAttribute(ccCount.Zero_Suppression, "Enable");
               SetAttribute(ccCount.Count_Multiplier, "2");
               SetAttribute(ccCount.Reset_Value, "0001");
               SetAttribute(ccCount.Count_Skip, "0");

               //SetAttribute(ccCount.Update_Unit_Halfway, 0);           // Causes COM Error
               //SetAttribute(ccCount.Update_Unit_Unit, 1);              // Causes COM Error
               //SetAttribute(ccCount.Type_Of_Reset_Signal, "Signal 1"); // Causes COM Error
               //SetAttribute(ccCount.External_Count, "Disable");        // Causes COM Error
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

      // Create a message containing a date
      private void cmdCreateDate_Click(object sender, EventArgs e) {
         success = true;
         int Item = 1;
         int Rule = 2;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Clean up the display
               CleanUpDisplay();
               SetText();

               // Step thru parts of the test
               for (int i = 0; i < 5; i++) {
                  switch (i) {
                     case 0:
                        //BuildMonthDaySR(Rule);
                        break;
                     case 1:
                        //BuildTimeCount(Item++);
                        break;
                     case 2:
                        //BuildMDYhms(Item++, Rule);
                        break;
                     case 3:
                        BuildShifts(Item++);
                        break;
                     case 4:
                        //TryDayOfWeekEtc(Item++);
                        break;
                     case 5:
                        //VerifyShifts(Item++);
                        break;
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void TryDayOfWeekEtc(int Item) {
         // Add the item if needed and select it
         if (Item != 1) {
            ServiceAttribute(ccPF.Add_Column, 0);
         }
         SetAttribute(ccIDX.Item, Item);

         // Set Item in Calendar Index
         SetAttribute(ccIDX.Calendar_Block, Item);


         SetAttribute(ccPF.Dot_Matrix, "5x8");
         SetAttribute(ccPF.InterCharacter_Space, 1);
         SetAttribute(ccPF.Print_Character_String, "=>{{77}-{WW}-{TTT}}<=");

         SetAttribute(ccCal.Substitute_Weeks, "Disable");
         SetAttribute(ccCal.Substitute_Day_Of_Week, "Ensable");

         SetAttribute(ccCal.Zero_Suppress_Weeks, "Disable");
         SetAttribute(ccCal.Zero_Suppress_Day_Of_Week, "Character Fill");
      }

      private void VerifyShifts(int Item) {

         // Select the Item
         SetAttribute(ccIDX.Item, Item);

         // For testing purposes, try to read then back
         SetAttribute(ccIDX.Calendar_Block, 1);
         int sh1 = GetAttribute(ccCal.Shift_Start_Hour, 0);
         int sm1 = GetAttribute(ccCal.Shift_Start_Minute, 0);
         int eh1 = GetAttribute(ccCal.Shift_End_Hour, 11);
         int em1 = GetAttribute(ccCal.Shift_End_Minute, 59);
         string sv1 = GetAttribute(ccCal.Shift_String_Value);

         // For testing putposes, try to read then back
         SetAttribute(ccIDX.Calendar_Block, 2);
         int sh2 = GetAttribute(ccCal.Shift_Start_Hour, 12);
         int sm2 = GetAttribute(ccCal.Shift_Start_Minute, 0);
         int eh2 = GetAttribute(ccCal.Shift_End_Hour, 23);
         int em2 = GetAttribute(ccCal.Shift_End_Minute, 59);
         string sv2 = GetAttribute(ccCal.Shift_String_Value);
      }

      private void BuildShifts(int Item) {
         // Add the item if needed and select it
         if (Item != 1) {
            ServiceAttribute(ccPF.Add_Column, 0);
         }
         SetAttribute(ccIDX.Item, Item);

         // Set Item in Calendar Index
         SetAttribute(ccIDX.Calendar_Block, Item);

         SetAttribute(ccPF.Dot_Matrix, "5x8");
         SetAttribute(ccPF.InterCharacter_Space, 1);
         SetAttribute(ccPF.Print_Character_String, "=>{{EE}}<=");

         // Set < Shift Number="1" StartHour="00" StartMinute="00" EndHour="11" EndMinute="59" Text="AA" />
         SetAttribute(ccIDX.Calendar_Block, 1);
         SetAttribute(ccCal.Shift_Start_Hour, 0);
         SetAttribute(ccCal.Shift_Start_Minute, 0);
         SetAttribute(ccCal.Shift_String_Value, "AA");

         // Set < Shift Number="2" StartHour="12" StartMinute="00" EndHour="23" EndMinute="59" Text="BB" />
         SetAttribute(ccIDX.Calendar_Block, 2);
         SetAttribute(ccCal.Shift_Start_Hour, 12);
         SetAttribute(ccCal.Shift_Start_Minute, 0);
         SetAttribute(ccCal.Shift_String_Value, "BB");
      }

      private void BuildMDYhms(int Item, int Rule) {
         // Add the item if needed and select it
         if (Item != 1) {
            ServiceAttribute(ccPF.Add_Column, 0);
         }
         SetAttribute(ccIDX.Item, Item);

         // Point to first substitution rule
         SetAttribute(ccIDX.Substitution_Rules_Setting, Rule);

         // Set Item in Calendar Index
         SetAttribute(ccIDX.Calendar_Block, Item);

         // Set font, ICS, and Text
         SetAttribute(ccPF.Dot_Matrix, "5x8");
         SetAttribute(ccPF.InterCharacter_Space, 1);
         SetAttribute(ccPF.Print_Character_String, "{{MMM}/{DD}/{YY} {hh}:{mm}:{ss}}");

         // Set <EnableSubstitution SubstitutionRule="01" Year="False" Month="True"  Day="False" 
         //      Hour ="False" Minute="False" Week="False" DayOfWeek="False" />
         SetAttribute(ccCal.Substitute_Year, "Disable");
         SetAttribute(ccCal.Substitute_Month, "Enable");
         SetAttribute(ccCal.Substitute_Day, "Disable");
         SetAttribute(ccCal.Substitute_Hour, "Disable");
         SetAttribute(ccCal.Substitute_Minute, "Disable");

         // Set <Offset Year="1" Month="2" Day="3" Hour="-4" Minute="-5" />
         SetAttribute(ccCal.Offset_Year, 1);
         SetAttribute(ccCal.Offset_Month, 2);
         SetAttribute(ccCal.Offset_Day, 3);
         SetAttribute(ccCal.Offset_Hour, 4);
         SetAttribute(ccCal.Offset_Minute, -5);

         // Set <ZeroSuppress Year="Disable" Month="Disable" Day="Disable"
         //      Hour ="Space Fill" Minute="Character Fill" />
         SetAttribute(ccCal.Zero_Suppress_Year, "Disable");
         SetAttribute(ccCal.Zero_Suppress_Month, "Disable");
         SetAttribute(ccCal.Zero_Suppress_Day, "Disable");
         SetAttribute(ccCal.Zero_Suppress_Hour, "Space Fill");
         SetAttribute(ccCal.Zero_Suppress_Minute, "Character Fill");
      }

      private void BuildTimeCount(int Item) {
         // Add the item if needed and select it
         if (Item != 1) {
            ServiceAttribute(ccPF.Add_Column, 0);
         }
         SetAttribute(ccIDX.Item, Item);

         // Set Item in Calendar Index
         SetAttribute(ccIDX.Calendar_Block, Item);

         SetAttribute(ccPF.Dot_Matrix, "5x8");
         SetAttribute(ccPF.InterCharacter_Space, 1);
         SetAttribute(ccPF.Print_Character_String, "=>{{FF}}<=");

         // Set <TimeCount Start="AA" End="JJ" Reset="AA" ResetTime="6" RenewalPeriod="30 Minutes" />
         SetAttribute(ccCal.Time_Count_Start_Value, "AA");
         SetAttribute(ccCal.Time_Count_End_Value, "KK");
         SetAttribute(ccCal.Time_Count_Reset_Value, "AA");
         SetAttribute(ccCal.Reset_Time_Value, 6);
         SetAttribute(ccCal.Update_Interval_Value, "30 Minutes");

      }

      private void BuildMonthDaySR(int Rule) {
         // Set <Substitution Rule="01" StartYear="2010" Delimeter="/">
         char delimeter = '/';
         SetAttribute(ccIDX.Substitution_Rules_Setting, Rule);
         SetAttribute(ccSR.Start_Year, 2010);

         // Set <Month Base="1">JAN/FEB/MAR/APR/MAY/JUN/JUL/AUG/SEP/OCT/NOV/DEC</Month>
         string[] months = "JAN/FEB/MAR/APR/MAY/JUN/JUL/AUG/SEP/OCT/NOV/DEC".Split(delimeter);
         for (int i = 0; i < months.Length; i++) {
            SetAttribute(ccSR.Month, i + 1, months[i]);
         }

         // Set <DayOfWeek Base="1">MON/TUE/WED/THU/FRI/SAT/SUN</DayOfWeek>
         string[] day = "MON/TUE/WED/THU/FRI/SAT/SUN".Split(delimeter);
         for (int i = 0; i < day.Length; i++) {
            SetAttribute(ccSR.Day_Of_Week, i + 1, day[i]);
         }
      }

      private bool CleanUpDisplay() {
         int cols = 0;
         success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Get the number of columns
               cols = GetAttribute(ccPF.Number_Of_Columns, 0);
               // Column number is 0 origin
               while (success && cols > 1) {
                  // Select the column
                  SetAttribute(ccIDX.Column, cols - 1);
                  // Delete the column
                  ServiceAttribute(ccPF.Delete_Column, 0);
                  cols--;
               }
               // Select item 1
               SetAttribute(ccIDX.Item, 1);
               // Select column 0
               SetAttribute(ccIDX.Column, 0);
               // Set line count to 1. (Need to find out how delete single item works.)
               SetAttribute(ccPF.Line_Count, 1);
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      private bool SetText() {
         int items = 0;
         success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Get the number of items
               items = GetAttribute(ccPF.Number_Of_Items, 0);
               // Place item number in all of the items for identity
               for (int i = 1; i <= items && success; i++) {
                  // Select the item
                  SetAttribute(ccIDX.Item, i);
                  // Set font
                  SetAttribute(ccPF.Dot_Matrix, "5x8");
                  // Set ICS to 1
                  SetAttribute(ccPF.InterCharacter_Space, 1);
                  // Insert the text
                  SetAttribute(ccPF.Print_Character_String, $" {i} ");
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
