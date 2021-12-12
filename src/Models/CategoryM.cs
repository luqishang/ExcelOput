using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（部門マスタ）
    /// </summary>
    public class CategoryM
    {
        public string SHOPID { get; set; }
        
        public string CATEGORYID { get; set; }
        
        public string CATEGORYNAME { get; set; }
        
        public string CATEGORYNAMEENG { get; set; }
        
        public Int16 DISPLAYNO { get; set; }
        
        public string INSUSERID { get; set; }
                
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }

    }
}