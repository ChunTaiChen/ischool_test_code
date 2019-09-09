using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    class YearMoralTarget : IRatingTarget
    {
        #region IRatingTarget 成員

        public string Name
        {
            get { return "學年德行成績"; }
        }

        public bool ContainsScore(Student student)
        {
            return student.YearMoral != null;
        }

        public decimal GetScore(Student student)
        {
            if (ContainsScore(student))
                return student.YearMoral.Score;
            else
                return decimal.MinValue;
        }

        public bool SetPlace(Student student, ResultPlace place)
        {
            if (ContainsScore(student))
            {
                ResultPlaceCollection places = student.YearMoral.RatingResults;

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
            if (!ContainsScore(student)) return null;

            EntryScore score = student.YearMoral;

            ResultPlace place;
            if (score.RatingResults.TryGetValue(type, out place))
                return place;
            else
                return null;
        }

        #endregion
    }

}
