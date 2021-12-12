using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 帳票出力パターン④　固定セル内容モデルクラス
    /// @Author:PTJ.小嶋
    /// @CreateDate:2020/09/25
    /// </summary>
    public class FoodSafetyFixedEM
    {
        /// <summary>
        /// 記録年月日
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 1)]
        public string Date { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 10)]
        public string Title { get; set; }

        /// <summary>
        /// 施設承認者名
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 23)]
        public string FacilityApprovalName { get; set; }

        /// <summary>
        /// 大分類承認者名
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 24)]
        public string MajorApprovalName { get; set; }

        /// <summary>
        /// 中分類承認者名
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 25)]
        public string MiddleApprovalName { get; set; }


    }
}