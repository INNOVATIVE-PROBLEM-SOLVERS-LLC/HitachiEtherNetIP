using System;
using System.Windows.Forms;
using EIP_Lib;

namespace H_EIP {
   public partial class TestEIP : Form {

      Browser browser = null;
      EIP EIP = null;

      public TestEIP(string IPAddress, string Port) {
         InitializeComponent();
         txtIPAddress.Text = IPAddress;
         txtPort.Text = Port;
      }

      private void TestEIP_Load(object sender, EventArgs e) {
         // Comment out next line if browser not needed
         browser = new Browser(txtIPAddress.Text, 44818, @"C:\Temp\EIP", @"C:\GitHubEtherNetIP\Messages");
         if (browser == null) {
            // Get a new EtherNet/IP instance
            EIP = new EIP(txtIPAddress.Text, 44818, @"C:\Temp\EIP");
         } else {
            // Use the instance from the browser
            EIP = browser.EIP;
         }
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
         EIP.UseAutomaticReflection = true; // Speed up processing
         if (EIP.StartSession(true)) {      // Open a session
            if (EIP.ForwardOpen()) {        // open a data forwarding path
               try {
                  EIP.DeleteAllButOne();                     // Clear the printer
                  for (int i = 2; i <= 5; i++) {             // Add four more columns
                     EIP.ServiceAttribute(ccPF.Add_Column);
                  }
                  EIP.SetAttribute(ccIDX.Item, 2);           // Stack column 2
                  EIP.SetAttribute(ccPF.Line_Count, 2);
                  EIP.SetAttribute(ccIDX.Item, 4);           // Stack column 4
                  EIP.SetAttribute(ccPF.Line_Count, 2);
                  for (int i = 1; i <= 7; i++) {
                     EIP.SetAttribute(ccIDX.Item, i);        // Select item
                     if (i == 1 || i == 4 || i == 7) {       // Set the font and text
                        EIP.SetAttribute(ccPF.Print_Character_String, $"{i}");
                        EIP.SetAttribute(ccPF.Dot_Matrix, "12x16");
                     } else {
                        EIP.SetAttribute(ccPF.Print_Character_String, $" {i} ");
                        EIP.SetAttribute(ccPF.Dot_Matrix, "5x8");
                     }
                  }
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
               } catch (Exception e2) {
                  // You are on your own here
               }
            }
            EIP.ForwardClose(); // Must be outside the ForwardOpen if block
         }
         EIP.EndSession();      // Must be outside the StartSession if block
      }

      private void cmdBrowse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = txtMessageFolder.Text };
         if (dlg.ShowDialog() == DialogResult.OK) {
            txtMessageFolder.Text = dlg.SelectedPath;
         }
      }

      public void Log(string msg) {
         EIP?.LogIt(msg);
      }

   }
}
