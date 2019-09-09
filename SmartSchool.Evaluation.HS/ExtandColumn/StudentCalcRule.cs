using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.ExtandColumn
{
    class StudentCalcRule : SmartSchool.Customization.PlugIn.ExtendedColumn.IColumnItem
    {
        private Dictionary<string, string> _Values = new Dictionary<string, string>();

        public StudentCalcRule()
        {
            EventHub.Instance.ClassReferenceCaleRuleChanged += new EventHandler(Instance_ClassReferenceCaleRuleChanged);
            EventHub.Instance.StudentReferenceCaleRuleChanged += new EventHandler(Instance_StudentReferenceCaleRuleChanged);
        }

        void Instance_StudentReferenceCaleRuleChanged(object sender, EventArgs e)
        {
            if ( VariableChanged != null )
                VariableChanged.Invoke(this, new EventArgs());
        }

        void Instance_ClassReferenceCaleRuleChanged(object sender, EventArgs e)
        {
            if ( VariableChanged != null )
                VariableChanged.Invoke(this, new EventArgs());
        }

        #region IColumnItem 成員

        public string ColumnHeader
        {
            get { return "計算規則"; }
        }

        public Dictionary<string, string> ExtendedValues
        {
            get { return _Values; }
        }

        public void FillExtendedValues(List<string> identities)
        {
            _Values.Clear();
            foreach ( string var in identities )
            {
                if ( ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var) == null )
                    _Values.Add(var, "");
                else
                    _Values.Add(var, ( ScoreCalcRule.ScoreCalcRule.Instance.IsStudentOverrided(var) ? "(指定)" : "" ) + ScoreCalcRule.ScoreCalcRule.Instance.GetStudentScoreCalcRuleInfo(var).Name);
            }
        }

        public event EventHandler VariableChanged;

        #endregion
    }
}
