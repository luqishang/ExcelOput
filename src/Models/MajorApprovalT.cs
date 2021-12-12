using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（大分類承認情報）
    /// </summary>
    public class MajorApprovalT
    {
        public string SHOPID { get; set; }
        
        public string CATEGORYID { get; set; }
        
        public string LOCATIONID { get; set; }
        
        public string REPORTID { get; set; }
        
        public string PERIOD { get; set; }
        
        public string PERIODSTART { get; set; }
        
        public string PERIODEND { get; set; }

        public Int16 MIDDLEGROUPNO { get; set; }
        
        public Int16 MAJORGROUPNO { get; set; }
        
        public string STATUS { get; set; }

        public Int16 STAMPFIELD { get; set; }

        public string REPORTNAME { get; set; }
        
        public string MAJORREQDATE { get; set; }
        
        public string MAJORREQCOMMENT { get; set; }

        public string MAJORREQWORKERID { get; set; }

        public string MAJORSNNDATE { get; set; }

        public string MAJORSNNCOMMENT { get; set; }

        public string MAJORSNNUSER { get; set; }

        public string DELETEFLAG { get; set; }

        public string INSUSERID { get; set; }
        
        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }

        // Cloneメソッドを使用
        public MajorApprovalT Clone()
        {
            // Object型で返ってくるのでキャストが必要
            return (MajorApprovalT)MemberwiseClone();
        }

    }
}