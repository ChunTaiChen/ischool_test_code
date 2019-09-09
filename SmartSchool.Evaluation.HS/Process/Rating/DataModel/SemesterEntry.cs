using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SmartSchool.Evaluation.Process.Rating
{
    class SemesterEntry : EntryScore
    {
        public SemesterEntry(string scoreName, XmlElement data)
            : base(scoreName, data)
        {
        }

        protected override string GetPathString()
        {
            return "ScoreInfo/SemesterEntryScore/Entry[@分項='{0}']";
        }
    }
}
