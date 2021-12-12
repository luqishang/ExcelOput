using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HACCPExtender.Models.Custom
{
    /// <summary>
    /// 帳票ファイル名 Model
    /// author : PTJ cheng
    /// Create Date : 2020/10/20
    /// </summary>
    public class CustomMiddleApproval
    {
        public string ReportFileName { get; set; }

        public string ReportFilePass { get; set; }
    }
}