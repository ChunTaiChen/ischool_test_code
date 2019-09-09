using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Process.Rating
{
    partial class SemesterRatingForm : BaseForm
    {
        public SemesterRatingForm()
        {
            InitializeComponent();
            intSchoolYear.MaxValue = CurrentUser.Instance.SchoolYear + 50;
            intSchoolYear.MinValue = CurrentUser.Instance.SchoolYear - 50;
            intSchoolYear.Value = CurrentUser.Instance.SchoolYear;

            intSemester.MinValue = 1;
            intSemester.MaxValue = 2;
            intSemester.Value = CurrentUser.Instance.Semester;
        }

        private void RatingForm_DoubleClick(object sender, EventArgs e)
        {
        }

        private void btnRank_Click(object sender, EventArgs e)
        {
            RatingParameters parameters = new RatingParameters();

            parameters.SchoolYear = intSchoolYear.Value.ToString();
            parameters.Semester = intSemester.Value.ToString();

            string msg = string.Format("現在將會進行成績排名，您確定嗎？", parameters.SchoolYear, parameters.Semester);
            DialogResult dr = MsgBox.Show(msg, Application.ProductName, MessageBoxButtons.YesNo);

            if (dr != DialogResult.Yes) return;

            if (chkSequence.Checked)
                parameters.RatingMethod = RatingMethod.Sequence;
            else if (chkUnSequence.Checked)
                parameters.RatingMethod = RatingMethod.Unsequence;
            else
            {
                MsgBox.Show("您必須要決定一種「排名選項」。", Application.ProductName);
                return;
            }

            if (chkGrade1.Checked) parameters.TargetGradeYears.Add("1");
            if (chkGrade2.Checked) parameters.TargetGradeYears.Add("2");
            if (chkGrade3.Checked) parameters.TargetGradeYears.Add("3");

            if (parameters.TargetGradeYears.Count <= 0)
            {
                MsgBox.Show("您必須選擇要排名的年級。", Application.ProductName);
                return;
            }

            if (chkSemsSubjectScore.Checked)
                parameters.RatingItems |= RatingItems.SemsSubject;

            if (chkSemsScore.Checked)
                parameters.RatingItems |= RatingItems.SemsScore;

            if (chkSemsMoral.Checked)
                parameters.RatingItems |= RatingItems.SemsMoral;

            if (((int)parameters.RatingItems) == 0)
            {
                MsgBox.Show("您必須至少選擇一種「排名項目」。", Application.ProductName);
                return;
            }

            RankProgressForm progress;
            if (Control.ModifierKeys == Keys.Shift)
                progress = new RankProgressForm(parameters, true);
            else
                progress = new RankProgressForm(parameters, false);

            if (progress.ShowDialog() == DialogResult.OK)
            {
                Utility.LogRank("學期成績固定排名",parameters, true);
                MsgBox.Show("排名完成。");
            }
        }
    }
}