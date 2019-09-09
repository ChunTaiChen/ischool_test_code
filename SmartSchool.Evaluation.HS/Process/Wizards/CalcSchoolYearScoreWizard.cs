using System;
using SmartSchool.Common;
using SmartSchool.Evaluation.Process.Rating;

namespace SmartSchool.Evaluation.Process.Wizards
{
    public partial class CalcSchoolYearScoreWizard : BaseForm
    {
        public CalcSchoolYearScoreWizard()
        {
            InitializeComponent();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearSubjectScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonX5_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearApplyScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearEntryScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            new SchoolYearRatingForm().ShowDialog();
        }

        private void buttonX4_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearEntryScoreWizard(SelectType.GradeYearStudent, SchoolYearScoreCalcType.SchoolYearSubject).ShowDialog();
        }
    }
}