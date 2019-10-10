using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace EIP_Lib {

   [XmlRoot("Label", IsNullable = false)]
   public class Label {
      [XmlAttribute]
      public string Version;
      public Printer Printer;
      public Message Message;
   }

   public class Printer {
      [XmlAttribute]
      public string Name;
      [XmlAttribute]
      public string Model;
   }

   public class Message {
      [XmlArray("Column")]
      public Item[] Item;
   }

   public class Item {
      [XmlAttribute]
      public string Type;
      public FontDesc Font;
   }

   public class FontDesc {
      [XmlAttribute]
      public string IncreaseWidth;
      [XmlAttribute]
      public string InterCharacterSpace;
      [XmlText]
      public string Face;
   }

   public partial class EIP {
      private void CreateItem(string filename) {

         XmlSerializer serializer = new XmlSerializer(typeof(Label));
         TextWriter writer = new StreamWriter(filename);
         Label Label = new Label() { Version = "Serialization-1" };

         Label.Printer = new Printer() { Name = "Hitachi", Model = GetAttribute(ccUI.Model_Name) };

         Label.Message = new Message();

         Label.Message.Item = new Item[3];
         Label.Message.Item[0] = new Item();
         Label.Message.Item[0].Type = "Text";
         Label.Message.Item[0].Font = new FontDesc();
         Label.Message.Item[0].Font.IncreaseWidth = "1";
         Label.Message.Item[0].Font.InterCharacterSpace = "2";
         Label.Message.Item[0].Font.Face = "12x16";

         Label.Message.Item[1] = new Item();
         Label.Message.Item[1].Type = "Calendar";

         Label.Message.Item[2] = new Item();
         Label.Message.Item[2].Type = "Counter";

         // Create our own namespaces for the output
         XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
         ns.Add("", "");
         serializer.Serialize(writer, Label, ns);

         writer.Close();
      }


   }
}
