using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers.Common
{
    public class TransactionFunction
    {
        readonly MasterContext context;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">マスタコンテキスト</param>
        public TransactionFunction(MasterContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// 中分類承認データ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="managetWorkId">管理作業者</param>
        /// <returns>中分類承認データリスト</returns>
        public List<MiddleData> GetMiddleApprovalData(string shopId, string managetWorkId, string periodYMD = null, bool pastFlg = false)
        {
            try
            {
                // 中分類承認データリスト
                var middleList = new List<MiddleData>();

                // 中分類承認(承認待ち)
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT ");
                sql.Append("MID.SHOPID ");
                sql.Append(",MID.CATEGORYID ");
                sql.Append(",MID.LOCATIONID ");
                sql.Append(",MID.REPORTID ");
                sql.Append(",MID.PERIOD ");
                sql.Append(",MID.PERIODSTART ");
                sql.Append(",MID.PERIODEND ");
                sql.Append(",COUNT(REPORTID) AS CNT ");
                sql.Append(",'0' AS MODE ");
                sql.Append("FROM ");
                sql.Append("MIDDLEAPPROVAL_T MID ");
                sql.Append("WHERE ");
                sql.Append("MID.SHOPID = '");
                sql.Append(shopId);
                sql.Append("' ");
                sql.Append("AND MID.STATUS = '");
                sql.Append(ApprovalStatus.PENDING);
                sql.Append("' ");
                sql.Append("AND MID.STAMPFIELD = ");
                sql.Append(StampField.STAMP_RESPOSNSIBLE);
                sql.Append(" ");
                sql.Append("AND MID.DELETEFLAG = '");
                sql.Append(DeleteFlg.NODELETE);
                sql.Append("' ");
                if (!string.IsNullOrEmpty(periodYMD))
                {
                    if (!pastFlg)
                    {
                        sql.Append("AND MID.PERIODSTART <= '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                        sql.Append("AND MID.PERIODEND >= '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                    }
                    else
                    {
                        sql.Append("AND MID.PERIODEND < '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                    }
                }
                sql.Append("GROUP BY MID.SHOPID, MID.CATEGORYID, MID.LOCATIONID, MID.REPORTID, MID.PERIOD, MID.PERIODSTART, MID.PERIODEND ");
                sql.Append("ORDER BY MID.PERIOD, MID.PERIODSTART ");
                sql.Append("FOR READ ONLY ");

                var middle = context.Database.SqlQuery<MiddleData>(sql.ToString());
                if (middle.Count() > 0)
                {
                    middleList.AddRange(middle.ToList());
                }

                // 中分類承認(承認済み、依頼まだのデータ)
                sql = new StringBuilder();
                sql.Append("SELECT ");
                sql.Append("MID.SHOPID ");
                sql.Append(",MID.CATEGORYID ");
                sql.Append(",MID.LOCATIONID ");
                sql.Append(",MID.REPORTID ");
                sql.Append(",MID.PERIOD ");
                sql.Append(",MID.PERIODSTART ");
                sql.Append(",MID.PERIODEND ");
                sql.Append(",'2' AS MODE ");
                sql.Append("FROM ");
                sql.Append("MIDDLEAPPROVAL_T MID ");
                sql.Append("WHERE ");
                sql.Append("MID.SHOPID = '");
                sql.Append(shopId);
                sql.Append("' ");
                sql.Append("AND MID.DELETEFLAG = '");
                sql.Append(DeleteFlg.NODELETE);
                sql.Append("' ");
                sql.Append("AND MID.STAMPFIELD =  ");
                sql.Append(StampField.STAMP_RESPOSNSIBLE);
                sql.Append(" ");
                if (!string.IsNullOrEmpty(periodYMD))
                {
                    if (!pastFlg)
                    {
                        sql.Append("AND MID.PERIODSTART <= '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                        sql.Append("AND MID.PERIODEND >= '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                    }
                    else
                    {
                        sql.Append("AND MID.PERIODEND < '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                    }
                }
                sql.Append("GROUP BY MID.SHOPID, MID.CATEGORYID, MID.LOCATIONID,  ");
                sql.Append("MID.REPORTID, MID.PERIOD, MID.PERIODSTART, MID.PERIODEND ");
                sql.Append("HAVING SUM(CASE WHEN (MID.STATUS = '");
                sql.Append(ApprovalStatus.PENDING);
                sql.Append("' OR MID.STATUS = '");
                sql.Append(ApprovalStatus.REMAND);
                sql.Append("') THEN 1 ELSE 0 END) = 0 ");
                sql.Append("AND SUM(CASE WHEN MID.MIDDLEGROUPNO = 0 THEN 0 ELSE 1 END) = 0 ");
                sql.Append("FOR READ ONLY ");

                var middle_request = context.Database.SqlQuery<MiddleData>(sql.ToString());
                if (middle_request.Count() > 0)
                {
                    middleList.AddRange(middle_request.ToList());
                }

                // 中分類承認(差戻)
                sql = new StringBuilder();
                sql.Append("SELECT ");
                sql.Append("MAJ.SHOPID ");
                sql.Append(",MAJ.CATEGORYID ");
                sql.Append(",MAJ.LOCATIONID ");
                sql.Append(",MAJ.REPORTID ");
                sql.Append(",MAJ.PERIOD ");
                sql.Append(",MAJ.PERIODSTART ");
                sql.Append(",MAJ.PERIODEND ");
                sql.Append(",'1' AS MODE ");
                sql.Append("FROM ");
                sql.Append("MAJORAPPROVAL_T MAJ  ");
                sql.Append("WHERE ");
                sql.Append("MAJ.SHOPID = '");
                sql.Append(shopId);
                sql.Append("' ");
                sql.Append("AND MAJ.STATUS = '");
                sql.Append(ApprovalStatus.REMAND);
                sql.Append("' ");
                sql.Append("AND MAJ.STAMPFIELD = ");
                sql.Append(StampField.STAMP_RESPOSNSIBLE);
                sql.Append(" ");
                sql.Append("AND MAJ.DELETEFLAG = '");
                sql.Append(DeleteFlg.NODELETE);
                sql.Append("' ");
                if (!string.IsNullOrEmpty(periodYMD))
                {
                    if (!pastFlg)
                    {
                        sql.Append("AND MAJ.PERIODSTART <= '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                        sql.Append("AND MAJ.PERIODEND >= '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                    }
                    else
                    {
                        sql.Append("AND MAJ.PERIODEND < '");
                        sql.Append(periodYMD);
                        sql.Append("' ");
                    }
                }
                sql.Append("GROUP BY MAJ.SHOPID, MAJ.CATEGORYID, MAJ.LOCATIONID, MAJ.REPORTID, MAJ.PERIOD, MAJ.PERIODSTART, MAJ.PERIODEND ");
                sql.Append("ORDER BY MAJ.PERIOD, MAJ.PERIODSTART ");
                sql.Append("FOR READ ONLY ");

                var middle_remand = context.Database.SqlQuery<MiddleData>(sql.ToString());
                if (middle_remand.Count() > 0)
                {
                    middleList.AddRange(middle_remand.ToList());
                }

                // 承認ログイン済みの場合は担当データのみ対象とする
                if (!string.IsNullOrEmpty(managetWorkId))
                {
                    // 承認経路マスタからデータを取得
                    var listRoute = from sn in context.ApprovalRouteMs
                                    where sn.SHOPID == shopId && sn.APPMANAGERID == managetWorkId && sn.APPROVALORDERCLASS == APPROVALLEVEL.MIDDLE
                                    select sn;

                    var pendingList = new List<MiddleData>();

                    foreach (ApprovalRouteM route in listRoute)
                    {
                        // 承認担当のデータを取得
                        var tanto = middleList.Where(a => a.CATEGORYID == route.CATEGORYID && a.LOCATIONID == route.LOCATIONID);
                        if (tanto.Count() > 0)
                        {
                            pendingList.AddRange(tanto.ToList());
                        }
                    }
                    // 画面用承認データに格納
                    return this.SetMiddleData(pendingList, shopId);
                }
                else
                {
                    // 画面用承認データに格納
                    return this.SetMiddleData(middleList, shopId);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                throw ex; 
            }
        }

        /// <summary>
        /// 大分類承認データ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="managetWorkId">管理作業者</param>
        /// <returns>大分類承認データリスト</returns>
        public List<MajorData> GetMajorData(string shopId, string managetWorkId, string periodYMD = null, bool pastFlg = false)
        {
            var majorList = new List<MajorData>();
            // 大分類承認（承認待ち）
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MAJ .SHOPID ");
            sql.Append(",MAJ .CATEGORYID ");
            sql.Append(",MAJ .PERIOD ");
            sql.Append(",MAJ .PERIODSTART ");
            sql.Append(",MAJ .PERIODEND ");
            sql.Append(",COUNT(PERIOD) AS CNT ");
            sql.Append(",'0' AS MODE ");
            sql.Append("FROM ");
            sql.Append("MAJORAPPROVAL_T MAJ  ");
            sql.Append("WHERE ");
            sql.Append("MAJ.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND MAJ .STATUS = '");
            sql.Append(ApprovalStatus.PENDING);
            sql.Append("' ");
            sql.Append("AND (MAJ.STAMPFIELD = ");
            sql.Append(StampField.STAMP_RESPOSNSIBLE);
            sql.Append(" OR MAJ.STAMPFIELD = ");
            sql.Append(StampField.STAMP_GROUPLEADER);
            sql.Append(") ");
            sql.Append("AND MAJ.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            if (!string.IsNullOrEmpty(periodYMD))
            {
                if (!pastFlg)
                {
                    sql.Append("AND MAJ.PERIODSTART <= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                    sql.Append("AND MAJ.PERIODEND >= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
                else
                {
                    sql.Append("AND MAJ.PERIODEND < '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
            }
            sql.Append("GROUP BY MAJ .SHOPID, MAJ .CATEGORYID, MAJ .PERIOD, MAJ .PERIODSTART, MAJ .PERIODEND ");
            sql.Append("FOR READ ONLY ");

            var major = context.Database.SqlQuery<MajorData>(sql.ToString());
            if (major.Count() > 0)
            {
                majorList.AddRange(major.ToList());
            }

            // 大分類承認（承認済み、依頼まだのデータ)
            sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("MAJ.SHOPID ");
            sql.Append(",MAJ.CATEGORYID ");
            sql.Append(",MAJ.PERIOD ");
            sql.Append(",MAJ.PERIODSTART ");
            sql.Append(",MAJ.PERIODEND ");
            sql.Append(",'2' AS MODE ");
            sql.Append("FROM ");
            sql.Append("MAJORAPPROVAL_T MAJ ");
            sql.Append("WHERE ");
            sql.Append("MAJ.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND MAJ.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("AND (MAJ.STAMPFIELD = '");
            sql.Append(StampField.STAMP_RESPOSNSIBLE);
            sql.Append("' OR MAJ.STAMPFIELD = '");
            sql.Append(StampField.STAMP_GROUPLEADER);
            sql.Append("') ");
            if (!string.IsNullOrEmpty(periodYMD))
            {
                if (!pastFlg)
                {
                    sql.Append("AND MAJ.PERIODSTART <= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                    sql.Append("AND MAJ.PERIODEND >= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
                else
                {
                    sql.Append("AND MAJ.PERIODEND < '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
            }
            sql.Append("GROUP BY MAJ.SHOPID, MAJ.CATEGORYID, MAJ.PERIOD, MAJ.PERIODSTART, MAJ.PERIODEND ");
            sql.Append("HAVING SUM(CASE WHEN (MAJ.STATUS = '");
            sql.Append(ApprovalStatus.PENDING);
            sql.Append("' OR MAJ.STATUS = '");
            sql.Append(ApprovalStatus.REMAND);
            sql.Append("') THEN 1 ELSE 0 END) = 0 ");
            sql.Append("AND SUM(CASE WHEN MAJ.MAJORGROUPNO = 0 THEN 0 ELSE 1 END) = 0 ");
            sql.Append("FOR READ ONLY ");

            var major_request = context.Database.SqlQuery<MajorData>(sql.ToString());
            if (major_request.Count() > 0)
            {
                majorList.AddRange(major_request.ToList());
            }

            // 大分類承認（差戻）
            sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("FACI.SHOPID ");
            sql.Append(",FACI.CATEGORYID ");
            sql.Append(",FACI.PERIOD ");
            sql.Append(",FACI.PERIODSTART ");
            sql.Append(",FACI.PERIODEND ");
            sql.Append(",'1' AS MODE ");
            sql.Append("FROM ");
            sql.Append("FACILITYAPPROVAL_T FACI  ");
            sql.Append("WHERE ");
            sql.Append("FACI.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND FACI.STATUS = '");
            sql.Append(ApprovalStatus.REMAND);
            sql.Append("' ");
            sql.Append("AND FACI.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            if (!string.IsNullOrEmpty(periodYMD))
            {
                if (!pastFlg)
                {
                    sql.Append("AND FACI.PERIODSTART <= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                    sql.Append("AND FACI.PERIODEND >= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
                else
                {
                    sql.Append("AND FACI.PERIODEND < '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
            }
            sql.Append("GROUP BY FACI.SHOPID, FACI.CATEGORYID, FACI.PERIOD, FACI.PERIODSTART, FACI.PERIODEND ");
            sql.Append("FOR READ ONLY ");

            var major_remand2 = context.Database.SqlQuery<MajorData>(sql.ToString());

            if (major_remand2.Count() > 0)
            {
                majorList.AddRange(major_remand2.ToList());
            }

            // 承認ログイン済みの場合は担当データのみ対象とする
            if (!string.IsNullOrEmpty(managetWorkId))
            {
                // 承認経路マスタからデータを取得
                var listRoute = from sn in context.ApprovalRouteMs
                                where sn.SHOPID == shopId && sn.APPMANAGERID == managetWorkId && sn.APPROVALORDERCLASS == APPROVALLEVEL.MAJORE
                                select sn;

                var pendingList = new List<MajorData>();

                foreach (ApprovalRouteM route in listRoute)
                {
                    // 承認担当のデータを取得
                    var tanto = majorList.Where(a => a.CATEGORYID == route.CATEGORYID);
                    if (tanto.Count() > 0)
                    {
                        pendingList.AddRange(tanto.ToList());
                    }
                }
                // 画面用承認データに格納
                return this.SetMajorData(pendingList, shopId);
            }
            else
            {
                // 画面用承認データに格納
                return this.SetMajorData(majorList, shopId);
            }
        }

        /// <summary>
        /// 施設承認データ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="managetWorkId">管理作業者</param>
        /// <returns>施設承認データリスト</returns>
        public List<FacilityData> GetFacilityData(string shopId, string managetWorkId, string periodYMD = null, bool pastFlg = false)
        {
            var facilityList = new List<FacilityData>();
            // 施設承認（承認待ち）
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("FACI.SHOPID ");
            sql.Append(",FACI.PERIOD ");
            sql.Append(",FACI.PERIODSTART ");
            sql.Append(",FACI.PERIODEND ");
            sql.Append(",COUNT(FACI.PERIOD) AS CNT ");
            sql.Append(",'0' AS MODE ");
            sql.Append("FROM ");
            sql.Append("FACILITYAPPROVAL_T FACI ");
            sql.Append("WHERE ");
            sql.Append("FACI.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND FACI.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("' ");
            sql.Append("AND FACI.STATUS = '");
            sql.Append(ApprovalStatus.PENDING);
            sql.Append("' ");
            if (!string.IsNullOrEmpty(periodYMD))
            {
                if (!pastFlg)
                {
                    sql.Append("AND FACI.PERIODSTART <= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                    sql.Append("AND FACI.PERIODEND >= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
                else
                {
                    sql.Append("AND FACI.PERIODEND < '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
            }
            sql.Append("GROUP BY FACI.SHOPID, FACI.PERIOD, FACI.PERIODSTART, FACI.PERIODEND ");
            sql.Append("FOR READ ONLY ");

            var facility = context.Database.SqlQuery<FacilityData>(sql.ToString());
            if (facility.Count() > 0)
            {
                facilityList.AddRange(facility.ToList());
            }

            // 施設承認（承認済み、依頼まだのデータ）
            sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("FACI.SHOPID ");
            sql.Append(",FACI.PERIOD ");
            sql.Append(",FACI.PERIODSTART ");
            sql.Append(",FACI.PERIODEND ");
            sql.Append(",'2' AS MODE ");
            sql.Append("FROM ");
            sql.Append("FACILITYAPPROVAL_T FACI ");
            sql.Append("WHERE ");
            sql.Append("FACI.SHOPID = '");
            sql.Append(shopId);
            sql.Append("'");
            sql.Append("AND FACI.DELETEFLAG = '");
            sql.Append(DeleteFlg.NODELETE);
            sql.Append("'");
            if (!string.IsNullOrEmpty(periodYMD))
            {
                if (!pastFlg)
                {
                    sql.Append("AND FACI.PERIODSTART <= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                    sql.Append("AND FACI.PERIODEND >= '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
                else
                {
                    sql.Append("AND FACI.PERIODEND < '");
                    sql.Append(periodYMD);
                    sql.Append("' ");
                }
            }
            sql.Append("GROUP BY FACI.SHOPID, FACI.PERIOD, FACI.PERIODSTART, FACI.PERIODEND ");
            sql.Append("HAVING SUM(CASE WHEN (FACI.STATUS = '");
            sql.Append(ApprovalStatus.PENDING);
            sql.Append("' OR FACI.STATUS = '");
            sql.Append(ApprovalStatus.REMAND);
            sql.Append("') THEN 1 ELSE 0 END) = 0 ");
            sql.Append("AND SUM(CASE WHEN FACI.FACILITYAPPGROUPNO = 0 THEN 0 ELSE 1 END) = 0 ");
            sql.Append("FOR READ ONLY ");

            var facility_pending = context.Database.SqlQuery<FacilityData>(sql.ToString());
            if (facility_pending.Count() > 0)
            {
                facilityList.AddRange(facility_pending.ToList());
            }

            // 承認ログイン済みの場合はデータを表示する
            if (!string.IsNullOrEmpty(managetWorkId))
            {
                // 承認経路マスタからデータを取得
                var listRoute = from sn in context.ApprovalRouteMs
                                where sn.SHOPID == shopId && sn.APPMANAGERID == managetWorkId && sn.APPROVALORDERCLASS == APPROVALLEVEL.FACILITY
                                select sn;

                if (listRoute.Count() == 0)
                {
                    return new List<FacilityData>();
                }
            }

            // 画面用承認データに格納
            return this.SetFacilityData(facilityList, shopId);
        }

        /// <summary>
        /// 中分類承認データ項目移送
        /// </summary>
        /// <param name="list">取得中分類データ</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns画面用中分類承認データ></returns>
        private List<MiddleData> SetMiddleData(List<MiddleData> list, string shopId)
        {
            List<MiddleData> setMiddle = new List<MiddleData>();

            // データが存在しない場合
            if (list.Count() == 0)
            {
                return setMiddle;
            }
            // 共通クラス
            CommonFunction comm = new CommonFunction();

            // 大分類マスタ
            var categoryDt = from a in context.CategoryMs
                             where a.SHOPID == shopId
                             select a;
            // 中分類マスタ
            var locationDt = from a in context.LocationMs
                             where a.SHOPID == shopId
                             select a;
            // 帳票マスタ
            var reportDt = from a in context.ReportMs
                           where a.SHOPID == shopId
                           select a;

            foreach (MiddleData middle in list)
            {
                MiddleData dt = new MiddleData
                {
                    SHOPID = middle.SHOPID,
                    CATEGORYID = middle.CATEGORYID,
                    LOCATIONID = middle.LOCATIONID,
                    PERIOD = middle.PERIOD,
                    REPORTID = middle.REPORTID,
                    CNT = middle.CNT,
                    PERIODSTART = middle.PERIODSTART,
                    PERIODEND = middle.PERIODEND,
                    // 周期文言
                    PERIODWORD = comm.GetPeriodDisp(middle.PERIOD),
                    // YYYY/MM/DD
                    PERIODSTARTDATE = comm.FormatDateStr(middle.PERIODSTART),
                    // YYYY/MM/DD
                    PERIODENDDATE = comm.FormatDateStr(middle.PERIODEND),
                    MODE = middle.MODE
                };

                // 大分類名
                var categoryData = categoryDt.Where(a => a.CATEGORYID == middle.CATEGORYID).FirstOrDefault();
                if (categoryData != null)
                {
                    dt.CATEGORYNAME = categoryData.CATEGORYNAME;
                    dt.CATEGORYORDER = categoryData.DISPLAYNO;
                }
                // 中分類名
                var locationData = locationDt.Where(a => a.LOCATIONID == middle.LOCATIONID).FirstOrDefault();
                if (locationData != null)
                {
                    dt.LOCATIONNAME = locationData.LOCATIONNAME;
                    dt.LOCATIONORDER = locationData.DISPLAYNO;
                }
                // 帳票名
                var reportData = reportDt.Where(a => a.REPORTID == middle.REPORTID).FirstOrDefault();
                if (reportData != null)
                {
                    dt.REPORTNAME = reportData.REPORTNAME;
                    dt.REPORTORDER = reportData.DISPLAYNO;
                }

                setMiddle.Add(dt);
            }

            return setMiddle.OrderBy(a => a.PERIOD)
                .ThenByDescending(a => a.PERIODSTART)
                .ThenBy(a => a.CATEGORYORDER)
                .ThenBy(a => a.LOCATIONORDER)
                .ThenBy(a => a.REPORTORDER).ToList();
        }

        /// <summary>
        /// 大分類承認データ項目移送
        /// </summary>
        /// <param name="list">取得大分類データ</param>
        /// <param name="shopId">店舗ID</param>
        /// <returns>画面用大分類承認データ</returns>
        private List<MajorData> SetMajorData(List<MajorData> list, string shopId)
        {
            List<MajorData> setMajor = new List<MajorData>();

            if (list.Count() == 0)
            {
                return setMajor;
            }

            CommonFunction comm = new CommonFunction();

            // 大分類マスタ
            var categoryDt = from a in context.CategoryMs
                             where a.SHOPID == shopId
                             select a;

            foreach (MajorData major in list)
            {
                MajorData dt = new MajorData
                {
                    SHOPID = major.SHOPID,
                    CATEGORYID = major.CATEGORYID,
                    PERIOD = major.PERIOD,
                    CNT = major.CNT,
                    PERIODSTART = major.PERIODSTART,
                    PERIODEND = major.PERIODEND,
                    // 周期文言
                    PERIODWORD = comm.GetPeriodDisp(major.PERIOD),
                    // YYYY/MM/DD
                    PERIODSTARTDATE = comm.FormatDateStr(major.PERIODSTART),
                    // YYYY/MM/DD
                    PERIODENDDATE = comm.FormatDateStr(major.PERIODEND),
                    MODE = major.MODE
                };
                // 大分類名
                var categoryData = categoryDt.Where(a => a.CATEGORYID == major.CATEGORYID).FirstOrDefault();
                if (categoryData != null)
                {
                    dt.CATEGORYNAME = categoryData.CATEGORYNAME;
                    dt.CATEGORYORDER = (short)categoryData.DISPLAYNO;
                }
                setMajor.Add(dt);
            }

            return setMajor.OrderBy(a => a.PERIOD)
                .ThenByDescending(a => a.PERIODSTART)
                .ThenBy(a => a.CATEGORYORDER).ToList();
        }

        /// <summary>
        /// 施設承認データ項目移送
        /// </summary>
        /// <param name="list">取得施設分類データ</param>
        /// <param name="shopId">店舗IS</param>
        /// <returns>画面用施設承認データ</returns>
        private List<FacilityData> SetFacilityData(List<FacilityData> list, string shopId)
        {
            List<FacilityData> setFacility = new List<FacilityData>();

            if (list.Count() == 0)
            {
                return setFacility;
            }

            CommonFunction comm = new CommonFunction();

            foreach (FacilityData facility in list)
            {
                FacilityData dt = new FacilityData
                {
                    SHOPID = facility.SHOPID,
                    PERIOD = facility.PERIOD,
                    CNT = facility.CNT,
                    PERIODSTART = facility.PERIODSTART,
                    PERIODEND = facility.PERIODEND,
                    // 周期文言
                    PERIODWORD = comm.GetPeriodDisp(facility.PERIOD),
                    // YYYY/MM/DD
                    PERIODSTARTDATE = comm.FormatDateStr(facility.PERIODSTART),
                    // YYYY/MM/DD
                    PERIODENDDATE = comm.FormatDateStr(facility.PERIODEND),
                    MODE = facility.MODE
                };

                setFacility.Add(dt);
            }

            return setFacility.OrderBy(a => a.PERIOD).ThenByDescending(a => a.PERIODSTART).ToList();
        }

        /// <summary>
        /// 承認権限有無
        /// </summary>
        /// <param name="shopId">店舗IDparam>
        /// <param name="categoryId">大分類ID</param>
        /// <param name="locationId">中分類ID</param>
        /// <param name="managerId">管理作業者ID</param>
        /// <param name="approvalClass">承認経路分類</param>
        /// <returns>判定結果（true：承認権限あり）</returns>
        public bool GetApprovalAuthority(string shopId, string categoryId, string locationId, string managerId, string approvalClass)
        {
            // 承認経路マスタからデータを取得
            var listRoute = from sn in context.ApprovalRouteMs
                            where sn.SHOPID == shopId 
                                && sn.CATEGORYID == categoryId
                                && sn.LOCATIONID == locationId
                                && sn.APPROVALORDERCLASS == approvalClass
                                && sn.APPMANAGERID == managerId
                            select sn;

            if (listRoute.Count() > 0)
            {
                return true;
            }

            return false;
        }
    }
}