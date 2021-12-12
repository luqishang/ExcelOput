using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（管理対象マスタ）
    /// </summary>
    public class ManagementM
    {
        public string SHOPID { get; set; }
        
        public string MANAGEMENTID { get; set; }
        
        public string MANAGEID { get; set; }
        
        public string MANAGENO { get; set; }
        
        public string MANAGENAME { get; set; }
        
        public string UNIT { get; set; }
        
        public Int16 UPPERLIMIT { get; set; }
        
        public Int16 LOWERLIMIT { get; set; }
        
        public string LOCATIONID { get; set; }
        
        public Int16 DISPLAYNO { get; set; }
        
        public string INSUSERID { get; set; }
                
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }
    }
}