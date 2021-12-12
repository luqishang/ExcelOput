extern alias EF;
using EF::System.ComponentModel.DataAnnotations;
using EF::System.ComponentModel.DataAnnotations.Schema;

namespace HACCPExtender.Models.Custom
{
    public class ManagerWorker
    {
        [Key, Column(Order = 1)]
        public string WORKERID { get; set; }

        public string WORKERNAME { get; set; }

    }
}