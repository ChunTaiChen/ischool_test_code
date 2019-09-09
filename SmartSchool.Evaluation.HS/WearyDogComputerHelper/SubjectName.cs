using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.WearyDogComputerHelper
{
    public struct SubjectName
    {
        public SubjectName(string name, string level)
            : this()
        {
            Name = name;
            Level = level.Trim();
        }

        public string Name { get; private set; }

        public string Level { get; private set; }
    }

    public class SubjectSet : IEnumerable<SubjectName>
    {
        private Dictionary<SubjectName, object> _subjects = new Dictionary<SubjectName, object>();

        public SubjectSet()
        {
            _subjects = new Dictionary<SubjectName, object>();
        }

        public SubjectSet(IEnumerable<SubjectName> subjects)
        {
            foreach (SubjectName each in subjects)
                _subjects.Add(each, null);
        }

        public void Add(SubjectName subject)
        {
            _subjects.Add(subject, null);
        }

        public bool Contains(SubjectName subject)
        {
            return _subjects.ContainsKey(subject);
        }

        #region IEnumerable<SubjectName> 成員

        public IEnumerator<SubjectName> GetEnumerator()
        {
            return _subjects.Keys.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成員

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _subjects.Keys.GetEnumerator();
        }

        #endregion
    }
}
