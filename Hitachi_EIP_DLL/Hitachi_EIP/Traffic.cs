using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace EIP_Lib {

   // Saving data to an excel spreadsheet is time consuming since
   // the Excel Application is implemented in C++ and uses marshalling.
   // So, run the Traffic Capture in another thread

   public class Traffic {

      #region Data Declarations

      // Folder for storing EtherNet/IP Traffic
      string TrafficFolder;
      string TrafficFileName = string.Empty;

      string trafficHdrs =
              "Count OK\tData OK\tStatus/Path\tAccess\tClass\tAttribute" +
              "\t#Out\tData Out\tRaw Out\t#In\tData In\tRaw In";
      string VerifycHdrs =
              "XML Name\tClass\tAttribute\tItem\tBlock\tSub Rule\tData Out\tData In";

      // Different steps in creating the traffic excel spreadsheet.
      public enum TaskType {
         Create = 0,
         AddTraffic = 1,
         AddLog = 2,
         AddVerify = 3,
         Close = 4,
         View = 5,
         Exit = 6,
      }

      // Do the work in the background
      Thread t;

      // Use Blocking Collection to avoid spin waits
      public BlockingCollection<TrafficPkt> Tasks = new BlockingCollection<TrafficPkt>();
      TrafficPkt pkt;

      // Declare the spreadsheet variables
      Excel.Application excelApp = null;
      Excel.Workbook wb = null;
      Excel.Worksheet wsTraffic = null;
      int wsTrafficRow;
      Excel.Worksheet wsVerify = null;
      int wsVerifyRow;


      // Use for calculating elapsed time
      DateTime lastTraffic = DateTime.Now;
      DateTime lastConnect = DateTime.Now;
      DateTime lastSession = DateTime.Now;
      DateTime lastForward = DateTime.Now;

      TimeSpan elapsed;

      #endregion

      #region Constructon and service routines

      public Traffic(string TrafficFolder) {
         // 
         this.TrafficFolder = TrafficFolder;
         // Set the time and elapsed time for the others
         t = new Thread(processTasks);
         t.Start();
      }

      // Loop to process the Blocking Collection
      private void processTasks() {
         try {
            bool done = false;
            // Just one big loop
            while (!done) {
               // Post the queue count and wait for the next request
               pkt = Tasks.Take();
               switch (pkt.Type) {
                  case TaskType.Create:
                     CreateExcelApplication();
                     break;
                  case TaskType.AddTraffic:
                     AddTrafficEntry();
                     break;
                  case TaskType.AddLog:
                     AddLogEntry();
                     break;
                  case TaskType.AddVerify:
                     AddVerifyEntry();
                     break;
                  case TaskType.Close:
                     CloseExcelApplication(pkt.View);
                     break;
                  case TaskType.View:
                     // Open Excel
                     Process.Start(TrafficFileName);
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
            CloseExcelApplication(false);
         }
      }

      // Create an Excel application with two worksheets
      private void CreateExcelApplication() {
         // Create the Excel application with two work sheets
         excelApp = new Excel.Application();
         excelApp.DisplayAlerts = false;
         wb = excelApp.Workbooks.Add(Missing.Value);

         // One worksheet is free
         wsTraffic = wb.Sheets[1];
         wsTraffic.Name = "Traffic";
         // Get the headers right for the first one
         string[] s = trafficHdrs.Split('\t');
         excelApp.Cells[1, 1] = "Date/Time";
         excelApp.Cells[1, 2] = "Elapsed";
         for (int i = 0; i < s.Length; i++) {
            excelApp.Cells[1, i + 3] = s[i];
         }

         // Set the Traffic worksheet as active
         wsTraffic.Activate();

         // Set column formatting
         for (int i = 1; i < 15; i++) {
            switch (i) {
               case 2: // Elapsed time
                  // Elapsed time to four decimal places
                  excelApp.Columns[i].NumberFormat = "0.0000";
                  break;
               case 9: // Input Length
               case 12: // Output Length
                  // Two columns are pure numbers
                  excelApp.Columns[i].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                  excelApp.Columns[i].NumberFormat = "0";
                  break;
               case 10: // Input formatted data
               case 13: // Output formatted data
                  // Two columns are numbers and text, center them
                  excelApp.Columns[i].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                  excelApp.Columns[i].NumberFormat = "@";
                  break;
               default:
                  // The rest are text
                  excelApp.Columns[i].NumberFormat = "@";
                  break;
            }
         }
         wsTrafficRow = 2;

         // Add Verify worksheet
         wsVerify = wb.Sheets.Add(wb.Sheets[1], Type.Missing, Type.Missing, Type.Missing);
         wsVerify.Name = "Verify";

         // Set the Traffic worksheet as active
         wsVerify.Activate();

         s = VerifycHdrs.Split('\t');
         wsVerify.Cells[1, 1] = "Date/Time";
         wsVerify.Cells[1, 2] = "Valid";
         for (int i = 0; i < s.Length; i++) {
            wsVerify.Cells[1, i + 3] = s[i];
            // Two columns are numbers and text, center them
            if (i >= s.Length - 5) {
               wsVerify.Columns[i + 3].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
               wsVerify.Columns[i + 3].NumberFormat = "@";
            }
         }
         wsVerifyRow = 2;

      }

      // Add a traffic entry.  Flag entries that differ from the Hitachi Spec
      private void AddTrafficEntry() {
         // Set the Traffic worksheet as active
         wsTraffic.Activate();

         // Set the time and elapsed time
         excelApp.Cells[wsTrafficRow, 1] = pkt.When.ToString("yy/MM/dd HH:mm:ss.ffff");
         elapsed = pkt.When - lastTraffic;
         excelApp.Cells[wsTrafficRow, 2] = (elapsed.TotalMilliseconds / 1000f).ToString("0.000");
         lastTraffic = pkt.When;
         string[] s = pkt.Data.Split('\t');
         for (int i = 0; i < s.Length; i++) {
            excelApp.Cells[wsTrafficRow, i + 3] = s[i];
            switch (i) {
               case 0:
               case 1:
                  if (s[i] == "False") {
                     excelApp.Cells[wsTrafficRow, i + 3].Interior.Color = Color.LightYellow;
                  }
                  break;
            }
         }
         wsTrafficRow++;
      }

      // Add a lod entry
      private void AddLogEntry() {
         // Set the Traffic worksheet as active
         wsTraffic.Activate();
         wsTraffic.Cells[wsTrafficRow, 1] = pkt.When.ToString("yy/MM/dd HH:mm:ss.ffff");
         switch (pkt.Data[0]) {
            case 'C':
               elapsed = pkt.When - lastConnect;
               lastConnect = pkt.When;
               break;
            case 'S':
               elapsed = pkt.When - lastSession;
               lastSession = pkt.When;
               break;
            case 'F':
               elapsed = pkt.When - lastForward;
               lastForward = pkt.When;
               lastTraffic = pkt.When;
               break;
            default:
               elapsed = pkt.When - lastTraffic;
               lastTraffic = pkt.When;
               break;
         }
         wsTraffic.Cells[wsTrafficRow, 2] = (elapsed.TotalMilliseconds / 1000f).ToString("0.0000");
         wsTraffic.Cells[wsTrafficRow, 3] = "N/A";
         wsTraffic.Cells[wsTrafficRow, 4] = "N/A";
         wsTraffic.Cells[wsTrafficRow, 5] = pkt.Data;

         wsTrafficRow++;

      }

      // Add the results of a verify operation
      private void AddVerifyEntry() {
         // Set the Traffic worksheet as active
         wsVerify.Activate();
         wsVerify.Cells[wsVerifyRow, 1] = pkt.When.ToString("yy/MM/dd HH:mm:ss.ffff");
         string[] s = pkt.Data.Split('\t');
         for (int i = 0; i < s.Length; i++) {
            wsVerify.Cells[wsVerifyRow, i + 3] = s[i];
         }
         if(s[s.Length - 2] == s[s.Length - 1]) {
            wsVerify.Cells[wsVerifyRow, 2] = "True";
         } else {
            wsVerify.Cells[wsVerifyRow, 2] = "False";
            wsVerify.Cells[wsVerifyRow, 2].Interior.Color = Color.LightYellow;
         }

         wsVerifyRow++;
      }


      // Close out the application
      private void CloseExcelApplication(bool View) {
         if (excelApp != null) {
            if (View) {
               // Set the Verify worksheet as active
               wsVerify.Activate();
               // Make a table out of the Verify data
               Excel.Range SourceRange = (Excel.Range)wsVerify.get_Range("A1", $"J{wsVerifyRow - 1}");
               SourceRange.Worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange,
               SourceRange, System.Type.Missing, Excel.XlYesNoGuess.xlYes, System.Type.Missing).Name = "Verify";
               SourceRange.Worksheet.ListObjects["Verify"].TableStyle = "TableStyleMedium2";
               wsVerify.Columns.AutoFit();

               // Set the Traffic worksheet as active
               wsTraffic.Activate();
               // Make a table out of the traffic data
               SourceRange = (Excel.Range)wsTraffic.get_Range("A1", $"N{wsTrafficRow - 1}");
               SourceRange.Worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange,
               SourceRange, System.Type.Missing, Excel.XlYesNoGuess.xlYes, System.Type.Missing).Name = "Traffic";
               SourceRange.Worksheet.ListObjects["Traffic"].TableStyle = "TableStyleMedium2";
               wsTraffic.Columns.AutoFit();

               // Save it away
               TrafficFileName = EIP.CreateFileName(TrafficFolder, "Traffic", "xlsx");
               excelApp.ActiveWorkbook.SaveAs(TrafficFileName);
            }
            wsVerify = null;
            wsTraffic = null;
            wb.Close();
            excelApp.Quit();
            Marshal.ReleaseComObject(wb);
            Marshal.ReleaseComObject(excelApp);
            wb = null;
            excelApp = null;
         }
      }

      #endregion

   }

   // Packet for capturing Log/traffic entries
   public class TrafficPkt {

      #region Constructors, Destructors, and Properties

      public Traffic.TaskType Type { get; set; }
      public string Data { get; set; } = string.Empty;
      public DateTime When { get; set; } = DateTime.Now;
      public bool View { get; set; } = false;

      public TrafficPkt(Traffic.TaskType Type, string Data) {
         this.Type = Type;
         this.Data = Data;
      }

      public TrafficPkt(Traffic.TaskType Type, bool View) {
         this.Type = Type;
         this.View = View;
      }

      public TrafficPkt(Traffic.TaskType Type) {
         this.Type = Type;
      }

      #endregion

   }

}
