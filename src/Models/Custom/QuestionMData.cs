namespace HACCPExtender.Models.Custom
{
    public class QuestionMData
    {
        // 大分類名
        public string CATEGORYNAME { get; set; }
        // 中分類名
        public string LOCATIONNAME { get; set; }
        // 帳票名
        public string REPORTNAME { get; set; }
        // 設問ID
        public string QUESTIONID { get; set; }
        // 設問
        public string QUESTION { get; set; }
        // 回答種類名
        public string ANSWERTYPENAME { get; set; }
        // 基準判定条件
        public string NORMALCONDITION_NAME { get; set; }
        // 基準値１
        public string NORMALCONDITION1 { get; set; }
        // 基準値２
        public string NORMALCONDITION2 { get; set; }
    }
}