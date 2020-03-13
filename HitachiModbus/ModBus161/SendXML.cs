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

      // Serialize the XML to a Lab and send it to the printer
      public bool SendXML(string xml) {
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
               SendXML(Lab);
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

      // Send a Serialized Lab to the printer
      public void SendXML(Lab Lab) {
         try {
            if (Lab.Printer != null) {
               for (int i = 0; i < Lab.Printer.Length; i++) {
                  Printer ptr = Lab.Printer[i];
                  int n = 0;
                  if (!string.IsNullOrEmpty(ptr.Nozzle)) {
                     if (int.TryParse(ptr.Nozzle, out n)) {
                        n = Math.Max(0, n - 1);
                     }
                  }
                  if (n > 0 && !p.TwinNozzle) {
                     continue;
                  }
                  p.Nozzle = n;
                  parent.Log($" \n// Sending Logos for nozzle {n + 1}\n ");
                  if (Lab.Printer[i].Logos != null) {
                     foreach (Logo l in ptr.Logos.Logo) {
                        switch (l.Layout) {
                           case "Free":
                              SendFreeLogo(l);
                              break;
                           case "Fixed":
                              SendFixedLogo(l);
                              break;
                        }
                     }
                  }
                  if (n > 0) // Load substitutions associated with nozzle 1 only
                     continue;
                  parent.Log($" \n// Sending Substitutions for nozzle {n + 1}\n ");
                  SendSubstitutionRules(ptr);
               }
            }

            // Send message settings
            if (Lab.Message != null) {
               for (int i = 0; i < Lab.Message.Length; i++) {
                  if (Lab.Message[i] != null) {
                     int n = 0;
                     if (!string.IsNullOrEmpty(Lab.Message[i].Nozzle)) {
                        if (int.TryParse(Lab.Message[i].Nozzle, out n)) {
                           n = Math.Max(0, n - 1);
                        }
                     }
                     if (n > 0 && !p.TwinNozzle) {
                        continue;
                     }
                     p.Nozzle = n;
                     parent.Log($" \n// Sending Message for nozzle {n + 1}\n ");
                     SendMessage(Lab.Message[i]);
                  }
               }
            }

            // Send printer settings
            if (Lab.Printer != null) {
               for (int i = 0; i < Lab.Printer.Length; i++) {
                  if (Lab.Printer[i] != null) {
                     int n = 0;
                     if (!string.IsNullOrEmpty(Lab.Printer[i].Nozzle)) {
                        if (int.TryParse(Lab.Printer[i].Nozzle, out n)) {
                           n = Math.Max(0, n - 1);
                        }
                     }
                     if (n > 0 && !p.TwinNozzle) {
                        continue;
                     }
                     p.Nozzle = n;
                     parent.Log($" \n// Sending Printer Settings for nozzle {n + 1}\n ");
                     SendPrinterSettings(Lab.Printer[i]); // Must be done last
                  }
               }
            }
         } catch (Exception e2) {
            parent.Log(e2.Message);
         }
      }

      #endregion

      #region Sent Message to printer

      // Send the message portion of the Lab
      private void SendMessage(Msg m) {
         // Set to only one item in printer
         p.DeleteAllButOne();

         if (m.Column != null) {
            AllocateRowsColumns(m);
         }
      }

      // Use the column/item structure of a Lab to allocate items in the printer
      private void AllocateRowsColumns(Msg m) {
         int index = 0;
         bool hasDateOrCount = false; // Save some time if no need to look
         int charPosition = 0;
         for (int c = 0; c < m.Column.Length; c++) {
            if (c > 0) {
               parent.Log($" \n// Add column {c + 1}\n ");
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               p.SetAttribute(ccPF.Add_Column, c + 1);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            }

            parent.Log($" \n// Set column {c + 1} to {m.Column[c].Item.Length} items\n ");
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccPF.Column, c + 1);
            p.SetAttribute(ccPF.Line, m.Column[c].Item.Length);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

            if (m.Column[c].Item.Length > 1) {
               parent.Log($" \n// Set ILS for items {index + 1} to {index + m.Column[c].Item.Length}\n ");
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               for (int j = 0; j < m.Column[c].Item.Length; j++) {
                  p.SetAttribute(ccPF.Line_Spacing, index + j, m.Column[c].InterLineSpacing);
               }
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            }

            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               parent.Log($" \n// Fill in item {index + 1}\n ");
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               Item item = m.Column[c].Item[r];
               if (item.Font != null) {
                  p.SetAttribute(ccPF.Dot_Matrix, index, item.Font.DotMatrix);
                  p.SetAttribute(ccPF.InterCharacter_Space, index, item.Font.InterCharacterSpace);
                  p.SetAttribute(ccPF.Character_Bold, index, item.Font.IncreasedWidth);
               }
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

               string s = p.HandleBraces(item.Text);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               p.SetAttribute(ccPC.Characters_per_Item, index, s.Length);
               p.SetAttribute(ccPC.Print_Character_String, charPosition, s);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
               charPosition += s.Length;
               hasDateOrCount |= item.Date != null | item.Counter != null | item.Shift != null | item.TimeCount != null;
               m.Column[c].Item[r].Location = new Location() { Index = index++, Row = r, Col = c };
            }
         }
         // Process calendar and count if needed
         if (hasDateOrCount) {
            SendDateCount(m);
         }
      }

      // Send the Calendar and Counter settings
      private void SendDateCount(Msg m) {
         // Get calendar and count blocks assigned by the printer
         parent.Log($" \n// Get number of Calendar and Count blocks used\n ");
         for (int c = 0; c < m.Column.Length; c++) {
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               Item item = m.Column[c].Item[r];
               int index = m.Column[c].Item[r].Location.Index;
               if (item.Date != null) {
                  item.Location.calCount = p.GetDecAttribute(ccPF.Number_of_Calendar_Blocks, index);
                  item.Location.calStart = p.GetDecAttribute(ccPF.First_Calendar_Block, index);
               }
               if (item.Counter != null) {
                  item.Location.countCount = p.GetDecAttribute(ccPF.Number_Of_Count_Blocks, index);
                  item.Location.countStart = p.GetDecAttribute(ccPF.First_Count_Block, index);
               }
            }
         }

         // Restore previous AutoReflection to previous state
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
            if (date.Block <= calCount && int.TryParse(date.SubstitutionRule, out int ruleNumber) && ruleNumber >  0) {

               parent.Log($" \n// Load settings for Substitution rule {1}\n ");
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               p.SetAttribute(ccIDX.Substitution_Rule, ruleNumber); // date.SubstitutionRule
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

               int index = calStart + date.Block - 2; // Cal start and date.Block are both 1-origin
               parent.Log($" \n// Set up calendar {index + 1}\n ");
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
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
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
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

               parent.Log($" \n// Set up count {index + 1}\n ");
               // Process Range
               Range r = c.Range;
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               if (r != null) {
                  if (r.Range1 != null)
                     p.SetAttribute(ccCount.Count_Range_1, index, r.Range1);
                  if (r.Range2 != null)
                     p.SetAttribute(ccCount.Count_Range_2, index, r.Range2);
                  if (r.JumpFrom != null)
                     p.SetAttribute(ccCount.Jump_From, index, r.JumpFrom);
                  if (r.JumpTo != null)
                     p.SetAttribute(ccCount.Jump_To, index, r.JumpTo);
               }
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

               // Process Count
               Count cc = c.Count;
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               if (cc != null) {
                  if (cc.InitialValue != null)
                     p.SetAttribute(ccCount.Initial_Value, index, cc.InitialValue);
                  if (cc.Increment != null)
                     p.SetAttribute(ccCount.Increment_Value, index, cc.Increment);
                  if (cc.Direction != null)
                     p.SetAttribute(ccCount.Direction_Value, index, cc.Direction);
                  if (cc.ZeroSuppression != null)
                     p.SetAttribute(ccCount.Zero_Suppression, index, cc.ZeroSuppression);
               }
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

               // Process Reset
               Reset rr = c.Reset;
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               if (rr != null) {
                  if (rr.Type != null)
                     p.SetAttribute(ccCount.Type_Of_Reset_Signal, index, rr.Type);
                  if (rr.Value != null)
                     p.SetAttribute(ccCount.Reset_Value, index, rr.Value);
               }
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

               // Process Misc
               Misc m = c.Misc;
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               if (m != null) {
                  if (m.UpdateUnit != null)
                     p.SetAttribute(ccCount.Update_Unit_Unit, index, m.UpdateUnit);
                  if (m.UpdateIP != null)
                     p.SetAttribute(ccCount.Update_Unit_Halfway, index, m.UpdateIP);
                  if (m.ExternalCount != null)
                     p.SetAttribute(ccCount.External_Count, index, m.ExternalCount);
                  if (m.Multiplier != null)
                     p.SetAttribute(ccCount.Count_Multiplier, index, m.Multiplier);
                  if (m.SkipCount != null)
                     p.SetAttribute(ccCount.Count_Skip, index, m.SkipCount);
               }
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            }
         }
      }

      private void SendShift(Item item) {

         parent.Log($" \n// Set up shifts\n ");
         // Process Shift
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         for (int j = 0; j < item.Shift.Length; j++) {
            p.SetAttribute(ccSR.Shift_Start_Hour, j, item.Shift[j].StartHour);
            p.SetAttribute(ccSR.Shift_Start_Minute, j, item.Shift[j].StartMinute);
            p.SetAttribute(ccSR.Shift_End_Hour, j, item.Shift[j].EndHour);
            p.SetAttribute(ccSR.Shift_End_Minute, j, item.Shift[j].EndMinute);
            p.SetAttribute(ccSR.Shift_String_Value, j, item.Shift[j].ShiftCode);
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
      }

      private void SendTimeCount(Item item) {

         parent.Log($" \n// Set up Time Count\n ");
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

         parent.Log($" \n// Send printer settings\n ");
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
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
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
      }

      private void SendFreeLogo(Logo l) {
         if (int.TryParse(l.Location, out int loc)
            && int.TryParse(l.Height, out int height) && height <= 32
            && int.TryParse(l.Width, out int width) && width <= 320
            && l.RawData.Length > 0) {

            byte[] rawdata = p.string_to_byte(l.RawData);      // Get source raw data
            if (p.SendFreeLogo(width, height, loc, rawdata)) {
               parent.Log($" \n// Set {width}x{height} Free Logo to  location {loc}\n ");
            } else {
               parent.Log($" \n// Failed to set {width}x{height} Free Logo to  location {loc}\n ");
            }
         }
      }

      private void SendFixedLogo(Logo l) {
         int[] logoLen = new int[] { 0, 8, 8, 8, 16, 16, 32, 32, 72, 128, 32, 5, 5, 7, 200, 288 };
         // Load the logo into the pattern area
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccIDX.User_Pattern_Size, l.DotMatrix);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         if (int.TryParse(l.Location, out int loc) && l.RawData.Length > 0) {

            parent.Log($" \n// Set {l.DotMatrix} Fixed Logo to  location {loc}\n ");

            // Write the registration bit
            int regLoc = loc / 16;
            int regBit = 15 - (loc % 16);
            AttrData attr = p.GetAttrData(ccUP.User_Pattern_Fixed_Registration);
            int regMask = p.GetDecAttribute(ccUP.User_Pattern_Fixed_Registration, regLoc);
            regMask |= 1 << regBit;

            // Write the pattern
            int n = p.ToDropdownValue(p.GetAttrData(ccIDX.User_Pattern_Size).Data, l.DotMatrix);
            byte[] data = new byte[logoLen[n]];
            byte[] rawdata = p.string_to_byte(l.RawData);
            for (int i = 0; i < Math.Min(data.Length, rawdata.Length); i++) {
               data[i] = rawdata[i];
            }
            // Write the pattern data
            attr = p.GetAttrData(ccUP.User_Pattern_Fixed_Data);
            int addr = attr.Val;
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccUP.User_Pattern_Fixed_Data, loc * (logoLen[n] / 2), data);
            p.SetAttribute(ccUP.User_Pattern_Fixed_Registration, regLoc, regMask);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }
      }

      // Send substitution rules
      private void SendSubstitutionRules(Printer ptr) {
         if (ptr.Substitution != null && ptr.Substitution.SubRule != null) {
            if (int.TryParse(ptr.Substitution.RuleNumber, out int ruleNumber)
               && int.TryParse(ptr.Substitution.StartYear, out int year)
               && ptr.Substitution.Delimiter.Length == 1) {
               // Force rule to be loaded
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               p.SetAttribute(ccIDX.Substitution_Rule, ruleNumber);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
               p.SetAttribute(ccSR.Start_Year, year);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               SendSubstitution(ptr.Substitution, ptr.Substitution.Delimiter);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
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
