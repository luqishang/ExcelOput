using HACCPExtender.Models;
using HACCPExtender.Models.API;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using static HACCPExtender.Controllers.Common.CommonConstants;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace HACCPExtender.Controllers.API
{
    /// <summary>
    /// WebAPI マスタ連携
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Produces("application/json")]
    public class GetDataLinkageController : ApiController
    {
        private MasterContext context = new MasterContext();
        private APICommonController comm = new APICommonController();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GetDataLinkageController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
        }

        /// <summary>
        /// 最新記録データ取得
        /// </summary>
        /// <param name="form">HttpRequest body</param>
        /// <returns>マスタデータ(json)</returns>
        [Route("api/GetLatestDateRecord")]
        [HttpPost]
        public HttpResponseMessage GetLatestDateRecord([FromBody] LatestRecord form)
        {
            // 店舗ID
            string shopId = form.ShopNO;
            // APIキー
            string APIKey = form.APIKey;
            // 大分類ID
            string categoryId = form.CategoryID;
            // 中分類ID
            string locationId = form.LocationID;
            // 帳票ID
            string reportId = form.ReportID;

            // パラメータチェック
            if (string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(APIKey) || string.IsNullOrEmpty(reportId))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            // result json
            object result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK };

            // APIKey期限の確認
            if (!comm.ChkAPIKey(context, shopId, APIKey))
            {
                // 期限切れの場合
                result = new { Code = APIConstants.CODE_APIKEY_EXPIRED, Status = APIConstants.STATUS_NG };
                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(result, Formatting.None), System.Text.Encoding.UTF8, "application/json")
                };
            }

            // 帳票テンプレートマスタからマージ単位を取得
            string margeUnit = string.Empty;
            var reportDt = from re in context.ReportMs
                           where re.SHOPID == shopId
                                && re.REPORTID == reportId
                           select re;
            if (reportDt.Count() > 0 && reportDt.FirstOrDefault() != null)
            {
                string templateId = reportDt.FirstOrDefault().REPORTTEMPLATEID;

                var templateDt = from temp in context.ReportTemplateMs
                                 where temp.SHOPID == shopId
                                        && temp.TEMPLATEID == templateId
                                 select temp;
                if (templateDt.Count() > 0 && templateDt.FirstOrDefault() != null)
                {
                    margeUnit = templateDt.FirstOrDefault().MERGEUNIT;
                }
            }

            if (ReportMergeUnit.UNIT_WORKER.Equals(margeUnit))
            {
                // 担当者単位の場合
                var personInCharge = this.GetPersonInChargeData(shopId, categoryId, locationId, reportId);

                if (personInCharge.Count() > 0)
                {
                    result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, data = personInCharge };
                }
            }
            else
            {
                // 中分類・大分類単位の場合
                var tempereDt = from tempe in context.TemperatureControlTs
                                where tempe.SHOPID == shopId
                                        && tempe.CATEGORYID == categoryId
                                        && tempe.LOCATIONID == locationId
                                        && tempe.REPORTID == reportId
                                select tempe.DATAYMD;

                if (tempereDt.Count() > 0 && tempereDt.FirstOrDefault() != null)
                {
                    var data = new LatestDateRecord
                    {
                        CATEGORYID = categoryId,
                        LOCATIONID = locationId,
                        REPORTID = reportId,
                        DATAYMD = (DateTime.ParseExact(tempereDt.Max(), "yyyyMMddHHmmss", null)).ToString("yyyy/MM/dd HH:mm:ss")
                    };
                    var dataList = new List<LatestDateRecord>();
                    dataList.Add(data);

                    result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, data = dataList };
                }
            }

            string jsonObj = JsonConvert.SerializeObject(result, Formatting.None);
            return new HttpResponseMessage()
            {
                Content = new StringContent(jsonObj, Encoding.UTF8, "application/json")
            };
        }

        /// <summary>
        /// 作業者単位の最新データ
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <returns>作業者単位の最新データ履歴</returns>
        private List<LatestDateRecord> GetPersonInChargeData(string shopId, string categoryId, string locationId, string reportId)
        {
            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("TEMPE.SHOPID ");
            sql.Append(", TEMPE.CATEGORYID ");
            sql.Append(", TEMPE.LOCATIONID ");
            sql.Append(", TEMPE.REPORTID ");
            sql.Append(", TO_CHAR(TO_DATE(MAX(TEMPE.DATAYMD),'YYYY/MM/DD HH24:MI:SS'), 'YYYY/MM/DD HH24:MI:SS') AS DATAYMD ");
            sql.Append(", TEMPE.WORKERID ");
            sql.Append(", TEMPE.WORKERNAME ");
            sql.Append("FROM ");
            sql.Append("TEMPERATURECONTROL_T TEMPE ");
            sql.Append("WHERE ");
            sql.Append("TEMPE.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND TEMPE.CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("AND TEMPE.LOCATIONID = '");
            sql.Append(locationId);
            sql.Append("' ");
            sql.Append("AND TEMPE.REPORTID = '");
            sql.Append(reportId);
            sql.Append("' ");
            sql.Append("GROUP BY ");
            sql.Append("SHOPID, CATEGORYID, LOCATIONID, REPORTID, WORKERID, WORKERNAME ");
            var person = context.Database.SqlQuery<LatestDateRecord>(sql.ToString());
            if (person.Count() > 0)
            {
                return person.ToList();
            }

            return new List<LatestDateRecord>();
        }
    }
}
