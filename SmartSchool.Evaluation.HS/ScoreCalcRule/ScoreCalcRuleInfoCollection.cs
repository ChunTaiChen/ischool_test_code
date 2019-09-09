using System.Collections;
using System.Collections.Generic;

namespace SmartSchool.Evaluation.ScoreCalcRule
{
    public class ScoreCalcRuleInfoCollection : IEnumerable<ScoreCalcRuleInfo>
    {
        private Dictionary<string, ScoreCalcRuleInfo> _Items;
        internal ScoreCalcRuleInfoCollection(Dictionary<string, ScoreCalcRuleInfo> items)
        {
            _Items = items;
        }

        public ScoreCalcRuleInfo this[string ID]
        {
            get
            {
                if (_Items.ContainsKey(ID))
                    return _Items[ID];
                else
                    return null;
            }
        }

        #region IEnumerable<ScoreCalcRuleInfo> 成員

        public IEnumerator<ScoreCalcRuleInfo> GetEnumerator()
        {
            return ((IEnumerable<ScoreCalcRuleInfo>)_Items.Values).GetEnumerator();
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
