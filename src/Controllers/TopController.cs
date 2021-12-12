using HACCPExtender.Business;
using HACCPExtender.Models;
using System.Linq;
using System;
using System.Diagnostics;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using System.Text;
using System.Collections.Generic;
using HACCPExtender.Models.Bussiness;
using HACCPExtender.Controllers.Common;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class TopController : Controller
    {
        private MasterContext context = new MasterContext();
        private readonly MasterFunction masterFunc = new MasterFunction();
        // 共通クラス
        readonly CommonFunction comm = new CommonFunction();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TopController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "Top");
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
        public ActionResult Show()
        {
            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // 現在時刻を取得
            string nowDateTime = DateTime.Now.ToString("yyyyMMddHHmm");
            string nowDateDay = nowDateTime.Substring(0, 8);

            var bTop = new BTop
            {
                // お知らせ情報を取得
                BnotificList = this.GetNotification(nowDateTime),

                // 過去の承認待ちデータ有無を設定
                HistorucalData = this.GetApprovalPstMinutes(shopId, nowDateDay),

                // 承認待ち情報を設定
                ApprovalDataList = this.GetSnnData(shopId, nowDateDay)
            };

            return View(bTop);
        }

        /// <summary>
        /// データ承認遷移
        /// </summary>
        /// <param name="form">画面選択値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Approval(FormCollection form)
        {
            string bunrui = form["sel_Bunrui"];
            bool paramChk = false;

            if (APPROVALLEVEL.MIDDLE.Equals(bunrui)
                && !string.IsNullOrEmpty(form["sel_CategoryId"])
                && !string.IsNullOrEmpty(form["sel_LocationId"])
                && !string.IsNullOrEmpty(form["sel_ReportId"])
                && !string.IsNullOrEmpty(form["sel_Period"])
                && !string.IsNullOrEmpty(form["sel_StartDate"]))
            {
                // 中分類承認
                paramChk = true;
            }
            else if (APPROVALLEVEL.MAJORE.Equals(bunrui)
                && !string.IsNullOrEmpty(form["sel_CategoryId"])
                && !string.IsNullOrEmpty(form["sel_Period"])
                && !string.IsNullOrEmpty(form["sel_StartDate"]))
            {
                // 大分類承認
                paramChk = true;
            }
            else if (APPROVALLEVEL.FACILITY.Equals(bunrui)
                && !string.IsNullOrEmpty(form["sel_Period"])
                && !string.IsNullOrEmpty(form["sel_StartDate"]))
            {
                // 施設承認
                paramChk = true;
            }

            if (!paramChk)
            {
                throw new ApplicationException();
            }

            // セッションにセット
            Session.Add("PENDINGNODEID", form["sel_Bunrui"]);
            Session.Add("PENDINGCATEGORYID", form["sel_CategoryId"]);
            Session.Add("PENDINGLOCATIONID", form["sel_LocationId"]);
            Session.Add("PENDINGREPORTID", form["sel_ReportId"]);
            Session.Add("PENDINGPERIODID", form["sel_Period"]);
            Session.Add("PENDINGSTARTDATE", form["sel_StartDate"]);

            // 管理者ログイン済みの場合
            if (ManagerLoginMode.LOGIN_ALREADY.Equals(Session["DISPMODE"]))
            {
                // データ承認へ遷移
                if (APPROVALLEVEL.MIDDLE.Equals(bunrui))
                {
                    // 管理者ログインへ遷移
                    return RedirectToAction("Show", "MiddleApproval");
                }
                else if (APPROVALLEVEL.MAJORE.Equals(bunrui))
                {
                    // 管理者ログインへ遷移
                    return RedirectToAction("Show", "MajorApproval");
                }
                else if (APPROVALLEVEL.FACILITY.Equals(bunrui))
                {
                    // 管理者ログインへ遷移
                    return RedirectToAction("Show", "FacilityApproval");
                }
            }
            else
            {
                return RedirectToAction("Show", "Manager");
            }

            // トップページへ遷移（不正）
            return RedirectToAction("Show", "Top");
        }

        /// <summary>
        /// データ履歴遷移
        /// </summary>
        /// <param name="form">選択値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DataHistory(FormCollection form)
        {
            // セッションにセット
            Session.Add("PENDINGCATEGORYID", form["sel_CategoryId"]);
            Session.Add("PENDINGLOCATIONID", form["sel_LocationId"]);
            Session.Add("PENDINGREPORTID", form["sel_ReportId"]);
            Session.Add("PENDINGSTARTDATE", DateTime.Now.ToString("yyyy-MM-dd"));

            return RedirectToAction("ToptoDataHistory", "DataHistory");
        }

        /// <summary>
        /// 承認待ち過去データ有無の取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="nowDateDay">処理日（YYYYMMDD）</param>
        /// <returns></returns>
        private bool GetApprovalPstMinutes(string shopId, string nowDateDay)
        {
            // トラン情報取得共通クラス
            var tranfunction = new TransactionFunction(context);

            bool kakoData = false;
            if (tranfunction.GetMiddleApprovalData(shopId, string.Empty, nowDateDay, true).Count() > 0)
            {
                // 中分類承認待ちデータが存在する場合
                kakoData = true;
            }
            else if (tranfunction.GetMajorData(shopId, string.Empty, nowDateDay, true).Count() > 0)
            {
                // 大分類承認待ちデータが存在する場合
                kakoData = true;
            }
            else if (tranfunction.GetFacilityData(shopId, string.Empty, nowDateDay, true).Count() > 0)
            {
                // 施設承認待ちデータが存在する場合
                kakoData = true;
            }

            return kakoData;
        }

        /// <summary>
        /// お知らせ情報取得
        /// </summary>
        /// <param name="nowDateTime">処理日付</param>
        /// <returns>お知らせ情報リスト</returns>
        private List<BNotification> GetNotification(string nowDateTime)
        {
            // メッセージ表示期間を取得（ヶ月）
            int retention = int.Parse(GetAppSet.GetAppSetValue("Notification", "Retention")) * -1;
            string day = nowDateTime.Substring(0, 8);
            string time = nowDateTime.Substring(8, 4);
            // メッセージ表示期間開始日
            string startDay = ((DateTime.Now).AddMonths(retention)).ToString("yyyyMMdd");

            // メッセージ情報取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("NOTICEID ");
            sql.Append(", NOTICECONTENT");
            sql.Append(", TO_CHAR(TO_DATE(STARTDATE,'YYYY/MM/DD'), 'YYYY.MM.DD') AS STARTDATE ");
            sql.Append("FROM ");
            sql.Append("NOTIFICATION_T ");
            sql.Append("WHERE ");
            sql.Append("(STARTDATE < '");
            sql.Append(day);
            sql.Append("' AND STARTDATE >= '");
            sql.Append(startDay);
            sql.Append("') ");
            sql.Append("OR (STARTDATE = '");
            sql.Append(day);
            sql.Append("' AND STARTTIME <= '");
            sql.Append(time);
            sql.Append("') ");
            sql.Append("ORDER BY STARTDATE ASC, STARTTIME ASC ");
            var notifi = context.Database.SqlQuery<BNotification>(sql.ToString());
            if (notifi.Count() > 0)
            {
                return notifi.ToList();
            }

            return new List<BNotification>();
        }

        /// <summary>
        /// 処理日の承認データ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="periodYMD">処理日時</param>
        /// <returns>承認データ</returns>
        private List<BTopApproval> GetSnnData(string shopId, string periodYMD)
        {
            // 中分類承認データすべて（日付指定）
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MID.SHOPID ");
            sql.Append(",MID.CATEGORYID ");
            sql.Append(",MID.LOCATIONID ");
            sql.Append(",MID.REPORTID ");
            sql.Append(",MID.PERIOD ");
            sql.Append(",MID.PERIODSTART ");
            sql.Append(",MID.PERIODEND ");
            sql.Append(",MID.MIDDLEGROUPNO ");
            sql.Append(",COUNT(REPORTID) AS CNT ");
            sql.Append("FROM ");
            sql.Append("MIDDLEAPPROVAL_T MID ");
            sql.Append("WHERE ");
            sql.Append("MID.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND MID.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("AND MID.PERIODSTART <= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("AND MID.PERIODEND >= '");
            sql.Append(periodYMD);
            sql.Append("' ");
            sql.Append("GROUP BY MID.SHOPID, MID.CATEGORYID, MID.LOCATIONID, MID.REPORTID, MID.PERIOD, MID.PERIODSTART, MID.PERIODEND, MID.MIDDLEGROUPNO ");
            sql.Append("ORDER BY MID.CATEGORYID, MID.PERIOD, MID.PERIODSTART ");
            sql.Append("FOR READ ONLY ");
            var middlebase = context.Database.SqlQuery<MiddleData>(sql.ToString());
            if (middlebase == null || middlebase.Count() == 0)
            {
                return new List<BTopApproval>();
            }

            // 承認データ
            var approvalData = new List<BTopApproval>();
            var bTopApproval = new BTopApproval();
            var middleDataList = new List<MiddleData>();
            // 大分類ID
            string addCategoryId = string.Empty;
            // 大分類マスタ
            var categoryDt = masterFunc.GetCategoryMData(context, shopId);

            // 帳票単位で承認ノードを取得する
            foreach (MiddleData middle in middlebase)
            {
                // 大分類IDが変更になる場合
                if (!addCategoryId.Equals(middle.CATEGORYID))
                {
                    if (middleDataList.Count() > 0)
                    {
                        // Entityに格納
                        bTopApproval.CategoryId = addCategoryId;
                        bTopApproval.CategoryName = categoryDt.Where(a => a.CATEGORYID == addCategoryId).FirstOrDefault().CATEGORYNAME;
                        bTopApproval.BApprovalList = middleDataList;
                        approvalData.Add(bTopApproval);

                        // 初期化
                        bTopApproval = new BTopApproval();
                        middleDataList = new List<MiddleData>();
                    }

                    // 大分類ID
                    addCategoryId = middle.CATEGORYID;
                }

                // 中分類承認の承認待ちデータが存在するかを確認する
                var middleDt = from mid in context.MiddleApprovalTs
                               where mid.SHOPID == shopId
                                    && mid.CATEGORYID == middle.CATEGORYID
                                    && mid.LOCATIONID == middle.LOCATIONID
                                    && mid.REPORTID == middle.REPORTID
                                    && mid.PERIOD == middle.PERIOD
                                    && mid.PERIODSTART == middle.PERIODSTART
                               select mid;
                // データが存在しない場合（通常あり得ない）
                if (middleDt == null || middleDt.Count() == 0)
                {
                    continue;
                }

                var topMiddleData = middle.Clone();

                // 承認待ちデータが存在する場合
                if (middleDt.Where(a => a.STATUS == ApprovalStatus.PENDING).Count() > 0)
                {
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.MIDDLE));
                    continue;
                }

                // 大分類承認依頼未完了の場合
                sql = new StringBuilder();
                sql.Append("SELECT ");
                sql.Append("MID.SHOPID ");
                sql.Append(",MID.CATEGORYID ");
                sql.Append(",MID.LOCATIONID ");
                sql.Append(",MID.REPORTID ");
                sql.Append(",MID.PERIOD ");
                sql.Append(",MID.PERIODSTART ");
                sql.Append("FROM ");
                sql.Append("MIDDLEAPPROVAL_T MID ");
                sql.Append("WHERE ");
                sql.Append("MID.SHOPID = '");
                sql.Append(shopId);
                sql.Append("' ");
                sql.Append("AND MID.CATEGORYID = '");
                sql.Append(topMiddleData.CATEGORYID);
                sql.Append("' ");
                sql.Append("AND MID.LOCATIONID = '");
                sql.Append(topMiddleData.LOCATIONID);
                sql.Append("' ");
                sql.Append("AND MID.REPORTID = '");
                sql.Append(topMiddleData.REPORTID);
                sql.Append("' ");
                sql.Append("AND MID.PERIOD = '");
                sql.Append(topMiddleData.PERIOD);
                sql.Append("' ");
                sql.Append("AND MID.PERIODSTART = '");
                sql.Append(topMiddleData.PERIODSTART);
                sql.Append("' ");
                sql.Append("AND MID.DELETEFLAG = '");
                sql.Append(DeleteFlg.NODELETE);
                sql.Append("' ");
                sql.Append("GROUP BY MID.SHOPID, MID.CATEGORYID, MID.LOCATIONID, MID.REPORTID, MID.PERIOD, MID.PERIODSTART ");
                sql.Append("HAVING SUM(CASE WHEN (MID.STATUS = '");
                sql.Append(ApprovalStatus.PENDING);
                sql.Append("') THEN 1 ELSE 0 END) = 0 ");
                sql.Append("AND SUM(CASE WHEN MID.MIDDLEGROUPNO = 0 THEN 0 ELSE 1 END) = 0 ");
                sql.Append("FOR READ ONLY ");
                var middle_request = context.Database.SqlQuery<MiddleData>(sql.ToString());
                if (middle_request.Count() > 0)
                {
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.MIDDLE));
                    continue;
                }

                // 大分類データ取得（1件）
                sql = new StringBuilder();
                sql.Append("SELECT ");
                sql.Append("MAJ.STATUS ");
                sql.Append(", MAJ.MAJORGROUPNO ");
                sql.Append("FROM ");
                sql.Append("MAJORAPPROVAL_T MAJ  ");
                sql.Append("WHERE ");
                sql.Append("MAJ.SHOPID = '");
                sql.Append(shopId);
                sql.Append("' ");
                sql.Append("AND MAJ.CATEGORYID = '");
                sql.Append(topMiddleData.CATEGORYID);
                sql.Append("' ");
                sql.Append("AND MAJ.LOCATIONID = '");
                sql.Append(topMiddleData.LOCATIONID);
                sql.Append("' ");
                sql.Append("AND MAJ.REPORTID = '");
                sql.Append(topMiddleData.REPORTID);
                sql.Append("' ");
                sql.Append("AND MAJ.PERIOD = '");
                sql.Append(topMiddleData.PERIOD);
                sql.Append("' ");
                sql.Append("AND MAJ.PERIODSTART = '");
                sql.Append(topMiddleData.PERIODSTART);
                sql.Append("' ");
                sql.Append("AND MAJ.DELETEFLAG = '");
                sql.Append(DeleteFlg.NODELETE);
                sql.Append("' ");
                sql.Append("FOR READ ONLY ");
                var major_data = context.Database.SqlQuery<MajorData>(sql.ToString());

                if (major_data == null || major_data.Count() == 0)
                {
                    // 中分類承認処理中（通常この状態はない）
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.MIDDLE));
                    continue;
                }

                // 大分類承認データ
                var majorDt = major_data.FirstOrDefault();
                // ステータス=差戻の場合
                if (ApprovalStatus.REMAND.Equals(majorDt.STATUS))
                {
                    // 中分類承認処理中
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.MIDDLE));
                    continue;
                }
                else if (ApprovalStatus.PENDING.Equals(majorDt.STATUS))
                {
                    // ステータス=承認待ちの場合 大分類承認処理中
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.MAJORE));
                    continue;
                }
                // ステータス=承認済み かつ、大分類承認グループ連番が0の場合
                if (majorDt.MAJORGROUPNO == 0)
                {
                    // 施設承認依頼未完了で大分類承認処理中
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.MAJORE));
                    continue;
                }

                // 施設承認データ取得（1件）
                sql = new StringBuilder();
                sql.Append("SELECT ");
                sql.Append("FACI.STATUS ");
                sql.Append(",FACI.FACILITYAPPGROUPNO ");
                sql.Append("FROM ");
                sql.Append("FACILITYAPPROVAL_T FACI ");
                sql.Append("WHERE ");
                sql.Append("FACI.SHOPID = '");
                sql.Append(shopId);
                sql.Append("' ");
                sql.Append("AND FACI.CATEGORYID = '");
                sql.Append(topMiddleData.CATEGORYID);
                sql.Append("' ");
                sql.Append("AND FACI.PERIOD = '");
                sql.Append(topMiddleData.PERIOD);
                sql.Append("' ");
                sql.Append("AND FACI.PERIODSTART = '");
                sql.Append(topMiddleData.PERIODSTART);
                sql.Append("' ");
                sql.Append("AND FACI.DELETEFLAG = '");
                sql.Append(DeleteFlg.NODELETE);
                sql.Append("' ");
                sql.Append("AND FACI.PERIODSTART = '");
                sql.Append(topMiddleData.PERIODSTART);
                sql.Append("' ");
                var facility_data = context.Database.SqlQuery<FacilityData>(sql.ToString());

                if (facility_data == null || facility_data.Count() == 0)
                {
                    // 大分類承認処理中（通常この状態はない）
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.MAJORE));
                    continue;
                }

                // 施設承認データ
                var ficilityDt = facility_data.FirstOrDefault();
                // ステータス=差戻の場合
                if (ApprovalStatus.REMAND.Equals(ficilityDt.STATUS))
                {
                    // 大分類承認処理中
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.MAJORE));
                    continue;
                }
                else if (ApprovalStatus.PENDING.Equals(ficilityDt.STATUS))
                {
                    // ステータス=承認待ちの場合 施設承認処理中
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.FACILITY));
                    continue;
                }
                // ステータス=承認済み かつ、施設承認グループ連番が0の場合
                if (ficilityDt.FACILITYAPPGROUPNO == 0)
                {
                    // 承認未完了で施設承認処理中
                    middleDataList.Add(
                        this.SetApprovalData(topMiddleData, shopId, APPROVALLEVEL.FACILITY));
                    continue;
                }

                // 上記すべての条件に一致しない場合は承認完了しているため、表示対象となる。
            }

            // 最終ループ分のデータ格納
            if (middleDataList.Count() > 0)
            {
                // Entityに格納
                bTopApproval.CategoryId = addCategoryId;
                bTopApproval.CategoryName = categoryDt.Where(a => a.CATEGORYID == addCategoryId).FirstOrDefault().CATEGORYNAME;
                bTopApproval.BApprovalList = middleDataList;
                approvalData.Add(bTopApproval);
            }

            return approvalData;
        }

        /// <summary>
        /// 承認データ名称設定
        /// </summary>
        /// <param name="data">中分類承認データ</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="approvalNode">承認ノード</param>
        /// <returns>中分類データ</returns>
        private MiddleData SetApprovalData(MiddleData data, string shopId, string approvalNode)
        {
            // 中分類マスタ
            var locationDt = masterFunc.GetLocationMData(context, shopId);
            // 帳票マスタ
            var reportDt = masterFunc.GetReportMData(context, shopId);

            // 承認ノード
            data.APPROVALNODE = approvalNode;
            data.APPROVALNODEDISP = comm.GetApprovalNodeDisp(approvalNode);
            // 中分類名
            if (locationDt != null && locationDt.Count() > 0)
            {
                var category = locationDt.Where(a => a.LOCATIONID == data.LOCATIONID).FirstOrDefault();
                if (category != null)
                {
                    data.LOCATIONNAME = category.LOCATIONNAME;
                }
            }
            // 帳票名
            if (reportDt != null && reportDt.Count() > 0)
            {
                var report = reportDt
                    .Where(a => a.CATEGORYID == data.CATEGORYID && a.LOCATIONID == data.LOCATIONID && a.REPORTID == data.REPORTID)
                    .FirstOrDefault();
                if (report != null)
                {
                    data.REPORTNAME = report.REPORTNAME;
                }
            }
            // 周期名
            data.PERIODWORD = comm.GetPeriodDisp(data.PERIOD);

            return data;
        }

    }
}