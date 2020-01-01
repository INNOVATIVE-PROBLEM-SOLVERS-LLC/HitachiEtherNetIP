using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace ModBus161 {

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
      public string Layout;    // Supports only individual at the moment
      [XmlAttribute]
      public string Name;     // Supports only individual at the moment
      [XmlElement("Column")]
      public Column[] Column;  // Message made up of columns and items within column
   }

   public class Column {
      [XmlAttribute]
      public string InterLineSpacing;  // Spacing between items
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

      [XmlArray("Shifts")]
      [XmlArrayItem("Shift")]
      public Shift[] Shift;            // Shift code appears in item even though it is printer wide.

      public TimeCount TimeCount;      // Time Count appears in item even though it is printer wide.

      public string Text;              // Message Text

      [XmlIgnore]
      public Location Location;        // Use for internal processing only

      public bool ShouldSerializeBarCode() {
         return BarCode != null;  // Write out BarCode only if it is used.
      }
   }

   public class Location {
      public int Row;                  // 0-Origin
      public int Col;                  // 0-Origin
      public int Index;                // 0-Origin
      public int X;                    // 0-Origin == Will be needed for Free Layout
      public int Y;                    // 0-Origin == Will be needed for Free Layout
      public int calStart = 0;         // 1-Origin == First calendar object in item
      public int calCount = 0;         // Number of calendar objects used in item
      public int countStart = 0;       // 1-Origin == First count object in item
      public int countCount = 0;       // Number of counter objects in item
   }

   public class FontDef {
      [XmlAttribute]
      public string InterCharacterSpace; // Space between characters
      [XmlAttribute]
      public string IncreasedWidth;      // Bolding
      [XmlAttribute]
      public string IW {                 // Bolding abbreviation
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

      public Offset Offset;
      public ZeroSuppress ZeroSuppress;
      public Substitute Substitute;

      public bool ShouldSerializeOffset() {
         return Offset != null && (Offset.Year != "0" || Offset.Month != "0" || Offset.Day != "0" || Offset.Hour != "0" || Offset.Minute != "0");
      }
      public bool ShouldSerializeZeroSuppress() {
         return ZeroSuppress != null && (ZeroSuppress.Year != null || ZeroSuppress.Month != null || ZeroSuppress.Day != null ||
            ZeroSuppress.Hour != null || ZeroSuppress.Minute != null || ZeroSuppress.Week != null || ZeroSuppress.DayOfWeek != null);
      }
      public bool ShouldSerializeSubstitute() {
         return Substitute != null && (Substitute.Year != null || Substitute.Month != null || Substitute.Day != null ||
            Substitute.Hour != null || Substitute.Minute != null || Substitute.Week != null || Substitute.DayOfWeek != null);
      }
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
      public bool ShouldSerializeYear() {
         return this.Year != "0";
      }
      public bool ShouldSerializeMonth() {
         return this.Month != "0";
      }
      public bool ShouldSerializeDay() {
         return this.Day != "0";
      }
      public bool ShouldSerializeHour() {
         return this.Hour != "0";
      }
      public bool ShouldSerializeMinute() {
         return this.Minute != "0";
      }
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
      public ClockSystem ClockSystem;
      public Substitution Substitution;
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
      public string DotMatrix;
      [XmlAttribute]
      public string Location;
      [XmlAttribute]
      public string FileName;
      [XmlAttribute]
      public string RawData;
   }

   public class Substitution {
      [XmlAttribute]
      public string Delimiter;
      [XmlAttribute]
      public string StartYear;
      [XmlAttribute]
      public string RuleNumber;

      [XmlElement("Rule")]
      public SubstitutionRule[] SubRule;

   }

   public class SubstitutionRule {
      [XmlAttribute]
      public string Type;
      [XmlAttribute]
      public string Base;

      [XmlText]
      public string Text;
   }


   #endregion

}
