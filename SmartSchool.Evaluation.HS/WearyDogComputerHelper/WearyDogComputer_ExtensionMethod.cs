using System.Collections.Generic;
using System.Linq;
using SmartSchool.Customization.Data.StudentExtension;
using SmartSchool.Evaluation;
using SmartSchool.Customization.Data;

namespace SmartSchool.Evaluation.WearyDogComputerHelper
{
    public static class WearyDogComputer_ExtensionMethod
    {
        /// <summary>
        /// 根據學期科目成績找出是否有上學期科目大於等於50分，小於60分；而且下學期有相同的學期科目成績是大於等於60分的科目情況。
        /// </summary>
        /// <param name="Scores"></param>
        /// <returns></returns>
        //public static List<string> SchoolYearScore(this IEnumerable<SemesterSubjectScoreInfo> Scores)
        //{
        //    List<string> SchoolYearSubjects = new List<string>();

        //    //Step1:將學期科目成績根據學年來做群組
        //    foreach (var SchoolYearScore in Scores.GroupBy(x => x.SchoolYear))
        //    {
        //        //Step2:將學期科目成績根據科目名稱來做群組
        //        foreach (var SchoolYearSubjectScore in SchoolYearScore.GroupBy(x => x.Subject))
        //        {
        //            //Step3:假設同個科目在同樣的學年內有兩筆以上
        //            if (SchoolYearSubjectScore.ToList().Count >= 2)
        //            {
        //                List<SemesterSubjectScoreInfo> records = SchoolYearSubjectScore.OrderBy(x => x.Semester).ToList();

        //                //上學期科目成績
        //                decimal? UpScore = K12.Data.Decimal.ParseAllowNull(records[0].Detail.GetAttribute("MaxScore"));

        //                //下學期科目成績
        //                decimal? DownScore = K12.Data.Decimal.ParseAllowNull(records[1].Detail.GetAttribute("MaxScore"));
                        
        //                //判斷是否上學期科目成績大於50小於60，並且下學期科目成績大於60
        //                if ((UpScore >= 50 && UpScore < 60) && DownScore >= 60)
        //                    SchoolYearSubjects.Add(records[0].Subject);
        //                    //SchoolYearSubjects.Add(records[0].SchoolYear+","+records[0].Subject);
        //            }
        //        }
        //    }

        //    return SchoolYearSubjects;
        //}

        /// <summary>
        /// 根據學年度取得學年科目成績列表的成績年級
        /// </summary>
        /// <param name="Scores">學年科目成績</param>
        /// <param name="schoolyear">學年度</param>
        /// <returns>成績年級</returns>
        public static int? GetGradeYear(this List<SchoolYearSubjectScoreInfo> Scores, int schoolyear)
        {
            int? gradeyear = null;
            #region 抓年級
            foreach (SchoolYearSubjectScoreInfo score in Scores)
            {
                if (score.SchoolYear == schoolyear)
                    if (gradeyear == null || score.GradeYear > gradeyear)
                        gradeyear = score.GradeYear;
            }
            #endregion 

            return gradeyear;
        }

        public static int? GetGradeYear(this List<SemesterEntryScoreInfo> Scores, int schoolyear)
        {
            int? gradeyear = null;

            #region 判斷年級
            List<string> enableEntryLists = new List<string>(new string[] { "體育", "國防通識", "健康與護理", "實習科目", "學業" });
            foreach (SemesterEntryScoreInfo score in Scores)
            {
                if (enableEntryLists.Contains(score.Entry) && score.SchoolYear == schoolyear)
                {
                    if (gradeyear == null || score.GradeYear > gradeyear)
                        gradeyear = score.GradeYear;
                }
            }
            #endregion

            return gradeyear;
        }


        public static void FilterSchoolYearSubjectScore(this List<SchoolYearSubjectScoreInfo> Scores, int? gradeyear, int schoolyear)
        {
            if (gradeyear != null)
            {
                int ApplySchoolYear = schoolyear;

                //先掃一遍抓出該年級最高的學年度
                foreach (SchoolYearSubjectScoreInfo scoreInfo in Scores)
                    if (scoreInfo.SchoolYear <= schoolyear && scoreInfo.GradeYear == gradeyear)
                        if (ApplySchoolYear < scoreInfo.SchoolYear)
                            ApplySchoolYear = scoreInfo.SchoolYear;

                //如果成績資料的年級學年度不在清單中就移掉
                List<SchoolYearSubjectScoreInfo> removeList = new List<SchoolYearSubjectScoreInfo>();

                foreach (SchoolYearSubjectScoreInfo scoreInfo in Scores)
                {
                    if (ApplySchoolYear != scoreInfo.SchoolYear)
                        removeList.Add(scoreInfo);
                }

                foreach (SchoolYearSubjectScoreInfo scoreInfo in removeList)
                {
                    Scores.Remove(scoreInfo);
                }
            }  
        }

        /// <summary>
        /// 將不需要的學期科目成績刪除
        /// </summary>
        /// <param name="Scores">學期科目成績</param>
        /// <param name="gradeyear">年級</param>
        /// <param name="schoolyear">學年度</param>
        public static void FilterSemesterSubjectScore(this List<SemesterSubjectScoreInfo> Scores, int? gradeyear,int schoolyear) 
        {
            if (gradeyear != null)
            {
                Dictionary<int, int> ApplySemesterSchoolYear = new Dictionary<int, int>();
                //先掃一遍抓出該年級最高的學年度
                foreach (SemesterSubjectScoreInfo scoreInfo in Scores)
                {
                    if (scoreInfo.SchoolYear <= schoolyear && scoreInfo.GradeYear == gradeyear)
                    {
                        //沒有包含該學期的話先將學期及學年度加入
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
                List<SemesterSubjectScoreInfo> removeList = new List<SemesterSubjectScoreInfo>();
                foreach (SemesterSubjectScoreInfo scoreInfo in Scores)
                {
                    if (!ApplySemesterSchoolYear.ContainsKey(scoreInfo.Semester) || ApplySemesterSchoolYear[scoreInfo.Semester] != scoreInfo.SchoolYear)
                        removeList.Add(scoreInfo);
                }
                foreach (SemesterSubjectScoreInfo scoreInfo in removeList)
                {
                    Scores.Remove(scoreInfo);
                }
            } 
        }

        public static Dictionary<string, decimal> CalculateSchoolYearEntryScore(this StudentRecord student, Dictionary<string, bool> calcEntry, Dictionary<string, bool> calcInStudy, int schoolyear, int gradeyear, WearyDogComputer.RoundMode mode, int decimals)
        {
            Dictionary<string, decimal> entryCalcScores = new Dictionary<string, decimal>();

            //學年科目成績的學分數
            Dictionary<string, decimal> subjectCreditCount = new Dictionary<string, decimal>();

            //各個分項成績的學分數
            Dictionary<string, decimal> entryCreditCount = new Dictionary<string, decimal>();

            //學年科目成績的各個科目成績列表
            Dictionary<string, List<decimal>> entrySubjectScores = new Dictionary<string, List<decimal>>();

            //學年科目成績的加權總計
            Dictionary<string, decimal> entryDividend = new Dictionary<string, decimal>();

            #region 加總學年科目成績對應到的學期科目成績，各個科目的學分數加總
            foreach (SemesterSubjectScoreInfo score in student.SemesterSubjectScoreList)
            {
                if (!subjectCreditCount.ContainsKey(score.Subject))
                    subjectCreditCount.Add(score.Subject, 0);
                subjectCreditCount[score.Subject] += score.CreditDec();
            }
            #endregion

            #region 計算該年級的分項成績
            //Dictionary<string, List<decimal>> entryScores = new Dictionary<string, List<decimal>>();
            
            //foreach (SemesterEntryScoreInfo score in Scores)
            //{
            //    if (calcEntry.ContainsKey(score.Entry) && score.SchoolYear <= schoolyear && score.GradeYear == gradeyear)
            //    {
            //        if (!entryScores.ContainsKey(score.Entry))
            //            entryScores.Add(score.Entry, new List<decimal>());
            //        entryScores[score.Entry].Add(score.Score);
            //    }
            //}
            
            //foreach (string key in entryScores.Keys)
            //{
            //    decimal sum = 0;
            //    decimal count = 0;
            //    foreach (decimal sc in entryScores[key])
            //    {
            //        sum += sc;
            //        count += 1;
            //    }
            //    if (count > 0)
            //        entryCalcScores.Add(key, WearyDogComputer.GetRoundScore(sum / count, decimals, mode));
            //}
            #endregion

            #region 將成績分到各分項類別中
            foreach (SchoolYearSubjectScoreInfo subjectNode in student.SchoolYearSubjectScoreList)
            {
                //if (subjectNode.SchoolYear == schoolyear && subjectNode.Semester == semester)
                //留意是==schoolyear或是<=schoolyear
                if (subjectNode.SchoolYear == schoolyear && subjectNode.GradeYear == gradeyear)
                {
                    //不計學分或不需評分不用算
                    //if (subjectNode.Detail.GetAttribute("不需評分") == "是" || subjectNode.Detail.GetAttribute("不計學分") == "是")
                    //    continue;
                    #region 分項類別跟學分數
                    //string entry = subjectNode.Detail.GetAttribute("開課分項類別");
                     //int credit = subjectNode.CreditDec();
                    decimal credit = subjectCreditCount.ContainsKey(subjectNode.Subject)?subjectCreditCount[subjectNode.Subject]:0 ;
                    #endregion
                    decimal maxScore = subjectNode.Score;
                    #region 取得最高分數
                    //decimal tryParseDecimal;
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

                    //switch (entry)
                    //{
                    //    case "體育":
                    //    case "國防通識":
                    //    case "健康與護理":
                    //    case "實習科目":
                    //        //計算分項成績
                    //        if (calcEntry[entry])
                    //        {
                    //            //加總學分數
                    //            if (!entryCreditCount.ContainsKey(entry))
                    //                entryCreditCount.Add(entry, credit);
                    //            else
                    //                entryCreditCount[entry] += credit;
                    //            //加入將成績資料分項
                    //            if (!entrySubjectScores.ContainsKey(entry)) entrySubjectScores.Add(entry, new List<decimal>());
                    //            entrySubjectScores[entry].Add(maxScore);
                    //            //加權總計
                    //            if (!entryDividend.ContainsKey(entry))
                    //                entryDividend.Add(entry, maxScore * credit);
                    //            else
                    //                entryDividend[entry] += (maxScore * credit);
                    //        }
                    //        //將科目成績與學業成績一併計算
                    //        if (calcInStudy[entry])
                    //        {
                    //            //加總學分數
                    //            if (!entryCreditCount.ContainsKey("學業"))
                    //                entryCreditCount.Add("學業", credit);
                    //            else
                    //                entryCreditCount["學業"] += credit;
                    //            //加入將成績資料分項
                    //            if (!entrySubjectScores.ContainsKey("學業")) entrySubjectScores.Add("學業", new List<decimal>());
                    //            entrySubjectScores["學業"].Add(maxScore);
                    //            //加權總計
                    //            if (!entryDividend.ContainsKey("學業"))
                    //                entryDividend.Add("學業", maxScore * credit);
                    //            else
                    //                entryDividend["學業"] += (maxScore * credit);
                    //        }
                    //        break;

                    //    case "學業":
                    //    default:
                    //        //加總學分數
                    //        if (!entryCreditCount.ContainsKey("學業"))
                    //            entryCreditCount.Add("學業", credit);
                    //        else
                    //            entryCreditCount["學業"] += credit;
                    //        //加入將成績資料分項
                    //        if (!entrySubjectScores.ContainsKey("學業")) entrySubjectScores.Add("學業", new List<decimal>());
                    //        entrySubjectScores["學業"].Add(maxScore);
                    //        //加權總計
                    //        if (!entryDividend.ContainsKey("學業"))
                    //            entryDividend.Add("學業", maxScore * credit);
                    //        else
                    //            entryDividend["學業"] += (maxScore * credit);
                    //        break;
                    //}
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
                entryScore = WearyDogComputer.GetRoundScore(entryScore, decimals, mode);
                #region 填入EntryScores
                entryCalcScores.Add(entry, entryScore);
                #endregion
            }
            #endregion

            return entryCalcScores;
        }
    }
}