
namespace SmartSchool.Evaluation.WearyDogComputerHelper
{
    /// <summary>
    /// 儲存畢業判斷選項物件
    /// </summary>
    internal class GraduateRule
    {

        public GraduateRule()
        {
            RefStudentID = string.Empty;
            IsDemeritNotExceedMaximum = false;
            IsEverySchoolYearEntryStudiesPass = false;
            TotalCredit = 0;
            RequiredCredit = 0;
            ChoicedCredit = 0;
            EduRequiredCredit = 0;
            SchoolRequiredCredit = 0;
            PhysicalCredit = 0;
            專業及實習總學分數 = 0;
        }

        /// <summary>
        /// 學生系統編號
        /// </summary>
        public string RefStudentID { get; set; }

        /// <summary>
        /// 是否每學年學業成績及格，預設為false。
        /// </summary>
        public bool IsEverySchoolYearEntryStudiesPass { get; set; }

        /// <summary>
        /// 是否功過相抵未滿三大過，預設為False。
        /// </summary>
        public bool IsDemeritNotExceedMaximum { get; set; }

        /// <summary>
        /// 總學分數。
        /// </summary>
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// 必修學分數，包含部、校訂。
        /// </summary>
        public decimal RequiredCredit { get; set; }

        /// <summary>
        /// 選修學分數。
        /// </summary>
        public decimal ChoicedCredit { get; set; }

        /// <summary>
        /// 部訂必須學分數。
        /// </summary>
        public decimal EduRequiredCredit { get; set; }

        /// <summary>
        /// 校訂必修學分數。
        /// </summary>
        public decimal SchoolRequiredCredit { get; set; }

        /// <summary>
        /// 實習學分數。
        /// </summary>
        public decimal PhysicalCredit { get; set; }

        public decimal 專業及實習總學分數 { get; set; }
    }
}