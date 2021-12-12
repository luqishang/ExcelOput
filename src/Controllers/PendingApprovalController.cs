using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class PendingApprovalController : Controller
    {
        private readonly MasterContext context = new MasterContext();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PendingApprovalController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "PendingApproval");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 画面表示
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpGet]
        public ActionResult Show()
        {
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // セッションから削除
            Session.Remove("PENDINGNODEID");
            Session.Remove("PENDINGCATEGORYID");
            Session.Remove("PENDINGLOCATIONID");
            Session.Remove("PENDINGREPORTID");
            Session.Remove("PENDINGPERIODID");
            Session.Remove("PENDINGSTARTDATE");

            // 管理者ログイン有無
            bool managerFlg = false;
            // 中分類担当
            bool middleFlg = false;
            // 大分類担当
            bool majorFlg = false;
            // 施設承認担当
            bool facilityFlg = false;
            // 管理者ID
            string loginUserId = (string)Session["LOGINMNGID"];
            // 承認者不在モードの場合
            if (ManagerLoginMode.NO_MANAGER.Equals(editMode))
            {
                loginUserId = string.Empty;
            }

            if (!string.IsNullOrEmpty(loginUserId))
            {
                // 承認経路マスタからデータを取得
                var manager = from sn in context.ApprovalRouteMs
                                 where sn.SHOPID == shopId && sn.APPMANAGERID == loginUserId
                                 select sn;

                if (manager.Count() > 0)
                {
                    managerFlg = true;
                    if (manager.Where(a => a.APPROVALORDERCLASS == APPROVALLEVEL.MIDDLE).Count() > 0) middleFlg = true;
                    if (manager.Where(a => a.APPROVALORDERCLASS == APPROVALLEVEL.MAJORE).Count() > 0) majorFlg = true;
                    if (manager.Where(a => a.APPROVALORDERCLASS == APPROVALLEVEL.FACILITY).Count() > 0) facilityFlg = true;
                }
            }

            // 画面用承認データ
            var pendingData = new PendingApprovalData();
            var tranfunction = new TransactionFunction(context);

            // 中分類承認
            // 管理者ログインありで中分類承認担当がない場合
            if (managerFlg && !middleFlg)
            {
                // 表示データなし
                pendingData.MiddleDatas = new List<MiddleData>();
            }
            else
            {
                pendingData.MiddleDatas = tranfunction.GetMiddleApprovalData(shopId, loginUserId);
            }

            // 大分類承認
            // 管理者ログインありで大分類承認担当がない場合
            if (managerFlg && !majorFlg)
            {
                // 表示データなし
                pendingData.MajorDatas = new List<MajorData>();
            }
            else
            {
                pendingData.MajorDatas = tranfunction.GetMajorData(shopId, loginUserId);
            }

            // 施設承認
            // 管理者ログインありで施設承認担当がない場合
            if (managerFlg && !facilityFlg)
            {
                // 表示データなし
                pendingData.FacilityDatas = new List<FacilityData>();
            } else
            {
                pendingData.FacilityDatas = tranfunction.GetFacilityData(shopId, loginUserId);
            }

            return View(pendingData);
        }

        /// <summary>
        /// データ承認遷移
        /// </summary>
        /// <param name="form"></param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Approval(FormCollection form)
        {
            string bunrui = form["sel_Bunrui"];
            bool paramChk = false;

            if (APPROVALLEVEL.MIDDLE.Equals(bunrui)
                && !string.IsNullOrEmpty(form["sel_CategoryId"])
                && !string.IsNullOrEmpty(form["sel_LocationId"])
                && !string.IsNullOrEmpty(form["sel_ReportId"])
                && !string.IsNullOrEmpty(form["sel_Period"])
                && !string.IsNullOrEmpty(form["sel_StartDate"]))
            {
                // 中分類承認
                paramChk = true;
            }
            else if (APPROVALLEVEL.MAJORE.Equals(bunrui)
                && !string.IsNullOrEmpty(form["sel_CategoryId"])
                && !string.IsNullOrEmpty(form["sel_Period"])
                && !string.IsNullOrEmpty(form["sel_StartDate"]))
            {
                // 大分類承認
                paramChk = true;
            }
            else if (APPROVALLEVEL.FACILITY.Equals(bunrui)
                && !string.IsNullOrEmpty(form["sel_Period"])
                && !string.IsNullOrEmpty(form["sel_StartDate"]))
            {
                // 施設承認
                paramChk = true;
            }

            if (!paramChk)
            {
                throw new ApplicationException();
            }

            // セッションにセット
            Session.Add("PENDINGNODEID", form["sel_Bunrui"]);
            Session.Add("PENDINGCATEGORYID", form["sel_CategoryId"]);
            Session.Add("PENDINGLOCATIONID", form["sel_LocationId"]);
            Session.Add("PENDINGREPORTID", form["sel_ReportId"]);
            Session.Add("PENDINGPERIODID", form["sel_Period"]);
            Session.Add("PENDINGSTARTDATE", form["sel_StartDate"]);

            // 管理者ログイン済みの場合
            if (ManagerLoginMode.LOGIN_ALREADY.Equals(Session["DISPMODE"]))
            {
                // データ承認へ遷移
                if (APPROVALLEVEL.MIDDLE.Equals(bunrui))
                {
                    // 管理者ログインへ遷移
                    return RedirectToAction("Show", "MiddleApproval");
                }
                else if (APPROVALLEVEL.MAJORE.Equals(bunrui))
                {
                    // 管理者ログインへ遷移
                    return RedirectToAction("Show", "MajorApproval");
                }
                else if (APPROVALLEVEL.FACILITY.Equals(bunrui))
                {
                    // 管理者ログインへ遷移
                    return RedirectToAction("Show", "FacilityApproval");
                }
            } else
            {
                return RedirectToAction("Show", "Manager");
            }

            // トップページへ遷移
            return RedirectToAction("Show", "Top");
        }
    }
}
