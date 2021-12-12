using System;

namespace HACCPExtender.Models.Bussiness
{
    public class FacilityData
    {
        public string SHOPID { get; set; }

        public string CATEGORYID { get; set; }

        public string CATEGORYNAME { get; set; }

        public string PERIOD { get; set; }

        public string PERIODWORD { get; set; }

        public string PERIODSTART { get; set; }

        public string PERIODEND { get; set; }

        public string PERIODSTARTDATE { get; set; }

        public string PERIODENDDATE { get; set; }

        public Int16 STAMPFIELD { get; set; }

        public Int32 CNT { get; set; }

        public bool CompleteData { get; set; }

        public bool RemandFlg { get; set; }

        public bool ApprovalFlg { get; set; }

        // 0:承認待、1：差戻、2：承認依頼未完了
        public string MODE { get; set; }

        public string STATUS { get; set; }

        public Boolean DataChk { get; set; }

        public string FACILITYSNNDATE { get; set; }

        public string FACILITYSNNCOMMENT { get; set; }

        public string FACILITYSNNUSERNAME { get; set; }

        public string FACILITYREQDATE { get; set; }

        public string FACILITYREQCOMMENT { get; set; }

        public string FACILITYREQWORKERNAME { get; set; }

        public string UPDDATE { get; set; }

        public Int16 FACILITYAPPGROUPNO { get; set; }
    }
}