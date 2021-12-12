using System.Collections.Generic;
using System.Web.Mvc;
using HACCPExtender.Models.Custom;
using HACCPExtender.ExcelOutput;
using System.Threading.Tasks;
using System.Threading;

namespace HACCPExtender.Controllers
{
    public class ReportProductionController : Controller
    {

        // GET: ReportProduction
        public ActionResult Index()
        {
            return View();
        }

        #region "帳票パターン①"
        [HttpPost]
        public ActionResult DownloadR_01()
        {
            //仮の帳票Interface
            CustomReportInterfaceM reportInterface = new CustomReportInterfaceM();
            reportInterface.ReportList = new List<CustomReportM>();
            reportInterface.ShopId = "00006";
            reportInterface.CategoryId = "01";
            reportInterface.Period = "2";
            reportInterface.PeriodStart = "20201123";
            reportInterface.Path = "~/document/hjwus/report";
            reportInterface.Title = "一般的衛生管理の実施記録1";
            reportInterface.ManageId = "00001";
            CustomReportM report1 = new CustomReportM();
            report1.LocationId = "01";
            report1.ReportId = "001";
            reportInterface.ReportList.Add(report1);
            CustomReportM report2 = new CustomReportM();
            report2.LocationId = "02";
            report2.ReportId = "002";
            reportInterface.ReportList.Add(report2);
            CustomReportM report3 = new CustomReportM();
            report3.LocationId = "03";
            report3.ReportId = "003";
            reportInterface.ReportList.Add(report3);
            CustomReportM report4 = new CustomReportM();
            report4.LocationId = "04";
            report4.ReportId = "004";
            reportInterface.ReportList.Add(report4);
            CustomReportM report5 = new CustomReportM();
            report5.LocationId = "05";
            report5.ReportId = "005";
            reportInterface.ReportList.Add(report5);
            //仮の帳票Interface

            _ = OutPDF_1(reportInterface);

            return View("Index");
        }

        public async Task OutPDF_1(CustomReportInterfaceM reportInterface)
        {
            await Task.Run(() =>
            {
                ExcelPattern_1 pattern_1 = new ExcelPattern_1();
                bool ret = pattern_1.OutPDF(reportInterface);
            });

            return;
        }

        #endregion

        #region "帳票パターン②"
        [HttpPost]
        public ActionResult DownloadR_02()
        {
            ////仮の帳票Interface
            ////CategoryId:01 ; 設問:5
            CustomReportInterfaceM reportInterface = new CustomReportInterfaceM();
            reportInterface.ReportList = new List<CustomReportM>();
            reportInterface.ShopId = "zhang";
            reportInterface.CategoryId = "01";
            reportInterface.Period = "1";
            reportInterface.PeriodStart = "20200901";
            reportInterface.Path = "~/document/hjwus/report";
            reportInterface.Title = "検収記録（日報）";
            reportInterface.ManageId = "00001";
            CustomReportM report1 = new CustomReportM();
            report1.LocationId = "01";
            report1.ReportId = "001";
            reportInterface.ReportList.Add(report1);
            CustomReportM report2 = new CustomReportM();
            report2.LocationId = "01";
            report2.ReportId = "002";
            reportInterface.ReportList.Add(report2);
            CustomReportM report3 = new CustomReportM();
            report3.LocationId = "01";
            report3.ReportId = "003";
            reportInterface.ReportList.Add(report3);
            CustomReportM report4 = new CustomReportM();
            report4.LocationId = "01";
            report4.ReportId = "004";
            reportInterface.ReportList.Add(report4);
            CustomReportM report5 = new CustomReportM();
            report5.LocationId = "01";
            report5.ReportId = "005";
            reportInterface.ReportList.Add(report5);

            ////CategoryId: 02; 設問: 20
            //CustomReportInterfaceM reportInterface = new CustomReportInterfaceM();
            //reportInterface.ReportList = new List<CustomReportM>();
            //reportInterface.ShopId = "zhang";
            //reportInterface.CategoryId = "02";
            //reportInterface.Period = "1";
            //reportInterface.PeriodStart = "20200901";
            //reportInterface.Path = "~/document/hjwus/report";
            //reportInterface.Title = "検収記録（日報）";
            //reportInterface.ManageId = "00001";
            //CustomReportM report6 = new CustomReportM();
            //report6.LocationId = "01";
            //report6.ReportId = "006";
            //reportInterface.ReportList.Add(report6);
            //CustomReportM report7 = new CustomReportM();
            //report7.LocationId = "01";
            //report7.ReportId = "007";
            //reportInterface.ReportList.Add(report7);
            //CustomReportM report8 = new CustomReportM();
            //report8.LocationId = "01";
            //report8.ReportId = "008";
            //reportInterface.ReportList.Add(report8);
            //CustomReportM report9 = new CustomReportM();
            //report9.LocationId = "01";
            //report9.ReportId = "009";
            //reportInterface.ReportList.Add(report9);
            //仮の帳票Interface

            //物理パスを取得する
            _ = OutPDF_2(reportInterface);

            //PDFをダウンロードする
            return View("Index");
        }

        public async Task OutPDF_2(CustomReportInterfaceM reportInterface)
        {
            await Task.Run(() =>
            {
                ExcelPattern_2 pattern_2 = new ExcelPattern_2();
                bool ret = pattern_2.OutPDF(reportInterface);
            });

            return;
        }
        #endregion

        #region "帳票パターン③"
        [HttpPost]
        public ActionResult DownloadR_03()
        {
            //仮の帳票Interface
            CustomReportInterfaceM reportInterface = new CustomReportInterfaceM();
            reportInterface.ReportList = new List<CustomReportM>();
            reportInterface.ShopId = "Kojim";
            reportInterface.Title = "個人衛生管理の実施記録(全体日報)";
            reportInterface.CategoryId = "01";
            reportInterface.Period = "1";
            reportInterface.PeriodStart = "20200916";
            reportInterface.Path = "~/document/hjwus/report";
            reportInterface.ManageId = "00001";
            CustomReportM report1 = new CustomReportM();
            report1.LocationId = "01";
            report1.ReportId = "001";
            reportInterface.ReportList.Add(report1);
            //仮の帳票Interface

            //物理パスを取得する
            _ = OutPDF_3(reportInterface);

            return View("Index");

        }
        public async Task OutPDF_3(CustomReportInterfaceM reportInterface)
        {
            await Task.Run(() =>
            {
                ExcelPattern_3 pattern_3 = new ExcelPattern_3();
                bool ret = pattern_3.OutPDF(reportInterface);
            });

            return;
        }
        #endregion

        #region "帳票パターン④"
        [HttpPost]
        public ActionResult DownloadR_04()
        {
            //仮の帳票Interface
            CustomReportInterfaceM reportInterface = new CustomReportInterfaceM();
            reportInterface.ReportList = new List<CustomReportM>();
            reportInterface.ShopId = "KJM";
            //フードセーフティデイリーチェック
            reportInterface.Title = "フードセーフティデイリーチェック";
            reportInterface.CategoryId = "01";
            reportInterface.Period = "1";
            reportInterface.PeriodStart = "20200901";
            reportInterface.ManageId = "00001";
            reportInterface.Path = "~/document/hjwus/report";
            reportInterface.ManageId = "00001";
            CustomReportM report1 = new CustomReportM();
            report1.LocationId = "01";
            report1.ReportId = "001";
            reportInterface.ReportList.Add(report1);
            CustomReportM report2 = new CustomReportM();
            report2.LocationId = "02";
            report2.ReportId = "002";
            reportInterface.ReportList.Add(report2);
            CustomReportM report3 = new CustomReportM();
            report3.LocationId = "03";
            report3.ReportId = "003";
            reportInterface.ReportList.Add(report3);
            CustomReportM report4 = new CustomReportM();
            report4.LocationId = "04";
            report4.ReportId = "004";
            reportInterface.ReportList.Add(report4);
            //仮の帳票Interface

            //物理パスを取得する
            _ = OutPDF_4(reportInterface);

            return View("Index");
        }

        public async Task OutPDF_4(CustomReportInterfaceM reportInterface)
        {
            await Task.Run(() =>
            {
                ExcelPattern_4 pattern_4 = new ExcelPattern_4();
                bool ret = pattern_4.OutPDF(reportInterface);
            });

            return;
        }
        #endregion

        #region "帳票パターン⑤"
        [HttpPost]
        public ActionResult DownloadR_05()
        {
            //仮の帳票Interface
            CustomReportInterfaceM reportInterface = new CustomReportInterfaceM();
            reportInterface.ReportList = new List<CustomReportM>();
            reportInterface.ShopId = "PT5";
            //個人衛生管理の実施記録(個人月報)
            reportInterface.Title = "個人衛生管理の実施記録(個人月報)";
            reportInterface.CategoryId = "01";
            reportInterface.Period = "3";
            reportInterface.PeriodStart = "20201001";
            reportInterface.ManageId = "00001";
            reportInterface.Path = "~/document/hjwus/report";
            reportInterface.ManageId = "00001";
            CustomReportM report1 = new CustomReportM();
            report1.LocationId = "01";
            report1.ReportId = "001";
            reportInterface.ReportList.Add(report1);
            //仮の帳票Interface

            //物理パスを取得する
            _ = OutPDF_5(reportInterface);

            return View("Index");
        }

        public async Task OutPDF_5(CustomReportInterfaceM reportInterface)
        {
            await Task.Run(() =>
            {
                ExcelPattern_5 pattern_5 = new ExcelPattern_5();
                bool ret = pattern_5.OutPDF(reportInterface);
            });

            return;
        }
        #endregion
    }
}