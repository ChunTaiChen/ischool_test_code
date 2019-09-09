using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSchool.Evaluation.Process.Rating
{
    /// <summary>
    /// 要排名的項目。
    /// </summary>
    [Flags()]
    enum RatingItems
    {
        /// <summary>
        /// 學期科目成績。
        /// </summary>
        SemsSubject = 1,
        /// <summary>
        /// 學年科目成績。
        /// </summary>
        YearSubject = 2,
        /// <summary>
        /// 學期學業成績。
        /// </summary>
        SemsScore = 4,
        /// <summary>
        /// 學年學業成績。
        /// </summary>
        YearScore = 8,
        /// <summary>
        /// 學期德行成績。
        /// </summary>
        SemsMoral = 16,
        /// <summary>
        /// 學年德行成績。
        /// </summary>
        YearMoral = 128,
        /// <summary>
        /// 畢業學業成績。
        /// </summary>
        GraduateScore = 256,
        /// <summary>
        /// 畢業德行成績。
        /// </summary>
        GraduateMoral = 512
    }
}
