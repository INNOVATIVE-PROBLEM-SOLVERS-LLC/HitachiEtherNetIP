using System;
using System.Windows.Forms;

namespace H_EIP {
   static class Program {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main() {
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         Application.Run(new EIP_Lib.Browser("192.168.126.100", 44818, @"c:\Temp\EIP"));
      }
   }
}
