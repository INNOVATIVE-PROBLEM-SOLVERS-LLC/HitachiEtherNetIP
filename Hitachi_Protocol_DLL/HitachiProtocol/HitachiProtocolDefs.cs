using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitachiProtocol {

   #region Namespace level definitions

   // The enumerations in this section are Order and Value dependent.  Do not change.

   // Attributed Characters
   public enum AC {
      UserPattern = 0x1100, // Add User Pattern offset value
      FreePattern = 0x1200, // Add Free Pattern offset value
      Count = 0x1300,       // Counter
      Year,                 // Date constants
      Month,                // Date constants
      Day,                  // Date constants
      MonthName,            // Date constants
      Hour,                 // Date constants
      Minute,               // Date constants
      Second,               // Date constants
      Shift,                // Date constants
      TotalDays,            // Date constants
      Week,                 // Date constants
      DayOfWeek,            // Date constants
      Apostrophe,           // Half size characters
      Period,               // Half size characters
      Colon,                // Half size characters
      Comma,                // Half size characters
      Space,                // Half size characters
      SemiColon,            // Half size characters
      Exclamation,          // Half size characters
      Link,                 // Not used in this application
      Prompt,               // Not used in this application
      DateCode,             // Not used in this application
      WorkOrder,            // Not used in this application
      LogoPattern = 0x1400, // Not used in this application
      Mask = 0xff00         // Mask for identifying attributed characters
   }

   // Hitachi printer type
   public enum HitachiPrinterType {
      PH = 0,
      PX = 1,
      PXR = 2,
      PXRH = 3,
      RX = 4,
      RX2 = 5,
      UX = 6,
      TwinNozzle = 7
   }

   // Methods of rendering month 3-character names
   internal enum MonthSubstitutionMethod {
      NotSupported = 0,
      ViaMonth = 1,
      ViaMonthName = 2,
   }

   // Message Formats
   public enum FormatSetup {
      Individual = 0,
      Overall = 1,
      FreeLayout = 2
   }

   // Connection Type
   public enum ConnectionType {
      Serial = 0,
      EthernetToSerial = 1,
      Simulator = 2,
      OffLine = 3
   }

   // Connection states
   public enum StateChange {
      Initializing,
      Connecting,
      Connected,
      ConnectFailed,
      Disconnected,
      TimeoutRetrying,
      TimeoutAbort,
      UpdateACKNAK,
      OffLine
   }

   // Event Logging constants
   public enum HPEventLogging {
      Unsolicited = 1,           // 0
      OperationStart = 2,        // 1
      Output = 4,                // 2
      Input = 8,                 // 3
      OperationComplete = 16,    // 4
      SetDTR = 32,               // 5
      SetRTS = 64,               // 6
      ReportCD = 128,            // 7
      ReportDSR = 256,           // 8
      ReportCTS = 512,           // 9
      RawData = 1024,            // 10
      All = 2047,                // 0 thru 10
      None = 0,                  // None set
   }

   // Hitachi Printer Operations
   public enum PrinterOps {
      Nop = 0,
      Connect = 1,
      Disconnect = 2,
      IssueControl = 3,
      ColumnSetup = 4,
      WriteSpecification = 5,
      WriteFormat = 6,
      WriteText = 7,
      WriteCalendarOffset = 8,
      WriteCalendarSubZS = 9,
      WriteCountCondition = 10,
      WritePattern = 11,
      Message = 12,
      Fetch = 13,
      Retrieve = 14,
      RetrievePattern = 15,
      SetClock = 16,
      Idle = 17,
      PassThru = 18,
      ENQ = 19,
      SOP16ClearBuffer = 20,
      SOP16RestartPrinting = 21,
      ChangeInkDropRule = 22,
      ChangeMessageFormat = 23,
      PositionItem = 24,
      WriteCalendarZS = 25,
      WriteCalendarSub = 26,
      WriteCalendarSubRule = 27,
      TimedDelay = 28,
      CreateMessage = 29,
      SendMessage = 30,
      SetNozzle = 31,
      ShutDown = 32,
   }

   // Control Operations
   public enum ControlOps {
      ComOn = 0,
      ComOff = 1,
      HydraulicsStart = 2, // SOP-04 or RX Only
      HydraulicsStop = 3,  // SOP-04 or RX Only
      Ready = 4,           // SOP-04 or RX Only
      Standby = 5,         // SOP-04 or RX Only
      ResetAlarm = 6,      // SOP-04 or RX Only
      DC2 = 7,
      DC3 = 8,
      Enquire = 9,
      ClearAll = 10,
      ClearAllByNozzle = 11
   }

   // Print Specification Operations
   public enum SpecificationOps {
      CharacterHeight = 0,
      CharacterWidth = 1,
      CharacterOrientation = 2,
      PrintStartDelay = 3,
      RepeatIntervals = 4,
      RepeatCount = 5,
      PrintStartDelayReverse = 6,
      TargetSensorTimer = 7,
      TargetSensorFilter = 8,
      TargetSensorFilterDivision = 9,
      HighSpeedPrinting = 10,
      ProductSpeedMatching = 11,
      FrequencyDivisor = 12,
      InkDropUsage = 13,
      OverallColumnSetup = 14,
      PrintStartDelayAll = 15,
      InkDropChargeRule = 16,
      LeadingCharWidthControl = 17,
      LeadingCharWidthControlWidth = 18,
      NozzleSpaceAlignment = 19
   }

   // Message Operations
   public enum MessageOps {
      MessageSave = 0,
      MessageRestore = 1
   }

   // Fetch Operations (SOP-04 Only)
   public enum FetchOps {
      Status = 0,
      Time = 1,
      PreviousMessage = 2,
      Currentmessage = 3
   }

   // Retrieve Operations == The enumerations are order and value dependent Do not change them
   public enum RetrieveOps {
      LineSetting = 0,               // 00 PXR C0 C1 RX C0 31
      PrintContentsAttributes,       // 01 PXR C0 C2 RX C0 32
      PrintContentsNoAttributes,     // 02 PXR C0 C3 RX C0 33
      CalendarCondition,             // 03 PXR C0 C4 RX C0 34
      SubstitutionRule,              // 04 PXR C0 C5 RX C0 35
      SubstitutionRuleData,          // 05 PXR       RX C0 36
      ShiftCodeSetup,                // 06 PXR C0 D5 RX C0 37
      TimeCountCondition,            // 07 PXR C0 D6 RX C0 38
      CountCondition,                // 08 PXR C0 C6 RX C0 39
      PrintFormat,                   // 09 PXR C0 C7 RX C0 3A
      AdjustICS,                     // 10 PXR       RX C0 3B
      PrintSpecifications,           // 11 PXR C0 C8 RX C0 3C
      VariousPrintSetup,             // 12 PXR       RX C0 3D
      MessageGroupNames,             // 13 PXR       RX CO 3E
      PrintData,                     // 14 PXR C0 C9 RX C0 3F
      UserEnvironmentSetup,          // 15 PXR C0 CA RX C0 40
      DateTimeSetup,                 // 16 PXR C0 CB RX C0 41
      CommunicationsSetup,           // 17 PXR C0 CC RX C0 42
      TouchScreenSetup,              // 18 PXR C0 CD RX C0 43
      UnitInformation,               // 19 PXR D0 D1 RX C0 47
      OperationManagement,           // 20 PXR C0 CE RX C0 48
      AlarmHistory,                  // 21 PXR C0 CF RX C0 49
      PartsUsageTime,                // 22 PXR C0 D1 RX C0 4A
      CirculationSystemSetup,        // 23 PXR       RX C0 4B
      SoftwareVersion,               // 24 PXR C0 D2 RX C0 4C
      AdjustmentOperationalCheckout, // 25 PXR D0 D4 RX C0 4D
      SolenoidValvePumpTest,         // 26 PXR DO D5 RX C0 4E
      FreeLayoutCoordinates,         // 27 PXR       RX C0 50
      StirrerTest,                   // 28 PXR C0 D3
      MonthSubstituteRule,           // 29 PXR C0 D4
      ViscometerCalibration,         // 30 PXR D0 D2
      SystemEnvironmentSetup,        // 31 PXR D0 D3
   }

   // Retrieve Operations
   public enum RetrievePatternOps {
      User = 0,
      Standard = 1,
   }

   // Set Clock Operations
   public enum SetClockOps {
      CurrentDateTime = 0,
      CalendarTimeControl = 1,
      CalendarDateTime = 2,
      TwelveTwentyFour = 3,
   }

   // Free Layout Operations
   public enum PositionOps {
      HorizontalVerticalPosition = 0,
      HorizontalPosition = 1,
      VerticalPosition = 2,
      HorizontalVerticalMove = 3,
      HorizontalMove = 4,
      VerticalMove = 5
   }

   // Calendar Substitution Types
   public enum CalendarSubTypes {
      Year = 0,
      Month = 1,
      Day = 2,
      Hour = 3,
      Minute = 4,
      Week = 5,
      DayOfWeek = 6
   }

   // Status areas
   public enum StatusAreas {
      Connection = 0,
      Reception = 1,
      Operation = 2,
      Alarm = 3
   }

   // Connection State
   public enum ConnectionStates {
      Closed = 0,
      Connecting = 1,
      Connected = 2
   }

   #endregion

   public partial class HitachiPrinter {

      #region Events

      // Operation Complete Event
      public event CompleteHandler Complete;
      public delegate void CompleteHandler(HitachiPrinter p, HPEventArgs e);

      // Errors Detected
      public event NotifyClientHandler NotifyClient;
      public delegate void NotifyClientHandler(HitachiPrinter p, HPEventArgs e);

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(HitachiPrinter p, HPEventArgs e);

      // Unsolicited Input from the printers
      public event UnsolicitedHandler Unsolicited;
      public delegate void UnsolicitedHandler(HitachiPrinter p, HPEventArgs e);

      // Status change from the printer
      public event StatusChangedHandler StatusChanged;
      public delegate void StatusChangedHandler(HitachiPrinter p, HPStatus status);

      // Progress bar functions
      public event RequestAddedHandler RequestAdded;
      public delegate void RequestAddedHandler(HitachiPrinter p);
      public event RequestCompletedHandler RequestCompleted;
      public delegate void RequestCompletedHandler(HitachiPrinter p);

      // Raw data for debug functions
      public event RawDataHandler RawData;
      public delegate void RawDataHandler(HitachiPrinter p, HPEventArgs e);

      #endregion

      #region Public Constants

      public const char cNUL = (char)0;    // 00h Null
      public const char cSTX = (char)2;    // 02h Start of Text
      public const char cETX = (char)3;    // 03h End of Text
      public const char cENQ = (char)5;    // 05h Enquire
      public const char cACK = (char)6;    // 06h Acknowledge
      public const char cBEL = (char)7;    // 07h Bell
      public const char cLF = (char)10;    // 0Ah Line Feed
      public const char cCR = (char)13;    // 0Dh carriage Return
      public const char cSO = (char)14;    // 0Eh Shift Out
      public const char cSI = (char)15;    // 0Fh Shift In
      public const char cDLE = (char)16;   // 10h Data Link Escape
      public const char cDC2 = (char)18;   // 12h Device Control 2
      public const char cDC3 = (char)19;   // 13h Device Control 3
      public const char cNAK = (char)21;   // 15h Negative Acknowledge
      public const char cEM = (char)25;    // 19h End of Medium
      public const char cESC = (char)27;   // 1Bh Escape for PX and PXR printers
      public const char cESC2 = (char)31;  // 1Fh Escape for RX and UX printer
      public const char cSpace = (char)32; // 20h Space

      // Control characters as strings
      public const string sNUL = "\x00";     // 00h Null Character
      public const string sSTX = "\x02";     // 02h Start of Text
      public const string sETX = "\x03";     // 03h End of Text
      public const string sENQ = "\x05";     // 05h Enquire
      public const string sACK = "\x06";     // 06h Acknowledge
      public const string sBEL = "\x07";     // 07h Bell
      public const string sLF = "\x0A";      // 0Ah Line Feed
      public const string sCR = "\x0D";      // 0Dh Carriage Return
      public const string sSO = "\x0E";      // 0Eh Shift Out
      public const string scSI = "\x0F";     // 0Fh Shift In
      public const string sDLE = "\x10";     // 10h Data Link Escape
      public const string sDC2 = "\x12";     // 12h Device Control 2
      public const string sDC3 = "\x13";     // 13h Device Control 3
      public const string sNAK = "\x15";     // 15h Negative Acknowledge
      public const string sEM = "\x19";      // 19h End of Medium
      public const string sESC = "\x1B";     // 1Bh Escape for PX and PXR printers
      public const string sESC2 = "\x1F";    // 1Fh Escape for RX printer
      public const string sSpace = "\x20";   // 20h Space character
      public const string sTilde = "\x7E";   // 7Eh Tilde character

      #endregion

      #region Constructors/Destructors

      // Class initialization for Simulated connection
      public HitachiPrinter(Form parent, int ID) {

         if (OpNames == null) {
            BuildOpNamescodes();
         }

         previous = DateTime.Now;

         this.parent = parent;
         this.ID = ID;

         // Initialize all default settings
         SetDefaults();

         // Set as Simulator connection
         Connection = ConnectionType.Simulator;
      }

      ~HitachiPrinter() {
         try {
            CleanUp();
         } catch {
         }
      }

      #endregion

      #region Public Declarations, Properties and Methods

      public FormatSetup MessageStyle { get; set; } = FormatSetup.Individual;

      public HitachiPrinterType PrinterType {
         get { return printerType; }
         set {
            printerType = value;
            rx2Class = printerType == HitachiPrinterType.RX2
                   || printerType == HitachiPrinterType.UX
                   || printerType == HitachiPrinterType.TwinNozzle;
            rxClass = rx2Class || printerType == HitachiPrinterType.RX;
            TenCharsPerItem = !rxClass;
            useESC2 = rxClass;
            if (rxClass) {
               maxLength = 3000 - 150;
            } else {
               maxLength = 1500 - 150;
            }
         }
      }

      internal MonthSubstitutionMethod MonthSubMethod {
         get {
            MonthSubstitutionMethod result = MonthSubstitutionMethod.NotSupported;
            switch (PrinterType) {
               case HitachiPrinterType.RX2:
                  if (SubstitutionRules) {
                     result = MonthSubstitutionMethod.ViaMonth;
                  } else {
                     result = MonthSubstitutionMethod.ViaMonthName;
                  }
                  break;
               case HitachiPrinterType.UX:
                  result = MonthSubstitutionMethod.ViaMonthName;
                  break;
            }
            return result;
         }
      }

      public int MaxItems {
         get { return maxItems; }
         set {
            if (value == 24 || value == 100) {
               maxItems = value;
            } else {
               maxItems = 24;
            }
            MaxExtent[0] = maxItems;
            MaxExtent[1] = maxItems;
         }
      }
      public int Nozzle { get { return nozzle; } set { nozzle = value; } }
      public bool SubstitutionRules { get; set; } = false;
      public int PrintDataTimeout { get; set; } = 10000;
      public bool TenCharsPerItem { get; set; } = false;
      public HPEventLogging EventLogging {
         get { return eventLogging; }
         set { eventLogging = value; }
      }
      public bool SOP4Enabled { get; set; } = false;
      public int nACKs = 0;
      public int nNAKs = 0;
      public bool MergeRequests { get; set; } = true;

      public bool RXClass {
         get { return rxClass; }
      }

      internal bool RX2Class {
         get { return rx2Class; }
      }

      // Status Area
      public HPStatus StatusArea;

      public HPStatus GetStatus() {
         return StatusArea;
      }

      public ConnectionStates ConnectionState {
         get {
            return connectionState;
         }
      }

      // Connect
      public void Connect(ConnectionType t = ConnectionType.Simulator) {
         this.Connection = t;

         // Signaling start of connect
         this.BuildStatus(StateChange.Connecting);

         // Connect to the printer, bring printer to an idle state and get the status
         if (t != ConnectionType.OffLine) {
            IssueRequest(GetRequest(PrinterOps.Connect));
            //IssueControl(ControlOps.ComOn);
         }
      }

      public void Connect(IPAddress IPAddress, int Port) {

         // Save the new Ethernet Parameters and connect to the printer
         this.IPAddress = IPAddress;
         this.IPPort = Port;
         this.Connect(ConnectionType.EthernetToSerial);
      }

      public void Connect(string PortName, int BaudRate, Parity Parity, int DataBits, StopBits StopBits) {

         // Save the new Serial Port Parameters and connect to the printer
         this.PortName = PortName;
         this.BaudRate = BaudRate;
         this.Parity = Parity;
         this.DataBits = DataBits;
         this.StopBits = StopBits;
         this.Connect(ConnectionType.Serial);
      }

      public void Disconnect() {
         IssueRequest(GetRequest(PrinterOps.Disconnect));
      }

      public void IssueControl(ControlOps Control) {
         // Watch out for SOP-04 Only operations
         switch (Control) {
            case ControlOps.HydraulicsStart:
            case ControlOps.HydraulicsStop:
            case ControlOps.Ready:
            case ControlOps.Standby:
               if (SOP4Enabled || rxClass) {
                  IssueRequest(GetRequest(PrinterOps.IssueControl, (int)Control));
               }
               break;
            case ControlOps.ResetAlarm:
               if (SOP4Enabled || rxClass) {
                  IssueRequest(GetRequest(PrinterOps.IssueControl, (int)Control));
                  Delay(1000);
               } else {
                  // For the non-RX Class, it takes two commands.  A ComOFF and a ComOn
                  IssueRequest(GetRequest(PrinterOps.IssueControl, (int)ControlOps.ComOff));
                  // Give it time to process
                  Delay(2000);
                  // The reset alarm will be issued as a com on
                  IssueRequest(GetRequest(PrinterOps.IssueControl, (int)ControlOps.ComOn));
               }
               break;
            default:
               IssueRequest(GetRequest(PrinterOps.IssueControl, (int)Control));
               break;
         }
      }

      public void ColumnSetup(string LineCount, string LineSpacing) {

         // If no data, assume single line
         if (LineCount.Length == 0) {
            LineCount = "1";
            LineSpacing = "0";
         }
         if (LineCount.Length == LineSpacing.Length) {
            HPRequest mReq = GetRequest(PrinterOps.ColumnSetup);
            mReq.Data1 = LineCount;
            mReq.Data2 = LineSpacing;
            IssueRequest(mReq);
         }
      }

      public void WriteSpecification(SpecificationOps SubOp, int Data1) {
         WriteSpecification(SubOp, Data1, -1);
      }

      public void WriteSpecification(SpecificationOps SubOp, int Data1, int Data2) {
         // Get the length right
         int[] pad = new int[] { 2, 3, 1, 4, 5, 4, 4, 3, 1, 4, 1, 1, 3, 2, 1, 4, 1, 1, 2, 1 };
         HPRequest mReq = GetRequest(PrinterOps.WriteSpecification, (int)SubOp);
         mReq.Data1 = PadLeftZeros(Data1, pad[(int)SubOp]);
         if (SubOp == SpecificationOps.LeadingCharWidthControlWidth) {
            mReq.Data2 = PadLeftZeros(Data1, pad[(int)SubOp]);
         }
         IssueRequest(mReq);
      }

      public void WriteFormat(int Item, string Font, int InterCharacterSpace, int IncreasedWidth, string BarcodeType = "(None)", string EANPrefix = "00", string HumanReadable = "(None)") {
         string ics;
         if (rxClass) {
            ics = PadLeftZeros(InterCharacterSpace, 2);
         } else {
            ics = PadLeftZeros(InterCharacterSpace, 1);
         }
         HPRequest mReq = GetRequest(PrinterOps.WriteFormat);
         mReq.Item = Item;
         mReq.Data1 = GetFont(Font);
         mReq.Data2 = ics;
         mReq.Data3 = PadLeftZeros(IncreasedWidth, 1);
         mReq.Data5 = BarcodeType;
         mReq.Data8 = EANPrefix;
         mReq.Data9 = HumanReadable;
         IssueRequest(mReq);
      }

      public void WriteText(int Item, string Data) {
         HPRequest mReq = GetRequest(PrinterOps.WriteText);
         mReq.Item = Item;
         mReq.Data1 = Data;
         IssueRequest(mReq);
      }

      public void WriteCalendarOffset(int Item, int YearOffset, int MonthOffset, int DayOffset, int HourOffset, int MinuteOffset, int calBlockNo, string subRule) {
         HPRequest mReq = GetRequest(PrinterOps.WriteCalendarOffset);
         mReq.Item = Item;
         mReq.Data1 = PadLeftZeros(YearOffset, 4);
         mReq.Data2 = PadLeftZeros(MonthOffset, 4);
         mReq.Data3 = PadLeftZeros(DayOffset, 4);
         mReq.Data4 = PadLeftZeros(HourOffset, 4);
         mReq.Data5 = PadLeftZeros(MinuteOffset, 4);
         mReq.Data6 = PadLeftZeros(subRule, 2);
         mReq.BlockNo = calBlockNo;
         IssueRequest(mReq);
      }

      public void WriteCalendarSubRule(int Item, int YearOffset, int MonthOffset, int DayOffset, int HourOffset, int MinuteOffset, int calBlockNo, string subRule) {
         HPRequest mReq = GetRequest(PrinterOps.WriteCalendarSubRule);
         mReq.Item = Item;
         mReq.Data1 = PadLeftZeros(YearOffset, 4);
         mReq.Data2 = PadLeftZeros(MonthOffset, 4);
         mReq.Data3 = PadLeftZeros(DayOffset, 4);
         mReq.Data4 = PadLeftZeros(HourOffset, 4);
         mReq.Data5 = PadLeftZeros(MinuteOffset, 4);
         mReq.Data6 = PadLeftZeros(subRule, 2);
         mReq.BlockNo = calBlockNo;
         IssueRequest(mReq);
      }

      public void WriteCalendarSubZS(int Item, CalendarSubTypes SubOp, int SubZS, bool Sub, bool ZS, int calBlockNo) {
         if (SubZS == 0) {
            if (printerType != HitachiPrinterType.PH) {
               HPRequest mReq = GetRequest(PrinterOps.WriteCalendarZS, (int)SubOp);
               mReq.Item = Item;
               mReq.Data1 = PadLeftZeros(Sub, 1);
               mReq.Data2 = PadLeftZeros(ZS, 1);
               mReq.BlockNo = calBlockNo;
               IssueRequest(mReq);
            }
         } else {
            HPRequest mReq = GetRequest(PrinterOps.WriteCalendarSub, (int)SubOp);
            mReq.Item = Item;
            mReq.Data1 = PadLeftZeros(Sub, 1);
            mReq.Data2 = PadLeftZeros(ZS, 1);
            mReq.BlockNo = calBlockNo;
            IssueRequest(mReq);
         }
      }

      public void WriteCountCondition(int Item, int CountSize, string InitialValue, string Range1, string Range2, string JumpFrom, string JumpTo, string Reset, string UpdateInProgress, string UpdateUnit, string Direction, string ExternalSignal, string ResetSignal, string Increment, int countBlockNo) {
         HPRequest mReq = GetRequest(PrinterOps.WriteCountCondition);
         mReq.Item = Item;
         mReq.Data1 = PadLeftZeros(InitialValue, CountSize);
         mReq.Data2 = PadLeftZeros(Range1, CountSize);
         mReq.Data3 = PadLeftZeros(Range2, CountSize);
         mReq.Data4 = JumpFrom;
         mReq.Data5 = JumpTo;
         mReq.Data6 = PadLeftZeros(Reset, CountSize);
         mReq.Data7 = PadLeftZeros(UpdateInProgress, 6);
         mReq.Data8 = PadLeftZeros(UpdateUnit, 6);
         mReq.Data9 = PadLeftZeros(Direction, 1);
         mReq.Data10 = PadLeftZeros(ExternalSignal, 1);
         mReq.Data11 = PadLeftZeros(ResetSignal, 1);
         mReq.Data12 = PadLeftZeros(Increment, 2);
         mReq.BlockNo = countBlockNo;
         IssueRequest(mReq);
      }

      public void WritePattern(string Font, int RegNumber, string PatternData) {
         HPRequest mReq = GetRequest(PrinterOps.WritePattern, RegNumber);
         mReq.Data1 = GetFont(Font);
         mReq.Data2 = PatternData;
         IssueRequest(mReq);
      }

      public void Message(MessageOps SubOp, int MessageNumber) {
         Message(SubOp, MessageNumber, string.Empty);
      }

      public void Message(MessageOps SubOp, int MessageNumber, string MessageName) {
         //if (FakeResponse != null) {
         //   FakeResponse.LastLoadedMessage = MessageNumber;
         //}
         HPRequest mReq = GetRequest(PrinterOps.Message, (int)SubOp);
         mReq.Item = MessageNumber;
         mReq.Data1 = MessageName;
         IssueRequest(mReq);
      }

      public void Fetch(FetchOps SubOp) {
         if (SOP4Enabled || SubOp == FetchOps.Time) {
            IssueRequest(GetRequest(PrinterOps.Fetch, (int)SubOp));
         }
      }

      public void Retrieve(RetrieveOps SubOp) {
         IssueRequest(GetRequest(PrinterOps.Retrieve, (int)SubOp));
      }

      public void RetrievePattern(RetrievePatternOps SubOp, string Font, int Page, int KbType) {

         int[] SizeRX = new int[] { 8, 8, 8, 16, 16, 32, 32, 72, 128, 32, 5, 5, 7, 200, 288 };
         int[] CountRX = new int[] { 37, 37, 37, 37, 37, 15 };
         int[] HeaderRX = new int[] { 4, 3 };

         int[] SizePXR = new int[] { 8, 8, 16, 32, 72, 128, 16 };
         int[,] CountPXR = new int[,] { { 45, 45, 38 }, { 46, 44, 0 } };
         int[] HeaderPXR = new int[] { 4, 2 };

         // Local Storage
         int intCharSize = 0;
         int intRcvLength = 0;

         // Translate the font
         intCharSize = GetFont(Font, true)[0] - '0';

         // Was the font invalid
         if (intCharSize < 0) {
            NotifyClient?.Invoke(this, new HPEventArgs("Invalid font specified!"));
            return;
         }

         // Is the page number valid
         switch (SubOp) {
            case RetrievePatternOps.Standard:
               if (Page < 1 || Page > 3) {
                  Page = -1;
               }
               break;
            case RetrievePatternOps.User:
               if (rxClass) {
                  if (Page < 1 || Page > 6) {
                     Page = -1;
                  }
               } else {
                  if (Page < 1 || Page > 3) {
                     Page = -1;
                  }
               }
               break;
         }

         // Is the page number invalid
         if (Page < 0) {
            NotifyClient?.Invoke(this, new HPEventArgs("Invalid page number specified!"));
            return;
         }

         // Get the number of characters expected and issue operation
         if (rxClass) {
            if (SubOp == RetrievePatternOps.User) {
               intRcvLength = (SizeRX[intCharSize] + HeaderRX[(int)SubOp]) * CountRX[Page - 1] + 2;
            } else {
               if (KbType == 1) {
                  if (Page == 2) {
                     intRcvLength = 2;
                  } else {
                     intRcvLength = (SizeRX[intCharSize] + HeaderRX[(int)SubOp]) * 38 + 2;
                  }
               } else {
                  intRcvLength = (SizeRX[intCharSize] + HeaderRX[(int)SubOp]) * 26 + 2;
               }
            }
         } else {
            intRcvLength = (SizePXR[intCharSize] + HeaderPXR[(int)SubOp]) * CountPXR[(int)SubOp, Page - 1] + 2;
         }
         HPRequest mReq = GetRequest(PrinterOps.RetrievePattern, (int)SubOp);
         mReq.CharSize = intCharSize;
         mReq.Page = Page;
         mReq.RcvLength = RcvLength;
         mReq.KbType = KbType;
         mReq.Retries = 0;
         IssueRequest(mReq);
      }

      public void SetClock(SetClockOps SubOp, object ClockData) {

         // Local Storage
         int intMode = 0;

         // Validate the input
         switch (SubOp) {
            case SetClockOps.CurrentDateTime:
            case SetClockOps.CalendarDateTime:
               HPRequest mReq = GetRequest(PrinterOps.SetClock, (int)SubOp);
               mReq.Data1 = ClockData.ToString();
               IssueRequest(mReq);
               break;
            case SetClockOps.CalendarTimeControl:
            case SetClockOps.TwelveTwentyFour:
               if (int.TryParse(ClockData.ToString(), out intMode)) {
                  if (intMode == 1 | intMode == 2) {
                     mReq = GetRequest(PrinterOps.SetClock, (int)SubOp);
                     mReq.Data1 = PadLeftZeros(intMode, 1);
                     IssueRequest(mReq);
                  } else {
                     NotifyClient?.Invoke(this, new HPEventArgs("Invalid Set Clock Mode!"));
                  }
               } else {
                  NotifyClient?.Invoke(this, new HPEventArgs("Invalid Set Clock Mode!"));
               }
               break;
            default:
               NotifyClient?.Invoke(this, new HPEventArgs("Invalid Set Clock Sub-type!"));
               break;
         }
      }

      public void Idle(string Marker) {
         HPRequest mReq = GetRequest(PrinterOps.Idle);
         mReq.Data1 = Marker;
         IssueRequest(mReq);
      }

      public void SetNozzle(int Nozzle) {
         IssueRequest(GetRequest(PrinterOps.SetNozzle, Nozzle));
      }

      public void PassThru(string Data, bool ExpectTextResponse) {
         HPRequest mReq = GetRequest(PrinterOps.PassThru);
         mReq.Data1 = Data;
         mReq.ExpectTextResponse = ExpectTextResponse;
         IssueRequest(mReq);
      }

      public void SOP16ClearBuffer() {
         IssueRequest(GetRequest(PrinterOps.SOP16ClearBuffer));
      }

      public void SOP16RestartPrinting() {
         IssueRequest(GetRequest(PrinterOps.SOP16RestartPrinting));
      }

      public string Translate(string strIn) {
         return TranslateInput(strIn);
      }

      public string TranslateOperation(PrinterOps Op, int SubOp) {
         return OperationName(Op, SubOp);
      }

      public void ChangeInkDropRule(string Rule) {
         HPRequest mReq = GetRequest(PrinterOps.ChangeInkDropRule);
         mReq.Data1 = Rule;
         IssueRequest(mReq);
      }

      public void ChangeMessageFormat(FormatSetup Format) {
         IssueRequest(GetRequest(PrinterOps.ChangeMessageFormat, (int)Format));
      }

      public void PositionItem(PositionOps How, int Item, int xCoord, int yCoord) {
         HPRequest mReq = GetRequest(PrinterOps.PositionItem, (int)How);
         mReq.Item = Item;
         mReq.xCoord = xCoord;
         mReq.yCoord = yCoord;
         IssueRequest(mReq);
      }

      public void Delay(int delay) {
         HPRequest mReq = GetRequest(PrinterOps.TimedDelay);
         mReq.TimedDelay = delay;
         mReq.Data1 = delay.ToString();
         IssueRequest(mReq);
      }

      public void CreateMessage() {
         IssueRequest(GetRequest(PrinterOps.CreateMessage));
      }

      public void SendMessage() {
         IssueRequest(GetRequest(PrinterOps.SendMessage));
      }

      public void StartCouponTimer(int milliSeconds) {
         CouponTimerInterval = milliSeconds;
         CouponTimerNextClick = DateTime.Now.AddMilliseconds(milliSeconds);
      }

      public void StopCouponTimer() {
         CouponTimerInterval = -1;
      }

      public string[,] Shifts {
         set {
            try {
               shifts = value;
               int n = shifts.GetLength(1);
               ShiftStarts = new int[n];
               ShiftCodes = new string[n];
               for (int i = 0; i < n; i++) {
                  string[] s = shifts[0, i].Split(':');
                  ShiftStarts[i] = Convert.ToInt32(s[0]) * 60 + Convert.ToInt32(s[1]);
                  ShiftCodes[i] = shifts[1, i];
               }
            } catch {
               shifts = null;
            }
         }
      }

      #endregion

   }
}
