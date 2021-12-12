using HACCPExtender.Business;
using HACCPExtender.Controllers.API;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.API;
using HACCPExtender.Models.Custom;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class RecordedDataEditController : AsyncController
    {
        public async Task EditRecordedData(List<DataCooperation> cooperationList)
        {
            await Task.Run(() =>
            {
                foreach (DataCooperation cooperation in cooperationList)
                {
                    this.SetTemperature(cooperation);
                }
            });

            return;
        }

        /// <summary>
        /// 温度衛生管理データ登録、承認処理実行
        /// </summary>
        /// <param name="cooperation">データ記録インプット</param>
        /// <returns>処理結果</returns>
        private bool SetTemperature(DataCooperation cooperation)
        {
            // Masterコンテキスト
            var context = new MasterContext();
            var commF = new CommonFunction();
            var masterF = new MasterFunction();

            // 店舗ID
            string shopId = cooperation.ShopNO;
            // 部門ID
            string categoryId = cooperation.CATEGORYID;
            // 場所ID
            string locationId = cooperation.LOCATIONID;
            // 帳票ID
            string reportId = cooperation.REPORTID;
            // 測定時間
            string recordDay = cooperation.DTIME.ToString("yyyy/MM/dd");

            // 周期
            string period = string.Empty;
            // 基準月
            string baseMonth = string.Empty;
            // 基準日(曜日)
            string baseDay = string.Empty;
            // 捺印数
            int stampField = 0;

            // 帳票マスタ
            var reportMDt = from re in context.ReportMs
                            where re.SHOPID == shopId
                             && re.REPORTID == reportId
                            select re;
            if (reportMDt.FirstOrDefault() != null)
            {
                period = reportMDt.FirstOrDefault().PERIOD;
                baseMonth = reportMDt.FirstOrDefault().BASEMONTH;
                baseDay = reportMDt.FirstOrDefault().REFERENCEDATE;
                stampField = reportMDt.FirstOrDefault().STAMPFIELD;
            }

            // 帳票IDから周期、基準日を取得
            var periodDay = this.GetPeriodStartEnd(period, baseMonth, baseDay, recordDay);

            // 温度衛生管理 登録エンティティを作成
            var temperature = new TemperatureControlT
            {
                // 店舗ID
                SHOPID = shopId,
                // 部門ID
                CATEGORYID = categoryId,
                // 場所ID
                LOCATIONID = locationId,
                // 帳票ID
                REPORTID = reportId,
                // 承認ID
                APPROVALID = cooperation.APPROVALID,
                // 作業者ID
                WORKERID = cooperation.WCD,
                // 作業者名称
                WORKERNAME = cooperation.WNM,
                // データ記録年月日
                DATAYMD = cooperation.DTIME.ToString("yyyyMMddHHmmss"),
                // 周期
                PERIOD = period,
                // 周期期間（自）
                PERIODSTART = periodDay.startDay,
                // 周期期間（至）
                PERIODEND = periodDay.endDay,
                // 登録ユーザーID
                INSUSERID = APIConstants.API_USER,
                // 更新ユーザーID
                UPDUSERID = APIConstants.API_USER
            };

            // 設問マスタ
            var questionDt = from qa in context.QuestionMs
                             orderby qa.DISPLAYNO
                             where qa.SHOPID == shopId
                                 && qa.REPORTID == reportId
                                 && qa.DELETEFLAG == DeleteFlg.NODELETE
                             select qa;
            if (questionDt.Count() == 0 || questionDt.FirstOrDefault() == null)
            {
                // ログ出力
                var msg = new StringBuilder();
                msg.Append("SHOPID=");
                msg.Append(shopId);
                msg.Append(", REPORTID=");
                msg.Append(reportId);
                LogHelper.Default.WriteError("データ記録の回答に該当する設問マスタデータなし: " + msg.ToString());
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

            // 回答種類マスタ
            var answerType = from answer in context.AnswerTypeMs
                             select answer;
            var shop_answerType = from answer in context.Shop_AnswerTypeMs
                                  where answer.SHOPID == shopId
                                  select answer;
            if ((answerType.Count() == 0 || answerType.FirstOrDefault() == null) && (shop_answerType.Count() == 0 || shop_answerType.FirstOrDefault() == null))
            {
                // ログ出力
                LogHelper.Default.WriteError("回答種類マスタデータ件数 = 0件 ");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

            // 異常データ情報リスト
            var abnormalList = new List<AbnormalJoho>();
            // 温度衛生管理エンティティ
            var tempControlT = new TemperatureControlT();
            // 画像ファイル ルートディレクトリ
            string rootDic = masterF.GetImageFolderName(context, shopId).Replace("~/", "");

            // カウンタ
            int count = 1;
            // 備考NO
            int bikoNo = 0;
            // 設問マスタの数分ループ
            foreach (QuestionM question in questionDt)
            {
                // 設問マスタ 設問 → 温度衛生管理 設問
                string questionStr = question.QUESTION;
                string questionColumn = string.Format("QUESTION{0}", count.ToString());
                var qProperty = temperature.GetType().GetProperty(questionColumn);
                qProperty.SetValue(temperature, questionStr);

                // データ記録連携情報 回答 → 温度衛生管理 回答
                var answerColumn = string.Format("US{0}", count.ToString());
                var aProperty = typeof(DataCooperation).GetProperty(answerColumn);
                var answer = (string)aProperty.GetValue(cooperation);
                var resultColumn = string.Format("RESULT{0}", count.ToString());
                var rProperty = temperature.GetType().GetProperty(resultColumn);
                rProperty.SetValue(temperature, answer);

                // 回答種類
                string answeTypeId = question.ANSWERTYPEID;
                // 正常値条件
                string condition = question.NORMALCONDITION;

                // 回答種類マスタ 回答区分
                string answeKbn = string.Empty;
                var kbnDt = answerType.Where(a => a.ANSWERTYPEID == answeTypeId).FirstOrDefault();
                var shop_kbnDt = shop_answerType.Where(a => a.ANSWERTYPEID == answeTypeId).FirstOrDefault();
                if (shop_kbnDt != null) //店舗別マスタを優先
                {
                    answeKbn = shop_kbnDt.ANSWERKBN;
                }
                else if (kbnDt != null)
                {
                    answeKbn = kbnDt.ANSWERKBN;
                }

                // 異常データ判定
                bool abnormalFlg = this.CheckAbnormalData(
                    answer,
                    answeKbn,
                    question.NORMALCONDITION,
                    question.NORMALCONDITION1,
                    question.NORMALCONDITION2,
                    cooperation.E1L,
                    cooperation.E1H,
                    cooperation.DTIME);

                // 異常データの場合
                if (!abnormalFlg)
                {
                    var abnormal = new AbnormalJoho
                    {
                        No = count,
                        Question = questionStr,
                        Answer = answer
                    };

                    abnormalList.Add(abnormal);
                }

                // 回答種類別による設定
                if (AnsweTypeKbn.CAMERA.Equals(answeKbn) || AnsweTypeKbn.VIDEO.Equals(answeKbn))
                {
                    // 添付ファイルパスの生成

                    string imageFileName = cooperation.CAM;
                    if (!string.IsNullOrEmpty(imageFileName))
                    {
                        // ファイルパス（ルート配下）+ファイル名
                        string filePath = rootDic + "/" + imageFileName;

                        // 設問結果添付ファイルへ設定
                        var tempColumn = string.Format("RESULTATTACHMENT{0}", count.ToString());
                        var tProperty = temperature.GetType().GetProperty(tempColumn);
                        tProperty.SetValue(temperature, filePath);
                    }
                }
                else if (AnsweTypeKbn.TEXTAREA.Equals(answeKbn))
                {
                    // テキストエリア = 備考

                    bikoNo = count;
                    // 備考設問NO
                    temperature.REMARKSNO = (short)count;
                }

                count++;
            }

            // 承認ルートの承認経路情報を取得
            var approvalRoute = this.GetApprovalRoute(context, stampField, shopId, categoryId, locationId);
            // メール送信機能
            var sender = new MailSenderFunction();
            // 承認者のメールアドレスリストを作成
            List<MailInfo> approvelarMailToList = sender.SetMailAddress(context, approvalRoute);


            // DB登録処理 温度衛生管理情報へ登録
            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // データ登録
                        context.TemperatureControlTs.Add(temperature);
                        context.SaveChanges();
                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                    catch (Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                }
            }

            // 記録データの異常データを検出
            if (abnormalList.Count() > 0)
            {
                StringBuilder strb = new StringBuilder();

                foreach (AbnormalJoho abJh in abnormalList)
                {
                    if (abJh.No == bikoNo)
                    {
                        if (!string.IsNullOrEmpty(abJh.Answer))
                        {
                            // 備考の場合
                            strb.AppendLine("");
                            strb.Append("備考：");
                            strb.AppendLine(abJh.Answer);
                        }
                    }
                    else
                    {
                        // 異常データの設問・回答
                        strb.Append("設問");
                        strb.Append(abJh.No);
                        strb.Append("：");
                        strb.AppendLine(abJh.Question);
                        strb.Append("入力内容");
                        strb.Append(abJh.No);
                        strb.Append("：");
                        strb.AppendLine(abJh.Answer);
                    }
                }

                using (context = new MasterContext())
                {
                    // メール送信設定管理者
                    var managerMailToList = new List<MailInfo>();
                    // 異常メール送信情報リスト
                    var abnormalMailToList = new List<MailInfo>();

                    // メール送信対象となる管理者を取得
                    var CategoryManager = this.GetManager(context, shopId, categoryId);
                    if (CategoryManager.Count() > 0)
                    {
                        foreach (WorkerM wk in CategoryManager)
                        {
                            // メール送信時間から送信対象か判定
                            if (this.ChkMailSendManager(cooperation.DTIME, wk.TRANSMISSIONTIME1, wk.TRANSMISSIONTIME2))
                            {
                                var info = new MailInfo
                                {
                                    WorkerId = wk.WORKERID,
                                    WorkerName = wk.WORKERNAME,
                                    MailAddress = wk.MAILADDRESSPC,
                                    MailAddressFeature = wk.MAILADDRESSFEATURE
                                };

                                managerMailToList.Add(info);
                            }
                        }

                        // 異常メール送信情報リストを作成
                        if (managerMailToList.Count() > 0)
                        {
                            abnormalMailToList.AddRange(managerMailToList);
                        }
                    }
                    // 承認者のメールアドレスを追加
                    if (approvelarMailToList.Count() > 0)
                    {
                        abnormalMailToList.AddRange(approvelarMailToList);
                    }

                    // メール送信
                    if (abnormalMailToList.Count() > 0)
                    {
                        // メールタイトル作成
                        var titleStr = GetAppSet.GetAppSetValue("AbnormalDataWarning", "Subject");
                        var systemName = GetAppSet.GetAppSetValue("Mail", "SYSTEMNAME");
                        string title = titleStr.Replace("%SYSTEMNAME%", systemName)
                                        .Replace("%CATEGORY%", cooperation.CATEGORYNM)
                                        .Replace("%LOCATION%", cooperation.LOCATIONNM);

                        var sendmail = new SendMailBusiness();
                        // メール本文作成 
                        string periodStartDay = commF.FormatDateStrhyphen(periodDay.startDay);
                        string URL = sendmail.SetURL(URLShoriKBN.DATAHISTORY, categoryId, locationId, reportId, period, periodStartDay, shopId);
                        string body = string.Empty;
                        var bodyStr = HostingEnvironment.MapPath(GetAppSet.GetAppSetValue("AbnormalDataWarning", "BodyTemplate"));
                        using (var sr = new StreamReader(bodyStr, Encoding.GetEncoding("shift_jis")))
                        {
                            // パラメータ（%～%）の置換
                            body = sr.ReadToEnd()
                                    .Replace("%SYSTEMNAME%", systemName)
                                    .Replace("%DATAYMD%", cooperation.DTIME.ToString("yyyy/MM/dd HH:mm:ss"))
                                    .Replace("%WORKERNAME%", cooperation.WNM)
                                    .Replace("%ABNORMALQA%", strb.ToString())
                                    .Replace("%URL%", URL);
                        }
                        // メール送信
                        sender.SendMail(
                            abnormalMailToList,
                            new List<MailInfo>(),
                            new List<MailInfo>(),
                            sender.GetSendMailAddress(),
                            title,
                            body,
                            null,   // 添付ファイル
                            GetAppSet.GetAppSetValue("AbnormalDataWarning", "ContentType"));
                    }
                }
            }

            var mailFunc = new SendMailBusiness();
            using (context = new MasterContext())
            {
                // 承認データ作成（メール送信、帳票出力）
                switch (stampField)
                {
                    case 3: // 中分類承認
                        this.SetMiddleApprovalData(temperature);
                        // 承認者へメール送信
                        mailFunc.SendMiddleRequestMail(context, approvelarMailToList, shopId, categoryId, locationId, reportId, period, periodDay.startDay);
                        break;

                    case 2: // 大分類承認
                        this.SetMajorApprovalData(temperature);
                        // 承認者へメール送信
                        mailFunc.SendMajorRequestMail(context, approvelarMailToList, shopId, categoryId, locationId, period, periodDay.startDay);
                        break;

                    case 1: // 施設承認
                        this.SetFacilityApprovalData(temperature);
                        // 承認者へメール送信
                        mailFunc.SendFacilityRequestMail(context, approvelarMailToList, shopId, categoryId, period, periodDay.startDay);
                        break;

                    default:
                        break;
                }

                // すべて正常に終了した場合はデータ記録連携情報を削除
                var coope = from co in context.DataCooperations
                            where co.ShopNO == temperature.SHOPID
                                && co.CATEGORYID == temperature.CATEGORYID
                                && co.LOCATIONID == temperature.LOCATIONID
                                && co.REPORTID == temperature.REPORTID
                                && co.APPROVALID == temperature.APPROVALID
                            select co;

                if (coope.Count() > 0 && coope.FirstOrDefault() != null)
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var dellData = coope.FirstOrDefault();
                            // データ削除
                            context.DataCooperations.Attach(dellData);
                            context.DataCooperations.Remove(dellData);
                            context.SaveChanges();
                            tran.Commit();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            // ロールバック
                            tran.Rollback();
                            LogHelper.Default.WriteError(ex.Message, ex);
                            throw new HttpResponseException(HttpStatusCode.InternalServerError);
                        }
                        catch (Exception ex)
                        {
                            // ロールバック
                            tran.Rollback();
                            LogHelper.Default.WriteError(ex.Message, ex);
                            throw new HttpResponseException(HttpStatusCode.InternalServerError);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 周期開始日・終了日の決定
        /// </summary>
        /// <param name="period">周期</param>
        /// <param name="baseMonth">基準月</param>
        /// <param name="baseDay">基準日（曜日）</param>
        /// <param name="dateTime">処理日（yyyy/MM/dd）</param>
        /// <returns>周期範囲</returns>
        private PeriodDay GetPeriodStartEnd(string period, string baseMonth, string baseDay, string shoriYMD)
        {
            var periodDay = new PeriodDay();

            if (string.IsNullOrEmpty(period) || string.IsNullOrEmpty(shoriYMD))
            {
                // ログ出力
                var msg = new StringBuilder();
                msg.Append("[period=");
                msg.Append(period);
                msg.Append(", shoriYMD=");
                msg.Append(shoriYMD);
                msg.Append("]");
                LogHelper.Default.WriteError("パラメータ必須チェックエラー: " + msg.ToString());
                return periodDay;
            }

            // 処理日
            DateTime tojitsu = DateTime.Parse(shoriYMD);

            if (PERIOD.ONEDAY.Equals(period))   // 1日
            {
                periodDay.startDay = shoriYMD.Replace("/", "");
                periodDay.endDay = shoriYMD.Replace("/", "");
            }
            else if (PERIOD.ONEWEEK.Equals(period))       // 1週間
            {
                // 基準曜日
                int baseYobi = OPTIONLIST_REFERENCEDATE_DAYOFWEEK[baseDay];
                int yobi = (int)tojitsu.DayOfWeek;

                if (baseYobi == yobi)   // 基準日 = 曜日
                {
                    periodDay.startDay = shoriYMD.Replace("/", "");
                    periodDay.endDay = tojitsu.AddDays(6).ToString("yyyyMMdd");

                }
                else if (baseYobi < yobi) // 基準日 < 曜日
                {
                    // 基準曜日 - 曜日の日数分だけ戻る
                    DateTime start = tojitsu.AddDays(-(yobi - baseYobi));
                    periodDay.startDay = start.ToString("yyyyMMdd");
                    periodDay.endDay = start.AddDays(6).ToString("yyyyMMdd");
                }
                else  // 曜日 < 基準日
                {
                    // 7+基準曜日 - 曜日の日数分だけ戻る
                    DateTime start = tojitsu.AddDays(-(7 + yobi - baseYobi));
                    periodDay.startDay = start.ToString("yyyyMMdd");
                    periodDay.endDay = start.AddDays(6).ToString("yyyyMMdd");
                }
            }
            else if (PERIOD.ONEMONTH.Equals(period))      // 1ヶ月
            {
                // 処理日の月末日
                DateTime dt = tojitsu.AddMonths(1);
                DateTime lastDay = new DateTime(dt.Year, dt.Month, 1).AddDays(-1);
                // 処理日の前月末日
                DateTime lastMonthDay = new DateTime(lastDay.Year, lastDay.Month, 1).AddDays(-1);

                // 月末の場合
                if ("0".Equals(baseDay))
                {
                    if (lastDay.ToString("yyyyMMdd").Equals(tojitsu.ToString("yyyyMMdd")))
                    {
                        // 処理日から始まる
                        periodDay.startDay = lastMonthDay.ToString("yyyyMMdd");
                        // 次月の月末-1日
                        DateTime next2Dt = new DateTime(tojitsu.Year, tojitsu.Month, 1);
                        next2Dt = next2Dt.AddMonths(2).AddDays(-1);
                        periodDay.endDay = next2Dt.ToString("yyyyMMdd");
                    }
                    else
                    {
                        // 先月の月末から始まる
                        periodDay.startDay = lastMonthDay.ToString("yyyyMMdd");
                        periodDay.endDay = lastDay.AddDays(-1).ToString("yyyyMMdd");
                    }
                }
                else
                {
                    int day = tojitsu.Day;
                    int iBaseDay = int.Parse(baseDay);

                    if (iBaseDay == day)
                    {
                        // 処理日から始まる
                        periodDay.startDay = tojitsu.ToString("yyyyMMdd");
                        // 処理日から1ヶ月
                        periodDay.endDay = tojitsu.AddMonths(1).AddDays(-1).ToString("yyyyMMdd");
                    }
                    else if (iBaseDay < day)
                    {
                        // 基準日 < 処理日
                        // 当月の開始日
                        DateTime start = new DateTime(tojitsu.Year, tojitsu.Month, iBaseDay);
                        periodDay.startDay = start.ToString("yyyyMMdd");
                        // 開始日の1ヶ月後
                        periodDay.endDay = start.AddMonths(1).AddDays(-1).ToString("yyyyMMdd");
                    }
                    else
                    {
                        // 処理日 < 基準日

                        if (28 < iBaseDay)
                        {
                            // 前月の基準日が存在しない月(29日以降)を考える

                            int matsu = lastMonthDay.Day;
                            // 末日 < 基準日
                            if (matsu < iBaseDay)
                            {
                                // 前月の月末
                                periodDay.startDay = lastMonthDay.ToString("yyyyMMdd");
                            }
                            else
                            {
                                // 前月の基準日
                                periodDay.startDay = new DateTime(lastMonthDay.Year, lastMonthDay.Month, iBaseDay).ToString("yyyyMMdd");
                            }

                            if (lastDay.Day < iBaseDay)
                            {
                                // 今月の月末日
                                periodDay.endDay = lastDay.AddDays(-1).ToString("yyyyMMdd");
                            }
                            else
                            {
                                // 今月の基準日
                                periodDay.endDay = new DateTime(lastDay.Year, lastDay.Month, iBaseDay).AddDays(-1).ToString("yyyyMMdd");
                            }
                        }
                        else
                        {
                            periodDay.startDay = new DateTime(lastMonthDay.Year, lastMonthDay.Month, iBaseDay).ToString("yyyyMMdd");
                            periodDay.endDay = new DateTime(lastDay.Year, lastDay.Month, iBaseDay).AddDays(-1).ToString("yyyyMMdd");
                        }
                    }
                }
            }
            else if (PERIOD.THREEMONTH.Equals(period))    // 3ヶ月
            {
                int iBaseDay = int.Parse(baseDay);
                int iMonth = int.Parse(baseMonth);

                // 2月29日指定で処理日の年がうるう年ではない場合
                if (iMonth == 2 && iBaseDay == 29 && !DateTime.IsLeapYear(tojitsu.Year))
                {
                    iBaseDay = 28;
                }

                // 基準月日, 基準月日+3ヶ月
                DateTime perod1start = new DateTime(tojitsu.Year, iMonth, iBaseDay);
                DateTime perod1end = perod1start.AddMonths(3).AddDays(-1);
                // 基準月日+3ヶ月, 基準月日+6ヶ月
                DateTime perod2start = perod1start.AddMonths(3);
                DateTime perod2end = perod2start.AddMonths(3).AddDays(-1);
                // 基準月日+6ヶ月, 基準月日+9ヶ月
                DateTime perod3start = perod2start.AddMonths(3);
                DateTime perod3end = perod3start.AddMonths(3).AddDays(-1);
                // 基準月日+9ヶ月, 基準月日+12ヶ月
                DateTime perod4start = perod3start.AddMonths(3);
                DateTime perod4end = perod4start.AddMonths(3).AddDays(-1);

                // 基準月日-3ヶ月, 基準月日
                DateTime perod01start = perod1start.AddMonths(-3);
                DateTime perod01end = perod1start.AddDays(-1);
                // 基準月日-6ヶ月, 基準月日-3ヶ月
                DateTime perod02start = perod01start.AddMonths(-3);
                DateTime perod02end = perod01start.AddDays(-1);
                // 基準月日-9月, 基準月日-6ヶ月
                DateTime perod03start = perod02start.AddMonths(-3);
                DateTime perod03end = perod02start.AddDays(-1);
                // 基準月日-12月, 基準月日-9ヶ月
                DateTime perod04start = perod03start.AddMonths(-3);
                DateTime perod04end = perod03start.AddDays(-1);

                if (this.IsBetween(tojitsu, perod1start, perod1end))
                {
                    periodDay.startDay = perod1start.ToString("yyyyMMdd");
                    periodDay.endDay = perod1end.ToString("yyyyMMdd");
                }
                else if (this.IsBetween(tojitsu, perod2start, perod2end))
                {
                    periodDay.startDay = perod2start.ToString("yyyyMMdd");
                    periodDay.endDay = perod2end.ToString("yyyyMMdd");
                }
                else if (this.IsBetween(tojitsu, perod3start, perod3end))
                {
                    periodDay.startDay = perod3start.ToString("yyyyMMdd");
                    periodDay.endDay = perod3end.ToString("yyyyMMdd");
                }
                else if (this.IsBetween(tojitsu, perod4start, perod4end))
                {
                    periodDay.startDay = perod4start.ToString("yyyyMMdd");
                    periodDay.endDay = perod4end.ToString("yyyyMMdd");
                }
                else if (this.IsBetween(tojitsu, perod01start, perod01end))
                {
                    periodDay.startDay = perod01start.ToString("yyyyMMdd");
                    periodDay.endDay = perod01end.ToString("yyyyMMdd");
                }
                else if (this.IsBetween(tojitsu, perod02start, perod02end))
                {
                    periodDay.startDay = perod02start.ToString("yyyyMMdd");
                    periodDay.endDay = perod02end.ToString("yyyyMMdd");
                }
                else if (this.IsBetween(tojitsu, perod03start, perod03end))
                {
                    periodDay.startDay = perod03start.ToString("yyyyMMdd");
                    periodDay.endDay = perod03end.ToString("yyyyMMdd");
                }
                else
                {
                    periodDay.startDay = perod04start.ToString("yyyyMMdd");
                    periodDay.endDay = perod04end.ToString("yyyyMMdd");
                }
            }
            else if (PERIOD.SIXMONTH.Equals(period))      // 6ヶ月
            {
                int iBaseDay = int.Parse(baseDay);
                int iMonth = int.Parse(baseMonth);

                // 2月29日指定で処理日の年がうるう年ではない場合
                if (iMonth == 2 && iBaseDay == 29 && !DateTime.IsLeapYear(tojitsu.Year))
                {
                    iBaseDay = 28;
                }
                // 基準月日, 基準月日+6ヶ月 
                DateTime perod1start = new DateTime(tojitsu.Year, iMonth, iBaseDay);
                DateTime perod1end = perod1start.AddMonths(6).AddDays(-1);
                // 基準月日+6ヶ月, 基準月日+12ヶ月
                DateTime perod2start = perod1start.AddMonths(6);
                DateTime perod2end = perod2start.AddMonths(6).AddDays(-1);

                // 基準月日-6ヶ月, 基準月日
                DateTime perod01start = perod1start.AddMonths(-6);
                DateTime perod01end = perod1start.AddDays(-1);
                // 基準月日-12ヶ月, 基準月日-6ヶ月
                DateTime perod02start = perod01start.AddMonths(-6);
                DateTime perod02end = perod01start.AddDays(-1);

                if (this.IsBetween(tojitsu, perod1start, perod1end))
                {
                    periodDay.startDay = perod1start.ToString("yyyyMMdd");
                    periodDay.endDay = perod1end.ToString("yyyyMMdd");
                }
                else if (this.IsBetween(tojitsu, perod2start, perod2end))
                {
                    periodDay.startDay = perod2start.ToString("yyyyMMdd");
                    periodDay.endDay = perod2end.ToString("yyyyMMdd");
                }
                else if (this.IsBetween(tojitsu, perod01start, perod01end))
                {
                    periodDay.startDay = perod01start.ToString("yyyyMMdd");
                    periodDay.endDay = perod01end.ToString("yyyyMMdd");
                }
                else
                {
                    periodDay.startDay = perod02start.ToString("yyyyMMdd");
                    periodDay.endDay = perod02end.ToString("yyyyMMdd");
                }
            }

            return periodDay;
        }

        /// <summary>
        /// 周期範囲
        /// </summary>
        private struct PeriodDay
        {
            // 開始日（yyyyMMdd）
            public string startDay;
            // 終了日（yyyyMMdd）
            public string endDay;
        }

        /// <summary>
        /// 異常データ情報
        /// </summary>
        private struct AbnormalJoho
        {
            // 設問NO
            public int No;
            // 質問
            public string Question;
            // 回答
            public string Answer;
        }

        /// <summary>
        /// 周期範囲内判定
        /// </summary>
        /// <param name="self">処理日</param>
        /// <param name="from">周期開始日</param>
        /// <param name="to">周期終了日</param>
        /// <returns>判定結果</returns>
        private bool IsBetween(DateTime self, DateTime from, DateTime to)
        {
            return from <= self && to >= self;
        }

        /// <summary>
        /// 異常データ判定処理
        /// </summary>
        /// <param name="resultVal">回答</param>
        /// <param name="answerTypeId">回答種類区分</param>
        /// <param name="condition">正常基準条件</param>
        /// <param name="conditionv1">正常基準値１</param>
        /// <param name="conditionv2">正常基準値２</param>
        /// <param name="low">マスタ下限温度</param>
        /// <param name="high">マスタ上限温度</param>
        /// <returns>判定結果（false : 異常データ）</returns>
        private bool CheckAbnormalData(string resultVal, string answerTypeId, string condition, string conditionv1, string conditionv2, double low, double high, DateTime dTime)
        {
            // 判定結果
            bool result = true;

            // 回答種類区分 = ボタン、4分岐の場合
            if (AnsweTypeKbn.BUTTON.Equals(answerTypeId) || AnsweTypeKbn.BRANCH_4.Equals(answerTypeId))
            {
                // 文字列比較
                result = this.JudgeConditionStr(resultVal, condition, conditionv1);

            }
            else if (AnsweTypeKbn.ONDOKEI.Equals(answerTypeId)
                || AnsweTypeKbn.TAIONKEI.Equals(answerTypeId)
                || AnsweTypeKbn.DECIMAL.Equals(answerTypeId))
            {
                // 回答種類区分 = 温度計, 体温計, 少数
                result = this.JudgeConditionDecimal(resultVal, condition, conditionv1, conditionv2);

            }
            else if (AnsweTypeKbn.NUMERIC.Equals(answerTypeId))
            {
                //  回答種類区分 = 整数
                result = this.JudgeConditionNumeric(resultVal, condition, conditionv1, conditionv2);

            }
            else if (AnsweTypeKbn.SHOKUZAI_MST.Equals(answerTypeId)
                || AnsweTypeKbn.RYORI_MST.Equals(answerTypeId)
                || AnsweTypeKbn.HANSEIHIN_MST.Equals(answerTypeId))
            {
                // 回答種類区分 = 食材マスタ, 料理マスタ, 半製品マスタ
                result = this.JudgeConditionMst(resultVal, low, high);
            }
            else if (AnsweTypeKbn.TEXTAREA.Equals(answerTypeId))
            {
                // 回答種類区分 = テキストエリア
                if (!string.IsNullOrEmpty(resultVal))
                {
                    // 備考欄に入力がある場合はエラーデータとみなす？
                    // result = false;
                }
            }
            else if (AnsweTypeKbn.DATE.Equals(answerTypeId))
            {
                // 回答種類区分 = 日付
                result = this.JudgeDate(resultVal, dTime, condition);
            }
            else if (AnsweTypeKbn.TIME.Equals(answerTypeId))
            {
                // 回答種類区分 = 時間
                result = this.JudgeTime(resultVal, dTime, condition);
            }

            return result;
        }

        /// <summary>
        /// 文字列異常データ判定
        /// </summary>
        /// <param name="resultVal">回答結果</param>
        /// <param name="condition">基準値条件</param>
        /// <param name="conditionv1">基準値条件値</param>
        /// <returns>判定結果（false : 異常データ）</returns>
        private bool JudgeConditionStr(string resultVal, string condition, string conditionv1)
        {
            bool result = true;

            // 条件が指定されていない場合は正常で返す
            if (NormalDataReference.UNSPECIFIED.Equals(condition) || string.IsNullOrEmpty(condition) || string.IsNullOrEmpty(conditionv1))
            {
                return result;
            }

            if (NormalDataReference.ISEQUALTO.Equals(condition))
            {
                // 次の値の間　に当てはまらない場合
                if (!conditionv1.Equals(resultVal))
                {
                    result = false;
                }
            }
            else if (NormalDataReference.NOTEQUALTO.Equals(condition))
            {
                // 次の値の間以外　に当てはまらない場合
                if (conditionv1.Equals(resultVal))
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 少数異常データ判定
        /// </summary>
        /// <param name="resultVal">回答結果</param>
        /// <param name="condition">基準値条件</param>
        /// <param name="conditionv1">基準値条件値１</param>
        /// <param name="conditionv2">基準値条件値２</param>
        /// <returns>判定結果（false : 異常データ）</returns>
        private bool JudgeConditionDecimal(string resultVal, string condition, string conditionv1, string conditionv2)
        {
            // 条件が指定されていない場合は正常で返す
            if (NormalDataReference.UNSPECIFIED.Equals(condition) || string.IsNullOrEmpty(condition) || string.IsNullOrEmpty(conditionv1))
            {
                return true;
            }

            // 基準条件ごとの数値比較
            return this.NumericComparison(1, resultVal, condition, conditionv1, conditionv2);
        }

        /// <summary>
        /// 整数異常データ判定
        /// </summary>
        /// <param name="resultVal">回答結果</param>
        /// <param name="condition">基準値条件</param>
        /// <param name="conditionv1">基準値条件値１</param>
        /// <param name="conditionv2">基準値条件値２</param>
        /// <returns>判定結果（false : 異常データ）</returns>
        private bool JudgeConditionNumeric(string resultVal, string condition, string conditionv1, string conditionv2)
        {
            // 条件が指定されていない場合は正常で返す
            if (NormalDataReference.UNSPECIFIED.Equals(condition) || string.IsNullOrEmpty(condition) || string.IsNullOrEmpty(conditionv1))
            {
                return true;
            }

            // 基準条件ごとの数値比較
            return this.NumericComparison(1, resultVal, condition, conditionv1, conditionv2);
        }

        /// <summary>
        /// マスタ設定温度異常データ判定
        /// </summary>
        /// <param name="resultVal">回答</param>
        /// <param name="row">マスタ下限温度</param>
        /// <param name="high">マスタ上限温度</param>
        /// <returns>判定結果（false : 異常データ）</returns>
        private bool JudgeConditionMst(string resultVal, double row, double high)
        {
            bool result = true;

            // マスタの上限下限温度が連携されてない場合は正常データとする
            if (row == 0 && high == 0)
            {
                return result;
            }

            double dResult;
            // 数値変換
            if (!double.TryParse(resultVal, out dResult))
            {
                return false;
            }

            // 上限下限温度の間　に当てはまらない場合
            if (dResult < row || high < dResult)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 日付異常データ判定
        /// </summary>
        /// <param name="hiduke">回答（日付）</param>
        /// <param name="dtime">データ記録日時</param>
        /// <param name="condition">基準値条件</param>
        /// <returns>判定結果（false : 異常データ）</returns>
        private bool JudgeDate(string hiduke, DateTime dtime, string condition)
        {
            // 条件が指定されていない場合は正常で返す
            if (NormalDataReference.UNSPECIFIED.Equals(condition) || string.IsNullOrEmpty(condition))
            {
                return true;
            }

            bool result = true;
            // 日付フォーマットに変換
            hiduke = hiduke.Replace("-", "/");

            if (!DateTime.TryParse(hiduke, out DateTime dt))
            {
                return false;
            }

            if (NormalDataReference.GREATER_THAN.Equals(condition))
            {
                // 次の値より大きい　に当てはまらない場合
                if (dtime.CompareTo(dt) > -1)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.GREATER_THAN.Equals(condition))
            {
                // 次の値より小さい　に当てはまらい場合
                if (dtime.CompareTo(dt) < 1)
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 時間異常データ判定
        /// </summary>
        /// <param name="hiduke">回答（時間）</param>
        /// <param name="dtime">データ記録日時</param>
        /// <param name="condition">基準値条件</param>
        /// <returns>判定結果（false : 異常データ）</returns>
        private bool JudgeTime(string time, DateTime dtime, string condition)
        {
            if (NormalDataReference.UNSPECIFIED.Equals(condition) || string.IsNullOrEmpty(condition))
            {
                return true;
            }

            bool result = true;

            // データ記録時間
            int iDtime = int.Parse(dtime.ToShortTimeString().Replace(":", ""));
            // 回答（日付）
            // 秒が指定されていた場合、切り捨てる
            string[] timeArr = time.Split('.');
            string timeStr = time;
            if (timeArr.Length > 1)
            {
                timeStr = timeArr[0];
            }
            timeStr = timeStr.Replace(":", "");
            timeStr = timeStr.Replace("-", "");
            int iTime = int.Parse(timeStr);

            if (NormalDataReference.GREATER_THAN.Equals(condition))
            {
                // 次の値より大きい　に当てはまらない場合
                if (iDtime >= iTime)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.GREATER_THAN.Equals(condition))
            {
                // 次の値より小さい　に当てはまらい場合
                if (iDtime <= iTime)
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 数値条件判定
        /// </summary>
        /// <param name="resultVal">回答結果</param>
        /// <param name="condition">基準値条件</param>
        /// <param name="conditionv1">基準値条件値１</param>
        /// <param name="conditionv2">基準値条件値２</param>
        /// <returns></returns>
        private bool NumericComparison(int mode, string resultVal, string condition, string conditionv1, string conditionv2)
        {
            bool result = true;

            double dResult;
            double dCond1;
            double dCond2 = 0;
            // 数値以外を除去
            resultVal = resultVal.Replace("度", "");
            resultVal = resultVal.Replace("℃", "");
            resultVal = resultVal.Replace("(℃)", "");
            resultVal = resultVal.Replace("（℃）", "");
            resultVal = resultVal.Replace("°", "");

            // 文字列→少数 変換
            if (!double.TryParse(resultVal, out dResult)
                || !double.TryParse(conditionv1, out dCond1)
                || (!string.IsNullOrEmpty(conditionv2) && !double.TryParse(conditionv2, out dCond2)))
            {
                // 数値変換できないため、異常データとみなす
                return false;
            }

            // 比較対象が整数の場合は小数点以下切り捨て
            if (mode == 2)
            {
                // 整数 小数点以下切り捨て
                dResult = Math.Floor(dResult);
                dCond1 = Math.Floor(dCond1);
                if (!string.IsNullOrEmpty(conditionv2))
                {
                    dCond2 = Math.Floor(dCond2);
                }
            }

            // 基準値条件による判定
            if (NormalDataReference.BETWEEN.Equals(condition))
            {
                // 次の値の間　に当てはまらない場合
                if (dResult < dCond1 || dCond2 < dResult)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.NO_BETWEEN.Equals(condition))
            {
                // 次の値の間以外　に当てはまらない場合
                if (dCond1 <= dResult && dResult <= dCond2)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.ISEQUALTO.Equals(condition))
            {
                // 次の値に等しい　に当てはまらない場合
                if (dResult != dCond1)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.NOTEQUALTO.Equals(condition))
            {
                // 次の値に等しくない　に当てはまらない場合
                if (dResult == dCond1)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.GREATER_THAN.Equals(condition))
            {
                // 次の値より大きい　に当てはまらない場合
                if (dResult <= dCond1)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.LESS_THAN.Equals(condition))
            {
                // 次の値より小さい　に当てはまらない場合
                if (dResult >= dCond1)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.GREATER_THAN_EQUAL.Equals(condition))
            {
                // 次の値以上　に当てはまらない場合
                if (dResult < dCond1)
                {
                    result = false;
                }
            }
            else if (NormalDataReference.LESS_THAN_EQUAL.Equals(condition))
            {
                // 次の値以下　に当てはまらない場合
                if (dCond1 < dResult)
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 異常メール送信管理者取得
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">部門ID</param>
        /// <returns></returns>
        private List<WorkerM> GetManager(MasterContext context, string shopId, string categoryId)
        {
            // 承認対象データ
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append("W.SHOPID ");
            sql.Append(", W.WORKERID ");
            sql.Append(", W.WORKERNAME ");
            sql.Append(", W.MANAGERKBN ");
            sql.Append(", W.CATEGORYKBN1 ");
            sql.Append(", W.CATEGORYKBN2 ");
            sql.Append(", W.CATEGORYKBN3 ");
            sql.Append(", W.CATEGORYKBN4 ");
            sql.Append(", W.CATEGORYKBN5 ");
            sql.Append(", W.CATEGORYKBN6 ");
            sql.Append(", W.CATEGORYKBN7 ");
            sql.Append(", W.CATEGORYKBN8 ");
            sql.Append(", W.CATEGORYKBN9 ");
            sql.Append(", W.CATEGORYKBN10 ");
            sql.Append(", W.MAILADDRESSPC ");
            sql.Append(", W.MAILADDRESSFEATURE ");
            sql.Append(", W.TRANSMISSIONTIME1 ");
            sql.Append(", W.TRANSMISSIONTIME2 ");
            sql.Append(", W.APPID ");
            sql.Append(", W.APPPASS ");
            sql.Append(", W.NODISPLAYKBN ");
            sql.Append(", W.DISPLAYNO ");
            sql.Append(", W.INSUSERID ");
            sql.Append(", W.UPDUSERID ");
            sql.Append(", W.UPDDATE ");
            sql.Append("FROM ");
            sql.Append("WORKER_M W ");
            sql.Append("WHERE ");
            sql.Append("W.SHOPID = '");
            sql.Append(shopId);
            sql.Append("' ");
            sql.Append("AND W.NODISPLAYKBN = '");
            sql.Append(BoolKbn.KBN_FALSE);
            sql.Append("' ");
            sql.Append("AND W.TRANSMISSIONTIME1 IS NOT NULL ");
            sql.Append("AND W.TRANSMISSIONTIME2 IS NOT NULL ");

            var majorPending = context.Database.SqlQuery<WorkerM>(sql.ToString());

            if (majorPending.Count() > 0)
            {
                var WorkList = new List<WorkerM>();
                foreach (WorkerM work in majorPending)
                {
                    string categoryKbn = "0";
                    switch (categoryId)
                    {
                        case "01":
                            categoryKbn = work.CATEGORYKBN1;
                            break;
                        case "02":
                            categoryKbn = work.CATEGORYKBN2;
                            break;
                        case "03":
                            categoryKbn = work.CATEGORYKBN3;
                            break;
                        case "04":
                            categoryKbn = work.CATEGORYKBN4;
                            break;
                        case "05":
                            categoryKbn = work.CATEGORYKBN5;
                            break;
                        case "06":
                            categoryKbn = work.CATEGORYKBN6;
                            break;
                        case "07":
                            categoryKbn = work.CATEGORYKBN7;
                            break;
                        case "08":
                            categoryKbn = work.CATEGORYKBN8;
                            break;
                        case "09":
                            categoryKbn = work.CATEGORYKBN9;
                            break;
                        case "10":
                            categoryKbn = work.CATEGORYKBN10;
                            break;
                        default:
                            break;
                    }

                    // 所属部門の場合
                    if (BoolKbn.KBN_TRUE.Equals(categoryKbn))
                    {
                        WorkList.Add(work);
                    }
                }

                return WorkList;
            }

            return new List<WorkerM>();

        }

        /// <summary>
        /// メール送信時間判定
        /// </summary>
        /// <param name="shoriTime">処理日時</param>
        /// <param name="startTime">メール送信（自）</param>
        /// <param name="endTime">メール送信（至）</param>
        /// <returns>判定結果（true : メール送信対象）</returns>
        private bool ChkMailSendManager(DateTime shoriTime, string startTime, string endTime)
        {
            bool result = false;
            // 時間を取得
            int sShoriTime = int.Parse(shoriTime.TimeOfDay.ToString("hhmm"));

            // 数値変換
            int start = int.Parse(startTime);
            int end = int.Parse(endTime);

            if (start == end)
            {
                result = true;
            }
            else if (start == sShoriTime)
            {
                result = true;
            }
            else if (sShoriTime == end)
            {
                result = false;
            }
            else if (start <= end)
            {
                if (start <= sShoriTime && sShoriTime < end)
                {
                    result = true;
                }
            }
            else if (end < start)
            {
                if (sShoriTime < end || start <= sShoriTime)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 承認経路情報取得
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="stampField">捺印乱数</param>
        /// <param name="shopId">店舗ID</param>
        /// <param name="categoryId">部門ID</param>
        /// <param name="locationId">場所ID</param>
        /// <returns>承認経路情報</returns>
        private IQueryable<ApprovalRouteM> GetApprovalRoute(MasterContext context, int stampField, string shopId, string categoryId, string locationId)
        {
            string category = categoryId;
            string location = locationId;
            string approvalNode = ApprovalCategory.MIDDLE;

            if (ApprovalCategory.FACILITY.Equals(stampField.ToString()))
            {
                category = ApprovalCategory.FACILITYDATA_CATEGORY;
                location = ApprovalCategory.MAJORDATA_LOCATION;
                approvalNode = ApprovalCategory.FACILITY;
            }
            else if (ApprovalCategory.MAJOR.Equals(stampField.ToString()))
            {
                location = ApprovalCategory.MAJORDATA_LOCATION;
                approvalNode = ApprovalCategory.MAJOR;
            }

            // 承認経路情報取得
            var approvalRoute = from ar in context.ApprovalRouteMs
                                where ar.SHOPID == shopId
                                    && ar.CATEGORYID == category
                                    && ar.LOCATIONID == location
                                    && ar.APPROVALORDERCLASS == approvalNode
                                select ar;

            return approvalRoute;
        }

        /// <summary>
        /// 中分類承認データ登録
        /// </summary>
        /// <param name="temperature">温度衛生管理情報</param>
        private void SetMiddleApprovalData(TemperatureControlT temperature)
        {
            // 中分類データ作成（承認待）
            using (var context = new MasterContext())
            {
                // 中分類承認データ
                var middleDt = SetMiddleDt(context, temperature);

                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // データ登録
                        context.MiddleApprovalTs.Add(middleDt);
                        context.SaveChanges();
                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                    catch (Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                }
            }
        }

        /// <summary>
        /// 大分類承認データまで登録
        /// </summary>
        /// <param name="temperature">温度衛生管理情報</param>
        private void SetMajorApprovalData(TemperatureControlT temperature)
        {
            using (var context = new MasterContext())
            {
                // 中分類データ作成（承認済）
                var middleDt = this.SetMiddleDt(context, temperature);
                middleDt.STATUS = ApprovalStatus.APPROVAL;       // ステータス
                middleDt.MIDDLESNNDATE = temperature.DATAYMD;    // 承認日時
                middleDt.MIDDLESNNCOMMENT = APIConstants.APPROVAL_SKIP_COMMENT;  // 承認コメント
                middleDt.MIDDLESNNUSER = APIConstants.API_USER;  // 承認管理者ユーザー

                // 大分類承認データ作成（承認待）
                var majorDt = this.SetMajorDt(context, middleDt);
                // 中分類承認データに中分類承認グループ連番
                middleDt.MIDDLEGROUPNO = majorDt.MIDDLEGROUPNO;

                // 大分類承認が存在する場合
                if (DeleteFlg.DELETE.Equals(majorDt.DELETEFLAG))
                {
                    // 大分類承認データを初期化
                    majorDt = null;
                }

                // データ登録
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 中分類承認データ
                        context.MiddleApprovalTs.Add(middleDt);
                        context.SaveChanges();

                        // 大分類承認データ
                        if (majorDt != null)
                        {
                            context.MajorApprovalTs.Add(majorDt);
                            context.SaveChanges();
                        }

                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                    catch (Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                }
            }
        }

        /// <summary>
        /// 施設承認データまで登録
        /// </summary>
        /// <param name="temperature">温度衛生管理情報</param>
        private void SetFacilityApprovalData(TemperatureControlT temperature)
        {
            using (var context = new MasterContext())
            {
                // 中分類データ作成（承認済）
                var middleDt = this.SetMiddleDt(context, temperature);
                middleDt.STATUS = ApprovalStatus.APPROVAL;                          // ステータス
                middleDt.MIDDLESNNDATE = temperature.DATAYMD;                       // 承認日時
                middleDt.MIDDLESNNCOMMENT = APIConstants.APPROVAL_SKIP_COMMENT;     // 承認コメント
                middleDt.MIDDLESNNUSER = APIConstants.API_USER;                     // 承認管理者ユーザー

                // 大分類承認データ作成（承認済）
                var majorDt = this.SetMajorDt(context, middleDt);
                majorDt.STATUS = ApprovalStatus.APPROVAL;                           // ステータス
                majorDt.MAJORSNNDATE = middleDt.MIDDLESNNDATE;                      // 承認日時
                majorDt.MAJORSNNCOMMENT = middleDt.MIDDLESNNCOMMENT;                // 承認コメント
                majorDt.MAJORSNNUSER = middleDt.MIDDLESNNUSER;                      // 承認管理者ユーザー

                // 中分類承認データに中分類承認グループ連番をセット
                middleDt.MIDDLEGROUPNO = majorDt.MIDDLEGROUPNO;


                // 施設承認データ作成（承認待）
                var facility = this.SetFacilityDt(context, majorDt);
                // 大分類承認データの大分類承認グループ連番をセット
                majorDt.MAJORGROUPNO = facility.MAJORGROUPNO;


                // 大分類承認が存在する場合
                if (DeleteFlg.DELETE.Equals(majorDt.DELETEFLAG))
                {
                    // 大分類承認データを初期化
                    majorDt = null;
                }
                // 施設承認が存在する場合
                if (DeleteFlg.DELETE.Equals(facility.DELETEFLAG))
                {
                    // 施設承認データを初期化
                    facility = null;
                }

                // データ登録
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 中分類承認データ
                        context.MiddleApprovalTs.Add(middleDt);
                        context.SaveChanges();

                        // 大分類承認データ
                        if (majorDt != null)
                        {
                            context.MajorApprovalTs.Add(majorDt);
                            context.SaveChanges();
                        }

                        // 施設承認データ
                        if (facility != null)
                        {
                            context.FacilityApprovalTs.Add(facility);
                            context.SaveChanges();
                        }

                        tran.Commit();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }
                    catch (Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new HttpResponseException(HttpStatusCode.InternalServerError);
                    }

                    // 帳票出力処理
                    this.outputReport(context, middleDt);
                }
            }
        }

        /// <summary>
        /// 中分類承認データ作成
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="temperature">温度衛生管理データ</param>
        /// <returns>中分類承認データ</returns>
        private MiddleApprovalT SetMiddleDt(MasterContext context, TemperatureControlT temperature)
        {
            // 帳票名
            string reportName = string.Empty;
            // 帳票テンプレートID
            string templateId = string.Empty;
            // 帳票テンプレートタイプ
            string templateType = string.Empty;
            // 帳票テンプレート名
            string templateName = string.Empty;
            // 捺印欄
            short stampField = 0;

            // 帳票マスタ
            var reportM = from re in context.ReportMs
                          where re.SHOPID == temperature.SHOPID
                              && re.REPORTID == temperature.REPORTID
                          select re;

            if (reportM.Count() > 0 && reportM.FirstOrDefault() != null)
            {
                // 帳票名
                reportName = reportM.FirstOrDefault().REPORTNAME;
                // 帳票テンプレートID
                templateId = reportM.FirstOrDefault().REPORTTEMPLATEID;
                // 捺印欄
                stampField = reportM.FirstOrDefault().STAMPFIELD;

                // 帳票テンプレートマスタ
                var reportTempM = from reT in context.ReportTemplateMs
                                  where reT.SHOPID == temperature.SHOPID
                                    && reT.TEMPLATEID == templateId
                                  select reT;

                if (reportTempM.Count() > 0 && reportTempM.FirstOrDefault() != null)
                {
                    templateType = reportTempM.FirstOrDefault().REPORTTEMPLATETYPE;
                    templateName = reportTempM.FirstOrDefault().TEMPLATENAME;
                }
            }

            var middleAppDt = new MiddleApprovalT
            {
                SHOPID = temperature.SHOPID,            // 店舗ID
                CATEGORYID = temperature.CATEGORYID,    // 部門ID
                LOCATIONID = temperature.LOCATIONID,    // 場所ID
                REPORTID = temperature.REPORTID,        // 帳票ID
                APPROVALID = temperature.APPROVALID,    // 承認ID
                DATAYMD = temperature.DATAYMD,          // データ記録年月日
                MIDDLEGROUPNO = 0,                      // 中分類承認グループ連番
                STATUS = ApprovalStatus.PENDING,        // ステータス
                PERIOD = temperature.PERIOD,            // 周期
                PERIODSTART = temperature.PERIODSTART,  // 周期開始日
                PERIODEND = temperature.PERIODEND,      // 周期終了日
                STAMPFIELD = stampField,                // 捺印欄数
                WORKERID = temperature.WORKERID,        // 申請作業者ID
                WORKERNAME = temperature.WORKERNAME,    // 申請作業者名称
                REPORTNAME = reportName,                // 帳票名称
                REPORTTEMPLATEID = templateId,          // 帳票テンプレートID
                REPORTTEMPLATETYPE = templateType,      // 帳票テンプレートタイプ
                REPORTTEMPLATENAME = templateName,      // 帳票テンプレート名
                DELETEFLAG = DeleteFlg.NODELETE,        // 論理削除フラグ
                INSUSERID = APIConstants.API_USER,      // 登録ユーザーID
                UPDUSERID = APIConstants.API_USER       // 更新ユーザーID
            };

            return middleAppDt;
        }

        /// <summary>
        /// 大分類承認データ作成
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="middleData">中分類承認データ</param>
        /// <returns>大分類承認データ</returns>
        private MajorApprovalT SetMajorDt(MasterContext context, MiddleApprovalT middleData)
        {
            // 大分類承認データ
            var majorDt = from maj in context.MajorApprovalTs
                          where maj.SHOPID == middleData.SHOPID
                            && maj.CATEGORYID == middleData.CATEGORYID
                            && maj.LOCATIONID == middleData.LOCATIONID
                            && maj.REPORTID == middleData.REPORTID
                            && maj.PERIOD == middleData.PERIOD
                            && maj.PERIODSTART == middleData.PERIODSTART
                            && maj.DELETEFLAG == DeleteFlg.NODELETE
                          select maj;

            // 大分類承認
            var major = new MajorApprovalT();
            major.SHOPID = middleData.SHOPID;
            major.CATEGORYID = middleData.CATEGORYID;
            major.LOCATIONID = middleData.LOCATIONID;
            major.REPORTID = middleData.REPORTID;
            major.PERIOD = middleData.PERIOD;
            major.PERIODSTART = middleData.PERIODSTART;
            major.PERIODEND = middleData.PERIODEND;
            major.STAMPFIELD = middleData.STAMPFIELD;

            if (majorDt.Count() > 0 && majorDt.FirstOrDefault() != null)
            {

                // 中分類承認グループ連番
                major.MIDDLEGROUPNO = majorDt.FirstOrDefault().MIDDLEGROUPNO;
                // 大分類承認グループ連番
                major.MAJORGROUPNO = majorDt.FirstOrDefault().MAJORGROUPNO;
                // 論理削除フラグを立てる
                major.DELETEFLAG = DeleteFlg.DELETE;

                return major;
            }

            // 大分類承認データ（登録用）
            major.MIDDLEGROUPNO = 1;
            major.MAJORGROUPNO = 0;
            major.STATUS = ApprovalStatus.PENDING;
            major.REPORTNAME = middleData.REPORTNAME;
            major.MAJORREQDATE = middleData.DATAYMD;
            major.MAJORREQCOMMENT = APIConstants.APPROVAL_SKIP_COMMENT;
            major.MAJORREQWORKERID = APIConstants.API_USER;
            major.DELETEFLAG = DeleteFlg.NODELETE;
            major.INSUSERID = APIConstants.API_USER;
            major.UPDUSERID = APIConstants.API_USER;

            return major;
        }

        /// <summary>
        /// 施設承認データ作成
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="major">大分類承認データ</param>
        /// <returns>施設承認データ</returns>
        private FacilityApprovalT SetFacilityDt(MasterContext context, MajorApprovalT major)
        {
            // 施設承認データ
            var facilityDt = from faci in context.FacilityApprovalTs
                             where faci.SHOPID == major.SHOPID
                                && faci.CATEGORYID == major.CATEGORYID
                                && faci.PERIOD == major.PERIOD
                                && faci.PERIODSTART == major.PERIODSTART
                                && faci.DELETEFLAG == DeleteFlg.NODELETE
                             select faci;

            // 施設承認
            var facility = new FacilityApprovalT();
            facility.SHOPID = major.SHOPID;
            facility.CATEGORYID = major.CATEGORYID;
            facility.PERIOD = major.PERIOD;
            facility.PERIODSTART = major.PERIODSTART;
            facility.STAMPFIELD = major.STAMPFIELD;

            if (facilityDt.Count() > 0 && facilityDt.FirstOrDefault() != null)
            {
                // 大分類承認グループ
                facility.MAJORGROUPNO = facilityDt.FirstOrDefault().MAJORGROUPNO;
                // 施設承認グループ
                facility.FACILITYAPPGROUPNO = facilityDt.FirstOrDefault().FACILITYAPPGROUPNO;
                // 論理削除フラグを立てる
                facility.DELETEFLAG = DeleteFlg.DELETE;

                return facility;
            }

            // 施設承認データ（登録用）
            facility.PERIODEND = major.PERIODEND;
            facility.MAJORGROUPNO = 1;
            facility.FACILITYAPPGROUPNO = 0;
            facility.STATUS = ApprovalStatus.PENDING;
            facility.FACILITYREQDATE = major.MAJORREQDATE;
            facility.FACILITYREQCOMMENT = major.MAJORREQCOMMENT;
            facility.FACILITYREQWORKERID = major.MAJORREQWORKERID;
            facility.DELETEFLAG = DeleteFlg.NODELETE;

            return facility;
        }

        /// <summary>
        /// 帳票出力処理呼出し
        /// </summary>
        /// <param name="context">Masterコンテキスト</param>
        /// <param name="middleDt">中分類承認データ</param>
        private void outputReport(MasterContext context, MiddleApprovalT middleDt)
        {
            // 帳票出力パス
            MasterFunction masterFunction = new MasterFunction();
            string reportStoragePath = masterFunction.GetReportFolderName(context, middleDt.SHOPID);

            var reportInterface = new CustomReportInterfaceM
            {
                ShopId = middleDt.SHOPID,
                CategoryId = middleDt.CATEGORYID,
                TemplateID = middleDt.REPORTTEMPLATEID,
                Period = middleDt.PERIOD,
                PeriodStart = middleDt.PERIODSTART,
                Title = middleDt.REPORTNAME,
                Path = reportStoragePath
            };

            // 帳票テンプレートマスタからマージ単位を取得
            string margeUnit = ReportMergeUnit.UNIT_MIDDLE;

            var reportTemplateM = from reTemp in context.ReportTemplateMs
                                  where reTemp.SHOPID == middleDt.SHOPID
                                  select reTemp;

            if (reportTemplateM.Count() > 0 && reportTemplateM.FirstOrDefault() != null)
            {
                var tempM = reportTemplateM.Where(a => a.TEMPLATEID == middleDt.REPORTTEMPLATEID).FirstOrDefault();
                if (tempM != null)
                {
                    margeUnit = tempM.MERGEUNIT;
                }
            }

            // 帳票出力インプット情報
            var reportList = new List<CustomReportM>();

            // マージ単位が部門の場合
            var reportM = from report in context.ReportMs
                          where report.SHOPID == middleDt.SHOPID
                            && report.CATEGORYID == middleDt.CATEGORYID
                            && report.REPORTTEMPLATEID == middleDt.REPORTTEMPLATEID
                          select report;
            if (reportM.Count() > 0)
            {
                if (ReportMergeUnit.UNIT_MAJOR.Equals(margeUnit))
                {
                    foreach (ReportM rep in reportM)
                    {
                        var cReport = new CustomReportM
                        {
                            LocationId = rep.LOCATIONID,
                            ReportId = rep.REPORTID
                        };

                        reportList.Add(cReport);
                    }
                }
                else
                {
                    // マージ単位が場所の場合
                    var reportIdDt = reportM.Where(a => a.REPORTID == middleDt.REPORTID);
                    if (reportIdDt.Count() > 0 && reportIdDt.FirstOrDefault() != null)
                    {
                        var cReport = new CustomReportM
                        {
                            LocationId = reportIdDt.FirstOrDefault().LOCATIONID,
                            ReportId = reportIdDt.FirstOrDefault().REPORTID
                        };
                        reportList.Add(cReport);
                    }
                }
            }

            if (reportList.Count() > 0)
            {
                // 帳票リストを設定
                reportInterface.ReportList = reportList;
                var interfaceList = new List<CustomReportInterfaceM>
                {
                    reportInterface
                };

                // 帳票出力の呼び出し
                CommonFunction comm = new CommonFunction();
                comm.CallReportOutputPdf(interfaceList, reportTemplateM);
            }
        }

    }
}