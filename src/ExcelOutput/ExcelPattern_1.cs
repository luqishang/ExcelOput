using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HACCPExtender.Models.ExcelModel;
using HACCPExtender.Models;
using HACCPExtender.Models.Custom;
using System.Drawing;
using HACCPExtender.Constants;
using PA.Office.ExcelObjects;
using OfficePositionAttributes;
using System.Reflection;
using System.Web.Hosting;
using System.Threading;
using HACCPExtender.Controllers.Common;
using System.Data.Entity;
using HACCPExtender.Business;

namespace HACCPExtender.ExcelOutput
{
    /// <summary>
    /// パターン①
    /// author : PTJ.Cheng
    /// Create Date : 2020/09/25
    /// </summary>
    public class ExcelPattern_1
    {
        private MasterContext context = new MasterContext();

        /// <summary>
        /// 中分類の明細データを取得する
        /// </summary>
        /// <param name="reportInterface"></param>
        public bool OutPDF(CustomReportInterfaceM reportInterface)
        {
            //テンプレートファイル取得
            string templeteFileName = HostingEnvironment.MapPath("~/ExcelTemplate/pattern_1_template.xlsx");

            //物理パスを取得する
            string path = HostingEnvironment.MapPath(reportInterface.Path + "/" + reportInterface.PeriodStart + "/");
            if (!ExcelComm.CheckDir(path))
            {
                return false;
            }
            
            //出力用ファイルのパスを取得する
            string fileName = reportInterface.ShopId + "_" +
                              reportInterface.CategoryId + "_" +
                              reportInterface.Title + "_" +
                              reportInterface.PeriodStart;
            string excelFileName = path + fileName + ".xlsx";
            //PDFのパスを取得する
            string pdfFileName = path + fileName + ".pdf";

            // ファイルが開かれている状態の場合は別スレッドでファイルを処理しているとみなす
            // ファイルロックが解除されるまで待機する
            int cnt = 0;
            while (true)
            {
                if (!ExcelComm.IsFileLocked(excelFileName))
                {
                    break;
                }
                // 1分以上待機の場合は処理を進めてみる
                if (cnt > 20)
                {
                    break;
                }
                // 3秒間待機
                Thread.Sleep(3000);
                cnt++;
            }

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
                SetFixData2Excel(excelSingleton, sheetName, reportInterface);
                // EXCELの明細内容を設定する。
                SetDetailData2Excel(excelSingleton, sheetName, reportInterface);
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

            //ExcelファイルをPDFに変換する
            ExcelSave excelSave = new ExcelSave();
            excelSave.SaveAsPdf(excelFileName, pdfFileName);

            //PDFに変換後、TempのExcelファイルを削除する
            File.Delete(excelFileName);

            //帳票ファイル名と帳票ファイル格納パスを中分類承認情報に登録する
            string documentFolder = GetAppSet.GetAppSetValue("Storage", "FolderName");
            string filePass = pdfFileName.Substring(pdfFileName.IndexOf(documentFolder));
            UpdateMiddleApprovalT(filePass, fileName + ".pdf", reportInterface);

            return true;
        }

        /// <summary>
        /// 帳票ファイル名と帳票ファイル格納パスを中分類承認情報に登録する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="reportInterface"></param>
        private void UpdateMiddleApprovalT(string path, string fileName, CustomReportInterfaceM reportInterface)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT ");
            sql.Append("  tbl_3.* ");
            sql.Append(" FROM  ");
            sql.Append("  TEMPERATURECONTROL_T tbl_1 ");
            sql.Append(" INNER JOIN ( ");
            sql.Append("    SELECT ");
            sql.Append("         tbl_5.SHOPID ");
            sql.Append("         , tbl_5.CATEGORYID ");
            sql.Append("         , tbl_5.LOCATIONID ");
            sql.Append("         , tbl_5.REPORTID ");
            sql.Append("         , SUBSTR(tbl_5.DATAYMD, 1, 8) AS YMD ");
            sql.Append("         , MAX(tbl_5.DATAYMD) AS MAXDATAYMD ");
            sql.Append("    FROM ");
            sql.Append("        TEMPERATURECONTROL_T tbl_5 ");
            sql.Append("    WHERE ");
            sql.Append("        tbl_5.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("  AND tbl_5.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("  AND tbl_5.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("  AND tbl_5.PERIODSTART = '");
                sql.Append(reportInterface.PeriodStart);
                sql.Append("' ");
            }
            sql.Append(ExcelComm.GetSqlForReportInfo(reportInterface.ReportList, "tbl_5"));
            sql.Append("    GROUP BY ");
            sql.Append("         tbl_5.SHOPID");
            sql.Append("        , tbl_5.CATEGORYID ");
            sql.Append("        , tbl_5.LOCATIONID ");
            sql.Append("        , tbl_5.REPORTID ");
            sql.Append("        , tbl_5.PERIOD ");
            sql.Append("        , tbl_5.PERIODSTART ");
            sql.Append("        , SUBSTR(tbl_5.DATAYMD, 1, 8) ");
            sql.Append(" ) tbl_6  ");
            sql.Append("     ON tbl_1.SHOPID = tbl_6.SHOPID  ");
            sql.Append("     AND tbl_1.CATEGORYID = tbl_6.CATEGORYID  ");
            sql.Append("     AND tbl_1.LOCATIONID = tbl_6.LOCATIONID  ");
            sql.Append("     AND tbl_1.REPORTID = tbl_6.REPORTID  ");
            sql.Append("     AND tbl_1.DATAYMD = tbl_6.MAXDATAYMD  ");
            sql.Append("  INNER JOIN LOCATION_M tbl_2 ");
            sql.Append("    ON tbl_1.SHOPID = tbl_2.SHOPID ");
            sql.Append("    AND tbl_1.LOCATIONID = tbl_2.LOCATIONID ");
            sql.Append("  INNER JOIN MIDDLEAPPROVAL_T tbl_3 ");
            sql.Append("    ON tbl_1.SHOPID = tbl_3.SHOPID ");
            sql.Append("    AND tbl_1.CATEGORYID = tbl_3.CATEGORYID ");
            sql.Append("    AND tbl_1.LOCATIONID = tbl_3.LOCATIONID ");
            sql.Append("    AND tbl_1.REPORTID = tbl_3.REPORTID ");
            sql.Append("    AND tbl_1.APPROVALID = tbl_3.APPROVALID ");
            sql.Append("    AND tbl_3.MIDDLEGROUPNO > '0'  ");
            sql.Append("    AND tbl_3.STATUS = '1'  ");
            sql.Append("  INNER JOIN WORKER_M tbl_4 ");
            sql.Append("    ON tbl_3.SHOPID = tbl_4.SHOPID ");
            sql.Append("    AND tbl_3.MIDDLESNNUSER = tbl_4.WORKERID ");

            var detailDt = context.Database.SqlQuery<MiddleApprovalT>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return;
            }

            List<MiddleApprovalT> dbList = detailDt.ToList();

            foreach(MiddleApprovalT detail in dbList)
            {
                //更新ユーザーID
                detail.UPDUSERID = reportInterface.ManageId;
                //帳票ファイル名
                detail.REPORTFILENAME = fileName;
                //帳票ファイル格納パス
                detail.REPORTFILEPASS = path;
            }

            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (MiddleApprovalT upddata in dbList)
                        {
                            context.MiddleApprovalTs.Attach(upddata);
                            context.Entry(upddata).State = EntityState.Modified;
                        }
                        context.SaveChanges();

                        // コミット
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 固定内容設定用メソッド
        /// </summary>
        /// <param name="excelSingleton"></param>
        /// <param name="sheetName"></param>
        /// <param name="reportInterface"></param>
        private void SetFixData2Excel(ExcelFileSingleton excelSingleton,
                                      string sheetName,
                                      CustomReportInterfaceM reportInterface)
        {
            SeisouFixedEM seisouRecord = new SeisouFixedEM();

            //タイトル
            seisouRecord.Title = reportInterface.Title;
            //承認者共通部品で承認者情報を取得する
            CustomApprovalInfo approvalInfo = ExcelComm.GetAprrovalInfo(reportInterface, context);
            //大分類承認者
            seisouRecord.MajorApprovalName = approvalInfo.MajorApprovalName;
            //施設承認者
            seisouRecord.FacilityApprovalName = approvalInfo.FacilityApprovalName;

            if (!string.IsNullOrEmpty(reportInterface.PeriodStart) && reportInterface.PeriodStart.Length == 8)
            {
                //年月
                seisouRecord.YearAndMonth = reportInterface.PeriodStart.Substring(0, 4) + "年" + reportInterface.PeriodStart.Substring(4, 2) + "月";
            }

            //日付と対応する曜日を取得する辞書型
            Dictionary<int, DayWeekName> dictDay = ExcelComm.GetDayAndWeekName(reportInterface);

            //ループによる曜日の取得
            for (int i = 1; i <= dictDay.Count; i++)
            {
                //日を設定する
                var day = typeof(SeisouFixedEM).GetProperty("Day_" + i);
                day.SetValue(seisouRecord, dictDay[i].Day);
                //曜日を設定する
                var item = typeof(SeisouFixedEM).GetProperty("WeekName_" + i);
                item.SetValue(seisouRecord, dictDay[i].WeekName);
            }

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();
            ExcelRowObject headRow = new ExcelRowObject();

            var eVtype = typeof(SeisouFixedEM);
            foreach (PropertyInfo pf in eVtype.GetProperties())
            {
                string cellValue = (string)pf.GetValue(seisouRecord);
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
        /// 明細内容設定用メソッド
        /// </summary>
        /// <param name="excelSingleton"></param>
        /// <param name="sheetName"></param>
        /// <param name="reportInterface"></param>
        private void SetDetailData2Excel(ExcelFileSingleton excelSingleton,
                                         string sheetName,
                                         CustomReportInterfaceM reportInterface)
        {            
            //中分類ごとの明細
            List<SeisouDetailEM> details = new List<SeisouDetailEM>();
            //備考の明細
            List<RemarksEM> remarks = new List<RemarksEM>();
            //中分類と備考のデータを設定する
            SetSeisouDetails(reportInterface, details, remarks);

            if (details == null)
            {
                return;
            }

            //Excel出力の開始行
            int startRowIndex = 7;

            //明細行番号を設定
            int rowIndex = startRowIndex;
            foreach (SeisouDetailEM detail in details)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }

            //備考行番号を設定
            int remarkRowIndex = 11;
            rowIndex = remarkRowIndex + details.Count - 2;
            foreach (RemarksEM detail in remarks)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }

            //明細行を挿入する
            excelSingleton.InsertRowOfSheet(sheetName, startRowIndex + 1, details.Count - 2);

            //備考行を挿入する
            excelSingleton.InsertRowOfSheet(sheetName, remarkRowIndex + details.Count - 1, remarks.Count - 2);

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();

            //中分類の明細をExcelに出力
            var eVtype = typeof(SeisouDetailEM);
            for (int i = 0; i < details.Count; i++)
            {
                ExcelRowObject row = new ExcelRowObject();
                foreach (PropertyInfo pf in eVtype.GetProperties())
                {
                    var attribute = (ExcelColPositionAttribute)pf.GetCustomAttributes(typeof(ExcelColPositionAttribute), false).FirstOrDefault();
                    if (attribute is null)
                    {
                        continue;
                    }
                    string cellValue = (string)pf.GetValue(details[i]);

                    ExcelCellObject cell = new ExcelCellObject();
                    cell.RowIndex = details[i].RowIndex;
                    cell.ColIndex = attribute.Col;
                    cell.Value = cellValue;
                    if (details[i].BackColor.HasValue)
                    {
                        cell.Color = details[i].BackColor;
                    }

                    row.Cells.Add(cell);
                }
                rows.Add(row);
            }

            //備考の明細をExcelに出力
            eVtype = typeof(RemarksEM);
            for (int i = 0; i < remarks.Count; i++)
            {
                ExcelRowObject row = new ExcelRowObject();
                foreach (PropertyInfo pf in eVtype.GetProperties())
                {
                    var attribute = (ExcelColPositionAttribute)pf.GetCustomAttributes(typeof(ExcelColPositionAttribute), false).FirstOrDefault();
                    if (attribute is null)
                    {
                        continue;
                    }
                    string cellValue = (string)pf.GetValue(remarks[i]);

                    ExcelCellObject cell = new ExcelCellObject();
                    cell.RowIndex = remarks[i].RowIndex;
                    cell.ColIndex = attribute.Col;
                    cell.Value = cellValue;

                    row.Cells.Add(cell);
                }
                rows.Add(row);
            }

            excelSingleton.WriteRowsToSheet(sheetName, rows);
        }

        /// <summary>
        /// 中分類の明細データを取得する
        /// </summary>
        /// <param name="reportInterface"></param>
        private List<CustomMiddleEM> GetSeisouDataFromDB(CustomReportInterfaceM reportInterface)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("  tbl_1.LOCATIONID AS LocationId ");
            sql.Append("  ,tbl_2.LOCATIONNAME AS MiddleName ");
            sql.Append("  ,TO_DATE(tbl_1.DATAYMD, 'yyyymmddhh24miss') AS RecordYMD ");
            sql.Append("  ,TO_DATE(tbl_1.PERIODSTART, 'yyyymmdd') AS StartDay ");
            sql.Append("  ,tbl_1.PERIOD AS Period ");
            sql.Append("  ,SUBSTR(tbl_1.DATAYMD, 9, 2)||':'||SUBSTR(tbl_1.DATAYMD, 11, 2) AS RecordTime ");
            sql.Append("  ,tbl_1.WORKERNAME AS WorkerName ");
            sql.Append("  ,tbl_4.WORKERNAME AS RecognizerName ");
            sql.Append("  ,tbl_1.QUESTION1 AS Question1 ");
            sql.Append("  ,tbl_1.QUESTION2 AS Question2 ");
            sql.Append("  ,tbl_1.QUESTION3 AS Question3 ");
            sql.Append("  ,tbl_1.QUESTION4 AS Question4 ");
            sql.Append("  ,tbl_1.QUESTION5 AS Question5 ");
            sql.Append("  ,tbl_1.QUESTION6 AS Question6 ");
            sql.Append("  ,tbl_1.QUESTION7 AS Question7 ");
            sql.Append("  ,tbl_1.QUESTION8 AS Question8 ");
            sql.Append("  ,tbl_1.QUESTION9 AS Question9 ");
            sql.Append("  ,tbl_1.QUESTION10 AS Question10 ");
            sql.Append("  ,tbl_1.QUESTION11 AS Question11 ");
            sql.Append("  ,tbl_1.QUESTION12 AS Question12 ");
            sql.Append("  ,tbl_1.QUESTION13 AS Question13 ");
            sql.Append("  ,tbl_1.QUESTION14 AS Question14 ");
            sql.Append("  ,tbl_1.QUESTION15 AS Question15 ");
            sql.Append("  ,tbl_1.QUESTION16 AS Question16 ");
            sql.Append("  ,tbl_1.QUESTION17 AS Question17 ");
            sql.Append("  ,tbl_1.QUESTION18 AS Question18 ");
            sql.Append("  ,tbl_1.QUESTION19 AS Question19 ");
            sql.Append("  ,tbl_1.QUESTION20 AS Question20 ");
            sql.Append("  ,tbl_1.RESULT1 AS Result1 ");
            sql.Append("  ,tbl_1.RESULT2 AS Result2 ");
            sql.Append("  ,tbl_1.RESULT3 AS Result3 ");
            sql.Append("  ,tbl_1.RESULT4 AS Result4 ");
            sql.Append("  ,tbl_1.RESULT5 AS Result5 ");
            sql.Append("  ,tbl_1.RESULT6 AS Result6 ");
            sql.Append("  ,tbl_1.RESULT7 AS Result7 ");
            sql.Append("  ,tbl_1.RESULT8 AS Result8 ");
            sql.Append("  ,tbl_1.RESULT9 AS Result9 ");
            sql.Append("  ,tbl_1.RESULT10 AS Result10 ");
            sql.Append("  ,tbl_1.RESULT11 AS Result11 ");
            sql.Append("  ,tbl_1.RESULT12 AS Result12 ");
            sql.Append("  ,tbl_1.RESULT13 AS Result13 ");
            sql.Append("  ,tbl_1.RESULT14 AS Result14 ");
            sql.Append("  ,tbl_1.RESULT15 AS Result15 ");
            sql.Append("  ,tbl_1.RESULT16 AS Result16 ");
            sql.Append("  ,tbl_1.RESULT17 AS Result17 ");
            sql.Append("  ,tbl_1.RESULT18 AS Result18 ");
            sql.Append("  ,tbl_1.RESULT19 AS Result19 ");
            sql.Append("  ,tbl_1.RESULT20 AS Result20 ");
            sql.Append("  ,CAST(tbl_1.REMARKSNO AS INTEGER) AS RemarksNo ");
            sql.Append(" FROM  ");
            sql.Append("  TEMPERATURECONTROL_T tbl_1 ");
            sql.Append(" INNER JOIN ( ");
            sql.Append("    SELECT ");
            sql.Append("         tbl_5.SHOPID ");
            sql.Append("         , tbl_5.CATEGORYID ");
            sql.Append("         , tbl_5.LOCATIONID ");
            sql.Append("         , tbl_5.REPORTID ");
            sql.Append("         , SUBSTR(tbl_5.DATAYMD, 1, 8) AS YMD ");
            sql.Append("         , MAX(tbl_5.DATAYMD) AS MAXDATAYMD ");
            sql.Append("    FROM ");
            sql.Append("        TEMPERATURECONTROL_T tbl_5 ");
            sql.Append("    WHERE ");
            sql.Append("        tbl_5.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("  AND tbl_5.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("  AND tbl_5.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("  AND tbl_5.PERIODSTART = '");
                sql.Append(reportInterface.PeriodStart);
                sql.Append("' ");
            }
            sql.Append(ExcelComm.GetSqlForReportInfo(reportInterface.ReportList, "tbl_5"));
            sql.Append("    GROUP BY ");
            sql.Append("         tbl_5.SHOPID");
            sql.Append("        , tbl_5.CATEGORYID ");
            sql.Append("        , tbl_5.LOCATIONID ");
            sql.Append("        , tbl_5.REPORTID ");
            sql.Append("        , tbl_5.PERIOD ");
            sql.Append("        , tbl_5.PERIODSTART ");
            sql.Append("        , SUBSTR(tbl_5.DATAYMD, 1, 8) ");
            sql.Append(" ) tbl_6  ");
            sql.Append("     ON tbl_1.SHOPID = tbl_6.SHOPID  ");
            sql.Append("     AND tbl_1.CATEGORYID = tbl_6.CATEGORYID  ");
            sql.Append("     AND tbl_1.LOCATIONID = tbl_6.LOCATIONID  ");
            sql.Append("     AND tbl_1.REPORTID = tbl_6.REPORTID  ");
            sql.Append("     AND tbl_1.DATAYMD = tbl_6.MAXDATAYMD  ");
            sql.Append("  INNER JOIN LOCATION_M tbl_2 ");
            sql.Append("    ON tbl_1.SHOPID = tbl_2.SHOPID ");
            sql.Append("    AND tbl_1.LOCATIONID = tbl_2.LOCATIONID ");
            sql.Append("  INNER JOIN MIDDLEAPPROVAL_T tbl_3 ");
            sql.Append("    ON tbl_1.SHOPID = tbl_3.SHOPID ");
            sql.Append("    AND tbl_1.CATEGORYID = tbl_3.CATEGORYID ");
            sql.Append("    AND tbl_1.LOCATIONID = tbl_3.LOCATIONID ");
            sql.Append("    AND tbl_1.REPORTID = tbl_3.REPORTID ");
            sql.Append("    AND tbl_1.APPROVALID = tbl_3.APPROVALID ");
            sql.Append("    AND tbl_3.MIDDLEGROUPNO > '0'  ");
            sql.Append("    AND tbl_3.STATUS = '1'  ");
            sql.Append("  INNER JOIN WORKER_M tbl_4 ");
            sql.Append("    ON tbl_3.SHOPID = tbl_4.SHOPID ");
            sql.Append("    AND tbl_3.MIDDLESNNUSER = tbl_4.WORKERID ");
            sql.Append(" ORDER BY tbl_1.LOCATIONID  ");

            var detailDt = context.Database.SqlQuery<CustomMiddleEM>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return null;
            }

            List<CustomMiddleEM> dbList = detailDt.ToList();

            return dbList;
        }

        /// <summary>
        /// 中分類の明細データをPDF出力用の明細データに変換する
        /// </summary>
        /// <param name="reportInterface"></param>
        /// <param name="details"></param>
        /// <param name="remarks"></param>
        private void SetSeisouDetails(CustomReportInterfaceM reportInterface,
                                     List<SeisouDetailEM> details,
                                     List<RemarksEM> remarks)
        {
            //DBから必要なデータを抽出する
            List<CustomMiddleEM> dbList = GetSeisouDataFromDB(reportInterface);

            //Dictionary<中分類, List<CustomMiddleEM>>を定義する
            Dictionary<string, List<CustomMiddleEM>> dict = new Dictionary<string, List<CustomMiddleEM>>();

            string preLocationId = "";
            foreach (CustomMiddleEM item in dbList)
            {
                //中分類が変わる。
                if (!preLocationId.Equals(item.LocationId))
                {
                    preLocationId = item.LocationId;
                    IEnumerable<CustomMiddleEM> value = dbList.Where(c => c.LocationId == preLocationId);
                    dict.Add(preLocationId, value.ToList());
                }
            }

            //日付と対応する曜日を取得する辞書型
            Dictionary<int, DateTime> dictDay = GetDictDate(reportInterface);

            //中分類ごとに、Excel出力用のデータを作成する
            foreach (KeyValuePair<string, List<CustomMiddleEM>> kvp in dict)
            {
                string locationId = kvp.Key;
                List<CustomMiddleEM> locationDetails = kvp.Value;

                //中分類の行を追加する
                SeisouDetailEM midDetail = new SeisouDetailEM();
                details.Add(midDetail);
                midDetail.CheckItem = locationDetails[0].MiddleName;
                midDetail.BackColor = Color.FromArgb(Const.BlueRgb.R, Const.BlueRgb.G, Const.BlueRgb.B);
                //記録時間の行を追加する
                SeisouDetailEM recordTimeDetail = new SeisouDetailEM();
                details.Add(recordTimeDetail);
                recordTimeDetail.CheckItem = Const.RecordTime;
                //記録時間の行のデータを設定する
                SetDetailDataByPropertyName(recordTimeDetail, locationDetails, dictDay, "RecordTime");

                //記録者の行を追加する
                SeisouDetailEM workerDetail = new SeisouDetailEM();
                details.Add(workerDetail);
                workerDetail.CheckItem = Const.WorkerName;
                SetDetailDataByPropertyName(workerDetail, locationDetails, dictDay, "WorkerName");

                //Dictionary<int, 設問の名称>
                Dictionary<int, string> dictQuestion = ExcelComm.GetQuestionDict<CustomMiddleEM>(locationDetails[0]);

                //設問ごとに行を追加する
                foreach (KeyValuePair<int, string> kvq in dictQuestion)
                {
                    SeisouDetailEM detail = new SeisouDetailEM();
                    SetDetailDataByPropertyName(detail, remarks, locationDetails, dictDay, kvq);
                    details.Add(detail);
                }

                //承認者の行を追加する
                SeisouDetailEM recognizerDetail = new SeisouDetailEM();
                details.Add(recognizerDetail);
                recognizerDetail.CheckItem = Const.RecognizerName;
                SetDetailDataByPropertyName(recognizerDetail, locationDetails, dictDay, "RecognizerName");
            }

        }

        /// <summary>
        /// 列数と日付を格納するための辞書
        /// </summary>
        /// <param name="reportInterface"></param>
        private  Dictionary<int, DateTime> GetDictDate(CustomReportInterfaceM reportInterface)
        {
            if (string.IsNullOrEmpty(reportInterface.PeriodStart)
                || reportInterface.PeriodStart.Length != 8
                || string.IsNullOrEmpty(reportInterface.Period))
            {
                return null;
            }

            if (!reportInterface.Period.Equals(Const.PeriodDay)
                && !reportInterface.Period.Equals(Const.PeriodWeek)
                && !reportInterface.Period.Equals(Const.PeriodMonth))
            {
                return null;
            }

            Nullable<DateTime> startDate = ExcelComm.GetStartDate(reportInterface);
            if (!startDate.HasValue) return null;
            Nullable<DateTime> endDate = ExcelComm.GetEndDate(reportInterface);

            //列数と日付を格納するための辞書
            Dictionary<int, DateTime> dictDay = new Dictionary<int, DateTime>();

            int key = 1;
            //日付、曜日を格納
            for (DateTime itemDay = startDate.Value; itemDay.CompareTo(endDate.Value) < 0; key++)
            {
                //辞書に追加
                dictDay.Add(key, itemDay);

                itemDay = itemDay.AddDays(1);
            }

            return dictDay;
        }

        /// <summary>
        /// 行のデータを設定する
        /// </summary>
        /// <param name="seisouDetail"></param>
        /// <param name="locationDetails"></param>
        /// <param name="dictDay"></param>
        /// <param name="propertyName"></param>
        private void SetDetailDataByPropertyName(SeisouDetailEM seisouDetail,
                                                 List<CustomMiddleEM> locationDetails,
                                                 Dictionary<int, DateTime> dictDay,
                                                 string propertyName)
        {

            //ループによる曜日の取得
            for (int i = 1; i <= dictDay.Count; i++)
            {
                DateTime colDay = dictDay[i];

                foreach (CustomMiddleEM customDetail in locationDetails)
                {
                    DateTime itemDay = customDetail.RecordYMD;
                    if (colDay.Year == itemDay.Year
                        && colDay.Month == itemDay.Month
                        && colDay.Day == itemDay.Day)
                    {
                        var itemPType = typeof(SeisouDetailEM).GetProperty("ItemsDay_" + i);
                        var customPType = typeof(CustomMiddleEM).GetProperty(propertyName);
                        itemPType.SetValue(seisouDetail, customPType.GetValue(customDetail));
                    }

                }
            }

        }


        /// <summary>
        /// 行のデータを設定する
        /// </summary>
        /// <param name="seisouDetail"></param>
        /// <param name="remarks"></param>
        /// <param name="locationDetails"></param>
        /// <param name="dictDay"></param>
        /// <param name="kvq"></param>
        private void SetDetailDataByPropertyName(SeisouDetailEM seisouDetail,
                                                 List<RemarksEM> remarks,
                                                 List<CustomMiddleEM> locationDetails,
                                                 Dictionary<int, DateTime> dictDay,
                                                 KeyValuePair<int, string> kvq)
        {
            //チェック項目の値を設定する
            seisouDetail.CheckItem = kvq.Value;

            //ループによる曜日の取得
            for (int i = 1; i <= dictDay.Count; i++)
            {
                DateTime colDay = dictDay[i];

                foreach (CustomMiddleEM customDetail in locationDetails)
                {
                    DateTime itemDay = customDetail.RecordYMD;
                    if (colDay.Year == itemDay.Year
                        && colDay.Month == itemDay.Month
                        && colDay.Day == itemDay.Day)
                    {
                        var itemPType = typeof(SeisouDetailEM).GetProperty("ItemsDay_" + i);
                        var resultType = typeof(CustomMiddleEM).GetProperty("Result" + kvq.Key);
                        
                        int remarkNo = customDetail.RemarksNo;
                        var value = resultType.GetValue(customDetail);
                        string resultValue =  value == null ? "" : value.ToString();
                        
                        if (remarkNo > 0 && kvq.Key == remarkNo) 
                        {
                            if (!string.IsNullOrEmpty(resultValue))
                            {
                                //備考がある
                                RemarksEM remark = new RemarksEM();
                                //[YYYY/MM/DD] 作業者名　[中分類名] 　設問の回答
                                remark.Remarks = itemDay.ToString("yyyy/MM/dd")
                                    + " " + customDetail.WorkerName
                                    + " " + customDetail.MiddleName
                                    + " " + resultValue;
                                remarks.Add(remark);
                            }
                        }
                        else
                        {
                            //備考がなし
                            itemPType.SetValue(seisouDetail, resultValue);
                        }                        
                    }
                }
            }
        }


    }
}

     