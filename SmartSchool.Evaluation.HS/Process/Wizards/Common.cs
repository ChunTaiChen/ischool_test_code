using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Wizards
{
    /// <summary>
    /// 選擇學生的方式，是選取學生還是選擇一個年級的學生
    /// </summary>
    public enum SelectType { Student, GradeYearStudent };

    /// <summary>
    /// 學年分項成績的計算方式，是依學期分項成績，還是依學年科目成績
    /// </summary>
    public enum SchoolYearScoreCalcType { SemesterEntryScore, SchoolYearSubject}
}
