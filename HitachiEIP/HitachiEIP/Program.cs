using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitachiEIP {
   static class Program {
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
            Application.Run(new HitachiBrowser());
         } catch (Exception e) {
            MessageBox.Show($"Exception in Main: {e.Message}\r\n{e.StackTrace}", "Exception", MessageBoxButtons.OK);
            //Application.Exit();
         }
      }

      static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
         try {
            Exception ex = (Exception)e.ExceptionObject;
            MessageBox.Show($"Unhandled Exception: {ex.Message}\r\n{ex.StackTrace}", "Exception", MessageBoxButtons.OK);
         } catch {

         }
      }

      static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
         try {
            Exception ex = e.Exception;
            MessageBox.Show($"Unhandled Thread Exception \r\n{ex.Message}\r\n{ex.StackTrace}", "Exception", MessageBoxButtons.OK);
         } catch {

         }
      }
   }
}
