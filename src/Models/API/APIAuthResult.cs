using Newtonsoft.Json;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データoutput用
    /// </summary>
    public class APIAuthResult
    {
        public int Code { get; set; }

        public string Status { get; set; }

        [JsonProperty("ShopNO", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ShopNO { get; set; }

        [JsonProperty("ShopName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ShopName { get; set; }

        [JsonProperty("APIKey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string APIKey { get; set; }
    }
}