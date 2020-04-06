using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

      Form parent;

      // Repeat interval for free logos
      public const int FreeLogoSize = 642;

      // I/O buffer size for read/write of logos (32 x 32 bitmap)
      public const int LogoMaxSizeIO = 128;

      // Choose between IJPLib names and EtherNet/IP names
      private bool UseIJPLibNames = true;

      // Log I/O buffers with traffic
      public bool LogIOs;
      public bool StopOnAllErrors = true;
      public string LogIOSpacer {
         get { return LogIOs ? "\n " : ""; }
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
      private byte[] ModbusReadPkt = new byte[] { 0, 0, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0 };

      Encoding Encode = Encoding.GetEncoding("ISO-8859-1");

      // Data Tables describing Hitachi Model 161
      static public Data M161 = new Data();

      // Check on connection and Com State
      public bool IsConnected { get { return stream != null; } }
      public bool ComIsOn { get; private set; } = false;

      // Log I/O as XML file
      public bool LogAsXML = false;

      #endregion

      #region Constructors and Destructors

      // Create object and build dictionary of attributes
      public Modbus(Form parent) {
         this.parent = parent;

         Data.BuildAttributeDictionary(ClassCodes, ClassCodeAttributes);
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
               stream.ReadTimeout = 2000;                 // Allow for up to 2 seconds for a response
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
         int n = dataBytes + (dataBytes & 1);        // Make even number of bytes
         byte[] r = new byte[6 + 7 + n];             // 6 header + 7 packet + n bytes
         r[0] = 0;                                   // Transaction ID
         r[1] = 0;                                   // Transaction ID
         r[2] = 0;                                   // Protocol ID
         r[3] = 0;                                   // Protocol ID
         r[4] = (byte)((7 + n) >> 8);                // Packet length high byte
         r[5] = (byte)(7 + n);                       // Packet length low byte
         r[6] = DevAddr;                             // Device address (Always 0)
         r[7] = (byte)fc;                            // Function Code
         r[8] = (byte)(addr >> 8);                   // Start address high byte
         r[9] = (byte)addr;                          // Start address low byte
         r[10] = (byte)(n >> 9);                     // Number of words to write high byte
         r[11] = (byte)(n >> 1);                     // Number of words to write low byte
         r[12] = (byte)n;                            // Number of bytes to write
         return r;
      }

      // Build a Modbus write packet and include the data
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
         ModbusReadPkt[6] = DevAddr;                             // Device address
         ModbusReadPkt[7] = (byte)fc;                            // Function Code
         ModbusReadPkt[8] = (byte)(addr >> 8);                   // Character position high byte
         ModbusReadPkt[9] = (byte)addr;                          // Character position low byte
         ModbusReadPkt[10] = (byte)(words >> 8);                 // high byte number of words to read
         ModbusReadPkt[11] = (byte)words;                        // low byte number of words to read
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
         Task.Delay(50);
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
         byte[] result;
         AttrData attr = GetAttrData(Attribute);
         if (!GetAttribute(attr, out result)) {
            result = null;
            ComIsOn = false;
         } else {
            if (attr.Class == ClassCode.IJP_operation && attr.Val == (int)ccIJP.Online_Offline) {
               ComIsOn = GetDecValue(result) > 0;
            }
         }
         return result;
      }

      // Get the contents of one attribute
      public byte[] GetAttribute<T>(T Attribute, int n) where T : Enum {
         byte[] result;
         AttrData attr = GetAttrData(Attribute).Clone();
         Debug.Assert(n < attr.Count);
         attr.Val += n * attr.Stride;
         if (!GetAttribute(attr, out result)) {
            result = null;
         }
         return result;
      }

      // Get the contents of one attribute
      public byte[] GetAttribute<T>(T Attribute, int n, int length) where T : Enum {
         byte[] result;
         AttrData attr = GetAttrData(Attribute).Clone();
         Debug.Assert(n < attr.Count);
         attr.Val += n * attr.Stride;
         attr.Data.Len = length;
         if (!GetAttribute(attr, out result)) {
            result = null;
         }
         return result;
      }

      // Get the contents of one attribute
      public byte[] GetAttribute(AttrData attr) {
         byte[] result;
         if (!GetAttribute(attr, out result)) {
            result = null;
         }
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute<T>(T Attribute) where T : Enum {
         int result = GetDecValue(GetAttribute(Attribute));
         AttrData attr = GetAttrData(Attribute);
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = " +
            $"{GetHRValue(attr, result)}{LogIOSpacer}");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute<T>(T Attribute, int n) where T : Enum {
         int result = GetDecValue(GetAttribute(Attribute, n));
         AttrData attr = GetAttrData(Attribute);
         Debug.Assert(n < attr.Count);
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}+{n * attr.Stride:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = {result}{LogIOSpacer}");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr) {
         int result = GetDecValue(GetAttribute(attr));
         LogIt($"Get[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = " +
            $"{GetHRValue(attr, result)}{LogIOSpacer}");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr, int n) {
         AttrData ad = attr.Clone();
         Debug.Assert(n < attr.Count);
         ad.Val += n * attr.Stride;
         int result = GetDecValue(GetAttribute(ad));
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
         AttrData attr = GetAttrData(Attribute);
         if (!string.IsNullOrEmpty(s)) {
            //AutomaticReflect(AccessCode.Set);
            byte[] data = FormatOutput(attr.Data, s);
            success = SetAttribute(attr, 0, data);
         }
         LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = \"{s}\"{LogIOSpacer}");
         return success;
      }

      // Set one indexed attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int n, string s) where T : Enum {
         bool success = true;
         AttrData attr = GetAttrData(Attribute);
         if (!string.IsNullOrEmpty(s)) {
            //AutomaticReflect(AccessCode.Set);
            byte[] data = FormatOutput(attr.Data, s);
            success = SetAttribute(attr, attr.Stride * n, data);
         }
         LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}+{attr.Stride * n:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = \"{s}\"{LogIOSpacer}");
         return success;
      }

      // Set one indexed attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int n, int val) where T : Enum {
         bool success = true;
         AttrData attr = GetAttrData(Attribute);
         //AutomaticReflect(AccessCode.Set);
         byte[] data = FormatOutput(attr.Data, val);
         success = SetAttribute(attr, attr.Stride * n, data);
         LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}+{attr.Stride * n:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)}[{n + attr.Origin}] = {val}{LogIOSpacer}");
         return success;
      }

      // Set one indexed attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int n, byte[] data, int start = 0, int len = -1) where T : Enum {
         bool success = true;
         AttrData attr = GetAttrData(Attribute);
         //AutomaticReflect(AccessCode.Set);
         success = SetAttribute(attr, attr.Stride * n, data, start, len);
         LogIt($"Set[{GetNozzle(attr)}{attr.Val:X4}+{attr.Stride * n:X4}] " +
            $"{GetAttributeName(attr.Class, attr.Val)} = {byte_to_string(data, start, len)}{LogIOSpacer}");
         return success;
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
            result = Enum.GetName(at, v);
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
         LogIt(" \n// Get number of items\n ");
         int itemCount = GetDecAttribute(ccIDX.Number_Of_Items);

         LogIt(" \n// Calculate number of columns\n ");
         while (n < itemCount) {
            lineCount = GetDecAttribute(ccPF.Line_Count, n);
            n += lineCount;
            cols++;
         }

         LogIt(" \n// Delete all columns but the first one\n ");
         for (int i = 0; i < cols - 1; i++) {
            SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
            SetAttribute(ccPF.Delete_Column, cols - i);
            SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         }

         LogIt(" \n// Set first column to line count of 1 and clear the item\n ");
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         SetAttribute(ccPF.Column, 1);
         SetAttribute(ccPF.Line, 1);
         SetAttribute(ccPC.Print_Erasure, 1);
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         LogIt(" \n// Set the format to the smallest size\n ");
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         SetAttribute(ccPF.Dot_Matrix, 0, "5x8");
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
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
         int[] logoLen = new int[] { 0, 8, 8, 8, 16, 16, 32, 32, 72, 128, 32, 5, 5, 7, 200, 288 };
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
               SetAttribute(ccUP.User_Pattern_Fixed_Data, loc * stride / 2 + k, data, k, Math.Min(LogoMaxSizeIO, data.Length - k));
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
         regMask = GetDecAttribute(attribute, loc / 16);
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
         int[] logoLen = new int[] { 0, 8, 8, 8, 16, 16, 32, 32, 72, 128, 32, 5, 5, 7, 200, 288 };
         // Load the logo into the pattern area
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         SetAttribute(ccIDX.User_Pattern_Size, DotMatrix);
         SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);

         // Does the registration bit indicate a logo exists?
         int regLoc = loc / 16;
         int regBit = 15 - (loc % 16);
         AttrData attr = GetAttrData(ccUP.User_Pattern_Fixed_Registration);
         int regMask = GetDecAttribute(ccUP.User_Pattern_Fixed_Registration, regLoc);
         if ((regMask & (1 << regBit)) > 0) {
            data = GetAttribute(ccUP.User_Pattern_Fixed_Data, loc * logoLen[DotMatrix] / 2, logoLen[DotMatrix]);
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

      #endregion

      #region ServiceRoutines

      // Get the device address
      private byte GetDevAdd(AttrData attr) {
         byte devAdd = 0;
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
      private string GetNozzle(AttrData attr) {
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
      public int GetDecValue(byte[] b) {
         int n = 0;
         for (int i = 0; i < b.Length; i++) {
            n = (n << 8) + b[i];
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
         if (LogIOs)
            LogIt($"[{len}] << " + byte_to_string(input, len));
      }

      // Display the input byte array as hex
      private void DisplayOutput(byte[] output, int len = -1) {
         if (LogIOs)
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
         string s = "";
         if (len == -1) {
            len = b.Length;
         }
         for (int i = 0; i < len; i++) {
            s += ((int)b[i]).ToString("X2") + " ";
         }
         return s;
      }

      private object byte_to_string(byte[] b, int start, int len) {
         string s = "";
         if (len == -1) {
            len = b.Length;
            start = 0;
         }
         for (int i = 0; i < len; i++) {
            s += ((int)b[start + i]).ToString("X2") + " ";
         }
         return s;
      }

      // Text is 2 bytes per character
      private string FormatText(byte[] b) {
         string result = "";
         for (int i = 0; i < b.Length; i += 2) {
            result += (char)b[i + 1];
         }
         return result.Replace("\x00", "");
      }

      // Text is 4 bytes per character
      private string FormatAttrText(byte[] text) {
         string result = "";
         for (int i = 0; i < text.Length; i += 4) {
            if (text[i] == 0 && text[i + 2] == 0) {
               result += (char)text[i + 3];
            } else if (text[i + 2] == 0xF1) {
               result += $"{{X/{text[i + 3] - 0x40}}}";
            } else if (text[i + 2] == 0xF2 && text[i + 3] >= 0x20 && text[i + 3] <= 0x27) {
               result += $"{{X/{text[i + 3] - 0x20 + 192}}}";
            } else if (text[i + 2] == 0xF6) {
               result += $"{{Z/{text[i + 3] - 0x40}}}";
            } else if (text[i] == 0xF2) {
               switch (text[i + 1]) {
                  case 0x50:
                     result += "{Y}";
                     break;
                  case 0x60:
                     result += "{{Y}";
                     break;
                  case 0x70:
                     result += "{Y}}";
                     break;
                  case 0x51:
                  case 0x57:
                     result += "{M}";
                     break;
                  case 0x61:
                  case 0x67:
                     result += "{{M}";
                     break;
                  case 0x71:
                  case 0x77:
                     result += "{M}}";
                     break;
                  case 0x52:
                     result += "{D}";
                     break;
                  case 0x62:
                     result += "{{D}";
                     break;
                  case 0x72:
                     result += "{D}}";
                     break;
                  case 0x53:
                     result += "{h}";
                     break;
                  case 0x63:
                     result += "{{h}";
                     break;
                  case 0x73:
                     result += "{h}}";
                     break;
                  case 0x54:
                     result += "{m}";
                     break;
                  case 0x64:
                     result += "{{m}";
                     break;
                  case 0x74:
                     result += "{m}}";
                     break;
                  case 0x55:
                     result += "{s}";
                     break;
                  case 0x65:
                     result += "{{s}";
                     break;
                  case 0x75:
                     result += "{s}}";
                     break;
                  case 0x56:
                     result += "{T}";
                     break;
                  case 0x66:
                     result += "{{T}";
                     break;
                  case 0x76:
                     result += "{T}}";
                     break;
                  case 0x58:
                     result += "{W}";
                     break;
                  case 0x68:
                     result += "{{W}";
                     break;
                  case 0x78:
                     result += "{W}}";
                     break;
                  case 0x59:
                     result += "{7}";
                     break;
                  case 0x69:
                     result += "{{7}";
                     break;
                  case 0x79:
                     result += "{7}}";
                     break;
                  case 0x5B:
                  case 0x6B:
                  case 0x7B:
                     result += "{E}";
                     break;
                  case 0x5C:
                  case 0x6C:
                  case 0x7C:
                     result += "{F}";
                     break;
                  case 0x5A:
                     result += "{C}";
                     break;
                  case 0x6A:
                     result += "{{C}";
                     break;
                  case 0x7A:
                     result += "{C}}";
                     break;
                  case 0X40:
                     result += "{'}";
                     break;
                  case 0X41:
                     result += "{.}";
                     break;
                  case 0X42:
                     result += "{:}";
                     break;
                  case 0X43:
                     result += "{,}";
                     break;
                  case 0X44:
                     result += "{ }";
                     break;
                  case 0X45:
                     result += "{;}";
                     break;
                  case 0X46:
                     result += "{!}";
                     break;
                  default:
                     result += "*";
                     break;
               }
            } else {
               result += "*";
            }
         }
         result = result.Replace("\x00", "");
         int n = result.IndexOf("}{", 1);
         while (n >= 0) {
            char c = result[n - 1];
            bool CalOrCnt = false;
            for (int j = 0; j < M161.CalCnt.GetLength(0) && !CalOrCnt; j++) {
               if (M161.CalCnt[j, 0] == c) {
                  CalOrCnt = true;
               }
            }
            if (CalOrCnt) {
               result = result.Substring(0, n) + result.Substring(n + 2);
            } else {
               n = n + 2;
            }
            n = result.IndexOf("}{", n);
         }
         return result.Replace("\x00", "");
      }

      // Format Output
      public byte[] FormatOutput(Prop prop, int n) {
         return ToBytes(n, prop.Len);
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
         int val;
         byte[] result = null;
         string[] sa;
         switch (prop.Fmt) {
            case DataFormats.Decimal:
            case DataFormats.SDecimal:
               if (int.TryParse(s, out val)) {
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
                  if (Array.FindIndex<Char>(M161.CalCntChars, x => x == c) >= 0) {
                     result[i * width] = (byte)(c >> 8);
                     result[i * width + 1] = (byte)c;
                  } else {
                     result[(i + 1) * width - 2] = (byte)(c >> 8);
                     result[(i + 1) * width - 1] = (byte)c;
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

      // Convert Hitachi Braced notation to characters
      public string HandleBraces(string s1) {
         // Braced Characters (count, date, half-size, logos
         string s2 = s1;

         for (int i = 0; i < M161.HalfSize.GetLength(0); i++) {
            s2 = s2.Replace(M161.HalfSize[i, 0], M161.HalfSize[i, 1]);
         }

         for (int i = 0; i < s2.Length; i++) {
            int j;
            i = s2.IndexOf("{X/", i);
            if (i >= 0 && (j = s2.IndexOf("}", i + 3)) > i &&
               int.TryParse(s2.Substring(i + 3, j - i - 3), out int n)) {
               if (n < 192) {
                  s2 = s2.Substring(0, i) + (char)('\uF140' + n) + s2.Substring(j + 1);
               } else {
                  s2 = s2.Substring(0, i) + (char)('\uF220' + n - 192) + s2.Substring(j + 1);
               }
            } else {
               break;
            }
         }

         for (int i = 0; i < s2.Length; i++) {
            int j;
            i = s2.IndexOf("{Z/", i);
            if (i >= 0 && (j = s2.IndexOf("}", i + 3)) > i &&
               int.TryParse(s2.Substring(i + 3, j - i - 3), out int n)) {
               s2 = s2.Substring(0, i) + (char)('\uF640' + n) + s2.Substring(j + 1);
            } else {
               break;
            }
         }

         int firstFound = s2.Length;
         int lastFound = -1;
         string result = "";
         int bCount = 0;
         for (int i = 0; i < s2.Length; i++) {
            char c = s2[i];
            switch (c) {
               case '{':
                  bCount++;
                  break;
               case '}':
                  bCount--;
                  break;
               default:
                  if (bCount == 0) {
                     result += c;
                  } else {
                     bool found = false;
                     for (int j = 0; j < M161.CalCnt.GetLength(0) && !found; j++) {
                        if (M161.CalCnt[j, 0] == c) {
                           firstFound = Math.Min(firstFound, result.Length);
                           lastFound = Math.Max(lastFound, result.Length);
                           result += M161.CalCnt[j, 1];
                           found = true;
                        }
                     }
                     if (!found) {
                        result += c;
                     }
                  }
                  break;
            }
         }
         if (firstFound != lastFound && firstFound < s2.Length) {
            result = result.Substring(0, firstFound) + (char)(result[firstFound] + 0x10) + result.Substring(firstFound + 1);
         }
         if (firstFound != lastFound && lastFound >= 0) {
            result = result.Substring(0, lastFound) + (char)(result[lastFound] + 0x20) + result.Substring(lastFound + 1);
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
            for (int j = 0; j < b[i].Length; j++) {
               result[n++] = b[i][j];
            }
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

      //// Convert Dropdown HR string to Dropdown value
      //public int ToDropdownValue(Prop prop, string s) {
      //   int val;
      //   s = s.ToLower();
      //   val = Array.FindIndex(Data.DropDowns[(int)prop.DropDown], x => x.ToLower().Contains(s));
      //   if (val < 0) {
      //      val = Array.FindIndex(Data.DropDownsIJPLib[(int)prop.DropDown], x => x.ToLower().Contains(s));
      //   }
      //   if (val >= 0) {
      //      val += (int)prop.Min;
      //   }
      //   return val;
      //}

      // Convert Dropdown value to Dropdown HR string
      public string ToDropdownString(Prop prop, int n) {
         string result;
         string[] dd = GetDropDownNames((int)prop.DropDown);
         n = n - prop.Min;
         if (n >= 0 && n < dd.Length) {
            result = dd[n];
         } else {
            result = n.ToString();
         }
         return result;
      }

      // Common logging to handle Cross-Thread traffic
      private void LogIt(string msg) {
         if (Log != null) {
            if (parent.InvokeRequired) {
               // Do not use BeginInvoke.  Causes issues up-stream.
               parent.Invoke(new EventHandler(delegate { Log(this, msg); }));
            } else {
               Log(this, msg);
            }
         }
      }

      // Common logging to handle Cross-Thread traffic
      private void CompleteIt(bool success) {
         if (Complete != null) {
            if (parent.InvokeRequired) {
               // Do not use BeginInvoke.  Causes issues up-stream.
               parent.Invoke(new EventHandler(delegate { Complete(this, success); }));
            } else {
               Complete(this, success);
            }
         }
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
