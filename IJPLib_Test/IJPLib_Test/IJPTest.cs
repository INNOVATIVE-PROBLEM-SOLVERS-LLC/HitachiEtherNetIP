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

            Utils.ResizeObject(ref R, lblMessageFolder, 1, 19, 2, 4);
            Utils.ResizeObject(ref R, txtMessageFolder, 1, 24, 2, 10);
            Utils.ResizeObject(ref R, cmdBrowse, 1, 35, 2, 4);

            Utils.ResizeObject(ref R, tclIJPTests, 4, 1, 45, 38);

            Utils.ResizeObject(ref R, tclIJPLib, 2, 1, 40, 31);
            Utils.ResizeObject(ref R, cmdGetXML, 4, 33, 2, 4);
            Utils.ResizeObject(ref R, cmdGetViews, 7, 33, 2, 4);
            Utils.ResizeObject(ref R, cmdSaveAs, 10, 33, 2, 4);

            Utils.ResizeObject(ref R, txtIjpIndented, 1, 1, 36, 29);
            Utils.ResizeObject(ref R, tvIJPLibTree, 1, 1, 36, 29);
            Utils.ResizeObject(ref R, txtXMLIndented, 1, 1, 36, 29);
            Utils.ResizeObject(ref R, tvXMLTree, 1, 1, 36, 29);

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

      private void cmdGetViews_Click(object sender, EventArgs e) {
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
         // Restore cursor
         Cursor.Current = Cursors.Arrow;
      }

      private void cmdGetXML_Click(object sender, EventArgs e) {
         //  Set hour glass
         Cursor.Current = Cursors.WaitCursor;
         // Out with the old
         txtXMLIndented.Text = string.Empty;
         ShowCurrentMessage();
         txtXMLIndented.Text = RetrieveXML(message);
         ProcessLabel(txtXMLIndented.Text);
         // Restore cursor
         Cursor.Current = Cursors.Arrow;
      }

      private void ccmdSaveAs_Click(object sender, EventArgs e) {
         switch (tclIJPLib.SelectedIndex) {
            case 0:

               break;
            case 2:

               break;
            default:
               break;
         }
      }

      private void cmdClear_Click(object sender, EventArgs e) {
         Log("Message Cleared");
         message = null;
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

      #endregion

      #region Message to XML 

      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
      }

      public string RetrieveXML(IJPMessage m) {
         string xml = string.Empty;
         ItemType itemType;
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
                                 int calBlocks = m.Items[i].CalendarBlockCount;
                                 int cntBlocks = m.Items[i].CountBlockCount;
                                 int[] mask = new int[1 + Math.Max(calBlocks, cntBlocks)];
                                 itemType = GetItemType(text, ref mask);
                                 writer.WriteStartElement("Item"); // Start Item
                                 {
                                    RetrieveFont(writer, (IJPMessageItem)m.Items[item]);
                                    switch (itemType) {
                                       case ItemType.Text:
                                          break;
                                       case ItemType.Date:
                                          // Missing multiple calendar block logic
                                          RetrieveCalendarSettings(writer, (IJPMessageItem)m.Items[i], m.CalendarConditions, mask);
                                          RetrieveShiftSettings(writer, m.ShiftCodes, mask);
                                          RetrieveTimeCountSettings(writer, m.TimeCount, mask);
                                          break;
                                       case ItemType.Counter:
                                          // Missing multiple counter block logic
                                          RetrieveCounterSettings(writer, (IJPMessageItem)m.Items[i], m.CountConditions);
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

      private void RetrieveCalendarSettings(XmlTextWriter writer, IJPMessageItem mi, IJPCalendarConditionCollection cc, int[] mask) {
         int FirstBlock = mi.CalendarBlockNumber;
         int BlockCount = mi.CalendarBlockCount;

         for (int i = 0; i < BlockCount; i++) {
            IJPCalendarCondition c = cc[FirstBlock + i - 1];
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

      private void RetrieveCounterSettings(XmlTextWriter writer, IJPMessageItem mi, IJPCountConditionCollection cc) {
         int FirstBlock = mi.CountBlockNumber;
         int BlockCount = mi.CountBlockCount;

         for (int i = 0; i < BlockCount; i++) {
            IJPCountCondition c = cc[FirstBlock + i - 1];
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

      // Send xlmDoc from file to printer
      public bool SendXmlToPrinter(XmlDocument xmlDoc) {
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

      private bool AllocateRowsColumns(XmlNodeList childNodes) {
         bool success = true;

         return success;
      }

      private bool SendPrinterSettings(XmlNode prnt) {
         bool success = true;

         return success;
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
         { "New Message", "Retrieve Message", "Send Message", "Clear Screen", "Create Message", "Create Complex Message" };

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
                  Echo();
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
               default:
                  break;
            }
         } catch (Exception e2) {
            Log($"ConnectIJP: {e2.Message}\r\n{e2.StackTrace}");
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
         ClearViews();
         message = (IJPMessage)this.ijp.GetMessage();
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
