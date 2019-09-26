using System;
using System.Collections.Generic;
using System.Text;
//using SmartSchool.Customization.PlugIn.ImportExport;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.AccessControl;
using SmartSchool.API.PlugIn;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool.Evaluation.ImportExport
{
    [FeatureCode("Button0140")]
    class ExportSemesterSubjectScore : SmartSchool.API.PlugIn.Export.Exporter//ExportProcess
    {

        public ExportSemesterSubjectScore()
        {
            this.Image = null;
            this.Text = "匯出學期科目成績";
        }
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            SmartSchool.API.PlugIn.VirtualCheckBox filterRepeat = new SmartSchool.API.PlugIn.VirtualCheckBox("自動略過重讀成績", true);
            wizard.Options.Add(filterRepeat);
            wizard.ExportableFields.AddRange(
                "科目"
                , "科目級別"
                , "學年度"
                , "學期"
                , "英文名稱"
                , "學分數"
                , "分項類別"
                , "成績年級"
                , "必選修"
                , "校部訂"
                , "科目成績"
                , "原始成績"
                , "補考成績"
                , "重修成績"
                , "手動調整成績"
                , "學年調整成績"
                , "取得學分"
                , "不計學分"
                , "不需評分"
                , "註記"
                , "計算規則-及格標準"
                , "計算規則-補考標準"
                , "畢業採計-學分數"
                , "畢業採計-分項類別"
                , "畢業採計-必選修"
                , "畢業採計-校部訂"
                , "畢業採計-不計學分"
                , "畢業採計-說明"
                , "是否補修成績"
                , "重修學年度"
                , "重修學期"
                , "修課及格標準"
                , "修課補考標準"
                , "修課科目代碼"
                , "修課備註"
                , "修課直接指定總成績"
                );
            filterRepeat.CheckedChanged += delegate
            {
                if (filterRepeat.Checked)
                {
                    foreach (var item in new string[]{ "畢業採計-學分數"
                            , "畢業採計-分項類別"
                            , "畢業採計-必選修"
                            , "畢業採計-校部訂"
                            , "畢業採計-不計學分"
                            , "畢業採計-說明"}
                        )
                    {
                        if (!wizard.ExportableFields.Contains(item))
                        {
                            wizard.ExportableFields.Add(item);
                        }
                    }
                }
                else
                {
                    foreach (var item in new string[]{ "畢業採計-學分數"
                            , "畢業採計-分項類別"
                            , "畢業採計-必選修"
                            , "畢業採計-校部訂"
                            , "畢業採計-不計學分"
                            , "畢業採計-說明"}
                        )
                    {
                        if (wizard.ExportableFields.Contains(item))
                        {
                            wizard.ExportableFields.Remove(item);
                        }
                    }
                }
            };
            AccessHelper _AccessHelper = new AccessHelper();
            wizard.ExportPackage += delegate (object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                List<StudentRecord> students = _AccessHelper.StudentHelper.GetStudents(e.List);
                if (filterRepeat.Checked)
                {
                    var gCheck = false;
                    foreach (var item in new string[]{ "畢業採計-學分數"
                            , "畢業採計-分項類別"
                            , "畢業採計-必選修"
                            , "畢業採計-校部訂"
                            , "畢業採計-不計學分"
                            , "畢業採計-說明"}
                        )
                    {
                        if (e.ExportFields.Contains(item))
                        {
                            gCheck = true;
                            break;
                        }
                    }
                    if (gCheck)
                        new WearyDogComputer().FillStudentGradCheck(_AccessHelper, students);
                    //_AccessHelper.StudentHelper.FillSemesterSubjectScore(filterRepeat.Checked, students);
                    else
                        _AccessHelper.StudentHelper.FillSemesterSubjectScore(filterRepeat.Checked, students);
                }
                else
                {
                    _AccessHelper.StudentHelper.FillSemesterSubjectScore(false, students);
                }
                if (e.ExportFields.Contains("計算規則-及格標準"))
                    _AccessHelper.StudentHelper.FillField("及格標準", students);
                if (e.ExportFields.Contains("計算規則-補考標準"))
                    _AccessHelper.StudentHelper.FillField("補考標準", students);
                foreach (StudentRecord stu in students)
                {
                    foreach (SemesterSubjectScoreInfo var in stu.SemesterSubjectScoreList)
                    {
                        RowData row = new RowData();
                        row.ID = stu.StudentID;
                        foreach (string field in e.ExportFields)
                        {
                            if (wizard.ExportableFields.Contains(field))
                            {
                                switch (field)
                                {
                                    case "科目": row.Add(field, var.Subject); break;
                                    case "科目級別": row.Add(field, var.Level); break;
                                    case "學年度": row.Add(field, "" + var.SchoolYear); break;
                                    case "學期": row.Add(field, "" + var.Semester); break;
                                    case "英文名稱": row.Add(field, var.Detail.GetAttribute("英文名稱")); break;
                                    case "學分數": row.Add(field, "" + var.CreditDec()); break;
                                    case "分項類別": row.Add(field, var.Detail.GetAttribute("開課分項類別")); break;
                                    case "成績年級": row.Add(field, "" + var.GradeYear); break;
                                    case "必選修": row.Add(field, var.Require ? "必修" : "選修"); break;
                                    case "校部訂": row.Add(field, var.Detail.GetAttribute("修課校部訂")); break;
                                    case "科目成績": row.Add(field, "" + var.Score); break;
                                    case "原始成績": row.Add(field, var.Detail.GetAttribute("原始成績")); break;
                                    case "補考成績": row.Add(field, var.Detail.GetAttribute("補考成績")); break;
                                    case "重修成績": row.Add(field, var.Detail.GetAttribute("重修成績")); break;
                                    case "手動調整成績": row.Add(field, var.Detail.GetAttribute("擇優採計成績")); break;
                                    case "學年調整成績": row.Add(field, var.Detail.GetAttribute("學年調整成績")); break;
                                    case "取得學分": row.Add(field, var.Pass ? "是" : "否"); break;
                                    case "不計學分": row.Add(field, var.Detail.GetAttribute("不計學分") == "是" ? "是" : ""); break;
                                    case "不需評分": row.Add(field, var.Detail.GetAttribute("不需評分") == "是" ? "是" : ""); break;
                                    case "註記": row.Add(field, var.Detail.HasAttribute("註記") ? var.Detail.GetAttribute("註記") : ""); break;

                                    case "計算規則-及格標準":
                                        if (stu.Fields.ContainsKey("及格標準") && stu.Fields["及格標準"] is Dictionary<int, decimal>)
                                        {
                                            Dictionary<int, decimal> applyLimit = (Dictionary<int, decimal>)stu.Fields["及格標準"];
                                            if (applyLimit.ContainsKey(var.GradeYear))
                                            {
                                                row.Add(field, "" + applyLimit[var.GradeYear]);
                                            }
                                            else
                                            {
                                                row.Add(field, "無法判斷");
                                            }
                                        }
                                        else
                                        {
                                            row.Add(field, "沒有成績計算規則");
                                        }
                                        break;

                                    case "計算規則-補考標準":
                                        if (stu.Fields.ContainsKey("補考標準") && stu.Fields["補考標準"] is Dictionary<int, decimal>)
                                        {
                                            Dictionary<int, decimal> reexamLimit = (Dictionary<int, decimal>)stu.Fields["補考標準"];
                                            if (reexamLimit.ContainsKey(var.GradeYear))
                                            {
                                                row.Add(field, "" + reexamLimit[var.GradeYear]);
                                            }
                                            else
                                            {
                                                row.Add(field, "無法判斷");
                                            }
                                        }
                                        else
                                        {
                                            row.Add(field, "沒有成績計算規則");
                                        }
                                        break;

                                    case "畢業採計-學分數":
                                    case "畢業採計-分項類別":
                                    case "畢業採計-必選修":
                                    case "畢業採計-校部訂":
                                    case "畢業採計-不計學分":
                                        if (var.Detail.GetAttribute(field) == "")
                                            row.Add(field, "--");
                                        else
                                            row.Add(field, var.Detail.GetAttribute(field));
                                        break;
                                    case "畢業採計-說明":
                                        row.Add(field, var.Detail.GetAttribute(field));
                                        break;

                                    case "是否補修成績": row.Add(field, var.Detail.GetAttribute("是否補修成績") == "是" ? "是" : ""); break;
                                    case "重修學年度": row.Add(field, var.Detail.GetAttribute("重修學年度")); break;
                                    case "重修學期": row.Add(field, var.Detail.GetAttribute("重修學期")); break;
                                    case "修課及格標準": row.Add(field, var.Detail.GetAttribute("修課及格標準")); break;
                                    case "修課補考標準": row.Add(field, var.Detail.GetAttribute("修課補考標準")); break;
                                    case "修課科目代碼": row.Add(field, var.Detail.GetAttribute("修課科目代碼")); break;
                                    case "修課備註": row.Add(field, var.Detail.GetAttribute("修課備註")); break;
                                    case "修課直接指定總成績": row.Add(field, var.Detail.GetAttribute("修課直接指定總成績")); break;
                                }
                            }
                        }
                        e.Items.Add(row);
                    }
                }
            };
        }
        //private AccessHelper _AccessHelper;

        //public ExportSemesterSubjectScore()
        //{
        //    this.Image = null; //Properties.Resources.ExportScoreImage;
        //    this.Title = "匯出學期科目成績";
        //    this.Group = "學期科目成績";
        //    foreach ( string var in new string[] { "科目", "科目級別", "學年度", "學期", "學分數", "分項類別", "成績年級", "必選修", "校部訂", "科目成績", "原始成績", "補考成績", "重修成績", "手動調整成績", "學年調整成績", "取得學分", "不計學分", "不需評分", "註記" } )
        //    {
        //        this.ExportableFields.Add(var);
        //    }
        //    this.ExportPackage += new EventHandler<ExportPackageEventArgs>(ExportSemesterSubjectScore_ExportPackage);
        //    _AccessHelper = new AccessHelper();
        //}

        //void ExportSemesterSubjectScore_ExportPackage(object sender, ExportPackageEventArgs e)
        //{
        //    List<StudentRecord> students = _AccessHelper.StudentHelper.GetStudents(e.List);
        //    _AccessHelper.StudentHelper.FillSemesterSubjectScore(true, students);
        //    foreach ( StudentRecord stu in students )
        //    {
        //        foreach ( SemesterSubjectScoreInfo var in stu.SemesterSubjectScoreList )
        //        {
        //            RowData row = new RowData();
        //            row.ID = stu.StudentID;
        //            foreach ( string field in e.ExportFields )
        //            {
        //                if ( ExportableFields.Contains(field) )
        //                {
        //                    switch ( field )
        //                    {
        //                        case "科目": row.Add(field,var.Subject); break;
        //                        case "科目級別": row.Add(field, var.Level); break;
        //                        case "學年度": row.Add(field, ""+var.SchoolYear); break;
        //                        case "學期": row.Add(field, ""+var.Semester); break;
        //                        case "學分數": row.Add(field,""+ var.Credit); break;
        //                        case "分項類別": row.Add(field, var.Detail.GetAttribute("開課分項類別")); break;
        //                        case "成績年級": row.Add(field, ""+var.GradeYear); break;
        //                        case "必選修": row.Add(field, var.Require ? "必修" : "選修"); break;
        //                        case "校部訂": row.Add(field, var.Detail.GetAttribute("修課校部訂")); break;
        //                        case "科目成績": row.Add(field, "" + var.Score); break;
        //                        case "原始成績": row.Add(field, var.Detail.GetAttribute("原始成績")); break;
        //                        case "補考成績": row.Add(field, var.Detail.GetAttribute("補考成績")); break;
        //                        case "重修成績": row.Add(field, var.Detail.GetAttribute("重修成績")); break;
        //                        case "手動調整成績": row.Add(field, var.Detail.GetAttribute("擇優採計成績")); break;
        //                        case "學年調整成績": row.Add(field, var.Detail.GetAttribute("學年調整成績")); break;
        //                        case "取得學分": row.Add(field, var.Pass ? "是" : "否"); break;
        //                        case "不計學分": row.Add(field, var.Detail.GetAttribute("不計學分") == "是" ? "是" : ""); break;
        //                        case "不需評分": row.Add(field, var.Detail.GetAttribute("不需評分") == "是" ? "是" : ""); break;
        //                        case "註記": row.Add(field, var.Detail.HasAttribute("註記") ? var.Detail.GetAttribute("註記") : ""); break;
        //                    }
        //                }
        //            }
        //            e.Items.Add(row);
        //        }
        //    }
        //}
    }
}