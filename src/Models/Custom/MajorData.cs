using System;

namespace HACCPExtender.Models.Bussiness
{
    public class MajorData
    {
        public string SHOPID { get; set; }

        public string CATEGORYID { get; set; }

        public string CATEGORYNAME { get; set; }

        public short CATEGORYORDER { get; set; }

        public string LOCATIONID { get; set; }

        public string LOCATIONNAME { get; set; }

        public string REPORTID { get; set; }

        public string REPORTNAME { get; set; }

        public string PERIOD { get; set; }

        public string PERIODWORD { get; set; }

        public string PERIODSTART { get; set; }

        public string PERIODEND { get; set; }

        public string PERIODSTARTDATE { get; set; }

        public string PERIODENDDATE { get; set; }

        public Int16 STAMPFIELD { get; set; }

        public string FacilityUpddate { get; set; }

        public string FacilityStatus { get; set; }

        public bool RemandFlg { get; set; }

        public bool ApprovalFlg { get; set; }

        public Int32 CNT { get; set; }

        // 0:承認待、1：差戻、2：承認依頼未完了
        public string MODE { get; set; }

        public string STATUS { get; set; }

        public Boolean DataChk { get; set; }

        public string MAJORSNNDATE { get; set; }

        public string MAJORSNNCOMMENT { get; set; }

        public string MAJORSNNUSERNAME { get; set; }

        public string MAJORREQDATE { get; set; }

        public string MAJORREQCOMMENT { get; set; }

        public string MAJORREQWORKERNAME { get; set; }

        public string UPDDATE { get; set; }

        public Int16 MAJORGROUPNO { get; set; }
    }
}