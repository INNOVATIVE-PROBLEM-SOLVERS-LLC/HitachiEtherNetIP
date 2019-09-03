using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HitachiProtocol;

namespace HitachiProtocol_Test {

   public partial class HPTest : Form {

      #region Data Declarations

      HitachiPrinter HP;

      // Ethernet connection parameters
      IPAddress ipAddress;
      int ipPort;

      // Serial connection parameters
      string sPort;
      int sBaudRate;
      Parity sParity;
      int sDataBits;
      StopBits sStopBits;

      bool SetupInProgress = false;

      #endregion

      #region Constructors and Destructors

      public HPTest() {
         InitializeComponent();
      }

      ~HPTest() {

      }

      #endregion

      #region Form Level Events

      void HPTest_Load(object sender, EventArgs e) {
         // Load saved settings
         LoadSettings();
         // Initialize communications settings
         string[] Portnames = System.IO.Ports.SerialPort.GetPortNames();
         cbPrinterPortName.Items.Clear();
         cbPrinterPortName.Items.AddRange(Portnames);
         if (Portnames.Length > 0) {
            cbPrinterPortName.Items.AddRange(Portnames);
            cbPrinterPortName.SelectedIndex = 0;
         }
         // Instantiate the printer
         HP = new HitachiPrinter(this, 0) {
            PrinterType = HitachiPrinterType.UX,
            SOP4Enabled = true,
            EventLogging = HPEventLogging.All,
            MessageStyle = FormatSetup.Individual,
            MergeRequests = false,
            Shifts = new string[,]
              {
                 { "0:00", "7:00", "15:00", "23:00", "24:00" },
                 { "3", "1", "2", "3", "3" }
              }
         };
         HP.Log += HP_Log;
         HP.Complete += HP_Complete;
         HP.Unsolicited += HP_Unsolicited;
         SetButtonEnables();
      }

      void HPTest_FormClosing(object sender, FormClosingEventArgs e) {
         HP.Log -= HP_Log;
         HP.Complete -= HP_Complete;
         HP.Unsolicited -= HP_Unsolicited;
         HP = null;
         SaveSettings();
      }

      #endregion

      #region Form Control Events

      void cmdConnect_Click(object sender, EventArgs e) {
         if (ConfigureConnection.SelectedIndex == 0) {
            // Ethernet Connection
            HP.Connect(ipAddress, ipPort);
         } else if (ConfigureConnection.SelectedIndex == 1) {
            // Serial connection
            HP.Connect(sPort, sBaudRate, sParity, sDataBits, sStopBits);
         } else {
            HP.Connect();
         }
         HP.IssueControl(ControlOps.ComOn);
      }

      void cmdDisconnect_Click(object sender, EventArgs e) {
         HP.Disconnect();
      }

      void cmdSend_Click(object sender, EventArgs e) {
         // Indicate operation in progress
         SetupInProgress = true;
         // Clear all current items
         HP.IssueControl(ControlOps.ClearAll);
         // Set all items to 7x10 format
         HP.WriteFormat(0, "7x10", 1, 1);
         // Insert text in three items
         HP.WriteText(1, 
            $"Hello World\r\n" + 
            $"{(char)AC.Month}{(char)AC.Month}/" +
            $"{(char)AC.Day}{(char)AC.Day}/" +
            $"{(char)AC.Year}{(char)AC.Year}\r\n" +
            $"Line 3");
         // Stack the items into a single column
         HP.ColumnSetup("3", "1");
         // Get the printer status
         HP.Fetch(FetchOps.Status);
         HP.Idle("Done");
      }

      void cmdExit_Click(object sender, EventArgs e) {
         this.Close();
      }

      #endregion

      #region Context Menu Routines

      void cmTraffic_Click(object sender, EventArgs e) {
         lbTraffic.Items.Clear();
      }

      void cmLoadInNotepad_Click(object sender, EventArgs e) {
         StreamWriter outputFileStream = null;
         string[] allLines = new string[lbTraffic.Items.Count];
         string outputPath = @"C:\Temp\traffic.txt";
         lbTraffic.Items.CopyTo(allLines, 0);
         outputFileStream = new StreamWriter(outputPath, false);
         for (int i = 0; i < lbTraffic.Items.Count; i++) {
            outputFileStream.WriteLine(allLines[i]);
         }
         outputFileStream.Flush();
         outputFileStream.Close();
         Process.Start("notepad.exe", outputPath);
      }

      #endregion

      #region Event Processing

      void HP_Log(HitachiPrinter p, HPEventArgs e) {
         lbTraffic.Items.Add(HP.Translate(e.Message));
      }

      void HP_Complete(HitachiPrinter p, HPEventArgs e) {
         switch (e.Op) {
            case PrinterOps.Nop:
               break;
            case PrinterOps.Connect:
               break;
            case PrinterOps.Disconnect:
               break;
            case PrinterOps.IssueControl:
               break;
            case PrinterOps.ColumnSetup:
               break;
            case PrinterOps.WriteSpecification:
               break;
            case PrinterOps.WriteFormat:
               break;
            case PrinterOps.WriteText:
               break;
            case PrinterOps.WriteCalendarOffset:
               break;
            case PrinterOps.WriteCalendarSubZS:
               break;
            case PrinterOps.WriteCountCondition:
               break;
            case PrinterOps.WritePattern:
               break;
            case PrinterOps.Message:
               break;
            case PrinterOps.Fetch:
               if ((FetchOps)e.SubOp == FetchOps.Status) {
                  HPStatus status = HP.GetStatus();
               }
               break;
            case PrinterOps.Retrieve:
               break;
            case PrinterOps.RetrievePattern:
               break;
            case PrinterOps.SetClock:
               break;
            case PrinterOps.Idle:
               if(e.Message == "Done") {
                  SetupInProgress = false;
               }
               break;
            case PrinterOps.PassThru:
               break;
            case PrinterOps.ENQ:
               break;
            case PrinterOps.SOP16ClearBuffer:
               break;
            case PrinterOps.SOP16RestartPrinting:
               break;
            case PrinterOps.ChangeInkDropRule:
               break;
            case PrinterOps.ChangeMessageFormat:
               break;
            case PrinterOps.PositionItem:
               break;
            case PrinterOps.WriteCalendarZS:
               break;
            case PrinterOps.WriteCalendarSub:
               break;
            case PrinterOps.WriteCalendarSubRule:
               break;
            case PrinterOps.TimedDelay:
               break;
            case PrinterOps.CreateMessage:
               break;
            case PrinterOps.SendMessage:
               break;
            case PrinterOps.SetNozzle:
               break;
            case PrinterOps.ShutDown:
               break;
            default:
               break;
         }
         SetButtonEnables();
      }

      void HP_Unsolicited(HitachiPrinter p, HPEventArgs e) {
         if (e.Message.StartsWith(HitachiPrinter.sSTX) && e.Message.EndsWith(HitachiPrinter.sETX)) {
            switch (e.Message.Substring(1, 1)) {
               case HitachiPrinter.sBEL:
                  PrintStart(p, e);
                  break;
               case HitachiPrinter.sDLE:
                  PrintEnd(p, e);
                  break;
               case "1":
                  // It is a status
                  break;
               default:
                  // Who knows
                  break;
            }
         } else {
            // Who knows
         }
      }

      void PrintStart(HitachiPrinter p, HPEventArgs e) {
         // Add message build here
      }

      void PrintEnd(HitachiPrinter p, HPEventArgs e) {
         // Send message to printer
      }

      #endregion

      #region Service Routines

      void LoadSettings() {
         // Create a shortcut for settings
         Properties.Settings p = Properties.Settings.Default;
         // Load the user's data
         ConfigureConnection.SelectedIndex = p.Tab;
         tbPrinterIPAddress.Text = p.IPAddress;
         tbPrinterPort.Text = p.IPPort;
         cbPrinterPortName.Text = p.SerialPort;
         cbPrinterBaudRate.Text = p.SerialBaudRate;
         cbPrinterDataBits.Text = p.SerialDataBits;
         cbPrinterParity.Text = p.SerialParity;
         cbPrinterStopBits.Text = p.SerialStopBits;
      }

      void SaveSettings() {
         // Create a shortcut for settings
         Properties.Settings p = Properties.Settings.Default;
         // Save the user's data
         p.Tab = ConfigureConnection.SelectedIndex;
         p.IPAddress = tbPrinterIPAddress.Text;
         p.IPPort = tbPrinterPort.Text;
         p.SerialPort = cbPrinterPortName.Text;
         p.SerialBaudRate = cbPrinterBaudRate.Text;
         p.SerialDataBits = cbPrinterDataBits.Text;
         p.SerialParity = cbPrinterParity.Text;
         p.SerialStopBits = cbPrinterStopBits.Text;
         p.Save();
      }

      void SetButtonEnables() {
         bool ConfigOK;
         if (ConfigureConnection.SelectedIndex == 0) {
            // Ethernet Connection
            if (IPAddress.TryParse(tbPrinterIPAddress.Text, out ipAddress)
               && int.TryParse(tbPrinterPort.Text, out ipPort)) {
               ConfigOK = true;
            } else {
               ConfigOK = false;
            }
         } else {
            // Serial connection
            ConfigOK = cbPrinterPortName.SelectedIndex >= 0
               && cbPrinterBaudRate.SelectedIndex >= 0
               && cbPrinterDataBits.SelectedIndex >= 0
               && cbPrinterParity.SelectedIndex >= 0
               && cbPrinterStopBits.SelectedIndex >= 0;
            if (ConfigOK) {
               sPort = cbPrinterPortName.Text;
               sBaudRate = Convert.ToInt32(cbPrinterBaudRate.Text);
               sParity = (Parity)Enum.Parse(typeof(Parity), cbPrinterParity.Text, true);
               sDataBits = Convert.ToInt32(cbPrinterDataBits.Text);
               sStopBits = (StopBits)Enum.Parse(typeof(StopBits), cbPrinterStopBits.Text, true);
            }
         }
         bool connected = HP != null && HP.ConnectionState != ConnectionStates.Closed;
         cmdConnect.Enabled = ConfigOK && !connected;
         cmdDisconnect.Enabled = connected;
         cmdSend.Enabled = connected;
         cmdExit.Enabled = !connected;
      }

      void SetButtonEnables(object sender, EventArgs e) {
         SetButtonEnables();
      }

      #endregion

   }
}
