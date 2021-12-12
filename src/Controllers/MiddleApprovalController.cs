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
using System.Text;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class MiddleApprovalController : Controller
    {
        private MasterContext context = new MasterContext();
        private CommonFunction comm = new CommonFunction();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MiddleApprovalController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "MiddleApproval");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// データ承認画面表示処理（中分類）
        /// </summary>
        /// <returns>画面表示</returns>
        [HttpGet]
        public ActionResult Show()
        {
            // 大分類ID
            string categoryId = (string)Session["PENDINGCATEGORYID"];
            // 中分類ID
            string locationId = (string)Session["PENDINGLOCATIONID"];
            // 帳票ID
            string reportId = (string)Session["PENDINGREPORTID"];
            // 周期ID
            string periodId = (string)Session["PENDINGPERIODID"];
            // 周期指定日
            string periodStart = (string)Session["PENDINGSTARTDATE"];

            // セッションから削除
            Session.Remove("PENDINGNODEID");
            Session.Remove("PENDINGCATEGORYID");
            Session.Remove("PENDINGLOCATIONID");
            Session.Remove("PENDINGREPORTID");
            Session.Remove("PENDINGPERIODID");
            Session.Remove("PENDINGSTARTDATE");

            // 登録メッセージを取得
            string registMsg = (string)Session["registMsg"];
            if (!string.IsNullOrEmpty(registMsg))
            {
                Session.Remove("registMsg");
                ViewBag.registMsg = registMsg;
            }

            BMiddleApproval data = this.GetMiddleData(categoryId, locationId, reportId, periodId, periodStart);
            data.PeriodDay = comm.FormatDateStrhyphen(periodStart);

            return View(data);
        }

        /// <summary>
        /// 条件変更（大分類、中分類ドロップダウンリスト）
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>画面表示</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangeCondition(FormCollection form)
        {
            // post時の情報をクリア
            ModelState.Clear();

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            var bMiddleApproval = new BMiddleApproval();

            string categoryId = form["CategoryId"];
            string locationId = form["LocationId"];
            string periodDay = form["PeriodDay"];
            string periodYMD = form["PeriodYMD"];
            string period = form["Period"];

            bMiddleApproval.CategoryDrop = this.GetCategory(shopId, categoryId);
            bMiddleApproval.LoactionDrop = this.GetLocation(shopId, locationId);
            bMiddleApproval.ReportDrop = this.GetReport(shopId, categoryId, locationId, string.Empty);
            bMiddleApproval.Period = period;
            bMiddleApproval.PeriodDay = periodDay;
            bMiddleApproval.PeriodYMD = periodYMD;

            return View("Show", bMiddleApproval);
        }

        /// <summary>
        /// 条件変更(周期指定日日、帳票ドロップダウンリスト)
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>画面表示</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangeDetermineConditions(FormCollection form)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 画面から値を取得
            string categoryId = form["CategoryId"];
            string locationId = form["LocationId"];
            string reportId = form["ReportId"];
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];

            // データ取得
            BMiddleApproval data = this.GetMiddleData(categoryId, locationId, reportId, periodId, periodYMD);
            data.PeriodDay = periodDay;

            return View("Show", data);
        }

        /// <summary>
        /// 承認処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <param name="list">画面入力値承認データ情報</param>
        /// <returns>画面表示</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult RegistApproval(FormCollection form, IList<MiddleData> list)
        {
            // post時の情報をクリア
            ModelState.Clear();

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // 画面から値を取得
            string categoryId = form["CategoryId"];
            string locationId = form["LocationId"];
            string reportId = form["ReportId"];
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];

            // 承認者
            string updUserId = (string)Session["LOGINMNGID"];
            var updMiddleTs = new List<MiddleApprovalT>();

            // 承認対象チェック
            bool chkFlg = false;
            // 行数
            int cnt = 0;
            // 入力チェック
            bool inputCheck = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            foreach (MiddleData middle in list)
            {
                if (middle.DataChk)
                {
                    chkFlg = true;

                    // 中分類承認情報データ
                    var middleDt = from mid in context.MiddleApprovalTs
                                   where mid.SHOPID == shopId
                                      && mid.CATEGORYID == categoryId
                                      && mid.LOCATIONID == locationId
                                      && mid.REPORTID == reportId
                                      && mid.APPROVALID == middle.APPROVALID
                                   select mid;

                    if (middleDt.Count() > 0)
                    {
                        MiddleApprovalT dbval = middleDt.FirstOrDefault();
                        // ステータス
                        dbval.STATUS = ApprovalStatus.APPROVAL;
                        // 承認日時
                        dbval.MIDDLESNNDATE = DateTime.Now.ToString("yyyyMMddHHmmss");
                        // 承認コメント
                        dbval.MIDDLESNNCOMMENT = middle.MIDDLESNNCOMMENT;
                        // 承認ユーザーID
                        dbval.MIDDLESNNUSER = updUserId;
                        // 更新ユーザーID
                        dbval.UPDUSERID = updUserId;
                        // 更新日時
                        dbval.UPDDATE = DateTime.Parse(middle.UPDDATE);
                        
                        updMiddleTs.Add(dbval);
                        
                    } else
                    {
                        inputCheck = false;
                        // データ排他
                        ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    }

                    // コメントデータ入力チェック
                    if (!CheckInput(middle, cnt, ref hsError)) inputCheck = false;
                }
                cnt++;
            }

            // 画面用中分類承認データ
            var bMiddleApproval = new BMiddleApproval
            {
                // 大分類ドロップ取得
                CategoryDrop = this.GetCategory(shopId, categoryId),
                // 中分類ドロップ取得
                LoactionDrop = this.GetLocation(shopId, locationId),
                // 帳票ドロップ取得
                ReportDrop = this.GetReport(shopId, categoryId, locationId, reportId),
                // 周期ID
                Period = periodId,
                // 周期指定日(YYYY/MM/DD)
                PeriodDay = periodDay,
                // 周期指定日(YYYYMMDD)
                PeriodYMD = periodYMD,
                // データリスト
                MiddleDatas = list.ToList(),
                // 承認ボタン活性
                ApprobalBtn = true,
                // 承認依頼ボタン活性
                RequestBtn = false
            };

            // 承認履歴取得
            bMiddleApproval.Historys = this.GetHistory(
                shopId, categoryId, locationId, reportId, periodId, periodYMD);

            // チェックがない場合はエラー
            if (!chkFlg)
            {
                ModelState.AddModelError(string.Empty, MsgConst.NO_CHOISE_APPROVAL_DATA);
                return View("Show", bMiddleApproval);
            }

            // 入力チェックエラーが存在する場合
            if (!inputCheck)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }

                return View("Show", bMiddleApproval);
            }

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (MiddleApprovalT upddata in updMiddleTs)
                        {
                            context.MiddleApprovalTs.Attach(upddata);
                            context.Entry(upddata).State = EntityState.Modified;
                        }
                        context.SaveChanges();

                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // ロールバック
                        tran.Rollback();
                        // 排他エラー
                        ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                        return View("Show", bMiddleApproval);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", bMiddleApproval);
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

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);

            // 画面再表示
            Session.Add("PENDINGNODEID", APPROVALLEVEL.MIDDLE);
            Session.Add("PENDINGCATEGORYID", categoryId);
            Session.Add("PENDINGLOCATIONID", locationId);
            Session.Add("PENDINGREPORTID", reportId);
            Session.Add("PENDINGPERIODID", periodId);
            Session.Add("PENDINGSTARTDATE", periodYMD);

            return RedirectToAction("Show");
        }

        /// <summary>
        /// 大分類承認依頼処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <param name="list">画面入力値承認データ情報</param>
        /// <returns></returns>
        public ActionResult RequestMajorApproval(FormCollection form, IList<MiddleData> list)
        {
            // post時の情報をクリア
            ModelState.Clear();

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // 画面から値を取得
            string categoryId = form["CategoryId"];
            string locationId = form["LocationId"];
            string reportId = form["ReportId"];
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];
            string periodStart = form["PeriodStart"];
            string periodEnd = form["PeriodEnd"];
            string requestComment = form["RequestComment"];
            string majorUpdddt = form["MajorUpdDate"];
            // 承認者
            string updUserId = (string)Session["LOGINMNGID"];
            // 更新日時
            string updDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            // 入力チェック
            bool inputCheck = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            // 画面用中分類承認データ
            var bMiddleApproval = new BMiddleApproval
            {
                // 大分類ドロップ取得
                CategoryDrop = this.GetCategory(shopId, categoryId),
                // 中分類ドロップ取得
                LoactionDrop = this.GetLocation(shopId, locationId),
                // 帳票ドロップ取得
                ReportDrop = this.GetReport(shopId, categoryId, locationId, reportId),
                // 周期ID
                Period = periodId,
                // 周期指定日(YYYY/MM/DD)
                PeriodDay = periodDay,
                // 周期指定日(YYYYMMDD)
                PeriodYMD = periodYMD,
                // データリスト
                MiddleDatas = list.ToList(),
                // 承認ボタン活性
                ApprobalBtn = false,
                // 承認依頼ボタン活性
                RequestBtn = true,
                // 承認依頼コメント
                RequestComment = requestComment
            };

            // 承認履歴取得
            bMiddleApproval.Historys = this.GetHistory(
                shopId, categoryId, locationId, reportId, periodId, periodYMD);

            bool pendingData = false;

            foreach (MiddleData midt in list)
            {
                if (ApprovalStatus.PENDING.Equals(midt.STATUS) 
                    || ApprovalStatus.REMAND.Equals(midt.STATUS))
                {
                    pendingData = true;
                    break;
                }
            }

            if (pendingData)
            {
                // 承認待ち・差戻データが存在するため、依頼不可
                bMiddleApproval.RequestBtn = false;
                ModelState.AddModelError(string.Empty, MsgConst.SNNREQUEST_PENDING_ERR);
                return View("Show", bMiddleApproval);
            }

            // 大分類承認依頼ボタン活性
            var approve = this.GetRequestState(
                shopId, categoryId, locationId, reportId, periodId, periodYMD, majorUpdddt);

            if ((string.IsNullOrEmpty(majorUpdddt) && approve != null)
                 || (!string.IsNullOrEmpty(majorUpdddt) && approve == null))
            {
                // 大分類承認データが更新されているため、排他エラー
                bMiddleApproval.RequestBtn = false;
                ModelState.AddModelError(string.Empty, MsgConst.MAJORREQUEST_DATACHANGE);
                return View("Show", bMiddleApproval);
            }

            // 承認依頼コメント入力チェック
            if (!CheckReqComment(requestComment, ref hsError)) inputCheck = false;

            // 入力チェックエラーが存在する場合
            if (!inputCheck)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }

                return View("Show", bMiddleApproval);
            }

            // 中分類承認グループ連番
            int middleGroupNo = 0;
            // 登録用大分類承認データ
            var addMajor = new MajorApprovalT();
            // 更新用大分類承認データ
            var updMajor = new MajorApprovalT();

            // 大分類承認情報を取得
            var majorDt = from maj in context.MajorApprovalTs
                          where maj.SHOPID == shopId
                            && maj.CATEGORYID == categoryId
                            && maj.LOCATIONID == locationId
                            && maj.REPORTID == reportId
                            && maj.PERIOD == periodId
                            && maj.PERIODSTART == periodStart
                            && maj.DELETEFLAG == DeleteFlg.NODELETE
                          select maj;
            if (majorDt.Count() > 0)
            {
                // 更新用大分類承認データ
                updMajor = majorDt.First();

                // 中分類承認グループ連番
                middleGroupNo = updMajor.MIDDLEGROUPNO;
                // 倫理削除フラグ
                updMajor.DELETEFLAG = DeleteFlg.DELETE;
                // 更新ユーザーID
                updMajor.UPDUSERID = updUserId;

                // 新規登録用大分類承認情報
                addMajor = this.SetAddMajorData(updMajor, requestComment, updUserId, updDate);
            } else
            {
                // 新規データを作成
                addMajor.SHOPID = shopId;
                addMajor.CATEGORYID = categoryId;
                addMajor.LOCATIONID = locationId;
                addMajor.REPORTID = reportId;
                addMajor.PERIOD = periodId;
                addMajor.PERIODSTART = periodStart;
                addMajor.PERIODEND = periodEnd;
                addMajor.MIDDLEGROUPNO = (short)(middleGroupNo + 1);
                addMajor.MAJORGROUPNO = 0;
                addMajor.STATUS = ApprovalStatus.PENDING;
                addMajor.STAMPFIELD = StampField.STAMP_RESPOSNSIBLE;
                addMajor.MAJORREQDATE = updDate;
                addMajor.MAJORREQCOMMENT = requestComment;
                addMajor.MAJORREQWORKERID = updUserId;
                addMajor.DELETEFLAG = DeleteFlg.NODELETE;
                addMajor.INSUSERID = updUserId;
                addMajor.UPDUSERID = updUserId;
            }

            bool middleErr = false;
            // 中分類承認情報を取得
            var middleDt = from mid in context.MiddleApprovalTs
                           where mid.SHOPID == shopId
                            && mid.CATEGORYID == categoryId
                            && mid.LOCATIONID == locationId
                            && mid.REPORTID == reportId
                            && mid.PERIOD == periodId
                            && mid.PERIODSTART == periodStart
                            && mid.DELETEFLAG == DeleteFlg.NODELETE
                           select mid;

            if (middleDt.Count() != list.Count())
            {
                // データ数が違うため、排他エラー
                middleErr = true;
            }

            // 更新用中分類承認情報
            List<MiddleApprovalT> UpdMiddleList = new List<MiddleApprovalT>();

            foreach (MiddleData middt in list)
            {
                // 承認IDからデータを取得
                var middleapp = middleDt.Where(a => a.APPROVALID == middt.APPROVALID).FirstOrDefault();
                if (middleapp == null)
                {
                    // データ数が違うため、排他エラー
                    middleErr = true;
                    continue;
                }

                // 中分類承認グループ連番
                middleapp.MIDDLEGROUPNO = (short)(middleGroupNo + 1);
                // 更新年月日
                middleapp.UPDDATE = DateTime.Parse(middt.UPDDATE);
                // 更新ユーザーID
                middleapp.UPDUSERID = updUserId;

                UpdMiddleList.Add(middleapp);
            }

            // 画面とDBデータに差異があるため、エラー
            if (middleErr)
            {
                bMiddleApproval.RequestBtn = false;
                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                return View("Show", bMiddleApproval);
            }

            // 大分類承認情報の帳票名を設定
            addMajor.REPORTNAME = middleDt.FirstOrDefault().REPORTNAME;

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 中分類データ更新
                        foreach (MiddleApprovalT updMiddleDt in UpdMiddleList)
                        {
                            context.MiddleApprovalTs.Attach(updMiddleDt);
                            context.Entry(updMiddleDt).State = EntityState.Modified;
                        }
                        context.SaveChanges();

                        // 大分類承認データ登録
                        context.MajorApprovalTs.Add(addMajor);
                        context.SaveChanges();

                        // 大分類承認データ更新
                        if (majorDt.Count() > 0)
                        {
                            context.MajorApprovalTs.Attach(updMajor);
                            context.Entry(updMajor).State = EntityState.Modified;
                            context.SaveChanges();
                        }

                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // ロールバック
                        tran.Rollback();
                        // 排他エラー
                        ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                        return View("Show", bMiddleApproval);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", bMiddleApproval);
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
                        throw new ApplicationException();
                    }
                }
            }

            var mailToList = new List<MailInfo>();
            var sender = new MailSenderFunction();
            var sendmail = new SendMailBusiness();

            using (context = new MasterContext())
            {
                // 大分類承認の承認管理者を取得
                var nextManager = from appRoute in context.ApprovalRouteMs
                                  where appRoute.SHOPID == shopId
                                    && appRoute.CATEGORYID == categoryId
                                    && appRoute.LOCATIONID == ApprovalCategory.MAJORDATA_LOCATION
                                  select appRoute;
                // メール情報リスト作成
                mailToList = sender.SetMailAddress(context, nextManager);
                // 大分類承認担当者へメール送信
                sendmail.SendMajorRequestMail(context, mailToList, shopId, categoryId, locationId, periodId, periodStart);
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_REQUEST_APPROVAL);

            // 画面再表示
            Session.Add("PENDINGNODEID", APPROVALLEVEL.MIDDLE);
            Session.Add("PENDINGCATEGORYID", categoryId);
            Session.Add("PENDINGLOCATIONID", locationId);
            Session.Add("PENDINGREPORTID", reportId);
            Session.Add("PENDINGPERIODID", periodId);
            Session.Add("PENDINGSTARTDATE", periodYMD);

            return RedirectToAction("Show");
        }

        /// <summary>
        /// データ承認詳細データ
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <param name="list">画面入力値（承認データ）</param>
        /// <returns>画面表示</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ShowDataDetail(FormCollection form)
        {
            // 画面再表示
            Session.Add("PENDINGNODEID", APPROVALLEVEL.MIDDLE);
            Session.Add("PENDINGCATEGORYID", form["CategoryId"]);
            Session.Add("PENDINGLOCATIONID", form["LocationId"]);
            Session.Add("PENDINGREPORTID", form["ReportId"]);
            Session.Add("PENDINGPERIODID", form["Period"]);
            Session.Add("PENDINGSTARTDATE", form["PeriodYMD"]);

            return RedirectToAction("Show", "ApprovalDataDetail");
        }

        /// <summary>
        /// 中分類承認データ項目移送
        /// </summary>
        /// <param name="major"></param>
        /// <param name="comment">承認コメント</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <param name="snnDate">承認日(yyyyMMddHHmmss)</param>
        /// <returns>中分類承認データ</returns>
        private MajorApprovalT SetAddMajorData(MajorApprovalT major, string comment, string updUserId, string snnDate)
        {
            int no = major.MIDDLEGROUPNO;

            MajorApprovalT addData = major.Clone();
            // 中分類承認グループ連番
            addData.MIDDLEGROUPNO = (short)(no + 1);
            // 大分類承認グループ連番
            addData.MAJORGROUPNO = 0;
            // ステータス
            addData.STATUS = ApprovalStatus.PENDING;
            // 大分類承認依頼日時
            addData.MAJORREQDATE = snnDate;
            // 大分類承認依頼コメント
            addData.MAJORREQCOMMENT = comment;
            // 大分類承認依頼ユーザー
            addData.MAJORREQWORKERID = updUserId;
            // 承認（差戻）日時
            addData.MAJORSNNDATE = null;
            // 承認（差戻）コメント
            addData.MAJORSNNCOMMENT = null;
            // 承認管理者ユーザー
            addData.MAJORSNNUSER = null;
            // 論理削除フラグ
            addData.DELETEFLAG = DeleteFlg.NODELETE;
            // 登録ユーザーID
            addData.INSUSERID = updUserId;
            // 更新ユーザーID
            addData.UPDUSERID = updUserId;

            return addData;
        }

        /// <summary>
        /// 中分類承認データ取得
        /// </summary>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodId">周期</param>
        /// <param name="periodYMD">周期指定日(YYYYMMDD)</param>
        /// <returns>中分類承認データ</returns>
        private BMiddleApproval GetMiddleData(string categoryId, string locationId, string reportId, string periodId, string periodYMD)
        {
            // データ表示
            bool display = true;

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            if (string.IsNullOrEmpty(categoryId)
                || string.IsNullOrEmpty(locationId)
                || string.IsNullOrEmpty(reportId)
                || string.IsNullOrEmpty(periodId)
                || string.IsNullOrEmpty(periodYMD))
            {
                // データ非表示
                display = false;
                ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
            }

            // 画面用中分類承認データ
            var bMiddleApproval = new BMiddleApproval
            {
                // 大分類ドロップ取得
                CategoryDrop = this.GetCategory(shopId, categoryId),
                // 中分類ドロップ取得
                LoactionDrop = this.GetLocation(shopId, locationId),
                // 帳票ドロップ取得
                ReportDrop = this.GetReport(shopId, categoryId, locationId, reportId),
                // 周期ID
                Period = periodId,
                // 周期指定日
                PeriodYMD = periodYMD,
                // 承認ボタン活性
                ApprobalBtn = false,
                // 承認依頼ボタン活性
                RequestBtn = true
            };

            // データ取得
            if (display)
            {
                var tranfunction = new TransactionFunction(context);
                // 承認者
                string managerId = (string)Session["LOGINMNGID"];
                // 承認権限チェック
                if (!tranfunction.GetApprovalAuthority(shopId, categoryId, locationId, managerId, ApprovalCategory.MIDDLE))
                {
                    bMiddleApproval.MiddleDatas = new List<MiddleData>();
                    ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
                    return bMiddleApproval;
                }

                // 承認対象データ
                var middle = this.GetMiddlePendingData(
                    shopId, categoryId, locationId, reportId, periodId, periodYMD);

                if (middle.Count() > 0)
                {
                    var dtList = new List<MiddleData>();

                    foreach (MiddleData dt in middle)
                    {
                        // 承認ボタン活性
                        if (ApprovalStatus.PENDING.Equals(dt.STATUS))
                        {
                            bMiddleApproval.ApprobalBtn = true;
                            bMiddleApproval.RequestBtn = false;
                        }
                        dtList.Add(dt);
                    }
                    bMiddleApproval.MiddleDatas = dtList;
                    // 周期開始・終了
                    bMiddleApproval.PeriodStart = middle.FirstOrDefault<MiddleData>().PERIODSTART;
                    bMiddleApproval.PeriodEnd = middle.FirstOrDefault<MiddleData>().PERIODEND;
                    bMiddleApproval.PeriodStartDate = comm.FormatDateStr(middle.FirstOrDefault<MiddleData>().PERIODSTART);
                    bMiddleApproval.PeriodEndDate = comm.FormatDateStr(middle.FirstOrDefault<MiddleData>().PERIODEND);
                } else
                {
                    bMiddleApproval.MiddleDatas = new List<MiddleData>();
                    ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
                }

                // 承認履歴取得
                bMiddleApproval.Historys = this.GetHistory(
                    shopId, categoryId, locationId, reportId, periodId, periodYMD);

                if (bMiddleApproval.RequestBtn)
                {
                    // 大分類承認依頼ボタン活性
                    var major = this.GetRequestState(
                        shopId, categoryId, locationId, reportId, periodId, periodYMD, string.Empty);

                    if (major != null)
                    {
                        if (ApprovalStatus.APPROVAL.Equals(major.STATUS))
                        {
                            bMiddleApproval.RequestBtn = false;
                        }
                        // 大分類承認データ更新日付
                        bMiddleApproval.MajorUpdDate = major.UPDDATE.ToString("yyyy/MM/dd HH:mm:ss.ffffff");
                    }
                }
            }

            return bMiddleApproval;
        }

        /// <summary>
        /// 中分類承認待ちデータ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodId">周期</param>
        /// <param name="periodYMD">周期指定日</param>
        /// <returns>中分類承認待ちデータ</returns>
        private List<MiddleData> GetMiddlePendingData(
            string shopId, string categoryId, string locationId, string reportId, string periodId, string periodYMD)
        {
            // 承認対象データ
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("TO_CHAR(TO_DATE(MID.MIDDLESNNDATE,'YYYY/MM/DD HH24:MI:SS'), 'YYYY/MM/DD HH24:MI:SS') AS MIDDLESNNDATE ");
            sql.Append(", MID.MIDDLESNNCOMMENT ");
            sql.Append(", MID.PERIOD ");
            sql.Append(", MID.PERIODSTART ");
            sql.Append(", MID.PERIODEND ");
            sql.Append(", MID.APPROVALID ");
            sql.Append(", MID.STATUS ");
            sql.Append(", MID.WORKERID ");
            sql.Append(", MID.WORKERNAME ");
            sql.Append(", TO_CHAR(TO_DATE(MID.DATAYMD,'YYYY/MM/DD HH24:MI:SS'), 'YYYY/MM/DD HH24:MI:SS') AS DATAYMD ");
            sql.Append(", TO_CHAR(MID.UPDDATE, 'YYYY/MM/DD HH24:MI:SS.FF') AS UPDDATE ");
            sql.Append("FROM MIDDLEAPPROVAL_T MID ");
            sql.Append("WHERE ");
            sql.Append("MID.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND MID.CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("AND MID.LOCATIONID = '");
            sql.Append(locationId);
            sql.Append("' ");
            sql.Append("AND MID.REPORTID = '");
            sql.Append(reportId);
            sql.Append("' ");
            sql.Append("AND MID.PERIOD = '");
            sql.Append(periodId);
            sql.Append("' ");
            sql.Append("AND MID.PERIODSTART <= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND MID.PERIODEND >= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND MID.STAMPFIELD = ");
            sql.Append(StampField.STAMP_RESPOSNSIBLE);
            sql.Append(" ");
            sql.Append("AND MID.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("ORDER BY MID.DATAYMD ");
            sql.Append("FOR READ ONLY ");
            var middle = context.Database.SqlQuery<MiddleData>(sql.ToString());

            if (middle.Count() > 0)
            {
                return middle.ToList();
            }

            return new List<MiddleData>();
        }

        /// <summary>
        /// 承認履歴情報取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodYMD">周期指定日</param>
        /// <returns></returns>
        private List<ApprovalHistory> GetHistory(
            string shopId, string categoryId, string locationId, string reportId, string periodId, string periodYMD)
        {
            // 承認履歴取得
            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("'");
            sql.Append(ApprovalCategory.NODE_CLASS_MAJOR);
            sql.Append("' AS APPROVALNODE ");
            sql.Append(", TO_CHAR(TO_DATE(MAJ.MAJORSNNDATE,'YYYY/MM/DD HH24:MI:SS'), 'YYYY/MM/DD HH24:MI:SS') AS APPROVALDATE ");
            sql.Append(", MAJ.MAJORSNNCOMMENT AS APPROVALCOMMENT ");
            sql.Append(", MAJ.STATUS ");
            sql.Append(", MAJ.MAJORSNNUSER AS APPROVALUSER ");
            sql.Append("FROM MAJORAPPROVAL_T MAJ ");
            sql.Append("WHERE ");
            sql.Append("MAJ.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND MAJ.CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("AND MAJ.LOCATIONID = '");
            sql.Append(locationId);
            sql.Append("' ");
            sql.Append("AND MAJ.REPORTID = '");
            sql.Append(reportId);
            sql.Append("' ");
            sql.Append("AND MAJ.PERIOD = '");
            sql.Append(periodId);
            sql.Append("' ");
            sql.Append("AND MAJ.PERIODSTART <= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND MAJ.PERIODEND >= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND MAJ.STATUS <> '");
            sql.Append(ApprovalStatus.PENDING);
            sql.Append("' ");
            sql.Append("UNION ALL ");
            sql.Append("SELECT ");
            sql.Append("'");
            sql.Append(ApprovalCategory.NODE_CLASS_FACILITY);
            sql.Append("' AS APPROVALNODE ");
            sql.Append(", TO_CHAR(TO_DATE(FACI.FACILITYSNNDATE,'YYYY/MM/DD HH24:MI:SS'), 'YYYY/MM/DD HH24:MI:SS') AS APPROVALDATE ");
            sql.Append(", FACI.FACILITYSNNCOMMENT AS APPROVALCOMMENT ");
            sql.Append(", FACI.STATUS ");
            sql.Append(", FACI.FACILITYSNNUSER AS APPROVALUSER ");
            sql.Append("FROM FACILITYAPPROVAL_T FACI ");
            sql.Append("WHERE ");
            sql.Append("FACI.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND FACI.CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("AND FACI.PERIOD = '");
            sql.Append(periodId);
            sql.Append("' ");
            sql.Append("AND FACI.PERIODSTART <= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND FACI.PERIODEND >= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND FACI.STATUS <> '");
            sql.Append(ApprovalStatus.PENDING);
            sql.Append("' ");
            sql.Append("ORDER BY APPROVALDATE ");
            sql.Append("FOR READ ONLY ");
            var history = context.Database.SqlQuery<ApprovalHistory>(sql.ToString());
            if (history.Count() > 0)
            {
                var worker = from w in context.WorkerMs
                             where w.SHOPID == shopId
                             select w;

                List < ApprovalHistory > historyList = new List<ApprovalHistory>();
                foreach (ApprovalHistory dt in history)
                {
                    WorkerM work = worker.Where(a => a.WORKERID == dt.APPROVALUSER).FirstOrDefault();
                    if (work != null)
                    {
                        dt.APPROVALUSERNAME = work.WORKERNAME;
                    }
                    historyList.Add(dt);
                }

                return historyList;
            }

            return new List<ApprovalHistory>();
        }

        /// <summary>
        /// 大分類承認依頼状態取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodYMD">周期指定日</param>
        /// <returns></returns>
        private ApprovalHistory GetRequestState(
            string shopId, string categoryId, string locationId, string reportId, string periodId, string periodYMD, string upddate)
        {
            // 大分類依頼状態取得
            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MAJ.STATUS ");
            sql.Append(", MAJ.UPDDATE ");
            sql.Append("FROM ");
            sql.Append("MAJORAPPROVAL_T MAJ ");
            sql.Append("WHERE ");
            sql.Append("MAJ.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND MAJ.CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("AND MAJ.LOCATIONID = '");
            sql.Append(locationId);
            sql.Append("' ");
            sql.Append("AND MAJ.REPORTID = '");
            sql.Append(reportId);
            sql.Append("' ");
            sql.Append("AND MAJ.PERIOD = '");
            sql.Append(periodId);
            sql.Append("' ");
            sql.Append("AND MAJ.PERIODSTART <= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND MAJ.PERIODEND >= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND MAJ.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            if (!string.IsNullOrEmpty(upddate))
            {
                sql.Append("AND TO_CHAR(MAJ.UPDDATE, 'YYYY/MM/DD HH24:MI:SS.FF') = '");
                sql.Append(upddate);
                sql.Append("' ");
            }
            sql.Append("FOR READ ONLY ");
            var mojor = context.Database.SqlQuery<ApprovalHistory>(sql.ToString());
            if (mojor.Count() > 0)
            {
                return mojor.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// 大分類ドロップダウンリスト取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <returns>大分類ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetCategory(string shopId, string categoryId)
        {
            return context.CategoryMs.Where(c => c.SHOPID == shopId).Select(c => new SelectListItem
            {
                Text = c.CATEGORYNAME,
                Value = c.CATEGORYID,
                Selected = c.CATEGORYID == categoryId
            });
        }

        /// <summary>
        /// 中分類ドロップダウンリスト取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <returns>中分類ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetLocation(string shopId, string locationId)
        {
            return context.LocationMs.Where(l => l.SHOPID == shopId).Select(l => new SelectListItem
            {
                Text = l.LOCATIONNAME,
                Value = l.LOCATIONID,
                Selected = l.LOCATIONID == locationId
            });
        }

        /// <summary>
        /// 帳票ドロップダウンリスト取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <returns>帳票ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetReport(string shopId, string categoryId, string locationId, string reportId)
        {
            return context.ReportMs.Where(r => r.SHOPID == shopId && r.CATEGORYID == categoryId && r.LOCATIONID == locationId).Select(r => new SelectListItem
            {
                Text = r.REPORTNAME,
                Value = r.REPORTID,
                Selected = r.REPORTID == reportId
            });
        }

        /// <summary>
        /// 桁数チェック
        /// </summary>
        /// <param name="dt">画面用承認データ</param>
        /// <param name="hsError">エラー文言</param>
        /// <returns>チェック結果</returns>
        private bool CheckInput(MiddleData middle, int cnt, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 桁数チェック
            if (!string.IsNullOrEmpty(middle.MIDDLESNNCOMMENT) && middle.MIDDLESNNCOMMENT.Length > 50)
            {
                hsError.Add("承認コメントは50文字以内で入力してください。");
                ModelState.AddModelError("list[" + cnt + "].MIDDLESNNCOMMENT", string.Empty);
                checkError = false;
            }

            return checkError;
        }

        /// <summary>
        /// 桁数チェック
        /// </summary>
        /// <param name="RequestComment">初認依頼コメント</param>
        /// <param name="hsError">エラー文言</param>
        /// <returns>チェック結果</returns>
        private bool CheckReqComment(string RequestComment, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 桁数チェック
            if (!string.IsNullOrEmpty(RequestComment) && RequestComment.Length > 50)
            {
                hsError.Add("承認依頼コメントは50文字以内で入力してください。");
                ModelState.AddModelError("RequestComment", string.Empty);
                checkError = false;
            }

            return checkError;
        }

    }
}