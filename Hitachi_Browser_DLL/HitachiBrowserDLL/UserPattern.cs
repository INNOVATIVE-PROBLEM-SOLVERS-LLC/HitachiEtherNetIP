using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EIP_Lib {

   #region User Pattern processing

   class UserPattern {

      #region Data Declarations

      ResizeInfo R;

      Browser parent;
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

      int blackPixel = Color.Black.ToArgb();
      int whitePixel = Color.White.ToArgb();

      bool ignoreChange = true;

      //
      string font;
      int dotMatrixCode;
      int charHeight = 0;
      int charWidth = 0;
      int ics;
      int maxICS;
      int bytesPerCharacter;
      int count = 0;
      int registration;
      long[][] stripes;

      int cellSize;
      int maxScrollColumn = 0;

      #endregion

      #region Constructors and destructors

      // Just tuck away the calling parameters
      public UserPattern(Browser parent, EIP EIP, TabPage tab) {
         this.parent = parent;
         this.EIP = EIP;
         this.tab = tab;
      }

      #endregion

      #region Routines called from parent

      // Dynamically build all the controls.
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
         cbUpCount.SelectedIndexChanged += cbUpCount_SelectedIndexChanged;

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
         pbGrid.MouseMove += pbGrid_MouseMove;
         pbGrid.MouseUp += pbGrid_MouseUp;

         ignoreChange = false;

         // Now fill in the controls
         for (int i = 0; i < EIP.DropDowns[(int)fmtDD.UserPatternFont].Length; i++) {
            cbUpFont.Items.Add(EIP.DropDowns[(int)fmtDD.UserPatternFont][i]);
         }
         for (int i = 0; i < 200; i++) {
            cbUpPosition.Items.Add(i.ToString());
            cbUpCount.Items.Add((i + 1).ToString());
         }
         SetButtonEnables();

      }

      // Refit the controls based on the new space requirem,ents
      public void ResizeUserPatternControls(ref ResizeInfo R, int GroupStart, int GroupHeight, int GroupWidth) {
         this.R = R;

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
               BitMapToImage();
            }
            Utils.ResizeObject(ref R, hsbGrid, GroupHeight - 6, 1, 2, GroupWidth - 2);

            Utils.ResizeObject(ref R, UpNew, GroupHeight - 3, GroupWidth - 17, 2, 3);
            Utils.ResizeObject(ref R, UpClear, GroupHeight - 3, GroupWidth - 13, 2, 3);
            Utils.ResizeObject(ref R, UpBrowse, GroupHeight - 3, GroupWidth - 9, 2, 3);
            Utils.ResizeObject(ref R, UpSaveAs, GroupHeight - 3, GroupWidth - 5, 2, 3);
         }
      }

      // Enable buttons only when they can be used
      public void SetButtonEnables() {
         bool UpEnabled = cbUpFont.SelectedIndex >= 0 && cbUpPosition.SelectedIndex >= 0 && cbUpCount.SelectedIndex >= 0;
         bool eipEnabled = parent.ComIsOn;
         UpGet.Enabled = UpEnabled && eipEnabled;
         UpSet.Enabled = UpEnabled && eipEnabled;
         hsbGrid.Visible = pbGrid != null && pbGrid.Width > grpGrid.Width - 2 * (int)R.W;
         UpNew.Enabled = cbUpFont.SelectedIndex >= 0 && cbUpCount.SelectedIndex >= 0;
         UpClear.Enabled = stripes != null;
         UpSaveAs.Enabled = stripes != null;
      }

      #endregion

      #region Form control routines

      // Allow all the image to be viewed
      private void HsbGrid_Scroll(object sender, ScrollEventArgs e) {
         pbGrid.Left = (int)R.W - e.NewValue * cellSize;
      }

      // Save the bitmap in cijConnect format
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
               TextWriter tw = new StreamWriter(sfd.FileName);
               // Load from controls
               tw.WriteLine($"{cbUpFont.Text},{0},{count},{registration}");
               // Need Bitmap not stripes
               string[] pattern = StripesToPattern(charHeight, BitMapToStripes(bmGrid, charWidth));
               for (int i = 0; i < pattern.Length; i++) {
                  tw.WriteLine(pattern[i]);
               }
               tw.Flush();
               tw.Close();
            }
         }
         SetButtonEnables();
      }

      // Load a file that is in cijConnect format
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
               ReadLogoFromFile(dlg.FileName, out stripes);
            }
         }
         SetButtonEnables();
      }

      // Send characters to the printer
      private void UpSet_Click(object sender, EventArgs e) {
         GetFontInfo(cbUpFont.Text, out charHeight, out charWidth, out maxICS, out dotMatrixCode, out bytesPerCharacter);
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               byte[][] b = StripesToBytes(charHeight, BitMapToStripes(bmGrid, charWidth));
               int pos = cbUpPosition.SelectedIndex;
               int count = cbUpCount.SelectedIndex + 1;
               for (int i = 0; i < count; i++) {
                  // <TODO> Need Format Output routine
                  byte[] data = EIP.Merge(EIP.ToBytes(dotMatrixCode, 1), EIP.ToBytes((pos + i), 1), b[i]);
                  if (!EIP.SetAttribute(ClassCode.User_pattern, (byte)ccUP.User_Pattern_Fixed, data)) {
                     EIP.LogIt("User Pattern Download Failed.  Aborting Doenload!");
                     break;
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         SetButtonEnables();
      }

      // Get characters from the printer
      private void UpGet_Click(object sender, EventArgs e) {
         CleanUpGrid();
         GetFontInfo(cbUpFont.Text, out charHeight, out charWidth, out maxICS, out dotMatrixCode, out bytesPerCharacter);
         // Build the blank image
         stripes = new long[cbUpCount.SelectedIndex + 1][];
         for (int i = 0; i < stripes.Length; i++) {
            stripes[i] = new long[charWidth];
         }
         bmGrid = StripesToBitMap(stripes);
         BitMapToImage();

         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               for (int i = 0; i <= cbUpCount.SelectedIndex; i++) {
                  byte[] data = new byte[] { (byte)(dotMatrixCode ), (byte)(cbUpPosition.SelectedIndex + i) };
                  bool Success = EIP.GetAttribute(ClassCode.User_pattern, (byte)ccUP.User_Pattern_Fixed, data);
                  if (Success) {
                     if (EIP.GetDataLength == bytesPerCharacter) {
                        stripes[i] = BytesToStripe(charHeight, EIP.GetData);
                        bmGrid = StripesToBitMap(stripes);
                        BitMapToImage();
                        grpGrid.Invalidate();
                     }
                  } else {
                     EIP.LogIt("User Pattern Upload Failed.  Aborting Doenload!");
                     break;
                  }
               }
            }
            EIP.ForwardClose();
         }
         EIP.EndSession();
         // Build the real image
         bmGrid = StripesToBitMap(stripes);
         BitMapToImage();
         SetButtonEnables();
      }

      // Build a stripe from a single character
      private long[] BytesToStripe(int charHeight, byte[] b) {
         int stride = (charHeight + 7) / 8;
         int count = b.Length / stride;
         long[] stripe = new long[count];
         for (int i = 0; i < b.Length; i += stride) {
            long val = 0;
            for (int j = 0; j < stride; j++) {
               val += b[i + j] << (8 * j);
            }
            stripe[i / stride] = val;
         }
         return stripe;
      }

      // Make the group box be more visible
      private void GroupBorder_Paint(object sender, PaintEventArgs e) {
         GroupBox gb = (GroupBox)sender;
         using (Pen p = new Pen(Color.CadetBlue, 2)) {
            e.Graphics.DrawRectangle(p, 1, 1, gb.Width - 2, gb.Height - 2);
         }
      }

      // Invert one pixel
      private void pbGrid_MouseClick(object sender, MouseEventArgs e) {
         Rectangle rect;
         if (pbGrid.Image != null) {
            // x and y are relative to the image
            int x = Math.Max(0, Math.Min(e.Location.X / cellSize * cellSize, pbGrid.Image.Width - 1));
            int y = Math.Max(0, Math.Min(e.Location.Y / cellSize * cellSize, pbGrid.Image.Height - 1));
            // row and column are relative to the bitmap
            int row = y / cellSize;
            int col = x / cellSize;

            rect = new Rectangle(x, y, cellSize, cellSize);
            using (Graphics g = Graphics.FromImage(pbGrid.Image)) {
               // Flip the color
               if (bmGrid.GetPixel(col, row).ToArgb() == blackPixel) {
                  // if black, write white
                  g.FillRectangle(Brushes.White, rect);
                  bmGrid.SetPixel(col, row, Color.White);
               } else {
                  // not black, write black
                  g.FillRectangle(Brushes.Black, rect);
                  bmGrid.SetPixel(col, row, Color.Black);
               }
            }
            pbGrid.Invalidate(rect);
         }
      }

      internal void pbGrid_MouseMove(object sender, MouseEventArgs e) {
         GetFontInfo(cbUpFont.Text, out charHeight, out charWidth, out maxICS, out dotMatrixCode, out bytesPerCharacter);
         int columns = (charWidth + maxICS) * cbUpCount.SelectedIndex + 1;
         int row;
         int col;
         int x;
         int y;
         Rectangle rect;

         if (pbGrid.Image != null && e.Button == MouseButtons.Left &&
                 ((Control.ModifierKeys & (Keys.Shift | Keys.Control)) != 0)) {
            using (Graphics g = Graphics.FromImage(pbGrid.Image)) {

               // in range 0 to columns - 1
               col = Math.Max(0, Math.Min(e.Location.X / (pbGrid.Width / columns), columns - 1));
               // in range 0 to rows - 1
               row = Math.Max(0, Math.Min(e.Location.Y / (pbGrid.Height / charHeight), charHeight - 1));

               x = col * cellSize;
               y = row * cellSize;
               rect = new Rectangle(x, y, cellSize, cellSize);

               if ((Control.ModifierKeys & Keys.Control) != 0) {
                  // Erase the rectangle
                  g.FillRectangle(Brushes.White, rect);
                  bmGrid.SetPixel(col, row, Color.White);
               } else {
                  // Fill the rectangle
                  g.FillRectangle(Brushes.Black, rect);
                  bmGrid.SetPixel(col, row, Color.Black);
               }
            }
            pbGrid.Invalidate();
         }
      }

      internal void pbGrid_MouseUp(object sender, MouseEventArgs e) {
         using (Graphics g = Graphics.FromImage(pbGrid.Image)) {
            // Redraw adjacent vertical lines to the left and right
            Pen pen = new Pen(Color.CadetBlue, 1);
            for (int i = 0; i <= pbGrid.Width / cellSize; i++) {
               pen.Color = (i % charWidth) == 0 ? Color.Red : Color.CadetBlue;
               g.DrawLine(pen, i * cellSize, 0, i * cellSize, pbGrid.Image.Height);
            }

            // Redraw adjacent horizontal lines above and below
            pen.Color = Color.CadetBlue;
            for (int i = 0; i <= pbGrid.Height / cellSize; i++) {
               g.DrawLine(pen, 0, i * cellSize, pbGrid.Image.Width, i * cellSize);
            }
         }
         pbGrid.Invalidate();
      }

      // Create an empty grid
      private void UpNew_Click(object sender, EventArgs e) {
         CleanUpGrid();
         GetFontInfo(cbUpFont.Text, out charHeight, out charWidth, out maxICS, out dotMatrixCode, out bytesPerCharacter);
         stripes = new long[cbUpCount.SelectedIndex + 1][];
         for (int i = 0; i < stripes.Length; i++) {
            stripes[i] = new long[charWidth];
         }
         bmGrid = StripesToBitMap(stripes);
         BitMapToImage();
         SetButtonEnables();
      }

      // Clear the stripes and rebuild the bitmap and image
      private void UpClear_Click(object sender, EventArgs e) {
         for (int i = 0; i < stripes.Length; i++) {
            for (int j = 0; j < stripes[i].Length; j++) {
               stripes[i][j] = 0;
            }
         }
         bmGrid = StripesToBitMap(stripes);
         BitMapToImage();
         SetButtonEnables();
      }

      // The selected font changed, get info about new font.
      private void cbUpFont_SelectedIndexChanged(object sender, EventArgs e) {
         if (!ignoreChange) {
            CleanUpGrid();
            GetFontInfo(cbUpFont.Text, out charHeight, out charWidth, out maxICS, out dotMatrixCode, out bytesPerCharacter);
         }
         cbUpICS.Items.Clear();
         for (int i = 0; i <= maxICS; i++) {
            cbUpICS.Items.Add(i.ToString());
         }
         SetButtonEnables();
      }

      // The location in the printer changed
      private void cbUpPosition_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // The inter-character space changed
      private void cbUpICS_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      // The number of characters changed
      private void cbUpCount_SelectedIndexChanged(object sender, EventArgs e) {
         if (!ignoreChange) {
            CleanUpGrid();
         }
         SetButtonEnables();
      }

      #endregion

      #region Service Routines

      // convert cijConnect logo file to stripes
      private void ReadLogoFromFile(string fullFileName, out long[][] stripes) {
         bool isValid = true;
         string[] pattern = null;
         stripes = null;
         try {
            if (File.Exists(fullFileName)) {
               using (StreamReader sr = new StreamReader(fullFileName)) {
                  // Process the header
                  string[] header = sr.ReadLine().Split(',');
                  if (header.Length > 2) {
                     font = header[0];
                     isValid = GetFontInfo(font, out charHeight, out charWidth, out maxICS, out dotMatrixCode, out bytesPerCharacter) &&
                        int.TryParse(header[1], out ics) && int.TryParse(header[2], out count);
                     if (isValid && header.Length > 3) {
                        if (!int.TryParse(header[3], out registration)) {
                           registration = -1;
                           isValid = false;
                        }
                     } else {
                        registration = -1;
                     }
                  }
                  // Retrieve the pattern
                  if (isValid) {
                     pattern = new string[count];
                     for (int i = 0; i < count && !sr.EndOfStream; i++) {
                        pattern[i] = sr.ReadLine();
                     }
                     isValid = sr.EndOfStream && pattern[count - 1] != null;
                  }
                  sr.Close();
               }
            } else {
               isValid = false;
            }
         } catch {
            isValid = false;
         }
         if (isValid) {
            // Load the cijConnect logo into the printer browser
            CleanUpGrid();
            stripes = PatternToStripes(charHeight, charWidth, pattern);
            bmGrid = StripesToBitMap(stripes);
            BitMapToImage();
            // Set the dropdowns to reflect the loaded image
            ignoreChange = true;
            cbUpFont.SelectedIndex = dotMatrixCode - 1;
            cbUpPosition.SelectedIndex = registration;
            cbUpICS.SelectedIndex = ics;
            cbUpCount.SelectedIndex = pattern.Length - 1;
            ignoreChange = false;
         }
      }

      // Convert cijConnect formatted file to stripes
      private long[][] PatternToStripes(int charHeight, int charWidth, string[] pattern) {
         long[][] result = null;
         int stride = ((charHeight + 7) / 8) * 2;
         result = new long[pattern.Length][];
         for (int i = 0; i < pattern.Length; i++) {
            result[i] = new long[charWidth];
            int l = pattern[i].Length / stride;
            for (int j = 0; j < l; j++) {
               long n = 0;
               string s = pattern[i].Substring(j * stride, stride);
               for (int k = 0; k < s.Length / 2; k++) {
                  n += (Convert.ToInt64(s.Substring(k * 2, 2), 16) << k * 8);
               }
               result[i][j] = n;
            }
         }
         return result;
      }

      // Convert stripes back to a cijConnect formatted pattern
      private string[] StripesToPattern(int charHeight, long[][] stripes) {
         string[] result = new string[stripes.Length];
         int stride = (charHeight + 7) / 8;
         for (int i = 0; i < stripes.Length; i++) {
            string s = string.Empty;
            for (int j = 0; j < stripes[i].Length; j++) {
               long n = stripes[i][j];
               for (int k = 0; k < stride; k++) {
                  s += (n & 0xFF).ToString("X2");
                  n >>= 8;
               }
            }
            result[i] = s;
         }
         return result;
      }

      // Convert stripes to a byte array for sending to the printer
      private byte[][] StripesToBytes(int charHeight, long[][] stripes) {
         int stride = (charHeight + 7) / 8;
         byte[][] result = new byte[stripes.Length][];
         for (int i = 0; i < stripes.Length; i++) {
            result[i] = new byte[stripes[i].Length * stride];
            for (int j = 0; j < stripes[i].Length; j++) {
               long n = stripes[i][j];
               for (int k = 0; k < stride; k++) {
                  result[i][j * stride + k] = (byte)n;
                  n >>= 8;
               }
            }
         }
         return result;
      }

      // get information about the selected font
      private bool GetFontInfo(string font, out int charHeight, out int charWidth, out int maxICS, out int dotmatrixCode, out int bytesPerCharacter) {
         bool IsValid = true;
         switch (font.Replace('X', 'x')) {
            case "1":
            case "4x5":
               charHeight = 5;
               charWidth = 8;
               maxICS = 4;
               bytesPerCharacter = 8;
               dotmatrixCode = 1;
               break;
            case "2":
            case "5x5":
               charHeight = 5;
               charWidth = 8;
               maxICS = 3;
               bytesPerCharacter = 8;
               dotmatrixCode = 2;
               break;
            case "3":
            case "5x8":
            case "5x7":
            case "5x8(5x7)":
               charHeight = 8;
               charWidth = 8;
               maxICS = 3;
               bytesPerCharacter = 8;
               dotmatrixCode = 3;
               break;
            case "4":
            case "9x8":
            case "9x7":
            case "9x8(9x7)":
               charHeight = 8;
               charWidth = 16;
               maxICS = 7;
               bytesPerCharacter = 16;
               dotmatrixCode = 4;
               break;
            case "5":
            case "7x10":
               charHeight = 10;
               charWidth = 8;
               maxICS = 1;
               bytesPerCharacter = 16;
               dotmatrixCode = 5;
               break;
            case "6":
            case "10x12":
               charHeight = 12;
               charWidth = 16;
               maxICS = 6;
               bytesPerCharacter = 32;
               dotmatrixCode = 6;
               break;
            case "7":
            case "12x16":
               charHeight = 16;
               charWidth = 16;
               maxICS = 4;
               bytesPerCharacter = 32;
               dotmatrixCode = 7;
               break;
            case "8":
            case "18x24":
               charHeight = 24;
               charWidth = 24;
               maxICS = 6;
               bytesPerCharacter = 72;
               dotmatrixCode = 8;
               break;
            case "9":
            case "24x32":
               charHeight = 32;
               charWidth = 32;
               maxICS = 12;
               bytesPerCharacter = 128;
               dotmatrixCode = 9;
               break;
            case "0x3A":
            case "11x11":
               charHeight = 11;
               charWidth = 16;
               maxICS = 5;
               bytesPerCharacter = 32;
               dotmatrixCode = 10;
               break;
            case "0x3B":
            case "5x3(Chimney)":
               charHeight = 3;
               charWidth = 5;
               maxICS = 0;
               bytesPerCharacter = 5;
               dotmatrixCode = 11;
               break;
            case "0x3C":
            case "5x5(Chimney)":
               charHeight = 5;
               charWidth = 5;
               maxICS = 0;
               bytesPerCharacter = 5;
               dotmatrixCode = 12;
               break;
            case "0x3D":
            case "7x5(Chimney)":
               charHeight = 5;
               charWidth = 7;
               maxICS = 0;
               bytesPerCharacter = 7;
               dotmatrixCode = 13;
               break;
            case "0x3E":
            case "30x40":
               charHeight = 40;
               charWidth = 40;
               maxICS = 10;
               bytesPerCharacter = 200;
               dotmatrixCode = 14;
               break;
            case "0x3F":
            case "36x48":
               charHeight = 48;
               charWidth = 48;
               maxICS = 12;
               bytesPerCharacter = 288;
               dotmatrixCode = 15;
               break;
            default:
               charHeight = -1;
               charWidth = -1;
               maxICS = -1;
               bytesPerCharacter = -1;
               dotmatrixCode = -1;
               IsValid = false;
               break;
         }
         return IsValid;
      }

      // Convert stripes to a bitmap 
      private Bitmap StripesToBitMap(long[][] stripes) {
         Bitmap result = new Bitmap(stripes.Length * charWidth, charHeight);
         using (Graphics g = Graphics.FromImage(result)) {
            g.Clear(Color.White);
            for (int i = 0; i < stripes.Length; i++) {
               for (int j = 0; j < stripes[i].Length; j++) {
                  long b = 1;
                  for (int k = charHeight - 1; k >= 0; k--) {
                     if ((stripes[i][j] & b) > 0) {
                        result.SetPixel(i * charWidth + j, k, Color.Black);
                     }
                     b <<= 1;
                  }
               }
            }
         }
         return result;
      }

      // Convert bitmap to stripes
      private long[][] BitMapToStripes(Bitmap b, int charWidth) {
         int nChars = b.Width / charWidth;
         long[][] s = new long[nChars][];
         int x = 0;
         for (int c = 0; c < nChars; c++) {
            s[c] = new long[charWidth];
            for (int i = 0; i < charWidth; i++) {
               long stripe = 0;
               int bit = 1;
               for (int y = b.Height - 1; y >= 0; y--) {
                  if (b.GetPixel(x, y).ToArgb() == blackPixel) {
                     stripe += bit;
                  }
                  bit <<= 1;
               }
               x += 1;
               s[c][i] = stripe;
            }
         }
         return s;
      }

      // Convert bitmap to an image
      private void BitMapToImage() {
         if (bmGrid != null) {
            // Dispose of old image
            if (pbGrid.Image != null) {
               pbGrid.Image.Dispose();
               pbGrid.Image = null;
            }
            // Allow for 3 extra spaces in the group box (2 above and 1 below)
            cellSize = Math.Min((int)((grpGrid.Height - 3 * R.H) / charHeight), 10);
            // Add an extra pixel below and to the right for drading lines
            pbGrid.Image = new Bitmap(bmGrid.Width * cellSize + 1, bmGrid.Height * cellSize + 1);
            // Place the image at Row 2, Column 1 in the group box
            pbGrid.Location = new Point((int)(1 * R.W), (int)(2 * R.H));
            // Make the image and the grid the same size
            pbGrid.Size = pbGrid.Image.Size;

            // Initialize Scroll Bar
            maxScrollColumn = ((pbGrid.Width - grpGrid.Size.Width) + (2 * (int)R.W)) / cellSize;
            hsbGrid.Visible = maxScrollColumn > 0;
            hsbGrid.Minimum = 0;
            hsbGrid.SmallChange = 1;
            hsbGrid.Maximum = bmGrid.Width;
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
                  for (int x = 0; x <= bmGrid.Width; x++) {
                     pen.Color = (x % charWidth) == 0 ? Color.Red : Color.CadetBlue;
                     g.DrawLine(pen, x * cellSize, 0, x * cellSize, pbGrid.Image.Height);
                  }

                  // horizontal lines
                  pen.Color = Color.CadetBlue;
                  for (int y = 0; y <= charHeight; y++) {
                     g.DrawLine(pen, 0, y * cellSize, pbGrid.Image.Width, y * cellSize);
                  }
               }

            }
            pbGrid.Invalidate();
            pbGrid.Visible = true;
         } else {
            pbGrid.Visible = false;
         }
      }

      // Clear out stripes, bitmap, and image
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

   #endregion

}

