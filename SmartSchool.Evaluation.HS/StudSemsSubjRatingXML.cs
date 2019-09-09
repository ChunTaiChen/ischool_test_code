using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace SmartSchool.Evaluation
{
    public class StudSemsSubjRatingXML
    {
        public string StudentID { get; set; }
        public string SchoolYear { get; set; }
        public string Semester { get; set; }

        public string GradeYear { get; set; }

        public XElement ClassRankXML { get; set; }
        public XElement DeptRankXML { get; set; }
        public XElement YearRankXML { get; set; }

        public XElement GroupRankXML { get; set; }

    }
}
