using System;

namespace HACCPExtender.Models.Bussiness
{
    public class BCsvHistoryT
    {
        // 0:未更新、1:更新、2:登録、3:削除
        public int EditMode { get; set; }
        public Boolean DelFlg { get; set; }
        public string ShopId { get; set; }
        public string ManagementId { get; set; }
        public string UploadTime { get; set; }
        public string FileName { get; set; }
        public string CodePos { get; set; }
        public string DataPos { get; set; }
        public string UnitPos { get; set; }
        public string UpperLimitPos { get; set; }
        public string LowerLimitPos { get; set; }
        public string LocationIdPos { get; set; }
        public string InsUserId { get; set; }
        public string UpdUserId { get; set; }
        public DateTime UpdDate { get; set; }
    }
}