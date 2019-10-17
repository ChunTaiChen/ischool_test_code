using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaoHsiungExamScore_JH.DAO
{
    /// <summary>
    /// 評量領域成績
    /// </summary>
    public class ExamDomainScore
    {
        /// <summary>
        /// 領域名稱
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// 領域學分
        /// </summary>
        public decimal? Credit { get; set; }

        /// <summary>
        /// 有成績的領域學分
        /// </summary>
        public decimal? Credit1 { get; set; }

        /// <summary>
        /// 領域成績
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
