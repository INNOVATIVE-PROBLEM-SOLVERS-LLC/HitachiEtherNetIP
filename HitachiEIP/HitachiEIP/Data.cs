using System;
using System.Collections.Generic;
using System.IO;

namespace HitachiEIP {

   public static class Data {

      #region Attribute raw data tables

      //   [0] = Value               = Attribute Value
      //   [1] = Set Available       = Set operations are possible
      //   [2] = Get Available       = Get operations are available
      //   [3] = Service Available   = Service Operations are available
      //                           = Property to describe Data and Get request
      //   [4] = Data Length         = Number of data bytes
      //   [5] = Format              = The format of the data (see DataFormats enum)
      //   [6] = Min Value           = Minimum value (Applies to Decimal Data only)
      //   [7] = Max Value           = Maximum value (Applies to Decimal Data only)
      //
      //   [8] = AlphaSortOrder      = For use if Alphabetical order is desired
      //   [9] = Ignore due to error = This request will cause the printer to lock up*
      //   [10] = Drop Down          = The dropdown to convert numbers to readable form
      //                           = Extra Property for some Get or Service routine
      //   [11] = Data Length        = Number of data bytes
      //   [12] = Format             = The format of the data (see DataFormats enum)
      //   [13] = Min Value          = Minimum value (Applies to Decimal Data only)
      //   [14] = Max Value          = Maximum value (Applies to Decimal Data only)

      // Print Data Management (Class Code 0x66) Complete!
      private static int[][] PrintDataManagement = new int[][] {
         new int[] { 0X64, 0, 0, 1, 0, 0, 0, 0, 9, 0, -1, 2, 0, 1, 2000}, // Select Message
         new int[] { 0X65, 1, 0, 0, 15, 1, 0, 0, 10, 0, -1},       // Store Print Data
         new int[] { 0X67, 1, 0, 0, 2, 0, 1, 2000, 3, 0, -1},      // Delete Print Data
         new int[] { 0X69, 1, 0, 0, 10, 1, 0, 0, 7, 0, -1},        // Print Data Name
         new int[] { 0X6A, 0, 1, 0, 0, 0, 0, 0, 6, 1, -1, 2, 0, 1, 2000}, // List of Messages
         new int[] { 0X6B, 1, 0, 0, 4, 0, 1, 2000, 8, 0, -1},      // Print Data Number
         new int[] { 0X6C, 1, 0, 0, 14, 1, 0, 14, 1, 0, -1},       // Change Create Group Name
         new int[] { 0X6D, 1, 0, 0, 1, 0, 1, 99, 4, 0, -1},        // Group Deletion
         new int[] { 0X6F, 0, 1, 0, 1, 0, 1, 99, 5, 1, -1},        // List of Groups
         new int[] { 0X70, 1, 0, 0, 2, 0, 1, 99, 2, 0, -1},        // Change Group Number
      };

      // Print Format (Class Code 0x67)
      private static int[][] PrintFormat = new int[][] {
         new int[] { 0X64, 0, 1, 0, 1, 0, 0, 0, 20, 1, -1},       // Message Name
         new int[] { 0X65, 0, 1, 0, 1, 0, 0, 99, 25, 0, -1},      // Print Item
         new int[] { 0X66, 0, 1, 0, 1, 0, 1, 100, 21, 0, -1},     // Number Of Columns
         new int[] { 0X67, 0, 1, 0, 1, 0, 1, 3, 14, 0, -1},       // Format Type
         new int[] { 0X69, 0, 0, 1, 1, 0, 0, 99, 15, 0, -1},      // Insert Column
         new int[] { 0X6A, 0, 0, 1, 1, 0, 0, 99, 8, 0, -1},       // Delete Column
         new int[] { 0X6B, 0, 0, 1, 1, 0, 0, 0, 1, 0, -1},        // Add Column
         new int[] { 0X6C, 1, 0, 0, 1, 0, 0, 1, 22, 0, -1},       // Number Of Print Line And Print Format
         new int[] { 0X6D, 1, 0, 0, 1, 0, 0, 2, 13, 0, -1},       // Format Setup
         new int[] { 0X6E, 0, 0, 1, 1, 0, 0, 0, 3, 0, -1},        // Adding Print Items
         new int[] { 0X6F, 0, 0, 1, 1, 0, 1, 100, 9, 0, -1},      // Deleting Print Items
         new int[] { 0X71, 1, 1, 0, 750, 1, 0, 0, 24, 0, -1},     // Print Character String
         new int[] { 0X72, 1, 1, 0, 1, 0, 1, 6, 18, 0, -1},       // Line Count
         new int[] { 0X73, 1, 1, 0, 1, 0, 0, 2, 19, 0, -1},       // Line Spacing
         new int[] { 0X74, 1, 1, 0, 1, 0, 1, 16, 11, 0, 14},      // Dot Matrix
         new int[] { 0X75, 1, 1, 0, 1, 0, 0, 26, 16, 0, -1},      // InterCharacter Space
         new int[] { 0X76, 1, 1, 0, 1, 0, 1, 9, 7, 0, 0},         // Character Bold
         new int[] { 0X77, 1, 1, 0, 1, 0, 0, 27, 5, 0, 9},        // Barcode Type
         new int[] { 0X78, 1, 1, 0, 1, 0, 0, 2, 27, 0, 8},        // Readable Code
         new int[] { 0X79, 1, 1, 0, 1, 0, 0, 99, 23, 0, -1},      // Prefix Code
         new int[] { 0X7A, 1, 1, 0, 3, 4, 0, 0, 28, 0, -1},       // X and Y Coordinate
         new int[] { 0X7B, 1, 1, 0, 2, 0, 0, 99, 17, 0, -1},      // InterCharacter SpaceII
         new int[] { 0X8A, 1, 0, 0, 750, 1, 0, 0, 2, 0, -1},      // Add To End Of String
         new int[] { 0X8D, 1, 1, 0, 1, 0, 0, 1, 6, 0, 13},        // Calendar Offset
         new int[] { 0X8E, 1, 1, 0, 1, 0, 0, 1, 10, 0, 1},        // DIN Print
         new int[] { 0X8F, 1, 1, 0, 1, 0, 0, 1, 12, 0, 12},       // EAN Prefix
         new int[] { 0X90, 1, 1, 0, 1, 0, 0, 1, 4, 0, 10},        // Barcode Printing
         new int[] { 0X91, 1, 1, 0, 1, 0, 0, 1, 26, 0, 11},       // QR Error Correction Level
     };

      // Print Specification (Class Code 0x68)
      private static int[][] PrintSpecification = new int[][] {
         new int[] { 0X64, 1, 1, 0, 1, 0, 0, 99, 2, 0, -1},         // Character Height
         new int[] { 0X65, 1, 1, 0, 1, 0, 1, 16, 8, 0, -1},         // Ink Drop Use
         new int[] { 0X66, 1, 1, 0, 1, 0, 0, 3, 6, 0, 17},          // High Speed Print
         new int[] { 0X67, 1, 1, 0, 2, 0, 0, 3999, 4, 0, -1},       // Character Width
         new int[] { 0X68, 1, 1, 0, 1, 0, 0, 3, 3, 0, 15},          // Character Orientation
         new int[] { 0X69, 1, 1, 0, 2, 0, 0, 9999, 11, 0, -1},      // Print Start Delay Forward
         new int[] { 0X6A, 1, 1, 0, 2, 0, 0, 9999, 10, 0, -1},      // Print Start Delay Reverse
         new int[] { 0X6B, 1, 1, 0, 1, 0, 0, 2, 14, 0, 16},         // Product Speed Matching
         new int[] { 0X6C, 1, 1, 0, 2, 0, 0, 999, 15, 0, -1},       // Pulse Rate Division Factor
         new int[] { 0X6D, 1, 1, 0, 1, 0, 0, 1, 18, 0, -1},         // Speed Compensation
         new int[] { 0X6E, 1, 1, 0, 2, 0, 0, 9999, 9, 0, -1},       // Line Speed
         new int[] { 0X6F, 1, 1, 0, 1, 0, 0, 99, 5, 0, -1},         // Distance Between Print Head And Object
         new int[] { 0X70, 1, 1, 0, 1, 0, 0, 99, 13, 0, -1},        // Print Target Width
         new int[] { 0X71, 1, 1, 0, 1, 0, 0, 99, 1, 0, -1},         // Actual Print Width
         new int[] { 0X72, 1, 1, 0, 2, 0, 0, 9999, 16, 0, -1},      // Repeat Count
         new int[] { 0X73, 1, 1, 0, 3, 0, 0, 99999, 17, 0, -1},     // Repeat Interval
         new int[] { 0X74, 1, 1, 0, 2, 0, 0, 999, 21, 0, -1},       // Target Sensor Timer
         new int[] { 0X75, 1, 1, 0, 1, 0, 0, 1, 20, 0, 18},         // Target Sensor Filter
         new int[] { 0X76, 1, 1, 0, 2, 0, 0, 9999, 19, 0, -1},      // Targer Sensor Filter Value
         new int[] { 0X77, 1, 1, 0, 1, 0, 0, 2, 7, 0, -1},          // Ink Drop Charge Rule
         new int[] { 0X78, 1, 1, 0, 2, 0, -50, +50, 12, 0, -1},     // Print Start Position Adjustment Value
      };

      // Calendar (Class Code = 0x69) Complete!
      private static int[][] Calendar = new int[][] {
         new int[] { 0X65, 0, 1, 0, 1, 0, 0, 0, 10, 0, -1},      // Shift Count Condition
         new int[] { 0X66, 0, 1, 0, 1, 0, 0, 8, 3, 0, -1},       // First Calendar Block Number
         new int[] { 0X67, 0, 1, 0, 1, 0, 0, 8, 1, 0, -1},       // Calendar Block Number In Item
         new int[] { 0X68, 1, 1, 0, 1, 0, 0, 99, 8, 0, -1},      // Offset Year
         new int[] { 0X69, 1, 1, 0, 1, 0, 0, 99, 7, 0, -1},      // Offset Month
         new int[] { 0X6A, 1, 1, 0, 2, 0, 0, 1999, 4, 0, -1},    // Offset Day
         new int[] { 0X6B, 1, 1, 0, 2, 0, -23, 99, 5, 0, -1},    // Offset Hour
         new int[] { 0X6C, 1, 1, 0, 2, 0, -59, 99, 6, 0, -1},    // Offset Minute
         new int[] { 0X6D, 1, 1, 0, 1, 0, 0, 2, 32, 0, 2},       // Zero Suppress Year
         new int[] { 0X6E, 1, 1, 0, 1, 0, 0, 2, 30, 0, 2},       // Zero Suppress Month
         new int[] { 0X6F, 1, 1, 0, 1, 0, 0, 2, 26, 0, 2},       // Zero Suppress Day
         new int[] { 0X70, 1, 1, 0, 1, 0, 0, 2, 28, 0, 2},       // Zero Suppress Hour
         new int[] { 0X71, 1, 1, 0, 1, 0, 0, 2, 29, 0, 2},       // Zero Suppress Minute
         new int[] { 0X72, 1, 1, 0, 1, 0, 0, 2, 31, 0, 2},       // Zero Suppress Weeks
         new int[] { 0X73, 1, 1, 0, 1, 0, 0, 2, 27, 0, 2},       // Zero Suppress Day Of Week
         new int[] { 0X74, 1, 1, 0, 1, 0, 0, 1, 21, 0, 1},       // Substitute Rule Year
         new int[] { 0X75, 1, 1, 0, 1, 0, 0, 1, 19, 0, 1},       // Substitute Rule Month
         new int[] { 0X76, 1, 1, 0, 1, 0, 0, 1, 15, 0, 1},       // Substitute Rule Day
         new int[] { 0X77, 1, 1, 0, 1, 0, 0, 1, 17, 0, 1},       // Substitute Rule Hour
         new int[] { 0X78, 1, 1, 0, 1, 0, 0, 1, 18, 0, 1},       // Substitute Rule Minute
         new int[] { 0X79, 1, 1, 0, 1, 0, 0, 1, 20, 0, 1},       // Substitute Rule Weeks
         new int[] { 0X7A, 1, 1, 0, 1, 0, 0, 1, 16, 0, 1},       // Substitute Rule Day Of Week
         new int[] { 0X7B, 1, 1, 0, 3, 1, 0, 0, 24, 0, -1},      // Time Count Start Value
         new int[] { 0X7C, 1, 1, 0, 3, 1, 0, 0, 22, 0, -1},      // Time Count End Value
         new int[] { 0X7D, 1, 1, 0, 3, 1, 0, 0, 23, 0, -1},      // Time Count Reset Value
         new int[] { 0X7E, 1, 1, 0, 1, 0, 0, 23, 9, 0, -1},      // Reset Time Value
         new int[] { 0X7F, 1, 1, 0, 1, 0, 1, 6, 25, 0, -1},      // Update Interval Value
         new int[] { 0X80, 1, 1, 0, 1, 0, 0, 23, 13, 0, -1},     // Shift Start Hour
         new int[] { 0X81, 1, 1, 0, 1, 0, 0, 59, 14, 0, -1},     // Shift Start Minute
         new int[] { 0X82, 1, 1, 0, 1, 0, 0, 23, 11, 0, -1},     // Shift End Hour
         new int[] { 0X83, 1, 1, 0, 1, 0, 0, 59, 12, 0, -1},     // Shift End Minute
         new int[] { 0X84, 1, 1, 0, 10, 1, 0, 0, 2, 0, -1},      // Count String Value
      };

      // User Pattern (Class Code 0x6B) Complete!
      private static int[][] UserPattern = new int[][] {
         new int[] { 0X64, 1, 1, 0, 0, 0, 0, 0, 1, 1, -1}, // User Pattern Fixed
         new int[] { 0X65, 1, 1, 0, 0, 0, 0, 0, 2, 1, -1}, // User Pattern Free
     };

      // Substitution Rules(Class Code 0x6C) Complete!
      private static int[][] SubstitutionRules = new int[][] {
         new int[] { 0X64, 1, 1, 0, 1, 0, 1, 99, 3, 0, -1},    // Number
         new int[] { 0X65, 1, 1, 0, 1, 1, 0, 0, 2, 1, -1},     // Name
         new int[] { 0X66, 1, 1, 0, 2, 0, 0, 0, 1, 0, -1},     // Start Year
         new int[] { 0X67, 1, 1, 0, 3, 1, 0, 0, 10, 0, -1},    // Year
         new int[] { 0X68, 1, 1, 0, 0, 1, 0, 0, 8, 0, -1},     // Month
         new int[] { 0X69, 1, 1, 0, 0, 1, 0, 0, 4, 0, -1},     // Day
         new int[] { 0X6A, 1, 1, 0, 0, 1, 0, 0, 6, 0, -1},     // Hour
         new int[] { 0X6B, 1, 1, 0, 0, 1, 0, 0, 7, 0, -1},     // Minute
         new int[] { 0X6C, 1, 1, 0, 0, 1, 0, 0, 9, 0, -1},     // Week
         new int[] { 0X6D, 1, 1, 0, 0, 1, 0, 0, 5, 0, -1},     // Day Of Week
      };

      // Enviroment Setting (Class Code 0x71) Complete!
      private static int[][] EnviromentSetting = new int[][] {
         new int[] { 0X65, 1, 1, 0, 12, 2, 0, 0, 5, 0, -1},    // Current Time
         new int[] { 0X66, 1, 1, 0, 12, 2, 0, 0, 1, 0, -1},    // Calendar Date Time
         new int[] { 0X67, 1, 1, 0, 1, 0, 1, 2, 2, 0, 4},      // Calendar Date Time Availibility
         new int[] { 0X68, 1, 1, 0, 1, 0, 1, 2, 4, 0, 3},      // Clock System
         new int[] { 0X69, 0, 1, 0, 16, 0, 0, 0, 8, 0, -1},    // User Environment Information
         new int[] { 0X6A, 0, 1, 0, 12, 0, 0, 0, 3, 0, -1},    // Cirulation Control Setting Value
         new int[] { 0X6B, 1, 0, 0, 2, 0, 0, 0, 7, 0, -1},     // Usage Time Of Circulation Control
         new int[] { 0X6C, 1, 0, 0, 0, 0, 0, 0, 6, 0, -1},     // Reset Usage Time Of Citculation Control
      };

      // Unit Information (Class Code 0x73) Complete!
      private static int[][] UnitInformation = new int[][] {
         new int[] { 0X64, 0, 1, 0, 64, 1, 0, 0, 20, 0, -1},       // Unit Information
         new int[] { 0X6B, 0, 1, 0, 12, 1, 0, 0, 15, 0, -1},       // Model Name
         new int[] { 0X6C, 0, 1, 0, 8, 0, 0, 0, 17, 0, -1},        // Serial Number
         new int[] { 0X6D, 0, 1, 0, 28, 1, 0, 0, 8, 0, -1},        // Ink Name
         new int[] { 0X6E, 0, 1, 0, 1, 0, 0, 0, 9, 0, -1},         // Input Mode
         new int[] { 0X6F, 0, 1, 0, 2, 0, 240, 1000, 11, 0, -1},   // Maximum Character Count
         new int[] { 0X70, 0, 1, 0, 2, 0, 300, 2000, 13, 0, -1},   // Maximum Registered Message Count
         new int[] { 0X71, 0, 1, 0, 1, 0, 1, 2, 1, 0, -1},         // Barcode Information
         new int[] { 0X72, 0, 1, 0, 1, 0, 0, 0, 21, 0, -1},        // Usable Character Size
         new int[] { 0X73, 0, 1, 0, 1, 0, 3, 8, 10, 0, -1},        // Maximum Calendar And Count
         new int[] { 0X74, 0, 1, 0, 1, 0, 48, 99, 14, 0, -1},      // Maximum Substitution Rule
         new int[] { 0X75, 0, 1, 0, 1, 0, 0, 99, 18, 0, -1},       // Shift Code And Time Count
         new int[] { 0X76, 0, 1, 0, 1, 0, 0, 0, 3, 0, -1},         // Chimney And DIN Print
         new int[] { 0X77, 0, 1, 0, 1, 0, 0, 0, 12, 0, -1},        // Maximum Line Count
         new int[] { 0X78, 0, 1, 0, 5, 1, 0, 0, 2, 0, -1},         // Basic Software Version
         new int[] { 0X79, 0, 1, 0, 5, 1, 0, 0, 4, 0, -1},         // Controller Software Version
         new int[] { 0X7A, 0, 1, 0, 5, 1, 0, 0, 5, 0, -1},         // Engine M Software Version
         new int[] { 0X7B, 0, 1, 0, 5, 1, 0, 0, 6, 0, -1},         // Engine S Software Version
         new int[] { 0X7C, 0, 1, 0, 5, 1, 0, 0, 7, 0, -1},         // First Language Version
         new int[] { 0X7D, 0, 1, 0, 5, 1, 0, 0, 16, 0, -1},        // Second Language Version
         new int[] { 0X7E, 0, 1, 0, 5, 1, 0, 0, 19, 1, -1},        // Software Option Version
      };

      // Operation Management (Class Code 0x74) Complete!
      private static int[][] OperationManagement = new int[][] {
         new int[] { 0X64, 0, 1, 0, 2, 1, 0, 0, 12, 0, -1},   // Operating Management
         new int[] { 0X65, 1, 1, 0, 2, 0, 0, 0, 9, 0, -1},    // Ink Operating Time
         new int[] { 0X66, 1, 1, 0, 2, 0, 0, 0, 1, 0, -1},    // Alarm Time
         new int[] { 0X67, 1, 1, 0, 2, 0, 0, 0, 13, 0, -1},   // Print Count
         new int[] { 0X68, 0, 1, 0, 2, 0, 0, 0, 3, 0, -1},    // Communications Environment
         new int[] { 0X69, 0, 1, 0, 2, 0, 0, 0, 4, 0, -1},    // Cumulative Operation Time
         new int[] { 0X6A, 0, 1, 0, 2, 1, 0, 0, 8, 0, -1},    // Ink And Makeup Name
         new int[] { 0X6B, 0, 1, 0, 2, 0, 0, 0, 11, 0, -1},   // Ink Viscosity
         new int[] { 0X6C, 0, 1, 0, 2, 0, 0, 0, 10, 0, -1},   // Ink Pressure
         new int[] { 0X6D, 0, 1, 0, 2, 0, 0, 0, 2, 0, -1},    // Ambient Temperature
         new int[] { 0X6E, 0, 1, 0, 2, 0, 0, 0, 5, 0, -1},    // Deflection Voltage
         new int[] { 0X6F, 0, 1, 0, 2, 0, 0, 0, 7, 0, -1},    // Excitation VRef Setup Value
         new int[] { 0X70, 0, 1, 0, 2, 0, 0, 0, 6, 0, -1},    // Excitation Frequency
     };

      // IJP Operation (Class Code 0x75) Complete!
      private static int[][] IJPOperation = new int[][] {
         new int[] { 0X64, 0, 1, 0, 1, 3, 0, 0, 7, 0, -1},    // Remote operation information
         new int[] { 0X66, 0, 1, 0, 6, 3, 0, 0, 4, 0, -1},    // Fault and warning history
         new int[] { 0X67, 0, 1, 0, 1, 3, 0, 0, 6, 0, -1},    // Operating condition
         new int[] { 0X68, 0, 1, 0, 1, 3, 0, 0, 10, 0, -1},   // Warning condition
         new int[] { 0X6A, 0, 1, 0, 10, 2, 0, 0, 1, 0, -1},   // Date and time information
         new int[] { 0X6B, 0, 1, 0, 1, 3, 0, 0, 3, 0, -1},    // Error code
         new int[] { 0X6C, 0, 0, 1, 0, 3, 0, 0, 8, 0, -1},    // Start Remote Operation
         new int[] { 0X6D, 0, 0, 1, 0, 3, 0, 0, 9, 0, -1},    // Stop Remote Operation
         new int[] { 0X6E, 0, 0, 1, 0, 3, 0, 0, 2, 0, -1},    // Deflection voltage control
         new int[] { 0X6F, 1, 1, 0, 1, 0, 0, 1, 5, 0, 5},     // Online Offline
      };

      // Count (Class Code 0x79) Complete!
      private static int[][] Count = new int[][] {
         new int[] { 0X66, 0, 1, 0, 0, 0, 0, 0, 12, 0, -1},   // Number Of Count Block
         new int[] { 0X67, 1, 1, 0, 0, 1, 0, 0, 9, 0, -1},    // Initial Value
         new int[] { 0X68, 1, 1, 0, 0, 1, 0, 0, 4, 0, -1},    // Count Range 1
         new int[] { 0X69, 1, 1, 0, 0, 1, 0, 0, 5, 0, -1},    // Count Range 2
         new int[] { 0X6A, 1, 1, 0, 0, 0, 0, 0, 15, 0, -1},   // Update Unit Halfway
         new int[] { 0X6B, 1, 1, 0, 0, 0, 0, 0, 16, 0, -1},   // Update Unit Unit
         new int[] { 0X6C, 1, 1, 0, 1, 0, 0, 0, 8, 0, -1},    // Increment Value
         new int[] { 0X6D, 1, 1, 0, 1, 0, 1, 2, 7, 0, 7},     // Direction Value
         new int[] { 0X6E, 1, 1, 0, 0, 1, 0, 0, 10, 0, -1},   // Jump From
         new int[] { 0X6F, 1, 1, 0, 0, 1, 0, 0, 11, 0, -1},   // Jump To
         new int[] { 0X70, 1, 1, 0, 0, 1, 0, 0, 13, 0, -1},   // Reset Value
         new int[] { 0X71, 1, 1, 0, 1, 0, 0, 0, 14, 0, 6},    // Type Of Reset Signal
         new int[] { 0X72, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1},     // Availibility Of External Count
         new int[] { 0X73, 1, 1, 0, 1, 0, 0, 0, 2, 0, 1},     // Availibility Of Zero Suppression
         new int[] { 0X74, 1, 1, 0, 0, 0, 0, 0, 3, 0, -1},    // Count Multiplier
         new int[] { 0X75, 1, 1, 0, 0, 0, 0, 0, 6, 0, -1},    // Count Skip
      };

      // Index (Class Code 0x7A) Complete!
      private static int[][] Index = new int[][] {
         new int[] { 0X64, 1, 1, 0, 1, 0, 0, 2, 10, 0, -1},     // Start Stop Management Flag
         new int[] { 0X65, 1, 1, 0, 1, 0, 0, 1, 1, 0, -1},      // Automatic reflection
         new int[] { 0X66, 1, 1, 0, 2, 0, 1, 100, 6, 0, -1},    // Item Count
         new int[] { 0X67, 1, 1, 0, 2, 0, 0, 99, 4, 0, -1},     // Column
         new int[] { 0X68, 1, 1, 0, 1, 0, 1, 6, 7, 0, 0},       // Line
         new int[] { 0X69, 1, 1, 0, 2, 0, 1, 1000, 3, 0, -1},   // Character position
         new int[] { 0X6A, 1, 1, 0, 2, 0, 1, 2000, 9, 0, -1},   // Print Data Message Number
         new int[] { 0X6B, 1, 1, 0, 1, 0, 1, 99, 8, 0, -1},     // Print Data Group Data
         new int[] { 0X6C, 1, 1, 0, 1, 0, 1, 99, 11, 0, -1},    // Substitution Rules Setting
         new int[] { 0X6D, 1, 1, 0, 1, 0, 1, 19, 12, 0, -1},    // User Pattern Size
         new int[] { 0X6E, 1, 1, 0, 1, 0, 1, 8, 5, 0, 0},       // Count Block
         new int[] { 0X6F, 1, 1, 0, 1, 0, 1, 8, 2, 0, 0},       // Calendar Block
      };

      // Reformat the raw data tables in this module to make them easier to read and modify
      public static void ReformatTables(StreamWriter RFS) {

         DumpTable(RFS, PrintDataManagement, ClassCode.Print_data_management, typeof(ccPDM));
         DumpTable(RFS, PrintFormat, ClassCode.Print_format, typeof(ccPF));
         DumpTable(RFS, PrintSpecification, ClassCode.Print_specification, typeof(ccPS));
         DumpTable(RFS, Calendar, ClassCode.Calendar, typeof(ccCal));
         DumpTable(RFS, UserPattern, ClassCode.User_pattern, typeof(ccUP));
         DumpTable(RFS, SubstitutionRules, ClassCode.Substitution_rules, typeof(ccSR));
         DumpTable(RFS, EnviromentSetting, ClassCode.Enviroment_setting, typeof(ccES));
         DumpTable(RFS, UnitInformation, ClassCode.Unit_Information, typeof(ccUI));
         DumpTable(RFS, OperationManagement, ClassCode.Operation_management, typeof(ccOM));
         DumpTable(RFS, IJPOperation, ClassCode.IJP_operation, typeof(ccIJP));
         DumpTable(RFS, Count, ClassCode.Count, typeof(ccCount));
         DumpTable(RFS, Index, ClassCode.Index, typeof(ccIDX));

      }

      // Process the tables one at a time
      private static void DumpTable(StreamWriter RFS, int[][] tbl, ClassCode cc, Type at) {
         string name = at.ToString();
         name = name.Substring(name.IndexOf('.') + 1);

         // Write out the table header
         RFS.WriteLine($"\t// {cc} (Class Code 0x{((int)cc).ToString("X2")})");
         RFS.WriteLine($"\tprivate static AttrData[] {name}_Addrs = new AttrData[] {{");

         // Now process each attributs within the Class
         string[] attrNames = Enum.GetNames(at);
         for (int i = 0; i < tbl.Length; i++) {

            // Turn Access into an enum
            string access = string.Empty;
            if (tbl[i][2] > 0) access += "Get";
            if (tbl[i][1] > 0) access += "Set";
            if (tbl[i][3] > 0) access += "Service";

            // Format Ignore as true/false and Data Format to an enum
            string ignore = tbl[i][9] > 0 ? "true" : "false";
            string fmt = ((DataFormats)tbl[i][5]).ToString();

            // Space the comment at the end of the line for readability
            string printLine = $"\t\t\tnew AttrData((byte){name}.{attrNames[i]}, GSS.{access}, {ignore}, {tbl[i][8]},";
            string spaces = new string(' ', Math.Max(70 - printLine.Length, 1));
            RFS.WriteLine($"{printLine}{spaces}// {attrNames[i].Replace("_", " ")}");

            // Was only one property specified
            if (tbl[i].Length == 11) {
               // Add the one property needed for processing Get, Set, or Service requests
               RFS.WriteLine($"\t\t\t\tnew Prop({tbl[i][4]}, DataFormats.{fmt}, {tbl[i][6]}, {tbl[i][7]}, fmtDD.{(fmtDD)tbl[i][10]})),");
            } else {
               // Add the two properties needed for processing the odd Get or Service requests
               string fmt2 = ((DataFormats)tbl[i][12]).ToString();
               RFS.WriteLine($"\t\t\t\tnew Prop({tbl[i][4]}, DataFormats.{fmt}, {tbl[i][6]}, {tbl[i][7]}, fmtDD.{(fmtDD)tbl[i][10]}),");
               RFS.WriteLine($"\t\t\t\tnew Prop({tbl[i][11]}, DataFormats.{fmt2}, {tbl[i][13]}, {tbl[i][14]}, fmtDD.{(fmtDD)tbl[i][10]})),");
            }
         }
         // Terminate the Attribute table
         RFS.WriteLine("\t\t};\n");
      }

      #endregion

   }
}
