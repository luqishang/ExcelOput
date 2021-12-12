using System;
using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 清掃記録詳細セル内容クラス
    /// author : PTJ.Cheng
    /// Create Date : 2020/09/09
    /// </summary>
    public class SeisouDetailEM
    {
        /// <summary>
        /// 行を変更するためのプロパティ
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// 背景色のプロパティ
        /// </summary>
        public Nullable<System.Drawing.Color> BackColor { get; set; }

        /// <summary>
        /// チェック項目表示用プロパティ
        /// </summary>
        [ExcelColPosition(Col = 1)]
        public string CheckItem { get; set; }

        /// <summary>
        /// 以下、○日のデータを取得、出力用プロパティ
        /// (例)ItemsDay_1…1日目のデータを取得、出力
        /// </summary>
        [ExcelColPosition(Col = 2)]
        public string ItemsDay_1 { get; set; }
        /// <summary>
        /// 2日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 3)]
        public string ItemsDay_2 { get; set; }
        /// <summary>
        /// 3日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 4)]
        public string ItemsDay_3 { get; set; }
        /// <summary>
        /// 4日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 5)]
        public string ItemsDay_4 { get; set; }
        /// <summary>
        /// 5日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 6)]
        public string ItemsDay_5 { get; set; }
        /// <summary>
        /// 6日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 7)]
        public string ItemsDay_6 { get; set; }
        /// <summary>
        /// 7日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 8)]
        public string ItemsDay_7 { get; set; }
        /// <summary>
        /// 8日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 9)]
        public string ItemsDay_8 { get; set; }
        /// <summary>
        /// 9日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 10)]
        public string ItemsDay_9 { get; set; }
        /// <summary>
        /// 10日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 11)]
        public string ItemsDay_10 { get; set; }
        /// <summary>
        /// 11日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 12)]
        public string ItemsDay_11 { get; set; }
        /// <summary>
        /// 12日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 13)]
        public string ItemsDay_12 { get; set; }
        /// <summary>
        /// 13日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 14)]
        public string ItemsDay_13 { get; set; }
        /// <summary>
        /// 14日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 15)]
        public string ItemsDay_14 { get; set; }
        /// <summary>
        /// 15日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 16)]
        public string ItemsDay_15 { get; set; }
        /// <summary>
        /// 16日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 17)]
        public string ItemsDay_16 { get; set; }
        /// <summary>
        /// 17日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 18)]
        public string ItemsDay_17 { get; set; }
        /// <summary>
        /// 18日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 19)]
        public string ItemsDay_18 { get; set; }
        /// <summary>
        /// 19日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 20)]
        public string ItemsDay_19 { get; set; }
        /// <summary>
        /// 20日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 21)]
        public string ItemsDay_20 { get; set; }
        /// <summary>
        /// 21日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 22)]
        public string ItemsDay_21 { get; set; }
        /// <summary>
        /// 22日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 23)]
        public string ItemsDay_22 { get; set; }
        /// <summary>
        /// 23日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 24)]
        public string ItemsDay_23 { get; set; }
        /// <summary>
        /// 24日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 25)]
        public string ItemsDay_24 { get; set; }
        /// <summary>
        /// 25日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 26)]
        public string ItemsDay_25 { get; set; }
        /// <summary>
        /// 26日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 27)]
        public string ItemsDay_26 { get; set; }
        /// <summary>
        /// 27日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 28)]
        public string ItemsDay_27 { get; set; }
        /// <summary>
        /// 28日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 29)]
        public string ItemsDay_28 { get; set; }
        /// <summary>
        /// 29日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 30)]
        public string ItemsDay_29 { get; set; }
        /// <summary>
        /// 30日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 31)]
        public string ItemsDay_30 { get; set; }
        /// <summary>
        /// 31日分入力データ
        /// </summary>
        [ExcelColPosition(Col = 32)]
        public string ItemsDay_31 { get; set; }

    }
}
