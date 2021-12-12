using System;

namespace HACCPExtender.Models.Bussiness
{
    public class ApprovalHistory
    {
        public string APPROVALNODE { set; get; }

        public string APPROVALDATE { set; get; }

        public string APPROVALCOMMENT { set; get; }

        public string STATUS { set; get; }

        public string APPROVALUSER { set; get; }

        public string APPROVALUSERNAME { set; get; }

        public string PERIODSTART { set; get; }

        public string PERIODEND { set; get; }

        public DateTime UPDDATE { get; set; }
    }
}