using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Reports.MultiSemesterScore.DataModel
{
    public class EntryInfo : ScoreInfo
    {
        public EntryInfo(string name)
            : base(name)
        {
        }
    }

    public class EntryCollection : Dictionary<string, EntryInfo>
    {
    }
}
