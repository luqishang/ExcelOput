using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（承認経路マスタ）
    /// </summary>
    public class ApprovalRouteM
    {
        public string SHOPID { get; set; }
        
        public string CATEGORYID { get; set; }
        
        public string LOCATIONID { get; set; }
        
        public Int16 APPROVALNODEID { get; set; }
        
        public string APPROVALORDERCLASS { get; set; }
        
        public string APPMANAGERID { get; set; }
        
        public string INSUSERID { get; set; }
                
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }
    }
}