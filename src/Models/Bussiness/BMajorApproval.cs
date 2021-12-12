using System.Collections.Generic;
using System.Web.Mvc;

namespace HACCPExtender.Models.Bussiness
{
    public class BMajorApproval
    {
        public string ShopId { get; set; }

        public IEnumerable<SelectListItem> CategoryDrop { get; set; }

        public string CategoryId { get; set; }

        public IEnumerable<SelectListItem> PeriodDrop { get; set; }

        public string LocationId { get; set; }

        public string ReportId { get; set; }

        // 周期ID
        public string Period { get; set; }

        // 周期指定日（YYYY-MM-DD）
        public string PeriodDay { get; set; }

        // 周期指定日（YYYYMMDD）
        public string PeriodYMD { get; set; }

        // 承認データ
        public List<MajorData> MajorDatas { get; set; }

        // 承認履歴
        public List<ApprovalHistory> Historys { get; set; }

        // 承認ボタン活性
        public bool ApprovalBtn { get; set; }

        // 差戻ボタン活性
        public bool RemandBtn { get; set; }

        // 承認依頼ボタン活性
        public bool RequestBtn { get; set; }

        // 承認依頼コメント
        public string RequestComment { get; set; }

    }
}