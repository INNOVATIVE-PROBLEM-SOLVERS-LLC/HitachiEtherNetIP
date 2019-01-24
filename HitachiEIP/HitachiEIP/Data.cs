using System;

namespace HitachiEIP {

   public static class Data {

      // Class Codes to Attributes
      public static Type[] ClassCodeAttributes = new Type[] {
            typeof(eipPrint_Data_Management),   // 0x66
            typeof(eipPrint_format),            // 0x67
            typeof(eipPrint_specification),     // 0x68
            typeof(eipCalendar),                // 0x69
            typeof(eipUser_pattern),            // 0x6B
            typeof(eipSubstitution_rules),      // 0x6C
            typeof(eipEnviroment_setting),      // 0x71
            typeof(eipUnit_Information),        // 0x73
            typeof(eipOperation_management),    // 0x74
            typeof(eipIJP_operation),           // 0x75
            typeof(eipCount),                   // 0x79
            typeof(eipIndex),                   // 0x7A
         };

      // Class Codes
      public static int[,] ClassCodes = new int[,] {
         { 0X66, 7}, // Print data management function
         { 0X67, 8}, // Print format function
         { 0X68, 9}, // Print specification function
         { 0X69, 1}, // Calendar function
         { 0X6B, 12}, // User pattern function
         { 0X6C, 10}, // Substitution rules function
         { 0X71, 3}, // Enviroment setting function
         { 0X73, 11}, // Unit Information function
         { 0X74, 6}, // Operation management function
         { 0X75, 4}, // IJP operation function
         { 0X79, 2}, // Count function
         { 0X7A, 5}, // Index function
      };

      // Print Data Management (Class Code 0x66) Complete!
      public static int[][] PrintDataManagement = new int[][] {
         new int[] { 0X64, 0, 0, 1, 2, 0, 1, 2000, 9, 0}, // Select Message
         new int[] { 0X65, 1, 0, 0, 15, 1, 0, 0, 10, 0}, // Store Print Data
         new int[] { 0X67, 1, 0, 0, 2, 0, 1, 2000, 3, 0}, // Delete Print Data
         new int[] { 0X69, 1, 0, 0, 10, 1, 0, 0, 7, 0}, // Print Data Name
         new int[] { 0X6A, 0, 1, 0, 2, 0, 1, 2000, 6, 1}, // List of Messages
         new int[] { 0X6B, 1, 0, 5, 4, 0, 1, 2000, 8, 0}, // Print Data Number
         new int[] { 0X6C, 1, 0, 0, 14, 1, 0, 14, 1, 0}, // Change Create Group Name
         new int[] { 0X6D, 1, 0, 0, 1, 0, 1, 99, 4, 0}, // Group Deletion
         new int[] { 0X6F, 0, 1, 0, 1, 0, 1, 99, 5, 1}, // List of Groups
         new int[] { 0X70, 1, 0, 0, 2, 0, 1, 99, 2, 0}, // Change Group Number
      };

      // Print Format (Class Code 0x67)
      public static int[][] PrintFormat = new int[][] {
         new int[] { 0X64, 0, 1, 0, 1, 0, 0, 0, 20, 1}, // Message Name
         new int[] { 0X65, 0, 1, 0, 1, 0, 0, 99, 25, 0}, // Print Item
         new int[] { 0X66, 0, 1, 0, 1, 0, 1, 100, 21, 0}, // Number Of Columns
         new int[] { 0X67, 0, 1, 0, 1, 0, 1, 3, 14, 0}, // Format Type
         new int[] { 0X69, 0, 0, 1, 1, 0, 0, 99, 15, 0}, // Insert Column
         new int[] { 0X6A, 0, 0, 1, 1, 0, 0, 99, 8, 0}, // Delete Column
         new int[] { 0X6B, 0, 0, 1, 1, 0, 0, 0, 1, 0}, // Add Column
         new int[] { 0X6C, 1, 0, 0, 1, 0, 0, 1, 22, 0}, // Number Of Print Line And Print Format
         new int[] { 0X6D, 1, 0, 0, 1, 0, 0, 2, 13, 0}, // Format Setup
         new int[] { 0X6E, 0, 0, 1, 1, 0, 0, 0, 3, 0}, // Adding Print Items
         new int[] { 0X6F, 0, 0, 1, 1, 0, 1, 100, 9, 0}, // Deleting Print Items
         new int[] { 0X71, 1, 1, 0, 750, 1, 0, 0, 24, 0}, // Print Character String
         new int[] { 0X72, 1, 1, 0, 1, 0, 1, 6, 18, 0}, // Line Count
         new int[] { 0X73, 1, 1, 0, 1, 0, 0, 2, 19, 0}, // Line Spacing
         new int[] { 0X74, 1, 1, 0, 1, 0, 1, 16, 11, 0}, // Dot Matrix
         new int[] { 0X75, 1, 1, 0, 1, 0, 0, 26, 16, 0}, // InterCharacter Space
         new int[] { 0X76, 1, 1, 0, 1, 0, 1, 9, 7, 0}, // Character Bold
         new int[] { 0X77, 1, 1, 0, 1, 0, 0, 27, 5, 0}, // Barcode Type
         new int[] { 0X78, 1, 1, 0, 1, 0, 0, 2, 27, 0}, // Readable Code
         new int[] { 0X79, 1, 1, 0, 1, 0, 0, 99, 23, 0}, // Prefix Code
         new int[] { 0X7A, 1, 1, 0, 3, 4, 0, 0, 28, 0}, // X and Y Coordinate
         new int[] { 0X7B, 1, 1, 0, 2, 0, 0, 99, 17, 0}, // InterCharacter SpaceII
         new int[] { 0X8A, 1, 0, 0, 750, 1, 0, 0, 2, 0}, // Add To End Of String
         new int[] { 0X8D, 1, 1, 0, 1, 0, 0, 1, 6, 0}, // Calendar Offset
         new int[] { 0X8E, 1, 1, 0, 1, 0, 0, 1, 10, 0}, // DIN Print
         new int[] { 0X8F, 1, 1, 0, 1, 0, 0, 1, 12, 0}, // EAN Prefix
         new int[] { 0X90, 1, 1, 0, 1, 0, 0, 1, 4, 0}, // Barcode Printing
         new int[] { 0X91, 1, 1, 0, 1, 0, 0, 1, 26, 0}, // QR Error Correction Level
     };

      // Print Specification (Class Code 0x68)
      public static int[][] PrintSpecification = new int[][] {
         new int[] { 0X64, 1, 1, 0, 1, 0, 0, 99, 2, 0}, // Character Height
         new int[] { 0X65, 1, 1, 0, 1, 0, 1, 16, 8, 0}, // Ink Drop Use
         new int[] { 0X66, 1, 1, 0, 1, 0, 0, 3, 6, 0}, // High Speed Print
         new int[] { 0X67, 1, 1, 0, 2, 0, 0, 3999, 4, 0}, // Character Width
         new int[] { 0X68, 1, 1, 0, 1, 0, 0, 3, 3, 0}, // Character Orientation
         new int[] { 0X69, 1, 1, 0, 2, 0, 0, 9999, 11, 0}, // Print Start Delay Forward
         new int[] { 0X6A, 1, 1, 0, 2, 0, 0, 9999, 10, 0}, // Print Start Delay Reverse
         new int[] { 0X6B, 1, 1, 0, 1, 0, 0, 2, 14, 0}, // Product Speed Matching
         new int[] { 0X6C, 1, 1, 0, 2, 0, 0, 999, 15, 0}, // Pulse Rate Division Factor
         new int[] { 0X6D, 1, 1, 0, 1, 0, 0, 1, 18, 0}, // Speed Compensation
         new int[] { 0X6E, 1, 1, 0, 2, 0, 0, 9999, 9, 0}, // Line Speed
         new int[] { 0X6F, 1, 1, 0, 1, 0, 0, 99, 5, 0}, // Distance Between Print Head And Object
         new int[] { 0X70, 1, 1, 0, 1, 0, 0, 99, 13, 0}, // Print Target Width
         new int[] { 0X71, 1, 1, 0, 1, 0, 0, 99, 1, 0}, // Actual Print Width
         new int[] { 0X72, 1, 1, 0, 2, 0, 0, 9999, 16, 0}, // Repeat Count
         new int[] { 0X73, 1, 1, 0, 3, 0, 0, 99999, 17, 0}, // Repeat Interval
         new int[] { 0X74, 1, 1, 0, 2, 0, 0, 999, 21, 0}, // Target Sensor Timer
         new int[] { 0X75, 1, 1, 0, 1, 0, 0, 1, 20, 0}, // Target Sensor Filter
         new int[] { 0X76, 1, 1, 0, 2, 0, 0, 9999, 19, 0}, // Targer Sensor Filter Value
         new int[] { 0X77, 1, 1, 0, 1, 0, 0, 2, 7, 0}, // Ink Drop Charge Rule
         new int[] { 0X78, 1, 1, 0, 2, 0, -50, +50, 12, 0}, // Print Start Position Adjustment Value
      };

      // Calendar (Class Code = 0x69) Complete!
      public static int[][] Calendar = new int[][] {
         new int[] { 0X65, 0, 1, 0, 1, 0, 0, 0, 10, 0}, // Shift Count Condition
         new int[] { 0X66, 0, 1, 0, 1, 0, 0, 8, 3, 0}, // First Calendar Block Number
         new int[] { 0X67, 0, 1, 0, 1, 0, 0, 8, 1, 0}, // Calendar Block Number In Item
         new int[] { 0X68, 1, 1, 0, 1, 0, 0, 99, 8, 0}, // Offset Year
         new int[] { 0X69, 1, 1, 0, 1, 0, 0, 99, 7, 0}, // Offset Month
         new int[] { 0X6A, 1, 1, 0, 2, 0, 0, 1999, 4, 0}, // Offset Day
         new int[] { 0X6B, 1, 1, 0, 2, 0, -23, 99, 5, 0}, // Offset Hour
         new int[] { 0X6C, 1, 1, 0, 2, 0, -59, 99, 6, 0}, // Offset Minute
         new int[] { 0X6D, 1, 1, 0, 1, 0, 0, 2, 32, 0}, // Zero Suppress Year
         new int[] { 0X6E, 1, 1, 0, 1, 0, 0, 2, 30, 0}, // Zero Suppress Month
         new int[] { 0X6F, 1, 1, 0, 1, 0, 0, 2, 26, 0}, // Zero Suppress Day
         new int[] { 0X70, 1, 1, 0, 1, 0, 0, 2, 28, 0}, // Zero Suppress Hour
         new int[] { 0X71, 1, 1, 0, 1, 0, 0, 2, 29, 0}, // Zero Suppress Minute
         new int[] { 0X72, 1, 1, 0, 1, 0, 0, 2, 31, 0}, // Zero Suppress Weeks
         new int[] { 0X73, 1, 1, 0, 1, 0, 0, 2, 27, 0}, // Zero Suppress Day Of Week
         new int[] { 0X74, 1, 1, 0, 1, 0, 0, 1, 21, 0}, // Substitute Rule Year
         new int[] { 0X75, 1, 1, 0, 1, 0, 0, 1, 19, 0}, // Substitute Rule Month
         new int[] { 0X76, 1, 1, 0, 1, 0, 0, 1, 15, 0}, // Substitute Rule Day
         new int[] { 0X77, 1, 1, 0, 1, 0, 0, 1, 17, 0}, // Substitute Rule Hour
         new int[] { 0X78, 1, 1, 0, 1, 0, 0, 1, 18, 0}, // Substitute Rule Minute
         new int[] { 0X79, 1, 1, 0, 1, 0, 0, 1, 20, 0}, // Substitute Rule Weeks
         new int[] { 0X7A, 1, 1, 0, 1, 0, 0, 1, 16, 0}, // Substitute Rule Day Of Week
         new int[] { 0X7B, 1, 1, 0, 3, 1, 0, 0, 24, 0}, // Time Count Start Value
         new int[] { 0X7C, 1, 1, 0, 3, 1, 0, 0, 22, 0}, // Time Count End Value
         new int[] { 0X7D, 1, 1, 0, 3, 1, 0, 0, 23, 0}, // Time Count Reset Value
         new int[] { 0X7E, 1, 1, 0, 1, 0, 0, 23, 9, 0}, // Reset Time Value
         new int[] { 0X7F, 1, 1, 0, 1, 0, 1, 6, 25, 0}, // Update Interval Value
         new int[] { 0X80, 1, 1, 0, 1, 0, 0, 23, 13, 0}, // Shift Start Hour
         new int[] { 0X81, 1, 1, 0, 1, 0, 0, 59, 14, 0}, // Shift Start Minute
         new int[] { 0X82, 1, 1, 0, 1, 0, 0, 23, 11, 0}, // Shift End Hour
         new int[] { 0X83, 1, 1, 0, 1, 0, 0, 59, 12, 0}, // Shift Ene Minute
         new int[] { 0X84, 1, 1, 0, 10, 1, 0, 0, 2, 0}, // Count String Value
      };

      // User Pattern (Class Code 0x6B) Complete!
      public static int[][] UserPattern = new int[][] {
         new int[] { 0X64, 1, 1, 0, 0, 0, 0, 0, 1, 1}, // User Pattern Fixed
         new int[] { 0X65, 1, 1, 0, 0, 0, 0, 0, 2, 1}, // User Pattern Free
     };

      // Substitution Rules(Class Code 0x6C) Complete!
      public static int[][] SubstitutionRules = new int[][] {
         new int[] { 0X64, 1, 1, 0, 1, 3, 0, 0, 3, 0}, // Number
         new int[] { 0X65, 1, 1, 0, 1, 3, 0, 0, 2, 1}, // Name
         new int[] { 0X66, 1, 1, 0, 2, 3, 0, 0, 1, 0}, // Start Year
         new int[] { 0X67, 1, 1, 0, 3, 3, 0, 0, 10, 0}, // Year
         new int[] { 0X68, 1, 1, 0, 0, 3, 0, 0, 8, 0}, // Month
         new int[] { 0X69, 1, 1, 0, 0, 3, 0, 0, 4, 0}, // Day
         new int[] { 0X6A, 1, 1, 0, 0, 3, 0, 0, 6, 0}, // Hour
         new int[] { 0X6B, 1, 1, 0, 0, 3, 0, 0, 7, 0}, // Minute
         new int[] { 0X6C, 1, 1, 0, 0, 3, 0, 0, 9, 0}, // Week
         new int[] { 0X6D, 1, 1, 0, 0, 3, 0, 0, 5, 0}, // Day Of Week
      };

      // Enviroment Setting (Class Code 0x71) Complete!
      public static int[][] EnviromentSetting = new int[][] {
         new int[] { 0X65, 1, 1, 0, 7, 2, 0, 0, 5, 0}, // Current Time
         new int[] { 0X66, 1, 1, 0, 7, 2, 0, 0, 1, 0}, // Calendar Date Time
         new int[] { 0X67, 1, 1, 0, 1, 0, 0, 0, 2, 0}, // Calendar Date Time Availibility
         new int[] { 0X68, 1, 1, 0, 11, 0, 0, 0, 4, 0}, // Clock System
         new int[] { 0X69, 0, 1, 0, 16, 0, 0, 0, 8, 0}, // User Environment Information
         new int[] { 0X6A, 0, 1, 0, 12, 0, 0, 0, 3, 0}, // Cirulation Control Setting Value
         new int[] { 0X6B, 1, 0, 0, 2, 0, 0, 0, 7, 0}, // Usage Time Of Circulation Control
         new int[] { 0X6C, 1, 0, 0, 0, 0, 0, 0, 6, 0}, // Reset Usage Time Of Citculation Control
      };

      // Unit Information (Class Code 0x73) Complete!
      public static int[][] UnitInformation = new int[][] {
         new int[] { 0X64, 0, 1, 0, 64, 1, 0, 0, 20, 0}, // Unit Information
         new int[] { 0X6B, 0, 1, 0, 12, 1, 0, 0, 15, 0}, // Model Name
         new int[] { 0X6C, 0, 1, 0, 8, 0, 0, 0, 17, 0}, // Serial Number
         new int[] { 0X6D, 0, 1, 0, 28, 1, 0, 0, 8, 0}, // Ink Name
         new int[] { 0X6E, 0, 1, 0, 1, 0, 0, 0, 9, 0}, // Input Mode
         new int[] { 0X6F, 0, 1, 0, 2, 0, 240, 1000, 11, 0}, // Maximum Character Count
         new int[] { 0X70, 0, 1, 0, 2, 0, 300, 2000, 13, 0}, // Maximum Registered Message Count
         new int[] { 0X71, 0, 1, 0, 2, 0, 1, 2, 1, 0}, // Barcode Information
         new int[] { 0X72, 0, 1, 0, 1, 0, 0, 0, 21, 0}, // Usable Character Size
         new int[] { 0X73, 0, 1, 0, 1, 0, 3, 8, 10, 0}, // Maximum Calendar And Count
         new int[] { 0X74, 0, 1, 0, 1, 0, 48, 99, 14, 0}, // Maximum Substitution Rule
         new int[] { 0X75, 0, 1, 0, 1, 0, 0, 99, 18, 0}, // Shift Code And Time Count
         new int[] { 0X76, 0, 1, 0, 1, 0, 0, 0, 3, 0}, // Chimney And DIN Print
         new int[] { 0X77, 0, 1, 0, 1, 0, 0, 0, 12, 0}, // Maximum Line Count
         new int[] { 0X78, 0, 1, 0, 5, 1, 0, 0, 2, 0}, // Basic Software Version
         new int[] { 0X79, 0, 1, 0, 5, 1, 0, 0, 4, 0}, // Controller Software Version
         new int[] { 0X7A, 0, 1, 0, 5, 1, 0, 0, 5, 0}, // Engine M Software Version
         new int[] { 0X7B, 0, 1, 0, 5, 1, 0, 0, 6, 0}, // Engine S Software Version
         new int[] { 0X7C, 0, 1, 0, 5, 1, 0, 0, 7, 0}, // First Language Version
         new int[] { 0X7D, 0, 1, 0, 5, 1, 0, 0, 16, 0}, // Second Language Version
         new int[] { 0X7E, 0, 1, 0, 5, 1, 0, 0, 19, 1}, // Software Option Version
      };

      // Operation Management (Class Code 0x74) Complete!
      public static int[][] OperationManagement = new int[][] {
         new int[] { 0X64, 0, 1, 0, 2, 1, 0, 0, 12, 0}, // Operating Management
         new int[] { 0X65, 1, 1, 0, 2, 0, 0, 0, 9, 0}, // Ink Operating Time
         new int[] { 0X66, 1, 1, 0, 2, 0, 0, 0, 1, 0}, // Alarm Time
         new int[] { 0X67, 1, 1, 0, 2, 0, 0, 0, 13, 0}, // Print Count
         new int[] { 0X68, 0, 1, 0, 2, 0, 0, 0, 3, 0}, // Communications Environment
         new int[] { 0X69, 0, 1, 0, 2, 0, 0, 0, 4, 0}, // Cumulative Operation Time
         new int[] { 0X6A, 0, 1, 0, 2, 1, 0, 0, 8, 0}, // Ink And Makeup Name
         new int[] { 0X6B, 0, 1, 0, 2, 0, 0, 0, 11, 0}, // Ink Viscosity
         new int[] { 0X6C, 0, 1, 0, 2, 0, 0, 0, 10, 0}, // Ink Pressure
         new int[] { 0X6D, 0, 1, 0, 2, 0, 0, 0, 2, 0}, // Ambient Temperature
         new int[] { 0X6E, 0, 1, 0, 2, 0, 0, 0, 5, 0}, // Deflection Voltage
         new int[] { 0X6F, 0, 1, 0, 2, 0, 0, 0, 7, 0}, // Excitation VRef Setup Value
         new int[] { 0X70, 0, 1, 0, 2, 0, 0, 0, 6, 0}, // Excitation Frequency
     };

      // IJP Operation (Class Code 0x75) Complete!
      public static int[][] IJPOperation = new int[][] {
         new int[] { 0X64, 0, 1, 0, 1, 3, 0, 0, 7, 0}, // Remote operation information
         new int[] { 0X66, 0, 1, 0, 6, 3, 0, 0, 4, 0}, // Fault and warning history
         new int[] { 0X67, 0, 1, 0, 1, 3, 0, 0, 6, 0}, // Operating condition
         new int[] { 0X68, 0, 1, 0, 1, 3, 0, 0, 10, 0}, // Warning condition
         new int[] { 0X6A, 0, 1, 0, 10, 3, 0, 0, 1, 0}, // Date and time information
         new int[] { 0X6B, 0, 1, 0, 1, 3, 0, 0, 3, 0}, // Error code
         new int[] { 0X6C, 0, 0, 1, 0, 3, 0, 0, 8, 0}, // Start Remote Operation
         new int[] { 0X6D, 0, 0, 1, 0, 3, 0, 0, 9, 0}, // Stop Remote Operation
         new int[] { 0X6E, 0, 0, 1, 0, 3, 0, 0, 2, 0}, // Deflection voltage control
         new int[] { 0X6F, 1, 1, 0, 1, 0, 0, 1, 5, 0}, // Online Offline
      };

      // Count (Class Code 0x79) Complete!
      public static int[][] Count = new int[][] {
         new int[] { 0X66, 0, 1, 0, 0, 0, 0, 0, 12, 0}, // Number Of Count Block
         new int[] { 0X67, 1, 1, 0, 0, 0, 0, 0, 9, 0}, // Initial Value
         new int[] { 0X68, 1, 1, 0, 0, 0, 0, 0, 4, 0}, // Count Range 1
         new int[] { 0X69, 1, 1, 0, 0, 0, 0, 0, 5, 0}, // Count Range 2
         new int[] { 0X6A, 1, 1, 0, 0, 0, 0, 0, 15, 0}, // Update Unit Halfway
         new int[] { 0X6B, 1, 1, 0, 0, 0, 0, 0, 16, 0}, // Update Unit Unit
         new int[] { 0X6C, 1, 1, 0, 0, 0, 0, 0, 8, 0}, // Increment Value
         new int[] { 0X6D, 1, 1, 0, 0, 0, 0, 0, 7, 0}, // Direction Value
         new int[] { 0X6E, 1, 1, 0, 0, 0, 0, 0, 10, 0}, // Jump From
         new int[] { 0X6F, 1, 1, 0, 0, 0, 0, 0, 11, 0}, // Jump To
         new int[] { 0X70, 1, 1, 0, 0, 0, 0, 0, 13, 0}, // Reset Value
         new int[] { 0X71, 1, 1, 0, 0, 0, 0, 0, 14, 0}, // Type Of Reset Signal
         new int[] { 0X72, 1, 1, 0, 0, 0, 0, 0, 1, 0}, // Availibility Of External Count
         new int[] { 0X73, 1, 1, 0, 0, 0, 0, 0, 2, 0}, // Availibility Of Zero Suppression
         new int[] { 0X74, 1, 1, 0, 0, 0, 0, 0, 3, 0}, // Count Multiplier
         new int[] { 0X75, 1, 1, 0, 0, 0, 0, 0, 6, 0}, // Count Skip
      };

      // Index (Class Code 0x7A) Complete!
      public static int[][] Index = new int[][] {
         new int[] { 0X64, 1, 1, 0, 1, 0, 0, 2, 10, 0},     // Start Stop Management Flag
         new int[] { 0X65, 1, 1, 0, 1, 0, 0, 1, 1, 0},      // Automatic reflection
         new int[] { 0X66, 1, 1, 0, 2, 0, 1, 100, 6, 0},    // Item Count
         new int[] { 0X67, 1, 1, 0, 2, 0, 0, 99, 4, 0},     // Column
         new int[] { 0X68, 1, 1, 0, 1, 0, 1, 6, 7, 0},      // Line
         new int[] { 0X69, 1, 1, 0, 2, 0, 1, 1000, 3, 0},   // Character position
         new int[] { 0X6A, 1, 1, 0, 2, 0, 1, 2000, 9, 0},   // Print Data Message Number
         new int[] { 0X6B, 1, 1, 0, 1, 0, 1, 99, 8, 0},     // Print Data Group Data
         new int[] { 0X6C, 1, 1, 0, 1, 0, 1, 99, 11, 0},    // Substitution Rules Setting
         new int[] { 0X6D, 1, 1, 0, 1, 0, 1, 19, 12, 0},    // User Pattern Size
         new int[] { 0X6E, 1, 1, 0, 1, 0, 1, 8, 5, 0},      // Count Block
         new int[] { 0X6F, 1, 1, 0, 1, 0, 1, 8, 2, 0},      // Calendar Block
      };

      // Class Codes to Data Tables
      public static int[][][] ClassCodeData = new int[][][] {
            PrintDataManagement,           // 0x66
            PrintFormat,                   // 0x67
            PrintSpecification,            // 0x68
            Calendar,                      // 0x69
            UserPattern,                   // 0x6B
            SubstitutionRules,             // 0x6C
            EnviromentSetting,             // 0x71
            UnitInformation,               // 0x73
            OperationManagement,           // 0x74
            IJPOperation,                  // 0x75
            Count,                         // 0x79
            Index,                         // 0x7A
         };

      // Get attribute data for an arbitrary class/attribute
      public static AttrData GetAttrData(byte Class, byte attr ) {
         for (int i = 0; i < ClassCodes.GetLength(0); i++) {
            if ((byte)ClassCodes[i,0] == Class) {
               int[][] tab = ClassCodeData[i];
               for (int j = 0; j < tab.Length; j++) {
                  if ((byte)tab[j][0] == attr) {
                     return new AttrData(tab[j]);
                  }
               }
            }
         }
         return null;
      }

   }

   public class ClassCodeData {

      // Class Codes = { 
      //   [0] = value, 
      //   [1] = AlphaSortOrder }

      int[] values;

      public byte Val { get { return (byte)values[0]; } }
      public int Order { get { return values[1] - 1; } }

      public ClassCodeData(int[] values) {
         this.values = values;
      }

   }

   public class AttrData {

      #region Attributes

      // Class Code Attributes = {
      //   [0] = Value
      //   [1] = Set Available
      //   [2] = Get Available
      //   [3] = Service Available
      //   [4] = Data Length
      //   [5] = Format
      //   [6] = Min Value
      //   [7] = Max Value
      //   [8] = AlphaSortOrder 
      //   [9] = Ignore due to error }

      int[] values;

      public byte Val { get { return (byte)values[0]; } }
      public bool HasSet { get { return values[1] > 0; } }
      public bool HasGet { get { return values[2] > 0; } }
      public bool HasService { get { return values[3] > 0; } }
      public int Len { get { return values[4]; } }
      public DataFormats Fmt { get { return (DataFormats)values[5]; } }
      public int Min { get { return values[6]; } }
      public int Max { get { return values[7]; } }
      public int Order { get { return values[8] - 1; } }
      public bool Ignore { get { return values[9] > 0; } }

      public AttrData(int[] values) {
         this.values = values;
      }

      #endregion

   }

}
