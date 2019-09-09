using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表學生的學年科目成績。
    /// </summary>
    class YearSubjectScore : RatableScore
    {
        public YearSubjectScore(XmlElement data)
            : base()
        {
            DSXmlHelper hlpData = new DSXmlHelper(data);

            ScoreName = hlpData.GetText("@科目");
            Score = Utility.ParseDecimal(hlpData.GetText("@學年成績"), int.MinValue);
            _subject_name=ScoreName;
        }

        private string _subject_name;
        public string SubjectName
        {
            get { return _subject_name; }
        }

    }

    class YearSubjectScoreCollection : Dictionary<string, YearSubjectScore>
    {
        private string _score_identity;

        public string ScoreRecordIdentity
        {
            get { return _score_identity; }
            set { _score_identity = value; }
        }

        public bool AddSubject(YearSubjectScore subjectScore)
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
