using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiEIP {

   public enum fmtDD {
      None = -1,
      Decimal = 0,
      Ascii = 1,
      EnableDisable = 2,
      Hour12_24 = 3,
      CurrentTime_StopClock = 4,
      OnlineOffline = 5,
      None_Signal_1_2 = 6,
      UpDown = 7,
      ReadableCode = 8,
      BracodeType = 9,
      NormalReverse = 10,
      M15Q25 = 11,
      EditPrint = 12,
      YesterdayToday = 13,
      FontType = 14,
      Orientation = 15,
      ProductSpeedMatching = 16,
      HighSpeedPrint = 17,
      TargetSensorFilter = 18,
      UserPatternFont = 19,
   }

   public static class DataII {

      // Print_data_management (Class Code 0x66)
      private static AttrDataII<ccPDM>[] ccPDMII = new AttrDataII<ccPDM>[] {
         new AttrDataII<ccPDM>(ccPDM.Select_Message, GSS.Service,           // Select Message
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),
         new AttrDataII<ccPDM>(ccPDM.Store_Print_Data, GSS.Set,             // Store Print Data
				new Prop(15, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccPDM>(ccPDM.Delete_Print_Data, GSS.Set,            // Delete Print Data
				new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),
         new AttrDataII<ccPDM>(ccPDM.Print_Data_Name, GSS.Set,              // Print Data Name
				new Prop(10, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccPDM>(ccPDM.List_of_Messages, GSS.Get,             // List of Messages
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None), true),
         new AttrDataII<ccPDM>(ccPDM.Print_Data_Number, GSS.Set,            // Print Data Number
				new Prop(4, DataFormats.Decimal, 1, 2000, fmtDD.None)),
         new AttrDataII<ccPDM>(ccPDM.Change_Create_Group_Name, GSS.Set,     // Change Create Group Name
				new Prop(14, DataFormats.ASCII, 0, 14, fmtDD.None)),
         new AttrDataII<ccPDM>(ccPDM.Group_Deletion, GSS.Set,               // Group Deletion
				new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),
         new AttrDataII<ccPDM>(ccPDM.List_of_Groups, GSS.Get,               // List of Groups
				new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None), true),
         new AttrDataII<ccPDM>(ccPDM.Change_Group_Number, GSS.Set,          // Change Group Number
				new Prop(2, DataFormats.Decimal, 1, 99, fmtDD.None)),
      };

      // Print_format (Class Code 0x67)
      private static AttrDataII<ccPF>[] ccPFII = new AttrDataII<ccPF>[] {
         new AttrDataII<ccPF>(ccPF.Message_Name, GSS.Get,                   // Message Name
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None), true),
         new AttrDataII<ccPF>(ccPF.Number_Of_Items, GSS.Get,                // Number Of Items
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Number_Of_Columns, GSS.Get,              // Number Of Columns
				new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Format_Type, GSS.Get,                    // Format Type
				new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Insert_Column, GSS.Service,              // Insert Column
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Delete_Column, GSS.Service,              // Delete Column
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Add_Column, GSS.Service,                 // Add Column
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Number_Of_Print_Line_And_Print_Format, GSS.Set, // Number Of Print Line And Print Format
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Format_Setup, GSS.Set,                   // Format Setup
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Adding_Print_Items, GSS.Service,         // Adding Print Items
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Deleting_Print_Items, GSS.Service,       // Deleting Print Items
				new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Print_Character_String, GSS.GetSet,      // Print Character String
				new Prop(750, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Line_Count, GSS.GetSet,                  // Line Count
				new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Line_Spacing, GSS.GetSet,                // Line Spacing
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Dot_Matrix, GSS.GetSet,                  // Dot Matrix
				new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.FontType)),
         new AttrDataII<ccPF>(ccPF.InterCharacter_Space, GSS.GetSet,        // InterCharacter Space
				new Prop(1, DataFormats.Decimal, 0, 26, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Character_Bold, GSS.GetSet,              // Character Bold
				new Prop(1, DataFormats.Decimal, 1, 9, fmtDD.Decimal)),
         new AttrDataII<ccPF>(ccPF.Barcode_Type, GSS.GetSet,                // Barcode Type
				new Prop(1, DataFormats.Decimal, 0, 27, fmtDD.BracodeType)),
         new AttrDataII<ccPF>(ccPF.Readable_Code, GSS.GetSet,               // Readable Code
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ReadableCode)),
         new AttrDataII<ccPF>(ccPF.Prefix_Code, GSS.GetSet,                 // Prefix Code
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.X_and_Y_Coordinate, GSS.GetSet,          // X and Y Coordinate
				new Prop(3, DataFormats.XY, 0, 0, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.InterCharacter_SpaceII, GSS.GetSet,      // InterCharacter SpaceII
				new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Add_To_End_Of_String, GSS.Set,           // Add To End Of String
				new Prop(750, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccPF>(ccPF.Calendar_Offset, GSS.GetSet,             // Calendar Offset
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.YesterdayToday)),
         new AttrDataII<ccPF>(ccPF.DIN_Print, GSS.GetSet,                   // DIN Print
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.Ascii)),
         new AttrDataII<ccPF>(ccPF.EAN_Prefix, GSS.GetSet,                  // EAN Prefix
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EditPrint)),
         new AttrDataII<ccPF>(ccPF.Barcode_Printing, GSS.GetSet,            // Barcode Printing
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.NormalReverse)),
         new AttrDataII<ccPF>(ccPF.QR_Error_Correction_Level, GSS.GetSet,   // QR Error Correction Level
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.M15Q25)),
      };

      // Print_specification (Class Code 0x68)
      private static AttrDataII<ccPS>[] ccPSII = new AttrDataII<ccPS>[] {
         new AttrDataII<ccPS>(ccPS.Character_Height, GSS.GetSet,            // Character Height
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Ink_Drop_Use, GSS.GetSet,                // Ink Drop Use
				new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.High_Speed_Print, GSS.GetSet,            // High Speed Print
				new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.HighSpeedPrint)),
         new AttrDataII<ccPS>(ccPS.Character_Width, GSS.GetSet,             // Character Width
				new Prop(2, DataFormats.Decimal, 0, 3999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Character_Orientation, GSS.GetSet,       // Character Orientation
				new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.Orientation)),
         new AttrDataII<ccPS>(ccPS.Print_Start_Delay_Forward, GSS.GetSet,   // Print Start Delay Forward
				new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Print_Start_Delay_Reverse, GSS.GetSet,   // Print Start Delay Reverse
				new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Product_Speed_Matching, GSS.GetSet,      // Product Speed Matching
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ProductSpeedMatching)),
         new AttrDataII<ccPS>(ccPS.Pulse_Rate_Division_Factor, GSS.GetSet,  // Pulse Rate Division Factor
				new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Speed_Compensation, GSS.GetSet,          // Speed Compensation
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Line_Speed, GSS.GetSet,                  // Line Speed
				new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Distance_Between_Print_Head_And_Object, GSS.GetSet, // Distance Between Print Head And Object
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Print_Target_Width, GSS.GetSet,          // Print Target Width
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Actual_Print_Width, GSS.GetSet,          // Actual Print Width
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Repeat_Count, GSS.GetSet,                // Repeat Count
				new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Repeat_Interval, GSS.GetSet,             // Repeat Interval
				new Prop(3, DataFormats.Decimal, 0, 99999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Target_Sensor_Timer, GSS.GetSet,         // Target Sensor Timer
				new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Target_Sensor_Filter, GSS.GetSet,        // Target Sensor Filter
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.TargetSensorFilter)),
         new AttrDataII<ccPS>(ccPS.Targer_Sensor_Filter_Value, GSS.GetSet,  // Targer Sensor Filter Value
				new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Ink_Drop_Charge_Rule, GSS.GetSet,        // Ink Drop Charge Rule
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),
         new AttrDataII<ccPS>(ccPS.Print_Start_Position_Adjustment_Value, GSS.GetSet, // Print Start Position Adjustment Value
				new Prop(2, DataFormats.Decimal, -50, 50, fmtDD.None)),
      };

      // Calendar (Class Code 0x69)
      private static AttrDataII<ccCal>[] ccCalII = new AttrDataII<ccCal>[] {
         new AttrDataII<ccCal>(ccCal.Shift_Count_Condition, GSS.Get,        // Shift Count Condition
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.First_Calendar_Block_Number, GSS.Get,  // First Calendar Block Number
				new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Calendar_Block_Number_In_Item, GSS.Get, // Calendar Block Number In Item
				new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Offset_Year, GSS.GetSet,               // Offset Year
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Offset_Month, GSS.GetSet,              // Offset Month
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Offset_Day, GSS.GetSet,                // Offset Day
				new Prop(2, DataFormats.Decimal, 0, 1999, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Offset_Hour, GSS.GetSet,               // Offset Hour
				new Prop(2, DataFormats.Decimal, -23, 99, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Offset_Minute, GSS.GetSet,             // Offset Minute
				new Prop(2, DataFormats.Decimal, -59, 99, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Zero_Suppress_Year, GSS.GetSet,        // Zero Suppress Year
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.EnableDisable)),
         new AttrDataII<ccCal>(ccCal.Zero_Suppress_Month, GSS.GetSet,       // Zero Suppress Month
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.EnableDisable)),
         new AttrDataII<ccCal>(ccCal.Zero_Suppress_Day, GSS.GetSet,         // Zero Suppress Day
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.EnableDisable)),
         new AttrDataII<ccCal>(ccCal.Zero_Suppress_Hour, GSS.GetSet,        // Zero Suppress Hour
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.EnableDisable)),
         new AttrDataII<ccCal>(ccCal.Zero_Suppress_Minute, GSS.GetSet,      // Zero Suppress Minute
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.EnableDisable)),
         new AttrDataII<ccCal>(ccCal.Zero_Suppress_Weeks, GSS.GetSet,       // Zero Suppress Weeks
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.EnableDisable)),
         new AttrDataII<ccCal>(ccCal.Zero_Suppress_Day_Of_Week, GSS.GetSet, // Zero Suppress Day Of Week
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.EnableDisable)),
         new AttrDataII<ccCal>(ccCal.Substitute_Rule_Year, GSS.GetSet,      // Substitute Rule Year
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.Ascii)),
         new AttrDataII<ccCal>(ccCal.Substitute_Rule_Month, GSS.GetSet,     // Substitute Rule Month
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.Ascii)),
         new AttrDataII<ccCal>(ccCal.Substitute_Rule_Day, GSS.GetSet,       // Substitute Rule Day
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.Ascii)),
         new AttrDataII<ccCal>(ccCal.Substitute_Rule_Hour, GSS.GetSet,      // Substitute Rule Hour
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.Ascii)),
         new AttrDataII<ccCal>(ccCal.Substitute_Rule_Minute, GSS.GetSet,    // Substitute Rule Minute
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.Ascii)),
         new AttrDataII<ccCal>(ccCal.Substitute_Rule_Weeks, GSS.GetSet,     // Substitute Rule Weeks
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.Ascii)),
         new AttrDataII<ccCal>(ccCal.Substitute_Rule_Day_Of_Week, GSS.GetSet, // Substitute Rule Day Of Week
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.Ascii)),
         new AttrDataII<ccCal>(ccCal.Time_Count_Start_Value, GSS.GetSet,    // Time Count Start Value
				new Prop(3, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Time_Count_End_Value, GSS.GetSet,      // Time Count End Value
				new Prop(3, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Time_Count_Reset_Value, GSS.GetSet,    // Time Count Reset Value
				new Prop(3, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Reset_Time_Value, GSS.GetSet,          // Reset Time Value
				new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Update_Interval_Value, GSS.GetSet,     // Update Interval Value
				new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Shift_Start_Hour, GSS.GetSet,          // Shift Start Hour
				new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Shift_Start_Minute, GSS.GetSet,        // Shift Start Minute
				new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Shift_End_Hour, GSS.GetSet,            // Shift End Hour
				new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Shift_Ene_Minute, GSS.GetSet,          // Shift Ene Minute
				new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None)),
         new AttrDataII<ccCal>(ccCal.Count_String_Value, GSS.GetSet,        // Count String Value
				new Prop(10, DataFormats.ASCII, 0, 0, fmtDD.None)),
      };

      // User_pattern (Class Code 0x6B)
      private static AttrDataII<ccUP>[] ccUPII = new AttrDataII<ccUP>[] {
         new AttrDataII<ccUP>(ccUP.User_Pattern_Fixed, GSS.GetSet,          // User Pattern Fixed
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None), true),
         new AttrDataII<ccUP>(ccUP.User_Pattern_Free, GSS.GetSet,           // User Pattern Free
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None), true),
      };

      // Substitution_rules (Class Code 0x6C)
      private static AttrDataII<ccSR>[] ccSRII = new AttrDataII<ccSR>[] {
         new AttrDataII<ccSR>(ccSR.Number, GSS.GetSet,                      // Number
				new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),
         new AttrDataII<ccSR>(ccSR.Name, GSS.GetSet,                        // Name
				new Prop(1, DataFormats.ASCII, 0, 0, fmtDD.None), true),
         new AttrDataII<ccSR>(ccSR.Start_Year, GSS.GetSet,                  // Start Year
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccSR>(ccSR.Year, GSS.GetSet,                        // Year
				new Prop(3, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccSR>(ccSR.Month, GSS.GetSet,                       // Month
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccSR>(ccSR.Day, GSS.GetSet,                         // Day
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccSR>(ccSR.Hour, GSS.GetSet,                        // Hour
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccSR>(ccSR.Minute, GSS.GetSet,                      // Minute
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccSR>(ccSR.Week, GSS.GetSet,                        // Week
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccSR>(ccSR.Day_Of_Week, GSS.GetSet,                 // Day Of Week
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
      };

      // Enviroment_setting (Class Code 0x71)
      private static AttrDataII<ccES>[] ccESII = new AttrDataII<ccES>[] {
         new AttrDataII<ccES>(ccES.Current_Time, GSS.GetSet,                // Current Time
				new Prop(12, DataFormats.Date, 0, 0, fmtDD.None)),
         new AttrDataII<ccES>(ccES.Calendar_Date_Time, GSS.GetSet,          // Calendar Date Time
				new Prop(12, DataFormats.Date, 0, 0, fmtDD.None)),
         new AttrDataII<ccES>(ccES.Calendar_Date_Time_Availibility, GSS.GetSet, // Calendar Date Time Availibility
				new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.CurrentTime_StopClock)),
         new AttrDataII<ccES>(ccES.Clock_System, GSS.GetSet,                // Clock System
				new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.Hour12_24)),
         new AttrDataII<ccES>(ccES.User_Environment_Information, GSS.Get,   // User Environment Information
				new Prop(16, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccES>(ccES.Cirulation_Control_Setting_Value, GSS.Get, // Cirulation Control Setting Value
				new Prop(12, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccES>(ccES.Usage_Time_Of_Circulation_Control, GSS.Set, // Usage Time Of Circulation Control
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccES>(ccES.Reset_Usage_Time_Of_Citculation_Control, GSS.Set, // Reset Usage Time Of Citculation Control
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),
      };

      // Unit_Information (Class Code 0x73)
      private static AttrDataII<ccUI>[] ccUIII = new AttrDataII<ccUI>[] {
         new AttrDataII<ccUI>(ccUI.Unit_Information, GSS.Get,               // Unit Information
				new Prop(64, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Model_Name, GSS.Get,                     // Model Name
				new Prop(12, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Serial_Number, GSS.Get,                  // Serial Number
				new Prop(8, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Ink_Name, GSS.Get,                       // Ink Name
				new Prop(28, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Input_Mode, GSS.Get,                     // Input Mode
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Maximum_Character_Count, GSS.Get,        // Maximum Character Count
				new Prop(2, DataFormats.Decimal, 240, 1000, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Maximum_Registered_Message_Count, GSS.Get, // Maximum Registered Message Count
				new Prop(2, DataFormats.Decimal, 300, 2000, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Barcode_Information, GSS.Get,            // Barcode Information
				new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Usable_Character_Size, GSS.Get,          // Usable Character Size
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Maximum_Calendar_And_Count, GSS.Get,     // Maximum Calendar And Count
				new Prop(1, DataFormats.Decimal, 3, 8, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Maximum_Substitution_Rule, GSS.Get,      // Maximum Substitution Rule
				new Prop(1, DataFormats.Decimal, 48, 99, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Shift_Code_And_Time_Count, GSS.Get,      // Shift Code And Time Count
				new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Chimney_And_DIN_Print, GSS.Get,          // Chimney And DIN Print
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Maximum_Line_Count, GSS.Get,             // Maximum Line Count
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Basic_Software_Version, GSS.Get,         // Basic Software Version
				new Prop(5, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Controller_Software_Version, GSS.Get,    // Controller Software Version
				new Prop(5, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Engine_M_Software_Version, GSS.Get,      // Engine M Software Version
				new Prop(5, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Engine_S_Software_Version, GSS.Get,      // Engine S Software Version
				new Prop(5, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.First_Language_Version, GSS.Get,         // First Language Version
				new Prop(5, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Second_Language_Version, GSS.Get,        // Second Language Version
				new Prop(5, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccUI>(ccUI.Software_Option_Version, GSS.Get,        // Software Option Version
				new Prop(5, DataFormats.ASCII, 0, 0, fmtDD.None), true),
      };

      // Operation_management (Class Code 0x74)
      private static AttrDataII<ccOM>[] ccOMII = new AttrDataII<ccOM>[] {
         new AttrDataII<ccOM>(ccOM.Operating_Management, GSS.Get,           // Operating Management
				new Prop(2, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Ink_Operating_Time, GSS.GetSet,          // Ink Operating Time
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Alarm_Time, GSS.GetSet,                  // Alarm Time
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Print_Count, GSS.GetSet,                 // Print Count
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Communications_Environment, GSS.Get,     // Communications Environment
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Cumulative_Operation_Time, GSS.Get,      // Cumulative Operation Time
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Ink_And_Makeup_Name, GSS.Get,            // Ink And Makeup Name
				new Prop(2, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Ink_Viscosity, GSS.Get,                  // Ink Viscosity
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Ink_Pressure, GSS.Get,                   // Ink Pressure
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Ambient_Temperature, GSS.Get,            // Ambient Temperature
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Deflection_Voltage, GSS.Get,             // Deflection Voltage
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Excitation_VRef_Setup_Value, GSS.Get,    // Excitation VRef Setup Value
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccOM>(ccOM.Excitation_Frequency, GSS.Get,           // Excitation Frequency
				new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),
      };

      // IJP_operation (Class Code 0x75)
      private static AttrDataII<ccIJP>[] ccIJPII = new AttrDataII<ccIJP>[] {
         new AttrDataII<ccIJP>(ccIJP.Remote_operation_information, GSS.Get, // Remote operation information
				new Prop(1, DataFormats.Bytes, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Fault_and_warning_history, GSS.Get,    // Fault and warning history
				new Prop(6, DataFormats.Bytes, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Operating_condition, GSS.Get,          // Operating condition
				new Prop(1, DataFormats.Bytes, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Warning_condition, GSS.Get,            // Warning condition
				new Prop(1, DataFormats.Bytes, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Date_and_time_information, GSS.Get,    // Date and time information
				new Prop(10, DataFormats.Date, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Error_code, GSS.Get,                   // Error code
				new Prop(1, DataFormats.Bytes, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Start_Remote_Operation, GSS.Service,   // Start Remote Operation
				new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Stop_Remote_Operation, GSS.Service,    // Stop Remote Operation
				new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Deflection_voltage_control, GSS.Service, // Deflection voltage control
				new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None)),
         new AttrDataII<ccIJP>(ccIJP.Online_Offline, GSS.GetSet,            // Online Offline
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OnlineOffline)),
      };

      // Count (Class Code 0x79)
      private static AttrDataII<ccCount>[] ccCountII = new AttrDataII<ccCount>[] {
         new AttrDataII<ccCount>(ccCount.Number_Of_Count_Block, GSS.Get,    // Number Of Count Block
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Initial_Value, GSS.GetSet,         // Initial Value
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Count_Range_1, GSS.GetSet,         // Count Range 1
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Count_Range_2, GSS.GetSet,         // Count Range 2
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Update_Unit_Halfway, GSS.GetSet,   // Update Unit Halfway
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Update_Unit_Unit, GSS.GetSet,      // Update Unit Unit
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Increment_Value, GSS.GetSet,       // Increment Value
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Direction_Value, GSS.GetSet,       // Direction Value
				new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.UpDown)),
         new AttrDataII<ccCount>(ccCount.Jump_From, GSS.GetSet,             // Jump From
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Jump_To, GSS.GetSet,               // Jump To
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Reset_Value, GSS.GetSet,           // Reset Value
				new Prop(0, DataFormats.ASCII, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Type_Of_Reset_Signal, GSS.GetSet,  // Type Of Reset Signal
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None_Signal_1_2)),
         new AttrDataII<ccCount>(ccCount.Availibility_Of_External_Count, GSS.GetSet, // Availibility Of External Count
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.Ascii)),
         new AttrDataII<ccCount>(ccCount.Availibility_Of_Zero_Suppression, GSS.GetSet, // Availibility Of Zero Suppression
				new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.Ascii)),
         new AttrDataII<ccCount>(ccCount.Count_Multiplier, GSS.GetSet,      // Count Multiplier
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),
         new AttrDataII<ccCount>(ccCount.Count_Skip, GSS.GetSet,            // Count Skip
				new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),
      };

      // Index (Class Code 0x7A)
      private static AttrDataII<ccIDX>[] ccIDXII = new AttrDataII<ccIDX>[] {
         new AttrDataII<ccIDX>(ccIDX.Start_Stop_Management_Flag, GSS.GetSet, // Start Stop Management Flag
				new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.Automatic_reflection, GSS.GetSet,      // Automatic reflection
				new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.Item, GSS.GetSet,                      // Item
				new Prop(2, DataFormats.Decimal, 1, 100, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.Column, GSS.GetSet,                    // Column
				new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.Line, GSS.GetSet,                      // Line
				new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.Decimal)),
         new AttrDataII<ccIDX>(ccIDX.Character_position, GSS.GetSet,        // Character position
				new Prop(2, DataFormats.Decimal, 1, 1000, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.Print_Data_Message_Number, GSS.GetSet, // Print Data Message Number
				new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.Print_Data_Group_Data, GSS.GetSet,     // Print Data Group Data
				new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.Substitution_Rules_Setting, GSS.GetSet, // Substitution Rules Setting
				new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.User_Pattern_Size, GSS.GetSet,         // User Pattern Size
				new Prop(1, DataFormats.Decimal, 1, 19, fmtDD.None)),
         new AttrDataII<ccIDX>(ccIDX.Count_Block, GSS.GetSet,               // Count Block
				new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal)),
         new AttrDataII<ccIDX>(ccIDX.Calendar_Block, GSS.GetSet,            // Calendar Block
				new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal)),
      };


   }
}
