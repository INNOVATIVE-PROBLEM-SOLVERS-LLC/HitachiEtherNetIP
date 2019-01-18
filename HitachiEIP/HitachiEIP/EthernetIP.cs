using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace HitachiEIP {

   #region Public Enums

   public enum mem {
      BigEndian,
      LittleEndian
   }

   public enum DataFormats {
      Decimal,
      ASCII,
      Date,
      Bytes,
   }

   public enum Protocol {
      TCP = 6
   }

   public enum EIP_Type {
      RegisterSession = 0x0065,
      UnRegisterSession = 0x0066,
      SendRRData = 0x006F,
      SendUnitData = 0x0070,
   }

   public enum Data_Type {
      ConnectedAddressItem = 0xa1,
      ConnectedDataItem = 0xb1,
      UnconnectedDataItem = 0xb2,
   }

   public enum EIP_Command {
      Null = 0,
      ForwardOpen = 0x54,
      ForwardClose = 0x4e,
   }

   public enum Segment {
      Class = 0x20,
      Instance = 0x24,
      Attribute = 0x30,
   }

   #endregion

   #region EtherNetIP Definitions

   // Access codes
   public enum eipAccessCode {
      Set = 0x32,
      Get = 0x33,
      Service = 0x34,
   }

   // Class codes
   public enum eipClassCode {
      Print_data_management = 0x07000066,
      Print_format = 0x08000067,
      Print_specification = 0x09000068,
      Calendar = 0x01000069,
      User_pattern = 0x0C00006B,
      Substitution_rules = 0x0A00006C,
      Enviroment_setting = 0x03000071,
      Unit_Information = 0x0B000073,
      Operation_management = 0x06000074,
      IJP_operation = 0x04000075,
      Count = 0x02000079,
      Index = 0x0500007A,
   }

   // Attributes within Print Data Management class 0x66
   public enum eipPrint_Data_Management : ulong {
      Select_Message = 0x09000000003064,
      Store_Print_Data = 0x0A000000003165,
      Delete_Print_Data = 0x03000000023167,
      Print_Data_Name = 0x07000000003169,
      List_of_Messages = 0x0600000002326A,
      Print_Data_Number = 0x0800000004316B,
      Change_Create_Group_Name = 0x0100000000316C,
      Group_Deletion = 0x0400000001316D,
      List_of_Groups = 0x0500000001326F,
      Change_Group_Number = 0x02000000023170,
   }

   // Attributes within Print Format class 0x67
   public enum eipPrint_format : ulong {
      Message_Name = 0x140000010264,
      Print_Item = 0x190000010265,
      Number_Of_Columns = 0x150000010266,
      Format_Type = 0x0E0000010267,
      Insert_Column = 0x0F0000010069,
      Delete_Column = 0x08000001006A,
      Add_Column = 0x01000001006B,
      Number_Of_Print_Line_And_Print_Format = 0x16000001016C,
      Format_Setup = 0x0D000001016D,
      Adding_Print_Items = 0x03000001006E,
      Deleting_Print_Items = 0x09000001006F,
      Print_Character_String = 0x180000010371,
      Line_Count = 0x120000010372,
      Line_Spacing = 0x130000010373,
      Dot_Matrix = 0x0B0000010374,
      InterCharacter_Space = 0x100000010375,
      Character_Bold = 0x070000010376,
      Barcode_Type = 0x050000010377,
      Readable_Code = 0x1B0000010378,
      Prefix_Code = 0x170000010379,
      X_and_Y_Coordinate = 0x1C000001037A,
      InterCharacter_SpaceII = 0x11000001037B,
      Add_To_End_Of_String = 0x02000001018A,
      Calendar_Offset = 0x06000001038D,
      DIN_Print = 0x0A000001038E,
      EAN_Prefix = 0x0C000001038F,
      Barcode_Printing = 0x040000010390,
      QR_Error_Correction_Level = 0x1A0000010391,
   }

   // Attributes within Print Specification class 0x68
   public enum eipPrint_specification : ulong {
      Character_Height = 0x02000000010364,
      Ink_Drop_Use = 0x08000000010365,
      High_Speed_Print = 0x06000000010366,
      Character_Width = 0x04000000020367,
      Character_Orientation = 0x03000000010368,
      Print_Start_Delay_Forward = 0x0B000000020369,
      Print_Start_Delay_Reverse = 0x0A00000002036A,
      Product_Speed_Matching = 0x0E00000001036B,
      Pulse_Rate_Division_Factor = 0x0F00000002036C,
      Speed_Compensation = 0x1200000001036D,
      Line_Speed = 0x0900000002036E,
      Distance_Between_Print_Head_And_Object = 0x0500000001036F,
      Print_Target_Width = 0x0D000000020370,
      Actual_Print_Width = 0x01000000020371,
      Repeat_Count = 0x10000000020372,
      Repeat_Interval = 0x11000000030373,
      Target_Sensor_Timer = 0x15000000020374,
      Target_Sensor_Filter = 0x14000000010375,
      Targer_Sensor_Filter_Value = 0x13000000010376,
      Ink_Drop_Charge_Rule = 0x07000000010377,
      Print_Start_Position_Adjustment_Value = 0x0C000000020378,
   }

   // Attributes within Calendar class 0x69
   public enum eipCalendar : ulong {
      Shift_Count_Condition = 0x0A000000010265,
      First_Calendar_Block_Number = 0x03000000010266,
      Calendar_Block_Number_In_Item = 0x01000000010267,
      Offset_Year = 0x08000000010368,
      Offset_Month = 0x07000000010369,
      Offset_Day = 0x0400000002036A,
      Offset_Hour = 0x0500000002036B,
      Offset_Minute = 0x0600000002036C,
      Zero_Suppress_Year = 0x2000000001036D,
      Zero_Suppress_Month = 0x1E00000001036E,
      Zero_Suppress_Day = 0x1A00000001036F,
      Zero_Suppress_Hour = 0x1C000000010370,
      Zero_Suppress_Minute = 0x1D000000010371,
      Zero_Suppress_Weeks = 0x1F000000010372,
      Zero_Suppress_Day_Of_Week = 0x1B000000010373,
      Substitute_Rule_Year = 0x15000000010374,
      Substitute_Rule_Month = 0x13000000010375,
      Substitute_Rule_Day = 0x0F000000010376,
      Substitute_Rule_Hour = 0x11000000010377,
      Substitute_Rule_Minute = 0x12000000010378,
      Substitute_Rule_Weeks = 0x14000000010379,
      Substitute_Rule_Day_Of_Week = 0x1000000001037A,
      Time_Count_Start_Value = 0x1800000003037B,
      Time_Count_End_Value = 0x1600000003037C,
      Time_Count_Reset_Value = 0x1700000003037D,
      Reset_Time_Value = 0x0900000001037E,
      Update_Interval_Value = 0x1900000001037F,
      Shift_Start_Hour = 0x0D000000010380,
      Shift_Start_Minute = 0x0E000000010381,
      Shift_End_Hour = 0x0B000000010382,
      Shift_Ene_Minute = 0x0C000000010383,
      Count_String_Value = 0x02000000010384,
   }

   // Attributes within User Pattern class 0x6B
   public enum eipUser_pattern : ulong { // 0x6B
      User_Pattern_Fixed = 0x01000000000364,
      User_Pattern_Free = 0x02000000000365,
   }

   // Attributes within Substitution Rules class 0x6C
   public enum eipSubstitution_rules : ulong { // 0x6C
      Number = 0x03000000000364,
      Name = 0x02000000000365,
      Start_Year = 0x01000000000366,
      Year = 0x0A000000000367,
      Month = 0x08000000000368,
      Day = 0x04000000000369,
      Hour = 0x0600000000036A,
      Minute = 0x0700000000036B,
      Week = 0x0900000000036C,
      Day_Of_Week = 0x0500000000036D,
   }

   // Attributes within Enviroment Setting class 0x71
   public enum eipEnviroment_setting : ulong {
      Current_Time = 0x05000000000365,
      Calendar_Date_Time = 0x01000000000366,
      Calendar_Date_Time_Availibility = 0x02000000000367,
      Clock_System = 0x04000000000368,
      User_Environment_Information = 0x08000000000269,
      Cirulation_Control_Setting_Value = 0x0300000000026A,
      Usage_Time_Of_Circulation_Control = 0x0700000000016B,
      Reset_Usage_Time_Of_Citculation_Control = 0x0600000000016C,
   }

   // Attributes within Unit Information class 0x73
   public enum eipUnit_Information : ulong {
      Unit_Information = 0x14000000000264,
      Model_Name = 0x0F00000000126B,
      Serial_Number = 0x1100000000026C,
      Ink_Name = 0x0800000000126D,
      Input_Mode = 0x0900000000026E,
      Maximum_Character_Count = 0x0B00000000026F,
      Maximum_Registered_Message_Count = 0x0D000000000270,
      Barcode_Information = 0x01000000000271,
      Usable_Character_Size = 0x15000000000272,
      Maximum_Calendar_And_Count = 0x0A000000000273,
      Maximum_Substitution_Rule = 0x0E000000000274,
      Shift_Code_And_Time_Count = 0x12000000000275,
      Chimney_And_DIN_Print = 0x03000000000276,
      Maximum_Line_Count = 0x0C000000000277,
      Basic_Software_Version = 0x02000000001278,
      Controller_Software_Version = 0x04000000001279,
      Engine_M_Software_Version = 0x0500000000127A,
      Engine_S_Software_Version = 0x0600000000127B,
      First_Language_Version = 0x0700000000127C,
      Second_Language_Version = 0x1000000000127D,
      Software_Option_Version = 0x1300000000127E,
   }

   // Attributes within Operation Management class 0x74
   public enum eipOperation_management : ulong {
      Operating_Management = 0x0C000000000264,
      Ink_Operating_Time = 0x09000000020365,
      Alarm_Time = 0x01000000020366,
      Print_Count = 0x0D000000020367,
      Communications_Environment = 0x03000000000268,
      Cumulative_Operation_Time = 0x04000000000269,
      Ink_And_Makeup_Name = 0x0800000000026A,
      Ink_Viscosity = 0x0B00000000026B,
      Ink_Pressure = 0x0A00000000026C,
      Ambient_Temperature = 0x0200000000026D,
      Deflection_Voltage = 0x0500000000026E,
      Excitation_VRef_Setup_Value = 0x0700000000026F,
      Excitation_Frequency = 0x06000000000270,
   }

   // Attributes within IJP Operation class 0x75
   public enum eipIJP_operation : ulong {
      Remote_operation_information = 0x07000000003264,
      Fault_and_warning_history = 0x04000000003266,
      Operating_condition = 0x06000000003267,
      Warning_condition = 0x0A000000003268,
      Date_and_time_information = 0x0100000000326A,
      Error_code = 0x0300000000326B,
      Start_Remote_Operation = 0x0800000000306C,
      Stop_Remote_Operation = 0x0900000000306D,
      Deflection_voltage_control = 0x0200000000306E,
      Online_Offline = 0x0500000101036F,
   }

   // Attributes within Count class 0x79
   public enum eipCount : ulong {
      Number_Of_Count_Block = 0x0C000000000266,
      Initial_Value = 0x09000000000367,
      Count_Range_1 = 0x04000000000368,
      Count_Range_2 = 0x05000000000369,
      Update_Unit_Halfway = 0x0F00000000036A,
      Update_Unit_Unit = 0x1000000000036B,
      Increment_Value = 0x0800000000036C,
      Direction_Value = 0x0700000000036D,
      Jump_From = 0x0A00000000036E,
      Jump_To = 0x0B00000000036F,
      Reset_Value = 0x0D000000000370,
      Type_Of_Reset_Signal = 0x0E000000000371,
      Availibility_Of_External_Count = 0x01000000000372,
      Availibility_Of_Zero_Suppression = 0x02000000000373,
      Count_Multiplier = 0x03000000000374,
      Count_Skip = 0x06000000000375,
   }

   // Attributes within Index class 0x7A
   public enum eipIndex : ulong {
      Start_Stop_Management_Flag = 0x0A000001010364,
      Automatic_reflection = 0x01000001010365,
      Item_Count = 0x06010064020366,
      Column = 0x04010064020367,
      Line = 0x07010006010368,
      Character_position = 0x030103E8020369,
      Print_Data_Message_Number = 0x090107D002036A,
      Print_Data_Group_Data = 0x0801006301036B,
      Substitution_Rules_Setting = 0x0B01006301036C,
      User_Pattern_Size = 0x0C01001301036D,
      Count_Block = 0x0501000801036E,
      Calendar_Block = 0x0201000801036F,
   }

   #endregion

   public class EIP {

      #region Events

      // Event Logging
      internal event LogHandler Log;
      internal delegate void LogHandler(EIP sender, string msg);

      // In case of fatal error
      internal event ErrorHandler Error;
      internal delegate void ErrorHandler(EIP sender, string msg);

      #endregion

      #region Declarations/Properties

      public Int32 port;
      public string IPAddress;

      TcpClient client = null;
      NetworkStream stream = null;

      public uint SessionID { get; set; } = 0;

      public eipAccessCode Access { get; set; }
      public eipClassCode Class { get; set; }
      public byte Instance { get; set; } = 1;
      public byte Attribute { get; set; } = 1;
      public byte DataLength { get; set; } = 1;
      public byte[] Data { get; set; } = { 0x31 };
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

      public byte[] ReadData;
      public Int32 ReadDataLength;

      private const uint setMask = 0x0100;     // not 0 implies true
      private const uint getMask = 0x0200;     // not 0 implies true
      private const uint serviceMask = 0x0300; // 0 implies true


      #endregion

      #region Constructors and Destructors

      public EIP(string IPAddress, Int32 port) {
         this.IPAddress = IPAddress;
         this.port = port;
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
         return result;
      }

      private bool Connect(string IPAddress, int port) {
         this.IPAddress = IPAddress;
         this.port = port;
         return (Connect());
      }

      // Disconnect from Hitachi printer
      private bool Disconnect() {
         bool result = false;
         try {
            stream.Close();
            stream = null;
            client.Close();
            client = null;
            result = true;
            LogIt("Disconnect Complete!");
         } catch (Exception e) {
            LogIt(e.Message);
         }
         return result;
      }

      // Start EtherNet/IP Session
      public void StartSession() {
         if (Connect()) {
            byte[] ed = EIP_Wrapper(EIP_Type.RegisterSession, EIP_Command.Null);
            Write(ed, 0, ed.Length);

            byte[] data;
            Int32 bytes;
            if (Read(out data, out bytes) && bytes >= 8) {
               SessionID = Get(data, 4, 4, mem.LittleEndian);
            } else {
               SessionID = 0;
            }
            LogIt("Session Started!");
         }
      }

      // End EtherNet/IP Session
      internal void EndSession() {
         byte[] ed = EIP_Wrapper(EIP_Type.UnRegisterSession, EIP_Command.Null);
         Write(ed, 0, ed.Length);

         byte[] data;
         Int32 bytes;
         if (Read(out data, out bytes)) {

         }
         SessionID = 0;
         LogIt("Session Ended!");
         Disconnect();
     }

      // Start EtherNet/IP Forward Open
      public void ForwardOpen() {
         byte[] ed = EIP_Wrapper(EIP_Type.SendRRData, EIP_Command.ForwardOpen);
         Write(ed, 0, ed.Length);

         byte[] data;
         Int32 bytes;
         if (Read(out data, out bytes) && bytes >= 52) {
            O_T_ConnectionID = Get(data, 44, 4, mem.LittleEndian);
            T_O_ConnectionID = Get(data, 48, 4, mem.LittleEndian);
         } else {
            O_T_ConnectionID = 0;
            T_O_ConnectionID = 0;
         }
         LogIt("Forward Open!");
      }

      // End EtherNet/IP Forward Open
      public void ForwardClose() {
         byte[] ed = EIP_Wrapper(EIP_Type.SendRRData, EIP_Command.ForwardClose);
         Write(ed, 0, ed.Length);

         byte[] data;
         Int32 bytes;
         if (!Read(out data, out bytes)) {

         }
         O_T_ConnectionID = 0;
         T_O_ConnectionID = 0;
         LogIt("Forward Close!");
      }

      // Read response to EtherNet/IP request
      public bool Read(out byte[] data, out int bytes) {
         bool result = false;
         data = new byte[256];
         bytes = -1;
         try {
            for (int t = 0; t < 10; t++) {
               if (stream.DataAvailable) {
                  bytes = stream.Read(data, 0, data.Length);
                  break;
               }
               Thread.Sleep(50);
            }
            result = bytes > 0;
         } catch (Exception e) {
            LogIt(e.Message);
         }
         return result;
      }

      // Issue EtherNet/IP request
      public bool Write(byte[] data, int start, int length) {
         bool result = false;
         try {
            stream.Write(data, start, length);
            result = true;
         } catch (Exception e) {
            LogIt(e.Message);
         }
         return result;
      }

      public bool ReadOneAttribute(eipClassCode Class, byte Attribute, out string val, DataFormats fmt) {
         bool result = false;

         bool OpenCloseSession = !SessionIsOpen;
         bool OpenCloseForward = !ForwardIsOpen;

         if (OpenCloseSession)
            StartSession();
         if (OpenCloseForward)
            ForwardOpen();

         val = string.Empty;

         Access = eipAccessCode.Get;
         this.Class = Class;
         Instance = 0x01;
         this.Attribute = Attribute;
         Data = new byte[] { };
         DataLength = 0;
         try {

            byte[] ed = EIP_Hitachi(EIP_Type.SendUnitData, eipAccessCode.Get);
            Write(ed, 0, ed.Length);

            if (Read(out ReadData, out ReadDataLength)) {
               int status = (int)Get(ReadData, 48, 2, mem.LittleEndian);
               if (status == 0) {
                  int len = ReadDataLength - 50;
                  switch (fmt) {
                     case DataFormats.Decimal:
                        if (len > 4) {
                           val = GetBytes(ReadData, 50, len);
                        } else {
                           val = Get(ReadData, 50, len, mem.BigEndian).ToString();
                        }
                        break;
                     case DataFormats.Bytes:
                        val = GetBytes(ReadData, 50, len);
                        break;
                     case DataFormats.ASCII:
                        val = GetAscii(ReadData, 50, len);
                        break;
                     default:
                        break;
                  }
                  result = true;
               } else {
                  val = "#Error";
               }
            }
         } catch (Exception e) {
            LogIt(e.Message);
         }

         if (OpenCloseForward)
            ForwardClose();
         if (OpenCloseSession)
            EndSession();

         return result;
      }

      public bool WriteOneAttribute(eipClassCode Class, byte Attribute, byte[] val) {
         bool result = false;
         bool OpenCloseSession = !SessionIsOpen;
         bool OpenCloseForward = !ForwardIsOpen;

         if (OpenCloseSession)
            StartSession();
         if (OpenCloseForward)
            ForwardOpen();

         Access = eipAccessCode.Set;
         this.Class = Class;
         Instance = 0x01;
         this.Attribute = Attribute;
         Data = val;
         DataLength = (byte)val.Length;
         try {
            byte[] ed = EIP_Hitachi(EIP_Type.SendUnitData, eipAccessCode.Set);
            Write(ed, 0, ed.Length);

            if (Read(out ReadData, out ReadDataLength)) {
               result = true;
            }
         } catch (Exception e2) {

         }

         if (OpenCloseForward)
            ForwardClose();
         if (OpenCloseSession)
            EndSession();

         return result;
      }

      // Handles Hitachi Get, Set, and Service
      public byte[] EIP_Hitachi(EIP_Type t, eipAccessCode c) {
         List<byte> packet = new List<byte>();
         switch (c) {
            case eipAccessCode.Get:
               Add(packet, (ulong)t, 2);                                 // Command
               Add(packet, (ulong)30, 2);                                // Length of added data at end
               Add(packet, (ulong)SessionID, 4);                         // Session ID
               Add(packet, (ulong)0, 4);                                 // Success
               Add(packet, (ulong)0x0200030000008601, 8, mem.BigEndian); // Sender Context
               Add(packet, (ulong)0, 4);                                 // option flags
               Add(packet, (ulong)0, 4);                                 // option interface handle
               Add(packet, (ulong)30, 2);                                // Timeout
               Add(packet, (ulong)2, 2);                                 // Item count

               // Item #1
               Add(packet, (ulong)Data_Type.ConnectedAddressItem, 2); // Connected address type
               Add(packet, (ulong)4, 2);                              // length of 4
               Add(packet, O_T_ConnectionID, 4);                      // O->T Connection ID

               // Item #2
               Add(packet, (ulong)Data_Type.ConnectedDataItem, 2);    // data type
               Add(packet, (ulong)10, 2);                             // length of 10
               Add(packet, (ulong)2, 2);                              // Count Sequence
               Add(packet, (byte)c, 3);                               // Hitachi command and count
               Add(packet, (byte)Segment.Class, (byte)Class);         // Class
               Add(packet, (byte)Segment.Instance, (byte)Instance);   // Instance
               Add(packet, (byte)Segment.Attribute, (byte)Attribute); // Attribute
               break;
            case eipAccessCode.Set:
            case eipAccessCode.Service:
               Add(packet, (ulong)t, 2);                                 // Command
               Add(packet, (ulong)(30 + DataLength), 2);                 // Length of added data at end
               Add(packet, (ulong)SessionID, 4);                         // Session ID
               Add(packet, (ulong)0, 4);                                 // Success
               Add(packet, (ulong)0x0200030000008601, 8, mem.BigEndian); // Sender Context
               Add(packet, (ulong)0, 4);                                 // option flags
               Add(packet, (ulong)0, 4);                                 // option interface handle
               Add(packet, (ulong)30, 2);                                // Timeout
               Add(packet, (ulong)2, 2);                                 // Item count

               // Item #1
               Add(packet, (ulong)Data_Type.ConnectedAddressItem, 2); // Connected address type
               Add(packet, (ulong)4, 2);                              // length of 4
               Add(packet, O_T_ConnectionID, 4);                      // O->T Connection ID

               // Item #2
               Add(packet, (ulong)Data_Type.ConnectedDataItem, 2);    // data type
               Add(packet, (ulong)(10 + DataLength), 2);              // length of 10 + data length
               Add(packet, (ulong)2, 2);                              // Count Sequence
               Add(packet, (byte)c, 3);                               // Hitachi command and count
               Add(packet, (byte)Segment.Class, (byte)Class);         // Class
               Add(packet, (byte)Segment.Instance, (byte)Instance);   // Instance
               Add(packet, (byte)Segment.Attribute, (byte)Attribute); // Attribute
               Add(packet, Data);                                     // Data

               break;
         }
         return packet.ToArray<byte>();
      }

      // handles Register Session, Unregister Session, Send RR Data(Forward Open, Forward Close)
      public byte[] EIP_Wrapper(EIP_Type t, EIP_Command c) {
         List<byte> packet = new List<byte>();
         switch (t) {
            case EIP_Type.RegisterSession:
               Add(packet, (ulong)t, 2); // Command
               Add(packet, (ulong)4, 2); // Length of added data at end
               Add(packet, (ulong)0, 4); // Session ID (Not set yet)
               Add(packet, (ulong)0, 4); // Status to be returned
               Add(packet, (ulong)0, 8); // Sender Context
               Add(packet, (ulong)0, 4); // Options
               Add(packet, (ulong)1, 2); // Protocol Version
               Add(packet, (ulong)0, 2); // Flags
               break;
            case EIP_Type.UnRegisterSession:
               Add(packet, (ulong)t, 2);         // Command
               Add(packet, (ulong)0, 2);         // Length of added data at end
               Add(packet, (ulong)SessionID, 4); // Session ID (Not set yet)
               Add(packet, (ulong)0, 4);         // Status to be returned
               Add(packet, (ulong)0, 8);         // Sender Context
               Add(packet, (ulong)0, 4);         // Options
               break;
            case EIP_Type.SendRRData:
               switch (c) {
                  case EIP_Command.ForwardOpen:
                     Add(packet, (ulong)t, 2);         // Command
                     Add(packet, (ulong)62, 2);        // Length of added data at end
                     Add(packet, (ulong)SessionID, 4); // Session ID
                     Add(packet, (ulong)0, 4);         // Success
                     Add(packet, (ulong)0x0200030000008601, 8, mem.BigEndian); // Pertinent to sender
                     Add(packet, (ulong)0, 4);         // option flags
                     Add(packet, (ulong)0, 4);         // option interface handle
                     Add(packet, (ulong)30, 2);        // Timeout
                     Add(packet, (ulong)2, 2);         // Item count
                     Add(packet, (ulong)0, 2);         // Null type
                     Add(packet, (ulong)0, 2);         // length of 0
                     Add(packet, (ulong)Data_Type.UnconnectedDataItem, 2); // data tyoe
                     Add(packet, (ulong)46, 2);        // length of 46

                     // Common Packet
                     Add(packet, (ulong)c, 1);         // Forward open
                     Add(packet, (ulong)02, 1);        // Requested path size
                     Add(packet, (byte)Segment.Class, 6);           // Class
                     Add(packet, (byte)Segment.Instance, Instance); // Instance
                     Add(packet, (ulong)7, 1);         // Priority/Time
                     Add(packet, (ulong)0xea, 1);      // Timeout Ticks
                     Add(packet, (ulong)0, 4);         // O->T Connection ID Random Number
                     Add(packet, 0x98000340, 4, mem.BigEndian);  // T->O Connection ID Random number
                     Add(packet, (ulong)0x98, 2);      // Connection Serial Number random number
                     Add(packet, (ulong)0, 2);         // vendor ID
                     Add(packet, (ulong)0, 4);         // Originator Serial number
                     Add(packet, (ulong)0, 1);         // Connection Timeout Multiplier
                     Add(packet, (ulong)0, 3);         // Reserved

                     Add(packet, 10000000, 4);         // O->T RPI
                     Add(packet, 0xff, 0x43);          // O->T Network Connection Parameters
                     Add(packet, 10000000, 4);         // T->O RPI
                     Add(packet, 0xff, 0x43);          // T->O Network Connection Parameters
                     Add(packet, (ulong)0xa3, 1);      // Transport type/trigger
                     Add(packet, (ulong)2, 1);         // Connection Path Size in 16-bit words
                     Add(packet, (byte)Segment.Class, 2);           // Class
                     Add(packet, (byte)Segment.Instance, Instance); // Instance
                     break;
                  case EIP_Command.ForwardClose:
                     Add(packet, (ulong)t, 2);         // Command
                     Add(packet, (ulong)34, 2);        // Length of added data at end
                     Add(packet, (ulong)SessionID, 4); // Session ID
                     Add(packet, (ulong)0, 4);         // Success
                     Add(packet, (ulong)0x0100030000008601, 8, mem.BigEndian); // Pertinant to sender(Unknown for now)
                     Add(packet, (ulong)0, 4);         // option flags
                     Add(packet, (ulong)0, 4);         // option interface handle
                     Add(packet, (ulong)30, 2);        // Timeout
                     Add(packet, (ulong)2, 2);         // Item count
                     Add(packet, (ulong)0, 2);         // Null type
                     Add(packet, (ulong)0, 2);         // length of 0
                     Add(packet, (ulong)Data_Type.UnconnectedDataItem, 2); // data tyoe
                     Add(packet, (ulong)18, 2);        // length of 46

                     // Common Packet
                     Add(packet, (ulong)c, 1);         // Forward open
                     Add(packet, (ulong)02, 1);        // Requested path size
                     Add(packet, (byte)Segment.Class, 6);           // Class
                     Add(packet, (byte)Segment.Instance, Instance); // Instance
                     Add(packet, (ulong)7, 1);         // Priority/Time
                     Add(packet, (ulong)0xea, 1);      // Timeout Ticks
                     Add(packet, (ulong)0x98, 2);      // Connection Serial Number random number
                     Add(packet, (ulong)0, 2);         // vendor ID
                     Add(packet, (ulong)0, 4);         // Originator Serial number
                     Add(packet, (ulong)0, 1);         // Connection Path Size
                     Add(packet, (ulong)0, 1);         // Reserved
                     break;
               }
               break;
         }
         return packet.ToArray<byte>();
      }

      // Get only the Functions(Attributes) that Apply to this Access Code
      public int GetDropDowns(eipAccessCode code, ComboBox cb, Type EnumType, out ulong[] values) {

         // Get all names associated with the enumeration
         string[] allNames = EnumType.GetEnumNames();
         ulong[] allValues = (ulong[])EnumType.GetEnumValues();

         // Weed out the unused ones
         List<string> name = new List<string>();
         List<ulong> value = new List<ulong>();
         for (int i = 0; i < allValues.Length; i++) {
            ulong x = allValues[i];
            if (code == eipAccessCode.Get && HasGet(x)
               || code == eipAccessCode.Set && HasSet(x)
               || code == eipAccessCode.Service && HasService(x)) {
               name.Add(allNames[i].Replace('_', ' '));
               value.Add((uint)allValues[i]);
            }
         }

         // Fix up the Attributes combo box
         string savedText = cb.Text;
         cb.Text = string.Empty;
         cb.Items.Clear();
         cb.Items.AddRange(name.ToArray());
         if (cb.FindStringExact(savedText) >= 0) {
            cb.Text = savedText;
         }

         // Return the used ones
         values = value.ToArray();
         return values.Length;
      }

      // Get attribute Human readable name
      public string GetAttributeName(eipClassCode c, ulong v) {
         switch (c) {
            case eipClassCode.Print_data_management:
               return ((eipPrint_Data_Management)v).ToString();
            case eipClassCode.Print_format:
               return ((eipPrint_format)v).ToString();
            case eipClassCode.Print_specification:
               return ((eipPrint_specification)v).ToString();
            case eipClassCode.Calendar:
               return ((eipCalendar)v).ToString();
            case eipClassCode.User_pattern:
               return ((eipUser_pattern)v).ToString();
            case eipClassCode.Substitution_rules:
               return ((eipSubstitution_rules)v).ToString();
            case eipClassCode.Enviroment_setting:
               return ((eipEnviroment_setting)v).ToString();
            case eipClassCode.Unit_Information:
               return ((eipUnit_Information)v).ToString();
            case eipClassCode.Operation_management:
               return ((eipOperation_management)v).ToString();
            case eipClassCode.IJP_operation:
               return ((eipIJP_operation)v).ToString();
            case eipClassCode.Count:
               return ((eipCount)v).ToString();
            case eipClassCode.Index:
               return ((eipIndex)v).ToString();
            default:
               break;
         }
         return "Unknown";
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

      public string GetBytes(byte[] data, int start, int length) {
         string s = string.Empty;
         for (int i = 0; i < Math.Min(length, 50); i++) {
            s += $"{data[start + i]:X2} ";
         }
         if (length > 50) {
            s += "...";
         }
         return s;
      }

      public string GetAscii(byte[] data, int start, int length) {
         string s = string.Empty;
         for (int i = 0; i < Math.Min(length, 50); i++) {
            s += $"{(char)data[start + i]}";
         }
         if (length > 50) {
            s += "...";
         }
         return s;
      }

      public byte[] ToBytes(uint v, int length) {
         byte[] result = new byte[length];
         for (int i = length - 1; i >= 0; i--) {
            result[i] = (byte)(v & 0xFF);
            v >>= 8;
         }
         return result;
      }

      public byte[] ToBytes(string v) {
         byte[] result = new byte[v.Length + 1];
         for (int i = 0; i < v.Length; i++) {
            result[i] = (byte)v[i];
         }
         return result;
      }

      #endregion

      #region Attribute Decode Routines

      public byte GetAttribute(ulong v) {
         return (byte)(v & 0xFF);
      }

      public DataFormats GetFmt(ulong v) {
         return (DataFormats)((v & 0xf000) >> 12);
      }

      public int GetMin(ulong v) {
         return (int)((v & 0xff0000000000) >> 40);
      }

      public int GetMax(ulong v) {
         return (int)((v & 0xffff000000) >> 24);
      }

      public int GetDataLength(ulong v) {
         return (int)((v & 0xff0000) >> 24);
      }

      public bool HasGet(ulong v) {
         return (v & getMask) > 0;
      }

      public bool HasSet(ulong v) {
         return (v & setMask) > 0;
      }

      public bool HasService(ulong v) {
         return (v & serviceMask) == 0;
      }

      #endregion

      #region Service Routines

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
            default:
               break;
         }

      }

      private void Add(List<byte> packet, byte v1, byte v2) {
         packet.Add(v1);
         packet.Add(v2);
      }

      private void Add(List<byte> packet, byte[] v) {
         for (int i = 0; i < v.Length; i++) {
            packet.Add(v[i]);
         }

      }

      private void LogIt(string msg) {
         Log?.Invoke(this, msg);
      }

      private void ErrorOut() {
         Error?.Invoke(this, "Ouch");
      }

      #endregion

   }
}
