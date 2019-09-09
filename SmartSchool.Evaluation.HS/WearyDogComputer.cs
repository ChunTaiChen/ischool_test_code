using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using FISCA.DSAUtil;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Evaluation.WearyDogComputerHelper;
using FISCA.Data;
using System.Data;
using System.Xml.Linq;

namespace SmartSchool.Evaluation
{
    /// <summary>
    /// 成績計算及畢業判斷檢查類別
    /// </summary>
    public class WearyDogComputer
    {
        /// <summary>
        /// 異常課程提示清單
        /// </summary>
        public List<string> _WarningList;

        /// <summary>
        /// 進位方式
        /// </summary>
        public enum RoundMode { 四捨五入, 無條件進位, 無條件捨去 }

        /// <summary>
        /// 根據進位條件取得成績
        /// </summary>
        /// <param name="score">成績</param>
        /// <param name="decimals">小數點</param>
        /// <param name="mode">進位模式</param>
        /// <returns></returns>
        internal static decimal GetRoundScore(decimal score, int decimals, RoundMode mode)
        {
            decimal seed = Convert.ToDecimal(Math.Pow(0.1, Convert.ToDouble(decimals)));
            switch (mode)
            {
                default:
                case RoundMode.四捨五入:
                    score = decimal.Round(score, decimals, MidpointRounding.AwayFromZero);
                    break;
                case RoundMode.無條件捨去:
                    score /= seed;
                    score = decimal.Floor(score);
                    score *= seed;
                    break;
                case RoundMode.無條件進位:
                    decimal d2 = GetRoundScore(score, decimals, RoundMode.無條件捨去);
                    if (d2 != score)
                        score = d2 + seed;
                    else
                        score = d2;
                    break;
            }
            string ss = "0.";
            for (int i = 0; i < decimals; i++)
            {
                ss += "0";
            }
            return Convert.ToDecimal(Math.Round(score, decimals).ToString(ss));
        }

        /// <summary>
        /// 計算學期科目成績
        /// </summary>
        /// <param name="schoolyear">學年度</param>
        /// <param name="semester">學期</param>
        /// <param name="accesshelper">資料存取類別</param>
        /// <param name="students">學生物件列表</param>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> FillSemesterSubjectCalcScore(int schoolyear, int semester, AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //抓學生學期歷程
            accesshelper.StudentHelper.FillField("SemesterHistory", students);
            //抓學生學期修課紀錄
            accesshelper.StudentHelper.FillAttendCourse(schoolyear, semester, students);
            //抓學生歷年學期科目成績
            accesshelper.StudentHelper.FillSemesterSubjectScore(false, students);

            // 2018/5/24 穎驊新增 紀錄 於 [H成績][06] 課程重讀重修設定，的新設定，假如學生有重覆科目級別被抓出來，且設定了計算方式，
            // 此字典可用於對照， 格式 <studentID_subject_level,重修(寫回原學期)||重讀(擇優採計成績)||視為一般修課>
            Dictionary<string, string> duplicateSubjectLevelMethodDict = new Dictionary<string, string>();

            Dictionary<string, string> duplicateSubjectLevelMethodDict_Afterfilter = new Dictionary<string, string>(); // 真正過濾後，有重覆科目級別的項目


            // 整理所欲計算學期科目學生的ID
            List<string> sidList = new List<string>();
            
            foreach (StudentRecord sr in students)
            {
                if (!sidList.Contains(sr.StudentID))
                {
                    sidList.Add(sr.StudentID);
                }
            }

            string sid = string.Join(",", sidList);

            string query2 = string.Format(@"SELECT sc_attend.id
	,sc_attend.extensions AS extensions
	,student.id AS refStudentID	
	,student.name AS studentName
	,student.student_number AS studentNumber
	,student.seat_no AS seatNo  
	,class.class_name AS className
	,class.grade_year AS gradeYear
	,course.id AS refCourseID
    ,course.course_name AS courseName
	,course.subject AS subjectName
	,course.subj_level AS subjectLevel
	FROM sc_attend 
	LEFT JOIN student ON sc_attend.ref_student_id =student.id 
	LEFT JOIN class ON student.ref_class_id =class.id  
	LEFT JOIN course ON sc_attend.ref_course_id =course.id  
	WHERE 
	student.status ='1' 
	AND course.school_year = '{0}'
    AND course.semester = '{1}'
    AND student.id IN ({2})
    ORDER BY courseName,className, seatNo ASC", schoolyear, semester, sid);

            QueryHelper qh1 = new QueryHelper();

            DataTable dt_SCAttend = qh1.Select(query2);

            // 將學生本學期的修課紀錄整理出來 (包含了重覆科目級別的計算處理方式會在extensions內)
            foreach (DataRow dr in dt_SCAttend.Rows)
            {
                string key = "" + dr["refStudentID"] + "_" + dr["subjectName"] + "_" + dr["subjectLevel"];

                string xmlStr = "<root>" + dr["extensions"] + "</root>";
                string method = "";

                XElement elmRoot = XElement.Parse(xmlStr);

                if (elmRoot != null)
                {
                    if (elmRoot.Element("Extensions") != null)
                    {
                        foreach (XElement ex in elmRoot.Element("Extensions").Elements("Extension"))
                        {
                            if (ex.Attribute("Name").Value == "DuplicatedLevelSubjectCalRule")
                            {
                                method = ex.Element("Rule").Value;
                            }
                        }
                    }
                }
                if (!duplicateSubjectLevelMethodDict.ContainsKey(key))
                {
                    duplicateSubjectLevelMethodDict.Add("" + dr["refStudentID"] + "_" + dr["subjectName"] + "_" + dr["subjectLevel"], method);
                }
            }


            foreach (StudentRecord var in students)
            {
                //成績年級
                int? gradeYear = null;
                //精準位數
                int decimals = 2;
                //進位模式
                RoundMode mode = RoundMode.四捨五入;
                //使用擇優採計成績
                bool choseBetter = true;
                //重修登錄至原學期
                //bool writeToFirstSemester = false;
                //及格標準<年及,及格標準>
                Dictionary<int, decimal> applyLimit = new Dictionary<int, decimal>();
                //applyLimit.Add(1, 60);
                //applyLimit.Add(2, 60);
                //applyLimit.Add(3, 60);
                //applyLimit.Add(4, 60);
                //成績年級及計算規則皆存在，允許計算成績
                bool canCalc = true;
                #region 取得成績年級跟計算規則
                {
                    #region 處理成績年級
                    XmlElement semesterHistory = (XmlElement)var.Fields["SemesterHistory"];
                    if (semesterHistory == null)
                    {
                        if (!_ErrorList.ContainsKey(var))
                            _ErrorList.Add(var, new List<string>());
                        _ErrorList[var].Add("沒有學期歷程紀錄，無法判斷成績年級。");
                        canCalc &= false;
                    }
                    else
                    {
                        foreach (XmlElement history in new DSXmlHelper(semesterHistory).GetElements("History"))
                        {
                            int year, sems, gradeyear;
                            if (
                                int.TryParse(history.GetAttribute("SchoolYear"), out year) &&
                                int.TryParse(history.GetAttribute("Semester"), out sems) &&
                                int.TryParse(history.GetAttribute("GradeYear"), out gradeyear) &&
                                year == schoolyear && sems == semester
                                )
                                gradeYear = gradeyear;
                        }
                        if (gradeYear == null)
                        {
                            if (!_ErrorList.ContainsKey(var))
                                _ErrorList.Add(var, new List<string>());
                            _ErrorList[var].Add("學期歷程中沒有" + schoolyear + "學年度第" + semester + "學期的紀錄，無法判斷成績年級。");
                            canCalc &= false;
                        }
                    }
                    #endregion
                    #region 處理計算規則
                    XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                    if (scoreCalcRule == null)
                    {
                        if (!_ErrorList.ContainsKey(var))
                            _ErrorList.Add(var, new List<string>());
                        _ErrorList[var].Add("沒有設定成績計算規則。");
                        canCalc &= false;
                    }
                    else
                    {
                        DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                        bool tryParsebool;
                        int tryParseint;
                        decimal tryParseDecimal;

                        if (scoreCalcRule.SelectSingleNode("各項成績計算位數/科目成績計算位數") != null)
                        {
                            if (int.TryParse(helper.GetText("各項成績計算位數/科目成績計算位數/@位數"), out tryParseint))
                                decimals = tryParseint;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/科目成績計算位數/@四捨五入"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.四捨五入;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/科目成績計算位數/@無條件捨去"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.無條件捨去;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/科目成績計算位數/@無條件進位"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.無條件進位;
                        }
                        //if (scoreCalcRule.SelectSingleNode("延修及重讀成績處理規則/重讀成績") != null) 2018/5/29 穎驊註解 ，因應 [H成績][04] 計算學期科目成績調整 ，不再使用這個屬性 ，若有重讀，此屬性永遠是true，擇優
                        //{
                        //    if (bool.TryParse(helper.GetText("延修及重讀成績處理規則/重讀成績/@擇優採計成績"), out tryParsebool))
                        //        choseBetter = tryParsebool;
                        //}
                        //if (bool.TryParse(helper.GetText("重修成績/@登錄至原學期"), out tryParsebool)) 2018/5/29 穎驊註解 ，因應 [H成績][04] 計算學期科目成績調整 ，不再使用這個屬性，若有重覆科目級別，請至教務作業 /成績作業 課程重讀重修設定
                        //    writeToFirstSemester = tryParsebool;
                        foreach (XmlElement element in helper.GetElements("及格標準/學生類別"))
                        {
                            string cat = element.GetAttribute("類別");
                            bool useful = false;
                            //掃描學生的類別作比對
                            foreach (CategoryInfo catinfo in var.StudentCategorys)
                            {
                                if (catinfo.Name == cat || catinfo.FullName == cat)
                                    useful = true;
                            }
                            //學生是指定的類別或類別為"預設"
                            if (cat == "預設" || useful)
                            {
                                for (int gyear = 1; gyear <= 4; gyear++)
                                {
                                    switch (gyear)
                                    {
                                        case 1:
                                            if (decimal.TryParse(element.GetAttribute("一年級及格標準"), out tryParseDecimal))
                                            {
                                                if (!applyLimit.ContainsKey(gyear))
                                                    applyLimit.Add(gyear, tryParseDecimal);
                                                if (applyLimit[gyear] > tryParseDecimal)
                                                    applyLimit[gyear] = tryParseDecimal;
                                            }
                                            break;
                                        case 2:
                                            if (decimal.TryParse(element.GetAttribute("二年級及格標準"), out tryParseDecimal))
                                            {
                                                if (!applyLimit.ContainsKey(gyear))
                                                    applyLimit.Add(gyear, tryParseDecimal);
                                                if (applyLimit[gyear] > tryParseDecimal)
                                                    applyLimit[gyear] = tryParseDecimal;
                                            }
                                            break;
                                        case 3:
                                            if (decimal.TryParse(element.GetAttribute("三年級及格標準"), out tryParseDecimal))
                                            {
                                                if (!applyLimit.ContainsKey(gyear))
                                                    applyLimit.Add(gyear, tryParseDecimal);
                                                if (applyLimit[gyear] > tryParseDecimal)
                                                    applyLimit[gyear] = tryParseDecimal;
                                            }
                                            break;
                                        case 4:
                                            if (decimal.TryParse(element.GetAttribute("四年級及格標準"), out tryParseDecimal))
                                            {
                                                if (!applyLimit.ContainsKey(gyear))
                                                    applyLimit.Add(gyear, tryParseDecimal);
                                                if (applyLimit[gyear] > tryParseDecimal)
                                                    applyLimit[gyear] = tryParseDecimal;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                XmlDocument doc = new XmlDocument();
                if (var.Fields.ContainsKey("SemesterSubjectCalcScore"))
                    var.Fields.Remove("SemesterSubjectCalcScore");
                XmlElement semesterSubjectCalcScoreElement = doc.CreateElement("SemesterSubjectCalcScore");
                if (canCalc)
                {
                    //已存在的學期科目成績<學年度<學期,<科目+級別,成績>>>
                    Dictionary<int, Dictionary<int, Dictionary<string, SemesterSubjectScoreInfo>>> semesterSubjectScoreList = new Dictionary<int, Dictionary<int, Dictionary<string, SemesterSubjectScoreInfo>>>();
                    //當學年度已紀錄之成績
                    Dictionary<SemesterSubjectScoreInfo, string> currentSubjectScoreList = new Dictionary<SemesterSubjectScoreInfo, string>();
                    //重讀成績
                    Dictionary<SemesterSubjectScoreInfo, string> repeatSubjectScoreList = new Dictionary<SemesterSubjectScoreInfo, string>();
                    //重修成績
                    Dictionary<SemesterSubjectScoreInfo, string> restudySubjectScoreList = new Dictionary<SemesterSubjectScoreInfo, string>();
                    #region 先掃一遍把學生成績分類
                    foreach (SemesterSubjectScoreInfo scoreinfo in var.SemesterSubjectScoreList)
                    {
                        string key = scoreinfo.Subject.Trim() + "_" + scoreinfo.Level.Trim();

                        if (scoreinfo.SchoolYear == schoolyear)
                        {
                            if (scoreinfo.Semester == semester)
                                currentSubjectScoreList.Add(scoreinfo, key);
                            else if (scoreinfo.Semester < semester)
                            {
                                if (scoreinfo.GradeYear == (int)gradeYear)
                                    repeatSubjectScoreList.Add(scoreinfo, key);
                                else
                                    restudySubjectScoreList.Add(scoreinfo, key);
                            }
                        }
                        else if (scoreinfo.SchoolYear < schoolyear)
                        {
                            if (scoreinfo.GradeYear == (int)gradeYear)
                                repeatSubjectScoreList.Add(scoreinfo, key);
                            else
                                restudySubjectScoreList.Add(scoreinfo, key);
                        }

                        if (duplicateSubjectLevelMethodDict.ContainsKey(var.StudentID + "_" + key) && !duplicateSubjectLevelMethodDict_Afterfilter.ContainsKey(var.StudentID + "_" + key) && !currentSubjectScoreList.ContainsValue(key)) //假如有key 又非本學期的成績(可能本學期末已經先算過一次了) 則加入重覆計算的規則
                        {
                            duplicateSubjectLevelMethodDict_Afterfilter.Add(var.StudentID + "_" + key, duplicateSubjectLevelMethodDict[var.StudentID + "_" + key]);
                        }

                        if (!semesterSubjectScoreList.ContainsKey(scoreinfo.SchoolYear))
                            semesterSubjectScoreList.Add(scoreinfo.SchoolYear, new Dictionary<int, Dictionary<string, SemesterSubjectScoreInfo>>());
                        if (!semesterSubjectScoreList[scoreinfo.SchoolYear].ContainsKey(scoreinfo.Semester))
                            semesterSubjectScoreList[scoreinfo.SchoolYear].Add(scoreinfo.Semester, new Dictionary<string, SemesterSubjectScoreInfo>());
                        if (!semesterSubjectScoreList[scoreinfo.SchoolYear][scoreinfo.Semester].ContainsKey(key))
                            semesterSubjectScoreList[scoreinfo.SchoolYear][scoreinfo.Semester].Add(key, scoreinfo);
                        else
                            semesterSubjectScoreList[scoreinfo.SchoolYear][scoreinfo.Semester][key] = scoreinfo;
                    }
                    #endregion
                    #region 移除重讀跟重修成績重複年級的學期成績
                    CleanUpRepeat(repeatSubjectScoreList);
                    CleanUpRepeat(restudySubjectScoreList);
                    #endregion
                    //新增的學期成績資料
                    Dictionary<int, Dictionary<int, Dictionary<string, XmlElement>>> insertSemesterSubjectScoreList = new Dictionary<int, Dictionary<int, Dictionary<string, XmlElement>>>();
                    //修改的學期成績資料
                    Dictionary<int, Dictionary<int, Dictionary<string, XmlElement>>> updateSemesterSubjectScoreList = new Dictionary<int, Dictionary<int, Dictionary<string, XmlElement>>>();
                    #region 掃描修課紀錄填入新增或修改的清單中
                    foreach (StudentAttendCourseRecord sacRecord in var.AttendCourseList)
                    {
                        if (!sacRecord.HasFinalScore && !sacRecord.NotIncludedInCalc)
                        {
                            if (!_ErrorList.ContainsKey(var))
                                _ErrorList.Add(var, new List<string>());
                            _ErrorList[var].Add("" + sacRecord.CourseName + "沒有修課總成績，無法計算。"); 
                            continue;
                        }
                        string key = sacRecord.Subject.Trim() + "_" + sacRecord.SubjectLevel.Trim();

                        if (duplicateSubjectLevelMethodDict_Afterfilter.ContainsKey(var.StudentID + "_" + key) ? duplicateSubjectLevelMethodDict_Afterfilter[var.StudentID + "_" + key] == "" : false) // 如果使用者 沒有設定，要擋下，逼他們設定完畢才可以計算完畢
                        {
                            int sy = 0, se = 0;
                            #region 找到最近一次修課紀錄
                            foreach (SemesterSubjectScoreInfo si in restudySubjectScoreList.Keys)
                            {
                                if (restudySubjectScoreList[si] == key)
                                {
                                    if (si.SchoolYear > sy || (si.SchoolYear == sy && si.Semester > se))
                                    {
                                        sy = si.SchoolYear;
                                        se = si.Semester;                                        
                                    }
                                }
                            }
                            #endregion

                            if (!_ErrorList.ContainsKey(var))
                                _ErrorList.Add(var, new List<string>());
                            _ErrorList[var].Add("課程名稱 :" + sacRecord.CourseName + "， 已於"+ sy +"學年度 第" +se +"學期修習過，請至 教務作業/成績作業 重覆修課採計方式 設定。");
                            continue;
                        }

                        //發現為重修科目
                        //if (writeToFirstSemester && restudySubjectScoreList.ContainsValue(key))
                        if (duplicateSubjectLevelMethodDict_Afterfilter.ContainsKey(var.StudentID + "_" + key)?duplicateSubjectLevelMethodDict_Afterfilter[var.StudentID + "_" + key] == "重修(寫回原學期)" : false)// 因應[H成績][04] 計算學期科目成績調整
                        {
                            #region 寫入重修成績回原學期
                            int sy = 0, se = 0;
                            SemesterSubjectScoreInfo updateScoreInfo = null;
                            #region 找到最近一次修課紀錄
                            foreach (SemesterSubjectScoreInfo si in restudySubjectScoreList.Keys)
                            {
                                if (restudySubjectScoreList[si] == key)
                                {
                                    if (si.SchoolYear > sy || (si.SchoolYear == sy && si.Semester > se))
                                    {
                                        sy = si.SchoolYear;
                                        se = si.Semester;
                                        updateScoreInfo = si;
                                    }
                                }
                            }
                            #endregion
                            if (!updateSemesterSubjectScoreList.ContainsKey(sy) || !updateSemesterSubjectScoreList[sy].ContainsKey(se) || !updateSemesterSubjectScoreList[sy][se].ContainsKey(key))
                            {
                                //寫入重修紀錄
                                XmlElement updateScoreElement = updateScoreInfo.Detail;
                                updateScoreElement.SetAttribute("重修成績", "" + GetRoundScore(sacRecord.FinalScore, decimals, mode));
                                //做取得學分判斷
                                #region 做取得學分判斷
                                //最高分
                                decimal maxScore = sacRecord.FinalScore;
                                #region 抓最高分

                                string[] scoreNames = new string[] { "原始成績", "學年調整成績", "擇優採計成績", "補考成績", "重修成績" };

                              

                                
                                foreach (string scorename in scoreNames)
                                {
                                    decimal s;
                                    if (decimal.TryParse(updateScoreElement.GetAttribute(scorename), out s))
                                    {
                                        if (s > maxScore)
                                        {
                                            maxScore = s;
                                        }
                                    }
                                }
                                #endregion
                                decimal passscore;
                                if (!applyLimit.ContainsKey(updateScoreInfo.GradeYear))
                                    passscore = 60;
                                else
                                    passscore = applyLimit[updateScoreInfo.GradeYear];
                                updateScoreElement.SetAttribute("是否取得學分", (updateScoreElement.GetAttribute("不需評分") == "是" || maxScore >= passscore) ? "是" : "否");
                                #endregion
                                if (!updateSemesterSubjectScoreList.ContainsKey(sy)) updateSemesterSubjectScoreList.Add(sy, new Dictionary<int, Dictionary<string, XmlElement>>());
                                if (!updateSemesterSubjectScoreList[sy].ContainsKey(se)) updateSemesterSubjectScoreList[sy].Add(se, new Dictionary<string, XmlElement>());
                                updateSemesterSubjectScoreList[sy][se].Add(key, updateScoreElement);
                            }
                            #endregion
                        }
                        else
                        {
                            //填入本學期科目成績
                            if (currentSubjectScoreList.ContainsValue(key))
                            {
                                #region 修改此學期已存在之成績
                                SemesterSubjectScoreInfo updateScoreInfo = null;
                                foreach (SemesterSubjectScoreInfo s in currentSubjectScoreList.Keys)
                                {
                                    if (currentSubjectScoreList[s] == key)
                                    {
                                        updateScoreInfo = s;
                                        break;
                                    }
                                }
                                int sy = schoolyear, se = semester;
                                if (!updateSemesterSubjectScoreList.ContainsKey(sy) || !updateSemesterSubjectScoreList[sy].ContainsKey(se) || !updateSemesterSubjectScoreList[sy][se].ContainsKey(key))
                                {
                                    //修改成績
                                    XmlElement updateScoreElement = updateScoreInfo.Detail;
                                    #region 重新填入課程資料


                                    updateScoreElement.SetAttribute("不計學分", sacRecord.NotIncludedInCredit ? "是" : "否");
                                    updateScoreElement.SetAttribute("不需評分", sacRecord.NotIncludedInCalc ? "是" : "否");
                                    updateScoreElement.SetAttribute("修課必選修", sacRecord.Required ? "必修" : "選修");
                                    updateScoreElement.SetAttribute("修課校部訂", (sacRecord.RequiredBy == "部訂" ? sacRecord.RequiredBy : "校訂"));
                                    updateScoreElement.SetAttribute("科目", sacRecord.Subject);
                                    updateScoreElement.SetAttribute("科目級別", sacRecord.SubjectLevel);
                                    updateScoreElement.SetAttribute("開課分項類別", sacRecord.Entry);

                                    updateScoreElement.SetAttribute("開課學分數", "" + sacRecord.CreditDec());


                                    #endregion
                                    updateScoreElement.SetAttribute("原始成績", (sacRecord.NotIncludedInCalc ? "" : "" + GetRoundScore(sacRecord.FinalScore, decimals, mode)));
                                    //做取得學分判斷
                                    #region 做取得學分判斷及填入擇優採計成績
                                    //最高分
                                    decimal maxScore = sacRecord.FinalScore;
                                    #region 抓最高分
                                    string[] scoreNames = new string[] { "原始成績", "學年調整成績", "擇優採計成績", "補考成績", "重修成績" };
                                    foreach (string scorename in scoreNames)
                                    {
                                        decimal s;
                                        if (decimal.TryParse(updateScoreElement.GetAttribute(scorename), out s))
                                        {
                                            if (s > maxScore)
                                            {
                                                maxScore = s;
                                            }
                                        }
                                    }
                                    #endregion
                                    //如果有擇優採計成績且重讀學期有修過課
                                    if (duplicateSubjectLevelMethodDict_Afterfilter.ContainsKey(var.StudentID + "_" + key) ? duplicateSubjectLevelMethodDict_Afterfilter[var.StudentID + "_" + key] == "重讀(擇優採計成績)":false)
                                    {
                                        #region 填入擇優採計成績
                                        foreach (SemesterSubjectScoreInfo s in repeatSubjectScoreList.Keys)
                                        {
                                            //之前的成績比現在的成績好
                                            if (repeatSubjectScoreList[s] == key && s.Score > maxScore)
                                            {
                                                updateScoreElement.SetAttribute("原始成績", "" + GetRoundScore(s.Score, decimals, mode));
                                                updateScoreElement.SetAttribute("註記", "修課成績：" + sacRecord.FinalScore);
                                                maxScore = s.Score;
                                            }
                                        }
                                        #endregion
                                    }
                                    decimal passscore;
                                    if (!applyLimit.ContainsKey(updateScoreInfo.GradeYear))
                                        passscore = 60;
                                    else
                                        passscore = applyLimit[updateScoreInfo.GradeYear];
                                    updateScoreElement.SetAttribute("是否取得學分", (sacRecord.NotIncludedInCalc || maxScore >= passscore) ? "是" : "否");
                                    #endregion
                                    if (!updateSemesterSubjectScoreList.ContainsKey(sy)) updateSemesterSubjectScoreList.Add(sy, new Dictionary<int, Dictionary<string, XmlElement>>());
                                    if (!updateSemesterSubjectScoreList[sy].ContainsKey(se)) updateSemesterSubjectScoreList[sy].Add(se, new Dictionary<string, XmlElement>());
                                    updateSemesterSubjectScoreList[sy][se].Add(key, updateScoreElement);
                                }
                                #endregion
                            }
                            else
                            {
                                #region 新增一筆成績
                                int sy = schoolyear, se = semester;
                                if (!insertSemesterSubjectScoreList.ContainsKey(sy) || !insertSemesterSubjectScoreList[sy].ContainsKey(se) || !insertSemesterSubjectScoreList[sy][se].ContainsKey(key))
                                {
                                    //允許的分項類別清單
                                    List<string> entrys = new List<string>(new string[] { "學業", "體育", "國防通識", "健康與護理", "實習科目", "專業科目" });

                                    //科目名稱空白或分項類別有錯誤時執行
                                    if (string.IsNullOrEmpty(sacRecord.Subject) || !entrys.Contains(sacRecord.Entry))
                                    {
                                        //_WarningList為空就先建立
                                        if (_WarningList == null) _WarningList = new List<string>();
                                        //_WarningList不存在此課程名稱才加入
                                        if (!_WarningList.Contains(sacRecord.CourseName))
                                        {
                                            _WarningList.Add(sacRecord.CourseName);
                                        }
                                        continue;
                                    }

                                    #region 加入新的資料
                                    XmlElement newScoreInfo = doc.CreateElement("Subject");
                                    newScoreInfo.SetAttribute("不計學分", sacRecord.NotIncludedInCredit ? "是" : "否");
                                    newScoreInfo.SetAttribute("不需評分", sacRecord.NotIncludedInCalc ? "是" : "否");
                                    newScoreInfo.SetAttribute("修課必選修", sacRecord.Required ? "必修" : "選修");
                                    newScoreInfo.SetAttribute("修課校部訂", (sacRecord.RequiredBy == "部訂" ? sacRecord.RequiredBy : "校訂"));
                                    newScoreInfo.SetAttribute("科目", sacRecord.Subject);
                                    newScoreInfo.SetAttribute("科目級別", sacRecord.SubjectLevel);
                                    newScoreInfo.SetAttribute("開課分項類別", sacRecord.Entry);
                                    newScoreInfo.SetAttribute("開課學分數", "" + sacRecord.CreditDec());
                                    newScoreInfo.SetAttribute("原始成績", (sacRecord.NotIncludedInCalc ? "" : "" + GetRoundScore(sacRecord.FinalScore, decimals, mode)));
                                    newScoreInfo.SetAttribute("重修成績", "");
                                    newScoreInfo.SetAttribute("學年調整成績", "");
                                    newScoreInfo.SetAttribute("擇優採計成績", "");
                                    newScoreInfo.SetAttribute("補考成績", "");
                                    //做取得學分判斷
                                    #region 做取得學分判斷及填入擇優採計成績
                                    //最高分
                                    decimal maxScore = sacRecord.FinalScore;
                                    #region 抓最高分
                                    string[] scoreNames = new string[] { "原始成績", "學年調整成績", "擇優採計成績", "補考成績", "重修成績" };
                                    foreach (string scorename in scoreNames)
                                    {
                                        decimal s;
                                        if (decimal.TryParse(newScoreInfo.GetAttribute(scorename), out s))
                                        {
                                            if (s > maxScore)
                                            {
                                                maxScore = s;
                                            }
                                        }
                                    }
                                    #endregion

                                    //如果有擇優採計成績且重讀學期有修過課
                                    if (duplicateSubjectLevelMethodDict_Afterfilter.ContainsKey(var.StudentID + "_" + key) ? duplicateSubjectLevelMethodDict_Afterfilter[var.StudentID + "_" + key] == "重讀(擇優採計成績)" : false)
                                    {
                                        #region 填入擇優採計成績
                                        foreach (SemesterSubjectScoreInfo s in repeatSubjectScoreList.Keys)
                                        {
                                            //之前的成績比現在的成績好
                                            if (repeatSubjectScoreList[s] == key && s.Score > maxScore)
                                            {
                                                //newScoreInfo.SetAttribute("擇優採計成績", "" + GetRoundScore(s.Score, decimals, mode));
                                                newScoreInfo.SetAttribute("原始成績", "" + GetRoundScore(s.Score, decimals, mode));
                                                newScoreInfo.SetAttribute("註記", "修課成績：" + sacRecord.FinalScore);
                                                maxScore = s.Score;
                                            }
                                        }
                                        #endregion
                                    }
                                    decimal passscore;
                                    if (!applyLimit.ContainsKey((int)gradeYear))
                                        passscore = 60;
                                    else
                                        passscore = applyLimit[(int)gradeYear];
                                    #endregion
                                    newScoreInfo.SetAttribute("是否取得學分", (sacRecord.NotIncludedInCalc || maxScore >= passscore) ? "是" : "否");
                                    #endregion
                                    if (!insertSemesterSubjectScoreList.ContainsKey(sy)) insertSemesterSubjectScoreList.Add(sy, new Dictionary<int, Dictionary<string, XmlElement>>());
                                    if (!insertSemesterSubjectScoreList[sy].ContainsKey(se)) insertSemesterSubjectScoreList[sy].Add(se, new Dictionary<string, XmlElement>());
                                    insertSemesterSubjectScoreList[sy][se].Add(key, newScoreInfo);
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                    #region 從新增跟修改清單中產生變動資料
                    //抓取暗藏的學生學期成績編號資料
                    Dictionary<int, Dictionary<int, string>> semeScoreID = (Dictionary<int, Dictionary<int, string>>)var.Fields["SemesterSubjectScoreID"];
                    foreach (int sy in updateSemesterSubjectScoreList.Keys)
                    {
                        foreach (int se in updateSemesterSubjectScoreList[sy].Keys)
                        {
                            List<string> appendedKeys = new List<string>();
                            XmlElement parentNode;
                            parentNode = doc.CreateElement("UpdateSemesterScore");
                            parentNode.SetAttribute("ID", semeScoreID[sy][se]);
                            if (sy == schoolyear && se == semester)
                                parentNode.SetAttribute("GradeYear", "" + gradeYear);
                            //加入修改過的成績資料
                            foreach (string key in updateSemesterSubjectScoreList[sy][se].Keys)
                            {
                                appendedKeys.Add(key);
                                parentNode.AppendChild(doc.ImportNode(updateSemesterSubjectScoreList[sy][se][key], true));
                            }
                            if (insertSemesterSubjectScoreList.ContainsKey(sy) && insertSemesterSubjectScoreList[sy].ContainsKey(se))
                            {
                                //加入新增的成績資料
                                foreach (string key in insertSemesterSubjectScoreList[sy][se].Keys)
                                {
                                    appendedKeys.Add(key);
                                    parentNode.AppendChild(doc.ImportNode(insertSemesterSubjectScoreList[sy][se][key], true));
                                }
                                insertSemesterSubjectScoreList[sy].Remove(se);
                                if (insertSemesterSubjectScoreList[sy].Count == 0)
                                    insertSemesterSubjectScoreList.Remove(sy);
                            }
                            if (semesterSubjectScoreList.ContainsKey(sy) && semesterSubjectScoreList[sy].ContainsKey(se))
                            {
                                //加入此學期沒有變動的成績資料
                                foreach (string key in semesterSubjectScoreList[sy][se].Keys)
                                {
                                    if (!appendedKeys.Contains(key))
                                        parentNode.AppendChild(doc.ImportNode(semesterSubjectScoreList[sy][se][key].Detail, true));
                                }
                            }
                            semesterSubjectCalcScoreElement.AppendChild(parentNode);
                        }
                    }
                    //如果還有新增成績，必定為此學期之成績，且此學期尚無任何成績紀錄
                    foreach (int sy in insertSemesterSubjectScoreList.Keys)
                    {
                        foreach (int se in insertSemesterSubjectScoreList[sy].Keys)
                        {
                            if (insertSemesterSubjectScoreList[sy][se].Count > 0)
                            {
                                XmlElement parentNode;
                                if (semeScoreID.ContainsKey(sy) && semeScoreID[sy].ContainsKey(se))
                                {
                                    parentNode = doc.CreateElement("UpdateSemesterScore");
                                    parentNode.SetAttribute("ID", semeScoreID[sy][se]);
                                    if (sy == schoolyear && se == semester)
                                        parentNode.SetAttribute("GradeYear", "" + gradeYear);
                                }
                                else
                                {
                                    parentNode = doc.CreateElement("InsertSemesterScore");
                                    parentNode.SetAttribute("GradeYear", "" + gradeYear);
                                    parentNode.SetAttribute("SchoolYear", "" + sy);
                                    parentNode.SetAttribute("Semester", "" + se);
                                }
                                foreach (XmlElement ele in insertSemesterSubjectScoreList[sy][se].Values)
                                {
                                    parentNode.AppendChild(doc.ImportNode(ele, true));
                                }
                                //如果此學期有新增成績卻沒有更新成績，需將原無變動之成績填回
                                if ((!updateSemesterSubjectScoreList.ContainsKey(sy) || !updateSemesterSubjectScoreList[sy].ContainsKey(se)) && semesterSubjectScoreList.ContainsKey(sy) && semesterSubjectScoreList[sy].ContainsKey(se))
                                {
                                    //加入此學期沒有變動的成績資料
                                    foreach (string key in semesterSubjectScoreList[sy][se].Keys)
                                    {
                                        parentNode.AppendChild(doc.ImportNode(semesterSubjectScoreList[sy][se][key].Detail, true));
                                    }
                                }
                                semesterSubjectCalcScoreElement.AppendChild(parentNode);
                            }
                        }
                    }
                    #endregion

                    #region OldWay
                    //List<SemesterSubjectScoreInfo> aforeSemesterScoreList = new List<SemesterSubjectScoreInfo>();
                    //List<SemesterSubjectScoreInfo> currentSemesterScoreList = new List<SemesterSubjectScoreInfo>();
                    //#region 先掃一遍把學生成績分類
                    //foreach (SemesterSubjectScoreInfo scoreinfo in var.SemesterSubjectScoreList)
                    //{
                    //    if (scoreinfo.SchoolYear == schoolyear)
                    //    {
                    //        if (scoreinfo.Semester == semester)
                    //            currentSemesterScoreList.Add(scoreinfo);
                    //        else if (scoreinfo.Semester < semester)
                    //            aforeSemesterScoreList.Add(scoreinfo);
                    //    }
                    //    else if (scoreinfo.SchoolYear < schoolyear)
                    //        aforeSemesterScoreList.Add(scoreinfo);
                    //}
                    //#endregion
                    //#region 針對之前學期的成績做重讀判斷
                    //Dictionary<int, Dictionary<int, int>> ApplySemesterSchoolYear = new Dictionary<int, Dictionary<int, int>>();
                    ////先掃一遍抓出每個年級最高的學年度
                    //foreach (SemesterSubjectScoreInfo scoreInfo in aforeSemesterScoreList)
                    //{
                    //    if (!ApplySemesterSchoolYear.ContainsKey(scoreInfo.GradeYear))
                    //        ApplySemesterSchoolYear.Add(scoreInfo.GradeYear, new Dictionary<int, int>());
                    //    if (!ApplySemesterSchoolYear[scoreInfo.GradeYear].ContainsKey(scoreInfo.Semester))
                    //        ApplySemesterSchoolYear[scoreInfo.GradeYear].Add(scoreInfo.Semester, scoreInfo.SchoolYear);
                    //    if (scoreInfo.SchoolYear > ApplySemesterSchoolYear[scoreInfo.GradeYear][scoreInfo.Semester])
                    //        ApplySemesterSchoolYear[scoreInfo.GradeYear][scoreInfo.Semester] = scoreInfo.SchoolYear;
                    //}
                    ////如果成績資料的年級學年度不在清單中就移掉
                    //List<SemesterSubjectScoreInfo> removeList = new List<SemesterSubjectScoreInfo>();
                    //foreach (SemesterSubjectScoreInfo scoreInfo in aforeSemesterScoreList)
                    //{
                    //    if (ApplySemesterSchoolYear[scoreInfo.GradeYear][scoreInfo.Semester] != scoreInfo.SchoolYear)
                    //        removeList.Add(scoreInfo);
                    //}
                    //foreach (SemesterSubjectScoreInfo scoreInfo in removeList)
                    //{
                    //    aforeSemesterScoreList.Remove(scoreInfo);
                    //}
                    //#endregion

                    //Dictionary<string, List<SemesterSubjectScoreInfo>> currentSLScoreDictionary = new Dictionary<string, List<SemesterSubjectScoreInfo>>();
                    //Dictionary<string, List<SemesterSubjectScoreInfo>> aforeSLScoreDictionary = new Dictionary<string, List<SemesterSubjectScoreInfo>>();
                    //#region 將有用的成績依科目級別填入
                    //foreach (SemesterSubjectScoreInfo scoreinfo in currentSemesterScoreList)
                    //{
                    //    string key = scoreinfo.Subject.Trim() + "_" + scoreinfo.Level.Trim();
                    //    if (!currentSLScoreDictionary.ContainsKey(key))
                    //        currentSLScoreDictionary.Add(key, new List<SemesterSubjectScoreInfo>());
                    //    currentSLScoreDictionary[key].Add(scoreinfo);
                    //}
                    //foreach (SemesterSubjectScoreInfo scoreinfo in aforeSemesterScoreList)
                    //{
                    //    string key = scoreinfo.Subject + "_" + scoreinfo.Level;
                    //    if (!aforeSLScoreDictionary.ContainsKey(key))
                    //        aforeSLScoreDictionary.Add(key, new List<SemesterSubjectScoreInfo>());
                    //    aforeSLScoreDictionary[key].Add(scoreinfo);
                    //}
                    //#endregion

                    //List<SemesterSubjectScoreCalcInfo> semesterSubjectScoreCalcInfoList = new List<SemesterSubjectScoreCalcInfo>();
                    //#region 建立修改清單
                    //foreach (StudentAttendCourseRecord sacRecord in var.AttendCourseList)
                    //{
                    //    if (!sacRecord.HasFinalScore && !sacRecord.NotIncludedInCalc)
                    //    {
                    //        LogError(var, _ErrorList, "" + sacRecord.CourseName + "沒有修課總成績，無法計算。");
                    //        continue;
                    //    }
                    //    string key = sacRecord.Subject.Trim() + "_" + sacRecord.SubjectLevel.Trim();
                    //    SemesterSubjectScoreCalcInfo info = new SemesterSubjectScoreCalcInfo();
                    //    info.SACRecord = sacRecord;
                    //    if (currentSLScoreDictionary.ContainsKey(key))
                    //    {
                    //        //當學期已有紀錄直接寫入當學期
                    //        #region 直接寫入當學期
                    //        info.UpdateSemesterSubjectScoreInfo = currentSLScoreDictionary[key][0];
                    //        //要評分才處理成績
                    //        if (!sacRecord.NotIncludedInCalc)
                    //        {
                    //            info.原始成績 = sacRecord.FinalScore;
                    //            //如果擇優採計重修成績
                    //            if (choseBetter && aforeSLScoreDictionary.ContainsKey(key))
                    //            {
                    //                decimal max = 0;
                    //                foreach (SemesterSubjectScoreInfo semeScore in aforeSLScoreDictionary[key])
                    //                {
                    //                    if (semeScore.Score > max)
                    //                        max = semeScore.Score;
                    //                }
                    //                if (max > info.原始成績)
                    //                    info.擇優採計成績 = max;
                    //            }
                    //        }
                    //        #endregion
                    //    }
                    //    else if (writeToFirstSemester && aforeSLScoreDictionary.ContainsKey(key))
                    //    {
                    //        //當重修成績寫回原學期且之前有修課紀錄時寫回最後一筆紀錄中
                    //        #region 寫回最後一筆紀錄中
                    //        SemesterSubjectScoreInfo lastSLScoreRecord = null;
                    //        foreach (SemesterSubjectScoreInfo semeScore in aforeSLScoreDictionary[key])
                    //        {
                    //            if (lastSLScoreRecord == null)
                    //                lastSLScoreRecord = semeScore;
                    //            else
                    //                if (lastSLScoreRecord.SchoolYear == semeScore.SchoolYear && lastSLScoreRecord.Semester < semeScore.Semester)
                    //                    lastSLScoreRecord = semeScore;
                    //                else
                    //                    if (lastSLScoreRecord.SchoolYear < semeScore.SchoolYear)
                    //                        lastSLScoreRecord = semeScore;
                    //        }
                    //        info.UpdateSemesterSubjectScoreInfo = lastSLScoreRecord;
                    //        info.重修成績 = sacRecord.FinalScore;
                    //        #endregion
                    //    }
                    //    else
                    //    {
                    //        //新增一筆紀錄至當學期
                    //        #region 新增一筆紀錄
                    //        //要評分才處理成績
                    //        if (!sacRecord.NotIncludedInCalc)
                    //        {
                    //            info.原始成績 = sacRecord.FinalScore;
                    //            //如果擇優採計重修成績
                    //            if (choseBetter && aforeSLScoreDictionary.ContainsKey(key))
                    //            {
                    //                decimal max = 0;
                    //                foreach (SemesterSubjectScoreInfo semeScore in aforeSLScoreDictionary[key])
                    //                {
                    //                    if (semeScore.Score > max)
                    //                        max = semeScore.Score;
                    //                }
                    //                if (max > info.原始成績)
                    //                    info.擇優採計成績 = max;
                    //            }
                    //        }
                    //        #endregion
                    //    }
                    //    semesterSubjectScoreCalcInfoList.Add(info);
                    //}
                    //#endregion
                    //#region 建立semesterSubjectCalcScoreElement
                    //Dictionary<int, Dictionary<int, List<SemesterSubjectScoreCalcInfo>>> semesterCalcInfo = new Dictionary<int, Dictionary<int, List<SemesterSubjectScoreCalcInfo>>>();
                    //#region 照學年度學期分開
                    //foreach (SemesterSubjectScoreCalcInfo calcInfo in semesterSubjectScoreCalcInfoList)
                    //{
                    //    int year, sems;
                    //    if (calcInfo.UpdateSemesterSubjectScoreInfo == null)
                    //    {
                    //        year = schoolyear;
                    //        sems = semester;
                    //    }
                    //    else
                    //    {
                    //        year = calcInfo.UpdateSemesterSubjectScoreInfo.SchoolYear;
                    //        sems = calcInfo.UpdateSemesterSubjectScoreInfo.Semester;
                    //    }
                    //    if (!semesterCalcInfo.ContainsKey(year))
                    //        semesterCalcInfo.Add(year, new Dictionary<int, List<SemesterSubjectScoreCalcInfo>>());
                    //    if (!semesterCalcInfo[year].ContainsKey(sems))
                    //        semesterCalcInfo[year].Add(sems, new List<SemesterSubjectScoreCalcInfo>());
                    //    semesterCalcInfo[year][sems].Add(calcInfo);
                    //}
                    //#endregion
                    //Dictionary<int, Dictionary<int, string>> semeScoreID = (Dictionary<int, Dictionary<int, string>>)var.Fields["SemesterSubjectScoreID"];
                    //foreach (int year in semesterCalcInfo.Keys)
                    //{
                    //    foreach (int sems in semesterCalcInfo[year].Keys)
                    //    {
                    //        XmlElement parentNode;
                    //        #region 建立parentNode，判斷是新增或修改，修改會有ID，新增會有年級
                    //        if (semeScoreID.ContainsKey(year) && semeScoreID[year].ContainsKey(sems))
                    //        {
                    //            parentNode = doc.CreateElement("UpdateSemesterScore");
                    //            parentNode.SetAttribute("ID", semeScoreID[year][sems]);
                    //        }
                    //        else
                    //        {
                    //            parentNode = doc.CreateElement("InsertSemesterScore");
                    //            parentNode.SetAttribute("GradeYear", "" + gradeYear);
                    //        }
                    //        #endregion
                    //        semesterSubjectCalcScoreElement.AppendChild(parentNode);
                    //        Dictionary<string, XmlElement> thisSemesterScores = new Dictionary<string, XmlElement>();
                    //        #region 找尋此學期成績中以存在的成績資料
                    //        if (year == schoolyear && sems == semester)
                    //        {
                    //            foreach (SemesterSubjectScoreInfo s in currentSemesterScoreList)
                    //            {
                    //                string key = s.Subject.Trim() + "_" + s.Level.Trim();
                    //                thisSemesterScores.Add(key, s.Detail);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            foreach (SemesterSubjectScoreInfo s in aforeSemesterScoreList)
                    //            {
                    //                if (s.SchoolYear == year && s.Semester == sems)
                    //                {
                    //                    string key = s.Subject.Trim() + "_" + s.Level.Trim();
                    //                    thisSemesterScores.Add(key, s.Detail);
                    //                }
                    //            }
                    //        }
                    //        #endregion
                    //        #region 新增或修改此學期的成績資料
                    //        foreach (SemesterSubjectScoreCalcInfo calcInfo in semesterCalcInfo[year][sems])
                    //        {
                    //            string key = calcInfo.SACRecord.Subject.Trim() + "_" + calcInfo.SACRecord.SubjectLevel.Trim();
                    //            if (thisSemesterScores.ContainsKey(key))
                    //            {
                    //                #region 修改已存在的資料
                    //                if (calcInfo.重修成績 != null)
                    //                    thisSemesterScores[key].SetAttribute("重修成績", "" + calcInfo.重修成績);
                    //                if (calcInfo.原始成績 != null)
                    //                    thisSemesterScores[key].SetAttribute("原始成績", "" + calcInfo.原始成績);
                    //                if (calcInfo.擇優採計成績 != null)
                    //                    thisSemesterScores[key].SetAttribute("擇優採計成績", "" + calcInfo.擇優採計成績);
                    //                if (calcInfo.SACRecord.NotIncludedInCalc || (calcInfo.原始成績 != null && calcInfo.原始成績 >= applyLimit) || (calcInfo.重修成績 != null && calcInfo.重修成績 >= applyLimit) || (calcInfo.擇優採計成績 != null && calcInfo.擇優採計成績 >= applyLimit))
                    //                    thisSemesterScores[key].SetAttribute("是否取得學分", "是");
                    //                #endregion
                    //            }
                    //            else
                    //            {
                    //                #region 加入新的資料
                    //                XmlElement newScoreInfo = doc.CreateElement("Subject");
                    //                newScoreInfo.SetAttribute("不計學分", calcInfo.SACRecord.NotIncludedInCredit ? "是" : "否");
                    //                newScoreInfo.SetAttribute("不需評分", calcInfo.SACRecord.NotIncludedInCalc ? "是" : "否");
                    //                newScoreInfo.SetAttribute("修課必選修", calcInfo.SACRecord.Required ? "必" : "選");
                    //                newScoreInfo.SetAttribute("修課校部訂", (calcInfo.SACRecord.RequiredBy == "部訂" ? calcInfo.SACRecord.RequiredBy : "校訂"));
                    //                newScoreInfo.SetAttribute("原始成績", "" + calcInfo.原始成績);
                    //                newScoreInfo.SetAttribute("學年調整成績", "");
                    //                newScoreInfo.SetAttribute("擇優採計成績", "" + calcInfo.擇優採計成績);
                    //                //不需評分或分數達及格標準
                    //                if (calcInfo.SACRecord.NotIncludedInCalc || (calcInfo.原始成績 != null && calcInfo.原始成績 >= applyLimit) || (calcInfo.重修成績 != null && calcInfo.重修成績 >= applyLimit) || (calcInfo.擇優採計成績 != null && calcInfo.擇優採計成績 >= applyLimit))
                    //                    newScoreInfo.SetAttribute("是否取得學分", "是");
                    //                else
                    //                    newScoreInfo.SetAttribute("是否取得學分", "否");
                    //                newScoreInfo.SetAttribute("科目", calcInfo.SACRecord.Subject);
                    //                newScoreInfo.SetAttribute("科目級別", calcInfo.SACRecord.SubjectLevel);
                    //                newScoreInfo.SetAttribute("補考成績", "");
                    //                newScoreInfo.SetAttribute("重修成績", "" + calcInfo.重修成績);
                    //                newScoreInfo.SetAttribute("開課分項類別", calcInfo.SACRecord.Entry);
                    //                newScoreInfo.SetAttribute("開課學分數", "" + calcInfo.SACRecord.Credit);
                    //                thisSemesterScores.Add(key, newScoreInfo);
                    //                #endregion
                    //            }
                    //        }
                    //        #endregion
                    //        foreach (XmlElement element in thisSemesterScores.Values)
                    //        {
                    //            parentNode.AppendChild(doc.ImportNode(element, true));
                    //        }
                    //        semesterSubjectCalcScoreElement.AppendChild(parentNode);
                    //    }
                    //}
                    //#endregion 
                    #endregion
                }
                var.Fields.Add("SemesterSubjectCalcScore", semesterSubjectCalcScoreElement);
            }

            return _ErrorList;
        }

        /// <summary>
        /// 計算學期分項成績
        /// </summary>
        /// <param name="schoolyear">學年度</param>
        /// <param name="semester">學期</param>
        /// <param name="accesshelper"></param>
        /// <param name="students">學生物件列表</param>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> FillSemesterEntryCalcScore(int schoolyear, int semester, AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();

            accesshelper.StudentHelper.FillSemesterSubjectScore(false, students);
            foreach (StudentRecord var in students)
            {
                Dictionary<string, decimal> entryCreditCount = new Dictionary<string, decimal>();
                Dictionary<string, List<decimal>> entrySubjectScores = new Dictionary<string, List<decimal>>();
                Dictionary<string, decimal> entryDividend = new Dictionary<string, decimal>();
                Dictionary<string, bool> calcEntry = new Dictionary<string, bool>();
                Dictionary<string, bool> calcInStudy = new Dictionary<string, bool>();
                List<string> takeScore = new List<string>();
                //精準位數
                int decimals = 2;
                //進位模式
                RoundMode mode = RoundMode.四捨五入;
                //成績年級及計算規則皆存在，允許計算成績
                bool canCalc = true;
                #region 取得成績年級跟計算規則
                {
                    #region 處理計算規則
                    XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                    if (scoreCalcRule == null)
                    {
                        if (!_ErrorList.ContainsKey(var))
                            _ErrorList.Add(var, new List<string>());
                        _ErrorList[var].Add("沒有設定成績計算規則。");
                        canCalc &= false;
                    }
                    else
                    {
                        DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                        bool tryParsebool;
                        int tryParseint;
                        decimal tryParseDecimal;

                        #region 精準位數
                        if (scoreCalcRule.SelectSingleNode("各項成績計算位數/學期分項成績計算位數") != null)
                        {
                            if (int.TryParse(helper.GetText("各項成績計算位數/學期分項成績計算位數/@位數"), out tryParseint))
                                decimals = tryParseint;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學期分項成績計算位數/@四捨五入"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.四捨五入;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學期分項成績計算位數/@無條件捨去"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.無條件捨去;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學期分項成績計算位數/@無條件進位"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.無條件進位;
                        }
                        #endregion
                        #region 計算類別
                        foreach (string entry in new string[] { "體育", "學業", "國防通識", "健康與護理", "實習科目", "專業科目" })
                        {
                            if (scoreCalcRule.SelectSingleNode("分項成績計算項目") == null || scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry) == null || ((XmlElement)scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry)).GetAttribute("計算成績") == "True")
                                calcEntry.Add(entry, true);
                            else
                                calcEntry.Add(entry, false);

                            // 2014/3/20，修改當沒有勾選 預設併入學期學業成績 ChenCT
                            if (scoreCalcRule.SelectSingleNode("分項成績計算項目") == null || scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry) == null || ((XmlElement)scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry)).GetAttribute("併入學期學業成績") == "True")
                                calcInStudy.Add(entry, true);
                            else
                                calcInStudy.Add(entry, false);
                        }
                        #endregion
                        #region 採計成績欄位
                        foreach (string item in new string[] { "原始成績", "補考成績", "重修成績", "擇優採計成績", "學年調整成績" })
                        {
                            if (!bool.TryParse(helper.GetText("分項成績計算採計成績欄位/@" + item), out tryParsebool) || tryParsebool)
                            {//沒有設定這項成績設定規則(預設true)或者設定值是true
                                takeScore.Add(item);
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion
                Dictionary<string, decimal> entryScores = new Dictionary<string, decimal>();
                if (canCalc)
                {
                    #region 將成績分到各分項類別中
                    foreach (SemesterSubjectScoreInfo subjectNode in var.SemesterSubjectScoreList)
                    {
                        if (subjectNode.SchoolYear == schoolyear && subjectNode.Semester == semester)
                        {
                            //不計學分或不需評分不用算
                            if (subjectNode.Detail.GetAttribute("不需評分") == "是" || subjectNode.Detail.GetAttribute("不計學分") == "是")
                                continue;
                            #region 分項類別跟學分數
                            string entry = subjectNode.Detail.GetAttribute("開課分項類別");
                            decimal credit = subjectNode.CreditDec();
                            #endregion
                            decimal tryParseDecimal;
                            decimal maxScore = 0;
                            decimal original = 0;
                            if (decimal.TryParse(subjectNode.Detail.GetAttribute("原始成績"), out tryParseDecimal))
                                original = tryParseDecimal;
                            #region 取得最高分數
                            foreach (var item in takeScore)
                            {
                                if (decimal.TryParse(subjectNode.Detail.GetAttribute(item), out tryParseDecimal) && maxScore < tryParseDecimal)
                                    maxScore = tryParseDecimal;
                            }
                            //if (decimal.TryParse(subjectNode.Detail.GetAttribute("原始成績"), out tryParseDecimal))
                            //    maxScore = tryParseDecimal;
                            //if (decimal.TryParse(subjectNode.Detail.GetAttribute("學年調整成績"), out tryParseDecimal) && maxScore < tryParseDecimal)
                            //    maxScore = tryParseDecimal;
                            //if (decimal.TryParse(subjectNode.Detail.GetAttribute("擇優採計成績"), out tryParseDecimal) && maxScore < tryParseDecimal)
                            //    maxScore = tryParseDecimal;
                            //if (decimal.TryParse(subjectNode.Detail.GetAttribute("補考成績"), out tryParseDecimal) && maxScore < tryParseDecimal)
                            //    maxScore = tryParseDecimal;
                            //if (decimal.TryParse(subjectNode.Detail.GetAttribute("重修成績"), out tryParseDecimal) && maxScore < tryParseDecimal)
                            //    maxScore = tryParseDecimal;
                            #endregion
                            switch (entry)
                            {
                                case "體育":
                                case "國防通識":
                                case "健康與護理":
                                case "實習科目":
                                case "專業科目":
                                    //計算分項成績
                                    if (calcEntry[entry])
                                    {
                                        #region original
                                        //加總學分數
                                        if (!entryCreditCount.ContainsKey(entry + "(原始)"))
                                            entryCreditCount.Add(entry + "(原始)", credit);
                                        else
                                            entryCreditCount[entry + "(原始)"] += credit;
                                        //加入將成績資料分項
                                        if (!entrySubjectScores.ContainsKey(entry + "(原始)")) entrySubjectScores.Add(entry + "(原始)", new List<decimal>());
                                        entrySubjectScores[entry + "(原始)"].Add(original);
                                        //加權總計
                                        if (!entryDividend.ContainsKey(entry + "(原始)"))
                                            entryDividend.Add(entry + "(原始)", original * credit);
                                        else
                                            entryDividend[entry + "(原始)"] += (original * credit);
                                        #endregion
                                        #region maxScore
                                        //加總學分數
                                        if (!entryCreditCount.ContainsKey(entry))
                                            entryCreditCount.Add(entry, credit);
                                        else
                                            entryCreditCount[entry] += credit;
                                        //加入將成績資料分項
                                        if (!entrySubjectScores.ContainsKey(entry)) entrySubjectScores.Add(entry, new List<decimal>());
                                        entrySubjectScores[entry].Add(maxScore);
                                        //加權總計
                                        if (!entryDividend.ContainsKey(entry))
                                            entryDividend.Add(entry, maxScore * credit);
                                        else
                                            entryDividend[entry] += (maxScore * credit);
                                        #endregion
                                    }
                                    //將科目成績與學業成績一併計算
                                    if (calcInStudy[entry])
                                    {
                                        #region original
                                        //加總學分數
                                        if (!entryCreditCount.ContainsKey("學業" + "(原始)"))
                                            entryCreditCount.Add("學業" + "(原始)", credit);
                                        else
                                            entryCreditCount["學業" + "(原始)"] += credit;
                                        //加入將成績資料分項
                                        if (!entrySubjectScores.ContainsKey("學業" + "(原始)")) entrySubjectScores.Add("學業" + "(原始)", new List<decimal>());
                                        entrySubjectScores["學業" + "(原始)"].Add(original);
                                        //加權總計
                                        if (!entryDividend.ContainsKey("學業" + "(原始)"))
                                            entryDividend.Add("學業" + "(原始)", original * credit);
                                        else
                                            entryDividend["學業" + "(原始)"] += (original * credit);
                                        #endregion
                                        #region maxScore
                                        //加總學分數
                                        if (!entryCreditCount.ContainsKey("學業"))
                                            entryCreditCount.Add("學業", credit);
                                        else
                                            entryCreditCount["學業"] += credit;
                                        //加入將成績資料分項
                                        if (!entrySubjectScores.ContainsKey("學業")) entrySubjectScores.Add("學業", new List<decimal>());
                                        entrySubjectScores["學業"].Add(maxScore);
                                        //加權總計
                                        if (!entryDividend.ContainsKey("學業"))
                                            entryDividend.Add("學業", maxScore * credit);
                                        else
                                            entryDividend["學業"] += (maxScore * credit);
                                        #endregion
                                    }
                                    break;

                                case "學業":
                                default:
                                    #region original
                                    //加總學分數
                                    if (!entryCreditCount.ContainsKey("學業" + "(原始)"))
                                        entryCreditCount.Add("學業" + "(原始)", credit);
                                    else
                                        entryCreditCount["學業" + "(原始)"] += credit;
                                    //加入將成績資料分項
                                    if (!entrySubjectScores.ContainsKey("學業" + "(原始)")) entrySubjectScores.Add("學業" + "(原始)", new List<decimal>());
                                    entrySubjectScores["學業" + "(原始)"].Add(original);
                                    //加權總計
                                    if (!entryDividend.ContainsKey("學業" + "(原始)"))
                                        entryDividend.Add("學業" + "(原始)", original * credit);
                                    else
                                        entryDividend["學業" + "(原始)"] += (original * credit);
                                    #endregion
                                    #region maxScore
                                    //加總學分數
                                    if (!entryCreditCount.ContainsKey("學業"))
                                        entryCreditCount.Add("學業", credit);
                                    else
                                        entryCreditCount["學業"] += credit;
                                    //加入將成績資料分項
                                    if (!entrySubjectScores.ContainsKey("學業")) entrySubjectScores.Add("學業", new List<decimal>());
                                    entrySubjectScores["學業"].Add(maxScore);
                                    //加權總計
                                    if (!entryDividend.ContainsKey("學業"))
                                        entryDividend.Add("學業", maxScore * credit);
                                    else
                                        entryDividend["學業"] += (maxScore * credit);
                                    #endregion
                                    break;
                            }
                        }
                    }
                    #endregion
                    #region 處理計算各分項類別的成績
                    foreach (string entry in entryCreditCount.Keys)
                    {
                        decimal entryScore = 0;
                        #region 計算entryScore
                        if (entryCreditCount[entry] == 0)
                        {
                            foreach (decimal score in entrySubjectScores[entry])
                            {
                                entryScore += score;
                            }
                            entryScore = (entryScore / entrySubjectScores[entry].Count);
                        }
                        else
                        {
                            //用加權總分除學分數
                            entryScore = (entryDividend[entry] / entryCreditCount[entry]);
                        }
                        #endregion
                        //精準位數處理
                        entryScore = GetRoundScore(entryScore, decimals, mode);
                        #region 填入EntryScores
                        entryScores.Add(entry, entryScore);
                        #endregion
                    }
                    #endregion
                }
                if (var.Fields.ContainsKey("CalcEntryScores"))
                    var.Fields["CalcEntryScores"] = entryScores;
                else
                    var.Fields.Add("CalcEntryScores", entryScores);
            }
            return _ErrorList;
        }

        /// <summary>
        /// 計算學年分項成績（依據學期科目成績計算）
        /// </summary>
        /// <param name="schoolyear">學年度</param>
        /// <param name="accesshelper"></param>
        /// <param name="students">學生物件列表</param>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> FillSchoolYearEntryCalcScoreBySchoolYearSubjectScore(int schoolyear, AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //取得學期科目成績
            accesshelper.StudentHelper.FillSemesterSubjectScore(false, students);

            //取得學年科目成績
            accesshelper.StudentHelper.FillSchoolYearSubjectScore(false, students);

            foreach (StudentRecord var in students)
            {
                //計算結果
                Dictionary<string, decimal> entryCalcScores = new Dictionary<string, decimal>();
                //精準位數
                int decimals = 2;
                //進位模式
                RoundMode mode = RoundMode.四捨五入;
                //成績年級及計算規則皆存在，允許計算成績
                bool canCalc = true;
                Dictionary<string, bool> calcEntry = new Dictionary<string, bool>();
                Dictionary<string, bool> calcInStudy = new Dictionary<string, bool>();

                #region 取得成績年級跟計算規則
                {
                    #region 處理計算規則
                    XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                    if (scoreCalcRule == null)
                    {
                        if (!_ErrorList.ContainsKey(var))
                            _ErrorList.Add(var, new List<string>());
                        _ErrorList[var].Add("沒有設定成績計算規則。");
                        canCalc &= false;
                    }
                    else
                    {
                        DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                        bool tryParsebool;
                        int tryParseint;

                        #region 精準位數
                        if (scoreCalcRule.SelectSingleNode("各項成績計算位數/學年分項成績計算位數") != null)
                        {
                            if (int.TryParse(helper.GetText("各項成績計算位數/學年分項成績計算位數/@位數"), out tryParseint))
                                decimals = tryParseint;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學年分項成績計算位數/@四捨五入"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.四捨五入;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學年分項成績計算位數/@無條件捨去"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.無條件捨去;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學年分項成績計算位數/@無條件進位"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.無條件進位;
                        }
                        #endregion
                        #region 計算類別
                        //學年科目成績沒有分項屬性，此部分暫不考量
                        //foreach (string entry in new string[] { "體育", "學業", "國防通識", "健康與護理", "實習科目" })
                        //{
                        //    if (scoreCalcRule.SelectSingleNode("分項成績計算項目") == null || scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry) == null || ((XmlElement)scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry)).GetAttribute("計算成績") == "True")
                        //        calcEntry.Add(entry, true);
                        //    else
                        //        calcEntry.Add(entry, false);

                        //    if (scoreCalcRule.SelectSingleNode("分項成績計算項目") == null || scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry) == null || ((XmlElement)scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry)).GetAttribute("併入學期學業成績") != "True")
                        //        calcInStudy.Add(entry, false);
                        //    else
                        //        calcInStudy.Add(entry, true);
                        //}
                        #endregion
                    }
                    #endregion
                }
                #endregion
                if (canCalc)
                {
                    //根據學年度取得學年科目成績的成績年級
                    int? gradeyear = var.SchoolYearSubjectScoreList.GetGradeYear(schoolyear);

                    if (gradeyear != null)
                    {
                        //根據學年度及成績年級刪除不需要學年科目成績
                        var.SchoolYearSubjectScoreList.FilterSchoolYearSubjectScore(gradeyear, schoolyear);

                        //根據學年度及成績年級刪除不需要學期科目成績
                        var.SemesterSubjectScoreList.FilterSemesterSubjectScore(gradeyear, schoolyear);

                        entryCalcScores = var.CalculateSchoolYearEntryScore(calcEntry, calcInStudy, schoolyear, gradeyear.Value, mode, decimals);
                    }
                }
                if (var.Fields.ContainsKey("CalcSchoolYearEntryScores"))
                    var.Fields["CalcSchoolYearEntryScores"] = entryCalcScores;
                else
                    var.Fields.Add("CalcSchoolYearEntryScores", entryCalcScores);
            }
            return _ErrorList;
        }

        /// <summary>
        /// 計算學年分項成績
        /// </summary>
        /// <param name="schoolyear">學年度</param>
        /// <param name="accesshelper"></param>
        /// <param name="students">學生物件列表</param>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> FillSchoolYearEntryCalcScore(int schoolyear, AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //取得學期分項成績
            accesshelper.StudentHelper.FillSemesterEntryScore(false, students);
            foreach (StudentRecord var in students)
            {
                //計算結果
                Dictionary<string, decimal> entryCalcScores = new Dictionary<string, decimal>();
                //精準位數
                int decimals = 2;
                //進位模式
                RoundMode mode = RoundMode.四捨五入;
                //成績年級及計算規則皆存在，允許計算成績
                bool canCalc = true;
                Dictionary<string, bool> calcEntry = new Dictionary<string, bool>();
                #region 取得成績年級跟計算規則
                {
                    #region 處理計算規則
                    XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                    if (scoreCalcRule == null)
                    {
                        if (!_ErrorList.ContainsKey(var))
                            _ErrorList.Add(var, new List<string>());
                        _ErrorList[var].Add("沒有設定成績計算規則。");
                        canCalc &= false;
                    }
                    else
                    {
                        DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                        bool tryParsebool;
                        int tryParseint;
                        decimal tryParseDecimal;

                        #region 精準位數
                        if (scoreCalcRule.SelectSingleNode("各項成績計算位數/學年分項成績計算位數") != null)
                        {
                            if (int.TryParse(helper.GetText("各項成績計算位數/學年分項成績計算位數/@位數"), out tryParseint))
                                decimals = tryParseint;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學年分項成績計算位數/@四捨五入"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.四捨五入;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學年分項成績計算位數/@無條件捨去"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.無條件捨去;
                            if (bool.TryParse(helper.GetText("各項成績計算位數/學年分項成績計算位數/@無條件進位"), out tryParsebool) && tryParsebool)
                                mode = RoundMode.無條件進位;
                        }
                        #endregion
                        #region 計算類別
                        foreach (string entry in new string[] { "體育", "學業", "國防通識", "健康與護理", "實習科目", "專業科目" })
                        {
                            if (scoreCalcRule.SelectSingleNode("分項成績計算項目") == null || scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry) == null || ((XmlElement)scoreCalcRule.SelectSingleNode("分項成績計算項目/" + entry)).GetAttribute("計算成績") == "True")
                                calcEntry.Add(entry, true);
                            else
                                calcEntry.Add(entry, false);
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion
                if (canCalc)
                {
                    int? gradeyear = null;
                    #region 抓年級
                    foreach (SemesterEntryScoreInfo score in var.SemesterEntryScoreList)
                    {
                        if (calcEntry.ContainsKey(score.Entry) && score.SchoolYear == schoolyear)
                        {
                            if (gradeyear == null || score.GradeYear > gradeyear)
                                gradeyear = score.GradeYear;
                        }
                    }
                    #endregion
                    if (gradeyear != null)
                    {
                        #region 移除不需要成績
                        Dictionary<int, int> ApplySemesterSchoolYear = new Dictionary<int, int>();
                        //先掃一遍抓出該年級最高的學年度
                        foreach (SemesterEntryScoreInfo scoreInfo in var.SemesterEntryScoreList)
                        {
                            if (scoreInfo.SchoolYear <= schoolyear && scoreInfo.GradeYear == gradeyear)
                            {
                                if (!ApplySemesterSchoolYear.ContainsKey(scoreInfo.Semester))
                                    ApplySemesterSchoolYear.Add(scoreInfo.Semester, scoreInfo.SchoolYear);
                                else
                                {
                                    if (ApplySemesterSchoolYear[scoreInfo.Semester] < scoreInfo.SchoolYear)
                                        ApplySemesterSchoolYear[scoreInfo.Semester] = scoreInfo.SchoolYear;
                                }
                            }
                        }
                        //如果成績資料的年級學年度不在清單中就移掉
                        List<SemesterEntryScoreInfo> removeList = new List<SemesterEntryScoreInfo>();
                        foreach (SemesterEntryScoreInfo scoreInfo in var.SemesterEntryScoreList)
                        {
                            if (!ApplySemesterSchoolYear.ContainsKey(scoreInfo.Semester) || ApplySemesterSchoolYear[scoreInfo.Semester] != scoreInfo.SchoolYear)
                                removeList.Add(scoreInfo);
                        }
                        foreach (SemesterEntryScoreInfo scoreInfo in removeList)
                        {
                            var.SemesterEntryScoreList.Remove(scoreInfo);
                        }
                        #endregion
                        #region 計算該年級的分項成績
                        Dictionary<string, List<decimal>> entryScores = new Dictionary<string, List<decimal>>();
                        foreach (SemesterEntryScoreInfo score in var.SemesterEntryScoreList)
                        {
                            if (calcEntry.ContainsKey(score.Entry) && score.SchoolYear <= schoolyear && score.GradeYear == gradeyear)
                            {
                                if (!entryScores.ContainsKey(score.Entry))
                                    entryScores.Add(score.Entry, new List<decimal>());
                                entryScores[score.Entry].Add(score.Score);
                            }
                        }
                        foreach (string key in entryScores.Keys)
                        {
                            decimal sum = 0;
                            decimal count = 0;
                            foreach (decimal sc in entryScores[key])
                            {
                                sum += sc;
                                count += 1;
                            }
                            if (count > 0)
                                entryCalcScores.Add(key, GetRoundScore(sum / count, decimals, mode));
                        }
                        #endregion
                    }
                }
                if (var.Fields.ContainsKey("CalcSchoolYearEntryScores"))
                    var.Fields["CalcSchoolYearEntryScores"] = entryCalcScores;
                else
                    var.Fields.Add("CalcSchoolYearEntryScores", entryCalcScores);
            }
            return _ErrorList;
        }

        /// <summary>
        /// 計算學年科目成績
        /// </summary>
        /// <param name="schoolyear">要計算的學年度</param>
        /// <param name="accesshelper"></param>
        /// <param name="students">學生物件列表</param>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> FillSchoolYearSubjectCalcScore(int schoolyear, AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //取得學生學期科目成績資料
            accesshelper.StudentHelper.FillSemesterSubjectScore(false, students);

            //針對每位學生計算學期科目成績
            foreach (StudentRecord var in students)
            {
                #region 處理CalcSchoolYearSubjectScores
                //計算結果
                Dictionary<string, decimal> subjectCalcScores = new Dictionary<string, decimal>();
                //精準位數
                int decimals = 2;
                //進位模式
                RoundMode mode = RoundMode.四捨五入;
                //成績年級及計算規則皆存在，允許計算成績
                bool canCalc = true;
                Dictionary<string, bool> calcEntry = new Dictionary<string, bool>();


                List<string> scoreTypeList = new List<string>();

                #region 取得成績年級跟計算規則
                #region 處理計算規則

                //取得某位學生的成績計算規則
                XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                if (scoreCalcRule == null)
                {
                    if (!_ErrorList.ContainsKey(var))
                        _ErrorList.Add(var, new List<string>());
                    _ErrorList[var].Add("沒有設定成績計算規則。");
                    canCalc &= false;
                }
                else
                {
                    DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                    bool tryParsebool;
                    int tryParseint;
                    decimal tryParseDecimal;

                    #region 精準位數
                    if (scoreCalcRule.SelectSingleNode("各項成績計算位數/學年科目成績計算位數") != null)
                    {
                        if (int.TryParse(helper.GetText("各項成績計算位數/學年科目成績計算位數/@位數"), out tryParseint))
                            decimals = tryParseint;
                        if (bool.TryParse(helper.GetText("各項成績計算位數/學年科目成績計算位數/@四捨五入"), out tryParsebool) && tryParsebool)
                            mode = RoundMode.四捨五入;
                        if (bool.TryParse(helper.GetText("各項成績計算位數/學年科目成績計算位數/@無條件捨去"), out tryParsebool) && tryParsebool)
                            mode = RoundMode.無條件捨去;
                        if (bool.TryParse(helper.GetText("各項成績計算位數/學年科目成績計算位數/@無條件進位"), out tryParsebool) && tryParsebool)
                            mode = RoundMode.無條件進位;
                    }
                    #endregion

                    #region 學年成績計算採計成績欄位
                    if (scoreCalcRule.SelectSingleNode("學年成績計算採計成績欄位") != null)
                    {
                        if (bool.TryParse(helper.GetText("學年成績計算採計成績欄位/@原始成績"), out tryParsebool) && tryParsebool)
                        {
                            scoreTypeList.Add("原始成績");
                        }
                        if (bool.TryParse(helper.GetText("學年成績計算採計成績欄位/@補考成績"), out tryParsebool) && tryParsebool)
                        {
                            scoreTypeList.Add("補考成績");
                        }
                        if (bool.TryParse(helper.GetText("學年成績計算採計成績欄位/@重修成績"), out tryParsebool) && tryParsebool)
                        {
                            scoreTypeList.Add("重修成績");
                        }
                        if (bool.TryParse(helper.GetText("學年成績計算採計成績欄位/@擇優採計成績"), out tryParsebool) && tryParsebool)
                        {
                            scoreTypeList.Add("擇優採計成績");
                        }
                        if (bool.TryParse(helper.GetText("學年成績計算採計成績欄位/@學年調整成績"), out tryParsebool) && tryParsebool)
                        {
                            scoreTypeList.Add("學年調整成績");
                        }


                    }
                    // 2017/1/13 穎驊 與恩正討論 過後， 需要 再加上一個 若使用者都沒有 儲存設定 的預設， 此預設項目 與舊模式 相同，
                    // 也與  SmartSchool.Evaluation.Configuration.ScoreCalcRuleEditor 內的 使用者設定介面預設  一樣
                    else 
                    {
                        scoreTypeList.Add("原始成績");
                        scoreTypeList.Add("補考成績");
                        scoreTypeList.Add("重修成績");
                        scoreTypeList.Add("擇優採計成績");
                        //scoreTypeList.Add("學年調整成績");                                        
                    }

                    #endregion


                }
                #endregion
                #endregion
                int? gradeyear = null;
                if (canCalc)
                {
                    #region 根據學期科目成績中的學年度取得成績計算的年級，年級以高的為準
                    foreach (SemesterSubjectScoreInfo score in var.SemesterSubjectScoreList)
                    {
                        //判斷學期科目成績的學年度
                        if (score.SchoolYear == schoolyear)
                        {
                            //假設年級為null或是目前學期科目成績的年級大於目前成績
                            if (gradeyear == null || score.GradeYear > gradeyear)
                                gradeyear = score.GradeYear;
                        }
                    }
                    #endregion

                    if (gradeyear != null)
                    {
                        #region 移除不需要成績

                        //此為實際計算成績的學年度及學期。Key是學期，Value是學年度；其中的Key應該只有1及2
                        Dictionary<int, int> ApplySemesterSchoolYear = new Dictionary<int, int>();

                        //先掃一遍抓出該年級最高的學年度
                        foreach (SemesterSubjectScoreInfo scoreInfo in var.SemesterSubjectScoreList)
                        {
                            if (scoreInfo.SchoolYear <= schoolyear && scoreInfo.GradeYear == gradeyear)
                            {
                                if (!ApplySemesterSchoolYear.ContainsKey(scoreInfo.Semester))
                                    ApplySemesterSchoolYear.Add(scoreInfo.Semester, scoreInfo.SchoolYear);
                                else
                                {
                                    if (ApplySemesterSchoolYear[scoreInfo.Semester] < scoreInfo.SchoolYear)
                                        ApplySemesterSchoolYear[scoreInfo.Semester] = scoreInfo.SchoolYear;
                                }
                            }
                        }
                        //如果成績資料的年級學年度不在清單中（指定的學年度及學期）就移掉
                        List<SemesterSubjectScoreInfo> removeList = new List<SemesterSubjectScoreInfo>();
                        foreach (SemesterSubjectScoreInfo scoreInfo in var.SemesterSubjectScoreList)
                        {
                            if (!ApplySemesterSchoolYear.ContainsKey(scoreInfo.Semester) || ApplySemesterSchoolYear[scoreInfo.Semester] != scoreInfo.SchoolYear)
                                removeList.Add(scoreInfo);
                        }
                        foreach (SemesterSubjectScoreInfo scoreInfo in removeList)
                        {
                            var.SemesterSubjectScoreList.Remove(scoreInfo);
                        }
                        #endregion
                        #region 計算該年級的科目成績
                        Dictionary<string, Dictionary<SemesterSubjectScoreInfo, decimal>> subjectScores = new Dictionary<string, Dictionary<SemesterSubjectScoreInfo, decimal>>();

                        //針對每筆學期科目成績，將學期科目先放到subjectScores
                        foreach (SemesterSubjectScoreInfo score in var.SemesterSubjectScoreList)
                        {
                            if (score.SchoolYear <= schoolyear && score.GradeYear == gradeyear)
                            {
                                //先判斷這筆科目成績能不能計算
                                decimal maxscore = decimal.MinValue, tryParsedecimal;
                                
                                bool hasScore = false;

                                // 2016/12/22 穎驊因應 客戶學校反映調整， 重修成績只能取得學分，不能納入計算分數， 恩正指示調整此處，移除 scoreType 重修成績。                                
                                //將學期科目成績擇優
                                //foreach (string scoreType in new string[] { "原始成績", "補考成績", "擇優採計成績" })
                                //{
                                //    if (decimal.TryParse(score.Detail.GetAttribute(scoreType), out tryParsedecimal))
                                //    {
                                //        hasScore |= true;
                                //        if (tryParsedecimal > maxscore)
                                //            maxscore = tryParsedecimal;
                                //    }
                                //}


                                // 2017/1/12 穎驊因應學校調整， 將上次的(上面)的更動 與恩正、韻如討論後， 決定 將"學年成績計算" 會有哪幾項成績納入計算
                                //一併放在 教務作業 設定 中的 "成績計算規則" 一起設定

                                foreach (string scoreType in scoreTypeList)
                                {
                                    if (decimal.TryParse(score.Detail.GetAttribute(scoreType), out tryParsedecimal))
                                    {
                                        hasScore |= true;
                                        if (tryParsedecimal > maxscore)
                                            maxscore = tryParsedecimal;
                                    }
                                }

                                //可以被計算
                                if (hasScore)
                                {
                                    if (!subjectScores.ContainsKey(score.Subject))
                                        subjectScores.Add(score.Subject, new Dictionary<SemesterSubjectScoreInfo, decimal>());
                                    subjectScores[score.Subject].Add(score, maxscore);
                                }
                            }
                        }

                        //計算年級科目成績
                        foreach (string key in subjectScores.Keys)
                        {
                            decimal sum = 0;
                            decimal count = 0;
                            foreach (decimal sc in subjectScores[key].Values)
                            {
                                sum += sc;
                                count += 1;
                            }
                            if (count > 0)
                            {
                                decimal schoolYearSubjectScore = GetRoundScore(sum / count, decimals, mode);

                                if (schoolYearSubjectScore < 60 && subjectScores[key].Count >= 2)
                                {
                                    XmlElement element = scoreCalcRule.SelectSingleNode("學期科目成績計算至學年科目成績規則") as XmlElement;
                                    if (element != null && element.GetAttribute("進校上下學期及格規則").Equals("True"))
                                    {
                                        bool upPass = false, downPass = false;
                                        foreach (var sssi in subjectScores[key].Keys)
                                        {
                                            if (sssi.Semester == 1 && subjectScores[key][sssi] >= 50) upPass = true;
                                            if (sssi.Semester == 2 && subjectScores[key][sssi] >= 60) downPass = true;
                                        }
                                        if (upPass && downPass) { schoolYearSubjectScore = 60; }
                                    }
                                }
                                subjectCalcScores.Add(key, schoolYearSubjectScore);
                            }
                        }
                        #endregion
                    }
                }
                if (var.Fields.ContainsKey("CalcSchoolYearSubjectScores"))
                    var.Fields["CalcSchoolYearSubjectScores"] = subjectCalcScores;
                else
                    var.Fields.Add("CalcSchoolYearSubjectScores", subjectCalcScores);
                #endregion
                //#region 處理SchoolYearApplyScores
                //canCalc = true;
                ////及格標準<年及,及格標準>
                //Dictionary<int, decimal> applyLimit = new Dictionary<int, decimal>();
                ////0:不登錄，1:60分登錄，2:及格標準登錄
                //int regWay = 0;
                //#region 處理計算規則
                //if (scoreCalcRule == null)
                //{
                //    canCalc &= false;
                //}
                //else
                //{
                //    DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                //    #region 及格標準
                //    foreach (XmlElement element in helper.GetElements("及格標準/學生類別"))
                //    {
                //        string cat = element.GetAttribute("類別");
                //        bool useful = false;
                //        //掃描學生的類別作比對
                //        foreach (CategoryInfo catinfo in var.StudentCategorys)
                //        {
                //            if (catinfo.Name == cat || catinfo.FullName == cat)
                //                useful = true;
                //        }
                //        //學生是指定的類別或類別為"預設"
                //        if (cat == "預設" || useful)
                //        {
                //            decimal tryParseDecimal;
                //            for (int gyear = 1; gyear <= 4; gyear++)
                //            {
                //                switch (gyear)
                //                {
                //                    case 1:
                //                        if (decimal.TryParse(element.GetAttribute("一年級及格標準"), out tryParseDecimal))
                //                        {
                //                            if (!applyLimit.ContainsKey(gyear))
                //                                applyLimit.Add(gyear, tryParseDecimal);
                //                            if (applyLimit[gyear] > tryParseDecimal)
                //                                applyLimit[gyear] = tryParseDecimal;
                //                        }
                //                        break;
                //                    case 2:
                //                        if (decimal.TryParse(element.GetAttribute("二年級及格標準"), out tryParseDecimal))
                //                        {
                //                            if (!applyLimit.ContainsKey(gyear))
                //                                applyLimit.Add(gyear, tryParseDecimal);
                //                            if (applyLimit[gyear] > tryParseDecimal)
                //                                applyLimit[gyear] = tryParseDecimal;
                //                        }
                //                        break;
                //                    case 3:
                //                        if (decimal.TryParse(element.GetAttribute("三年級及格標準"), out tryParseDecimal))
                //                        {
                //                            if (!applyLimit.ContainsKey(gyear))
                //                                applyLimit.Add(gyear, tryParseDecimal);
                //                            if (applyLimit[gyear] > tryParseDecimal)
                //                                applyLimit[gyear] = tryParseDecimal;
                //                        }
                //                        break;
                //                    case 4:
                //                        if (decimal.TryParse(element.GetAttribute("四年級及格標準"), out tryParseDecimal))
                //                        {
                //                            if (!applyLimit.ContainsKey(gyear))
                //                                applyLimit.Add(gyear, tryParseDecimal);
                //                            if (applyLimit[gyear] > tryParseDecimal)
                //                                applyLimit[gyear] = tryParseDecimal;
                //                        }
                //                        break;
                //                    default:
                //                        break;
                //                }
                //            }
                //        }
                //    }
                //    #endregion
                //    bool tryParseBool = false;
                //    if (bool.TryParse(helper.GetText("學年調整成績/@不登錄學年調整成績"), out tryParseBool) && tryParseBool)
                //        regWay = 0;
                //    if (bool.TryParse(helper.GetText("學年調整成績/@以六十分登錄"), out tryParseBool) && tryParseBool)
                //        regWay = 1;
                //    if (bool.TryParse(helper.GetText("學年調整成績/@以學生及格標準登錄"), out tryParseBool) && tryParseBool)
                //        regWay = 2;
                //    if (bool.TryParse(helper.GetText("學年調整成績/@不使用學年調整成績"), out tryParseBool) && tryParseBool)
                //        regWay = 3;
                //}
                //#endregion
                //#region 處理學年調整成績
                //List<SemesterSubjectScoreInfo> updateScores = new List<SemesterSubjectScoreInfo>();
                //if (gradeyear != null)
                //{
                //    decimal applylimit = 40;
                //    if (applyLimit.ContainsKey((int)gradeyear))
                //        applylimit = applyLimit[(int)gradeyear];
                //    foreach (SemesterSubjectScoreInfo score in var.SemesterSubjectScoreList)
                //    {
                //        if (!score.Pass && subjectCalcScores.ContainsKey(score.Subject) && subjectCalcScores[score.Subject] >= applylimit)
                //        {
                //            if (regWay != 3)//不使用學年調整成績
                //            {
                //                switch (regWay)
                //                {
                //                    default:
                //                    case 0:
                //                        break;
                //                    case 1:
                //                        score.Detail.SetAttribute("學年調整成績", "60");
                //                        break;
                //                    case 2:
                //                        score.Detail.SetAttribute("學年調整成績", "" + applylimit);
                //                        break;

                //                }
                //                score.Detail.SetAttribute("是否取得學分", "是");
                //                updateScores.Add(score);
                //            }
                //        }
                //    }
                //}
                //if (var.Fields.ContainsKey("SchoolYearApplyScores"))
                //    var.Fields["SchoolYearApplyScores"] = updateScores;
                //else
                //    var.Fields.Add("SchoolYearApplyScores", updateScores);
                //#endregion
                //#endregion
            }
            return _ErrorList;
        }


        /// <summary>
        /// 計算學年調整成績
        /// </summary>
        /// <param name="schoolyear">要計算的學年度</param>
        /// <param name="accesshelper"></param>
        /// <param name="students">學生物件列表</param>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> FillSchoolYearApplyScoresCalcScore(int schoolyear, AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //取得學生學期科目成績資料
            accesshelper.StudentHelper.FillSemesterSubjectScore(false, students);
            accesshelper.StudentHelper.FillSchoolYearSubjectScore(false, students);

            foreach (StudentRecord var in students)
            {
                bool canCalc = true;
                //成績年級
                int? gradeyear = null;
                //及格標準<年及,及格標準>
                Dictionary<int, decimal> applyLimit = new Dictionary<int, decimal>();
                //0:不登錄，1:60分登錄，2:及格標準登錄，3.不使用學年調整成績
                int regWay = 3;  // 2014/3/26 ChenCT，因使用者需求調整預設不使用學年調整成績
                #region 取得成績年級跟計算規則
                #region 處理計算規則
                //取得某位學生的成績計算規則
                XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                if (scoreCalcRule == null)
                {
                    canCalc &= false;
                }
                else
                {
                    DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                    #region 及格標準
                    foreach (XmlElement element in helper.GetElements("及格標準/學生類別"))
                    {
                        string cat = element.GetAttribute("類別");
                        bool useful = false;
                        //掃描學生的類別作比對
                        foreach (CategoryInfo catinfo in var.StudentCategorys)
                        {
                            if (catinfo.Name == cat || catinfo.FullName == cat)
                                useful = true;
                        }
                        //學生是指定的類別或類別為"預設"
                        if (cat == "預設" || useful)
                        {
                            decimal tryParseDecimal;
                            for (int gyear = 1; gyear <= 4; gyear++)
                            {
                                switch (gyear)
                                {
                                    case 1:
                                        if (decimal.TryParse(element.GetAttribute("一年級及格標準"), out tryParseDecimal))
                                        {
                                            if (!applyLimit.ContainsKey(gyear))
                                                applyLimit.Add(gyear, tryParseDecimal);
                                            if (applyLimit[gyear] > tryParseDecimal)
                                                applyLimit[gyear] = tryParseDecimal;
                                        }
                                        break;
                                    case 2:
                                        if (decimal.TryParse(element.GetAttribute("二年級及格標準"), out tryParseDecimal))
                                        {
                                            if (!applyLimit.ContainsKey(gyear))
                                                applyLimit.Add(gyear, tryParseDecimal);
                                            if (applyLimit[gyear] > tryParseDecimal)
                                                applyLimit[gyear] = tryParseDecimal;
                                        }
                                        break;
                                    case 3:
                                        if (decimal.TryParse(element.GetAttribute("三年級及格標準"), out tryParseDecimal))
                                        {
                                            if (!applyLimit.ContainsKey(gyear))
                                                applyLimit.Add(gyear, tryParseDecimal);
                                            if (applyLimit[gyear] > tryParseDecimal)
                                                applyLimit[gyear] = tryParseDecimal;
                                        }
                                        break;
                                    case 4:
                                        if (decimal.TryParse(element.GetAttribute("四年級及格標準"), out tryParseDecimal))
                                        {
                                            if (!applyLimit.ContainsKey(gyear))
                                                applyLimit.Add(gyear, tryParseDecimal);
                                            if (applyLimit[gyear] > tryParseDecimal)
                                                applyLimit[gyear] = tryParseDecimal;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    #endregion
                    bool tryParseBool = false;
                    if (bool.TryParse(helper.GetText("學年調整成績/@不登錄學年調整成績"), out tryParseBool) && tryParseBool)
                        regWay = 0;
                    if (bool.TryParse(helper.GetText("學年調整成績/@以六十分登錄"), out tryParseBool) && tryParseBool)
                        regWay = 1;
                    if (bool.TryParse(helper.GetText("學年調整成績/@以學生及格標準登錄"), out tryParseBool) && tryParseBool)
                        regWay = 2;
                    if (bool.TryParse(helper.GetText("學年調整成績/@不使用學年調整成績"), out tryParseBool) && tryParseBool)
                        regWay = 3;
                }
                #endregion
                #endregion
                #region 根據學期科目成績中的學年度取得成績計算的年級，年級以高的為準
                foreach (SemesterSubjectScoreInfo score in var.SemesterSubjectScoreList)
                {
                    //判斷學期科目成績的學年度
                    if (score.SchoolYear == schoolyear)
                    {
                        //假設年級為null或是目前學期科目成績的年級大於目前成績
                        if (gradeyear == null || score.GradeYear > gradeyear)
                            gradeyear = score.GradeYear;
                    }
                }
                #endregion
                #region 處理學年調整成績
                List<SemesterSubjectScoreInfo> updateScores = new List<SemesterSubjectScoreInfo>();
                if (gradeyear != null)
                {
                    decimal applylimit = 40;
                    if (applyLimit.ContainsKey((int)gradeyear))
                        applylimit = applyLimit[(int)gradeyear];
                    foreach (SemesterSubjectScoreInfo score in var.SemesterSubjectScoreList)
                    {
                        if (!score.Pass && score.SchoolYear == schoolyear)
                        {//&& subjectCalcScores.ContainsKey(score.Subject) && subjectCalcScores[score.Subject] >= applylimit
                            foreach (var schoolYearSubjectScore in var.SchoolYearSubjectScoreList)
                            {
                                if (schoolYearSubjectScore.SchoolYear == schoolyear && schoolYearSubjectScore.Subject == score.Subject && schoolYearSubjectScore.Score >= applylimit)
                                {
                                    //subjectCalcScores.Add(schoolYearSubjectScore.Subject, schoolYearSubjectScore.Score);//[score.Subject]
                                    decimal tryParseDecimal, topScore = decimal.MinValue;
                                    string tip = score.Detail.GetAttribute("註記");
                                    foreach (string field in new string[] { "結算成績", "補考成績", "重修成績" })
                                    {
                                        tip = tip.Replace("學年" + field + "及格。", "");
                                        if (decimal.TryParse(schoolYearSubjectScore.Detail.GetAttribute(field), out tryParseDecimal) && tryParseDecimal >= applylimit && tryParseDecimal > topScore)
                                        {
                                            tip += "學年" + field + "及格。";
                                            topScore = tryParseDecimal;
                                        }
                                    }
                                    if (regWay != 3)//不使用學年調整成績
                                    {
                                        switch (regWay)
                                        {
                                            default:
                                            case 0:
                                                break;
                                            case 1:
                                                score.Detail.SetAttribute("學年調整成績", "60");
                                                break;
                                            case 2:
                                                score.Detail.SetAttribute("學年調整成績", "" + applylimit);
                                                break;
                                        }
                                        score.Detail.SetAttribute("是否取得學分", "是");
                                        score.Detail.SetAttribute("註記", tip);
                                        updateScores.Add(score);
                                    }
                                }
                            }
                        }
                    }
                }
                if (var.Fields.ContainsKey("SchoolYearApplyScores"))
                    var.Fields["SchoolYearApplyScores"] = updateScores;
                else
                    var.Fields.Add("SchoolYearApplyScores", updateScores);
                #endregion
            }
            return _ErrorList;
        }

        /// <summary>
        /// 計算學生畢業成績
        /// </summary>
        /// <param name="accesshelper"></param>
        /// <param name="students"></param>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> FillStudentGradCalcScore(AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //抓成績資料
            accesshelper.StudentHelper.FillSemesterSubjectScore(true, students);
            accesshelper.StudentHelper.FillSemesterEntryScore(true, students);
            foreach (StudentRecord var in students)
            {
                //可以計算(表示需要的資料都有)
                bool canCalc = true;
                //精準位數(預設小數點後2位)
                int decimals = 2;
                //進位模式(預設四捨五入)
                RoundMode mode = RoundMode.四捨五入;
                //計算資料採用課程規劃(預設採用課程規劃)
                bool useGPlan = true;
                //畢業成績使用所有科目成績加權計算(預設為否=>使用分項成績平均 )
                bool useSubjectAdv = false;

                XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                if (scoreCalcRule == null)
                {
                    if (!_ErrorList.ContainsKey(var))
                        _ErrorList.Add(var, new List<string>());
                    _ErrorList[var].Add("沒有設定成績計算規則。");
                    canCalc &= false;
                }
                else
                {
                    DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                    #region 處理精準位數
                    bool tryParsebool;
                    int tryParseint;
                    decimal tryParseDecimal;

                    if (scoreCalcRule.SelectSingleNode("各項成績計算位數/畢業成績計算位數") != null)
                    {
                        if (int.TryParse(helper.GetText("各項成績計算位數/畢業成績計算位數/@位數"), out tryParseint))
                            decimals = tryParseint;
                        if (bool.TryParse(helper.GetText("各項成績計算位數/畢業成績計算位數/@四捨五入"), out tryParsebool) && tryParsebool)
                            mode = RoundMode.四捨五入;
                        if (bool.TryParse(helper.GetText("各項成績計算位數/畢業成績計算位數/@無條件捨去"), out tryParsebool) && tryParsebool)
                            mode = RoundMode.無條件捨去;
                        if (bool.TryParse(helper.GetText("各項成績計算位數/畢業成績計算位數/@無條件進位"), out tryParsebool) && tryParsebool)
                            mode = RoundMode.無條件進位;
                    }
                    #endregion
                    #region 處理採計方式
                    if (scoreCalcRule.SelectSingleNode("學期科目成績屬性採計方式") != null)
                    {
                        if (scoreCalcRule.SelectSingleNode("學期科目成績屬性採計方式").InnerText == "以實際學期科目成績內容為準")
                        {
                            useGPlan = false;
                        }
                    }
                    if (useGPlan && GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(var.StudentID) == null)
                    {
                        if (!_ErrorList.ContainsKey(var))
                            _ErrorList.Add(var, new List<string>());
                        _ErrorList[var].Add("沒有設定課程規劃表，當\"學期科目成績屬性採計方式\"設定使用\"以課程規劃表內容為準\"時，學生必須要有課程規劃表以做參考。");
                        canCalc &= false;
                    }
                    #endregion
                    #region 處理畢業成績計算規則
                    if (scoreCalcRule.SelectSingleNode("處理畢業成績計算規則") != null)
                    {
                        useSubjectAdv = scoreCalcRule.SelectSingleNode("處理畢業成績計算規則").InnerText == "學期科目成績加權";
                    }
                    #endregion
                }


                XmlDocument doc = new XmlDocument();
                XmlElement gradeCalcScoreElement = doc.CreateElement("GradScore");
                if (canCalc)
                {
                    Dictionary<string, decimal> entryCount = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> entrySum = new Dictionary<string, decimal>();
                    //使用所有科目成績加權計算畢業成績(學業)
                    if (useSubjectAdv)
                    {
                        #region 使用所有科目成績加權計算畢業成績(總分及總數)
                        decimal creditCount = 0;
                        decimal scoreSum = 0;
                        foreach (SemesterSubjectScoreInfo subjectScoreInfo in var.SemesterSubjectScoreList)
                        {
                            if (useGPlan)//相關屬性採用課程規劃
                            {
                                //不計學分或不需評分不用算
                                if (GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(var.StudentID).GetSubjectInfo(subjectScoreInfo.Subject, subjectScoreInfo.Level).NotIncludedInCredit ||
                                     GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(var.StudentID).GetSubjectInfo(subjectScoreInfo.Subject, subjectScoreInfo.Level).NotIncludedInCalc
                                    )
                                    continue;
                                decimal realCredit = 0;
                                decimal.TryParse(GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(var.StudentID).GetSubjectInfo(subjectScoreInfo.Subject, subjectScoreInfo.Level).Credit, out realCredit);
                                scoreSum += subjectScoreInfo.Score * realCredit;
                                creditCount += realCredit;
                            }
                            else//相關屬性直接採用成績內容
                            {
                                //不計學分不用算
                                if (subjectScoreInfo.Detail.GetAttribute("不計學分") == "是" || subjectScoreInfo.Detail.GetAttribute("不需評分") == "是")
                                    continue;
                                scoreSum += subjectScoreInfo.Score * subjectScoreInfo.Credit;
                                creditCount += subjectScoreInfo.Credit;
                            }
                        }
                        if (creditCount != 0)
                        {
                            entryCount.Add("學業", creditCount);
                            entrySum.Add("學業", scoreSum);
                        }
                        #endregion
                    }
                    //計算各分項畢業成績(總分及總數)
                    foreach (SemesterEntryScoreInfo entryScore in var.SemesterEntryScoreList)
                    {
                        #region 計算各分項畢業成績
                        if (entryScore.Entry != "學業" || !useSubjectAdv)//其它分項或者是學業分項但不使用科目成績加權
                        {
                            if (!entryCount.ContainsKey(entryScore.Entry))
                                entryCount.Add(entryScore.Entry, 0);
                            if (!entrySum.ContainsKey(entryScore.Entry))
                                entrySum.Add(entryScore.Entry, 0);

                            entryCount[entryScore.Entry]++;
                            entrySum[entryScore.Entry] += entryScore.Score;
                        }
                        #endregion
                    }
                    //計算&填入分項畢業成績
                    foreach (string entry in entryCount.Keys)
                    {
                        XmlElement entryScoreElement = doc.CreateElement("EntryScore");
                        entryScoreElement.SetAttribute("Entry", entry);
                        entryScoreElement.SetAttribute("Score", "" + GetRoundScore(entrySum[entry] / entryCount[entry], decimals, mode));
                        gradeCalcScoreElement.AppendChild(entryScoreElement);
                    }
                }
                if (var.Fields.ContainsKey("GradCalcScore"))
                    var.Fields.Remove("GradCalcScore");
                var.Fields.Add("GradCalcScore", gradeCalcScoreElement);
            }
            return _ErrorList;
        }

        /// <summary>
        /// 判斷學生是否符合畢業資格
        /// </summary>
        /// <param name="accesshelper"></param>
        /// <param name="students"></param>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> FillStudentGradCheck(AccessHelper accesshelper, List<StudentRecord> students)
        {
            return new SmartSchool.Evaluation.WearyDogComputerHelper.GraduateEvaluator(accesshelper, students).Evaluate();
        }

        public Dictionary<StudentRecord, List<string>> FillStudentFulfilledProgram(AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //抓成績資料
            accesshelper.StudentHelper.FillSemesterSubjectScore(true, students);
            foreach (StudentRecord var in students)
            {
                bool canCalc = true;
                //計算資料採用課程規劃(預設採用課程規劃)
                bool useGPlan = true;

                XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                if (scoreCalcRule == null)
                {
                    if (!_ErrorList.ContainsKey(var))
                        _ErrorList.Add(var, new List<string>());
                    _ErrorList[var].Add("沒有設定成績計算規則。");
                    canCalc &= false;
                }
                else
                {
                    DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                    #region 學期科目成績屬性採計方式
                    if (scoreCalcRule.SelectSingleNode("學期科目成績屬性採計方式") != null)
                    {
                        if (scoreCalcRule.SelectSingleNode("學期科目成績屬性採計方式").InnerText == "以實際學期科目成績內容為準")
                        {
                            useGPlan = false;
                        }
                    }
                    if (useGPlan && GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(var.StudentID) == null)
                    {
                        if (!_ErrorList.ContainsKey(var))
                            _ErrorList.Add(var, new List<string>());
                        _ErrorList[var].Add("沒有設定課程規劃表，當\"學期科目成績屬性採計方式\"設定使用\"以課程規劃表內容為準\"時，學生必須要有課程規劃表以做參考。");
                        canCalc &= false;
                    }
                    #endregion
                }

                XmlDocument doc = new XmlDocument();
                XmlElement fulfilledProgramElement = doc.CreateElement("FulfilledProgram");//可以計算(表示需要的資料都有)
                if (canCalc)
                {
                    foreach (SubjectTableItem subjectTable in SubjectTable.Items["學程科目表"])
                    {
                        XmlElement contentElement = (XmlElement)subjectTable.Content.SelectSingleNode("SubjectTableContent");

                        decimal passLimit;
                        decimal.TryParse(contentElement.GetAttribute("CreditCount"), out passLimit);

                        decimal coreLimit;
                        decimal.TryParse(contentElement.GetAttribute("CoreCount"), out coreLimit);

                        List<string> subjectLevelsInTable = new List<string>();
                        List<string> subjectLevelsInCore = new List<string>();
                        #region 整理在科目表中所有的科目級別(科目+^_^+級別)
                        foreach (XmlElement snode in contentElement.SelectNodes("Subject"))
                        {
                            string name = ((XmlElement)snode).GetAttribute("Name");
                            bool iscore = false;
                            bool.TryParse(snode.GetAttribute("IsCore"), out iscore);

                            if (snode.SelectNodes("Level").Count == 0)
                            {
                                subjectLevelsInTable.Add(name + "^_^");
                                if (iscore)
                                    subjectLevelsInCore.Add(name + "^_^");
                            }
                            else
                            {
                                foreach (XmlNode lnode in snode.SelectNodes("Level"))
                                {
                                    subjectLevelsInTable.Add(name + "^_^" + lnode.InnerText);
                                    if (iscore)
                                        subjectLevelsInCore.Add(name + "^_^" + lnode.InnerText);
                                }
                            }
                        }
                        #endregion

                        decimal credits = 0;
                        decimal coreCredits = 0;
                        foreach (SemesterSubjectScoreInfo subjectScore in var.SemesterSubjectScoreList)
                        {
                            //總學分數
                            if (subjectScore.Pass && subjectLevelsInTable.Contains(subjectScore.Subject + "^_^" + subjectScore.Level))
                            {
                                if (useGPlan)
                                {
                                    #region 從課程規劃表取得這個科目的相關屬性
                                    GraduationPlan.GraduationPlanSubject gPlanSubject = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(var.StudentID).GetSubjectInfo(subjectScore.Subject, subjectScore.Level);
                                    //不計學分不用算
                                    if (gPlanSubject.NotIncludedInCredit)
                                        continue;

                                    decimal credit = 0;
                                    decimal.TryParse(gPlanSubject.Credit, out credit);
                                    credits += credit;
                                    #endregion
                                }
                                else
                                {
                                    #region 直接使用科目成績上的屬性
                                    //不計學分不用算
                                    if (subjectScore.Detail.GetAttribute("不計學分") == "是")
                                        continue;
                                    credits += subjectScore.Credit;
                                    #endregion
                                }
                            }
                            //核心學分數
                            if (subjectScore.Pass && subjectLevelsInCore.Contains(subjectScore.Subject + "^_^" + subjectScore.Level))
                            {
                                if (useGPlan)
                                {
                                    #region 從課程規劃表取得這個科目的相關屬性
                                    GraduationPlan.GraduationPlanSubject gPlanSubject = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(var.StudentID).GetSubjectInfo(subjectScore.Subject, subjectScore.Level);
                                    //不計學分不用算
                                    if (gPlanSubject.NotIncludedInCredit)
                                        continue;

                                    decimal credit = 0;
                                    decimal.TryParse(gPlanSubject.Credit, out credit);
                                    coreCredits += credit;
                                    #endregion
                                }
                                else
                                {
                                    #region 直接使用科目成績上的屬性
                                    //不計學分不用算
                                    if (subjectScore.Detail.GetAttribute("不計學分") == "是")
                                        continue;
                                    coreCredits += subjectScore.Credit;
                                    #endregion
                                }
                            }
                        }
                        if (credits >= passLimit && coreCredits >= coreLimit)
                        {
                            XmlElement programElement = doc.CreateElement("Program");
                            programElement.InnerText = subjectTable.Name;
                            fulfilledProgramElement.AppendChild(programElement);
                        }
                    }
                }
                if (var.Fields.ContainsKey("FulfilledProgram"))
                    var.Fields.Remove("FulfilledProgram");
                var.Fields.Add("FulfilledProgram", fulfilledProgramElement);

            }
            return _ErrorList;
        }

        public void FillSemesterSubjectScoreInfoWithResit(AccessHelper accesshelper, bool filterRepeat, List<StudentRecord> students)
        {
            //抓科目成績
            accesshelper.StudentHelper.FillSemesterSubjectScore(filterRepeat, students);
            foreach (StudentRecord var in students)
            {
                //補考標準<年及,及格標準>
                Dictionary<int, decimal> resitLimit = new Dictionary<int, decimal>();
                //resitLimit.Add(1, 40);
                //resitLimit.Add(2, 40);
                //resitLimit.Add(3, 40);
                //resitLimit.Add(4, 40);

                #region 處理計算規則
                XmlElement scoreCalcRule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;
                if (scoreCalcRule == null)
                {
                }
                else
                {
                    DSXmlHelper helper = new DSXmlHelper(scoreCalcRule);
                    foreach (XmlElement element in helper.GetElements("及格標準/學生類別"))
                    {
                        string cat = element.GetAttribute("類別");
                        bool useful = false;
                        //掃描學生的類別作比對
                        foreach (CategoryInfo catinfo in var.StudentCategorys)
                        {
                            if (catinfo.Name == cat || catinfo.FullName == cat)
                                useful = true;
                        }
                        //學生是指定的類別或類別為"預設"
                        if (cat == "預設" || useful)
                        {
                            decimal tryParseDecimal;
                            for (int gyear = 1; gyear <= 4; gyear++)
                            {
                                switch (gyear)
                                {
                                    case 1:
                                        if (decimal.TryParse(element.GetAttribute("一年級補考標準"), out tryParseDecimal))
                                        {
                                            if (!resitLimit.ContainsKey(gyear))
                                                resitLimit.Add(gyear, tryParseDecimal);
                                            if (resitLimit[gyear] > tryParseDecimal)
                                                resitLimit[gyear] = tryParseDecimal;
                                        }
                                        break;
                                    case 2:
                                        if (decimal.TryParse(element.GetAttribute("二年級補考標準"), out tryParseDecimal))
                                        {
                                            if (!resitLimit.ContainsKey(gyear))
                                                resitLimit.Add(gyear, tryParseDecimal);
                                            if (resitLimit[gyear] > tryParseDecimal)
                                                resitLimit[gyear] = tryParseDecimal;
                                        }
                                        break;
                                    case 3:
                                        if (decimal.TryParse(element.GetAttribute("三年級補考標準"), out tryParseDecimal))
                                        {
                                            if (!resitLimit.ContainsKey(gyear))
                                                resitLimit.Add(gyear, tryParseDecimal);
                                            if (resitLimit[gyear] > tryParseDecimal)
                                                resitLimit[gyear] = tryParseDecimal;
                                        }
                                        break;
                                    case 4:
                                        if (decimal.TryParse(element.GetAttribute("四年級補考標準"), out tryParseDecimal))
                                        {
                                            if (!resitLimit.ContainsKey(gyear))
                                                resitLimit.Add(gyear, tryParseDecimal);
                                            if (resitLimit[gyear] > tryParseDecimal)
                                                resitLimit[gyear] = tryParseDecimal;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                #endregion
                foreach (SemesterSubjectScoreInfo score in var.SemesterSubjectScoreList)
                {
                    bool canResit = false;
                    decimal s = 0;
                    decimal limit = 40;
                    if (decimal.TryParse(score.Detail.GetAttribute("原始成績"), out s))
                    {
                        if (resitLimit.ContainsKey(score.GradeYear)) limit = resitLimit[score.GradeYear];
                        canResit = (s >= limit);
                    }
                    score.Detail.SetAttribute("達補考標準", canResit ? "是" : "否");
                    score.Detail.SetAttribute("補考標準", limit.ToString());
                }
            }
        }

        /// <summary>
        /// 刪除重讀成績
        /// </summary>
        /// <param name="subjectScoreList"></param>
        private void CleanUpRepeat(Dictionary<SemesterSubjectScoreInfo, string> subjectScoreList)
        {
            #region 刪除重讀成績
            Dictionary<int, Dictionary<int, int>> ApplySemesterSchoolYear = new Dictionary<int, Dictionary<int, int>>();
            //先掃一遍抓出每個年級最高的學年度
            foreach (SemesterSubjectScoreInfo scoreInfo in subjectScoreList.Keys)
            {
                if (!ApplySemesterSchoolYear.ContainsKey(scoreInfo.GradeYear))
                    ApplySemesterSchoolYear.Add(scoreInfo.GradeYear, new Dictionary<int, int>());
                if (!ApplySemesterSchoolYear[scoreInfo.GradeYear].ContainsKey(scoreInfo.Semester))
                    ApplySemesterSchoolYear[scoreInfo.GradeYear].Add(scoreInfo.Semester, scoreInfo.SchoolYear);
                if (scoreInfo.SchoolYear > ApplySemesterSchoolYear[scoreInfo.GradeYear][scoreInfo.Semester])
                    ApplySemesterSchoolYear[scoreInfo.GradeYear][scoreInfo.Semester] = scoreInfo.SchoolYear;
            }
            //如果成績資料的年級學年度不在清單中就移掉
            List<SemesterSubjectScoreInfo> removeList = new List<SemesterSubjectScoreInfo>();
            foreach (SemesterSubjectScoreInfo scoreInfo in subjectScoreList.Keys)
            {
                if (ApplySemesterSchoolYear[scoreInfo.GradeYear][scoreInfo.Semester] != scoreInfo.SchoolYear)
                    removeList.Add(scoreInfo);
            }
            foreach (SemesterSubjectScoreInfo scoreInfo in removeList)
            {
                subjectScoreList.Remove(scoreInfo);
            }
            #endregion
        }
    }
}