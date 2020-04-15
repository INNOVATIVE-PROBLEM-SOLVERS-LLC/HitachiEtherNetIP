using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace EIP_Lib {
   public partial class XML {

      #region Data Declarations

      readonly Browser parent;

      readonly EIP EIP;
      readonly TabPage tab;

      // Tab Controls
      TabControl tclViewXML;
      TabPage tabTreeView;
      TabPage tabIndented;
      TabPage tabVerify;
      TabPage tabTraffic;

      TreeView tvXML;
      TextBox txtIndentedView;

      // Operating Buttons
      Button cmdOpen;
      Button cmdClear;
      Button cmdRetrieve;
      Button cmdSaveAs;
      Button cmdVerify;
      Button cmdSend;

      // Testing Buttons
      Label lblSelectXmlTest;
      ComboBox cbAvailableXmlTests;
      Button cmdBrowse;
      Button cmdSendFileToPrinter;
      Button cmdSendDisplayToPrinter;
      CheckBox chkIJPLibNames;
      CheckBox chkErrorsOnly;
      CheckBox chkTraffic;

      Label lblSelectHardTest;
      ComboBox cbAvailableHardTests;
      Button cmdRunHardTest;

      // Verify data Grid
      DataGridView VerifyView;

      DataGridViewColumn V_XMLName;
      DataGridViewColumn V_Class;
      DataGridViewColumn V_Attribute;
      DataGridViewColumn V_Item;
      DataGridViewColumn V_Block;
      DataGridViewColumn V_SubRule;
      DataGridViewColumn V_DataOut;
      DataGridViewColumn V_DataIn;

      // Traffic data Grid
      DataGridView TrafficView;

      DataGridViewColumn T_Access;
      DataGridViewColumn T_Class;
      DataGridViewColumn T_Attribute;
      DataGridViewColumn T_NOut;
      DataGridViewColumn T_DataOut;
      DataGridViewColumn T_RawOut;
      DataGridViewColumn T_NIn;
      DataGridViewColumn T_DataIn;
      DataGridViewColumn T_RawIn;


      // XML Processing
      string XMLFileName = string.Empty;
      string XMLText = string.Empty;
      XmlDocument xmlDoc = null;
      enum ItemType {
         Unknown = 0,
         Text = 1,
         Date = 2,
         Counter = 3,
         Logo = 4,
         Link = 5,     // Not supported in the printer
         Prompt = 6,   // Not supported in the printer
      }

      // Make things easier to read
      readonly Font courier = new Font("Courier New", 9);

      #endregion

      #region Constructors and destructors

      // Create class
      public XML(Browser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;

         BuildControls();

         BuildTestFileList();

         SetButtonEnables();
      }

      #endregion

      #region Form Control Events

      // Open a new XML file
      private void Open_Click(object sender, EventArgs e) {
         // Clear out any currently loaded file
         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.Title = "Select XML formatted file!";
            dlg.Filter = "XML (*.xml)|*.xml|All (*.*)|*.*";
            DialogResult dlgResult = DialogResult.Retry;
            while (dlgResult == DialogResult.Retry) {
               dlgResult = dlg.ShowDialog();
               if (dlgResult == DialogResult.OK) {
                  try {
                     XMLFileName = dlg.FileName;
                     XMLText = File.ReadAllText(dlg.FileName);
                     ProcessLabel(XMLText);
                  } catch (Exception ex) {
                     MessageBox.Show(parent, ex.Message, "Cannot load XML File!");
                  }
               }
            }
         }
         SetButtonEnables();
      }

      // Clear out the screens
      private void Clear_Click(object sender, EventArgs e) {
         if (tclViewXML.SelectedTab == tabTraffic) {
            TrafficView.Rows.Clear();
         } else if (tclViewXML.SelectedTab == tabVerify) {
            VerifyView.Rows.Clear();
         } else if (tclViewXML.SelectedTab == tabTreeView | tclViewXML.SelectedTab == tabIndented) {
            txtIndentedView.Text = string.Empty;
            xmlDoc = null;
            tvXML.Nodes.Clear();
            XMLText = string.Empty;
            XMLFileName = string.Empty;
         }
         SetButtonEnables();
      }

      // Save the Retrieved XML file
      private void SaveAs_Click(object sender, EventArgs e) {
         DialogResult dlgResult;

         using (SaveFileDialog saveFileDialog1 = new SaveFileDialog()) {
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "xml";
            saveFileDialog1.Filter = "xml|*.xml";
            saveFileDialog1.Title = "Save Printer Layout to XML file";
            saveFileDialog1.FileName = XMLFileName;
            dlgResult = saveFileDialog1.ShowDialog();
            if (dlgResult == DialogResult.OK && !String.IsNullOrEmpty(saveFileDialog1.FileName)) {
               XMLFileName = saveFileDialog1.FileName;
               Stream outfs = new FileStream(XMLFileName, FileMode.Create);
               // Might have some possibilities here <TODO>
               outfs.Write(EIP.Encode.GetBytes(XMLText), 0, XMLText.Length);
               outfs.Flush();
               outfs.Close();
               outfs.Dispose();
            }
         }
         SetButtonEnables();
      }

      #endregion

      #region Event Handlers

      private void EIP_VerifyEvent(EIP sender, string msg) {
         string[] v = msg.Split('\t');
         VerifyView.Rows.Add(v);
         VerifyView.FirstDisplayedScrollingRowIndex = VerifyView.RowCount - 1;
      }

      private void EIP_TrafficRes(EIP sender, string msg) {
         string[] t = msg.Split('\t');
         TrafficView.Rows.Add(t);
         TrafficView.FirstDisplayedScrollingRowIndex = TrafficView.RowCount - 1;
      }

      private void TrafficView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e) {
         if ((string)TrafficView.Rows[e.RowIndex].Cells[2].Value == "Automatic reflection"
            || (string)TrafficView.Rows[e.RowIndex].Cells[2].Value == "Start Stop Management Flag") {
            TrafficView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Pink;
         } else {
            switch (TrafficView.Rows[e.RowIndex].Cells[0].Value) {
               case "Get":
                  TrafficView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                  break;
               case "Set":
                  TrafficView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                  break;
               case "Service":
                  TrafficView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Cyan;
                  break;
            }
         }
      }

      private void ChkTraffic_CheckedChanged(object sender, EventArgs e) {
         if (chkTraffic.Checked) {
            EIP.TrafficRes += EIP_TrafficRes;
         } else {
            EIP.TrafficRes -= EIP_TrafficRes;
         }
      }

      #endregion

      #region Service Routines

      // Build XML page controls
      private void BuildControls() {
         tclViewXML = new TabControl() { Name = "tclViewXML", Font = courier };
         tabTreeView = new TabPage() { Name = "tabTreeView", Text = "Tree View" };
         tabIndented = new TabPage() { Name = "tabIndented", Text = "Indented View" };

         tvXML = new TreeView() { Name = "tvXML", Font = courier };
         txtIndentedView = new TextBox() { Name = "txtIndentedView", Font = courier, Multiline = true, ScrollBars = ScrollBars.Both };

         cmdOpen = new Button() { Text = "Open" };
         cmdOpen.Click += Open_Click;

         cmdClear = new Button() { Text = "Clear" };
         cmdClear.Click += Clear_Click;

         cmdRetrieve = new Button() { Text = "Retrieve" };
         cmdRetrieve.Click += Retrieve_Click;

         cmdSaveAs = new Button() { Text = "Save As" };
         cmdSaveAs.Click += SaveAs_Click;

         cmdSendFileToPrinter = new Button() { Text = "File To Printer" };
         cmdSendFileToPrinter.Click += SendFileToPrinter_Click;

         cmdSendDisplayToPrinter = new Button() { Text = "Display To Printer" };
         cmdSendDisplayToPrinter.Click += SendDisplayToPrinter_Click;

         chkIJPLibNames = new CheckBox() { Text = "IJPLib Names", Checked = false };
         chkErrorsOnly = new CheckBox() { Text = "Errors Only", Checked = true };
         chkTraffic = new CheckBox() { Text = "Log Traffic", Checked = false };
         chkTraffic.CheckedChanged += ChkTraffic_CheckedChanged;

         tab.Controls.Add(tclViewXML);

         tclViewXML.Controls.Add(tabTreeView);
         tclViewXML.Controls.Add(tabIndented);

         tabTreeView.Controls.Add(tvXML);
         tabIndented.Controls.Add(txtIndentedView);

         tab.Controls.Add(cmdOpen);
         tab.Controls.Add(cmdClear);
         tab.Controls.Add(cmdRetrieve);
         tab.Controls.Add(cmdSaveAs);
         tab.Controls.Add(cmdSendFileToPrinter);
         tab.Controls.Add(cmdSendDisplayToPrinter);
         tab.Controls.Add(chkIJPLibNames);
         tab.Controls.Add(chkErrorsOnly);
         tab.Controls.Add(chkTraffic);

         // Testing controls
         lblSelectXmlTest = new Label() { Text = "Select XML Test", TextAlign = ContentAlignment.BottomCenter };
         cbAvailableXmlTests = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         cbAvailableXmlTests.SelectedIndexChanged += cbAvailableTests_SelectedIndexChanged;
         cmdBrowse = new Button() { Text = "Browse" };
         cmdBrowse.Click += cmdBrowse_Click;

         lblSelectHardTest = new Label() { Text = "Select Hard Test", TextAlign = ContentAlignment.BottomCenter };
         cbAvailableHardTests = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         cbAvailableHardTests.Items.AddRange(
            new string[] {
               "Reset", "Shift Code", "Month Day SR", "Time Count",
               "Day of Week etc", "MDY hms", "Multi-Line", "Counter",
               "ComprehensiveI", "ComprehensiveII", "???" }
            );
         cbAvailableHardTests.SelectedIndexChanged += CbAvailableHardTests_SelectedIndexChanged;
         cmdRunHardTest = new Button() { Text = "Run Test" };
         cmdRunHardTest.Click += cmdRunHardTest_Click;

         cmdVerify = new Button() { Text = "Verify" };
         cmdVerify.Click += cmdVerify_Click;

         cmdSend = new Button() { Text = "Send" };
         cmdSend.Click += SendFileToPrinter_Click;

         tab.Controls.Add(lblSelectXmlTest);
         tab.Controls.Add(lblSelectHardTest);
         tab.Controls.Add(cbAvailableXmlTests);
         tab.Controls.Add(cbAvailableHardTests);
         tab.Controls.Add(cmdVerify);
         tab.Controls.Add(cmdBrowse);
         tab.Controls.Add(cmdSend);
         tab.Controls.Add(cmdRunHardTest);

         BuildTraffic();

         BuildVerify();
      }

      private void BuildVerify() {

         tabVerify = new TabPage() { Name = "tabVerify", Text = "Verify View" };

         // Verify data Grid
         VerifyView = new DataGridView() {
            Name = "VerifyView",
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            RowHeadersVisible = false,
            Visible = true
         };

         DataGridViewTextBoxCell ct = new DataGridViewTextBoxCell();
         DataGridViewCellStyle cs = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter };

         V_XMLName = new DataGridViewColumn() { Name = "XMLName", HeaderText = "XML Name", ReadOnly = true, CellTemplate = ct };
         VerifyView.Columns.Add(V_XMLName);

         V_Class = new DataGridViewColumn() { Name = "Class", HeaderText = "Class", ReadOnly = true, CellTemplate = ct };
         VerifyView.Columns.Add(V_Class);

         V_Attribute = new DataGridViewColumn() { Name = "Attribute", HeaderText = "Attribute", ReadOnly = true, CellTemplate = ct };
         VerifyView.Columns.Add(V_Attribute);

         V_Item = new DataGridViewColumn() { Name = "Item", HeaderText = "Item", ReadOnly = true, CellTemplate = ct, DefaultCellStyle = cs };
         VerifyView.Columns.Add(V_Item);

         V_Block = new DataGridViewColumn() { Name = "Block", HeaderText = "Block", ReadOnly = true, CellTemplate = ct, DefaultCellStyle = cs };
         VerifyView.Columns.Add(V_Block);

         V_SubRule = new DataGridViewColumn() { Name = "SubRule", HeaderText = "Rule", ReadOnly = true, CellTemplate = ct, DefaultCellStyle = cs };
         VerifyView.Columns.Add(V_SubRule);

         V_DataOut = new DataGridViewColumn() { Name = "DataOut", HeaderText = "Data Out", ReadOnly = true, CellTemplate = ct };
         VerifyView.Columns.Add(V_DataOut);

         V_DataIn = new DataGridViewColumn() { Name = "DataIn", HeaderText = "Data In", ReadOnly = true, CellTemplate = ct };
         VerifyView.Columns.Add(V_DataIn);

         tabVerify.Controls.Add(VerifyView);

         tclViewXML.Controls.Add(tabVerify);
      }

      private void BuildTraffic() {

         tabTraffic = new TabPage() { Name = "tabTraffic", Text = "Traffic View" };

         // Traffic data Grid
         TrafficView = new DataGridView() {
            Name = "TrafficView",
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            RowHeadersVisible = false,
            Visible = true
         };

         DataGridViewTextBoxCell ct = new DataGridViewTextBoxCell();
         DataGridViewCellStyle cs = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter };

         T_Access = new DataGridViewColumn() { Name = "Access", HeaderText = "Access", ReadOnly = true, CellTemplate = ct };
         TrafficView.Columns.Add(T_Access);

         T_Class = new DataGridViewColumn() { Name = "Class", HeaderText = "Class", ReadOnly = true, CellTemplate = ct };
         TrafficView.Columns.Add(T_Class);

         T_Attribute = new DataGridViewColumn() { Name = "Attribute", HeaderText = "Attribute", ReadOnly = true, CellTemplate = ct };
         TrafficView.Columns.Add(T_Attribute);

         T_NOut = new DataGridViewColumn() { Name = "NOut", HeaderText = "#Out", ReadOnly = true, CellTemplate = ct, DefaultCellStyle = cs };
         TrafficView.Columns.Add(T_NOut);

         T_DataOut = new DataGridViewColumn() { Name = "DataOut", HeaderText = "Data Out", ReadOnly = true, CellTemplate = ct, DefaultCellStyle = cs };
         TrafficView.Columns.Add(T_DataOut);

         T_RawOut = new DataGridViewColumn() { Name = "RawOut", HeaderText = "Raw Out", ReadOnly = true, CellTemplate = ct };
         TrafficView.Columns.Add(T_RawOut);

         T_NIn = new DataGridViewColumn() { Name = "NIn", HeaderText = "#In", ReadOnly = true, CellTemplate = ct, DefaultCellStyle = cs };
         TrafficView.Columns.Add(T_NIn);

         T_DataIn = new DataGridViewColumn() { Name = "DataIn", HeaderText = "Data In", ReadOnly = true, CellTemplate = ct, DefaultCellStyle = cs };
         TrafficView.Columns.Add(T_DataIn);

         T_RawIn = new DataGridViewColumn() { Name = "RawIn", HeaderText = "Raw In", ReadOnly = true, CellTemplate = ct };
         TrafficView.Columns.Add(T_RawIn);

         tabTraffic.Controls.Add(TrafficView);

         tclViewXML.Controls.Add(tabTraffic);

         TrafficView.RowPrePaint += TrafficView_RowPrePaint;
      }

      private void CbAvailableHardTests_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // Called from parent
      public void ResizeControls(ref ResizeInfo R) {
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         int tclWidth = (int)(tab.ClientSize.Width / R.W);
         float offset = (int)(tab.ClientSize.Height - tclHeight * R.H);
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         R.offset = offset;
         Utils.ResizeObject(ref R, tclViewXML, 0, 1, tclHeight - 7, tclWidth - 1);
         {
            Utils.ResizeObject(ref R, tvXML, 1, 1, tclHeight - 12, tclWidth - 3);
            Utils.ResizeObject(ref R, txtIndentedView, 1, 1, tclHeight - 12, tclWidth - 3);

            Utils.ResizeObject(ref R, chkIJPLibNames, tclHeight - 6, 1, 1.5f, 4);
            Utils.ResizeObject(ref R, chkErrorsOnly, tclHeight - 4.5f, 1, 1.5f, 4);
            Utils.ResizeObject(ref R, chkTraffic, tclHeight - 3, 1, 1.5f, 4);

            Utils.ResizeObject(ref R, cmdOpen, tclHeight - 6, 5, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdRetrieve, tclHeight - 3, 5, 2.5f, 4);

            Utils.ResizeObject(ref R, cmdSend, tclHeight - 6, 9.5f, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdVerify, tclHeight - 3, 9.5f, 2.5f, 4);

            Utils.ResizeObject(ref R, cmdSaveAs, tclHeight - 6, 14, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdClear, tclHeight - 3, 14, 2.5f, 4);

            Utils.ResizeObject(ref R, lblSelectXmlTest, tclHeight - 6, 18.5f, 1, 6);
            Utils.ResizeObject(ref R, cbAvailableXmlTests, tclHeight - 5, 18.5f, 2, 6);
            Utils.ResizeObject(ref R, cmdBrowse, tclHeight - 3, 18.5f, 2.5f, 6);

            Utils.ResizeObject(ref R, cmdSendFileToPrinter, tclHeight - 6, 25, 2.5f, 4.5f);
            Utils.ResizeObject(ref R, cmdSendDisplayToPrinter, tclHeight - 3, 25, 2.5f, 4.5f);

            Utils.ResizeObject(ref R, lblSelectHardTest, tclHeight - 6, 30, 1, 5);
            Utils.ResizeObject(ref R, cbAvailableHardTests, tclHeight - 5, 30, 2, 5);
            Utils.ResizeObject(ref R, cmdRunHardTest, tclHeight - 3, 30, 2.5f, 5);

            // Resize VerifyView
            Utils.ResizeObject(ref R, VerifyView, 1, 1, tclHeight - 12, tclWidth - 3, 0.9f);
            Utils.ResizeObject(ref R, TrafficView, 1, 1, tclHeight - 12, tclWidth - 3, 0.9f);

         }
         R.offset = 0;
      }

      // Only allow buttons if conditions are right to process the request
      public void SetButtonEnables() {
         cmdSaveAs.Enabled = XMLText.Length > 0;
         cmdSend.Enabled = !string.IsNullOrEmpty(XMLFileName);
         cmdSendFileToPrinter.Enabled = !string.IsNullOrEmpty(XMLFileName);
         cmdSendDisplayToPrinter.Enabled = xmlDoc != null;
         cmdRunHardTest.Enabled = cbAvailableHardTests.SelectedIndex >= 0;
      }

      private void BuildTestFileList() {
         cbAvailableXmlTests.Items.Clear();
         try {
            string[] FileNames = Directory.GetFiles(parent.MessageFolder, "*.XML");
            Array.Sort(FileNames);
            for (int i = 0; i < FileNames.Length; i++) {
               cbAvailableXmlTests.Items.Add(Path.GetFileNameWithoutExtension(FileNames[i]));
            }
         } catch {

         }
      }

      #endregion

      #region Test Routines

      // Verify send vs received
      private void cmdVerify_Click(object sender, EventArgs e) {
         // Need a XMP Document to continue
         if (xmlDoc == null) {
            Open_Click(null, null);
         }
         if (xmlDoc != null) {
            VerifyView.Rows.Clear();
            EIP.Verify += EIP_VerifyEvent;
            EIP.VerifyXmlVsPrinter(xmlDoc, !chkErrorsOnly.Checked);
            EIP.Verify -= EIP_VerifyEvent;
         }
      }

      // Add text to all items (Control Deleted)
      private void cmdAddText_Click(object sender, EventArgs e) {
         // Add new logic here
      }

      // Create a message with text only (Control Deleted)
      private void cmdCreateText_Click(object sender, EventArgs e) {
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               try {
                  for (int step = 0; step < 3; step++) {
                     switch (step) {
                        case 0:
                           // Cleanup the display
                           CleanUpDisplay();
                           break;
                        case 1:
                           // Put in some items
                           for (int i = 0; i < 5; i++) {
                              EIP.ServiceAttribute(ccPF.Add_Column, 0);
                           }
                           break;
                        case 2:
                           // Set the text
                           SetText("Hello World");
                           break;
                     }
                  }
               } catch (EIPIOException e1) {
                  // In case of an EIP I/O error
                  string name = $"{EIP.GetAttributeName(e1.ClassCode, e1.Attribute)}";
                  string msg = $"EIP I/O Error on {e1.AccessCode}/{e1.ClassCode}/{name}";
                  MessageBox.Show(msg, "EIP I/O Error", MessageBoxButtons.OK);
               } catch {
                  // You are on your own here
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void cmdSaveToPrinter_Click(object sender, EventArgs e) {
         // A single command, no need to open/close the connection here
         byte[] data = EIP.Merge(EIP.ToBytes(4, 2), EIP.ToBytes(2, 1), EIP.ToBytes("AAA\x00"));
         AttrData attr = EIP.GetAttrData(ccPDM.Store_Print_Data);
         EIP.SetAttribute(attr.Class, attr.Val, data);
      }

      private void cmdBrowse_Click(object sender, EventArgs e) {
         FolderBrowserDialog dlg = new FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = parent.MessageFolder };
         if (dlg.ShowDialog() == DialogResult.OK) {
            parent.MessageFolder = dlg.SelectedPath;
            BuildTestFileList();
         }
         dlg.Dispose();
      }

      #endregion

   }

}
