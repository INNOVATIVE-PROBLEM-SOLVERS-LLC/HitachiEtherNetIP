using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EIP_Lib {
   public partial class XML {

      #region Data Declarations

      Browser parent;

      EIP EIP;
      TabPage tab;

      // Tab Controls
      TabControl tclViewXML;
      TabPage tabTreeView;
      TabPage tabIndented;

      TreeView tvXML;
      TextBox txtIndentedView;

      // Operating Buttons
      Button cmdOpen;
      Button cmdClear;
      Button cmdGenerate;
      Button cmdSaveAs;
      Button cmdDeleteAll;
      Button cmdSaveInPrinter;

      // Testing Buttons
      Label lblSelectXmlTest;
      ComboBox cbAvailableXmlTests;
      Button cmdBrowse;
      Button cmdSendToPrinter;

      Label lblSelectHardTest;
      ComboBox cbAvailableHardTests;
      Button cmdRunHardTest;

      // XML Processing
      string XMLText = string.Empty;
      XmlDocument xmlDoc = null;
      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
         Link = 5,     // Not supported in the printer
         Prompt = 6,   // Not supported in the printer
      }

      // Make things easier to read
      Font courier = new Font("Courier New", 9);

      // Flag for Attribute Not Present
      const string N_A = "N!A";

      // Braced Characters (count, date, half-size, logos
      char[] bc = new char[] { 'C', 'Y', 'M', 'D', 'h', 'm', 's', 'T', 'W', '7', 'E', 'F', ' ', '\'', '.', ';', ':', '!', ',', 'X', 'Z' };
      // Attributes of braced characters
      enum ba {
         Count = 1 << 0,
         Year = 1 << 1,
         Month = 1 << 2,
         Day = 1 << 3,
         Hour = 1 << 4,
         Minute = 1 << 5,
         Second = 1 << 6,
         Julian = 1 << 7,
         Week = 1 << 8,
         DayOfWeek = 1 << 9,
         Shift = 1 << 10,
         TimeCount = 1 << 11,
         Space = 1 << 12,
         Quote = 1 << 13,
         Period = 1 << 14,
         SemiColon = 1 << 15,
         Colon = 1 << 16,
         Exclamation = 1 << 17,
         Comma = 1<< 18,
         FixedPattern = 1 << 19,
         FreePattern = 1 << 20,
         Unknown = 1 << 21,
         DateCode = (1 << 12) - 2, // All the date codes combined

      }
      #endregion

      #region Constructors and destructors

      // Create class
      public XML(Browser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;

         BuildControls();

         BuildTestFileList();

         SetButtonEnables();
      }

      #endregion

      #region Form Control Events

      // Open a new XML file
      private void Open_Click(object sender, EventArgs e) {
         // Clear out any currently loaded file
         Clear_Click(null, null);
         DialogResult dlgResult = DialogResult.Retry;
         string fileName = String.Empty;
         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
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
         SetButtonEnables();
      }

      // Clear out the screens
      private void Clear_Click(object sender, EventArgs e) {
         txtIndentedView.Text = string.Empty;
         xmlDoc = null;
         tvXML.Nodes.Clear();
         XMLText = string.Empty;
         SetButtonEnables();
      }

      // Save the generated XML file
      private void SaveAs_Click(object sender, EventArgs e) {
         DialogResult dlgResult;
         string filename = string.Empty;

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
               using (Stream outfs = new FileStream(filename, FileMode.Create)) {
                  // Might have some possibilities here <TODO>
                  outfs.Write(EIP.Encode.GetBytes(XMLText), 0, XMLText.Length);
                  outfs.Flush();
                  outfs.Close();
                  SetButtonEnables();
               }
            }
         }
         SetButtonEnables();
      }

      #endregion

      #region XML Save Routines

      #endregion

      #region Send to Printer Routines

      // Send xlmDoc to printer
      private void SendToPrinter_Click(object sender, EventArgs e) {
         // Need a XMP Document to continue
         if (xmlDoc == null) {
            Open_Click(null, null);
            if (xmlDoc == null) {
               return;
            }
         }
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Set to only one item in printer
               CleanUpDisplay();
               XmlNode lab = xmlDoc.SelectSingleNode("Label");
               foreach (XmlNode l in lab.ChildNodes) {
                  if (l is XmlWhitespace)
                     continue;
                  switch (l.Name) {
                     case "Printer":
                        // Send printer wide settings
                        SendPrinterSettings(l);
                        break;
                     case "Objects":
                        // Send the objects one at a time
                        SendObjectSettings(l.ChildNodes);
                        break;
                  }
                  //// Send printer wide settings
                  //SendPrinterSettings(xmlDoc.SelectSingleNode("Label/Printer"));
                  //// Send the objects one at a time
                  //SendObjectSettings(xmlDoc.SelectNodes("Label/Objects")[0].ChildNodes);
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      // Send the Printer Wide Settings
      private void SendPrinterSettings(XmlNode pr) {
         success = true;
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  EIP.SetAttribute(ccPS.Character_Orientation, GetAttr(c, "Orientation"));
                  break;
               case "ContinuousPrinting":
                  EIP.SetAttribute(ccPS.Repeat_Interval, GetAttr(c, "RepeatInterval"));
                  EIP.SetAttribute(ccPS.Repeat_Count, GetAttr(c, "PrintsPerTrigger"));
                  break;
               case "TargetSensor":
                  EIP.SetAttribute(ccPS.Target_Sensor_Filter, GetAttr(c, "Filter"));
                  EIP.SetAttribute(ccPS.Targer_Sensor_Filter_Value, GetAttr(c, "SetupValue"));
                  EIP.SetAttribute(ccPS.Target_Sensor_Timer, GetAttr(c, "Timer"));
                  break;
               case "CharacterSize":
                  EIP.SetAttribute(ccPS.Character_Width, GetAttr(c, "Width"));
                  EIP.SetAttribute(ccPS.Character_Width, GetAttr(c, "Height"));
                  break;
               case "PrintStartDelay":
                  EIP.SetAttribute(ccPS.Print_Start_Delay_Reverse, GetAttr(c, "Reverse"));
                  EIP.SetAttribute(ccPS.Print_Start_Delay_Forward, GetAttr(c, "Forward"));
                  break;
               case "EncoderSettings":
                  EIP.SetAttribute(ccPS.High_Speed_Print, GetAttr(c, "HighSpeedPrinting"));
                  EIP.SetAttribute(ccPS.Pulse_Rate_Division_Factor, GetAttr(c, "Divisor"));
                  EIP.SetAttribute(ccPS.Product_Speed_Matching, GetAttr(c, "ExternalEncoder"));
                  break;
               case "InkStream":
                  EIP.SetAttribute(ccPS.Ink_Drop_Use, GetAttr(c, "InkDropUse"));
                  EIP.SetAttribute(ccPS.Ink_Drop_Charge_Rule, GetAttr(c, "ChargeRule"));
                  break;
               case "TwinNozzle":
                  // Not supported in EtherNet/IP
                  //this.LeadingCharacterControl = GetAttr(c, "LeadingCharControl", 0);
                  //this.LeadingCharacterControlWidth1 = GetAttr(c, "LeadingCharControlWidth1", 32);
                  //this.LeadingCharacterControlWidth1 = GetAttr(c, "LeadingCharControlWidth2", 32);
                  //this.NozzleSpaceAlignment = GetAttr(c, "NozzleSpaceAlignment", 0);
                  break;
               case "Substitution":
                  SendSubstitution(c);
                  break;
               case "Calendar":
                  SendCalendar(c);
                  break;
               case "Count":
                  SendCounter(c);
                  break;
            }
         }
      }

      // Set all the values for a single substitution rule
      private void SendSubstitution(XmlNode p) {
         AttrData attr;
         byte[] data;

         // Get the standard attributes for substitution
         string rule = GetAttr(p, "Rule");
         string startYear = GetAttr(p, "StartYear");
         string delimeter = GetAttr(p, "Delimeter");

         // Avoid user errors
         if (int.TryParse(rule, out int ruleNumber) && int.TryParse(startYear, out int year) && delimeter.Length == 1) {

            // Sub Substitution rule in Index class
            attr = EIP.AttrDict[ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting];
            data = EIP.FormatOutput(attr.Set, ruleNumber);
            EIP.SetAttribute(ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting, data);

            // Set the start year in the substitution rule
            attr = EIP.AttrDict[ClassCode.Index, (byte)ccSR.Start_Year];
            data = EIP.FormatOutput(attr.Set, year);
            EIP.SetAttribute(ClassCode.Substitution_rules, (byte)ccSR.Start_Year, data);

            // Load the individual rules
            foreach (XmlNode c in p.ChildNodes) {
               switch (c.Name) {
                  case "Year":
                     SetSubValues(ccSR.Year, c, delimeter);
                     break;
                  case "Month":
                     SetSubValues(ccSR.Month, c, delimeter);
                     break;
                  case "Day":
                     SetSubValues(ccSR.Day, c, delimeter);
                     break;
                  case "Hour":
                     SetSubValues(ccSR.Hour, c, delimeter);
                     break;
                  case "Minute":
                     SetSubValues(ccSR.Minute, c, delimeter);
                     break;
                  case "Week":
                     SetSubValues(ccSR.Week, c, delimeter);
                     break;
                  case "DayOfWeek":
                     SetSubValues(ccSR.Day_Of_Week, c, delimeter);
                     break;
                  case "Skip":
                     // Do not process these nodes
                     break;
               }
            }
         }
      }

      // Send Date related information
      private void SendCalendar(XmlNode d) {

         XmlNode n = d.SelectSingleNode("Offset");
         if (n != null) {
            EIP.SetAttribute(ccCal.Offset_Year, GetAttr(n, "Year"));
            EIP.SetAttribute(ccCal.Offset_Month, GetAttr(n, "Month"));
            EIP.SetAttribute(ccCal.Offset_Day, GetAttr(n, "Day"));
            EIP.SetAttribute(ccCal.Offset_Hour, GetAttr(n, "Hour"));
            EIP.SetAttribute(ccCal.Offset_Minute, GetAttr(n, "Minute"));
         }

         n = d.SelectSingleNode("ZeroSuppress");
         if (n != null) {
            EIP.SetAttribute(ccCal.Zero_Suppress_Year, GetAttr(n, "Year"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Month, GetAttr(n, "Month"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Day, GetAttr(n, "Day"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Hour, GetAttr(n, "Hour"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Minute, GetAttr(n, "Minute"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Weeks, GetAttr(n, "Week"));
            EIP.SetAttribute(ccCal.Zero_Suppress_Day_Of_Week, GetAttr(n, "DayOfWeek"));
         }

         n = d.SelectSingleNode("EnableSubstitution");
         if (n != null) {
            EIP.SetAttribute(ccCal.Substitute_Year, GetAttr(n, "Year"));
            EIP.SetAttribute(ccCal.Substitute_Month, GetAttr(n, "Month"));
            EIP.SetAttribute(ccCal.Substitute_Day, GetAttr(n, "Day"));
            EIP.SetAttribute(ccCal.Substitute_Hour, GetAttr(n, "Hour"));
            EIP.SetAttribute(ccCal.Substitute_Minute, GetAttr(n, "Minute"));
            EIP.SetAttribute(ccCal.Substitute_Weeks, GetAttr(n, "Week"));
            EIP.SetAttribute(ccCal.Substitute_Day_Of_Week, GetAttr(n, "DayOfWeek"));
         }

         n = d.SelectSingleNode("TimeCount");
         if (n != null) {
            EIP.SetAttribute(ccCal.Time_Count_Start_Value, GetAttr(n, "Start"));
            EIP.SetAttribute(ccCal.Time_Count_End_Value, GetAttr(n, "End"));
            EIP.SetAttribute(ccCal.Time_Count_Reset_Value, GetAttr(n, "Reset"));
            EIP.SetAttribute(ccCal.Reset_Time_Value, GetAttr(n, "ResetTime"));
            EIP.SetAttribute(ccCal.Update_Interval_Value, GetAttr(n, "RenewalPeriod"));
         }

         n = d.SelectSingleNode("Shift");
         if (n != null) {
            EIP.SetAttribute(ccIDX.Item, GetAttr(n, "Number"));
            EIP.SetAttribute(ccCal.Shift_Start_Hour, GetAttr(n, "StartHour"));
            EIP.SetAttribute(ccCal.Shift_Start_Minute, GetAttr(n, "StartMinute"));
            EIP.SetAttribute(ccCal.Shift_End_Hour, GetAttr(n, "EndHour"));
            EIP.SetAttribute(ccCal.Shift_End_Minute, GetAttr(n, "EndMinute"));
         }
      }

      // Send counter related information
      private void SendCounter(XmlNode c) {
         if (c != null) {
            EIP.SetAttribute(ccCount.Initial_Value, GetAttr(c, "InitialValue"));
            EIP.SetAttribute(ccCount.Count_Range_1, GetAttr(c, "Range1"));
            EIP.SetAttribute(ccCount.Count_Range_2, GetAttr(c, "Range2"));
            EIP.SetAttribute(ccCount.Update_Unit_Halfway, GetAttr(c, "UpdateIP"));
            EIP.SetAttribute(ccCount.Update_Unit_Unit, GetAttr(c, "UpdateUnit"));
            EIP.SetAttribute(ccCount.Increment_Value, GetAttr(c, "Increment"));
            string s = bool.TryParse(GetAttr(c, "CountUp"), out bool b) && !b ? "Down" : "Up";
            EIP.SetAttribute(ccCount.Direction_Value, s);
            EIP.SetAttribute(ccCount.Jump_From, GetAttr(c, "JumpFrom"));
            EIP.SetAttribute(ccCount.Jump_To, GetAttr(c, "JumpTo"));
            EIP.SetAttribute(ccCount.Reset_Value, GetAttr(c, "Reset"));
            EIP.SetAttribute(ccCount.Type_Of_Reset_Signal, GetAttr(c, "ResetSignal"));
            EIP.SetAttribute(ccCount.External_Count, GetAttr(c, "ExternalSignal"));
            EIP.SetAttribute(ccCount.Zero_Suppression, GetAttr(c, "ZeroSuppression"));
            EIP.SetAttribute(ccCount.Count_Multiplier, GetAttr(c, "Multiplier"));
            EIP.SetAttribute(ccCount.Count_Skip, GetAttr(c, "Skip"));
         }
      }

      // Set the substitution values for a class
      private void SetSubValues(ccSR attribute, XmlNode c, string delimeter) {
         // Avoid user errors
         if (int.TryParse(GetAttr(c, "Base"), out int b)) {
            Prop prop = EIP.AttrDict[ClassCode.Substitution_rules, (byte)attribute].Set;
            string[] s = GetValue(c).Split(delimeter[0]);
            for (int i = 0; i < s.Length && success; i++) {
               int n = b + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  byte[] data = EIP.FormatOutput(prop, n, 1, s[i]);
                  success = EIP.SetAttribute(ClassCode.Substitution_rules, (byte)attribute, data);
               }
            }
         }
      }

      // Send the individual objects
      private void SendObjectSettings(XmlNodeList objs) {
         success = true;
         ItemType type;
         XmlNode n;
         int count = 1;
         int calendar = 1;
         int item = 1;
         foreach (XmlNode obj in objs) {
            if (obj is XmlWhitespace)
               continue;
            // Get the item type
            type = (ItemType)Enum.Parse(typeof(ItemType), GetAttr(obj, "Type"), true);
            // Handle multiple line texts
            string[] text = GetValue(obj.SelectSingleNode("Text")).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < text.Length; i++) {
               // Printer always has one item
               if (item > 1) {
                  // Add an item <TODO> Need to add item, not column
                  EIP.ServiceAttribute(ccPF.Add_Column, 0);
               }

               // Point to the item
               EIP.SetAttribute(ccIDX.Item, item);

               // Set the common parameters
               n = obj.SelectSingleNode("Location");
               //x = GetAttr(n, "Left", 0);
               //y = GetAttr(n, "Top", 0) - GetAttr(n, "Height", 0);
               //int r = GetAttr(n, "Row", -1);
               //int c = GetAttr(n, "Column", -1);

               n = obj.SelectSingleNode("Font");
               EIP.SetAttribute(ccPF.Dot_Matrix, n.InnerText);
               EIP.SetAttribute(ccPF.InterCharacter_Space, GetAttr(n, "InterCharacterSpace"));
               EIP.SetAttribute(ccPF.Line_Spacing, GetAttr(n, "InterLineSpace"));
               EIP.SetAttribute(ccPF.Character_Bold, GetAttr(n, "IncreasedWidth"));


               //p = new TPB(this, type, x, y, F, ICS, ILS, IW);

               //p.Row = r;
               //p.Column = c;

               //p.BarCode = GetAttr(n, "BarCode", "(None)");
               //p.HumanReadableFont = GetAttr(n, "HumanReadableFont", "(None)");
               //p.EANPrefix = GetAttr(n, "EANPrefix", "00");

               switch (type) {
                  case ItemType.Text:
                     EIP.SetAttribute(ccPF.Print_Character_String, text[i]);
                     break;
                  case ItemType.Counter:
                     EIP.SetAttribute(ccIDX.Count_Block, count++);
                     EIP.SetAttribute(ccPF.Print_Character_String, FormatCounter(text[i]));
                     SendCounter(obj.SelectSingleNode("Counter"));
                     break;
                  case ItemType.Date:
                     EIP.SetAttribute(ccIDX.Calendar_Block, calendar++);
                     EIP.SetAttribute(ccPF.Print_Character_String, FormatDate(text[i]));
                     n = obj.SelectSingleNode("Date");
                     if (n != null) {
                        SendCalendar(n);
                     }
                     break;
                  case ItemType.Logo:
                     break;
                  case ItemType.Link:
                     break;
                  case ItemType.Prompt:
                     break;
                  default:
                     break;
               }
               item++;
            }
         }
      }

      // Convert from cijConnect format to Hitachi format
      private string FormatCounter(string text) {
         string result = text;
         if (text.IndexOf("{{") < 0) {
            int lBrace = text.IndexOf('{');
            int rBrace = text.LastIndexOf('}');
            if (lBrace >= 0 && lBrace < rBrace) {
               result = text.Substring(0, lBrace) + "{{" + new string('C', rBrace - lBrace - 1) + "}}" + text.Substring(rBrace + 1);
            }
         }
         return result;
      }

      // Convert from cijConnect format to Hitachi format
      private string FormatDate(string text) {
         string result = text;
         if (text.IndexOf("{{") < 0) {
            int lBrace = text.IndexOf('{');
            int rBrace = text.LastIndexOf('}');
            if (lBrace >= 0 && lBrace < rBrace) {
               result = text.Substring(0, lBrace) + "{{" + text.Substring(lBrace + 1, rBrace - lBrace - 1) + "}}" + text.Substring(rBrace + 1);
            }
         }
         return result;
      }

      #endregion

      #region Service Routines

      // Build XML page controls
      private void BuildControls() {
         tclViewXML = new TabControl() { Name = "tclViewXML", Font = courier };
         tabTreeView = new TabPage() { Name = "tabTreeView", Text = "Tree View" };
         tabIndented = new TabPage() { Name = "tabIndented", Text = "Indented View" };

         tvXML = new TreeView() { Name = "tvXML", Font = courier };
         txtIndentedView = new TextBox() { Name = "txtIndentedView", Multiline = true, ScrollBars = ScrollBars.Both };

         cmdOpen = new Button() { Text = "Open" };
         cmdOpen.Click += Open_Click;

         cmdClear = new Button() { Text = "Clear" };
         cmdClear.Click += Clear_Click;

         cmdGenerate = new Button() { Text = "Generate" };
         cmdGenerate.Click += Generate_Click;

         cmdSaveAs = new Button() { Text = "Save As" };
         cmdSaveAs.Click += SaveAs_Click;

         cmdSendToPrinter = new Button() { Text = "Send To Printer" };
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

         // Testing controls
         lblSelectXmlTest = new Label() { Text = "Select XML Test", TextAlign = ContentAlignment.BottomCenter };
         cbAvailableXmlTests = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         cbAvailableXmlTests.SelectedIndexChanged += cbAvailableTests_SelectedIndexChanged;
         cmdBrowse = new Button() { Text = "Browse" };
         cmdBrowse.Click += cmdBrowse_Click;

         lblSelectHardTest = new Label() { Text = "Select Hard Test", TextAlign = ContentAlignment.BottomCenter };
         cbAvailableHardTests = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         cbAvailableHardTests.Items.AddRange(
            new string[] { "Reset", "Shift Code", "Month Day SR", "Time Count", "Day of Week etc", "MDY hms", "Multi-Line", "Counter", "???" }
            );
         cbAvailableHardTests.SelectedIndexChanged += CbAvailableHardTests_SelectedIndexChanged;
         cmdRunHardTest = new Button() { Text = "Run Test" };
         cmdRunHardTest.Click += cmdRunHardTest_Click;

         cmdDeleteAll = new Button() { Text = "Delete All" };
         cmdSaveInPrinter = new Button() { Text = "Save In Printer" };

         cmdDeleteAll.Click += cmdDeleteAll_Click;
         cmdSaveInPrinter.Click += cmdSaveToPrinter_Click;

         tab.Controls.Add(lblSelectXmlTest);
         tab.Controls.Add(lblSelectHardTest);
         tab.Controls.Add(cbAvailableXmlTests);
         tab.Controls.Add(cbAvailableHardTests);
         tab.Controls.Add(cmdDeleteAll);
         tab.Controls.Add(cmdBrowse);
         tab.Controls.Add(cmdSaveInPrinter);
         tab.Controls.Add(cmdRunHardTest);
      }

      private void CbAvailableHardTests_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // Called from parent
      public void ResizeControls(ref ResizeInfo R) {
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         int tclWidth = (int)(tab.ClientSize.Width / R.W);
         float offset = (int)(tab.ClientSize.Height - tclHeight * R.H);
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         R.offset = offset;
         Utils.ResizeObject(ref R, tclViewXML, 0, 1, tclHeight - 7, tclWidth - 1);
         {
            Utils.ResizeObject(ref R, tvXML, 1, 1, tclHeight - 12, tclWidth - 3);
            Utils.ResizeObject(ref R, txtIndentedView, 1, 1, tclHeight - 12, tclWidth - 3);

            Utils.ResizeObject(ref R, cmdDeleteAll, tclHeight - 6, 1, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdOpen, tclHeight - 3, 1, 2.5f, 4);

            Utils.ResizeObject(ref R, cmdSaveAs, tclHeight - 6, 5.5f, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdClear, tclHeight - 3, 5.5f, 2.5f, 4);

            Utils.ResizeObject(ref R, cmdGenerate, tclHeight - 6, 10, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdSaveInPrinter, tclHeight - 3, 10, 2.5f, 4);

            Utils.ResizeObject(ref R, lblSelectXmlTest, tclHeight - 6, 14.5f, 1, 6);
            Utils.ResizeObject(ref R, cbAvailableXmlTests, tclHeight - 5, 14.5f, 2, 6);
            Utils.ResizeObject(ref R, cmdBrowse, tclHeight - 3, 14.5f, 2.5f, 6);

            Utils.ResizeObject(ref R, cmdSendToPrinter, tclHeight - 6, 21, 5.5f, 4);

            Utils.ResizeObject(ref R, lblSelectHardTest, tclHeight - 6, 25.5f, 1, 5);
            Utils.ResizeObject(ref R, cbAvailableHardTests, tclHeight - 5, 25.5f, 2, 5);
            Utils.ResizeObject(ref R, cmdRunHardTest, tclHeight - 6, 31, 5.5f, 4);

         }
         R.offset = 0;
      }

      // Only allow buttons if conditions are right to process the request
      public void SetButtonEnables() {
         cmdSaveAs.Enabled = XMLText.Length > 0;
         cmdSendToPrinter.Enabled = xmlDoc != null;
         cmdRunHardTest.Enabled = cbAvailableHardTests.SelectedIndex >= 0;
      }

      private string GetValue(XmlNode node) {
         if (node != null) {
            return node.InnerText;
         } else {
            return N_A;
         }
      }

      private string GetAttr(XmlNode node, string AttrName) {
         XmlNode n;
         if (node != null && (n = node.Attributes[AttrName]) != null) {
            return n.Value;
         } else {
            return N_A;
         }
      }

      private void BuildTestFileList() {
         cbAvailableXmlTests.Items.Clear();
         string[] FileNames = Directory.GetFiles(parent.MessageFolder, "*.XML");
         Array.Sort(FileNames);
         for (int i = 0; i < FileNames.Length; i++) {
            cbAvailableXmlTests.Items.Add(Path.GetFileNameWithoutExtension(FileNames[i]));
         }

      }

      #endregion

      #region Test Routines

      // Success is "Global" so the Get/Set/Service Attributes callers can avoid continuously testing it
      bool success = true;

      // Get the contents of one attribute
      private string GetAttribute<T>(T Attribute) {
         string val = string.Empty;
         ClassCode cc = EIP.ClassCodes[Array.IndexOf(EIP.ClassCodeAttributes, typeof(T))];
         byte at = Convert.ToByte(Attribute);
         AttrData attr = EIP.AttrDict[cc, at];
         if (EIP.GetAttribute(cc, at, EIP.Nodata)) {
            val = EIP.GetDataValue;
            if (attr.Data.Fmt == DataFormats.UTF8) {
               val = EIP.FromQuoted(EIP.GetDataValue);
            } else if (attr.Data.DropDown != fmtDD.None) {
               string[] dd = EIP.DropDowns[(int)attr.Data.DropDown];
               long n = EIP.GetDecValue - attr.Data.Min;
               if (n >= 0 && n < dd.Length) {
                  val = dd[n];
               }
            }
         }
         return val;
      }

      // Get the value of an attribute that is known to be a decimal number
      private int GetDecimalAttribute<T>(T Attribute) where T : Enum {
         AttrData attr = EIP.GetAttrData(Attribute);
         EIP.GetAttribute(attr.Class, attr.Val, EIP.Nodata);
         return EIP.GetDecValue;
      }

      // Delete all but 1
      private void cmdDeleteAll_Click(object sender, EventArgs e) {
         CleanUpDisplay();
      }

      // Add text to all items (Control Deleted)
      private void cmdAddText_Click(object sender, EventArgs e) {
         // Add new logic here
      }

      // Create a message with text only (Control Deleted)
      private void cmdCreateText_Click(object sender, EventArgs e) {
         success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               for (int step = 0; step < 3 && success; step++) {
                  switch (step) {
                     case 0:
                        // Cleanup the display
                        CleanUpDisplay();
                        break;
                     case 1:
                        // Put in some items
                        for (int i = 0; i < 5; i++) {
                           EIP.ServiceAttribute(ccPF.Add_Column, 0);
                        }
                        break;
                     case 2:
                        // Set the text
                        SetText("Hello World");
                        break;
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void cmdSaveToPrinter_Click(object sender, EventArgs e) {
         success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               byte[] data = EIP.Merge(EIP.ToBytes(4, 2), EIP.ToBytes(2, 1), EIP.ToBytes("AAA" + "\x00"));
               AttrData attr = EIP.GetAttrData(ccPDM.Store_Print_Data);
               EIP.SetAttribute(attr.Class, attr.Val, data);
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void cmdBrowse_Click(object sender, EventArgs e) {
         using (FolderBrowserDialog dlg = new FolderBrowserDialog()) {
            dlg.ShowNewFolderButton = true;
            dlg.SelectedPath = parent.MessageFolder;
            if (dlg.ShowDialog() == DialogResult.OK) {
               parent.MessageFolder = dlg.SelectedPath;
               BuildTestFileList();
            }
         }
      }

      #endregion

   }

}
