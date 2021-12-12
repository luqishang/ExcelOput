extern alias EF;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（手引書マスタ）
    /// </summary>
    public class Manual
    {
        public string ManualID { get; set; }

        public string UploadDate { get; set; }

        public string ManualName { get; set; }

        public string ManualPath { get; set; }
    }
}