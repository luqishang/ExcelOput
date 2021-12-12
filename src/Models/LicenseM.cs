using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（ライセンスマスタ）
    /// </summary>
    public class LicenseM
    {
        public string SHOPID { get; set; }

        public string LICENSEKEY { get; set; }

        public Int16 LICENSECONTRACT { get; set; }

        public string MEMO { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }

    }
}