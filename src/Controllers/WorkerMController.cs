using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using static HACCPExtender.Constants.Const;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtender.Controllers
{
    public class WorkerMController : Controller
    {
        private MasterContext context = new MasterContext();
        private readonly MasterFunction masterFunc = new MasterFunction();
        private readonly CommonFunction comFunc = new CommonFunction();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WorkerMController()
        {
            context.Database.Log = sql =>
            {
                Debug.Write(sql);
            };
        }

        /// <summary>
        /// 初期処理
        /// </summary>
        /// <param name="requestContext">リクエスト</param>
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            // 画面説明ファイルURL取得
            string strPathAndQuery = requestContext.HttpContext.Request.Url.AbsoluteUri.Replace(requestContext.HttpContext.Request.Url.AbsolutePath, "/");
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "WorkerM");
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
            //　セッションから編集モードを取得
            string editMode = (string)Session["DISPMODE"];
            // 画面モードの決定
            CommonFunction comfunc = new CommonFunction();
            ViewBag.editMode = comfunc.GetEditButton(editMode);

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // 登録メッセージを取得
            string registMsg = (string)Session["registMsg"];
            if (!string.IsNullOrEmpty(registMsg))
            {
                Session.Remove("registMsg");
                ViewBag.registMsg = registMsg;
            }

            // 部門表示用データ取得設定
            ViewBag.categoryNameList = CreateCategoryNameList(shopId);

            // データ取得
            var workerDt = from a in context.WorkerMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            List<BWorkerM> listWorker = new List<BWorkerM>();

            if (workerDt.Count() == 0)
            {
                BWorkerM bWorker = new BWorkerM();
                listWorker.Add(bWorker);
            }
            else
            {
                foreach (var dt in workerDt)
                {
                    BWorkerM bWorker = new BWorkerM
                    {
                        // 編集モード
                        EditMode = 0,
                        // 削除
                        DelFlg = false,
                        // 作業者ID
                        WorkerId = dt.WORKERID,
                        // 作業者名
                        WorkerName = dt.WORKERNAME,
                        // 管理者区分
                        ManagerKbn = !BoolKbn.KBN_FALSE.Equals(dt.MANAGERKBN),
                        // 部門1区分
                        CategoryKbn1 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN1),
                        // 部門2区分
                        CategoryKbn2 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN2),
                        // 部門3区分
                        CategoryKbn3 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN3),
                        // 部門4区分
                        CategoryKbn4 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN4),
                        // 部門5区分
                        CategoryKbn5 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN5),
                        // 部門6区分
                        CategoryKbn6 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN6),
                        // 部門7区分
                        CategoryKbn7 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN7),
                        // 部門8区分
                        CategoryKbn8 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN8),
                        // 部門9区分
                        CategoryKbn9 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN9),
                        // 部門10区分
                        CategoryKbn10 = !BoolKbn.KBN_FALSE.Equals(dt.CATEGORYKBN10),
                        // 承認ID
                        AppId = dt.APPID,
                        // 承認パスワード
                        AppPass = dt.APPPASS,
                        // メールアドレス（PC）
                        MailAddressPc = dt.MAILADDRESSPC,
                        // メールアドレス（携帯）
                        MailAddressFeature = dt.MAILADDRESSFEATURE,
                        // メール送信時間（自）
                        TransMissionTime1 = comfunc.FormatTime(dt.TRANSMISSIONTIME1),
                        // メール送信時間（至）
                        TransMissionTime2 = comfunc.FormatTime(dt.TRANSMISSIONTIME2),
                        // 表示対象外区分
                        NoDisplayKbn = !BoolKbn.KBN_FALSE.Equals(dt.NODISPLAYKBN),
                        // 表示No
                        DisplayNo = dt.DISPLAYNO,
                        // 登録ユーザーID
                        InsUserId = dt.INSUSERID,
                        // 更新ユーザーID
                        UpdUserId = dt.UPDUSERID,
                        // 更新年月日
                        UpdDate = dt.UPDDATE.ToString("yyyy/MM/dd HH:mm:ss.ffffff")
                    };

                    // リストにセット
                    listWorker.Add(bWorker);
                }
            }

            // CSV位置(デフォルトは出力時と同じため出力用定義から作成)
            List<int> listCsvPos = new List<int>();
            CsvDef csvDefs = CsvManage.DEFS_WORKER.First();
            foreach (CsvColumn csvColDt in csvDefs.Columns)
            {
                listCsvPos.Add(csvColDt.Pos);
            }

            List<BUploadFile> listUploadFile = new List<BUploadFile>();
            BUploadFile bUploadFile = new BUploadFile
            {
                UploadFile = null,
            };
            listUploadFile.Add(bUploadFile);

            BWorkerMs workerVal = new BWorkerMs
            {
                BWorkerMList = listWorker,
                BCsvPosList = listCsvPos,
                BUploadFileList = listUploadFile,
            };

            return View(workerVal);
        }

        /// <summary>
        /// 行追加処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <returns>ViewStateオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Add(IList<BWorkerM> list, List<int> posList, List<BUploadFile> uploadFileList)
        {
            // post時の情報をクリア
            ModelState.Clear();
            // 画面の表示順に並び替えてリストに設定
            IList<BWorkerM> addList = list.OrderBy(BWorkerM => BWorkerM.No).ToList();

            // 追加分
            BWorkerM addRow = new BWorkerM
            {
                // 編集モード
                EditMode = 0
            };
            // 行追加
            addList.Add(addRow);
            BWorkerMs workerVal = new BWorkerMs
            {
                BWorkerMList = addList,
                BCsvPosList = posList,
                BUploadFileList = uploadFileList,
            };

            // 部門表示用データ取得設定
            ViewBag.categoryNameList = CreateCategoryNameList((string)Session["SHOPID"]);

            return View("Show", workerVal);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<BWorkerM> list, List<int> posList, List<BUploadFile> uploadFileList)
        {
            // post時の情報をクリア
            ModelState.Clear();
            //  画面の表示順に並び替えてリストに設定
            List<BWorkerM> updList = list.OrderBy(BWorkerM => BWorkerM.No).ToList();

            // 削除用データ
            var delWorkerMs = new List<WorkerM>();
            // 登録データ
            var insWorkerMs = new List<WorkerM>();
            // 更新データ
            var updWorkerMs = new List<WorkerM>();
            // 表示順 DB登録用
            int rowNo = 1;
            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            // セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            // 部門表示用データ取得設定
            ViewBag.categoryNameList = CreateCategoryNameList(shopId);

            // 更新チェック用現在データ取得
            var workerDt = from a in context.WorkerMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            // 変更データ抽出（ソート番号以外）
            foreach (BWorkerM dt in updList)
            {
                dt.ShopId = shopId;

                // 追加行（作業者IDなし）で、削除checkあり or 必須項目がすべて未入力の場合は無視
                if (string.IsNullOrEmpty(dt.WorkerId)
                    && (dt.DelFlg || (string.IsNullOrEmpty(dt.WorkerName))))
                {
                    continue;
                }

                // 削除checkありの場合は入力内容無視
                if (dt.DelFlg)
                {
                    // データ整合性チェック（削除対象）
                    if (this.CheckRelation(dt, 1))
                    {
                        // 削除対象に追加
                        delWorkerMs.Add(this.SetWorkerM(dt, dt.DisplayNo, managerId));
                    }
                    else
                    {
                        // 関連チェックエラー
                        checkError = false;
                    }
                    continue;
                }
                
                // 新規追加行
                if (string.IsNullOrEmpty(dt.WorkerId))
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                        continue;
                    }

                    // 一意性チェック
                    if (!CheckUniqueness(dt, updList, workerDt, ref hsError))
                    {
                        checkError = false;
                    }

                    // 新規
                    insWorkerMs.Add(this.SetWorkerM(dt, rowNo, managerId, managerId));
                    rowNo++;
                    continue;
                }

                // DBデータから取得
                List<WorkerM> registData = new List<WorkerM>(workerDt.Where(a => a.WORKERID == dt.WorkerId));

                // DBにデータが存在しない場合
                if (registData.Count == 0)
                {
                    // データが更新されているため、排他エラーとする
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    BWorkerMs workerData = new BWorkerMs
                    {
                        BWorkerMList = updList,
                        BCsvPosList = posList,
                        BUploadFileList = uploadFileList,
                    };
                    return View("Show", workerData);
                }
                else if (this.CheckDataUpd(registData, dt))
                {
                    // データ整合性チェック（更新対象）ソート番号のみの更新は除く
                    // 更新データの関連チェック
                    if (!this.CheckRelation(dt, 2))
                    {
                        // 関連チェックエラー
                        checkError = false;
                        continue;
                    }

                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                        continue;
                    }

                    // 一意性チェック
                    if (!CheckUniqueness(dt, updList, workerDt, ref hsError))
                    {
                        checkError = false;
                    }
                }
                else
                {
                    // 表示順に変更がない場合は次の行へ
                    if (dt.DisplayNo == rowNo)
                    {
                        rowNo++;
                        continue;
                    }
                }
                // 更新
                updWorkerMs.Add(this.SetWorkerM(dt, rowNo, managerId));
                rowNo++;
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                BWorkerMs workerData = new BWorkerMs
                {
                    BWorkerMList = updList,
                    BCsvPosList = posList,
                    BUploadFileList = uploadFileList,
                };

                return View("Show", workerData);
            }

            // DB更新
            if (delWorkerMs.Count > 0 || updWorkerMs.Count > 0 || insWorkerMs.Count > 0)
            {
                using (context = new MasterContext()) 
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // データ削除
                            if (delWorkerMs.Count > 0)
                            {
                                foreach (WorkerM deldata in delWorkerMs)
                                {
                                    context.WorkerMs.Attach(deldata);
                                }
                                context.WorkerMs.RemoveRange(delWorkerMs);
                                context.SaveChanges();
                            }

                            // データ登録
                            foreach (WorkerM insModel in insWorkerMs)
                            {
                                // 作業者IDを採番
                                insModel.WORKERID = masterFunc.GetNumberingID(
                                    context: context, tableName: "WORKER_M", columnName: "WORKERID", shopId: insModel.SHOPID, digits: 5);
                                // データ登録
                                context.WorkerMs.Add(insModel);
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updWorkerMs.Count > 0)
                            {
                                foreach (WorkerM upddata in updWorkerMs)
                                {
                                    context.WorkerMs.Attach(upddata);
                                    context.Entry(upddata).State = EntityState.Modified;
                                }
                                // 登録・更新の実行
                                context.SaveChanges();
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
                            BWorkerMs workerData = new BWorkerMs
                            {
                                BWorkerMList = updList,
                                BCsvPosList = posList,
                                BUploadFileList = uploadFileList,
                            };
                            return View("Show", workerData);
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                            {
                                //一意制約エラー
                                // ロールバック
                                tran.Rollback();
                                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                                BWorkerMs workerData = new BWorkerMs
                                {
                                    BWorkerMList = updList,
                                    BCsvPosList = posList,
                                    BUploadFileList = uploadFileList,
                                };
                                return View("Show", workerData);
                            }
                            else
                            {
                                // ロールバック
                                tran.Rollback();
                                LogHelper.Default.WriteError(ex.Message, ex);
                                throw ex;
                            }
                        }
                        catch
                        {
                            // ロールバック
                            tran.Rollback();
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
        /// CSVファイルデータアップロード処理
        /// </summary>
        /// <param name="list">画面入力値（管理対象データ）</param>
        /// <param name="posList">画面入力値</param>
        /// <param name="uploadFileList">CSVファイル</param>
        /// <returns>ViewResultオブジェクト</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult FileUpload(IList<BWorkerM> list, List<int> posList, List<BUploadFile> uploadFileList)
        {
            // post時の情報をクリア
            ModelState.Clear();
            // 画面の表示順に並び替えてリストに設定
            List<BWorkerM> dispList = list.OrderBy(BWorkerM => BWorkerM.No).ToList();

            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            // CSV管理情報を元に位置順に並び替えてリストを作成
            CsvDef csvDefs = CsvManage.DEFS_WORKER.First();
            List<CsvColumn> csvColumns = new List<CsvColumn>();
            for (int i = 0; i < csvDefs.Columns.Count; i++)
            {
                CsvColumn csvColDt = new CsvColumn()
                {
                    Pos = posList[i],
                    Name = csvDefs.Columns[i].Name,
                    Title = csvDefs.Columns[i].Title,
                };
                csvColumns.Add(csvColDt);
            }

            // セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            // 部門表示用データ取得設定
            ViewBag.categoryNameList = CreateCategoryNameList(shopId);

            // CSV位置チェック
            for (int i = 0; i < posList.Count(); i++) {
                // CSV位置が0か重複ならエラー
                if (posList[i] == 0 || posList.FindAll(n => n == posList[i]).Count() > 1)
                {
                    hsError.Add("CSV位置の指定が誤っています。");
                    ModelState.AddModelError("posList[" + i + "]", string.Empty);
                    checkError = false;
                }
            }

            // ファイル存在チェック
            if (uploadFileList[0].UploadFile == null)
            {
                hsError.Add("CSVファイルが存在しません。");
                checkError = false;
            }
            else
            {
                //CSVファイルかどうかチェック
                string extension = System.IO.Path.GetExtension(uploadFileList[0].UploadFile.FileName).ToUpper();
                if (!".CSV".Equals(extension))
                {
                    hsError.Add("CSVファイルをアップロードしてください。");
                    checkError = false;
                }

                // ファイル長チェック
                if (uploadFileList[0].UploadFile.ContentLength == 0)
                {
                    hsError.Add("CSVファイルが不正です。ファイルサイズ=[0]");
                    checkError = false;
                }


            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                BWorkerMs workerData = new BWorkerMs
                {
                    BWorkerMList = dispList,
                    BCsvPosList = posList,
                    BUploadFileList = uploadFileList,
                };

                return View("Show", workerData);
            }

            // 1行目スキップ設定
            bool firstLineSkip = uploadFileList[0].HeadFlg;

            List<string[]> listCsv = null;
            // CSV読み込み
            CsvParser csvParser = null;

            try
            {
                csvParser = new CsvParser(uploadFileList[0].UploadFile.InputStream);
                listCsv = csvParser.csvList.ToList();

                if (csvParser.ErrorMessageList.Count() > 0)
                {
                    hsError = (HashSet<string>)hsError.Concat(csvParser.ErrorMessageList);
                    checkError = false;
                }

                // データ件数チェック
                if (listCsv.Count() == 0)
                {
                    hsError.Add("CSVファイルが不正です。データ件数=[0]");
                    checkError = false;
                }
                else
                {
                    if (firstLineSkip)
                    {
                        // 1行目がタイトル行の場合1行目のデータを削除
                        listCsv.RemoveAt(0);
                        if (listCsv.Count() == 0)
                        {
                            hsError.Add("CSVファイルが不正です。データ件数=[0]");
                            checkError = false;
                        }
                    }
                }
                // CSV位置MAX値チェック
                int posMax = posList.OrderByDescending(n => n).First();
                if (listCsv.Where(n => n.Count() < posMax).Count() > 0)
                {
                    hsError.Add("CSVファイルが不正です。CSV位置=[" + posMax + "] に満たないデータ行があります。");
                    checkError = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                hsError.Add("CSVファイルが不正です。読込エラーが発生しました。");
                checkError = false;
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                BWorkerMs workerData = new BWorkerMs
                {
                    BWorkerMList = dispList,
                    BCsvPosList = posList,
                    BUploadFileList = uploadFileList,
                };

                return View("Show", workerData);
            }

            // 更新チェック用現在データ取得
            var workerDt = from a in context.WorkerMs
                               orderby a.DISPLAYNO
                               where a.SHOPID == shopId
                               select a;

            // 登録データ
            var insWorkerMs = new List<WorkerM>();
            // 表示順 DB登録用
            int rowNo = (workerDt.Count() == 0) ? 1 : workerDt.OrderByDescending(n => n.DISPLAYNO).First().DISPLAYNO;

            // CSV行番号
            int lineNo = 0;
            if (firstLineSkip) {
                // 1行目がタイトル行の場合
                lineNo++;
            }

            foreach (var csvDt in listCsv)
            {
                lineNo++;

                // DBデータから取得
                string targetWorkerName = csvDt[(csvColumns.Find(n => n.Name == "WORKERNAME").Pos - 1)];
                List<WorkerM> registData = new List<WorkerM>(workerDt.Where(a => a.WORKERNAME == targetWorkerName));

                var model = new WorkerM();

                // DBにデータが存在しない場合
                if (registData.Count == 0)
                {
                    foreach (CsvColumn csvColDt in csvColumns) {
                        // 必須チェック
                        if (string.IsNullOrEmpty(csvDt[csvColDt.Pos - 1]))
                        {
                            // 必須チェックエラー
                            hsError.Add("CSVファイルが不正です。" + csvColDt.Title + "の値を入力してください。行番号=[" + lineNo + "] ");
                            checkError = false;
                        }

                        // 対象データのカラム名よりデータ設定
                        PropertyInfo pInfo = typeof(WorkerM).GetProperty(csvColDt.Name);
                        if (pInfo.PropertyType.Name == "String")
                        {
                            pInfo.SetValue(model, csvDt[csvColDt.Pos - 1], null);
                        }
                        else if (pInfo.PropertyType.Name == "Int16")
                        {
                            pInfo.SetValue(model, short.Parse(csvDt[csvColDt.Pos - 1]), null);
                        }
                    }
                    // 店舗ID
                    model.SHOPID = shopId;
                    // 表示No
                    model.DISPLAYNO = (short)rowNo++;
                    // 管理者
                    model.MANAGERKBN = BoolKbn.KBN_FALSE;
                    // 大分類
                    model.CATEGORYKBN1 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN2 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN3 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN4 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN5 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN6 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN7 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN8 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN9 = BoolKbn.KBN_FALSE;
                    model.CATEGORYKBN10 = BoolKbn.KBN_FALSE;
                    // 表示対象区分
                    model.NODISPLAYKBN = BoolKbn.KBN_FALSE;
                    // 登録ユーザーID
                    model.INSUSERID = managerId;
                    // 更新ユーザーID
                    model.UPDUSERID = managerId;

                    insWorkerMs.Add(model);
                }
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                BWorkerMs workerData = new BWorkerMs
                {
                    BWorkerMList = dispList,
                    BCsvPosList = posList,
                    BUploadFileList = uploadFileList,
                };

                return View("Show", workerData);
            }

            // DB登録
            if (insWorkerMs.Count > 0)
            {
                using (context = new MasterContext()) 
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // データ登録
                            foreach (WorkerM insModel in insWorkerMs)
                            {
                                // 作業者IDを採番
                                insModel.WORKERID = masterFunc.GetNumberingID(
                                    context: context, tableName: "WORKER_M", columnName: "WORKERID", shopId: insModel.SHOPID, digits: 5);
                                // データ登録
                                context.WorkerMs.Add(insModel);
                                context.SaveChanges();
                            }

                            tran.Commit();
                        }                    
                        catch (DbUpdateConcurrencyException)
                        {
                            // ロールバック
                            tran.Rollback();
                            // 排他エラー
                            ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                            BWorkerMs workerData = new BWorkerMs
                            {
                                BWorkerMList = dispList,
                                BCsvPosList = posList,
                                BUploadFileList = uploadFileList,
                            };
                            return View("Show", workerData);
                        }
                        catch (Exception ex)
                        {
                            // ロールバック
                            tran.Rollback();
                            LogHelper.Default.WriteError(ex.Message, ex);
                            throw ex;
                        }
                    }
                }
            }

            // セッションに登録メッセージを保持
            Session.Add("registMsg", MsgConst.REGIST_CSV_NORMAL_MSG);
            // 初期表示処理に戻す
            return RedirectToAction("Show");
        }

        /// <summary>
        /// 初期設定完了
        /// </summary>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult InitComplete()
        {
            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];
            // データ取得
            var workerDt = from a in context.WorkerMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                                && a.MANAGERKBN == "1"
                                && a.NODISPLAYKBN == "0"
                           select a;

            if (workerDt.Count() == 0)
            {
                // ログイン可能な管理者がいない場合
                Session.Add("DISPMODE", ManagerLoginMode.NO_MANAGER);
            }
            else
            {
                // ログイン可能な管理者がいる場合
                Session.Add("DISPMODE", ManagerLoginMode.LOGIN_NONE);
            }

            // フォーマット系列店舗ID
            Session.Remove("FORMATSHOPID");

            return RedirectToAction("Show", "Top");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bworkerm"></param>
        /// <param name="orderNo"></param>
        /// <param name="updUserId"></param>
        /// <returns></returns>
        private WorkerM SetWorkerM(BWorkerM bworkerm, int orderNo, string updUserId)
        {
            return SetWorkerM(bworkerm, orderNo, string.Empty, updUserId);
        }

        /// <summary>
        /// 作業者マスタデータ項目移送
        /// </summary>
        /// <param name="bworkerm">画面作業者マスタデータ</param>
        /// <param name="orderNo">表示No</param>
        /// <param name="insUserId">登録ユーザーID</param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns></returns>
        private WorkerM SetWorkerM(BWorkerM bworkerm, int orderNo, string insUserId, string updUserId)
        {
            var model = new WorkerM
            {
                // 店舗ID
                SHOPID = bworkerm.ShopId,
                // 作業者ID
                WORKERID = bworkerm.WorkerId,
                // 作業者名
                WORKERNAME = bworkerm.WorkerName,
                // 管理者区分
                MANAGERKBN = bworkerm.ManagerKbn ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門1区分
                CATEGORYKBN1 = bworkerm.CategoryKbn1 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門2区分
                CATEGORYKBN2 = bworkerm.CategoryKbn2 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門3区分
                CATEGORYKBN3 = bworkerm.CategoryKbn3 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門4区分
                CATEGORYKBN4 = bworkerm.CategoryKbn4 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門5区分
                CATEGORYKBN5 = bworkerm.CategoryKbn5 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門6区分
                CATEGORYKBN6 = bworkerm.CategoryKbn6 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門7区分
                CATEGORYKBN7 = bworkerm.CategoryKbn7 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門8区分
                CATEGORYKBN8 = bworkerm.CategoryKbn8 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門9区分
                CATEGORYKBN9 = bworkerm.CategoryKbn9 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 部門10区分
                CATEGORYKBN10 = bworkerm.CategoryKbn10 ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 承認ID
                APPID = bworkerm.AppId,
                // 承認パスワード
                APPPASS = bworkerm.AppPass,
                // メールアドレス（PC）
                MAILADDRESSPC = bworkerm.MailAddressPc,
                // メールアドレス（携帯）
                MAILADDRESSFEATURE = bworkerm.MailAddressFeature,
                // メール送信時間（自）
                TRANSMISSIONTIME1 = comFunc.FormatTimeStr(bworkerm.TransMissionTime1),
                // メール送信時間（至）
                TRANSMISSIONTIME2 = comFunc.FormatTimeStr(bworkerm.TransMissionTime2),
                // 表示対象外区分
                NODISPLAYKBN = bworkerm.NoDisplayKbn ? BoolKbn.KBN_TRUE : BoolKbn.KBN_FALSE,
                // 表示No
                DISPLAYNO = (short)orderNo,
                // 登録ユーザーID
                INSUSERID = insUserId,
                // 更新ユーザーID
                UPDUSERID = updUserId
            };
            // 削除・更新の場合
            if (string.IsNullOrEmpty(insUserId))
            {
                // 登録ユーザーID
                model.INSUSERID = bworkerm.InsUserId;
                // 更新年月日
                if (bworkerm.UpdDate != null)
                {
                    model.UPDDATE = DateTime.Parse(bworkerm.UpdDate);
                }
            }

            return model;
        }

        /// <summary>
        /// データ関連チェック
        /// </summary>
        /// <param name="bworkerm">作業者マスタデータ（画面入力値）</param>
        /// <returns></returns>
        private bool CheckRelation(BWorkerM bworkerm, int mode)
        {
            bool check = true;
            string errMsg = "";

            if (bworkerm.DelFlg) {
                errMsg = "削除対象の作業者を使用しているマスタが存在するため、データ削除できません。作業者ID=[" + bworkerm.WorkerId + "]";
            } else if (!bworkerm.ManagerKbn || bworkerm.NoDisplayKbn) {
                errMsg = "承認者として登録されているため、管理者以外または、表示対象外に更新できません。作業者ID=[" + bworkerm.WorkerId + "]";
            }

            // 削除の場合
            if (mode == 1)
            {
                // ログイン管理者チェック
                if (bworkerm.WorkerId.Equals((string)Session["LOGINMNGID"]))
                {
                    check = false;
                    ModelState.AddModelError(string.Empty, "管理者ログイン中の作業者は削除できません。作業者ID=[" + bworkerm.WorkerId + "]");
                }
            }

            // 承認経路マスタデータ存在チェック
            if (!string.IsNullOrEmpty(errMsg) && masterFunc.IsExistsApprovalrouteM(context: context, shopId: bworkerm.ShopId, managerId: bworkerm.WorkerId))
            {
                check = false;
                ModelState.AddModelError(string.Empty, errMsg);
            }

            return check;
        }

        /// <summary>
        /// 入力データチェック
        /// </summary>
        /// <param name="dt">作業者マスタデータ（画面入力値）</param>
        /// <param name="hsError">エラーメッセージセット</param>
        /// <returns>チェック結果（エラー：false）</returns>
        private bool CheckRequire(BWorkerM dt, ref HashSet<string> hsError)
        {
            bool checkError = true;
            // 入力項目のチェック
            if (string.IsNullOrEmpty(dt.WorkerName))
            {
                hsError.Add("作業者名を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].WorkerName", string.Empty);
                checkError = false;
            }
            // 桁数チェック
            if (checkError && dt.WorkerName.Length > 15)
            {
                hsError.Add("作業者名は15文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].WorkerName", string.Empty);
                checkError = false;
            }
            // メール送信時刻（自）
            if (!string.IsNullOrEmpty(dt.TransMissionTime1))
            {
                string strTime = dt.TransMissionTime1.Replace(":", "");
                string format = "HHmm";
                CultureInfo ci = CultureInfo.CurrentCulture;
                DateTimeStyles dts = DateTimeStyles.None;
                DateTime dateTime;
                if (!DateTime.TryParseExact(strTime, format, ci, dts, out dateTime))
                {
                    hsError.Add("メール送信時刻（自）に誤りがあります。HH:mmの形式で入力してください。");
                    ModelState.AddModelError("list[" + dt.No + "].TransMissionTime1", string.Empty);
                    checkError = false;
                }
            }
            // メール送信時刻（至）
            if (!string.IsNullOrEmpty(dt.TransMissionTime2))
            {
                string strTime = dt.TransMissionTime2.Replace(":", "");
                string format = "HHmm";
                CultureInfo ci = CultureInfo.CurrentCulture;
                DateTimeStyles dts = DateTimeStyles.None;
                DateTime dateTime;
                if (!DateTime.TryParseExact(strTime, format, ci, dts, out dateTime))
                {
                    hsError.Add("メール送信時刻（至）に誤りがあります。HH:mmの形式で入力してください。");
                    ModelState.AddModelError("list[" + dt.No + "].TransMissionTime2", string.Empty);
                    checkError = false;
                }
            }

            dt.TransMissionTime1Flg = false;
            dt.TransMissionTime2Flg = false;
            // メール送信時刻（自）（至）
            if (!string.IsNullOrEmpty(dt.TransMissionTime1) || !string.IsNullOrEmpty(dt.TransMissionTime2))
            {
                if (string.IsNullOrEmpty(dt.TransMissionTime1))
                {
                    hsError.Add("メール送信時刻（自）を入力してください。");
                    ModelState.AddModelError("list[" + dt.No + "].TransMissionTime1", string.Empty);
                    checkError = false;
                    dt.TransMissionTime1Flg = true;
                }
                if (string.IsNullOrEmpty(dt.TransMissionTime2))
                {
                    hsError.Add("メール送信時刻（至）を入力してください。");
                    ModelState.AddModelError("list[" + dt.No + "].TransMissionTime2", string.Empty);
                    checkError = false;
                    dt.TransMissionTime2Flg = true;
                }
            }
            if (!string.IsNullOrEmpty(dt.TransMissionTime1) && !string.IsNullOrEmpty(dt.TransMissionTime2))
            {
                // メールアドレス
                if (string.IsNullOrEmpty(dt.MailAddressPc) && string.IsNullOrEmpty(dt.MailAddressFeature))
                {
                    hsError.Add("メール送信時刻を設定した場合は、メールアドレスを入力してください。");
                    ModelState.AddModelError("list[" + dt.No + "].MailAddressPc", string.Empty);
                    ModelState.AddModelError("list[" + dt.No + "].MailAddressFeature", string.Empty);
                    checkError = false;
                }
            }

            // 管理者の場合
            if (dt.ManagerKbn)
            {
                // 承認ID
                if (string.IsNullOrEmpty(dt.AppId))
                {
                    hsError.Add("管理者IDを入力してください。");
                    ModelState.AddModelError("list[" + dt.No + "].AppId", string.Empty);
                    checkError = false;
                }
                // 承認パス
                if (string.IsNullOrEmpty(dt.AppPass))
                {
                    hsError.Add("管理者パスワードを入力してください。");
                    ModelState.AddModelError("list[" + dt.No + "].AppPass", string.Empty);
                    checkError = false;
                }

                // メールアドレス
                if (string.IsNullOrEmpty(dt.MailAddressPc) && string.IsNullOrEmpty(dt.MailAddressFeature))
                {
                    hsError.Add("メールアドレス（PC）または（携帯）のどちらかを入力してください。");
                    ModelState.AddModelError("list[" + dt.No + "].MailAddressPc", string.Empty);
                    ModelState.AddModelError("list[" + dt.No + "].MailAddressFeature", string.Empty);
                    checkError = false;
                }

                // メールアドレス形式チェック（PC）
                if (!string.IsNullOrEmpty(dt.MailAddressPc) && !comFunc.IsValidMailAddress(dt.MailAddressPc))
                {
                    hsError.Add("メールアドレス（PC）の形式が誤っています。");
                    ModelState.AddModelError("list[" + dt.No + "].MailAddressPc", string.Empty);
                    checkError = false;
                }

                // メールアドレス形式チェック（携帯）
                if (!string.IsNullOrEmpty(dt.MailAddressFeature) && !comFunc.IsValidMailAddress(dt.MailAddressFeature))
                {
                    hsError.Add("メールアドレス（携帯）の形式が誤っています。"); 
                    ModelState.AddModelError("list[" + dt.No + "].MailAddressFeature", string.Empty);
                    checkError = false;
                }
            } else
            {
                // ログイン中の管理者は管理者区分を外せない
                if (!string.IsNullOrEmpty(dt.WorkerId) && dt.WorkerId.Equals((string)Session["LOGINMNGID"]))
                {
                    hsError.Add("管理者ログイン中の作業者は管理者区分を変更できません。");
                    ModelState.AddModelError("list[" + dt.No + "].ManagerKbn", string.Empty);
                    checkError = false;
                }
            }

            return checkError;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="updList"></param>
        /// <param name="List"></param>
        /// <param name="hsError"></param>
        /// <returns></returns>
        private bool CheckUniqueness(BWorkerM dt, List<BWorkerM> updList, IQueryable<WorkerM> RegisteredList, ref HashSet<string> hsError)
        {
            bool checkError = true;

            if (!string.IsNullOrEmpty(dt.WorkerName)) {
                // 登録用データ内の重複をチェック
                List<BWorkerM> duplicateList = updList.FindAll(n => n.WorkerName == dt.WorkerName && n.DelFlg == false);
                if (duplicateList.Count > 1) {
                    hsError.Add("作業者名が重複しています。No=[" + (dt.No+1) + "]");
                    ModelState.AddModelError("list[" + dt.No + "].WorkerName", string.Empty);
                    checkError = false;
                }

                // 登録用データ以外の現在のDB登録データ内の重複をチェック
                IEnumerable<string> workerIds = updList.Select(item => { return item.WorkerId; });
                string[] workerIdArray = workerIds.ToArray();   // 登録用データ除外用
                List<WorkerM> registDuplicateList = new List<WorkerM>(RegisteredList.Where(a => a.WORKERNAME == dt.WorkerName && !workerIdArray.Contains(a.WORKERID)));
                if (registDuplicateList.Count > 0)
                {
                    hsError.Add("作業者名が既に登録されています。修正するか画面を更新して再度、登録してください。No=[" + (dt.No + 1) + "] 登録済作業者ID=[" + registDuplicateList[0].WORKERID + "]");
                    ModelState.AddModelError("list[" + dt.No + "].WorkerName", string.Empty);
                    checkError = false;
                }
            }
            return checkError;
        }

        /// <summary>
        /// 更新項目結果チェック
        /// </summary>
        /// <param name="registData">DBデータ</param>
        /// <param name="dt">画面データ</param>
        /// <returns>bool（true:更新あり、false：更新なし）</returns>
        private bool CheckDataUpd(List<WorkerM> registData, BWorkerM dt)
        {
            if (registData[0].WORKERNAME != dt.WorkerName
                || registData[0].MANAGERKBN != (dt.ManagerKbn ? "1" : "0")
                || registData[0].CATEGORYKBN1 != (dt.CategoryKbn1 ? "1" : "0")
                || registData[0].CATEGORYKBN2 != (dt.CategoryKbn2 ? "1" : "0")
                || registData[0].CATEGORYKBN3 != (dt.CategoryKbn3 ? "1" : "0")
                || registData[0].CATEGORYKBN4 != (dt.CategoryKbn4 ? "1" : "0")
                || registData[0].CATEGORYKBN5 != (dt.CategoryKbn5 ? "1" : "0")
                || registData[0].CATEGORYKBN6 != (dt.CategoryKbn6 ? "1" : "0")
                || registData[0].CATEGORYKBN7 != (dt.CategoryKbn7 ? "1" : "0")
                || registData[0].CATEGORYKBN8 != (dt.CategoryKbn8 ? "1" : "0")
                || registData[0].CATEGORYKBN9 != (dt.CategoryKbn9 ? "1" : "0")
                || registData[0].CATEGORYKBN10 != (dt.CategoryKbn10 ? "1" : "0")
                || registData[0].TRANSMISSIONTIME1 != dt.TransMissionTime1
                || registData[0].TRANSMISSIONTIME2 != dt.TransMissionTime2
                || registData[0].NODISPLAYKBN != (dt.NoDisplayKbn ? "1" : "0")
                )
            {
                return true;
            }

            // 管理者の場合
            if (dt.ManagerKbn 
                && (registData[0].APPID != dt.AppId
                    || registData[0].APPPASS != dt.AppPass
                    || registData[0].MAILADDRESSPC != dt.MailAddressPc
                    || registData[0].MAILADDRESSFEATURE != dt.MailAddressFeature))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 部門名称リスト生成
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        private string[] CreateCategoryNameList(string shopId)
        {
            // データ取得
            var categoryDt = from a in context.CategoryMs
                             where a.SHOPID == shopId
                             select a;

            string[] categoryNameList = new string[10];
            if (categoryDt.Count() != 0)
            {
                foreach (var dt in categoryDt)
                {
                    if (!string.IsNullOrEmpty(dt.CATEGORYNAME)) {
                        categoryNameList[int.Parse(dt.CATEGORYID)-1] = dt.CATEGORYNAME;
                    }
                }
            }

            return categoryNameList;
        }
    }
}