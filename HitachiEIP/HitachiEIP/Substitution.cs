using System;
using System.Drawing;
using System.Windows.Forms;

namespace HitachiEIP {
   class Substitution {

      #region Dat Declarations

      ResizeInfo R;
      int GroupStart;
      int GroupHeight;
      int GroupWidth;
      EIP EIP;
      TabPage tab;

      string[] attributeNames;
      eipSubstitution_rules[] attributeValues;

      // Substitution Specific Controls
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

      int visibleCategory = -1;

      #endregion

      #region Constructors and destructors

      public Substitution(EIP EIP, TabPage tab) {
         this.EIP = EIP;
         this.tab = tab;
         this.attributeNames = (string[])typeof(eipSubstitution_rules).GetEnumNames();
         this.attributeValues = (eipSubstitution_rules[])typeof(eipSubstitution_rules).GetEnumValues();
      }

      #endregion

      #region Tab Specific Routines (Substitution)

      public void BuildSubstitutionControls() {
         SubControls = new GroupBox() { Text = "Substitution Rules" };
         tab.Controls.Add(SubControls);
         SubControls.Paint += GroupBorder_Paint;

         lblCategory = new Label() { Text = "Category", TextAlign = ContentAlignment.TopRight };
         SubControls.Controls.Add(lblCategory);

         cbCategory = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         SubControls.Controls.Add(cbCategory);
         cbCategory.SelectedIndexChanged += CbCategory_SelectedIndexChanged;
         for (int i = 3; i < attributeNames.Length; i++) {
            string s = $"{attributeNames[i].Replace('_', ' ')} (0x{(int)attributeValues[i]:X2})";
            cbCategory.Items.Add(s);
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

      private void SubGet_Click(object sender, EventArgs e) {
         if (visibleCategory >= 0) {

         }
      }

      private void SubSet_Click(object sender, EventArgs e) {
         if (visibleCategory >= 0) {
            bool OpenCloseForward = !EIP.ForwardIsOpen;
            if (OpenCloseForward) {
               EIP.ForwardOpen();
            }
            for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
               byte[] data = EIP.ToBytes((char)(i + startWith[visibleCategory]) + subTexts[visibleCategory][i].Text + "\x00");
               EIP.WriteOneAttribute(eipClassCode.Substitution_rules, (byte)at[visibleCategory], data);
            }
            if (OpenCloseForward && EIP.ForwardIsOpen) {
               EIP.ForwardClose();
            }
         }
      }

      private void CbCategory_SelectedIndexChanged(object sender, EventArgs e) {
         if (visibleCategory >= 0) {
            for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
               subLabels[visibleCategory][i].Visible = false;
               subTexts[visibleCategory][i].Visible = false;
            }
         }
         visibleCategory = cbCategory.SelectedIndex;
         for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
            subLabels[visibleCategory][i].Visible = true;
            subTexts[visibleCategory][i].Visible = true;
         }
         ResizeSubstitutionControls(ref R, GroupStart, GroupHeight, GroupWidth);
      }

      public void ResizeSubstitutionControls(ref ResizeInfo R, int GroupStart, int GroupHeight, int GroupWidth) {
         this.R = R;
         this.GroupStart = GroupStart;
         this.GroupHeight = GroupHeight;
         this.GroupWidth = GroupWidth;

         Utils.ResizeObject(ref R, SubControls, GroupStart + 0.75f, 0.5f, GroupHeight - 1, GroupWidth - 0.5f);
         {
            Utils.ResizeObject(ref R, lblCategory, 1, 1, 1.5f, 4);
            Utils.ResizeObject(ref R, cbCategory, 1, 5, 1.5f, 6);
            Utils.ResizeObject(ref R, subGet, 1, GroupWidth - 9, 1.5f, 3);
            Utils.ResizeObject(ref R, subSet, 1, GroupWidth - 5, 1.5f, 3);
         }
         if (visibleCategory >= 0) {
            for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
               float r = 3.5f + 2 * (int)(i / 15);
               float c = (i % 15) * 2.25f + 0.25f;
               Utils.ResizeObject(ref R, subLabels[visibleCategory][i], r, c, 1.5f, 1);
               Utils.ResizeObject(ref R, subTexts[visibleCategory][i], r, c + 1, 1.5f, 1.25f);
            }
         }
      }

      public void SetSubstitutionButtonEnables() {

      }

      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         using (Pen p = new Pen(Color.CadetBlue, 2)) {
            e.Graphics.DrawRectangle(p, 1, 1, gb.Width - 2, gb.Height - 2);
         }
      }

      #endregion

   }
}
