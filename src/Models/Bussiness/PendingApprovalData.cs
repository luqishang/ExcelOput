using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class PendingApprovalData
    {
        public List<MiddleData> MiddleDatas { get; set; }

        public List<MajorData> MajorDatas { get; set; }

        public List<FacilityData> FacilityDatas { get; set; }
    }
}