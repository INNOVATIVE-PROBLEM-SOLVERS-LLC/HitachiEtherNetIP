using System;
using System.Collections.Generic;
using System.IO;

namespace EIP_Lib {

   #region Public enumerations

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
   }

   #endregion

   // Completely describe the Hitachi Model 161 data
   public class DataII {

      public DataII() {
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
         new AttrData((byte)ccPDM.Select_Message, GSS.Service, false, 9,        // Select Message 0x64
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Service
         new AttrData((byte)ccPDM.Store_Print_Data, GSS.Set, false, 10,         // Store Print Data 0x65
            new Prop(15, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(15, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccPDM.Delete_Print_Data, GSS.Set, false, 3,         // Delete Print Data 0x67
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Set
         new AttrData((byte)ccPDM.Print_Data_Name, GSS.Set, false, 7,           // Print Data Name 0x69
            new Prop(10, DataFormats.UTF8, 0, 14, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(10, DataFormats.N2Char, 0, 14, fmtDD.None)),               //   Set
         new AttrData((byte)ccPDM.List_of_Messages, GSS.Get, true, 6,           // List of Messages 0x6A
            new Prop(2, DataFormats.Decimal, 0, 2000, fmtDD.None),              //   Data
            new Prop(2, DataFormats.Decimal, 0, 2000, fmtDD.None),              //   Get
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccPDM.Print_Data_Number, GSS.Set, false, 8,         // Print Data Number 0x6B
            new Prop(4, DataFormats.Decimal, 1, 2000, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(4, DataFormats.N2N2, 1, 2000, fmtDD.None)),                //   Set
         new AttrData((byte)ccPDM.Change_Create_Group_Name, GSS.Set, false, 1,  // Change Create Group Name 0x6C
            new Prop(14, DataFormats.UTF8, 0, 14, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(14, DataFormats.N1Char, 0, 14, fmtDD.None)),               //   Set
         new AttrData((byte)ccPDM.Group_Deletion, GSS.Set, false, 4,            // Group Deletion 0x6D
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccPDM.List_of_Groups, GSS.Get, false, 5,            // List of Groups 0x6F
            new Prop(1, DataFormats.Bytes, 0, 99, fmtDD.None),                  //   Data
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Get
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccPDM.Change_Group_Number, GSS.Set, false, 2,       // Change Group Number 0x70
            new Prop(2, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.N1N1, 1, 99, fmtDD.None)),                  //   Set
      };

      // Print_format (Class Code 0x67)
      private AttrData[] ccPF_Addrs = new AttrData[] {
         new AttrData((byte)ccPF.Message_Name, GSS.Get, false, 20,              // Message Name 0x64
            new Prop(14, DataFormats.UTF8, 0, 14, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Get
            new Prop(14, DataFormats.UTF8, 0, 14, fmtDD.None)),                 //   Set
         new AttrData((byte)ccPF.Number_Of_Items, GSS.Get, false, 25,           // Number Of Items 0x65
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Set
         new AttrData((byte)ccPF.Number_Of_Columns, GSS.Get, false, 21,         // Number Of Columns 0x66
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Set
         new AttrData((byte)ccPF.Format_Type, GSS.Get, false, 14,               // Format Type 0x67
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout)),       //   Set
         new AttrData((byte)ccPF.Insert_Column, GSS.Service, false, 15,         // Insert Column 0x69
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((byte)ccPF.Delete_Column, GSS.Service, false, 8,          // Delete Column 0x6A
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((byte)ccPF.Add_Column, GSS.Service, false, 1,             // Add Column 0x6B
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((byte)ccPF.Number_Of_Print_Line_And_Print_Format, GSS.Set, false, 22, // Number Of Print Line And Print Format 0x6C
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None)),                //   Set
         new AttrData((byte)ccPF.Format_Setup, GSS.Set, false, 13,              // Format Setup 0x6D
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 3, fmtDD.Messagelayout)),       //   Set
         new AttrData((byte)ccPF.Adding_Print_Items, GSS.Service, false, 3,     // Adding Print Items 0x6E
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((byte)ccPF.Deleting_Print_Items, GSS.Service, false, 9,   // Deleting Print Items 0x6F
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None),               //   Data
            new Prop(1, DataFormats.Decimal, 1, 100, fmtDD.None)),              //   Service
         new AttrData((byte)ccPF.Print_Character_String, GSS.GetSet, false, 24, // Print Character String 0x71
            new Prop(750, DataFormats.UTF8, 0, 0, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(750, DataFormats.UTF8, 0, 0, fmtDD.None)),                 //   Set
         new AttrData((byte)ccPF.Line_Count, GSS.GetSet, false, 18,             // Line Count 0x72
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.None)),                //   Set
         new AttrData((byte)ccPF.Line_Spacing, GSS.GetSet, false, 19,           // Line Spacing 0x73
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),                //   Set
         new AttrData((byte)ccPF.Dot_Matrix, GSS.GetSet, false, 11,             // Dot Matrix 0x74
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.FontType),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.FontType)),           //   Set
         new AttrData((byte)ccPF.InterCharacter_Space, GSS.GetSet, false, 16,   // InterCharacter Space 0x75
            new Prop(1, DataFormats.Decimal, 0, 26, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 26, fmtDD.None)),               //   Set
         new AttrData((byte)ccPF.Character_Bold, GSS.GetSet, false, 7,          // Character Bold 0x76
            new Prop(1, DataFormats.Decimal, 1, 9, fmtDD.Decimal),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 9, fmtDD.Decimal)),             //   Set
         new AttrData((byte)ccPF.Barcode_Type, GSS.GetSet, false, 5,            // Barcode Type 0x77
            new Prop(1, DataFormats.Decimal, 0, 27, fmtDD.BarcodeType),         //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 27, fmtDD.BarcodeType)),        //   Set
         new AttrData((byte)ccPF.Readable_Code, GSS.GetSet, false, 27,          // Readable Code 0x78
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ReadableCode),         //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ReadableCode)),        //   Set
         new AttrData((byte)ccPF.Prefix_Code, GSS.GetSet, false, 23,            // Prefix Code 0x79
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccPF.X_and_Y_Coordinate, GSS.GetSet, false, 28,     // X and Y Coordinate 0x7A
            new Prop(3, DataFormats.XY, 0, 0, fmtDD.None),                      //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.XY, 0, 0, fmtDD.None)),                     //   Set
         new AttrData((byte)ccPF.InterCharacter_SpaceII, GSS.GetSet, false, 17, // InterCharacter SpaceII 0x7B
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccPF.Add_To_End_Of_String, GSS.Set, false, 2,       // Add To End Of String 0x8A
            new Prop(750, DataFormats.UTF8, 0, 0, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(750, DataFormats.UTF8, 0, 0, fmtDD.None)),                 //   Set
         new AttrData((byte)ccPF.Calendar_Offset, GSS.GetSet, false, 6,         // Calendar Offset 0x8D
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.YesterdayToday),       //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.YesterdayToday)),      //   Set
         new AttrData((byte)ccPF.DIN_Print, GSS.GetSet, false, 10,              // DIN Print 0x8E
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((byte)ccPF.EAN_Prefix, GSS.GetSet, false, 12,             // EAN Prefix 0x8F
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EditPrint),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EditPrint)),           //   Set
         new AttrData((byte)ccPF.Barcode_Printing, GSS.GetSet, false, 4,        // Barcode Printing 0x90
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.NormalReverse),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.NormalReverse)),       //   Set
         new AttrData((byte)ccPF.QR_Error_Correction_Level, GSS.GetSet, false, 26, // QR Error Correction Level 0x91
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.M15Q25),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.M15Q25)),              //   Set
      };

      // Print_specification (Class Code 0x68)
      private AttrData[] ccPS_Addrs = new AttrData[] {
         new AttrData((byte)ccPS.Character_Height, GSS.GetSet, false, 2,        // Character Height 0x64
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccPS.Ink_Drop_Use, GSS.GetSet, false, 8,            // Ink Drop Use 0x65
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 16, fmtDD.None)),               //   Set
         new AttrData((byte)ccPS.High_Speed_Print, GSS.GetSet, false, 6,        // High Speed Print 0x66
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.HighSpeedPrint),       //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.HighSpeedPrint)),      //   Set
         new AttrData((byte)ccPS.Character_Width, GSS.GetSet, false, 4,         // Character Width 0x67
            new Prop(2, DataFormats.Decimal, 0, 3999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 3999, fmtDD.None)),             //   Set
         new AttrData((byte)ccPS.Character_Orientation, GSS.GetSet, false, 3,   // Character Orientation 0x68
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.Orientation),          //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 3, fmtDD.Orientation)),         //   Set
         new AttrData((byte)ccPS.Print_Start_Delay_Forward, GSS.GetSet, false, 11, // Print Start Delay Forward 0x69
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((byte)ccPS.Print_Start_Delay_Reverse, GSS.GetSet, false, 10, // Print Start Delay Reverse 0x6A
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((byte)ccPS.Product_Speed_Matching, GSS.GetSet, false, 14, // Product Speed Matching 0x6B
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ProductSpeedMatching), //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ProductSpeedMatching)), //   Set
         new AttrData((byte)ccPS.Pulse_Rate_Division_Factor, GSS.GetSet, false, 15, // Pulse Rate Division Factor 0x6C
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None)),              //   Set
         new AttrData((byte)ccPS.Speed_Compensation, GSS.GetSet, false, 18,     // Speed Compensation 0x6D
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccPS.Line_Speed, GSS.GetSet, false, 9,              // Line Speed 0x6E
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((byte)ccPS.Distance_Between_Print_Head_And_Object, GSS.GetSet, false, 5, // Distance Between Print Head And Object 0x6F
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccPS.Print_Target_Width, GSS.GetSet, false, 13,     // Print Target Width 0x70
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccPS.Actual_Print_Width, GSS.GetSet, false, 1,      // Actual Print Width 0x71
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccPS.Repeat_Count, GSS.GetSet, false, 16,           // Repeat Count 0x72
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((byte)ccPS.Repeat_Interval, GSS.GetSet, false, 17,        // Repeat Interval 0x73
            new Prop(3, DataFormats.Decimal, 0, 99999, fmtDD.None),             //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.Decimal, 0, 99999, fmtDD.None)),            //   Set
         new AttrData((byte)ccPS.Target_Sensor_Timer, GSS.GetSet, false, 21,    // Target Sensor Timer 0x74
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 999, fmtDD.None)),              //   Set
         new AttrData((byte)ccPS.Target_Sensor_Filter, GSS.GetSet, false, 20,   // Target Sensor Filter 0x75
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.TargetSensorFilter),   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.TargetSensorFilter)),  //   Set
         new AttrData((byte)ccPS.Targer_Sensor_Filter_Value, GSS.GetSet, false, 19, // Targer Sensor Filter Value 0x76
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 9999, fmtDD.None)),             //   Set
         new AttrData((byte)ccPS.Ink_Drop_Charge_Rule, GSS.GetSet, false, 7,    // Ink Drop Charge Rule 0x77
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ChargeRule),           //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.ChargeRule)),          //   Set
         new AttrData((byte)ccPS.Print_Start_Position_Adjustment_Value, GSS.GetSet, false, 12, // Print Start Position Adjustment Value 0x78
            new Prop(2, DataFormats.Decimal, -50, 50, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, -50, 50, fmtDD.None)),             //   Set
      };

      // Calendar (Class Code 0x69)
      private AttrData[] ccCal_Addrs = new AttrData[] {
         new AttrData((byte)ccCal.Shift_Code_Condition, GSS.Get, true, 10,      // Shift Code Condition 0x65
            new Prop(1, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccCal.First_Calendar_Block, GSS.Get, false, 3,      // First Calendar Block 0x66
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Set
         new AttrData((byte)ccCal.Number_of_Calendar_Blocks, GSS.Get, false, 1, // Number of Calendar Blocks 0x67
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Set
         new AttrData((byte)ccCal.Offset_Year, GSS.GetSet, false, 8,            // Offset Year 0x68
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccCal.Offset_Month, GSS.GetSet, false, 7,           // Offset Month 0x69
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccCal.Offset_Day, GSS.GetSet, false, 4,             // Offset Day 0x6A
            new Prop(2, DataFormats.Decimal, 0, 1999, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 1999, fmtDD.None)),             //   Set
         new AttrData((byte)ccCal.Offset_Hour, GSS.GetSet, false, 5,            // Offset Hour 0x6B
            new Prop(2, DataFormats.SDecimal, -23, 99, fmtDD.None),             //   Data
            new Prop(0, DataFormats.SDecimal, 0, 0, fmtDD.None),                //   Get
            new Prop(2, DataFormats.SDecimal, -23, 99, fmtDD.None)),            //   Set
         new AttrData((byte)ccCal.Offset_Minute, GSS.GetSet, false, 6,          // Offset Minute 0x6C
            new Prop(2, DataFormats.SDecimal, -59, 99, fmtDD.None),             //   Data
            new Prop(0, DataFormats.SDecimal, 0, 0, fmtDD.None),                //   Get
            new Prop(2, DataFormats.SDecimal, -59, 99, fmtDD.None)),            //   Set
         new AttrData((byte)ccCal.Zero_Suppress_Year, GSS.GetSet, false, 32,    // Zero Suppress Year 0x6D
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((byte)ccCal.Zero_Suppress_Month, GSS.GetSet, false, 30,   // Zero Suppress Month 0x6E
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((byte)ccCal.Zero_Suppress_Day, GSS.GetSet, false, 26,     // Zero Suppress Day 0x6F
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((byte)ccCal.Zero_Suppress_Hour, GSS.GetSet, false, 28,    // Zero Suppress Hour 0x70
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((byte)ccCal.Zero_Suppress_Minute, GSS.GetSet, false, 29,  // Zero Suppress Minute 0x71
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((byte)ccCal.Zero_Suppress_Weeks, GSS.GetSet, false, 31,   // Zero Suppress Weeks 0x72
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((byte)ccCal.Zero_Suppress_Day_Of_Week, GSS.GetSet, false, 27, // Zero Suppress Day Of Week 0x73
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar),     //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.DisableSpaceChar)),    //   Set
         new AttrData((byte)ccCal.Substitute_Year, GSS.GetSet, false, 21,       // Substitute Year 0x74
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCal.Substitute_Month, GSS.GetSet, false, 19,      // Substitute Month 0x75
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCal.Substitute_Day, GSS.GetSet, false, 15,        // Substitute Day 0x76
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCal.Substitute_Hour, GSS.GetSet, false, 17,       // Substitute Hour 0x77
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCal.Substitute_Minute, GSS.GetSet, false, 18,     // Substitute Minute 0x78
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCal.Substitute_Weeks, GSS.GetSet, false, 20,      // Substitute Weeks 0x79
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCal.Substitute_Day_Of_Week, GSS.GetSet, false, 16, // Substitute Day Of Week 0x7A
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCal.Time_Count_Start_Value, GSS.GetSet, false, 24, // Time Count Start Value 0x7B
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccCal.Time_Count_End_Value, GSS.GetSet, false, 22,  // Time Count End Value 0x7C
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccCal.Time_Count_Reset_Value, GSS.GetSet, false, 23, // Time Count Reset Value 0x7D
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccCal.Reset_Time_Value, GSS.GetSet, false, 9,       // Reset Time Value 0x7E
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Set
         new AttrData((byte)ccCal.Update_Interval_Value, GSS.GetSet, false, 25, // Update Interval Value 0x7F
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.TimeCount),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.TimeCount)),           //   Set
         new AttrData((byte)ccCal.Shift_Start_Hour, GSS.GetSet, false, 13,      // Shift Start Hour 0x80
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Set
         new AttrData((byte)ccCal.Shift_Start_Minute, GSS.GetSet, false, 14,    // Shift Start Minute 0x81
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None)),               //   Set
         new AttrData((byte)ccCal.Shift_End_Hour, GSS.Get, false, 11,           // Shift End Hour 0x82
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 23, fmtDD.None)),               //   Set
         new AttrData((byte)ccCal.Shift_End_Minute, GSS.Get, false, 12,         // Shift End Minute 0x83
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 59, fmtDD.None)),               //   Set
         new AttrData((byte)ccCal.Shift_String_Value, GSS.GetSet, false, 2,     // Shift String Value 0x84
            new Prop(1, DataFormats.UTF8N, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.UTF8N, 0, 0, fmtDD.None)),                  //   Set
      };

      // User_pattern (Class Code 0x6B)
      private AttrData[] ccUP_Addrs = new AttrData[] {
         new AttrData((byte)ccUP.User_Pattern_Fixed, GSS.GetSet, true, 1,       // User Pattern Fixed 0x64
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(2, DataFormats.N1N1, 0, 0, fmtDD.None),                    //   Get
            new Prop(2, DataFormats.N1N1, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccUP.User_Pattern_Free, GSS.GetSet, true, 2,        // User Pattern Free 0x65
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(4, DataFormats.N1N2N1, 0, 0, fmtDD.None),                  //   Get
            new Prop(4, DataFormats.N1N2N1, 0, 0, fmtDD.None)),                 //   Set
      };

      // Substitution_rules (Class Code 0x6C)
      private AttrData[] ccSR_Addrs = new AttrData[] {
         new AttrData((byte)ccSR.Number, GSS.GetSet, false, 3,                  // Number 0x64
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccSR.Name, GSS.GetSet, false, 2,                    // Name 0x65
            new Prop(13, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(13, DataFormats.Item, 0, 0, fmtDD.None),                   //   Get
            new Prop(13, DataFormats.ItemChar, 0, 0, fmtDD.None)),              //   Set
         new AttrData((byte)ccSR.Start_Year, GSS.GetSet, false, 1,              // Start Year 0x66
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccSR.Year, GSS.GetSet, false, 10,                   // Year 0x67
            new Prop(2, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 0, 23, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.ItemChar, 0, 23, fmtDD.None)),              //   Set
         new AttrData((byte)ccSR.Month, GSS.GetSet, false, 8,                   // Month 0x68
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 1, 12, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.ItemChar, 1, 12, fmtDD.None)),              //   Set
         new AttrData((byte)ccSR.Day, GSS.GetSet, false, 4,                     // Day 0x69
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 1, 31, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.ItemChar, 1, 31, fmtDD.None)),              //   Set
         new AttrData((byte)ccSR.Hour, GSS.GetSet, false, 6,                    // Hour 0x6A
            new Prop(2, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 0, 23, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.ItemChar, 0, 23, fmtDD.None)),              //   Set
         new AttrData((byte)ccSR.Minute, GSS.GetSet, false, 7,                  // Minute 0x6B
            new Prop(2, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 0, 59, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.ItemChar, 0, 59, fmtDD.None)),              //   Set
         new AttrData((byte)ccSR.Week, GSS.GetSet, false, 9,                    // Week 0x6C
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 1, 53, fmtDD.None),                   //   Get
            new Prop(3, DataFormats.ItemChar, 1, 53, fmtDD.None)),              //   Set
         new AttrData((byte)ccSR.Day_Of_Week, GSS.GetSet, false, 5,             // Day Of Week 0x6D
            new Prop(3, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(1, DataFormats.Item, 1, 7, fmtDD.None),                    //   Get
            new Prop(3, DataFormats.ItemChar, 1, 7, fmtDD.None)),               //   Set
      };

      // Enviroment_setting (Class Code 0x71)
      private AttrData[] ccES_Addrs = new AttrData[] {
         new AttrData((byte)ccES.Current_Time, GSS.GetSet, false, 5,            // Current Time 0x65
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccES.Calendar_Date_Time, GSS.GetSet, false, 1,      // Calendar Date Time 0x66
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(12, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccES.Calendar_Date_Time_Availibility, GSS.GetSet, false, 2, // Calendar Date Time Availibility 0x67
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.CurrentTime_StopClock), //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.CurrentTime_StopClock)), //   Set
         new AttrData((byte)ccES.Clock_System, GSS.GetSet, false, 4,            // Clock System 0x68
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.Hour12_24),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.Hour12_24)),           //   Set
         new AttrData((byte)ccES.User_Environment_Information, GSS.Get, false, 8, // User Environment Information 0x69
            new Prop(16, DataFormats.Bytes, 0, 0, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(16, DataFormats.Decimal, 0, 0, fmtDD.None)),               //   Set
         new AttrData((byte)ccES.Cirulation_Control_Setting_Value, GSS.Get, false, 3, // Cirulation Control Setting Value 0x6A
            new Prop(12, DataFormats.Bytes, 0, 0, fmtDD.None),                  //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(12, DataFormats.Decimal, 0, 0, fmtDD.None)),               //   Set
         new AttrData((byte)ccES.Usage_Time_Of_Circulation_Control, GSS.Set, false, 7, // Usage Time Of Circulation Control 0x6B
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccES.Reset_Usage_Time_Of_Circulation_Control, GSS.Set, false, 6, // Reset Usage Time Of Circulation Control 0x6C
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
      };

      // Unit_Information (Class Code 0x73)
      private AttrData[] ccUI_Addrs = new AttrData[] {
         new AttrData((byte)ccUI.Unit_Information, GSS.Get, false, 20,          // Unit Information 0x64
            new Prop(64, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(64, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccUI.Model_Name, GSS.Get, false, 15,                // Model Name 0x6B
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccUI.Serial_Number, GSS.Get, false, 17,             // Serial Number 0x6C
            new Prop(8, DataFormats.DecimalLE, 0, 99999999, fmtDD.None),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(8, DataFormats.DecimalLE, 0, 99999999, fmtDD.None)),       //   Set
         new AttrData((byte)ccUI.Ink_Name, GSS.Get, false, 8,                   // Ink Name 0x6D
            new Prop(28, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(28, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccUI.Input_Mode, GSS.Get, false, 9,                 // Input Mode 0x6E
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccUI.Maximum_Character_Count, GSS.Get, false, 11,   // Maximum Character Count 0x6F
            new Prop(2, DataFormats.Decimal, 240, 1000, fmtDD.None),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 240, 1000, fmtDD.None)),           //   Set
         new AttrData((byte)ccUI.Maximum_Registered_Message_Count, GSS.Get, false, 13, // Maximum Registered Message Count 0x70
            new Prop(2, DataFormats.Decimal, 300, 2000, fmtDD.None),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 300, 2000, fmtDD.None)),           //   Set
         new AttrData((byte)ccUI.Barcode_Information, GSS.Get, false, 1,        // Barcode Information 0x71
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.None)),                //   Set
         new AttrData((byte)ccUI.Usable_Character_Size, GSS.Get, false, 21,     // Usable Character Size 0x72
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccUI.Maximum_Calendar_And_Count, GSS.Get, false, 10, // Maximum Calendar And Count 0x73
            new Prop(1, DataFormats.Decimal, 3, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 3, 8, fmtDD.None)),                //   Set
         new AttrData((byte)ccUI.Maximum_Substitution_Rule, GSS.Get, false, 14, // Maximum Substitution Rule 0x74
            new Prop(1, DataFormats.Decimal, 48, 99, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 48, 99, fmtDD.None)),              //   Set
         new AttrData((byte)ccUI.Shift_Code_And_Time_Count, GSS.Get, false, 18, // Shift Code And Time Count 0x75
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccUI.Chimney_And_DIN_Print, GSS.Get, false, 3,      // Chimney And DIN Print 0x76
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccUI.Maximum_Line_Count, GSS.Get, false, 12,        // Maximum Line Count 0x77
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccUI.Basic_Software_Version, GSS.Get, false, 2,     // Basic Software Version 0x78
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccUI.Controller_Software_Version, GSS.Get, false, 4, // Controller Software Version 0x79
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccUI.Engine_M_Software_Version, GSS.Get, false, 5,  // Engine M Software Version 0x7A
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccUI.Engine_S_Software_Version, GSS.Get, false, 6,  // Engine S Software Version 0x7B
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccUI.First_Language_Version, GSS.Get, false, 7,     // First Language Version 0x7C
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccUI.Second_Language_Version, GSS.Get, false, 16,   // Second Language Version 0x7D
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
         new AttrData((byte)ccUI.Software_Option_Version, GSS.Get, false, 19,   // Software Option Version 0x7E
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(5, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
      };

      // Operation_management (Class Code 0x74)
      private AttrData[] ccOM_Addrs = new AttrData[] {
         new AttrData((byte)ccOM.Operating_Management, GSS.Get, false, 12,      // Operating Management 0x64
            new Prop(2, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Get
            new Prop(2, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccOM.Ink_Operating_Time, GSS.GetSet, false, 9,      // Ink Operating Time 0x65
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Alarm_Time, GSS.GetSet, false, 1,              // Alarm Time 0x66
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Print_Count, GSS.GetSet, false, 13,            // Print Count 0x67
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Communications_Environment, GSS.Get, false, 3, // Communications Environment 0x68
            new Prop(2, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Cumulative_Operation_Time, GSS.Get, false, 4,  // Cumulative Operation Time 0x69
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Ink_And_Makeup_Name, GSS.Get, false, 8,        // Ink And Makeup Name 0x6A
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(12, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccOM.Ink_Viscosity, GSS.Get, false, 11,             // Ink Viscosity 0x6B
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Ink_Pressure, GSS.Get, false, 10,              // Ink Pressure 0x6C
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Ambient_Temperature, GSS.Get, false, 2,        // Ambient Temperature 0x6D
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Deflection_Voltage, GSS.Get, false, 5,         // Deflection Voltage 0x6E
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Excitation_VRef_Setup_Value, GSS.Get, false, 7, // Excitation VRef Setup Value 0x6F
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccOM.Excitation_Frequency, GSS.Get, false, 6,       // Excitation Frequency 0x70
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
      };

      // IJP_operation (Class Code 0x75)
      private AttrData[] ccIJP_Addrs = new AttrData[] {
         new AttrData((byte)ccIJP.Remote_operation_information, GSS.Get, false, 7, // Remote operation information 0x64
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccIJP.Fault_and_warning_history, GSS.Get, false, 4, // Fault and warning history 0x66
            new Prop(6, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(6, DataFormats.Bytes, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccIJP.Operating_condition, GSS.Get, false, 6,       // Operating condition 0x67
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccIJP.Warning_condition, GSS.Get, false, 10,        // Warning condition 0x68
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccIJP.Date_and_time_information, GSS.Get, false, 1, // Date and time information 0x6A
            new Prop(10, DataFormats.Date, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(10, DataFormats.Date, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccIJP.Error_code, GSS.Get, false, 3,                // Error code 0x6B
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccIJP.Start_Remote_Operation, GSS.Service, false, 8, // Start Remote Operation 0x6C
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((byte)ccIJP.Stop_Remote_Operation, GSS.Service, false, 9, // Stop Remote Operation 0x6D
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((byte)ccIJP.Deflection_voltage_control, GSS.Service, false, 2, // Deflection voltage control 0x6E
            new Prop(0, DataFormats.Bytes, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Service
         new AttrData((byte)ccIJP.Online_Offline, GSS.GetSet, false, 5,         // Online Offline 0x6F
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OnlineOffline),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.OnlineOffline)),       //   Set
      };

      // Count (Class Code 0x79)
      private AttrData[] ccCount_Addrs = new AttrData[] {
         new AttrData((byte)ccCount.First_Count_Block, GSS.Get, false, 12,      // First Count Block 0x65
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Set
         new AttrData((byte)ccCount.Number_Of_Count_Blocks, GSS.Get, false, 12, // Number Of Count Blocks 0x66
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 8, fmtDD.None)),                //   Set
         new AttrData((byte)ccCount.Initial_Value, GSS.GetSet, false, 9,        // Initial Value 0x67
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccCount.Count_Range_1, GSS.GetSet, false, 4,        // Count Range 1 0x68
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccCount.Count_Range_2, GSS.GetSet, false, 5,        // Count Range 2 0x69
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccCount.Update_Unit_Halfway, GSS.GetSet, false, 15, // Update Unit Halfway 0x6A
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None)),           //   Set
         new AttrData((byte)ccCount.Update_Unit_Unit, GSS.GetSet, false, 16,    // Update Unit Unit 0x6B
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(3, DataFormats.Decimal, 0, 999999, fmtDD.None)),           //   Set
         new AttrData((byte)ccCount.Increment_Value, GSS.GetSet, false, 8,      // Increment Value 0x6C
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None)),                //   Set
         new AttrData((byte)ccCount.Direction_Value, GSS.GetSet, false, 7,      // Direction Value 0x6D
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.UpDown),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 2, fmtDD.UpDown)),              //   Set
         new AttrData((byte)ccCount.Jump_From, GSS.GetSet, false, 10,           // Jump From 0x6E
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccCount.Jump_To, GSS.GetSet, false, 11,             // Jump To 0x6F
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccCount.Reset_Value, GSS.GetSet, false, 13,         // Reset Value 0x70
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(20, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccCount.Type_Of_Reset_Signal, GSS.GetSet, false, 14, // Type Of Reset Signal 0x71
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None_Signal_1_2),      //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.None_Signal_1_2)),     //   Set
         new AttrData((byte)ccCount.External_Count, GSS.GetSet, false, 1,       // External Count 0x72
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 0, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCount.Zero_Suppression, GSS.GetSet, false, 2,     // Zero Suppression 0x73
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable),        //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.EnableDisable)),       //   Set
         new AttrData((byte)ccCount.Count_Multiplier, GSS.GetSet, false, 3,     // Count Multiplier 0x74
            new Prop(10, DataFormats.UTF8, 0, 0, fmtDD.None),                   //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(10, DataFormats.UTF8, 0, 0, fmtDD.None)),                  //   Set
         new AttrData((byte)ccCount.Count_Skip, GSS.GetSet, false, 6,           // Count Skip 0x75
            new Prop(4, DataFormats.UTF8, 0, 0, fmtDD.None),                    //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(4, DataFormats.UTF8, 0, 0, fmtDD.None)),                   //   Set
      };

      // Index (Class Code 0x7A)
      private AttrData[] ccIDX_Addrs = new AttrData[] {
         new AttrData((byte)ccIDX.Start_Stop_Management_Flag, GSS.GetSet, false, 10, // Start Stop Management Flag 0x64
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 2, fmtDD.None)),                //   Set
         new AttrData((byte)ccIDX.Automatic_reflection, GSS.GetSet, false, 1,   // Automatic reflection 0x65
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None),                 //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 0, 1, fmtDD.None)),                //   Set
         new AttrData((byte)ccIDX.Item, GSS.GetSet, false, 6,                   // Item 0x66
            new Prop(2, DataFormats.Decimal, 0, 100, fmtDD.None),               //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 100, fmtDD.None)),              //   Set
         new AttrData((byte)ccIDX.Column, GSS.GetSet, false, 4,                 // Column 0x67
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccIDX.Line, GSS.GetSet, false, 7,                   // Line 0x68
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.Decimal),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 6, fmtDD.Decimal)),             //   Set
         new AttrData((byte)ccIDX.Character_position, GSS.GetSet, false, 3,     // Character position 0x69
            new Prop(2, DataFormats.Decimal, 0, 1000, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 0, 1000, fmtDD.None)),             //   Set
         new AttrData((byte)ccIDX.Print_Data_Message_Number, GSS.GetSet, false, 9, // Print Data Message Number 0x6A
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(2, DataFormats.Decimal, 1, 2000, fmtDD.None)),             //   Set
         new AttrData((byte)ccIDX.Print_Data_Group_Data, GSS.GetSet, false, 8,  // Print Data Group Data 0x6B
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccIDX.Substitution_Rule, GSS.GetSet, false, 11,     // Substitution Rule 0x6C
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None),                //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 99, fmtDD.None)),               //   Set
         new AttrData((byte)ccIDX.User_Pattern_Size, GSS.GetSet, false, 12,     // User Pattern Size 0x6D
            new Prop(1, DataFormats.Decimal, 1, 19, fmtDD.FontType),            //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 19, fmtDD.FontType)),           //   Set
         new AttrData((byte)ccIDX.Count_Block, GSS.GetSet, false, 5,            // Count Block 0x6E
            new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal),              //   Data
            new Prop(0, DataFormats.Decimal, 0, 0, fmtDD.None),                 //   Get
            new Prop(1, DataFormats.Decimal, 1, 8, fmtDD.Decimal)),             //   Set
         new AttrData((byte)ccIDX.Calendar_Block, GSS.GetSet, false, 2,         // Calendar Block 0x6F
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

      // Process the tables one at a time
      private void DumpTable(StreamWriter RFS, AttrData[] tbl, ClassCode cc, Type at) {
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
            string printLine = $"{t2}new AttrData((byte){name}.{attrNames[i]}, GSS.{access}, {ignore}, {tbl[i].Order},";
            string spaces = new string(' ', Math.Max(80 - printLine.Length, 1));
            RFS.WriteLine($"{printLine}{spaces}// {attrNames[i].Replace("_", " ")} 0x{tbl[i].Val:X2}");

            // See how many properties are needed
            string[] s = null;
            p.Clear();
            p.Add(tbl[i].Data);
            if (tbl[i].HasService) {
               p.Add(tbl[i].Service);
               s = new string[] { "Data", "Service" };
            } else {
               if (tbl[i].HasGet || tbl[i].HasSet) {
                  p.Add(tbl[i].Get);
                  p.Add(tbl[i].Set);
                  s = new string[] { "Data", "Get", "Set" };
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

   }

}


