using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Serialization;

namespace Modbus_DLL {
   public partial class SendRetrieveXML {

      #region Events

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(object sender, string msg);

      #endregion

      #region Data Declarations

      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
         Link = 5,     // Not supported in the printer
         Prompt = 6,   // Not supported in the printer
      }

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
        (int)ba.Julian | (int)ba.Week | (int)ba.DayOfWeek | (int)ba.Shift | (int)ba.TimeCount;

      const int DateSubZS =
         (int)ba.Year | (int)ba.Month | (int)ba.Day | (int)ba.Hour | (int)ba.Minute |
         (int)ba.Week | (int)ba.DayOfWeek;

      // Flag for Attribute Not Present
      const string N_A = "N!A";

      // Currently active printer
      Modbus p;

      // XML I/O Logging
      private MemoryStream XMLms;
      private XmlTextWriter XMLwriter;
      public string LogXML;

      // Structures for retrieving logos
      enum logoLayout {
         Free = 0,
         Fixed = 1,
      }
      struct logoInfo {
         public logoLayout layout;
         public string dotMatrix;
         public int registration;
      }

      #endregion

      #region Constructors and destructors

      public SendRetrieveXML(Modbus printer) {
         p = printer;
      }

      public string Retrieve() {
         string xml = string.Empty;
         if (p.GetDecAttribute(ccIJP.Online_Offline) == 0) {
            p.SetAttribute(ccIJP.Online_Offline, 1);
            if (p.GetDecAttribute(ccIJP.Online_Offline) == 0) {
               Log?.Invoke(p, "Cannot turn com on!  Retrieve aborted!");
               return xml;
            }
         }
         try {
            XMLms = new MemoryStream();
            XMLwriter = new XmlTextWriter(XMLms, Encoding.GetEncoding("UTF-8"));
            XMLwriter.Formatting = Formatting.Indented;
            XMLwriter.WriteStartDocument();
            XMLwriter.WriteStartElement("Retrieve"); // Start Retrieve
            p.Log += P_Log;
            Lab Label = new Lab() { Version = "Serialization-1" };
            Label.Message = new Msg[p.NozzleCount];
            Label.Printer = new Printer[p.NozzleCount];
            for (int nozzle = 0; nozzle < p.NozzleCount; nozzle++) {
               p.Nozzle = nozzle;

               XMLwriter.WriteStartElement("Message");
               Label.Message[nozzle] = RetrieveMessage();
               XMLwriter.WriteEndElement();

               XMLwriter.WriteStartElement("PrinterSettings");
               Label.Printer[nozzle] = RetrievePrinterSettings();
               XMLwriter.WriteEndElement();

               XMLwriter.WriteStartElement("Substitutions");
               Label.Printer[nozzle].Substitution = RetrieveSubstitutions(Label.Message[nozzle]);
               XMLwriter.WriteEndElement();

               Label.Message[nozzle].Nozzle = (nozzle + 1).ToString();
               Label.Printer[nozzle].Nozzle = (nozzle + 1).ToString();
            }
            XMLwriter.WriteStartElement("Logos");
            RetrieveLogos(Label);
            XMLwriter.WriteEndElement();
            XmlSerializer serializer = new XmlSerializer(typeof(Lab));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using (MemoryStream ms = new MemoryStream()) {
               serializer.Serialize(ms, Label, ns);
               ms.Position = 0;
               xml = new StreamReader(ms).ReadToEnd();
            }
         } catch (Exception e2) {
            Log?.Invoke(p, e2.Message);
         } finally {
            p.Log -= P_Log;
            XMLwriter.WriteEndElement(); // End Retrieve
            XMLwriter.WriteEndDocument();
            XMLwriter.Flush();
            XMLms.Position = 0;
            using (StreamReader sr = new StreamReader(XMLms)) {
               LogXML = sr.ReadToEnd();
            }
            XMLwriter.Close();
            XMLms.Close();
            XMLwriter = null;
            XMLms = null;
         }
         return xml;
      }

      #endregion

      #region Retrieve Message

      // Retrieve the Message portion of the XML
      private Msg RetrieveMessage() {
         Log?.Invoke(p, $" \n// Retrieving Message Layout\n ");
         Msg m = new Msg();
         m.Layout = p.GetHRAttribute(ccPF.Format_Setup);
         RetrieveRowsColumns(m);
         return m;
      }

      // Retrieve row/column/items
      private void RetrieveRowsColumns(Msg m) {
         Log?.Invoke(p, $" \n// Retrieving Rows and Columns\n ");
         XMLwriter.WriteStartElement("ColumnLayout");
         int itemCount = p.GetDecAttribute(ccIDX.Number_Of_Items);
         int lineCount;
         int n = 0;
         List<int> cols = new List<int>();            // Holds the number of rows in each column
         List<string> spacing = new List<string>();   // Holds the line spacing
         while (n < itemCount) {
            cols.Add(lineCount = p.GetDecAttribute(ccPF.Line_Count, n));
            spacing.Add(p.GetHRAttribute(ccPF.Line_Spacing, n));
            n += lineCount;
         }
         XMLwriter.WriteEndElement();

         // Fill in the items
         m.Column = new Column[cols.Count];                               // Allocate the columns array  
         n = 0;
         int totalCharacters = 0;
         for (int col = 0; col < m.Column.Length; col++) {
            XMLwriter.WriteStartElement("Column");
            XMLwriter.WriteAttributeString("Column", (col + 1).ToString());
            m.Column[col] = new Column();                                 // Allocate the column
            m.Column[col].InterLineSpacing = spacing[col];
            m.Column[col].Item = new Item[cols[col]];                     // Allocate the items array
            for (int row = 0; row < m.Column[col].Item.Length; row++) {
               Log?.Invoke(p, $" \n// Retrieving Item in Column {col + 1} Row {row + 1}\n ");
               XMLwriter.WriteStartElement("Item");
               XMLwriter.WriteAttributeString("Row", (row + 1).ToString());
               Item item = new Item();                                    // Allocate the item
               int characterCount = p.GetDecAttribute(ccPC.Characters_per_Item, n);
               if (characterCount > 0) {
                  item.Text = p.GetHRAttribute(ccPC.Print_Character_String, totalCharacters, characterCount);
               } else {
                  item.Text = string.Empty;
               }
               item.Font = new FontDef();                                 // Build font definition
               item.Font.DotMatrix = p.GetHRAttribute(ccPF.Dot_Matrix, n);
               item.Font.InterCharacterSpace = p.GetDecAttribute(ccPF.InterCharacter_Space, n);
               item.Font.IncreasedWidth = p.GetDecAttribute(ccPF.Character_Bold, n);
               int bcType = p.GetDecAttribute(ccPF.Barcode_Type, n);
               if (bcType > 0) {
                  item.BarCode = new BarCode();                           // Build barcode only if needed
                  item.BarCode.DotMatrix = p.GetHRAttribute(ccPF.Barcode_Type, n);
                  item.BarCode.HumanReadableFont = p.GetHRAttribute(ccPF.Readable_Code, n);
                  item.BarCode.EANPrefix = p.GetHRAttribute(ccAPP.EAN_Prefix, n);
               }

               item.Location = new Location() { Index = n, Row = row, Col = col };
               if (m.Layout == "FreeLayout") {
                  item.Location.X = p.GetDecAttribute(ccPF.X_Coordinate, n);
                  item.Location.Y = p.GetDecAttribute(ccPF.Y_Coordinate, n);
               }
               int[] mask = new int[1 + 8];
               ItemType itemType = GetItemType(item.Text, ref mask);
               item.Location.calCount = p.GetDecAttribute(ccPF.Number_of_Calendar_Blocks, n);
               item.Location.countCount = p.GetDecAttribute(ccPF.Number_Of_Count_Blocks, n);
               if (item.Location.calCount > 0) {
                  item.Location.calStart = p.GetDecAttribute(ccPF.First_Calendar_Block, n);
                  Log?.Invoke(p, $" \n// Retrieving Calendar Block {item.Location.calStart}\n ");
                  RetrieveCalendarSettings(item, mask);
               }
               if (item.Location.countCount > 0) {
                  item.Location.countStart = p.GetDecAttribute(ccPF.First_Count_Block, n);
                  Log?.Invoke(p, $" \n// Retrieving Count Block {item.Location.countStart}\n ");
                  RetrieveCountSettings(item);
               }
               m.Column[col].Item[row] = item;
               n++;
               totalCharacters += characterCount;
               XMLwriter.WriteEndElement();
            }
            XMLwriter.WriteEndElement();
         }
      }

      // Retrieve Calendar settings
      private void RetrieveCalendarSettings(Item item, int[] mask) {
         int n = item.Location.calStart - 1;
         item.Date = new Date[item.Location.calCount];
         for (int i = 0; i < item.Location.calCount; i++) {
            XMLwriter.WriteStartElement("Calendar");
            XMLwriter.WriteAttributeString("Block", (item.Location.calStart + i).ToString());
            // Where do you get Substitution rule number
            item.Date[i] = new Date() { Block = i + 1 };
            if ((mask[i] & DateOffset) > 0) {
               XMLwriter.WriteStartElement("Offset");
               item.Date[i].SubstitutionRule = "1";
               item.Date[i].RuleName = "";
               item.Date[i].Offset = new Offset() {
                  Year = p.GetHRAttribute(ccCal.Offset_Year, n),
                  Month = p.GetHRAttribute(ccCal.Offset_Month, n),
                  Day = p.GetHRAttribute(ccCal.Offset_Day, n),
                  Hour = p.GetHRAttribute(ccCal.Offset_Hour, n),
                  Minute = p.GetHRAttribute(ccCal.Offset_Minute, n)
               };
               XMLwriter.WriteEndElement();
            }
            if ((mask[i] & DateSubZS) > 0) {
               XMLwriter.WriteStartElement("ZeroSuppress");
               item.Date[i].ZeroSuppress = new ZeroSuppress();
               string s;
               if ((mask[i] & (int)ba.Year) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Year, n)))
                     item.Date[i].ZeroSuppress.Year = s;
               if ((mask[i] & (int)ba.Month) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Month, n)))
                     item.Date[i].ZeroSuppress.Month = s;
               if ((mask[i] & (int)ba.Day) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Day, n)))
                     item.Date[i].ZeroSuppress.Day = s;
               if ((mask[i] & (int)ba.Hour) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Hour, n)))
                     item.Date[i].ZeroSuppress.Hour = s;
               if ((mask[i] & (int)ba.Minute) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Minute, n)))
                     item.Date[i].ZeroSuppress.Minute = s;
               if ((mask[i] & (int)ba.Week) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_Weeks, n)))
                     item.Date[i].ZeroSuppress.Week = s;
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  if (!IsDefaultValue(fmtDD.DisableSpaceChar, s = p.GetHRAttribute(ccCal.Zero_Suppress_DayOfWeek, n)))
                     item.Date[i].ZeroSuppress.DayOfWeek = s;
               XMLwriter.WriteEndElement();

               XMLwriter.WriteStartElement("Substitute");
               item.Date[i].Substitute = new Substitute();
               if ((mask[i] & (int)ba.Year) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Year, n)))
                     item.Date[i].Substitute.Year = s;
               if ((mask[i] & (int)ba.Month) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Month, n)))
                     item.Date[i].Substitute.Month = s;
               if ((mask[i] & (int)ba.Day) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Day, n)))
                     item.Date[i].Substitute.Day = s;
               if ((mask[i] & (int)ba.Hour) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Hour, n)))
                     item.Date[i].Substitute.Hour = s;
               if ((mask[i] & (int)ba.Minute) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Minute, n)))
                     item.Date[i].Substitute.Minute = s;
               if ((mask[i] & (int)ba.Week) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_Weeks, n)))
                     item.Date[i].Substitute.Week = s;
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  if (!IsDefaultValue(fmtDD.EnableDisable, s = p.GetHRAttribute(ccCal.Substitute_DayOfWeek, n)))
                     item.Date[i].Substitute.DayOfWeek = s;
               XMLwriter.WriteEndElement();
            }
            if ((mask[i] & (int)ba.Shift) > 0) {
               item.Date[i].Shift = RetrieveShifts();
            }
            if ((mask[i] & (int)ba.TimeCount) > 0) {
               item.Date[i].TimeCount = RetrieveTimeCount();
            }
            n++;
            XMLwriter.WriteEndElement();
         }
      }

      // Retrieve Count Settings
      private void RetrieveCountSettings(Item item) {
         int n = item.Location.countCount - 1;
         item.Counter = new Counter[item.Location.countCount];
         for (int i = 0; i < item.Location.countCount; i++) {
            XMLwriter.WriteStartElement("Count");
            XMLwriter.WriteAttributeString("Block", (item.Location.countStart + i).ToString());
            item.Counter[i] = new Counter() { Block = i + 1 };
            item.Counter[i].Range = new Range() {
               Range1 = p.GetHRAttribute(ccCount.Count_Range_1, n),
               Range2 = p.GetHRAttribute(ccCount.Count_Range_2, n),
               JumpFrom = p.GetHRAttribute(ccCount.Jump_From, n),
               JumpTo = p.GetHRAttribute(ccCount.Jump_To, n),
            };
            item.Counter[i].Count = new Count() {
               InitialValue = p.GetHRAttribute(ccCount.Initial_Value, n),
               Increment = p.GetHRAttribute(ccCount.Increment_Value, n),
               Direction = p.GetHRAttribute(ccCount.Direction_Value, n),
               ZeroSuppression = p.GetHRAttribute(ccCount.Zero_Suppression, n),
            };
            item.Counter[i].Reset = new Reset() {
               Type = p.GetHRAttribute(ccCount.Type_Of_Reset_Signal, n),
               Value = p.GetHRAttribute(ccCount.Reset_Value, n),
            };
            item.Counter[i].Misc = new Misc() {
               UpdateIP = p.GetHRAttribute(ccCount.Update_Unit_Halfway, n),
               UpdateUnit = p.GetHRAttribute(ccCount.Update_Unit_Unit, n),
               ExternalCount = p.GetHRAttribute(ccCount.External_Count, n),
               Multiplier = p.GetHRAttribute(ccCount.Count_Multiplier, n),
               SkipCount = p.GetHRAttribute(ccCount.Count_Skip, n),
            };
            n++;
            XMLwriter.WriteEndElement();
         }
      }

      // Retrieve shift settings
      private Shift[] RetrieveShifts() {
         XMLwriter.WriteStartElement("Shifts");
         List<Shift> s = new List<Shift>();
         string endHour;
         string endMinute;
         int n = 0;
         do {
            Log?.Invoke(p, $" \n// Retrieving Shift {n + 1}\n ");
            XMLwriter.WriteStartElement("Shift");
            XMLwriter.WriteAttributeString("Shift", (n + 1).ToString());
            s.Add(new Shift() {
               ShiftNumber = n + 1,
               StartHour = p.GetHRAttribute(ccSR.Shift_Start_Hour, n),
               StartMinute = p.GetHRAttribute(ccSR.Shift_Start_Minute, n),
               EndHour = endHour = p.GetHRAttribute(ccSR.Shift_End_Hour, n),
               EndMinute = endMinute = p.GetHRAttribute(ccSR.Shift_End_Minute, n),
               ShiftCode = p.GetHRAttribute(ccSR.Shift_String_Value, n),
            });
            n++;
            XMLwriter.WriteEndElement();
         } while (endHour != "23" || endMinute != "59");
         XMLwriter.WriteEndElement();
         return s.ToArray();
      }

      // Retrieve Time count settings
      private TimeCount RetrieveTimeCount() {
         XMLwriter.WriteStartElement("TimeCount");
         TimeCount TimeCount = new TimeCount() {
            Interval = p.GetHRAttribute(ccSR.Update_Interval_Value),
            Start = p.GetHRAttribute(ccSR.Time_Count_Start_Value),
            End = p.GetHRAttribute(ccSR.Time_Count_End_Value),
            ResetTime = p.GetHRAttribute(ccSR.Reset_Time_Value),
            ResetValue = p.GetHRAttribute(ccSR.Time_Count_Reset_Value),
         };
         XMLwriter.WriteEndElement();
         return TimeCount;
      }

      #endregion

      #region Retrieve printer settings

      // Retrieve printer settings
      private Printer RetrievePrinterSettings() {
         Log?.Invoke(p, $" \n// Retrieving Printer Settings\n ");
         Printer ptr = new Printer() {
            Make = "Hitachi",

            Model = p.GetHRAttribute(ccUI.Model_Name),
            PrintHead = new PrintHead() {
               Orientation = p.GetHRAttribute(ccPS.Character_Orientation)
            },
            ContinuousPrinting = new ContinuousPrinting() {
               RepeatInterval = p.GetHRAttribute(ccPS.Repeat_Interval),
               PrintsPerTrigger = p.GetHRAttribute(ccPS.Repeat_Count)
            },
            TargetSensor = new TargetSensor() {
               Filter = p.GetHRAttribute(ccPS.Target_Sensor_Filter),
               SetupValue = p.GetHRAttribute(ccPS.Target_Sensor_Filter_Value),
               Timer = p.GetHRAttribute(ccPS.Target_Sensor_Timer)
            },
            CharacterSize = new CharacterSize() {
               Width = p.GetHRAttribute(ccPS.Character_Width),
               Height = p.GetHRAttribute(ccPS.Character_Height)
            },
            PrintStartDelay = new PrintStartDelay() {
               Forward = p.GetHRAttribute(ccPS.Print_Start_Delay_Forward),
               Reverse = p.GetHRAttribute(ccPS.Print_Start_Delay_Reverse)
            },
            EncoderSettings = new EncoderSettings() {
               HighSpeedPrinting = p.GetHRAttribute(ccPS.High_Speed_Print),
               Divisor = p.GetHRAttribute(ccPS.Pulse_Rate_Division_Factor),
               ExternalEncoder = p.GetHRAttribute(ccPS.Product_Speed_Matching)
            },
            InkStream = new InkStream() {
               InkDropUse = p.GetHRAttribute(ccPS.Ink_Drop_Use),
               ChargeRule = p.GetHRAttribute(ccPS.Ink_Drop_Charge_Rule)
            },
         };
         return ptr;
      }

      // Retrieve all logos
      private void RetrieveLogos(Lab lab) {
         for (int nozzle = 0; nozzle < p.NozzleCount; nozzle++) {
            Msg m = lab.Message[nozzle];
            // Find out which logos are used
            List<logoInfo> neededLogo = new List<logoInfo>();
            int n;
            for (int c = 0; c < m.Column.Length; c++) {
               for (int r = 0; r < m.Column[c].Item.Length; r++) {
                  Item item = m.Column[c].Item[r];
                  if (!string.IsNullOrEmpty(item.Text)) {
                     string s = p.HandleBraces(item.Text);
                     for (int i = 0; i < s.Length; i++) {
                        switch (s[i] >> 8) {
                           case '\xF6':
                              n = (s[i] & 0xFF) - 0x40;
                              if (n >= 0 && n < 50) {
                                 neededLogo.Add(new logoInfo() { layout = logoLayout.Free, registration = n, dotMatrix = "N/A" });
                              }
                              break;
                           case '\xF1':
                              n = (s[i] & 0xFF) - 0x40;
                              if (n >= 0) {
                                 neededLogo.Add(new logoInfo() { layout = logoLayout.Fixed, registration = n, dotMatrix = item.Font.DotMatrix });
                              }
                              break;
                           case '\xF2':
                              n = (s[i] & 0xFF) - 0x20;
                              if (n >= 0 && n < 8) {
                                 neededLogo.Add(new logoInfo() { layout = logoLayout.Fixed, registration = n + 192, dotMatrix = item.Font.DotMatrix });
                              }
                              break;
                        }
                     }
                  }
               }
            }
            // Eliminate Duplicates
            neededLogo = (neededLogo.OrderBy(nl => nl.layout).ToList()).OrderBy(nl => nl.registration).ToList().OrderBy(nl => nl.dotMatrix).ToList();
            int j = 0;
            while (j < neededLogo.Count - 1) {
               if (neededLogo[j].layout == neededLogo[j + 1].layout &&
                  neededLogo[j].registration == neededLogo[j + 1].registration &&
                  neededLogo[j].dotMatrix == neededLogo[j + 1].dotMatrix) {
                  neededLogo.RemoveAt(j + 1);
               } else {
                  j++;
               }
            }
            // List of retrieved logos
            List<Logo> retrievedLogos = new List<Logo>();
            // Retrieve any fixed logos
            for (int i = 0; i < neededLogo.Count; i++) {
               switch (neededLogo[i].layout) {
                  case logoLayout.Free:
                     Log?.Invoke(p, $" \n// Retrieving Free Logo {neededLogo[i].registration}\n ");
                     XMLwriter.WriteStartElement("FreeLogo");
                     if (p.GetFreeLogo(neededLogo[i].registration, out int width, out int height, out byte[] freeData)) {
                        Logo logo = new Logo() {
                           Location = neededLogo[i].registration,
                           Width = width,
                           Height = height,
                           RawData = p.byte_to_string(freeData),
                           Layout = "Free"
                        };
                        retrievedLogos.Add(logo);
                     }
                     XMLwriter.WriteEndElement();
                     break;
                  case logoLayout.Fixed:
                     Log?.Invoke(p, $" \n// Retrieving Fixed Logo:  Dot Matrix {neededLogo[i].dotMatrix}, Location {neededLogo[i].registration}\n ");
                     XMLwriter.WriteStartElement("FIxedLogo");
                     if (p.GetFixedLogo(neededLogo[i].dotMatrix, neededLogo[i].registration, out byte[] fixedData)) {
                        Logo logo = new Logo() {
                           Location = neededLogo[i].registration,
                           DotMatrix = neededLogo[i].dotMatrix,
                           RawData = p.byte_to_string(fixedData),
                           Layout = "Fixed"
                        };
                        retrievedLogos.Add(logo);
                     }
                     XMLwriter.WriteEndElement();
                     break;
                  default:
                     break;
               }
               if (retrievedLogos.Count > 0) {
                  Printer pr = lab.Printer[nozzle];
                  pr.Logos = new Logos() { Logo = retrievedLogos.ToArray() };
               }
            }
         }
      }

      // Retrieve Substitution rules
      private Substitution RetrieveSubstitutions(Msg m) {
         bool needYear = false;
         bool needMonth = false;
         bool needDay = false;
         bool needHour = false;
         bool needMinute = false;
         bool needWeek = false;
         bool needDayOfWeek = false;
         string ruleNumber = "01";
         for (int c = 0; c < m.Column.Length; c++) {
            Column col = m.Column[c];
            for (int r = 0; r < col.Item.Length; r++) {
               Item item = col.Item[r];
               if (item.Date != null) {
                  for (int i = 0; i < item.Date.Length; i++) {
                     ruleNumber = item.Date[i].SubstitutionRule;
                     Substitute sub = item.Date[i].Substitute;
                     if (sub != null) {
                        needYear |= sub.Year != null;
                        needMonth |= sub.Month != null;
                        needDay |= sub.Day != null;
                        needHour |= sub.Hour != null;
                        needMinute |= sub.Minute != null;
                        needDayOfWeek |= sub.DayOfWeek != null;
                        needWeek |= sub.Week != null;
                     }
                  }
               }
            }
         }
         // Need to load the rule (just use 1 for now)
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccIDX.Substitution_Rule, 1);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         string startYear = p.GetHRAttribute(ccSR.Start_Year);

         List<SubstitutionRule> sr = new List<SubstitutionRule>();
         if (needYear)
            RetrieveSubstitution(sr, ccSR.Year);
         if (needMonth)
            RetrieveSubstitution(sr, ccSR.Month);
         if (needDay)
            RetrieveSubstitution(sr, ccSR.Day);
         if (needHour)
            RetrieveSubstitution(sr, ccSR.Hour);
         if (needMinute)
            RetrieveSubstitution(sr, ccSR.Minute);
         if (needWeek)
            RetrieveSubstitution(sr, ccSR.Week);
         if (needDayOfWeek)
            RetrieveSubstitution(sr, ccSR.DayOfWeek);
         Substitution substitution = new Substitution() {
            Delimiter = "/",
            StartYear = startYear,
            RuleNumber = "1",
            SubRule = sr.ToArray()
         };
         return substitution;
      }

      // Retrieve one substitution type
      private void RetrieveSubstitution(List<SubstitutionRule> sr, ccSR rule) {
         Log?.Invoke(p, $" \n// Retrieving substitution for {rule}\n ");
         XMLwriter.WriteStartElement("Substitution");
         XMLwriter.WriteAttributeString("Rule", rule.ToString());
         AttrData attr = p.GetAttrData(rule);
         int n = (int)(attr.Data.Max - attr.Data.Min + 1);
         string[] subCode = new string[n];
         for (int i = 0; i < n; i++) {
            subCode[i] = p.GetHRAttribute(rule, i);
         }
         for (int i = 0; i < n; i += 10) {
            sr.Add(new SubstitutionRule() {
               Type = rule.ToString().Replace("_", ""),
               Base = (i + attr.Data.Min).ToString(),
               Text = string.Join("/", subCode, i, Math.Min(10, n - i)),
            });
         }
         XMLwriter.WriteEndElement();
      }

      #endregion

      #region Service Routines

      // Check if string value is first entry in dropdown
      bool IsDefaultValue(fmtDD fmt, string s) {
         if (string.IsNullOrEmpty(s)) {
            return true;
         }
         if (int.TryParse(s, out int val)) {
            return val == 0;
         }
         if (bool.TryParse(s, out bool b)) {
            return !b;
         }
         s = s.ToLower();
         val = Array.FindIndex(Data.DropDowns[(int)fmt], x => x.ToLower().Contains(s));
         if (val < 0) {
            val = Array.FindIndex(Data.DropDownsIJPLib[(int)fmt], x => x.ToLower().Contains(s));
         }
         return val == 0;
      }

      // Examine the contents of a print message to determine its type
      ItemType GetItemType(string text, ref int[] mask) {
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

      #endregion

   }
}
