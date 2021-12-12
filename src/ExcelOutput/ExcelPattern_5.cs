using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Hosting;
using System.Threading;
using System.Data.Entity;
using System.Collections.Generic;
using HACCPExtender.Models.ExcelModel;
using HACCPExtender.Models;
using HACCPExtender.Models.Custom;
using HACCPExtender.Constants;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Business;
using PA.Office.ExcelObjects;
using OfficePositionAttributes;

namespace HACCPExtender.ExcelOutput
{
    /// <summary>
    /// パターン⑤
    /// Author:PTJ.小嶋
    /// Create Date:2020/10/22
    /// </summary>
    public class ExcelPattern_5
    {
        private MasterContext context = new MasterContext();

        /// <summary>
        /// 中分類の明細データを取得する
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns></returns>
        public bool OutPDF(CustomReportInterfaceM reportInterface)
        {
            //テンプレートファイル取得
            string templeteFileName = HostingEnvironment.MapPath("~/ExcelTemplate/pattern_5_template.xlsx");

            //物理パスを取得する
            string path = HostingEnvironment.MapPath(reportInterface.Path + "/" + reportInterface.PeriodStart + "/");
            if (!ExcelComm.CheckDir(path))
            {
                return false;
            }
            //辞書型<Key:作業者ID,Value:作業者ごとのDBデータ>
            var workerDataDic = GetDataByWorkerID(reportInterface);

            //承認者共通部品で承認者情報を取得する
            CustomApprovalInfo approvalInfo = ExcelComm.GetAprrovalInfo(reportInterface, context);
            //承認者情報を格納したList
            var approvalList = new List<string>
            {
                //施設承認者
                approvalInfo.FacilityApprovalName,
                //大分類承認者
                approvalInfo.MajorApprovalName,
                //中分類承認者
                approvalInfo.MiddleApprovalName
            };

            //作業者ごとに帳票を出力
            foreach (KeyValuePair<string, List<CustomPersonalMonthlyEM>> kvp in workerDataDic)
            {
                //作業者ID
                var workerID = kvp.Key;
                //作業者名称
                var workerName = kvp.Value[0].WorkerName;

                //出力用ファイルのパスを取得する
                string fileName = reportInterface.ShopId + "_" +
                                  reportInterface.CategoryId + "_" +
                                  reportInterface.Title + "_" +
                                  workerID + "_" +
                                  workerName + "_" +
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
                    //EXCELの固定セルの内容を設定する。
                    SetFixData2ExcelForPattern5(excelSingleton, sheetName, reportInterface, workerName, approvalList);
                    //EXCELの明細内容を設定する。
                    SetDetailData2ExcelForPattern5(excelSingleton, sheetName, reportInterface, kvp.Value);
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
                UpdateMiddleApprovalT(filePass, fileName + ".pdf", reportInterface, workerID);
            }

            return true;
        }

        /// <summary>
        /// 帳票ファイル名と帳票ファイル格納パスを中分類承認情報に登録する
        /// </summary>
        /// <param name="path">帳票ファイル格納パス</param>
        /// <param name="fileName">帳票ファイル名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <param name="workerID">作業者ID</param>
        private void UpdateMiddleApprovalT(string path,
                                           string fileName,
                                           CustomReportInterfaceM reportInterface,
                                           string workerID)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT ");
            sql.Append("  tbl_3.* ");
            sql.Append("FROM");
            sql.Append("    TEMPERATURECONTROL_T tbl_1 ");
            sql.Append("    INNER JOIN ( ");
            sql.Append("        SELECT");
            sql.Append("            tbl_5.SHOPID");
            sql.Append("            , tbl_5.CATEGORYID");
            sql.Append("            , tbl_5.LOCATIONID");
            sql.Append("            , tbl_5.REPORTID");
            sql.Append("            , tbl_5.WORKERID");
            sql.Append("            , SUBSTR(tbl_5.DATAYMD, 1, 8) AS YMD");
            sql.Append("            , MAX(tbl_5.DATAYMD) AS MAXDATAYMD ");
            sql.Append("        FROM");
            sql.Append("            TEMPERATURECONTROL_T tbl_5 ");
            sql.Append("        WHERE");
            sql.Append("            tbl_5.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("        AND tbl_5.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("        AND tbl_5.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("        AND tbl_5.PERIODSTART = '");
                sql.Append(reportInterface.PeriodStart);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(workerID))
            {
                sql.Append("        AND tbl_5.WORKERID = '");
                sql.Append(workerID);
                sql.Append("' ");
            }
            sql.Append(ExcelComm.GetSqlForReportInfo(reportInterface.ReportList, "tbl_5"));
            sql.Append("        GROUP BY");
            sql.Append("            tbl_5.SHOPID");
            sql.Append("            , tbl_5.CATEGORYID ");
            sql.Append("            , tbl_5.LOCATIONID ");
            sql.Append("            , tbl_5.REPORTID ");
            sql.Append("            , tbl_5.PERIOD ");
            sql.Append("            , tbl_5.PERIODSTART ");
            sql.Append("            , SUBSTR(tbl_5.DATAYMD, 1, 8) ");
            sql.Append("            , tbl_5.WORKERID");
            sql.Append("    ) tbl_6  ");
            sql.Append("        ON tbl_1.SHOPID = tbl_6.SHOPID  ");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_6.CATEGORYID  ");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_6.LOCATIONID  ");
            sql.Append("        AND tbl_1.REPORTID = tbl_6.REPORTID  ");
            sql.Append("        AND tbl_1.WORKERID = tbl_6.WORKERID ");
            sql.Append("        AND tbl_1.DATAYMD = tbl_6.MAXDATAYMD  ");
            sql.Append("    INNER JOIN MIDDLEAPPROVAL_T tbl_3 ");
            sql.Append("        ON tbl_1.SHOPID = tbl_3.SHOPID ");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_3.CATEGORYID ");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_3.LOCATIONID ");
            sql.Append("        AND tbl_1.REPORTID = tbl_3.REPORTID ");
            sql.Append("        AND tbl_1.APPROVALID = tbl_3.APPROVALID ");
            sql.Append("        AND tbl_3.MIDDLEGROUPNO > '0'  ");
            sql.Append("        AND tbl_3.STATUS = '1'  ");

            var detailDt = context.Database.SqlQuery<MiddleApprovalT>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return;
            }

            List<MiddleApprovalT> dbList = detailDt.ToList();

            foreach (MiddleApprovalT detail in dbList)
            {
                //更新ユーザーID
                detail.UPDUSERID = reportInterface.ManageId;
                //帳票ファイル名
                detail.REPORTFILENAME = fileName;
                //帳票ファイル格納パス
                detail.REPORTFILEPASS = path;
            }

            using (MasterContext contextUpd = new MasterContext())
            {
                using (var tran = contextUpd.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (MiddleApprovalT upddata in dbList)
                        {
                            contextUpd.MiddleApprovalTs.Attach(upddata);
                            contextUpd.Entry(upddata).State = EntityState.Modified;
                        }
                        contextUpd.SaveChanges();

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
        /// 固定セル内容設定用メソッド(帳票出力パターン⑤)
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <param name="workerName">作業者ID</param>
        /// <param name="approvalList">承認者情報を格納したList 
        /// [0]施設承認者名[1]大分類承認者[2]中分類承認者</param>
        private void SetFixData2ExcelForPattern5(ExcelFileSingleton excelSingleton,
                                                 string sheetName,
                                                 CustomReportInterfaceM reportInterface,
                                                 string workerName,
                                                 List<string> approvalList)
        {
            PersonalMonthlyFixedEM fixedEM = new PersonalMonthlyFixedEM
            {
                //タイトル
                Title = reportInterface.Title,
                //作業者
                WorkerName = "作業者:" + workerName,
                //施設承認者
                FacilityApprovalName = approvalList[0],
                //大分類承認者
                MajorApprovalName = approvalList[1],
                //中分類承認者
                MiddleApprovalName = approvalList[2]
            };

            if (!string.IsNullOrEmpty(reportInterface.PeriodStart) && reportInterface.PeriodStart.Length == 8)
            {
                //記録年月
                fixedEM.RecordYM = reportInterface.PeriodStart.Substring(0, 4) + "年" + reportInterface.PeriodStart.Substring(4, 2) + "月";
            }

            //日付と対応する曜日を取得する辞書型
            Dictionary<int, DayWeekName> dictDay = ExcelComm.GetDayAndWeekName(reportInterface);

            //ループによる曜日の取得
            for (int i = 1; i <= dictDay.Count; i++)
            {
                //日を設定する
                var day = typeof(PersonalMonthlyFixedEM).GetProperty("Day" + i);
                day.SetValue(fixedEM, dictDay[i].Day);
                //曜日を設定する
                var item = typeof(PersonalMonthlyFixedEM).GetProperty("WeekName" + i);
                item.SetValue(fixedEM, dictDay[i].WeekName);
            }

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();
            ExcelRowObject headRow = new ExcelRowObject();

            var eVtype = typeof(PersonalMonthlyFixedEM);
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
        /// 明細セル内容設定用メソッド(帳票出力パターン⑤)
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <param name="dividedData">作業者ごとに分けたDBデータ</param>
        private void SetDetailData2ExcelForPattern5(ExcelFileSingleton excelSingleton,
                                                    string sheetName,
                                                    CustomReportInterfaceM reportInterface,
                                                    List<CustomPersonalMonthlyEM> dividedData)
        {
            //作業者ごとの明細セル内容List
            List<PersonalMonthlyDetailEM> details = new List<PersonalMonthlyDetailEM>();
            //備考List
            List<RemarksEM> remarks = new List<RemarksEM>();
            //DBデータをPDF出力用データに変更
            SetDetailData(reportInterface, details, remarks, dividedData);

            if (details == null)
            {
                return;
            }

            //Excel出力の開始行
            int startRowIndex = 7;

            //明細行番号を設定
            int rowIndex = startRowIndex;
            foreach (PersonalMonthlyDetailEM detail in details)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }

            //備考行番号を設定
            int remarkRowIndex = 11;
            rowIndex = remarkRowIndex + details.Count - 2;
            //備考行挿入のための変数
            int insertRow = rowIndex + 1;

            foreach (RemarksEM detail in remarks)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }

            //明細行を挿入する
            excelSingleton.InsertRowOfSheet(sheetName, startRowIndex + 1, details.Count - 2);

            //備考行を挿入する
            excelSingleton.InsertRowOfSheet(sheetName, insertRow, remarks.Count - 2);

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();

            //明細内容をExcelに出力
            var eVtype = typeof(PersonalMonthlyDetailEM);
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
        /// DBデータをPDF出力用データに変更
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <param name="details">明細内容List</param>
        /// <param name="remarks">備考List</param>
        /// <param name="dividedData">作業者ごとに分けたDBデータ</param>
        private void SetDetailData(CustomReportInterfaceM reportInterface,
                                   List<PersonalMonthlyDetailEM> details,
                                   List<RemarksEM> remarks,
                                   List<CustomPersonalMonthlyEM> dividedData)
        {
            //日付と対応する曜日を取得する辞書型
            Dictionary<int, DateTime> dictDay = GetDictDate(reportInterface);

            //記録時間の行を追加
            PersonalMonthlyDetailEM recordTimeDetail = new PersonalMonthlyDetailEM();
            details.Add(recordTimeDetail);
            recordTimeDetail.CheckItem = Const.RecordTime;
            //記録時間の行のデータを設定する
            SetDetailDataByPropertyName(recordTimeDetail, dividedData, dictDay, "RecordTime");

            //設問を格納した辞書
            Dictionary<int, string> dictQuestion = ExcelComm.GetQuestionDict<CustomPersonalMonthlyEM>(dividedData[0]);

            //設問ごとに行を追加する
            foreach (KeyValuePair<int, string> kvq in dictQuestion)
            {
                PersonalMonthlyDetailEM detail = new PersonalMonthlyDetailEM();
                SetDetailDataByPropertyName(detail, remarks, dividedData, dictDay, kvq);
                details.Add(detail);
            }
        }

        /// <summary>
        /// 行のデータを設定
        /// </summary>
        /// <param name="detail">明細内容オブジェクト</param>
        /// <param name="dividedData">作業者ごとに分けたDBデータ</param>
        /// <param name="dictDay">日付と曜日を格納したDictionary</param>
        /// <param name="propertyName">プロパティ名</param>
        private void SetDetailDataByPropertyName(PersonalMonthlyDetailEM detail,
                                                 List<CustomPersonalMonthlyEM> dividedData,
                                                 Dictionary<int, DateTime> dictDay,
                                                 string propertyName)
        {
            //ループによる曜日の取得
            for (int i = 1; i <= dictDay.Count; i++)
            {
                DateTime colDay = dictDay[i];

                foreach (CustomPersonalMonthlyEM customDetail in dividedData)
                {
                    DateTime itemDay = customDetail.RecordYMD;
                    if (colDay.Year == itemDay.Year
                        && colDay.Month == itemDay.Month
                        && colDay.Day == itemDay.Day)
                    {
                        var itemPType = typeof(PersonalMonthlyDetailEM).GetProperty("ItemsDay" + i);
                        var customPType = typeof(CustomPersonalMonthlyEM).GetProperty(propertyName);
                        itemPType.SetValue(detail, customPType.GetValue(customDetail));
                    }
                }
            }
        }

        /// <summary>
        /// 行のデータを設定する
        /// </summary>
        /// <param name="detail">明細内容オブジェクト</param>
        /// <param name="remarks">備考List</param>
        /// <param name="dividedData">作業者ごとに分けたDBデータ</param>
        /// <param name="dictDay">日付と曜日を格納したDictionary</param>
        /// <param name="kvq">設問内容が格納されたDictionary</param>
        private void SetDetailDataByPropertyName(PersonalMonthlyDetailEM detail,
                                                 List<RemarksEM> remarks,
                                                 List<CustomPersonalMonthlyEM> dividedData,
                                                 Dictionary<int, DateTime> dictDay,
                                                 KeyValuePair<int, string> kvq)
        {
            //チェック項目の値を設定する
            detail.CheckItem = kvq.Value;

            //ループによる曜日の取得
            for (int i = 1; i <= dictDay.Count; i++)
            {
                DateTime colDay = dictDay[i];

                foreach (CustomPersonalMonthlyEM customDetail in dividedData)
                {
                    DateTime itemDay = customDetail.RecordYMD;
                    if (colDay.Year == itemDay.Year
                        && colDay.Month == itemDay.Month
                        && colDay.Day == itemDay.Day)
                    {
                        var itemPType = typeof(PersonalMonthlyDetailEM).GetProperty("ItemsDay" + i);
                        var resultType = typeof(CustomPersonalMonthlyEM).GetProperty("Result" + kvq.Key);

                        int remarkNo = customDetail.RemarksNo;
                        var value = resultType.GetValue(customDetail);
                        string resultValue = value == null ? "" : value.ToString();

                        if (remarkNo > 0 && kvq.Key == remarkNo)
                        {
                            if (!string.IsNullOrEmpty(resultValue))
                            {
                                //備考がある
                                RemarksEM remark = new RemarksEM();
                                //[YYYY/MM/DD] 設問の回答
                                remark.Remarks = itemDay.ToString("yyyy/MM/dd")
                                    + " " + resultValue;
                                remarks.Add(remark);
                            }
                        }
                        else
                        {
                            //備考がなし
                            itemPType.SetValue(detail, resultValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 作業者IDと関連するDB情報を格納するDictionary
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns>辞書型<Key:作業者ID,Value:作業者に関するDB情報></returns>
        private Dictionary<string, List<CustomPersonalMonthlyEM>> GetDataByWorkerID(CustomReportInterfaceM reportInterface)
        {
            //DBからデータを取得
            var dbList = GetCustomDataFromDB(reportInterface);

            //辞書型<Key:作業者ID,Value:作業者ごとのDBデータ>
            var workerDataDic = new Dictionary<string, List<CustomPersonalMonthlyEM>>();

            string preWorkerID = string.Empty;
            foreach (CustomPersonalMonthlyEM item in dbList)
            {
                //作業者IDを基準に辞書に分ける
                if (!preWorkerID.Equals(item.WorkerID))
                {
                    preWorkerID = item.WorkerID;
                    IEnumerable<CustomPersonalMonthlyEM> value = dbList.Where(c => c.WorkerID == preWorkerID);
                    workerDataDic.Add(preWorkerID, value.ToList());
                }
            }

            return workerDataDic;
        }

        /// <summary>
        /// DBデータを取得
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns>DBから取得したデータList</returns>
        private List<CustomPersonalMonthlyEM> GetCustomDataFromDB(CustomReportInterfaceM reportInterface)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT");
            sql.Append("    tbl_1.WORKERNAME AS WorkerName");
            sql.Append("    , tbl_1.WORKERID AS WorkerID");
            sql.Append("    , TO_DATE(tbl_1.DATAYMD, 'yyyymmddhh24miss') AS RecordYMD");
            sql.Append("    , TO_DATE(tbl_1.PERIODSTART, 'yyyymmdd') AS StartDay");
            sql.Append("    , SUBSTR(tbl_1.DATAYMD, 9, 2) || ':' || SUBSTR(tbl_1.DATAYMD, 11, 2) AS RecordTime");
            sql.Append("    ,tbl_1.QUESTION1 AS Question1 ");
            sql.Append("    ,tbl_1.QUESTION2 AS Question2 ");
            sql.Append("    ,tbl_1.QUESTION3 AS Question3 ");
            sql.Append("    ,tbl_1.QUESTION4 AS Question4 ");
            sql.Append("    ,tbl_1.QUESTION5 AS Question5 ");
            sql.Append("    ,tbl_1.QUESTION6 AS Question6 ");
            sql.Append("    ,tbl_1.QUESTION7 AS Question7 ");
            sql.Append("    ,tbl_1.QUESTION8 AS Question8 ");
            sql.Append("    ,tbl_1.QUESTION9 AS Question9 ");
            sql.Append("    ,tbl_1.QUESTION10 AS Question10 ");
            sql.Append("    ,tbl_1.QUESTION11 AS Question11 ");
            sql.Append("    ,tbl_1.QUESTION12 AS Question12 ");
            sql.Append("    ,tbl_1.QUESTION13 AS Question13 ");
            sql.Append("    ,tbl_1.QUESTION14 AS Question14 ");
            sql.Append("    ,tbl_1.QUESTION15 AS Question15 ");
            sql.Append("    ,tbl_1.QUESTION16 AS Question16 ");
            sql.Append("    ,tbl_1.QUESTION17 AS Question17 ");
            sql.Append("    ,tbl_1.QUESTION18 AS Question18 ");
            sql.Append("    ,tbl_1.QUESTION19 AS Question19 ");
            sql.Append("    ,tbl_1.QUESTION20 AS Question20 ");
            sql.Append("    ,tbl_1.RESULT1 AS Result1 ");
            sql.Append("    ,tbl_1.RESULT2 AS Result2 ");
            sql.Append("    ,tbl_1.RESULT3 AS Result3 ");
            sql.Append("    ,tbl_1.RESULT4 AS Result4 ");
            sql.Append("    ,tbl_1.RESULT5 AS Result5 ");
            sql.Append("    ,tbl_1.RESULT6 AS Result6 ");
            sql.Append("    ,tbl_1.RESULT7 AS Result7 ");
            sql.Append("    ,tbl_1.RESULT8 AS Result8 ");
            sql.Append("    ,tbl_1.RESULT9 AS Result9 ");
            sql.Append("    ,tbl_1.RESULT10 AS Result10 ");
            sql.Append("    ,tbl_1.RESULT11 AS Result11 ");
            sql.Append("    ,tbl_1.RESULT12 AS Result12 ");
            sql.Append("    ,tbl_1.RESULT13 AS Result13 ");
            sql.Append("    ,tbl_1.RESULT14 AS Result14 ");
            sql.Append("    ,tbl_1.RESULT15 AS Result15 ");
            sql.Append("    ,tbl_1.RESULT16 AS Result16 ");
            sql.Append("    ,tbl_1.RESULT17 AS Result17 ");
            sql.Append("    ,tbl_1.RESULT18 AS Result18 ");
            sql.Append("    ,tbl_1.RESULT19 AS Result19 ");
            sql.Append("    ,tbl_1.RESULT20 AS Result20 ");
            sql.Append("    ,CAST(tbl_1.REMARKSNO AS INTEGER) AS RemarksNo ");
            sql.Append("FROM");
            sql.Append("    TEMPERATURECONTROL_T tbl_1 ");
            sql.Append("    INNER JOIN ( ");
            sql.Append("        SELECT");
            sql.Append("            tbl_5.SHOPID");
            sql.Append("            , tbl_5.CATEGORYID");
            sql.Append("            , tbl_5.LOCATIONID");
            sql.Append("            , tbl_5.REPORTID");
            sql.Append("            , tbl_5.WORKERID");
            sql.Append("            , SUBSTR(tbl_5.DATAYMD, 1, 8) AS YMD");
            sql.Append("            , MAX(tbl_5.DATAYMD) AS MAXDATAYMD ");
            sql.Append("        FROM");
            sql.Append("            TEMPERATURECONTROL_T tbl_5 ");
            sql.Append("        WHERE");
            sql.Append("            tbl_5.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("        AND tbl_5.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("        AND tbl_5.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("        AND tbl_5.PERIODSTART = '");
                sql.Append(reportInterface.PeriodStart);
                sql.Append("' ");
            }
            sql.Append(ExcelComm.GetSqlForReportInfo(reportInterface.ReportList, "tbl_5"));
            sql.Append("        GROUP BY");
            sql.Append("            tbl_5.SHOPID");
            sql.Append("            , tbl_5.CATEGORYID ");
            sql.Append("            , tbl_5.LOCATIONID ");
            sql.Append("            , tbl_5.REPORTID ");
            sql.Append("            , tbl_5.PERIOD ");
            sql.Append("            , tbl_5.PERIODSTART ");
            sql.Append("            , SUBSTR(tbl_5.DATAYMD, 1, 8) ");
            sql.Append("            , tbl_5.WORKERID");
            sql.Append("    ) tbl_6  ");
            sql.Append("        ON tbl_1.SHOPID = tbl_6.SHOPID  ");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_6.CATEGORYID  ");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_6.LOCATIONID  ");
            sql.Append("        AND tbl_1.REPORTID = tbl_6.REPORTID  ");
            sql.Append("        AND tbl_1.WORKERID = tbl_6.WORKERID ");
            sql.Append("        AND tbl_1.DATAYMD = tbl_6.MAXDATAYMD  ");
            sql.Append("    INNER JOIN MIDDLEAPPROVAL_T tbl_3 ");
            sql.Append("        ON tbl_1.SHOPID = tbl_3.SHOPID ");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_3.CATEGORYID ");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_3.LOCATIONID ");
            sql.Append("        AND tbl_1.REPORTID = tbl_3.REPORTID ");
            sql.Append("        AND tbl_1.APPROVALID = tbl_3.APPROVALID ");
            sql.Append("        AND tbl_3.MIDDLEGROUPNO > '0'  ");
            sql.Append("        AND tbl_3.STATUS = '1'  ");
            sql.Append(" ORDER BY ");
            sql.Append("     tbl_1.WORKERID ");
            sql.Append("     ,tbl_1.DATAYMD");

            var detailDt = context.Database.SqlQuery<CustomPersonalMonthlyEM>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return null;
            }

            List<CustomPersonalMonthlyEM> dbList = detailDt.ToList();

            return dbList;
        }

        /// <summary>
        /// 列数と日付を格納するための辞書
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns>Dictionary<Key:列数,Value:日付></returns>
        private Dictionary<int, DateTime> GetDictDate(CustomReportInterfaceM reportInterface)
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
    }
}

