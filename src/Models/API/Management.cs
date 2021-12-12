extern alias EF;
using Newtonsoft.Json;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（管理対象マスタ）
    /// </summary>
    public class Management
    {
        [Key, Column(Order = 0)]
        public string MANAGEMENTID { get; set; }

        [Key, Column(Order = 1)]
        public string MANAGEID { get; set; }

        public string MANAGENO { get; set; }

        [JsonProperty("MANAGENAME", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string MANAGENAME { get; set; }

        [JsonProperty("UNIT", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UNIT { get; set; }

        [JsonProperty("UPPERLIMIT", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UPPERLIMIT { get; set; }

        [JsonProperty("LOWERLIMIT", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LOWERLIMIT { get; set; }

        [JsonProperty("LOCATIONID", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LOCATIONID { get; set; }

    }
}