using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using FISCA.Permission;
using FISCA.Presentation;

namespace HsinChuExamScore_JH
{
    /// <summary>
    /// 新竹評量成績單
    /// </summary>
    public class Program
    {
        static DataTable _dtEpost = new DataTable();

        [FISCA.MainMethod]
        public static void Main()
        {
            RibbonBarItem rbItem1 = MotherForm.RibbonBarItems["學生", "資料統計"];
            rbItem1["報表"]["成績相關報表"]["評量成績通知單(固定排名)"].Enable = UserAcl.Current["JH.Student.HsinChuExamScore_JH_Student"].Executable;
            rbItem1["報表"]["成績相關報表"]["評量成績通知單(固定排名)"].Click += delegate
            {
                if (K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0)
                {
                    PrintForm pf = new PrintForm(K12.Presentation.NLDPanels.Student.SelectedSource);
                    pf.ShowDialog();
                }
                else
                {
                    FISCA.Presentation.Controls.MsgBox.Show("請選擇選學生");
                    return;
                }
            };

            RibbonBarItem rbItem2 = MotherForm.RibbonBarItems["班級", "資料統計"];
            rbItem2["報表"]["成績相關報表"]["評量成績通知單(固定排名)"].Enable = UserAcl.Current["JH.Student.HsinChuExamScore_JH_Class"].Executable;
            rbItem2["報表"]["成績相關報表"]["評量成績通知單(固定排名)"].Click += delegate
            {
                if (K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0)
                {
                    List<string> StudentIDList = Utility.GetClassStudentIDList1ByClassID(K12.Presentation.NLDPanels.Class.SelectedSource);
                    PrintForm pf = new PrintForm(StudentIDList);
                    pf.ShowDialog();
                }
                else
                {
                    FISCA.Presentation.Controls.MsgBox.Show("請選擇選班級");
                    return;                
                }
            
            };
            // 評量成績通知單
            Catalog catalog1a = RoleAclSource.Instance["學生"]["功能按鈕"];
            catalog1a.Add(new RibbonFeature("JH.Student.HsinChuExamScore_JH_Student", "評量成績通知單(固定排名)"));

            // 評量成績通知單
            Catalog catalog1b = RoleAclSource.Instance["班級"]["功能按鈕"];
            catalog1b.Add(new RibbonFeature("JH.Student.HsinChuExamScore_JH_Class", "評量成績通知單(固定排名)"));

        }
   
    }
}
