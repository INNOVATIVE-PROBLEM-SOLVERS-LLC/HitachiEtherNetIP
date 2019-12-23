using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ModBus161 {
   public partial class UI161 : Form {

      #region Data Declarations

      TcpClient client = null;
      NetworkStream stream = null;
      int IPPort;

      Encoding encode = Encoding.GetEncoding("ISO-8859-1");

      public byte[] Nodata = new byte[0];
      public byte[] Null = new byte[] { 0 };
      public byte[] DataZero = new byte[] { 0 };
      public byte[] DataOne = new byte[] { 1 };
      public byte[] DataTwo = new byte[] { 2 };

      public bool UseIJPLibNames = true;

      // Data Tables describing Hitachi Model 161
      static public DataII M161 = new DataII();

      #endregion

      #region Constructors an destructors

      public UI161() {
         InitializeComponent();
         BuildAttributeDictionary();
      }

      enum FunctionCode {
         WriteMultiple = 0x10,
         WriteSingle = 0x06,
         ReadHolding = 0x03,
         ReadInput = 0x04,
      }

      #endregion

      #region Form Level Events

      #endregion

      #region Form Control Events

      // Connect to printer and turn COM on
      private void cmdConnect_Click(object sender, EventArgs e) {
         if (IPAddress.TryParse(txtIPAddress.Text, out IPAddress ipAddress) &&
            int.TryParse(txtIPPort.Text, out IPPort)) {
            client = new TcpClient(txtIPAddress.Text, IPPort);
            stream = client.GetStream();
            Log("Connection Accepted");
            byte[] b = GetAttribute(ccIJP.Online_Offline);
            Log($"Com status is {GetHumanReadableValue(ccIJP.Online_Offline, b)}");
            if (GetDecValue(b) == 0) {
               SetAttribute(ccIJP.Online_Offline, 1);
               b = GetAttribute(ccIJP.Online_Offline);
               Log($"Com status is now {GetHumanReadableValue(ccIJP.Online_Offline, b)}");
            }
         }
      }

      // Turn com on
      private void comOn_Click(object sender, EventArgs e) {
         SetAttribute(ccIJP.Online_Offline, 1);
      }

      // Turn com off
      private void cmdComOff_Click(object sender, EventArgs e) {
         SetAttribute(ccIJP.Online_Offline, 0);
      }

      // Read data from the printer
      private void cmdReadData_Click(object sender, EventArgs e) {
         if (int.TryParse(txtDataAddress.Text, NumberStyles.HexNumber, null, out int addr)
            && int.TryParse(txtDataLength.Text, out int len)) {
            GetAttribute(addr, len, out byte[] data);
         }
      }

      // Send data to the printer
      private void cmdWriteData_Click(object sender, EventArgs e) {

      }

      // Retrieve message from printer and convert to XML
      private void cmdRetrieve_Click(object sender, EventArgs e) {
         RetrieveXML retrieve = new RetrieveXML(this);
         string xml = retrieve.Retrieve();
         try {
            // Can be called with a Filename or XML text
            int xmlStart = xml.IndexOf("<Label");
            if (xmlStart == -1) {
               xml = File.ReadAllText(xml);
               xmlStart = xml.IndexOf("<Label");
            }
            // No label found, exit
            if (xmlStart == -1) {
               return;
            }
            int xmlEnd = xml.IndexOf("</Label>", xmlStart + 7);
            if (xmlEnd > 0) {
               xml = xml.Substring(xmlStart, xmlEnd - xmlStart + 8);
               XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = true };
               xmlDoc.LoadXml(xml);
               xml = ToIndentedString(xml);
               xmlStart = xml.IndexOf("<Label");
               if (xmlStart > 0) {
                  xml = xml.Substring(xmlStart);
                  txtIndentedView.Text = xml;

                  tvXML.Nodes.Clear();
                  tvXML.Nodes.Add(new TreeNode(xmlDoc.DocumentElement.Name));
                  TreeNode tNode = new TreeNode();
                  tNode = tvXML.Nodes[0];

                  AddNode(xmlDoc.DocumentElement, tNode);
                  tvXML.ExpandAll();

               }
            }
         } catch {

         }
      }

      // Exit the program
      private void cmdExit_Click(object sender, EventArgs e) {
         this.Close();
      }

      #endregion

      #region Modbus command builders

      private byte[] BuildCom(bool on) {
         byte[] r = BuildModbusWrite(FunctionCode.WriteMultiple, 0x2490, 2);
         r[14] = on ? (byte)1 : (byte)0;
         return r;
      }

      private byte[] BuildText(int position, string s) {
         byte[] r = BuildModbusWrite(FunctionCode.WriteMultiple, 0x80 + position * 4, 4 * s.Length);
         byte[] b = encode.GetBytes(s);              // Encode input data
         int n = 13;                                 // Start of data area
         for (int i = 0; i < s.Length; i++) {
            r[n] = 0;                                // High byte of attribute
            r[n + 1] = 0;                            // Low byte of attribute
            r[n + 2] = 0;                            // High byte of character
            r[n + 3] = (byte)b[i];                   // Low byte of character
            n += 4;                                  // Move to next character position
         }
         return r;
      }

      private byte[] BuildCharacterCount(int loc, string s) {
         byte[] r = BuildModbusWrite(FunctionCode.WriteMultiple, 0x20 + loc - 1, 2);
         r[14] = (byte)s.Length;
         return r;
      }

      private byte[] BuildStartStop(bool on) {
         byte[] r = BuildModbusWrite(FunctionCode.WriteMultiple, 0, 2);
         r[14] = on ? (byte)1 : (byte)2;
         return r;
      }

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

      #region Ethernet interface

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
            } catch (IOException e) {
               Log(e.Message);
            }
         }
         if (!successful) {
            Log("Read Failed.");
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
            } catch (IOException e) {
               Log(e.Message);
            }
         }
         if (!successful) {
            Log("Write Failed. Connection Closed!");
         }
         return successful;
      }

      #endregion

      #region Service Routines

      // Convert an XML Document into an indented text string
      private string ToIndentedString(string unformattedXml) {
         string result;
         XmlReaderSettings readeroptions = new XmlReaderSettings { IgnoreWhitespace = true };
         XmlReader reader = XmlReader.Create(new StringReader(unformattedXml), readeroptions);
         StringBuilder sb = new StringBuilder();
         XmlWriterSettings xmlSettingsWithIndentation = new XmlWriterSettings { Indent = true };
         using (XmlWriter writer = XmlWriter.Create(sb, xmlSettingsWithIndentation)) {
            writer.WriteNode(reader, true);
         }
         result = sb.ToString();
         return result;
      }

      // Add a node to the tree view
      private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode) {
         if (inXmlNode is XmlWhitespace)
            return;
         XmlNode xNode;
         XmlNodeList nodeList;
         if (inXmlNode.HasChildNodes) {
            inTreeNode.Text = GetNameAttr(inXmlNode);
            nodeList = inXmlNode.ChildNodes;
            int j = 0;
            for (int i = 0; i < nodeList.Count; i++) {
               xNode = inXmlNode.ChildNodes[i];
               if (xNode is XmlWhitespace)
                  continue;
               if (xNode.Name == "#text") {
                  inTreeNode.Text = inXmlNode.OuterXml.Trim();
               } else {
                  if (!(xNode is XmlWhitespace)) {
                     inTreeNode.Nodes.Add(new TreeNode(GetNameAttr(xNode)));
                     AddNode(xNode, inTreeNode.Nodes[j]);
                  }
               }
               j++;
            }
         } else {
            inTreeNode.Text = inXmlNode.OuterXml.Trim();
         }
      }

      // Get the attributes associated with a node
      private string GetNameAttr(XmlNode n) {
         string result = n.Name;
         if (n.Attributes != null && n.Attributes.Count > 0) {
            foreach (XmlAttribute attribute in n.Attributes) {
               result += $" {attribute.Name}=\"{attribute.Value}\"";
            }
         }
         return result;
      }

      private void DisplayInput(byte[] input, int len = -1) {
         string s = byte_to_string(input, len);
         Log($"{len} data bytes arrived");
         Log(s);
      }

      private void DisplayOutput(byte[] output, int len = -1) {
         string s = byte_to_string(output, len);
         Log($"{len} data bytes sent");
         Log(s);
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

      public void Log(string msg) {
         lstMessages.Items.Add(msg);
         lstMessages.SelectedIndex = lstMessages.Items.Count - 1;
         lstMessages.Update();
      }

      #endregion

      #region Attribute Routines

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

      // Get attribute data for an arbitrary class/attribute
      AttrData GetAttrData(ClassCode Class, int attr) {
         AttrData[] tab = M161.ClassCodeAttrData[Array.IndexOf(ClassCodes, Class)];
         AttrData result = Array.Find(tab, at => at.Val == attr);
         result.Class = Class;
         return result;
      }

      // Class Codes
      public ClassCode[] ClassCodes = (ClassCode[])Enum.GetValues(typeof(ClassCode));

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
      public byte[] GetAttribute(AttrData attr) {
         byte[] result;
         if (!GetAttribute(attr, out result)) {
            result = null;
         }
         return result;
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute<T>(T Attribute) where T : Enum {
         return GetDecValue(GetAttribute(Attribute));
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute<T>(T Attribute, int offset) where T : Enum {
         return GetDecValue(GetAttribute(Attribute, offset));
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr) {
         return GetDecValue(GetAttribute(attr));
      }

      // Get the decimal value of the attribute
      public int GetDecAttribute(AttrData attr, int offset) {
         AttrData ad = attr.Clone();
         ad.Val += offset;
         return GetDecValue(GetAttribute(ad));
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
         }
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
         }
         return result;
      }

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

      // Get AttrData with just the Enum
      public AttrData GetAttrData(Enum e) {
         return AttrDict[ClassCodes[Array.IndexOf(ClassCodeAttributes, e.GetType())], Convert.ToInt32(e)];
      }

      // Convert Modbus printstring to ASCII string
      private string FromQuoted(byte[] b) {

         return "";
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

      // Get list of names for conversion to human readable
      public string[] GetDropDownNames(int n) {
         if (UseIJPLibNames) {
            return DataII.DropDownsIJPLib[n];
         } else {
            return DataII.DropDowns[n];
         }
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

      #endregion

   }

   #region State Object

   internal class StateObject {
      // Client socket. 
      internal Socket workSocket = null;
      // Size of receive buffer. 
      internal const int BufferSize = 256;
      // Receive buffer. 
      internal byte[] buffer = new byte[BufferSize];
      // Received data string. 
      public StringBuilder sb = new StringBuilder();
   }

   #endregion

}
