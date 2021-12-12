using System;

namespace HACCPExtender.Models
{
    /// <summary>
    /// データモデル（お知らせ情報）
    /// </summary>
    public class NotificationT
    {
        public string NOTICEID { get; set; }

        public string NOTICECONTENT { get; set; }

        public string STARTDATE { get; set; }

        public string STARTTIME { get; set; }

        public string INSUSERID { get; set; }

        public string UPDUSERID { get; set; }

        public DateTime UPDDATE { get; set; }

    }
}