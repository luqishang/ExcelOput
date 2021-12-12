using HACCPExtender.Models.API;
using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（店舗別回答種類マスタ）
    /// </summary>
    public class Shop_AnswerTypeM
    {
        public string SHOPID { get; set; }
        public string ANSWERTYPEID { get; set; }
        
        public string ANSWERTYPENAME { get; set; }
        
        public string ANSWERKBN { get; set; }
        
        public string ANSWERCONTENT { get; set; }
        
        public string INSUSERID { get; set; }
        
        public string UPDUSERID { get; set; }
        
        public DateTime UPDDATE { get; set; }

        public AnswerTypeM GetAnswerTypeM()
        {
            return new AnswerTypeM
            {
                ANSWERTYPEID = this.ANSWERTYPEID,
                ANSWERTYPENAME = this.ANSWERTYPENAME,
                ANSWERKBN = this.ANSWERKBN,
                ANSWERCONTENT = this.ANSWERCONTENT,
                INSUSERID = this.INSUSERID,
                UPDUSERID = this.UPDUSERID,
                UPDDATE = this.UPDDATE
            };
        }
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