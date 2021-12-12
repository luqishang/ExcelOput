using HACCPExtender.Business;
using HACCPExtender.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers.Common
{
    public class SendMailBusiness
    {
        /// <summary>
        /// 中分類承認依頼メール送信処理
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="mailToList">メール送信情報リスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodId">周期</param>
        /// <param name="periodStart">周期開始日</param>
        public void SendMiddleRequestMail(MasterContext context, List<MailInfo> mailToList, string shopId, string categoryId, string locationId, string reportId, string periodId, string periodStart)
        {
            if (mailToList.Count() == 0)
            {
                return;
            }

            var sender = new MailSenderFunction();

            // 大分類名
            var categoryDt = context.CategoryMs.Where(c => c.SHOPID == shopId && c.CATEGORYID == categoryId);
            // 中分類名
            var locationDt = context.LocationMs.Where(l => l.SHOPID == shopId && l.LOCATIONID == locationId);
            // 帳票名
            //var reportDt = context.ReportMs.Where(r => r.SHOPID == shopId && r.REPORTID == reportId);

            // メールタイトル作成
            var titleStr = GetAppSet.GetAppSetValue("MiddleRequestMail", "Subject");
            var systemName = GetAppSet.GetAppSetValue("Mail", "SYSTEMNAME");
            string title = titleStr.Replace("%SYSTEMNAME%", systemName)
                            .Replace("%CATEGORY%", categoryDt.FirstOrDefault().CATEGORYNAME)
                            .Replace("%LOCATION%", locationDt.FirstOrDefault().LOCATIONNAME);

            // メール本文作成 
            string URL = this.SetURL(URLShoriKBN.MIDDLE_APPROVAL, categoryId, locationId, reportId, periodId, periodStart, shopId);
            string body = string.Empty;
            var bodyStr = HostingEnvironment.MapPath(GetAppSet.GetAppSetValue("MiddleRequestMail", "BodyTemplate"));
            using (var sr = new StreamReader(bodyStr, Encoding.GetEncoding("shift_jis")))
            {
                // パラメータ（%～%）の置換
                body = sr.ReadToEnd()
                        .Replace("%SYSTEMNAME%", systemName)
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
                GetAppSet.GetAppSetValue("MiddleRequestMail", "ContentType"));
        }

        /// <summary>
        /// 大分類承認依頼メール送信処理
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="mailToList">メール送信情報リスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="periodId">周期</param>
        /// <param name="periodStart">周期開始日</param>
        public void SendMajorRequestMail(MasterContext context, List<MailInfo> mailToList, string shopId, string categoryId, string locationId, string periodId, string periodStart)
        {
            // 大分類承認担当者へメール送信
            if (mailToList.Count() == 0)
            {
                return;
            }

            var sender = new MailSenderFunction();

            // 大分類名
            var categoryDt = context.CategoryMs.Where(c => c.SHOPID == shopId && c.CATEGORYID == categoryId);
            // 中分類名
            var locationDt = context.LocationMs.Where(l => l.SHOPID == shopId && l.LOCATIONID == locationId);
            // 帳票名
            //var reportDt = context.ReportMs.Where(r => r.SHOPID == shopId && r.REPORTID == reportId);

            // メールタイトル作成
            var titleStr = GetAppSet.GetAppSetValue("MajorRequestMail", "Subject");
            var systemName = GetAppSet.GetAppSetValue("Mail", "SYSTEMNAME");
            string title = titleStr.Replace("%SYSTEMNAME%", systemName)
                            .Replace("%CATEGORY%", categoryDt.FirstOrDefault().CATEGORYNAME);

            // メール本文作成 
            string URL = this.SetURL(URLShoriKBN.MAJOR_APPROVAL, categoryId, locationId, null, periodId, periodStart, shopId);
            string body = string.Empty;
            var bodyStr = HostingEnvironment.MapPath(GetAppSet.GetAppSetValue("MajorRequestMail", "BodyTemplate"));
            using (var sr = new StreamReader(bodyStr, Encoding.GetEncoding("shift_jis")))
            {
                // パラメータ（%～%）の置換
                body = sr.ReadToEnd()
                        .Replace("%SYSTEMNAME%", systemName)
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
                GetAppSet.GetAppSetValue("MajorRequestMail", "ContentType"));
        }

        /// <summary>
        /// 施設承認依頼メール送信処理
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="mailToList">メール送信情報リスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="periodId">周期</param>
        /// <param name="periodStart">周期開始日</param>
        public void SendFacilityRequestMail(MasterContext context, List<MailInfo> mailToList, string shopId, string categoryId, string periodId, string periodStart)
        {
            // 大分類承認担当者へメール送信
            if (mailToList.Count() == 0)
            {
                return;
            }

            var sender = new MailSenderFunction();

            // 大分類名
            var categoryDt = context.CategoryMs.Where(c => c.SHOPID == shopId && c.CATEGORYID == categoryId);

            // メールタイトル作成 
            var titleStr = GetAppSet.GetAppSetValue("FacilityRequestMail", "Subject");
            var systemName = GetAppSet.GetAppSetValue("Mail", "SYSTEMNAME");
            string title = titleStr.Replace("%SYSTEMNAME%", systemName);

            // メール本文作成 
            string URL = this.SetURL(URLShoriKBN.FACILITY_APPROVAL, categoryId, null, null, periodId, periodStart, shopId);
            string body = string.Empty;
            var bodyStr = HostingEnvironment.MapPath(GetAppSet.GetAppSetValue("FacilityRequestMail", "BodyTemplate"));
            using (var sr = new StreamReader(bodyStr, Encoding.GetEncoding("shift_jis")))
            {
                // パラメータ（%～%）の置換
                body = sr.ReadToEnd()
                        .Replace("%SYSTEMNAME%", systemName)
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
                GetAppSet.GetAppSetValue("FacilityRequestMail", "ContentType"));
        }

        /// <summary>
        /// URL作成（メール記載用）
        /// </summary>
        /// <param name="mode">処理区分</param>
        /// <param name="category">大分類ID</param>
        /// <param name="location">中分類ID</param>
        /// <param name="report">帳票ID</param>
        /// <param name="period">周期</param>
        /// <param name="periodStart">周期開始日</param>
        /// <param name="shop">店舗ID</param>
        /// <returns>URL文字列</returns>
        public string SetURL(string mode, string category, string location, string report, string period, string periodStart, string shop)
        {
            string pUrl = URLParameter.URL;
            string pMode = URLParameter.MODE;
            string pCategory = URLParameter.CATEGORYID;
            string pLocation = URLParameter.LOCATIONID;
            string pReport = URLParameter.REPORTID;
            string pPeriod = URLParameter.PERIODID;
            string pPeriodStart = URLParameter.PERIODSTART;
            string pShop = URLParameter.SHOPID;
            string root = string.Empty;

            using (var context = new MasterContext())
            {
                // 汎用マスタからルートURL取得
                var GeneralM = from ge in context.GeneralPurposeMs
                               where ge.KEY == EnvironmentKey.KEY_HOSTNAME
                               select ge;

                if (GeneralM.Count() > 0 && GeneralM.FirstOrDefault() != null)
                {
                    root = GeneralM.FirstOrDefault().VALUE1 + pUrl;
                }
            }
            // 汎用マスタから取得できない場合
            if (string.IsNullOrEmpty(root) && HttpContext.Current != null)
            {
                root = HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.AbsolutePath, "") + pUrl;
            }

            StringBuilder strUrl = new StringBuilder();
            strUrl.Append(root);
            strUrl.Append("?");
            strUrl.Append(pMode);
            strUrl.Append("=");
            strUrl.Append(mode);
            if (!string.IsNullOrEmpty(category))
            {
                strUrl.Append("&");
                strUrl.Append(pCategory);
                strUrl.Append("=");
                strUrl.Append(category);
            }
            if (!string.IsNullOrEmpty(location))
            {
                strUrl.Append("&");
                strUrl.Append(pLocation);
                strUrl.Append("=");
                strUrl.Append(location);
            }
            if (!string.IsNullOrEmpty(report))
            {
                strUrl.Append("&");
                strUrl.Append(pReport);
                strUrl.Append("=");
                strUrl.Append(report);
            }
            strUrl.Append("&");
            strUrl.Append(pPeriod);
            strUrl.Append("=");
            strUrl.Append(period);
            strUrl.Append("&");
            strUrl.Append(pPeriodStart);
            strUrl.Append("=");
            strUrl.Append(periodStart);
            strUrl.Append("&");
            strUrl.Append(pShop);
            strUrl.Append("=");
            strUrl.Append(shop);

            return strUrl.ToString();
        }
    }
}