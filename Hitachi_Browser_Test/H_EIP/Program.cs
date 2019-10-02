using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H_EIP {
   static class Program {

      static internal TestEIP ThisProgram;
      static Properties.Settings p;
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main() {
         p = Properties.Settings.Default;
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         ThisProgram = new TestEIP(p.IPAddress, p.Port);
         ThisProgram.FormClosing += TestEIP_FormClosing;
         Application.Run(ThisProgram);
      }

      internal static void TestEIP_FormClosing(object sender, FormClosingEventArgs e) {
         p.IPAddress = ThisProgram.txtIPAddress.Text;
         p.Port = ThisProgram.txtPort.Text;
         p.Save();
      }

   }
}
