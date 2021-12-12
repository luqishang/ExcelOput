using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using HACCPExtender.Models;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Business;
using HACCPExtender.Models.Bussiness;
using static HACCPExtender.Controllers.Common.CommonConstants;
using System.Data.Entity.Validation;
using System.Text;
using static HACCPExtender.Constants.Const;

namespace HACCPExtender.Controllers
{
    public class CategoryMController : Controller
    {
        private MasterContext context = new MasterContext();
        // 大分類 最大登録件数（appset.configから取得）
        private static readonly int CATEGORY_REGIST_NUM_MAX = int.Parse(GetAppSet.GetAppSetValue("Category", "Max"));
        // 未登録
        private readonly int EDIT_NOTREGIST = 0;
        // 未更新
        private readonly int EDIT_NOTUPDATE = 1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CategoryMController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "categoryM");
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

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // 登録メッセージを取得
            string registMsg = (string)Session["registMsg"];
            if (!string.IsNullOrEmpty(registMsg))
            {
                Session.Remove("registMsg");
                ViewBag.registMsg = registMsg;
            }

            // データ取得
            var categoryDt = from a in context.CategoryMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            List<BCategoryM> listCategory = new List<BCategoryM>();

            if (categoryDt.Count() > 0)
            {
                int order = 1;
                var orderList = new List<int>();
                var idList = new List<int>();

                // DBに存在するデータをセット
                foreach (var dt in categoryDt)
                {
                    BCategoryM bCategory = new BCategoryM
                    {
                        // 編集モード
                        EditMode = EDIT_NOTUPDATE,
                        // 削除
                        DelFlg = false,
                        // 部門ID
                        CategoryId = dt.CATEGORYID,
                        // 部門名称
                        CategoryName = dt.CATEGORYNAME,
                        // 部門名称（英語表記）
                        CategoryNameEng = dt.CATEGORYNAMEENG,
                        // 表示No
                        DisplayNo = dt.DISPLAYNO,
                        // 登録ユーザーID
                        InsUserId = dt.INSUSERID,
                        // 更新ユーザーID
                        UpdUserId = dt.UPDUSERID,
                        // 更新年月日
                        UpdDate = dt.UPDDATE.ToString("yyyy/MM/dd HH:mm:ss.ffffff")
                    };

                    // リストにセット
                    listCategory.Add(bCategory);
                    // 表示順
                    orderList.Add(dt.DISPLAYNO);
                    // ID
                    idList.Add(int.Parse(dt.CATEGORYID));
                    order++;
                }

                // すべての大分類枠からDB登録分を抜く
                var allOrder = new List<int>();
                for (int i = 1; i <= CATEGORY_REGIST_NUM_MAX; i++)
                {
                    allOrder.Add(i);
                }
                // 空データオーダー
                int[] empOrder = allOrder.Except(orderList).ToArray();
                int[] empCategoryId = allOrder.Except(idList).ToArray();

                for (int i = 0; i < empCategoryId.Count(); i++)
                {
                    // 大分類ID
                    string idnumber = EditCategoryId(empCategoryId[i]);

                    BCategoryM bCategory = new BCategoryM
                    {
                        // 編集モード
                        EditMode = EDIT_NOTREGIST,
                        // 削除
                        DelFlg = false,
                        // 部門ID
                        CategoryId = idnumber,
                        // 表示No
                        DisplayNo = (short)empOrder[i]
                    };

                    // リストにセット
                    listCategory.Add(bCategory);
                }

                listCategory.Sort((a, b) => a.DisplayNo - b.DisplayNo);

            } else
            {
                // 警告メッセージ
                ModelState.AddModelError(string.Empty, MsgConst.NO_DATA);

                for (int i = 1; i <= CATEGORY_REGIST_NUM_MAX; i++)
                {
                    // 大分類ID
                    string idnumber = EditCategoryId(i);

                    BCategoryM bCategory = new BCategoryM
                    {
                        // 編集モード
                        EditMode = EDIT_NOTREGIST,
                        // 削除
                        DelFlg = false,
                        // 部門ID
                        CategoryId = idnumber,
                        // 表示No
                        DisplayNo = (short) i
                    };

                    // リストにセット
                    listCategory.Add(bCategory);
                }
            }

            return View(listCategory);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <returns>初期表示</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<BCategoryM> list)
        {
            // post時の情報をクリア
            ModelState.Clear();
            //  画面の表示順に並び替えてリストに設定
            List<BCategoryM> updList = list.OrderBy(BCategoryM => BCategoryM.No).ToList();

            // 削除用データ
            var delCategoryMs = new List<CategoryM>();
            // 登録データ
            var insCategoryMs = new List<CategoryM>();
            // 更新データ
            var updCategoryMs = new List<CategoryM>();
            // 表示順 DB登録用
            int rowNo = 1;
            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            //セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            // 更新チェック用現在データ取得
            var categoryDt = from a in context.CategoryMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            // 変更データ抽出（ソート番号以外）
            foreach (BCategoryM dt in updList)
            {
                dt.ShopId = shopId;

                // 追加行で、削除checkあり or 必須項目がすべて未入力の場合は無視
                if (dt.EditMode == EDIT_NOTREGIST
                    && (dt.DelFlg
                        || (string.IsNullOrEmpty(dt.CategoryName)
                            && string.IsNullOrEmpty(dt.CategoryNameEng))))
                {
                    rowNo++;
                    continue;
                }

                // 削除checkありの場合は入力内容無視
                if (dt.DelFlg)
                {
                    // データ整合性チェック（削除対象）
                    if (this.CheckRelation(dt, MsgConst.DELETE))
                    {
                        // 削除対象に追加
                        delCategoryMs.Add(this.SetCategoryM(dt, dt.DisplayNo, managerId));
                    }
                    else
                    {
                        // 関連チェックエラー
                        checkError = false;
                    }
                    rowNo++;
                    continue;
                }

                // 新規追加行
                if (dt.EditMode == EDIT_NOTREGIST)
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                    }
                    // 新規
                    insCategoryMs.Add(this.SetCategoryM(dt, rowNo, managerId, managerId));
                    rowNo++;
                    continue;
                }

                // DBデータから取得
                List<CategoryM> registData = new List<CategoryM>(categoryDt.Where(a => a.CATEGORYID == dt.CategoryId));

                // DBにデータが存在しない場合
                if (registData.Count == 0)
                {
                    // データが更新されているため、排他エラーとする
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    return View("Show", updList);
                }
                else if (this.CheckDataUpd(registData, dt))
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                    }
                }
                else
                {
                    // 表示順に変更がない場合は次の行へ
                    if (dt.DisplayNo == rowNo)
                    {
                        rowNo++;
                        continue;
                    }
                }
                // 更新
                updCategoryMs.Add(this.SetCategoryM(dt, rowNo, managerId));
                rowNo++;
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }

                return View("Show", updList);
            }

            // DB更新
            if (delCategoryMs.Count() > 0 || insCategoryMs.Count() > 0 || updCategoryMs.Count() > 0)
            {
                using (context = new MasterContext())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // データ削除
                            if (delCategoryMs.Count > 0)
                            {
                                foreach (CategoryM deldata in delCategoryMs)
                                {
                                    context.CategoryMs.Attach(deldata);
                                }
                                context.CategoryMs.RemoveRange(delCategoryMs);
                                context.SaveChanges();
                            }

                            // データ登録
                            if (insCategoryMs.Count() > 0)
                            {
                                foreach (CategoryM insModel in insCategoryMs)
                                {
                                    // データ登録
                                    context.CategoryMs.Add(insModel);
                                }
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updCategoryMs.Count() > 0)
                            {
                                foreach (CategoryM upddata in updCategoryMs)
                                {
                                    context.CategoryMs.Attach(upddata);
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
                            return View("Show", updList);
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                            {
                                //一意制約エラー
                                // ロールバック
                                tran.Rollback();
                                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                                return View("Show", updList);
                            }
                            else
                            {
                                // ロールバック
                                tran.Rollback();
                                LogHelper.Default.WriteError(ex.Message, ex);
                                throw new ApplicationException();
                            }   
                        }
                        catch(Exception ex)
                        {
                            // ロールバック
                            tran.Rollback();
                            LogHelper.Default.WriteError(ex.Message, ex);
                            throw new ApplicationException();
                        }
                    }
                }
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);
            // 初期表示処理に戻す
            return RedirectToAction("Show");
        }

        /// <summary>
        /// 初期ログイン時設定
        /// </summary>
        /// <returns>初期表示</returns>
        [HttpGet]
        public ActionResult InitialSetCategory()
        {
            //店舗ID
            string shopId = (string)Session["SHOPID"];
            // フォーマット系列店舗ID
            string formatShopId = (string)Session["FORMATSHOPID"];

            var categoryMDt = from ca in context.CategoryMs
                              where ca.SHOPID == formatShopId
                              select ca;
            if (categoryMDt.Count() == 0 || categoryMDt.FirstOrDefault() == null)
            {
                return RedirectToAction("Show");
            }

            // 登録用大分類リスト
            var categoryDtList = new List<CategoryM>();

            foreach (CategoryM category in categoryMDt)
            {
                category.SHOPID = shopId;                          // 店舗ID
                category.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;    // 登録ユーザーID
                category.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;    // 更新ユーザーID
                categoryDtList.Add(category);
            }

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (CategoryM insModel in categoryDtList)
                        {
                            // データ登録
                            context.CategoryMs.Add(insModel);
                        }
                        context.SaveChanges();
                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // ロールバック
                        tran.Rollback();
                    }
                    catch (DbUpdateException)
                    {
                        // ロールバック
                        tran.Rollback();
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

            return RedirectToAction("Show");
        }

        /// <summary>
        /// 初回ログイン 中分類マスタ遷移
        /// </summary>
        /// <returns>ResultViewオブジェクト</returns>
        [HttpPost]
        public ActionResult InitialSetLocation()
        {
            return RedirectToAction("InitialSetLocation", "LocationM");
        }

        /// <summary>
        /// 数字から0埋2桁文字列に編集
        /// </summary>
        /// <param name="id">数値</param>
        /// <returns>2桁文字列</returns>
        private string EditCategoryId (int id)
        {
            string idnumber = ("0" + id.ToString());
            return idnumber.Substring(idnumber.Length - 2);
        }

        /// <summary>
        /// 画面用データからモデル用データへ移送(登録ユーザーなし)
        /// </summary>
        /// <param name="bcategorym"></param>
        /// <param name="orderNo"></param>
        /// <param name="updUserId"></param>
        /// <returns></returns>
        private CategoryM SetCategoryM(BCategoryM bcategorym, int orderNo, string updUserId)
        {
            return SetCategoryM(bcategorym, orderNo, string.Empty, updUserId);
        }

        /// <summary>
        /// 画面用データからモデル用データへ移送
        /// </summary>
        /// <param name="bcategorym"></param>
        /// <returns></returns>
        private CategoryM SetCategoryM(BCategoryM bcategorym, int orderNo, string insUserId, string updUserId)
        {
            var model = new CategoryM
            {
                // 店舗ID
                SHOPID = bcategorym.ShopId,
                // 部門ID
                CATEGORYID = bcategorym.CategoryId,
                // 部門名称
                CATEGORYNAME = bcategorym.CategoryName,
                // 部門名称（英語表記）
                CATEGORYNAMEENG = string.IsNullOrEmpty(bcategorym.CategoryNameEng) ? bcategorym.CategoryName : bcategorym.CategoryNameEng,
                // 表示No
                DISPLAYNO = (short)orderNo,
                // 登録ユーザーID
                INSUSERID = insUserId,
                // 更新ユーザーID
                UPDUSERID = updUserId
            };
            // 削除・更新の場合
            if (string.IsNullOrEmpty(insUserId))
            {
                // 登録ユーザーID
                model.INSUSERID = bcategorym.InsUserId;
                // 更新年月日
                if (bcategorym.UpdDate != null)
                {
                    model.UPDDATE = DateTime.Parse(bcategorym.UpdDate);
                }
            }

            return model;
        }

        /// <summary>
        /// データ関連チェック
        /// </summary>
        /// <param name="bcategorym">画面用大分類マスタデータ</param>
        /// <returns>チェック結果</returns>
        private bool CheckRelation(BCategoryM bcategorym, string updelStr)
        {
            bool check = true;
            string categoryID = "大分類ID";
            var comm = new MasterFunction();

            // 初回ログイン時以外のみチェック
            if (!ManagerLoginMode.FIRST_LOGIN.Equals(Session["DISPMODE"]))
            {
                // 設問マスタデータ存在チェック
                bool question = comm.IsExistsQuestionM(context: context, shopId: bcategorym.ShopId, categoryId: bcategorym.CategoryId);
                if (question)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_QUESTION_MSG, updelStr, categoryID, bcategorym.CategoryId));
                    check = false;
                }

                // 帳票マスタデータ存在チェック
                bool report = comm.IsExistsReportM(context: context, shopId: bcategorym.ShopId, categoryId: bcategorym.CategoryId);
                if (report)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_REPORT_MSG, updelStr, categoryID, bcategorym.CategoryId));
                    check = false;
                }

                // 承認経路マスタデータ存在チェック
                var appRoute = comm.IsExistsApprovalrouteM(context: context, shopId: bcategorym.ShopId, categoryId: bcategorym.CategoryId);
                if (appRoute)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_APPROVALAR_MSG, updelStr, categoryID, bcategorym.CategoryId));
                    check = false;
                }

                // 承認情報データ存在チェック
                var approvalDt = comm.IsDataApproval(context: context, shopId: bcategorym.ShopId, categoryId: bcategorym.CategoryId);
                if (approvalDt)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_APPROVE_MSG, updelStr, categoryID, bcategorym.CategoryId));
                    check = false;
                }
            }

            return check;
        }

        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="dt">画面用大分類マスタデータ</param>
        /// <param name="hsError">エラー文言</param>
        /// <returns>チェック結果</returns>
        private bool CheckRequire(BCategoryM dt, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 入力項目のチェック
            if (string.IsNullOrEmpty(dt.CategoryName))
            {
                hsError.Add("大分類名称を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].CategoryName", string.Empty);
                checkError = false;
            }
            // 桁数チェック
            if (checkError && dt.CategoryName.Length > 25)
            {
                hsError.Add("大分類名称は25文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].CategoryName", string.Empty);
                checkError = false;
            }
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            if (!string.IsNullOrEmpty(dt.CategoryNameEng) && sjisEnc.GetByteCount(dt.CategoryNameEng) > 75)
            {
                hsError.Add("大分類名称（英語表記）は半角75文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].CategoryNameEng", string.Empty);
                checkError = false;
            }
            return checkError;
        }

        /// <summary>
        /// 更新項目結果チェック
        /// </summary>
        /// <param name="registData">DBデータ</param>
        /// <param name="dt">画面データ</param>
        /// <returns>bool（true:更新あり、false：更新なし）</returns>
        private bool CheckDataUpd(List<CategoryM> registData, BCategoryM dt)
        {
            return (registData[0].CATEGORYNAME != dt.CategoryName
                || registData[0].CATEGORYNAMEENG != dt.CategoryNameEng);
        }
    }
}