using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Xml;
using Serialization;
using UTF8vsHitachiCodes;

namespace Modbus_DLL {

   public partial class SendRetrieveXML {

      #region Data Declarations

      public Encoding Encode = Encoding.UTF8;

      #endregion

      #region Methods

      // Serialize the XML to a Lab and send it to the printer
      public bool SendXML(string xml) {
         if (xml.IndexOf("<Label", StringComparison.OrdinalIgnoreCase) < 0) { // XML direct or a File Name?
            xml = File.ReadAllText(xml);                                      // Read the XML from the file
         }
         bool success = true;
         Serializer<Lab> ser = new Serializer<Lab>();       // Serializer for converting XML to class structure
         try {
            ser.Log += Ser_Log;                             // Logging routine in case of XML Errors
            SendXML(ser.XmlToClass(xml));                   // Do the conversion and pass it on for sending to printer
         } catch (Exception e) {
            success = false;
            Log?.Invoke(p, e.Message);                      // Most likely XML Syntax errors
         } finally {
            ser.Log -= Ser_Log;                             // Release the logging event
         }
         return success;
      }

      // Logger for XML unknown attributes issues
      private void Ser_Log(object sender, SerializerEventArgs e) {
         Log?.Invoke(p, e.Message);                         // Just pass it on
      }

      // Send a Serialized Lab to the printer
      public void SendXML(Lab Lab) {
         try {
            if (Lab.Printer != null) {
               for (int i = 0; i < Lab.Printer.Length; i++) { // Multiple printers imply a Twin Nozzle printer
                  Printer ptr = Lab.Printer[i];
                  int n = Math.Max(0, ptr.Nozzle - 1);
                  if (n > 0 && !p.TwinNozzle)                // Only process additional printers if Twin Nozzle
                     continue;
                  p.Nozzle = n;
                  Log?.Invoke(p, $" \n// Sending Logos\n ");
                  if (Lab.Printer[i].Logos != null) {         // If logos exist, divide them into two groups
                     Logo[] fixedLogos = Array.FindAll<Logo>(Lab.Printer[i].Logos, l => l.Layout == "Fixed");
                     Logo[] freeLogos = Array.FindAll<Logo>(Lab.Printer[i].Logos, l => l.Layout == "Free");
                     if (fixedLogos.Length > 0) {             // Process fixed logos if any exist
                        Log?.Invoke(p, $" \n// Sending Fixed Logos\n ");
                        SendFixedLogos(fixedLogos);
                     }
                     if (freeLogos.Length > 0) {              // Process free logos if any exist
                        Log?.Invoke(p, $" \n// Sending Free Logos\n ");
                        SendFreeLogos(freeLogos);
                     }
                  }
                  if (n > 0)                                  // Substitutions are printer wide, not nozzle specific
                     continue;
                  if (ptr.Substitution != null
                     && ptr.Substitution.SubRule != null
                     && ptr.Substitution.Delimiter.Length == 1) {
                     Log?.Invoke(p, $" \n// Sending Substitutions\n ");
                     SendSubstitution(ptr.Substitution);
                  }
               }
            }
            
            if (Lab.Message != null) {                      // Send message settings
               for (int i = 0; i < Lab.Message.Length; i++) {
                  if (Lab.Message[i] != null) {
                     int n = Math.Max(0, Lab.Message[i].Nozzle - 1);
                     if (n > 0 && !p.TwinNozzle) {          // Multiple messages are for twin nozzle
                        continue;
                     }
                     p.Nozzle = n;
                     if (p.TwinNozzle) {
                        Log?.Invoke(p, $" \n// Sending Message for nozzle {n + 1}\n ");
                     }
                     SendMessage(Lab.Message[i]);
                  }
               }
            }

            if (Lab.Printer != null) {                        // Send printer settings
               for (int i = 0; i < Lab.Printer.Length; i++) {
                  if (Lab.Printer[i] != null) {
                     int n = Math.Max(0, Lab.Printer[i].Nozzle - 1);
                     if (n > 0 && !p.TwinNozzle) {            // Multiple settings are for twin nozzle
                        continue;
                     }
                     p.Nozzle = n;
                     if (p.TwinNozzle) {
                        Log?.Invoke(p, $" \n// Sending Printer Settings for nozzle {n + 1}\n ");
                     }
                     SendPrinterSettings(Lab.Printer[i]); // Must be done last
                  }
               }
            }

         } catch (Exception e2) {
            Log?.Invoke(p, e2.Message);
         }
      }

      // Group fixed logos by size, sort by location, and send by groups with consecutive locations
      private void SendFixedLogos(Logo[] fixedLogos) {
         AttrData attr;
         List<Logo>[] bySize = new List<Logo>[Modbus.logoLen.Length];        // Break the list up by character size
         for (int i = 0; i < fixedLogos.Length; i++) {                       // Group logos by size
            int n = Data.ToDropdownValue(p.GetAttrData(ccIDX.User_Pattern_Size).Data, fixedLogos[i].DotMatrix);
            if (bySize[n] == null) {
               bySize[n] = new List<Logo>();
            }
            bySize[n].Add(fixedLogos[i]);
         }
         for (int i = 0; i < Modbus.logoLen.Length; i++) {                   // Process one font size at a time
            if (bySize[i] == null)                                           // skip if no logos for this size
               continue;
            List<Logo> l = bySize[i].OrderBy(x => x.Location).ToList();      // Create a shorthand for the sorted list
            p.SetAttribute(ccIDX.User_Pattern_Size, i);                      // Load table by font size
            attr = p.GetAttrData(ccUP.User_Pattern_Fixed_Registration);

            Section<ccUP> regs = new Section<ccUP>(p, ccUP.User_Pattern_Fixed_Registration, 0, attr.Count, true);
            { // Read to read the old data since this may be add to the existing user patterns
               for (int j = 0; j < l.Count; j++) {
                  p.SetBit(l[j].Location, regs.b);                           // Location is the bit position in the registry
               }
               regs.WriteSection();                                          // Rewrite the registration
            }

            int start = 0;
            int len = Modbus.logoLen[i];
            attr = p.GetAttrData(ccUP.User_Pattern_Fixed_Data).Clone();      // Need a clone to handle variable length by font
            attr.Stride = Modbus.logoLen[i] / 2;                             // Get the distance between patterns
            for (int n = 0; n < l.Count; n++) {                              // Find sets of adjacent logos
               if (n == (l.Count - 1) || (l[n].Location + 1) != l[n + 1].Location) { // End of list or location discontinuity
                  int logoCount = n - start + 1;                             // Number of consecutive logo characters
                  Section<ccUP> pattern = new Section<ccUP>(p, attr, l[start].Location, attr.Stride * logoCount, false);
                  for (int j = start; j <= n; j++) {                         // Step thru list of adjacent logos
                     byte[] rawdata = p.string_to_byte(l[j].RawData);        // Comes in as Hex Characters
                     pattern.SetUserPattern(rawdata, l[j].Location, Math.Min(len, rawdata.Length));
                  }
                  pattern.WriteSection(true);                                // Write the characters to the printer
                  start = n + 1;                                             // Start here next time
               }
            }
         }
      }

      // Free logos are all the same size.  Order by location just for readability
      private void SendFreeLogos(Logo[] freeLogos) {
         AttrData attr = p.GetAttrData(ccUP.User_Pattern_Free_Registration);
         List<Logo> l = freeLogos.OrderBy(x => x.Location).ToList();      // Create a shorthand for the sorted list
         int[] width = new int[l.Count];
         Section<ccUP> regs = new Section<ccUP>(p, ccUP.User_Pattern_Free_Registration, 0, attr.Count, true);
         {
            for (int i = 0; i < l.Count; i++) {                       // Step thru the logos
               Logo logo = l[i];
               int loc = logo.Location;                               // The location is the bit in the registry
               if (p.CheckBit(loc, regs.b)) {                         // If it was it already in use, must overwrite old
                  int oldWidth = p.GetDecAttribute(ccUP.User_Pattern_Free_Width, loc);
                  width[i] = Math.Max(logo.Width, oldWidth);          // Use size of widest logo
               } else {
                  width[i] = logo.Width;                              // Use size of new logo
               }
               p.SetBit(loc, regs.b);                                 // Set used bit in registry
               byte[] rawdata = p.string_to_byte(logo.RawData);       // Comes in as Hex Characters
               Section<ccUP> bitmaps = new Section<ccUP>(p, ccUP.User_Pattern_Free_Height, logo.Location, (width[i] + 1) * 2, false);
               { // Do not pre-read the data.  Old data will be overwritten if it is larger than the new data
                  int n = (logo.Height + 7) / 8;                      // Calculate source height in bytes per stripe
                  int newLength = width[i] * 4;                       // Widht is in stripes
                  byte[] data = new byte[newLength];                  // Free logos are always 4 bytes per stripe
                  int k = 0;
                  for (int i2 = 0; i2 < newLength && k < rawdata.Length; i2 += 4) {  // Pad the data to 4 bytes per stripe
                     for (int j = 0; j < n && k < rawdata.Length; j++) {
                        data[i2 + j] = rawdata[k++];
                     }
                  }
                  AttrData dataAttr = p.GetAttrData(ccUP.User_Pattern_Free_Data).Clone();
                  dataAttr.Data.Len = width[i] * 2;                   // Need to set the width since it is variable length
                  bitmaps.SetAttribute(ccUP.User_Pattern_Free_Height, logo.Location, logo.Height);
                  bitmaps.SetAttribute(ccUP.User_Pattern_Free_Width, logo.Location, logo.Width);
                  bitmaps.SetAttribute(dataAttr, logo.Location, data);
                  bitmaps.WriteSection(true);                         // Need to stack if multiple writes
               }
            }
            regs.WriteSection();                                      // Rewrite the registration
         }
      }

      #endregion

      #region Sent Message to printer

      // Send the message portion of the Lab (In forward order)
      private void SendMessage(Msg m) {
         bool use_left_to_right = false; // Right to left is fastest
         p.DeleteAllButOne();            // Leave to only one item in printer (Deletes are done in individual mode)
         if (m.Column != null) {
            if (use_left_to_right) {
               Log?.Invoke(this, " \n// Loading new message from left to right\n ");
               AllocateRowsColumnsII(m);
            } else {
               Log?.Invoke(this, " \n// Loading new message from right to left\n ");
               AllocateRowsColumns(m);
            }
         }
      }

      // Allocate items in a left to right fashion
      private void AllocateRowsColumnsII(Msg m) {
         bool barCodesExist = false;                        // Has to be set after the text is loaded
         bool hasDateOrCount = false;                       // Save some time if no need to look
         List<int> sl = new List<int>(100);                 // String Length in Characters per item
         StringBuilder sb = new StringBuilder(100);         // Text of items
         int n = 0;                                         // Active item number
         // Step thru the columns right-to-left
         for (int c = 0; c < m.Column.Length; c++) {
            Column col = m.Column[c];                                 // Create a shorthand
            Log?.Invoke(p, $" \n// Set column {c + 1} to {col.Item.Length} items\n ");
            if (c > 0) {
               p.SetAttribute(ccPF.Add_Column, c + 1);
            }
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);      // These two must be done together
            p.SetAttribute(ccPF.Column, c + 1);                       // Point to the allocated column
            p.SetAttribute(ccPF.Line, m.Column[c].Item.Length);       // Allocate all items in column
            p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);      // Now set them all at once if 2 or more items

            if (col.Item.Length > 1) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);   // Stack up the requests if 2 or more items
            }

            string[] sp = new string[col.Item.Length];                // Want to make all items in a column the same length
            for (int r = 0; r < col.Item.Length; r++) {
               Item item = col.Item[r];                               // Shorthand for item and font
               FontDef font = item.Font;

               // Create a block write for Font, ICS, Bolding, Barcode
               Section<ccPF> sect = new Section<ccPF>(p, ccPF.Line_Spacing, ccPF.Barcode_Type, n + r, false);
               {
                  if (col.Item.Length == 1) {
                     sect.SetAttribute(ccPF.Line_Spacing, n, 0);      // Failure to set the 0 could be fatal
                  } else {
                     sect.SetAttribute(ccPF.Line_Spacing, n + r, col.InterLineSpacing);
                  }
                  sect.SetAttribute(ccPF.Dot_Matrix, n + r, font.DotMatrix);
                  sect.SetAttribute(ccPF.InterCharacter_Space, n + r, font.InterCharacterSpace);
                  sect.SetAttribute(ccPF.Character_Bold, n + r, font.IncreasedWidth);
                  sect.SetAttribute(ccPF.Barcode_Type, n + r, 0); //  (set to 0 now, cannot be set until text is loaded)
                  sect.WriteSection();
               }

               sp[r] = UTF8Hitachi.HandleBraces(item.Text);     // ?? Convert string to Hitachi Attributed characters
               barCodesExist |= item.BarCode != null;
               hasDateOrCount |= item.Date != null | item.Counter != null;
            }
            if (col.Item.Length > 1) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);      // Process the whole column at once
            }

            // Make the display look nice (this may be an issue.)
            int maxLen = sp.Max(x => x.Length);       // Get the maximum string length
            for (int si = 0; si < sp.Length; si++) {
               sl.Add(maxLen);                        // Insert at end since processing in forward order
               sb.Append(sp[si].PadRight(maxLen));    // Pad to max length.
            }
            n += col.Item.Length;
         }

         // Now, write all the text at once
         string s = sb.ToString();                    // Get all text items into a single string.
         Log?.Invoke(p, $" \n//Write all text at once: {sl.Count} items and {s.Length} Characters\n ");

         // Characters per item and text per item must be set as a group
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1); // Start stacking requests
         {
            Section<ccPC> cpi = new Section<ccPC>(p, ccPC.Characters_per_Item, 0, sl.Count, false);
            cpi.SetWords(sl.ToArray(), 0);                    // Set characters per item for all items
            cpi.WriteSection();

            Section<ccPC> tpi = new Section<ccPC>(p, ccPC.Print_Character_String, 0, s.Length * 2, false);
            tpi.SetAttrChrs(s, 0);                            // Set text for all items
            tpi.WriteSection();
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2); // Save lengths and text at once.

         // Set the item x/y coordinates if this message is free layout?
         if (m.Layout == "FreeLayout") {
            // Change message to free layout
            Log?.Invoke(p, $" \n// Change message to free layout\n ");
            p.SetAttribute(ccPF.Format_Setup, m.Layout);
            int index = 0;                                  // This is Item number
            for (int c = 0; c < m.Column.Length; c++) {     // Can step thru forward or backward.
               for (int r = 0; r < m.Column[c].Item.Length; r++) {
                  Log?.Invoke(p, $" \n// Position item {index + 1}\n ");
                  Item item = m.Column[c].Item[r];
                  if (item.Location != null) {
                     Section<ccPF> xy = new Section<ccPF>(p, ccPF.X_Coordinate, ccPF.Y_Coordinate, index, false);
                     {
                        xy.SetAttribute(ccPF.X_Coordinate, index, item.Location.X);
                        xy.SetAttribute(ccPF.Y_Coordinate, index, item.Location.Y);
                        xy.WriteSection();
                     }
                  }
                  index++;
               }
            }
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

      // Use the column/item structure of a Lab to allocate items in the printer (From right to left)
      private void AllocateRowsColumns(Msg m) {
         bool barCodesExist = false;                        // Has to be set after the text is loaded
         bool hasDateOrCount = false;                       // Save some time if no need to look
         List<int> sl = new List<int>(100);                 // Characters per item
         StringBuilder sb = new StringBuilder(100);         // Text of items

         // Step thru the columns right-to-left
         p.SetAttribute(ccPF.Column, 1);                              // All work is done on the first column (column 1)
         for (int c = m.Column.GetUpperBound(0); c >= m.Column.GetLowerBound(0); c--) {
            Column col = m.Column[c];                                 // Create a shorthand
            Log?.Invoke(p, $" \n// Set column {c + 1} to {col.Item.Length} items\n ");
            p.SetAttribute(ccPF.Line, m.Column[c].Item.Length);       // Allocate all items in column
            if (col.Item.Length > 1) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);   // Stack up the requests if 2 or more items
            }

            // Step thru the items in a column bottom-to-top
            string[] sp = new string[col.Item.Length];                // Want to make all items in a column the same length
            for (int r = col.Item.GetUpperBound(0); r >= col.Item.GetLowerBound(0); r--) {
               Item item = col.Item[r];                               // Shorthand for item and font
               FontDef font = item.Font;

               // Create a block write for ILS, Font, ICS, Bolding, and Barcode
               Section<ccPF> sect = new Section<ccPF>(p, ccPF.Line_Spacing, ccPF.Barcode_Type, r, false);
               {
                  if (col.Item.Length == 1) {
                     sect.SetAttribute(ccPF.Line_Spacing, r, 0);      // This has to be set
                  } else {
                     sect.SetAttribute(ccPF.Line_Spacing, r, col.InterLineSpacing);
                  }
                  sect.SetAttribute(ccPF.Dot_Matrix, r, font.DotMatrix);
                  sect.SetAttribute(ccPF.InterCharacter_Space, r, font.InterCharacterSpace);
                  sect.SetAttribute(ccPF.Character_Bold, r, font.IncreasedWidth);
                  sect.SetAttribute(ccPF.Barcode_Type, r, 0);         // Cannot set barcode until text is written
                  sect.WriteSection();
               }

               sp[r] = UTF8Hitachi.HandleBraces(item.Text);           // Convert string to Hitachi Attributed characters
               barCodesExist |= item.BarCode != null;
               hasDateOrCount |= item.Date != null | item.Counter != null;
            }
            // 
            if (col.Item.Length > 1) {
               p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);   // Now set them all at once if 2 or more items 
            }
            if (c > m.Column.GetLowerBound(0)) {                      // Are there more columns to come?
               p.SetAttribute(ccPF.Insert_Column, 1);                 // Allocate a new column 1
               p.SetAttribute(ccPF.Dot_Matrix, 0, "5X5");             // Make sure column is stackable to 6 rows
            }
            // Make the display look nice (this may be an issue.)
            int maxLen = sp.Max(x => x.Length);       // Get the maximum string length
            for (int si = sp.GetUpperBound(0); si >= sp.GetLowerBound(0); si--) {
               sl.Insert(0, maxLen);                  // Insert at front since processing in reverse order
               sb.Insert(0, sp[si].PadRight(maxLen)); // Pad to max length.
            }
         }

         // Now, write all the text at once
         string s = sb.ToString();                                    // Get all text items into a single string.
         Log?.Invoke(p, $" \n//Write all text at once: {sl.Count} items and {s.Length} Characters\n ");

         // Characters per item and text per item must be set as a group
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);         // Start stacking requests
         {
            Section<ccPC> cpi = new Section<ccPC>(p, ccPC.Characters_per_Item, 0, sl.Count, false);
            cpi.SetWords(sl.ToArray(), 0);                            // Set characters per item for all items
            cpi.WriteSection();

            Section<ccPC> tpi = new Section<ccPC>(p, ccPC.Print_Character_String, 0, s.Length * 2, false);
            tpi.SetAttrChrs(s, 0);                                    // Set text for all items
            tpi.WriteSection();
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2); // Save lengths and text at once.

         // Set the item x/y coordinates if this message is free layout?
         if (m.Layout == "FreeLayout") {
            // Change message to free layout
            Log?.Invoke(p, $" \n// Change message to free layout\n ");
            p.SetAttribute(ccPF.Format_Setup, m.Layout);
            int index = 0;                                            // This is Item number
            for (int c = 0; c < m.Column.Length; c++) {               // Can step thru forward or backward.
               for (int r = 0; r < m.Column[c].Item.Length; r++) {
                  Log?.Invoke(p, $" \n// Position item {index + 1}\n ");
                  Item item = m.Column[c].Item[r];
                  if (item.Location != null) {
                     Section<ccPF> xy = new Section<ccPF>(p, ccPF.X_Coordinate, ccPF.Y_Coordinate, index, false);
                     { // No need to pre-read.  This just avoids laving to stack two I/Os
                        xy.SetAttribute(ccPF.X_Coordinate, index, item.Location.X);
                        xy.SetAttribute(ccPF.Y_Coordinate, index, item.Location.Y);
                        xy.WriteSection();
                     }
                  }
                  index++;
               }
            }
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
      }

      // Send the Calendar and Counter settings
      private void SendDateCount(Msg m) {
         // Get calendar and count blocks assigned by the printer
         Log?.Invoke(p, $" \n// Get number of Calendar and Count blocks used\n ");
         for (int c = 0; c < m.Column.Length; c++) {
            for (int r = 0; r < m.Column[c].Item.Length; r++) {
               Item item = m.Column[c].Item[r];
               int index = m.Column[c].Item[r].Location.Index - 1;
               if (item.Date != null || item.Counter != null) {
                  Section<ccPF> ccs = new Section<ccPF>(p, ccPF.First_Calendar_Block, ccPF.Number_Of_Count_Blocks, index, true);
                  { // Avoids 4 I/O to get the data
                     item.Location.calStart = ccs.GetDecAttribute(ccPF.First_Calendar_Block, index);
                     item.Location.calCount = ccs.GetDecAttribute(ccPF.Number_of_Calendar_Blocks, index);
                     item.Location.countStart = ccs.GetDecAttribute(ccPF.First_Count_Block, index);
                     item.Location.countCount = ccs.GetDecAttribute(ccPF.Number_Of_Count_Blocks, index);
                     ccs = null;
                  }
                  if (item.Date != null) {
                     SendCalendar(item);          // Send calendar if needed
                  }
                  if (item.Counter != null) {
                     SendCount(item);             // Send counter if needed.
                  }
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
                  Log?.Invoke(p, $" \n// Set up calendar block {index + 1}\n ");

                  Section<ccCal> cs = new Section<ccCal>(p, ccCal.Offset_Year, ccCal.Zero_Suppress_DayOfWeek, index, false);
                  { // No pre-read since all will be filled in
                     cs.SetAttribute(ccCal.Substitute_Rule, index, 1); // Only substitution rule 1?

                     Offset o = date.Offset;                   // Process Offset
                     if (o != null) {
                        cs.SetAttribute(ccCal.Offset_Year, index, o.Year);
                        cs.SetAttribute(ccCal.Offset_Month, index, o.Month);
                        cs.SetAttribute(ccCal.Offset_Day, index, o.Day);
                        cs.SetAttribute(ccCal.Offset_Hour, index, o.Hour);
                        cs.SetAttribute(ccCal.Offset_Minute, index, o.Minute);
                     }

                     ZeroSuppress zs = date.ZeroSuppress;      // Process Zero Suppress
                     if (zs != null) {
                        cs.SetAttribute(ccCal.Zero_Suppress_Year, index, zs.Year);
                        cs.SetAttribute(ccCal.Zero_Suppress_Month, index, zs.Month);
                        cs.SetAttribute(ccCal.Zero_Suppress_Day, index, zs.Day);
                        cs.SetAttribute(ccCal.Zero_Suppress_Hour, index, zs.Hour);
                        cs.SetAttribute(ccCal.Zero_Suppress_Minute, index, zs.Minute);
                        cs.SetAttribute(ccCal.Zero_Suppress_Weeks, index, zs.Week);
                        cs.SetAttribute(ccCal.Zero_Suppress_DayOfWeek, index, zs.DayOfWeek);
                     }

                     Substitute s = date.Substitute;           // Process Substitutions
                     if (s != null) {
                        cs.SetAttribute(ccCal.Substitute_Year, index, s.Year);
                        cs.SetAttribute(ccCal.Substitute_Month, index, s.Month);
                        cs.SetAttribute(ccCal.Substitute_Day, index, s.Day);
                        cs.SetAttribute(ccCal.Substitute_Hour, index, s.Hour);
                        cs.SetAttribute(ccCal.Substitute_Minute, index, s.Minute);
                        cs.SetAttribute(ccCal.Substitute_Weeks, index, s.Week);
                        cs.SetAttribute(ccCal.Substitute_DayOfWeek, index, s.DayOfWeek);
                     }
                  }
                  cs.WriteSection();
               }

               if (date.Shifts != null) {                   // Process shifts
                  Log?.Invoke(p, $" \n// Set up shifts\n ");
                  AttrData attr = p.GetAttrData(ccSR.Shift_Start_Hour);
                  span = attr.Data.Len * date.Shifts.Length;
                  Section<ccSR> ss = new Section<ccSR>(p, ccSR.Shift_Start_Hour, 0, span, false);
                  {
                     for (int j = 0; j < date.Shifts.Length; j++) {
                        Shift ds = date.Shifts[j];
                        ss.SetAttribute(ccSR.Shift_Start_Hour, j, ds.StartHour);
                        ss.SetAttribute(ccSR.Shift_Start_Minute, j, ds.StartMinute);
                        ss.SetAttribute(ccSR.Shift_End_Hour, j, ds.EndHour);
                        ss.SetAttribute(ccSR.Shift_End_Minute, j, ds.EndMinute);
                        ss.SetAttribute(ccSR.Shift_String_Value, j, ds.ShiftCode);
                        ss.WriteSection();
                     }
                  }
               }


               if (date.TimeCount != null) {                // Process TimeCount
                  TimeCount tc = date.TimeCount;
                  Log?.Invoke(p, $" \n// Set up Time Count\n ");
                  Section<ccSR> tcs = new Section<ccSR>(p, ccSR.Time_Count_Start_Value, ccSR.Update_Interval_Value, 0, false);
                  {
                     tcs.SetAttribute(ccSR.Time_Count_Start_Value, tc.Start);
                     tcs.SetAttribute(ccSR.Time_Count_End_Value, tc.End);
                     tcs.SetAttribute(ccSR.Time_Count_Reset_Value, tc.ResetValue);
                     tcs.SetAttribute(ccSR.Reset_Time_Value, tc.ResetTime);
                     tcs.SetAttribute(ccSR.Update_Interval_Value, tc.Interval);
                     tcs.WriteSection();
                  }
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
               Section<ccCount> cs = new Section<ccCount>(p, ccCount.Initial_Value, ccCount.Count_Skip, index, false);
               {
                  Range r = c.Range;                        // Process Range
                  if (r != null) {
                     cs.SetAttribute(ccCount.Count_Range_1, index, r.Range1);
                     cs.SetAttribute(ccCount.Count_Range_2, index, r.Range2);
                     cs.SetAttribute(ccCount.Jump_From, index, r.JumpFrom);
                     cs.SetAttribute(ccCount.Jump_To, index, r.JumpTo);
                  }

                  Count cc = c.Count;                       // Process Count
                  if (cc != null) {
                     cs.SetAttribute(ccCount.Initial_Value, index, cc.InitialValue);
                     cs.SetAttribute(ccCount.Increment_Value, index, cc.Increment);
                     cs.SetAttribute(ccCount.Direction_Value, index, cc.Direction);
                     cs.SetAttribute(ccCount.Zero_Suppression, index, cc.ZeroSuppression);
                  }

                  Reset rr = c.Reset;                       // Process Reset
                  if (rr != null) {
                     cs.SetAttribute(ccCount.Type_Of_Reset_Signal, index, rr.Type);
                     cs.SetAttribute(ccCount.Reset_Value, index, rr.Value);
                  }

                  Misc m = c.Misc;                          // Process Misc
                  if (m != null) {
                     cs.SetAttribute(ccCount.Update_Unit_Unit, index, m.UpdateUnit);
                     cs.SetAttribute(ccCount.Update_Unit_Halfway, index, m.UpdateIP);
                     cs.SetAttribute(ccCount.External_Count, index, m.ExternalCount);
                     cs.SetAttribute(ccCount.Count_Multiplier, index, m.Multiplier);
                     cs.SetAttribute(ccCount.Count_Skip, index, m.SkipCount);
                  }
                  cs.WriteSection();
               }
            }
         }
      }

      #endregion

      #region Send Printer Settings to printer

      // Load printer wide settings
      private void SendPrinterSettings(Printer ptr) {
         Log?.Invoke(p, $" \n// Send printer settings\n ");
         Section<ccPS> pss = new Section<ccPS>(p, ccPS.Character_Height, ccPS.Speed_Compensation_Fine_Control, 0, true);
         {  // This section must be read as not all values are guaranteed to be set.
            if (ptr.PrintHead != null) {
               pss.SetAttribute(ccPS.Character_Orientation, ptr.PrintHead.Orientation);
            }
            if (ptr.ContinuousPrinting != null) {
               pss.SetAttribute(ccPS.Repeat_Interval, ptr.ContinuousPrinting.RepeatInterval);
               pss.SetAttribute(ccPS.Repeat_Count, ptr.ContinuousPrinting.PrintsPerTrigger);
            }
            if (ptr.TargetSensor != null) { // Causes a fault in the printer.
               //pss.SetAttribute(ccPS.Target_Sensor_Filter, ptr.TargetSensor.Filter);
               //pss.SetAttribute(ccPS.Target_Sensor_Filter_Value, ptr.TargetSensor.SetupValue);
               //pss.SetAttribute(ccPS.Target_Sensor_Timer, ptr.TargetSensor.Timer);
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
         }
         pss.WriteSection();
      }

      // Send the substitution header
      public void SendSubstitution(Substitution s) {
         string delimiter = s.Delimiter;
         // Need to load the rule (Rule number is 1-origin)
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);         // Not sure that this is needed
         p.SetAttribute(ccIDX.Substitution_Rule, s.RuleNumber);       // Load the needed substitution rule into memory
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);         // Again, unsure of the need for this
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);         // Stack up all substitutions 
         p.SetAttribute(ccSR.Start_Year, s.StartYear);                // Set the base year
         for (int i = 0; i < s.SubRule.Length; i++) {                 // Step thru all rules in the packet
            SubstitutionRule r = s.SubRule[i];                        // Create a shorthand
            if (Enum.TryParse(r.Type, true, out ccSR type)) {         // Valid type?
               SetSubValues(type, r, delimiter);                      // Send the substitution values
            } else {
               Log?.Invoke(p, $"Unknown substitution rule type =>{r.Type}<=");
            }
         }
         p.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);         // Now process the stacked requests
      }

      // Set the values associated with a rule
      private void SetSubValues(ccSR attribute, SubstitutionRule r, string delimiter) {
         AttrData attr = p.GetAttrData(attribute);
         Prop prop = attr.Data;
         string[] s = r.Text.Split(delimiter[0]);
         int n = Math.Min(attr.Count, s.Length);
         Section<ccSR> sub = new Section<ccSR>(p, attribute, 0, prop.Len * n, false);
         { // Avoid many I/Os
            for (int i = 0; i < n; i++) {
               sub.SetAttribute(attribute, i, s[i]);
            }
            sub.WriteSection();
         }
      }

      #endregion

   }
}
