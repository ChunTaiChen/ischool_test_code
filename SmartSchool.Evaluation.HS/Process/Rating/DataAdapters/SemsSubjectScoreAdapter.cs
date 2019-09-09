using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Feature.Score;
using SmartSchool.Common;
using FISCA.DSAUtil;
using System.Xml;
using RankStudent = SmartSchool.Evaluation.Process.Rating.Student;
using SmartSchool.Feature.Score.Rating;

namespace SmartSchool.Evaluation.Process.Rating
{
    class SemsSubjectScoreAdapter : DataAdapter
    {
        /// <summary>
        /// 每次取得資料的大小。
        /// </summary>
        private const int PackageSize = 300;

        public SemsSubjectScoreAdapter(StudentCollection students, RatingParameters parameters)
            : base(students, parameters)
        {
        }

        protected override void RegisterJobs()
        {
            WeightTable.RegisterJob("GetSemsSubject", 0.10663002f);
            WeightTable.RegisterJob("SetSemsSubject", 0.222608631f);
        }

        public override bool Fill()
        {
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("GetSemsSubject"));

            try
            {
                Utility.StartTime("GetSemsSubject");
                progress.ReportMessage("取得學期科目成績資料…");

                List<List<string>> packages = Utility.SplitGetPackage(Students, PackageSize);

                int offset = 1;
                foreach (List<string> eachPackage in packages)
                {
                    if (MainProgress.Cancellation) return false; //這個部份要看的必須是 MainProgress。

                    DSXmlHelper response = QueryScore.GetSemesterSubjectScoreBySemester(false, Parameters.SchoolYear, Parameters.Semester, eachPackage.ToArray());

                    foreach (XmlElement eachStudent in response.GetElements("SemesterSubjectScore"))
                    {
                        DSXmlHelper hlpScore = new DSXmlHelper(eachStudent);
                        string studentId = hlpScore.GetText("RefStudentId");
                        string scoreId = hlpScore.GetText("@ID");

                        if (Students.ContainsKey(studentId))
                        {
                            RankStudent student = Students[studentId];
                            student.SemsSubjects.ScoreRecordIdentity = scoreId;

                            string scoresPath = "ScoreInfo/SemesterSubjectScoreInfo/Subject";
                            foreach (XmlElement eachScore in eachStudent.SelectNodes(scoresPath))
                            {
                                SemsSubjectScore objScore = new SemsSubjectScore(eachScore);
                                if (!student.SemsSubjects.AddSubject(objScore))
                                    LogDuplicateSubject(progress, student, objScore);
                            }
                        }
                    }

                    progress.ReportProgress((int)(((float)offset / packages.Count) * 100));
                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("GetSemsSubject");
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
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("SetSemsSubject"));

            try
            {
                Utility.StartTime("SetSemsSubject");
                progress.ReportMessage("更新學期科目成績排名資料…");

                List<List<Student>> packages = Utility.SplitUpdatePackage(Students, PackageSize);

                int offset = 1;
                foreach (List<Student> eachPackage in packages)
                {
                    if (MainProgress.Cancellation) return false; //這個部份要看的必須是 MainProgress。

                    DSXmlCreator request = new DSXmlCreator(false);
                    bool updateRequired = false;

                    request.CreateStartElement("Request");
                    foreach (Student eachStudent in eachPackage)
                    {
                        //代表沒有排名。
                        if (eachStudent.SemsSubjects.Count <= 0) continue;

                        updateRequired = true;
                        request.CreateStartElement("SemesterSubjectRating");
                        //學期成績記錄的編號。
                        request.CreateElement("ScoreId", eachStudent.SemsSubjects.ScoreRecordIdentity);

                        RatingScope scopeClass = new RatingScope("ClassRating");
                        RatingScope scopeDept = new RatingScope("DeptRating");
                        RatingScope scopeYear = new RatingScope("YearRating");

                        foreach (SemsSubjectScore eachScore in eachStudent.SemsSubjects.Values)
                        {
                            foreach (ResultPlace eachPlace in eachScore.RatingResults.Values)
                            {
                                if (eachPlace.Scope.ScopeType == ScopeType.Class)
                                    scopeClass.CreateRatingItem(eachScore, eachPlace);
                                else if (eachPlace.Scope.ScopeType == ScopeType.Dept)
                                    scopeDept.CreateRatingItem(eachScore, eachPlace);
                                else if (eachPlace.Scope.ScopeType == ScopeType.GradeYear)
                                    scopeYear.CreateRatingItem(eachScore, eachPlace);
                            }
                        }

                        scopeClass.FinalizeCreate();
                        scopeDept.FinalizeCreate();
                        scopeYear.FinalizeCreate();

                        request.CreateSubtree(scopeClass.GetResult());
                        request.CreateSubtree(scopeDept.GetResult());
                        request.CreateSubtree(scopeYear.GetResult());

                        request.CreateEndElement();
                    }
                    request.CreateEndElement();

                    if (updateRequired)
                        EditRating.UpdateSemesterSubjectRating(request.GetResultAsDSXmlHelper());

                    progress.ReportProgress((int)(((float)offset / packages.Count) * 100));
                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("SetSemsSubject");
                return true;
            }
            catch (Exception ex)
            {
                progress.ReportException(ex);
                return false;
            }
        }

        private static void LogDuplicateSubject(IProgressUI progressUI, RankStudent student, SemsSubjectScore objScore)
        {
            string msg = "學生「{0}(編號：{1})」的科目成績「{2}」重覆 (學期)。";

            //progressUI.LogMessage(string.Format(msg, student.Name, student.Identity, objScore.ScoreName));
        }

        class RatingScope
        {
            private string _name;
            private DSXmlCreator _data;
            private bool _has_data = false;

            public RatingScope(string name)
            {
                _name = name;
                _data = new DSXmlCreator(false);
                _data.CreateStartElement(_name);
            }

            public void CreateRatingItem(SemsSubjectScore eachScore, ResultPlace eachPlace)
            {
                if (_has_data == false)
                {
                    _data.CreateStartElement("Rating");
                    _data.CreateAttribute("範圍人數", eachPlace.RatingBase.ToString());
                }

                _has_data = true;

                _data.CreateStartElement("Item");
                _data.CreateAttribute("科目", eachScore.SubjectName);
                _data.CreateAttribute("科目級別", eachScore.Level);
                _data.CreateAttribute("成績", eachScore.Score.ToString());
                _data.CreateAttribute("排名", eachPlace.Place.ToString());
                _data.CreateAttribute("成績人數", eachPlace.ActualBase.ToString());
                _data.CreateEndElement();
            }

            public bool HasData
            {
                get { return _has_data; }
            }

            public void FinalizeCreate()
            {
                _data.CreateEndElement();

                if (HasData)
                    _data.CreateEndElement();
            }

            public XmlElement GetResult()
            {
                return _data.GetResultAsXmlDocument().DocumentElement;
            }
        }
    }
}
