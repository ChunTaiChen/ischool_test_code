using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 進行排名的參數。
    /// </summary>
    class RatingParameters
    {
        private RatingItems _rating_items;
        private RatingMethod _rating_method;
        private List<string> _target_grades;
        private string _school_year;
        private string _semester;

        public string SchoolYear
        {
            get { return _school_year; }
            set { _school_year = value; }
        }

        public string Semester
        {
            get { return _semester; }
            set { _semester = value; }
        }

        public RatingParameters()
        {
            _target_grades = new List<string>();
        }

        public RatingItems RatingItems
        {
            get { return _rating_items; }
            set { _rating_items = value; }
        }

        public RatingMethod RatingMethod
        {
            get { return _rating_method; }
            set { _rating_method = value; }
        }

        public List<string> TargetGradeYears
        {
            get { return _target_grades; }
        }
    }
}
