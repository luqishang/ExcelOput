using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 備考クラス
    /// author : PTJ.Cheng
    /// Create Date : 2020/09/17
    /// </summary>
    public class RemarksEM
    {
        /// <summary>
        /// 行を変更するためのプロパティ
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// 備考
        /// </summary>
        [ExcelColPosition(Col = 1)]
        public string Remarks { get; set; }
    }
}