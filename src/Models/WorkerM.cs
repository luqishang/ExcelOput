using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（作業者マスタ）
    /// </summary>
    public class WorkerM
    {
        public string SHOPID { get; set; }

        public string WORKERID { get; set; }

        public string WORKERNAME { get; set; }

        public string MANAGERKBN { get; set; }

        public string CATEGORYKBN1 { get; set; }

        public string CATEGORYKBN2 { get; set; }

        public string CATEGORYKBN3 { get; set; }

        public string CATEGORYKBN4 { get; set; }

        public string CATEGORYKBN5 { get; set; }

        public string CATEGORYKBN6 { get; set; }

        public string CATEGORYKBN7 { get; set; }

        public string CATEGORYKBN8 { get; set; }

        public string CATEGORYKBN9 { get; set; }

        public string CATEGORYKBN10 { get; set; }

        public string APPID { get; set; }
        
        public string APPPASS { get; set; }
        
        public string MAILADDRESSPC { get; set; }
        
        public string MAILADDRESSFEATURE { get; set; }
        
        public string TRANSMISSIONTIME1 { get; set; }
        
        public string TRANSMISSIONTIME2 { get; set; }
        
        public string NODISPLAYKBN { get; set; }

        public Int16 DISPLAYNO { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }

    }
}