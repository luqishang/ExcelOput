using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class IndustrySelectionController : Controller
    {
        private MasterContext context = new MasterContext();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public IndustrySelectionController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "IndustrySelection");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 初期表示
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

            //店舗ID
            string shopId = (string)Session["SHOPID"];
            // 契約ID
            string groupId = (string)Session["GROUPID"];

            // 画面情報
            var bIndustrySelect = new BIndustrySelect();
            // ドロップダウンリストフラグ
            bIndustrySelect.DropDownFlg = false;
            ViewBag.mode = "1";

            //業種マスタを取得
            var industryDt = from ind in context.IndustryMs
                             select ind;
            // 業種マスタデータが存在しない場合
            if (industryDt.Count() == 0)
            {
                ModelState.AddModelError(string.Empty, "業種マスタが存在しません。システム管理者へ連絡をお願いします。");
                return View(bIndustrySelect);
            }

            // 業種情報
            var IndustryList = new List<IndustryData>();

            // 契約IDが存在する場合
            if (!string.IsNullOrEmpty(groupId))
            {
                // 系列店舗データ取得
                var affiliateList = this.GetShop(groupId, shopId);
                if (affiliateList.Count() > 0)
                {
                    // ﾄﾞロップダウンリストフラグ
                    bIndustrySelect.DropDownFlg = true;
                    bIndustrySelect.AffiliateStoresList = affiliateList;
                    ViewBag.mode = "0";

                    // ドロップダウンリスト作成
                    ViewBag.SelectDrop = this.GetDropList("0");
                    return View(bIndustrySelect);
                }
            }

            // 業種選択一覧の作成
            var industryDtList = this.GetIndustry();
            bIndustrySelect.IndustryList = industryDtList;

            return View(bIndustrySelect);
        }

        /// <summary>
        /// ドロップダウンリスト変更
        /// </summary>
        /// <param name="form">画面表示値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CategoryChange(FormCollection form)
        {
            // ドロップダウン選択値を取得
            string mode = form["SelectMode"];
            ViewBag.mode = mode;
            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            // 契約ID
            string groupId = (string)Session["GROUPID"];
            // 画面情報
            var bIndustrySelect = new BIndustrySelect();
            // ﾄﾞロップダウンリストフラグ
            bIndustrySelect.DropDownFlg = true;
            // ドロップダウンリスト作成
            ViewBag.SelectDrop = this.GetDropList(mode);

            if ("0".Equals(mode))
            {
                // 店舗選択
                var affiliateList = this.GetShop(groupId, shopId);
                bIndustrySelect.AffiliateStoresList = affiliateList;
            }
            else
            {
                // 業種選択
                var industryList = this.GetIndustry();
                bIndustrySelect.IndustryList = industryList;
            }

            return View("Show", bIndustrySelect);
        }

        /// <summary>
        /// 業種登録
        /// </summary>
        /// <param name="form">画面入力情報</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Decision(FormCollection form)
        {
            string mode = form["SelectMode"];
            string selShopId = form["sel_ShopId"];
            string selIndustryId = form["sel_IndustryId"];
            bool dropDownFlg = Convert.ToBoolean(form["DropDownFlg"]);

            bool chk = true;
            // 画面の項目を選択されていない場合
            if ("0".Equals(mode) && string.IsNullOrEmpty(selShopId))
            {
                chk = false;
                ModelState.AddModelError(string.Empty, "店舗を選択してください。");
            } else if ("1".Equals(mode) && string.IsNullOrEmpty(selIndustryId))
            {
                chk = false;
                ModelState.AddModelError(string.Empty, "業種を選択してください。");
            }

            BIndustrySelect bIndustrySelect = this.SetIndustryData(mode, dropDownFlg);
            // 業種、系列店舗未選択の場合
            if (!chk)
            {
                return View("Show", bIndustrySelect);
            }

            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            // 自店舗の情報
            var MyShopM = from s in context.ShopMs
                          where s.SHOPID == shopId
                          select s;

            var shopM = MyShopM.FirstOrDefault();
            shopM.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;    // 更新ユーザーID

            if ("0".Equals(mode))
            {
                // 系列店舗の場合
                var ShopDt = from s in context.ShopMs
                             where s.SHOPID == selShopId
                             select s;

                if (ShopDt.Count() == 0 || ShopDt.FirstOrDefault() == null)
                {
                    throw new ApplicationException();
                }

                shopM.INDUSTRYID = ShopDt.FirstOrDefault().INDUSTRYID;      // 業種ID

                // フォーマット系列店舗IDをセッションに追加
                Session.Add("FORMATSHOPID", selShopId);
            } else
            {
                shopM.INDUSTRYID = selIndustryId;      // 業種ID
                // 業種マスタ
                var industryM = from indust in context.IndustryMs
                                where indust.INDUSTRYID == selIndustryId
                                select indust;

                if (industryM.Count() == 0 || industryM.FirstOrDefault() == null)
                {
                    throw new ApplicationException();
                }

                // フォーマット系列店舗IDをセッションに追加
                Session.Add("FORMATSHOPID", industryM.FirstOrDefault().INDUSTRYDATAID);
            }

            // データ登録
            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // データ更新
                        context.ShopMs.Attach(shopM);
                        context.Entry(shopM).State = EntityState.Modified;
                        context.SaveChanges();
                        // コミット
                        tran.Commit();

                        // ストレージフォルダ名を登録
                        var master = new MasterFunction();
                        master.SetShopStorageDirectory(context, shopId);

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // ロールバック
                        tran.Rollback();
                        // 排他エラー
                        ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                        return View("Show", bIndustrySelect);
                    }
                    catch (DbUpdateException ex)
                    {   
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", bIndustrySelect);
                        }
                        else
                        {
                            // ロールバック
                            tran.Rollback();
                            LogHelper.Default.WriteError(ex.Message, ex);
                            throw ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw ex;
                    }
                }
            }

            // データプレビュー画面へ遷移
            return RedirectToAction("Show", "InitialDataPreview");
        }

        /// <summary>
        /// 画面情報取得
        /// </summary>
        /// <param name="mode">表示モード</param>
        /// <returns>業種設定画面情報</returns>
        private BIndustrySelect SetIndustryData(string mode, bool dropDownFlg)
        {
            ViewBag.mode = mode;
            // 画面情報
            var bIndustrySelect = new BIndustrySelect();
            // ﾄﾞロップダウンリストフラグ
            bIndustrySelect.DropDownFlg = dropDownFlg;

            //店舗ID
            string shopId = (string)Session["SHOPID"];
            // 契約ID
            string groupId = (string)Session["GROUPID"];
            // ドロップダウンリスト作成
            ViewBag.SelectDrop = this.GetDropList(mode);

            // 契約IDが存在する場合
            if ("0".Equals(mode))
            {
                // 系列店舗データ取得
                var affiliateList = this.GetShop(groupId, shopId);
                if (affiliateList.Count() > 0)
                {
                    bIndustrySelect.AffiliateStoresList = affiliateList;
                    return bIndustrySelect;
                }
            }

            // 業種選択一覧の作成
            var industryDtList = this.GetIndustry();
            bIndustrySelect.IndustryList = industryDtList;

            return bIndustrySelect;
        }


        /// <summary>
        /// 系列店舗データ取得
        /// </summary>
        /// <param name="groupId">契約ID</param>
        /// <returns>店舗リスト</returns>
        private List<AffiliateStoreData> GetShop(string groupId, string shopId)
        {
            // 店舗マスタから系列店舗を取得
            var ShopMDt = from s in context.ShopMs
                          where (s.CONTRACTID == groupId || s.SHOPID == groupId) && s.SHOPID != shopId
                          select s;

            var affiliateList = new List<AffiliateStoreData>();

            if (ShopMDt.Count() > 0)
            {
                
                foreach (ShopM shop in ShopMDt)
                {
                    var dt = new AffiliateStoreData
                    {
                        ShopId = shop.SHOPID,           // 店舗ID
                        ShopName = shop.SHOPNAME,       // 店舗名
                        GroupId = shop.CONTRACTID,      // 契約ID
                        IndustryId = shop.INDUSTRYID    // 業種ID
                    };
                    affiliateList.Add(dt);
                }
            }

            return affiliateList;
        }

        /// <summary>
        /// 業種データ取得
        /// </summary>
        /// <returns>業種リスト</returns>
        private List<IndustryData> GetIndustry()
        {
            // 業種情報
            var IndustryList = new List<IndustryData>();

            //業種マスタを取得
            var industryDt = from ind in context.IndustryMs
                             select ind;

            if (industryDt.Count() > 0)
            {
                // 業種選択一覧の作成
                foreach (IndustryM indDt in industryDt)
                {
                    var dt = new IndustryData
                    {
                        IndustryId = indDt.INDUSTRYID,          // 業種ID
                        IndustryName = indDt.INDUSTRYNAME,      // 業種名称
                        IndustryDataId = indDt.INDUSTRYDATAID   // 業種データID
                    };

                    IndustryList.Add(dt);
                }
            }

            return IndustryList;
        }

        /// <summary>
        /// ドロップダウンリスト作成
        /// </summary>
        /// <param name="mode">選択値</param>
        /// <returns>選択ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetDropList(string mode)
        {
            return new SelectList(
                        new SelectListItem[] {
                            new SelectListItem() { Value="0", Text="店舗選択" },
                            new SelectListItem() { Value="1", Text="業種選択" }
                        },
                        "Value",
                        "Text",
                        mode
                    );
        }
    }
}