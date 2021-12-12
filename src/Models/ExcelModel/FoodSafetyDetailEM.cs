using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 帳票出力パターン④　明細セル内容モデルクラス
    /// </summary>
    public class FoodSafetyDetailEM
    {
        /// <summary>
        /// 行を変更するためのインデックス
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// チェック項目
        /// </summary>
        [ExcelColPosition(Col = 1)]
        public string CheckItem { get; set; }

        /// <summary>
        /// 1回目
        /// </summary>
        [ExcelColPosition(Col = 2)]
        public string No_1 { get; set; }

        /// <summary>
        /// 2回目
        /// </summary>
        [ExcelColPosition(Col = 3)]
        public string No_2 { get; set; }

        /// <summary>
        /// 3回目
        /// </summary>
        [ExcelColPosition(Col = 4)]
        public string No_3 { get; set; }

        /// <summary>
        /// 4回目
        /// </summary>
        [ExcelColPosition(Col = 5)]
        public string No_4 { get; set; }

        /// <summary>
        /// 5回目
        /// </summary>
        [ExcelColPosition(Col = 6)]
        public string No_5 { get; set; }

        /// <summary>
        /// 6回目
        /// </summary>
        [ExcelColPosition(Col = 7)]
        public string No_6 { get; set; }

        /// <summary>
        /// 7回目
        /// </summary>
        [ExcelColPosition(Col = 8)]
        public string No_7 { get; set; }

        /// <summary>
        /// 8回目
        /// </summary>
        [ExcelColPosition(Col = 9)]
        public string No_8 { get; set; }

        /// <summary>
        /// 9回目
        /// </summary>
        [ExcelColPosition(Col = 10)]
        public string No_9 { get; set; }

        /// <summary>
        /// 10回目
        /// </summary>
        [ExcelColPosition(Col = 11)]
        public string No_10 { get; set; }

        /// <summary>
        /// 11回目
        /// </summary>
        [ExcelColPosition(Col = 12)]
        public string No_11 { get; set; }

        /// <summary>
        /// 12回目
        /// </summary>
        [ExcelColPosition(Col = 13)]
        public string No_12 { get; set; }

        /// <summary>
        /// 13回目
        /// </summary>
        [ExcelColPosition(Col = 14)]
        public string No_13 { get; set; }

        /// <summary>
        /// 14回目
        /// </summary>
        [ExcelColPosition(Col = 15)]
        public string No_14 { get; set; }

        /// <summary>
        /// 15回目
        /// </summary>
        [ExcelColPosition(Col = 16)]
        public string No_15 { get; set; }

        /// <summary>
        /// 16回目
        /// </summary>
        [ExcelColPosition(Col = 17)]
        public string No_16 { get; set; }

        /// <summary>
        /// 17回目
        /// </summary>
        [ExcelColPosition(Col = 18)]
        public string No_17 { get; set; }

        /// <summary>
        /// 18回目
        /// </summary>
        [ExcelColPosition(Col = 19)]
        public string No_18 { get; set; }

        /// <summary>
        /// 19回目
        /// </summary>
        [ExcelColPosition(Col = 20)]
        public string No_19 { get; set; }

        /// <summary>
        /// 20回目
        /// </summary>
        [ExcelColPosition(Col = 21)]
        public string No_20 { get; set; }

        /// <summary>
        /// 21回目
        /// </summary>
        [ExcelColPosition(Col = 22)]
        public string No_21 { get; set; }

        /// <summary>
        /// 22回目
        /// </summary>
        [ExcelColPosition(Col = 23)]
        public string No_22 { get; set; }

        /// <summary>
        /// 23回目
        /// </summary>
        [ExcelColPosition(Col = 24)]
        public string No_23 { get; set; }

        /// <summary>
        /// 24回目
        /// </summary>
        [ExcelColPosition(Col = 25)]
        public string No_24 { get; set; }
    }
}