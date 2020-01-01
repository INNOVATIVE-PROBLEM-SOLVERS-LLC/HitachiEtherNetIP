using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Modbus_DLL {
   public class Modbus {

      #region Events

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(Modbus sender, string msg);

      #endregion

      #region Data Declarations

      bool UseIJPLibNames = true;

      bool LogIOs = true;

      enum FunctionCode {
         WriteMultiple = 0x10,
         WriteSingle = 0x06,
         ReadHolding = 0x03,
         ReadInput = 0x04,
      }

      TcpClient client = null;
      NetworkStream stream = null;

      bool comIsOn = false;

      Encoding Encode = Encoding.GetEncoding("ISO-8859-1");

      // Data Tables describing Hitachi Model 161
      static public Data M161 = new Data();

      // Check on connection and Com State
      public bool IsConnected { get { return stream != null; } }
      public bool ComIsOn { get { return comIsOn; } }

      #endregion

      #region Constructors and Destructors

      // Create object and build dictionary of attributes
      public Modbus() {
         BuildAttributeDictionary();
      }

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
         if (IPAddress.TryParse(ipAddress, out IPAddress ipAddr)) {
            client = new TcpClient(ipAddress, ipPort);
            stream = client.GetStream();
            Log?.Invoke(this, "Connection Accepted");
            int n = GetDecAttribute(ccIJP.Online_Offline);
            if (!comIsOn) {
               SetAttribute(ccIJP.Online_Offline, 1);
               n = GetDecAttribute(ccIJP.Online_Offline);
               success = true;
            }
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
            if(client.Connected) {
               client.Close();
            }
            client.Dispose();
         }
         stream = null;
         client = null;
         comIsOn = false;
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
               // Allow for up to 5 seconds for a response
               stream.ReadTimeout = 5000;
               bytes = stream.Read(data, 0, data.Length);
               successful = bytes >= 0;
               DisplayInput(data, bytes);
            } catch (Exception e) {
               Log?.Invoke(this, e.Message);
            }
         }
         if (!successful) {
            Log?.Invoke(this, "Read Failed.");
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
               Log?.Invoke(this, e.Message);
            }
         }
         if (!successful) {
            Log?.Invoke(this, "Write Failed. Connection Closed!");
         }
         return successful;
      }

      #endregion

      #region Modbus Buffer Builders

      // Build a Modbus write packet with room for data
      private byte[] BuildModbusWrite(FunctionCode fc, int loc, int dataBytes) {
         int n = dataBytes + (dataBytes & 1);          // Make even number of bytes
         byte[] r = new byte[6 + 7 + n];             // Allocate the buffer
         r[0] = 0;                                   // Transaction ID
         r[1] = 0;                                   // Transaction ID
         r[2] = 0;                                   // Protocol ID
         r[3] = 0;                                   // Protocol ID
         r[4] = (byte)((7 + n) >> 8);                // Packet length high byte
         r[5] = (byte)(7 + n);                       // Packet length low byte
         r[6] = 0;                                   // Device address (Always 0)
         r[7] = (byte)fc;                            // Function Code
         r[8] = (byte)(loc >> 8);                    // Start address high byte
         r[9] = (byte)loc;                           // Start address low byte
         r[10] = (byte)(n >> 9);                     // Number of words to write high byte
         r[11] = (byte)(n >> 1);                     // Number of words to write low byte
         r[12] = (byte)n;                            // Number of bytes to write
         return r;
      }

      // Build a Modbus write packet and include the data
      private byte[] BuildModbusWrite(FunctionCode fc, int loc, byte[] data) {
         byte[] r = BuildModbusWrite(fc, loc, data.Length); // Get a buffer without data
         int n = r.Length - data.Length;                    // Calculate location where data will be placed
         for (int i = 0; i < data.Length; i++) {            // Step thru the input buffer
            r[n + i] = data[i];                             // move the data to the end of the buffer
         }
         return r;
      }

      // Build a Modbus read packet
      private byte[] BuildModbusRead(FunctionCode fc, int loc, int dataBytes) {
         byte[] r = new byte[12];
         r[4] = 0;                                   // Packet length high byte
         r[5] = 6;                                   // Packet length low byte
         r[6] = 0;                                   // Device address
         r[7] = (byte)fc;                            // Function Code
         r[8] = (byte)(loc >> 8);                    // Character position high byte
         r[9] = (byte)loc;                           // Character position low byte
         r[10] = 0;                                  // high byte number of words to read
         r[11] = (byte)((dataBytes + 1) >> 1);       // low byte number of words write
         return r;
      }

      #endregion

      #region Get Attribute Routines

      // Get the contents of one attribute
      private bool GetAttribute(AttrData attr, out byte[] result) {
         bool success = false;
         byte[] data = null;
         int len = attr.Data.Len;
         switch (attr.Data.Fmt) {
            case DataFormats.None:
               break;
            case DataFormats.Decimal:
               break;
            case DataFormats.SDecimal:
               break;
            case DataFormats.UTF8:
            case DataFormats.UTF8N:
               len *= 2;
               break;
            case DataFormats.Date:
               break;
            case DataFormats.Bytes:
               break;
            case DataFormats.XY:
               break;
            case DataFormats.N2N2:
               break;
            case DataFormats.N2Char:
               break;
            case DataFormats.ItemChar:
               break;
            case DataFormats.Item:
               break;
            case DataFormats.GroupChar:
               break;
            case DataFormats.MsgChar:
               break;
            case DataFormats.N1Char:
               break;
            case DataFormats.N1N1:
               break;
            case DataFormats.N1N2N1:
               break;
            case DataFormats.AttrText:
               len *= 4;
               break;
            default:
               break;
         }
         byte[] request = BuildModbusRead(attr.HoldingReg ? FunctionCode.ReadHolding : FunctionCode.ReadInput, attr.Val, len);
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
      public bool GetAttribute(int addr, int Len, bool HoldingReg, out byte[] result) {
         bool success = false;
         byte[] data = null;
         int len = 10;
         byte[] request = BuildModbusRead(HoldingReg ? FunctionCode.ReadHolding : FunctionCode.ReadInput, addr, Len);
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
      public byte[] GetAttribute<T>(T Attribute) where T : Enum {
         byte[] result;
         AttrData attr = GetAttrData(Attribute);
         if (!GetAttribute(attr, out result)) {
            result = null;
            comIsOn = false;
         } else {
            if (attr.Class == ClassCode.IJP_operation && attr.Val == (int)ccIJP.Online_Offline) {
               comIsOn = GetDecValue(result) > 0;
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
         Log?.Invoke(this, $"Get[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = {GetHRValue(attr, result)}");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute<T>(T Attribute, int n) where T : Enum {
         int result = GetDecValue(GetAttribute(Attribute, n));
         AttrData attr = GetAttrData(Attribute);
         Debug.Assert(n < attr.Count);
         Log?.Invoke(this, $"Get[{attr.Val:X4}+{n * attr.Stride:X4}] {GetAttributeName(attr.Class, attr.Val)}[{n}] = {result}");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr) {
         int result = GetDecValue(GetAttribute(attr));
         Log?.Invoke(this, $"Get[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = {GetHRValue(attr, result)}");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr, int n) {
         AttrData ad = attr.Clone();
         Debug.Assert(n < attr.Count);
         ad.Val += n * attr.Stride;
         int result = GetDecValue(GetAttribute(ad));
         Log?.Invoke(this, $"Get[{attr.Val:X4}+{n * attr.Stride:X4}] {GetAttributeName(attr.Class, attr.Val)}[{n}] = {result}");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return result;
      }

      // Get human readable value of the attribute
      public string GetHRAttribute<T>(T Attribute) where T : Enum {
         byte[] b = GetAttribute(Attribute);
         int n = GetDecValue(b);
         string result = n.ToString();
         AttrData attr = GetAttrData(Attribute);
         if (attr.Data.DropDown != fmtDD.None) {
            result = ToDropdownString(attr.Data, n);
         } else if (attr.Data.Fmt == DataFormats.UTF8 || attr.Data.Fmt == DataFormats.UTF8N) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         Log?.Invoke(this, $"Get[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = \"{result}\"");
         if (LogIOs)
            Log?.Invoke(this, " ");
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
         } else if (attr.Data.Fmt == DataFormats.UTF8 || attr.Data.Fmt == DataFormats.UTF8N) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         Log?.Invoke(this, $"Get[{attr.Val:X4}+{n * attr.Stride:X4}] {GetAttributeName(attr.Class, attr.Val)}[{n}] = \"{result}\"");
         if (LogIOs)
            Log?.Invoke(this, " ");
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
         } else if (attr.Data.Fmt == DataFormats.UTF8 || attr.Data.Fmt == DataFormats.UTF8N) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         Log?.Invoke(this, $"Get[{attr.Val:X4}+{n * attr.Stride:X4}] {GetAttributeName(attr.Class, attr.Val)}[{n}] = \"{result}\"");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return result;
      }

      #endregion

      #region Set Attribute Routines

      // Write to a specific address
      public bool SetAttribute(int addr, byte[] DataOut) {
         bool Successful = false;
         byte[] request = BuildModbusWrite(FunctionCode.WriteMultiple, addr, DataOut);
         if (Write(request)) {
            if (Read(out byte[] data, out int bytesRead)) {
               Successful = true;
            }
         }
         return Successful;
      }

      // Set one attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int val) where T : Enum {
         bool success = false;
         byte[] data;
         AttrData attr = GetAttrData(Attribute);
         data = FormatOutput(attr.Data, val);
         if (SetAttribute(attr.Val, data)) {
            if (attr.Class == ClassCode.IJP_operation && attr.Val == (int)ccIJP.Online_Offline) {
               comIsOn = val > 0;
            }
            success = true;
         }
         Log?.Invoke(this, $"Set[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = {GetHRValue(attr, val + attr.Data.Min)}");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return success;
      }

      // Set one attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, string s) where T : Enum {
         bool success = true;
         AttrData attr = GetAttrData(Attribute);
         if (!string.IsNullOrEmpty(s)) {
            //AutomaticReflect(AccessCode.Set);
            byte[] data = FormatOutput(attr.Data, s);
            success = SetAttribute(attr.Val, data);
         }
         Log?.Invoke(this, $"Set[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = \"{s}\"");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return success;
      }

      // Set one indexed attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int n, string s) where T : Enum {
         bool success = true;
         AttrData attr = GetAttrData(Attribute);
         if (!string.IsNullOrEmpty(s)) {
            //AutomaticReflect(AccessCode.Set);
            byte[] data = FormatOutput(attr.Data, s);
            success = SetAttribute(attr.Val + attr.Stride * n, data);
         }
         Log?.Invoke(this, $"Set[{attr.Val:X4}+{attr.Stride * n:X4}] {GetAttributeName(attr.Class, attr.Val)}[{n}] = \"{s}\"");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return success;
      }

      // Set one indexed attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int n, int val) where T : Enum {
         bool success = true;
         AttrData attr = GetAttrData(Attribute);
         //AutomaticReflect(AccessCode.Set);
         byte[] data = FormatOutput(attr.Data, val);
         success = SetAttribute(attr.Val + attr.Stride * n, data);
         Log?.Invoke(this, $"Set[{attr.Val:X4}+{attr.Stride * n:X4}] {GetAttributeName(attr.Class, attr.Val)}[{n + attr.Data.Min}] = {val}");
         if (LogIOs)
            Log?.Invoke(this, " ");
         return success;
      }

      // Set one indexed attribute based on the Data Property
      public bool SetAttribute<T>(T Attribute, int n, byte[] data) where T : Enum {
         bool success = true;
         AttrData attr = GetAttrData(Attribute);
         //AutomaticReflect(AccessCode.Set);
         success = SetAttribute(attr.Val + attr.Stride * n, data);
         Log?.Invoke(this, $"Set[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = byte[{data.Length}]");
                if (LogIOs)
  Log?.Invoke(this, " ");
         return success;
      }

      #endregion

      #region Attribute Routines

      // Class Codes to Attributes
      public Type[] ClassCodeAttributes = new Type[] {
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

      // Lookup for getting attributes associated with a Class/Function
      public Dictionary<ClassCode, int, AttrData> AttrDict;

      // Build the Attribute Dictionary
      void BuildAttributeDictionary() {
         if (AttrDict == null) {
            AttrDict = new Dictionary<ClassCode, int, AttrData>();
            for (int i = 0; i < ClassCodes.Length; i++) {
               int[] ClassAttr = (int[])ClassCodeAttributes[i].GetEnumValues();
               for (int j = 0; j < ClassAttr.Length; j++) {
                  AttrDict.Add(ClassCodes[i], (int)ClassAttr[j], GetAttrData(ClassCodes[i], (int)ClassAttr[j]));
               }
            }
         }
      }

      // Get AttrData with just the Enum
      public AttrData GetAttrData(Enum e) {
         return AttrDict[ClassCodes[Array.IndexOf(ClassCodeAttributes, e.GetType())], Convert.ToInt32(e)];
      }

      // Get attribute data for an arbitrary class/attribute
      public AttrData GetAttrData(ClassCode Class, int attr) {
         AttrData[] tab = M161.ClassCodeAttrData[Array.IndexOf(ClassCodes, Class)];
         AttrData result = Array.Find(tab, at => at.Val == attr);
         result.Class = Class;
         return result;
      }

      // Class Codes
      public ClassCode[] ClassCodes = (ClassCode[])Enum.GetValues(typeof(ClassCode));

      // get the human readable name
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

      #region ServiceRoutines

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
            Log?.Invoke(this, $"[{len}] << " + byte_to_string(input, len));
      }

      // Display the input byte array as hex
      private void DisplayOutput(byte[] output, int len = -1) {
         if (LogIOs)
            Log?.Invoke(this, $"[{len}] >> " + byte_to_string(output, len));
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
                     result += "{M}";
                     break;
                  case 0x61:
                     result += "{{M}";
                     break;
                  case 0x71:
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
                  case 0x63:
                  case 0x73:
                     result += "{h}";
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
         return result.Replace("}{", "").Replace("\x00", "");
      }

      // Format Output
      public byte[] FormatOutput(Prop prop, int n) {
         return ToBytes(n, prop.Len);
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
                     result = ToBytes(ToDropdownValue(prop, s), prop.Len);
                  }
               }
               break;
            case DataFormats.UTF8:
            case DataFormats.UTF8N:
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
                  if (c < 0x100) {
                     result[(i + 1) * width - 1] = (byte)c;
                  } else if ((c >> 8) == 0xF1) {
                     result[(i + 1) * width - 2] = (byte)(c >> 8);
                     result[(i + 1) * width - 1] = (byte)c;
                  } else {
                     result[i * width] = (byte)(c >> 8);
                     result[i * width + 1] = (byte)c;
                  }
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
            case DataFormats.N2Char:
               sa = s.Split(new char[] { ',' }, 2);
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint n)) {
                     result = Merge(ToBytes(n, 2), Encode.GetBytes(FromQuoted(sa[1]) + "\x00"));
                  }
               }
               break;
            case DataFormats.N1Char:
               sa = s.Split(new char[] { ',' }, 2);
               if (sa.Length == 2) {
                  if (uint.TryParse(sa[0].Trim(), out uint n)) {
                     result = Merge(ToBytes(n, 1), Encode.GetBytes(FromQuoted(sa[1]) + "\x00"));
                  }
               }
               break;
            case DataFormats.N1N2N1:
               sa = s.Split(',');
               if (sa.Length == 3) {
                  if (uint.TryParse(sa[0].Trim(), out uint n1) &&
                     uint.TryParse(sa[1].Trim(), out uint n2) &&
                     uint.TryParse(sa[2].Trim(), out uint n3)) {
                     result = Merge(ToBytes(n1, 1), ToBytes(n2, 2), ToBytes(n3, 1));
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
         // Calendar and count
         char[,] bc = new char[,]
         { {'C', '\uF25A'}, {'Y', '\uF250'}, {'M', '\uF251'}, {'D', '\uF252'}, {'h', '\uF253'},
           {'m', '\uF254'}, {'s', '\uF255'}, {'T', '\uF256'}, {'W', '\uF258'}, {'7', '\uF259'},
           {'E', '\uF25B'}, {'F', '\uF25C'} };

         // Half size characters
         string[,] hs = new string[,]
         { {"{ }", "\uF244"}, {"{\'}", "\uF240"}, {"{.}", "\uF241"}, {"{;}", "\uF245"},
           {"{:}", "\uF242"}, {"{!}", "\uF246"}, {"{,}", "\uF243"} };

         for (int i = 0; i < hs.GetLength(0); i++) {
            s2 = s2.Replace(hs[i, 0], hs[i, 1]);
         }

         for (int i = 0; i < s2.Length; i++) {
            int j;
            i = s2.IndexOf("{X/", i);
            if (i >= 0 && (j = s2.IndexOf("}", i + 3)) > i &&
               int.TryParse(s2.Substring(i + 3, j - i - 3), out int n)) {
               s2 = s2.Substring(0, i) + (char)('\uF140' + n) + s2.Substring(j + 1);
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
                     for (int j = 0; j < bc.GetLength(0) && !found; j++) {
                        if (bc[j, 0] == c) {
                           firstFound = Math.Min(firstFound, result.Length);
                           lastFound = Math.Max(lastFound, result.Length);
                           result += bc[j, 1];
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
         if (firstFound < s2.Length) {
            result = result.Substring(0, firstFound) + (char)(result[firstFound] + 0x10) + result.Substring(firstFound + 1);
         }
         if (lastFound >= 0) {
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

      // Convert Dropdown HR string to Dropdown value
      public int ToDropdownValue(Prop prop, string s) {
         int val;
         s = s.ToLower();
         val = Array.FindIndex(Data.DropDowns[(int)prop.DropDown], x => x.ToLower().Contains(s));
         if (val < 0) {
            val = Array.FindIndex(Data.DropDownsIJPLib[(int)prop.DropDown], x => x.ToLower().Contains(s));
         }
         if (val >= 0) {
            val += (int)prop.Min;
         }
         return val;
      }

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

      #endregion

   }
}
