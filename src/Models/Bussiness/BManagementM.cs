using System;

namespace HACCPExtender.Models.Bussiness
{
    public class BManagementM
    {
        // 0:未更新、1:更新、2:登録、3:削除
        public int EditMode { get; set; }
        public Boolean DelFlg { get; set; }
        public string ShopId { get; set; }
        public string ManagementId { get; set; }
        public string ManageId { get; set; }
        public string ManageNo { get; set; }
        public string ManageName { get; set; }
        public string Unit { get; set; }
        public string UpperLimit { get; set; }
        public string LowerLimit { get; set; }
        public string LocationId { get; set; }
        public string GroupId { get; set; }
        public Int16 DisplayNo { get; set; }
        public string InsUserId { get; set; }
        public string UpdUserId { get; set; }
        public string UpdDate { get; set; }
        public int No { get; set; }
    }
}