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
         if (EIP.ReadOneAttribute(cc, (byte)Convert.ToUInt32(attributes[tag]), out string val, DataFormats.Decimal)) {
            texts[tag].Text = val;
         } else {
            texts[tag].Text = "#Error";
         }
         SetButtonEnables();
      }

      private void Set_Click(object sender, EventArgs e) {
         int tag = Convert.ToInt32(((Button)sender).Tag);
         if (!int.TryParse(texts[tag].Text, out int val)) {
            val = validData[tag, 0];
         }
         int len = (Convert.ToInt32(attributes[tag]) & 0xFF0000) >> 16;
         bool Success = EIP.WriteOneAttribute(cc, (byte)Convert.ToUInt32(attributes[tag]), EIP.ToBytes((uint)val, len));
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
            labels[i] = new Label() { Tag = i, TextAlign = System.Drawing.ContentAlignment.TopRight, Text = EIP.GetAttributeName(cc, Convert.ToUInt32(attributes[i])) };
            tab.Controls.Add(labels[i]);
            texts[i] = new TextBox() { Tag = i };
            tab.Controls.Add(texts[i]);
            uint attr = Convert.ToUInt32(attributes[i]);
            if ((attr & 0x200) > 0) {
               gets[i] = new Button() { Tag = i, Text = "Get" };
               gets[i].Click += Get_Click;
               tab.Controls.Add(gets[i]);
            }
            if ((attr & 0x100) > 0) {
               sets[i] = new Button() { Tag = i, Text = "Set" };
               tab.Controls.Add(sets[i]);
            } else {
               texts[i].ReadOnly = true;
            }
            if ((attr & 0x400) > 0) {
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
         Utils.ResizeControls(ref R, tab, labels, texts, gets, sets, services, getAll, setAll);
      }

      public void SetButtonEnables() {
         bool enable = parent.ComIsOn & EIP.SessionIsOpen;
         bool anySets = false;
         bool anyGets = false;
         for (int i = 0; i < attributes.Length; i++) {
            if (sets[i] != null) {
               sets[i].Enabled = enable;
               anySets |= enable;
            }
            if (gets[i] != null) {
               gets[i].Enabled = enable;
               anyGets |= enable;
            }
            if (services[i] != null) {
               services[i].Enabled = enable;
            }
         }
         setAll.Enabled = anySets;
         getAll.Enabled = anyGets;
      }

      #endregion

   }
}
