using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 帳票出力パターン⑤　固定セル内容モデルクラス
    /// </summary>
    public class PersonalMonthlyFixedEM
    {
        /// <summary>
        /// 記録年月
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 1)]
        public string RecordYM { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 5)]
        public string Title { get; set; }

        /// <summary>
        /// 作業者名
        /// </summary>
        [ExcelCellPosition(Row = 3, Col = 1)]
        public string WorkerName { get; set; }

        /// <summary>
        /// 施設の承認者
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 27)]
        public string FacilityApprovalName { get; set; }

        /// <summary>
        /// 大分類の承認者
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 29)]
        public string MajorApprovalName { get; set; }

        /// <summary>
        /// 中分類の承認者
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 31)]
        public string MiddleApprovalName { get; set; }

        /// <summary>
        /// 日付
        /// 1日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 2)]
        public string Day1 { get; set; }

        /// <summary>
        /// 2日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 3)]
        public string Day2 { get; set; }

        /// <summary>
        /// 3日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 4)]
        public string Day3 { get; set; }

        /// <summary>
        /// 4日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 5)]
        public string Day4 { get; set; }

        /// <summary>
        /// 5日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 6)]
        public string Day5 { get; set; }

        /// <summary>
        /// 6日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 7)]
        public string Day6 { get; set; }

        /// <summary>
        /// 7日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 8)]
        public string Day7 { get; set; }

        /// <summary>
        /// 8日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 9)]
        public string Day8 { get; set; }

        /// <summary>
        /// 9日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 10)]
        public string Day9 { get; set; }

        /// <summary>
        /// 10日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 11)]
        public string Day10 { get; set; }

        /// <summary>
        /// 11日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 12)]
        public string Day11 { get; set; }

        /// <summary>
        /// 12日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 13)]
        public string Day12 { get; set; }

        /// <summary>
        /// 13日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 14)]
        public string Day13 { get; set; }

        /// <summary>
        /// 14日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 15)]
        public string Day14 { get; set; }

        /// <summary>
        /// 15日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 16)]
        public string Day15 { get; set; }

        /// <summary>
        /// 16日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 17)]
        public string Day16 { get; set; }

        /// <summary>
        /// 17日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 18)]
        public string Day17 { get; set; }

        /// <summary>
        /// 18日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 19)]
        public string Day18 { get; set; }

        /// <summary>
        /// 19日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 20)]
        public string Day19 { get; set; }

        /// <summary>
        /// 20日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 21)]
        public string Day20 { get; set; }

        /// <summary>
        /// 21日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 22)]
        public string Day21 { get; set; }

        /// <summary>
        /// 22日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 23)]
        public string Day22 { get; set; }

        /// <summary>
        /// 23日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 24)]
        public string Day23 { get; set; }

        /// <summary>
        /// 24日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 25)]
        public string Day24 { get; set; }

        /// <summary>
        /// 25日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 26)]
        public string Day25 { get; set; }

        /// <summary>
        /// 26日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 27)]
        public string Day26 { get; set; }

        /// <summary>
        /// 27日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 28)]
        public string Day27 { get; set; }

        /// <summary>
        /// 28日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 29)]
        public string Day28 { get; set; }

        /// <summary>
        /// 29日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 30)]
        public string Day29 { get; set; }

        /// <summary>
        /// 30日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 31)]
        public string Day30 { get; set; }

        /// <summary>
        /// 31日
        /// </summary>
        [ExcelCellPosition(Row = 5, Col = 32)]
        public string Day31 { get; set; }

        /// <summary>
        /// 曜日
        /// 1日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 2)]
        public string WeekName1 { get; set; }
        /// <summary>
        /// 2日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 3)]
        public string WeekName2 { get; set; }
        /// <summary>
        /// 3日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 4)]
        public string WeekName3 { get; set; }
        /// <summary>
        /// 4日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 5)]
        public string WeekName4 { get; set; }
        /// <summary>
        /// /// <summary>
        /// 5日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 6)]
        public string WeekName5 { get; set; }
        /// <summary>
        /// 6日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 7)]
        public string WeekName6 { get; set; }
        /// <summary>
        /// 7日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 8)]
        public string WeekName7 { get; set; }
        /// <summary>
        /// 8日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 9)]
        public string WeekName8 { get; set; }
        /// <summary>
        /// 9日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 10)]
        public string WeekName9 { get; set; }
        /// <summary>
        /// 10日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 11)]
        public string WeekName10 { get; set; }
        /// <summary>
        /// 11日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 12)]
        public string WeekName11 { get; set; }
        /// <summary>
        /// 12日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 13)]
        public string WeekName12 { get; set; }
        /// <summary>
        /// 13日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 14)]
        public string WeekName13 { get; set; }
        /// <summary>
        /// 14日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 15)]
        public string WeekName14 { get; set; }
        /// <summary>
        /// 15日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 16)]
        public string WeekName15 { get; set; }
        /// <summary>
        /// 16日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 17)]
        public string WeekName16 { get; set; }
        /// <summary>
        /// 17日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 18)]
        public string WeekName17 { get; set; }
        /// <summary>
        /// 18日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 19)]
        public string WeekName18 { get; set; }
        /// <summary>
        /// 19日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 20)]
        public string WeekName19 { get; set; }
        /// <summary>
        /// 20日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 21)]
        public string WeekName20 { get; set; }
        /// <summary>
        /// 21日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 22)]
        public string WeekName21 { get; set; }
        /// <summary>
        /// 22日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 23)]
        public string WeekName22 { get; set; }
        /// <summary>
        /// 23日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 24)]
        public string WeekName23 { get; set; }
        /// <summary>
        /// 24日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 25)]
        public string WeekName24 { get; set; }
        /// <summary>
        /// 25日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 26)]
        public string WeekName25 { get; set; }
        /// <summary>
        /// 26日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 27)]
        public string WeekName26 { get; set; }
        /// <summary>
        /// 27日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 28)]
        public string WeekName27 { get; set; }
        /// <summary>
        /// 28日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 29)]
        public string WeekName28 { get; set; }
        /// <summary>
        /// 29日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 30)]
        public string WeekName29 { get; set; }
        /// <summary>
        /// 30日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 31)]
        public string WeekName30 { get; set; }
        /// <summary>
        /// 31日に対応する曜日
        /// </summary>
        [ExcelCellPosition(Row = 6, Col = 32)]
        public string WeekName31 { get; set; }
    }

    /// <summary>
    /// 日付と曜日のセット
    /// </summary>
    public class DayAndWeekName
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