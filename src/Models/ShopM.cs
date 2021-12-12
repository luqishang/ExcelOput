using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（店舗マスタ）
    /// </summary>
    public class ShopM
    {
        public string SHOPID { get; set; }
        
        public string SHOPNAME { get; set; }
        
        public string CONTRACTID { get; set; }
        
        public string INDUSTRYID { get; set; }

        public string STORAGEFNAME { get; set; }

        public string INSUSERID { get; set; }
        
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }
    }
}