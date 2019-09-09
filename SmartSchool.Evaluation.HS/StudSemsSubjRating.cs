using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartSchool.Evaluation
{
    public class StudSemsSubjRating
    {
        public string StudentID { get; set; }
        public string SchoolYear { get; set; }
        public string Semester { get; set; }

        public string GradeYear { get; set; }

        // 班成績人數
        private Dictionary<string, int> _ClassCountDict = new Dictionary<string, int>();

        // 班排名
        private Dictionary<string, int> _ClassRankDict = new Dictionary<string, int>();

        // 科成績人數
        private Dictionary<string, int> _DeptCountDict = new Dictionary<string, int>();

        // 科排名
        private Dictionary<string, int> _DeptRankDict = new Dictionary<string, int>();

        // 年成績人數
        private Dictionary<string, int> _YearCountDict = new Dictionary<string, int>();

        // 年排名
        private Dictionary<string, int> _YearRankDict = new Dictionary<string, int>();

        /// <summary>
        /// 類1名稱
        /// </summary>
        public string Group1 { get; set; }

        // 類1成績人數
        private Dictionary<string, int> _Group1CountDict = new Dictionary<string, int>();

        // 類1排名
        private Dictionary<string, int> _Group1RankDict = new Dictionary<string, int>();


        public void AddClassCount(string name,int count)
        {
            if (!_ClassCountDict.ContainsKey(name))
                _ClassCountDict.Add(name, count);
        }

        public void AddClassRank(string name, int rank)
        {
            if (!_ClassRankDict.ContainsKey(name))
                _ClassRankDict.Add(name, rank);
        }

        public void AddDeptCount(string name, int count)
        {
            if (!_DeptCountDict.ContainsKey(name))
                _DeptCountDict.Add(name, count);
        }

        public void AddDeptRank(string name, int rank)
        {
            if (!_DeptRankDict.ContainsKey(name))
                _DeptRankDict.Add(name, rank);
        }

        public void AddYearCount(string name, int count)
        {
            if (!_YearCountDict.ContainsKey(name))
                _YearCountDict.Add(name, count);
        }

        public void AddYearRank(string name, int rank)
        {
            if (!_YearRankDict.ContainsKey(name))
                _YearRankDict.Add(name, rank);
        }

        public void AddGroup1Count(string name, int count)
        {
            if (!_Group1CountDict.ContainsKey(name))
                _Group1CountDict.Add(name, count);
        }

        public void AddGroup1Rank(string name, int rank)
        {
            if (!_Group1RankDict.ContainsKey(name))
                _Group1RankDict.Add(name, rank);
        }

        /// <summary>
        /// 取得班成績人數
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetClassCount(string name)
        { 
            string value="";
            if (_ClassCountDict.ContainsKey(name))
                value = _ClassCountDict[name].ToString();

            return value;
        }

        /// <summary>
        /// 取得班名次
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetClassRank(string name)
        {
            string value = "";
            if (_ClassRankDict.ContainsKey(name))
                value = _ClassRankDict[name].ToString();

            return value;
        }

        /// <summary>
        /// 取得班排名百分比
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetClassRankP(string name)
        {
            string value = "";
            if (_ClassRankDict.ContainsKey(name) && _ClassCountDict.ContainsKey(name))            
                value = Utility.ParseRankPercent(_ClassRankDict[name], _ClassCountDict[name]).ToString();            
            
            return value;
        }

        /// <summary>
        /// 取得科成績人數
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetDeptCount(string name)
        {
            string value = "";
            if (_DeptCountDict.ContainsKey(name))
                value = _DeptCountDict[name].ToString();

            return value;
        }

        /// <summary>
        /// 取得科名次
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetDeptRank(string name)
        {
            string value = "";
            if (_DeptRankDict.ContainsKey(name))
                value = _DeptRankDict[name].ToString();

            return value;
        }

        /// <summary>
        /// 取得科排名百分比
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetDeptRankP(string name)
        {
            string value = "";
            if (_DeptRankDict.ContainsKey(name) && _DeptCountDict.ContainsKey(name))
                value = Utility.ParseRankPercent(_DeptRankDict[name], _DeptCountDict[name]).ToString();

            return value;
        }

        /// <summary>
        /// 取得校成績人數
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetYearCount(string name)
        {
            string value = "";
            if (_YearCountDict.ContainsKey(name))
                value = _YearCountDict[name].ToString();

            return value;
        }

        /// <summary>
        /// 取得校名次
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetYearRank(string name)
        {
            string value = "";
            if (_YearRankDict.ContainsKey(name))
                value = _YearRankDict[name].ToString();

            return value;
        }

        /// <summary>
        /// 取得校排名百分比
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetYearRankP(string name)
        {
            string value = "";
            if (_YearRankDict.ContainsKey(name) && _YearCountDict.ContainsKey(name))
                value = Utility.ParseRankPercent(_YearRankDict[name], _YearCountDict[name]).ToString();

            return value;
        }

        /// <summary>
        /// 取得類1成績人數
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetGroup1Count(string name)
        {
            string value = "";
            if (_Group1CountDict.ContainsKey(name))
                value = _Group1CountDict[name].ToString();

            return value;
        }

        /// <summary>
        /// 取得類1名次
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetGroup1Rank(string name)
        {
            string value = "";
            if (_Group1RankDict.ContainsKey(name))
                value = _Group1RankDict[name].ToString();

            return value;
        }

        /// <summary>
        /// 取得校類1名百分比
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetGroup1RankP(string name)
        {
            string value = "";
            if (_Group1RankDict.ContainsKey(name) && _Group1CountDict.ContainsKey(name))
                value = Utility.ParseRankPercent(_Group1RankDict[name], _Group1CountDict[name]).ToString();

            return value;
        }
    }
}
