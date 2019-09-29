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
      ListBox lbGroupNames;
      Button cmdGetGroupNames;
      GroupBox grpMessages;
      ListBox lbMessagesNames;
      Button cmdGetMessageNames;

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
         {
            lbGroupNames = new ListBox() { ScrollAlwaysVisible = true };
            grpGroups.Controls.Add(lbGroupNames);

            cmdGetGroupNames = new Button() { Text = "Refresh" };
            grpGroups.Controls.Add(cmdGetGroupNames);
            cmdGetGroupNames.Click += cmdGetGroupNames_Click;
         }

         grpMessages = new GroupBox() { Text = "Group Messages" };
         tab.Controls.Add(grpMessages);
         grpMessages.Paint += GroupBorder_Paint;
         {
            lbMessagesNames = new ListBox() { ScrollAlwaysVisible = true };
            grpMessages.Controls.Add(lbMessagesNames);

            cmdGetMessageNames = new Button() { Text = "Refresh" };
            grpMessages.Controls.Add(cmdGetMessageNames);
            cmdGetMessageNames.Click += cmdGetMessageNames_Click;
         }
      }

      private void cmdGetGroupNames_Click(object sender, EventArgs e) {
         lbGroupNames.Items.Clear();
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               for (int i = 0; i < 100; i += 10) {
                  byte[] GroupNames;
                  bool success = EIP.GetAttribute(ccPDM.List_of_Groups, i, out GroupNames);
                  if (success) {
                     for (int j = 0; j < GroupNames.Length; j += 50) {
                        long n = EIP.Get(GroupNames, j, 2, mem.LittleEndian);
                        string Name = EIP.GetUTF8(GroupNames, j + 2, 48).Replace("\0", "");
                        if (Name.Length > 2) {
                           lbGroupNames.Items.Add($"{n:00} | {Name}");
                        }
                     }
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void cmdGetMessageNames_Click(object sender, EventArgs e) {

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
         float gap = 0.25f;
         float size = (GroupWidth - 3 * gap) / 2.0f;

         Utils.ResizeObject(ref R, grpGroups, GroupStart, gap, GroupHeight, size);
         {
            Utils.ResizeObject(ref R, lbGroupNames, 1, 1, GroupHeight - 4, size - 2, 1.5f);
            Utils.ResizeObject(ref R, cmdGetGroupNames, GroupHeight - 3, size - 5, 2, 4);
         }

         Utils.ResizeObject(ref R, grpMessages, GroupStart, size+gap * 2, GroupHeight, size);
         {
            Utils.ResizeObject(ref R, lbMessagesNames, 1, 1, GroupHeight - 4, size - 2, 1.5f);
            Utils.ResizeObject(ref R, cmdGetMessageNames, GroupHeight - 3, size - 5, 2, 4);
         }
      }

      #endregion

   }
}
