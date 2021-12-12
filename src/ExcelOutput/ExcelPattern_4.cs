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
using HACCPExtender.Business;
using HACCPExtender.Constants;
using HACCPExtender.Controllers.Common;
using PA.Office.ExcelObjects;
using OfficePositionAttributes;

namespace HACCPExtender.ExcelOutput
{
    public class ExcelPattern_4
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
            string templeteFileName = HostingEnvironment.MapPath("~/ExcelTemplate/pattern_4_template.xlsx");

            //物理パスを取得する
            string path = HostingEnvironment.MapPath(reportInterface.Path + "/" + reportInterface.PeriodStart + "/");
            if (!ExcelComm.CheckDir(path))
            {
                return false;
            }

            //出力用ファイルのパスを取得する
            string fileName = reportInterface.ShopId + "_" +
                              reportInterface.CategoryId + "_" +
                              reportInterface.ReportList[0].LocationId + "_" +
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
                //Excelファイルを読み込む。
                excelSingleton.OpenExcel(excelFileName);

                //EXCELの固定セルの内容を設定する。
                SetFixData2ExcelForPattern4(excelSingleton, sheetName, reportInterface);

                //EXCELの明細内容を設定する。
                SetDetailData2ExcelForPattern4(excelSingleton, sheetName, reportInterface);
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
        /// <param name="fileName">ファイル名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void UpdateMiddleApprovalT(string path, string fileName, CustomReportInterfaceM reportInterface)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT ");
            sql.Append("    tbl_3.* ");
            sql.Append("FROM ");
            sql.Append("    TEMPERATURECONTROL_T tbl_1");
            sql.Append("    INNER JOIN LOCATION_M tbl_2");
            sql.Append("        ON tbl_1.SHOPID = tbl_2.SHOPID");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_2.LOCATIONID");
            sql.Append("    INNER JOIN MIDDLEAPPROVAL_T tbl_3");
            sql.Append("        ON tbl_1.SHOPID = tbl_3.SHOPID");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_3.CATEGORYID");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_3.LOCATIONID");
            sql.Append("        AND tbl_1.REPORTID = tbl_3.REPORTID");
            sql.Append("        AND tbl_1.APPROVALID = tbl_3.APPROVALID");
            sql.Append("        AND tbl_3.MIDDLEGROUPNO > '0'");
            sql.Append("        AND tbl_3.STATUS = '1'");
            sql.Append("WHERE ");
            sql.Append("    tbl_1.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("AND tbl_1.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("AND tbl_1.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("AND tbl_1.PERIODSTART = '");
                sql.Append(reportInterface.PeriodStart);
                sql.Append("' ");
            }
            sql.Append(ExcelComm.GetSqlForReportInfo(reportInterface.ReportList, "tbl_1"));

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
        /// 固定セル内容設定用メソッド(帳票出力パターン④)
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void SetFixData2ExcelForPattern4(ExcelFileSingleton excelSingleton,
                                                 string sheetName,
                                                 CustomReportInterfaceM reportInterface)

        {
            //承認者名の取得
            CustomApprovalInfo approvalInfo = ExcelComm.GetAprrovalInfo(reportInterface, context);
            //固定内容用モデル
            FoodSafetyFixedEM fixedEM = new FoodSafetyFixedEM
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

            var eVtype = typeof(FoodSafetyFixedEM);
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
        /// 明細セル内容設定用メソッド(帳票出力パターン④)
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void SetDetailData2ExcelForPattern4(ExcelFileSingleton excelSingleton,
                                                    string sheetName,
                                                    CustomReportInterfaceM reportInterface)
        {
            //DBから必要なデータを抽出する
            var dbList = GetCustomDataFromDB(reportInterface);

            if (dbList == null)
            {
                return;
            }

            //中分類ごとにDBデータを辞書型に分ける(Key:中分類名称,Value:Keyに対応するデータのList)
            var dividedDataDic = GetFoodSafetyDataDict(dbList);

            //ブロック数(=出力する中分類の数)
            int blockIndex = dividedDataDic.Count;

            //備考欄の内容を格納するList
            List<RemarksEM> remarks = new List<RemarksEM>();

            //DBから取得したデータを明細データに変換(Key:中分類名称,Value:Keyに対応する明細内容List)
            var detailDic = SetDetailDataByMiddleName(dividedDataDic, remarks);

            if (remarks.Count != 0)
            {
                //備考のみ先に出力
                WriteRemarksToExcelSheet(remarks, excelSingleton, sheetName);
            }

            //中分類が2つ以上の場合、テンプレートをコピー
            for (int i = 0; i < blockIndex - 1; i++)
            {
                //コピーに必要な行を指定行に挿入する
                excelSingleton.InsertRowOfSheet(sheetName, 8 + (3 * i), 3);

                excelSingleton.SheetRangeCopy(sheetName, (5 + 3 * i) + ":" + (7 + 3 * i), (8 + 3 * i) + ":" + (10 + 3 * i));
            }


            //場所名称を格納したList(Excel出力の際に使用)
            var keyList = new List<string>(detailDic.Keys);

            //中分類ごとに明細内容をExcelファイルに出力
            for (int i = keyList.Count - 1; i >= 0; i--)
            {
                //Excel出力開始行
                int startRowIndex = 5 + 3 * i;

                //設問の数を取得
                Dictionary<int, string> dictQuestion = ExcelComm.GetQuestionDict<CustomFoodSafetyEM>(dividedDataDic[keyList[i]][0]);

                //Excelファイルに出力
                WriteToExcelSheetByBlock(excelSingleton, sheetName, detailDic[keyList[i]], dictQuestion, startRowIndex);
            }
        }

        /// <summary>
        /// DBデータを取得
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns>DBから取得したデータのList</returns>
        private List<CustomFoodSafetyEM> GetCustomDataFromDB(CustomReportInterfaceM reportInterface)
        {
            //データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("    tbl_2.LOCATIONNAME AS MiddleName");
            sql.Append("    , tbl_2.LOCATIONID AS LocationID");
            sql.Append("    , TO_DATE(tbl_1.DATAYMD, 'yyyymmddhh24miss') AS RecordYMD");
            sql.Append("    , TO_DATE(tbl_1.PERIODSTART, 'yyyymmdd') AS StartDay");
            sql.Append("    , SUBSTR(tbl_1.DATAYMD, 9, 2) || ':' || SUBSTR(tbl_1.DATAYMD, 11, 2) AS RecordTime");
            sql.Append("    , tbl_1.WORKERNAME AS WorkerName");
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
            sql.Append("FROM ");
            sql.Append("    TEMPERATURECONTROL_T tbl_1");
            sql.Append("    INNER JOIN LOCATION_M tbl_2");
            sql.Append("        ON tbl_1.SHOPID = tbl_2.SHOPID");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_2.LOCATIONID");
            sql.Append("    INNER JOIN MIDDLEAPPROVAL_T tbl_3");
            sql.Append("        ON tbl_1.SHOPID = tbl_3.SHOPID");
            sql.Append("        AND tbl_1.CATEGORYID = tbl_3.CATEGORYID");
            sql.Append("        AND tbl_1.LOCATIONID = tbl_3.LOCATIONID");
            sql.Append("        AND tbl_1.REPORTID = tbl_3.REPORTID");
            sql.Append("        AND tbl_1.APPROVALID = tbl_3.APPROVALID");
            sql.Append("        AND tbl_3.MIDDLEGROUPNO > '0'");
            sql.Append("        AND tbl_3.STATUS = '1'");
            sql.Append("WHERE ");
            sql.Append("    tbl_1.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("AND tbl_1.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("AND tbl_1.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("AND tbl_1.PERIODSTART = '");
                sql.Append(reportInterface.PeriodStart);
                sql.Append("' ");
            }
            sql.Append(ExcelComm.GetSqlForReportInfo(reportInterface.ReportList, "tbl_1"));
            sql.Append("ORDER BY ");
            sql.Append("    tbl_1.LOCATIONID");
            sql.Append("    , tbl_1.DATAYMD");

            var detailDt = context.Database.SqlQuery<CustomFoodSafetyEM>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return null;
            }

            List<CustomFoodSafetyEM> dbList = detailDt.ToList();

            return dbList;
        }

        /// <summary>
        /// 中分類ごとにDBのデータを格納
        /// </summary>
        /// <param name="dbList">DBから取得したデータ</param>
        /// <returns>辞書型(Key:中分類名称 Value:Keyに対応するデータのList)</returns>
        private Dictionary<string, List<CustomFoodSafetyEM>> GetFoodSafetyDataDict(List<CustomFoodSafetyEM> dbList)
        {
            //Key:中分類名称 Value:中分類ごとに分けたDBから取得したデータ
            var dividedDataDic = new Dictionary<string, List<CustomFoodSafetyEM>>();

            string preMIddleID = string.Empty;
            foreach (CustomFoodSafetyEM item in dbList)
            {
                //中分類IDが一致した場合、辞書に値を追加
                if (!preMIddleID.Equals(item.LocationID))
                {
                    preMIddleID = item.LocationID;
                    IEnumerable<CustomFoodSafetyEM> value = dbList.Where(c => c.LocationID == preMIddleID);
                    dividedDataDic.Add(item.MiddleName, value.ToList());
                }
            }
            return dividedDataDic;
        }

        /// <summary>
        /// 中分類ごとのDBデータををPDF出力用の明細データに変換
        /// </summary>
        /// <param name="dividedDataDic">中分類ごとにDBデータを分けた辞書</param>
        /// <param name="remarks">備考出力用のList</param>
        /// <returns>辞書型(Key:中分類名称 Value:Keyに対応する明細内容List)</returns>
        private Dictionary<string, List<FoodSafetyDetailEM>> SetDetailDataByMiddleName(Dictionary<string, List<CustomFoodSafetyEM>> dividedDataDic,
                                                                                       List<RemarksEM> remarks)
        {
            //辞書(Key:中分類名称 Value:明細内容リスト)
            var detailDic = new Dictionary<string, List<FoodSafetyDetailEM>>();

            //中分類ごとに、Excel出力用の明細内容データを作成する
            foreach (KeyValuePair<string, List<CustomFoodSafetyEM>> kvp in dividedDataDic)
            {
                List<FoodSafetyDetailEM> details = new List<FoodSafetyDetailEM>();
                //場所名称
                var locationName = kvp.Key;
                //中分類ごとのDBデータ
                var dividedData = kvp.Value;

                //列と対応する記録時間を取得する辞書型
                var recTimeDict = GetRecordTimeDict(dividedData);

                //中分類名称の行を追加する
                FoodSafetyDetailEM locationNameDetail = new FoodSafetyDetailEM();
                locationNameDetail.CheckItem = locationName;
                //24回分のデータを追加
                for (int i = 1; i <= 24; i++)
                {
                    var itemPType = typeof(FoodSafetyDetailEM).GetProperty("No_" + i);
                    itemPType.SetValue(locationNameDetail, i + "回目");
                }
                details.Add(locationNameDetail);

                //記録時間の行を追加する
                FoodSafetyDetailEM recordTimeDetail = new FoodSafetyDetailEM();
                recordTimeDetail.CheckItem = Const.RecordTime;
                details.Add(recordTimeDetail);
                //記録時間の行のデータを設定する
                SetDetailDataByPropertyName(recordTimeDetail, dividedData, recTimeDict, "RecordTime");

                //記録者の行を追加する
                FoodSafetyDetailEM workerNameDetail = new FoodSafetyDetailEM();
                workerNameDetail.CheckItem = Const.WorkerName;
                details.Add(workerNameDetail);
                //記録者の行のデータを設定する
                SetDetailDataByPropertyName(workerNameDetail, dividedData, recTimeDict, "WorkerName");

                //Dictionary<int, 設問の名称>
                var dictQuestion = ExcelComm.GetQuestionDict<CustomFoodSafetyEM>(dividedData[0]);

                //設問ごとに行を追加する
                foreach (KeyValuePair<int, string> kvq in dictQuestion)
                {
                    FoodSafetyDetailEM safeDetail = new FoodSafetyDetailEM();
                    SetDetailDataByPropertyName(safeDetail, remarks, dividedData, recTimeDict, kvq);
                    details.Add(safeDetail);
                }
                detailDic.Add(locationName, details);
            }
            return detailDic;
        }

        /// <summary>
        /// 行のデータを設定
        /// </summary>
        /// <param name="detail">明細内容のオブジェクト</param>
        /// <param name="dividedData">中分類ごとのDBデータ</param>
        /// <param name="recTimeDict">列と対応する記録時間を格納した辞書</param>
        /// <param name="propertyName">プロパティ名</param>
        private void SetDetailDataByPropertyName(FoodSafetyDetailEM detail,
                                                 List<CustomFoodSafetyEM> dividedData,
                                                 Dictionary<int, string> recTimeDict,
                                                 string propertyName)
        {
            //ループによるデータの設定
            for (int i = 0; i < recTimeDict.Count; i++)
            {
                string colTime = recTimeDict[i];

                foreach (CustomFoodSafetyEM customData in dividedData)
                {
                    //記録時間が辞書型の記録時間と一致した場合、値を設定
                    string recTime = customData.RecordTime;
                    if (colTime.Equals(recTime))
                    {
                        var itemPType = typeof(FoodSafetyDetailEM).GetProperty("No_" + (i + 1));
                        var customPType = typeof(CustomFoodSafetyEM).GetProperty(propertyName);
                        itemPType.SetValue(detail, customPType.GetValue(customData));
                    }
                }
            }
        }
        /// <summary>
        /// 行のデータを設定(設問、設問結果用)
        /// </summary>
        /// <param name="detail">明細内容のオブジェクト</param>
        /// <param name="remarks">備考出力用List</param>
        /// <param name="dividedData">中分類ごとのDBデータ</param>
        /// <param name="recTimeDict">列と対応する記録時間を格納した辞書</param>
        /// <param name="kvq">Dictionary<int, 設問の名称></param>
        private void SetDetailDataByPropertyName(FoodSafetyDetailEM detail,
                                                 List<RemarksEM> remarks,
                                                 List<CustomFoodSafetyEM> dividedData,
                                                 Dictionary<int, string> recTimeDict,
                                                 KeyValuePair<int, string> kvq)
        {
            //チェック項目の値を設定する
            detail.CheckItem = kvq.Value;

            //ループによる設問結果、備考の設定
            for (int i = 0; i < recTimeDict.Count; i++)
            {
                string colTime = recTimeDict[i];

                foreach (CustomFoodSafetyEM customData in dividedData)
                {
                    //記録時間が辞書型の記録時間と一致した場合、値を設定
                    string recTime = customData.RecordTime;
                    if (colTime.Equals(recTime))
                    {
                        var itemPType = typeof(FoodSafetyDetailEM).GetProperty("No_" + (i + 1));
                        var resultType = typeof(CustomFoodSafetyEM).GetProperty("Result" + kvq.Key);
                        int remarkNo = customData.RemarksNo;
                        var value = resultType.GetValue(customData);
                        string resultValue = value == null ? "" : value.ToString();
                        if (remarkNo > 0 && kvq.Key == remarkNo)
                        {
                            //備考がある
                            RemarksEM remark = new RemarksEM();
                            //作業者名　[中分類名] 　設問の回答
                            remark.Remarks = customData.WorkerName
                                             + " " + customData.MiddleName
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
        /// 列と記録時間を格納する辞書
        /// </summary>
        /// <param name="dividedData">中分類ごとのDBデータList</param>
        /// <returns>辞書型(Key:列 Value:記録時間)</returns>
        private Dictionary<int, string> GetRecordTimeDict(List<CustomFoodSafetyEM> dividedData)
        {
            var recordTimeDict = new Dictionary<int, string>();
            for (int i = 0; i < 24; i++)
            {
                if (i == dividedData.Count)
                {
                    break;
                }
                recordTimeDict.Add(i, dividedData[i].RecordTime);
            }
            return recordTimeDict;
        }

        /// <summary>
        /// 明細内容をExcelファイルに出力
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="details">中分類ごとの明細内容List</param>
        /// <param name="dictQuestion">Dictionary<int,設問内容></param>
        /// <param name="startRowIndex">Excel出力開始行</param>
        private void WriteToExcelSheetByBlock(ExcelFileSingleton excelSingleton,
                                              string sheetName,
                                              List<FoodSafetyDetailEM> details,
                                              Dictionary<int, string> dictQuestion,
                                              int startRowIndex)
        {
            //出力開始行
            int rowIndex = startRowIndex;
            //出力行の設定
            foreach (FoodSafetyDetailEM detail in details)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }

            //明細出力に必要な行を指定行に挿入する
            excelSingleton.InsertRowOfSheet(sheetName, startRowIndex + 2, dictQuestion.Count);

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();

            var eVtype = typeof(FoodSafetyDetailEM);

            for (int i = 0; i < dictQuestion.Count + 3; i++)
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

            excelSingleton.WriteRowsToSheet(sheetName, rows);
        }
        /// <summary>
        /// 備考内容を出力
        /// </summary>
        /// <param name="remarks">備考出力用List</param>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        private void WriteRemarksToExcelSheet(List<RemarksEM> remarks,
                                              ExcelFileSingleton excelSingleton,
                                              string sheetName)
        {
            //備考出力開始行
            int remarkRowIndex = 10;
            int insertRow = remarkRowIndex;
            //備考出力行の設定
            foreach (RemarksEM detail in remarks)
            {
                detail.RowIndex = insertRow;
                insertRow++;
            }

            //備考出力に必要な行を指定行に挿入する
            excelSingleton.InsertRowOfSheet(sheetName, remarkRowIndex + 1, remarks.Count - 2);

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();

            //備考の明細をExcelに出力
            var eVtype = typeof(RemarksEM);
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