using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Process
{
    public partial class SelectSemesterForm : BaseForm
    {
        // 是否開全部課程
        public bool isCreateAll = false;

    
        K12.Data.Configuration.ConfigData cd = K12.Data.School.Configuration["依課程規劃表開課設定畫面"];

        public SelectSemesterForm()
        {
            InitializeComponent();
            
            Text = "選擇學年度學期";
            bool.TryParse(cd["含選修課"].ToString(),out isCreateAll);
            chkCreateAll.Checked = isCreateAll;
            try
            {
                for (int i = -2; i <= 2; i++)
                {
                    cboSchoolYear.Items.Add(CurrentUser.Instance.SchoolYear + i);
                }
                cboSchoolYear.Text = CurrentUser.Instance.SchoolYear.ToString();
                cboSemester.Text = CurrentUser.Instance.Semester.ToString();
            }
            catch (Exception ex)
            {
                CurrentUser.ReportError(ex);
            }
        }

        private EnhancedErrorProvider ErrorProvider
        {
            get { return errorProvider1; }
        }

        public int SchoolYear
        {
            get
            {
                int a;
                if (int.TryParse(cboSchoolYear.Text, out a))
                    return a;
                return CurrentUser.Instance.SchoolYear;
            }
        }

        public int Semester
        {
            get
            {
                int a;
                if (int.TryParse(cboSemester.Text, out a))
                    return a;
                return CurrentUser.Instance.Semester;
            }
        }

        private bool Validate()
        {
            ErrorProvider.Clear();
            int i;
            if (!int.TryParse(cboSchoolYear.Text, out i))
                ErrorProvider.SetError(cboSchoolYear, "學年度必須為數字");
            if (int.TryParse(cboSchoolYear.Text, out i) && i <= 0)
                ErrorProvider.SetError(cboSchoolYear, "學年度必須為正整數");
            if (!int.TryParse(cboSemester.Text, out i))
                ErrorProvider.SetError(cboSemester, "學期必須為 1 或 2");

            return !ErrorProvider.HasError;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!Validate())
                return;

            isCreateAll = chkCreateAll.Checked;
            cd["含選修課"] = isCreateAll.ToString();
            cd.Save();
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void chkCreateAll_CheckedChanged(object sender, EventArgs e)
        {
            this.isCreateAll = chkCreateAll.Checked;
        }
    }
}