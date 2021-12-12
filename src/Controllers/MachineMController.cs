using HACCPExtender.Business;
using HACCPExtender.Controllers.Common;
using HACCPExtender.Models;
using HACCPExtender.Models.Bussiness;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using static HACCPExtender.Controllers.Common.CommonConstants;

namespace HACCPExtenfer.Controllers
{
    public class MachineMController : Controller
    {
        // コンテキスト
        private MasterContext context = new MasterContext();
        // 未登録
        private readonly int EDIT_NOTREGIST = 0;
        // 未更新
        private readonly int EDIT_NOTUPDATE = 1;

        /// <summary>
        /// 
        /// </summary>
        public MachineMController()
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
            string fileName = GetAppSet.GetAppSetValue("Screenexplanation", "MachineM");
            if (!string.IsNullOrEmpty(fileName))
            {
                ViewBag.screenExplanation = strPathAndQuery + fileName;
            }

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 初期表示
        /// </summary>
        /// <returns>なし</returns>
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

            // ドロップダウンリスト用データ取得設定
            List<LocationM> locationMList = this.GetLocationMData(shopId);
            if (locationMList.Count() == 0)
            {
                // 中分類データが存在しない場合メッセージを表示
                ModelState.AddModelError(string.Empty, MsgConst.NODATA_LOCATION);
                ViewBag.editMode = "disabled";
            }
            ViewBag.locationMSelectListItem = CreateLocationMOptionList(locationMList);

            // データ取得
            var machineDt = from a in context.MachineMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            List<BMachineM> listMachine = new List<BMachineM>();

            if (machineDt.Count() > 0)
            {
                foreach (var dt in machineDt)
                {
                    BMachineM bMachine = new BMachineM
                    {
                        // 未更新
                        EditMode = EDIT_NOTUPDATE,
                        // 削除
                        DelFlg = false,
                        // 場所ID
                        LocationId = dt.LOCATIONID,
                        // 機器ID
                        MachineId = dt.MACHINEID,
                        // 機器名称
                        MachineName = dt.MACHINENAME,
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
                    listMachine.Add(bMachine);
                }
            }

            return View(listMachine);
        }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add(IList<BMachineM> list)
        {
            // post時の情報をクリア
            ModelState.Clear();
            // 画面の表示順に並び替えてリストに設定
            IList<BMachineM> addList = list.OrderBy(BMachineM => BMachineM.No).ToList();

            //セッションから店舗IDを取得する
            string shopId = (string)Session["SHOPID"];

            // ドロップダウンリスト用データ取得設定
            List<LocationM> locationMList = GetLocationMData(shopId);
            ViewBag.locationMSelectListItem = CreateLocationMOptionList(locationMList);

            // 追加分
            BMachineM addRow = new BMachineM
            {
                // 未登録
                EditMode = EDIT_NOTREGIST
            };
            // 行追加
            addList.Add(addRow);

            return View("Show", addList);
        }

        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="list">画面入力値</param>
        /// <returns>初期処理</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<BMachineM> list)
        {
            // post時の情報をクリア
            ModelState.Clear();
            //  画面の表示順に並び替えてリストに設定
            List<BMachineM> updList = list.OrderBy(BMachineM => BMachineM.No).ToList();

            // 削除用データ
            var delMachineMs = new List<MachineM>();
            // 登録データ
            var insMachineMs = new List<MachineM>();
            // 更新データ
            var updMachineMs = new List<MachineM>();
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
            List<LocationM> locationMList = GetLocationMData(shopId);
            ViewBag.locationMSelectListItem = CreateLocationMOptionList(locationMList);

            // 更新チェック用現在データ取得
            var machineDt = from a in context.MachineMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            // 変更データ抽出（ソート番号以外）
            foreach (BMachineM dt in updList)
            {
                dt.ShopId = shopId;

                // 追加行（機器IDなし）で、削除checkあり or 必須項目がすべて未入力の場合は無視
                if (string.IsNullOrEmpty(dt.MachineId)
                    && (dt.DelFlg
                        || (string.IsNullOrEmpty(dt.MachineName)
                            && string.IsNullOrEmpty(dt.LocationId))))
                {
                    continue;
                }

                // 削除checkありの場合は入力内容無視
                if (dt.DelFlg)
                {
                    // 削除対象に追加
                    delMachineMs.Add(this.SetMachineM(dt, dt.DisplayNo, managerId));
                    rowNo++;
                    continue;
                }

                // 新規追加行
                if (string.IsNullOrEmpty(dt.MachineId))
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
                    {
                        checkError = false;
                    }
                    // 新規
                    insMachineMs.Add(this.SetMachineM(dt, rowNo, managerId, managerId));
                    rowNo++;
                    continue;
                }

                // DBデータから取得
                List<MachineM> registData = new List<MachineM>(machineDt.Where(a => a.MACHINEID == dt.MachineId));

                // DBにデータが存在しない場合
                if (registData.Count == 0)
                {
                    // データが更新されているため、排他エラーとする
                    ModelState.AddModelError(string.Empty, MsgConst.ERR_EXCLUSIVE);
                    return View("Show", updList);
                }
                else if (this.CheckDataUpd(registData, dt))
                {
                    // 必須入力チェック
                    if (!CheckRequire(dt, ref hsError))
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
                updMachineMs.Add(this.SetMachineM(dt, rowNo, managerId));
                rowNo++;
            }

            // チェックエラーがある場合、エラーメッセージ表示
            if (!checkError)
            {
                foreach (string word in hsError)
                {
                    ModelState.AddModelError(string.Empty, word);
                }

                return View("Show", updList);
            }

            // DB更新
            if (delMachineMs.Count > 0 || updMachineMs.Count > 0 || insMachineMs.Count > 0)
            {
                using (context = new MasterContext())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // マスタ共通処理
                            var comm = new MasterFunction();

                            // データ削除
                            if (delMachineMs.Count > 0)
                            {
                                foreach (MachineM deldata in delMachineMs)
                                {
                                    context.MachineMs.Attach(deldata);
                                }
                                context.MachineMs.RemoveRange(delMachineMs);
                                context.SaveChanges();
                            }

                            // データ登録
                            foreach (MachineM insdata in insMachineMs)
                            {
                                // 機器IDを採番
                                insdata.MACHINEID = comm.GetNumberingID(
                                    context: context, tableName: "MACHINE_M", columnName: "MACHINEID", shopId: insdata.SHOPID, digits: 2);
                                // データ登録
                                context.MachineMs.Add(insdata);
                                context.SaveChanges();
                            }

                            // データ更新
                            if (updMachineMs.Count > 0)
                            {
                                foreach (MachineM upddata in updMachineMs)
                                {
                                    context.MachineMs.Attach(upddata);
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
                            return View("Show", updList);
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
            Session.Add("registMsg", MsgConst.REGIST_NORMAL_MSG);
            // 初期表示処理に戻す
            return RedirectToAction("Show");
        }

        /// <summary>
        /// 中分類マスタデータ取得
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <returns>中分類マスタデータ</returns>
        private List<LocationM> GetLocationMData(string shopId)
        {
            // データ取得
            var locationDt = from a in context.LocationMs
                             orderby a.DISPLAYNO
                             where a.SHOPID == shopId
                             select a;

            List<LocationM> locationMList = locationDt.ToArray().ToList();

            return locationMList;
        }

        /// <summary>
        /// 中分類マスタドロップダウンリスト用選択オプション生成
        /// </summary>
        /// <param name="locationMList">中分類データリスト</param>
        /// <returns>ドロップダウンリスト</returns>
        private SelectListItem[] CreateLocationMOptionList(List<LocationM> locationMList)
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
        /// 
        /// </summary>
        /// <param name="bmachinem"></param>
        /// <param name="orderNo"></param>
        /// <param name="updUserId"></param>
        /// <returns></returns>
        private MachineM SetMachineM(BMachineM bmachinem, int orderNo, string updUserId)
        {
            return SetMachineM(bmachinem, orderNo, string.Empty, updUserId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmachinem"></param>
        /// <returns></returns>
        private MachineM SetMachineM(BMachineM bmachinem, int orderNo, string insUserId, string updUserId)
        {
            var model = new MachineM
            {
                // 店舗ID
                SHOPID = bmachinem.ShopId,
                // 場所ID
                LOCATIONID = bmachinem.LocationId,
                // 機器ID
                MACHINEID = bmachinem.MachineId,
                // 機器名称
                MACHINENAME = bmachinem.MachineName,
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
                model.INSUSERID = bmachinem.InsUserId;
                // 更新年月日
                if (bmachinem.UpdDate != null)
                {
                    model.UPDDATE = DateTime.Parse(bmachinem.UpdDate);
                }
            }

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="hsError"></param>
        /// <returns></returns>
        private bool CheckRequire(BMachineM dt, ref HashSet<string> hsError)
        {
            bool checkError = true;

            // 入力項目のチェック
            if (string.IsNullOrEmpty(dt.MachineName))
            {
                hsError.Add("機器名を入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].MachineName", string.Empty);
                checkError = false;
            }else if (dt.MachineName.Length > 20)
            {
                // 桁数チェック
                hsError.Add("機器名は20文字以内で入力してください。");
                ModelState.AddModelError("list[" + dt.No + "].MachineName", string.Empty);
                checkError = false;
            }
            if (string.IsNullOrEmpty(dt.LocationId))
            {
                hsError.Add("使用場所を選択してください。");
                ModelState.AddModelError("list[" + dt.No + "].LocationId", string.Empty);
                checkError = false;
            }
            
            return checkError;
        }

        /// <summary>
        /// 更新項目結果チェック
        /// </summary>
        /// <param name="registData">DBデータ</param>
        /// <param name="dt">画面データ</param>
        /// <returns>bool（true:更新あり、false：更新なし）</returns>
        private bool CheckDataUpd(List<MachineM> registData, BMachineM dt)
        {
            return (registData[0].MACHINENAME != dt.MachineName
                || registData[0].LOCATIONID != dt.LocationId);
        }
    }
}