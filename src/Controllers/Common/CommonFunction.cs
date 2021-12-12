using HACCPExtender.ExcelOutput;
using HACCPExtender.Models;
using HACCPExtender.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers.Common
{
    public class CommonFunction
    {
        /// <summary>
        /// ルートディレクトリ取得
        /// </summary>
        /// <param name="httpContext">基底クラス</param>
        /// <returns></returns>
        public string GetURLRoot(HttpContextBase httpContext)
        {
            var scheme = httpContext.Request.Url.Scheme;
            var authority = httpContext.Request.Url.Authority;
            var path = HttpRuntime.AppDomainAppVirtualPath;
            return string.Format("{0}://{1}{2}", scheme, authority, path);
        }

        /// <summary>
        /// ボタン活性化の判定
        /// </summary>
        /// <param name="displayMode">編集モード</param>
        /// <returns></returns>
        public string GetEditButton(string displayMode)
        {
            if (ManagerLoginMode.LOGIN_NONE.Equals(displayMode))
            {
                return "disabled";
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetDataRecording(string str)
        {
            if (str.Length < 14)
            {
                return string.Empty;
            }

            string ymd = str.Substring(0, 8);
            string time = str.Substring(8, 6);
            return this.FormatDateStr(ymd) + " " + FormatTime(time);
        }

        /// <summary>
        /// 日付文字列を変換（YYYYMMDD→YYYY/MM/DD)
        /// </summary>
        /// <param name="str">変換前文字列</param>
        /// <returns>変換後文字列</returns>
        public string FormatDateStr(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return str.Substring(0, 4) + "/" +
                                str.Substring(4, 2) + "/" +
                                str.Substring(6, 2);
        }

        /// <summary>
        /// 日付文字列を変換（YYYYMMDD→YYYY-MM-DD)
        /// </summary>
        /// <param name="str">変換前文字列</param>
        /// <returns>変換後文字列</returns>
        public string FormatDateStrhyphen(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return str.Substring(0, 4) + "-" +
                                str.Substring(4, 2) + "-" +
                                str.Substring(6, 2);
        }

        /// <summary>
        /// 時間文字列を変換（HHmm→HH:mm)
        /// </summary>
        /// <param name="str">変換前文字列</param>
        /// <returns>変換後文字列</returns>
        public string FormatTime(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return str.Substring(0, 2) + ":" +
                                str.Substring(2, 2);
        }

        /// <summary>
        /// 時間文字列を変換（HH:mm→HHmm)
        /// </summary>
        /// <param name="str">変換前文字列</param>
        /// <returns>変換後文字列</returns>
        public string FormatTimeStr(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return str.Replace(":", "");
        }

        /// <summary>
        /// 周期画面表示文言取得
        /// </summary>
        /// <param name="period">周期ID</param>
        /// <returns>画面表示用周期</returns>
        public string GetPeriodDisp(string period)
        {
            string periodStr = string.Empty;

            if (PERIOD.ONEDAY.Equals(period))
            {
                periodStr = PERIOD.ONEDAY_W;
            }
            else if (PERIOD.ONEWEEK.Equals(period))
            {
                periodStr = PERIOD.ONEWEEK_W;
            }
            else if (PERIOD.ONEMONTH.Equals(period))
            {
                periodStr = PERIOD.ONEMONTH_W;
            }
            else if (PERIOD.THREEMONTH.Equals(period))
            {
                periodStr = PERIOD.ONEMONTH_W;
            }
            else if (PERIOD.SIXMONTH.Equals(period))
            {
                periodStr = PERIOD.SIXMONTH_W;
            }

            return periodStr;
        }

        public string GetApprovalNodeDisp(string approvalNode)
        {
            string nodeStr = string.Empty;

            if (APPROVALLEVEL.MIDDLE.Equals(approvalNode))
            {
                nodeStr = ApprovalCategory.NODE_CLASS_MIDDLE;

            } else if (APPROVALLEVEL.MAJORE.Equals(approvalNode))
            {
                nodeStr = ApprovalCategory.NODE_CLASS_MAJOR;
            
            } else if (APPROVALLEVEL.FACILITY.Equals(approvalNode))
            {
                nodeStr = ApprovalCategory.NODE_CLASS_FACILITY;
            }

            return nodeStr;
        }

        /// <summary>
        /// 場所マスタドロップダウンリスト用選択オプション生成
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SelectListItem[] CreateLocationMOptionList(List<LocationM> locationMList)
        {
            SelectListItem[] selectOptions = new SelectListItem[locationMList.Count()];
            int key = 0;
            locationMList.ForEach(a => {
                selectOptions[key] = new SelectListItem() { Value = a.LOCATIONID, Text = a.LOCATIONNAME };
                key++;
            });

            return selectOptions;
        }

        /// <summary>
        /// 周期ドロップダウンリスト作成
        /// </summary>
        /// <param name="period">周期ID</param>
        /// <returns>周期ドロップダウンリスト</returns>
        public IEnumerable<SelectListItem> GetPeriodDropList(string period)
        {
            return new SelectList(
                        new SelectListItem[] {
                            new SelectListItem() { Value=PERIOD.ONEDAY, Text=PERIOD.ONEDAY_W },
                            new SelectListItem() { Value=PERIOD.ONEWEEK, Text=PERIOD.ONEWEEK_W },
                            new SelectListItem() { Value=PERIOD.ONEMONTH, Text=PERIOD.ONEMONTH_W },
                            new SelectListItem() { Value=PERIOD.THREEMONTH, Text=PERIOD.THREEMONTH_W },
                            new SelectListItem() { Value=PERIOD.SIXMONTH, Text=PERIOD.SIXMONTH_W },
                        },
                        "Value",
                        "Text",
                        period
                    );
        }

        /// <summary>
        /// 指定された文字列がメールアドレスとして正しい形式か検証する
        /// </summary>
        /// <param name="address">検証する文字列</param>
        /// <returns>正しい時はTrue。正しくない時はFalse。</returns>
        public bool IsValidMailAddress(string address)
        {
            try
            {
                System.Net.Mail.MailAddress a =
                    new System.Net.Mail.MailAddress(address);
            }
            catch (FormatException ex)
            {
                //FormatExceptionがスローされた時は、正しくない
                LogHelper.Default.WriteError(ex.Message, ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 大分類ドロップダウンリスト用選択オプション生成
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public SelectListItem[] CreateCategoryMOptionList(List<CategoryM> categoryMList)
        {
            SelectListItem[] selectOptions = new SelectListItem[categoryMList.Count()];
            int key = 0;
            categoryMList.ForEach(a => {
                selectOptions[key] = new SelectListItem() { Value = a.CATEGORYID, Text = a.CATEGORYNAME };
                key++;
            });

            return selectOptions;
        }

        /// <summary>
        /// ドロップダウンリスト用選択オプション生成
        /// </summary>
        /// <param name="">list</param>
        /// <returns>SelectListItem[]</returns>
        public SelectListItem[] CreateOptionList(Dictionary<string, string> dictionary)
        {
            SelectListItem[] selectOptions = new SelectListItem[dictionary.Count];
            int key = 0;
            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                selectOptions[key] = new SelectListItem() { Value = kvp.Key, Text = kvp.Value };
                key++;
            }

            return selectOptions;
        }

        /// <summary>
        /// 帳票インタフェース設定
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="updUserId">更新者ID</param>
        /// <param name="reportTemplateUnitDt">帳票出力インタフェース</param>
        /// <param name="registryPath">帳票格納パス</param>
        /// <returns></returns>
        public List<CustomReportInterfaceM> SetReportInterface(string shopId, string updUserId, List<BReportInterface> reportTemplateUnitDt, string registryPath)
        {
            var customReportList = new List<CustomReportInterfaceM>();

            if (reportTemplateUnitDt.Count() > 0)
            {
                var templateUnitDt = reportTemplateUnitDt.OrderBy(a => a.ReportTemplateId);
                var customReport = new CustomReportInterfaceM();
                string reportTemplateId = string.Empty;
                foreach (BReportInterface bReportInterface in templateUnitDt)
                {
                    // 前回ループの帳票テンプレートIDと同じかつ、マージ単位が大分類の場合
                    if (reportTemplateId.Equals(bReportInterface.ReportTemplateId)
                        && ReportMergeUnit.UNIT_MAJOR.Equals(bReportInterface.ReportMergeUnit))
                    {
                        if (customReport.ReportList == null)
                        {
                            // 店舗ID
                            customReport.ShopId = shopId;
                            // 更新者ID
                            customReport.ManageId = updUserId;
                            // 部門ID
                            customReport.CategoryId = bReportInterface.CategoryId;
                            // 周期
                            customReport.Period = bReportInterface.Period;
                            // 周期開始日
                            customReport.PeriodStart = bReportInterface.PeriodStart;
                            // 帳票タイトル
                            customReport.Title = bReportInterface.Title;
                            // 帳票テンプレートID
                            customReport.TemplateID = bReportInterface.ReportTemplateId;
                            // 帳票リスト
                            customReport.ReportList = new List<CustomReportM>();
                            // パス
                            customReport.Path = registryPath;
                        }
                        // 帳票リストに追加
                        var reportlist = new CustomReportM
                        {
                            ReportId = bReportInterface.ReportId,
                            LocationId = bReportInterface.LocationId
                        };
                        customReport.ReportList.Add(reportlist);
                    }
                    else
                    {
                        // 前回ループ分のデータをリストへ格納
                        if (customReport.ReportList != null && customReport.ReportList.Count() > 0)
                        {
                            customReportList.Add(customReport);
                        }
                        // ベースになる帳票テンプレートIDを設定
                        reportTemplateId = bReportInterface.ReportTemplateId;

                        customReport = new CustomReportInterfaceM
                        {
                            // 店舗ID
                            ShopId = shopId,
                            // 部門ID
                            CategoryId = bReportInterface.CategoryId,
                            // 周期
                            Period = bReportInterface.Period,
                            // 周期開始日
                            PeriodStart = bReportInterface.PeriodStart,
                            // 帳票タイトル
                            Title = bReportInterface.Title,
                            // 帳票テンプレートID
                            TemplateID = bReportInterface.ReportTemplateId,
                            // 帳票リスト
                            ReportList = new List<CustomReportM>(),
                            // パス
                            Path = registryPath
                        };

                        // 帳票リストに追加
                        var reportlist = new CustomReportM
                        {
                            ReportId = bReportInterface.ReportId,
                            LocationId = bReportInterface.LocationId
                        };
                        customReport.ReportList.Add(reportlist);
                    }
                }

                // 最終ループ分のデータをリストへ格納
                customReportList.Add(customReport);
            }

            return customReportList;
        }

        /// <summary>
        /// 帳票ファイル出力処理
        /// </summary>
        /// <param name="customReportList">帳票出力インタフェース</param>
        /// <param name="reportTempMDt">帳票テンプレートマスタ</param>
        /// <param name="curPath">出力パス</param>
        public void CallReportOutputPdf(List<CustomReportInterfaceM> customReportList, IQueryable<ReportTemplateM> reportTempMDt)
        {
            if (customReportList == null || customReportList.Count() == 0)
            {
                return;
            }

            foreach (CustomReportInterfaceM interfaceM in customReportList)
            {
                string tenplateType = string.Empty;
                if (reportTempMDt != null && reportTempMDt.Count() > 0)
                {
                    var reportTemp = reportTempMDt.Where(a => a.TEMPLATEID == interfaceM.TemplateID).FirstOrDefault();
                    if (reportTemp != null)
                    {
                        tenplateType = reportTemp.REPORTTEMPLATETYPE;
                    }
                }
                if ("01".Equals(tenplateType))
                {
                    ExcelPattern_1 excel1 = new ExcelPattern_1();
                    bool ret1 = excel1.OutPDF(interfaceM);

                }
                else if ("02".Equals(tenplateType))
                {
                    ExcelPattern_2 excel2 = new ExcelPattern_2();
                    bool ret1 = excel2.OutPDF(interfaceM);

                }
                else if ("03".Equals(tenplateType))
                {
                    ExcelPattern_3 excel3 = new ExcelPattern_3();
                    bool ret1 = excel3.OutPDF(interfaceM);

                }
                else if ("04".Equals(tenplateType))
                {
                    ExcelPattern_4 excel4 = new ExcelPattern_4();
                    bool ret1 = excel4.OutPDF(interfaceM);
                }
                else if ("05".Equals(tenplateType))
                {
                    ExcelPattern_5 excel5 = new ExcelPattern_5();
                    bool ret1 = excel5.OutPDF(interfaceM);
                }
            }

        }
    }
}