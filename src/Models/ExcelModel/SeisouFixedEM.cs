using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 清掃記録固定セル内容クラス
    /// author : PTJ Cheng
    /// Create Date : 2020/09/09
    /// </summary>
    public class SeisouFixedEM
    {
        /// <summary>
        /// 年月
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 1)]
        public string YearAndMonth { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 5)]
        public string Title { get; set; }

        /// <summary>
        /// 施設承認者名
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 29)]
        public string FacilityApprovalName { get; set; }

        /// <summary>
        /// 大分類承認者名
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 31)]
        public string MajorApprovalName { get; set; }

        /// <summary>
        /// 1日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 2)]
        public string Day_1 { get; set; }

        /// <summary>
        /// 2日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 3)]
        public string Day_2 { get; set; }

        /// <summary>
        /// 3日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 4)]
        public string Day_3 { get; set; }

        /// <summary>
        /// 4日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 5)]
        public string Day_4 { get; set; }

        /// <summary>
        /// 5日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 6)]
        public string Day_5 { get; set; }

        /// <summary>
        /// 6日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 7)]
        public string Day_6 { get; set; }

        /// <summary>
        /// 7日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 8)]
        public string Day_7 { get; set; }

        /// <summary>
        /// 8日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 9)]
        public string Day_8 { get; set; }

        /// <summary>
        /// 9日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 10)]
        public string Day_9 { get; set; }

        /// <summary>
        /// 10日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 11)]
        public string Day_10 { get; set; }

        /// <summary>
        /// 11日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 12)]
        public string Day_11 { get; set; }

        /// <summary>
        /// 12日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 13)]
        public string Day_12 { get; set; }

        /// <summary>
        /// 13日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 14)]
        public string Day_13 { get; set; }

        /// <summary>
        /// 14日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 15)]
        public string Day_14 { get; set; }

        /// <summary>
        /// 15日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 16)]
        public string Day_15 { get; set; }

        /// <summary>
        /// 16日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 17)]
        public string Day_16 { get; set; }

        /// <summary>
        /// 17日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 18)]
        public string Day_17 { get; set; }

        /// <summary>
        /// 18日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 19)]
        public string Day_18 { get; set; }

        /// <summary>
        /// 19日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 20)]
        public string Day_19 { get; set; }

        /// <summary>
        /// 20日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 21)]
        public string Day_20 { get; set; }

        /// <summary>
        /// 21日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 22)]
        public string Day_21 { get; set; }

        /// <summary>
        /// 22日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 23)]
        public string Day_22 { get; set; }

        /// <summary>
        /// 23日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 24)]
        public string Day_23 { get; set; }

        /// <summary>
        /// 24日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 25)]
        public string Day_24 { get; set; }

        /// <summary>
        /// 25日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 26)]
        public string Day_25 { get; set; }

        /// <summary>
        /// 26日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 27)]
        public string Day_26 { get; set; }

        /// <summary>
        /// 27日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 28)]
        public string Day_27 { get; set; }

        /// <summary>
        /// 28日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 29)]
        public string Day_28 { get; set; }

        /// <summary>
        /// 29日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 30)]
        public string Day_29 { get; set; }

        /// <summary>
        /// 30日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 31)]
        public string Day_30 { get; set; }

        /// <summary>
        /// 31日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 32)]
        public string Day_31 { get; set; }

        /// <summary>
        /// 以下、取得した曜日を対応する日に当てはめる
        /// (例)1日が月曜日
        /// WeekName_1 = "月"
        /// WeekName_2 = "火"
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 2)]
        public string WeekName_1 { get; set; }
        /// <summary>
        /// 2日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 3)]
        public string WeekName_2 { get; set; }
        /// <summary>
        /// 3日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 4)]
        public string WeekName_3 { get; set; }
        /// <summary>
        /// 4日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 5)]
        public string WeekName_4 { get; set; }
        /// <summary>
        /// /// <summary>
        /// 5日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 6)]
        public string WeekName_5 { get; set; }
        /// <summary>
        /// 6日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 7)]
        public string WeekName_6 { get; set; }
        /// <summary>
        /// 7日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 8)]
        public string WeekName_7 { get; set; }
        /// <summary>
        /// 8日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 9)]
        public string WeekName_8 { get; set; }
        /// <summary>
        /// 9日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 10)]
        public string WeekName_9 { get; set; }
        /// <summary>
        /// 10日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 11)]
        public string WeekName_10 { get; set; }
        /// <summary>
        /// 11日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 12)]
        public string WeekName_11 { get; set; }
        /// <summary>
        /// 12日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 13)]
        public string WeekName_12 { get; set; }
        /// <summary>
        /// 13日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 14)]
        public string WeekName_13 { get; set; }
        /// <summary>
        /// 14日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 15)]
        public string WeekName_14 { get; set; }
        /// <summary>
        /// 15日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 16)]
        public string WeekName_15 { get; set; }
        /// <summary>
        /// 16日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 17)]
        public string WeekName_16 { get; set; }
        /// <summary>
        /// 17日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 18)]
        public string WeekName_17 { get; set; }
        /// <summary>
        /// 18日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 19)]
        public string WeekName_18 { get; set; }
        /// <summary>
        /// 19日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 20)]
        public string WeekName_19 { get; set; }
        /// <summary>
        /// 20日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 21)]
        public string WeekName_20 { get; set; }
        /// <summary>
        /// 21日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 22)]
        public string WeekName_21 { get; set; }
        /// <summary>
        /// 22日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 23)]
        public string WeekName_22 { get; set; }
        /// <summary>
        /// 23日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 24)]
        public string WeekName_23 { get; set; }
        /// <summary>
        /// 24日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 25)]
        public string WeekName_24 { get; set; }
        /// <summary>
        /// 25日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 26)]
        public string WeekName_25 { get; set; }
        /// <summary>
        /// 26日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 27)]
        public string WeekName_26 { get; set; }
        /// <summary>
        /// 27日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 28)]
        public string WeekName_27 { get; set; }
        /// <summary>
        /// 28日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 29)]
        public string WeekName_28 { get; set; }
        /// <summary>
        /// 29日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 30)]
        public string WeekName_29 { get; set; }
        /// <summary>
        /// 30日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 31)]
        public string WeekName_30 { get; set; }
        /// <summary>
        /// 31日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 32)]
        public string WeekName_31 { get; set; }
    }

    /// <summary>
    /// 清掃記録固定セル内容クラス
    /// author : PTJ cheng
    /// Create Date : 2020/09/16
    /// </summary>
    public class DayWeekName
    {
        /// <summary>
        /// 日付
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// 曜日
        /// </summary>
        public string WeekName { get; set; }
    }
}
