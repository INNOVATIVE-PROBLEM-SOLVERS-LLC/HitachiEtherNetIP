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
         Link = 5,     // Not supported in the printer
         Prompt = 6,   // Not supported in the printer
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
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
               EIP.ForwardOpen(true);
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
               EIP.ForwardClose(true);

               int itemCount = GetDecimalAttribute(ClassCode.Print_format, (byte)ccPF.Number_Of_Items);
               for (int i = 0; i < itemCount; i++) {
                  EIP.ForwardOpen(true);

                  SetAttribute(ClassCode.Index, (byte)ccIDX.Item, i + 1);
                  string text = GetAttribute(ClassCode.Print_format, (byte)ccPF.Print_Character_String);

                  itemType = GetItemType(text);

                  writer.WriteStartElement("Object"); // Start Object

                  writer.WriteAttributeString("Type", Enum.GetName(typeof(ItemType), itemType));

                  writer.WriteStartElement("Font"); // Start Font
                  {
                     writer.WriteAttributeString("HumanReadableFont", GetAttribute(ClassCode.Print_format, (byte)ccPF.Readable_Code));
                     writer.WriteAttributeString("EANPrefix", GetAttribute(ClassCode.Print_format, (byte)ccPF.Prefix_Code));
                     writer.WriteAttributeString("BarCode", GetAttribute(ClassCode.Print_format, (byte)ccPF.Barcode_Type));
                     writer.WriteAttributeString("IncreasedWidth", GetAttribute(ClassCode.Print_format, (byte)ccPF.Character_Bold));
                     writer.WriteAttributeString("InterLineSpace", GetAttribute(ClassCode.Print_format, (byte)ccPF.Line_Spacing));
                     writer.WriteAttributeString("InterCharacterSpace", GetAttribute(ClassCode.Print_format, (byte)ccPF.InterCharacter_Space));
                     writer.WriteString(GetAttribute(ClassCode.Print_format, (byte)ccPF.Dot_Matrix));
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

                  EIP.ForwardClose(true);
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
         writer.WriteAttributeString("Reset", GetAttribute(ClassCode.Count, (byte)ccCount.Reset_Value));
         //writer.WriteAttributeString("ExternalSignal", p.CtExternalSignal);
         //writer.WriteAttributeString("ResetSignal", p.CtResetSignal);
         writer.WriteAttributeString("CountUp", GetAttribute(ClassCode.Count, (byte)ccCount.Direction_Value));
         writer.WriteAttributeString("Increment", GetAttribute(ClassCode.Count, (byte)ccCount.Increment_Value));
         writer.WriteAttributeString("JumpTo", GetAttribute(ClassCode.Count, (byte)ccCount.Jump_To));
         writer.WriteAttributeString("JumpFrom", GetAttribute(ClassCode.Count, (byte)ccCount.Jump_From));
         writer.WriteAttributeString("UpdateUnit", GetAttribute(ClassCode.Count, (byte)ccCount.Update_Unit_Unit));
         writer.WriteAttributeString("UpdateIP", GetAttribute(ClassCode.Count, (byte)ccCount.Update_Unit_Halfway));
         writer.WriteAttributeString("Range2", GetAttribute(ClassCode.Count, (byte)ccCount.Count_Range_2));
         writer.WriteAttributeString("Range1", GetAttribute(ClassCode.Count, (byte)ccCount.Count_Range_1));
         writer.WriteAttributeString("InitialValue", GetAttribute(ClassCode.Count, (byte)ccCount.Initial_Value));
         //writer.WriteAttributeString("Format", p.RawText);
         writer.WriteAttributeString("Multiplier", GetAttribute(ClassCode.Count, (byte)ccCount.Count_Multiplier));
         writer.WriteAttributeString("ZeroSuppression", GetAttribute(ClassCode.Count, (byte)ccCount.Availibility_Of_Zero_Suppression));
         writer.WriteEndElement(); //  End Counter
      }

      private void WriteCalendarSettings(XmlTextWriter writer) {
         writer.WriteStartElement("Date"); // Start Date
         {
            //writer.WriteAttributeString("Format", p.RawText);

            writer.WriteStartElement("Offset"); // Start Offset
            {
               writer.WriteAttributeString("Minute", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Hour));
               writer.WriteAttributeString("Day", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Day));
               writer.WriteAttributeString("Month", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Month));
               writer.WriteAttributeString("Year", GetAttribute(ClassCode.Calendar, (byte)ccCal.Offset_Year));
            }
            writer.WriteEndElement(); // End Offset

            writer.WriteStartElement("ZeroSuppress"); // Start ZeroSuppress
            {
               writer.WriteAttributeString("DayOfWeek", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Day_Of_Week));
               writer.WriteAttributeString("Week", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Weeks));
               writer.WriteAttributeString("Minute", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Hour));
               writer.WriteAttributeString("Day", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Day));
               writer.WriteAttributeString("Month", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Month));
               writer.WriteAttributeString("Year", GetAttribute(ClassCode.Calendar, (byte)ccCal.Zero_Suppress_Year));
            }
            writer.WriteEndElement(); // End ZeroSuppress

            writer.WriteStartElement("EnableSubstitution"); // Start EnableSubstitution
            {
               //writer.WriteAttributeString("SubstitutionRule", p.DTSubRule);
               writer.WriteAttributeString("DayOfWeek", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Day_Of_Week));
               writer.WriteAttributeString("Week", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Week));
               writer.WriteAttributeString("Minute", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Minute));
               writer.WriteAttributeString("Hour", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Hour));
               writer.WriteAttributeString("Day", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Day));
               writer.WriteAttributeString("Month", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Month));
               writer.WriteAttributeString("Year", GetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Year));
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
               writer.WriteAttributeString("Model", GetAttribute(ClassCode.Unit_Information, (byte)ccUI.Model_Name));
            }
            writer.WriteAttributeString("Make", "Hitachi");

            writer.WriteStartElement("PrintHead");
            {
               writer.WriteAttributeString("Orientation", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Orientation));
            }
            writer.WriteEndElement(); // PrintHead

            writer.WriteStartElement("ContinuousPrinting");
            {
               writer.WriteAttributeString("RepeatInterval", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Repeat_Interval));
               writer.WriteAttributeString("PrintsPerTrigger", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Repeat_Count));
            }
            writer.WriteEndElement(); // ContinuousPrinting

            writer.WriteStartElement("TargetSensor");
            {
               writer.WriteAttributeString("Filter", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Target_Sensor_Filter));
               writer.WriteAttributeString("SetupValue", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Targer_Sensor_Filter_Value));
               writer.WriteAttributeString("Timer", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Target_Sensor_Timer));
            }
            writer.WriteEndElement(); // TargetSensor

            writer.WriteStartElement("CharacterSize");
            {
               writer.WriteAttributeString("Height", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Width));
               writer.WriteAttributeString("Width", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Character_Height));
            }
            writer.WriteEndElement(); // CharacterSize

            writer.WriteStartElement("PrintStartDelay");
            {
               writer.WriteAttributeString("Reverse", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Print_Start_Delay_Forward));
               writer.WriteAttributeString("Forward", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Print_Start_Delay_Reverse));
            }
            writer.WriteEndElement(); // PrintStartDelay

            writer.WriteStartElement("EncoderSettings");
            {
               writer.WriteAttributeString("HighSpeedPrinting", GetAttribute(ClassCode.Print_specification, (byte)ccPS.High_Speed_Print));
               writer.WriteAttributeString("Divisor", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Pulse_Rate_Division_Factor));
               writer.WriteAttributeString("ExternalEncoder", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Product_Speed_Matching));
            }
            writer.WriteEndElement(); // EncoderSettings

            writer.WriteStartElement("InkStream");
            {
               writer.WriteAttributeString("InkDropUse", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Ink_Drop_Use));
               writer.WriteAttributeString("ChargeRule", GetAttribute(ClassCode.Print_specification, (byte)ccPS.Ink_Drop_Charge_Rule));
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

      // Move xml string into the printer
      internal void LoadXMLFile(string xml) {
         int Version;
         ItemType type;
         XmlNode n;
         //   FormatSetup msgStyle;

         XmlDocument xmlDoc = new XmlDocument();
         xmlDoc.PreserveWhitespace = true;
         try {
            xmlDoc.LoadXml(xml);
            n = xmlDoc.SelectSingleNode("Label");
            //this.MessageName = GetAttr(n, "Name", Path.GetFileNameWithoutExtension(fileName));
            //this.Registration = GetAttr(n, "Registration", 1);
            //this.ClockSystem = GetAttr(n, "ClockSystem", "24-Hour");
            //this.BeRestrictive = GetAttr(n, "BeRestrictive", false);
            //this.UseHalfSpace = GetAttr(n, "UseHalfSpace", false);
            //this.MessageGroup = GetAttr(n, "GroupName", "");
            //this.MessageGroupNumber = GetAttr(n, "GroupNumber", "");
            //if (Enum.TryParse(GetAttr(n, "Format", "Individual"), out msgStyle)) {
            //   this.MessageStyle = msgStyle;
            //} else {
            //   this.MessageStyle = FormatSetup.Individual;
            //}
            Version = GetAttr(n, "Version", 1);

            PrinterSettings(xmlDoc.SelectSingleNode("Label/Printer"));

            foreach (System.Xml.XmlNode item in xmlDoc.SelectNodes("Label/Objects")[0].ChildNodes) {
               if (!(item is XmlWhitespace)) {
                  // New item must be created here
                  type = (ItemType)Enum.Parse(typeof(ItemType), GetAttr(item, "Type", "Text"), true);
                  n = item.SelectSingleNode("Location");
                  int x = GetAttr(n, "Left", 0);
                  int y = GetAttr(n, "Top", 0) - GetAttr(n, "Height", 0);
                  int r = GetAttr(n, "Row", -1);
                  int c = GetAttr(n, "Column", -1);

                  n = item.SelectSingleNode("Font");
                  string F = n.InnerText;
                  int ICS = GetAttr(n, "InterCharacterSpace", 1);
                  int ILS = GetAttr(n, "InterLineSpace", 1);
                  int IW = GetAttr(n, "IncreasedWidth", 1);

                  //p = new TPB(this, type, x, y, F, ICS, ILS, IW);

                  //p.Row = r;
                  //p.Column = c;

                  //p.BarCode = GetAttr(n, "BarCode", "(None)");
                  //p.HumanReadableFont = GetAttr(n, "HumanReadableFont", "(None)");
                  //p.EANPrefix = GetAttr(n, "EANPrefix", "00");

                  // Done to here
                  switch (type) {
                     case ItemType.Counter:
                        CountSettings(item);
                        break;
                     case ItemType.Date:
                        CalendarSettings(item);
                        break;
                     case ItemType.Logo:
                        LogoSettings(item);
                        break;
                     case ItemType.Text:
                        n = item.SelectSingleNode("Text");
                        //p.RawText = GetValue(n, "<TEXT>");
                        break;
                  }
               }
            }
         } catch (Exception ex2) {
            MessageBox.Show($"Invalid file format for file \"{LoadedFileName}\"");
            //LogIt("Invalid file format for file \"" + fileName + "\"\r\n" + ex2.ToString());
         }
      }

      // Load printer wide settings
      private void PrinterSettings(XmlNode pr) {
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  SetAttribute(ClassCode.Print_specification,
                              (byte)ccPS.Character_Orientation,
                              GetAttr(c, "Orientation"));
                  //this.CharacterOrientation = GetAttr(c, "Orientation", "0");
                  break;
               case "ContinuousPrinting":
                  SetAttribute(ClassCode.Print_specification,
                              (byte)ccPS.Repeat_Interval,
                              GetAttr(c, "RepeatInterval"));
                  //this.RepeatInterval = GetAttr(c, "RepeatInterval", "0000");
                  SetAttribute(ClassCode.Print_specification,
                              (byte)ccPS.Repeat_Count,
                              GetAttr(c, "PrintsPerTrigger"));
                  //this.PrintsPerTrigger = GetAttr(c, "PrintsPerTrigger", "0000");
                  break;
               case "TargetSensor":
                  SetAttribute(ClassCode.Print_specification,
                             (byte)ccPS.Target_Sensor_Filter,
                             GetAttr(c, "Filter"));
                  //this.TargetSensorFilter = GetAttr(c, "Filter", "2");
                  SetAttribute(ClassCode.Print_specification,
                             (byte)ccPS.Targer_Sensor_Filter_Value,
                             GetAttr(c, "SetupValue"));
                  //this.TargetSensorSetupValue = GetAttr(c, "SetupValue", "0050");
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Target_Sensor_Timer,
                            GetAttr(c, "Timer"));
                  //this.TargetSensorTimer = GetAttr(c, "Timer", "000");
                  break;
               case "CharacterSize":
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Character_Width,
                            GetAttr(c, "Width"));
                  //this.CharacterWidth = GetAttr(c, "Width", "010");
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Character_Height,
                            GetAttr(c, "Height"));
                  //this.CharacterHeight = GetAttr(c, "Height", "70");
                  break;
               case "PrintStartDelay":
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Print_Start_Delay_Reverse,
                            GetAttr(c, "Reverse"));
                  //this.ReverseDelay = GetAttr(c, "Reverse", "0000");
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Print_Start_Delay_Forward,
                            GetAttr(c, "Forward"));
                  //this.ForwardDelay = GetAttr(c, "Forward", "0000");
                  break;
               case "EncoderSettings":
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.High_Speed_Print,
                            GetAttr(c, "HighSpeedPrinting"));
                  //this.HighSpeedPrinting = GetAttr(c, "HighSpeedPrinting", "0");
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Pulse_Rate_Division_Factor,
                            GetAttr(c, "Divisor"));
                  //this.Divisor = GetAttr(c, "Divisor", "001");
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Product_Speed_Matching,
                            GetAttr(c, "ExternalEncoder"));
                  //this.ExternalEncoder = GetAttr(c, "ExternalEncoder", false);
                  break;
               case "InkStream":
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Ink_Drop_Use,
                            GetAttr(c, "InkDropUse"));
                  //this.InkDropUse = GetAttr(c, "InkDropUse", "03");
                  SetAttribute(ClassCode.Print_specification,
                            (byte)ccPS.Ink_Drop_Charge_Rule,
                            GetAttr(c, "ChargeRule"));
                  //this.InkDropChargeRule = GetAttr(c, "ChargeRule", InkDropChargeRules.Standard);
                  break;
            }
         }
      }

      // Load Count Block
      private void CountSettings(XmlNode item) {
         XmlNode c;
         c = item.SelectSingleNode("Text");
         //SetAttribute(eipClassCode.Count,
         //            (byte)eipCount.Character_Orientation,
         //            GetAttr(c, "Orientation"));
         //p.RawText = GetValue(n, "{0000}");

         c = item.SelectSingleNode("Counter");
         // Must set length before any other attribute
         string initValue = GetAttr(c, "InitialValue", "0000");
         //p.CtWidth = initValue.Length;
         //p.CtInitialValue = initValue;
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Count_Range_1,
                     GetAttr(c, "Range1"));
         //p.CtRangeStart = GetAttr(n, "Range1", "0000");
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Count_Range_2,
                     GetAttr(c, "Range2"));
         //p.CtRangeEnd = GetAttr(n, "Range2", "9999");
         SetAttribute(ClassCode.Count,
                   (byte)ccCount.Update_Unit_Halfway,
                   GetAttr(c, "UpdateIP"));
         //p.CtUpdateIP = GetAttr(n, "UpdateIP", "0000");
         SetAttribute(ClassCode.Count,
                  (byte)ccCount.Update_Unit_Unit,
                  GetAttr(c, "UpdateUnit"));
         //p.CtUpdateUnit = GetAttr(n, "UpdateUnit", "0001");
         SetAttribute(ClassCode.Count,
                    (byte)ccCount.Jump_From,
                    GetAttr(c, "JumpFrom"));
         //p.CtJumpFrom = GetAttr(n, "JumpFrom", "9999");
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Jump_To,
                     GetAttr(c, "JumpTo"));
         //p.CtJumpTo = GetAttr(n, "JumpTo", "0000");
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Increment_Value,
                     GetAttr(c, "Increment"));
         //p.CtIncrement = GetAttr(n, "Increment", "01");
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Direction_Value,
                     GetAttr(c, "CountUp"));
         //p.CtDirection = GetAttr(n, "CountUp", true);
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Type_Of_Reset_Signal,
                     GetAttr(c, "Reset"));
         //p.CtReset = GetAttr(n, "Reset", "0");
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Count_Multiplier,
                     GetAttr(c, "Multiplier"));
         //p.CtMultiplier = GetAttr(n, "Multiplier", "0001");
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Availibility_Of_Zero_Suppression,
                     GetAttr(c, "ZeroSuppression"));
         //p.CtZeroSuppression = GetAttr(n, "ZeroSuppression", false);
         SetAttribute(ClassCode.Count,
                     (byte)ccCount.Type_Of_Reset_Signal,
                     GetAttr(c, "ResetSignal"));
         //p.CtResetSignal = GetAttr(n, "ResetSignal", "0");
         SetAttribute(ClassCode.Count,
                    (byte)ccCount.Availibility_Of_External_Count,
                    GetAttr(c, "ExternalSignal"));
         //p.CtExternalSignal = GetAttr(n, "ExternalSignal", "0");
      }

      // Load Logo
      private void LogoSettings(XmlNode item) {
         XmlNode n;
         n = item.SelectSingleNode("Logo");
         //p.LogoFilter = GetAttr(n, "Filter", 84);
         //p.LogoReverseVideo = GetAttr(n, "ReverseVideo", false);
         //p.LogoSource = GetAttr(n, "Source", "");
         //p.LogoLength = GetAttr(n, "LogoLength", 1);
         //p.LogoRegistration = GetAttr(n, "Registration", -1);
         //p.LogoSource = GetAttr(n, "Source", "");
         //p.ScaledImage = GetLogoScaledImage(p, n);
         n = item.SelectSingleNode("Text");
         //if (n == null) {
         //   p.LogoItemText = string.Empty;
         //} else {
         //   string ItemText = GetValue(n, "<TEXT>");
         //   string it = "";
         //   if (ItemText.Length > 0) {
         //      for (int i = 0; i < ItemText.Length; i += 4) {
         //         it += (char)Convert.ToInt16(ItemText.Substring(i, 4), 16);
         //      }
         //      p.LogoItemText = it;
         //   }
         //}
      }

      // Load Calendar Block
      private void CalendarSettings(XmlNode item) {
         XmlNode n;
         n = item.SelectSingleNode("Date");
         //p.RawText = GetAttr(n, "Format", item.SelectSingleNode("Text").InnerText);

         n = item.SelectSingleNode("Date/Offset");
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Offset_Year,
                   GetAttr(n, "Year"));
         //p.DtYearOffset = GetAttr(n, "Year", "0000");
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Offset_Month,
                   GetAttr(n, "Month"));
         //p.DtMonthOffset = GetAttr(n, "Month", "0000");
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Offset_Day,
                   GetAttr(n, "Day"));
         //p.DtDayOffset = GetAttr(n, "Day", "0000");
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Offset_Hour,
                   GetAttr(n, "Hour"));
         //p.DtHourOffset = GetAttr(n, "Hour", "0000");
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Offset_Minute,
                   GetAttr(n, "Minute"));
         //p.DtMinuteOffset = GetAttr(n, "Minute", "0000");

         n = item.SelectSingleNode("Date/ZeroSuppress");
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Zero_Suppress_Year,
                   GetAttr(n, "Year"));
         //p.DtYearZS = GetAttr(n, "Year", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Zero_Suppress_Month,
                   GetAttr(n, "Month"));
         //p.DtMonthZS = GetAttr(n, "Month", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Zero_Suppress_Day,
                   GetAttr(n, "Day"));
         //p.DtDayZS = GetAttr(n, "Day", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Zero_Suppress_Hour,
                   GetAttr(n, "Hour"));
         //p.DtHourZS = GetAttr(n, "Hour", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Zero_Suppress_Minute,
                   GetAttr(n, "Minute"));
         //p.DtMinuteZS = GetAttr(n, "Minute", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Zero_Suppress_Weeks,
                   GetAttr(n, "Week"));
         //p.DtWeekZS = GetAttr(n, "Week", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Zero_Suppress_Day_Of_Week,
                   GetAttr(n, "DayOfWeek"));
         //p.DtDayOfWeekZS = GetAttr(n, "DayOfWeek", false);

         n = item.SelectSingleNode("Date/EnableSubstitution");
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Substitute_Rule_Year,
                   GetAttr(n, "Year"));
         //p.DtYearSub = GetAttr(n, "Year", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Substitute_Rule_Month,
                   GetAttr(n, "Month"));
         //p.DtMonthSub = GetAttr(n, "Month", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Substitute_Rule_Day,
                   GetAttr(n, "Day"));
         //p.DtDaySub = GetAttr(n, "Day", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Substitute_Rule_Hour,
                   GetAttr(n, "Hour"));
         //p.DtHourSub = GetAttr(n, "Hour", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Substitute_Rule_Minute,
                   GetAttr(n, "Minute"));
         //p.DtMinuteSub = GetAttr(n, "Minute", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Substitute_Rule_Weeks,
                   GetAttr(n, "Week"));
         //p.DtWeekSub = GetAttr(n, "Week", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Substitute_Rule_Day_Of_Week,
                   GetAttr(n, "DayOfWeek"));
         //p.DtDayOfWeekSub = GetAttr(n, "DayOfWeek", false);
         SetAttribute(ClassCode.Calendar,
                   (byte)ccCal.Calendar_Block_Number_In_Item,
                   GetAttr(n, "SubstitutionRule"));
         //p.DTSubRule = GetAttr(n, "SubstitutionRule", "01");
      }

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
      private string GetAttribute(ClassCode Class, byte Attribute) {
         // <TODO> Some data may be required in place of NoData
         bool successful = EIP.ReadOneAttribute(Class, Attribute, EIP.Nodata, out string val);
         return val;
      }

      // Get the value of an attribute that is known to be a decimal number
      private int GetDecimalAttribute(ClassCode Class, byte Attribute) {
         GetAttribute(Class, Attribute);
         return EIP.GetDecValue;
      }

      // Set one attribute based on the Set Property
      private void SetAttribute(ClassCode Class, byte Attribute, int n) {
         AttrData attr = DataII.AttrDict[Class, Attribute];
         byte[] data = EIP.ToBytes((uint)n, attr.Set.Len);
         bool successful = EIP.WriteOneAttribute(Class, Attribute, data);
      }

      // Set one attribute based on the Set Property
      private void SetAttribute(ClassCode Class, byte Attribute, string s) {
         AttrData attr = DataII.AttrDict[Class, Attribute];
         byte[] data = EIP.FormatOutput(s, attr.Set);
         bool successful = EIP.WriteOneAttribute(Class, Attribute, data);
      }

      // Set one attribute based on the Set Property
      private void ServiceAttribute(ClassCode Class, byte Attribute, int n) {
         // <TODO> Need to format the output.
         bool successful = EIP.ServiceAttribute(Class, Attribute, EIP.ToBytes((uint)n, DataII.AttrDict[Class, Attribute].Service.Len));
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
                        // {{MM}/{DD}/{YY} {hh}:[mm]:[ss}}
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

      private string GetAttr(XmlNode node, string AttrName, string DefaultValue = "?") {
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
