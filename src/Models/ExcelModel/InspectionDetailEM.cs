using OfficePositionAttributes;

namespace HACCPExtender.Models.ExcelModel
{
    /// <summary>
    /// 検収記録の明細 内容のモデル
    /// author : PTJ.張
    /// Create Date : 2020/09/16
    /// </summary>
    public class InspectionDetailEM
    {
        /// <summary>
        /// 行を変更するためのインデックス
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// 受入時間(hh:mm)
        /// </summary>
        [ExcelColPosition(Col = 1)]
        public string AcceptanceTime { get; set; }

        /// <summary>
        /// 設問結果1
        /// </summary>
        [ExcelColPosition(Col = 2)]
        public string Result1 { get; set; }

        /// <summary>
        /// 設問結果2
        /// </summary>
        [ExcelColPosition(Col = 3)]
        public string Result2 { get; set; }

        /// <summary>
        /// 設問結果3
        /// </summary>
        [ExcelColPosition(Col = 4)]
        public string Result3 { get; set; }

        /// <summary>
        /// 設問結果4
        /// </summary>
        [ExcelColPosition(Col = 5)]
        public string Result4 { get; set; }

        /// <summary>
        /// 設問結果5
        /// </summary>
        [ExcelColPosition(Col = 6)]
        public string Result5 { get; set; }

        /// <summary>
        /// 設問結果6
        /// </summary>
        [ExcelColPosition(Col = 7)]
        public string Result6 { get; set; }

        /// <summary>
        /// 設問結果7
        /// </summary>
        [ExcelColPosition(Col = 8)]
        public string Result7 { get; set; }

        /// <summary>
        /// 設問結果8
        /// </summary>
        [ExcelColPosition(Col = 9)]
        public string Result8 { get; set; }

        /// <summary>
        /// 設問結果9
        /// </summary>
        [ExcelColPosition(Col = 10)]
        public string Result9 { get; set; }

        /// <summary>
        /// 設問結果10
        /// </summary>
        [ExcelColPosition(Col = 11)]
        public string Result10 { get; set; }

        /// <summary>
        /// 設問結果11
        /// </summary>
        [ExcelColPosition(Col = 12)]
        public string Result11 { get; set; }

        /// <summary>
        /// 設問結果12
        /// </summary>
        [ExcelColPosition(Col = 13)]
        public string Result12 { get; set; }

        /// <summary>
        /// 設問結果13
        /// </summary>
        [ExcelColPosition(Col = 14)]
        public string Result13 { get; set; }

        /// <summary>
        /// 設問結果14
        /// </summary>
        [ExcelColPosition(Col = 15)]
        public string Result14 { get; set; }

        /// <summary>
        /// 設問結果15
        /// </summary>
        [ExcelColPosition(Col = 16)]
        public string Result15 { get; set; }

        /// <summary>
        /// 設問結果16
        /// </summary>
        [ExcelColPosition(Col = 17)]
        public string Result16 { get; set; }

        /// <summary>
        /// 設問結果17
        /// </summary>
        [ExcelColPosition(Col = 18)]
        public string Result17 { get; set; }

        /// <summary>
        /// 設問結果18
        /// </summary>
        [ExcelColPosition(Col = 19)]
        public string Result18 { get; set; }

        /// <summary>
        /// 設問結果19
        /// </summary>
        [ExcelColPosition(Col = 20)]
        public string Result19 { get; set; }

        /// <summary>
        /// 設問結果20
        /// </summary>
        [ExcelColPosition(Col = 21)]
        public string Result20 { get; set; }

        /// <summary>
        /// 記録者
        /// </summary>
        [ExcelColPosition(Col = 22)]
        public string WorkerName { get; set; }

        /// <summary>
        /// 備考
        /// </summary>
        [ExcelColPosition(Col = 23)]
        public string Remarks { get; set; }

    }
}