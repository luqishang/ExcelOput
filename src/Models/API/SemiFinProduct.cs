﻿extern alias EF;
using Newtonsoft.Json;
using System;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    public class SemiFinProduct
    {
        [Key, Column(Order = 0)]
        public string MANAGEID { get; set; }

        public string SEMIFINPRODUCTNO { get; set; }

        public string SEMIFINPRODUCTNAME { get; set; }

        [JsonProperty("UNIT", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UNIT { get; set; }

        [JsonProperty("UPPERLIMIT", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Int16 UPPERLIMIT { get; set; }

        [JsonProperty("LOWERLIMIT", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Int16 LOWERLIMIT { get; set; }

        [JsonProperty("LOCATIONID", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LOCATIONID { get; set; }
    }
}