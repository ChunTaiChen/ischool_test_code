using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Feature.Score;
using FISCA.DSAUtil;
using System.Xml;
using SmartSchool.Feature.Score.Rating;

namespace SmartSchool.Evaluation.Process.Rating
{
    class YearMoralAdapter : DataAdapter
    {
        /// <summary>
        /// 每次取得資料的大小。
        /// </summary>
        private const int PackageSize = 300;

        public YearMoralAdapter(StudentCollection students, RatingParameters parameters)
            : base(students, parameters)
        {
        }

        protected override void RegisterJobs()
        {
            WeightTable.RegisterJob("GetYearMoral", 0.039451051f);
            WeightTable.RegisterJob("SetYearMoral", 0.059288759f);
        }

        public override bool Fill()
        {
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("GetYearMoral"));

            try
            {
                Utility.StartTime("GetYearMoral");
                progress.ReportMessage("取得學年德行成績資料…");

                List<List<string>> packages = Utility.SplitGetPackage(Students, PackageSize);

                int offset = 1;
                foreach (List<string> eachPackage in packages)
                {
                    if (MainProgress.Cancellation) return false; //這個部份要看的必須是 MainProgress。

                    DSXmlHelper response = QueryScore.GetSchoolYearEntryScore(true,
                        Parameters.SchoolYear,
                        QueryScore.EntryGroup.行為,
                        eachPackage.ToArray());

                    foreach (XmlElement eachStudent in response.GetElements("SchoolYearEntryScore"))
                    {
                        DSXmlHelper hlpScore = new DSXmlHelper(eachStudent);
                        string studentId = hlpScore.GetText("RefStudentId");
                        string scoreId = hlpScore.GetText("@ID");

                        if (Students.ContainsKey(studentId))
                        {
                            Student student = Students[studentId];
                            student.YearMoral = new SchoolYearEntry("德行", eachStudent);
                        }
                    }

                    progress.ReportProgress((int)(((float)offset / packages.Count) * 100));
                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("GetYearMoral");
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
            SubProgress progress = new SubProgress(MainProgress, WeightTable.GetJobWeight("SetYearMoral"));

            try
            {
                Utility.StartTime("SetYearMoral");
                progress.ReportMessage("更新學年德行成績排名資料…");

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
                        if (eachStudent.YearMoral == null) continue;

                        updateRequired = true;
                        request.CreateStartElement("SchoolYearEntryRating");
                        //學期成績記錄的編號。
                        request.CreateElement("ScoreId", eachStudent.YearMoral.ScoreRecordIdentity);

                        RatingScope scopeClass = new RatingScope("ClassRating");
                        RatingScope scopeDept = new RatingScope("DeptRating");
                        RatingScope scopeYear = new RatingScope("YearRating");

                        foreach (ResultPlace eachPlace in eachStudent.YearMoral.RatingResults.Values)
                        {
                            if (eachPlace.Scope.ScopeType == ScopeType.Class)
                                scopeClass.CreateRatingItem(eachStudent.YearMoral, eachPlace);
                            else if (eachPlace.Scope.ScopeType == ScopeType.Dept)
                                scopeDept.CreateRatingItem(eachStudent.YearMoral, eachPlace);
                            else if (eachPlace.Scope.ScopeType == ScopeType.GradeYear)
                                scopeYear.CreateRatingItem(eachStudent.YearMoral, eachPlace);
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
                        EditRating.UpdateSchoolYearEntryRating(request.GetResultAsDSXmlHelper());

                    progress.ReportProgress((int)(((float)offset / packages.Count) * 100));
                    offset++;
                }

                progress.ReportProgress(100);
                Utility.EndTime("SetYearMoral");
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
