using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Customization.Data;

namespace SmartSchool.Evaluation.Process.Wizards
{
    public class LogUtility
    {
        public static void WriteLog(SelectType type, List<StudentRecord> selectedStudents, string year, string cate)
        {
            string desc = "";
            switch (type)
            {
                case SelectType.Student:
                    foreach (StudentRecord each_stu in selectedStudents)
                    {
                        desc = "計算「" + each_stu.StudentName + "(" + (string.IsNullOrEmpty(each_stu.StudentNumber) ? "無學號" : each_stu.StudentNumber) + ")」" + year + "學年" + cate + "成績";
                        CurrentUser.Instance.AppLog.Write(SmartSchool.ApplicationLog.EntityType.Student, "計算學年" + cate + "成績", each_stu.StudentID, desc, "成績計算", "");
                    }
                    break;
                case SelectType.GradeYearStudent:
                    desc = "計算「" + year + "年級」" + SmartSchool.Customization.Data.SystemInformation.SchoolYear + "學年" + cate + "成績";
                    CurrentUser.Instance.AppLog.Write("計算學年" + cate + "成績", desc, "成績計算", "");
                    break;
            }
        }

        public static void WriteLog(SelectType type, List<StudentRecord> selectedStudents, string year, string semester, string cate)
        {
            string desc = "";
            switch (type)
            {
                case SelectType.Student:
                    foreach (StudentRecord each_stu in selectedStudents)
                    {
                        desc = "計算「" + each_stu.StudentName + "(" + (string.IsNullOrEmpty(each_stu.StudentNumber) ? "無學號" : each_stu.StudentNumber) + ")」" + year + "學年第" + semester + "學期" + cate + "成績";
                        CurrentUser.Instance.AppLog.Write(SmartSchool.ApplicationLog.EntityType.Student, "計算學期" + cate + "成績", each_stu.StudentID, desc, "成績計算", "");
                    }
                    break;
                case SelectType.GradeYearStudent:
                    desc = "計算「" + year + "年級」" + SmartSchool.Customization.Data.SystemInformation.SchoolYear + "學年第" + SmartSchool.Customization.Data.SystemInformation.Semester + "學期" + cate + "成績";
                    CurrentUser.Instance.AppLog.Write("計算學期" + cate + "成績", desc, "成績計算", "");
                    break;
            }
        }
    }
}
