using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HIES.IJP.RX;

namespace IJPLib_Test {
   public partial class IJPTest : Form {

      #region Data Declarations

      // Get the current message.
      IJPMessage message = null;

      bool comOn = false;

      #endregion

      #region Constructors and Destructors

      private IJP ijp;

      public IJPTest() {
         InitializeComponent();
      }

      ~IJPTest() {

      }

      #endregion

      #region Form level events


      private void IJPTest_Load(object sender, EventArgs e) {
         setButtonEnables();
      }

      #endregion

      #region Form Control Events

      private void cmdConnect_Click(object sender, EventArgs e) {
         if (null == this.ijp) {
            // Connect to the printer
            ConnectIJP();
            // Get com on
            cmdComOn_Click(null, null);
            // Set Caption
            this.cmdConnect.Text = "Disconnect";
         } else {
            // Turn com off
            cmdComOff_Click(null, null);
            // Disconnect from printer
            DisconnectIJP();
            // Set caption
            this.cmdConnect.Text = "Connect";
         }
      }

      private void cmdComOn_Click(object sender, EventArgs e) {
         this.ijp.SetComPort(IJPOnlineStatus.Online);
         comOn = true;
         setButtonEnables();
      }

      private void cmdComOff_Click(object sender, EventArgs e) {
         this.ijp.SetComPort(IJPOnlineStatus.Offline);
         comOn = false;
         setButtonEnables();
      }

      private void cmdDump_Click(object sender, EventArgs e) {
         //  Set hour glass
         Cursor.Current = Cursors.WaitCursor;
         // Out with the old
         ivMessage.Text = string.Empty;
         tvMessage.Nodes.Clear();
         // In with the new
         ShowCurrentMessage();
         // Generate the views
         ObjectDumper od = new ObjectDumper(2);
         string indentedView;
         TreeNode treeNode;
         od.Dump(message, out indentedView, out treeNode);
         // Display the viewd
         ivMessage.Text = indentedView;
         tvMessage.Nodes.Add(treeNode);
         tvMessage.ExpandAll();
         // Restore uurser
         Cursor.Current = Cursors.Arrow;
      }

      #endregion

      #region Service routines

      private void ConnectIJP() {
         ConnectIJP(this.ipAddressTextBox.Text, 5000, 5);
      }

      private void ConnectIJP(string ipAddress, int timeout, int retry) {
         if (null != this.ijp) {
            DisconnectIJP();
            this.ijp = null;
         }
         try {

            // Create the IJP object.
            this.ijp = new IJP();

            // Set parameters.
            this.ijp.IPAddress = ipAddress;
            this.ijp.Timeout = timeout;
            this.ijp.Retry = retry;

            // Connect the Ink jet printer.
            this.ijp.Connect();
         } catch (Exception e) {

         }
         setButtonEnables();
      }

      private void DisconnectIJP() {
         if (null != this.ijp) {
            this.ijp.Disconnect();
            this.ijp = null;
         }
         setButtonEnables();
      }

      private void ShowCurrentMessage() {
         try {
            // Get the current message.
            message = (IJPMessage)this.ijp.GetMessage();
         } catch (Exception e) {

         }
         setButtonEnables();
      }

      private void setButtonEnables() {
         bool connected = ijp != null;
         // These must connect first
         cmdComOn.Enabled = connected && !comOn;
         cmdComOff.Enabled = connected && comOn;
         cmdDump.Enabled = connected && comOn;
      }

      #endregion

   }
}
