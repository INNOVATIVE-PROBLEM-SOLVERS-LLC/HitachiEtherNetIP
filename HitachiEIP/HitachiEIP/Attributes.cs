using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
      eipClassCode cc;
      eipClassCode ccIndex = eipClassCode.Index;

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

      #endregion

      #region Constructors and destructors

      public Attributes(HitachiBrowser parent, EIP EIP, TabPage tab, eipClassCode cc, int Extras = 0) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
         IsSubstitution = Equals(tab, parent.tabSubstitution);
         IsUserPattern = Equals(tab, parent.tabUserPattern);
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
            parent.AllGood = EIP.ReadOneAttribute(cc, attr.Val, attr, data, out string val);
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
         bool OpenCloseForward = !EIP.ForwardIsOpen;
         if (OpenCloseForward) {
            EIP.ForwardOpen();
         }
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
         if (OpenCloseForward && EIP.ForwardIsOpen) {
            EIP.ForwardClose();
         }
         SetButtonEnables();
      }

      private void SetAll_Click(object sender, EventArgs e) {
         parent.AllGood = true;
         bool OpenCloseForward = !EIP.ForwardIsOpen;
         if (OpenCloseForward) {
            EIP.ForwardOpen();
         }
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
         if (OpenCloseForward && EIP.ForwardIsOpen) {
            EIP.ForwardClose();
         }
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
         AttrData attr = Data.AttrDict[ccIndex, at];
         if (EIP.ReadOneAttribute(ccIndex, at, attr, EIP.Nodata, out string val)) {
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
         AttrData attr = Data.AttrDict[eipClassCode.Index, at];
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
         BuildSubstitutionControls();
         BuildUserPatternControls();

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
            AddExtras(ref n, eipIndex.Item);
         }
         if ((Extras & HitachiBrowser.AddColumn) > 0) {
            AddExtras(ref n, eipIndex.Column);
         }
         if ((Extras & HitachiBrowser.AddLine) > 0) {
            AddExtras(ref n, eipIndex.Line);
         }
         if ((Extras & HitachiBrowser.AddPosition) > 0) {
            AddExtras(ref n, eipIndex.Character_position);
         }
         if ((Extras & HitachiBrowser.AddCalendar) > 0) {
            AddExtras(ref n, eipIndex.Calendar_Block);
         }
         if ((Extras & HitachiBrowser.AddCount) > 0) {
            AddExtras(ref n, eipIndex.Count_Block);
         }
         if ((Extras & HitachiBrowser.AddSubstitution) > 0) {
            AddExtras(ref n, eipIndex.Substitution_Rules_Setting);
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

      private void AddExtras(ref byte n, eipIndex function) {
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
         ResizeSubstitutionControls(ref R);
         ResizeUserPatternnControls(ref R);

         R.offset = 0;
         parent.tclClasses.Visible = true;
      }

      public void RefreshExtras() {
         bool enabled = parent.ComIsOn & EIP.SessionIsOpen;
         if (extrasLoaded || !enabled | parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         bool OpenCloseForward = !EIP.ForwardIsOpen;
         if (OpenCloseForward) {
            EIP.ForwardOpen();
         }
         for (int i = 0; i < extrasUsed; i++) {
            GetExtras_Click(ExtraGet[i], null);
         }
         GetAll_Click(null, null);
         if (OpenCloseForward) {
            EIP.ForwardClose();
         }
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
         SetSubstitutionButtonEnables();
         SetUpButtonEnables();

      }

      public void SetExtraButtonEnables(object sender, EventArgs e) {
         bool enabled = parent.ComIsOn & EIP.SessionIsOpen;
         for (int i = 0; i < extrasUsed; i++) {
            byte at = ((byte[])ExtraSet[i].Tag)[1];
            AttrData attr = Data.AttrDict[eipClassCode.Index, at];
            ExtraGet[i].Enabled = enabled;
            ExtraSet[i].Enabled = enabled && int.TryParse(ExtraText[i].Text, out int val) &&
               val >= attr.Data.Min && val <= attr.Data.Max;
         }
      }

      #endregion

      #region Tab Specific Routines (Substitution)

      // Substitution Specific Controls
      bool IsSubstitution = false;
      GroupBox SubControls;
      Label lblCategory;
      ComboBox cbCategory;
      Button subGet;
      Button subSet;
      Label[][] subLabels;
      TextBox[][] subTexts;
      int[] startWith = new[] { 19, 1, 1, 0, 0, 1, 1 };
      eipSubstitution_rules[] at = new eipSubstitution_rules[] {
         eipSubstitution_rules.Year,
         eipSubstitution_rules.Month,
         eipSubstitution_rules.Day,
         eipSubstitution_rules.Hour,
         eipSubstitution_rules.Minute,
         eipSubstitution_rules.Week,
         eipSubstitution_rules.Day_Of_Week,
      };

      int lastCategory = -1;

      private void BuildSubstitutionControls() {
         if (IsSubstitution) {
            SubControls = new GroupBox() { Text = "Substitution Rules" };
            tab.Controls.Add(SubControls);
            SubControls.Paint += GroupBorder_Paint;

            lblCategory = new Label() { Text = "Category", TextAlign = ContentAlignment.TopRight };
            SubControls.Controls.Add(lblCategory);

            cbCategory = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
            SubControls.Controls.Add(cbCategory);
            cbCategory.SelectedIndexChanged += CbCategory_SelectedIndexChanged;
            for (int i = 3; i < labels.Length; i++) {
               cbCategory.Items.Add(labels[i].Text);
            }

            subGet = new Button() { Text = "Get" };
            SubControls.Controls.Add(subGet);
            subGet.Click += SubGet_Click;

            subSet = new Button() { Text = "Set" };
            SubControls.Controls.Add(subSet);
            subSet.Click += SubSet_Click;

            subLabels = new Label[][] {
               new Label[25],
               new Label[12],
               new Label[31],
               new Label[24],
               new Label[60],
               new Label[53],
               new Label[7],
            };
            subTexts = new TextBox[][] {
               new TextBox[25],
               new TextBox[12],
               new TextBox[31],
               new TextBox[24],
               new TextBox[60],
               new TextBox[53],
               new TextBox[7],
            };
            for (int i = 0; i < subLabels.GetLength(0); i++) {
               for (int j = 0; j < subLabels[i].Length; j++) {
                  subLabels[i][j] = new Label() { Text = (j + startWith[i]).ToString("D2"), Visible = false, TextAlign = ContentAlignment.TopRight };
                  SubControls.Controls.Add(subLabels[i][j]);
                  subTexts[i][j] = new TextBox() { Visible = false, TextAlign = HorizontalAlignment.Center };
                  SubControls.Controls.Add(subTexts[i][j]);
               }
            }
         }
      }

      private void SubGet_Click(object sender, EventArgs e) {
         if (lastCategory >= 0) {

         }
      }

      private void SubSet_Click(object sender, EventArgs e) {
         if (lastCategory >= 0) {
            bool OpenCloseForward = !EIP.ForwardIsOpen;
            if (OpenCloseForward) {
               EIP.ForwardOpen();
            }
            for (int i = 0; i < subLabels[lastCategory].Length; i++) {
               byte[] data = EIP.ToBytes((char)(i + startWith[lastCategory]) + subTexts[lastCategory][i].Text + "\x00");
               EIP.WriteOneAttribute(eipClassCode.Substitution_rules, (byte)at[lastCategory], data);
            }
            if (OpenCloseForward && EIP.ForwardIsOpen) {
               EIP.ForwardClose();
            }
         }
      }

      private void CbCategory_SelectedIndexChanged(object sender, EventArgs e) {
         if (lastCategory >= 0) {
            for (int i = 0; i < subLabels[lastCategory].Length; i++) {
               subLabels[lastCategory][i].Visible = false;
               subTexts[lastCategory][i].Visible = false;
            }
         }
         lastCategory = cbCategory.SelectedIndex;
         for (int i = 0; i < subLabels[lastCategory].Length; i++) {
            subLabels[lastCategory][i].Visible = true;
            subTexts[lastCategory][i].Visible = true;
         }
         ResizeSubstitutionControls(ref R);
      }

      private void ResizeSubstitutionControls(ref ResizeInfo R) {
         if (IsSubstitution) {
            int groupStart = (labels.Length + 1) * 2;
            int groupHeight = tclHeight - groupStart - 5;
            Utils.ResizeObject(ref R, SubControls, groupStart + 0.75f, 0.5f, groupHeight, tclWidth - 0.5f);
            {
               Utils.ResizeObject(ref R, lblCategory, 1, 1, 1.5f, 4);
               Utils.ResizeObject(ref R, cbCategory, 1, 5, 1.5f, 6);
               Utils.ResizeObject(ref R, subGet, 1, tclWidth - 9, 1.5f, 3);
               Utils.ResizeObject(ref R, subSet, 1, tclWidth - 5, 1.5f, 3);
            }
         }
         if (lastCategory >= 0) {
            for (int i = 0; i < subLabels[lastCategory].Length; i++) {
               int r = 3 + 2 * (int)(i / 15);
               float c = (i % 15) * 2.25f + 0.25f;
               Utils.ResizeObject(ref R, subLabels[lastCategory][i], r, c, 1.5f, 1);
               Utils.ResizeObject(ref R, subTexts[lastCategory][i], r, c + 1, 1.5f, 1.25f);
            }
         }
      }

      private void SetSubstitutionButtonEnables() {
         if (IsSubstitution) {

         }
      }

      #endregion

      #region Tab Specific Routines (User Pattern)

      // User Pattern Specific Controls
      bool IsUserPattern = false;
      GroupBox UpControls;
      Label lblUpFont;
      ComboBox cbUpFont;
      Label lblUpPosition;
      ComboBox cbUpPosition;
      Label lblUpCount;
      ComboBox cbUpCount;
      Button UpGet;
      Button UpSet;
      PictureBox pbUpMain;

      private void BuildUserPatternControls() {
         if (IsUserPattern) {
            UpControls = new GroupBox() { Text = "User Pattern Rules" };
            tab.Controls.Add(UpControls);
            UpControls.Paint += GroupBorder_Paint;

            lblUpFont = new Label() { Text = "Font", TextAlign = ContentAlignment.TopRight };
            UpControls.Controls.Add(lblUpFont);

            cbUpFont = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
            UpControls.Controls.Add(cbUpFont);
            cbUpFont.SelectedIndexChanged += cbUpFont_SelectedIndexChanged;
            for (int i = 0; i < Data.DropDowns[19].Length; i++) {
               cbUpFont.Items.Add(Data.DropDowns[19][i]);
            }

            lblUpPosition = new Label() { Text = "Position", TextAlign = ContentAlignment.TopRight };
            UpControls.Controls.Add(lblUpPosition);

            cbUpPosition = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
            UpControls.Controls.Add(cbUpPosition);
            cbUpPosition.SelectedIndexChanged += cbUpPosition_SelectedIndexChanged;
            for (int i = 0; i < 200; i++) {
               cbUpPosition.Items.Add(i.ToString("D3"));
            }

            lblUpCount = new Label() { Text = "Count", TextAlign = ContentAlignment.TopRight };
            UpControls.Controls.Add(lblUpCount);

            cbUpCount = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
            UpControls.Controls.Add(cbUpCount);
            cbUpPosition.SelectedIndexChanged += cbUpPosition_SelectedIndexChanged;
            for (int i = 1; i < 10; i++) {
               cbUpCount.Items.Add(i.ToString("D0"));
            }
            cbUpCount.SelectedIndex = 0;

            UpGet = new Button() { Text = "Get" };
            UpControls.Controls.Add(UpGet);
            UpGet.Click += UpGet_Click;

            UpSet = new Button() { Text = "Set" };
            UpControls.Controls.Add(UpSet);
            UpSet.Click += UpSet_Click;

            pbUpMain = new PictureBox();
            UpControls.Controls.Add(pbUpMain);

         }
      }

      private void cbUpPosition_SelectedIndexChanged(object sender, EventArgs e) {
         SetUpButtonEnables();
      }

      private void cbUpFont_SelectedIndexChanged(object sender, EventArgs e) {
         SetUpButtonEnables();
      }

      private void UpSet_Click(object sender, EventArgs e) {

      }

      private void UpGet_Click(object sender, EventArgs e) {
         bool OpenCloseForward = !EIP.ForwardIsOpen;
         if (OpenCloseForward) {
            EIP.ForwardOpen();
         }
         for (int i = 0; i < cbUpCount.SelectedIndex + 1; i++) {
            byte[] data = new byte[] { (byte)(cbUpFont.SelectedIndex + 1), (byte)(cbUpPosition.SelectedIndex + i) };
            AttrData attr = Data.AttrDict[eipClassCode.User_pattern, (byte)eipUser_pattern.User_Pattern_Fixed];
            bool Success = EIP.ReadOneAttribute(eipClassCode.User_pattern, (byte)eipUser_pattern.User_Pattern_Fixed, attr, data, out string val);
         }
         if (OpenCloseForward && EIP.ForwardIsOpen) {
            EIP.ForwardClose();
         }
      }

      private void ResizeUserPatternnControls(ref ResizeInfo R) {
         if (IsUserPattern) {
            pbUpMain.Image = new Bitmap((int)(R.H * 3), (int)(R.W * 3));
            pbUpMain.BackColor = Color.LightGreen;
            int groupStart = (labels.Length + 1) * 2;
            int groupHeight = tclHeight - groupStart - 5;
            Utils.ResizeObject(ref R, UpControls, groupStart + 0.75f, 0.5f, groupHeight, tclWidth - 0.5f);
            {
               Utils.ResizeObject(ref R, lblUpFont, 1, 1, 1.5f, 3);
               Utils.ResizeObject(ref R, cbUpFont, 1, 4, 1.5f, 3);
               Utils.ResizeObject(ref R, lblUpPosition, 1, 7, 1.5f, 3);
               Utils.ResizeObject(ref R, cbUpPosition, 1, 10, 1.5f, 3);
               Utils.ResizeObject(ref R, lblUpCount, 1, 13, 1.5f, 3);
               Utils.ResizeObject(ref R, cbUpCount, 1, 16, 1.5f, 3);
               Utils.ResizeObject(ref R, UpGet, 1, tclWidth - 9, 1.5f, 3);
               Utils.ResizeObject(ref R, UpSet, 1, tclWidth - 5, 1.5f, 3);
               Utils.ResizeObject(ref R, pbUpMain, 3, 1, 3, 3);
            }
         }
      }

      private void SetUpButtonEnables() {
         if (IsUserPattern) {
            bool UpEnabled = cbUpFont.SelectedIndex >= 0 && cbUpPosition.SelectedIndex >= 0;
            UpGet.Enabled = UpEnabled;
            UpSet.Enabled = UpEnabled;
         }
      }

      #endregion

   }
}
