using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace EIP_Lib {

   [XmlRoot("Label", IsNullable = false)]
   public class Lab {
      [XmlAttribute]
      public string Version;
      public Printer Printer;
      public Msg Message;
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
      public Date Date;
      public Counter Counter;
      public TimeCount TimeCount;
      [XmlArray("Shifts")]
      public Shift[] Shift;
      public string Text;
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
      public string Block;
      [XmlAttribute]
      public string Reset;
      [XmlAttribute]
      public string CountDirection;
      [XmlAttribute]
      public string Increment;
      [XmlAttribute]
      public string JumpFrom;
      [XmlAttribute]
      public string JumpTo;
      [XmlAttribute]
      public string BloUpdateUnit;
      [XmlAttribute]
      public string UpdateIP;
      [XmlAttribute]
      public string Range1;
      [XmlAttribute]
      public string Range2;
      [XmlAttribute]
      public string InitialValue;
      [XmlAttribute]
      public string Multiplier;
      [XmlAttribute]
      public string ZeroSuppression;
   }

   public class Date {
      [XmlAttribute]
      public string Block;
      [XmlAttribute]
      public string SubstitutionRule;
      [XmlAttribute]
      public string RuleName;

      public Offset Offset;
      public ZeroSuppress ZeroSuppress;
      public Substitute Substitute;
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
      public string ShiftNumber;
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
      public Logo Logo;
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

      public void SendXMLAsSerialization(string filename, bool UseAutoReflection = false) {
         Lab Lab;

         XmlSerializer serializer = new XmlSerializer(typeof(Lab));

         serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
         serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

         using (FileStream fs = new FileStream(filename, FileMode.Open)) {
            Lab = (Lab)serializer.Deserialize(fs);
            fs.Flush();
            fs.Close();
            fs.Dispose();
         }

         //serializer = new XmlSerializer(typeof(Lab));
         //TextWriter writer = new StreamWriter(@"c:\Temp\xxx.xml");
         //// Create our own namespaces for the output
         //XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
         //ns.Add("", "");
         //serializer.Serialize(writer, Lab, ns);

         //writer.Close();
      }

      #endregion

      #region Retrieve XML from printer using Serialization

      public void RetrieveXMLAsSerialization(string FileName, bool UseAutoReflection = false) {
         if (StartSession(true)) {
            if (ForwardOpen()) {
               try {
                  Lab Label = new Lab() { Version = "Serialization-1" };

                  Label.Printer = new Printer() { Make = "Hitachi", Model = GetAttribute(ccUI.Model_Name) };

                  Label.Message = new Msg();

                  XmlSerializer serializer = new XmlSerializer(typeof(Label));
                  TextWriter writer = new StreamWriter(FileName);
                  // Create our own namespaces for the output
                  XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                  ns.Add("", "");
                  serializer.Serialize(writer, Label, ns);

                  writer.Close();
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
               } catch {
                  // You are on your own here
               }
            }
            ForwardClose();
         }
         EndSession();
      }

      #endregion

      #region Service Routines

      private void serializer_UnknownNode(object sender, XmlNodeEventArgs e) {
         LogIt($"Unknown Node:{e.Name}\t{e.Text}");
      }

      private void serializer_UnknownAttribute (object sender, XmlAttributeEventArgs e) {
         System.Xml.XmlAttribute attr = e.Attr;
         LogIt($"Unknown Node:{attr.Name}\t{attr.Value}");
      }

      #endregion

   }
}
