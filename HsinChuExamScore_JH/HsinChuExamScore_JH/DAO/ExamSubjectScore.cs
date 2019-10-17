using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HsinChuExamScore_JH.DAO
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
        /// 學分數
        /// </summary>
        public decimal? Credit { get; set; }

        /// <summary>
        /// 定期評量
        /// </summary>
        public decimal? ScoreF { get; set; }

        /// <summary>
        /// 平時評量
        /// </summary>
        public decimal? ScoreA { get; set; }

        /// <summary>
        /// 總成績
        /// </summary>
        public decimal? ScoreT { get; set; }

        /// <summary>
        /// 文字評量
        /// </summary>
        public string Text { get; set; }
    }
}
