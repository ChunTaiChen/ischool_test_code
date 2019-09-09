using System;
using FISCA.Presentation;
using SmartSchool.AccessControl;
using SmartSchool.Evaluation.Process.Rating;
//using SmartSchool.StudentRelated;
using SmartSchool.Evaluation.Process.Wizards;

namespace SmartSchool.Evaluation.Process
{
    [FeatureCode("Button0705")]
    public partial class CalculationBatch : SmartSchool.Evaluation.Process.RibbonBarBase
    {
        public CalculationBatch()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalculationBatch));
            var buttonItem103 = MotherForm.RibbonBarItems["教務作業", "批次作業/檢視"]["成績作業"];
            buttonItem103.Enable = CurrentUser.Acl["Button0670"].Executable;
            //buttonItem103.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem103.Image") ) );
            buttonItem103["學期成績處理"].BeginGroup = true;
            buttonItem103["學期成績處理"].Enable = CurrentUser.Acl["Button0670"].Executable;
            buttonItem103["學年成績處理"].Enable = CurrentUser.Acl["Button0670"].Executable;

            buttonItem103["學期成績處理"].Click += new System.EventHandler(this.buttonItem6_Click_1);
            buttonItem103["學年成績處理"].Click += new System.EventHandler(this.buttonItem8_Click_1);

            var buttonItem9 = MotherForm.RibbonBarItems["學務作業", "成績作業"]["德行成績(舊制)"];
            buttonItem9.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem9.Image") ) );
            buttonItem9.Enable = CurrentUser.Acl["Button0705"].Executable;
            buttonItem9["計算德行學期成績(舊制)"].Click += new System.EventHandler(this.buttonItem6_Click);
            buttonItem9["計算德行學年成績(舊制)"].Click += new System.EventHandler(this.buttonItem8_Click);
        }
        public override string ProcessTabName
        {
            get
            {
                return "成績處理";
            }
        }
        /// <summary>
        /// 計算學期分項成績
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonItem5_Click(object sender, EventArgs e)
        {
            new CalcSemesterEntryScoreWizard(SelectType.GradeYearStudent).ShowDialog();

        }
        /// <summary>
        /// 計算學年科目成績
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonItem4_Click(object sender, EventArgs e)
        {
            new CalcSemesterSubjectScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonItem6_Click(object sender, EventArgs e)
        {
            new CalcSemesterMoralScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonItem3_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearEntryScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonItem8_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearMoralScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonItem2_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearSubjectScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonItem2_1_Click(object sender, EventArgs e)
        {
            new CalcSchoolYearApplyScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void btnSemesterRank_Click(object sender, EventArgs e)
        {
            new SemesterRatingForm().ShowDialog();
        }

        private void btnSchoolYearRank_Click(object sender, EventArgs e)
        {
            new SchoolYearRatingForm().ShowDialog();
        }

        private void buttonItem6_Click_1(object sender, EventArgs e)
        {
            new CalsSemesterScoreWizard().ShowDialog();
        }

        private void buttonItem8_Click_1(object sender, EventArgs e)
        {
            new CalcSchoolYearScoreWizard().ShowDialog();
        }
    }
}

