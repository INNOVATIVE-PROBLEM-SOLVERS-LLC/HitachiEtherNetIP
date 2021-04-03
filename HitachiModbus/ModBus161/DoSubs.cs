using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Serialization;
using Modbus_DLL;
using System.IO;

namespace ModBus161 {
   class DoSubs {

      #region Enumerations

      // Attributes within Substitution Rules class
      public enum ccSR {
         Year = 0,
         Month = 1,
         Day = 2,
         Hour = 3,
         Minute = 4,
         Week = 5,
         DayOfWeek = 6,
      }

      #endregion

      #region Data Declarations

      ResizeInfo R;

      internal enum Src {
         Global = 0,
         Message = 1,
      }

      internal Substitution[] Subs;

      readonly UI161 parent;
      GroupBox gb;
      Modbus MB;

      string fileName;

      // Substitution Specific Controls
      GroupBox SubControls;

      Label lblAttribute;
      ComboBox cbAttribute;

      Label lblSource;
      ComboBox cbSource;

      Label lblDelimiter;
      TextBox txtDelimiter;

      Label lblBaseYear;
      TextBox txtBaseYear;

      Label lblRuleNumber;
      TextBox txtRuleNumber;

      Button subGet;
      Button subSet;

      Button toGlobal;
      Button toGlobalAll;

      Button cmdOpen;
      Button cmdSaveAs;

      Label lblMsgFileName;
      public TextBox txtMsgFileName;

      Label lblGlobalFileName;
      public TextBox txtGlobalFileName;

      Label[][] subLabels;
      TextBox[][] subTexts;

      readonly bool[] resizeNeeded = new bool[] { true, true, true, true, true, true, true };
      readonly int[] startWith = new[] { 0, 1, 1, 0, 0, 1, 1 };
      readonly ccSR[] at = new ccSR[] {
         ccSR.Year,
         ccSR.Month,
         ccSR.Day,
         ccSR.Hour,
         ccSR.Minute,
         ccSR.Week,
         ccSR.DayOfWeek,
      };

      // Visible Category
      int vCat = -1;

      readonly System.Drawing.Font courier = new System.Drawing.Font("Courier New", 9);

      Color allColor = Color.Red;

      bool getInProgress = false;

      #endregion

      #region Constructors and destructors

      public DoSubs(UI161 parent, Modbus MB, GroupBox gb) {
         this.MB = MB;
         this.parent = parent;
         this.gb = gb;
      }

      #endregion

      #region Routines called from parent

      // Build all controls unique to this class
      public void BuildControls(Properties.Settings prop) {
         string[] attributeNames = typeof(ccSR).GetEnumNames();

         SubControls = new GroupBox() { Text = "Substitution Rules" };
         gb.Controls.Add(SubControls);
         SubControls.Paint += GroupBorder_Paint;

         lblAttribute = new Label() { Text = "Attribute", TextAlign = ContentAlignment.BottomCenter };
         SubControls.Controls.Add(lblAttribute);

         cbAttribute = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         SubControls.Controls.Add(cbAttribute);
         for (int i = 0; i < attributeNames.Length; i++) {
            string s = $"{attributeNames[i].Replace('_', ' ')}";
            cbAttribute.Items.Add(s);
         }
         cbAttribute.SelectedIndexChanged += cbAttribute_SelectedIndexChanged;

         lblDelimiter = new Label() { Text = "Delimiter", TextAlign = ContentAlignment.BottomCenter };
         SubControls.Controls.Add(lblDelimiter);

         txtDelimiter = new TextBox() { Text = "/", TextAlign = HorizontalAlignment.Center };
         SubControls.Controls.Add(txtDelimiter);

         lblBaseYear = new Label() { Text = "Base", TextAlign = ContentAlignment.BottomCenter };
         SubControls.Controls.Add(lblBaseYear);

         txtBaseYear = new TextBox() { Text = DateTime.Now.Year.ToString(), TextAlign = HorizontalAlignment.Center };
         SubControls.Controls.Add(txtBaseYear);

         lblRuleNumber = new Label() { Text = "Rule #", TextAlign = ContentAlignment.BottomCenter };
         SubControls.Controls.Add(lblRuleNumber);

         txtRuleNumber = new TextBox() { Text = "1", TextAlign = HorizontalAlignment.Center };
         SubControls.Controls.Add(txtRuleNumber);

         lblSource = new Label() { Text = "Source", TextAlign = ContentAlignment.BottomCenter };
         SubControls.Controls.Add(lblSource);

         cbSource = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         SubControls.Controls.Add(cbSource);
         cbSource.Items.AddRange(new string[] { "Global", "Message" });
         cbSource.SelectedIndexChanged += cbSource_SelectedIndexChanged;

         subGet = new Button() { Text = "Get" };
         SubControls.Controls.Add(subGet);
         subGet.Click += SubGet_Click;

         subSet = new Button() { Text = "Set" };
         SubControls.Controls.Add(subSet);
         subSet.Click += SubSet_Click;

         toGlobal = new Button() { Text = "To Global" };
         SubControls.Controls.Add(toGlobal);
         toGlobal.Click += toGlobal_Click;

         toGlobalAll = new Button() { Text = "All To Global" };
         SubControls.Controls.Add(toGlobalAll);
         toGlobalAll.Click += toGlobalAll_Click;

         cmdOpen = new Button() { Text = "Open" };
         SubControls.Controls.Add(cmdOpen);
         cmdOpen.Click += CmdOpen_Click;

         cmdSaveAs = new Button() { Text = "Save As" };
         SubControls.Controls.Add(cmdSaveAs);
         cmdSaveAs.Click += CmdSaveAs_Click;

         lblGlobalFileName = new Label() { Text = "Global", TextAlign = ContentAlignment.TopRight };
         SubControls.Controls.Add(lblGlobalFileName);
         txtGlobalFileName = new TextBox() { Text = prop.GlobalFileName, TextAlign = HorizontalAlignment.Left };
         SubControls.Controls.Add(txtGlobalFileName);


         lblMsgFileName = new Label() { Text = "Msg.", TextAlign = ContentAlignment.TopRight };
         SubControls.Controls.Add(lblMsgFileName);
         txtMsgFileName = new TextBox() { Text = prop.MsgFileName, TextAlign = HorizontalAlignment.Left };
         SubControls.Controls.Add(txtMsgFileName);

         lblSource = new Label() { Text = "Source", TextAlign = ContentAlignment.BottomCenter };
         SubControls.Controls.Add(lblSource);

         subLabels = new Label[][] {
               new Label[25],        // Year
               new Label[12],        // Month
               new Label[31],        // Day
               new Label[24],        // Hour
               new Label[60],        // Minute
               new Label[53],        // Week
               new Label[7],         // Day of week
            };
         subTexts = new TextBox[][] {
               new TextBox[25],      // Year
               new TextBox[12],      // Month
               new TextBox[31],      // Day
               new TextBox[24],      // Hour
               new TextBox[60],      // Minute
               new TextBox[53],      // Week
               new TextBox[7],       // Day of week
            };
         for (int i = 0; i < subLabels.GetLength(0); i++) {
            for (int j = 0; j < subLabels[i].Length; j++) {
               subLabels[i][j] = new Label() { Text = (j + startWith[i]).ToString("D2"), Visible = false, TextAlign = ContentAlignment.TopRight };
               subTexts[i][j] = new TextBox() { Visible = false, TextAlign = HorizontalAlignment.Center, Font = courier };
               subTexts[i][j].Enter += TextBox_Enter;
            }
            SubControls.Controls.AddRange(subLabels[i]);
            SubControls.Controls.AddRange(subTexts[i]);
         }

      }

      // Time to start things going.  cbSource must happen first to load substitutions
      internal void DoSubs_Load(object sender, EventArgs e) {
         // Get the global list of substitution rules
         string fileName = Path.Combine(parent.txtMessageFolder.Text, txtGlobalFileName.Text);
         SubRules sr = Deserialize<SubRules>(fileName);
         Subs[(int)Src.Global] = sr.Substitution;

         // Get the global loaded
         cbSource.SelectedIndex = (int)Src.Global;
         cbAttribute.SelectedIndex = (int)ccSR.Month;

         // get a message that contains substitution rules.
         fileName = Path.Combine(parent.txtMessageFolder.Text, txtMsgFileName.Text);
         Lab lab = Deserialize<Lab>(fileName);
         Subs[(int)Src.Message] = lab.Printer[0].Substitution;

      }

      // Allow button clicks only if conditions allow it
      public void SetButtonEnables() {
         subGet.Enabled = !getInProgress;

      }

      #endregion

      #region Form Control routines

      // Request the substitution rules and wait for the response
      private void SubGet_Click(object sender, EventArgs e) {
         if (MB.IsConnected) {
            getInProgress = true;                        // Avoid multiple concurrent requests
            parent.asyncIO.Complete += SubGet_Complete;  // Register for completion
            ModbusPkt res = new ModbusPkt(AsyncIO.TaskType.Substitutions) { substitution = null };
            parent.asyncIO.AsyncIOTasks.Add(res);
            SetButtonEnables();
         }
      }

      // TODO -- This is something like a callback. Maybe a callback is better.
      private void SubGet_Complete(object sender, AsyncComplete status) {
         AsyncIO  p = (AsyncIO)sender;
         if (status.Type == AsyncIO.TaskType.Substitutions) {
            // Cancel the notification
            p.Complete -= SubGet_Complete;
            getInProgress = false;
            // Get the substitution from the packet
            Src src = (Src)cbSource.SelectedIndex;
            Subs[(int)src] = status.substitution;
            // post the data retrieved
            clearSubstitutions();
            loadSubstitutions(Subs[(int)src]);
            SetButtonEnables();
         }
      }

      // Set the substitution rule data
      private void SubSet_Click(object sender, EventArgs e) {
         Src src = (Src)cbSource.SelectedIndex;
         if (Subs[(int)src] != null) {
            ModbusPkt res = new ModbusPkt(AsyncIO.TaskType.Substitutions) { substitution = Subs[(int)src] };
            parent.asyncIO.AsyncIOTasks.Add(res);
         }
         SetButtonEnables();
      }

      // Hide the old controls and make the new ones visible
      private void cbAttribute_SelectedIndexChanged(object sender, EventArgs e) {
         // Hide the current set of controls
         if (vCat >= 0) {
            for (int i = 0; i < subLabels[vCat].Length; i++) {
               subLabels[vCat][i].Visible = false;
               subTexts[vCat][i].Visible = false;
            }
         }
         // Show the new set of controls
         vCat = cbAttribute.SelectedIndex;
         for (int i = 0; i < subLabels[vCat].Length; i++) {
            subLabels[vCat][i].Visible = true;
            subTexts[vCat][i].Visible = true;
         }
         resizeSubstitutions(ref R);
         SetButtonEnables();
      }

      // Make the group box more visible
      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         e.Graphics.DrawRectangle(new Pen(Color.CadetBlue, 2), 1, 1, gb.Width - 2, gb.Height - 2);
      }

      // The Message/Global selection changed
      private void cbSource_SelectedIndexChanged(object sender, EventArgs e) {
         Src src = (Src)cbSource.SelectedIndex;
         Src other = src == Src.Global ? Src.Message : Src.Global;
         // Get any pending changes and save to the opposite (previously selected) substitution
         Subs[(int)other] = GetCurrentSubstitution();
         // Load the substitutions
         clearSubstitutions();
         loadSubstitutions(Subs[(int)src]);
         // Make the button text correct
         toGlobal.Text = $"To {other}";
         toGlobalAll.Text = $"All To {other}";
         SetButtonEnables();
      }

      // Set available printers Green, unavalable printers Red
      private void cbPrinters_DrawItem(object sender, DrawItemEventArgs e) {
         int n = e.Index;
         if (n >= 0) {
            ComboBox cb = (ComboBox)sender;
            Color c;
            if (MB.IsConnected) {
               c = Color.Green;
            } else {
               c = Color.Red;
            }
            e.DrawBackground();
            e.Graphics.FillRectangle(new SolidBrush(Color.White), e.Bounds);
            e.Graphics.DrawString(cb.Items[e.Index].ToString(), cb.Font, new SolidBrush(c), e.Bounds);
         }
      }

      // Move settings to the global for all attributes
      private void toGlobalAll_Click(object sender, EventArgs e) {
         Src other = (Src)cbSource.SelectedIndex == Src.Global ? Src.Message : Src.Global;
         Subs[(int)other] = GetCurrentSubstitution();
         SetButtonEnables();
      }

      // Move settings to global for current attribute only
      private void toGlobal_Click(object sender, EventArgs e) {
         Src other = (Src)cbSource.SelectedIndex == Src.Global ? Src.Message : Src.Global;
         AddOneRule(other);
         SetButtonEnables();
      }

      // Select all text when text box is entered
      private void TextBox_Enter(object sender, EventArgs e) {
         TextBox tb = (TextBox)sender;
         parent.BeginInvoke((Action)delegate { tb.SelectAll(); });
      }

      #endregion

      #region Service Routines

      // Add a new rule to either msg or global
      private void AddOneRule(Src n) {
         ccSR attr = (ccSR)cbAttribute.SelectedIndex;
         // Make sure the user filled it in
         if (ruleSpecified(attr)) {
            // Get the new rule
            SubstitutionRule sr = GetSubstitutionRule(attr);
            if (Subs[(int)n] == null) {
               // Create a whole new substitution with one rule
               Subs[(int)n] = GetNewSubstitution();
               Subs[(int)n].SubRule = new SubstitutionRule[] { sr };
            } else {
               // Delete the rule if it exists
               List<SubstitutionRule> r = new List<SubstitutionRule>();
               foreach (SubstitutionRule t in Subs[(int)n].SubRule) {
                  if (t.Type != attr.ToString()) {
                     r.Add(t);
                  }
               }
               // Add the new rule to the list and turn it back to an array
               r.Add(sr);
               Subs[(int)n].SubRule = r.ToArray();
            }
         }
      }

      // Clear all substitutions to string.empty
      private void clearSubstitutions() {
         for (int i = 0; i < subTexts.Length; i++) {
            for (int j = 0; j < subTexts[i].Length; j++) {
               subTexts[i][j].Text = string.Empty;
            }
         }
      }

      // Check to see if a rule is being used
      private bool ruleSpecified(ccSR rule) {
         for (int i = 0; i < subTexts[(int)rule].Length; i++) {
            if (!string.IsNullOrEmpty(subTexts[(int)rule][i].Text)) {
               return true;
            }
         }
         return false;
      }

      // Load either global or local substitution values
      private void loadSubstitutions(Substitution subs) {
         if (subs != null) {
            for (int i = 0; i < subs.SubRule.Length; i++) {
               if (Enum.TryParse<ccSR>(subs.SubRule[i].Type, out ccSR cat)) {
                  string[] s = subs.SubRule[i].Text.Split(subs.Delimiter[0]);
                  for (int j = 0; j < s.Length; j++) {
                     int n = subs.SubRule[i].Base - startWith[(int)cat] + j;
                     if (n >= 0 && n < subTexts[(int)cat].Length) {
                        subTexts[(int)cat][n].Text = s[j];
                     }
                  }
               }
            }
         }
      }

      // Save the current settings
      private Substitution GetCurrentSubstitution() {
         Substitution result = null;
         List<SubstitutionRule> rules = new List<SubstitutionRule>();
         foreach (ccSR t in at) {
            if (ruleSpecified(t)) {
               rules.Add(GetSubstitutionRule(t));
            }
         }
         if (rules.Count > 0 && int.TryParse(txtBaseYear.Text, out int year) && int.TryParse(txtRuleNumber.Text, out int rn)) {
            result = new Substitution() {
               Delimiter = txtDelimiter.Text,
               RuleNumber = rn,
               StartYear = year,
               SubRule = (SubstitutionRule[])rules.ToArray()
            };
         }
         return result;
      }

      // Get a new Substitution without any rules
      private Substitution GetNewSubstitution() {
         Substitution result = null;
         if (int.TryParse(txtBaseYear.Text, out int year) && int.TryParse(txtRuleNumber.Text, out int rn)) {
            result = new Substitution() {
               Delimiter = txtDelimiter.Text,
               RuleNumber = rn,
               StartYear = year
            };
         }
         return result;
      }

      // get the substitutions for one type
      private SubstitutionRule GetSubstitutionRule(ccSR t) {
         TextBox[] tbs = subTexts[(int)t];
         string[] s = new string[tbs.Length];
         for (int i = 0; i < tbs.Length; i++) {
            s[i] = tbs[i].Text;
         }
         return new SubstitutionRule() {
            Base = startWith[(int)t],
            Type = t.ToString(),
            Text = String.Join(txtDelimiter.Text, s)
         };
      }

      // Adjust for screen resolution
      public void ResizeControls(ref ResizeInfo R, float RowStart, float ColStart) {
         this.R = R;

         Utils.ResizeObject(ref R, SubControls, 1, 1, 19, 43);
         {
            for (int i = 0; i < resizeNeeded.Length; i++) {
               resizeNeeded[i] = true;
            }
            resizeSubstitutions(ref R);
         }
      }

      // Called on resize or category change
      private void resizeSubstitutions(ref ResizeInfo R) {
         Utils.ResizeObject(ref R, lblAttribute, 1.5f, 1, 1.5f, 3.5f);
         Utils.ResizeObject(ref R, cbAttribute, 3, 1, 2, 3.5f);

         Utils.ResizeObject(ref R, lblSource, 1.5f, 5, 1.5f, 3.5f);
         Utils.ResizeObject(ref R, cbSource, 3, 5, 2, 3.5f);

         Utils.ResizeObject(ref R, lblDelimiter, 1.5f, 9, 1.5f, 2.5f);
         Utils.ResizeObject(ref R, txtDelimiter, 3, 9, 2, 2.5f);

         Utils.ResizeObject(ref R, lblBaseYear, 1.5f, 12, 1.5f, 2.5f);
         Utils.ResizeObject(ref R, txtBaseYear, 3, 12, 2, 2.5f);

         Utils.ResizeObject(ref R, lblRuleNumber, 1.5f, 15, 1.5f, 2.5f);
         Utils.ResizeObject(ref R, txtRuleNumber, 3, 15, 2, 2.5f);

         if (vCat >= 0 && resizeNeeded[vCat]) {
            resizeNeeded[vCat] = false;
            for (int i = 0; i < subLabels[vCat].Length; i++) {
               float r = 6.5f + 2.5f * (int)(i / 12);
               float c = (i % 12) * 3.5f + 0.25f;
               Utils.ResizeObject(ref R, subLabels[vCat][i], r, c, 2, 1);
               Utils.ResizeObject(ref R, subTexts[vCat][i], r, c + 1, 2, 2.25f);
            }
         }
         Utils.ResizeObject(ref R, toGlobal, 1, 18, 2, 5.5f);
         Utils.ResizeObject(ref R, toGlobalAll, 3.5f, 18, 2, 5.5f);

         Utils.ResizeObject(ref R, subGet, 1, 24, 2, 3.5f);
         Utils.ResizeObject(ref R, subSet, 3.5f, 24, 2, 3.5f);

         Utils.ResizeObject(ref R, cmdOpen, 1, 28, 2, 3.5f);
         Utils.ResizeObject(ref R, cmdSaveAs, 3.5f, 28, 2, 3.5f);

         Utils.ResizeObject(ref R, lblGlobalFileName, 1, 32, 2, 2.5f);
         Utils.ResizeObject(ref R, lblMsgFileName, 3.5f, 32, 2, 2.5f);

         Utils.ResizeObject(ref R, txtGlobalFileName, 1, 35, 2, 7.5f);
         Utils.ResizeObject(ref R, txtMsgFileName, 3.5f, 35, 2, 7.5f);
      }

      // Deserialize the Substitution Class
      internal T Deserialize<T>(string fileName) {
         T sr = default;
         if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName)) {
            sr = Utils.LoadNewFormatXML<T>(fileName);
         }
         return sr;
      }

      // Serialize the Substitution Class
      internal string Serialize() {
         string result = string.Empty;
         Src src = (Src)cbSource.SelectedIndex;
         if (Subs[(int)src] != null) {
            if (src == Src.Global) {
               SubRules sr = new SubRules() { Substitution = Subs[(int)src] };
               result = Serializer<SubRules>.ClassToXml(sr);
            } else {
               // TODO Have to save it
            }
         }
         return result;
      }

      // Open "Lab" or "SubRules" file
      private void CmdOpen_Click(object sender, EventArgs e) {
         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            Src src = (Src)cbSource.SelectedIndex;
            if (src == Src.Global) {
               dlg.Title = "Select default substitution file";
               dlg.Filter = "Substitution File (HML)|*.hml|All (*.*)|*.*";
            } else {
               dlg.Title = "Select Message file";
               dlg.Filter = "Substitution File (HML)|*.hml|All (*.*)|*.*";
            }
            if (!string.IsNullOrEmpty(parent.txtMessageFolder.Text)) {
               dlg.InitialDirectory = Path.GetDirectoryName(parent.txtMessageFolder.Text);
            }
            if (dlg.ShowDialog(parent) == DialogResult.OK) {
               if (src == Src.Global) {
                  SubRules sr = Deserialize<SubRules>(dlg.FileName);
                  if (sr != null) {
                     Subs[(int)Src.Global] = sr.Substitution;
                     clearSubstitutions();
                     loadSubstitutions(Subs[(int)src]);
                  }
               } else {
                  Lab lab = Deserialize<Lab>(dlg.FileName);
                  if (lab != null) {
                     Subs[(int)Src.Message] = lab.Printer[0].Substitution;
                     clearSubstitutions();
                     loadSubstitutions(Subs[(int)Src.Message]);
                  }
               }
            }
         }
      }

      // Save "Lab" or "SubRules" file
      private void CmdSaveAs_Click(object sender, EventArgs e) {
         DialogResult dlgResult;
         using (SaveFileDialog dlg = new SaveFileDialog()) {
            dlg.CheckFileExists = false;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = "hml";
            dlg.Filter = "Substitution File (HML)|*.hml|All (*.*)|*.*";
            dlg.Title = "Save Substitution File to HML file";
            dlg.FileName = fileName;
            dlgResult = dlg.ShowDialog();
            if (dlgResult == DialogResult.OK && !String.IsNullOrEmpty(dlg.FileName)) {
               fileName = dlg.FileName;
               string hml = Serialize();
               if (!string.IsNullOrEmpty(hml)) {
                  File.WriteAllText(fileName, hml);
               }
               SetButtonEnables();
            }
         }
      }

      #endregion

   }
}

