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

namespace IJPLibXML {
   public class MsgToXml {

      #region Data Declarations

      // Braced Characters (count, date, half-size, logos
      readonly char[] Bc = new char[] { 'C', 'Y', 'M', 'D', 'h', 'm', 's', 'T', 'W', '7', 'E', 'F', ' ', '\'', '.', ';', ':', '!', ',', 'X', 'Z' };

      // Braced Attributes for Braced Characters
      public enum Ba {
         Count = 1 << 0,
         Year = 1 << 1,
         Month = 1 << 2,
         Day = 1 << 3,
         Hour = 1 << 4,
         Minute = 1 << 5,
         Second = 1 << 6,
         Julian = 1 << 7,
         WeekNumber = 1 << 8,
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
         (int)Ba.Year | (int)Ba.Month | (int)Ba.Day | (int)Ba.Hour | (int)Ba.Minute | (int)Ba.Second |
         (int)Ba.Julian | (int)Ba.WeekNumber | (int)Ba.DayOfWeek | (int)Ba.Shift | (int)Ba.TimeCount;

      const int DateOffset =
        (int)Ba.Year | (int)Ba.Month | (int)Ba.Day | (int)Ba.Hour | (int)Ba.Minute | (int)Ba.Second |
        (int)Ba.Julian | (int)Ba.WeekNumber | (int)Ba.DayOfWeek;

      const int DateSubZS =
         (int)Ba.Year | (int)Ba.Month | (int)Ba.Day | (int)Ba.Hour | (int)Ba.Minute |
         (int)Ba.WeekNumber | (int)Ba.DayOfWeek;

      const int DateUseSubRule = DateOffset;

      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
      }

      struct LogoSave {
         public IJPDotMatrix dm;
         public int location;
         public bool fixedStyle;

         public bool Equals(LogoSave other) {
            return this.fixedStyle.Equals(other.fixedStyle) &&
               this.dm.Equals(other.dm) &&
               this.location.Equals(other.location);
         }
      }

      #endregion

      #region Constructors and Destructors

      public MsgToXml() {
      }

      #endregion

      #region Message to XML 

      public string RetrieveXML(IJPMessage m, IJP ijp = null, IJPMessageInfo mInfo = null) {
         string xml = string.Empty;
         ItemType itemType;
         int calBlockNumber = 0;
         int cntBlockNumber = 0;
         using (MemoryStream MemStream = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(MemStream, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
               try {
                  writer.WriteStartElement("Label"); // Start Label
                  {
                     writer.WriteAttributeString("Version", "1");
                     RetrievePrinterSettings(writer, m, ijp);
                     writer.WriteStartElement("Message"); // Start Message
                     {
                        writer.WriteAttributeString("Layout", m.FormatSetup.ToString());
                        if (mInfo == null) {
                           writer.WriteAttributeString("Name", m.Nickname);
                        } else {
                           writer.WriteAttributeString("Registration", mInfo.RegistrationNumber.ToString());
                           writer.WriteAttributeString("GroupNumber", mInfo.GroupNumber.ToString());
                           writer.WriteAttributeString("Name", mInfo.Nickname);
                        }
                        int item = 0;
                        while (item < m.Items.Count) {
                           writer.WriteStartElement("Column"); // Start Column
                           {
                              int colCount = m.Items[item].PrintLine;
                              writer.WriteAttributeString("InterLineSpacing", m.Items[item].LineSpacing.ToString());
                              for (int i = item; i < item + colCount; i++) {
                                 string text = m.Items[i].Text;
                                 int calBlockCount = m.Items[i].CalendarBlockCount;
                                 int cntBlockCount = m.Items[i].CountBlockCount;
                                 int[] mask = new int[1 + Math.Max(calBlockCount, cntBlockCount)];
                                 itemType = GetItemType(text, ref mask);
                                 writer.WriteStartElement("Item"); // Start Item
                                 {
                                    RetrieveFont(writer, (IJPMessageItem)m.Items[item]);
                                    switch (itemType) {
                                       case ItemType.Text:
                                          break;
                                       case ItemType.Date:
                                          RetrieveCalendarSettings(writer, calBlockNumber, calBlockCount, m.CalendarConditions, mask);
                                          RetrieveShiftSettings(writer, m.ShiftCodes, mask);
                                          RetrieveTimeCountSettings(writer, m.TimeCount, mask);
                                          calBlockNumber += calBlockCount;
                                          break;
                                       case ItemType.Counter:
                                          RetrieveCounterSettings(writer, cntBlockNumber, cntBlockCount, m.CountConditions);
                                          break;
                                       default:
                                          break;
                                    }

                                    writer.WriteElementString("Text", FormatText(m.Items[i].Text));
                                 }
                                 writer.WriteEndElement(); // End Item
                              }
                              item += colCount;
                           }

                           writer.WriteEndElement(); // End Column
                        }
                     }
                     writer.WriteEndElement(); // End Message
                  }
                  writer.WriteEndElement(); // End Label
               } catch (Exception e1) {
                  MessageBox.Show($"IJP_XML: {e1.Message}\n{e1.StackTrace}", "EIP I/O Error", MessageBoxButtons.OK);
               }
               writer.WriteEndDocument();
               writer.Flush();
               MemStream.Position = 0;
               xml = new StreamReader(MemStream).ReadToEnd();
            }
         }
         return xml;
      }

      private void RetrievePrinterSettings(XmlTextWriter writer, IJPMessage m, IJP ijp) {

         writer.WriteStartElement("Printer");
         {
            {
               writer.WriteAttributeString("Make", "Hitachi");
               //IJPUnitInformation ui = (IJPUnitInformation)ijp.GetUnitInformation();
               //writer.WriteAttributeString("Model", ui.TypeName);
            }

            writer.WriteStartElement("PrintHead");
            {
               writer.WriteAttributeString("Orientation", m.CharacterOrientation.ToString());
            }
            writer.WriteEndElement(); // PrintHead

            writer.WriteStartElement("ContinuousPrinting");
            {
               writer.WriteAttributeString("RepeatInterval", m.RepeatIntervals.ToString());
               writer.WriteAttributeString("PrintsPerTrigger",
                  m.RepeatCount == 0 ? "1" : m.RepeatCount.ToString());
            }
            writer.WriteEndElement(); // ContinuousPrinting

            writer.WriteStartElement("TargetSensor");
            {
               writer.WriteAttributeString("Filter", m.TargetSensorFilter.ToString());
               writer.WriteAttributeString("SetupValue", m.TimeSetup.ToString());
               writer.WriteAttributeString("Timer", m.TargetSensorTimer.ToString());
            }
            writer.WriteEndElement(); // TargetSensor

            writer.WriteStartElement("CharacterSize");
            {
               writer.WriteAttributeString("Width", m.CharacterWidth.ToString());
               writer.WriteAttributeString("Height", m.CharacterHeight.ToString());
            }
            writer.WriteEndElement(); // CharacterSize

            writer.WriteStartElement("PrintStartDelay");
            {
               writer.WriteAttributeString("Forward", m.PrintStartDelayForward.ToString());
               writer.WriteAttributeString("Reverse", m.PrintStartDelayReverse.ToString());
            }
            writer.WriteEndElement(); // PrintStartDelay

            writer.WriteStartElement("EncoderSettings");
            {
               writer.WriteAttributeString("HighSpeedPrinting", m.HiSpeedPrint.ToString());
               writer.WriteAttributeString("Divisor", m.PulseRateDivisionFactor.ToString());
               writer.WriteAttributeString("ExternalEncoder", m.ProductSpeedMatching.ToString());
            }
            writer.WriteEndElement(); // EncoderSettings

            writer.WriteStartElement("InkStream");
            {
               writer.WriteAttributeString("InkDropUse", m.InkDropUse.ToString());
               writer.WriteAttributeString("ChargeRule", m.InkDropChargeRule.ToString());
            }
            writer.WriteEndElement(); // InkStream

            if (ijp != null) {
               writer.WriteStartElement("ClockSystem");
               {
                  writer.WriteAttributeString("HourMode24", ijp.GetHourMode24().ToString());
               }
               writer.WriteEndElement(); // ClockSystem
               RetrieveSubstitutions(writer, m, ijp);
               RetrieveUserPatternSettings(writer, m, ijp);
            }
         }
         writer.WriteEndElement(); // Printer
      }

      private void RetrieveFont(XmlTextWriter writer, IJPMessageItem m) {
         writer.WriteStartElement("Font"); // Start Font
         {
            writer.WriteAttributeString("InterCharacterSpace", m.InterCharacterSpace.ToString());
            writer.WriteAttributeString("IncreasedWidth", m.Bold.ToString());
            writer.WriteAttributeString("DotMatrix", m.DotMatrix.ToString());
         }
         writer.WriteEndElement(); // End Font

         writer.WriteStartElement("BarCode"); // Start Barcode
         {
            if (m.Barcode != IJPBarcode.Nothing) {
               writer.WriteAttributeString("HumanReadableFont", m.ReadableCode.ToString());
               writer.WriteAttributeString("EANPrefix", m.Prefix.ToString());
               writer.WriteAttributeString("DotMatrix", m.Barcode.ToString());
            }
         }
         writer.WriteEndElement(); // End BarCode
      }

      private void RetrieveCalendarSettings(XmlTextWriter writer, int FirstBlock, int BlockCount, IJPCalendarConditionCollection cc, int[] mask) {

         for (int i = 0; i < BlockCount; i++) {
            IJPCalendarCondition c = cc[FirstBlock + i];
            // Where is the Substitution Rule
            writer.WriteStartElement("Date"); // Start Date
            {
               writer.WriteAttributeString("Block", (i + 1).ToString());
               if ((mask[i] & DateUseSubRule) > 0) {
                  writer.WriteAttributeString("SubstitutionRule", c.SubstitutionRuleNumber.ToString());
                  writer.WriteAttributeString("RuleName", "");
               }

               if ((mask[i] & DateOffset) > 0) { // Not always needed
                  writer.WriteStartElement("Offset"); // Start Offset
                  {
                     writer.WriteAttributeString("Year", c.YearOffset.ToString());
                     writer.WriteAttributeString("Month", c.MonthOffset.ToString());
                     writer.WriteAttributeString("Day", c.DayOffset.ToString());
                     writer.WriteAttributeString("Hour", c.HourOffset.ToString());
                     writer.WriteAttributeString("Minute", c.MinuteOffset.ToString());
                  }
                  writer.WriteEndElement(); // End Offset
               }

               if ((mask[i] & DateSubZS) > 0) {
                  writer.WriteStartElement("ZeroSuppress"); // Start ZeroSuppress
                  {
                     if ((mask[i] & (int)Ba.Year) > 0)
                        writer.WriteAttributeString("Year", c.YearZeroSuppression.ToString());
                     if ((mask[i] & (int)Ba.Month) > 0)
                        writer.WriteAttributeString("Month", c.MonthZeroSuppression.ToString());
                     if ((mask[i] & (int)Ba.Day) > 0)
                        writer.WriteAttributeString("Day", c.DayZeroSuppression.ToString());
                     if ((mask[i] & (int)Ba.Hour) > 0)
                        writer.WriteAttributeString("Hour", c.HourZeroSuppression.ToString());
                     if ((mask[i] & (int)Ba.Minute) > 0)
                        writer.WriteAttributeString("Minute", c.MinuteZeroSuppression.ToString());
                     if ((mask[i] & (int)Ba.WeekNumber) > 0)
                        writer.WriteAttributeString("Week", c.WeekNumberZeroSuppression.ToString());
                     if ((mask[i] & (int)Ba.DayOfWeek) > 0)
                        writer.WriteAttributeString("DayOfWeek", c.WeekZeroSuppression.ToString());
                  }
                  writer.WriteEndElement(); // End ZeroSuppress

                  writer.WriteStartElement("Substitute"); // Start Substitute
                  {
                     if ((mask[i] & (int)Ba.Year) > 0)
                        writer.WriteAttributeString("Year", c.YearSubstitutionRule.ToString());
                     if ((mask[i] & (int)Ba.Month) > 0)
                        writer.WriteAttributeString("Month", c.MonthSubstitutionRule.ToString());
                     if ((mask[i] & (int)Ba.Day) > 0)
                        writer.WriteAttributeString("Day", c.DaySubstitutionRule.ToString());
                     if ((mask[i] & (int)Ba.Hour) > 0)
                        writer.WriteAttributeString("Hour", c.HourSubstitutionRule.ToString());
                     if ((mask[i] & (int)Ba.Minute) > 0)
                        writer.WriteAttributeString("Minute", c.MinuteSubstitutionRule.ToString());
                     if ((mask[i] & (int)Ba.WeekNumber) > 0)
                        writer.WriteAttributeString("Week", c.WeekNumberSubstitutionRule.ToString());
                     if ((mask[i] & (int)Ba.DayOfWeek) > 0)
                        writer.WriteAttributeString("DayOfWeek", c.WeekSubstitutionRule.ToString());
                  }
                  writer.WriteEndElement(); // End EnableSubstitution
               }

            }
            writer.WriteEndElement(); // End Date
         }
      }

      private void RetrieveShiftSettings(XmlTextWriter writer, IJPShiftCodeCollection ss, int[] mask) {
         for (int i = 0; i < mask.Length; i++) {
            if ((mask[i] & (int)Ba.Shift) > 0) {
               writer.WriteStartElement("Shifts"); // Start Shifts
               {
                  for (int shift = 0; shift < ss.Count; shift++) {
                     writer.WriteStartElement("Shift"); // Start Shift
                     {
                        writer.WriteAttributeString("ShiftNumber", (shift + 1).ToString());
                        writer.WriteAttributeString("StartHour", ss[shift].StartTime.Hour.ToString());
                        writer.WriteAttributeString("StartMinute", ss[shift].StartTime.Minute.ToString());
                        writer.WriteAttributeString("ShiftCode", ss[shift].String);
                     }
                     writer.WriteEndElement(); // End Shift
                  }
               }
               writer.WriteEndElement(); // End Shifts
            }
         }
      }

      private void RetrieveTimeCountSettings(XmlTextWriter writer, IJPTimeCountCondition tc, int[] mask) {
         for (int i = 0; i < mask.Length; i++) {
            if ((mask[i] & (int)Ba.TimeCount) > 0) {
               writer.WriteStartElement("TimeCount"); // Start TimeCount
               {
                  writer.WriteAttributeString("Interval", tc.RenewalPeriod.ToString());
                  writer.WriteAttributeString("Start", tc.LowerRange.ToString());
                  writer.WriteAttributeString("End", tc.UpperRange.ToString());
                  writer.WriteAttributeString("ResetTime", tc.ResetTime.ToString());
                  writer.WriteAttributeString("ResetValue", tc.Reset.ToString());
               }
               writer.WriteEndElement(); // End TimeCount
            }
         }
      }

      private void RetrieveCounterSettings(XmlTextWriter writer, int FirstBlock, int BlockCount, IJPCountConditionCollection cc) {
         for (int i = 0; i < BlockCount; i++) {
            IJPCountCondition c = cc[FirstBlock + i];
            writer.WriteStartElement("Counter"); // Start Counter
            {
               writer.WriteAttributeString("Block", (i + 1).ToString());
               writer.WriteStartElement("Range"); // Start Range
               {
                  writer.WriteAttributeString("Range1", c.LowerRange);
                  writer.WriteAttributeString("Range2", c.UpperRange);
                  writer.WriteAttributeString("JumpFrom", c.JumpFrom);
                  writer.WriteAttributeString("JumpTo", c.JumpTo);
               }
               writer.WriteEndElement(); //  End Range

               writer.WriteStartElement("Count"); // Start Count
               {
                  writer.WriteAttributeString("InitialValue", c.Value);
                  writer.WriteAttributeString("Increment", c.Increment.ToString());
                  writer.WriteAttributeString("Direction", c.Direction.ToString());
                  writer.WriteAttributeString("ZeroSuppression", c.SuppressesZero.ToString());
               }
               writer.WriteEndElement(); //  End Count

               //writer.WriteStartElement("Reset"); // Start Reset
               //{
               //   writer.WriteAttributeString("Type", c.ResetSignal.ToString());
               //   writer.WriteAttributeString("Value", c.Reset.ToString());
               //}
               //writer.WriteEndElement(); //  End Reset

               writer.WriteStartElement("Misc"); // Start Misc
               {
                  writer.WriteAttributeString("UpdateIP", c.UpdateInProgress.ToString());
                  writer.WriteAttributeString("UpdateUnit", c.UpdateUnit.ToString());
                  writer.WriteAttributeString("ExternalCount", c.UsesExternalSignalCount.ToString());
                  //writer.WriteAttributeString("Multiplier",c.Multiplier.ToString());
                  //writer.WriteAttributeString("SkipCount",c.CountSkip.ToString());
               }
               writer.WriteEndElement(); //  End Misc

            }
            writer.WriteEndElement(); //  End Counter
         }
      }

      private void RetrieveUserPatternSettings(XmlTextWriter writer, IJPMessage m, IJP ijp) {
         List<LogoSave> neededLogos = new List<LogoSave>();
         for (int i = 0; i < m.Items.Count; i++) {
            string s = m.Items[i].Text;
            for (int n = 0; n < s.Length; n++) {
               char c = s[n];
               if (c >= IJPLib_XML.FirstFixedUP && c <= IJPLib_XML.LastFixedUP) {
                  neededLogos.Add(new LogoSave() { fixedStyle = true, dm = m.Items[i].DotMatrix, location = c - IJPLib_XML.FirstFixedUP });
               } else if (c >= IJPLib_XML.FirstFreeUP && c <= IJPLib_XML.LastFreeUP) {
                  neededLogos.Add(new LogoSave() { fixedStyle = false, dm = m.Items[i].DotMatrix, location = c - IJPLib_XML.FirstFreeUP });
               }
            }

         }
         // Any referenced?
         if (neededLogos.Count > 0) {
            // Eliminate duplicates
            neededLogos = neededLogos.OrderBy(o => o.location).OrderBy(o => o.dm).OrderBy(o => o.fixedStyle).ToList();
            for (int i = 0; i < neededLogos.Count - 1; i++) {
               if (neededLogos[i].Equals(neededLogos[i = 1])) {
                  neededLogos.RemoveAt(i + 1);
               }
            }
            // Output the remainder
            writer.WriteStartElement("Logos"); // Start Logos
            {
               //writer.WriteAttributeString("Folder", Properties.Settings.Default.MessageFolder);
               // Now write the logos
               IIJPUserPattern up;
               for (int i = 0; i < neededLogos.Count; i++) {
                  writer.WriteStartElement("Logo"); // Start Logo
                  {
                     if (neededLogos[i].fixedStyle) {
                        writer.WriteAttributeString("Layout", "Fixed");
                        up = ijp.GetFixedUserPattern(neededLogos[i].location + 1, neededLogos[i].dm);
                     } else {
                        writer.WriteAttributeString("Layout", "Free");
                        up = ijp.GetFreeUserPattern(neededLogos[i].location + 1);
                     }
                     writer.WriteAttributeString("DotMatrix", neededLogos[i].dm.ToString());
                     writer.WriteAttributeString("Location", neededLogos[i].location.ToString());
                     writer.WriteAttributeString("FileName", "");
                     writer.WriteAttributeString("Width", up.Pattern.Width.ToString());
                     writer.WriteAttributeString("Height", up.Pattern.Height.ToString());

                     writer.WriteAttributeString("RawData", BitMapToRawData(up.Pattern));
                  }
                  writer.WriteEndElement(); //  End Logo
               }


            }
            writer.WriteEndElement(); //  End Logos
         }
      }

      private string BitMapToRawData(Bitmap bm) {
         ulong[] stripes = new ulong[bm.Width];
         for (int x = 0; x < bm.Width; x++) {
            for (int y = 0; y < bm.Height; y++) {
               stripes[x] <<= 1;
               if (bm.GetPixel(x, y).ToArgb() == Color.Black.ToArgb()) {
                  stripes[x] |= 1;
               }
            }
         }
         // Eliminate trailing stripes
         int n = 0;
         for (n = stripes.Length - 1; n > 0; n--) {
            if (stripes[n] != 0) {
               break;
            }
         }
         // Generate the raw data
         int stride = (bm.Height + 7) / 8;
         string result = "";
         for (int i = 0; i <= n; i++) {
            for (int j = 0; j < stride; j++) {
               result += $"{stripes[i] & 0xFF:X2} ";
               stripes[i] >>= 8;
            }
         }

         return result.Trim();
      }

      private void RetrieveSubstitutions(XmlTextWriter writer, IJPMessage m, IJP ijp) {
         int maxSZ = -1;
         int[] sr = new int[100]; // They are assumed to be 0
         for (int i = 0; i < m.CalendarConditions.Count; i++) {
            IJPCalendarCondition cc = m.CalendarConditions[i];
            int n = cc.SubstitutionRuleNumber;
            maxSZ = Math.Max(maxSZ, n);
            if (cc.YearSubstitutionRule)
               sr[n] |= (int)Ba.Year;
            if (cc.MonthSubstitutionRule)
               sr[n] |= (int)Ba.Month;
            if (cc.DaySubstitutionRule)
               sr[n] |= (int)Ba.Day;
            if (cc.HourSubstitutionRule)
               sr[n] |= (int)Ba.Hour;
            if (cc.MinuteSubstitutionRule)
               sr[n] |= (int)Ba.Minute;
            if (cc.WeekNumberSubstitutionRule)
               sr[n] |= (int)Ba.WeekNumber;
            if (cc.WeekSubstitutionRule)
               sr[n] |= (int)Ba.DayOfWeek;
         }
         for (int i = 1; i <= maxSZ; i++) {
            if (sr[i] > 0) {
               IJPSubstitutionRule srs = (IJPSubstitutionRule)ijp.GetSubstitutionRule(i);
               writer.WriteStartElement("Substitution");
               {
                  writer.WriteAttributeString("Delimiter", "/");
                  writer.WriteAttributeString("StartYear", srs.StartYear.ToString());
                  writer.WriteAttributeString("RuleNumber", i.ToString());
                  writer.WriteAttributeString("RuleName", srs.Name);
                  if ((sr[i] & (int)Ba.Year) > 0)
                     RetrieveSubstitution(writer, srs, Ba.Year, 0, 23);
                  if ((sr[i] & (int)Ba.Month) > 0)
                     RetrieveSubstitution(writer, srs, Ba.Month, 1, 12);
                  if ((sr[i] & (int)Ba.Day) > 0)
                     RetrieveSubstitution(writer, srs, Ba.Day, 1, 31);
                  if ((sr[i] & (int)Ba.Hour) > 0)
                     RetrieveSubstitution(writer, srs, Ba.Hour, 0, 23);
                  if ((sr[i] & (int)Ba.Minute) > 0)
                     RetrieveSubstitution(writer, srs, Ba.Minute, 0, 59);
                  if ((sr[i] & (int)Ba.WeekNumber) > 0)
                     RetrieveSubstitution(writer, srs, Ba.WeekNumber, 1, 53);
                  if ((sr[i] & (int)Ba.DayOfWeek) > 0)
                     RetrieveSubstitution(writer, srs, Ba.DayOfWeek, 1, 7);
               }
               writer.WriteEndElement(); // Substitution
            }
         }
      }

      private void RetrieveSubstitution(XmlTextWriter writer, IJPSubstitutionRule srs, Ba rule, int start, int end) {
         int n = end - start + 1;
         string[] subCode = new string[n];
         for (int i = 0; i < n; i++) {
            switch (rule) {
               case Ba.Year:
                  subCode[i] = srs.GetYearSetup(i + start);
                  break;
               case Ba.Month:
                  subCode[i] = srs.GetMonthSetup(i + start);
                  break;
               case Ba.Day:
                  subCode[i] = srs.GetDaySetup(i + start);
                  break;
               case Ba.Hour:
                  subCode[i] = srs.GetHourSetup(i + start);
                  break;
               case Ba.Minute:
                  subCode[i] = srs.GetMinuteSetup(i + start);
                  break;
               case Ba.WeekNumber:
                  subCode[i] = srs.GetWeekNumberSetup(i);
                  break;
               case Ba.DayOfWeek:
                  subCode[i] = srs.GetWeekSetup((DayOfWeek)i);
                  break;
            }
         }
         for (int i = 0; i < n; i += 10) {
            writer.WriteStartElement("Rule");
            {
               writer.WriteAttributeString("Type", rule.ToString());
               writer.WriteAttributeString("Base", (i + start).ToString());
               writer.WriteString(string.Join("/", subCode, i, Math.Min(10, n - i)));
            }
            writer.WriteEndElement(); // Rule
         }
      }

      #endregion

      #region Service Routines

      // Examine the contents of a print message to determine its type
      private ItemType GetItemType(string text, ref int[] mask) {
         int l = 0;
         mask[l] = 0;
         string[] s = text.Split('{');
         for (int i = 0; i < s.Length; i++) {
            int n = s[i].IndexOf('}');
            if (n >= 0) {
               for (int j = 0; j < n; j++) {
                  int k = Array.IndexOf(Bc, s[i][j]);
                  if (k >= 0) {
                     mask[l] |= 1 << k;
                  } else {
                     mask[l] |= (int)Ba.Unknown;
                  }
               }
            }
            if (s[i].IndexOf('}', n + 1) > 0) {
               l++;
            }
         }
         // Calendar and Count cannot appear in the same item
         if ((mask[0] & (int)Ba.Count) > 0) {
            return ItemType.Counter;
         } else if ((mask[0] & DateCode) > 0) {
            return ItemType.Date;
         } else {
            return ItemType.Text;
         }
      }

      // Resolve differences between IJPLib and EtherNet/IP text syntax
      private string FormatText(string s) {
         string result = string.Empty;
         int start = s.IndexOf("{");
         if (start >= 0) {
            int end = s.LastIndexOf("}");
            if (end > 0) {
               string text = s.Substring(start + 1, end - start - 1).Trim().Replace("{", "").Replace("}", "");
               s = s.Substring(0, start + 1) + text + s.Substring(end);
            }
         }
         for (int i = 0; i < s.Length; i++) {
            char c = s[i];
            if (c >= IJPLib_XML.FirstFixedUP && c <= IJPLib_XML.LastFixedUP) {
               result += $"{{X/{c - IJPLib_XML.FirstFixedUP}}}";
            } else if (c >= IJPLib_XML.FirstFreeUP && c <= IJPLib_XML.LastFreeUP) {
               result += $"{{Z/{c - IJPLib_XML.FirstFreeUP}}}";
            } else {
               result += s.Substring(i, 1);
            }
         }
         return result;
      }

      #endregion

   }
}
