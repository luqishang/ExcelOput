using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using HACCPExtender.Models.Custom;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class FacilityApprovalController : Controller
    {
        private MasterContext context = new MasterContext();
        private readonly CommonFunction comm = new CommonFunction();
        private MasterFunction masterFunction = new MasterFunction();

        // 処理ボタン（承認）
        private readonly string MODE_APPROVAL = "1";
        // 処理ボタン（差戻）
        private readonly string MODE_REMAND = "2";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FacilityApprovalController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "FacilityApproval");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// データ承認画面表示処理（施設）
        /// </summary>
        /// <returns>画面表示</returns>
        [HttpGet]
        public ActionResult Show()
        {
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

            BFacilityApproval data = this.GetFacilityData(periodId, periodStart);
            data.PeriodDay = comm.FormatDateStrhyphen(periodStart);

            return View(data);
        }

        /// <summary>
        /// 条件変更(周期指定日日、大分類ドロップダウンリスト)
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
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];

            // データ取得
            BFacilityApproval data = this.GetFacilityData(periodId, periodYMD);
            data.PeriodDay = periodDay;

            return View("Show", data);
        }

        /// <summary>
        /// 承認（差戻）処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <param name="list">画面入力値承認データ情報</param>
        /// <returns>画面表示</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult RegistApproval(FormCollection form, IList<FacilityData> list)
        {
            // post時の情報をクリア
            ModelState.Clear();

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // 画面から値を取得
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];
            string shoriMode = form["ShoriMode"];

            // 承認者
            string updUserId = (string)Session["LOGINMNGID"];
            var updFacilityTs = new List<FacilityApprovalT>();

            // 承認対象チェック
            bool chkFlg = false;
            // 行数
            int cnt = 0;
            // 入力チェック
            bool inputCheck = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();
            // ステータス
            string updStatus = ApprovalStatus.APPROVAL;
            if (MODE_REMAND.Equals(shoriMode))
            {
                // ステータス：差戻
                updStatus = ApprovalStatus.REMAND;
            }

            foreach (FacilityData facility in list)
            {
                if (facility.DataChk)
                {
                    chkFlg = true;

                    // 中分類承認スキップの場合、差戻不可
                    if (MODE_REMAND.Equals(shoriMode) && StampField.STAMP_FACILITYMANAGER.Equals(facility.STAMPFIELD))
                    {
                        hsError.Add("大分類、中分類承認をスキップしているデータは差戻を実行できません。");
                        ModelState.AddModelError("list[" + cnt + "].DataChk", string.Empty);
                        inputCheck = false;
                        cnt++;
                        continue;
                    }
                    // 承認完了済みステータスの場合
                    if (MODE_REMAND.Equals(shoriMode) && facility.CompleteData)
                    {
                        hsError.Add("既に承認完了されています。");
                        ModelState.AddModelError("list[" + cnt + "].DataChk", string.Empty);
                        inputCheck = false;
                        cnt++;
                        continue;
                    }
                    // 承認処理不可能なデータの場合
                    if (MODE_APPROVAL.Equals(shoriMode) && !facility.ApprovalFlg)
                    {
                        hsError.Add("承認できないデータです。");
                        ModelState.AddModelError("list[" + cnt + "].DataChk", string.Empty);
                        inputCheck = false;
                        cnt++;
                        continue;
                    }
                    // 差戻処理不可能でデータの場合
                    if (MODE_REMAND.Equals(shoriMode) && !facility.RemandFlg)
                    {
                        hsError.Add("差戻できないデータです。");
                        ModelState.AddModelError("list[" + cnt + "].DataChk", string.Empty);
                        inputCheck = false;
                        cnt++;
                        continue;
                    }

                    // 施設承認情報データ
                    var facilityDt = from faci in context.FacilityApprovalTs
                                  where faci.SHOPID == shopId
                                     && faci.CATEGORYID == facility.CATEGORYID
                                     && faci.PERIOD == periodId
                                     && faci.PERIODSTART == periodYMD
                                     && faci.DELETEFLAG == DeleteFlg.NODELETE
                                  select faci;

                    if (facilityDt.Count() > 0)
                    {
                        FacilityApprovalT dbval = facilityDt.FirstOrDefault();

                        // ステータス
                        dbval.STATUS = updStatus;
                        // 承認日時
                        dbval.FACILITYSNNDATE = DateTime.Now.ToString("yyyyMMddHHmmss");
                        // 承認コメント
                        dbval.FACILITYSNNCOMMENT = facility.FACILITYSNNCOMMENT;
                        // 承認ユーザーID
                        dbval.FACILITYSNNUSER = updUserId;
                        // 更新ユーザーID
                        dbval.UPDUSERID = updUserId;
                        // 更新日時
                        dbval.UPDDATE = DateTime.Parse(facility.UPDDATE);

                        updFacilityTs.Add(dbval);
                    } else
                    {
                        inputCheck = false;
                        // データ排他
                        ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    }

                    // コメントデータ入力チェック
                    if (!CheckInput(facility, cnt, shoriMode, ref hsError)) inputCheck = false;
                }
                cnt++;
            }

            // 画面用施設承認データ
            var bFacilityApproval = new BFacilityApproval
            {
                // 周期ドロップ取得
                PeriodDrop = comm.GetPeriodDropList(periodId),
                // 周期ID
                Period = periodId,
                // 周期指定日(YYYY/MM/DD)
                PeriodDay = periodDay,
                // 周期指定日(YYYYMMDD)
                PeriodYMD = periodYMD,
                // データリスト
                FacilityDatas = list.ToList(),
                // 承認ボタン活性
                ApprovalBtn = Convert.ToBoolean(form["ApprobalBtn"]),
                //  差戻ボタン活性
                RemandBtn = Convert.ToBoolean(form["RemandBtn"]),
                // 承認依頼ボタン活性
                CompleteBtn = Convert.ToBoolean(form["CompleteBtn"])
            };

            // チェックがない場合はエラー
            if (!chkFlg)
            {
                ModelState.AddModelError(string.Empty, MsgConst.NO_CHOISE_APPROVAL_DATA);
                return View("Show", bFacilityApproval);
            }

            // 入力チェックエラーが存在する場合
            if (!inputCheck)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }

                return View("Show", bFacilityApproval);
            }

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 更新処理
                        foreach (FacilityApprovalT upddata in updFacilityTs)
                        {
                            context.FacilityApprovalTs.Attach(upddata);
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
                        return View("Show", bFacilityApproval);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", bFacilityApproval);
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

            // 大分類承認者へ差戻メールを送信
            if (MODE_REMAND.Equals(shoriMode))
            {
                using (context = new MasterContext())
                {
                    var sender = new MailSenderFunction();
                    var sendMail = new SendMailBusiness();
                    foreach (FacilityApprovalT upddata in updFacilityTs)
                    {
                        // 承認者
                        var sendManager = from appRoute in context.ApprovalRouteMs
                                          where appRoute.SHOPID == shopId
                                          && appRoute.CATEGORYID == upddata.CATEGORYID
                                          && appRoute.LOCATIONID == ApprovalCategory.MAJORDATA_LOCATION
                                          select appRoute;

                        var mailToList = sender.SetMailAddress(context, sendManager);

                        // メール送信
                        if (mailToList.Count() > 0)
                        {
                            // 大分類名
                            var categoryDt = context.CategoryMs.Where(ca => ca.SHOPID == shopId && ca.CATEGORYID == upddata.CATEGORYID);

                            // メールタイトル作成
                            var titleStr = GetAppSet.GetAppSetValue("FacilityRemandMail", "Subject");
                            var systemName = GetAppSet.GetAppSetValue("Mail", "SYSTEMNAME");
                            string title = titleStr.Replace("%SYSTEMNAME%", systemName)
                                                    .Replace("%CATEGORY%", categoryDt.FirstOrDefault().CATEGORYNAME);

                            // メール本文作成 
                            string URL = sendMail.SetURL(URLShoriKBN.MAJOR_APPROVAL, upddata.CATEGORYID, null, null, periodId, upddata.PERIODSTART, shopId);
                            string body = string.Empty;
                            var bodyStr = HostingEnvironment.MapPath(GetAppSet.GetAppSetValue("FacilityRemandMail", "BodyTemplate"));
                            using (var sr = new StreamReader(bodyStr, Encoding.GetEncoding("shift_jis")))
                            {
                                // パラメータ（%～%）の置換
                                body = sr.ReadToEnd()
                                        .Replace("%SYSTEMNAME%", DateTime.Now.ToString("yyyy/MM/dd"))
                                        .Replace("%REMANDCOMMENT%", upddata.FACILITYSNNCOMMENT)
                                        .Replace("%URL%", URL);
                            }

                            // メール送信
                            sender.SendMail(
                                mailToList,
                                new List<MailInfo>(),
                                new List<MailInfo>(),
                                sender.GetSendMailAddress(),
                                title,
                                body,
                                null,   // 添付ファイル
                                GetAppSet.GetAppSetValue("FacilityRemandMail", "ContentType"));
                        }
                    }
                }
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);

            // 画面再表示
            Session.Add("PENDINGNODEID", APPROVALLEVEL.FACILITY);
            Session.Add("PENDINGPERIODID", periodId);
            Session.Add("PENDINGSTARTDATE", periodYMD);

            return RedirectToAction("Show");
        }

        /// <summary>
        /// 承認完了処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <param name="list">画面入力値承認データ情報</param>
        /// <returns>画面表示</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CompleteApproval(FormCollection form, IList<FacilityData> list)
        {
            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // 画面から値を取得
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];
            string completeComment = form["CompleteComment"];
            // 承認者
            string updUserId = (string)Session["LOGINMNGID"];
            // 更新日時
            string updDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            // 入力チェック
            bool inputCheck = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            // 画面用大分類承認データ
            var bFacilityApproval = new BFacilityApproval
            {
                // 周期ドロップ取得
                PeriodDrop = comm.GetPeriodDropList(periodId),
                // 周期ID
                Period = periodId,
                // 周期指定日(YYYY/MM/DD)
                PeriodDay = periodDay,
                // 周期指定日(YYYYMMDD)
                PeriodYMD = periodYMD,
                // データリスト
                FacilityDatas = list.ToList(),
                // 承認ボタン活性
                ApprovalBtn = false,
                // 差戻ボタン活性
                RemandBtn = false,
                // 承認完了ボタン活性
                CompleteBtn = true,
                // 承認依頼コメント
                CompleteComment = completeComment
            };

            bool pendingData = false;
            // 周期開始日リスト
            var periodList = new Dictionary<string, string>();
            var completeList = new Dictionary<string, bool>();
            var completeInfo = new Dictionary<string, string>();
            // 帳票データエンティティリスト
            var bReportList = new List<BReportInterface>();
            // 帳票テンプレートマスタ
            var reportTempMDt = from reTemp in context.ReportTemplateMs
                                where reTemp.SHOPID == shopId
                                select reTemp;

            foreach (FacilityData fadt in list)
            {
                if (ApprovalStatus.PENDING.Equals(fadt.STATUS)
                    || ApprovalStatus.REMAND.Equals(fadt.STATUS))
                {
                    // 承認済み以外のデータが存在する場合
                    pendingData = true;
                    break;
                }

                // 周期開始日マップ作成
                if (!periodList.ContainsKey(fadt.PERIODSTART))
                {
                    periodList.Add(fadt.PERIODSTART, fadt.PERIODEND);
                    completeList.Add(fadt.PERIODSTART, fadt.CompleteData);
                }

                // 帳票対象データ取得
                var reporList = this.GetReportJoho(shopId, fadt.CATEGORYID, periodId, fadt.PERIODSTART);
                if (reporList != null)
                {
                    bReportList.AddRange(reporList);
                }
            }

            if (pendingData)
            {
                // 承認待ち・差戻データが存在するため、完了不可
                bFacilityApproval.CompleteBtn = false;
                ModelState.AddModelError(string.Empty, MsgConst.SNNREQUEST_PENDING_ERR);
                return View("Show", bFacilityApproval);
            }

            // 更新用施設承認情報
            var UpdFacilityList = new List<FacilityApprovalT>();
            // 登録用承認完了情報
            var addCompleteList = new List<ApprovalCompleteT>();


            foreach (KeyValuePair<string, bool> facility in completeList)
            {
                if (facility.Value)
                {
                    // 承認完了データが存在する場合は、処理スキップでデータ登録しない
                    continue;
                }

                // 施設承認依頼ボタン活性
                bool completeDt = this.GetRequestState(
                shopId, periodId, facility.Key);

                if (completeDt)
                {
                    // 承認状態が変更されているため、排他エラー
                    bFacilityApproval.CompleteBtn = false;
                    ModelState.AddModelError(string.Empty, MsgConst.MAJORREQUEST_DATACHANGE);
                    return View("Show", bFacilityApproval);
                }

                // 承認依頼コメント入力チェック
                if (!CheckReqComment(completeComment, ref hsError)) inputCheck = false;

                // 入力チェックエラーが存在する場合
                if (!inputCheck)
                {
                    foreach (string word in hsError)
                    {
                        ModelState.AddModelError(string.Empty, word);
                    }

                    return View("Show", bFacilityApproval);
                }

                // 施設承認グループ連番
                int facilityGroupNo = 0;
                // 登録用承認完了データ
                var addComplete = new ApprovalCompleteT
                {
                    SHOPID = shopId,
                    PERIOD = periodId,
                    PERIODSTART = facility.Key,
                    PERIODEND = periodList[facility.Key],
                    FACILITYAPPGROUPNO = 1,
                    APPROVALCOMMENT = completeComment,
                    APPROVALWORKERID = updUserId,
                    INSUSERID = updUserId,
                    UPDUSERID = updUserId,
                };
                addCompleteList.Add(addComplete);

                bool facilityErr = false;
                // 施設承認情報を取得
                var facilityDt = from faci in context.FacilityApprovalTs
                                 where faci.SHOPID == shopId
                                  && faci.PERIOD == periodId
                                  && faci.PERIODSTART == facility.Key
                                  && faci.DELETEFLAG == DeleteFlg.NODELETE
                                 select faci;
                if (facilityDt.Count() != list.Count())
                {
                    // データ数が違うため、排他エラー
                    facilityErr = true;
                }

                foreach (FacilityData faciltdt in list.Where(a => a.PERIODSTART == facility.Key))
                {
                    // データを取得
                    var facilityapp = facilityDt.Where(a => a.CATEGORYID == faciltdt.CATEGORYID).FirstOrDefault();
                    if (facilityapp == null)
                    {
                        // データが更新されているため、排他エラー
                        facilityErr = true;
                        continue;
                    }

                    // 施設承認グループ連番
                    facilityapp.FACILITYAPPGROUPNO = (short)(facilityGroupNo + 1);
                    // 更新年月日
                    facilityapp.UPDDATE = DateTime.Parse(faciltdt.UPDDATE);
                    // 更新ユーザーID
                    facilityapp.UPDUSERID = updUserId;

                    UpdFacilityList.Add(facilityapp);
                }

                // 画面とDBデータに差異があるため、エラー
                if (facilityErr)
                {
                    bFacilityApproval.CompleteBtn = false;
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    return View("Show", bFacilityApproval);
                }
            }
            // 帳票格納レジストリパスを取得
            string reportStoragePath = masterFunction.GetReportFolderName(context, shopId);

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 施設承認データ更新
                        foreach (FacilityApprovalT updFacilityDt in UpdFacilityList)
                        {
                            context.FacilityApprovalTs.Attach(updFacilityDt);
                            context.Entry(updFacilityDt).State = EntityState.Modified;
                        }
                        context.SaveChanges();

                        // 承認完了データ登録
                        foreach (ApprovalCompleteT complete in addCompleteList)
                        {
                            context.ApprovalCompleteTs.Add(complete);
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
                        return View("Show", bFacilityApproval);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", bFacilityApproval);
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
                        LogHelper.Default.WriteError(ex.Message, ex);
                        // ロールバック
                        tran.Rollback();
                        throw new ApplicationException();
                    }

                }
            }

            // （周期、周期開始日）単位ごとに処理を行う
            foreach (KeyValuePair<string, string> period in periodList)
            {
                // 対象周期の帳票データを取得
                var reportInterface = bReportList.Where(a => a.PeriodStart == period.Key);
                if (reportInterface != null)
                {
                    // 帳票テンプレート単位で処理
                    var customReportList = comm.SetReportInterface(shopId, updUserId, reportInterface.ToList(), reportStoragePath);
                    // 帳票出力の呼び出し
                    comm.CallReportOutputPdf(customReportList, reportTempMDt);
                }
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_REQUEST_APPROVAL);

            // 画面再表示
            Session.Add("PENDINGNODEID", APPROVALLEVEL.FACILITY);
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
            Session.Add("PENDINGNODEID", APPROVALLEVEL.FACILITY);
            Session.Add("PENDINGCATEGORYID", form["SelCategoryId"]);
            Session.Add("PENDINGPERIODID", form["Period"]);
            Session.Add("PENDINGSTARTDATE", form["PeriodYMD"]);

            return RedirectToAction("Show", "ApprovalDataDetail");
        }

        /// <summary>
        /// 施設承認データ取得
        /// </summary>
        /// <param name="periodId">周期</param>
        /// <param name="periodYMD">周期指定日(YYYYMMDD)</param>
        /// <returns>施設承認データ</returns>
        private BFacilityApproval GetFacilityData(string periodId, string periodYMD)
        {
            // データ表示
            bool display = true;

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            if (string.IsNullOrEmpty(periodId)
                || string.IsNullOrEmpty(periodYMD))
            {
                // データ非表示
                display = false;
                ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
            }

            // 画面用大分類承認データ
            var bFacilityApproval = new BFacilityApproval
            {
                // 周期ドロップ取得
                PeriodDrop = comm.GetPeriodDropList(periodId),
                // 周期ID
                Period = periodId,
                // 周期指定日
                PeriodYMD = periodYMD,
                // 承認ボタン活性
                ApprovalBtn = false,
                // 差戻ボタン活性
                RemandBtn = false,
                // 承認完了ボタン活性
                CompleteBtn = true
            };

            // データ取得
            if (display)
            {
                // 承認者
                string managerId = (string)Session["LOGINMNGID"];
                var tranfunction = new TransactionFunction(context);
                // 承認権限チェック
                if (!tranfunction.GetApprovalAuthority(
                        shopId, 
                        ApprovalCategory.FACILITYDATA_CATEGORY, 
                        ApprovalCategory.MAJORDATA_LOCATION, 
                        managerId, 
                        ApprovalCategory.FACILITY))
                {
                    bFacilityApproval.FacilityDatas = new List<FacilityData>();
                    ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
                    return bFacilityApproval;
                }

                // 承認対象データ
                var facility = this.GetFacilityPendingData(
                    shopId, periodId, periodYMD);

                // 処理中周期開始日
                string processPeriodStart = string.Empty;
                // 処理中承認完了データ有無
                bool processCompleteData = false;

                if (facility.Count() > 0)
                {
                    var dtList = new List<FacilityData>();

                    foreach (FacilityData dt in facility)
                    {
                        // 周期開始日（表示用）
                        dt.PERIODSTARTDATE = comm.FormatDateStr(dt.PERIODSTART);
                        // 周期終了日（表示用）
                        dt.PERIODENDDATE = comm.FormatDateStr(dt.PERIODEND);

                        if (processPeriodStart.Equals(dt.PERIODSTART))
                        {
                            dt.CompleteData = processCompleteData;
                        }
                        else
                        {
                            // 周期開始日が切り替わった場合
                            processPeriodStart = dt.PERIODSTART;

                            // 承認完了データの有無
                            processCompleteData = this.GetRequestState(
                                shopId, periodId, periodYMD);

                            if (processCompleteData)
                            {
                                // 承認完了ボタン非活性
                                bFacilityApproval.CompleteBtn = false;
                            }
                        }

                        dt.ApprovalFlg = false;
                        dt.RemandFlg = false;

                        // 承認ボタン・差戻ボタン活性制御
                        if (ApprovalStatus.PENDING.Equals(dt.STATUS))
                        {
                            // ステータス=承認待ち
                            dt.ApprovalFlg = true;
                            dt.RemandFlg = true;

                            bFacilityApproval.ApprovalBtn = true;
                            bFacilityApproval.RemandBtn = true;
                            bFacilityApproval.CompleteBtn = false;
                        }
                        else if (ApprovalStatus.REMAND.Equals(dt.STATUS))
                        {
                            // ステータス=差戻
                            bFacilityApproval.CompleteBtn = false;
                        }

                        dtList.Add(dt);
                    }
                    // 施設承認データ
                    bFacilityApproval.FacilityDatas = dtList;
                }
                else
                {
                    bFacilityApproval.FacilityDatas = new List<FacilityData>();
                    ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
                }
            }

            return bFacilityApproval;
        }

        /// <summary>
        /// 施設承認待ちデータ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="periodId">周期</param>
        /// <param name="periodYMD">周期指定日</param>
        /// <returns>施設承認待ちデータ</returns>
        private List<FacilityData> GetFacilityPendingData(
            string shopId, string periodId, string periodYMD)
        {
            // 承認対象データ
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("TO_CHAR(TO_DATE(FACI.FACILITYSNNDATE,'YYYY/MM/DD HH24:MI:SS'), 'YYYY/MM/DD HH24:MI:SS') AS FACILITYSNNDATE ");
            sql.Append(", FACI.PERIOD ");
            sql.Append(", FACI.PERIODSTART ");
            sql.Append(", FACI.PERIODEND ");
            sql.Append(", FACI.STATUS ");
            sql.Append(", FACI.FACILITYSNNCOMMENT ");
            sql.Append(", FACI.FACILITYSNNUSER ");
            sql.Append(", FACI.CATEGORYID ");
            sql.Append(", CA.CATEGORYNAME ");
            sql.Append(", W.WORKERNAME AS FACILITYREQWORKERNAME ");
            sql.Append(", FACI.FACILITYREQCOMMENT ");
            sql.Append(", FACI.FACILITYREQWORKERID ");
            sql.Append(", FACI.STAMPFIELD "); 
            sql.Append(", TO_CHAR(FACI.UPDDATE, 'YYYY/MM/DD HH24:MI:SS.FF') AS UPDDATE ");
            sql.Append("FROM FACILITYAPPROVAL_T FACI ");
            sql.Append("LEFT JOIN CATEGORY_M CA ON CA.SHOPID = FACI.SHOPID AND CA.CATEGORYID = FACI.CATEGORYID ");
            sql.Append("LEFT JOIN WORKER_M W ON W.SHOPID = FACI.SHOPID AND W.WORKERID = FACI.FACILITYREQWORKERID ");
            sql.Append("WHERE ");
            sql.Append("FACI.SHOPID = '");
            sql.Append(shopId);
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
            sql.Append("AND FACI.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("ORDER BY FACI.PERIODSTART ");
            sql.Append("FOR READ ONLY ");
            var facilityPending = context.Database.SqlQuery<FacilityData>(sql.ToString());

            if (facilityPending.Count() > 0)
            {
                return facilityPending.ToList();
            }

            return new List<FacilityData>();
        }

        /// <summary>
        /// 承認完了データ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodYMD">周期指定日</param>
        /// <returns>承認完了データ</returns>
        private bool GetRequestState(
            string shopId, string periodId, string periodYMD)
        {
            // 承認完了データ取得
            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("COMP.SHOPID  ");
            sql.Append(", COMP.PERIOD ");
            sql.Append(", COMP.PERIODSTART ");
            sql.Append("FROM ");
            sql.Append("APPROVALCOMPLETE_T COMP ");
            sql.Append("WHERE ");
            sql.Append("COMP.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND COMP.PERIOD = '");
            sql.Append(periodId);
            sql.Append("' ");
            sql.Append("AND COMP.PERIODSTART = '");
            sql.Append(periodYMD);
            sql.Append("' ");
            var status = context.Database.SqlQuery<ApprovalHistory>(sql.ToString());
            if (status.Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="major">データ承認データ（施設）</param>
        /// <param name="cnt">行数</param>
        /// <param name="shoriMode">処理モード</param>
        /// <param name="hsError">エラーメッセージ</param>
        /// <returns></returns>
        private bool CheckInput(FacilityData facility, int cnt, string shoriMode, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 差戻処理の場合
            if (MODE_REMAND.Equals(shoriMode))
            {
                // 入力チェック
                if (string.IsNullOrEmpty(facility.FACILITYSNNCOMMENT))
                {
                    hsError.Add("差戻コメントを入力してください。");
                    ModelState.AddModelError("list[" + cnt + "].FACILITYSNNCOMMENT", string.Empty);
                    checkError = false;
                }
            }

            // 桁数チェック
            if (!string.IsNullOrEmpty(facility.FACILITYSNNCOMMENT) && facility.FACILITYSNNCOMMENT.Length > 50)
            {
                hsError.Add("承認コメントは50文字以内で入力してください。");
                ModelState.AddModelError("list[" + cnt + "].FACILITYSNNCOMMENT", string.Empty);
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

        /// <summary>
        /// 帳票出力対象データ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <returns></returns>
        private List<BReportInterface> GetReportJoho(string shopId, string categoryId, string periodId, string periodStart)
        {
            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MAJ.SHOPID AS ShopId ");
            sql.Append(", MAJ.CATEGORYID AS CategoryId ");
            sql.Append(", MAJ.LOCATIONID AS LocationId ");
            sql.Append(", MAJ.REPORTID AS ReportId ");
            sql.Append(", MAJ.REPORTNAME AS title ");
            sql.Append(", MAJ.PERIOD AS Period ");
            sql.Append(", MAJ.PERIODSTART AS PeriodStart ");
            sql.Append(", REP.REPORTTEMPLATEID AS ReportTemplateId ");
            sql.Append(", RTEMP.MERGEUNIT AS ReportMergeUnit ");
            sql.Append("FROM ");
            sql.Append("MAJORAPPROVAL_T MAJ ");
            sql.Append("INNER JOIN FACILITYAPPROVAL_T FACI ");
            sql.Append("ON FACI.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND FACI.CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("AND FACI.PERIOD = '");
            sql.Append(periodId);
            sql.Append("' ");
            sql.Append("AND FACI.PERIODSTART = '");
            sql.Append(periodStart);
            sql.Append("' ");
            sql.Append("AND FACI.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("LEFT JOIN REPORT_M REP ");
            sql.Append("ON MAJ.SHOPID = REP.SHOPID ");
            sql.Append("AND MAJ.REPORTID = REP.REPORTID ");
            sql.Append("LEFT JOIN REPORTTEMPLATE_M RTEMP ");
            sql.Append("ON REP.SHOPID = RTEMP.SHOPID ");
            sql.Append("AND REP.REPORTTEMPLATEID = RTEMP.TEMPLATEID ");
            sql.Append("WHERE ");
            sql.Append("MAJ.SHOPID = FACI.SHOPID ");
            sql.Append("AND MAJ.CATEGORYID = FACI.CATEGORYID ");
            sql.Append("AND MAJ.PERIOD = FACI.PERIOD ");
            sql.Append("AND MAJ.PERIODSTART = FACI.PERIODSTART ");
            sql.Append("AND MAJ.MAJORGROUPNO = FACI.MAJORGROUPNO ");
            sql.Append("AND MAJ.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            var status = context.Database.SqlQuery<BReportInterface>(sql.ToString());
            if (status.Count() > 0)
            {
                return status.ToList();
            }
            return null;

        }
    }
}