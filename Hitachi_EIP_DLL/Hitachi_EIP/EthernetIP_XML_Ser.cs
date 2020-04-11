using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using Serialization;

namespace EIP_Lib {

   public partial class EIP {

      #region Send XML to printer using Serialization

      public bool SendXMLAsSerialization(string xml, bool AutoReflect = true) {
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
               SendXMLAsSerialization(Lab, AutoReflect);
            }
         } catch (Exception e) {
            success = false;
            LogIt(e.Message);
            // String passed is not XML, simply return defaultXmlClass
         } finally {
            // Release the error detection events
            serializer.UnknownNode -= new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute -= new XmlAttributeEventHandler(serializer_UnknownAttribute);
         }
         return success;
      }

      public void SendXMLAsSerialization(Lab Lab, bool AutoReflect = true) {
         UseAutomaticReflection = AutoReflect; // Speed up processing
         if (StartSession(true)) {
            if (ForwardOpen()) {
               try {
                  if (Lab.Message != null) {
                     SendMessage(Lab.Message[0]);
                  }

                  if (Lab.Printer != null) {
                     SendPrinterSettings(Lab.Printer[0]); // Must be done last
                  }

               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
               } catch (Exception e2) {
                  LogIt(e2.Message);
               }
            }
            ForwardClose();
         }
         EndSession();
         UseAutomaticReflection = false;
      }

      private void SendPrinterSettings(Printer p) {
         if (p.PrintHead != null) {
            SetAttribute(ccPS.Character_Orientation, p.PrintHead.Orientation);
         }
         if (p.ContinuousPrinting != null) {
            SetAttribute(ccPS.Repeat_Interval, p.ContinuousPrinting.RepeatInterval);
            SetAttribute(ccPS.Repeat_Count, p.ContinuousPrinting.PrintsPerTrigger);
         }
         if (p.TargetSensor != null) {
            SetAttribute(ccPS.Target_Sensor_Filter, p.TargetSensor.Filter);
            SetAttribute(ccPS.Target_Sensor_Filter_Value, p.TargetSensor.SetupValue);
            SetAttribute(ccPS.Target_Sensor_Timer, p.TargetSensor.Timer);
         }
         if (p.CharacterSize != null) {
            SetAttribute(ccPS.Character_Width, p.CharacterSize.Width);
            SetAttribute(ccPS.Character_Height, p.CharacterSize.Height);
         }
         if (p.PrintStartDelay != null) {
            SetAttribute(ccPS.Print_Start_Delay_Forward, p.PrintStartDelay.Forward);
            SetAttribute(ccPS.Print_Start_Delay_Reverse, p.PrintStartDelay.Reverse);
         }
         if (p.EncoderSettings != null) {
            SetAttribute(ccPS.High_Speed_Print, p.EncoderSettings.HighSpeedPrinting);
            SetAttribute(ccPS.Pulse_Rate_Division_Factor, p.EncoderSettings.Divisor);
            SetAttribute(ccPS.Product_Speed_Matching, p.EncoderSettings.ExternalEncoder);
         }
         if (p.InkStream != null) {
            SetAttribute(ccPS.Ink_Drop_Use, p.InkStream.InkDropUse);
            SetAttribute(ccPS.Ink_Drop_Charge_Rule, p.InkStream.ChargeRule);
         }
         if (p.Logos != null) {
            foreach (Logo l in p.Logos.Logo) {

            }
         }
         if (p.Substitution != null && p.Substitution.SubRule != null) {
            if (int.TryParse(p.Substitution.RuleNumber, out int ruleNumber)
               && int.TryParse(p.Substitution.StartYear, out int year)
               && p.Substitution.Delimiter.Length == 1) {
               // Substitution rules cannot be set with Auto Reflection on
               bool saveAR = UseAutomaticReflection;
               UseAutomaticReflection = false;

               SetAttribute(ccIDX.Substitution_Rule, ruleNumber);
               SetAttribute(ccSR.Start_Year, year);
               SendSubstitution(p.Substitution, p.Substitution.Delimiter);

               UseAutomaticReflection = saveAR;
            }
         }
      }

      private void SendSubstitution(Substitution s, string delimiter) {
         for (int i = 0; i < s.SubRule.Length; i++) {
            SubstitutionRule r = s.SubRule[i];
            if (Enum.TryParse(r.Type, true, out ccSR type)) {
               SetSubValues(type, r, delimiter);
            } else {
               LogIt($"Unknown substitution rule type =>{r.Type}<=");
            }
         }
      }

      private void SetSubValues(ccSR attribute, SubstitutionRule r, string delimeter) {
         if (int.TryParse(r.Base, out int b)) {
            Prop prop = EIP.AttrDict[ClassCode.Substitution_rules, (byte)attribute].Set;
            string[] s = r.Text.Split(delimeter[0]);
            for (int i = 0; i < s.Length; i++) {
               int n = b + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  byte[] data = FormatOutput(prop, n, 1, s[i]);
                  SetAttribute(ClassCode.Substitution_rules, (byte)attribute, data);
               }
            }
         }
      }

      private void SendMessage(Msg m) {
         // Set to only one item in printer
         DeleteAllButOne();

         if (m.Column != null) {
            AllocateRowsColumns(m);
         }
      }

      private void AllocateRowsColumns(Msg m) {
         int index = 0;
         bool hasDateOrCount = false; // Save some time if no need to look
         for (int c = 0; c < m.Column.Length; c++) {
            if (c > 0) {
               ServiceAttribute(ccPF.Add_Column);
            }
            // Should this be Column and not Item?
            SetAttribute(ccIDX.Item, c + 1);
            SetAttribute(ccPF.Line_Count, m.Column[c].Item.Length);
            if (m.Column[c].Item.Length > 1) {
               SetAttribute(ccIDX.Column, c + 1);
               SetAttribute(ccPF.Line_Spacing, m.Column[c].InterLineSpacing);
            }
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               SetAttribute(ccIDX.Item, index + 1);
               Item item = m.Column[c].Item[r];
               if (item.Font != null) {
                  SetAttribute(ccPF.Dot_Matrix, item.Font.DotMatrix);
                  SetAttribute(ccPF.InterCharacter_Space, item.Font.InterCharacterSpace);
                  SetAttribute(ccPF.Character_Bold, item.Font.IncreasedWidth);
               }
               SetAttribute(ccPF.Print_Character_String, item.Text);
               hasDateOrCount |= item.Date != null | item.Counter != null;
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
               int index = m.Column[c].Item[r].Location.Index + 1;
               if (item.Date != null) {
                  SetAttribute(ccIDX.Item, index);
                  GetAttribute(ccCal.Number_of_Calendar_Blocks, out item.Location.calCount);
                  GetAttribute(ccCal.First_Calendar_Block, out item.Location.calStart);
               }
               if (item.Counter != null) {
                  SetAttribute(ccIDX.Item, index);
                  GetAttribute(ccCount.Number_Of_Count_Blocks, out item.Location.countCount);
                  GetAttribute(ccCount.First_Count_Block, out item.Location.countStart);
               }
            }
         }

         // Restore previous AutoReflection to previous state
         UseAutomaticReflection = saveAR;
         for (int c = 0; c < m.Column.Length; c++) {
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               Item item = m.Column[c].Item[r];
               int index = m.Column[c].Item[r].Location.Index;
               if (item.Date != null) {
                  SetAttribute(ccIDX.Item, index + 1);
                  SendCalendar(item);
               }
               if (item.Counter != null) {
                  SetAttribute(ccIDX.Item, index + 1);
                  SendCount(item);
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
               SetAttribute(ccIDX.Substitution_Rule, date.SubstitutionRule);
               SetAttribute(ccIDX.Calendar_Block, calStart + date.Block - 1);

               // Process Offset
               Offset o = date.Offset;
               int n;
               if (o != null) {
                  if (int.TryParse(o.Year, out n) && n != 0) {
                     SetAttribute(ccCal.Offset_Year, n);
                  }
                  if (int.TryParse(o.Month, out n) && n != 0) {
                     SetAttribute(ccCal.Offset_Month, n);
                  }
                  if (int.TryParse(o.Day, out n) && n != 0) {
                     SetAttribute(ccCal.Offset_Day, n);
                  }
                  if (int.TryParse(o.Hour, out n) && n != 0) {
                     SetAttribute(ccCal.Offset_Hour, n);
                  }
                  if (int.TryParse(o.Minute, out n) && n != 0) {
                     SetAttribute(ccCal.Offset_Minute, n);
                  }
               }

               // Process Zero Suppress
               ZeroSuppress zs = date.ZeroSuppress;
               if (zs != null) {
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Year)) {
                     SetAttribute(ccCal.Zero_Suppress_Year, zs.Year);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Month)) {
                     SetAttribute(ccCal.Zero_Suppress_Month, zs.Month);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Day)) {
                     SetAttribute(ccCal.Zero_Suppress_Day, zs.Day);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Hour)) {
                     SetAttribute(ccCal.Zero_Suppress_Hour, zs.Hour);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Minute)) {
                     SetAttribute(ccCal.Zero_Suppress_Minute, zs.Minute);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.Week)) {
                     SetAttribute(ccCal.Zero_Suppress_Weeks, zs.Week);
                  }
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, zs.DayOfWeek)) {
                     SetAttribute(ccCal.Zero_Suppress_DayOfWeek, zs.DayOfWeek);
                  }
               }

               // Process Substitutions
               Substitute s = date.Substitute;
               if (s != null) {
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Year)) {
                     SetAttribute(ccCal.Substitute_Year, s.Year);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Month)) {
                     SetAttribute(ccCal.Substitute_Month, s.Month);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Day)) {
                     SetAttribute(ccCal.Substitute_Day, s.Day);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Hour)) {
                     SetAttribute(ccCal.Substitute_Hour, s.Hour);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Minute)) {
                     SetAttribute(ccCal.Substitute_Minute, s.Minute);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.Week)) {
                     SetAttribute(ccCal.Substitute_Weeks, s.Week);
                  }
                  if (!IsDefaultValue(fmtDD.EnableDisable, s.DayOfWeek)) {
                     SetAttribute(ccCal.Substitute_DayOfWeek, s.DayOfWeek);
                  }
               }
               // Process Shifts
               if (date.Shift != null) {
                  for (int j = 0; j < date.Shift.Length; j++) {
                     SetAttribute(ccIDX.Calendar_Block, j + 1);
                     SetAttribute(ccCal.Shift_Start_Hour, date.Shift[j].StartHour);
                     SetAttribute(ccCal.Shift_Start_Minute, date.Shift[j].StartMinute);
                     SetAttribute(ccCal.Shift_String_Value, date.Shift[j].ShiftCode);
                  }
               }
               if (date.TimeCount != null) {
                  TimeCount tc = date.TimeCount;
                  if (tc != null) {
                     SetAttribute(ccCal.Update_Interval_Value, tc.Interval);
                     SetAttribute(ccCal.Time_Count_Start_Value, tc.Start);
                     SetAttribute(ccCal.Time_Count_End_Value, tc.End);
                     SetAttribute(ccCal.Reset_Time_Value, tc.ResetTime);
                     SetAttribute(ccCal.Time_Count_Reset_Value, tc.ResetValue);
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
               SetAttribute(ccIDX.Count_Block, countStart + c.Block - 1);
               // Process Range
               Range r = c.Range;
               if (r != null) {
                  SetAttribute(ccCount.Count_Range_1, r.Range1);
                  SetAttribute(ccCount.Count_Range_2, r.Range2);
                  SetAttribute(ccCount.Jump_From, r.JumpFrom);
                  SetAttribute(ccCount.Jump_To, r.JumpTo);
               }

               // Process Count
               Count cc = c.Count;
               if (cc != null) {
                  SetAttribute(ccCount.Initial_Value, cc.InitialValue);
                  SetAttribute(ccCount.Increment_Value, cc.Increment);
                  SetAttribute(ccCount.Direction_Value, cc.Direction);
                  SetAttribute(ccCount.Zero_Suppression, cc.ZeroSuppression);
               }

               // Process Reset
               Reset rr = c.Reset;
               if (rr != null) {
                  SetAttribute(ccCount.Type_Of_Reset_Signal, rr.Type);
                  SetAttribute(ccCount.Reset_Value, rr.Value);
               }

               // Process Misc
               Misc m = c.Misc;
               if (m != null) {
                  SetAttribute(ccCount.Update_Unit_Unit, m.UpdateUnit);
                  SetAttribute(ccCount.Update_Unit_Halfway, m.UpdateIP);
                  SetAttribute(ccCount.External_Count, m.ExternalCount);
                  SetAttribute(ccCount.Count_Multiplier, m.Multiplier);
                  SetAttribute(ccCount.Count_Skip, m.SkipCount);
               }
            }
         }
      }

      #endregion

      #region Retrieve XML from printer using Serialization

      public string RetrieveXMLAsSerialization(bool UseIJPLibNames) {
         this.UseIJPLibNames = UseIJPLibNames;
         string xml = string.Empty;
         UseAutomaticReflection = false; // Never want Auto Reflection on
         if (StartSession(true)) {
            if (ForwardOpen()) {
               try {
                  Lab Label = new Lab() { Version = "Serialization-1" };
                  Label.Message = new Msg[1];
                  Label.Message[0] = RetrieveMessage();

                  Label.Printer = new Printer[1];
                  Label.Printer[0] = RetrievePrinterSettings();

                  Label.Printer[0].Substitution = RetrieveSubstitutions(Label.Message[0]);

                  XmlSerializer serializer = new XmlSerializer(typeof(Lab));
                  //TextWriter writer = new StreamWriter(FileName);
                  // Create our own namespaces for the output
                  XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                  ns.Add("", "");
                  using (MemoryStream ms = new MemoryStream()) {
                     serializer.Serialize(ms, Label, ns);
                     ms.Position = 0;
                     xml = new StreamReader(ms).ReadToEnd();
                  }
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
               } catch (Exception e2) {
                  LogIt(e2.Message);
               }
            }
            ForwardClose();
         }
         EndSession();
         UseAutomaticReflection = false;
         return xml;
      }

      private Printer RetrievePrinterSettings() {
         Printer p = new Printer() {
            Make = "Hitachi",
            Model = GetAttribute(ccUI.Model_Name),
            PrintHead = new PrintHead() {
               Orientation = GetAttribute(ccPS.Character_Orientation)
            },
            ContinuousPrinting = new ContinuousPrinting() {
               RepeatInterval = GetAttribute(ccPS.Repeat_Interval),
               PrintsPerTrigger = GetAttribute(ccPS.Repeat_Count)
            },
            TargetSensor = new TargetSensor() {
               Filter = GetAttribute(ccPS.Target_Sensor_Filter),
               SetupValue = GetAttribute(ccPS.Target_Sensor_Filter_Value),
               Timer = GetAttribute(ccPS.Target_Sensor_Timer)
            },
            CharacterSize = new CharacterSize() {
               Width = GetAttribute(ccPS.Character_Width),
               Height = GetAttribute(ccPS.Character_Height)
            },
            PrintStartDelay = new PrintStartDelay() {
               Forward = GetAttribute(ccPS.Print_Start_Delay_Forward),
               Reverse = GetAttribute(ccPS.Print_Start_Delay_Reverse)
            },
            EncoderSettings = new EncoderSettings() {
               HighSpeedPrinting = GetAttribute(ccPS.High_Speed_Print),
               Divisor = GetAttribute(ccPS.Pulse_Rate_Division_Factor),
               ExternalEncoder = GetAttribute(ccPS.Product_Speed_Matching)
            },
            InkStream = new InkStream() {
               InkDropUse = GetAttribute(ccPS.Ink_Drop_Use),
               ChargeRule = GetAttribute(ccPS.Ink_Drop_Charge_Rule)
            },
            Logos = RetrieveLogos(),
         };

         // Logos TBD
         return p;
      }

      private Substitution RetrieveSubstitutions(Msg m) {
         bool needYear = false;
         bool needMonth = false;
         bool needDay = false;
         bool needHour = false;
         bool needMinute = false;
         bool needWeek = false;
         bool needDayOfWeek = false;
         for (int c = 0; c < m.Column.Length; c++) {
            Column col = m.Column[c];
            for (int r = 0; r < col.Item.Length; r++) {
               Item item = col.Item[r];
               if (item.Date != null) {
                  for (int i = 0; i < item.Date.Length; i++) {
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
            StartYear = GetAttribute(ccSR.Start_Year),
            RuleNumber = "1",
            SubRule = sr.ToArray()
         };
         return substitution;
      }

      private void RetrieveSubstitution(List<SubstitutionRule> sr, ccSR rule) {
         AttrData attr = GetAttrData(rule);
         int n = (int)(attr.Get.Max - attr.Get.Min + 1);
         string[] subCode = new string[n];
         for (int i = 0; i < n; i++) {
            subCode[i] = GetAttribute(rule, i + (int)attr.Get.Min);
         }
         for (int i = 0; i < n; i += 10) {
            sr.Add(new SubstitutionRule() {
               Type = rule.ToString().Replace("_", ""),
               Base = (i + attr.Get.Min).ToString(),
               Text = string.Join("/", subCode, i, Math.Min(10, n - i)),
            });
         }
      }

      private Logos RetrieveLogos() {
         Logos logos = new Logos();
         logos.Logo = new Logo[1];
         logos.Logo[0] = new Logo() {
            Layout = "Fixed",
            DotMatrix = "18x24",
            Location = 0,
            FileName = "Square 5x8",
            RawData = "FF 81 81 99 99 81 81 FF",
         };
         return logos;
      }

      private Msg RetrieveMessage() {
         Msg m = new Msg() { Layout = GetAttribute(ccPF.Format_Type) };
         RetrieveRowsColummns(m);
         return m;
      }

      private void RetrieveRowsColummns(Msg m) {
         int index = 0;
         GetAttribute(ccPF.Number_Of_Columns, out int colCount);
         m.Column = new Column[colCount];
         for (int col = 0; col < colCount; col++) {
            SetAttribute(ccIDX.Column, col + 1);
            m.Column[col] = new Column() { InterLineSpacing = GetAttribute(ccPF.Line_Spacing) };
            GetAttribute(ccPF.Line_Count, out int LineCount);
            m.Column[col].Item = new Item[LineCount];
            for (int row = 0; row < LineCount; row++) {
               SetAttribute(ccIDX.Item, index + 1);
               Item item = new Item() {
                  Text = GetAttribute(ccPF.Print_Character_String),
                  Font = new FontDef() {
                     InterCharacterSpace = Convert.ToInt32(GetAttribute(ccPF.InterCharacter_Space)),
                     IncreasedWidth = Convert.ToInt32(GetAttribute(ccPF.Character_Bold)),
                     DotMatrix = GetAttribute(ccPF.Dot_Matrix),
                  },
                  BarCode = new BarCode(),
               };
               string barcode = GetAttribute(ccPF.Barcode_Type);
               if (barcode != GetDropDownNames((int)fmtDD.BarcodeType)[0]) {
                  item.BarCode.HumanReadableFont = GetAttribute(ccPF.Readable_Code);
                  item.BarCode.EANPrefix = GetAttribute(ccPF.Prefix_Code);
                  item.BarCode.DotMatrix = barcode;
               }
               item.Location = new Location() { Index = index, Row = row, Col = col };
               int[] mask = new int[1 + 8];
               ItemType itemType = GetItemType(item.Text, ref mask);
               GetAttribute(ccCal.Number_of_Calendar_Blocks, out item.Location.calCount);
               if (item.Location.calCount > 0) {
                  GetAttribute(ccCal.First_Calendar_Block, out item.Location.calStart);
                  RetrieveCalendarSettings(item, mask);
               }
               GetAttribute(ccCount.Number_Of_Count_Blocks, out item.Location.countCount);
               if (item.Location.countCount > 0) {
                  GetAttribute(ccCount.First_Count_Block, out item.Location.countStart);
                  RetrieveCountSettings(item);
               }
               m.Column[col].Item[row] = item;
               index++;
            }
         }
      }

      private void RetrieveCalendarSettings(Item item, int[] mask) {
         item.Date = new Date[item.Location.calCount];
         for (int i = 0; i < item.Location.calCount; i++) {
            SetAttribute(ccIDX.Calendar_Block, item.Location.calStart + i);
            // Where do you get Substitution rule number
            item.Date[i] = new Date() { Block = i + 1 };
            if ((mask[i] & DateOffset) > 0) {
               item.Date[i].SubstitutionRule = "1";
               item.Date[i].RuleName = "";
            }
            if ((mask[i] & DateOffset) > 0) {
               item.Date[i].Offset = new Offset() {
                  Year = GetAttribute(ccCal.Offset_Year),
                  Month = GetAttribute(ccCal.Offset_Month),
                  Day = GetAttribute(ccCal.Offset_Day),
                  Hour = GetAttribute(ccCal.Offset_Hour),
                  Minute = GetAttribute(ccCal.Offset_Minute)
               };
            }
            if ((mask[i] & DateSubZS) > 0) {
               item.Date[i].ZeroSuppress = new ZeroSuppress();
               string s;
               if ((mask[i] & (int)ba.Year) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = GetAttribute(ccCal.Zero_Suppress_Year)))
                     item.Date[i].ZeroSuppress.Year = s;
               if ((mask[i] & (int)ba.Month) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = GetAttribute(ccCal.Zero_Suppress_Month)))
                     item.Date[i].ZeroSuppress.Month = s;
               if ((mask[i] & (int)ba.Day) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = GetAttribute(ccCal.Zero_Suppress_Day)))
                     item.Date[i].ZeroSuppress.Day = s;
               if ((mask[i] & (int)ba.Hour) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = GetAttribute(ccCal.Zero_Suppress_Hour)))
                     item.Date[i].ZeroSuppress.Hour = s;
               if ((mask[i] & (int)ba.Minute) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = GetAttribute(ccCal.Zero_Suppress_Minute)))
                     item.Date[i].ZeroSuppress.Minute = s;
               if ((mask[i] & (int)ba.Week) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = GetAttribute(ccCal.Zero_Suppress_Weeks)))
                     item.Date[i].ZeroSuppress.Week = s;
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = GetAttribute(ccCal.Zero_Suppress_DayOfWeek)))
                     item.Date[i].ZeroSuppress.DayOfWeek = s;

               item.Date[i].Substitute = new Substitute();
               if ((mask[i] & (int)ba.Year) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = GetAttribute(ccCal.Substitute_Year)))
                     item.Date[i].Substitute.Year = s;
               if ((mask[i] & (int)ba.Month) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = GetAttribute(ccCal.Substitute_Month)))
                     item.Date[i].Substitute.Month = s;
               if ((mask[i] & (int)ba.Day) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = GetAttribute(ccCal.Substitute_Day)))
                     item.Date[i].Substitute.Day = s;
               if ((mask[i] & (int)ba.Hour) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = GetAttribute(ccCal.Substitute_Hour)))
                     item.Date[i].Substitute.Hour = s;
               if ((mask[i] & (int)ba.Minute) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = GetAttribute(ccCal.Substitute_Minute)))
                     item.Date[i].Substitute.Minute = s;
               if ((mask[i] & (int)ba.Week) > 0) // Printer reports these wrong
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = GetAttribute(ccCal.Substitute_DayOfWeek)))
                     item.Date[i].Substitute.Week = s;
               if ((mask[i] & (int)ba.DayOfWeek) > 0) // Printer reports these wrong
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = GetAttribute(ccCal.Substitute_Weeks)))
                     item.Date[i].Substitute.DayOfWeek = s;
            }
            if (item.Date[i].Shift == null && (mask[i] & (int)ba.Shift) > 0) {
               item.Date[i].Shift = RetrieveShifts();
            }
            if (item.Date[i].TimeCount == null && (mask[i] & (int)ba.TimeCount) > 0) {
               item.Date[i].TimeCount = RetrieveTimeCount();
            }
         }
      }

      private void RetrieveCountSettings(Item item) {
         item.Counter = new Counter[item.Location.countCount];
         for (int i = 0; i < item.Location.countCount; i++) {
            SetAttribute(ccIDX.Count_Block, item.Location.calStart + i);
            item.Counter[i] = new Counter() { Block = i + 1 };
            item.Counter[i].Range = new Range() {
               Range1 = GetAttribute(ccCount.Count_Range_1),
               Range2 = GetAttribute(ccCount.Count_Range_2),
               JumpFrom = GetAttribute(ccCount.Jump_From),
               JumpTo = GetAttribute(ccCount.Jump_To),
            };
            item.Counter[i].Count = new Count() {
               InitialValue = GetAttribute(ccCount.Initial_Value),
               Increment = GetAttribute(ccCount.Increment_Value),
               Direction = GetAttribute(ccCount.Direction_Value),
               ZeroSuppression = GetAttribute(ccCount.Zero_Suppression),
            };
            item.Counter[i].Reset = new Reset() {
               Type = GetAttribute(ccCount.Type_Of_Reset_Signal),
               Value = GetAttribute(ccCount.Reset_Value),
            };
            item.Counter[i].Misc = new Misc() {
               UpdateIP = GetAttribute(ccCount.Update_Unit_Halfway),
               UpdateUnit = GetAttribute(ccCount.Update_Unit_Unit),
               ExternalCount = GetAttribute(ccCount.External_Count),
               Multiplier = GetAttribute(ccCount.Count_Multiplier),
               SkipCount = GetAttribute(ccCount.Count_Skip),
            };
         }
      }

      private Shift[] RetrieveShifts() {
         List<Shift> s = new List<Shift>();
         string endHour;
         string endMinute;
         int shift = 1;
         do {
            SetAttribute(ccIDX.Item, shift);
            s.Add(new Shift() {
               ShiftNumber = shift,
               StartHour = GetAttribute(ccCal.Shift_Start_Hour),
               StartMinute = GetAttribute(ccCal.Shift_Start_Minute),
               EndHour = endHour = GetAttribute(ccCal.Shift_End_Hour),
               EndMinute = endMinute = GetAttribute(ccCal.Shift_End_Minute),
               ShiftCode = GetAttribute(ccCal.Shift_String_Value),
            });
            shift++;
         } while (endHour != "23" || endMinute != "59");
         return s.ToArray();
      }

      private TimeCount RetrieveTimeCount() {
         TimeCount TimeCount = new TimeCount() {
            Interval = GetAttribute(ccCal.Update_Interval_Value),
            Start = GetAttribute(ccCal.Time_Count_Start_Value),
            End = GetAttribute(ccCal.Time_Count_End_Value),
            ResetTime = GetAttribute(ccCal.Reset_Time_Value),
            ResetValue = GetAttribute(ccCal.Time_Count_Reset_Value),
         };
         return TimeCount;
      }

      #endregion

      #region Service Routines

      private void serializer_UnknownNode(object sender, XmlNodeEventArgs e) {
         LogIt($"Unknown Node:{e.Name}\t{e.Text}");
      }

      private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e) {
         System.Xml.XmlAttribute attr = e.Attr;
         LogIt($"Unknown Node:{attr.Name}\t{attr.Value}");
      }

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
         val = Array.FindIndex(DropDowns[(int)fmt], x => x.ToLower().Contains(s));
         if (val < 0) {
            val = Array.FindIndex(DropDownsIJPLib[(int)fmt], x => x.ToLower().Contains(s));
         }
         return val == 0;
      }

      public string[] GetDropDownNames(int n) {
         if (UseIJPLibNames) {
            return DropDownsIJPLib[n];
         } else {
            return DropDowns[n];
         }
      }

      #endregion

   }
}
