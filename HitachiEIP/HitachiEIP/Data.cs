using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiEIP {

   static class Data {

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

      // Print Data Management (Class Code 0x66)
      public static int[,] PrintDataManagement = new int[,] {
         { 0X64, 0, 0, 1, 0, 3, 0, 0, 9}, // Select Message
         { 0X65, 1, 0, 0, 0, 3, 0, 0, 10}, // Store Print Data
         { 0X67, 1, 0, 0, 2, 3, 0, 0, 3}, // Delete Print Data
         { 0X69, 1, 0, 0, 0, 3, 0, 0, 7}, // Print Data Name
         { 0X6A, 0, 1, 0, 2, 3, 0, 0, 6}, // List of Messages
         { 0X6B, 1, 0, 0, 4, 3, 0, 0, 8}, // Print Data Number
         { 0X6C, 1, 0, 0, 0, 3, 0, 0, 1}, // Change Create Group Name
         { 0X6D, 1, 0, 0, 1, 3, 0, 0, 4}, // Group Deletion
         { 0X6F, 0, 1, 0, 1, 3, 0, 0, 5}, // List of Groups
         { 0X70, 1, 0, 0, 2, 3, 0, 0, 2}, // Change Group Number
      };

      // Print Format (Class Code 0x67)
      public static int[,] PrintFormat = new int[,] {
         { 0X64, 0, 1, 0, 1, 0, 0, 0, 20}, // Message Name
         { 0X65, 0, 1, 0, 1, 0, 0, 0, 25}, // Print Item
         { 0X66, 0, 1, 0, 1, 0, 0, 0, 21}, // Number Of Columns
         { 0X67, 0, 1, 0, 1, 0, 0, 0, 14}, // Format Type
         { 0X69, 0, 0, 1, 1, 0, 0, 0, 15}, // Insert Column
         { 0X6A, 0, 0, 1, 1, 0, 0, 0, 8}, // Delete Column
         { 0X6B, 0, 0, 1, 1, 0, 0, 0, 1}, // Add Column
         { 0X6C, 1, 0, 0, 1, 0, 0, 0, 22}, // Number Of Print Line And Print Format
         { 0X6D, 1, 0, 0, 1, 0, 0, 0, 13}, // Format Setup
         { 0X6E, 0, 0, 1, 1, 0, 0, 0, 3}, // Adding Print Items
         { 0X6F, 0, 0, 1, 1, 0, 0, 0, 9}, // Deleting Print Items
         { 0X71, 1, 1, 0, 1, 0, 0, 0, 24}, // Print Character String
         { 0X72, 1, 1, 0, 1, 0, 0, 0, 18}, // Line Count
         { 0X73, 1, 1, 0, 1, 0, 0, 0, 19}, // Line Spacing
         { 0X74, 1, 1, 0, 1, 0, 0, 0, 11}, // Dot Matrix
         { 0X75, 1, 1, 0, 1, 0, 0, 0, 16}, // InterCharacter Space
         { 0X76, 1, 1, 0, 1, 0, 0, 0, 7}, // Character Bold
         { 0X77, 1, 1, 0, 1, 0, 0, 0, 5}, // Barcode Type
         { 0X78, 1, 1, 0, 1, 0, 0, 0, 27}, // Readable Code
         { 0X79, 1, 1, 0, 1, 0, 0, 0, 23}, // Prefix Code
         { 0X7A, 1, 1, 0, 1, 0, 0, 0, 28}, // X and Y Coordinate
         { 0X7B, 1, 1, 0, 1, 0, 0, 0, 17}, // InterCharacter SpaceII
         { 0X8A, 1, 0, 0, 1, 0, 0, 0, 2}, // Add To End Of String
         { 0X8D, 1, 1, 0, 1, 0, 0, 0, 6}, // Calendar Offset
         { 0X8E, 1, 1, 0, 1, 0, 0, 0, 10}, // DIN Print
         { 0X8F, 1, 1, 0, 1, 0, 0, 0, 12}, // EAN Prefix
         { 0X90, 1, 1, 0, 1, 0, 0, 0, 4}, // Barcode Printing
         { 0X91, 1, 1, 0, 1, 0, 0, 0, 26}, // QR Error Correction Level
     };

      // Print Specification (Class Code 0x68)
      public static int[,] PrintSpecification = new int[,] {
         { 0X64, 1, 1, 0, 1, 0, 0, 0, 2}, // Character Height
         { 0X65, 1, 1, 0, 1, 0, 0, 0, 8}, // Ink Drop Use
         { 0X66, 1, 1, 0, 1, 0, 0, 0, 6}, // High Speed Print
         { 0X67, 1, 1, 0, 2, 0, 0, 0, 4}, // Character Width
         { 0X68, 1, 1, 0, 1, 0, 0, 0, 3}, // Character Orientation
         { 0X69, 1, 1, 0, 2, 0, 0, 0, 11}, // Print Start Delay Forward
         { 0X6A, 1, 1, 0, 2, 0, 0, 0, 10}, // Print Start Delay Reverse
         { 0X6B, 1, 1, 0, 1, 0, 0, 0, 14}, // Product Speed Matching
         { 0X6C, 1, 1, 0, 2, 0, 0, 0, 15}, // Pulse Rate Division Factor
         { 0X6D, 1, 1, 0, 1, 0, 0, 0, 18}, // Speed Compensation
         { 0X6E, 1, 1, 0, 2, 0, 0, 0, 9}, // Line Speed
         { 0X6F, 1, 1, 0, 1, 0, 0, 0, 5}, // Distance Between Print Head And Object
         { 0X70, 1, 1, 0, 2, 0, 0, 0, 13}, // Print Target Width
         { 0X71, 1, 1, 0, 2, 0, 0, 0, 1}, // Actual Print Width
         { 0X72, 1, 1, 0, 2, 0, 0, 0, 16}, // Repeat Count
         { 0X73, 1, 1, 0, 3, 0, 0, 0, 17}, // Repeat Interval
         { 0X74, 1, 1, 0, 2, 0, 0, 0, 21}, // Target Sensor Timer
         { 0X75, 1, 1, 0, 1, 0, 0, 0, 20}, // Target Sensor Filter
         { 0X76, 1, 1, 0, 1, 0, 0, 0, 19}, // Targer Sensor Filter Value
         { 0X77, 1, 1, 0, 1, 0, 0, 0, 7}, // Ink Drop Charge Rule
         { 0X78, 1, 1, 0, 2, 0, 0, 0, 12}, // Print Start Position Adjustment Value
      };

      // Calendar (Class Code = 0x69)
      public static int[,] Calendar = new int[,] {
         { 0X65, 0, 1, 0, 1, 0, 0, 0, 10}, // Shift Count Condition
         { 0X66, 0, 1, 0, 1, 0, 0, 0, 3}, // First Calendar Block Number
         { 0X67, 0, 1, 0, 1, 0, 0, 0, 1}, // Calendar Block Number In Item
         { 0X68, 1, 1, 0, 1, 0, 0, 0, 8}, // Offset Year
         { 0X69, 1, 1, 0, 1, 0, 0, 0, 7}, // Offset Month
         { 0X6A, 1, 1, 0, 2, 0, 0, 0, 4}, // Offset Day
         { 0X6B, 1, 1, 0, 2, 0, 0, 0, 5}, // Offset Hour
         { 0X6C, 1, 1, 0, 2, 0, 0, 0, 6}, // Offset Minute
         { 0X6D, 1, 1, 0, 1, 0, 0, 0, 32}, // Zero Suppress Year
         { 0X6E, 1, 1, 0, 1, 0, 0, 0, 30}, // Zero Suppress Month
         { 0X6F, 1, 1, 0, 1, 0, 0, 0, 26}, // Zero Suppress Day
         { 0X70, 1, 1, 0, 1, 0, 0, 0, 28}, // Zero Suppress Hour
         { 0X71, 1, 1, 0, 1, 0, 0, 0, 29}, // Zero Suppress Minute
         { 0X72, 1, 1, 0, 1, 0, 0, 0, 31}, // Zero Suppress Weeks
         { 0X73, 1, 1, 0, 1, 0, 0, 0, 27}, // Zero Suppress Day Of Week
         { 0X74, 1, 1, 0, 1, 0, 0, 0, 21}, // Substitute Rule Year
         { 0X75, 1, 1, 0, 1, 0, 0, 0, 19}, // Substitute Rule Month
         { 0X76, 1, 1, 0, 1, 0, 0, 0, 15}, // Substitute Rule Day
         { 0X77, 1, 1, 0, 1, 0, 0, 0, 17}, // Substitute Rule Hour
         { 0X78, 1, 1, 0, 1, 0, 0, 0, 18}, // Substitute Rule Minute
         { 0X79, 1, 1, 0, 1, 0, 0, 0, 20}, // Substitute Rule Weeks
         { 0X7A, 1, 1, 0, 1, 0, 0, 0, 16}, // Substitute Rule Day Of Week
         { 0X7B, 1, 1, 0, 3, 0, 0, 0, 24}, // Time Count Start Value
         { 0X7C, 1, 1, 0, 3, 0, 0, 0, 22}, // Time Count End Value
         { 0X7D, 1, 1, 0, 3, 0, 0, 0, 23}, // Time Count Reset Value
         { 0X7E, 1, 1, 0, 1, 0, 0, 0, 9}, // Reset Time Value
         { 0X7F, 1, 1, 0, 1, 0, 0, 0, 25}, // Update Interval Value
         { 0X80, 1, 1, 0, 1, 0, 0, 0, 13}, // Shift Start Hour
         { 0X81, 1, 1, 0, 1, 0, 0, 0, 14}, // Shift Start Minute
         { 0X82, 1, 1, 0, 1, 0, 0, 0, 11}, // Shift End Hour
         { 0X83, 1, 1, 0, 1, 0, 0, 0, 12}, // Shift Ene Minute
         { 0X84, 1, 1, 0, 1, 0, 0, 0, 2}, // Count String Value
      };

      // User Pattern (Class Code 0x6B)
      public static int[,] UserPattern = new int[,] {
         { 0X64, 1, 1, 0, 0, 0, 0, 0, 1}, // User Pattern Fixed
         { 0X65, 1, 1, 0, 0, 0, 0, 0, 2}, // User Pattern Free
     };

      // Substitution Rules(Class Code 0x6C)
      public static int[,] SubstitutionRules = new int[,] {
         { 0X64, 1, 1, 0, 0, 0, 0, 0, 3}, // Number
         { 0X65, 1, 1, 0, 0, 0, 0, 0, 2}, // Name
         { 0X66, 1, 1, 0, 0, 0, 0, 0, 1}, // Start Year
         { 0X67, 1, 1, 0, 0, 0, 0, 0, 10}, // Year
         { 0X68, 1, 1, 0, 0, 0, 0, 0, 8}, // Month
         { 0X69, 1, 1, 0, 0, 0, 0, 0, 4}, // Day
         { 0X6A, 1, 1, 0, 0, 0, 0, 0, 6}, // Hour
         { 0X6B, 1, 1, 0, 0, 0, 0, 0, 7}, // Minute
         { 0X6C, 1, 1, 0, 0, 0, 0, 0, 9}, // Week
         { 0X6D, 1, 1, 0, 0, 0, 0, 0, 5}, // Day Of Week
      };

      // Enviroment Setting (Class Code 0x71)
      public static int[,] EnviromentSetting = new int[,] {
         { 0X65, 1, 1, 0, 0, 0, 0, 0, 5}, // Current Time
         { 0X66, 1, 1, 0, 0, 0, 0, 0, 1}, // Calendar Date Time
         { 0X67, 1, 1, 0, 0, 0, 0, 0, 2}, // Calendar Date Time Availibility
         { 0X68, 1, 1, 0, 0, 0, 0, 0, 4}, // Clock System
         { 0X69, 0, 1, 0, 0, 0, 0, 0, 8}, // User Environment Information
         { 0X6A, 0, 1, 0, 0, 0, 0, 0, 3}, // Cirulation Control Setting Value
         { 0X6B, 1, 0, 0, 0, 0, 0, 0, 7}, // Usage Time Of Circulation Control
         { 0X6C, 1, 0, 0, 0, 0, 0, 0, 6}, // Reset Usage Time Of Citculation Control
      };

      // Unit Information (Class Code 0x73)
      public static int[,] UnitInformation = new int[,] {
         { 0X64, 0, 1, 0, 0, 0, 0, 0, 20}, // Unit Information
         { 0X6B, 0, 1, 0, 0, 1, 0, 0, 15}, // Model Name
         { 0X6C, 0, 1, 0, 0, 0, 0, 0, 17}, // Serial Number
         { 0X6D, 0, 1, 0, 0, 1, 0, 0, 8}, // Ink Name
         { 0X6E, 0, 1, 0, 0, 0, 0, 0, 9}, // Input Mode
         { 0X6F, 0, 1, 0, 0, 0, 0, 0, 11}, // Maximum Character Count
         { 0X70, 0, 1, 0, 0, 0, 0, 0, 13}, // Maximum Registered Message Count
         { 0X71, 0, 1, 0, 0, 0, 0, 0, 1}, // Barcode Information
         { 0X72, 0, 1, 0, 0, 0, 0, 0, 21}, // Usable Character Size
         { 0X73, 0, 1, 0, 0, 0, 0, 0, 10}, // Maximum Calendar And Count
         { 0X74, 0, 1, 0, 0, 0, 0, 0, 14}, // Maximum Substitution Rule
         { 0X75, 0, 1, 0, 0, 0, 0, 0, 18}, // Shift Code And Time Count
         { 0X76, 0, 1, 0, 0, 0, 0, 0, 3}, // Chimney And DIN Print
         { 0X77, 0, 1, 0, 0, 0, 0, 0, 12}, // Maximum Line Count
         { 0X78, 0, 1, 0, 0, 1, 0, 0, 2}, // Basic Software Version
         { 0X79, 0, 1, 0, 0, 1, 0, 0, 4}, // Controller Software Version
         { 0X7A, 0, 1, 0, 0, 1, 0, 0, 5}, // Engine M Software Version
         { 0X7B, 0, 1, 0, 0, 1, 0, 0, 6}, // Engine S Software Version
         { 0X7C, 0, 1, 0, 0, 1, 0, 0, 7}, // First Language Version
         { 0X7D, 0, 1, 0, 0, 1, 0, 0, 16}, // Second Language Version
         { 0X7E, 0, 1, 0, 0, 1, 0, 0, 19}, // Software Option Version
      };

      // Operation Management (Class Code 0x74)
      public static int[,] OperationManagement = new int[,] {
         { 0X64, 0, 1, 0, 0, 0, 0, 0, 12}, // Operating Management
         { 0X65, 1, 1, 0, 2, 0, 0, 0, 9}, // Ink Operating Time
         { 0X66, 1, 1, 0, 2, 0, 0, 0, 1}, // Alarm Time
         { 0X67, 1, 1, 0, 2, 0, 0, 0, 13}, // Print Count
         { 0X68, 0, 1, 0, 0, 0, 0, 0, 3}, // Communications Environment
         { 0X69, 0, 1, 0, 0, 0, 0, 0, 4}, // Cumulative Operation Time
         { 0X6A, 0, 1, 0, 0, 0, 0, 0, 8}, // Ink And Makeup Name
         { 0X6B, 0, 1, 0, 0, 0, 0, 0, 11}, // Ink Viscosity
         { 0X6C, 0, 1, 0, 0, 0, 0, 0, 10}, // Ink Pressure
         { 0X6D, 0, 1, 0, 0, 0, 0, 0, 2}, // Ambient Temperature
         { 0X6E, 0, 1, 0, 0, 0, 0, 0, 5}, // Deflection Voltage
         { 0X6F, 0, 1, 0, 0, 0, 0, 0, 7}, // Excitation VRef Setup Value
         { 0X70, 0, 1, 0, 0, 0, 0, 0, 6}, // Excitation Frequency
     };

      // IJP Operation (Class Code 0x75)
      public static int[,] IJPOperation = new int[,] {
         { 0X64, 0, 1, 0, 1, 3, 0, 0, 7}, // Remote operation information
         { 0X66, 0, 1, 0, 6, 3, 0, 0, 4}, // Fault and warning history
         { 0X67, 0, 1, 0, 1, 3, 0, 0, 6}, // Operating condition
         { 0X68, 0, 1, 0, 1, 3, 0, 0, 10}, // Warning condition
         { 0X6A, 0, 1, 0, 10, 3, 0, 0, 1}, // Date and time information
         { 0X6B, 0, 1, 0, 1, 3, 0, 0, 3}, // Error code
         { 0X6C, 0, 0, 1, 0, 3, 0, 0, 8}, // Start Remote Operation
         { 0X6D, 0, 0, 1, 0, 3, 0, 0, 9}, // Stop Remote Operation
         { 0X6E, 0, 0, 1, 0, 3, 0, 0, 2}, // Deflection voltage control
         { 0X6F, 1, 1, 0, 1, 0, 0, 1, 5}, // Online Offline
      };

      // Count (Class Code 0x79)
      public static int[,] Count = new int[,] {
         { 0X66, 0, 1, 0, 0, 0, 0, 0, 12}, // Number Of Count Block
         { 0X67, 1, 1, 0, 0, 0, 0, 0, 9}, // Initial Value
         { 0X68, 1, 1, 0, 0, 0, 0, 0, 4}, // Count Range 1
         { 0X69, 1, 1, 0, 0, 0, 0, 0, 5}, // Count Range 2
         { 0X6A, 1, 1, 0, 0, 0, 0, 0, 15}, // Update Unit Halfway
         { 0X6B, 1, 1, 0, 0, 0, 0, 0, 16}, // Update Unit Unit
         { 0X6C, 1, 1, 0, 0, 0, 0, 0, 8}, // Increment Value
         { 0X6D, 1, 1, 0, 0, 0, 0, 0, 7}, // Direction Value
         { 0X6E, 1, 1, 0, 0, 0, 0, 0, 10}, // Jump From
         { 0X6F, 1, 1, 0, 0, 0, 0, 0, 11}, // Jump To
         { 0X70, 1, 1, 0, 0, 0, 0, 0, 13}, // Reset Value
         { 0X71, 1, 1, 0, 0, 0, 0, 0, 14}, // Type Of Reset Signal
         { 0X72, 1, 1, 0, 0, 0, 0, 0, 1}, // Availibility Of External Count
         { 0X73, 1, 1, 0, 0, 0, 0, 0, 2}, // Availibility Of Zero Suppression
         { 0X74, 1, 1, 0, 0, 0, 0, 0, 3}, // Count Multiplier
         { 0X75, 1, 1, 0, 0, 0, 0, 0, 6}, // Count Skip
      };

      // Index (Class Code 0x7A)
      public static int[,] Index = new int[,] {
         { 0X64, 1, 1, 0, 1, 0, 0, 1, 10}, // Start Stop Management Flag
         { 0X65, 1, 1, 0, 1, 0, 0, 1, 1}, // Automatic reflection
         { 0X66, 1, 1, 0, 2, 0, 1, 100, 6}, // Item Count
         { 0X67, 1, 1, 0, 2, 0, 1, 100, 4}, // Column
         { 0X68, 1, 1, 0, 1, 0, 1, 6, 7}, // Line
         { 0X69, 1, 1, 0, 2, 0, 1, 1000, 3}, // Character position
         { 0X6A, 1, 1, 0, 2, 0, 1, 2000, 9}, // Print Data Message Number
         { 0X6B, 1, 1, 0, 1, 0, 1, 99, 8}, // Print Data Group Data
         { 0X6C, 1, 1, 0, 1, 0, 1, 99, 11}, // Substitution Rules Setting
         { 0X6D, 1, 1, 0, 1, 0, 1, 19, 12}, // User Pattern Size
         { 0X6E, 1, 1, 0, 1, 0, 1, 8, 5}, // Count Block
         { 0X6F, 1, 1, 0, 1, 0, 1, 8, 2}, // Calendar Block
      };

   }
}
