using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JHSchool.Evaluation.Calculation;

namespace KaoHsiungExamScore_JH.DAO
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

            string ss = "_加權", sc = "_學分", sa = "_平時加權", sc1 = "_學分值";

            foreach (ExamSubjectScore ess in _ExamSubjectScoreDict.Values)
            {
                string keys = ess.DomainName + ss;
                string keyc = ess.DomainName + sc;
                string keyc1 = ess.DomainName + sc1;
                string keya=ess.DomainName+sa;

                if (!DomainNameList.Contains(ess.DomainName))
                    DomainNameList.Add(ess.DomainName);

                if (!scDict.ContainsKey(keys)) scDict.Add(keys, new List<decimal>());
                if (!scDict.ContainsKey(keyc)) scDict.Add(keyc, new List<decimal>());
                if (!scDict.ContainsKey(keyc1)) scDict.Add(keyc1, new List<decimal>());
                if (!scDict.ContainsKey(keya)) scDict.Add(keya, new List<decimal>());

                // 評量加權
                if(ess.Score.HasValue && ess.Credit.HasValue)
                    if(ess.Credit.Value>0)
                        scDict[keys].Add(_Calculator.ParseSubjectScore(ess.Score.Value * ess.Credit.Value));

                // 平時加權
                if(ess.AssignmentScore.HasValue && ess.Credit.HasValue)
                    if(ess.Credit.HasValue)
                        scDict[keya].Add(_Calculator.ParseSubjectScore(ess.AssignmentScore.Value * ess.Credit.Value));

                // 學分數加總
                if (ess.Credit.HasValue)
                    scDict[keyc].Add(ess.Credit.Value);

                // 有評量成績，有學分數加總
                if (ess.Credit.HasValue && ess.Score.HasValue)
                    scDict[keyc1].Add(ess.Credit.Value);
            }

            foreach (string name in DomainNameList)
             {
                ExamDomainScore eds = new ExamDomainScore();
                // 處理彈性課程
                if (string.IsNullOrEmpty(name))
                    eds.DomainName = "彈性課程";
                else
                    eds.DomainName = name;

                string skeyC=eds.DomainName + sc;
                string skeyC1=eds.DomainName + sc1;
                if(scDict.ContainsKey(skeyC))
                    eds.Credit = scDict[skeyC].Sum();
                if(scDict.ContainsKey(skeyC1))
                    eds.Credit1 = scDict[skeyC1].Sum();

                // 有學分數有成績才進入計算
                // 評量加權
                string skeyS = eds.DomainName + ss;
                string skeyA=eds.DomainName + sa;
                if(eds.Credit1.HasValue)
                    if(eds.Credit1.Value>0)
                        if(scDict.ContainsKey(skeyS))
                            eds.Score = _Calculator.ParseDomainScore(scDict[skeyS].Sum() / eds.Credit1.Value);
                
                // 平時加權
                if (eds.Credit1.HasValue)
                    if(eds.Credit1.Value>0)
                        if(scDict.ContainsKey(skeyA))
                            eds.AssignmentScore = _Calculator.ParseDomainScore(scDict[skeyA].Sum() / eds.Credit1.Value);
        
                _ExamDomainScoreDict.Add(name, eds);
            }

        }

        /// <summary>
        /// 評量領域加權平均
        /// </summary>
        /// <returns></returns>
        public decimal? GetDomainScoreA()
        {
            decimal? score=null;
            decimal ss = 0;            
            decimal  cc = 0;
            foreach (ExamDomainScore sc in _ExamDomainScoreDict.Values)
            {
                //  使用有成績去計算加權
                if (sc.Score.HasValue && sc.Credit1.HasValue)
                {
                    ss += sc.Score.Value * sc.Credit1.Value;
                    cc += sc.Credit1.Value;
                }
            }

            if (cc > 0)
                score = _Calculator.ParseDomainScore(ss/cc);

            return score;
        }

        /// <summary>
        /// 評量科目評量分數加權平均
        /// </summary>
        /// <returns></returns>
        public decimal? GetSubjectScoreAF()
        {
            decimal? score = null;
            decimal ss = 0;
            decimal cc = 0;
            foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
            {
                if (sc.Score.HasValue && sc.Credit.HasValue)
                {                    
                    ss += sc.Score.Value * sc.Credit.Value;
                    cc += sc.Credit.Value;
                }
            }

            if (cc > 0)
                score = _Calculator.ParseSubjectScore(ss / cc);

            return score;
        }

        /// <summary>
        /// 評量科目平時成績加權平均
        /// </summary>
        /// <returns></returns>
        public decimal? GetSubjectScoreAA()
        {
            decimal? score = null;
            decimal ss = 0;
            decimal cc = 0;
            foreach (ExamSubjectScore sc in _ExamSubjectScoreDict.Values)
            {
                if (sc.AssignmentScore.HasValue && sc.Credit.HasValue)
                {
                    ss += sc.AssignmentScore.Value * sc.Credit.Value;
                    cc += sc.Credit.Value;
                }
            }

            if (cc > 0)
                score = _Calculator.ParseSubjectScore(ss / cc);

            return score;
        }    
    }
}
