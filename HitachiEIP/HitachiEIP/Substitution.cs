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
      readonly ccSR[] at = new ccSR[] {
         ccSR.Year,
         ccSR.Month,
         ccSR.Day,
         ccSR.Hour,
         ccSR.Minute,
         ccSR.Week,
         ccSR.Day_Of_Week,
      };

      int visibleCategory = -1;

      #endregion

      #region Constructors and destructors

      public Substitution(HitachiBrowser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
      }

      #endregion

      #region Routines called from parent

      // Build all controls unique to this class
      public void BuildSubstitutionControls() {
         string[] attributeNames = (string[])typeof(ccSR).GetEnumNames();
         ccSR[] attributeValues = (ccSR[])typeof(ccSR).GetEnumValues();

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
         cbAttribute.SelectedIndexChanged += cbCategory_SelectedIndexChanged;
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
               subTexts[i][j] = new TextBox() { Visible = false, TextAlign = HorizontalAlignment.Center };
            }
            SubControls.Controls.AddRange(subLabels[i]);
            SubControls.Controls.AddRange(subTexts[i]);
         }
      }

      // Adjust for screen resolution
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

      // Allow button clicke only if conditions allow it
      public void SetButtonEnables() {
         bool eipEnabled = parent.ComIsOn & EIP.SessionIsOpen;
         bool subEnabled = cbRule.SelectedIndex > 0 && cbAttribute.SelectedIndex > 0;
         subGet.Enabled = eipEnabled && subEnabled;
         subSet.Enabled = eipEnabled && subEnabled;
      }

      #endregion

      #region Form Control routines

      // Get the substitution rule data
      private void SubGet_Click(object sender, EventArgs e) {
         byte[] data;
         if (visibleCategory >= 0) {
            // Save the state on entry
            EIP.ForwardOpen(true);
            // Set the correct substitution Rule
            data = EIP.ToBytes((uint)cbRule.SelectedIndex + 1, 1);
            EIP.WriteOneAttribute(ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting, data);
            // Get the substitution all at once
            EIP.ReadOneAttribute(ClassCode.Substitution_rules, (byte)at[visibleCategory], EIP.Nodata, out string dataIn);

            // <TODO> decode the input once data is returned by the printer

            // Restore the state
            EIP.ForwardClose(true);
         }
         SetButtonEnables();
      }

      // Set the substitution rule data
      private void SubSet_Click(object sender, EventArgs e) {
         byte[] data;
         if (visibleCategory >= 0) {
            // Save the state on entry
            EIP.ForwardOpen(true);
            // Set the correct substitution Rule
            data = EIP.ToBytes((uint)cbRule.SelectedIndex + 1, 1);
            EIP.WriteOneAttribute(ClassCode.Index, (byte)ccIDX.Substitution_Rules_Setting, data);
            // Send the substitution data one at a time
            for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
               data = EIP.Merge(EIP.ToBytes((uint)(i + startWith[visibleCategory]), 1),
                                EIP.ToBytes(subTexts[visibleCategory][i].Text + "\x00"));
               EIP.WriteOneAttribute(ClassCode.Substitution_rules, (byte)at[visibleCategory], data);
            }
            // Restore the state
            EIP.ForwardClose(true);
         }
         SetButtonEnables();
      }

      // Hide the old controls and make the new ones visible
      private void cbCategory_SelectedIndexChanged(object sender, EventArgs e) {
         // Hide the current set of controls
         if (visibleCategory >= 0) {
            for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
               subLabels[visibleCategory][i].Visible = false;
               subTexts[visibleCategory][i].Visible = false;
            }
         }
         // Show the new set of controls
         visibleCategory = cbAttribute.SelectedIndex;
         for (int i = 0; i < subLabels[visibleCategory].Length; i++) {
            subLabels[visibleCategory][i].Visible = true;
            subTexts[visibleCategory][i].Visible = true;
         }
         resizeSubstitutions(ref R);
         SetButtonEnables();
      }

      // Make the group box more visible
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

      // Called on resize or category change
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
