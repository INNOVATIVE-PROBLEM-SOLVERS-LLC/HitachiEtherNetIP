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
      HitachiBrowser parent;
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
      Label lblUpICS;
      ComboBox cbUpICS;
      Button UpGet;
      Button UpSet;

      Button UpClear;
      Button UpNew;
      Button UpBrowse;
      Button UpSaveAs;

      // Grid objects
      GroupBox grpGrid;
      PictureBox pbGrid;
      Bitmap bmGrid = null;
      HScrollBar hsbGrid;


      bool ignoreChange = true;

      //
      bool isValid = true;
      string fileName;
      string logoName;
      string font;
      int charHeight = 0;
      int charWidth = 0;
      int bytesPerCharacter;
      int pos;
      int ics;
      int count = 0;
      int registration;
      string[] pattern = null;
      long[,] stripes;

      int cellSize;
      int maxScrollColumn = 0;

      #endregion

      #region Constructors and destructors

      public UserPattern(HitachiBrowser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
      }

      #endregion

      #region Routines called from parent

      public void BuildUserPatternControls() {
         UpControls = new GroupBox() { Text = "User Pattern Rules" };
         tab.Controls.Add(UpControls);
         UpControls.Paint += GroupBorder_Paint;

         lblUpFont = new Label() { Text = "Font", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblUpFont);

         cbUpFont = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbUpFont);
         cbUpFont.SelectedIndexChanged += cbUpFont_SelectedIndexChanged;

         lblUpPosition = new Label() { Text = "Position", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblUpPosition);

         cbUpPosition = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbUpPosition);
         cbUpPosition.SelectedIndexChanged += cbUpPosition_SelectedIndexChanged;

         lblUpICS = new Label() { Text = "ICS", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblUpICS);

         cbUpICS = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbUpICS);
         cbUpICS.SelectedIndexChanged += cbUpICS_SelectedIndexChanged;

         lblUpCount = new Label() { Text = "Count", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblUpCount);

         cbUpCount = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbUpCount);
         cbUpPosition.SelectedIndexChanged += cbUpPosition_SelectedIndexChanged;

         UpGet = new Button() { Text = "Get" };
         UpControls.Controls.Add(UpGet);
         UpGet.Click += UpGet_Click;

         UpSet = new Button() { Text = "Set" };
         UpControls.Controls.Add(UpSet);
         UpSet.Click += UpSet_Click;

         UpClear = new Button() { Text = "Clear" };
         UpControls.Controls.Add(UpClear);
         UpClear.Click += UpClear_Click;

         UpNew = new Button() { Text = "New" };
         UpControls.Controls.Add(UpNew);
         UpNew.Click += UpNew_Click;

         UpBrowse = new Button() { Text = "Browse" };
         UpControls.Controls.Add(UpBrowse);
         UpBrowse.Click += UpBrowse_Click;

         UpSaveAs = new Button() { Text = "Save As" };
         UpControls.Controls.Add(UpSaveAs);
         UpSaveAs.Click += UpSaveAs_Click;

         grpGrid = new GroupBox() { Text = "Scaled Image Grid", BackColor = Color.LightBlue };
         UpControls.Controls.Add(grpGrid);
         grpGrid.Paint += GroupBorder_Paint;

         hsbGrid = new HScrollBar() { Visible = false };
         UpControls.Controls.Add(hsbGrid);
         hsbGrid.Scroll += HsbGrid_Scroll;

         pbGrid = new PictureBox();
         grpGrid.Controls.Add(pbGrid);
         pbGrid.MouseClick += pbGrid_MouseClick;

         ignoreChange = false;

         // Now fill in the controls
         for (int i = 0; i < Data.DropDowns[19].Length; i++) {
            cbUpFont.Items.Add(Data.DropDowns[19][i]);
         }
         for (int i = 0; i < 200; i++) {
            cbUpPosition.Items.Add(i.ToString());
            cbUpCount.Items.Add((i + 1).ToString());
         }
         for (int i = 0; i < 13; i++) {
            cbUpICS.Items.Add(i.ToString());
         }

         SetButtonEnables();

      }

      public void ResizeUserPatternControls(ref ResizeInfo R, int GroupStart, int GroupHeight, int GroupWidth) {
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
            Utils.ResizeObject(ref R, lblUpICS, 2, 13, 1.5f, 3);
            Utils.ResizeObject(ref R, cbUpICS, 2, 16, 1.5f, 3);
            Utils.ResizeObject(ref R, lblUpCount, 2, 19, 1.5f, 3);
            Utils.ResizeObject(ref R, cbUpCount, 2, 22, 1.5f, 3);
            Utils.ResizeObject(ref R, UpGet, 1.75f, GroupWidth - 9, 2, 3);
            Utils.ResizeObject(ref R, UpSet, 1.75f, GroupWidth - 5, 2, 3);

            Utils.ResizeObject(ref R, grpGrid, 4, 1, GroupHeight - 10, GroupWidth - 2);
            {
               BuildImage();
            }
            Utils.ResizeObject(ref R, hsbGrid, GroupHeight - 6, 1, 2, GroupWidth - 2);

            Utils.ResizeObject(ref R, UpNew, GroupHeight - 3, GroupWidth - 17, 2, 3);
            Utils.ResizeObject(ref R, UpClear, GroupHeight - 3, GroupWidth - 13, 2, 3);
            Utils.ResizeObject(ref R, UpBrowse, GroupHeight - 3, GroupWidth - 9, 2, 3);
            Utils.ResizeObject(ref R, UpSaveAs, GroupHeight - 3, GroupWidth - 5, 2, 3);
         }
      }

      public void SetButtonEnables() {
         bool UpEnabled = cbUpFont.SelectedIndex >= 0 && cbUpPosition.SelectedIndex >= 0 && cbUpCount.SelectedIndex >= 0;
         bool eipEnabled = parent.ComIsOn & EIP.SessionIsOpen;
         UpGet.Enabled = UpEnabled && eipEnabled;
         UpSet.Enabled = UpEnabled && eipEnabled;
         hsbGrid.Visible = pbGrid != null && pbGrid.Width > grpGrid.Width - 2 * (int)R.W;
         UpNew.Enabled = UpEnabled;
         UpClear.Enabled = stripes != null;
         UpSaveAs.Enabled = stripes != null;
      }

      #endregion

      #region Form control routines

      private void HsbGrid_Scroll(object sender, ScrollEventArgs e) {
         if (e.NewValue > maxScrollColumn) {
            pbGrid.Left = -(maxScrollColumn * cellSize) + cellSize / 2;
         } else {
            pbGrid.Left = (int)R.W - e.NewValue * cellSize;
         }
         SetButtonEnables();
      }

      private void UpSaveAs_Click(object sender, EventArgs e) {
         string filename = "Logo" + String.Format("{0:yyyyMMddHHmm}", DateTime.Now);
         using (SaveFileDialog sfd = new SaveFileDialog()) {
            sfd.DefaultExt = "txt";
            sfd.Filter = "Text|*.txt";
            sfd.Title = "Save Printer Image to Text file";
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;
            sfd.FileName = filename;
            if (sfd.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty(sfd.FileName)) {
               // Rewrite the Image Directory
               TextWriter tw = new StreamWriter(fileName);
               tw.WriteLine($"{cbUpFont.Text},{0},{count},{registration}");
               string[] Pattern = StripesToPattern(charHeight, stripes);
               for (int i = 0; i < pattern.Length; i++) {
                  tw.WriteLine(pattern[i]);
               }
               tw.Flush();
               tw.Close();
            }
         }
         SetButtonEnables();
      }

      private void UpBrowse_Click(object sender, EventArgs e) {

         using (OpenFileDialog dlg = new OpenFileDialog()) {
            dlg.AutoUpgradeEnabled = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.Title = "Select Printer Logo file";
            dlg.Filter = "Printer Logo Files|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK) {
               CleanUpGrid();
               if (ReadLogoFromFile(dlg.FileName, out stripes)) {
                  bmGrid = BuildBitMap(stripes);
                  BuildImage();
               }
               SetButtonEnables();
            }
         }
      }

      private void cbUpPosition_SelectedIndexChanged(object sender, EventArgs e) {
         if(!ignoreChange) {

         }
         SetButtonEnables();
      }

      private void cbUpFont_SelectedIndexChanged(object sender, EventArgs e) {
         if (!ignoreChange) {

         }
         SetButtonEnables();
      }

      private void UpSet_Click(object sender, EventArgs e) {
         byte[,] b = StripesToBytes(charHeight, stripes);
         bool OpenCloseForward = !EIP.ForwardIsOpen;
         if (OpenCloseForward) {
            EIP.ForwardOpen();
         }
         // <TODO> == Send It Out
         if (OpenCloseForward && EIP.ForwardIsOpen) {
            EIP.ForwardClose();
         }
         SetButtonEnables();
      }

      private void UpGet_Click(object sender, EventArgs e) {
         CleanUpGrid();
         GetFontInfo(cbUpFont.Text, out charHeight, out charWidth, out pos, out bytesPerCharacter);
         // Build the blank image
         stripes = new long[cbUpCount.SelectedIndex + 1, charWidth];
         bmGrid = BuildBitMap(stripes);
         BuildImage();

         bool OpenCloseForward = !EIP.ForwardIsOpen;
         if (OpenCloseForward) {
            EIP.ForwardOpen();
         }
         for (int i = 0; i < cbUpCount.SelectedIndex + 1; i++) {
            byte[] data = new byte[] { (byte)(cbUpFont.SelectedIndex + 1), (byte)(cbUpPosition.SelectedIndex + i) };
            AttrData attr = Data.AttrDict[eipClassCode.User_pattern, (byte)eipUser_pattern.User_Pattern_Fixed];
            bool Success = EIP.ReadOneAttribute(eipClassCode.User_pattern, (byte)eipUser_pattern.User_Pattern_Fixed, attr, data, out string val);
            if(Success) {
               if(EIP.GetDataLength == bytesPerCharacter) {
                  BytesToStripes(i, charHeight, EIP.GetData);
                  bmGrid = BuildBitMap(stripes);
                  BuildImage();
                  grpGrid.Invalidate();
               } else {
                  break;
               }
            }
         }
         if (OpenCloseForward && EIP.ForwardIsOpen) {
            EIP.ForwardClose();
         }
         // Build the real image
         bmGrid = BuildBitMap(stripes);
         BuildImage();
         SetButtonEnables();
      }

      private void BytesToStripes(int n, int charHeight, byte[] getData) {
         int stride = (charHeight + 7) / 8;
         long val = 0;
         for (int i = 0; i < getData.Length; i += stride) {
            for (int j = 0; j < stride; j++) {
               val += getData[i + j] << (8 * j);
            }
            stripes[n, i / stride] = val;
         }
      }

      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         using (Pen p = new Pen(Color.CadetBlue, 2)) {
            e.Graphics.DrawRectangle(p, 1, 1, gb.Width - 2, gb.Height - 2);
         }
      }

      private void cbUpICS_SelectedIndexChanged(object sender, EventArgs e) {
         if (!ignoreChange) {

         }
         SetButtonEnables();
      }

      private void pbGrid_MouseClick(object sender, MouseEventArgs e) {
         Rectangle rect;

         if (pbGrid.Image != null) {
            int x = Math.Max(0, Math.Min(e.Location.X / cellSize * cellSize, pbGrid.Image.Width - 1));
            int y = Math.Max(0, Math.Min(e.Location.Y / cellSize * cellSize, pbGrid.Image.Height - 1));
            int row = y / cellSize;
            int col = x / cellSize;

            rect = new Rectangle(x, y, cellSize, cellSize);
            using (Graphics g = Graphics.FromImage(pbGrid.Image)) {
               // Flip the color
               if (Color.Black.ToArgb() == bmGrid.GetPixel(col, row).ToArgb()) {
                  // if black, write white
                  g.FillRectangle(Brushes.White, rect);
                  bmGrid.SetPixel(col, row, Color.White);
               } else {
                  // not black, write black
                  g.FillRectangle(Brushes.Black, rect);
                  bmGrid.SetPixel(col, row, Color.Black);
               }
               Pen pen = new Pen(Color.CadetBlue, 1);
               // Redraw adjacent vertical lines
               for (int i = col; i <= col + 2; i++) {
                  if ((i % charWidth) == 0) {
                     pen.Color = Color.Red;
                  } else {
                     pen.Color = Color.CadetBlue;
                  }
                  g.DrawLine(pen, i * cellSize, 0, i * cellSize, pbGrid.Image.Height);
               }

               // Redraw adjacent horizontal lines
               for (int i = row; i <= row + 2; i++) {
                  if (i == 0 || i == charHeight) {
                     pen.Color = Color.Black;
                  } else {
                     pen.Color = Color.CadetBlue;
                  }
                  g.DrawLine(pen, 0, i * cellSize, pbGrid.Image.Width, i * cellSize);
               }

            }
            pbGrid.Invalidate(rect);
         }
      }

      private void UpNew_Click(object sender, EventArgs e) {
         CleanUpGrid();
         GetFontInfo(cbUpFont.Text, out charHeight, out charWidth, out pos, out bytesPerCharacter);
         stripes = new long[cbUpCount.SelectedIndex + 1, charWidth];
         bmGrid = BuildBitMap(stripes);
         BuildImage();
         SetButtonEnables();
      }

      private void UpClear_Click(object sender, EventArgs e) {
         for (int i = 0; i < stripes.GetLength(0); i++) {
            for (int j = 0; j < stripes.GetLength(1); j++) {
               stripes[i, j] = 0;
            }
         }
         bmGrid = BuildBitMap(stripes);
         BuildImage();
         SetButtonEnables();
      }

      #endregion

      #region Service Routines

      private bool ReadLogoFromFile(string fullFileName, out long[,] stripes) {
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
                     isValid = GetFontInfo(font, out charHeight, out charWidth, out pos, out bytesPerCharacter) &&
                        int.TryParse(header[1], out ics) && int.TryParse(header[2], out count);
                     if (isValid && header.Length > 3 && (isValid = int.TryParse(header[3], out registration))) {
                     } else {
                        registration = -1;
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
                     isValid = sr.EndOfStream;
                  }
                  sr.Close();
                  stripes = PatternToStripes(charHeight, charWidth, pattern);
               }
            } else {
               isValid = false;
            }
         } catch (Exception e) {
            isValid = false;
         }
         if(isValid) {
            ignoreChange = true;
            cbUpFont.SelectedIndex = pos;
            cbUpPosition.SelectedIndex = registration;
            cbUpICS.SelectedIndex = ics;
            cbUpCount.SelectedIndex = pattern.Length - 1;
            ignoreChange = false;
         }
         return isValid;
      }

      private long[,] PatternToStripes(int charHeight, int charWidth, string[] pattern) {
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

      private string[] StripesToPattern(int charHeight, long[,] stripes) {
         string[] result = new string[stripes.GetLength(0)];
         int stride = (charHeight + 7) / 8;
         for (int i = 0; i < stripes.GetLength(0); i++) {
            string s = string.Empty;
            for (int j = 0; j < stripes.GetLength(1); j++) {
               long n = stripes[i, j];
               for (int k = 0; k < stride; k++)  {
                  s += (n & 0xFF).ToString("X2");
                  n >>= 8;
               }
            }
            result[i] = s;
         }
         return result;
      }

      private byte[,] StripesToBytes(int charHeight, long[,] stripes) {
         int stride = (charHeight + 7) / 8;
         byte[,] result = new byte[stripes.GetLength(0), stripes.GetLength(1) * stride];
         for (int i = 0; i < stripes.GetLength(0); i++) {
            int b = 0;
            for (int j = 0; j < stripes.GetLength(1); j++) {
               long n = stripes[i, j];
               for (int k = 0; k < stride; k++) {
                  result[i, b++] = (byte)n;
                  n >>= 8;
               }
            }
         }
         return result;
      }

      private bool GetFontInfo(string font, out int charHeight, out int charWidth, out int pos, out int bytesPerCharacter) {
         bool IsValid = true;
         switch (font.Replace('X', 'x')) {
            case "0":
            case "4x5":
               charHeight = 5;
               charWidth = 8;
               bytesPerCharacter = 8;
               pos = 0;
               break;
            case "1":
            case "5x5":
               charHeight = 5;
               charWidth = 8;
               bytesPerCharacter = 8;
               pos = 1;
               break;
            case "2":
            case "5x8":
            case "5x7":
            case "5x8(5x7)":
               charHeight = 8;
               charWidth = 8;
               bytesPerCharacter = 8;
               pos = 2;
               break;
            case "3":
            case "9x8":
            case "9x7":
            case "9x8(9x7)":
               charHeight = 8;
               charWidth = 16;
               bytesPerCharacter = 16;
               pos = 3;
               break;
            case "4":
            case "7x10":
               charHeight = 10;
               charWidth = 8;
               bytesPerCharacter = 16;
               pos = 4;
               break;
            case "5":
            case "10x12":
               charHeight = 12;
               charWidth = 16;
               bytesPerCharacter = 32;
               pos = 5;
               break;
            case "6":
            case "12x16":
               charHeight = 16;
               charWidth = 16;
               bytesPerCharacter = 32;
               pos = 6;
               break;
            case "7":
            case "18x24":
               charHeight = 24;
               charWidth = 24;
               bytesPerCharacter = 72;
               pos = 7;
               break;
            case "8":
            case "24x32":
               charHeight = 32;
               charWidth = 32;
               bytesPerCharacter = 128;
               pos = 8;
               break;
            case "9":
            case "11x11":
               charHeight = 11;
               charWidth = 16;
               bytesPerCharacter = 32;
               pos = 9;
               break;
            case "10":
            case "5x3(Chimney)":
               charHeight = 3;
               charWidth = 5;
               pos = 10;
               bytesPerCharacter = 5;
               break;
            case "11":
            case "5x5(Chimney)":
               charHeight = 5;
               charWidth = 5;
               pos = 11;
               bytesPerCharacter = 5;
               break;
            case "12":
            case "7x5(Chimney)":
               charHeight = 5;
               charWidth = 7;
               bytesPerCharacter = 7;
               pos = 12;
               break;
            case "13":
            case "30x40":
               charHeight = 40;
               charWidth = 40;
               bytesPerCharacter = 200;
               pos = 13;
               break;
            case "14":
            case "36x48":
               charHeight = 48;
               charWidth = 48;
               bytesPerCharacter = 288;
               pos = 14;
               break;
            default:
               charHeight = 0;
               charWidth = 0;
               bytesPerCharacter = 0;
               pos = -1;
               IsValid = false;
               break;
         }
         return IsValid;
      }

      private Bitmap BuildBitMap(long[,] stripes) {
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

      private void BuildImage() {
         if (stripes != null) {
            if (pbGrid.Image != null) {
               pbGrid.Image.Dispose();
               pbGrid.Image = null;
            }
            cellSize = Math.Min((int)((GroupHeight - 13) * R.H / charHeight), 10);
            pbGrid.Image = new Bitmap(bmGrid.Width * cellSize + 1, bmGrid.Height * cellSize + 1);
            pbGrid.Location = new Point((int)(1 * R.W), (int)(2 * R.H));
            pbGrid.Size = pbGrid.Image.Size;

            // Initialize Scroll Bar
            maxScrollColumn = ((pbGrid.Width - grpGrid.Size.Width) + (2 * (int)R.W)) / cellSize;
            hsbGrid.Visible = maxScrollColumn > 0;
            hsbGrid.Minimum = 0;
            hsbGrid.SmallChange = 1;
            hsbGrid.Maximum = charWidth * stripes.GetLength(0);
            hsbGrid.LargeChange = Math.Max(hsbGrid.Maximum - maxScrollColumn, cellSize);
            hsbGrid.Value = 0;

            using (Graphics g = Graphics.FromImage(pbGrid.Image)) {
               SolidBrush pb = new SolidBrush(Color.Black);
               g.Clear(Color.White);

               // Fill in the grid
               for (int x = 0; x < bmGrid.Width; x++) {
                  for (int y = 0; y < bmGrid.Height; y++) {
                     pb.Color = bmGrid.GetPixel(x, y);
                     g.FillRectangle(pb, x * cellSize, y * cellSize, cellSize, cellSize);
                  }
               }

               using (Pen pen = new Pen(Color.CadetBlue, 1)) {
                  // vertical lines
                  for (int x = 0; x <= charWidth * stripes.GetLength(0); x++) {
                     if ((x % charWidth) == 0) {
                        pen.Color = Color.Red;
                     } else {
                        pen.Color = Color.CadetBlue;
                     }
                     g.DrawLine(pen, x * cellSize, 0, x * cellSize, pbGrid.Image.Height);
                  }

                  // horizontal lines
                  for (int y = 0; y <= charHeight; y++) {
                     if (y == 0 || y == charHeight) {
                        pen.Color = Color.Black;
                     } else {
                        pen.Color = Color.CadetBlue;
                     }
                     g.DrawLine(pen, 0, y * cellSize, pbGrid.Image.Width, y * cellSize);
                  }
               }
            }
            pbGrid.Invalidate();
         }
      }

      private void CleanUpGrid() {
         // Clean up the old display
         if (stripes != null) {
            stripes = null;
            if (pbGrid.Image != null) {
               pbGrid.Image.Dispose();
               pbGrid.Image = null;
            }
            if (bmGrid != null) {
               bmGrid.Dispose();
               bmGrid = null;
            }
         }
      }

      #endregion

   }
}
