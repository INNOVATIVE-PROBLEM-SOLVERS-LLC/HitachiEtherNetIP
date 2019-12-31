using System;
using System.Collections.Generic;
using System.IO;

namespace Modbus_DLL {

   #region EtherNetIP Definitions (Modified for Modbus)

   // Access codes
   public enum AccessCode {
      Set = 0x32,
      Get = 0x33,
      Service = 0x34,
   }

   // Class codes
   public enum ClassCode {
      Print_data_management = 0x66,
      Print_format = 0x67,
      Print_specification = 0x68,
      Calendar = 0x69,
      User_pattern = 0x6B,
      Substitution_rules = 0x6C,
      Enviroment_setting = 0x71,
      Unit_Information = 0x73,
      Operation_management = 0x74,
      IJP_operation = 0x75,
      Count = 0x79,
      Index = 0x7A,
   }

   // Attributes within Print Data Management class 0x66
   public enum ccPDM {
      Select_Message = 0x64,
      Store_Print_Data = 0x65,
      Delete_Print_Data = 0x67,
      Print_Data_Name = 0x69,
      List_of_Messages = 0x6A,
      Print_Data_Number = 0x6B,
      Change_Create_Group_Name = 0x6C,
      Group_Deletion = 0x6D,
      List_of_Groups = 0x6F,
      Change_Group_Number = 0x70,
   }

   // Attributes within Print Format class 0x67
   public enum ccPF {
      Message_Name = 0x64,
      Number_Of_Columns = 0x66,
      Format_Type = 0x67,
      Number_Of_Print_Line_And_Print_Format = 0x1020,
      Insert_Column = 0x1021,
      Delete_Column = 0x1022,
      Add_Column = 0x1023,
      Column = 0x1024,
      Line = 0x1025,
      Format_Setup = 0x103F,
      Adding_Print_Items = 0x6E,
      Deleting_Print_Items = 0x6F,

      Line_Count = 0x1040,
      Line_Spacing = 0x1041,
      Dot_Matrix = 0x1042,
      InterCharacter_Space = 0x1043,
      Character_Bold = 0x1044,
      Barcode_Type = 0x1045,
      Readable_Code = 0x1046,
      Prefix_Code = 0x1047,
      First_Calendar_Block = 0x1048,
      Number_of_Calendar_Blocks = 0x1049,
      First_Count_Block = 0x104A,
      Number_Of_Count_Blocks = 0x104B,

      X_and_Y_Coordinate = 0x104C,
      InterCharacter_SpaceII = 0x7B,
      Add_To_End_Of_String = 0x8A,

      Calendar_Offset = 0x2480,
      DIN_Print = 0x2481,
      EAN_Prefix = 0x2482,
      Barcode_Printing = 0x2483,
      QR_Error_Correction_Level = 0x2084,
   }

   // Attributes within Print Specification class 0x68 (Modbus ready)
   public enum ccPS {
      Character_Height = 0x19A0,
      Ink_Drop_Use = 0x19A1,
      High_Speed_Print = 0x19A2,
      Character_Width = 0x19A3,
      Character_Orientation = 0x19A4,
      Print_Start_Delay_Forward = 0x19A5,
      Print_Start_Delay_Reverse = 0x19A6,
      Product_Speed_Matching = 0x19A7,
      Pulse_Rate_Division_Factor = 0x19A8,
      Speed_Compensation = 0x19A9,
      Line_Speed = 0x19AA,
      Distance_Between_Print_Head_And_Object = 0x19AB,
      Print_Target_Width = 0x19AC,
      Actual_Print_Width = 0x19AD,
      Repeat_Count = 0x19AE,
      Repeat_Interval = 0x19AF,
      Target_Sensor_Timer = 0x19B1,
      Target_Sensor_Filter = 0x19B2,
      Target_Sensor_Filter_Value = 0x19B3,
      Ink_Drop_Charge_Rule = 0x19B4,
      Print_Start_Position_Adjustment_Value = 0x19B5,
   }

   // Attributes within Calendar class 0x69
   public enum ccCal {
      Shift_Code_Condition = 0x65,

      // This block appears only once
      Offset_Year = 0x19C0,
      Offset_Month = 0x19C1,
      Offset_Day = 0x19C2,
      Offset_Hour = 0x19C3,
      Offset_Minute = 0x19C4,
      Zero_Suppress_Year = 0x19C5,
      Zero_Suppress_Month = 0x19C6,
      Zero_Suppress_Day = 0x19C7,
      Zero_Suppress_Hour = 0x19C8,
      Zero_Suppress_Minute = 0x19C9,
      Zero_Suppress_Weeks = 0x19D1,
      Zero_Suppress_DayOfWeek = 0x19D3,
      Substitute_Year = 0x19CA,
      Substitute_Month = 0x19CB,
      Substitute_Day = 0x19CC,
      Substitute_Hour = 0x19CD,
      Substitute_Minute = 0x19CE,
      Substitute_Weeks = 0x19D0,
      Substitute_DayOfWeek = 0x19D2,

      // This block appears only once
      Time_Count_Start_Value = 0x1CD4, // Thru 0x1CD6 - 3 characters
      Time_Count_End_Value = 0x1CD7,   // Thru 0x1CD9 - 3 characters
      Time_Count_Reset_Value = 0x1CDA, // Thru 0x1CDC - 3 characters
      Reset_Time_Value = 0x1CDD,
      Update_Interval_Value = 0x1CDE,

      // This block repeats every 16 bytes for 48 times
      Shift_Start_Hour = 0x1CE0,
      Shift_Start_Minute = 0x1CE1,
      Shift_End_Hour = 0x1CE2,
      Shift_End_Minute = 0x1CE3,
      Shift_String_Value = 0x1CE4,     // Thru 0x1CED - 10 characters
   }

   // Attributes within User Pattern class 0x6B
   public enum ccUP { // 0x6B
      User_Pattern_Fixed = 0x64,
      User_Pattern_Free = 0x65,
   }

   // Attributes within Substitution Rules class 0x6C
   public enum ccSR { // 0x6C
      Number = 0x1AC0,

      Name = 0x65,

      Start_Year = 0x1AC1,
      Year = 0x1AC2,      // Thru 0x1AF3 == 50 locations - 2 char max
      Month = 0x1AF4,     // Thru 0x1B17 == 36 locations - 3 char max
      Day = 0x1B18,       // Thru 0x1B74 == 93 locations - 3 char max
      Hour = 0x1B75,      // Thru 0x1BA4 == 48 locations - 2 char max
      Minute = 0x1BA5,    // Thru 0x1C1C == 120 locations - 2 char max
      Week = 0x1C1D,      // Thru 0x1CBB == 159 locations - 3 char max
      DayOfWeek = 0x1CBC, // Thru 0x1CD0 == 21 locations - 3 char max
   }

   // Attributes within Enviroment Setting class 0x71
   public enum ccES {
      Current_Time = 0x65,
      Calendar_Date_Time = 0x66,
      Calendar_Date_Time_Availibility = 0x67,
      Clock_System = 0x68,
      User_Environment_Information = 0x69,
      Cirulation_Control_Setting_Value = 0x6A,
      Usage_Time_Of_Circulation_Control = 0x6B,
      Reset_Usage_Time_Of_Circulation_Control = 0x6C,
   }

   // Attributes within Unit Information class 0x73
   public enum ccUI {
      Unit_Information = 0x64,
      Model_Name = 0x0010,
      Serial_Number = 0x0020,
      Ink_Name = 0x6D,
      Input_Mode = 0x6E,
      Maximum_Character_Count = 0x6F,
      Maximum_Registered_Message_Count = 0x70,
      Barcode_Information = 0x71,
      Usable_Character_Size = 0x72,
      Maximum_Calendar_And_Count = 0x73,
      Maximum_Substitution_Rule = 0x74,
      Shift_Code_And_Time_Count = 0x75,
      Chimney_And_DIN_Print = 0x76,
      Maximum_Line_Count = 0x77,
      Basic_Software_Version = 0x78,
      Controller_Software_Version = 0x79,
      Engine_M_Software_Version = 0x7A,
      Engine_S_Software_Version = 0x7B,
      First_Language_Version = 0x7C,
      Second_Language_Version = 0x7D,
      Software_Option_Version = 0x7E,
   }

   // Attributes within Operation Management class 0x74
   public enum ccOM {
      Operating_Management = 0x64,
      Ink_Operating_Time = 0x25B0,
      Alarm_Time = 0x25B1,
      Print_Count = 0x25B2,
      Communications_Environment = 0x68,
      Cumulative_Operation_Time = 0x69,
      Ink_And_Makeup_Name = 0x6A,
      Ink_Viscosity = 0x6B,
      Ink_Pressure = 0x6C,
      Ambient_Temperature = 0x6D,
      Deflection_Voltage = 0x6E,
      Excitation_VRef_Setup_Value = 0x6F,
      Excitation_Frequency = 0x70,
   }

   // Attributes within IJP Operation class 0x75
   public enum ccIJP {
      Remote_operation_information = 0x64,
      Fault_and_warning_history = 0x66,
      Operating_condition = 0x67,
      Warning_condition = 0x68,
      Date_and_time_information = 0x6A,
      Error_code = 0x6B,
      Start_Remote_Operation = 0x6C,
      Stop_Remote_Operation = 0x6D,
      Deflection_voltage_control = 0x6E,

      Online_Offline = 0x2490,
   }

   // Attributes within Count class 0x79
   public enum ccCount {

      // This block repeats every 20 bytes for 8 times
      Initial_Value = 0x1FE0,          // Thru 0x1FF3 - 20 Digits
      Count_Range_1 = 0x1FF4,          // Thru 0x2007 - 20 digits
      Count_Range_2 = 0x2008,          // Thru 0x20B1 - 20 digits
      Update_Unit_Halfway = 0x201C,    // Thru 0x201D - 2 words
      Update_Unit_Unit = 0x201E,       // Thru 0x201F - 2 words
      Increment_Value = 0x2020,
      Direction_Value = 0x2021,
      Jump_From = 0x2022,              // Thru 0x2035 - 20 digits
      Jump_To = 0x2036,                // Thru 0x2049 - 20 digits
      Reset_Value = 0x204A,            // Thru 0x205D - 20 digits
      Type_Of_Reset_Signal = 0x205E,
      External_Count = 0x205F,
      Zero_Suppression = 0x2060,
      Count_Multiplier = 0x2061,       // Thru 0x206A - 10 digits
      Count_Skip = 0x206B,             // Thru 0x206F - 5 digits
   }

   // Attributes within Index class 0x7A
   public enum ccIDX {
      Start_Stop_Management_Flag = 0x0000,
      Automatic_reflection = 0x65,
      Item = 0x66,
      Characters_per_Item = 0x0020,
      Number_Of_Items = 0x0008,
      Message_Number = 0x0010,
      Group_Number = 0x6B,
      Substitution_Rule = 0x0012,
      User_Pattern_Size = 0x0013,
      //Count_Block = 0x6E,
      //Calendar_Block = 0x6F,
      Print_Character_String = 0x0084,
   }

   #endregion


   #region Public enumerations

   // Get, Set, or Service
   public enum GSS {
      Get,
      Set,
      GetSet,
      Service,
   }

   public enum mem {
      BigEndian,
      LittleEndian
   }

   // Dropdowns to be used in the display when discrete values are returned.
   public enum fmtDD {
      None = -1,
      Decimal = 0,
      EnableDisable = 1,
      DisableSpaceChar = 2,
      Hour12_24 = 3,
      CurrentTime_StopClock = 4,
      OnlineOffline = 5,
      None_Signal_1_2 = 6,
      UpDown = 7,
      ReadableCode = 8,
      BarcodeType = 9,
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
      Messagelayout = 20,
      ChargeRule = 21,
      TimeCount = 22,
      OffOn = 23,
      EANRule = 24,
      AttributedCharacters = 25,
   }

   public enum DataFormats {
      None = -1,      // No formating
      Decimal = 0,    // Unsigned Decimal numbers up to 8 digits (Big Endian)
      SDecimal = 2,   // Signed Decimal numbers up to 8 digits (Big Endian)
      UTF8 = 4,       // UTF8 characters followed by a Null character
      UTF8N = 5,      // UTF8 characters without the null character
      Date = 6,       // YYYY MM DD HH MM SS 6 2-byte values in Little Endian format
      Bytes = 7,      // Raw data in 2-digit hex notation
      XY = 8,         // x = 2 bytes, y = 1 byte
      N2N2 = 9,       // 2 2-byte numbers
      N2Char = 10,    // 2-byte number + UTF8 String + 0x00
      ItemChar = 11,  // 1-byte item number + UTF8 String + 0x00
      Item = 12,      // 1-byte item number
      GroupChar = 13, // 1-byte group number + UTF8 String + 0x00
      MsgChar = 14,   // 2-byte message number + UTF8 String + 0x00
      N1Char = 15,    // 1-byte number + UTF8 String + 0x00
      N1N1 = 16,      // 2 1-byte numbers
      N1N2N1 = 17,    // 1-byte, 2-byte, 1-byte
      AttrText = 18,  // 4-bytes per character attributed Text
   }

   #endregion

   // Completely describe the Hitachi Model 161 data
   public class Data {

      public Data() {
         ClassCodeAttrData = new AttrData[][] {
            ccPDM_Addrs,           // 0x66 Print data management function
            ccPF_Addrs,            // 0x67 Print format function
            ccPS_Addrs,            // 0x68 Print specification function
            ccCal_Addrs,           // 0x69 Calendar function
            ccUP_Addrs,            // 0x6B User pattern function
            ccSR_Addrs,            // 0x6C Substitution rules function
            ccES_Addrs,            // 0x71 Enviroment setting function
            ccUI_Addrs,            // 0x73 Unit Information function
            ccOM_Addrs,            // 0x74 Operation management function
            ccIJP_Addrs,           // 0x75 IJP operation function
            ccCount_Addrs,         // 0x79 Count function
            ccIDX_Addrs,           // 0x7A Index function

         };
      }

      #region Data Tables

      // Print_data_management (Class Code 0x66)
      private AttrData[] ccPDM_Addrs = new AttrData[] {
         new AttrData((int)ccPDM.Select_Message, true, 1, 0,                    // Select Message 0x64
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccPDM.Store_Print_Data, true, 1, 0,                  // Store Print Data 0x65
            new Prop(15, DataFormats.UTF8, 0, 14, fmtDD.None)),                 //   Data
         new AttrData((int)ccPDM.Delete_Print_Data, true, 1, 0,                 // Delete Print Data 0x67
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Data
         new AttrData((int)ccPDM.Print_Data_Name, true, 1, 0,                   // Print Data Name 0x69
            new Prop(10, DataFormats.UTF8, 0, 14, fmtDD.None)),                 //   Data
         new AttrData((int)ccPDM.List_of_Messages, true, 1, 0,                  // List of Messages 0x6A
            new Prop(2, DataFormats.Decimal, 0, 2000, fmtDD.None)),             //   Data
         new AttrData((int)ccPDM.Print_Data_Number, true, 1, 0,                 // Print Data Number 0x6B
            new Prop(4, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Data
         new AttrData((int)ccPDM.Change_Create_Group_Name, true, 1, 0,          // Change Create Group Name 0x6C
            new Prop(14, DataFormats.UTF8, 0, 14, fmtDD.None)),                 //   Data
         new AttrData((int)ccPDM.Group_Deletion, true, 1, 0,                    // Group Deletion 0x6D
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPDM.List_of_Groups, true, 1, 0,                    // List of Groups 0x6F
            new Prop(500, DataFormats.Bytes, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPDM.Change_Group_Number, true, 1, 0,               // Change Group Number 0x70
            new Prop(2, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Data
      };

      // Print_format (Class Code 0x67)
      private AttrData[] ccPF_Addrs = new AttrData[] {
         new AttrData((int)ccPF.Message_Name, true, 100, 24,                    // Message Name 0x64
            new Prop(14, DataFormats.UTF8, 0, 14, fmtDD.None)),                 //   Data
         new AttrData((int)ccPF.Number_Of_Columns, true, 100, 24,               // Number Of Columns 0x66
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Data
         new AttrData((int)ccPF.Format_Type, true, 100, 24,                     // Format Type 0x67
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout)),       //   Data
         new AttrData((int)ccPF.Insert_Column, true, 100, 24,                   // Insert Column 0x69
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPF.Delete_Column, true, 100, 24,                   // Delete Column 0x6A
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPF.Add_Column, true, 100, 24,                      // Add Column 0x6B
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.Number_Of_Print_Line_And_Print_Format, true, 100, 24, // Number Of Print Line And Print Format 0x6C
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.Format_Setup, true, 1, 0,                       // Format Setup 0x6D
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout)),       //   Data
         new AttrData((int)ccPF.Adding_Print_Items, true, 100, 24,              // Adding Print Items 0x6E
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.Deleting_Print_Items, true, 100, 24,            // Deleting Print Items 0x6F
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Data
         new AttrData((int)ccPF.InterCharacter_SpaceII, true, 100, 24,          // InterCharacter SpaceII 0x7B
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPF.Add_To_End_Of_String, true, 100, 24,            // Add To End Of String 0x8A
            new Prop(750, DataFormats.UTF8, 0, 0, fmtDD.None)),                 //   Data
         new AttrData((int)ccPF.Column, true, 1, 0,                             // Column 0x67
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPF.Line, true, 1, 0,                               // Line 0x68
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.Decimal)),             //   Data
         new AttrData((int)ccPF.Line_Count, true, 100, 24,                      // Line Count 0x1040
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.Line_Spacing, true, 100, 24,                    // Line Spacing 0x1041
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.Dot_Matrix, true, 100, 24,                      // Dot Matrix 0x1042
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.FontType)),           //   Data
         new AttrData((int)ccPF.InterCharacter_Space, true, 100, 24,            // InterCharacter Space 0x1043
            new Prop(1, DataFormats.Decimal, 0, 26, fmtDD.None)),               //   Data
         new AttrData((int)ccPF.Character_Bold, true, 100, 24,                  // Character Bold 0x1044
            new Prop(1, DataFormats.Decimal, 1, 9, fmtDD.Decimal)),             //   Data
         new AttrData((int)ccPF.Barcode_Type, true, 100, 24,                    // Barcode Type 0x1045
            new Prop(1, DataFormats.Decimal, 0, 27, fmtDD.BarcodeType)),        //   Data
         new AttrData((int)ccPF.Readable_Code, true, 100, 24,                   // Readable Code 0x1046
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ReadableCode)),        //   Data
         new AttrData((int)ccPF.Prefix_Code, true, 100, 24,                     // Prefix Code 0x1047
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPF.First_Calendar_Block, true, 8, 24,              // First Calendar Block 0x1048
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.Number_of_Calendar_Blocks, true, 8, 24,         // Number of Calendar Blocks 0x1049
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.First_Count_Block, true, 8, 24,              // First Count Block 0x104A
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.Number_Of_Count_Blocks, true, 8, 24,         // Number Of Count Blocks 0x104B
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Data
         new AttrData((int)ccPF.X_and_Y_Coordinate, true, 100, 24,              // X and Y Coordinate 0x104C
            new Prop(3, DataFormats.XY, 0, 0, fmtDD.None)),                     //   Data
         new AttrData((int)ccPF.QR_Error_Correction_Level, true, 100, 24,       // QR Error Correction Level 0x2084
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.M15Q25)),              //   Data
         new AttrData((int)ccPF.Calendar_Offset, true, 100, 24,                 // Calendar Offset 0x2480
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.YesterdayToday)),      //   Data
         new AttrData((int)ccPF.DIN_Print, true, 100, 24,                       // DIN Print 0x2481
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.DisableSpaceChar)),    //   Data
         new AttrData((int)ccPF.EAN_Prefix, true, 100, 24,                      // EAN Prefix 0x2482
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EditPrint)),           //   Data
         new AttrData((int)ccPF.Barcode_Printing, true, 100, 24,                // Barcode Printing 0x2483
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.NormalReverse)),       //   Data
      };

      // Print_specification (Class Code 0x68)
      private AttrData[] ccPS_Addrs = new AttrData[] {
         new AttrData((int)ccPS.Character_Height, true, 1, 0,                   // Character Height 0x19A0
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPS.Ink_Drop_Use, true, 1, 0,                       // Ink Drop Use 0x19A1
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.None)),               //   Data
         new AttrData((int)ccPS.High_Speed_Print, true, 1, 0,                   // High Speed Print 0x19A2
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.HighSpeedPrint)),      //   Data
         new AttrData((int)ccPS.Character_Width, true, 1, 0,                    // Character Width 0x19A3
            new Prop(2, DataFormats.Decimal, 0, 3999, fmtDD.None)),             //   Data
         new AttrData((int)ccPS.Character_Orientation, true, 1, 0,              // Character Orientation 0x19A4
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.Orientation)),         //   Data
         new AttrData((int)ccPS.Print_Start_Delay_Forward, true, 1, 0,          // Print Start Delay Forward 0x19A5
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Data
         new AttrData((int)ccPS.Print_Start_Delay_Reverse, true, 1, 0,          // Print Start Delay Reverse 0x19A6
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Data
         new AttrData((int)ccPS.Product_Speed_Matching, true, 1, 0,             // Product Speed Matching 0x19A7
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ProductSpeedMatching)), //   Data
         new AttrData((int)ccPS.Pulse_Rate_Division_Factor, true, 1, 0,         // Pulse Rate Division Factor 0x19A8
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None)),              //   Data
         new AttrData((int)ccPS.Speed_Compensation, true, 1, 0,                 // Speed Compensation 0x19A9
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccPS.Line_Speed, true, 1, 0,                         // Line Speed 0x19AA
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Data
         new AttrData((int)ccPS.Distance_Between_Print_Head_And_Object, true, 1, 0, // Distance Between Print Head And Object 0x19AB
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPS.Print_Target_Width, true, 1, 0,                 // Print Target Width 0x19AC
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPS.Actual_Print_Width, true, 1, 0,                 // Actual Print Width 0x19AD
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccPS.Repeat_Count, true, 1, 0,                       // Repeat Count 0x19AE
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Data
         new AttrData((int)ccPS.Repeat_Interval, true, 1, 0,                    // Repeat Interval 0x19AF
            new Prop(3, DataFormats.Decimal, 0, 99999, fmtDD.None)),            //   Data
         new AttrData((int)ccPS.Target_Sensor_Timer, true, 1, 0,                // Target Sensor Timer 0x19B1
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None)),              //   Data
         new AttrData((int)ccPS.Target_Sensor_Filter, true, 1, 0,               // Target Sensor Filter 0x19B2
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.TargetSensorFilter)),  //   Data
         new AttrData((int)ccPS.Target_Sensor_Filter_Value, true, 1, 0,         // Target Sensor Filter Value 0x19B3
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Data
         new AttrData((int)ccPS.Ink_Drop_Charge_Rule, true, 1, 0,               // Ink Drop Charge Rule 0x19B4
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ChargeRule)),          //   Data
         new AttrData((int)ccPS.Print_Start_Position_Adjustment_Value, true, 1, 0, // Print Start Position Adjustment Value 0x19B5
            new Prop(2, DataFormats.Decimal, -50, 50, fmtDD.None)),             //   Data
      };

      // Calendar (Class Code 0x69)
      private AttrData[] ccCal_Addrs = new AttrData[] {
         new AttrData((int)ccCal.Shift_Code_Condition, true, 8, 32,             // Shift Code Condition 0x65
            new Prop(1, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccCal.Offset_Year, true, 8, 32,                      // Offset Year 0x19C0
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccCal.Offset_Month, true, 8, 32,                     // Offset Month 0x19C1
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccCal.Offset_Day, true, 8, 32,                       // Offset Day 0x19C2
            new Prop(2, DataFormats.Decimal, 0, 1999, fmtDD.None)),             //   Data
         new AttrData((int)ccCal.Offset_Hour, true, 8, 32,                      // Offset Hour 0x19C3
            new Prop(2, DataFormats.SDecimal, -23, 99, fmtDD.None)),            //   Data
         new AttrData((int)ccCal.Offset_Minute, true, 8, 32,                    // Offset Minute 0x19C4
            new Prop(2, DataFormats.SDecimal, -59, 99, fmtDD.None)),            //   Data
         new AttrData((int)ccCal.Zero_Suppress_Year, true, 8, 32,               // Zero Suppress Year 0x19C5
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Data
         new AttrData((int)ccCal.Zero_Suppress_Month, true, 8, 32,              // Zero Suppress Month 0x19C6
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Data
         new AttrData((int)ccCal.Zero_Suppress_Day, true, 8, 32,                // Zero Suppress Day 0x19C7
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Data
         new AttrData((int)ccCal.Zero_Suppress_Hour, true, 8, 32,               // Zero Suppress Hour 0x19C8
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Data
         new AttrData((int)ccCal.Zero_Suppress_Minute, true, 8, 32,             // Zero Suppress Minute 0x19C9
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Data
         new AttrData((int)ccCal.Substitute_Year, true, 8, 32,                  // Substitute Year 0x19CA
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCal.Substitute_Month, true, 8, 32,                 // Substitute Month 0x19CB
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCal.Substitute_Day, true, 8, 32,                   // Substitute Day 0x19CC
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCal.Substitute_Hour, true, 8, 32,                  // Substitute Hour 0x19CD
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCal.Substitute_Minute, true, 8, 32,                // Substitute Minute 0x19CE
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCal.Substitute_Weeks, true, 8, 32,                 // Substitute Weeks 0x19D0
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCal.Zero_Suppress_Weeks, true, 8, 32,              // Zero Suppress Weeks 0x19D1
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Data
         new AttrData((int)ccCal.Substitute_DayOfWeek, true, 8, 32,             // Substitute DayOfWeek 0x19D2
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCal.Zero_Suppress_DayOfWeek, true, 8, 32,          // Zero Suppress DayOfWeek 0x19D3
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Data
         new AttrData((int)ccCal.Time_Count_Start_Value, true, 1, 0,            // Time Count Start Value 0x1CD4
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccCal.Time_Count_End_Value, true, 1, 0,              // Time Count End Value 0x1CD7
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccCal.Time_Count_Reset_Value, true, 1, 0,            // Time Count Reset Value 0x1CDA
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccCal.Reset_Time_Value, true, 1, 0,                  // Reset Time Value 0x1CDD
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Data
         new AttrData((int)ccCal.Update_Interval_Value, true, 1, 0,             // Update Interval Value 0x1CDE
            new Prop(1, DataFormats.Decimal, 0, 5, fmtDD.TimeCount)),           //   Data
         new AttrData((int)ccCal.Shift_Start_Hour, true, 48, 16,                // Shift Start Hour 0x1CE0
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Data
         new AttrData((int)ccCal.Shift_Start_Minute, true, 48, 16,              // Shift Start Minute 0x1CE1
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None)),               //   Data
         new AttrData((int)ccCal.Shift_End_Hour, true, 48, 16,                  // Shift End Hour 0x1CE2
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Data
         new AttrData((int)ccCal.Shift_End_Minute, true, 48, 16,                // Shift End Minute 0x1CE3
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None)),               //   Data
         new AttrData((int)ccCal.Shift_String_Value, true, 48, 16,              // Shift String Value 0x1CE4
            new Prop(1, DataFormats.UTF8N, 0, 0, fmtDD.None)),                  //   Data
      };

      // User_pattern (Class Code 0x6B)
      private AttrData[] ccUP_Addrs = new AttrData[] {
         new AttrData((int)ccUP.User_Pattern_Fixed, true, 1, 0,                 // User Pattern Fixed 0x64
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccUP.User_Pattern_Free, true, 1, 0,                  // User Pattern Free 0x65
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
      };

      // Substitution_rules (Class Code 0x6C)
      private AttrData[] ccSR_Addrs = new AttrData[] {
         new AttrData((int)ccSR.Name, true, 1, 0,                               // Name 0x65
            new Prop(13, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccSR.Number, true, 1, 0,                             // Number 0x1AC0
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccSR.Start_Year, true, 1, 0,                         // Start Year 0x1AC1
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccSR.Year, true, 25, 2,                              // Year 0x1AC2
            new Prop(2, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccSR.Month, true, 12, 3,                             // Month 0x1AF4
            new Prop(3, DataFormats.UTF8, 1, 12, fmtDD.None)),                  //   Data
         new AttrData((int)ccSR.Day, true, 31, 3,                               // Day 0x1B18
            new Prop(3, DataFormats.UTF8, 1, 31, fmtDD.None)),                  //   Data
         new AttrData((int)ccSR.Hour, true, 24, 2,                              // Hour 0x1B75
            new Prop(2, DataFormats.UTF8, 0, 23, fmtDD.None)),                  //   Data
         new AttrData((int)ccSR.Minute, true, 60, 2,                            // Minute 0x1BA5
            new Prop(2, DataFormats.UTF8, 0, 59, fmtDD.None)),                  //   Data
         new AttrData((int)ccSR.Week, true, 53, 3,                              // Week 0x1C1D
            new Prop(3, DataFormats.UTF8, 0, 52, fmtDD.None)),                  //   Data
         new AttrData((int)ccSR.DayOfWeek, true, 7, 3,                          // DayOfWeek 0x1CBC
            new Prop(6, DataFormats.UTF8, 1, 7, fmtDD.None)),                   //   Data
      };

      // Enviroment_setting (Class Code 0x71)
      private AttrData[] ccES_Addrs = new AttrData[] {
         new AttrData((int)ccES.Current_Time, true, 1, 0,                       // Current Time 0x65
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccES.Calendar_Date_Time, true, 1, 0,                 // Calendar Date Time 0x66
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccES.Calendar_Date_Time_Availibility, true, 1, 0,    // Calendar Date Time Availibility 0x67
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.CurrentTime_StopClock)), //   Data
         new AttrData((int)ccES.Clock_System, true, 1, 0,                       // Clock System 0x68
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.Hour12_24)),           //   Data
         new AttrData((int)ccES.User_Environment_Information, true, 1, 0,       // User Environment Information 0x69
            new Prop(16, DataFormats.Bytes, 0, 0, fmtDD.None)),                 //   Data
         new AttrData((int)ccES.Cirulation_Control_Setting_Value, true, 1, 0,   // Cirulation Control Setting Value 0x6A
            new Prop(12, DataFormats.Bytes, 0, 0, fmtDD.None)),                 //   Data
         new AttrData((int)ccES.Usage_Time_Of_Circulation_Control, true, 1, 0,  // Usage Time Of Circulation Control 0x6B
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccES.Reset_Usage_Time_Of_Circulation_Control, true, 1, 0, // Reset Usage Time Of Circulation Control 0x6C
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
      };

      // Unit_Information (Class Code 0x73)
      private AttrData[] ccUI_Addrs = new AttrData[] {
         new AttrData((int)ccUI.Unit_Information, true, 1, 0,                   // Unit Information 0x64
            new Prop(64, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccUI.Model_Name, false, 1, 0,                        // Model Name 0x6B
            new Prop(16, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccUI.Serial_Number, false, 1, 0,                     // Serial Number 0x6C
            new Prop(4, DataFormats.Decimal, 0, 99999999, fmtDD.None)),         //   Data
         new AttrData((int)ccUI.Ink_Name, true, 1, 0,                           // Ink Name 0x6D
            new Prop(28, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccUI.Input_Mode, true, 1, 0,                         // Input Mode 0x6E
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccUI.Maximum_Character_Count, true, 1, 0,            // Maximum Character Count 0x6F
            new Prop(2, DataFormats.Decimal, 240, 1000, fmtDD.None)),           //   Data
         new AttrData((int)ccUI.Maximum_Registered_Message_Count, true, 1, 0,   // Maximum Registered Message Count 0x70
            new Prop(2, DataFormats.Decimal, 300, 2000, fmtDD.None)),           //   Data
         new AttrData((int)ccUI.Barcode_Information, true, 1, 0,                // Barcode Information 0x71
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.None)),                //   Data
         new AttrData((int)ccUI.Usable_Character_Size, true, 1, 0,              // Usable Character Size 0x72
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccUI.Maximum_Calendar_And_Count, true, 1, 0,         // Maximum Calendar And Count 0x73
            new Prop(1, DataFormats.Decimal, 3, 8, fmtDD.None)),                //   Data
         new AttrData((int)ccUI.Maximum_Substitution_Rule, true, 1, 0,          // Maximum Substitution Rule 0x74
            new Prop(1, DataFormats.Decimal, 48, 99, fmtDD.None)),              //   Data
         new AttrData((int)ccUI.Shift_Code_And_Time_Count, true, 1, 0,          // Shift Code And Time Count 0x75
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccUI.Chimney_And_DIN_Print, true, 1, 0,              // Chimney And DIN Print 0x76
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccUI.Maximum_Line_Count, true, 1, 0,                 // Maximum Line Count 0x77
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccUI.Basic_Software_Version, true, 1, 0,             // Basic Software Version 0x78
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccUI.Controller_Software_Version, true, 1, 0,        // Controller Software Version 0x79
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccUI.Engine_M_Software_Version, true, 1, 0,          // Engine M Software Version 0x7A
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccUI.Engine_S_Software_Version, true, 1, 0,          // Engine S Software Version 0x7B
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccUI.First_Language_Version, true, 1, 0,             // First Language Version 0x7C
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccUI.Second_Language_Version, true, 1, 0,            // Second Language Version 0x7D
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
         new AttrData((int)ccUI.Software_Option_Version, true, 1, 0,            // Software Option Version 0x7E
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
      };

      // Operation_management (Class Code 0x74)
      private AttrData[] ccOM_Addrs = new AttrData[] {
         new AttrData((int)ccOM.Operating_Management, true, 1, 0,               // Operating Management 0x64
            new Prop(2, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccOM.Communications_Environment, true, 1, 0,         // Communications Environment 0x68
            new Prop(2, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccOM.Cumulative_Operation_Time, true, 1, 0,          // Cumulative Operation Time 0x69
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Ink_And_Makeup_Name, true, 1, 0,                // Ink And Makeup Name 0x6A
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccOM.Ink_Viscosity, true, 1, 0,                      // Ink Viscosity 0x6B
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Ink_Pressure, true, 1, 0,                       // Ink Pressure 0x6C
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Ambient_Temperature, true, 1, 0,                // Ambient Temperature 0x6D
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Deflection_Voltage, true, 1, 0,                 // Deflection Voltage 0x6E
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Excitation_VRef_Setup_Value, true, 1, 0,        // Excitation VRef Setup Value 0x6F
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Excitation_Frequency, true, 1, 0,               // Excitation Frequency 0x70
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Ink_Operating_Time, true, 1, 0,                 // Ink Operating Time 0x25B0
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Alarm_Time, true, 1, 0,                         // Alarm Time 0x25B1
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccOM.Print_Count, true, 1, 0,                        // Print Count 0x25B2
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
      };

      // IJP_operation (Class Code 0x75)
      private AttrData[] ccIJP_Addrs = new AttrData[] {
         new AttrData((int)ccIJP.Remote_operation_information, true, 1, 0,      // Remote operation information 0x64
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccIJP.Fault_and_warning_history, true, 1, 0,         // Fault and warning history 0x66
            new Prop(6, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccIJP.Operating_condition, true, 1, 0,               // Operating condition 0x67
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccIJP.Warning_condition, true, 1, 0,                 // Warning condition 0x68
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccIJP.Date_and_time_information, true, 1, 0,         // Date and time information 0x6A
            new Prop(10, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccIJP.Error_code, true, 1, 0,                        // Error code 0x6B
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccIJP.Start_Remote_Operation, true, 1, 0,            // Start Remote Operation 0x6C
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccIJP.Stop_Remote_Operation, true, 1, 0,             // Stop Remote Operation 0x6D
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccIJP.Deflection_voltage_control, true, 1, 0,        // Deflection voltage control 0x6E
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccIJP.Online_Offline, true, 1, 0,                    // Online Offline 0x2490
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OnlineOffline)),       //   Data
      };

      // Count (Class Code 0x79)
      private AttrData[] ccCount_Addrs = new AttrData[] {
         new AttrData((int)ccCount.Initial_Value, true, 8, 148,                 // Initial Value 0x1FE0
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccCount.Count_Range_1, true, 8, 148,                 // Count Range 1 0x1FF4
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccCount.Count_Range_2, true, 8, 148,                 // Count Range 2 0x2008
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccCount.Update_Unit_Halfway, true, 8, 148,           // Update Unit Halfway 0x201C
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None)),           //   Data
         new AttrData((int)ccCount.Update_Unit_Unit, true, 8, 148,              // Update Unit Unit 0x201E
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None)),           //   Data
         new AttrData((int)ccCount.Increment_Value, true, 8, 148,               // Increment Value 0x2020
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Data
         new AttrData((int)ccCount.Direction_Value, true, 8, 148,               // Direction Value 0x2021
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.UpDown)),              //   Data
         new AttrData((int)ccCount.Jump_From, true, 8, 148,                     // Jump From 0x2022
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccCount.Jump_To, true, 8, 148,                       // Jump To 0x2036
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccCount.Reset_Value, true, 8, 148,                   // Reset Value 0x204A
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccCount.Type_Of_Reset_Signal, true, 8, 148,          // Type Of Reset Signal 0x205E
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None_Signal_1_2)),     //   Data
         new AttrData((int)ccCount.External_Count, true, 8, 148,                // External Count 0x205F
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCount.Zero_Suppression, true, 8, 148,              // Zero Suppression 0x2060
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Data
         new AttrData((int)ccCount.Count_Multiplier, true, 8, 148,              // Count Multiplier 0x2061
            new Prop(10, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Data
         new AttrData((int)ccCount.Count_Skip, true, 8, 148,                    // Count Skip 0x206B
            new Prop(4, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Data
      };

      // Index (Class Code 0x7A)
      private AttrData[] ccIDX_Addrs = new AttrData[] {
         new AttrData((int)ccIDX.Start_Stop_Management_Flag, true, 1, 0,        // Start Stop Management Flag 0x00
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),                //   Data
         new AttrData((int)ccIDX.Number_Of_Items, true, 100, 24,                // Number Of Items 0x08
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Data
         new AttrData((int)ccIDX.Automatic_reflection, true, 1, 0,              // Automatic reflection 0x65
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OffOn)),               //   Data
         new AttrData((int)ccIDX.Item, true, 1, 0,                              // Item 0x66
            new Prop(2, DataFormats.Decimal, 0, 100, fmtDD.None)),              //   Data
         new AttrData((int)ccIDX.Characters_per_Item, true, 1000, 1,            // Character per Item 0x0020
            new Prop(2, DataFormats.Decimal, 0, 1000, fmtDD.None)),             //   Data
         new AttrData((int)ccIDX.Message_Number, true, 1, 0,                    // Message Number 0x6A
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Data
         new AttrData((int)ccIDX.Group_Number, true, 1, 0,                      // Group Number 0x6B
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccIDX.Substitution_Rule, true, 1, 0,                 // Substitution Rule 0x6C
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Data
         new AttrData((int)ccIDX.User_Pattern_Size, true, 1, 0,                 // User Pattern Size 0x6D
            new Prop(1, DataFormats.Decimal, 1, 19, fmtDD.FontType)),           //   Data
          new AttrData((int)ccIDX.Print_Character_String, true, 1000, 2,        // Print Character String 0x84
            new Prop(4, DataFormats.AttrText, 0, 0, fmtDD.None)),               //   Data
        //new AttrData((int)ccIDX.Count_Block, true, 1, 0,                       // Count Block 0x6E
         //   new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal)),             //   Data
         //new AttrData((int)ccIDX.Calendar_Block, true, 1, 0,                    // Calendar Block 0x6F
         //   new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal)),             //   Data
      };

      #endregion

      #region Class Codes => Attributes => Attribute Data lookup tables

      // Class Codes to Data Tables Conversion
      public AttrData[][] ClassCodeAttrData;

      #endregion

      #region Data reformatting routines

      // Reformat the raw data tables in this module to make them easier to read and modify
      public void ReformatTables(StreamWriter RFS) {

         DumpTable(RFS, ccPDM_Addrs, ClassCode.Print_data_management, typeof(ccPDM));
         DumpTable(RFS, ccPF_Addrs, ClassCode.Print_format, typeof(ccPF));
         DumpTable(RFS, ccPS_Addrs, ClassCode.Print_specification, typeof(ccPS));
         DumpTable(RFS, ccCal_Addrs, ClassCode.Calendar, typeof(ccCal));
         DumpTable(RFS, ccUP_Addrs, ClassCode.User_pattern, typeof(ccUP));
         DumpTable(RFS, ccSR_Addrs, ClassCode.Substitution_rules, typeof(ccSR));
         DumpTable(RFS, ccES_Addrs, ClassCode.Enviroment_setting, typeof(ccES));
         DumpTable(RFS, ccUI_Addrs, ClassCode.Unit_Information, typeof(ccUI));
         DumpTable(RFS, ccOM_Addrs, ClassCode.Operation_management, typeof(ccOM));
         DumpTable(RFS, ccIJP_Addrs, ClassCode.IJP_operation, typeof(ccIJP));
         DumpTable(RFS, ccCount_Addrs, ClassCode.Count, typeof(ccCount));
         DumpTable(RFS, ccIDX_Addrs, ClassCode.Index, typeof(ccIDX));

      }

      // Export table to tab delimited
      private void DumpTable(StreamWriter RFS, AttrData[] tbl, ClassCode cc, Type at) {
         // Now process each attribute within the Class
         string[] attrNames = Enum.GetNames(at);
         int[] addrValues = (int[])Enum.GetValues(at);
         for (int i = 0; i < tbl.Length; i++) {
            AttrData attr = Array.Find<AttrData>(tbl, z => z.Val == addrValues[i]);
            string printLine = $"(0x{((int)cc).ToString("X2")}){cc}\t(0x{attr.Val:X4}){attrNames[i]}\t";

            printLine += $"{attr.Count}\t{attr.Stride}\t{attr.HoldingReg}\t";
            Prop x = attr.Data;
            printLine += $"{x.Len}\t{typeof(DataFormats)}.{x.Fmt}\t{x.Min}\t{x.Max}\t{typeof(fmtDD)}.{x.DropDown}";

            RFS.WriteLine(printLine);

         }
         RFS.WriteLine(" ");
      }

      // Process the tables one at a time
      private void DumpTableII(StreamWriter RFS, AttrData[] tbl, ClassCode cc, Type at) {
         string name = at.ToString();
         name = name.Substring(name.IndexOf('.') + 1);

         // Spacing tabs
         string t1 = "      ";
         string t2 = t1 + "   ";
         string t3 = t2 + "   ";

         // Set of properties
         List<Prop> p = new List<Prop>();

         // Write out the table header
         RFS.WriteLine($"{t1}// {cc} (Class Code 0x{((int)cc).ToString("X2")})");
         RFS.WriteLine($"{t1}private AttrData[] {name}_Addrs = new AttrData[] {{");

         // Now process each attribute within the Class
         string[] attrNames = Enum.GetNames(at);
         int[] attrValues = (int[])Enum.GetValues(at);
         for (int i = 0; i < tbl.Length; i++) {
            // Allow for the names to be reordered
            AttrData attr = Array.Find<AttrData>(tbl, x => x.Val == attrValues[i]);
            // Turn Access into an enum
            string access = string.Empty;
            //if (attr.HasGet)
            //   access += "Get";
            //if (attr.HasSet)
            //   access += "Set";
            //if (attr.HasService)
            //   access += "Service";

            // Format Ignore as true/false and Data Format to an enum
            //string ignore = attr.Ignore ? "true" : "false";

            // Space the comment at the end of the line for readability
            string printLine = $"{t2}new AttrData((int){name}.{attrNames[i]}, {attr.HoldingReg}, {attr.Count}, {attr.Stride},";
            string spaces = new string(' ', Math.Max(80 - printLine.Length, 1));
            RFS.WriteLine($"{printLine}{spaces}// {attrNames[i].Replace("_", " ")} 0x{attr.Val:X2}");

            // See how many properties are needed
            string[] s = null;
            p.Clear();
            p.Add(attr.Data);
            s = new string[] { "Data" };

            // Format and output the properties
            for (int n = 0; n < p.Count; n++) {
               printLine = $"{t3}new Prop({p[n].Len}, DataFormats.{p[n].Fmt}, {p[n].Min}, {p[n].Max}, fmtDD.{(fmtDD)p[n].DropDown}";
               if (n == p.Count - 1) {
                  printLine += ")),";
               } else {
                  printLine += "),";
               }
               spaces = new string(' ', Math.Max(80 - printLine.Length, 1));
               RFS.WriteLine($"{printLine}{spaces}//   {s[n]}");
            }
         }
         // Terminate the Attribute table
         RFS.WriteLine($"{t1}}};");
         RFS.WriteLine();
      }

      #endregion

      // Attribute DropDown conversion (EtherNet/IP Names)
      static public string[][] DropDowns = new string[][] {
         new string[] { },                                            // 0 - Just decimal values
         new string[] { "Disable", "Enable" },                        // 1 - Enable and disable
         new string[] { "Disable", "Space Fill", "Character Fill" },  // 2 - Disable, space fill, character fill
         new string[] { "TwentyFour Hour", "Twelve Hour" },           // 3 - 12 & 24 hour
         new string[] { "Current Time", "Stop Clock" },               // 4 - Current time or stop clock
         new string[] { "Off Line", "On Line" },                      // 5 - Offline/Online
         new string[] { "None", "Signal 1", "Signal 2" },             // 6 - None, Signal 1, Signal 2
         new string[] { "Up", "Down" },                               // 7 - Up/Down
         new string[] { "None", "5X5", "5X7" },                       // 8 - Readable Code 5X5 or 5X7
         new string[] { "None", "code 39", "ITF", "NW-7", "EAN-13", "DM8x32", "DM16x16", "DM16x36",
                        "DM16x48", "DM18x18", "DM20x20", "DM22x22", "DM24x24", "Code 128 (Code set B)",
                        "Code 128 (Code set C)", "UPC-A", "UPC-E", "EAN-8", "QR21x21", "QR25x25",
                        "QR29x29", "QR33x33", "EAN-13add-on5", "Micro QR (15 x 15)",
                        "GS1 DataBar (Limited)", "GS1 DataBar (Omnidirectional)",
                        "GS1 DataBar (Stacked)", "DM14x14", },        // 9 - BarCode Types
         new string[] { "Normal", "Reverse" },                        // 10 - Normal/reverse
         new string[] { "M 15%", "Q 25%" },                           // 11 - M 15%, Q 25%
         new string[] { "Edit Message", "Print Format" },             // 12 - Edit/Print
         new string[] { "From Yesterday", "From Today" },             // 13 - From Yesterday/Today
         new string[] { "4x5", "5x5", "5x8(5x7)", "9x8(9x7)", "7x10", "10x12", "12x16", "18x24", "24x32",
                        "11x11", "QR33", "30x40", "36x48", "5x3(Chimney)", "5x5(Chimney)", "7x5(Chimney)"  },
                                                                      // 14 - Font Types
         new string[] { "Normal/Forward", "Normal/Reverse",
                        "Inverted/Forward", "Inverted/Reverse",},     // 15 - Orientation
         new string[] { "None", "Encoder", "Auto" },                  // 16 - Product speed matching
         new string[] { "HM", "NM", "QM", "SM" },                     // 17 - High Speed Print
         new string[] { "Time Setup", "Until End of Print" },         // 18 - Target Sensor Filter
         new string[] { "4x5", "5x5", "5x8(5x7)", "9x8(9x7)", "7x10", "10x12", "12x16", "18x24", "24x32",
                        "11x11", "5x3(Chimney)", "5x5(Chimney)", "7x5(Chimney)", "30x40", "36x48"  },
                                                                      // 19 - User Pattern Font Types
         new string[] { "Individual", "Overall", "Free Layout" },     // 20 - Message Layout
         new string[] { "Standard", "Mixed", "Dot Mixed" },           // 21 - Charge Rule
         new string[] { "5 Minutes", "6 Minutes", "10 Minutes", "15 Minutes", "20 Minutes", "30 Minutes" },
                                                                      // 22 - Time Count renewal period
         new string[] { "Off", "On" },                                // 23 - On/Off for Auto Reflection
         new string[] { "CharacterInput", "MessageFormat" },          // 24 - EAN Prefix
         //new string[] { "CharacterInput", "MessageFormat" },          // 25 - Attributed Data
     };

      // Attribute DropDown conversion (IJPLib Names)
      static public string[][] DropDownsIJPLib = new string[][] {
         new string[] { },                                            // 0 - Just decimal values
         new string[] { "False", "True" },                            // 1 - Enable and disable
         new string[] { "None", "Space", "CharacterFill" },           // 2 - Disable, space fill, character fill
         new string[] { "TwentyFour Hour", "Twelve Hour" },           // 3 - 12 & 24 hour
         new string[] { "Current Time", "Stop Clock" },               // 4 - Current time or stop clock
         new string[] { "Offline", "Online" },                        // 5 - Offline/Online
         new string[] { "Nothing", "Signal1", "Signal2" },            // 6 - None, Signal 1, Signal 2
         new string[] { "Up", "Down" },                               // 7 - Up/Down
         new string[] { "Nothing", "Size5x5", "Size5x7" },                       // 8 - Readable Code 5X5 or 5X7
         new string[] { "Nothing", "Code39", "ITF", "NW7", "JAN_13", "DM8x32", "DM16x16", "DM16x36",
                        "DM16x48", "DM18x18", "DM20x20", "DM22x22", "DM24x24", "Code128_CodesetB",
                        "Code128_CodesetC", "UPC_A", "UPC_E", "JAN_8", "QR21x21", "QR25x25",
                        "QR29x29", "QR33x33", "JAN_13add_on5", "MicroQR15x15",
                        "GS1_Lim", "GS1_Omn",
                        "GS1 DataBar (Stacked)", "DM14x14", },        // 9 - BarCode Types
         new string[] { "Normal", "Reverse" },                        // 10 - Normal/reverse
         new string[] { "M", "Q" },                                   // 11 - M 15%, Q 25%
         new string[] { "Edit Message", "Print Format" },             // 12 - Edit/Print
         new string[] { "OneDayAgo", "None" },                        // 13 - From Yesterday/Today
         new string[] { "Size4x5", "Size5x5", "Size5x7", "Size9x7", "Size7x10", "Size10x12", "Size12x16", "Size18x24",
                        "Size24x32", "Size11x11", "Size5x3_Chimney", "Size5x5_Chimney", "Size7x5_Chimney"  },
                                                                      // 14 - Font Types
         new string[] { "Normal_Forward", "Normal_Backward",
                        "Reverse_Forward", "Reverse_Forward",},       // 15 - Orientation
         new string[] { "Off", "On", "Auto" },                        // 16 - Product speed matching
         new string[] { "HM", "NM", "QM", "SM" },                     // 17 - High Speed Print
         new string[] { "Time", "Complete" },                         // 18 - Target Sensor Filter
         new string[] { "Size4x5", "Size5x5", "Size5x7", "Size9x7", "Size7x10", "Size10x12", "Size12x16", "Size18x24",
                        "Size24x32", "Size11x11", "Size5x3_Chimney", "Size5x5_Chimney", "Size7x5_Chimney"  },
                                                                      // 19 - User Pattern Font Types
         new string[] { "SeparateSetup", "CollectiveSetup", "FreeLayout" },     // 20 - Message Layout
         new string[] { "Normal", "DotMixed", "Mixed" },              // 21 - Charge Rule
         new string[] { "FiveMinutes", "SixMinutes", "TenMinutes", "QuarterHour", "TwentyMinutes", "HalfHour" },
                                                                      // 22 - Time Count renewal period
         new string[] { "Off", "On" },                                // 23 - On/Off for Auto Reflection
         new string[] { "CharacterInput", "MessageFormat" },          // 24 - EAN Prefix
         //new string[] { "CharacterInput", "MessageFormat" },          // 25 - Attributed Data
     };

   }

   public class AttrData {

      #region Properties and Constructor

      public ClassCode Class { get; set; }           // The class code is set when the dictionary is built
      public int Val { get; set; } = 0;              // The Attribute (Makes the tables easier to read)
      //public bool HasSet { get; set; } = false;      // Supports a Set Request
      //public bool HasGet { get; set; } = false;      // Supports a Get Request
      //public bool HasService { get; set; } = false;  // Supports a Service Request
      //public int Order { get; set; } = 0;            // Sort Order if Alpha Sort is requested
      //public bool Ignore { get; set; } = false;      // Indicates that the request will hang printer
      public int Count { get; set; } = 1;            // Indicates max number of repetitions
      public int Stride { get; set; } = 0;           // Indicates the distance between repetitions
      public bool HoldingReg { get; set; } = true;   // Input vs Holding register

      // Four views of the printer data
      public Prop Data { get; set; }     // As it appears in the printer
      //public Prop Get { get; set; }      // Data to be passed on a Get Request
      //public Prop Set { get; set; }      // Data to be passed on a Set Request
      //public Prop Service { get; set; }  // Data to be passed on a Service Request

      // A description of the data from four points of view.
      public AttrData(int Val, bool HoldingReg, int Count, int Stride, Prop Data, Prop Data2 = null, Prop Data3 = null) {
         this.Val = Val;
         this.HoldingReg = HoldingReg;
         this.Count = Count;
         this.Stride = Stride;

         // This is what the data looks like in the printer
         this.Data = Data;
         //if (this.HasService) {
         //   this.Service = Data2;
         //} else {
         //   this.Get = Data2;
         //   this.Set = Data3;
         //}
      }

      public AttrData Clone() {
         return (AttrData)this.MemberwiseClone();
      }

      #endregion

   }

   public class Prop {

      #region Constructors, properties and methods

      public int Len { get; set; }
      public DataFormats Fmt { get; set; }
      public int Min { get; set; }
      public int Max { get; set; }
      public fmtDD DropDown { get; set; }

      public Prop(int Len, DataFormats Fmt, int Min, int Max, fmtDD DropDown = fmtDD.None) {
         this.Len = Len;
         this.Fmt = Fmt;
         this.Min = Min;
         this.Max = Max;
         this.DropDown = DropDown;
      }

      #endregion

   }

   // Look up AttrData using Class and Attribute
   public class Dictionary<TKey1, TKey2, TValue>
      : Dictionary<Tuple<TKey1, TKey2>, TValue>, IDictionary<Tuple<TKey1, TKey2>, TValue> {

      #region Constructor and methods

      // Convert the Class and Attribute to a Tuple for get/set
      public TValue this[TKey1 key1, TKey2 key2] {
         get { return base[Tuple.Create(key1, key2)]; }
      }

      // Convert the Class and Attribute to a Tuple for Add
      public void Add(TKey1 key1, TKey2 key2, TValue value) {
         base.Add(Tuple.Create(key1, key2), value);
      }

      #endregion

   }

}


