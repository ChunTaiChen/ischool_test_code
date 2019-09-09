using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 代表排名範圍類型。
    /// </summary>
    enum ScopeType
    {
        GradeYear,
        Class,
        Dept,
    }

    /// <summary>
    /// 代表特定範圍的學生。
    /// 例：一年級學生、資一忠學生、資處科學生。
    /// </summary>
    class RatingScope : IEnumerable<Student>
    {
        private StudentCollection _students;
        private RatingTargetCollection _targets;
        private ScopeType _scope_type;
        private string _name;

        public RatingScope(StudentCollection students, ScopeType type, string name)
        {
            _targets = new RatingTargetCollection();
            _students = students;
            _scope_type = type;
            _name = name;
        }

        public int Count
        {
            get { return _students.Count; }
        }

        public ScopeType ScopeType
        {
            get { return _scope_type; }
        }

        public RatingTargetCollection RatingTargets
        {
            get { return _targets; }
        }

        public void Rank(RatingMethod method)
        {
            foreach (IRatingTarget each in RatingTargets)
                RatingUtility.Rank(this, each, method);
        }

        public List<Student> GetContainTargetList(IRatingTarget target)
        {
            List<Student> stus = new List<Student>();
            foreach (Student each in _students.Values)
            {
                if (target.ContainsScore(each))
                    stus.Add(each);
            }

            return stus;
        }

        public string Name
        {
            get { return _name; }
        }

        #region IEnumerable<Student> 成員

        public IEnumerator<Student> GetEnumerator()
        {
            return _students.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成員

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _students.Values.GetEnumerator();
        }

        #endregion
    }

    class RatingScopeCollection : List<RatingScope>
    {
    }
}
