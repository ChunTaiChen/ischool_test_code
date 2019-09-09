using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Customization.PlugIn.ImportExport;
using SmartSchool.Feature.Score;

namespace SmartSchool.Evaluation.ImportExport
{
    class ImportSemesterEntryScoreRank : ImportProcess
    {
        private AccessHelper _AccessHelper;

        private List<string> _ID_SchoolYear_Semester = new List<string>();

        private Dictionary<string, StudentRecord> _StudentCollection = new Dictionary<string, StudentRecord>();

        public ImportSemesterEntryScoreRank()
        {
            this.Image = null;
            this.Title = "匯入學期分項成績";
            this.Group = "學期分項成績";
            this.PackageLimit = 500;
            //foreach (string field in new string[] { "學年度"
            //    , "學期"
            //    , "成績年級"
            //    , "學業"
            //    , "體育"
            //    , "國防通識"
            //    , "健康與護理"
            //    , "實習科目"
            //    , "專業科目"
            //    , "學業(原始)"
            //    , "體育(原始)"
            //    , "國防通識(原始)"
            //    , "健康與護理(原始)"
            //    , "實習科目(原始)"
            //    , "專業科目(原始)"
            //    , "德行"
            //    , "學業成績班排名"
            //    , "學業成績班排名母數"
            //    , "學業成績科排名"
            //    , "學業成績科排名母數"
            //    , "學業成績校排名"
            //    , "學業成績校排名母數"
            //    , "學業成績排名類別1"
            //    , "學業成績類1排名"
            //    , "學業成績類1排名母數"
            //    , "學業成績排名類別2"
            //    , "學業成績類2排名"
            //    , "學業成績類2排名母數"})

            //{
            //    this.ImportableFields.Add(field);
            //}

            // 2018/8 穎驊註解，經過討論後， 先暫時將 ischool類別2 排名的概念拿掉，因為目前的結構 無法區隔類別1、類別2，待日後設計完整
            foreach (string field in new string[] { "學年度"
                , "學期"
                , "成績年級"
                , "學業"
                , "體育"
                , "國防通識"
                , "健康與護理"
                , "實習科目"
                , "專業科目"
                , "學業(原始)"
                , "體育(原始)"
                , "國防通識(原始)"
                , "健康與護理(原始)"
                , "實習科目(原始)"
                , "專業科目(原始)"
                , "德行"
                , "學業成績班排名"
                , "學業成績班排名母數"
                , "學業成績科排名"
                , "學業成績科排名母數"
                , "學業成績校排名"
                , "學業成績校排名母數"
                , "學業成績排名類別1"
                , "學業成績類1排名"
                , "學業成績類1排名母數"
                })

            {
                this.ImportableFields.Add(field);
            }

            foreach (string field in new string[] { "學年度", "學期" })
            {
                this.RequiredFields.Add(field);
            }
            this.BeginValidate += new EventHandler<BeginValidateEventArgs>(ImportSemesterEntryScore_BeginValidate);
            this.RowDataValidated += new EventHandler<RowDataValidatedEventArgs>(ImportSemesterEntryScore_RowDataValidated);
            this.DataImport += new EventHandler<DataImportEventArgs>(ImportSemesterEntryScore_DataImport);
            this.EndImport += new EventHandler(ImportSemesterEntryScore_EndImport);
        }

        private void ImportSemesterEntryScore_BeginValidate(object sender, BeginValidateEventArgs e)
        {
            _ID_SchoolYear_Semester.Clear();
            _StudentCollection.Clear();


            _AccessHelper = new AccessHelper();
            List<StudentRecord> list = _AccessHelper.StudentHelper.GetStudents(e.List);
            List<List<StudentRecord>> loadPackages = new List<List<StudentRecord>>();
            List<List<StudentRecord>> loadPackages2 = new List<List<StudentRecord>>();
            {
                List<StudentRecord> package = null;
                int count = 0;
                foreach (StudentRecord var in list)
                {
                    if (count == 0)
                    {
                        package = new List<StudentRecord>(250);
                        count = 250;
                        if ((loadPackages.Count & 1) == 0)
                            loadPackages.Add(package);
                        else
                            loadPackages2.Add(package);
                    }
                    package.Add(var);
                    count--;
                }
            }
            Thread threadLoadSemesterSubjectScore = new Thread(new ParameterizedThreadStart(loadSemesterEntryScore));
            threadLoadSemesterSubjectScore.IsBackground = true;
            threadLoadSemesterSubjectScore.Start(loadPackages);
            Thread threadLoadSemesterSubjectScore2 = new Thread(new ParameterizedThreadStart(loadSemesterEntryScore));
            threadLoadSemesterSubjectScore2.IsBackground = true;
            threadLoadSemesterSubjectScore2.Start(loadPackages2);

            threadLoadSemesterSubjectScore.Join();
            threadLoadSemesterSubjectScore2.Join();


            foreach (StudentRecord stu in list)
            {
                if (!_StudentCollection.ContainsKey(stu.StudentID))
                    _StudentCollection.Add(stu.StudentID, stu);
            }
        }

        private void ImportSemesterEntryScore_RowDataValidated(object sender, RowDataValidatedEventArgs e)
        {
            int t;
            decimal d;
            StudentRecord student;
            if (_StudentCollection.ContainsKey(e.Data.ID))
            {
                student = _StudentCollection[e.Data.ID];
            }
            else
            {
                e.ErrorMessage = "壓根就沒有這個學生" + e.Data.ID;
                return;
            }
            bool inputFormatPass = true;
            #region 驗各欄位填寫格式
            foreach (string field in e.SelectFields)
            {
                string value = e.Data[field];
                switch (field)
                {
                    default:
                        break;
                    case "學年度":
                    case "學分數":
                    case "成績年級":
                        if (value == "" || !int.TryParse(value, out t))
                        {
                            inputFormatPass &= false;
                            e.ErrorFields.Add(field, "必須填入整數");
                        }
                        break;
                    case "學期":
                        if (value == "" || !int.TryParse(value, out t) || t > 2 || t < 1)
                        {
                            inputFormatPass &= false;
                            e.ErrorFields.Add(field, "必須填入1或2");
                        }
                        break;
                    case "學業成績班排名母數":
                    case "學業成績科排名母數":
                    case "學業成績校排名母數":
                    case "學業成績類1排名母數":
                        if (!e.SelectFields.Contains(field.Replace("母數", "")))
                        {
                            inputFormatPass &= false;
                            e.ErrorFields.Add(field, "無法單獨匯入排名母數而不匯入名次。");
                        }
                        else
                        {
                            if (value !="" &&!int.TryParse(value, out t))
                            {
                                inputFormatPass &= false;
                                if (!e.ErrorFields.ContainsKey(field))
                                {
                                    e.ErrorFields.Add(field, "必須填入整數");
                                }
                                else
                                {
                                    e.ErrorFields[field] += "必須填入整數";
                                }
                            }
                        }
                         
                        break;
                    case "學業成績班排名":
                    case "學業成績科排名":
                    case "學業成績校排名":
                        if (value == "") { }
                        else
                        {
                            if (!int.TryParse(value, out t))
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入整數");
                            }
                            if (!e.SelectFields.Contains(field + "母數"))
                            {
                                inputFormatPass &= false;
                                if (!e.ErrorFields.ContainsKey(field))
                                {
                                    e.ErrorFields.Add(field, "匯入排名時應同時匯入母數");
                                }
                                else
                                {
                                    e.ErrorFields[field] += "匯入排名時應同時匯入母數";
                                }                                
                            }
                        }
                        break;
                    case "學業成績類1排名":
                        if (value == "") { }
                        else
                        {
                            if (!int.TryParse(value, out t))
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入整數");
                            }
                            
                            if (!e.SelectFields.Contains(field + "母數"))
                            {
                                inputFormatPass &= false;
                                if (!e.ErrorFields.ContainsKey(field))
                                {
                                    e.ErrorFields.Add(field, "匯入排名時應同時匯入母數");
                                }
                                else
                                {
                                    e.ErrorFields[field] += "匯入排名時應同時匯入母數";
                                }                                
                            }
                            if (!e.SelectFields.Contains("學業成績排名類別1"))
                            {
                                inputFormatPass &= false;                                
                                if (!e.ErrorFields.ContainsKey(field))
                                {
                                    e.ErrorFields.Add(field, "匯入排名時應同時匯入學業成績排名類別1");
                                }
                                else
                                {
                                    e.ErrorFields[field] += "匯入排名時應同時匯入學業成績排名類別1";
                                }
                            }
                            //e.WarningFields.Add(field, "類組排名將會匯入至類別排名1的位置，如果原有類別排名2的排名資料將會遺失。");
                        }
                        break;
                    case "學業成績排名類別1":
                        if (value == "") { }
                        else
                        {
                            if (!e.SelectFields.Contains("學業成績類1排名母數"))
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "匯入排名時應同時匯入母數。");
                            }

                            if (!e.SelectFields.Contains("學業成績類1排名"))
                            {
                                inputFormatPass &= false;
                                if (!e.ErrorFields.ContainsKey(field))
                                {
                                    e.ErrorFields.Add(field, "匯入排名類別1時應同時匯入學業成績類1排名。");
                                }
                                else
                                {
                                    e.ErrorFields[field] += "匯入排名類別1時應同時匯入學業成績類1排名。";
                                }                                
                            }                            
                        }
                        break;
                    case "學業":
                    case "體育":
                    case "國防通識":
                    case "健康與護理":
                    case "實習科目":
                    case "專業科目":
                    case "學業(原始)":
                    case "體育(原始)":
                    case "國防通識(原始)":
                    case "健康與護理(原始)":
                    case "實習科目(原始)":
                    case "專業科目(原始)":
                    case "德行":
                        if (value != "" && !decimal.TryParse(value, out d))
                        {
                            inputFormatPass &= false;
                            e.ErrorFields.Add(field, "必須填入空白或數值");
                        }
                        break;
                }
            }
            #endregion
            if (inputFormatPass)
            {
                string schoolYear = e.Data["學年度"];
                string semester = e.Data["學期"];
                int sy = int.Parse(schoolYear);
                int se = int.Parse(semester);
                #region 驗證資料情節
                #region 驗證新增成績學期
                bool isNewSubjectInfo = true;
                string message = "";
                foreach (SemesterEntryScoreInfo info in student.SemesterEntryScoreList)
                {
                    if (info.SchoolYear == sy && info.Semester == se)
                    {
                        isNewSubjectInfo = false;
                    }
                }
                if (isNewSubjectInfo)
                {
                    if (!e.WarningFields.ContainsKey("查無此科目"))
                        e.WarningFields.Add("查無此科目", "學生在此學期分項成績資訊，將會新增分項成績");
                    if (!e.SelectFields.Contains("成績年級"))
                    {
                        message = "發現此學期無分項成績，\n將會新增學期分項成績\n缺少成績必要欄位：成績年級";
                    }
                    if (message != "")
                        e.ErrorMessage += (e.ErrorMessage == "" ? "" : "\n") + message;
                }
                #endregion
                #region 驗證重複學期資料
                string key = e.Data.ID + "_" + sy + "_" + se;
                if (_ID_SchoolYear_Semester.Contains(key))
                {
                    e.ErrorMessage += (e.ErrorMessage == "" ? "" : "\n") + "同一學期不允許多筆資料";
                }
                else
                    _ID_SchoolYear_Semester.Add(key);
                #endregion
                #region 驗證成績年級變更
                if (e.SelectFields.Contains("成績年級"))
                {
                    int gy = int.Parse(e.Data["成績年級"]);
                    foreach (SemesterEntryScoreInfo info in student.SemesterEntryScoreList)
                    {
                        if (info.SchoolYear == sy && info.Semester == se && info.GradeYear != gy)
                        {
                            if (!e.WarningFields.ContainsKey("成績年級"))
                                e.WarningFields.Add("成績年級", "修改成績年級資訊將會改變此學生在該學期的所有分項成績的成績年級");
                        }
                    }
                }
                #endregion
                #endregion
            }
        }

        private void loadSemesterEntryScore(object item)
        {
            List<List<StudentRecord>> Packages = (List<List<StudentRecord>>)item;
            foreach (List<StudentRecord> package in Packages)
            {
                _AccessHelper.StudentHelper.FillSemesterEntryScore(false, package);
            }
        }

        private void ImportSemesterEntryScore_DataImport(object sender, DataImportEventArgs e)
        {
            Dictionary<string, List<RowData>> id_Rows = new Dictionary<string, List<RowData>>();
            #region 分包裝
            foreach (RowData data in e.Items)
            {
                if (!id_Rows.ContainsKey(data.ID))
                    id_Rows.Add(data.ID, new List<RowData>());
                id_Rows[data.ID].Add(data);
            }
            #endregion
            List<SmartSchool.Feature.Score.AddScore.InsertInfo> insertList = new List<SmartSchool.Feature.Score.AddScore.InsertInfo>();
            List<SmartSchool.Feature.Score.EditScore.UpdateInfo> updateList = new List<SmartSchool.Feature.Score.EditScore.UpdateInfo>();
            List<string> deleteList = new List<string>();
            XmlDocument doc = new XmlDocument();

            List<string> studentIDList = new List<string>();

            foreach (string id in id_Rows.Keys)
            {
                studentIDList.Add(id);
            }

            // 讀取各學業排名

            Dictionary<string, List<StudSemsEntryRating>> StudSemsEntryRatingDict = Utility.GetStudSemsEntryRatingByStudentID(studentIDList);

            List<string> sql = new List<string>();
            //交叉比對各學生資料
            #region 交叉比對各學生資料
            foreach (string id in id_Rows.Keys)
            {
                StudentRecord studentRec = _StudentCollection[id];
                Dictionary<int, Dictionary<int, Dictionary<string, string>>> scoreIDDictionary = (Dictionary<int, Dictionary<int, Dictionary<string, string>>>)studentRec.Fields["SemesterEntryScoreID"];
                //該學生的學期各分項成績
                Dictionary<int, Dictionary<int, Dictionary<string, SemesterEntryScoreInfo>>> semesterScoreDictionary = new Dictionary<int, Dictionary<int, Dictionary<string, SemesterEntryScoreInfo>>>();
                Dictionary<int, Dictionary<int, int>> semesterGradeYearDictionary = new Dictionary<int, Dictionary<int, int>>();
                #region 整理現有的成績資料
                foreach (SemesterEntryScoreInfo var in studentRec.SemesterEntryScoreList)
                {
                    //學期分項成績資料
                    if (!semesterScoreDictionary.ContainsKey(var.SchoolYear))
                        semesterScoreDictionary.Add(var.SchoolYear, new Dictionary<int, Dictionary<string, SemesterEntryScoreInfo>>());
                    if (!semesterScoreDictionary[var.SchoolYear].ContainsKey(var.Semester))
                        semesterScoreDictionary[var.SchoolYear].Add(var.Semester, new Dictionary<string, SemesterEntryScoreInfo>());
                    semesterScoreDictionary[var.SchoolYear][var.Semester].Add(var.Entry, var);
                    //學期成績年級資料
                    if (!semesterGradeYearDictionary.ContainsKey(var.SchoolYear))
                        semesterGradeYearDictionary.Add(var.SchoolYear, new Dictionary<int, int>());
                    if (!semesterGradeYearDictionary[var.SchoolYear].ContainsKey(var.Semester))
                        semesterGradeYearDictionary[var.SchoolYear].Add(var.Semester, var.GradeYear);
                }
                #endregion
                //匯入資訊中各學期各分項成績值
                Dictionary<int, Dictionary<int, Dictionary<string, decimal?>>> semesterImportScore = new Dictionary<int, Dictionary<int, Dictionary<string, decimal?>>>();
                #region 整理要匯入的資料
                foreach (RowData row in id_Rows[id])
                {
                    int t;
                    string schoolYear = row["學年度"];
                    string semester = row["學期"];
                    int sy = int.Parse(schoolYear);
                    int se = int.Parse(semester);
                    foreach (string key in row.Keys)
                    {
                        bool gyChanged = false;
                        if (e.ImportFields.Contains("成績年級") && key == "成績年級")
                        {
                            int gy = int.Parse(row["成績年級"]);
                            //學期成績年級資料
                            if (!semesterGradeYearDictionary.ContainsKey(sy))
                                semesterGradeYearDictionary.Add(sy, new Dictionary<int, int>());
                            if (!semesterGradeYearDictionary[sy].ContainsKey(se))
                            {
                                semesterGradeYearDictionary[sy].Add(se, gy);
                                gyChanged = true;
                            }
                            else
                            {
                                if (semesterGradeYearDictionary[sy][se] != gy)
                                {
                                    semesterGradeYearDictionary[sy][se] = gy;
                                    gyChanged = true;
                                }
                            }
                        }
                        if (gyChanged)//成績年級有變更就一定要改
                        {
                            if (!semesterImportScore.ContainsKey(sy))
                                semesterImportScore.Add(sy, new Dictionary<int, Dictionary<string, decimal?>>());
                            if (!semesterImportScore[sy].ContainsKey(se))
                                semesterImportScore[sy].Add(se, new Dictionary<string, decimal?>());
                        }
                        if (e.ImportFields.Contains(key) && key == "學業"
                            || key == "體育"
                            || key == "國防通識"
                            || key == "健康與護理"
                            || key == "實習科目"
                            || key == "專業科目"
                            || key == "學業(原始)"
                            || key == "體育(原始)"
                            || key == "國防通識(原始)"
                            || key == "健康與護理(原始)"
                            || key == "實習科目(原始)"
                            || key == "專業科目(原始)"
                            || key == "德行")
                        {
                            bool import = true;
                            decimal? score = null;
                            decimal d;
                            if (decimal.TryParse(row[key], out d))
                                score = decimal.Parse(row[key]);
                            //如果這學期有這筆分項成績
                            if (semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey(key))
                            {
                                //成績相同
                                if (semesterScoreDictionary[sy][se][key].Score == score)
                                    import = false;
                            }
                            else
                            {
                                //原本沒有此分項成績，是匯入的也沒有成績
                                if (score == null)
                                    import = false;
                            }
                            if (import)//成績年級有變更就一定要改
                            {
                                if (!semesterImportScore.ContainsKey(sy))
                                    semesterImportScore.Add(sy, new Dictionary<int, Dictionary<string, decimal?>>());
                                if (!semesterImportScore[sy].ContainsKey(se))
                                    semesterImportScore[sy].Add(se, new Dictionary<string, decimal?>());
                                if (!semesterImportScore[sy][se].ContainsKey(key))
                                    semesterImportScore[sy][se].Add(key, score);
                            }
                        }

                        #region 排名
                        bool hasclassRating = false;
                        bool hasdeptRating = false;
                        bool hasschoolRating = false;
                        bool hastag1Rating = false;
                        bool hastag2Rating = false;

                        bool clearClassRating = false;
                        bool clearDeptRating = false;
                        bool clearSchoolRating = false;
                        bool clearTag1Raging = false;
                        bool clearTag2Raging = false;

                        var classRating = doc.CreateElement("Rating"); classRating.SetAttribute("範圍人數", "0");
                        var deptRating = doc.CreateElement("Rating"); deptRating.SetAttribute("範圍人數", "0");
                        var schoolRating = doc.CreateElement("Rating"); schoolRating.SetAttribute("範圍人數", "0");

                        // 以前是固定 "類組" 排名
                        //var tag1Rating = doc.CreateElement("Rating"); tag1Rating.SetAttribute("範圍人數", "0"); tag1Rating.SetAttribute("類別", "類組");

                        //類別1 排名
                        var tag1Rating = doc.CreateElement("Rating"); tag1Rating.SetAttribute("範圍人數", "0"); tag1Rating.SetAttribute("類別", "");

                        // 2018/8 穎驊註解，經過討論後， 先暫時將 ischool類別2 排名的概念拿掉，因為目前的結構 無法區隔類別1、類別2，待日後設計完整
                        ////類別2 排名
                        //var tag2Rating = doc.CreateElement("Rating"); tag2Rating.SetAttribute("範圍人數", "0"); tag2Rating.SetAttribute("類別", "");

                        // 整理舊資料
                        if (StudSemsEntryRatingDict.ContainsKey(id))
                        {
                            foreach (StudSemsEntryRating sser in StudSemsEntryRatingDict[id])
                            {
                                if (sser.SchoolYear == "" + sy && sser.Semester == "" + se)
                                {
                                    // 類1
                                    if (sser.Group1 != "")
                                    {
                                        var item = doc.CreateElement("Item");
                                        item.SetAttribute("成績", "" + sser.Score);
                                        item.SetAttribute("成績人數", "" + sser.Group1Count);
                                        item.SetAttribute("排名", "" + sser.Group1Rank);
                                        item.SetAttribute("分項", "學業");

                                        tag1Rating.AppendChild(item);

                                        tag1Rating.SetAttribute("範圍人數", "" + sser.Group1Count);
                                        tag1Rating.SetAttribute("類別", "" + sser.Group1);
                                    }
                                    //// 類2
                                    //if (sser.Group2 != "")
                                    //{
                                    //    var item = doc.CreateElement("Item");
                                    //    item.SetAttribute("成績", "" + sser.Score);
                                    //    item.SetAttribute("成績人數", "" + sser.Group2Count);
                                    //    item.SetAttribute("排名", "" + sser.Group2Rank);
                                    //    item.SetAttribute("分項", "學業");

                                    //    tag2Rating.AppendChild(item);

                                    //    tag2Rating.SetAttribute("範圍人數", "" + sser.Group2Count);
                                    //    tag2Rating.SetAttribute("類別", "" + sser.Group2);
                                    //}
                                }
                            }
                        }


                        if (e.ImportFields.Contains("學業成績班排名"))
                        {
                            hasclassRating = true;
                            if (row["學業成績班排名"] == "")
                            {
                                clearClassRating = true;
                            }
                            else
                            {
                                var item = doc.CreateElement("Item");
                                item.SetAttribute("成績", "" + (e.ImportFields.Contains("學業") ? row["學業"] : ((semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey("學業")) ? semesterScoreDictionary[sy][se]["學業"].Score.ToString() : "0")));
                                if (e.ImportFields.Contains("學業成績班排名母數"))
                                {
                                    item.SetAttribute("成績人數", "" + row["學業成績班排名母數"]);
                                    classRating.SetAttribute("範圍人數", "" + row["學業成績班排名母數"]);
                                }
                                else
                                {
                                    item.SetAttribute("成績人數", "0");
                                    classRating.SetAttribute("範圍人數", "0");
                                }
                                item.SetAttribute("排名", "" + row["學業成績班排名"]);
                                item.SetAttribute("分項", "學業");
                                classRating.AppendChild(item);
                            }
                        }
                        if (e.ImportFields.Contains("學業成績科排名"))
                        {
                            hasdeptRating = true;
                            if (row["學業成績科排名"] == "")
                            {
                                clearDeptRating = true;
                            }
                            else
                            {
                                var item = doc.CreateElement("Item");
                                item.SetAttribute("成績", "" + (e.ImportFields.Contains("學業") ? row["學業"] : ((semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey("學業")) ? semesterScoreDictionary[sy][se]["學業"].Score.ToString() : "0")));
                                if (e.ImportFields.Contains("學業成績科排名母數"))
                                {
                                    item.SetAttribute("成績人數", "" + row["學業成績科排名母數"]);
                                    deptRating.SetAttribute("範圍人數", "" + row["學業成績科排名母數"]);
                                }
                                else
                                {
                                    item.SetAttribute("成績人數", "0");
                                    deptRating.SetAttribute("範圍人數", "0");
                                }
                                item.SetAttribute("排名", "" + row["學業成績科排名"]);
                                item.SetAttribute("分項", "學業");
                                deptRating.AppendChild(item);
                            }
                        }
                        if (e.ImportFields.Contains("學業成績校排名"))
                        {
                            hasschoolRating = true;
                            if (row["學業成績校排名"] == "")
                            {
                                clearSchoolRating = true;
                            }
                            else
                            {
                                var item = doc.CreateElement("Item");
                                item.SetAttribute("成績", "" + (e.ImportFields.Contains("學業") ? row["學業"] : ((semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey("學業")) ? semesterScoreDictionary[sy][se]["學業"].Score.ToString() : "0")));
                                if (e.ImportFields.Contains("學業成績校排名母數"))
                                {
                                    item.SetAttribute("成績人數", "" + row["學業成績校排名母數"]);
                                    schoolRating.SetAttribute("範圍人數", "" + row["學業成績校排名母數"]);
                                }
                                else
                                {
                                    item.SetAttribute("成績人數", "0");
                                    schoolRating.SetAttribute("範圍人數", "0");
                                }
                                item.SetAttribute("排名", "" + row["學業成績校排名"]);
                                item.SetAttribute("分項", "學業");
                                schoolRating.AppendChild(item);
                            }
                        }
                        // 類別1 排名匯入
                        if (e.ImportFields.Contains("學業成績類1排名"))
                        {
                            tag1Rating.RemoveAll();

                            hastag1Rating = true;
                            if (row["學業成績類1排名"] == "")
                            {
                                clearTag1Raging = true;
                            }
                            else
                            {
                                var item = doc.CreateElement("Item");
                                item.SetAttribute("成績", "" + (e.ImportFields.Contains("學業") ? row["學業"] : ((semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey("學業")) ? semesterScoreDictionary[sy][se]["學業"].Score.ToString() : "0")));
                                if (e.ImportFields.Contains("學業成績類1排名母數"))
                                {
                                    item.SetAttribute("成績人數", "" + row["學業成績類1排名母數"]);
                                    tag1Rating.SetAttribute("範圍人數", "" + row["學業成績類1排名母數"]);
                                }
                                else
                                {
                                    item.SetAttribute("成績人數", "0");
                                    tag1Rating.SetAttribute("範圍人數", "0");
                                }
                                item.SetAttribute("排名", "" + row["學業成績類1排名"]);
                                item.SetAttribute("分項", "學業");
                                tag1Rating.AppendChild(item);

                                tag1Rating.SetAttribute("類別", "" + row["學業成績排名類別1"]);
                            }
                        }

                        // 類別2 排名匯入
                        //if (e.ImportFields.Contains("學業成績類2排名"))
                        //{
                        //    tag2Rating.RemoveAll();

                        //    hastag2Rating = true;
                        //    if (row["學業成績類2排名"] == "")
                        //    {
                        //        clearTag2Raging = true;
                        //    }
                        //    else
                        //    {


                        //        var item = doc.CreateElement("Item");
                        //        item.SetAttribute("成績", "" + (e.ImportFields.Contains("學業") ? row["學業"] : ((semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey("學業")) ? semesterScoreDictionary[sy][se]["學業"].Score.ToString() : "0")));
                        //        if (e.ImportFields.Contains("學業成績類2排名母數"))
                        //        {
                        //            item.SetAttribute("成績人數", "" + row["學業成績類2排名母數"]);
                        //            tag2Rating.SetAttribute("範圍人數", "" + row["學業成績類2排名母數"]);
                        //        }
                        //        else
                        //        {
                        //            item.SetAttribute("成績人數", "0");
                        //            tag2Rating.SetAttribute("範圍人數", "0");
                        //        }
                        //        item.SetAttribute("排名", "" + row["學業成績類2排名"]);
                        //        item.SetAttribute("分項", "學業");
                        //        tag2Rating.AppendChild(item);

                        //        tag2Rating.SetAttribute("類別", "" + row["學業成績排名類別2"]);
                        //    }
                        //}
                        if (hasclassRating || hasdeptRating || hasschoolRating || hastag1Rating || hastag2Rating)
                        {
                            string updateSql = "update sems_entry_score set ";
                            if (hasclassRating)
                            {
                                if (updateSql != "update sems_entry_score set ")
                                {
                                    updateSql += ",";
                                }
                                if (clearClassRating)
                                    updateSql += "class_rating=null";
                                else
                                    updateSql += "class_rating='" + classRating.OuterXml + "'";
                            }
                            if (hasdeptRating)
                            {
                                if (updateSql != "update sems_entry_score set ")
                                {
                                    updateSql += ",";
                                }
                                if (clearDeptRating)
                                    updateSql += "dept_rating=null";
                                else
                                    updateSql += "dept_rating='" + deptRating.OuterXml + "'";
                            }
                            if (hasschoolRating)
                            {
                                if (updateSql != "update sems_entry_score set ")
                                {
                                    updateSql += ",";
                                }
                                if (clearSchoolRating)
                                    updateSql += "year_rating=null";
                                else
                                    updateSql += "year_rating='" + schoolRating.OuterXml + "'";
                            }


                            // 2018/8 穎驊註解，經過討論後， 先暫時將 ischool類別2 排名的概念拿掉，因為目前的結構 無法區隔類別1、類別2，待日後設計完整

                            //// 僅匯入類1排名， 若類2排名有資料則保留
                            //if (hastag1Rating && !hastag2Rating)
                            //{
                            //    if (updateSql != "update sems_entry_score set ")
                            //    {
                            //        updateSql += ",";
                            //    }
                            //    if (clearTag1Raging) //清除類1 保留類2
                            //        updateSql += "group_rating='<Ratings>" + tag2Rating.OuterXml + "</Ratings>'";
                            //    else
                            //        updateSql += "group_rating='<Ratings>" + tag1Rating.OuterXml + tag2Rating.OuterXml + "</Ratings>'";
                            //}


                            //// 僅匯入類2排名， 若類1排名有資料則保留
                            //if (hastag2Rating && !hastag1Rating)
                            //{
                            //    if (updateSql != "update sems_entry_score set ")
                            //    {
                            //        updateSql += ",";
                            //    }
                            //    if (clearTag2Raging) // 清除類2，保留類1
                            //        updateSql += "group_rating='<Ratings>" + tag1Rating.OuterXml + "</Ratings>'";
                            //    else
                            //        updateSql += "group_rating='<Ratings>" + tag1Rating.OuterXml + tag2Rating.OuterXml + "</Ratings>'";
                            //}

                            //// 同時匯入類1、類2 排名
                            //if (hastag1Rating && hastag2Rating)
                            //{
                            //    if (updateSql != "update sems_entry_score set ")
                            //    {
                            //        updateSql += ",";
                            //    }
                            //    if (clearTag1Raging && clearTag2Raging) // 清除類1 類2
                            //    {
                            //        updateSql += "group_rating=null";
                            //    }
                            //    else if (clearTag1Raging && !clearTag2Raging) //清除類1 保留類2
                            //    {
                            //        updateSql += "group_rating='<Ratings>" + tag2Rating.OuterXml + "</Ratings>'";
                            //    }
                            //    else if(!clearTag1Raging && clearTag2Raging) //清除類2 保留類1
                            //    {
                            //        updateSql += "group_rating='<Ratings>" + tag1Rating.OuterXml + "</Ratings>'";
                            //    }
                            //    else // 都上傳
                            //        updateSql += "group_rating='<Ratings>" + tag1Rating.OuterXml + tag2Rating.OuterXml + "</Ratings>'";
                            //}


                            // 僅匯入類1排名
                            if (hastag1Rating)
                            {
                                if (updateSql != "update sems_entry_score set ")
                                {
                                    updateSql += ",";
                                }             
                                    updateSql += "group_rating='<Ratings>" + tag1Rating.OuterXml + "</Ratings>'";
                            }


                            updateSql += " WHERE ref_student_id =" + id + " and school_year=" + sy + " and semester=" + se + " and entry_group=1;";
                            sql.Add(updateSql);
                        }
                        #endregion
                    }
                }
                #endregion
                #region 整理成新增或修改清單
                foreach (int sy in semesterImportScore.Keys)
                {
                    foreach (int se in semesterImportScore[sy].Keys)
                    {
                        //抓年級
                        string gy = "" + semesterGradeYearDictionary[sy][se];
                        //匯入行為類成績
                        #region 匯入行為類成績
                        if (semesterImportScore[sy][se].ContainsKey("德行"))
                        {
                            if (semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey("德行"))
                            {
                                //修改
                                XmlElement entryScore = doc.CreateElement("SemesterEntryScore");
                                string scoreId = scoreIDDictionary[sy][se]["行為"];
                                if (semesterImportScore[sy][se]["德行"] != null)
                                {
                                    XmlElement entryElement = doc.CreateElement("Entry");
                                    entryElement.SetAttribute("分項", "德行");
                                    entryElement.SetAttribute("成績", "" + semesterImportScore[sy][se]["德行"]);
                                    entryScore.AppendChild(entryElement);
                                    updateList.Add(new EditScore.UpdateInfo(scoreId, gy, entryScore));
                                }
                                else
                                {
                                    deleteList.Add(scoreId);
                                }
                            }
                            else
                            {
                                if (semesterImportScore[sy][se]["德行"] != null)
                                {
                                    //新增
                                    XmlElement entryScore = doc.CreateElement("SemesterEntryScore");
                                    XmlElement entryElement = doc.CreateElement("Entry");
                                    entryElement.SetAttribute("分項", "德行");
                                    entryElement.SetAttribute("成績", "" + semesterImportScore[sy][se]["德行"]);
                                    entryScore.AppendChild(entryElement);
                                    insertList.Add(new AddScore.InsertInfo("" + studentRec.StudentID, "" + sy, "" + se, gy, "行為", entryScore));
                                }
                            }
                        }
                        else if (e.ImportFields.Contains("成績年級") && semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey("德行"))
                        {
                            //修改成績年級
                            string scoreId = scoreIDDictionary[sy][se]["行為"];
                            XmlElement entryScore = doc.CreateElement("SemesterEntryScore");
                            XmlElement entryElement = doc.CreateElement("Entry");
                            entryElement.SetAttribute("分項", "德行");
                            entryElement.SetAttribute("成績", "" + semesterScoreDictionary[sy][se]["德行"].Score);
                            entryScore.AppendChild(entryElement);
                            updateList.Add(new EditScore.UpdateInfo(scoreId, gy, entryScore));
                        }
                        #endregion
                        //匯入學習類成績
                        #region 匯入學習類成績
                        if (semesterImportScore[sy][se].ContainsKey("學業")
                            || semesterImportScore[sy][se].ContainsKey("體育")
                            || semesterImportScore[sy][se].ContainsKey("國防通識")
                            || semesterImportScore[sy][se].ContainsKey("健康與護理")
                            || semesterImportScore[sy][se].ContainsKey("實習科目")
                            || semesterImportScore[sy][se].ContainsKey("專業科目")
                            || semesterImportScore[sy][se].ContainsKey("學業(原始)")
                            || semesterImportScore[sy][se].ContainsKey("體育(原始)")
                            || semesterImportScore[sy][se].ContainsKey("國防通識(原始)")
                            || semesterImportScore[sy][se].ContainsKey("健康與護理(原始)")
                            || semesterImportScore[sy][se].ContainsKey("實習科目(原始)")
                            || semesterImportScore[sy][se].ContainsKey("專業科目(原始)")
                            )
                        {
                            #region 匯入分項成績
                            string scoreId = "";
                            bool hasEntry = false;
                            XmlElement entryScore = doc.CreateElement("SemesterEntryScore");
                            foreach (string key in new string[] { "學業"
                                , "體育"
                                , "國防通識"
                                , "健康與護理"
                                , "實習科目" 
                                , "專業科目" 
                                , "學業(原始)"
                                , "體育(原始)"
                                , "國防通識(原始)"
                                , "健康與護理(原始)"
                                , "實習科目(原始)" 
                                , "專業科目(原始)" 
                                })
                            {
                                if (semesterImportScore[sy][se].ContainsKey(key) ||//有匯入此分項成績
                                    (semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey(key))//原本有此分項成績
                                    )
                                {
                                    XmlElement entryElement = doc.CreateElement("Entry");
                                    entryElement.SetAttribute("分項", key);
                                    if (semesterImportScore[sy][se].ContainsKey(key))//有匯入
                                    {
                                        if (semesterImportScore[sy][se][key] != null)//有成績
                                        {
                                            entryElement.SetAttribute("成績", "" + semesterImportScore[sy][se][key]);
                                            entryScore.AppendChild(entryElement);
                                            hasEntry = true;
                                        }
                                    }
                                    else//保留原本就有的
                                    {
                                        entryElement.SetAttribute("成績", "" + semesterScoreDictionary[sy][se][key].Score);
                                        entryScore.AppendChild(entryElement);
                                        hasEntry = true;
                                    }
                                    //之前有學習類分項成績的話就抓ID
                                    if (scoreId == "" && semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey(key))
                                    {
                                        scoreId = scoreIDDictionary[sy][se]["學習"];
                                    }
                                }
                            }
                            if (scoreId != "")
                            {
                                if (hasEntry)
                                {
                                    //修改
                                    updateList.Add(new EditScore.UpdateInfo(scoreId, gy, entryScore));
                                }
                                else
                                    deleteList.Add(scoreId);
                            }
                            else
                            {
                                //新增
                                insertList.Add(new AddScore.InsertInfo(studentRec.StudentID, "" + sy, "" + se, gy, "學習", entryScore));
                            }
                            #endregion
                        }//沒有匯入分項成績但有改成績年級
                        else if (e.ImportFields.Contains("成績年級") && semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) &&
                            (
                            semesterScoreDictionary[sy][se].ContainsKey("學業") ||
                            semesterScoreDictionary[sy][se].ContainsKey("體育") ||
                            semesterScoreDictionary[sy][se].ContainsKey("國防通識") ||
                            semesterScoreDictionary[sy][se].ContainsKey("健康與護理") ||
                            semesterScoreDictionary[sy][se].ContainsKey("實習科目") ||
                            semesterScoreDictionary[sy][se].ContainsKey("專業科目") ||
                            semesterScoreDictionary[sy][se].ContainsKey("學業(原始)") ||
                            semesterScoreDictionary[sy][se].ContainsKey("體育(原始)") ||
                            semesterScoreDictionary[sy][se].ContainsKey("國防通識(原始)") ||
                            semesterScoreDictionary[sy][se].ContainsKey("健康與護理(原始)") ||
                            semesterScoreDictionary[sy][se].ContainsKey("實習科目(原始)") ||
                            semesterScoreDictionary[sy][se].ContainsKey("專業科目(原始)")
                            ))
                        {
                            #region 修改成績年級
                            XmlElement entryScore = doc.CreateElement("SemesterEntryScore");
                            string scoreId = "";
                            foreach (string key in new string[] { "學業"
                                , "體育"
                                , "國防通識"
                                , "健康與護理"
                                , "實習科目" 
                                , "專業科目"     
                                , "學業(原始)"
                                , "體育(原始)"
                                , "國防通識(原始)"
                                , "健康與護理(原始)"
                                , "實習科目(原始)" 
                                , "專業科目(原始) "     
                            })
                            {
                                if (semesterScoreDictionary.ContainsKey(sy) && semesterScoreDictionary[sy].ContainsKey(se) && semesterScoreDictionary[sy][se].ContainsKey(key)) //原本有此分項成績                        
                                {
                                    XmlElement entryElement = doc.CreateElement("Entry");
                                    entryElement.SetAttribute("分項", key);
                                    if (semesterImportScore[sy][se].ContainsKey(key))//有匯入
                                    {
                                        if (semesterImportScore[sy][se][key] != null)//有成績
                                        {
                                            entryElement.SetAttribute("成績", "" + semesterImportScore[sy][se][key]);
                                            entryScore.AppendChild(entryElement);
                                        }
                                    }
                                    else//保留原本就有的
                                    {
                                        entryElement.SetAttribute("成績", "" + semesterScoreDictionary[sy][se][key].Score);
                                        entryScore.AppendChild(entryElement);
                                    }
                                    scoreId = scoreIDDictionary[sy][se]["學習"];
                                }
                            }
                            updateList.Add(new EditScore.UpdateInfo(scoreId, gy, entryScore));
                            #endregion
                        }
                        #endregion
                    }
                }
                #endregion
            }
            #endregion

            if (updateList.Count > 0)
            {
                #region 分批次兩路上傳
                List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>> updatePackages = new List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>>();
                List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>> updatePackages2 = new List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>>();
                {
                    List<SmartSchool.Feature.Score.EditScore.UpdateInfo> package = null;
                    int count = 0;
                    foreach (SmartSchool.Feature.Score.EditScore.UpdateInfo var in updateList)
                    {
                        if (count == 0)
                        {
                            package = new List<SmartSchool.Feature.Score.EditScore.UpdateInfo>(30);
                            count = 30;
                            if ((updatePackages.Count & 1) == 0)
                                updatePackages.Add(package);
                            else
                                updatePackages2.Add(package);
                        }
                        package.Add(var);
                        count--;
                    }
                }
                Thread threadUpdateSemesterEntryScore = new Thread(new ParameterizedThreadStart(updateSemesterEntryScore));
                threadUpdateSemesterEntryScore.IsBackground = true;
                threadUpdateSemesterEntryScore.Start(updatePackages);
                Thread threadUpdateSemesterEntryScore2 = new Thread(new ParameterizedThreadStart(updateSemesterEntryScore));
                threadUpdateSemesterEntryScore2.IsBackground = true;
                threadUpdateSemesterEntryScore2.Start(updatePackages2);

                threadUpdateSemesterEntryScore.Join();
                threadUpdateSemesterEntryScore2.Join();
                #endregion
            }
            if (insertList.Count > 0)
            {
                #region 分批次兩路上傳

                List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>> insertPackages = new List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>>();
                List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>> insertPackages2 = new List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>>();
                {
                    List<SmartSchool.Feature.Score.AddScore.InsertInfo> package = null;
                    int count = 0;
                    foreach (SmartSchool.Feature.Score.AddScore.InsertInfo var in insertList)
                    {
                        if (count == 0)
                        {
                            package = new List<SmartSchool.Feature.Score.AddScore.InsertInfo>(30);
                            count = 30;
                            if ((insertPackages.Count & 1) == 0)
                                insertPackages.Add(package);
                            else
                                insertPackages2.Add(package);
                        }
                        package.Add(var);
                        count--;
                    }
                }
                Thread threadInsertSemesterEntryScore = new Thread(new ParameterizedThreadStart(insertSemesterEntryScore));
                threadInsertSemesterEntryScore.IsBackground = true;
                threadInsertSemesterEntryScore.Start(insertPackages);
                Thread threadInsertSemesterEntryScore2 = new Thread(new ParameterizedThreadStart(insertSemesterEntryScore));
                threadInsertSemesterEntryScore2.IsBackground = true;
                threadInsertSemesterEntryScore2.Start(insertPackages2);

                threadInsertSemesterEntryScore.Join();
                threadInsertSemesterEntryScore2.Join();
                #endregion
            }
            if (deleteList.Count > 0)
            {
                #region 分批次兩路上傳
                List<List<string>> deletePackages = new List<List<string>>();
                List<List<string>> deletePackages2 = new List<List<string>>();
                {
                    List<string> package = null;
                    int count = 0;
                    foreach (string var in deleteList)
                    {
                        if (count == 0)
                        {
                            package = new List<string>(30);
                            count = 30;
                            if ((deletePackages.Count & 1) == 0)
                                deletePackages.Add(package);
                            else
                                deletePackages2.Add(package);
                        }
                        package.Add(var);
                        count--;
                    }
                }
                Thread threadDeleteSemesterEntryScore = new Thread(new ParameterizedThreadStart(deleteSemesterEntryScore));
                threadDeleteSemesterEntryScore.IsBackground = true;
                threadDeleteSemesterEntryScore.Start(deletePackages);
                Thread threadDeleteSemesterEntryScore2 = new Thread(new ParameterizedThreadStart(deleteSemesterEntryScore));
                threadDeleteSemesterEntryScore2.IsBackground = true;
                threadDeleteSemesterEntryScore2.Start(deletePackages2);

                threadDeleteSemesterEntryScore.Join();
                threadDeleteSemesterEntryScore2.Join();
                #endregion
            }
            if (sql.Count > 0)
            {
                UpdateHelper updateHelper = new UpdateHelper();
                updateHelper.Execute(sql);
            }
        }

        private void updateSemesterEntryScore(object item)
        {
            List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>> updatePackages = (List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>>)item;
            foreach (List<SmartSchool.Feature.Score.EditScore.UpdateInfo> package in updatePackages)
            {
                SmartSchool.Feature.Score.EditScore.UpdateSemesterEntryScore(package.ToArray());
            }
        }

        private void insertSemesterEntryScore(object item)
        {
            List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>> insertPackages = (List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>>)item;
            foreach (List<SmartSchool.Feature.Score.AddScore.InsertInfo> package in insertPackages)
            {
                SmartSchool.Feature.Score.AddScore.InsertSemesterEntryScore(package.ToArray());
            }
        }

        private void deleteSemesterEntryScore(object item)
        {
            List<List<string>> insertPackages = (List<List<string>>)item;
            foreach (List<string> package in insertPackages)
            {
                SmartSchool.Feature.Score.RemoveScore.DeleteSemesterEntityScore(package.ToArray());
            }
        }

        private void ImportSemesterEntryScore_EndImport(object sender, EventArgs e)
        {
            //EventHub.Instance.InvokScoreChanged(new List<string>(_StudentCollection.Keys).ToArray());
        }
    }
}
