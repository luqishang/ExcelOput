extern alias EF;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データjson用（作業者マスタ）
    /// </summary>
    public class Worker
    {
        [Key, Column(Order = 0)]
        public string WORKERID { get; set; }
        
        public string WORKERNAME { get; set; }
        
        public string MANAGERKBN { get; set; }
        
        public string CATEGORYKBN1 { get; set; }
        
        public string CATEGORYKBN2 { get; set; }
        
        public string CATEGORYKBN3 { get; set; }
        
        public string CATEGORYKBN4 { get; set; }
        
        public string CATEGORYKBN5 { get; set; }
        
        public string CATEGORYKBN6 { get; set; }
        
        public string CATEGORYKBN7 { get; set; }
        
        public string CATEGORYKBN8 { get; set; }
        
        public string CATEGORYKBN9 { get; set; }
        
        public string CATEGORYKBN10 { get; set; }

    }
}