extern alias EF;
using Newtonsoft.Json;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（回答種類マスタ）
    /// </summary>
    public class AnswerType
    {
        [Key, Column(Order = 0)]
        public string ANSWERTYPEID { get; set; }

        [JsonProperty("ANSWERTYPENAME", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ANSWERTYPENAME { get; set; }

        public string ANSWERKBN { get; set; }

        [JsonProperty("ANSWERCONTENT", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ANSWERCONTENT { get; set; }
    }
}