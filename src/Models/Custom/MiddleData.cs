using System;

namespace HACCPExtender.Models.Bussiness
{
    public class MiddleData
    {
        public string SHOPID { get; set; }

        public string CATEGORYID { get; set; }

        public string CATEGORYNAME { get; set; }

        public short CATEGORYORDER { get; set; }

        public string LOCATIONID { get; set; }

        public string LOCATIONNAME { get; set; }

        public short LOCATIONORDER { get; set; }

        public string REPORTID { get; set; }

        public string REPORTNAME { get; set; }

        public string REPORTTEMPLATEID { get; set; }

        public short REPORTORDER { get; set; }

        public string PERIOD { get; set; }

        public string PERIODWORD { get; set; }

        public string PERIODSTART { get; set; }

        public string PERIODEND { get; set; }

        public string PERIODSTARTDATE { get; set; }

        public string PERIODENDDATE { get; set; }

        public string REPORTFILENAME { get; set; }

        public string REPORTFILEPASS { get; set; }

        public Int32 CNT { get; set; }

        // 0:承認待、1：差戻、2：承認依頼未完了
        public string MODE { get; set; }

        public string STATUS { get; set; }

        public string APPROVALID { get; set; }

        public string WORKERID { get; set; }

        public string WORKERNAME { get; set; }

        public string DATAYMD { get; set; }

        public Boolean DataChk { get; set; }

        public string MIDDLESNNDATE { get; set; }

        public short MIDDLEGROUPNO { get; set; }

        public string APPROVALNODE { get; set; }

        public string APPROVALNODEDISP { get; set; }

        public string MIDDLESNNCOMMENT { get; set; }

        public string UPDDATE { get; set; }

        // Cloneメソッドを使用
        public MiddleData Clone()
        {
            // Object型で返ってくるのでキャストが必要
            return (MiddleData)MemberwiseClone();
        }

    }
}