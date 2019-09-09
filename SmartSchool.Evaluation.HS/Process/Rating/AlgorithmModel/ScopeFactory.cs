using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    static class ScopeFactory
    {
        public static RatingScopeCollection CreateScopes(StudentCollection students, ScopeType scopeType)
        {
            Dictionary<string, StudentCollection> stuCollList = new Dictionary<string, StudentCollection>();
            foreach (Student eachStudent in students.Values)
            {
                string key = GetKey(eachStudent, scopeType);
                if (stuCollList.ContainsKey(key))
                    stuCollList[key].AddStudent(eachStudent);
                else
                {
                    StudentCollection stus = new StudentCollection();
                    stuCollList.Add(key, stus);
                    stus.AddStudent(eachStudent);
                }
            }

            RatingScopeCollection scopes = ConvertToScopes(stuCollList, scopeType);

            return scopes;
        }

        private static string GetKey(Student eachStudent, ScopeType scopeType)
        {
            string key;

            if (scopeType == ScopeType.Class)
                key = eachStudent.RefClassID;
            else if (scopeType == ScopeType.Dept)
                key = eachStudent.RefDeptID+"_"+eachStudent.GradeYear;
            else if (scopeType == ScopeType.GradeYear)
                key = eachStudent.GradeYear;
            else
                throw new ArgumentException("不支援此類型的排名範圍。(" + scopeType.ToString() + ")");

            return key;
        }

        private static RatingScopeCollection ConvertToScopes(Dictionary<string, StudentCollection> stuCollList, ScopeType type)
        {
            RatingScopeCollection scopes = new RatingScopeCollection();
            foreach (StudentCollection eachStus in stuCollList.Values)
            {
                string name = string.Empty;

                foreach (Student eachStu in eachStus.Values)
                {
                    if (type == ScopeType.Class)
                        name = eachStu.ClassName;
                    else if (type == ScopeType.Dept)
                        name = eachStu.DeptName;
                    else if (type == ScopeType.GradeYear)
                        name = "年級：" + eachStu.GradeYear;
                    else
                        throw new ArgumentException("不支援此類型的排名範圍。(" + type.ToString() + ")");
                    break;
                }

                scopes.Add(new RatingScope(eachStus, type, name));
            }
            return scopes;
        }
    }
}
