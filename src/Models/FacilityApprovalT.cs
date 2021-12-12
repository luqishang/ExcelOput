using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（施設承認情報）
    /// </summary>
    public class FacilityApprovalT
    {
        public string SHOPID { get; set; }
        
        public string CATEGORYID { get; set; }
        
        public string PERIOD { get; set; }
        
        public string PERIODSTART { get; set; }
        
        public string PERIODEND { get; set; }
        
        public Int16 MAJORGROUPNO { get; set; }

        public Int16 FACILITYAPPGROUPNO { get; set; }

        public string STATUS { get; set; }

        public Int16 STAMPFIELD { get; set; }

        public string FACILITYREQDATE { get; set; }

        public string FACILITYREQCOMMENT { get; set; }

        public string FACILITYREQWORKERID { get; set; }

        public string FACILITYSNNDATE { get; set; }

        public string FACILITYSNNCOMMENT { get; set; }

        public string FACILITYSNNUSER { get; set; }

        public string DELETEFLAG { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }

        // Cloneメソッドを使用
        public FacilityApprovalT Clone()
        {
            // Object型で返ってくるのでキャストが必要
            return (FacilityApprovalT)MemberwiseClone();
        }
    }
}