using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitachiEIP {
   class Attributes<t1> {

      #region Data Declarations

      const int MaxExtras = 6;
      int extrasUsed = 0;

      HitachiBrowser parent;
      EIP EIP;
      TabPage tab;
      AttrData[] attrs;
      int Extras = 0;

      t1[] attributes;
      eipClassCode cc;
      eipClassCode ccIndex = eipClassCode.Index;

      // Headers
      Label[] hdrs;

      Label[] labels;
      TextBox[] texts;
      TextBox[] counts;
      Button[] gets;
      Button[] sets;
      Button[] services;
      Button getAll;
      Button setAll;

      Label[] ExtraLabel;
      TextBox[] ExtraText;
      Button[] ExtraGet;
      Button[] ExtraSet;

      #endregion

      #region Constructors and destructors

      public Attributes(HitachiBrowser parent, EIP EIP, TabPage tab, eipClassCode cc, int[][] data, int Extras = 0) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
         this.attributes = (t1[])typeof(t1).GetEnumValues();
         this.cc = cc;
         attrs = new AttrData[data.Length];
         for (int i = 0; i < data.Length; i++) {
            attrs[i] = new AttrData(data[i]);
         }
         this.Extras = Extras;
         BuildControls();
         if (Extras > 0) {
            extrasUsed = AddExtraControls();
         }
      }

      #endregion

      #region Events handlers

      private void Get_Click(object sender, EventArgs e) {
         int tag = Convert.ToInt32(((Button)sender).Tag);
         AttrData attr = attrs[tag];
         if (attr.Ignore) {
            texts[tag].Text = "Ignored!";
            texts[tag].BackColor = Color.Pink;
            counts[tag].BackColor = Color.LightGreen;
         } else {
            texts[tag].Text = "Loading";
            if (EIP.ReadOneAttribute(cc, attr.Val, out string val, attr.Fmt)) {
               texts[tag].Text = val;
            } else {
               texts[tag].Text = "#Error";
               parent.AllGood = false;
            }
            counts[tag].Text = EIP.GetDataLength.ToString();
            if (attr.Fmt == DataFormats.Decimal) {
               if (attr.Len == EIP.GetDataLength) {
                  counts[tag].BackColor = Color.LightGreen;
               } else {
                  counts[tag].BackColor = Color.Pink;
               }
               if (EIP.GetDataLength <= 8) {
                  ulong dec = EIP.Get(EIP.GetData, 0, EIP.GetDataLength, mem.BigEndian);
                  if (attr.Max == 0 || dec >= (ulong)attr.Min && dec <= (ulong)attr.Max) {
                     texts[tag].BackColor = Color.LightGreen;
                  } else {
                     texts[tag].BackColor = Color.Pink;
                  }
               }
            } else if (attr.Fmt == DataFormats.Bytes) {
               if (attr.Len == EIP.GetDataLength) {
                  counts[tag].BackColor = Color.LightGreen;
                  texts[tag].BackColor = Color.LightGreen;
               } else {
                  counts[tag].BackColor = Color.Pink;
                  texts[tag].BackColor = Color.Pink;
               }

            } else if (attr.Fmt == DataFormats.ASCII) {
               if (attr.Len >= EIP.GetDataLength) {
                  counts[tag].BackColor = Color.LightGreen;
               } else {
                  counts[tag].BackColor = Color.Pink;
               }
               if (AllAscii(EIP.GetData)) {
                  texts[tag].BackColor = Color.LightGreen;
               } else {
                  texts[tag].BackColor = Color.Pink;
               }
            } else if (attr.Fmt == DataFormats.XY) {
               if (attr.Len == EIP.GetDataLength) {
                  counts[tag].BackColor = Color.LightGreen;
                  uint x = EIP.Get(EIP.GetData, 0, 2, mem.BigEndian);
                  uint y = EIP.Get(EIP.GetData, 2, 1, mem.BigEndian);
                  if (x <= 65535 && y <= 47) {
                     texts[tag].BackColor = Color.LightGreen;
                  } else {
                     texts[tag].BackColor = Color.Pink;
                  }
               } else {
                  counts[tag].BackColor = Color.Pink;
                  texts[tag].BackColor = Color.Pink;
               }
            } else if (attr.Fmt == DataFormats.Date) {
               if(attr.Len == EIP.GetDataLength) {
                  counts[tag].BackColor = Color.LightGreen;
               } else {
                  counts[tag].BackColor = Color.Pink;
               }
               if (EIP.GetDataLength == 12) {
                  if (DateTime.TryParse(texts[tag].Text, out DateTime d)) {
                     texts[tag].BackColor = Color.LightGreen;
                  } else {
                     texts[tag].BackColor = Color.Pink;
                  }
               } else {
                  texts[tag].BackColor = Color.Pink;
               }
            }
         }
         SetButtonEnables();
      }

      private void Set_Click(object sender, EventArgs e) {
         byte[] data;
         int tag = Convert.ToInt32(((Button)sender).Tag);
         AttrData attr = attrs[tag];
         if (attr.Ignore) {
            texts[tag].Text = "Ignored!";
         } else {
            if (attr.Fmt == DataFormats.Decimal) {
               int len = attr.Len;
               if (!int.TryParse(texts[tag].Text, out int val)) {
                  val = attr.Min;
               }
               data = EIP.ToBytes((uint)val, len);
            } else if (attr.Fmt == DataFormats.ASCII) {
               data = EIP.ToBytes(texts[tag].Text);
            } else {
               data = new byte[] { };
            }
            bool Success = EIP.WriteOneAttribute(cc, attr.Val, data);
         }
         SetButtonEnables();
      }

      private void GetAll_Click(object sender, EventArgs e) {
         parent.AllGood = true;
         for (int i = 0; i < gets.Length; i++) {
            counts[i].BackColor = SystemColors.Control;
            if (gets[i] != null) {
               texts[i].Text = "Loading";
               texts[i].BackColor = SystemColors.Control;
            }
         }
         parent.Refresh();
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
         SetButtonEnables();
      }

      private void SetAll_Click(object sender, EventArgs e) {
         parent.AllGood = true;
         for (int i = 0; i < sets.Length && parent.AllGood; i++) {
            if (gets[i] != null) {
               Set_Click(sets[i], null);
               parent.Refresh();
               Application.DoEvents();
            }
         }
         if (!parent.AllGood) {
            parent.EIP_Log(null, "SetAll completed abnormally");
         }
      }

      private void NumbersOnly_KeyPress(object sender, KeyPressEventArgs e) {
         TextBox t = (TextBox)sender;
         e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
      }

      private void GetExtras_Click(object sender, EventArgs e) {
         Button b = (Button)sender;
         byte n = ((byte[])b.Tag)[0];
         byte at = ((byte[])b.Tag)[1];
         ExtraText[n].Text = "Loading";
         if (EIP.ReadOneAttribute(ccIndex, at, out string val, DataFormats.Decimal)) {
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
         AttrData attr = Data.GetAttrData((byte)eipClassCode.Index, at);
         int len = attr.Len;
         if (!int.TryParse(ExtraText[n].Text, out int val)) {
            val = attr.Min;
         }
         byte[] data = EIP.ToBytes((uint)val, len);
         bool Success = EIP.WriteOneAttribute(ccIndex, attr.Val, data);
      }

      #endregion

      #region Service Routines

      private bool AllAscii(byte[] s) {
         bool result = true;
         for (int i = 0; i < s.Length; i++) {
            result &= s[i] >= 0x20 && s[i] < 0x80;
         }
         return result;
      }

      private void BuildControls() {

         // build headers
         if (attributes.Length > 17) {
            hdrs = new Label[8];
         } else {
            hdrs = new Label[4];
         }
         hdrs[0] = new Label() { Text = "Attributes", TextAlign = System.Drawing.ContentAlignment.TopRight};
         hdrs[1] = new Label() { Text = "#", TextAlign = System.Drawing.ContentAlignment.TopCenter };
         hdrs[2] = new Label() { Text = "Data", TextAlign = System.Drawing.ContentAlignment.TopCenter };
         hdrs[3] = new Label() { Text = "Control", TextAlign = System.Drawing.ContentAlignment.TopCenter };
         if (attributes.Length > 17) {
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
         counts = new TextBox[attributes.Length];
         gets = new Button[attributes.Length];
         sets = new Button[attributes.Length];
         services = new Button[attributes.Length];

         for (int i = 0; i < attributes.Length; i++) {
            AttrData attr = attrs[i];
            string s = $"{attributes[i].ToString().Replace('_', ' ')} (0x{attr.Val:X2})";
            labels[i] = new Label() { Tag = i, TextAlign = System.Drawing.ContentAlignment.TopRight,
                                      Text = s };
            tab.Controls.Add(labels[i]);
            texts[i] = new TextBox() { Tag = i, TextAlign = HorizontalAlignment.Center };
            tab.Controls.Add(texts[i]);

            counts[i] = new TextBox() { Tag = i, TextAlign = HorizontalAlignment.Center, Text = attr.Len.ToString() };
            tab.Controls.Add(counts[i]);

            if (attr.HasGet) {
               gets[i] = new Button() { Tag = i, Text = "Get" };
               gets[i].Click += Get_Click;
               tab.Controls.Add(gets[i]);
            }
            if (attr.HasSet) {
               sets[i] = new Button() { Tag = i, Text = "Set" };
               sets[i].Click += Set_Click;
               tab.Controls.Add(sets[i]);
               if (attr.Fmt == DataFormats.Decimal) {
                  texts[i].KeyPress += NumbersOnly_KeyPress;
               }
            } else {
               texts[i].ReadOnly = true;
            }
            if (attr.HasService) {
               services[i] = new Button() { Tag = i, Text = "Service" };
               services[i].Click += Set_Click;
               tab.Controls.Add(services[i]);
            }
         }

         getAll = new Button() { Text = "Get All" };
         getAll.Click += GetAll_Click;
         tab.Controls.Add(getAll);
         setAll = new Button() { Text = "Set All" };
         tab.Controls.Add(setAll);
         setAll.Click += SetAll_Click;

      }

      private int AddExtraControls() {
         byte n = 0;
         ExtraLabel = new Label[MaxExtras];
         ExtraText = new TextBox[MaxExtras];
         ExtraGet = new Button[MaxExtras];
         ExtraSet = new Button[MaxExtras];

         if ((Extras & HitachiBrowser.AddItem) > 0) {
            ExtraLabel[n] = new Label() { TextAlign = ContentAlignment.TopRight, Text = eipIndex.Item_Count.ToString().Replace('_', ' ') };
            ExtraText[n] = new TextBox() { TextAlign = HorizontalAlignment.Center };
            ExtraGet[n] = new Button() { Text = "Get", Tag = new byte[] { n, (byte)eipIndex.Item_Count } };
            ExtraSet[n] = new Button() { Text = "Set", Tag = new byte[] { n, (byte)eipIndex.Item_Count } };
            ExtraGet[n].Click += GetExtras_Click;
            ExtraSet[n].Click += SetExtras_Click;
            n++;
         }
         if ((Extras & HitachiBrowser.AddColumn) > 0) {
            ExtraLabel[n] = new Label() { TextAlign = ContentAlignment.TopRight, Text = eipIndex.Column.ToString().Replace('_', ' ') };
            ExtraText[n] = new TextBox() { TextAlign = HorizontalAlignment.Center };
            ExtraGet[n] = new Button() { Text = "Get", Tag = new byte[] { n, (byte)eipIndex.Column } };
            ExtraSet[n] = new Button() { Text = "Set", Tag = new byte[] { n, (byte)eipIndex.Column } };
            ExtraGet[n].Click += GetExtras_Click;
            ExtraSet[n].Click += SetExtras_Click;
            n++;
         }
         if ((Extras & HitachiBrowser.AddLine) > 0) {
            ExtraLabel[n] = new Label() { TextAlign = ContentAlignment.TopRight, Text = eipIndex.Line.ToString().Replace('_', ' ') };
            ExtraText[n] = new TextBox() { TextAlign = HorizontalAlignment.Center };
            ExtraGet[n] = new Button() { Text = "Get", Tag = new byte[] { n, (byte)eipIndex.Line } };
            ExtraSet[n] = new Button() { Text = "Set", Tag = new byte[] { n, (byte)eipIndex.Line } };
            ExtraGet[n].Click += GetExtras_Click;
            ExtraSet[n].Click += SetExtras_Click;
            n++;
         }
         if ((Extras & HitachiBrowser.AddPosition) > 0) {
            ExtraLabel[n] = new Label() { TextAlign = ContentAlignment.TopRight, Text = eipIndex.Character_position.ToString().Replace('_', ' ') };
            ExtraText[n] = new TextBox() { TextAlign = HorizontalAlignment.Center };
            ExtraGet[n] = new Button() { Text = "Get", Tag = new byte[] { n, (byte)eipIndex.Character_position } };
            ExtraSet[n] = new Button() { Text = "Set", Tag = new byte[] { n, (byte)eipIndex.Character_position } };
            ExtraGet[n].Click += GetExtras_Click;
            ExtraSet[n].Click += SetExtras_Click;
            n++;
         }
         if ((Extras & HitachiBrowser.AddCalendar) > 0) {
            ExtraLabel[n] = new Label() { TextAlign = ContentAlignment.TopRight, Text = eipIndex.Calendar_Block.ToString().Replace('_', ' ') };
            ExtraText[n] = new TextBox() { TextAlign = HorizontalAlignment.Center };
            ExtraGet[n] = new Button() { Text = "Get", Tag = new byte[] { n, (byte)eipIndex.Calendar_Block } };
            ExtraSet[n] = new Button() { Text = "Set", Tag = new byte[] { n, (byte)eipIndex.Calendar_Block } };
            ExtraGet[n].Click += GetExtras_Click;
            ExtraSet[n].Click += SetExtras_Click;
            n++;
         }
         if ((Extras & HitachiBrowser.AddCount) > 0) {
            ExtraLabel[n] = new Label() { TextAlign = ContentAlignment.TopRight, Text = eipIndex.Count_Block.ToString().Replace('_', ' ') };
            ExtraText[n] = new TextBox() { TextAlign = HorizontalAlignment.Center };
            ExtraGet[n] = new Button() { Text = "Get", Tag = new byte[] { n, (byte)eipIndex.Count_Block } };
            ExtraSet[n] = new Button() { Text = "Set", Tag = new byte[] { n, (byte)eipIndex.Count_Block } };
            ExtraGet[n].Click += GetExtras_Click;
            ExtraSet[n].Click += SetExtras_Click;
            n++;
         }
         for(int i = 0; i < n; i++) {
            tab.Controls.Add(ExtraLabel[i]);
            tab.Controls.Add(ExtraText[i]);
            tab.Controls.Add(ExtraGet[i]);
            tab.Controls.Add(ExtraSet[i]);
         }
         return n; 
      }

      public void ResizeControls(ref ResizeInfo R) {
         if (parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         parent.tclClasses.Visible = false;
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         int half = 15;
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
            Utils.ResizeObject(ref R, labels[i], r, 0.25f + c * cw, 2, 8);
            Utils.ResizeObject(ref R, counts[i], r, 8.25f + c * cw, 1.5f, 1);
            Utils.ResizeObject(ref R, texts[i], r, 9.5f + c * cw, 1.5f, 4.75f);
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
         Utils.ResizeObject(ref R, getAll, tclHeight - 3, 27, 2.5f, 4);
         Utils.ResizeObject(ref R, setAll, tclHeight - 3, 31.5f, 2.5f, 4);

         if (extrasUsed > 0) {
            int r = tclHeight - 2 *Math.Min( extrasUsed, 3);
            int c = 0;
            for (int i = 0; i < extrasUsed; i++) {
               Utils.ResizeObject(ref R, ExtraLabel[i], r, 0.25f + c, 2, 4);
               Utils.ResizeObject(ref R, ExtraText[i], r, 4.5f + c, 1.5f, 2);
               Utils.ResizeObject(ref R, ExtraGet[i], r, 7 + c, 1.5f, 2);
               Utils.ResizeObject(ref R, ExtraSet[i], r, 9.5f + c, 1.5f, 2);
               if (i == 2) {
                  r = tclHeight - 2 * Math.Min(extrasUsed, 3);
                  c = 12;
               } else {
                  r += 2;
               }
            }
         }
         parent.tclClasses.Visible = true;
      }

      public void RefreshExtras() {
         bool enabled = parent.ComIsOn & EIP.SessionIsOpen;
         if (!enabled | parent.tclClasses.SelectedIndex != parent.tclClasses.TabPages.IndexOf(tab)) {
            return;
         }
         for (int i = 0; i < extrasUsed; i++) {
            GetExtras_Click(ExtraGet[i], null);
         }
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
               int min = attr.Min;
               int max = attr.Max;
               bool validData = true;
               if(attr.Fmt == DataFormats.Decimal && max > 0) {
                  if (int.TryParse(texts[i].Text, out int val)) {
                     validData = val >= min && val <= max;
                  } else {
                     validData = false;
                  }
               }
               sets[i].Enabled = enable && validData;
               anySets |= enable;
            }
            if (attr.HasGet) {
               gets[i].Enabled = enable;
               anyGets |= enable;
            }
            if (attr.HasService) {
               services[i].Enabled = enable;
            }
         }
         setAll.Enabled = anySets;
         getAll.Enabled = anyGets;

         for (int i = 0; i < extrasUsed; i++) {
            ExtraGet[i].Enabled = enable;
            ExtraSet[i].Enabled = enable;
         }
      }

      #endregion

   }
}
