using HACCPExtender.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class StubController : Controller
    {
        private readonly MasterContext context = new MasterContext();

        public StubController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
        }

        // GET: Stub
        public ActionResult Index()
        {
            // 画面編集モード
            ViewBag.DisplayOptions = this.GetDisplayDropList(string.Empty);

            // 店舗データ取得
            ViewBag.ShopOptions = this.GetShopMDropList(string.Empty);

            // 承認者データ取得
            ViewBag.SnnOptions = new SelectListItem[] { };

            // 設定ファイルの読み込み方法
            //string configval = GetAppSet.GetAppSetValue("mail", "smtp");

            return View();
        }

        [HttpPost]
        public ActionResult ShopChange(FormCollection form)
        {
            string shopId = form["ShopDrop"];
            string dispMode = form["DisplayDrop"];

            // 画面編集モード
            ViewBag.DisplayOptions = this.GetDisplayDropList(dispMode);
            // 店舗ドロップ
            ViewBag.ShopOptions = this.GetShopMDropList(shopId);
            // 承認者ドロップ
            ViewBag.SnnOptions = GetManagerDropList(shopId, string.Empty);

            return View("Index");
        }

        [HttpPost]
        public ActionResult kick(FormCollection form)
        {
            string dispMode = form["DisplayDrop"];
            string shopId = form["ShopDrop"];
            string manager = form["ManagerDrop"];

            bool validFlg = true;
            if (string.IsNullOrEmpty(dispMode))
            {
                ModelState.AddModelError("DisplayDrop", string.Empty);
                ModelState.AddModelError(string.Empty, "画面編集モードを選択してください");
                validFlg = false;
            }
            if (string.IsNullOrEmpty(shopId))
            {
                ModelState.AddModelError("ShopDrop", string.Empty);
                ModelState.AddModelError(string.Empty, "店舗IDを選択してください");
                validFlg = false;
            }
            if (dispMode == "1" && string.IsNullOrEmpty(manager))
            {
                ModelState.AddModelError("ManagerDrop", string.Empty);
                ModelState.AddModelError(string.Empty, "管理作業者を選択してください");
                validFlg = false;
            }

            if (!validFlg)
            {
                // 画面編集モード
                ViewBag.DisplayOptions = this.GetDisplayDropList(dispMode);
                // 店舗ドロップ
                ViewBag.ShopOptions = this.GetShopMDropList(shopId);
                // 承認者ドロップ
                ViewBag.SnnOptions = this.GetManagerDropList(shopId, manager);

                return View("Index");
            }

            // 店舗名称
            var shopDt = from s in context.ShopMs
                         where s.SHOPID == shopId
                         select s;

            Session.Add("SHOPID", shopId);
            Session.Add("SHOPNM", shopDt.FirstOrDefault().SHOPNAME);
            Session.Add("GROUPID", shopDt.FirstOrDefault().CONTRACTID);
            Session.Add("DISPMODE", dispMode);

            // 初回ログインの場合
            if ("3".Equals(dispMode))
            {
                // 業務設定画面へ遷移する予定
                Session.Add("LOGINMNGID", USERNAME.FIRSTLOGIN_UPDID);

                return RedirectToAction("Show", "IndustrySelection");
            }
            else if ("2".Equals(dispMode))
            {
                // 管理者不在
                Session.Add("LOGINMNGID", USERNAME.NOMANAGER_UPDID);
            }
            else
            {
                // 管理者名
                var work = from w in context.WorkerMs
                           where w.SHOPID == shopId && w.WORKERID == manager
                           select w.WORKERNAME;

                // 条件値の設定
                Session.Add("LOGINMNGID", manager);
                Session.Add("LOGINMNGNM", work.FirstOrDefault());
            }            

            return RedirectToAction("Show", "Top");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetDisplayDropList(string mode)
        {
            return new SelectList(
                        new SelectListItem[] {
                            new SelectListItem() { Value="0", Text="管理者未ログイン（編集不可）" },
                            new SelectListItem() { Value="1", Text="管理者ログイン済（編集可） " },
                            new SelectListItem() { Value="2", Text="管理者不在（編集可）" },
                            new SelectListItem() { Value="3", Text="初回ログイン（編集可）" }
                        },
                        "Value",
                        "Text",
                        mode
                    );
        }

        /// <summary>
        /// 店舗
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetShopMDropList(string shopId)
        {
            return context.ShopMs.Select(s => new SelectListItem
            {
                Text = s.SHOPNAME,
                Value = s.SHOPID,
                Selected = s.SHOPID == shopId
            });
        }

        private IEnumerable<SelectListItem> GetManagerDropList(string shopId, string workerId)
        {
            return context.WorkerMs
                .Where(s => s.SHOPID == shopId && s.MANAGERKBN == "1" && s.NODISPLAYKBN == "0")
                .Select(s => new SelectListItem
                {
                    Text = s.WORKERNAME,
                    Value = s.WORKERID,
                    Selected = s.WORKERID == workerId
                });
        }

    }
}