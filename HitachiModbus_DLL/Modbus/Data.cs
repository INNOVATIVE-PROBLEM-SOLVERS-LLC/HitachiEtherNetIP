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
      Number_Of_Items = 0x0008,
      Number_Of_Columns = 0x66,
      Format_Type = 0x67,
      Insert_Column = 0x69,
      Delete_Column = 0x6A,
      Add_Column = 0x6B,
      Number_Of_Print_Line_And_Print_Format = 0x6C,
      Format_Setup = 0x6D,
      Adding_Print_Items = 0x6E,
      Deleting_Print_Items = 0x6F,
      Print_Character_String = 0x0084,

      Line_Count = 0x1040,
      Line_Spacing = 0x1041,
      Dot_Matrix = 0x1042,
      InterCharacter_Space = 0x1043,
      Character_Bold = 0x1044,
      Barcode_Type = 0x1045,
      Readable_Code = 0x1046,
      Prefix_Code = 0x1047,

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
      First_Calendar_Block = 0x1048,
      Number_of_Calendar_Blocks = 0x1049,

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
      Year = 0x1AC2,      // Thru 0x1AF3 == 50 locations - 2 Char max
      Month = 0x1AF4,     // Thru 0x1B17 == 36 locations
      Day = 0x1B18,       // Thru 0x1B74 == 93 locations
      Hour = 0x1B75,      // Thru 0x1BA4 == 48 locations
      Minute = 0x1BA5,    // Thru 0x1C1C == 120 locations
      Week = 0x1C1D,      // Thru 0x1CBB == 159 locations
      DayOfWeek = 0x1CBC, // Thru 0x1CD0 == 21 locations
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
      Model_Name = 0x6B,
      Serial_Number = 0x6C,
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
      First_Count_Block = 0x104A,
      Number_Of_Count_Blocks = 0x104B,

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
      Column = 0x67,
      Line = 0x68,
      Character_position = 0x69,
      Message_Number = 0x6A,
      Group_Number = 0x6B,
      Substitution_Rule = 0x6C,
      User_Pattern_Size = 0x6D,
      Count_Block = 0x6E,
      Calendar_Block = 0x6F,
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
      DecimalLE = 1,  // Unsigned Decimal numbers up to 8 digits (Little Endian)
      SDecimal = 2,   // Signed Decimal numbers up to 8 digits (Big Endian)
      SDecimalLE = 3, // Signed Decimal numbers up to 8 digits (Little Endian)
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
      private readonly AttrData[] ccPDM_Addrs = new AttrData[] {
         new AttrData((int)ccPDM.Select_Message, GSS.Service, false, 9,        // Select Message 0x64
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Service
         new AttrData((int)ccPDM.Store_Print_Data, GSS.Set, false, 10,         // Store Print Data 0x65
            new Prop(15, DataFormats.UTF8, 0, 14, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(15, DataFormats.N2Char, 0, 14, fmtDD.None)),               //   Set
         new AttrData((int)ccPDM.Delete_Print_Data, GSS.Set, false, 3,         // Delete Print Data 0x67
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Set
         new AttrData((int)ccPDM.Print_Data_Name, GSS.Set, false, 7,           // Print Data Name 0x69
            new Prop(10, DataFormats.UTF8, 0, 14, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(10, DataFormats.N2Char, 0, 14, fmtDD.None)),               //   Set
         new AttrData((int)ccPDM.List_of_Messages, GSS.Get, true, 6,           // List of Messages 0x6A
            new Prop(2, DataFormats.Decimal, 0, 2000, fmtDD.None),              //   Data
            new Prop(2, DataFormats.Decimal, 0, 2000, fmtDD.None),              //   Get
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccPDM.Print_Data_Number, GSS.Set, false, 8,         // Print Data Number 0x6B
            new Prop(4, DataFormats.Decimal, 1, 2000, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(4, DataFormats.N2N2, 1, 2000, fmtDD.None)),                //   Set
         new AttrData((int)ccPDM.Change_Create_Group_Name, GSS.Set, false, 1,  // Change Create Group Name 0x6C
            new Prop(14, DataFormats.UTF8, 0, 14, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(14, DataFormats.N1Char, 0, 14, fmtDD.None)),               //   Set
         new AttrData((int)ccPDM.Group_Deletion, GSS.Set, false, 4,            // Group Deletion 0x6D
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccPDM.List_of_Groups, GSS.Get, false, 5,            // List of Groups 0x6F
            new Prop(500, DataFormats.Bytes, 0, 99, fmtDD.None),                //   Data
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Get
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccPDM.Change_Group_Number, GSS.Set, false, 2,       // Change Group Number 0x70
            new Prop(2, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.N1N1, 1, 99, fmtDD.None)),                  //   Set
      };

      // Print_format (Class Code 0x67)
      private readonly AttrData[] ccPF_Addrs = new AttrData[] {
         new AttrData((int)ccPF.Message_Name, GSS.Get, false, 20,              // Message Name 0x64
            new Prop(14, DataFormats.UTF8, 0, 14, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(14, DataFormats.UTF8, 0, 14, fmtDD.None)),                 //   Set
         new AttrData((int)ccPF.Number_Of_Items, GSS.Get, false, 25,           // Number Of Items 0x65
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Set
         new AttrData((int)ccPF.Number_Of_Columns, GSS.Get, false, 21,         // Number Of Columns 0x66
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Set
         new AttrData((int)ccPF.Format_Type, GSS.Get, false, 14,               // Format Type 0x67
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout)),       //   Set
         new AttrData((int)ccPF.Insert_Column, GSS.Service, false, 15,         // Insert Column 0x69
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((int)ccPF.Delete_Column, GSS.Service, false, 8,          // Delete Column 0x6A
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((int)ccPF.Add_Column, GSS.Service, false, 1,             // Add Column 0x6B
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((int)ccPF.Number_Of_Print_Line_And_Print_Format, GSS.Set, false, 22, // Number Of Print Line And Print Format 0x6C
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None)),                //   Set
         new AttrData((int)ccPF.Format_Setup, GSS.Set, false, 13,              // Format Setup 0x6D
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout)),       //   Set
         new AttrData((int)ccPF.Adding_Print_Items, GSS.Service, false, 3,     // Adding Print Items 0x6E
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((int)ccPF.Deleting_Print_Items, GSS.Service, false, 9,   // Deleting Print Items 0x6F
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None),               //   Data
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Service
         new AttrData((int)ccPF.Print_Character_String, GSS.GetSet, false, 24,  // Print Character String 0x71
            new Prop(750, DataFormats.AttrText, 0, 0, fmtDD.None),              //   Data
            new Prop(0, DataFormats.AttrText, 0, 0, fmtDD.None),                //   Get
            new Prop(750, DataFormats.AttrText, 0, 0, fmtDD.None)),             //   Set
         new AttrData((int)ccPF.Line_Count, GSS.GetSet, false, 18,             // Line Count 0x72
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.None)),                //   Set
         new AttrData((int)ccPF.Line_Spacing, GSS.GetSet, false, 19,           // Line Spacing 0x73
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),                //   Set
         new AttrData((int)ccPF.Dot_Matrix, GSS.GetSet, false, 11,             // Dot Matrix 0x74
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.FontType),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.FontType)),           //   Set
         new AttrData((int)ccPF.InterCharacter_Space, GSS.GetSet, false, 16,   // InterCharacter Space 0x75
            new Prop(1, DataFormats.Decimal, 0, 26, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 26, fmtDD.None)),               //   Set
         new AttrData((int)ccPF.Character_Bold, GSS.GetSet, false, 7,          // Character Bold 0x76
            new Prop(1, DataFormats.Decimal, 1, 9, fmtDD.Decimal),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 9, fmtDD.Decimal)),             //   Set
         new AttrData((int)ccPF.Barcode_Type, GSS.GetSet, false, 5,            // BarCode Type 0x77
            new Prop(1, DataFormats.Decimal, 0, 27, fmtDD.BarcodeType),         //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 27, fmtDD.BarcodeType)),        //   Set
         new AttrData((int)ccPF.Readable_Code, GSS.GetSet, false, 27,          // Readable Code 0x78
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ReadableCode),         //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ReadableCode)),        //   Set
         new AttrData((int)ccPF.Prefix_Code, GSS.GetSet, false, 23,            // Prefix Code 0x79
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccPF.X_and_Y_Coordinate, GSS.GetSet, false, 28,     // X and Y Coordinate 0x7A
            new Prop(3, DataFormats.XY, 0, 0, fmtDD.None),                      //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.XY, 0, 0, fmtDD.None)),                     //   Set
         new AttrData((int)ccPF.InterCharacter_SpaceII, GSS.GetSet, false, 17, // InterCharacter SpaceII 0x7B
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccPF.Add_To_End_Of_String, GSS.Set, false, 2,       // Add To End Of String 0x8A
            new Prop(750, DataFormats.UTF8, 0, 0, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(750, DataFormats.UTF8, 0, 0, fmtDD.None)),                 //   Set
         new AttrData((int)ccPF.Calendar_Offset, GSS.GetSet, false, 6,         // Calendar Offset 0x8D
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.YesterdayToday),       //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.YesterdayToday)),      //   Set
         new AttrData((int)ccPF.DIN_Print, GSS.GetSet, false, 10,              // DIN Print 0x8E
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((int)ccPF.EAN_Prefix, GSS.GetSet, false, 12,             // EAN Prefix 0x8F
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EditPrint),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EditPrint)),           //   Set
         new AttrData((int)ccPF.Barcode_Printing, GSS.GetSet, false, 4,        // BarCode Printing 0x90
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.NormalReverse),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.NormalReverse)),       //   Set
         new AttrData((int)ccPF.QR_Error_Correction_Level, GSS.GetSet, false, 26, // QR Error Correction Level 0x91
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.M15Q25),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.M15Q25)),              //   Set
      };

      // Print_specification (Class Code 0x68)
      private readonly AttrData[] ccPS_Addrs = new AttrData[] {
         new AttrData((int)ccPS.Character_Height, GSS.GetSet, false, 2,        // Character Height 0x64
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccPS.Ink_Drop_Use, GSS.GetSet, false, 8,            // Ink Drop Use 0x65
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.None)),               //   Set
         new AttrData((int)ccPS.High_Speed_Print, GSS.GetSet, false, 6,        // High Speed Print 0x66
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.HighSpeedPrint),       //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.HighSpeedPrint)),      //   Set
         new AttrData((int)ccPS.Character_Width, GSS.GetSet, false, 4,         // Character Width 0x67
            new Prop(2, DataFormats.Decimal, 0, 3999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 3999, fmtDD.None)),             //   Set
         new AttrData((int)ccPS.Character_Orientation, GSS.GetSet, false, 3,   // Character Orientation 0x68
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.Orientation),          //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.Orientation)),         //   Set
         new AttrData((int)ccPS.Print_Start_Delay_Forward, GSS.GetSet, false, 11, // Print Start Delay Forward 0x69
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((int)ccPS.Print_Start_Delay_Reverse, GSS.GetSet, false, 10, // Print Start Delay Reverse 0x6A
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((int)ccPS.Product_Speed_Matching, GSS.GetSet, false, 14, // Product Speed Matching 0x6B
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ProductSpeedMatching), //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ProductSpeedMatching)), //   Set
         new AttrData((int)ccPS.Pulse_Rate_Division_Factor, GSS.GetSet, false, 15, // Pulse Rate Division Factor 0x6C
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None)),              //   Set
         new AttrData((int)ccPS.Speed_Compensation, GSS.GetSet, false, 18,     // Speed Compensation 0x6D
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccPS.Line_Speed, GSS.GetSet, false, 9,              // Line Speed 0x6E
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((int)ccPS.Distance_Between_Print_Head_And_Object, GSS.GetSet, false, 5, // Distance Between Print Head And Object 0x6F
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccPS.Print_Target_Width, GSS.GetSet, false, 13,     // Print Target Width 0x70
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccPS.Actual_Print_Width, GSS.GetSet, false, 1,      // Actual Print Width 0x71
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccPS.Repeat_Count, GSS.GetSet, false, 16,           // Repeat Count 0x72
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((int)ccPS.Repeat_Interval, GSS.GetSet, false, 17,        // Repeat Interval 0x73
            new Prop(3, DataFormats.Decimal, 0, 99999, fmtDD.None),             //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.Decimal, 0, 99999, fmtDD.None)),            //   Set
         new AttrData((int)ccPS.Target_Sensor_Timer, GSS.GetSet, false, 21,    // Target Sensor Timer 0x74
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None)),              //   Set
         new AttrData((int)ccPS.Target_Sensor_Filter, GSS.GetSet, false, 20,   // Target Sensor Filter 0x75
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.TargetSensorFilter),   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.TargetSensorFilter)),  //   Set
         new AttrData((int)ccPS.Target_Sensor_Filter_Value, GSS.GetSet, false, 19, // Target Sensor Filter Value 0x76
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((int)ccPS.Ink_Drop_Charge_Rule, GSS.GetSet, false, 7,    // Ink Drop Charge Rule 0x77
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ChargeRule),           //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ChargeRule)),          //   Set
         new AttrData((int)ccPS.Print_Start_Position_Adjustment_Value, GSS.GetSet, false, 12, // Print Start Position Adjustment Value 0x78
            new Prop(2, DataFormats.Decimal, -50, 50, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, -50, 50, fmtDD.None)),             //   Set
      };

      // Calendar (Class Code 0x69)
      private readonly AttrData[] ccCal_Addrs = new AttrData[] {
         new AttrData((int)ccCal.Shift_Code_Condition, GSS.Get, true, 10,      // Shift Code Condition 0x65
            new Prop(1, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccCal.First_Calendar_Block, GSS.Get, false, 3,      // First Calendar Block 0x66
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Set
         new AttrData((int)ccCal.Number_of_Calendar_Blocks, GSS.Get, false, 1, // Number of Calendar Blocks 0x67
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Set
         new AttrData((int)ccCal.Offset_Year, GSS.GetSet, false, 8,            // Offset Year 0x68
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccCal.Offset_Month, GSS.GetSet, false, 7,           // Offset Month 0x69
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccCal.Offset_Day, GSS.GetSet, false, 4,             // Offset Day 0x6A
            new Prop(2, DataFormats.Decimal, 0, 1999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 1999, fmtDD.None)),             //   Set
         new AttrData((int)ccCal.Offset_Hour, GSS.GetSet, false, 5,            // Offset Hour 0x6B
            new Prop(2, DataFormats.SDecimal, -23, 99, fmtDD.None),             //   Data
            new Prop(0, DataFormats.SDecimal, 0, 0, fmtDD.None),                //   Get
            new Prop(2, DataFormats.SDecimal, -23, 99, fmtDD.None)),            //   Set
         new AttrData((int)ccCal.Offset_Minute, GSS.GetSet, false, 6,          // Offset Minute 0x6C
            new Prop(2, DataFormats.SDecimal, -59, 99, fmtDD.None),             //   Data
            new Prop(0, DataFormats.SDecimal, 0, 0, fmtDD.None),                //   Get
            new Prop(2, DataFormats.SDecimal, -59, 99, fmtDD.None)),            //   Set
         new AttrData((int)ccCal.Zero_Suppress_Year, GSS.GetSet, false, 32,    // Zero Suppress Year 0x6D
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.DisableSpaceChar),     //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((int)ccCal.Zero_Suppress_Month, GSS.GetSet, false, 30,   // Zero Suppress Month 0x6E
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.DisableSpaceChar),     //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((int)ccCal.Zero_Suppress_Day, GSS.GetSet, false, 26,     // Zero Suppress Day 0x6F
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.DisableSpaceChar),     //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((int)ccCal.Zero_Suppress_Hour, GSS.GetSet, false, 28,    // Zero Suppress Hour 0x70
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.DisableSpaceChar),     //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((int)ccCal.Zero_Suppress_Minute, GSS.GetSet, false, 29,  // Zero Suppress Minute 0x71
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.DisableSpaceChar),     //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((int)ccCal.Zero_Suppress_Weeks, GSS.GetSet, false, 31,   // Zero Suppress Weeks 0x72
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.DisableSpaceChar),     //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((int)ccCal.Zero_Suppress_DayOfWeek, GSS.GetSet, false, 27, // Zero Suppress Day Of Week 0x73
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.DisableSpaceChar),     //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((int)ccCal.Substitute_Year, GSS.GetSet, false, 21,       // Substitute Year 0x74
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCal.Substitute_Month, GSS.GetSet, false, 19,      // Substitute Month 0x75
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCal.Substitute_Day, GSS.GetSet, false, 15,        // Substitute Day 0x76
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCal.Substitute_Hour, GSS.GetSet, false, 17,       // Substitute Hour 0x77
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCal.Substitute_Minute, GSS.GetSet, false, 18,     // Substitute Minute 0x78
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCal.Substitute_Weeks, GSS.GetSet, false, 20,      // Substitute Weeks 0x79
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCal.Substitute_DayOfWeek, GSS.GetSet, false, 16,  // Substitute Day Of Week 0x7A
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCal.Time_Count_Start_Value, GSS.GetSet, false, 24, // Time Count Start Value 0x7B
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccCal.Time_Count_End_Value, GSS.GetSet, false, 22,  // Time Count End Value 0x7C
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccCal.Time_Count_Reset_Value, GSS.GetSet, false, 23, // Time Count Reset Value 0x7D
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccCal.Reset_Time_Value, GSS.GetSet, false, 9,       // Reset Time Value 0x7E
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Set
         new AttrData((int)ccCal.Update_Interval_Value, GSS.GetSet, false, 25, // Update Interval Value 0x7F
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.TimeCount),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.TimeCount)),           //   Set
         new AttrData((int)ccCal.Shift_Start_Hour, GSS.GetSet, false, 13,      // Shift Start Hour 0x80
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Set
         new AttrData((int)ccCal.Shift_Start_Minute, GSS.GetSet, false, 14,    // Shift Start Minute 0x81
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None)),               //   Set
         new AttrData((int)ccCal.Shift_End_Hour, GSS.Get, false, 11,           // Shift End Hour 0x82
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Set
         new AttrData((int)ccCal.Shift_End_Minute, GSS.Get, false, 12,         // Shift End Minute 0x83
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None)),               //   Set
         new AttrData((int)ccCal.Shift_String_Value, GSS.GetSet, false, 2,     // Shift String Value 0x84
            new Prop(1, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.UTF8N, 0, 0, fmtDD.None)),                  //   Set
      };

      // User_pattern (Class Code 0x6B)
      private readonly AttrData[] ccUP_Addrs = new AttrData[] {
         new AttrData((int)ccUP.User_Pattern_Fixed, GSS.GetSet, true, 1,       // User Pattern Fixed 0x64
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(2, DataFormats.N1N1, 0, 0, fmtDD.None),                    //   Get
            new Prop(2, DataFormats.N1N1, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccUP.User_Pattern_Free, GSS.GetSet, true, 2,        // User Pattern Free 0x65
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(4, DataFormats.N1N2N1, 0, 0, fmtDD.None),                  //   Get
            new Prop(4, DataFormats.N1N2N1, 0, 0, fmtDD.None)),                 //   Set
      };

      // Substitution_rules (Class Code 0x6C)
      private readonly AttrData[] ccSR_Addrs = new AttrData[] {
         new AttrData((int)ccSR.Number, GSS.GetSet, false, 3,                  // Number 0x64
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccSR.Name, GSS.GetSet, false, 2,                    // Name 0x65
            new Prop(13, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(13, DataFormats.Item, 0, 0, fmtDD.None),                   //   Get
            new Prop(13, DataFormats.ItemChar, 0, 0, fmtDD.None)),              //   Set
         new AttrData((int)ccSR.Start_Year, GSS.GetSet, false, 1,              // Start Year 0x66
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccSR.Year, GSS.GetSet, false, 10,                   // Year 0x67
            new Prop(2, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 0, 23, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.ItemChar, 0, 23, fmtDD.None)),              //   Set
         new AttrData((int)ccSR.Month, GSS.GetSet, false, 8,                   // Month 0x68
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 1, 12, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.ItemChar, 1, 12, fmtDD.None)),              //   Set
         new AttrData((int)ccSR.Day, GSS.GetSet, false, 4,                     // Day 0x69
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 1, 31, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.ItemChar, 1, 31, fmtDD.None)),              //   Set
         new AttrData((int)ccSR.Hour, GSS.GetSet, false, 6,                    // Hour 0x6A
            new Prop(2, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 0, 23, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.ItemChar, 0, 23, fmtDD.None)),              //   Set
         new AttrData((int)ccSR.Minute, GSS.GetSet, false, 7,                  // Minute 0x6B
            new Prop(2, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 0, 59, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.ItemChar, 0, 59, fmtDD.None)),              //   Set
         new AttrData((int)ccSR.Week, GSS.GetSet, false, 9,                    // Week 0x6C
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 1, 53, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.ItemChar, 1, 53, fmtDD.None)),              //   Set
         new AttrData((int)ccSR.DayOfWeek, GSS.GetSet, false, 5,               // Day Of Week 0x6D
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 1, 7, fmtDD.None),                    //   Get
            new Prop(3, DataFormats.ItemChar, 1, 7, fmtDD.None)),               //   Set
      };

      // Enviroment_setting (Class Code 0x71)
      private readonly AttrData[] ccES_Addrs = new AttrData[] {
         new AttrData((int)ccES.Current_Time, GSS.GetSet, false, 5,            // Current Time 0x65
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccES.Calendar_Date_Time, GSS.GetSet, false, 1,      // Calendar Date Time 0x66
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccES.Calendar_Date_Time_Availibility, GSS.GetSet, false, 2, // Calendar Date Time Availibility 0x67
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.CurrentTime_StopClock), //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.CurrentTime_StopClock)), //   Set
         new AttrData((int)ccES.Clock_System, GSS.GetSet, false, 4,            // Clock System 0x68
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.Hour12_24),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.Hour12_24)),           //   Set
         new AttrData((int)ccES.User_Environment_Information, GSS.Get, false, 8, // User Environment Information 0x69
            new Prop(16, DataFormats.Bytes, 0, 0, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(16, DataFormats.Decimal, 0, 0, fmtDD.None)),               //   Set
         new AttrData((int)ccES.Cirulation_Control_Setting_Value, GSS.Get, false, 3, // Cirulation Control Setting Value 0x6A
            new Prop(12, DataFormats.Bytes, 0, 0, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(12, DataFormats.Decimal, 0, 0, fmtDD.None)),               //   Set
         new AttrData((int)ccES.Usage_Time_Of_Circulation_Control, GSS.Set, false, 7, // Usage Time Of Circulation Control 0x6B
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccES.Reset_Usage_Time_Of_Circulation_Control, GSS.Set, false, 6, // Reset Usage Time Of Circulation Control 0x6C
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
      };

      // Unit_Information (Class Code 0x73)
      private readonly AttrData[] ccUI_Addrs = new AttrData[] {
         new AttrData((int)ccUI.Unit_Information, GSS.Get, false, 20,          // Unit Information 0x64
            new Prop(64, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(64, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccUI.Model_Name, GSS.Get, false, 15,                // Model Name 0x6B
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccUI.Serial_Number, GSS.Get, false, 17,             // Serial Number 0x6C
            new Prop(8, DataFormats.DecimalLE, 0, 99999999, fmtDD.None),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(8, DataFormats.DecimalLE, 0, 99999999, fmtDD.None)),       //   Set
         new AttrData((int)ccUI.Ink_Name, GSS.Get, false, 8,                   // Ink Name 0x6D
            new Prop(28, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(28, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccUI.Input_Mode, GSS.Get, false, 9,                 // Input Mode 0x6E
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccUI.Maximum_Character_Count, GSS.Get, false, 11,   // Maximum Character Count 0x6F
            new Prop(2, DataFormats.Decimal, 240, 1000, fmtDD.None),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 240, 1000, fmtDD.None)),           //   Set
         new AttrData((int)ccUI.Maximum_Registered_Message_Count, GSS.Get, false, 13, // Maximum Registered Message Count 0x70
            new Prop(2, DataFormats.Decimal, 300, 2000, fmtDD.None),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 300, 2000, fmtDD.None)),           //   Set
         new AttrData((int)ccUI.Barcode_Information, GSS.Get, false, 1,        // BarCode Information 0x71
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.None)),                //   Set
         new AttrData((int)ccUI.Usable_Character_Size, GSS.Get, false, 21,     // Usable Character Size 0x72
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccUI.Maximum_Calendar_And_Count, GSS.Get, false, 10, // Maximum Calendar And Count 0x73
            new Prop(1, DataFormats.Decimal, 3, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 3, 8, fmtDD.None)),                //   Set
         new AttrData((int)ccUI.Maximum_Substitution_Rule, GSS.Get, false, 14, // Maximum Substitution Rule 0x74
            new Prop(1, DataFormats.Decimal, 48, 99, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 48, 99, fmtDD.None)),              //   Set
         new AttrData((int)ccUI.Shift_Code_And_Time_Count, GSS.Get, false, 18, // Shift Code And Time Count 0x75
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccUI.Chimney_And_DIN_Print, GSS.Get, false, 3,      // Chimney And DIN Print 0x76
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccUI.Maximum_Line_Count, GSS.Get, false, 12,        // Maximum Line Count 0x77
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccUI.Basic_Software_Version, GSS.Get, false, 2,     // Basic Software Version 0x78
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccUI.Controller_Software_Version, GSS.Get, false, 4, // Controller Software Version 0x79
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccUI.Engine_M_Software_Version, GSS.Get, false, 5,  // Engine M Software Version 0x7A
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccUI.Engine_S_Software_Version, GSS.Get, false, 6,  // Engine S Software Version 0x7B
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccUI.First_Language_Version, GSS.Get, false, 7,     // First Language Version 0x7C
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccUI.Second_Language_Version, GSS.Get, false, 16,   // Second Language Version 0x7D
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((int)ccUI.Software_Option_Version, GSS.Get, false, 19,   // Software Option Version 0x7E
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
      };

      // Operation_management (Class Code 0x74)
      private readonly AttrData[] ccOM_Addrs = new AttrData[] {
         new AttrData((int)ccOM.Operating_Management, GSS.Get, false, 12,      // Operating Management 0x64
            new Prop(2, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccOM.Ink_Operating_Time, GSS.GetSet, false, 9,      // Ink Operating Time 0x65
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Alarm_Time, GSS.GetSet, false, 1,              // Alarm Time 0x66
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Print_Count, GSS.GetSet, false, 13,            // Print Count 0x67
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Communications_Environment, GSS.Get, false, 3, // Communications Environment 0x68
            new Prop(2, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Cumulative_Operation_Time, GSS.Get, false, 4,  // Cumulative Operation Time 0x69
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Ink_And_Makeup_Name, GSS.Get, false, 8,        // Ink And Makeup Name 0x6A
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccOM.Ink_Viscosity, GSS.Get, false, 11,             // Ink Viscosity 0x6B
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Ink_Pressure, GSS.Get, false, 10,              // Ink Pressure 0x6C
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Ambient_Temperature, GSS.Get, false, 2,        // Ambient Temperature 0x6D
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Deflection_Voltage, GSS.Get, false, 5,         // Deflection Voltage 0x6E
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Excitation_VRef_Setup_Value, GSS.Get, false, 7, // Excitation VRef Setup Value 0x6F
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccOM.Excitation_Frequency, GSS.Get, false, 6,       // Excitation Frequency 0x70
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
      };

      // IJP_operation (Class Code 0x75)
      private readonly AttrData[] ccIJP_Addrs = new AttrData[] {
         new AttrData((int)ccIJP.Remote_operation_information, GSS.Get, false, 7, // Remote operation information 0x64
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccIJP.Fault_and_warning_history, GSS.Get, false, 4, // Fault and warning history 0x66
            new Prop(6, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(6, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccIJP.Operating_condition, GSS.Get, false, 6,       // Operating condition 0x67
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccIJP.Warning_condition, GSS.Get, false, 10,        // Warning condition 0x68
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccIJP.Date_and_time_information, GSS.Get, false, 1, // Date and time information 0x6A
            new Prop(10, DataFormats.Date, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(10, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccIJP.Error_code, GSS.Get, false, 3,                // Error code 0x6B
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccIJP.Start_Remote_Operation, GSS.Service, false, 8, // Start Remote Operation 0x6C
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((int)ccIJP.Stop_Remote_Operation, GSS.Service, false, 9, // Stop Remote Operation 0x6D
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((int)ccIJP.Deflection_voltage_control, GSS.Service, false, 2, // Deflection voltage control 0x6E
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((int)ccIJP.Online_Offline, GSS.GetSet, false, 5,         // Online Offline 0x6F
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OnlineOffline),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OnlineOffline)),       //   Set
      };

      // Count (Class Code 0x79)
      private readonly AttrData[] ccCount_Addrs = new AttrData[] {
         new AttrData((int)ccCount.First_Count_Block, GSS.Get, false, 12,      // First Count Block 0x65
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Set
         new AttrData((int)ccCount.Number_Of_Count_Blocks, GSS.Get, false, 12, // Number Of Count Blocks 0x66
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Set
         new AttrData((int)ccCount.Initial_Value, GSS.GetSet, false, 9,        // Initial Value 0x67
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccCount.Count_Range_1, GSS.GetSet, false, 4,        // Count Range 1 0x68
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccCount.Count_Range_2, GSS.GetSet, false, 5,        // Count Range 2 0x69
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccCount.Update_Unit_Halfway, GSS.GetSet, false, 15, // Update Unit Halfway 0x6A
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None)),           //   Set
         new AttrData((int)ccCount.Update_Unit_Unit, GSS.GetSet, false, 16,    // Update Unit Unit 0x6B
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None)),           //   Set
         new AttrData((int)ccCount.Increment_Value, GSS.GetSet, false, 8,      // Increment Value 0x6C
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((int)ccCount.Direction_Value, GSS.GetSet, false, 7,      // Direction Value 0x6D
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.UpDown),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.UpDown)),              //   Set
         new AttrData((int)ccCount.Jump_From, GSS.GetSet, false, 10,           // Jump From 0x6E
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccCount.Jump_To, GSS.GetSet, false, 11,             // Jump To 0x6F
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccCount.Reset_Value, GSS.GetSet, false, 13,         // Reset Value 0x70
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccCount.Type_Of_Reset_Signal, GSS.GetSet, false, 14, // Type Of Reset Signal 0x71
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None_Signal_1_2),      //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None_Signal_1_2)),     //   Set
         new AttrData((int)ccCount.External_Count, GSS.GetSet, false, 1,       // External Count 0x72
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCount.Zero_Suppression, GSS.GetSet, false, 2,     // Zero Suppression 0x73
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((int)ccCount.Count_Multiplier, GSS.GetSet, false, 3,     // Count Multiplier 0x74
            new Prop(10, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(10, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((int)ccCount.Count_Skip, GSS.GetSet, false, 6,           // Count Skip 0x75
            new Prop(4, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Get
            new Prop(4, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
      };

      // Index (Class Code 0x7A)
      private readonly AttrData[] ccIDX_Addrs = new AttrData[] {
         new AttrData((int)ccIDX.Start_Stop_Management_Flag, GSS.GetSet, false, 10, // Start Stop Management Flag 0x64
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),                //   Set
         new AttrData((int)ccIDX.Automatic_reflection, GSS.GetSet, false, 1,   // Automatic reflection 0x65
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OffOn),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.OffOn),                //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OffOn)),               //   Set
         new AttrData((int)ccIDX.Item, GSS.GetSet, false, 6,                   // Item 0x66
            new Prop(2, DataFormats.Decimal, 0, 100, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 100, fmtDD.None)),              //   Set
         new AttrData((int)ccIDX.Column, GSS.GetSet, false, 4,                 // Column 0x67
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccIDX.Line, GSS.GetSet, false, 7,                   // Line 0x68
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.Decimal),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.Decimal)),             //   Set
         new AttrData((int)ccIDX.Character_position, GSS.GetSet, false, 3,     // Character position 0x69
            new Prop(2, DataFormats.Decimal, 0, 1000, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 1000, fmtDD.None)),             //   Set
         new AttrData((int)ccIDX.Message_Number, GSS.GetSet, false, 9,         // Message Number 0x6A
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Set
         new AttrData((int)ccIDX.Group_Number, GSS.GetSet, false, 8,           // Group Number 0x6B
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccIDX.Substitution_Rule, GSS.GetSet, false, 11,     // Substitution Rule 0x6C
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Set
         new AttrData((int)ccIDX.User_Pattern_Size, GSS.GetSet, false, 12,     // User Pattern Size 0x6D
            new Prop(1, DataFormats.Decimal, 1, 19, fmtDD.FontType),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 19, fmtDD.FontType)),           //   Set
         new AttrData((int)ccIDX.Count_Block, GSS.GetSet, false, 5,            // Count Block 0x6E
            new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal)),             //   Set
         new AttrData((int)ccIDX.Calendar_Block, GSS.GetSet, false, 2,         // Calendar Block 0x6F
            new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal)),             //   Set
      };

      #endregion

      #region Class Codes => Attributes => Attribute Data lookup tables

      // Class Codes to Data Tables Conversion
      public AttrData[][] ClassCodeAttrData;

      #endregion

      #region Data reformatting routines

      // Reformat the raw data tables in this module to make them easier to read and modify
      public void ReformatTables(StreamWriter RFS) {

         DumpTableII(RFS, ccPDM_Addrs, ClassCode.Print_data_management, typeof(ccPDM));
         DumpTableII(RFS, ccPF_Addrs, ClassCode.Print_format, typeof(ccPF));
         DumpTableII(RFS, ccPS_Addrs, ClassCode.Print_specification, typeof(ccPS));
         DumpTableII(RFS, ccCal_Addrs, ClassCode.Calendar, typeof(ccCal));
         DumpTableII(RFS, ccUP_Addrs, ClassCode.User_pattern, typeof(ccUP));
         DumpTableII(RFS, ccSR_Addrs, ClassCode.Substitution_rules, typeof(ccSR));
         DumpTableII(RFS, ccES_Addrs, ClassCode.Enviroment_setting, typeof(ccES));
         DumpTableII(RFS, ccUI_Addrs, ClassCode.Unit_Information, typeof(ccUI));
         DumpTableII(RFS, ccOM_Addrs, ClassCode.Operation_management, typeof(ccOM));
         DumpTableII(RFS, ccIJP_Addrs, ClassCode.IJP_operation, typeof(ccIJP));
         DumpTableII(RFS, ccCount_Addrs, ClassCode.Count, typeof(ccCount));
         DumpTableII(RFS, ccIDX_Addrs, ClassCode.Index, typeof(ccIDX));

      }

      private void DumpTable(StreamWriter RFS, AttrData[] tbl, ClassCode cc, Type at) {
         // Now process each attribute within the Class
         string[] attrNames = Enum.GetNames(at);
         for (int i = 0; i < tbl.Length; i++) {

            string printLine = $"(0x{((int)cc).ToString("X2")}){cc}\t(0x{tbl[i].Val:X2}){attrNames[i]}\t";

            if (tbl[i].HasGet)
               printLine += "Get";
            printLine += "\t";
            if (tbl[i].HasSet)
               printLine += "Set";
            printLine += "\t";
            if (tbl[i].HasService)
               printLine += "Service";
            printLine += "\t";
            RFS.WriteLine(printLine.Replace('_',' '));

         }
         RFS.WriteLine(" ");
      }

      // Process the tables one at a time
      private void DumpTableII(StreamWriter RFS, AttrData[] tbl, ClassCode cc, Type at) {
         int count = 1;
         int stride = 0;
         // Calculate number and stride
         switch (cc) {
            case ClassCode.Print_data_management:
               break;
            case ClassCode.Print_format:
               count = 100;
               stride = 0x18; // 24
               break;
            case ClassCode.Print_specification:
               break;
            case ClassCode.Calendar:
               count = 8;
               stride = 0x20; // 32
               break;
            case ClassCode.User_pattern:
               break;
            case ClassCode.Substitution_rules:
               break;
            case ClassCode.Enviroment_setting:
               break;
            case ClassCode.Unit_Information:
               break;
            case ClassCode.Operation_management:
               break;
            case ClassCode.IJP_operation:
               break;
            case ClassCode.Count:
               count = 8;
               stride = 0x94; // 148
               break;
            case ClassCode.Index:
               break;
            default:
               break;
         }
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
         for (int i = 0; i < tbl.Length; i++) {

            // Turn Access into an enum
            string access = string.Empty;
            if (tbl[i].HasGet)
               access += "Get";
            if (tbl[i].HasSet)
               access += "Set";
            if (tbl[i].HasService)
               access += "Service";

            // Format Ignore as true/false and Data Format to an enum
            string ignore = tbl[i].Ignore ? "true" : "false";

            // Space the comment at the end of the line for readability
            string printLine = $"{t2}new AttrData((int){name}.{attrNames[i]}, GSS.{access}, {count}, {stride},";
            string spaces = new string(' ', Math.Max(80 - printLine.Length, 1));
            RFS.WriteLine($"{printLine}{spaces}// {attrNames[i].Replace("_", " ")} 0x{tbl[i].Val:X2}");

            // See how many properties are needed
            string[] s = null;
            p.Clear();
            p.Add(tbl[i].Data);
            if (tbl[i].HasService) {
               //p.Add(tbl[i].Service);
               s = new string[] { "Data" };
            } else {
               if (tbl[i].HasGet || tbl[i].HasSet) {
                  //p.Add(tbl[i].Get);
                  //p.Add(tbl[i].Set);
                  s = new string[] { "Data" };
               }
            }

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
      public bool HasSet { get; set; } = false;      // Supports a Set Request
      public bool HasGet { get; set; } = false;      // Supports a Get Request
      public bool HasService { get; set; } = false;  // Supports a Service Request
      public int Order { get; set; } = 0;            // Sort Order if Alpha Sort is requested
      public bool Ignore { get; set; } = false;      // Indicates that the request will hang printer
      public int Count { get; set; } = 1;            // Indicates max number of repetitions
      public int Stride { get; set; } = 0;           // Indicates the distance between repetitions

      // Four views of the printer data
      public Prop Data { get; set; }     // As it appears in the printer
      public Prop Get { get; set; }      // Data to be passed on a Get Request
      public Prop Set { get; set; }      // Data to be passed on a Set Request
      public Prop Service { get; set; }  // Data to be passed on a Service Request

      // A description of the data from four points of view.
      public AttrData(int Val, GSS acc, bool Ignore, int Order, Prop Data, Prop Data2 = null, Prop Data3 = null) {
         this.Val = Val;
         this.HasSet = acc == GSS.Set || acc == GSS.GetSet;
         this.HasGet = acc == GSS.Get || acc == GSS.GetSet;
         this.HasService = acc == GSS.Service;
         this.Ignore = Ignore;
         this.Order = Order;

         // This is what the data looks like in the printer
         this.Data = Data;
         if (this.HasService) {
            this.Service = Data2;
         } else {
            this.Get = Data2;
            this.Set = Data3;
         }
      }

      // A description of the data from four points of view.
      public AttrData(int Val, GSS acc, int Count, int Stride, Prop Data, Prop Data2 = null, Prop Data3 = null) {
         this.Val = Val;
         this.HasSet = acc == GSS.Set || acc == GSS.GetSet;
         this.HasGet = acc == GSS.Get || acc == GSS.GetSet;
         this.HasService = acc == GSS.Service;
         this.Count = Count;
         this.Stride = Stride;

         // This is what the data looks like in the printer
         this.Data = Data;
         if (this.HasService) {
            this.Service = Data2;
         } else {
            this.Get = Data2;
            this.Set = Data3;
         }
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
      public long Min { get; set; }
      public long Max { get; set; }
      public fmtDD DropDown { get; set; }

      public Prop(int Len, DataFormats Fmt, long Min, long Max, fmtDD DropDown = fmtDD.None) {
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


