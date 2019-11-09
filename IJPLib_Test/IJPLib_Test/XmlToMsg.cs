using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using HIES.IJP.RX;

namespace IJPLib_Test {
   class XmlToMsg {

      #region Data Declarations

      XmlDocument xmlDoc;

      List<XmlNode> Items;

      #endregion

      #region Events

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(string msg);

      #endregion

      #region Constructors and destructors

      public XmlToMsg(string XmlText) {
         XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = true };
         xmlDoc.LoadXml(XmlText);
         this.xmlDoc = xmlDoc;
      }

      public XmlToMsg(XmlDocument xmlDoc) {
         this.xmlDoc = xmlDoc;
      }

      ~XmlToMsg() {

      }

      #endregion

      #region XML to Message

      // Send xlmDoc from file to printer
      public IJPMessage GetMessage() {
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
                  //SendSubstitution(c);
                  break;
            }
         }
         return success;
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
                  item.Text = GetXmlValue(Items[i].SelectSingleNode("Text"));
               }
               i++;
            }
         }
         LoadDateCount(m);
         return success;
      }

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
            if (Items[i].SelectSingleNode("TimeCount") != null) {
               m.TimeCount = LoadTimeCount(Items[i].SelectSingleNode("TimeCount"));
            }
            if (Items[i].SelectSingleNode("Shift") != null) {
               LoadShift(m.ShiftCodes, Items[i]);
            }
         }

         Items = null;

         return success;
      }

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

      // Get XML Attribute Value
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
         Log?.Invoke($"{typeof(T)}.{EnumValue} is unknown enumeration value");
         return (T)Enum.GetValues(typeof(T)).GetValue(0);
      }

      #endregion

   }
}
