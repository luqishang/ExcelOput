using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BReportMs
    {
        public IList<BReportM> BReportMList { get; set; }
        public List<string> BConditionList { get; set; }
    }
}