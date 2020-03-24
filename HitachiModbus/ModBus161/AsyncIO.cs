using System;
using System.Collections.Concurrent;
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
      public delegate void CompleteHandler(object sender, TaskType type, string msg);

      #endregion

      #region Data Declarations

      Form parent;

      public enum TaskType {
         Send = 0,
         Retrieve = 1,
         Exit = 2,
      }

      // Do the work in the background
      Thread t;

      // Use Blocking Collection to avoid spin waits
      public BlockingCollection<ModbusPkt> Tasks = new BlockingCollection<ModbusPkt>();
      ModbusPkt pkt;

      // Printer to use for I/O
      Modbus MP = null;

      #endregion

      #region Constructors and Destructors

      public AsyncIO(Form parent, Modbus MP) {
         // 
         this.parent = parent;
         this.MP = MP;
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
               // Post the queue count and wait for the next request
               pkt = Tasks.Take();
               switch (pkt.Type) {
                  case TaskType.Send:
                     string xml = string.Empty;
                     SendRetrieveXML send = new SendRetrieveXML(MP);
                     send.Log += Modbus_Log;
                     try {
                        send.SendXML(pkt.Data);
                     } finally {
                        if (Complete != null) {
                           parent.Invoke(new EventHandler(delegate { Complete(this, TaskType.Send, send.LogXML); }));
                        }
                        send.Log -= Modbus_Log;
                        send = null;
                     }
                     break;
                  case TaskType.Retrieve:

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
      public DateTime When { get; set; } = DateTime.Now;
      public bool View { get; set; } = false;

      public ModbusPkt(AsyncIO.TaskType Type, string Data) {
         this.Type = Type;
         this.Data = Data;
      }

      public ModbusPkt(AsyncIO.TaskType Type, bool View) {
         this.Type = Type;
         this.View = View;
      }

      public ModbusPkt(AsyncIO.TaskType Type) {
         this.Type = Type;
      }

      #endregion

   }



}
