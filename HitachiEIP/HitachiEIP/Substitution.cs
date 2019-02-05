using System;
using System.Drawing;
using System.Windows.Forms;

namespace HitachiEIP {
   class Substitution {

      #region Data Declarations

      ResizeInfo R;

      HitachiBrowser parent;
      EIP EIP;
      TabPage tab;

      string[] attributeNames;
      eipSubstitution_rules[] attributeValues;

      // Substitution Specific Controls
      GroupBox SubControls;
      Label lblRule;
      ComboBox cbRule;
      Label lblAttribute;
      ComboBox cbAttribute;
      Button subGet;
      Button subSet;
      Label[][] subLabels;
      TextBox[][] subTexts;

      bool[] resizeNeeded = new bool[] { true, true, true, true, true, true, true };
      readonly int[] startWith = new[] { 19, 1, 1, 0, 0, 1, 1 };
      readonly eipSubstitution_rules[] at = new eipSubstitution_rules[] {
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

      public Substitution(HitachiBrowser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
         this.attributeNames = (string[])typeof(eipSubstitution_rules).GetEnumNames();
         this.attributeValues = (eipSubstitution_rules[])typeof(eipSubstitution_rules).GetEnumValues();
      }

      #endregion

      #region Routines called from parent

      public void BuildSubstitutionControls() {
         SubControls = new GroupBox() { Text = "Substitution Rules" };
         tab.Controls.Add(SubControls);
         SubControls.Paint += GroupBorder_Paint;

         lblRule = new Label() { Text = "Substitution Rule", TextAlign = ContentAlignment.TopRight };
         SubControls.Controls.Add(lblRule);

         cbRule = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         SubControls.Controls.Add(cbRule);
         for (int i = 1; i < 100; i++) {
            cbRule.Items.Add(i.ToString());
         }
         cbRule.Click += cbRule_Click;

         lblAttribute = new Label() { Text = "Attribute", TextAlign = ContentAlignment.TopRight };
         SubControls.Controls.Add(lblAttribute);

         cbAttribute = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         SubControls.Controls.Add(cbAttribute);
         cbAttribute.SelectedIndexChanged += CbCategory_SelectedIndexChanged;
         for (int i = 3; i < attributeNames.Length; i++) {
            string s = $"{attributeNames[i].Replace('_', ' ')} (0x{(int)attributeValues[i]:X2})";
            cbAttribute.Items.Add(s);
         }
         cbAttribute.Click += cbAttribute_Click;

         subGet = new Button() { Text = "Get" };
         SubControls.Controls.Add(subGet);
         subGet.Click += SubGet_Click;

         subSet = new Button() { Text = "Set" };
         SubControls.Controls.Add(subSet);
         subSet.Click += SubSet_Click;

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
               SubControls.Controls.Add(subLabels[i][j]);
               subTexts[i][j] = new TextBox() { Visible = false, TextAlign = HorizontalAlignment.Center };
               SubControls.Controls.Add(subTexts[i][j]);
            }
         }
      }

      public void ResizeSubstitutionControls(ref ResizeInfo R, int GroupStart, int GroupHeight, int GroupWidth) {
         this.R = R;

         Utils.ResizeObject(ref R, SubControls, GroupStart + 0.75f, 0.5f, GroupHeight - 1, GroupWidth - 0.5f);
         {
            Utils.ResizeObject(ref R, lblRule, 1, 1, 1.5f, 4);
            Utils.ResizeObject(ref R, cbRule, 1, 5, 1.5f, 4);
            Utils.ResizeObject(ref R, lblAttribute, 1, 9, 1.5f, 4);
            Utils.ResizeObject(ref R, cbAttribute, 1, 13, 1.5f, 6);
            for (int i = 0; i < resizeNeeded.Length; i++) {
               resizeNeeded[i] = true;
            }
            resizeSubstitutions(ref R);
            Utils.ResizeObject(ref R, subGet, 1, GroupWidth - 9, 1.5f, 3);
            Utils.ResizeObject(ref R, subSet, 1, GroupWidth - 5, 1.5f, 3);
         }
      }

      public void SetButtonEnables() {
         bool eipEnabled = parent.ComIsOn & EIP.SessionIsOpen;
         bool subEnabled = cbRule.SelectedIndex > 0 && cbAttribute.SelectedIndex > 0;
         subGet.Enabled = eipEnabled && subEnabled;
         subSet.Enabled = eipEnabled && subEnabled;
      }

      #endregion

      #region Form Control routines

      private void SubGet_Click(object sender, EventArgs e) {
         if (visibleCategory >= 0) {

         }
         SetButtonEnables();
      }

      private void SubSet_Click(object sender, EventArgs e) {
         byte[] data;
         if (visibleCategory >= 0) {
            // Save the state on entry
            EIP.ForwardOpen(true);
            // Set the correct substitution Rule
            data = EIP.ToBytes((uint)cbRule.SelectedIndex + 1, 1);
            EIP.WriteOneAttribute(eipClassCode.Index, (byte)eipIndex.Substitution_Rules_Setting, data);
            // Send the substitution data one at a time
            for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
               data = EIP.Merge(EIP.ToBytes((uint)(i + startWith[visibleCategory]), 1),
                                EIP.ToBytes(subTexts[visibleCategory][i].Text + "\x00"));
               EIP.WriteOneAttribute(eipClassCode.Substitution_rules, (byte)at[visibleCategory], data);
            }
            // Restore the state
            EIP.ForwardClose(true);
         }
         SetButtonEnables();
      }

      private void CbCategory_SelectedIndexChanged(object sender, EventArgs e) {
         if (visibleCategory >= 0) {
            for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
               subLabels[visibleCategory][i].Visible = false;
               subTexts[visibleCategory][i].Visible = false;
            }
         }
         visibleCategory = cbAttribute.SelectedIndex;
         for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
            subLabels[visibleCategory][i].Visible = true;
            subTexts[visibleCategory][i].Visible = true;
         }
         resizeSubstitutions(ref R);
         SetButtonEnables();
      }

      // Make the group box be more visible
      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         using (Pen p = new Pen(Color.CadetBlue, 2)) {
            e.Graphics.DrawRectangle(p, 1, 1, gb.Width - 2, gb.Height - 2);
         }
      }

      // Class attribute changed.  
      private void cbAttribute_Click(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // Substitution rule number changed.
      private void cbRule_Click(object sender, EventArgs e) {
         SetButtonEnables();
      }

      #endregion

      #region Service Routines

      // Call o0n resize or category change
      private void resizeSubstitutions(ref ResizeInfo R) {
         if (visibleCategory >= 0 && resizeNeeded[visibleCategory]) {
            resizeNeeded[visibleCategory] = false;
            for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
               float r = 3.5f + 2 * (int)(i / 15);
               float c = (i % 15) * 2.25f + 0.25f;
               Utils.ResizeObject(ref R, subLabels[visibleCategory][i], r, c, 1.5f, 1);
               Utils.ResizeObject(ref R, subTexts[visibleCategory][i], r, c + 1, 1.5f, 1.25f);
            }
         }
      }

      #endregion

   }
}
