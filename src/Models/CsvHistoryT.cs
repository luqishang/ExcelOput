using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（CSV履歴情報）
    /// </summary>
    public class CsvHistoryT
    {
        public string SHOPID { get; set; }

        public string MANAGEMENTID { get; set; }

        public string UPLOADDATE { get; set; }

        public string FILENAME { get; set; }

        public System.Int16 CODEPOS { get; set; }

        public System.Int16 DATAPOS { get; set; }

        public System.Int16 UNITPOS { get; set; }

        public System.Int16 UPPERLIMITPOS { get; set; }

        public System.Int16 LOWERLIMITPOS { get; set; }

        public System.Int16 LOCATIONIDPOS { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }
    }
}