using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using HIES.IJP.RX;

namespace IJPLib_Test {
   public partial class IJPTest : Form {

      #region Data Declarations

      ResizeInfo R;
      bool initComplete = false;

      // Braced Characters (count, date, half-size, logos
      readonly char[] bc = new char[] { 'C', 'Y', 'M', 'D', 'h', 'm', 's', 'T', 'W', '7', 'E', 'F', ' ', '\'', '.', ';', ':', '!', ',', 'X', 'Z' };

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
         Comma = 1 << 18,
         FixedPattern = 1 << 19,
         FreePattern = 1 << 20,
         Unknown = 1 << 21,
         //DateCode = (1 << 12) - 2, // All the date codes combined
      }

      const int DateCode =
         (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute | (int)ba.Second |
         (int)ba.Julian | (int)ba.Week | (int)ba.DayOfWeek | (int)ba.Shift | (int)ba.TimeCount;

      const int DateOffset =
        (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute | (int)ba.Second |
        (int)ba.Julian | (int)ba.Week | (int)ba.DayOfWeek;

      const int DateSubZS =
         (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute |
         (int)ba.Week | (int)ba.DayOfWeek;

      const int DateUseSubRule = DateOffset;

      // Get the current message.
      IJPMessage message = null;

      IJPOnlineStatus comOn = IJPOnlineStatus.Offline;

      IJPLib_Test.Properties.Settings p;

      #endregion

      #region Constructors and Destructors

      private IJP ijp;

      public IJPTest() {
         InitializeComponent();
         initComplete = true;
         p = IJPLib_Test.Properties.Settings.Default;
         ipAddressTextBox.Text = p.IPAddress;
         txtMessageFolder.Text = p.MessageFolder;
      }

      ~IJPTest() {

      }

      #endregion

      #region Form level events

      private void IJPTest_Load(object sender, EventArgs e) {
         // Center the form on the screen
         Utils.PositionForm(this, 0.5f, 0.9f);
         cbSelectHardCodedTest.Items.AddRange(AvailableTests);
         BuildTestFileList();
         setButtonEnables();
      }

      private void IJPTest_Resize(object sender, EventArgs e) {
         //
         // Avoid resize before Program Load has run or on screen minimize
         if (initComplete && ClientRectangle.Height > 0) {
            //
            this.SuspendLayout();
            // Build local parameters
            R = Utils.InitializeResize(this, 60, 40, true);

            Utils.ResizeObject(ref R, ipAddressTextBox, 1, 1, 2, 5);
            Utils.ResizeObject(ref R, cmdConnect, 1, 7, 2, 5);
            Utils.ResizeObject(ref R, cmdComOnOff, 1, 13, 2, 5);

            Utils.ResizeObject(ref R, lblMessageFolder, 1, 24, 2, 10);
            Utils.ResizeObject(ref R, txtMessageFolder, 3, 24, 2, 10);
            Utils.ResizeObject(ref R, cmdBrowse, 1, 35, 4, 4);

            Utils.ResizeObject(ref R, tclIJPLib, 5, 1, 43, 33);
            Utils.ResizeObject(ref R, cmdClear, 7, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdGetXML, 10, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdGetViews, 13, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdSaveAs, 16, 35, 2, 4);
            Utils.ResizeObject(ref R, cmdSend, 19, 35, 2, 4);

            Utils.ResizeObject(ref R, txtIjpIndented, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, tvIJPLibTree, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, txtXMLIndented, 1, 1, 38, 31);
            Utils.ResizeObject(ref R, tvXMLTree, 1, 1, 38, 31);

            Utils.ResizeObject(ref R, lstLogs, 50, 1, 9, 15);

            Utils.ResizeObject(ref R, lblSelectHardCodedTest, 50, 17, 2, 10);
            Utils.ResizeObject(ref R, cbSelectHardCodedTest, 52, 17, 2, 10);
            Utils.ResizeObject(ref R, cmdRunHardCodedTest, 55, 17, 3, 10);

            Utils.ResizeObject(ref R, lblSelectXMLTest, 50, 28, 2, 10);
            Utils.ResizeObject(ref R, cbSelectXMLTest, 52, 28, 2, 10);
            Utils.ResizeObject(ref R, cmdRunXMLTest, 55, 28, 3, 10);

            this.ResumeLayout();
         }
      }

      private void IJPTest_FormClosing(object sender, FormClosingEventArgs e) {
         p.IPAddress = ipAddressTextBox.Text;
         p.MessageFolder = txtMessageFolder.Text;
         p.Save();
      }

      #endregion

      #region Form Control Events

      private void tclIJPLib_SelectedIndexChanged(object sender, EventArgs e) {
         setButtonEnables();
      }

      private void cmdConnect_Click(object sender, EventArgs e) {
         if (null == this.ijp) {
            // Connect to the printer
            ConnectIJP();
            // Get com setting
            comOn = ijp.GetComPort();
            if (comOn == IJPOnlineStatus.Offline) {
               cmdComOnOff_Click(null, null);
            }
            // Set Caption
            this.cmdConnect.Text = "Disconnect";
         } else {
            // Turn com off
            cmdComOnOff_Click(null, null);
            // Disconnect from printer
            DisconnectIJP();
            // Set caption
            this.cmdConnect.Text = "Connect";
         }
         setButtonEnables();
      }

      private void cmdComOnOff_Click(object sender, EventArgs e) {
         switch (comOn) {
            case IJPOnlineStatus.Offline:
               this.ijp.SetComPort(IJPOnlineStatus.Online);
               comOn = IJPOnlineStatus.Online;
               break;
            case IJPOnlineStatus.Online:
               this.ijp.SetComPort(IJPOnlineStatus.Offline);
               comOn = IJPOnlineStatus.Offline;
               break;
            default:
               break;
         }
         setButtonEnables();
      }

      private void cmdClear_Click(object sender, EventArgs e) {
         message = null;    // Force retrieval of next message
         ClearViews();      // Clear the four screens
      }

      private void cmdGetViews_Click(object sender, EventArgs e) {
         Log("Get View Starting");
         //  Set hour glass
         Cursor.Current = Cursors.WaitCursor;
         // Out with the old
         txtIjpIndented.Text = string.Empty;
         tvIJPLibTree.Nodes.Clear();
         // In with the new
         ShowCurrentMessage();
         // Generate the views
         ObjectDumper od = new ObjectDumper(2);
         string indentedView;
         TreeNode treeNode;
         od.Dump(message, out indentedView, out treeNode);
         // Display the viewd
         txtIjpIndented.Text = indentedView;
         tvIJPLibTree.Nodes.Add(treeNode);
         tvIJPLibTree.ExpandAll();
         tclIJPLib.SelectedTab = tabIndentedView;
         // Restore cursor
         Cursor.Current = Cursors.Arrow;
         setButtonEnables();
         Log("Get View Complete");
      }

      private void cmdGetXML_Click(object sender, EventArgs e) {
         Log("Get XML Starting");                      // Show start
         Cursor.Current = Cursors.WaitCursor;          // Set hour glass
         ClearViews();                                 // Out with the old
         ShowCurrentMessage();                         // Use current or new message
         txtXMLIndented.Text = MessageToXML(message);   // Display the indented XML as it is.
         ProcessLabel(txtXMLIndented.Text);            // Build XML Tree and display it
         tclIJPLib.SelectedTab = tabXMLIndented;       // Make XML Indented tab visible
         Cursor.Current = Cursors.Arrow;               // Restore cursor
         setButtonEnables();                           // Enable the correct buttons
         Log("Get XML Complete");                      // Show Completion
      }

      private void ccmdSaveAs_Click(object sender, EventArgs e) {
         string fileName = string.Empty;
         string fileText = string.Empty; ;
         using (SaveFileDialog sfd = new SaveFileDialog()) {
            switch (tclIJPLib.SelectedIndex) {
               case 0:
                  fileName = "IJPIndented.txt";
                  fileText = txtIjpIndented.Text;
                  sfd.DefaultExt = "txt";
                  sfd.Filter = "Text|*.txt";
                  sfd.Title = "Save Printer Image to Text file";
                  break;
               case 2:
                  fileName = "XMLIndented.XML";
                  fileText = txtXMLIndented.Text;
                  sfd.DefaultExt = "xml";
                  sfd.Filter = "XML|*.xml";
                  sfd.Title = "Save Printer Image to XML file";
                  break;
               default:
                  break;
            }
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.InitialDirectory = txtMessageFolder.Text;
            sfd.FileName = fileName;
            if (sfd.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty(sfd.FileName)) {
               fileName = Path.Combine(txtMessageFolder.Text, sfd.FileName);
               File.WriteAllText(fileName, fileText);
            }
         }
         BuildTestFileList();
         setButtonEnables();
      }

      private void cmdSend_Click(object sender, EventArgs e) {
         try {
            Log("Send Message Starting");
            cmdRunXMLTest_Click(null, null);
            ijp.SetMessage(message);
         } catch (Exception e2) {
            Log($"Send Message: {e2.Message}\r\n{e2.StackTrace}");
         } finally {
            Log("Send Message Complete");
         }
      }

      private void cbSelectTest_SelectedIndexChanged(object sender, EventArgs e) {
         setButtonEnables();
      }

      private void cmErrLogToNotepad_Click(object sender, EventArgs e) {
         string ViewFilename = @"c:\Temp\Err.txt";
         File.WriteAllLines(ViewFilename, lstLogs.Items.Cast<string>().ToArray());
         Process.Start("notepad.exe", ViewFilename);
      }

      private void cmErrLogClearlog_Click(object sender, EventArgs e) {
         lstLogs.Items.Clear();
      }

      private void cmdBrowse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() 
            { ShowNewFolderButton = true, SelectedPath = txtMessageFolder.Text };
         if (dlg.ShowDialog() == DialogResult.OK) {
            txtMessageFolder.Text = dlg.SelectedPath;
            BuildTestFileList();
         }
      }

      private void cbSelectXMLTest_SelectedIndexChanged(object sender, EventArgs e) {
         string fileName = Path.Combine(txtMessageFolder.Text, cbSelectXMLTest.Text + ".xml");
         ClearViews();
         string xml = File.ReadAllText(fileName);
         ProcessLabel(xml);
         setButtonEnables();
      }

      private void cmdRunXMLTest_Click(object sender, EventArgs e) {
         XmlDocument  xmlDoc = new XmlDocument() { PreserveWhitespace = true };
         xmlDoc.LoadXml(txtXMLIndented.Text);
         SendXmlToPrinter(xmlDoc);
      }

      #endregion

      #region Message to XML 

      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
      }

      public string MessageToXML(IJPMessage m) {
         string xml = string.Empty;
         ItemType itemType;
         int calBlockNumber = 0;
         int cntBlockNumber = 0;
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlTextWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding("UTF-8"))) {
               writer.Formatting = Formatting.Indented;
               writer.WriteStartDocument();
               try {
                  writer.WriteStartElement("Label"); // Start Label
                  {
                     writer.WriteAttributeString("Version", "1");
                     RetrievePrinterSettings(writer, m);
                     writer.WriteStartElement("Message"); // Start Message
                     {
                        writer.WriteAttributeString("Layout", m.FormatSetup.ToString());
                        int item = 0;
                        while (item < m.Items.Count) {
                           writer.WriteStartElement("Column"); // Start Column
                           {
                              int colCount = m.Items[item].PrintLine;
                              writer.WriteAttributeString("InterLineSpacing", m.Items[item].LineSpacing.ToString());
                              for (int i = item; i < item + colCount; i++) {
                                 string text = m.Items[i].Text;
                                 int calBlockCount = m.Items[i].CalendarBlockCount;
                                 int cntBlockCount = m.Items[i].CountBlockCount;
                                 int[] mask = new int[1 + Math.Max(calBlockCount, cntBlockCount)];
                                 itemType = GetItemType(text, ref mask);
                                 writer.WriteStartElement("Item"); // Start Item
                                 {
                                    RetrieveFont(writer, (IJPMessageItem)m.Items[item]);
                                    switch (itemType) {
                                       case ItemType.Text:
                                          break;
                                       case ItemType.Date:
                                          RetrieveCalendarSettings(writer, calBlockNumber, calBlockCount, m.CalendarConditions, mask);
                                          RetrieveShiftSettings(writer, m.ShiftCodes, mask);
                                          RetrieveTimeCountSettings(writer, m.TimeCount, mask);
                                          calBlockNumber += calBlockCount;
                                          break;
                                       case ItemType.Counter:
                                          RetrieveCounterSettings(writer, cntBlockNumber, cntBlockCount, m.CountConditions);
                                          break;
                                       case ItemType.Logo:
                                          RetrieveUserPatternSettings(writer);
                                          break;
                                       default:
                                          break;
                                    }

                                    writer.WriteElementString("Text", m.Items[i].Text);
                                 }
                                 writer.WriteEndElement(); // End Item
                              }
                              item += colCount;
                           }

                           writer.WriteEndElement(); // End Column
                        }
                     }
                     writer.WriteEndElement(); // End Message
                  }
                  writer.WriteEndElement(); // End Label
               } catch (Exception e1) {
                  MessageBox.Show("Help", "EIP I/O Error", MessageBoxButtons.OK);
               }
               writer.WriteEndDocument();
               writer.Flush();
               ms.Position = 0;

               xml = new StreamReader(ms).ReadToEnd();
               int xmlStart = 0;
               int xmlEnd = 0;
               // Can be called with a Filename or XML text
               xmlStart = xml.IndexOf("<Label");
               if (xmlStart == -1) {
                  xml = File.ReadAllText(xml);
                  xmlStart = xml.IndexOf("<Label");
               }
               // No label found, exit
               if (xmlStart == -1) {
                  return string.Empty;
               }
               xmlEnd = xml.IndexOf("</Label>", xmlStart + 7);
               if (xmlEnd > 0) {
                  xml = xml.Substring(xmlStart, xmlEnd - xmlStart + 8);
               }
            }
         }
         return xml;
      }

      private void RetrievePrinterSettings(XmlTextWriter writer, IJPMessage m) {

         writer.WriteStartElement("Printer");
         {
            {
               writer.WriteAttributeString("Make", "Hitachi");
               IJPUnitInformation ui = (IJPUnitInformation)ijp.GetUnitInformation();
               writer.WriteAttributeString("Model", ui.TypeName);
            }

            writer.WriteStartElement("PrintHead");
            {
               writer.WriteAttributeString("Orientation", m.CharacterOrientation.ToString());
            }
            writer.WriteEndElement(); // PrintHead

            writer.WriteStartElement("ContinuousPrinting");
            {
               writer.WriteAttributeString("RepeatInterval", m.RepeatIntervals.ToString());
               writer.WriteAttributeString("PrintsPerTrigger", m.RepeatCount.ToString());
            }
            writer.WriteEndElement(); // ContinuousPrinting

            writer.WriteStartElement("TargetSensor");
            {
               writer.WriteAttributeString("Filter", m.TargetSensorFilter.ToString());
               writer.WriteAttributeString("SetupValue", m.TimeSetup.ToString());
               writer.WriteAttributeString("Timer", m.TargetSensorTimer.ToString());
            }
            writer.WriteEndElement(); // TargetSensor

            writer.WriteStartElement("CharacterSize");
            {
               writer.WriteAttributeString("Width", m.CharacterWidth.ToString());
               writer.WriteAttributeString("Height", m.CharacterHeight.ToString());
            }
            writer.WriteEndElement(); // CharacterSize

            writer.WriteStartElement("PrintStartDelay");
            {
               writer.WriteAttributeString("Forward", m.PrintStartDelayForward.ToString());
               writer.WriteAttributeString("Reverse", m.PrintStartDelayReverse.ToString());
            }
            writer.WriteEndElement(); // PrintStartDelay

            writer.WriteStartElement("EncoderSettings");
            {
               writer.WriteAttributeString("HighSpeedPrinting", m.HiSpeedPrint.ToString());
               writer.WriteAttributeString("Divisor", m.PulseRateDivisionFactor.ToString());
               writer.WriteAttributeString("ExternalEncoder", m.ProductSpeedMatching.ToString());
            }
            writer.WriteEndElement(); // EncoderSettings

            writer.WriteStartElement("InkStream");
            {
               writer.WriteAttributeString("InkDropUse", m.InkDropUse.ToString());
               writer.WriteAttributeString("ChargeRule", m.InkDropChargeRule.ToString());
            }
            writer.WriteEndElement(); // InkStream

            RetrieveSubstitutions(writer);

            RetrieveLogos(writer);
         }
         writer.WriteEndElement(); // Printer
      }

      private void RetrieveFont(XmlTextWriter writer, IJPMessageItem mi) {
         writer.WriteStartElement("Font"); // Start Font
         {
            writer.WriteAttributeString("InterCharacterSpace", mi.InterCharacterSpace.ToString());
            writer.WriteAttributeString("IncreasedWidth", mi.Bold.ToString());
            writer.WriteAttributeString("DotMatrix", mi.DotMatrix.ToString());
         }
         writer.WriteEndElement(); // End Font

         writer.WriteStartElement("BarCode"); // Start Barcode
         {
            if (mi.Barcode != IJPBarcode.Nothing) {
               writer.WriteAttributeString("HumanReadableFont", mi.ReadableCode.ToString());
               writer.WriteAttributeString("EANPrefix", mi.Prefix.ToString());
               writer.WriteAttributeString("DotMatrix", mi.Barcode.ToString());
            }
         }
         writer.WriteEndElement(); // End BarCode
      }

      private void RetrieveCalendarSettings(XmlTextWriter writer, int FirstBlock, int BlockCount, IJPCalendarConditionCollection cc, int[] mask) {

         for (int i = 0; i < BlockCount; i++) {
            IJPCalendarCondition c = cc[FirstBlock + i];
            // Where is the Substitution Rule
            writer.WriteStartElement("Date"); // Start Date
            {
               writer.WriteAttributeString("Block", (i + 1).ToString());
               if ((mask[i] & DateUseSubRule) > 0) {
                  writer.WriteAttributeString("SubstitutionRule", c.SubstitutionRuleNumber.ToString());
                  writer.WriteAttributeString("RuleName", "");
               }

               if ((mask[i] & DateOffset) > 0) { // Not always needed
                  writer.WriteStartElement("Offset"); // Start Offset
                  {
                     writer.WriteAttributeString("Year", c.YearOffset.ToString());
                     writer.WriteAttributeString("Month", c.MonthOffset.ToString());
                     writer.WriteAttributeString("Day", c.DayOffset.ToString());
                     writer.WriteAttributeString("Hour", c.HourOffset.ToString());
                     writer.WriteAttributeString("Minute", c.MinuteOffset.ToString());
                  }
                  writer.WriteEndElement(); // End Offset
               }

               if ((mask[i] & DateSubZS) > 0) {
                  writer.WriteStartElement("ZeroSuppress"); // Start ZeroSuppress
                  {
                     if ((mask[i] & (int)ba.Year) > 0)
                        writer.WriteAttributeString("Year", c.YearZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Month) > 0)
                        writer.WriteAttributeString("Month", c.MonthZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Day) > 0)
                        writer.WriteAttributeString("Day", c.DayZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Hour) > 0)
                        writer.WriteAttributeString("Hour", c.HourZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Minute) > 0)
                        writer.WriteAttributeString("Minute", c.MinuteZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.Week) > 0)
                        writer.WriteAttributeString("Week", c.WeekNumberZeroSuppression.ToString());
                     if ((mask[i] & (int)ba.DayOfWeek) > 0)
                        writer.WriteAttributeString("DayOfWeek", c.WeekZeroSuppression.ToString());
                  }
                  writer.WriteEndElement(); // End ZeroSuppress

                  writer.WriteStartElement("Substitute"); // Start Substitute
                  {
                     if ((mask[i] & (int)ba.Year) > 0)
                        writer.WriteAttributeString("Year", c.YearSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Month) > 0)
                        writer.WriteAttributeString("Month", c.MonthSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Day) > 0)
                        writer.WriteAttributeString("Day", c.DaySubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Hour) > 0)
                        writer.WriteAttributeString("Hour", c.HourSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Minute) > 0)
                        writer.WriteAttributeString("Minute", c.MinuteSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.Week) > 0)
                        writer.WriteAttributeString("Week", c.WeekNumberSubstitutionRule.ToString());
                     if ((mask[i] & (int)ba.DayOfWeek) > 0)
                        writer.WriteAttributeString("DayOfWeek", c.WeekSubstitutionRule.ToString());
                  }
                  writer.WriteEndElement(); // End EnableSubstitution
               }

            }
            writer.WriteEndElement(); // End Date
         }
      }

      private void RetrieveShiftSettings(XmlTextWriter writer, IJPShiftCodeCollection ss, int[] mask) {
         for (int i = 0; i < mask.Length; i++) {
            if ((mask[i] & (int)ba.Shift) > 0) {
               for (int shift = 0; shift < ss.Count; shift++) {
                  writer.WriteStartElement("Shift"); // Start Shift
                  {
                     writer.WriteAttributeString("ShiftNumber", (shift + 1).ToString());
                     writer.WriteAttributeString("StartHour", ss[shift].StartTime.Hour.ToString());
                     writer.WriteAttributeString("StartMinute", ss[shift].StartTime.Minute.ToString());
                     writer.WriteAttributeString("ShiftCode", ss[shift].String);
                  }
                  writer.WriteEndElement(); // End Shift
               }
            }
         }
      }

      private void RetrieveTimeCountSettings(XmlTextWriter writer, IJPTimeCountCondition tc, int[] mask) {
         for (int i = 0; i < mask.Length; i++) {
            if ((mask[i] & (int)ba.TimeCount) > 0) {
               writer.WriteStartElement("TimeCount"); // Start TimeCount
               {
                  writer.WriteAttributeString("Interval", tc.RenewalPeriod.ToString());
                  writer.WriteAttributeString("Start", tc.LowerRange.ToString());
                  writer.WriteAttributeString("End", tc.UpperRange.ToString());
                  writer.WriteAttributeString("ResetTime", tc.ResetTime.ToString());
                  writer.WriteAttributeString("ResetValue", tc.Reset.ToString());
               }
               writer.WriteEndElement(); // End TimeCount
            }
         }
      }

      private void RetrieveCounterSettings(XmlTextWriter writer, int FirstBlock, int BlockCount, IJPCountConditionCollection cc) {
         for (int i = 0; i < BlockCount; i++) {
            IJPCountCondition c = cc[FirstBlock + i];
            writer.WriteStartElement("Counter"); // Start Counter
            {
               writer.WriteAttributeString("Block", (i + 1).ToString());
               writer.WriteStartElement("Range"); // Start Range
               {
                  writer.WriteAttributeString("Range1", c.LowerRange);
                  writer.WriteAttributeString("Range2", c.UpperRange);
                  writer.WriteAttributeString("JumpFrom", c.JumpFrom);
                  writer.WriteAttributeString("JumpTo", c.JumpTo);
               }
               writer.WriteEndElement(); //  End Range

               writer.WriteStartElement("Count"); // Start Count
               {
                  writer.WriteAttributeString("InitialValue", c.Value);
                  writer.WriteAttributeString("Increment", c.Increment.ToString());
                  writer.WriteAttributeString("Direction", c.Direction.ToString());
                  writer.WriteAttributeString("ZeroSuppression", c.SuppressesZero.ToString());
               }
               writer.WriteEndElement(); //  End Count

               writer.WriteStartElement("Reset"); // Start Reset
               {
                  writer.WriteAttributeString("Type", c.ResetSignal.ToString());
                  writer.WriteAttributeString("Value", c.Reset.ToString());
               }
               writer.WriteEndElement(); //  End Reset

               writer.WriteStartElement("Misc"); // Start Misc
               {
                  writer.WriteAttributeString("UpdateIP", c.UpdateInProgress.ToString());
                  writer.WriteAttributeString("UpdateUnit", c.UpdateUnit.ToString());
                  writer.WriteAttributeString("ExternalCount", c.UsesExternalSignalCount.ToString());
                  //writer.WriteAttributeString("Multiplier",c.Multiplier.ToString());
                  //writer.WriteAttributeString("SkipCount",c.CountSkip.ToString());
               }
               writer.WriteEndElement(); //  End Misc

            }
            writer.WriteEndElement(); //  End Counter
         }
      }

      private void RetrieveUserPatternSettings(XmlTextWriter writer) {

      }

      // Examine the contents of a print message to determine its type
      private ItemType GetItemType(string text, ref int[] mask) {
         int l = 0;
         mask[l] = 0;
         string[] s = text.Split('{');
         for (int i = 0; i < s.Length; i++) {
            int n = s[i].IndexOf('}');
            if (n >= 0) {
               for (int j = 0; j < n; j++) {
                  int k = Array.IndexOf(bc, s[i][j]);
                  if (k >= 0) {
                     mask[l] |= 1 << k;
                  } else {
                     mask[l] |= (int)ba.Unknown;
                  }
               }
            }
            if (s[i].IndexOf('}', n + 1) > 0) {
               l++;
            }
         }
         // Calendar and Count cannot appear in the same item
         if ((mask[0] & (int)ba.Count) > 0) {
            return ItemType.Counter;
         } else if ((mask[0] & DateCode) > 0) {
            return ItemType.Date;
         } else {
            return ItemType.Text;
         }
      }

      private void RetrieveLogos(XmlTextWriter writer) {

      }

      private void RetrieveSubstitutions(XmlTextWriter writer) {

      }

      #endregion

      #region XML to Message

      List<XmlNode> Items;

      // Send xlmDoc from file to printer
      public bool SendXmlToPrinter(XmlDocument xmlDoc) {
         message = new IJPMessage();
         bool success = true;
         // Need a XMP Document to continue
         if (xmlDoc == null) {
            return false;
         }
         try {
            XmlNode objs = xmlDoc.SelectSingleNode("Label/Message");
            if (objs != null) {
               success = AllocateRowsColumns(objs.ChildNodes); // Allocate rows and columns
            }

            XmlNode prnt = xmlDoc.SelectSingleNode("Label/Printer");
            if (success && prnt != null) {
               success = SendPrinterSettings(prnt);            // Send printer wide settings
            }
         } catch (Exception e1) {
            success = false;
         } finally {

         }

         return success;
      }

      //ild the structure and load Items
      private bool AllocateRowsColumns(XmlNodeList objs) {
         XmlNode n;
         Items = new List<XmlNode>();
         bool success = true;
         int[] columns = new int[100];
         int[] ILS = new int[100];
         int maxCol = 0;

         // Count the rows and columns
         foreach (XmlNode col in objs) {
            if (col is XmlWhitespace)
               continue;
            switch (col.Name) {
               case "Column":
                  columns[maxCol] = 0;
                  int.TryParse(GetXmlAttr(col, "InterLineSpacing"), out ILS[maxCol]);
                  foreach (XmlNode item in col) {
                     if (item is XmlWhitespace)
                        continue;
                     switch (item.Name) {
                        case "Item":
                           Items.Add(item);
                           columns[maxCol]++;
                           break;
                     }
                  }
                  maxCol++;
                  break;
            }
         }
         // Allocate the rows and columns
         int i = 0;
         for (int col = 0; col < maxCol; col++) {
            if (columns[col] == 0) {
               return false;
            }
            message.AddColumn();
            message.SetRow(col, (byte)columns[col]);

            for (int row = 0; row < columns[col]; row++) {
               IJPMessageItem item = (IJPMessageItem)message.Items[i];
               if ((n = Items[i].SelectSingleNode("Font")) != null) {
                  item.DotMatrix = ParseEnum<IJPDotMatrix>(GetXmlAttr(n, "DotMatrix"));
                  item.InterCharacterSpace = (byte)GetXmlAttrN(n, "InterCharacterSpace");
                  item.LineSpacing = (byte)ILS[col];
                  item.Bold = (byte)GetXmlAttrN(n, "IncreasedWidth");
                  item.Text = GetXmlValue(Items[i].SelectSingleNode("Text"));
               }
               i++;
            }
         }
         SendDateCount();
         return success;
      }

      private bool SendDateCount() {
         bool success = true;
         int calBlockNumber = 0;
         int cntBlockNumber = 0;

         int[] calStart = new int[Items.Count];
         int[] calCount = new int[Items.Count];
         int[] countStart = new int[Items.Count];
         int[] countCount = new int[Items.Count];

         for (int i = 0; i < Items.Count; i++) {
            IJPMessageItem item = (IJPMessageItem)message.Items[i];
            if (Items[i].SelectSingleNode("Date") != null) {
               calCount[i] = item.CalendarBlockCount;
               if (calCount[i] > 0) {
                  calStart[i] = calBlockNumber;
                  calBlockNumber += calCount[i];
               }
            }
            if (Items[i].SelectSingleNode("Counter") != null) {
               countCount[i] = item.CountBlockCount;
               if (countCount[i] > 0) {
                  countStart[i] = cntBlockNumber;
                  cntBlockNumber += countCount[i];
               }
            }
         }

         for (int i = 0; i < Items.Count; i++) {
            if (Items[i].SelectSingleNode("Date") != null) {
               LoadCalendar(Items[i], calCount[i], calStart[i]);
            }
            if (Items[i].SelectSingleNode("Counter") != null) {
               LoadCount(Items[i], countCount[i], countStart[i]);
            }
            if (Items[i].SelectSingleNode("TimeCount") != null) {
               LoadTimeCount(Items[i].SelectSingleNode("TimeCount"));
            }
            if (Items[i].SelectSingleNode("Shift") != null) {
               LoadShift(Items[i]);
            }
         }

         Items = null;

         return success;
      }

      private void LoadShift(XmlNode obj) {

         foreach (XmlNode d in obj) {
            if (d is XmlWhitespace)
               continue;
            if (d.Name == "Shift") {
               IJPShiftCode sc = new IJPShiftCode();
               if (int.TryParse(GetXmlAttr(d, "ShiftNumber"), out int shift)) {
                  foreach (XmlAttribute a in d.Attributes) {
                     switch (a.Name) {
                        case "StartHour":
                           sc.StartTime.Hour = (byte)Convert.ToInt16(a.Value);
                           break;
                        case "StartMinute":
                           sc.StartTime.Minute = (byte)Convert.ToInt16(a.Value);
                           break;
                        case "ShiftCode":
                           sc.String = a.Value;
                           break;
                     }
                  }
               }
               message.ShiftCodes.Add(sc);
            }
         }
      }

      private void LoadTimeCount(XmlNode d) {
         IJPTimeCountCondition tc = new IJPTimeCountCondition();
         foreach (XmlAttribute a in d.Attributes) {
            switch (a.Name) {
               case "Start":
                  tc.LowerRange = a.Value;
                  break;
               case "End":
                  tc.UpperRange = a.Value;
                  break;
               case "ResetValue":
                  tc.Reset = a.Value;
                  break;
               case "ResetTime":
                  tc.ResetTime = (byte)Convert.ToInt16(a.Value);
                  break;
               case "Interval":
                  tc.RenewalPeriod = ParseEnum<IJPTimeCountConditionRenewalPeriod>(a.Value);
                  break;
            }
         }
         message.TimeCount = tc;
      }

      // Send Calendar related information
      private bool LoadCalendar(XmlNode obj, int CalBlockCount, int FirstCalBlock) {
         bool success = true;

         foreach (XmlNode d in obj) {
            if (d is XmlWhitespace)
               continue;
            if (d.Name == "Date" && int.TryParse(GetXmlAttr(d, "Block"), out int b) && b <= CalBlockCount) {
               IJPCalendarCondition cc = message.CalendarConditions[FirstCalBlock + b - 1];

               if (int.TryParse(GetXmlAttr(d, "SubstitutionRule"), out int sr)) {
                  cc.SubstitutionRuleNumber = (byte)sr;
               }
               foreach (XmlNode n in d.ChildNodes) {
                  if (n is XmlWhitespace)
                     continue;
                  switch (n.Name) {
                     case "Offset":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (int.TryParse(a.Value, out int x)) {
                              switch (a.Name) {
                                 case "Year":
                                    cc.YearOffset = (byte)x;
                                    break;
                                 case "Month":
                                    cc.MonthOffset = (byte)x;
                                    break;
                                 case "Day":
                                    cc.DayOffset = (ushort)x;
                                    break;
                                 case "Hour":
                                    cc.HourOffset = (short)x;
                                    break;
                                 case "Minute":
                                    cc.MinuteOffset = (short)x;
                                    break;
                              }
                           }
                        }
                        break;
                     case "ZeroSuppress":
                        foreach (XmlAttribute a in n.Attributes) {
                           IJPCalendarConditionZeroSuppress zs = ParseEnum<IJPCalendarConditionZeroSuppress>(a.Value);
                           switch (a.Name) {
                              case "Year":
                                 cc.YearZeroSuppression = zs;
                                 break;
                              case "Month":
                                 cc.MonthZeroSuppression = zs;
                                 break;
                              case "Day":
                                 cc.DayZeroSuppression = zs;
                                 break;
                              case "Hour":
                                 cc.HourZeroSuppression = zs;
                                 break;
                              case "Minute":
                                 cc.MinuteZeroSuppression = zs;
                                 break;
                              case "Week":
                                 cc.WeekZeroSuppression = zs;
                                 break;
                              case "DayOfWeek":
                                 cc.WeekNumberZeroSuppression = zs;
                                 break;
                           }
                        }
                        break;
                     case "Substitute":
                        foreach (XmlAttribute a in n.Attributes) {
                           if (bool.TryParse(a.Value, out bool sub)) {
                              switch (a.Name) {
                                 case "Year":
                                    cc.YearSubstitutionRule = sub;
                                    break;
                                 case "Month":
                                    cc.MonthSubstitutionRule = sub;
                                    break;
                                 case "Day":
                                    cc.DaySubstitutionRule = sub;
                                    break;
                                 case "Hour":
                                    cc.HourSubstitutionRule = sub;
                                    break;
                                 case "Minute":
                                    cc.MinuteSubstitutionRule = sub;
                                    break;
                                 case "Week":
                                    cc.WeekNumberSubstitutionRule = sub;
                                    break;
                                 case "DayOfWeek":
                                    cc.WeekSubstitutionRule = sub;
                                    break;
                              }
                           }
                        }
                        break;
                  }
               }
            } else if (d.Name == "TimeCount") {
               IJPTimeCountCondition tc = message.TimeCount;
               foreach (XmlAttribute a in d.Attributes) {
                  switch (a.Name) {
                     case "Start":
                        tc.LowerRange = a.Value;
                        break;
                     case "End":
                        tc.LowerRange = a.Value;
                        break;
                     case "ResetValue":
                        tc.Reset = a.Value;
                        break;
                     case "ResetTime":
                        tc.ResetTime = (byte)Convert.ToInt16(a.Value);
                        break;
                     case "Interval":
                        tc.RenewalPeriod = ParseEnum<IJPTimeCountConditionRenewalPeriod>(a.Value);
                        break;
                  }
               }
            } else if (d.Name == "Shift") {
               if (int.TryParse(GetXmlAttr(d, "ShiftNumber"), out int shift)) {
                  IJPShiftCode sc = message.ShiftCodes[shift - 1];
                  foreach (XmlAttribute a in d.Attributes) {
                     switch (a.Name) {
                        case "StartHour":
                           sc.StartTime.Hour = (byte)Convert.ToInt16(a.Value);
                           break;
                        case "StartMinute":
                           sc.StartTime.Minute = (byte)Convert.ToInt16(a.Value);
                           break;
                        case "ShiftCode":
                           sc.String = a.Value;
                           break;
                     }
                  }
               }
            }
         }
         return success;
      }

      // Send counter related information
      private bool LoadCount(XmlNode obj, int CountBlockCount, int FirstCountBlock) {
         bool success = true;
         XmlNode n;
         foreach (XmlNode c in obj) {
            if (c is XmlWhitespace)
               continue;
            if (c.Name == "Counter" && int.TryParse(GetXmlAttr(c, "Block"), out int b) && b <= CountBlockCount) {
               IJPCountCondition cc = message.CountConditions[FirstCountBlock + b - 1];
               if ((n = c.SelectSingleNode("Range")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "Range1":
                           cc.LowerRange = a.Value;
                           break;
                        case "Range2":
                           cc.UpperRange = a.Value;
                           break;
                        case "JumpFrom":
                           cc.JumpFrom = a.Value;
                           break;
                        case "JumpTo":
                           cc.JumpTo = a.Value;
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Count")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "InitialValue":
                           cc.Value = a.Value;
                           break;
                        case "Increment":
                           cc.Increment = (byte)Convert.ToInt32(a.Value);
                           break;
                        case "Direction":
                           cc.Direction = ParseEnum<IJPCountConditionDirection>(a.Value);
                           break;
                        case "ZeroSuppression":
                           cc.SuppressesZero = Convert.ToBoolean(a.Value);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Reset")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "Value":
                           cc.Reset = a.Value;
                           break;
                        case "Type":
                           cc.ResetSignal = ParseEnum<IJPCountConditionResetSignal>(a.Value);
                           break;
                     }
                  }
               }

               if ((n = c.SelectSingleNode("Misc")) != null) {
                  foreach (XmlAttribute a in n.Attributes) {
                     switch (a.Name) {
                        case "UpdateIP":
                           cc.UpdateInProgress = Convert.ToUInt32(a.Value);
                           break;
                        case "UpdateUnit":
                           cc.UpdateUnit = Convert.ToUInt32(a.Value);
                           break;
                        case "ExternalCount":
                           cc.UsesExternalSignalCount = Convert.ToBoolean(a.Value);
                           break;
                        case "Multiplier":
                           cc.Multiplier = a.Value;
                           break;
                        case "Skip":
                           cc.CountSkip = a.Value;
                           break;
                     }
                  }
               }
            }
         }
         return success;
      }

      // Send the Printer Wide Settings
      private bool SendPrinterSettings(XmlNode pr) {
         bool success = true;
         foreach (XmlNode c in pr.ChildNodes) {
            switch (c.Name) {
               case "PrintHead":
                  message.CharacterOrientation = ParseEnum<IJPCharacterOrientation>(GetXmlAttr(c, "Orientation"));
                  break;
               case "ContinuousPrinting":
                  message.RepeatIntervals = (uint)GetXmlAttrN(c, "RepeatInterval");
                  message.RepeatCount = (ushort)GetXmlAttrN(c, "PrintsPerTrigger");
                  break;
               case "TargetSensor":
                  message.TargetSensorFilter = ParseEnum<IJPSensorFilter>(GetXmlAttr(c, "Filter"));
                  message.TimeSetup = (ushort)GetXmlAttrN(c, "SetupValue");
                  message.TargetSensorTimer = (ushort)GetXmlAttrN(c, "Timer");
                  break;
               case "CharacterSize":
                  message.CharacterWidth = (ushort)GetXmlAttrN(c, "Width");
                  message.CharacterHeight = (byte)GetXmlAttrN(c, "Height");
                  break;
               case "PrintStartDelay":
                  message.PrintStartDelayForward = (ushort)GetXmlAttrN(c, "Forward");
                  message.PrintStartDelayReverse = (ushort)GetXmlAttrN(c, "Reverse");
                  break;
               case "EncoderSettings":
                  message.HiSpeedPrint = ParseEnum<IJPHiSpeedPrintType>(GetXmlAttr(c, "HighSpeedPrinting"));
                  message.PulseRateDivisionFactor = (ushort)GetXmlAttrN(c, "Divisor");
                  message.ProductSpeedMatching = ParseEnum<IJPProductSpeedMatching>(GetXmlAttr(c, "ExternalEncoder"));
                  break;
               case "InkStream":
                  message.InkDropUse = (byte)GetXmlAttrN(c, "InkDropUse");
                  message.InkDropChargeRule = ParseEnum<IJPInkDropChargeRule>(GetXmlAttr(c, "ChargeRule"));
                  break;
               case "Substitution":
                  //SendSubstitution(c);
                  break;
            }
         }
         return success;
      }

      // Get XML Text
      private string GetXmlValue(XmlNode node) {
         if (node != null) {
            return node.InnerText;
         } else {
            return "N_A";
         }
      }

      // Get XML Attribute Value
      private string GetXmlAttr(XmlNode node, string AttrName) {
         XmlNode n;
         if (node != null && (n = node.Attributes[AttrName]) != null) {
            return n.Value;
         } else {
            return "N_A";
         }
      }

      // Get XML Attribute Value
      private long GetXmlAttrN(XmlNode node, string AttrName) {
         XmlNode n;
         if (node != null && (n = node.Attributes[AttrName]) != null) {
            if(long.TryParse(n.Value, out long v)) {
               return v;
            }
         }
         return 0;
      }

      // Convert string back to Enum
      public T ParseEnum<T>(string EnumValue) {
         if (Enum.IsDefined(typeof(T), EnumValue)) {
            return (T)Enum.Parse(typeof(T), EnumValue, true);
         }
         Log($"{typeof(T)}.{EnumValue} is unknown enumeration value");
         return (T)Enum.GetValues(typeof(T)).GetValue(0);
      }

      #endregion

      #region XML Formatting

      // Process an XML Label
      private bool ProcessLabel(string xml) {
         XmlDocument xmlDoc;
         bool result = false;
         int xmlStart;
         int xmlEnd;
         try {
            // Can be called with a Filename or XML text
            xmlStart = xml.IndexOf("<Label");
            if (xmlStart == -1) {
               xml = File.ReadAllText(xml);
               xmlStart = xml.IndexOf("<Label");
            }
            // No label found, exit
            if (xmlStart == -1) {
               return result;
            }
            xmlEnd = xml.IndexOf("</Label>", xmlStart + 7);
            if (xmlEnd > 0) {
               xml = xml.Substring(xmlStart, xmlEnd - xmlStart + 8);
               xmlDoc = new XmlDocument() { PreserveWhitespace = true };
               xmlDoc.LoadXml(xml);
               xml = ToIndentedString(xml);
               xmlStart = xml.IndexOf("<Label");
               if (xmlStart > 0) {
                  xml = xml.Substring(xmlStart);
                  txtXMLIndented.Text = xml;

                  tvXMLTree.Nodes.Clear();
                  tvXMLTree.Nodes.Add(new TreeNode(xmlDoc.DocumentElement.Name));
                  TreeNode tNode = new TreeNode();
                  tNode = tvXMLTree.Nodes[0];

                  AddNode(xmlDoc.DocumentElement, tNode);
                  tvXMLTree.ExpandAll();

                  result = true;
               }
            }
         } catch {

         }
         return result;
      }

      // Convert an XML Document into an indented text string
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

      // Get the attributes associated with a node
      private string GetNameAttr(XmlNode n) {
         string result = n.Name;
         if (n.Attributes != null && n.Attributes.Count > 0) {
            foreach (XmlAttribute attribute in n.Attributes) {
               result += $" {attribute.Name}=\"{attribute.Value}\"";
            }
         }
         return result;
      }

      #endregion

      #region Service routines

      private void ConnectIJP() {
         ConnectIJP(this.ipAddressTextBox.Text, 5000, 5);
      }

      private void ConnectIJP(string ipAddress, int timeout, int retry) {
         if (null != this.ijp) {
            DisconnectIJP();
            this.ijp = null;
         }
         try {

            // Create the IJP object.
            this.ijp = new IJP();

            // Set parameters.
            this.ijp.IPAddress = ipAddress;
            this.ijp.Timeout = timeout;
            this.ijp.Retry = retry;

            // Connect the Ink jet printer.
            this.ijp.Connect();

         } catch (Exception e) {
            Log($"ConnectIJP: {e.Message} \n{e.StackTrace}");
         }
         setButtonEnables();
      }

      private void DisconnectIJP() {
         if (null != this.ijp) {
            this.ijp.Disconnect();
            this.ijp = null;
         }
         setButtonEnables();
      }

      private void ShowCurrentMessage() {
         try {
            if (message == null) {
               // Get the current message.
               message = (IJPMessage)this.ijp.GetMessage();
            }
         } catch (Exception e) {

         }
         setButtonEnables();
      }

      private void BuildTestFileList() {
         cbSelectXMLTest.Items.Clear();
         try {
            string[] FileNames = Directory.GetFiles(txtMessageFolder.Text, "*.XML");
            Array.Sort(FileNames);
            for (int i = 0; i < FileNames.Length; i++) {
               cbSelectXMLTest.Items.Add(Path.GetFileNameWithoutExtension(FileNames[i]));
            }
         } catch {

         }
      }

      private void Log(string s) {
         lstLogs.Items.Add(s);
         lstLogs.Update();
      }

      private void setButtonEnables() {
         bool connected = ijp != null;
         // These must connect first
         cmdComOnOff.Enabled = connected;
         cmdComOnOff.Text = comOn == IJPOnlineStatus.Online ? "Com Off" : "Com On";
         cmdGetViews.Enabled = connected && comOn == IJPOnlineStatus.Online || message != null;
         cmdGetXML.Enabled = connected && comOn == IJPOnlineStatus.Online || message != null;

         cmdRunHardCodedTest.Enabled = comOn == IJPOnlineStatus.Online && cbSelectHardCodedTest.SelectedIndex >= 0;
         cmdRunXMLTest.Enabled = comOn == IJPOnlineStatus.Online && cbSelectXMLTest.SelectedIndex >= 0;

         switch (tclIJPLib.SelectedIndex) {
            case 0:
               cmdSaveAs.Enabled = txtIjpIndented.Text.Length > 0;
               break;
            case 2:
               cmdSaveAs.Enabled = txtXMLIndented.Text.Length > 0;
               break;
            default:
               cmdSaveAs.Enabled = false;
               break;
         }
      }

      #endregion

      #region Test Routines

      string[] AvailableTests = new string[]
         { "New Message", "Retrieve Message", "Send Message", "Clear Screen",
           "Create Message", "Create Complex Message", "Echo" };

      private void cmdRunTest_Click(object sender, EventArgs e) {
         try {
            Cursor.Current = Cursors.WaitCursor;
            Log($"{cbSelectHardCodedTest.Text} Starting");
            switch (cbSelectHardCodedTest.SelectedIndex) {
               case 0:
                  NewMessage();
                  break;
               case 1:
                  RetrieveMessage();
                  break;
               case 2:
                  SendMessage();
                  break;
               case 3:
                  ClearDisplay();
                  break;
               case 4:
                  CreateMessage();
                  break;
               case 5:
                  CreateComplex();
                  break;
               case 6:
                  Echo();
                  break;
               default:
                  break;
            }
         } catch (Exception e2) {
            Log($"Run Test: {e2.Message}\r\n{e2.StackTrace}");
         } finally {
            Log($"{cbSelectHardCodedTest.Text} Complete");
            Cursor.Current = Cursors.Arrow;
         }
      }

      private void CreateComplex() {
         ClearViews();
         message = new IJPMessage();
         message.AddColumn();
         message.SetRow(0, 3);
         message.AddColumn();
         message.SetRow(1, 2);
         for (int i = 0; i < message.Items.Count; i++) {
            message.Items[i].Text = $"Item #{i + 1}";
         }
         message.InkDropUse = 2; // Missing from documentation example
         // Set to IJP.
      }

      private void CreateMessage() {
         ClearViews();
         message = new IJPMessage();
         message.AddColumn();
         message.Items[0].Text = "ABC";
         //message.Items[0].Bold = 5;
         message.InkDropUse = 2; // Missing from documentation example
         // Set to IJP.
         ijp.SetMessage(message);
      }

      private void NewMessage() {
         message = new IJPMessage();
         ClearViews();
      }

      private void ClearViews() {
         tvIJPLibTree.Nodes.Clear();
         tvXMLTree.Nodes.Clear();
         txtIjpIndented.Text = string.Empty;
         txtXMLIndented.Text = string.Empty;
      }

      private void RetrieveMessage() {
         ClearViews();
         message = (IJPMessage)ijp.GetMessage();
      }

      private void SendMessage() {
         ijp.SetMessage(message);
      }

      private void Echo() {
         message = (IJPMessage)this.ijp.GetMessage();
         Log("Message Retrieved");
         ijp.SetMessage(message);
      }

      private void ClearDisplay() {
         ClearViews();
         message = new IJPMessage();
         message.InkDropUse = 2; // Missing from documentation example
         message.AddColumn();
         message.AddItem();
         message.Items[0].Text = "X";
         //ijp.SetMessage(message);
      }

      #endregion

   }
}
