using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Web.Hosting;
using System.Data.Entity;
using System.Collections.Generic;
using HACCPExtender.Models;
using HACCPExtender.Models.Custom;
using HACCPExtender.Models.ExcelModel;
using HACCPExtender.Business;
using HACCPExtender.Constants;
using HACCPExtender.Controllers.Common;
using PA.Office.ExcelObjects;
using OfficePositionAttributes;

namespace HACCPExtender.ExcelOutput
{
    public class ExcelPattern_3
    {
        private MasterContext context = new MasterContext();

        /// <summary>
        /// データをPDFに変換
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns></returns>
        public bool OutPDF(CustomReportInterfaceM reportInterface)
        {
            //テンプレートファイル取得
            string templeteFileName = HostingEnvironment.MapPath("~/ExcelTemplate/pattern_3_template.xlsx");

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
                SetFixData2ExcelForPattern3(excelSingleton, sheetName, reportInterface);

                // EXCELの明細内容を設定する。
                SetDetailData2ExcelForPattern3(excelSingleton, sheetName, reportInterface);

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
        /// <param name="path">帳票ファイル格納パス</param>
        /// <param name="fileName">帳票ファイル名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void UpdateMiddleApprovalT(string path, string fileName, CustomReportInterfaceM reportInterface)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT ");
            sql.Append("    tbl_3.* ");
            sql.Append("FROM ");
            sql.Append("    TEMPERATURECONTROL_T tbl_1 ");
            sql.Append("    INNER JOIN ( ");
            sql.Append("        SELECT");
            sql.Append("            tbl_5.SHOPID");
            sql.Append("            , tbl_5.CATEGORYID");
            sql.Append("            , tbl_5.LOCATIONID");
            sql.Append("            , tbl_5.REPORTID");
            sql.Append("            , tbl_5.WORKERID");
            sql.Append("            , MAX(tbl_5.DATAYMD) AS MAXDATAYMD ");
            sql.Append("        FROM");
            sql.Append("            TEMPERATURECONTROL_T tbl_5 ");
            sql.Append("        WHERE");
            sql.Append("            tbl_5.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("            AND tbl_5.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("            AND tbl_5.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("            AND tbl_5.PERIODSTART = '");
                sql.Append(reportInterface.PeriodStart);
                sql.Append("' ");
            }
            sql.Append(ExcelComm.GetSqlForReportInfo(reportInterface.ReportList, "tbl_5"));
            sql.Append("        GROUP BY");
            sql.Append("            tbl_5.SHOPID");
            sql.Append("            , tbl_5.CATEGORYID");
            sql.Append("            , tbl_5.LOCATIONID");
            sql.Append("            , tbl_5.REPORTID");
            sql.Append("            , tbl_5.WORKERID");
            sql.Append("            , SUBSTR(tbl_5.DATAYMD, 1, 8)");
            sql.Append("    ) tbl_6 ");
            sql.Append("        ON tbl_1.SHOPID = tbl_6.SHOPID ");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_6.CATEGORYID ");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_6.LOCATIONID ");
            sql.Append("        AND tbl_1.REPORTID = tbl_6.REPORTID ");
            sql.Append("        AND tbl_1.WORKERID = tbl_6.WORKERID ");
            sql.Append("        AND tbl_1.DATAYMD = tbl_6.MAXDATAYMD ");
            sql.Append("    INNER JOIN MIDDLEAPPROVAL_T tbl_3 ");
            sql.Append("        ON tbl_1.SHOPID = tbl_3.SHOPID ");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_3.CATEGORYID ");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_3.LOCATIONID ");
            sql.Append("        AND tbl_1.REPORTID = tbl_3.REPORTID ");
            sql.Append("        AND tbl_1.APPROVALID = tbl_3.APPROVALID ");
            sql.Append("        AND tbl_3.MIDDLEGROUPNO > 0 ");
            sql.Append("        AND tbl_3.STATUS = '1'");

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
        /// 固定セル内容設定用メソッド(帳票出力パターン③)
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void SetFixData2ExcelForPattern3(ExcelFileSingleton excelSingleton,
                                                 string sheetName,
                                                 CustomReportInterfaceM reportInterface)
        {
            //承認者情報の取得
            CustomApprovalInfo approvalInfo = ExcelComm.GetAprrovalInfo(reportInterface, context);

            //固定セル内容を設定
            PersonalFixedEM fixedEM = new PersonalFixedEM
            {
                //タイトル
                Title = reportInterface.Title,
                //中分類承認者名
                MiddleApprovalName = approvalInfo.MiddleApprovalName,
                //大分類承認者
                MajorApprovalName = approvalInfo.MajorApprovalName,
                //施設承認者名
                FacilityApprovalName = approvalInfo.FacilityApprovalName
            };

            if (!string.IsNullOrEmpty(reportInterface.PeriodStart) && reportInterface.PeriodStart.Length == 8)
            {
                //記録年月日
                fixedEM.Date = reportInterface.PeriodStart.Substring(0, 4) + "年" + reportInterface.PeriodStart.Substring(4, 2) + "月" + reportInterface.PeriodStart.Substring(6, 2) + "日";
            }

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();
            ExcelRowObject headRow = new ExcelRowObject();

            var eVtype = typeof(PersonalFixedEM);
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
        /// 明細セル内容設定用メソッド(帳票出力パターン③)
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void SetDetailData2ExcelForPattern3(ExcelFileSingleton excelSingleton,
                                                    string sheetName,
                                                    CustomReportInterfaceM reportInterface)
        {
            //DBから必要なデータを抽出し、Listに格納
            var dbList = GetCustomDataFromDB(reportInterface);

            if (dbList == null)
            {
                return;
            }

            //設問の数を取得
            var dictQuestion = ExcelComm.GetQuestionDict<CustomPersonalEM>(dbList[0]);

            //ブロック数(1つの場合0)
            int blockNum = dbList.Count / 31;
            //データ数がちょうど31の倍数の場合、ブロック数を一つ減らす
            if (dbList.Count % 31 == 0)
            {
                blockNum--;
            }
            //ブロックごとに辞書型に分ける(Key:ブロック数,Value:DBから取得したデータのList)
            var dividedData = GetPersonalDataDict(dbList);

            //テンプレートをブロック数だけコピー
            for (int i = 0; i < blockNum; i++)
            {
                excelSingleton.SheetRangeCopy(sheetName, (5 + 7 * i) + ":" + (11 + 7 * i), (12 + 7 * i) + ":" + (18 + 7 * i));
            }

            //辞書型のValueをそれぞれ明細Listに変換、Excelファイルに出力
            for (int i = blockNum; i >= 0; i--)
            {

                //Excel出力開始行
                int startRowIndex = 5 + 7 * i;
                //備考出力開始行
                int remarkRowIndex = 10 + 7 * i;

                //Excelファイルに出力
                WriteToExcelSheetByBlock(excelSingleton, sheetName, dictQuestion, dividedData[i], startRowIndex, remarkRowIndex);
            }
        }

        /// <summary>
        /// DBデータを取得
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns>DBから取得したデータList</returns>
        public List<CustomPersonalEM> GetCustomDataFromDB(CustomReportInterfaceM reportInterface)
        {
            //データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("    tbl_1.WORKERNAME AS WorkerName");
            sql.Append("    , tbl_1.WORKERID AS WorkerID");
            sql.Append("    , SUBSTR(tbl_1.DATAYMD, 1, 4) || '年' || SUBSTR(tbl_1.DATAYMD, 5, 2) || '月' || SUBSTR(tbl_1.DATAYMD, 7, 2)");
            sql.Append("     || '日' AS RecordDate");
            sql.Append("    , SUBSTR(tbl_1.DATAYMD, 9, 2) || ':' || SUBSTR(tbl_1.DATAYMD, 11, 2) AS RecordTime");
            sql.Append("    , tbl_1.QUESTION1 AS Question1");
            sql.Append("    , tbl_1.QUESTION2 AS Question2");
            sql.Append("    , tbl_1.QUESTION3 AS Question3");
            sql.Append("    , tbl_1.QUESTION4 AS Question4");
            sql.Append("    , tbl_1.QUESTION5 AS Question5");
            sql.Append("    , tbl_1.QUESTION6 AS Question6");
            sql.Append("    , tbl_1.QUESTION7 AS Question7");
            sql.Append("    , tbl_1.QUESTION8 AS Question8");
            sql.Append("    , tbl_1.QUESTION9 AS Question9");
            sql.Append("    , tbl_1.QUESTION10 AS Question10");
            sql.Append("    , tbl_1.QUESTION11 AS Question11");
            sql.Append("    , tbl_1.QUESTION12 AS Question12");
            sql.Append("    , tbl_1.QUESTION13 AS Question13");
            sql.Append("    , tbl_1.QUESTION14 AS Question14");
            sql.Append("    , tbl_1.QUESTION15 AS Question15");
            sql.Append("    , tbl_1.QUESTION16 AS Question16");
            sql.Append("    , tbl_1.QUESTION17 AS Question17");
            sql.Append("    , tbl_1.QUESTION18 AS Question18");
            sql.Append("    , tbl_1.QUESTION19 AS Question19");
            sql.Append("    , tbl_1.QUESTION20 AS Question20");
            sql.Append("    , tbl_1.RESULT1 AS Result1");
            sql.Append("    , tbl_1.RESULT2 AS Result2");
            sql.Append("    , tbl_1.RESULT3 AS Result3");
            sql.Append("    , tbl_1.RESULT4 AS Result4");
            sql.Append("    , tbl_1.RESULT5 AS Result5");
            sql.Append("    , tbl_1.RESULT6 AS Result6");
            sql.Append("    , tbl_1.RESULT7 AS Result7");
            sql.Append("    , tbl_1.RESULT8 AS Result8");
            sql.Append("    , tbl_1.RESULT9 AS Result9");
            sql.Append("    , tbl_1.RESULT10 AS Result10");
            sql.Append("    , tbl_1.RESULT11 AS Result11");
            sql.Append("    , tbl_1.RESULT12 AS Result12");
            sql.Append("    , tbl_1.RESULT13 AS Result13");
            sql.Append("    , tbl_1.RESULT14 AS Result14");
            sql.Append("    , tbl_1.RESULT15 AS Result15");
            sql.Append("    , tbl_1.RESULT16 AS Result16");
            sql.Append("    , tbl_1.RESULT17 AS Result17");
            sql.Append("    , tbl_1.RESULT18 AS Result18");
            sql.Append("    , tbl_1.RESULT19 AS Result19");
            sql.Append("    , tbl_1.RESULT20 AS Result20");
            sql.Append("    , CAST(tbl_1.REMARKSNO AS INTEGER) AS RemarksNo ");
            sql.Append(" FROM ");
            sql.Append("    TEMPERATURECONTROL_T tbl_1 ");
            sql.Append("    INNER JOIN ( ");
            sql.Append("        SELECT");
            sql.Append("            tbl_5.SHOPID");
            sql.Append("            , tbl_5.CATEGORYID");
            sql.Append("            , tbl_5.LOCATIONID");
            sql.Append("            , tbl_5.REPORTID");
            sql.Append("            , tbl_5.WORKERID");
            sql.Append("            , MAX(tbl_5.DATAYMD) AS MAXDATAYMD ");
            sql.Append("        FROM");
            sql.Append("            TEMPERATURECONTROL_T tbl_5 ");
            sql.Append("        WHERE");
            sql.Append("            tbl_5.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("'");
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
            sql.Append("            , tbl_5.CATEGORYID");
            sql.Append("            , tbl_5.LOCATIONID");
            sql.Append("            , tbl_5.REPORTID");
            sql.Append("            , tbl_5.WORKERID");
            sql.Append("            , SUBSTR(tbl_5.DATAYMD, 1, 8)");
            sql.Append("    ) tbl_6 ");
            sql.Append("        ON tbl_1.SHOPID = tbl_6.SHOPID ");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_6.CATEGORYID ");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_6.LOCATIONID ");
            sql.Append("        AND tbl_1.REPORTID = tbl_6.REPORTID ");
            sql.Append("        AND tbl_1.WORKERID = tbl_6.WORKERID ");
            sql.Append("        AND tbl_1.DATAYMD = tbl_6.MAXDATAYMD ");
            sql.Append("    INNER JOIN MIDDLEAPPROVAL_T tbl_3 ");
            sql.Append("        ON tbl_1.SHOPID = tbl_3.SHOPID ");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_3.CATEGORYID ");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_3.LOCATIONID ");
            sql.Append("        AND tbl_1.REPORTID = tbl_3.REPORTID ");
            sql.Append("        AND tbl_1.APPROVALID = tbl_3.APPROVALID ");
            sql.Append("        AND tbl_3.MIDDLEGROUPNO > 0 ");
            sql.Append("        AND tbl_3.STATUS = '1'");
            sql.Append(" ORDER BY");
            sql.Append("     tbl_1.WORKERID");

            var detailDt = context.Database.SqlQuery<CustomPersonalEM>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return null;
            }

            List<CustomPersonalEM> dbList = detailDt.ToList();

            return dbList;
        }

        /// <summary>
        /// PDF出力用の明細データに変換
        /// </summary>
        /// <param name="dividedData">ブロックごとのDBデータ</param>
        /// <param name="personalDetails">中分類ごとの明細内容List</param>
        /// <param name="remarks">備考内容格納List</param>
        private void SetPersonalDetails(List<CustomPersonalEM> dividedData,
                                        List<PersonalDetailEM> personalDetails,
                                        List<RemarksEM> remarks)
        {

            var workerIDDic = WorkerIDDic(dividedData);

            //作業者名称の行を追加する
            PersonalDetailEM workerNameDetail = new PersonalDetailEM();
            workerNameDetail.CheckItem = "チェック項目";
            personalDetails.Add(workerNameDetail);
            //作業者名称の行のデータを設定する
            SetDetailDataByPropertyName(workerNameDetail, dividedData, workerIDDic, "WorkerName");

            //記録時間の行を追加する
            PersonalDetailEM recordTimeDetail = new PersonalDetailEM();
            recordTimeDetail.CheckItem = Const.RecordTime;
            personalDetails.Add(recordTimeDetail);
            //記録時間の行のデータを設定する
            SetDetailDataByPropertyName(recordTimeDetail, dividedData, workerIDDic, "RecordTime");

            //Dictionary<Question, 設問内容>
            var dictQuestion = ExcelComm.GetQuestionDict<CustomPersonalEM>(dividedData[0]);

            //設問,設問結果の行を追加する
            for (int i = 0; i < dictQuestion.Count; i++)
            {
                //設問,設問結果の行を追加する
                PersonalDetailEM questionDetail = new PersonalDetailEM();
                questionDetail.CheckItem = dictQuestion[i + 1];
                personalDetails.Add(questionDetail);
                //記録時間の行のデータを設定する
                SetDetailDataByPropertyName(questionDetail, remarks, dividedData, workerIDDic, i + 1);
            }
        }

        /// <summary>
        /// 列と作業者IDを格納する辞書
        /// </summary>
        /// <param name="dividedData">ブロックごとのDBデータ</param>
        /// <returns>辞書型(Key:列 Value:作業者ID)</returns>
        private Dictionary<int, string> WorkerIDDic(List<CustomPersonalEM> dividedData)
        {
            var workerIDDic = new Dictionary<int, string>();

            //列と作業者IDを格納
            for (int i = 0; i < dividedData.Count; i++)
            {
                workerIDDic.Add(i + 2, dividedData[i].WorkerID);
            }

            return workerIDDic;
        }

        /// <summary>
        /// 行のデータを設定
        /// </summary>
        /// <param name="detail">明細内容のオブジェクト</param>
        /// <param name="dividedData">ブロックごとのDBデータ</param>
        /// <param name="workerIDDic">作業者IDを格納した辞書</param>
        /// <param name="propertyName">プロパティ名</param>
        private void SetDetailDataByPropertyName(PersonalDetailEM detail,
                                                 List<CustomPersonalEM> dividedData,
                                                 Dictionary<int, string> workerIDDic,
                                                 string propertyName)

        {
            //ループによる値の取得
            for (int i = 0; i < workerIDDic.Count; i++)
            {
                string colID = workerIDDic[i + 2];

                foreach (CustomPersonalEM customData in dividedData)
                {
                    string workerID = customData.WorkerID;
                    if (workerID.Equals(colID))
                    {
                        var itemPType = typeof(PersonalDetailEM).GetProperty("Result_" + (i + 1));
                        var customPType = typeof(CustomPersonalEM).GetProperty(propertyName);

                        var value = customPType.GetValue(customData);
                        itemPType.SetValue(detail, value);
                    }
                }
            }
        }

        /// <summary>
        /// 行のデータを設定(設問、設問結果用)
        /// </summary>
        /// <param name="detail">明細内容のオブジェクト</param>
        /// <param name="remarks">備考内容を格納したList</param>
        /// <param name="dividedData">ブロックごとのDBデータ</param>
        /// <param name="workerIDDic">作業者IDを格納した辞書</param>
        /// <param name="questionNo">設問内容のインデックス</param>
        private void SetDetailDataByPropertyName(PersonalDetailEM detail,
                                                 List<RemarksEM> remarks,
                                                 List<CustomPersonalEM> dividedData,
                                                 Dictionary<int, string> workerIDDic,
                                                 int questionNo)
        {
            //ループによる値の取得
            foreach (CustomPersonalEM customData in dividedData)
            {
                for (int i = 0; i < workerIDDic.Count; i++)
                {
                    string colID = workerIDDic[i + 2];
                    string workerID = customData.WorkerID;
                    if (workerID.Equals(colID))
                    {
                        var itemPType = typeof(PersonalDetailEM).GetProperty("Result_" + (i + 1));
                        var resultType = typeof(CustomPersonalEM).GetProperty("Result" + (questionNo));
                        int remarkNo = customData.RemarksNo;
                        var value = resultType.GetValue(customData);
                        string resultValue = value == null ? "" : value.ToString();
                        if (remarkNo > 0 && remarkNo == questionNo)
                        {
                            //備考がある
                            RemarksEM remark = new RemarksEM();
                            //作業者名 設問の回答
                            remark.Remarks = customData.WorkerName
                                + " " + resultValue;
                            remarks.Add(remark);
                        }
                        else
                        {
                            //備考なし
                            itemPType.SetValue(detail, resultValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ブロックごとにDBのデータを格納
        /// </summary>
        /// <param name="dbList">DBから取得したデータ</param>
        /// <returns>辞書型(Key:ブロック数 Value:ブロックごとのDBデータList)</returns>
        private Dictionary<int, List<CustomPersonalEM>> GetPersonalDataDict(List<CustomPersonalEM> dbList)
        {
            //Key:ブロック数 Value:DBから取得したデータ
            Dictionary<int, List<CustomPersonalEM>> dividedData = new Dictionary<int, List<CustomPersonalEM>>();
            //DBを格納するList
            List<CustomPersonalEM> customEMs = new List<CustomPersonalEM>();
            //ブロック数
            int index = 1;

            for (int i = 0; i < dbList.Count; i++)
            {
                //各ブロックの1番目
                if (i == (index - 1) * 31)
                {
                    customEMs = new List<CustomPersonalEM>();
                }
                //各ブロックの31番目まで出力
                if (i < index * 31)
                {
                    customEMs.Add(dbList[i]);
                }
                //ブロックの最後、もしくはカスタムデータListの最後で、辞書に格納
                if (i == index * 31 - 1 || i == dbList.Count - 1)
                {
                    dividedData.Add(index - 1, customEMs);
                    index++;
                }
            }
            return dividedData;
        }

        /// <summary>
        /// Excelファイルに出力
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="dictQuestion">設問内容を格納した辞書</param>
        /// <param name="dividedData">ブロックごとのDBデータ</param>
        /// <param name="startRowIndex">出力開始行</param>
        /// <param name="remarkRowIndex">備考出力開始行</param>
        private void WriteToExcelSheetByBlock(ExcelFileSingleton excelSingleton,
                                              string sheetName,
                                              Dictionary<int, string> dictQuestion,
                                              List<CustomPersonalEM> dividedData,
                                              int startRowIndex,
                                              int remarkRowIndex)

        {
            //中分類ごとの明細List
            List<PersonalDetailEM> details = new List<PersonalDetailEM>();
            //備考の明細List
            List<RemarksEM> remarks = new List<RemarksEM>();
            //明細データに変換
            SetPersonalDetails(dividedData, details, remarks);

            if (details.Count == 0)
            {
                return;
            }
            //出力開始行
            int rowIndex = startRowIndex;

            //出力行を設定
            foreach (PersonalDetailEM detail in details)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }

            rowIndex = remarkRowIndex + dictQuestion.Count - 1;
            //備考行挿入のための変数
            int insertRow = rowIndex + 1;
            foreach (RemarksEM detail in remarks)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }

            //明細内容出力に必要な行を指定行に挿入する
            excelSingleton.InsertRowOfSheet(sheetName, startRowIndex + 2, dictQuestion.Count - 1);

            //備考出力に必要な行を指定行に挿入する
            excelSingleton.InsertRowOfSheet(sheetName, insertRow, remarks.Count - 1);

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();

            var eVtype = typeof(PersonalDetailEM);

            for (int i = 0; i < dictQuestion.Count + 2; i++)
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
    }
}
