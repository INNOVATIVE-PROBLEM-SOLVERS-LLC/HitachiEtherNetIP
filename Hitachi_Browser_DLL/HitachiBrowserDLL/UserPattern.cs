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
      Label lblLayout;         // Fixed or Free
      ComboBox cbLayout;
      Label lblFontRows;       // Font for Fixed, Rows for Free (1 to 32)
      ComboBox cbFontRows;
      Label lblIcsCols;        // ICS for Fixed, Cols for Free (1 to 320)
      ComboBox cbIcsCols;
      Label lblUpCount;        // Character count for Fixed, "1" for Free
      ComboBox cbUpCount;
      Label lblUpPosition;     // 0 to 199 for Fixed, 0 to 49 for Free
      ComboBox cbUpPosition;
      Button UpGet;            // Retrieve from printer
      Button UpSet;            // Send to printer

      Button UpClear;          // Clear the Grid
      Button UpNew;            // Create a new grid
      Button UpBrowse;         // Find Logo File to load
      Button UpSaveAs;         // Save the grid to a Logo File

      // Grid objects
      GroupBox grpGrid;        // Group box to hold the grid
      PictureBox pbGrid;       // The grid
      Bitmap bmGrid = null;    // Bitmap for loading the grid
      HScrollBar hsbGrid;      // Horizontal Scroll Bar if the grid is large

      int blackPixel = Color.Black.ToArgb(); // Pixel colors
      int whitePixel = Color.White.ToArgb();

      bool ignoreChange = true; // Ignore change if setup is in progress

      // Parameters returned by GetFontInfo
      string font;
      int dotMatrixCode;
      int charHeight = 0;
      int charWidth = 0;
      int maxICS;

      // Hide the controls as much as possinle
      public int Count {
         get {
            return cbUpCount.SelectedIndex + 1;
         }
         set {
            if (value > 0 && value <= cbUpCount.Items.Count) {
               cbUpCount.SelectedIndex = value - 1;
            } else {
               cbUpCount.SelectedIndex = -1;
            }
         }
      }
      public int Registration {
         get {
            return cbUpPosition.SelectedIndex;
         }
         set {
            if (value >= 0 && value < cbUpPosition.Items.Count) {
               cbUpPosition.SelectedIndex = value;
            } else {
               cbUpPosition.SelectedIndex = -1;
            }
         }
      }

      int ics;
      long[][] stripes;

      int cellSize;
      int maxScrollColumn = 0;

      enum Layout {
         Fixed = 0,
         Free = 1,
      }


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
      public void BuildControls() {
         UpControls = new GroupBox() { Text = "User Pattern Rules" };
         tab.Controls.Add(UpControls);
         UpControls.Paint += GroupBorder_Paint;

         lblLayout = new Label() { Text = "Layout", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblLayout);

         cbLayout = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbLayout);
         cbLayout.SelectedIndexChanged += cbLayout_SelectedIndexChanged;

         lblFontRows = new Label() { Text = "Font", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblFontRows);

         cbFontRows = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbFontRows);
         cbFontRows.SelectedIndexChanged += cbUpFont_SelectedIndexChanged;

         lblUpPosition = new Label() { Text = "Position", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblUpPosition);

         cbUpPosition = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbUpPosition);
         cbUpPosition.SelectedIndexChanged += cbUpPosition_SelectedIndexChanged;

         lblIcsCols = new Label() { Text = "ICS", TextAlign = ContentAlignment.TopRight };
         UpControls.Controls.Add(lblIcsCols);

         cbIcsCols = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
         UpControls.Controls.Add(cbIcsCols);
         cbIcsCols.SelectedIndexChanged += cbUpICS_SelectedIndexChanged;

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

         cbLayout_SelectedIndexChanged(null, null);

         SetButtonEnables();

      }

      // Refit the controls based on the new space requirem,ents
      public void ResizeControls(ref ResizeInfo R, float GroupStart, float GroupHeight, int GroupWidth) {
         this.R = R;

         Utils.ResizeObject(ref R, UpControls, GroupStart + 0.75f, 0.5f, GroupHeight, GroupWidth - 0.5f);
         {
            Utils.ResizeObject(ref R, lblLayout, 2, 1, 1.5f, 2);
            Utils.ResizeObject(ref R, cbLayout, 2, 3, 1.5f, 4);

            Utils.ResizeObject(ref R, lblFontRows, 2, 7, 1.5f, 2);
            Utils.ResizeObject(ref R, cbFontRows, 2, 9, 1.5f, 3);

            Utils.ResizeObject(ref R, lblIcsCols, 2, 12, 1.5f, 2);
            Utils.ResizeObject(ref R, cbIcsCols, 2, 14, 1.5f, 3);

            Utils.ResizeObject(ref R, lblUpCount, 2, 17, 1.5f, 2);
            Utils.ResizeObject(ref R, cbUpCount, 2, 19, 1.5f, 3);

            Utils.ResizeObject(ref R, lblUpPosition, 2, 22, 1.5f, 2);
            Utils.ResizeObject(ref R, cbUpPosition, 2, 24, 1.5f, 3);

            Utils.ResizeObject(ref R, UpGet, 1.75f, 27.5f, 2, 3);
            Utils.ResizeObject(ref R, UpSet, 1.75f, 31, 2, 3);

            Utils.ResizeObject(ref R, grpGrid, 4, 1, GroupHeight - 8, GroupWidth - 2);
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
         bool UpEnabled = cbFontRows.SelectedIndex >= 0 && Registration >= 0 && Count > 0;
         bool eipEnabled = parent.ComIsOn;
         UpGet.Enabled = UpEnabled && eipEnabled;
         UpSet.Enabled = UpEnabled && eipEnabled;
         hsbGrid.Visible = pbGrid != null && pbGrid.Width > grpGrid.Width - 2 * (int)R.W;
         UpNew.Enabled = cbFontRows.SelectedIndex >= 0 && Count > 0;
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
         string filename = $"Logo{DateTime.Now:yyyyMMddHHmm}";
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
               if ((Layout)cbLayout.SelectedIndex == Layout.Fixed) {
                  tw.WriteLine($"{cbFontRows.Text},{0},{Count},{Registration}");
               } else {
                  tw.WriteLine($"Free,{cbFontRows.SelectedIndex + 1},{cbIcsCols.SelectedIndex + 1},{Registration}");
               }
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
         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               byte[][] b = StripesToBytes(charHeight, BitMapToStripes(bmGrid, charWidth));
               byte[] data;
               for (int i = 0; i < Count; i++) {
                  if ((Layout)cbLayout.SelectedIndex == Layout.Fixed) {
                     // Font ID, Position, and Bit Map
                     data = EIP.Merge(EIP.ToBytes(dotMatrixCode, 1), EIP.ToBytes(Registration + i, 1), b[i]);
                     if (EIP.SetAttribute(ClassCode.User_pattern, (byte)ccUP.User_Pattern_Fixed, data)) {
                        // Should return nothing.  Need to see what we get to know how to process it
                     } else {
                        EIP.LogIt("Fixed Pattern Download Failed.  Aborting Download!");
                        break;
                     }
                  } else {
                     // Vertical Size (charHeigth), Horizontol Size (charWidth), Position, Bit Map
                     data = EIP.Merge(EIP.ToBytes(charHeight, 1), EIP.ToBytes(charWidth, 2), EIP.ToBytes(Registration + i, 1), b[i]);
                     if (EIP.SetAttribute(ClassCode.User_pattern, (byte)ccUP.User_Pattern_Free, data)) {
                        // Should return nothing.  Need to see what we get to know how to process it
                     } else {
                        EIP.LogIt("Free Pattern Download Failed.  Aborting Download!");
                        break;
                     }
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
         bool Success;
         int bytesPerCharacter = (charHeight + 7) / 8 * charWidth;

         CleanUpGrid();
         // Build the blank image
         stripes = new long[Count][];
         for (int i = 0; i < stripes.Length; i++) {
            stripes[i] = new long[charWidth];
         }
         bmGrid = StripesToBitMap(stripes);
         BitMapToImage();

         if (EIP.StartSession()) {
            if (EIP.ForwardOpen()) {
               byte[] data;
               for (int i = 0; i <= Count; i++) {
                  if ((Layout)cbLayout.SelectedIndex == Layout.Fixed) {
                     data = new byte[] { (byte)(dotMatrixCode), (byte)(Registration + i) };
                     Success = EIP.GetAttribute(ClassCode.User_pattern, (byte)ccUP.User_Pattern_Fixed, data);
                  } else {
                     data = EIP.Merge(EIP.ToBytes(charHeight, 1), EIP.ToBytes(charWidth, 2), EIP.ToBytes(Registration + i, 1));
                     Success = EIP.GetAttribute(ClassCode.User_pattern, (byte)ccUP.User_Pattern_Free, data);
                  }
                  if (Success) {
                     if (EIP.GetDataLength == bytesPerCharacter + (cbLayout.SelectedIndex == (int)Layout.Fixed ? 2 : 4)) {
                        stripes[i] = BytesToStripe(charHeight, EIP.GetData);
                        bmGrid = StripesToBitMap(stripes);
                        BitMapToImage();
                        grpGrid.Invalidate();
                     }
                  } else {
                     EIP.LogIt("User Pattern Upload Failed.  Aborting Upload!");
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

      // Build the stripes from a single character
      private long[] BytesToStripe(int charHeight, byte[] b) {
         int stride = (charHeight + 7) / 8;
         int count = b.Length / stride;
         long[] stripe = new long[count];
         for (int i = 0; i < b.Length; i += stride) {
            stripe[i / stride] = EIP.Get(b, i, stride, mem.BigEndian);
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

      // Use MouseDown+shift to set multiple pixels, MouseDown+ctrl to clear multiple pixels
      internal void pbGrid_MouseMove(object sender, MouseEventArgs e) {
         int columns = charWidth * Count;
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

      // Redraw the horizontal and vertical grid lines
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
         stripes = new long[Count][];
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

      // Switch between Fixed and Free Logo layout
      private void cbLayout_SelectedIndexChanged(object sender, EventArgs e) {
         // If first call, initialize it.  Setting SelectedIndex will cause another call
         if (cbLayout.Items.Count == 0) {
            cbLayout.Items.AddRange(new string[] { "Fixed Pattern", "Free Layout" });
            cbLayout.SelectedIndex = 0;
            return;
         }
         CleanUpGrid();
         ignoreChange = true; // Ignore changes during transition
         switch ((Layout)cbLayout.SelectedIndex) {
            case Layout.Fixed:
               lblFontRows.Text = "Font";
               string[] dd = EIP.DropDowns[(int)fmtDD.UserPatternFont];
               cbFontRows.Items.Clear();
               cbFontRows.Items.AddRange(dd);

               cbIcsCols.Items.Clear();
               lblIcsCols.Text = "ICS";

               cbUpCount.Items.Clear();
               cbUpPosition.Items.Clear();
               for (int i = 0; i < 200; i++) {
                  cbUpCount.Items.Add((i + 1).ToString());
                  cbUpPosition.Items.Add(i.ToString());
               }
               break;
            case Layout.Free:
               lblFontRows.Text = "Rows";
               cbFontRows.Items.Clear();
               for (int i = 0; i < 32; i++) {
                  cbFontRows.Items.Add((i + 1).ToString());
               }

               lblIcsCols.Text = "Columns";
               cbIcsCols.Items.Clear();
               for (int i = 0; i < 320; i++) {
                  cbIcsCols.Items.Add((i + 1).ToString());
               }

               cbUpCount.Items.Clear();
               cbUpCount.Items.AddRange(new string[] { "1" });

               cbUpPosition.Items.Clear();
               for (int i = 0; i < 50; i++) {
                  cbUpPosition.Items.Add(i.ToString());
               }
               break;
         }
         ignoreChange = false;
         SetButtonEnables();
      }

      // The Font/Rows changed
      private void cbUpFont_SelectedIndexChanged(object sender, EventArgs e) {
         if (ignoreChange) {
            return;
         }
         CleanUpGrid();
         GetFontInfo(cbFontRows.Text, out charHeight, out charWidth, out maxICS, out dotMatrixCode);
         switch ((Layout)cbLayout.SelectedIndex) {
            case Layout.Fixed:
               cbIcsCols.Items.Clear();
               for (int i = 0; i <= maxICS; i++) {
                  cbIcsCols.Items.Add(i.ToString());
               }
               break;
            case Layout.Free:

               break;
         }
         SetButtonEnables();
      }

      // The ICS/Cols changed
      private void cbUpICS_SelectedIndexChanged(object sender, EventArgs e) {
         if (ignoreChange) {
            return;
         }
         switch ((Layout)cbLayout.SelectedIndex) {
            case Layout.Fixed:

               break;
            case Layout.Free:
               CleanUpGrid();
               GetFontInfo(cbFontRows.Text, out charHeight, out charWidth, out maxICS, out dotMatrixCode);
               break;
         }
         SetButtonEnables();
      }

      // The number of characters changed
      private void cbUpCount_SelectedIndexChanged(object sender, EventArgs e) {
         if (ignoreChange) {
            return;
         }
         CleanUpGrid();
         SetButtonEnables();
      }

      // The location in the printer changed
      private void cbUpPosition_SelectedIndexChanged(object sender, EventArgs e) {
         SetButtonEnables();
      }

      #endregion

      #region Service Routines

      // convert cijConnect logo file to stripes
      private void ReadLogoFromFile(string fullFileName, out long[][] stripes) {
         bool isValid = true;
         int count = 0;
         int reg = 0;
         string[] pattern = null;
         stripes = null;
         try {
            if (File.Exists(fullFileName)) {
               using (StreamReader sr = new StreamReader(fullFileName)) {
                  // Process the header
                  string[] header = sr.ReadLine().Split(',');
                  if (header.Length > 2) {
                     if (header[0].ToUpper() == "FREE") {
                        cbLayout.SelectedIndex = (int)Layout.Free; // "FREE, Rows, Columns, Registration
                        isValid = int.TryParse(header[1], out charHeight) && int.TryParse(header[2], out charWidth);
                        count = 1;
                        ics = 0;
                        maxICS = 0;
                        dotMatrixCode = -1;
                     } else {
                        cbLayout.SelectedIndex = (int)Layout.Fixed; // Font, ICS, Count, Registration
                        font = header[0];
                        isValid = GetFontInfo(font, out charHeight, out charWidth, out maxICS, out dotMatrixCode) &&
                           int.TryParse(header[1], out ics) && int.TryParse(header[2], out count);
                     }
                     if (isValid && header.Length > 3) {
                        if (!int.TryParse(header[3], out reg)) {
                           reg = -1;
                           isValid = false;
                        }
                     } else {
                        reg = -1;
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
            // Set the dropdowns to reflect the loaded image
            ignoreChange = true;
            Count = count;
            Registration = reg;
            switch ((Layout)cbLayout.SelectedIndex) {
               case Layout.Fixed:
                  ignoreChange = false; // Need to set ICS dropdown
                  cbFontRows.SelectedIndex = dotMatrixCode - 1;
                  cbIcsCols.SelectedIndex = ics;
                  break;
               case Layout.Free:
                  cbFontRows.SelectedIndex = charHeight - 1;
                  cbIcsCols.SelectedIndex = charWidth - 1;
                  ignoreChange = false;
                  break;
               default:
                  break;
            }
            // Load the cijConnect logo into the printer browser
            CleanUpGrid();
            stripes = PatternToStripes(charHeight, charWidth, pattern);
            bmGrid = StripesToBitMap(stripes);
            BitMapToImage();
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
      private bool GetFontInfo(string font, out int charHeight, out int charWidth, out int maxICS, out int dotmatrixCode) {
         bool IsValid = true;
         if ((Layout)cbLayout.SelectedIndex == Layout.Fixed) {
            switch (font.Replace('X', 'x')) {
               case "4x5":
                  charHeight = 5;
                  charWidth = 8;
                  maxICS = 4;
                  dotmatrixCode = 1;
                  break;
               case "5x5":
                  charHeight = 5;
                  charWidth = 8;
                  maxICS = 3;
                  dotmatrixCode = 2;
                  break;
               case "5x8":
               case "5x7":
               case "5x8(5x7)":
                  charHeight = 8;
                  charWidth = 8;
                  maxICS = 3;
                  dotmatrixCode = 3;
                  break;
               case "9x8":
               case "9x7":
               case "9x8(9x7)":
                  charHeight = 8;
                  charWidth = 16;
                  maxICS = 7;
                  dotmatrixCode = 4;
                  break;
               case "7x10":
                  charHeight = 10;
                  charWidth = 8;
                  maxICS = 1;
                  dotmatrixCode = 5;
                  break;
               case "10x12":
                  charHeight = 12;
                  charWidth = 16;
                  maxICS = 6;
                  dotmatrixCode = 6;
                  break;
               case "12x16":
                  charHeight = 16;
                  charWidth = 16;
                  maxICS = 4;
                  dotmatrixCode = 7;
                  break;
               case "18x24":
                  charHeight = 24;
                  charWidth = 24;
                  maxICS = 6;
                  dotmatrixCode = 8;
                  break;
               case "24x32":
                  charHeight = 32;
                  charWidth = 32;
                  maxICS = 12;
                  dotmatrixCode = 9;
                  break;
               case "11x11":
                  charHeight = 11;
                  charWidth = 16;
                  maxICS = 5;
                  dotmatrixCode = 10;
                  break;
               case "5x3(Chimney)":
                  charHeight = 3;
                  charWidth = 5;
                  maxICS = 0;
                  dotmatrixCode = 11;
                  break;
               case "5x5(Chimney)":
                  charHeight = 5;
                  charWidth = 5;
                  maxICS = 0;
                  dotmatrixCode = 12;
                  break;
               case "7x5(Chimney)":
                  charHeight = 5;
                  charWidth = 7;
                  maxICS = 0;
                  dotmatrixCode = 13;
                  break;
               case "30x40":
                  charHeight = 40;
                  charWidth = 40;
                  maxICS = 10;
                  dotmatrixCode = 14;
                  break;
               case "36x48":
                  charHeight = 48;
                  charWidth = 48;
                  maxICS = 12;
                  dotmatrixCode = 15;
                  break;
               default:
                  charHeight = -1;
                  charWidth = -1;
                  maxICS = -1;
                  dotmatrixCode = -1;
                  IsValid = false;
                  break;
            }
         } else {
            charHeight = cbFontRows.SelectedIndex + 1;
            charWidth = cbIcsCols.SelectedIndex + 1;
            ics = 0;
            maxICS = 0;
            dotmatrixCode = -1;
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

