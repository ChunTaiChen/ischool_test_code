using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Evaluation.GraduationPlan;

namespace SmartSchool.Evaluation.WearyDogComputerHelper
{
    internal class GraduateEvaluator
    {
        public const string REQUIRED = "必修";
        public const string PERCENTAGE = "%";

        public GraduateEvaluator(AccessHelper accesshelper, List<StudentRecord> students)
        {
            DataSource = accesshelper;
            SourceStudents = students;
        }

        private AccessHelper DataSource { get; set; }

        private List<StudentRecord> SourceStudents { get; set; }

        /// <summary>
        /// 檢查學生是否符合畢業條件，傳回無法做畢業判斷的學生及原因。
        /// </summary>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> Evaluate()
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //取得學期科目成績
            DataSource.StudentHelper.FillSemesterSubjectScore(true, SourceStudents);
            //取得學期分項成績，因德行成績判斷已移除，故不需取得此資料
            //DataSource.StudentHelper.FillSemesterEntryScore(true, SourceStudents);
            //取得學年分項成績
            DataSource.StudentHelper.FillSchoolYearEntryScore(true, SourceStudents);

            #region 取得獎勵懲戒資料 & 功過換算表

            List<string> StudentIDList = new List<string>();
            foreach (StudentRecord item in SourceStudents)
            {
                if (!StudentIDList.Contains(item.StudentID))
                    StudentIDList.Add(item.StudentID);
            }
            List<K12.Data.DisciplineRecord> DisciplineList = K12.Data.Discipline.SelectByStudentIDs(StudentIDList);
            Dictionary<string, List<K12.Data.DisciplineRecord>> DisciplineDic = new Dictionary<string, List<K12.Data.DisciplineRecord>>();
            foreach (var item in DisciplineList)
            {
                if (!DisciplineDic.ContainsKey(item.RefStudentID))
                    DisciplineDic.Add(item.RefStudentID, new List<K12.Data.DisciplineRecord>());

                DisciplineDic[item.RefStudentID].Add(item);
            }

            //取得功過換算表 MeritDemeritReduce.Select()
            K12.Data.MeritDemeritReduceRecord mreducerecord = K12.Data.MeritDemeritReduce.Select();

            #endregion

            #region 針對每位學生判斷是否符合畢業條件
            foreach (StudentRecord student in SourceStudents)
            {
                //可以計算(表示需要的資料都有)
                bool isDataReasonable = true;
                //計算資料採用課程規劃(預設採用課程規劃)
                bool useGPlan = true;
                //修滿所有必修課程
                bool attendAllRequiredSubjects = false;
                //及格標準<年及,及格標準>
                Dictionary<int, decimal> applyLimit = new Dictionary<int, decimal>();
                //學分規則相關資訊。
                GraduateRule crule = new GraduateRule();
                //同一科目級別不重複採計
                bool filterSameSubject = true;

                XmlElement rule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(student.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(student.StudentID).ScoreCalcRuleElement;
                if (rule == null)
                {
                    if (!_ErrorList.ContainsKey(student))
                        _ErrorList.Add(student, new List<string>());
                    _ErrorList[student].Add("沒有設定成績計算規則。");
                    isDataReasonable &= false;
                }
                else
                {
                    DSXmlHelper helper = new DSXmlHelper(rule);
                    crule.RefStudentID = student.StudentID;
                    #region 學期科目成績屬性採計方式
                    //判斷學期科目成績屬性採計方式以課程規劃為主，還是以學生實際修課為主
                    if (rule.SelectSingleNode("學期科目成績屬性採計方式") != null)
                    {
                        if (rule.SelectSingleNode("學期科目成績屬性採計方式").InnerText == "以實際學期科目成績內容為準")
                        {
                            useGPlan = false;
                        }
                        else
                            useGPlan = true;
                    }
                    //判斷若以課程規為主，但是學生身上卻沒有課程規劃
                    if (useGPlan && GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID) == null)
                    {
                        if (!_ErrorList.ContainsKey(student))
                            _ErrorList.Add(student, new List<string>());
                        _ErrorList[student].Add("沒有設定課程規劃表，當\"學期科目成績屬性採計方式\"設定使用\"以課程規劃表內容為準\"時，學生必須要有課程規劃表以做參考。");
                        isDataReasonable &= false;
                    }
                    #endregion
                    #region 修滿所有必修課程
                    //修滿所有必修課程
                    if (rule.SelectSingleNode("修滿所有必修課程") != null)
                    {
                        if (rule.SelectSingleNode("修滿所有必修課程").InnerText == "True")
                        {
                            attendAllRequiredSubjects = true;
                            //判斷若以課程規為主，但是學生身上卻沒有課程規劃
                            if (GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID) == null)
                            {
                                if (!_ErrorList.ContainsKey(student))
                                    _ErrorList.Add(student, new List<string>());
                                _ErrorList[student].Add("沒有設定課程規劃表，當畢業資格審查條件包含\"必需修滿課程規劃表中所列必修課程\"時，學生必須要有課程規劃表以做參考。");
                                isDataReasonable &= false;
                            }
                        }
                    }
                    #endregion
                    #region 畢業學分數
                    //學科累計總學分數
                    #region 總學分數
                    if (rule.SelectSingleNode("畢業學分數/學科累計總學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/學科累計總學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    decimal credit = 0;
                                    decimal.TryParse(gplanSubject.Credit, out credit);
                                    count += credit;
                                }

                                decimal totalCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out totalCreditTemp);
                                crule.TotalCredit = totalCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal totalCreditTemp = 0;
                                decimal.TryParse(creditstring, out totalCreditTemp);
                                crule.TotalCredit = totalCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //必修學分數
                    #region 必修學分數
                    if (rule.SelectSingleNode("畢業學分數/必修學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/必修學分數").InnerText.Trim();

                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains(PERCENTAGE))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Required == "必修")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal requiredCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace(PERCENTAGE, ""), out requiredCreditTemp);
                                crule.RequiredCredit = requiredCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal requiredCreditTemp = 0;
                                decimal.TryParse(creditstring, out requiredCreditTemp);
                                crule.RequiredCredit = requiredCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //部訂必修學分數
                    #region 部訂必修學分數
                    if (rule.SelectSingleNode("畢業學分數/部訂必修學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/部訂必修學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Required == "必修" && gplanSubject.RequiredBy == "部訂")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal eduRequiredCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out eduRequiredCreditTemp);
                                crule.EduRequiredCredit = eduRequiredCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal eduRequiredCreditTemp = 0;
                                decimal.TryParse(creditstring, out eduRequiredCreditTemp);
                                crule.EduRequiredCredit = eduRequiredCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //實習學分數
                    #region 實習學分數
                    if (rule.SelectSingleNode("畢業學分數/實習學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/實習學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Entry == "實習科目")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal physicalCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out physicalCreditTemp);
                                crule.PhysicalCredit = physicalCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal physicalCreditTemp = 0;
                                decimal.TryParse(creditstring, out physicalCreditTemp);
                                crule.PhysicalCredit = physicalCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //選修學分數
                    #region 選修學分數
                    if (rule.SelectSingleNode("畢業學分數/選修學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/選修學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Required == "選修")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal choicedCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out choicedCreditTemp);
                                crule.ChoicedCredit = choicedCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal choicedCreditTemp = 0;
                                decimal.TryParse(creditstring, out choicedCreditTemp);
                                crule.ChoicedCredit = choicedCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //校訂必修學分數
                    #region 校訂必修學分數
                    if (rule.SelectSingleNode("畢業學分數/校訂必修學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/校訂必修學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Required == "必修" && gplanSubject.RequiredBy == "校訂")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal schoolRequiredCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out schoolRequiredCreditTemp);
                                crule.SchoolRequiredCredit = schoolRequiredCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal schoolRequiredCreditTemp = 0;
                                decimal.TryParse(creditstring, out schoolRequiredCreditTemp);
                                crule.SchoolRequiredCredit = schoolRequiredCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //專業及實習總學分數
                    #region 專業及實習總學分數
                    if (rule.SelectSingleNode("畢業學分數/專業及實習總學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/專業及實習總學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Entry == "實習科目" || gplanSubject.Entry == "專業科目")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal tryParseDecimal = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out tryParseDecimal);
                                crule.專業及實習總學分數 = tryParseDecimal * count / 100m;
                            }
                            else
                            {
                                decimal tryParseDecimal = 0;
                                decimal.TryParse(creditstring, out tryParseDecimal);
                                crule.專業及實習總學分數 = tryParseDecimal;
                            }
                        }
                    }
                    #endregion
                    //同一科目級別不重複採計
                    #region 同一科目級別不重複採計
                    if (rule.SelectSingleNode("畢業學分數/同一科目級別不重複採計") != null)
                    {
                        if (rule.SelectSingleNode("畢業學分數/同一科目級別不重複採計").InnerText.Trim() == "FALSE")
                            filterSameSubject = false;
                    }
                    #endregion
                    #endregion
                    #region 功過相抵未滿三大過
                    //<德行成績畢業判斷規則 功過相抵未滿三大過="true">每學年德行成績均及格</德行成績畢業判斷規則>
                    if (rule.SelectSingleNode("德行成績畢業判斷規則") != null)
                    {
                        XmlElement Element = rule.SelectSingleNode("德行成績畢業判斷規則") as XmlElement;

                        if (Element.GetAttribute("功過相抵未滿三大過").ToUpper().Equals("TRUE"))
                        {
                            crule.IsDemeritNotExceedMaximum = true;



                        }
                    }
                    #endregion
                    #region 學年學業成績及格
                    //<畢業成績計算規則 學年學業成績及格="true">學期分項成績平均</畢業成績計算規則>
                    if (rule.SelectSingleNode("畢業成績計算規則") != null)
                    {
                        XmlElement Element = rule.SelectSingleNode("畢業成績計算規則") as XmlElement;

                        if (Element.GetAttribute("學年學業成績及格").ToUpper().Equals("TRUE"))
                        {
                            crule.IsEverySchoolYearEntryStudiesPass = true;
                            #region 及格標準
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
                        }
                    }
                    #endregion
                }

                XmlDocument doc = new XmlDocument();
                XmlElement evalResult = doc.CreateElement("GradCheck");

                if (isDataReasonable)
                {
                    #region 整理學期科目成績
                    //按學年度學期排好。
                    List<SemesterSubjectScoreInfo> subjectsByStudent = student.SemesterSubjectScoreList;
                    subjectsByStudent.Sort((x, y) =>
                    {
                        SemesterData xx = new SemesterData(0, x.SchoolYear, x.Semester);
                        SemesterData yy = new SemesterData(0, y.SchoolYear, y.Semester);
                        return xx.Order.CompareTo(yy.Order);
                    });
                    #endregion
                    //修滿所有必修課程
                    #region 使用課程規劃表時，判斷必修是否都有修習
                    if (attendAllRequiredSubjects)
                    {
                        //學生修已修的科目中屬於必修的清單。
                        SubjectSet requiredList = new SubjectSet();
                        foreach (SemesterSubjectScoreInfo subjectScore in subjectsByStudent)
                        {
                            GraduationPlan.GraduationPlanSubject gPlanSubject = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).GetSubjectInfo(subjectScore.Subject, subjectScore.Level);

                            if (gPlanSubject.Required.Trim() == REQUIRED)
                            {
                                SubjectName sn = new SubjectName(gPlanSubject.SubjectName, gPlanSubject.Level);

                                //處理重修問題。
                                if (!requiredList.Contains(sn))
                                    requiredList.Add(sn);
                            }
                        }

                        //學生在課程規劃上的必修科目清單。
                        SubjectSet pRequiredSubjects = new SubjectSet();

                        foreach (GraduationPlanSubject each in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                        {
                            //判斷課程規劃科目為必修而且須計學分，亦即必修科目
                            if (each.Required.Trim() == REQUIRED)
                                pRequiredSubjects.Add(new SubjectName(each.SubjectName, each.Level));
                        }
                        foreach (SubjectName each in pRequiredSubjects)
                        {
                            //如果有一個科目沒修，就是修課不完全。
                            if (!requiredList.Contains(each))
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "未修習所有必修課程";
                                evalResult.AppendChild(unPasselement);
                                break;
                            }
                        }
                    }

                    #endregion
                    //判斷畢業學分
                    #region 判斷畢業學分
                    decimal get總學分數 = 0;
                    decimal get必修學分數 = 0;
                    decimal get選修學分數 = 0;
                    decimal get部訂必修學分數 = 0;
                    decimal get校訂必修學分數 = 0;
                    decimal get實習學分數 = 0;
                    decimal get專業學分數 = 0;
                    SubjectSet PassedSubjects = new SubjectSet();
                    foreach (SemesterSubjectScoreInfo subjectScore in subjectsByStudent)
                    {
                        //if (subjectScore.Pass)
                        //{
                        if (useGPlan)
                        {
                            #region 從課程規劃表取得這個科目的相關屬性
                            GraduationPlan.GraduationPlanSubject gPlanSubject = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).GetSubjectInfo(subjectScore.Subject, subjectScore.Level);
                            decimal credit = 0;
                            decimal.TryParse(gPlanSubject.Credit, out credit);

                            subjectScore.Detail.SetAttribute("畢業採計-學分數", "" + credit);
                            subjectScore.Detail.SetAttribute("畢業採計-分項類別", gPlanSubject.Entry);
                            subjectScore.Detail.SetAttribute("畢業採計-必選修", gPlanSubject.Required);
                            subjectScore.Detail.SetAttribute("畢業採計-校部訂", gPlanSubject.RequiredBy);
                            subjectScore.Detail.SetAttribute("畢業採計-不計學分", gPlanSubject.NotIncludedInCredit ? "是" : "否");
                            if (subjectScore.Pass)
                            {
                                //不計學分不用算
                                if (gPlanSubject.NotIncludedInCredit)
                                {
                                    subjectScore.Detail.SetAttribute("畢業採計-學分數", "0");
                                    subjectScore.Detail.SetAttribute("畢業採計-說明", "不計學分");
                                    continue;
                                }

                                bool Uncounted = true;
                                if (filterSameSubject)
                                {
                                    SubjectName sn = new SubjectName(gPlanSubject.SubjectName.Trim(), gPlanSubject.Level.Trim());
                                    if (PassedSubjects.Contains(sn))
                                    {
                                        subjectScore.Detail.SetAttribute("畢業採計-學分數", "0");
                                        subjectScore.Detail.SetAttribute("畢業採計-說明", "同科目級別重複取得學分數");
                                        Uncounted = false;
                                    }
                                    else
                                        PassedSubjects.Add(sn);
                                }

                                if (Uncounted)
                                {
                                    get總學分數 += credit;
                                    if (gPlanSubject.Required == "必修" && gPlanSubject.RequiredBy == "校訂")
                                        get校訂必修學分數 += credit;
                                    if (gPlanSubject.Required == "選修")
                                        get選修學分數 += credit;
                                    if (gPlanSubject.Entry == "實習科目")
                                        get實習學分數 += credit;
                                    if (gPlanSubject.Entry == "專業科目")
                                        get專業學分數 += credit;
                                    if (gPlanSubject.Required == "必修" && gPlanSubject.RequiredBy == "部訂")
                                        get部訂必修學分數 += credit;
                                    if (gPlanSubject.Required == "必修")
                                        get必修學分數 += credit;
                                }
                            }
                            else
                            {
                                subjectScore.Detail.SetAttribute("畢業採計-學分數", "0");
                                subjectScore.Detail.SetAttribute("畢業採計-說明", "未取得學分");
                            }
                            subjectScore.Detail.SetAttribute("畢業採計-說明", (subjectScore.Detail.GetAttribute("畢業採計-說明") == "" ? "" : (subjectScore.Detail.GetAttribute("畢業採計-說明") + "\n")) + "以課程規劃表(" + GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Name + ")為主");
                            #endregion
                        }
                        else
                        {
                            #region 直接使用科目成績上的屬性
                            subjectScore.Detail.SetAttribute("畢業採計-學分數", "" + subjectScore.CreditDec());
                            subjectScore.Detail.SetAttribute("畢業採計-分項類別", subjectScore.Detail.GetAttribute("開課分項類別"));
                            subjectScore.Detail.SetAttribute("畢業採計-必選修", subjectScore.Require ? "必修" : "選修");
                            subjectScore.Detail.SetAttribute("畢業採計-校部訂", subjectScore.Detail.GetAttribute("修課校部訂"));
                            subjectScore.Detail.SetAttribute("畢業採計-不計學分", subjectScore.Detail.GetAttribute("不計學分"));
                            if (subjectScore.Pass)
                            {
                                //不計學分不用算
                                if (subjectScore.Detail.GetAttribute("不計學分") == "是")
                                {
                                    subjectScore.Detail.SetAttribute("畢業採計-學分數", "0");
                                    subjectScore.Detail.SetAttribute("畢業採計-說明", "不計學分");
                                    continue;
                                }
                                bool Uncounted = true;
                                if (filterSameSubject)
                                {
                                    SubjectName sn = new SubjectName(subjectScore.Subject.Trim(), subjectScore.Level.Trim());
                                    if (PassedSubjects.Contains(sn))
                                    {
                                        subjectScore.Detail.SetAttribute("畢業採計-學分數", "0");
                                        subjectScore.Detail.SetAttribute("畢業採計-說明", "同科目級別重複取得學分數");
                                        Uncounted = false;
                                    }
                                    else
                                        PassedSubjects.Add(sn);
                                }

                                if (Uncounted)
                                {
                                    get總學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Require && subjectScore.Detail.GetAttribute("修課校部訂") == "校訂")
                                         get校訂必修學分數 += subjectScore.CreditDec();
                                    if (!subjectScore.Require)
                                         get選修學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Detail.GetAttribute("開課分項類別") == "實習科目")
                                         get實習學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Detail.GetAttribute("開課分項類別") == "專業科目")
                                         get專業學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Require && subjectScore.Detail.GetAttribute("修課校部訂") == "部訂")
                                         get部訂必修學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Require)
                                         get必修學分數 += subjectScore.CreditDec();
                                }
                            }
                            else
                            {
                                subjectScore.Detail.SetAttribute("畢業採計-學分數", "0");
                                subjectScore.Detail.SetAttribute("畢業採計-說明", "未取得學分");
                            }
                            subjectScore.Detail.SetAttribute("畢業採計-說明", (subjectScore.Detail.GetAttribute("畢業採計-說明") == "" ? "" : (subjectScore.Detail.GetAttribute("畢業採計-說明") + "\n")) + "直接使用成績內容");
                            #endregion
                        }
                        //}
                    }

                    if (get總學分數 < crule.TotalCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "總學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get必修學分數 < crule.RequiredCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "必修學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get選修學分數 < crule.ChoicedCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "選修學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get部訂必修學分數 < crule.EduRequiredCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "部訂必修學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get校訂必修學分數 < crule.SchoolRequiredCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "校訂必修學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get實習學分數 < crule.PhysicalCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "實習學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if ((get專業學分數 + get實習學分數) < crule.專業及實習總學分數)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "專業及實習總學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    #endregion
                    //判斷核心科目表
                    #region 判斷核心科目表
                    //2011/4/6 由騉翔修改，增加核心科目表應修學分數判斷
                    List<string> checkedList = new List<string>();
                    #region 整理要選用的科目表
                    foreach (XmlNode st in rule.SelectNodes("核心科目表/科目表"))
                    {
                        checkedList.Add(st.InnerText);
                    }
                    #endregion
                    foreach (SubjectTableItem subjectTable in SubjectTable.Items["核心科目表"])
                    {
                        #region 是要選用的科目表就進行判斷
                        if (checkedList.Contains(subjectTable.Name) && subjectTable.Content.SelectSingleNode("SubjectTableContent") != null)
                        {
                            XmlElement contentElement = (XmlElement)subjectTable.Content.SelectSingleNode("SubjectTableContent");

                            decimal passLimit; //必須取得的學分數
                            decimal attendLimit; //必須修習的學分數
                            decimal.TryParse(contentElement.GetAttribute("CreditCount"), out passLimit);
                            decimal.TryParse(contentElement.GetAttribute("AttendCount"), out attendLimit);

                            List<string> subjectLevelsInTable = new List<string>();
                            #region 整理在科目表中所有的科目級別(科目+^_^+級別)
                            foreach (XmlNode snode in contentElement.SelectNodes("Subject"))
                            {
                                string name = ((XmlElement)snode).GetAttribute("Name");
                                if (snode.SelectNodes("Level").Count == 0)
                                    subjectLevelsInTable.Add(name + "^_^");
                                else
                                {
                                    foreach (XmlNode lnode in snode.SelectNodes("Level"))
                                    {
                                        subjectLevelsInTable.Add(name + "^_^" + lnode.InnerText);
                                    }
                                }
                            }
                            #endregion

                            decimal passCredits = 0; //學生於核心科目表取得學分數
                            decimal attendCredits = 0; //學生於核心科目表已修學分數

                            //針對學生的每項學期科目成績
                            foreach (SemesterSubjectScoreInfo subjectScore in student.SemesterSubjectScoreList)
                            {
                                //if (subjectScore.Pass && subjectLevelsInTable.Contains(subjectScore.Subject + "^_^" + subjectScore.Level))

                                if (subjectLevelsInTable.Contains(subjectScore.Subject + "^_^" + subjectScore.Level))
                                {
                                    if (useGPlan)
                                    {
                                        #region 從課程規劃表取得這個科目的相關屬性
                                        GraduationPlan.GraduationPlanSubject gPlanSubject = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).GetSubjectInfo(subjectScore.Subject, subjectScore.Level);
                                        //不計學分不用算
                                        if (gPlanSubject.NotIncludedInCredit)
                                            continue;

                                        decimal credit = 0;
                                        decimal.TryParse(gPlanSubject.Credit, out credit);

                                        attendCredits += credit;

                                        if (subjectScore.Pass)
                                            passCredits += credit;
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 直接使用科目成績上的屬性
                                        //不計學分不用算
                                        if (subjectScore.Detail.GetAttribute("不計學分") == "是")
                                            continue;

                                        attendCredits += subjectScore.CreditDec();

                                        if (subjectScore.Pass)
                                             passCredits += subjectScore.CreditDec();
                                        #endregion
                                    }
                                }
                            }

                            if (passCredits < passLimit)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "未達" + subjectTable.Name + "取得學分標準";
                                evalResult.AppendChild(unPasselement);
                            }

                            if (attendCredits < attendLimit)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "未達" + subjectTable.Name + "應修學分標準";
                                evalResult.AppendChild(unPasselement);
                            }
                        }
                        #endregion
                    }
                    #endregion
                    //判斷學年學業分項成績及格
                    #region 判斷學年學業分項成績及格
                    if (crule.IsEverySchoolYearEntryStudiesPass)
                    {
                        Dictionary<int, bool?> passGrades = new Dictionary<int, bool?>();
                        passGrades.Add(1, null);
                        passGrades.Add(2, null);
                        passGrades.Add(3, null);
                        foreach (var item in student.SchoolYearEntryScoreList)
                        {
                            if (item.Entry != "學業") continue;
                            decimal applylimit = 60;
                            if (applyLimit.ContainsKey(item.GradeYear))
                                applylimit = applyLimit[item.GradeYear];
                            if (!passGrades.ContainsKey(item.GradeYear))
                                passGrades.Add(item.GradeYear, item.Score >= applylimit);
                            else
                                passGrades[item.GradeYear] = item.Score >= applylimit;
                        }
                        foreach (var gradeYear in passGrades.Keys)
                        {
                            if (passGrades[gradeYear] == null)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "缺少" + gradeYear + "年級學年學業分項成績";
                                evalResult.AppendChild(unPasselement);
                            }
                            else if (!passGrades[gradeYear].Value)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "" + gradeYear + "年級學年學業分項成績不及格";
                                evalResult.AppendChild(unPasselement);
                            }
                        }
                    }
                    #endregion
                    //判斷功過相抵未滿三大過
                    #region 判斷功過相抵未滿三大過
                    if (crule.IsDemeritNotExceedMaximum)
                    {
                        //判斷使用自訂功過換算表或系統預設
                        if (mreducerecord != null)
                        {
                            //將大過數轉為警告數
                            int MaxDemeritC = mreducerecord.DemeritAToDemeritB.HasValue && mreducerecord.DemeritBToDemeritC.HasValue ? mreducerecord.DemeritAToDemeritB.Value * mreducerecord.DemeritBToDemeritC.Value * 3 : 27; //3個大過

                            //學生功過相減統計，以最小單位警告來做計算
                            int StudentDemeritCount = 0;

                            if (DisciplineDic.ContainsKey(student.StudentID))
                            {
                                #region 針對每位學生獎懲資料進行判斷
                                foreach (K12.Data.DisciplineRecord record in DisciplineDic[student.StudentID])
                                {
                                    if (record.MeritFlag == "2") //留察資料予以跳過
                                        continue;

                                    if (record.MeritFlag == "0")
                                        if (record.Cleared == "是")
                                            continue;


                                    //是否進行功過相抵，若是的話才加總獎勵

                                    //(獎勵部份)
                                    //大功
                                    if (mreducerecord.MeritAToMeritB.HasValue && mreducerecord.DemeritBToDemeritC.HasValue && record.MeritA.HasValue)
                                        StudentDemeritCount += record.MeritA.Value * mreducerecord.MeritAToMeritB.Value * mreducerecord.MeritBToMeritC.Value;

                                    //小功
                                    if (mreducerecord.MeritAToMeritB.HasValue && record.MeritB.HasValue)
                                        StudentDemeritCount += record.MeritB.Value * mreducerecord.MeritBToMeritC.Value;

                                    //嘉獎
                                    if (record.MeritC.HasValue)
                                        StudentDemeritCount += record.MeritC.Value;

                                    //(懲戒部份)
                                    if (mreducerecord.DemeritAToDemeritB.HasValue && mreducerecord.DemeritBToDemeritC.HasValue && record.DemeritA.HasValue)
                                        StudentDemeritCount -= record.DemeritA.Value * mreducerecord.DemeritAToDemeritB.Value * mreducerecord.DemeritBToDemeritC.Value;

                                    if (mreducerecord.DemeritBToDemeritC.HasValue && record.DemeritB.HasValue)
                                        StudentDemeritCount -= record.DemeritB.Value * mreducerecord.DemeritBToDemeritC.Value;

                                    if (record.DemeritC.HasValue)
                                        StudentDemeritCount -= record.DemeritC.Value;

                                }
                                #endregion
                            }
                            //如果滿三大過
                            if (StudentDemeritCount <= -MaxDemeritC)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "功過相抵滿三大過";
                                evalResult.AppendChild(unPasselement);
                            }
                        }
                        else
                        {
                            XmlElement unPasselement = doc.CreateElement("UnPassReson");
                            unPasselement.InnerText = "沒有設定功過換算表";
                            evalResult.AppendChild(unPasselement);
                        }
                    }
                    #endregion
                }
                else
                {
                    string reson = "";
                    foreach (var item in _ErrorList[student])
                    {
                        reson += (reson == "" ? "" : "\n") + item;
                    }
                    foreach (var item in student.SemesterSubjectScoreList)
                    {
                        item.Detail.SetAttribute("畢業採計-說明", reson);
                    }
                }
                //將畢業判斷結果加到學生身上
                if (student.Fields.ContainsKey("GradCheck"))
                    student.Fields.Remove("GradCheck");
                student.Fields.Add("GradCheck", evalResult);

            }
            #endregion
            return _ErrorList;
        }


        /// <summary>
        /// 檢查學生是否符合畢業條件，傳回無法做畢業判斷的學生及原因。
        /// </summary>
        /// <returns></returns>
        public Dictionary<StudentRecord, List<string>> EvaluateS()
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //取得學期科目成績
            DataSource.StudentHelper.FillSemesterSubjectScore(true, SourceStudents);
            //取得學期分項成績，因德行成績判斷已移除，故不需取得此資料
            //DataSource.StudentHelper.FillSemesterEntryScore(true, SourceStudents);
            //取得學年分項成績
            DataSource.StudentHelper.FillSchoolYearEntryScore(true, SourceStudents);

            #region 取得獎勵懲戒資料 & 功過換算表

            List<string> StudentIDList = new List<string>();
            foreach (StudentRecord item in SourceStudents)
            {
                if (!StudentIDList.Contains(item.StudentID))
                    StudentIDList.Add(item.StudentID);
            }
            List<K12.Data.DisciplineRecord> DisciplineList = K12.Data.Discipline.SelectByStudentIDs(StudentIDList);
            Dictionary<string, List<K12.Data.DisciplineRecord>> DisciplineDic = new Dictionary<string, List<K12.Data.DisciplineRecord>>();
            foreach (var item in DisciplineList)
            {
                if (!DisciplineDic.ContainsKey(item.RefStudentID))
                    DisciplineDic.Add(item.RefStudentID, new List<K12.Data.DisciplineRecord>());

                DisciplineDic[item.RefStudentID].Add(item);
            }

            //取得功過換算表 MeritDemeritReduce.Select()
            K12.Data.MeritDemeritReduceRecord mreducerecord = K12.Data.MeritDemeritReduce.Select();

            #endregion

            #region 針對每位學生判斷是否符合畢業條件
            foreach (StudentRecord student in SourceStudents)
            {
                //可以計算(表示需要的資料都有)
                bool isDataReasonable = true;
                //計算資料採用課程規劃(預設採用課程規劃)
                bool useGPlan = true;
                //修滿所有必修課程
                bool attendAllRequiredSubjects = false;
                //及格標準<年及,及格標準>
                Dictionary<int, decimal> applyLimit = new Dictionary<int, decimal>();
                //學分規則相關資訊。
                GraduateRule crule = new GraduateRule();

                XmlElement rule = ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(student.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(student.StudentID).ScoreCalcRuleElement;
                if (rule == null)
                {
                    if (!_ErrorList.ContainsKey(student))
                        _ErrorList.Add(student, new List<string>());
                    _ErrorList[student].Add("沒有設定成績計算規則。");
                    isDataReasonable &= false;
                }
                else
                {
                    DSXmlHelper helper = new DSXmlHelper(rule);
                    crule.RefStudentID = student.StudentID;
                    #region 學期科目成績屬性採計方式
                    //判斷學期科目成績屬性採計方式以課程規劃為主，還是以學生實際修課為主
                    if (rule.SelectSingleNode("學期科目成績屬性採計方式") != null)
                    {
                        if (rule.SelectSingleNode("學期科目成績屬性採計方式").InnerText == "以實際學期科目成績內容為準")
                        {
                            useGPlan = false;
                        }
                        else
                            useGPlan = true;
                    }
                    //判斷若以課程規為主，但是學生身上卻沒有課程規劃
                    if (useGPlan && GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID) == null)
                    {
                        if (!_ErrorList.ContainsKey(student))
                            _ErrorList.Add(student, new List<string>());
                        _ErrorList[student].Add("沒有設定課程規劃表，當\"學期科目成績屬性採計方式\"設定使用\"以課程規劃表內容為準\"時，學生必須要有課程規劃表以做參考。");
                        isDataReasonable &= false;
                    }
                    #endregion
                    #region 修滿所有必修課程
                    //修滿所有必修課程
                    if (rule.SelectSingleNode("修滿所有必修課程") != null)
                    {
                        if (rule.SelectSingleNode("修滿所有必修課程").InnerText == "True")
                        {
                            attendAllRequiredSubjects = true;
                            //判斷若以課程規為主，但是學生身上卻沒有課程規劃
                            if (GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID) == null)
                            {
                                if (!_ErrorList.ContainsKey(student))
                                    _ErrorList.Add(student, new List<string>());
                                _ErrorList[student].Add("沒有設定課程規劃表，當畢業資格審查條件包含\"必需修滿課程規劃表中所列必修課程\"時，學生必須要有課程規劃表以做參考。");
                                isDataReasonable &= false;
                            }
                        }
                    }
                    #endregion
                    #region 畢業學分數
                    //學科累計總學分數
                    #region 總學分數
                    if (rule.SelectSingleNode("畢業學分數/學科累計總學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/學科累計總學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    decimal credit = 0;
                                    decimal.TryParse(gplanSubject.Credit, out credit);
                                    count += credit;
                                }

                                decimal totalCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out totalCreditTemp);
                                crule.TotalCredit = totalCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal totalCreditTemp = 0;
                                decimal.TryParse(creditstring, out totalCreditTemp);
                                crule.TotalCredit = totalCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //必修學分數
                    #region 必修學分數
                    if (rule.SelectSingleNode("畢業學分數/必修學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/必修學分數").InnerText.Trim();

                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains(PERCENTAGE))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Required == "必修")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal requiredCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace(PERCENTAGE, ""), out requiredCreditTemp);
                                crule.RequiredCredit = requiredCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal requiredCreditTemp = 0;
                                decimal.TryParse(creditstring, out requiredCreditTemp);
                                crule.RequiredCredit = requiredCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //部訂必修學分數
                    #region 部訂必修學分數
                    if (rule.SelectSingleNode("畢業學分數/部訂必修學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/部訂必修學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Required == "必修" && gplanSubject.RequiredBy == "部訂")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal eduRequiredCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out eduRequiredCreditTemp);
                                crule.EduRequiredCredit = eduRequiredCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal eduRequiredCreditTemp = 0;
                                decimal.TryParse(creditstring, out eduRequiredCreditTemp);
                                crule.EduRequiredCredit = eduRequiredCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //實習學分數
                    #region 實習學分數
                    if (rule.SelectSingleNode("畢業學分數/實習學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/實習學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Entry == "實習科目")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal physicalCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out physicalCreditTemp);
                                crule.PhysicalCredit = physicalCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal physicalCreditTemp = 0;
                                decimal.TryParse(creditstring, out physicalCreditTemp);
                                crule.PhysicalCredit = physicalCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //選修學分數
                    #region 選修學分數
                    if (rule.SelectSingleNode("畢業學分數/選修學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/選修學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Required == "選修")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal choicedCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out choicedCreditTemp);
                                crule.ChoicedCredit = choicedCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal choicedCreditTemp = 0;
                                decimal.TryParse(creditstring, out choicedCreditTemp);
                                crule.ChoicedCredit = choicedCreditTemp;
                            }
                        }
                    }
                    #endregion
                    //校訂必修學分數
                    #region 校訂必修學分數
                    if (rule.SelectSingleNode("畢業學分數/校訂必修學分數") != null)
                    {
                        string creditstring = rule.SelectSingleNode("畢業學分數/校訂必修學分數").InnerText.Trim();
                        if (creditstring != "")
                        {
                            //若學分數設定為百分比，則掃描學生身上的課程規劃科目取代成實際的學分數
                            if (creditstring.Contains("%"))
                            {
                                decimal count = 0;
                                foreach (GraduationPlan.GraduationPlanSubject gplanSubject in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                                {
                                    if (gplanSubject.Required == "必修" && gplanSubject.RequiredBy == "校訂")
                                    {
                                        decimal credit = 0;
                                        decimal.TryParse(gplanSubject.Credit, out credit);
                                        count += credit;
                                    }
                                }
                                decimal schoolRequiredCreditTemp = 0;
                                decimal.TryParse(creditstring.Replace("%", ""), out schoolRequiredCreditTemp);
                                crule.SchoolRequiredCredit = schoolRequiredCreditTemp * count / 100m;
                            }
                            else
                            {
                                decimal schoolRequiredCreditTemp = 0;
                                decimal.TryParse(creditstring, out schoolRequiredCreditTemp);
                                crule.SchoolRequiredCredit = schoolRequiredCreditTemp;
                            }
                        }
                    }
                    #endregion
                    #endregion
                    #region 功過相抵未滿三大過
                    //<德行成績畢業判斷規則 功過相抵未滿三大過="true">每學年德行成績均及格</德行成績畢業判斷規則>
                    if (rule.SelectSingleNode("德行成績畢業判斷規則") != null)
                    {
                        XmlElement Element = rule.SelectSingleNode("德行成績畢業判斷規則") as XmlElement;

                        if (Element.GetAttribute("功過相抵未滿三大過").ToUpper().Equals("TRUE"))
                        {
                            crule.IsDemeritNotExceedMaximum = true;



                        }
                    }
                    #endregion
                    #region 學年學業成績及格
                    //<畢業成績計算規則 學年學業成績及格="true">學期分項成績平均</畢業成績計算規則>
                    if (rule.SelectSingleNode("畢業成績計算規則") != null)
                    {
                        XmlElement Element = rule.SelectSingleNode("畢業成績計算規則") as XmlElement;

                        if (Element.GetAttribute("學年學業成績及格").ToUpper().Equals("TRUE"))
                        {
                            crule.IsEverySchoolYearEntryStudiesPass = true;
                            #region 及格標準
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
                        }
                    }
                    #endregion
                }

                XmlDocument doc = new XmlDocument();
                XmlElement evalResult = doc.CreateElement("GradCheck");

                if (isDataReasonable)
                {
                    #region 整理學期科目成績
                    //按學年度學期排好。
                    List<SemesterSubjectScoreInfo> subjectsByStudent = student.SemesterSubjectScoreList;
                    subjectsByStudent.Sort((x, y) =>
                    {
                        SemesterData xx = new SemesterData(0, x.SchoolYear, x.Semester);
                        SemesterData yy = new SemesterData(0, y.SchoolYear, y.Semester);
                        return xx.Order.CompareTo(yy.Order);
                    });
                    #endregion
                    //修滿所有必修課程
                    #region 使用課程規劃表時，判斷必修是否都有修習
                    if (attendAllRequiredSubjects)
                    {
                        //學生修已修的科目中屬於必修的清單。
                        SubjectSet requiredList = new SubjectSet();
                        foreach (SemesterSubjectScoreInfo subjectScore in subjectsByStudent)
                        {
                            GraduationPlan.GraduationPlanSubject gPlanSubject = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).GetSubjectInfo(subjectScore.Subject, subjectScore.Level);

                            if (gPlanSubject.Required.Trim() == REQUIRED)
                            {
                                SubjectName sn = new SubjectName(gPlanSubject.SubjectName, gPlanSubject.Level);

                                //處理重修問題。
                                if (!requiredList.Contains(sn))
                                    requiredList.Add(sn);
                            }
                        }

                        //學生在課程規劃上的必修科目清單。
                        SubjectSet pRequiredSubjects = new SubjectSet();

                        foreach (GraduationPlanSubject each in GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).Subjects)
                        {
                            //判斷課程規劃科目為必修而且須計學分，亦即必修科目
                            if (each.Required.Trim() == REQUIRED)
                                pRequiredSubjects.Add(new SubjectName(each.SubjectName, each.Level));
                        }
                        foreach (SubjectName each in pRequiredSubjects)
                        {
                            //如果有一個科目沒修，就是修課不完全。
                            if (!requiredList.Contains(each))
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "未修習所有必修課程";
                                evalResult.AppendChild(unPasselement);
                                break;
                            }
                        }
                    }

                    #endregion
                    //判斷畢業學分
                    #region 判斷畢業學分
                    decimal get總學分數 = 0;
                    decimal get必修學分數 = 0;
                    decimal get選修學分數 = 0;
                    decimal get部訂必修學分數 = 0;
                    decimal get校訂必修學分數 = 0;
                    decimal get實習學分數 = 0;
                    SubjectSet PassedSubjects = new SubjectSet();
                    foreach (SemesterSubjectScoreInfo subjectScore in subjectsByStudent)
                    {


                        if (subjectScore.Pass)
                        {
                            if (useGPlan)
                            {
                                #region 從課程規劃表取得這個科目的相關屬性
                                GraduationPlan.GraduationPlanSubject gPlanSubject = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).GetSubjectInfo(subjectScore.Subject, subjectScore.Level);
                                decimal credit = 0;
                                decimal.TryParse(gPlanSubject.Credit, out credit);

                                //不計學分不用算
                                if (gPlanSubject.NotIncludedInCredit)
                                    continue;

                                SubjectName sn = new SubjectName(gPlanSubject.SubjectName.Trim(), gPlanSubject.Level.Trim());

                                bool Uncounted = true;
                                if (PassedSubjects.Contains(sn))
                                    Uncounted = false;
                                else
                                    PassedSubjects.Add(sn);

                                if (Uncounted)
                                {
                                    get總學分數 += credit;
                                    if (gPlanSubject.Required == "必修" && gPlanSubject.RequiredBy == "校訂")
                                        get校訂必修學分數 += credit;
                                    if (gPlanSubject.Required == "選修")
                                        get選修學分數 += credit;
                                    if (gPlanSubject.Entry == "實習科目")
                                        get實習學分數 += credit;
                                    if (gPlanSubject.Required == "必修" && gPlanSubject.RequiredBy == "部訂")
                                        get部訂必修學分數 += credit;
                                    if (gPlanSubject.Required == "必修")
                                        get必修學分數 += credit;
                                }
                                #endregion
                            }
                            else
                            {
                                #region 直接使用科目成績上的屬性
                                //不計學分不用算
                                if (subjectScore.Detail.GetAttribute("不計學分") == "是")
                                    continue;

                                SubjectName sn = new SubjectName(subjectScore.Subject.Trim(), subjectScore.Level.Trim());
                                bool Uncounted = true;
                                if (PassedSubjects.Contains(sn))
                                    Uncounted = false;
                                else
                                    PassedSubjects.Add(sn);

                                if (Uncounted)
                                {
                                     get總學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Require && subjectScore.Detail.GetAttribute("修課校部訂") == "校訂")
                                         get校訂必修學分數 += subjectScore.CreditDec();
                                    if (!subjectScore.Require)
                                         get選修學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Detail.GetAttribute("開課分項類別") == "實習科目")
                                         get實習學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Require && subjectScore.Detail.GetAttribute("修課校部訂") == "部訂")
                                         get部訂必修學分數 += subjectScore.CreditDec();
                                    if (subjectScore.Require)
                                         get必修學分數 += subjectScore.CreditDec();
                                }
                                #endregion
                            }
                        }
                    }

                    if (get總學分數 < crule.TotalCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "總學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get必修學分數 < crule.RequiredCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "必修學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get選修學分數 < crule.ChoicedCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "選修學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get部訂必修學分數 < crule.EduRequiredCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "部訂必修學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get校訂必修學分數 < crule.SchoolRequiredCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "校訂必修學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    if (get實習學分數 < crule.PhysicalCredit)
                    {
                        XmlElement unPasselement = doc.CreateElement("UnPassReson");
                        unPasselement.InnerText = "實習學分數不足";
                        evalResult.AppendChild(unPasselement);
                    }
                    #endregion
                    //判斷核心科目表
                    #region 判斷核心科目表
                    //2011/4/6 由騉翔修改，增加核心科目表應修學分數判斷
                    List<string> checkedList = new List<string>();
                    #region 整理要選用的科目表
                    foreach (XmlNode st in rule.SelectNodes("核心科目表/科目表"))
                    {
                        checkedList.Add(st.InnerText);
                    }
                    #endregion
                    foreach (SubjectTableItem subjectTable in SubjectTable.Items["核心科目表"])
                    {
                        #region 是要選用的科目表就進行判斷
                        if (checkedList.Contains(subjectTable.Name) && subjectTable.Content.SelectSingleNode("SubjectTableContent") != null)
                        {
                            XmlElement contentElement = (XmlElement)subjectTable.Content.SelectSingleNode("SubjectTableContent");

                            decimal passLimit; //必須取得的學分數
                            decimal attendLimit; //必須修習的學分數
                            decimal.TryParse(contentElement.GetAttribute("CreditCount"), out passLimit);
                            decimal.TryParse(contentElement.GetAttribute("AttendCount"), out attendLimit);

                            List<string> subjectLevelsInTable = new List<string>();
                            #region 整理在科目表中所有的科目級別(科目+^_^+級別)
                            foreach (XmlNode snode in contentElement.SelectNodes("Subject"))
                            {
                                string name = ((XmlElement)snode).GetAttribute("Name");
                                if (snode.SelectNodes("Level").Count == 0)
                                    subjectLevelsInTable.Add(name + "^_^");
                                else
                                {
                                    foreach (XmlNode lnode in snode.SelectNodes("Level"))
                                    {
                                        subjectLevelsInTable.Add(name + "^_^" + lnode.InnerText);
                                    }
                                }
                            }
                            #endregion

                            decimal passCredits = 0; //學生於核心科目表取得學分數
                            decimal attendCredits = 0; //學生於核心科目表已修學分數

                            //針對學生的每項學期科目成績
                            foreach (SemesterSubjectScoreInfo subjectScore in student.SemesterSubjectScoreList)
                            {
                                //if (subjectScore.Pass && subjectLevelsInTable.Contains(subjectScore.Subject + "^_^" + subjectScore.Level))

                                if (subjectLevelsInTable.Contains(subjectScore.Subject + "^_^" + subjectScore.Level))
                                {
                                    if (useGPlan)
                                    {
                                        #region 從課程規劃表取得這個科目的相關屬性
                                        GraduationPlan.GraduationPlanSubject gPlanSubject = GraduationPlan.GraduationPlan.Instance.GetStudentGraduationPlan(student.StudentID).GetSubjectInfo(subjectScore.Subject, subjectScore.Level);
                                        //不計學分不用算
                                        if (gPlanSubject.NotIncludedInCredit)
                                            continue;

                                        decimal credit = 0;
                                        decimal.TryParse(gPlanSubject.Credit, out credit);

                                        attendCredits += credit;

                                        if (subjectScore.Pass)
                                            passCredits += credit;
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 直接使用科目成績上的屬性
                                        //不計學分不用算
                                        if (subjectScore.Detail.GetAttribute("不計學分") == "是")
                                            continue;

                                        attendCredits += subjectScore.CreditDec();

                                        if (subjectScore.Pass)
                                             passCredits += subjectScore.CreditDec();
                                        #endregion
                                    }
                                }
                            }

                            if (passCredits < passLimit)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "未達" + subjectTable.Name + "取得學分標準";
                                evalResult.AppendChild(unPasselement);
                            }

                            if (attendCredits < attendLimit)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "未達" + subjectTable.Name + "應修學分標準";
                                evalResult.AppendChild(unPasselement);
                            }
                        }
                        #endregion
                    }
                    #endregion
                    //判斷學年學業分項成績及格
                    #region 判斷學年學業分項成績及格
                    if (crule.IsEverySchoolYearEntryStudiesPass)
                    {
                        Dictionary<int, bool?> passGrades = new Dictionary<int, bool?>();
                        passGrades.Add(1, null);
                        passGrades.Add(2, null);
                        passGrades.Add(3, null);
                        foreach (var item in student.SchoolYearEntryScoreList)
                        {
                            if (item.Entry != "學業") continue;
                            decimal applylimit = 60;
                            if (applyLimit.ContainsKey(item.GradeYear))
                                applylimit = applyLimit[item.GradeYear];
                            if (!passGrades.ContainsKey(item.GradeYear))
                                passGrades.Add(item.GradeYear, item.Score >= applylimit);
                            else
                                passGrades[item.GradeYear] = item.Score >= applylimit;
                        }
                        foreach (var gradeYear in passGrades.Keys)
                        {
                            if (passGrades[gradeYear] == null)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "缺少" + gradeYear + "年級學年學業分項成績";
                                evalResult.AppendChild(unPasselement);
                            }
                            else if (!passGrades[gradeYear].Value)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "" + gradeYear + "年級學年學業分項成績不及格";
                                evalResult.AppendChild(unPasselement);
                            }
                        }
                    }
                    #endregion
                    //判斷功過相抵未滿三大過
                    #region 判斷功過相抵未滿三大過
                    if (crule.IsDemeritNotExceedMaximum)
                    {
                        //判斷使用自訂功過換算表或系統預設
                        if (mreducerecord != null)
                        {
                            //將大過數轉為警告數
                            int MaxDemeritC = mreducerecord.DemeritAToDemeritB.HasValue && mreducerecord.DemeritBToDemeritC.HasValue ? mreducerecord.DemeritAToDemeritB.Value * mreducerecord.DemeritBToDemeritC.Value * 3 : 27; //3個大過

                            //學生功過相減統計，以最小單位警告來做計算
                            int StudentDemeritCount = 0;

                            if (DisciplineDic.ContainsKey(student.StudentID))
                            {
                                #region 針對每位學生獎懲資料進行判斷
                                foreach (K12.Data.DisciplineRecord record in DisciplineDic[student.StudentID])
                                {
                                    if (record.MeritFlag == "2") //留察資料予以跳過
                                        continue;

                                    if (record.MeritFlag == "0")
                                        if (record.Cleared == "是")
                                            continue;


                                    //是否進行功過相抵，若是的話才加總獎勵

                                    //(獎勵部份)
                                    //大功
                                    if (mreducerecord.MeritAToMeritB.HasValue && mreducerecord.DemeritBToDemeritC.HasValue && record.MeritA.HasValue)
                                        StudentDemeritCount += record.MeritA.Value * mreducerecord.MeritAToMeritB.Value * mreducerecord.MeritBToMeritC.Value;

                                    //小功
                                    if (mreducerecord.MeritAToMeritB.HasValue && record.MeritB.HasValue)
                                        StudentDemeritCount += record.MeritB.Value * mreducerecord.MeritBToMeritC.Value;

                                    //嘉獎
                                    if (record.MeritC.HasValue)
                                        StudentDemeritCount += record.MeritC.Value;

                                    //(懲戒部份)
                                    if (mreducerecord.DemeritAToDemeritB.HasValue && mreducerecord.DemeritBToDemeritC.HasValue && record.DemeritA.HasValue)
                                        StudentDemeritCount -= record.DemeritA.Value * mreducerecord.DemeritAToDemeritB.Value * mreducerecord.DemeritBToDemeritC.Value;

                                    if (mreducerecord.DemeritBToDemeritC.HasValue && record.DemeritB.HasValue)
                                        StudentDemeritCount -= record.DemeritB.Value * mreducerecord.DemeritBToDemeritC.Value;

                                    if (record.DemeritC.HasValue)
                                        StudentDemeritCount -= record.DemeritC.Value;

                                }
                                #endregion
                            }
                            //如果滿三大過
                            if (StudentDemeritCount <= -MaxDemeritC)
                            {
                                XmlElement unPasselement = doc.CreateElement("UnPassReson");
                                unPasselement.InnerText = "功過相抵滿三大過";
                                evalResult.AppendChild(unPasselement);
                            }
                        }
                        else
                        {
                            XmlElement unPasselement = doc.CreateElement("UnPassReson");
                            unPasselement.InnerText = "沒有設定功過換算表";
                            evalResult.AppendChild(unPasselement);
                        }
                    }
                    #endregion
                }
                //將畢業判斷結果加到學生身上
                if (student.Fields.ContainsKey("GradCheck"))
                    student.Fields.Remove("GradCheck");
                student.Fields.Add("GradCheck", evalResult);
            }
            #endregion
            return _ErrorList;
        }
    }
}