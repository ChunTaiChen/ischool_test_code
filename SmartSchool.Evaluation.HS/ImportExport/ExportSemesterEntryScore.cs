using System;
using System.Collections.Generic;
using System.Text;
//using SmartSchool.Customization.PlugIn.ImportExport;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.AccessControl;
using SmartSchool.API.PlugIn;
using FISCA.Data;
using System.Data;
using System.Xml.Linq;

namespace SmartSchool.Evaluation.ImportExport
{
    [FeatureCode("Button0150")]
    class ExportSemesterEntryScore : SmartSchool.API.PlugIn.Export.Exporter
    {

        public ExportSemesterEntryScore()
        {
            this.Image = null;
            this.Text = "匯出學期分項成績";
        }

        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            SmartSchool.API.PlugIn.VirtualCheckBox filterRepeat = new SmartSchool.API.PlugIn.VirtualCheckBox("自動略過重讀成績", true);
            wizard.Options.Add(filterRepeat);

            // 2018/8 穎驊註解，經過討論後， 先暫時將 ischool類別2 排名的概念拿掉，因為目前的結構 無法區隔類別1、類別2，待日後設計完整
            //wizard.ExportableFields.AddRange("學年度", "學期", "成績年級", "學業", "體育", "國防通識", "健康與護理", "實習科目", "專業科目", "學業(原始)", "體育(原始)", "國防通識(原始)", "健康與護理(原始)", "實習科目(原始)", "專業科目(原始)", "德行", "學業成績班排名", "學業成績班排名母數", "學業成績科排名", "學業成績科排名母數", "學業成績校排名", "學業成績校排名母數", "學業成績排名類別1", "學業成績類1排名", "學業成績類1排名母數", "學業成績排名類別2", "學業成績類2排名", "學業成績類2排名母數");

            wizard.ExportableFields.AddRange("學年度", "學期", "成績年級", "學業", "體育", "國防通識", "健康與護理", "實習科目", "專業科目", "學業(原始)", "體育(原始)", "國防通識(原始)", "健康與護理(原始)", "實習科目(原始)", "專業科目(原始)", "德行", "學業成績班排名", "學業成績班排名母數", "學業成績科排名", "學業成績科排名母數", "學業成績校排名", "學業成績校排名母數", "學業成績排名類別1", "學業成績類1排名", "學業成績類1排名母數");
            AccessHelper _AccessHelper = new AccessHelper();
            wizard.ExportPackage += delegate (object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                List<StudentRecord> students = _AccessHelper.StudentHelper.GetStudents(e.List);

           

                Dictionary<string, List<StudSemsEntryRating>> StudSemsEntryRatingDict = new Dictionary<string, List<StudSemsEntryRating>>();
                StudSemsEntryRatingDict = Utility.GetStudSemsEntryRatingByStudentID(e.List);

                _AccessHelper.StudentHelper.FillSemesterEntryScore(filterRepeat.Checked, students);
                foreach (StudentRecord stu in students)
                {
                    Dictionary<int, Dictionary<int, List<SemesterEntryScoreInfo>>> semesterEntryScoreList = new Dictionary<int, Dictionary<int, List<SemesterEntryScoreInfo>>>();
                    foreach (SemesterEntryScoreInfo var in stu.SemesterEntryScoreList)
                    {
                        if (!semesterEntryScoreList.ContainsKey(var.SchoolYear))
                            semesterEntryScoreList.Add(var.SchoolYear, new Dictionary<int, List<SemesterEntryScoreInfo>>());
                        if (!semesterEntryScoreList[var.SchoolYear].ContainsKey(var.Semester))
                            semesterEntryScoreList[var.SchoolYear].Add(var.Semester, new List<SemesterEntryScoreInfo>());
                        semesterEntryScoreList[var.SchoolYear][var.Semester].Add(var);
                    }
                    foreach (int sy in semesterEntryScoreList.Keys)
                    {
                        foreach (int se in semesterEntryScoreList[sy].Keys)
                        {
                            RowData row = new RowData();
                            row.ID = stu.StudentID;
                            row.Add("學年度", "" + sy);
                            row.Add("學期", "" + se);
                            foreach (SemesterEntryScoreInfo var in semesterEntryScoreList[sy][se])
                            {
                                if (!row.ContainsKey("成績年級"))
                                    row.Add("成績年級", "" + var.GradeYear);
                                if (e.ExportFields.Contains(var.Entry))
                                {
                                    row.Add(var.Entry, "" + var.Score);
                                }
                            }

                            //處理學業成績排名資料

                            if (StudSemsEntryRatingDict.ContainsKey(stu.StudentID))
                            {
                                foreach (var record in StudSemsEntryRatingDict[stu.StudentID])
                                {
                                    if (record.SchoolYear == "" + sy && record.Semester == "" + se)
                                    {
                                        row.Add("學業成績班排名", "" + record.ClassRank);
                                        row.Add("學業成績班排名母數", "" + record.ClassCount);
                                        row.Add("學業成績科排名", "" + record.DeptRank);
                                        row.Add("學業成績科排名母數", "" + record.DeptCount);
                                        row.Add("學業成績校排名", "" + record.YearRank);
                                        row.Add("學業成績校排名母數", "" + record.YearCount);

                                        row.Add("學業成績排名類別1", "" + record.Group1);
                                        row.Add("學業成績類1排名", "" + record.Group1Rank);
                                        row.Add("學業成績類1排名母數", "" + record.Group1Count);

                                        // 2018/8 穎驊註解，經過討論後， 先暫時將 ischool類別2 排名的概念拿掉，因為目前的結構 無法區隔類別1、類別2，待日後設計完整

                                        //row.Add("學業成績排名類別2", "" + record.Group2);
                                        //row.Add("學業成績類2排名", "" + record.Group2Rank);
                                        //row.Add("學業成績類2排名母數", "" + record.Group2Count);
                                    }

                                }                                
                            }


                            e.Items.Add(row);
                        }
                    }
                }
            };
        }


        




    }
}
