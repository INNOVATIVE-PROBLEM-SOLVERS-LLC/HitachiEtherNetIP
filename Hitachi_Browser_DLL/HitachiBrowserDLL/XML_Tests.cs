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
         success = true;
         int Item = 1;
         int Rule = 1;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Clean up the display
               success = success && CleanUpDisplay();
               // Run selected test
               switch (cbAvailableHardTests.SelectedIndex) {
                  case 0:
                     // Gets us down to a single item
                     break;
                  case 1:
                     success = success && BuildShifts(Item++);
                     break;
                  case 2:
                     success = success && BuildMonthDaySR(Rule);
                     break;
                  case 3:
                     success = success && BuildTimeCount(Item++);
                     break;
                  case 4:
                     success = success && TryDayOfWeekEtc(Item++);
                     break;
                  case 5:
                     success = success && BuildMDYhms(Item++, Rule);
                     break;
                  case 6:
                     success = success && MultiLine();
                     break;
                  case 7:
                     success = success && CreateCounter();
                     break;
                  case 8:
                     success = success && SetText("{{MMM}/{DD}/{YY}}\n {{hh}:{mm}:{ss}}");
                     break;
               }
               //success = success && VerifyShifts(Item++);
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      #endregion

      #region Test Routines

      public bool CleanUpDisplay() {
         success = true;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Get the number of columns
               success = EIP.GetAttribute(ccPF.Number_Of_Columns, out int cols);
               // Make things faster
               //success = EIP.SetAttribute(ccIDX.Automatic_reflection, 1);
               // No need to delete columns if there is only one
               if (cols > 1) {
                  // Select to continuously delete column 2 (0 origin on deletes)
                  success = EIP.SetAttribute(ccIDX.Column, 1);
                  // Column number is 0 origin
                  while (success && cols > 1) {
                     // Delete the column
                     success = EIP.ServiceAttribute(ccPF.Delete_Column, 0);
                     cols--;
                  }
               }
               // Select item 1 (1 origin on Line Count)
               success = EIP.SetAttribute(ccIDX.Item, 1);
               // Set line count to 1. (Need to find out how delete single item works.)
               success = success && EIP.SetAttribute(ccPF.Line_Count, 1);
               // Test item size
               success = success && EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
               success = success && EIP.SetAttribute(ccPF.Barcode_Type, "None");
               // Set simple text in case Calendar or Counter was used
               success = success && EIP.SetAttribute(ccPF.Print_Character_String, "1");
               // Make things faster
               //success = EIP.SetAttribute(ccIDX.Automatic_reflection, 0);
               //success = EIP.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      private bool BuildShifts(int Item) {
         // Add the item if needed and select it
         if (Item != 1) {
            success = success && EIP.ServiceAttribute(ccPF.Add_Column, 0);
         }
         success = success && EIP.SetAttribute(ccIDX.Item, Item);

         //// Set Item in Calendar Index
         EIP.SetAttribute(ccIDX.Calendar_Block, Item);

         success = success && EIP.SetAttribute(ccPF.Print_Character_String, "=>{{EE}}<=");

         // Set < Shift Number="1" StartHour="00" StartMinute="00" EndHour="7" EndMinute="59" Text="CC" />
         success = success && EIP.SetAttribute(ccIDX.Calendar_Block, 1);
         success = success && EIP.SetAttribute(ccCal.Shift_Start_Hour, 0);
         success = success && EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
         success = success && EIP.SetAttribute(ccCal.Shift_String_Value, "D");

         // Set < Shift Number="2" StartHour="8" StartMinute="00" EndHour="15" EndMinute="59" Text="AA" />
         success = success && EIP.SetAttribute(ccIDX.Calendar_Block, 2);
         success = success && EIP.SetAttribute(ccCal.Shift_Start_Hour, 8);
         success = success && EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
         success = success && EIP.SetAttribute(ccCal.Shift_String_Value, "E");

         // Set < Shift Number="2" StartHour="16" StartMinute="00" EndHour="23" EndMinute="59" Text="BB" />
         success = success && EIP.SetAttribute(ccIDX.Calendar_Block, 3);
         success = success && EIP.SetAttribute(ccCal.Shift_Start_Hour, 16);
         success = success && EIP.SetAttribute(ccCal.Shift_Start_Minute, 0);
         success = success && EIP.SetAttribute(ccCal.Shift_String_Value, "F");
         return success;
      }

      private bool BuildMonthDaySR(int Rule) {
         // Set <Substitution Rule="01" StartYear="2010" Delimeter="/">
         char delimeter = '/';
         success = success && EIP.SetAttribute(ccIDX.Substitution_Rules_Setting, Rule);
         success = success && EIP.SetAttribute(ccSR.Start_Year, 2010);

         // Set <Month Base="1">JAN/FEB/MAR/APR/MAY/JUN/JUL/AUG/SEP/OCT/NOV/DEC</Month>
         string[] months = "JAN/FEB/MAR/APR/MAY/JUN/JUL/AUG/SEP/OCT/NOV/DEC".Split(delimeter);
         for (int i = 0; i < months.Length && success; i++) {
            success = EIP.SetAttribute(ccSR.Month, i + 1, months[i]);
         }

         // Set <DayOfWeek Base="1">MON/TUE/WED/THU/FRI/SAT/SUN</DayOfWeek>
         string[] day = "MON/TUE/WED/THU/FRI/SAT/SUN".Split(delimeter);
         for (int i = 0; i < day.Length && success; i++) {
            success = EIP.SetAttribute(ccSR.Day_Of_Week, i + 1, day[i]);
         }
         return success;
      }

      private bool BuildTimeCount(int Item) {
         success = success && EIP.SetAttribute(ccIDX.Item, Item);

         // Set Item in Calendar Index
         success = success && EIP.SetAttribute(ccIDX.Calendar_Block, Item);

         success = success && EIP.SetAttribute(ccPF.Print_Character_String, "=>{{FF}}<=");

         // Set <TimeCount Start="AA" End="JJ" Reset="AA" ResetTime="6" RenewalPeriod="30 Minutes" />
         success = success && EIP.SetAttribute(ccCal.Time_Count_Start_Value, "AA");
         success = success && EIP.SetAttribute(ccCal.Time_Count_End_Value, "KK");
         success = success && EIP.SetAttribute(ccCal.Time_Count_Reset_Value, "AA");
         success = success && EIP.SetAttribute(ccCal.Reset_Time_Value, 6);
         success = success && EIP.SetAttribute(ccCal.Update_Interval_Value, "30 Minutes");
         return success;
      }

      private bool TryDayOfWeekEtc(int Item) {
         if (Item != 1) {
            success = success && EIP.ServiceAttribute(ccPF.Add_Column);
         }
         success = success && EIP.SetAttribute(ccIDX.Item, Item);
         success = success && EIP.SetAttribute(ccIDX.Calendar_Block, Item);
         success = success && EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
         success = success && EIP.SetAttribute(ccPF.InterCharacter_Space, 1);
         success = success && EIP.SetAttribute(ccPF.Print_Character_String, "=>{{77}-{WW}-{TTT}}<=");
         success = success && EIP.SetAttribute(ccCal.Substitute_Weeks, "Disable");
         success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Weeks, "Disable");
         success = success && EIP.SetAttribute(ccCal.Substitute_Day_Of_Week, "Enable");
         success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Day_Of_Week, "Disable");
         return success;
      }

      private bool BuildMDYhms(int Item, int Rule) {
         int firstBlock = 1;
         int blockCount = 1;
         // Add the item if needed and select it
         success = success && EIP.SetAttribute(ccIDX.Item, Item);

         // Set Text
         success = success && EIP.SetAttribute(ccPF.Print_Character_String, "{{MMM}/{DD}/{YY}} {{hh}:{mm}:{ss}}");

         // Get first block and Substitution Rule
         success = success && EIP.GetAttribute(ccCal.First_Calendar_Block, out firstBlock);
         success = success && EIP.GetAttribute(ccCal.Number_of_Calendar_Blocks, out blockCount);

         // Set Item in Calendar Index
         success = success && EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock);

         //// Point to first substitution rule
         //success = success && EIP.SetAttribute(ccIDX.Substitution_Rules_Setting, Rule);

         // Set <EnableSubstitution SubstitutionRule="01" Year="False" Month="True"  Day="False" />
         success = success && EIP.SetAttribute(ccCal.Substitute_Year, "Disable");
         success = success && EIP.SetAttribute(ccCal.Substitute_Month, "Enable");
         success = success && EIP.SetAttribute(ccCal.Substitute_Day, "Disable");

         // Set <Offset Year="1" Month="2" Day="3" />
         success = success && EIP.SetAttribute(ccCal.Offset_Year, 1);
         success = success && EIP.SetAttribute(ccCal.Offset_Month, 2);
         success = success && EIP.SetAttribute(ccCal.Offset_Day, 3);

         // Set <ZeroSuppress Year="Disable" Month="Disable" Day="Disable" />
         success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Year, "Disable");
         success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Month, "Disable");
         success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Day, "Disable");

         // Set Item in Calendar Index
         if (blockCount > 1) {
            success = success && EIP.SetAttribute(ccIDX.Calendar_Block, firstBlock + 1);
         }

         // Set <EnableSubstitution SubstitutionRule="01" Year="False" Month="True"  Day="False" 
         //      Hour ="False" Minute="False" Week="False" DayOfWeek="False" />
         success = success && EIP.SetAttribute(ccCal.Substitute_Hour, "Disable");
         success = success && EIP.SetAttribute(ccCal.Substitute_Minute, "Disable");

         // Set <Offset Year="1" Month="2" Day="3" Hour="-4" Minute="-5" />
         success = success && EIP.SetAttribute(ccCal.Offset_Hour, 4);
         success = success && EIP.SetAttribute(ccCal.Offset_Minute, -5);

         // Set <ZeroSuppress Year="Disable" Month="Disable" Day="Disable"
         //      Hour ="Space Fill" Minute="Character Fill" />
         success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Hour, "Space Fill");
         success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Minute, "Character Fill");

         return success;
      }

      private bool MultiLine() {
         success = true;
         if (EIP.StartSession()) {    // Open a session
            if (EIP.ForwardOpen()) {  // open a data forwarding path
               // Be sure we are in Individual Layout
               success = success && EIP.SetAttribute(ccPF.Format_Setup, "Individual");
               // Select item 1 and set to 1 line (1 origin on Line Count)
               success = success && EIP.SetAttribute(ccIDX.Item, 1);
               success = success && EIP.SetAttribute(ccPF.Line_Count, 1);
               // Add four more columns
               for (int i = 2; success && i <= 5; i++) {
                  success = success && EIP.ServiceAttribute(ccPF.Add_Column, 0);
               }
               // Stack columns 2 and 4 (1 origin on Line Count)
               success = success && EIP.SetAttribute(ccIDX.Item, 2);
               success = success && EIP.SetAttribute(ccPF.Line_Count, 2);
               success = success && EIP.SetAttribute(ccIDX.Item, 4);
               success = success && EIP.SetAttribute(ccPF.Line_Count, 2);
               for (int i = 1; i <= 7; i++) {
                  success = success && EIP.SetAttribute(ccIDX.Item, i);  // Select item
                  if (i == 1 || i == 4 || i == 7) { // Set the font and text
                     success = success && EIP.SetAttribute(ccPF.Print_Character_String, $"{i}");
                     success = success && EIP.SetAttribute(ccPF.Dot_Matrix, "12x16");
                  } else {
                     success = success && EIP.SetAttribute(ccPF.Print_Character_String, $" {i} ");
                     success = success && EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
                  }
               }
            }
            EIP.ForwardClose(); // Must be outside the ForwardOpen if block
         }
         EIP.EndSession();      // Must be outside the StartSession if block
         return success;
      }

      private bool CreateCounter() {
         success = true;
         int firstBlock = 1;
         int blockCount = 1;
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Set to first item
               int item = 1;

               // Select item #1
               success = success && EIP.SetAttribute(ccIDX.Item, item);

               // Set Text as a 4 digit counter
               success = success && EIP.SetAttribute(ccPF.Print_Character_String, "{{CCCC}} {{CCC}}");

               // Now retrieve the counter block allocations
               success = success && EIP.GetAttribute(ccCount.First_Count_Block, out firstBlock);
               success = success && EIP.GetAttribute(ccCount.Number_Of_Count_Blocks, out blockCount);

               // Set <Counter InitialValue="0001" Range1="0000" Range2="9999" JumpFrom="6666" JumpTo ="7777"
               //      Increment="1" Direction="Up" ZeroSuppression="Enable" UpdateIP="0" UpdateUnit="1"
               //      Multiplier ="2" CountSkip="0" Reset="0001" ExternalSignal="Disable" ResetSignal="Signal 1" />

               // Set item number in count block
               success = success && EIP.SetAttribute(ccIDX.Count_Block, firstBlock);

               success = success && EIP.SetAttribute(ccCount.Initial_Value, "0001");
               success = success && EIP.SetAttribute(ccCount.Count_Range_1, "0000");
               success = success && EIP.SetAttribute(ccCount.Count_Range_2, "9999");
               success = success && EIP.SetAttribute(ccCount.Jump_From, "6666");
               success = success && EIP.SetAttribute(ccCount.Jump_To, "7777");
               success = success && EIP.SetAttribute(ccCount.Increment_Value, 1);
               success = success && EIP.SetAttribute(ccCount.Direction_Value, "Up");
               success = success && EIP.SetAttribute(ccCount.Zero_Suppression, "Disable");
               success = success && EIP.SetAttribute(ccCount.Count_Multiplier, "2");
               success = success && EIP.SetAttribute(ccCount.Reset_Value, "0001");
               success = success && EIP.SetAttribute(ccCount.Count_Skip, "0");

               success = success && EIP.SetAttribute(ccCount.Update_Unit_Halfway, 0);           // Causes COM Error
               success = success && EIP.SetAttribute(ccCount.Update_Unit_Unit, 1);              // Causes COM Error
               success = success && EIP.SetAttribute(ccCount.Type_Of_Reset_Signal, "Signal 1"); // Causes COM Error
               success = success && EIP.SetAttribute(ccCount.External_Count, "Disable");        // Causes COM Error

               // In case it is the two counter test
               if (blockCount > 1) {
                  success = success && EIP.SetAttribute(ccIDX.Count_Block, firstBlock + 1);
                  success = success && EIP.SetAttribute(ccCount.Initial_Value, "001");
                  success = success && EIP.SetAttribute(ccCount.Count_Range_1, "000");
                  success = success && EIP.SetAttribute(ccCount.Count_Range_2, "999");
                  success = success && EIP.SetAttribute(ccCount.Jump_From, "199");
                  success = success && EIP.SetAttribute(ccCount.Jump_To, "300");
                  success = success && EIP.SetAttribute(ccCount.Increment_Value, 2);
                  success = success && EIP.SetAttribute(ccCount.Direction_Value, "Down");
                  success = success && EIP.SetAttribute(ccCount.Zero_Suppression, "Disable");
                  success = success && EIP.SetAttribute(ccCount.Count_Multiplier, "2");
                  success = success && EIP.SetAttribute(ccCount.Reset_Value, "001");
                  success = success && EIP.SetAttribute(ccCount.Count_Skip, "0");

                  success = success && EIP.SetAttribute(ccCount.Update_Unit_Halfway, 0);           // Causes COM Error
                  success = success && EIP.SetAttribute(ccCount.Update_Unit_Unit, 1);              // Causes COM Error
                  success = success && EIP.SetAttribute(ccCount.Type_Of_Reset_Signal, "Signal 1"); // Causes COM Error
                  success = success && EIP.SetAttribute(ccCount.External_Count, "Disable");        // Causes COM Error

               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      public bool SetText(string text) {
         success = true;
         int calNo = 0;
         string[] s = text.Split('\n');
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Select the item
               success = success && EIP.SetAttribute(ccIDX.Item, 1);
               // Insert the text
               success = success && EIP.SetAttribute(ccPF.Print_Character_String, s[0]);
               for (int i = 1; i < s.Length; i++) {
                  success = success && EIP.ServiceAttribute(ccPF.Add_Column);
                  success = success && EIP.SetAttribute(ccIDX.Item, i + 1);
                  success = success && EIP.SetAttribute(ccPF.Print_Character_String, s[i]);
               }
               // Set info in first Calendar Block
               success = success && EIP.SetAttribute(ccIDX.Item, 1);
               success = success && EIP.GetAttribute(ccCal.First_Calendar_Block, out calNo);
               success = success && EIP.SetAttribute(ccIDX.Calendar_Block, calNo);
               success = success && EIP.SetAttribute(ccCal.Offset_Month, 1);
               // Set info in Second Calendar Block
               success = success && EIP.SetAttribute(ccIDX.Item, 2);
               success = success && EIP.GetAttribute(ccCal.First_Calendar_Block, out calNo);
               success = success && EIP.SetAttribute(ccIDX.Calendar_Block, calNo);
               success = success && EIP.SetAttribute(ccCal.Zero_Suppress_Hour, "Space Fill");
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         return success;
      }

      private bool VerifyShifts(int Item) {
         // Need to rethink this
         // Select the Item
         bool success = EIP.SetAttribute(ccIDX.Item, Item);

         // For testing purposes, try to read then back
         success &= EIP.SetAttribute(ccIDX.Calendar_Block, 1);
         success &= EIP.GetAttribute(ccCal.Shift_Start_Hour, out int sh1) && sh1 == 0;
         success &= EIP.GetAttribute(ccCal.Shift_Start_Minute, out int sm1) && sm1 == 0;
         success &= EIP.GetAttribute(ccCal.Shift_End_Hour, out int eh1) && eh1 == 11;
         success &= EIP.GetAttribute(ccCal.Shift_End_Minute, out int em1) && em1 == 59;
         success &= EIP.GetAttribute(ccCal.Shift_String_Value, out string sv1) && sv1 == "AA";

         // For testing putposes, try to read then back
         success &= EIP.SetAttribute(ccIDX.Calendar_Block, 2);
         success &= EIP.GetAttribute(ccCal.Shift_Start_Hour, out int sh2) && sh1 == 12;
         success &= EIP.GetAttribute(ccCal.Shift_Start_Minute, out int sm2) && sm2 == 0;
         success &= EIP.GetAttribute(ccCal.Shift_End_Hour, out int eh2) && eh2 == 23;
         success &= EIP.GetAttribute(ccCal.Shift_End_Minute, out int em2) && em2 == 59;
         success &= EIP.GetAttribute(ccCal.Shift_String_Value, out string sv2) && sv2 == "BB";
         return success;
      }

      #endregion

   }
}
