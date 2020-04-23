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

      // Flag for Attribute Not Present
      const string N_A = "N!A";

      #endregion

      #region Verify Load was Successful

      bool ReportAll = true;

      // Verify the printer settings vs the XML File
      public bool VerifyXmlVsPrinter(string FileName, bool ReportAll = true) {
         XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = true };
         if (FileName.IndexOf("<Label", StringComparison.OrdinalIgnoreCase) >= 0) {
            xmlDoc.LoadXml(FileName);
         } else {
            xmlDoc.Load(FileName);
         }
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
         // Get the standard attributes for substitution
         string rule = GetXmlAttr(p, "RuleNumber");
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
                           if (row.SelectSingleNode("Date") != null) {
                              VerifyCalendar(row);
                           }
                           if (row.SelectSingleNode("Counter") != null) {
                              VerifyCount(row);
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
               if (int.TryParse(GetXmlAttr(d, "SubstitutionRule"), out int sr)) {
                  SetAttribute(ccIDX.Substitution_Rule, sr);
               }
               foreach (XmlNode n in d) {
                  if (n is XmlWhitespace)
                     continue;
                  switch (n.Name) {
                     case "Offset":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (Enum.TryParse($"Offset_{a.Name}", out ccCal attr)) {
                              VerifyXml(n, a.Name, attr, item, cb, sr);
                           }
                        }
                        break;
                     case "ZeroSuppress":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (Enum.TryParse($"Zero_Suppress_{a.Name}", out ccCal attr)) {
                              VerifyXml(n, a.Name, attr, item, cb, sr);
                           }
                        }
                        break;
                     case "Substitute":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (Enum.TryParse($"Substitute_{a.Name}", out ccCal attr)) {
                              VerifyXml(n, a.Name, attr, item, cb, sr);
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
            if (!sent.Equals(back, StringComparison.OrdinalIgnoreCase)) {
               Verify?.Invoke(this, msg);
            }
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
               string[] dd = GetDropDownNames((int)attr.Data.DropDown);
               long n = GetDecValue - attr.Data.Min;
               if (n >= 0 && n < dd.Length) {
                  val = dd[n];
               }
            }
         }
         return val;
      }

      // Get the contents of one attribute
      private int GetDecAttribute<T>(T Attribute) where T : Enum {
         int val = 0;
         AttrData attr = GetAttrData(Attribute);
         if (GetAttribute(attr.Class, attr.Val, Nodata)) {
            val = GetDecValue;
         }
         return val;
      }

      // Examine the contents of a print message to determine its type
      private ItemType GetItemType(string text, ref int[] mask, bool reset = true) {
         int l = 0;
         if (reset) {
            mask[l] = 0;
         }
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
            if (s[i].IndexOf('}', n + 1) > 0 && l < mask.GetUpperBound(0)) {
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

      #endregion

   }

}
