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
    [FeatureCode("Button0170")]
    class ExportSchoolYearEntryScore : SmartSchool.API.PlugIn.Export.Exporter//ExportProcess
    {
        public ExportSchoolYearEntryScore()
        {
            this.Image = null;
            this.Text = "匯出學年分項成績";
        }
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            SmartSchool.API.PlugIn.VirtualCheckBox filterRepeat = new SmartSchool.API.PlugIn.VirtualCheckBox("自動略過重讀成績", true);
            wizard.Options.Add(filterRepeat);
            wizard.ExportableFields.AddRange("學年度", "成績年級", "學業", "體育", "國防通識", "健康與護理", "實習科目", "專業科目", "德行");
            AccessHelper _AccessHelper = new AccessHelper();
            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                List<StudentRecord> students = _AccessHelper.StudentHelper.GetStudents(e.List);
                _AccessHelper.StudentHelper.FillSchoolYearEntryScore(filterRepeat.Checked, students);
                foreach ( StudentRecord stu in students )
                {
                    Dictionary<int, List<SchoolYearEntryScoreInfo>> schoolYearEntryScoreList = new Dictionary<int, List<SchoolYearEntryScoreInfo>>();
                    foreach ( SchoolYearEntryScoreInfo var in stu.SchoolYearEntryScoreList )
                    {
                        if ( !schoolYearEntryScoreList.ContainsKey(var.SchoolYear) )
                            schoolYearEntryScoreList.Add(var.SchoolYear, new List<SchoolYearEntryScoreInfo>());
                        schoolYearEntryScoreList[var.SchoolYear].Add(var);
                    }
                    foreach ( int sy in schoolYearEntryScoreList.Keys )
                    {
                        RowData row = new RowData();
                        row.ID = stu.StudentID;
                        row.Add("學年度", "" + sy);
                        foreach ( SchoolYearEntryScoreInfo var in schoolYearEntryScoreList[sy] )
                        {
                            if ( !row.ContainsKey("成績年級") )
                                row.Add("成績年級", "" + var.GradeYear);
                            if ( e.ExportFields.Contains(var.Entry) )
                            {
                                row.Add(var.Entry, "" + var.Score);
                            }
                        }
                        e.Items.Add(row);
                    }
                }
            };
        }
        //private AccessHelper _AccessHelper;

        //public ExportSchoolYearEntryScore()
        //{
        //    this.Image = null;
        //    this.Title = "匯出學年分項成績";
        //    this.Group = "學年分項成績";
        //    foreach ( string var in new string[] { "學年度", "成績年級", "學業", "體育", "國防通識", "健康與護理", "實習科目", "德行" } )
        //    {
        //        this.ExportableFields.Add(var);
        //    }
        //    this.ExportPackage += new EventHandler<ExportPackageEventArgs>(ExportSemesterEntryScore_ExportPackage);
        //    _AccessHelper = new AccessHelper();
        //}

        //void ExportSemesterEntryScore_ExportPackage(object sender, ExportPackageEventArgs e)
        //{
        //    List<StudentRecord> students = _AccessHelper.StudentHelper.GetStudents(e.List);
        //    _AccessHelper.StudentHelper.FillSchoolYearEntryScore(true, students);
        //    foreach ( StudentRecord stu in students )
        //    {
        //        Dictionary<int,  List<SchoolYearEntryScoreInfo>> schoolYearEntryScoreList = new Dictionary<int,  List<SchoolYearEntryScoreInfo>>();
        //        foreach ( SchoolYearEntryScoreInfo var in stu.SchoolYearEntryScoreList )
        //        {
        //            if ( !schoolYearEntryScoreList.ContainsKey(var.SchoolYear) )
        //                schoolYearEntryScoreList.Add(var.SchoolYear,new List<SchoolYearEntryScoreInfo>() );
        //            schoolYearEntryScoreList[var.SchoolYear].Add(var);
        //        }
        //        foreach ( int sy in schoolYearEntryScoreList.Keys )
        //        {
        //            RowData row = new RowData();
        //            row.ID = stu.StudentID;
        //            row.Add("學年度", "" + sy);
        //            foreach ( SchoolYearEntryScoreInfo var in schoolYearEntryScoreList[sy] )
        //            {
        //                if ( !row.ContainsKey("成績年級") )
        //                    row.Add("成績年級", "" + var.GradeYear);
        //                if ( ExportableFields.Contains(var.Entry) )
        //                {
        //                    row.Add(var.Entry, "" + var.Score);
        //                }
        //            }
        //            e.Items.Add(row);
        //        }
        //    }
        //}
    }
}
