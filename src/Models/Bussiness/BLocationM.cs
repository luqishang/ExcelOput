using System;

namespace HACCPExtender.Models.Bussiness
{
    /// <summary>
    /// 画面用データモデル（場所マスタ）
    /// </summary>
    public class BLocationM
    {
        // 0:未更新、1:更新、2:登録、3:削除
        public int EditMode { get; set; }

        public Boolean DelFlg { get; set; }

        public string ShopId { get; set; }

        public string LocationId { get; set; }

        public string LocationName { get; set; }

        public string LocationNameEng { get; set; }

        public Boolean ManagerKbn { get; set; }

        public Int16 DisplayNo { get; set; }

        public string InsUserId { get; set; }

        public string UpdUserId { get; set; }

        public string UpdDate { get; set; }

        public int No { get; set; }
    }
}