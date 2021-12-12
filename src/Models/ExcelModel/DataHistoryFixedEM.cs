using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    public class DataHistoryFixedEM
    {
        /// <summary>
        /// 大分類名称
        /// </summary>
        [ExcelCellPosition(Row = 1, Col = 1)]
        public string CategoryName { get; set; }

        /// <summary>
        /// 中分類名称
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 1)]
        public string LocationName { get; set; }

        /// <summary>
        /// 帳票名称
        /// </summary>
        [ExcelCellPosition(Row = 3, Col = 1)]
        public string ReportName { get; set; }

        /// <summary>
        /// データ記録範囲
        /// </summary>
        [ExcelCellPosition(Row = 4, Col = 1)]
        public string DataRecordingRange { get; set; }

    }
}