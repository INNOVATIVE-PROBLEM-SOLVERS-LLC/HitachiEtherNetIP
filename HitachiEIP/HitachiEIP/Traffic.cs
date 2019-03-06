using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace HitachiEIP {

   public class Traffic {

      // Saving data to an excel spreadsheet is time consuming since
      // the Excel Application is implemented in C++ and uses marshalling.
      // So, run the saving in another thread

      #region Data Declarations

      // Different steps in creating the traffic excel spreadsheet.
      public enum TaskType {
         Create = 0,
         AddTraffic = 1,
         AddLog = 2,
         Close = 3,
         View = 4,
         Exit = 5,
      }

      // Do the work in the background
      Thread t;

      // Use Blocking Collection to avoid spin waits
      public BlockingCollection<TrafficPkt> Tasks = new BlockingCollection<TrafficPkt>();

      // Declare the spreadsheet variables
      Excel.Application excelApp = null;
      Excel.Workbook wb = null;
      Excel.Worksheet wsTraffic = null;
      Excel.Worksheet wsLog = null;
      int wsTrafficRow;
      int wsLogRow;

      // Use for calculating elapsed time
      DateTime lastTraffic = DateTime.Now;
      DateTime lastLog = DateTime.Now;

      #endregion

      #region Constructon and only method

      public Traffic() {
         // Set the time and elapsed time for the others
         t = new Thread(processTasks);
         t.Start();
      }

      private void processTasks() {
         bool done = false;
         // Just one big loop
         while (!done) {
            // Wait for the next packet to arrive
            TrafficPkt pkt = Tasks.Take();
            switch (pkt.Type) {
               case TaskType.Create:
                  // Create the Excel application with two work sheets
                  excelApp = new Excel.Application();
                  excelApp.DisplayAlerts = false;
                  wb = excelApp.Workbooks.Add(Missing.Value);
                  // One worksheet is free
                  wsTraffic = wb.Sheets[1];
                  wsTraffic.Name = "Traffic";
                  wsTrafficRow = 1;
                  // Create the second worksheet
                  //wsLog = excelApp.Worksheets.Add(Type.Missing, excelApp.Worksheets[excelApp.Worksheets.Count], 1, Excel.XlSheetType.xlWorksheet);
                  wsLog = excelApp.Worksheets.Add(Type.Missing, wsTraffic, 1, Excel.XlSheetType.xlWorksheet);
                  wsLog.Name = "Log";
                  wsLogRow = 1;
                  break;
               case TaskType.AddTraffic:
                  // Set the Traffic worksheet as active
                  wsTraffic.Activate();

                  if (wsTrafficRow == 1) {
                     // Get the headers right for the first one
                     excelApp.Cells[wsTrafficRow, 1] = "Date/Time";
                     excelApp.Cells[wsTrafficRow, 2] = "Elapsed";
                  } else {
                     // Set the time and elapsed time for the others
                     excelApp.Cells[wsTrafficRow, 1] = pkt.When.ToString("yy/MM/dd HH:mm:ss.ffff");
                     TimeSpan elapsed = pkt.When - lastTraffic;
                     excelApp.Cells[wsTrafficRow, 2] = (elapsed.Milliseconds / 1000f).ToString("0.000");
                     lastTraffic = pkt.When;
                  }
                  string[] s = pkt.Data.Split('\t');
                  for (int i = 0; i < s.Length; i++) {
                     excelApp.Cells[wsTrafficRow, i + 3] = s[i];
                     if (wsTrafficRow == 1) {
                        switch (i) {
                           case 7:
                           case 8:
                           case 10:
                           case 11:
                              // These columns are numbers
                              break;
                           default:
                              // The rest are text
                              excelApp.Columns[i + 3].NumberFormat = "@";
                              break;
                        }
                     }
                  }
                  wsTrafficRow++;
                  break;
               case TaskType.AddLog:
                  // Set the Log worksheet as active
                  wsLog.Activate();

                  if (wsLogRow == 1) {
                     // Get the headers right for the first one
                     excelApp.Cells[wsLogRow, 1] = "Date/Time";
                     excelApp.Cells[wsLogRow, 2] = "Elapsed";
                  } else {
                     // Set the time and elapsed time for the others
                     excelApp.Cells[wsLogRow, 1] = pkt.When.ToString("yy/MM/dd HH:mm:ss.ffff");
                     TimeSpan elapsed = pkt.When - lastLog;
                     excelApp.Cells[wsLogRow, 2] = (elapsed.Milliseconds / 1000f).ToString("0.000");
                     lastLog = pkt.When;
                  }
                  excelApp.Cells[wsLogRow, 3] = pkt.Data;
                  wsLogRow++;
                  break;
               case TaskType.Close:
                  // Make a table for traffic
                  Excel.Range SourceRange = (Excel.Range)wsTraffic.get_Range("A1", $"P{wsTrafficRow - 1}");
                  SourceRange.Worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange,
                  SourceRange, System.Type.Missing, Excel.XlYesNoGuess.xlYes, System.Type.Missing).Name = "Traffic";
                  SourceRange.Worksheet.ListObjects["Traffic"].TableStyle = "TableStyleMedium2";
                  wsTraffic.Columns.AutoFit();

                  // Make a table for Log
                  SourceRange = (Excel.Range)wsLog.get_Range("A1", $"C{wsLogRow - 1}");
                  SourceRange.Worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange,
                  SourceRange, System.Type.Missing, Excel.XlYesNoGuess.xlYes, System.Type.Missing).Name = "Log";
                  SourceRange.Worksheet.ListObjects["Log"].TableStyle = "TableStyleMedium2";
                  wsLog.Columns.AutoFit();

                  // Make traffic visible when workbook opened
                  wsTraffic.Activate();

                  // Save it away
                  excelApp.ActiveWorkbook.SaveAs(pkt.Data);
                  wb.Close();
                  excelApp.Quit();
                  excelApp = null;
                  break;
               case TaskType.View:
                  // Open Excel
                  Process.Start(pkt.Data);
                  break;
               case TaskType.Exit:
                  // That's all folks
                  done = true;
                  break;
               default:
                  return;
            }
         }
      }

      #endregion

   }

   public class TrafficPkt {

      public Traffic.TaskType Type { get; set; }
      public string Data { get; set; }
      public DateTime When { get; set; }

      public TrafficPkt(Traffic.TaskType Type, string Data) {
         this.Type = Type;
         this.Data = Data;
         this.When = DateTime.Now;
      }
   }

}
