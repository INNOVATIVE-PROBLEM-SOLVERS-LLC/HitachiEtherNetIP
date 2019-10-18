using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EIP_Lib {

   public partial class EIP {

      #region Data Declarations

      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
         Link = 5,     // Not supported in the printer
         Prompt = 6,   // Not supported in the printer
      }

      // Braced Characters (count, date, half-size, logos
      readonly char[] bc = new char[] { 'C', 'Y', 'M', 'D', 'h', 'm', 's', 'T', 'W', '7', 'E', 'F', ' ', '\'', '.', ';', ':', '!', ',', 'X', 'Z' };

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
         //DateCode = (1 << 12) - 2, // All the date codes combined
      }

      const int DateCode =
         (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute | (int)ba.Second |
         (int)ba.Julian | (int)ba.Week | (int)ba.DayOfWeek | (int)ba.Shift | (int)ba.TimeCount;

      const int DateOffset =
        (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute | (int)ba.Second |
        (int)ba.Julian | (int)ba.Week | (int)ba.DayOfWeek;

      const int DateSubZS =
         (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute |
         (int)ba.Week | (int)ba.DayOfWeek;

      const int DateUseSubRule = DateOffset;

      // Flag for Attribute Not Present
      const string N_A = "N!A";

      // A list of all items in XML file
      List<XmlNode> Items;

      #endregion

      #region Send XML to Printer

      // Send xml file to printer
      public bool SendXmlToPrinter(string FileName, bool UseAutoReflection = false) {
         XmlDocument xmlDoc = new XmlDocument { PreserveWhitespace = true };
         xmlDoc.Load(FileName);
         return SendXmlToPrinter(xmlDoc, UseAutoReflection);
      }

      // Send xlmDoc from file to printer
      public bool SendXmlToPrinter(XmlDocument xmlDoc, bool UseAutoReflection = false) {
         // Need a XMP Document to continue
         if (xmlDoc == null) {
            return false;
         }
         bool success = true;
         if (StartSession()) {
            if (ForwardOpen()) {
               try {
                  this.UseAutomaticReflection = UseAutoReflection;
                  // Set to only one item in printer
                  DeleteAllButOne();

                  XmlNode objs = xmlDoc.SelectSingleNode("Label/Message");
                  if (objs != null) {
                     success = AllocateRowsColumns(objs.ChildNodes); // Allocate rows and columns
                  }

                  XmlNode prnt = xmlDoc.SelectSingleNode("Label/Printer");
                  if (success && prnt != null) {
                     success = SendPrinterSettings(prnt);            // Send printer wide settings
                  }
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
                  success = false;
               } catch {
                  // You are on your own here
               } finally {
                  this.UseAutomaticReflection = false;
               }
            }
            ForwardClose();
         }
         EndSession();
         return success;
      }

      // Simulate Delete All But One
      public void DeleteAllButOne() {
         GetAttribute(ccPF.Number_Of_Columns, out int cols); // Get the number of columns
         if (cols > 1) {                                     // No need to delete columns if there is only one
            SetAttribute(ccIDX.Column, 1);                   // Select to continuously delete column 2 (0 origin on deletes)
            while (--cols > 0) {                             // Delete all but one column
               ServiceAttribute(ccPF.Delete_Column, 0);
            }
         }
         SetAttribute(ccIDX.Item, 1);                    // Select item 1 (1 origin on Line Count)
         SetAttribute(ccPF.Line_Count, 1);               // Set line count to 1. (In case column 1 has multiple lines)
         SetAttribute(ccPF.Dot_Matrix, "5x8");           // Clear any barcodes
         SetAttribute(ccPF.Barcode_Type, "None");
         SetAttribute(ccPF.Print_Character_String, "1"); // Set simple text in case Calendar or Counter was used
      }

      // Send the Printer Wide Settings
      private bool SendPrinterSettings(XmlNode pr) {
         bool success = true;
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  SetAttribute(ccPS.Character_Orientation, GetXmlAttr(c, "Orientation"));
                  break;
               case "ContinuousPrinting":
                  SetAttribute(ccPS.Repeat_Interval, GetXmlAttr(c, "RepeatInterval"));
                  SetAttribute(ccPS.Repeat_Count, GetXmlAttr(c, "PrintsPerTrigger"));
                  break;
               case "TargetSensor":
                  SetAttribute(ccPS.Target_Sensor_Filter, GetXmlAttr(c, "Filter"));
                  SetAttribute(ccPS.Target_Sensor_Filter_Value, GetXmlAttr(c, "SetupValue"));
                  SetAttribute(ccPS.Target_Sensor_Timer, GetXmlAttr(c, "Timer"));
                  break;
               case "CharacterSize":
                  SetAttribute(ccPS.Character_Width, GetXmlAttr(c, "Width"));
                  SetAttribute(ccPS.Character_Height, GetXmlAttr(c, "Height"));
                  break;
               case "PrintStartDelay":
                  SetAttribute(ccPS.Print_Start_Delay_Forward, GetXmlAttr(c, "Forward"));
                  SetAttribute(ccPS.Print_Start_Delay_Reverse, GetXmlAttr(c, "Reverse"));
                  break;
               case "EncoderSettings":
                  SetAttribute(ccPS.High_Speed_Print, GetXmlAttr(c, "HighSpeedPrinting"));
                  SetAttribute(ccPS.Pulse_Rate_Division_Factor, GetXmlAttr(c, "Divisor"));
                  SetAttribute(ccPS.Product_Speed_Matching, GetXmlAttr(c, "ExternalEncoder"));
                  break;
               case "InkStream":
                  SetAttribute(ccPS.Ink_Drop_Use, GetXmlAttr(c, "InkDropUse"));
                  SetAttribute(ccPS.Ink_Drop_Charge_Rule, GetXmlAttr(c, "ChargeRule"));
                  break;
               case "Substitution":
                  SendSubstitution(c);
                  break;
            }
         }
         return success;
      }

      // Set all the values for a single substitution rule
      private bool SendSubstitution(XmlNode p) {

         bool saveAR = UseAutomaticReflection;
         UseAutomaticReflection = false;

         bool success = true;
         AttrData attr;
         byte[] data;

         // Get the standard attributes for substitution
         string rule = GetXmlAttr(p, "RuleNumber");
         string startYear = GetXmlAttr(p, "StartYear");
         string delimiter = GetXmlAttr(p, "Delimiter");

         // Avoid user errors
         if (int.TryParse(rule, out int ruleNumber) && int.TryParse(startYear, out int year) && delimiter.Length == 1) {
            // Sub Substitution rule in Index class
            SetAttribute(ccIDX.Substitution_Rule, ruleNumber);

            // Set the start year in the substitution rule
            SetAttribute(ccSR.Start_Year, year);

            // Load the individual rules
            foreach (XmlNode c in p.ChildNodes) {
               switch (c.Name) {
                  case "Rule":
                     if (Enum.TryParse(GetXmlAttr(c, "Type"), true, out ccSR type)) {
                        SetSubValues(type, c, delimiter);
                     } else {
                        LogIt($"Unknown substitution rule type =>{GetXmlAttr(c, "Type")}<=");
                     }
                     break;
               }
            }
         }

         UseAutomaticReflection = saveAR;

         return success;
      }

      // Set the substitution values for a class
      private bool SetSubValues(ccSR attribute, XmlNode c, string delimeter) {
         bool success = true;
         // Avoid user errors
         if (int.TryParse(GetXmlAttr(c, "Base"), out int b)) {
            Prop prop = EIP.AttrDict[ClassCode.Substitution_rules, (byte)attribute].Set;
            string[] s = GetXmlValue(c).Split(delimeter[0]);
            for (int i = 0; i < s.Length && success; i++) {
               int n = b + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  byte[] data = FormatOutput(prop, n, 1, s[i]);
                  SetAttribute(ClassCode.Substitution_rules, (byte)attribute, data);
               }
            }
         }
         return success;
      }

      // Allocate rows, columns, and inner-line spacing 
      private bool AllocateRowsColumns(XmlNodeList objs) {
         bool hasDateOrCount = false;
         XmlNode n;
         Items = new List<XmlNode>();
         bool success = true;
         int[] columns = new int[100];
         int[] ILS = new int[100];
         int maxCol = 0;
         // Count the rows and columns
         foreach (XmlNode col in objs) {
            if (col is XmlWhitespace)
               continue;
            switch (col.Name) {
               case "Column":
                  columns[maxCol] = 0;
                  int.TryParse(GetXmlAttr(col, "InterLineSpacing"), out ILS[maxCol]);
                  foreach (XmlNode item in col) {
                     if (item is XmlWhitespace)
                        continue;
                     switch (item.Name) {
                        case "Item":
                           Items.Add(item);
                           columns[maxCol]++;
                           break;
                     }
                  }
                  maxCol++;
                  break;
            }
         }

         // Allocate the rows and columns
         int i = 0;
         for (int col = 0; col < maxCol; col++) {
            if (columns[col] == 0) {
               return false;
            }
            if (col > 0) {
               ServiceAttribute(ccPF.Add_Column);
            }
            // Should this be Column and not Item?
            SetAttribute(ccIDX.Item, col + 1);
            SetAttribute(ccPF.Line_Count, columns[col]);
            if (columns[col] > 1) {
               SetAttribute(ccIDX.Column, col + 1);
               SetAttribute(ccPF.Line_Spacing, ILS[col]);
            }
            for (int row = 0; row < columns[col]; row++) {
               SetAttribute(ccIDX.Item, i + 1);
               XmlNode item = Items[i];
               if ((n = Items[i].SelectSingleNode("Font")) != null) {
                  SetAttribute(ccPF.Dot_Matrix, GetXmlAttr(n, "DotMatrix"));
                  SetAttribute(ccPF.InterCharacter_Space, GetXmlAttr(n, "InterCharacterSpace"));
                  SetAttribute(ccPF.Character_Bold, GetXmlAttr(n, "IncreasedWidth"));
                  SetAttribute(ccPF.Print_Character_String, GetXmlValue(Items[i].SelectSingleNode("Text"))); // Load the text last
               }
               hasDateOrCount |= Items[i].SelectSingleNode("Date") != null | Items[i].SelectSingleNode("Counter") != null;
               i++;
            }
         }
         if (hasDateOrCount) {
            SendDateCount();
         }
         return success;
      }

      // Load objects
      private bool SendDateCount() {
         bool success = true;
         bool saveAR = UseAutomaticReflection;
         UseAutomaticReflection = false;

         int[] calStart = new int[Items.Count];
         int[] calCount = new int[Items.Count];
         int[] countStart = new int[Items.Count];
         int[] countCount = new int[Items.Count];

         for (int i = 0; i < Items.Count; i++) {
            if (Items[i].SelectSingleNode("Date") != null) {
               SetAttribute(ccIDX.Item, i + 1);
               GetAttribute(ccCal.Number_of_Calendar_Blocks, out calCount[i]);
               if (calCount[i] > 0) {
                  GetAttribute(ccCal.First_Calendar_Block, out calStart[i]);
               }
            }
            if (Items[i].SelectSingleNode("Counter") != null) {
               SetAttribute(ccIDX.Item, i + 1);
               GetAttribute(ccCount.Number_Of_Count_Blocks, out countCount[i]);
               if (countCount[i] > 0) {
                  GetAttribute(ccCount.First_Count_Block, out countStart[i]);
               }
            }
         }

         UseAutomaticReflection = saveAR;

         for (int i = 0; i < Items.Count; i++) {
            if (Items[i].SelectSingleNode("Date") != null) {
               SetAttribute(ccIDX.Item, i + 1);
               LoadCalendar(Items[i], calCount[i], calStart[i]);
            }
            if (Items[i].SelectSingleNode("Counter") != null) {
               SetAttribute(ccIDX.Item, i + 1);
               LoadCount(Items[i], countCount[i], countStart[i]);
            }
         }

         Items = null;

         return success;
      }

      // Send Calendar related information
      private bool LoadCalendar(XmlNode obj, int CalBlockCount, int FirstCalBlock) {
         bool success = true;

         foreach (XmlNode d in obj) {
            if (d is XmlWhitespace)
               continue;
            if (d.Name == "Date" && int.TryParse(GetXmlAttr(d, "Block"), out int b) && b <= CalBlockCount) {
               SetAttribute(ccIDX.Calendar_Block, FirstCalBlock + b - 1);

               foreach (XmlNode n in d.ChildNodes) {
                  if (n is XmlWhitespace)
                     continue;
                  switch (n.Name) {
                     case "Offset":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (int.TryParse(a.Value, out int x) && x != 0) {
                              switch (a.Name) {
                                 case "Year":
                                    SetAttribute(ccCal.Offset_Year, x);
                                    break;
                                 case "Month":
                                    SetAttribute(ccCal.Offset_Month, x);
                                    break;
                                 case "Day":
                                    SetAttribute(ccCal.Offset_Day, x);
                                    break;
                                 case "Hour":
                                    SetAttribute(ccCal.Offset_Hour, x);
                                    break;
                                 case "Minute":
                                    SetAttribute(ccCal.Offset_Minute, x);
                                    break;
                              }
                           }
                        }
                        break;
                     case "ZeroSuppress":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (!IsDefaultValue(fmtDD.DisableSpaceChar, a.Value)) {
                              switch (a.Name) {
                                 case "Year":
                                    SetAttribute(ccCal.Zero_Suppress_Year, a.Value);
                                    break;
                                 case "Month":
                                    SetAttribute(ccCal.Zero_Suppress_Month, a.Value);
                                    break;
                                 case "Day":
                                    SetAttribute(ccCal.Zero_Suppress_Day, a.Value);
                                    break;
                                 case "Hour":
                                    SetAttribute(ccCal.Zero_Suppress_Hour, a.Value);
                                    break;
                                 case "Minute":
                                    SetAttribute(ccCal.Zero_Suppress_Minute, a.Value);
                                    break;
                                 case "Week":
                                    SetAttribute(ccCal.Zero_Suppress_Weeks, a.Value);
                                    break;
                                 case "DayOfWeek":
                                    SetAttribute(ccCal.Zero_Suppress_DayOfWeek, a.Value);
                                    break;
                              }
                           }
                        }
                        break;
                     case "Substitute":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (!IsDefaultValue(fmtDD.EnableDisable, a.Value)) {
                              switch (a.Name) {
                                 case "Year":
                                    SetAttribute(ccCal.Substitute_Year, a.Value);
                                    break;
                                 case "Month":
                                    SetAttribute(ccCal.Substitute_Month, a.Value);
                                    break;
                                 case "Day":
                                    SetAttribute(ccCal.Substitute_Day, a.Value);
                                    break;
                                 case "Hour":
                                    SetAttribute(ccCal.Substitute_Hour, a.Value);
                                    break;
                                 case "Minute":
                                    SetAttribute(ccCal.Substitute_Minute, a.Value);
                                    break;
                                 case "Week":
                                    SetAttribute(ccCal.Substitute_Weeks, a.Value);
                                    break;
                                 case "DayOfWeek":
                                    SetAttribute(ccCal.Substitute_DayOfWeek, a.Value);
                                    break;
                              }
                           }
                        }
                        break;
                     case "TimeCount":
                        foreach (XmlAttribute a in n.Attributes) {
                           switch (a.Name) {
                              case "Start":
                                 SetAttribute(ccCal.Time_Count_Start_Value, a.Value);
                                 break;
                              case "End":
                                 SetAttribute(ccCal.Time_Count_End_Value, a.Value);
                                 break;
                              case "ResetValue":
                                 SetAttribute(ccCal.Time_Count_Reset_Value, a.Value);
                                 break;
                              case "ResetTime":
                                 SetAttribute(ccCal.Reset_Time_Value, a.Value);
                                 break;
                              case "Interval":
                                 SetAttribute(ccCal.Update_Interval_Value, a.Value);
                                 break;
                           }
                        }
                        break;
                     case "Shift":
                        if (int.TryParse(GetXmlAttr(n, "ShiftNumber"), out int shift)) {
                           SetAttribute(ccIDX.Item, shift);
                           foreach (XmlAttribute a in n.Attributes) {
                              switch (a.Name) {
                                 case "StartHour":
                                    SetAttribute(ccCal.Shift_Start_Hour, a.Value);
                                    break;
                                 case "StartMinute":
                                    SetAttribute(ccCal.Shift_Start_Minute, a.Value);
                                    break;
                                 case "EndHour":
                                    //SetAttribute(ccCal.Shift_End_Hour, a.Value);  // Read Only
                                    break;
                                 case "EndMinute":
                                    //SetAttribute(ccCal.Shift_End_Minute, a.Value); // Read Only
                                    break;
                                 case "ShiftCode":
                                    SetAttribute(ccCal.Shift_String_Value, a.Value);
                                    break;
                              }
                           }
                        }
                        break;
                  }
               }
            }
         }
         return success;
      }

      // Send counter related information
      private bool LoadCount(XmlNode obj, int CountBlockCount, int FirstCountBlock) {
         bool success = true;
         XmlNode n;
         foreach (XmlNode c in obj) {
            if (c is XmlWhitespace)
               continue;
            if (c.Name == "Counter" && int.TryParse(GetXmlAttr(c, "Block"), out int b) && b <= CountBlockCount) {
               SetAttribute(ccIDX.Count_Block, FirstCountBlock + b - 1);

               if ((n = c.SelectSingleNode("Range")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "Range1":
                           SetAttribute(ccCount.Count_Range_1, a.Value);
                           break;
                        case "Range2":
                           SetAttribute(ccCount.Count_Range_2, a.Value);
                           break;
                        case "JumpFrom":
                           SetAttribute(ccCount.Jump_From, a.Value);
                           break;
                        case "JumpTo":
                           SetAttribute(ccCount.Jump_To, a.Value);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Count")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "InitialValue":
                           SetAttribute(ccCount.Initial_Value, a.Value);
                           break;
                        case "Increment":
                           SetAttribute(ccCount.Increment_Value, a.Value);
                           break;
                        case "Direction":
                           SetAttribute(ccCount.Direction_Value, a.Value);
                           break;
                        case "ZeroSuppression":
                           SetAttribute(ccCount.Zero_Suppression, a.Value);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Reset")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "Type":
                           SetAttribute(ccCount.Type_Of_Reset_Signal, a.Value);
                           break;
                        case "Value":
                           SetAttribute(ccCount.Reset_Value, a.Value);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Misc")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "UpdateIP":
                           SetAttribute(ccCount.Update_Unit_Halfway, a.Value);
                           break;
                        case "UpdateUnit":
                           SetAttribute(ccCount.Update_Unit_Unit, a.Value);
                           break;
                        case "ExternalCount":
                           SetAttribute(ccCount.External_Count, a.Value);
                           break;
                        case "Multiplier":
                           SetAttribute(ccCount.Count_Multiplier, a.Value);
                           break;
                        case "Skip":
                           SetAttribute(ccCount.Count_Skip, a.Value);
                           break;
                     }
                  }
               }
            }
         }
         return success;
      }

      #endregion

      #region Retrieve XML from printer

      // Generate an XMP Doc form the current printer settings
      public string RetrieveXML() {
         string xml = string.Empty;
         ItemType itemType;
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
               if (StartSession(true)) {
                  if (ForwardOpen()) {
                     try {
                        writer.WriteStartElement("Label"); // Start Label
                        {
                           writer.WriteAttributeString("Version", "1");
                           RetrievePrinterSettings(writer);
                           writer.WriteStartElement("Message"); // Start Message
                           {
                              writer.WriteAttributeString("Layout", GetAttribute(ccPF.Format_Type));
                              int item = 0;
                              GetAttribute(ccPF.Number_Of_Columns, out int colCount);
                              for (int col = 1; col <= colCount; col++) {
                                 SetAttribute(ccIDX.Column, col);
                                 writer.WriteStartElement("Column"); // Start Column
                                 {
                                    writer.WriteAttributeString("InterLineSpacing", GetAttribute(ccPF.Line_Spacing));

                                    GetAttribute(ccPF.Line_Count, out int LineCount);
                                    for (int row = LineCount; row > 0; row--) {
                                       SetAttribute(ccIDX.Item, ++item);
                                       GetAttribute(ccPF.Print_Character_String, out string text);
                                       GetAttribute(ccCal.Number_of_Calendar_Blocks, out int calBlocks);
                                       GetAttribute(ccCount.Number_Of_Count_Blocks, out int cntBlocks);
                                       int[] mask = new int[1 + Math.Max(calBlocks, cntBlocks)];
                                       itemType = GetItemType(text, ref mask);
                                       writer.WriteStartElement("Item"); // Start Item
                                       {
                                          writer.WriteAttributeString("Type", Enum.GetName(typeof(ItemType), itemType));

                                          RetrieveFont(writer);

                                          switch (itemType) {
                                             case ItemType.Text:
                                                break;
                                             case ItemType.Date:
                                                // Missing multiple calendar block logic
                                                RetrieveCalendarSettings(writer, mask);
                                                break;
                                             case ItemType.Counter:
                                                // Missing multiple counter block logic
                                                RetrieveCounterSettings(writer);
                                                break;
                                             case ItemType.Logo:
                                                RetrieveUserPatternSettings(writer);
                                                break;
                                             default:
                                                break;
                                          }

                                          writer.WriteElementString("Text", text);
                                       }
                                       writer.WriteEndElement(); // End Item
                                    }
                                 }
                                 writer.WriteEndElement(); // End Column
                              }
                           }
                           writer.WriteEndElement(); // End Message
                        }
                        writer.WriteEndElement(); // End Label
                     } catch (EIPIOException e1) {
                        // In case of an EIP I/O error
                        string name = $"{GetAttributeName(e1.ClassCode, e1.Attribute)}";
                        string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                        MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
                     } catch (Exception e2) {
                        // You are on your own here
                     }
                  }
                  ForwardClose();
               }
               EndSession();
               writer.WriteEndDocument();
               writer.Flush();
               ms.Position = 0;

               xml = new StreamReader(ms).ReadToEnd();
               int xmlStart = 0;
               int xmlEnd = 0;
               // Can be called with a Filename or XML text
               xmlStart = xml.IndexOf("<Label");
               if (xmlStart == -1) {
                  xml = File.ReadAllText(xml);
                  xmlStart = xml.IndexOf("<Label");
               }
               // No label found, exit
               if (xmlStart == -1) {
                  return string.Empty;
               }
               xmlEnd = xml.IndexOf("</Label>", xmlStart + 7);
               if (xmlEnd > 0) {
                  xml = xml.Substring(xmlStart, xmlEnd - xmlStart + 8);
               }
            }
         }
         return xml;
      }

      // Retrieve the global printer settings
      private void RetrievePrinterSettings(XmlTextWriter writer) {

         writer.WriteStartElement("Printer");
         {
            {
               writer.WriteAttributeString("Make", "Hitachi");
               writer.WriteAttributeString("Model", GetAttribute(ccUI.Model_Name));
            }

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
               writer.WriteAttributeString("SetupValue", GetAttribute(ccPS.Target_Sensor_Filter_Value));
               writer.WriteAttributeString("Timer", GetAttribute(ccPS.Target_Sensor_Timer));
            }
            writer.WriteEndElement(); // TargetSensor

            writer.WriteStartElement("CharacterSize");
            {
               writer.WriteAttributeString("Width", GetAttribute(ccPS.Character_Width));
               writer.WriteAttributeString("Height", GetAttribute(ccPS.Character_Height));
            }
            writer.WriteEndElement(); // CharacterSize

            writer.WriteStartElement("PrintStartDelay");
            {
               writer.WriteAttributeString("Forward", GetAttribute(ccPS.Print_Start_Delay_Forward));
               writer.WriteAttributeString("Reverse", GetAttribute(ccPS.Print_Start_Delay_Reverse));
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

            RetrieveSubstitutions(writer);

            RetrieveLogos(writer);
         }
         writer.WriteEndElement(); // Printer
      }

      private void RetrieveLogos(XmlTextWriter writer) {
         // This needs to be expanded when the printer fixes logos
         writer.WriteStartElement("Logos");
         {
            writer.WriteStartElement("Logo");
            {
            }
            writer.WriteEndElement(); // Logo
         }
         writer.WriteEndElement(); // Logos
      }

      // This is a work in progress
      private void RetrieveSubstitutions(XmlTextWriter writer) {
         // We need to figure out what substitution rules are being used
         // and which substitutions within the rule are needed.
         writer.WriteStartElement("Substitution");
         {
            writer.WriteAttributeString("Delimiter", "/");
            writer.WriteAttributeString("StartYear", GetAttribute(ccSR.Start_Year));
            writer.WriteAttributeString("RuleNumber", "1");
            //WriteSubstitution(writer, ccSR.Year, 0, 23);
            RetrieveSubstitution(writer, ccSR.Month, 1, 12);
            //WriteSubstitution(writer, ccSR.Day, 1, 31);
            //WriteSubstitution(writer, ccSR.Hour, 0, 23);
            //WriteSubstitution(writer, ccSR.Minute, 0, 59);
            //WriteSubstitution(writer, ccSR.Week, 1, 53);
            RetrieveSubstitution(writer, ccSR.DayOfWeek, 1, 7);
         }
         writer.WriteEndElement(); // Substitution
      }

      // Retrieve a single rule
      private void RetrieveSubstitution(XmlTextWriter writer, ccSR attr, int start, int end) {
         int n = end - start + 1;
         string[] subCode = new string[n];
         for (int i = 0; i < n; i++) {
            subCode[i] = GetAttribute(attr, i + start);
         }
         for (int i = 0; i < n; i += 10) {
            writer.WriteStartElement("Rule");
            {
               writer.WriteAttributeString("Type", attr.ToString().Replace("_", ""));
               writer.WriteAttributeString("Base", (i + start).ToString());
               writer.WriteString(string.Join("/", subCode, i, Math.Min(10, n - i)));
            }
            writer.WriteEndElement(); // Rule
         }
      }

      // Retrieve the Font XML
      private void RetrieveFont(XmlTextWriter writer) {
         writer.WriteStartElement("Font"); // Start Font
         {
            writer.WriteAttributeString("InterCharacterSpace", GetAttribute(ccPF.InterCharacter_Space));
            writer.WriteAttributeString("IncreasedWidth", GetAttribute(ccPF.Character_Bold));
            writer.WriteAttributeString("DotMatrix", GetAttribute(ccPF.Dot_Matrix));
         }
         writer.WriteEndElement(); // End Font

         writer.WriteStartElement("BarCode"); // Start Barcode
         {
            string BarCode = GetAttribute(ccPF.Barcode_Type);
            if (BarCode != "None") {
               writer.WriteAttributeString("HumanReadableFont", GetAttribute(ccPF.Readable_Code));
               writer.WriteAttributeString("EANPrefix", GetAttribute(ccPF.Prefix_Code));
               writer.WriteAttributeString("DotMatrix", BarCode);
            }
         }
         writer.WriteEndElement(); // End BarCode
      }

      // Retrieve the Calendar Settings
      private void RetrieveCalendarSettings(XmlTextWriter writer, int[] mask) {
         bool success = true;
         GetAttribute(ccCal.First_Calendar_Block, out int FirstBlock);
         GetAttribute(ccCal.Number_of_Calendar_Blocks, out int BlockCount);
         for (int i = 0; success && i < BlockCount; i++) {
            SetAttribute(ccIDX.Calendar_Block, FirstBlock + i);
            writer.WriteStartElement("Date"); // Start Date
            {
               writer.WriteAttributeString("Block", (i + 1).ToString());
               if ((mask[i] & DateUseSubRule) > 0) {
                  writer.WriteAttributeString("SubstitutionRule", "1");
                  writer.WriteAttributeString("RuleName", "");
               }

               if ((mask[i] & DateOffset) > 0) { // Not always needed
                  writer.WriteStartElement("Offset"); // Start Offset
                  {
                     writer.WriteAttributeString("Year", GetAttribute(ccCal.Offset_Year));
                     writer.WriteAttributeString("Month", GetAttribute(ccCal.Offset_Month));
                     writer.WriteAttributeString("Day", GetAttribute(ccCal.Offset_Day));
                     writer.WriteAttributeString("Hour", GetAttribute(ccCal.Offset_Hour));
                     writer.WriteAttributeString("Minute", GetAttribute(ccCal.Offset_Minute));
                  }
                  writer.WriteEndElement(); // End Offset
               }

               if ((mask[i] & DateSubZS) > 0) {
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
                        writer.WriteAttributeString("DayOfWeek", GetAttribute(ccCal.Zero_Suppress_DayOfWeek));
                  }
                  writer.WriteEndElement(); // End ZeroSuppress

                  writer.WriteStartElement("Substitute"); // Start Substitute
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
                        writer.WriteAttributeString("DayOfWeek", GetAttribute(ccCal.Substitute_DayOfWeek));
                  }
                  writer.WriteEndElement(); // End EnableSubstitution
               }

               if ((mask[i] & (int)ba.Shift) > 0) {
                  string endHour;
                  string endMinute;
                  int shift = 1;
                  do {
                     writer.WriteStartElement("Shift"); // Start Shift
                     {
                        SetAttribute(ccIDX.Item, shift);
                        writer.WriteAttributeString("ShiftNumber", shift.ToString());
                        writer.WriteAttributeString("StartHour", GetAttribute(ccCal.Shift_Start_Hour));
                        writer.WriteAttributeString("StartMinute", GetAttribute(ccCal.Shift_Start_Minute));
                        writer.WriteAttributeString("EndHour", endHour = GetAttribute(ccCal.Shift_End_Hour));
                        writer.WriteAttributeString("EndMinute", endMinute = GetAttribute(ccCal.Shift_End_Minute));
                        writer.WriteAttributeString("ShiftCode", GetAttribute(ccCal.Shift_String_Value));
                     }
                     writer.WriteEndElement(); // End Shift
                     shift++;
                  } while (endHour != "23" || endMinute != "59");
               }
               if ((mask[i] & (int)ba.TimeCount) > 0) {
                  writer.WriteStartElement("TimeCount"); // Start TimeCount
                  {
                     writer.WriteAttributeString("Interval", GetAttribute(ccCal.Update_Interval_Value));
                     writer.WriteAttributeString("Start", GetAttribute(ccCal.Time_Count_Start_Value));
                     writer.WriteAttributeString("End", GetAttribute(ccCal.Time_Count_End_Value));
                     writer.WriteAttributeString("ResetTime", GetAttribute(ccCal.Reset_Time_Value));
                     writer.WriteAttributeString("ResetValue", GetAttribute(ccCal.Time_Count_Reset_Value));
                  }
                  writer.WriteEndElement(); // End TimeCount
               }
            }
            writer.WriteEndElement(); // End Date
         }
      }

      // Retrieve the Counter Settings
      private void RetrieveCounterSettings(XmlTextWriter writer) {
         bool success = true;
         GetAttribute(ccCount.First_Count_Block, out int FirstBlock);
         GetAttribute(ccCount.Number_Of_Count_Blocks, out int BlockCount);
         for (int i = 0; success && i < BlockCount; i++) {
            SetAttribute(ccIDX.Count_Block, FirstBlock + i);
            writer.WriteStartElement("Counter"); // Start Counter
            {
               writer.WriteAttributeString("Block", (i + 1).ToString());
               writer.WriteStartElement("Range"); // Start Range
               {
                  writer.WriteAttributeString("Range1", GetAttribute(ccCount.Count_Range_1));
                  writer.WriteAttributeString("Range2", GetAttribute(ccCount.Count_Range_2));
                  writer.WriteAttributeString("JumpFrom", GetAttribute(ccCount.Jump_From));
                  writer.WriteAttributeString("JumpTo", GetAttribute(ccCount.Jump_To));
               }
               writer.WriteEndElement(); //  End Range

               writer.WriteStartElement("Count"); // Start Count
               {
                  writer.WriteAttributeString("InitialValue", GetAttribute(ccCount.Initial_Value));
                  writer.WriteAttributeString("Increment", GetAttribute(ccCount.Increment_Value));
                  writer.WriteAttributeString("Direction", GetAttribute(ccCount.Direction_Value));
                  writer.WriteAttributeString("ZeroSuppression", GetAttribute(ccCount.Zero_Suppression));
               }
               writer.WriteEndElement(); //  End Count

               writer.WriteStartElement("Reset"); // Start Reset
               {
                  writer.WriteAttributeString("Type", GetAttribute(ccCount.Type_Of_Reset_Signal));
                  writer.WriteAttributeString("Value", GetAttribute(ccCount.Reset_Value));
               }
               writer.WriteEndElement(); //  End Reset

               writer.WriteStartElement("Misc"); // Start Misc
               {
                  writer.WriteAttributeString("UpdateIP", GetAttribute(ccCount.Update_Unit_Halfway));
                  writer.WriteAttributeString("UpdateUnit", GetAttribute(ccCount.Update_Unit_Unit));
                  writer.WriteAttributeString("ExternalCount", GetAttribute(ccCount.External_Count));
                  writer.WriteAttributeString("Multiplier", GetAttribute(ccCount.Count_Multiplier));
                  writer.WriteAttributeString("SkipCount", GetAttribute(ccCount.Count_Skip));
               }
               writer.WriteEndElement(); //  End Misc

            }
            writer.WriteEndElement(); //  End Counter
         }
      }

      // Retrieve the User Pattern Settings
      private void RetrieveUserPatternSettings(XmlTextWriter writer) {
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

      #region Verify Load was Successful

      bool ReportAll = true;

      // Verify the printer settings vs the XML File
      public bool VerifyXmlVsPrinter(string FileName, bool ReportAll = true) {
         XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = true };
         xmlDoc.Load(FileName);
         return VerifyXmlVsPrinter(xmlDoc, ReportAll);
      }

      // Verify the printer settings vs the XML Document
      public bool VerifyXmlVsPrinter(XmlDocument xmlDoc, bool ReportAll = true) {
         this.ReportAll = ReportAll;
         // Need a XMP Document to continue
         if (xmlDoc == null) {
            return false;
         }
         bool success = true;
         if (StartSession()) {
            if (ForwardOpen()) {
               try {
                  XmlNode lab = xmlDoc.SelectSingleNode("Label");
                  foreach (XmlNode l in lab.ChildNodes) {
                     if (l is XmlWhitespace)
                        continue;
                     switch (l.Name) {
                        case "Printer":
                           VerifyPrinterSettings(l);            // Send printer wide settings
                           break;
                        case "Message":
                           VerifyRowsColumns(l.ChildNodes);     // Allocate rows and columns
                           VerifyObjects(l.ChildNodes);         // Send the objects one at a time
                           break;
                     }
                  }
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
                  success = false;
               } catch {
                  // You are on your own here
               }
            }
            ForwardClose();
         }
         EndSession();
         return success;
      }

      private void VerifyPrinterSettings(XmlNode pr) {
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  foreach (XmlAttribute a in c.Attributes) {
                     switch (a.Name) {
                        case "Orientation":
                           VerifyXml(c, "Orientation", ccPS.Character_Orientation);
                           break;
                     }
                  }
                  break;
               case "ContinuousPrinting":
                  foreach (XmlAttribute a in c.Attributes) {
                     switch (a.Name) {
                        case "RepeatInterval":
                           VerifyXml(c, "RepeatInterval", ccPS.Repeat_Interval);
                           break;
                        case "PrintsPerTrigger":
                           VerifyXml(c, "PrintsPerTrigger", ccPS.Repeat_Count);
                           break;
                     }
                  }
                  break;
               case "TargetSensor":
                  foreach (XmlAttribute a in c.Attributes) {
                     switch (a.Name) {
                        case "Filter":
                           VerifyXml(c, "Filter", ccPS.Target_Sensor_Filter);
                           break;
                        case "SetupValue":
                           VerifyXml(c, "SetupValue", ccPS.Target_Sensor_Filter_Value);
                           break;
                        case "Timer":
                           VerifyXml(c, "Timer", ccPS.Target_Sensor_Timer);
                           break;
                     }
                  }
                  break;
               case "CharacterSize":
                  foreach (XmlAttribute a in c.Attributes) {
                     switch (a.Name) {
                        case "Width":
                           VerifyXml(c, "Width", ccPS.Character_Width);
                           break;
                        case "Height":
                           VerifyXml(c, "Height", ccPS.Character_Height);
                           break;
                     }
                  }
                  break;
               case "PrintStartDelay":
                  foreach (XmlAttribute a in c.Attributes) {
                     switch (a.Name) {
                        case "Forward":
                           VerifyXml(c, "Forward", ccPS.Print_Start_Delay_Forward);
                           break;
                        case "Reverse":
                           VerifyXml(c, "Reverse", ccPS.Print_Start_Delay_Reverse);
                           break;
                     }
                  }
                  break;
               case "EncoderSettings":
                  foreach (XmlAttribute a in c.Attributes) {
                     switch (a.Name) {
                        case "HighSpeedPrinting":
                           VerifyXml(c, "HighSpeedPrinting", ccPS.High_Speed_Print);
                           break;
                        case "Divisor":
                           VerifyXml(c, "Divisor", ccPS.Pulse_Rate_Division_Factor);
                           break;
                        case "ExternalEncoder":
                           VerifyXml(c, "ExternalEncoder", ccPS.Product_Speed_Matching);
                           break;
                     }
                  }
                  break;
               case "InkStream":
                  foreach (XmlAttribute a in c.Attributes) {
                     switch (a.Name) {
                        case "InkDropUse":
                           VerifyXml(c, "InkDropUse", ccPS.Ink_Drop_Use);
                           break;
                        case "ChargeRule":
                           VerifyXml(c, "ChargeRule", ccPS.Ink_Drop_Charge_Rule);
                           break;
                     }
                  }
                  break;
               case "Substitution":
                  VerifySubstitution(c);
                  break;
            }
         }
      }

      private void VerifySubstitution(XmlNode p) {
         AttrData attr;
         byte[] data;

         // Get the standard attributes for substitution
         string rule = GetXmlAttr(p, "Rule");
         string startYear = GetXmlAttr(p, "StartYear");
         string delimiter = GetXmlAttr(p, "Delimiter");

         // Avoid user errors
         if (int.TryParse(rule, out int ruleNumber) && int.TryParse(startYear, out int year) && delimiter.Length == 1) {

            // Sub Substitution rule in Index class
            SetAttribute(ccIDX.Substitution_Rule, ruleNumber);

            // Validate the start year in the substitution rule
            VerifyXml(p, "StartYear", ccSR.Start_Year, SubRule: ruleNumber);

            // Load the individual rules
            foreach (XmlNode c in p.ChildNodes) {
               switch (c.Name) {
                  case "Rule":
                     if (Enum.TryParse(GetXmlAttr(c, "Type"), true, out ccSR type)) {
                        VerifySubValues(type, c, delimiter);
                     } else {
                        LogIt($"Unknown substitution rule type =>{GetXmlAttr(c, "Type")}<=");
                     }
                     break;
               }
            }
         }
      }

      // Set the substitution values for a class
      private bool VerifySubValues(ccSR attribute, XmlNode c, string delimeter) {
         bool success = true;
         // Avoid user errors
         if (int.TryParse(GetXmlAttr(c, "Base"), out int b)) {
            Prop prop = EIP.AttrDict[ClassCode.Substitution_rules, (byte)attribute].Set;
            string[] s = GetXmlValue(c).Split(delimeter[0]);
            for (int i = 0; i < s.Length; i++) {
               int n = b + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  string sent = s[i];
                  string back = GetAttribute(attribute, n);
                  if (ReportAll || sent != back) {
                     string msg = $"{c.Name}\t{GetAttrData(attribute).Class}\t{attribute}\t{n}"
                             + $"\t{"N/A"}\t{GetIndexSetting(ccIDX.Item)}\t{sent}\t{back}";
                     Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddVerify, msg.Replace('_', ' ')));
                  }
               }
            }
         }
         return success;
      }

      private void VerifyRowsColumns(XmlNodeList childNodes) {
         // <TODO>
      }

      private void VerifyObjects(XmlNodeList objs) {
         XmlNode n;
         int item = 1;
         foreach (XmlNode col in objs) {
            if (col is XmlWhitespace)
               continue;
            switch (col.Name) {
               case "Column":
                  foreach (XmlNode row in col.ChildNodes) {
                     if (row is XmlWhitespace)
                        continue;
                     switch (row.Name) {
                        case "Item":
                           SetAttribute(ccIDX.Item, item);
                           n = row.SelectSingleNode("Font");
                           foreach (XmlAttribute a in n.Attributes) {
                              switch (a.Name) {
                                 case "InterCharacterSpace":
                                    VerifyXml(n, "InterCharacterSpace", ccPF.InterCharacter_Space, item);
                                    break;
                                 case "IncreasedWidth":
                                    VerifyXml(n, "IncreasedWidth", ccPF.Character_Bold, item);
                                    break;
                                 case "DotMatrix":
                                    VerifyXml(n, "DotMatrix", ccPF.Dot_Matrix, item);
                                    break;
                              }
                           }
                           VerifyXml(row.SelectSingleNode("Text"), "Text", ccPF.Print_Character_String, item);

                           ItemType type = (ItemType)Enum.Parse(typeof(ItemType), GetXmlAttr(row, "Type"), true);
                           switch (type) {
                              case ItemType.Date:
                                 VerifyCalendar(row);
                                 break;
                              case ItemType.Counter:
                                 VerifyCount(row);
                                 break;
                           }
                           item++;
                           break;
                     }
                  }
                  break;
            }

         }
      }

      private void VerifyCount(XmlNode obj) {
         XmlNode n;
         // Get data assigned by the printer
         GetAttribute(ccCount.First_Count_Block, out int FirstCountBlock);
         GetAttribute(ccCount.Number_Of_Count_Blocks, out int CountBlockCount);
         int item = GetIndexSetting(ccIDX.Item);

         foreach (XmlNode c in obj) {
            if (c is XmlWhitespace)
               continue;
            if (c.Name == "Counter" && int.TryParse(GetXmlAttr(c, "Block"), out int b) && b <= CountBlockCount) {
               int cb = FirstCountBlock + b - 1;
               SetAttribute(ccIDX.Count_Block, cb);

               if ((n = c.SelectSingleNode("Range")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "Range1":
                           VerifyXml(n, "Range1", ccCount.Count_Range_1, item, cb);
                           break;
                        case "Range2":
                           VerifyXml(n, "Range2", ccCount.Count_Range_2, item, cb);
                           break;
                        case "JumpFrom":
                           VerifyXml(n, "JumpFrom", ccCount.Jump_From, item, cb);
                           break;
                        case "JumpTo":
                           VerifyXml(n, "JumpTo", ccCount.Jump_To, item, cb);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Count")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "InitialValue":
                           VerifyXml(n, "InitialValue", ccCount.Initial_Value, item, cb);
                           break;
                        case "Increment":
                           VerifyXml(n, "Increment", ccCount.Increment_Value, item, cb);
                           break;
                        case "Direction":
                           VerifyXml(n, "Direction", ccCount.Direction_Value, item, cb);
                           break;
                        case "ZeroSuppression":
                           VerifyXml(n, "ZeroSuppression", ccCount.Zero_Suppression, item, cb);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Reset")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "Type":
                           VerifyXml(n, "Type", ccCount.Type_Of_Reset_Signal, item, cb);
                           break;
                        case "Value":
                           VerifyXml(n, "Value", ccCount.Reset_Value, item, cb);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Misc")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "UpdateIP":
                           VerifyXml(n, "UpdateIP", ccCount.Update_Unit_Halfway, item, cb);
                           break;
                        case "UpdateUnit":
                           VerifyXml(n, "UpdateUnit", ccCount.Update_Unit_Unit, item, cb);
                           break;
                        case "ExternalSignal":
                           VerifyXml(n, "ExternalSignal", ccCount.External_Count, item, cb);
                           break;
                        case "Multiplier":
                           VerifyXml(n, "Multiplier", ccCount.Count_Multiplier, item, cb);
                           break;
                        case "Skip":
                           VerifyXml(n, "Skip", ccCount.Count_Skip, item, cb);
                           break;
                     }
                  }
               }
            }
         }
      }

      private void VerifyCalendar(XmlNode obj) {
         // Get data assigned by the printer
         GetAttribute(ccCal.First_Calendar_Block, out int FirstCalBlock);
         GetAttribute(ccCal.Number_of_Calendar_Blocks, out int CalBlockCount);
         int item = GetIndexSetting(ccIDX.Item);

         foreach (XmlNode d in obj) {
            if (d is XmlWhitespace)
               continue;
            if (d.Name == "Date" && int.TryParse(GetXmlAttr(d, "Block"), out int b) && b <= CalBlockCount) {
               int cb = FirstCalBlock + b - 1;
               SetAttribute(ccIDX.Calendar_Block, cb);
               foreach (XmlNode n in d) {
                  if (n is XmlWhitespace)
                     continue;
                  switch (n.Name) {
                     case "Offset":
                        foreach (XmlAttribute a in n.Attributes) {
                           switch (a.Name) {
                              case "Year":
                                 VerifyXml(n, "Year", ccCal.Offset_Year, item, cb);
                                 break;
                              case "Month":
                                 VerifyXml(n, "Month", ccCal.Offset_Month, item, cb);
                                 break;
                              case "Day":
                                 VerifyXml(n, "Day", ccCal.Offset_Day, item, cb);
                                 break;
                              case "Hour":
                                 VerifyXml(n, "Hour", ccCal.Offset_Hour, item, cb);
                                 break;
                              case "Minute":
                                 VerifyXml(n, "Minute", ccCal.Offset_Minute, item, cb);
                                 break;
                           }
                        }
                        break;
                     case "ZeroSuppress":
                        foreach (XmlAttribute a in n.Attributes) {
                           switch (a.Name) {
                              case "Year":
                                 VerifyXml(n, "Year", ccCal.Zero_Suppress_Year, item, cb);
                                 break;
                              case "Month":
                                 VerifyXml(n, "Month", ccCal.Zero_Suppress_Month, item, cb);
                                 break;
                              case "Day":
                                 VerifyXml(n, "Day", ccCal.Zero_Suppress_Day, item, cb);
                                 break;
                              case "Hour":
                                 VerifyXml(n, "Hour", ccCal.Zero_Suppress_Hour, item, cb);
                                 break;
                              case "Minute":
                                 VerifyXml(n, "Minute", ccCal.Zero_Suppress_Minute, item, cb);
                                 break;
                              case "Week":
                                 VerifyXml(n, "Week", ccCal.Zero_Suppress_Weeks, item, cb);
                                 break;
                              case "DayOfWeek":
                                 VerifyXml(n, "DayOfWeek", ccCal.Zero_Suppress_DayOfWeek, item, cb);
                                 break;
                           }
                        }
                        break;
                     case "Substitute":
                        foreach (XmlAttribute a in n.Attributes) {
                           switch (a.Name) {
                              case "Year":
                                 VerifyXml(n, "Year", ccCal.Substitute_Year, item, cb);
                                 break;
                              case "Month":
                                 VerifyXml(n, "Month", ccCal.Substitute_Month, item, cb);
                                 break;
                              case "Day":
                                 VerifyXml(n, "Day", ccCal.Substitute_Day, item, cb);
                                 break;
                              case "Hour":
                                 VerifyXml(n, "Hour", ccCal.Substitute_Hour, item, cb);
                                 break;
                              case "Minute":
                                 VerifyXml(n, "Minute", ccCal.Substitute_Minute, item, cb);
                                 break;
                              case "Week":
                                 VerifyXml(n, "Week", ccCal.Substitute_Weeks, item, cb);
                                 break;
                              case "DayOfWeek":
                                 VerifyXml(n, "DayOfWeek", ccCal.Substitute_DayOfWeek, item, cb);
                                 break;
                           }
                        }
                        break;
                     case "TimeCount":
                        foreach (XmlAttribute a in n.Attributes) {
                           switch (a.Name) {
                              case "Start":
                                 VerifyXml(n, "Start", ccCal.Time_Count_Start_Value, item, cb);
                                 break;
                              case "End":
                                 VerifyXml(n, "End", ccCal.Time_Count_End_Value, item, cb);
                                 break;
                              case "Reset":
                                 VerifyXml(n, "ResetValue", ccCal.Time_Count_Reset_Value, item, cb);
                                 break;
                              case "ResetTime":
                                 VerifyXml(n, "ResetTime", ccCal.Reset_Time_Value, item, cb);
                                 break;
                              case "RenewalPeriod":
                                 VerifyXml(n, "Interval", ccCal.Update_Interval_Value, item, cb);
                                 break;
                           }
                        }
                        break;
                     case "Shift":
                        if (int.TryParse(GetXmlAttr(n, "ShiftNumber"), out int shift)) {
                           SetAttribute(ccIDX.Item, shift);
                           foreach (XmlAttribute a in n.Attributes) {
                              switch (a.Name) {
                                 case "StartHour":
                                    VerifyXml(n, "StartHour", ccCal.Shift_Start_Hour, item, cb);
                                    break;
                                 case "StartMinute":
                                    VerifyXml(n, "StartMinute", ccCal.Shift_Start_Minute, item, cb);
                                    break;
                                 case "EndHour": // Read Only
                                    VerifyXml(n, "EndHour", ccCal.Shift_End_Hour, item, cb);
                                    break;
                                 case "EndMinute": // Read Only
                                    VerifyXml(n, "EndMinute", ccCal.Shift_End_Minute, item, cb);
                                    break;
                                 case "ShiftCode":
                                    VerifyXml(n, "ShiftCode", ccCal.Shift_String_Value, item, cb);
                                    break;
                              }
                           }
                        }
                        break;
                  }
               }
            }
         }
      }

      #endregion

      #region Service Routines

      private void VerifyXml<T>(XmlNode n, string xmlName, T Attribute, int Item = int.MinValue, int Block = int.MinValue, int SubRule = int.MinValue) where T : Enum {
         string sent;
         string back;
         if (n.Name == xmlName) {
            sent = GetXmlValue(n);
         } else {
            sent = GetXmlAttr(n, xmlName);
         }
         back = GetAttribute(Attribute);
         if (ReportAll || sent != back) {
            string sItem = Item == int.MinValue ? "N/A" : Item.ToString();
            string sBlock = Block == int.MinValue ? "N/A" : Block.ToString();
            string sSubRule = SubRule == int.MinValue ? "N/A" : SubRule.ToString();
            string msg = $"{xmlName}\t{GetAttrData(Attribute).Class}\t{Attribute}\t{sItem}"
                       + $"\t{sBlock}\t{sSubRule}\t{sent}\t{back}";
            Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddVerify, msg.Replace('_', ' ')));
         }
      }

      // Get XML Text
      private string GetXmlValue(XmlNode node) {
         if (node != null) {
            return node.InnerText;
         } else {
            return N_A;
         }
      }

      // Get XML Attribute Value
      private string GetXmlAttr(XmlNode node, string AttrName) {
         XmlNode n;
         if (node != null && (n = node.Attributes[AttrName]) != null) {
            return n.Value;
         } else {
            return N_A;
         }
      }

      // Get the contents of one attribute
      private string GetAttribute<T>(T Attribute, int n) where T : Enum {
         string val = string.Empty;
         AttrData attr = GetAttrData(Attribute);
         if (GetAttribute(attr.Class, attr.Val, FormatOutput(attr.Get, n))) {
            val = GetDataValue;
            if (attr.Data.Fmt == DataFormats.UTF8 || attr.Data.Fmt == DataFormats.UTF8N) {
               val = FromQuoted(val);
            }
         }
         return val;
      }

      // Get the contents of one attribute
      private string GetAttribute<T>(T Attribute) where T : Enum {
         string val = string.Empty;
         AttrData attr = GetAttrData(Attribute);
         if (GetAttribute(attr.Class, attr.Val, Nodata)) {
            val = GetDataValue;
            if (attr.Data.Fmt == DataFormats.UTF8 || attr.Data.Fmt == DataFormats.UTF8N) {
               val = FromQuoted(val);
            } else if (attr.Data.DropDown != fmtDD.None) {
               string[] dd = EIP.DropDowns[(int)attr.Data.DropDown];
               long n = GetDecValue - attr.Data.Min;
               if (n >= 0 && n < dd.Length) {
                  val = dd[n];
               }
            }
         }
         return val;
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
         } else if ((mask[0] & DateCode) > 0) {
            return ItemType.Date;
         } else {
            return ItemType.Text;
         }
      }

      #endregion

   }

}
