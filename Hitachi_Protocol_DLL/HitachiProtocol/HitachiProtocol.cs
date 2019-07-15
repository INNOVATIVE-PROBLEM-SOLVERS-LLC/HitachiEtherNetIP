using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitachiProtocol {

   public partial class HitachiPrinter {

      #region Data Declarations

      static List<Ops> OpNames;

      DateTime previous;

      Form parent;
      int ID;

      HitachiPrinterType printerType;
      bool rxClass;

      ConnectionType Connection = ConnectionType.OffLine;

      // Serial Port and Connection data
      SerialPort comPXR;
      string PortName;
      int BaudRate;
      int DataBits;
      StopBits StopBits;
      Parity Parity;

      // Socket and Ethernet Connection
      Socket soxPXR;
      IPAddress IPAddress;
      int IPPort;

      // Data Arrival
      object PartialLock = new object();
      byte[] Partial;
      int PartialLength;
      int PartialTimeoutCount = 0;

      // Run queues
      List<HPRequest> HP_Requests;
      List<HPRequest> HP_Idle;

      bool PXROperationInProgress;
      bool LeaveInStandBy = false;

      // Printer Data
      int maxItems;
      int[] MaxExtent = new int[2];
      int nozzle = 0;
      HPEventLogging eventLogging = HPEventLogging.All;

      Encoding encode = System.Text.Encoding.GetEncoding("ISO-8859-1");

      int RcvLength = 0;

      #endregion

      #region Constructors/Destructors

      // Initialize the default space
      void SetDefaults() {

         lock (PartialLock) {
            Partial = new byte[4096];
            PartialLength = 0;
            PartialTimeoutCount = 0;
         }

         // Connection data
         IPPort = 8000;
         IPAddress = IPAddress.Parse("127.0.0.1");

         //
         MaxItems = 24;
         SOP4Enabled = true;

         // Special features
         eventLogging = HPEventLogging.All;

         // Build request and idle lists
         HP_Requests = new List<HPRequest>();
         HP_Idle = new List<HPRequest>();

         // Status Area
         statusArea = new HPStatus(ID);
         // Signaling new PXR object
         if (Connection == ConnectionType.OffLine) {
            BuildStatus(StateChange.OffLine);
         } else {
            BuildStatus(StateChange.Disconnected);
         }

         // Timer is a pain
         RequestQueueTimer = new System.Windows.Forms.Timer {
            Interval = 20
         };
         RequestQueueTimer.Tick += ProcessRequestQueue;
         TimedDelay(100);
         RequestQueueTimer.Start();
      }
      // Clean-up
      void CleanUp() {

         // Release the timer
         RequestQueueTimer.Tick -= ProcessRequestQueue;

         // Clean up the request queue
         if (HP_Requests != null) {
            HP_Requests.Clear();
         }

         // Clean up the idle queue
         if (HP_Idle != null) {
            HP_Idle.Clear();
         }

         // Release the timer and status area
         TimedDelay(0);
         statusArea = null;
      }

      #endregion

      #region Request Queue Processing

      DateTime DeviceIdleTime;

      // Simulate Print Start/End
      int CouponTimerInterval = -1;
      DateTime CouponTimerNextClick;
      bool PrintStartSent = false;
      string LastMessageText = string.Empty;
      System.Windows.Forms.Timer RequestQueueTimer = null;

      // Process Hitachi Printer Request Queue
      void ProcessRequestQueue(object sender, EventArgs e) {
         DateTime thistime = DateTime.Now;
         TimeSpan diff = thistime - previous;
         double diffMS = diff.TotalMilliseconds;
         previous = thistime;

         // Simulation code for print start and print end printer interrupts
         if (CouponTimerInterval > 0 && CouponTimerNextClick < DateTime.Now) {
            //
            // Advance the clock
            if (PrintStartSent) {
               PrintStartSent = false;
               CouponTimerNextClick = CouponTimerNextClick.AddMilliseconds(CouponTimerInterval);
               parent.BeginInvoke(new EventHandler(delegate { Sim_DataReceived("\x02\x10\x19\x03"); }));
            } else {
               PrintStartSent = true;
               CouponTimerNextClick = CouponTimerNextClick.AddMilliseconds(CouponTimerInterval / 2);
               parent.BeginInvoke(new EventHandler(delegate { Sim_DataReceived("\x02\x07\x03"); }));
            }
         }

         // Is idle time requested?
         if (DeviceIdleTime > DateTime.UtcNow) {
            return;
         }

         // Was an operation being timed?
         if (PXROperationInProgress) {
            // Get the next request
            Debug.Assert(HP_Requests.Count > 0, "PXR Operation In Progress but no request available!");
            HPRequest mReq = HP_Requests[0];
            if (mReq.Op == PrinterOps.TimedDelay) {
               // Timed delays are handled by the timer
               CompleteOperation(mReq, $"Timed Delay({mReq.TimedDelay})");
               PXROperationInProgress = false;
            } else {
               // Are we faking the completion?
               if (Connection == ConnectionType.Simulator) {
                  // Send back any command that is being simulated and try next operation
                  Log?.Invoke(this, new HPEventArgs("Fake response not implemented"));
                  //parent.BeginInvoke(new EventHandler(delegate { Sim_DataReceived(FakeResponse.BuildFakeCompletion(mReq, this)); }));
               } else {
                  // Has this been retried?
                  if (mReq.Retries > 0 || mReq.Op == PrinterOps.Connect) {
                     if (mReq.Op == PrinterOps.Connect && Connection == ConnectionType.EthernetToSerial) {
                        if (soxPXR.Connected) {
                           soxPXR.Shutdown(SocketShutdown.Both);
                           soxPXR.Disconnect(false);
                        }
                        soxPXR.Close();
                        soxPXR.Dispose();
                        soxPXR = null;
                        connectionState = ConnectionStates.Closed;
                     }
                     // Signaling Operation Timed Out == Aborting
                     BuildStatus(StateChange.TimeoutAbort);
                     NotifyClient?.Invoke(this, new HPEventArgs(mReq.Op, "Time-Out/PXR operation timed out"));
                     // Reset the I/O queue
                     ResetPXRQueue();
                  } else {
                     // Signaling Opertaion Timed Out == Retrying
                     BuildStatus(StateChange.TimeoutRetrying);
                     Log?.Invoke(this, new HPEventArgs("Retrying Operation " + OperationName(mReq.Op, mReq.SubOp)));
                     // Retry the operation
                     mReq.Retries += 1;
                     PXROperationInProgress = false;
                  }
               }
            }
         } else {
            // Only time the input if nobody is waiting.
            if (PartialLength > 0) {
               if (++PartialTimeoutCount > 10) {
                  string s = Extract(PartialLength);
                  if ((EventLogging & HPEventLogging.Input) > 0) {
                     Log?.Invoke(this, new HPEventArgs("Partial= " + s));
                  }
                  ProcessReceive(s);
               }
            }
         }
         // Attempt to issue the next command
         IssueOperation();
      }

      void TimedDelay(int Duration) {
         if (Duration > 0 && Duration < 100)
            Duration = 5;
         DeviceIdleTime = DateTime.UtcNow.AddMilliseconds(Duration);
      }

      void Sim_DataReceived(string s) {
         int n = s.Length;
         byte[] b = encode.GetBytes(s);
         lock (PartialLock) {
            for (int i = 0; i < n; i++) {
               Partial[PartialLength++] = b[i];
            }
         }
         ParseInput();
      }

      void ResetPXRQueue() {

         // Mark the control idle
         PXROperationInProgress = false;
         TimedDelay(0);
         lock (PartialLock) {
            PartialLength = 0;
            PartialTimeoutCount = 0;
         }

         // Save request packets and update progress bar information
         while (HP_Requests.Count > 0) {
            MoveToIdleQueue(HP_Requests[0]);
         }

      }

      void MoveToIdleQueue(HPRequest mReq) {

         // Update progress bar information
         RequestCompleted?.Invoke(this);
         // Remove it from the current list
         HP_Requests.Remove(mReq);
         // Add to the idle queue
         HP_Idle.Add(mReq);
      }

      #endregion

      #region Request Processing

      void IssueOperation() {

         // Local Storage
         HPRequest mReq;
         string strData;

         // Issue PXR I/O Operation request if
         //  1) Idle
         //  2) Hardware delay satisfied
         //  3) there is something to do
         if (!PXROperationInProgress && HP_Requests.Count > 0 && DeviceIdleTime <= DateTime.UtcNow) {

            // Get the request detail
            mReq = HP_Requests[0];

            // Get the item and data
            strData = mReq.Data1;

            // Just log it
            if ((EventLogging & HPEventLogging.OperationStart) > 0 && Log != null) {
               string opName = OperationName(mReq.Op, mReq.SubOp);
               if (mReq.Op == PrinterOps.Connect) {
                  switch (Connection) {
                     case ConnectionType.Serial:
                        Log(this, new HPEventArgs($"{opName} Starting => {PortName},{BaudRate},{Parity},{DataBits},{StopBits}"));
                        break;
                     case ConnectionType.EthernetToSerial:
                        Log(this, new HPEventArgs($"{opName} Starting => Ethernet {IPAddress}({IPPort})"));
                        break;
                     case ConnectionType.Simulator:
                        Log(this, new HPEventArgs($"{opName} Starting => In Simulated I/O mode"));
                        break;
                     default:
                        Log(this, new HPEventArgs($"{opName} Starting => Off Line"));
                        break;
                  }
               } else {
                  Log(this, new HPEventArgs($"{opName} Starting =>{strData}<="));
               }
            }
            IssueSerialOperation(mReq);
         }
      }

      void IssueSerialOperation(HPRequest mReq) {
         // Give device time to respond
         TimedDelay(50);
         string strMarker;
         // Fanout on request type
         switch (mReq.Op) {
            case PrinterOps.Connect: // 1
               ConnectToPXR(mReq);
               break;
            case PrinterOps.Disconnect: // 2
               DisconnectFromPXR(false);
               // Complete the request
               CompleteOperation(mReq, string.Empty);
               break;
            case PrinterOps.IssueControl: // 3
               SendOutputToPrinter(BuildControlString(mReq), mReq);
               break;
            case PrinterOps.ColumnSetup: // 4
               SendOutputToPrinter(BuildLineCountSpacing(mReq), mReq);
               break;
            case PrinterOps.WriteSpecification: // 5
               SendOutputToPrinter(BuildSpecificationString(mReq), mReq);
               break;
            case PrinterOps.WriteFormat: // 6
               SendOutputToPrinter(BuildFormatString(mReq), mReq);
               break;
            case PrinterOps.WriteText: // 7
               SendOutputToPrinter(BuildTextString(mReq), mReq);
               break;
            case PrinterOps.WriteCalendarOffset: // 8
               SendOutputToPrinter(BuildCalendarOffset(mReq), mReq);
               break;
            case PrinterOps.WriteCalendarSubRule: // 27
               SendOutputToPrinter(BuildCalendarSubRule(mReq), mReq);
               break;
            case PrinterOps.WriteCalendarSubZS: // 9
               SendOutputToPrinter(BuildCalendarSubZSString(mReq), mReq);
               break;
            case PrinterOps.WriteCalendarZS: // 25
               SendOutputToPrinter(BuildCalendarZSString(mReq), mReq);
               break;
            case PrinterOps.WriteCalendarSub: // 26
               SendOutputToPrinter(BuildCalendarSubString(mReq), mReq);
               break;
            case PrinterOps.WriteCountCondition: // 10
               SendOutputToPrinter(BuildCountConditionString(mReq), mReq);
               break;
            case PrinterOps.WritePattern: // 11
               SendOutputToPrinter(BuildWritePatternString(mReq), mReq);
               break;
            case PrinterOps.Message: // 12
               SendOutputToPrinter(BuildMessageString(mReq), mReq);
               break;
            case PrinterOps.Fetch: // 13
               SendOutputToPrinter(BuildFetchString(mReq), mReq);
               break;
            case PrinterOps.Retrieve: // 14
               SendOutputToPrinter(BuildRetrieveString(mReq), mReq);
               break;
            case PrinterOps.RetrievePattern: // 15
               SendOutputToPrinter(BuildRetrievePatternString(mReq), mReq);
               break;
            case PrinterOps.SetClock: // 16
               SendOutputToPrinter(BuildSetClockString(mReq), mReq);
               break;
            case PrinterOps.Idle: // 17
               strMarker = mReq.Data1;
               HPEventArgs ea = new HPEventArgs(PrinterOps.Idle, strMarker) {
                  nACKs = nACKs,
                  nNAKs = nNAKs
               };
               PXROperationInProgress = true;
               CompleteOperation(mReq, strMarker);
               Complete?.Invoke(this, ea);
               PXROperationInProgress = false;
               IssueOperation();
               break;
            case PrinterOps.PassThru: // 18
               SendOutputToPrinter(mReq.Data1, mReq);
               break;
            case PrinterOps.ENQ: // 19
               SendOutputToPrinter(cENQ.ToString(), mReq);
               break;
            case PrinterOps.SOP16ClearBuffer:
            case PrinterOps.SOP16RestartPrinting: // 20 & 21
               SendOutputToPrinter(BuildSOP16Command(mReq), mReq);
               break;
            case PrinterOps.ChangeInkDropRule: // 22
               SendOutputToPrinter(BuildChangeInkDropRule(mReq), mReq);
               break;
            case PrinterOps.ChangeMessageFormat: // 23
               SendOutputToPrinter(BuildChangeMessageFormat(mReq), mReq);
               break;
            case PrinterOps.PositionItem: // 24
               SendOutputToPrinter(BuildPositionItem(mReq), mReq);
               break;
            case PrinterOps.TimedDelay: // 28
               PXROperationInProgress = true;
               int delay = mReq.TimedDelay;
               TimedDelay(delay);
               break;
            case PrinterOps.CreateMessage: // 29
            case PrinterOps.SendMessage: // 30
               PXROperationInProgress = true;
               CompleteOperation(mReq, "\x05");
               Complete?.Invoke(this, new HPEventArgs(mReq.Op, sACK));
               PXROperationInProgress = false;
               IssueOperation();
               break;
            case PrinterOps.SetNozzle: // 31
               Nozzle = mReq.SubOp;
               PXROperationInProgress = true;
               CompleteOperation(mReq, Nozzle.ToString());
               Complete?.Invoke(this, new HPEventArgs(PrinterOps.SetNozzle, Nozzle, sACK));
               PXROperationInProgress = false;
               IssueOperation();
               break;
            default: // 0
               // Reset the queue and notify user
               ResetPXRQueue();
               NotifyClient?.Invoke(this, new HPEventArgs(mReq.Op, "Unknown IssuePXROperation Command"));
               break;
         }
      }

      void SendOutputToPrinter(string strOutput, HPRequest mReq) {

         if (strOutput == null) {
            strOutput = ""; // Unhandled Exceptions have occurred here, so prevent that, AND log it
            Log?.Invoke(this, new HPEventArgs("The strOutput object is undefined. Changing it to empty string."));
         }

         // Base time on size of buffer expected
         int DelayTime = 6000;

         if (mReq.Op == PrinterOps.Retrieve || mReq.Op == PrinterOps.RetrievePattern) {
            DelayTime = PrintDataTimeout;
         }

         // Is there something to do?
         if (strOutput.Length == 0 || strOutput == sSTX + sETX) {

            // Move the request to the idle queue
            CompleteOperation(mReq, string.Empty);

            // Issue the next operation
            IssueOperation();
         } else {
            // Indicate the minimum amount required 
            RcvLength = mReq.RcvLength;

            // Send based on connection type
            switch (Connection) {
               case ConnectionType.EthernetToSerial:

                  // Now issue the operation if the port is open
                  if (soxPXR != null && soxPXR.Connected) {

                     // Mark Printer Busy and set delay
                     PXROperationInProgress = true;
                     TimedDelay(DelayTime); // intWait

                     // Log output of data
                     if ((EventLogging & HPEventLogging.Output) > 0) {
                        Log?.Invoke(this, new HPEventArgs("Output = " + strOutput));
                     }
                     ReportRawData(false, strOutput);

                     // Issue the output
                     try {
                        soxPXR.Send(encode.GetBytes(strOutput), strOutput.Length, SocketFlags.None);
                     } catch {
                        // Clean up the queue and tell the user
                        NotifyClient?.Invoke(this, new HPEventArgs(mReq.Op,
                                "Ethernet Socket reset because existing connection was forcibly closed by the remote host!"));
                        ResetPXRQueue();
                     }
                  } else {

                     // Clean up the queue and tell the user
                     NotifyClient?.Invoke(this, new HPEventArgs(mReq.Op, "Ethernet Socket not open!"));
                     ResetPXRQueue();
                  }
                  break;
               case ConnectionType.Serial:

                  // Now issue the operation if the port is open
                  if (comPXR != null && comPXR.IsOpen) {

                     // Mark Printer Busy and set delay
                     PXROperationInProgress = true;
                     TimedDelay(DelayTime); // intDelay

                     // Log output of data
                     if ((EventLogging & HPEventLogging.Output) > 0 && Log != null) {
                        Log(this, new HPEventArgs("Output = " + strOutput));
                     }
                     ReportRawData(false, strOutput);

                     // Issue the output
                     try {
                        comPXR.Write(strOutput);
                     } catch {
                        // Clean up the queue and tell the user
                        NotifyClient?.Invoke(this, new HPEventArgs(mReq.Op,
                                "Issue I/O Operation/Port not open!"));
                        ResetPXRQueue();
                     }
                  } else {

                     // Clean up the queue and tell the user
                     NotifyClient?.Invoke(this, new HPEventArgs(mReq.Op, "Issue I/O Operation/Port not open!"));
                     // Move the request to the idle queue
                     CompleteOperation(mReq, cNAK.ToString());

                     // Issue the next operation
                     IssueOperation();
                  }
                  break;
               case ConnectionType.Simulator:

                  // Log output of data
                  if ((EventLogging & HPEventLogging.Output) > 0 && Log != null) {
                     Log(this, new HPEventArgs("Output = " + TranslateInput(strOutput)));
                  }
                  ReportRawData(false, strOutput);

                  // Mark Printer Busy and set delay
                  PXROperationInProgress = true;

                  // Delay so more can be added to queue
                  TimedDelay(5); // intDelay
                  break;
               case ConnectionType.OffLine:
                  Log(this, new HPEventArgs("Attempt to send data to offline printer."));

                  // Move the request to the idle queue
                  CompleteOperation(mReq, string.Empty);

                  // Issue the next operation
                  IssueOperation();
                  break;
               default:

                  // Move the request to the idle queue
                  CompleteOperation(mReq, string.Empty);

                  // Issue the next operation
                  IssueOperation();
                  break;
            }
         }
      }

      void CompleteOperation(HPRequest mReq, string strResult) {

         // Save away the Op/SubOp
         PrinterOps intOp = mReq.Op;
         int intSubOp = mReq.SubOp;

         // Move request to the idle queue
         MoveToIdleQueue(mReq);

         // Just log it
         if ((EventLogging & HPEventLogging.OperationComplete) > 0) {
            Log?.Invoke(this, new HPEventArgs(OperationName(intOp, intSubOp) + " Complete =>" + strResult + "<="));
         }
         if (Connection == ConnectionType.Simulator) {
            parent.BeginInvoke(new EventHandler(delegate { ProcessRequestQueue(null, null); }));
         }
      }

      HPRequest GetRequest(PrinterOps oP, int SubOp = 0) {
         HPRequest mReq;

         RequestAdded?.Invoke(this);
         // Is the idle request queue empty?
         if (HP_Idle.Count == 0) {

            // Get a new packet
            mReq = new HPRequest();
         } else {

            // Get an idle packet
            mReq = HP_Idle[0];
            HP_Idle.RemoveAt(0);
         }
         mReq.Op = oP;
         mReq.SubOp = SubOp;

         // Return the request
         return mReq;
      }

      void IssueRequest(HPRequest mReq) {
         if (printerType == HitachiPrinterType.PX) {
            ScheduleENQ(mReq);
         }
         HP_Requests.Add(mReq);
         if (!PXROperationInProgress && DeviceIdleTime <= DateTime.UtcNow) {
            IssueOperation();
         }
      }

      void ScheduleENQ(HPRequest r) {
         bool enqRequired = false;
         if (printerType == HitachiPrinterType.PX) {

            // Does an ENQ need to be built?
            switch (r.Op) {
               case PrinterOps.ColumnSetup:
               case PrinterOps.WriteSpecification:
               case PrinterOps.WriteFormat:
               case PrinterOps.WriteText:
               case PrinterOps.WriteCalendarOffset:
               case PrinterOps.WriteCalendarSubRule:
               case PrinterOps.WriteCalendarSubZS:
               case PrinterOps.WriteCalendarZS:
               case PrinterOps.WriteCalendarSub:
               case PrinterOps.WriteCountCondition:
               case PrinterOps.WritePattern:
               case PrinterOps.Message:
               case PrinterOps.SetClock:
                  enqRequired = true;
                  break;

               case PrinterOps.PassThru:

                  // Is the first character an <stx>
                  if (r.Data1[0] == cSTX) {
                     enqRequired = true;
                  }
                  break;

               case PrinterOps.IssueControl:

                  // Some do, some don't
                  switch ((ControlOps)r.SubOp) {
                     case ControlOps.HydraulicsStart:
                     case ControlOps.HydraulicsStop:
                     case ControlOps.Ready:
                     case ControlOps.Standby:
                     case ControlOps.ResetAlarm:
                        enqRequired = true;
                        break;
                  }
                  break;
            }
         }

         // Stuff away the data
         if (enqRequired) {
            HPRequest mReq = GetRequest(PrinterOps.ENQ);
            HP_Requests.Add(mReq);
         }
      }

      #endregion

      #region Output String Building Routines

      string BuildChangeInkDropRule(HPRequest mReq) {
         return sSTX + sESC2 + "\x25\x42" + mReq.Data1 + sETX;
      }

      string BuildChangeMessageFormat(HPRequest mReq) {
         string Format = "0";
         MessageStyle = (FormatSetup)mReq.SubOp;
         switch (MessageStyle) {
            case FormatSetup.Overall:
               Format = "1";
               break;
            case FormatSetup.FreeLayout:
               Format = "2";
               break;
         }
         return sSTX + sESC2 + "\x22\x33" + Format + sETX;
      }

      string BuildPositionItem(HPRequest mReq) {
         string strPosition = "";
         string Item;
         int xCoord;
         int yCoord;
         string xSign;
         string ySign;

         HPRequest mTemp = mReq;

         Item = ItemNumber(mTemp.Item);
         if (mTemp.xCoord < 0) {
            xSign = "-";
         } else {
            xSign = "+";
         }
         if (mTemp.yCoord < 0) {
            ySign = "-";
         } else {
            ySign = "+";
         }
         xCoord = Math.Abs(mTemp.xCoord);
         yCoord = Math.Abs(mTemp.yCoord);
         switch ((PositionOps)mTemp.SubOp) {
            case PositionOps.HorizontalVerticalPosition:
               strPosition += sESC2 + "\x24\x31" + Item + xCoord.ToString("00000") + yCoord.ToString("00");
               break;
            case PositionOps.HorizontalPosition:
               strPosition += sESC2 + "\x24\x32" + Item + xCoord.ToString("00000");
               break;
            case PositionOps.VerticalPosition:
               strPosition += sESC2 + "\x24\x33" + Item + yCoord.ToString("00");
               break;
            case PositionOps.HorizontalVerticalMove:
               strPosition += sESC2 + "\x24\x34" + Item + xSign + xCoord.ToString("00000") + ySign + yCoord.ToString("00");
               break;
            case PositionOps.HorizontalMove:
               strPosition += sESC2 + "\x24\x35" + Item + xSign + xCoord.ToString("00000");
               break;
            case PositionOps.VerticalMove:
               strPosition += sESC2 + "\x24\x36" + Item + ySign + yCoord.ToString("00");
               break;
            default:
               break;
         }
         return sSTX + strPosition + sETX;
      }

      string BuildTextString(HPRequest mReq) {
         // Local storage
         string strTemp;
         string strMessage;
         int intItem;
         string[] strData;
         string strDataA;
         char strC;
         int intC;
         bool blnPreserve;
         bool SendAllItems;
         int reg;

         // Get item number
         if (mReq.Item == 0) {
            intItem = 1;
            SendAllItems = true;
         } else {
            intItem = mReq.Item;
            SendAllItems = false;
         }

         // Must handle special case of null string
         strData = mReq.Data1.Split(new string[] { "\r\n" }, StringSplitOptions.None);

         // 
         blnPreserve = intItem < 0;
         intItem = System.Math.Abs(intItem);

         // Start with empty message
         strMessage = string.Empty;

         // Messages items not supported by the hitachi printer may not be sent to the printer
         bool UnsupportedItem;

         // Repeat for all parts of the message
         for (int i = 0; i < strData.Length; i++) {
            // Repeat for all sections of the message
            while (strData[i].Length > 0) {
               UnsupportedItem = false;
               if ((rxClass) && !TenCharsPerItem) {
                  strTemp = strData[i];
                  strData[i] = string.Empty;
               } else {
                  // Get next section of message to output
                  if (strData[i].Length > 10) {
                     strTemp = strData[i].Substring(0, 10);
                     strData[i] = strData[i].Substring(10);
                  } else {
                     strTemp = strData[i];
                     strData[i] = string.Empty;
                  }
               }

               // Is there attributed data
               strDataA = string.Empty;
               for (int n = 0; n < strTemp.Length; n++) {
                  // Get one character
                  strC = strTemp[n];
                  intC = (int)strC;
                  #region Translate Line
                  //
                  // Standard character or User defined character
                  if (intC < 0x80) {
                     strDataA = strDataA + strC;
                  } else if ((intC & (int)AC.Mask) == (int)AC.UserPattern) {
                     // These are the user pattern characters 0 thru 199
                     reg = intC - (int)AC.UserPattern;
                     if (reg > 191) {
                        strDataA = strDataA + (char)0xF2 + (char)(reg - 192 + 0x20);
                     } else {
                        strDataA = strDataA + (char)0xF1 + (char)(reg + 0x40);
                     }
                  } else if ((intC & (int)AC.Mask) == (int)AC.FreePattern) {
                     reg = intC - (int)AC.FreePattern;
                     strDataA = strDataA + (char)0xF6 + (char)(reg + 0x40);
                  } else if (intC >= 0x80 && intC < (int)AC.UserPattern) {
                     //DataRow[] drPattern = Utils.HitachiStandardPatterns.Select($"SP_Char_Code='{strC}'");
                     //if (drPattern.Length > 0) {
                     //   int HitachiCode = drPattern[0].Field<int>("SP_HitachiCode");
                     //   strDataA = strDataA + (char)(HitachiCode >> 8) + (char)(HitachiCode & 0xff);
                     //} else {
                     strDataA = strDataA + "?";
                     //}
                  } else {
                     //
                     // Translate the character
                     switch ((AC)intC) {
                        case AC.Apostrophe:
                           strDataA = strDataA + Chr(0xF2) + Chr(0x40);
                           break;
                        case AC.Period:
                           strDataA = strDataA + Chr(0xF2) + Chr(0x41);
                           break;
                        case AC.Colon:
                           strDataA = strDataA + Chr(0xF2) + Chr(0x42);
                           break;
                        case AC.Comma:
                           strDataA = strDataA + Chr(0xF2) + Chr(0x43);
                           break;
                        case AC.Space:
                           strDataA = strDataA + Chr(0xF2) + Chr(0x44);
                           break;
                        case AC.SemiColon:
                           strDataA = strDataA + Chr(0xF2) + Chr(0x45);
                           break;
                        case AC.Exclamation:
                           strDataA = strDataA + Chr(0xF2) + Chr(0x46);
                           break;
                        case AC.Year: // "Y" Year
                           strDataA = strDataA + Chr(0xF2) + Chr(0x50);
                           break;
                        case AC.Month: // "M" Month
                           strDataA = strDataA + Chr(0xF2) + Chr(0x51);
                           break;
                        case AC.Day: // "D" Day
                           strDataA = strDataA + Chr(0xF2) + Chr(0x52);
                           break;
                        case AC.Hour: // "h" Hour
                           strDataA = strDataA + Chr(0xF2) + Chr(0x53);
                           break;
                        case AC.Minute: // "m" Minute
                           strDataA = strDataA + Chr(0xF2) + Chr(0x54);
                           break;
                        case AC.Second:// "s" Second
                           strDataA = strDataA + Chr(0xF2) + Chr(0x55);
                           break;
                        case AC.TotalDays: // "T" Total Days
                           strDataA = strDataA + Chr(0xF2) + Chr(0x56);
                           break;
                        case AC.MonthName: // "J" Jan - Dec
                           strDataA = strDataA + Chr(0xF2) + Chr(0x57);
                           break;
                        case AC.Week: // "W" Week
                           strDataA = strDataA + Chr(0xF2) + Chr(0x58);
                           break;
                        case AC.DayOfWeek: // "w" Day of Week
                           strDataA = strDataA + Chr(0xF2) + Chr(0x59);
                           break;
                        case AC.Count: // "N" Count
                           strDataA = strDataA + Chr(0xF2) + Chr(0x5A);
                           break;
                        case AC.Shift: // "F" Shift
                           strDataA = strDataA + GetShiftCode();
                           break;
                        default:
                           strDataA = strDataA + strC;
                           break;
                     }
                  }
                  #endregion
               }

               // Add this part to the message, advance to next item
               if (!UnsupportedItem) {
                  //if (global.SettingsConfig.Mode != "Recall" || !UnsupportedItem) {
                  strMessage = strMessage + cDLE + ItemNumber(intItem) + strDataA;
               }
               intItem++;
            }
         }

         // Do we want to clear the rest of the items?
         if (!blnPreserve) {

            // clear out rest of buffer where contents are unknown
            if (SendAllItems) {
               for (int i = intItem; i <= MaxExtent[Nozzle]; i++) {
                  strMessage = strMessage + cDLE + ItemNumber(i);
               }
            }

            // Save max penetration of this message
            MaxExtent[Nozzle] = intItem - 1;
         }

         strMessage = sSTX + strMessage + sETX;
         LastMessageText = strMessage;
         // Load a single item
         return strMessage;
      }

      string BuildControlString(HPRequest mReq) {
         // Fan out on Sub Operation
         switch ((ControlOps)mReq.SubOp) {
            case ControlOps.ComOn:
               if (rxClass) {
                  return sESC2 + "s";
               } else {
                  return sESC + "y";
               }
            case ControlOps.ComOff:
               if (rxClass) {
                  return sESC2 + "t";
               } else {
                  return sESC + "z";
               }
            case ControlOps.HydraulicsStart:
               if (rxClass) {
                  return sSTX + sESC2 + "r0" + sETX;
               } else {
                  return sSTX + sESC + "q0" + sETX;
               }
            case ControlOps.HydraulicsStop:
               if (rxClass) {
                  return sSTX + sESC2 + "r1" + sETX;
               } else {
                  return sSTX + sESC + "q1" + sETX;
               }
            case ControlOps.Ready:
               if (rxClass) {
                  return sSTX + sESC2 + "r2" + sETX;
               } else {
                  return sSTX + sESC + "q2" + sETX;
               }
            case ControlOps.Standby:
               if (rxClass) {
                  return sSTX + sESC2 + "r3" + sETX;
               } else {
                  return sSTX + sESC + "q3" + sETX;
               }
            case ControlOps.ResetAlarm:
               nACKs = nNAKs = 0;
               if (SOP4Enabled || rxClass) {
                  if (rxClass) {
                     return sSTX + sESC2 + "r4" + sETX;
                  } else {
                     return sSTX + sESC + "q4" + sETX;
                  }
               } else {
                  if (rxClass) {
                     return sESC2 + "s";
                  } else {
                     return sESC + "y";
                  }
               }
            case ControlOps.DC2:
               return cDC2.ToString();
            case ControlOps.DC3:
               return cDC3.ToString();
            case ControlOps.Enquire:
               return cENQ.ToString();
            case ControlOps.ClearAll:
               if (rxClass) {
                  if (printerType == HitachiPrinterType.TwinNozzle && Nozzle == 1) {
                     return string.Empty;
                  } else {
                     return sSTX + sESC2 + (char)0x7A + sETX;
                  }
               } else {
                  return string.Empty;
               }
            case ControlOps.ClearAllByNozzle:
               return sSTX + sESC2 + (char)0x7B + sDLE + ItemNumber(1) + sETX;
            default:
               return string.Empty;
         }
      }

      // Other Escaped Characters
      enum EscapeOther {
         ItemNumber = 0x24,
         OverallColumnSetup = 0x2B,
         LineCountSpacing = 0x22,
         ItemNumberRX = 0x70
      }

      string BuildSpecificationString(HPRequest mReq) {

         char eCharacterHeight;
         char eCharacterWidth;
         char eCharacterOrientation;
         char eRepeatIntervals;
         char eRepeatCount;
         char ePrintStartDelay;
         char ePrintStartDelayReverse;
         char eTargetSensorTimer;
         char eTargetSensorFilter;
         char eTargetSensorFilterValue;
         char eHighSpeedPrinting;
         char eProductSpeedMatching;
         char eFrequencyDivisor;
         char eInkDropUsage;
         char eInkDropChargeRule;
         // Twin Nozzle
         char eLeadingCharWidthControl = (char)0x43;
         char eLeadingCharWidthControlWidth = (char)0x44;
         char eNozzleSpaceAlignment = (char)0x45;
         string NozzleSelect = string.Empty;
         if (printerType == HitachiPrinterType.TwinNozzle) {
            NozzleSelect = sESC2 + (char)0x7C + (Nozzle + 1).ToString();
         }
         bool BothNozzles;


         // Print Specification Characters

         if (rxClass) {
            eCharacterHeight = (char)0x31;
            eInkDropUsage = (char)0x32;
            eHighSpeedPrinting = (char)0x33;
            eCharacterWidth = (char)0x34;
            eCharacterOrientation = (char)0x35;
            ePrintStartDelay = (char)0x36;
            ePrintStartDelayReverse = (char)0x37;
            eProductSpeedMatching = (char)0x38;
            eFrequencyDivisor = (char)0x39;
            eRepeatCount = (char)0x3d;
            eRepeatIntervals = (char)0x3e;
            eTargetSensorTimer = (char)0x3f;
            eTargetSensorFilter = (char)0x40;
            eTargetSensorFilterValue = (char)0x41;
            eInkDropChargeRule = (char)0x42;
         } else {
            eCharacterHeight = (char)0x30;
            eCharacterWidth = (char)0x31;
            eCharacterOrientation = (char)0x32;
            eRepeatIntervals = (char)0x34;
            eRepeatCount = (char)0x35;
            ePrintStartDelay = (char)0x33;
            ePrintStartDelayReverse = (char)0x36;
            eTargetSensorTimer = (char)0x37;
            eTargetSensorFilterValue = (char)0x38;
            eTargetSensorFilter = (char)0x39;
            eHighSpeedPrinting = (char)0x3A;
            eProductSpeedMatching = (char)0x3B;
            eFrequencyDivisor = (char)0x3C;
            eInkDropUsage = (char)0x3D;
            eInkDropChargeRule = (char)0x42;
         }

         // local storage
         string strData;
         string strData2;
         SpecificationOps intSubOp;
         string strResult = "";

         // Set up for first time thru
         strResult = string.Empty;

         HPRequest mTemp = mReq;

         do {
            string strPart = "";
            // Get sub type and data
            strData = "00000" + mTemp.Data1;
            strData2 = "00000" + mTemp.Data2;
            intSubOp = (SpecificationOps)mTemp.SubOp;
            BothNozzles = string.IsNullOrEmpty(NozzleSelect);

            // form escape sequence for each Sub Operation and data
            switch (intSubOp) {
               case SpecificationOps.CharacterHeight:
                  strPart = eCharacterHeight + Right(strData, 2);
                  break;
               case SpecificationOps.CharacterWidth:
                  if (rxClass) {
                     strPart = eCharacterWidth + Right(strData, 4);
                  } else {
                     strPart = eCharacterWidth + Right(strData, 3);
                  }
                  break;
               case SpecificationOps.CharacterOrientation:
                  strPart = eCharacterOrientation + Right(strData, 1);
                  break;
               case SpecificationOps.PrintStartDelay:
                  strPart = ePrintStartDelay + Right(strData, 4);
                  break;
               case SpecificationOps.RepeatIntervals:
                  if (rxClass) {
                     strPart = eRepeatIntervals + Right(strData, 5);
                  } else {
                     strPart = eRepeatIntervals + Right(strData, 4);
                  }
                  break;
               case SpecificationOps.RepeatCount:
                  strPart = eRepeatCount + Right(strData, 4);
                  break;
               case SpecificationOps.PrintStartDelayReverse:
                  strPart = ePrintStartDelayReverse + Right(strData, 4);
                  break;
               case SpecificationOps.TargetSensorTimer:
                  strPart = eTargetSensorTimer + Right(strData, 3);
                  break;
               case SpecificationOps.TargetSensorFilter:
                  strPart = eTargetSensorFilter + Right(strData, 1);
                  break;
               case SpecificationOps.TargetSensorFilterDivision:
                  strPart = eTargetSensorFilterValue + Right(strData, 4);
                  break;
               case SpecificationOps.HighSpeedPrinting:
                  strPart = eHighSpeedPrinting + Right(strData, 1);
                  break;
               case SpecificationOps.ProductSpeedMatching:
                  strPart = eProductSpeedMatching + Right(strData, 1);
                  break;
               case SpecificationOps.FrequencyDivisor:
                  strPart = eFrequencyDivisor + Right(strData, 3);
                  break;
               case SpecificationOps.InkDropUsage:
                  if (printerType != HitachiPrinterType.PXRH) {
                     strPart = eInkDropUsage + Right(strData, 2);
                  }
                  break;
               case SpecificationOps.OverallColumnSetup:
                  strPart = Chr(EscapeOther.OverallColumnSetup) + Right(strData, 1);
                  break;
               case SpecificationOps.PrintStartDelayAll:
                  strPart = ePrintStartDelay + Right(strData, 4) + sESC + ePrintStartDelayReverse + Right(strData2, 4);
                  break;
               case SpecificationOps.InkDropChargeRule:
                  if (rxClass && MessageStyle != FormatSetup.FreeLayout) {
                     strPart = eInkDropChargeRule + Right(strData, 1);
                  }
                  break;
               case SpecificationOps.LeadingCharWidthControl:
                  if (printerType == HitachiPrinterType.TwinNozzle) {
                     strPart = eLeadingCharWidthControl + Right(strData, 1);
                     BothNozzles = true;
                  }
                  break;
               case SpecificationOps.LeadingCharWidthControlWidth:
                  if (printerType == HitachiPrinterType.TwinNozzle) {
                     strPart = eLeadingCharWidthControlWidth + Right(strData, 2) + Right(strData2, 2);
                     BothNozzles = true;
                  }
                  break;
               case SpecificationOps.NozzleSpaceAlignment:
                  if (printerType == HitachiPrinterType.TwinNozzle) {
                     strPart = eNozzleSpaceAlignment + Right(strData, 1);
                     BothNozzles = true;
                  }
                  break;
               default:
                  strPart = string.Empty;
                  break;
            }
            if (strPart != string.Empty) {
               if (rxClass) {
                  if (BothNozzles) {
                     strResult += sESC2 + (char)0x25 + strPart;
                  } else {
                     strResult += NozzleSelect + sESC2 + (char)0x25 + strPart;
                  }
               } else {
                  strResult += sESC + strPart;
               }
            }

            if (!Equals(mTemp, mReq)) {
               MoveToIdleQueue(mTemp);
            }
            if (HP_Requests.Count == 1) {
               break;
            }
            mTemp = HP_Requests[1];

         } while (MergeRequests && mTemp.Op == PrinterOps.WriteSpecification);
         // Build final string
         return sSTX + strResult + sETX;
      }

      string BuildLineCountSpacing(HPRequest mReq) {

         // Local Storage
         int i;
         int j;
         int intCols;
         string lc = mReq.Data1;
         string ls = mReq.Data2;
         string strResult = String.Empty;

         string FixedCount = new string(lc[0], lc.Length);
         string FixedSpacing = new string(ls[0], ls.Length);
         if (lc == FixedCount && ls == FixedSpacing && printerType != HitachiPrinterType.TwinNozzle) {
            // Set uniform spacing
            lc = lc.Substring(0, 1);
            ls = ls.Substring(0, 1);
         }

         // Handle global settings
         if (lc.Length == 1 && printerType != HitachiPrinterType.TwinNozzle) {
            if (rxClass) {
               strResult = sSTX + sESC2 + Chr(0x22) + Chr(0x32) + lc.Substring(0, 1) + ls.Substring(0, 1) + sETX;
            } else {
               strResult = sSTX + sESC + Chr(EscapeOther.LineCountSpacing) + lc.Substring(0, 1) + ls.Substring(0, 1) + sETX;
            }
         } else {
            strResult = sSTX;

            // Step thru the columns
            i = 1;
            while (lc.Length > 0) {
               intCols = Convert.ToInt16(lc.Substring(0, 1));
               for (j = i; j <= i + intCols - 1; j++) {
                  //
                  // Don't output single row columns as they were set with global command
                  if (lc.Substring(0, 1) != "1" || printerType == HitachiPrinterType.TwinNozzle) {
                     if (rxClass) {
                        strResult = strResult + Item(j) + sESC2 + Chr(0x22) + Chr(0x32) + lc.Substring(0, 1) + ls.Substring(0, 1);
                     } else {
                        strResult = strResult + Item(j) + sESC + Chr(EscapeOther.LineCountSpacing) + lc.Substring(0, 1) + ls.Substring(0, 1);
                     }
                  }
               }
               i += intCols;
               lc = lc.Substring(1);
               ls = ls.Substring(1);
            }

            // Put in the trailer
            strResult = strResult + sETX;
         }
         return strResult;
      }

      string BuildCalendarSubZSString(HPRequest mReq) {
         string result = "\x02";
         HPRequest mTemp = mReq;
         int maxLength;
         if (rxClass) {
            maxLength = 3000 - 150;
         } else {
            maxLength = 1500 - 150;
         }
         if (rxClass) {
            do {
               if (SubstitutionRules) {
                  result += "\x1f\x28\x33" + (char)(0x30 + mTemp.BlockNo) + (char)(mTemp.SubOp + 0x30) + mTemp.Data1;
               }
               result += "\x1f\x28\x34" + (char)(0x30 + mTemp.BlockNo) + (char)(mTemp.SubOp + 0x30) + mTemp.Data2;
               if (!Equals(mTemp, mReq)) {
                  MoveToIdleQueue(mTemp);
               }
               if (HP_Requests.Count == 1) {
                  break;
               }
               mTemp = HP_Requests[1];
               if (!MergeRequests || mTemp.Op != PrinterOps.WriteCalendarSubZS) {
                  break;
               }
            } while (result.Length < maxLength);
         } else {
            do {
               result += "\x1b\x77" + (char)(mTemp.Item + 0x30) + (char)(mTemp.SubOp + 0x30) + mTemp.Data1;
               if (!Equals(mTemp, mReq)) {
                  MoveToIdleQueue(mTemp);
               }
               if (HP_Requests.Count == 1) {
                  break;
               }
               mTemp = HP_Requests[1];
               if (!MergeRequests || mTemp.Op != PrinterOps.WriteCalendarSubZS) {
                  break;
               }
            } while (result.Length < maxLength);
         }
         return result + sETX;
      }

      string BuildCalendarZSString(HPRequest mReq) {
         string result = "\x02";
         HPRequest mTemp = mReq;
         int maxLength;
         if (rxClass) {
            maxLength = 3000 - 150;
         } else {
            maxLength = 1500 - 150;
         }
         if (rxClass) {
            do {
               result += "\x1f\x28\x34" + (char)(0x30 + mTemp.BlockNo) + (char)(mTemp.SubOp + 0x30) + mTemp.Data2;
               if (!Equals(mTemp, mReq)) {
                  MoveToIdleQueue(mTemp);
               }
               if (HP_Requests.Count == 1) {
                  break;
               }
               mTemp = HP_Requests[1];
               if (!MergeRequests || mTemp.Op != PrinterOps.WriteCalendarZS) {
                  break;
               }
            } while (result.Length < maxLength);
         } else {
            do {
               result += "\x1b\x77" + (char)(mTemp.Item + 0x30) + (char)(mTemp.SubOp + 0x30) + mTemp.Data2;
               if (!Equals(mTemp, mReq)) {
                  MoveToIdleQueue(mTemp);
               }
               if (HP_Requests.Count == 1) {
                  break;
               }
               mTemp = HP_Requests[1];
               if (!MergeRequests || mTemp.Op != PrinterOps.WriteCalendarZS) {
                  break;
               }
            } while (result.Length < maxLength);
         }
         return result + sETX;
      }

      string BuildCalendarSubString(HPRequest mReq) {
         HPRequest mTemp = mReq;
         int maxLength;
         if (rxClass) {
            maxLength = 3000 - 150;
         } else {
            maxLength = 1500 - 150;
         }
         if (rxClass) {
            string result = "\x02";
            do {
               if (SubstitutionRules) {
                  result += "\x1f\x28\x33" + (char)(0x30 + mTemp.BlockNo) + (char)(mTemp.SubOp + 0x30) + mTemp.Data1;
               }
               if (!Equals(mTemp, mReq)) {
                  MoveToIdleQueue(mTemp);
               }
               if (HP_Requests.Count == 1) {
                  break;
               }
               mTemp = HP_Requests[1];
               if (!MergeRequests || mTemp.Op != PrinterOps.WriteCalendarSub) {
                  break;
               }
            } while (result.Length < maxLength);
            return result + sETX;
         } else {
            return "";
         }
      }

      // Print Format Characters
      enum EscapeFormat {
         FontICSpace = 0x21,
         IncreasedWidth = 0x28,
         BC_Use = 0x29,
         BC_DataMatrixMode = 0x27,
         BC_Type = 0x2A,
         BC_EANPrefixCode = 0x2C,
         BC_CodeSet = 0x3F
      }

      string BuildFormatString(HPRequest mReq) {

         string strResult = string.Empty;
         string strItem = Item(mReq.Item);
         int BarcodeType;

         HPRequest mTemp = mReq;

         do {

            strItem = Item(mTemp.Item);
            if (mTemp.Data5 == "(None)") {
               BarcodeType = 0;
            } else {
               BarcodeType = BarcodeNameToHitachi(mTemp.Data5, this.RXClass);
               //if (rxClass) {
               //   mTemp.Data2 = "00"; // ICS 0 for Barcodes
               //} else {
               //   mTemp.Data2 = "0"; // ICS 0 for Barcodes
               //}
            }
            int HumanReadable;
            switch (mTemp.Data9) {
               case "5X5":
                  HumanReadable = 1;
                  break;
               case "5X7":
                  HumanReadable = 2;
                  break;
               default:
                  HumanReadable = 0;
                  break;
            }

            if (mTemp.Item < 1 || BarcodeType != -1) {
               if (rxClass) {
                  strResult += strItem + sESC2 + (char)0x23 + "3" + (char)(0x30 + BarcodeType);
                  if (BarcodeType > 0) {
                     strResult += strItem + sESC2 + (char)0x23 + "4" + (char)(0x30 + HumanReadable);
                  }
                  if (mTemp.Data5 == "EAN-13") {
                     strResult += strItem + sESC2 + (char)0x23 + "5" + mTemp.Data8;
                  }
               } else {
                  if (mTemp.Data5 == "(None)") {
                     strResult += strItem + sESC + Chr(EscapeFormat.BC_Use) + "0";
                  } else {
                     strResult += strItem + sESC + Chr(EscapeFormat.BC_Use) + "1" +
                         sESC + Chr(EscapeFormat.BC_Type) + (char)(0x30 + BarcodeType);
                     if (mTemp.Data5 == "EAN-13") {
                        strResult += strItem + sESC + Chr(EscapeFormat.BC_EANPrefixCode) + mTemp.Data8;
                     } else if (mTemp.Data5.Substring(0, 2) == "DM") {
                        strResult += strItem + sESC + Chr(EscapeFormat.BC_DataMatrixMode) + "1";
                     } else if (mTemp.Data5 == "Code 128(Code Set B)") {
                        strResult += strItem + sESC + Chr(EscapeFormat.BC_CodeSet) + "0";
                     } else if (mTemp.Data5 == "Code 128(Code Set C)") {
                        strResult += strItem + sESC + Chr(EscapeFormat.BC_CodeSet) + "1";
                     }
                  }
               }
            }

            if (rxClass) {
               strResult = strResult + strItem + sESC2 + (char)0x23 + "1" + GetFont(mTemp.Data1) + mTemp.Data2 + strItem + sESC2 + (char)0x23 + "2" + mTemp.Data3;
            } else {
               strResult = strResult + strItem + sESC + Chr(EscapeFormat.FontICSpace) + GetFont(mTemp.Data1) + mTemp.Data2 + strItem + sESC + Chr(EscapeFormat.IncreasedWidth) + mTemp.Data3;
            }
            if (!Equals(mTemp, mReq)) {
               MoveToIdleQueue(mTemp);
            }
            if (HP_Requests.Count == 1) {
               break;
            }
            mTemp = HP_Requests[1];
         } while (MergeRequests && mTemp.Op == PrinterOps.WriteFormat);

         // Put it all together
         return sSTX + strResult + sETX;
      }

      string BuildSOP16Command(HPRequest mReq) {

         // SOP16 Operations
         const char eClearBuffer = (char)0x2D;
         const char eRestartPrinting = (char)0x5A;

         // Fan out on Sub Operation
         switch ((PrinterOps)mReq.Op) {
            case PrinterOps.SOP16ClearBuffer:
               if (rxClass) {
                  return sSTX + sESC2 + "\x76" + sETX;
               } else {
                  return sSTX + sESC + eClearBuffer + sETX;
               }
            case PrinterOps.SOP16RestartPrinting:
               if (rxClass) {
                  return sSTX + sESC2 + "\x77" + sETX;
               } else {
                  return sSTX + sESC + eRestartPrinting + sETX;
               }
            default:
               return string.Empty;
         }
      }

      string BuildFetchString(HPRequest mReq) {

         // Control Operation Escaped Characters
         const char eGetStatus = (char)0x23; // PX, PXR, RX -- SOP-04
         const char eGetCurrentMessage = (char)0x2F; // PX, PXR, RX -- SOP-04
         const char eGetPreviousMessage = (char)0x2E; // PX, PXR, RX -- SOP-04
         const char eGetCurrentTime = (char)0x7B;

         // Fan out on Sub Operation
         switch ((FetchOps)mReq.SubOp) {
            case FetchOps.Status:
               return sESC + eGetStatus;
            case FetchOps.Time:
               if (rxClass) {
                  return sESC2 + (char)0x75;
               } else {
                  return sESC + eGetCurrentTime;
               }
            case FetchOps.PreviousMessage:
               return sESC + eGetPreviousMessage;
            case FetchOps.Currentmessage:
               return sESC + eGetCurrentMessage;
            default:
               return string.Empty;
         }
      }

      string BuildRetrievePatternString(HPRequest mReq) {

         // cijConnect Retrieve Header
         const char eRetrieveUserData = (char)0xC0;
         string CharSize = Convert.ToString((Convert.ToChar(48 + mReq.CharSize)));
         string Page = Convert.ToString((Convert.ToChar(48 + mReq.Page)));

         // Retrieve Operation Escaped Characters
         const char eRetrieveUserPattern = (char)0xD7;
         const char eRetrieveStandardCharacterPattern = (char)0xD8;

         // Fan out on Sub Operation
         switch ((RetrievePatternOps)mReq.SubOp) {
            case RetrievePatternOps.Standard: // 0
               if (rxClass) {
                  return sESC2 + "\x50\x46" + mReq.KbType.ToString() + CharSize + Page;
               } else {
                  return sESC + eRetrieveUserData + eRetrieveStandardCharacterPattern + CharSize + Page;
               }
            case RetrievePatternOps.User: // 1
               if (rxClass) {
                  return sESC2 + "\x50\x44" + CharSize + Page;
               } else {
                  return sESC + eRetrieveUserData + eRetrieveUserPattern + CharSize + Page;
               }
            default:
               return string.Empty;
         }
      }

      string BuildRetrieveString(HPRequest mReq) {

         // cijConnect Retrieve Header
         const char eRetrieveUserData = (char)0xC0;
         const char eRetrieveDistributorData = (char)0xD0;

         // Retrieve Operation Escaped Characters
         const char eRetrieveLineSetting = (char)0xC1;
         const char eRetrievePrintContentsAttributes = (char)0xC2;
         const char eRetrievePrintContentsNoAttributes = (char)0xC3;
         const char eRetrieveCalendarCondition = (char)0xC4;
         const char eRetrieveSubstitutionRule = (char)0xC5;
         const char eRetrieveCountCondition = (char)0xC6;
         const char eRetrievePrintFormatSetting = (char)0xC7;
         const char eRetrievePrintSpecificationSetting = (char)0xC8;
         const char eRetrievePrintData = (char)0xC9;
         const char eRetrieveUserEnvironmentSetup = (char)0xCA;
         const char eRetrieveDateTimeSetup = (char)0xCB;
         const char eRetrieveCommunicationsSetup = (char)0xCC;
         const char eRetrieveTouchScreenSetup = (char)0xCD;
         const char eRetrieveOperationManagement = (char)0xCE;
         const char eRetrieveAlarmHistory = (char)0xCF;
         const char eRetrievePartsUsageTime = (char)0xD1;
         const char eRetrieveSoftwareVersion = (char)0xD2;
         const char eRetrieveStirrerTest = (char)0xD3;
         const char eRetrieveMonthSubstituteRule = (char)0xD4;
         const char eRetrieveShiftCodeSetup = (char)0xD5;
         const char eRetrieveTimeCountCondition = (char)0xD6;
         const char eRetrieveUnitInformation = (char)0xD1;
         const char eRetrieveViscometerCalibration = (char)0xD2;
         const char eRetrieveSystemEnvironmentSetup = (char)0xD3;
         const char eRetrieveAdjustmentOperationalCheckout = (char)0xD4;
         const char eRetrieveSolenoidValvePumpTest = (char)0xD5;

         if (rxClass) {
            switch ((RetrieveOps)mReq.SubOp) {
               case RetrieveOps.LineSetting:
                  return "\x1f\x50\x31";
               case RetrieveOps.PrintContentsAttributes:
                  return "\x1f\x50\x32";
               case RetrieveOps.PrintContentsNoAttributes:
                  return "\x1f\x50\x33";
               case RetrieveOps.CalendarCondition:
                  return "\x1f\x50\x34";
               case RetrieveOps.SubstitutionRule:
                  return "\x1f\x50\x35";
               case RetrieveOps.SubstitutionRuleData:
                  return "\x1f\x50\x36";
               case RetrieveOps.ShiftCodeSetup:
                  return "\x1f\x50\x37";
               case RetrieveOps.TimeCountCondition:
                  return "\x1f\x50\x38";
               case RetrieveOps.CountCondition:
                  return "\x1f\x50\x39";
               case RetrieveOps.PrintFormat:
                  return "\x1f\x50\x3a";
               case RetrieveOps.AdjustICS:
                  return "\x1f\x50\x3b";
               case RetrieveOps.PrintSpecifications:
                  return "\x1f\x50\x3c";
               case RetrieveOps.VariousPrintSetup:
                  return "\x1f\x50\x3d";
               case RetrieveOps.MessageGroupNames:
                  return "\x1f\x50\x3e";
               case RetrieveOps.PrintData:
                  return "\x1f\x50\x3f";
               case RetrieveOps.UserEnvironmentSetup:
                  return "\x1f\x50\x40";
               case RetrieveOps.DateTimeSetup:
                  return "\x1f\x50\x41";
               case RetrieveOps.CommunicationsSetup:
                  return "\x1f\x50\x42";
               case RetrieveOps.TouchScreenSetup:
                  return "\x1f\x50\x43";
               case RetrieveOps.UnitInformation:
                  return "\x1f\x50\x47";
               case RetrieveOps.OperationManagement:
                  return "\x1f\x50\x48";
               case RetrieveOps.AlarmHistory:
                  return "\x1f\x50\x49";
               case RetrieveOps.PartsUsageTime:
                  return "\x1f\x50\x4a";
               case RetrieveOps.CirculationSystemSetup:
                  return "\x1f\x50\x4b";
               case RetrieveOps.SoftwareVersion:
                  return "\x1f\x50\x4c";
               case RetrieveOps.AdjustmentOperationalCheckout:
                  return "\x1f\x50\x4d";
               case RetrieveOps.SolenoidValvePumpTest:
                  return "\x1f\x50\x4e";
               case RetrieveOps.FreeLayoutCoordinates:
                  return "\x1f\x50\x50";
               default:
                  return string.Empty;
            }
         } else {
            switch ((RetrieveOps)mReq.SubOp) {
               case RetrieveOps.LineSetting: // 0
                  return sESC + eRetrieveUserData + eRetrieveLineSetting;
               case RetrieveOps.PrintContentsAttributes: // 1
                  return sESC + eRetrieveUserData + eRetrievePrintContentsAttributes;
               case RetrieveOps.PrintContentsNoAttributes: // 2
                  return sESC + eRetrieveUserData + eRetrievePrintContentsNoAttributes;
               case RetrieveOps.CalendarCondition: // 3
                  return sESC + eRetrieveUserData + eRetrieveCalendarCondition;
               case RetrieveOps.SubstitutionRule: // 4
                  return sESC + eRetrieveUserData + eRetrieveSubstitutionRule;
               case RetrieveOps.CountCondition: // 5
                  return sESC + eRetrieveUserData + eRetrieveCountCondition;
               case RetrieveOps.PrintFormat: // 6
                  return sESC + eRetrieveUserData + eRetrievePrintFormatSetting;
               case RetrieveOps.PrintSpecifications: // 7
                  return sESC + eRetrieveUserData + eRetrievePrintSpecificationSetting;
               case RetrieveOps.PrintData: // 8
                  return sESC + eRetrieveUserData + eRetrievePrintData;
               case RetrieveOps.UserEnvironmentSetup: // 9
                  return sESC + eRetrieveUserData + eRetrieveUserEnvironmentSetup;
               case RetrieveOps.DateTimeSetup: // 10
                  return sESC + eRetrieveUserData + eRetrieveDateTimeSetup;
               case RetrieveOps.CommunicationsSetup: // 11
                  return sESC + eRetrieveUserData + eRetrieveCommunicationsSetup;
               case RetrieveOps.TouchScreenSetup: // 12
                  return sESC + eRetrieveUserData + eRetrieveTouchScreenSetup;
               case RetrieveOps.OperationManagement: // 13
                  return sESC + eRetrieveUserData + eRetrieveOperationManagement;
               case RetrieveOps.AlarmHistory: // 14
                  return sESC + eRetrieveUserData + eRetrieveAlarmHistory;
               case RetrieveOps.PartsUsageTime: // 15
                  return sESC + eRetrieveUserData + eRetrievePartsUsageTime;
               case RetrieveOps.SoftwareVersion: // 16
                  return sESC + eRetrieveUserData + eRetrieveSoftwareVersion;
               case RetrieveOps.StirrerTest: //= 17
                  return sESC + eRetrieveUserData + eRetrieveStirrerTest;
               case RetrieveOps.MonthSubstituteRule: // 18
                  return sESC + eRetrieveUserData + eRetrieveMonthSubstituteRule;
               case RetrieveOps.ShiftCodeSetup: // 19
                  return sESC + eRetrieveUserData + eRetrieveShiftCodeSetup;
               case RetrieveOps.TimeCountCondition: // 20
                  return sESC + eRetrieveUserData + eRetrieveTimeCountCondition;
               case RetrieveOps.UnitInformation: // 21
                  return sESC + eRetrieveDistributorData + eRetrieveUnitInformation;
               case RetrieveOps.ViscometerCalibration: // 22
                  return sESC + eRetrieveDistributorData + eRetrieveViscometerCalibration;
               case RetrieveOps.SystemEnvironmentSetup: // 23
                  return sESC + eRetrieveDistributorData + eRetrieveSystemEnvironmentSetup;
               case RetrieveOps.AdjustmentOperationalCheckout: // 24
                  return sESC + eRetrieveDistributorData + eRetrieveAdjustmentOperationalCheckout;
               case RetrieveOps.SolenoidValvePumpTest: // 25
                  return sESC + eRetrieveDistributorData + eRetrieveSolenoidValvePumpTest;
               default:
                  return string.Empty;
            }
         }
      }

      string BuildSetClockString(HPRequest mReq) {
         if (rxClass) {
            return sSTX + sESC2 + "\x2e" + (char)('1' + mReq.SubOp) + mReq.Data1 + sETX;
         } else {
            return sSTX + sESC + (char)('\x72' + mReq.SubOp) + mReq.Data1 + sETX;
         }
      }

      string BuildWritePatternString(HPRequest mReq) {
         string result = "";
         string regNumber;
         int maxLength;
         HPRequest mTemp = mReq;
         // 150 is the lenght of the largest character 
         if (rxClass) {
            maxLength = 3000 - 150;
         } else {
            maxLength = 1500 - 150;
         }
         do {
            if (mTemp.SubOp < 192) {
               regNumber = "" + (char)0xF1 + (char)(mTemp.SubOp + 0x40);
            } else {
               regNumber = "" + (char)0xF2 + (char)(mTemp.SubOp - 192 + 0x20);
            }

            // Put into wrapper
            if (rxClass) {
               result += sESC2 + (char)0x32 + mTemp.Data1 + regNumber + mTemp.Data2;
            } else {
               result += sESC + (char)0x20 + mTemp.Data1 + regNumber + mTemp.Data2;
            }
            if (!Equals(mTemp, mReq)) {
               MoveToIdleQueue(mTemp);
            }
            if (HP_Requests.Count == 1) {
               break;
            }
            mTemp = HP_Requests[1];
            if (!MergeRequests || mTemp.Op != PrinterOps.WritePattern) {
               break;
            }
         } while (result.Length < maxLength);
         // Put into wrapper
         if (rxClass) {
            return sSTX + result + sETX;
         } else {
            return sSTX + result + sETX;
         }
      }

      string BuildCalendarOffset(HPRequest mReq) {
         // Local Storage
         string strStart;
         string strResult;

         if (rxClass) {
            strStart = "\x1f\x28\x32" + (char)(mReq.BlockNo + 0x30);
         } else {
            strStart = sESC + (char)0x76 + (char)(mReq.Item + 0x30);
         }
         strResult = strStart + "0" + mReq.Data1 // Year
                   + strStart + "1" + mReq.Data2 // Month
                   + strStart + "2" + mReq.Data3 // Day
                   + strStart + "3" + mReq.Data4 // Hour
                   + strStart + "4" + mReq.Data5; // Minute

         // Complete the string
         return sSTX + strResult + sETX;
      }

      string BuildCalendarSubRule(HPRequest mReq) {
         if (rxClass && SubstitutionRules) {
            return "\x02\x1f\x28\x31" + (char)(mReq.BlockNo + 0x30) + mReq.Data6 + "\x03";
         }
         return "";
      }

      string BuildMessageString(HPRequest mReq) {

         string eMessageSave;
         string eMessageRecall;
         string eMessageName;
         string messageNumber;

         if (rxClass) {
            messageNumber = mReq.Item.ToString("0000");
            eMessageRecall = sESC2 + (char)0x20 + (char)0x31;
            eMessageSave = sESC2 + (char)0x21 + (char)0x31;
            eMessageName = sESC2 + (char)0x21 + (char)0x32;
         } else {
            messageNumber = mReq.Item.ToString("000");
            eMessageSave = sESC + (char)0x55;
            eMessageRecall = sESC + (char)0x56;
            eMessageName = sESC + (char)0x86;
         }
         if (printerType == HitachiPrinterType.PX || mReq.Data1.Length == 0) {
            eMessageName = string.Empty;
         } else {
            eMessageName += mReq.Data1;
         }

         // Fan out on Sub Operation
         switch ((MessageOps)mReq.SubOp) {
            case MessageOps.MessageSave:
               return sSTX + eMessageSave + messageNumber + eMessageName + sETX;
            case MessageOps.MessageRestore:
               return sSTX + eMessageRecall + messageNumber + sETX;
            default:
               return string.Empty;
         }
      }

      // Count Conditions
      enum Count80 {
         eCtInitialValue = 0x30,
         eCtRange1 = 0x31,
         eCtRange2 = 0x32,
         eCtJumpFrom = 0x33,
         eCtJumpTo = 0x34,
         eCtReset = 0x35
      }

      // Count Conditions
      enum Count81 {
         eCtUpdateInProgress = 0x30,
         eCtUpdateUnit = 0x31
      }

      // Count Conditions
      enum Count82 {
         eCtDirection = 0x30,
         eCtExternalSignal = 0x31,
         eCtResetSignal = 0x32
      }

      string BuildCountConditionString(HPRequest mReq) {
         string result = "";
         if (rxClass) {
            if (mReq.Data4.Length == 0) {
               result = sSTX +
                  sESC2 + Chr(0x2C) + Chr(0x32) + Chr(0x30 + mReq.BlockNo) + mReq.Data2 +  // Range 1
                  sESC2 + Chr(0x2C) + Chr(0x33) + Chr(0x30 + mReq.BlockNo) + mReq.Data3 +  // Range 2
                  sESC2 + Chr(0x2C) + Chr(0x31) + Chr(0x30 + mReq.BlockNo) + mReq.Data1 +  // Initial Value
                  sESC2 + Chr(0x2C) + Chr(0x34) + Chr(0x30 + mReq.BlockNo) + mReq.Data7 +  // Update IP
                  sESC2 + Chr(0x2C) + Chr(0x35) + Chr(0x30 + mReq.BlockNo) + mReq.Data8 +  // Update Unit
                  sESC2 + Chr(0x2C) + Chr(0x37) + Chr(0x30 + mReq.BlockNo) + mReq.Data9 +  // Direction
                  sESC2 + Chr(0x2C) + Chr(0x36) + Chr(0x30 + mReq.BlockNo) + mReq.Data12 + // Increment
                  sETX;
            } else {
               result = sSTX +
                  sESC2 + Chr(0x2C) + Chr(0x32) + Chr(0x30 + mReq.BlockNo) + mReq.Data2 +  // Range 1
                  sESC2 + Chr(0x2C) + Chr(0x33) + Chr(0x30 + mReq.BlockNo) + mReq.Data3 +  // Range 2
                  sESC2 + Chr(0x2C) + Chr(0x38) + Chr(0x30 + mReq.BlockNo) + mReq.Data4 +  // Jump From
                  sESC2 + Chr(0x2C) + Chr(0x39) + Chr(0x30 + mReq.BlockNo) + mReq.Data5 +  // Jump to
                  sESC2 + Chr(0x2C) + Chr(0x31) + Chr(0x30 + mReq.BlockNo) + mReq.Data1 +  // Initial Value
                  sESC2 + Chr(0x2C) + Chr(0x3A) + Chr(0x30 + mReq.BlockNo) + mReq.Data6 +  // Reset
                  sESC2 + Chr(0x2C) + Chr(0x34) + Chr(0x30 + mReq.BlockNo) + mReq.Data7 +  // Update IP
                  sESC2 + Chr(0x2C) + Chr(0x35) + Chr(0x30 + mReq.BlockNo) + mReq.Data8 +  // Update Unit
                  sESC2 + Chr(0x2C) + Chr(0x37) + Chr(0x30 + mReq.BlockNo) + mReq.Data9 +  // Direction
                  sESC2 + Chr(0x2C) + Chr(0x36) + Chr(0x30 + mReq.BlockNo) + mReq.Data12 + // Increment
                  sETX;
            }
         } else {
            // Local Storage
            string strStart80;
            string strStart81;
            string strStart82;
            string strStart83;

            // Form item number
            strStart80 = sESC + Chr(0x80) + Chr(mReq.Item + '0');
            strStart81 = sESC + Chr(0x81) + Chr(mReq.Item + '0');
            strStart82 = sESC + Chr(0x82) + Chr(mReq.Item + '0');
            strStart83 = sESC + Chr(0x83) + Chr(mReq.Item + '0');

            // Build all conditions in a single write
            if (mReq.Data4.Length == 0) {
               result = sSTX +
                   strStart80 + Chr(Count80.eCtRange1) + mReq.Data2 +
                   strStart80 + Chr(Count80.eCtRange2) + mReq.Data3 +
                   strStart80 + Chr(Count80.eCtInitialValue) + mReq.Data1 +
                   strStart81 + Chr(Count81.eCtUpdateInProgress) + mReq.Data7 +
                   strStart81 + Chr(Count81.eCtUpdateUnit) + mReq.Data8 +
                   strStart82 + Chr(Count82.eCtDirection) + mReq.Data9 +
                   strStart83 + mReq.Data12 +
                   sETX;
            } else {
               result = sSTX +
                   strStart80 + Chr(Count80.eCtRange1) + mReq.Data2 +
                   strStart80 + Chr(Count80.eCtRange2) + mReq.Data3 +
                   strStart80 + Chr(Count80.eCtJumpFrom) + mReq.Data4 +
                   strStart80 + Chr(Count80.eCtJumpTo) + mReq.Data5 +
                   strStart80 + Chr(Count80.eCtInitialValue) + mReq.Data1 +
                   strStart80 + Chr(Count80.eCtReset) + mReq.Data6 +
                   strStart81 + Chr(Count81.eCtUpdateInProgress) + mReq.Data7 +
                   strStart81 + Chr(Count81.eCtUpdateUnit) + mReq.Data8 +
                   strStart82 + Chr(Count82.eCtDirection) + mReq.Data9 +
                   strStart83 + mReq.Data12 +
                   sETX;
            }
         }
         return result;
      }

      #endregion

      #region Connect/Disconnect routines

      // The state of connection
      ConnectionStates connectionState;
      bool PendingENQ = false;

      void ConnectToPXR(HPRequest mReq) {

         // see if any errors occur
         try {

            // Reset the I/O queue
            PendingENQ = false;

            nACKs = 0;
            nNAKs = 0;

            // What type of connection
            switch (Connection) {
               case ConnectionType.EthernetToSerial:
                  // New connection needed?
                  if (connectionState == ConnectionStates.Closed) {
                     //Give network time to respond
                     TimedDelay(10000);
                     PXROperationInProgress = true;
                     connectionState = ConnectionStates.Connecting;
                     soxPXR = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                     soxPXR.BeginConnect(IPAddress, IPPort, Sox_Connect, soxPXR);
                  }
                  break;
               case ConnectionType.Serial:

                  // Set up the port and open it
                  if (string.IsNullOrEmpty(PortName))
                     PortName = "COM1";
                  comPXR = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits) {
                     Encoding = encode
                  };
                  comPXR.DataReceived += new SerialDataReceivedEventHandler(PXR_DataReceived);
                  comPXR.Open();

                  // Set as connected before generating the event
                  connectionState = ConnectionStates.Connected;

                  // Signaling successful Serial Port Connection
                  BuildStatus(StateChange.Connected);

                  // Let the user know
                  Complete?.Invoke(this, new HPEventArgs(mReq.Op, mReq.SubOp, sACK));

                  // Throw away the request and issue the next request
                  CompleteOperation(mReq, string.Empty);
                  PXROperationInProgress = false;
                  IssueOperation();
                  break;
               case ConnectionType.Simulator:
               case ConnectionType.OffLine:

                  // Set as connected before generating the event
                  connectionState = ConnectionStates.Connected;

                  // Signaling successful simulator connection
                  BuildStatus(StateChange.Connected);

                  // Let the user know
                  Complete?.Invoke(this, new HPEventArgs(mReq.Op, sACK));

                  // Throw away the request and issue next request
                  CompleteOperation(mReq, string.Empty);
                  PXROperationInProgress = false;
                  IssueOperation();
                  break;
               default:
                  // Signaling failed connection attempt
                  BuildStatus(StateChange.ConnectFailed);

                  // Let the user know
                  Complete?.Invoke(this, new HPEventArgs(mReq.Op, sNAK));

                  // Throw away the request and issue next request
                  CompleteOperation(mReq, string.Empty);
                  PXROperationInProgress = false;
                  IssueOperation();
                  break;
            }
         } catch (Exception e) {

            // Tell caller about error
            NotifyClient?.Invoke(this, new HPEventArgs(mReq.Op, "Connect Error/" + e.Message));
            // Set as not connected before generating the event
            connectionState = ConnectionStates.Closed;

            // Signaling failed connection attempt
            BuildStatus(StateChange.ConnectFailed);

            // Let the user know
            Complete?.Invoke(this, new HPEventArgs(mReq.Op, sNAK));

            // Throw away the request and issue next request
            CompleteOperation(mReq, string.Empty);
            PXROperationInProgress = false;
            IssueOperation();
         }
      }

      void DisconnectFromPXR(bool blnTerminating) {

         // See if any errors occur
         try {

            // No longer connected
            connectionState = ConnectionStates.Closed;

            // Signaling disconnect
            this.BuildStatus(StateChange.Disconnected);

            // What type of connection?
            switch (Connection) {
               case ConnectionType.EthernetToSerial:

                  // Close the socket
                  if (soxPXR.Connected) {
                     soxPXR.Shutdown(SocketShutdown.Both);
                     soxPXR.Disconnect(false);
                  }
                  soxPXR.Close();
                  soxPXR.Dispose();
                  soxPXR = null;
                  break;
               case ConnectionType.Serial:

                  // Port Close required?
                  if (comPXR.IsOpen) {

                     // Disallow input and close port
                     comPXR.Close();
                  }
                  comPXR = null;
                  break;
               default:
                  break;
            }

            // Do not report if terminating
            if (!blnTerminating) {
               Complete?.Invoke(this, new HPEventArgs(PrinterOps.Disconnect, sACK));
            }
         } catch {

            // Do not report if terminating
            if (!blnTerminating) {
               Complete?.Invoke(this, new HPEventArgs(PrinterOps.Disconnect, sNAK));
            }
         }
      }

      #endregion

      #region Callback Routines

      void Sox_Connect(IAsyncResult ar) {
         if(parent.InvokeRequired) {
            parent.BeginInvoke(new EventHandler(delegate { Sox_Connect(ar); }));
            return;
         }
         try {
            // Local storage
            Socket client = (Socket)ar.AsyncState;

            // Is there a Send Data operation in progress, Complete the operation
            if (PXROperationInProgress) {

               // Get the outstanding request
               Debug.Assert(HP_Requests.Count > 0, "PXR Operation In Progress but no request available!");
               HPRequest mReq = HP_Requests[0];

               // Was the connection good?
               if (mReq.Op == PrinterOps.Connect && soxPXR.Connected) {

                  // Signaling successful ethernet connection
                  BuildStatus(StateChange.Connected);

                  // Complete the request, Mark Success
                  connectionState = ConnectionStates.Connected;
                  CompleteOperation(mReq, $"OK to IP {IPAddress}({IPPort})!");

                  // Create a listener callback 
                  StateObject state = new StateObject {
                     workSocket = client
                  };
                  client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(Sox_DataArrival), state);

               } else {

                  // Signaling failed ethernet connection
                  BuildStatus(StateChange.ConnectFailed);

                  // Complete the request, Mark Failure
                  connectionState = ConnectionStates.Closed;
                  CompleteOperation(mReq, "Failed to IP " + IPAddress + "(" + IPPort + ")!");
               }

               // The request is now complete, issue the next commamd
               PXROperationInProgress = false;
               TimedDelay(50);

            }

            // Finish up the connection
            if (client.Connected) {
               client.EndConnect(ar);
            }
         } catch (Exception e) {
            Log?.Invoke(this, new HPEventArgs(PrinterOps.Nop, e.ToString()));
         }
      }

      void Sox_DataArrival(IAsyncResult ar) {
         try {

            // can get here if socket closes
            if (connectionState == ConnectionStates.Closed)
               return;
            if (!soxPXR.Connected) {
               connectionState = ConnectionStates.Closed;
               Log?.Invoke(this, new HPEventArgs("Connection Dropped!"));
               return;
            }

            StateObject state;
            Socket client;

            // Retrieve the state object and the client socket from the asynchronous state object.
            state = (StateObject)ar.AsyncState;
            client = state.workSocket;

            // Read data from the remote device.
            int bytesRead = client.EndReceive(ar);
            if (bytesRead == 0)
               return;
            lock (PartialLock) {
               Array.Copy(state.buffer, 0, Partial, PartialLength, bytesRead);
               PartialLength += bytesRead;
            }
            // Begin receiving the data from the remote device.
            ParseInput();
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(Sox_DataArrival), state);
         } catch (SocketException e) {
            if (Log != null) {
               parent.BeginInvoke(new EventHandler(delegate { Log(this, new HPEventArgs(PrinterOps.Nop, e.ToString())); }));
            }
         }
      }

      void PXR_DataReceived(object Sender, EventArgs e) {
         int n = comPXR.BytesToRead;
         lock (PartialLock) {
            comPXR.Read(Partial, PartialLength, n);
            PartialLength += n;
         }
         ParseInput();
      }

      #endregion

      #region Input Processing routines

      void ParseInput() {
         try {
            if (parent.InvokeRequired) {
               parent.BeginInvoke(new EventHandler(delegate { ParseInput(); }));
               return;
            }
            string s;
            int n;
            while (PartialLength > 0) {
               switch ((char)Partial[0]) {
                  case cACK:
                  case cNAK:
                     ProcessACK_NAK(Extract(1));
                     break;
                  case cENQ:
                     ReceiveENQ(Extract(1));
                     break;
                  case cSTX:
                     if (PartialLength >= RcvLength) {
                        n = Math.Max(RcvLength - 1, 0);
                        n = Array.IndexOf(Partial, (byte)cETX, n, PartialLength - n);
                        if (n >= 0) {
                           ProcessSTX(Extract(n + 1));
                        } else
                           return;
                     } else
                        return;
                     break;
                  default:
                     s = Extract(PartialLength);
                     Log?.Invoke(this, new HPEventArgs("Partial= " + s));
                     break;
               }
            }
            if (!PXROperationInProgress) {
               TimedDelay(0);
            }
         } catch {
            // Avoid error on shutdown
         }
      }

      void ProcessACK_NAK(string strIn) {

         // Local storage
         HPRequest mReg;
         PrinterOps Op;
         int SubOp;
         int Item;
         string Font;
         int CharSize;
         int Page;
         int KbType;
         string strStatus;
         bool fakeStatus = false;

         if (strIn[0] == cACK) {
            nACKs++;
            BuildStatus(StateChange.UpdateACKNAK);
         }
         if (strIn[0] == cNAK) {
            nNAKs++;
            BuildStatus(StateChange.UpdateACKNAK);
         }

         // Was printer wanting to talk?
         if (PendingENQ) {
            ProcessENQ(strIn);
            return;
         }

         // Is there an operation in progress?
         if (PXROperationInProgress) {

            // Get the requesting packet
            Debug.Assert(HP_Requests.Count > 0, "PXR Operation In Progress but no request available!");
            mReg = HP_Requests[0];

            // Fanout on the request type
            Op = mReg.Op;
            SubOp = mReg.SubOp;
            Item = mReg.Item;
            switch (Op) {

               // Connect = 1, Disconnect = 2, Idle = 17, SetNozzle = 31 (Cannot get here)
               case PrinterOps.IssueControl: // 3

                  // Assume no status to return
                  strStatus = string.Empty;

                  // If SOP4 is not on, no status will be returned.  Fake one
                  if (!SOP4Enabled) {
                     switch ((ControlOps)SubOp) {
                        case ControlOps.ComOn:
                           strStatus = sSTX + "11120" + sETX;
                           fakeStatus = true;
                           break;
                        case ControlOps.ComOff:
                           strStatus = sSTX + "10120" + sETX;
                           fakeStatus = true;
                           break;
                        default:
                           break;
                     }
                     if (strStatus != string.Empty) {
                        // Signaling Fake SOP-04 status
                        BuildStatus(strStatus);
                     }
                  }

                  // Complete the request
                  CompleteOperation(mReg, strIn);
                  Complete?.Invoke(this, new HPEventArgs(Op, SubOp, strIn));

                  // Let the operator know
                  ProcessUnsolicited(strStatus, fakeStatus);
                  break;
               case PrinterOps.ColumnSetup: // 4
               case PrinterOps.WriteSpecification: // 5
               case PrinterOps.WriteText: // 7
               case PrinterOps.WriteCalendarOffset: // 8
               case PrinterOps.WriteCalendarSubZS: // 9
               case PrinterOps.WriteCountCondition: // 10
               case PrinterOps.Message: // 12
               case PrinterOps.Fetch: // 13
               case PrinterOps.Retrieve: // 14
               case PrinterOps.SetClock: // 16
               case PrinterOps.PassThru: // 18
               case PrinterOps.SOP16ClearBuffer: // 20
               case PrinterOps.SOP16RestartPrinting: // 21
               case PrinterOps.ChangeInkDropRule: // 22
               case PrinterOps.ChangeMessageFormat: // 23
               case PrinterOps.PositionItem: // 24
               case PrinterOps.WriteCalendarZS: // 25
               case PrinterOps.WriteCalendarSub: // 26
               case PrinterOps.WriteCalendarSubRule: // 27
                  CompleteOperation(mReg, strIn);
                  Complete?.Invoke(this, new HPEventArgs(Op, SubOp, strIn));
                  break;
               case PrinterOps.WriteFormat: // 6
               case PrinterOps.WritePattern: // 11

                  // Complete the request
                  CompleteOperation(mReg, strIn);
                  Complete?.Invoke(this, new HPEventArgs(Op, SubOp, Item, strIn));
                  break;
               case PrinterOps.RetrievePattern: // 15

                  // Save the response data so CompleteOperation does not clear it
                  Font = mReg.Data1;
                  CharSize = mReg.CharSize;
                  Page = mReg.Page;
                  KbType = mReg.KbType;

                  // Complete the request
                  CompleteOperation(mReg, strIn);
                  Complete?.Invoke(this, new HPEventArgs(Op, SubOp, 0, Font, CharSize, Page, KbType, strIn));
                  break;
               case PrinterOps.ENQ: // 19

                  // Did we get a positive response?
                  if (strIn == sACK | mReg.Retries > 10) {

                     // Complete the request
                     CompleteOperation(mReg, strIn);
                  } else {
                     mReg.Retries += 1;
                     TimedDelay(50); // intDelay;
                  }
                  break;
               case PrinterOps.Nop: // 0

                  // The operations queue has been corrupted, reset it
                  ResetPXRQueue();
                  break;
               default:
                  ProcessUnsolicited(strIn);
                  break;
            }
            TimedDelay(10);
            PXROperationInProgress = false;
         } else {
            ProcessUnsolicited(strIn);
         }
      }

      void ReceiveENQ(string strIn) {
         HPRequest mReq;
         //
         // Tell the user it came in
         if ((EventLogging & HPEventLogging.Input) > 0 && Log != null) {
            Log(this, new HPEventArgs("InputPE = " + strIn));
         }
         //
         // The printer sends ENQ on Com Off to report status but goes silent before allowing a response
         if (PXROperationInProgress) {
            Debug.Assert(HP_Requests.Count > 0, "PXR Operation In Progress but no request available!");
            mReq = HP_Requests[0];
            if (mReq.Op == PrinterOps.IssueControl && (ControlOps)mReq.SubOp == ControlOps.ComOff) {
               if (Unsolicited != null) {
                  string raw = sSTX + "1023B" + sETX;
                  //
                  // Let the user know it went away
                  Unsolicited(this, new HPEventArgs(raw));
                  //
                  // Report the raw data
                  ReportRawData(true, raw);
                  //
                  // No need to acknowledge it as the printer is no longer talking
                  return;
               }
            }
         }
         //
         // make sure that we wait for the response
         PendingENQ = true;
         //
         // Report the raw data
         ReportRawData(false, sACK);
         //
         // Acknowledge the ENQ request
         switch (Connection) {
            case ConnectionType.Serial:
               // Issue the output
               comPXR.Write(new byte[] { (byte)cACK }, 0, 1);
               break;
            case ConnectionType.EthernetToSerial:
               // Issue the output
               soxPXR.Send(new byte[] { (byte)cACK }, 1, SocketFlags.None);
               break;
            case ConnectionType.Simulator:
               break;
            default:
               break;
         }
         // Log output of data
         if ((EventLogging & HPEventLogging.Output) > 0 && Log != null) {
            Log(this, new HPEventArgs("Output = " + sACK));
         }

      }

      void ProcessSTX(string strIn) {

         // Local storage
         string strFont;
         int intCharSize;
         int intPage;
         int KbType;
         HPRequest mReq;
         PrinterOps Op;
         int SubOp;

         // Was printer wanting to talk?
         if (PendingENQ) {
            ProcessENQ(strIn);
            return;
         }

         // Got status pending?
         if (PXROperationInProgress) {

            // Get the requesting packet
            Debug.Assert(HP_Requests.Count > 0, "PXR Operation In Progress but no request available!");
            mReq = HP_Requests[0];
            Op = mReq.Op;
            SubOp = mReq.SubOp;

            // Treat status as unsolicited unless it was really solicited
            if (IsStatus(strIn)) {
               if (Op != PrinterOps.Fetch || (FetchOps)SubOp != FetchOps.Status) {
                  BuildStatus(strIn);
                  ProcessUnsolicited(strIn);
                  return;
               }
            }

            // Operation Specific
            switch (Op) {

               // Complete a fetch
               case PrinterOps.Fetch:
                  if ((FetchOps)SubOp == FetchOps.Status) {
                     if (IsStatus(strIn)) {
                        // Signaling real SOP-04 status
                        BuildStatus(strIn);
                     }
                  }
                  CompleteOperation(mReq, strIn);
                  Complete?.Invoke(this, new HPEventArgs(Op, SubOp, strIn));
                  break;
               case PrinterOps.Retrieve:
                  if ((RetrieveOps)SubOp == RetrieveOps.PrintContentsNoAttributes) {
                     LastMessageText = strIn;
                     StatusChanged?.Invoke(this, statusArea);
                  }
                  CompleteOperation(mReq, strIn);
                  Complete?.Invoke(this, new HPEventArgs(Op, SubOp, strIn));
                  break;
               case PrinterOps.PassThru:
                  if (mReq.ExpectTextResponse) {
                     CompleteOperation(mReq, strIn);
                     Complete?.Invoke(this, new HPEventArgs(Op, SubOp, strIn));
                  } else {
                     ProcessUnsolicited(strIn);
                     // Need to avoid clearing PXROperationInProgress
                     return;
                  }
                  break;
               case PrinterOps.RetrievePattern:
                  // Save the response data so CompleteOperation does not clear it
                  strFont = mReq.Data1;
                  intCharSize = mReq.CharSize;
                  intPage = mReq.Page;
                  KbType = mReq.KbType;

                  // Complete a request
                  CompleteOperation(mReq, strIn);
                  Complete?.Invoke(this, new HPEventArgs(Op, SubOp, 0, strFont, intCharSize, intPage, KbType, strIn));
                  break;
               default:
                  ProcessUnsolicited(strIn);
                  // Need to avoid clearing PXROperationInProgress
                  return;
            }

            // Delay to allow printer time to digest last request
            TimedDelay(50);

            // Mark printer idle
            PXROperationInProgress = false;
         } else {
            ProcessUnsolicited(strIn);
         }
      }

      void ProcessENQ(string strIn) {

         // Allow traffic again
         PendingENQ = false;

         // ENQ Input is always unsolicited
         ProcessUnsolicited(strIn);

         // If there was an operation in progress, restart the clock
         if (PXROperationInProgress) {
            TimedDelay(2000); // intWait
         }
      }

      void ProcessReceive(string strIn) {

         // Log data completion (* for unsolicited)
         if (PXROperationInProgress) {
            if ((EventLogging & HPEventLogging.Input) > 0) {
               Log?.Invoke(this, new HPEventArgs("Input  = " + strIn));
            }
         } else {
            if ((EventLogging & HPEventLogging.Unsolicited) > 0) {
               Log?.Invoke(this, new HPEventArgs("InputPR= " + strIn));
            }
         }

         // Is there an ENQ Pending?
         if (PendingENQ) {
            ProcessENQ(strIn);
         } else {

            // Fan out on the character
            switch (strIn[0]) {

               // Start of text, Process an <cSTX>
               case cSTX:
                  ProcessSTX(strIn);
                  break;

               // Request acknowlegement, Process an <cACK> or <cNAK>
               case cACK:
               case cNAK:
                  ProcessACK_NAK(strIn);
                  break;

               // Just trash, log it and throw it away
               default:
                  ProcessUnsolicited(strIn);
                  break;
            }
         }
      }

      void ProcessUnsolicited(string strIn, bool fakeStatus = false) {
         // Some calls are for null string
         if (!string.IsNullOrEmpty(strIn)) {
            // Acknowledge returned if text or ENQ
            if (!fakeStatus && (strIn[0] == cSTX || strIn[0] == cENQ)) {
               //
               // Acknowledge the ENQ request
               switch (Connection) {
                  case ConnectionType.Serial:
                     // Issue the output
                     if (comPXR != null && comPXR.IsOpen) {
                        comPXR.Write(new byte[] { (byte)cACK }, 0, 1);
                     }
                     break;
                  case ConnectionType.EthernetToSerial:
                     // Issue the output
                     if (soxPXR != null && soxPXR.Connected) {
                        soxPXR.Send(new byte[] { (byte)cACK }, 1, SocketFlags.None);
                     }
                     break;
               }
               //
               // Report the raw data
               ReportRawData(true, strIn);
               // Log output of data
               if ((EventLogging & HPEventLogging.Output) > 0 && Log != null) {
                  Log(this, new HPEventArgs("Output = " + sACK));
               }
               //
               // Report the raw data
               ReportRawData(false, sACK);
            }
            // Notify the user
            Unsolicited?.Invoke(this, new HPEventArgs(strIn));
            // Most unsolicited info is status
            if (IsStatus(strIn)) {
               // Signaling unsolicited SOP-04 status
               BuildStatus(strIn);
            }
         }
      }

      void ReportRawData(bool input, string rawData) {
         if ((EventLogging & HPEventLogging.RawData) > 0 && RawData != null) {
            if (input) {
               RawData(this, new HPEventArgs("Raw Data << " + rawData));
            } else {
               RawData(this, new HPEventArgs("Raw Data >> " + rawData));
            }
         }
      }

      string Extract(int n) {
         string s;
         lock (PartialLock) {
            s = encode.GetString(Partial, 0, n);
            PartialLength -= n;
            if (PartialLength > 0) {
               Array.Copy(Partial, n, Partial, 0, PartialLength);
            }
            PartialTimeoutCount = 0;
         }
         return s;
      }

      string GetFont(string strFont, bool useSizeII = false) {
         string result = strFont;
         if (strFont.Length > 1) {
            if (FTs == null) {
               BuildFonts();
            }
            int i = FTs.FindIndex(x => x.Name == strFont.ToUpper());
            if (i >= 0) {
               int n = -1;
               if (rxClass) {
                  n = FTs[i].RxId;
               } else {
                  n = FTs[i].PxrId;
               }
               result = Chr(n + '0');
            } else {
               result = "1";
            }
         }
         return result;
      }

      #endregion

      #region Status Processing

      // Status Area
      HPStatus statusArea;

      void BuildStatus(StateChange newState) {
         if (Connection == ConnectionType.OffLine) {
            statusArea.State = StateChange.OffLine;
            statusArea.SetAllSeverity(Color.LightGray);
         } else {
            statusArea.State = newState;
            switch (newState) {
               case StateChange.Connected:
               case StateChange.Connecting:
                  statusArea.SetAllSeverity(Color.LightGreen);
                  break;
               case StateChange.TimeoutRetrying:
               case StateChange.Initializing:
                  statusArea.SetAllSeverity(Color.Yellow);
                  statusArea.SetAllSeverity(Color.Yellow);
                  break;
               case StateChange.TimeoutAbort:
               case StateChange.ConnectFailed:
                  statusArea.SetAllSeverity(Color.Pink);
                  break;
               case StateChange.UpdateACKNAK:
                  statusArea.SetCounts(nACKs, nNAKs);
                  break;
               case StateChange.Disconnected:
               case StateChange.OffLine:
                  statusArea.SetAllSeverity(Color.LightGray);
                  break;
               default:
                  break;
            }
         }
         StatusChanged?.Invoke(this, statusArea);
      }

      void BuildStatus(string strStatus) {

         // 
         if (IsStatus(strStatus)) {
            statusArea.Response = strStatus;
            // Connection
            statusArea.SetDescription(StatusAreas.Connection, TranslateStatus(StatusAreas.Connection, strStatus[2]));
            switch ((byte)strStatus[2]) {
               case 0x30:
                  statusArea.SetSeverity(StatusAreas.Connection, Color.Yellow);
                  break;
               case 0x31:
                  statusArea.SetSeverity(StatusAreas.Connection, Color.LightGreen);
                  break;
               default:
                  statusArea.SetSeverity(StatusAreas.Connection, Color.Pink);
                  break;
            }

            // Reception
            statusArea.SetDescription(StatusAreas.Reception, TranslateStatus(StatusAreas.Reception, strStatus[3]));
            switch ((byte)strStatus[3]) {
               case 0x30:
                  statusArea.SetSeverity(StatusAreas.Reception, Color.Yellow);
                  break;
               case 0x31:
                  statusArea.SetSeverity(StatusAreas.Reception, Color.LightGreen);
                  break;
               default:
                  if (!SOP4Enabled) {
                     statusArea.SetSeverity(StatusAreas.Reception, Color.Gray);
                  } else {
                     statusArea.SetSeverity(StatusAreas.Reception, Color.Pink);
                  }
                  break;
            }

            // Operation
            statusArea.SetDescription(StatusAreas.Operation, TranslateStatus(StatusAreas.Operation, strStatus[4]));
            switch ((byte)strStatus[4]) {
               case 0x30:
               case 0x31:
                  statusArea.SetSeverity(StatusAreas.Operation, Color.Yellow);
                  break;
               case 0x32:
                  statusArea.SetSeverity(StatusAreas.Operation, Color.LightGreen);
                  break;
               default:
                  if (!SOP4Enabled) {
                     statusArea.SetSeverity(StatusAreas.Operation, Color.Gray);
                  } else {
                     statusArea.SetSeverity(StatusAreas.Operation, Color.Pink);
                  }
                  break;
            }

            // Alarm
            statusArea.SetDescription(StatusAreas.Alarm, TranslateStatus(StatusAreas.Alarm, strStatus[5]));
            switch ((byte)strStatus[5]) {
               case 0x30:
                  statusArea.SetSeverity(StatusAreas.Alarm, Color.LightGreen);
                  break;
               case 0x31:
                  statusArea.SetSeverity(StatusAreas.Alarm, Color.Yellow);
                  break;
               default:
                  if (!SOP4Enabled) {
                     statusArea.SetSeverity(StatusAreas.Alarm, Color.Gray);
                  } else {
                     statusArea.SetSeverity(StatusAreas.Alarm, Color.Pink);
                  }
                  break;
            }

         }
         StatusChanged?.Invoke(this, statusArea);
      }

      string TranslateStatus(StatusAreas Area, char Value) {

         string Result;
         if (!SOP4Enabled) {
            Result = "N/A";
         } else {
            Result = "Unknown(" + Value + ")";
         }
         // Now translate it
         switch (Area) {
            case StatusAreas.Connection:
               switch ((byte)Value) {
                  case 0x30:
                     Result = "Offline";
                     break;
                  case 0x31:
                     Result = "Online";
                     break;
               }
               break;

            case StatusAreas.Reception:
               switch ((byte)Value) {
                  case 0x30:
                     Result = "Reception not possible";
                     break;
                  case 0x31:
                     Result = "Reception possible";
                     break;
               }
               break;

            case StatusAreas.Operation:
               switch ((byte)Value) {
                  case 0x30:
                     Result = "Paused";
                     break;
                  case 0x31:
                     Result = "Running - Not Ready";
                     break;
                  case 0x32:
                     Result = "Ready";
                     break;
                  case 0x49:
                     Result = "Stopping";
                     break;
                  case 0x5c:
                     Result = "Maint. Running";
                     break;
                  case 0x33:
                     Result = "Deflection Voltage Fault";
                     break;
                  case 0x34:
                     Result = "Main Ink Tank Too Full";
                     break;
                  case 0x35:
                     Result = "Blank Print Items";
                     break;
                  case 0x36:
                     Result = "Ink Drop Charge Too Low";
                     break;
                  case 0x37:
                     Result = "Ink Drop Charge Too High";
                     break;
                  case 0x38:
                     Result = "Print Head Cover Open";
                     break;
                  case 0x39:
                     Result = "Target Sensor Fault";
                     break;
                  case 0x3a:
                     Result = "System Operation Error C";
                     break;
                  case 0x3b:
                     Result = "Target Spacing Too Close";
                     break;
                  case 0x3c:
                     Result = "Improper Sensor Position";
                     break;
                  case 0x3d:
                     Result = "System Operation Error M";
                     break;
                  case 0x3e:
                     Result = "Charge Voltage Fault";
                     break;
                  case 0x3f:
                     Result = "Barcode Short On Numbers";
                     break;
                  case 0x41:
                     Result = "Multi DC Power Supply Fan Fault";
                     break;
                  case 0x42:
                     Result = "Deflection Voltage Leakage";
                     break;
                  case 0x43:
                     Result = "Print Overlap Fault";
                     break;
                  case 0x44:
                     Result = "Ink Low Fault";
                     break;
                  case 0x45:
                     Result = "Makeup Ink Low Fault";
                     break;
                  case 0x46:
                     Result = "Print Data Changeover In Progress M";
                     break;
                  case 0x47:
                     Result = "Excessive Format Count";
                     break;
                  case 0x48:
                     Result = "Makeup Ink Replenishment Time-out";
                     break;
                  case 0x4a:
                     Result = "Ink Replenishment Time-out";
                     break;
                  case 0x4b:
                     Result = "No Ink Drop Charge";
                     break;
                  case 0x4c:
                     Result = "Ink Heating Unit Too High";
                     break;
                  case 0x4d:
                     Result = "Ink Heating Unit Temperature Sensor Fault";
                     break;
                  case 0x4e:
                     Result = "Ink Heating Unit Over Current";
                     break;
                  case 0x4f:
                     Result = "Internal Communication Error C";
                     break;
                  case 0x50:
                     Result = "Internal Communication Error M";
                     break;
                  case 0x51:
                     Result = "Internal Communication Error S";
                     break;
                  case 0x52:
                     Result = "System Operation Error S";
                     break;
                  case 0x53:
                     Result = "Memory Fault C";
                     break;
                  case 0x54:
                     Result = "Memory Fault M";
                     break;
                  case 0x55:
                     Result = "Ambient Temperature Sensor Fault";
                     break;
                  case 0x56:
                     Result = "Print Controller Cooling Fan Fault";
                     break;
                  case 0x59:
                     Result = "Print Data Changeover In Progress S";
                     break;
                  case 0x5a:
                     Result = "Print Data Changeover In Progress V";
                     break;
                  case 0x5d:
                     Result = "Memory Fault S";
                     break;
                  case 0x5e:
                     Result = "Pump Motor Fault";
                     break;
                  case 0x5f:
                     Result = "Viscometer Ink Temperature Sensor Fault";
                     break;
                  case 0x60:
                     Result = "External Communication Error";
                     break;
                  case 0x61:
                     Result = "External Signal Error";
                     break;
                  case 0x62:
                     Result = "Memory Fault OP";
                     break;
                  case 0x63:
                     Result = "Ink Heating Unit Temperature Low";
                     break;
                  case 0x64:
                     Result = "Model-key Fault";
                     break;
                  case 0x65:
                     Result = "Language-key Fault";
                     break;
                  case 0x66:
                     Result = "Communication Buffer Fault";
                     break;
                  case 0x67:
                     Result = "Shutdown Fault";
                     break;
                  case 0x68:
                     Result = "Count Overflow";
                     break;
                  case 0x69:
                     Result = "Data changeover timing fault";
                     break;
                  case 0x6a:
                     Result = "Count changeover timing fault";
                     break;
                  case 0x6b:
                     Result = "Print start timing fault";
                     break;
                  case 0x6c:
                     Result = "Ink Shelf Life Information";
                     break;
                  case 0x6d:
                     Result = "Makeup Shelf Life Information";
                     break;
                  case 0x71:
                     Result = "Print Data Changeover Error C";
                     break;
                  case 0x72:
                     Result = "Print Data Changeover Error M";
                     break;
               }
               break;

            case StatusAreas.Alarm:
               switch ((byte)Value) {
                  case 0x30:
                     Result = "No Alarm";
                     break;
                  case 0x31:
                     Result = "Ink Low Warning";
                     break;
                  case 0x32:
                     Result = "Makeup ink Low Warning";
                     break;
                  case 0x33:
                     Result = "Ink Shelf Life Exceeded";
                     break;
                  case 0x34:
                     Result = "Battery Low M";
                     break;
                  case 0x35:
                     Result = "Ink Pressure High";
                     break;
                  case 0x36:
                     Result = "Product Speed Matching Error";
                     break;
                  case 0x37:
                     Result = "External Communication Error nnn";
                     break;
                  case 0x38:
                     Result = "Ambient Temperature Too High";
                     break;
                  case 0x39:
                     Result = "Ambient Temperature Too Low";
                     break;
                  case 0x3a:
                     Result = "Ink heating failure";
                     break;
                  case 0x3b:
                     Result = "External Signal Error nnn";
                     break;
                  case 0x3c:
                     Result = "Ink Pressure Low";
                     break;
                  case 0x3d:
                     Result = "Excitation V-ref. Review";
                     break;
                  case 0x3e:
                     Result = "Viscosity Reading Instability";
                     break;
                  case 0x3f:
                     Result = "Viscosity Readings Out of Range";
                     break;
                  case 0x40:
                     Result = "High Ink Viscosity";
                     break;
                  case 0x41:
                     Result = "Low Ink Viscosity";
                     break;
                  case 0x42:
                     Result = "Excitation V-ref. Review 2";
                     break;
                  case 0x44:
                     Result = "Battery Low C";
                     break;
                  case 0x45:
                     Result = "Calendar Content Inaccurate";
                     break;
                  case 0x46:
                     Result = "Excitation V-ref. Char. height Review";
                     break;
                  case 0x47:
                     Result = "Ink Shelf Life Information";
                     break;
                  case 0x48:
                     Result = "Makeup Shelf Life Information";
                     break;
                  case 0x49:
                     Result = "Model-key Failure";
                     break;
                  case 0x4a:
                     Result = "Language-key Failure";
                     break;
                  case 0x4c:
                     Result = "Upgrade-Key Fault";
                     break;
                  case 0x50:
                     Result = "Circulation System Cooling Fan Fault";
                     break;
                  case 0x51:
                     Result = "Ink Tempurature Too High";
                     break;
               }
               break;
         }
         return Result;
      }

      bool IsStatus(string status) {
         return status.Length == 7
             && status[0] == cSTX
             && status[1] == '1'
             && status[6] == cETX;
      }

      #endregion

      #region Service Routines

      protected class Ops {
         public Ops(PrinterOps Op, int SubOp, string Desc, bool HasSubOps) {
            this.Op = Op;
            this.SubOp = SubOp;
            this.Desc = Desc;
            this.HasSubOps = HasSubOps;
         }
         public PrinterOps Op;
         public int SubOp = 0;
         public string Desc;
         public bool HasSubOps = true;
      }

      void AddOpNames(PrinterOps Op, Enum SubOp) {
         string[] names =  Enum.GetNames(SubOp.GetType());
         int[] values = (int[])Enum.GetValues(SubOp.GetType());
         string OpName = SpaceCaps(Op.ToString());
         for (int i = 0; i < names.Length; i++) {
            string Desc = $"{SpaceCaps(Op.ToString())}({SpaceCaps(names[i])})";
            OpNames.Add(new Ops(Op, values[i], Desc, true));
         }
      }

      void AddOpNames(Enum Op) {
         string[] names = Enum.GetNames(Op.GetType());
         int[] values = (int[])Enum.GetValues(Op.GetType());
         for (int i = 0; i < names.Length; i++) {
            if (OpNames.FindIndex(x => x.Op == (PrinterOps)values[i]) < 0) {
               string Desc = $"{SpaceCaps(names[i])}";
               OpNames.Add(new Ops((PrinterOps)values[i], 0, Desc, false));
            }
         }
      }

      private string SpaceCaps(string s) {
         string result = s.Substring(0, 2);
         for (int i = 2; i < s.Length; i++) {
            if (s[i] >= 'A' && s[i] <= 'Z') {
               result += " ";
            }
            result += s.Substring(i, 1);
         }
         return result;
      }

      void BuildOpNamescodes() {
         OpNames = new List<Ops>();
         AddOpNames(PrinterOps.IssueControl, new ControlOps());
         AddOpNames(PrinterOps.WriteSpecification, new SpecificationOps());
         AddOpNames(PrinterOps.WriteCalendarSubZS, new CalendarSubTypes());
         AddOpNames(PrinterOps.WriteCalendarSub, new CalendarSubTypes());
         AddOpNames(PrinterOps.WriteCalendarZS, new CalendarSubTypes());
         AddOpNames(PrinterOps.Message, new MessageOps());
         AddOpNames(PrinterOps.Fetch, new FetchOps());
         AddOpNames(PrinterOps.Retrieve, new RetrieveOps());
         AddOpNames(PrinterOps.RetrievePattern, new RetrievePatternOps());
         AddOpNames(PrinterOps.SetClock, new SetClockOps());
         AddOpNames(PrinterOps.PositionItem, new PositionOps());
         AddOpNames(new PrinterOps());
      }

      // Convert Op/SubOp to human readable form
      string OperationName(PrinterOps OP, int SubOp) {
         int n;
         string result = string.Empty;
         if(OpNames == null) {
            result = "OperationName not initialized";
         } else if((n = OpNames.FindIndex(x => x.Op == OP && !x.HasSubOps)) >= 0) {
            result = OpNames[n].Desc;
         } else if((n = OpNames.FindIndex(x => x.Op == OP && x.SubOp == SubOp)) >= 0) {
            result = OpNames[n].Desc;
         } else {
            result = $"{OP}/{SubOp:X2}";
         }
         return result;
      }

      string Chr(object c) {
         return Convert.ToString(Convert.ToChar(c));
      }

      string Right(string original, int n) {
         return original.Substring(original.Length - n);
      }

      string ItemNumber(int intItem) {
         if (printerType == HitachiPrinterType.TwinNozzle && Nozzle == 1) {
            intItem += 100;
         }
         return ((char)(intItem + '0')).ToString();
      }

      string Item(int intItem) {

         // Does this refer to all items?
         if (intItem < 1) {
            return string.Empty;
         } else {
            string item;
            if (printerType == HitachiPrinterType.TwinNozzle && Nozzle == 1) {
               intItem += 100;
            }
            item = Chr('0' + intItem);
            if (rxClass) {
               return sESC2 + Chr(EscapeOther.ItemNumberRX) + item;
            } else {
               return sESC + Chr(EscapeOther.ItemNumber) + item;
            }
         }
      }

      string[,] shifts = null;

      int[] ShiftStarts = null;
      string[] ShiftCodes = null;

      string GetShiftCode() {
         if (shifts == null) {
            Shifts = new string[,] { { "0:00", "7:00", "15:00", "23:00", "24:00" }, { "3", "1", "2", "3", "3" } };
         }
         string ShiftCode = ShiftCodes[ShiftCodes.Length - 1];
         double n = DateTime.Now.TimeOfDay.TotalMinutes;
         for (int i = 0; i < ShiftCodes.Length - 1; i++) {
            if (n >= ShiftStarts[i] && n < ShiftStarts[i + 1]) {
               ShiftCode = ShiftCodes[i];
               break;
            }
         }
         return ShiftCode;
      }

      private string TranslateInput(string strIn) {

         // Local storage
         string strOut;
         int intByte;

         // Clear string
         strOut = string.Empty;

         // Loop thru the string
         while (strIn.Length > 0) {
            intByte = strIn[0];
            switch (intByte) {
               case cESC:
               case cESC2:
                  strOut = strOut + $"<{intByte:X2}>";
                  strIn = strIn.Substring(1);
                  if (strIn != string.Empty) {
                     intByte = strIn[0];
                     strOut = strOut + $"<{intByte:X2}>";
                     if (intByte == 0xC0 | intByte == 0xD0) {
                        strIn = strIn.Substring(1);
                        if (strIn != string.Empty) {
                           strOut = strOut + $"<{(int)strIn[0]:X2}>";
                        }
                     }
                  }
                  break;
               default:
                  //
                  // Control Character?
                  if (intByte < 32 | intByte > 127) {
                     if(intByte >= (int)AC.Count && intByte < (int)AC.LogoPattern) {
                        strOut = strOut + $"<{(AC)intByte}>";
                     } else {
                        strOut = strOut + $"<{intByte:X2}>";
                     }
                  } else {
                     strOut = strOut + strIn.Substring(0, 1);
                  }
                  break;
            }
            if (strIn.Length > 0) {
               strIn = strIn.Substring(1);
            }
         }

         // Set the return
         return strOut;
      }

      protected class BC {
         public BC(string Name, bool Rx, bool Pxr, int RxId, int PxrId) {
            this.Name = Name;
            this.Rx = Rx;
            this.Pxr = Pxr;
            this.RxId = RxId;
            this.PxrId = PxrId;
         }
         public string Name;
         public bool Rx;
         public bool Pxr;
         public int RxId;
         public int PxrId;
      }

      List<BC> BCs = null;

      int BarcodeNameToHitachi(string t, bool rxClass) {
         if (BCs == null) {
            BuildBarcodes();
         }
         int result = -1; // -1 implies not supported
         int i = BCs.FindIndex(x => x.Name == t);
         if (i >= 0) {
            if (rxClass) {
               result = BCs[i].RxId;
            } else {
               result = BCs[i].PxrId;
            }
         }
         return result;
      }

      void BuildBarcodes() {
         BCs = new List<BC> {
            new BC("Code39", true, true, 1, 0),
            new BC("ITF", true, true, 2, 1),
            new BC("NW-7", true, true, 3, 2),
            new BC("43478", true, true, -1, -1),
            new BC("DM16x16", true, true, 6, 4),
            new BC("DM8x32", true, true, 5, 5),
            new BC("Code 128(Code Set B)", true, true, 13, 6),
            new BC("Code 128(Code Set C)", true, true, 14, 6),
            new BC("DM16x36", true, true, 7, 7),
            new BC("DM16x48", true, true, 8, 8),
            new BC("DM18x18", true, true, 9, 9),
            new BC("DM20x20", true, true, 10, 10),
            new BC("DM22x22", true, true, 11, 11),
            new BC("DM24x24", true, true, 12, 12),
            new BC("UPC-A", true, false, 15, -1),
            new BC("UPC-B", true, false, 16, -1),
            new BC("EAN-8", true, false, 17, -1),
            new BC("QR21x21", true, false, 18, -1),
            new BC("QR25x25", true, false, 19, -1),
            new BC("QR29x29", true, false, 20, -1),
            new BC("GS1_Lim", true, false, 21, -1),
            new BC("GS1_Omn", true, false, 22, -1),
            new BC("GS1_Stk", true, false, 23, -1),
            new BC("EAN-13 AddOn 5", true, false, 25, -1),
            new BC("MicroQR15x15", true, false, 27, -1),
            new BC("EAN-13", true, true, 4, 3)
         };
      }

      List<BC> FTs = null;
      void BuildFonts() {
         FTs = new List<BC> {
            new BC("4X5", true, false, 0, -1),
            new BC("5X5", true, true, 1, 0),
            new BC("5X7", true, true, 2, 1),
            new BC("5X8", true, true, 2, 1),
            new BC("9X7", true, true, 3, 2),
            new BC("9X8", true, true, 3, 2),
            new BC("7X10", true, true, 4, 2),
            new BC("10X12", true, false, 5, -1),
            new BC("12X16", true, true, 6, 3),
            new BC("18X24", true, true, 7, 4),
            new BC("24X32", true, true, 8, 5),
            new BC("11X11", false, false, 9, -1),
            new BC("5X3(CHIMNEY)", true, false, 10, -1),
            new BC("5X5(CHIMNEY)", true, false, 11, -1),
            new BC("7X5(CHIMNEY)", true, false, 12, -1)
         };
      }

      string PadLeftZeros(string Data, int Count) {
         string Result;
         Result = "0000000000" + Data;
         return Result.Substring(Result.Length - Count);
      }

      string PadLeftZeros(int Data, int Count) {
         string Result = "0000000000" + Math.Abs(Data).ToString();
         if (Data < 0) {
            return "-" + Result.Substring(Result.Length - Count + 1);
         } else {
            return Result.Substring(Result.Length - Count);
         }
      }

      string PadLeftZeros(bool Data, int Count) {
         string Result;
         if (Data) {
            Result = "0000000001";
         } else {
            Result = "0000000000";
         }
         return Result.Substring(Result.Length - Count);
      }

      #endregion

   }

   #region State Object

   public class StateObject {
      // Client socket. 
      public Socket workSocket = null;
      // Size of receive buffer. 
      public const int BufferSize = 256;
      // Receive buffer. 
      public byte[] buffer = new byte[BufferSize];
      // Received data string. 
      public StringBuilder sb = new StringBuilder();
   }

   #endregion

}
