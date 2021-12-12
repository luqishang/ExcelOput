using System.Collections.Generic;

namespace HACCPExtender.Models.Bussiness
{
    public class BQuestionMs
    {
        public IList<BQuestionM> BQuestionMList { get; set; }
        public List<string> BConditionList { get; set; }
    }
}