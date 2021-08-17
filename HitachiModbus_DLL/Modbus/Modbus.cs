using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UTF8vsHitachiCodes;

namespace Modbus_DLL {
   public class Modbus {

      #region Events

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(object sender, string msg);

      // I/O Complete
      public event CompleteHandler Complete;
      public delegate void CompleteHandler(object sender, bool Success);

      #endregion

      #region Data Declarations

      // Modbus mapping                         0  1  2  3   4   5   6   7   8    9  10 11 12 13   14   15   16
      public static int[] logoLen = new int[] { 0, 8, 8, 8, 16, 16, 32, 32, 72, 128, 32, 5, 5, 7, 200, 288, 512 };
      // Font names                            n/a | 5x5 |  9x8  | 10x12 | 18x24 | 11x11 | 5x5C| 30x40  |  48x64
      //                                        X 4x5   5x8     7x10   12x16   24x32    5x3C  7x5C    36x48

      Form parent;

      // Errors encountered
      public int Errors { get; set; } = 0;

      // Repeat interval for free logos
      public const int FreeLogoSize = 642;

      // I/O buffer size for read/write of logos (32 x 32 bitmap)
      public const int LogoMaxSizeIO = 240;

      // Choose between IJPLib names and EtherNet/IP names
      private bool UseIJPLibNames = true;

      // Log I/O buffers with traffic
      public bool LogIO { get; set; }
      public bool LogAllIO { get; set; }

      public bool StopOnAllErrors = true;
      public string LogIOSpacer {
         get { return LogAllIO ? "\n " : ""; }
      }

      // Modbus function codes
      public enum FunctionCode {
         WriteMultiple = 0x10,
         WriteSingle = 0x06,
         ReadHolding = 0x03,
         ReadInput = 0x04,
      }

      // Error codes
      public enum ErrorCodes {
         None = 0,
         Not_Supported = 1,
         Illegal_Address = 2,
         Illegal_Data = 3,
      }

      // Modbus traffic uses Network Streams
      private TcpClient client = null;
      private NetworkStream stream = null;

      // Must be turned on before connecting
      private bool twinNozzle = false;

      public bool TwinNozzle {
         get { return twinNozzle; }
         set {
            twinNozzle = value;
            NozzleCount = twinNozzle ? 2 : 1;
         }
      }
      public int NozzleCount { get; set; } = 1;
      public int Nozzle { get; set; } = 0;

      // Modbus read packet is of fixed length
      private readonly byte[] ModbusReadPkt = new byte[] { 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0 };

      Encoding Encode = Encoding.GetEncoding("ISO-8859-1");

      // Data Tables describing Hitachi Model 161
      static public Data M161 = new Data();

      // Check on connection and Com State
      public bool IsConnected { get { return stream != null; } }
      public bool ComIsOn { get; private set; } = false;

      // Log I/O as XML file
      public bool LogAsXML = false;

      private int TransactionId { get; set; } = 0;

      private int NextTid() {
         var tid = TransactionId++;
         TransactionId &= 0xFFFF;
         return tid;
      }

      private byte High(int value) {
         return (byte)((value >> 8) & 0xff);
      }

      private byte Low(int value) {
         return (byte)((value >> 0) & 0xff);
      }

      #endregion

      #region Constructors and Destructors

      // Create object and build dictionary of attributes
      public Modbus(Form parent) {
         this.parent = parent;

         Data.BuildAttributeDictionary(ClassCodes, ClassCodeAttributes);
         Data.BuildHumanReadableDictionary();
      }

      // Nothing to do here
      ~Modbus() {

      }

      #endregion

      #region Methods

      // Connect to printer and turn COM on
      public bool Connect(string ipAddress, string ipPort) {
         bool success = false;
         if (int.TryParse(ipPort, out int port)) {
            success = Connect(ipAddress, port);
         }
         return success;
      }

      // Connect to printer and turn COM on
      public bool Connect(string ipAddress, int ipPort) {
         bool success = false;
         try {
            if (IPAddress.TryParse(ipAddress, out IPAddress ipAddr)) {
               client = new TcpClient();
               if (!client.ConnectAsync(ipAddress, ipPort).Wait(2000)) {
                  LogIt("Connection Failed");
                  client = null;
               } else {
                  stream = client.GetStream();
                  LogIt("Connection Accepted");
                  int n = GetDecAttribute(ccIJP.Online_Offline);
                  if (!ComIsOn) {
                     SetAttribute(ccIJP.Online_Offline, 1);
                     n = GetDecAttribute(ccIJP.Online_Offline);
                  }
                  success = true;
               }
            }
         } catch (Exception e) {
            LogIt($"Connection Failed\n{ e.StackTrace}");
            Disconnect();
         }
         return success;
      }

      // Disconnect from TcpClient ad stream
      public void Disconnect() {
         if (stream != null) {
            stream.Close();
            stream.Dispose();
         }
         if (client != null) {
            if (client.Connected) {
               client.Close();
            }
            client.Dispose();
         }
         stream = null;
         client = null;
         ComIsOn = false;
      }

      #endregion

      #region Modbus Read/Write

      // Issue Modbus read request
      private bool Read(out byte[] data, out int bytes) {
         bool successful = false;
         data = new byte[256];
         bytes = -1;
         if (stream != null) {
            try {
               Thread.Sleep(10);
               stream.ReadTimeout = 6000;                 // Allow for up to 6 seconds for a response
               bytes = stream.Read(data, 0, data.Length); // Get number of bytes read
               successful = bytes >= 8;                   // Need to at least get the packet + devAddr, Function code, and length
               DisplayInput(data, bytes);                 // Display the input returned
            } catch (Exception e) {
               if (StopOnAllErrors) {
                  throw new ModbusException(e.Message);
               }
            }
         }
         if (successful) {
            if ((data[7] & 0x80) > 0) {
               string s = $"Device rejected the request \"{(ErrorCodes)data[8]}\".";
               LogIt(s);
               CompleteIt(false);
               successful = false;
               throw new ModbusException(s);
            } else {
               CompleteIt(true);
            }
         } else {
            LogIt("Read Failed.");
         }
         return successful;
      }

      // Issue Modbus write request
      private bool Write(byte[] data) {
         bool successful = false;
         DisplayOutput(data, data.Length);
         if (stream != null) {
            try {
               Thread.Sleep(10);
               stream.Write(data, 0, data.Length);
               successful = true;
            } catch (Exception e) {
               LogIt(e.Message);
            }
         }
         if (!successful) {
            LogIt("Write Failed. Connection Closed!");
         }
         return successful;
      }

      #endregion

      #region Modbus Buffer Builders

      // Build a Modbus write packet with room for data
      private byte[] BuildModbusWrite(FunctionCode fc, byte DevAddr, int addr, int dataBytes) {
         int tid = NextTid();
         int n = dataBytes + (dataBytes & 1);        // Make even number of bytes
         byte[] r = new byte[6 + 7 + n];             // 6 header + 7 packet + n bytes
         r[0] = High(tid);                           // Transaction ID
         r[1] = Low(tid);                            // Transaction ID
         r[2] = 0;                                   // Protocol ID
         r[3] = 0;                                   // Protocol ID
         r[4] = High(7 + n);                         // Packet length high byte
         r[5] = Low(7 + n);                          // Packet length low byte
         r[6] = DevAddr;                             // Device address (Always 0)
         r[7] = (byte)fc;                            // Function Code
         r[8] = High(addr);                          // Start address high byte
         r[9] = Low(addr);                           // Start address low byte
         r[10] = High(n >> 1);                       // Number of words to write high byte
         r[11] = Low(n >> 1);                        // Number of words to write low byte
         r[12] = (byte)n;                            // Number of bytes to write
         return r;
      }

      // Build a Modbus write packet and include the data; addr is in words, start and length are in bytes
      private byte[] BuildModbusWrite(FunctionCode fc, byte DevAddr, int addr, byte[] data, int start = 0, int len = -1) {
         if (len == -1) {
            len = data.Length;
         }
         byte[] r = BuildModbusWrite(fc, DevAddr, addr, len);         // Get a buffer without data
         int n = r.Length - len;                                      // Calculate location where data will be placed
         for (int i = 0; i < len; i++) {                              // Step thru the input buffer
            r[n + i] = data[start + i];                               // move the data to the end of the buffer
         }
         return r;
      }

      // Build a Modbus read packet
      private byte[] BuildModbusRead(FunctionCode fc, byte DevAddr, int addr, int dataBytes) {
         int words = (dataBytes + (dataBytes & 1)) >> 1;         // Round to nearest word
         int tid = NextTid();                                    // Get the next transaction ID
         ModbusReadPkt[0] = High(tid);                           // High byte of transaction ID
         ModbusReadPkt[1] = Low(tid);                            // Low byte of transaction ID
         ModbusReadPkt[6] = DevAddr;                             // Device address
         ModbusReadPkt[7] = (byte)fc;                            // Function Code
         ModbusReadPkt[8] = High(addr);                          // Character position high byte
         ModbusReadPkt[9] = Low(addr);                           // Character position low byte
         ModbusReadPkt[10] = High(words);                        // high byte number of words to read
         ModbusReadPkt[11] = Low(words);                         // low byte number of words to read
         return ModbusReadPkt;
      }

      #endregion

      #region Get Attribute Routines

      // Get the contents of one attribute
      private bool GetAttribute(AttrData attr, out byte[] result) {
         bool success = false;
         byte[] data = null;
         int len = attr.Data.Len;
         switch (attr.Data.Fmt) {
            case DataFormats.UTF8:
               len *= 2;
               break;
            case DataFormats.AttrText:
               len *= 4;
               break;
         }
         FunctionCode fc = attr.HoldingReg ? FunctionCode.ReadHolding : FunctionCode.ReadInput;
         byte devAddr = GetDevAdd(attr);
         byte[] request = BuildModbusRead(fc, devAddr, attr.Val, len);
         //Task.Delay(25);
         if (Write(request)) {
            if (Read(out data, out len)) {
               success = true;
            }
         }
         if (success) {
            result = new byte[len - 9];
            for (int i = 9; i < len; i++) {
               result[i - 9] = data[i];
            }
         } else {
            result = new byte[0];
         }
         return true;
      }

      // Get the contents of one attribute
      public bool GetAttribute(FunctionCode fc, byte DevAddr, int addr, int Len, out byte[] result) {
         Task.Delay(50);
         if (Write(BuildModbusRead(fc, DevAddr, addr, Len)) && Read(out byte[] data, out int len)) {
            result = new byte[len - 9];
            for (int i = 9; i < len; i++) {
               result[i - 9] = data[i];
            }
         } else {
            result = new byte[0];
         }
         return true;
      }

      // Get the contents of one attribute
      public byte[] GetAttribute<T>(T Attribute) where T : Enum {
         AttrData attr = GetAttrData(Attribute);
         if (!GetAttribute(attr, out byte[] result)) {
            result = null;
            ComIsOn = false;
         } else {
            if (attr.Class == ClassCode.IJP_operation && attr.Val == (int)ccIJP.Online_Offline) {
               ComIsOn = GetDecValue(result) > 0;
            }
         }
         return result;
      }

      // Get the contents of one attribute from attribute array
      public byte[] GetAttribute<T>(T Attribute, int n) where T : Enum {
         AttrData attr = GetAttrData(Attribute).Clone();
         Debug.Assert(n < attr.Count);
         attr.Val += n * attr.Stride;
         if (!GetAttribute(attr, out byte[] result)) {
            result = null;
         }
         return result;
      }

      // Get the contents of one attribute
      public byte[] GetAttribute<T>(T Attribute, int n, int length) where T : Enum {
         AttrData attr = GetAttrData(Attribute).Clone();
         Debug.Assert(n < attr.Count);
         attr.Val += n * attr.Stride;
         attr.Data.Len = length;
         if (!GetAttribute(attr, out byte[] result)) {
            result = null;
         }
         return result;
      }

      // Get a block of data starting at an attribute[n], length in words, offset in words
      public byte[] GetAttributeBlock(AttrData attrIn, int n, int length, int offset = 0) {
         AttrData attr = attrIn.Clone();                    // Don't mess up the caller's AttrData
         attr.Data.Fmt = DataFormats.UTF8;                  // Fake it as 16 bit entities
         attr.Val += attr.Stride * n + offset;              // n is array reference plus offset
         attr.Data.Len = length;                            // Length is in 16 bit words
         if (!GetAttribute(attr, out byte[] result)) {
            result = null;
         }
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = " +
            $"{byte_to_string(result, 0, Math.Min(32, result.Length))}{LogIOSpacer}");
         return result;
      }

      // Get the contents of one attribute
      public byte[] GetAttribute(AttrData attr) {
         if (!GetAttribute(attr, out byte[] result)) {
            result = null;
         }
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute<T>(T Attribute) where T : Enum {
         AttrData attr = GetAttrData(Attribute);
         int result = GetDecValue(GetAttribute(Attribute), attr.Data.Fmt == DataFormats.SDecimal);
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = " +
            $"{GetHRValue(attr, result)}{LogIOSpacer}");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute<T>(T Attribute, int n) where T : Enum {
         AttrData attr = GetAttrData(Attribute);
         int result = GetDecValue(GetAttribute(Attribute, n), attr.Data.Fmt == DataFormats.SDecimal);
         Debug.Assert(n < attr.Count);
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}+{n * attr.Stride:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = {result}{LogIOSpacer}");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr) {
         int result = GetDecValue(GetAttribute(attr), attr.Data.Fmt == DataFormats.SDecimal);
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = " +
            $"{GetHRValue(attr, result)}{LogIOSpacer}");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr, int n) {
         AttrData ad = attr.Clone();
         Debug.Assert(n < attr.Count);
         ad.Val += n * attr.Stride;
         int result = GetDecValue(GetAttribute(ad), attr.Data.Fmt == DataFormats.SDecimal);
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}+{n * attr.Stride:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = {result}{LogIOSpacer}");
         return result;
      }

      // Get human readable value of the attribute
      public string GetHRAttribute<T>(T Attribute) where T : Enum {
         byte[] b = GetAttribute(Attribute);
         int d = GetDecValue(b);
         string result = d.ToString();
         AttrData attr = GetAttrData(Attribute);
         if (attr.Data.DropDown != fmtDD.None) {
            result = ToDropdownString(attr.Data, d);
         } else if (attr.Data.Fmt == DataFormats.UTF8) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.SDecimal) {
            result = FormatSignedDec(d, attr.Data.Len);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = \"{result}\"{LogIOSpacer}");
         return result;
      }

      // Get human readable value of the attribute
      public string GetHRAttribute<T>(T Attribute, int n) where T : Enum {
         byte[] b = GetAttribute(Attribute, n);
         int d = GetDecValue(b);
         string result = d.ToString();
         AttrData attr = GetAttrData(Attribute);
         Debug.Assert(n < attr.Count);
         if (attr.Data.DropDown != fmtDD.None) {
            result = ToDropdownString(attr.Data, d);
         } else if (attr.Data.Fmt == DataFormats.UTF8) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.SDecimal) {
            result = FormatSignedDec(d, attr.Data.Len);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}+{n * attr.Stride:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = \"{result}\"{LogIOSpacer}");
         return result;
      }

      private string FormatSignedDec(int d, int n) {
         string result = string.Empty;
         int nBits = (4 - n) * 8;
         result = ((d << nBits) >> nBits).ToString();
         return result;
      }

      // Get human readable value of the attribute
      public string GetHRAttribute<T>(T Attribute, int n, int length) where T : Enum {
         byte[] b = GetAttribute(Attribute, n, length);
         int d = GetDecValue(b);
         string result = d.ToString();
         AttrData attr = GetAttrData(Attribute);
         if (attr.Data.DropDown != fmtDD.None) {
            result = ToDropdownString(attr.Data, d);
         } else if (attr.Data.Fmt == DataFormats.UTF8) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}+{n * attr.Stride:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = \"{result}\"{LogIOSpacer}");
         return result;
      }

      // Get byte[] value of the attribute
      public byte[] GetByteArrayAttribute<T>(T Attribute, int n, int length) where T : Enum {
         byte[] b = GetAttribute(Attribute, n, length);
         AttrData attr = GetAttrData(Attribute);
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}+{n * attr.Stride:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = \"{byte_to_string(b)}\"{LogIOSpacer}");
         return b;
      }

      #endregion

      #region Set Attribute Routines

      // Write to a specific address
      public bool SetAttribute(byte devAddr, int addr, byte[] DataOut, int start = 0, int len = -1) {
         bool Successful = false;
         byte[] request = BuildModbusWrite(FunctionCode.WriteMultiple, devAddr, addr, DataOut, start, len);
         Task.Delay(50);
         if (Write(request)) {
            if (Read(out byte[] data, out int bytesRead)) {
               Successful = true;
            }
         }
         return Successful;
      }

      // Write to a specific address
      public bool SetAttribute(AttrData attr, int offset, byte[] DataOut, int start = 0, int len = -1) {
         return SetAttribute(GetDevAdd(attr), attr.Val + offset, DataOut, start, len);
      }

      // Set one attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int val) where T : Enum {
         bool success = false;
         byte[] data;
         AttrData attr = GetAttrData(Attribute);
         data = FormatOutput(attr.Data, val);
         if (SetAttribute(attr, 0, data)) {
            if (attr.Class == ClassCode.IJP_operation && attr.Val == (int)ccIJP.Online_Offline) {
               ComIsOn = val > 0;
            }
            success = true;
         }
         LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = " +
            $"{GetHRValue(attr, val)}{LogIOSpacer}");
         return success;
      }

      // Set one attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, string s) where T : Enum {
         bool success = true;
         if (!string.IsNullOrEmpty(s)) {
            AttrData attr = GetAttrData(Attribute);
            byte[] data = FormatOutput(attr.Data, s);
            success = SetAttribute(attr, 0, data);
            LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = \"{s}\"{LogIOSpacer}");
         }
         return success;
      }

      // Set one indexed attribute based on the Data Property
      public bool SetAttribute<T, D>(T Attribute, int n, D val) where T : Enum {
         bool success = true;
         byte[] data = null;
         string s = Convert.ToString(val);
         AttrData attr = GetAttrData(Attribute);
         if (IsNumeric(typeof(D))) {
            data = FormatOutput(attr.Data, Convert.ToInt32(val));
            success = SetAttribute(attr, attr.Stride * n, data);
         } else {
            if (!string.IsNullOrEmpty(s)) {
               data = FormatOutput(attr.Data, s);
               success = SetAttribute(attr, attr.Stride * n, data);
            }
         }
         LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}+{attr.Stride * n:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = \"{s}\"{LogIOSpacer}");
         return success;
      }

      // Set one indexed attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int n, byte[] data, int start = 0, int len = -1) where T : Enum {
         bool success = true;
         AttrData attr = GetAttrData(Attribute);
         success = SetAttribute(attr, attr.Stride * n, data, start, len);
         LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}+{attr.Stride * n:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)} = {byte_to_string(data, start, len)}{LogIOSpacer}");
         return success;
      }

      // Write to a specific address
      public bool SetBlockAttribute(AttrData attr, int offset, byte[] data, int start = 0, int len = -1) {
         bool result = SetAttribute(attr, offset + start / 2, data, start, len);
         if (attr.Data.Fmt == DataFormats.UTF8) {
            LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}+{offset + start / 2:X4}] " +
               $"{GetAttributeName(attr.Class, attr.Val)} = {FormatText(data)}{LogIOSpacer}");
         } else {
            LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}+{offset + start / 2:X4}] " +
               $"{GetAttributeName(attr.Class, attr.Val)} = {byte_to_string(data, start, len)}{LogIOSpacer}");
         }
         return result;
      }

      #endregion

      #region Attribute Routines

      // Class Codes to Attributes
      public Type[] ClassCodeAttributes = new Type[] {
            typeof(ccPDR),   // 0x66 Print data registration function
            typeof(ccPDM),   // 0x66 Print data management function
            typeof(ccPF),    // 0x67 Print format function
            typeof(ccPS),    // 0x68 Print specification function
            typeof(ccCal),   // 0x69 Calendar function
            typeof(ccUP),    // 0x6B User pattern function
            typeof(ccSR),    // 0x6C Substitution rules function
            typeof(ccES),    // 0x71 Enviroment setting function
            typeof(ccUS),    // 0x72 Unit Status function
            typeof(ccUI),    // 0x73 Unit Information function
            typeof(ccOM),    // 0x74 Operation management function
            typeof(ccIJP),   // 0x75 IJP operation function
            typeof(ccCount), // 0x79 Count function
            typeof(ccIDX),   // 0x7A Index function
            typeof(ccPC),    // 0x7B Print Contents function
            typeof(ccAPP),   // 0x7C Adjust Print Parameters
            typeof(ccAH),    // 0x7D Alarm History Parameters
            typeof(ccMM),    // 0x7E Manage Messages
            typeof(ccMG),    // 0x7F Manage Groups
      };

      // Get AttrData with just the Enum
      public AttrData GetAttrData(Enum e) {
         return Data.AttrDict[ClassCodes[Array.IndexOf(ClassCodeAttributes, e.GetType())], Convert.ToInt32(e)];
      }

      // Get attribute data for an arbitrary class/attribute
      public AttrData GetAttrData(ClassCode Class, int attr) {
         AttrData[] tab = Data.ClassCodeAttrData[Array.IndexOf(ClassCodes, Class)];
         AttrData result = Array.Find(tab, at => at.Val == attr);
         result.Class = Class;
         return result;
      }

      // Class Codes
      public ClassCode[] ClassCodes = (ClassCode[])Enum.GetValues(typeof(ClassCode));

      // Get the human readable name
      public string GetAttributeName(ClassCode Class, int v) {
         string result;
         int i = Array.IndexOf(ClassCodes, Class);
         if (i >= 0) {
            Type at = ClassCodeAttributes[i];
            result = $"{at.Name}.{Enum.GetName(at, v)}";
         } else {
            result = " "; //  $"Get[{v.ToString("X4")}]";
         }
         return result;
      }

      #endregion

      #region Macro-Operations

      // Simulate Delete All But One
      public void DeleteAllButOne() {
         int lineCount;
         int n = 0;
         int cols = 0;
         LogIt(" \n// Deleting old message\n ");


         // Do the deletes in individual layout mode
         LogIt(" \n// Delete done in Individual Layout mode\n ");
         SetAttribute(ccPF.Format_Setup, "Individual");

         LogIt(" \n// Get number of items\n ");
         int itemCount = GetDecAttribute(ccIDX.Number_Of_Items);

         LogIt(" \n// Calculate number of columns\n ");
         while (n < itemCount) {
            lineCount = GetDecAttribute(ccPF.Line_Count, n);
            n += lineCount;
            cols++;
         }

         LogIt(" \n// Delete all columns\n ");
         for (int i = 0; i < cols; i++) {
            SetAttribute(ccPF.Delete_Column, cols - i);
         }

         LogIt(" \n// Clean up on isle 1\n ");
         SetAttribute(ccPF.Dot_Matrix, 0, "5X5"); // Make sure there is enough room for 6 lines
         SetAttribute(ccPF.Barcode_Type, 0, 0);   // Must be cleared before erasing text
         SetAttribute(ccPC.Print_Erasure, 1);     // Clear text in item
      }

      // Delete message if it exists
      public bool DeleteMessage(int n) {
         int word = (n - 1) / 16;
         int bit = 15 - (n - 1) % 16;
         int reg = GetDecAttribute(ccMM.Registration, word);
         if ((reg & (1 << bit)) > 0) {
            SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            SetAttribute(ccPDM.Delete_Print_Data, n);
            SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }
         return true;
      }

      // Send fixed logo to the printer
      public bool SendFixedLogo(string DotMatrix, int loc, byte[] logo) {
         bool result = false;
         int n = Data.ToDropdownValue(GetAttrData(ccIDX.User_Pattern_Size).Data, DotMatrix);
         if (n >= 0) {
            result = SendFixedLogo(n, loc, logo);
         }
         return result;
      }

      // Send fixed logo to the printer
      public bool SendFixedLogo(int DotMatrix, int loc, byte[] logo) {
         // Load the logo into the pattern area
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         SetAttribute(ccIDX.User_Pattern_Size, DotMatrix);
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         // There may be many characters to send at once
         int i = 0;
         int stride = logoLen[DotMatrix];
         byte[] data = new byte[stride];
         while (i < logo.Length) {
            // Write the registration bit
            int regLoc = loc / 16;
            int regBit = 15 - (loc % 16);
            int regMask = GetDecAttribute(ccUP.User_Pattern_Fixed_Registration, regLoc);
            regMask |= 1 << regBit;
            for (int j = 0; j < Math.Min(stride, logo.Length - i); j++) {
               if ((i + j) < logo.Length) {
                  data[j] = logo[i + j];
               } else {
                  data[j] = 0;
               }
            }
            // Write the pattern data
            SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            SetAttribute(ccUP.User_Pattern_Fixed_Registration, regLoc, regMask);
            for (int k = 0; k < data.Length; k += LogoMaxSizeIO) {
               SetAttribute(ccUP.User_Pattern_Fixed_Data, loc * stride / 2 + k / 2, data, k, Math.Min(LogoMaxSizeIO, data.Length - k));
            }
            SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            loc++;
            i += stride;
         }
         return true;
      }

      // Send free logo to the printer
      public bool SendFreeLogo(int width, int height, int loc, byte[] logo) {
         LogIt($" \n// Set {width}x{height} Free Logo to location {loc}\n ");
         bool result = true;
         // Have to delete the old image if one exists
         int oldWidth;
         if (UPExists(ccUP.User_Pattern_Free_Registration, loc, out int regBit, out int regMask)) {
            oldWidth = GetDecAttribute(ccUP.User_Pattern_Free_Width, loc);
         } else {
            regMask |= 1 << regBit;
            oldWidth = width;
         }
         // Build the write data
         int n = (height + 7) / 8;                                   // Calculate source height in bytes
         int newLength = (logo.Length + n - 1) / n * 4;
         byte[] data = new byte[Math.Max(newLength, oldWidth * 4)];  // Free logos are always 4 bytes per stripe
         int k = 0;
         for (int i = 0; i < newLength; i += 4) {              // Pad the data to 4 bytes per stripe
            for (int j = 0; j < n && k < logo.Length; j++) {
               data[i + j] = logo[k++];
            }
         }
         // Send the logo
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         SetAttribute(ccUP.User_Pattern_Free_Height, loc, height);
         SetAttribute(ccUP.User_Pattern_Free_Width, loc, width);
         SetAttribute(ccUP.User_Pattern_Free_Registration, loc / 16, regMask);
         // Write the pattern
         for (int i = 0; i < data.Length; i += LogoMaxSizeIO) {
            SetAttribute(ccUP.User_Pattern_Free_Data, loc * Modbus.FreeLogoSize + i, data, i, Math.Min(LogoMaxSizeIO, data.Length - i));
         }
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         return result;
      }

      // See if the User Pattern exists
      public bool UPExists(ccUP attribute, int loc, out int regBit, out int regMask) {
         regBit = 15 - (loc % 16);
         regMask = 0; // GetDecAttribute(attribute, loc / 16);
         return (regMask & (1 << regBit)) > 0;
      }

      // Retrieve Fixed logo
      public bool GetFixedLogo(string DotMatrix, int loc, out byte[] data) {
         bool result = false;
         data = null;
         int n = Data.ToDropdownValue(GetAttrData(ccIDX.User_Pattern_Size).Data, DotMatrix);
         if (n >= 0) {
            result = GetFixedLogo(n, loc, out data);
         }
         return result;
      }

      // retrieve fixed logo by Dot Matrix 
      public bool GetFixedLogo(int DotMatrix, int loc, out byte[] data) {
         bool result = false;
         data = null;
         // Load the logo into the pattern area
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         SetAttribute(ccIDX.User_Pattern_Size, DotMatrix);
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         // Does the registration bit indicate a logo exists?
         int regLoc = loc / 16;
         int regBit = 15 - (loc % 16);
         int regMask = GetDecAttribute(ccUP.User_Pattern_Fixed_Registration, regLoc);
         if ((regMask & (1 << regBit)) > 0) {
            AttrData attr = GetAttrData(ccUP.User_Pattern_Fixed_Data).Clone();
            attr.Stride = Modbus.logoLen[DotMatrix] / 2;                             // Get the distance between patterns
            Section<ccUP> upd = new Section<ccUP>(this, attr, loc, attr.Stride, true);
            {
               data = new byte[logoLen[DotMatrix]];
               Buffer.BlockCopy(upd.b, 0, data, 0, data.Length);
            }
            LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}+{loc * logoLen[DotMatrix] / 2:X4}] " +
               $"{ccUP.User_Pattern_Fixed_Data}[{loc}] = \"{byte_to_string(data)}\"{LogIOSpacer}");
            result = true;
         }
         return result;
      }

      // Retrieve free logo
      public bool GetFreeLogo(int loc, out int Width, out int Height, out byte[] data) {
         if (UPExists(ccUP.User_Pattern_Free_Registration, loc, out int regBit, out int regMask)) {
            Width = GetDecAttribute(ccUP.User_Pattern_Free_Width, loc);
            Height = GetDecAttribute(ccUP.User_Pattern_Free_Height, loc);
            byte[] logo = new byte[Width * 4];
            // Bring it in at 128 bytes (32x32 section) at a time
            for (int i = 0; i < Width * 4; i += LogoMaxSizeIO) {
               byte[] part = GetAttribute(ccUP.User_Pattern_Free_Data,
                  loc * Modbus.FreeLogoSize + i,
                  Math.Min(Width * 4 - i, LogoMaxSizeIO));
               Array.Copy(part, 0, logo, i, part.Length);
            }
            // Now compact the data to the real size
            int n = (Height + 7) / 8;  // Calculate height in bytes
            if (n < 4) {
               data = new byte[n * Width];
               int k = 0;
               for (int i = 0; i < Width * 4; i += 4) {
                  for (int j = 0; j < n; j++) {
                     data[k++] = logo[i + j];
                  }
               }
            } else {
               data = logo;
            }
            AttrData attr = GetAttrData(ccUP.User_Pattern_Free_Data);
            LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}+{loc * Modbus.FreeLogoSize:X4}] " +
               $"{ccUP.User_Pattern_Free_Data}[{loc}] = \"{byte_to_string(data)}\"{LogIOSpacer}");

            return true;
         } else {
            Width = -1;
            Height = -1;
            data = null;
            return false;
         }
      }

public bool WriteSelectedItems(int item, string data) {
   bool success = true;
   int itemStart = 0;

   // Get the number of items in the message
   int itemCount = GetDecAttribute(ccIDX.Number_Of_Items);

   // Read the section containing the text length of each item and convert to words
   Section<ccPC> pb = new Section<ccPC>(this, ccPC.Characters_per_Item, 0, itemCount, true);
   int[] cpi = pb.GetWords(0, itemCount);

   // Calculate the length and starting location of the Farm Code Item
   int itemLength = cpi[item - 1];
   for (int i = 0; i < item - 1; i++) {
      itemStart += cpi[i];
   }

   // Format the text to exactly match Farm Code Item
   string itemText = data.PadRight(itemLength).Substring(0, itemLength);

   // Create a section that matches the location of the Item, Fill in the Farm Code, and Write it back.
   Section<ccPC> tpi = new Section<ccPC>(this, ccPC.Print_Character_String, itemStart, itemText.Length * 2, false);
   tpi.SetAttrChrs(itemText, 0);
   tpi.WriteSection();

   // Done!
   return success;
}

      #endregion

      #region ServiceRoutines

      // Get the device address
      private byte GetDevAdd(AttrData attr) {
         byte devAdd = 1;
         if (twinNozzle && attr != null) {
            switch (attr.Nozzle) {
               case Noz.None:
                  devAdd = 1;
                  break;
               case Noz.Current:
                  devAdd = (byte)(Nozzle + 1);
                  break;
               case Noz.Both:
                  devAdd = 3;
                  break;
            }
         }
         return devAdd;
      }

      // Get Nozzle Designation
      internal string GetNozzle(AttrData attr) {
         string noz = "";
         if (TwinNozzle) {
            switch (attr.Nozzle) {
               case Noz.None:
                  noz = "Pr:";
                  break;
               case Noz.Current:
                  noz = $"N{Nozzle + 1}:";
                  break;
               case Noz.Both:
                  noz = "NB:";
                  break;
            }
         }
         return noz;
      }

      // Convert result to decimal value
      public int GetDecValue(byte[] b, bool signed = false) {
         int n = 0;
         for (int i = 0; i < b.Length; i++) {
            n = (n << 8) + b[i];
         }
         if (signed) {
            int nBits = (4 - Math.Min(b.Length, 4)) * 8;
            n = (n << nBits) >> nBits;
         }
         return n;
      }

      // Convert the decimal value to human readable
      public string GetHRValue(AttrData attr, int n) {
         string result;
         if (attr.Data.DropDown != fmtDD.None) {
            result = ToDropdownString(attr.Data, n);
         } else {
            result = n.ToString();
         }
         return result;
      }

      // Convert the decimal value to human readable
      public string GetHRValue<T>(T Attribute, int n) where T : Enum {
         return GetHRValue(GetAttrData(Attribute), n);
      }

      // Display the input byte array as hex
      private void DisplayInput(byte[] input, int len = -1) {
         if (LogAllIO)
            LogIt($"[{len}] << " + byte_to_string(input, len));
      }

      // Display the input byte array as hex
      private void DisplayOutput(byte[] output, int len = -1) {
         if (LogAllIO)
            LogIt($"[{len}] >> " + byte_to_string(output, len));
      }

      // Convert UTF8 string to byte array
      public byte[] string_to_byte(string sIn) {
         string[] s = sIn.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
         byte[] b = new byte[s.Length];
         for (int i = 0; i < s.Length; i++) {
            if (!byte.TryParse(s[i], NumberStyles.HexNumber, null, out b[i])) {
               b[i] = 0;
            }
         }
         return b;
      }

      // Convert byte array to UTF8 string
      public string byte_to_string(byte[] b, int len = -1) {
         StringBuilder s = new StringBuilder(b.Length);
         if (len == -1) {
            len = b.Length;
         }
         for (int i = 0; i < len; i++) {
            s.Append(((int)b[i]).ToString("X2") + " ");
         }
         return s.ToString();
      }

      private string byte_to_string(byte[] b, int start, int len) {
         if (len == -1) {
            len = b.Length;
            start = 0;
         }
         StringBuilder s = new StringBuilder(len);
         for (int i = 0; i < len; i++) {
            s.Append(((int)b[start + i]).ToString("X2") + " ");
         }
         return s.ToString();
      }

      // Text is 2 bytes per character
      public string FormatText(byte[] b, int start = 0) {
         StringBuilder s = new StringBuilder(b.Length);
         for (int i = start; i < b.Length; i += 2) {
            int c = (b[i] << 8) + b[i + 1];
            if (UTF8Hitachi.HitachiToUTF8.ContainsKey(c)) {
               s.Append(UTF8Hitachi.HitachiToUTF8[c]);
            } else {
               s.Append((char)((b[i] << 8) + b[i + 1]));
            }
         }
         return s.ToString().Replace("\x00", "").Replace("}{", "");
      }

      // Text is 4 bytes per character
      private string FormatAttrText(byte[] text) {
         StringBuilder s = new StringBuilder(text.Length);
         for (int i = 0; i < text.Length; i += 4) {
            int c1 = (text[i] << 8) + text[i + 1];
            int c2 = (text[i + 2] << 8) + text[i + 3];
            if (UTF8Hitachi.HitachiToUTF8.ContainsKey(c1)) {
               s.Append(UTF8Hitachi.HitachiToUTF8[c1]);
            } else if (UTF8Hitachi.HitachiToUTF8.ContainsKey(c2)) {
               s.Append(UTF8Hitachi.HitachiToUTF8[c2]);
            } else if (text[i] == 0 && text[i + 2] == 0) {
               s.Append((char)text[i + 3]);
            } else if (text[i + 2] == 0xF1) {
               s.Append($"{{X/{text[i + 3] - 0x40}}}");
            } else if (text[i + 2] == 0xF2 && text[i + 3] >= 0x20 && text[i + 3] <= 0x27) {
               s.Append($"{{X/{text[i + 3] - 0x20 + 192}}}");
            } else if (text[i + 2] == 0xF6) {
               s.Append($"{{Z/{text[i + 3] - 0x40}}}");
            } else {
               s.Append("*");
            }
         }
         string result = s.ToString().Replace("\x00", "");
         if (!string.IsNullOrEmpty(result)) {
            int n = result.IndexOf("}{", 1);
            while (n >= 0) {
               char c = result[n - 1];
               bool CalOrCnt = false;
               for (int j = 0; j < UTF8Hitachi.CalCnt.GetLength(0) && !CalOrCnt; j++) {
                  if (UTF8Hitachi.CalCnt[j, 0] == c) {
                     CalOrCnt = true;
                  }
               }
               if (CalOrCnt && c == result[n + 2]) {
                  result = result.Substring(0, n) + result.Substring(n + 2);
               } else {
                  n = n + 2;
               }
               n = result.IndexOf("}{", n);
            }
         }
         return result.Replace("\x00", "");
      }

      // Format Output
      public byte[] FormatOutput(Prop prop, int n) {
         return ToBytes(n, prop.Len * 2);
      }

      // Convert unsigned integer to byte array
      public byte[] ToBytes(long v, int length) {
         byte[] result = new byte[length];
         for (int i = length - 1; i >= 0; i--) {
            result[i] = (byte)(v & 0xFF);
            v >>= 8;
         }
         return result;
      }

      // Get list of names for conversion to human readable
      public string[] GetDropDownNames(int n) {
         if (UseIJPLibNames) {
            return Data.DropDownsIJPLib[n];
         } else {
            return Data.DropDowns[n];
         }
      }

      // Format output
      public byte[] FormatOutput(Prop prop, string s) {
         if (prop.Len == 0) {
            return new byte[0];
         }
         byte[] result = null;
         string[] sa;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
            case DataFormats.SDecimal:
               if (int.TryParse(s, out int val)) {
                  result = ToBytes(val, prop.Len);
               } else if (bool.TryParse(s, out bool b)) {
                  val = b ? 1 : 0;
                  result = ToBytes(val, prop.Len);
               } else {
                  // Translate dropdown back to a number
                  if (prop.DropDown != fmtDD.None) {
                     result = ToBytes(Data.ToDropdownValue(prop, s), prop.Len);
                  }
               }
               break;
            case DataFormats.UTF8:
            case DataFormats.AttrText:
               string s2 = s;
               int width;
               if (s2.Length > 1 && s2.StartsWith("\"") && s2.EndsWith("\"")) {
                  s2 = FromQuoted(s2);
               }
               // This is an issue since the data in the printer is not UTF-8
               if (prop.Fmt == DataFormats.AttrText) {
                  width = 4;
               } else {
                  s2 = s2.PadRight(prop.Len);
                  width = 2;
               }
               result = new byte[s2.Length * width];
               for (int i = 0; i < s2.Length; i++) {
                  char c = s2[i];
                  if (Array.FindIndex<Char>(UTF8Hitachi.CalCntChars, x => x == c) >= 0) {
                     result[i * width + 0] = (byte)(c >> 8);
                     result[i * width + 1] = (byte)c;
                  } else {
                     if (UTF8Hitachi.UTF8ToHitachi.ContainsKey(s2.Substring(i, 1))) {
                        c = (char)UTF8Hitachi.UTF8ToHitachi[s2.Substring(i, 1)];
                     }
                     if (prop.Fmt == DataFormats.UTF8) {
                        result[i * width + 0] = (byte)(c >> 8);
                        result[i * width + 1] = (byte)c;
                     } else {
                        result[i * width + 2] = (byte)(c >> 8);
                        result[i * width + 3] = (byte)c;
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
         }
         if (result == null) {
            result = new byte[0];
         }
         return result;
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
            Buffer.BlockCopy(b[i], 0, result, n, b[i].Length);
            n += b[i].Length;
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

      // Convert Dropdown value to Dropdown HR string
      public string ToDropdownString(Prop prop, int n) {
         string result;
         if ((int)prop.DropDown < (int)fmtDD.ConnectionStatus) {
            n = n - prop.Min;
         }
         if (!Data.HR_Dict.TryGetValue(new Tuple<fmtDD, char>(prop.DropDown, (char)n), out result)) {
            result = n.ToString();
         }
         return result;
      }

      // Common logging to handle Cross-Thread traffic
      internal void LogIt(string msg) {
         if (Log != null && LogIO) {
            if (parent.InvokeRequired) {
               // Do not use BeginInvoke.  Causes issues up-stream.
               parent.BeginInvoke(new EventHandler(delegate { Log(this, msg); }));
            } else {
               Log(this, msg);
            }
         }
      }

      // Common logging to handle Cross-Thread traffic
      private void CompleteIt(bool success) {
         if (!success) {
            Errors++;
         }
         if (Complete != null) {
            if (parent.InvokeRequired) {
               // Do not use BeginInvoke.  Causes issues up-stream.
               parent.BeginInvoke(new EventHandler(delegate { Complete(this, success); }));
            } else {
               Complete(this, success);
            }
         }
      }

      // Things that can be converted to number
      public bool IsNumeric(Type type) {
         switch (Type.GetTypeCode(type)) {
            case TypeCode.Boolean:
            case TypeCode.Byte:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
               return true;
         }
         return false;
      }

      // Set a bit in Hitachi's registry
      public void SetBit(int loc, byte[] b) {
         int byt = loc >> 3;                      // Get the byte within the array
         int msk = 0x80 >> (loc & 7);             // Mask for setting bit (high to low)
         b[byt] |= (byte)msk;                     // Set the bit
      }

      // Clear a bit in Hitachi's registry
      public void ClearBit(int loc, byte[] b) {
         int byt = loc >> 3;                      // Get the byte within the array
         int msk = 0x80 >> (loc & 7);            // Mask for setting bit (high to low)
         b[byt] &= (byte)~msk;                    // Clear the bit
      }

      // Test a bit in Hitachi's registry
      public bool CheckBit(int loc, byte[] b) {
         int byt = loc >> 3;                      // Get the byte within the array
         int msk = 0x80 >> (loc & 7);            // Mask for setting bit (high to low)
         return (b[byt] & (byte)msk) != 0;        // Clear the bit
      }

      #endregion

   }

   #region Modbus Exception Class

   // Modbus exception when something goes wrong
   public class ModbusException : Exception {

      public AccessCode AccessCode;
      public ClassCode ClassCode;
      public byte Attribute;

      // Just use the base definition
      public ModbusException() : base() {

      }

      // Just use the base definition
      public ModbusException(string message) : base(message) {

      }

      // More info but may never need
      public ModbusException(string message, AccessCode AccessCode, ClassCode ClassCode, byte Attribute) : base(message) {
         this.AccessCode = AccessCode;
         this.ClassCode = ClassCode;
         this.Attribute = Attribute;
      }

   }

   #endregion

}
