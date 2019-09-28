using System;
using System.Drawing;
using System.Windows.Forms;

namespace EIP_Lib {
   class Errors {

      #region Data Declarations

      ResizeInfo R;
      Browser parent;
      EIP EIP;
      TabPage tab;

      GroupBox grpErrors;
      ListBox lbErrors;
      Button cmdGetErrors;
      Button cmdClearErrors;
      Label lblErrorCount;
      TextBox txtErrorCount;

      #endregion

      #region Constructors and destructors

      public Errors(Browser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
      }

      #endregion

      #region Routines called from parent

      // Build all controls unique to this class
      public void BuildControls() {
         grpErrors = new GroupBox() { Text = "Printer Errors" };
         tab.Controls.Add(grpErrors);
         grpErrors.Paint += GroupBorder_Paint;

         lbErrors = new ListBox() { ScrollAlwaysVisible = true };
         grpErrors.Controls.Add(lbErrors);

         lblErrorCount = new Label() { Text = "Count", TextAlign = ContentAlignment.BottomCenter };
         grpErrors.Controls.Add(lblErrorCount);

         txtErrorCount = new TextBox() { TextAlign = HorizontalAlignment.Center, ReadOnly = true };
         grpErrors.Controls.Add(txtErrorCount);

         cmdGetErrors = new Button() { Text = "Get Errors" };
         grpErrors.Controls.Add(cmdGetErrors);
         cmdGetErrors.Click += CmdGetErrors_Click;

         cmdClearErrors = new Button() { Text = "Clear Errors" };
         grpErrors.Controls.Add(cmdClearErrors);
         cmdClearErrors.Click += CmdClearErrors_Click;

      }
      #endregion

      #region Form Control routines

      private void CmdGetErrors_Click(object sender, EventArgs e) {
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               EIP.GetAttribute(ClassCode.IJP_operation, (byte)ccIJP.Fault_and_warning_history, EIP.Nodata);
               byte[] d = EIP.GetData;
               lbErrors.Items.Clear();
               long count = 0;
               if (d.Length > 3) {
                  count = EIP.Get(d, 0, 4, mem.LittleEndian);
                  for (int i = 0; i < Math.Min(count, (d.Length - 4) / 10); i++) {
                     int n = i * 10 + 4;
                     long year = EIP.Get(d, n, 2, mem.LittleEndian);
                     lbErrors.Items.Add($"{i + 1:00} | {year:0000}/{d[n + 2]:00}/{d[n + 3]:00} {d[n + 4]:00}:{d[n + 5]:00}:{d[n + 6]:00} | {d[n + 7]:00} | {d[n + 8]:00} | {d[n + 9]:00}");
                  }
               }
               txtErrorCount.Text = count.ToString();
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
      }

      private void CmdClearErrors_Click(object sender, EventArgs e) {

      }

      #endregion

      #region Service Routines

      // Make the group box more visible
      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         using (Pen p = new Pen(Color.CadetBlue, 2)) {
            e.Graphics.DrawRectangle(p, 1, 1, gb.Width - 2, gb.Height - 2);
         }
      }

      public void ResizeControls(ref ResizeInfo R, float GroupStart, float GroupHeight, int GroupWidth) {
         int tclHeight = (int)(tab.ClientSize.Height / R.H);
         this.R = R;

         Utils.ResizeObject(ref R, grpErrors, GroupStart, 1, GroupHeight, GroupWidth - 1);
         {
            Utils.ResizeObject(ref R, lbErrors, 1, 1, GroupHeight - 2, GroupWidth - 7, 1.5f);
            Utils.ResizeObject(ref R, lblErrorCount, GroupHeight - 9, GroupWidth - 5, 2, 3.5f);
            Utils.ResizeObject(ref R, txtErrorCount, GroupHeight - 9, GroupWidth - 5, 2, 3.5f);
            Utils.ResizeObject(ref R, cmdGetErrors, GroupHeight - 7, GroupWidth - 5, 3, 3.5f);
            Utils.ResizeObject(ref R, cmdClearErrors, GroupHeight - 3.5f, GroupWidth - 5, 3, 3.5f);
         }
      }

      #endregion

   }
}
