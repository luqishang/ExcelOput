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
    public class InitialDataPreviewController : Controller
    {
        private MasterContext context = new MasterContext();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InitialDataPreviewController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "InitioalDataPreview");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 画面表示
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpGet]
        public ActionResult Show()
        {
            // フォーマット系列店舗ID
            string formatShopId = (string)Session["FORMATSHOPID"];

            var masterF = new MasterFunction();
            // 画面情報
            var bInitialData = new BInitialData
            {
                // 大分類マスタ
                CategoryMDatas = masterF.GetCategoryMData(context, formatShopId),
                // 中分類マスタ
                LocationMDatas = masterF.GetLocationMData(context, formatShopId),
                // 設問マスタ
                QuestionMDatas = this.GetQuestionM(context, formatShopId)
            };

            return View(bInitialData);
        }

        /// <summary>
        /// マスタデータ登録（一括登録）
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit()
        {
            //店舗ID
            string shopId = (string)Session["SHOPID"];
            // フォーマット系列店舗ID
            string formatShopId = (string)Session["FORMATSHOPID"];

            // 大分類マスタデータ
            var categoryMList = this.SetCategoryMs(shopId, formatShopId);

            // 中分類マスタデータ
            var locationMList = this.SetLocationMs(shopId, formatShopId);

            // 帳票テンプレートマスタデータ
            var reportTempList = SetReportTemplateM(shopId, formatShopId);

            // 帳票マスタデータ
            var reportMList = this.SetReportMs(shopId, formatShopId);

            // 設問マスタデータ
            var questionMList = this.SetQuestionMs(shopId, formatShopId);

            // 作業者マスタデータ
            var workerMDt = this.SetWorkerMs(shopId);

            var masterF = new MasterFunction();
            // 画面情報
            var bInitialData = new BInitialData
            {
                // 大分類マスタ
                CategoryMDatas = masterF.GetCategoryMData(context, formatShopId),
                // 中分類マスタ
                LocationMDatas = masterF.GetLocationMData(context, formatShopId),
                // 設問マスタ
                QuestionMDatas = this.GetQuestionM(context, formatShopId)
            };

            // 回答種類マスタデータ（北川追加）
            var anstypeMList = this.SetShop_AnswerTypeMs(shopId, formatShopId);

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // データ登録
                        // 大分類マスタ
                        if (categoryMList.Count() > 0)
                        {
                            foreach (CategoryM insModel in categoryMList)
                            {
                                // データ登録
                                context.CategoryMs.Add(insModel);
                            }
                            context.SaveChanges();
                        }

                        // 中分類マスタ
                        if (locationMList.Count() > 0)
                        {
                            foreach (LocationM insModel in locationMList)
                            {
                                // データ登録
                                context.LocationMs.Add(insModel);
                            }
                            context.SaveChanges();
                        }

                        // 帳票テンプレートマスタ
                        if (reportTempList.Count() > 0)
                        {
                            foreach (ReportTemplateM insModel in reportTempList)
                            {
                                // データ登録
                                context.ReportTemplateMs.Add(insModel);
                            }
                        }

                        // 帳票マスタ
                        if (reportMList.Count() > 0)
                        {
                            foreach (ReportM insModel in reportMList)
                            {
                                // データ登録
                                context.ReportMs.Add(insModel);
                            }
                            context.SaveChanges();
                        }

                        // 設問マスタ
                        if (questionMList.Count() > 0)
                        {
                            foreach (QuestionM insModel in questionMList)
                            {
                                // データ登録
                                context.QuestionMs.Add(insModel);
                            }
                            context.SaveChanges();
                        }

                        // 作業者マスタ
                        context.WorkerMs.Add(workerMDt);
                        context.SaveChanges();

                        // 店舗別回答種類マスタ（北川追加）
                        if (anstypeMList.Count() > 0)
                        {
                            foreach (Shop_AnswerTypeM insModel in anstypeMList)
                            {
                                // データ登録
                                context.Shop_AnswerTypeMs.Add(insModel);
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
                        ModelState.AddModelError(string.Empty, MsgConst.FIRST_LOGIN_INSERT_FAILER);
                        return View("Show", bInitialData);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.FIRST_LOGIN_INSERT_FAILER);
                            return View("Show", bInitialData);
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

            return RedirectToAction("Show", "WorkerM");
        }

        /// <summary>
        /// マスタデータ登録（個別登録）
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult IndividualEdit()
        {
            //店舗ID
            string shopId = (string)Session["SHOPID"];
            // フォーマット系列店舗ID
            string formatShopId = (string)Session["FORMATSHOPID"];

            // 作業者マスタデータ
            var workerMDt = this.SetWorkerMs(shopId);

            var masterF = new MasterFunction();
            // 画面情報
            var bInitialData = new BInitialData
            {
                // 大分類マスタ
                CategoryMDatas = masterF.GetCategoryMData(context, formatShopId),
                // 中分類マスタ
                LocationMDatas = masterF.GetLocationMData(context, formatShopId),
                // 設問マスタ
                QuestionMDatas = this.GetQuestionM(context, formatShopId)
            };

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // データ登録

                        // 作業者マスタ
                        context.WorkerMs.Add(workerMDt);
                        context.SaveChanges();
                        // コミット
                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // ロールバック
                        tran.Rollback();
                        // 排他エラー
                        ModelState.AddModelError(string.Empty, MsgConst.FIRST_LOGIN_INSERT_FAILER);
                        return View("Show", bInitialData);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.FIRST_LOGIN_INSERT_FAILER);
                            return View("Show", bInitialData);
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

            // 大分類マスタへ遷移
            return RedirectToAction("InitialSetCategory", "CategoryM");
        }

        /// <summary>
        /// 部門マスタ登録データ設定
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="basicShopId">フォーマット系列店舗ID</param>
        /// <returns>部門マスタデータリスト</returns>
        private List<CategoryM> SetCategoryMs(string shopId, string basicShopId)
        {
            // 部門マスタ取得
            var categoryM = from ca in context.CategoryMs
                            where ca.SHOPID == basicShopId
                            select ca;

            var categoryMList = new List<CategoryM>();

            if (categoryM.Count() > 0)
            {
                foreach (CategoryM ca in categoryM)
                {
                    // 店舗ID
                    ca.SHOPID = shopId;
                    ca.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    ca.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    categoryMList.Add(ca);
                }
            }

            return categoryMList;
        }

        /// <summary>
        /// 場所マスタ登録データ設定
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="basicShopId">フォーマット系列店舗ID</param>
        /// <returns>場所マスタデータリスト</returns>
        private List<LocationM> SetLocationMs(string shopId, string basicShopId)
        {
            // 場所マスタ取得
            var locationM = from lo in context.LocationMs
                            where lo.SHOPID == basicShopId
                            select lo;

            var locationMList = new List<LocationM>();
            if (locationM.Count() > 0)
            {
                foreach (LocationM lo in locationM)
                {
                    // 店舗ID
                    lo.SHOPID = shopId;
                    lo.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    lo.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    locationMList.Add(lo);
                }
            }

            return locationMList;
        }

        private List<ReportTemplateM> SetReportTemplateM(string shopId, string basicShopId)
        {
            // 帳票テンプレートマスタ
            var reportTemplateM = from re in context.ReportTemplateMs
                                  where re.SHOPID == basicShopId
                                  select re;

            var reportTMList = new List<ReportTemplateM>();

            if (reportTemplateM.Count() > 0)
            {
                foreach (ReportTemplateM re in reportTemplateM)
                {
                    // 店舗ID
                    re.SHOPID = shopId;
                    re.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    re.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    reportTMList.Add(re);
                }
            }

            return reportTMList;
        }

        /// <summary>
        /// 帳票マスタ登録データ設定
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="basicShopId">フォーマット系列店舗ID</param>
        /// <returns>帳票マスタデータリスト</returns>
        private List<ReportM> SetReportMs(string shopId, string basicShopId)
        {
            // 帳票マスタ
            var reportM = from re in context.ReportMs
                          where re.SHOPID == basicShopId
                          select re;

            var reportMList = new List<ReportM>();

            if (reportM.Count() > 0)
            {
                foreach (ReportM re in reportM)
                {
                    // 店舗ID
                    re.SHOPID = shopId;
                    re.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    re.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    reportMList.Add(re);
                }
            }

            return reportMList;
        }

        /// <summary>
        /// 設問マスタ登録データ設定
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="basicShopId">フォーマット系列店舗ID</param>
        /// <returns>設問マスタデータリスト</returns>
        private List<QuestionM> SetQuestionMs(string shopId, string basicShopId)
        {
            // 設問マスタ
            var questionM = from qa in context.QuestionMs
                            where qa.SHOPID == basicShopId
                              && qa.DELETEFLAG == DeleteFlg.NODELETE
                            select qa;

            var reportMList = new List<QuestionM>();

            if (questionM.Count() > 0)
            {
                foreach (QuestionM qa in questionM)
                {
                    // 店舗ID
                    qa.SHOPID = shopId;
                    qa.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    qa.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    reportMList.Add(qa);
                }
            }

            return reportMList;
        }

        /// <summary>
        /// 作業者マスタ登録データ設定
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="basicShopId">フォーマット系列店舗ID</param>
        /// <returns>作業者マスタデータリスト</returns>
        private WorkerM SetWorkerMs(string shopId)
        {
            // データ取得
            var initWorkerDt = from a in context.WorkerMs
                               orderby a.DISPLAYNO
                               where a.SHOPID == WorkerInitData.SHOPID
                                  && a.WORKERID == WorkerInitData.WORKERID
                               select a;

            if (initWorkerDt.Count() == 0)
            {
                // 作業者マスタ
                return new WorkerM
                {
                    SHOPID = shopId,                        // 店舗ID
                    WORKERID = FIRSTLOGIN.WORKER_ID,        // 作業者ID
                    WORKERNAME = FIRSTLOGIN.WORKER_NAME,    // 作業者名
                    NODISPLAYKBN = BoolKbn.KBN_FALSE,       // 表示対象外区分
                    DISPLAYNO = 0,                          // 表示No
                    INSUSERID = USERNAME.FIRSTLOGIN_UPDID,  // 登録ユーザーID
                    UPDUSERID = USERNAME.FIRSTLOGIN_UPDID   // 更新ユーザーID
                };
            }

            var worker = initWorkerDt.FirstOrDefault();
            worker.SHOPID = shopId;                         // 店舗ID
            worker.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;   // 登録ユーザーID
            worker.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;   // 更新ユーザーID

            return worker;
        }

        /// <summary>
        /// 店舗別回答種類マスタ登録データ設定(北川追加）
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="basicShopId">フォーマット系列店舗ID</param>
        /// <returns>設問マスタデータリスト</returns>
        private List<Shop_AnswerTypeM> SetShop_AnswerTypeMs(string shopId, string basicShopId)
        {
            // 設問マスタ
            var anstypeM = from qa in context.Shop_AnswerTypeMs
                            where qa.SHOPID == basicShopId
                            select qa;

            var shopAnsTypeList = new List<Shop_AnswerTypeM>();

            if (anstypeM.Count() > 0)
            {
                foreach (Shop_AnswerTypeM qa in anstypeM)
                {
                    // 店舗ID
                    qa.SHOPID = shopId;
                    qa.INSUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    qa.UPDUSERID = USERNAME.FIRSTLOGIN_UPDID;
                    shopAnsTypeList.Add(qa);
                }
            }

            return shopAnsTypeList;
        }

        /// <summary>
        /// 業務設定画面へ戻る
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult BackIndustry()
        {
            // フォーマット系列店舗IDを削除
            Session.Remove("FORMATSHOPID");
            // 業種設定画面へ遷移
            return RedirectToAction("Show", "IndustrySelection");
        }

        /// <summary>
        /// 設問マスタデータ取得
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="shopId">フォーマット店舗ID</param>
        /// <returns>設問マスタデータ</returns>
        private List<QuestionMData> GetQuestionM(MasterContext context, string shopId)
        {
            // 設問マスタ
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("CA.CATEGORYNAME ");
            sql.Append(", LO.LOCATIONNAME ");
            sql.Append(", RE.REPORTNAME ");
            sql.Append(", QA.QUESTIONID ");
            sql.Append(", QA.QUESTION ");
            sql.Append(", ANS.ANSWERTYPENAME ");
            sql.Append(", CASE WHEN QA.NORMALCONDITION = '0' THEN '指定なし' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '1' THEN '次の値の間' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '2' THEN '次の値の間以外' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '3' THEN '次の値に等しい' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '4' THEN '次の値に等しくない' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '5' THEN '次の値より大きい' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '6' THEN '次の値より小さい' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '7' THEN '次の値以上' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '8' THEN '次の値以下' ");
            sql.Append(" WHEN QA.NORMALCONDITION = '9' THEN '上限下限温度の間' ");
            sql.Append(" ELSE '指定なし' ");
            sql.Append(" END AS NORMALCONDITION_NAME ");
            sql.Append(", QA.NORMALCONDITION1 ");
            sql.Append(", QA.NORMALCONDITION2 ");
            sql.Append("FROM ");
            sql.Append("QUESTION_M QA ");
            sql.Append("LEFT JOIN REPORT_M RE ON QA.SHOPID = RE.SHOPID AND QA.REPORTID = RE.REPORTID ");
            sql.Append("LEFT JOIN CATEGORY_M CA ON QA.SHOPID = CA.SHOPID AND QA.CATEGORYID = CA.CATEGORYID ");
            sql.Append("LEFT JOIN LOCATION_M LO ON QA.SHOPID = LO.SHOPID AND QA.LOCATIONID = LO.LOCATIONID ");
            sql.Append("LEFT JOIN ANSWERTYPE_M ANS ON QA.ANSWERTYPEID = ANS.ANSWERTYPEID ");
            sql.Append("WHERE ");
            sql.Append("QA.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND QA.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("ORDER BY RE.REPORTNAME, CA.CATEGORYNAME, LO.LOCATIONNAME, QA.DISPLAYNO ");
            var QuestionList = context.Database.SqlQuery<QuestionMData>(sql.ToString());

            StringBuilder sql2 = new StringBuilder();
            sql2.Append("SELECT ");
            sql2.Append("CA.CATEGORYNAME ");
            sql2.Append(", LO.LOCATIONNAME ");
            sql2.Append(", RE.REPORTNAME ");
            sql2.Append(", QA.QUESTIONID ");
            sql2.Append(", QA.QUESTION ");
            sql2.Append(", SANS.ANSWERTYPENAME ");
            sql2.Append(", CASE WHEN QA.NORMALCONDITION = '0' THEN '指定なし' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '1' THEN '次の値の間' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '2' THEN '次の値の間以外' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '3' THEN '次の値に等しい' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '4' THEN '次の値に等しくない' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '5' THEN '次の値より大きい' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '6' THEN '次の値より小さい' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '7' THEN '次の値以上' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '8' THEN '次の値以下' ");
            sql2.Append(" WHEN QA.NORMALCONDITION = '9' THEN '上限下限温度の間' ");
            sql2.Append(" ELSE '指定なし' ");
            sql2.Append(" END AS NORMALCONDITION_NAME ");
            sql2.Append(", QA.NORMALCONDITION1 ");
            sql2.Append(", QA.NORMALCONDITION2 ");
            sql2.Append("FROM ");
            sql2.Append("QUESTION_M QA ");
            sql2.Append("LEFT JOIN REPORT_M RE ON QA.SHOPID = RE.SHOPID AND QA.REPORTID = RE.REPORTID ");
            sql2.Append("LEFT JOIN CATEGORY_M CA ON QA.SHOPID = CA.SHOPID AND QA.CATEGORYID = CA.CATEGORYID ");
            sql2.Append("LEFT JOIN LOCATION_M LO ON QA.SHOPID = LO.SHOPID AND QA.LOCATIONID = LO.LOCATIONID ");
            sql2.Append("INNER JOIN SHOP_ANSWERTYPE_M SANS ON QA.ANSWERTYPEID = SANS.ANSWERTYPEID AND QA.SHOPID = SANS.SHOPID "); //LEFTJOINだとANSWERTYPENAMEがNULLのデータが出るのでINNERJOINで必要なだけ出す
            sql2.Append("WHERE ");
            sql2.Append("QA.SHOPID = '");
            sql2.Append(shopId);
            sql2.Append("' ");
            sql2.Append("AND QA.DELETEFLAG = '");
            sql2.Append(DeleteFlg.NODELETE);
            sql2.Append("' ");
            sql2.Append("ORDER BY RE.REPORTNAME, CA.CATEGORYNAME, LO.LOCATIONNAME, QA.DISPLAYNO ");
            var QuestionList2 = context.Database.SqlQuery<QuestionMData>(sql2.ToString());

            Dictionary<string, QuestionMData> questionKeyValue = new Dictionary<string, QuestionMData>();

            foreach (QuestionMData questionMData in QuestionList)
            {
                String Key = questionMData.REPORTNAME + questionMData.CATEGORYNAME + questionMData.LOCATIONNAME + questionMData.QUESTIONID;
                questionKeyValue.Add(Key, questionMData);
            }
            foreach (QuestionMData questionMData in QuestionList2)
            {
                String Key = questionMData.REPORTNAME + questionMData.CATEGORYNAME + questionMData.LOCATIONNAME + questionMData.QUESTIONID;
                if (questionKeyValue.Keys.Contains(Key))
                    questionKeyValue[Key] = questionMData;
                else
                    questionKeyValue.Add(Key, questionMData);
            }

            List<QuestionMData> returnList = (from s in questionKeyValue.Values orderby s.REPORTNAME, s.CATEGORYNAME, s.LOCATIONNAME, s.QUESTIONID select s).ToList();


            if (returnList.Count() > 0)
            {
                return returnList.ToList();
            }
            //if (QuestionList.Count() > 0)
            //{
            //    return QuestionList.ToList();
            //}

            return new List<QuestionMData>();
        }

        /// <summary>
        /// 業種マスタからフォーマット店舗IDを取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <returns>フォーマット店舗ID</returns>
        private string GetIndustryShopId(string shopId)
        {
            // 業種のフォーマットデータを設定する場合
            string dataShopId = string.Empty;

            // 店舗マスタ
            var AffiliatedShopM = from s in context.ShopMs
                                  where s.SHOPID == shopId
                                  select s;
            if (AffiliatedShopM.Count() == 0 || AffiliatedShopM.FirstOrDefault() == null)
            {
                throw new ApplicationException();
            }

            // 業種マスタ
            var IndustryM = from indust in context.IndustryMs
                            where indust.INDUSTRYID == AffiliatedShopM.FirstOrDefault().INDUSTRYID
                            select indust;
            if (IndustryM.Count() == 0 || IndustryM.FirstOrDefault() == null)
            {
                throw new ApplicationException();
            }

            dataShopId = IndustryM.FirstOrDefault().INDUSTRYDATAID;


            return dataShopId;
        }

    }
}