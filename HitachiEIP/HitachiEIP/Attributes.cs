using System;
using System.Drawing;
using System.IO;
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

      AttrData[] attrs;

      t1[] attributes;
      ClassCode cc;
      ClassCode ccIndex = ClassCode.Index;

      // Headers
      Label[] hdrs;

      Label[] labels;
      TextBox[] texts;
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

      #endregion

      #region Constructors and destructors

      public Attributes(HitachiBrowser parent, EIP EIP, TabPage tab, ClassCode cc, int Extras = 0) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
         if (IsSubstitution = Equals(tab, parent.tabSubstitution)) {
            Substitution = new Substitution(parent, EIP, tab);
         }
         if (IsUserPattern = Equals(tab, parent.tabUserPattern)) {
            UserPattern = new UserPattern(parent, EIP, tab);
         }
         this.attributes = (t1[])typeof(t1).GetEnumValues();
         this.cc = cc;
         attrs = new AttrData[attributes.Length];
         for (int i = 0; i < attributes.Length; i++) {
            attrs[i] = Data.AttrDict[cc, Convert.ToByte(attributes[i])];
         }
         this.Extras = Extras;

         extrasUsed = AddExtraControls();
         half = 16;
         BuildControls();
      }

      #endregion

      #region Events handlers

      private void Get_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = (int)b.Tag;
         AttrData attr = attrs[tag];
         if (attr.Ignore) {
            texts[tag].Text = "Ignored!";
            texts[tag].BackColor = Color.Pink;
            counts[tag].BackColor = Color.LightGreen;
         } else {
            byte[] data = EIP.FormatOutput(texts[tag], dropdowns[tag], attr, attr.Get);
            texts[tag].Text = "Loading";
            parent.AllGood = EIP.ReadOneAttribute(cc, attr.Val, data, out string val);
            EIP.SetBackColor(attr, counts[tag], texts[tag], dropdowns[tag], attr.Data);
         }
         SetButtonEnables();
      }

      private void Set_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = (int)b.Tag;
         AttrData attr = attrs[tag];
         if (attr.Ignore) {
            texts[tag].Text = "Ignored!";
         } else {
            byte[] data = EIP.FormatOutput(texts[tag], dropdowns[tag], attr, attr.Set);
            bool Success = EIP.WriteOneAttribute(cc, attr.Val, data);
            if (Success) {
               texts[tag].BackColor = Color.LightGreen;
            }
         }
         SetButtonEnables();
      }

      private void Service_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         int tag = (int)b.Tag;
         AttrData attr = attrs[tag];
         if (attr.Ignore) {
            texts[tag].Text = "Ignored!";
         } else {
            byte[] data = EIP.FormatOutput(texts[tag], dropdowns[tag], attr, attr.Service);
            bool Success = EIP.ServiceAttribute(cc, attr.Val, data);
         }
         SetButtonEnables();
      }

      private void GetAll_Click(object sender, EventArgs e) {
         parent.AllGood = true;
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
         parent.Refresh();
         EIP.ForwardOpen(true);
         for (int i = 0; i < gets.Length && parent.AllGood; i++) {
            if (gets[i] != null) {
               Get_Click(gets[i], null);
               parent.Refresh();
               Application.DoEvents();
            }
         }
         if (!parent.AllGood) {
            parent.EIP_Log(null, "GetAll completed abnormally");
         }
         EIP.ForwardClose(true);
         SetButtonEnables();
      }

      private void SetAll_Click(object sender, EventArgs e) {
         parent.AllGood = true;
         EIP.ForwardOpen(true);
         for (int i = 0; i < sets.Length && parent.AllGood; i++) {
            if (sets[i] != null) {
               Set_Click(sets[i], null);
               parent.Refresh();
               Application.DoEvents();
            }
         }
         if (!parent.AllGood) {
            parent.EIP_Log(null, "SetAll completed abnormally");
         }
         EIP.ForwardClose(true);
      }

      private void NumbersOnly_KeyPress(object sender, KeyPressEventArgs e) {
         TextBox t = (TextBox)sender;
         e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
         t.BackColor = Color.LightYellow;
      }

      private void GetExtras_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         byte n = ((byte[])b.Tag)[0];
         byte at = ((byte[])b.Tag)[1];
         ExtraText[n].Text = "Loading";
         if (EIP.ReadOneAttribute(ccIndex, at, EIP.Nodata, out string val)) {
            ExtraText[n].Text = val;
            ExtraText[n].BackColor = Color.LightGreen;
         } else {
            ExtraText[n].Text = "#Error";
            ExtraText[n].BackColor = Color.Pink;
         }
      }

      private void SetExtras_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         byte n = ((byte[])b.Tag)[0];
         byte at = ((byte[])b.Tag)[1];
         AttrData attr = Data.AttrDict[ClassCode.Index, at];
         int len = attr.Set.Len;
         if (!long.TryParse(ExtraText[n].Text, out long val)) {
            val = attr.Set.Min;
         }
         byte[] data = EIP.ToBytes((uint)val, len);
         bool Success = EIP.WriteOneAttribute(ccIndex, attr.Val, data);
         if (Success) {
            ExtraText[n].BackColor = Color.LightGreen;
            GetAll_Click(null, null);
         }
      }

      private void Text_Enter(object sender, EventArgs e) {
         TextBox tb = (TextBox)sender;
         parent.BeginInvoke((Action)delegate { tb.SelectAll(); });
      }

      private void Text_Leave(object sender, EventArgs e) {
         TextBox b = (TextBox)sender;
         int tag = (int)b.Tag;
         AttrData attr = attrs[tag];
         SetButtonEnables();
      }

      private void Text_KeyPress(object sender, KeyPressEventArgs e) {
         TextBox t = (TextBox)sender;
         t.BackColor = Color.LightYellow;
      }

      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         using (Pen p = new Pen(Color.CadetBlue, 2)) {
            e.Graphics.DrawRectangle(p, 1, 1, gb.Width - 2, gb.Height - 2);
         }
      }

      #endregion

      #region Service Routines

      private void BuildControls() {

         // build headers
         if (attributes.Length > half) {
            hdrs = new Label[8];
         } else {
            hdrs = new Label[4];
         }
         hdrs[0] = new Label() { Text = "Attributes", TextAlign = System.Drawing.ContentAlignment.TopRight };
         hdrs[1] = new Label() { Text = "#", TextAlign = System.Drawing.ContentAlignment.TopCenter };
         hdrs[2] = new Label() { Text = "Data", TextAlign = System.Drawing.ContentAlignment.TopCenter };
         hdrs[3] = new Label() { Text = "Control", TextAlign = System.Drawing.ContentAlignment.TopCenter };
         if (attributes.Length > half) {
            hdrs[4] = new Label() { Text = "Attributes", TextAlign = System.Drawing.ContentAlignment.TopRight };
            hdrs[5] = new Label() { Text = "#", TextAlign = System.Drawing.ContentAlignment.TopCenter };
            hdrs[6] = new Label() { Text = "Data", TextAlign = System.Drawing.ContentAlignment.TopCenter };
            hdrs[7] = new Label() { Text = "Control", TextAlign = System.Drawing.ContentAlignment.TopCenter };
         }
         for (int i = 0; i < hdrs.Length; i++) {
            hdrs[i].Font = new Font(hdrs[i].Font, FontStyle.Underline | FontStyle.Bold);
         }
         tab.Controls.AddRange(hdrs);

         //validData = new int[attributes.Length, 2];
         labels = new Label[attributes.Length];
         texts = new TextBox[attributes.Length];
         dropdowns = new ComboBox[attributes.Length];
         counts = new TextBox[attributes.Length];
         gets = new Button[attributes.Length];
         sets = new Button[attributes.Length];
         services = new Button[attributes.Length];

         for (int i = 0; i < attributes.Length; i++) {
            AttrData attr = attrs[i];
            string s = $"{attributes[i].ToString().Replace('_', ' ')} (0x{attr.Val:X2})";
            labels[i] = new Label() {
               Tag = i,
               TextAlign = System.Drawing.ContentAlignment.TopRight,
               Text = s
            };
            tab.Controls.Add(labels[i]);

            counts[i] = new TextBox() { Tag = i, ReadOnly = true, TextAlign = HorizontalAlignment.Center };
            if (attr.HasService) {
               counts[i].Text = attr.Service.Len.ToString();
            } else if (attr.HasSet) {
               counts[i].Text = attr.Set.Len.ToString();
            } else {
               counts[i].Text = attr.Get.Len.ToString();
            }

            tab.Controls.Add(counts[i]);

            texts[i] = new TextBox() { Tag = i, TextAlign = HorizontalAlignment.Center };
            texts[i].Enter += Text_Enter;
            tab.Controls.Add(texts[i]);
            texts[i].ReadOnly = !(attr.HasSet || attr.HasGet && attr.Get.Len > 0 || attr.HasService && attr.Service.Len > 0);

            if (attr.DropDown >= 0) {
               dropdowns[i] = new ComboBox() { FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
               dropdowns[i].Items.AddRange(GetDropdownNames(attr));
               tab.Controls.Add(dropdowns[i]);
            }

            if (attr.HasGet) {
               gets[i] = new Button() { Tag = i, Text = "Get" };
               gets[i].Click += Get_Click;
               tab.Controls.Add(gets[i]);
            }
            if (attr.HasSet) {
               sets[i] = new Button() { Tag = i, Text = "Set" };
               sets[i].Click += Set_Click;
               tab.Controls.Add(sets[i]);
               if (attr.Set.Fmt == DataFormats.Decimal) {
                  texts[i].KeyPress += NumbersOnly_KeyPress;
               } else {
                  texts[i].KeyPress += Text_KeyPress;
               }
            }
            if (attr.HasService) {
               services[i] = new Button() { Tag = i, Text = "Service" };
               services[i].Click += Service_Click;
               tab.Controls.Add(services[i]);
            }
            if (attr.HasSet || attr.HasGet && attr.Get.Len > 0 || attr.HasService && attr.Service.Len > 0) {
               texts[i].Leave += Text_Leave;
            }
         }

         getAll = new Button() { Text = "Get All" };
         getAll.Click += GetAll_Click;
         tab.Controls.Add(getAll);
         setAll = new Button() { Text = "Set All" };
         tab.Controls.Add(setAll);
         setAll.Click += SetAll_Click;

         // Tab specific controls
         Substitution?.BuildSubstitutionControls();
         UserPattern?.BuildUserPatternControls();

      }

      private string[] GetDropdownNames(AttrData attr) {
         if (attr.DropDown == 0) {
            string[] names = new string[(int)(attr.Data.Max - attr.Data.Min + 1)];
            for (int i = 0; i < names.Length; i++) {
               names[i] = (i + attr.Data.Min).ToString();
            }
            return names;
         } else {
            return Data.DropDowns[attr.DropDown];
         }
      }

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

      private void AddExtras(ref byte n, ccIDX function) {
         ExtraLabel[n] = new Label() { TextAlign = ContentAlignment.TopRight, Text = function.ToString().Replace('_', ' ') };
         ExtraText[n] = new TextBox() { Tag = n, TextAlign = HorizontalAlignment.Center };
         ExtraGet[n] = new Button() { Text = "Get", Tag = new byte[] { n, (byte)function } };
         ExtraSet[n] = new Button() { Text = "Set", Tag = new byte[] { n, (byte)function } };
         ExtraText[n].Enter += Text_Enter;
         ExtraText[n].Leave += SetExtraButtonEnables;
         ExtraGet[n].Click += GetExtras_Click;
         ExtraSet[n].Click += SetExtras_Click;
         ExtraText[n].KeyPress += Text_KeyPress;
         n++;
      }

      public void ResizeControls(ref ResizeInfo R) {
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
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

      public void RefreshExtras() {
         bool enabled = parent.ComIsOn & EIP.SessionIsOpen;
         if (extrasLoaded || !enabled | parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         EIP.ForwardOpen(true);
         for (int i = 0; i < extrasUsed; i++) {
            GetExtras_Click(ExtraGet[i], null);
         }
         EIP.ForwardClose(true);
         GetAll_Click(null, null);
         extrasLoaded = true;
         SetExtraButtonEnables(null, null);
      }

      public void SetButtonEnables() {
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         bool enable = parent.ComIsOn & EIP.SessionIsOpen;
         bool anySets = false;
         bool anyGets = false;
         for (int i = 0; i < attributes.Length; i++) {
            AttrData attr = attrs[i];
            if (attr.HasSet) {
               if (EIP.TextIsValid(texts[i].Text, attr.Set)) {
                  sets[i].Enabled = parent.ComIsOn & EIP.SessionIsOpen;
                  anySets |= enable;
               } else {
                  sets[i].Enabled = false;
               }
            }
            if (attr.HasGet) {
               if (attr.Get.Len == 0 || EIP.TextIsValid(texts[i].Text, attr.Get)) {
                  gets[i].Enabled = parent.ComIsOn & EIP.SessionIsOpen;
                  anyGets |= enable;
               } else {
                  gets[i].Enabled = false;
               }
            }
            if (attr.HasService) {
               if (attr.Service.Len == 0 || EIP.TextIsValid(texts[i].Text, attr.Service)) {
                  services[i].Enabled = parent.ComIsOn & EIP.SessionIsOpen;
               } else {
                  services[i].Enabled = false;
               }
            }
         }
         setAll.Enabled = anySets;
         getAll.Enabled = anyGets;

         SetExtraButtonEnables(null, null);
         Substitution?.SetButtonEnables();
         UserPattern?.SetButtonEnables();

      }

      public void SetExtraButtonEnables(object sender, EventArgs e) {
         bool enabled = parent.ComIsOn & EIP.SessionIsOpen;
         for (int i = 0; i < extrasUsed; i++) {
            byte at = ((byte[])ExtraSet[i].Tag)[1];
            AttrData attr = Data.AttrDict[ClassCode.Index, at];
            ExtraGet[i].Enabled = enabled;
            ExtraSet[i].Enabled = enabled && int.TryParse(ExtraText[i].Text, out int val) &&
               val >= attr.Data.Min && val <= attr.Data.Max;
         }
      }

      #endregion

   }

}
