using System;
using System.Collections.Generic;
using System.Text;
//using SmartSchool.Customization.PlugIn.ImportExport;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.AccessControl;
using SmartSchool.API.PlugIn;

namespace SmartSchool.Evaluation.ImportExport
{
    [FeatureCode("Button0160")]
    class ExportSchoolYearSubjectScore : SmartSchool.API.PlugIn.Export.Exporter//ExportProcess
    {
        public ExportSchoolYearSubjectScore()
        {
            this.Image = null;
            this.Text = "匯出學年科目成績";
        }
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            SmartSchool.API.PlugIn.VirtualCheckBox filterRepeat = new SmartSchool.API.PlugIn.VirtualCheckBox("自動略過重讀成績", true);
            wizard.Options.Add(filterRepeat);
            wizard.ExportableFields.AddRange("學年度", "成績年級", "科目", "學年成績", "結算成績", "補考成績", "重修成績");
            AccessHelper _AccessHelper = new AccessHelper();
            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                List<StudentRecord> students = _AccessHelper.StudentHelper.GetStudents(e.List);
                _AccessHelper.StudentHelper.FillSchoolYearSubjectScore(filterRepeat.Checked, students);
                foreach (StudentRecord stu in students)
                {
                    foreach (SchoolYearSubjectScoreInfo var in stu.SchoolYearSubjectScoreList)
                    {
                        RowData row = new RowData();
                        row.ID = stu.StudentID;
                        foreach (string field in e.ExportFields)
                        {
                            if (wizard.ExportableFields.Contains(field))
                            {
                                switch (field)
                                {
                                    case "學年度": row.Add(field, "" + var.SchoolYear); break;
                                    case "成績年級": row.Add(field, "" + var.GradeYear); break;
                                    case "科目": row.Add(field, var.Subject); break;
                                    case "學年成績": row.Add(field, "" + var.Score); break;
                                    case "結算成績": row.Add(field, var.Detail.GetAttribute(field) == "" ? ("" + var.Score) : var.Detail.GetAttribute(field)); break;
                                    case "補考成績": row.Add(field, var.Detail.GetAttribute(field)); break;
                                    case "重修成績": row.Add(field, var.Detail.GetAttribute(field)); break;
                                }
                            }
                        }
                        e.Items.Add(row);
                    }
                }
            };
        }
        ////private AccessHelper _AccessHelper;

        ////public ExportSchoolYearSubjectScore()
        ////{
        ////    this.Image = null;
        ////    this.Title = "匯出學年科目成績";
        ////    this.Group = "學年科目成績";
        ////    foreach ( string var in new string[] { "學年度", "成績年級","科目", "學年成績" } )
        ////    {
        ////        this.ExportableFields.Add(var);
        ////    }
        ////    this.ExportPackage += new EventHandler<ExportPackageEventArgs>(ExportSemesterSubjectScore_ExportPackage);
        ////    _AccessHelper = new AccessHelper();
        ////}

        ////void ExportSemesterSubjectScore_ExportPackage(object sender, ExportPackageEventArgs e)
        ////{
        ////    List<StudentRecord> students = _AccessHelper.StudentHelper.GetStudents(e.List);
        ////    _AccessHelper.StudentHelper.FillSchoolYearSubjectScore(true, students);
        ////    foreach ( StudentRecord stu in students )
        ////    {
        ////        foreach ( SchoolYearSubjectScoreInfo var in stu.SchoolYearSubjectScoreList )
        ////        {
        ////            RowData row = new RowData();
        ////            row.ID = stu.StudentID;
        ////            foreach ( string field in e.ExportFields )
        ////            {
        ////                if ( ExportableFields.Contains(field) )
        ////                {
        ////                    switch ( field )
        ////                    {
        ////                        case "學年度": row.Add(field, ""+var.SchoolYear); break;
        ////                        case "成績年級": row.Add(field, ""+var.GradeYear); break;
        ////                        case "科目": row.Add(field,var.Subject); break;
        ////                        case "學年成績": row.Add(field, "" + var.Score); break;
        ////                    }
        ////                }
        ////            }
        ////            e.Items.Add(row);
        ////        }
        ////    }
        ////}
    }
}
