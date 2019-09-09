using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SmartSchool.Evaluation.GraduationPlan.Editor
{
   public   interface  IGraduationPlanEditor
    {
        void SetSource(XmlElement source);
        XmlElement GetSource();
        XmlElement GetSource(string schoolYear);
        bool IsDirty { get;}
        event EventHandler IsDirtyChanged;
        bool IsValidated
        {
            get;
        }
    }
}
