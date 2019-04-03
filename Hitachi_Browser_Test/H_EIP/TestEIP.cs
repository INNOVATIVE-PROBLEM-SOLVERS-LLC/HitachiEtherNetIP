using System;
using System.Windows.Forms;
using EIP_Base;

namespace H_EIP {
   public partial class TestEIP : Form {

      Browser browser = null;
      EIP EIP = null;

      public TestEIP() {
         InitializeComponent();
      }

      private void TestEIP_Load(object sender, EventArgs e) {
         // Comment out next line if browser not needed
         browser = new Browser("192.168.0.1", 44818, @"C:\Temp\EIP");
         if (browser == null) {
            EIP = new EIP("192.168.0.1", 44818, @"C:\Temp\EIP");
         } else {
            EIP = browser.EIP;
         }
         cbFont.Items.AddRange(EIP.DropDowns[(int)fmtDD.FontType]);
      }

      private void cmdViewTraffic_Click(object sender, EventArgs e) {
         EIP.CloseExcelFile(true);
      }

      private void cmdStartBrowser_Click(object sender, EventArgs e) {
         browser?.ShowDialog();
      }

      private void cmdExit_Click(object sender, EventArgs e) {
         Application.Exit();
      }

      private void TestEIP_FormClosing(object sender, FormClosingEventArgs e) {
         EIP.CloseExcelFile(false);
         EIP = null;
         browser = null;
      }

      // Create a simple message
      private void cmdTest_Click(object sender, EventArgs e) {
         if (EIP.StartSession()) {    // Open a session
            if (EIP.ForwardOpen()) {  // open a data forwarding path
               int cols = EIP.GetAttribute(ccPF.Number_Of_Columns); // Get the number of columns
               EIP.SetAttribute(ccIDX.Automatic_reflection, 1); // Stack up all the operations
               if (cols > 1) { // No need to delete columns if there is only one
                  EIP.SetAttribute(ccIDX.Column, 1); // Select column 2 (0 origin on deletes)
                  for (int i = 2; i <= cols; i++)
                     EIP.ServiceAttribute(ccPF.Delete_Column); // Keep deleting column 2
               }
               EIP.SetAttribute(ccIDX.Item, 1);      // Set column 1(1 origin on Line Count)
               EIP.SetAttribute(ccPF.Line_Count, 1); // Set to 1 line
               for (int i = 2; i <= 5; i++)          // Add four more columns
                  EIP.ServiceAttribute(ccPF.Add_Column);
               EIP.SetAttribute(ccIDX.Item, 2);      // Stack column 2
               EIP.SetAttribute(ccPF.Line_Count, 2);
               EIP.SetAttribute(ccIDX.Item, 4);      // Stack column 4
               EIP.SetAttribute(ccPF.Line_Count, 2);
               for (int i = 1; i <= 7; i++) {
                  EIP.SetAttribute(ccIDX.Item, i);   // Select item
                  if (i == 1 || i == 4 || i == 7) {  // Set the font and text
                     EIP.SetAttribute(ccPF.Print_Character_String, $"{i}");
                     EIP.SetAttribute(ccPF.Dot_Matrix, "12x16");
                  } else {
                     EIP.SetAttribute(ccPF.Print_Character_String, $" {i} ");
                     EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
                  }
               }
               // Execute all the operations
               EIP.SetAttribute(ccIDX.Automatic_reflection, 0);
               EIP.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            }
            EIP.ForwardClose(); // Must be outside the ForwardOpen if block
         }
         EIP.EndSession();      // Must be outside the StartSession if block
      }

   }
}
