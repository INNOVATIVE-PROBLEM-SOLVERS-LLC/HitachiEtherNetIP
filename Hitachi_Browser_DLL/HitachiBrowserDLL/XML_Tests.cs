using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EIP_Lib {

   public partial class XML {

      #region Test Driver

      // Run hard coded test
      private void cmdRunHardTest_Click(object sender, EventArgs e) {
         int Item = 1;
         int Rule = 1;
         if (EIP.StartSession(true)) {
            if (EIP.ForwardOpen()) {
               try {
                  // Clean up the display
                  CleanUpDisplay();
                  // Run selected test
                  switch (cbAvailableHardTests.SelectedIndex) {
                     case 0:
                        // Gets us down to a single item
                        break;
                     case 1:
                        BuildShifts(Item++);
                        break;
                     case 2:
                        BuildMonthDaySR(Rule);
                        break;
                     case 3:
                        BuildTimeCount(Item++);
                        break;
                     case 4:
                        TryDayOfWeekEtc(Item++);
                        break;
                     case 5:
                        BuildMDYhms(Item++, Rule);
                        break;
                     case 6:
                        MultiLine();
                        break;
                     case 7:
                        CreateCounter();
                        break;
                     case 8:
                        Comprehensive();
                        break;
                     case 9:
                        SetText("{{MMM}/{DD}/{YY}}\n {{hh}:{mm}:{ss}}");
                        break;
                  }
                  //VerifyShifts(Item++);
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
               } catch (Exception e2) {
                  // You are on your own here
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      #endregion

      #region Test Routines

      public bool CleanUpDisplay() {
         bool success = true;
         if (EIP.StartSession(true)) {
            if (EIP.ForwardOpen()) {
               try {
                  // Get the number of columns
                  EIP.GetAttribute(ccPF.Number_Of_Columns, out int cols);
                  // No need to delete columns if there is only one
                  if (cols > 1) {
                     // Select to continuously delete column 2 (0 origin on deletes)
                     EIP.SetAttribute(ccIDX.Column, 1);
                     // Column number is 0 origin
                     while (--cols > 0) {
                        // Delete the column
                        EIP.ServiceAttribute(ccPF.Delete_Column);
                     }
                  }
                  // Select item 1 (1 origin on Line Count)
                  EIP.SetAttribute(ccIDX.Item, 1);
                  // Set line count to 1. (Need to find out how delete single item works.)
                  EIP.SetAttribute(ccPF.Line_Count, 1);
                  // Test item size
                  EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
                  EIP.SetAttribute(ccPF.Barcode_Type, "None");
                  // Set simple text in case Calendar or Counter was used
                  EIP.SetAttribute(ccPF.Print_Character_String, "1");
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
                  success = false;
               } catch (Exception e2) {
                  // You are on your own here
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      private bool BuildShifts(int Item) {
         // Add the item if needed and select it
         if (Item != 1) {
            EIP.ServiceAttribute(ccPF.Add_Column, 0);
         }
         EIP.SetAttribute(ccIDX.Item, Item);

         EIP.SetAttribute(ccPF.Print_Character_String, "=>{{E}}<=");

         // Set < Shift Number="1" StartHour="00" StartMinute="00" EndHour="7" EndMinute="59" Text="D" />
         EIP.SetAttribute(ccIDX.Calendar_Block, 1);
         EIP.SetAttribute(ccCal.Shift_Start_Hour, 0);
         EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
         EIP.SetAttribute(ccCal.Shift_String_Value, "D");

         // Set < Shift Number="2" StartHour="8" StartMinute="00" EndHour="15" EndMinute="59" Text="E" />
         EIP.SetAttribute(ccIDX.Calendar_Block, 2);
         EIP.SetAttribute(ccCal.Shift_Start_Hour, 8);
         EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
         EIP.SetAttribute(ccCal.Shift_String_Value, "E");

         // Set < Shift Number="2" StartHour="16" StartMinute="00" EndHour="23" EndMinute="59" Text="F" />
         EIP.SetAttribute(ccIDX.Calendar_Block, 3);
         EIP.SetAttribute(ccCal.Shift_Start_Hour, 16);
         EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
         EIP.SetAttribute(ccCal.Shift_String_Value, "F");
         return true;
      }

      private bool BuildMonthDaySR(int Rule) {
         // Set <Substitution Rule="01" StartYear="2010" Delimeter="/">
         char delimeter = '/';
         EIP.SetAttribute(ccIDX.Substitution_Rule, Rule);
         EIP.SetAttribute(ccSR.Start_Year, 2010);

         // Set <Month Base="1">JAN/FEB/MAR/APR/MAY/JUN/JUL/AUG/SEP/OCT/NOV/DEC</Month>
         string[] months = "JAN/FEB/MAR/APR/MAY/JUN/JUL/AUG/SEP/OCT/NOV/DEC".Split(delimeter);
         for (int i = 0; i < months.Length; i++) {
            EIP.SetAttribute(ccSR.Month, i + 1, months[i]);
         }

         // Set <DayOfWeek Base="1">MON/TUE/WED/THU/FRI/SAT/SUN</DayOfWeek>
         string[] day = "MON/TUE/WED/THU/FRI/SAT/SUN".Split(delimeter);
         for (int i = 0; i < day.Length; i++) {
            EIP.SetAttribute(ccSR.Day_Of_Week, i + 1, day[i]);
         }
         return true;
      }

      private bool BuildTimeCount(int Item) {
         int block = 1;
         EIP.SetAttribute(ccIDX.Item, Item);

         EIP.SetAttribute(ccPF.Print_Character_String, "=>{{FF}}<=");
         EIP.GetAttribute(ccCal.First_Calendar_Block, out block);
         EIP.SetAttribute(ccIDX.Calendar_Block, block);

         // Set <TimeCount Start="AA" End="JJ" Reset="AA" ResetTime="6" RenewalPeriod="30 Minutes" />
         EIP.SetAttribute(ccCal.Update_Interval_Value, "30 Minutes");
         EIP.SetAttribute(ccCal.Time_Count_Start_Value, "A1");
         EIP.SetAttribute(ccCal.Time_Count_End_Value, "X2");
         EIP.SetAttribute(ccCal.Reset_Time_Value, 6);
         EIP.SetAttribute(ccCal.Time_Count_Reset_Value, "A1");
         return true;
      }

      private bool TryDayOfWeekEtc(int Item) {
         if (Item != 1) {
            EIP.ServiceAttribute(ccPF.Add_Column);
         }
         EIP.SetAttribute(ccIDX.Item, Item);
         EIP.SetAttribute(ccIDX.Calendar_Block, Item);
         EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
         EIP.SetAttribute(ccPF.InterCharacter_Space, 1);
         EIP.SetAttribute(ccPF.Print_Character_String, "=>{{77}-{WW}-{TTT}}<=");
         EIP.SetAttribute(ccCal.Substitute_Weeks, "Disable");
         EIP.SetAttribute(ccCal.Zero_Suppress_Weeks, "Disable");
         EIP.SetAttribute(ccCal.Substitute_Day_Of_Week, "Enable");
         EIP.SetAttribute(ccCal.Zero_Suppress_Day_Of_Week, "Disable");
         return true;
      }

      private bool BuildMDYhms(int Item, int Rule) {
         bool success = true;
         try {
            int firstBlock = 1;
            int blockCount = 1;
            // Add the item if needed and select it
            EIP.SetAttribute(ccIDX.Item, Item);

            // Set Text
            EIP.SetAttribute(ccPF.Print_Character_String, "{{MMM}/{DD}/{YY}} {{hh}:{mm}:{ss}}");

            // Get first block and Substitution Rule
            EIP.GetAttribute(ccCal.First_Calendar_Block, out firstBlock);
            EIP.GetAttribute(ccCal.Number_of_Calendar_Blocks, out blockCount);

            // Set Item in Calendar Index
            EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock);

            //// Point to first substitution rule
            //EIP.SetAttribute(ccIDX.Substitution_Rules_Setting, Rule);

            // Set <EnableSubstitution SubstitutionRule="01" Year="False" Month="True"  Day="False" />
            EIP.SetAttribute(ccCal.Substitute_Year, "Disable");
            EIP.SetAttribute(ccCal.Substitute_Month, "Enable");
            EIP.SetAttribute(ccCal.Substitute_Day, "Disable");

            // Set <Offset Year="1" Month="2" Day="3" />
            EIP.SetAttribute(ccCal.Offset_Year, 1);
            EIP.SetAttribute(ccCal.Offset_Month, 2);
            EIP.SetAttribute(ccCal.Offset_Day, 3);

            // Set <ZeroSuppress Year="Disable" Month="Disable" Day="Disable" />
            EIP.SetAttribute(ccCal.Zero_Suppress_Year, "Disable");
            EIP.SetAttribute(ccCal.Zero_Suppress_Month, "Disable");
            EIP.SetAttribute(ccCal.Zero_Suppress_Day, "Disable");

            // Set Item in Calendar Index
            if (blockCount > 1) {
               EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock + 1);
            }

            // Set <EnableSubstitution SubstitutionRule="01" Year="False" Month="True"  Day="False" 
            //      Hour ="False" Minute="False" Week="False" DayOfWeek="False" />
            EIP.SetAttribute(ccCal.Substitute_Hour, "Disable");
            EIP.SetAttribute(ccCal.Substitute_Minute, "Disable");

            // Set <Offset Year="1" Month="2" Day="3" Hour="-4" Minute="-5" />
            EIP.SetAttribute(ccCal.Offset_Hour, 4);
            EIP.SetAttribute(ccCal.Offset_Minute, -5);

            // Set <ZeroSuppress Year="Disable" Month="Disable" Day="Disable"
            //      Hour ="Space Fill" Minute="Character Fill" />
            EIP.SetAttribute(ccCal.Zero_Suppress_Hour, "Space Fill");
            EIP.SetAttribute(ccCal.Zero_Suppress_Minute, "Character Fill");

         } catch (EIPIOException e1) {
            // In case of an EIP I/O error
            string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
            string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
            MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
            success = false;
         } catch (Exception e2) {
            // You are on your own here
         }
         return success;
      }

      private bool MultiLine() {
         bool success = true;
         if (EIP.StartSession(true)) {    // Open a session
            if (EIP.ForwardOpen()) {  // open a data forwarding path
               try {
                  // Be sure we are in Individual Layout
                  EIP.SetAttribute(ccPF.Format_Setup, "Individual");
                  // Select item 1 and set to 1 line (1 origin on Line Count)
                  EIP.SetAttribute(ccIDX.Item, 1);
                  EIP.SetAttribute(ccPF.Line_Count, 1);
                  // Add four more columns
                  for (int i = 2; i <= 5; i++) {
                     EIP.ServiceAttribute(ccPF.Add_Column, 0);
                  }
                  // Stack columns 2 and 4 (1 origin on Line Count)
                  EIP.SetAttribute(ccIDX.Item, 2);
                  EIP.SetAttribute(ccPF.Line_Count, 2);
                  EIP.SetAttribute(ccIDX.Item, 4);
                  EIP.SetAttribute(ccPF.Line_Count, 2);
                  for (int i = 1; i <= 7; i++) {
                     EIP.SetAttribute(ccIDX.Item, i);  // Select item
                     if (i == 1 || i == 4 || i == 7) { // Set the font and text
                        EIP.SetAttribute(ccPF.Print_Character_String, $"{i}");
                        EIP.SetAttribute(ccPF.Dot_Matrix, "12x16");
                     } else {
                        EIP.SetAttribute(ccPF.Print_Character_String, $" {i} ");
                        EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
                     }
                  }
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
                  success = false;
               } catch (Exception e2) {
                  // You are on your own here
               }
            }
            EIP.ForwardClose(); // Must be outside the ForwardOpen if block
         }
         EIP.EndSession();      // Must be outside the StartSession if block
         return success;
      }

      private bool CreateCounter() {
         bool success = true;
         int firstBlock = 1;
         int blockCount = 1;
         if (EIP.StartSession(true)) {
            if (EIP.ForwardOpen()) {
               try {
                  // Set to first item
                  int item = 1;

                  // Select item #1
                  EIP.SetAttribute(ccIDX.Item, item);

                  // Set Text as a 4 digit counter
                  EIP.SetAttribute(ccPF.Print_Character_String, "{{CCCC}} {{CCC}}");

                  // Now retrieve the counter block allocations
                  EIP.GetAttribute(ccCount.First_Count_Block, out firstBlock);
                  EIP.GetAttribute(ccCount.Number_Of_Count_Blocks, out blockCount);

                  // Set <Counter InitialValue="0001" Range1="0000" Range2="9999" JumpFrom="6666" JumpTo ="7777"
                  //      Increment="1" Direction="Up" ZeroSuppression="Enable" UpdateIP="0" UpdateUnit="1"
                  //      Multiplier ="2" CountSkip="0" Reset="0001" ExternalSignal="Disable" ResetSignal="Signal 1" />

                  // Set item number in count block
                  EIP.SetAttribute(ccIDX.Count_Block, firstBlock);

                  EIP.SetAttribute(ccCount.Initial_Value, "0001");
                  EIP.SetAttribute(ccCount.Count_Range_1, "0000");
                  EIP.SetAttribute(ccCount.Count_Range_2, "9999");
                  EIP.SetAttribute(ccCount.Jump_From, "6666");
                  EIP.SetAttribute(ccCount.Jump_To, "7777");
                  EIP.SetAttribute(ccCount.Increment_Value, 1);
                  EIP.SetAttribute(ccCount.Direction_Value, "Up");
                  EIP.SetAttribute(ccCount.Zero_Suppression, "Disable");
                  EIP.SetAttribute(ccCount.Count_Multiplier, "2");
                  EIP.SetAttribute(ccCount.Reset_Value, "0001");
                  EIP.SetAttribute(ccCount.Count_Skip, "0");

                  EIP.SetAttribute(ccCount.Update_Unit_Halfway, 0);           // Causes COM Error
                  EIP.SetAttribute(ccCount.Update_Unit_Unit, 1);              // Causes COM Error
                  EIP.SetAttribute(ccCount.Type_Of_Reset_Signal, "Signal 1"); // Causes COM Error
                  EIP.SetAttribute(ccCount.External_Count, "Disable");        // Causes COM Error

                  // In case it is the two counter test
                  if (blockCount > 1) {
                     EIP.SetAttribute(ccIDX.Count_Block, firstBlock + 1);
                     EIP.SetAttribute(ccCount.Initial_Value, "001");
                     EIP.SetAttribute(ccCount.Count_Range_1, "000");
                     EIP.SetAttribute(ccCount.Count_Range_2, "999");
                     EIP.SetAttribute(ccCount.Jump_From, "199");
                     EIP.SetAttribute(ccCount.Jump_To, "300");
                     EIP.SetAttribute(ccCount.Increment_Value, 2);
                     EIP.SetAttribute(ccCount.Direction_Value, "Down");
                     EIP.SetAttribute(ccCount.Zero_Suppression, "Disable");
                     EIP.SetAttribute(ccCount.Count_Multiplier, "2");
                     EIP.SetAttribute(ccCount.Reset_Value, "001");
                     EIP.SetAttribute(ccCount.Count_Skip, "0");

                     EIP.SetAttribute(ccCount.Update_Unit_Halfway, 0);           // Causes COM Error
                     EIP.SetAttribute(ccCount.Update_Unit_Unit, 1);              // Causes COM Error
                     EIP.SetAttribute(ccCount.Type_Of_Reset_Signal, "Signal 1"); // Causes COM Error
                     EIP.SetAttribute(ccCount.External_Count, "Disable");        // Causes COM Error

                  }
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
                  success = false;
               } catch (Exception e2) {
                  // You are on your own here
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      // Create a message with three rows, two columns,and a Logo that contains one of everything
      private bool Comprehensive() {
         bool success = true;
         string[] itemText = new string[] {
            "SELLBY {{MMM}/{DD}/{YY}}  ", "USE BY {{MMM}/{DD}/{YY}}  ", "PACKED {{TTT} {777}} ",
            "Shift {{E}}", "T-Ct {{FF}} ", "#{{CCCCCC}} ", "{X/0}"
         };
         int firstBlock = 1;
         if (EIP.StartSession(true)) {
            if (EIP.ForwardOpen()) {
               try {
                  // Clean up the display
                  {
                     EIP.GetAttribute(ccPF.Number_Of_Columns, out int cols);
                     if (cols > 1) {
                        EIP.SetAttribute(ccIDX.Column, 1); // Actually column 2
                        while (--cols > 0) {
                           EIP.ServiceAttribute(ccPF.Delete_Column);
                        }
                     }
                     EIP.SetAttribute(ccIDX.Item, 1);
                     EIP.SetAttribute(ccPF.Line_Count, 1);
                     // Avoid issues with add columns
                     EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
                     EIP.SetAttribute(ccPF.Barcode_Type, "None");
                     EIP.SetAttribute(ccPF.Print_Character_String, "1");
                  }

                  // Load the message properties
                  {
                     EIP.SetAttribute(ccPF.Format_Setup, "Individual");
                     EIP.SetAttribute(ccPS.Character_Orientation, "Normal/Forward");
                     EIP.SetAttribute(ccPS.Target_Sensor_Filter, "Time Setup");
                     EIP.SetAttribute(ccPS.Targer_Sensor_Filter_Value, 50);
                     EIP.SetAttribute(ccPS.Target_Sensor_Timer, 0);
                     EIP.SetAttribute(ccPS.Character_Height, 99);
                     EIP.SetAttribute(ccPS.Character_Width, 10);
                     EIP.SetAttribute(ccPS.Print_Start_Delay_Forward, 55);
                     EIP.SetAttribute(ccPS.Print_Start_Delay_Reverse, 45);
                     EIP.SetAttribute(ccPS.Ink_Drop_Use, 2);
                     EIP.SetAttribute(ccPS.Ink_Drop_Charge_Rule, "Mixed");
                     EIP.SetAttribute(ccPS.Product_Speed_Matching, "Auto");
                  }
                  // Set up the rows and columns
                  {
                     // First column is already there, just create the second and third columns
                     EIP.ServiceAttribute(ccPF.Add_Column);
                     EIP.ServiceAttribute(ccPF.Add_Column);
                     // Allocate the items in each column (Should this be Column and not Item?)
                     EIP.SetAttribute(ccIDX.Item, 1);
                     EIP.SetAttribute(ccPF.Line_Count, 3);
                     EIP.SetAttribute(ccIDX.Item, 2);
                     EIP.SetAttribute(ccPF.Line_Count, 3);
                     EIP.SetAttribute(ccIDX.Item, 3);
                     EIP.SetAttribute(ccPF.Line_Count, 1);
                     // Set the Interline Spacing
                     EIP.SetAttribute(ccIDX.Column, 1);
                     EIP.SetAttribute(ccPF.Line_Spacing, 1);
                     EIP.SetAttribute(ccIDX.Column, 2);
                     EIP.SetAttribute(ccPF.Line_Spacing, 2);
                  }

                  // Format the items
                  {
                     // Set the format consistant for all six items
                     for (int i = 1; i <= 6; i++) {
                        EIP.SetAttribute(ccIDX.Item, i);
                        EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
                        EIP.SetAttribute(ccPF.InterCharacter_Space, 1);
                        EIP.SetAttribute(ccPF.Character_Bold, 1);
                        EIP.SetAttribute(ccPF.Barcode_Type, "None");
                        EIP.SetAttribute(ccPF.Print_Character_String, itemText[i - 1]);
                     }
                     // Set a logo into the seventh item
                     EIP.SetAttribute(ccIDX.Item, 7);
                     EIP.SetAttribute(ccPF.Dot_Matrix, "18x24");
                     EIP.SetAttribute(ccPF.InterCharacter_Space, 2);
                     EIP.SetAttribute(ccPF.Character_Bold, 2);
                     EIP.SetAttribute(ccPF.Barcode_Type, "None");
                     EIP.SetAttribute(ccPF.Print_Character_String, itemText[6]);
                  }

                  // Set up the clock for item 1
                  {
                     EIP.SetAttribute(ccIDX.Item, 1);
                     EIP.GetAttribute(ccCal.First_Calendar_Block, out firstBlock);
                     EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock);
                     EIP.SetAttribute(ccCal.Substitute_Month, "Enable");
                     EIP.SetAttribute(ccCal.Zero_Suppress_Day, "Enable");
                  }
                  // Set up the clock for item 2
                  {
                     EIP.SetAttribute(ccIDX.Item, 2);
                     EIP.GetAttribute(ccCal.First_Calendar_Block, out firstBlock);
                     EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock);
                     EIP.SetAttribute(ccCal.Substitute_Month, "Enable");
                     EIP.SetAttribute(ccCal.Zero_Suppress_Day, "Enable");
                     EIP.SetAttribute(ccCal.Offset_Day, 30);
                     EIP.SetAttribute(ccPF.Calendar_Offset, "From Yesterday");
                  }
                  // Set up the clock for item 3
                  {
                     EIP.SetAttribute(ccIDX.Item, 3);
                     EIP.GetAttribute(ccCal.First_Calendar_Block, out firstBlock);
                     EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock);
                     EIP.SetAttribute(ccCal.Substitute_Day_Of_Week, "Enable");
                  }
                  // Set up the clock for item 4
                  {
                     EIP.SetAttribute(ccIDX.Item, 4);
                     EIP.GetAttribute(ccCal.First_Calendar_Block, out firstBlock);
                     EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock);

                     // Set < Shift Number="1" StartHour="00" StartMinute="00" EndHour="7" EndMinute="59" Text="D" />
                     EIP.SetAttribute(ccIDX.Calendar_Block, 1);
                     EIP.SetAttribute(ccCal.Shift_Start_Hour, 0);
                     EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
                     EIP.SetAttribute(ccCal.Shift_String_Value, "D");

                     // Set < Shift Number="2" StartHour="8" StartMinute="00" EndHour="15" EndMinute="59" Text="E" />
                     EIP.SetAttribute(ccIDX.Calendar_Block, 2);
                     EIP.SetAttribute(ccCal.Shift_Start_Hour, 8);
                     EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
                     EIP.SetAttribute(ccCal.Shift_String_Value, "E");

                     // Set < Shift Number="2" StartHour="16" StartMinute="00" EndHour="23" EndMinute="59" Text="F" />
                     EIP.SetAttribute(ccIDX.Calendar_Block, 3);
                     EIP.SetAttribute(ccCal.Shift_Start_Hour, 16);
                     EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
                     EIP.SetAttribute(ccCal.Shift_String_Value, "F");
                  }
                  // Set up the clock for item 5
                  {
                     EIP.SetAttribute(ccIDX.Item, 5);
                     EIP.GetAttribute(ccCal.First_Calendar_Block, out firstBlock);
                     EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock);

                     // Set <TimeCount Start="A1" End="X2" Reset="A1" ResetTime="6" RenewalPeriod="30 Minutes" />
                     EIP.SetAttribute(ccCal.Update_Interval_Value, "30 Minutes");
                     EIP.SetAttribute(ccCal.Time_Count_Start_Value, "A1");
                     EIP.SetAttribute(ccCal.Time_Count_End_Value, "X2");
                     EIP.SetAttribute(ccCal.Reset_Time_Value, 6);
                     EIP.SetAttribute(ccCal.Time_Count_Reset_Value, "A1");
                  }
                  // Set up the counter for item 6
                  {
                     EIP.SetAttribute(ccIDX.Item, 6);
                     EIP.GetAttribute(ccCount.First_Count_Block, out firstBlock);
                     EIP.SetAttribute(ccIDX.Count_Block, firstBlock);

                     EIP.SetAttribute(ccCount.Initial_Value, "000001");
                     EIP.SetAttribute(ccCount.Count_Range_1, "000000");
                     EIP.SetAttribute(ccCount.Count_Range_2, "999999");
                     EIP.SetAttribute(ccCount.Jump_From, "000199");
                     EIP.SetAttribute(ccCount.Jump_To, "000300");
                     EIP.SetAttribute(ccCount.Increment_Value, 2);
                     EIP.SetAttribute(ccCount.Direction_Value, "Down");
                     EIP.SetAttribute(ccCount.Zero_Suppression, "Enable");
                     EIP.SetAttribute(ccCount.Count_Multiplier, "2");
                     EIP.SetAttribute(ccCount.Reset_Value, "000001");
                     EIP.SetAttribute(ccCount.Count_Skip, "0");
                     EIP.SetAttribute(ccCount.Update_Unit_Halfway, 0);
                     EIP.SetAttribute(ccCount.Update_Unit_Unit, 1);
                     EIP.SetAttribute(ccCount.Type_Of_Reset_Signal, "Signal 1");
                     EIP.SetAttribute(ccCount.External_Count, "Disable");
                  }
                  // Set up the logo for item 7
                  {
                     // Once logo processing works in the printer, loading of the logo will be added here.
                  }
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
                  success = false;
               } catch (Exception e2) {
                  // You are on your own here
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      public bool SetText(string text) {
         bool success = true;
         int calNo = 0;
         string[] s = text.Split('\n');
         if (EIP.StartSession(true)) {
            if (EIP.ForwardOpen()) {
               try {
                  // Select the item
                  EIP.SetAttribute(ccIDX.Item, 1);
                  // Insert the text
                  EIP.SetAttribute(ccPF.Print_Character_String, s[0]);
                  for (int i = 1; i < s.Length; i++) {
                     EIP.ServiceAttribute(ccPF.Add_Column);
                     EIP.SetAttribute(ccIDX.Item, i + 1);
                     EIP.SetAttribute(ccPF.Print_Character_String, s[i]);
                  }
                  // Set info in first Calendar Block
                  EIP.SetAttribute(ccIDX.Item, 1);
                  EIP.GetAttribute(ccCal.First_Calendar_Block, out calNo);
                  EIP.SetAttribute(ccIDX.Calendar_Block, calNo);
                  EIP.SetAttribute(ccCal.Offset_Month, 1);
                  // Set info in Second Calendar Block
                  EIP.SetAttribute(ccIDX.Item, 2);
                  EIP.GetAttribute(ccCal.First_Calendar_Block, out calNo);
                  EIP.SetAttribute(ccIDX.Calendar_Block, calNo);
                  EIP.SetAttribute(ccCal.Zero_Suppress_Hour, "Space Fill");
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
                  success = false;
               } catch (Exception e2) {
                  // You are on your own here
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      #endregion

   }
}
