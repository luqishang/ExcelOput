using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HACCPExtender.Models.Bussiness
{
    public class BApprovaler
    {
        // 0:未更新、1:更新、2:登録、3:削除
        public int EditMode { get; set; }

        public Int16 ApprovalNodeId { get; set; }

        public Boolean DelFlg { get; set; }

        public string ApprovalManagerId { get; set; }

        public string ShopId { get; set; }

        public string CategoryId { get; set; }

        public string LocationId { get; set; }

        public string ApprovalOrderClass { get; set; }

        public IEnumerable<SelectListItem> ManagerDropList { get; set; }

        public string InsUertId { get; set; }

        public string UpdUserId { get; set; }

        public string Update { get; set; }

    }
}