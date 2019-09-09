using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    interface IRatingTarget
    {
        string Name { get;}

        bool ContainsScore(Student student);

        decimal GetScore(Student student);

        bool SetPlace(Student student, ResultPlace place);

        ResultPlace GetPlace(Student student, ScopeType type);
    }

    class RatingTargetCollection : List<IRatingTarget>
    {
    }
}
