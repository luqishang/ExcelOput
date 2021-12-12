using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HACCPExtender.Models.Bussiness
{
    public class BIndustrySelect
    {

        // 選択ドロップダウンリスト
        public bool DropDownFlg { get; set; }

        // 業種リスト
        public List<IndustryData> IndustryList { get; set; }

        // 系列店舗リスト
        public List<AffiliateStoreData> AffiliateStoresList { get; set; }
    }

    public class IndustryData
    {
        public string IndustryId { get; set; }

        public string IndustryName { get; set; }

        public string IndustryDataId { get; set; }

    }

    public  class AffiliateStoreData
    {
        public string ShopId { get; set; }

        public string ShopName { get; set; }

        public string GroupId { get; set; }

        public string IndustryId { get; set; }

    }
}