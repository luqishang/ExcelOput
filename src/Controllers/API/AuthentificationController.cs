using HACCPExtender.Business;
using HACCPExtender.Models;
using HACCPExtender.Models.API;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;
using HACCPExtender.Controllers.Common;

namespace HACCPExtender.Controllers.API
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Produces("application/json")]
    public class AuthentificationController : ApiController
    {
        private MasterContext context = new MasterContext();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AuthentificationController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
        }

        /// <summary>
        /// ライセンス端末確認
        /// </summary>
        /// <param name="form">HttpRequest body</param>
        /// <returns>認証結果</returns>
        [Route("api/License")]
        [HttpPost]
        public IHttpActionResult LicenseAuthentification([FromBody] APIAuth form)
        {
            try
            {
                // 返却値設定
                APIAuthResult pIAuthResult = new APIAuthResult
                {
                    Code = APIConstants.CODE_NG,
                    Status = APIConstants.STATUS_NG
                };
                // 店舗ID
                string shopId = form.ShopNo;
                // GUID
                string guId = form.GUID;
                // パラメータチェック
                if (string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(guId))
                {
                    return BadRequest();
                }

                // モバイル端末情報
                var mobileDt = from mobile in context.MobileTs
                               where mobile.SHOPID == shopId && mobile.GUID == guId
                               select mobile;

                if (mobileDt.Count() > 0)
                {
                    int mobileNo = mobileDt.FirstOrDefault().TERMINALNO;
                    // APIキーの発行
                    string APIKey = this.IssueAPIKey();

                    // 端末情報を更新
                    MobileT mobile = mobileDt.FirstOrDefault();
                    // APIKey
                    mobile.APIKEY = APIKey;
                    // 有効期限
                    mobile.EXPIRATION = this.GetDeadLine();
                    // 更新ユーザーID
                    mobile.UPDUSERID = APIConstants.API_USER;

                    using (context = new MasterContext())
                    {
                        using (var tran = context.Database.BeginTransaction())
                        {
                            try
                            {
                                // 端末情報を更新
                                context.MobileTs.Attach(mobile);
                                context.Entry(mobile).State = EntityState.Modified;
                                context.SaveChanges();
                                tran.Commit();

                                // 返却項目
                                pIAuthResult = new APIAuthResult
                                {
                                    Code = APIConstants.CODE_OK,
                                    Status = APIConstants.STATUS_OK,
                                    ShopNO = shopId,
                                    ShopName = this.GetShopName(shopId),
                                    APIKey = mobileNo.ToString() + "-" + APIKey
                                };

                                return Ok(pIAuthResult);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                // ロールバック
                                tran.Rollback();
                                return InternalServerError();
                            }
                            catch (Exception ex)
                            {
                                // ロールバック
                                tran.Rollback();
                                LogHelper.Default.WriteError(ex.Message, ex);
                                return InternalServerError();
                            }
                        }
                    }
                } else
                {
                    return Ok(pIAuthResult);
                }
            }
            catch
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// ライセンス端末登録
        /// </summary>
        /// <param name="form">HttpRequest body</param>
        /// <returns>認証結果</returns>
        [Route("api/LicenseRegist")]
        [HttpPost]
        public IHttpActionResult LicenseRegist([FromBody] APIAuth form)
        {
            try
            {
                // 返却値設定
                APIAuthResult pIAuthResult = new APIAuthResult
                {
                    Code = APIConstants.CODE_NG,
                    Status = APIConstants.STATUS_NG
                };
                // 店舗ID
                string shopId = form.ShopNo;
                // ライセンスキー
                string licensekey = form.LicenseKey;
                // シリアル番号
                string guId = form.GUID;
                // パラメータチェック
                if (string.IsNullOrEmpty(shopId) || string.IsNullOrEmpty(licensekey) || string.IsNullOrEmpty(guId))
                {
                    return Ok(pIAuthResult);
                }
                // ライセンスマスタ
                var licenseDt = from license in context.LicenseMs
                                where license.SHOPID == shopId && license.LICENSEKEY == licensekey
                                select license.LICENSECONTRACT;
                if (licenseDt.Count() > 0)
                {
                    // ライセンス数
                    int licenseCnt = licenseDt.First();

                    // モバイル端末情報
                    var mobileDt = from mobile in context.MobileTs
                                   where mobile.SHOPID == shopId
                                   select mobile;

                    // ライセンス数がある場合
                    if (licenseCnt > mobileDt.Count())
                    {
                        using (context = new MasterContext())
                        {
                            using (var tran = context.Database.BeginTransaction())
                            {
                                try
                                {
                                    int mobileNo = 1;
                                    if (mobileDt.Count() > 0)
                                    {
                                        var maxcnt = mobileDt.OrderByDescending(a => a.TERMINALNO).FirstOrDefault().TERMINALNO;
                                        mobileNo += maxcnt;
                                    }
                                    
                                    // APIキーの発行
                                    string APIKey = this.IssueAPIKey();

                                    // モバイル端末情報の登録
                                    context.MobileTs.Add(new MobileT
                                    {
                                        SHOPID = shopId,
                                        TERMINALNO = (short)mobileNo,
                                        GUID = guId,
                                        APIKEY = APIKey,
                                        EXPIRATION = this.GetDeadLine(),
                                        INSUSERID = APIConstants.API_USER,
                                        UPDUSERID = APIConstants.API_USER
                                    });
                                    context.SaveChanges();
                                    tran.Commit();

                                    // 返却項目
                                    pIAuthResult = new APIAuthResult
                                    {
                                        Code = APIConstants.CODE_OK,
                                        Status = APIConstants.STATUS_OK,
                                        ShopNO = shopId,
                                        ShopName = this.GetShopName(shopId),
                                        APIKey = mobileNo.ToString() + "-" + APIKey
                                    };

                                    return Ok(pIAuthResult);
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    // ロールバック
                                    tran.Rollback();
                                    return InternalServerError();
                                }
                                catch (Exception ex)
                                {
                                    // ロールバック
                                    tran.Rollback();
                                    LogHelper.Default.WriteError(ex.Message, ex);
                                    return InternalServerError();
                                }
                            }
                        }
                    } else
                    {
                        // ライセンス数オーバー
                        pIAuthResult.Code = APIConstants.CODE_LICENSE_OVER;
                        return Ok(pIAuthResult);
                    }
                }
                else
                {
                    // ライセンス数なし
                    pIAuthResult.Code = APIConstants.CODE_LICENSE_OVER;
                    return Ok(pIAuthResult);
                }
            } catch
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// APIキー生成
        /// </summary>
        /// <returns>APIキー</returns>
        private string IssueAPIKey()
        {
            const string keyChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; 
            int digit = int.Parse(GetAppSet.GetAppSetValue("APIKey", "Digit"));

            StringBuilder sb = new StringBuilder(digit);
            Random r = new Random();

            for (int i = 0; i < digit; i++)
            {
                // 文字位置をランダムに選択
                int pos = r.Next(keyChars.Length);
                // 選択した文字位置の文字を取得
                char c = keyChars[pos];
                // パスワードに追加
                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// APIキーの期限取得
        /// </summary>
        /// <returns>期限日時(YYYYMMDDHHMM)</returns>
        private string GetDeadLine()
        {
            // APIキーの期限を取得（分）
            int deadlineMinutes = int.Parse(GetAppSet.GetAppSetValue("APIKey", "DeadlineMinutes"));

            // APIキー期限を設定
            return DateTime.Now.AddMinutes(deadlineMinutes).ToString("yyyyMMddHHmm");
        }

        /// <summary>
        /// 店舗名称取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <returns>店舗名称</returns>
        private string GetShopName(string shopId)
        {
            string shopName = string.Empty;
            var shopDt = from s in context.ShopMs
                         where s.SHOPID == shopId
                         select s;
            if (shopDt != null && shopDt.Count() > 0)
            {
                shopName = shopDt.FirstOrDefault().SHOPNAME;
            }

            return shopName;
        }
    }
}
