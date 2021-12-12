using System.Text;
using System.IO;
using HACCPExtender.Models.ExcelModel;
using HACCPExtender.Models;
using HACCPExtender.Models.Custom;
using HACCPExtender.Constants;
using System.Collections.Generic;
using System;
using System.Linq;
using PA.Office.ExcelObjects;
using System.Reflection;
using OfficePositionAttributes;
using System.Web.Hosting;
using System.Threading;
using HACCPExtender.Controllers.Common;
using System.Data.Entity;
using HACCPExtender.Business;

namespace HACCPExtender.ExcelOutput
{
    /// <summary>
    /// 検収記録（日報）のPDFを出力
    /// author : PTJ.張
    /// Create Date : 2020/09/25
    /// </summary>
    public class ExcelPattern_2
    {
        //設問の辞書型＜設問の番号、設問の内容＞のcountをフィールド変数として定義
        private int m_ColIndex;

        private int m_RemarkColIndex = 0;

        private MasterContext context = new MasterContext();

        /// <summary>
        /// PDFファイルを出力
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns></returns>
        public bool OutPDF(CustomReportInterfaceM reportInterface)
        {
            //テンプレートファイル取得
            string templeteFileName = HostingEnvironment.MapPath("~/ExcelTemplate/pattern_2_template.xlsx");

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

            //Constからシート名を取得する
            string sheetName = Const.SheetName;

            //Excel出力用オブジェクトを生成する
            ExcelFileSingleton excelSingleton = ExcelFileSingleton.GetInstance();

            try
            {
                //Excelファイルを読み込みます。
                excelSingleton.OpenExcel(excelFileName);

                // EXCELの固定セルの内容を設定する。
                SetFixData2ExcelForInspection(excelSingleton, sheetName, reportInterface);

                // EXCELの明細内容を設定する。
                SetDetailData2ExcelForInspection(excelSingleton, sheetName, reportInterface);

                //設問の数を判断して、空白の列を削除
                if (m_ColIndex < 20)
                {
                    excelSingleton.DeleteColOfSheet(sheetName, m_ColIndex + 1, 20 - m_ColIndex);
                }

                //備考列を削除する
                if (m_RemarkColIndex != 0)
                {
                    excelSingleton.DeleteColOfSheet(sheetName, m_RemarkColIndex, 1);
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
        /// <param name="path">ファイルパス</param>
        /// <param name="fileName">ファイル名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void UpdateMiddleApprovalT(string path, string fileName, CustomReportInterfaceM reportInterface)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT ");
            sql.Append("  tbl_3.* ");
            sql.Append(" FROM  ");
            sql.Append("  TEMPERATURECONTROL_T tbl_1 ");
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
            sql.Append("    WHERE ");
            sql.Append("        tbl_1.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("  AND tbl_1.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("  AND tbl_1.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("  AND tbl_1.PERIODSTART = '");
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
        /// 固定セル内容設定用メソッド(検収記録用)
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void SetFixData2ExcelForInspection(ExcelFileSingleton excelSingleton, string sheetName, CustomReportInterfaceM reportInterface)
        {

            //承認者共通部品で承認者情報を取得する
            CustomApprovalInfo approvalInfo = ExcelComm.GetAprrovalInfo(reportInterface, context);

            //固定セル内容を設定
            InspectionFixedEM fixedEM = new InspectionFixedEM
            {

                //タイトル
                Title = reportInterface.Title,
                //施設承認者名
                FacilitiesRecognizerName = approvalInfo.FacilityApprovalName,
                //大分類承認者
                MajorRecognizerName = approvalInfo.MajorApprovalName,
                //中分類承認者名
                MiddleRecognizerName = approvalInfo.MiddleApprovalName,
            };

            //記録時間(周期開始日)（NULL,空値と長さを確認する）
            if (!string.IsNullOrEmpty(reportInterface.PeriodStart) && reportInterface.PeriodStart.Length == 8)
            {
                //記録時間(周期開始日)
                fixedEM.RecordTime = reportInterface.PeriodStart.Substring(0, 4) + "年" + reportInterface.PeriodStart.Substring(4, 2) + "月" + reportInterface.PeriodStart.Substring(6, 2) + "日";
            }

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();
            ExcelRowObject headRow = new ExcelRowObject();

            var eVtype = typeof(InspectionFixedEM);
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
        /// 詳細セル内容設定用メソッド(検収記録用)
        /// </summary>
        /// <param name="excelSingleton">Excel出力用オブジェクト</param>
        /// <param name="sheetName">シート名</param>
        /// <param name="reportInterface">帳票インターフェース</param>
        private void SetDetailData2ExcelForInspection(ExcelFileSingleton excelSingleton, string sheetName, CustomReportInterfaceM reportInterface)
        {
            //中分類の明細のデータを設定する
            List<InspectionDetailEM> details = GetInspectionDetails(reportInterface);

            //Excel出力の開始行
            int startRowIndex = 5;

            int rowIndex = startRowIndex;

            foreach (InspectionDetailEM detail in details)
            {
                detail.RowIndex = rowIndex;
                rowIndex++;
            }
            //新規の行を指定行(7)に挿入する,挿入した行数は（details.Count - 3）
            excelSingleton.InsertRowOfSheet(sheetName, startRowIndex + 2, details.Count - 3);

            //セルの値を設定する
            List<ExcelRowObject> rows = new List<ExcelRowObject>();

            var eVtype = typeof(InspectionDetailEM);

            for (int i = 0; i < details.Count; i++)
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
        /// 検収記録の明細データを取得する
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns>DBから取得したデータList</returns>
        public List<CustomInspectionEM> GetInspectionDataFromDB(CustomReportInterfaceM reportInterface)
        {
            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("  SUBSTR(tbl_1.DATAYMD, 9, 2) || ':' || SUBSTR(tbl_1.DATAYMD, 11, 2) AS AcceptanceTime");
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
            sql.Append("  ,tbl_1.WORKERNAME AS WorkerName ");
            sql.Append("  , CAST(tbl_1.REMARKSNO AS INTEGER) AS RemarksNo ");
            sql.Append(" FROM  ");
            sql.Append("  TEMPERATURECONTROL_T tbl_1 ");
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
            sql.Append("    WHERE ");
            sql.Append("        tbl_1.SHOPID = '");
            sql.Append(reportInterface.ShopId);
            sql.Append("' ");
            if (!String.IsNullOrEmpty(reportInterface.CategoryId))
            {
                sql.Append("  AND tbl_1.CATEGORYID = '");
                sql.Append(reportInterface.CategoryId);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.Period))
            {
                sql.Append("  AND tbl_1.PERIOD = '");
                sql.Append(reportInterface.Period);
                sql.Append("' ");
            }
            if (!String.IsNullOrEmpty(reportInterface.PeriodStart))
            {
                sql.Append("  AND tbl_1.PERIODSTART = '");
                sql.Append(reportInterface.PeriodStart);
                sql.Append("' ");
            }
            sql.Append(ExcelComm.GetSqlForReportInfo(reportInterface.ReportList, "tbl_1"));

            var detailDt = context.Database.SqlQuery<CustomInspectionEM>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return null;
            }

            List<CustomInspectionEM> dbList = detailDt.ToList();

            return dbList;
        }

        /// <summary>
        /// 検収記録の明細データをPDF出力用の明細データに変換する
        /// </summary>
        /// <param name="reportInterface">帳票インターフェース</param>
        /// <returns>抽象化された明細モデルのオブジェクトのList</returns>
        private List<InspectionDetailEM> GetInspectionDetails(CustomReportInterfaceM reportInterface)
        {
            //明細モデルのオブジェクトを生成(抽象化)
            List<InspectionDetailEM> inspectionDetailEMs = new List<InspectionDetailEM>();

            //DBから必要なデータを抽出する
            List<CustomInspectionEM> dbList = GetInspectionDataFromDB(reportInterface);

            //設問の辞書型＜設問の番号、設問の内容＞（DBから抽出された最初のデータを利用）
            Dictionary<int, string> questionDic = ExcelComm.GetQuestionDict<CustomInspectionEM>(dbList[0]);

            this.m_ColIndex = questionDic.Count;

            //HEADの行を追加する
            InspectionDetailEM headDetail = new InspectionDetailEM
            {

                AcceptanceTime = "受入時間",
                WorkerName = "記録者",
                Remarks = "備考",
            };

            foreach (KeyValuePair<int, string> kvp in questionDic)
            {
                int key = kvp.Key;
                string value = kvp.Value;

                if (value.Equals("備考"))
                {
                    m_RemarkColIndex = key;
                }

                var itemPType = typeof(InspectionDetailEM).GetProperty("Result" + key);
                itemPType.SetValue(headDetail, value);
            }
            inspectionDetailEMs.Add(headDetail);

            foreach (CustomInspectionEM dbDetail in dbList)
            {
                //明細モデルを抽象化する
                InspectionDetailEM item = new InspectionDetailEM();
                //明細モデルのオブジェクトに追加
                inspectionDetailEMs.Add(item);
                //受入時間
                item.AcceptanceTime = dbDetail.AcceptanceTime;
                //備考設問番号
                int remarksNo = dbDetail.RemarksNo;

                foreach (KeyValuePair<int, string> kvp in questionDic)
                {
                    int key = kvp.Key;
                    var dbtype = typeof(CustomInspectionEM).GetProperty("Result" + key);
                    var value = dbtype.GetValue(dbDetail);
                    string resultValue = value == null ? "" : value.ToString();

                    //設問結果を備考欄に出力
                    if (remarksNo > 0 && key == remarksNo)
                    {
                        //設問の回答
                        item.Remarks = resultValue;
                    }
                    //設問結果を元のところに出力
                    else
                    {
                        var itemPType = typeof(InspectionDetailEM).GetProperty("Result" + key);
                        itemPType.SetValue(item, value);
                    }

                }
                //記録者
                item.WorkerName = dbDetail.WorkerName;
            }
            return inspectionDetailEMs;
        }
    }
}