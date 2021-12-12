using Newtonsoft.Json;
using System;

namespace HACCPExtender.Models.API
{
    public class LatestDateRecord
    {
        public string CATEGORYID { get; set; }
        
        public string LOCATIONID { get; set; }
        
        public string REPORTID { get; set; }

        [JsonProperty("WORKERID", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string WORKERID { get; set; }

        [JsonProperty("WORKERNAME", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string WORKERNAME { get; set; }
        
        public string DATAYMD { get; set; }
    }
}