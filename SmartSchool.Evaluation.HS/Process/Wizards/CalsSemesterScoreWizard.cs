using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.Evaluation.Process.Rating;

namespace SmartSchool.Evaluation.Process.Wizards
{
    public partial class CalsSemesterScoreWizard : BaseForm
    {
        public CalsSemesterScoreWizard()
        {
            InitializeComponent();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            new CalcSemesterSubjectScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            new CalcSemesterEntryScoreWizard(SelectType.GradeYearStudent).ShowDialog();
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            //new SemesterRatingForm().ShowDialog();
            // 2018/5/31 穎驊 因應[H成績][03] 學期成績計算相關流程操作文字提醒 調整，將原本的計算排名功能關閉，一率引導到固定排名功能
            SHStaticRank.Data.CalcSemeSubjectRank.DoRank(true);  // 教務作業 批次成績處理 將會鎖住學年度學期，不讓使用者自己調整。
        }
    }
}