using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BManualMs
    {
        public IList<BManualM> BManualMList { get; set; }
        public string TargetManualId { get; set; }
        public string ManualName { get; set; }
        public BUploadFile UploadManual { get; set; }
    }
}