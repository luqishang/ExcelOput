using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（機器マスタ）
    /// </summary>
    public class MachineM
    {
        public string SHOPID { get; set; }
        
        public string LOCATIONID { get; set; }
        
        public string MACHINEID { get; set; }
        
        public string MACHINENAME { get; set; }
        
        public Int16 DISPLAYNO { get; set; }
        
        public string INSUSERID { get; set; }
        
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }
    }
}