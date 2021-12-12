using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BManagementMs
    {
        public IList<BManagementM> BManagementMList { get; set; }
        public List<int> BCsvPosList { get; set; }
        public List<BUploadFile> BUploadFileList { get; set; }
    }
}