using System;
using System.Drawing;
using System.Windows.Forms;

namespace EIP_Lib {

   class PrintManagement {

      #region Data Declarations

      ResizeInfo R;

      Browser parent;
      EIP EIP;
      TabPage tab;

      GroupBox grpGroups;
      GroupBox grpMessages;

      #endregion

      #region Constructors and destructors

      #endregion

      #region Constructors and destructors

      public PrintManagement(Browser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
      }

      #endregion

      #region Routines called from parent

      public void BuildControls() {
         grpGroups = new GroupBox() { Text = "Message Groups" };
         tab.Controls.Add(grpGroups);
         grpGroups.Paint += GroupBorder_Paint;

         grpMessages = new GroupBox() { Text = "Group Messages" };
         tab.Controls.Add(grpMessages);
         grpMessages.Paint += GroupBorder_Paint;

      }

      #endregion

      #region Form Control routines

      #endregion

      #region Service Routines

      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         using (Pen p = new Pen(Color.CadetBlue, 2)) {
            e.Graphics.DrawRectangle(p, 1, 1, gb.Width - 2, gb.Height - 2);
         }
      }

      public void ResizeControls(ref ResizeInfo R, float GroupStart, float GroupHeight, int GroupWidth) {
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         this.R = R;

         Utils.ResizeObject(ref R, grpGroups, GroupStart, 1, GroupHeight, GroupWidth / 2 - 1);
         {

         }

         Utils.ResizeObject(ref R, grpMessages, GroupStart, GroupWidth / 2 + 1, GroupHeight, GroupWidth / 2 - 1);
         {

         }
      }

      #endregion

   }
}
