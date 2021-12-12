using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Mvc;
using static HACCPExtender.Controllers.Common.CommonConstants;
using HACCPExtender.Models.Custom;
using System.Text;
using HACCPExtender.ExcelOutput;
using System.Web.Hosting;

namespace HACCPExtender.Controllers
{
    public class ReportDownloadController : Controller
    {
        private readonly MasterContext context = new MasterContext();
        CommonFunction func = new CommonFunction();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ReportDownloadController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "ReportDownload");
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
            ViewBag.editMode = func.GetEditButton(editMode);

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // 大分類ドロップダウンリスト
            var categoryDrop = this.GetBumonMDropList(shopId, string.Empty);
            if (categoryDrop.Count() == 0)
            {
                // 大分類データが存在しない場合はメッセージ表示
                ModelState.AddModelError(string.Empty, MsgConst.NODATA_CATEGORY);
                ViewBag.editMode = "disabled";
            }

            ViewBag.categoryOptions = categoryDrop;

            return View(new List<BReportDownload>());
        }

        /// <summary>
        /// 大分類ドロップダウンリスト変更処理
        /// </summary>
        /// <param name="form">画面入力値</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CategoryChange(FormCollection form)
        {
            ModelState.Clear();
            // 大分類
            string category = form["Category"];
            if (string.IsNullOrEmpty(category))
            {
                throw new ApplicationException();
            }

            // 店舗ID
            string shopId = (string)Session["SHOPID"];
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            ViewBag.editMode = func.GetEditButton(editMode);

            // 大分類ドロップダウンリスト
            ViewBag.categoryOptions = this.GetBumonMDropList(shopId, category);
            // 帳票一覧
            var ReportList = new List<BReportDownload>();
            // 帳票マスタを取得
            var reportMDt = from re in context.ReportMs
                            orderby re.DISPLAYNO
                            where re.SHOPID == shopId && re.CATEGORYID == category
                            select re;
            // 場所マスタを取得
            var locationMDt = from lo in context.LocationMs
                              where lo.SHOPID == shopId
                              select lo;

            if (reportMDt == null || reportMDt.Count() == 0)
            {
                ViewBag.noReport = "選択された大分類に帳票が設定されておりません。";
                return View("Show", ReportList);
            }

            foreach (ReportM report in reportMDt)
            {
                string locationName = string.Empty;
                var locationN = locationMDt.Where(a => a.LOCATIONID == report.LOCATIONID).FirstOrDefault();
                if (locationN != null)
                {
                    locationName = locationN.LOCATIONNAME;
                }
                var bReportDownload = new BReportDownload
                {
                    CategoryId = category,
                    LocationId = report.LOCATIONID,
                    LocationName = locationName,
                    ReportId = report.REPORTID,
                    ReportName = report.REPORTNAME,
                    ReportTemplateId = report.REPORTTEMPLATEID
                };

                ReportList.Add(bReportDownload);
            }

            // 処理日
            string nowDate = DateTime.Now.ToString("yyyyMMdd");
            string dispNowDate = DateTime.Now.ToString("yyyy-MM-dd");
            // ダウンロード自
            string startDay = form["startDate"];
            if (string.IsNullOrEmpty(startDay))
            {
                ViewBag.startDate = dispNowDate;
            } else
            {
                ViewBag.startDate = startDay;
            }
            // ダウンロード至
            string endDay = form["endDate"];
            if (string.IsNullOrEmpty(startDay))
            {
                ViewBag.endDate = dispNowDate;
            } else
            {
                ViewBag.endDate = endDay;
            }

            return View("Show", ReportList);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Download(FormCollection form, IList<BReportDownload> list)
        {
            ModelState.Clear();
            // 大分類
            string category = form["Category"];
            if (string.IsNullOrEmpty(category))
            {
                throw new ApplicationException();
            }
            // 店舗ID
            string shopId = (string)Session["SHOPID"];            
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            ViewBag.editMode = func.GetEditButton(editMode);

            // 大分類ドロップダウンリスト
            ViewBag.categoryOptions = this.GetBumonMDropList(shopId, category);
            //  画面の表示順に並び替えてリストに設定
            List<BReportDownload> reportList = list.OrderBy(BReportDownload => BReportDownload.No).ToList();
            // エラーチェック
            bool dataChk = true;

            // ダウンロード期間日時
            string startDay = form["startDate"];
            string endDay = form["endDate"];
            ViewBag.startDate = startDay;
            ViewBag.endDate = endDay;

            // ダウンロード期間の入力チェック
            if (!ChkDownloadPeriod(startDay, endDay))
            {
                dataChk = false;
            }

            // 帳票ファイル名
            var reportFile = new Dictionary<string, string>();
            // ダウンロード対象の選択
            bool choice = false;
            List<CustomReportM> reportMs = new List<CustomReportM>();

            foreach (BReportDownload report in reportList)
            {
                if (report.CheckFlg)
                {
                    choice = true;

                    // ダウンロード対象のファイル名とパスを格納
                    CustomReportM reportM = new CustomReportM();
                    reportM.LocationId = report.LocationId;
                    reportM.ReportId = report.ReportId;
                    reportMs.Add(reportM);
                }
            }

            // 未選択の場合
            if (!choice)
            {
                ModelState.AddModelError(string.Empty, "ダウンロードを行う帳票を選択してください。");
                dataChk = false;
            }

            // 入力エラーがある場合
            if (!dataChk)
            {
                return View("Show", reportList);
            }

            string start = startDay.Replace("-", "");
            start = start.Replace("/", "");
            string end = endDay.Replace("-", "");
            end = end.Replace("/", "");

            List<CustomMiddleApproval> details = GetMiddleApprovalT(shopId, category, reportMs, start, end);
            if(details == null)
            {
                ModelState.AddModelError(string.Empty, "ダウンロードする帳票が存在しませんので、条件を変えてください。");
                return View("Show", reportList);
            }

            // downloadフォルダが存在するかどうか
            MasterFunction masterFunc = new MasterFunction();
            string downloadPath = masterFunc.GetReportFolderName(context, shopId) + "/download/";
            string path = HostingEnvironment.MapPath(downloadPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // 圧縮ファイル名を生成
            string fileName = start + "-" + end + ".zip";
            string filePath = HostingEnvironment.MapPath(downloadPath + fileName);
            // 既にファイルが存在する場合は削除
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var z = ZipFile.Open(filePath, ZipArchiveMode.Update))
            {
                string sourceFileName = "";
                foreach(CustomMiddleApproval detail in details)
                {
                    sourceFileName = HostingEnvironment.MapPath("~/" + detail.ReportFilePass);
                    if(sourceFileName.IndexOf(".pdf") >= 0)
                    {
                        z.CreateEntryFromFile(sourceFileName, detail.ReportFileName, CompressionLevel.Optimal);
                    }
                }
            }

            var contentType = "application/zip";
            return File(filePath, contentType, Server.UrlEncode(fileName));
        }

        /// <summary>
        /// 中分類承認情報を取得する
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">部門ID</param>
        /// <param name="reportMs">場所ID,帳票ID</param>
        /// <param name="start">周期開始日</param>
        /// <param name="end">周期終了日</param>
        private List<CustomMiddleApproval> GetMiddleApprovalT(string shopId, string categoryId, 
                                                         List<CustomReportM> reportMs, 
                                                         string start, string end)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT DISTINCT ");
            sql.Append("   tbl_1.REPORTFILENAME AS ReportFileName ");
            sql.Append("   ,tbl_1.REPORTFILEPASS AS ReportFilePass ");
            sql.Append(" FROM  ");
            sql.Append("   MIDDLEAPPROVAL_T tbl_1 ");
            sql.Append(" WHERE ");
            sql.Append("   tbl_1.SHOPID = '" + shopId + "'");
            sql.Append("   AND tbl_1.CATEGORYID = '" + categoryId + "'");
            sql.Append("   AND tbl_1.PERIODSTART >= '" + start + "'");
            sql.Append("   AND tbl_1.PERIODSTART <= '" + end + "'");
            sql.Append("   AND tbl_1.MIDDLEGROUPNO > '0'  ");
            sql.Append("   AND tbl_1.STATUS = '1'  ");
            sql.Append(ExcelComm.GetSqlForReportInfo(reportMs, "tbl_1"));

            var detailDt = context.Database.SqlQuery<CustomMiddleApproval>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return null;
            }

            List<CustomMiddleApproval> dbList = detailDt.ToList();
            return dbList;
        }

        /// <summary>
        /// 大分類ドロップダウンリスト作成
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">部門ID</param>
        /// <returns>大分類ドロップダウンリスト</returns>
        private IEnumerable<SelectListItem> GetBumonMDropList(string shopId, string categoryId)
        {
            return context.CategoryMs.Where(s => s.SHOPID == shopId).OrderBy(s => s.DISPLAYNO).Select(s => new SelectListItem
            {
                Text = s.CATEGORYNAME,
                Value = s.CATEGORYID,
                Selected = s.CATEGORYID == categoryId
            });
        }

        /// <summary>
        /// ダウンロード期間入力チェック
        /// </summary>
        /// <param name="startDay">ダウンロード期間（開始日）</param>
        /// <param name="endDay">ダウンロード期間（終了日）</param>
        /// <returns></returns>
        private bool ChkDownloadPeriod(string startDay, string endDay)
        {
            CommonFunction comm = new CommonFunction();
            bool errorChk = true;
            DateTime dts;
            DateTime dte;

            if (string.IsNullOrEmpty(startDay))
            {
                ModelState.AddModelError(string.Empty, "ダウンロード期間（開始）を入力してください。");
                ModelState.AddModelError("startDate", string.Empty);
                errorChk = false;
            }

            if (string.IsNullOrEmpty(endDay))
            {
                ModelState.AddModelError(string.Empty, "ダウンロード期間（終了）を入力してください。");
                ModelState.AddModelError("endDate", string.Empty);
                errorChk = false;
            }

            if (!errorChk)
            {
                return errorChk;
            }

            string start = startDay.Replace("-", "");
            start = start.Replace("/", "");
            start = comm.FormatDateStr(start);
            if (!DateTime.TryParse(start, out dts))
            {
                ModelState.AddModelError(string.Empty, "ダウンロード期間（開始）の形式が誤ってます。");
                ModelState.AddModelError("startDate", string.Empty);
                errorChk = false;
            }
            string end = endDay.Replace("-", "");
            end = end.Replace("/", "");
            end = comm.FormatDateStr(end);
            if (!DateTime.TryParse(end, out dte))
            {
                ModelState.AddModelError(string.Empty, "ダウンロード期間（終了）の形式が誤ってます。");
                ModelState.AddModelError("endDate", string.Empty);
                errorChk = false;
            }

            if (!errorChk)
            {
                return errorChk;
            }

            if (dts.Date.CompareTo(dte) == 1)
            {
                ModelState.AddModelError(string.Empty, "ダウンロード期間の指定が誤っています。");
                ModelState.AddModelError("startDate", string.Empty);
                ModelState.AddModelError("endDate", string.Empty);
                errorChk = false;
            }

            return errorChk;
        }
    }
}