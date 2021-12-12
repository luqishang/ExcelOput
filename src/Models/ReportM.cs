using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（帳票マスタ）
    /// </summary>
    public class ReportM
    {
        public string SHOPID { get; set; }

        public string CATEGORYID { get; set; }

        public string LOCATIONID { get; set; }

        public string REPORTTEMPLATEID { get; set; }

        public string REPORTID { get; set; }

        public string REPORTNAME { get; set; }

        public Int16 STAMPFIELD { get; set; }

        public string PERIOD { get; set; }

        public string BASEMONTH { get; set; }

        public string REFERENCEDATE { get; set; }

        public Int16 DISPLAYNO { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }

    }
}