using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EIP_Lib {

   public partial class XML {

      #region Data Declarations

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
         Comma = 1 << 18,
         FixedPattern = 1 << 19,
         FreePattern = 1 << 20,
         Unknown = 1 << 21,
         DateCode = (1 << 12) - 2, // All the date codes combined

      }

      #endregion

      #region XML Driver Routines

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

      #endregion

      #region Send to Printer Routines

      private void SendDisplayToPrinter_Click(object sender, EventArgs e) {
         xmlDoc = new XmlDocument();
         xmlDoc.PreserveWhitespace = true;
         xmlDoc.LoadXml(txtIndentedView.Text);
         SendFileToPrinter_Click(null, null);
      }

      // Send xlmDoc to printer
      private void SendFileToPrinter_Click(object sender, EventArgs e) {
         bool success = true;
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
               success = success && CleanDisplay();
               XmlNode lab = xmlDoc.SelectSingleNode("Label");
               foreach (XmlNode l in lab.ChildNodes) {
                  if (l is XmlWhitespace)
                     continue;
                  switch (l.Name) {
                     case "Printer":
                        // Send printer wide settings
                        success = success && SendPrinterSettings(l);
                        break;
                     case "Objects":
                        // Allocate rows and columns
                        success = success && AllocateRowsColumns(l.ChildNodes);

                        // Send the objects one at a time
                        success = success && LoadObjectSettings(l.ChildNodes);

                        // Let printer allocate Calendar and Count blocks
                        success = success && EIP.SetAttribute(ccIDX.Automatic_reflection, 0);
                        success = success && EIP.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

                        // Send the objects one at a time
                        //success = success && LoadCalendarCountSettings(l.ChildNodes);

                        break;
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      public bool CleanDisplay() {
         success = true;
         // Get the number of columns
         success = EIP.GetAttribute(ccPF.Number_Of_Columns, out int cols);
         // Make things faster
         success = success && EIP.SetAttribute(ccIDX.Automatic_reflection, 1);
         //No need to delete columns if there is only one
         if (cols > 1) {
            // Select to continuously delete column 2 (0 origin on deletes)
            success = success && EIP.SetAttribute(ccIDX.Column, 1);
            // Column number is 0 origin
            while (success && cols > 1) {
               // Delete the column
               success = success && EIP.ServiceAttribute(ccPF.Delete_Column, 0);
               cols--;
            }
         }
         // Select item 1 (1 origin on Line Count)
         success = success && EIP.SetAttribute(ccIDX.Item, 1);
         // Set line count to 1. (In case column 1 has multiple lines)
         success = success && EIP.SetAttribute(ccPF.Line_Count, 1);
         // Clear any barcodes
         success = success && EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
         success = success && EIP.SetAttribute(ccPF.Barcode_Type, "None");
         // Set simple text in case Calendar or Counter was used
         success = success && EIP.SetAttribute(ccPF.Print_Character_String, "1");
         return success;
      }

      private bool AllocateRowsColumns(XmlNodeList objs) {
         bool success = true;
         int[] columns = new int[100];
         int maxCol = 0;
         // Collect information about rows and columns (both 1-origin in XML file)
         foreach (XmlNode obj in objs) {
            if (obj is XmlWhitespace)
               continue;
            XmlNode n = obj.SelectSingleNode("Location");
            if (int.TryParse(GetAttr(n, "Row"), out int row) && int.TryParse(GetAttr(n, "Column"), out int col)) {
               columns[col] = Math.Max(columns[col], row);
               maxCol = Math.Max(maxCol, col);
            } else {
               return false;
            }
         }
         // Allocate the rows and columns
         for (int i = 1; i <= maxCol && success; i++) {
            if (columns[i] == 0) {
               return false;
            }
            if (i > 1) {
               success = success && EIP.ServiceAttribute(ccPF.Add_Column);
            }
            // Should this be Column and not Item?
            success = success && EIP.SetAttribute(ccIDX.Item, i);
            success = success && EIP.SetAttribute(ccPF.Line_Count, columns[i]);
            //if(columns[i] > 1) {
            //   success = success && EIP.SetAttribute(ccIDX.Column, i);
            //   success = success && EIP.SetAttribute(ccPF.Line_Spacing, 1);
            //}

         }
         return success;
      }

      // Send the Printer Wide Settings
      private bool SendPrinterSettings(XmlNode pr) {
         success = true;
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  success = success && EIP.SetAttribute(ccPS.Character_Orientation, GetAttr(c, "Orientation"));
                  break;
               case "ContinuousPrinting":
                  success = success && EIP.SetAttribute(ccPS.Repeat_Interval, GetAttr(c, "RepeatInterval"));
                  success = success && EIP.SetAttribute(ccPS.Repeat_Count, GetAttr(c, "PrintsPerTrigger"));
                  break;
               case "TargetSensor":
                  success = success && EIP.SetAttribute(ccPS.Target_Sensor_Filter, GetAttr(c, "Filter"));
                  success = success && EIP.SetAttribute(ccPS.Targer_Sensor_Filter_Value, GetAttr(c, "SetupValue"));
                  success = success && EIP.SetAttribute(ccPS.Target_Sensor_Timer, GetAttr(c, "Timer"));
                  break;
               case "CharacterSize":
                  success = success && EIP.SetAttribute(ccPS.Character_Width, GetAttr(c, "Width"));
                  success = success && EIP.SetAttribute(ccPS.Character_Width, GetAttr(c, "Height"));
                  break;
               case "PrintStartDelay":
                  success = success && EIP.SetAttribute(ccPS.Print_Start_Delay_Reverse, GetAttr(c, "Reverse"));
                  success = success && EIP.SetAttribute(ccPS.Print_Start_Delay_Forward, GetAttr(c, "Forward"));
                  break;
               case "EncoderSettings":
                  success = success && EIP.SetAttribute(ccPS.High_Speed_Print, GetAttr(c, "HighSpeedPrinting"));
                  success = success && EIP.SetAttribute(ccPS.Pulse_Rate_Division_Factor, GetAttr(c, "Divisor"));
                  success = success && EIP.SetAttribute(ccPS.Product_Speed_Matching, GetAttr(c, "ExternalEncoder"));
                  break;
               case "InkStream":
                  success = success && EIP.SetAttribute(ccPS.Ink_Drop_Use, GetAttr(c, "InkDropUse"));
                  success = success && EIP.SetAttribute(ccPS.Ink_Drop_Charge_Rule, GetAttr(c, "ChargeRule"));
                  break;
               case "TwinNozzle":
                  // Not supported in EtherNet/IP
                  //this.LeadingCharacterControl = GetAttr(c, "LeadingCharControl", 0);
                  //this.LeadingCharacterControlWidth1 = GetAttr(c, "LeadingCharControlWidth1", 32);
                  //this.LeadingCharacterControlWidth1 = GetAttr(c, "LeadingCharControlWidth2", 32);
                  //this.NozzleSpaceAlignment = GetAttr(c, "NozzleSpaceAlignment", 0);
                  break;
               case "Substitution":
                  success = success && SendSubstitution(c);
                  break;
               case "Calendar":
                  success = success && SendCalendar(c);
                  break;
               case "Count":
                  success = success && SendCounter(c);
                  break;
            }
         }
         return success;
      }

      // Set all the values for a single substitution rule
      private bool SendSubstitution(XmlNode p) {
         bool success = true;
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
            success = success && EIP.SetAttribute(ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting, data);

            // Set the start year in the substitution rule
            attr = EIP.AttrDict[ClassCode.Index, (byte)ccSR.Start_Year];
            data = EIP.FormatOutput(attr.Set, year);
            success = success && EIP.SetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Start_Year, data);

            // Load the individual rules
            foreach (XmlNode c in p.ChildNodes) {
               switch (c.Name) {
                  case "Year":
                     success = success && SetSubValues(ccSR.Year, c, delimeter);
                     break;
                  case "Month":
                     success = success && SetSubValues(ccSR.Month, c, delimeter);
                     break;
                  case "Day":
                     success = success && SetSubValues(ccSR.Day, c, delimeter);
                     break;
                  case "Hour":
                     success = success && SetSubValues(ccSR.Hour, c, delimeter);
                     break;
                  case "Minute":
                     success = success && SetSubValues(ccSR.Minute, c, delimeter);
                     break;
                  case "Week":
                     success = success && SetSubValues(ccSR.Week, c, delimeter);
                     break;
                  case "DayOfWeek":
                     success = success && SetSubValues(ccSR.Day_Of_Week, c, delimeter);
                     break;
                  case "Skip":
                     // Do not process these nodes
                     break;
               }
            }
         }
         return success;
      }

      // Send Date related information
      private bool SendCalendar(XmlNode d) {
         bool success = true;
         XmlNode n = d.SelectSingleNode("Offset");
         if (n != null) {
            success = success && EIP.SetAttribute(ccCal.Offset_Year, GetAttr(n, "Year"));
            success = success && EIP.SetAttribute(ccCal.Offset_Month, GetAttr(n, "Month"));
            success = success && EIP.SetAttribute(ccCal.Offset_Day, GetAttr(n, "Day"));
            success = success && EIP.SetAttribute(ccCal.Offset_Hour, GetAttr(n, "Hour"));
            success = success && EIP.SetAttribute(ccCal.Offset_Minute, GetAttr(n, "Minute"));
         }

         n = d.SelectSingleNode("ZeroSuppress");
         if (n != null) {
            success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Year, GetAttr(n, "Year"));
            success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Month, GetAttr(n, "Month"));
            success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Day, GetAttr(n, "Day"));
            success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Hour, GetAttr(n, "Hour"));
            success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Minute, GetAttr(n, "Minute"));
            success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Weeks, GetAttr(n, "Week"));
            success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Day_Of_Week, GetAttr(n, "DayOfWeek"));
         }

         n = d.SelectSingleNode("EnableSubstitution");
         if (n != null) {
            success = success && EIP.SetAttribute(ccCal.Substitute_Year, GetAttr(n, "Year"));
            success = success && EIP.SetAttribute(ccCal.Substitute_Month, GetAttr(n, "Month"));
            success = success && EIP.SetAttribute(ccCal.Substitute_Day, GetAttr(n, "Day"));
            success = success && EIP.SetAttribute(ccCal.Substitute_Hour, GetAttr(n, "Hour"));
            success = success && EIP.SetAttribute(ccCal.Substitute_Minute, GetAttr(n, "Minute"));
            success = success && EIP.SetAttribute(ccCal.Substitute_Weeks, GetAttr(n, "Week"));
            success = success && EIP.SetAttribute(ccCal.Substitute_Day_Of_Week, GetAttr(n, "DayOfWeek"));
         }

         n = d.SelectSingleNode("TimeCount");
         if (n != null) {
            success = success && EIP.SetAttribute(ccCal.Time_Count_Start_Value, GetAttr(n, "Start"));
            success = success && EIP.SetAttribute(ccCal.Time_Count_End_Value, GetAttr(n, "End"));
            success = success && EIP.SetAttribute(ccCal.Time_Count_Reset_Value, GetAttr(n, "Reset"));
            success = success && EIP.SetAttribute(ccCal.Reset_Time_Value, GetAttr(n, "ResetTime"));
            success = success && EIP.SetAttribute(ccCal.Update_Interval_Value, GetAttr(n, "RenewalPeriod"));
         }

         n = d.SelectSingleNode("Shift");
         if (n != null) {
            success = success && EIP.SetAttribute(ccIDX.Item, GetAttr(n, "Number"));
            success = success && EIP.SetAttribute(ccCal.Shift_Start_Hour, GetAttr(n, "StartHour"));
            success = success && EIP.SetAttribute(ccCal.Shift_Start_Minute, GetAttr(n, "StartMinute"));
            success = success && EIP.SetAttribute(ccCal.Shift_End_Hour, GetAttr(n, "EndHour"));
            success = success && EIP.SetAttribute(ccCal.Shift_End_Minute, GetAttr(n, "EndMinute"));
         }
         return success;
      }

      // Send counter related information
      private bool SendCounter(XmlNode c) {
         bool success = true;
         if (c != null) {
            success = success && EIP.SetAttribute(ccCount.Initial_Value, GetAttr(c, "InitialValue"));
            success = success && EIP.SetAttribute(ccCount.Count_Range_1, GetAttr(c, "Range1"));
            success = success && EIP.SetAttribute(ccCount.Count_Range_2, GetAttr(c, "Range2"));
            success = success && EIP.SetAttribute(ccCount.Update_Unit_Halfway, GetAttr(c, "UpdateIP"));
            success = success && EIP.SetAttribute(ccCount.Update_Unit_Unit, GetAttr(c, "UpdateUnit"));
            success = success && EIP.SetAttribute(ccCount.Increment_Value, GetAttr(c, "Increment"));
            string s = bool.TryParse(GetAttr(c, "CountUp"), out bool b) && !b ? "Down" : "Up";
            success = success && EIP.SetAttribute(ccCount.Direction_Value, s);
            success = success && EIP.SetAttribute(ccCount.Jump_From, GetAttr(c, "JumpFrom"));
            success = success && EIP.SetAttribute(ccCount.Jump_To, GetAttr(c, "JumpTo"));
            success = success && EIP.SetAttribute(ccCount.Reset_Value, GetAttr(c, "Reset"));
            success = success && EIP.SetAttribute(ccCount.Type_Of_Reset_Signal, GetAttr(c, "ResetSignal"));
            success = success && EIP.SetAttribute(ccCount.External_Count, GetAttr(c, "ExternalSignal"));
            success = success && EIP.SetAttribute(ccCount.Zero_Suppression, GetAttr(c, "ZeroSuppression"));
            success = success && EIP.SetAttribute(ccCount.Count_Multiplier, GetAttr(c, "Multiplier"));
            success = success && EIP.SetAttribute(ccCount.Count_Skip, GetAttr(c, "Skip"));
         }
         return success;
      }

      // Set the substitution values for a class
      private bool SetSubValues(ccSR attribute, XmlNode c, string delimeter) {
         bool success = true;
         // Avoid user errors
         if (int.TryParse(GetAttr(c, "Base"), out int b)) {
            Prop prop = EIP.AttrDict[ClassCode.Substitution_rules, (byte)attribute].Set;
            string[] s = GetValue(c).Split(delimeter[0]);
            for (int i = 0; i < s.Length && success; i++) {
               int n = b + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  byte[] data = EIP.FormatOutput(prop, n, 1, s[i]);
                  success = success && EIP.SetAttribute(ClassCode.Substitution_rules, (byte)attribute, data);
               }
            }
         }
         return success;
      }

      // Load object settings
      private bool LoadObjectSettings(XmlNodeList objs) {
         success = true;
         XmlNode n;
         foreach (XmlNode obj in objs) {
            if (obj is XmlWhitespace)
               continue;

            // Get the item number of the object
            n = obj.SelectSingleNode("Location");
            if (!int.TryParse(GetAttr(n, "ItemNumber"), out int item)) {
               return false;
            }

            // Handle multiple line texts
            string[] text = GetValue(obj.SelectSingleNode("Text")).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < text.Length; i++) {
               // Point to the item
               EIP.SetAttribute(ccIDX.Item, item + i);
               // Load the text
               EIP.SetAttribute(ccPF.Print_Character_String, FormatDate(text[i]));

               n = obj.SelectSingleNode("Font");
               EIP.SetAttribute(ccPF.Dot_Matrix, n.InnerText);
               EIP.SetAttribute(ccPF.InterCharacter_Space, GetAttr(n, "InterCharacterSpace"));
               EIP.SetAttribute(ccPF.Line_Spacing, GetAttr(n, "InterLineSpace"));
               EIP.SetAttribute(ccPF.Character_Bold, GetAttr(n, "IncreasedWidth"));

               //// Get the item type
               //ItemType type = (ItemType)Enum.Parse(typeof(ItemType), GetAttr(obj, "Type"), true);
               //switch (type) {
               //   case ItemType.Text:
               //      break;
               //   case ItemType.Counter:
               //      SendCounter(obj.SelectSingleNode("Counter"));
               //      break;
               //   case ItemType.Date:
               //      n = obj.SelectSingleNode("Date");
               //      if (n != null) {
               //         SendCalendar(n);
               //      }
               //      break;
               //}
            }
         }
         return success;
      }

      // Load object settings
      private bool LoadCalendarCountSettings(XmlNodeList objs) {
         success = true;
         XmlNode n;
         foreach (XmlNode obj in objs) {
            if (obj is XmlWhitespace)
               continue;

            // Get the item number of the object
            n = obj.SelectSingleNode("Location");
            if (!int.TryParse(GetAttr(n, "ItemNumber"), out int item)) {
               return false;
            }

            // Handle multiple line texts
            string[] text = GetValue(obj.SelectSingleNode("Text")).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < text.Length; i++) {
               // Point to the item
               EIP.SetAttribute(ccIDX.Item, item + i);
               // Load the text
               EIP.SetAttribute(ccPF.Print_Character_String, FormatDate(text[i]));

               n = obj.SelectSingleNode("Font");
               EIP.SetAttribute(ccPF.Dot_Matrix, n.InnerText);
               EIP.SetAttribute(ccPF.InterCharacter_Space, GetAttr(n, "InterCharacterSpace"));
               EIP.SetAttribute(ccPF.Line_Spacing, GetAttr(n, "InterLineSpace"));
               EIP.SetAttribute(ccPF.Character_Bold, GetAttr(n, "IncreasedWidth"));

               //// Get the item type
               //ItemType type = (ItemType)Enum.Parse(typeof(ItemType), GetAttr(obj, "Type"), true);
               //switch (type) {
               //   case ItemType.Text:
               //      break;
               //   case ItemType.Counter:
               //      SendCounter(obj.SelectSingleNode("Counter"));
               //      break;
               //   case ItemType.Date:
               //      n = obj.SelectSingleNode("Date");
               //      if (n != null) {
               //         SendCalendar(n);
               //      }
               //      break;
               //}
            }
         }
         return success;
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
