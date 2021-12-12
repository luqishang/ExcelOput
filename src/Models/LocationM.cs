using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（場所マスタ）
    /// </summary>
    public class LocationM
    {
        public string SHOPID { get; set; }

        public string LOCATIONID { get; set; }
        
        public string LOCATIONNAME { get; set; }

        public string LOCATIONNAMEENG { get; set; }

        public string MANAGERKBN { get; set; }

        public Int16 DISPLAYNO { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }

    }
}