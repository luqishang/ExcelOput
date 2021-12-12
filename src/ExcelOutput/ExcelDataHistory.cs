using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Collections.Generic;
using HACCPExtender.Models;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Constants;
using HACCPExtender.Models.ExcelModel;
using PA.Office.ExcelObjects;
using OfficePositionAttributes;


namespace HACCPExtender.ExcelOutput
{
    public class ExcelDataHistory
    {
        // コンテキスト
        private readonly MasterContext context = new MasterContext();
        // 共通処理
        readonly CommonFunction comm = new CommonFunction();
        readonly MasterFunction masterFunc = new MasterFunction();

        /// <summary>
        /// データをExcelファイルに変換
        /// </summary>
        /// <param name="conditionList">画面入力値</param>
        /// <returns>ファイル名称list</returns>
        public List<string> OutToExcelFile(List<string> conditionList)
        {
            //設問数のインデックス
            int m_ColIndex = 0;

            //テンプレートファイル取得
            string templeteFileName = HostingEnvironment.MapPath("~/ExcelTemplate/dataHistory_template.xlsx");
            //大分類ID
            string categoryId = conditionList[0];
            //中分類ID
            string locationId = conditionList[1];
            //帳票ID
            string reportId = conditionList[2];
            //周期日
            string periodDay = conditionList[3];
            //店舗ID
            string shopId = conditionList[4];

            //大分類名・中分類名・帳票名・データ記録範囲の取得
            var categoryName = GetCategoryName(categoryId, shopId);
            var locationName = GetLocationName(locationId, shopId);
            var reportName = GetReportName(categoryId, locationId, reportId, shopId);
            var dataRecordingRange = GetDataRecordingRange(categoryId, locationId, reportId, periodDay, shopId);

            //ダウンロード時間
            var downLoadTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            //物理パスを取得する
            string downloadPath = masterFunc.GetReportFolderName(context, shopId);
            string path = HostingEnvironment.MapPath(downloadPath + "/download/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fileName = "データ履歴_" + downLoadTime;
            string excelFileName = path + fileName + ".xlsx";

            //テンプレートファイルをコピーする
            File.Copy(templeteFileName, excelFileName, true);
            File.SetAttributes(excelFileName, FileAttributes.Normal);

            //シート名
            string sheetName = Const.SheetName;

            //Excel出力用オブジェクトを生成する
            ExcelFileSingleton excelSingleton = ExcelFileSingleton.GetInstance();

            try
            {
                //Excelファイルを読み込みます。
                excelSingleton.OpenExcel(excelFileName);

                // EXCELの固定セルの内容を設定する。
                SetFixData2ExcelForDataHistory(excelSingleton, sheetName, categoryName, locationName, reportName, dataRecordingRange);

                // EXCELの明細内容を設定する。
                SetDetailData2ExcelForDataHistory(excelSingleton, sheetName, conditionList, ref m_ColIndex);

                //設問の数を判断して、空白の列を削除
                if (m_ColIndex < 20)
                {
                    excelSingleton.DeleteColOfSheet(sheetName, m_ColIndex + 2, 20 - m_ColIndex);
                }
            }

            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                throw ex;
            }

            finally
            {
                excelSingleton.CloseExcel();
            }

            List<string> retList = new List<string>();
            retList.Add(excelFileName);
            retList.Add(fileName + ".xlsx");
            return retList;
        }

        /// <summary>
        /// 固定セル内容設定メソッド
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="categoryName">大分類名称</param>
        /// <param name="locationName">中分類名称</param>
        /// <param name="reportName">帳票名</param>
        /// <param name="dataRecordingRange">データ記録範囲</param>
        private void SetFixData2ExcelForDataHistory(ExcelFileSingleton excelSingleton,
                                                    string sheetName,
                                                    string categoryName,
                                                    string locationName,
                                                    string reportName,
                                                    string dataRecordingRange)
        {
            var fixedEM = new DataHistoryFixedEM
            {
                CategoryName = "大分類:" + categoryName,
                LocationName = "中分類:" + locationName,
                ReportName = "帳票:" + reportName,
                //データ記録範囲
                DataRecordingRange = "データ記録範囲:" + dataRecordingRange
            };

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();
            ExcelRowObject headRow = new ExcelRowObject();

            var eVtype = typeof(DataHistoryFixedEM);
            foreach (PropertyInfo pf in eVtype.GetProperties())
            {
                string cellValue = (string)pf.GetValue(fixedEM);
                var attribute = (ExcelCellPositionAttribute)pf.GetCustomAttributes(typeof(ExcelCellPositionAttribute), false).FirstOrDefault();

                ExcelCellObject cell = new ExcelCellObject();
                cell.RowIndex = attribute.Row;
                cell.ColIndex = attribute.Col;
                cell.Value = cellValue;

                headRow.Cells.Add(cell);
            }
            rows.Add(headRow);
            excelSingleton.WriteRowsToSheet(sheetName, rows);
        }

        /// <summary>
        /// 明細内容セル設定メソッド
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="conditionList">画面入力値</param>
        /// <param name="m_ColIndex">出力時に削除する列数</param>
        private void SetDetailData2ExcelForDataHistory(ExcelFileSingleton excelSingleton,
                                                       string sheetName,
                                                       List<string> conditionList,
                                                       ref int m_ColIndex)
        {
            //Excel出力用Listを取得
            var detailList = GetDetailDataList(conditionList, ref m_ColIndex);

            //Excel出力の開始行
            int startRowIndex = 6;

            int rowIndex = startRowIndex;

            foreach (DataHistoryDetailEM detail in detailList)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }

            //新規の行を指定行に挿入する,
            excelSingleton.InsertRowOfSheet(sheetName, startRowIndex + 2, detailList.Count - 3);

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();

            var eVtype = typeof(DataHistoryDetailEM);

            for (int i = 0; i < detailList.Count; i++)
            {
                ExcelRowObject row = new ExcelRowObject();
                foreach (PropertyInfo pf in eVtype.GetProperties())
                {
                    var atts = pf.GetCustomAttributes(typeof(ExcelColPositionAttribute), false);
                    var attribute = (ExcelColPositionAttribute)pf.GetCustomAttributes(typeof(ExcelColPositionAttribute), false).FirstOrDefault();
                    if (attribute is null)
                    {
                        continue;
                    }
                    string cellValue = (string)pf.GetValue(detailList[i]);

                    ExcelCellObject cell = new ExcelCellObject();
                    cell.RowIndex = detailList[i].RowIndex;
                    cell.ColIndex = attribute.Col;
                    cell.Value = cellValue;

                    row.Cells.Add(cell);
                }
                rows.Add(row);
            }
            excelSingleton.WriteRowsToSheet(sheetName, rows);
        }

        /// <summary>
        /// 明細内容List取得
        /// </summary>
        /// <param name="conditionList">画面入力値</param>
        /// <param name="m_ColIndex">出力時に削除する列数</param>
        /// <returns>明細内容出力リスト</returns>
        private List<DataHistoryDetailEM> GetDetailDataList(List<string> conditionList,
                                                            ref int m_ColIndex)
        {
            var detailList = new List<DataHistoryDetailEM>();
            // 大分類Id
            string categoryId = conditionList[0];
            // 中分類Id
            string locationId = conditionList[1];
            // 帳票ID
            string reportId = conditionList[2];
            //店舗ID
            string shopId = conditionList[4];
            // 指定日
            string periodDay = conditionList[3].Replace("-", "");

            var temperatureDt = from a in context.TemperatureControlTs
                               where a.SHOPID == shopId
                                   && a.CATEGORYID == categoryId
                                   && a.LOCATIONID == locationId
                                   && a.REPORTID == reportId
                                   && a.PERIODSTART.CompareTo(periodDay) <= 0
                                   && a.PERIODEND.CompareTo(periodDay) >= 0
                               select a;

            var dbList = temperatureDt.ToArray().ToList();

            //設問の辞書型＜設問の番号、設問の内容＞（DBから抽出された最初のデータを利用）
            Dictionary<int, string> questionDic = GetQuestionDict(dbList[0]);

            //出力時に削除する列数
            m_ColIndex = questionDic.Count;

            //ヘッダ行の格納
            var header = new DataHistoryDetailEM
            {
                RecordTime = "データ記録日時",
                WorkerName = "作業者",
            };
            //質問内容をExcel出力用Listに格納
            foreach (KeyValuePair<int, string> kvp in questionDic)
            {
                var itemPType = typeof(DataHistoryDetailEM).GetProperty("Result" + kvp.Key);
                itemPType.SetValue(header, kvp.Value);
            }
            detailList.Add(header);

            //回答内容を明細内容リストに格納
            for (int i = 0; i < dbList.Count; i++)
            {
                var detail = new DataHistoryDetailEM
                {
                    RecordTime = ChangeRecordTimeFormat(dbList[i].DATAYMD),
                    WorkerName = dbList[i].WORKERNAME
                };
                for (int j = 0; j < questionDic.Count; j++)
                {
                    var itemPType = typeof(DataHistoryDetailEM).GetProperty("Result" + (j + 1));
                    var resultType = typeof(TemperatureControlT).GetProperty("RESULT" + (j + 1));
                    var result = resultType.GetValue(dbList[i]);
                    itemPType.SetValue(detail, result);
                }
                detailList.Add(detail);
            }
            return detailList;
        }

        /// <summary>
        /// 大分類名称取得
        /// </summary>
        /// <param name="categoryId">部門ID</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>大分類名称</returns>
        private string GetCategoryName(string categoryId, string shopId)
        {
            //大分類名称
            var categoryName = string.Empty;
            // 大分類マスタデータ取得
            var categoryDt = from cat in context.CategoryMs
                             orderby cat.DISPLAYNO
                             where cat.SHOPID == shopId
                             && cat.CATEGORYID == categoryId
                             select cat;
            //大分類Idを大分類名称に変換
            CategoryM categoryM = categoryDt.First();
            if (categoryM.CATEGORYID.Equals(categoryId))
            {
                categoryName = categoryM.CATEGORYNAME;
            }
            return categoryName;
        }

        /// <summary>
        /// 中分類名称取得
        /// </summary>
        /// <param name="locationId">場所ID</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>中分類名称</returns>
        private string GetLocationName(string locationId, string shopId)
        {
            //中分類名称
            var locationName = string.Empty;
            // 中分類データ取得
            var locationDt = from loc in context.LocationMs
                             orderby loc.DISPLAYNO
                             where loc.SHOPID == shopId
                             && loc.LOCATIONID == locationId
                             select loc;
            //中分類Idを中分類名称に変換
            LocationM locationM = locationDt.First();
            if (locationM.LOCATIONID.Equals(locationId))
            {
                locationName = locationM.LOCATIONNAME;
            }
            return locationName;
        }

        /// <summary>
        /// 帳票名取得
        /// </summary>
        /// <param name="categoryId">部門ID</param>
        /// <param name="locationId">場所ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>帳票名</returns>
        private string GetReportName(string categoryId,
                                     string locationId,
                                     string reportId,
                                     string shopId)
        {
            //帳票名
            var reportName = string.Empty;
            // 帳票データ取得
            var reportDt = from rep in context.ReportMs
                           orderby rep.DISPLAYNO
                           where rep.SHOPID == shopId
                              && rep.CATEGORYID == categoryId
                              && rep.LOCATIONID == locationId
                           select rep;
            //帳票Idから帳票名に変換
            ReportM reportM = reportDt.First();
            if (reportM.REPORTID.Equals(reportId))
            {
                reportName = reportM.REPORTNAME;
            }
            return reportName;
        }

        /// <summary>
        /// データ記録範囲
        /// </summary>
        /// <param name="categoryId">部門ID</param>
        /// <param name="locationId">場所ID</param>
        /// <param name="reportId">帳票ID</param>
        /// <param name="periodDay">周期日</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>データ記録範囲</returns>
        private string GetDataRecordingRange(string categoryId,
                                             string locationId,
                                             string reportId,
                                             string periodDay,
                                             string shopId)
        {
            //周期開始日
            var periodStartDate = string.Empty;
            //周期終了日
            var periodEndDate = string.Empty;

            var periodYMD = periodDay.Replace("-", "");
            var temperatureDt = from a in context.TemperatureControlTs
                                where a.SHOPID == shopId
                                    && a.CATEGORYID == categoryId
                                    && a.LOCATIONID == locationId
                                    && a.REPORTID == reportId
                                    && a.PERIODSTART.CompareTo(periodYMD) <= 0
                                    && a.PERIODEND.CompareTo(periodYMD) >= 0
                                select a;
            if (temperatureDt.Count() > 0)
            {
                TemperatureControlT temeratureItem = temperatureDt.First();
                periodStartDate = comm.FormatDateStr(temeratureItem.PERIODSTART);
                periodEndDate = comm.FormatDateStr(temeratureItem.PERIODEND);
            }
            //データ記録範囲
            string DataRecordingRange = periodStartDate + "～" + periodEndDate;
            return DataRecordingRange;
        }

        /// <summary>
        /// 設問を格納するための辞書
        /// </summary>
        /// <param name="data">DBから抽出された最初のデータ</param>
        /// <returns>設問を格納したList</returns>
        public static Dictionary<int, string> GetQuestionDict(TemperatureControlT data)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            string questionName = "QUESTION";

            for (int i = 1; i <= 20; i++)
            {
                string propertyName = questionName + i.ToString();
                var property = typeof(TemperatureControlT).GetProperty(propertyName);
                var value = property.GetValue(data);
                if (value != null)
                {
                    dict.Add(i, value.ToString());
                }
            }
            return dict;
        }

        /// <summary>
        /// 記録時間を表示用フォーマットに修正
        /// </summary>
        /// <param name="recordTime">記録時間</param>
        /// <returns>表示用に修正した記録時間</returns>
        private string ChangeRecordTimeFormat(string recordTime)
        {
            var year = recordTime.Substring(0, 4);
            var month = recordTime.Substring(4, 2);
            var day = recordTime.Substring(6, 2);
            var hour = recordTime.Substring(8, 2);
            var minute = recordTime.Substring(10, 2);
            return year + "/" + month + "/" + day + " " + hour + ":" + minute;
        }
    }
}