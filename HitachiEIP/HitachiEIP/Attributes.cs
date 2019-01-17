using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitachiEIP {
   class Attributes<t1> {

      #region Data Declarations

      HitachiBrowser parent;
      EIP EIP;
      TabPage tab;

      t1[] attributes;
      eipClassCode cc;

      int[,] validData;

      Label[] labels;
      TextBox[] texts;
      Button[] gets;
      Button[] sets;
      Button[] services;
      Button getAll;
      Button setAll;

      #endregion

      #region Constructors and destructors

      public Attributes(HitachiBrowser parent, EIP EIP, TabPage tab, eipClassCode cc) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
         this.attributes = (t1[])typeof(t1).GetEnumValues();
         this.cc = cc;
         BuildControls();
      }

      #endregion

      #region Events handlers

      private void Get_Click(object sender, EventArgs e) {
         int tag = Convert.ToInt32(((Button)sender).Tag);
         texts[tag].Text = "Loading";
         ulong attr = Convert.ToUInt64(attributes[tag]);
         if (EIP.ReadOneAttribute(cc, (byte)attr, out string val, EIP.GetFmt(attr))) {
            texts[tag].Text = val;
         } else {
            texts[tag].Text = "#Error";
         }
         SetButtonEnables();
      }

      private void Set_Click(object sender, EventArgs e) {
         byte[] data;
         int tag = Convert.ToInt32(((Button)sender).Tag);
         ulong attr = Convert.ToUInt64(attributes[tag]);
         DataFormats fmt = EIP.GetFmt(attr);
         if (fmt == DataFormats.Decimal) {
            int len = EIP.GetDataLength(attr);
            if (!int.TryParse(texts[tag].Text, out int val)) {
               val = EIP.GetMin(attr);
            }
            data = EIP.ToBytes((uint)val, len);
         } else if(fmt == DataFormats.ASCII) {
            data = EIP.ToBytes(texts[tag].Text);
         } else {
            data = new byte[] { };
         }
         bool Success = EIP.WriteOneAttribute(cc, (byte)attr, data);
         SetButtonEnables();
      }

      private void GetAll_Click(object sender, EventArgs e) {
         for (int i = 0; i < gets.Length; i++) {
            if (gets[i] != null) {
               Get_Click(gets[i], null);
               parent.Refresh();
            }
         }
         SetButtonEnables();
      }

      private void SetAll_Click(object sender, EventArgs e) {
         for (int i = 0; i < sets.Length; i++) {
            if (gets[i] != null) {
               Set_Click(sets[i], null);
               parent.Refresh();
            }
         }
      }

      private void NumbersOnly_KeyPress(object sender, KeyPressEventArgs e) {
         TextBox t = (TextBox)sender;
         e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
      }

      #endregion

      #region Service Routines

      public void BuildControls() {

         validData = new int[attributes.Length, 2];
         labels = new Label[attributes.Length];
         texts = new TextBox[attributes.Length];
         gets = new Button[attributes.Length];
         sets = new Button[attributes.Length];
         services = new Button[attributes.Length];

         for (int i = 0; i < attributes.Length; i++) {
            ulong attr = Convert.ToUInt64(attributes[i]);
            labels[i] = new Label() { Tag = i, TextAlign = System.Drawing.ContentAlignment.TopRight, Text = EIP.GetAttributeName(cc, Convert.ToUInt64(attributes[i])) };
            tab.Controls.Add(labels[i]);
            texts[i] = new TextBox() { Tag = i, TextAlign = HorizontalAlignment.Center };
            tab.Controls.Add(texts[i]);
            if (EIP.HasGet(attr)) {
               gets[i] = new Button() { Tag = i, Text = "Get" };
               gets[i].Click += Get_Click;
               tab.Controls.Add(gets[i]);
            }
            if (EIP.HasSet(attr)) {
               sets[i] = new Button() { Tag = i, Text = "Set" };
               sets[i].Click += Set_Click;
               tab.Controls.Add(sets[i]);
               if (EIP.GetFmt(attr) == DataFormats.Decimal) {
                  texts[i].KeyPress += NumbersOnly_KeyPress;
               }
            } else {
               texts[i].ReadOnly = true;
            }
            if (EIP.HasService(attr)) {
               services[i] = new Button() { Tag = i, Text = "Service" };
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

      public void ResizeControls(ref ResizeInfo R) {
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         int half = 17;
         for (int i = 0; i < labels.Length; i++) {
            int r;
            int c;
            float cw = 12.5f;
            if (i < half) {
               r = 2 + i * 2;
               c = 0;
            } else {
               r = 2 + (i - half) * 2;
               c = 1;
            }
            Utils.ResizeObject(ref R, labels[i], r, 0.25f + c * cw, 1.5f, 5.75f);
            Utils.ResizeObject(ref R, texts[i], r, 6.5f + c * cw, 1.5f, 2);
            if (gets[i] != null) {
               Utils.ResizeObject(ref R, gets[i], r, 9 + c * cw, 1.5f, 1.5f);
            }
            if (sets[i] != null) {
               Utils.ResizeObject(ref R, sets[i], r, 11 + c * cw, 1.5f, 1.5f);
            }
            if (services[i] != null) {
               Utils.ResizeObject(ref R, services[i], r, 9 + c * cw, 1.5f, 3.5f);
            }
         }
         Utils.ResizeObject(ref R, getAll, tclHeight - 3, 17, 2.5f, 4);
         Utils.ResizeObject(ref R, setAll, tclHeight - 3, 21.5f, 2.5f, 4);
      }

      public void SetButtonEnables() {
         bool enable = parent.ComIsOn & EIP.SessionIsOpen;
         bool anySets = false;
         bool anyGets = false;
         for (int i = 0; i < attributes.Length; i++) {
            ulong attr = Convert.ToUInt64(attributes[i]);
            if (EIP.HasSet(attr)) {
               DataFormats fmt = EIP.GetFmt(attr);
               int min = EIP.GetMin(attr);
               int max = EIP.GetMax(attr);
               bool validData = true;
               if(fmt == DataFormats.Decimal && max > 0) {
                  if (int.TryParse(texts[i].Text, out int val)) {
                     validData = val >= min && val <= max;
                  } else {
                     validData = false;
                  }
               }
               sets[i].Enabled = enable && validData;
               anySets |= enable;
            }
            if (EIP.HasGet(attr)) {
               gets[i].Enabled = enable;
               anyGets |= enable;
            }
            if (EIP.HasService(attr)) {
               services[i].Enabled = enable;
            }
         }
         setAll.Enabled = anySets;
         getAll.Enabled = anyGets;
      }

      #endregion

   }
}
