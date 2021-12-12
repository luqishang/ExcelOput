using System.Web;

namespace HACCPExtender.Models.Bussiness
{
    public class BUploadFile
    {
        public HttpPostedFileBase UploadFile { get; set; }

        public bool HeadFlg { get; set; }
    }
}