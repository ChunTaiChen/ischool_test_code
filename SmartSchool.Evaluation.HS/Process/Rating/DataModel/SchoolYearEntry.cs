using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SmartSchool.Evaluation.Process.Rating
{
    class SchoolYearEntry : EntryScore
    {
        public SchoolYearEntry(string scoreName, XmlElement data)
            : base(scoreName, data)
        {
        }

        protected override string GetPathString()
        {
            return "ScoreInfo/SchoolYearEntryScore/Entry[@分項='{0}']";
        }
    }
}
