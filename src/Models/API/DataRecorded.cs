using System.Collections.Generic;

namespace HACCPExtender.Models.API
{
    public class DataRecorded
    {
        public string ShopNO { get; set; }

        public string APIKey { get; set; }

        public List<DataCooperation> DataList { get; set; }
    }
}