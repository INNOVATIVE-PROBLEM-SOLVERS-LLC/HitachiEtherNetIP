using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HitachiEIP {
   class UserPattern {

      #region Data Declarations

      ResizeInfo R;
      int GroupStart;
      int GroupHeight;
      int GroupWidth;
      EIP EIP;
      TabPage tab;

      // User Pattern Specific Controls
      GroupBox UpControls;
      Label lblUpFont;
      ComboBox cbUpFont;
      Label lblUpPosition;
      ComboBox cbUpPosition;
      Label lblUpCount;
      ComboBox cbUpCount;
      Button UpGet;
      Button UpSet;

      Button UpBrowse;
      Button UpSaveAs;

      // Grid objects
      GroupBox grpGrid;
      PictureBox pbGrid;
      HScrollBar hsbGrid;

      //
      bool isValid = true;
      string fileName;
      string logoName;
      string font;
      int charHeight = 0;
      int charWidth = 0;
      int pos;
      int ics;
      int count = 0;
      int registration;
      string[] pattern = null;
      long[,] stripes;


      #endregion

      #region Constructors and destructors

      public UserPattern(EIP EIP, TabPage tab) {
         this.EIP = EIP;
         this.tab = tab;
      }

      #endregion

      #region Tab Specific Routines (User Pattern)

      public void BuildUserPatternControls() {
         UpControls = new GroupBox() { Text = "User Pattern Rules" };
         tab.Controls.Add(UpControls);
         UpControls.Paint += GroupBorder_Paint;

         lblUpFont = new Label() { Text = "Font", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblUpFont);

         cbUpFont = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbUpFont);
         cbUpFont.SelectedIndexChanged += cbUpFont_SelectedIndexChanged;
         for (int i = 0; i < Data.DropDowns[19].Length; i++) {
            cbUpFont.Items.Add(Data.DropDowns[19][i]);
         }

         lblUpPosition = new Label() { Text = "Position", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblUpPosition);

         cbUpPosition = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbUpPosition);
         cbUpPosition.SelectedIndexChanged += cbUpPosition_SelectedIndexChanged;
         for (int i = 0; i < 200; i++) {
            cbUpPosition.Items.Add(i.ToString("D3"));
         }

         lblUpCount = new Label() { Text = "Count", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblUpCount);

         cbUpCount = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbUpCount);
         cbUpPosition.SelectedIndexChanged += cbUpPosition_SelectedIndexChanged;
         for (int i = 1; i < 10; i++) {
            cbUpCount.Items.Add(i.ToString("D0"));
         }
         cbUpCount.SelectedIndex = 0;

         UpGet = new Button() { Text = "Get" };
         UpControls.Controls.Add(UpGet);
         UpGet.Click += UpGet_Click;

         UpSet = new Button() { Text = "Set" };
         UpControls.Controls.Add(UpSet);
         UpSet.Click += UpSet_Click;

         UpBrowse = new Button() { Text = "Browse" };
         UpControls.Controls.Add(UpBrowse);
         UpBrowse.Click += UpBrowse_Click;

         UpSaveAs = new Button() { Text = "Save As" };
         UpControls.Controls.Add(UpSaveAs);
         UpSaveAs.Click += UpSaveAs_Click;

         grpGrid = new GroupBox() { Text = "Scaled Image Grid", BackColor = Color.LightBlue };
         UpControls.Controls.Add(grpGrid);
         grpGrid.Paint += GroupBorder_Paint;

         hsbGrid = new HScrollBar() { };
         UpControls.Controls.Add(hsbGrid);

         pbGrid = new PictureBox();
         grpGrid.Controls.Add(pbGrid);

      }

      private void UpSaveAs_Click(object sender, EventArgs e) {

      }

      private void UpBrowse_Click(object sender, EventArgs e) {
         string fileName = String.Empty;

         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.Title = "Select Printer Logo file";
            dlg.Filter = "Printer Logo Files|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK) {
               fileName = dlg.FileName;
               stripes = null;
               if (ReadLogoFromFile(fileName, out stripes)) {
                  //pbGrid.Image = LoadLogo(stripes);
                  //pbGrid.Refresh();
                  ResizeUserPatternnControls(ref R, GroupStart, GroupHeight, GroupWidth);
               }
               SetUpButtonEnables();
            }
         }
      }

      private Bitmap LoadLogo(long[,] stripes) {
         Bitmap result = new Bitmap(stripes.GetLength(0) * stripes.GetLength(1), charHeight);
         using (Graphics g = Graphics.FromImage(result)) {
            for (int i = 0; i < stripes.GetLength(0); i++) {
               for (int j = 0; j < stripes.GetLength(1); j++) {
                  long b = 1;
                  for (int k = charHeight - 1; k >= 0; k--) {
                     if ((stripes[i, j] & b) > 0) {
                        result.SetPixel(i * charWidth + j, k, Color.Black);
                     } else {
                        result.SetPixel(i * charWidth + j, k, Color.White);
                     }
                     b <<= 1;
                  }
               }
            }
         }
         return result;
      }

      private void cbUpPosition_SelectedIndexChanged(object sender, EventArgs e) {
         SetUpButtonEnables();
      }

      private void cbUpFont_SelectedIndexChanged(object sender, EventArgs e) {
         SetUpButtonEnables();
      }

      private void UpSet_Click(object sender, EventArgs e) {

      }

      private void UpGet_Click(object sender, EventArgs e) {
         bool OpenCloseForward = !EIP.ForwardIsOpen;
         if (OpenCloseForward) {
            EIP.ForwardOpen();
         }
         for (int i = 0; i < cbUpCount.SelectedIndex + 1; i++) {
            byte[] data = new byte[] { (byte)(cbUpFont.SelectedIndex + 1), (byte)(cbUpPosition.SelectedIndex + i) };
            AttrData attr = Data.AttrDict[eipClassCode.User_pattern, (byte)eipUser_pattern.User_Pattern_Fixed];
            bool Success = EIP.ReadOneAttribute(eipClassCode.User_pattern, (byte)eipUser_pattern.User_Pattern_Fixed, attr, data, out string val);
         }
         if (OpenCloseForward && EIP.ForwardIsOpen) {
            EIP.ForwardClose();
         }
      }

      public void ResizeUserPatternnControls(ref ResizeInfo R, int GroupStart, int GroupHeight, int GroupWidth) {
         this.R = R;
         this.GroupStart = GroupStart;
         this.GroupHeight = GroupHeight;
         this.GroupWidth = GroupWidth;

         Utils.ResizeObject(ref R, UpControls, GroupStart + 0.75f, 0.5f, GroupHeight, GroupWidth - 0.5f);
         {
            Utils.ResizeObject(ref R, lblUpFont, 2, 1, 1.5f, 3);
            Utils.ResizeObject(ref R, cbUpFont, 2, 4, 1.5f, 3);
            Utils.ResizeObject(ref R, lblUpPosition, 2, 7, 1.5f, 3);
            Utils.ResizeObject(ref R, cbUpPosition, 2, 10, 1.5f, 3);
            Utils.ResizeObject(ref R, lblUpCount, 2, 13, 1.5f, 3);
            Utils.ResizeObject(ref R, cbUpCount, 2, 16, 1.5f, 3);
            Utils.ResizeObject(ref R, UpGet, 1.75f, GroupWidth - 9, 2, 3);
            Utils.ResizeObject(ref R, UpSet, 1.75f, GroupWidth - 5, 2, 3);

            Utils.ResizeObject(ref R, grpGrid, 4, 1, GroupHeight - 10, GroupWidth - 2);
            {
               if (stripes != null) {
                  if (pbGrid.Image != null) {
                     pbGrid.Image.Dispose();
                     pbGrid.Image = null;
                  }
                  int cellSize = Math.Min((int)((GroupHeight - 13) * R.H / charHeight), 6);
                  pbGrid.Image = new Bitmap(stripes.GetLength(0) * stripes.GetLength(1) * cellSize, charHeight * cellSize);
                  pbGrid.Location = new Point((int)(1 * R.W), (int)(2 * R.H));
                  pbGrid.Size = pbGrid.Image.Size;
                  using (Graphics g = Graphics.FromImage(pbGrid.Image)) {
                     SolidBrush pb = new SolidBrush(Color.Black);
                     g.Clear(Color.White);

                     // Fill in the grid
                     float xOffset = 0;
                     for (int i = 0; i < stripes.GetLength(0); i++) {
                        for (int j = 0; j < stripes.GetLength(1); j++) {
                           long n = stripes[i, j];
                           int b = 1;
                           float yOffset = cellSize * charHeight;
                           for (int k = 0; k < charHeight; k++) {
                              yOffset -= cellSize;
                              if ((n & b) > 0) {
                                 g.FillRectangle(pb, xOffset, yOffset, cellSize, cellSize);
                              }
                              b <<= 1;
                           }
                           xOffset += cellSize;
                        }
                     }
                  }
                  pbGrid.Invalidate();
               }
            }
            Utils.ResizeObject(ref R, hsbGrid, GroupHeight - 6, 1, 2, GroupWidth - 2);

            Utils.ResizeObject(ref R, UpBrowse, GroupHeight - 3, GroupWidth - 9, 2, 3);
            Utils.ResizeObject(ref R, UpSaveAs, GroupHeight - 3, GroupWidth - 5, 2, 3);
         }
      }

      internal bool ReadLogoFromFile(string fullFileName, out long[,] stripes) {
         stripes = null;
         try {
            isValid = true;

            if (File.Exists(fullFileName)) {
               fileName = fullFileName;
               logoName = Path.GetFileNameWithoutExtension(fileName);
               using (StreamReader sr = new StreamReader(fileName)) {
                  string[] header = sr.ReadLine().Split(',');
                  if (header.Length > 2) {
                     font = header[0];
                     if (GetFontInfo(font, out charHeight, out charWidth, out pos)) {
                     } else {
                        isValid = false;
                     }
                     if (!int.TryParse(header[1], out ics) || !int.TryParse(header[2], out count)) {
                        isValid = false;
                     } else {
                        if (header.Length > 3) {
                           if (!int.TryParse(header[3], out registration)) {
                              isValid = false;
                           }
                        } else {
                           registration = -1;
                        }
                     }
                  }
                  if (isValid) {
                     pattern = new string[count];
                     for (int i = 0; i < count; i++) {
                        if (sr.EndOfStream) {
                           isValid = false;
                        } else {
                           pattern[i] = sr.ReadLine();
                        }
                     }
                     if (!sr.EndOfStream) {
                        isValid = false;
                     }
                  }
                  sr.Close();
                  stripes = BuildStripes(charHeight, charWidth, pattern);
               }
            } else {
               isValid = false;
            }
         } catch (Exception e) {
            isValid = false;
         }
         return isValid;
      }

      private long[,] BuildStripes(int charHeight, int charWidth, string[] pattern) {
         long[,] result = null;
         int stride = ((charHeight + 7) / 8) * 2;
         result = new long[pattern.Length, charWidth];
         for (int i = 0; i < pattern.Length; i++) {
            int l = pattern[i].Length / stride;
            for (int j = 0; j < l; j++) {
               long n = 0;
               string s = pattern[i].Substring(j * stride, stride);
               for (int k = 0; k < s.Length / 2; k++) {
                  n += (Convert.ToInt64(s.Substring(k * 2, 2), 16) << k * 8);
               }
               result[i, j] = n;
            }
         }
         return result;
      }

      private bool GetFontInfo(string font, out int charHeight, out int charWidth, out int pos) {
         bool IsValid = true;
         switch (font.Replace('X', 'x')) {
            case "0":
            case "4x5":
               charHeight = 5;
               charWidth = 8;
               pos = 0;
               break;
            case "1":
            case "5x5":
               charHeight = 5;
               charWidth = 8;
               pos = 1;
               break;
            case "2":
            case "5x8":
            case "5x7":
            case "5x8(5x7)":
               charHeight = 8;
               charWidth = 8;
               pos = 2;
               break;
            case "3":
            case "9x8":
            case "9x7":
            case "9x8(9x7)":
               charHeight = 8;
               charWidth = 16;
               pos = 3;
               break;
            case "4":
            case "7x10":
               charHeight = 10;
               charWidth = 8;
               pos = 4;
               break;
            case "5":
            case "10x12":
               charHeight = 12;
               charWidth = 16;
               pos = 5;
               break;
            case "6":
            case "12x16":
               charHeight = 16;
               charWidth = 16;
               pos = 6;
               break;
            case "7":
            case "18x24":
               charHeight = 24;
               charWidth = 24;
               pos = 7;
               break;
            case "8":
            case "24x32":
               charHeight = 32;
               charWidth = 32;
               pos = 8;
               break;
            case "9":
            case "11x11":
               charHeight = 11;
               charWidth = 16;
               pos = 9;
               break;
            case "10":
            case "5x3(Chimney)":
               charHeight = 3;
               charWidth = 5;
               pos = 10;
               break;
            case "11":
            case "5x5(Chimney)":
               charHeight = 5;
               charWidth = 5;
               pos = 11;
               break;
            case "12":
            case "7x5(Chimney)":
               charHeight = 5;
               charWidth = 7;
               pos = 12;
               break;
            case "13":
            case "30x40":
               charHeight = 40;
               charWidth = 40;
               pos = 13;
               break;
            case "14":
            case "36x48":
               charHeight = 48;
               charWidth = 48;
               pos = 14;
               break;
            default:
               charHeight = 0;
               charWidth = 0;
               pos = -1;
               IsValid = false;
               break;
         }
         return IsValid;
      }

      public void SetUpButtonEnables() {
         bool UpEnabled = cbUpFont.SelectedIndex >= 0 && cbUpPosition.SelectedIndex >= 0;
         UpGet.Enabled = UpEnabled;
         UpSet.Enabled = UpEnabled;
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
