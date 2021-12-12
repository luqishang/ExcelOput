using Newtonsoft.Json;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（マスタデータ更新日付）
    /// </summary>
    public class MstDateTimes
    {
        // 部門マスタ
        [JsonProperty("CategoryM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CategoryM { get; set; }

        // 作業者マスタ
        [JsonProperty("WorkerM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string WorkerM { get; set; }

        // 場所マスタ
        [JsonProperty("LocationM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LocationM { get; set; }

        // 設問マスタ
        [JsonProperty("QuestionM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string QuestionM { get; set; }

        // 回答種類マスタ
        [JsonProperty("AnsTypeM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AnsTypeM { get; set; }

        // 機械マスタ
        [JsonProperty("MachineM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string MachineM { get; set; }

        // 管理対象マスタ(仕入先)
        [JsonProperty("SupplierM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SupplierM { get; set; }

        // 管理対象マスタ(食材)
        [JsonProperty("FoodStuffM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FoodStuffM { get; set; }

        // 管理対象マスタ(料理)
        [JsonProperty("CuisineM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CuisineM { get; set; }

        // 管理対象マスタ(半製品)
        [JsonProperty("SemiFinProductM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SemiFinProductM { get; set; }

        // 管理対象マスタ(ユーザー)
        [JsonProperty("UserM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UserM { get; set; }

        // 手引書マスタ
        [JsonProperty("ManualM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ManualM { get; set; }
 
        // 帳票マスタ
        [JsonProperty("ReportM", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReportM { get; set; }
    }
}