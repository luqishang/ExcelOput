using HACCPExtender.Controllers.API;
using HACCPExtender.Controllers.Common;
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
using System.Web.Http;
using System.Web.Http.Cors;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace HACCPExtender.Controllers
{
    /// <summary>
    /// WebAPI マスタ連携
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Produces("application/json")]
    public class GetMstController : ApiController
    {
        private readonly MasterContext context = new MasterContext();
        private APICommonController comm = new APICommonController();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GetMstController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
        }

        /// <summary>
        /// マスタ更新日時連携
        /// </summary>
        /// <param name="form">HttpRequest body</param>
        /// <returns>マスタ更新日時(json)</returns>
        [Route("api/GetMstDate")]
        [HttpPost]
        public IHttpActionResult GetMstDate([FromBody] APIMst form)
        {
            try
            {
                // 店舗ID
                string shopId = form.ShopNO;
                // APIキー
                string APIKey = form.APIKey;
                // パラメータチェック
                if (string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(APIKey))
                {
                    return BadRequest();
                }

                // 返却値
                MstDateTimeResult result = new MstDateTimeResult
                {
                    Code = APIConstants.CODE_NG,
                    Status = APIConstants.STATUS_NG
                };

                // APIKey期限の確認
                if (!comm.ChkAPIKey(context, shopId, APIKey))
                {
                    result.Code = APIConstants.CODE_APIKEY_EXPIRED;
                    return Ok(result);
                }

                // マスタ更新日データ
                MstDateTimes mstData = new MstDateTimes();

                // 部門マスタ
                var categoryDt = from categoryM in context.CategoryMs
                                 where categoryM.SHOPID == shopId
                                 select categoryM.UPDDATE;
                if (categoryDt.Count() > 0)
                {
                    mstData.CategoryM = categoryDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 作業者マスタ
                var workDt = from workM in context.WorkerMs
                             where workM.SHOPID == shopId
                             select workM.UPDDATE;
                if (workDt.Count() > 0)
                {
                    mstData.WorkerM = workDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 場所マスタ
                var locationDt = from locationM in context.LocationMs
                                 where locationM.SHOPID == shopId
                                 select locationM.UPDDATE;
                if (locationDt.Count() > 0)
                {
                    mstData.LocationM = locationDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 設問マスタ
                var questionDt = from questionM in context.QuestionMs
                                 where questionM.SHOPID == shopId
                                    && questionM.DELETEFLAG == DeleteFlg.NODELETE
                                 select questionM.UPDDATE;
                if (questionDt.Count() > 0)
                {
                    mstData.QuestionM = questionDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 回答種類マスタ
                var answerTypeDt = from answerTypeM in context.AnswerTypeMs
                                   select answerTypeM.UPDDATE;
                var sanswerTypeDt = from sanswerTypeM in context.Shop_AnswerTypeMs
                                    where sanswerTypeM.SHOPID == shopId
                                    select sanswerTypeM.UPDDATE;
                if (answerTypeDt.Count() > 0 && sanswerTypeDt.Count() == 0)
                {
                    mstData.AnsTypeM = answerTypeDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                else if (answerTypeDt.Count() > 0 && sanswerTypeDt.Count() > 0)
                {
                    if (answerTypeDt.Max() >= sanswerTypeDt.Max())
                        mstData.AnsTypeM = answerTypeDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                    else
                        mstData.AnsTypeM = sanswerTypeDt.Max().ToString("yyyy/MM/dd HH:mm:ss");

                }
                // 機器マスタ
                var machineDt = from machineM in context.MachineMs
                                where machineM.SHOPID == shopId
                                select machineM.UPDDATE;
                if (machineDt.Count() > 0)
                {
                    mstData.MachineM = machineDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 管理対象マスタ(仕入先)
                var shiiresakiDt = from managementM in context.ManagementMs
                                   where managementM.SHOPID == shopId && managementM.MANAGEMENTID == ManagementID.SHIIRESAKI
                                   select managementM.UPDDATE;
                if (shiiresakiDt.Count() > 0)
                {
                    mstData.SupplierM = shiiresakiDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 管理対象マスタ(食材)
                var shokuzaiDt = from managementM in context.ManagementMs
                                 where managementM.SHOPID == shopId && managementM.MANAGEMENTID == ManagementID.SHOKUZAI
                                 select managementM.UPDDATE;
                if (shokuzaiDt.Count() > 0)
                {
                    mstData.FoodStuffM = shokuzaiDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 管理対象マスタ(料理)
                var ryoriDt = from managementM in context.ManagementMs
                              where managementM.SHOPID == shopId && managementM.MANAGEMENTID == ManagementID.RYORI
                              select managementM.UPDDATE;
                if (ryoriDt.Count() > 0)
                {
                    mstData.CuisineM = ryoriDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 管理対象マスタ(半製品)
                var hanseihinDt = from managementM in context.ManagementMs
                                  where managementM.SHOPID == shopId && managementM.MANAGEMENTID == ManagementID.HANSEIHIN
                                  select managementM.UPDDATE;
                if (hanseihinDt.Count() > 0)
                {
                    mstData.SemiFinProductM = hanseihinDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 管理対象マスタ(ユーザー)
                var userDt = from managementM in context.ManagementMs
                             where managementM.SHOPID == shopId && managementM.MANAGEMENTID == ManagementID.USERMST
                             select managementM.UPDDATE;
                if (userDt.Count() > 0)
                {
                    mstData.UserM = userDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 手引書マスタ
                var manualDt = from manualM in context.ManualMs
                               where manualM.SHOPID == shopId
                               select manualM.UPDDATE;
                if (manualDt.Count() > 0)
                {
                    mstData.ManualM = manualDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }
                // 帳票マスタ
                var reportDt = from reportM in context.ReportMs
                               where reportM.SHOPID == shopId
                               select reportM.UPDDATE;
                if (reportDt.Count() > 0)
                {
                    mstData.ReportM = reportDt.Max().ToString("yyyy/MM/dd HH:mm:ss");
                }

                // 返却値
                result.Code = APIConstants.CODE_OK;
                result.Status = APIConstants.STATUS_OK;
                result.data = mstData;

                return Ok(result);

            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// マスタデータ連携
        /// </summary>
        /// <param name="form">HttpRequest body</param>
        /// <returns>マスタデータ(json)</returns>
        [Route("api/GetMst")]
        [HttpPost]
        public HttpResponseMessage GetMst([FromBody] APIMst form)
        {
            try
            {
                // 店舗ID
                string shopId = form.ShopNO;
                // マスタID
                int MstId = form.MstId;
                // APIキー
                string APIKey = form.APIKey;
                // パラメータチェック
                if (string.IsNullOrEmpty(shopId) || MstId == 0 || string.IsNullOrEmpty(APIKey))
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                // result json
                object result = new { };

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

                // 指定マスタのデータ取得
                switch (MstId)
                {
                    case 1: // 部門マスタ
                        var categoryDt = from a in context.CategoryMs
                                         orderby a.CATEGORYID
                                         where a.SHOPID == shopId
                                         select a;

                        List<Category> categorylist = new List<Category>();

                        if (categoryDt.Count() > 0)
                        {
                            foreach (CategoryM dt in categoryDt)
                            {
                                Category cat = new Category
                                {
                                    CATEGORYID = dt.CATEGORYID,                 // 部門ID
                                    CATEGORYNAME = dt.CATEGORYNAME,             // 部門名称
                                    CATEGORYNAMEENG = dt.CATEGORYNAMEENG,       // 部門名称（英語名称）
                                };
                                categorylist.Add(cat);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "CategoryM", data = categorylist };

                        break;
                    case 2: // 作業者マスタ
                        var workerDt = from a in context.WorkerMs
                                       orderby a.WORKERID
                                       where a.SHOPID == shopId && a.NODISPLAYKBN == CommonConstants.BoolKbn.KBN_FALSE
                                       select a;

                        List<Worker> workerList = new List<Worker>();

                        if (workerDt.Count() > 0)
                        {
                            foreach (WorkerM dt in workerDt)
                            {
                                Worker work = new Worker
                                {
                                    WORKERID = dt.WORKERID,                     // 作業者ID
                                    WORKERNAME = dt.WORKERNAME,                 // 作業者名称
                                    MANAGERKBN = dt.MANAGERKBN,                 // 管理者区分
                                    CATEGORYKBN1 = dt.CATEGORYKBN1,             // 部門1区分
                                    CATEGORYKBN2 = dt.CATEGORYKBN2,             // 部門2区分
                                    CATEGORYKBN3 = dt.CATEGORYKBN3,             // 部門3区分
                                    CATEGORYKBN4 = dt.CATEGORYKBN4,             // 部門4区分
                                    CATEGORYKBN5 = dt.CATEGORYKBN5,             // 部門5区分
                                    CATEGORYKBN6 = dt.CATEGORYKBN6,             // 部門6区分
                                    CATEGORYKBN7 = dt.CATEGORYKBN7,             // 部門7区分
                                    CATEGORYKBN8 = dt.CATEGORYKBN8,             // 部門8区分
                                    CATEGORYKBN9 = dt.CATEGORYKBN9,             // 部門9区分
                                    CATEGORYKBN10 = dt.CATEGORYKBN10            // 部門10区分
                                };
                                workerList.Add(work);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "WorkerM", data = workerList };

                        break;
                    case 3: // 中分類マスタ
                        var locationDt = from a in context.LocationMs
                                         orderby a.LOCATIONID
                                         where a.SHOPID == shopId
                                         select a;

                        List<Location> locationList = new List<Location>();

                        if (locationDt.Count() > 0)
                        {
                            foreach (LocationM dt in locationDt)
                            {
                                Location location = new Location
                                {
                                    LOCATIONID = dt.LOCATIONID,             // 場所ID
                                    LOCATIONNAME = dt.LOCATIONNAME,         // 場所名称
                                    LOCATIONNAMEENG = dt.LOCATIONNAMEENG,   // 場所名称（英語表記）
                                    MANAGERKBN = dt.MANAGERKBN              // 管理者限定区分
                                };
                                locationList.Add(location);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "LocationM", data = locationList };

                        break;
                    case 4: // 設問マスタ
                        var questionDt = from a in context.QuestionMs
                                         orderby a.LOCATIONID
                                         where a.SHOPID == shopId && a.DELETEFLAG == CommonConstants.BoolKbn.KBN_FALSE
                                         select a;

                        List<Question> questionList = new List<Question>();

                        if (questionDt.Count() > 0)
                        {
                            foreach (QuestionM dt in questionDt)
                            {
                                Question question = new Question
                                {
                                    REPORTID = dt.REPORTID,                 // 帳票ID
                                    CATEGORYID = dt.CATEGORYID,             // 部門ID
                                    LOCATIONID = dt.LOCATIONID,             // 場所ID
                                    QUESTIONID = dt.QUESTIONID,             // 設問ID
                                    QUESTION = dt.QUESTION,                 // 設問
                                    QUESTIONENG = dt.QUESTIONENG,           // 設問（英語表記）
                                    ANSWERTYPEID = dt.ANSWERTYPEID,         // 回答種類ID
                                    NORMALCONDITION = dt.NORMALCONDITION,   // 正常結果条件
                                    NORMALCONDITION1 = dt.NORMALCONDITION1, // 正常結果条件値1
                                    NORMALCONDITION2 = dt.NORMALCONDITION2, // 正常結果条件値2
                                    DISPLAYNO = dt.DISPLAYNO                // 表示NO
                                };
                                questionList.Add(question);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "QuestionM", data = questionList };

                        break;
                    case 5: // 回答種類マスタ
                        var answerDt = from a in context.AnswerTypeMs
                                       orderby a.ANSWERTYPEID
                                       select a;
                        var shop_answerDt = from a in context.Shop_AnswerTypeMs
                                            where a.SHOPID == shopId
                                            orderby a.ANSWERTYPEID
                                            select a;

                        List<AnswerType> answerTypeList = new List<AnswerType>();

                        if (answerDt.Count() > 0)
                        {
                            foreach (AnswerTypeM dt in answerDt)
                            {
                                //AnswerTypeと被るShop_AnswerTypeの判定
                                var query = from a in shop_answerDt where a.ANSWERTYPEID == dt.ANSWERTYPEID select a;

                                if (query.FirstOrDefault() == null)
                                {
                                    answerTypeList.Add(dt.GetAnswerType());
                                }
                                else
                                {
                                    answerTypeList.Add(query.FirstOrDefault().GetAnswerType());
                                }

                            }
                        }
                        //AnswerTypeにないShop_AnswerTypeの追加
                        if (shop_answerDt.Count() > 0)
                        {
                            foreach (Shop_AnswerTypeM dt in shop_answerDt)
                            {
                                var query = from a in answerDt where a.ANSWERTYPEID == dt.ANSWERTYPEID select a;

                                if (query.FirstOrDefault() == null)
                                {
                                    answerTypeList.Add(dt.GetAnswerType());
                                }
                            }
                        }

                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "AnsTypeM", data = answerTypeList };

                        break;
                    case 6: // 機器マスタ
                        var machineDt = from a in context.MachineMs
                                        orderby a.MACHINEID
                                        where a.SHOPID == shopId
                                        select a;

                        List<Machine> machineList = new List<Machine>();

                        if (machineDt.Count() > 0)
                        {
                            foreach (MachineM dt in machineDt)
                            {
                                Machine machine = new Machine
                                {
                                    MACHINEID = dt.MACHINEID,      　// 機器ID
                                    MACHINENAME = dt.MACHINENAME,    // 機器名称
                                    LOCATIONID = dt.LOCATIONID   　  // 場所ID
                                };
                                machineList.Add(machine);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "MachineM", data = machineList };

                        break;
                    case 7: // 仕入先マスタ（管理対象マスタ）
                        var supplierDt = from a in context.ManagementMs
                                         orderby a.MANAGEMENTID, a.MANAGEID
                                         where a.SHOPID == shopId && a.MANAGEMENTID == ManagementID.SHIIRESAKI
                                         select a;

                        List<Supplier> SupplierList = new List<Supplier>();

                        if (supplierDt.Count() > 0)
                        {
                            foreach (ManagementM dt in supplierDt)
                            {
                                Supplier management = new Supplier
                                {
                                    MANAGEID = dt.MANAGEID,                 // 管理対象ID
                                    SUPPLIERNO = dt.MANAGENO,               // 仕入先番号
                                    SUPPLIERNAME = dt.MANAGENAME            // 仕入先名称
                                };
                                SupplierList.Add(management);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "SupplierM", data = SupplierList };

                        break;
                    case 8: // 食材マスタ（管理対象マスタ）
                        var FoodStuffDt = from a in context.ManagementMs
                                          orderby a.MANAGEMENTID, a.MANAGEID
                                          where a.SHOPID == shopId && a.MANAGEMENTID == ManagementID.SHOKUZAI
                                          select a;

                        List<FoodStuff> foodStuffList = new List<FoodStuff>();

                        if (FoodStuffDt.Count() > 0)
                        {
                            foreach (ManagementM dt in FoodStuffDt)
                            {
                                FoodStuff management = new FoodStuff
                                {
                                    MANAGEID = dt.MANAGEID,                // 管理対象ID
                                    FOODSTUFFNO = dt.MANAGENO,             // 食材番号
                                    FOODSTUFFNAME = dt.MANAGENAME,         // 食材名称
                                    UNIT = dt.UNIT,                        // 単位
                                    UPPERLIMIT = dt.UPPERLIMIT,            // 上限温度
                                    LOWERLIMIT = dt.LOWERLIMIT             // 下限温度
                                };
                                foodStuffList.Add(management);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "FoodStuffM", data = foodStuffList };

                        break;
                    case 9: // 料理マスタ（管理対象マスタ）
                        var CuisineDt = from a in context.ManagementMs
                                        orderby a.MANAGEMENTID, a.MANAGEID
                                        where a.SHOPID == shopId && a.MANAGEMENTID == ManagementID.RYORI
                                        select a;

                        List<Cuisine> cuisineList = new List<Cuisine>();

                        if (CuisineDt.Count() > 0)
                        {
                            foreach (ManagementM dt in CuisineDt)
                            {
                                Cuisine management = new Cuisine
                                {
                                    MANAGEID = dt.MANAGEID,                // 管理対象ID
                                    CUISINENO = dt.MANAGENO,               // 料理番号
                                    CUISINENAME = dt.MANAGENAME,           // 料理名称
                                    UNIT = dt.UNIT,                        // 単位
                                    UPPERLIMIT = dt.UPPERLIMIT,            // 上限温度
                                    LOWERLIMIT = dt.LOWERLIMIT,            // 下限温度
                                    LOCATIONID = dt.LOCATIONID             // 中分類ID 
                                };
                                cuisineList.Add(management);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "CuisineM", data = cuisineList };

                        break;
                    case 10: // 半製品マスタ（管理対象マスタ）
                        var SemiFinProductMDt = from a in context.ManagementMs
                                                orderby a.MANAGEMENTID, a.MANAGEID
                                                where a.SHOPID == shopId && a.MANAGEMENTID == ManagementID.HANSEIHIN
                                                select a;

                        List<SemiFinProduct> semiFinProductList = new List<SemiFinProduct>();

                        if (SemiFinProductMDt.Count() > 0)
                        {
                            foreach (ManagementM dt in SemiFinProductMDt)
                            {
                                SemiFinProduct management = new SemiFinProduct
                                {
                                    MANAGEID = dt.MANAGEID,                 // 管理対象ID
                                    SEMIFINPRODUCTNO = dt.MANAGENO,         // 半製品番号
                                    SEMIFINPRODUCTNAME = dt.MANAGENAME,     // 半製品名称
                                    UNIT = dt.UNIT,                         // 単位
                                    UPPERLIMIT = dt.UPPERLIMIT,             // 上限温度
                                    LOWERLIMIT = dt.LOWERLIMIT,             // 下限温度
                                    LOCATIONID = dt.LOCATIONID              // 中分類ID 
                                };
                                semiFinProductList.Add(management);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "SemiFinProductM", data = semiFinProductList };

                        break;
                    case 11: // ユーザーマスタ（管理対象マスタ）
                        var userDt = from a in context.ManagementMs
                                     orderby a.MANAGEMENTID, a.MANAGEID
                                     where a.SHOPID == shopId && a.MANAGEMENTID == ManagementID.USERMST
                                     select a;

                        List<User> userList = new List<User>();

                        if (userDt.Count() > 0)
                        {
                            foreach (ManagementM dt in userDt)
                            {
                                User management = new User
                                {
                                    MANAGEID = dt.MANAGEID,                 // 管理対象ID
                                    USERNO = dt.MANAGENO,                   // ユーザー番号
                                    USERNAME = dt.MANAGENAME,               // ユーザー名称
                                    UNIT = dt.UNIT,                         // 単位
                                    UPPERLIMIT = dt.UPPERLIMIT,             // 上限温度
                                    LOWERLIMIT = dt.LOWERLIMIT,             // 下限温度
                                    LOCATIONID = dt.LOCATIONID              // 中分類ID 
                                };
                                userList.Add(management);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "UserM", data = userList };

                        break;
                    case 12: // 手引書マスタ
                        var manualDt = from a in context.ManualMs
                                       orderby a.MANUALID
                                       where a.SHOPID == shopId
                                       select a;

                        List<Manual> manualList = new List<Manual>();

                        if (manualDt.Count() > 0)
                        {
                            var GeneralDt = from ge in context.GeneralPurposeMs
                                            where ge.KEY == EnvironmentKey.KEY_HOSTNAME
                                            select ge;
                            string root = string.Empty;

                            if (GeneralDt.Count() > 0 && GeneralDt.FirstOrDefault() != null)
                            {
                                root = GeneralDt.FirstOrDefault().VALUE1;
                            }

                            var masterF = new MasterFunction();
                            string folderNm = masterF.GetManualFolderName(context, shopId).Replace("~/", "");

                            foreach (ManualM dt in manualDt)
                            {
                                Manual manual = new Manual
                                {
                                    ManualID = dt.MANUALID,                     // 手引書ID
                                    UploadDate = dt.UPLOADDATE,                 // アップロード日時
                                    ManualName = dt.MANUALNAME,                 // 手引書名称
                                    ManualPath = root + "/" + folderNm + "/" + dt.MANUALPATH     // 手引書格納パス
                                };
                                manualList.Add(manual);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "ManualM", data = manualList };

                        break;
                    case 13: // 帳票マスタ
                        var reportDt = from a in context.ReportMs
                                       orderby a.CATEGORYID, a.LOCATIONID, a.REPORTID
                                       where a.SHOPID == shopId
                                       select a;

                        List<Report> reportList = new List<Report>();

                        if (reportDt.Count() > 0)
                        {
                            foreach (ReportM dt in reportDt)
                            {
                                Report report = new Report
                                {
                                    REPORTID = dt.REPORTID,                     // 帳票ID
                                    REPORTNAME = dt.REPORTNAME,                 // 手引書名称
                                    CATEGORYID = dt.CATEGORYID,                 // 手引書名称
                                    LOCATIONID = dt.LOCATIONID,                 // 手引書名称
                                    PERIOD = dt.PERIOD,                 // 手引書名称
                                    BASEMONTH = dt.BASEMONTH,                 // 手引書名称
                                    REFERENCEDATE = dt.REFERENCEDATE,                 // 手引書名称
                                    DISPLAYNO = dt.DISPLAYNO                 // 手引書名称
                                };
                                reportList.Add(report);
                            }
                        }
                        result = new { Code = APIConstants.CODE_OK, Status = APIConstants.STATUS_OK, master = "ReportM", data = reportList };

                        break;
                    default:
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
                string jsonObj = JsonConvert.SerializeObject(result, Formatting.None);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(jsonObj, System.Text.Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
    }
}
