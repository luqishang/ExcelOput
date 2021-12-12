using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（設問マスタ）
    /// </summary>
    public class QuestionM
    {
        public string SHOPID { get; set; }
        
        public string REPORTID { get; set; }
        
        public string CATEGORYID { get; set; }
        
        public string LOCATIONID { get; set; }
        
        public string QUESTIONID { get; set; }
        
        public string QUESTION { get; set; }
        
        public string QUESTIONENG { get; set; }
        
        public string ANSWERTYPEID { get; set; }
        
        public string NORMALCONDITION { get; set; }
        
        public string NORMALCONDITION1 { get; set; }
        
        public string NORMALCONDITION2 { get; set; }
        
        public Int16 DISPLAYNO { get; set; }
        
        public string DELETEFLAG { get; set; }
        
        public string INSUSERID { get; set; }
        
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }

    }
}