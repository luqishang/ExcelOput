using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;
using HACCPExtender.Controllers.Common;
using System.Text;
using HACCPExtender.Business;

namespace HACCPExtender.Controllers
{
    public class LocationMController : Controller
    {
        private MasterContext context = new MasterContext();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LocationMController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "locationM");
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
            string registMsg = (string) Session["registMsg"];
            if (!string.IsNullOrEmpty(registMsg))
            {
                Session.Remove("registMsg");
                ViewBag.registMsg = registMsg;
            }
            
            // データ取得
            var locationDt = from a in context.LocationMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            List<BLocationM> listLocation = new List<BLocationM>();

            if (locationDt.Count() == 0)
            {
                // 警告メッセージ
                ModelState.AddModelError(string.Empty, MsgConst.NO_DATA);
            } else
            {
                foreach (var dt in locationDt)
                {
                    BLocationM bLocation = new BLocationM
                    {
                        // 編集モード
                        EditMode = 0,
                        // 削除
                        DelFlg = false,
                        // 場所ID
                        LocationId = dt.LOCATIONID,
                        // 中分類名称
                        LocationName = dt.LOCATIONNAME,
                        // 中分類名称（英語表記）
                        LocationNameEng = dt.LOCATIONNAMEENG,
                        // 管理者限定区分
                        ManagerKbn = !BoolKbn.KBN_FALSE.Equals(dt.MANAGERKBN),
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
                    listLocation.Add(bLocation);
                }
            }
            return View(listLocation);
        }

        /// <summary>
        /// 行追加処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <returns>行追加データ</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Add(IList<BLocationM> list)
        {
            // post時の情報をクリア
            ModelState.Clear();
            // 画面の表示順に並び替えてリストに設定
            IList<BLocationM> addList = list.OrderBy(BLocationM => BLocationM.No).ToList();

            // 追加分
            BLocationM addRow = new BLocationM
            {
                // 編集モード
                EditMode = 0
            };
            // 行追加
            addList.Add(addRow);

            return View("Show", addList);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<BLocationM> list)
        {
            // post時の情報をクリア
            ModelState.Clear();
            //  画面の表示順に並び替えてリストに設定
            List<BLocationM> updList = list.OrderBy(BLocationM => BLocationM.No).ToList();

            // 削除用データ
            var delLocationMs = new List<LocationM>();
            // 登録データ
            var insLocationMs = new List<LocationM>();
            // 更新データ
            var updLocationMs = new List<LocationM>();
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
            var locationDt = from a in context.LocationMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            // 変更データ抽出（ソート番号以外）
            foreach (BLocationM dt in updList)
            {
                dt.ShopId = shopId;

                // 追加行（場所IDなし）で、削除チェックありまたは必須項目が何も入力されていない行は無視
                if (string.IsNullOrEmpty(dt.LocationId)
                    && (string.IsNullOrEmpty(dt.LocationName) && string.IsNullOrEmpty(dt.LocationNameEng)
                        || dt.DelFlg))
                {
                    continue;
                }

                // 削除checkありの場合は入力内容無視
                if (dt.DelFlg)
                {
                    // データ整合性チェック（削除対象）
                    if (this.CheckRelation(dt, MsgConst.DELETE))
                    {
                        // 削除対象に追加
                        delLocationMs.Add(this.SetLocationM(dt, dt.DisplayNo, managerId));
                    }
                    else
                    {
                        // 関連チェックエラー
                        checkError = false;
                    }
                    continue;
                }

                // 新規追加行
                if (string.IsNullOrEmpty(dt.LocationId))
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                    }
                    // 新規
                    insLocationMs.Add(this.SetLocationM(dt, rowNo, managerId, managerId));
                    rowNo++;
                    continue;
                }

                // DBデータから取得
                List<LocationM> registData = new List<LocationM>(locationDt.Where(a => a.LOCATIONID == dt.LocationId));

                // DBにデータが存在しない場合
                if (registData.Count == 0)
                {
                    // データが更新されているため、排他エラーとする
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    return View("Show", updList);
                }
                else if (this.CheckDataUpd(registData, dt))
                {
                    // データ整合性チェック（更新対象）ソート番号のみの更新は除く

                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                    }
                } else
                {
                    // 表示順に変更がない場合は次の行へ
                    if (dt.DisplayNo == rowNo)
                    {
                        rowNo++;
                        continue;
                    }
                }
                // 更新
                updLocationMs.Add(this.SetLocationM(dt, rowNo, managerId));
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
            if (delLocationMs.Count > 0 || updLocationMs.Count > 0 || insLocationMs.Count > 0)
            {
                using (context = new MasterContext())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // マスタ共通処理
                            var comm = new MasterFunction();

                            // データ削除
                            if (delLocationMs.Count > 0)
                            {
                                foreach (LocationM deldata in delLocationMs)
                                {
                                    context.LocationMs.Attach(deldata);
                                }
                                context.LocationMs.RemoveRange(delLocationMs);
                                context.SaveChanges();
                            }

                            // データ登録
                            foreach (LocationM insModel in insLocationMs)
                            {
                                // 場所IDを採番
                                insModel.LOCATIONID = comm.GetNumberingID(
                                    context: context, tableName: "LOCATION_M", columnName: "LOCATIONID", shopId: insModel.SHOPID, digits: 2);
                                // データ登録
                                context.LocationMs.Add(insModel);
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updLocationMs.Count() > 0)
                            {
                                foreach (LocationM upddata in updLocationMs)
                                {
                                    context.LocationMs.Attach(upddata);
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
                                throw ex;
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
        public ActionResult InitialSetLocation()
        {
            //店舗ID
            string shopId = (string)Session["SHOPID"];
            // フォーマット系列店舗ID
            string formatShopId = (string)Session["FORMATSHOPID"];

            var locationMDt = from ca in context.LocationMs
                              where ca.SHOPID == formatShopId
                              select ca;
            if (locationMDt.Count() == 0 || locationMDt.FirstOrDefault() == null)
            {
                return RedirectToAction("Show");
            }

            // 中分類マスタ
            var locationMList = new List<LocationM>();

            foreach (LocationM location in locationMDt)
            {
                location.SHOPID = shopId;                           // 店舗ID
                location.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;     // 登録ユーザーID
                location.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;     // 更新ユーザーID

                locationMList.Add(location);
            }

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (LocationM insModel in locationMList)
                        {
                            // データ登録
                            context.LocationMs.Add(insModel);
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
                    }
                }
            }

            return RedirectToAction("Show");
        }

        /// <summary>
        /// 初回ログイン 設問マスタ遷移
        /// </summary>
        /// <returns>ResultViewオブジェクト</returns>
        [HttpPost]
        public ActionResult InitialSetQuestion()
        {
            return RedirectToAction("InitialSetQuestion", "QuestionM");
        }

        /// <summary>
        /// 画面用データからモデル用データへ移送(登録ユーザーなし)
        /// </summary>
        /// <param name="blocationm">中分類名</param>
        /// <param name="orderNo">画面表示順</param>
        /// <param name="updUserId">更新ユーザー名</param>
        /// <returns>中分類マスタデータ</returns>
        private LocationM SetLocationM(BLocationM blocationm, int orderNo, string updUserId)
        {
            return SetLocationM(blocationm, orderNo, string.Empty, updUserId);
        }

        /// <summary>
        /// 画面用データからモデル用データへ移送
        /// </summary>
        /// <param name="blocationm">画面用中分類マスタデータ</param>
        /// <returns>中分類マスタデータ</returns>
        private LocationM SetLocationM(BLocationM blocationm, int orderNo, string insUserId, string updUserId)
        {
            var model = new LocationM
            {
                // 店舗ID
                SHOPID = blocationm.ShopId,
                //  場所ID
                LOCATIONID = blocationm.LocationId,
                // 中分類名称
                LOCATIONNAME = blocationm.LocationName,
                // 中分類名称（英語表記）
                LOCATIONNAMEENG = string.IsNullOrEmpty(blocationm.LocationNameEng) ? blocationm.LocationName : blocationm.LocationNameEng,
                // 管理者限定区分
                MANAGERKBN = blocationm.ManagerKbn ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
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
                model.INSUSERID = blocationm.InsUserId;
                // 更新年月日
                if (blocationm.UpdDate != null)
                {
                    model.UPDDATE = DateTime.Parse(blocationm.UpdDate);
                }
            }

            return model;
        }

        /// <summary>
        /// データ関連チェック
        /// </summary>
        /// <param name="blocationm">画面用中分類マスタデータ</param>
        /// <returns>チェック結果</returns>
        private bool CheckRelation(BLocationM blocationm, string updelStr)
        {
            bool check = true;
            string locationID = "中分類ID";
            var comm = new MasterFunction();

            // 初回ログイン時以外のみチェック
            if (!ManagerLoginMode.FIRST_LOGIN.Equals(Session["DISPMODE"]))
            {
                // 帳票マスタデータ存在チェック
                bool report = comm.IsExistsReportM(context: context, shopId: blocationm.ShopId, locationId: blocationm.LocationId);
                if (report)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_REPORT_MSG, updelStr, locationID, blocationm.LocationId));
                    check = false;
                }

                // 設問マスタ
                bool question = comm.IsExistsQuestionM(context: context, shopId: blocationm.ShopId, locationId: blocationm.LocationId);
                if (question)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_QUESTION_MSG, updelStr, locationID, blocationm.LocationId));
                    check = false;
                }

                // 承認経路マスタデータ存在チェック
                var appRoute = comm.IsExistsApprovalrouteM(context: context, shopId: blocationm.ShopId, locationId: blocationm.LocationId);
                if (appRoute)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_APPROVALAR_MSG, updelStr, locationID, blocationm.LocationId));
                    check = false;
                }

                // 料理マスタ
                var food = comm.IsExistsManagementM(context: context, shopId: blocationm.ShopId, locationId: blocationm.LocationId, managementId: ManagementID.SHOKUZAI);
                if (food)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_CUISINE_MSG, updelStr, locationID, blocationm.LocationId));
                    check = false;
                }
                // 半製品マスタ
                var half = comm.IsExistsManagementM(context: context, shopId: blocationm.ShopId, locationId: blocationm.LocationId, managementId: ManagementID.HANSEIHIN);
                if (half)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_SEMIFINISH_MSG, updelStr, locationID, blocationm.LocationId));
                    check = false;
                }
                // ユーザーマスタ
                var userm = comm.IsExistsManagementM(context: context, shopId: blocationm.ShopId, locationId: blocationm.LocationId, managementId: ManagementID.USERMST);
                if (userm)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_USERMST_MSG, updelStr, locationID, blocationm.LocationId));
                    check = false;
                }

                // 承認情報データ存在チェック
                var approvalDt = comm.IsDataApproval(context: context, shopId: blocationm.ShopId, locationId: blocationm.LocationId);
                if (approvalDt)
                {
                    ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_APPROVE_MSG, updelStr, locationID, blocationm.LocationId));
                    check = false;
                }

            }
            return check;
        }

        /// <summary>
        /// 必須チェック
        /// </summary>
        /// <param name="dt">画面用中分類マスタデータ</param>
        /// <param name="hsError">エラー文言</param>
        /// <returns>チェック結果</returns>
        private bool CheckRequire(BLocationM dt, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 入力項目のチェック
            if (string.IsNullOrEmpty(dt.LocationName))
            {
                hsError.Add("中分類名を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].LocationName", string.Empty);
                checkError = false;
            }
            // 桁数チェック
            if (checkError && dt.LocationName.Length > 30)
            {
                hsError.Add("中分類名は30文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].LocationName", string.Empty);
                checkError = false;
            }
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            if (!string.IsNullOrEmpty(dt.LocationNameEng) && sjisEnc.GetByteCount(dt.LocationNameEng) > 90)
            {
                hsError.Add("中分類名中分類名（英語表記）は半角90文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].LocationNameEng", string.Empty);
                checkError = false;
            }

            return checkError;
        }

        /// <summary>
        /// 更新項目結果チェック
        /// </summary>
        /// <param name="registData">DBデータ</param>
        /// <param name="dt">画面データ</param>
        /// <returns>チェック結果（true:更新あり、false：更新なし）</returns>
        private bool CheckDataUpd(List<LocationM> registData, BLocationM dt)
        {
            return (registData[0].LOCATIONNAME != dt.LocationName
                || registData[0].LOCATIONNAMEENG != dt.LocationNameEng
                || registData[0].MANAGERKBN != (dt.ManagerKbn ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE)) ;
        }
    }
}