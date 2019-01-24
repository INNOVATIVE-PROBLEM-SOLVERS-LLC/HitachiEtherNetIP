using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitachiEIP {
   public partial class AttrProperties : Form {

      ResizeInfo R;
      bool initComplete = false;

      AttrData attr;

      HitachiBrowser parent;
      EIP EIP;
      int selectedCC;
      int selectedAttr;

      eipClassCode[] ClassCodes;
      int[] ClassAttr;

      public AttrProperties(HitachiBrowser parent, EIP EIP, int selectedCC, int selectedAttr) {
         this.EIP = EIP;
         this.selectedCC = selectedCC;
         this.selectedAttr = selectedAttr;
         InitializeComponent();
         initComplete = true;
      }

      private void Properties_Load(object sender, EventArgs e) {
         Utils.PositionForm(this, 0.3f, 0.5f);
         string[] ClassNames = Enum.GetNames(typeof(eipClassCode));
         ClassCodes = (eipClassCode[])Enum.GetValues(typeof(eipClassCode));

         cbFormat.Items.Clear();
         cbFormat.Items.AddRange(Enum.GetNames(typeof(DataFormats)));

         cbClassCode.Items.Clear();
         for (int i = 0; i < ClassNames.Length; i++) {
            cbClassCode.Items.Add($"{ClassNames[i].Replace('_', ' ')} (0x{(byte)ClassCodes[i]:X2})");
         }
         cbClassCode.SelectedIndex = selectedCC;
         cbAttribute.SelectedIndex = selectedAttr;
         SetButtonEnables();
      }

      private void cbClassCode_SelectedIndexChanged(object sender, EventArgs e) {
         cbAttribute.Items.Clear();
         //ClassAttr = null;

         btnIssueGet.Visible = false;
         btnIssueSet.Visible = false;
         btnIssueService.Visible = false;

         if (cbClassCode.SelectedIndex >= 0) {
            // Get all names associated with the enumeration
            string[] names = Data.ClassCodeAttributes[cbClassCode.SelectedIndex].GetEnumNames();
            ClassAttr = (int[])Data.ClassCodeAttributes[cbClassCode.SelectedIndex].GetEnumValues();
            for (int i = 0; i < names.Length; i++) {
               cbAttribute.Items.Add($"{names[i].Replace('_', ' ')}  (0x{ClassAttr[i]:X2})");
            }
         }
         SetButtonEnables();
      }

      private void cbAttribute_SelectedIndexChanged(object sender, EventArgs e) {
         btnIssueGet.Visible = false;
         btnIssueSet.Visible = false;
         btnIssueService.Visible = false;
         if (cbClassCode.SelectedIndex >= 0 && cbAttribute.SelectedIndex >= 0) {
            attr = new AttrData(Data.ClassCodeData[cbClassCode.SelectedIndex][cbAttribute.SelectedIndex]);
            btnIssueGet.Visible = attr.HasGet;
            btnIssueSet.Visible = attr.HasSet;
            btnIssueService.Visible = attr.HasService;
         }
         chkHasGet.Checked = attr.HasGet;
         chkHasSet.Checked = attr.HasSet;
         chkHasService.Checked = attr.HasService;

         txtLength.Text = attr.Len.ToString();
         txtMin.Text = attr.Min.ToString();
         txtMax.Text = attr.Max.ToString();

         cbFormat.SelectedIndex = (int)attr.Fmt;
         chkLockUp.Checked = attr.Ignore;

         SetButtonEnables();
      }

      private void AttrProperties_Resize(object sender, EventArgs e) {
         //
         // Avoid resize on screen minimize
         if (initComplete && ClientRectangle.Height > 0) {
            //
            this.SuspendLayout();
            // Build local parameters
            R = Utils.InitializeResize(this, 23, 14, true);

            Utils.ResizeObject(ref R, lblClassCode, 2, 1, 2, 4);
            Utils.ResizeObject(ref R, cbClassCode, 2, 5, 2, 8);
            Utils.ResizeObject(ref R, lblAttribute, 4, 1, 2, 4);
            Utils.ResizeObject(ref R, cbAttribute, 4, 5, 2, 8);

            Utils.ResizeObject(ref R, chkHasSet, 7, 1, 2, 4);
            Utils.ResizeObject(ref R, chkHasGet, 9, 1, 2, 4);
            Utils.ResizeObject(ref R, chkHasService, 11, 1, 2, 4);
            Utils.ResizeObject(ref R, chkLockUp, 13, 1, 2, 4);

            Utils.ResizeObject(ref R, lblLength, 7, 5, 2, 4);
            Utils.ResizeObject(ref R, txtLength, 7, 9, 2, 4);
            Utils.ResizeObject(ref R, lblMin, 9, 5, 2, 4);
            Utils.ResizeObject(ref R, txtMin, 9, 9, 2, 4);
            Utils.ResizeObject(ref R, lblMax, 11, 5, 2, 4);
            Utils.ResizeObject(ref R, txtMax, 11, 9, 2, 4);
            Utils.ResizeObject(ref R, lblFormat, 13, 5, 2, 4);
            Utils.ResizeObject(ref R, cbFormat, 13, 9, 2, 4);

            Utils.ResizeObject(ref R, lblData, 16, 1, 2, 2);
            Utils.ResizeObject(ref R, txtData, 16, 3, 2, 3);
            Utils.ResizeObject(ref R, lblRawData, 18, 1, 2, 2);
            Utils.ResizeObject(ref R, txtRawData, 18, 3, 2, 10);

            Utils.ResizeObject(ref R, lblStatus, 16, 6, 2, 2);
            Utils.ResizeObject(ref R, txtStatus, 16, 8, 2, 5);

            Utils.ResizeObject(ref R, btnIssueGet, 20, 1, 2, 3);
            Utils.ResizeObject(ref R, btnIssueSet, 20, 4.5f, 2, 3);
            Utils.ResizeObject(ref R, btnIssueService, 20, 1, 2, 6.5f);

            Utils.ResizeObject(ref R, btnExit, 20, 10, 2, 3);

            this.Refresh();
            this.ResumeLayout();
         }
      }

      private void chkHasSet_CheckedChanged(object sender, EventArgs e) {
         if (chkHasSet.Checked) {
            chkHasService.Checked = false;
         }
         SetButtonEnables();
      }

      private void chkHasGet_CheckedChanged(object sender, EventArgs e) {
         if (chkHasGet.Checked) {
            chkHasService.Checked = false;
         }
         SetButtonEnables();
      }

      private void chkHasService_CheckedChanged(object sender, EventArgs e) {
         if (chkHasService.Checked) {
            chkHasGet.Checked = false;
            chkHasSet.Checked = false;
         }
         SetButtonEnables();
      }

      private void SetButtonEnables() {
         bool attrSelected = cbClassCode.SelectedIndex >= 0 && cbAttribute.SelectedIndex >= 0;
         btnIssueGet.Visible = chkHasGet.Checked && attrSelected;
         btnIssueSet.Visible = chkHasSet.Checked && attrSelected;
         btnIssueService.Visible = chkHasService.Checked && attrSelected;
      }

      private void btnIssueSet_Click(object sender, EventArgs e) {
         bool Success = false;
         try {
            byte[] Data = EIP.FormatOutput(txtData.Text, attr);
            Success = EIP.WriteOneAttribute(ClassCodes[cbClassCode.SelectedIndex], (byte)ClassAttr[cbAttribute.SelectedIndex], Data);
            txtStatus.Text = EIP.GetStatus;
         } catch (Exception e2) {

         }
      }

      private void btnExit_Click(object sender, EventArgs e) {
         this.Close();
      }

      private void btnIssueGet_Click(object sender, EventArgs e) {
         bool Success = false;
         Success = EIP.ReadOneAttribute(ClassCodes[cbClassCode.SelectedIndex], (byte)ClassAttr[cbAttribute.SelectedIndex], out string val, attr.Fmt);
         txtStatus.Text = EIP.GetStatus;
         txtData.Text = EIP.GetDataValue;
         txtRawData.Text = EIP.GetBytes(EIP.GetData, 0, EIP.GetDataLength);
      }
   }
}
