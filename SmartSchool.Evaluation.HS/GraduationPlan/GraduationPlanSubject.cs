using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SmartSchool.Evaluation.GraduationPlan
{
    public class GraduationPlanSubject
    {
        readonly private string _Category;
        readonly private string _Credit;
        readonly private string _Domain;
        readonly private string _FullName;
        //readonly private string _GradeYear;
        readonly private string _Level;
        readonly private string _Required;
        readonly private string _RequiredBy;
        //readonly private string _Semester;
        readonly private string _SubjectName;
        readonly private string _Entry;
        readonly private bool _NotIncludedInCredit;
        readonly private bool _NotIncludedInCalc;

        internal GraduationPlanSubject(XmlElement subjectElement)
        {
            _Category = subjectElement.GetAttribute("Category");
            _Credit = subjectElement.GetAttribute("Credit");
            _Domain = subjectElement.GetAttribute("Domain");
            _FullName = subjectElement.GetAttribute("FullName");
            //_GradeYear = subjectElement.GetAttribute("GradeYear");
            _Level = subjectElement.GetAttribute("Level");
            _Required = subjectElement.GetAttribute("Required");
            _RequiredBy = subjectElement.GetAttribute("RequiredBy");
            //_Semester = subjectElement.GetAttribute("Semester");
            _SubjectName = subjectElement.GetAttribute("SubjectName");
            _Entry = subjectElement.GetAttribute("Entry");
            bool b = false;
            bool.TryParse(subjectElement.GetAttribute("NotIncludedInCredit"),out b);
            _NotIncludedInCredit = b; 
            b = false;
            bool.TryParse(subjectElement.GetAttribute("NotIncludedInCalc"), out b);
            _NotIncludedInCalc = b;
        }

        public string Category { get { return _Category; } }
        public string Credit { get { return _Credit; } }
        public string Domain { get { return _Domain; } }
        public string FullName { get { return _FullName; } }
        //public string GradeYear { get { return _GradeYear; } }
        public string Level { get { return _Level; } }
        public string Required { get { return _Required; } }
        public string RequiredBy { get { return _RequiredBy; } }
        //public string Semester { get { return _Semester; } }
        public string SubjectName { get { return _SubjectName; } }
        public string Entry { get { return _Entry; } }
        public bool NotIncludedInCredit { get { return _NotIncludedInCredit; } }
        public bool NotIncludedInCalc { get { return _NotIncludedInCalc; } }
    }
}
