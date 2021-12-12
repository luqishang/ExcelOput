using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BWorkerMs
    {
        public IList<BWorkerM> BWorkerMList { get; set; }
        public List<int> BCsvPosList { get; set; }
        public List<BUploadFile> BUploadFileList { get; set; }
    }
}