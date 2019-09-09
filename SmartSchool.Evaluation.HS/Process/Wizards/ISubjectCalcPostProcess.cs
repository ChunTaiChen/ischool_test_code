using SmartSchool.Customization.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartSchool.Evaluation.Process.Wizards
{
    public interface ISubjectCalcPostProcess
    {
        void ShowConfigForm();
        void PostProcess(int schoolYear,int semester,List<StudentRecord> list);
    }
}
