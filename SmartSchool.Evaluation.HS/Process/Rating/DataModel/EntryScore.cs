using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表學業、德行的學年與學期成績。
    /// </summary>
    class EntryScore : RatableScore
    {
        public EntryScore(string scoreName, XmlElement data)
            : base()
        {
            DSXmlHelper hlpData = new DSXmlHelper(data);

            _score_record_identity = hlpData.GetText("@ID");
            ScoreName = scoreName;

            string path = string.Format(GetPathString(), ScoreName);
            if (hlpData.PathExist(path))
            {
                DSXmlHelper entry = new DSXmlHelper(hlpData.GetElement(path));
                Score = Utility.ParseDecimal(entry.GetText("@成績"), decimal.MinValue);
            }
            else
                Score = decimal.MinValue;
        }

        protected virtual string GetPathString()
        {
            throw new NotImplementedException("未貫作重要方法。");
        }

        private string _score_record_identity;
        public string ScoreRecordIdentity
        {
            get { return _score_record_identity; }
        }

    }
}
