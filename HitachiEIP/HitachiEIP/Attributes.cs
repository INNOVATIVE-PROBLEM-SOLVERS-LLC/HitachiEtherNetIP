using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HitachiEIP {
   class Attributes<t1> {

      #region Data Declarations

      ResizeInfo R;

      HitachiBrowser parent;
      EIP EIP;
      TabPage tab;
      int tclHeight;
      int tclWidth;

      ClassCode cc;
      public byte[] ccAttribute;
      ClassCode ccIndex = ClassCode.Index;

      // Headers
      Label[] hdrs;

      Label[] labels;
      public TextBox[] texts;
      ComboBox[] dropdowns;
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
      bool extrasLoaded = false;

      GroupBox ExtraControls;
      Label[] ExtraLabel;
      TextBox[] ExtraText;
      Button[] ExtraGet;
      Button[] ExtraSet;

      int half;

      bool IsSubstitution = false;
      Substitution Substitution;

      bool IsUserPattern = false;
      UserPattern UserPattern;

      Font courier = new Font("Courier New", 9);

      #endregion

      #region Constructors and destructors

      public Attributes(HitachiBrowser parent, EIP EIP, TabPage tab, ClassCode cc, int Extras = 0) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
         this.cc = cc;
         this.ccAttribute = ((t1[])typeof(t1).GetEnumValues()).Select(x => Convert.ToByte(x)).ToArray();
         this.Extras = Extras;

         extrasUsed = AddExtraControls();
         half = 16;

         BuildControls();

         // Substitution has extra controls
         if (IsSubstitution = Equals(tab, parent.tabSubstitution)) {
            // Assumes only one extra control
            Substitution = new Substitution(parent, EIP, tab, texts[0]);
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
         AttrData attr = DataII.AttrDict[cc, ccAttribute[tag]];
         if (attr.Ignore) {
            // Avoid a printer hang
            texts[tag].Text = "Ignored!";
            texts[tag].BackColor = Color.Pink;
            counts[tag].BackColor = Color.LightGreen;
         } else {
            // Build and issue the request
            byte[] data = EIP.FormatOutput(texts[tag], dropdowns[tag], attr, attr.Get);
            texts[tag].Text = "Loading";
            parent.AllGood = EIP.ReadOneAttribute(cc, attr.Val, data, out string val);
            // Process the data returned
            EIP.SetBackColor(attr, counts[tag], texts[tag], dropdowns[tag], attr.Data);
         }
         SetButtonEnables();
      }

      // Issue a single Set request
      private void Set_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = ((byte[])b.Tag)[0];
         AttrData attr = DataII.AttrDict[cc, ccAttribute[tag]];
         if (attr.Ignore) {
            // Avoid a printer hang
            texts[tag].Text = "Ignored!";
         } else {
            // Build output string if needed and issue Set request
            byte[] data = EIP.FormatOutput(texts[tag], dropdowns[tag], attr, attr.Set);
            bool Success = EIP.WriteOneAttribute(cc, attr.Val, data);
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
         AttrData attr = DataII.AttrDict[cc, ccAttribute[tag]];
         if (attr.Ignore) {
            // Avoid a printer hang
            texts[tag].Text = "Ignored!";
         } else {
            // Build output string if needed and issue Service request
            byte[] data = EIP.FormatOutput(texts[tag], dropdowns[tag], attr, attr.Service);
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
                     // Show the value immediately
                     //parent.Refresh();
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
         if(!e.Handled) {
            t.BackColor = Color.LightYellow;                           // Indicate that it has changed
         }
      }

      // Get the value associated with an extra control
      private void GetExtras_Click(object sender, EventArgs e) {
         Button b = (Button)sender;

         // Tag contains control number and attribute
         byte n = ((byte[])b.Tag)[0];
         ClassCode cc = (ClassCode)((byte[])b.Tag)[1];
         byte at = ((byte[])b.Tag)[2];

         // Mark as loading
         ExtraText[n].Text = "Loading";
         if (EIP.ReadOneAttribute(cc, at, EIP.Nodata, out string val)) {
            // Success, post the new value
            ExtraText[n].Text = val;
            ExtraText[n].BackColor = Color.LightGreen;
         } else {
            // Failure, indicate so.
            ExtraText[n].Text = "#Error";
            ExtraText[n].BackColor = Color.Pink;
         }
         SetButtonEnables();
      }

      // Update the printer with the value of an Extra control
      private void SetExtras_Click(object sender, EventArgs e) {
         Button b = (Button)sender;

         // Tag contains control number and attribute
         byte n = ((byte[])b.Tag)[0];
         ClassCode cc = (ClassCode)((byte[])b.Tag)[1];
         byte at = ((byte[])b.Tag)[2];
         AttrData attr = DataII.AttrDict[cc, at];

         // Only decimal values are allowed.  Set to Min if in error
         int len = attr.Set.Len;
         if (!long.TryParse(ExtraText[n].Text, out long val)) {
            val = attr.Set.Min;
         }

         // Write the value to the printer
         byte[] data = EIP.ToBytes((uint)val, len);
         if (EIP.WriteOneAttribute(cc, attr.Val, data)) {
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
         AttrData attr = DataII.AttrDict[cc, at];

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

         bool twoCols = ccAttribute.Length > half;
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
            AttrData attr = DataII.AttrDict[cc, ccAttribute[i]];
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
               dropdowns[i] = new ComboBox() { FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
               dropdowns[i].Items.AddRange(GetDropdownNames(attr));
               tab.Controls.Add(dropdowns[i]);
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


         getAll = new Button() { Text = "Get All" };
         getAll.Click += GetAll_Click;
         tab.Controls.Add(getAll);
         setAll = new Button() { Text = "Set All" };
         tab.Controls.Add(setAll);
         setAll.Click += SetAll_Click;
      }

      // Get the names associated with the dropdown
      private string[] GetDropdownNames(AttrData attr) {
         if (attr.Data.DropDown == fmtDD.Decimal) {
            // For decimal, just git the integer values from Min to Max
            string[] names = new string[(int)(attr.Data.Max - attr.Data.Min + 1)];
            for (int i = 0; i < names.Length; i++) {
               names[i] = (i + attr.Data.Min).ToString();
            }
            return names;
         } else {
            // Get the names from the translation table
            return DataII.DropDowns[(int)attr.Data.DropDown];
         }
      }

      // Add the controls that the user desires
      private int AddExtraControls() {
         byte n = 0;
         ExtraLabel = new Label[MaxExtras];
         ExtraText = new TextBox[MaxExtras];
         ExtraGet = new Button[MaxExtras];
         ExtraSet = new Button[MaxExtras];

         if ((Extras & HitachiBrowser.AddItem) > 0) {
            AddExtras(ref n, ccIDX.Item);
         }
         if ((Extras & HitachiBrowser.AddColumn) > 0) {
            AddExtras(ref n, ccIDX.Column);
         }
         if ((Extras & HitachiBrowser.AddLine) > 0) {
            AddExtras(ref n, ccIDX.Line);
         }
         if ((Extras & HitachiBrowser.AddPosition) > 0) {
            AddExtras(ref n, ccIDX.Character_position);
         }
         if ((Extras & HitachiBrowser.AddCalendar) > 0) {
            AddExtras(ref n, ccIDX.Calendar_Block);
         }
         if ((Extras & HitachiBrowser.AddCount) > 0) {
            AddExtras(ref n, ccIDX.Count_Block);
         }
         if ((Extras & HitachiBrowser.AddSubstitution) > 0) {
            AddExtras(ref n, ccIDX.Substitution_Rules_Setting);
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

         Utils.ResizeObject(ref R, hdrs[0], 0.5f, 0.25f, 1.5f, 8);
         Utils.ResizeObject(ref R, hdrs[1], 0.5f, 8.25f, 1.5f, 1f);
         Utils.ResizeObject(ref R, hdrs[2], 0.5f, 9.25f, 1.5f, 5);
         Utils.ResizeObject(ref R, hdrs[3], 0.5f, 14.25f, 1.5f, 3);
         if (labels.Length > half) {
            Utils.ResizeObject(ref R, hdrs[4], 0.5f, 0.25f + cw, 1.5f, 8);
            Utils.ResizeObject(ref R, hdrs[5], 0.5f, 8.25f + cw, 1.5f, 1f);
            Utils.ResizeObject(ref R, hdrs[6], 0.5f, 9.25f + cw, 1.5f, 5);
            Utils.ResizeObject(ref R, hdrs[7], 0.5f, 14.25f + cw, 1.5f, 3);
         }

         for (int i = 0; i < labels.Length; i++) {
            int r;
            int c;
            if (i < half) {
               r = 2 + i * 2;
               c = 0;
            } else {
               r = 2 + (i - half) * 2;
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
         Utils.ResizeObject(ref R, getAll, tclHeight - 3, 27, 2.75f, 4);
         Utils.ResizeObject(ref R, setAll, tclHeight - 3, 31.5f, 2.75f, 4);

         if (extrasUsed > 0) {
            Utils.ResizeObject(ref R, ExtraControls, tclHeight - 2 - 2 * ((extrasUsed + 1) / 2), 1, (2 * ((extrasUsed + 1) / 2)) + 1.25f, 25);
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
               Utils.ResizeObject(ref R, ExtraText[i], r, 4.5f + c, 1.5f, 2);
               Utils.ResizeObject(ref R, ExtraGet[i], r, 7 + c, 1.5f, 2);
               Utils.ResizeObject(ref R, ExtraSet[i], r, 9.5f + c, 1.5f, 2);
            }
         }

         // Tab specific controls
         int groupStart = (labels.Length + 1) * 2;
         int groupHeight = tclHeight - groupStart - 5;
         Substitution?.ResizeSubstitutionControls(ref R, groupStart, groupHeight, tclWidth);
         UserPattern?.ResizeUserPatternControls(ref R, groupStart, groupHeight, tclWidth);

         R.offset = 0;
         parent.tclClasses.Visible = true;
      }

      // Reload the extra controls from the printer
      public void RefreshExtras() {
         if (!extrasLoaded && parent.ComIsOn) {
            if (extrasUsed > 0) {
               if (EIP.StartSession()) {
                  if (EIP.ForwardOpen()) {
                     for (int i = 0; i < extrasUsed; i++) {
                        GetExtras_Click(ExtraGet[i], null);
                     }
                     extrasLoaded = true;
                  }
                  EIP.ForwardClose();
               }
               EIP.EndSession();
            }
            GetAll_Click(null, null);
            SetExtraButtonEnables(null, null);
         }
      }

      // Enable appropriate buttons based on conditions
      public void SetButtonEnables() {
         bool enable = parent.ComIsOn;
         bool anySets = false;
         bool anyGets = false;
         for (int i = 0; i < ccAttribute.Length; i++) {
            AttrData attr = DataII.AttrDict[cc, ccAttribute[i]];
            if (attr.HasSet) {
               if (EIP.TextIsValid(texts[i].Text, attr.Set)) {
                  sets[i].Enabled = parent.ComIsOn;
                  anySets |= enable;
               } else {
                  sets[i].Enabled = false;
               }
            }
            if (attr.HasGet) {
               if (attr.Get.Len == 0 || EIP.TextIsValid(texts[i].Text, attr.Get)) {
                  gets[i].Enabled = parent.ComIsOn;
                  anyGets |= enable;
               } else {
                  gets[i].Enabled = false;
               }
            }
            if (attr.HasService) {
               if (attr.Service.Len == 0 || EIP.TextIsValid(texts[i].Text, attr.Service)) {
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
            AttrData attr = DataII.AttrDict[ClassCode.Index, at];
            ExtraGet[i].Enabled = enabled;
            ExtraSet[i].Enabled = enabled && int.TryParse(ExtraText[i].Text, out int val) &&
               val >= attr.Data.Min && val <= attr.Data.Max;
         }
      }

      #endregion

   }

}
