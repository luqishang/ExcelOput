using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using static HACCPExtender.Controllers.Common.CommonConstants;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using HACCPExtender.Models.API;
using HACCPExtender.Models;
using System.IO;
using System.Net.Http.Headers;
using System.Web.Hosting;

namespace HACCPExtender.Controllers.API
{
    /// <summary>
    /// WebAPI アップデートファイル
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Produces("application/json")]
    public class UpdateController : ApiController
    {
        private MasterContext context = new MasterContext();
        private APICommonController comm = new APICommonController();

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody]APIUPdate form)
        {
            // 店舗ID
            string shopId = form.ShopNO;
            // APIキー
            string APIKey = form.APIKey;
            // 大分類ID
            string version = form.Version;
            double versionNo;

            // パラメータチェック
            if (string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(APIKey) || string.IsNullOrEmpty(version))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            if (!double.TryParse(version, out versionNo))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            // result json
            object result = new { };

            // APIKey期限の確認
            if (!comm.ChkAPIKey(context, shopId, APIKey))
            {
                // 期限切れの場合
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            var GeneralDt = from ge in context.GeneralPurposeMs
                            where ge.KEY == EnvironmentKey.KEY_APPVERSION
                            select ge;
            string root = string.Empty;

            if (GeneralDt.Count() > 0 && GeneralDt.FirstOrDefault() != null)
            {
                root = GeneralDt.FirstOrDefault().VALUE1;
                string localFilePath = HostingEnvironment.MapPath("~/HACCPExtender.apk");
                if (double.Parse(root) > versionNo && File.Exists(localFilePath))
                {
                    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                    message.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
                    message.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    message.Content.Headers.ContentDisposition.FileName = "HACCPExtender.apk";
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.android.package-archive");
                    message.Content.Headers.Add("ApkVersion", root);
                    return message;
                }
                else
                {
                    return new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.NoContent
                    };
                }
            }
            else
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NoContent
                };
            }




        }

    }
}