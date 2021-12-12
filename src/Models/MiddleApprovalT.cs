using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（中分類承認情報）
    /// </summary>
    public class MiddleApprovalT
    {
        public string SHOPID { get; set; }
        
        public string CATEGORYID { get; set; }
        
        public string LOCATIONID { get; set; }
        
        public string REPORTID { get; set; }
        
        public string APPROVALID { get; set; }
        
        public string DATAYMD { get; set; }
        
        public Int16 MIDDLEGROUPNO { get; set; }
        
        public string STATUS { get; set; }
        
        public string MIDDLESNNDATE { get; set; }
        
        public string MIDDLESNNCOMMENT { get; set; }
        
        public string MIDDLESNNUSER { get; set; }
        
        public string PERIOD { get; set; }
        
        public string PERIODSTART { get; set; }
        
        public string PERIODEND { get; set; }
        
        public Int16 STAMPFIELD { get; set; }
        
        public string WORKERID { get; set; }
        
        public string WORKERNAME { get; set; }
        
        public string REPORTNAME { get; set; }
        
        public string REPORTTEMPLATEID { get; set; }
        
        public string REPORTTEMPLATETYPE { get; set; }
        
        public string REPORTTEMPLATENAME { get; set; }
        
        public string REPORTFILENAME { get; set; }
        
        public string REPORTFILEPASS { get; set; }
        
        public string DELETEFLAG { get; set; }
        
        public string INSUSERID { get; set; }
        
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }

    }
}