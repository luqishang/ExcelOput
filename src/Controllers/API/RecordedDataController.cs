using HACCPExtender.Models;
using HACCPExtender.Models.API;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Linq;
using static HACCPExtender.Controllers.Common.CommonConstants;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using System.Threading.Tasks;

using System.IO;
using System.Web;
using HACCPExtender.Controllers.Common;
using System.Web.Hosting;

namespace HACCPExtender.Controllers.API
{
    /// <summary>
    /// WebAPI マスタ連携
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Produces("application/json")]
    public class RecordedDataController : ApiController
    {
        private MasterContext context = new MasterContext();
        private APICommonController comm = new APICommonController();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RecordedDataController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
        }

        /// <summary>
        /// データ記録連携処理
        /// </summary>
        /// <param name="form">HttpRequest body</param>
        /// <returns>マスタデータ(json)</returns>
        [Route("api/RecordedData")]
        [HttpPost]
        public HttpResponseMessage TempDataStorage([FromBody] DataRecorded form)
        {
            object result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK };
            string jsonObj = JsonConvert.SerializeObject(result, Formatting.None);

            // 店舗IDが取得できない場合はエラー
            if (string.IsNullOrEmpty(form.ShopNO) || string.IsNullOrEmpty(form.APIKey))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            // 店舗ID
            string shopNo = form.ShopNO;

            // APIKey有効チェック
            if (!comm.ChkAPIKey(context, shopNo, form.APIKey))
            {
                // APIKeyの有効期限切れの場合
                jsonObj = JsonConvert.SerializeObject(new { Code = APIConstants.CODE_APIKEY_EXPIRED, Status = APIConstants.STATUS_NG }, Formatting.None);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(jsonObj, System.Text.Encoding.UTF8, "application/json")
                };
            }

            // データが存在しない場合は処理せず、正常終了
            if (form.DataList.Count == 0)
            {
                jsonObj = JsonConvert.SerializeObject(new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK }, Formatting.None);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(jsonObj, System.Text.Encoding.UTF8, "application/json")
                };
            }

            List<DataCooperation> recordedList = new List<DataCooperation>();

            // 温度衛生管理情報
            var temperaDt = from Tempera in context.TemperatureControlTs
                            where Tempera.SHOPID == shopNo
                            select Tempera;

            foreach (DataCooperation coopetatDt in form.DataList)
            {
                // 店舗ID
                coopetatDt.ShopNO = shopNo;
                // 承認ID
                string recordTime = coopetatDt.DTIME.ToString("yyyyMMddHHmmss");
                coopetatDt.APPROVALID = shopNo + coopetatDt.CATEGORYID + coopetatDt.LOCATIONID + recordTime + coopetatDt.WCD;

                // 既存データ確認
                if (temperaDt.Count() > 0)
                {
                    var existing = temperaDt.Where(
                        a => a.CATEGORYID == coopetatDt.CATEGORYID
                        && a.LOCATIONID == coopetatDt.LOCATIONID
                        && a.REPORTID == coopetatDt.REPORTID
                        && a.APPROVALID == coopetatDt.APPROVALID);

                    if (existing.Count() > 0 && existing.FirstOrDefault() != null)
                    {
                        // 処理済みのため処理をしない。次ループへ進む
                        continue;
                    }
                }

                // 測定温度下限逸脱
                if (string.IsNullOrEmpty(coopetatDt.PV1L) || "Flase".Equals(coopetatDt.PV1L) || "false".Equals(coopetatDt.PV1L))
                {
                    coopetatDt.PV1L = BoolKbn.KBN_FALSE;
                }
                else
                {
                    coopetatDt.PV1L = BoolKbn.KBN_TRUE;
                }
                // 測定温度上限逸脱
                if (string.IsNullOrEmpty(coopetatDt.PV1H) || "Flase".Equals(coopetatDt.PV1H) || "false".Equals(coopetatDt.PV1H))
                {
                    coopetatDt.PV1H = BoolKbn.KBN_FALSE;
                }
                else
                {
                    coopetatDt.PV1H = BoolKbn.KBN_TRUE;
                }

                recordedList.Add(coopetatDt);
            }
            // DB登録処理
            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (DataCooperation insModel in recordedList)
                        {
                            // データ登録
                            context.DataCooperations.Add(insModel);
                        }
                        context.SaveChanges();
                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                    catch (Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                }
            }

            // 非同期処理を起動
            var hidouki = new RecordedDataEditController();
            hidouki.EditRecordedData(recordedList);

            return new HttpResponseMessage()
            {
                Content = new StringContent(jsonObj, System.Text.Encoding.UTF8, "application/json")
            };
        }

        /// <summary>
        /// データ記録ファイルアップロード処理
        /// </summary>
        /// <returns>マスタデータ(json)</returns>
        [Route("api/FileUpload")]
        [HttpPost]
        public async Task<HttpResponseMessage> FileUpload()
        {
            object result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK };
            string jsonObj = JsonConvert.SerializeObject(result, Formatting.None);

            // multipart/form-data 以外は 415 を返す
            if (!Request.Content.IsMimeMultipartContent())
            {
                // APIKeyの有効期限切れの場合
                jsonObj = JsonConvert.SerializeObject(new { Code = APIConstants.CODE_APIKEY_EXPIRED, Status = APIConstants.STATUS_NG }, Formatting.None);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(jsonObj, System.Text.Encoding.UTF8, "application/json")
                };
            }

            // 店舗ID
            var shopNo = HttpContext.Current.Request.Form["ShopNO"];
            // APIキー
            var apiKey = HttpContext.Current.Request.Form["APIKey"];

            // 店舗IDが取得できない場合はエラー
            if (string.IsNullOrEmpty(shopNo) || string.IsNullOrEmpty(apiKey))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            // APIKey有効チェック
            if (!comm.ChkAPIKey(context, shopNo, apiKey))
            {
                // APIKeyの有効期限切れの場合
                jsonObj = JsonConvert.SerializeObject(new { Code = APIConstants.CODE_APIKEY_EXPIRED, Status = APIConstants.STATUS_NG }, Formatting.None);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(jsonObj, System.Text.Encoding.UTF8, "application/json")
                };
            }

            var masterFunction = new MasterFunction();
            string imageStoragePath = masterFunction.GetImageFolderName(context, shopNo);
            // 格納ディレクトリ
            var root = HostingEnvironment.MapPath(imageStoragePath);
            // ディレクトリ存在確認
            if (!Directory.Exists(root))
            {
                // ディレクトリ作成
                Directory.CreateDirectory(root);
            }

            // アップロード処理
            await this.Upload(HttpContext.Current.Request, root);

            return new HttpResponseMessage()
            {
                Content = new StringContent(jsonObj, System.Text.Encoding.UTF8, "application/json")
            };
        }

        /// <summary>
        /// ファイルアップロード処理呼出し
        /// </summary>
        /// <param name="request">Httpリクエスト</param>
        /// <param name="folderPath">格納フォルダパス</param>
        public async Task Upload(HttpRequest request, string folderPath)
        {
            await Task.Run(() =>
            {
                this.UploadAttachment(request, folderPath);
            });

            return;
        }

        /// <summary>
        /// ファイルアップロード処理
        /// </summary>
        /// <param name="request">Httpリクエスト</param>
        /// <param name="folderPath">格納フォルダパス</param>
        /// <returns>処理結果</returns>
        private bool UploadAttachment(HttpRequest request, string folderPath)
        {
            // ファイル指定がない場合
            if (request.Files.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(folderPath, fileName);
                // ファイルアップロード
                file.SaveAs(path);
            }
            return true;
        }

    }
}
