using System.Collections.Generic;
using System.Web.Mvc;

namespace HACCPExtender.Models.Bussiness
{
    public class BMiddleApproval
    {
        public string ShopId { get; set; }

        public IEnumerable<SelectListItem> CategoryDrop { get; set; }

        public string CategoryId { get; set; }

        public IEnumerable<SelectListItem> LoactionDrop { get; set; }

        public string LocationId { get; set; }

        public IEnumerable<SelectListItem> ReportDrop { get; set; }

        public string ReportId { get; set; }

        // 周期ID
        public string Period { get; set; }

        // 周期指定日（YYYY-MM-DD）
        public string PeriodDay { get; set; }

        // 周期指定日（YYYYMMDD）
        public string PeriodYMD { get; set; }

        // 周期開始日（YYYYMMDD）
        public string PeriodStart { get; set; }

        // 周期終了日（YYYYMMDD）
        public string PeriodEnd { get; set; }

        // 周期開始日（YYYY/MM/DD）
        public string PeriodStartDate { get; set; }

        // 周期終了日（YYYY/MM/DD）
        public string PeriodEndDate { get; set; }

        // 承認データ
        public List<MiddleData> MiddleDatas { get; set; }

        // 承認履歴
        public List<ApprovalHistory> Historys { get; set; }

        // 承認ボタン活性
        public bool ApprobalBtn { get; set; }

        // 承認依頼ボタン活性
        public bool RequestBtn { get; set; }

        // 承認依頼コメント
        public string RequestComment { get; set; }

        // 大分類データ更新日付
        public string MajorUpdDate { get; set; }

    }
}