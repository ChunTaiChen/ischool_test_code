using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Reports.MultiSemesterScore.DataModel
{
    public class ScoreInfo : IScoreInfo
    {
        protected string _name;
        public string Name
        {
            get { return _name; }
        }

        protected SemsScores _sems_scores;
        public SemsScores SemsScores
        {
            get { return _sems_scores; }
        }

        public ScoreInfo(string name)
        {
            _name = name;
            _sems_scores = new SemsScores();
        }

        #region IScoreInfo 成員
        private int _place;
        public int Place
        {
            get { return _place; }
            set { _place = value; }
        }

        private int _radix;
        public int Radix
        {
            get { return _radix; }
            set { _radix = value; }
        }

        public void AddSemsScore(int grade_year, int semester, decimal score)
        {
            _sems_scores.AddScore(grade_year, semester, score);
        }

        public decimal GetAverange()
        {
            if (_sems_scores.Count <= 0) return 0;

            decimal sum = 0;
            foreach (decimal each_sems_score in _sems_scores.Values)
                sum += each_sems_score;
            return Math.Round(sum / _sems_scores.Count, 2, MidpointRounding.AwayFromZero);
        }

        public decimal GetPercentage()
        {
            //第一名一定是 1。
            if (Place == 1) return 1;

            //名次等於人數時，一定是 100。
            if (Place == Radix)
                return 100;

            decimal score;
            if (_radix == 0)
                score = (decimal)0;
            else
                score = ((decimal)_place * 100) / _radix;
            return Math.Round(score, 1);
        }

        public void FixToLimit(decimal limit)
        {
            List<int> fixSem = new List<int>();
            foreach ( int sem in _sems_scores.Keys )
            {
                if ( _sems_scores[sem] > limit )
                    fixSem.Add(sem);
            }
            foreach ( int sem in fixSem )
            {
                _sems_scores[sem] = limit;
            }
        }
        #endregion
    }

    public class SemsScores : Dictionary<int, decimal>
    {
        public void AddScore(int grade_year, int semester, decimal score)
        {
            int index = GetIndex(grade_year, semester);
            if (!ContainsKey(index))
                Add(index, score);
        }

        public decimal GetScore(int grade_year, int semester)
        {
            int index = GetIndex(grade_year, semester);
            if (ContainsKey(index))
                return this[index];
            return -1;
        }

        private int GetIndex(int grade_year, int semester)
        {
            return (grade_year - 1) * 2 + semester;
        }
    }
}
