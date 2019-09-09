using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    class ResultPlace
    {
        /// <summary>
        /// 排名結果。
        /// </summary>
        /// <param name="ratingBase">排名的總人數。</param>
        /// <param name="actualBase">實際有成績，而排名的人數。</param>
        /// <param name="place">名次。</param>
        public ResultPlace(RatingScope scope, IRatingTarget target, int ratingBase, int actualBase, int place)
        {
            _scope = scope;
            _target = target;
            _rating_base = ratingBase;
            _actual_base = actualBase;
            _place = place;
        }

        private RatingScope _scope;
        public RatingScope Scope
        {
            get { return _scope; }
        }

        private IRatingTarget _target;
        public IRatingTarget RatingTarget
        {
            get { return _target; }
        }

        private int _rating_base;
        public int RatingBase
        {
            get { return _rating_base; }
        }

        private int _actual_base;
        public int ActualBase
        {
            get { return _actual_base; }
        }

        private int _place;
        public int Place
        {
            get { return _place; }
        }
    }

    class ResultPlaceCollection : Dictionary<ScopeType, ResultPlace>
    {
        public void AddPlace(ResultPlace place)
        {
            Add(place.Scope.ScopeType, place);
        }
    }
}
