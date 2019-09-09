using System.Collections.Generic;
using System.Threading;
using System.Xml;
using SmartSchool.AccessControl;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Customization.PlugIn.ImportExport;

namespace SmartSchool.Evaluation.ImportExport
{
    [FeatureCode("Button0240")]
    class ImportSchoolYearSubjectScore : ImportProcess
    {
        private Dictionary<string, int> _ID_SchoolYear_GradeYear = new Dictionary<string, int>();

        private Dictionary<string, List<string>> _ID_SchoolYear_Subject = new Dictionary<string, List<string>>();

        private Dictionary<string, StudentRecord> _StudentCollection = new Dictionary<string, StudentRecord>();

        private Dictionary<string, Dictionary<int, List<string>>> _StudentSchoolYearSubjectCollection = new Dictionary<string, Dictionary<int, List<string>>>();

        private Dictionary<string, Dictionary<int, int>> _StudentSchoolYearGradeYearCollection = new Dictionary<string, Dictionary<int, int>>();

        private AccessHelper _AccessHelper;

        public ImportSchoolYearSubjectScore()
        {
            this.Image = null;
            this.Title = "匯入學年科目成績";
            this.Group = "學年科目成績";
            this.PackageLimit = 3000;
            foreach (string field in new string[] { "科目", "學年度", "成績年級", "結算成績", "補考成績", "重修成績" })
            {
                this.ImportableFields.Add(field);
            }
            foreach (string field in new string[] { "科目", "學年度", "成績年級" })
            {
                this.RequiredFields.Add(field);
            }
        }

        protected override void OnBeginValidate(BeginValidateEventArgs e)
        {
            _ID_SchoolYear_GradeYear.Clear();
            _ID_SchoolYear_Subject.Clear();
            _StudentCollection.Clear();
            _StudentSchoolYearSubjectCollection.Clear();
            _StudentSchoolYearGradeYearCollection.Clear();
            _AccessHelper = new AccessHelper();

            List<StudentRecord> list = _AccessHelper.StudentHelper.GetStudents(e.List);
            //_AccessHelper.StudentHelper.FillSemesterSubjectScore(false, list);


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
            Thread threadLoadSchoolYearSubjectScore = new Thread(new ParameterizedThreadStart(loadSubjectScore));
            threadLoadSchoolYearSubjectScore.IsBackground = true;
            threadLoadSchoolYearSubjectScore.Start(loadPackages);
            Thread threadLoadSchoolYearSubjectScore2 = new Thread(new ParameterizedThreadStart(loadSubjectScore));
            threadLoadSchoolYearSubjectScore2.IsBackground = true;
            threadLoadSchoolYearSubjectScore2.Start(loadPackages2);

            threadLoadSchoolYearSubjectScore.Join();
            threadLoadSchoolYearSubjectScore2.Join();


            foreach (StudentRecord stu in list)
            {
                if (!_StudentCollection.ContainsKey(stu.StudentID))
                {
                    _StudentCollection.Add(stu.StudentID, stu);
                    _StudentSchoolYearSubjectCollection.Add(stu.StudentID, new Dictionary<int, List<string>>());
                    _StudentSchoolYearGradeYearCollection.Add(stu.StudentID, new Dictionary<int, int>());
                    foreach (SemesterSubjectScoreInfo semescore in stu.SemesterSubjectScoreList)
                    {
                        //統計此學年中學期科目成績中所包含的科目
                        if (!_StudentSchoolYearSubjectCollection[stu.StudentID].ContainsKey(semescore.SchoolYear))
                            _StudentSchoolYearSubjectCollection[stu.StudentID].Add(semescore.SchoolYear, new List<string>());
                        if (!_StudentSchoolYearSubjectCollection[stu.StudentID][semescore.SchoolYear].Contains(semescore.Subject))
                            _StudentSchoolYearSubjectCollection[stu.StudentID][semescore.SchoolYear].Add(semescore.Subject);
                        //填入學期科目成績的成績年級資料
                        if (!_StudentSchoolYearGradeYearCollection[stu.StudentID].ContainsKey(semescore.SchoolYear))
                            _StudentSchoolYearGradeYearCollection[stu.StudentID].Add(semescore.SchoolYear, semescore.GradeYear);
                    }
                }
            }
        }

        private void loadSubjectScore(object item)
        {
            List<List<StudentRecord>> Packages = (List<List<StudentRecord>>)item;
            foreach (List<StudentRecord> package in Packages)
            {
                _AccessHelper.StudentHelper.FillSchoolYearSubjectScore(false, package);
                _AccessHelper.StudentHelper.FillSemesterSubjectScore(false, package);
            }
        }

        protected override void OnValidateRow(RowDataValidatedEventArgs e)
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
            bool hasScore = false;
            #region 驗各欄位填寫格式
            foreach (string field in e.SelectFields)
            {
                string value = e.Data[field];
                switch (field)
                {
                    default:
                        break;
                    case "科目":
                        if (value == "")
                        {
                            inputFormatPass &= false;
                            e.ErrorFields.Add(field, "必須填寫");
                        }
                        break;
                    case "學年度":
                    case "成績年級":
                        if (value == "" || !int.TryParse(value, out t))
                        {
                            inputFormatPass &= false;
                            e.ErrorFields.Add(field, "必須填入整數");
                        }
                        break;
                    case "結算成績":
                    case "補考成績":
                    case "重修成績":
                        if (value != "")
                        {
                            if (!decimal.TryParse(value, out d))
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入數字");
                            }
                            else
                            {
                                hasScore = true;
                            }
                        }
                        break;
                }
            }
            #endregion
            //輸入格式正確才會針對情節做檢驗
            if (inputFormatPass)
            {
                string errorMessage = "";

                string subject = e.Data["科目"];
                string schoolYear = e.Data["學年度"];
                int? sy = null;
                if (int.TryParse(schoolYear, out t))
                    sy = t;
                if (sy != null)
                {

                    string key = e.Data.ID + "_" + sy;
                    #region 驗證新增學年成績
                    bool isNewSubjectInfo = true;
                    string message = "";
                    foreach (SchoolYearSubjectScoreInfo info in student.SchoolYearSubjectScoreList)
                    {
                        if (info.SchoolYear == sy)
                        {
                            if (info.Subject == subject)
                                isNewSubjectInfo = false;
                        }
                    }
                    if (isNewSubjectInfo)
                    {
                        if (!e.WarningFields.ContainsKey("科目"))
                            e.WarningFields.Add("科目", "學生在此學年並無此筆學年成績資訊，將會新增此學年成績。");
                        else
                            e.WarningFields["科目"] += "、" + "學生在此學年並無此筆學年成績資訊，將會新增此學年成績。";

                        foreach (string field in new string[] { "科目", "學年度", "成績年級" })
                        {
                            if (!e.SelectFields.Contains(field))
                                message += (message == "" ? "發現此學期無此科目，\n將會新增成績\n缺少成績必要欄位" : "、") + field;
                        }
                        bool hasScoreField = false;
                        foreach (string field in new string[] { "結算成績", "補考成績", "重修成績" })
                        {
                            if (e.SelectFields.Contains(field))
                                hasScoreField = true;
                        }
                        if (!hasScoreField)
                        {
                            message += (message == "" ? "發現此學期無此科目，\n將會新增成績\n缺少成績必要欄位" : "、") + "(結算成績、補考成績、重修成績 擇一)";
                        }
                        if (message != "")
                            errorMessage += (errorMessage == "" ? "" : "\n") + message;
                    }
                    #endregion
                    #region 驗證重複科目資料
                    string skey = subject;
                    if (!_ID_SchoolYear_Subject.ContainsKey(key))
                        _ID_SchoolYear_Subject.Add(key, new List<string>());
                    if (_ID_SchoolYear_Subject[key].Contains(skey))
                    {
                        errorMessage += (errorMessage == "" ? "" : "\n") + "同一學期不允許多筆相同科目資料";
                    }
                    else
                        _ID_SchoolYear_Subject[key].Add(skey);
                    #endregion
                    #region 檢查學期成績包含此科目
                    if (!_StudentSchoolYearSubjectCollection[e.Data.ID].ContainsKey((int)sy) || !_StudentSchoolYearSubjectCollection[e.Data.ID][(int)sy].Contains(subject))
                    {
                        if (!e.WarningFields.ContainsKey("科目"))
                            e.WarningFields.Add("科目", "在此學年的學生學期科目成績中，查無此科目的成績。");
                        else
                            e.WarningFields["科目"] += "、" + "在此學年的學生學期科目成績中，查無此科目的成績。";
                    }
                    #endregion
                    if (e.SelectFields.Contains("成績年級"))
                    {
                        int gy = int.Parse(e.Data["成績年級"]);
                        #region 驗證成績年級變更
                        foreach (SchoolYearSubjectScoreInfo info in student.SchoolYearSubjectScoreList)
                        {
                            if (info.SchoolYear == sy && info.GradeYear != gy)
                            {
                                if (!e.WarningFields.ContainsKey("成績年級"))
                                    e.WarningFields.Add("成績年級", "修改成績年級資訊將會改變此學生在該學期的所有學年成績的成績年級");
                                else
                                    e.WarningFields["成績年級"] += "、" + "修改成績年級資訊將會改變此學生在該學期的所有學年成績的成績年級";
                                break;
                            }
                        }
                        #endregion
                        #region 驗證同學期在匯入資料中成績年級會相同

                        if (!_ID_SchoolYear_GradeYear.ContainsKey(key))
                            _ID_SchoolYear_GradeYear.Add(key, gy);
                        else
                        {
                            if (_ID_SchoolYear_GradeYear[key] != gy)
                            {
                                if (!e.ErrorFields.ContainsKey("成績年級"))
                                    e.ErrorFields.Add("成績年級", "發現此學生同一學年有不同成績年級資料");
                                else
                                    e.ErrorFields["成績年級"] += "、" + "發現此學生同一學年有不同成績年級資料";
                            }
                        }
                        #endregion
                        #region 驗證成績年級與學期成績相同
                        if (_StudentSchoolYearGradeYearCollection[e.Data.ID].ContainsKey((int)sy) && _StudentSchoolYearGradeYearCollection[e.Data.ID][(int)sy] != gy)
                        {
                            if (!e.WarningFields.ContainsKey("成績年級"))
                                e.WarningFields.Add("成績年級", "成績年級與學生科目成績中的成績年級不同。");
                            else
                                e.WarningFields["成績年級"] += "、" + "成績年級與學生科目成績中的成績年級不同。";
                        }
                        #endregion
                    }
                }
                e.ErrorMessage = errorMessage;

                if (!hasScore)
                {
                    e.ErrorMessage += "匯入資料必須包含成績";
                }
            }
        }

        protected override void OnDataImport(DataImportEventArgs args)
        {
            Dictionary<string, List<RowData>> id_Rows = new Dictionary<string, List<RowData>>();
            #region 分包裝
            foreach (RowData data in args.Items)
            {
                if (!id_Rows.ContainsKey(data.ID))
                    id_Rows.Add(data.ID, new List<RowData>());
                id_Rows[data.ID].Add(data);
            }
            #endregion
            List<SmartSchool.Feature.Score.AddScore.InsertInfo> insertList = new List<SmartSchool.Feature.Score.AddScore.InsertInfo>();
            List<SmartSchool.Feature.Score.EditScore.UpdateInfo> updateList = new List<SmartSchool.Feature.Score.EditScore.UpdateInfo>();
            //交叉比對各學生資料
            #region 交叉比對各學生資料
            foreach (string id in id_Rows.Keys)
            {
                XmlDocument doc = new XmlDocument();
                StudentRecord studentRec = _StudentCollection[id];
                //該學生的學期學年成績
                Dictionary<int, Dictionary<string, SchoolYearSubjectScoreInfo>> schoolYearScoreDictionary = new Dictionary<int, Dictionary<string, SchoolYearSubjectScoreInfo>>();
                Dictionary<int, string> schoolYearScoreID = new Dictionary<int, string>();
                Dictionary<int, string> scoreID = (Dictionary<int, string>)studentRec.Fields["SchoolYearSubjectScoreID"];
                #region 整理現有的成績資料
                foreach (SchoolYearSubjectScoreInfo var in studentRec.SchoolYearSubjectScoreList)
                {
                    string key = var.Subject;
                    if (!schoolYearScoreDictionary.ContainsKey(var.SchoolYear))
                        schoolYearScoreDictionary.Add(var.SchoolYear, new Dictionary<string, SchoolYearSubjectScoreInfo>());
                    if (!schoolYearScoreDictionary[var.SchoolYear].ContainsKey(key))
                        schoolYearScoreDictionary[var.SchoolYear].Add(key, var);
                    //填入學年科目成績編號
                    if (!schoolYearScoreID.ContainsKey(var.SchoolYear))
                        schoolYearScoreID.Add(var.SchoolYear, scoreID[var.SchoolYear]);
                }
                #endregion
                //要匯入的學期學年成績
                Dictionary<int, Dictionary<string, RowData>> schoolYearImportScoreDictionary = new Dictionary<int, Dictionary<string, RowData>>();
                #region 整理要匯入的資料
                foreach (RowData row in id_Rows[id])
                {
                    int t;
                    string subject = row["科目"];
                    string schoolYear = row["學年度"];
                    int sy = int.Parse(schoolYear);
                    string key = subject;
                    if (!schoolYearImportScoreDictionary.ContainsKey(sy))
                        schoolYearImportScoreDictionary.Add(sy, new Dictionary<string, RowData>());
                    if (!schoolYearImportScoreDictionary[sy].ContainsKey(key))
                        schoolYearImportScoreDictionary[sy].Add(key, row);
                }
                #endregion

                //學期年級重整
                Dictionary<int, int> schoolYearGradeYear = new Dictionary<int, int>();
                //要變更成績的學期
                List<int> updatedSchoolYear = new List<int>();
                //在變更學期中新增加的成績資料
                Dictionary<int, List<RowData>> updatedNewSchoolYearScore = new Dictionary<int, List<RowData>>();
                //要增加成績的學期
                Dictionary<int, List<RowData>> insertNewSchoolYearScore = new Dictionary<int, List<RowData>>();
                //開始處理ImportScore
                #region 開始處理ImportScore
                foreach (int sy in schoolYearImportScoreDictionary.Keys)
                {
                    foreach (string key in schoolYearImportScoreDictionary[sy].Keys)
                    {
                        RowData data = schoolYearImportScoreDictionary[sy][key];
                        //如果是本來沒有這筆學期的成績就加到insertNewSemesterScore
                        if (!schoolYearScoreDictionary.ContainsKey(sy))
                        {
                            if (!insertNewSchoolYearScore.ContainsKey(sy))
                                insertNewSchoolYearScore.Add(sy, new List<RowData>());
                            insertNewSchoolYearScore[sy].Add(data);
                            //加入學期年級
                            int gy = int.Parse(data["成績年級"]);
                            if (!schoolYearGradeYear.ContainsKey(sy))
                                schoolYearGradeYear.Add(sy, gy);
                            else
                                schoolYearGradeYear[sy] = gy;
                        }
                        else
                        {
                            bool hasChanged = false;
                            //修改已存在的資料
                            if (schoolYearScoreDictionary[sy].ContainsKey(key))
                            {
                                SchoolYearSubjectScoreInfo score = schoolYearScoreDictionary[sy][key];
                                #region 填入此學期的年級資料
                                if (!schoolYearGradeYear.ContainsKey(sy))
                                    schoolYearGradeYear.Add(sy, score.GradeYear);
                                #endregion
                                #region 直接修改已存在的成績資料的Detail
                                foreach (string field in args.ImportFields)
                                {
                                    string value = data[field];
                                    switch (field)
                                    {
                                        default: break;
                                        case "結算成績":
                                        case "補考成績":
                                        case "重修成績":
                                            if (score.Detail.GetAttribute(field) != value)
                                            {
                                                score.Detail.SetAttribute(field, value);
                                                hasChanged = true;
                                            }
                                            break;
                                        case "成績年級":
                                            int gy = int.Parse(data["成績年級"]);
                                            if (score.GradeYear != gy)
                                            {
                                                schoolYearGradeYear[sy] = gy;
                                                hasChanged = true;
                                            }
                                            break;
                                    }
                                }
                                decimal topScore = decimal.MinValue, tryParseScore;
                                foreach (var item in new string[]{
                                        "結算成績",
                                        "補考成績",
                                        "重修成績"})
                                {
                                    if (decimal.TryParse(score.Detail.GetAttribute(item), out tryParseScore) && tryParseScore > topScore)
                                        topScore = tryParseScore;
                                }
                                score.Detail.SetAttribute("學年成績", "" + topScore);
                                #endregion
                            }
                            else//加入新成績至已存在的學期
                            {
                                //加入學期年級
                                int gy = int.Parse(data["成績年級"]);
                                if (!schoolYearGradeYear.ContainsKey(sy))
                                    schoolYearGradeYear.Add(sy, gy);
                                else
                                    schoolYearGradeYear[sy] = gy;
                                //加入新成績至已存在的學期
                                if (!updatedNewSchoolYearScore.ContainsKey(sy))
                                    updatedNewSchoolYearScore.Add(sy, new List<RowData>());
                                updatedNewSchoolYearScore[sy].Add(data);
                                hasChanged = true;
                            }
                            //真的有變更
                            if (hasChanged)
                            {
                                #region 登錄有變更的學期
                                if (!updatedSchoolYear.Contains(sy))
                                    updatedSchoolYear.Add(sy);
                                #endregion
                            }
                        }
                    }
                }
                #endregion
                //處理已登錄要更新的學期成績
                #region 處理已登錄要更新的學期成績
                foreach (int sy in updatedSchoolYear)
                {
                    string scoreid = schoolYearScoreID[sy];//從學年抓ID
                    string gradeyear = "" + schoolYearGradeYear[sy];//抓年級
                    XmlElement subjectScoreInfo = doc.CreateElement("SchoolYearSubjectScore");
                    #region 產生該學期學年成績的XML
                    foreach (SchoolYearSubjectScoreInfo scoreInfo in schoolYearScoreDictionary[sy].Values)
                    {
                        subjectScoreInfo.AppendChild(doc.ImportNode(scoreInfo.Detail, true));
                    }
                    if (updatedNewSchoolYearScore.ContainsKey(sy))
                    {
                        foreach (RowData row in updatedNewSchoolYearScore[sy])
                        {
                            XmlElement newScore = doc.CreateElement("Subject");
                            #region 建立newScore
                            foreach (string field in new string[] { "科目", "結算成績", "補考成績", "重修成績" })
                            {
                                if (args.ImportFields.Contains(field))
                                {
                                    string value = row[field];
                                    switch (field)
                                    {
                                        default: break;
                                        case "科目":
                                            newScore.SetAttribute("科目", value);
                                            break;

                                        case "結算成績":
                                        case "補考成績":
                                        case "重修成績":
                                            newScore.SetAttribute(field, value);
                                            break;
                                    }
                                }
                            }
                            decimal topScore = decimal.MinValue, tryParseScore;
                            foreach (var item in new string[]{
                                        "結算成績",
                                        "補考成績",
                                        "重修成績"})
                            {
                                if (decimal.TryParse(newScore.GetAttribute(item), out tryParseScore) && tryParseScore > topScore)
                                    topScore = tryParseScore;
                            }
                            newScore.SetAttribute("學年成績", "" + topScore);
                            #endregion
                            subjectScoreInfo.AppendChild(newScore);
                        }
                    }
                    #endregion
                    updateList.Add(new SmartSchool.Feature.Score.EditScore.UpdateInfo(scoreid, gradeyear, subjectScoreInfo));
                }
                #endregion
                //處理新增成績學期
                #region 處理新增成績學期
                foreach (int sy in insertNewSchoolYearScore.Keys)
                {
                    XmlElement subjectScoreInfo = doc.CreateElement("SchoolYearSubjectScore");
                    string gradeyear = "" + schoolYearGradeYear[sy];//抓年級
                    foreach (RowData row in insertNewSchoolYearScore[sy])
                    {
                        XmlElement newScore = doc.CreateElement("Subject");
                        #region 建立newScore
                        foreach (string field in new string[] { "科目", "結算成績", "補考成績", "重修成績" })
                        {
                            if (args.ImportFields.Contains(field))
                            {
                                string value = row[field];
                                switch (field)
                                {
                                    default: break;
                                    case "科目":
                                        newScore.SetAttribute("科目", value);
                                        break;

                                    case "結算成績":
                                    case "補考成績":
                                    case "重修成績":
                                        newScore.SetAttribute(field, value);
                                        break;
                                }
                            }
                        }
                        decimal topScore = decimal.MinValue, tryParseScore;
                        foreach (var item in new string[]{
                                        "結算成績",
                                        "補考成績",
                                        "重修成績"})
                        {
                            if (decimal.TryParse(newScore.GetAttribute(item), out tryParseScore) && tryParseScore > topScore)
                                topScore = tryParseScore;
                        }
                        newScore.SetAttribute("學年成績", "" + topScore);
                        #endregion
                        subjectScoreInfo.AppendChild(newScore);
                    }
                    insertList.Add(new SmartSchool.Feature.Score.AddScore.InsertInfo(studentRec.StudentID, "" + sy, "", gradeyear, "", subjectScoreInfo));
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
                Thread threadUpdateSchoolYearSubjectScore = new Thread(new ParameterizedThreadStart(updateSchoolYearSubjectScore));
                threadUpdateSchoolYearSubjectScore.IsBackground = true;
                threadUpdateSchoolYearSubjectScore.Start(updatePackages);
                Thread threadUpdateSchoolYearSubjectScore2 = new Thread(new ParameterizedThreadStart(updateSchoolYearSubjectScore));
                threadUpdateSchoolYearSubjectScore2.IsBackground = true;
                threadUpdateSchoolYearSubjectScore2.Start(updatePackages2);

                threadUpdateSchoolYearSubjectScore.Join();
                threadUpdateSchoolYearSubjectScore2.Join();
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
                Thread threadInsertSchoolYearSubjectScore = new Thread(new ParameterizedThreadStart(insertSchoolYearSubjectScore));
                threadInsertSchoolYearSubjectScore.IsBackground = true;
                threadInsertSchoolYearSubjectScore.Start(insertPackages);
                Thread threadInsertSchoolYearSubjectScore2 = new Thread(new ParameterizedThreadStart(insertSchoolYearSubjectScore));
                threadInsertSchoolYearSubjectScore2.IsBackground = true;
                threadInsertSchoolYearSubjectScore2.Start(insertPackages2);

                threadInsertSchoolYearSubjectScore.Join();
                threadInsertSchoolYearSubjectScore2.Join();
                #endregion
            }
        }

        private void updateSchoolYearSubjectScore(object item)
        {
            List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>> updatePackages = (List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>>)item;
            foreach (List<SmartSchool.Feature.Score.EditScore.UpdateInfo> package in updatePackages)
            {
                SmartSchool.Feature.Score.EditScore.UpdateSchoolYearSubjectScore(package.ToArray());
            }
        }

        private void insertSchoolYearSubjectScore(object item)
        {
            List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>> insertPackages = (List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>>)item;
            foreach (List<SmartSchool.Feature.Score.AddScore.InsertInfo> package in insertPackages)
            {
                SmartSchool.Feature.Score.AddScore.InsertSchoolYearSubjectScore(package.ToArray());
            }
        }

        protected override void OnEndImport()
        {
            EventHub.Instance.InvokScoreChanged(new List<string>(_StudentCollection.Keys).ToArray());
        }
    }
}
