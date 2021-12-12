extern alias EF;
using HACCPExtender.Models.ExcelModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（設問マスタ）
    /// </summary>
    public class Question
    {
        [Key, Column(Order = 0)]
        public string REPORTID { get; set; }

        public string CATEGORYID { get; set; }

        public string LOCATIONID { get; set; }

        [Key, Column(Order = 0)]
        public string QUESTIONID { get; set; }

        public string QUESTION { get; set; }

        [JsonProperty("QUESTIONENG", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string QUESTIONENG { get; set; }

        public string ANSWERTYPEID { get; set; }

        public string NORMALCONDITION { get; set; }

        [JsonProperty("NORMALCONDITION1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string NORMALCONDITION1 { get; set; }

        [JsonProperty("NORMALCONDITION2", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string NORMALCONDITION2 { get; set; }

        public Int16 DISPLAYNO { get; set; }

        internal void GetValue(List<InspectionDetailEM> details)
        {
            throw new NotImplementedException();
        }
    }
}