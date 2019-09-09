using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Common;

namespace SmartSchool.Evaluation.Process.Rating
{
    static class Utility
    {
        public static decimal ParseDecimal(string value, decimal defaultValue)
        {
            decimal result;

            if (decimal.TryParse(value, out result))
                return result;
            else
                return defaultValue;
        }

        public static List<List<string>> SplitGetPackage(StudentCollection students, int packageSize)
        {
            List<List<string>> packages = new List<List<string>>();
            List<string> package = null;
            int offset = 0;
            foreach (Student each in students.Values)
            {
                if ((offset % packageSize) == 0)
                {
                    package = new List<string>();
                    packages.Add(package);
                }

                package.Add(each.Identity);
                offset++;
            }

            return packages;
        }

        public static List<List<Student>> SplitUpdatePackage(StudentCollection students, int packageSize)
        {
            List<List<Student>> packages = new List<List<Student>>();

            List<Student> package = null;
            int offset = 0;
            foreach (Student each in students.Values)
            {
                if ((offset % packageSize) == 0)
                {
                    package = new List<Student>();
                    packages.Add(package);
                }

                package.Add(each);
                offset++;
            }

            return packages;
        }

        private static Dictionary<string, int> _times = new Dictionary<string, int>();

        public static void StartTime(string name)
        {
            if (_times.ContainsKey(name))
                _times[name] = Environment.TickCount;
            else
                _times.Add(name, Environment.TickCount);
        }

        public static void EndTime(string name)
        {
            if (_times.ContainsKey(name))
            {
                int result = Environment.TickCount - _times[name];
                Console.WriteLine(string.Format("使用時間{0}：{1}", name, result));
            }
            else
                Console.WriteLine(string.Format("不存在：{0}", name));
        }

        public static void LogRank(string action, RatingParameters parameters, bool includeSemester)
        {
            try
            {
                StringBuilder builder = new StringBuilder();

                if (includeSemester)
                    builder.AppendLine(string.Format("學年度：{0}，學期：{1}", parameters.SchoolYear, parameters.Semester));
                else
                    builder.AppendLine(string.Format("學年度：{0}", parameters.SchoolYear));

                builder.AppendLine(string.Format("排名選項：{0}", GetRatingOption(parameters.RatingMethod)));
                builder.AppendLine(string.Format("排名年級：{0}", GetRatingDataCondition(parameters.TargetGradeYears)));
                builder.AppendLine(string.Format("排名項目：{0}", GetRatingItems(parameters.RatingItems)));

                CurrentUser.Instance.AppLog.Write(action, builder.ToString(), action, string.Empty);
            }
            catch (Exception ex)
            {
                CurrentUser.ReportError(ex);
            }
        }

        private static string GetRatingOption(RatingMethod ratingMethod)
        {
            if (ratingMethod == RatingMethod.Sequence)
                return "接序排名";
            else if (ratingMethod == RatingMethod.Unsequence)
                return "不接序排名";
            else
                throw new ArgumentException("沒有此種排名選項。");
        }

        private static string GetRatingDataCondition(List<string> list)
        {
            return string.Join(",", list.ToArray());
        }

        private static string GetRatingItems(RatingItems ratingItems)
        {
            List<string> list = new List<string>();

            if ((ratingItems & RatingItems.SemsSubject) == RatingItems.SemsSubject)
                list.Add("學期科目成績");

            if ((ratingItems & RatingItems.SemsScore) == RatingItems.SemsScore)
                list.Add("學期學業成績");

            if ((ratingItems & RatingItems.SemsMoral) == RatingItems.SemsMoral)
                list.Add("學期德行成績");

            if ((ratingItems & RatingItems.YearSubject) == RatingItems.YearSubject)
                list.Add("學年科目成績");

            if ((ratingItems & RatingItems.YearScore) == RatingItems.YearScore)
                list.Add("學年學業成績");

            if ((ratingItems & RatingItems.YearMoral) == RatingItems.YearMoral)
                list.Add("學年德行成績");

            return string.Join(",", list.ToArray());
        }
    }
}
