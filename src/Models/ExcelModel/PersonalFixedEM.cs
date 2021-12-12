using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 帳票出力パターン③　固定セル内容モデルクラス
    /// @Author:PTJ.小嶋
    /// @CreateDate:2020/9/16
    /// </summary>
    public class PersonalFixedEM
    {
        /// <summary>
        /// 記録年月日
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 1)]
        public string Date { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 9)]
        public string Title { get; set; }

        /// <summary>
        /// 施設承認者名
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 27)]
        public string FacilityApprovalName { get; set; }

        /// <summary>
        /// 大分類承認者名
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 29)]
        public string MajorApprovalName { get; set; }

        /// <summary>
        /// 中分類承認者名
        /// </summary>
        [ExcelCellPosition(Row = 2, Col = 31)]
        public string MiddleApprovalName { get; set; }


    }
}