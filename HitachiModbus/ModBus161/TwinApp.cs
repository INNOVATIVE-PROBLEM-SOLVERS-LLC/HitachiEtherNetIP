using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModBus161 {

   // Twin Nozzle Application
   public class TwinApp {

      #region Data Declarations

      string fileName;
      public List<string> workSheets;
      public string[][] workSheetVariables;

      string error;
      OleDbConnection edbConnection = null;
      string[] edbConnectionString = { @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=Excel 12.0",
                                         @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=Excel 8.0" };
      
      #endregion

      #region Constructors and Destructors

      public TwinApp() {
      }

      #endregion

      #region Excel Spreadsheet handling

      // Prepare in case the user wants a different data source
      public enum dbArchitecture {
         Excel = 0,
         Access = 1,
         SQLServer = 2
      }
      // At the moment, Excel is all that is supported
      dbArchitecture architecture = dbArchitecture.Excel;

      // Open the database and retrieve worksheet and column information
      public bool Open(string fileName) {
         this.fileName = fileName;
         bool result = false;
         error = "(None)";
         try {
            edbConnection = ConnectToDatabase(architecture, fileName, out error);
            if (edbConnection != null && ReadSpreadSheetNames() && ReadVariableNames()) {
               error = "(None)";
               result = true;
            }
         } catch (Exception e) {
            error = e.Message;
         }
         return result;
      }

      // Close the database connection
      public void Close() {
         if (edbConnection != null) {
            if (edbConnection.State != ConnectionState.Closed) {
               edbConnection.Close();
            }
            edbConnection = null;
         }
      }

      // Establish the database connection
      internal static OleDbConnection ConnectToDatabase(dbArchitecture Architecture, string FileName, out string error) {
         string[] edbConnectionString = null;
         string ext = Path.GetExtension(FileName).ToUpper();
         switch (Architecture) {
            case dbArchitecture.Excel:
               if (ext.Length >= 4 && ext.Substring(0, 4) == ".XLS") {
                  edbConnectionString = new string[] {
                           @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0;IMEX=1;READONLY=TRUE""",
                           @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=""Excel 8.0;IMEX=1;READONLY=TRUE""" };
               }
               break;
            case dbArchitecture.Access:
               if (ext == ".MDB" || ext == ".ACCDB") {
                  edbConnectionString = new string[] {
                  @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};",
                  @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};" };
               }
               break;
            case dbArchitecture.SQLServer:
               if (ext == ".TXT") {
                  edbConnectionString = new string[] { System.IO.File.ReadAllText(FileName) };
               }
               break;
         }
         error = "(None)";
         OleDbConnection Connection = null;
         if (edbConnectionString == null) {
            error = "Selected database is not compatible with the selected Architecture";
         } else {
            for (int i = 0; i < edbConnectionString.Length; i++) {
               try {
                  Connection = new OleDbConnection(string.Format(edbConnectionString[i], FileName));
                  Connection.Open();
                  if (Connection.State == ConnectionState.Open) {
                     return Connection;
                  }
               } catch (Exception e) {
                  error = e.Message;
                  Connection = null;
               }
               Connection = null;
            }
         }
         return null;
      }

      // Get all worksheet names
      bool ReadSpreadSheetNames() {
         bool result = false;
         string schemaName;
         string tableName;
         try {
            using (DataTable schema = edbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null)) {
               if (schema != null) {
                  workSheets = new List<string>(schema.Rows.Count);
                  foreach (DataRow row in schema.Rows) {
                     schemaName = row["Table_Schema"].ToString();
                     tableName = row["Table_Name"].ToString();
                     switch (architecture) {
                        case dbArchitecture.Excel:
                           if (tableName.EndsWith("$") || tableName.EndsWith("$'")) {
                              workSheets.Add(tableName);
                           }
                           break;
                        case dbArchitecture.Access:
                           if (tableName.Length < 4 || tableName.Substring(0, 4) != "MSys") {
                              workSheets.Add(tableName);
                           }
                           break;
                        case dbArchitecture.SQLServer:
                           if (row[3].ToString() == "TABLE" || row[3].ToString() == "VIEW") {
                              workSheets.Add($"{schemaName}.{tableName}");
                           }
                           break;
                     }
                  }
                  result = true;
               }
            }
         } catch (Exception e) {
            workSheets = null;
            error = e.Message;
         }
         return result;
      }

      // Retrieve the column names for all worksheets
      bool ReadVariableNames() {
         error = "(None)";
         int k;
         string name;
         bool result = false;
         DataTable schema;
         try {
            workSheetVariables = new string[workSheets.Count][];
            for (int i = 0; i < workSheets.Count; i++) {
               k = 0;
               string[] s = workSheets[i].Split(new char[] { '.' });
               schema = edbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                   new object[] { null, null, s[s.Length - 1], null });
               workSheetVariables[i] = new string[schema.Rows.Count + 1];
               workSheetVariables[i][0] = "(None)";
               for (int j = 0; j < schema.Rows.Count; j++) {
                  if (s.Length == 1 || schema.Rows[j]["Table_Schema"].ToString() == s[0]) {
                     int n = Convert.ToInt32(schema.Rows[j]["Ordinal_Position"]);
                     name = schema.Rows[j]["Column_Name"].ToString();
                     switch (architecture) {
                        case dbArchitecture.Excel:
                           if (name != "F" + n.ToString()) {
                              workSheetVariables[i][k++] = name;
                           }
                           break;
                        case dbArchitecture.Access:
                        case dbArchitecture.SQLServer:
                           workSheetVariables[i][k++] = name;
                           break;
                     }
                  }
               }
               Array.Resize(ref workSheetVariables[i], k);
            }
            result = true;
         } catch (Exception e) {
            workSheetVariables = null;
            error = e.Message;
         }
         return result;
      }

      // Get a list of part numbers b ased on worksheet and primary key
      public string[] PartNumbers(string worksheet, string primaryKey) {
         string[] result = null;
         string SQL;
         try {
            if (edbConnection.State == ConnectionState.Open && worksheet.Length > 0
                && primaryKey.Length > 0) {
               using (DataTable dt = new DataTable()) {
                     SQL = $"SELECT DISTINCT A.[{primaryKey}] FROM {TableName(worksheet)} AS A WHERE NOT A.[{primaryKey}] IS NULL";
                  SQL += $" ORDER BY A.[{primaryKey}]";
                  using (OleDbCommand cmd = new OleDbCommand(SQL, edbConnection)) {
                     dt.Load(cmd.ExecuteReader());
                     result = (from DataRow r in dt.Rows select r[0].ToString().ToUpper()).ToArray();
                  }
               }
            }
         } catch (Exception e) {
            error = e.Message;
            result = null;
         }
         return result;
      }

      // Resolve schema names
      private string TableName(string WorkSheet) {
         string result;
         string[] s = WorkSheet.Split(new char[] { '.' });
         if (s.Length == 2) {
            result = $"[{s[0]}].[{s[1]}]";
         } else {
            result = $"[{WorkSheet}]";
         }
         return result;
      }

      #endregion

   }
}
