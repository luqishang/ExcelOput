using HACCPExtender.Models.API;
using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（回答種類マスタ）
    /// </summary>
    public class AnswerTypeM
    {
        public string ANSWERTYPEID { get; set; }
        
        public string ANSWERTYPENAME { get; set; }
        
        public string ANSWERKBN { get; set; }
        
        public string ANSWERCONTENT { get; set; }
        
        public string INSUSERID { get; set; }
        
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }

        public AnswerType GetAnswerType()
        {
            return new AnswerType
            {
                ANSWERTYPEID = this.ANSWERTYPEID,
                ANSWERTYPENAME = this.ANSWERTYPENAME,
                ANSWERKBN = this.ANSWERKBN,
                ANSWERCONTENT = this.ANSWERCONTENT
            };
        }
    }
}