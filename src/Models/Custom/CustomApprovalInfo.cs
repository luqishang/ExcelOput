namespace HACCPExtender.Models.Custom
{
    /// <summary>
    /// 承認者情報
    /// author : PTJ Cheng
    /// Create Date : 2020/09/18
    /// </summary>
    public class CustomApprovalInfo
    {
        /// <summary>
        /// 施設承認者の名前
        /// </summary>
        public string FacilityApprovalName { get; set; }

        /// <summary>
        /// 大分類承認者の名前
        /// </summary>
        public string MajorApprovalName { get; set; }

        /// <summary>
        /// 中分類承認者の名前
        /// </summary>
        public string MiddleApprovalName { get; set; }
    }
}