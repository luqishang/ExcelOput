using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（帳票テンプレートマスタ）
    /// </summary>
    public class ReportTemplateM
    {
        public string SHOPID { get; set; }

        public string TEMPLATEID { get; set; }

        public string TEMPLATENAME { get; set; }

        public string REPORTTEMPLATETYPE { get; set; }

        public string TEMPLATEPATH { get; set; }

        public string MERGEUNIT { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }

    }
}