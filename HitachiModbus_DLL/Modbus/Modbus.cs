using System;
using System.Collections.Generic;
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

      enum FunctionCode {
         WriteMultiple = 0x10,
         WriteSingle = 0x06,
         ReadHolding = 0x03,
         ReadInput = 0x04,
      }

      TcpClient client = null;
      NetworkStream stream = null;

      Encoding encode = Encoding.GetEncoding("ISO-8859-1");

      // Data Tables describing Hitachi Model 161
      static public Data M161 = new Data();

      #endregion

      #region Constructors and Destructors

      public Modbus() {
         BuildAttributeDictionary();
      }

      ~Modbus() {

      }

      #endregion

      #region Methods

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
            byte[] b = GetAttribute(ccIJP.Online_Offline);
            Log?.Invoke(this, $"Com status is {GetHumanReadableValue(ccIJP.Online_Offline, b)}");
            if (GetDecValue(b) == 0) {
               SetAttribute(ccIJP.Online_Offline, 1);
               b = GetAttribute(ccIJP.Online_Offline);
               Log?.Invoke(this, $"Com status is now {GetHumanReadableValue(ccIJP.Online_Offline, b)}");
               success = true;
            }
         }
         return success;
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

      private byte[] BuildModbusWrite(FunctionCode fc, int loc, byte[] data) {
         byte[] r = BuildModbusWrite(fc, loc, data.Length); // Get a buffer without data
         int n = r.Length - data.Length;                    // Calculate location where data will be placed
         for (int i = 0; i < data.Length; i++) {            // Step thru the input buffer
            r[n + i] = data[i];                             // move the data to the end of the buffer
         }
         return r;
      }

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
         int len = 10;
         byte[] request = BuildModbusRead(FunctionCode.ReadHolding, attr.Val, attr.Data.Len);
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
      public bool GetAttribute(int addr, int Len, out byte[] result) {
         bool success = false;
         byte[] data = null;
         int len = 10;
         byte[] request = BuildModbusRead(FunctionCode.ReadHolding, addr, Len);
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
         }
         return result;
      }

      // Get the contents of one attribute
      public byte[] GetAttribute<T>(T Attribute, int offset) where T : Enum {
         byte[] result;
         AttrData attr = GetAttrData(Attribute).Clone();
         attr.Val += offset;
         if (!GetAttribute(attr, out result)) {
            result = null;
         }
         return result;
      }

      // Get the contents of one attribute
      public byte[] GetAttribute<T>(T Attribute, int offset, int length) where T : Enum {
         byte[] result;
         AttrData attr = GetAttrData(Attribute).Clone();
         attr.Val += offset;
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
         Log?.Invoke(this, $"Addr[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = {result}");
         Log?.Invoke(this, " ");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute<T>(T Attribute, int offset) where T : Enum {
         int result = GetDecValue(GetAttribute(Attribute, offset));
         AttrData attr = GetAttrData(Attribute);
         Log?.Invoke(this, $"Addr[{attr.Val:X4}+{offset:X4}] {GetAttributeName(attr.Class, attr.Val)} = {result}");
         Log?.Invoke(this, " ");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr) {
         int result = GetDecValue(GetAttribute(attr));
         Log?.Invoke(this, $"Addr[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = {result}");
         Log?.Invoke(this, " ");
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr, int offset) {
         AttrData ad = attr.Clone();
         ad.Val += offset;
         int result = GetDecValue(GetAttribute(ad));
         Log?.Invoke(this, $"Addr[{attr.Val:X4}+{offset:X4}] {GetAttributeName(attr.Class, attr.Val)} = {result}");
         Log?.Invoke(this, " ");
         return result;
      }

      // Get human readable value of the attribute
      public string GetHRAttribute<T>(T Attribute) where T : Enum {
         byte[] b = GetAttribute(Attribute);
         long n = GetDecValue(b);
         string result = n.ToString();
         AttrData attr = GetAttrData(Attribute);
         if (attr.Data.DropDown != fmtDD.None) {
            string[] dd = GetDropDownNames((int)attr.Data.DropDown);
            n = n - attr.Data.Min;
            if (n >= 0 && n < dd.Length) {
               result = dd[n];
            }
         } else if (attr.Data.Fmt == DataFormats.UTF8 || attr.Data.Fmt == DataFormats.UTF8N) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         Log?.Invoke(this, $"Addr[{attr.Val:X4}] {GetAttributeName(attr.Class, attr.Val)} = \"{result}\"");
         Log?.Invoke(this, " ");
         return result;
      }

      // Get human readable value of the attribute
      public string GetHRAttribute<T>(T Attribute, int offset) where T : Enum {
         byte[] b = GetAttribute(Attribute, offset);
         long n = GetDecValue(b);
         string result = n.ToString();
         AttrData attr = GetAttrData(Attribute);
         if (attr.Data.DropDown != fmtDD.None) {
            string[] dd = GetDropDownNames((int)attr.Data.DropDown);
            n = n - attr.Data.Min;
            if (n >= 0 && n < dd.Length) {
               result = dd[n];
            }
         } else if (attr.Data.Fmt == DataFormats.UTF8 || attr.Data.Fmt == DataFormats.UTF8N) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         Log?.Invoke(this, $"Addr[{attr.Val:X4}+{offset:X4}] {GetAttributeName(attr.Class, attr.Val)} = \"{result}\"");
         Log?.Invoke(this, " ");
         return result;
      }

      // Get human readable value of the attribute
      public string GetHRAttribute<T>(T Attribute, int offset, int length) where T : Enum {
         byte[] b = GetAttribute(Attribute, offset, length);
         long n = GetDecValue(b);
         string result = n.ToString();
         AttrData attr = GetAttrData(Attribute);
         if (attr.Data.DropDown != fmtDD.None) {
            string[] dd = GetDropDownNames((int)attr.Data.DropDown);
            n = n - attr.Data.Min;
            if (n >= 0 && n < dd.Length) {
               result = dd[n];
            }
         } else if (attr.Data.Fmt == DataFormats.UTF8 || attr.Data.Fmt == DataFormats.UTF8N) {
            result = FormatText(b);
         } else if (attr.Data.Fmt == DataFormats.AttrText) {
            result = FormatAttrText(b);
         }
         Log?.Invoke(this, $"Addr[{attr.Val:X4}+{offset:X4}] {GetAttributeName(attr.Class, attr.Val)} = \"{result}\"");
         Log?.Invoke(this, " ");
         return result;
      }

      #endregion

      #region Set Attribute Routines

      // Write one attribute
      public bool SetAttribute(AttrData attr, byte[] DataOut) {
         bool Successful = false;
         byte[] request = BuildModbusWrite(FunctionCode.WriteMultiple, attr.Val, DataOut);
         if (Write(request)) {
            if (Read(out byte[] data, out int bytesRead)) {
               Successful = true;
            }
         }
         return Successful;
      }

      // Set one attribute based on the Set Property
      public bool SetAttribute<T>(T Attribute, int n) where T : Enum {
         byte[] data;
         AttrData attr = GetAttrData(Attribute);
         data = FormatOutput(attr.Set, n);
         return SetAttribute(attr, data);
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
      AttrData GetAttrData(ClassCode Class, int attr) {
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
            result = " "; //  $"Addr[{v.ToString("X4")}]";
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
      public string GetHumanReadableValue<T>(T Attribute, byte[] b) where T : Enum {
         long n = GetDecValue(b);
         string result = n.ToString();
         AttrData attr = GetAttrData(Attribute);
         if (attr.Data.DropDown != fmtDD.None) {
            string[] dd = GetDropDownNames((int)attr.Data.DropDown);
            n = n - attr.Data.Min;
            if (n >= 0 && n < dd.Length) {
               result = dd[n];
            }
         }
         return result;
      }

      private void DisplayInput(byte[] input, int len = -1) {
         string s = byte_to_string(input, len);
         Log?.Invoke(this, $"{len} data bytes arrived");
         Log?.Invoke(this, s);
      }

      private void DisplayOutput(byte[] output, int len = -1) {
         string s = byte_to_string(output, len);
         Log?.Invoke(this, $"{len} data bytes sent");
         Log?.Invoke(this, s);
      }

      private byte[] string_to_byte(string sIn) {
         string[] s = sIn.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
         byte[] b = new byte[s.Length];
         for (int i = 0; i < s.Length; i++) {
            if (!byte.TryParse(s[i], NumberStyles.HexNumber, null, out b[i])) {
               b[i] = 0;
            }
         }
         return b;
      }

      private string byte_to_string(byte[] b, int len = -1) {
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
         return result;
      }

      // Text is 4 bytes per character
      private string FormatAttrText(byte[] text) {
         string result = "";
         for (int i = 0; i < text.Length; i += 4) {
            if (text[i] == 0) {
               result += (char)text[i + 3];
            } else if (text[i] == 0xF1) {

            } else if (text[i] == 0xF2) {
               switch (text[i + 1]) {
                  case 0x50:
                  case 0x60:
                  case 0x70:
                     result += "{Y}";
                     break;
                  case 0x51:
                  case 0x61:
                  case 0x71:
                     result += "{M}";
                     break;
                  case 0x52:
                  case 0x62:
                  case 0x72:
                     result += "{D}";
                     break;
                  case 0x54:
                  case 0x64:
                  case 0x74:
                     result += "{h}";
                     break;
                  case 0x55:
                  case 0x65:
                  case 0x75:
                     result += "{m}";
                     break;
                  case 0x56:
                  case 0x66:
                  case 0x76:
                     result += "{s}";
                     break;
                  case 0x57:
                  case 0x67:
                  case 0x77:
                     result += "{T}";
                     break;
                  case 0x58:
                  case 0x68:
                  case 0x78:
                     result += "{W}";
                     break;
                  case 0x59:
                  case 0x69:
                  case 0x79:
                     result += "{7}";
                     break;
                  case 0x5B:
                     result += "{E}";
                     break;
                  case 0x6C:
                  case 0x7C:
                     result += "{F}";
                     break;
                  case 0x5A:
                  case 0x6A:
                  case 0x7A:
                     result += "{C}";
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
         return result.Replace("}{", "");
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

      #endregion

   }
}
