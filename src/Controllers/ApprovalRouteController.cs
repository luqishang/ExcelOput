using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using HACCPExtender.Models.Custom;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class ApprovalRouteController : Controller
    {
        private readonly MasterContext context = new MasterContext();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ApprovalRouteController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "ApprovalRoute");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 承認マスタ画面表示
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpGet]
        public ActionResult Show()
        {
            // セッションから破棄
            Session.Remove("APPROVALNODEID");
            Session.Remove("APPROVALCATEGORYID");
            Session.Remove("APPROVALLOCATIONID");

            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);

            // 承認分類
            ViewBag.SnnOptions = this.GetSnnCategoryList(string.Empty);
            // 大分類
            ViewBag.categoryOptions = new SelectListItem[] { };
            // Model
            List<BApprovalRoute> routeList = new List<BApprovalRoute>();
            // モード
            ViewBag.mode = string.Empty;

            return View(routeList);
        }

        /// <summary>
        /// 承認分類ドロップダウンリスト変更処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SnnCategoryChange(FormCollection form)
        {
            // 承認分類
            string snnCategory = form["Snncategory"];

            // 承認分類が取得できない場合はアプリケーションエラー
            if (string.IsNullOrEmpty(snnCategory))
            {
                throw new ApplicationException();
            }

            // 承認分類
            ViewBag.SnnOptions = this.GetSnnCategoryList(snnCategory);
            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);

            if (ApprovalCategory.MIDDLE.Equals(snnCategory))
            { // 中分類承認

                // 大分類ドロップダウンリスト
                var categoryDrop = this.GetBumonMDropList(shopId, string.Empty);
                if (categoryDrop.Count() == 0)
                {
                    // 大分類データが存在しない場合はメッセージ表示
                    ModelState.AddModelError(string.Empty, MsgConst.NODATA_CATEGORY);
                    ViewBag.editMode = "disabled";
                }
                ViewBag.categoryOptions = categoryDrop;
                // Model
                List<BApprovalRoute> routeList = new List<BApprovalRoute>();
                // モード
                ViewBag.mode = string.Empty;

                return View("Show", routeList);

            } else if (ApprovalCategory.MAJOR.Equals(snnCategory))
            { // 大分類承認

                // モード
                ViewBag.mode = ApprovalCategory.MODE_MAJOR;
                // 大分類ドロップダウンリスト
                ViewBag.categoryOptions = new SelectListItem[] { };
                // 大分類を取得
                var categoryDt = this.GetCategory(shopId);
                if (categoryDt == null)
                {
                    // 大分類データが存在しない場合はメッセージ表示
                    ModelState.AddModelError(string.Empty, MsgConst.NODATA_CATEGORY);
                    ViewBag.editMode = "disabled";
                    return View("Show", new List<BApprovalRoute>());
                }
                else
                {
                    // 承認データ（大分類）
                    var appmstData = this.GetAppovalRoute(shopId, ApprovalCategory.MAJOR, string.Empty);
                    // 承認データのマージ
                    List<BApprovalRoute> appmstList = this.GetRouteData(appmstData, ApprovalCategory.MAJOR);
                    // 大分類データとマージ
                    List<BApprovalRoute> appList = MajorCategoryData(categoryDt, appmstList);
                    return View("Show", appList);
                }
            }
            else if (ApprovalCategory.FACILITY.Equals(snnCategory))
            { // 施設承認

                // 大分類ドロップダウンリスト
                ViewBag.categoryOptions = new SelectListItem[] { };
                // 承認データ（大分類）
                var appData = this.GetAppovalRoute(shopId, ApprovalCategory.FACILITY, string.Empty);
                List<BApprovalRoute> appList = this.GetRouteData(appData, ApprovalCategory.FACILITY);
                // モード
                ViewBag.mode = ApprovalCategory.MODE_FACILITY;

                return View("Show", appList);
            } else
            {
                throw new ApplicationException();
            }
        }

        /// <summary>
        /// 大分類ドロップダウンリスト変更処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CategoryChange(FormCollection form)
        {
            // 承認分類
            string snnCategory = form["Snncategory"];
            // 大分類
            string category = form["Category"];

            if (string.IsNullOrEmpty(snnCategory) || string.IsNullOrEmpty(category))
            {
                throw new ApplicationException();
            }

            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);
            // モード
            ViewBag.mode = ApprovalCategory.MODE_MIDDLE;
            // 承認分類
            ViewBag.SnnOptions = this.GetSnnCategoryList(snnCategory);
            // 大分類ドロップダウンリスト
            ViewBag.categoryOptions = this.GetBumonMDropList(shopId, category);
            // 中分類を取得
            var locationDt = this.GetLocation(shopId);
            
            if (locationDt == null)
            {
                // 中分類データが存在しない場合はメッセージ表示
                ModelState.AddModelError(string.Empty, MsgConst.NODATA_LOCATION);
                ViewBag.editMode = "disabled";
                return View("Show", new List<BApprovalRoute>());
            } else
            {
                // 承認データ（中分類）
                var appData = this.GetAppovalRoute(shopId, ApprovalCategory.MIDDLE, category);
                // 承認データのマージ
                List<BApprovalRoute> appmstList = this.GetRouteData(appData, ApprovalCategory.MIDDLE);
                // 中分類データとマージ
                List<BApprovalRoute> appList = this.MiddleCategoryData(locationDt, appmstList, category);

                return View("Show", appList);
            }
        }

        /// <summary>
        /// 承認者選択へ遷移
        /// </summary>
        /// <param name="form">画面情報</param>
        /// <returns>ViewResultオブジェクト</returns>
        public ActionResult Approvaler(FormCollection form)
        {
            // セッションにセット
            Session.Add("APPROVALNODEID", form["sel_ApprovalNodeId"]);
            Session.Add("APPROVALCATEGORYID", form["sel_CategoryId"]);
            Session.Add("APPROVALLOCATIONID", form["sel_LocationId"]);

            // 承認者選択画面へ遷移
            return RedirectToAction("Show", "Approvaler");
        }

        /// <summary>
        /// 承認者選択からの戻り処理
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult BackShow(FormCollection form)
        {
            // セッションから破棄
            Session.Remove("APPROVALNODEID");
            Session.Remove("APPROVALCATEGORYID");
            Session.Remove("APPROVALLOCATIONID");

            // 承認分類
            string appClass = form["approvalCategory"];
            // 大分類ID
            string categoryId = form["categoryId"];

            // パラメータチェック
            if (string.IsNullOrEmpty(appClass))
            {
                throw new ApplicationException();
            }

            // 承認分類
            ViewBag.SnnOptions = this.GetSnnCategoryList(appClass);
            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);

            // 承認分類
            ViewBag.SnnOptions = this.GetSnnCategoryList(appClass);

            var appList = new List<BApprovalRoute>();

            if (ApprovalCategory.MIDDLE.Equals(appClass))    // 中分類承認
            {
                // モード
                ViewBag.mode = ApprovalCategory.MODE_MIDDLE;

                // 大分類ドロップダウンリスト
                ViewBag.categoryOptions = this.GetBumonMDropList(shopId, categoryId);
                // 中分類を取得
                var locationDt = this.GetLocation(shopId);
                // 承認データ（中分類）
                var appData = this.GetAppovalRoute(shopId, ApprovalCategory.MIDDLE, categoryId);
                // 承認データのマージ
                List<BApprovalRoute> appmstList = this.GetRouteData(appData, ApprovalCategory.MIDDLE);
                // 中分類データとマージ
                appList = this.MiddleCategoryData(locationDt, appmstList, categoryId);

            }
            else if (ApprovalCategory.MAJOR.Equals(appClass))  // 大分類承認
            {
                // モード
                ViewBag.mode = ApprovalCategory.MODE_MAJOR;

                // 大分類ドロップダウンリスト
                ViewBag.categoryOptions = new SelectListItem[] { };
                // 大分類を取得
                var categoryDt = this.GetCategory(shopId);
                // 承認データ（大分類）
                var appmstData = this.GetAppovalRoute(shopId, ApprovalCategory.MAJOR, string.Empty);
                // 承認データのマージ
                List<BApprovalRoute> appmstList = this.GetRouteData(appmstData, ApprovalCategory.MAJOR);
                // 大分類データとマージ
                appList = MajorCategoryData(categoryDt, appmstList);

            } else if (ApprovalCategory.FACILITY.Equals(appClass))   // 施設承認
            {
                // モード
                ViewBag.mode = ApprovalCategory.MODE_FACILITY;

                // 大分類ドロップダウンリスト
                ViewBag.categoryOptions = new SelectListItem[] { };
                // 承認データ（施設）
                var appData = this.GetAppovalRoute(shopId, ApprovalCategory.FACILITY, string.Empty);
                appList = this.GetRouteData(appData, ApprovalCategory.FACILITY);
            }

            return View("Show", appList);
        }

        /// <summary>
        /// 承認経路データ編集
        /// </summary>
        /// <param name="appData">承認経路データ</param>
        /// <param name="level">承認分類</param>
        /// <returns>ViewResultオブジェクト</returns>
        private List<BApprovalRoute> GetRouteData(DbRawSqlQuery<ApprovalRouteJ> appData, string level)
        {
            List<BApprovalRoute> appList = new List<BApprovalRoute>();

            if (appData == null)
            {
                return appList;
            }
            else
            {
                BApprovalRoute appRoute = new BApprovalRoute();
                string key = string.Empty;

                int i = 0;
                int maxCnt = appData.Count() - 1;
                foreach (ApprovalRouteJ route in appData)
                {
                    string dataval = string.Empty;
                    if (ApprovalCategory.MIDDLE.Equals(level))
                    {
                        dataval = route.LOCATIONID;
                    }
                    else if (ApprovalCategory.MAJOR.Equals(level))
                    {
                        dataval = route.CATEGORYID;
                    } else
                    {
                        dataval = route.SHOPID;
                    }

                    if (i == 0 || !key.Equals(dataval))
                    {
                        // 初回以外は前回までの情報をリストに追加
                        if (i > 0)
                        {
                            appList.Add(appRoute);
                        } else
                        {
                            appRoute = new BApprovalRoute();
                        }
                        // 承認経路をマスタを格納
                        appRoute = new BApprovalRoute
                        {
                            ShopId = route.SHOPID,
                            CategoryId = route.CATEGORYID,
                            CategoryName = route.CATEGORYNAME,
                            LocationId = route.LOCATIONID,
                            LocationName = route.LOCATIONNAME,
                            ApprovalOrderClass = level,
                            ApprovalManagerName = route.WORKERNAME
                        };
                    }
                    else
                    {
                        // 承認者名に追加
                        string name = appRoute.ApprovalManagerName + ", " + route.WORKERNAME;
                        appRoute.ApprovalManagerName = name;
                    }

                    if (ApprovalCategory.MIDDLE.Equals(level))
                    {
                        key = appRoute.LocationId;
                    } else if (ApprovalCategory.MAJOR.Equals(level))
                    {
                        key = appRoute.CategoryId;
                    } else
                    {
                        key = appRoute.ShopId;
                    }
                    
                    if (i == maxCnt)
                    {
                        appList.Add(appRoute);
                    }

                    i++;
                }
                return appList;
            }
        }

        /// <summary>
        /// 中分類マスタデータと承認経路マスタデータをマージ
        /// </summary>
        /// <param name="locationList">中分類マスタデータ</param>
        /// <param name="approvalList">承認経路マスタデータ</param>
        /// <returns></returns>
        private List<BApprovalRoute> MiddleCategoryData(IQueryable<LocationM> locationList, List<BApprovalRoute> approvalList, string categoryId)
        {
            var appList = new List<BApprovalRoute>();

            var routeMap = new Dictionary<string, BApprovalRoute>();
            // LocationIdがキーのディクショナリを作成
            if (approvalList.Count > 0)
            {
                foreach (BApprovalRoute route in approvalList)
                {
                    routeMap.Add(route.LocationId, route);
                }
            }

            foreach (LocationM location in locationList)
            {
                BApprovalRoute Bapp = new BApprovalRoute();

                Bapp = new BApprovalRoute
                {
                    ShopId = location.SHOPID,
                    CategoryId = categoryId,
                    LocationId = location.LOCATIONID,
                    LocationName = location.LOCATIONNAME,
                    ApprovalOrderClass = ApprovalCategory.MIDDLE
                };

                // 承認経路データが存在する場合
                if (routeMap.ContainsKey(location.LOCATIONID))
                {
                    // 承認者名をセット
                    Bapp.ApprovalManagerName = routeMap[location.LOCATIONID].ApprovalManagerName;
                }
                appList.Add(Bapp);
            }
            return appList;
        }

        /// <summary>
        /// 大分類マスタデータと承認経路マスタデータをマージ
        /// </summary>
        /// <param name="categoryList">大分類マスタデータ</param>
        /// <param name="approvalList">承認経路マスタデータ</param>
        /// <returns></returns>
        private List<BApprovalRoute> MajorCategoryData(IQueryable<CategoryM> categoryList, List<BApprovalRoute> approvalList)
        {
            var appList = new List<BApprovalRoute>();

            var routeMap = new Dictionary<string, BApprovalRoute>();
            // LocationIdがキーのディクショナリを作成
            if (approvalList.Count > 0)
            {
                foreach (BApprovalRoute route in approvalList)
                {
                    routeMap.Add(route.CategoryId, route);
                }
            }

            foreach (CategoryM category in categoryList)
            {
                BApprovalRoute　Bapp = new BApprovalRoute
                {
                    ShopId = category.SHOPID,
                    CategoryId = category.CATEGORYID,
                    CategoryName = category.CATEGORYNAME,
                    LocationId = ApprovalCategory.MAJORDATA_LOCATION,
                    ApprovalOrderClass = ApprovalCategory.MAJOR
                };

                // 承認経路データが存在する場合
                if (routeMap.ContainsKey(category.CATEGORYID))
                {
                    // 承認者名をセット
                    Bapp.ApprovalManagerName = routeMap[category.CATEGORYID].ApprovalManagerName;
                }
                appList.Add(Bapp);
            }
            return appList;
        }

        /// <summary>
        /// 中分類を取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <returns>中分類データ</returns>
        private IQueryable<LocationM> GetLocation(string shopId)
        {
            var locationDt = from l in context.LocationMs
                             where l.SHOPID == shopId
                             select l;

            if (locationDt.Count() > 0)
            {
                return locationDt;
            }

            return null;
        }

        /// <summary>
        /// 大分類を取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <returns>大分類データ</returns>
        private IQueryable<CategoryM> GetCategory(string shopId)
        {
            var categoryDt = from a in context.CategoryMs
                             where a.SHOPID == shopId
                             select a;

            if (categoryDt.Count() > 0)
            {
                return categoryDt;
            }

            return null;
        }

        /// <summary>
        /// 承認経路データ取得(中分類)
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="snnCategory">承認分類</param>
        /// <returns>承認経路データ</returns>
        private DbRawSqlQuery<ApprovalRouteJ> GetMiddleRoute1(string shopId, string categoryId, string snnCategory)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT a.SHOPID, a.CATEGORYID, a.LOCATIONID, a.APPMANAGERID, c.WORKERNAME, ");
            sql.Append("b.LOCATIONNAME, ");
            sql.Append("d.CATEGORYNAME ");
            sql.Append("FROM APPROVALROUTE_M a,  ");
            sql.Append("LOCATION_M b, ");
            sql.Append("WORKER_M c, CATEGORY_M d WHERE ");
            sql.Append("a.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("and a.CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("and a.APPROVALORDERCLASS = ");
            sql.Append(snnCategory);
            sql.Append(" ");
            sql.Append("and a.SHOPID = b.SHOPID ");
            sql.Append("and a.LOCATIONID = b.LOCATIONID ");
            sql.Append("and a.SHOPID = c.SHOPID ");
            sql.Append("and a.APPMANAGERID = c.WORKERID ");
            sql.Append("and a.SHOPID = d.SHOPID ");
            sql.Append("and a.CATEGORYID = d.CATEGORYID ");
            sql.Append("FOR READ ONLY");

            var snnKeiroDt = context.Database.SqlQuery<ApprovalRouteJ>(sql.ToString());

            if (snnKeiroDt.Count() == 0)
            {
                return null;
            }
            return snnKeiroDt;
        }

        /// <summary>
        /// 承認経路データ取得(小分類)
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="snnCategory">承認分類</param>
        /// <param name="category">大分類ID</param>
        /// <returns></returns>
        private DbRawSqlQuery<ApprovalRouteJ> GetAppovalRoute(string shopId, string snnCategory, string category)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT a.SHOPID, a.CATEGORYID, a.LOCATIONID, a.APPMANAGERID, a.APPROVALNODEID, w.WORKERNAME ");
            sql.Append("FROM APPROVALROUTE_M a, WORKER_M w ");
            sql.Append("WHERE ");
            sql.Append("a.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("and a.APPROVALORDERCLASS = '");
            sql.Append(snnCategory);
            sql.Append("' ");
            if (ApprovalCategory.MIDDLE.Equals(snnCategory))
            {
                sql.Append("and a.CATEGORYID = '");
                sql.Append(category);
                sql.Append("' ");
            }
            sql.Append("and a.SHOPID = w.SHOPID ");
            sql.Append("and a.APPMANAGERID = w.WORKERID ");
            sql.Append("ORDER BY ");
            if (ApprovalCategory.MIDDLE.Equals(snnCategory))
            {
                sql.Append("a.CATEGORYID, a.LOCATIONID, a.APPROVALNODEID ");
            }
            else if (ApprovalCategory.MAJOR.Equals(snnCategory))
            {
                sql.Append("a.CATEGORYID, a.APPROVALNODEID ");
            } else
            {
                sql.Append("a.APPROVALNODEID ");
            }
            sql.Append("FOR READ ONLY");

            var snnKeiroDt = context.Database.SqlQuery<ApprovalRouteJ>(sql.ToString());

            if (snnKeiroDt.Count() == 0)
            {
                return null;
            }
            return snnKeiroDt;
        }

        /// <summary>
        /// 承認経路データ取得(大分類)
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="snnCategory">承認分類</param>
        /// <returns>承認経路データ</returns>
        private DbRawSqlQuery<ApprovalRouteJ> GetMojorRoute1(string shopId, string snnCategory)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT a.SHOPID, a.CATEGORYID, a.LOCATIONID, a.APPMANAGERID, c.WORKERNAME, ");
            sql.Append("d.CATEGORYNAME ");
            sql.Append("FROM APPROVALROUTE_M a,  ");
            sql.Append("WORKER_M c, CATEGORY_M d WHERE ");
            sql.Append("a.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("and a.APPROVALORDERCLASS = ");
            sql.Append(snnCategory);
            sql.Append(" ");
            sql.Append("and a.SHOPID = c.SHOPID ");
            sql.Append("and a.APPMANAGERID = c.WORKERID ");
            sql.Append("and a.SHOPID = d.SHOPID ");
            sql.Append("and a.CATEGORYID = d.CATEGORYID ");
            sql.Append("FOR READ ONLY");

            var snnKeiroDt = context.Database.SqlQuery<ApprovalRouteJ>(sql.ToString());

            if (snnKeiroDt.Count() == 0)
            {
                return null;
            }
            return snnKeiroDt;
        }

        /// <summary>
        /// 承認経路データ取得(施設)
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="snnCategory">承認分類</param>
        /// <returns>承認経路データ</returns>
        private DbRawSqlQuery<ApprovalRouteJ> GetFacirityRoute(string shopId, string snnCategory)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT a.SHOPID, a.CATEGORYID, a.LOCATIONID, a.APPMANAGERID, c.WORKERNAME ");
            sql.Append("FROM APPROVALROUTE_M a,  ");
            sql.Append("WORKER_M c WHERE ");
            sql.Append("a.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("and a.APPROVALORDERCLASS = ");
            sql.Append(snnCategory);
            sql.Append(" ");
            sql.Append("and a.SHOPID = c.SHOPID ");
            sql.Append("and a.APPMANAGERID = c.WORKERID ");
            sql.Append("FOR READ ONLY");

            var snnKeiroDt = context.Database.SqlQuery<ApprovalRouteJ>(sql.ToString());

            if (snnKeiroDt.Count() == 0)
            {
                return null;
            }
            return snnKeiroDt;
        }

        /// <summary>
        /// 承認ノードドロップダウンリスト作成
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetSnnCategoryList(string mode)
        {
            return new SelectList(
                        new SelectListItem[] {
                            new SelectListItem() { Value="1", Text="中分類承認" },
                            new SelectListItem() { Value="2", Text="大分類承認" },
                            new SelectListItem() { Value="3", Text="施設承認" },
                        },
                        "Value",
                        "Text",
                        mode
                    );
        }

        /// <summary>
        /// 大分類ドロップダウンリスト作成
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">部門ID</param>
        /// <returns>大分類ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetBumonMDropList(string shopId, string categoryId)
        {
            return context.CategoryMs.Where(s => s.SHOPID == shopId).OrderBy(s => s.DISPLAYNO).Select(s => new SelectListItem
            {
                Text = s.CATEGORYNAME,
                Value = s.CATEGORYID,
                Selected = s.CATEGORYID == categoryId
            });
        }

    }
}