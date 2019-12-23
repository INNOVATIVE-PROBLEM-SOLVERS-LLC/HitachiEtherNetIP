using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace ModBus161 {
   public class RetrieveXML {

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

      UI161 p;

      Prop prop;
      AttrData attr;

      #endregion

      #region Constructors and destructors

      public RetrieveXML(UI161 parent) {
         p = parent;
         prop = new Prop(2, DataFormats.Decimal, long.MinValue, long.MaxValue, fmtDD.None);
         attr = new AttrData(0, GSS.Get, false, 0, prop);
      }

      public string Retrieve() {
         string xml = string.Empty;
         if (p.GetDecAttribute(ccIJP.Online_Offline) == 0) {
            p.SetAttribute(ccIJP.Online_Offline, 1);
            if (p.GetDecAttribute(ccIJP.Online_Offline) == 0) {
               p.Log("Cannot turn com on!  Retrieve aborted!");
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
            p.Log(e2.Message);
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
         attr.Val = 0x08;
         int itemCount = p.GetDecAttribute(attr);
         int lineCount;
         int n = 0;
         int stride = 0x1058 - 0x1040;                // Distance between print items
         List<int> cols = new List<int>();            // Holds the number of rows in each column
         List<string> spacing = new List<string>();   // Holds the line spacing
         while (n < itemCount) {
            cols.Add(lineCount = p.GetDecAttribute(ccPF.Line_Count, n * stride));
            spacing.Add(p.GetHRAttribute(ccPF.Line_Count, n * stride));
            n += lineCount;
         }

         // Fill in the items
         m.Column = new Column[cols.Count];           // Allocate the columns    
         int offset = 0;
         n = 0;
         int totalCharacters = 0;
         for (int col = 0; col < m.Column.Length; col++) {
            m.Column[col] = new Column();
            m.Column[col].InterLineSpacing = spacing[col];
            m.Column[col].Item = new Item[cols[col]];
            for (int row = 0; row < m.Column[col].Item.Length; row++) {
               Item item = new Item();
               attr.Val = 0x0020 + n;
               int characterCount = p.GetDecAttribute(attr);
               attr.Val = 0x0084 + 2 * totalCharacters;
               p.GetAttribute(attr.Val, characterCount * 4, out byte[] text);
               item.Text = formatText(text);
               item.Font = new FontDef();
               item.Font.DotMatrix = p.GetHRAttribute(ccPF.Dot_Matrix, offset);
               item.Font.InterCharacterSpace = p.GetHRAttribute(ccPF.InterCharacter_Space, offset);
               item.Font.DotMatrix = p.GetHRAttribute(ccPF.Dot_Matrix, offset);
               item.Font.IncreasedWidth = p.GetHRAttribute(ccPF.Character_Bold, offset);
               item.BarCode = new BarCode();
               item.BarCode.DotMatrix = p.GetHRAttribute(ccPF.Barcode_Type, offset);
               item.BarCode.HumanReadableFont = p.GetHRAttribute(ccPF.Readable_Code, offset);
               item.BarCode.EANPrefix = p.GetHRAttribute(ccPF.EAN_Prefix, offset);
               // Get calendar condition
               int calCount = p.GetDecAttribute(ccCal.Number_of_Calendar_Blocks, offset);

               // Get count condition
               int cntCount = p.GetDecAttribute(ccCount.Number_Of_Count_Blocks, offset);

               item.Location = new Location() { Index = n, Row = row, Col = col };
               int[] mask = new int[1 + 8];
               ItemType itemType = GetItemType(item.Text, ref mask);
               item.Location.calCount = p.GetDecAttribute(ccCal.Number_of_Calendar_Blocks, offset);
               if (item.Location.calCount > 0) {
                  item.Location.calStart = p.GetDecAttribute(ccCal.First_Calendar_Block, offset);
                  RetrieveCalendarSettings(item, mask);
               }
               item.Location.countCount = p.GetDecAttribute(ccCount.Number_Of_Count_Blocks, offset);
               if (item.Location.countCount > 0) {
                  item.Location.countStart = p.GetDecAttribute(ccCount.First_Count_Block, offset);
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
               offset += stride;
               n++;
               totalCharacters += characterCount;
            }
         }
      }

      // Retrieve Calendar settings
      private void RetrieveCalendarSettings(Item item, int[] mask) {
         int stride = 0x19E0 - 0x19C0;
         int offset = (item.Location.calStart - 1) * stride;
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
                  Year = p.GetHRAttribute(ccCal.Offset_Year, offset),
                  Month = p.GetHRAttribute(ccCal.Offset_Month, offset),
                  Day = p.GetHRAttribute(ccCal.Offset_Day, offset),
                  Hour = p.GetHRAttribute(ccCal.Offset_Hour, offset),
                  Minute = p.GetHRAttribute(ccCal.Offset_Minute, offset)
               };
            }
            if ((mask[i] & DateSubZS) > 0) {
               item.Date[i].ZeroSuppress = new ZeroSuppress();
               string s;
               if ((mask[i] & (int)ba.Year) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Year, offset)))
                     item.Date[i].ZeroSuppress.Year = s;
               if ((mask[i] & (int)ba.Month) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Month, offset)))
                     item.Date[i].ZeroSuppress.Month = s;
               if ((mask[i] & (int)ba.Day) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Day, offset)))
                     item.Date[i].ZeroSuppress.Day = s;
               if ((mask[i] & (int)ba.Hour) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Hour, offset)))
                     item.Date[i].ZeroSuppress.Hour = s;
               if ((mask[i] & (int)ba.Minute) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Minute, offset)))
                     item.Date[i].ZeroSuppress.Minute = s;
               if ((mask[i] & (int)ba.Week) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Weeks, offset)))
                     item.Date[i].ZeroSuppress.Week = s;
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_DayOfWeek, offset)))
                     item.Date[i].ZeroSuppress.DayOfWeek = s;

               item.Date[i].Substitute = new Substitute();
               if ((mask[i] & (int)ba.Year) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Year, offset)))
                     item.Date[i].Substitute.Year = s;
               if ((mask[i] & (int)ba.Month) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Month, offset)))
                     item.Date[i].Substitute.Month = s;
               if ((mask[i] & (int)ba.Day) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Day, offset)))
                     item.Date[i].Substitute.Day = s;
               if ((mask[i] & (int)ba.Hour) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Hour, offset)))
                     item.Date[i].Substitute.Hour = s;
               if ((mask[i] & (int)ba.Minute) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Minute, offset)))
                     item.Date[i].Substitute.Minute = s;
               if ((mask[i] & (int)ba.Week) > 0) // Printer reports these wrong
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_DayOfWeek, offset)))
                     item.Date[i].Substitute.Week = s;
               if ((mask[i] & (int)ba.DayOfWeek) > 0) // Printer reports these wrong
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Weeks, offset)))
                     item.Date[i].Substitute.DayOfWeek = s;
            }
            offset += stride;
         }
      }

      // Retrieve Count Settings
      private void RetrieveCountSettings(Item item) {
         int stride = 0x2074 - 0x1FE0;
         int offset = (item.Location.countCount - 1) * stride;
         item.Counter = new Counter[item.Location.countCount];
         for (int i = 0; i < item.Location.countCount; i++) {
            item.Counter[i] = new Counter() { Block = i + 1 };
            item.Counter[i].Range = new Range() {
               Range1 = p.GetHRAttribute(ccCount.Count_Range_1),
               Range2 = p.GetHRAttribute(ccCount.Count_Range_2),
               JumpFrom = p.GetHRAttribute(ccCount.Jump_From),
               JumpTo = p.GetHRAttribute(ccCount.Jump_To),
            };
            item.Counter[i].Count = new Count() {
               InitialValue = p.GetHRAttribute(ccCount.Initial_Value),
               Increment = p.GetHRAttribute(ccCount.Increment_Value),
               Direction = p.GetHRAttribute(ccCount.Direction_Value),
               ZeroSuppression = p.GetHRAttribute(ccCount.Zero_Suppression),
            };
            item.Counter[i].Reset = new Reset() {
               Type = p.GetHRAttribute(ccCount.Type_Of_Reset_Signal),
               Value = p.GetHRAttribute(ccCount.Reset_Value),
            };
            item.Counter[i].Misc = new Misc() {
               UpdateIP = p.GetHRAttribute(ccCount.Update_Unit_Halfway),
               UpdateUnit = p.GetHRAttribute(ccCount.Update_Unit_Unit),
               ExternalCount = p.GetHRAttribute(ccCount.External_Count),
               Multiplier = p.GetHRAttribute(ccCount.Count_Multiplier),
               SkipCount = p.GetHRAttribute(ccCount.Count_Skip),
            };
            offset += stride;
         }
      }

      // Retrieve shift settings
      private Shift[] RetrieveShifts() {
         List<Shift> s = new List<Shift>();
         string endHour;
         string endMinute;
         int shift = 1;
         int offset = 0;
         int stride = 0x1CF0 - 0x1CE0;
         do {
            s.Add(new Shift() {
               ShiftNumber = shift,
               StartHour = p.GetHRAttribute(ccCal.Shift_Start_Hour, offset),
               StartMinute = p.GetHRAttribute(ccCal.Shift_Start_Minute, offset),
               EndHour = endHour = p.GetHRAttribute(ccCal.Shift_End_Hour, offset),
               EndMinute = endMinute = p.GetHRAttribute(ccCal.Shift_End_Minute, offset),
               ShiftCode = p.GetHRAttribute(ccCal.Shift_String_Value, offset),
            });
            offset += stride;
            shift++;
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

      // Retrieve printer settings
      private Printer RetrievePrinterSettings() {
         Printer p = new Printer();

         return p;
      }

      // Retrieve Substitution rules
      private Substitution RetrieveSubstitutions(Msg message) {
         Substitution s = new Substitution();

         return s;
      }

      #region Service Routines

      // Text is 4 bytes per character
      private string formatText(byte[] text) {
         string result = "";
         for (int i = 0; i < text.Length; i += 4) {
            if (text[i] == 0) {
               result += (char)text[i + 3];
            } else if (text[i] == 0xF2) {
               switch (text[i + 1]) {
                  case 0x50:
                  case 0x60:
                  case 0x70:
                     result += "{Y}";
                     break;
                  case 0x51:
                  case 0x61:
                  case 0x71:
                     result += "{M}";
                     break;
                  case 0x52:
                  case 0x62:
                  case 0x72:
                     result += "{D}";
                     break;
                  case 0x54:
                  case 0x64:
                  case 0x74:
                     result += "{h}";
                     break;
                  case 0x55:
                  case 0x65:
                  case 0x75:
                     result += "{m}";
                     break;
                  case 0x56:
                  case 0x66:
                  case 0x76:
                     result += "{T}";
                     break;
                  case 0x57:
                  case 0x67:
                  case 0x77:
                     result += "{7}";
                     break;
                  case 0x58:
                  case 0x68:
                  case 0x78:
                     result += "{E}";
                     break;
                  case 0x59:
                  case 0x69:
                  case 0x79:
                     result += "{F}";
                     break;
                  case 0x5A:
                  case 0x6A:
                  case 0x7A:
                     result += "{C}";
                     break;
                  case 0X40:
                     result += "{'}";
                     break;
                  case 0X41:
                     result += "{.}";
                     break;
                  case 0X42:
                     result += "{:}";
                     break;
                  case 0X43:
                     result += "{,}";
                     break;
                  case 0X44:
                     result += "{ }";
                     break;
                  case 0X45:
                     result += "{;}";
                     break;
                  case 0X46:
                     result += "{!}";
                     break;
                  default:

                     break;
               }
            } else {
               result += "*";
            }
         }
         return result.Replace("}{", "");
      }

      // Avoid output of property if default value is specified
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
         val = Array.FindIndex(DataII.DropDowns[(int)fmt], x => x.ToLower().Contains(s));
         if (val < 0) {
            val = Array.FindIndex(DataII.DropDownsIJPLib[(int)fmt], x => x.ToLower().Contains(s));
         }
         return val == 0;
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

      #endregion

   }
}
