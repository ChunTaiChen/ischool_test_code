using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Feature.Score;
using FISCA.DSAUtil;
using System.Xml;
using SmartSchool.Feature.Score.Rating;

namespace SmartSchool.Evaluation.Process.Rating
{
    class YearSubjectScoreAdapter : DataAdapter
    {
        /// <summary>
        /// 每次取得資料的大小。
        /// </summary>
        private const int PackageSize = 300;

        public YearSubjectScoreAdapter(StudentCollection students, RatingParameters parameters)
            : base(students, parameters)
        {
        }

        protected override void RegisterJobs()
        {
            WeightTable.RegisterJob("GetYearSubject", 0.052576471f);
            WeightTable.RegisterJob("SetYearSubject", 0.156888041f);
        }

        public override bool Fill()
        {
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("GetYearSubject"));

            try
            {
                Utility.StartTime("GetYearSubject");
                progress.ReportMessage("取得學年科目成績資料…");

                List<List<string>> packages = Utility.SplitGetPackage(Students, PackageSize);

                int offset = 1;
                foreach (List<string> eachPackage in packages)
                {
                    if (MainProgress.Cancellation) return false; //這個部份要看的必須是 MainProgress。

                    DSXmlHelper response = QueryScore.GetSchoolYearSubjectScore(true, Parameters.SchoolYear, eachPackage.ToArray());

                    foreach (XmlElement eachStudent in response.GetElements("SchoolYearSubjectScore"))
                    {
                        DSXmlHelper hlpScore = new DSXmlHelper(eachStudent);
                        string studentId = hlpScore.GetText("RefStudentId");
                        string scoreId = hlpScore.GetText("@ID");

                        if (Students.ContainsKey(studentId))
                        {
                            Student student = Students[studentId];
                            student.YearSubjects.ScoreRecordIdentity = scoreId;

                            string scoresPath = "ScoreInfo/SchoolYearSubjectScore/Subject";
                            foreach (XmlElement eachScore in eachStudent.SelectNodes(scoresPath))
                            {
                                YearSubjectScore objScore = new YearSubjectScore(eachScore);
                                if (!student.YearSubjects.AddSubject(objScore))
                                    LogDuplicateSubject(progress, student, objScore);
                            }
                        }
                    }

                    progress.ReportProgress((int)(((float)offset / packages.Count) * 100));
                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("GetYearSubject");
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
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("SetYearSubject"));

            try
            {
                Utility.StartTime("SetYearSubject");
                progress.ReportMessage("更新學年科目成績排名資料…");

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
                        if (eachStudent.YearSubjects.Count <= 0) continue;

                        updateRequired = true;
                        request.CreateStartElement("SchoolYearSubjectRating");
                        //學期成績記錄的編號。
                        request.CreateElement("ScoreId", eachStudent.YearSubjects.ScoreRecordIdentity);

                        RatingScope scopeClass = new RatingScope("ClassRating");
                        RatingScope scopeDept = new RatingScope("DeptRating");
                        RatingScope scopeYear = new RatingScope("YearRating");

                        foreach (YearSubjectScore eachScore in eachStudent.YearSubjects.Values)
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
                        EditRating.UpdateSchoolYearSubjectRating(request.GetResultAsDSXmlHelper());

                    progress.ReportProgress((int)(((float)offset / packages.Count) * 100));
                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("SetYearSubject");
                return true;
            }
            catch (Exception ex)
            {
                progress.ReportException(ex);
                return false;
            }
        }

        private static void LogDuplicateSubject(IProgressUI progressUI, Student student, YearSubjectScore objScore)
        {
            string msg = "學生「{0}(編號：{1})」的科目成績「{2}」重覆 (學年)。";

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

            public void CreateRatingItem(YearSubjectScore eachScore, ResultPlace eachPlace)
            {
                if (_has_data == false)
                {
                    _data.CreateStartElement("Rating");
                    _data.CreateAttribute("範圍人數", eachPlace.RatingBase.ToString());
                }

                _has_data = true;

                _data.CreateStartElement("Item");
                _data.CreateAttribute("科目", eachScore.SubjectName);
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
