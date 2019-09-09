using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Reports.MultiSemesterScore.DataModel
{
    public class SubjectInfo : ScoreInfo
    {
        public SubjectInfo(string name)
            : base(name)
        {
        }
    }

    public class SubjectCollection : Dictionary<string, SubjectInfo>
    {
    }
}
