using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JHSchool.Evaluation.Calculation;

namespace HsinChuExamScore_JH.DAO
{
    /// <summary>
    /// 學生評量成績
    /// </summary>
    public class StudExamScore
    {
        /// <summary>
        /// 學生系統編號
        /// </summary>
        public string StudentID { get; set; }

        /// <summary>
        /// 年級
        /// </summary>
        public int GradeYear { get; set; }

        /// <summary>
        /// 班級ID
        /// </summary>
        public string ClassID { get; set; }

        /// <summary>
        /// 試別名稱
        /// </summary>
        public string ExamName { get; set; }

        /// <summary>
        /// 領域成績
        /// </summary>
        public Dictionary<string, ExamDomainScore> _ExamDomainScoreDict = new Dictionary<string, ExamDomainScore>();

        /// <summary>
        /// 科目成績
        /// </summary>
        public Dictionary<string, ExamSubjectScore> _ExamSubjectScoreDict = new Dictionary<string, ExamSubjectScore>();

        /// <summary>
        /// 成績計算規則
        /// </summary>
        ScoreCalculator _Calculator;
        
        public StudExamScore(ScoreCalculator studentCalculator)
        { 
            _Calculator=studentCalculator;
        }

        public void CalcSubjectToDomain()
        {
            Dictionary<string, List<decimal>> scDict = new Dictionary<string, List<decimal>>();

            List<string> DomainNameList = new List<string>();
            string ss = "_加權", sc = "_學分", sa = "_平時加權", sf = "_定期加權", scs = "_學分值";

            foreach (ExamSubjectScore ess in _ExamSubjectScoreDict.Values)
            {
                string keys = ess.DomainName + ss;
                string keyc = ess.DomainName + sc;
                string keya = ess.DomainName + sa;
                string keyf = ess.DomainName + sf;
                string keycs = ess.DomainName + scs;

                if (!DomainNameList.Contains(ess.DomainName))
                    DomainNameList.Add(ess.DomainName);

                if (!scDict.ContainsKey(keys)) scDict.Add(keys, new List<decimal>());
                if (!scDict.ContainsKey(keyc)) scDict.Add(keyc, new List<decimal>());
                if (!scDict.ContainsKey(keya)) scDict.Add(keya, new List<decimal>());
                if (!scDict.ContainsKey(keyf)) scDict.Add(keyf, new List<decimal>());
                if (!scDict.ContainsKey(keycs)) scDict.Add(keycs, new List<decimal>());

                // 總分加權平均
                if(ess.ScoreT.HasValue && ess.Credit.HasValue)
                    scDict[keys].Add(_Calculator.ParseSubjectScore(ess.ScoreT.Value * ess.Credit.Value));

                // 定期加權平均
                if (ess.ScoreF.HasValue && ess.Credit.HasValue)
                    scDict[keyf].Add(_Calculator.ParseSubjectScore(ess.ScoreF.Value * ess.Credit.Value));

                // 平時加權平均
                if (ess.ScoreA.HasValue && ess.Credit.HasValue)
                    scDict[keya].Add(_Calculator.ParseSubjectScore(ess.ScoreA.Value * ess.Credit.Value));


                // 學分
                if (ess.Credit.HasValue)
                    scDict[keyc].Add(ess.Credit.Value);

                // 有成績的學分數
                if(ess.Credit.HasValue && ess.ScoreT.HasValue)
                    scDict[keycs].Add(ess.Credit.Value);
            }


            foreach (string name in DomainNameList)
            {
                //string ss = "_加權", sc = "_學分", sa = "_平時加權", sf = "_定期加權", scs = "_學分值";
                ExamDomainScore eds = new ExamDomainScore();
                // 處理彈性課程
                if (string.IsNullOrEmpty(name))
                    eds.DomainName = "彈性課程";
                else
                    eds.DomainName = name;

                string keyc = name + sc;
                string keycs = name + scs;
                // 學分加總
                if(scDict.ContainsKey(keyc))
                    eds.Credit = scDict[keyc].Sum();

                // 有成績學分
                if(scDict.ContainsKey(keycs))
                    eds.Credit1 = scDict[keycs].Sum();

                string keyss = name + ss;
                string keysf = name + sf;
                string keysa = name + sa;
                // 領域總成績加權平均
                if(eds.Credit1.HasValue)
                    if(eds.Credit1.Value>0)
                        if(scDict.ContainsKey(keyss))
                            eds.ScoreT = _Calculator.ParseDomainScore(scDict[keyss].Sum() / eds.Credit1.Value);

                // 領域定期成績加權平均
                if (eds.Credit1.HasValue)
                    if (eds.Credit1.Value > 0)
                        if(scDict.ContainsKey(keysf))
                            eds.ScoreF = _Calculator.ParseDomainScore(scDict[keysf].Sum() / eds.Credit1.Value);

                // 領域平時成績加權平均
                if (eds.Credit1.HasValue)
                    if (eds.Credit1.Value > 0)
                        if(scDict.ContainsKey(keysa))
                            eds.ScoreA = _Calculator.ParseDomainScore(scDict[keysa].Sum() / eds.Credit1.Value);

                _ExamDomainScoreDict.Add(name, eds);
            }

        }

        /// <summary>
        /// 評量領域總成績加權平均
        /// </summary>
        /// <returns></returns>
        public decimal? GetDomainScoreA(bool all)
        {
            decimal? score=null;
            decimal ss = 0;            
            decimal  cc = 0;
            if (all)
            {
                foreach (ExamDomainScore sc in _ExamDomainScoreDict.Values)
                {
                    // 使用有成績計算加權
                    if (sc.ScoreT.HasValue && sc.Credit1.HasValue)
                    {
                        ss += sc.ScoreT.Value * sc.Credit1.Value;
                        cc += sc.Credit1.Value;
                    }
                }
            }
            else
            {
                foreach (ExamDomainScore sc in _ExamDomainScoreDict.Values)
                {
                    // 過濾彈性課程
                    if (string.IsNullOrEmpty(sc.DomainName) || sc.DomainName=="彈性課程")
                        continue;

                    // 使用有成績計算加權
                    if (sc.ScoreT.HasValue && sc.Credit1.HasValue)
                    {
                        ss += sc.ScoreT.Value * sc.Credit1.Value;
                        cc += sc.Credit1.Value;
                    }
                }
            }

            if (cc > 0)
                score = _Calculator.ParseDomainScore(ss/cc);

            return score;
        }

        /// <summary>
        /// 評量科目定期評量加權平均
        /// </summary>
        /// <returns></returns>
        public decimal? GetSubjectScoreAF(bool all)
        {
            decimal? score = null;
            decimal ss = 0;
            decimal cc = 0;

            if (all)
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    if (sc.ScoreF.HasValue && sc.Credit.HasValue)
                    {
                        ss += sc.ScoreF.Value * sc.Credit.Value;
                        cc += sc.Credit.Value;
                    }
                }
            }
            else
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    // 過濾彈性課程
                    if (string.IsNullOrEmpty(sc.DomainName) || sc.DomainName == "彈性課程")
                        continue;
   
                    if (sc.ScoreF.HasValue && sc.Credit.HasValue)
                    {
                        ss += sc.ScoreF.Value * sc.Credit.Value;
                        cc += sc.Credit.Value;
                    }
                }            
            }

            if (cc > 0)
                score = _Calculator.ParseSubjectScore(ss / cc);

            return score;
        }

        /// <summary>
        /// 評量科目平時評量加權平均
        /// </summary>
        /// <returns></returns>
        public decimal? GetSubjectScoreAA(bool all)
        {
            decimal? score = null;
            decimal ss = 0;
            decimal cc = 0;

            if (all)
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    if (sc.ScoreA.HasValue && sc.Credit.HasValue)
                    {
                        ss += sc.ScoreA.Value * sc.Credit.Value;
                        cc += sc.Credit.Value;
                    }
                }
            }
            else
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    // 過濾彈性課程
                    if (string.IsNullOrEmpty(sc.DomainName) || sc.DomainName == "彈性課程")
                        continue;

                    if (sc.ScoreA.HasValue && sc.Credit.HasValue)
                    {
                        ss += sc.ScoreA.Value * sc.Credit.Value;
                        cc += sc.Credit.Value;
                    }
                }            
            }

            if (cc > 0)
                score = _Calculator.ParseSubjectScore(ss / cc);

            return score;
        }

        /// <summary>
        /// 評量科目總成績加權平均
        /// </summary>
        /// <returns></returns>
        public decimal? GetSubjectScoreAT(bool all)
        {
            decimal? score = null;
            decimal ss = 0;
            decimal cc = 0;

            if (all)
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    if (sc.ScoreT.HasValue)
                    {
                        ss += sc.ScoreT.Value * sc.Credit.Value;
                        cc += sc.Credit.Value;
                    }
                }
            }
            else
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    // 過濾彈性課程
                    if (string.IsNullOrEmpty(sc.DomainName) || sc.DomainName == "彈性課程")
                        continue;

                    if (sc.ScoreT.HasValue)
                    {
                        ss += sc.ScoreT.Value * sc.Credit.Value;
                        cc += sc.Credit.Value;
                    }
                }
            }

            if (cc > 0)
                score =_Calculator.ParseSubjectScore(ss / cc);

            return score;
        }

        /// <summary>
        /// 評量領域總成績加權總分
        /// </summary>
        /// <returns></returns>
        public decimal? GetDomainScoreS(bool all)
        {
            decimal? score = null;
            decimal ss = 0;
            if (all)
            {
                foreach (ExamDomainScore sc in _ExamDomainScoreDict.Values)
                {
                    // 使用有成績計算加權
                    if (sc.ScoreT.HasValue && sc.Credit1.HasValue)
                    {
                        ss += sc.ScoreT.Value * sc.Credit1.Value;
                    }
                }
            }
            else
            {
                foreach (ExamDomainScore sc in _ExamDomainScoreDict.Values)
                {
                    // 過濾彈性課程
                    if (string.IsNullOrEmpty(sc.DomainName) || sc.DomainName == "彈性課程")
                        continue;

                    // 使用有成績計算加權
                    if (sc.ScoreT.HasValue && sc.Credit1.HasValue)
                    {
                        ss += sc.ScoreT.Value * sc.Credit1.Value;
                    }
                }
            }

            score = ss;

            return score;
        }

        /// <summary>
        /// 評量科目定期評量加權總分
        /// </summary>
        /// <returns></returns>
        public decimal? GetSubjectScoreSF(bool all)
        {
            decimal? score = null;
            decimal ss = 0;

            if (all)
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    if (sc.ScoreF.HasValue && sc.Credit.HasValue)
                    {
                        ss += sc.ScoreF.Value * sc.Credit.Value;
                    }
                }
            }
            else
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    // 過濾彈性課程
                    if (string.IsNullOrEmpty(sc.DomainName) || sc.DomainName == "彈性課程")
                        continue;

                    if (sc.ScoreF.HasValue && sc.Credit.HasValue)
                    {
                        ss += sc.ScoreF.Value * sc.Credit.Value;
                    }
                }
            }

            score = ss;

            return score;
        }

        /// <summary>
        /// 評量科目平時評量加權總分
        /// </summary>
        /// <returns></returns>
        public decimal? GetSubjectScoreSA(bool all)
        {
            decimal? score = null;
            decimal ss = 0;

            if (all)
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    if (sc.ScoreA.HasValue && sc.Credit.HasValue)
                    {
                        ss += sc.ScoreA.Value * sc.Credit.Value;
                    }
                }
            }
            else
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    // 過濾彈性課程
                    if (string.IsNullOrEmpty(sc.DomainName) || sc.DomainName == "彈性課程")
                        continue;

                    if (sc.ScoreA.HasValue && sc.Credit.HasValue)
                    {
                        ss += sc.ScoreA.Value * sc.Credit.Value;
                    }
                }
            }

            score = ss;

            return score;
        }

        /// <summary>
        /// 評量科目總成績加權總分
        /// </summary>
        /// <returns></returns>
        public decimal? GetSubjectScoreST(bool all)
        {
            decimal? score = null;
            decimal ss = 0;

            if (all)
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    if (sc.ScoreT.HasValue)
                    {
                        ss += sc.ScoreT.Value * sc.Credit.Value;
                    }
                }
            }
            else
            {
                foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
                {
                    // 過濾彈性課程
                    if (string.IsNullOrEmpty(sc.DomainName) || sc.DomainName == "彈性課程")
                        continue;

                    if (sc.ScoreT.HasValue)
                    {
                        ss += sc.ScoreT.Value * sc.Credit.Value;
                    }
                }
            }

            score = ss;

            return score;
        }

    }
}
