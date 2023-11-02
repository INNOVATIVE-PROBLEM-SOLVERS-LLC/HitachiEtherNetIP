using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serialization;

namespace Modbus_DLL {
   public partial class AsyncIO {

      #region Events

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(object sender, string msg);

      // I/O Complete
      public event CompleteHandler Complete;
      public delegate void CompleteHandler(object sender, AsyncComplete status);

      #endregion

      #region Data Declarations

      Form parent;

      public enum TaskType {
         Connect,
         Disconnect,
         Send,
         SendLabel,
         RetrieveMsg,
         WriteData,
         ReadData,
         RecallMessage,
         AddMessage,
         Substitutions,
         DeleteMessage,
         IssueccIJP,
         GetStatus,
         GetMessages,
         GetErrors,
         AckOnly,
         Specification,
         WritePattern,
         TimedDelay,
         Retrieve,
         WriteSelectedItems,
         WriteDateOffset,
         WritePrintDelay,
         WriteAllItems,
         Idle,
         Exit,
      }

      // Do the work in the background
      Thread t;

      // Use Blocking Collection to avoid spin waits
      public BlockingCollection<ModbusPkt> AsyncIOTasks = new BlockingCollection<ModbusPkt>();
      ModbusPkt pkt;

      // Printer to use for I/O
      Modbus MB = null;

      public bool LogIO { get; set; }

      // Remote Operations
      private enum RemoteOps {
         Start = 0,
         Stop = 1,
         Ready = 2,
         StandBy = 3,
         ClearFault = 4,
      }

      #endregion

      #region Constructors and Destructors

      public AsyncIO(Form parent, Modbus MB) {
         // 
         this.parent = parent;
         this.MB = MB;
         t = new Thread(processTasks);
         t.Start();
      }

      #endregion

      #region Task Processing

      private void processTasks() {
         bool done = false;
         // Just one big loop
         while (!done) {
            try {
               // Wait for the next request
               pkt = AsyncIOTasks.Take();
               switch (pkt.Type) {
                  case TaskType.Connect:
                     Connect(pkt);
                     break;
                  case TaskType.Disconnect:
                     Disconnect(pkt);
                     break;
                  case TaskType.Send:
                     Send(pkt);
                     break;
                  case TaskType.SendLabel:
                     SendLabel(pkt);
                     break;
                  case TaskType.RetrieveMsg:
                     RetrieveMsg(pkt);
                     break;
                  case TaskType.WriteData:
                     WriteData(pkt);
                     break;
                  case TaskType.ReadData:
                     ReadData(pkt);
                     break;
                  case TaskType.RecallMessage:
                     RecallMessage(pkt);
                     break;
                  case TaskType.Substitutions:
                     DoSubstitutions(pkt);
                     break;
                  case TaskType.AddMessage:
                     AddMessage(pkt);
                     break;
                  case TaskType.DeleteMessage:
                     DeleteMessage(pkt);
                     break;
                  case TaskType.IssueccIJP:
                     IssueccIJP(pkt);
                     break;
                  case TaskType.GetStatus:
                     GetStatus(pkt);
                     break;
                  case TaskType.GetMessages:
                     GetMessages(pkt);
                     break;
                  case TaskType.GetErrors:
                     GetErrors(pkt);
                     break;
                  case TaskType.AckOnly:
                     AckOnly(pkt);
                     break;
                  case TaskType.Specification:
                     Specification(pkt);
                     break;
                  case TaskType.WritePattern:
                     WritePattern(pkt);
                     break;
                  case TaskType.TimedDelay:
                     Thread.Sleep(pkt.TimeDelay);
                     AckOnly(pkt);
                     break;
                  case TaskType.Idle:
                     AckOnly(pkt);
                     break;
                  case TaskType.Retrieve:
                     Retrieve(pkt);
                     break;
                  case TaskType.WriteSelectedItems:
                     WriteSelectedItems(pkt);
                     break;
                  case TaskType.WriteDateOffset:
                     WriteDateOffset(pkt);
                     break;
                  case TaskType.WritePrintDelay:
                     WritePrintDelay(pkt);
                     break;
                  case TaskType.WriteAllItems:
                     WriteAllItems(pkt);
                     break;
                  case TaskType.Exit:
                     done = true;
                     break;
                  default:
                     return;
               }
            } catch {
               AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = false };
               parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
            }
         }
      }

      private void WriteAllItems(ModbusPkt pkt) {
         bool success = false;
         SendRetrieveXML send = new SendRetrieveXML(MB);
         send.Log += Modbus_Log;
         try {
            success = send.WriteAllItems(pkt.Data2);
         } finally {
            AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
            parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
            send.Log -= Modbus_Log;
            send = null;
         }
      }

      private void WriteSelectedItems(ModbusPkt pkt) {
         bool success = false;
         SendRetrieveXML send = new SendRetrieveXML(MB);
         send.Log += Modbus_Log;
         try {
            success = send.WriteSelectedItems(pkt.Item, pkt.Data);
         } finally {
            AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
            parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
            send.Log -= Modbus_Log;
            send = null;
         }
      }

      private void WriteDateOffset(ModbusPkt pkt) {
         bool success = false;
         SendRetrieveXML send = new SendRetrieveXML(MB);
         send.Log += Modbus_Log;
         try {
            success = send.WriteDateOffset(pkt.Data2);
         } finally {
            AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
            parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
            send.Log -= Modbus_Log;
            send = null;
         }
      }

      private void WritePrintDelay(ModbusPkt pkt) {
         bool success = false;
         SendRetrieveXML send = new SendRetrieveXML(MB);
         send.Log += Modbus_Log;
         try {
            success = send.WritePrintDelay(pkt.Data);
         } finally {
            AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
            parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
            send.Log -= Modbus_Log;
            send = null;
         }
      }

      private void WritePattern(ModbusPkt pkt) {
         bool success = MB.SendFixedLogo(pkt.DotMatrix, pkt.Addr, pkt.DataA);
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Specification(ModbusPkt pkt) {
         bool success = false;
         switch (pkt.PrinterSpec) {
            case ccPS.Character_Height:
            case ccPS.Character_Width:
            case ccPS.Repeat_Interval:
               int st = MB.GetDecAttribute(ccUS.Operation_Status);
               if (st >= 0x32) { // 0x30 and 0x31 indicate already in standby
                  success = MB.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.StandBy);
                  success = MB.SetAttribute(pkt.PrinterSpec, pkt.Data);
                  success = MB.SetAttribute(ccIJP.Remote_operation, (int)RemoteOps.Ready);
               } else {
                  success = MB.SetAttribute(pkt.PrinterSpec, pkt.Data);
               }
               break;
            default:
               break;
         }
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void AckOnly(ModbusPkt pkt) {
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = true };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Idle(ModbusPkt pkt) {
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = true, Resp1 = pkt.Marker };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Connect(ModbusPkt pkt) {
         bool success = MB.Connect(pkt.IpAddress, pkt.IpPort);
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Disconnect(ModbusPkt pkt) {
         MB.Disconnect();
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = true };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Send(ModbusPkt pkt) {
         SendRetrieveXML send = new SendRetrieveXML(MB);
         send.Log += Modbus_Log;
         try {
            send.SendXML(pkt.Data);
         } finally {
            AsyncComplete ac = new AsyncComplete(MB, pkt);
            parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
            send.Log -= Modbus_Log;
            send = null;
         }
      }

      private void SendLabel(ModbusPkt pkt) {
         SendRetrieveXML send = new SendRetrieveXML(MB);
         send.Log += Modbus_Log;
         try {
            send.SendXML(pkt.Label);
         } finally {
            AsyncComplete ac = new AsyncComplete(MB, pkt);
            parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
            send.Log -= Modbus_Log;
            send = null;
         }
      }

      private void RetrieveMsg(ModbusPkt pkt) {
         string msgXML = string.Empty;
         SendRetrieveXML retrieveMsg = new SendRetrieveXML(MB);
         retrieveMsg.Log += Modbus_Log;
         try {
            msgXML = retrieveMsg.Retrieve();
         } finally {
            AsyncComplete ac = new AsyncComplete(MB, pkt) { Resp1 = msgXML };
            parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
            retrieveMsg.Log -= Modbus_Log;
            retrieveMsg = null;
         }
      }

      private void WriteData(ModbusPkt pkt) {
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         bool success = MB.SetAttribute(pkt.DevAddr, pkt.Addr, pkt.DataA);
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void ReadData(ModbusPkt pkt) {
         MB.GetAttribute(pkt.fc, pkt.DevAddr, pkt.Addr, pkt.Len, out byte[] data);
         AsyncComplete ac = new AsyncComplete(MB, pkt) { DataA = data };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void RecallMessage(ModbusPkt pkt) {
         bool success = MB.SetAttribute(ccPDR.Recall_Message, pkt.Value);
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void AddMessage(ModbusPkt pkt) {
         bool success = MB.DeleteMessage(pkt.Value);
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         success &= MB.SetAttribute(ccPDR.MessageName, pkt.Data);
         success &= MB.SetAttribute(ccPDR.Message_Number, pkt.Value);
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void DeleteMessage(ModbusPkt pkt) {
         bool success = MB.DeleteMessage(pkt.Value);
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void GetMessages(ModbusPkt pkt) {
         List<string> msgs = new List<string>();
         string[] s = new string[3];

         // Get maximum number of messages allowed and read them ad a block (16 registrations per word)
         int msgCount = MB.GetDecAttribute(ccUI.Maximum_Registered_Message_Count);
         Section<ccMM> mm = new Section<ccMM>(MB, ccMM.Registration, 0, (msgCount + 15) / 16, true);
         int[] regs = mm.GetWords(0);


         for (int i = 0; i < regs.Length; i++) {
            if (regs[i] != 0) {                                                     // Any on this block?
               for (int j = 0; j < 16; j++) {
                  int n = 15 - j;
                  if ((regs[i] & (1 << n)) > 0) {
                     n = i * 16 + j;                                                // 1-origin
                     MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
                     MB.SetAttribute(ccIDX.Message_Number, n + 1);         // Load the message into input registers
                     MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
                     Section<ccMM> msg = new Section<ccMM>(MB, ccMM.Message_Number, 0, 14, true);
                     s[0] = msg.Get(ccMM.Group_Number, 2);
                     s[1] = msg.Get(ccMM.Message_Number, 4);
                     s[2] = msg.Get(ccMM.Message_Name, 12);
                     msgs.Add(string.Join(",", s));
                  }
               }
            }
         }
         AsyncComplete ac = new AsyncComplete(MB, pkt) { MultiLine = msgs.ToArray() };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void GetErrors(ModbusPkt pkt) {
         int errCount = MB.GetDecAttribute(ccAH.Message_Count);
         string[] errs = new string[errCount];
         AttrData attr = MB.GetAttrData(ccAH.Year);
         Section<ccAH> hist = new Section<ccAH>(MB, attr, 0, errCount * attr.Stride, true);
         byte[] data = new byte[errCount * attr.Stride * 2];
         Buffer.BlockCopy(hist.b, 0, data, 0, data.Length);
         int n = 0;
         for (int i = 0; i < errCount * attr.Stride * 2; i += attr.Stride * 2) {
            int year = (data[i + 0] << 8) + data[i + 1];
            int month = data[i + 3];
            int day = data[i + 5];
            int hour = data[i + 7];
            int minute = data[i + 9];
            int second = data[i + 11];
            int fault = (data[i + 12] << 8) + data[i + 13];
            errs[n++] = $"{fault:D3} {year}/{month:D2}/{day:D2} {hour:D2}:{minute:D2}:{second:D2}";
         }
         AsyncComplete ac = new AsyncComplete(MB, pkt) { MultiLine = errs, Value = errCount };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void GetStatus(ModbusPkt pkt) {
         AsyncComplete ac = new AsyncComplete(MB, pkt) {
            Status = GetStatus(),
         };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void IssueccIJP(ModbusPkt pkt) {
         MB.SetAttribute(pkt.Attribute, pkt.Value);
         AsyncComplete ac = new AsyncComplete(MB, pkt) {
            Attribute = pkt.Attribute,
            Value = pkt.Value,
            Status = GetStatus(),
         };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Modbus_Log(object sender, string msg) {
         if (Log != null) {
            parent.BeginInvoke(new EventHandler(delegate { Log(sender, msg); }));
         }
      }

      private void DoSubstitutions(ModbusPkt pkt) {
         bool success = true;
         Substitution sub = null;
         SendRetrieveXML sr = new SendRetrieveXML(MB);
         if (pkt.substitution == null) {
            sub = sr.RetrieveAllSubstitutions(1);
         } else {
            sr.SendSubstitution(pkt.substitution);
         }
         sr = null;
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Success = success, substitution = sub };
         parent.BeginInvoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      #endregion

      #region Service Routines

      private string GetStatus() {
         Section<ccUS> stat = new Section<ccUS>(MB, ccUS.Communication_Status, 0, 4, true);
         char c = (char)stat.GetDecAttribute(ccUS.Communication_Status);
         char r = (char)stat.GetDecAttribute(ccUS.Receive_Status);
         char o = (char)stat.GetDecAttribute(ccUS.Operation_Status);
         char w = (char)stat.GetDecAttribute(ccUS.Warning_Status);
         return new string(new char[] { '\x02', '1', c, r, o, w, '\x03' });
      }

      #endregion

   }

   public class ModbusPkt {

      #region Constructors, Destructors, and Properties

      public AsyncIO.TaskType Type { get; set; }
      public int DotMatrix { get; set; }
      public string Data { get; set; } = string.Empty;
      public string[] Data2 { get; set; } = null;
      public byte[] DataA { get; set; }
      public DateTime When { get; set; } = DateTime.Now;
      public bool View { get; set; } = false;
      public ccIJP Attribute { get; set; }
      public int Value { get; set; }
      public string IpAddress { get; set; }
      public string IpPort { get; set; }
      public Modbus.FunctionCode fc { get; set; }
      public byte DevAddr { get; set; }
      public int Addr { get; set; }
      public int Len { get; set; }
      public object Packet { get; set; }
      public Serialization.Lab Label { get; set; }
      public Serialization.Substitution substitution { get; set; }
      public Modbus_DLL.ccPS PrinterSpec { get; set; }
      public int TimeDelay { get; set; }
      public string Marker { get; set; }
      public int SubOp { get; set; }
      public int CharSize { get; set; }
      public int Page { get; set; }
      public int KbType { get; set; }
      public int RcvLength { get; set; }
      public int Item { get; set; }

      public ModbusPkt(AsyncIO.TaskType Type) {
         this.Type = Type;
      }

      public ModbusPkt(AsyncIO.TaskType Type, string Data) {
         this.Type = Type;
         this.Data = Data;
      }

      public ModbusPkt(AsyncIO.TaskType Type, ccIJP Attribute, int value) {
         this.Type = Type;
         this.Attribute = Attribute;
         this.Value = value;
      }

      #endregion

   }

   public class AsyncComplete {

      #region Constructors, Destructors, and Properties

      public Modbus Printer { get; set; }
      public AsyncIO.TaskType Type { get; set; }
      public string Resp1 { get; set; }
      //public string Resp2 { get; set; }
      public ccIJP Attribute { get; set; }
      public int Value { get; set; }
      public string[] MultiLine { get; set; }
      public bool Success { get; set; } = true;
      public byte[] DataA { get; set; }
      public object Packet { get; set; }
      public string Status { get; set; }
      public Substitution substitution { get; set; }

      public AsyncComplete(Modbus p, ModbusPkt pkt) {
         this.Printer = p;
         this.Type = pkt.Type;
         this.Packet = pkt.Packet;
      }

      #endregion

   }

}
