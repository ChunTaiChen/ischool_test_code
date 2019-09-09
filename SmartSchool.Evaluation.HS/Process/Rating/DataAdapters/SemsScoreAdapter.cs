using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Feature.Score;
using FISCA.DSAUtil;
using System.Xml;
using SmartSchool.Feature.Score.Rating;

namespace SmartSchool.Evaluation.Process.Rating
{
    class SemsScoreAdapter : DataAdapter
    {
        /// <summary>
        /// 每次取得資料的大小。
        /// </summary>
        private const int PackageSize = 300;

        public SemsScoreAdapter(StudentCollection students, RatingParameters parameters)
            : base(students, parameters)
        {
        }

        protected override void RegisterJobs()
        {
            WeightTable.RegisterJob("GetSemsScore", 0.040909431f);
            WeightTable.RegisterJob("SetSemsScore", 0.084997382f);
        }

        public override bool Fill()
        {
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("GetSemsScore"));

            try
            {
                Utility.StartTime("GetSemsScore");
                progress.ReportMessage("取得學期分項成績資料…");

                List<List<string>> packages = Utility.SplitGetPackage(Students, PackageSize);

                int offset = 1;
                foreach (List<string> eachPackage in packages)
                {
                    if (MainProgress.Cancellation) return false; //這個部份要看的必須是 MainProgress。

                    DSXmlHelper response = QueryScore.GetSemesterEntryScoreBySemester(true,
                        Parameters.SchoolYear,
                        Parameters.Semester,
                        QueryScore.EntryGroup.學習,
                        eachPackage.ToArray());

                    foreach (XmlElement eachStudent in response.GetElements("SemesterEntryScore"))
                    {
                        DSXmlHelper hlpScore = new DSXmlHelper(eachStudent);
                        string studentId = hlpScore.GetText("RefStudentId");
                        string scoreId = hlpScore.GetText("@ID");

                        if (Students.ContainsKey(studentId))
                        {
                            Student student = Students[studentId];
                            student.SemsScore = new SemesterEntry("學業", eachStudent);
                        }
                    }

                    progress.ReportProgress((int)(((float)offset / packages.Count) * 100));
                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("GetSemsScore");
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
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("SetSemsScore"));

            try
            {
                Utility.StartTime("SetSemsScore");
                progress.ReportMessage("更新學期分項成績排名資料…");

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
                        if (eachStudent.SemsScore == null) continue;

                        updateRequired = true;
                        request.CreateStartElement("SemesterEntryRating");
                        //學期成績記錄的編號。
                        request.CreateElement("ScoreId", eachStudent.SemsScore.ScoreRecordIdentity);

                        RatingScope scopeClass = new RatingScope("ClassRating");
                        RatingScope scopeDept = new RatingScope("DeptRating");
                        RatingScope scopeYear = new RatingScope("YearRating");

                        foreach (ResultPlace eachPlace in eachStudent.SemsScore.RatingResults.Values)
                        {
                            if (eachPlace.Scope.ScopeType == ScopeType.Class)
                                scopeClass.CreateRatingItem(eachStudent.SemsScore, eachPlace);
                            else if (eachPlace.Scope.ScopeType == ScopeType.Dept)
                                scopeDept.CreateRatingItem(eachStudent.SemsScore, eachPlace);
                            else if (eachPlace.Scope.ScopeType == ScopeType.GradeYear)
                                scopeYear.CreateRatingItem(eachStudent.SemsScore, eachPlace);
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
                        EditRating.UpdateSemesterEntryRating(request.GetResultAsDSXmlHelper());

                    progress.ReportProgress((int)(((float)offset / packages.Count) * 100));
                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("SetSemsScore");
                return true;
            }
            catch (Exception ex)
            {
                progress.ReportException(ex);
                return false;
            }
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

            public void CreateRatingItem(EntryScore eachScore, ResultPlace eachPlace)
            {
                if (_has_data == false)
                {
                    _data.CreateStartElement("Rating");
                    _data.CreateAttribute("範圍人數", eachPlace.RatingBase.ToString());
                }

                _has_data = true;

                _data.CreateStartElement("Item");
                _data.CreateAttribute("分項", eachScore.ScoreName);
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
