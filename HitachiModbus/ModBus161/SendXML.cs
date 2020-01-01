using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;
using Modbus_DLL;

namespace ModBus161 {

   public partial class SendRetrieveXML {

      #region Data Declarations

      bool UseAutomaticReflection;

      public Encoding Encode = Encoding.UTF8;

      #endregion

      #region Methods

      public bool SendXML(string xml, bool AutoReflect = true) {
         if (xml.IndexOf("<Label", StringComparison.OrdinalIgnoreCase) < 0) {
            xml = File.ReadAllText(xml);
         }
         bool success = true;
         Lab Lab;
         XmlSerializer serializer = new XmlSerializer(typeof(Lab));
         try {
            // Arm the Serializer
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            using (TextReader reader = new StringReader(xml)) {
               // Deserialize the file contents
               Lab = (Lab)serializer.Deserialize(reader);
               SendXML(Lab, AutoReflect);
            }
         } catch (Exception e) {
            success = false;
            parent.Log(e.Message);
            // String passed is not XML, simply return defaultXmlClass
         } finally {
            // Release the error detection events
            serializer.UnknownNode -= new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute -= new XmlAttributeEventHandler(serializer_UnknownAttribute);
         }
         return success;
      }

      public void SendXML(Lab Lab, bool AutoReflect = true) {
         UseAutomaticReflection = AutoReflect; // Speed up processing
         try {

            if (Lab.Printer != null && Lab.Printer.Logos != null) {
               SendLogos(Lab.Printer.Logos);
            }

            if (Lab.Message != null) {
               SendMessage(Lab.Message);
            }

            if (Lab.Printer != null) {
               SendPrinterSettings(Lab.Printer); // Must be done last
            }
         } catch (Exception e2) {
            parent.Log(e2.Message);
         }
         UseAutomaticReflection = false;
      }

      #endregion

      #region Sent Message to printer

      private void SendMessage(Msg m) {
         // Set to only one item in printer
         DeleteAllButOne();

         if (m.Column != null) {
            AllocateRowsColumns(m);
         }
      }

      // Simulate Delete All But One
      public void DeleteAllButOne() {
         int lineCount;
         int n = 0;
         int cols = 0;            // Holds the number of rows in each column
         int itemCount = p.GetDecAttribute(ccIDX.Number_Of_Items);
         while (n < itemCount) {
            lineCount = p.GetDecAttribute(ccPF.Line_Count, n);
            n += lineCount;
            cols++;
         }

         for (int i = 0; i < cols - 1; i++) {
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccPF.Delete_Column, cols - i);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }

         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccPF.Column, 1);
         p.SetAttribute(ccPF.Line, 1);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccPF.Dot_Matrix, 0, "5x8");           // Clear any barcodes
         p.SetAttribute(ccPF.Barcode_Type, 0, "None");
         p.SetAttribute(ccIDX.Characters_per_Item, 0, 1);
         p.SetAttribute(ccIDX.Print_Character_String, 0, "1"); // Set simple text in case Calendar or Counter was used
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
      }

      private void AllocateRowsColumns(Msg m) {
         int index = 0;
         bool hasDateOrCount = false; // Save some time if no need to look
         int charPosition = 0;
         for (int c = 0; c < m.Column.Length; c++) {
            if (c > 0) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               p.SetAttribute(ccPF.Add_Column, 0);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            }
            // Should this be Column and not Item?
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccPF.Column, c + 1);
            p.SetAttribute(ccPF.Line, m.Column[c].Item.Length);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               if (m.Column[c].Item.Length > 1) {
                  p.SetAttribute(ccPF.Line_Spacing, index, m.Column[c].InterLineSpacing);
               }
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               Item item = m.Column[c].Item[r];
               if (item.Font != null) {
                  p.SetAttribute(ccPF.Dot_Matrix, index, item.Font.DotMatrix);
                  p.SetAttribute(ccPF.InterCharacter_Space, index, item.Font.InterCharacterSpace);
                  p.SetAttribute(ccPF.Character_Bold, index, item.Font.IncreasedWidth);
               }
               string s = p.HandleBraces(item.Text);
               p.SetAttribute(ccIDX.Characters_per_Item, index, s.Length);
               p.SetAttribute(ccIDX.Print_Character_String, charPosition, s);
               charPosition += item.Text.Length;
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
               hasDateOrCount |= item.Date != null | item.Counter != null | item.Shift != null | item.TimeCount != null;
               m.Column[c].Item[r].Location = new Location() { Index = index++, Row = r, Col = c };
            }
         }
         // Process calendar and count if needed
         if (hasDateOrCount) {
            SendDateCount(m);
         }
      }

      private void SendDateCount(Msg m) {
         // Need a combination of sets and gets.  Turn AutoReflection off
         bool saveAR = UseAutomaticReflection;
         UseAutomaticReflection = false;
         // Get calendar and count blocks assigned by the printer
         for (int c = 0; c < m.Column.Length; c++) {
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               Item item = m.Column[c].Item[r];
               int index = m.Column[c].Item[r].Location.Index;
               if (item.Date != null) {
                  //p.SetAttribute(ccIDX.Item, index);
                  item.Location.calCount = p.GetDecAttribute(ccPF.Number_of_Calendar_Blocks, index);
                  item.Location.calStart = p.GetDecAttribute(ccPF.First_Calendar_Block, index);
               }
               if (item.Counter != null) {
                  //p.SetAttribute(ccIDX.Item, index);
                  item.Location.countCount = p.GetDecAttribute(ccPF.Number_Of_Count_Blocks, index);
                  item.Location.countStart = p.GetDecAttribute(ccPF.First_Count_Block, index);
               }
            }
         }

         // Restore previous AutoReflection to previous state
         UseAutomaticReflection = saveAR;
         for (int c = 0; c < m.Column.Length; c++) {
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               Item item = m.Column[c].Item[r];
               if (item.Date != null) {
                  SendCalendar(item);
               }
               if (item.Counter != null) {
                  SendCount(item);
               }
               if (item.Shift != null) {
                  SendShift(item);
               }
               if (item.TimeCount != null) {
                  SendTimeCount(item);
               }
            }
         }
      }

      private void SendCalendar(Item item) {
         int calStart = item.Location.calStart;
         int calCount = item.Location.calCount;
         for (int i = 0; i < item.Date.Length; i++) {
            Date date = item.Date[i];
            if (date.Block <= calCount) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               p.SetAttribute(ccIDX.Substitution_Rule, 1); // date.SubstitutionRule
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

               int index = calStart + date.Block - 2; // Cal start and date.Block are both 1-origin
               // Process Offset
               Offset o = date.Offset;
               int n;
               if (o != null) {
                  if (int.TryParse(o.Year, out n) && n != 0) {
                     p.SetAttribute(ccCal.Offset_Year, index, n);
                  }
                  if (int.TryParse(o.Month, out n) && n != 0) {
                     p.SetAttribute(ccCal.Offset_Month, index, n);
                  }
                  if (int.TryParse(o.Day, out n) && n != 0) {
                     p.SetAttribute(ccCal.Offset_Day, index, n);
                  }
                  if (int.TryParse(o.Hour, out n) && n != 0) {
                     p.SetAttribute(ccCal.Offset_Hour, index, n);
                  }
                  if (int.TryParse(o.Minute, out n) && n != 0) {
                     p.SetAttribute(ccCal.Offset_Minute, index, n);
                  }
               }

               // Process Zero Suppress
               ZeroSuppress zs = date.ZeroSuppress;
               if (zs != null) {
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Year)) {
                     p.SetAttribute(ccCal.Zero_Suppress_Year, index, zs.Year);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Month)) {
                     p.SetAttribute(ccCal.Zero_Suppress_Month, index, zs.Month);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Day)) {
                     p.SetAttribute(ccCal.Zero_Suppress_Day, index, zs.Day);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Hour)) {
                     p.SetAttribute(ccCal.Zero_Suppress_Hour, index, zs.Hour);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Minute)) {
                     p.SetAttribute(ccCal.Zero_Suppress_Minute, index, zs.Minute);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Week)) {
                     p.SetAttribute(ccCal.Zero_Suppress_Weeks, index, zs.Week);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.DayOfWeek)) {
                     p.SetAttribute(ccCal.Zero_Suppress_DayOfWeek, zs.DayOfWeek);
                  }
               }

               // Process Substitutions
               Substitute s = date.Substitute;
               if (s != null) {
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Year)) {
                     p.SetAttribute(ccCal.Substitute_Year, index, s.Year);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Month)) {
                     p.SetAttribute(ccCal.Substitute_Month, index, s.Month);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Day)) {
                     p.SetAttribute(ccCal.Substitute_Day, index, s.Day);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Hour)) {
                     p.SetAttribute(ccCal.Substitute_Hour, index, s.Hour);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Minute)) {
                     p.SetAttribute(ccCal.Substitute_Minute, index, s.Minute);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Week)) {
                     p.SetAttribute(ccCal.Substitute_Weeks, index, s.Week);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.DayOfWeek)) {
                     p.SetAttribute(ccCal.Substitute_DayOfWeek, index, s.DayOfWeek);
                  }
               }
            }
         }
      }

      private void SendCount(Item item) {
         int countStart = item.Location.countStart;
         int countCount = item.Location.countCount;
         for (int i = 0; i < item.Counter.Length; i++) {
            Counter c = item.Counter[i];
            if (c.Block <= countCount) {
               int index = countStart + c.Block - 2; // Both count start and count block are 1-origin
               // Process Range
               Range r = c.Range;
               if (r != null) {
                  p.SetAttribute(ccCount.Count_Range_1, index, r.Range1);
                  p.SetAttribute(ccCount.Count_Range_2, index, r.Range2);
                  p.SetAttribute(ccCount.Jump_From, index, r.JumpFrom);
                  p.SetAttribute(ccCount.Jump_To, index, r.JumpTo);
               }

               // Process Count
               Count cc = c.Count;
               if (cc != null) {
                  p.SetAttribute(ccCount.Initial_Value, index, cc.InitialValue);
                  p.SetAttribute(ccCount.Increment_Value, index, cc.Increment);
                  p.SetAttribute(ccCount.Direction_Value, index, cc.Direction);
                  p.SetAttribute(ccCount.Zero_Suppression, index, cc.ZeroSuppression);
               }

               // Process Reset
               Reset rr = c.Reset;
               if (rr != null) {
                  p.SetAttribute(ccCount.Type_Of_Reset_Signal, index, rr.Type);
                  p.SetAttribute(ccCount.Reset_Value, index, rr.Value);
               }

               // Process Misc
               Misc m = c.Misc;
               if (m != null) {
                  p.SetAttribute(ccCount.Update_Unit_Unit, index, m.UpdateUnit);
                  p.SetAttribute(ccCount.Update_Unit_Halfway, index, m.UpdateIP);
                  p.SetAttribute(ccCount.External_Count, index, m.ExternalCount);
                  p.SetAttribute(ccCount.Count_Multiplier, index, m.Multiplier);
                  p.SetAttribute(ccCount.Count_Skip, index, m.SkipCount);
               }
            }
         }
      }

      private void SendShift(Item item) {
         // Process Shift
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         for (int j = 0; j < item.Shift.Length; j++) {
            p.SetAttribute(ccSR.Shift_Start_Hour, j, item.Shift[j].StartHour);
            p.SetAttribute(ccSR.Shift_Start_Minute, j, item.Shift[j].StartMinute);
            p.SetAttribute(ccSR.Shift_String_Value, j, item.Shift[j].ShiftCode);
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
      }

      private void SendTimeCount(Item item) {
         TimeCount tc = item.TimeCount;
         if (tc != null) {
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccSR.Update_Interval_Value, tc.Interval);
            p.SetAttribute(ccSR.Time_Count_Start_Value, tc.Start);
            p.SetAttribute(ccSR.Time_Count_End_Value, tc.End);
            p.SetAttribute(ccSR.Reset_Time_Value, tc.ResetTime);
            p.SetAttribute(ccSR.Time_Count_Reset_Value, tc.ResetValue);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }
      }

      #endregion

      #region Send Printer Settings to printer

      private void SendPrinterSettings(Printer ptr) {
         if (ptr.PrintHead != null) {
            p.SetAttribute(ccPS.Character_Orientation, ptr.PrintHead.Orientation);
         }
         if (ptr.ContinuousPrinting != null) {
            p.SetAttribute(ccPS.Repeat_Interval, ptr.ContinuousPrinting.RepeatInterval);
            p.SetAttribute(ccPS.Repeat_Count, ptr.ContinuousPrinting.PrintsPerTrigger);
         }
         if (ptr.TargetSensor != null) {
            p.SetAttribute(ccPS.Target_Sensor_Filter, ptr.TargetSensor.Filter);
            p.SetAttribute(ccPS.Target_Sensor_Filter_Value, ptr.TargetSensor.SetupValue);
            p.SetAttribute(ccPS.Target_Sensor_Timer, ptr.TargetSensor.Timer);
         }
         if (ptr.CharacterSize != null) {
            p.SetAttribute(ccPS.Character_Width, ptr.CharacterSize.Width);
            p.SetAttribute(ccPS.Character_Height, ptr.CharacterSize.Height);
         }
         if (ptr.PrintStartDelay != null) {
            p.SetAttribute(ccPS.Print_Start_Delay_Forward, ptr.PrintStartDelay.Forward);
            p.SetAttribute(ccPS.Print_Start_Delay_Reverse, ptr.PrintStartDelay.Reverse);
         }
         if (ptr.EncoderSettings != null) {
            p.SetAttribute(ccPS.High_Speed_Print, ptr.EncoderSettings.HighSpeedPrinting);
            p.SetAttribute(ccPS.Pulse_Rate_Division_Factor, ptr.EncoderSettings.Divisor);
            p.SetAttribute(ccPS.Product_Speed_Matching, ptr.EncoderSettings.ExternalEncoder);
         }
         if (ptr.InkStream != null) {
            p.SetAttribute(ccPS.Ink_Drop_Use, ptr.InkStream.InkDropUse);
            p.SetAttribute(ccPS.Ink_Drop_Charge_Rule, ptr.InkStream.ChargeRule);
         }
         if (ptr.Substitution != null && ptr.Substitution.SubRule != null) {
            if (int.TryParse(ptr.Substitution.RuleNumber, out int ruleNumber)
               && int.TryParse(ptr.Substitution.StartYear, out int year)
               && ptr.Substitution.Delimiter.Length == 1) {
               // Substitution rules cannot be set with Auto Reflection on
               bool saveAR = UseAutomaticReflection;
               UseAutomaticReflection = false;
               // Force rule to be loaded
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               p.SetAttribute(ccIDX.Substitution_Rule, ruleNumber);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
               p.SetAttribute(ccSR.Start_Year, year);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               SendSubstitution(ptr.Substitution, ptr.Substitution.Delimiter);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

               UseAutomaticReflection = saveAR;
            }
         }
      }

      private void SendLogos(Logos logos) {
         int[] logoLen = new int[] { 0, 8, 8, 8, 16, 16, 32, 32, 72, 128, 32, 5, 5, 7, 200, 288 };
         foreach (Logo l in logos.Logo) {
            // Load the logo into the pattern area
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccIDX.User_Pattern_Size, l.DotMatrix);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            if (int.TryParse(l.Location, out int loc) && l.RawData.Length > 0) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               // Write the registration bit
               int regLoc = loc / 16;
               int regBit = 15 - (loc % 16);
               AttrData attr = p.GetAttrData(ccUP.User_Pattern_Fixed_Registration);
               int regMask = p.GetDecAttribute(ccUP.User_Pattern_Fixed_Registration, regLoc);
               regMask |= 1 << regBit;
               p.SetAttribute(ccUP.User_Pattern_Fixed_Registration, regLoc, regMask);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
               // Write the pattern
               int n = p.ToDropdownValue(p.GetAttrData(ccIDX.User_Pattern_Size).Data, l.DotMatrix);
               byte[] data = new byte[logoLen[n]];
               byte[] rawdata = p.string_to_byte(l.RawData);
               for (int i = 0; i < Math.Min(data.Length, rawdata.Length); i++) {
                  data[i] = rawdata[i];
               }
               // Write the pattern data
               attr = p.GetAttrData(ccUP.User_Pattern_Fixed_Data);
               int addr = attr.Val + loc * (logoLen[n] / 2);
               p.SetAttribute(addr, data);
            }
         }
      }

      private void SendSubstitution(Substitution s, string delimiter) {
         for (int i = 0; i < s.SubRule.Length; i++) {
            SubstitutionRule r = s.SubRule[i];
            if (Enum.TryParse(r.Type, true, out ccSR type)) {
               SetSubValues(type, r, delimiter);
            } else {
               parent.Log($"Unknown substitution rule type =>{r.Type}<=");
            }
         }
      }

      private void SetSubValues(ccSR attribute, SubstitutionRule r, string delimeter) {
         if (int.TryParse(r.Base, out int b)) {
            Prop prop = p.AttrDict[ClassCode.Substitution_rules, (int)attribute].Data;
            string[] s = r.Text.Split(delimeter[0]);
            for (int i = 0; i < s.Length; i++) {
               int n = b + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  p.SetAttribute(attribute, n - prop.Min, s[i]);
               }
            }
         }
      }

      #endregion

      #region Service Routines

      private void serializer_UnknownNode(object sender, XmlNodeEventArgs e) {
         parent.Log($"Unknown Node:{e.Name}\t{e.Text}");
      }

      private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e) {
         System.Xml.XmlAttribute attr = e.Attr;
         parent.Log($"Unknown Node:{attr.Name}\t{attr.Value}");
      }

      #endregion

   }
}
