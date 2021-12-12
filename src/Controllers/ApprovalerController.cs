using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using HACCPExtender.Models.Custom;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class ApprovalerController : Controller
    {
        private MasterContext context = new MasterContext();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ApprovalerController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "Approvaler");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 承認者選択画面表示
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>承認者選択</returns>
        [HttpGet]
        public ActionResult Show()
        {
            // 承認分類
            string snnNode = (string)Session["APPROVALNODEID"];
            // 大分類ID
            string categoryId = (string)Session["APPROVALCATEGORYID"];
            // 中分類ID
            string locationId = (string)Session["APPROVALLOCATIONID"];

            // セッションから破棄
            Session.Remove("APPROVALNODEID");
            Session.Remove("APPROVALCATEGORYID");
            Session.Remove("APPROVALLOCATIONID");

            // 承認分類が取得できない場合はエラー
            if (string.IsNullOrEmpty(snnNode))
            {
                throw new ApplicationException();
            }
            // 登録メッセージを取得
            string registMsg = (string)Session["registMsg"];
            if (!string.IsNullOrEmpty(registMsg))
            {
                Session.Remove("registMsg");
                ViewBag.registMsg = registMsg;
            }

            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);
            // 承認情報リスト
            List<ManagerWorker> workList = new List<ManagerWorker>();
            List<BApprovaler> appList = new List<BApprovaler>();

            // 画面タイトル設定
            this.SetTitle(shopId, snnNode, categoryId, locationId);

            // 作業者マスタ取得
            var workerDt = GetWorkers(shopId, categoryId);
            if (workerDt == null)
            {
                ModelState.AddModelError(string.Empty, MsgConst.NODATA_WORKER);
                ViewBag.editMode = "disabled";
                // 空リスト
                return View(appList);
            }

            // 承認経路マスタ取得
            var approvalDt = from a in context.ApprovalRouteMs
                             where a.SHOPID == shopId 
                                 && a.CATEGORYID == categoryId 
                                 && a.LOCATIONID == locationId 
                                 && a.APPROVALORDERCLASS == snnNode
                             select a;

            if (approvalDt.Count() == 0)
            {
                return View(appList);
            } else
            {                
                foreach (ApprovalRouteM app in approvalDt)
                {
                    BApprovaler approve = new BApprovaler
                    {
                        // 管理者ID
                        ApprovalManagerId = app.APPMANAGERID,
                        // 承認ノード順序
                        ApprovalNodeId = app.APPROVALNODEID,
                        // 承認者ドロップダウンリスト
                        ManagerDropList = workerDt.Select(s => new SelectListItem
                        {
                            Text = s.WORKERNAME,
                            Value = s.WORKERID,
                            Selected = s.WORKERID == app.APPMANAGERID
                        }),
                        InsUertId = app.INSUSERID,
                        UpdUserId = app.UPDUSERID,
                        Update = app.UPDDATE.ToString("yyyy/MM/dd HH:mm:ss.ffffff")
                    };
                    appList.Add(approve);
                }
            }

            return View("Show", appList);
        }

        /// <summary>
        /// 行追加
        /// </summary>
        /// <param name="form">画面フォームデータ</param>
        /// <param name="list">画面データ</param>
        /// <returns>画面表示</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Add(FormCollection form, IList<BApprovaler> list)
        {
            // post時の情報をクリア
            ModelState.Clear();
            // 承認分類
            string snnNode = form["approvalCategory"];
            // 大分類ID
            string categoryId = form["categoryId"];
            // 中分類ID
            string locationId = form["locationId"];

            // 承認分類が取得できない場合はエラー
            if (string.IsNullOrEmpty(snnNode))
            {
                throw new ApplicationException();
            }

            // 店舗ID
            string shopId = (string)Session["SHOPID"];

            ViewBag.approvalCategory = snnNode;
            ViewBag.categoryId = categoryId;
            ViewBag.locationId = locationId;

            // 画面タイトル設定
            this.SetTitle(shopId, snnNode, categoryId, locationId);

            // 作業者マスタ(管理者)
            var workerDt = GetWorkers(shopId, categoryId);
            // 画面連携リスト
            List<BApprovaler> appList = new List<BApprovaler>();
            // 追加分
            BApprovaler addRow = new BApprovaler
            {
                // 承認者ドロップダウンリスト
                ManagerDropList = workerDt.Select(s => new SelectListItem
                {
                    Text = s.WORKERNAME,
                    Value = s.WORKERID
                })
            };

            if (list == null)
            {
                // 1行目追加
                appList.Add(addRow);
                return View("Show", appList);
            } else
            {
                int i = 0;
                foreach (BApprovaler app in list)
                {
                    string key = string.Format("list[{0}].", i);
                    BApprovaler dataRow = new BApprovaler
                    {
                        ApprovalNodeId = app.ApprovalNodeId,
                        DelFlg = app.DelFlg,
                        ApprovalManagerId = app.ApprovalManagerId,
                        ManagerDropList = workerDt.Select(s => new SelectListItem
                        {
                            Text = s.WORKERNAME,
                            Value = s.WORKERID,
                            Selected = s.WORKERID == form[key + "ManagerDrop"]
                        }),
                        InsUertId = app.InsUertId,
                        UpdUserId = app.UpdUserId,
                        Update = app.Update
                    };
                    appList.Add(dataRow);
                    i++;
                }
            }
            // 行追加
            appList.Add(addRow);

            return View("Show", appList);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="form">画面フォームデータ</param>
        /// <param name="list">画面データ</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection form, IList<BApprovaler> list)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 承認分類
            string approvalCategory = form["approvalCategory"];
            // 大分類
            string categoryId = form["categoryId"];
            // 中分類
            string locationId = form["locationId"];

            // 削除用データ
            var delApprovalRouteMs = new List<ApprovalRouteM>();
            // 登録データ
            var insApprovalRouteMs = new List<ApprovalRouteM>();
            // 更新データ
            var updApprovalRouteMs = new List<ApprovalRouteM>();
            // 承認ノードIDリスト
            var nodeIdList = new List<short>();
            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            //セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            // 画面タイトルを設定
            this.SetTitle(shopId, approvalCategory, categoryId, locationId);

            // 更新チェック用現在データ取得
            var aaprovalRouteDt = from a in context.ApprovalRouteMs
                             orderby a.APPROVALNODEID
                             where a.SHOPID == shopId 
                                 && a.APPROVALORDERCLASS == approvalCategory 
                             select a;
            // 行カウンタ
            int i = 0;
            // 管理者ID
            var managerIdList = new List<string>();

            if (list == null)
            {
                ModelState.AddModelError(string.Empty, MsgConst.NOREGIST_DATA);
                return View("Show", this.SetDisplay(form, list, shopId));
            }

            foreach (BApprovaler approve in list)
            {
                // ドロップボックス選択値取得のキー
                string key = String.Format("list[{0}].", i);

                // 追加行（管理者IDなし）で、管理者が選択されていない行は無視
                if (approve.ApprovalNodeId == 0
                        && (string.IsNullOrEmpty(form[key + "ManagerDrop"]) || approve.DelFlg))
                {
                    i++;
                    continue;
                }

                // 店舗ID
                approve.ShopId = shopId;
                // 大分類
                approve.CategoryId = categoryId;
                // 中分類
                approve.LocationId = locationId;
                // 選択管理者ID
                string appManagerId = form[key + "ManagerDrop"];

                // 削除checkありの場合
                if (approve.DelFlg)
                {
                    // 削除対象に追加
                    delApprovalRouteMs.Add(this.SetApprovalRouteM(approve, managerId));
                    i++;
                    continue;
                }

                // 承認経路分類
                approve.ApprovalOrderClass = approvalCategory;

                // 新規追加行
                if (approve.ApprovalNodeId == 0)
                {
                    // 重複チェック
                    if (managerIdList.Contains(appManagerId))
                    {
                        hsError.Add(string.Format(MsgConst.ERR_DUPLICATION, "管理者"));
                        ModelState.AddModelError("list[" + i + "].ManagerDrop", string.Empty);
                        checkError = false;
                    }

                    // 承認管理者ID
                    approve.ApprovalManagerId = appManagerId;

                    // 新規
                    insApprovalRouteMs.Add(this.SetApprovalRouteM(approve, managerId, managerId));
                    // 重複チェック用リストにセット
                    managerIdList.Add(appManagerId);
                } else
                {
                    // 必須チェック
                    if (string.IsNullOrEmpty(appManagerId))
                    {
                        hsError.Add("承認者を選択してください。");
                        ModelState.AddModelError("list[" + i + "].ManagerDrop", string.Empty);
                        checkError = false;
                        i++;
                        continue;
                    }

                    // 承認管理者ID
                    approve.ApprovalManagerId = appManagerId;
                    int nodeId = approve.ApprovalNodeId;
                    
                    // DBデータから取得
                    List<ApprovalRouteM> registData = new List<ApprovalRouteM>(aaprovalRouteDt
                        .Where(a => a.CATEGORYID == approve.CategoryId 
                                && a.LOCATIONID == approve.LocationId 
                                && a.APPROVALNODEID == nodeId));

                    // DBにデータが存在しない場合
                    if (registData.Count == 0)
                    {
                        // データが更新されているため、排他エラーとする
                        hsError.Add(MsgConst.ERR_EXCLUSIVE);
                        checkError = false;
                    } else
                    {
                        // 更新対象チェック
                        if (!appManagerId.Equals(registData[0].APPMANAGERID))
                        {
                            // 重複チェック
                            if (managerIdList.Contains(appManagerId))
                            {
                                hsError.Add(string.Format(MsgConst.ERR_DUPLICATION, "管理者"));
                                ModelState.AddModelError("list[" + i + "].ManagerDrop", string.Empty);
                                checkError = false;
                            }
                            // 更新
                            updApprovalRouteMs.Add(this.SetApprovalRouteM(approve, managerId));
                        }
                        // 重複チェック用リストにセット
                        managerIdList.Add(appManagerId);
                        // 承認ノードIDリストにセット
                        nodeIdList.Add(approve.ApprovalNodeId);
                    }
                }
                // 行カウントアップ
                i++;
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }

                return View("Show", this.SetDisplay(form, list, shopId));
            }

            // 更新処理
            if (delApprovalRouteMs.Count > 0 || updApprovalRouteMs.Count > 0 || insApprovalRouteMs.Count > 0)
            {
                using (context = new MasterContext())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        // 画面情報
                        var dispval = this.SetDisplay(form, list, shopId);

                        try
                        {
                            // データ削除
                            if (delApprovalRouteMs.Count > 0)
                            {
                                foreach (ApprovalRouteM deldata in delApprovalRouteMs)
                                {
                                    context.ApprovalRouteMs.Attach(deldata);
                                }
                                context.ApprovalRouteMs.RemoveRange(delApprovalRouteMs);
                                context.SaveChanges();
                            }

                            // データ登録
                            if (insApprovalRouteMs.Count() > 0)
                            {
                                foreach (ApprovalRouteM insModel in insApprovalRouteMs)
                                {
                                    // ノードIDを採番
                                    insModel.APPROVALNODEID = this.GetNodeId(ref nodeIdList);
                                    // データ登録
                                    context.ApprovalRouteMs.Add(insModel);
                                }
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updApprovalRouteMs.Count() > 0)
                            {
                                foreach (ApprovalRouteM upddata in updApprovalRouteMs)
                                {
                                    context.ApprovalRouteMs.Attach(upddata);
                                    context.Entry(upddata).State = EntityState.Modified;
                                }
                                context.SaveChanges();
                            }

                            // コミット
                            tran.Commit();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            // ロールバック
                            tran.Rollback();
                            // 排他エラー
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            context = new MasterContext();
                            return View("Show", this.SetDisplay(form, list, shopId));
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                            {
                                //一意制約エラー
                                // ロールバック
                                tran.Rollback();
                                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                                return View("Show", this.SetDisplay(form, list, shopId));
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
            }
            // 登録メッセージ
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);
            // セッションへセット
            Session.Add("APPROVALNODEID", approvalCategory);
            Session.Add("APPROVALCATEGORYID", categoryId);
            Session.Add("APPROVALLOCATIONID", locationId);

            // 初期表示処理に戻す
            return RedirectToAction("Show");
        }

        /// <summary>
        /// 画面表示値設定
        /// </summary>
        /// <param name="form">画面フォームデータ</param>
        /// <param name="list">画面データ</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns></returns>
        private List<BApprovaler> SetDisplay(FormCollection form, IList<BApprovaler> list, string shopId)
        {
            // 画面連携リスト
            List<BApprovaler> appList = new List<BApprovaler>();
            if (list == null)
            {
                return appList;
            }

            // 作業者マスタ(管理者)
            var workerDt = GetWorkers(shopId, form["categoryId"]);

            int cnt = 0;
            foreach (BApprovaler app in list)
            {
                string key = string.Format("list[{0}].", cnt);
                BApprovaler dataRow = new BApprovaler
                {
                    ApprovalNodeId = app.ApprovalNodeId,
                    DelFlg = app.DelFlg,
                    ApprovalManagerId = app.ApprovalManagerId,
                    ManagerDropList = workerDt.Select(s => new SelectListItem
                    {
                        Text = s.WORKERNAME,
                        Value = s.WORKERID,
                        Selected = s.WORKERID == form[key + "ManagerDrop"]
                    }),
                    InsUertId = app.InsUertId,
                    UpdUserId = app.UpdUserId,
                    Update = app.Update
                };
                appList.Add(dataRow);
                cnt++;
            }
            return appList;
        }

        /// <summary>
        /// タイトル部設定
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="approvalCategory">承認分類</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        private void SetTitle(string shopId, string approvalCategory, string categoryId, string locationId)
        {
            ViewBag.approvalCategory = approvalCategory;
            ViewBag.categoryId = categoryId;
            ViewBag.locationId = locationId;

            if (ApprovalCategory.MIDDLE.Equals(approvalCategory))
            {
                ViewBag.approvalerTitle = "中分類承認者選択";
                ViewBag.mode = ApprovalCategory.MODE_MIDDLE;
                ViewBag.categoryName = this.GetCategoryName(shopId, categoryId);
                ViewBag.locationName = this.GetLocationName(shopId, locationId);
            }
            else if (ApprovalCategory.MAJOR.Equals(approvalCategory))
            {
                ViewBag.approvalerTitle = "大分類承認者選択";
                ViewBag.mode = ApprovalCategory.MODE_MAJOR;
                ViewBag.categoryName = this.GetCategoryName(shopId, categoryId);
            }
            else if (ApprovalCategory.FACILITY.Equals(approvalCategory))
            {
                ViewBag.approvalerTitle = "施設承認者選択";
                ViewBag.mode = ApprovalCategory.MODE_FACILITY;
            }

        }

        /// <summary>
        /// モデルデータ移送（削除・更新用）
        /// </summary>
        /// <param name="approve">画面値</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns></returns>
        private ApprovalRouteM SetApprovalRouteM(BApprovaler approve, string updUserId)
        {
            return SetApprovalRouteM(approve, string.Empty, updUserId);
        }

        /// <summary>
        /// モデルデータ移送
        /// </summary>
        /// <param name="approve">画面値</param>
        /// <param name="insUserId">登録ユーザーID</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns>承認経路データ</returns>
        private ApprovalRouteM SetApprovalRouteM(BApprovaler approve, string insUserId, string updUserId)
        {
            var model = new ApprovalRouteM
            {
                // 店舗ID
                SHOPID = approve.ShopId,
                // 大分類ID
                CATEGORYID = approve.CategoryId,
                // 中分類ID
                LOCATIONID = approve.LocationId,
                // 承認ノード内番号
                APPROVALNODEID = approve.ApprovalNodeId,
                // 承認経路分類
                APPROVALORDERCLASS = approve.ApprovalOrderClass,
                // 承認管理者ID
                APPMANAGERID = approve.ApprovalManagerId,
                // 登録ユーザーID
                INSUSERID = insUserId,
                // 更新ユーザーID
                UPDUSERID = updUserId,
            };
            if (string.IsNullOrEmpty(insUserId))
            {
                model.INSUSERID = approve.InsUertId;
                // 更新日時
                if (approve.Update != null)
                {
                    model.UPDDATE = DateTime.Parse(approve.Update);
                }
            }

            return model;
        }

        /// <summary>
        /// 大分類名を取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <returns>大分類名</returns>
        private string GetCategoryName(string shopId, string categoryId)
        {
            var dt = from c in context.CategoryMs
                        where c.SHOPID == shopId && c.CATEGORYID == categoryId
                        select c.CATEGORYNAME;

            if (dt.Count() > 0)
            {
                return dt.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// 中分類名を取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <returns>中分類名</returns>
        private string GetLocationName(string shopId, string locationId)
        {
            var dt = from l in context.LocationMs
                        where l.SHOPID == shopId && l.LOCATIONID == locationId
                        select l.LOCATIONNAME;

            if (dt.Count() > 0)
            {
                return dt.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// 作業者マスタから管理者を取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <returns></returns>
        private DbRawSqlQuery<ManagerWorker> GetWorkers(string shopId, string categoryId)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT WORKERID, WORKERNAME ");
            sql.Append("FROM WORKER_M ");
            sql.Append("WHERE ");
            sql.Append("SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND MANAGERKBN = '1' ");
            sql.Append("AND NODISPLAYKBN = '0' ");
            if (!string.IsNullOrEmpty(categoryId) && !ApprovalCategory.FACILITYDATA_CATEGORY.Equals(categoryId))
            {
                sql.Append("AND CATEGORYKBN");
                sql.Append(categoryId.Replace("0", "")); // 前0削除
                sql.Append(" = '1' ");
            }
            sql.Append("ORDER BY DISPLAYNO ");
            sql.Append("FOR READ ONLY");

            var managerDt = context.Database.SqlQuery<ManagerWorker>(sql.ToString());

            if (managerDt.Count() == 0)
            {
                return null;
            }
            return managerDt;
        }

        /// <summary>
        /// 承認ノードIDの採番
        /// </summary>
        /// <param name="nodeIdList">ノードIDリスト</param>
        /// <returns>採番ノードID</returns>
        private short GetNodeId(ref List<short> nodeIdList)
        {

            short nodeId = 1;

            if (nodeIdList.Count() > 0)
            {
                for (int i =1; i <= nodeIdList.Max()+1; i++)
                {
                    if (!nodeIdList.Contains((short)i))
                    {
                        nodeId = (short) i;
                        nodeIdList.Add(nodeId);
                        break;
                    }
                }
            } else
            {
                nodeIdList.Add(nodeId);
            }

            return nodeId;
        }

    }
}