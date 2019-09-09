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
    partial class SchoolYearRatingForm : BaseForm
    {
        public SchoolYearRatingForm()
        {
            InitializeComponent();
            intSchoolYear.MaxValue = CurrentUser.Instance.SchoolYear + 50;
            intSchoolYear.MinValue = CurrentUser.Instance.SchoolYear - 50;
            intSchoolYear.Value = CurrentUser.Instance.SchoolYear;

        }

        private void RatingForm_DoubleClick(object sender, EventArgs e)
        {
            //if (Control.ModifierKeys == (Keys.Shift | Keys.Alt))
            //{
            //    txtSchoolYear.Visible = true;
            //    txtSemester.Visible = true;
            //}
        }

        private void btnRank_Click(object sender, EventArgs e)
        {
            RatingParameters parameters = new RatingParameters();

            parameters.SchoolYear = intSchoolYear.Value.ToString();
            parameters.Semester = "1";

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

            //if (chkYearSubjectScore.Checked)
            //    parameters.RatingItems |= RatingItems.SemsSubject;

            if (chkYearSubjectScore.Checked)
                parameters.RatingItems |= RatingItems.YearSubject;

            //if (chkYearScore.Checked)
            //    parameters.RatingItems |= RatingItems.SemsScore;

            if (chkYearScore.Checked)
                parameters.RatingItems |= RatingItems.YearScore;

            //if (chkYearMoral.Checked)
            //    parameters.RatingItems |= RatingItems.SemsMoral;

            if (chkYearMoral.Checked)
                parameters.RatingItems |= RatingItems.YearMoral;

            //if (chkGraduateScore.Checked)
            //    parameters.RatingItems |= RatingItems.GraduateScore;

            //if (chkGraduateMoral.Checked)
            //    parameters.RatingItems |= RatingItems.GraduateMoral;

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
                Utility.LogRank("學年成績固定排名", parameters, false);
                MsgBox.Show("排名完成。");
            }
        }
    }
}