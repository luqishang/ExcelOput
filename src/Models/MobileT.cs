using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（モバイル端末情報）
    /// </summary>
    public class MobileT
    {
        public string SHOPID { get; set; }

        public Int16 TERMINALNO { get; set; }

        public string GUID { get; set; }

        public string APIKEY { get; set; }

        public string EXPIRATION { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }
    }
}