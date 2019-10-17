using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaoHsiungExamScore_JH.DAO
{
    /// <summary>
    /// 評量科目成績
    /// </summary>
    public class ExamSubjectScore
    {
        /// <summary>
        /// 領域名稱
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// 科目名稱
        /// </summary>
        public string SubjectName { get; set; }

        /// <summary>
        /// 權數
        /// </summary>
        public decimal? Credit { get; set; }

        /// <summary>
        /// 評量分數
        /// </summary>
        public decimal? Score { get; set; }

        /// <summary>
        /// 平時評量
        /// </summary>
        public decimal? AssignmentScore { get; set; }

        /// <summary>
        /// 努力程度
        /// </summary>
        public int? Effort { get; set; }

        /// <summary>
        /// 文字描述
        /// </summary>
        public string Text { get; set; }
    }
}
