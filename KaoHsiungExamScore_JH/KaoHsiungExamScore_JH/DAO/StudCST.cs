using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaoHsiungExamScore_JH.DAO
{
    /// <summary>
    /// 學生班級、座號、導師
    /// </summary>
    public class StudCST
    {
        public string StudentID { get; set; }

        public string ClassName { get; set; }

        public string SeatNo { get; set; }

        public string TeacherName { get; set; }
    }
}
