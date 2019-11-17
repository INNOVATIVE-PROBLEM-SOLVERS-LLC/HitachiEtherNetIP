using System;
using System.IO;
using System.Windows.Forms;

namespace IJPLib_Test {
   static class Program {
      static internal IJPTest ThisProgram;
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
            ThisProgram = new IJPTest();
            p = Properties.Settings.Default;
            ThisProgram.ipAddressTextBox.Text = p.IPAddress;
            ThisProgram.txtMessageFolder.Text = p.MessageFolder;
            ThisProgram.txtLogFolder.Text = p.LogFolder;
            ThisProgram.FormClosing += IJPTest_FormClosing;
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
               string logDir = ThisProgram.txtLogFolder.Text;
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

      internal static void IJPTest_FormClosing(object sender, FormClosingEventArgs e) {
         p.IPAddress = ThisProgram.ipAddressTextBox.Text;
         p.MessageFolder = ThisProgram.txtMessageFolder.Text;
         p.LogFolder = ThisProgram.txtLogFolder.Text;
         p.Save();
      }

   }
}
