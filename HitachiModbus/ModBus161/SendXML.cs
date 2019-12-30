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
         List<int> cols = new List<int>();            // Holds the number of rows in each column
         List<string> spacing = new List<string>();   // Holds the line spacing
         int itemCount = p.GetDecAttribute(ccPF.Number_Of_Items);
         while (n < itemCount) {
            cols.Add(lineCount = p.GetDecAttribute(ccPF.Line_Count, n));
            spacing.Add(p.GetHRAttribute(ccPF.Line_Spacing, n));
            n += lineCount;
         }

         for (int i = 0; i < cols.Count - 1; i++) {
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccPF.Delete_Column, cols.Count - i);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccIDX.Column, 1);
         p.SetAttribute(ccIDX.Line, 1);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         //p.SetAttribute(ccPF.Dot_Matrix, "5x8");           // Clear any barcodes
         //p.SetAttribute(ccPF.Barcode_Type, "None");
         //p.SetAttribute(ccPF.Print_Character_String, 0, "1"); // Set simple text in case Calendar or Counter was used
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
            p.SetAttribute(ccIDX.Column, c + 1);
            p.SetAttribute(ccIDX.Line, m.Column[c].Item.Length);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            if (m.Column[c].Item.Length > 1) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               p.SetAttribute(ccIDX.Column, c + 1);
               p.SetAttribute(ccPF.Line_Spacing, index, m.Column[c].InterLineSpacing);
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            }
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
               Item item = m.Column[c].Item[r];
               if (item.Font != null) {
                  p.SetAttribute(ccPF.Dot_Matrix, index, item.Font.DotMatrix);
                  p.SetAttribute(ccPF.InterCharacter_Space, index, item.Font.InterCharacterSpace);
                  p.SetAttribute(ccPF.Character_Bold, index, item.Font.IncreasedWidth);
               }
               string s = p.HandleBraces(item.Text);
               p.SetAttribute(ccIDX.Characters_per_Item, index, s.Length);
               p.SetAttribute(ccPF.Print_Character_String, charPosition, s);
               charPosition += item.Text.Length;
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
               hasDateOrCount |= item.Date != null | item.Counter != null;
               m.Column[c].Item[r].Location = new Location() { Index = index++, Row = r, Col = c };
            }
         }
         // Process calendar and count if needed
         if (hasDateOrCount) {
            //SendDateCount(m);
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
         if (ptr.Logos != null) {
            foreach (Logo l in ptr.Logos.Logo) {

            }
         }
         if (ptr.Substitution != null && ptr.Substitution.SubRule != null) {
            if (int.TryParse(ptr.Substitution.RuleNumber, out int ruleNumber)
               && int.TryParse(ptr.Substitution.StartYear, out int year)
               && ptr.Substitution.Delimiter.Length == 1) {
               // Substitution rules cannot be set with Auto Reflection on
               bool saveAR = UseAutomaticReflection;
               UseAutomaticReflection = false;

               p.SetAttribute(ccIDX.Substitution_Rule, ruleNumber);
               p.SetAttribute(ccSR.Start_Year, year);
               SendSubstitution(ptr.Substitution, ptr.Substitution.Delimiter);

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
               parent.Log($"Unknown substitution rule type =>{r.Type}<=");
            }
         }
      }

      private void SetSubValues(ccSR attribute, SubstitutionRule r, string delimeter) {
         if (int.TryParse(r.Base, out int b)) {
            Prop prop = p.AttrDict[ClassCode.Substitution_rules, (byte)attribute].Set;
            string[] s = r.Text.Split(delimeter[0]);
            for (int i = 0; i < s.Length; i++) {
               int n = b + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  // <TODO>
                  //byte[] data = FormatOutput(prop, n, 1, s[i]);
                  //p.SetAttribute((byte)attribute, data);
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
