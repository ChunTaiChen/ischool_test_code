using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.Data;
using SmartSchool.Customization.Data.StudentExtension;

namespace SmartSchool.Evaluation.Reports.MultiSemesterScore.DataModel
{
    public class Student
    {
        private StudentRecord _student;
        private ScoreType _score_type;

        private string _dept;
        private string _class;
        private string _seat_no;
        private string _student_number;
        private string _name;

        public string Department { get { return _dept; } }
        public string ClassName { get { return _class; } }
        public string SeatNo { get { return _seat_no; } }
        public string StudentNumber { get { return _student_number; } }
        public string StudentName { get { return _name; } }

        private SubjectCollection _subjects;
        private EntryCollection _entries;

        public SubjectCollection SubjectCollection { get { return _subjects; } }
        public EntryCollection EntryCollection { get { return _entries; } }



        public Student(StudentRecord income_stu, ScoreType score_type,int semesters,List<string> printEntries)
        {
            _dept = income_stu.Department;
            _class = income_stu.RefClass == null ? "" : income_stu.RefClass.ClassName;
            _seat_no = income_stu.SeatNo;
            _student_number = income_stu.StudentNumber;
            _name = income_stu.StudentName;

            _student = income_stu;
            _score_type = score_type;

            _subjects = new SubjectCollection();
            _entries = new EntryCollection();
            ProcessScoreList(semesters, printEntries);
        }

        private void ProcessScoreList(int semesters, List<string> printEntries)
        {
            //統計有成績的學期
            if ( semesters <= 0 ) semesters = int.MaxValue;
            List<int> semesterlist = new List<int>();
            foreach ( SemesterSubjectScoreInfo info in _student.SemesterSubjectScoreList )
            {
                if ( info.Detail.GetAttribute("不需評分") == "是" )
                    continue;
                if ( !semesterlist.Contains(( info.GradeYear - 1 ) * 2 + info.Semester) )
                    semesterlist.Add(( info.GradeYear - 1 ) * 2 + info.Semester);
            }

            foreach ( SemesterEntryScoreInfo info in _student.SemesterEntryScoreList )
            {
                if ( !semesterlist.Contains(( info.GradeYear - 1 ) * 2 + info.Semester) )
                    semesterlist.Add(( info.GradeYear - 1 ) * 2 + info.Semester);
            }
            semesterlist.Sort();

            foreach (SemesterSubjectScoreInfo info in _student.SemesterSubjectScoreList)
            {
                if ( info.Detail.GetAttribute("不需評分") == "是" )
                    continue;
                //超過統計學期當沒看到
                if ( semesterlist.Count > semesters && ( info.GradeYear - 1 ) * 2 + info.Semester > semesterlist[semesters - 1] )
                    continue;
                if (!_subjects.ContainsKey(info.Subject))
                {
                    SubjectInfo new_info = new SubjectInfo(info.Subject);
                    _subjects.Add(info.Subject, new_info);
                    new_info.AddSemsScore(info.GradeYear, info.Semester, SelectScore(info));
                }
                else
                {
                    _subjects[info.Subject].AddSemsScore(info.GradeYear, info.Semester, SelectScore(info));
                }
            }

            foreach (SemesterEntryScoreInfo info in _student.SemesterEntryScoreList)
            {
                //超過統計學期當沒看到
                if ( semesterlist.Count > semesters && ( info.GradeYear - 1 ) * 2 + info.Semester > semesterlist[semesters - 1] )
                    continue;
                //不是要列印的分項當沒看到
                if ( !printEntries.Contains(info.Entry) )
                    continue;
                if (!_entries.ContainsKey(info.Entry))
                {
                    EntryInfo new_info = new EntryInfo(info.Entry);
                    _entries.Add(info.Entry, new_info);
                    new_info.AddSemsScore(info.GradeYear, info.Semester, info.Score);
                }
                else
                {
                    _entries[info.Entry].AddSemsScore(info.GradeYear, info.Semester, info.Score);
                }
            }
        }

        private decimal SelectScore(SemesterSubjectScoreInfo info)
        {
            switch (_score_type)
            {
                case ScoreType.原始成績:
                    {
                        decimal tryValue;
                        if (info.Detail.HasAttribute("原始成績"))
                        {
                            if (decimal.TryParse(info.Detail.GetAttribute("原始成績"), out tryValue))
                                return tryValue;
                        }
                        return 0;
                    }

                case ScoreType.原始補考擇優:
                    {
                        bool hasValue = false;
                        decimal value = decimal.MinValue;
                        decimal tryValue;
                        if ( info.Detail.HasAttribute("原始成績") )
                        {
                            if ( decimal.TryParse(info.Detail.GetAttribute("原始成績"), out tryValue) )
                            {
                                value = tryValue;
                                hasValue = true;
                            }
                        }
                        if ( info.Detail.HasAttribute("補考成績") )
                        {
                            if ( decimal.TryParse(info.Detail.GetAttribute("補考成績"), out tryValue) )
                            {
                                value = tryValue > value ? tryValue : value;
                                hasValue = true;
                            }
                        }
                        return hasValue ? value : 0;
                    }
                case ScoreType.擇優成績:
                    return info.Score;
                default:
                    return 0;
            }
        }
    }

    public class StudentCollection : Dictionary<string, Student>
    {
    }
}
