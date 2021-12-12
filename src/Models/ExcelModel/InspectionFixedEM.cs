using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 検収記録（日報）の固定内容のモデル
    /// author : PTJ.張
    /// Create Date : 2020/09/16
    /// </summary>
    public class InspectionFixedEM
    {
        /// <summary>
        /// 記録時間(周期開始日)
        /// </summary>
        [ExcelCellPosition(Row = 1, Col = 1)]
        public string RecordTime { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 2)]
        public string Title { get; set; }

        /// <summary>
        /// 施設の承認者
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 23)]
        public string FacilitiesRecognizerName { get; set; }

        /// <summary>
        /// 大分類の承認者
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 24)]
        public string MajorRecognizerName { get; set; }

        /// <summary>
        /// 中分類の承認者
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 25)]
        public string MiddleRecognizerName { get; set; }
    }
}