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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class MajorApprovalController : Controller
    {
        private MasterContext context = new MasterContext();
        private CommonFunction comm = new CommonFunction();
        private MasterFunction masterFunction = new MasterFunction();

        // 処理ボタン（承認）
        private readonly string MODE_APPROVAL = "1";
        // 処理ボタン（差戻）
        private readonly string MODE_REMAND = "2";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MajorApprovalController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "MajorApproval");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// データ承認画面表示処理（大分類）
        /// </summary>
        /// <returns>画面表示</returns>
        [HttpGet]
        public ActionResult Show()
        {
            // 大分類ID
            string categoryId = (string)Session["PENDINGCATEGORYID"];
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

            BMajorApproval data = this.GetMajorData(categoryId, periodId, periodStart);
            data.PeriodDay = comm.FormatDateStrhyphen(periodStart);

            return View(data);
        }

        /// <summary>
        /// 条件変更（大分類ドロップダウンリスト）
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
            string categoryId = form["CategoryId"];
            string periodDay = form["PeriodDay"];
            string periodYMD = form["PeriodYMD"];
            string periodId = form["Period"];

            if (!string.IsNullOrEmpty(categoryId) && !string.IsNullOrEmpty(periodYMD) && !string.IsNullOrEmpty(periodId))
            {
                // データ取得
                BMajorApproval data = this.GetMajorData(categoryId, periodId, periodYMD);
                data.PeriodDay = periodDay;
                return View("Show", data);
            } else
            {
                var bMajorApproval = new BMajorApproval();
                bMajorApproval.CategoryDrop = this.GetCategory(shopId, categoryId);
                bMajorApproval.PeriodDrop = comm.GetPeriodDropList(periodId);
                bMajorApproval.PeriodDay = periodDay;
                bMajorApproval.PeriodYMD = periodYMD;

                return View("Show", bMajorApproval);
            }
        }

        /// <summary>
        /// 条件変更(周期指定日日、周期ドロップダウンリスト)
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
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];

            // データ取得
            BMajorApproval data = this.GetMajorData(categoryId, periodId, periodYMD);
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
        public ActionResult RegistApproval(FormCollection form, IList<MajorData> list)
        {
            // post時の情報をクリア
            ModelState.Clear();

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // 画面から値を取得
            string categoryId = form["CategoryId"];
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];
            string shoriMode = form["ShoriMode"];

            // 承認者
            string updUserId = (string)Session["LOGINMNGID"];
            // 更新大分類データ
            var updMajorTs = new List<MajorApprovalT>();
            // 登録大分類データ
            var insMajorTs = new List<MajorApprovalT>();
            // 承認対象チェック
            bool chkFlg = false;
            // 行数
            int cnt = 0;
            // 入力チェック
            bool inputCheck = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();
            string insdate = DateTime.Now.ToString("yyyyMMddHHmmss");

            // ステータス
            string updStatus = ApprovalStatus.APPROVAL;
            if (MODE_REMAND.Equals(shoriMode))
            {
                // ステータス：差戻
                updStatus = ApprovalStatus.REMAND;
            }

            foreach (MajorData major in list)
            {
                if (major.DataChk)
                {
                    chkFlg = true;

                    // 中分類承認スキップの場合、差戻不可
                    if (MODE_REMAND.Equals(shoriMode) && StampField.STAMP_GROUPLEADER.Equals(major.STAMPFIELD))
                    {
                        hsError.Add("中分類承認をスキップしているデータは差戻を実行できません。");
                        ModelState.AddModelError("list[" + cnt + "].DataChk", string.Empty);
                        inputCheck = false;
                        cnt++;
                        continue;
                    }
                    // 承認済みステータスの場合
                    if (MODE_REMAND.Equals(shoriMode) && ApprovalStatus.APPROVAL.Equals(major.FacilityStatus))
                    {
                        hsError.Add("施設承認で承認されています。施設承認から差戻を実行してください。");
                        ModelState.AddModelError("list[" + cnt + "].DataChk", string.Empty);
                        inputCheck = false;
                        cnt++;
                        continue;
                    }
                    // 承認処理不可能なデータの場合
                    if (MODE_APPROVAL.Equals(shoriMode) && !major.ApprovalFlg)
                    {
                        hsError.Add("承認できないデータです。");
                        ModelState.AddModelError("list[" + cnt + "].DataChk", string.Empty);
                        inputCheck = false;
                        cnt++;
                        continue;
                    }
                    // 差戻処理不可能でデータの場合
                    if (MODE_REMAND.Equals(shoriMode) && !major.RemandFlg)
                    {
                        hsError.Add("差戻できないデータです。");
                        ModelState.AddModelError("list[" + cnt + "].DataChk", string.Empty);
                        inputCheck = false;
                        cnt++;
                        continue;
                    }

                    // 大分類承認情報データ
                    var majorDt = from maj in context.MajorApprovalTs
                                   where maj.SHOPID == shopId
                                      && maj.CATEGORYID == categoryId
                                      && maj.LOCATIONID == major.LOCATIONID
                                      && maj.REPORTID == major.REPORTID
                                      && maj.PERIOD == periodId
                                      && maj.PERIODSTART == major.PERIODSTART
                                      && maj.DELETEFLAG == DeleteFlg.NODELETE
                                   select maj;

                    if (majorDt.Count() > 0)
                    {
                        MajorApprovalT dbval = majorDt.FirstOrDefault();

                        // 施設承認からの差戻データの場合
                        if (MODE_REMAND.Equals(shoriMode) && ApprovalStatus.REMAND.Equals(major.FacilityStatus))
                        {
                            // 新規登録データ
                            MajorApprovalT insVal = majorDt.FirstOrDefault().Clone();
                            // ステータス
                            insVal.STATUS = updStatus;
                            // 中分類承認グループ連番
                            insVal.MIDDLEGROUPNO = (short)(dbval.MIDDLEGROUPNO + 1);
                            // 承認日時
                            insVal.MAJORSNNDATE = insdate;
                            // 承認コメント
                            insVal.MAJORSNNCOMMENT = major.MAJORSNNCOMMENT;
                            // 承認ユーザー
                            insVal.MAJORSNNUSER = updUserId;

                            insMajorTs.Add(insVal);

                            // 論理削除フラグ
                            dbval.DELETEFLAG = DeleteFlg.DELETE;
                        } 
                        else
                        {
                            // ステータス
                            dbval.STATUS = updStatus;
                            // 承認日時
                            dbval.MAJORSNNDATE = insdate;
                            // 承認コメント
                            dbval.MAJORSNNCOMMENT = major.MAJORSNNCOMMENT;
                            // 承認ユーザー
                            dbval.MAJORSNNUSER = updUserId;
                        }

                        // 承認ユーザーID
                        dbval.MAJORSNNUSER = updUserId;
                        // 更新ユーザーID
                        dbval.UPDUSERID = updUserId;
                        // 更新日時
                        dbval.UPDDATE = DateTime.Parse(major.UPDDATE);

                        updMajorTs.Add(dbval);
                    }
                    else
                    {
                        inputCheck = false;
                        // データ排他
                        ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    }

                    // コメントデータ入力チェック
                    if (!CheckInput(major, cnt, shoriMode, ref hsError)) inputCheck = false;
                }
                cnt++;
            }

            // 画面用大分類承認データ
            var bMajorApproval = new BMajorApproval
            {
                // 大分類ドロップ取得
                CategoryDrop = this.GetCategory(shopId, categoryId),
                // 周期ドロップ取得
                PeriodDrop = comm.GetPeriodDropList(periodId),
                // 周期ID
                Period = periodId,
                // 周期指定日(YYYY/MM/DD)
                PeriodDay = periodDay,
                // 周期指定日(YYYYMMDD)
                PeriodYMD = periodYMD,
                // データリスト
                MajorDatas = list.ToList(),
                // 承認ボタン活性
                ApprovalBtn = Convert.ToBoolean(form["ApprobalBtn"]),
                //  差戻ボタン活性
                RemandBtn = Convert.ToBoolean(form["RemandBtn"]),
                // 承認依頼ボタン活性
                RequestBtn = Convert.ToBoolean(form["RequestBtn"])
            };

            // 承認履歴取得
            bMajorApproval.Historys = this.GetHistory(
                shopId, categoryId, periodId, periodYMD);

            // チェックがない場合はエラー
            if (!chkFlg)
            {
                ModelState.AddModelError(string.Empty, MsgConst.NO_CHOISE_APPROVAL_DATA);
                return View("Show", bMajorApproval);
            }

            // 入力チェックエラーが存在する場合
            if (!inputCheck)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }

                return View("Show", bMajorApproval);
            }

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 新規登録
                        foreach (MajorApprovalT ins in insMajorTs)
                        {
                            context.MajorApprovalTs.Add(ins);
                            context.SaveChanges();
                        }

                        // 更新処理
                        foreach (MajorApprovalT upddata in updMajorTs)
                        {
                            context.MajorApprovalTs.Attach(upddata);
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
                        return View("Show", bMajorApproval);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", bMajorApproval);
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

            // 中分類承認者へ差戻メールを送信
            if (MODE_REMAND.Equals(shoriMode))
            {
                using (context = new MasterContext())
                {
                    var sender = new MailSenderFunction();
                    var sendMail = new SendMailBusiness();
                    // 大分類名
                    var categoryDt = context.CategoryMs.Where(c => c.SHOPID == shopId && c.CATEGORYID == categoryId);

                    foreach (MajorApprovalT upddata in updMajorTs)
                    {
                        // 承認者
                        var sendManager = from appRoute in context.ApprovalRouteMs
                                          where appRoute.SHOPID == shopId
                                          && appRoute.CATEGORYID == categoryId
                                          && appRoute.LOCATIONID == upddata.LOCATIONID
                                          select appRoute;

                        var mailToList = sender.SetMailAddress(context, sendManager);
                        
                        // メール送信
                        if (mailToList.Count() > 0)
                        {
                            // 中分類名
                            var locationDt = context.LocationMs.Where(l => l.SHOPID == shopId && l.LOCATIONID == upddata.LOCATIONID);
                            // 帳票名
                            var reportDt = context.ReportMs.Where(r => r.SHOPID == shopId && r.REPORTID == upddata.REPORTID);
                            // 差戻コメントを取得
                            if (string.IsNullOrEmpty(upddata.MAJORSNNCOMMENT))
                            {
                                var majorCommentDt = from remandDt in context.MajorApprovalTs
                                                     where remandDt.SHOPID == shopId
                                                     && remandDt.CATEGORYID == categoryId
                                                     && remandDt.LOCATIONID == upddata.LOCATIONID
                                                     && remandDt.REPORTID == upddata.REPORTID
                                                     && remandDt.PERIOD == upddata.PERIOD
                                                     && remandDt.PERIODSTART == upddata.PERIODSTART
                                                     && remandDt.DELETEFLAG == DeleteFlg.NODELETE
                                                     select remandDt;
                                if (majorCommentDt != null && majorCommentDt.Count() > 0)
                                {
                                    var remanddt = majorCommentDt.FirstOrDefault();
                                    if (remanddt != null)
                                    {
                                        upddata.MAJORSNNCOMMENT = remanddt.MAJORSNNCOMMENT;
                                    }
                                }
                            }

                            // メールタイトル作成
                            var titleStr = GetAppSet.GetAppSetValue("MajorRemandMail", "Subject");
                            var systemName = GetAppSet.GetAppSetValue("Mail", "SYSTEMNAME");
                            // パラメータ（%～%）の置換
                            string　title = titleStr
                                    .Replace("%SYSTEMNAME%", systemName)
                                    .Replace("%CATEGORY%", categoryDt.FirstOrDefault().CATEGORYNAME)
                                    .Replace("%LOCATION%", locationDt.FirstOrDefault().LOCATIONNAME);

                            // メール本文作成 
                            string URL = sendMail.SetURL(URLShoriKBN.MIDDLE_APPROVAL, upddata.CATEGORYID, upddata.LOCATIONID, upddata.REPORTID, periodId, upddata.PERIODSTART, shopId);
                            string body = string.Empty;
                            var bodyStr = HostingEnvironment.MapPath(GetAppSet.GetAppSetValue("MajorRemandMail", "BodyTemplate"));
                            using (var sr = new StreamReader(bodyStr, Encoding.GetEncoding("shift_jis")))
                            {
                                // パラメータ（%～%）の置換
                                body = sr.ReadToEnd()
                                        .Replace("%SYSTEMNAME%", systemName)
                                        .Replace("%REMANDCOMMENT%", upddata.MAJORSNNCOMMENT)
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
                                GetAppSet.GetAppSetValue("MajorRemandMail", "ContentType"));
                        }

                    }
                }
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);

            // 画面再表示
            Session.Add("PENDINGNODEID", APPROVALLEVEL.MAJORE);
            Session.Add("PENDINGCATEGORYID", categoryId);
            Session.Add("PENDINGPERIODID", periodId);
            Session.Add("PENDINGSTARTDATE", periodYMD);

            return RedirectToAction("Show");
        }

        /// <summary>
        /// 施設承認依頼処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <param name="list">画面入力値承認データ情報</param>
        /// <returns>画面表示</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult RequestFacilityApproval(FormCollection form, IList<MajorData> list)
        {
            // post時の情報をクリア
            ModelState.Clear();

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // 画面から値を取得
            string categoryId = form["CategoryId"];
            string periodId = form["Period"];
            string periodYMD = form["PeriodYMD"];
            string periodDay = form["PeriodDay"];
            string requestComment = form["RequestComment"];
            // 承認者
            string updUserId = (string)Session["LOGINMNGID"];
            // 更新日時
            string updDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            // 入力チェック
            bool inputCheck = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            // 画面用大分類承認データ
            var bMajorApproval = new BMajorApproval
            {
                // 大分類ドロップ取得
                CategoryDrop = this.GetCategory(shopId, categoryId),
                // 周期ドロップ取得
                PeriodDrop = comm.GetPeriodDropList(periodId),
                // 周期ID
                Period = periodId,
                // 周期指定日(YYYY/MM/DD)
                PeriodDay = periodDay,
                // 周期指定日(YYYYMMDD)
                PeriodYMD = periodYMD,
                // データリスト
                MajorDatas = list.ToList(),
                // 承認ボタン活性
                ApprovalBtn = false,
                // 差戻ボタン活性
                RemandBtn = false,
                // 承認依頼ボタン活性
                RequestBtn = true,
                // 承認依頼コメント
                RequestComment = requestComment
            };

            // 承認履歴取得
            bMajorApproval.Historys = this.GetHistory(
                shopId, categoryId, periodId, periodYMD);

            bool pendingData = false;
            // 周期開始日リスト（周期開始日、周期終了日）
            var periodList = new Dictionary<string, string>();
            // 施設リスト（周期開始日, 施設承認情報更新日時）
            var facilityList = new Dictionary<string, string>();
            // 帳票データエンティティリスト
            var bReportList = new List<BReportInterface>();
            // 帳票マスタ
            var reportMDt = from reT in context.ReportMs
                               where reT.SHOPID == shopId
                               select reT;
            // 帳票テンプレートマスタ
            var reportTempMDt = from reTemp in context.ReportTemplateMs
                                where reTemp.SHOPID == shopId
                                select reTemp;

            foreach (MajorData madt in list)
            {
                if (ApprovalStatus.PENDING.Equals(madt.STATUS)
                    || ApprovalStatus.REMAND.Equals(madt.STATUS))
                {
                    // 承認済み以外のデータが存在する場合
                    pendingData = true;
                    break;
                }
                // 周期開始日リスト作成
                if (!periodList.ContainsKey(madt.PERIODSTART))
                {
                    periodList.Add(madt.PERIODSTART, madt.PERIODEND);
                    facilityList.Add(madt.PERIODSTART, madt.FacilityUpddate);
                }

                // 帳票データエンティティを設定
                var bReportInterface = new BReportInterface
                {
                    ShopId = shopId,
                    CategoryId = categoryId,
                    Period = periodId,
                    PeriodStart = madt.PERIODSTART,
                    LocationId = madt.LOCATIONID,
                    ReportId = madt.REPORTID
                };
                // 帳票テンプレートIDを設定
                string reportTemplateId = string.Empty;
                if (reportMDt != null && reportMDt.Count() > 0)
                {
                    var reportT = reportMDt.Where(a => a.REPORTID == madt.REPORTID);
                    if (reportT != null)
                    {
                        var reportTdata = reportT.FirstOrDefault();
                        // 帳票テンプレートID
                        reportTemplateId = reportTdata.REPORTTEMPLATEID;
                        bReportInterface.ReportTemplateId = reportTemplateId;
                        // 帳票タイトル
                        bReportInterface.Title = reportTdata.REPORTNAME;
                    }
                }
                if (!string.IsNullOrEmpty(reportTemplateId))
                {
                    // 帳票テンプレートマスタ
                    if (reportTempMDt != null && reportTempMDt.Count() > 0)
                    {
                        var repportTemplate = reportTempMDt.Where(a => a.TEMPLATEID == reportTemplateId).FirstOrDefault();
                        if (repportTemplate != null)
                        {
                            bReportInterface.ReportMergeUnit = repportTemplate.MERGEUNIT;
                        }
                    }
                }
                // 帳票データエンティティリストに設定
                bReportList.Add(bReportInterface);
            }

            if (pendingData)
            {
                // 承認待ち・差戻データが存在するため、依頼不可
                bMajorApproval.RequestBtn = false;
                ModelState.AddModelError(string.Empty, MsgConst.SNNREQUEST_PENDING_ERR);
                return View("Show", bMajorApproval);
            }

            // 更新用大分類承認情報
            var UpdMajorList = new List<MajorApprovalT>();
            // 登録用施設承認情報
            var addFacilityList = new List<FacilityApprovalT>();
            // 更新用施設承認情報
            var updFacilityList = new List<FacilityApprovalT>();

            // （大分類ID、周期、周期開始日）単位ごとに処理を行う
            foreach (KeyValuePair<string, string> facility in facilityList)
            {
                // 施設承認依頼ボタン活性
                var approve = this.GetRequestState(
                shopId, categoryId, periodId, facility.Key, facility.Value);

                if ((string.IsNullOrEmpty(facility.Value) && approve != null)
                    || (!string.IsNullOrEmpty(facility.Value) && approve == null))
                {
                    // 承認状態が変更されているため、排他エラー
                    bMajorApproval.RequestBtn = false;

                    if (facilityList.Count() > 1)
                    {
                        ModelState.AddModelError(string.Empty, "周期開始日：" + comm.FormatDateStr(facility.Key) + " のデータは施設承認のステータスが変更されています。");
                        return View("Show", bMajorApproval);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, MsgConst.FACILITYREQUEST_DATACHANGE);
                        return View("Show", bMajorApproval);
                    }
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

                    return View("Show", bMajorApproval);
                }

                // 大分類承認グループ連番
                int majorGroupNo = 0;
                // 登録用施設承認データ
                var addFacility = new FacilityApprovalT();
                // 更新用施設承認データ
                var updFacility = new FacilityApprovalT();

                // 施設承認情報を取得
                var facilityDt = from faci in context.FacilityApprovalTs
                                 where faci.SHOPID == shopId
                                   && faci.CATEGORYID == categoryId
                                   && faci.PERIOD == periodId
                                   && faci.PERIODSTART == facility.Key
                                   && faci.DELETEFLAG == DeleteFlg.NODELETE
                                 select faci;
                if (facilityDt.Count() > 0)
                {
                    // 更新用施設承認データ
                    updFacility = facilityDt.First();

                    // 中分類承認グループ連番
                    majorGroupNo = updFacility.MAJORGROUPNO;
                    // 倫理削除フラグ
                    updFacility.DELETEFLAG = DeleteFlg.DELETE;
                    // 更新ユーザーID
                    updFacility.UPDUSERID = updUserId;
                    updFacilityList.Add(updFacility);

                    // 新規登録用大分類承認情報
                    addFacility = this.SetAddFacilityData(updFacility, requestComment, updUserId, updDate);
                }
                else
                {
                    // 新規データを作成
                    addFacility.SHOPID = shopId;
                    addFacility.CATEGORYID = categoryId;
                    addFacility.PERIOD = periodId;
                    addFacility.PERIODSTART = facility.Key;
                    addFacility.PERIODEND = periodList[facility.Key];
                    addFacility.MAJORGROUPNO = (short)(majorGroupNo + 1);
                    addFacility.STATUS = ApprovalStatus.PENDING;
                    addFacility.FACILITYREQDATE = updDate;
                    addFacility.FACILITYREQCOMMENT = requestComment;
                    addFacility.FACILITYREQWORKERID = updUserId;
                    addFacility.DELETEFLAG = DeleteFlg.NODELETE;
                    addFacility.INSUSERID = updUserId;
                    addFacility.UPDUSERID = updUserId;
                }

                bool majorErr = false;
                // 大分類承認情報を取得
                var majorDt = from maj in context.MajorApprovalTs
                              where maj.SHOPID == shopId
                               && maj.CATEGORYID == categoryId
                               && maj.PERIOD == periodId
                               && maj.PERIODSTART == facility.Key
                               && maj.DELETEFLAG == DeleteFlg.NODELETE
                              select maj;
                if (majorDt.Count() != list.Where(a => a.PERIODSTART == facility.Key).Count())
                {
                    // データ数が違うため、排他エラー
                    majorErr = true;
                }

                foreach (MajorData majdt in list.Where(a => a.PERIODSTART == facility.Key))
                {
                    // データを取得
                    var majorapp = majorDt.Where(a => a.LOCATIONID == majdt.LOCATIONID && a.REPORTID == majdt.REPORTID).FirstOrDefault();
                    if (majorapp == null)
                    {
                        // データが更新されているため、排他エラー
                        majorErr = true;
                        break;
                    }

                    // 大分類承認グループ連番
                    majorapp.MAJORGROUPNO = (short)(majorGroupNo + 1);
                    // 更新年月日
                    majorapp.UPDDATE = DateTime.Parse(majdt.UPDDATE);
                    // 更新ユーザーID
                    majorapp.UPDUSERID = updUserId;

                    UpdMajorList.Add(majorapp);
                }

                // 画面とDBデータに差異があるため、エラー
                if (majorErr)
                {
                    bMajorApproval.RequestBtn = false;
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    return View("Show", bMajorApproval);
                }

                // 大分類承認情報の捺印数を設定
                addFacility.STAMPFIELD = majorDt.OrderByDescending(a => a.STAMPFIELD).FirstOrDefault().STAMPFIELD;
                addFacilityList.Add(addFacility);
            }

            // 帳票ファイルパスを取得する
            string reportStoragePath = masterFunction.GetReportFolderName(context, shopId);

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 大分類データ更新
                        foreach (MajorApprovalT updMajorDt in UpdMajorList)
                        {
                            context.MajorApprovalTs.Attach(updMajorDt);
                            context.Entry(updMajorDt).State = EntityState.Modified;
                        }
                        context.SaveChanges();

                        // 施設承認データ登録
                        foreach (FacilityApprovalT addFacility in addFacilityList)
                        {
                            context.FacilityApprovalTs.Add(addFacility);
                            context.SaveChanges();
                        }

                        // 施設承認データ更新
                        if (updFacilityList.Count() > 0)
                        {
                            foreach (FacilityApprovalT updFacility in updFacilityList)
                            {
                                context.FacilityApprovalTs.Attach(updFacility);
                                context.Entry(updFacility).State = EntityState.Modified;
                            }
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
                        return View("Show", bMajorApproval);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", bMajorApproval);
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

            // メール送信クラス
            var sendmail = new SendMailBusiness();
            var sender = new MailSenderFunction();

            // （大分類ID、周期、周期開始日）単位ごとに処理を行う
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

                // 施設承認の承認管理者を取得
                var mailToList = new List<MailInfo>();

                using (context = new MasterContext())
                {
                    // 施設承認者を取得
                    var nextManager = from appRoute in context.ApprovalRouteMs
                                      where appRoute.SHOPID == shopId
                                        && appRoute.CATEGORYID == ApprovalCategory.FACILITYDATA_CATEGORY
                                        && appRoute.LOCATIONID == ApprovalCategory.MAJORDATA_LOCATION
                                      select appRoute;

                    // メール情報リスト作成
                    mailToList = sender.SetMailAddress(context, nextManager);
                    // 施設承認担当者へメール送信
                    sendmail.SendFacilityRequestMail(context, mailToList, shopId, categoryId, periodId, period.Key);
                }
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_REQUEST_APPROVAL);

            // 画面再表示
            Session.Add("PENDINGNODEID", APPROVALLEVEL.MAJORE);
            Session.Add("PENDINGCATEGORYID", categoryId);
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
            Session.Add("PENDINGNODEID", APPROVALLEVEL.MAJORE);
            Session.Add("PENDINGCATEGORYID", form["CategoryId"]);
            Session.Add("PENDINGLOCATIONID", form["SelLocationId"]);
            Session.Add("PENDINGREPORTID", form["SelReportId"]);
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
        private FacilityApprovalT SetAddFacilityData(FacilityApprovalT facility, string comment, string updUserId, string snnDate)
        {
            int no = facility.MAJORGROUPNO;

            FacilityApprovalT addData = facility.Clone();
            // 大分類承認グループ連番
            addData.MAJORGROUPNO = (short)(no + 1);
            // 施設承認グループ連番
            addData.FACILITYAPPGROUPNO = 0;
            // ステータス
            addData.STATUS = ApprovalStatus.PENDING;
            // 施設承認依頼日時
            addData.FACILITYREQDATE = snnDate;
            // 施設承認依頼コメント
            addData.FACILITYREQCOMMENT = comment;
            // 施設承認依頼ユーザーID
            addData.FACILITYREQWORKERID = updUserId;
            // 承認（差戻）日時
            addData.FACILITYSNNDATE = null;
            // 承認（差戻）コメント
            addData.FACILITYSNNCOMMENT = null;
            // 承認管理者ユーザー
            addData.FACILITYSNNUSER = null;
            // 論理削除フラグ
            addData.DELETEFLAG = DeleteFlg.NODELETE;
            // 登録ユーザーID
            addData.INSUSERID = updUserId;
            // 更新ユーザーID
            addData.UPDUSERID = updUserId;

            return addData;
        }


        /// <summary>
        /// 大分類承認データ取得
        /// </summary>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="periodId">周期</param>
        /// <param name="periodYMD">周期指定日(YYYYMMDD)</param>
        /// <returns>大分類承認データ</returns>
        private BMajorApproval GetMajorData(string categoryId, string periodId, string periodYMD)
        {
            // データ表示
            bool display = true;
            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            if (string.IsNullOrEmpty(categoryId)
                || string.IsNullOrEmpty(periodId)
                || string.IsNullOrEmpty(periodYMD))
            {
                // データ非表示
                display = false;
                ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
            }

            // 画面用大分類承認データ
            var bMajorApproval = new BMajorApproval
            {
                // 大分類ドロップ取得
                CategoryDrop = this.GetCategory(shopId, categoryId),
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
                // 承認依頼ボタン活性
                RequestBtn = true
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
                        categoryId,
                        ApprovalCategory.MAJORDATA_LOCATION,
                        managerId,
                        ApprovalCategory.MAJOR))
                {
                    bMajorApproval.MajorDatas = new List<MajorData>();
                    ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
                    return bMajorApproval;
                }

                // 承認対象データ
                var major = this.GetMajorPendingData(
                    shopId, categoryId, periodId, periodYMD);

                // 処理中周期開始日
                string processPeriodStart = string.Empty;
                // 処理中施設承認ステータス
                string processFacilityStatus = string.Empty;
                // 処理中施設承認更新日時
                string processFacilityUpddt = string.Empty;

                if (major.Count() > 0)
                {
                    var dtList = new List<MajorData>();

                    foreach (MajorData dt in major)
                    {
                        // 周期開始日（表示用）
                        dt.PERIODSTARTDATE = comm.FormatDateStr(dt.PERIODSTART);
                        // 周期終了日（表示用）
                        dt.PERIODENDDATE = comm.FormatDateStr(dt.PERIODEND);

                        if (processPeriodStart.Equals(dt.PERIODSTART))
                        {
                            dt.FacilityStatus = processFacilityStatus;
                            dt.FacilityUpddate = processFacilityUpddt;
                        } else
                        {
                            // 周期開始日が切り替わった場合
                            processPeriodStart = dt.PERIODSTART;

                            // 施設承認データのステータス
                            var facilityStatus = this.GetRequestState(
                                shopId, categoryId, periodId, processPeriodStart, string.Empty);
                            
                            if (facilityStatus != null)
                            {
                                processFacilityStatus = facilityStatus.STATUS;
                                processFacilityUpddt = facilityStatus.UPDDATE.ToString("yyyy/MM/dd HH:mm:ss.ffffff");
                                dt.FacilityStatus = processFacilityStatus;
                                dt.FacilityUpddate = processFacilityUpddt;
                                if (ApprovalStatus.APPROVAL.Equals(processFacilityStatus))
                                {
                                    // 施設承認依頼ボタン非活性
                                    bMajorApproval.RequestBtn = false;
                                }
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

                            bMajorApproval.ApprovalBtn = true;
                            bMajorApproval.RemandBtn = true;
                            bMajorApproval.RequestBtn = false;
                        }
                        else if (ApprovalStatus.REMAND.Equals(dt.STATUS))
                        {
                            // ステータス=差戻
                            bMajorApproval.RequestBtn = false;
                        }
                        else
                        {
                            // ステータス=承認済み
                            if (ApprovalStatus.REMAND.Equals(processFacilityStatus))
                            {
                                dt.RemandFlg = true;
                                bMajorApproval.RemandBtn = true;
                            }
                        }

                        dtList.Add(dt);
                    }

                    // 大分類データ
                    bMajorApproval.MajorDatas = dtList;
                }
                else
                {
                    bMajorApproval.MajorDatas = new List<MajorData>();
                    ViewBag.noDateMsg = MsgConst.NO_DATA_APPROVAL_DATA;
                }

                // 承認履歴取得
                bMajorApproval.Historys = this.GetHistory(
                    shopId, categoryId, periodId, periodYMD);
            }

            return bMajorApproval;
        }

        /// <summary>
        /// 大分類承認待ちデータ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="periodId">周期</param>
        /// <param name="periodYMD">周期指定日</param>
        /// <returns>大分類承認待ちデータ</returns>
        private List<MajorData> GetMajorPendingData(
            string shopId, string categoryId, string periodId, string periodYMD)
        {
            // 承認対象データ
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("TO_CHAR(TO_DATE(MAJ.MAJORSNNDATE,'YYYY/MM/DD HH24:MI:SS'), 'YYYY/MM/DD HH24:MI:SS') AS MAJORSNNDATE ");
            sql.Append(", MAJ.MAJORSNNCOMMENT ");
            sql.Append(", MAJ.PERIOD ");
            sql.Append(", MAJ.PERIODSTART ");
            sql.Append(", MAJ.PERIODEND ");
            sql.Append(", MAJ.STATUS ");
            sql.Append(", MAJ.REPORTID ");
            sql.Append(", MAJ.REPORTNAME ");
            sql.Append(", MAJ.MAJORSNNCOMMENT ");
            sql.Append(", MAJ.MAJORSNNUSER ");
            sql.Append(", W.WORKERNAME AS MAJORREQWORKERNAME ");
            sql.Append(", MAJ.LOCATIONID ");
            sql.Append(", LO.LOCATIONNAME ");
            sql.Append(", MAJ.MAJORREQCOMMENT ");
            sql.Append(", MAJ.STAMPFIELD ");
            sql.Append(", TO_CHAR(MAJ.UPDDATE, 'YYYY/MM/DD HH24:MI:SS.FF') AS UPDDATE ");
            sql.Append("FROM MAJORAPPROVAL_T MAJ ");
            sql.Append("LEFT JOIN WORKER_M W ON W.SHOPID = MAJ.SHOPID AND W.WORKERID = MAJ.MAJORREQWORKERID ");
            sql.Append("LEFT JOIN LOCATION_M LO ON LO.SHOPID = MAJ.SHOPID AND LO.LOCATIONID = MAJ.LOCATIONID ");
            sql.Append("WHERE ");
            sql.Append("MAJ.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND MAJ.CATEGORYID = '");
            sql.Append(categoryId);
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
            sql.Append("AND (MAJ.STAMPFIELD = ");
            sql.Append(StampField.STAMP_GROUPLEADER);
            sql.Append(" OR MAJ.STAMPFIELD = ");
            sql.Append(StampField.STAMP_RESPOSNSIBLE);
            sql.Append(") ");
            sql.Append("AND MAJ.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("ORDER BY MAJ.REPORTNAME, MAJ.PERIODSTART, MAJ.LOCATIONID ");
            sql.Append("FOR READ ONLY ");
            var majorPending = context.Database.SqlQuery<MajorData>(sql.ToString());

            if (majorPending.Count() > 0)
            {
                return majorPending.ToList();
            }

            return new List<MajorData>();
        }

        /// <summary>
        /// 承認履歴情報取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodYMD">周期指定日</param>
        /// <returns>承認履歴情報</returns>
        private List<ApprovalHistory> GetHistory(string shopId, string categoryId, string periodId, string periodYMD)
        {
            // 承認履歴取得
            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("'");
            sql.Append(ApprovalCategory.NODE_CLASS_FACILITY);
            sql.Append("' AS APPROVALNODE ");
            sql.Append(", TO_CHAR(TO_DATE(FACI.FACILITYSNNDATE,'YYYY/MM/DD HH24:MI:SS'), 'YYYY/MM/DD HH24:MI:SS') AS APPROVALDATE ");
            sql.Append(", FACI.FACILITYSNNCOMMENT AS APPROVALCOMMENT ");
            sql.Append(", FACI.STATUS ");
            sql.Append(", FACI.FACILITYSNNUSER AS APPROVALUSER ");
            sql.Append(", FACI.PERIODSTART ");
            sql.Append(", FACI.PERIODEND ");
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

                List<ApprovalHistory> historyList = new List<ApprovalHistory>();
                foreach (ApprovalHistory dt in history)
                {
                    WorkerM work = worker.Where(a => a.WORKERID == dt.APPROVALUSER).FirstOrDefault();
                    if (work != null)
                    {
                        dt.APPROVALUSERNAME = work.WORKERNAME;
                    }
                    // 周期開始日（表示用）
                    dt.PERIODSTART = comm.FormatDateStr(dt.PERIODSTART);
                    // 周期終了日（表示用）
                    dt.PERIODEND = comm.FormatDateStr(dt.PERIODEND);

                    historyList.Add(dt);
                }

                return historyList;
            }

            return new List<ApprovalHistory>();
        }

        /// <summary>
        /// 施設承認依頼状態取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <returns>施設承認データステータス</returns>
        private ApprovalHistory GetRequestState(
            string shopId, string categoryId, string periodId, string periodStart, string upddate)
        {
            // 施設承認依頼状態取得
            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("FACI.STATUS ");
            sql.Append(", FACI.UPDDATE ");
            sql.Append("FROM ");
            sql.Append("FACILITYAPPROVAL_T FACI ");
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
            sql.Append("AND FACI.PERIODSTART = '");
            sql.Append(periodStart);
            sql.Append("' ");
            sql.Append("AND FACI.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("'");
            if (!string.IsNullOrEmpty(upddate))
            {
                sql.Append("AND TO_CHAR(FACI.UPDDATE, 'YYYY/MM/DD HH24:MI:SS.FF') = '");
                sql.Append(upddate);
                sql.Append("' ");
            }

            var facility = context.Database.SqlQuery<ApprovalHistory>(sql.ToString());
            if (facility.Count() > 0)
            {
                return facility.FirstOrDefault();
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
        /// 入力チェック
        /// </summary>
        /// <param name="major">データ承認データ（大分類）</param>
        /// <param name="cnt">行数</param>
        /// <param name="shoriMode">処理モード</param>
        /// <param name="hsError">エラーメッセージ</param>
        /// <returns></returns>
        private bool CheckInput(MajorData major, int cnt, string shoriMode, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 差戻処理の場合
            if (MODE_REMAND.Equals(shoriMode))
            {
                // 入力チェック
                if (string.IsNullOrEmpty(major.MAJORSNNCOMMENT))
                {
                    hsError.Add("差戻コメントを入力してください。");
                    ModelState.AddModelError("list[" + cnt + "].MAJORSNNCOMMENT", string.Empty);
                    checkError = false;
                }
            }

            // 桁数チェック
            if (!string.IsNullOrEmpty(major.MAJORSNNCOMMENT) && major.MAJORSNNCOMMENT.Length > 50)
            {
                hsError.Add("承認コメントは50文字以内で入力してください。");
                ModelState.AddModelError("list[" + cnt + "].MAJORSNNCOMMENT", string.Empty);
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