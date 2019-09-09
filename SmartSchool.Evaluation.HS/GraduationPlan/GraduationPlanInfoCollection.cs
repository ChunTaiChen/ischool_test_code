using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace SmartSchool.Evaluation.GraduationPlan
{
    public class GraduationPlanInfoCollection : IEnumerable<GraduationPlanInfo>
    {
        private Dictionary<string, GraduationPlanInfo> _Items;
        internal GraduationPlanInfoCollection(Dictionary<string, GraduationPlanInfo> items)
        {
            _Items = items;
        }
        public GraduationPlanInfo this[string ID]
        {
            get
            {
                if (_Items.ContainsKey(ID))
                    return _Items[ID];
                else
                    return null;
            }
        }

        #region IEnumerable<GraduationPlanInfo> 成員

        public IEnumerator<GraduationPlanInfo> GetEnumerator()
        {
            return ((IEnumerable<GraduationPlanInfo>)_Items.Values).GetEnumerator();
        }

        #endregion

        #region IEnumerable 成員

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Items.Values.GetEnumerator();
        }

        #endregion
    }
}
