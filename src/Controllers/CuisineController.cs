using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using static HACCPExtender.Constants.Const;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using HACCPExtender.Controllers.Common;
using static HACCPExtender.Controllers.Common.CommonConstants;
using HACCPExtender.Business;

namespace HACCPExtender.Controllers
{
    public class CuisineController : Controller
    {
        private MasterContext context = new MasterContext();
        readonly CommonFunction comFunc = new CommonFunction();
        readonly MasterFunction masterFunc = new MasterFunction();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CuisineController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "Cuisine");
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
            ViewBag.editMode = comFunc.GetEditButton(editMode);

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // 登録メッセージを取得
            string registMsg = (string)Session["registMsg"];
            if (!string.IsNullOrEmpty(registMsg))
            {
                Session.Remove("registMsg");
                ViewBag.registMsg = registMsg;
            }

            // ドロップダウンリスト用データ取得設定
            List<LocationM> locationMList = masterFunc.GetLocationMData(context, shopId);
            ViewBag.locationMSelectListItem = comFunc.CreateLocationMOptionList(locationMList);

            // データ取得
            var managementDt = from a in context.ManagementMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                                && a.MANAGEMENTID == ManagementID.RYORI
                             select a;

            List<BManagementM> listManagement = new List<BManagementM>();

            if (managementDt.Count() == 0)
            {
                BManagementM bManagement = new BManagementM();
                listManagement.Add(bManagement);
            }
            else
            {
                foreach (var dt in managementDt)
                {
                    BManagementM bManagement = new BManagementM
                    {
                        // 編集モード
                        EditMode = 0,
                        // 削除
                        DelFlg = false,
                        // 管理対象ID
                        ManageId = dt.MANAGEID,
                        // 管理対象番号
                        ManageNo = dt.MANAGENO,
                        // 管理対象名称
                        ManageName = dt.MANAGENAME,
                        // 単位
                        Unit = dt.UNIT,
                        // 上限温度
                        UpperLimit = dt.UPPERLIMIT.ToString(),
                        // 下限温度
                        LowerLimit = dt.LOWERLIMIT.ToString(),
                        // 場所ID
                        LocationId = dt.LOCATIONID,
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
                    listManagement.Add(bManagement);
                }
            }

            // CSV位置(デフォルトは出力時と同じため出力用定義から作成)
            List<int> listCsvPos = new List<int>();
            CsvDef csvDefs = CsvManage.DEFS.Find(n => n.ManagementId == ManagementID.RYORI);
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

            BManagementMs managementVal = new BManagementMs
            {
                BManagementMList = listManagement,
                BCsvPosList = listCsvPos,
                BUploadFileList = listUploadFile,
            };

            return View(managementVal);
        }

        /// <summary>
        /// 行追加処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <returns>行追加データ</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Add(IList<BManagementM> list, List<int> posList, List<BUploadFile> uploadFileList)
        {
            // post時の情報をクリア
            ModelState.Clear();
            // 画面の表示順に並び替えてリストに設定
            IList<BManagementM> addList = list.OrderBy(BManagementM => BManagementM.No).ToList();

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // ドロップダウンリスト用データ取得設定
            List<LocationM> locationMList = masterFunc.GetLocationMData(context, shopId);
            ViewBag.locationMSelectListItem = comFunc.CreateLocationMOptionList(locationMList);

            // 追加分
            BManagementM addRow = new BManagementM
            {
                // 編集モード
                EditMode = 0
            };
            // 行追加
            addList.Add(addRow);
            BManagementMs managementVal = new BManagementMs
            {
                BManagementMList = addList,
                BCsvPosList = posList,
                BUploadFileList = uploadFileList,
            };

            return View("Show", managementVal);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="list">画面入力値(料理リスト)</param>
        /// <param name="posList">画面入力値</param>
        /// <param name="uploadFileList">CSV位置</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<BManagementM> list, List<int> posList, List<BUploadFile> uploadFileList)
        {
            // post時の情報をクリア
            ModelState.Clear();
            //  画面の表示順に並び替えてリストに設定
            List<BManagementM> updList = list.OrderBy(BManagementM => BManagementM.No).ToList();

            // 削除用データ
            var delManagementMs = new List<ManagementM>();
            // 登録データ
            var insManagementMs = new List<ManagementM>();
            // 更新データ
            var updManagementMs = new List<ManagementM>();
            // 表示順 DB登録用
            int rowNo = 1;
            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            //セッションから店舗ID, ユーザーIDを取得する
            string shopId = (string)Session["SHOPID"];
            string managerId = (string)Session["LOGINMNGID"];

            // ドロップダウンリスト用データ取得設定
            List<LocationM> locationMList = masterFunc.GetLocationMData(context, shopId);
            ViewBag.locationMSelectListItem = comFunc.CreateLocationMOptionList(locationMList);

            // 更新チェック用現在データ取得
            var managementDt = from a in context.ManagementMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                                && a.MANAGEMENTID == ManagementID.RYORI
                             select a;

            // 変更データ抽出（ソート番号以外）
            foreach (BManagementM dt in updList)
            {
                dt.ShopId = shopId;
                dt.ManagementId = ManagementID.RYORI;

                // 追加行（管理対象IDなし）で、削除checkあり or 必須項目がすべて未入力の場合は無視
                if (string.IsNullOrEmpty(dt.ManageId)
                    && (dt.DelFlg || 
                        (string.IsNullOrEmpty(dt.ManageNo) && string.IsNullOrEmpty(dt.ManageName))))
                {
                    continue;
                }

                // 削除checkありの場合は入力内容無視
                if (dt.DelFlg)
                {
                    // 削除対象に追加
                    delManagementMs.Add(this.SetManagementM(dt, dt.DisplayNo, managerId));
                    continue;
                }

                // 新規追加行
                if (string.IsNullOrEmpty(dt.ManageId))
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                        continue;
                    }

                    // 一意性チェック
                    if (!CheckUniqueness(dt, updList, managementDt, ref hsError))
                    {
                        checkError = false;
                    }

                    // 新規
                    insManagementMs.Add(this.SetManagementM(dt, rowNo, managerId, managerId));
                    rowNo++;
                    continue;
                }

                // DBデータから取得
                List<ManagementM> registData = new List<ManagementM>(managementDt.Where(a => a.MANAGEID == dt.ManageId));

                // DBにデータが存在しない場合
                if (registData.Count == 0)
                {
                    // データが更新されているため、排他エラーとする
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    BManagementMs managementData = new BManagementMs
                    {
                        BManagementMList = updList,
                        BCsvPosList = posList,
                        BUploadFileList = uploadFileList,
                    };
                    return View("Show", managementData);
                }
                else if (this.CheckDataUpd(registData, dt))
                {
                    // データ整合性チェック（更新対象）ソート番号のみの更新は除く

                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                        continue;
                    }

                    // 一意性チェック
                    if (!CheckUniqueness(dt, updList, managementDt, ref hsError))
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
                updManagementMs.Add(this.SetManagementM(dt, rowNo, managerId));
                rowNo++;
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                BManagementMs managementData = new BManagementMs
                {
                    BManagementMList = updList,
                    BCsvPosList = posList,
                    BUploadFileList = uploadFileList,
                };

                return View("Show", managementData);
            }

            // DB更新
            if (delManagementMs.Count > 0 || updManagementMs.Count > 0 || insManagementMs.Count > 0)
            {
                using (context = new MasterContext()) 
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {

                            // データ削除
                            if (delManagementMs.Count > 0)
                            {
                                foreach (ManagementM deldata in delManagementMs)
                                {
                                    context.ManagementMs.Attach(deldata);
                                }
                                context.ManagementMs.RemoveRange(delManagementMs);
                                context.SaveChanges();
                            }

                            // データ登録
                            foreach (ManagementM insModel in insManagementMs)
                            {
                                // 管理対象IDを採番
                                insModel.MANAGEID = masterFunc.GetNumberingID(
                                    context: context, tableName: "MANAGEMENT_M", columnName: "MANAGEID", shopId: insModel.SHOPID, managementId: ManagementID.RYORI, digits: 5);
                                // データ登録
                                context.ManagementMs.Add(insModel);
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updManagementMs.Count > 0)
                            {
                                foreach (ManagementM upddata in updManagementMs)
                                {
                                    context.ManagementMs.Attach(upddata);
                                    context.Entry(upddata).State = EntityState.Modified;
                                }
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
                            BManagementMs managementData = new BManagementMs
                            {
                                BManagementMList = updList,
                                BCsvPosList = posList,
                                BUploadFileList = uploadFileList,
                            };

                            return View("Show", managementData);
                        }
                        catch (DbUpdateException ex)
                        {   
                            if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                            {
                                //一意制約エラー
                                // ロールバック
                                tran.Rollback();
                                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                                BManagementMs managementData = new BManagementMs
                                {
                                    BManagementMList = updList,
                                    BCsvPosList = posList,
                                    BUploadFileList = uploadFileList,
                                };

                                return View("Show", managementData);
                            }
                            else
                            {
                                // ロールバック
                                tran.Rollback();
                                LogHelper.Default.WriteError(ex.Message, ex);
                                throw new ApplicationException();
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
        /// CSVファイルデータアップロード処理
        /// </summary>
        /// <param name="list">画面入力値（管理対象データ）</param>
        /// <param name="posList">画面入力値</param>
        /// <param name="uploadFileList">CSVファイル</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult FileUpload(IList<BManagementM> list, List<int> posList, List<BUploadFile> uploadFileList)
        {
            // post時の情報をクリア
            ModelState.Clear();
            // 画面の表示順に並び替えてリストに設定
            List<BManagementM> dispList = list.OrderBy(BManagementM => BManagementM.No).ToList();

            // アップロード日時
            string updateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            // チェックエラー
            bool checkError = true;
            // エラー
            HashSet<string> hsError = new HashSet<string>();

            // CSV管理情報を元に位置順に並び替えてリストを作成
            CsvDef csvDefs = CsvManage.DEFS.Find(n => n.ManagementId == ManagementID.RYORI);
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

            // ドロップダウンリスト用データ取得設定
            List<LocationM> locationMList = masterFunc.GetLocationMData(context, shopId);
            ViewBag.locationMSelectListItem = comFunc.CreateLocationMOptionList(locationMList);

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
                // 文言
                hsError.Add("CSVファイルが存在しません。");
                checkError = false;
            }
            else
            {
                //CSVファイルかどうかチェック(拡張子の確認)
                string extension = System.IO.Path.GetExtension(uploadFileList[0].UploadFile.FileName).ToUpper();
                if (!".CSV".Equals(extension))
                {
                    hsError.Add("CSVファイルをアップロードしてください。");
                    checkError = false;
                }
                // ファイル長チェック
                if (uploadFileList[0].UploadFile.ContentLength == 0)
                {
                    // 文言
                    hsError.Add("CSVファイルが不正です。ファイルサイズ=[0]");
                    checkError = false;
                }
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError || hsError.Count > 0)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                BManagementMs managementData = new BManagementMs
                {
                    BManagementMList = dispList,
                    BCsvPosList = posList,
                    BUploadFileList = uploadFileList,
                };

                return View("Show", managementData);
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
                    // 文言
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
                    // 文言
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
            if (!checkError || hsError.Count > 0)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                BManagementMs managementData = new BManagementMs
                {
                    BManagementMList = dispList,
                    BCsvPosList = posList,
                    BUploadFileList = uploadFileList,
                };

                return View("Show", managementData);
            }

            var locationDt = from a in context.LocationMs
                               where a.SHOPID == shopId
                               select a;
            List<LocationM> locations = locationDt.ToList();
            List<String> locationList = new List<string>();
            foreach(LocationM item in locations)
            {
                locationList.Add(item.LOCATIONID);
            }

            // 更新チェック用現在データ取得
            var managementDt = from a in context.ManagementMs
                               orderby a.DISPLAYNO
                               where a.SHOPID == shopId
                                  && a.MANAGEMENTID == ManagementID.RYORI
                               select a;

            // 登録データ
            var insManagementMs = new List<ManagementM>();
            // 更新データ
            var updManagementMs = new List<ManagementM>();
            // 表示順 DB登録用
            int rowNo = (managementDt.Count() == 0) ? 1 : managementDt.OrderByDescending(n => n.DISPLAYNO).First().DISPLAYNO;

            // CSV行番号
            int lineNo = 0;
            if (firstLineSkip)
            {
                lineNo = 1;
            }

            foreach (var csvDt in listCsv)
            {
                lineNo++;
                // DBデータから取得
                //料理番号（文字列可）は空の文字列またはNULLをチェックする。（必須チェック）
                string targetManageNo = csvDt[(csvColumns.Find(n => n.Name == "MANAGENO").Pos - 1)];
                if (string.IsNullOrEmpty(targetManageNo))
                {
                    hsError.Add("CSVファイルが不正です。料理番号の値を入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //料理番号の長さをチェックする。（桁数チェック）
                else if (targetManageNo.Length > 20)
                {
                    hsError.Add("CSVファイルが不正です。料理番号は20文字以内で入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //料理名は空の文字列またはNULLをチェックする。（必須チェック）
                string targetManageName = csvDt[(csvColumns.Find(n => n.Name == "MANAGENAME").Pos - 1)];
                if (string.IsNullOrEmpty(targetManageName))
                {
                    hsError.Add("CSVファイルが不正です。料理名の値を入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //料理名の長さをチェックする。（桁数チェック）
                else if (targetManageName.Length > 20)
                {
                    hsError.Add("CSVファイルが不正です。料理名は20文字以内で入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }

                //単位
                string targetUnit = csvDt[(csvColumns.Find(n => n.Name == "UNIT").Pos - 1)];
                //単位は空の文字列またはNULLをチェックする。（必須チェック）
                if (string.IsNullOrEmpty(targetUnit))
                {
                    hsError.Add("CSVファイルが不正です。単位の値を入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //単位の長さをチェックする。（桁数チェック）
                else if (targetUnit.Length > 5)
                {
                    hsError.Add("CSVファイルが不正です。単位は5文字以内で入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }

                //上限温度
                string targetUpperLimit = csvDt[(csvColumns.Find(n => n.Name == "UPPERLIMIT").Pos - 1)];
                //上限温度は空の文字列またはNULLをチェックする。（必須チェック）
                if (string.IsNullOrEmpty(targetUpperLimit))
                {
                    hsError.Add("CSVファイルが不正です。上限温度の値を入力してください。行番号=[" + lineNo + "] ");
             
                    checkError = false;
                }
                //上限温度は半角数値をチェックする。
                else if (!CheckFunction.CheckInteger(targetUpperLimit))
                {
                    hsError.Add("CSVファイルが不正です。上限温度を半角整数で入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //上限温度の大きさをチェックする。
                else if (!CheckFunction.CheckOverflow(targetUpperLimit))
                {
                    hsError.Add("CSVファイルが不正です。上限温度の値が大きすぎます。行番号=[" + lineNo + "] ");
                    checkError = false;
                }

                //下限温度
                string targetLowerLimit = csvDt[(csvColumns.Find(n => n.Name == "LOWERLIMIT").Pos - 1)];
                //下限温度は空の文字列またはNULLをチェックする。（必須チェック）
                if (string.IsNullOrEmpty(targetLowerLimit))
                {
                    hsError.Add("CSVファイルが不正です。下限温度の値を入力してください。行番号=[" + lineNo + "] ");
                
                    checkError = false;
                }
                //下限温度は半角数値をチェックする。
                else if (!CheckFunction.CheckInteger(targetLowerLimit))
                {
                    hsError.Add("CSVファイルが不正です。下限温度を半角整数で入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //下限温度の大きさをチェックする。
                else if (!CheckFunction.CheckOverflow(targetLowerLimit))
                {
                    hsError.Add("CSVファイルが不正です。下限温度の値が大きすぎます。行番号=[" + lineNo + "] ");
                    checkError = false;
                }

                //上限温度、下限温度の大きさを比較する。
                if (checkError && !CheckFunction.CheckBigness(targetLowerLimit, targetUpperLimit))
                {
                    hsError.Add("CSVファイルが不正です。下限温度を上限温度以下で入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //中分類ID
                string targetLocationID = csvDt[(csvColumns.Find(n => n.Name == "LOCATIONID").Pos - 1)];
                //中分類IDは空の文字列またはNULLをチェックする。（必須チェック）
                if (string.IsNullOrEmpty(targetLocationID))
                {
                    hsError.Add("CSVファイルが不正です。中分類IDの値を入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //中分類IDの長さをチェックする。（桁数チェック）
                else if (targetLocationID.Length > 2)
                {  
                    hsError.Add("CSVファイルが不正です。中分類IDは2文字以内で入力してください。行番号=[" + lineNo + "] ");
                    checkError = false;
                }
                //CSVファイルに記載された中分類IDはDBにあるかどうかをチェックする。
                else if (!locationList.Contains(targetLocationID))
                {
                    hsError.Add("CSVファイルが不正です。中分類IDは存在しません。行番号=[" + lineNo + "] ");
                    checkError = false;
                }

                if (!checkError) continue;

                List<ManagementM> registData = new List<ManagementM>(managementDt.Where(a => a.MANAGENO == targetManageNo));

                // DBにデータが複数存在する場合
                if (registData.Count > 1)
                {
                    hsError.Add("既存データが不正です。仕入先番号（文字列可）=[" + targetManageNo + "] のデータが複数存在します。");
                    checkError = false;
                    continue;
                }

                var model = new ManagementM();

                // DBにデータが存在する場合
                if (registData.Count > 0)
                {
                    model = registData[0];
                }

                // 新規・更新共通設定項目
                foreach (CsvColumn csvColDt in csvColumns)
                { 
                    // 対象データのカラム名よりデータ設定
                    PropertyInfo pInfo = typeof(ManagementM).GetProperty(csvColDt.Name);
                    if (pInfo.PropertyType.Name == "String")
                    {
                        pInfo.SetValue(model, csvDt[csvColDt.Pos - 1], null);
                    }
                    else if (pInfo.PropertyType.Name == "Int16")
                    {
                        pInfo.SetValue(model, short.Parse(csvDt[csvColDt.Pos - 1]), null);
                    }
                }
                model.UPDUSERID = managerId;

                // DBにデータが存在する場合
                if (registData.Count > 0)
                {
                    updManagementMs.Add(model);
                } else
                {
                    // 店舗ID
                    model.SHOPID = shopId;
                    // 管理ID
                    model.MANAGEMENTID = ManagementID.RYORI;
                    // 表示No
                    model.DISPLAYNO = (short)rowNo++;
                    // 登録ユーザーID
                    model.INSUSERID = managerId;

                    insManagementMs.Add(model);
                }
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError || hsError.Count > 0)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }
                BManagementMs managementData = new BManagementMs
                {
                    BManagementMList = dispList,
                    BCsvPosList = posList,
                    BUploadFileList = uploadFileList,
                };

                return View("Show", managementData);
            }

            // DB更新
            if (updManagementMs.Count > 0 || insManagementMs.Count > 0)
            {
                using (context = new MasterContext())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // CSV履歴情報
                            // 更新チェック用現在データ取得
                            var csvHistoryDt = from a in context.CsvHistoryTs
                                                where a.SHOPID == shopId
                                                    && a.MANAGEMENTID == ManagementID.RYORI
                                                select a;

                            // DBデータから取得
                            List<CsvHistoryT> registCsvHistory = new List<CsvHistoryT>(csvHistoryDt.Where(a => a.UPLOADDATE == updateTime));

                            var csvHistory = new CsvHistoryT
                            {
                                SHOPID = shopId,
                                MANAGEMENTID = ManagementID.RYORI,
                                UPLOADDATE = updateTime,
                                FILENAME = uploadFileList[0].UploadFile.FileName
                            };

                            var targetItem = csvColumns.Find(n => n.Name == "MANAGENO");
                            if (targetItem != null)
                            {
                                csvHistory.CODEPOS = (short)targetItem.Pos;
                            }
                            targetItem = csvColumns.Find(n => n.Name == "MANAGENAME");
                            if (targetItem != null)
                            {
                                csvHistory.DATAPOS = (short)targetItem.Pos;
                            }
                            targetItem = csvColumns.Find(n => n.Name == "UNIT");
                            if (targetItem != null)
                            {
                                csvHistory.UNITPOS = (short)targetItem.Pos;
                            }
                            targetItem = csvColumns.Find(n => n.Name == "UPPERLIMIT");
                            if (targetItem != null)
                            {
                                csvHistory.UPPERLIMITPOS = (short)targetItem.Pos;
                            }
                            targetItem = csvColumns.Find(n => n.Name == "LOWERLIMIT");
                            if (targetItem != null)
                            {
                                csvHistory.LOWERLIMITPOS = (short)targetItem.Pos;
                            }
                            targetItem = csvColumns.Find(n => n.Name == "LOCATIONID");
                            if (targetItem != null)
                            {
                                csvHistory.LOCATIONIDPOS = (short)targetItem.Pos;
                            }
                            csvHistory.INSUSERID = managerId;
                            csvHistory.UPDUSERID = managerId;

                            if (registCsvHistory.Count() > 0)
                            {
                                csvHistory.INSUSERID = registCsvHistory[0].INSUSERID;
                                csvHistory.UPDDATE = registCsvHistory[0].UPDDATE;
                                context.CsvHistoryTs.Attach(csvHistory);
                                context.Entry(csvHistory).State = EntityState.Modified;
                            }
                            else
                            {
                                // データ登録
                                context.CsvHistoryTs.Add(csvHistory);
                            }
                            context.SaveChanges();

                            foreach (ManagementM insModel in insManagementMs)
                            {
                                // 管理対象IDを採番
                                insModel.MANAGEID = masterFunc.GetNumberingID(
                                    context: context, tableName: "MANAGEMENT_M", columnName: "MANAGEID", shopId: insModel.SHOPID, managementId: ManagementID.RYORI, digits: 5);
                                // データ登録
                                context.ManagementMs.Add(insModel);
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updManagementMs.Count > 0)
                            {
                                foreach (ManagementM upddata in updManagementMs)
                                {
                                    context.ManagementMs.Attach(upddata);
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
                            BManagementMs managementData = new BManagementMs
                            {
                                BManagementMList = dispList,
                                BCsvPosList = posList,
                                BUploadFileList = uploadFileList,
                            };
                            return View("Show", managementData);
                        }
                        catch (DbUpdateException ex)
                        {
                            if (ex.InnerException.InnerException.Message.IndexOf("SQL0803N") >= 0)
                            {
                                //一意制約エラー
                                // ロールバック
                                tran.Rollback();
                                ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                                BManagementMs managementData = new BManagementMs
                                {
                                    BManagementMList = dispList,
                                    BCsvPosList = posList,
                                    BUploadFileList = uploadFileList,
                                };
                                return View("Show", managementData);
                            }
                            else
                            {
                                // ロールバック
                                tran.Rollback();
                                LogHelper.Default.WriteError(ex.Message, ex);
                                throw ex;
                            }
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
        /// CSVファイルエクスポート
        /// </summary>
        /// <returns>CSVエクスポートファイル</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CsvExport()
        {
            // CSV管理情報を元に位置順に並び替えてリストを作成
            CsvDef csvDefs = CsvManage.DEFS.Find(n => n.ManagementId == ManagementID.RYORI);
            List<CsvColumn> csvColumns = csvDefs.Columns.OrderBy(n => n.Pos).ToList();
            List<string> csvTitles = new List<string>();
            List<string> dbColumns = new List<string>();
            foreach (CsvColumn dt in csvColumns)
            {
                // タイトル名設定
                csvTitles.Add(dt.Title);
                // 対象データのカラム名設定
                dbColumns.Add(dt.Name);
            }

            // エンコードタイプ設定
            Encoding enc = Encoding.GetEncoding("shift_jis");

            // 出力用配列にタイトル名設定
            byte[] csv = enc.GetBytes("\"" + string.Join("\",\"", csvTitles.ToArray()) + "\"\r\n");

            // セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // データ取得
            var managementDt = from a in context.ManagementMs
                               orderby a.DISPLAYNO
                               where a.SHOPID == shopId
                                  && a.MANAGEMENTID == ManagementID.RYORI
                               select a;

            foreach (var mngDt in managementDt)
            {
                // 対象データのカラム名より出力データ抽出
                List<string> dbData = new List<string>();
                foreach (var column in dbColumns)
                {
                    PropertyInfo pInfo = typeof(ManagementM).GetProperty(column);
                    dbData.Add(pInfo.GetValue(mngDt, null) == null ? string.Empty : pInfo.GetValue(mngDt, null).ToString());
                }
                // 出力用配列に1レコード分のデータ追加
                csv = csv.Concat(enc.GetBytes("\"" + string.Join("\",\"", dbData.ToArray()) + "\"\r\n")).ToArray();
            }

            return File(csv, "text/csv", csvDefs.FileName);
        }

        /// <summary>
        /// 管理対象データ項目移送（更新・削除用）
        /// </summary>
        /// <param name="bmanagementm">画面入力値管理対象データ</param>
        /// <param name="orderNo"></param>
        /// <param name="updUserId">更新ユーザーID</param>
        /// <returns管理対象データ></returns>
        private ManagementM SetManagementM(BManagementM bmanagementm, int orderNo, string updUserId)
        {
            return SetManagementM(bmanagementm, orderNo, string.Empty, updUserId);
        }

        /// <summary>
        /// 管理対象データ項目移送
        /// </summary>
        /// <param name="bmanagementm">画面入力値管理対象データ</param>
        /// <returns>管理対象データ</returns>
        private ManagementM SetManagementM(BManagementM bmanagementm, int orderNo, string insUserId, string updUserId)
        {
            var model = new ManagementM
            {
                // 店舗ID
                SHOPID = bmanagementm.ShopId,
                // 管理ID
                MANAGEMENTID = bmanagementm.ManagementId,
                // 管理対象ID
                MANAGEID = bmanagementm.ManageId,
                // 管理対象番号
                MANAGENO = bmanagementm.ManageNo,
                // 管理対象名称
                MANAGENAME = bmanagementm.ManageName,
                // 単位
                UNIT = bmanagementm.Unit,
                // 上限温度
                UPPERLIMIT = (string.IsNullOrEmpty(bmanagementm.UpperLimit)) ? (short)0 : short.Parse(bmanagementm.UpperLimit),
                // 下限温度
                LOWERLIMIT = (string.IsNullOrEmpty(bmanagementm.LowerLimit)) ? (short)0 : short.Parse(bmanagementm.LowerLimit),
                // 場所ID
                LOCATIONID = bmanagementm.LocationId,
                // 表示No
                DISPLAYNO = (short)orderNo,
                // 登録ユーザーID
                INSUSERID = insUserId,
                // 更新ユーザーID
                UPDUSERID = updUserId,
            };
            // 削除・更新の場合
            if (string.IsNullOrEmpty(insUserId))
            {
                // 登録ユーザーID
                model.INSUSERID = bmanagementm.InsUserId;
                // 更新年月日
                if (bmanagementm.UpdDate != null)
                {
                    model.UPDDATE = DateTime.Parse(bmanagementm.UpdDate);
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
        private bool CheckRequire(BManagementM dt, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 入力項目のチェック
            // 料理番号
            if (string.IsNullOrEmpty(dt.ManageNo))
            {
                hsError.Add("料理番号（文字列可）を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].ManageNo", string.Empty);
                checkError = false;
            }else if (dt.ManageNo.Length > 20)
            {
                // 桁数チェック
                hsError.Add("料理番号（文字列可）は20文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].ManageNo", string.Empty);
                checkError = false;
            }
            // 料理名
            if (string.IsNullOrEmpty(dt.ManageName))
            {
                hsError.Add("料理名を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].ManageName", string.Empty);
                checkError = false;
            }else if (dt.ManageName.Length > 20)
            {
                // 桁数チェック
                hsError.Add("料理名は20文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].ManageName", string.Empty);
                checkError = false;
            }
            // 単位
            if (string.IsNullOrEmpty(dt.Unit))
            {
                hsError.Add("単位を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].Unit", string.Empty);
                checkError = false;
            }else if (dt.Unit.Length > 5)
            {
                hsError.Add("単位は5文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].Unit", string.Empty);
                checkError = false;
            }
            // 上限温度
            if (string.IsNullOrEmpty(dt.UpperLimit))
            {
                hsError.Add("上限温度を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].UpperLimit", string.Empty);
                checkError = false;
            }else if (!CheckFunction.CheckInteger(dt.UpperLimit))
            {
                // 温度など半角・数値チェック
                hsError.Add("上限温度を半角整数で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].UpperLimit", string.Empty);
                checkError = false;
            }
            else if (!CheckFunction.CheckOverflow(dt.UpperLimit))
            {
                hsError.Add("上限温度の値が大きすぎます。");
                ModelState.AddModelError("list[" + dt.No + "].UpperLimit", string.Empty);
                checkError = false;
            }
            // 下限温度
            if (string.IsNullOrEmpty(dt.LowerLimit))
            {
                hsError.Add("下限温度を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].LowerLimit", string.Empty);
                checkError = false;
            }
            else if (!CheckFunction.CheckInteger(dt.LowerLimit))
            {
                // 温度など半角・数値チェック
                hsError.Add("下限温度を半角整数で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].LowerLimit", string.Empty);
                checkError = false;
            }
            else if (!CheckFunction.CheckOverflow(dt.LowerLimit))
            {
                hsError.Add("下限温度の値が大きすぎます。");
                ModelState.AddModelError("list[" + dt.No + "].LowerLimit", string.Empty);
                checkError = false;
            }

            // 中分類名
            if (string.IsNullOrEmpty(dt.LocationId))
            {
                hsError.Add("中分類名を選択してください。");
                ModelState.AddModelError("list[" + dt.No + "].LocationId", string.Empty);
                checkError = false;
            }
            // 上限、下限の大小比較
            if (checkError && !CheckFunction.CheckBigness(dt.LowerLimit, dt.UpperLimit))
            {
                hsError.Add("下限温度を上限温度以下で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].LowerLimit", string.Empty);
                checkError = false;
            }
            
            return checkError;
        }

        /// <summary>
        /// データ重複チェック
        /// </summary>
        /// <param name="dt">画面入力値</param>
        /// <param name="updList"></param>
        /// <param name="List"></param>
        /// <param name="hsError"></param>
        /// <returns>チェック結果（重複ありエラー：false）</returns>
        private bool CheckUniqueness(BManagementM dt, List<BManagementM> updList, IQueryable<ManagementM> RegisteredList, ref HashSet<string> hsError)
        {
            bool checkError = true;

            if (!string.IsNullOrEmpty(dt.ManageNo)) {
                // 登録用データ内の重複をチェック
                List<BManagementM> duplicateList = updList.FindAll(n => n.ManageNo == dt.ManageNo && n.DelFlg == false);
                if (duplicateList.Count > 1) {
                    hsError.Add("料理番号（文字列可）が重複しています。No=[" + (dt.No+1) + "]");
                    ModelState.AddModelError("list[" + dt.No + "].ManageNo", string.Empty);
                    checkError = false;
                }

                // 登録用データ以外の現在のDB登録データ内の重複をチェック
                IEnumerable<string> manageIds = updList.Select(item => { return item.ManageId; });
                string[] manageIdArray = manageIds.ToArray();   // 登録用データ除外用
                List<ManagementM> registDuplicateList = new List<ManagementM>(RegisteredList.Where(a => a.MANAGENO == dt.ManageNo && !manageIdArray.Contains(a.MANAGEID)));
                if (registDuplicateList.Count > 0)
                {
                    hsError.Add("料理番号（文字列可）が既に登録されています。修正するか画面を更新して再度、登録してください。No=[" + (dt.No + 1) + "] 登録済料理ID=[" + registDuplicateList[0].MANAGEID + "]");
                    ModelState.AddModelError("list[" + dt.No + "].ManageNo", string.Empty);
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
        private bool CheckDataUpd(List<ManagementM> registData, BManagementM dt)
        {
            return (registData[0].MANAGENO != dt.ManageNo
                || registData[0].MANAGENAME != dt.ManageName
                || registData[0].UNIT != dt.Unit
                || string.IsNullOrEmpty(dt.UpperLimit)
                || registData[0].UPPERLIMIT.ToString() != dt.UpperLimit
                || string.IsNullOrEmpty(dt.LowerLimit)
                || registData[0].LOWERLIMIT.ToString() != dt.LowerLimit
                || registData[0].LOCATIONID != dt.LocationId
                );
        }
    }
}