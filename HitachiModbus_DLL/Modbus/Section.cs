using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus_DLL {
   #region Section Class

   public class Section<T> where T : Enum {

      #region Data Declarations

      const int MaxWordSize = 120;                          // Maximum number of 2-byte items to read/write
      const int MaxByteSize = MaxWordSize * 2;              // Maximum number of byte items to read/write

      public byte[] b;
      Modbus MB;              // I/O Module
      T attr;                 // Attribute for the start 
      AttrData BaseAttr;      // 
      int Index;              // Index of first repeated structure in section
      int Len;                // Length of area to allocate
      bool Load;              // Save to cut down on trace I/O

      #endregion

      #region Constructor, Destructor, and Service Routines

      // Let Section find the attrData and calculate the span
      public Section(Modbus MB, T startAttr, T endAttr, int index, bool load)
         : this(MB, MB.GetAttrData(startAttr), index, GetSpan(MB, startAttr, endAttr), load) {
      }

      // Let Section find the attrData 
      public Section(Modbus MB, T attr, int index, int Len, bool load)
         : this(MB, MB.GetAttrData(attr), index, Len, load) {
      }

      // attrData, index (stride count), and length (in words) are provided by the caller
      public Section(Modbus MB, AttrData BaseAttr, int index, int Len, bool load) {
         this.MB = MB;                                      // Save Modbus I/O object
         this.attr = (T)(object)BaseAttr.Val;               // Reverse lookup AttrData => Attr
         this.Index = index;                                // For repeated structures
         this.BaseAttr = BaseAttr;                          // Need the stride for indexing
         this.Len = Len;                                    // Save for section write message
         this.Load = load;                                  // False => already already cleared to 0's
         string idx = string.Empty;
         if (BaseAttr.Count > 1) {
            idx = $"[{index}]";
         }
         MB.LogIt($"\nAllocating Section {MB.GetAttributeName(BaseAttr.Class, BaseAttr.Val)}{idx} Length = {Len},  PreLoad = {load}");
         if (load) {                                        // Load if previous contents needed
            if (Len <= MaxWordSize) {                       // If only one read needed, read it directly
               b = MB.GetAttributeBlock(BaseAttr, index, Len);  // 
            } else {                                        // Otherwise, read it in block size chunks
               b = new byte[Len * 2];                       // Big enough for all reads
               int wordsRead = 0;                           // Words read so far
               int wordsToRead = Len;                       // Words needed
               while (wordsToRead > 0) {                    // Until all have been read
                  byte[] t = MB.GetAttributeBlock(BaseAttr, index, Math.Min(wordsToRead, MaxWordSize), wordsRead);
                  Buffer.BlockCopy(t, 0, b, wordsRead * 2, t.Length); // save away the bytes
                  wordsRead += MaxWordSize;                 // Update words read
                  wordsToRead -= MaxWordSize;               // Update words remaining (OK if it goes negative)
               }
            }
         } else {
            b = new byte[Len * 2];                          // Empty array big enough for all data
         }
      }

      // Get the length of the area in words (2 bytes per word)
      public static int GetSpan(Modbus MB, T start, T end) {
         AttrData startAttr = MB.GetAttrData(start);            // Starting memory address (Words)
         AttrData endAttr = MB.GetAttrData(end);                // Ending memory address
         Debug.Assert(startAttr.Val <= endAttr.Val);            // Make sure they are in the correct order
         return endAttr.Val - startAttr.Val + endAttr.Data.Len; // Be sure to include the last element
      }

      #endregion

      #region Read interface

      // Get an attribute and convert it to a string for SOP output
      public string Get(T item, int resultLen) {
         return Get(item, 0, resultLen);                    // For non-replicated items, Index is not needed
      }

      // Get an attribute and convert it to a string for SOP output
      public string Get(T item, int index, int resultLen) {
         string result = string.Empty;
         AttrData getAttr = MB.GetAttrData(item);           // Get attributes of desired item
         int n = (getAttr.Val - BaseAttr.Val) * 2 +         // Get offset in byte array (2 bytes per word)
            (index - this.Index) * BaseAttr.Stride * 2;     // Relative to the Index that was loaded
         int len = getAttr.Data.Len;                        // Get length
         switch (getAttr.Data.Fmt) {                        // Format the data as needed
            case DataFormats.None:
               break;
            case DataFormats.SDecimal:
            case DataFormats.Decimal:
               len += len;                                  // Length is in words so double it
               int res = 0;
               for (int i = 0; i < len; i++) {
                  res = (res << 8) + b[n + i];              // Words are stored in Big Endial format
               }
               result = res.ToString($"D{resultLen}");      // To string with leading zeros
               break;
            case DataFormats.UTF8:
               char[] c = new char[len];
               Buffer.BlockCopy(b, n + 1, c, 0, len * 2 - 1); // Characters are stored as little endian.
               result = new string(c);
               break;
            case DataFormats.Date:                          // Probably on a TODO list
            case DataFormats.Bytes:
            case DataFormats.AttrText:
            default:
               break;
         }
         return result;
      }

      // Get a UTF-8 character string
      public string GetString(T item, int count) {
         AttrData getAttr = MB.GetAttrData(item);           // Get attributes of desired item
         int n = (getAttr.Val - BaseAttr.Val) * 2;          // Get offset in byte array (2 bytes per word)
         char[] c = new char[count];
         for (int i = 0; i < count; i++) {
            c[i] = (char)((b[n + 1] << 8) + b[n]);          // UTF-8 characters are in Little Endian format
            n += 2;
         }
         return new string(c);                              // Characters to string
      }

      // Get the attribute as an integer
      public int GetDecAttribute(T item, int index = 0) {
         AttrData getAttr = MB.GetAttrData(item);           // Get attributes of desired item
         int n = (getAttr.Val - BaseAttr.Val) * 2 +
            (index - this.Index) * getAttr.Stride * 2;      // Get offset in byte array (2 bytes per word)
         int len = getAttr.Data.Len;                        // Get length
         int res = 0;
         switch (getAttr.Data.Fmt) {                        // Format the data as needed
            case DataFormats.Decimal:                       // Caller has to know that it is a decimal
               len += len;                                  // Length is in words so double it
               for (int i = 0; i < len; i++) {
                  res = (res << 8) + b[n + i];
               }
               break;
            default:
               // Need msg here
               break;
         }
         return res;
      }

      // Get a block of bytes from the section
      public byte[] GetBytes(int offset, int len) {
         byte[] response = new byte[len];
         Buffer.BlockCopy(b, offset, response, 0, len);     // Is Block Copy always the best
         return response;
      }

      // Get a block of words from a section
      public int[] GetWords(int start, int len = -1) {      // start in bytes, len in words
         if (len == -1) {                                   // If length not specified, return all
            len = b.Length / 2 - start;
         }
         int[] response = new int[len];
         for (int i = 0; i < len; i++) {
            int n = (start + i) * 2;
            response[i] = (b[n] << 8) + b[n + 1];           // Values are stored Big Endian
         }
         return response;
      }

      // Get the message text (Only place attributed characters are used)
      public byte[] GetAttributedChrs(int start, int len) {
         byte[] response = new byte[len * 4];                  // start and length in 4-byte chunks (2 words)
         Buffer.BlockCopy(b, start * 4, response, 0, len * 4); // Faster than a loop
         return response;
      }

      // Get user patterns as strings (ISO-8859-1 format) -- Something to work out with cijConnect
      public string[] GetUserPatterns(int count) {
         string[] result = new string[count];               // Patterns are returned as strings
         int n = b.Length / count;                          // Number of bytes per string
         char[] c = new char[n];                            // A place to stage the output
         int k = 0;
         for (int i = 0; i < count; i++) {                  // Repeat for each logo
            for (int j = 0; j < n; j++) {
               c[j] = (char)b[k++];
            }
            result[i] = new string(c);
         }
         return result;
      }

      #endregion

      #region Write interface

      // Write Section into proper area of (possibly) a replicated structure
      public void WriteSection(bool stack = false) {
         bool unstack = false;
         int bytesWritten = 0;
         int bytesToWrite = b.Length;
         if (stack && bytesToWrite > MaxByteSize) {
            MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);              // These two must be done together
            unstack = true;
         }
         while (bytesToWrite > 0) {                         // Base and offset stay constant, index into byte array advances
            MB.SetBlockAttribute(BaseAttr, BaseAttr.Stride * this.Index, b, bytesWritten, Math.Min(bytesToWrite, MaxByteSize));
            bytesWritten += MaxByteSize;
            bytesToWrite -= MaxByteSize;
         }
         if (unstack) {
            MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);              // These two must be done together
         }
         string idx = string.Empty;
         if (BaseAttr.Count > 1) {
            idx = $"[{Index}]";
         }
         MB.LogIt($"Writing Section {MB.GetAttributeName(BaseAttr.Class, BaseAttr.Val)}{idx} Length = {Len},  Stack = {stack}\n");
      }

      // Copy a user pattern into the section of user patterns (here the user pattern is provided as bytes)
      public void SetUserPattern(byte[] data, int index, int len = -1) {
         if (len == -1) {
            len = data.Length;
         }
         int loc = (index - this.Index) * BaseAttr.Stride * 2;        // Location of the user pattern within the section
         Buffer.BlockCopy(data, 0, b, loc, len);                      // Fast move?
      }

      // Move a block of words into a section
      public void SetWords(int[] data, int offset) {
         int n = offset * 2;
         for (int i = 0; i < data.Length; i++) {
            b[n++] = (byte)(data[i] >> 8);                  // Words are stored in Big Endian format
            b[n++] = (byte)(data[i]);
         }
      }

      // Set attributed characters into section
      public void SetAttrChrs(string s, int offset) {
         byte[] data = MB.FormatOutput(BaseAttr.Data, s);
         Buffer.BlockCopy(data, 0, b, offset * 2, data.Length);              // Fast move?
         MB.LogIt($"BSet[{MB.GetNozzle(BaseAttr)}{BaseAttr.Val:X4}+{offset:X4}] {MB.GetAttributeName(BaseAttr.Class, BaseAttr.Val)} = " +
           $"{MB.FormatText(data)}");
      }

      // Set one attribute based on the Data Property
      public void SetAttribute(T Attribute, int val) {
         AttrData attr = MB.GetAttrData(Attribute);
         SetData(attr, 0, MB.FormatOutput(attr.Data, val)); // Format output formats the value into bytes
      }

      // Set one attribute based on the Data Property
      public void SetAttribute(T Attribute, string s) {
         if (!string.IsNullOrEmpty(s)) {
            AttrData attr = MB.GetAttrData(Attribute);
            SetData(attr, 0, MB.FormatOutput(attr.Data, s)); // Format output formats the value into bytes
         }
      }

      //Set one indexed attribute based on the Data Property
      public void SetAttribute<D>(T Attribute, int index, D val) {
         AttrData attr = MB.GetAttrData(Attribute);                   // Description of the data
         SetAttribute(attr, index, val);                              // Pass it on
      }

      //Set one indexed attribute based on the Data Property
      public void SetAttribute<D>(AttrData attr, int index, D val) {
         byte[] data = null;                                          // The data to be tucked away
         if (typeof(D) == typeof(byte[])) {                           // Already a byte array?
            data = (byte[])(object)val;                               // Use the array as the data (strange cast)
         } else if (MB.IsNumeric(typeof(D))) {                        // Numerics can do a simple convert
            data = MB.FormatOutput(attr.Data, Convert.ToInt32(val));  // Get the correct length as bytes
         } else {                                                     // Others require a conversion from Dropdowns
            string s = Convert.ToString(val);                         // May or may not be required but other conversion uses it
            if (!string.IsNullOrEmpty(s)) {
               data = MB.FormatOutput(attr.Data, s);                  // Get the correct length as bytes
            }
         }
         SetData(attr, (index - this.Index) * attr.Stride, data);     // Set the data into the section
      }

      // Set bytes into section
      private void SetData(AttrData attr, int offset, byte[] data) {
         string index = string.Empty;
         if (!Load && data.Count(x => x != 0) == 0) {
            return;
         }
         if (attr.Count > 1) {
            index = $"+{offset:X4}";                  // Account for non-zero origin 
         }
         if (data != null && data.Length > 0) {
            int n = (attr.Val - BaseAttr.Val + attr.Data.Len + offset) * 2;  // Get offset in byte array (2 bytes per word)
            int len = data.Length;                                    // Get length in bytes (2 bytes per word)
            for (int i = 0; i < len; i++) {                           // Tuck away the data
               b[--n] = data[data.GetUpperBound(0) - i];              // in reverse order (required for odd number of bytes)
            }
            if (attr.Data.DropDown == fmtDD.None) {
               if (attr.Data.Fmt == DataFormats.Decimal) {
                  MB.LogIt($"BSet[{MB.GetNozzle(attr)}{attr.Val:X4}{index}] {MB.GetAttributeName(attr.Class, attr.Val)} = " +
                     $"{MB.GetDecValue(data)}");
               } else {
                  MB.LogIt($"BSet[{MB.GetNozzle(attr)}{attr.Val:X4}{index}] {MB.GetAttributeName(attr.Class, attr.Val)} = " +
                     $"{MB.byte_to_string(data)}");
               }
            } else {
               MB.LogIt($"BSet[{MB.GetNozzle(attr)}{attr.Val:X4}{index}] {MB.GetAttributeName(attr.Class, attr.Val)} = " +
                  $"\"{MB.GetHRValue(attr, data[data.Length - 1])}\"");
            }
         }
      }

      #endregion

   }

   #endregion
}
