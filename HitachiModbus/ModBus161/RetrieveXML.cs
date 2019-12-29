using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
//using System.Windows.Forms;
using Modbus_DLL;

namespace ModBus161 {
   public partial class SendRetrieveXML {

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

      UI161 parent;
      Modbus p;

      #endregion

      #region Constructors and destructors

      public SendRetrieveXML(UI161 parent, Modbus printer) {
         this.parent = parent;
         p = printer;
      }

      public string Retrieve() {
         string xml = string.Empty;
         if (p.GetDecAttribute(ccIJP.Online_Offline) == 0) {
            p.SetAttribute(ccIJP.Online_Offline, 1);
            if (p.GetDecAttribute(ccIJP.Online_Offline) == 0) {
               parent.Log("Cannot turn com on!  Retrieve aborted!");
               return xml;
            }
         }
         try {
            Lab Label = new Lab() { Version = "Serialization-1" };
            Label.Message = RetrieveMessage();
            Label.Printer = RetrievePrinterSettings();
            Label.Printer.Substitution = RetrieveSubstitutions(Label.Message);
            XmlSerializer serializer = new XmlSerializer(typeof(Lab));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using (MemoryStream ms = new MemoryStream()) {
               serializer.Serialize(ms, Label, ns);
               ms.Position = 0;
               xml = new StreamReader(ms).ReadToEnd();
            }
         } catch (Exception e2) {
            parent.Log(e2.Message);
         }
         return xml;
      }

      #endregion

      #region Retrieve Message

      // Retrieve the Message portion of the XML
      private Msg RetrieveMessage() {
         Msg m = new Msg();
         m.Layout = p.GetHRAttribute(ccPF.Format_Type);
         RetrieveRowsColumns(m);
         return m;
      }

      // Retrieve row/column/items
      private void RetrieveRowsColumns(Msg m) {
         int itemCount = p.GetDecAttribute(ccPF.Number_Of_Items);
         int lineCount;
         int n = 0;
         List<int> cols = new List<int>();            // Holds the number of rows in each column
         List<string> spacing = new List<string>();   // Holds the line spacing
         while (n < itemCount) {
            cols.Add(lineCount = p.GetDecAttribute(ccPF.Line_Count, n));
            spacing.Add(p.GetHRAttribute(ccPF.Line_Spacing, n));
            n += lineCount;
         }

         // Fill in the items
         m.Column = new Column[cols.Count];                               // Allocate the columns array  
         n = 0;
         int totalCharacters = 0;
         for (int col = 0; col < m.Column.Length; col++) {
            m.Column[col] = new Column();                                 // Allocate the column
            m.Column[col].InterLineSpacing = spacing[col];
            m.Column[col].Item = new Item[cols[col]];                     // Allocate the items array
            for (int row = 0; row < m.Column[col].Item.Length; row++) {
               Item item = new Item();                                    // Allocate the item
               int characterCount = p.GetDecAttribute(ccIDX.Characters_per_Item, n);
               item.Text = p.GetHRAttribute(ccPF.Print_Character_String, totalCharacters, characterCount);
               item.Font = new FontDef();                                 // Build font definition
               item.Font.DotMatrix = p.GetHRAttribute(ccPF.Dot_Matrix, n);
               item.Font.InterCharacterSpace = p.GetHRAttribute(ccPF.InterCharacter_Space, n);
               item.Font.DotMatrix = p.GetHRAttribute(ccPF.Dot_Matrix, n);
               item.Font.IncreasedWidth = p.GetHRAttribute(ccPF.Character_Bold, n);
               int bcType = p.GetDecAttribute(ccPF.Barcode_Type, n);
               if (bcType > 0) {
                  item.BarCode = new BarCode();                           // Build barcode only if needed
                  item.BarCode.DotMatrix = p.GetHRAttribute(ccPF.Barcode_Type, n);
                  item.BarCode.HumanReadableFont = p.GetHRAttribute(ccPF.Readable_Code, n);
                  item.BarCode.EANPrefix = p.GetHRAttribute(ccPF.EAN_Prefix, n);
               }

               item.Location = new Location() { Index = n, Row = row, Col = col };
               int[] mask = new int[1 + 8];
               ItemType itemType = GetItemType(item.Text, ref mask);
               item.Location.calCount = p.GetDecAttribute(ccCal.Number_of_Calendar_Blocks, n);
               if (item.Location.calCount > 0) {
                  item.Location.calStart = p.GetDecAttribute(ccCal.First_Calendar_Block, n);
                  RetrieveCalendarSettings(item, mask);
               }
               item.Location.countCount = p.GetDecAttribute(ccCount.Number_Of_Count_Blocks, n);
               if (item.Location.countCount > 0) {
                  item.Location.countStart = p.GetDecAttribute(ccCount.First_Count_Block, n);
                  RetrieveCountSettings(item);
               }
               for (int i = 0; i < mask.Length && (item.Shift == null || item.TimeCount == null); i++) {
                  if (item.Shift == null && (mask[i] & (int)ba.Shift) > 0) {
                     item.Shift = RetrieveShifts();
                  }
                  if (item.TimeCount == null && (mask[i] & (int)ba.TimeCount) > 0) {
                     item.TimeCount = RetrieveTimeCount();
                  }
               }
               m.Column[col].Item[row] = item;
               n++;
               totalCharacters += characterCount;
            }
         }
      }

      // Retrieve Calendar settings
      private void RetrieveCalendarSettings(Item item, int[] mask) {
         int n = item.Location.calStart - 1;
         item.Date = new Date[item.Location.calCount];
         for (int i = 0; i < item.Location.calCount; i++) {
            // Where do you get Substitution rule number
            item.Date[i] = new Date() { Block = i + 1 };
            if ((mask[i] & DateOffset) > 0) {
               item.Date[i].SubstitutionRule = "1";
               item.Date[i].RuleName = "";
            }
            if ((mask[i] & DateOffset) > 0) {
               item.Date[i].Offset = new Offset() {
                  Year = p.GetHRAttribute(ccCal.Offset_Year, n),
                  Month = p.GetHRAttribute(ccCal.Offset_Month, n),
                  Day = p.GetHRAttribute(ccCal.Offset_Day, n),
                  Hour = p.GetHRAttribute(ccCal.Offset_Hour, n),
                  Minute = p.GetHRAttribute(ccCal.Offset_Minute, n)
               };
            }
            if ((mask[i] & DateSubZS) > 0) {
               item.Date[i].ZeroSuppress = new ZeroSuppress();
               string s;
               if ((mask[i] & (int)ba.Year) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Year, n)))
                     item.Date[i].ZeroSuppress.Year = s;
               if ((mask[i] & (int)ba.Month) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Month, n)))
                     item.Date[i].ZeroSuppress.Month = s;
               if ((mask[i] & (int)ba.Day) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Day, n)))
                     item.Date[i].ZeroSuppress.Day = s;
               if ((mask[i] & (int)ba.Hour) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Hour, n)))
                     item.Date[i].ZeroSuppress.Hour = s;
               if ((mask[i] & (int)ba.Minute) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Minute, n)))
                     item.Date[i].ZeroSuppress.Minute = s;
               if ((mask[i] & (int)ba.Week) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Weeks, n)))
                     item.Date[i].ZeroSuppress.Week = s;
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_DayOfWeek, n)))
                     item.Date[i].ZeroSuppress.DayOfWeek = s;

               item.Date[i].Substitute = new Substitute();
               if ((mask[i] & (int)ba.Year) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Year, n)))
                     item.Date[i].Substitute.Year = s;
               if ((mask[i] & (int)ba.Month) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Month, n)))
                     item.Date[i].Substitute.Month = s;
               if ((mask[i] & (int)ba.Day) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Day, n)))
                     item.Date[i].Substitute.Day = s;
               if ((mask[i] & (int)ba.Hour) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Hour, n)))
                     item.Date[i].Substitute.Hour = s;
               if ((mask[i] & (int)ba.Minute) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Minute, n)))
                     item.Date[i].Substitute.Minute = s;
               if ((mask[i] & (int)ba.Week) > 0) // Printer reports these wrong
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_DayOfWeek, n)))
                     item.Date[i].Substitute.Week = s;
               if ((mask[i] & (int)ba.DayOfWeek) > 0) // Printer reports these wrong
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Weeks, n)))
                     item.Date[i].Substitute.DayOfWeek = s;
            }
            n++;
         }
      }

      // Retrieve Count Settings
      private void RetrieveCountSettings(Item item) {
         int n = item.Location.countCount - 1;
         item.Counter = new Counter[item.Location.countCount];
         for (int i = 0; i < item.Location.countCount; i++) {
            item.Counter[i] = new Counter() { Block = i + 1 };
            item.Counter[i].Range = new Range() {
               Range1 = p.GetHRAttribute(ccCount.Count_Range_1, n),
               Range2 = p.GetHRAttribute(ccCount.Count_Range_2, n),
               JumpFrom = p.GetHRAttribute(ccCount.Jump_From, n),
               JumpTo = p.GetHRAttribute(ccCount.Jump_To, n),
            };
            item.Counter[i].Count = new Count() {
               InitialValue = p.GetHRAttribute(ccCount.Initial_Value, n),
               Increment = p.GetHRAttribute(ccCount.Increment_Value, n),
               Direction = p.GetHRAttribute(ccCount.Direction_Value, n),
               ZeroSuppression = p.GetHRAttribute(ccCount.Zero_Suppression, n),
            };
            item.Counter[i].Reset = new Reset() {
               Type = p.GetHRAttribute(ccCount.Type_Of_Reset_Signal, n),
               Value = p.GetHRAttribute(ccCount.Reset_Value, n),
            };
            item.Counter[i].Misc = new Misc() {
               UpdateIP = p.GetHRAttribute(ccCount.Update_Unit_Halfway, n),
               UpdateUnit = p.GetHRAttribute(ccCount.Update_Unit_Unit, n),
               ExternalCount = p.GetHRAttribute(ccCount.External_Count, n),
               Multiplier = p.GetHRAttribute(ccCount.Count_Multiplier, n),
               SkipCount = p.GetHRAttribute(ccCount.Count_Skip, n),
            };
            n++;
         }
      }

      // Retrieve shift settings
      private Shift[] RetrieveShifts() {
         List<Shift> s = new List<Shift>();
         string endHour;
         string endMinute;
         int n = 0;
         do {
            s.Add(new Shift() {
               ShiftNumber = n + 1,
               StartHour = p.GetHRAttribute(ccCal.Shift_Start_Hour, n),
               StartMinute = p.GetHRAttribute(ccCal.Shift_Start_Minute, n),
               EndHour = endHour = p.GetHRAttribute(ccCal.Shift_End_Hour, n),
               EndMinute = endMinute = p.GetHRAttribute(ccCal.Shift_End_Minute, n),
               ShiftCode = p.GetHRAttribute(ccCal.Shift_String_Value, n),
            });
            n++;
         } while (endHour != "23" || endMinute != "59");
         return s.ToArray();
      }

      // Retrieve Time count settings
      private TimeCount RetrieveTimeCount() {
         TimeCount TimeCount = new TimeCount() {
            Interval = p.GetHRAttribute(ccCal.Update_Interval_Value),
            Start = p.GetHRAttribute(ccCal.Time_Count_Start_Value),
            End = p.GetHRAttribute(ccCal.Time_Count_End_Value),
            ResetTime = p.GetHRAttribute(ccCal.Reset_Time_Value),
            ResetValue = p.GetHRAttribute(ccCal.Time_Count_Reset_Value),
         };
         return TimeCount;
      }

      #endregion

      #region Retrieve printer settings

      // Retrieve printer settings
      private Printer RetrievePrinterSettings() {
         Printer ptr = new Printer() {
            Make = "Hitachi",
            Model = p.GetHRAttribute(ccUI.Model_Name),
            PrintHead = new PrintHead() {
               Orientation = p.GetHRAttribute(ccPS.Character_Orientation)
            },
            ContinuousPrinting = new ContinuousPrinting() {
               RepeatInterval = p.GetHRAttribute(ccPS.Repeat_Interval),
               PrintsPerTrigger = p.GetHRAttribute(ccPS.Repeat_Count)
            },
            TargetSensor = new TargetSensor() {
               Filter = p.GetHRAttribute(ccPS.Target_Sensor_Filter),
               SetupValue = p.GetHRAttribute(ccPS.Target_Sensor_Filter_Value),
               Timer = p.GetHRAttribute(ccPS.Target_Sensor_Timer)
            },
            CharacterSize = new CharacterSize() {
               Width = p.GetHRAttribute(ccPS.Character_Width),
               Height = p.GetHRAttribute(ccPS.Character_Height)
            },
            PrintStartDelay = new PrintStartDelay() {
               Forward = p.GetHRAttribute(ccPS.Print_Start_Delay_Forward),
               Reverse = p.GetHRAttribute(ccPS.Print_Start_Delay_Reverse)
            },
            EncoderSettings = new EncoderSettings() {
               HighSpeedPrinting = p.GetHRAttribute(ccPS.High_Speed_Print),
               Divisor = p.GetHRAttribute(ccPS.Pulse_Rate_Division_Factor),
               ExternalEncoder = p.GetHRAttribute(ccPS.Product_Speed_Matching)
            },
            InkStream = new InkStream() {
               InkDropUse = p.GetHRAttribute(ccPS.Ink_Drop_Use),
               ChargeRule = p.GetHRAttribute(ccPS.Ink_Drop_Charge_Rule)
            },
            Logos = RetrieveLogos(),
         };


         return ptr;
      }

      private Logos RetrieveLogos() {
         return null;
      }

      // Retrieve Substitution rules
      private Substitution RetrieveSubstitutions(Msg m) {
         bool needYear = false;
         bool needMonth = false;
         bool needDay = false;
         bool needHour = false;
         bool needMinute = false;
         bool needWeek = false;
         bool needDayOfWeek = false;
         string ruleNumber = "01";
         for (int c = 0; c < m.Column.Length; c++) {
            Column col = m.Column[c];
            for (int r = 0; r < col.Item.Length; r++) {
               Item item = col.Item[r];
               if (item.Date != null) {
                  for (int i = 0; i < item.Date.Length; i++) {
                     ruleNumber = item.Date[i].SubstitutionRule;
                     Substitute sub = item.Date[i].Substitute;
                     if (sub != null) {
                        needYear |= sub.Year != null;
                        needMonth |= sub.Month != null;
                        needDay |= sub.Day != null;
                        needHour |= sub.Hour != null;
                        needMinute |= sub.Minute != null;
                        needDayOfWeek |= sub.DayOfWeek != null;
                        needWeek |= sub.Week != null;
                     }
                  }
               }
            }
         }
         // Need to load the rule (just use 1 for now)
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccIDX.Substitution_Rule, 1);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         List<SubstitutionRule> sr = new List<SubstitutionRule>();
         if (needYear)
            RetrieveSubstitution(sr, ccSR.Year);
         if (needMonth)
            RetrieveSubstitution(sr, ccSR.Month);
         if (needDay)
            RetrieveSubstitution(sr, ccSR.Day);
         if (needHour)
            RetrieveSubstitution(sr, ccSR.Hour);
         if (needMinute)
            RetrieveSubstitution(sr, ccSR.Minute);
         if (needWeek)
            RetrieveSubstitution(sr, ccSR.Week);
         if (needDayOfWeek)
            RetrieveSubstitution(sr, ccSR.DayOfWeek);
         Substitution substitution = new Substitution() {
            Delimiter = "/",
            StartYear = p.GetHRAttribute(ccSR.Start_Year),
            RuleNumber = "1",
            SubRule = sr.ToArray()
         };
         return substitution;
      }

      // Retrieve one substitution type
      private void RetrieveSubstitution(List<SubstitutionRule> sr, ccSR rule) {
         AttrData attr = p.GetAttrData(rule);
         int n = (int)(attr.Data.Max - attr.Data.Min + 1);
         string[] subCode = new string[n];
         for (int i = 0; i < n; i++) {
            subCode[i] = p.GetHRAttribute(rule, i);
         }
         for (int i = 0; i < n; i += 10) {
            sr.Add(new SubstitutionRule() {
               Type = rule.ToString().Replace("_", ""),
               Base = (i + attr.Data.Min).ToString(),
               Text = string.Join("/", subCode, i, Math.Min(10, n - i)),
            });
         }
      }

      #endregion

      #region Service Routines

      // Check if string value is first entry in dropdown
      private bool IsDefaultValue(fmtDD fmt, string s) {
         if (string.IsNullOrEmpty(s)) {
            return true;
         }
         if (int.TryParse(s, out int val)) {
            return val == 0;
         }
         if (bool.TryParse(s, out bool b)) {
            return !b;
         }
         s = s.ToLower();
         val = Array.FindIndex(Data.DropDowns[(int)fmt], x => x.ToLower().Contains(s));
         if (val < 0) {
            val = Array.FindIndex(Data.DropDownsIJPLib[(int)fmt], x => x.ToLower().Contains(s));
         }
         return val == 0;
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
