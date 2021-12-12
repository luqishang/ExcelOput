using System;
using System.Collections.Generic;

namespace HACCPExtender.Models
{
    /// <summary>
    /// 画面データ（温度衛生管理情報）
    /// </summary>
    public class BTemperatureControl
    {
        public string ApprovalId { get; set; }
        
        public string WorkerId { get; set; }
        
        public string WorkerName { get; set; }
        
        public string DataYMD { get; set; }

        // ヘッダー
        public Dictionary<int, string> HeaderDic { get; set; }

        // 回答
        public Dictionary<int, string> AnserDic { get; set; }

        // 添付有無
        public Dictionary<int, bool> AttachDic { get; set; }

        public Int16 REMARKSNO { get; set; }

        public string REPORTFILENAME { get; set; }

        public string REPORTFILEPASS { get; set; }

    }
}