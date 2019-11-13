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
   class MsgToXml {

      #region Data Declarations

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
         (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute | (int)ba.Second |
         (int)ba.Julian | (int)ba.WeekNumber | (int)ba.DayOfWeek | (int)ba.Shift | (int)ba.TimeCount;

      const int DateOffset =
        (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute | (int)ba.Second |
        (int)ba.Julian | (int)ba.WeekNumber | (int)ba.DayOfWeek;

      const int DateSubZS =
         (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute |
         (int)ba.WeekNumber | (int)ba.DayOfWeek;

      const int DateUseSubRule = DateOffset;

      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
      }

      #endregion

      #region Constructors and Destructors

      public MsgToXml() {
      }

      #endregion

      #region Message to XML 

      public string RetrieveXML(IJPMessage m, IJP ijp = null) {
         string xml = string.Empty;
         ItemType itemType;
         int calBlockNumber = 0;
         int cntBlockNumber = 0;
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
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
                                       case ItemType.Logo:
                                          RetrieveUserPatternSettings(writer);
                                          break;
                                       default:
                                          break;
                                    }

                                    writer.WriteElementString("Text", m.Items[i].Text);
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
                  MessageBox.Show("Help", "EIP I/O Error", MessageBoxButtons.OK);
               }
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
               writer.WriteAttributeString("PrintsPerTrigger", m.RepeatCount.ToString());
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

            RetrieveSubstitutions(writer, m, ijp);

            RetrieveLogos(writer);
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
                     if ((mask[i] & (int)ba.Year) > 0)
                        writer.WriteAttributeString("Year", c.YearZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Month) > 0)
                        writer.WriteAttributeString("Month", c.MonthZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Day) > 0)
                        writer.WriteAttributeString("Day", c.DayZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Hour) > 0)
                        writer.WriteAttributeString("Hour", c.HourZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Minute) > 0)
                        writer.WriteAttributeString("Minute", c.MinuteZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.WeekNumber) > 0)
                        writer.WriteAttributeString("Week", c.WeekNumberZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.DayOfWeek) > 0)
                        writer.WriteAttributeString("DayOfWeek", c.WeekZeroSuppression.ToString());
                  }
                  writer.WriteEndElement(); // End ZeroSuppress

                  writer.WriteStartElement("Substitute"); // Start Substitute
                  {
                     if ((mask[i] & (int)ba.Year) > 0)
                        writer.WriteAttributeString("Year", c.YearSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Month) > 0)
                        writer.WriteAttributeString("Month", c.MonthSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Day) > 0)
                        writer.WriteAttributeString("Day", c.DaySubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Hour) > 0)
                        writer.WriteAttributeString("Hour", c.HourSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Minute) > 0)
                        writer.WriteAttributeString("Minute", c.MinuteSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.WeekNumber) > 0)
                        writer.WriteAttributeString("Week", c.WeekNumberSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.DayOfWeek) > 0)
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
            if ((mask[i] & (int)ba.Shift) > 0) {
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
         }
      }

      private void RetrieveTimeCountSettings(XmlTextWriter writer, IJPTimeCountCondition tc, int[] mask) {
         for (int i = 0; i < mask.Length; i++) {
            if ((mask[i] & (int)ba.TimeCount) > 0) {
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

               writer.WriteStartElement("Reset"); // Start Reset
               {
                  writer.WriteAttributeString("Type", c.ResetSignal.ToString());
                  writer.WriteAttributeString("Value", c.Reset.ToString());
               }
               writer.WriteEndElement(); //  End Reset

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

      private void RetrieveUserPatternSettings(XmlTextWriter writer) {

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

      private void RetrieveLogos(XmlTextWriter writer) {

      }

      private void RetrieveSubstitutions(XmlTextWriter writer, IJPMessage m, IJP ijp) {
         int maxSZ = -1;
         int[] sr = new int[100]; // They are assumed to be 0
         if (ijp != null) {
            for (int i = 0; i < m.CalendarConditions.Count; i++) {
               IJPCalendarCondition cc = m.CalendarConditions[i];
               int n = cc.SubstitutionRuleNumber;
               maxSZ = Math.Max(maxSZ, n);
               if (cc.YearSubstitutionRule)
                  sr[n] |= (int)ba.Year;
               if (cc.MonthSubstitutionRule)
                  sr[n] |= (int)ba.Month;
               if (cc.DaySubstitutionRule)
                  sr[n] |= (int)ba.Day;
               if (cc.HourSubstitutionRule)
                  sr[n] |= (int)ba.Hour;
               if (cc.MinuteSubstitutionRule)
                  sr[n] |= (int)ba.Minute;
               if (cc.WeekNumberSubstitutionRule)
                  sr[n] |= (int)ba.WeekNumber;
               if (cc.WeekSubstitutionRule)
                  sr[n] |= (int)ba.DayOfWeek;
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
                     if ((sr[i] & (int)ba.Year) > 0)
                        WriteSubstitution(writer, srs, ba.Year, 0, 23);
                     if ((sr[i] & (int)ba.Month) > 0)
                        WriteSubstitution(writer, srs, ba.Month, 1, 12);
                     if ((sr[i] & (int)ba.Day) > 0)
                        WriteSubstitution(writer, srs, ba.Day, 1, 31);
                     if ((sr[i] & (int)ba.Hour) > 0)
                        WriteSubstitution(writer, srs, ba.Hour, 0, 23);
                     if ((sr[i] & (int)ba.Minute) > 0)
                        WriteSubstitution(writer, srs, ba.Minute, 0, 59);
                     if ((sr[i] & (int)ba.WeekNumber) > 0)
                        WriteSubstitution(writer, srs, ba.WeekNumber, 1, 53);
                     if ((sr[i] & (int)ba.DayOfWeek) > 0)
                        WriteSubstitution(writer, srs, ba.DayOfWeek, 1, 7);
                  }
                  writer.WriteEndElement(); // Substitution
               }
            }
         }
      }

      private void WriteSubstitution(XmlTextWriter writer, IJPSubstitutionRule srs, ba rule, int start, int end) {
         int n = end - start + 1;
         string[] subCode = new string[n];
         for (int i = 0; i < n; i++) {
            switch (rule) {
               case ba.Year:
                  subCode[i] = srs.GetYearSetup(i + start);
                  break;
               case ba.Month:
                  subCode[i] = srs.GetMonthSetup(i + start);
                  break;
               case ba.Day:
                  subCode[i] = srs.GetDaySetup(i + start);
                  break;
               case ba.Hour:
                  subCode[i] = srs.GetHourSetup(i + start);
                  break;
               case ba.Minute:
                  subCode[i] = srs.GetMinuteSetup(i + start);
                  break;
               case ba.WeekNumber:
                  subCode[i] = srs.GetWeekNumberSetup(i);
                  break;
               case ba.DayOfWeek:
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

   }
}
