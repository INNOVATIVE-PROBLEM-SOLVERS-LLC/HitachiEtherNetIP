using System;
using System.IO;
using System.Windows.Forms;

namespace ModBus161 {
   static class Program {
      static internal UI161 ThisProgram;
      private static TextWriter twError = null;
      static Properties.Settings p;


      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main() {
         Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
         AppDomain.CurrentDomain.UnhandledException +=
            new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
         Application.ThreadException +=
            new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         try {
            ThisProgram = new UI161();
            p = Properties.Settings.Default;
            ThisProgram.txtIPAddress.Text = p.IPAddress;
            ThisProgram.txtIPPort.Text = p.IPPort;
            ThisProgram.txtMessageFolder.Text = p.MessageFolder;
            ThisProgram.txtDataAddress.Text = p.HexAddress;
            ThisProgram.txtDataLength.Text = p.Length;
            ThisProgram.txtData.Text = p.Data;
            ThisProgram.optHoldingRegister.Checked = p.HoldingReg;
            ThisProgram.chkTwinNozzle.Checked = p.TwinNozzle;
            ThisProgram.cbNozzle.SelectedIndex = p.Nozzle;
            ThisProgram.chkHex.Checked = p.HexData;
            ThisProgram.chkLogIO.Checked = p.LogIO;
            ThisProgram.FormClosing += UI161_FormClosing;
            Application.Run(ThisProgram);
         } catch (Exception ex) {
            try {
               GetTextWriter(ex);
               if (twError != null) {
                  twError.WriteLine(DateTime.Now + " Exception in Main: " + ex.ToString());
                  twError.Flush();
                  twError.Close();
                  twError = null;
               }
            } finally {
               if (twError != null) {
                  twError.Flush();
                  twError.Close();
               }
            }
            MessageBox.Show("Exception in Main: " + ex.Message);
            Application.Exit();
         }
      }

      static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
         try {
            Exception ex = (Exception)e.ExceptionObject;
            GetTextWriter(e);
            if (twError != null) {
               twError.WriteLine("UnhandledException " + DateTime.Now + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
               twError.Flush();
               twError.Close();
               twError = null;
            }
         } finally {
            if (twError != null) {
               twError.Flush();
               twError.Close();
            }
         }
      }

      static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
         try {
            GetTextWriter(e);
            if (twError != null) {
               twError.WriteLine(ThisProgram.Text);
               twError.WriteLine("UnhandledThreadException " + DateTime.Now + "\r\n" + e.Exception.Message + "\r\n" + e.Exception.StackTrace);
               twError.Flush();
               twError.Close();
               twError = null;
            }
         } finally {
            if (twError != null) {
               twError.Flush();
               twError.Close();
            }
         }
      }

      static void GetTextWriter(object ex) {
         if (twError == null) {
            try {
               string logDir = ThisProgram.txtMessageFolder.Text;
               if (Directory.Exists(logDir)) {
                  twError = new StreamWriter(logDir + @"\Error" + String.Format("{0:yyyyMMddHHmmss}" + ".log", DateTime.Now), true);
                  ThisProgram.Log("Error File Generated!");
               } else {
                  MessageBox.Show("Exception(No Log File): " + ex.ToString());
               }
            } catch {
               twError = null;
            }
         }
      }

      internal static void UI161_FormClosing(object sender, FormClosingEventArgs e) {
         ThisProgram.FormClosing -= UI161_FormClosing;
         p.IPAddress = ThisProgram.txtIPAddress.Text;
         p.IPPort = ThisProgram.txtIPPort.Text;
         p.MessageFolder = ThisProgram.txtMessageFolder.Text;
         p.HexAddress = ThisProgram.txtDataAddress.Text;
         p.Length = ThisProgram.txtDataLength.Text;
         p.Data = ThisProgram.txtData.Text;
         p.HoldingReg = ThisProgram.optHoldingRegister.Checked;
         p.TwinNozzle = ThisProgram.chkTwinNozzle.Checked;
         p.Nozzle = ThisProgram.cbNozzle.SelectedIndex;
         p.HexData = ThisProgram.chkHex.Checked;
         p.LogIO = ThisProgram.chkLogIO.Checked;
         p.Save();
      }

   }
}
