using HACCPExtender.Models;
using System;
using System.Collections.Generic;
using HACCPExtender.Business;
using System.Diagnostics;
using System.IO;
using MimeKit;
using System.Linq;

namespace HACCPExtender.Controllers.Common
{
    public class MailSenderFunction
    {
        private const string CLASS_FULL_NAME = "CEmailSender";

        private string SMTPHost = "";
        private int SMTPPort = 0;
        private bool isSSLConnection = false;
        private string SMTPAccount = "";
        private string SMTPPassword = "";
        private bool mustAuthenticate = false;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="smtpHost">SMTPホスト</param>
        /// <param name="smtpPort">SMTPポート</param>
        /// <param name="account">アカウント</param>
        /// <param name="password">パスワード</param>
        /// <param name="isSSLConnect">SSL接続</param>
        public MailSenderFunction()
        {
            this.SMTPHost = GetAppSet.GetAppSetValue("Mail", "SMTPHost");
            this.SMTPPort = int.Parse(GetAppSet.GetAppSetValue("Mail", "SMTPPort"));
            this.isSSLConnection = bool.Parse(GetAppSet.GetAppSetValue("Mail", "IsSSLConnection"));
            this.SMTPAccount = GetAppSet.GetAppSetValue("Mail", "SMTPAccount");
            this.SMTPPassword = GetAppSet.GetAppSetValue("Mail", "SMTPPassword");
            this.mustAuthenticate = bool.Parse(GetAppSet.GetAppSetValue("Mail", "MustAuthenticate"));
        }

        /// <summary>
        /// メール送信者情報設定
        /// </summary>
        /// <returns></returns>
        public MailInfo GetSendMailAddress()
        {
            MailInfo info = new MailInfo
            {
                WorkerName = GetAppSet.GetAppSetValue("Mail", "SendMailerName"),
                MailAddress = GetAppSet.GetAppSetValue("Mail", "SendEmailAddress"),
            };

            return info;
        }

        /// <summary>
        /// 次承認管理者メール情報設定
        /// </summary>
        /// <param name="context">マスタコンテキスト</param>
        /// <param name="ApprovalRouteMs">承認経路マスタデータ</param>
        /// <returns></returns>
        public List<MailInfo> SetMailAddress(MasterContext context, IQueryable<ApprovalRouteM> ApprovalRouteMs)
        {
            var infoList = new List<MailInfo>();

            if (ApprovalRouteMs.Count() > 0)
            {
                foreach (ApprovalRouteM app in ApprovalRouteMs)
                {
                    var manager = from w in context.WorkerMs
                                  where w.SHOPID == app.SHOPID
                                    && w.WORKERID == app.APPMANAGERID
                                  select w;
                    if (manager.Count() > 0)
                    {
                        MailInfo mail = new MailInfo
                        {
                            // 名前
                            WorkerName = manager.FirstOrDefault().WORKERNAME,
                            // メールアドレス（PC）
                            MailAddress = manager.FirstOrDefault().MAILADDRESSPC,
                            // メールアドレス（携帯）
                            MailAddressFeature = manager.FirstOrDefault().MAILADDRESSFEATURE
                        };

                        infoList.Add(mail);
                    }
                }
            }

            return infoList;
        }

        /// <summary>
        /// メール送信
        /// </summary>
        /// <param name="toEmailAddrLst">宛先メールアドレス群</param>
        /// <param name="ccEmailAddrLst">CCメールアドレス群</param>
        /// <param name="bccEmailAddrLst">BCCメールアドレス群</param>
        /// <param name="fromEmailAddr">送信者メールアドレス</param>
        /// <param name="subject">題名</param>
        /// <param name="content">メール本文</param>
        /// <param name="attachFileLst">添付ファイルリスト</param>
        /// <param name="contentType">メール本文の形式(TEXT or HTML)</param>
        public void SendMail(List<MailInfo> toEmailAddrLst,
                                       List<MailInfo> ccEmailAddrLst,
                                       List<MailInfo> bccEmailAddrLst,
                                       MailInfo fromEmailAddr,
                                       string subject,
                                       string content,
                                       List<string> attachFileLst,
                                       string contentType)
        {

            string curMethodName = "SendMail()";

            //宛先メールアドレス無し
            if (toEmailAddrLst.Count == 0)
            {
                Debug.WriteLine(curMethodName, "宛先メールアドレスが指定されていません。");
                return;
            }//end if

            try
            {
                MimeMessage msg = new MimeMessage();

                //宛先メールアドレス設定
                foreach (MailInfo tmp in toEmailAddrLst)
                {
                    // PCメールアドレス
                    if (!string.IsNullOrEmpty(tmp.MailAddress))
                    {
                        msg.To.Add(new MailboxAddress(tmp.WorkerName, tmp.MailAddress));
                    }
                    // 携帯メールアドレス
                    if (!string.IsNullOrEmpty(tmp.MailAddressFeature))
                    {
                        msg.To.Add(new MailboxAddress(tmp.WorkerName, tmp.MailAddressFeature));
                    }
                }//end foreach

                //CCメールアドレス設定
                foreach (MailInfo tmp in ccEmailAddrLst)
                {
                    if (!string.IsNullOrEmpty(tmp.MailAddress))
                    {
                        msg.Cc.Add(new MailboxAddress(tmp.WorkerName, tmp.MailAddress));
                    }
                }//end foreach

                //BCCメールアドレス設定
                foreach (MailInfo tmp in bccEmailAddrLst)
                {
                    if (!string.IsNullOrEmpty(tmp.MailAddress))
                    {
                        msg.Bcc.Add(new MailboxAddress(tmp.WorkerName, tmp.MailAddress));
                    }
                }//end foreach

                //fromメールアドレス設定
                msg.From.Add(new MailboxAddress(fromEmailAddr.WorkerName, fromEmailAddr.MailAddress));

                msg.Subject = subject;

                TextPart textPart = null;
                if ("TEXT".Equals(contentType))
                {
                    textPart = new TextPart(MimeKit.Text.TextFormat.Plain);
                }
                else if ("HTML".Equals(contentType))
                {
                    textPart = new TextPart(MimeKit.Text.TextFormat.Html);
                }

                //メール本文
                textPart.Text = content;

                if (attachFileLst == null || attachFileLst.Count == 0)  //添付ファイル無し
                {
                    msg.Body = textPart;  //メール本文設定
                }
                else  //添付ファイル有
                {
                    Multipart multipart = new Multipart("Multipart/Mixed")
                    {
                        textPart  //メール本文設定
                    };

                    foreach (string curFile in attachFileLst)
                    {
                        //添付ファイルはバイナリデータとして扱う
                        MimePart attachement = new MimePart("Application/Octet-Stream")
                        {
                            Content = new MimeContent(File.OpenRead(curFile)),
                            ContentDisposition = new ContentDisposition(),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = Path.GetFileName(curFile)
                        };  //end new MimePart()

                        multipart.Add(attachement);
                    }//end foreach

                    msg.Body = multipart;
                }//end if

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    Debug.WriteLine(curMethodName, "SMTPサーバへ接続中...");

                    smtp.Connect(this.SMTPHost, this.SMTPPort, this.isSSLConnection);  //use SSL: true ファイル添付時必須

                    Debug.WriteLine(curMethodName, "SMTPサーバへ接続完了。");

                    //SMTPサーバがユーザ認証を必要とする場合
                    if (this.mustAuthenticate)
                    {
                        //SMTPサーバユーザ認証
                        smtp.Authenticate(this.SMTPAccount, this.SMTPPassword);
                        Debug.WriteLine(curMethodName, "SMTPサーバ認証完了。");
                    }//end if

                    Debug.WriteLine(curMethodName, "メール送信中...");

                    smtp.Send(msg);
                    Debug.WriteLine(curMethodName, "メール送信済み。");

                    smtp.Disconnect(true);
                    Debug.WriteLine(curMethodName, "STMPサーバとの接続切断。");
                }//end using

            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
            }//end try
        }
    }

}