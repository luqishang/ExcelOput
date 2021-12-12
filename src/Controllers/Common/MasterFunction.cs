using HACCPExtender.Business;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers.Common
{
    public class MasterFunction
    {
        /// <summary>
        /// 店舗マスタデータ取得
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>店舗マスタデータ</returns>
        public List<ShopM> GetShopMData(MasterContext context, string shopId)
        {
            // データ取得
            var shopDt = from a in context.ShopMs
                         where a.SHOPID == shopId
                         select a;

            List<ShopM> shopMList = shopDt.ToArray().ToList();

            return shopMList;
        }

        /// <summary>
        /// 大分類マスタデータ取得
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>大分類マスタデータ</returns>
        public List<CategoryM> GetCategoryMData(MasterContext context, string shopId)
        {
            // データ取得
            var categoryDt = from a in context.CategoryMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            List<CategoryM> categoryMList = categoryDt.ToArray().ToList();

            return categoryMList;
        }

        /// <summary>
        /// 中分類マスタデータ取得
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>中分類マスタデータ</returns>
        public List<LocationM> GetLocationMData(MasterContext context, string shopId)
        {
            // データ取得
            var locationDt = from a in context.LocationMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            List<LocationM> locationMList = locationDt.ToArray().ToList();

            return locationMList;
        }

        /// <summary>
        /// 中分類マスタデータ取得(大分類を加味する）
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>中分類マスタデータ</returns>
        public List<LocationM> GetLocationMData(MasterContext context, string shopId, string categoryId)
        {

            if (categoryId != null && !categoryId.Trim().Equals(""))
            {
                // データ取得
                var reportDt1 = from a in context.ReportMs
                                where a.SHOPID == shopId
                                && a.CATEGORYID == categoryId
                                select a.LOCATIONID;

                var locationDt1 = from a in context.LocationMs
                                  where a.SHOPID == shopId
                                  join y in reportDt1 on a.LOCATIONID equals y
                                  orderby a.DISPLAYNO
                                  select a;

                return locationDt1.ToArray().ToList();

             }
            else
            {
                // データ取得
                var locationDt = from a in context.LocationMs
                                 orderby a.DISPLAYNO
                                 where a.SHOPID == shopId
                                 select a;
                return locationDt.ToArray().ToList();
            }


        }

        /// <summary>
        /// 帳票マスタデータ取得
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>帳票マスタデータ</returns>
        public List<ReportM> GetReportMData(MasterContext context, string shopId)
        {
            // データ取得
            var reportDt = from a in context.ReportMs
                           where a.SHOPID == shopId
                           select a;

            List<ReportM> reportMList = reportDt.ToArray().ToList();

            return reportMList;
        }

        /// <summary>
        /// 帳票マスタ存在チェック
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <returns>判定結果(true：データあり)</returns>
        public bool IsExistsReportM(MasterContext context, string shopId = "", string categoryId = "", string locationId = "")
        {
            bool isExists = false;

            // データ取得
            var query = from r in context.ReportMs
                        where r.SHOPID == shopId
                        select r;

            if (!string.IsNullOrEmpty(categoryId))
            {
                query = query.Where(a => a.CATEGORYID == categoryId);
            }

            if (!string.IsNullOrEmpty(locationId))
            {
                query = query.Where(a => a.LOCATIONID == locationId);
            }

            // データ件数
            if (0 < query.Count())
            {
                isExists = true;
            }

            return isExists;
        }

        /// <summary>
        /// 設問マスタ存在チェック
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <returns>判定結果(true：データあり)</returns>
        public bool IsExistsQuestionM(MasterContext context, string shopId, string categoryId = "", string locationId = "")
        {
            bool isExists = false;

            var query = from a in context.QuestionMs
                        where a.SHOPID == shopId
                            && a.DELETEFLAG == BoolKbn.KBN_FALSE
                        select a;

            if (!string.IsNullOrEmpty(categoryId))
            {
                query = query.Where(a => a.CATEGORYID == categoryId);
            }

            if (!string.IsNullOrEmpty(locationId))
            {
                query = query.Where(a => a.LOCATIONID == locationId);
            }

            if (query.Count() > 0)
            {
                isExists = true;
            }

            return isExists;
        }

        /// <summary>
        /// 承認経路マスタ存在チェック
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="managerId">承認管理作業者ID</param>
        /// <returns>判定結果(true：データあり)</returns>
        public bool IsExistsApprovalrouteM(MasterContext context, string shopId, string categoryId = "", string locationId = "", string managerId = "")
        {
            bool isExists = false;

            // データ取得
            var query = from r in context.ApprovalRouteMs
                        where r.SHOPID == shopId
                        select r;

            if (!string.IsNullOrEmpty(categoryId))
            {
                query = query.Where(a => a.CATEGORYID == categoryId);
            }

            if (!string.IsNullOrEmpty(locationId))
            {
                query = query.Where(a => a.LOCATIONID == locationId);
            }

            if (!string.IsNullOrEmpty(managerId))
            {
                query = query.Where(a => a.APPMANAGERID == managerId);
            }

            // データ件数
            if (0 < query.Count())
            {
                isExists = true;
            }

            return isExists;
        }

        /// <summary>
        /// データ承認データ存在チェック
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <returns>判定結果(true：データあり)</returns>
        public bool IsDataApproval(MasterContext context, string shopId, string categoryId = "", string locationId = "", string reportId = "")
        {
            bool isExists = false;

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MIDDLE.SHOPID ");
            sql.Append(", MIDDLE.PERIOD ");
            sql.Append(", MIDDLE.PERIODSTART ");
            sql.Append(", MIDDLE.PERIODEND ");
            sql.Append(", MIDDLE.LOCATIONID ");
            sql.Append("FROM  ");
            sql.Append("MIDDLEAPPROVAL_T MIDDLE ");
            sql.Append("WHERE ");
            sql.Append("MIDDLE.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            if (!string.IsNullOrEmpty(categoryId))
            {
                sql.Append("AND MIDDLE.CATEGORYID = '");
                sql.Append(categoryId);
                sql.Append("' ");
            }
            if (!string.IsNullOrEmpty(locationId))
            {
                sql.Append("AND MIDDLE.LOCATIONID = '");
                sql.Append(locationId);
                sql.Append("' ");
            }
            if (!string.IsNullOrEmpty(reportId))
            {
                sql.Append("AND MIDDLE.REPORTID = '");
                sql.Append(reportId);
                sql.Append("' ");
            }
            sql.Append("AND NOT EXISTS ( ");
            sql.Append("SELECT ");
            sql.Append("COMP.PERIOD ");
            sql.Append(", COMP.PERIODSTART ");
            sql.Append(", COMP.PERIODEND ");
            sql.Append("FROM ");
            sql.Append("APPROVALCOMPLETE_T COMP ");
            sql.Append("WHERE ");
            sql.Append("COMP.SHOPID = MIDDLE.SHOPID ");
            sql.Append("AND COMP.PERIOD = MIDDLE.PERIOD ");
            sql.Append("AND COMP.PERIODSTART = MIDDLE.PERIODSTART ");
            sql.Append("AND COMP.PERIODEND = MIDDLE.PERIODEND ");
            sql.Append(") ");
            var middlebase = context.Database.SqlQuery<MiddleData>(sql.ToString());
            if (middlebase != null && middlebase.Count() > 0)
            {
                isExists = true;
            }

            return isExists;
        }

        /// <summary>
        /// 管理対象マスタ存在チェック
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="managementId">管理作業者ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <returns>判定結果(true：データあり)</returns>
        public bool IsExistsManagementM(MasterContext context, string shopId, string managementId, string locationId = "")
        {
            bool isExists = false;

            var query = from a in context.ManagementMs
                        where a.SHOPID == shopId
                        select a;

            if (!string.IsNullOrEmpty(locationId))
            {
                query = query.Where(a => a.LOCATIONID == locationId);
            }

            if (!string.IsNullOrEmpty(managementId))
            {
                query = query.Where(a => a.MANAGEMENTID == locationId);
            }

            // データ件数
            if (0 < query.Count())
            {
                isExists = true;
            }

            return isExists;
        }

        /// <summary>
        /// ID採番処理
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="columnName">カラム名</param>
        /// <param name="managementId">管理対象ID</param>
        /// <param name="digits">ID桁数</param>
        /// <returns>採番ID</returns>
        public string GetNumberingID(MasterContext context, string shopId, string tableName, string columnName, string managementId = "", string reportId = "", int digits = 2)
        {
            string startId = StringOfId(1, digits);
            // IDを採番
            // 最小が"01"の場合
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT COUNT(");
            sql.Append(columnName);
            sql.Append(") AS VACANT ");
            sql.Append("FROM  ");
            sql.Append(tableName);
            sql.Append(" WHERE SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND ");
            sql.Append(columnName);
            sql.Append(" = '");
            sql.Append(startId);
            sql.Append("' ");
            if (!string.IsNullOrEmpty(managementId))
            {
                sql.Append("AND MANAGEMENTID = '");
                sql.Append(managementId);
                sql.Append("' ");
            }
            if (!string.IsNullOrEmpty(reportId))
            {
                sql.Append("AND REPORTID = '");
                sql.Append(reportId);
                sql.Append("' ");
            }
            var first = context.Database.SqlQuery<NumberingId>(sql.ToString());
            if (first.FirstOrDefault() == null
                || first.First().VACANT == 0)
            {
                return this.StringOfId(1, digits);
            }

            // 最小が"01"以外の場合
            sql = new StringBuilder();
            sql.Append("SELECT (MIN(INTEGER(");
            sql.Append(columnName);
            sql.Append("))+1) AS VACANT ");
            sql.Append("FROM  ");
            sql.Append(tableName);
            sql.Append(" WHERE SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND (INTEGER(");
            sql.Append(columnName);
            sql.Append(") +1) ");
            sql.Append("NOT IN (SELECT ");
            sql.Append(columnName);
            sql.Append(" FROM ");
            sql.Append(tableName);
            sql.Append(" WHERE SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            if (!string.IsNullOrEmpty(managementId))
            {
                sql.Append(" AND MANAGEMENTID = '");
                sql.Append(managementId);
                sql.Append("'");
            }
            if (!string.IsNullOrEmpty(reportId))
            {
                sql.Append("AND REPORTID = '");
                sql.Append(reportId);
                sql.Append("' ");
            }
            sql.Append(") ");
            sql.Append("FOR READ ONLY");
            var location_saiban = context.Database.SqlQuery<NumberingId>(sql.ToString());
            if (location_saiban == null)
            {
                return this.StringOfId(1, digits);
            }
            else
            {
                return this.StringOfId(location_saiban.First().VACANT, digits);
            }
        }

        /// <summary>
        /// IDの0埋処理
        /// </summary>
        /// <param name="num">ID</param>
        /// <param name="digits">全桁数</param>
        /// <returns></returns>
        private string StringOfId(int num, int digits)
        {
            string format = string.Empty;
            for (int i = 0; i < digits; i++)
            {
                format += "0";
            }
            string idnumber = (format + num);
            return idnumber.Substring(idnumber.Length - digits);
        }

        /// <summary>
        /// 手引書ファイル格納ディレクトリ取得
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns></returns>
        public string GetManualFolderName(MasterContext context, string shopId)
        {
            string documentFolder = GetAppSet.GetAppSetValue("Storage", "FolderName");
            string manualFolder = GetAppSet.GetAppSetValue("Storage", "Manual");

            string shopDic = this.SetShopStorageDirectory(context, shopId);

            return "~/" + documentFolder + "/" + shopDic + "/" + manualFolder;
        }

        /// <summary>
        /// 帳票ファイル格納ディレクトリ取得
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>帳票ファイル格納ディレクトリ</returns>
        public string GetReportFolderName(MasterContext context, string shopId)
        {
            string documentFolder = GetAppSet.GetAppSetValue("Storage", "FolderName");
            string reportFolder = GetAppSet.GetAppSetValue("Storage", "Report");

            string shopDic = this.SetShopStorageDirectory(context, shopId);

            return "~/" + documentFolder + "/" + shopDic + "/" + reportFolder;
        }

        /// <summary>
        /// 画像ファイル格納ディレクトリ取得
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>画像ファイル格納ディレクトリ</returns>
        public string GetImageFolderName(MasterContext context, string shopId)
        {
            string documentFolder = GetAppSet.GetAppSetValue("Storage", "FolderName");
            string imageFolder = GetAppSet.GetAppSetValue("Storage", "Image");

            string shopDic = this.SetShopStorageDirectory(context, shopId);

            return "~/" + documentFolder + "/" + shopDic + "/" + imageFolder;
        }

        /// <summary>
        /// 店舗フォルダ名取得
        /// </summary>
        /// <param name="context">MasterContextオブジェクト</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns></returns>
        public string SetShopStorageDirectory(MasterContext context, string shopId)
        {
            string path = string.Empty;

            if (string.IsNullOrEmpty(shopId))
            {
                return path;
            }

            // 店舗マスタデータ取得
            var ShopMDt = from s in context.ShopMs
                          where s.SHOPID == shopId
                          select s;

            if (ShopMDt != null && ShopMDt.Count() > 0)
            {
                string shopDic = ShopMDt.FirstOrDefault().STORAGEFNAME;
                // フォルダ名が存在する場合
                if (!string.IsNullOrEmpty(shopDic))
                {
                    return shopDic;
                }

                while (true)
                {
                    // フォルダ名が存在しない場合
                    shopDic = this.GetFolderStr();

                    // 店舗マスタデータ取得
                    var ShopMDtes = from s in context.ShopMs
                                    where s.STORAGEFNAME == shopDic
                                    select s;
                    // フォルダ名が存在しなければ決定
                    if (ShopMDtes == null || ShopMDtes.Count() == 0)
                    {
                        break;
                    }
                }
                // 店舗マスタ更新情報
                var shopM = new ShopM();
                shopM = ShopMDt.FirstOrDefault();
                shopM.STORAGEFNAME = shopDic;

                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.ShopMs.Attach(shopM);
                        context.Entry(shopM).State = EntityState.Modified;
                        context.SaveChanges();
                        tran.Commit();

                        // ディレクトリ作成
                        this.MakeDirectory(context, shopId, shopDic);

                        return shopDic;
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // ロールバック
                        tran.Rollback();
                    }
                    catch (DbUpdateException)
                    {
                        // ロールバック
                        tran.Rollback();
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

            return path;
        }

        /// <summary>
        /// 店舗ごとの各ファイルフォルダ作成
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="shopDic">店舗ストレージ名</param>
        private void MakeDirectory(MasterContext context, string shopId, string shopDic)
        {
            MasterFunction masterFunc = new MasterFunction();

            // 店舗フォルダ作成
            string documentFolder = GetAppSet.GetAppSetValue("Storage", "FolderName");
            string shopFolderPath = HostingEnvironment.MapPath("~/" + documentFolder + "/" + shopDic);
            if (!Directory.Exists(shopFolderPath))
            {
                Directory.CreateDirectory(shopFolderPath);
            }

            // reportフォルダ作成
            string reportStoragePath = masterFunc.GetReportFolderName(context, shopId);
            string reportFolderPath = HostingEnvironment.MapPath(reportStoragePath);
            if (!Directory.Exists(reportFolderPath))
            {
                Directory.CreateDirectory(reportFolderPath);
            }

            // manualフォルダ作成
            string manualStoragePath = masterFunc.GetManualFolderName(context, shopId);
            string manualFolderPath = HostingEnvironment.MapPath(manualStoragePath);
            if (!Directory.Exists(manualFolderPath))
            {
                Directory.CreateDirectory(manualFolderPath);
            }

            // imagesフォルダ作成
            string imageStoragePath = masterFunc.GetImageFolderName(context, shopId);
            string imageFolderPath = HostingEnvironment.MapPath(imageStoragePath);
            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }
        }

        /// <summary>
        /// ランダム文字列作成
        /// </summary>
        /// <returns>文字列</returns>
        private string GetFolderStr()
        {
            const string keyChars = "0123456789abcdefghijklmnopqrstuvwxyz";
            int digit = int.Parse(GetAppSet.GetAppSetValue("Storage", "Digit"));

            StringBuilder sb = new StringBuilder(digit);
            Random r = new Random();

            for (int i = 0; i < digit; i++)
            {
                // 文字位置をランダムに選択
                int pos = r.Next(keyChars.Length);
                // 選択した文字位置の文字を取得
                char c = keyChars[pos];
                // パスワードに追加
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}