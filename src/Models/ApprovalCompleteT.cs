using System;

namespace HACCPExtender.Models
{
    public class ApprovalCompleteT
    {
        public string SHOPID { get; set; }

        public string PERIOD { get; set; }

        public string PERIODSTART { get; set; }

        public string PERIODEND { get; set; }

        public Int16 FACILITYAPPGROUPNO { get; set; }

        public string APPROVALCOMMENT { get; set; }

        public string APPROVALWORKERID { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }
    }
}