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

      // Different steps in creating the traffic excel spreadsheet.
      public enum TaskType {
         Create = 0,
         Add = 1,
         Close = 2,
         View = 3,
         Exit = 4,
      }

      // Do the work in the background
      Thread t;

      // First try at blocking collections
      public BlockingCollection<TrafficPkt> Tasks = new BlockingCollection<TrafficPkt>();

      Excel.Application excelApp = null;
      Excel.Workbook wb = null;
      Excel.Worksheet ws = null;
      int wsRow;

      public Traffic() {
         t = new Thread(processTasks);
         t.Start();
      }

      private void processTasks() {
         bool done = false;
         TrafficPkt pkt;
         while(!done) {
            int z = 1;
            pkt = Tasks.Take();
            switch (pkt.Type) {
               case TaskType.Create:
                  excelApp = new Excel.Application();
                  excelApp.DisplayAlerts = false;
                  wb = excelApp.Workbooks.Add(Missing.Value);
                  ws = wb.ActiveSheet;
                  ws.Name = "HitachiBrowser";
                  wsRow = 1;
                  break;
               case TaskType.Add:
                  string[] s = pkt.Data.Split('\t');
                  for (int i = 0; i < s.Length; i++) {
                     excelApp.Cells[wsRow, i + 1] = s[i];
                     switch (i) {
                        case 7:
                        case 8:
                        case 10:
                        case 11:
                           // These columns are numbers
                           break;
                        default:
                           // The rest are text
                           excelApp.Columns[i + 1].NumberFormat = "@";
                           break;
                     }
                  }
                  wsRow++;
                  break;
               case TaskType.Close:
                  FormatAsTable();
                  excelApp.ActiveWorkbook.SaveAs(pkt.Data);
                  wb.Close();
                  excelApp.Quit();
                  excelApp = null;
                  break;
               case TaskType.View:
                  Process.Start(pkt.Data);
                  break;
               case TaskType.Exit:
                  done = true;
                  break;
               default:
                  return;
            }
         }
      }

      private void FormatAsTable() {
         Excel.Range SourceRange = (Excel.Range)ws.get_Range("A1", $"N{wsRow - 1}");
         SourceRange.Worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange,
         SourceRange, System.Type.Missing, Excel.XlYesNoGuess.xlYes, System.Type.Missing).Name = "Traffic";
         SourceRange.Worksheet.ListObjects["Traffic"].TableStyle = "TableStyleMedium2";
         ws.Columns.AutoFit();
      }

   }

   public class TrafficPkt {

      public Traffic.TaskType Type { get; set; }
      public string Data { get; set; }

      public TrafficPkt(Traffic.TaskType Type, string Data) {
         this.Type = Type;
         this.Data = Data;
      }
   }

}
