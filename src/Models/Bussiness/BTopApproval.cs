using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BTopApproval
    {
        // 大分類ID
        public string CategoryId { get; set; }

        // 大分類名
        public string CategoryName { get; set; }

        // 承認データリスト
        public List<MiddleData> BApprovalList { get; set; }

    }
}