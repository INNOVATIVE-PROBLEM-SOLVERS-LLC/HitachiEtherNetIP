using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Modbus_DLL;

namespace ModBus161 {
   public class AsyncIO {

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
         Retrieve,
         WriteData,
         ReadData,
         RecallMessage,
         AddMessage,
         DeleteMessage,
         IssueccIJP,
         GetStatus,
         GetMessages,
         GetErrors,
         Exit,
      }

      // Do the work in the background
      Thread t;

      // Use Blocking Collection to avoid spin waits
      public BlockingCollection<ModbusPkt> Tasks = new BlockingCollection<ModbusPkt>();
      ModbusPkt pkt;

      // Printer to use for I/O
      Modbus MB = null;

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
         try {
            bool done = false;
            // Just one big loop
            while (!done) {
               // Wait for the next request
               pkt = Tasks.Take();
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
                  case TaskType.Retrieve:
                     Retrieve(pkt);
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
                  case TaskType.Exit:
                     done = true;
                     break;
                  default:
                     return;
               }
            }
         } catch {

         } finally {

         }
      }

      private void Connect(ModbusPkt pkt) {
         bool success = MB.Connect(pkt.IpAddress, pkt.IpPort);
         AsyncComplete ac = new AsyncComplete(MB, TaskType.Connect) { Success = success };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Disconnect(ModbusPkt pkt) {
         MB.Disconnect();
         AsyncComplete ac = new AsyncComplete(MB, TaskType.Disconnect);
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Send(ModbusPkt pkt) {
         SendRetrieveXML send = new SendRetrieveXML(MB);
         send.Log += Modbus_Log;
         try {
            send.SendXML(pkt.Data);
         } finally {
            string logXML = send.LogXML;
            AsyncComplete ac = new AsyncComplete(MB, TaskType.Send) { Resp2 = logXML };
            parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
            send.Log -= Modbus_Log;
            send = null;
         }
      }

      private void Retrieve(ModbusPkt pkt) {
         string msgXML = string.Empty;
         string logXML = string.Empty;
         SendRetrieveXML retrieve = new SendRetrieveXML(MB);
         retrieve.Log += Modbus_Log;
         try {
            msgXML = retrieve.Retrieve();
         } finally {
            logXML = retrieve.LogXML;
            AsyncComplete ac = new AsyncComplete(MB, TaskType.Retrieve) { Resp1 = msgXML, Resp2 = logXML };
            parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
            retrieve.Log -= Modbus_Log;
            retrieve = null;
         }
      }

      private void WriteData(ModbusPkt pkt) {
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         bool success = MB.SetAttribute(pkt.DevAddr, pkt.Addr, pkt.DataA);
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         AsyncComplete ac = new AsyncComplete(MB, TaskType.WriteData) { Success = success };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void ReadData(ModbusPkt pkt) {
         MB.GetAttribute(pkt.fc, pkt.DevAddr, pkt.Addr, pkt.Len, out byte[] data);
         AsyncComplete ac = new AsyncComplete(MB, TaskType.ReadData) { DataA = data };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void RecallMessage(ModbusPkt pkt) {
         bool success = MB.SetAttribute(ccPDR.Recall_Message, pkt.Value);
         AsyncComplete ac = new AsyncComplete(MB, TaskType.RecallMessage) { Success = success };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void AddMessage(ModbusPkt pkt) {
         bool success = MB.DeleteMessage(pkt.Value);
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
         success &= MB.SetAttribute(ccPDR.MessageName, pkt.Data);
         success &= MB.SetAttribute(ccPDR.Message_Number, pkt.Value);
         MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
         AsyncComplete ac = new AsyncComplete(MB, TaskType.AddMessage) { Success = success };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void DeleteMessage(ModbusPkt pkt) {
         bool success = MB.DeleteMessage(pkt.Value);
         AsyncComplete ac = new AsyncComplete(MB, TaskType.DeleteMessage) { Success = success };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void GetMessages(ModbusPkt pkt) {
         List<string> msgs = new List<string>();
         string[] s = new string[3];
         // For now, look at the first 48 only.  Need to implement block read
         AttrData attrCount = MB.GetAttrData(ccMM.Registration);
         for (int i = 0; i < Math.Min(3, attrCount.Count); i++) {
            int reg = MB.GetDecAttribute(ccMM.Registration, i);
            if (reg == 0) {
               continue;
            }
            for (int j = 15; j >= 0; j--) {
               if ((reg & (1 << j)) > 0) {
                  int n = i * 16 - j + 15; // 1-origin
                  MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
                  MB.SetAttribute(ccIDX.Message_Number, n + 1);         // Load the message into input registers
                  MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
                  s[0] = MB.GetHRAttribute(ccMM.Group_Number);
                  s[1] = MB.GetHRAttribute(ccMM.Message_Number);
                  s[2] = MB.GetHRAttribute(ccMM.Message_Name);
                  msgs.Add(string.Join(",", s));
               }
            }
         }
         AsyncComplete ac = new AsyncComplete(MB, TaskType.GetMessages) { MultiLine = msgs.ToArray() };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void GetErrors(ModbusPkt pkt) {
         int errCount = MB.GetDecAttribute(ccAH.Message_Count);
         string[] errs = new string[errCount];
         AttrData attr = MB.GetAttrData(ccAH.Year);
         int len = attr.Stride;
         for (int i = 0; i < errCount; i++) {
            MB.GetAttribute(Modbus.FunctionCode.ReadInput, 1, attr.Val + i * len, len * 2, out byte[] data);
            int year = (data[0] << 8) + data[1];
            int month = data[3];
            int day = data[5];
            int hour = data[7];
            int minute = data[9];
            int second = data[11];
            int fault = (data[12] << 8) + data[13];
            errs[i] = $"{fault:###} {year}/{month:##}/{day:##} {hour:##}:{minute:##}:{second:##}";
         }
         AsyncComplete ac = new AsyncComplete(MB, TaskType.GetErrors) { MultiLine = errs, Value = errCount };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void GetStatus(ModbusPkt pkt) {
         string comm = Status.TranslateStatus(Status.StatusAreas.Connection, MB.GetDecAttribute(ccUS.Communication_Status));
         string receive = Status.TranslateStatus(Status.StatusAreas.Reception, MB.GetDecAttribute(ccUS.Receive_Status));
         string operation = Status.TranslateStatus(Status.StatusAreas.Operation, MB.GetDecAttribute(ccUS.Operation_Status));
         string warn = Status.TranslateStatus(Status.StatusAreas.Warning, MB.GetDecAttribute(ccUS.Warning_Status));
         string a1 = Status.TranslateStatus(Status.StatusAreas.Analysis1, MB.GetDecAttribute(ccUS.Analysis_Info_1));
         string a2 = Status.TranslateStatus(Status.StatusAreas.Analysis2, MB.GetDecAttribute(ccUS.Analysis_Info_2));
         string a3 = Status.TranslateStatus(Status.StatusAreas.Analysis3, MB.GetDecAttribute(ccUS.Analysis_Info_3));
         string a4 = Status.TranslateStatus(Status.StatusAreas.Analysis4, MB.GetDecAttribute(ccUS.Analysis_Info_4));
         AsyncComplete ac = new AsyncComplete(MB, TaskType.GetStatus) {
            Resp1 = $"{comm}/{receive}/{operation}/{warn}", Resp2 = $"{a1}/{a2}/{a3}/{a4}" };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void IssueccIJP(ModbusPkt pkt) {
         MB.SetAttribute(pkt.Attribute, pkt.Value);
         AsyncComplete ac = new AsyncComplete(MB, TaskType.IssueccIJP) { Attribute = pkt.Attribute, Value = pkt.Value };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      private void Modbus_Log(object sender, string msg) {
         if (Log != null) {
            parent.BeginInvoke(new EventHandler(delegate { Log(sender, msg); }));
         }
      }

      #endregion

      #region Service Routines

      #endregion

   }

   public class ModbusPkt {

      #region Constructors, Destructors, and Properties

      public AsyncIO.TaskType Type { get; set; }
      public string Data { get; set; } = string.Empty;
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
      public string Resp2 { get; set; }
      public ccIJP Attribute { get; set; }
      public int Value { get; set; }
      public string[] MultiLine { get; set; }
      public bool Success { get; set; } = true;
      public byte[] DataA { get; set; }

      public AsyncComplete(Modbus p, AsyncIO.TaskType type) {
         this.Printer = p;
         this.Type = type;
      }

      #endregion

   }

}
