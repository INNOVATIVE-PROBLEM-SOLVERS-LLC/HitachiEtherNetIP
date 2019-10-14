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

      TreeView tvXML;
      TextBox txtIndentedView;
      TextBox txtVerify;

      // Operating Buttons
      Button cmdOpen;
      Button cmdClear;
      Button cmdGenerate;
      Button cmdSaveAs;
      Button cmdVerify;
      Button cmdSend;

      // Testing Buttons
      Label lblSelectXmlTest;
      ComboBox cbAvailableXmlTests;
      Button cmdBrowse;
      Button cmdSendFileToPrinter;
      Button cmdSendDisplayToPrinter;
      CheckBox chkAutoReflect;
      CheckBox chkErrorsOnly;
      CheckBox chkSerialize;

      Label lblSelectHardTest;
      ComboBox cbAvailableHardTests;
      Button cmdRunHardTest;

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
                     ProcessLabel(File.ReadAllText(dlg.FileName));
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
         txtIndentedView.Text = string.Empty;
         xmlDoc = null;
         tvXML.Nodes.Clear();
         XMLText = string.Empty;
         XMLFileName = string.Empty;
         SetButtonEnables();
      }

      // Save the generated XML file
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

      #region Service Routines

      // Build XML page controls
      private void BuildControls() {
         tclViewXML = new TabControl() { Name = "tclViewXML", Font = courier };
         tabTreeView = new TabPage() { Name = "tabTreeView", Text = "Tree View" };
         tabIndented = new TabPage() { Name = "tabIndented", Text = "Indented View" };
         tabVerify = new TabPage() { Name = "tabVerify", Text = "Verify View" };

         tvXML = new TreeView() { Name = "tvXML", Font = courier };
         txtIndentedView = new TextBox() { Name = "txtIndentedView", Font = courier, Multiline = true, ScrollBars = ScrollBars.Both };
         txtVerify = new TextBox() { Name = "txtVerify", Font = courier, Multiline = true, ScrollBars = ScrollBars.Both };

         cmdOpen = new Button() { Text = "Open" };
         cmdOpen.Click += Open_Click;

         cmdClear = new Button() { Text = "Clear" };
         cmdClear.Click += Clear_Click;

         cmdGenerate = new Button() { Text = "Generate" };
         cmdGenerate.Click += Generate_Click;

         cmdSaveAs = new Button() { Text = "Save As" };
         cmdSaveAs.Click += SaveAs_Click;

         cmdSendFileToPrinter = new Button() { Text = "File To Printer" };
         cmdSendFileToPrinter.Click += SendFileToPrinter_Click;

         cmdSendDisplayToPrinter = new Button() { Text = "Display To Printer" };
         cmdSendDisplayToPrinter.Click += SendDisplayToPrinter_Click;

         chkAutoReflect = new CheckBox() { Text = "Auto Reflect.", Checked = true };
         chkErrorsOnly = new CheckBox() { Text = "Errors Only", Checked = true };
         chkSerialize = new CheckBox() { Text = "Serialize", Checked = false };

         tab.Controls.Add(tclViewXML);

         tclViewXML.Controls.Add(tabTreeView);
         tclViewXML.Controls.Add(tabIndented);
         tclViewXML.Controls.Add(tabVerify);

         tabTreeView.Controls.Add(tvXML);
         tabIndented.Controls.Add(txtIndentedView);
         tabVerify.Controls.Add(txtVerify);

         tab.Controls.Add(cmdOpen);
         tab.Controls.Add(cmdClear);
         tab.Controls.Add(cmdGenerate);
         tab.Controls.Add(cmdSaveAs);
         tab.Controls.Add(cmdSendFileToPrinter);
         tab.Controls.Add(cmdSendDisplayToPrinter);
         tab.Controls.Add(chkAutoReflect);
         tab.Controls.Add(chkErrorsOnly);
         tab.Controls.Add(chkSerialize);

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
            Utils.ResizeObject(ref R, txtVerify, 1, 1, tclHeight - 12, tclWidth - 3);

            Utils.ResizeObject(ref R, chkAutoReflect, tclHeight - 6, 1, 2, 4);
            Utils.ResizeObject(ref R, chkErrorsOnly, tclHeight - 4, 1, 2, 4);
            Utils.ResizeObject(ref R, chkSerialize, tclHeight - 2, 1, 2, 4);

            Utils.ResizeObject(ref R, cmdOpen, tclHeight - 6, 5, 2.5f, 4);
            Utils.ResizeObject(ref R, cmdGenerate, tclHeight - 3, 5, 2.5f, 4);

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

         }
         R.offset = 0;
      }

      // Only allow buttons if conditions are right to process the request
      public void SetButtonEnables() {
         cmdSaveAs.Enabled = XMLText.Length > 0;
         cmdSend.Enabled = xmlDoc != null;
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
            EIP.VerifyXmlVsPrinter(xmlDoc, !chkErrorsOnly.Checked);
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
