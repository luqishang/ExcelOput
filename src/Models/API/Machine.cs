extern alias EF;
using Newtonsoft.Json;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（機器マスタ）
    /// </summary>
    public class Machine
    {
        [Key, Column(Order = 0)]        
        public string MACHINEID { get; set; }
        
        public string MACHINENAME { get; set; }

        [JsonProperty("LOCATIONID", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LOCATIONID { get; set; }

    }
}