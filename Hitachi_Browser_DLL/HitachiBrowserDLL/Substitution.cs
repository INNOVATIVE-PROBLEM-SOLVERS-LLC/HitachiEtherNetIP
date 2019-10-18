using System;
using System.Drawing;
using System.Windows.Forms;

namespace EIP_Lib {
   class Substitution {

      #region Data Declarations

      ResizeInfo R;
      int GroupWidth = 0;
      readonly Browser parent;
      readonly EIP EIP;
      readonly TabPage tab;

      // Substitution Specific Controls
      GroupBox SubControls;
      Label lblAttribute;
      ComboBox cbAttribute;
      Button subGet;
      Button subSet;
      Label[][] subLabels;
      TextBox[][] subTexts;

      readonly bool[] resizeNeeded = new bool[] { true, true, true, true, true, true, true };
      readonly int[] startWith = new[] { 0, 1, 1, 0, 0, 1, 1 };
      readonly ccSR[] at = new ccSR[] {
         ccSR.Year,
         ccSR.Month,
         ccSR.Day,
         ccSR.Hour,
         ccSR.Minute,
         ccSR.Week,
         ccSR.DayOfWeek,
      };

      // Visible Category
      int vCat = -1;

      readonly System.Drawing.Font courier = new System.Drawing.Font("Courier New", 9);

      #endregion

      #region Constructors and destructors

      public Substitution(Browser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
      }

      #endregion

      #region Routines called from parent

      // Build all controls unique to this class
      public void BuildControls() {
         string[] attributeNames = (string[])typeof(ccSR).GetEnumNames();
         ccSR[] attributeValues = (ccSR[])typeof(ccSR).GetEnumValues();

         SubControls = new GroupBox() { Text = "Substitution Rules" };
         tab.Controls.Add(SubControls);
         SubControls.Paint += GroupBorder_Paint;

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
               subTexts[i][j] = new TextBox() { Visible = false, TextAlign = HorizontalAlignment.Center, Font = courier };
               subTexts[i][j].Enter += TextBox_Enter;
            }
            SubControls.Controls.AddRange(subLabels[i]);
            SubControls.Controls.AddRange(subTexts[i]);
         }
      }

      // Adjust for screen resolution
      public void ResizeControls(ref ResizeInfo R, float GroupStart, float GroupHeight, int GroupWidth) {
         this.R = R;
         this.GroupWidth = GroupWidth;

         Utils.ResizeObject(ref R, SubControls, GroupStart + 0.75f, 0.5f, GroupHeight - 1, GroupWidth - 0.5f);
         {
            for (int i = 0; i < resizeNeeded.Length; i++) {
               resizeNeeded[i] = true;
            }
            resizeSubstitutions(ref R);
         }
      }

      // Allow button clicke only if conditions allow it
      public void SetButtonEnables() {
         bool eipEnabled = parent.ComIsOn;
         bool subEnabled = cbAttribute.SelectedIndex >= 0;
         subGet.Enabled = eipEnabled && subEnabled;
         subSet.Enabled = eipEnabled && subEnabled;
      }

      #endregion

      #region Form Control routines

      // Get the substitution rule data
      private void SubGet_Click(object sender, EventArgs e) {
         byte[] data;
         if (vCat >= 0) {
            if (EIP.StartSession()) {
               // Save the state on entry
               if (EIP.ForwardOpen()) {
                  for (int i = 0; i < subLabels[vCat].Length; i++) {
                     // Get the substitution all at once
                     data = EIP.ToBytes((i + startWith[vCat]), 1);
                     EIP.SetDataValue = data[0].ToString();
                     if (EIP.GetAttribute(ClassCode.Substitution_rules, (byte)at[vCat], data)) {
                        subTexts[vCat][i].Text = EIP.GetDataValue;
                     }
                  }
               }
               EIP.ForwardClose();
            }
            EIP.EndSession();
         }
         SetButtonEnables();
      }

      // Set the substitution rule data
      private void SubSet_Click(object sender, EventArgs e) {
         byte[] data;
         if (vCat >= 0) {
            if (EIP.StartSession()) {
               if (EIP.ForwardOpen()) {
                  Prop prop = EIP.AttrDict[ClassCode.Substitution_rules, (byte)at[vCat]].Set;
                  // The correct substitution rule is already set
                  for (int i = 0; i < subLabels[vCat].Length; i++) {
                     // Send the substitution data one at a time
                     data = EIP.FormatOutput(prop, i + startWith[vCat], 1, subTexts[vCat][i].Text);
                     if (!EIP.SetAttribute(ClassCode.Substitution_rules, (byte)at[vCat], data)) {
                        EIP.LogIt("Error writing substitution data!  Aborting");
                        break;
                     }
                  }
               }
               EIP.ForwardClose();
            }
            EIP.EndSession();
         }
         SetButtonEnables();
      }

      // Hide the old controls and make the new ones visible
      private void cbCategory_SelectedIndexChanged(object sender, EventArgs e) {
         // Hide the current set of controls
         if (vCat >= 0) {
            for (int i = 0; i < subLabels[vCat].Length; i++) {
               subLabels[vCat][i].Visible = false;
               subTexts[vCat][i].Visible = false;
            }
         }
         // Show the new set of controls
         vCat = cbAttribute.SelectedIndex;
         for (int i = 0; i < subLabels[vCat].Length; i++) {
            subLabels[vCat][i].Visible = true;
            subTexts[vCat][i].Visible = true;
         }
         resizeSubstitutions(ref R);
         SetButtonEnables();
      }

      // Make the group box more visible
      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         e.Graphics.DrawRectangle(new Pen(Color.CadetBlue, 2), 1, 1, gb.Width - 2, gb.Height - 2);
      }

      // Class attribute changed.  
      private void cbAttribute_Click(object sender, EventArgs e) {
         SetButtonEnables();
      }

      #endregion

      #region Service Routines

      // Select all text when text box is entered
      private void TextBox_Enter(object sender, EventArgs e) {
         TextBox tb = (TextBox)sender;
         parent.BeginInvoke((Action)delegate { tb.SelectAll(); });
      }

      // Called on resize or category change
      private void resizeSubstitutions(ref ResizeInfo R) {
         Utils.ResizeObject(ref R, lblAttribute, 1, 1, 1.5f, 4);
         Utils.ResizeObject(ref R, cbAttribute, 1, 5, 1.5f, 6);

         if (vCat >= 0 && resizeNeeded[vCat]) {
            resizeNeeded[vCat] = false;
            for (int i = 0; i < subLabels[vCat].Length; i++) {
               float r = 3.5f + 3 * (int)(i / 10);
               float c = (i % 10) * 3.25f + 0.25f;
               Utils.ResizeObject(ref R, subLabels[vCat][i], r, c, 1.5f, 1);
               Utils.ResizeObject(ref R, subTexts[vCat][i], r, c + 1, 1.5f, 2.25f);
            }
         }
         Utils.ResizeObject(ref R, subGet, 1, GroupWidth - 9, 1.5f, 3);
         Utils.ResizeObject(ref R, subSet, 1, GroupWidth - 5, 1.5f, 3);
      }

      #endregion

   }
}
