using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace H_EIP {
   static class Program {
      // Data definitions
      static internal EIP_Lib.Browser ThisProgram;
      static HitachiEIP.Properties.Settings p;
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main() {
         p = HitachiEIP.Properties.Settings.Default;
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         ThisProgram = new EIP_Lib.Browser(p.IPAddress, p.IPPort, p.TrafficFolder, p.MessageFolder);
         ThisProgram.FormClosing += cijConnect_FormClosing;
         Application.Run(ThisProgram);
      }

      internal static void cijConnect_FormClosing(object sender, FormClosingEventArgs e) {
         p.IPAddress = ThisProgram.IPAddress;
         p.IPPort = ThisProgram.IPPort;
         p.TrafficFolder = ThisProgram.TrafficFolder;
         p.MessageFolder = ThisProgram.MessageFolder;
         p.Save();
      }
   }
}
