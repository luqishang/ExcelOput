using System.Collections.Generic;
using System.Web.Mvc;

namespace HACCPExtender.Models.Bussiness
{
    public class BFacilityApproval
    {
        public string ShopId { get; set; }

        public IEnumerable<SelectListItem> PeriodDrop { get; set; }

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
        public List<FacilityData> FacilityDatas { get; set; }

        // 承認履歴
        public List<ApprovalHistory> Historys { get; set; }

        // 承認ボタン活性
        public bool ApprovalBtn { get; set; }

        // 差戻ボタン活性
        public bool RemandBtn { get; set; }

        // 承認依頼ボタン活性
        public bool CompleteBtn { get; set; }

        // 承認依頼コメント
        public string CompleteComment { get; set; }

        // 承認完了データ
        public bool ApprovalComplete { get; set; }

    }
}