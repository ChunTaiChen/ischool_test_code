using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表一個學生，包含基本資料與一個學期(或學年)的所有成績資料。
    /// 如果是學期成績則是目前學期的資料，如果是學年成績則是目前學年的資料。
    /// 學期成績不會同時包含二個學期的資料。
    /// </summary>
    class Student
    {
        private string _identity, _name, _class_name, _ref_class_id, _seat_no, _ref_dept_id, _grade_year;
        private string _dept_name;

        private SemsSubjectScoreCollection _sems_subject_scores;
        private YearSubjectScoreCollection _year_subject_scores;
        private EntryScore _sems_score;
        private EntryScore _year_score;
        private EntryScore _sems_moral;
        private EntryScore _year_moral;

        public Student(XmlElement data)
        {
            DSXmlHelper hlpData = new DSXmlHelper(data);

            _identity = hlpData.GetText("@ID");
            _name = hlpData.GetText("Name");
            _ref_class_id = hlpData.GetText("RefClassID");
            _seat_no = hlpData.GetText("SeatNo");
            _ref_dept_id = hlpData.GetText("RefDepartmentID");
            _grade_year = hlpData.GetText("GradeYear");

            _class_name = hlpData.GetText("ClassName");
            _dept_name = hlpData.GetText("DepartmentName");

            if (_class_name == string.Empty)
                _class_name = "<未分班>";

            if (_dept_name == string.Empty)
                _dept_name = "<未分科>";

            _sems_subject_scores = new SemsSubjectScoreCollection();
            _year_subject_scores = new YearSubjectScoreCollection();
        }

        public string Identity
        {
            get { return _identity; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string RefClassID
        {
            get { return _ref_class_id; }
        }

        public string ClassName
        {
            get { return _class_name; }
        }

        public string SeatNumber
        {
            get { return _seat_no; }
        }

        public string RefDeptID
        {
            get { return _ref_dept_id; }
        }

        public string DeptName
        {
            get { return _dept_name; }
        }

        public string GradeYear
        {
            get { return _grade_year; }
        }

        /// <summary>
        /// 學期科目成績。
        /// </summary>
        public SemsSubjectScoreCollection SemsSubjects
        {
            get { return _sems_subject_scores; }
        }

        /// <summary>
        /// 學年科目成績。
        /// </summary>
        public YearSubjectScoreCollection YearSubjects
        {
            get { return _year_subject_scores; }
        }

        /// <summary>
        /// 學期學業成績。
        /// </summary>
        public EntryScore SemsScore
        {
            get { return _sems_score; }
            set { _sems_score = value; }
        }

        /// <summary>
        /// 學年學業成績。
        /// </summary>
        public EntryScore YearScore
        {
            get { return _year_score; }
            set { _year_score = value; }
        }

        /// <summary>
        /// 學期德行成績。
        /// </summary>
        public EntryScore SemsMoral
        {
            get { return _sems_moral; }
            set { _sems_moral = value; }
        }

        /// <summary>
        /// 學年德行成績。
        /// </summary>
        public EntryScore YearMoral
        {
            get { return _year_moral; }
            set { _year_moral = value; }
        }
    }

    class StudentCollection : Dictionary<string, Student>
    {
        public void AddStudent(Student student)
        {
            Add(student.Identity, student);
        }
    }
}
