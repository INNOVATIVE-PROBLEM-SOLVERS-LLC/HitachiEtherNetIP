using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Xml;
using Serialization;

namespace Modbus_DLL {

   public partial class SendRetrieveXML {

      #region Data Declarations

      public Encoding Encode = Encoding.UTF8;

      //                          0  1  2  3   4   5   6   7   8    9  10 11 12 13   14   15   16
      int[] logoLen = new int[] { 0, 8, 8, 8, 16, 16, 32, 32, 72, 128, 32, 5, 5, 7, 200, 288, 512 };
      //                         n/a | 5x5 |  9x8  | 10x12 | 18x24 | 11x11 | 5x5C| 30x40  |  48x64
      //                          X 4x5   5x8     7x10   12x16   24x32    5x3C  7x5C    36x48
      #endregion

      #region Methods

      // Serialize the XML to a Lab and send it to the printer
      public bool SendXML(string xml) {
         if (xml.IndexOf("<Label", StringComparison.OrdinalIgnoreCase) < 0) {
            xml = File.ReadAllText(xml);
         }
         bool success = true;
         Serializer<Lab> ser = new Serializer<Lab>();
         try {
            ser.Log += Ser_Log;
            SendXML(ser.XmlToClass(xml));
         } catch (Exception e) {
            success = false;
            Log?.Invoke(p, e.Message);
         } finally {
            ser.Log -= Ser_Log;
         }
         return success;
      }

      private void Ser_Log(object sender, SerializerEventArgs e) {
         Log?.Invoke(p, e.Message);
      }

      // Send a Serialized Lab to the printer
      public void SendXML(Lab Lab) {
         try {

            XMLms = new MemoryStream();
            XMLwriter = new XmlTextWriter(XMLms, Encoding.GetEncoding("UTF-8"));
            XMLwriter.Formatting = Formatting.Indented;
            XMLwriter.WriteStartDocument();
            XMLwriter.WriteStartElement("Send"); // Start Send
            p.Log += P_Log;

            if (Lab.Printer != null) {
               for (int i = 0; i < Lab.Printer.Length; i++) {
                  Printer ptr = Lab.Printer[i];
                  int n = Math.Max(0, ptr.Nozzle - 1);
                  if (n > 0 && !p.TwinNozzle) {
                     continue;
                  }
                  p.Nozzle = n;
                  Log?.Invoke(p, $" \n// Sending Logos\n ");
                  if (Lab.Printer[i].Logos != null) {
                     XMLwriter.WriteStartElement("Logos");
                     Logo[] fixedLogos = Array.FindAll<Logo>(Lab.Printer[i].Logos, l => l.Layout == "Fixed");
                     Logo[] freeLogos = Array.FindAll<Logo>(Lab.Printer[i].Logos, l => l.Layout == "Free");
                     if (fixedLogos.Length > 0) {
                        SendFixedLogos(fixedLogos);
                     }
                     if (freeLogos.Length > 0) {
                        SendFreeLogos(freeLogos);
                     }
                     XMLwriter.WriteEndElement();
                  }
                  if (n > 0) // Load substitutions associated with nozzle 1 only
                     continue;
                  Log?.Invoke(p, $" \n// Sending Substitutions\n ");
                  XMLwriter.WriteStartElement("Substitutions");
                  SendSubstitutionRules(ptr);
                  XMLwriter.WriteEndElement(); // End Substitutions
               }
            }

            // Send message settings
            if (Lab.Message != null) {
               for (int i = 0; i < Lab.Message.Length; i++) {
                  if (Lab.Message[i] != null) {
                     int n = Math.Max(0, Lab.Message[i].Nozzle - 1);
                     if (n > 0 && !p.TwinNozzle) {
                        continue;
                     }
                     p.Nozzle = n;
                     if (p.TwinNozzle) {
                        Log?.Invoke(p, $" \n// Sending Message for nozzle {n + 1}\n ");
                     }
                     XMLwriter.WriteStartElement("Message");
                     SendMessage(Lab.Message[i]);
                     XMLwriter.WriteEndElement();
                  }
               }
            }

            // Send printer settings
            if (Lab.Printer != null) {
               for (int i = 0; i < Lab.Printer.Length; i++) {
                  if (Lab.Printer[i] != null) {
                     int n = Math.Max(0, Lab.Printer[i].Nozzle - 1);
                     if (n > 0 && !p.TwinNozzle) {
                        continue;
                     }
                     p.Nozzle = n;
                     if (p.TwinNozzle) {
                        Log?.Invoke(p, $" \n// Sending Printer Settings for nozzle {n + 1}\n ");
                     }
                     XMLwriter.WriteStartElement("PrinterSettings");
                     SendPrinterSettings(Lab.Printer[i]); // Must be done last
                     XMLwriter.WriteEndElement();
                  }
               }
            }

         } catch (Exception e2) {
            Log?.Invoke(p, e2.Message);
         } finally {

            p.Log -= P_Log;
            XMLwriter.WriteEndElement(); // End Label
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
      }

      private void SendFixedLogos(Logo[] fixedLogos) {
         List<Logo>[] bySize = new List<Logo>[logoLen.Length];                  // Break the list up by character size
         for (int i = 0; i < fixedLogos.Length; i++) {
            int n = Data.ToDropdownValue(p.GetAttrData(ccIDX.User_Pattern_Size).Data, fixedLogos[i].DotMatrix);
            if (bySize[n] == null) {
               bySize[n] = new List<Logo>();
            }
            bySize[n].Add(fixedLogos[i]);
         }
         for (int i = 0; i < logoLen.Length; i++) {                             // Process one font size at a time
            if (bySize[i] != null) {
               List<Logo> l = bySize[i].OrderBy(x => x.Location).ToList();      // Create a shorthand for the sorted list
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);             // Get the registration table loaded
               p.SetAttribute(ccIDX.User_Pattern_Size, i);

               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);             // Get the registration table loaded
               int len = (200 + 15) / 16;                                       // Number of words for 200 bits
               Section<ccUP> regs = new Section<ccUP>(p, ccUP.User_Pattern_Fixed_Registration, 0, len);
               for (int n = 0; n < l.Count; n++) {                              // Add this set of logos to the registration
                  int bit = l[n].Location;                                      // 0-origin registration of logos
                  regs.b[bit >> 3] |= (byte)(0x80 >> (int)(bit & 0x07));        // Set the bit in a byte array
               }
               regs.WriteSection();
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

               int start = 0;
               AttrData attr = p.GetAttrData(ccUP.User_Pattern_Fixed_Data).Clone();
               attr.Stride = logoLen[i] / 2;                                    // Get the distance between patterns
               for (int n = 0; n < l.Count; n++) {                              // Find sets of adjacent logos
                  if (n == (l.Count -1) || (l[n].Location + 1) != l[n + 1].Location ) { // End or a discontinuity
                     int logoCount = n - start + 1;                             // Number of consecutive logo characters
                     Section<ccUP> pattern = new Section<ccUP>(p, attr, l[start].Location, attr.Stride * logoCount, false);
                     for (int j = start; j <= n; j++) {
                        byte[] data = new byte[logoLen[i]];
                        byte[] rawdata = p.string_to_byte(l[j].RawData);
                        for (int k = 0; k < Math.Min(data.Length, rawdata.Length); k++) {
                           data[k] = rawdata[k];
                        }
                        pattern.SetUserPattern(data, l[j].Location);
                     }
                     pattern.WriteSection();                                    // Write the characters to the printer
                     start = n + 1;                                             // Start here next time
                  }
               }
            }
         }
      }

      private void SendFreeLogos(Logo[] freeLogos) {

      }

      private void P_Log(object sender, string msg) {
         if (XMLwriter != null && (msg.StartsWith("Get") || msg.StartsWith("Set"))) {
            XMLwriter.WriteElementString("IO", msg.Replace("\n", ""));
         }
      }

      #endregion

      #region Sent Message to printer

      // Send the message portion of the Lab
      private void SendMessage(Msg m) {

         // Set to only one item in printer (Deletes are done in individual mode)
         XMLwriter.WriteStartElement("DeleteOld");
         p.DeleteAllButOne();

         XMLwriter.WriteEndElement();

         if (m.Column != null) {
            Log?.Invoke(this, " \n// Loading new message\n ");
            XMLwriter.WriteStartElement("BuildNew");
            AllocateRowsColumns(m);
            XMLwriter.WriteEndElement();
         }
      }

      // Use the column/item structure of a Lab to allocate items in the printer
      private void AllocateRowsColumns(Msg m) {
         bool barCodesExist = false;                        // Has to be set after the text is loaded
         bool hasDateOrCount = false;                       // Save some time if no need to look
         List<int> sl = new List<int>(100);                 // Characters per item
         StringBuilder sb = new StringBuilder(100);         // Text of items
         string s;                                          // Always need a string for something
         int index = 0;                                     // This is Item number

         // Allocate the items
         p.SetAttribute(ccPF.Column, 1);                    // All work is done on the first column (column 1)

         // Step thru the columns right-to-left
         for (int c = m.Column.GetUpperBound(0); c >= m.Column.GetLowerBound(0); c--) {
            Column col = m.Column[c];                                 // Create a shorthand
            Log?.Invoke(p, $" \n// Set column {c + 1} to {col.Item.Length} items\n ");
            p.SetAttribute(ccPF.Line, m.Column[c].Item.Length);       // Allocate all items in column
            if (col.Item.Length > 1) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);   // Stack up the requests if 2 or more items
            }

            // Step thru the items in a column bottom-to-top
            string[] sp = new string[col.Item.Length];
            for (int r = col.Item.GetUpperBound(0); r >= col.Item.GetLowerBound(0); r--) {
               Item item = col.Item[r];                               // Shorthand for item and font
               FontDef font = item.Font;

               // Create a block write for ILS, Font, ICS, and Bolding
               Section<ccPF> sect = new Section<ccPF>(p, ccPF.Line_Spacing, ccPF.Character_Bold, r, false);
               {
                  sect.SetAttribute(ccPF.Line_Spacing, r, col.InterLineSpacing);
                  sect.SetAttribute(ccPF.Dot_Matrix, r, font.DotMatrix);
                  sect.SetAttribute(ccPF.InterCharacter_Space, r, font.InterCharacterSpace);
                  sect.SetAttribute(ccPF.Character_Bold, r, font.IncreasedWidth);
               }
               sect.WriteSection();

               // Process the text
               sp[r] = p.HandleBraces(item.Text);     // Convert string to Hitachi Attributed characters
               barCodesExist |= item.BarCode != null;
               hasDateOrCount |= item.Date != null | item.Counter != null;
            }

            // Make the display look nice (this may be an issue.)
            int maxLen = sp.Max(x => x.Length);       // Get the maximum string length
            for (int si = sp.GetUpperBound(0); si >= sp.GetLowerBound(0); si--) {
               sl.Insert(0, maxLen);                  // Insert at front since processing in reverse order
               sb.Insert(0, sp[si].PadRight(maxLen)); // Pad to max length.
            }
            if (col.Item.Length > 1) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);   // Now set them all at once if 2 or more items 
            }
            if (c > m.Column.GetLowerBound(0)) {                      // Are there more columns to come?
               p.SetAttribute(ccPF.Insert_Column, 1);                 // Allocate a new column 1
               p.SetAttribute(ccPF.Dot_Matrix, 0, "5X5");             // Make sure column is stackable to 6 rows
            }
         }

         // Now, write all the text at once
         int charPosition = 0;                              // Position in the array of attributed characters (4 bytes each)
         s = sb.ToString();                                 // Get all text items into a single string.
         Log?.Invoke(p, $" \n//Write all text at once: {sl.Count} items and {s.Length} Characters\n ");

         // Characters per item and item text must be set as a group
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1); // Start stacking requests
         Section<ccPC> cpi = new Section<ccPC>(p, ccPC.Characters_per_Item, 0, sl.Count, false);
         cpi.SetWords(sl.ToArray(), 0);                       // Set characters per item for all items
         cpi.WriteSection();

         Section<ccPC> tpi = new Section<ccPC>(p, ccPC.Print_Character_String, 0, s.Length * 2, false);
         tpi.SetAttrChrs(s, 0);                               // Set text for all items
         tpi.WriteSection();
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2); // Save lengths and text at once.

         // Is this message free layout?      <TODO> Not used now but, if needed, do later
         if (m.Layout == "FreeLayout") {
            XMLwriter.WriteStartElement("PositionItems");
            // Change message to free layout
            Log?.Invoke(p, $" \n// Change message to free layout\n ");
            XMLwriter.WriteStartElement("SetLayout");
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            p.SetAttribute(ccPF.Format_Setup, m.Layout);
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            XMLwriter.WriteEndElement();
            index = 0;
            charPosition = 0;
            for (int c = 0; c < m.Column.Length; c++) {
               for (int r = 0; r < m.Column[c].Item.Length; r++) {
                  Log?.Invoke(p, $" \n// Position item {index + 1}\n ");
                  XMLwriter.WriteStartElement("Item");
                  XMLwriter.WriteAttributeString("Index", (index + 1).ToString());
                  Item item = m.Column[c].Item[r];
                  s = p.HandleBraces(item.Text);
                  p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
                  if (item.Location != null) {
                     p.SetAttribute(ccPF.X_Coordinate, index, item.Location.X);
                     p.SetAttribute(ccPF.Y_Coordinate, index, item.Location.Y);
                  }
                  p.SetAttribute(ccPC.Characters_per_Item, index, s.Length);
                  while (s.Length > 0) {
                     int len = Math.Min(s.Length, 32);
                     p.SetAttribute(ccPC.Print_Character_String, charPosition, s.Substring(0, len));
                     s = s.Substring(len);
                     charPosition += len;
                  }
                  p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
                  charPosition += s.Length;
                  index++;
                  XMLwriter.WriteEndElement();
               }
            }
            XMLwriter.WriteEndElement();
         }
         // Process calendar and count if needed
         if (hasDateOrCount) {
            SendDateCount(m);
         }
         // At this point, barcode settings can be set
         if (barCodesExist) {
            SetBarcode(m);
         }
      }

      // Set the Barcode after conditions have been loaded
      private void SetBarcode(Msg m) {
         // Change message to free layout
         Log?.Invoke(p, $" \n// Load needed Barcode Formats\n ");
         XMLwriter.WriteStartElement("SetBarcode");
         int index = 0;
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         for (int c = 0; c < m.Column.Length; c++) {
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               Item item = m.Column[c].Item[r];
               if (item.BarCode != null) {
                  p.SetAttribute(ccPF.Barcode_Type, index, item.BarCode.DotMatrix);
               }
               index++;
            }
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         XMLwriter.WriteEndElement();
      }

      // Send the Calendar and Counter settings
      private void SendDateCount(Msg m) {
         // Get calendar and count blocks assigned by the printer
         Log?.Invoke(p, $" \n// Get number of Calendar and Count blocks used\n ");
         XMLwriter.WriteStartElement("CalCountUsage");
         for (int c = 0; c < m.Column.Length; c++) {
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               Item item = m.Column[c].Item[r];
               int index = m.Column[c].Item[r].Location.Index - 1;
               if (item.Date != null || item.Counter != null) {
                  Section<ccPF> ccs = new Section<ccPF>(p, ccPF.First_Calendar_Block, ccPF.Number_Of_Count_Blocks, index);
                  if (item.Date != null) {
                     item.Location.calCount = ccs.GetDecAttribute(ccPF.Number_of_Calendar_Blocks, index);
                     item.Location.calStart = ccs.GetDecAttribute(ccPF.First_Calendar_Block, index);
                  }
                  if (item.Counter != null) {
                     item.Location.countCount = ccs.GetDecAttribute(ccPF.Number_Of_Count_Blocks, index);
                     item.Location.countStart = ccs.GetDecAttribute(ccPF.First_Count_Block, index);
                  }
               }
            }
         }
         XMLwriter.WriteEndElement();

         for (int c = 0; c < m.Column.Length; c++) {
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               Item item = m.Column[c].Item[r];
               if (item.Date != null) {
                  XMLwriter.WriteStartElement("Calendar");
                  XMLwriter.WriteAttributeString("Block", item.Location.calStart.ToString());
                  SendCalendar(item);
                  XMLwriter.WriteEndElement();
               }
               if (item.Counter != null) {
                  XMLwriter.WriteStartElement("Count");
                  XMLwriter.WriteAttributeString("Block", item.Location.countStart.ToString());
                  SendCount(item);
                  XMLwriter.WriteEndElement();
               }
            }
         }
      }

      // Send Calendar settings
      private void SendCalendar(Item item) {
         int span;
         int calStart = item.Location.calStart;
         int calCount = item.Location.calCount;
         for (int i = 0; i < item.Date.Length; i++) {
            Date date = item.Date[i];
            if (date.Block <= calCount) {
               if (date.Offset != null || date.ZeroSuppress != null || date.Substitute != null) {
                  int index = calStart + date.Block - 2; // Cal start and date.Block are both 1-origin

                  //Log?.Invoke(p, $" \n// Load settings for Substitution rule {index + 1}\n ");
                  //p.SetAttribute(ccIDX.Substitution_Rule, index + 1); // date.SubstitutionRule

                  Log?.Invoke(p, $" \n// Set up calendar block {index + 1}\n ");

                  span = Section<ccCal>.GetSpan(p, ccCal.Offset_Year, ccCal.Zero_Suppress_DayOfWeek);
                  Section<ccCal> cs = new Section<ccCal>(p, ccCal.Offset_Year, index, span, false);
                  //cs.SetAttribute(ccCal.Substitute_Rule, index, index + 1);
                  cs.SetAttribute(ccCal.Substitute_Rule, index, 1);

                  // Process Offset
                  Offset o = date.Offset;
                  if (o != null) {
                     XMLwriter.WriteStartElement("Offset");
                     cs.SetAttribute(ccCal.Offset_Year, index, o.Year);
                     cs.SetAttribute(ccCal.Offset_Month, index, o.Month);
                     cs.SetAttribute(ccCal.Offset_Day, index, o.Day);
                     cs.SetAttribute(ccCal.Offset_Hour, index, o.Hour);
                     cs.SetAttribute(ccCal.Offset_Minute, index, o.Minute);
                     XMLwriter.WriteEndElement();
                  }

                  // Process Zero Suppress
                  ZeroSuppress zs = date.ZeroSuppress;
                  if (zs != null) {
                     XMLwriter.WriteStartElement("ZeroSuppress");
                     cs.SetAttribute(ccCal.Zero_Suppress_Year, index, zs.Year);
                     cs.SetAttribute(ccCal.Zero_Suppress_Month, index, zs.Month);
                     cs.SetAttribute(ccCal.Zero_Suppress_Day, index, zs.Day);
                     cs.SetAttribute(ccCal.Zero_Suppress_Hour, index, zs.Hour);
                     cs.SetAttribute(ccCal.Zero_Suppress_Minute, index, zs.Minute);
                     cs.SetAttribute(ccCal.Zero_Suppress_Weeks, index, zs.Week);
                     cs.SetAttribute(ccCal.Zero_Suppress_DayOfWeek, index, zs.DayOfWeek);
                     XMLwriter.WriteEndElement();
                  }
                  // Process Substitutions
                  Substitute s = date.Substitute;
                  if (s != null) {
                     XMLwriter.WriteStartElement("Substitutions");
                     cs.SetAttribute(ccCal.Substitute_Year, index, s.Year);
                     cs.SetAttribute(ccCal.Substitute_Month, index, s.Month);
                     cs.SetAttribute(ccCal.Substitute_Day, index, s.Day);
                     cs.SetAttribute(ccCal.Substitute_Hour, index, s.Hour);
                     cs.SetAttribute(ccCal.Substitute_Minute, index, s.Minute);
                     cs.SetAttribute(ccCal.Substitute_Weeks, index, s.Week);
                     cs.SetAttribute(ccCal.Substitute_DayOfWeek, index, s.DayOfWeek);
                     XMLwriter.WriteEndElement();
                  }
                  cs.WriteSection();
               }

               // Process shifts
               if (date.Shifts != null) {
                  Log?.Invoke(p, $" \n// Set up shifts\n ");
                  span = 16 * date.Shifts.Length;                               // <TODO>  Remove const 16
                  Section<ccSR> ss = new Section<ccSR>(p, ccSR.Shift_Start_Hour, 0, span, false);
                  XMLwriter.WriteStartElement("Shifts");
                  for (int j = 0; j < date.Shifts.Length; j++) {
                     Shift ds = date.Shifts[j];
                     //XMLwriter.WriteStartElement("Shift");
                     //XMLwriter.WriteAttributeString("Shift", (j + 1).ToString());
                     if (j > 0) {
                        ss.SetAttribute(ccSR.Shift_Start_Hour, j, ds.StartHour);
                        ss.SetAttribute(ccSR.Shift_Start_Minute, j, ds.StartMinute);
                     }
                     ss.SetAttribute(ccSR.Shift_End_Hour, j, ds.EndHour);
                     ss.SetAttribute(ccSR.Shift_End_Minute, j, ds.EndMinute);
                     ss.SetAttribute(ccSR.Shift_String_Value, j, ds.ShiftCode);
                     //XMLwriter.WriteEndElement();
                  }
                  ss.WriteSection();
                  XMLwriter.WriteEndElement();
               }

               // Process TimeCount
               if (date.TimeCount != null) {
                  TimeCount tc = date.TimeCount;
                  Log?.Invoke(p, $" \n// Set up Time Count\n ");
                  XMLwriter.WriteStartElement("TimeCount");
                  span = Section<ccSR>.GetSpan(p, ccSR.Time_Count_Start_Value, ccSR.Update_Interval_Value);
                  Section<ccSR> tcs = new Section<ccSR>(p, ccSR.Time_Count_Start_Value, 0, span, false);
                  tcs.SetAttribute(ccSR.Time_Count_Start_Value, tc.Start);
                  tcs.SetAttribute(ccSR.Time_Count_End_Value, tc.End);
                  tcs.SetAttribute(ccSR.Time_Count_Reset_Value, tc.ResetValue);
                  tcs.SetAttribute(ccSR.Reset_Time_Value, tc.ResetTime);
                  tcs.SetAttribute(ccSR.Update_Interval_Value, tc.Interval);

                  tcs.WriteSection();

                  XMLwriter.WriteEndElement();
               }
            }
         }
      }

      // Send count settings
      private void SendCount(Item item) {
         int countStart = item.Location.countStart;
         int countCount = item.Location.countCount;
         for (int i = 0; i < item.Counter.Length; i++) {
            Counter c = item.Counter[i];
            if (c.Block <= countCount) {
               int index = countStart + c.Block - 2; // Both count start and count block are 1-origin

               Log?.Invoke(p, $" \n// Set up count {index + 1}\n ");
               XMLwriter.WriteStartElement("Count");
               XMLwriter.WriteAttributeString("Block", (countStart + i).ToString());
               Section<ccCount> cs = new Section<ccCount>(p, ccCount.Initial_Value, ccCount.Count_Skip, index, false);
               // Process Range
               Range r = c.Range;
               if (r != null) {
                  if (r.Range1 != null)
                     cs.SetAttribute(ccCount.Count_Range_1, index, r.Range1);
                  if (r.Range2 != null)
                     cs.SetAttribute(ccCount.Count_Range_2, index, r.Range2);
                  if (r.JumpFrom != null)
                     cs.SetAttribute(ccCount.Jump_From, index, r.JumpFrom);
                  if (r.JumpTo != null)
                     cs.SetAttribute(ccCount.Jump_To, index, r.JumpTo);
               }

               // Process Count
               Count cc = c.Count;
               if (cc != null) {
                  if (cc.InitialValue != null)
                     cs.SetAttribute(ccCount.Initial_Value, index, cc.InitialValue);
                  if (cc.Increment != null)
                     cs.SetAttribute(ccCount.Increment_Value, index, cc.Increment);
                  if (cc.Direction != null)
                     cs.SetAttribute(ccCount.Direction_Value, index, cc.Direction);
                  if (cc.ZeroSuppression != null)
                     cs.SetAttribute(ccCount.Zero_Suppression, index, cc.ZeroSuppression);
               }

               // Process Reset
               Reset rr = c.Reset;
               if (rr != null) {
                  if (rr.Type != null)
                     cs.SetAttribute(ccCount.Type_Of_Reset_Signal, index, rr.Type);
                  if (rr.Value != null)
                     cs.SetAttribute(ccCount.Reset_Value, index, rr.Value);
               }

               // Process Misc
               Misc m = c.Misc;
               if (m != null) {
                  if (m.UpdateUnit != null)
                     cs.SetAttribute(ccCount.Update_Unit_Unit, index, m.UpdateUnit);
                  if (m.UpdateIP != null)
                     cs.SetAttribute(ccCount.Update_Unit_Halfway, index, m.UpdateIP);
                  if (m.ExternalCount != null)
                     cs.SetAttribute(ccCount.External_Count, index, m.ExternalCount);
                  if (m.Multiplier != null)
                     cs.SetAttribute(ccCount.Count_Multiplier, index, m.Multiplier);
                  if (m.SkipCount != null)
                     cs.SetAttribute(ccCount.Count_Skip, index, m.SkipCount);
               }
               cs.WriteSection();
               XMLwriter.WriteEndElement();
            }
         }
      }

      #endregion

      #region Send Printer Settings to printer

      private void SendPrinterSettings(Printer ptr) {
         Log?.Invoke(p, $" \n// Send printer settings\n ");

         int len = Section<ccPS>.GetSpan(p, ccPS.Character_Height, ccPS.Speed_Compensation_Fine_Control);
         Section<ccPS> pss = new Section<ccPS>(p, ccPS.Character_Height, 0, len);

         if (ptr.PrintHead != null) {
            pss.SetAttribute(ccPS.Character_Orientation, ptr.PrintHead.Orientation);
         }
         if (ptr.ContinuousPrinting != null) {
            pss.SetAttribute(ccPS.Repeat_Interval, ptr.ContinuousPrinting.RepeatInterval);
            pss.SetAttribute(ccPS.Repeat_Count, ptr.ContinuousPrinting.PrintsPerTrigger);
         }
         if (ptr.TargetSensor != null) {
            //p.SetAttribute(ccPS.Target_Sensor_Filter, ptr.TargetSensor.Filter);
            //p.SetAttribute(ccPS.Target_Sensor_Filter_Value, ptr.TargetSensor.SetupValue);
            //p.SetAttribute(ccPS.Target_Sensor_Timer, ptr.TargetSensor.Timer);
         }
         if (ptr.CharacterSize != null) {
            pss.SetAttribute(ccPS.Character_Width, ptr.CharacterSize.Width);
            pss.SetAttribute(ccPS.Character_Height, ptr.CharacterSize.Height);
         }
         if (ptr.PrintStartDelay != null) {
            pss.SetAttribute(ccPS.Print_Start_Delay_Forward, ptr.PrintStartDelay.Forward);
            pss.SetAttribute(ccPS.Print_Start_Delay_Reverse, ptr.PrintStartDelay.Reverse);
         }
         if (ptr.EncoderSettings != null) {
            pss.SetAttribute(ccPS.High_Speed_Print, ptr.EncoderSettings.HighSpeedPrinting);
            pss.SetAttribute(ccPS.Pulse_Rate_Division_Factor, ptr.EncoderSettings.Divisor);
            pss.SetAttribute(ccPS.Product_Speed_Matching, ptr.EncoderSettings.ExternalEncoder);
         }
         if (ptr.InkStream != null) {
            pss.SetAttribute(ccPS.Ink_Drop_Use, ptr.InkStream.InkDropUse);

            pss.SetAttribute(ccPS.Ink_Drop_Charge_Rule, ptr.InkStream.ChargeRule);
         }
         pss.WriteSection();
      }

      private void SendFreeLogo(Logo l) {
         if (l.Height <= 32 && l.Width <= 320 && l.RawData.Length > 0) {
            byte[] rawdata = p.string_to_byte(l.RawData);      // Get source raw data
            if (!p.SendFreeLogo(l.Width, l.Height, l.Location, rawdata)) {
               Log?.Invoke(p, $" \n// Failed to set {l.Width}x{l.Height} Free Logo to location {l.Location}\n ");
            }
         }
      }

      private void SendFixedLogo(Logo l) {
         if (l.RawData.Length > 0) {
            Log?.Invoke(p, $" \n// Set {l.DotMatrix} Fixed Logo to location {l.Location}\n ");
            // Pad the logo to full size
            int n = Data.ToDropdownValue(p.GetAttrData(ccIDX.User_Pattern_Size).Data, l.DotMatrix);
            byte[] data = new byte[logoLen[n]];
            byte[] rawdata = p.string_to_byte(l.RawData);
            for (int i = 0; i < Math.Min(data.Length, rawdata.Length); i++) {
               data[i] = rawdata[i];
            }

            if (!p.SendFixedLogo(n, l.Location, data)) {
               Log?.Invoke(p, $" \n// Failed to set {l.DotMatrix} Fixed Logo to location {l.Location}\n ");
            }
         }
      }

      // Send substitution rules
      private void SendSubstitutionRules(Printer ptr) {
         if (ptr.Substitution != null && ptr.Substitution.SubRule != null) {
            if (ptr.Substitution.Delimiter.Length == 1) {
               SendSubstitution(ptr.Substitution);
            }
         }
      }

      public void SendSubstitution(Substitution s) {
         string delimiter = s.Delimiter;
         // Need to load the rule (Rule number is 1-origin)
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccIDX.Substitution_Rule, s.RuleNumber);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         p.SetAttribute(ccSR.Start_Year, s.StartYear);
         for (int i = 0; i < s.SubRule.Length; i++) {
            SubstitutionRule r = s.SubRule[i];
            if (Enum.TryParse(r.Type, true, out ccSR type)) {
               if (XMLwriter != null) {
                  XMLwriter.WriteStartElement(r.Type);
               }
               SetSubValues(type, r, delimiter);
               if (XMLwriter != null) {
                  XMLwriter.WriteEndElement();
               }
            } else {
               Log?.Invoke(p, $"Unknown substitution rule type =>{r.Type}<=");
            }
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
      }

      private void SetSubValues(ccSR attribute, SubstitutionRule r, string delimiter) {
         int n;
         string s2;
         Prop prop = Data.AttrDict[ClassCode.Substitution_rules, (int)attribute].Data;
         string t = new string(' ', prop.Len);
         string[] s = r.Text.Split(delimiter[0]);
         if (s.Length <= (prop.Max - prop.Min + 1)) {
            n = r.Base - prop.Min;
            s2 = string.Empty;
            for (int i = 0; i < s.Length; i++) {
               string t2 = new string(' ', prop.Len) + s[i];
               s2 += t2.Substring(t2.Length - prop.Len);
               if (s2.Length > 120 || i == s.Length - 1) {
                  p.SetAttribute(attribute, n, s2);
                  s2 = string.Empty;
                  n = r.Base + i - prop.Min + 1; // +1 is where i will be next time
               }
            }
         } else {
            for (int i = 0; i < s.Length; i++) {
               n = r.Base + i;
               // Avoid user errors
               if (n >= prop.Min && n <= prop.Max) {
                  string t2 = new string(' ', prop.Len) + s[i];
                  p.SetAttribute(attribute, n - prop.Min, t2.Substring(t2.Length - prop.Len));
               }
            }
         }
      }

      #endregion

   }
}
