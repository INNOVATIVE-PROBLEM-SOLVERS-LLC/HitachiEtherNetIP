using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EIP_Lib {
   class Attributes<t1> {

      #region Data Declarations

      ResizeInfo R;

      Browser parent;
      EIP EIP;
      TabPage tab;
      int tclHeight;
      int tclWidth;

      ClassCode cc;
      public byte[] ccAttribute;

      // Headers
      Label[] hdrs;

      Label[] labels;
      public TextBox[] texts;
      public ComboBox[] dropdowns;
      TextBox[] counts;
      Button[] gets;
      Button[] sets;
      Button[] services;
      Button getAll;
      Button setAll;

      // Data associated with extra Get/Set buttons
      int Extras = 0;
      const int MaxExtras = 7;
      int extrasUsed = 0;
      bool attributesLoaded = false;

      GroupBox ExtraControls;
      Label[] ExtraLabel;
      TextBox[] ExtraText;
      public ComboBox[] ExtraDropdowns;
      Button[] ExtraGet;
      Button[] ExtraSet;

      // For IJP Operations only
      GroupBox grpErrors;
      ListBox lbErrors;
      Button cmdGetErrors;
      Button cmdClearErrors;
      Label lblErrorCount;
      TextBox txtErrorCount;

      public int Half {
         get {
            return cc == ClassCode.Substitution_rules ? 5 : 16;
         }
      }

      bool IsSubstitution = false;
      Substitution Substitution;

      bool IsUserPattern = false;
      UserPattern UserPattern;

      Font courier = new Font("Courier New", 9);

      #endregion

      #region Constructors and destructors

      public Attributes(Browser parent, EIP EIP, TabPage tab, ClassCode cc, int Extras = 0) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
         this.cc = cc;
         this.ccAttribute = ((t1[])typeof(t1).GetEnumValues()).Select(x => Convert.ToByte(x)).ToArray();
         this.Extras = Extras;

         extrasUsed = AddExtraControls();

         BuildControls();

         // Substitution has extra controls
         if (IsSubstitution = Equals(tab, parent.tabSubstitution)) {
            // Assumes only one extra control
            Substitution = new Substitution(parent, EIP, tab);
            Substitution.BuildSubstitutionControls();
         }
         // UserPattern has extra controls
         if (IsUserPattern = Equals(tab, parent.tabUserPattern)) {
            UserPattern = new UserPattern(parent, EIP, tab);
            UserPattern.BuildUserPatternControls();
         }

      }

      #endregion

      #region Events handlers

      // Issue a single Get request
      private void Get_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = ((byte[])b.Tag)[0];
         AttrData attr = EIP.AttrDict[cc, ccAttribute[tag]];
         if (attr.Ignore) {
            // Avoid a printer hang
            texts[tag].Text = "Ignored!";
            texts[tag].BackColor = Color.Pink;
            counts[tag].BackColor = Color.LightGreen;
         } else {
            // Build and issue the request
            byte[] data = EIP.FormatOutput(attr.Get, texts[tag], dropdowns[tag], attr);
            texts[tag].Text = "Loading";
            parent.AllGood = EIP.GetAttribute(cc, attr.Val, data);
            // Process the data returned
            EIP.SetBackColor(attr.Data, attr, counts[tag], texts[tag], dropdowns[tag]);
         }
         SetButtonEnables();
      }

      // Issue a single Set request
      private void Set_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = ((byte[])b.Tag)[0];
         AttrData attr = EIP.AttrDict[cc, ccAttribute[tag]];
         if (attr.Ignore) {
            // Avoid a printer hang
            texts[tag].Text = "Ignored!";
         } else {
            // Build output string if needed and issue Set request
            byte[] data = EIP.FormatOutput(attr.Set, texts[tag], dropdowns[tag], attr);
            bool Success = EIP.SetAttribute(cc, attr.Val, data);
            if (Success) {
               // In case the control was yellow
               texts[tag].BackColor = Color.LightGreen;
            }
         }
         SetButtonEnables();
      }

      // Issue a single service request
      private void Service_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = ((byte[])b.Tag)[0];
         AttrData attr = EIP.AttrDict[cc, ccAttribute[tag]];
         if (attr.Ignore) {
            // Avoid a printer hang
            texts[tag].Text = "Ignored!";
         } else {
            // Build output string if needed and issue Service request
            byte[] data = EIP.FormatOutput(attr.Service, texts[tag], dropdowns[tag], attr);
            bool Success = EIP.ServiceAttribute(cc, attr.Val, data);
         }
         SetButtonEnables();
      }

      // Get all the valid input data on the display
      private void GetAll_Click(object sender, EventArgs e) {
         parent.AllGood = true;
         // Set all controls to their initial empty state
         for (int i = 0; i < gets.Length; i++) {
            counts[i].BackColor = SystemColors.Control;
            if (gets[i] != null) {
               if (dropdowns[i] != null) {
                  dropdowns[i].Visible = false;
                  texts[i].Visible = true;
               }
               texts[i].Text = "Loading";
               texts[i].BackColor = SystemColors.Control;
            }
         }
         // Avoid Open/Close on each request
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Do them all but stop on an error
               for (int i = 0; i < gets.Length && parent.AllGood; i++) {
                  if (gets[i] != null) {
                     Get_Click(gets[i], null);
                     // Let the Cancel button event be processed
                     Application.DoEvents();
                  }
               }
               if (!parent.AllGood) {
                  // Oops, something bad happened
                  parent.EIP_Log(null, "GetAll completed abnormally");
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         SetButtonEnables();
      }

      // Set all the valid output data on the display
      private void SetAll_Click(object sender, EventArgs e) {
         parent.AllGood = true;
         // Avoid Open/Close on each request
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               // Do them all but stop on an error
               for (int i = 0; i < sets.Length && parent.AllGood; i++) {
                  if (sets[i] != null) {
                     Set_Click(sets[i], null);
                     // Let the Cancel button event be processed
                     Application.DoEvents();
                  }
               }
               if (!parent.AllGood) {
                  // Oops, something bad happened
                  parent.EIP_Log(null, "SetAll completed abnormally");
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      // Allow only positive numbers
      private void NumbersOnly_KeyPress(object sender, KeyPressEventArgs e) {
         TextBox t = (TextBox)sender;
         e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
      }

      // Allow only positive and negative numbers
      private void SignedNumbersOnly_KeyPress(object sender, KeyPressEventArgs e) {
         TextBox t = (TextBox)sender;
         e.Handled = !char.IsControl(e.KeyChar) && ("0123456789-+".IndexOf(e.KeyChar) == -1);
         if (!e.Handled && !char.IsControl(e.KeyChar)) {
            string s =
               t.Text.Substring(0, t.SelectionStart) +                 // Data before the selection
               e.KeyChar +                                             // Character entered
               t.Text.Substring(t.SelectionStart + t.SelectionLength); // Number after the selection
            if (s != "-" && s != "+") {                                // A lone minus or plus should be allowed
               if (!int.TryParse(s, out int n)) {                      // See if it is an integer
                  e.Handled = true;                                    // Nope, throw away the character
               }
            }
         }
         if (!e.Handled) {
            t.BackColor = Color.LightYellow;                           // Indicate that it has changed
         }
      }

      // Get the value associated with an extra control
      private void GetExtras_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         byte[] tag = (byte[])b.Tag;
         RefreshOneExtra(tag[0]);
         SetButtonEnables();
      }

      // Update the printer with the value of an Extra control
      private void SetExtras_Click(object sender, EventArgs e) {
         Button b = (Button)sender;

         // Tag contains control number and attribute
         byte n = ((byte[])b.Tag)[0];
         ClassCode cc = (ClassCode)((byte[])b.Tag)[1];
         byte at = ((byte[])b.Tag)[2];
         AttrData attr = EIP.AttrDict[cc, at];

         // Only decimal values are allowed.  Set to Min if in error
         int len = attr.Set.Len;
         if (!int.TryParse(ExtraText[n].Text, out int val)) {
            val = (int)attr.Set.Min;
         }

         // Write the value to the printer
         byte[] data = EIP.FormatOutput(attr.Set, val);
         if (EIP.SetAttribute(cc, attr.Val, data)) {
            // It worked, set normal on the control and update the full display
            ExtraText[n].BackColor = Color.LightGreen;
            GetAll_Click(null, null);
         }
         SetButtonEnables();
      }

      // Highlight all the text
      private void Text_Enter(object sender, EventArgs e) {
         TextBox tb = (TextBox)sender;
         parent.BeginInvoke((Action)delegate { tb.SelectAll(); });
      }

      // Update the enables to reflect data change
      private void Text_Leave(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // Turn the text box yellow to indicate a change.  Back to normal when the I/O completes
      private void Text_KeyPress(object sender, KeyPressEventArgs e) {
         TextBox t = (TextBox)sender;
         t.BackColor = Color.LightYellow;
      }

      // Respond to mouse wheel over a control
      private void Text_MouseWheel(object sender, MouseEventArgs e) {
         TextBox t = (TextBox)sender;

         // Tag contains control number and attribute
         byte n = ((byte[])t.Tag)[0];
         ClassCode cc = (ClassCode)((byte[])t.Tag)[1];
         byte at = ((byte[])t.Tag)[2];
         AttrData attr = EIP.AttrDict[cc, at];

         // Only decimal values are allowed.  Set to Min if in error
         int len = attr.Data.Len;
         if (!long.TryParse(t.Text, out long val)) {
            val = attr.Set.Min;
         }

         if (e.Delta > 0) {
            val = Math.Min(val + 1, attr.Set.Max);
         } else {
            val = Math.Max(val - 1, attr.Set.Min);
         }
         t.Text = val.ToString();
         t.BackColor = Color.LightYellow;
      }

      // Keep ExtraText up to date if dropdown changes
      private void ExtraDropdownIndexChanged(object sender, EventArgs e) {
         ComboBox cb = (ComboBox)sender;
         byte[] tag = (byte[])cb.Tag;
         int n = tag[0];
         AttrData attr = EIP.GetAttrData((ccIDX)tag[2]);
         ExtraText[n].Text = (cb.SelectedIndex + attr.Data.Min).ToString();
      }

      // Make the group box more visible
      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         using (Pen p = new Pen(Color.CadetBlue, 2)) {
            e.Graphics.DrawRectangle(p, 1, 1, gb.Width - 2, gb.Height - 2);
         }
      }

      #endregion

      #region Service Routines

      // Build all the controls for this attribute tab
      private void BuildControls() {

         bool twoCols = ccAttribute.Length > Half;
         // build headers for one or two columns of attributes
         if (twoCols) {
            hdrs = new Label[8];
         } else {
            hdrs = new Label[4];
         }
         // Need to bold them all
         Font bolded = new Font(tab.Font, FontStyle.Underline | FontStyle.Bold);

         // Build all the headers
         for (int i = 0; i < (twoCols ? 2 : 1); i++) {
            hdrs[i * 4 + 0] = new Label() { Text = "Attributes", Font = bolded, TextAlign = ContentAlignment.TopRight };
            hdrs[i * 4 + 1] = new Label() { Text = "#", Font = bolded, TextAlign = ContentAlignment.TopCenter };
            hdrs[i * 4 + 2] = new Label() { Text = "Data", Font = bolded, TextAlign = ContentAlignment.TopCenter };
            hdrs[i * 4 + 3] = new Label() { Text = "Control", Font = bolded, TextAlign = ContentAlignment.TopCenter };
         }
         tab.Controls.AddRange(hdrs);

         // Allocate the arrays to hold the controls
         labels = new Label[ccAttribute.Length];
         texts = new TextBox[ccAttribute.Length];
         dropdowns = new ComboBox[ccAttribute.Length];
         counts = new TextBox[ccAttribute.Length];
         gets = new Button[ccAttribute.Length];
         sets = new Button[ccAttribute.Length];
         services = new Button[ccAttribute.Length];

         // Build the controls
         for (int i = 0; i < ccAttribute.Length; i++) {
            AttrData attr = EIP.AttrDict[cc, ccAttribute[i]];
            string s = Enum.GetName(typeof(t1), ccAttribute[i]);
            byte[] tag = new byte[] { (byte)i, (byte)cc, ccAttribute[i] };
            labels[i] = new Label() {
               Tag = tag,
               TextAlign = System.Drawing.ContentAlignment.TopRight,
               Text = $"{Enum.GetName(typeof(t1), ccAttribute[i]).Replace('_', ' ')} (0x{attr.Val:X2})"
            };

            counts[i] = new TextBox() { Tag = tag, ReadOnly = true, TextAlign = HorizontalAlignment.Center, Font = courier };
            if (attr.HasService) {
               counts[i].Text = attr.Service.Len.ToString();
            } else if (attr.HasSet) {
               counts[i].Text = attr.Set.Len.ToString();
            } else {
               counts[i].Text = attr.Get.Len.ToString();
            }

            texts[i] = new TextBox() { Tag = tag, TextAlign = HorizontalAlignment.Center, Font = courier };
            texts[i].Enter += Text_Enter;
            tab.Controls.Add(texts[i]);
            texts[i].ReadOnly = !(attr.HasSet || attr.HasGet && attr.Get.Len > 0 || attr.HasService && attr.Service.Len > 0);
            if (attr.HasSet && attr.Data.DropDown == fmtDD.None &&
               (attr.Data.Fmt == DataFormats.Decimal || attr.Data.Fmt == DataFormats.DecimalLE ||
                attr.Data.Fmt == DataFormats.SDecimal || attr.Data.Fmt == DataFormats.SDecimalLE)) {
               texts[i].MouseWheel += Text_MouseWheel;
            }

            if (attr.Data.DropDown != fmtDD.None) {
               dropdowns[i] = new ComboBox() { Tag = tag, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Visible = !attr.HasGet };
               dropdowns[i].Items.AddRange(GetDropdownNames(attr));
               if (dropdowns[i].Visible) {
                  texts[i].Visible = false;
                  dropdowns[i].BackColor = Color.LightGreen;
                  dropdowns[i].SelectedIndex = 0;
                  texts[i].Text = attr.Set.Min.ToString();
                  dropdowns[i].SelectedIndexChanged += Attributes_SelectedIndexChanged;
               }
               tab.Controls.Add(dropdowns[i]);
            } else if (attr.HasSet && !attr.HasGet && attr.Set.Fmt == DataFormats.Decimal) {
               texts[i].Text = attr.Set.Min.ToString();
               texts[i].BackColor = Color.LightGreen;
            }

            if (attr.HasGet) {
               gets[i] = new Button() { Tag = tag, Text = "Get" };
               gets[i].Click += Get_Click;
               tab.Controls.Add(gets[i]);
            }
            if (attr.HasSet) {
               sets[i] = new Button() { Tag = tag, Text = "Set" };
               sets[i].Click += Set_Click;
               tab.Controls.Add(sets[i]);
               if (attr.Set.Fmt == DataFormats.Decimal || attr.Set.Fmt == DataFormats.DecimalLE) {
                  texts[i].KeyPress += NumbersOnly_KeyPress;
               } else if (attr.Set.Fmt == DataFormats.SDecimal || attr.Set.Fmt == DataFormats.SDecimalLE) {
                  texts[i].KeyPress += SignedNumbersOnly_KeyPress;
               } else {
                  texts[i].KeyPress += Text_KeyPress;
               }
            }
            if (attr.HasService) {
               services[i] = new Button() { Tag = tag, Text = "Service" };
               services[i].Click += Service_Click;
               tab.Controls.Add(services[i]);
            }
            if (attr.HasSet || attr.HasGet && attr.Get.Len > 0 || attr.HasService && attr.Service.Len > 0) {
               texts[i].Leave += Text_Leave;
            }
         }
         tab.Controls.AddRange(labels);
         tab.Controls.AddRange(counts);

         if (cc == ClassCode.IJP_operation) {
            grpErrors = new GroupBox() { Text = "Printer Errors" };
            tab.Controls.Add(grpErrors);
            grpErrors.Paint += GroupBorder_Paint;

            lbErrors = new ListBox() { ScrollAlwaysVisible = true };
            grpErrors.Controls.Add(lbErrors);

            lblErrorCount = new Label() { Text = "Count", TextAlign = ContentAlignment.BottomCenter };
            tab.Controls.Add(lblErrorCount);

            txtErrorCount = new TextBox() { TextAlign = HorizontalAlignment.Center, ReadOnly = true };
            tab.Controls.Add(txtErrorCount);

            cmdGetErrors = new Button() { Text = "Get Errors" };
            tab.Controls.Add(cmdGetErrors);
            cmdGetErrors.Click += CmdGetErrors_Click;

            cmdClearErrors = new Button() { Text = "Clear Errors" };
            tab.Controls.Add(cmdClearErrors);
            cmdClearErrors.Click += CmdClearErrors_Click;
         }

         getAll = new Button() { Text = "Get All" };
         getAll.Click += GetAll_Click;
         tab.Controls.Add(getAll);
         setAll = new Button() { Text = "Set All" };
         tab.Controls.Add(setAll);
         setAll.Click += SetAll_Click;
      }

      private void CmdGetErrors_Click(object sender, EventArgs e) {
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               EIP.GetAttribute(cc, (byte)ccIJP.Fault_and_warning_history, EIP.Nodata);
               byte[] d = EIP.GetData;
               lbErrors.Items.Clear();
               long count = 0;
               if (d.Length > 3) {
                  count = EIP.Get(d, 0, 4, mem.LittleEndian);
                  for (int i = 0; i < Math.Min(count, (d.Length - 4) / 10); i++) {
                     int n = i * 10 + 4;
                     long year = EIP.Get(d, n, 2, mem.LittleEndian);
                     lbErrors.Items.Add($"{i + 1:00} | {year:0000}/{d[n + 2]:00}/{d[n + 3]:00} {d[n + 4]:00}:{d[n + 5]:00}:{d[n + 6]:00} | {d[n + 7]:00} | {d[n + 8]:00} | {d[n + 9]:00}");
                  }
               }
               txtErrorCount.Text = count.ToString();
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void CmdClearErrors_Click(object sender, EventArgs e) {

      }

      private void Attributes_SelectedIndexChanged(object sender, EventArgs e) {
         ComboBox dd = (ComboBox)sender;
         Byte[] tag = (byte[])dd.Tag;
         AttrData attr = EIP.AttrDict[(ClassCode)tag[1], tag[2]];
         texts[tag[0]].Text = (dd.SelectedIndex + attr.Set.Min).ToString();
      }

      // Get the names associated with the dropdown
      private string[] GetDropdownNames(AttrData attr) {
         if (attr.Data.DropDown == fmtDD.Decimal) {
            // For decimal, just get the integer values from Min to Max
            string[] names = new string[(int)(attr.Data.Max - attr.Data.Min + 1)];
            for (int i = 0; i < names.Length; i++) {
               names[i] = (i + attr.Data.Min).ToString();
            }
            return names;
         } else {
            // Get the names from the translation table
            return EIP.DropDowns[(int)attr.Data.DropDown];
         }
      }

      // Add the controls that the user desires
      private int AddExtraControls() {
         byte n = 0;
         ExtraLabel = new Label[MaxExtras];
         ExtraText = new TextBox[MaxExtras];
         ExtraDropdowns = new ComboBox[MaxExtras];
         ExtraGet = new Button[MaxExtras];
         ExtraSet = new Button[MaxExtras];

         if ((Extras & Browser.AddItem) > 0) {
            AddExtras(ref n, ccIDX.Item);
         }
         if ((Extras & Browser.AddColumn) > 0) {
            AddExtras(ref n, ccIDX.Column);
         }
         if ((Extras & Browser.AddLine) > 0) {
            AddExtras(ref n, ccIDX.Line);
         }
         if ((Extras & Browser.AddPosition) > 0) {
            AddExtras(ref n, ccIDX.Character_position);
         }
         if ((Extras & Browser.AddCalendar) > 0) {
            AddExtras(ref n, ccIDX.Calendar_Block);
         }
         if ((Extras & Browser.AddCount) > 0) {
            AddExtras(ref n, ccIDX.Count_Block);
         }
         if ((Extras & Browser.AddSubstitution) > 0) {
            AddExtras(ref n, ccIDX.Substitution_Rules_Setting);
         }
         if ((Extras & Browser.AddGroupNumber) > 0) {
            AddExtras(ref n, ccIDX.Print_Data_Group_Data);
         }
         if ((Extras & Browser.AddMessageNumber) > 0) {
            AddExtras(ref n, ccIDX.Print_Data_Message_Number);
         }
         if ((Extras & Browser.AddUserPatternSize) > 0) {
            AddExtras(ref n, ccIDX.User_Pattern_Size);
         }
         if (n > 0) {
            ExtraControls = new GroupBox() { Text = "Index Functions" };
            ExtraControls.Paint += GroupBorder_Paint;
            tab.Controls.Add(ExtraControls);
            for (int i = 0; i < n; i++) {
               ExtraControls.Controls.Add(ExtraLabel[i]);
               ExtraControls.Controls.Add(ExtraText[i]);
               ExtraControls.Controls.Add(ExtraGet[i]);
               ExtraControls.Controls.Add(ExtraSet[i]);
               if (ExtraDropdowns[i] != null) {
                  ExtraControls.Controls.Add(ExtraDropdowns[i]);
               }
            }
         }
         return n;
      }

      // Add a single extra control
      private void AddExtras(ref byte n, ccIDX function) {
         byte[] tag = new byte[] { n, (byte)ClassCode.Index, (byte)function };
         ExtraLabel[n] = new Label() { TextAlign = ContentAlignment.TopRight, Text = function.ToString().Replace('_', ' ') };
         ExtraText[n] = new TextBox() { Tag = tag, TextAlign = HorizontalAlignment.Center, Font = courier };
         ExtraGet[n] = new Button() { Text = "Get", Tag = tag };
         ExtraSet[n] = new Button() { Text = "Set", Tag = tag };
         ExtraText[n].Enter += Text_Enter;
         ExtraText[n].Leave += SetExtraButtonEnables;
         ExtraGet[n].Click += GetExtras_Click;
         ExtraSet[n].Click += SetExtras_Click;
         ExtraText[n].KeyPress += Text_KeyPress;
         ExtraText[n].MouseWheel += Text_MouseWheel;
         AttrData attr = EIP.GetAttrData(function);
         if (attr.Data.DropDown != fmtDD.None) {
            ExtraDropdowns[n] = new ComboBox() { Tag = tag, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            ExtraDropdowns[n].Items.AddRange(GetDropdownNames(attr));
            ExtraDropdowns[n].SelectedIndexChanged += ExtraDropdownIndexChanged;
         }
         n++;
      }

      // Resize all the controls on the tab
      public void ResizeControls(ref ResizeInfo R) {
         this.R = R;
         parent.tclClasses.Visible = false;
         tclHeight = (int)(tab.ClientSize.Height / R.H);
         tclWidth = (int)(tab.ClientSize.Width / R.W);
         float offset = (int)(tab.ClientSize.Height - tclHeight * R.H);
         R.offset = offset;
         float cw = 17.5f;
         float ExtraGroupHeight = 0;

         Utils.ResizeObject(ref R, hdrs[0], 0.5f, 0.25f, 1.5f, 8);
         Utils.ResizeObject(ref R, hdrs[1], 0.5f, 8.25f, 1.5f, 1f);
         Utils.ResizeObject(ref R, hdrs[2], 0.5f, 9.25f, 1.5f, 5);
         Utils.ResizeObject(ref R, hdrs[3], 0.5f, 14.25f, 1.5f, 3);
         if (labels.Length > Half) {
            Utils.ResizeObject(ref R, hdrs[4], 0.5f, 0.25f + cw, 1.5f, 8);
            Utils.ResizeObject(ref R, hdrs[5], 0.5f, 8.25f + cw, 1.5f, 1f);
            Utils.ResizeObject(ref R, hdrs[6], 0.5f, 9.25f + cw, 1.5f, 5);
            Utils.ResizeObject(ref R, hdrs[7], 0.5f, 14.25f + cw, 1.5f, 3);
         }

         for (int i = 0; i < labels.Length; i++) {
            int r;
            int c;
            if (i < Half) {
               r = 2 + i * 2;
               c = 0;
            } else {
               r = 2 + (i - Half) * 2;
               c = 1;
            }
            Utils.ResizeObject(ref R, labels[i], r, 0.25f + c * cw, 1.5f, 8);
            Utils.ResizeObject(ref R, counts[i], r, 8.25f + c * cw, 1.5f, 1);
            Utils.ResizeObject(ref R, texts[i], r, 9.5f + c * cw, 1.5f, 4.75f);
            if (dropdowns[i] != null) {
               Utils.ResizeObject(ref R, dropdowns[i], r, 9.5f + c * cw, 1.5f, 4.75f);
            }
            if (gets[i] != null) {
               Utils.ResizeObject(ref R, gets[i], r, 14.5f + c * cw, 1.5f, 1.5f);
            }
            if (sets[i] != null) {
               Utils.ResizeObject(ref R, sets[i], r, 16.25f + c * cw, 1.5f, 1.5f);
            }
            if (services[i] != null) {
               Utils.ResizeObject(ref R, services[i], r, 14.5f + c * cw, 1.5f, 3.25f);
            }
         }

         Utils.ResizeObject(ref R, getAll, tclHeight - 4, 27, 2.75f, 4);
         Utils.ResizeObject(ref R, setAll, tclHeight - 4, 31.5f, 2.75f, 4);

         if (extrasUsed > 0) {
            ExtraGroupHeight = (2 * ((extrasUsed + 1) / 2)) + 1.25f;
            Utils.ResizeObject(ref R, ExtraControls, tclHeight - 2 - 2 * ((extrasUsed + 1) / 2), 1, ExtraGroupHeight, 25);
            int r = -1;
            int c = 0;
            for (int i = 0; i < extrasUsed; i++) {
               if ((i & 1) == 0) {
                  c = 0;
                  r += 2;
               } else {
                  c = 12;
               }
               Utils.ResizeObject(ref R, ExtraLabel[i], r, 0.25f + c, 2, 4);
               Utils.ResizeObject(ref R, ExtraText[i], r, 4.5f + c, 1.5f, 3);
               if(ExtraDropdowns[i] != null) {
                  Utils.ResizeObject(ref R, ExtraDropdowns[i], r, 4.5f + c, 1.5f, 3);
               }
               Utils.ResizeObject(ref R, ExtraGet[i], r, 8 + c, 1.5f, 2);
               Utils.ResizeObject(ref R, ExtraSet[i], r, 10.5f + c, 1.5f, 2);
            }
         }

         // Tab specific controls
         float groupStart = cc == ClassCode.Substitution_rules ? (labels.Length / 2 + 1) * 2 : (labels.Length + 1) * 2;
         float groupHeight = tclHeight - groupStart - ExtraGroupHeight - 1;
         if (cc == ClassCode.IJP_operation) {
            Utils.ResizeObject(ref R, grpErrors, groupStart, 1, groupHeight, 25);
            {
               Utils.ResizeObject(ref R, lbErrors, 1, 1, groupHeight - 2, 23, 1.5f);
            }
            Utils.ResizeObject(ref R, lblErrorCount, tclHeight - 11, 31.5f, 2, 4);
            Utils.ResizeObject(ref R, txtErrorCount, tclHeight - 9, 31.5f, 2, 4);
            Utils.ResizeObject(ref R, cmdGetErrors, tclHeight - 7, 27, 2.75f, 4);
            Utils.ResizeObject(ref R, cmdClearErrors, tclHeight - 7, 31.5f, 2.75f, 4);
         }

         Substitution?.ResizeSubstitutionControls(ref R, groupStart, groupHeight, tclWidth);
         UserPattern?.ResizeUserPatternControls(ref R, groupStart, groupHeight - 1, tclWidth);

         R.offset = 0;
         parent.tclClasses.Visible = true;
      }

      // Reload the extra controls from the printer
      public void RefreshExtras() {
         bool reloadTab = !attributesLoaded;
         for (int i = 0; i < extrasUsed; i++) {
            reloadTab |= RefreshOneExtra(i);
         }
         if (reloadTab && parent.ComIsOn) {
            GetAll_Click(null, null);
            attributesLoaded = true;
         }
         SetExtraButtonEnables(null, null);
      }

      private bool RefreshOneExtra(int i) {
         bool reloadTab = false;
         int at = ((byte[])ExtraGet[i].Tag)[2];
         int n = EIP.GetIndexSetting((ccIDX)at);
         string s = n.ToString();
         AttrData attr = EIP.GetAttrData((ccIDX)at);
         if (ExtraText[i].Text != s) {
            ExtraText[i].Text = s;
            reloadTab = true;
         }
         ExtraText[i].Visible = true;
         if (attr.Data.DropDown != fmtDD.None) {
            n -= (int)attr.Data.Min;
            if (n >= 0 && n < ExtraDropdowns[i].Items.Count) {
               ExtraDropdowns[i].SelectedIndex = n;
               ExtraText[i].Visible = false;
               ExtraDropdowns[i].Visible = true;
               ExtraDropdowns[i].BackColor = Color.LightGreen;
            } else {
               ExtraDropdowns[i].Visible = false;
            }
         }
         ExtraText[i].BackColor = Color.LightGreen;
         return reloadTab;
      }

      // Enable appropriate buttons based on conditions
      public void SetButtonEnables() {
         bool enable = parent.ComIsOn;
         bool anySets = false;
         bool anyGets = false;
         for (int i = 0; i < ccAttribute.Length; i++) {
            AttrData attr = EIP.AttrDict[cc, ccAttribute[i]];
            if (attr.HasSet) {
               if (EIP.TextIsValid(attr.Set, texts[i].Text)) {
                  sets[i].Enabled = parent.ComIsOn;
                  anySets |= enable;
               } else {
                  sets[i].Enabled = false;
               }
            }
            if (attr.HasGet) {
               if (attr.Get.Len == 0 || EIP.TextIsValid(attr.Get, texts[i].Text)) {
                  gets[i].Enabled = parent.ComIsOn;
                  anyGets |= enable;
               } else {
                  gets[i].Enabled = false;
               }
            }
            if (attr.HasService) {
               if (attr.Service.Len == 0 || EIP.TextIsValid(attr.Service, texts[i].Text)) {
                  services[i].Enabled = parent.ComIsOn;
               } else {
                  services[i].Enabled = false;
               }
            }
         }
         setAll.Enabled = anySets;
         getAll.Enabled = anyGets;

         SetExtraButtonEnables(null, null);

         // Substitution and User Pattern have extra controls
         Substitution?.SetButtonEnables();
         UserPattern?.SetButtonEnables();

      }

      // Set enables on the extra buttons of the display
      public void SetExtraButtonEnables(object sender, EventArgs e) {
         bool enabled = parent.ComIsOn;
         for (int i = 0; i < extrasUsed; i++) {
            byte at = ((byte[])ExtraSet[i].Tag)[2];
            AttrData attr = EIP.AttrDict[ClassCode.Index, at];
            ExtraGet[i].Enabled = enabled;
            ExtraSet[i].Enabled = enabled && int.TryParse(ExtraText[i].Text, out int val) &&
               val >= attr.Data.Min && val <= attr.Data.Max;
         }
      }

      #endregion

   }

}
