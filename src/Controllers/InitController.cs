using HACCPExtender.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class InitController : Controller
    {
        private MasterContext context = new MasterContext();

        public InitController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
            ViewBag.gamen = string.Empty;
        }

        /// <summary>
        /// HACCPExtenderの初期処理を行う。
        /// </summary>
        /// <param name="form">HttpRequest body</param>
        /// <returns>画面遷移。</returns>
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            Response.Headers["Access-Control-Allow-Origin"] = "*";

            // POST値
            string shopId = form["ExtenderShopID"];
            string groupId = form["ExtenderGroupID"];
            string categoryId = form["ExtenderCategoryID"];
            string locationId = form["ExtenderLocationID"];
            string reportId = form["ExtenderReportID"];

            // 店舗IDが取得できない場合はエラー
            if (string.IsNullOrEmpty(shopId))
            {
                // エラー画面へ遷移(認証失敗)
                Response.Redirect("/AuthError.html");
            }

            // 店舗マスタに存在確認
            var shopDt = from s in context.ShopMs
                         where s.SHOPID == shopId
                         select s;

            // 該当データが存在しない場合
            if (shopDt.Count() == 0 || shopDt.First() == null)
            {
                // エラー画面へ遷移(認証失敗)
                return Redirect("/AuthError.html");
            }
            else
            {
                // 店舗ID
                Session.Add("SHOPID", shopId);
                // 店舗名称
                Session.Add("SHOPNM", shopDt.First().SHOPNAME);
                // 契約ID
                Session.Add("GROUPID", groupId);
                // 業種IDが存在しない場合は初回ログイン
                if (string.IsNullOrEmpty(shopDt.First().INDUSTRYID))
                {
                    // 画面モード
                    Session.Add("DISPMODE", ManagerLoginMode.FIRST_LOGIN);
                    // 更新ユーザを設定
                    Session.Add("LOGINMNGID", USERNAME.FIRSTLOGIN_UPDID);
                    // 業種設定画面へ遷移
                    return RedirectToAction("Show", "IndustrySelection");

                } else
                {
                    // 管理者確認
                    var workerDt = from w in context.WorkerMs
                                   where w.SHOPID == shopId 
                                        && w.MANAGERKBN == BoolKbn.KBN_TRUE 
                                        && w.NODISPLAYKBN == BoolKbn.KBN_FALSE
                                   select w.WORKERID;

                    if (workerDt.Count() > 0)
                    {
                        // 画面モード(ログインなし)
                        Session.Add("DISPMODE", ManagerLoginMode.LOGIN_NONE);
                    } else
                    {
                        // 画面モード(管理者不在)
                        Session.Add("DISPMODE", ManagerLoginMode.NO_MANAGER);
                        // 更新ユーザを設定
                        Session.Add("LOGINMNGID", USERNAME.NOMANAGER_UPDID);
                    }
                }

                // 大分類IDが存在する場合は履歴画面へリダイレクト
                if (!string.IsNullOrEmpty(categoryId))
                {
                    // 大分類ID
                    Session.Add("CATEGORYID", categoryId);
                    // 中分類ID
                    Session.Add("LOCATIONID", locationId);
                    // 帳票ID
                    Session.Add("REPORTID", reportId);

                    // 履歴画面へ遷移
                    return RedirectToAction("LocalShow", "DataHistory");

                }
            }

            // 初回ログイン時以外はトップページへ遷移
            return RedirectToAction("Show", "Top");
        }

        /// <summary>
        /// URLから起動された場合の処理
        /// </summary>
        /// <returns>画面遷移</returns>
        [HttpGet]
        public ActionResult NotificationProcess()
        {
            Response.Headers["Access-Control-Allow-Origin"] = "*";

            // POST値
            var requestCount = Request.QueryString.Count;
            var keys = Request.QueryString.AllKeys;
            string mode = "";
            var keyDic = this.GetParamKey();

            foreach (var key in keys)
            {
                if (keyDic.ContainsKey(key))
                {
                    // セッションに格納
                    Session.Add(keyDic[key], Request.QueryString.Get(key));
                }
                else if (URLParameter.MODE.Equals(key))
                {
                    // 処理区分
                    mode = Request.QueryString.Get(key);
                } 
            }

            // 店舗ID
            string shopId = (string)Session["SHOPID"];

            if (!string.IsNullOrEmpty(shopId))
            {
                // 店舗IDから店舗名、契約IDをセッションに格納
                var shopDt = from s in context.ShopMs
                             where s.SHOPID == shopId
                             select s;

                if (shopDt.Count() == 0 || shopDt.First() == null)
                {
                    // 該当ページなし
                    return Redirect("/NotFound.html");
                }

                var shopM = shopDt.FirstOrDefault();
                Session.Add("SHOPNM", shopM.SHOPNAME);      // 店舗ID
                Session.Add("GROUPID", shopM.CONTRACTID);   // 契約ID

                string dispMode = (string)Session["DISPMODE"];

                // 既にブラウザにシステムが起動されている場合
                if (!string.IsNullOrEmpty(dispMode) && ManagerLoginMode.LOGIN_ALREADY.Equals(dispMode))
                {
                    // 処理区分から遷移先を決める
                    if (URLShoriKBN.DATAHISTORY.Equals(mode))
                    {
                        // データ履歴
                        return RedirectToAction("ToptoDataHistory", "DataHistory");
                    }
                    else if (URLShoriKBN.MIDDLE_APPROVAL.Equals(mode))
                    {
                        // 中分類承認
                        return RedirectToAction("Show", "MiddleApproval");
                    }
                    else if (URLShoriKBN.MAJOR_APPROVAL.Equals(mode))
                    {
                        // 大分類承認
                        return RedirectToAction("Show", "MajorApproval");
                    }
                    else if (URLShoriKBN.FACILITY_APPROVAL.Equals(mode))
                    {
                        // 施設承認
                        return RedirectToAction("Show", "FacilityApproval");
                    }

                    // 該当ページなし
                    return Redirect("/NotFound.html");
                } else
                {
                    // 管理者確認
                    var workerDt = from w in context.WorkerMs
                                   where w.SHOPID == shopId
                                        && w.MANAGERKBN == BoolKbn.KBN_TRUE
                                        && w.NODISPLAYKBN == BoolKbn.KBN_FALSE
                                   select w.WORKERID;

                    if (workerDt.Count() > 0)
                    {
                        // 画面モード(ログインなし)
                        Session.Add("DISPMODE", ManagerLoginMode.LOGIN_NONE);
                    }
                    else
                    {
                        // 画面モード(管理者不在)
                        Session.Add("DISPMODE", ManagerLoginMode.NO_MANAGER);
                        // 更新ユーザを設定
                        Session.Add("LOGINMNGID", USERNAME.NOMANAGER_UPDID);
                    }
                }

                // 処理区分から遷移先を決める
                if (URLShoriKBN.DATAHISTORY.Equals(mode))
                {
                    // データ履歴
                    return RedirectToAction("ToptoDataHistory", "DataHistory");
                }
                else if (URLShoriKBN.MIDDLE_APPROVAL.Equals(mode))
                {
                    // 管理者ログイン（中分類承認）
                    Session.Add("PENDINGNODEID", "1");
                    return RedirectToAction("Show", "Manager");
                }
                else if (URLShoriKBN.MAJOR_APPROVAL.Equals(mode))
                {
                    // 管理者ログイン（大分類承認）
                    Session.Add("PENDINGNODEID", "2");
                    return RedirectToAction("Show", "Manager");
                }
                else if (URLShoriKBN.FACILITY_APPROVAL.Equals(mode))
                {
                    // 管理者ログイン（施設承認）
                    Session.Add("PENDINGNODEID", "3");
                    return RedirectToAction("Show", "Manager");
                }
            }

            // 該当ページなし
            return Redirect("/NotFound.html");
        }

        /// <summary>
        /// パラメータキーとセッションキーマッピング
        /// </summary>
        /// <returns>マッピングディクショナリ</returns>
        private Dictionary<string, string> GetParamKey()
        {
            var sessionKeys = new Dictionary<string, string> ();
            sessionKeys.Add(URLParameter.SHOPID, "SHOPID");                     // 店舗ID
            sessionKeys.Add(URLParameter.CATEGORYID, "PENDINGCATEGORYID");      // 大分類ID
            sessionKeys.Add(URLParameter.LOCATIONID, "PENDINGLOCATIONID");      // 中分類ID
            sessionKeys.Add(URLParameter.REPORTID, "PENDINGREPORTID");          // 帳票ID
            sessionKeys.Add(URLParameter.PERIODID, "PENDINGPERIODID");          // 周期ID
            sessionKeys.Add(URLParameter.PERIODSTART, "PENDINGSTARTDATE");      // 周期開始日

            return sessionKeys;
        }

    }
}