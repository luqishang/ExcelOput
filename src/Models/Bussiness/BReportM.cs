using System;

namespace HACCPExtender.Models.Bussiness
{
    public class BReportM
    {
        public Boolean DelFlg { get; set; }

        public string ShopId { get; set; }

        public string CategoryId { get; set; }

        public string LocationId { get; set; }

        public string ReportTemplateId { get; set; }

        public string ReportId { get; set; }

        public string ReportName { get; set; }

        public string StampField { get; set; }

        public string Period { get; set; }

        public string BaseMonth { get; set; }

        public string ReferenceDate { get; set; }

        public Int16 DisplayNo { get; set; }

        public string InsUserId { get; set; }

        public string UpdUserId { get; set; }

        public string UpdDate { get; set; }

        public int No { get; set; }
    }
}