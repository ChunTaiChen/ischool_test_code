using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表一個學生的某個成績。
    /// 例：國文 I 成績 、學期學業成績 ...
    /// </summary>
    class RatableScore
    {
        private string _score_name;
        private decimal _score;

        protected RatableScore()
        {
            _rating_results = new ResultPlaceCollection();
            _score_name = string.Empty;
            _score = int.MinValue;
        }

        /// <summary>
        /// 成績名稱，例：國文 I、數學 I、學業成績 ...。
        /// 可能是科目名稱或是分項名稱。
        /// </summary>
        public virtual string ScoreName
        {
            get { return _score_name; }
            protected set { _score_name = value; }
        }

        public virtual decimal Score
        {
            get { return _score; }
            protected set { _score = value; }
        }

        private ResultPlaceCollection _rating_results;
        /// <summary>
        /// 排名結果集合。
        /// </summary>
        public ResultPlaceCollection RatingResults
        {
            get { return _rating_results; }
        }
    }
}
