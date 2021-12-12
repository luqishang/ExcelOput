using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（手引書マスタ）
    /// </summary>
    public class ManualM
    {
        public string SHOPID { get; set; }

        public string MANUALID { get; set; }

        public string UPLOADDATE { get; set; }

        public string MANUALNAME { get; set; }

        public string MANUALPATH { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }
    }
}