using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    static class RatingUtility
    {
        public static void Rank(RatingScope scope, IRatingTarget targetScore, RatingMethod method)
        {
            //int t1 = Environment.TickCount;

            List<Student> students = scope.GetContainTargetList(targetScore);

            //按成績高低排序。
            students.Sort(new ScoreComparer(targetScore));

            //實際有包含成績的人數。
            int actualBase = students.Count;

            //所有被排名的人數。
            int baseCount = scope.Count;

            //名次決定演算法。
            IRatingAlgorithm placeDecision = GetAlgorithm(method);

            foreach (Student eachStudent in students)
            {
                //決定名次
                int place = placeDecision.NextPlace(targetScore.GetScore(eachStudent));

                //寫入名次資訊。
                targetScore.SetPlace(eachStudent, new ResultPlace(scope, targetScore, baseCount, actualBase, place));
            }

            //string msg = "排名時間：{0} 範圍名稱：{1} 成績名稱：{2}";
            //Console.WriteLine(msg, Environment.TickCount - t1, scope.Name, targetScore.Name);
        }

        private static IRatingAlgorithm GetAlgorithm(RatingMethod method)
        {
            IRatingAlgorithm placeDecision;
            if (method == RatingMethod.Sequence)
                placeDecision = new SequenceAlgorithm();
            else
                placeDecision = new UnSequenceAlgorithm();

            return placeDecision;
        }

        #region ScoreComparer

        class ScoreComparer : IComparer<Student>
        {
            private IRatingTarget _target;

            public ScoreComparer(IRatingTarget target)
            {
                _target = target;
            }

            #region IComparer<Student> 成員

            public int Compare(Student x, Student y)
            {
                decimal scoreX = _target.GetScore(x);
                decimal scoreY = _target.GetScore(y);

                //反過來排，由大到小。
                return scoreY.CompareTo(scoreX);
            }

            #endregion
        }

        #endregion

        #region RatingAlgorithm

        interface IRatingAlgorithm
        {
            int NextPlace(decimal score);
        }

        class UnSequenceAlgorithm : IRatingAlgorithm
        {
            decimal _previous_score;
            int _previous_place;
            int _sequence;

            public UnSequenceAlgorithm()
            {
                _previous_score = int.MaxValue;
                _previous_place = 0;
                _sequence = 0;
            }

            public int NextPlace(decimal score)
            {
                bool nextRequired = IsNextRequired(score);
                int place;

                if (nextRequired)
                {
                    place = ++_sequence;
                    _previous_place = _sequence;
                }
                else
                {
                    place = _previous_place;
                    ++_sequence;
                }

                _previous_score = score;
                return place;
            }

            private bool IsNextRequired(decimal score)
            {
                //小於 0 score < _previous_score
                return score.CompareTo(_previous_score) < 0;
            }
        }

        class SequenceAlgorithm : IRatingAlgorithm
        {
            decimal _previous_score;
            int _previous_place;

            public SequenceAlgorithm()
            {
                _previous_score = int.MaxValue;
                _previous_place = 0;
            }

            public int NextPlace(decimal score)
            {
                bool nextRequired = IsNextRequired(score);
                int place;

                if (nextRequired)
                    place = ++_previous_place;
                else
                    place = _previous_place;

                _previous_score = score;
                return place;
            }

            private bool IsNextRequired(decimal score)
            {
                //小於 0 score < _previous_score
                return score.CompareTo(_previous_score) < 0;
            }
        }

        #endregion
    }
}
