extern alias EF;
using Newtonsoft.Json;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（場所マスタ）
    /// </summary>
    public class Location
    {
        [Key, Column(Order = 0)]
        public string LOCATIONID { get; set; }

        public string LOCATIONNAME { get; set; }

        [JsonProperty("LOCATIONNAMEENG", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LOCATIONNAMEENG { get; set; }

        public string MANAGERKBN { get; set; }
    }
}