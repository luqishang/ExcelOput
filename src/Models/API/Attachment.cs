using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace HACCPExtender.Models.API
{
    public class FileUpload
    {
        public string ShopNO { get; set; }

        public string APIKey { get; set; }

        public string DATANO { get; set; }

        public IFormFile Attach { get; set; }

        public string FileName { get; set; }

        public List<Attachment> AttachList { get; set; }
    }

    public class Attachment
    {
        public string FileName { get; set; }

        public string Attachement { get; set; }
    }
}