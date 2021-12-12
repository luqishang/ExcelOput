using System;

namespace HACCPExtender.Models.Bussiness
{
    public class BCategoryM
    {
        // 0:未登録、1:未更新
        public int EditMode { get; set; }

        public Boolean DelFlg { get; set; }

        public string ShopId { get; set; }

        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryNameEng { get; set; }

        public Int16 DisplayNo { get; set; }

        public string InsUserId { get; set; }

        public string UpdUserId { get; set; }

        public string UpdDate { get; set; }

        public int No { get; set; }
    }
}