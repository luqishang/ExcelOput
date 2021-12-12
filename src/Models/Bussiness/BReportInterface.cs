namespace HACCPExtender.Models.Custom
{
    /// <summary>
    /// 帳票Interface Model
    /// author : PTJ cheng
    /// Create Date : 2020/09/16
    /// </summary>
    public class BReportInterface
    {
        /// <summary>
        /// 店舗ID
        /// </summary>
        public string ShopId { get; set; }

        /// <summary>
        /// 部門ID
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// 周期
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// 周期開始日
        /// </summary>
        public string PeriodStart { get; set; }

        /// <summary>
        /// 帳票タイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 場所ID
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// 帳票ID
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// 帳票テンプレートID
        /// </summary>
        public string ReportTemplateId { get; set; }

        /// <summary>
        /// 帳票マージ単位
        /// </summary>
        public string ReportMergeUnit { get; set; }
    }

}

