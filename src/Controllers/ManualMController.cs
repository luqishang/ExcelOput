using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using System.Web.Hosting;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class ManualMController : Controller
    {
        private MasterContext context = new MasterContext();
        private readonly CommonFunction comFunc = new CommonFunction();
        private readonly MasterFunction masterFunc = new MasterFunction();
        // 手引書ファイル名区切り文字（appset.configから取得）
        private static readonly string MANUAL_FILE_NAME_DELIMITER = GetAppSet.GetAppSetValue("ManualM", "ManualFileNameDelimiter");

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ManualMController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
        }

        // GET: ManualM
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="requestContext">リクエスト</param>
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            // 画面説明ファイルURL取得
            string strPathAndQuery = requestContext.HttpContext.Request.Url.AbsoluteUri.Replace(requestContext.HttpContext.Request.Url.AbsolutePath, "/");
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "ManualM");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 初期表示
        /// </summary>
        /// <returns>ViewResultオブジェクト</returns>
        [HttpGet]
        public ActionResult Show()
        {
            // セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // 登録メッセージを取得
            string registMsg = (string)Session["registMsg"];
            if (!string.IsNullOrEmpty(registMsg))
            {
                Session.Remove("registMsg");
                ViewBag.registMsg = registMsg;
            }

            // データ取得
            var manualDt = from a in context.ManualMs
                             orderby a.UPLOADDATE descending
                             where a.SHOPID == shopId
                             select a;

            List<BManualM> listManual = new List<BManualM>();

            if (manualDt.Count() > 0)
            {
                foreach (var dt in manualDt)
                {
                    BManualM bManual = new BManualM
                    {
                        // 削除
                        DelFlg = false,
                        // 手引書ID
                        ManualId = dt.MANUALID,
                        // アップロード日時
                        UploadDate = comFunc.GetDataRecording(dt.UPLOADDATE),
                        // 手引書名称
                        ManualName = dt.MANUALNAME,
                        // 手引書ファイル名
                        ManualFileName = Path.GetFileName(dt.MANUALPATH),
                        // 登録ユーザーID
                        InsUserId = dt.INSUSERID,
                        // 更新ユーザーID
                        UpdUserId = dt.UPDUSERID,
                        // 更新年月日
                        UpdDate = dt.UPDDATE.ToString("yyyy/MM/dd HH:mm:ss.ffffff")
                    };

                    // リストにセット
                    listManual.Add(bManual);
                }
            }

            BManualMs manualVal = new BManualMs
            {
                BManualMList = listManual,
                TargetManualId = string.Empty,
                ManualName = string.Empty,
                UploadManual = new BUploadFile(),
            };

            return View(manualVal);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="list">画面入力値(手引書マスタリスト)</param>
        /// <param name="targetManualId">画面入力値(対象手引書ID)</param>
        /// <param name="manualName">画面入力値(手引書名)</param>
        /// <param name="uploadManual">画面入力値(アップロードファイル)</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<BManualM> list, string targetManualId, string manualName, HttpPostedFileBase uploadManual)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 画面の表示順に並び替えてリストに設定
            List<BManualM> updList = new List<BManualM>();
            if (list != null)
            {
                updList = list.OrderBy(BManualM => BManualM.No).ToList();
            }

            // 登録データ
            var insManualMs = new List<ManualM>();
            // 更新データ
            var updManualMs = new List<ManualM>();
            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            BManualMs manualData = new BManualMs
            {
                BManualMList = updList,
                TargetManualId = string.Empty,
                ManualName = string.Empty,
                UploadManual = new BUploadFile(),
            };

            // セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            //// アップロード日時
            var now = DateTime.Now;
            string updateTime = now.ToString("yyyy/MM/dd HH:mm:ss");

            string oldFileName = string.Empty;
            
            // 更新チェック用現在データ取得
            var manualDt = from a in context.ManualMs
                             orderby a.UPLOADDATE descending
                             where a.SHOPID == shopId
                             select a;

            // ファイル存在チェック
            if (uploadManual == null || Request.Files.Count == 0)
            {
                hsError.Add("手引書ファイルが存在しません。");
                checkError = false;
            }
            // ファイル長チェック
            else if (uploadManual.ContentLength == 0)
            {
                hsError.Add("手引書ファイルが不正です。ファイルサイズ=[0]");
                checkError = false;
            }

            BManualM dt = new BManualM();

            // 変更データ抽出
            if (string.IsNullOrEmpty(targetManualId))
            {
                // 新規追加行
                // 値設定
                dt.ShopId = shopId;
                dt.UploadDate = updateTime;
                dt.ManualName = manualName;
                // 必須入力チェック
                if (!CheckRequire(dt, ref hsError))
                {
                    checkError = false;
                }
                else
                {
                    insManualMs.Add(this.SetManualM(dt, managerId, managerId));
                }
            }
            else
            {
                // 更新
                List<BManualM> targetList = new List<BManualM>(updList.Where(a => a.ManualId == targetManualId));
                dt = targetList.First();
                dt.ShopId = shopId;
                dt.UploadDate = updateTime;
                dt.ManualName = manualName;
                // 必須入力チェック
                if (!CheckRequire(dt, ref hsError))
                {
                    checkError = false;
                }
                else
                {
                    // 手引書ファイル更新なし
                    updManualMs.Add(this.SetManualM(dt, managerId));
                    oldFileName = dt.ManualFileName;
                }
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                
                return View("Show", manualData);
            }

            string manualStoragePath = masterFunc.GetManualFolderName(context, shopId);

            // DB更新
            if (updManualMs.Count > 0 || insManualMs.Count > 0)
            {
                using (context = new MasterContext()) 
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            string path = HostingEnvironment.MapPath(manualStoragePath + "/");
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            // データ登録
                            if (insManualMs.Count > 0)
                            {
                                foreach (ManualM insModel in insManualMs)
                                {
                                    // 手引書IDを採番
                                    insModel.MANUALID = masterFunc.GetNumberingID(
                                        context: context, tableName: "MANUAL_M", columnName: "MANUALID", shopId: insModel.SHOPID, digits: 2);
                                    // 手引書格納パスを生成
                                    //newManualPath = this.CreateManualSavePath(insModel.SHOPID, insModel.MANUALID, tmpUploadFileName);
                                    insModel.MANUALPATH = GetManualFileName(insModel.SHOPID, insModel.MANUALID, uploadManual.FileName);
                                    // データ登録
                                    context.ManualMs.Add(insModel);
                                    context.SaveChanges();

                                    //ファイルをアップロード
                                    string filePath = path + insModel.MANUALPATH;
                                    uploadManual.SaveAs(filePath);
                                }
                            }

                            // データ更新
                            if (updManualMs.Count > 0)
                            {
                                foreach (ManualM upddata in updManualMs)
                                {  
                                    string oldFile = path + oldFileName;
                                    if (System.IO.File.Exists(oldFile))
                                    {
                                        FileInfo file = new FileInfo(oldFile);
                                        file.Delete();
                                    }

                                    upddata.MANUALPATH = GetManualFileName(upddata.SHOPID, upddata.MANUALID, uploadManual.FileName);
                                    context.ManualMs.Attach(upddata);
                                    context.Entry(upddata).State = EntityState.Modified;
                                    // 登録・更新の実行
                                    context.SaveChanges();

                                    //ファイルをアップロード
                                    string filePath = path + upddata.MANUALPATH;
                                    uploadManual.SaveAs(filePath);
                                }
                                
                            }
                            // コミット
                            tran.Commit();

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            // ロールバック
                            tran.Rollback();
                            // 排他エラー
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", manualData);
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                            {
                                //一意制約エラー
                                // ロールバック
                                tran.Rollback();
                                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                                return View("Show", manualData);
                            }
                            else
                            {
                                // ロールバック
                                tran.Rollback();
                                LogHelper.Default.WriteError(ex.Message, ex);
                                throw ex;
                            }

                        }
                        catch(Exception ex)
                        {
                            // ロールバック
                            tran.Rollback();
                            LogHelper.Default.WriteError(ex.Message, ex);
                            throw new ApplicationException();
                        }
                    }
                }
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);
            // 初期表示処理に戻す
            return RedirectToAction("Show");
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="list">画面入力値(手引書マスタリスト)</param>
        /// <param name="targetManualId">画面入力値(対象手引書ID)</param>
        /// <param name="manualName">画面入力値(手引書名)</param>
        /// <param name="uploadManual">画面入力値(アップロードファイル)</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(IList<BManualM> list, string targetManualId, string manualName, HttpPostedFileBase uploadManual)
        {
            // post時の情報をクリア
            ModelState.Clear();

            // 画面の表示順に並び替えてリストに設定
            List<BManualM> updList = new List<BManualM>();
            if (list != null)
            {
                updList = list.OrderBy(BManualM => BManualM.No).ToList();
            }

            BManualMs manualData = new BManualMs
            {
                BManualMList = updList,
                TargetManualId = string.Empty,
                ManualName = string.Empty,
                UploadManual = new BUploadFile(),
            };

            // セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            // 更新チェック用現在データ取得
            var manualDt = from a in context.ManualMs
                           orderby a.UPLOADDATE descending
                           where a.SHOPID == shopId
                           select a;

            BManualM dt = new BManualM();

            // 削除用データ
            var delManualMs = new List<ManualM>();
            // 削除
            List<BManualM> targetList = new List<BManualM>(updList.Where(a => a.ManualId == targetManualId));
            dt = targetList.First();
            dt.ShopId = shopId;
            delManualMs.Add(this.SetManualM(dt, managerId));

            string fileName = dt.ManualFileName;

            bool updateFlg = false;
            
            using (context = new MasterContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    try
                    {
                        // データ削除
                        if (delManualMs.Count > 0)
                        {
                            foreach (ManualM deldata in delManualMs)
                            {
                                context.ManualMs.Attach(deldata);
                            }
                            context.ManualMs.RemoveRange(delManualMs);
                            context.SaveChanges();
                        }

                        // コミット
                        tran.Commit();

                        updateFlg = true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // ロールバック
                        tran.Rollback();
                        // 排他エラー
                        ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                        return View("Show", manualData);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                        {
                            //一意制約エラー
                            // ロールバック
                            tran.Rollback();
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            return View("Show", manualData);
                        }
                        else
                        {
                            // ロールバック
                            tran.Rollback();
                            LogHelper.Default.WriteError(ex.Message, ex);
                            throw ex;
                        }
                    }
                    catch(Exception ex)
                    {
                        // ロールバック
                        tran.Rollback();
                        LogHelper.Default.WriteError(ex.Message, ex);
                        throw new ApplicationException();
                    }
                    finally
                    {   
                    }
                }
            }

            if (updateFlg)
            {
                MasterContext context = new MasterContext();
                string manualStoragePath = masterFunc.GetManualFolderName(context, shopId);
                string filePath = HostingEnvironment.MapPath(manualStoragePath + "/" + fileName);

                if (System.IO.File.Exists(filePath))
                {
                    FileInfo file = new FileInfo(filePath);
                    file.Delete();
                }
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);
            // 初期表示処理に戻す
            return RedirectToAction("Show");
        }


        /// <summary>
        /// ファイルダウンロード
        /// </summary>
        /// <param name="list">画面入力値(手引書マスタリスト)</param>
        /// <param name="targetManualId">画面入力値(対象手引書ID)</param>
        /// <param name="manualName">画面入力値(手引書名)</param>
        /// <param name="uploadManual">画面入力値(アップロードファイル)</param>
        /// <returns>ダウンロードファイル</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult FileDownload(IList<BManualM> list, string targetManualId, string manualName, BUploadFile uploadManual)
        {
            // post時の情報をクリア
            ModelState.Clear();
            //  画面の表示順に並び替えてリストに設定
            List<BManualM> updList = list.OrderBy(BManualM => BManualM.No).ToList();

            BManualMs manualVal = new BManualMs
            {
                BManualMList = updList,
                TargetManualId = string.Empty,
                ManualName = string.Empty,
                UploadManual = new BUploadFile(),
            };

            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // データ取得
            var manualDt = from a in context.ManualMs
                             where a.SHOPID == shopId
                                && a.MANUALID == targetManualId
                             select a;

            if (manualDt.Count() == 0) {
                ModelState.AddModelError(string.Empty, "対象の手引書データが存在しません。画面を更新して再度実行してください。");
                return View("Show", manualVal);
            }

            var dt = manualDt.First();

            string manualStoragePath = masterFunc.GetManualFolderName(context, shopId);
            string manualFileName = dt.MANUALPATH;
            string manualPath = HostingEnvironment.MapPath(manualStoragePath + "/" + dt.MANUALPATH);

            if (!System.IO.File.Exists(manualPath))
            {
                ModelState.AddModelError(string.Empty, "対象の手引書ファイルが存在しません。画面を更新して再度実行してください。");
                return View("Show", manualVal);
            }

            Response.ClearHeaders();
            Response.ClearContent();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + manualFileName);
            Response.Flush();
            try
            {
                Response.TransmitFile(manualPath);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "対象の手引書データのダウンロードに失敗しました。画面を更新して再度実行するか、データを登録しなおしてください。");
                return View(manualVal);
            }
            Response.End();
            return null;
        }

        /// <summary>
        /// 手引書データ項目移送（更新・削除用）
        /// </summary>
        /// <param name="bmanualm">画面入力値管理対象データ</param>
        /// <param name="orderNo"></param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns手引書データ></returns>
        private ManualM SetManualM(BManualM bmanualm, string updUserId)
        {
            return SetManualM(bmanualm, string.Empty, updUserId);
        }

        /// <summary>
        /// 手引書データ項目移送
        /// </summary>
        /// <param name="bmanualm">画面入力値管理対象データ</param>
        /// <returns>手引書データ</returns>
        private ManualM SetManualM(BManualM bmanualm, string insUserId, string updUserId)
        {
            var model = new ManualM
            {
                // 店舗ID
                SHOPID = bmanualm.ShopId,
                // 手引書ID
                MANUALID = bmanualm.ManualId,
                // アップロード日時
                UPLOADDATE = string.IsNullOrEmpty(bmanualm.UploadDate) ? string.Empty : Regex.Replace(bmanualm.UploadDate, "[/: ]", string.Empty),
                // 手引書名称
                MANUALNAME = bmanualm.ManualName,
                // 登録ユーザーID
                INSUSERID = insUserId,
                // 更新ユーザーID
                UPDUSERID = updUserId,
            };
            // 削除・更新の場合
            if (string.IsNullOrEmpty(insUserId))
            {
                // 登録ユーザーID
                model.INSUSERID = bmanualm.InsUserId;
                // 更新年月日
                if (bmanualm.UpdDate != null)
                {
                    model.UPDDATE = DateTime.Parse(bmanualm.UpdDate);
                }
            }

            return model;
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="dt">画面入力値</param>
        /// <param name="hsError">エラーセット</param>
        /// <returns>チェック結果（エラー：false）</returns>
        private bool CheckRequire(BManualM dt, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 入力項目のチェック
            if (string.IsNullOrEmpty(dt.ManualName))
            {
                hsError.Add("手引書名を入力してください。");
                ModelState.AddModelError(string.Empty, string.Empty);
                checkError = false;
            }
            else if (dt.ManualName.Length > 30)
            {
                hsError.Add("手引書名は30文字以内で入力してください。");
                ModelState.AddModelError(string.Empty, string.Empty);
                checkError = false;
            }

            return checkError;
        }

        ///// <summary>
        ///// 手引書格納パス生成
        ///// </summary>
        ///// <param name="shopId">店舗ID</param>
        ///// <param name="manualId">手引書ID</param>
        ///// <param name="tmpFileName">一時ファイル名</param>
        ///// <returns>手引書格納パス</returns>
        //private string CreateManualSavePath(string shopId, string manualId, string tmpFileName)
        //{
        //    string extension = Path.GetExtension(tmpFileName);
        //    string manualPath = Path.Combine(shopId, shopId + MANUAL_FILE_NAME_DELIMITER + manualId + extension);

        //    return manualPath;
        //}

        /// <summary>
        /// 手引書格納パス生成
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="manualId">手引書ID</param>
        /// <param name="tmpFileName">一時ファイル名</param>
        /// <returns>手引書格納パス</returns>
        private string GetManualFileName(string shopId, string manualId, string tmpFileName)
        {
            string extension = Path.GetExtension(tmpFileName);
            string fileName = shopId + MANUAL_FILE_NAME_DELIMITER + manualId + extension;

            return fileName;
        }
    }
}