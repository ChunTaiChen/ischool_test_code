using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using SmartSchool;
using SmartSchool.Evaluation.Reports;
using FISCA.Presentation;

namespace SmartSchool.Evaluation.Process
{
    public partial class Retake : SmartSchool.Evaluation.Process.RibbonBarBase
    {
        public Retake()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Retake));
            var buttonItem52 = MotherForm.RibbonBarItems["教務作業", "批次作業/檢視"]["成績作業"];
            buttonItem52.Image = Properties.Resources.calc_save_64;
            buttonItem52.Size = RibbonBarButton.MenuButtonSize.Large;
            buttonItem52["建議重修名單-依科目"].BeginGroup = true;
            buttonItem52["建議重修名單-依科目"].Click += new System.EventHandler(this.buttonItem1_Click);
            buttonItem52["建議重修名單-依學生"].Click += new System.EventHandler(this.buttonItem2_Click);
            buttonItem52["重修成績匯入表"].Click += new System.EventHandler(this.buttonItem3_Click);

            buttonItem52.Enable = CurrentUser.Acl["Button0680"].Executable;
        }
        public override string ProcessTabName
        {
            get
            {
                return "成績處理";
            }
        }
        #region 建議重修名單-依科目 (科目不及格名單)
        private void buttonItem1_Click(object sender, EventArgs e)
        {
            new RetakeListBySubject();
        }
        #endregion

        #region 建議重修名單-依學生 (學生重修科目名單)
        private void buttonItem2_Click(object sender, EventArgs e)
        {
            new RetakeListByStudent();
        }
        #endregion

        #region 重修成績匯入表
        private void buttonItem3_Click(object sender, EventArgs e)
        {
            new RetakeScoreImport();
        }
        #endregion
    }
}

