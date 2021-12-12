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

namespace HACCPExtenfer.Controllers
{
    public class ReportMController : Controller
    {
        // コンテキスト
        private MasterContext context = new MasterContext();
        // 共通処理
        readonly CommonFunction comFunc = new CommonFunction();
        readonly MasterFunction masterFunc = new MasterFunction();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ReportMController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "ReportM");
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
        public ActionResult Show(string sel1)
        {
            // 大分類
            string categoryId = string.Empty;
            if (!string.IsNullOrEmpty(sel1))
            {
                categoryId = sel1;
            }

            // セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];

            // 画面モードの決定
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            // 登録メッセージを取得
            string registMsg = (string)Session["registMsg"];
            if (!string.IsNullOrEmpty(registMsg))
            {
                Session.Remove("registMsg");
                ViewBag.registMsg = registMsg;
            }

            // ドロップダウンリストデータ設定
            SetDropDownList();

            List<BReportM> listReport = new List<BReportM>();

            // 条件リスト
            List<string> listCondition = new List<string>()
            {
                {string.Empty},
            };

            // セッションから店舗IDを取得
            string shopId = (string)Session["SHOPID"];

            if (!string.IsNullOrEmpty(categoryId))
            {
                listCondition[0] = categoryId;

                var reportDt = from a in context.ReportMs
                               orderby a.DISPLAYNO
                               where a.SHOPID == shopId
                                  && a.CATEGORYID == categoryId
                               select a;

                if (reportDt.Count() == 0)
                {
                    BReportM bReport = new BReportM();
                    listReport.Add(bReport);
                }
                else
                {
                    foreach (var dt in reportDt)
                    {
                        BReportM bReport = new BReportM
                        {
                            // 削除
                            DelFlg = false,
                            // 大分類ID
                            CategoryId = dt.CATEGORYID,
                            // 中分類ID
                            LocationId = dt.LOCATIONID,
                            // 帳票テンプレートID
                            ReportTemplateId = dt.REPORTTEMPLATEID,
                            // 帳票ID
                            ReportId = dt.REPORTID,
                            // 帳票名称
                            ReportName = dt.REPORTNAME,
                            // 捺印欄
                            StampField = dt.STAMPFIELD == 0 ? string.Empty : dt.STAMPFIELD.ToString(),
                            // 周期
                            Period = dt.PERIOD,
                            // 基準月
                            BaseMonth = dt.BASEMONTH,
                            // 基準日（曜日）
                            ReferenceDate = dt.REFERENCEDATE,
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
                        listReport.Add(bReport);
                    }
                }
            }

            BReportMs reportVal = new BReportMs
            {
                BReportMList = listReport,
                BConditionList = listCondition,
            };

            return View(reportVal);
        }

        /// <summary>
        /// ドロップダウンリスト変更時処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <param name="conditionList">条件リスト(大分類)</param>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpPost]
        public ActionResult ConditionChange(IList<BReportM> list, List<string> conditionList)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 大分類
            string categoryId = conditionList[0];

            // セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];

            // 画面モードの決定
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            // ドロップダウンリストデータ設定
            SetDropDownList();

            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            BReportMs reportVal = new BReportMs();
            List<BReportM> listReport = new List<BReportM>();

            if (string.IsNullOrEmpty(categoryId))
            {
                reportVal.BReportMList = listReport;
                reportVal.BConditionList = conditionList;

                return View("Show", reportVal);
            }

            var reportDt = from a in context.ReportMs
                           orderby a.DISPLAYNO
                           where a.SHOPID == shopId
                              && a.CATEGORYID == categoryId
                           select a;

            if (reportDt.Count() == 0)
            {
                BReportM bReport = new BReportM();
                listReport.Add(bReport);
            }
            else
            {
                foreach (var dt in reportDt)
                {
                    BReportM bReport = new BReportM
                    {
                        // 削除
                        DelFlg = false,
                        // 大分類ID
                        CategoryId = dt.CATEGORYID,
                        // 中分類ID
                        LocationId = dt.LOCATIONID,
                        // 帳票テンプレートID
                        ReportTemplateId = dt.REPORTTEMPLATEID,
                        // 帳票ID
                        ReportId = dt.REPORTID,
                        // 帳票名称
                        ReportName = dt.REPORTNAME,
                        // 捺印欄
                        StampField = dt.STAMPFIELD == 0 ? string.Empty : dt.STAMPFIELD.ToString(),
                        // 周期
                        Period = dt.PERIOD,
                        // 基準月
                        BaseMonth = dt.BASEMONTH,
                        // 基準日（曜日）
                        ReferenceDate = dt.REFERENCEDATE,
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
                    listReport.Add(bReport);
                }
            }
            reportVal.BReportMList = listReport;
            reportVal.BConditionList = conditionList;

            return View("Show", reportVal);
        }

        /// <summary>
        /// 行追加処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <param name="conditionList">条件リスト(大分類)</param>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpPost]
        public ActionResult Add(IList<BReportM> list, List<string> conditionList)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 大分類
            string categoryId = conditionList[0];

            // セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];

            // 画面モードの決定
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            // ドロップダウンリストデータ設定
            SetDropDownList();

            BReportMs reportVal = new BReportMs();
            List<BReportM> listReport = new List<BReportM>();

            // 大分類未選択の場合は行追加不可
            if (string.IsNullOrEmpty(categoryId))
            {
                ModelState.AddModelError(string.Empty, "大分類を選択してください。");
                ModelState.AddModelError("conditionList[0]", string.Empty);

                reportVal.BReportMList = listReport;
                reportVal.BConditionList = conditionList;

                return View("Show", reportVal);
            }

            // 画面の表示順に並び替えてリストに設定
            IList<BReportM> addList = list.OrderBy(BReportM => BReportM.No).ToList();

            // 追加分
            BReportM addRow = new BReportM();

            // 行追加
            addList.Add(addRow);
            reportVal.BReportMList = addList;
            reportVal.BConditionList = conditionList;

            return View("Show", reportVal);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <param name="conditionList">条件リスト(大分類)</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<BReportM> list, List<string> conditionList)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 大分類
            string categoryId = conditionList[0];

            // セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];

            // 画面モードの決定
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            // ドロップダウンリストデータ設定
            SetDropDownList();

            BReportMs reportVal = new BReportMs();
            List<BReportM> listReport = new List<BReportM>();

            // 大分類未選択の場合は登録不可
            if (string.IsNullOrEmpty(categoryId))
            {
                ModelState.AddModelError(string.Empty, "大分類を選択してください。");
                ModelState.AddModelError("conditionList[0]", string.Empty);

                reportVal.BReportMList = listReport;
                reportVal.BConditionList = conditionList;

                return View("Show", reportVal);
            }

            // 画面の表示順に並び替えてリストに設定
            List<BReportM> updList = list.OrderBy(BReportM => BReportM.No).ToList();

            // セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            BReportMs reportData = new BReportMs
            {
                BReportMList = updList,
                BConditionList = conditionList,
            };

            // 削除用データ
            var delReportMs = new List<ReportM>();
            // 登録データ
            var insReportMs = new List<ReportM>();
            // 更新データ
            var updReportMs = new List<ReportM>();
            // 表示順 DB登録用
            int rowNo = 1;
            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            //中分類ID、帳票テンプレートIDに重複がないか確認
            if (!CheckDuplicated(updList, ref hsError))
            {
                checkError = false;
            }

            // 更新チェック用現在データ取得
            var reportDt = from a in context.ReportMs
                           orderby a.DISPLAYNO
                           where a.SHOPID == shopId
                           select a;

            // 変更データ抽出（ソート番号以外）
            foreach (BReportM dt in updList)
            {
                dt.ShopId = shopId;
                dt.CategoryId = categoryId;

                // 追加行（帳票IDなし）で、削除checkあり or 入力項目がすべて未入力の場合は無視
                if (string.IsNullOrEmpty(dt.ReportId)
                    && (dt.DelFlg
                        || (string.IsNullOrEmpty(dt.LocationId)
                            && string.IsNullOrEmpty(dt.ReportName)
                            && string.IsNullOrEmpty(dt.ReportTemplateId)
                            && string.IsNullOrEmpty(dt.StampField)
                            && string.IsNullOrEmpty(dt.Period)
                            && string.IsNullOrEmpty(dt.BaseMonth)
                            && string.IsNullOrEmpty(dt.ReferenceDate))))
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
                        delReportMs.Add(this.SetReportM(dt, dt.DisplayNo, managerId));
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
                if (string.IsNullOrEmpty(dt.ReportId))
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                    }
                    // 新規
                    insReportMs.Add(this.SetReportM(dt, rowNo, managerId, managerId));
                    rowNo++;
                    continue;
                }

                // DBデータから取得
                List<ReportM> registData = new List<ReportM>(reportDt.Where(a => a.REPORTID == dt.ReportId));

                // DBにデータが存在しない場合
                if (registData.Count == 0)
                {
                    // データが更新されているため、排他エラーとする
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    return View("Show", reportData);
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
                updReportMs.Add(this.SetReportM(dt, rowNo, managerId));
                rowNo++;
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                return View("Show", reportData);
            }

            // DB更新
            if (delReportMs.Count > 0 || updReportMs.Count > 0 || insReportMs.Count > 0)
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
                            if (delReportMs.Count > 0)
                            {
                                foreach (ReportM deldata in delReportMs)
                                {
                                    context.ReportMs.Attach(deldata);
                                }
                                context.ReportMs.RemoveRange(delReportMs);
                                context.SaveChanges();
                            }

                            // データ登録
                            foreach (ReportM insdata in insReportMs)
                            {
                                // 帳票IDを採番
                                insdata.REPORTID = comm.GetNumberingID(
                                    context: context, tableName: "REPORT_M", columnName: "REPORTID", shopId: insdata.SHOPID, digits: 3);
                                // データ登録
                                context.ReportMs.Add(insdata);
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updReportMs.Count > 0)
                            {
                                foreach (ReportM upddata in updReportMs)
                                {
                                    context.ReportMs.Attach(upddata);
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
                            return View("Show", reportData);
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                            {
                                //一意制約エラー
                                // ロールバック
                                tran.Rollback();
                                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                                return View("Show", reportData);
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

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);

            // 条件選択状態での初期表示に戻す
            reportData.BReportMList = new List<BReportM>();
            return RedirectToAction("Show", new { @sel1 = categoryId });
        }

        /// <summary>
        /// 帳票テンプレートデータ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <returns>帳票テンプレートマスタデータ</returns>
        private List<ReportTemplateM> GetReportTemplateMData(string shopId)
        {
            // データ取得
            var reportTemplateDt = from a in context.ReportTemplateMs
                                   orderby a.SHOPID, a.TEMPLATEID
                                   where a.SHOPID == shopId
                                      || a.SHOPID == ReportTemplateCommonData.SHOPID
                                   select a;

            List<ReportTemplateM> reportTemplateMList = reportTemplateDt.ToArray().ToList();

            return reportTemplateMList;
        }

        /// <summary>
        /// 帳票テンプレートドロップダウンリスト用選択オプション生成
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private SelectListItem[] CreateReportTemplateMOptionList(List<ReportTemplateM> reportTemplateMList)
        {
            SelectListItem[] selectOptions = new SelectListItem[reportTemplateMList.Count()];
            int key = 0;
            reportTemplateMList.ForEach(a =>
            {
                selectOptions[key] = new SelectListItem() { Value = a.TEMPLATEID, Text = a.TEMPLATENAME };
                key++;
            });

            return selectOptions;
        }

        /// <summary>
        /// 画面用データからモデル用データへ移送(登録ユーザーなし)
        /// </summary>
        /// <param name="breportm">画面用データ</param>
        /// <param name="orderNo">画面表示順</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns>モデル用データ</returns>
        private ReportM SetReportM(BReportM breportm, int orderNo, string updUserId)
        {
            return SetReportM(breportm, orderNo, string.Empty, updUserId);
        }

        /// <summary>
        /// 画面用データからモデル用データへ移送
        /// </summary>
        /// <param name="breportm">画面用データ</param>
        /// <param name="orderNo">画面表示順</param>
        /// <param name="insUserId">登録ユーザーID</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns>モデル用データ</returns>
        private ReportM SetReportM(BReportM breportm, int orderNo, string insUserId, string updUserId)
        {
            var model = new ReportM
            {
                // 店舗ID
                SHOPID = breportm.ShopId,
                // 大分類ID
                CATEGORYID = breportm.CategoryId,
                // 中分類ID
                LOCATIONID = breportm.LocationId,
                // 帳票テンプレートID
                REPORTTEMPLATEID = breportm.ReportTemplateId,
                // 帳票ID
                REPORTID = breportm.ReportId,
                // 帳票名称
                REPORTNAME = breportm.ReportName,
                // 捺印欄
                STAMPFIELD = (string.IsNullOrEmpty(breportm.StampField)) ? (short)0 : short.Parse(breportm.StampField),
                // 周期
                PERIOD = breportm.Period,
                // 基準月
                BASEMONTH = breportm.BaseMonth,
                // 基準日（曜日）
                REFERENCEDATE = breportm.ReferenceDate,
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
                model.INSUSERID = breportm.InsUserId;
                // 更新年月日
                if (breportm.UpdDate != null)
                {
                    model.UPDDATE = DateTime.Parse(breportm.UpdDate);
                }
            }

            return model;
        }

        /// <summary>
        /// データ関連チェック
        /// </summary>
        /// <param name="breportm">画面用データ</param>
        /// <returns>チェック結果</returns>
        private bool CheckRelation(BReportM breportm, string updelStr)
        {
            bool check = true;
            var comm = new MasterFunction();

            var approvalDt = comm.IsDataApproval(context: context, shopId: breportm.ShopId, categoryId: breportm.CategoryId, locationId: breportm.LocationId, reportId: breportm.ReportId);
            // 中分類承認情報・大分類承認情報・店舗承認情報
            if (approvalDt)
            {
                ModelState.AddModelError(string.Empty, string.Format(MsgConst.RELERR_APPROVE_MSG, updelStr, "帳票ID", breportm.ReportId));
                check = false;
            }

            return check;
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="dt">画面入力データ</param>
        /// <param name="hsError"></param>
        /// <returns>判定結果</returns>
        private bool CheckRequire(BReportM dt, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 入力項目のチェック
            if (string.IsNullOrEmpty(dt.LocationId))
            {
                hsError.Add("中分類名を選択してください。");
                ModelState.AddModelError("list[" + dt.No + "].LocationId", string.Empty);
                checkError = false;
            }
            if (string.IsNullOrEmpty(dt.ReportName))
            {
                hsError.Add("帳票名を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].ReportName", string.Empty);
                checkError = false;
            }
            else if (dt.ReportName.Length > 20)
            {
                hsError.Add("帳票名は20文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].ReportName", string.Empty);
                checkError = false;
            }
            if (string.IsNullOrEmpty(dt.ReportTemplateId))
            {
                hsError.Add("帳票タイプを選択してください。");
                ModelState.AddModelError("list[" + dt.No + "].ReportTemplateId", string.Empty);
                checkError = false;
            }
            if (string.IsNullOrEmpty(dt.StampField))
            {
                hsError.Add("捺印欄数を選択してください。");
                ModelState.AddModelError("list[" + dt.No + "].StampField", string.Empty);
                checkError = false;
            }
            if (string.IsNullOrEmpty(dt.Period))
            {
                hsError.Add("周期を選択してください。");
                ModelState.AddModelError("list[" + dt.No + "].Period", string.Empty);
                checkError = false;
            }
            else
            {
                if (IsNeedBaseMonth(dt.Period))
                {
                    if (string.IsNullOrEmpty(dt.BaseMonth))
                    {
                        hsError.Add("基準月を選択してください。");
                        ModelState.AddModelError("list[" + dt.No + "].BaseMonth", string.Empty);
                        checkError = false;
                    }
                }
                if (IsNeedReferenceDate(dt.Period))
                {
                    if (string.IsNullOrEmpty(dt.ReferenceDate))
                    {
                        hsError.Add("基準日（曜日）を選択してください。");
                        ModelState.AddModelError("list[" + dt.No + "].ReferenceDate", string.Empty);
                        checkError = false;
                    }
                }

                //周期：3ヶ月、6ヶ月
                //基準月：2  →　30,31
                //        4  →　31
                //        6  →　31
                //        9  →　31
                //        11 →　31
                //を選択した場合には入力エラーとする。
                //エラーメッセージ「基準日の指定に誤りがあります。」
                if (dt.Period == PERIOD.THREEMONTH || dt.Period == PERIOD.SIXMONTH)
                {
                    if ("2".Equals(dt.BaseMonth) && ("30".Equals(dt.ReferenceDate) || "31".Equals(dt.ReferenceDate)))
                    {
                        checkError = false;
                    }
                    else if (("4".Equals(dt.BaseMonth)
                            || "6".Equals(dt.BaseMonth)
                            || "9".Equals(dt.BaseMonth)
                            || "11".Equals(dt.BaseMonth)
                            ) && "31".Equals(dt.ReferenceDate))
                    {
                        checkError = false;
                    }
                    if (!checkError)
                    {
                        hsError.Add("基準日の指定に誤りがあります。");
                        ModelState.AddModelError("list[" + dt.No + "].ReferenceDate", string.Empty);
                    }
                }

            }

            return checkError;
        }

        /// <summary>
        /// 更新項目結果チェック
        /// </summary>
        /// <param name="registData">DBデータ</param>
        /// <param name="dt">画面データ</param>
        /// <returns>bool（true:更新あり、false：更新なし）</returns>
        private bool CheckDataUpd(List<ReportM> registData, BReportM dt)
        {
            return (registData[0].CATEGORYID != dt.CategoryId
                || registData[0].LOCATIONID != dt.LocationId
                || registData[0].REPORTTEMPLATEID != dt.ReportTemplateId
                || registData[0].REPORTID != dt.ReportId
                || registData[0].REPORTNAME != dt.ReportName
                || registData[0].STAMPFIELD != (string.IsNullOrEmpty(dt.StampField) ? 0 : short.Parse(dt.StampField))
                || registData[0].PERIOD != dt.Period
                || registData[0].BASEMONTH != dt.BaseMonth
                || registData[0].REFERENCEDATE != dt.ReferenceDate);
        }

        /// <summary>
        /// 基準月入力必須チェック
        /// </summary>
        /// <param name="period">周期</param>
        /// <returns>bool（true:必要、false：不要）</returns>
        private bool IsNeedBaseMonth(string period)
        {
            return (PERIOD_BASEMONTH_NEED[period]);
        }

        /// <summary>
        /// 基準日（曜日）入力必須チェック
        /// </summary>
        /// <param name="period">周期</param>
        /// <returns>bool（true:必要、false：不要）</returns>
        private bool IsNeedReferenceDate(string period)
        {
            return (PERIOD_REFERENCEDATE_NEED[period]);
        }

        /// <summary>
        /// ドロップダウンリストデータ設定
        /// </summary>
        /// <returns>void</returns>
        private void SetDropDownList()
        {
            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // ドロップダウンリスト用データ取得設定
            // 大分類ドロップダウンリスト
            List<CategoryM> categoryMList = masterFunc.GetCategoryMData(context, shopId);
            if (categoryMList.Count() == 0)
            {
                // 大分類データが存在しない場合メッセージを表示
                ModelState.AddModelError(string.Empty, MsgConst.NODATA_CATEGORY);
                ViewBag.editMode = "disabled";
            }
            ViewBag.categoryMSelectListItem = comFunc.CreateCategoryMOptionList(categoryMList);
            // 中分類ドロップダウンリスト
            List<LocationM> locationMList = masterFunc.GetLocationMData(context, shopId);
            if (locationMList.Count() == 0)
            {
                // 中分類データが存在しない場合メッセージを表示
                ModelState.AddModelError(string.Empty, MsgConst.NODATA_LOCATION);
                ViewBag.editMode = "disabled";
            }
            ViewBag.locationMSelectListItem = comFunc.CreateLocationMOptionList(locationMList);
            // 帳票テンプレートドロップダウンリスト
            List<ReportTemplateM> reportTemplateMList = this.GetReportTemplateMData(shopId);
            if (reportTemplateMList.Count() == 0)
            {
                // 帳票テンプレートデータが存在しない場合メッセージを表示
                ModelState.AddModelError(string.Empty, "帳票テンプレートデータが存在しません。サービスセンターまでご連絡ください。");
                ViewBag.editMode = "disabled";
            }
            ViewBag.reportTemplateMSelectListItem = this.CreateReportTemplateMOptionList(reportTemplateMList);
            // 捺印数ドロップダウンリスト
            ViewBag.stampFieldSelectListItem = comFunc.CreateOptionList(OPTIONLIST_STAMPFIELD);
            // 周期ドロップダウンリスト
            ViewBag.periodSelectListItem = comFunc.CreateOptionList(OPTIONLIST_PERIOD);
            // 基準月ドロップダウンリスト
            ViewBag.baseMonthSelectListItem = comFunc.CreateOptionList(OPTIONLIST_BASEMONTH);
            // 基準日ドロップダウンリスト（周期：1週間）
            ViewBag.referenceDateSelectListItemOneWeek = comFunc.CreateOptionList(OPTIONLIST_REFERENCEDATE_ONEWEEK);
            // 基準日ドロップダウンリスト（周期：1ヶ月）
            ViewBag.referenceDateSelectListItemOneMonth = comFunc.CreateOptionList(OPTIONLIST_REFERENCEDATE_ONEMONTH);
            // 基準日ドロップダウンリスト（周期：数ヶ月）
            ViewBag.referenceDateSelectListItemSeveralMonth = comFunc.CreateOptionList(OPTIONLIST_REFERENCEDATE_SEVERALMONTH);
        }

        /// <summary>
        /// オプションリスト表示用周期
        /// </summary>
        private static readonly Dictionary<string, string> OPTIONLIST_PERIOD = new Dictionary<string, string>()
        {
            // 周期（値）, 周期（表示値）
            {PERIOD.ONEDAY, PERIOD.ONEDAY_W},
            {PERIOD.ONEWEEK, PERIOD.ONEWEEK_W},
            {PERIOD.ONEMONTH, PERIOD.ONEMONTH_W},
            {PERIOD.THREEMONTH, PERIOD.THREEMONTH_W},
            {PERIOD.SIXMONTH, PERIOD.SIXMONTH_W},
        };

        /// <summary>
        /// 周期に対する基準月入力要否
        /// </summary>
        private static readonly Dictionary<string, bool> PERIOD_BASEMONTH_NEED = new Dictionary<string, bool>()
        {
            // 周期, 基準月入力要否（true:必要、false：不要）
            {PERIOD.ONEDAY, false},
            {PERIOD.ONEWEEK, false},
            {PERIOD.ONEMONTH, false},
            {PERIOD.THREEMONTH, true},
            {PERIOD.SIXMONTH, true},
        };

        /// <summary>
        /// 周期に対する基準日（曜日）入力要否
        /// </summary>
        private static readonly Dictionary<string, bool> PERIOD_REFERENCEDATE_NEED = new Dictionary<string, bool>()
        {
            // 周期, 基準日（曜日）入力要否（true:必要、false：不要）
            {PERIOD.ONEDAY, false},
            {PERIOD.ONEWEEK, true},
            {PERIOD.ONEMONTH, true},
            {PERIOD.THREEMONTH, true},
            {PERIOD.SIXMONTH, true},
        };

        /// <summary>
        /// 中分類ID、帳票テンプレートIDに重複がないか確認
        /// </summary>
        /// <param name="updList">画面入力値</param>
        /// <param name="hsError">エラー</param>
        /// <returns></returns>
        private bool CheckDuplicated(List<BReportM> updList, ref HashSet<string> hsError)
        {
            //重複が確認された場合falseに
            bool isDuplicated = true;
            //[中分類ID:帳票テンプレートID]を格納したList
            var checkList = new List<string>();
            foreach (BReportM item in updList)
            {
                checkList.Add(item.LocationId + ":" + item.ReportTemplateId);
            }

            //総当たりで重複が無いかを確認
            for (int i = 0; i < updList.Count; i++)
            {
                for (int j = 0; j < updList.Count; j++)
                {
                    if (checkList[i].Equals(checkList[j]) && i != j)
                    {
                        isDuplicated = false;
                        hsError.Add("中分類名,帳票タイプが重複しています。");
                        ModelState.AddModelError("list[" + i + "].LocationId", string.Empty);
                        ModelState.AddModelError("list[" + i + "].ReportTemplateId", string.Empty);
                    }
                }
            }
            return isDuplicated;
        }
    }
}