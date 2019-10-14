using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace EIP_Lib {

   [XmlRoot("Label", IsNullable = false)]
   public class Lab {
      [XmlAttribute]
      public string Version;   // Keep track of version to allow for changes
      public Printer Printer;  // Information that pertains to the printer
      public Msg Message;      // Information that pertains to the message
   }

   #region Message Classes

   public class Msg {
      [XmlAttribute]
      public string Layout;
      [XmlElement("Column")]
      public Column[] Column;
   }

   public class Column {
      [XmlAttribute]
      public string InterLineSpacing;
      [XmlElement("Item")]
      public Item[] Item;
   }

   public class Item {
      [XmlAttribute]
      public string Type;
      public FontDef Font;
      public BarCode BarCode;
      [XmlElement("Date")]
      public Date[] Date;
      [XmlElement("Counter")]
      public Counter[] Counter;
      public string Text;

      [XmlIgnore]
      public Location Location;
   }

   public class Location {
      public int Row;     // 0-Origin
      public int Col;     // 0-Origin
      public int Index;   // 0-Origin
      public int X;       // 0-Origin
      public int Y;       // 0-Origin
      public int calStart = 0;
      public int calCount = 0;
      public int countStart = 0;
      public int countCount = 0;
   }

   public class FontDef {
      [XmlAttribute]
      public string IncreasedWidth;
      [XmlAttribute]
      public string InterCharacterSpace;
      [XmlAttribute]
      public string Face;
   }

   public class BarCode {

   }

   public class Counter {
      [XmlAttribute]
      public int Block;

      public Range Range;
      public Count Count;
      public Reset Reset;
      public Misc Misc;
   }

   public class Range {
      [XmlAttribute]
      public string Range1;
      [XmlAttribute]
      public string Range2;
      [XmlAttribute]
      public string JumpFrom;
      [XmlAttribute]
      public string JumpTo;
   }

   public class Count {
      [XmlAttribute]
      public string InitialValue;
      [XmlAttribute]
      public string Increment;
      [XmlAttribute]
      public string Direction;
      [XmlAttribute]
      public string ZeroSuppression;
   }

   public class Reset {
      [XmlAttribute]
      public string Type;
      [XmlAttribute]
      public string Value;
   }

   public class Misc {
      [XmlAttribute]
      public string UpdateUnit;
      [XmlAttribute]
      public string UpdateIP;
      [XmlAttribute]
      public string Multiplier;
      [XmlAttribute]
      public string ExternalCount;
      [XmlAttribute]
      public string CountSkip;
   }

   public class Date {
      [XmlAttribute]
      public int Block;
      [XmlAttribute]
      public string SubstitutionRule;
      [XmlAttribute]
      public string RuleName;

      public Offset Offset;
      public ZeroSuppress ZeroSuppress;
      public Substitute Substitute;
      public TimeCount TimeCount;
      [XmlElement("Shift")]
      public Shift[] Shift;
   }

   public class Offset {
      [XmlAttribute]
      public string Year;
      [XmlAttribute]
      public string Month;
      [XmlAttribute]
      public string Day;
      [XmlAttribute]
      public string Hour;
      [XmlAttribute]
      public string Minute;
   }

   public class ZeroSuppress {
      [XmlAttribute]
      public string Year;
      [XmlAttribute]
      public string Month;
      [XmlAttribute]
      public string Day;
      [XmlAttribute]
      public string Hour;
      [XmlAttribute]
      public string Minute;
      [XmlAttribute]
      public string Week;
      [XmlAttribute]
      public string DayOfWeek;
   }

   public class Substitute {
      [XmlAttribute]
      public string Year;
      [XmlAttribute]
      public string Month;
      [XmlAttribute]
      public string Day;
      [XmlAttribute]
      public string Hour;
      [XmlAttribute]
      public string Minute;
      [XmlAttribute]
      public string Week;
      [XmlAttribute]
      public string DayOfWeek;
   }

   public class Shift {
      [XmlAttribute]
      public int ShiftNumber;
      [XmlAttribute]
      public string StartHour;
      [XmlAttribute]
      public string StartMinute;
      [XmlAttribute]
      public string EndHour;
      [XmlAttribute]
      public string EndMinute;
      [XmlAttribute]
      public string ShiftCode;
   }

   public class TimeCount {
      [XmlAttribute]
      public string Interval;
      [XmlAttribute]
      public string Start;
      [XmlAttribute]
      public string End;
      [XmlAttribute]
      public string ResetTime;
      [XmlAttribute]
      public string ResetValue;
   }

   #endregion

   #region Printer Classes

   public class Printer {
      [XmlAttribute]
      public string Make;
      [XmlAttribute]
      public string Model;
      public PrintHead PrintHead;
      public ContinuousPrinting ContinuousPrinting;
      public TargetSensor TargetSensor;
      public CharacterSize CharacterSize;
      public PrintStartDelay PrintStartDelay;
      public EncoderSettings EncoderSettings;
      public InkStream InkStream;
      public Logos Logos;
   }

   public class PrintHead {
      [XmlAttribute]
      public string Orientation;
   }

   public class ContinuousPrinting {
      [XmlAttribute]
      public string RepeatInterval;
      [XmlAttribute]
      public string PrintsPerTrigger;
   }

   public class TargetSensor {
      [XmlAttribute]
      public string Filter;
      [XmlAttribute]
      public string SetupValue;
      [XmlAttribute]
      public string Timer;
   }

   public class CharacterSize {
      [XmlAttribute]
      public string Height;
      [XmlAttribute]
      public string Width;
   }

   public class PrintStartDelay {
      [XmlAttribute]
      public string Forward;
      [XmlAttribute]
      public string Reverse;
   }

   public class EncoderSettings {
      [XmlAttribute]
      public string HighSpeedPrinting;
      [XmlAttribute]
      public string Divisor;
      [XmlAttribute]
      public string ExternalEncoder;
   }

   public class InkStream {
      [XmlAttribute]
      public string InkDropUse;
      [XmlAttribute]
      public string ChargeRule;
   }

   public class Logos {
      [XmlAttribute]
      public string Folder;
      [XmlElement("Logo")]
      public Logo[] Logo;
   }

   public class Logo {
      [XmlAttribute]
      public string Layout;
      [XmlAttribute]
      public string Location;
      [XmlAttribute]
      public string RawData;
      [XmlAttribute]
      public string FileName;
   }
   #endregion

   public partial class EIP {

      #region Send XML to printer using Serialization

      public void SendXMLAsSerialization(string xml, bool AutoReflect = true) {
         Lab Lab;
         XmlSerializer serializer = new XmlSerializer(typeof(Lab));
         try {
            // Arm the Serializer
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            using (TextReader reader = new StringReader(xml)) {
               // Deserialize the file contents
               Lab = (Lab)serializer.Deserialize(reader);
               SendLabelToPrinter(Lab, AutoReflect);
            }
         } catch (Exception e) {
            LogIt(e.Message);
            // String passed is not XML, simply return defaultXmlClass
         } finally {
            // Release the error detection events
            serializer.UnknownNode -= new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute -= new XmlAttributeEventHandler(serializer_UnknownAttribute);
         }
      }

      public bool SendFileAsSerialization(string filename, bool AutoReflect = true) {
         bool success = true;
         Lab Lab;
         XmlSerializer serializer = new XmlSerializer(typeof(Lab));
         try {
            // Arm the Serializer
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            using (FileStream reader = new FileStream(filename, FileMode.Open)) {
               // Deserialize the file contents
               Lab = (Lab)serializer.Deserialize(reader);
               SendLabelToPrinter(Lab, AutoReflect);
            }
         } catch (Exception e) {
            LogIt(e.Message);
            success = false;
         } finally {
            // Release the error detection events
            serializer.UnknownNode -= new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute -= new XmlAttributeEventHandler(serializer_UnknownAttribute);
         }
         return success;
      }

      private void SendLabelToPrinter(Lab Lab, bool AutoReflect) {
         UseAutomaticReflection = AutoReflect; // Speed up processing
         if (StartSession(true)) {
            if (ForwardOpen()) {
               try {

                  DeleteAllButOne();  // Delete all but one item in printer

                  if (Lab.Message != null) {
                     SendMessage(Lab.Message);
                  }

                  if (Lab.Printer != null) {
                     SendPrinterSettings(Lab.Printer); // Must be done last
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
      }

      private void SendMessage(Msg m) {
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
                  SetAttribute(ccPF.InterCharacter_Space, item.Font.InterCharacterSpace);
                  SetAttribute(ccPF.Character_Bold, item.Font.IncreasedWidth);
                  SetAttribute(ccPF.Dot_Matrix, item.Font.Face);
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
               SetAttribute(ccIDX.Calendar_Block, calStart + date.Block - 1);

               // Process Offset
               Offset o = date.Offset;
               if (o != null) {
                  SetAttribute(ccCal.Offset_Year, o.Year);
                  SetAttribute(ccCal.Offset_Month, o.Month);
                  SetAttribute(ccCal.Offset_Day, o.Day);
                  SetAttribute(ccCal.Offset_Hour, o.Hour);
                  SetAttribute(ccCal.Offset_Minute, o.Minute);
               }

               // Process Zero Suppress
               ZeroSuppress zs = date.ZeroSuppress;
               if (zs != null) {
                  SetAttribute(ccCal.Zero_Suppress_Year, zs.Year);
                  SetAttribute(ccCal.Zero_Suppress_Month, zs.Month);
                  SetAttribute(ccCal.Zero_Suppress_Day, zs.Day);
                  SetAttribute(ccCal.Zero_Suppress_Hour, zs.Hour);
                  SetAttribute(ccCal.Zero_Suppress_Minute, zs.Minute);
                  SetAttribute(ccCal.Zero_Suppress_Weeks, zs.Week);
                  SetAttribute(ccCal.Zero_Suppress_Day_Of_Week, zs.DayOfWeek);
               }

               // Process Substitutions
               Substitute s = date.Substitute;
               if (s != null) {
                  SetAttribute(ccCal.Substitute_Year, s.Year);
                  SetAttribute(ccCal.Substitute_Month, s.Month);
                  SetAttribute(ccCal.Substitute_Day, s.Day);
                  SetAttribute(ccCal.Substitute_Hour, s.Hour);
                  SetAttribute(ccCal.Substitute_Minute, s.Minute);
                  SetAttribute(ccCal.Substitute_Weeks, s.Week);
                  SetAttribute(ccCal.Substitute_Day_Of_Week, s.DayOfWeek);
               }

               // Process Time Count
               TimeCount tc = date.TimeCount;
               if (tc != null) {
                  SetAttribute(ccCal.Time_Count_Start_Value, tc.Start);
                  SetAttribute(ccCal.Time_Count_End_Value, tc.End);
                  SetAttribute(ccCal.Time_Count_Reset_Value, tc.ResetValue);
                  SetAttribute(ccCal.Reset_Time_Value, tc.ResetTime);
                  SetAttribute(ccCal.Update_Interval_Value, tc.Interval);
               }

               // Process Shift
               if (date.Shift != null) {
                  for (int j = 0; j < date.Shift.Length; j++) {
                     SetAttribute(ccIDX.Calendar_Block, date.Shift[j].ShiftNumber);
                     SetAttribute(ccCal.Shift_Start_Hour, date.Shift[j].StartHour);
                     SetAttribute(ccCal.Shift_Start_Minute, date.Shift[j].StartMinute);
                     //SetAttribute(ccCal.Shift_End_Hour, date.Shift[j].EndHour);     // Get Only
                     //SetAttribute(ccCal.Shift_End_Minute, date.Shift[j].EndMinute); // Get only
                     SetAttribute(ccCal.Shift_String_Value, date.Shift[j].ShiftCode);
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
                  SetAttribute(ccCount.Update_Unit_Halfway, m.UpdateIP);
                  SetAttribute(ccCount.Update_Unit_Unit, m.UpdateUnit);
                  SetAttribute(ccCount.External_Count, m.ExternalCount);
                  SetAttribute(ccCount.Count_Multiplier, m.Multiplier);
                  SetAttribute(ccCount.Count_Skip, m.CountSkip);
               }
            }
         }
      }

      #endregion

      #region Retrieve XML from printer using Serialization

      public string RetrieveXMLAsSerialization(bool AutoReflect = false) {
         string xml = string.Empty;
         UseAutomaticReflection = AutoReflect; // Speed up processing
         if (StartSession(true)) {
            if (ForwardOpen()) {
               try {
                  Lab Label = new Lab() { Version = "Serialization-1" };

                  Label.Printer = RetrievePrinterSettings();

                  Label.Message = RetrieveMessage();

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
         };
         // Logos TBD
         return p;
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
                  Type = ItemType.Text.ToString(),
                  Text = GetAttribute(ccPF.Print_Character_String),
                  Font = new FontDef() {
                     InterCharacterSpace = GetAttribute(ccPF.InterCharacter_Space),
                     IncreasedWidth = GetAttribute(ccPF.Character_Bold),
                     Face = GetAttribute(ccPF.Dot_Matrix),
                  },
                  BarCode = new BarCode(),
               };
               item.Location = new Location() { Index = index, Row = row, Col = col };
               GetAttribute(ccCal.Number_of_Calendar_Blocks, out item.Location.calCount);
               if (item.Location.calCount > 0) {
                  item.Type = ItemType.Date.ToString();
                  GetAttribute(ccCal.First_Calendar_Block, out item.Location.calStart);
                  RetrieveCalendarSettings(item);
               }
               GetAttribute(ccCount.Number_Of_Count_Blocks, out item.Location.countCount);
               if (item.Location.countCount > 0) {
                  item.Type = ItemType.Counter.ToString();
                  GetAttribute(ccCount.First_Count_Block, out item.Location.countStart);
                  RetrieveCountSettings(item);
               }
               m.Column[col].Item[row] = item;
               index++;
            }
         }
      }

      private void RetrieveCalendarSettings(Item item) {
         int[] mask = new int[1 + item.Location.calCount];
         ItemType itemType = GetItemType(item.Text, ref mask);
         item.Date = new Date[item.Location.calCount];
         for (int i = 0; i < item.Location.calCount; i++) {
            SetAttribute(ccIDX.Calendar_Block, item.Location.calStart + i);
            item.Date[i] = new Date() { Block = i + 1 };
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
               if ((mask[i] & (int)ba.Year) > 0)
                  item.Date[i].ZeroSuppress.Year = GetAttribute(ccCal.Zero_Suppress_Year);
               if ((mask[i] & (int)ba.Month) > 0)
                  item.Date[i].ZeroSuppress.Month = GetAttribute(ccCal.Zero_Suppress_Month);
               if ((mask[i] & (int)ba.Day) > 0)
                  item.Date[i].ZeroSuppress.Day = GetAttribute(ccCal.Zero_Suppress_Day);
               if ((mask[i] & (int)ba.Hour) > 0)
                  item.Date[i].ZeroSuppress.Hour = GetAttribute(ccCal.Zero_Suppress_Hour);
               if ((mask[i] & (int)ba.Minute) > 0)
                  item.Date[i].ZeroSuppress.Minute = GetAttribute(ccCal.Zero_Suppress_Minute);
               if ((mask[i] & (int)ba.Week) > 0)
                  item.Date[i].ZeroSuppress.Week = GetAttribute(ccCal.Zero_Suppress_Weeks);
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  item.Date[i].ZeroSuppress.DayOfWeek = GetAttribute(ccCal.Zero_Suppress_Day_Of_Week);

               item.Date[i].Substitute = new Substitute();
               if ((mask[i] & (int)ba.Year) > 0)
                  item.Date[i].Substitute.Year = GetAttribute(ccCal.Substitute_Year);
               if ((mask[i] & (int)ba.Month) > 0)
                  item.Date[i].Substitute.Month = GetAttribute(ccCal.Substitute_Month);
               if ((mask[i] & (int)ba.Day) > 0)
                  item.Date[i].Substitute.Day = GetAttribute(ccCal.Substitute_Day);
               if ((mask[i] & (int)ba.Hour) > 0)
                  item.Date[i].Substitute.Hour = GetAttribute(ccCal.Substitute_Hour);
               if ((mask[i] & (int)ba.Minute) > 0)
                  item.Date[i].Substitute.Minute = GetAttribute(ccCal.Substitute_Minute);
               if ((mask[i] & (int)ba.Week) > 0)
                  item.Date[i].Substitute.Week = GetAttribute(ccCal.Substitute_Weeks);
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  item.Date[i].Substitute.DayOfWeek = GetAttribute(ccCal.Substitute_Day_Of_Week);
            }
            if ((mask[i] & (int)ba.Shift) > 0) {
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
                     EndHour = (endHour = GetAttribute(ccCal.Shift_End_Hour)),
                     EndMinute = (endMinute = GetAttribute(ccCal.Shift_End_Minute)),
                     ShiftCode = GetAttribute(ccCal.Shift_String_Value),
                  });
                  shift++;
               } while (endHour != "23" || endMinute != "59");
               item.Date[i].Shift = s.ToArray();
            }

            if ((mask[i] & (int)ba.TimeCount) > 0) {
               item.Date[i].TimeCount = new TimeCount() {
                  Interval = GetAttribute(ccCal.Update_Interval_Value),
                  Start = GetAttribute(ccCal.Time_Count_Start_Value),
                  End = GetAttribute(ccCal.Time_Count_End_Value),
                  ResetTime = GetAttribute(ccCal.Reset_Time_Value),
                  ResetValue = GetAttribute(ccCal.Time_Count_Reset_Value),
               };
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
               CountSkip = GetAttribute(ccCount.Count_Skip),
            };
         }
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

      #endregion

   }
}
