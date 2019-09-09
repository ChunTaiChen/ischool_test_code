using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    class SemsSubjectTarget : IRatingTarget
    {
        private string _subject_name;

        public SemsSubjectTarget(string subjectName)
        {
            _subject_name = subjectName;
        }

        #region IRatingTarget 成員

        public string Name
        {
            get { return string.Format("學期科目成績({0})", _subject_name); }
        }

        public bool ContainsScore(Student student)
        {
            return student.SemsSubjects.ContainsKey(_subject_name);
        }

        public decimal GetScore(Student student)
        {
            if (ContainsScore(student))
                return student.SemsSubjects[_subject_name].Score;
            else
                return decimal.MinValue;
        }

        public bool SetPlace(Student student, ResultPlace place)
        {
            if (ContainsScore(student))
            {
                ResultPlaceCollection places = student.SemsSubjects[_subject_name].RatingResults;

                if (places.ContainsKey(place.Scope.ScopeType))
                    places[place.Scope.ScopeType] = place;
                else
                    places.AddPlace(place);

                return true;
            }
            else
                return false;
        }

        public ResultPlace GetPlace(Student student, ScopeType type)
        {
            SemsSubjectScore score;

            if (student.SemsSubjects.TryGetValue(_subject_name, out score))
            {
                ResultPlace place;
                if (score.RatingResults.TryGetValue(type, out place))
                    return place;
                else
                    return null;
            }
            else
                return null;
        }

        #endregion

        public static List<IRatingTarget> GroupBy(RatingScope scope)
        {
            Dictionary<string, IRatingTarget> targets = new Dictionary<string, IRatingTarget>();

            foreach (Student eachStudent in scope)
            {
                foreach (SemsSubjectScore eachScore in eachStudent.SemsSubjects.Values)
                {
                    if (!targets.ContainsKey(eachScore.ScoreName))
                        targets.Add(eachScore.ScoreName, new SemsSubjectTarget(eachScore.ScoreName));
                }
            }
            return new List<IRatingTarget>(targets.Values);
        }
    }
}
