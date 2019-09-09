using System.Collections.Generic;
using System.Threading;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.AccessControl;
using SmartSchool.API.PlugIn;
using SmartSchool.Common;
//using SmartSchool.Customization.PlugIn.ImportExport;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;

namespace SmartSchool.Evaluation.ImportExport
{
    [FeatureCode("Button0220")]
    class ImportSemesterSubjectScore : SmartSchool.API.PlugIn.Import.Importer
    {
        public ImportSemesterSubjectScore()
        {
            this.Image = null;
            this.Text = "匯入學期科目成績";
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            Dictionary<string, int> _ID_SchoolYear_Semester_GradeYear = new Dictionary<string, int>();
            Dictionary<string, List<string>> _ID_SchoolYear_Semester_Subject = new Dictionary<string, List<string>>();
            Dictionary<string, StudentRecord> _StudentCollection = new Dictionary<string, StudentRecord>();
            Dictionary<StudentRecord, Dictionary<int, decimal>> _StudentPassScore = new Dictionary<StudentRecord, Dictionary<int, decimal>>();
            AccessHelper _AccessHelper;

            VirtualRadioButton autoCheckPass = new VirtualRadioButton("自動判斷取得學分", false);
            VirtualRadioButton manulCheckPass = new VirtualRadioButton("手動判斷取得學分", true);
            autoCheckPass.CheckedChanged += delegate
            {
                if (autoCheckPass.Checked)
                {
                    if (!wizard.RequiredFields.Contains("成績年級"))
                        wizard.RequiredFields.Add("成績年級");
                    if (!wizard.RequiredFields.Contains("取得學分"))
                        wizard.RequiredFields.Add("取得學分");
                }
            };
            manulCheckPass.CheckedChanged += delegate
            {
                if (manulCheckPass.Checked)
                {
                    if (wizard.RequiredFields.Contains("成績年級"))
                        wizard.RequiredFields.Remove("成績年級");
                    if (wizard.RequiredFields.Contains("取得學分"))
                        wizard.RequiredFields.Remove("取得學分");
                }
            };
            wizard.Options.AddRange(autoCheckPass, manulCheckPass);
            wizard.PackageLimit = 3000;
            wizard.ImportableFields.AddRange("科目", "科目級別", "學年度", "學期", "英文名稱", "學分數", "分項類別", "成績年級", "必選修", "校部訂", "原始成績", "補考成績", "重修成績", "手動調整成績", "學年調整成績", "取得學分", "不計學分", "不需評分", "註記", "是否補修成績", "重修學年度", "重修學期");
            wizard.RequiredFields.AddRange("科目", "科目級別", "學年度", "學期");

            wizard.ValidateStart += delegate (object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
            {
                #region ValidateStart
                _ID_SchoolYear_Semester_GradeYear.Clear();
                _ID_SchoolYear_Semester_Subject.Clear();
                _StudentCollection.Clear();
                _AccessHelper = new AccessHelper();

                List<StudentRecord> list = _AccessHelper.StudentHelper.GetStudents(e.List);
                MultiThreadWorker<StudentRecord> loader = new MultiThreadWorker<StudentRecord>();
                loader.MaxThreads = 3;
                loader.PackageSize = 250;
                loader.PackageWorker += delegate (object sender1, PackageWorkEventArgs<StudentRecord> e1)
                {
                    _AccessHelper.StudentHelper.FillSemesterSubjectScore(false, e1.List);
                };
                loader.Run(list);

                foreach (StudentRecord stu in list)
                {
                    if (!_StudentCollection.ContainsKey(stu.StudentID))
                        _StudentCollection.Add(stu.StudentID, stu);
                }
                #endregion
            };
            wizard.ValidateRow += delegate (object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
            {
                #region ValidateRow
                int t;
                decimal k;
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
                        case "科目":
                            if (value == "")
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填寫");
                            }
                            break;
                        case "科目級別":
                            if (value != "" && !int.TryParse(value, out t))
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入空白或整數");
                            }
                            break;
                        case "學年度":
                        case "學分數":
                            if (value == "" || !decimal.TryParse(value, out k))
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入數字或小數");
                            }
                            break;
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
                        case "原始成績":
                        case "補考成績":
                        case "重修成績":
                        case "手動調整成績":
                        case "學年調整成績":
                            if (value != "" && !decimal.TryParse(value, out d))
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入空白或數值");
                            }
                            break;
                        case "取得學分":
                            if (value != "是" && value != "否" && manulCheckPass.Checked)
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入是或否");
                            }
                            else if (autoCheckPass.Checked && value != "自動")
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "使用自動判斷取得學分時，取得學分欄位必須填入「自動」");
                            }
                            break;
                        case "不需評分":
                        case "不計學分":
                            if (value != "是" && value != "否" && value != "")
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入空白或是否");
                            }
                            break;
                        case "必選修":
                            if (value != "必修" && value != "選修")
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入必修或選修");
                            }
                            break;
                        case "校部訂":
                            if (value != "校訂" && value != "部訂")
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入校訂或部訂");
                            }
                            break;
                        case "分項類別":
                            if (value != "學業" && value != "體育" && value != "國防通識" && value != "健康與護理" && value != "實習科目" && value != "專業科目")
                            {
                                inputFormatPass &= false;
                                e.ErrorFields.Add(field, "必須填入 學業、體育、國防通識、健康與護理、專業科目或實習科目");
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
                    string level = e.Data["科目級別"];
                    string schoolYear = e.Data["學年度"];
                    string semester = e.Data["學期"];
                    int? sy = null;
                    int? se = null;
                    int? le = null;
                    if (int.TryParse(level, out t))
                        le = t;
                    if (int.TryParse(schoolYear, out t))
                        sy = t;
                    if (int.TryParse(semester, out t))
                        se = t;
                    if (sy != null && se != null)
                    {
                        string key = e.Data.ID + "_" + sy + "_" + se;
                        #region 驗證新增科目成績
                        bool isNewSubjectInfo = true;
                        string message = "";
                        foreach (SemesterSubjectScoreInfo info in student.SemesterSubjectScoreList)
                        {
                            if (info.SchoolYear == sy && info.Semester == se)
                            {
                                if (info.Subject == subject && info.Level == level)
                                    isNewSubjectInfo = false;
                            }
                        }
                        if (isNewSubjectInfo)
                        {
                            if (!e.WarningFields.ContainsKey("查無此科目"))
                                e.WarningFields.Add("查無此科目", "學生在此學期並無此筆科目成績資訊，將會新增此科目成績");
                            foreach (string field in new string[] { "科目", "科目級別", "學年度", "學期", "學分數", "分項類別", "成績年級", "必選修", "校部訂", "取得學分" })
                            {
                                if (!e.SelectFields.Contains(field))
                                    message += (message == "" ? "發現此學期無此科目，\n將會新增成績\n缺少成績必要欄位" : "、") + field;
                            }
                            if (message != "")
                                errorMessage += (errorMessage == "" ? "" : "\n") + message;
                        }
                        #endregion
                        #region 驗證重複科目資料
                        string skey = subject + "_" + le;
                        if (!_ID_SchoolYear_Semester_Subject.ContainsKey(key))
                            _ID_SchoolYear_Semester_Subject.Add(key, new List<string>());
                        if (_ID_SchoolYear_Semester_Subject[key].Contains(skey))
                        {
                            errorMessage += (errorMessage == "" ? "" : "\n") + "同一學期不允許多筆相同科目級別的資料";
                        }
                        else
                            _ID_SchoolYear_Semester_Subject[key].Add(skey);
                        #endregion
                        if (e.SelectFields.Contains("成績年級"))
                        {
                            int gy = int.Parse(e.Data["成績年級"]);
                            #region 驗證成績年級變更
                            foreach (SemesterSubjectScoreInfo info in student.SemesterSubjectScoreList)
                            {
                                if (info.SchoolYear == sy && info.Semester == se && info.GradeYear != gy)
                                {
                                    if (!e.WarningFields.ContainsKey("成績年級"))
                                        e.WarningFields.Add("成績年級", "修改成績年級資訊將會改變此學生在該學期的所有科目成績的成績年級");
                                    else
                                        e.WarningFields["成績年級"] += "、" + "修改成績年級資訊將會改變此學生在該學期的所有科目成績的成績年級";
                                    break;
                                }
                            }
                            #endregion
                            #region 驗證同學期在匯入資料中成績年級會相同

                            if (!_ID_SchoolYear_Semester_GradeYear.ContainsKey(key))
                                _ID_SchoolYear_Semester_GradeYear.Add(key, gy);
                            else
                            {
                                if (_ID_SchoolYear_Semester_GradeYear[key] != gy)
                                {
                                    if (!e.ErrorFields.ContainsKey("成績年級"))
                                        e.ErrorFields.Add("成績年級", "發現此學生同一學期有不同成績年級資料");
                                    else
                                        e.ErrorFields["成績年級"] += "、" + "發現此學生同一學期有不同成績年級資料";
                                }
                            }
                            #endregion
                            if (autoCheckPass.Checked)
                            {
                                #region 自動判斷取得學分時
                                if (!_StudentPassScore.ContainsKey(student) || !_StudentPassScore[student].ContainsKey(gy))
                                {
                                    decimal passScore = decimal.MinValue;
                                    #region 處理計算規則
                                    XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(student.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(student.StudentID).ScoreCalcRuleElement;
                                    if (scoreCalcRule != null)
                                    {
                                        #region 取得及格標準
                                        DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                                        decimal tryParseDecimal;
                                        foreach (XmlElement element in helper.GetElements("及格標準/學生類別"))
                                        {
                                            string cat = element.GetAttribute("類別");
                                            bool useful = false;
                                            //掃描學生的類別作比對
                                            foreach (CategoryInfo catinfo in student.StudentCategorys)
                                            {
                                                if (catinfo.Name == cat || catinfo.FullName == cat)
                                                    useful = true;
                                            }
                                            //學生是指定的類別或類別為"預設"
                                            if (cat == "預設" || useful)
                                            {
                                                switch (gy)
                                                {
                                                    case 1:
                                                        if (decimal.TryParse(element.GetAttribute("一年級及格標準"), out tryParseDecimal))
                                                        {
                                                            if (passScore == decimal.MinValue || passScore > tryParseDecimal)
                                                                passScore = tryParseDecimal;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (decimal.TryParse(element.GetAttribute("二年級及格標準"), out tryParseDecimal))
                                                        {
                                                            if (passScore == decimal.MinValue || passScore > tryParseDecimal)
                                                                passScore = tryParseDecimal;
                                                        }
                                                        break;
                                                    case 3:
                                                        if (decimal.TryParse(element.GetAttribute("三年級及格標準"), out tryParseDecimal))
                                                        {
                                                            if (passScore == decimal.MinValue || passScore > tryParseDecimal)
                                                                passScore = tryParseDecimal;
                                                        }
                                                        break;
                                                    case 4:
                                                        if (decimal.TryParse(element.GetAttribute("四年級及格標準"), out tryParseDecimal))
                                                        {
                                                            if (passScore == decimal.MinValue || passScore > tryParseDecimal)
                                                                passScore = tryParseDecimal;
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    #endregion
                                    if (passScore == decimal.MinValue)
                                    {
                                        e.WarningFields.Add("成績年級", "學生沒有設定成績計算規則，將以60分做為及格標準。");
                                        passScore = 60m;
                                    }
                                    if (!_StudentPassScore.ContainsKey(student))
                                        _StudentPassScore.Add(student, new Dictionary<int, decimal>());
                                    _StudentPassScore[student].Add(gy, passScore);
                                }
                                #endregion
                            }
                        }
                    }
                    e.ErrorMessage = errorMessage;
                }
                #endregion
            };

            wizard.ImportPackage += delegate (object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
            {
                #region ImportPackage
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
                //交叉比對各學生資料
                #region 交叉比對各學生資料
                foreach (string id in id_Rows.Keys)
                {
                    XmlDocument doc = new XmlDocument();
                    StudentRecord studentRec = _StudentCollection[id];
                    //該學生的學期科目成績
                    Dictionary<int, Dictionary<int, Dictionary<string, SemesterSubjectScoreInfo>>> semesterScoreDictionary = new Dictionary<int, Dictionary<int, Dictionary<string, SemesterSubjectScoreInfo>>>();
                    #region 整理現有的成績資料
                    foreach (SemesterSubjectScoreInfo var in studentRec.SemesterSubjectScoreList)
                    {
                        string key = var.Subject + "_" + var.Level;
                        if (!semesterScoreDictionary.ContainsKey(var.SchoolYear))
                            semesterScoreDictionary.Add(var.SchoolYear, new Dictionary<int, Dictionary<string, SemesterSubjectScoreInfo>>());
                        if (!semesterScoreDictionary[var.SchoolYear].ContainsKey(var.Semester))
                            semesterScoreDictionary[var.SchoolYear].Add(var.Semester, new Dictionary<string, SemesterSubjectScoreInfo>());
                        if (!semesterScoreDictionary[var.SchoolYear][var.Semester].ContainsKey(key))
                            semesterScoreDictionary[var.SchoolYear][var.Semester].Add(key, var);
                    }
                    #endregion
                    //要匯入的學期科目成績
                    Dictionary<int, Dictionary<int, Dictionary<string, RowData>>> semesterImportScoreDictionary = new Dictionary<int, Dictionary<int, Dictionary<string, RowData>>>();
                    #region 整理要匯入的資料
                    foreach (RowData row in id_Rows[id])
                    {
                        int t;
                        string subject = row["科目"];
                        string level = row["科目級別"];
                        string schoolYear = row["學年度"];
                        string semester = row["學期"];
                        int sy = int.Parse(schoolYear);
                        int se = int.Parse(semester);
                        string key = subject + "_" + level;
                        if (!semesterImportScoreDictionary.ContainsKey(sy))
                            semesterImportScoreDictionary.Add(sy, new Dictionary<int, Dictionary<string, RowData>>());
                        if (!semesterImportScoreDictionary[sy].ContainsKey(se))
                            semesterImportScoreDictionary[sy].Add(se, new Dictionary<string, RowData>());
                        if (!semesterImportScoreDictionary[sy][se].ContainsKey(key))
                            semesterImportScoreDictionary[sy][se].Add(key, row);
                    }
                    #endregion

                    //學期年級重整
                    Dictionary<int, Dictionary<int, int>> semesterGradeYear = new Dictionary<int, Dictionary<int, int>>();
                    //要變更成績的學期
                    Dictionary<int, List<int>> updatedSemester = new Dictionary<int, List<int>>();
                    //在變更學期中新增加的成績資料
                    Dictionary<int, Dictionary<int, List<RowData>>> updatedNewSemesterScore = new Dictionary<int, Dictionary<int, List<RowData>>>();
                    //要增加成績的學期
                    Dictionary<int, Dictionary<int, List<RowData>>> insertNewSemesterScore = new Dictionary<int, Dictionary<int, List<RowData>>>();
                    //開始處理ImportScore
                    #region 開始處理ImportScore
                    foreach (int sy in semesterImportScoreDictionary.Keys)
                    {
                        foreach (int se in semesterImportScoreDictionary[sy].Keys)
                        {
                            foreach (string key in semesterImportScoreDictionary[sy][se].Keys)
                            {
                                RowData data = semesterImportScoreDictionary[sy][se][key];
                                //如果是本來沒有這筆學期的成績就加到insertNewSemesterScore
                                if (!semesterScoreDictionary.ContainsKey(sy) || !semesterScoreDictionary[sy].ContainsKey(se))
                                {
                                    if (!insertNewSemesterScore.ContainsKey(sy))
                                        insertNewSemesterScore.Add(sy, new Dictionary<int, List<RowData>>());
                                    if (!insertNewSemesterScore[sy].ContainsKey(se))
                                        insertNewSemesterScore[sy].Add(se, new List<RowData>());
                                    insertNewSemesterScore[sy][se].Add(data);
                                    //加入學期年級
                                    int gy = int.Parse(data["成績年級"]);
                                    if (!semesterGradeYear.ContainsKey(sy))
                                        semesterGradeYear.Add(sy, new Dictionary<int, int>());
                                    if (!semesterGradeYear[sy].ContainsKey(se))
                                        semesterGradeYear[sy].Add(se, gy);
                                    else
                                        semesterGradeYear[sy][se] = gy;
                                }
                                else
                                {
                                    bool hasChanged = false;
                                    //修改已存在的資料
                                    if (semesterScoreDictionary[sy][se].ContainsKey(key))
                                    {
                                        SemesterSubjectScoreInfo score = semesterScoreDictionary[sy][se][key];
                                        #region 填入此學期的年級資料
                                        if (!semesterGradeYear.ContainsKey(sy))
                                            semesterGradeYear.Add(sy, new Dictionary<int, int>());
                                        if (!semesterGradeYear[sy].ContainsKey(se))
                                            semesterGradeYear[sy].Add(se, score.GradeYear);
                                        #endregion
                                        #region 直接修改已存在的成績資料的Detail
                                        foreach (string field in e.ImportFields)
                                        {
                                            string value = data[field];
                                            switch (field)
                                            {
                                                default: break;
                                                case "學分數":
                                                    if (score.Detail.GetAttribute("開課學分數") != value)
                                                    {
                                                        score.Detail.SetAttribute("開課學分數", value);
                                                        hasChanged = true;
                                                    }
                                                    break;
                                                case "分項類別":
                                                    if (score.Detail.GetAttribute("開課分項類別") != value)
                                                    {
                                                        score.Detail.SetAttribute("開課分項類別", value);
                                                        hasChanged = true;
                                                    }
                                                    break;
                                                case "必選修":
                                                    if (score.Detail.GetAttribute("修課必選修") != value)
                                                    {
                                                        score.Detail.SetAttribute("修課必選修", value);
                                                        hasChanged = true;
                                                    }
                                                    break;
                                                case "校部訂":
                                                    if (score.Detail.GetAttribute("修課校部訂") != value)
                                                    {
                                                        score.Detail.SetAttribute("修課校部訂", value);
                                                        hasChanged = true;
                                                    }
                                                    break;
                                                case "取得學分":
                                                    if (score.Detail.GetAttribute("是否取得學分") != value)
                                                    {
                                                        score.Detail.SetAttribute("是否取得學分", value);
                                                        hasChanged = true;
                                                    }
                                                    break;

                                                case "手動調整成績":
                                                    if (score.Detail.GetAttribute("擇優採計成績") != value)
                                                    {
                                                        score.Detail.SetAttribute("擇優採計成績", value);
                                                        hasChanged = true;
                                                    }
                                                    break;
                                                case "原始成績":
                                                case "補考成績":
                                                case "重修成績":
                                                case "學年調整成績":
                                                    if (score.Detail.GetAttribute(field) != value)
                                                    {
                                                        score.Detail.SetAttribute(field, value);
                                                        hasChanged = true;
                                                    }
                                                    break;
                                                case "不計學分":
                                                case "不需評分":
                                                case "是否補修成績":
                                                    value = (value == "" ? "否" : value);
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
                                                        semesterGradeYear[sy][se] = gy;
                                                        hasChanged = true;
                                                    }
                                                    break;
                                                case "註記":
                                                case "重修學年度":
                                                case "重修學期":
                                                    if (score.Detail.GetAttribute(field) != value)
                                                    {
                                                        score.Detail.SetAttribute(field, value);
                                                        hasChanged = true;
                                                    }
                                                    break;
                                                case "英文名稱":
                                                    if (score.Detail.GetAttribute(field) != value)
                                                    {
                                                        score.Detail.SetAttribute(field, value);
                                                        hasChanged = true;
                                                    }
                                                    break;

                                            }
                                        }
                                        #endregion
                                        if (autoCheckPass.Checked)
                                        {
                                            int gy = int.Parse(data["成績年級"]);
                                            #region 做取得學分判斷及填入擇優採計成績
                                            //最高分
                                            decimal maxScore = decimal.MinValue;
                                            #region 抓最高分
                                            string[] scoreNames = new string[] { "原始成績", "學年調整成績", "擇優採計成績", "補考成績", "重修成績" };
                                            foreach (string scorename in scoreNames)
                                            {
                                                decimal s;
                                                if (decimal.TryParse(score.Detail.GetAttribute(scorename), out s))
                                                {
                                                    if (s > maxScore)
                                                    {
                                                        maxScore = s;
                                                    }
                                                }
                                            }
                                            #endregion
                                            #endregion
                                            score.Detail.SetAttribute("是否取得學分", ((score.Detail.GetAttribute("不需評分") == "是") || maxScore >= _StudentPassScore[studentRec][gy]) ? "是" : "否");
                                        }
                                    }
                                    else//加入新成績至已存在的學期
                                    {
                                        //加入學期年級
                                        int gy = int.Parse(data["成績年級"]);
                                        if (!semesterGradeYear.ContainsKey(sy))
                                            semesterGradeYear.Add(sy, new Dictionary<int, int>());
                                        if (!semesterGradeYear[sy].ContainsKey(se))
                                            semesterGradeYear[sy].Add(se, gy);
                                        else
                                            semesterGradeYear[sy][se] = gy;
                                        //加入新成績至已存在的學期
                                        if (!updatedNewSemesterScore.ContainsKey(sy))
                                            updatedNewSemesterScore.Add(sy, new Dictionary<int, List<RowData>>());
                                        if (!updatedNewSemesterScore[sy].ContainsKey(se))
                                            updatedNewSemesterScore[sy].Add(se, new List<RowData>());
                                        updatedNewSemesterScore[sy][se].Add(data);
                                        hasChanged = true;
                                    }
                                    //真的有變更
                                    if (hasChanged)
                                    {
                                        #region 登錄有變更的學期
                                        if (!updatedSemester.ContainsKey(sy))
                                            updatedSemester.Add(sy, new List<int>());
                                        if (!updatedSemester[sy].Contains(se))
                                            updatedSemester[sy].Add(se);
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    //處理已登錄要更新的學期成績
                    #region 處理已登錄要更新的學期成績
                    foreach (int sy in updatedSemester.Keys)
                    {
                        foreach (int se in updatedSemester[sy])
                        {
                            Dictionary<int, Dictionary<int, string>> semeScoreID = (Dictionary<int, Dictionary<int, string>>)studentRec.Fields["SemesterSubjectScoreID"];
                            string semesterScoreID = semeScoreID[sy][se];//從學期抓ID
                            string gradeyear = "" + semesterGradeYear[sy][se];//抓年級
                            XmlElement subjectScoreInfo = doc.CreateElement("SemesterSubjectScoreInfo");
                            #region 產生該學期科目成績的XML
                            foreach (SemesterSubjectScoreInfo scoreInfo in semesterScoreDictionary[sy][se].Values)
                            {
                                subjectScoreInfo.AppendChild(doc.ImportNode(scoreInfo.Detail, true));
                            }
                            if (updatedNewSemesterScore.ContainsKey(sy) && updatedNewSemesterScore[sy].ContainsKey(se))
                            {
                                foreach (RowData row in updatedNewSemesterScore[sy][se])
                                {
                                    XmlElement newScore = doc.CreateElement("Subject");
                                    #region 建立newScore
                                    foreach (string field in new string[] { "科目", "科目級別", "學分數", "分項類別", "必選修", "校部訂", "原始成績", "補考成績", "重修成績", "手動調整成績", "學年調整成績", "取得學分", "不計學分", "不需評分" })
                                    {
                                        if (e.ImportFields.Contains(field))
                                        {
                                            string value = row[field];
                                            switch (field)
                                            {
                                                default: break;
                                                case "科目":
                                                    newScore.SetAttribute("科目", value);
                                                    break;
                                                case "科目級別":
                                                    int level;
                                                    if (int.TryParse(value, out level))
                                                        newScore.SetAttribute("科目級別", "" + level);
                                                    else
                                                        newScore.SetAttribute("科目級別", "");
                                                    break;
                                                case "學分數":
                                                    newScore.SetAttribute("開課學分數", value);
                                                    break;
                                                case "分項類別":
                                                    newScore.SetAttribute("開課分項類別", value);
                                                    break;
                                                case "必選修":
                                                    newScore.SetAttribute("修課必選修", value);
                                                    break;
                                                case "校部訂":
                                                    newScore.SetAttribute("修課校部訂", value);
                                                    break;
                                                case "取得學分":
                                                    newScore.SetAttribute("是否取得學分", value);
                                                    break;

                                                case "手動調整成績":
                                                    newScore.SetAttribute("擇優採計成績", value);
                                                    break;
                                                case "註記":
                                                case "重修學年度":
                                                case "重修學期":
                                                case "英文名稱":
                                                case "原始成績":
                                                case "補考成績":
                                                case "重修成績":
                                                case "學年調整成績":
                                                    newScore.SetAttribute(field, value);
                                                    break;
                                                case "不計學分":
                                                case "不需評分":
                                                case "是否補修成績":
                                                    value = (value == "" ? "否" : value);
                                                    newScore.SetAttribute(field, value);
                                                    break;
                                            }
                                        }
                                    }
                                    //這兩個是為二有預設值的
                                    foreach (string field in new string[] { "不計學分", "不需評分" })
                                    {
                                        if (!e.ImportFields.Contains(field))
                                        {
                                            newScore.SetAttribute(field, "否");
                                        }
                                    }
                                    if (autoCheckPass.Checked)
                                    {
                                        int gy = int.Parse(row["成績年級"]);
                                        #region 做取得學分判斷及填入擇優採計成績
                                        //最高分
                                        decimal maxScore = decimal.MinValue;
                                        #region 抓最高分
                                        string[] scoreNames = new string[] { "原始成績", "學年調整成績", "擇優採計成績", "補考成績", "重修成績" };
                                        foreach (string scorename in scoreNames)
                                        {
                                            decimal s;
                                            if (decimal.TryParse(newScore.GetAttribute(scorename), out s))
                                            {
                                                if (s > maxScore)
                                                {
                                                    maxScore = s;
                                                }
                                            }
                                        }
                                        #endregion
                                        #endregion
                                        newScore.SetAttribute("是否取得學分", ((newScore.GetAttribute("不需評分") == "是") || maxScore >= _StudentPassScore[studentRec][gy]) ? "是" : "否");
                                    }
                                    #endregion
                                    subjectScoreInfo.AppendChild(newScore);
                                }
                            }
                            #endregion
                            updateList.Add(new SmartSchool.Feature.Score.EditScore.UpdateInfo(semesterScoreID, gradeyear, subjectScoreInfo));
                        }
                    }
                    #endregion
                    //處理新增成績學期
                    #region 處理新增成績學期
                    foreach (int sy in insertNewSemesterScore.Keys)
                    {
                        foreach (int se in insertNewSemesterScore[sy].Keys)
                        {
                            XmlElement subjectScoreInfo = doc.CreateElement("SemesterSubjectScoreInfo");
                            string gradeyear = "" + semesterGradeYear[sy][se];//抓年級
                            foreach (RowData row in insertNewSemesterScore[sy][se])
                            {
                                XmlElement newScore = doc.CreateElement("Subject");
                                #region 建立newScore
                                foreach (string field in new string[] { "科目", "科目級別", "學分數", "分項類別", "必選修", "校部訂", "原始成績", "補考成績", "重修成績", "手動調整成績", "學年調整成績", "取得學分", "不計學分", "不需評分", "是否補修成績", "重修學年度", "重修學期" })
                                {
                                    if (e.ImportFields.Contains(field))
                                    {
                                        string value = row[field];
                                        switch (field)
                                        {
                                            default: break;
                                            case "科目":
                                                newScore.SetAttribute("科目", value);
                                                break;
                                            case "科目級別":
                                                int level;
                                                if (int.TryParse(value, out level))
                                                    newScore.SetAttribute("科目級別", "" + level);
                                                else
                                                    newScore.SetAttribute("科目級別", "");
                                                break;
                                            case "學分數":
                                                newScore.SetAttribute("開課學分數", value);
                                                break;
                                            case "分項類別":
                                                newScore.SetAttribute("開課分項類別", value);
                                                break;
                                            case "必選修":
                                                newScore.SetAttribute("修課必選修", value);
                                                break;
                                            case "校部訂":
                                                newScore.SetAttribute("修課校部訂", value);
                                                break;
                                            case "取得學分":
                                                newScore.SetAttribute("是否取得學分", value);
                                                break;

                                            case "手動調整成績":
                                                newScore.SetAttribute("擇優採計成績", value);
                                                break;
                                            case "原始成績":
                                            case "補考成績":
                                            case "重修成績":
                                            case "學年調整成績":
                                            case "重修學年度":
                                            case "重修學期":
                                                newScore.SetAttribute(field, value);
                                                break;
                                            case "不計學分":
                                            case "不需評分":
                                            case "是否補修成績":
                                                value = (value == "" ? "否" : value);
                                                newScore.SetAttribute(field, value);
                                                break;
                                        }
                                    }
                                }
                                //這兩個是為二有預設值的
                                foreach (string field in new string[] { "不計學分", "不需評分" })
                                {
                                    if (!e.ImportFields.Contains(field))
                                    {
                                        newScore.SetAttribute(field, "否");
                                    }
                                }

                                if (autoCheckPass.Checked)
                                {
                                    int gy = int.Parse(row["成績年級"]);
                                    #region 做取得學分判斷及填入擇優採計成績
                                    //最高分
                                    decimal maxScore = decimal.MinValue;
                                    #region 抓最高分
                                    string[] scoreNames = new string[] { "原始成績", "學年調整成績", "擇優採計成績", "補考成績", "重修成績" };
                                    foreach (string scorename in scoreNames)
                                    {
                                        decimal s;
                                        if (decimal.TryParse(newScore.GetAttribute(scorename), out s))
                                        {
                                            if (s > maxScore)
                                            {
                                                maxScore = s;
                                            }
                                        }
                                    }
                                    #endregion
                                    #endregion
                                    newScore.SetAttribute("是否取得學分", ((newScore.GetAttribute("不需評分") == "是") || maxScore >= _StudentPassScore[studentRec][gy]) ? "是" : "否");
                                }
                                #endregion
                                subjectScoreInfo.AppendChild(newScore);
                            }
                            insertList.Add(new SmartSchool.Feature.Score.AddScore.InsertInfo(studentRec.StudentID, "" + sy, "" + se, gradeyear, "", subjectScoreInfo));
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
                    Thread threadUpdateSemesterSubjectScore = new Thread(new ParameterizedThreadStart(updateSemesterSubjectScore));
                    threadUpdateSemesterSubjectScore.IsBackground = true;
                    threadUpdateSemesterSubjectScore.Start(updatePackages);
                    Thread threadUpdateSemesterSubjectScore2 = new Thread(new ParameterizedThreadStart(updateSemesterSubjectScore));
                    threadUpdateSemesterSubjectScore2.IsBackground = true;
                    threadUpdateSemesterSubjectScore2.Start(updatePackages2);

                    threadUpdateSemesterSubjectScore.Join();
                    threadUpdateSemesterSubjectScore2.Join();
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
                    Thread threadInsertSemesterSubjectScore = new Thread(new ParameterizedThreadStart(insertSemesterSubjectScore));
                    threadInsertSemesterSubjectScore.IsBackground = true;
                    threadInsertSemesterSubjectScore.Start(insertPackages);
                    Thread threadInsertSemesterSubjectScore2 = new Thread(new ParameterizedThreadStart(insertSemesterSubjectScore));
                    threadInsertSemesterSubjectScore2.IsBackground = true;
                    threadInsertSemesterSubjectScore2.Start(insertPackages2);

                    threadInsertSemesterSubjectScore.Join();
                    threadInsertSemesterSubjectScore2.Join();
                    #endregion
                }
                #endregion
            };
            wizard.ImportComplete += delegate
            {
                EventHub.Instance.InvokScoreChanged(new List<string>(_StudentCollection.Keys).ToArray());
            };
        }


        private void updateSemesterSubjectScore(object item)
        {
            List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>> updatePackages = (List<List<SmartSchool.Feature.Score.EditScore.UpdateInfo>>)item;
            foreach (List<SmartSchool.Feature.Score.EditScore.UpdateInfo> package in updatePackages)
            {
                SmartSchool.Feature.Score.EditScore.UpdateSemesterSubjectScore(package.ToArray());
            }
        }

        private void insertSemesterSubjectScore(object item)
        {
            List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>> insertPackages = (List<List<SmartSchool.Feature.Score.AddScore.InsertInfo>>)item;
            foreach (List<SmartSchool.Feature.Score.AddScore.InsertInfo> package in insertPackages)
            {
                SmartSchool.Feature.Score.AddScore.InsertSemesterSubjectScore(package.ToArray());
            }
        }
    }
}