using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
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
    public class ApprovalDataDetailController : Controller
    {
        private MasterContext context = new MasterContext();
        readonly CommonFunction comm = new CommonFunction();
        private static readonly int QUESTION_NUM_MAX = int.Parse(GetAppSet.GetAppSetValue("Question", "Max"));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ApprovalDataDetailController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "ApprovalDataDetail");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 初期表示
        /// </summary>
        /// <param name="ApplovalId">承認ID</param>
        /// <returns>画面表示</returns>
        [HttpGet]
        public ActionResult Show()
        {
            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // 承認分類
            string nodeId = (string)Session["PENDINGNODEID"];
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

            // 画面情報
            var approvalDataDetail = new BApprovalDataDetail();

            // 中分類承認、大分類承認から遷移の場合
            if (APPROVALLEVEL.MIDDLE.Equals(nodeId) || APPROVALLEVEL.MAJORE.Equals(nodeId))
            {
                // データ記録取得
                approvalDataDetail = GetApprovalDatas(shopId, categoryId, locationId, reportId, periodId, periodStart);
            }
            else
            {
                // 帳票
                ViewBag.reportOptions = new SelectListItem[] { };

                // 施設承認から遷移の場合

                // 大分類ID
                approvalDataDetail.CategoryId = categoryId;
                // 中分類ドロップボックス取得
                ViewBag.locationOptions = GetLocationDrop(shopId, categoryId, periodId, periodStart, string.Empty);
                // 周期
                approvalDataDetail.Period = periodId;
                // 周期開始日
                approvalDataDetail.PeriodStart = periodStart;
            }

            // 承認ノード
            approvalDataDetail.nodeId = nodeId;

            return View(approvalDataDetail);
        }

        /// <summary>
        /// 中分類ドロップダウンリスト変更処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult LocationChange(FormCollection form)
        {
            // post時の変更前情報をクリア
            ModelState.Clear();

            // 大分類
            string categoryId = form["CategoryId"];
            // 中分類
            string locationId = form["LocationId"];
            // 周期
            string periodId = form["Period"];
            // 周期開始日
            string periodStart = form["PeriodStart"];

            if (string.IsNullOrEmpty(categoryId) 
                || string.IsNullOrEmpty(locationId) 
                || string.IsNullOrEmpty(periodId) 
                || string.IsNullOrEmpty(periodStart))
            {
                throw new ApplicationException();
            }

            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);

            // 画面情報
            var approvalDataDetail = new BApprovalDataDetail();

            // ノードID
            approvalDataDetail.nodeId = APPROVALLEVEL.FACILITY;
            // 大分類ID
            approvalDataDetail.CategoryId = categoryId;
            // 中分類ドロップダウンリスト
            ViewBag.locationOptions = this.GetLocationDrop(shopId, categoryId, periodId, periodStart, locationId);
            // 周期
            approvalDataDetail.Period = periodId;
            // 周期開始日
            approvalDataDetail.PeriodStart = periodStart;
            // 帳票ドロップダウンリスト
            ViewBag.reportOptions = this.GetReportDrop(shopId, categoryId, locationId, periodId, periodStart, string.Empty);

            return View("Show", approvalDataDetail);
        }

        /// <summary>
        /// 帳票ドロップダウンリスト変更処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ReportChange(FormCollection form)
        {
            // post時の変更前情報をクリア
            ModelState.Clear();

            // 大分類
            string categoryId = form["CategoryId"];
            // 中分類
            string locationId = form["LocationId"];
            // 帳票ID
            string reportId = form["ReportId"];
            // 周期
            string periodId = form["Period"];
            // 周期開始日
            string periodStart = form["PeriodStart"];

            if (string.IsNullOrEmpty(categoryId)
                || string.IsNullOrEmpty(locationId)
                || string.IsNullOrEmpty(reportId)
                || string.IsNullOrEmpty(periodId)
                || string.IsNullOrEmpty(periodStart))
            {
                throw new ApplicationException();
            }

            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);

            // データ記録取得
            var approvalDataDetail = GetApprovalDatas(shopId, categoryId, locationId, reportId, periodId, periodStart);

            // ノードID
            approvalDataDetail.nodeId = APPROVALLEVEL.FACILITY;
            // 大分類ID
            approvalDataDetail.CategoryId = categoryId;
            // 中分類ドロップダウンリスト
            ViewBag.locationOptions = this.GetLocationDrop(shopId, categoryId, periodId, periodStart, locationId);
            // 周期
            approvalDataDetail.Period = periodId;
            // 周期開始日
            approvalDataDetail.PeriodStart = periodStart;
            // 帳票ドロップダウンリスト
            ViewBag.reportOptions = this.GetReportDrop(shopId, categoryId, locationId, periodId, periodStart, reportId);

            return View("Show", approvalDataDetail);
        }

        /// <summary>
        /// データ承認画面戻り
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult BackShow(FormCollection form)
        {
            string nodeId = form["nodeId"];

            Session.Add("PENDINGNODEID", nodeId);
            Session.Add("PENDINGCATEGORYID", form["CategoryId"]);
            Session.Add("PENDINGLOCATIONID", form["LocationId"]);
            Session.Add("PENDINGREPORTID", form["ReportId"]);
            Session.Add("PENDINGPERIODID", form["Period"]);
            Session.Add("PENDINGSTARTDATE", form["PeriodStart"]);

            if (APPROVALLEVEL.MIDDLE.Equals(nodeId))
            {
                return RedirectToAction("Show", "MiddleApproval");

            } else if (APPROVALLEVEL.MAJORE.Equals(nodeId))
            {
                return RedirectToAction("Show", "MajorApproval");

            } else if (APPROVALLEVEL.FACILITY.Equals(nodeId))
            {
                return RedirectToAction("Show", "FacilityApproval");
            }

            return View("Show");
        }

        /// <summary>
        /// 承認データ詳細情報取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <returns>承認データ詳細情報</returns>
        private BApprovalDataDetail GetApprovalDatas(string shopId, string categoryId, string locationId, string reportId, string periodId, string periodStart)
        {
            // 画面情報
            var approvalDataDetail = new BApprovalDataDetail();

            // 大分類ID
            approvalDataDetail.CategoryId = categoryId;
            // 中分類ID
            approvalDataDetail.LocationId = locationId;
            // 帳票ID
            approvalDataDetail.ReportId = reportId;
            // 周期
            approvalDataDetail.Period = periodId;
            // 周期開始日
            approvalDataDetail.PeriodStart = periodStart;
            // 帳票ファイル数
            approvalDataDetail.ReportFileCount = 0;

            // 大分類名
            var category = from ca in context.CategoryMs
                           where ca.SHOPID == shopId && ca.CATEGORYID == categoryId
                           select ca;
            if (category.Count() > 0)
            {
                approvalDataDetail.CategoryName = category.First().CATEGORYNAME;
            }
            // 中分類名
            var location = from lo in context.LocationMs
                           where lo.SHOPID == shopId && lo.LOCATIONID == locationId
                           select lo;
            if (location.Count() > 0)
            {
                approvalDataDetail.LocationName = location.First().LOCATIONNAME;
            }
            // 帳票名
            var report = from re in context.ReportMs
                         where re.SHOPID == shopId
                             && re.CATEGORYID == categoryId
                             && re.LOCATIONID == locationId
                             && re.REPORTID == reportId
                         select re;
            if (report.Count() > 0)
            {
                approvalDataDetail.ReportName = report.First().REPORTNAME;
            }

            // 帳票ファイルデータ取得
            var reportFileDt = this.GetReportFile(shopId, categoryId, locationId, reportId, periodId, periodStart);
            if (reportFileDt != null && reportFileDt.Count() == 1)
            {
                approvalDataDetail.ReportFileName = reportFileDt[0].REPORTFILENAME;    // 帳票ファイル名
                approvalDataDetail.ReportFilePath = reportFileDt[0].REPORTFILEPASS;    // 帳票ファイルパス
                approvalDataDetail.ReportFileCount = 1;                                // 帳票ファイル別数
            } else if (reportFileDt != null && reportFileDt.Count() > 1)
            {
                approvalDataDetail.ReportFileCount = reportFileDt.Count;
            }

            // 画面用データリスト
            var bTemperatureControl = new List<BTemperatureControl>();

            // 温度衛生管理を取得
            var temeratureTDt = from tem in context.TemperatureControlTs
                                orderby tem.DATAYMD
                                where tem.SHOPID == shopId
                                    && tem.CATEGORYID == categoryId
                                    && tem.LOCATIONID == locationId
                                    && tem.REPORTID == reportId
                                    && tem.PERIOD == periodId
                                    && tem.PERIODSTART == periodStart
                                select tem;

            if (temeratureTDt.Count() > 0)
            {
                foreach (TemperatureControlT temperature in temeratureTDt)
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
                            } else
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
                            } else
                            {
                                property = typeof(TemperatureControlT).GetProperty(string.Format("RESULT{0:D2}", (i + 1).ToString()));
                                var result = (string)property.GetValue(temperature);
                                // 回答を設定
                                anserDic.Add(i + 1, result);
                                // 添付フラグ=false
                                attachDic.Add(i + 1, false);
                            }
                        } else
                        {
                            // 設問がない場合
                            break;
                        }
                    }

                    // データ個々で違う帳票ファイルが存在する場合
                    if (approvalDataDetail.ReportFileCount > 1)
                    {
                        // 中分類データ承認から帳票ファイルを取得
                        var middleDt = from mi in context.MiddleApprovalTs
                                       where mi.SHOPID == shopId
                                            && mi.CATEGORYID == categoryId
                                            && mi.LOCATIONID == locationId
                                            && mi.REPORTID == reportId
                                            && mi.APPROVALID == temperature.APPROVALID
                                       select mi;

                        if (middleDt.Count() > 0 && middleDt.FirstOrDefault() != null)
                        {
                            BTemp.REPORTFILENAME = middleDt.FirstOrDefault().REPORTFILENAME;    // 帳票ファイル名
                            BTemp.REPORTFILEPASS = middleDt.FirstOrDefault().REPORTFILEPASS;    // 帳票ファイルパス
                        }
                    }

                    BTemp.HeaderDic = headerDic;
                    BTemp.AnserDic = anserDic;
                    BTemp.AttachDic = attachDic;
                    bTemperatureControl.Add(BTemp);
                }

                // ヘッダーDictionaryの内容で設問文章の差異を判定し、出力するテーブルを変える
                var bTemperatureControlList = new List<List<BTemperatureControl>>();
                var BList =  new List<BTemperatureControl>();
                int cnt = 0;
                var baseHeader = new Dictionary<int, string>();
                foreach (BTemperatureControl control in bTemperatureControl)
                {
                    if (cnt == 0)
                    {
                        baseHeader = control.HeaderDic;
                        BList.Add(control);
                        // データが1件の場合
                        if (bTemperatureControl.Count() == 1)
                        {
                            bTemperatureControlList.Add(BList);
                        }
                        cnt++;
                        continue;
                    }

                    bool differ = true;

                    // 同じインスタンスの場合
                    if (Object.ReferenceEquals(baseHeader, control.HeaderDic))
                    {
                        differ = true;

                    } else if (baseHeader.Count != control.HeaderDic.Count)
                    {
                        // 要素数が違う場合
                        differ = false;

                    } else
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

                    } else
                    {
                        // 要素数が違う場合はリスト要素を変える
                        bTemperatureControlList.Add(BList);

                        BList = new List<BTemperatureControl>();
                        BList.Add(control);
                        baseHeader = control.HeaderDic;
                    }

                    // 最終ループの場合
                    if (bTemperatureControl.Count() == cnt + 1)
                    {
                        bTemperatureControlList.Add(BList);
                    }

                    cnt++;
                }
                
                approvalDataDetail.ApprovalDataList = bTemperatureControlList;
            } else
            {
                ModelState.AddModelError(string.Empty, "表示対象データが存在しません。");
            }

            return approvalDataDetail;
        }

        /// <summary>
        /// 中分類ドロップダウンリスト
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <param name="locationId">帳票ID</param>
        /// <returns>中分類ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetLocationDrop(string shopId, string categoryId, string periodId, string periodStart, string locationId)
        {
            var locationDt = this.GetLocation(shopId, categoryId, periodId, periodStart);
            if (locationDt == null)
            {
                ModelState.AddModelError(string.Empty, "表示対象データが存在しません。");
                return new SelectListItem[] { };
            }

            // 中分類ドロップボックス取得
            return locationDt.Select(s => new SelectListItem
            {
                Text = s.LOCATIONNAME,
                Value = s.LOCATIONID,
                Selected = s.LOCATIONID == locationId
            });
        }

        /// <summary>
        /// 中分類データ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <returns>中分類データ</returns>
        private DbRawSqlQuery<MiddleData> GetLocation(string shopId, string categoryId, string periodId, string periodStart)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MID.LOCATIONID ");
            sql.Append(", LO.LOCATIONNAME ");
            sql.Append("FROM ");
            sql.Append("LOCATION_M LO, ");
            sql.Append("( ");
            sql.Append("SELECT ");
            sql.Append("DISTINCT(LOCATIONID) AS LOCATIONID ");
            sql.Append("FROM ");
            sql.Append("MIDDLEAPPROVAL_T ");
            sql.Append("WHERE ");
            sql.Append("SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("AND PERIOD = '");
            sql.Append(periodId);
            sql.Append("' ");
            sql.Append("AND PERIODSTART = '");
            sql.Append(periodStart);
            sql.Append("' ");
            sql.Append("AND DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append(") MID ");
            sql.Append("WHERE ");
            sql.Append("LO.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND LO.LOCATIONID = MID.LOCATIONID ");
            var major = context.Database.SqlQuery<MiddleData>(sql.ToString());
            if (major.Count() > 0)
            {
                return major;
            }

            return null;
        }

        /// <summary>
        /// 帳票ドロップダウンリスト作成
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <param name="reportId">帳票ID</param>
        /// <returns>帳票ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetReportDrop(string shopId, string categoryId, string locationId, string periodId, string periodStart, string reportId)
        {
            var reportDt = this.GetReport(shopId, categoryId, locationId, periodId, periodStart);
            if (reportDt == null)
            {
                ModelState.AddModelError(string.Empty, "表示対象データが存在しません。");
                return new SelectListItem[] { };
            }

            // 帳票ドロップボックス取得
            return reportDt.Select(s => new SelectListItem
            {
                Text = s.REPORTNAME,
                Value = s.REPORTID,
                Selected = s.REPORTID == reportId
            });
        }

        /// <summary>
        /// 帳票データ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <returns>帳票ドロップダウンリスト</returns>
        private DbRawSqlQuery<MiddleData> GetReport(string shopId, string categoryId, string locationId, string periodId, string periodStart)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MID.REPORTID ");
            sql.Append(", RE.REPORTNAME ");
            sql.Append("FROM ");
            sql.Append("REPORT_M RE, ");
            sql.Append("( ");
            sql.Append("SELECT ");
            sql.Append("DISTINCT(REPORTID) AS REPORTID ");
            sql.Append("FROM ");
            sql.Append("MIDDLEAPPROVAL_T ");
            sql.Append("WHERE ");
            sql.Append("SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND CATEGORYID = '");
            sql.Append(categoryId);
            sql.Append("' ");
            sql.Append("AND LOCATIONID = '");
            sql.Append(locationId);
            sql.Append("' ");
            sql.Append("AND PERIOD = '");
            sql.Append(periodId);
            sql.Append("' ");
            sql.Append("AND PERIODSTART = '");
            sql.Append(periodStart);
            sql.Append("' ");
            sql.Append("AND DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append(") MID ");
            sql.Append("WHERE ");
            sql.Append("RE.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND RE.REPORTID = MID.REPORTID ");
            var major = context.Database.SqlQuery<MiddleData>(sql.ToString());
            if (major.Count() > 0)
            {
                return major;
            }

            return null;
        }

        /// <summary>
        /// 帳票ファイルデータ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="periodId">周期ID</param>
        /// <param name="periodStart">周期開始日</param>
        /// <returns>帳票ファイルデータ</returns>
        private List<MiddleData> GetReportFile(string shopId, string categoryId, string locationId, string reportId, string periodId, string periodStart)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MID.REPORTFILENAME ");
            sql.Append(", MID.REPORTFILEPASS ");
            sql.Append("FROM ");
            sql.Append("MIDDLEAPPROVAL_T MID ");
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
            sql.Append("AND MID.PERIODSTART = '");
            sql.Append(periodStart);
            sql.Append("' ");
            sql.Append("AND MID.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("AND MID.REPORTFILENAME IS NOT NULL ");
            sql.Append("AND MID.REPORTFILEPASS IS NOT NULL ");
            sql.Append("GROUP BY ");
            sql.Append("MID.REPORTFILENAME, MID.REPORTFILEPASS ");
            var major = context.Database.SqlQuery<MiddleData>(sql.ToString());
            if (major.Count() > 0)
            {
                return major.ToList();
            }

            return null;
        }
    }
}