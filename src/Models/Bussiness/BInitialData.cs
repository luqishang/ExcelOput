using HACCPExtender.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HACCPExtender.Models.Bussiness
{
    public class BInitialData
    {
        // フォーマットデータ店舗ID
        public string FormatShopId { get; set; }

        // 大分類マスタデータ
        public List<CategoryM> CategoryMDatas { get; set; }

        // 中分類マスタデータ
        public List<LocationM> LocationMDatas { get; set; }

        // 設問マスタデータ
        public List<QuestionMData> QuestionMDatas { get; set; }

    }
}