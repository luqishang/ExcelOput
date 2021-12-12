using System;

namespace HACCPExtender.Models.Bussiness
{
    public class BMachineM
    {
        // 0:未登録、1:未更新
        public int EditMode { get; set; }

        public Boolean DelFlg { get; set; }

        public string ShopId { get; set; }

        public string LocationId { get; set; }

        public string MachineId { get; set; }

        public string MachineName { get; set; }

        public Int16 DisplayNo { get; set; }

        public string InsUserId { get; set; }

        public string UpdUserId { get; set; }

        public string UpdDate { get; set; }

        public int No { get; set; }
    }
}