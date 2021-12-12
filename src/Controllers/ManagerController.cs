using HACCPExtender.Business;
using HACCPExtender.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class ManagerController : Controller
    {
        private readonly MasterContext context = new MasterContext();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ManagerController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
        }

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="requestContext">リクエスト</param>
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            // 画面説明ファイルURL取得
            string strPathAndQuery = requestContext.HttpContext.Request.Url.AbsoluteUri.Replace(requestContext.HttpContext.Request.Url.AbsolutePath, "/");
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "Manager");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 管理者ログイン画面表示
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpGet]
        public ActionResult Show()
        {
            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // データ取得
            IEnumerable<SelectListItem> doropList = this.GetBManagerDropList(shopId, string.Empty);
            ViewBag.ServerOptions = doropList;
            ViewBag.disabled = string.Empty;

            var keys = Request.Params["menu"];
            // ヘッダーのリンクからきた場合
            if (!string.IsNullOrEmpty(keys) && "1".Equals(keys))
            {
                // 承認ページ遷移情報をセッションから削除
                Session.Remove("PENDINGNODEID");
                Session.Remove("PENDINGCATEGORYID");
                Session.Remove("PENDINGLOCATIONID");
                Session.Remove("PENDINGREPORTID");
                Session.Remove("PENDINGPERIODID");
                Session.Remove("PENDINGSTARTDATE");
            }

            // 0件の場合は警告を出す
            if (doropList.Count() == 0)
            {
                ViewBag.disabled = "disabled";
                ModelState.AddModelError(string.Empty, "作業者マスタで管理者を登録してください。");
            }

            return View();
        }

        /// <summary>
        /// 管理者ログイン
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Signin(FormCollection form)
        {
            // 画面入力値
            string workerId = form["WorkerId"];
            string loginId = form["loginId"];
            string passWord = form["passWord"];
            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            
            // バリデーション
            if (!ChkInput(workerId, loginId, passWord))
            {
                // バリデーションエラーの場合
                ViewBag.ServerOptions = this.GetBManagerDropList(shopId, workerId);
                ViewBag.snnId = loginId;
                ViewBag.disabled = string.Empty;
                return View("Show");
            }

            // 認証処理
            var authenticationDt = from worker in context.WorkerMs
                             where worker.SHOPID == shopId 
                                && worker.WORKERID == workerId
                                && worker.MANAGERKBN == BoolKbn.KBN_TRUE 
                                && worker.NODISPLAYKBN == BoolKbn.KBN_FALSE
                                && worker.APPID == loginId 
                                && worker.APPPASS == passWord
                             select worker;

            if (authenticationDt.Count() == 0)
            {
                // 認証失敗の場合
                ModelState.AddModelError(string.Empty, "承認IDまたはパスワードに誤りがあるか、登録されていません。");

                ViewBag.ServerOptions = this.GetBManagerDropList(shopId, workerId);
                ViewBag.snnId = loginId;
                ViewBag.disabled = string.Empty;
                return View("Show");
            } else
            {
                // 認証成功の場合
                // セッションに値を設定
                // ログイン管理者ID
                Session.Add("LOGINMNGID", authenticationDt.FirstOrDefault().WORKERID);
                // ログイン管理者名
                Session.Add("LOGINMNGNM", authenticationDt.FirstOrDefault().WORKERNAME);
                // 画面モード
                Session.Add("DISPMODE", ManagerLoginMode.LOGIN_ALREADY);
            }

            // 承認待ちデータが選択されていた場合はデータ承認へ遷移する
            string nodeClass = (string)Session["PENDINGNODEID"];
            if (!string.IsNullOrEmpty(nodeClass))
            {
                if (APPROVALLEVEL.MIDDLE.Equals(nodeClass))
                {
                    return RedirectToAction("Show", "MiddleApproval");

                } else if (APPROVALLEVEL.MAJORE.Equals(nodeClass))
                {
                    return RedirectToAction("Show", "MajorApproval");

                } else if (APPROVALLEVEL.FACILITY.Equals(nodeClass))
                {
                    return RedirectToAction("Show", "FacilityApproval");
                }
            }

            // TOP画面へ遷移する
            return RedirectToAction("Show", "Top");
        }

        /// <summary>
        /// 管理者ログアウト
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpGet]
        public ActionResult Signout()
        {
            // セッションに値を設定
            // ログイン管理者ID
            Session.Remove("LOGINMNGID");
            // ログイン管理者名
            Session.Remove("LOGINMNGNM");
            // 画面モード
            Session.Add("DISPMODE", ManagerLoginMode.LOGIN_NONE);

            // TOP画面へ遷移する
            return RedirectToAction("Show", "Top");
        }


        /// <summary>
        /// 管理作業者リスト取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="workerId">作業者ID</param>
        /// <returns>ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetBManagerDropList(string shopId, string workerId)
        {
            return context.WorkerMs.Where(s => s.SHOPID == shopId && s.MANAGERKBN == BoolKbn.KBN_TRUE && s.NODISPLAYKBN == BoolKbn.KBN_FALSE).Select(s => new SelectListItem
            {
                Text = s.WORKERNAME,
                Value = s.WORKERID,
                Selected = s.WORKERID == workerId
            });
        }

        /// <summary>
        /// 管理者認証処理
        /// </summary>
        /// <param name="workerId">作業者ID</param>
        /// <param name="loginId">管理者ログインID</param>
        /// <param name="passWord">管理者パスワード</param>
        /// <returns>判定結果</returns>
        private bool ChkInput(string workerId, string loginId, string passWord)
        {
            bool validFlg = true;
            if (string.IsNullOrEmpty(workerId))
            {
                ModelState.AddModelError("WorkerId", string.Empty);
                ModelState.AddModelError(string.Empty, "管理作業者を選択してください");
                validFlg = false;
            }
            if (string.IsNullOrEmpty(loginId))
            {
                ModelState.AddModelError("loginId", string.Empty);
                ModelState.AddModelError(string.Empty, "承認IDを入力してください");
                validFlg = false;
            }
            if (string.IsNullOrEmpty(passWord))
            {
                ModelState.AddModelError("passWord", string.Empty);
                ModelState.AddModelError(string.Empty, "承認パスワードを入力してください");
                validFlg = false;
            }

            return validFlg;
        }
    }
}