using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BTop
    {
        // お知らせ情報リスト
        public List<BNotification> BnotificList { get; set; }

        // 承認過去データ有無
        public bool HistorucalData { get; set; }

        // 処理日承認待ちデータリスト
        public List<BTopApproval> ApprovalDataList { get; set; }

    }
}