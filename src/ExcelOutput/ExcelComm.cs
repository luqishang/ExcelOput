using System;
using System.Collections.Generic;
using System.Linq;
using HACCPExtender.Models;
using HACCPExtender.Models.Custom;
using HACCPExtender.Models.ExcelModel;
using HACCPExtender.Constants;
using System.Text;
using System.IO;
using HACCPExtender.Controllers.Common;

namespace HACCPExtender.ExcelOutput
{
    public class ExcelComm
    {
        /// <summary>
		/// EXCELのセルの内容を設定、取得します。
		/// </summary>
		/// <param name="reportInterface">開始日</param>
		/// <returns>Dictionary<key日, value曜日></returns>
		/// <remarks></remarks>
        public static Dictionary<int, DayWeekName> GetDayAndWeekName(CustomReportInterfaceM reportInterface)
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

            Nullable<DateTime> startDate = GetStartDate(reportInterface);
            if (!startDate.HasValue) return null;
            Nullable<DateTime> endDate = GetEndDate(reportInterface);


            //日付、曜日を格納するための辞書
            Dictionary<int, DayWeekName> dictDay = new Dictionary<int, DayWeekName>();

            int key = 1;
            //日付、曜日を格納
            for (DateTime itemDay = startDate.Value; itemDay.CompareTo(endDate.Value) < 0; key++)
            {
                DayWeekName dayWeekName = new DayWeekName();
                //日付を定義
                dayWeekName.Day = itemDay.Day.ToString() + "日";
                //曜日を取得
                dayWeekName.WeekName = itemDay.ToString("ddd");
                //辞書に追加
                dictDay.Add(key, dayWeekName);

                itemDay = itemDay.AddDays(1);
            }

            return dictDay;
        }

        /// <summary>
		/// EXCELのセルの内容を設定、取得します。
		/// </summary>
		/// <param name="reportInterface">開始日</param>
		/// <returns>Dictionary<key日, value曜日></returns>
		/// <remarks></remarks>
        public static Nullable<DateTime> GetStartDate(CustomReportInterfaceM reportInterface)
        {
            try
            {
                int year;
                int month;
                int day;
                year = Int32.Parse(reportInterface.PeriodStart.Substring(0, 4));
                month = Int32.Parse(reportInterface.PeriodStart.Substring(4, 2));
                day = Int32.Parse(reportInterface.PeriodStart.Substring(6, 2));
                DateTime startDate = new DateTime(year, month, day);
                return startDate;
            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
		/// EXCELのセルの内容を設定、取得します。
		/// </summary>
		/// <param name="reportInterface">開始日</param>
		/// <returns>Dictionary<key日, value曜日></returns>
		/// <remarks></remarks>
        public static Nullable<DateTime> GetEndDate(CustomReportInterfaceM reportInterface)
        {
            Nullable<DateTime> endDate = null;
            try
            {
                Nullable<DateTime> startDate = GetStartDate(reportInterface);
                if (!startDate.HasValue) return null;
                if (reportInterface.Period == Const.PeriodDay)
                {
                    endDate = startDate.Value.AddDays(1);
                }
                else if (reportInterface.Period == Const.PeriodWeek)
                {
                    endDate = startDate.Value.AddDays(7);
                }
                else if (reportInterface.Period == Const.PeriodMonth)
                {
                    endDate = startDate.Value.AddMonths(1);
                }
                return endDate;
            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// 帳票リストからSQL文を作成する
        /// </summary>
        /// <param name="reportList"></param>
        /// <param name="tblName"></param>
        public static string GetSqlForReportInfo(List<CustomReportM> reportList, string tblName)
        {
            if ((reportList == null) || (reportList.Count == 0))
            {
                return "";
            }

            StringBuilder sql = new StringBuilder();
            sql.Append(" AND( ");

            for (int i = 0; i < reportList.Count; i++)
            {
                string statment = "";
                if (i > 0)
                {
                    statment = " OR (" + tblName + ".LOCATIONID = 'param1' AND " + tblName + ".REPORTID = 'param2' ) ";
                }
                else
                {
                    statment = "  (" + tblName + ".LOCATIONID = 'param1' AND " + tblName + ".REPORTID = 'param2' ) ";
                }
                statment = statment.Replace("param1", reportList[i].LocationId);
                statment = statment.Replace("param2", reportList[i].ReportId);
                sql.Append(statment);
            }
            sql.Append(" ) ");

            return sql.ToString();
        }

        /// <summary>
        /// 承認者情報を取得する
        /// </summary>
        /// <param name="reportList"></param>
        /// <param name="tblName"></param>
        public static CustomApprovalInfo GetAprrovalInfo(CustomReportInterfaceM reportInterface, MasterContext context)
        {

            // データ取得
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT ");
            sql.Append("  tbl_4.WORKERNAME AS MiddleApprovalName ");
            sql.Append("  ,tbl_8.WORKERNAME AS MajorApprovalName ");
            sql.Append("  ,tbl_10.WORKERNAME AS FacilityApprovalName ");
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
            sql.Append("  INNER JOIN MAJORAPPROVAL_T tbl_7 ");
            sql.Append("    ON tbl_3.SHOPID = tbl_7.SHOPID ");
            sql.Append("    AND tbl_3.CATEGORYID = tbl_7.CATEGORYID ");
            sql.Append("    AND tbl_3.LOCATIONID = tbl_7.LOCATIONID ");
            sql.Append("    AND tbl_3.REPORTID = tbl_7.REPORTID ");
            sql.Append("    AND tbl_3.PERIOD = tbl_7.PERIOD ");
            sql.Append("    AND tbl_3.PERIODSTART = tbl_7.PERIODSTART ");
            sql.Append("    AND tbl_7.DELETEFLAG = '0' ");
            sql.Append("  INNER JOIN WORKER_M tbl_8 ");
            sql.Append("    ON tbl_7.SHOPID = tbl_8.SHOPID ");
            sql.Append("    AND tbl_7.MAJORSNNUSER = tbl_8.WORKERID ");
            sql.Append("  LEFT JOIN FACILITYAPPROVAL_T tbl_9 ");
            sql.Append("    ON tbl_7.SHOPID = tbl_9.SHOPID ");
            sql.Append("    AND tbl_7.CATEGORYID = tbl_9.CATEGORYID ");
            sql.Append("    AND tbl_7.PERIOD = tbl_9.PERIOD ");
            sql.Append("    AND tbl_7.PERIODSTART = tbl_9.PERIODSTART ");
            sql.Append("    AND tbl_9.DELETEFLAG = '0' ");
            sql.Append("  LEFT JOIN WORKER_M tbl_10 ");
            sql.Append("    ON tbl_9.SHOPID = tbl_10.SHOPID ");
            sql.Append("    AND tbl_9.FACILITYSNNUSER = tbl_10.WORKERID ");

            var detailDt = context.Database.SqlQuery<CustomApprovalInfo>(sql.ToString());

            if (detailDt.Count() == 0)
            {
                return new CustomApprovalInfo();
            }

            List<CustomApprovalInfo> dbList = detailDt.ToList();
            return dbList[0];
        }

        /// <summary>
        /// 設問を格納するための辞書
        /// </summary>
        /// <param name="customData"></param>
        public static Dictionary<int, string> GetQuestionDict<T>(T detail)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            string questionName = "Question";

            for (int i = 1; i <= 20; i++)
            {
                string propertyName = questionName + i.ToString();
                var property = typeof(T).GetProperty(propertyName);
                var value = property.GetValue(detail);
                if (value != null)
                {
                    dict.Add(i, value.ToString());
                }
            }
            return dict;
        }

        /// <summary>
        /// Dirが存在するかどうかチェックする
        /// 存在しない場合、作成する
        /// </summary>
        /// <param name="customData"></param>
        public static bool CheckDir(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }   
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// ファイルがロックされるかどうか判断する
        /// </summary>
        /// <param name="filename"></param>
        public static bool IsFileLocked(string filename)
        {
            try
            {
                //ファイルが存在しない場合
                if (!File.Exists(filename)) return false;
                //ファイルが存在する場合、ロックされるかどうかチェックする
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return !(inputStream.Length > 0);
            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                return true;
            }
        }

    }
}