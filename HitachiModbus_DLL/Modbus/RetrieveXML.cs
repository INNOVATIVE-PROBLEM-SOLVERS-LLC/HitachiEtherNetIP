using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Serialization;
using UTF8vsHitachiCodes;


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
            Lab Label = new Lab() { Version = "Serialization-1" };
            Label.Message = new Msg[p.NozzleCount];
            Label.Printer = new Printer[p.NozzleCount];
            for (int nozzle = 0; nozzle < p.NozzleCount; nozzle++) {
               p.Nozzle = nozzle;

               Label.Message[nozzle] = RetrieveMessage();

               Label.Printer[nozzle] = RetrievePrinterSettings();

               Label.Printer[nozzle].Substitution = RetrieveSubstitutions(Label.Message[nozzle]);

               Label.Message[nozzle].Nozzle = nozzle + 1;
               Label.Printer[nozzle].Nozzle = nozzle + 1;
            }
            RetrieveLogos(Label);

            xml = Serializer<Lab>.ClassToXml(Label);

         } catch (Exception e2) {
            Log?.Invoke(p, e2.Message);
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
         int itemCount = p.GetDecAttribute(ccIDX.Number_Of_Items);
         int lineCount;
         int n = 0;
         List<int> cols = new List<int>();            // Holds the number of rows in each column
         List<int> spacing = new List<int>();   // Holds the line spacing
         while (n < itemCount) {
            cols.Add(lineCount = p.GetDecAttribute(ccPF.Line_Count, n));
            spacing.Add(p.GetDecAttribute(ccPF.Line_Spacing, n));
            n += lineCount;
         }

         // Fill in the items
         m.Column = new Column[cols.Count];                               // Allocate the columns array  
         n = 0;
         int totalCharacters = 0;
         for (int col = 0; col < m.Column.Length; col++) {
            m.Column[col] = new Column();                                 // Allocate the column
            m.Column[col].InterLineSpacing = spacing[col];
            m.Column[col].Item = new Item[cols[col]];                     // Allocate the items array
            for (int row = 0; row < m.Column[col].Item.Length; row++) {
               Log?.Invoke(p, $" \n// Retrieving Item in Column {col + 1} Row {row + 1}\n ");
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

               item.Location = new Location() { Index = n + 1, Row = row + 1, Col = col + 1 };
               if (m.Layout == "FreeLayout") {
                  item.Location.X = p.GetDecAttribute(ccPF.X_Coordinate, n);
                  item.Location.Y = p.GetDecAttribute(ccPF.Y_Coordinate, n);
               } else {
                  item.Location.X = -1;
                  item.Location.Y = -1;
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
            }
         }
      }

      // Retrieve Calendar settings
      private void RetrieveCalendarSettings(Item item, int[] mask) {
         int n = item.Location.calStart - 1;
         item.Date = new Date[item.Location.calCount];
         for (int i = 0; i < item.Location.calCount; i++) {
            // Where do you get Substitution rule number
            item.Date[i] = new Date() { Block = i + 1 };
            if ((mask[i] & DateOffset) > 0) {
               item.Date[i].SubstitutionRule = "1";
               item.Date[i].RuleName = "";
               item.Date[i].Offset = new Offset() {
                  Year = p.GetDecAttribute(ccCal.Offset_Year, n),
                  Month = p.GetDecAttribute(ccCal.Offset_Month, n),
                  Day = p.GetDecAttribute(ccCal.Offset_Day, n),
                  Hour = p.GetDecAttribute(ccCal.Offset_Hour, n),
                  Minute = p.GetDecAttribute(ccCal.Offset_Minute, n)
               };
            }
            if ((mask[i] & DateSubZS) > 0) {
               item.Date[i].ZeroSuppress = new ZeroSuppress();
               if ((mask[i] & (int)ba.Year) > 0)
                  item.Date[i].ZeroSuppress.Year = (ZS)p.GetDecAttribute(ccCal.Zero_Suppress_Year, n);
               if ((mask[i] & (int)ba.Month) > 0)
                  item.Date[i].ZeroSuppress.Month = (ZS)p.GetDecAttribute(ccCal.Zero_Suppress_Month, n);
               if ((mask[i] & (int)ba.Day) > 0)
                  item.Date[i].ZeroSuppress.Day = (ZS)p.GetDecAttribute(ccCal.Zero_Suppress_Day, n);
               if ((mask[i] & (int)ba.Hour) > 0)
                  item.Date[i].ZeroSuppress.Hour = (ZS)p.GetDecAttribute(ccCal.Zero_Suppress_Hour, n);
               if ((mask[i] & (int)ba.Minute) > 0)
                  item.Date[i].ZeroSuppress.Minute = (ZS)p.GetDecAttribute(ccCal.Zero_Suppress_Minute, n);
               if ((mask[i] & (int)ba.Week) > 0)
                  item.Date[i].ZeroSuppress.Week = (ZS)p.GetDecAttribute(ccCal.Zero_Suppress_Weeks, n);
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  item.Date[i].ZeroSuppress.DayOfWeek = (ZS)p.GetDecAttribute(ccCal.Zero_Suppress_DayOfWeek, n);

               item.Date[i].Substitute = new Substitute();
               if ((mask[i] & (int)ba.Year) > 0)
                  item.Date[i].Substitute.Year = (ED)p.GetDecAttribute(ccCal.Substitute_Year, n);
               if ((mask[i] & (int)ba.Month) > 0)
                  item.Date[i].Substitute.Month = (ED)p.GetDecAttribute(ccCal.Substitute_Month, n);
               if ((mask[i] & (int)ba.Day) > 0)
                  item.Date[i].Substitute.Day = (ED)p.GetDecAttribute(ccCal.Substitute_Day, n);
               if ((mask[i] & (int)ba.Hour) > 0)
                  item.Date[i].Substitute.Hour = (ED)p.GetDecAttribute(ccCal.Substitute_Hour, n);
               if ((mask[i] & (int)ba.Minute) > 0)
                  item.Date[i].Substitute.Minute = (ED)p.GetDecAttribute(ccCal.Substitute_Minute, n);
               if ((mask[i] & (int)ba.Week) > 0)
                  item.Date[i].Substitute.Week = (ED)p.GetDecAttribute(ccCal.Substitute_Weeks, n);
               if ((mask[i] & (int)ba.DayOfWeek) > 0)
                  item.Date[i].Substitute.DayOfWeek = (ED)p.GetDecAttribute(ccCal.Substitute_DayOfWeek, n);
            }
            if ((mask[i] & (int)ba.Shift) > 0) {
               item.Date[i].Shifts = RetrieveShifts();
            }
            if ((mask[i] & (int)ba.TimeCount) > 0) {
               item.Date[i].TimeCount = RetrieveTimeCount();
            }
            n++;
         }
      }

      // Retrieve Count Settings
      private void RetrieveCountSettings(Item item) {
         int n = item.Location.countCount - 1;
         item.Counter = new Counter[item.Location.countCount];
         for (int i = 0; i < item.Location.countCount; i++) {
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
         }
      }

      // Retrieve shift settings
      private Shift[] RetrieveShifts() {
         List<Shift> s = new List<Shift>();
         string endHour;
         string endMinute;
         int n = 0;
         do {
            Log?.Invoke(p, $" \n// Retrieving Shift {n + 1}\n ");
            s.Add(new Shift() {
               ShiftNumber = n + 1,
               StartHour = p.GetHRAttribute(ccSR.Shift_Start_Hour, n),
               StartMinute = p.GetHRAttribute(ccSR.Shift_Start_Minute, n),
               EndHour = endHour = p.GetHRAttribute(ccSR.Shift_End_Hour, n),
               EndMinute = endMinute = p.GetHRAttribute(ccSR.Shift_End_Minute, n),
               ShiftCode = p.GetHRAttribute(ccSR.Shift_String_Value, n),
            });
            n++;
         } while (endHour != "23" || endMinute != "59");
         return s.ToArray();
      }

      // Retrieve Time count settings
      private TimeCount RetrieveTimeCount() {
         TimeCount TimeCount = new TimeCount() {
            Interval = p.GetHRAttribute(ccSR.Update_Interval_Value),
            Start = p.GetHRAttribute(ccSR.Time_Count_Start_Value),
            End = p.GetHRAttribute(ccSR.Time_Count_End_Value),
            ResetTime = p.GetHRAttribute(ccSR.Reset_Time_Value),
            ResetValue = p.GetHRAttribute(ccSR.Time_Count_Reset_Value),
         };
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
                     string s = UTF8Hitachi.HandleBraces(item.Text);
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
                     break;
                  case logoLayout.Fixed:
                     Log?.Invoke(p, $" \n// Retrieving Fixed Logo:  Dot Matrix {neededLogo[i].dotMatrix}, Location {neededLogo[i].registration}\n ");
                     if (p.GetFixedLogo(neededLogo[i].dotMatrix, neededLogo[i].registration, out byte[] fixedData)) {
                        Logo logo = new Logo() {
                           Location = neededLogo[i].registration,
                           DotMatrix = neededLogo[i].dotMatrix,
                           RawData = p.byte_to_string(fixedData),
                           Layout = "Fixed"
                        };
                        retrievedLogos.Add(logo);
                     }
                     break;
                  default:
                     break;
               }
               if (retrievedLogos.Count > 0) {
                  Printer pr = lab.Printer[nozzle];
                  pr.Logos = retrievedLogos.ToArray();
               }
            }
         }
      }

      // Retrieve all substitutions
      public Substitution RetrieveAllSubstitutions(int ruleNo) {

         // Need to load the rule (Rule number is 1-origin)
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccIDX.Substitution_Rule, ruleNo);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         // Get all the individual rules
         List<SubstitutionRule> sr = new List<SubstitutionRule>();
         RetrieveSubstitution(sr, ccSR.Year);
         RetrieveSubstitution(sr, ccSR.Month);
         RetrieveSubstitution(sr, ccSR.Day);
         RetrieveSubstitution(sr, ccSR.Hour);
         RetrieveSubstitution(sr, ccSR.Minute);
         RetrieveSubstitution(sr, ccSR.Week);
         RetrieveSubstitution(sr, ccSR.DayOfWeek);

         // Put it all together
         return new Substitution() {
            Delimiter = "/",
            StartYear = p.GetDecAttribute(ccSR.Start_Year),
            RuleNumber = ruleNo,
            SubRule = sr.ToArray()
         };
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
                        needYear |= sub.Year != ED.Disable;
                        needMonth |= sub.Month != ED.Disable;
                        needDay |= sub.Day != ED.Disable;
                        needHour |= sub.Hour != ED.Disable;
                        needMinute |= sub.Minute != ED.Disable;
                        needDayOfWeek |= sub.DayOfWeek != ED.Disable;
                        needWeek |= sub.Week != ED.Disable;
                     }
                  }
               }
            }
         }
         // Need to load the rule (just use 1 for now)
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccIDX.Substitution_Rule, 1);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         int startYear = p.GetDecAttribute(ccSR.Start_Year);

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
            RuleNumber = 1,
            SubRule = sr.ToArray()
         };
         return substitution;
      }

      // Retrieve one substitution type
      private void RetrieveSubstitution(List<SubstitutionRule> sr, ccSR rule) {
         Log?.Invoke(p, $" \n// Retrieving substitution for {rule}\n ");
         AttrData attr = p.GetAttrData(rule);
         int n = (int)(attr.Data.Max - attr.Data.Min + 1);
         string[] subCode = new string[n];
         for (int i = 0; i < n; i++) {
            subCode[i] = p.GetHRAttribute(rule, i).Trim();
         }
         // Do not save if all were empty strings
         string s = string.Join("/", subCode);
         if (s.Length > n - 1) {
            sr.Add(new SubstitutionRule() {
               Type = rule.ToString().Replace("_", ""),
               Base = attr.Data.Min,
               Text = s,
            });
         }
      }

      #endregion

      #region Service Routines

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
