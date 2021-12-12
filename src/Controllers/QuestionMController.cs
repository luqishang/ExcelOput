using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using static HACCPExtender.Controllers.Common.CommonConstants;
using Microsoft.VisualBasic;
using HACCPExtender.Models.Bussiness;
using HACCPExtender.Business;

namespace HACCPExtenfer.Controllers
{
    public class QuestionMController : Controller
    {
        // コンテキスト
        private MasterContext context = new MasterContext();
        // 共通処理
        readonly CommonFunction comFunc = new CommonFunction();
        readonly MasterFunction masterFunc = new MasterFunction();
        // 設問マスタ登録最大数
        private static readonly int QUESTION_REGIST_NUM_MAX = int.Parse(GetAppSet.GetAppSetValue("Question", "Max"));
        // 回答区分による入力タイプ（小数）
        private static readonly string[] ANSWERKBN_INPUTTYPE_DECIMALS = { "04", "05", "09" };
        // 回答区分による入力タイプ（整数）
        private static readonly string[] ANSWERKBN_INPUTTYPE_INTEGER = { "08" };

        /// <summary>
        /// コンテキスト
        /// </summary>
        public QuestionMController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "QuestionM");
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
        public ActionResult Show(string sel1, string sel2, string sel3)
        {
            // 大分類
            string categoryId = string.Empty;
            if (!string.IsNullOrEmpty(sel1))
            {
                categoryId = sel1;
            }
            // 中分類
            string locationId = string.Empty;
            if (!string.IsNullOrEmpty(sel2))
            {
                locationId = sel2;
            }
            // 帳票
            string reportId = string.Empty;
            if (!string.IsNullOrEmpty(sel3))
            {
                reportId = sel3;
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
            SetDropDownList(categoryId, locationId);

            List<BQuestionM> listQuestion = new List<BQuestionM>();

            // 条件リスト
            List<string> listCondition = new List<string>()
            {
                {string.Empty},
                {string.Empty},
                {string.Empty},
            };

            // セッションから店舗IDを取得
            string shopId = (string)Session["SHOPID"];

            if (!string.IsNullOrEmpty(categoryId))
            {
                listCondition[0] = categoryId;
            }
            if (!string.IsNullOrEmpty(locationId))
            {
                listCondition[1] = locationId;
            }
            if (!string.IsNullOrEmpty(reportId))
            {
                listCondition[2] = reportId;
            }
            if (!string.IsNullOrEmpty(categoryId) && !string.IsNullOrEmpty(locationId) && !string.IsNullOrEmpty(reportId))
            {
                var questionDt = from a in context.QuestionMs
                                 orderby a.DISPLAYNO
                                 where a.SHOPID == shopId
                                    && a.REPORTID == reportId
                                    && a.CATEGORYID == categoryId
                                    && a.LOCATIONID == locationId
                                    && a.DELETEFLAG == DeleteFlg.NODELETE
                                 select a;

                if (questionDt.Count() == 0)
                {
                    BQuestionM bQuestion = new BQuestionM();
                    listQuestion.Add(bQuestion);
                }
                else
                {
                    foreach (var dt in questionDt)
                    {
                        BQuestionM bQuestion = new BQuestionM
                        {
                            // 削除
                            DelFlg = false,
                            // 帳票ID
                            ReportId = dt.REPORTID,
                            // 大分類ID
                            CategoryId = dt.CATEGORYID,
                            // 中分類ID
                            LocationId = dt.LOCATIONID,
                            // 設問ID
                            QuestionId = dt.QUESTIONID,
                            // 設問
                            Question = dt.QUESTION,
                            // 設問（英語表記）
                            QuestionEng = dt.QUESTIONENG,
                            // 回答種類ID
                            AnswerTypeId = dt.ANSWERTYPEID,
                            // 正常結果条件
                            NormalCondition = dt.NORMALCONDITION,
                            // 正常結果条件値１
                            NormalCondition1 = dt.NORMALCONDITION1,
                            // 正常結果条件値２
                            NormalCondition2 = dt.NORMALCONDITION2,
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
                        listQuestion.Add(bQuestion);
                    }
                }
            }

            BQuestionMs questionVal = new BQuestionMs
            {
                BQuestionMList = listQuestion,
                BConditionList = listCondition,
            };

            return View(questionVal);
        }

        /// <summary>
        /// ドロップダウンリスト変更時処理
        /// </summary>
        /// <param name="conditionList">条件リスト(大分類)</param>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpPost]
        public ActionResult ConditionChange(List<string> conditionList)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 大分類
            string categoryId = conditionList[0];
            // 中分類
            string locationId = conditionList[1];
            // 帳票
            string reportId = conditionList[2];

            // セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];

            // 画面モードの決定
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            // ドロップダウンリストデータ設定
            SetDropDownList(categoryId, locationId);

            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            BQuestionMs questionVal = new BQuestionMs();
            List<BQuestionM> listQuestion = new List<BQuestionM>();

            if (string.IsNullOrEmpty(categoryId) || string.IsNullOrEmpty(locationId) || string.IsNullOrEmpty(reportId))
            {
                questionVal.BQuestionMList = listQuestion;
                questionVal.BConditionList = conditionList;

                return View("Show", questionVal);
            }

            var questionDt = from a in context.QuestionMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                                && a.REPORTID == reportId
                                && a.CATEGORYID == categoryId
                                && a.LOCATIONID == locationId
                                && a.DELETEFLAG == DeleteFlg.NODELETE
                             select a;

            if (questionDt.Count() == 0)
            {
                BQuestionM bQuestion = new BQuestionM();
                listQuestion.Add(bQuestion);
            }
            else
            {
                foreach (var dt in questionDt)
                {
                    BQuestionM bQuestion = new BQuestionM
                    {
                        // 削除
                        DelFlg = false,
                        // 帳票ID
                        ReportId = dt.REPORTID,
                        // 大分類ID
                        CategoryId = dt.CATEGORYID,
                        // 中分類ID
                        LocationId = dt.LOCATIONID,
                        // 設問ID
                        QuestionId = dt.QUESTIONID,
                        // 設問
                        Question = dt.QUESTION,
                        // 設問（英語表記）
                        QuestionEng = dt.QUESTIONENG,
                        // 回答種類ID
                        AnswerTypeId = dt.ANSWERTYPEID,
                        // 正常結果条件
                        NormalCondition = dt.NORMALCONDITION,
                        // 正常結果条件値１
                        NormalCondition1 = dt.NORMALCONDITION1,
                        // 正常結果条件値２
                        NormalCondition2 = dt.NORMALCONDITION2,
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
                    listQuestion.Add(bQuestion);
                }
            }
            questionVal.BQuestionMList = listQuestion;
            questionVal.BConditionList = conditionList;

            return View("Show", questionVal);
        }

        /// <summary>
        /// 行追加処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <param name="conditionList">条件リスト(大分類)</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add(IList<BQuestionM> list, List<string> conditionList)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 大分類
            string categoryId = conditionList[0];
            // 中分類
            string locationId = conditionList[1];
            // 帳票
            string reportId = conditionList[2];

            // セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];

            // 画面モードの決定
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            // ドロップダウンリストデータ設定
            SetDropDownList(categoryId, locationId);

            BQuestionMs questionVal = new BQuestionMs();
            List<BQuestionM> listQuestion = new List<BQuestionM>();

            // 大分類・中分類・帳票未選択の場合は行追加不可
            if (string.IsNullOrEmpty(categoryId) || string.IsNullOrEmpty(locationId) || string.IsNullOrEmpty(reportId))
            {
                if (string.IsNullOrEmpty(categoryId))
                {
                    ModelState.AddModelError(string.Empty, "大分類を選択してください。");
                    ModelState.AddModelError("conditionList[0]", string.Empty);
                }
                if (string.IsNullOrEmpty(locationId))
                {
                    ModelState.AddModelError(string.Empty, "中分類を選択してください。");
                    ModelState.AddModelError("conditionList[1]", string.Empty);
                }
                if (string.IsNullOrEmpty(reportId))
                {
                    ModelState.AddModelError(string.Empty, "帳票を選択してください。");
                    ModelState.AddModelError("conditionList[2]", string.Empty);
                }

                questionVal.BQuestionMList = listQuestion;
                questionVal.BConditionList = conditionList;

                return View("Show", questionVal);
            }

            // 画面の表示順に並び替えてリストに設定
            IList<BQuestionM> addList = list.OrderBy(BQuestionM => BQuestionM.No).ToList();

            // 追加分
            BQuestionM addRow = new BQuestionM();

            // 行追加
            addList.Add(addRow);
            questionVal.BQuestionMList = addList;
            questionVal.BConditionList = conditionList;

            return View("Show", questionVal);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <param name="conditionList">条件リスト(大分類)</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<BQuestionM> list, List<string> conditionList)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 大分類
            string categoryId = conditionList[0];
            // 中分類
            string locationId = conditionList[1];
            // 帳票
            string reportId = conditionList[2];

            // セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];

            // 画面モードの決定
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            // ドロップダウンリストデータ設定
            SetDropDownList(categoryId, locationId);

            BQuestionMs questionVal = new BQuestionMs();
            List<BQuestionM> listQuestion = new List<BQuestionM>();

            // 大分類・中分類・帳票未選択の場合は登録不可
            if (string.IsNullOrEmpty(categoryId) || string.IsNullOrEmpty(locationId) || string.IsNullOrEmpty(reportId))
            {
                if (string.IsNullOrEmpty(categoryId))
                {
                    ModelState.AddModelError(string.Empty, "大分類を選択してください。");
                    ModelState.AddModelError("conditionList[0]", string.Empty);
                }
                if (string.IsNullOrEmpty(locationId))
                {
                    ModelState.AddModelError(string.Empty, "中分類を選択してください。");
                    ModelState.AddModelError("conditionList[1]", string.Empty);
                }
                if (string.IsNullOrEmpty(reportId))
                {
                    ModelState.AddModelError(string.Empty, "帳票を選択してください。");
                    ModelState.AddModelError("conditionList[2]", string.Empty);
                }

                questionVal.BQuestionMList = listQuestion;
                questionVal.BConditionList = conditionList;

                return View("Show", questionVal);
            }

            // 画面の表示順に並び替えてリストに設定
            List<BQuestionM> updList = list.OrderBy(BQuestionM => BQuestionM.No).ToList();

            // セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            BQuestionMs questionData = new BQuestionMs
            {
                BQuestionMList = updList,
                BConditionList = conditionList,
            };

            // 登録データ
            var insQuestionMs = new List<QuestionM>();
            // 更新データ
            var updQuestionMs = new List<QuestionM>();
            // 表示順 DB登録用
            int rowNo = 1;
            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();
            //テキストエリア数カウント用
            int textAreaIndex = 0;

            // 更新チェック用現在データ取得
            var questionDt = from a in context.QuestionMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                                && a.REPORTID == reportId
                                && a.CATEGORYID == categoryId
                                && a.LOCATIONID == locationId
                                && a.DELETEFLAG == DeleteFlg.NODELETE
                             select a;

            // 変更データ抽出（ソート番号以外）
            foreach (BQuestionM dt in updList)
            {
                dt.ShopId = shopId;
                dt.ReportId = reportId;
                dt.CategoryId = categoryId;
                dt.LocationId = locationId;

                // 追加行（設問IDなし）で、削除checkあり or 入力項目がすべて未入力の場合は無視
                if (string.IsNullOrEmpty(dt.QuestionId)
                    && (dt.DelFlg
                        || (string.IsNullOrEmpty(dt.Question)
                            && string.IsNullOrEmpty(dt.QuestionEng)
                            && string.IsNullOrEmpty(dt.AnswerTypeId)
                            && string.IsNullOrEmpty(dt.NormalCondition)
                            && string.IsNullOrEmpty(dt.NormalCondition1)
                            && string.IsNullOrEmpty(dt.NormalCondition2))))
                {
                    continue;
                }

                // DBデータから取得
                List<QuestionM> registData = new List<QuestionM>(questionDt.Where(a => a.QUESTIONID == dt.QuestionId));

                // 削除checkありの場合は入力内容無視
                if (dt.DelFlg)
                {
                    if (registData.Count == 0)
                    {
                        // データが更新されているため、排他エラーとする
                        ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                        return View("Show", questionData);
                    }
                    else
                    {
                        // 削除(論理削除フラグ更新)に追加
                        updQuestionMs.Add(this.SetQuestionMDel(registData[0], managerId));
                    }

                    rowNo++;
                    continue;
                }
                //帳票内の表示方法にテキストエリアが2つ以上選択されている場合
                if (!CheckTextArea(dt.AnswerTypeId, ref textAreaIndex))
                {
                    hsError.Add("1帳票内にテキストエリアを2個以上指定できません。");
                    ModelState.AddModelError("list[" + dt.No + "].AnswerTypeId", string.Empty);
                    checkError = false;
                }

                // 新規追加行
                if (string.IsNullOrEmpty(dt.QuestionId))
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                    }
                    // 新規
                    insQuestionMs.Add(this.SetQuestionM(dt, rowNo, managerId, managerId));
                    rowNo++;
                    continue;
                }

                // DBにデータが存在しない場合
                if (registData.Count == 0)
                {
                    // データが更新されているため、排他エラーとする
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    return View("Show", questionData);
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
                updQuestionMs.Add(this.SetQuestionM(dt, rowNo, managerId));
                rowNo++;
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                return View("Show", questionData);
            }

            // DB更新
            if (updQuestionMs.Count > 0 || insQuestionMs.Count > 0)
            {
                using (context = new MasterContext())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // マスタ共通処理
                            var comm = new MasterFunction();

                            // データ登録
                            foreach (QuestionM insdata in insQuestionMs)
                            {
                                // 設問IDを採番
                                insdata.QUESTIONID = comm.GetNumberingID(
                                    context: context, tableName: "QUESTION_M", columnName: "QUESTIONID", shopId: insdata.SHOPID, reportId: insdata.REPORTID, digits: 2);
                                // データ登録
                                context.QuestionMs.Add(insdata);
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updQuestionMs.Count > 0)
                            {
                                foreach (QuestionM upddata in updQuestionMs)
                                {
                                    context.QuestionMs.Attach(upddata);
                                    context.Entry(upddata).State = EntityState.Modified;
                                }
                                context.SaveChanges();
                            }

                            // 競合がない場合、最終的な件数チェック
                            // DBデータから取得
                            var afterDt = from a in context.QuestionMs
                                          orderby a.DISPLAYNO
                                          where a.SHOPID == shopId
                                             && a.REPORTID == reportId
                                             && a.CATEGORYID == categoryId
                                             && a.LOCATIONID == locationId
                                             && a.DELETEFLAG == DeleteFlg.NODELETE
                                          select a;
                            List<QuestionM> afterData = new List<QuestionM>(afterDt);

                            // 最大登録件数を超える場合
                            if (QUESTION_REGIST_NUM_MAX < afterData.Count)
                            {
                                ModelState.AddModelError(string.Empty, QUESTION_REGIST_NUM_MAX + "件以内で登録してください。このまま登録した場合の件数=[" + afterData.Count + "]");
                                ModelState.AddModelError(string.Empty, "件数が異なる場合は画面を更新して再度、登録してください。");
                            }
                            if (!ModelState.IsValid)
                            {
                                // ロールバック
                                tran.Rollback();
                                return View("Show", questionData);
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
                            return View("Show", questionData);
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                            {
                                //一意制約エラー
                                // ロールバック
                                tran.Rollback();
                                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                                return View("Show", questionData);
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
            questionData.BQuestionMList = new List<BQuestionM>();
            return RedirectToAction("Show", new { @sel1 = categoryId, @sel2 = locationId, @sel3 = reportId });
        }

        /// <summary>
        /// 初期ログイン時設定
        /// </summary>
        /// <returns>初期表示</returns>
        [HttpGet]
        public ActionResult InitialSetQuestion()
        {
            //店舗ID
            string shopId = (string)Session["SHOPID"];
            // フォーマット系列店舗ID
            string formatShopId = (string)Session["FORMATSHOPID"];

            // 大分類IDマップ
            var categoryDic = new Dictionary<string, string>();
            // 中分類IDマップ
            var locationDic = new Dictionary<string, string>();

            // 大分類マスタ
            var categoryDt = from ca in context.CategoryMs
                             where ca.SHOPID == shopId
                             select ca;
            foreach (CategoryM category in categoryDt)
            {
                if (!categoryDic.ContainsKey(category.CATEGORYID))
                {
                    categoryDic.Add(category.CATEGORYID, category.CATEGORYID);
                }
            }

            // 中分類マスタ
            var locationDt = from lo in context.LocationMs
                             where lo.SHOPID == shopId
                             select lo;
            foreach (LocationM location in locationDt)
            {
                if (!locationDic.ContainsKey(location.LOCATIONID))
                {
                    locationDic.Add(location.LOCATIONID, location.LOCATIONID);
                }
            }

            // 帳票テンプレートリスト（登録用）
            var reportTemplateList = new List<ReportTemplateM>();
            // 帳票マスタデータリスト（登録用）
            var reportList = new List<ReportM>();
            // 設問マスタデータリスト（登録用）
            var questionList = new List<QuestionM>();

            // 帳票テンプレートマスタ
            var reportTemplateM = from repT in context.ReportTemplateMs
                                  where repT.SHOPID == formatShopId
                                  select repT;

            if (reportTemplateM.Count() > 0)
            {
                foreach (ReportTemplateM template in reportTemplateM)
                {
                    template.SHOPID = shopId;
                    template.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    template.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    reportTemplateList.Add(template);
                }
            }

            // 帳票マスタ
            var reportM = from re in context.ReportMs
                          where re.SHOPID == formatShopId
                          select re;

            if (reportM.Count() > 0)
            {
                foreach (ReportM report in reportM)
                {
                    // 各マスタに部門ID、場所IDが存在する場合
                    if (categoryDic.ContainsKey(report.CATEGORYID)
                        && locationDic.ContainsKey(report.LOCATIONID))
                    {
                        report.SHOPID = shopId;                         // 店舗ID
                        report.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;   // 登録ユーザーID
                        report.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;   // 更新ユーザーID
                        reportList.Add(report);
                    }
                }
            }

            if (reportList.Count() > 0)
            {
                // 設問マスタ
                var questionMDt = from qu in context.QuestionMs
                                  where qu.SHOPID == formatShopId
                                    && qu.DELETEFLAG == DeleteFlg.NODELETE
                                  select qu;

                if (questionMDt.Count() > 0)
                {
                    foreach (QuestionM question in questionMDt)
                    {
                        // 各マスタに部門ID、場所IDが存在する場合
                        if (categoryDic.ContainsKey(question.CATEGORYID)
                            && locationDic.ContainsKey(question.LOCATIONID))
                        {
                            question.SHOPID = shopId;                         // 店舗ID
                            question.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;   // 登録ユーザーID
                            question.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;   // 更新ユーザーID
                            questionList.Add(question);
                        }
                    }
                }
            }

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 帳票テンプレートマスタ
                        if (reportTemplateList.Count() > 0)
                        {
                            foreach (ReportTemplateM insModel in reportTemplateList)
                            {
                                // データ登録
                                context.ReportTemplateMs.Add(insModel);
                            }
                            context.SaveChanges();
                        }

                        // 帳票マスタ
                        if (reportList.Count() > 0)
                        {
                            foreach (ReportM insModel in reportList)
                            {
                                // データ登録
                                context.ReportMs.Add(insModel);
                            }
                            context.SaveChanges();
                        }

                        // 設問マスタ
                        if (questionList.Count() > 0)
                        {
                            foreach (QuestionM insModel in questionList)
                            {
                                // データ登録
                                context.QuestionMs.Add(insModel);
                            }
                            context.SaveChanges();
                        }

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
        /// 初回ログイン 設問マスタ遷移
        /// </summary>
        /// <returns>ResultViewオブジェクト</returns>
        [HttpPost]
        public ActionResult InitialSetWorker()
        {
            return RedirectToAction("Show", "WorkerM");
        }

        /// <summary>
        /// 帳票マスタデータ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <returns>帳票マスタデータ</returns>
        private List<ReportM> GetReportMData(string shopId, string categoryId, string locationId)
        {
            // データ取得
            var reportDt = from a in context.ReportMs
                           orderby a.DISPLAYNO
                           where a.SHOPID == shopId
                              && a.CATEGORYID == categoryId
                              && a.LOCATIONID == locationId
                           select a;

            List<ReportM> reportMList = reportDt.ToArray().ToList();

            return reportMList;
        }

        /// <summary>
        /// 帳票ドロップダウンリスト用選択オプション生成
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private SelectListItem[] CreateReportMOptionList(List<ReportM> reportMList)
        {
            SelectListItem[] selectOptions = new SelectListItem[reportMList.Count()];
            int key = 0;
            reportMList.ForEach(a =>
            {
                selectOptions[key] = new SelectListItem() { Value = a.REPORTID, Text = a.REPORTNAME };
                key++;
            });

            return selectOptions;
        }

        /// <summary>
        /// 回答種類マスタデータ取得
        /// </summary>
        /// <returns>回答種類マスタデータ</returns>
        private List<AnswerTypeM> GetAnswerTypeMData()
        {
            string shopId = (string)Session["SHOPID"];
            // データ取得
            var answerTypeDt = from a in context.AnswerTypeMs
                               orderby a.ANSWERTYPEID
                               select a;
            var shop_answerTypeDt = from a in context.Shop_AnswerTypeMs
                                    where a.SHOPID == shopId
                                    orderby a.ANSWERTYPEID
                                    select a;

            //List<AnswerTypeM> answerTypeMList = answerTypeDt.ToArray().ToList();

            //店舗別AnswerType対応
            List<AnswerTypeM> answerTypeMList = new List<AnswerTypeM>();

            if (answerTypeDt.Count() > 0)
            {
                foreach (AnswerTypeM dt in answerTypeDt)
                {
                    var query = from a in shop_answerTypeDt where a.ANSWERTYPEID == dt.ANSWERTYPEID select a;

                    if (query.FirstOrDefault() == null)
                    {
                        answerTypeMList.Add(dt);
                    }
                    else
                    {
                        answerTypeMList.Add(query.FirstOrDefault().GetAnswerTypeM());
                    }

                }
            }
            if (shop_answerTypeDt.Count() > 0)
            {
                foreach (Shop_AnswerTypeM dt in shop_answerTypeDt)
                {
                    var query = from a in answerTypeDt where a.ANSWERTYPEID == dt.ANSWERTYPEID select a;

                    if (query.FirstOrDefault() == null)
                    {
                        answerTypeMList.Add(dt.GetAnswerTypeM());
                    }

                }
            }

            return answerTypeMList;
        }

        /// <summary>
        /// 表示方法ドロップダウンリスト用選択オプション生成
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private SelectListItem[] CreateAnswerTypeMOptionList(List<AnswerTypeM> answerTypeMList)
        {
            SelectListItem[] selectOptions = new SelectListItem[answerTypeMList.Count()];
            int key = 0;
            answerTypeMList.ForEach(a =>
            {
                selectOptions[key] = new SelectListItem() { Value = a.ANSWERTYPEID, Text = a.ANSWERTYPENAME };
                key++;
            });

            return selectOptions;
        }

        /// <summary>
        /// 正常結果条件ドロップダウンリスト用選択オプション生成
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private SelectListItem[] CreateNormalConditionOptionList()
        {
            SelectListItem[] selectOptions = new SelectListItem[OPTIONLIST_NORMALCONDITION.Count];
            int key = 0;
            foreach (KeyValuePair<string, string> kvp in OPTIONLIST_NORMALCONDITION)
            {
                selectOptions[key] = new SelectListItem() { Value = kvp.Key, Text = kvp.Value };
                key++;
            }

            return selectOptions;
        }

        /// <summary>
        /// 画面用データからモデル用データへ移送(登録ユーザーなし)
        /// </summary>
        /// <param name="bquestionm">画面用データ</param>
        /// <param name="orderNo">画面表示順</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns>モデル用データ</returns>
        private QuestionM SetQuestionM(BQuestionM bquestionm, int orderNo, string updUserId)
        {
            return SetQuestionM(bquestionm, orderNo, string.Empty, updUserId);
        }

        /// <summary>
        /// 画面用データからモデル用データへ移送
        /// </summary>
        /// <param name="bquestionm">画面用データ</param>
        /// <param name="orderNo">画面表示順</param>
        /// <param name="insUserId">登録ユーザーID</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns>モデル用データ</returns>
        private QuestionM SetQuestionM(BQuestionM bquestionm, int orderNo, string insUserId, string updUserId)
        {
            var model = new QuestionM
            {
                // 店舗ID
                SHOPID = bquestionm.ShopId,
                // 帳票ID
                REPORTID = bquestionm.ReportId,
                // 大分類ID
                CATEGORYID = bquestionm.CategoryId,
                // 中分類ID
                LOCATIONID = bquestionm.LocationId,
                // 設問ID
                QUESTIONID = bquestionm.QuestionId,
                // 設問
                QUESTION = bquestionm.Question,
                // 設問（英語表記）
                QUESTIONENG = string.IsNullOrEmpty(bquestionm.QuestionEng) ? bquestionm.Question : bquestionm.QuestionEng,
                // 回答種類ID
                ANSWERTYPEID = bquestionm.AnswerTypeId,
                // 正常結果条件
                NORMALCONDITION = bquestionm.NormalCondition,
                // 正常結果条件値１
                NORMALCONDITION1 = bquestionm.NormalCondition1,
                // 正常結果条件値２
                NORMALCONDITION2 = bquestionm.NormalCondition2,
                // 表示No
                DISPLAYNO = (short)orderNo,
                // 論理削除フラグ
                DELETEFLAG = DeleteFlg.NODELETE,
                // 登録ユーザーID
                INSUSERID = insUserId,
                // 更新ユーザーID
                UPDUSERID = updUserId
            };
            // 削除・更新の場合
            if (string.IsNullOrEmpty(insUserId))
            {
                // 登録ユーザーID
                model.INSUSERID = bquestionm.InsUserId;
                // 更新年月日
                if (bquestionm.UpdDate != null)
                {
                    model.UPDDATE = DateTime.Parse(bquestionm.UpdDate);
                }
            }

            return model;
        }

        /// <summary>
        /// 論理削除用設問ユーザーデータ作成
        /// </summary>
        /// <param name="registData">設問データ</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns>論理削除用設問ユーザーデータ</returns>
        private QuestionM SetQuestionMDel(QuestionM registData, string updUserId)
        {
            // 論理削除フラグ
            registData.DELETEFLAG = DeleteFlg.DELETE;
            // 更新ユーザーID
            registData.UPDUSERID = updUserId;

            return registData;
        }

        /// <summary>
        /// 入力データチェック
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="hsError"></param>
        /// <returns></returns>
        private bool CheckRequire(BQuestionM dt, ref HashSet<string> hsError)
        {
            bool checkError = true;
            bool isDecimals = false;    // 小数入力判定用
            bool isInteger = false;    // 整数入力判定用
            string shopId = (string)Session["SHOPID"];

            // 回答種類マスタ
            //GetAnswerTypeMDataで事足りるのでは（北川）

            //var answerTypeDt = from a in context.AnswerTypeMs
            //                   select a;
            //var shop_answerTypeDt = from a in context.Shop_AnswerTypeMs
            //                        where a.SHOPID == shopId
            //                        select a;
            // DBデータから取得
            //List<AnswerTypeM> registData = new List<AnswerTypeM>(answerTypeDt.Where(a => a.ANSWERTYPEID == dt.AnswerTypeId));

            List<AnswerTypeM> registData = GetAnswerTypeMData().Where(a => a.ANSWERTYPEID == dt.AnswerTypeId).ToList();
            var AnswerKbn = "01";
            if (registData.Count() > 0)
            {
                AnswerKbn = registData[0].ANSWERKBN;
            }

            // 入力項目のチェック
            if (string.IsNullOrEmpty(dt.Question))
            {
                hsError.Add("表示項目を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].Question", string.Empty);
                checkError = false;
            }
            else if (dt.Question.Length > 250)
            {
                hsError.Add("表示項目は250文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].Question", string.Empty);
                checkError = false;
            }
            if (!string.IsNullOrEmpty(dt.QuestionEng) && dt.QuestionEng.Length > 750)
            {
                hsError.Add("表示項目（英語表記）は750文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].QuestionEng", string.Empty);
                checkError = false;
            }
            if (string.IsNullOrEmpty(dt.AnswerTypeId))
            {
                hsError.Add("表示方法を選択してください。");
                ModelState.AddModelError("list[" + dt.No + "].AnswerTypeId", string.Empty);
                checkError = false;
            }
            else
            {
                //回答種類チェック
                //AnswerTypeIDで判定しては変更追加に対応できない AnswerKBNで判定すべき （北川）
                switch (AnswerKbn)
                {
                    case "01": //2択3択
                    case "18": //4分岐
                        //正常結果条件
                        if (!"0".Equals(dt.NormalCondition) && !"3".Equals(dt.NormalCondition) && !"4".Equals(dt.NormalCondition))
                        {
                            hsError.Add("正常結果条件を「指定なし」、「次の値に等しい」、「次の値に等しくない」から選択してください。");
                            ModelState.AddModelError("list[" + dt.No + "].NormalCondition", string.Empty);
                            checkError = false;
                        }
                        break;
                    case "04": //温度
                    case "05": //体温
                        //正常結果条件
                        if (!"0".Equals(dt.NormalCondition) && !"1".Equals(dt.NormalCondition) && !"2".Equals(dt.NormalCondition)
                            && !"3".Equals(dt.NormalCondition) && !"4".Equals(dt.NormalCondition) && !"5".Equals(dt.NormalCondition)
                            && !"6".Equals(dt.NormalCondition) && !"7".Equals(dt.NormalCondition) && !"8".Equals(dt.NormalCondition))
                        {
                            hsError.Add("正常結果条件を「上限下限温度の間」以外で選択してください。");
                            ModelState.AddModelError("list[" + dt.No + "].NormalCondition", string.Empty);
                            checkError = false;
                        }
                        break;
                    case "06": //日付
                    case "07": //時間
                    case "08": //整数
                    case "09": //小数
                        //正常結果条件
                        if (!"0".Equals(dt.NormalCondition) && !"5".Equals(dt.NormalCondition) && !"6".Equals(dt.NormalCondition))
                        {
                            hsError.Add("正常結果条件を「指定なし」、「次の値より大きい」、「次の値より小さい」から選択してください。");
                            ModelState.AddModelError("list[" + dt.No + "].NormalCondition", string.Empty);
                            checkError = false;
                        }
                        break;
                    case "14": //食材
                    case "15": //料理
                    case "16": //半製品
                        //正常結果条件
                        if (!"9".Equals(dt.NormalCondition))
                        {
                            hsError.Add("正常結果条件を「上限下限温度の間」で選択してください。");
                            ModelState.AddModelError("list[" + dt.No + "].NormalCondition", string.Empty);
                            checkError = false;
                        }
                        break;
                    default:
                        break;
                }

                ////回答種類チェック
                //if ("01".Equals(dt.AnswerTypeId) || "02".Equals(dt.AnswerTypeId) || "03".Equals(dt.AnswerTypeId) || "21".Equals(dt.AnswerTypeId) || "22".Equals(dt.AnswerTypeId) || "23".Equals(dt.AnswerTypeId))
                //{
                //    //正常結果条件
                //    if (!"0".Equals(dt.NormalCondition) && !"3".Equals(dt.NormalCondition) && !"4".Equals(dt.NormalCondition))
                //    {
                //        hsError.Add("正常結果条件を「指定なし」、「次の値に等しい」、「次の値に等しくない」から選択してください。");
                //        ModelState.AddModelError("list[" + dt.No + "].NormalCondition", string.Empty);
                //        checkError = false;
                //    }
                //}

                //if ("08".Equals(dt.AnswerTypeId) || "09".Equals(dt.AnswerTypeId))
                //{
                //    //正常結果条件
                //    if (!"0".Equals(dt.NormalCondition) && !"5".Equals(dt.NormalCondition) && !"6".Equals(dt.NormalCondition))
                //    {
                //        hsError.Add("正常結果条件を「指定なし」、「次の値より大きい」、「次の値より小さい」から選択してください。");
                //        ModelState.AddModelError("list[" + dt.No + "].NormalCondition", string.Empty);
                //        checkError = false;
                //    }
                //}

                //if ("06".Equals(dt.AnswerTypeId) || "07".Equals(dt.AnswerTypeId) || "10".Equals(dt.AnswerTypeId) || "11".Equals(dt.AnswerTypeId))
                //{
                //    //正常結果条件
                //    if (!"0".Equals(dt.NormalCondition) && !"1".Equals(dt.NormalCondition) && !"2".Equals(dt.NormalCondition)
                //        && !"3".Equals(dt.NormalCondition) && !"4".Equals(dt.NormalCondition) && !"5".Equals(dt.NormalCondition)
                //        && !"6".Equals(dt.NormalCondition) && !"7".Equals(dt.NormalCondition) && !"8".Equals(dt.NormalCondition))
                //    {
                //        hsError.Add("正常結果条件を「上限下限温度の間」以外で選択してください。");
                //        ModelState.AddModelError("list[" + dt.No + "].NormalCondition", string.Empty);
                //        checkError = false;
                //    }
                //}

                //if ("17".Equals(dt.AnswerTypeId) || "18".Equals(dt.AnswerTypeId) || "19".Equals(dt.AnswerTypeId))
                //{
                //    //正常結果条件
                //    if (!"9".Equals(dt.NormalCondition))
                //    {
                //        hsError.Add("正常結果条件を「上限下限温度の間」で選択してください。");
                //        ModelState.AddModelError("list[" + dt.No + "].NormalCondition", string.Empty);
                //        checkError = false;
                //    }
                //}


                if (registData.Count() > 0)
                {
                    if (0 <= Array.IndexOf(ANSWERKBN_INPUTTYPE_DECIMALS, registData[0].ANSWERKBN))
                    {
                        isDecimals = true;
                    }
                    else if (0 <= Array.IndexOf(ANSWERKBN_INPUTTYPE_INTEGER, registData[0].ANSWERKBN))
                    {
                        isInteger = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(dt.NormalCondition))
            {
                if (this.IsNeedNormalCondition1(dt.NormalCondition) && !("08".Equals(dt.AnswerTypeId) || "09".Equals(dt.AnswerTypeId)))
                {
                    if (string.IsNullOrEmpty(dt.NormalCondition1))
                    {
                        hsError.Add("正常結果条件値１を入力してください。");
                        ModelState.AddModelError("list[" + dt.No + "].NormalCondition1", string.Empty);
                        checkError = false;
                    }
                    else if (isDecimals)
                    {
                        Double val;
                        try
                        {
                            val = Double.Parse(Strings.StrConv(dt.NormalCondition1, VbStrConv.Narrow, 0));
                            if (val < -999.99 || val > 999.99)
                            {
                                throw new Exception();
                            }
                            dt.NormalCondition1 = val.ToString();
                        }
                        catch (Exception)
                        {
                            hsError.Add("正常結果条件値１は -999.99 ～ 999.99 の数値で入力してください。");
                            ModelState.AddModelError("list[" + dt.No + "].NormalCondition1", string.Empty);
                            checkError = false;
                        }
                    }
                    else if (isInteger)
                    {
                        short val;
                        try
                        {
                            val = short.Parse(Strings.StrConv(dt.NormalCondition1, VbStrConv.Narrow, 0));
                            if (val < -999 || val > 999)
                            {
                                throw new Exception();
                            }
                            dt.NormalCondition1 = val.ToString();
                        }
                        catch (Exception)
                        {
                            hsError.Add("正常結果条件値１は -999 ～ 999 の整数で入力してください。");
                            ModelState.AddModelError("list[" + dt.No + "].NormalCondition1", string.Empty);
                            checkError = false;
                        }
                    }
                    else if (dt.NormalCondition1.Length > 10)
                    {
                        hsError.Add("正常結果条件値１は10文字以内で入力してください。");
                        ModelState.AddModelError("list[" + dt.No + "].NormalCondition1", string.Empty);
                        checkError = false;
                    }
                }
                if (this.IsNeedNormalCondition2(dt.NormalCondition) && !("08".Equals(dt.AnswerTypeId) || "09".Equals(dt.AnswerTypeId)))
                {
                    if (string.IsNullOrEmpty(dt.NormalCondition2))
                    {
                        hsError.Add("正常結果条件値２を入力してください。");
                        ModelState.AddModelError("list[" + dt.No + "].NormalCondition2", string.Empty);
                        checkError = false;
                    }
                    else if (isDecimals)
                    {
                        Double val;
                        try
                        {
                            val = Double.Parse(Strings.StrConv(dt.NormalCondition2, VbStrConv.Narrow, 0));
                            if (val < -999.99 || val > 999.99)
                            {
                                throw new Exception();
                            }
                            dt.NormalCondition2 = val.ToString();
                        }
                        catch (Exception)
                        {
                            hsError.Add("正常結果条件値２は -999.99 ～ 999.99 の数値で入力してください。");
                            ModelState.AddModelError("list[" + dt.No + "].NormalCondition2", string.Empty);
                            checkError = false;
                        }
                    }
                    else if (isInteger)
                    {
                        short val;
                        try
                        {
                            val = short.Parse(Strings.StrConv(dt.NormalCondition2, VbStrConv.Narrow, 0));
                            if (val < -999 || val > 999)
                            {
                                throw new Exception();
                            }
                            dt.NormalCondition2 = val.ToString();
                        }
                        catch (Exception)
                        {
                            hsError.Add("正常結果条件値１は -999 ～ 999 の整数で入力してください。");
                            ModelState.AddModelError("list[" + dt.No + "].NormalCondition2", string.Empty);
                            checkError = false;
                        }
                    }
                    else if (dt.NormalCondition2.Length > 10)
                    {
                        hsError.Add("正常結果条件値２は10文字以内で入力してください。");
                        ModelState.AddModelError("list[" + dt.No + "].NormalCondition2", string.Empty);
                        checkError = false;
                    }
                }
            }

            if (checkError)
            {
                if ("17".Equals(dt.AnswerTypeId))
                {
                    //食材
                    dt.NormalCondition1 = "FoodStuffM";
                    dt.NormalCondition2 = "FoodStuffM";
                }
                if ("18".Equals(dt.AnswerTypeId))
                {
                    //料理
                    dt.NormalCondition1 = "CuisineM";
                    dt.NormalCondition2 = "CuisineM";
                }
                if ("19".Equals(dt.AnswerTypeId))
                {
                    //半製品
                    dt.NormalCondition1 = "SemiFinProductM";
                    dt.NormalCondition2 = "SemiFinProductM";
                }
            }

            return checkError;
        }
        /// <summary>
        /// 帳票内にテキストエリアが２つ以上ないかチェック
        /// </summary>
        /// <param name="AnswerTypeId">回答種類ID</param>
        /// <param name="textAreaIndex">帳票内のテキストエリア数をカウント</param>
        /// <returns>2つ以上かつ、表示方法がテキストエリアだった場合false</returns>
        private bool CheckTextArea(string AnswerTypeId, ref int textAreaIndex)
        {
            bool isDuplicated = true;
            if (!string.IsNullOrEmpty(AnswerTypeId) && AnswerTypeId.Equals("05"))
            {
                textAreaIndex++;
            }
            if (!string.IsNullOrEmpty(AnswerTypeId) && textAreaIndex > 1 && AnswerTypeId.Equals("05"))
            {
                isDuplicated = false;
            }
            return isDuplicated;
        }

        /// <summary>
        /// 更新項目結果チェック
        /// </summary>
        /// <param name="registData">DBデータ</param>
        /// <param name="dt">画面データ</param>
        /// <returns>bool（true:更新あり、false：更新なし）</returns>
        private bool CheckDataUpd(List<QuestionM> registData, BQuestionM dt)
        {
            return (registData[0].REPORTID != dt.ReportId
                || registData[0].CATEGORYID != dt.CategoryId
                || registData[0].LOCATIONID != dt.LocationId
                || registData[0].QUESTION != dt.Question
                || registData[0].QUESTIONENG != dt.QuestionEng
                || registData[0].ANSWERTYPEID != dt.AnswerTypeId
                || registData[0].NORMALCONDITION != dt.NormalCondition
                || registData[0].NORMALCONDITION1 != dt.NormalCondition1
                || registData[0].NORMALCONDITION2 != dt.NormalCondition2);
        }

        /// <summary>
        /// 正常結果条件値１入力必須チェック
        /// </summary>
        /// <param name="normalCondition">正常結果条件</param>
        /// <returns>bool（true:必要、false：不要）</returns>
        private bool IsNeedNormalCondition1(string normalCondition)
        {
            return (NORMALCONDITION_NORMALCONDITION1_NEED[normalCondition]);
        }

        /// <summary>
        /// 正常結果条件値２入力必須チェック
        /// </summary>
        /// <param name="normalCondition">正常結果条件</param>
        /// <returns>bool（true:必要、false：不要）</returns>
        private bool IsNeedNormalCondition2(string normalCondition)
        {
            return (NORMALCONDITION_NORMALCONDITION2_NEED[normalCondition]);
        }

        /// <summary>
        /// ドロップダウンリストデータ設定
        /// </summary>
        /// <returns>void</returns>
        private void SetDropDownList(string categoryId, string locationId)
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
            // 帳票ドロップダウンリスト
            List<ReportM> reportMList = this.GetReportMData(shopId, categoryId, locationId);
            if (!string.IsNullOrEmpty(categoryId) && !string.IsNullOrEmpty(locationId) && reportMList.Count() == 0)
            {
                // 帳票データが存在しない場合メッセージを表示
                ModelState.AddModelError(string.Empty, "帳票データが存在しません。帳票マスタから登録してください。");
                ViewBag.editMode = "disabled";
            }
            ViewBag.reportMSelectListItem = this.CreateReportMOptionList(reportMList);
            // 表示方法ドロップダウンリスト
            List<AnswerTypeM> answerTypeMList = this.GetAnswerTypeMData();
            if (answerTypeMList.Count() == 0)
            {
                // 回答種類データが存在しない場合メッセージを表示
                ModelState.AddModelError(string.Empty, "回答種類マスタデータが存在しません。サービスセンターまでご連絡ください。");
                ViewBag.editMode = "disabled";
            }
            ViewBag.answerTypeMSelectListItem = this.CreateAnswerTypeMOptionList(answerTypeMList);
            // 正常結果条件ドロップダウンリスト
            ViewBag.normalConditionSelectListItem = comFunc.CreateOptionList(OPTIONLIST_NORMALCONDITION);
        }

        /// <summary>
        /// オプションリスト表示用正常結果条件
        /// </summary>
        private static readonly Dictionary<string, string> OPTIONLIST_NORMALCONDITION = new Dictionary<string, string>()
        {
            // 正常結果条件（値）, 正常結果条件（表示値）
            {"0", "指定なし"},
            {"1", "次の値の間"},
            {"2", "次の値の間以外"},
            {"3", "次の値に等しい"},
            {"4", "次の値に等しくない"},
            {"5", "次の値より大きい"},
            {"6", "次の値より小さい"},
            {"7", "次の値以上"},
            {"8", "次の値以下"},
            {"9", "上限下限温度の間"},
        };

        /// <summary>
        /// 正常結果条件に対する正常結果条件値１入力要否
        /// </summary>
        private static readonly Dictionary<string, bool> NORMALCONDITION_NORMALCONDITION1_NEED = new Dictionary<string, bool>()
        {
            // 正常結果条件, 正常結果条件値１入力要否（true:必要、false：不要）
            {"0", false},
            {"1", true},
            {"2", true},
            {"3", true},
            {"4", true},
            {"5", true},
            {"6", true},
            {"7", true},
            {"8", true},
            {"9", false},
        };

        /// <summary>
        /// 正常結果条件に対する正常結果条件値２入力要否
        /// </summary>
        private static readonly Dictionary<string, bool> NORMALCONDITION_NORMALCONDITION2_NEED = new Dictionary<string, bool>()
        {
            // 正常結果条件, 正常結果条件値２入力要否（true:必要、false：不要）
            {"0", false},
            {"1", true},
            {"2", true},
            {"3", false},
            {"4", false},
            {"5", false},
            {"6", false},
            {"7", false},
            {"8", false},
            {"9", false},
        };
    }
}