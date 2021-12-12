using Newtonsoft.Json;

namespace HACCPExtender.Models.API
{
    public class MstDateTimeResult
    {
        public int Code { get; set; }

        public string Status { get; set; }

        [JsonProperty("data", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MstDateTimes data { get; set; }

    }
}