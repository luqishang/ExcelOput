extern alias EF;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.API
{
    public class Supplier
    {
        [Key, Column(Order = 0)]
        public string MANAGEID { get; set; }

        public string SUPPLIERNO { get; set; }

        public string SUPPLIERNAME { get; set; }
    }
}