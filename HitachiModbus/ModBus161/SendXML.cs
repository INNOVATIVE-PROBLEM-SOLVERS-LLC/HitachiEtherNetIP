using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace ModBus161 {

   public partial class SendRetrieveXML {

      bool UseAutomaticReflection;

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

      private void SendMessage(Msg message) {

      }

      #endregion

      #region Send Printer Settings to printer

      private void SendPrinterSettings(Printer printer) {

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
