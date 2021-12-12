namespace HACCPExtender.Models.Bussiness
{
    /// <summary>
    /// 画面用データモデル（帳票ダウンロード）
    /// </summary>
    public class BReportDownload
    {
        public bool CheckFlg { get; set; }

        public string ShopId { get; set; }

        public string CategoryId { get; set; }

        public string LocationId { get; set; }

        public string LocationName { get; set; }

        public string ReportId { get; set; }

        public string ReportName { get; set; }

        public string ReportTemplateId { get; set; }

        public string DownloadStart { get; set; }

        public string DownloadEnd { get; set; }

        public string DownloadStartDisp { get; set; }

        public string DownloadEndDisp { get; set; }

        public int No { get; set; }
    }
}