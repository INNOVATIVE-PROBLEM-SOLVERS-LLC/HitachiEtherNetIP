using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace HitachiEIP {

   public class XML {

      #region Data Declarations

      HitachiBrowser parent;
      EIP EIP;
      TabPage tab;

      string LoadedFileName;

      // Controls
      TabControl tclViewXML;
      TabPage tabTreeView;
      TabPage tabIndented;

      TreeView tvXML;
      TextBox txtIndentedView;

      Button cmdOpen;
      Button cmdClear;
      Button cmdGenerate;
      Button cmdSaveAs;
      Button cmdSendToPrinter;

      string XMLText = string.Empty;

      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
      }

      #endregion

      #region Constructors and destructors

      public XML(HitachiBrowser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;

         tclViewXML = new TabControl() { Name = "tclViewXML" };
         tabTreeView = new TabPage() { Name = "tabTreeView", Text = "Tree View" };
         tabIndented = new TabPage() { Name = "tabIndented", Text = "Indented View" };

         tvXML = new TreeView() { Name = "tvXML" };
         txtIndentedView = new TextBox() { Name = "txtIndentedView", Multiline = true, ScrollBars = ScrollBars.Both };

         cmdOpen = new Button() { Text = "Open" };
         cmdOpen.Click += Open_Click;

         cmdClear = new Button() { Text = "Clear" };
         cmdClear.Click += Clear_Click;

         cmdGenerate = new Button() { Text = "Generate" };
         cmdGenerate.Click += Generate_Click;

         cmdSaveAs = new Button() { Text = "Save As" };
         cmdSaveAs.Click += SaveAs_Click;

         cmdSendToPrinter = new Button() { Text = "Send To Printer", Enabled = false };
         cmdSendToPrinter.Click += SendToPrinter_Click;

         tab.Controls.Add(tclViewXML);

         tclViewXML.Controls.Add(tabTreeView);
         tclViewXML.Controls.Add(tabIndented);

         tabTreeView.Controls.Add(tvXML);
         tabIndented.Controls.Add(txtIndentedView);

         tab.Controls.Add(cmdOpen);
         tab.Controls.Add(cmdClear);
         tab.Controls.Add(cmdGenerate);
         tab.Controls.Add(cmdSaveAs);
         tab.Controls.Add(cmdSendToPrinter);
      }

      #endregion

      #region Form Control Events

      private void Open_Click(object sender, EventArgs e) {
         DialogResult dlgResult = DialogResult.Retry;
         string fileName = String.Empty;
         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.InitialDirectory = LoadedFileName;
            dlg.Title = "Select XML formatted file!";
            dlg.Filter = "XML (*.xml)|*.xml|All (*.*)|*.*";
            dlg.DefaultExt = "txt";
            dlg.FilterIndex = 1;
            dlgResult = DialogResult.Retry;
            while (dlgResult == DialogResult.Retry) {
               dlgResult = dlg.ShowDialog();
               if (dlgResult == DialogResult.OK) {
                  try {
                     ProcessLabel(File.ReadAllText(dlg.FileName));
                  } catch (Exception ex) {
                     MessageBox.Show(parent, ex.Message, "Cannot load XML File!");
                  }
               }
            }
         }
      }

      private void Clear_Click(object sender, EventArgs e) {
         txtIndentedView.Text = string.Empty;
         tvXML.Nodes.Clear();
         XMLText = string.Empty;
         SetButtonEnables();
      }

      private void Generate_Click(object sender, EventArgs e) {
         XMLText = ConvertLayoutToXML();
         ProcessLabel(XMLText);
         SetButtonEnables();
      }

      private void SaveAs_Click(object sender, EventArgs e) {
         DialogResult dlgResult;
         string filename = this.LoadedFileName;

         using (SaveFileDialog saveFileDialog1 = new SaveFileDialog()) {
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "xml";
            saveFileDialog1.Filter = "xml|*.xml";
            saveFileDialog1.Title = "Save Printer Layout to XML file";
            saveFileDialog1.FileName = filename;
            dlgResult = saveFileDialog1.ShowDialog();
            if (dlgResult == DialogResult.OK && !String.IsNullOrEmpty(saveFileDialog1.FileName)) {
               filename = saveFileDialog1.FileName;
               this.SaveLayoutToXML(filename);
            }
         }
         SetButtonEnables();
      }

      private void SaveLayoutToXML(string filename) {
         Stream outfs = null;
         outfs = new FileStream(filename, FileMode.Create);
         outfs.Write(EIP.encode.GetBytes(XMLText), 0, XMLText.Length);
         outfs.Flush();
         outfs.Close();
         SetButtonEnables();
      }

      private void SendToPrinter_Click(object sender, EventArgs e) {
      }

      #endregion

      #region XML  Save Routines

      private string ConvertLayoutToXML() {
         ItemType itemType = ItemType.Text;
         bool OpenCloseForward = !EIP.ForwardIsOpen;
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
               OpenCloseForward = !EIP.ForwardIsOpen;
               if (OpenCloseForward) {
                  EIP.ForwardOpen();
               }
               writer.WriteStartElement("Label"); // Start Label

               //writer.WriteAttributeString("ClockSystem", this.ClockSystem);
               //writer.WriteAttributeString("Registration", this.Registration.ToString());
               //writer.WriteAttributeString("GroupNumber", this.MessageGroupNumber);
               //writer.WriteAttributeString("GroupName", this.MessageGroup);
               //writer.WriteAttributeString("Name", this.MessageName);
               //writer.WriteAttributeString("BeRestrictive", this.BeRestrictive.ToString());
               //writer.WriteAttributeString("UseHalfSpace", this.UseHalfSpace.ToString());
               //writer.WriteAttributeString("Format", MessageStyle.ToString());

               writer.WriteAttributeString("Version", "1");
               WritePrinterSettings(writer);

               writer.WriteStartElement("Objects"); // Start Objects
               if (EIP.ForwardIsOpen && OpenCloseForward) {
                  EIP.ForwardClose();
               }

               int itemCount = GetDecimalAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Number_Of_Items);
               for (int i = 0; i < itemCount; i++) {
                  OpenCloseForward = !EIP.ForwardIsOpen;
                  if (OpenCloseForward) {
                     EIP.ForwardOpen();
                  }

                  SetAttribute(eipClassCode.Index, (byte)eipIndex.Item, i + 1);
                  string text = GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Print_Character_String);

                  itemType = GetItemType(text);

                  writer.WriteStartElement("Object"); // Start Object

                  writer.WriteAttributeString("Type", Enum.GetName(typeof(ItemType), itemType));

                  writer.WriteStartElement("Font"); // Start Font
                  {
                     writer.WriteAttributeString("HumanReadableFont", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Readable_Code));
                     writer.WriteAttributeString("EANPrefix", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Prefix_Code));
                     writer.WriteAttributeString("BarCode", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Barcode_Type));
                     writer.WriteAttributeString("IncreasedWidth", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Character_Bold));
                     writer.WriteAttributeString("InterLineSpace", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Line_Spacing));
                     writer.WriteAttributeString("InterCharacterSpace", GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.InterCharacter_Space));
                     writer.WriteString(GetAttribute(eipClassCode.Print_format, (byte)eipPrint_format.Dot_Matrix));
                  }
                  writer.WriteEndElement(); // End Font

                  writer.WriteStartElement("Location"); // Start Location
                  {
                     writer.WriteAttributeString("ItemNumber", (i + 1).ToString());
                     //   writer.WriteAttributeString("Column", p.Column.ToString());
                     //   writer.WriteAttributeString("Row", p.Row.ToString());
                     //   writer.WriteAttributeString("Height", p.ItemHeight.ToString());
                     //   writer.WriteAttributeString("Width", (p.ItemWidth * p.IncreasedWidth).ToString());
                     //   writer.WriteAttributeString("Left", p.X.ToString());
                     //   writer.WriteAttributeString("Top", (p.Y + p.ScaledImage.Height).ToString());
                  }
                  writer.WriteEndElement(); // End Location

                  switch (itemType) {
                     case ItemType.Text:
                        break;
                     case ItemType.Date:
                        WriteCalendarSettings(writer);
                        break;
                     case ItemType.Counter:
                        WriteCounterSettings(writer);
                        break;
                     case ItemType.Logo:
                        WriteUserPatternSettings(writer);
                        break;
                     default:
                        break;
                  }

                  writer.WriteElementString("Text", text);
                  writer.WriteEndElement(); // End Object

                  if (EIP.ForwardIsOpen && OpenCloseForward) {
                     EIP.ForwardClose();
                  }
               }

               writer.WriteEndElement(); // End Objects
               writer.WriteEndElement(); // End Label

               writer.WriteEndDocument();
               writer.Flush();
               ms.Position = 0;
               return new StreamReader(ms).ReadToEnd();
            }
         }
      }

      private void WriteCounterSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Counter"); // Start Counter
         writer.WriteAttributeString("Reset", GetAttribute(eipClassCode.Count, (byte)eipCount.Reset_Value));
         //writer.WriteAttributeString("ExternalSignal", p.CtExternalSignal);
         //writer.WriteAttributeString("ResetSignal", p.CtResetSignal);
         writer.WriteAttributeString("CountUp", GetAttribute(eipClassCode.Count, (byte)eipCount.Direction_Value));
         writer.WriteAttributeString("Increment", GetAttribute(eipClassCode.Count, (byte)eipCount.Increment_Value));
         writer.WriteAttributeString("JumpTo", GetAttribute(eipClassCode.Count, (byte)eipCount.Jump_To));
         writer.WriteAttributeString("JumpFrom", GetAttribute(eipClassCode.Count, (byte)eipCount.Jump_From));
         writer.WriteAttributeString("UpdateUnit", GetAttribute(eipClassCode.Count, (byte)eipCount.Update_Unit_Unit));
         writer.WriteAttributeString("UpdateIP", GetAttribute(eipClassCode.Count, (byte)eipCount.Update_Unit_Halfway));
         writer.WriteAttributeString("Range2", GetAttribute(eipClassCode.Count, (byte)eipCount.Count_Range_2));
         writer.WriteAttributeString("Range1", GetAttribute(eipClassCode.Count, (byte)eipCount.Count_Range_1));
         writer.WriteAttributeString("InitialValue", GetAttribute(eipClassCode.Count, (byte)eipCount.Initial_Value));
         //writer.WriteAttributeString("Format", p.RawText);
         writer.WriteAttributeString("Multiplier", GetAttribute(eipClassCode.Count, (byte)eipCount.Count_Multiplier));
         writer.WriteAttributeString("ZeroSuppression", GetAttribute(eipClassCode.Count, (byte)eipCount.Availibility_Of_Zero_Suppression));
         writer.WriteEndElement(); //  End Counter
      }

      private void WriteCalendarSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Date"); // Start Date
         {
            //writer.WriteAttributeString("Format", p.RawText);

            writer.WriteStartElement("Offset"); // Start Offset
            {
               writer.WriteAttributeString("Minute", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Offset_Minute));
               writer.WriteAttributeString("Hour", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Offset_Hour));
               writer.WriteAttributeString("Day", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Offset_Day));
               writer.WriteAttributeString("Month", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Offset_Month));
               writer.WriteAttributeString("Year", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Offset_Year));
            }
            writer.WriteEndElement(); // End Offset

            writer.WriteStartElement("ZeroSuppress"); // Start ZeroSuppress
            {
               writer.WriteAttributeString("DayOfWeek", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Zero_Suppress_Day_Of_Week));
               writer.WriteAttributeString("Week", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Zero_Suppress_Weeks));
               writer.WriteAttributeString("Minute", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Zero_Suppress_Minute));
               writer.WriteAttributeString("Hour", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Zero_Suppress_Hour));
               writer.WriteAttributeString("Day", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Zero_Suppress_Day));
               writer.WriteAttributeString("Month", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Zero_Suppress_Month));
               writer.WriteAttributeString("Year", GetAttribute(eipClassCode.Calendar, (byte)eipCalendar.Zero_Suppress_Year));
            }
            writer.WriteEndElement(); // End ZeroSuppress

            writer.WriteStartElement("EnableSubstitution"); // Start EnableSubstitution
            {
               //writer.WriteAttributeString("SubstitutionRule", p.DTSubRule);
               writer.WriteAttributeString("DayOfWeek", GetAttribute(eipClassCode.Substitution_rules, (byte)eipSubstitution_rules.Day_Of_Week));
               writer.WriteAttributeString("Week", GetAttribute(eipClassCode.Substitution_rules, (byte)eipSubstitution_rules.Week));
               writer.WriteAttributeString("Minute", GetAttribute(eipClassCode.Substitution_rules, (byte)eipSubstitution_rules.Minute));
               writer.WriteAttributeString("Hour", GetAttribute(eipClassCode.Substitution_rules, (byte)eipSubstitution_rules.Hour));
               writer.WriteAttributeString("Day", GetAttribute(eipClassCode.Substitution_rules, (byte)eipSubstitution_rules.Day));
               writer.WriteAttributeString("Month", GetAttribute(eipClassCode.Substitution_rules, (byte)eipSubstitution_rules.Month));
               writer.WriteAttributeString("Year", GetAttribute(eipClassCode.Substitution_rules, (byte)eipSubstitution_rules.Year));
            }
            writer.WriteEndElement(); // End EnableSubstitution

            writer.WriteEndElement(); // End Date
         }
      }

      private void WriteUserPatternSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Logo"); // Start Logo
         {
            //writer.WriteAttributeString("Variable", p.WlxVariableName);
            //writer.WriteAttributeString("HAlignment", Enum.GetName(typeof(Utils.HAlignment), p.LogoHAlignment));
            //writer.WriteAttributeString("VAlignment", Enum.GetName(typeof(Utils.VAlignment), p.LogoVAlignment));
            //writer.WriteAttributeString("Filter", p.LogoFilter.ToString());
            //writer.WriteAttributeString("ReverseVideo", p.LogoReverseVideo.ToString());
            //writer.WriteAttributeString("Source", p.LogoSource);
            //writer.WriteAttributeString("LogoLength", p.LogoLength.ToString());
            //writer.WriteAttributeString("Registration", p.LogoRegistration.ToString());

            //using (MemoryStream ms2 = new MemoryStream()) {
            //   // bm.Save does not like transparent pixels so make them white
            //   Bitmap bm2 = new Bitmap(p.ItemWidth, p.ItemHeight);
            //   using (Graphics g = Graphics.FromImage(bm2)) {
            //      g.Clear(Color.White);
            //      for (int x = 0; x < p.ItemWidth; x++) {
            //         for (int y = 0; y < p.ItemHeight; y++) {
            //            if (p.ScaledImage.GetPixel(x, y).ToArgb() != 0) {
            //               bm2.SetPixel(x, y, Color.Black);
            //            }
            //         }
            //      }
            //   }
            //   Bitmap bm = Utils.BitmapTo1Bpp(bm2);
            //   bm.Save(ms2, ImageFormat.Bmp);
            //   using (BinaryReader br = new BinaryReader(ms2)) {
            //      ms2.Position = 0;
            //      byte[] b = br.ReadBytes((int)ms2.Length);
            //      int length = Utils.LittleEndian(b, 2, 4);
            //      string data = "";
            //      writer.WriteAttributeString("size", length.ToString());
            //      for (int j = 0; j < length; j++) {
            //         data = data + string.Format("{0:X2} ", b[j]);
            //      }
            //      writer.WriteStartElement("Data"); // Start Data
            //      writer.WriteAttributeString("Length", length.ToString());
            //      writer.WriteString(data.TrimEnd());
            //      writer.WriteEndElement(); // End Data
            //   }
            //}
         }
         writer.WriteEndElement(); // End Logo
         //string LogoText = "";
         //for (int j = 0; j < p.ItemText.Length; j++) {
         //   LogoText += ((short)p.ItemText[j]).ToString("X4");
         //}
      }

      private void WritePrinterSettings(XmlTextWriter writer) {

         writer.WriteStartElement("Printer");
         {
            {
               writer.WriteAttributeString("Model", GetAttribute(eipClassCode.Unit_Information, (byte)eipUnit_Information.Model_Name));
            }
            writer.WriteAttributeString("Make", "Hitachi");

            writer.WriteStartElement("PrintHead");
            {
               writer.WriteAttributeString("Orientation", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Character_Orientation));
            }
            writer.WriteEndElement(); // PrintHead

            writer.WriteStartElement("ContinuousPrinting");
            {
               writer.WriteAttributeString("RepeatInterval", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Repeat_Interval));
               writer.WriteAttributeString("PrintsPerTrigger", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Repeat_Count));
            }
            writer.WriteEndElement(); // ContinuousPrinting

            writer.WriteStartElement("TargetSensor");
            {
               writer.WriteAttributeString("Filter", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Target_Sensor_Filter));
               writer.WriteAttributeString("SetupValue", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Targer_Sensor_Filter_Value));
               writer.WriteAttributeString("Timer", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Target_Sensor_Timer));
            }
            writer.WriteEndElement(); // TargetSensor

            writer.WriteStartElement("CharacterSize");
            {
               writer.WriteAttributeString("Height", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Character_Width));
               writer.WriteAttributeString("Width", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Character_Height));
            }
            writer.WriteEndElement(); // CharacterSize

            writer.WriteStartElement("PrintStartDelay");
            {
               writer.WriteAttributeString("Reverse", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Print_Start_Delay_Forward));
               writer.WriteAttributeString("Forward", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Print_Start_Delay_Reverse));
            }
            writer.WriteEndElement(); // PrintStartDelay

            writer.WriteStartElement("EncoderSettings");
            {
               writer.WriteAttributeString("HighSpeedPrinting", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.High_Speed_Print));
               writer.WriteAttributeString("Divisor", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Pulse_Rate_Division_Factor));
               writer.WriteAttributeString("ExternalEncoder", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Product_Speed_Matching));
            }
            writer.WriteEndElement(); // EncoderSettings

            writer.WriteStartElement("InkStream");
            {
               writer.WriteAttributeString("InkDropUse", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Ink_Drop_Use));
               writer.WriteAttributeString("ChargeRule", GetAttribute(eipClassCode.Print_specification, (byte)eipPrint_specification.Ink_Drop_Charge_Rule));
            }
            writer.WriteEndElement(); // InkStream

            writer.WriteStartElement("TwinNozzle");
            {
               //writer.WriteAttributeString("LeadingCharControl", this.LeadingCharacterControl.ToString());
               //writer.WriteAttributeString("LeadingCharControlWidth1", this.LeadingCharacterControlWidth1.ToString());
               //writer.WriteAttributeString("LeadingCharControlWidth2", this.LeadingCharacterControlWidth2.ToString());
               //writer.WriteAttributeString("NozzleSpaceAlignment", this.NozzleSpaceAlignment.ToString());
            }
            writer.WriteEndElement(); // TwinNozzle
         }
         writer.WriteEndElement(); // Printer
      }

      private bool ProcessLabel(string xml) {
         bool result = false;
         try {
            int i = xml.IndexOf("<Label");
            if (i == -1) {
               xml = File.ReadAllText(xml);
               i = xml.IndexOf("<Label");
            }
            i = xml.IndexOf("<Label");
            int j = xml.IndexOf("</Label>", i + 7);
            if (j > 0)
               xml = xml.Substring(i, j - i + 8);
            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;
            dom.LoadXml(xml);
            xml = ToIndentedString(xml);
            i = xml.IndexOf("<Label");
            if (i > 0) {
               xml = xml.Substring(i);
               txtIndentedView.Text = xml;

               tvXML.Nodes.Clear();
               tvXML.Nodes.Add(new TreeNode(dom.DocumentElement.Name));
               TreeNode tNode = new TreeNode();
               tNode = tvXML.Nodes[0];

               AddNode(dom.DocumentElement, tNode);
               tvXML.ExpandAll();

               result = true;
            }
         } catch {

         }
         return result;
      }

      private string ToIndentedString(string unformattedXml) {
         string result;
         XmlReaderSettings readeroptions = new XmlReaderSettings { IgnoreWhitespace = true };
         XmlReader reader = XmlReader.Create(new StringReader(unformattedXml), readeroptions);
         StringBuilder sb = new StringBuilder();
         XmlWriterSettings xmlSettingsWithIndentation = new XmlWriterSettings { Indent = true };
         using (XmlWriter writer = XmlWriter.Create(sb, xmlSettingsWithIndentation)) {
            writer.WriteNode(reader, true);
         }
         result = sb.ToString();
         return result;
      }

      // Add a node to the tree view
      private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode) {
         if (inXmlNode is XmlWhitespace)
            return;
         XmlNode xNode;
         XmlNodeList nodeList;
         if (inXmlNode.HasChildNodes) {
            inTreeNode.Text = GetNameAttr(inXmlNode);
            nodeList = inXmlNode.ChildNodes;
            int j = 0;
            for (int i = 0; i < nodeList.Count; i++) {
               xNode = inXmlNode.ChildNodes[i];
               if (xNode is XmlWhitespace)
                  continue;
               if (xNode.Name == "#text") {
                  inTreeNode.Text = inXmlNode.OuterXml.Trim();
               } else {
                  if (!(xNode is XmlWhitespace)) {
                     inTreeNode.Nodes.Add(new TreeNode(GetNameAttr(xNode)));
                     AddNode(xNode, inTreeNode.Nodes[j]);
                  }
               }
               j++;
            }
         } else {
            inTreeNode.Text = inXmlNode.OuterXml.Trim();
         }
      }

      // Get the attributes associated witha anode
      private string GetNameAttr(XmlNode n) {
         string result = n.Name;
         if (n.Attributes.Count > 0) {
            foreach (XmlAttribute attribute in n.Attributes) {
               result += $" {attribute.Name}=\"{attribute.Value}\"";
            }
         }
         return result;
      }

      #endregion

      #region XML Load Routines

      //internal void LoadXMLFile(string fileName, bool resolve = true) {
      //   FileStream fs = null;

      //   ItemType type;
      //   XmlNode n;
      //   FormatSetup msgStyle;
      //   int x;
      //   int y = 0;
      //   int newX;
      //   int newY;

      //   try {
      //      fs = new FileStream(fileName, FileMode.Open);
      //      try {
      //         inputFileType = FileType.cijConnect;
      //         TPB p;
      //         XmlDocument xmlDoc = new XmlDocument();
      //         xmlDoc.PreserveWhitespace = true;
      //         try {
      //            xmlDoc.Load(fs);
      //            n = xmlDoc.SelectSingleNode("Label");
      //            //this.MessageName = GetAttr(n, "Name", Path.GetFileNameWithoutExtension(fileName));
      //            //this.Registration = GetAttr(n, "Registration", 1);
      //            //this.ClockSystem = GetAttr(n, "ClockSystem", "24-Hour");
      //            //this.BeRestrictive = GetAttr(n, "BeRestrictive", false);
      //            //this.UseHalfSpace = GetAttr(n, "UseHalfSpace", false);
      //            //this.MessageGroup = GetAttr(n, "GroupName", "");
      //            //this.MessageGroupNumber = GetAttr(n, "GroupNumber", "");
      //            //if (Enum.TryParse(GetAttr(n, "Format", "Individual"), out msgStyle)) {
      //            //   this.MessageStyle = msgStyle;
      //            //} else {
      //            //   this.MessageStyle = FormatSetup.Individual;
      //            //}
      //            //this.Version = GetAttr(n, "Version", 1);

      //            ReadPrinterSettings(xmlDoc.SelectSingleNode("Label/Printer"));

      //            foreach (System.Xml.XmlNode item in xmlDoc.SelectNodes("Label/Objects")[0].ChildNodes) {
      //               if (!(item is XmlWhitespace)) {
      //                  type = (TPB.ItemTypes)Enum.Parse(typeof(TPB.ItemTypes), GetAttr(item, "Type", "Text"), true);
      //                  n = item.SelectSingleNode("Location");
      //                  x = GetAttr(n, "Left", 0);
      //                  y = GetAttr(n, "Top", 0) - GetAttr(n, "Height", 0);
      //                  int r = GetAttr(n, "Row", -1);
      //                  int c = GetAttr(n, "Column", -1);

      //                  n = item.SelectSingleNode("Font");
      //                  string F = n.InnerText;
      //                  int ICS = GetAttr(n, "InterCharacterSpace", 1);
      //                  int ILS = GetAttr(n, "InterLineSpace", 1);
      //                  int IW = GetAttr(n, "IncreasedWidth", 1);

      //                  p = new TPB(this, type, x, y, F, ICS, ILS, IW);

      //                  p.Row = r;
      //                  p.Column = c;

      //                  p.BarCode = GetAttr(n, "BarCode", "(None)");
      //                  p.HumanReadableFont = GetAttr(n, "HumanReadableFont", "(None)");
      //                  p.EANPrefix = GetAttr(n, "EANPrefix", "00");

      //                  // Done to here
      //                  switch (type) {
      //                     case TPB.ItemTypes.Counter:
      //                        n = item.SelectSingleNode("Text");
      //                        p.RawText = GetValue(n, "{0000}");

      //                        n = item.SelectSingleNode("Counter");
      //                        // Must set length before any other attribute
      //                        string initValue = GetAttr(n, "InitialValue", "0000");
      //                        p.CtWidth = initValue.Length;
      //                        p.CtInitialValue = initValue;
      //                        p.CtRangeStart = GetAttr(n, "Range1", "0000");
      //                        p.CtRangeEnd = GetAttr(n, "Range2", "9999");
      //                        p.CtUpdateIP = GetAttr(n, "UpdateIP", "0000");
      //                        p.CtUpdateUnit = GetAttr(n, "UpdateUnit", "0001");
      //                        p.CtJumpFrom = GetAttr(n, "JumpFrom", "9999");
      //                        p.CtJumpTo = GetAttr(n, "JumpTo", "0000");
      //                        p.CtIncrement = GetAttr(n, "Increment", "01");
      //                        p.CtDirection = GetAttr(n, "CountUp", true);
      //                        p.CtReset = GetAttr(n, "Reset", "0");
      //                        p.CtReset = GetAttr(n, "Reset", "0");
      //                        p.CtMultiplier = GetAttr(n, "Multiplier", "0001");
      //                        p.CtZeroSuppression = GetAttr(n, "ZeroSuppression", false);
      //                        p.CtResetSignal = GetAttr(n, "ResetSignal", "0");
      //                        p.CtExternalSignal = GetAttr(n, "ExternalSignal", "0");
      //                        p.WlxVariableName = GetAttr(n, "Variable", "");

      //                        //ResolveOneReference(p);
      //                        break;
      //                     case TPB.ItemTypes.Date:
      //                        n = item.SelectSingleNode("Date");
      //                        p.DtWillettFormat = !GetAttr(n, "WindowsFormat", false);
      //                        p.RawText = GetAttr(n, "Format", item.SelectSingleNode("Text").InnerText);
      //                        p.WlxVariableName = GetAttr(n, "Variable", "");

      //                        n = item.SelectSingleNode("Date/Offset");
      //                        p.DtYearOffset = GetAttr(n, "Year", "0000");
      //                        p.DtMonthOffset = GetAttr(n, "Month", "0000");
      //                        p.DtDayOffset = GetAttr(n, "Day", "0000");
      //                        p.DtHourOffset = GetAttr(n, "Hour", "0000");
      //                        p.DtMinuteOffset = GetAttr(n, "Minute", "0000");

      //                        n = item.SelectSingleNode("Date/ZeroSuppress");
      //                        p.DtYearZS = GetAttr(n, "Year", false);
      //                        p.DtMonthZS = GetAttr(n, "Month", false);
      //                        p.DtDayZS = GetAttr(n, "Day", false);
      //                        p.DtHourZS = GetAttr(n, "Hour", false);
      //                        p.DtMinuteZS = GetAttr(n, "Minute", false);
      //                        p.DtWeekZS = GetAttr(n, "Week", false);
      //                        p.DtDayOfWeekZS = GetAttr(n, "DayOfWeek", false);

      //                        n = item.SelectSingleNode("Date/EnableSubstitution");
      //                        p.DtYearSub = GetAttr(n, "Year", false);
      //                        p.DtMonthSub = GetAttr(n, "Month", false);
      //                        p.DtDaySub = GetAttr(n, "Day", false);
      //                        p.DtHourSub = GetAttr(n, "Hour", false);
      //                        p.DtMinuteSub = GetAttr(n, "Minute", false);
      //                        p.DtWeekSub = GetAttr(n, "Week", false);
      //                        p.DtDayOfWeekSub = GetAttr(n, "DayOfWeek", false);
      //                        p.DTSubRule = GetAttr(n, "SubstitutionRule", "01");

      //                        //ResolveOneReference(p);
      //                        break;
      //                     case TPB.ItemTypes.Logo:
      //                        n = item.SelectSingleNode("Logo");
      //                        p.LogoFilter = GetAttr(n, "Filter", 84);
      //                        p.LogoReverseVideo = GetAttr(n, "ReverseVideo", false);
      //                        p.LogoSource = GetAttr(n, "Source", "");
      //                        p.LogoLength = GetAttr(n, "LogoLength", 1);
      //                        p.LogoRegistration = GetAttr(n, "Registration", -1);
      //                        p.LogoSource = GetAttr(n, "Source", "");
      //                        p.ScaledImage = GetLogoScaledImage(p, n);
      //                        p.WlxVariableName = GetAttr(n, "Variable", "");
      //                        n = item.SelectSingleNode("Text");
      //                        if (n == null) {
      //                           p.LogoItemText = string.Empty;
      //                        } else {
      //                           string ItemText = GetValue(n, "<TEXT>");
      //                           string it = "";
      //                           if (ItemText.Length > 0) {
      //                              for (int i = 0; i < ItemText.Length; i += 4) {
      //                                 it += (char)Convert.ToInt16(ItemText.Substring(i, 4), 16);
      //                              }
      //                              p.LogoItemText = it;
      //                           }
      //                        }
      //                        break;
      //                     case TPB.ItemTypes.Text:
      //                        n = item.SelectSingleNode("Text");
      //                        p.RawText = GetValue(n, "<TEXT>");
      //                        p.WlxVariableName = "";
      //                        //ResolveOneReference(p);
      //                        break;
      //                     case TPB.ItemTypes.Link:
      //                        n = item.SelectSingleNode("Link");
      //                        p.FileLocation = GetAttr(n, "Location", "");
      //                        p.WlxVariableName = GetAttr(n, "Variable", "");

      //                        n = item.SelectSingleNode("Text");
      //                        p.RawText = GetValue(n, "{l}");
      //                        //ResolveOneReference(p);
      //                        break;
      //                     case TPB.ItemTypes.Prompt:
      //                        n = item.SelectSingleNode("Prompt");
      //                        p.PromptMessage = GetAttr(n, "Message", "Please Respond");
      //                        p.PromptResponses = GetAttr(n, "Responses", "Y|N");
      //                        p.PromptAssumedResponse = GetAttr(n, "AssumedResponse", "Y");
      //                        p.WlxVariableName = GetAttr(n, "Variable", "");

      //                        n = item.SelectSingleNode("Text");
      //                        p.RawText = GetValue(n, "{p}");
      //                        //ResolveOneReference(p);
      //                        break;
      //                  }
      //                  if ((p.ItemType == TPB.ItemTypes.Logo && p.ScaledImage != null)
      //                     || (p.ItemType != TPB.ItemTypes.Logo && !string.IsNullOrEmpty(p.RawText))) {
      //                     LinkItIn(p);
      //                  } else {
      //                     Utils.Log($"Message {Path.GetFileNameWithoutExtension(fileName)} contains empty string!  Item Ignored");
      //                  }
      //               }
      //            }
      //         } catch (Exception ex2) {
      //            MessageBox.Show("Invalid file format for file \"" + fileName + "\"");
      //            Utils.Log("Invalid file format for file \"" + fileName + "\"\r\n" + ex2.ToString());
      //         }
      //      } catch (Exception ex1) {
      //         MessageBox.Show("Invalid internal file format for file \"" + fileName + "\"");
      //         Utils.Log("Can't open file \"" + fileName + "\"\r\n" + ex1.ToString());
      //      }
      //   } catch (Exception ex) {
      //      MessageBox.Show("Can't open file \"" + fileName + "\"\r\n" + ex.Message);
      //      Utils.Log("Can't open file \"" + fileName + "\"\r\n" + ex.ToString());
      //   } finally {
      //      if (fs != null)
      //         fs.Close();
      //   }
      //   if (resolve) {
      //      ResolveAllReferences();
      //   } else {
      //      foreach (TPB p in Item) {
      //         if (p.ItemType != TPB.ItemTypes.Logo) {
      //            p.ItemTextAttr = p.RawText;
      //            p.ItemText = p.RawText;
      //         }
      //      }
      //   }

      //   parent.pbMainMove(Global.gridOffset - parent.pbMain.Left, 0);
      //   SortByLocationInPC();
      //   DeselectAll();
      //   global.LineSpeed.LoadControls(true);
      //   Utils.Log($"XML File \"{fileName}\" Loaded successfully({parent.GetCheckedItem()})", Severity.Normal);
      //}

      #endregion

      #region Service Routines

      public void ResizeControls(ref ResizeInfo R) {
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         int tclWidth = (int)(tab.ClientSize.Width / R.W);
         float offset = (int)(tab.ClientSize.Height - tclHeight * R.H);
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         R.offset = offset;
         Utils.ResizeObject(ref R, tclViewXML, 0, 1, tclHeight - 4, tclWidth - 1);
         {
            Utils.ResizeObject(ref R, tvXML, 1, 1, tclHeight - 9, tclWidth - 3);

            Utils.ResizeObject(ref R, txtIndentedView, 1, 1, tclHeight - 9, tclWidth - 3);

            Utils.ResizeObject(ref R, cmdOpen, tclHeight - 3, 1, 2, 6);
            Utils.ResizeObject(ref R, cmdClear, tclHeight - 3, 8, 2, 6);
            Utils.ResizeObject(ref R, cmdGenerate, tclHeight - 3, 15, 2, 6);
            Utils.ResizeObject(ref R, cmdSaveAs, tclHeight - 3, 22, 2, 6);
            Utils.ResizeObject(ref R, cmdSendToPrinter, tclHeight - 3, 29, 2, 6);
         }
         R.offset = 0;
      }

      // Get the contents of one attribute
      private string GetAttribute(eipClassCode Class, byte Attribute) {
         AttrData attr = Data.AttrDict[Class, Attribute];
         // Have a bug here.  Some data may be required in place of NoData
         bool successful = EIP.ReadOneAttribute(Class, Attribute, attr, EIP.Nodata, out string val);
         return val;
      }

      // Get the value of an attribute that is known to be a decimal number
      private int GetDecimalAttribute(eipClassCode Class, byte Attribute) {
         GetAttribute(Class, Attribute);
         return EIP.GetDecValue;
      }

      // Set one attribute based on the Set Property
      private void SetAttribute(eipClassCode Class, byte Attribute, int n) {
         // Have a bug here.  Data is not always int
         bool successful = EIP.WriteOneAttribute(Class, Attribute, EIP.ToBytes((uint)n, Data.AttrDict[Class, Attribute].Set.Len));
      }

      // Set one attribute based on the Set Property
      private void ServiceAttribute(eipClassCode Class, byte Attribute, int n) {
         // Have a bug here.  Need to format the output.
         bool successful = EIP.ServiceAttribute(Class, Attribute, EIP.ToBytes((uint)n, Data.AttrDict[Class, Attribute].Service.Len));
      }

      // Only allow buttons if conditions are right to process the request
      public void SetButtonEnables() {
         // No need to set the button enables if this screen is not visible
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         cmdSaveAs.Enabled = XMLText.Length > 0;
      }

      // Examine the contents of a print message to determine its type
      private ItemType GetItemType(string text) {
         string[] s = text.Split('{');
         for (int i = 0; i < s.Length; i++) {
            int n = s[i].IndexOf('}');
            if (n >= 0) {
               for (int j = 0; j < n; j++) {
                  switch (s[i][j]) {
                     case 'C':
                        // Contains a counter character
                        // "{CCCC}"
                        return ItemType.Counter;
                     case 'Y':
                     case 'M':
                     case 'D':
                     case 'h':
                     case 'm':
                     case 's':
                     case 'T':
                     case 'W':
                     case '7':
                     case 'E':
                     case 'F':
                        // Contains a calendar character
                        // {{MM}/{DD}/{YY}| {hh}:[mm]:[ss}}
                        return ItemType.Date;
                     case 'X':
                     case 'Z':
                        // Contains a Fixed or Free layout user pattern
                        // {X/n} or {Z/n} where n is the character position
                        return ItemType.Logo;
                     case ' ':
                     case '\'':
                     case '.':
                     case ';':
                     case ':':
                     case '!':
                     case ',':
                        // Half size characters are treated as text
                        // {'}{.}{:}{,}{;}{!}{ }
                        break;
                  }
               }
            }
         }
         return ItemType.Text;
      }

      private string GetValue(XmlNode node, string DefaultValue) {
         try {
            return node.InnerText;
         } catch (Exception e) {
            //ErrorMessage = e.Message;
            return DefaultValue;
         }
      }

      private int GetAttr(XmlNode node, string AttrName, int DefaultValue) {
         try {
            return Convert.ToInt32(node.Attributes[AttrName].Value);
         } catch (Exception e) {
            //ErrorMessage = e.Message;
            return DefaultValue;
         }
      }

      private bool GetAttr(XmlNode node, string AttrName, bool DefaultValue) {
         bool result = DefaultValue;
         string s;
         try {
            s = node.Attributes[AttrName].Value;
            switch (s) {
               case "0":
                  result = false;
                  break;
               case "1":
                  result = true;
                  break;
               default:
                  result = bool.Parse(s);
                  break;
            }
         } catch (Exception e) {
            //ErrorMessage = e.Message;
         }
         return result;
      }

      private string GetAttr(XmlNode node, string AttrName, string DefaultValue) {
         try {
            return node.Attributes[AttrName].Value;
         } catch (Exception e) {
            //ErrorMessage = e.Message;
            return DefaultValue;
         }
      }

      #endregion

   }

}
