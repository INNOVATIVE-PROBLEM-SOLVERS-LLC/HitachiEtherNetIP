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

   // Class Code enum values
   //   The value of the class code enum is
   //     -- 0x = the values that follow are in Hexadecimal 
   //     -- aa = enums are listed in ascending numerical order.  These two Hex
   //             digits cause the sort order to also be in alphabetical order
   //     -- 0000 = Reserved but not currently used
   //     -- vv = The Hex value assigned in the Hitachi EtherNet/IP document

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

   // Attribute enum values
   //   The value of the class code enum is
   //     -- 0x = the values that follow are in Hexadecimal 
   //     -- aa = enums are listed in ascending numerical order.  These two Hex
   //             digits cause the sort order to also be in alphabetical order
   //     -- ll = Length of the data to be sent to the printer
   //     -- cc = 00000abc - Right 3 Bit indicate which access codes are valid for this service
   //             a = Allow Set if a = 1
   //             b = Allow Get if b = 1
   //             c = Allow Service if c = 1
   //     -- vv = The Hex value assigned in the Hitachi EtherNet/IP document

   // Attributes within Print Data Management class 0x66
   public enum eipPrint_Data_Management {
      Select_Message = 0x09000464,
      Store_Print_Data = 0x0A000165,
      Delete_Print_Data = 0x03000167,
      Print_Data_Name = 0x07000169,
      List_of_Messages = 0x0600026A,
      Print_Data_Number = 0x0800016B,
      Change_Create_Group_Name = 0x0100016C,
      Group_Deletion = 0x0400016D,
      List_of_Groups = 0x0500026F,
      Change_Group_Number = 0x02000170,
   }

   // Attributes within Print Format class 0x67
   public enum eipPrint_format {
      Message_Name = 0x14010264,
      Print_Item = 0x19010265,
      Number_Of_Columns = 0x15010266,
      Format_Type = 0x0E010267,
      Insert_Column = 0x0F010469,
      Delete_Column = 0x0801046A,
      Add_Column = 0x0101046B,
      Number_Of_Print_Line_And_Print_Format = 0x1601016C,
      Format_Setup = 0x0D01016D,
      Adding_Print_Items = 0x0301046E,
      Deleting_Print_Items = 0x0901046F,
      Print_Character_String = 0x18010371,
      Line_Count = 0x12010372,
      Line_Spacing = 0x13010373,
      Dot_Matrix = 0x0B010374,
      InterCharacter_Space = 0x10010375,
      Character_Bold = 0x07010376,
      Barcode_Type = 0x05010377,
      Readable_Code = 0x1B010378,
      Prefix_Code = 0x17010379,
      X_and_Y_Coordinate = 0x1C01037A,
      InterCharacter_SpaceII = 0x1101037B,
      Add_To_End_Of_String = 0x0201018A,
      Calendar_Offset = 0x0601038D,
      DIN_Print = 0x0A01038E,
      EAN_Prefix = 0x0C01038F,
      Barcode_Printing = 0x04010390,
      QR_Error_Correction_Level = 0x1A010391,
   }

   // Attributes within Print Specification class 0x68
   public enum eipPrint_specification {
      Character_Height = 0x02010364,
      Ink_Drop_Use = 0x08010365,
      High_Speed_Print = 0x06010366,
      Character_Width = 0x04020367,
      Character_Orientation = 0x03010368,
      Print_Start_Delay_Forward = 0x0B020369,
      Print_Start_Delat_Reverse = 0x0A02036A,
      Product_Speed_Matching = 0x0E01036B,
      Pulse_Rate_Division_Factor = 0x0F02036C,
      Speed_Compensation = 0x1201036D,
      Line_Speed = 0x0902036E,
      Distance_Between_Print_Head_And_Object = 0x0501036F,
      Print_Target_Width = 0x0D010370,
      Actual_Print_Width = 0x01010371,
      Repeat_Count = 0x10020372,
      Repeat_Interval = 0x11030373,
      Target_Sensor_Timer = 0x15020374,
      Target_Sensor_Filter = 0x14010375,
      Targer_Sensor_Filter_Value = 0x13020376,
      Ink_Drop_Charge_Rule = 0x07010377,
      Print_Start_Position_Adjustment_Value = 0x0C020378,
   }

   // Attributes within Calendar class 0x69
   public enum eipCalendar {
      Shift_Count_Condition = 0x0A010265,
      First_Calendar_Block_Number = 0x03010266,
      Calendar_Block_Number_In_Item = 0x01010267,
      Offset_Year = 0x08010368,
      Offset_Month = 0x07010369,
      Offset_Day = 0x0402036A,
      Offset_Hour = 0x0502036B,
      Offset_Minute = 0x0602036C,
      Zero_Suppress_Year = 0x2001036D,
      Zero_Suppress_Month = 0x1E01036E,
      Zero_Suppress_Day = 0x1A01036F,
      Zero_Suppress_Hour = 0x1C010370,
      Zero_Suppress_Minute = 0x1D010371,
      Zero_Suppress_Weeks = 0x1F010372,
      Zero_Suppress_Day_Of_Week = 0x1B010373,
      Substitute_Rule_Year = 0x15010374,
      Substitute_Rule_Month = 0x13010375,
      Substitute_Rule_Day = 0x0F010376,
      Substitute_Rule_Hour = 0x11010377,
      Substitute_Rule_Minute = 0x12010378,
      Substitute_Rule_Weeks = 0x14010379,
      Substitute_Rule_Day_Of_Week = 0x1001037A,
      Time_Count_Start_Value = 0x1803037B,
      Time_Count_End_Value = 0x1603037C,
      Time_Count_Reset_Value = 0x1703037D,
      Reset_Time_Value = 0x0901037E,
      Update_Interval_Value = 0x1901037F,
      Shift_Start_Hour = 0x0D010380,
      Shift_Start_Minute = 0x0E010381,
      Shift_End_Hour = 0x0B010382,
      Shift_End_Minute = 0x0C010383,
      Count_String_Value = 0x02010384,
   }

   // Attributes within User Pattern class 0x6B
   public enum eipUser_pattern { // 0x6B
      User_Pattern_Fixed = 0x01000364,
      User_Pattern_Free = 0x02000365,
   }

   // Attributes within Substitution Rules class 0x6C
   public enum eipSubstitution_rules { // 0x6C
      Number = 0x03000364,
      Name = 0x02000365,
      Start_Year = 0x01000366,
      Year = 0x0A000367,
      Month = 0x08000368,
      Day = 0x04000369,
      Hour = 0x0600036A,
      Minute = 0x0700036B,
      Week = 0x0900036C,
      Day_Of_Week = 0x0500036D,
   }

   // Attributes within Enviroment Setting class 0x71
   public enum eipEnviroment_setting {
      Current_Time = 0x05000365,
      Calendar_Date_Time = 0x01000366,
      Calendar_Date_Time_Availibility = 0x02000367,
      Clock_System = 0x04000368,
      User_Environment_Information = 0x08000269,
      Cirulation_Control_Setting_Value = 0x0300026A,
      Usage_Time_Of_Circulation_Control = 0x0700016B,
      Reset_Usage_Time_Of_Citculation_Control = 0x0600016C,
   }

   // Attributes within Unit Information class 0x73
   public enum eipUnit_Information {
      Unit_Information = 0x14000264,
      Model_Name = 0x0F00026B,
      Serial_Number = 0x1100026C,
      Ink_Name = 0x0800026D,
      Input_Mode = 0x0900026E,
      Maximum_Character_Count = 0x0B00026F,
      Maximum_Registered_Message_Count = 0x0D000270,
      Barcode_Information = 0x01000271,
      Usable_Character_Size = 0x15000272,
      Maximum_Calendar_And_Count = 0x0A000273,
      Maximum_Substitution_Rule = 0x0E000274,
      Shift_Code_And_Time_Count = 0x12000275,
      Chimney_And_DIN_Print = 0x03000276,
      Maximum_Line_Count = 0x0C000277,
      Basic_Software_Version = 0x02000278,
      Controller_Software_Version = 0x04000279,
      Engine_M_Software_Version = 0x0500027A,
      Engine_S_Software_Version = 0x0600027B,
      First_Language_Version = 0x0700027C,
      Second_Language_Version = 0x1000027D,
      Software_Option_Version = 0x1300027E,
   }

   // Attributes within Operation Management class 0x74
   public enum eipOperation_management {
      Operating_Management = 0x0C000264,
      Ink_Operating_Time = 0x09020365,
      Alarm_Time = 0x01020366,
      Print_Count = 0x0D020367,
      Communications_Environment = 0x03000268,
      Cumulative_Operation_Time = 0x04000269,
      Ink_And_Makeup_Name = 0x0800026A,
      Ink_Viscosity = 0x0B00026B,
      Ink_Pressure = 0x0A00026C,
      Ambient_Temperature = 0x0200026D,
      Deflection_Voltage = 0x0500026E,
      Excitation_VRef_Setup_Value = 0x0700026F,
      Excitation_Frequency = 0x06000270,
   }

   // Attributes within IJP Operation class 0x75
   public enum eipIJP_operation {
      Remote_operation_information = 0x07000264,
      Fault_and_warning_history = 0x04000266,
      Operating_condition = 0x06000267,
      Warning_condition = 0x0A000268,
      Date_and_time_information = 0x0100026A,
      Error_code = 0x0300026B,
      Start_Remote_Operation = 0x0800046C,
      Stop_Remote_Operation = 0x0900046D,
      Deflection_voltage_control = 0x0200046E,
      Online_Offline = 0x0500036F,
   }

   // Attributes within Count class 0x79
   public enum eipCount {
      Number_Of_Count_Block = 0x0C000266,
      Initial_Value = 0x09000367,
      Count_Range_1 = 0x04000368,
      Count_Range_2 = 0x05000369,
      Update_Unit_Halfway = 0x0F00036A,
      Update_Unit_Unit = 0x1000036B,
      Increment_Value = 0x0800036C,
      Direction_Value = 0x0700036D,
      Jump_From = 0x0A00036E,
      Jump_To = 0x0B00036F,
      Reset_Value = 0x0D000370,
      Type_Of_Reset_Signal = 0x0E000371,
      Availibility_Of_External_Count = 0x01000372,
      Availibility_Of_Zero_Suppression = 0x02000373,
      Count_Multiplier = 0x03000374,
      Count_Skip = 0x06000375,
   }

   // Attributes within Index class 0x7A
   public enum eipIndex {
      Start_Stop_Management_Flag = 0x0A010364,
      Automatic_reflection = 0x01010365,
      Item_Count = 0x06020366,
      Column = 0x04020367,
      Line = 0x07010368,
      Character_position = 0x03020369,
      Print_Data_Message_Number = 0x0902036A,
      Print_Data_Group_Data = 0x0801036B,
      Substitution_Rules_Setting = 0x0B01036C,
      User_Pattern_Size = 0x0C01036D,
      Count_Block = 0x0501036E,
      Calendar_Block = 0x0201036F,
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
            for (int t = 0; t < 20; t++) {
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

         bool OpenCloseConnection = !IsConnected;
         bool OpenCloseSession = !SessionIsOpen;
         bool OpenCloseForward = !ForwardIsOpen;

         if (OpenCloseConnection)
            Connect();
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
                  switch (fmt) {
                     case DataFormats.Decimal:
                        val = Get(ReadData, 50, ReadDataLength - 50, mem.BigEndian).ToString();
                        break;
                     case DataFormats.Bytes:
                        val = GetBytes(ReadData, 50, ReadDataLength - 50);
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
         if (OpenCloseConnection)
            Disconnect();

         return result;
      }

      public bool WriteOneAttribute(eipClassCode Class, byte Attribute, byte[] val) {
         bool result = false;
         bool OpenCloseConnection = !IsConnected;
         bool OpenCloseSession = !SessionIsOpen;
         bool OpenCloseForward = !ForwardIsOpen;

         if (OpenCloseConnection)
            Connect();
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
         if (OpenCloseConnection)
            Disconnect();

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
               Add(packet, (ulong)Data_Type.ConnectedDataItem, 2);    // data tyoe
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
               Add(packet, (ulong)Data_Type.ConnectedDataItem, 2);    // data tyoe
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
      public int GetDropDowns(eipAccessCode code, Type EnumType, ComboBox cb, out uint[] values) {

         // Get all names associated with the enumeration
         string[] allNames = EnumType.GetEnumNames();
         int[] allValues = (int[])EnumType.GetEnumValues();

         // Get the mask for selecting the needed ones
         int mask = 0;
         switch (code) {
            case eipAccessCode.Set:
               mask = 1 << 8;
               break;
            case eipAccessCode.Get:
               mask = 2 << 8;
               break;
            case eipAccessCode.Service:
               mask = 4 << 8;
               break;
            default:
               break;
         }

         // Weed out the unused ones
         List<string> name = new List<string>();
         List<uint> value = new List<uint>();
         for (int i = 0; i < allValues.Length; i++) {
            int x = allValues[i];
            if ((x & mask) > 0) {
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
      public string GetAttributeName(eipClassCode c, uint v) {
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

      public byte[] ToBytes(uint v, int length) {
         byte[] result = new byte[length];
         for (int i = length - 1; i >= 0; i--) {
            result[i] = (byte)(v & 0xFF);
            v >>= 8;
         }
         return result;
      }

      public byte GetAttribute(uint v) {
         return (byte)(v & 0xFF);

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
