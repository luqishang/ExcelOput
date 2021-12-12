using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.ExcelOutput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;
using HACCPExtender.Models.Bussiness;
using HACCPExtender.Business;

namespace HACCPExtenfer.Controllers
{
    public class DataHistoryController : Controller
    {
        // コンテキスト
        private readonly MasterContext context = new MasterContext();
        // 共通処理
        readonly CommonFunction comm = new CommonFunction();
        readonly MasterFunction masterFunc = new MasterFunction();
        // 設問マスタ登録最大数
        private static readonly int QUESTION_NUM_MAX = int.Parse(GetAppSet.GetAppSetValue("Question", "Max"));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DataHistoryController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "DataHistory");
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
        public ActionResult Show(string sel1, string sel2, string sel3, string sel4)
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
            // 指定日(初期値：当日)
            var now = DateTime.Now;
            string periodDay = now.ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(sel4))
            {
                periodDay = sel4;
            }

            // ドロップダウンリストデータ設定
            SetDropDownList(categoryId, locationId);

            // 画面情報
            var dataHistoryVal = GetDataHistoryData(categoryId, locationId, reportId, periodDay);

            return View(dataHistoryVal);
        }

        /// <summary>
        /// 初期表示(タブレット用）
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpGet]
        public ActionResult LocalShow(string sel1, string sel2, string sel3, string sel4)
        {
            // 大分類
            string categoryId = (string)Session["CATEGORYID"]; ;
            if (!string.IsNullOrEmpty(sel1))
            {
                categoryId = sel1;
            }
            // 中分類
            string locationId = (string)Session["LOCATIONID"]; ;
            if (!string.IsNullOrEmpty(sel2))
            {
                locationId = sel2;
            }
            // 帳票
            string reportId = (string)Session["REPORTID"]; ;
            if (!string.IsNullOrEmpty(sel3))
            {
                reportId = sel3;
            }

            Session.Remove("CATEGORYID");
            Session.Remove("LOCATIONID");
            Session.Remove("REPORTID");

            // 指定日(初期値：当日)
            var now = DateTime.Now;
            string periodDay = now.ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(sel4))
            {
                periodDay = sel4;
            }

            // ドロップダウンリストデータ設定
            SetDropDownList(categoryId, locationId);

            // 画面情報
            var dataHistoryVal = GetDataHistoryData(categoryId, locationId, reportId, periodDay);

            return View(dataHistoryVal);
        }

        /// <summary>
        /// 条件変更時処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <param name="conditionList">条件リスト(大分類・中分類・帳票・指定日)</param>
        /// <returns>ViewResultオブジェクト</returns>
        public ActionResult ChangeCondition(List<string> conditionList)
        {
            // post時の情報をクリア
            ModelState.Clear();

            if (conditionList == null)
            {
                return RedirectToAction("Show");
            }

            // 大分類
            string categoryId = conditionList[0];
            // 中分類
            string locationId = conditionList[1];
            // 帳票
            string reportId = conditionList[2];
            // 指定日
            string periodDay = conditionList[3];

            // ドロップダウンリストデータ設定
            SetDropDownList(categoryId, locationId);

            // 画面情報
            var dataHistoryVal = GetDataHistoryData(categoryId, locationId, reportId, periodDay);

            return View("Show", dataHistoryVal);
        }

        /// <summary>
        /// 履歴データをダウンロードする
        /// </summary>
        /// <param name="conditionList"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Download(List<string> conditionList)
        {
            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            conditionList.Add(shopId);

            ExcelDataHistory history = new ExcelDataHistory();
            List<string> retList = history.OutToExcelFile(conditionList);
            string fileName = retList[0];
            string fileDownloadName = retList[1];

            var contentType = "application/xlsx";
            return File(fileName, contentType, Server.UrlEncode(fileDownloadName));
        }

        /// <summary>
        /// トップページから遷移
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpGet]
        public ActionResult ToptoDataHistory()
        {
            // 大分類ID
            string categoryId = (string)Session["PENDINGCATEGORYID"];
            // 中分類ID
            string locationId = (string)Session["PENDINGLOCATIONID"];
            // 帳票ID
            string reportId = (string)Session["PENDINGREPORTID"];
            // 周期指定日
            string periodDay = (string)Session["PENDINGSTARTDATE"];

            // セッションから削除
            Session.Remove("PENDINGCATEGORYID");
            Session.Remove("PENDINGLOCATIONID");
            Session.Remove("PENDINGREPORTID");
            Session.Remove("PENDINGSTARTDATE");

            // ドロップダウンリストデータ設定
            SetDropDownList(categoryId, locationId);

            // 画面情報
            var dataHistoryVal = GetDataHistoryData(categoryId, locationId, reportId, periodDay);

            return View("Show", dataHistoryVal);
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
            ViewBag.categoryMSelectListItem = comm.CreateCategoryMOptionList(categoryMList);
            // 中分類ドロップダウンリスト
            List<LocationM> locationMList = masterFunc.GetLocationMData(context, shopId, categoryId);
            if (locationMList.Count() == 0)
            {
                // 中分類データが存在しない場合メッセージを表示
                ModelState.AddModelError(string.Empty, MsgConst.NODATA_LOCATION);
                ViewBag.editMode = "disabled";
            }
            ViewBag.locationMSelectListItem = comm.CreateLocationMOptionList(locationMList);
            // 帳票ドロップダウンリスト
            List<ReportM> reportMList = this.GetReportMData(shopId, categoryId, locationId);
            if (!string.IsNullOrEmpty(categoryId) && !string.IsNullOrEmpty(locationId) && reportMList.Count() == 0)
            {
                // 帳票データが存在しない場合メッセージを表示
                ModelState.AddModelError(string.Empty, "帳票データが存在しません。帳票マスタから登録してください。");
                ViewBag.editMode = "disabled";
            }
            ViewBag.reportMSelectListItem = this.CreateReportMOptionList(reportMList);
        }

        /// <summary>
        /// データ履歴データ取得
        /// </summary>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodDay">周期指定日(YYYY-MM-DD)</param>
        /// <returns>データ履歴データ</returns>
        private BDataHistory GetDataHistoryData(string categoryId, string locationId, string reportId, string periodDay)
        {
            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // 周期指定日（YYYYMMDD）
            string periodYMD = string.Empty;

            // 条件リスト
            List<string> listCondition = new List<string>()
            {
                {string.Empty},
                {string.Empty},
                {string.Empty},
                {string.Empty},
            };

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
            if (!string.IsNullOrEmpty(periodDay))
            {
                listCondition[3] = periodDay;
                periodYMD = periodDay.Replace("-", "");
            }

            // 周期開始日（YYYYMMDD）
            string periodStart = string.Empty;
            // 周期開始日（YYYY/MM/DD）
            string periodStartDate = string.Empty;
            // 周期終了日（YYYY/MM/DD）
            string periodEndDate = string.Empty;
            // 周期
            string periodId = string.Empty;
            if (!string.IsNullOrEmpty(shopId)
                && !string.IsNullOrEmpty(categoryId)
                && !string.IsNullOrEmpty(locationId)
                && !string.IsNullOrEmpty(reportId)
                && !string.IsNullOrEmpty(periodYMD))
            {
                var temeratureDt = from tem in context.TemperatureControlTs
                                   where tem.SHOPID == shopId
                                       && tem.CATEGORYID == categoryId
                                       && tem.LOCATIONID == locationId
                                       && tem.REPORTID == reportId
                                       && tem.PERIODSTART.CompareTo(periodYMD) <= 0
                                       && tem.PERIODEND.CompareTo(periodYMD) >= 0
                                   select tem;

                if (temeratureDt.Count() > 0)
                {
                    TemperatureControlT temeratureItem = temeratureDt.First();
                    periodId = temeratureItem.PERIOD;
                    periodStart = temeratureItem.PERIODSTART;
                    periodStartDate = comm.FormatDateStr(temeratureItem.PERIODSTART);
                    periodEndDate = comm.FormatDateStr(temeratureItem.PERIODEND);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "表示対象データが存在しません。");
                }
            }

            // 温度衛生管理情報データ
            List<List<BTemperatureControl>> temperatureControlDatas = new List<List<BTemperatureControl>>();
            // 承認履歴
            List<ApprovalHistory> historys = new List<ApprovalHistory>();
            if (!string.IsNullOrEmpty(shopId)
                && !string.IsNullOrEmpty(categoryId)
                && !string.IsNullOrEmpty(locationId)
                && !string.IsNullOrEmpty(reportId)
                && !string.IsNullOrEmpty(periodYMD)
                && !string.IsNullOrEmpty(periodId))
            {
                temperatureControlDatas = this.GetTemperatureControlData(shopId, categoryId, locationId, reportId, periodStart);
                historys = this.GetHistory(shopId, categoryId, locationId, reportId, periodId, periodYMD);
            }

            // 画面用データ履歴データ
            var bDataHistory = new BDataHistory
            {
                // 周期開始日（YYYY/MM/DD）
                PeriodStartDate = periodStartDate,
                // 周期終了日（YYYY/MM/DD）
                PeriodEndDate = periodEndDate,
                // 条件リスト
                BConditionList = listCondition,
                // 温度衛生管理情報データ
                TemperatureControlDatas = temperatureControlDatas,
                // 承認履歴
                Historys = historys,
            };

            return bDataHistory;
        }

        /// <summary>
        /// 温度衛生管理情報取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <returns></returns>
        private List<List<BTemperatureControl>> GetTemperatureControlData(
            string shopId, string categoryId, string locationId, string reportId, string periodStart)
        {
            // 温度衛生管理情報データ
            List<List<BTemperatureControl>> TemperatureControlDatas = new List<List<BTemperatureControl>>();

            // 画面用データリスト
            var bTemperatureControl = new List<BTemperatureControl>();

            // 温度衛生管理を取得
            var temeratureDt = from tem in context.TemperatureControlTs
                               orderby tem.DATAYMD
                               where tem.SHOPID == shopId
                                   && tem.CATEGORYID == categoryId
                                   && tem.LOCATIONID == locationId
                                   && tem.REPORTID == reportId
                                   && tem.PERIODSTART == periodStart
                               select tem;

            if (temeratureDt.Count() > 0)
            {
                foreach (TemperatureControlT temperature in temeratureDt)
                {
                    var headerDic = new Dictionary<int, string>();
                    var anserDic = new Dictionary<int, string>();
                    var attachDic = new Dictionary<int, bool>();

                    var BTemp = new BTemperatureControl
                    {
                        WorkerId = temperature.WORKERID,
                        WorkerName = temperature.WORKERNAME,
                        DataYMD = comm.GetDataRecording(temperature.DATAYMD)
                    };

                    for (int i = 0; i < QUESTION_NUM_MAX; i++)
                    {
                        var property = typeof(TemperatureControlT).GetProperty(string.Format("QUESTION{0:D2}", (i + 1).ToString()));
                        var question = (string)property.GetValue(temperature);
                        // 設問がある場合
                        if (!string.IsNullOrEmpty(question))
                        {
                            // 設問をセット
                            string qaStr = string.Empty;
                            Encoding e = Encoding.GetEncoding("Shift_JIS");
                            if (e.GetByteCount(question) > 65)
                            {
                                // 設問文章が長い場合が途中までを表示する
                                qaStr = new string(question.TakeWhile((c, r) => e.GetByteCount(question.Substring(0, r + 1)) <= 65).ToArray()) + "・・・";
                            }
                            else
                            {
                                qaStr = question;
                            }
                            headerDic.Add(i + 1, qaStr);

                            // 添付ファイルの値
                            property = typeof(TemperatureControlT).GetProperty(string.Format("RESULTATTACHMENT{0:D2}", (i + 1).ToString()));
                            var attachment = (string)property.GetValue(temperature);

                            if (!string.IsNullOrEmpty(attachment))
                            {
                                // 回答に添付パスを設定
                                anserDic.Add(i + 1, attachment);
                                // 添付フラグ=true
                                attachDic.Add(i + 1, true);
                            }
                            else
                            {
                                property = typeof(TemperatureControlT).GetProperty(string.Format("RESULT{0:D2}", (i + 1).ToString()));
                                var result = (string)property.GetValue(temperature);
                                // 回答を設定
                                anserDic.Add(i + 1, result);
                                // 添付フラグ=false
                                attachDic.Add(i + 1, false);
                            }
                        }
                        else
                        {
                            // 設問がない場合
                            break;
                        }
                    }

                    BTemp.HeaderDic = headerDic;
                    BTemp.AnserDic = anserDic;
                    BTemp.AttachDic = attachDic;
                    bTemperatureControl.Add(BTemp);
                }

                // ヘッダーDictionaryの内容で設問文章の差異を判定し、出力するテーブルを変える
                var bTemperatureControlList = new List<List<BTemperatureControl>>();
                var BList = new List<BTemperatureControl>();
                int cnt = 0;
                var baseHeader = new Dictionary<int, string>();
                foreach (BTemperatureControl control in bTemperatureControl)
                {
                    if (cnt == 0)
                    {
                        baseHeader = control.HeaderDic;
                        BList.Add(control);
                        cnt++;
                        continue;
                    }

                    bool differ = true;

                    // 同じインスタンスの場合
                    if (Object.ReferenceEquals(baseHeader, control.HeaderDic))
                    {
                        differ = true;

                    }
                    else if (baseHeader.Count != control.HeaderDic.Count)
                    {
                        // 要素数が違う場合
                        differ = false;

                    }
                    else
                    {
                        // 設問1つずつ差分を見ていく
                        for (int i = 1; i <= baseHeader.Count; i++)
                        {
                            if (!baseHeader[i].Equals(control.HeaderDic[i]))
                            {
                                // 1つでも差異がある場合
                                differ = false;
                                break;
                            }
                        }
                    }

                    // ヘッダーに差分がない場合
                    if (differ)
                    {
                        BList.Add(control);

                    }
                    else
                    {
                        // 要素数が違う場合はリスト要素を変える
                        bTemperatureControlList.Add(BList);

                        BList = new List<BTemperatureControl>();
                        BList.Add(control);
                        baseHeader = control.HeaderDic;
                    }

                    cnt++;
                }

                // 最終ループのデータを追加
                if (BList.Count() > 0)
                {
                    bTemperatureControlList.Add(BList);
                }

                TemperatureControlDatas = bTemperatureControlList;
            }

            return TemperatureControlDatas;
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

                List<ApprovalHistory> historyList = new List<ApprovalHistory>();
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

    }
}