using System.Collections.Generic;
using System.Threading;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Feature.ScoreCalcRule;

namespace SmartSchool.Evaluation
{
    public class AngelDemonComputer
    {
        //private enum RoundMode { 四捨五入, 無條件進位, 無條件捨去 }
        private static decimal GetRoundScore(decimal score, int decimals, SmartSchool.Evaluation.WearyDogComputer.RoundMode mode)
        {
            return WearyDogComputer.GetRoundScore(score,decimals,mode);
        }

        private XmlElement _MoralConductElement;

        private DSXmlHelper _MoralConductHelper;

        private List<UsefulPeriodAbsence> _UsefulPeriodAbsences;

        private List<string> _NoabsenceList = new List<string>();

        private Dictionary<string, decimal> _degreeList;

        //取得對照表並且對照出節次->類別的清單(99/11/24 by dylan)
        Dictionary<string, string> periodDic = new Dictionary<string, string>();

        public AngelDemonComputer()
        {
            //取得德行成績計算規則
            _MoralConductElement = QueryScoreCalcRule.GetMoralConductCalcRule();
            if (_MoralConductElement != null)
                _MoralConductHelper = new DSXmlHelper(_MoralConductElement);
            else
                _MoralConductHelper = new DSXmlHelper();
            #region 取得記分缺曠項目

            #region 取得節次類別與缺曠對照表
            List<string> periodList = new List<string>();
            List<string> absenceList = new List<string>();

            //取得對照表並且對照出節次->類別的清單(99/11/24 by dylan)
            periodDic.Clear();

            //取得節次對照表
            foreach (XmlElement var in SmartSchool.Feature.Basic.Config.GetPeriodList().GetContent().GetElements("Period"))
            {
                string name = var.GetAttribute("Type");
                if (!periodList.Contains(name))
                    periodList.Add(name);

                //取得對照表並且對照出節次->類別的清單(99/11/24 by dylan)
                if (!periodDic.ContainsKey(var.GetAttribute("Name")))
                {
                    periodDic.Add(var.GetAttribute("Name"), var.GetAttribute("Type"));
                }
            }
            //取得假別對照表
            foreach (XmlElement var in SmartSchool.Feature.Basic.Config.GetAbsenceList().GetContent().GetElements("Absence"))
            {
                //假別清單
                string name = var.GetAttribute("Name");
                if (!absenceList.Contains(name))
                    absenceList.Add(name);
                //建立不影響全勤清單
                bool noabsence;
                if (bool.TryParse(var.GetAttribute("Noabsence"), out noabsence) && noabsence && !_NoabsenceList.Contains(name))
                    _NoabsenceList.Add(name);
            }
            #endregion
            _UsefulPeriodAbsences = new List<UsefulPeriodAbsence>();
            foreach (XmlElement element in _MoralConductHelper.GetElements("PeriodAbsenceCalcRule/Rule"))
            {
                string absence = element.GetAttribute("Absence");
                string period = element.GetAttribute("Period");
                decimal subtract;
                decimal.TryParse(element.GetAttribute("Subtract"), out subtract);
                int aggregated;
                int.TryParse(element.GetAttribute("Aggregated"), out aggregated);
                if (aggregated > 0 && subtract > 0 && periodList.Contains(period) && absenceList.Contains(absence))
                {
                    _UsefulPeriodAbsences.Add(new UsefulPeriodAbsence(absence, period, subtract, aggregated));
                }
            }
            #endregion

            SystemInformation.getField("Degree");
            _degreeList = (Dictionary<string, decimal>)SystemInformation.Fields["Degree"];
        }

        private void fillReward(object item)
        {
            AccessHelper dataSeed = (AccessHelper)( ( (object[])item )[0] );
            int schoolyear = (int)( ( (object[])item )[1] );
            int semester = (int)( ( (object[])item )[2] );
            List<StudentRecord> students = (List<StudentRecord>)( ( (object[])item )[3] );
            dataSeed.StudentHelper.FillReward(schoolyear, semester, students);
        }

        private void fillAttendance(object item)
        {
            AccessHelper dataSeed = (AccessHelper)( ( (object[])item )[0] );
            int schoolyear = (int)( ( (object[])item )[1] );
            int semester = (int)( ( (object[])item )[2] );
            List<StudentRecord> students = (List<StudentRecord>)( ( (object[])item )[3] );
            dataSeed.StudentHelper.FillAttendance(schoolyear, semester, students);
        }

        private void fillSemesterMoralScore(object item)
        {
            AccessHelper dataSeed = (AccessHelper)( ( (object[])item )[0] );
            int schoolyear = (int)( ( (object[])item )[1] );
            int semester = (int)( ( (object[])item )[2] );
            List<StudentRecord> students = (List<StudentRecord>)( ( (object[])item )[3] );
            dataSeed.StudentHelper.FillSemesterMoralScore(true, students);
        }

        public decimal ComputeRewardScore(int AwardACount, int AwardBCount, int AwardCCount, int FaultACount, int FaultBCount, int FaultCCount)
        {
            decimal finalScore = 0m;
            #region 處理大功
            if ( AwardACount > 0 )
            {
                decimal subScore = 0;
                decimal addScore = 0;
                for ( int i = 0 ; i < AwardACount ; i++ )
                {
                    decimal newscore;
                    if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@AwardA" + ( i + 1 )), out newscore) )
                        addScore = newscore;
                    subScore += addScore;
                }
                finalScore += subScore;
            }
            #endregion
            #region 處理小功
            if ( AwardBCount > 0 )
            {
                decimal subScore = 0;
                decimal addScore = 0;
                for ( int i = 0 ; i < AwardBCount ; i++ )
                {
                    decimal newscore;
                    if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@AwardB" + ( i + 1 )), out newscore) )
                        addScore = newscore;
                    subScore += addScore;
                }
                finalScore += subScore;
            }
            #endregion
            #region 處理嘉獎
            if ( AwardCCount > 0 )
            {
                decimal subScore = 0;
                decimal addScore = 0;
                for ( int i = 0 ; i < AwardCCount ; i++ )
                {
                    decimal newscore;
                    if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@AwardC" + ( i + 1 )), out newscore) )
                        addScore = newscore;
                    subScore += addScore;
                }
                finalScore += subScore;
            }
            #endregion
            #region 處理大過
            if ( FaultACount > 0 )
            {
                decimal subScore = 0;
                decimal addScore = 0;
                for ( int i = 0 ; i < FaultACount ; i++ )
                {
                    decimal newscore;
                    if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@FaultA" + ( i + 1 )), out newscore) )
                        addScore = newscore * ( -1 );
                    subScore += addScore;
                }
                finalScore += subScore;
            }
            #endregion
            #region 處理小過
            if ( FaultBCount > 0 )
            {
                decimal subScore = 0;
                decimal addScore = 0;
                for ( int i = 0 ; i < FaultBCount ; i++ )
                {
                    decimal newscore;
                    if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@FaultB" + ( i + 1 )), out newscore) )
                        addScore = newscore * ( -1 );
                    subScore += addScore;
                }
                finalScore += subScore;
            }
            #endregion
            #region 處理警告
            if ( FaultCCount > 0 )
            {
                decimal subScore = 0;
                decimal addScore = 0;
                for ( int i = 0 ; i < FaultCCount ; i++ )
                {
                    decimal newscore;
                    if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@FaultC" + ( i + 1 )), out newscore) )
                        addScore = newscore * ( -1 );
                    subScore += addScore;
                }
                finalScore += subScore;
            }
            #endregion
            return finalScore;
        }

        public decimal ComputeAttendanceScore(string period, string absence, int times)
        {
            foreach ( UsefulPeriodAbsence u in UsefulPeriodAbsences )
            {
                if ( u.Period == period && u.Absence == absence )
                {
                    return u.Subtract * ( times / u.Aggregated * ( -1 ) );
                }
            }
            return 0m;
        }

        public void FillDemonScore(AccessHelper dataSeed, int schoolyear, int semester, List<StudentRecord> students)
        {

            //dataSeed.StudentHelper.FillSemesterMoralScore(true, students);
            //dataSeed.StudentHelper.FillReward(schoolyear, semester, students);
            //dataSeed.StudentHelper.FillAttendance(schoolyear, semester, students);
            Thread threadSemesterMoralScore = new Thread(new ParameterizedThreadStart(fillSemesterMoralScore));
            threadSemesterMoralScore.IsBackground = true;
            threadSemesterMoralScore.Start(new object[] { dataSeed, schoolyear, semester, students });

            Thread threadReward = new Thread(new ParameterizedThreadStart(fillReward));
            threadReward.IsBackground = true;
            threadReward.Start(new object[] { dataSeed, schoolyear, semester, students });

            Thread threadAttendance = new Thread(new ParameterizedThreadStart(fillAttendance));
            threadAttendance.IsBackground = true;
            threadAttendance.Start(new object[] { dataSeed, schoolyear, semester, students });

            threadSemesterMoralScore.Join();
            threadReward.Join();
            threadAttendance.Join();

            XmlDocument doc = new XmlDocument();
            foreach ( StudentRecord student in students )
            {
                XmlElement element = doc.CreateElement("DemonScore");
                XmlElement subScoreElement;
                decimal subScore;
                decimal finalScore = 0;
                //精準位數
                int decimals = 2;
                if ( !int.TryParse(_MoralConductHelper.GetText("BasicScore/@Decimals"), out decimals) )
                    decimals = 2;
                //進位模式
                SmartSchool.Evaluation.WearyDogComputer.RoundMode mode = SmartSchool.Evaluation.WearyDogComputer.RoundMode.四捨五入;
                switch ( _MoralConductHelper.GetText("BasicScore/@DecimalType") )
                {
                    default:
                    case "四捨五入":
                        mode = SmartSchool.Evaluation.WearyDogComputer.RoundMode.四捨五入;
                        break;
                    case "無條件捨去":
                        mode = SmartSchool.Evaluation.WearyDogComputer.RoundMode.無條件捨去;
                        break;
                    case "無條件進位":
                        mode = SmartSchool.Evaluation.WearyDogComputer.RoundMode.無條件進位;
                        break;
                }
                //超過一百分以一百分計
                bool limit100 = _MoralConductHelper.GetText("BasicScore/@Over100")=="以100分計";
                #region 處理獎懲
                //銷過紀錄是否計算
                bool calcCancel = false;
                if ( !bool.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@CalcCancel"), out calcCancel) )
                    calcCancel = false;
                #region 統計獎懲次數
                int AwardACount = 0;
                int AwardBCount = 0;
                int AwardCCount = 0;
                int FaultACount = 0;
                int FaultBCount = 0;
                int FaultCCount = 0;
                bool hasUltimateAdmonition = false;
                foreach ( RewardInfo reward in student.RewardList )
                {
                    if ( reward.SchoolYear != schoolyear || reward.Semester != semester )
                        continue;
                    if ( !reward.Cleared || calcCancel )
                    {
                        AwardACount += reward.AwardA;
                        AwardBCount += reward.AwardB;
                        AwardCCount += reward.AwardC;
                        FaultACount += reward.FaultA;
                        FaultBCount += reward.FaultB;
                        FaultCCount += reward.FaultC;
                        hasUltimateAdmonition |= reward.UltimateAdmonition;
                    }
                }
                #endregion
                #region 處理基分
                subScoreElement = doc.CreateElement("SubScore");
                subScore = 0;
                subScoreElement.SetAttribute("Type", "基分");
                if ( hasUltimateAdmonition )
                {
                    subScoreElement.SetAttribute("Status", "留校查看");
                    decimal.TryParse(_MoralConductHelper.GetText("BasicScore/@UltimateAdmonitionScore"), out subScore);
                    subScoreElement.SetAttribute("Score", "" + subScore);
                }
                else
                {
                    subScoreElement.SetAttribute("Status", "一般生");
                    decimal.TryParse(_MoralConductHelper.GetText("BasicScore/@NormalScore"), out subScore);
                    subScoreElement.SetAttribute("Score", "" + subScore);
                }
                element.AppendChild(subScoreElement);
                finalScore += subScore;
                #endregion
                #region 計算獎懲項目成績
                #region 處理大功
                if ( AwardACount > 0 )
                {
                    subScoreElement = doc.CreateElement("SubScore");
                    subScore = 0;
                    subScoreElement.SetAttribute("Type", "獎懲");
                    subScoreElement.SetAttribute("Name", "大功");
                    subScoreElement.SetAttribute("Count", "" + AwardACount);
                    decimal addScore = 0;
                    for ( int i = 0 ; i < AwardACount ; i++ )
                    {
                        decimal newscore;
                        if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@AwardA" + ( i + 1 )), out newscore) )
                            addScore = newscore;
                        subScore += addScore;
                    }
                    subScoreElement.SetAttribute("Score", "" + subScore);
                    element.AppendChild(subScoreElement);
                    finalScore += subScore;
                }
                #endregion
                #region 處理小功
                if ( AwardBCount > 0 )
                {
                    subScoreElement = doc.CreateElement("SubScore");
                    subScore = 0;
                    subScoreElement.SetAttribute("Type", "獎懲");
                    subScoreElement.SetAttribute("Name", "小功");
                    subScoreElement.SetAttribute("Count", "" + AwardBCount);
                    decimal addScore = 0;
                    for ( int i = 0 ; i < AwardBCount ; i++ )
                    {
                        decimal newscore;
                        if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@AwardB" + ( i + 1 )), out newscore) )
                            addScore = newscore;
                        subScore += addScore;
                    }
                    subScoreElement.SetAttribute("Score", "" + subScore);
                    element.AppendChild(subScoreElement);
                    finalScore += subScore;
                }
                #endregion
                #region 處理嘉獎
                if ( AwardCCount > 0 )
                {
                    subScoreElement = doc.CreateElement("SubScore");
                    subScore = 0;
                    subScoreElement.SetAttribute("Type", "獎懲");
                    subScoreElement.SetAttribute("Name", "嘉獎");
                    subScoreElement.SetAttribute("Count", "" + AwardCCount);
                    decimal addScore = 0;
                    for ( int i = 0 ; i < AwardCCount ; i++ )
                    {
                        decimal newscore;
                        if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@AwardC" + ( i + 1 )), out newscore) )
                            addScore = newscore;
                        subScore += addScore;
                    }
                    subScoreElement.SetAttribute("Score", "" + subScore);
                    element.AppendChild(subScoreElement);
                    finalScore += subScore;
                }
                #endregion
                #region 處理大過
                if ( FaultACount > 0 )
                {
                    subScoreElement = doc.CreateElement("SubScore");
                    subScore = 0;
                    subScoreElement.SetAttribute("Type", "獎懲");
                    subScoreElement.SetAttribute("Name", "大過");
                    subScoreElement.SetAttribute("Count", "" + FaultACount);
                    decimal addScore = 0;
                    for ( int i = 0 ; i < FaultACount ; i++ )
                    {
                        decimal newscore;
                        if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@FaultA" + ( i + 1 )), out newscore) )
                            addScore = newscore * ( -1 );
                        subScore += addScore;
                    }
                    subScoreElement.SetAttribute("Score", "" + subScore);
                    element.AppendChild(subScoreElement);
                    finalScore += subScore;
                }
                #endregion
                #region 處理小過
                if ( FaultBCount > 0 )
                {
                    subScoreElement = doc.CreateElement("SubScore");
                    subScore = 0;
                    subScoreElement.SetAttribute("Type", "獎懲");
                    subScoreElement.SetAttribute("Name", "小過");
                    subScoreElement.SetAttribute("Count", "" + FaultBCount);
                    decimal addScore = 0;
                    for ( int i = 0 ; i < FaultBCount ; i++ )
                    {
                        decimal newscore;
                        if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@FaultB" + ( i + 1 )), out newscore) )
                            addScore = newscore * ( -1 );
                        subScore += addScore;
                    }
                    subScoreElement.SetAttribute("Score", "" + subScore);
                    element.AppendChild(subScoreElement);
                    finalScore += subScore;
                }
                #endregion
                #region 處理警告
                if ( FaultCCount > 0 )
                {
                    subScoreElement = doc.CreateElement("SubScore");
                    subScore = 0;
                    subScoreElement.SetAttribute("Type", "獎懲");
                    subScoreElement.SetAttribute("Name", "警告");
                    subScoreElement.SetAttribute("Count", "" + FaultCCount);
                    decimal addScore = 0;
                    for ( int i = 0 ; i < FaultCCount ; i++ )
                    {
                        decimal newscore;
                        if ( decimal.TryParse(_MoralConductHelper.GetText("RewardCalcRule/@FaultC" + ( i + 1 )), out newscore) )
                            addScore = newscore * ( -1 );
                        subScore += addScore;
                    }
                    subScoreElement.SetAttribute("Score", "" + subScore);
                    element.AppendChild(subScoreElement);
                    finalScore += subScore;
                }
                #endregion
                #endregion
                #endregion
                #region 處理缺曠
                Dictionary<string, int> attendanceCount = new Dictionary<string, int>();
                bool noabsence = true;
                foreach ( UsefulPeriodAbsence u in UsefulPeriodAbsences )
                {
                    attendanceCount.Add(u.Period + "_" + u.Absence, 0);
                }
                foreach ( AttendanceInfo attendance in student.AttendanceList )
                {
                    if ( attendance.SchoolYear != schoolyear || attendance.Semester != semester )
                        continue;
                    //假別次數

                    //取得對照表並且對照出節次->類別的清單(99/11/24 by dylan)
                    if (periodDic.ContainsKey(attendance.Period))
                    {
                        if (attendanceCount.ContainsKey(periodDic[attendance.Period] + "_" + attendance.Absence))
                            attendanceCount[periodDic[attendance.Period] + "_" + attendance.Absence]++;
                    }

                    //全勤判斷
                    if ( !_NoabsenceList.Contains(attendance.Absence) )
                        noabsence = false;
                }
                //填入加減分缺曠
                foreach ( UsefulPeriodAbsence u in UsefulPeriodAbsences )
                {
                    if ( attendanceCount[u.Period + "_" + u.Absence] > 0 )
                    {
                        subScoreElement = doc.CreateElement("SubScore");
                        subScore = 0;
                        subScore = u.Subtract * ( attendanceCount[u.Period + "_" + u.Absence] / u.Aggregated * ( -1 ) );
                        subScoreElement.SetAttribute("Type", "缺曠");
                        subScoreElement.SetAttribute("Absence", u.Absence);
                        subScoreElement.SetAttribute("PeriodType", u.Period);
                        subScoreElement.SetAttribute("Count", "" + attendanceCount[u.Period + "_" + u.Absence]);
                        subScoreElement.SetAttribute("Score", "" + subScore);
                        element.AppendChild(subScoreElement);
                        finalScore += subScore;
                    }
                }
                //填入全勤加分
                if ( noabsence )
                {
                    subScoreElement = doc.CreateElement("SubScore");
                    subScore = 0;
                    decimal.TryParse(_MoralConductHelper.GetText("PeriodAbsenceCalcRule/@NoAbsenceReward"), out subScore);
                    subScoreElement.SetAttribute("Type", "全勤");
                    subScoreElement.SetAttribute("Score", "" + subScore);
                    element.AppendChild(subScoreElement);
                    finalScore += subScore;
                }
                #endregion
                #region 處理加減分及評語
                foreach ( SemesterMoralScoreInfo moralscore in student.SemesterMoralScoreList )
                {
                    //是這學期的
                    if ( moralscore.SchoolYear == schoolyear && moralscore.Semester == semester )
                    {
                        //導師加減分
                        subScoreElement = doc.CreateElement("SubScore");
                        subScore = 0;
                        subScore = moralscore.SupervisedByDiff;
                        subScoreElement.SetAttribute("Type", "加減分");
                        subScoreElement.SetAttribute("DiffItem", "導師加減分");
                        subScoreElement.SetAttribute("Score", "" + subScore);
                        element.AppendChild(subScoreElement);
                        finalScore += subScore;
                        #region 其他加減分
                        if ( moralscore.OtherDiff != null )
                        {
                            foreach ( string diffItem in moralscore.OtherDiff.Keys )
                            {
                                subScoreElement = doc.CreateElement("SubScore");
                                subScore = 0;
                                subScore = moralscore.OtherDiff[diffItem];
                                subScoreElement.SetAttribute("Type", "加減分");
                                subScoreElement.SetAttribute("DiffItem", diffItem);
                                subScoreElement.SetAttribute("Score", "" + subScore);
                                element.AppendChild(subScoreElement);
                                finalScore += subScore;
                            }
                        }
                        #endregion
                        #region 評語
                        subScoreElement = doc.CreateElement("Others");
                        subScoreElement.SetAttribute("Comment", moralscore.SupervisedByComment);
                        element.AppendChild(subScoreElement);
                        #endregion
                    }
                }
                #endregion
                element.SetAttribute("RealScore", "" + finalScore);
                element.SetAttribute("Score", "" + GetRoundScore(
                    (limit100&&finalScore>100)?100:finalScore,//超過一百以一百分計
                    decimals,
                    mode));
                student.Fields.Add("DemonScore", element);
            }
        }

        public Dictionary<StudentRecord, List<string>> FillSchoolYearDemonScore(int schoolyear, AccessHelper accesshelper, List<StudentRecord> students)
        {
            Dictionary<StudentRecord, List<string>> _ErrorList = new Dictionary<StudentRecord, List<string>>();
            //抓成績資料
            accesshelper.StudentHelper.FillSemesterEntryScore(false, students);
            foreach ( StudentRecord var in students )
            {
                //計算結果
                Dictionary<string, decimal> entryCalcScores = new Dictionary<string, decimal>();

                //精準位數
                int decimals = 2;
                if ( !int.TryParse(_MoralConductHelper.GetText("BasicScore/@Decimals"), out decimals) )
                    decimals = 2;
                //進位模式
                SmartSchool.Evaluation.WearyDogComputer.RoundMode mode = SmartSchool.Evaluation.WearyDogComputer.RoundMode.四捨五入;
                switch ( _MoralConductHelper.GetText("BasicScore/@DecimalType") )
                {
                    default:
                    case "四捨五入":
                        mode = SmartSchool.Evaluation.WearyDogComputer.RoundMode.四捨五入;
                        break;
                    case "無條件捨去":
                        mode = SmartSchool.Evaluation.WearyDogComputer.RoundMode.無條件捨去;
                        break;
                    case "無條件進位":
                        mode = SmartSchool.Evaluation.WearyDogComputer.RoundMode.無條件進位;
                        break;
                }

                int? gradeyear = null;
                #region 抓年級
                foreach ( SemesterEntryScoreInfo score in var.SemesterEntryScoreList )
                {
                    if ( score.Entry == "德行" && score.SchoolYear == schoolyear )
                    {
                        if ( gradeyear == null || score.GradeYear > gradeyear )
                            gradeyear = score.GradeYear;
                    }
                }
                #endregion
                if ( gradeyear != null )
                {
                    #region 移除不需要成績
                    Dictionary<int, int> ApplySemesterSchoolYear = new Dictionary<int, int>();
                    //先掃一遍抓出該年級最高的學年度
                    foreach ( SemesterEntryScoreInfo scoreInfo in var.SemesterEntryScoreList )
                    {
                        if ( scoreInfo.SchoolYear <= schoolyear && scoreInfo.GradeYear == gradeyear )
                        {
                            if ( !ApplySemesterSchoolYear.ContainsKey(scoreInfo.Semester) )
                                ApplySemesterSchoolYear.Add(scoreInfo.Semester, scoreInfo.SchoolYear);
                            else
                            {
                                if ( ApplySemesterSchoolYear[scoreInfo.Semester] < scoreInfo.SchoolYear )
                                    ApplySemesterSchoolYear[scoreInfo.Semester] = scoreInfo.SchoolYear;
                            }
                        }
                    }
                    //如果成績資料的年級學年度不在清單中就移掉
                    List<SemesterEntryScoreInfo> removeList = new List<SemesterEntryScoreInfo>();
                    foreach ( SemesterEntryScoreInfo scoreInfo in var.SemesterEntryScoreList )
                    {
                        if ( !ApplySemesterSchoolYear.ContainsKey(scoreInfo.Semester) || ApplySemesterSchoolYear[scoreInfo.Semester] != scoreInfo.SchoolYear )
                            removeList.Add(scoreInfo);
                    }
                    foreach ( SemesterEntryScoreInfo scoreInfo in removeList )
                    {
                        var.SemesterEntryScoreList.Remove(scoreInfo);
                    }
                    #endregion
                    #region 計算該年級的分項成績
                    Dictionary<string, List<decimal>> entryScores = new Dictionary<string, List<decimal>>();
                    foreach ( SemesterEntryScoreInfo score in var.SemesterEntryScoreList )
                    {
                        if ( score.Entry == "德行" && score.SchoolYear <= schoolyear && score.GradeYear == gradeyear )
                        {
                            if ( !entryScores.ContainsKey(score.Entry) )
                                entryScores.Add(score.Entry, new List<decimal>());
                            entryScores[score.Entry].Add(score.Score);
                        }
                    }
                    foreach ( string key in entryScores.Keys )
                    {
                        decimal sum = 0;
                        decimal count = 0;
                        foreach ( decimal sc in entryScores[key] )
                        {
                            sum += sc;
                            count += 1;
                        }
                        if ( count > 0 )
                            entryCalcScores.Add(key, GetRoundScore(sum / count, decimals, mode));
                    }
                    #endregion
                }
                if ( var.Fields.ContainsKey("CalcSchoolYearMoralScores") )
                    var.Fields["CalcSchoolYearMoralScores"] = entryCalcScores;
                else
                    var.Fields.Add("CalcSchoolYearMoralScores", entryCalcScores);
            }
            return _ErrorList;
        }

        public List<UsefulPeriodAbsence> UsefulPeriodAbsences
        {
            get { return _UsefulPeriodAbsences; }
        }

        public string ParseLevel(decimal score)
        {
            foreach (string var in _degreeList.Keys)
            {
                if (_degreeList[var] <= score)
                    return var;
            }
            return "";
        }

        public class UsefulPeriodAbsence
        {
            private string _Absence;
            private string _Period;
            private decimal _Subtract;
            private int _Aggregated;
            public UsefulPeriodAbsence(string absence,string period,decimal subtract,int aggregated)
            {
                _Absence = absence;
                _Period = period;
                _Subtract = subtract;
                _Aggregated = aggregated;
            }
            /// <summary>
            /// 假別
            /// </summary>
            public string Absence
            {
                get { return _Absence; }
            }
            /// <summary>
            /// 節次類別
            /// </summary>
            public string Period
            {
                get { return _Period; }
            }
            /// <summary>
            /// 扣分
            /// </summary>
            public decimal Subtract
            {
                get { return _Subtract; }
            }
            /// <summary>
            /// 累計次數
            /// </summary>
            public int Aggregated
            {
                get { return _Aggregated; }
            }
        }

    }
}
