using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Serialization {

   #region Enumerations

   // Previous implementation used true/false
   public enum ZS {
      None = 0,
      [XmlEnum(Name = "false")]
      prevFalse = 0,
      Space = 1,
      [XmlEnum(Name = "true")]
      prevTrue = 1,
      Character = 2,
   }

   // Previous implementation used true/false
   public enum ED {
      Disable = 0,
      [XmlEnum(Name = "false")]
      prevFalse = 0,
      Enable = 1,
      [XmlEnum(Name = "true")]
      prevTrue = 1,
   }

   #endregion

   #region Rool level classes

   [XmlRoot("Label", IsNullable = false)]
   public class Lab {
      [XmlAttribute]
      public string Version;     // Keep track of version to allow for changes
      [XmlElement("Printer")]
      public Printer[] Printer;  // Information that pertains to the printer
      [XmlElement("Message")]
      public Msg[] Message;      // Information that pertains to the message
   }

   [XmlRoot("SubRules", IsNullable = false)]
   public class SubRules {
      [XmlIgnore]
      public bool IsDirty = false;
      public Substitution Substitution;

      [XmlArray("Shifts")]
      [XmlArrayItem("Shift")]
      public Shift[] Shifts;

      public TimeCount TimeCount;

   }

   #endregion

   #region Message Classes

   public class Msg {
      [XmlAttribute]
      public string Layout;    // Supports only individual at the moment
      [XmlAttribute]
      public string Name;      // Name for moving messages to/from the directory
      [XmlAttribute]
      public int Nozzle;
      [XmlElement("Column")]
      public Column[] Column;  // Message made up of columns and items within column
   }

   public class Column {
      [XmlAttribute]
      public int InterLineSpacing;  // Spacing between items
      [XmlElement("Item")]
      public Item[] Item;              // Items within a column
   }

   public class Item {
      [XmlAttribute]
      public string Type;              // Not needed here.  Used for cijConnect
      public FontDef Font;             // DotMatrix code
      public BarCode BarCode;          // Only used if barcode exists

      [XmlElement("Date")]
      public Date[] Date;              // Multiple calendars can appear in an item

      [XmlElement("Counter")]
      public Counter[] Counter;        // Multiple counters can appear within an item

      public Location Location;        // Use for internal processing only

      public string Text;              // Message Text
      public int SplitAt = -1;         // Text must be broken into two parts at this point

      public bool ShouldSerializeBarCode() {
         return BarCode != null;  // Write out BarCode only if it is used.
      }
   }

   public class Location {
      [XmlAttribute]
      public int Row;                  // 1-Origin
      [XmlAttribute]
      public int Col;                  // 1-Origin
      [XmlAttribute]
      public int Index;                // 1-Origin
      [XmlAttribute]
      public int X;                    // 0-Origin == Will be needed for Free Layout
      [XmlAttribute]
      public int Y;                    // 0-Origin == Will be needed for Free Layout
      [XmlIgnore]
      public int calStart = 0;         // 1-Origin == First calendar object in item
      [XmlIgnore]
      public int calCount = 0;         // Number of calendar objects used in item
      [XmlIgnore]
      public int countStart = 0;       // 1-Origin == First count object in item
      [XmlIgnore]
      public int countCount = 0;       // Number of counter objects in item

      public bool ShouldSerializeX() {
         return X >= 0;                // Set to -1 if Layout != FreeLayout.
      }
      public bool ShouldSerializeY() {
         return Y >= 0;                // Set to -1 if Layout != FreeLayout.
      }
   }

   public class FontDef {
      [XmlAttribute]
      public int InterCharacterSpace; // Space between characters
      [XmlAttribute]
      public int IncreasedWidth;      // Bolding
      [XmlAttribute]
      public int IW {                 // Bolding abbreviation
         get { return IncreasedWidth; }
         set { IncreasedWidth = value; }
      }
      [XmlAttribute]
      public string DotMatrix;           // Font face

      public bool ShouldSerializeIW() {
         return false;                    // Load IW but save as IncreasedWidth.
      }
   }

   public class BarCode {
      [XmlAttribute]
      public string HumanReadableFont;   // Human readable font face
      [XmlAttribute]
      public string EANPrefix;           // EAN Prefix
      [XmlAttribute]
      public string DotMatrix;           // Barcode symbology
   }

   public class Counter {
      [XmlAttribute]
      public int Block;                  // 1-Origin designation of counter within item

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
      public string UpdateIP;
      [XmlAttribute]
      public string UpdateUnit;
      [XmlAttribute]
      public string ExternalCount;
      [XmlAttribute]
      public string Multiplier;
      [XmlAttribute]
      public string SkipCount;
   }

   public class Date {
      [XmlAttribute]
      public int Block;
      [XmlAttribute]
      public string SubstitutionRule;
      [XmlAttribute]
      public string RuleName;
      [XmlAttribute]
      public string PlainText;

      public Offset Offset;
      public ZeroSuppress ZeroSuppress;
      public Substitute Substitute;

      [XmlArray("Shifts")]
      [XmlArrayItem("Shift")]
      public Shift[] Shifts;

      public TimeCount TimeCount;

      public bool ShouldSerializeOffset() {
         return Offset != null && Offset.ShouldSerializeOffset();
      }
      public bool ShouldSerializeZeroSuppress() {
         return ZeroSuppress != null && (ZeroSuppress.Year != ZS.None || ZeroSuppress.Month != ZS.None || ZeroSuppress.Day != ZS.None ||
            ZeroSuppress.Hour != ZS.None || ZeroSuppress.Minute != ZS.None || ZeroSuppress.Week != ZS.None || ZeroSuppress.DayOfWeek != ZS.None);
      }
      public bool ShouldSerializeSubstitute() {
         return Substitute != null
            && (Substitute.Year != ED.Disable || Substitute.Month != ED.Disable || Substitute.Day != ED.Disable
            || Substitute.Hour != ED.Disable || Substitute.Minute != ED.Disable || Substitute.Week != ED.Disable
            || Substitute.DayOfWeek != ED.Disable);
      }
   }

   public class Offset {
      [XmlAttribute]
      public int Year = 0;
      [XmlAttribute]
      public int Month = 0;
      [XmlAttribute]
      public int Day = 0;
      [XmlAttribute]
      public int Hour = 0;
      [XmlAttribute]
      public int Minute = 0;

      public bool ShouldSerializeYear() {
         return this.Year != 0;
      }
      public bool ShouldSerializeMonth() {
         return this.Month != 0;
      }
      public bool ShouldSerializeDay() {
         return this.Day != 0;
      }
      public bool ShouldSerializeHour() {
         return this.Hour != 0;
      }
      public bool ShouldSerializeMinute() {
         return this.Minute != 0;
      }
      public Offset Copy() {
         return (Offset)MemberwiseClone();
      }
      public bool ShouldSerializeOffset() {
         return Year != 0 || Month != 0 || Day != 0 || Hour != 0 || Minute != 0;
      }
   }

   public class ZeroSuppress {
      [XmlAttribute]
      public ZS Year = ZS.None;
      [XmlAttribute]
      public ZS Month = ZS.None;
      [XmlAttribute]
      public ZS Day = ZS.None;
      [XmlAttribute]
      public ZS Hour = ZS.None;
      [XmlAttribute]
      public ZS Minute = ZS.None;
      [XmlAttribute]
      public ZS Week = ZS.None;
      [XmlAttribute]
      public ZS DayOfWeek = ZS.None;
      public bool ShouldSerializeYear() {
         return this.Year != ZS.None;
      }
      public bool ShouldSerializeMonth() {
         return this.Month != ZS.None;
      }
      public bool ShouldSerializeDay() {
         return this.Day != ZS.None;
      }
      public bool ShouldSerializeHour() {
         return this.Hour != ZS.None;
      }
      public bool ShouldSerializeMinute() {
         return this.Minute != ZS.None;
      }
      public bool ShouldSerializeWeek() {
         return this.Week != ZS.None;
      }
      public bool ShouldSerializeDayOfWeek() {
         return this.DayOfWeek != ZS.None;
      }
      public ZeroSuppress Copy() {
         return (ZeroSuppress)this.MemberwiseClone();
      }
      public bool ShouldSerializeZeroSuppress() {
         return Year != ZS.None || Month != ZS.None || Day != ZS.None ||
            Hour != ZS.None || Minute != ZS.None || Week != ZS.None || DayOfWeek != ZS.None;
      }
   }

   public class Substitute {
      [XmlAttribute]
      public ED Year = ED.Disable;
      [XmlAttribute]
      public ED Month = ED.Disable;
      [XmlAttribute]
      public ED Day = ED.Disable;
      [XmlAttribute]
      public ED Hour = ED.Disable;
      [XmlAttribute]
      public ED Minute = ED.Disable;
      [XmlAttribute]
      public ED Week = ED.Disable;
      [XmlAttribute]
      public ED DayOfWeek = ED.Disable;

      public bool ShouldSerializeYear() {
         return this.Year != ED.Disable;
      }
      public bool ShouldSerializeMonth() {
         return this.Month != ED.Disable;
      }
      public bool ShouldSerializeDay() {
         return this.Day != ED.Disable;
      }
      public bool ShouldSerializeHour() {
         return this.Hour != ED.Disable;
      }
      public bool ShouldSerializeMinute() {
         return this.Minute != ED.Disable;
      }
      public bool ShouldSerializeWeek() {
         return this.Week != ED.Disable;
      }
      public bool ShouldSerializeDayOfWeek() {
         return this.DayOfWeek != ED.Disable;
      }
      public bool ShouldSerializeSubstitute() {
         return Year != ED.Disable || Month != ED.Disable || Day != ED.Disable || Hour != ED.Disable ||
            Minute != ED.Disable || Week != ED.Disable || DayOfWeek != ED.Disable;
      }
      public Substitute Copy() {
         return (Substitute)this.MemberwiseClone();
      }
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

      public Shift Copy() {
         return new Shift() {
            ShiftNumber = this.ShiftNumber,
            StartHour = string.Copy(this.StartHour),
            StartMinute = string.Copy(this.StartMinute),
            EndHour = string.Copy(this.EndHour),
            EndMinute = string.Copy(this.EndMinute),
            ShiftCode = string.Copy(this.ShiftCode),
         };
      }
   }

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
   public class TimeCount {
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
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

      public TimeCount Copy() {
         return new TimeCount() {
            Interval = string.Copy(this.Interval),
            Start = string.Copy(this.Start),
            End = string.Copy(this.End),
            ResetTime = string.Copy(this.ResetTime),
            ResetValue = string.Copy(this.ResetValue)
         };
      }

      public override bool Equals(object other) {
         TimeCount t = (TimeCount)other;
         if (this.Interval != t.Interval)
            return false;
         if (this.Start != t.Start)
            return false;
         if (this.End != t.End)
            return false;
         if (this.ResetTime != t.ResetTime)
            return false;
         if (this.ResetValue != t.ResetValue)
            return false;
         return true;
      }
   }

   #endregion

   #region Printer Classes

   public class Printer {
      [XmlAttribute]
      public string Make;
      [XmlAttribute]
      public string Model;
      [XmlAttribute]
      public int Nozzle;
      public PrintHead PrintHead;
      public ContinuousPrinting ContinuousPrinting;
      public TargetSensor TargetSensor;
      public CharacterSize CharacterSize;
      public PrintStartDelay PrintStartDelay;
      public EncoderSettings EncoderSettings;
      public InkStream InkStream;
      public ClockSystem ClockSystem;
      public Substitution Substitution;
      [XmlArray("Logos")]
      [XmlArrayItem("Logo")]
      public Logo[] Logos;
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
      public bool ShouldSerializeContinuousPrinting() {
         if (int.TryParse(PrintsPerTrigger, out int n)) {
            return n > 0;
         } else {
            return false;
         }
      }
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
      public string Width;
      [XmlAttribute]
      public string Height;
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

   public class ClockSystem {
      [XmlAttribute]
      public string HourMode24;
   }

   public class Logo {
      [XmlAttribute]
      public string Layout;
      [XmlAttribute]
      public string DotMatrix;
      [XmlAttribute]
      public int Height;
      [XmlAttribute]
      public int Width;
      [XmlAttribute]
      public int Location;
      [XmlAttribute]
      public string FileName;
      [XmlAttribute]
      public string RawData;
      public bool ShouldSerializeDotMatrix() {
         return this.Layout == "Fixed";
      }
      public bool ShouldSerializeDotWidth() {
         return this.Layout == "Free";
      }
      public bool ShouldSerializeDotHeight() {
         return this.Layout == "Free";
      }
   }

   public class Substitution {
      [XmlAttribute]
      public string Delimiter;
      [XmlAttribute]
      public int StartYear;
      [XmlAttribute]
      public int RuleNumber;

      [XmlElement("Rule")]
      public SubstitutionRule[] SubRule;

      public Substitution Copy() {
         SubstitutionRule[] sr = new SubstitutionRule[this.SubRule.Length];
         for (int i = 0; i < this.SubRule.Length; i++) {
            sr[i] = this.SubRule[i].Copy();
         }
         return new Substitution() {
            Delimiter = string.Copy(this.Delimiter),
            StartYear = this.StartYear,
            RuleNumber = this.RuleNumber,
            SubRule = sr
         };
      }

   }

   public class SubstitutionRule {
      [XmlAttribute]
      public string Type;
      [XmlAttribute]
      public int Base;
      [XmlText]
      public string Text;

      public SubstitutionRule Copy() {
         return new SubstitutionRule() {
            Type = string.Copy(this.Type),
            Base = this.Base,
            Text = string.Copy(this.Text)
         };
      }
   }


   #endregion

   #region Serialize and Deserialize routines

   public class Serializer<T> {

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(object sender, SerializerEventArgs e);

      public static string ClassToXml(T Label) {
         string result = string.Empty;
         XmlSerializer serializer = new XmlSerializer(typeof(T));
         XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
         ns.Add("", "");
         using (MemoryStream ms = new MemoryStream()) {
            serializer.Serialize(ms, Label, ns);
            ms.Position = 0;
            result = new StreamReader(ms).ReadToEnd();
         }
         return result;
      }

      public T XmlToClass(string xml) {
         T subRules = default(T);
         XmlSerializer serializer = new XmlSerializer(typeof(T));
         try {
            // Arm the Serializer
            serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            using (TextReader reader = new StringReader(xml)) {
               // Deserialize the file contents
               subRules = (T)serializer.Deserialize(reader);
            }
         } catch (Exception e) {
            Log?.Invoke(this, new SerializerEventArgs() { Message = e.Message });
         } finally {
            // Release the error detection events
            serializer.UnknownNode -= new XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute -= new XmlAttributeEventHandler(serializer_UnknownAttribute);
         }
         return subRules;
      }

      private void serializer_UnknownNode(object sender, XmlNodeEventArgs e) {
         Log?.Invoke(this, new SerializerEventArgs() { Message = $"Unknown Node:{e.Name}\t{e.Text}" });
      }

      private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e) {
         Log?.Invoke(this, new SerializerEventArgs() { Message = $"Unknown Node:{e.Attr.Name}\t{e.Attr.Value}" });
      }

   }

   public class SerializerEventArgs : EventArgs {
      public string Message = string.Empty;
   }

   #endregion

}
