using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BApprovalDataDetail
    {
        public string nodeId { get; set; }

        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string LocationId { get; set; }

        public string LocationName { get; set; }

        public string ReportId { get; set; }

        public string ReportName { get; set; }

        public string Period { get; set; }

        public string PeriodStart { get; set; }

        public int QuestionNo { get; set; }

        public int ReportFileCount { get; set; }

        public string ReportFileName { get; set; }

        public string ReportFilePath { get; set; }

        public List<List<BTemperatureControl>> ApprovalDataList { get; set; }
    }
}