using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using HIES.IJP.RX;

namespace IJPLib_Test {
   class XmlToMsg {

      #region Data Declarations

      IJP ijp;
      XmlDocument xmlDoc;

      List<XmlNode> Items;

      #endregion

      #region Events

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(string msg);

      #endregion

      #region Constructors and destructors

      // Convert XML Text to a IJPMessage
      public XmlToMsg(string XmlText, IJP ijp = null) {
         XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = true };
         xmlDoc.LoadXml(XmlText);
         this.xmlDoc = xmlDoc;
         this.ijp = ijp;
      }

      // Convert XML Document to a IJPMessage
      public XmlToMsg(XmlDocument xmlDoc, IJP ijp = null) {
         this.xmlDoc = xmlDoc;
         this.ijp = ijp;
      }

      // Clean up
      ~XmlToMsg() {
         xmlDoc = null;
         Items = null;
      }

      #endregion

      #region XML to Message

      // Convert XML Document to a IJPMessage
      public IJPMessage BuildMessage() {
         IJPMessage m = null;
         bool success = true;
         // Need a XMP Document to continue
         if (xmlDoc == null) {
            return m;
         }
         try {
            m = new IJPMessage();

            XmlNode prnt = xmlDoc.SelectSingleNode("Label/Printer");
            if (success && prnt != null) {
               success = LoadPrinterSettings(m, prnt);            // Send printer wide settings
            }

            XmlNode objs = xmlDoc.SelectSingleNode("Label/Message");
            if (objs != null) {
               success = AllocateRowsColumns(m, objs.ChildNodes); // Allocate rows and columns
            }

            // Send it to the printer (Maybe)
            if (ijp != null) {
               ijp.SetMessage(m);
            }

         } catch (Exception e1) {
            success = false;
         } finally {

         }

         return m;
      }

      // Send the Printer Wide Settings
      private bool LoadPrinterSettings(IJPMessage m, XmlNode pr) {
         bool success = true;
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  m.CharacterOrientation = ParseEnum<IJPCharacterOrientation>(GetXmlAttr(c, "Orientation"));
                  break;
               case "ContinuousPrinting":
                  m.RepeatIntervals = (uint)GetXmlAttrN(c, "RepeatInterval");
                  m.RepeatCount = (ushort)GetXmlAttrN(c, "PrintsPerTrigger");
                  break;
               case "TargetSensor":
                  m.TargetSensorFilter = ParseEnum<IJPSensorFilter>(GetXmlAttr(c, "Filter"));
                  m.TimeSetup = (ushort)GetXmlAttrN(c, "SetupValue");
                  m.TargetSensorTimer = (ushort)GetXmlAttrN(c, "Timer");
                  break;
               case "CharacterSize":
                  m.CharacterWidth = (ushort)GetXmlAttrN(c, "Width");
                  m.CharacterHeight = (byte)GetXmlAttrN(c, "Height");
                  break;
               case "PrintStartDelay":
                  m.PrintStartDelayForward = (ushort)GetXmlAttrN(c, "Forward");
                  m.PrintStartDelayReverse = (ushort)GetXmlAttrN(c, "Reverse");
                  break;
               case "EncoderSettings":
                  m.HiSpeedPrint = ParseEnum<IJPHiSpeedPrintType>(GetXmlAttr(c, "HighSpeedPrinting"));
                  m.PulseRateDivisionFactor = (ushort)GetXmlAttrN(c, "Divisor");
                  m.ProductSpeedMatching = ParseEnum<IJPProductSpeedMatching>(GetXmlAttr(c, "ExternalEncoder"));
                  break;
               case "InkStream":
                  m.InkDropUse = (byte)GetXmlAttrN(c, "InkDropUse");
                  m.InkDropChargeRule = ParseEnum<IJPInkDropChargeRule>(GetXmlAttr(c, "ChargeRule"));
                  break;
               case "Substitution":
                  if (ijp != null) {
                     LoadBuildSubstitution(c);
                  }
                  break;
               case "TimeCount":
                  //BuildTimeCount(c);
                  break;
               case "Shifts":
                  //BuildShifts(c);
                  break;
               case "Logos":
                  if (ijp != null) {
                     LoadLogos(c);
                  }
                  break;
            }
         }
         return success;
      }

      // Handle Fixed logos (Free Style and cijConnect formats later)
      private void LoadLogos(XmlNode c) {
         Bitmap bm = null;
         foreach (XmlNode l in c.ChildNodes) {
            if (l is XmlWhitespace || l.Name != "Logo")
               continue;
            string layout = GetXmlAttr(l, "Layout");
            IJPDotMatrix dotMatrix = ParseEnum<IJPDotMatrix>(GetXmlAttr(l, "DotMatrix"));
            int location = (int)GetXmlAttrN(l, "Location");
            string rawData = GetXmlAttr(l, "RawData");
            if (rawData == string.Empty) {
               string folder = GetXmlAttr(c, "Folder");
               string fileName = GetXmlAttr(l, "FileName");
               string fullFileName = Path.Combine(folder, fileName + ".bmp");
               if (File.Exists(fullFileName)) {
                  bm = new Bitmap(fullFileName);
               } else {
                  continue;
               }
            } else {
               if (!int.TryParse(GetXmlAttr(l, "Width"), out int width)
                 || !int.TryParse(GetXmlAttr(l, "Height"), out int height)) {
                  GetBitmapSize(dotMatrix, out width, out height);
               }
               ulong[] stripes = rawDataToStripes(height, rawData);
               bm = BuildBitMap(stripes, width, height);
            }
            if (layout == "Fixed") {
               IJPFixedUserPattern upFixed = new IJPFixedUserPattern(location + 1, dotMatrix, bm);
               ijp.SetFixedUserPattern(upFixed);
            } else {
               IJPFreeUserPattern upFree = new IJPFreeUserPattern(location + 1, bm);
               ijp.SetFreeUserPattern(upFree);
            }
         }
      }

      // Load substitutions
      private void LoadBuildSubstitution(XmlNode p) {
         // Get the standard attributes for substitution
         string rule = GetXmlAttr(p, "RuleNumber");
         string startYear = GetXmlAttr(p, "StartYear");
         string delimiter = GetXmlAttr(p, "Delimiter");

         // Avoid user errors
         if (int.TryParse(rule, out int ruleNumber) && ushort.TryParse(startYear, out ushort year) && delimiter.Length == 1) {
            // Sub Substitution rule in Index class
            IJPSubstitutionRule sr = (IJPSubstitutionRule)ijp.GetSubstitutionRule(ruleNumber);
            // Set the start year in the substitution rule
            sr.StartYear = year;

            // Load the individual rules
            foreach (XmlNode c in p.ChildNodes) {
               switch (c.Name) {
                  case "Rule":
                     if (Enum.TryParse(GetXmlAttr(c, "Type"), true, out MsgToXml.ba type)) {
                        SetSubValues(sr, type, c, delimiter);
                     } else {
                        Log?.Invoke($"Unknown substitution rule type =>{GetXmlAttr(c, "Type")}<=");
                     }
                     break;
               }
            }
            ijp.SetSubstitutionRule(sr);
         }

      }

      // Set valuse within a substiturion rule
      private void SetSubValues(IJPSubstitutionRule sr, MsgToXml.ba type, XmlNode c, string delimiter) {
         bool success = true;
         // Avoid user errors
         if (int.TryParse(GetXmlAttr(c, "Base"), out int b)) {
            string[] s = GetXmlValue(c).Split(delimiter[0]);
            for (int i = 0; i < s.Length && success; i++) {
               int n = b + i;
               // Avoid user errors
               switch (type) {
                  case MsgToXml.ba.Year:
                     sr.SetYearSetup(n, s[i]);
                     break;
                  case MsgToXml.ba.Month:
                     sr.SetMonthSetup(n, s[i]);
                     break;
                  case MsgToXml.ba.Day:
                     sr.SetDaySetup(n, s[i]);
                     break;
                  case MsgToXml.ba.Hour:
                     sr.SetHourSetup(n, s[i]);
                     break;
                  case MsgToXml.ba.Minute:
                     sr.SetMinuteSetup(n, s[i]);
                     break;
                  case MsgToXml.ba.WeekNumber:
                     sr.SetWeekNumberSetup(n, s[i]);
                     break;
                  case MsgToXml.ba.DayOfWeek:
                     sr.SetWeekSetup((DayOfWeek)(n - 1), s[i]); // 
                     break;
               }
            }
         }
      }

      // Build the structure and load Items
      private bool AllocateRowsColumns(IJPMessage m, XmlNodeList objs) {
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
            m.AddColumn();
            m.SetRow(col, (byte)columns[col]);

            for (int row = 0; row < columns[col]; row++) {
               IJPMessageItem item = (IJPMessageItem)m.Items[i];
               if ((n = Items[i].SelectSingleNode("Font")) != null) {
                  item.DotMatrix = ParseEnum<IJPDotMatrix>(GetXmlAttr(n, "DotMatrix"));
                  item.InterCharacterSpace = (byte)GetXmlAttrN(n, "InterCharacterSpace");
                  item.LineSpacing = (byte)ILS[col];
                  item.Bold = (byte)GetXmlAttrN(n, "IncreasedWidth");
                  item.Text = FormatText(GetXmlValue(Items[i].SelectSingleNode("Text")));
               }
               i++;
            }
         }
         LoadDateCount(m);
         return success;
      }

      // Load the Calendar and date objects from the XML
      private bool LoadDateCount(IJPMessage m) {
         bool success = true;
         int calBlockNumber = 0;
         int cntBlockNumber = 0;

         int[] calStart = new int[Items.Count];
         int[] calCount = new int[Items.Count];
         int[] countStart = new int[Items.Count];
         int[] countCount = new int[Items.Count];

         for (int i = 0; i < Items.Count; i++) {
            IJPMessageItem item = (IJPMessageItem)m.Items[i];
            if (Items[i].SelectSingleNode("Date") != null) {
               calCount[i] = item.CalendarBlockCount;
               if (calCount[i] > 0) {
                  calStart[i] = calBlockNumber;
                  calBlockNumber += calCount[i];
               }
            }
            if (Items[i].SelectSingleNode("Counter") != null) {
               countCount[i] = item.CountBlockCount;
               if (countCount[i] > 0) {
                  countStart[i] = cntBlockNumber;
                  cntBlockNumber += countCount[i];
               }
            }
         }

         for (int i = 0; i < Items.Count; i++) {
            if (Items[i].SelectSingleNode("Date") != null) {
               LoadCalendar(m.CalendarConditions, Items[i], calCount[i], calStart[i]);
            }
            if (Items[i].SelectSingleNode("Counter") != null) {
               LoadCount(m.CountConditions, Items[i], countCount[i], countStart[i]);
            }

            // Time Count is not by item but are by message.  
            if (Items[i].SelectSingleNode("TimeCount") != null) {
               m.TimeCount = LoadTimeCount(Items[i].SelectSingleNode("TimeCount"));
            }

            // Shift codes are not by item but are by message.  
            if (Items[i].SelectSingleNode("Shift") != null) {
               LoadShift(m.ShiftCodes, Items[i]);
            }
         }

         Items = null;

         return success;
      }

      // Load one or more shift codes into the Shift Code Collection
      private void LoadShift(IJPShiftCodeCollection scc, XmlNode obj) {

         foreach (XmlNode d in obj) {
            if (d is XmlWhitespace)
               continue;
            if (d.Name == "Shift") {
               IJPShiftCode sc = new IJPShiftCode();
               if (int.TryParse(GetXmlAttr(d, "ShiftNumber"), out int shift)) {
                  foreach (XmlAttribute a in d.Attributes) {
                     switch (a.Name) {
                        case "StartHour":
                           sc.StartTime.Hour = (byte)Convert.ToInt16(a.Value);
                           break;
                        case "StartMinute":
                           sc.StartTime.Minute = (byte)Convert.ToInt16(a.Value);
                           break;
                        case "ShiftCode":
                           sc.String = a.Value;
                           break;
                     }
                  }
               }
               scc.Add(sc);
            }
         }
      }

      // Build the Time Count object and return it
      private IJPTimeCountCondition LoadTimeCount(XmlNode d) {
         IJPTimeCountCondition tc = new IJPTimeCountCondition();
         foreach (XmlAttribute a in d.Attributes) {
            switch (a.Name) {
               case "Start":
                  tc.LowerRange = a.Value;
                  break;
               case "End":
                  tc.UpperRange = a.Value;
                  break;
               case "ResetValue":
                  tc.Reset = a.Value;
                  break;
               case "ResetTime":
                  tc.ResetTime = (byte)Convert.ToInt16(a.Value);
                  break;
               case "Interval":
                  tc.RenewalPeriod = ParseEnum<IJPTimeCountConditionRenewalPeriod>(a.Value);
                  break;
            }
         }
         return tc;
      }

      // Send Calendar related information
      private bool LoadCalendar(IJPCalendarConditionCollection ccc, XmlNode obj, int CalBlockCount, int FirstCalBlock) {
         bool success = true;

         foreach (XmlNode d in obj) {
            if (d is XmlWhitespace)
               continue;
            if (d.Name == "Date" && int.TryParse(GetXmlAttr(d, "Block"), out int b) && b <= CalBlockCount) {
               IJPCalendarCondition cc = ccc[FirstCalBlock + b - 1];

               if (int.TryParse(GetXmlAttr(d, "SubstitutionRule"), out int sr)) {
                  cc.SubstitutionRuleNumber = (byte)sr;
               }
               foreach (XmlNode n in d.ChildNodes) {
                  if (n is XmlWhitespace)
                     continue;
                  switch (n.Name) {
                     case "Offset":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (int.TryParse(a.Value, out int x)) {
                              switch (a.Name) {
                                 case "Year":
                                    cc.YearOffset = (byte)x;
                                    break;
                                 case "Month":
                                    cc.MonthOffset = (byte)x;
                                    break;
                                 case "Day":
                                    cc.DayOffset = (ushort)x;
                                    break;
                                 case "Hour":
                                    cc.HourOffset = (short)x;
                                    break;
                                 case "Minute":
                                    cc.MinuteOffset = (short)x;
                                    break;
                              }
                           }
                        }
                        break;
                     case "ZeroSuppress":
                        foreach (XmlAttribute a in n.Attributes) {
                           IJPCalendarConditionZeroSuppress zs = ParseEnum<IJPCalendarConditionZeroSuppress>(a.Value);
                           switch (a.Name) {
                              case "Year":
                                 cc.YearZeroSuppression = zs;
                                 break;
                              case "Month":
                                 cc.MonthZeroSuppression = zs;
                                 break;
                              case "Day":
                                 cc.DayZeroSuppression = zs;
                                 break;
                              case "Hour":
                                 cc.HourZeroSuppression = zs;
                                 break;
                              case "Minute":
                                 cc.MinuteZeroSuppression = zs;
                                 break;
                              case "Week":
                                 cc.WeekZeroSuppression = zs;
                                 break;
                              case "DayOfWeek":
                                 cc.WeekNumberZeroSuppression = zs;
                                 break;
                           }
                        }
                        break;
                     case "Substitute":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (bool.TryParse(a.Value, out bool sub)) {
                              switch (a.Name) {
                                 case "Year":
                                    cc.YearSubstitutionRule = sub;
                                    break;
                                 case "Month":
                                    cc.MonthSubstitutionRule = sub;
                                    break;
                                 case "Day":
                                    cc.DaySubstitutionRule = sub;
                                    break;
                                 case "Hour":
                                    cc.HourSubstitutionRule = sub;
                                    break;
                                 case "Minute":
                                    cc.MinuteSubstitutionRule = sub;
                                    break;
                                 case "Week":
                                    cc.WeekNumberSubstitutionRule = sub;
                                    break;
                                 case "DayOfWeek":
                                    cc.WeekSubstitutionRule = sub;
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
      private bool LoadCount(IJPCountConditionCollection ccc, XmlNode obj, int CountBlockCount, int FirstCountBlock) {
         bool success = true;
         XmlNode n;
         foreach (XmlNode c in obj) {
            if (c is XmlWhitespace)
               continue;
            if (c.Name == "Counter" && int.TryParse(GetXmlAttr(c, "Block"), out int b) && b <= CountBlockCount) {
               IJPCountCondition cc = ccc[FirstCountBlock + b - 1];
               if ((n = c.SelectSingleNode("Range")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "Range1":
                           cc.LowerRange = a.Value;
                           break;
                        case "Range2":
                           cc.UpperRange = a.Value;
                           break;
                        case "JumpFrom":
                           cc.JumpFrom = a.Value;
                           break;
                        case "JumpTo":
                           cc.JumpTo = a.Value;
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Count")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "InitialValue":
                           cc.Value = a.Value;
                           break;
                        case "Increment":
                           cc.Increment = (byte)Convert.ToInt32(a.Value);
                           break;
                        case "Direction":
                           cc.Direction = ParseEnum<IJPCountConditionDirection>(a.Value);
                           break;
                        case "ZeroSuppression":
                           cc.SuppressesZero = Convert.ToBoolean(a.Value);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Reset")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "Value":
                           cc.Reset = a.Value;
                           break;
                        case "Type":
                           cc.ResetSignal = ParseEnum<IJPCountConditionResetSignal>(a.Value);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Misc")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "UpdateIP":
                           cc.UpdateInProgress = Convert.ToUInt32(a.Value);
                           break;
                        case "UpdateUnit":
                           cc.UpdateUnit = Convert.ToUInt32(a.Value);
                           break;
                        case "ExternalCount":
                           cc.UsesExternalSignalCount = Convert.ToBoolean(a.Value);
                           break;
                        case "Multiplier":
                           cc.Multiplier = a.Value;
                           break;
                        case "Skip":
                           cc.CountSkip = a.Value;
                           break;
                     }
                  }
               }
            }
         }
         return success;
      }

      #endregion

      #region Service Routines

      // Get XML Text
      private string GetXmlValue(XmlNode node) {
         if (node != null) {
            return node.InnerText;
         } else {
            return "N_A";
         }
      }

      // Get XML Attribute Value
      private string GetXmlAttr(XmlNode node, string AttrName) {
         XmlNode n;
         if (node != null && (n = node.Attributes[AttrName]) != null) {
            return n.Value;
         } else {
            return "N_A";
         }
      }

      // Get XML Attribute Value as long
      private long GetXmlAttrN(XmlNode node, string AttrName) {
         XmlNode n;
         if (node != null && (n = node.Attributes[AttrName]) != null) {
            if (long.TryParse(n.Value, out long v)) {
               return v;
            }
         }
         return 0;
      }

      // Convert string back to Enum
      private T ParseEnum<T>(string EnumValue) {
         if (Enum.IsDefined(typeof(T), EnumValue)) {
            return (T)Enum.Parse(typeof(T), EnumValue, true);
         }

         // May want to check for wrong case
         Log?.Invoke($"{typeof(T)}.{EnumValue} is unknown enumeration value");
         return (T)Enum.GetValues(typeof(T)).GetValue(0);
      }

      // Resolve brace usage differences between IJPLib and EtherNet/IP
      private string FormatText(string s) {
         string result = s;
         int start = -1;
         int end = 0;
         int n = 0;
         result = result.Replace("{{", "}").Replace("}}", "}");
         while ((start = result.IndexOf("{X", start + 1)) >= 0) {
            if ((end = result.IndexOf("}", start)) > 0) {
               string[] t = result.Substring(start + 1, end - start - 1).Split('/');
               if (t.Length == 2 && int.TryParse(t[1], out n) && n >= 0 && n < 200) {
                  result = result.Substring(0, start) + (char)(n + IJPTest.FirstFixedUP) + result.Substring(end + 1);
               }
            }
         }
         while ((start = result.IndexOf("{Z", start + 1)) >= 0) {
            if ((end = result.IndexOf("}", start)) > 0) {
               string[] t = result.Substring(start + 1, end - start - 1).Split('/');
               if (t.Length == 2 && int.TryParse(t[1], out n) && n >= 0 && n < 50) {
                  result = result.Substring(0, start) + (char)(n + IJPTest.FirstFreeUP) + result.Substring(end + 1);
               }
            }
         }
         int x = result[0];
         return result;
      }

      // Get the height and width of a user pattern
      private void GetBitmapSize(IJPDotMatrix dm, out int width, out int height) {
         switch (dm) {
            case IJPDotMatrix.Size4x5:
               width = 8;
               height = 5;
               break;
            case IJPDotMatrix.Size5x5:
               width = 8;
               height = 5;
               break;
            case IJPDotMatrix.Size5x7:
               width = 8;
               height = 7;
               break;
            case IJPDotMatrix.Size9x7:
               width = 16;
               height = 7;
               break;
            case IJPDotMatrix.Size7x10:
               width = -8;
               height = 10;
               break;
            case IJPDotMatrix.Size10x12:
               width = 16;
               height = 12;
               break;
            case IJPDotMatrix.Size12x16:
               width = 16;
               height = 16;
               break;
            case IJPDotMatrix.Size18x24:
               width = 24;
               height = 24;
               break;
            case IJPDotMatrix.Size24x32:
               width = 32;
               height = 32;
               break;
            case IJPDotMatrix.Size11x11:
               width = 16;
               height = 11;
               break;
            case IJPDotMatrix.Size5x3_Chimney:
               width = 5;
               height = 3;
               break;
            case IJPDotMatrix.Size5x5_Chimney:
               width = 5;
               height = 5;
               break;
            case IJPDotMatrix.Size7x5_Chimney:
               width = 7;
               height = 5;
               break;
            default:
               width = -1;
               height = -1;
               break;
         }

      }

      // Convert raw data to stripes
      private ulong[] rawDataToStripes(int height, string rawData) {
         string[] s = rawData.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
         int stride = (height + 7) / 8;
         int nStripes = (s.Length + stride - 1) / stride;
         ulong[] result = new ulong[nStripes];
         int n = 0;
         for (int i = 0; i < nStripes; i++) {
            for (int j = 0; j < stride && n < s.Length; j++) {
               if (ulong.TryParse(s[n++], NumberStyles.HexNumber, null, out ulong x)) {
                  result[i] += x << (8 * j);
               }
            }
         }
         return result;
      }

      // Convert the stripes to a bitmap
      private Bitmap BuildBitMap(ulong[] stripes, int width, int height) {
         Bitmap result = new Bitmap(width, height);
         using (Graphics g = Graphics.FromImage(result)) {
            g.Clear(Color.White);
            for (int x = 0; x < Math.Min(width, stripes.Length); x++) {
               for (int y = height - 1; stripes[x] > 0 && y >= 0; y--) {
                  if ((stripes[x] & 1) != 0) {
                     result.SetPixel(x, y, Color.Black);
                  }
                  stripes[x] >>= 1;
               }
            }
            return result;
         }
      }

      #endregion

   }

}
