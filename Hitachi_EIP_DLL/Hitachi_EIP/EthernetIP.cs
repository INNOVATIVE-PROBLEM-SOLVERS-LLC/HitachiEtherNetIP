using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace EIP_Lib {

   #region Public Enums

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
      GroupChar = 13, // 1 byte group number + UTF8 String + 0x00
      MsgChar = 14,   // 2 byte message number + UTF8 String + 0x00
      N1Char = 15,    // 1-byte number + UTF8 String + 0x00
      N1N1 = 16,      // 2 1-byte numbers
   }

   #endregion

   #region EtherNetIP Definitions

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
      Number_Of_Items = 0x65,
      Number_Of_Columns = 0x66,
      Format_Type = 0x67,
      Insert_Column = 0x69,
      Delete_Column = 0x6A,
      Add_Column = 0x6B,
      Number_Of_Print_Line_And_Print_Format = 0x6C,
      Format_Setup = 0x6D,
      Adding_Print_Items = 0x6E,
      Deleting_Print_Items = 0x6F,
      Print_Character_String = 0x71,
      Line_Count = 0x72,
      Line_Spacing = 0x73,
      Dot_Matrix = 0x74,
      InterCharacter_Space = 0x75,
      Character_Bold = 0x76,
      Barcode_Type = 0x77,
      Readable_Code = 0x78,
      Prefix_Code = 0x79,
      X_and_Y_Coordinate = 0x7A,
      InterCharacter_SpaceII = 0x7B,
      Add_To_End_Of_String = 0x8A,
      Calendar_Offset = 0x8D,
      DIN_Print = 0x8E,
      EAN_Prefix = 0x8F,
      Barcode_Printing = 0x90,
      QR_Error_Correction_Level = 0x91,
   }

   // Attributes within Print Specification class 0x68
   public enum ccPS {
      Character_Height = 0x64,
      Ink_Drop_Use = 0x65,
      High_Speed_Print = 0x66,
      Character_Width = 0x67,
      Character_Orientation = 0x68,
      Print_Start_Delay_Forward = 0x69,
      Print_Start_Delay_Reverse = 0x6A,
      Product_Speed_Matching = 0x6B,
      Pulse_Rate_Division_Factor = 0x6C,
      Speed_Compensation = 0x6D,
      Line_Speed = 0x6E,
      Distance_Between_Print_Head_And_Object = 0x6F,
      Print_Target_Width = 0x70,
      Actual_Print_Width = 0x71,
      Repeat_Count = 0x72,
      Repeat_Interval = 0x73,
      Target_Sensor_Timer = 0x74,
      Target_Sensor_Filter = 0x75,
      Targer_Sensor_Filter_Value = 0x76,
      Ink_Drop_Charge_Rule = 0x77,
      Print_Start_Position_Adjustment_Value = 0x78,
   }

   // Attributes within Calendar class 0x69
   public enum ccCal {
      Shift_Code_Condition = 0x65,
      First_Calendar_Block = 0x66,
      Number_of_Calendar_Blocks = 0x67,
      Offset_Year = 0x68,
      Offset_Month = 0x69,
      Offset_Day = 0x6A,
      Offset_Hour = 0x6B,
      Offset_Minute = 0x6C,
      Zero_Suppress_Year = 0x6D,
      Zero_Suppress_Month = 0x6E,
      Zero_Suppress_Day = 0x6F,
      Zero_Suppress_Hour = 0x70,
      Zero_Suppress_Minute = 0x71,
      Zero_Suppress_Weeks = 0x72,
      Zero_Suppress_Day_Of_Week = 0x73,
      Substitute_Year = 0x74,
      Substitute_Month = 0x75,
      Substitute_Day = 0x76,
      Substitute_Hour = 0x77,
      Substitute_Minute = 0x78,
      Substitute_Weeks = 0x79,
      Substitute_Day_Of_Week = 0x7A,
      Time_Count_Start_Value = 0x7B,
      Time_Count_End_Value = 0x7C,
      Time_Count_Reset_Value = 0x7D,
      Reset_Time_Value = 0x7E,
      Update_Interval_Value = 0x7F,
      Shift_Start_Hour = 0x80,
      Shift_Start_Minute = 0x81,
      Shift_End_Hour = 0x82,
      Shift_End_Minute = 0x83,
      Shift_String_Value = 0x84,
   }

   // Attributes within User Pattern class 0x6B
   public enum ccUP { // 0x6B
      User_Pattern_Fixed = 0x64,
      User_Pattern_Free = 0x65,
   }

   // Attributes within Substitution Rules class 0x6C
   public enum ccSR { // 0x6C
      Number = 0x64,
      Name = 0x65,
      Start_Year = 0x66,
      Year = 0x67,
      Month = 0x68,
      Day = 0x69,
      Hour = 0x6A,
      Minute = 0x6B,
      Week = 0x6C,
      Day_Of_Week = 0x6D,
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
      Reset_Usage_Time_Of_Citculation_Control = 0x6C,
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
      Ink_Operating_Time = 0x65,
      Alarm_Time = 0x66,
      Print_Count = 0x67,
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
      Online_Offline = 0x6F,
   }

   // Attributes within Count class 0x79
   public enum ccCount {
      Number_Of_Count_Block = 0x66,
      Initial_Value = 0x67,
      Count_Range_1 = 0x68,
      Count_Range_2 = 0x69,
      Update_Unit_Halfway = 0x6A,
      Update_Unit_Unit = 0x6B,
      Increment_Value = 0x6C,
      Direction_Value = 0x6D,
      Jump_From = 0x6E,
      Jump_To = 0x6F,
      Reset_Value = 0x70,
      Type_Of_Reset_Signal = 0x71,
      External_Count = 0x72,
      Zero_Suppression = 0x73,
      Count_Multiplier = 0x74,
      Count_Skip = 0x75,
   }

   // Attributes within Index class 0x7A
   public enum ccIDX {
      Start_Stop_Management_Flag = 0x64,
      Automatic_reflection = 0x65,
      Item = 0x66,
      Column = 0x67,
      Line = 0x68,
      Character_position = 0x69,
      Print_Data_Message_Number = 0x6A,
      Print_Data_Group_Data = 0x6B,
      Substitution_Rules_Setting = 0x6C,
      User_Pattern_Size = 0x6D,
      Count_Block = 0x6E,
      Calendar_Block = 0x6F,
   }

   #endregion

   public class EIP {

      #region Events

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(EIP sender, string msg);

      // Read completion
      public event IOHandler IOComplete;
      public delegate void IOHandler(EIP sender, EIPEventArg e);

      // Read completion
      public event ConnectionStateChangedHandler StateChanged;
      public delegate void ConnectionStateChangedHandler(EIP sender, string msg);

      #endregion

      #region Declarations/Properties

      enum Protocol {
         TCP = 6
      }

      enum EIP_Type {
         RegisterSession = 0x0065,
         UnRegisterSession = 0x0066,
         SendRRData = 0x006F,
         SendUnitData = 0x0070,
      }

      enum Data_Type {
         AddressItem = 0xa1,                 // Connected Address Item
         ConnectedDataItem = 0xb1,           // Connected Data Item
         UCDataItem = 0xb2,                  // Unconnected Data Item
      }

      enum EIP_Command {
         Null = 0,
         ForwardOpen = 0x54,
         ForwardClose = 0x4e,
      }

      enum Segment {
         Class = 0x20,
         Instance = 0x24,
         Attribute = 0x30,
      }

      // Data Tables describing Hitachi Model 161
      static public DataII M161 = new DataII();

      // Lookup for getting attributes associated with a Class/Function
      static public Dictionary<ClassCode, byte, AttrData> AttrDict;

      // Class Codes
      static public ClassCode[] ClassCodes = (ClassCode[])Enum.GetValues(typeof(ClassCode));

      // Class Codes to Attributes
      static public Type[] ClassCodeAttributes = new Type[] {
            typeof(ccPDM),   // 0x66 Print data management function
            typeof(ccPF),    // 0x67 Print format function
            typeof(ccPS),    // 0x68 Print specification function
            typeof(ccCal),   // 0x69 Calendar function
            typeof(ccUP),    // 0x6B User pattern function
            typeof(ccSR),    // 0x6C Substitution rules function
            typeof(ccES),    // 0x71 Enviroment setting function
            typeof(ccUI),    // 0x73 Unit Information function
            typeof(ccOM),    // 0x74 Operation management function
            typeof(ccIJP),   // 0x75 IJP operation function
            typeof(ccCount), // 0x79 Count function
            typeof(ccIDX),   // 0x7A Index function
         };

      // Attribute DropDown conversion
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
                        "GS1 DataBar (Stacked)", "DM14x14", },        // 9 - Barcode Types
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
      };

      public int port { get; set; }
      public string IPAddress { get; set; }

      TcpClient client = null;
      NetworkStream stream = null;

      public uint SessionID { get; set; } = 0;

      public AccessCode Access { get; set; }
      public ClassCode Class { get; set; }
      public byte Instance { get; set; } = 1;
      public byte Attribute { get; set; } = 1;
      public uint O_T_ConnectionID { get; set; } = 0;
      public uint T_O_ConnectionID { get; set; } = 0;

      private bool IsConnected {
         get { return client != null && stream != null && client.Connected; }
      }

      public bool SessionIsOpen {
         get { return SessionID > 0; }
      }

      public bool ForwardIsOpen {
         get { return O_T_ConnectionID > 0; }
      }

      public string LastIO { get; set; }

      // Full Read Packet
      public byte[] ReadData;
      public Int32 ReadDataLength;

      public bool LengthIsValid { get; set; }
      public bool DataIsValid { get; set; }

      // User data portion of the packet
      public int GetDataLength { get; set; }
      public byte[] GetData { get; set; }
      public string GetDataValue { get; set; }
      public int GetDecValue { get; set; }
      public string GetStatus { get; set; }

      public int SetDataLength { get; set; } = 0;
      public byte[] SetData { get; set; } = { };
      public string SetDataValue { get; set; }
      public int SetDecValue { get; set; }

      public byte[] Nodata = new byte[0];
      public byte[] Null = new byte[] { 0 };
      public byte[] DataZero = new byte[] { 0 };
      public byte[] DataOne = new byte[] { 1 };
      public byte[] DataTwo = new byte[] { 2 };

      public Encoding Encode = Encoding.UTF8;

      // Flag to avoid constant session open/close if already open
      int OCS = 0;
      bool OpenCloseSession {
         get {
            return (OCS & 1) > 0;
         }
         set {
            if (value) {
               OCS |= 1;
            } else {
               OCS &= ~1;
            }
         }
      }

      int OCF = 0;
      // Flag to avoid constant forward open/close if alread open
      bool OpenCloseForward {
         get {
            return (OCF & 1) > 0;
         }
         set {
            if (value) {
               OCF |= 1;
            } else {
               OCF &= ~1;
            }
         }
      }

      // Save area for Index function values
      int[] IndexValue;
      Byte[] IndexAttr;

      // Status of last request
      bool Successful = false;

      public static readonly object TrafficLock = new object();
      public static Traffic Traffic = null;
      public static int TrafficUsers = 0;


      #endregion

      #region Constructors and Destructors

      public EIP(string IPAddress, int port, string TrafficFolder) {
         // Save caller's parameters
         this.IPAddress = IPAddress;
         this.port = port;

         // Block concurrent use
         lock (TrafficLock) {
            // Build traffic file
            if (string.IsNullOrEmpty(TrafficFolder)) {
               TrafficUsers += 1;
            } else {
               if (Traffic == null) {
                  Traffic = new Traffic(TrafficFolder);
                  TrafficUsers = 1;
                  CreateExcelApp();
               } else {
                  TrafficUsers += 1;
               }
            }
         }
         // Build save area for the ccIDX values
         IndexAttr = ((ccIDX[])Enum.GetValues(typeof(ccIDX))).Select(x => Convert.ToByte(x)).ToArray();
         IndexValue = new int[IndexAttr.Length];
         // Build Dictionary by ClassCode/Enum valus
         BuildAttributeDictionary();
      }

      ~EIP() {

      }

      public void CleanUpTraffic() {
         lock (TrafficLock) {
            TrafficUsers -= 1;
            if (TrafficUsers == 0) {
               CloseExcelFile(false);
               Traffic = null;
            }
         }
      }

      #endregion

      #region Methods

      // Connect to Hitachi printer
      private bool Connect() {
         bool result = false;
         try {
            client = new TcpClient(IPAddress, port);
            stream = client.GetStream();
            result = true;
            LogIt("Connection Open!");
         } catch (Exception e) {
            LogIt(e.Message);
         }
         StateChanged?.Invoke(this, "Connection Changed");
         return result;
      }

      // Set a new IP Address and port before connecting
      private bool Connect(string IPAddress, int port) {
         this.IPAddress = IPAddress;
         this.port = port;
         return (Connect());
      }

      // Disconnect from Hitachi printer
      private bool Disconnect() {
         bool result = false;
         SessionID = 0;
         O_T_ConnectionID = 0;
         T_O_ConnectionID = 0;
         try {
            if (stream != null) {
               stream.Close();
               stream = null;
            }
            if (client != null) {
               client.Close();
               client = null;
            }
            result = true;
            LogIt("Connection Close!");
         } catch (Exception e) {
            LogIt(e.Message);
         }
         StateChanged?.Invoke(this, "Connection Changed");
         return result;
      }

      // Start EtherNet/IP Session
      public bool StartSession() {
         bool successful = true;
         OCS <<= 1;
         if (OpenCloseSession = !SessionIsOpen) {
            SessionID = 1;
            if (Connect()) {
               byte[] ed = EIP_Session(EIP_Type.RegisterSession);
               if (Write(ed, 0, ed.Length) && Read(out byte[] data, out int bytes) && bytes >= 8) {
                  SessionID = (uint)Get(data, 4, 4, mem.LittleEndian);
                  LogIt("Session Open!");
               } else {
                  successful = false;
               }
            } else {
               successful = false;
            }
            if (!successful) {
               LogIt("Session Start Failed!");
               Disconnect();
            }
            StateChanged?.Invoke(this, "Session Changed");
         }
         return successful;
      }

      // End EtherNet/IP Session
      public void EndSession() {
         if (OpenCloseSession) {
            if (SessionIsOpen) {
               SessionID = 0;
               if (client.Connected) {
                  byte[] ed = EIP_Session(EIP_Type.UnRegisterSession, SessionID);
                  Write(ed, 0, ed.Length);
               }
               LogIt("Session Close!");
               Disconnect();
               StateChanged?.Invoke(this, "Session Changed");
            }
         }
         OCS >>= 1;
      }

      // Opens a Data Forwarding path to the printer.
      public bool ForwardOpen() {
         bool successful = true;
         OCF <<= 1;
         if (OpenCloseForward = !ForwardIsOpen) {
            byte[] data;
            Int32 bytes;
            O_T_ConnectionID = 0;
            T_O_ConnectionID = 0;
            byte[] ed = EIP_Wrapper(EIP_Type.SendRRData, EIP_Command.ForwardOpen);
            if (Write(ed, 0, ed.Length) && Read(out data, out bytes) && bytes >= 52) {
               O_T_ConnectionID = (uint)Get(data, 44, 4, mem.LittleEndian);
               T_O_ConnectionID = (uint)Get(data, 48, 4, mem.LittleEndian);
               LogIt("Forward Open!");
            } else {
               successful = false;
               LogIt("Forward Open Failed!");
            }
         }
         StateChanged?.Invoke(this, "Forward Changed");
         return successful;
      }

      // End EtherNet/IP Forward Open
      public void ForwardClose() {
         if (OpenCloseForward) {
            O_T_ConnectionID = 0;
            T_O_ConnectionID = 0;
            byte[] ed = EIP_Wrapper(EIP_Type.SendRRData, EIP_Command.ForwardClose);
            if (Write(ed, 0, ed.Length) && Read(out byte[] data, out int bytes)) {
               LogIt("Forward Close!");
            } else {
               LogIt("Forward Close Failed!");
            }
            StateChanged?.Invoke(this, "Forward Changed");
         }
         OCF >>= 1;
      }

      // Read response to EtherNet/IP request
      private bool Read(out byte[] data, out int bytes) {
         bool successful = false;
         data = new byte[10000];
         bytes = -1;
         if (stream != null) {
            try {
               // Allow for up to 5 seconds for a response
               stream.ReadTimeout = 15000;
               bytes = stream.Read(data, 0, data.Length);
               successful = bytes >= 0;
            } catch (IOException e) {
               LogIt(e.Message);
            }
         }
         if (!successful) {
            LogIt("Read Failed.");
            //Disconnect();
         }
         return successful;
      }

      // Issue EtherNet/IP request
      private bool Write(byte[] data, int start, int length) {
         bool successful = false;
         if (stream != null) {
            try {
               stream.Write(data, start, length);
               successful = true;
            } catch (IOException e) {
               LogIt(e.Message);
            }
         }
         if (!successful) {
            LogIt("Write Failed. Connection Closed!");
            Disconnect();
         }
         return successful;
      }

      // Read one attribute
      public bool GetAttribute(ClassCode Class, byte Attribute, byte[] DataOut) {
         Successful = false;
         if (StartSession()) {
            if (ForwardOpen()) {
               AttrData attr = SetRequest(AccessCode.Get, Class, 0x01, Attribute, DataOut);
               LengthIsValid = false;
               DataIsValid = false;
               int n = EIP_GetSetSrv(EIP_Type.SendUnitData, AccessCode.Get, Class, 0x01, Attribute, DataOut);
               // Write the request and read the response
               if (Write(GetSetSrvPkt, 0, n) && Read(out ReadData, out ReadDataLength)) {
                  InterpretResult(ReadData, ReadDataLength);
                  LengthIsValid = CountIsValid(attr.Data, GetData);
                  DataIsValid = TextIsValid(attr.Data, GetData);
                  GetDataValue = FormatResult(attr.Data.Fmt, GetData);
                  if (Class == ClassCode.Index) {
                     // reflect any changes back to the Index Function
                     int i = Array.FindIndex(IndexAttr, x => x == Attribute);
                     IndexValue[i] = (int)Get(GetData, 0, GetDataLength, mem.BigEndian);
                  }
                  Successful = true;
               } else {
                  int status = (int)Get(ReadData, 48, 2, mem.LittleEndian);
                  GetStatus = $"?? -- {status:X2} -- {LastIO}";
                  GetData = new byte[0];
                  Successful = false;
               }
               // Record the operation in the Traffic file
               Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddTraffic, GetTraffic(attr)));

               IOComplete?.Invoke(this, new EIPEventArg(AccessCode.Get, Class, 0x01, Attribute, Successful));
            }
            ForwardClose();
         }
         EndSession();
         return Successful;
      }

      // Get one attribute based on the Data Property
      public bool GetAttribute<T>(T Attribute, out int value) where T : Enum {
         AttrData attr = GetAttrData(Attribute);
         bool success = GetAttribute(attr.Class, attr.Val, Nodata);
         value = GetDecValue;
         return success;
      }

      // Get the contents of one attribute
      public bool GetAttribute<T>(T Attribute, out string value) where T : Enum {
         AttrData attr = GetAttrData(Attribute);
         bool success = GetAttribute(attr.Class, attr.Val, Nodata);
         value = attr.Data.Fmt == DataFormats.UTF8 ? FromQuoted(GetDataValue) : "";
         return success;
      }

      // Write one attribute
      public bool SetAttribute(ClassCode Class, byte Attribute, byte[] DataOut) {
         Successful = false;
         if (StartSession()) {
            if (ForwardOpen()) {
               AttrData attr = SetRequest(AccessCode.Set, Class, 0x01, Attribute, DataOut);
               int n = EIP_GetSetSrv(EIP_Type.SendUnitData, AccessCode.Set, Class, 0x01, Attribute, DataOut);
               // Write the request and read the response
               if (Write(GetSetSrvPkt, 0, n) && Read(out ReadData, out ReadDataLength)) {
                  InterpretResult(ReadData, ReadDataLength);
                  LengthIsValid = CountIsValid(attr.Set, SetData);
                  DataIsValid = TextIsValid(attr.Data, SetData);
                  if (Class == ClassCode.Index) {
                     // reflect any changes back to the Index Function
                     int i = Array.FindIndex(IndexAttr, x => x == Attribute);
                     IndexValue[i] = (int)Get(SetData, 0, SetDataLength, mem.BigEndian);
                  }
                  Successful = true;
               } else {
                  int status = (int)Get(ReadData, 48, 2, mem.LittleEndian);
                  GetStatus = $"?? -- {status:X2} -- {LastIO}";
                  GetData = new byte[0];
               }
               // Record the operation in the Traffic file
               Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddTraffic, GetTraffic(attr)));

               IOComplete?.Invoke(this, new EIPEventArg(AccessCode.Set, Class, 0x01, Attribute, Successful));
            }
            ForwardClose();
         }
         EndSession();
         return Successful;
      }

      // Set one attribute based on the Set Property
      public bool SetAttribute<T>(T Attribute, int n) where T : Enum {
         byte[] data;
         AttrData attr = GetAttrData(Attribute);
         if (attr.Set.Fmt == DataFormats.UTF8) {
            data = FormatOutput(attr.Set, n.ToString());
         } else {
            data = FormatOutput(attr.Set, n);
         }
         return SetAttribute(attr.Class, attr.Val, data);
      }

      // Set one attribute based on the Set Property
      public bool SetAttribute<T>(T Attribute, string s) where T : Enum {
         AttrData attr = GetAttrData(Attribute);
         byte[] data = FormatOutput(attr.Set, s);
         return SetAttribute(attr.Class, attr.Val, data);
      }

      // Set one attribute based on the Set Property
      public bool SetAttribute<T>(T Attribute, int item, string s) {
         ClassCode cc = ClassCodes[Array.IndexOf(ClassCodeAttributes, typeof(T))];
         byte at = Convert.ToByte(Attribute);
         AttrData attr = AttrDict[cc, at];
         byte[] data = FormatOutput(attr.Set, item, 1, s);
         return SetAttribute(cc, at, data);
      }

      // Service one attribute
      public bool ServiceAttribute(ClassCode Class, byte Attribute, byte[] DataOut) {
         Successful = false;
         if (StartSession()) {
            if (ForwardOpen()) {
               GetDataValue = string.Empty;
               SetDataValue = string.Empty;
               AttrData attr = SetRequest(AccessCode.Service, Class, 0x01, Attribute, DataOut);
               int n = EIP_GetSetSrv(EIP_Type.SendUnitData, AccessCode.Service, Class, 0x01, Attribute, DataOut);
               // Write the request and read the response
               if (Write(GetSetSrvPkt, 0, n) && Read(out ReadData, out ReadDataLength)) {
                  InterpretResult(ReadData, ReadDataLength);
                  LengthIsValid = CountIsValid(attr.Service, SetData);
                  DataIsValid = TextIsValid(attr.Data, SetData);
                  Successful = true;
               } else {
                  int status = (int)Get(ReadData, 48, 2, mem.LittleEndian);
                  GetStatus = $"?? -- {status:X2} -- {LastIO}";
                  GetData = new byte[0];
               }
               // Record the operation in the Traffic file
               Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddTraffic, GetTraffic(attr)));

               IOComplete?.Invoke(this, new EIPEventArg(AccessCode.Service, Class, 0x01, Attribute, Successful));
            }
            ForwardClose();
         }
         EndSession();
         return Successful;
      }

      // Service one attribute based on the Set Property
      public bool ServiceAttribute<T>(T Attribute, int n) {
         ClassCode cc = ClassCodes[Array.IndexOf(ClassCodeAttributes, typeof(T))];
         byte at = Convert.ToByte(Attribute);
         AttrData attr = AttrDict[cc, at];
         byte[] data = ToBytes(n, attr.Service.Len);
         return ServiceAttribute(cc, at, data);
      }

      // Service one attribute based on the Set Property
      public bool ServiceAttribute<T>(T Attribute) {
         ClassCode cc = ClassCodes[Array.IndexOf(ClassCodeAttributes, typeof(T))];
         byte at = Convert.ToByte(Attribute);
         AttrData attr = AttrDict[cc, at];
         return ServiceAttribute(cc, at, DataZero);
      }

      // Handles Hitachi Get, Set, and Service
      byte[] GetSetSrvPkt = null;
      private int EIP_GetSetSrv(EIP_Type t, AccessCode c, ClassCode Class, byte Instance, byte Attribute, byte[] DataOut) {
         if (GetSetSrvPkt == null) {
            List<byte> packet = new List<byte>();
            Add(packet, (ulong)t, 2);                              // 00-01 Command
            Add(packet, (ulong)(30 + DataOut.Length), 2);          // 02-03 Length of added data at end
            Add(packet, (ulong)SessionID, 4);                      // 04-07 Session ID
            Add(packet, (ulong)0, 4);                              // 08-11 Success
            Add(packet, (ulong)1, 8);                              // 12-19 Sender Context
            Add(packet, (ulong)0, 4);                              // 20-23 option flags
            Add(packet, (ulong)0, 4);                              // 24-27 option interface handle
            Add(packet, (ulong)30, 2);                             // 28-29 Timeout
            Add(packet, (ulong)2, 2);                              // 30-31 Item count

            // Item #1
            Add(packet, (ulong)Data_Type.AddressItem, 2);          // 32-33 Connected address type
            Add(packet, (ulong)4, 2);                              // 34-35 length of 4
            Add(packet, O_T_ConnectionID, 4);                      // 36-39 O->T Connection ID

            // Item #2
            Add(packet, (ulong)Data_Type.ConnectedDataItem, 2);    // 40-41 data type
            Add(packet, (ulong)(10 + DataOut.Length), 2);          // 42-43 length of 10 + data length
            Add(packet, (ulong)2, 2);                              // 44-45 Count Sequence
            Add(packet, (byte)c, (byte)3);                         // 46-47 Hitachi command and count
            Add(packet, (byte)Segment.Class, (byte)Class);         // 48-49 Class
            Add(packet, (byte)Segment.Instance, (byte)Instance);   // 50-51 Instance
            Add(packet, (byte)Segment.Attribute, (byte)Attribute); // 52-53 Attribute
            Add(packet, DataOut);                                  // 54... Data

            GetSetSrvPkt = new byte[2000];
            for (int i = 0; i < packet.Count; i++) {
               GetSetSrvPkt[i] = packet[i];
            }
            return 54 + DataOut.Length;
         }
         // Fill in the multi-byte variable data
         Set(GetSetSrvPkt, (uint)t, 0, 2);
         Set(GetSetSrvPkt, (uint)(30 + DataOut.Length), 2, 2);
         Set(GetSetSrvPkt, SessionID, 4, 4);
         Set(GetSetSrvPkt, O_T_ConnectionID, 36, 4);
         Set(GetSetSrvPkt, (uint)(10 + DataOut.Length), 42, 2);

         // Fill in the Single byte variable data
         GetSetSrvPkt[46] = (byte)c;
         GetSetSrvPkt[49] = (byte)Class;
         GetSetSrvPkt[51] = Instance;
         GetSetSrvPkt[53] = Attribute;

         // Add in the extra data
         Buffer.BlockCopy(DataOut, 0, GetSetSrvPkt, 54, DataOut.Length);

         return 54 + DataOut.Length;
      }

      // Shortcut building of session start/end packets
      byte[] SessionPkt = null;
      private byte[] EIP_Session(EIP_Type t, uint SessionID = 0) {
         if (SessionPkt == null) {
            List<byte> packet = new List<byte>();
            Add(packet, (ulong)t, 2);           // 00-01 Command
            Add(packet, (ulong)4, 2);           // 02-03 Length of added data at end
            Add(packet, (ulong)0, 4);           // 04-07 Session ID (Not set yet)
            Add(packet, (ulong)0, 4);           // 08-11 Status to be returned
            Add(packet, (ulong)0, 8);           // 12-19 Sender Context
            Add(packet, (ulong)0, 4);           // 20-23 Options
            Add(packet, (ulong)1, 2);           // 24-25 Protocol Version
            Add(packet, (ulong)0, 2);           // 26-27 Flags
            SessionPkt = packet.ToArray<byte>();
         }
         Set(SessionPkt, (uint)t, 0, 2);     // Set the command
         Set(SessionPkt, SessionID, 4, 4);   // Set the session ID
         return SessionPkt;
      }

      // handles Send RR Data(Forward Open, Forward Close)
      byte[] EIPForwardOpen = null;
      byte[] EIPForwardClose = null;
      private byte[] EIP_Wrapper(EIP_Type t, EIP_Command c) {
         List<byte> packet = new List<byte>();
         switch (c) {
            case EIP_Command.ForwardOpen:
               // Has a packet already been built?
               if (EIPForwardOpen == null) {
                  Add(packet, (ulong)t, 2);                    // 00-01 Command
                  Add(packet, (ulong)62, 2);                   // 02-03 Length of added data at end
                  Add(packet, (ulong)SessionID, 4);            // 04-07 Session ID
                  Add(packet, (ulong)0, 4);                    // 08-11 Success
                  Add(packet, (ulong)0, 8);                    // 12-19 Pertinent to sender
                  Add(packet, (ulong)0, 4);                    // 20-23 option flags
                  Add(packet, (ulong)0, 4);                    // 24-27 option interface handle
                  Add(packet, (ulong)255, 2);                  // 28-29 Timeout
                  Add(packet, (ulong)2, 2);                    // 30-31 Item count
                  Add(packet, (ulong)0, 2);                    // 32-33 Null type
                  Add(packet, (ulong)0, 2);                    // 34-35 length of 0
                  Add(packet, (ulong)Data_Type.UCDataItem, 2); // 36-37 data type
                  Add(packet, (ulong)46, 2);                   // 38-39 length of 46
                  Add(packet, (byte)c, (byte)2);               // 40-41 Forward open/Packet size
                  Add(packet, (byte)Segment.Class, (byte)6);   // 42-43 Class
                  Add(packet, (byte)Segment.Instance, (byte)1);// 44-45 Instance

                  // Common Packet
                  Add(packet, (ulong)7, 1);                    // 46    Priority/Time
                  Add(packet, (ulong)0xea, 1);                 // 47    Timeout Ticks
                  Add(packet, (ulong)0, 4);                    // 48-51 O->T Connection ID Random Number
                  Add(packet, (ulong)0x77, 4);                 // 52-55 T->O Connection ID Random number
                  Add(packet, (ulong)0x98, 2);                 // 56-57 Connection Serial Number random number
                  Add(packet, (ulong)0, 2);                    // 58-59 vendor ID
                  Add(packet, (ulong)0, 4);                    // 60-63 Originator Serial number
                  Add(packet, (ulong)0, 1);                    // 64    Connection Timeout Multiplier
                  Add(packet, (ulong)0, 3);                    // 65-67 Reserved

                  Add(packet, 10000000, 4);                    // 68-71 O->T RPI
                  Add(packet, 0xff, 0x43);                     // 72-73 O->T Network Connection Parameters
                  Add(packet, 10000000, 4);                    // 74-77 T->O RPI
                  Add(packet, 0xff, 0x43);                     // 78-79 T->O Network Connection Parameters
                  Add(packet, (ulong)0xa3, 1);                 // 80    Transport type/trigger
                  Add(packet, (ulong)2, 1);                    // 81    Connection Path Size in 16-bit words
                  Add(packet, (byte)Segment.Class, (byte)2);   // 82-83 Class
                  Add(packet, (byte)Segment.Instance, (byte)1);// 84-85 Instance

                  EIPForwardOpen = packet.ToArray<byte>();
                  return EIPForwardOpen;
               }
               Set(EIPForwardOpen, SessionID, 4, 4);
               return EIPForwardOpen;
            case EIP_Command.ForwardClose:
               // Has a packet already been built?
               if (EIPForwardClose == null) {
                  Add(packet, (ulong)t, 2);                       // 00-01 Command
                  Add(packet, (ulong)34, 2);                      // 02-03 Length of added data at end
                  Add(packet, (ulong)SessionID, 4);               // 04-07 Session ID
                  Add(packet, (ulong)0, 4);                       // 08-11 Success
                  Add(packet, (ulong)0, 8);                       // 12-19 Pertinant to sender(Unknown for now)
                  Add(packet, (ulong)0, 4);                       // 20-23 option flags
                  Add(packet, (ulong)0, 4);                       // 24-27 option interface handle
                  Add(packet, (ulong)30, 2);                      // 28-29 Timeout
                  Add(packet, (ulong)2, 2);                       // 30-31 Item count
                  Add(packet, (ulong)0, 2);                       // 32-33 Null type
                  Add(packet, (ulong)0, 2);                       // 34-35 length of 0
                  Add(packet, (ulong)Data_Type.UCDataItem, 2);    // 36-37 data type
                  Add(packet, (ulong)18, 2);                      // 38-39 length of 46
                  Add(packet, (byte)c, (byte)2);                  // 40-41 Forward close
                  Add(packet, (byte)Segment.Class, (byte)6);      // 42-43 Class
                  Add(packet, (byte)Segment.Instance, (byte)1);   // 44-45 Instance

                  // Common Packet
                  Add(packet, (ulong)7, 1);                       // 46    Priority/Time
                  Add(packet, (ulong)0xea, 1);                    // 47    Timeout Ticks
                  Add(packet, (ulong)0x98, 2);                    // 48-49 Connection Serial Number random number
                  Add(packet, (ulong)0, 2);                       // 50-51 vendor ID
                  Add(packet, (ulong)0, 4);                       // 52-53 Originator Serial number
                  Add(packet, (ulong)0, 1);                       // 54-55 Connection Path Size
                  Add(packet, (ulong)0, 1);                       // 56-57 Reserved

                  EIPForwardClose = packet.ToArray<byte>();
                  return EIPForwardClose;
               }
               Set(EIPForwardClose, SessionID, 4, 4);
               return EIPForwardClose;
         }
         return null;
      }

      // get the human readable name
      public string GetAttributeName(Type cc, int v) {
         string result = Enum.GetName(cc, v);
         return result;
      }

      // Get an unsigned int from up to 4 consecutive bytes
      public long Get(byte[] b, int start, int length, mem m) {
         uint result = 0;
         switch (m) {
            case mem.BigEndian:
               for (int i = 0; i < length; i++) {
                  result <<= 8;
                  result += b[start + i];
               }
               break;
            case mem.LittleEndian:
               for (int i = 0; i < length; i++) {
                  result += (uint)b[start + i] << (8 * i);
               }
               break;
            default:
               break;
         }
         return result;
      }

      // Get data array as xx xx xx xx ...
      public string GetBytes(byte[] data, int start, int length) {
         string s = string.Empty;
         for (int i = 0; i < Math.Min(length, data.Length - start); i++) {
            s += $"{data[start + i]:X2} ";
         }
         return s;
      }

      // Get data as UTF8 characters
      public string GetUTF8(byte[] data, int start, int length) {
         return ToQuoted(Encode.GetString(data, 0, length));
      }

      // Convert unsigned integer to byte array
      public byte[] ToBytes(long v, int length, mem order = mem.BigEndian) {
         byte[] result = new byte[length];
         switch (order) {
            case mem.BigEndian:
               for (int i = length - 1; i >= 0; i--) {
                  result[i] = (byte)(v & 0xFF);
                  v >>= 8;
               }
               break;
            case mem.LittleEndian:
               for (int i = 0; i < length; i++) {
                  result[i] = (byte)(v & 0xFF);
                  v >>= 8;
               }
               break;
         }
         return result;
      }

      // Convert UTF8 string to bytes
      public byte[] ToBytes(string v) {
         return Encode.GetBytes(v);
      }

      // Format input byte array to readable characters
      public void SetBackColor(Prop prop, AttrData attr, TextBox count, TextBox text, ComboBox dropdown) {
         count.Text = GetDataLength.ToString();
         count.BackColor = LengthIsValid ? Color.LightGreen : Color.Pink;
         text.BackColor = DataIsValid ? Color.LightGreen : Color.Pink;
         text.Text = GetDataValue;
         if (attr.Data.DropDown != fmtDD.None) {
            if (long.TryParse(GetDataValue, out long val)) {
               if (val >= prop.Min && val <= prop.Max) {
                  dropdown.SelectedIndex = (int)(val - prop.Min);
                  dropdown.BackColor = Color.LightGreen;
                  dropdown.Visible = true;
                  text.Visible = false;
               } else {
                  dropdown.Visible = false;
                  text.Visible = true;
               }
            } else {
               dropdown.Visible = false;
               text.Visible = true;
            }
         }
      }

      // Format Output
      public byte[] FormatOutput(Prop prop, int val, int n, string s) {
         string t = FromQuoted(s);
         t = t.Substring(0, Math.Min(prop.Len, t.Length));
         SetDataValue = $"{val},{s}" ;
         return Merge(ToBytes(val, n), ToBytes(t + "\x00"));
      }

      // Format Output
      public byte[] FormatOutput(Prop prop, int n) {
         SetDecValue = n;
         SetDataValue = n.ToString();
         return ToBytes(n, prop.Len);
      }

      // Format output
      public byte[] FormatOutput(Prop prop, TextBox t, ComboBox c, AttrData attr) {
         if (attr.Data.DropDown != fmtDD.None && c.Visible) {
            int n = (int)(c.SelectedIndex + prop.Min);
            SetDecValue = n;
            SetDataValue = n.ToString();
            return ToBytes(n, prop.Len);
         } else {
            return FormatOutput(prop, t.Text);
         }
      }

      // Format output
      public byte[] FormatOutput(Prop prop, string s) {
         if (prop.Len == 0) {
            return Nodata;
         }
         int val;
         byte[] result = null;
         string[] sa;
         SetDataValue = s;
         SetDecValue = -1;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
            case DataFormats.SDecimal:
            case DataFormats.DecimalLE:
            case DataFormats.SDecimalLE:
               if (int.TryParse(s, out val)) {
                  if (prop.Fmt == DataFormats.Decimal || prop.Fmt == DataFormats.SDecimal) {
                     result = ToBytes(val, prop.Len);
                  } else {
                     result = ToBytes(val, prop.Len, mem.LittleEndian);
                  }
               } else if (bool.TryParse(s, out bool b)) {
                  val = b ? 1 : 0;
                  result = ToBytes(val, prop.Len);
               } else {
                  // Translate dropdown back to a number
                  if (prop.DropDown != fmtDD.None) {
                     s = s.ToLower();
                     val = Array.FindIndex(DropDowns[(int)prop.DropDown], x => x.ToLower().Contains(s));
                     if (val >= 0) {
                        val += (int)prop.Min;
                        result = ToBytes(val, prop.Len);
                     }
                  }
               }
               SetDecValue = val;
               break;
            case DataFormats.UTF8:
               if (s.Length > 1 && s.StartsWith("\"") && s.EndsWith("\"")) {
                  SetDataValue = s;
                  result = Encode.GetBytes($"{FromQuoted(s)}\x00");
               } else {
                  SetDataValue = ToQuoted(s);
                  result = Encode.GetBytes($"{s}\x00");
               }
               break;
            case DataFormats.UTF8N:
               if (s.Length > 1 && s.StartsWith("\"") && s.EndsWith("\"")) {
                  SetDataValue = s;
                  result = Encode.GetBytes(FromQuoted(s));
               } else {
                  SetDataValue = ToQuoted(s);
                  result = Encode.GetBytes(s);
               }
               break;
            case DataFormats.Date:
               if (DateTime.TryParse(s, out DateTime d)) {
                  byte[] year = ToBytes(d.Year, 2, mem.LittleEndian);
                  byte[] month = ToBytes(d.Month, 2, mem.LittleEndian);
                  byte[] day = ToBytes(d.Day, 2, mem.LittleEndian);
                  byte[] hour = ToBytes(d.Hour, 2, mem.LittleEndian);
                  byte[] minute = ToBytes(d.Minute, 2, mem.LittleEndian);
                  byte[] second = ToBytes(d.Second, 2, mem.LittleEndian);
                  result = Merge(year, month, day, hour, minute, second);
               }
               break;
            case DataFormats.Bytes:
               sa = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               result = new byte[prop.Len];
               for (int i = 0; i < Math.Min(sa.Length, prop.Len); i++) {
                  if (int.TryParse(sa[i], System.Globalization.NumberStyles.HexNumber, null, out int n)) {
                     result[i] = (byte)n;
                  }
               }
               break;
            case DataFormats.XY:
               sa = s.Split(',');
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint x) && uint.TryParse(sa[1].Trim(), out uint y)) {
                     result = Merge(ToBytes(x, 2), ToBytes(y, 1));
                  }
               }
               break;
            case DataFormats.N1N1:
               sa = s.Split(',');
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint n1) && uint.TryParse(sa[1].Trim(), out uint n2)) {
                     result = Merge(ToBytes(n1, 1), ToBytes(n2, 1));
                  }
               }
               break;
            case DataFormats.N2N2:
               sa = s.Split(',');
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint n1) && uint.TryParse(sa[1].Trim(), out uint n2)) {
                     result = Merge(ToBytes(n1, 2), ToBytes(n2, 2));
                  }
               }
               break;
            case DataFormats.ItemChar:
               result = Merge(ToBytes(GetIndexSetting(ccIDX.Item), 1), Encode.GetBytes(FromQuoted(s) + "\x00"));
               SetDataValue = $"{GetIndexSetting(ccIDX.Item)}, {s}";
               break;
            case DataFormats.GroupChar:
               result = Merge(ToBytes(GetIndexSetting(ccIDX.Print_Data_Group_Data), 1), Encode.GetBytes(FromQuoted(s) + "\x00"));
               break;
            case DataFormats.MsgChar:
               result = Merge(ToBytes(GetIndexSetting(ccIDX.Print_Data_Message_Number), 2), Encode.GetBytes(FromQuoted(s) + "\x00"));
               break;
            case DataFormats.N2Char:
               sa = s.Split(new char[] { ',' }, 2);
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint n)) {
                     result = Merge(ToBytes(n, 2), Encode.GetBytes(sa[1] + "\x00"));
                  }
               }
               break;
            case DataFormats.N1Char:
               sa = s.Split(new char[] { ',' }, 2);
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint n)) {
                     result = Merge(ToBytes(n, 1), Encode.GetBytes(sa[1] + "\x00"));
                  }
               }
               break;
            case DataFormats.Item:
               result = ToBytes(GetIndexSetting(ccIDX.Item), 1);
               SetDataValue = result[0].ToString();
               break;
         }
         if (result == null) {
            result = new byte[0];
         }
         return result;
      }

      // Convert string to quoted string
      public string ToQuoted(string s) {
         return $"\"{s.Replace("\"", "\"\"")}\"";
      }

      // Convert quoted string to string
      public string FromQuoted(string s) {
         if (s.Length > 1 && s.StartsWith("\"") && s.EndsWith("\"")) {
            return s.Substring(1, s.Length - 2).Replace("\"\"", "\"");
         } else {
            return s;
         }
      }

      // Does count agree with Hitachi Document?
      public bool CountIsValid(Prop prop, byte[] data) {
         bool IsValid = false;
         int n;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
            case DataFormats.DecimalLE:
            case DataFormats.SDecimal:
            case DataFormats.SDecimalLE:
            case DataFormats.Date:
            case DataFormats.Bytes:
            case DataFormats.XY:
            case DataFormats.N1N1:
            case DataFormats.N2N2:
               IsValid = prop.Len == data.Length;
               break;
            case DataFormats.N1Char:
            case DataFormats.N2Char:
            case DataFormats.UTF8:
            case DataFormats.UTF8N:
               IsValid = prop.Len >= data.Length;
               break;
            case DataFormats.ItemChar:
            case DataFormats.Item:
               n = (int)GetIndexSetting(ccIDX.Item);
               IsValid = n >= prop.Min && n <= prop.Max;
               break;
            case DataFormats.GroupChar:
               n = (int)GetIndexSetting(ccIDX.Print_Data_Group_Data);
               IsValid = n >= prop.Min && n <= prop.Max;
               break;
            case DataFormats.MsgChar:
               n = (int)GetIndexSetting(ccIDX.Print_Data_Message_Number);
               IsValid = n >= prop.Min && n <= prop.Max;
               break;
            default:
               break;
         }
         return IsValid;
      }

      // Does text agree with Hitachi Document?
      public bool TextIsValid(Prop prop, byte[] data) {
         bool IsValid = false;
         int i;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
            case DataFormats.SDecimal:
               if (data.Length <= 8) {
                  long dec = Get(data, 0, data.Length, mem.BigEndian);
                  if (prop.Fmt == DataFormats.SDecimal) {
                     // Sign extend the number
                     dec <<= (64 - data.Length * 8);
                     dec >>= (64 - data.Length * 8);
                  }
                  IsValid = prop.Max == 0 || dec >= prop.Min && dec <= prop.Max;
               }
               break;
            case DataFormats.DecimalLE:
            case DataFormats.SDecimalLE:
               if (data.Length <= 8) {
                  long dec = (uint)Get(data, 0, data.Length, mem.LittleEndian);
                  if (prop.Fmt == DataFormats.SDecimalLE) {
                     // Sign extend the number
                     dec <<= (64 - data.Length * 8);
                     dec >>= (64 - data.Length * 8);
                  }
                  IsValid = prop.Max == 0 || dec >= prop.Min && dec <= prop.Max;
               }
               break;
            case DataFormats.UTF8:
            case DataFormats.UTF8N:
               IsValid = true;
               break;
            case DataFormats.Date:
               if (data.Length == 12) {
                  IsValid = DateTime.TryParse(FormatResult(prop.Fmt, data), out DateTime d);
               }
               break;
            case DataFormats.Bytes:
               IsValid = prop.Len == data.Length;
               break;
            case DataFormats.XY:
               if (prop.Len == data.Length) {
                  long x = Get(data, 0, 2, mem.BigEndian);
                  long y = Get(data, 2, 1, mem.BigEndian);
                  IsValid = x <= 65535 && y <= 47;
               }
               break;
            case DataFormats.N1N1:
               if (prop.Len == data.Length) {
                  long n1 = Get(data, 0, 1, mem.LittleEndian);
                  long n2 = Get(data, 1, 1, mem.LittleEndian);
                  IsValid = n1 >= prop.Min && n1 <= prop.Max && n2 >= prop.Min && n2 <= prop.Max;
               }
               break;
            case DataFormats.N2N2:
               if (prop.Len == data.Length) {
                  long n1 = Get(data, 0, 2, mem.LittleEndian);
                  long n2 = Get(data, 2, 2, mem.LittleEndian);
                  IsValid = n1 >= prop.Min && n1 <= prop.Max && n2 >= prop.Min && n2 <= prop.Max;
               }
               break;
            case DataFormats.ItemChar:
            case DataFormats.Item:
               i = (int)GetIndexSetting(ccIDX.Item);
               IsValid = i >= prop.Min && i <= prop.Max;
               break;
            case DataFormats.GroupChar:
               i = (int)GetIndexSetting(ccIDX.Print_Data_Group_Data);
               IsValid = i >= prop.Min && i <= prop.Max;
               break;
            case DataFormats.MsgChar:
               i = (int)GetIndexSetting(ccIDX.Print_Data_Message_Number);
               IsValid = i >= prop.Min && i <= prop.Max;
               break;
            case DataFormats.N1Char:
               if (data.Length > 1) {
                  long n = Get(data, 0, 1, mem.LittleEndian);
                  IsValid = n >= prop.Min && n <= prop.Max;
               }
               break;
            case DataFormats.N2Char:
               if (data.Length > 1) {
                  long n = Get(data, 0, 2, mem.LittleEndian);
                  IsValid = n >= prop.Min && n <= prop.Max;
               }
               break;
         }
         return IsValid;
      }

      // Does text agree with Hitachi Document?
      public bool TextIsValid(Prop prop, string s) {
         bool IsValid = false;
         int i;
         string[] gp;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
            case DataFormats.DecimalLE:
            case DataFormats.SDecimal:
            case DataFormats.SDecimalLE:
               if (long.TryParse(s, out long dec)) {
                  IsValid = prop.Max == 0 || dec >= prop.Min && dec <= prop.Max;
               }
               break;
            case DataFormats.UTF8:
            case DataFormats.UTF8N:
               IsValid = FromQuoted(s).Length <= prop.Len;
               break;
            case DataFormats.Date:
               IsValid = DateTime.TryParse(s, out DateTime d);
               break;
            case DataFormats.Bytes:
               string[] b = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               for (i = 0; i < b.Length; i++) {
                  if (int.TryParse(b[i], out int n)) {
                     if (n < 0 || n > 255) {
                        break;
                     }
                  } else {
                     break;
                  }
               }
               break;
            case DataFormats.XY:
               string[] xy = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
               if (xy.Length == 2) {
                  if (!int.TryParse(xy[0].Trim(), out int x)) {
                     break;
                  }
                  if (!int.TryParse(xy[1].Trim(), out int y)) {
                     break;
                  }
                  IsValid = x <= 65535 && y <= 47;
               }
               break;
            case DataFormats.N1N1:
            case DataFormats.N2N2:
               string[] n1n2 = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
               if (n1n2.Length == 2) {
                  if (!int.TryParse(n1n2[0].Trim(), out int x)) {
                     break;
                  }
                  if (!int.TryParse(n1n2[1].Trim(), out int y)) {
                     break;
                  }
                  IsValid = x <= prop.Max && y <= prop.Max;
               }
               break;
            case DataFormats.ItemChar:
               i = (int)GetIndexSetting(ccIDX.Item);
               IsValid = i >= prop.Min && i <= prop.Max && FromQuoted(s).Length <= prop.Len;
               break;
            case DataFormats.GroupChar:
               i = (int)GetIndexSetting(ccIDX.Print_Data_Group_Data);
               IsValid = i >= prop.Min && i <= prop.Max && FromQuoted(s).Length <= prop.Len;
               break;
            case DataFormats.MsgChar:
               i = (int)GetIndexSetting(ccIDX.Print_Data_Message_Number);
               IsValid = i >= prop.Min && i <= prop.Max && FromQuoted(s).Length <= prop.Len;
               break;
            case DataFormats.N1Char:
            case DataFormats.N2Char:
               gp = s.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
               if (gp.Length == 2) {
                  if (!int.TryParse(gp[0].Trim(), out int x)) {
                     break;
                  }
                  IsValid = gp[0].Length >= prop.Min && gp[0].Length <= prop.Max;
               }
               break;
            case DataFormats.Item:
               i = (int)GetIndexSetting(ccIDX.Item);
               IsValid = i >= prop.Min && i <= prop.Max;
               break;
         }
         return IsValid;
      }

      // Merge parts of a byte array into a single byte array
      public byte[] Merge(params byte[][] b) {
         // Calculate the needed length
         int n = 0;
         for (int i = 0; i < b.Length; i++) {
            n += b[i].Length;
         }
         byte[] result = new byte[n];
         // Stuff away the pieces into the result
         n = 0;
         for (int i = 0; i < b.Length; i++) {
            for (int j = 0; j < b[i].Length; j++) {
               result[n++] = b[i][j];
            }
         }
         return result;
      }

      // Build the string to save in the traffic Excel Spreadsheet
      public string GetTraffic(AttrData attr) {
         Type at = ClassCodeAttributes[Array.IndexOf(ClassCodes, Class)];
         string trafficText = $"{LengthIsValid}\t{DataIsValid}\t{GetStatus}";
         trafficText += $"\t{Access}\t{Class}\t{GetAttributeName(at, Attribute)}";
         if (Successful) {
            if (GetDataLength == 0 && Access != AccessCode.Get) {
               trafficText += $"\t\t\t";
            } else {
               trafficText += $"\t{GetDataLength}";
               if (attr.Data.Fmt == DataFormats.Bytes) {
                  trafficText += $"\tSee=>";
               } else {
                  if (Access == AccessCode.Get && attr.Data.DropDown != fmtDD.None) {
                     string[] dd = DropDowns[(int)attr.Data.DropDown];
                     long n = GetDecValue - attr.Data.Min;
                     if (n >= 0 && n < dd.Length) {
                        trafficText += $"\t{dd[n]}";
                     } else {
                        trafficText += $"\t{GetDataValue}";
                     }
                  } else {
                     trafficText += $"\t{GetDataValue}";
                  }
               }
               trafficText += $"\t{GetBytes(GetData, 0, Math.Min(GetDataLength, 16))}";
            }
            if (SetDataLength == 0) {
               trafficText += $"\t\t\t";
            } else {
               trafficText += $"\t{SetDataLength}";
               if (!string.IsNullOrEmpty(SetDataValue) && SetDataValue.Length > 50) {
                  trafficText += $"\tSee=>";
               } else {
                  if (Access == AccessCode.Set && attr.Data.DropDown != fmtDD.None) {
                     string[] dd = DropDowns[(int)attr.Data.DropDown];
                     long n = SetDecValue - attr.Data.Min;
                     if (n >= 0 && n < dd.Length) {
                        trafficText += $"\t{dd[n]}";
                     } else {
                        trafficText += $"\t{SetDataValue}";
                     }
                  } else {
                     trafficText += $"\t{SetDataValue}";
                  }
               }
               trafficText += $"\t{GetBytes(SetData, 0, Math.Min(SetDataLength, 16))}";
            }
         }
         return trafficText;
      }

      #endregion

      #region Service Routines

      // Build the Attribute Dictionary
      void BuildAttributeDictionary() {
         if (AttrDict == null) {
            AttrDict = new Dictionary<ClassCode, byte, AttrData>();
            for (int i = 0; i < ClassCodes.Length; i++) {
               int[] ClassAttr = (int[])ClassCodeAttributes[i].GetEnumValues();
               for (int j = 0; j < ClassAttr.Length; j++) {
                  AttrDict.Add(ClassCodes[i], (byte)ClassAttr[j], GetAttrData(ClassCodes[i], (Byte)ClassAttr[j]));
               }
            }
         }
      }

      // Get attribute data for an arbitrary class/attribute
      AttrData GetAttrData(ClassCode Class, byte attr) {
         AttrData[] tab = M161.ClassCodeAttrData[Array.IndexOf(ClassCodes, Class)];
         AttrData result = Array.Find(tab, at => at.Val == attr);
         result.Class = Class;
         return result;
      }

      // Get AttrData with just the Enum
      public AttrData GetAttrData(Enum e) {
         return AttrDict[ClassCodes[Array.IndexOf(ClassCodeAttributes, e.GetType())], Convert.ToByte(e)];
      }

      // Get the current setting of an index parameter
      public int GetIndexSetting(ccIDX attr) {
         int i = Array.FindIndex(IndexAttr, x => x == (byte)attr);
         return IndexValue[i];
      }

      // Convert a number to adds specifying a count and memory layout
      private void Add(List<byte> packet, ulong value, int count, mem m = mem.LittleEndian) {
         switch (m) {
            case mem.BigEndian:
               for (int i = (count - 1) * 8; i >= 0; i -= 8) {
                  packet.Add((byte)(value >> i));
               }
               break;
            case mem.LittleEndian:
               for (int i = 0; i < count; i++) {
                  packet.Add((byte)value);
                  value >>= 8;
               }
               break;
         }
      }

      // Add two bytes
      private void Add(List<byte> packet, byte v1, byte v2) {
         packet.Add(v1);
         packet.Add(v2);
      }

      // Convert the byte array to individual adds
      private void Add(List<byte> packet, byte[] v) {
         for (int i = 0; i < v.Length; i++) {
            packet.Add(v[i]);
         }
      }

      // Set data at a specific location in a byte array
      private void Set(byte[] dest, uint val, int start, int len, mem m = mem.LittleEndian) {
         for (int i = 0; i < len; i++) {
            dest[i + start] = (byte)val;
            val >>= 8;
         }
      }

      // Register a message if someone is listening
      public void LogIt(string msg) {
         Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddLog, msg));
         Log?.Invoke(this, msg);
      }

      // Interpret the CIP Status and EtherNet/IP Status
      private void InterpretResult(byte[] readData, int readDataLength) {
         string text = "Unknown!";
         int status = (int)Get(ReadData, 48, 2, mem.LittleEndian);
         GetDataLength = ReadDataLength - 50;
         GetDataValue = string.Empty;
         GetDecValue = 0;
         if (GetDataLength >= 0) {
            // There needs to be another case.
            // Hitachi returns success even when COM is off or value is not valid
            switch (status) {
               case 0:
                  // Success returned
                  text = "O.K.";
                  break;
               case 0x14:
                  // Have seen this once but not repeatable
                  text = "Attribute Not Supported!";
                  break;
            }
            GetStatus = $"{status:X2} -- {text} -- {LastIO}";
            if (GetDataLength > 0) {
               // Build a byte array containing the returned data
               GetData = new byte[GetDataLength];
               for (int i = 0; i < GetDataLength; i++) {
                  GetData[i] = ReadData[50 + i];
               }
            } else {
               // No data returned
               GetData = new byte[0];
            }
         } else {
            // No idea what happened
            GetStatus = $"?? -- {text} -- {LastIO}";
            GetData = new byte[0];
         }
      }

      // Format the data based on the data format specified
      private string FormatResult(DataFormats fmt, byte[] data) {
         // Assume that the worst will happen
         string val = GetBytes(data, 0, data.Length);
         switch (fmt) {
            case DataFormats.Decimal:
            case DataFormats.SDecimal:
               if (data.Length <= 8) {
                  // Convert to a decimal and string value
                  GetDecValue = (int)Get(data, 0, data.Length, mem.BigEndian);
                  val = GetDecValue.ToString();
               }
               break;
            case DataFormats.DecimalLE:
            case DataFormats.SDecimalLE:
               if (data.Length <= 8) {
                  // Convert to a decimal and string value
                  GetDecValue = (int)Get(data, 0, data.Length, mem.LittleEndian);
                  val = GetDecValue.ToString();
               }
               break;
            case DataFormats.Bytes:
               // Default will work
               break;
            case DataFormats.UTF8:
            case DataFormats.UTF8N:
               // Convert bytes to characters using "UTF8"
               val = GetUTF8(data, 0, data.Length);
               break;
            case DataFormats.XY:
               if (data.Length == 3) {
                  // Exactly three characters gets xxx,yy
                  val = $"{Get(data, 0, 2, mem.BigEndian)}, {Get(data, 2, 1, mem.BigEndian)}";
               }
               break;
            case DataFormats.Date:
               if (data.Length == 10 || data.Length == 12) {
                  // Date is returned as "MM DD YYYY hh mm"
                  val = $"{Get(data, 0, 2, mem.LittleEndian)}/{Get(data, 2, 2, mem.LittleEndian)}/{Get(data, 4, 2, mem.LittleEndian)}";
                  val += $" {Get(data, 6, 2, mem.LittleEndian)}:{Get(data, 8, 2, mem.LittleEndian)}";
                  if (data.Length == 12) {
                     // and maybe "ss"
                     val += $":{Get(data, 10, 2, mem.LittleEndian)}";
                  }
               }
               break;
            case DataFormats.N1N1:
               if (data.Length == 2) {
                  // Shown an nn,nn
                  val = $"{Get(data, 0, 1, mem.BigEndian)}, {Get(data, 1, 1, mem.BigEndian)}";
               }
               break;
            case DataFormats.N2N2:
               if (data.Length == 4) {
                  // Shown an nn,nn
                  val = $"{Get(data, 0, 2, mem.BigEndian)}, {Get(data, 2, 2, mem.BigEndian)}";
               }
               break;
            case DataFormats.ItemChar:
               if (data.Length > 1) {
                  val = GetUTF8(data, 1, data.Length - 1);
               }
               break;
            case DataFormats.GroupChar:
               if (data.Length > 1) {
                  val = GetUTF8(data, 1, data.Length - 1);
               }
               break;
            case DataFormats.MsgChar:
               if (data.Length > 2) {
                  val = GetUTF8(data, 2, data.Length - 2);
               }
               break;
            case DataFormats.N1Char:
               if (data.Length > 1) {
                  // shown as nn, "UTF8 characters"
                  val = $"{Get(data, 0, 1, mem.BigEndian)}, {GetUTF8(data, 1, data.Length - 1)}";
               }
               break;
            case DataFormats.N2Char:
               if (data.Length > 2) {
                  // shown as nn, "UTF8 characters"
                  val = $"{Get(data, 0, 1, mem.BigEndian)}, {GetUTF8(data, 2, data.Length - 2)}";
               }
               break;
         }
         return val;
      }

      // Save away the details of the request and return the associated attribute data
      private AttrData SetRequest(AccessCode Access, ClassCode Class, byte Instance, byte Attribute, byte[] dataOut) {
         this.Access = Access;
         this.Class = Class;
         this.Instance = Instance;
         this.Attribute = Attribute;
         this.SetData = dataOut;
         this.SetDataLength = (byte)dataOut.Length;
         LastIO = $"{(int)Access:X2} {(int)Class & 0xFF:X2} {(int)Instance:X2} {(int)Attribute:X2}";
         return AttrDict[Class, Attribute];
      }

      #endregion

      #region Excel traffic capture

      // Send a request to the Traffic class to start a new spreadsheet and set the headers
      public static void CreateExcelApp() {
         string trafficHdrs =
           "Count OK\tData OK\tStatus/Path\tAccess\tClass\tAttribute" +
           "\t#In\tData In\tRaw In\t#Out\tData Out\tRaw Out";

         Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.Create, trafficHdrs));
      }

      // Send a new traffic line to the Excel spreadsheet
      public static void FillInColData(string data) {
         Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddTraffic, data));
      }

      // Close out the Excel file.  If View is set, a save folder name must be supplied. 
      public static void CloseExcelFile(bool view) {
         Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.Close, view));
         if (view) {
            Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.View));
            CreateExcelApp();
         } else {
            Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.Exit));
         }
      }

      // Close out the Excel file.  If View is set, a save folder name must be supplied. 
      public static void ResetExcelFile() {
         Traffic?.Tasks.Add(new TrafficPkt(Traffic.TaskType.Close, false));
         CreateExcelApp();
      }

      // Create a unique file name by incorporating time into the filename
      public static string CreateFileName(string directory, string s, string ext = "csv") {
         if (Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
         }
         return Path.Combine(directory, $"{s}{DateTime.Now.ToString("yyMMdd-HHmmss")}.{ext}");
      }

      #endregion

   }

   public class EIPEventArg {

      #region Properties, Constructors and Destructors

      public AccessCode Access { get; set; }
      public ClassCode Class { get; set; }
      public byte Instance { get; set; }
      public byte Attribute { get; set; }
      public bool Successful { get; set; }

      // Pass reguest information and status on to the user
      public EIPEventArg(AccessCode Access, ClassCode Class, byte Instance, byte Attribute, bool Successful) {
         this.Access = Access;
         this.Class = Class;
         this.Instance = Instance;
         this.Attribute = Attribute;
         this.Successful = Successful;
      }

      #endregion

   }

}
