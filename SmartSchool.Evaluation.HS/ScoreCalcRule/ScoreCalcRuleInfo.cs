using System.Collections.Generic;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.StudentRelated;
using SmartSchool.TagManage;

namespace SmartSchool.Evaluation.ScoreCalcRule
{
    public class ScoreCalcRuleInfo
    {
        private readonly string _ID;
        private readonly string _Name;
        private readonly XmlElement _ScoreCalcRuleElement;

        private readonly string _SchoolYear;
        private readonly string _TrimName;
        //private readonly bool _DefinedSubjectInfoByGPlan;

        /// <summary>
        /// 成績計算規則
        /// </summary>
        /// <param name="scrElement"></param>
        internal ScoreCalcRuleInfo(XmlElement scrElement)
        {
            _ID = scrElement.GetAttribute("ID");
            _Name = scrElement.SelectSingleNode("Name").InnerText;
            _ScoreCalcRuleElement = (XmlElement)scrElement.SelectSingleNode("Content/ScoreCalcRule");
            
            _SchoolYear = _ScoreCalcRuleElement.HasAttribute("SchoolYear") ? _ScoreCalcRuleElement.GetAttribute("SchoolYear") : string.Empty;
            _TrimName = _Name;
            if (!string.IsNullOrEmpty(_SchoolYear))
            {
                //_TrimName = _Name.TrimStart(_SchoolYear.ToCharArray());
                _TrimName = _Name.Substring(_SchoolYear.Length);
            }
            //_DefinedSubjectInfoByGPlan = false;
            //if (_ScoreCalcRuleElement.SelectSingleNode("學分及修課資訊採計方式") == null || ((XmlElement)_ScoreCalcRuleElement.SelectSingleNode("學分及修課資訊採計方式")).GetAttribute("由課程規劃表取得") == "True")
            //    _DefinedSubjectInfoByGPlan = true;
        }

        public string ID { get { return _ID; } }
        public string Name { get { return _Name; } }
        public string SchoolYear { get { return _SchoolYear; } }
        public string TrimName { get { return _TrimName; } }
        //public bool DefinedSubjectInfoByGPlan{get { return _DefinedSubjectInfoByGPlan; }}
        public XmlElement ScoreCalcRuleElement
        {
            get
            {
                return (XmlElement)(new XmlDocument().ImportNode(_ScoreCalcRuleElement, true));
            }
        }

        public XmlElement CalculateSemesterEntryScore(XmlElement semesterSubjectScore)
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
            WearyDogComputer.RoundMode mode = WearyDogComputer.RoundMode.四捨五入;
            //成績年級及計算規則皆存在，允許計算成績
            bool canCalc = true;
            #region 取得成績年級跟計算規則

            #region 處理計算規則
            XmlElement scoreCalcRule = (XmlElement)_ScoreCalcRuleElement;//ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID) == null ? null : ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var.StudentID).ScoreCalcRuleElement;

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
                    mode = WearyDogComputer.RoundMode.四捨五入;
                if (bool.TryParse(helper.GetText("各項成績計算位數/學期分項成績計算位數/@無條件捨去"), out tryParsebool) && tryParsebool)
                    mode = WearyDogComputer.RoundMode.無條件捨去;
                if (bool.TryParse(helper.GetText("各項成績計算位數/學期分項成績計算位數/@無條件進位"), out tryParsebool) && tryParsebool)
                    mode = WearyDogComputer.RoundMode.無條件進位;
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

            #endregion

            #endregion
            Dictionary<string, decimal> entryScores = new Dictionary<string, decimal>();

            #region 將成績分到各分項類別中
            foreach (XmlNode subjectNode in semesterSubjectScore.SelectNodes("Subject"))
            {
                XmlElement subjectElement = (XmlElement)subjectNode;
                //不計學分或不需評分不用算
                if (subjectElement.GetAttribute("不需評分") == "是" || subjectElement.GetAttribute("不計學分") == "是")
                    continue;
                #region 分項類別跟學分數
                string entry = subjectElement.GetAttribute("開課分項類別");
                decimal credit = 0;
                decimal.TryParse(subjectElement.GetAttribute("開課學分數"), out credit);
                #endregion
                decimal maxScore = 0;
                decimal original = 0;
                if (decimal.TryParse(subjectElement.GetAttribute("原始成績"), out tryParseDecimal))
                    original = tryParseDecimal;
                #region 取得最高分數
                foreach (var item in takeScore)
                {
                    if (decimal.TryParse(subjectElement.GetAttribute(item), out tryParseDecimal) && maxScore < tryParseDecimal)
                        maxScore = tryParseDecimal;
                }
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
            #endregion

            XmlDocument doc = new XmlDocument();
            XmlElement entryScoreRoot = doc.CreateElement("SemesterEntryScore");
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
                #region 填入Xml
                XmlElement entryElement = doc.CreateElement("Entry");
                entryElement.SetAttribute("分項", entry);
                entryElement.SetAttribute("成績", entryScore.ToString());
                entryScoreRoot.AppendChild(entryElement);
                #endregion
            }
            #endregion
            return entryScoreRoot;
        }

        public decimal GetStudentPassScore(BriefStudentData student, int gradeYear)
        {
            string childElement;
            switch (gradeYear)
            {
                case 1: childElement = "一年級及格標準"; break;
                case 2: childElement = "二年級及格標準"; break;
                case 3: childElement = "三年級及格標準"; break;
                case 4: childElement = "四年級及格標準"; break;
                default: childElement = "超過了啦"; break;
            }
            decimal passScore = decimal.MaxValue, tryPraseScore;
            DSXmlHelper helper = new DSXmlHelper(_ScoreCalcRuleElement);
            foreach (XmlElement element in helper.GetElements("及格標準/學生類別"))
            {
                string tagName = element.GetAttribute("類別");
                if (tagName == "預設" && decimal.TryParse(element.GetAttribute(childElement), out tryPraseScore))
                {
                    if (tryPraseScore < passScore)
                        passScore = tryPraseScore;
                }
                else
                {
                    foreach (TagInfo tag in student.Tags)
                    {
                        if (((tag.Prefix == "" && tagName == tag.Name) || tagName == tag.FullName) && decimal.TryParse(element.GetAttribute(childElement), out tryPraseScore))
                        {
                            if (tryPraseScore < passScore)
                                passScore = tryPraseScore;
                            break;
                        }
                    }
                }
            }
            if (passScore == decimal.MaxValue)
                passScore = 60;
            return passScore;
        }

        private enum RoundMode { 四捨五入, 無條件進位, 無條件捨去 }
        private decimal GetRoundScore(decimal entryScore, int decimals, SmartSchool.Evaluation.WearyDogComputer.RoundMode mode)
        {
            return WearyDogComputer.GetRoundScore(entryScore, decimals, mode);
        }
    }
}
