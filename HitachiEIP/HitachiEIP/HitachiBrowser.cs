using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace HitachiEIP {

   public partial class HitachiBrowser : Form {

      #region Data declarations

      string IPAddress;
      int port;

      EIP EIP;

      int[] ClassAttr;
      AttrData attr;

      // Reformat files
      string RFN;
      StreamWriter RFS = null;

      ResizeInfo R;
      bool initComplete = false;

      public bool AllGood { get; set; } = true;

      // Attribute Screens
      Attributes<ccIDX> indexAttr;     // 0x7A
      Attributes<ccIJP> oprAttr;       // 0x75
      Attributes<ccPDM> pdmAttr;       // 0x66
      Attributes<ccPS> psAttr;         // 0x68
      Attributes<ccPF> pFmtAttr;       // 0x67
      Attributes<ccCal> calAttr;       // 0x69
      Attributes<ccSR> sRulesAttr;     // 0x6C
      Attributes<ccCount> countAttr;   // 0x79
      Attributes<ccUI> unitInfoAttr;   // 0x73
      Attributes<ccES> envirAttr;      // 0x71
      Attributes<ccOM> mgmtAttr;       // 0x74
      Attributes<ccUP> userPatAttr;    // 0x6B
      XML processXML;                  // xml processing


      public bool ComIsOn = false;
      public bool MgmtIsOn = false;
      public bool AutoReflIsOn = false;

      // Flags for adding extra controls to set Index functions
      public const int AddNone = 0;
      public const int AddItem = 0x01;
      public const int AddColumn = 0x02;
      public const int AddLine = 0x04;
      public const int AddPosition = 0x08;
      public const int AddCalendar = 0x10;
      public const int AddCount = 0x20;
      public const int AddSubstitution = 0x40;
      public const int AddAll = 0x7F;

      Traffic Traffic = null;

      string trafficHdrs =
         "Status/Path\tCount OK\tData OK\tAccess\tClass\tAttribute" +
         "\t#In\tData In\tRaw In\t#Out\tData Out\tRaw Out";

      #endregion

      #region Constructors and Destructors

      public HitachiBrowser() {
         InitializeComponent();

         this.Text += " - " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

         txtIPAddress.Text = Properties.Settings.Default.IPAddress;
         txtPort.Text = Properties.Settings.Default.IPPort;
         txtSaveFolder.Text = Properties.Settings.Default.LogFolder;
         if (!Directory.Exists(txtSaveFolder.Text)) {
            Directory.CreateDirectory(txtSaveFolder.Text);
         }
         VerifyAddressAndPort();

         EIP = new EIP(txtIPAddress.Text, port);
         EIP.Log += EIP_Log;
         EIP.IOComplete += EIP_IO_Complete;
         EIP.StateChanged += EIP_StateChanged;

      }

      #endregion

      #region Form Level events

      // Application has finished initialization, start the application
      private void HitachiBrowser_Load(object sender, EventArgs e) {
         // Center the form on the screen
         Utils.PositionForm(this, 0.75f, 0.9f);

         // Build traffic and log files
         Traffic = new Traffic(this);
         CreateExcelApp();

         // Create the ClassCode dropdown without the underscores 
         string[] ClassNames = Enum.GetNames(typeof(ClassCode));
         for (int i = 0; i < ClassNames.Length; i++) {
            cbClassCode.Items.Add($"{ClassNames[i].Replace('_', ' ')} (0x{(byte)EIP.ClassCodes[i]:X2})");
         }

         // Load all the tabbed control data
         indexAttr = new Attributes<ccIDX>
            (this, EIP, tabIndex, ClassCode.Index);
         oprAttr = new Attributes<ccIJP>
            (this, EIP, tabIJPOperation, ClassCode.IJP_operation, AddItem);
         pdmAttr = new Attributes<ccPDM>
            (this, EIP, tabPrintManagement, ClassCode.Print_data_management);
         psAttr = new Attributes<ccPS>
            (this, EIP, tabPrintSpec, ClassCode.Print_specification);
         pFmtAttr = new Attributes<ccPF>
            (this, EIP, tabPrintFormat, ClassCode.Print_format, AddItem | AddPosition | AddColumn);
         calAttr = new Attributes<ccCal>
            (this, EIP, tabCalendar, ClassCode.Calendar, AddCalendar | AddItem);
         sRulesAttr = new Attributes<ccSR>
            (this, EIP, tabSubstitution, ClassCode.Substitution_rules, AddItem);
         countAttr = new Attributes<ccCount>
            (this, EIP, tabCount, ClassCode.Count, AddItem | AddCount);
         unitInfoAttr = new Attributes<ccUI>
            (this, EIP, tabUnitInformation, ClassCode.Unit_Information);
         envirAttr = new Attributes<ccES>
            (this, EIP, tabEnviroment, ClassCode.Enviroment_setting);
         mgmtAttr = new Attributes<ccOM>
            (this, EIP, tabOpMgmt, ClassCode.Operation_management);
         userPatAttr = new Attributes<ccUP>
            (this, EIP, tabUserPattern, ClassCode.User_pattern);
         processXML = new XML(this, EIP, tabXML);

         // Force a resize
         initComplete = true;
         HitachiBrowser_Resize(null, null);

         //Start out connected to the printer
         btnStartSession_Click(null, null);

         // Force the first tab to load
         if (EIP.SessionIsOpen) {
            tclClasses_SelectedIndexChanged(null, null);
         }

         // Close out the session
         btnEndSession_Click(null, null);

         SetButtonEnables();
      }

      // Browser closing.  No un-managed memory so let the runtime environment clean most of it up
      private void HitachiBrowser_FormClosing(object sender, FormClosingEventArgs e) {

         // Stop logging
         EIP.Log -= EIP_Log;
         EIP.IOComplete -= EIP_IO_Complete;
         EIP.StateChanged -= EIP_StateChanged;

         // Close traffic/log files
         CloseExcelFile(false);

         // Save away the user's data
         Properties.Settings.Default.IPAddress = txtIPAddress.Text;
         Properties.Settings.Default.IPPort = txtIPAddress.Text;
         Properties.Settings.Default.LogFolder = txtSaveFolder.Text;
         Properties.Settings.Default.Save();
      }

      // Handle all screen resolutions
      private void HitachiBrowser_Resize(object sender, EventArgs e) {
         //
         // Avoid resize before Program Load has run or on screen minimize
         if (initComplete && ClientRectangle.Height > 0) {
            //
            this.SuspendLayout();
            // Build local parameters
            R = Utils.InitializeResize(this, 49, 47, true);

            #region Left Column

            Utils.ResizeObject(ref R, lblIPAddress, 1, 1, 2, 4);
            Utils.ResizeObject(ref R, txtIPAddress, 1, 5, 2, 4);
            Utils.ResizeObject(ref R, lblPort, 3, 1, 2, 4);
            Utils.ResizeObject(ref R, txtPort, 3, 5, 2, 4);
            Utils.ResizeObject(ref R, lblSessionID, 5, 1, 2, 4);
            Utils.ResizeObject(ref R, txtSessionID, 5, 5, 2, 4);

            Utils.ResizeObject(ref R, btnStartSession, 7.5f, 0.5f, 2, 4);
            Utils.ResizeObject(ref R, btnEndSession, 7.5f, 5, 2, 4);
            Utils.ResizeObject(ref R, btnForwardOpen, 10, 0.5f, 2, 4);
            Utils.ResizeObject(ref R, btnForwardClose, 10, 5, 2, 4);

            Utils.ResizeObject(ref R, lblClassCode, 13, 0.5f, 1, 8.5f);
            Utils.ResizeObject(ref R, cbClassCode, 14, 0.5f, 2, 8.5f);
            Utils.ResizeObject(ref R, lblFunction, 16, 0.5f, 1, 8.5f);
            Utils.ResizeObject(ref R, cbFunction, 17, 0.5f, 2, 8.5f);
            Utils.ResizeObject(ref R, btnIssueGet, 19, 0.5f, 2, 4);
            Utils.ResizeObject(ref R, btnIssueSet, 19, 5, 2, 4);
            Utils.ResizeObject(ref R, btnIssueService, 19, 0.5f, 2, 8.5f);

            Utils.ResizeObject(ref R, lblStatus, 21, 0.5f, 1, 8.5f);
            Utils.ResizeObject(ref R, txtStatus, 22, 0.5f, 2, 8.5f);

            Utils.ResizeObject(ref R, lblCountOut, 24, 0.5f, 1, 1);
            Utils.ResizeObject(ref R, lbldataOut, 24, 2, 1, 7);
            Utils.ResizeObject(ref R, txtCountOut, 25, 0.5f, 2, 1);
            Utils.ResizeObject(ref R, txtDataOut, 25, 2, 2, 7);
            Utils.ResizeObject(ref R, txtDataBytesOut, 27, 0.5f, 2, 8.5f);

            Utils.ResizeObject(ref R, lblCountIn, 29, 0.5f, 1, 1);
            Utils.ResizeObject(ref R, lbldataIn, 29, 2, 1, 7);
            Utils.ResizeObject(ref R, txtCountIn, 30, 0.5f, 2, 1);
            Utils.ResizeObject(ref R, txtDataIn, 30, 2, 2, 7);
            Utils.ResizeObject(ref R, txtDataBytesIn, 32, 0.5f, 2, 8.5f);

            Utils.ResizeObject(ref R, lblSaveFolder, 34, 0.5f, 1, 6);
            Utils.ResizeObject(ref R, txtSaveFolder, 35, 0.5f, 2, 8.5f);
            Utils.ResizeObject(ref R, btnBrowse, 37, 0.5f, 2, 4);
            Utils.ResizeObject(ref R, btnProperties, 37, 5, 2, 4);

            Utils.ResizeObject(ref R, lstErrors, 40, 0.5f, 8, 8.5f);

            #endregion

            #region  Classes

            Utils.ResizeObject(ref R, tclClasses, 1, 10, 44, 36);
            switch (tclClasses.SelectedIndex) {
               case 0:                         // ccIDX == 0x7A Index function 
                  indexAttr.ResizeControls(ref R);
                  break;
               case 1:                         // ccIJP == 0x75 IJP operation function
                  oprAttr.ResizeControls(ref R);
                  break;
               case 2:                         // ccPDM == 0x66 Print data management function
                  pdmAttr.ResizeControls(ref R);
                  break;
               case 3:                         // ccPS == 0x68 Print specification function
                  psAttr.ResizeControls(ref R);
                  break;
               case 4:                         // ccPF == 0x67 Print format function
                  pFmtAttr.ResizeControls(ref R);
                  break;
               case 5:                         // ccCal == 0x69 Calendar function
                  calAttr.ResizeControls(ref R);
                  break;
               case 6:                         // ccSR == 0x6C Substitution rules function
                  sRulesAttr.ResizeControls(ref R);
                  break;
               case 7:                         // ccCount == 0x79 Count function
                  countAttr.ResizeControls(ref R);
                  break;
               case 8:                         // ccUI == 0x73 Unit Information function
                  unitInfoAttr.ResizeControls(ref R);
                  break;
               case 9:                         // ccES == 0x71 Enviroment setting function
                  envirAttr.ResizeControls(ref R);
                  break;
               case 10:                        // ccOM == 0x74 Operation management function
                  mgmtAttr.ResizeControls(ref R);
                  break;
               case 11:                        // ccUP == 0x6B User pattern function
                  userPatAttr.ResizeControls(ref R);
                  break;
               case 12:
                  processXML.ResizeControls(ref R);
                  break;
            }

            #endregion

            #region Bottom Row

            Utils.ResizeObject(ref R, btnCom, 45.5f, 10, 3, 5);
            Utils.ResizeObject(ref R, btnAutoReflection, 45.5f, 15.5f, 3, 5);
            Utils.ResizeObject(ref R, btnManagementFlag, 45.5f, 21, 3, 5);

            Utils.ResizeObject(ref R, btnRefresh, 46, 26.5f, 2, 3);
            Utils.ResizeObject(ref R, btnStop, 46, 30, 2, 3);
            Utils.ResizeObject(ref R, btnViewTraffic, 46, 33.5f, 2, 3);
            Utils.ResizeObject(ref R, lblTraffic, 46, 37, 2, 2);
            Utils.ResizeObject(ref R, btnReadAll, 46, 39.5f, 2, 3);
            Utils.ResizeObject(ref R, btnExit, 46, 43.5f, 2, 3);

            #endregion

            //this.Refresh();
            this.ResumeLayout();

            Traffic.Tasks.Add(new TrafficPkt(Traffic.TaskType.Resize, null));
         }
      }

      #endregion

      #region Form control events

      // Start a new session
      private void btnStartSession_Click(object sender, EventArgs e) {
         // Verify the IP address and port.  Then use them to open the session
         VerifyAddressAndPort();
         EIP.IPAddress = txtIPAddress.Text;
         EIP.port = port;

         // Be sure that com is on
         if (EIP.StartSession()) {
            // Session ID is returned
            txtSessionID.Text = EIP.SessionID.ToString();

            // Open a path to the device
            if (EIP.ForwardOpen()) {
               // Blindly Set COM on and read it back
               AttrData attr = EIP.AttrDict[ClassCode.IJP_operation, (byte)ccIJP.Online_Offline];
               byte[] data = EIP.FormatOutput(1, attr.Set);
               EIP.WriteOneAttribute(ClassCode.IJP_operation, (byte)ccIJP.Online_Offline, data);
               GetComSetting();
               if (ComIsOn) {
                  // Got it, Get the other critical settings
                  GetAutoReflectionSetting();
                  GetMgmtSetting();
               }
               // Close the forward to avoid a timeout
            }
            EIP.ForwardClose();
         }

         SetButtonEnables();
      }

      // Close out a session
      private void btnEndSession_Click(object sender, EventArgs e) {
         EIP.EndSession();
         txtSessionID.Text = string.Empty;
         SetButtonEnables();
      }

      // Open a path to the device
      private void btnForwardOpen_Click(object sender, EventArgs e) {
         EIP.ForwardOpen();
         SetButtonEnables();
      }

      // Close the path to the device
      private void btnForwardClose_Click(object sender, EventArgs e) {
         EIP.ForwardClose();
         SetButtonEnables();
      }

      // That's all folks, Cleanup and exit
      private void btnExit_Click(object sender, EventArgs e) {
         if (EIP.ForwardIsOpen) {
            btnForwardClose_Click(null, null);
         }
         if (EIP.SessionIsOpen) {
            btnEndSession_Click(null, null);
         }
         this.Close();
      }

      // Issue a Get based on the Class Code and Function dropdowns
      private void btnIssueGet_Click(object sender, EventArgs e) {
         byte[] data = EIP.FormatOutput(txtDataOut.Text, attr.Get);
         EIP.ReadOneAttribute(EIP.ClassCodes[cbClassCode.SelectedIndex], (byte)ClassAttr[cbFunction.SelectedIndex], data);
      }

      // Issue a Set based on the Class Code and Function dropdowns
      private void btnIssueSet_Click(object sender, EventArgs e) {
         bool Success = false;
         try {
            byte[] data = EIP.FormatOutput(txtDataOut.Text, attr.Set);
            Success = EIP.WriteOneAttribute(EIP.ClassCodes[cbClassCode.SelectedIndex], (byte)ClassAttr[cbFunction.SelectedIndex], data);
         } catch {
            AllGood = false;
         }
      }

      // Issue a Service based on the Class Code and Function dropdowns
      private void btnIssueService_Click(object sender, EventArgs e) {
         bool Success = false;
         try {
            byte[] data = EIP.FormatOutput(txtDataOut.Text, attr.Service);
            Success = EIP.ServiceAttribute(EIP.ClassCodes[cbClassCode.SelectedIndex], (byte)ClassAttr[cbFunction.SelectedIndex], data);
         } catch {
            AllGood = false;
         }
      }

      // The class code changed, update the fulction dropdown list
      private void cbClassCode_SelectedIndexChanged(object sender, EventArgs e) {
         cbFunction.Items.Clear();
         ClassAttr = null;

         btnIssueGet.Visible = false;
         btnIssueSet.Visible = false;
         btnIssueService.Visible = false;

         if (cbClassCode.SelectedIndex >= 0) {
            // Get all names associated with the enumeration
            string[] names = EIP.ClassCodeAttributes[cbClassCode.SelectedIndex].GetEnumNames();
            ClassAttr = (int[])EIP.ClassCodeAttributes[cbClassCode.SelectedIndex].GetEnumValues();
            for (int i = 0; i < names.Length; i++) {
               cbFunction.Items.Add($"{names[i].Replace('_', ' ')}  (0x{ClassAttr[i]:X2})");
            }
         }
         SetButtonEnables();
      }

      // The function drop changed.  Set the correct Get/Set/Service buttons visible
      private void cbFunction_SelectedIndexChanged(object sender, EventArgs e) {
         if (cbClassCode.SelectedIndex >= 0 && cbFunction.SelectedIndex >= 0) {
            attr = EIP.AttrDict[EIP.ClassCodes[cbClassCode.SelectedIndex], (byte)ClassAttr[cbFunction.SelectedIndex]];
            btnIssueGet.Visible = attr.HasGet;
            btnIssueSet.Visible = attr.HasSet;
            btnIssueService.Visible = attr.HasService;
         } else {
            btnIssueGet.Visible = false;
            btnIssueSet.Visible = false;
            btnIssueService.Visible = false;
         }
         SetButtonEnables();
      }

      // Cycle the traffic file and bring it up in Notepad
      private void btnViewTraffic_Click(object sender, EventArgs e) {
         CloseExcelFile(true);
      }

      // Step thru all classes and attributes with classes to issue Get requests
      private void btnReadAll_Click(object sender, EventArgs e) {
         EIP_Log(null, "Read All Starting");
         // Get is assumed for read all request
         AllGood = true;
         if (EIP.StartSession()) {
            for (int i = 0; i < EIP.ClassCodeAttributes.Length && AllGood; i++) {
               ClassAttr = (int[])EIP.ClassCodeAttributes[i].GetEnumValues();
               if (EIP.ForwardOpen()) {
                  // Issue commands for this group
                  for (int j = 0; j < ClassAttr.Length && AllGood; j++) {
                     // Get attr for request
                     attr = EIP.AttrDict[EIP.ClassCodes[i], (byte)ClassAttr[j]];
                     if (attr.HasGet && !attr.Ignore) {
                        byte[] data = EIP.FormatOutput(txtDataOut.Text, attr.Get);
                        EIP.ReadOneAttribute(EIP.ClassCodes[i], (byte)ClassAttr[j], data);
                     }
                  }
               }
               EIP.ForwardClose();
            }
         }
         EIP.EndSession();
         EIP_Log(null, "Read All Complete");
      }

      // Invert the com setting
      private void btnCom_Click(object sender, EventArgs e) {
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Get (guess at) the current state, invert it, and read it back
               AttrData attr = EIP.AttrDict[ClassCode.IJP_operation, (byte)ccIJP.Online_Offline];
               byte[] data = EIP.FormatOutput(ComIsOn ? 0 : 1, attr.Set);
               if (EIP.WriteOneAttribute(ClassCode.IJP_operation, (byte)ccIJP.Online_Offline, data)) {
                  GetComSetting();
                  if (ComIsOn) {
                     // Update the other two major controls
                     GetAutoReflectionSetting();
                     GetMgmtSetting();
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         SetButtonEnables();
      }

      // Issue the command to clean up any stacked requests
      private void btnManagementFlag_Click(object sender, EventArgs e) {
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Don't know what the "1" state means.  If it is off, issue the "2"
               AttrData attr = EIP.AttrDict[ClassCode.Index, (byte)ccIDX.Start_Stop_Management_Flag];
               byte[] data = EIP.FormatOutput(MgmtIsOn ? 0 : 2, attr.Set);
               if (EIP.WriteOneAttribute(ClassCode.Index, (byte)ccIDX.Start_Stop_Management_Flag, data)) {
                  // Refresh the setting since "2" does not take but returns 0
                  GetMgmtSetting();
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         SetButtonEnables();
      }

      // Invert the Auto-Reflection flag
      private void btnAutoReflection_Click(object sender, EventArgs e) {
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               AttrData attr = EIP.AttrDict[ClassCode.Index, (byte)ccIDX.Automatic_reflection];
               byte[] data = EIP.FormatOutput(AutoReflIsOn ? 0 : 1, attr.Set);
               if (EIP.WriteOneAttribute(ClassCode.Index, (byte)ccIDX.Automatic_reflection, data)) {
                  GetAutoReflectionSetting();
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         SetButtonEnables();
      }

      // Update the Extra controls on the Attributes page that is open
      private void tclClasses_SelectedIndexChanged(object sender, EventArgs e) {
         HitachiBrowser_Resize(null, null);
         switch (tclClasses.SelectedIndex) {
            case 0:                         // ccIDX == 0x7A Index function 
               indexAttr.RefreshExtras();
               break;
            case 1:                         // ccIJP == 0x75 IJP operation function
               oprAttr.RefreshExtras();
               break;
            case 2:                         // ccPDM == 0x66 Print data management function
               pdmAttr.RefreshExtras();
               break;
            case 3:                         // ccPS == 0x68 Print specification function
               psAttr.RefreshExtras();
               break;
            case 4:                         // ccPF == 0x67 Print format function
               pFmtAttr.RefreshExtras();
               break;
            case 5:                         // ccCal == 0x69 Calendar function
               calAttr.RefreshExtras();
               break;
            case 6:                         // ccSR == 0x6C Substitution rules function
               sRulesAttr.RefreshExtras();
               break;
            case 7:                         // ccCount == 0x79 Count function
               countAttr.RefreshExtras();
               break;
            case 8:                         // ccUI == 0x73 Unit Information function
               unitInfoAttr.RefreshExtras();
               break;
            case 9:                         // ccES == 0x71 Enviroment setting function
               envirAttr.RefreshExtras();
               break;
            case 10:                        // ccOM == 0x74 Operation management function
               mgmtAttr.RefreshExtras();
               break;
            case 11:                        // ccUP == 0x6B User pattern function
               userPatAttr.RefreshExtras();
               break;
         }
      }

      // Clear the Log display
      private void cmLogClear_Click(object sender, EventArgs e) {
         lstErrors.Items.Clear();
      }

      // Copy the Log display to a file and open it in Notepad
      private void cmLogView_Click(object sender, EventArgs e) {
         string ViewFilename = CreateFileName(txtSaveFolder.Text, "View");
         StreamWriter ViewStream = new StreamWriter(ViewFilename, false);
         for (int i = 0; i < lstErrors.Items.Count; i++) {
            ViewStream.WriteLine(lstErrors.Items[i].ToString());
         }
         ViewStream.Flush();
         ViewStream.Close();
         Process.Start("notepad.exe", ViewFilename);
      }

      // Douple click triggers Copy/Display of the Log file
      private void lstErrors_MouseDoubleClick(object sender, MouseEventArgs e) {
         cmLogView_Click(null, null);
      }

      // Stop the Read All operation
      private void btnStop_Click(object sender, EventArgs e) {
         AllGood = false;
      }

      // Log messages from EIP
      public void EIP_Log(EIP sender, string msg) {
         Traffic.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddLog, msg));
         lstErrors.Items.Add(msg);
         lstErrors.SelectedIndex = lstErrors.Items.Count - 1;
      }

      // Respond to EIP change in success/fail state
      private void EIP_StateChanged(EIP sender, string msg) {
         if (!EIP.SessionIsOpen) {
            AllGood = false;
         }
         SetButtonEnables();
      }

      // A Get/Set/Service request has completed
      private void EIP_IO_Complete(EIP sender, EIPEventArg e) {
         // Record status and color it
         txtStatus.Text = EIP.GetStatus;
         if (e.Successful) {
            txtStatus.BackColor = Color.LightGreen;
            switch (e.Class) {
               case ClassCode.Index:
                  switch (e.Access) {
                     case AccessCode.Set:
                        // reflect any changes back to the Index Function
                        int n = Array.FindIndex(indexAttr.ccAttribute, x => x == e.Attribute);
                        indexAttr.texts[n].Text = EIP.SetDataValue;
                        break;
                  }
                  break;
               case ClassCode.IJP_operation:
                  if ((ccIJP)e.Attribute == ccIJP.Online_Offline) {
                     switch (e.Access) {
                        case AccessCode.Set:
                           break;
                        case AccessCode.Get:
                           break;
                     }
                  }
                  break;
            }
         } else {
            txtStatus.BackColor = Color.Pink;
         }

         // Display the data sent to the printer
         txtCountOut.Text = EIP.SetDataLength.ToString();
         txtDataOut.Text = EIP.SetDataValue;
         txtDataBytesOut.Text = EIP.GetBytes(EIP.SetData, 0, EIP.SetDataLength);

         // Display the printer's response
         txtCountIn.Text = EIP.GetDataLength.ToString();
         txtDataIn.Text = EIP.GetDataValue;
         txtDataBytesIn.Text = EIP.GetBytes(EIP.GetData, 0, EIP.GetDataLength);

         // Record the operation in the Traffic file
         Type at = EIP.ClassCodeAttributes[Array.IndexOf(EIP.ClassCodes, e.Class)];
         string trafficText = $"{EIP.GetStatus}\t{EIP.LengthIsValid}\t{EIP.DataIsValid}";
         trafficText += $"\t{e.Access}\t{e.Class}\t{EIP.GetAttributeName(at, e.Attribute)}";
         if (e.Successful) {
            if (EIP.GetDataLength == 0) {
               trafficText += $"\t";
            } else {
               trafficText += $"\t{EIP.GetDataLength}";
            }
            if (!string.IsNullOrEmpty(EIP.GetDataValue) && EIP.GetDataValue.Length > 50) {
               trafficText += $"\tSee=>";
            } else {
               trafficText += $"\t{EIP.GetDataValue}";
            }
            trafficText += $"\t{EIP.GetBytes(EIP.GetData, 0, Math.Min(EIP.GetDataLength, 16))}";
            if (EIP.SetDataLength == 0) {
               trafficText += $"\t";
            } else {
               trafficText += $"\t{EIP.SetDataLength}";
            }
            if (!string.IsNullOrEmpty(EIP.SetDataValue) && EIP.SetDataValue.Length > 50) {
               trafficText += $"\tSee=>";
            } else {
               trafficText += $"\t{EIP.SetDataValue}";
            }
            trafficText += $"\t{EIP.GetBytes(EIP.SetData, 0, Math.Min(EIP.SetDataLength, 16))}";
         }
         FillInColData(trafficText);

         // Record the operation in the log
         EIP_Log(sender, $"{EIP.LastIO} -- {e.Access}/{e.Class}/{EIP.GetAttributeName(at, e.Attribute)} Complete");
      }

      // A start to an idea that must be finished someday
      private void btnProperties_Click(object sender, EventArgs e) {
         //using (AttrProperties p = new AttrProperties(this, EIP, cbClassCode.SelectedIndex, cbFunction.SelectedIndex)) {
         //   p.ShowDialog(this);
         //}
      }

      // Get folder for saving Log/Traffic/Reformat data
      private void btnBrowse_Click(object sender, EventArgs e) {
         BrowseForFolder(txtSaveFolder);
      }

      // Reformat the old raw "Data" into "DataII" for maintainability
      private void btnReformat_Click(object sender, EventArgs e) {

         RFN = CreateFileName(txtSaveFolder.Text, "Reformat");
         RFS = new StreamWriter(RFN, false, Encoding.UTF8);

         DataII.ReformatTables(RFS);

         RFS.Flush();
         RFS.Close();
         Process.Start("notepad.exe", RFN);
      }

      // Refresh COM/Management/Auto-Reflection controls
      private void btnRefresh_Click(object sender, EventArgs e) {
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               GetComSetting();
               GetMgmtSetting();
               GetAutoReflectionSetting();
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      #endregion

      #region Service Routines

      // Make sure the IP Address and Port look valid
      private void VerifyAddressAndPort() {
         // Port must be a number
         if (!Int32.TryParse(txtPort.Text, out port)) {
            port = 44818;
            txtPort.Text = port.ToString();
         }
         // IP Address must be IPV4 format
         if (!System.Net.IPAddress.TryParse(txtIPAddress.Text, out System.Net.IPAddress IPAddress)) {
            txtIPAddress.Text = "192.168.0.1";
            IPAddress = IPAddress.Parse(txtIPAddress.Text);
         }
         this.IPAddress = IPAddress.ToString();
      }

      // Create a unique file name by incorporating time into the filename
      private string CreateFileName(string directory, string s, string ext = "csv") {
         if (Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
         }
         return Path.Combine(directory, $"{s}{DateTime.Now.ToString("yyMMdd-HHmmss")}.{ext}");
      }

      // Get the com setting
      private bool GetComSetting() {
         bool result;
         // Get the current setting == Com must be on for the printer to respond properly
         if (EIP.ReadOneAttribute(ClassCode.IJP_operation, (byte)ccIJP.Online_Offline, EIP.Nodata)) {
            if (EIP.GetDataValue == "1") {
               // Com is on.  That is good
               btnCom.Text = "COM\n1";
               btnCom.BackColor = Color.LightGreen;
               ComIsOn = true;
            } else {
               // Com is Off.  That is an issue
               btnCom.Text = "COM\n0";
               btnCom.BackColor = Color.Pink;
               ComIsOn = false;
            }
            result = true;
         } else {
            // Com state is unknown.  That is an issue
            btnCom.Text = "COM\n?";
            btnCom.BackColor = SystemColors.Control;
            ComIsOn = false;
            result = false;
         }
         SetButtonEnables();
         return result;
      }

      // Get Start/Stop Management flag
      private bool GetMgmtSetting() {
         bool result;
         // Get the currevt setting
         if (EIP.ReadOneAttribute(ClassCode.Index, (byte)ccIDX.Start_Stop_Management_Flag, EIP.Nodata)) {
            if (EIP.GetDataValue != "0") {
               // Non-zero says requests are to be stacked.
               btnManagementFlag.Text = $"S/S Management\n{EIP.GetDataValue}";
               btnManagementFlag.BackColor = Color.Pink;
               MgmtIsOn = true;
            } else {
               // Zero says requests are processed immediately
               btnManagementFlag.Text = "S/S Management\n0";
               btnManagementFlag.BackColor = Color.LightGreen;
               MgmtIsOn = false;
            }
            result = true;
         } else {
            // Could not be read so indicate so
            btnManagementFlag.Text = "S/S Management\n?";
            btnManagementFlag.BackColor = SystemColors.Control;
            MgmtIsOn = false;
            result = false;
         }
         SetButtonEnables();
         return result;
      }

      // Get the auto-reflection setting in the printer
      private bool GetAutoReflectionSetting() {
         bool result;
         // Read the value
         if (EIP.ReadOneAttribute(ClassCode.Index, (byte)ccIDX.Automatic_reflection, EIP.Nodata)) {
            if (EIP.GetDataValue == "1") {
               // Do not know whet 1 means but I think it is bad
               btnAutoReflection.Text = "Auto Reflection\n1";
               btnAutoReflection.BackColor = Color.Pink;
               AutoReflIsOn = true;
            } else {
               // Hoped for response
               btnAutoReflection.Text = "Auto Reflection\n0";
               btnAutoReflection.BackColor = Color.LightGreen;
               AutoReflIsOn = false;
            }
            result = true;
         } else {
            // Cannot read it so indicate so.
            btnAutoReflection.Text = "Auto Reflection\n?";
            btnAutoReflection.BackColor = SystemColors.Control;
            AutoReflIsOn = false;
            result = false;
         }
         SetButtonEnables();
         // Return true if state is known
         return result;
      }

      // Browse for folder to save Log/Traffic/Reformat data
      private void BrowseForFolder(TextBox tb) {
         using (FolderBrowserDialog dlg = new FolderBrowserDialog()) {
            dlg.ShowNewFolderButton = true;
            dlg.SelectedPath = tb.Text;
            if (dlg.ShowDialog() == DialogResult.OK) {
               tb.Text = dlg.SelectedPath;
            }
         }

      }

      // Enable only the buttons that should be used
      void SetButtonEnables() {
         bool SessionIsOpen = EIP.SessionIsOpen;
         bool ForwardIsOpen = EIP.ForwardIsOpen;

         btnStartSession.Enabled = !SessionIsOpen;
         btnEndSession.Enabled = SessionIsOpen;
         btnForwardOpen.Enabled = SessionIsOpen && !ForwardIsOpen;
         btnForwardClose.Enabled = SessionIsOpen && ForwardIsOpen;
         btnIssueGet.Enabled = btnIssueSet.Enabled = btnIssueService.Enabled =
            cbClassCode.SelectedIndex >= 0 && cbFunction.SelectedIndex >= 0;

         btnCom.Enabled = true;
         btnAutoReflection.Enabled = ComIsOn;
         btnManagementFlag.Enabled = ComIsOn;

         btnReadAll.Enabled = ComIsOn;

         if (initComplete) {
            switch (tclClasses.SelectedIndex) {
               case 0:                         // ccIDX == 0x7A Index function 
                  indexAttr.SetButtonEnables();
                  break;
               case 1:                         // ccIJP == 0x75 IJP operation function
                  oprAttr.SetButtonEnables();
                  break;
               case 2:                         // ccPDM == 0x66 Print data management function
                  pdmAttr.SetButtonEnables();
                  break;
               case 3:                         // ccPS == 0x68 Print specification function
                  psAttr.SetButtonEnables();
                  break;
               case 4:                         // ccPF == 0x67 Print format function
                  pFmtAttr.SetButtonEnables();
                  break;
               case 5:                         // ccCal == 0x69 Calendar function
                  calAttr.SetButtonEnables();
                  break;
               case 6:                         // ccSR == 0x6C Substitution rules function
                  sRulesAttr.SetButtonEnables();
                  break;
               case 7:                         // ccCount == 0x79 Count function
                  countAttr.SetButtonEnables();
                  break;
               case 8:                         // ccUI == 0x73 Unit Information function
                  unitInfoAttr.SetButtonEnables();
                  break;
               case 9:                         // ccES == 0x71 Enviroment setting function
                  envirAttr.SetButtonEnables();
                  break;
               case 10:                        // ccOM == 0x74 Operation management function
                  mgmtAttr.SetButtonEnables();
                  break;
               case 11:                        // ccUP == 0x6B User pattern function
                  userPatAttr.SetButtonEnables();
                  break;
               case 12:
                  processXML.SetButtonEnables();
                  break;
            }
         }

      }

      #endregion

      #region Excel traffic capture

      private void CreateExcelApp() {
         Traffic.Tasks.Add(new TrafficPkt(Traffic.TaskType.Create, trafficHdrs));
      }

      private void FillInColData(string data) {
         Traffic.Tasks.Add(new TrafficPkt(Traffic.TaskType.AddTraffic, data));
      }

      private void CloseExcelFile(bool view) {
         string filename = CreateFileName(txtSaveFolder.Text, "Traffic", "xlsx");
         Traffic.Tasks.Add(new TrafficPkt(Traffic.TaskType.Close, filename));

         if (view) {
            Traffic.Tasks.Add(new TrafficPkt(Traffic.TaskType.View, filename));
            CreateExcelApp();
         } else {
            Traffic.Tasks.Add(new TrafficPkt(Traffic.TaskType.Exit, null));
         }
      }

      #endregion

   }
}

