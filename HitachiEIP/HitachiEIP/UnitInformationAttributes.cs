using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitachiEIP {
   class UnitInformationAttributes {

      #region Data Declarations

      Form1 parent;
      EIP EIP;
      TabPage tab;

      eipUnit_Information[] attributes = new eipUnit_Information[] {
         eipUnit_Information.Unit_Information,
         eipUnit_Information.Model_Name,
         eipUnit_Information.Serial_Number,
         eipUnit_Information.Ink_Name,
         eipUnit_Information.Input_Mode,
         eipUnit_Information.Maximum_Character_Count,
         eipUnit_Information.Maximum_Registered_Message_Count,
         eipUnit_Information.Barcode_Information,
         eipUnit_Information.Usable_Character_Size,
         eipUnit_Information.Maximum_Calendar_And_Count,
         eipUnit_Information.Maximum_Substitution_Rule,
         eipUnit_Information.Shift_Code_And_Time_Count,
         eipUnit_Information.Chimney_And_DIN_Print,
         eipUnit_Information.Maximum_Line_Count,
         eipUnit_Information.Basic_Software_Version,
         eipUnit_Information.Controller_Software_Version,
         eipUnit_Information.Engine_M_Software_Version,
         eipUnit_Information.Engine_S_Software_Version,
         eipUnit_Information.First_Language_Version,
         eipUnit_Information.Second_Language_Version,
     };

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

      public UnitInformationAttributes(Form1 parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
         BuildControls();
      }

      #endregion

      #region Events handlers

      private void Get_Click(object sender, EventArgs e) {
         int tag = Convert.ToInt32(((Button)sender).Tag);
         texts[tag].Text = "Loading";
         if (EIP.ReadOneAttribute(eipClassCode.Unit_Information, (byte)attributes[tag], out string val, DataFormats.Decimal)) {
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
         int len = ((int)attributes[tag] & 0xFF0000) >> 16;
         bool Success = EIP.WriteOneAttribute(eipClassCode.Unit_Information, (byte)attributes[tag], EIP.ToBytes((uint)val, len));
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
            labels[i] = new Label() { Tag = i, TextAlign = System.Drawing.ContentAlignment.TopRight, Text = EIP.GetAttributeName(eipClassCode.Unit_Information, (uint)attributes[i]) };
            tab.Controls.Add(labels[i]);
            texts[i] = new TextBox() { Tag = i };
            tab.Controls.Add(texts[i]);
            if (((uint)attributes[i] & 0x200) > 0) {
               gets[i] = new Button() { Tag = i, Text = "Get" };
               gets[i].Click += Get_Click;
               tab.Controls.Add(gets[i]);
            }
            if (((uint)attributes[i] & 0x100) > 0) {
               sets[i] = new Button() { Tag = i, Text = "Set" };
               tab.Controls.Add(sets[i]);
            } else {
               texts[i].ReadOnly = true;
            }
            if (((uint)attributes[i] & 0x400) > 0) {
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

      private void SetButtonEnables() {

      }

      #endregion

   }
}
