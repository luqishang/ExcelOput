using System.Collections.Generic;

namespace HACCPExtender.Models.Custom
{
    /// <summary>
    /// 帳票Interface Model
    /// author : PTJ cheng
    /// Create Date : 2020/09/16
    /// </summary>
    public class CustomReportInterfaceM
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
        /// パス
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 帳票タイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public string ManageId { get; set; }

        /// <summary>
        /// 帳票テンプレートID
        /// </summary>
        public string TemplateID { get; set; }

        /// <summary>
        /// 帳票リスト
        /// </summary>
        public List<CustomReportM> ReportList { get; set; }
    }

    public class CustomReportM
    {
        /// <summary>
        /// 場所ID
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// 帳票ID
        /// </summary>
        public string ReportId { get; set; }
    }
}

