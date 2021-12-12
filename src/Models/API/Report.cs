extern alias EF;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（帳票マスタ）
    /// </summary>
    public class Report
    {
        public string CATEGORYID { get; set; }
        public string LOCATIONID { get; set; }
        public string REPORTID { get; set; }
        public string REPORTNAME { get; set; }
        public string PERIOD { get; set; }
        public string BASEMONTH { get; set; }
        public string REFERENCEDATE { get; set; }
        public short DISPLAYNO { get; set; }
    }
}