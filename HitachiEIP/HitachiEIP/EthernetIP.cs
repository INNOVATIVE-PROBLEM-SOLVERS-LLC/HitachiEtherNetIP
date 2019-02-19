using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HitachiEIP {

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
      None = -1,     // No formating
      Decimal = 0,   // Decimal numbers up to 8 digits
      UTF8 = 1,      // UTF8 characters (Not ASCII or unicode)
      Date = 2,      // YYYY MM DD HH MM SS 6 2-byte values in Little Endian format
      Bytes = 3,     // Raw data in 2-digit hex notation
      XY = 4,        // x = 2 bytes, y = 1 byte
      N2N2 = 5,      // 2 2-byte numbers
      N2Char = 6     // 2-byte number + UTF8 String
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
      Shift_Count_Condition = 0x65,
      First_Calendar_Block_Number = 0x66,
      Calendar_Block_Number_In_Item = 0x67,
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
      Substitute_Rule_Year = 0x74,
      Substitute_Rule_Month = 0x75,
      Substitute_Rule_Day = 0x76,
      Substitute_Rule_Hour = 0x77,
      Substitute_Rule_Minute = 0x78,
      Substitute_Rule_Weeks = 0x79,
      Substitute_Rule_Day_Of_Week = 0x7A,
      Time_Count_Start_Value = 0x7B,
      Time_Count_End_Value = 0x7C,
      Time_Count_Reset_Value = 0x7D,
      Reset_Time_Value = 0x7E,
      Update_Interval_Value = 0x7F,
      Shift_Start_Hour = 0x80,
      Shift_Start_Minute = 0x81,
      Shift_End_Hour = 0x82,
      Shift_Ene_Minute = 0x83,
      Count_String_Value = 0x84,
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
      Availibility_Of_External_Count = 0x72,
      Availibility_Of_Zero_Suppression = 0x73,
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

      // In case of fatal error
      public event ErrorHandler Error;
      public delegate void ErrorHandler(EIP sender, string msg);

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

      public enum EIP_Type {
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

      public enum EIP_Command {
         Null = 0,
         ForwardOpen = 0x54,
         ForwardClose = 0x4e,
      }

      enum Segment {
         Class = 0x20,
         Instance = 0x24,
         Attribute = 0x30,
      }

      public Int32 port { get; set; }
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

      public byte[] Nodata = new byte[0];

      public Encoding encode = Encoding.GetEncoding("ISO-8859-1");

      // Flag to avoid constant forward open/close if alread open
      bool OpenCloseForward = false;

      #endregion

      #region Constructors and Destructors

      public EIP(string IPAddress, Int32 port) {
         this.IPAddress = IPAddress;
         this.port = port;

         DataII.BuildAttributeDictionary();
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
            LogIt("Connect Complete!");
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
            LogIt("Disconnect Complete!");
         } catch (Exception e) {
            LogIt(e.Message);
         }
         StateChanged?.Invoke(this, "Connection Changed");
         return result;
      }

      // Start EtherNet/IP Session
      public bool StartSession() {
         bool successful = false;
         byte[] data;
         Int32 bytes;
         SessionID = 0;
         if (Connect()) {
            byte[] ed = EIP_Session(EIP_Type.RegisterSession);
            if (Write(ed, 0, ed.Length) && Read(out data, out bytes) && bytes >= 8) {
               SessionID = Get(data, 4, 4, mem.LittleEndian);
               LogIt("Session Started!");
               successful = true;
            }
         }
         if (!successful) {
            LogIt("Session Start Failed!");
            Disconnect();
         }
         StateChanged?.Invoke(this, "Session Changed");
         return successful;
      }

      // End EtherNet/IP Session
      public bool EndSession() {
         SessionID = 0;
         if (client.Connected) {
            byte[] ed = EIP_Session(EIP_Type.UnRegisterSession, SessionID);
            Write(ed, 0, ed.Length);
         }
         LogIt("Session Ended!");
         Disconnect();
         StateChanged?.Invoke(this, "Session Changed");
         return true;
      }

      // Start EtherNet/IP Forward Open
      public bool ForwardOpen(bool preserveState = false) {
         bool successful = false;
         if (preserveState) {
            OpenCloseForward = !ForwardIsOpen;
            if (OpenCloseForward) {
               successful = ForwardOpen();
            }
         } else {
            byte[] data;
            Int32 bytes;
            O_T_ConnectionID = 0;
            T_O_ConnectionID = 0;
            byte[] ed = EIP_Wrapper(EIP_Type.SendRRData, EIP_Command.ForwardOpen);
            if (Write(ed, 0, ed.Length) && Read(out data, out bytes) && bytes >= 52) {
               O_T_ConnectionID = Get(data, 44, 4, mem.LittleEndian);
               T_O_ConnectionID = Get(data, 48, 4, mem.LittleEndian);
               successful = true;
               LogIt("Forward Open!");
            } else {
               LogIt("Forward Open Failed!");
            }
            StateChanged?.Invoke(this, "Forward Changed");
         }
         return successful;
      }

      // End EtherNet/IP Forward Open
      public bool ForwardClose(bool restoreState = false) {
         if (restoreState) {
            if (OpenCloseForward && ForwardIsOpen) {
               ForwardClose();
            }
            OpenCloseForward = false;
         } else {
            byte[] data;
            Int32 bytes;
            O_T_ConnectionID = 0;
            T_O_ConnectionID = 0;
            byte[] ed = EIP_Wrapper(EIP_Type.SendRRData, EIP_Command.ForwardClose);
            if (Write(ed, 0, ed.Length) && Read(out data, out bytes)) {
               LogIt("Forward Close!");
            } else {
               LogIt("Forward Close Failed!");
            }
            StateChanged?.Invoke(this, "Forward Changed");
         }
         return true;
      }

      // Read response to EtherNet/IP request
      private bool Read(out byte[] data, out int bytes) {
         bool successful = false;
         data = new byte[10000];
         bytes = -1;
         if (stream != null) {
            try {
               // Allow for up to 2 seconds for a response
               stream.ReadTimeout = 2000;
               bytes = stream.Read(data, 0, data.Length);
               successful = bytes >= 0;
            } catch (IOException e) {
               LogIt(e.Message);
            }
         }
         if (!successful) {
            LogIt("Read Failed. Connection Closed!");
            Disconnect();
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
      public bool ReadOneAttribute(ClassCode Class, byte Attribute, byte[] DataOut, out string dataIn) {
         bool Successful = false;
         bool OpenCloseForward = !ForwardIsOpen;
         if (OpenCloseForward) {
            // Status is checked later
            ForwardOpen();
         }
         AttrData attr = SetRequest(AccessCode.Get, Class, 0x01, Attribute, DataOut);
         dataIn = "#Error";
         if (ForwardIsOpen) {
            SetDataValue = string.Empty;
            LengthIsValid = false;
            DataIsValid = false;
            int n = EIP_GSS(EIP_Type.SendUnitData, AccessCode.Get, Class, 0x01, Attribute, DataOut);
            // Write the request and read the response
            if (Write(GetSetSrvPkt, 0, n) && Read(out ReadData, out ReadDataLength)) {
               InterpretResult(ReadData, ReadDataLength);
               LengthIsValid = CountIsValid(GetData, attr);
               DataIsValid = TextIsValid(GetData, attr.Data);
               GetDataValue = dataIn = FormatResult(attr.Data.Fmt, GetData);
               Successful = true;
            }
         }
         IOComplete?.Invoke(this, new EIPEventArg(AccessCode.Get, Class, 0x01, Attribute, Successful));
         if (OpenCloseForward && ForwardIsOpen) {
            ForwardClose();
         }
         return Successful;
      }

      // Write one attribute
      public bool WriteOneAttribute(ClassCode Class, byte Attribute, byte[] DataOut) {
         bool Successful = false;
         // Can be called with or without a Forward path open
         bool OpenCloseForward = !ForwardIsOpen;
         if (OpenCloseForward) {
            ForwardOpen();
         }
         AttrData attr = SetRequest(AccessCode.Set, Class, 0x01, Attribute, DataOut);
         if (ForwardIsOpen) {
            int n = EIP_GSS(EIP_Type.SendUnitData, AccessCode.Set, Class, 0x01, Attribute, DataOut);
            // Write the request and read the response
            if (Write(GetSetSrvPkt, 0, n) && Read(out ReadData, out ReadDataLength)) {
               InterpretResult(ReadData, ReadDataLength);
               LengthIsValid = CountIsValid(SetData, attr);
               DataIsValid = TextIsValid(SetData, attr.Data);
               Successful = true;
            }
         }
         IOComplete?.Invoke(this, new EIPEventArg(AccessCode.Set, Class, 0x01, Attribute, Successful));
         if (OpenCloseForward && ForwardIsOpen) {
            ForwardClose();
         }
         return Successful;
      }

      // Service one attribute
      public bool ServiceAttribute(ClassCode Class, byte Attribute, byte[] DataOut) {
         bool Successful = false;
         bool OpenCloseForward = !ForwardIsOpen;
         if (OpenCloseForward) {
            ForwardOpen();
         }
         AttrData attr = SetRequest(AccessCode.Service, Class, 0x01, Attribute, DataOut);
         if (ForwardIsOpen) {
            int n = EIP_GSS(EIP_Type.SendUnitData, AccessCode.Service, Class, 0x01, Attribute, DataOut);
            // Write the request and read the response
            if (Write(GetSetSrvPkt, 0, n) && Read(out ReadData, out ReadDataLength)) {
               InterpretResult(ReadData, ReadDataLength);
               LengthIsValid = CountIsValid(SetData, attr);
               DataIsValid = TextIsValid(SetData, attr.Data);
               Successful = true;
            }
         }
         IOComplete?.Invoke(this, new EIPEventArg(AccessCode.Service, Class, 0x01, Attribute, Successful));
         if (OpenCloseForward && ForwardIsOpen) {
            ForwardClose();
         }
         return Successful;
      }

      // Handles Hitachi Get, Set, and Service
      byte[] GetSetSrvPkt = null;
      private int EIP_GSS(EIP_Type t, AccessCode c, ClassCode Class, byte Instance, byte Attribute, byte[] DataOut) {
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
      public uint Get(byte[] b, int start, int length, mem m) {
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
         for (int i = 0; i < Math.Min(length, 20); i++) {
            s += $"{data[start + i]:X2} ";
         }
         if (length > 20) {
            s += "...";
         }
         return s;
      }

      // Get data as UTF8 characters
      public string GetUTF8(byte[] data, int start, int length) {
         string s = encode.GetString(data, 0, Math.Min(length, 20));
         if (length > 20) {
            s += "...";
         }
         return s;
      }

      // Convert unsigned integer to byte array
      public byte[] ToBytes(uint v, int length, mem order = mem.BigEndian) {
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
         return encode.GetBytes(v);
      }

      // Format input byte array to readable characters
      public void SetBackColor(AttrData attr, TextBox count, TextBox text, ComboBox dropdown, Prop prop) {
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

      // Format output
      public byte[] FormatOutput(TextBox t, ComboBox c, AttrData attr, Prop prop) {
         if (attr.Data.DropDown != fmtDD.None && c.Visible) {
            SetDataValue = (c.SelectedIndex + prop.Min).ToString();
            return ToBytes((uint)(c.SelectedIndex + prop.Min), prop.Len);
         } else {
            return FormatOutput(t.Text, prop);
         }
      }

      // Format output
      public byte[] FormatOutput(string s, Prop prop) {
         if (prop.Len == 0) {
            return Nodata;
         }
         byte[] result = null;
         string[] sa;
         SetDataValue = s;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
               if (uint.TryParse(s, out uint val)) {
                  result = ToBytes(val, prop.Len);
               }
               break;
            case DataFormats.UTF8:
               if (s.StartsWith("\"") && s.EndsWith("\"")) {
                  result = encode.GetBytes($"{s.Substring(1, s.Length - 2)}\x00");
               } else {
                  result = encode.GetBytes($"{s}\x00");
               }
               break;
            case DataFormats.Date:
               if (DateTime.TryParse(s, out DateTime d)) {
                  byte[] year = ToBytes((uint)d.Year, 4, mem.LittleEndian);
                  byte[] month = ToBytes((uint)d.Month, 2, mem.LittleEndian);
                  byte[] day = ToBytes((uint)d.Day, 2, mem.LittleEndian);
                  byte[] hour = ToBytes((uint)d.Hour, 2, mem.LittleEndian);
                  byte[] minute = ToBytes((uint)d.Minute, 2, mem.LittleEndian);
                  result = new byte[12];
                  for (int i = 0; i < 4; i++) {
                     result[i] = year[i];
                     if (i < 2) {
                        result[i + 4] = month[i];
                        result[i + 6] = day[i];
                        result[i + 7] = hour[i];
                        result[i + 10] = minute[i];
                     }
                  }
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
            case DataFormats.N2N2:
               sa = s.Split(',');
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint n1) && uint.TryParse(sa[1].Trim(), out uint n2)) {
                     result = Merge(ToBytes(n1, 2), ToBytes(n2, 2));
                  }
               }
               break;
            case DataFormats.N2Char:
               sa = s.Split(new char[] { ',' }, 1);
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint n)) {
                     string gp = new string(new char[] { (char)(n >> 8), (char)(n & 0xFF) });
                     result = Merge(ToBytes(n, 2), encode.GetBytes(s + "\x00"));
                  }
               }
               break;
         }
         if (result == null) {
            result = new byte[0];
         }
         return result;
      }

      // Does count agree with Hitachi Document?
      public bool CountIsValid(byte[] data, AttrData attr) {
         bool IsValid = false;
         switch (attr.Data.Fmt) {
            case DataFormats.Decimal:
            case DataFormats.Date:
            case DataFormats.Bytes:
            case DataFormats.XY:
            case DataFormats.N2N2:
               IsValid = attr.Data.Len == data.Length;
               break;
            case DataFormats.N2Char:
            case DataFormats.UTF8:
               IsValid = attr.Data.Len >= data.Length;
               break;
            default:
               break;
         }
         return IsValid;
      }

      // Does text agree with Hitachi Document?
      public bool TextIsValid(byte[] data, Prop prop) {
         bool IsValid = false;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
               if (data.Length <= 8) {
                  ulong dec = Get(data, 0, data.Length, mem.BigEndian);
                  IsValid = prop.Max == 0 || dec >= (ulong)prop.Min && dec <= (ulong)prop.Max;
               }
               break;
            case DataFormats.UTF8:
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
                  uint x = Get(data, 0, 2, mem.BigEndian);
                  uint y = Get(data, 2, 1, mem.BigEndian);
                  IsValid = x <= 65535 && y <= 47;
               }
               break;
            case DataFormats.N2N2:
               if (prop.Len == data.Length) {
                  uint n1 = Get(data, 0, 2, mem.LittleEndian);
                  uint n2 = Get(data, 2, 2, mem.LittleEndian);
                  IsValid = n1 >= prop.Min && n1 <= prop.Max && n2 >= prop.Min && n2 <= prop.Max;
               }
               break;
            case DataFormats.N2Char:
               if (data.Length > 1) {
                  uint n = Get(data, 0, 2, mem.LittleEndian);
                  IsValid = n >= prop.Min && n <= prop.Max;
               }
               break;
            default:
               break;
         }
         return IsValid;
      }

      // Does text agree with Hitachi Document?
      public bool TextIsValid(string s, Prop prop) {
         bool IsValid = false;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
               if (long.TryParse(s, out long dec)) {
                  IsValid = prop.Max == 0 || dec >= prop.Min && dec <= prop.Max;
               }
               break;
            case DataFormats.UTF8:
               IsValid = s.Length > 0;
               break;
            case DataFormats.Date:
               IsValid = DateTime.TryParse(s, out DateTime d);
               break;
            case DataFormats.Bytes:
               string[] b = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               for (int i = 0; i < b.Length; i++) {
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
            case DataFormats.N2Char:
               string[] gp = s.Split(new char[] { ',' }, 1, StringSplitOptions.RemoveEmptyEntries);
               if (gp.Length == 2) {
                  if (!int.TryParse(gp[0].Trim(), out int x)) {
                     break;
                  }
                  IsValid = x >= prop.Min && x <= prop.Max;
               }
               break;
            default:
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

      #endregion

      #region Service Routines

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
      private void LogIt(string msg) {
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
               if (data.Length <= 8) {
                  // Convert to a decimal and string value
                  GetDecValue = (int)Get(data, 0, data.Length, mem.BigEndian);
                  val = GetDecValue.ToString();
               }
               break;
            case DataFormats.Bytes:
               // Default will work
               break;
            case DataFormats.UTF8:
               // Convert bytes to characters using "ISO-8859-1"
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
            case DataFormats.N2N2:
               if (data.Length == 4) {
                  // Shown an nn,nn
                  val = $"{Get(data, 0, 2, mem.BigEndian)}, {Get(data, 2, 2, mem.BigEndian)}";
               }
               break;
            case DataFormats.N2Char:
               if (data.Length > 1) {
                  // shown as nn, "ISO-8859-1 characters"
                  val = $"{Get(data, 0, 1, mem.BigEndian)}, {GetUTF8(data, 1, data.Length - 1)}";
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
         return DataII.AttrDict[Class, Attribute];
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
