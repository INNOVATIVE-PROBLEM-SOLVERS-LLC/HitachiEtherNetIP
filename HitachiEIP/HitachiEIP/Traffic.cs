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

      public enum TaskType {
         Create = 0,
         Add = 1,
         Close = 2,
         View = 3,
         Exit = 4,
      }

      BlockingCollection<TrafficPkt> tasks = new BlockingCollection<TrafficPkt>();
      string fileName = string.Empty;

      Excel.Application excelApp = null;
      Excel.Workbook wb = null;
      Excel.Worksheet ws = null;
      int wsRow;

      public Traffic() {

      }

      private void processTasks() {
         TrafficPkt n = tasks.Take();
         switch (n.Type) {
            case TaskType.Create:
               excelApp = new Excel.Application();
               excelApp.DisplayAlerts = false;
               wb = excelApp.Workbooks.Add(Missing.Value);
               ws = wb.ActiveSheet;
               ws.Name = "HitachiBrowser";
               wsRow = 1;
               break;
            case TaskType.Add:
               string[] s = n.Data.Split('\t');
               for (int i = 0; i < s.Length; i++) {
                  excelApp.Cells[wsRow, i + 1] = s[i];
                  switch (i) {
                     case 7:
                     case 8:
                     case 10:
                     case 11:
                        break;
                     default:
                        excelApp.Columns[i + 1].NumberFormat = "@";
                        break;
                  }
               }
               wsRow = 2;
               break;
            case TaskType.Close:
               FormatAsTable();
               string ExcelFileName = n.Data;
               excelApp.ActiveWorkbook.SaveAs(ExcelFileName);
               wb.Close();
               excelApp.Quit();
               excelApp = null;
               break;
            case TaskType.View:
               Process.Start(fileName);
               break;
            case TaskType.Exit:

               break;
            default:

               break;
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
