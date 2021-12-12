extern alias EF;
using Newtonsoft.Json;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（部門マスタ）
    /// </summary>
    public class Category
    {
        [Key, Column(Order = 0)]
        public string CATEGORYID { get; set; }

        public string CATEGORYNAME { get; set; }

        [JsonProperty("CATEGORYNAMEENG", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CATEGORYNAMEENG { get; set; }

    }
}