using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表學生的學期科目成績。
    /// </summary>
    class SemsSubjectScore : RatableScore
    {
        public SemsSubjectScore(XmlElement data)
            : base()
        {
            DSXmlHelper hlpData = new DSXmlHelper(data);

            _subject_name = hlpData.GetText("@科目");
            _level = hlpData.GetText("@科目級別");
            decimal origin_score = Utility.ParseDecimal(hlpData.GetText("@原始成績"), decimal.MinValue);
            decimal reset_score = Utility.ParseDecimal(hlpData.GetText("@補考成績"), decimal.MinValue);

            Score = Math.Max(origin_score, reset_score);
            ScoreName = _subject_name + _level;
        }

        private string _level;
        public string Level
        {
            get { return _level; }
        }

        private string _subject_name;
        public string SubjectName
        {
            get { return _subject_name; }
        }

    }

    class SemsSubjectScoreCollection : Dictionary<string, SemsSubjectScore>
    {
        private string _score_identity;

        public string ScoreRecordIdentity
        {
            get { return _score_identity; }
            set { _score_identity = value; }
        }

        public bool AddSubject(SemsSubjectScore subjectScore)
        {
            if (ContainsKey(subjectScore.ScoreName))
                return false;
            else
            {
                Add(subjectScore.ScoreName, subjectScore);
                return true;
            }
        }
    }
}
