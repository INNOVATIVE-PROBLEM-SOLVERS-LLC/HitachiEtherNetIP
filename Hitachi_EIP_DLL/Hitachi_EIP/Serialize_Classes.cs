using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


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
      [XmlIgnore]
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
      public string InterCharacterSpace;
      [XmlAttribute]
      public string IncreasedWidth;
      [XmlAttribute]
      public string DotMatrix;

      [XmlAttribute]
      public string ICS {
         get { return InterCharacterSpace; }
         set { InterCharacterSpace = value; }
      }
   }

   public class BarCode {
      [XmlAttribute]
      public string HumanReadableFont;
      [XmlAttribute]
      public string EANPrefix;
      [XmlAttribute]
      public string DotMatrix;
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
