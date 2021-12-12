using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BDataHistory
    {
        // 周期開始日（YYYY/MM/DD）
        public string PeriodStartDate { get; set; }

        // 周期終了日（YYYY/MM/DD）
        public string PeriodEndDate { get; set; }

        // 条件リスト
        public List<string> BConditionList { get; set; }

        // 温度衛生管理情報データ
        public List<List<BTemperatureControl>> TemperatureControlDatas { get; set; }

        // 承認履歴
        public List<ApprovalHistory> Historys { get; set; }
    }
}