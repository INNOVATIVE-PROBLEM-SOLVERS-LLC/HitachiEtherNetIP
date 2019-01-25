using System;
using System.Drawing;
using System.Windows.Forms;

namespace HitachiEIP {

   #region Global Declarations

   public struct ResizeInfo {
      public float W;
      public float H;
      public float FS;
      public bool setTabIndex;
      public int tabIndex;
      public float offset;
   }
   #endregion

   #region Utils Class

   public static class Utils {

      #region Resize Routines

      static public ResizeInfo InitializeResize(Form F, float nRows, float nCols, bool setTabIndex = true, int offset = 0) {
         ResizeInfo R;
         R.H = (F.ClientRectangle.Height - offset) / nRows;
         R.W = F.ClientRectangle.Width / nCols;
         R.FS = Math.Max(Math.Min(Math.Min(R.W / 2, R.H / 2), 12), 8);
         R.setTabIndex = setTabIndex;
         R.tabIndex = 0;
         R.offset = offset;
         return R;
      }

      static Point getLocation(ResizeInfo R, Control c, float nRow, float nCol) {
         Point result;
         if (c.Parent.GetType().BaseType.Equals(typeof(Form))) {
            result = new System.Drawing.Point((int)(nCol * R.W), (int)(nRow * R.H + R.offset));
         } else {
            result = new System.Drawing.Point((int)(nCol * R.W), (int)(nRow * R.H + R.offset));
         }
         return result;
      }

      static public void ResizeObject(ref ResizeInfo R, DateTimePicker dt, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         dt.Location = getLocation(R, dt, sngRow, sngCol);
         dt.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         dt.Font = ChangeFontSize(dt.Font, R.FS * adjustFont);
         dt.TabIndex = ++R.tabIndex;
      }

      static public void ResizeObject(ref ResizeInfo R, ProgressBar pb, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         pb.Location = getLocation(R, pb, sngRow, sngCol);
         pb.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         pb.Font = ChangeFontSize(pb.Font, R.FS * adjustFont);
         pb.TabIndex = ++R.tabIndex;
      }

      static public void ResizeObject(ref ResizeInfo R, Button b, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         b.Location = getLocation(R, b, sngRow, sngCol);
         b.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         b.Font = ChangeFontSize(b.Font, R.FS * adjustFont);
         if (R.setTabIndex) {
            b.TabIndex = ++R.tabIndex;
         }
      }

      static public void ResizeObject(ref ResizeInfo R, Panel p, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         p.Location = getLocation(R, p, sngRow, sngCol);
         p.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         p.Font = ChangeFontSize(p.Font, R.FS * adjustFont);
      }

      static public void ResizeObject(ref ResizeInfo R, TabControl t, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         t.Location = getLocation(R, t, sngRow, sngCol);
         t.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         t.Font = ChangeFontSize(t.Font, R.FS * adjustFont);
      }

      static public void ResizeObject(ref ResizeInfo R, Label l, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         l.Location = getLocation(R, l, sngRow, sngCol);
         l.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         l.Font = ChangeFontSize(l.Font, R.FS * adjustFont);
      }

      static public void ResizeObject(ref ResizeInfo R, CheckBox c, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         c.Location = getLocation(R, c, sngRow, sngCol);
         c.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         c.Font = ChangeFontSize(c.Font, R.FS * adjustFont);
         if (R.setTabIndex) {
            c.TabIndex = ++R.tabIndex;
         }
      }

      static public void ResizeObject(ref ResizeInfo R, TextBox t, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         t.Location = getLocation(R, t, sngRow, sngCol);
         t.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         t.Font = ChangeFontSize(t.Font, R.FS * adjustFont);
         if (R.setTabIndex) {
            if (t.ReadOnly) {
               t.TabStop = false;
            } else {
               t.TabIndex = ++R.tabIndex;
            }
         }
      }

      static public void ResizeObject(ref ResizeInfo R, GroupBox g, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         g.Location = getLocation(R, g, sngRow, sngCol);
         g.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         g.Font = ChangeFontSize(g.Font, R.FS * adjustFont);
         if (R.setTabIndex) {
            g.TabIndex = ++R.tabIndex;
         }
      }

      static public void ResizeObject(ref ResizeInfo R, ListBox l, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 0.8f) {
         l.Location = getLocation(R, l, sngRow, sngCol);
         l.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         l.Font = ChangeFontSize(l.Font, R.FS * adjustFont);
         l.ItemHeight = l.Font.Height;
         if (R.setTabIndex) {
            l.TabIndex = ++R.tabIndex;
         }
      }

      static public void ResizeObject(ref ResizeInfo R, RadioButton r, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         r.Location = getLocation(R, r, sngRow, sngCol);
         r.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         r.Font = ChangeFontSize(r.Font, R.FS * adjustFont);
         if (R.setTabIndex) {
            r.TabIndex = ++R.tabIndex;
         }
      }

      static public void ResizeObject(ref ResizeInfo R, ComboBox c, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         c.Location = getLocation(R, c, sngRow, sngCol);
         c.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         c.Font = ChangeFontSize(c.Font, R.FS * adjustFont);
         if (R.setTabIndex) {
            c.TabIndex = ++R.tabIndex;
         }
      }

      static public void ResizeObject(ref ResizeInfo R, PictureBox p, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         p.Location = getLocation(R, p, sngRow, sngCol);
         p.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
      }

      static public void ResizeObject(ref ResizeInfo R, HScrollBar h, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         h.Location = getLocation(R, h, sngRow, sngCol);
         h.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
      }

      static public void ResizeObject(ref ResizeInfo R, DataGridView dg, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         dg.Location = getLocation(R, dg, sngRow, sngCol);
         dg.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
         dg.Font = ChangeFontSize(dg.Font, R.FS * adjustFont);
      }

      static public void ResizeObject(ref ResizeInfo R, ToolStrip ts, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         ts.Location = getLocation(R, ts, sngRow, sngCol);
         ts.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
      }

      static public void ResizeObject(ref ResizeInfo R, TreeView tv, float sngRow, float sngCol, float sngNRows, float sngNCols, float adjustFont = 1) {
         tv.Location = getLocation(R, tv, sngRow, sngCol);
         tv.Size = new System.Drawing.Size((int)(sngNCols * R.W), (int)(sngNRows * R.H));
      }

      public static Font ChangeFontSize(Font font, float fontSize) {
         if (font != null) {
            float currentSize = font.Size;
            if (currentSize != fontSize) {
               font = new Font(font.Name, fontSize,
                   font.Style, font.Unit,
                   font.GdiCharSet, font.GdiVerticalFont);
            }
         }
         return font;
      }

      public static void PositionForm(Form f, float width, float height) {
         int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
         int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
         f.Location = new Point((int)((screenWidth - screenWidth * width) / 2f), (int)((screenHeight - screenHeight * height) / 2));
         f.Size = new Size((int)(screenWidth * width), (int)(screenHeight * height));
      }

      public static void PositionForm(Form f) {
         int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
         int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
         f.Location = new Point((int)((screenWidth - f.Width) / 2f), (int)((screenHeight - f.Height) / 2f));
      }

      #endregion

   }

   #endregion

}
