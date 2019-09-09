using System;
using System.Collections.Generic;
using System.Text;
using FISCA.DSAUtil;
using SmartSchool.Feature;
using System.Xml;
using RankStudent = SmartSchool.Evaluation.Process.Rating.Student;

namespace SmartSchool.Evaluation.Process.Rating
{
    class StudentAdapter : DataAdapter
    {
        public StudentAdapter(StudentCollection emptyContainer, RatingParameters parameters)
            : base(emptyContainer, parameters)
        {
        }

        protected override void RegisterJobs()
        {
            WeightTable.RegisterJob("GetStudentData", 0.014901653f);
        }

        public override bool Fill()
        {
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("GetStudentData"));
            try
            {
                Utility.StartTime("GetStudentData");
                progress.ReportMessage("取得學生資料…");

                int offset = 1;
                foreach (string eachYear in Parameters.TargetGradeYears)
                {
                    if (MainProgress.Cancellation) //這個部份必須要看的是 MainProgress。
                        return false;

                    DSXmlHelper response = QueryStudent.GetAbstractList(eachYear);

                    foreach (XmlElement eachStudent in response.GetElements("Student"))
                        Students.AddStudent(new RankStudent(eachStudent));

                    progress.ReportProgress((int)(((float)offset / Parameters.TargetGradeYears.Count) * 100));

                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("GetStudentData");
                return true;
            }
            catch (Exception ex)
            {
                progress.ReportException(ex);
                return false;
            }
        }

        public override bool Update()
        {
            return true;
        }
    }
}
