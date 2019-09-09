using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.ExtandColumn
{
    class ClassGraduationPlan:SmartSchool.Customization.PlugIn.ExtendedColumn.IColumnItem
    {
        private Dictionary<string, string> _Values = new Dictionary<string, string>();

        public ClassGraduationPlan()
        {
            EventHub.Instance.ClassReferenceGranduationPlanChanged += new EventHandler(Instance_ClassReferenceGranduationPlanChanged);
        }

        void Instance_ClassReferenceGranduationPlanChanged(object sender, EventArgs e)
        {
            if ( VariableChanged != null )
                VariableChanged.Invoke(this, new EventArgs());
        }

        #region IColumnItem 成員

        public string ColumnHeader
        {
            get { return "課程規劃"; }
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
                if ( GraduationPlan.GraduationPlan.Instance.GetClassGraduationPlan(var) == null )
                    _Values.Add(var, "");
                else
                    _Values.Add(var, GraduationPlan.GraduationPlan.Instance.GetClassGraduationPlan(var).Name);
            }
        }

        public event EventHandler VariableChanged;

        #endregion
    }
}
