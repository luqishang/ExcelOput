using DocumentFormat.OpenXml.Drawing.Diagrams;
using HACCPExtender.Business;
using System.Collections.Generic;

namespace HACCPExtender.Controllers.Common
{
    public static class CommonConstants
    {
        public static class BoolKbn
        {
            // 区分値：0
            public static readonly string KBN_FALSE = "0";
            // 区分値：1
            public static readonly string KBN_TRUE = "1";
        }

        public static class MsgConst
        {
            // 正常登録
            public static readonly string REGIST_NORMAL_MSG = "正常に登録されました。";

            // CSV正常登録
            public static readonly string REGIST_CSV_NORMAL_MSG = "CSVデータから正常に登録されました。";

            // 更新
            public static readonly string UPDATE = "更新";
            
            // 削除
            public static readonly string DELETE = "削除";

            // 排他エラー
            public static readonly string ERR_EXCLUSIVE = "更新の競合が検出されました。画面を更新して再度、登録してください。";

            // 登録データなし
            public static readonly string NOREGIST_DATA = "登録対象データが存在しません。";

            // データなし
            public static readonly string NO_DATA = "登録データが存在しません。";

            // 関連チェック（設問マスタ）
            public static readonly string RELERR_QUESTION_MSG = "設問マスタにデータが存在するため、{0}できません。{1}=[{2}]";

            // 関連チェック（帳票マスタ）
            public static readonly string RELERR_REPORT_MSG = "帳票マスタにデータが存在するため、{0}できません。{1}=[{2}]";

            // 関連チェック（承認マスタ）
            public static readonly string RELERR_APPROVALAR_MSG = "承認マスタにデータが存在するため、{0}できません。{1}=[{2}]";

            // 関連チェック（承認情報）
            public static readonly string RELERR_APPROVE_MSG = "承認中データが存在するため、{0}できません。{1}=[{2}]";

            // 関連チェック（料理マスタ）
            public static readonly string RELERR_CUISINE_MSG = "料理マスタにデータが存在するため、{0}できません。{1}=[{2}]";

            // 関連チェック（半製品マスタ）
            public static readonly string RELERR_SEMIFINISH_MSG = "半製品マスタにデータが存在するため、{0}できません。{1}=[{2}]";

            // 関連チェック（ユーザーマスタ）
            public static readonly string RELERR_USERMST_MSG = "ユーザーマスタにデータが存在するため、{0}できません。{1}=[{2}]";

            // 登録データなし（大分類マスタ）
            public static readonly string NODATA_CATEGORY = "大分類データが存在しません。大分類マスタから登録してください。";

            // 登録データなし（中分類マスタ）
            public static readonly string NODATA_LOCATION = "中分類データが存在しません。中分類マスタから登録してください。";

            // 登録データなし（作業者マスタ）
            public static readonly string NODATA_WORKER = "作業者データが存在しません。作業者マスタから登録してください。";

            // 登録データ重複エラー
            public static readonly string ERR_DUPLICATION = "{0}が重複してます。";

            // 承認データ未選択エラー
            public static readonly string NO_DATA_APPROVAL_DATA = "承認データはありません。";

            // 承認データなし
            public static readonly string NO_CHOISE_APPROVAL_DATA = "処理データを選択してください。";

            // 大分類承認依頼、データ更新ありエラー
            public static readonly string MAJORREQUEST_DATACHANGE = "大分類承認でステータスが更新されました。\n更新が必要な場合は大分類承認で差戻を行ってください。";

            // 施設承認依頼、データ更新ありエラー
            public static readonly string FACILITYREQUEST_DATACHANGE = "施設承認でステータスが更新されました。\n更新が必要な場合は施設承認で差戻を行ってください。";

            // 承認依頼、承認待ちデータありエラー
            public static readonly string SNNREQUEST_PENDING_ERR = "承認待ちデータが存在します。すべて承認を行い、承認依頼を実行してください。";

            // 承認依頼実行
            public static readonly string REGIST_REQUEST_APPROVAL = "承認依頼が正常に実行されました。";

            // 初期データ登録失敗
            public static readonly string FIRST_LOGIN_INSERT_FAILER = "フォーマットデータの登録に失敗しました。サービスセンターまでご連絡ください。";

        }

        /// <summary>
        /// 承認分類別コード
        /// </summary>
        public static class ApprovalCategory
        {
            // 施設承認
            public static readonly string FACILITY = "3";
            public static readonly string MODE_FACILITY = "facility";
            public static readonly string FACILITYDATA_CATEGORY = "00";
            public static readonly string NODE_CLASS_FACILITY = "施設承認";

            // 大分類承認
            public static readonly string MAJOR = "2";
            public static readonly string MODE_MAJOR = "major";
            public static readonly string MAJORDATA_LOCATION = "00";
            public static readonly string NODE_CLASS_MAJOR = "大分類承認";

            // 中分類承認
            public static readonly string MIDDLE = "1";
            public static readonly string MODE_MIDDLE = "middle";
            public static readonly string NODE_CLASS_MIDDLE = "中分類承認";
        }

        /// <summary>
        /// 捺印回数
        /// </summary>
        public static class StampField
        {
            // 中分類・大分類・施設承認
            public static readonly short STAMP_RESPOSNSIBLE = 3;
            // 大分類・施設承認
            public static readonly short STAMP_GROUPLEADER = 2;
            // 施設承認
            public static readonly short STAMP_FACILITYMANAGER = 1;
        }

        /// <summary>
        /// 作業者マスタ初期データ
        /// </summary>
        public static class WorkerInitData
        {
            // 店舗ID
            public static readonly string SHOPID = GetAppSet.GetAppSetValue("WorkerM", "InitShopId");
            // 作業者ID
            public static readonly string WORKERID = GetAppSet.GetAppSetValue("WorkerM", "InitWorkerId");
        }

        /// <summary>
        /// URLパラメータ項目
        /// </summary>
        public static class URLParameter
        {
            // URL
            public static readonly string URL = GetAppSet.GetAppSetValue("URLParam", "URL");
            // 店舗ID
            public static readonly string SHOPID = GetAppSet.GetAppSetValue("URLParam", "ShopId");
            // 処理区分
            public static readonly string MODE = GetAppSet.GetAppSetValue("URLParam", "Mode");
            // 大分類ID
            public static readonly string CATEGORYID = GetAppSet.GetAppSetValue("URLParam", "CategoryId");
            // 中分類ID
            public static readonly string LOCATIONID = GetAppSet.GetAppSetValue("URLParam", "LocationId");
            // 帳票ID
            public static readonly string REPORTID = GetAppSet.GetAppSetValue("URLParam", "ReportId");
            // 周期ID
            public static readonly string PERIODID = GetAppSet.GetAppSetValue("URLParam", "PeriodId");
            // 周期開始日
            public static readonly string PERIODSTART = GetAppSet.GetAppSetValue("URLParam", "PeriodStart");
        }

        /// <summary>
        ///  URLパラメータ処理区分
        /// </summary>
        public static class URLShoriKBN
        {
            // データ履歴
            public static readonly string DATAHISTORY = "1";
            // 中分類承認
            public static readonly string MIDDLE_APPROVAL = "2";
            // 大分類承認
            public static readonly string MAJOR_APPROVAL = "3";
            // 施設承認
            public static readonly string FACILITY_APPROVAL = "4";
        }

        /// <summary>
        /// 論理削除フラグ
        /// </summary>
        public static class DeleteFlg
        {
            // 有効データ
            public static readonly string NODELETE = "0";
            // 論理削除
            public static readonly string DELETE = "1";
        }

        /// <summary>
        /// 帳票マージ単位
        /// </summary>
        public static class ReportMergeUnit
        {
            // 中分類単位
            public static readonly string UNIT_MIDDLE = "0";
            // 大分類単位
            public static readonly string UNIT_MAJOR = "1";
            // 担当者単位（中分類）
            public static readonly string UNIT_WORKER = "3";
        }

        /// <summary>
        /// 周期
        /// </summary>
        public static class PERIOD
        {
            public static readonly string ONEDAY = "1";
            public static readonly string ONEDAY_W = "1日";

            public static readonly string ONEWEEK = "2";
            public static readonly string ONEWEEK_W = "1週間";

            public static readonly string ONEMONTH = "3";
            public static readonly string ONEMONTH_W = "1ヶ月";


            public static readonly string THREEMONTH = "4";
            public static readonly string THREEMONTH_W = "3ヶ月";

            public static readonly string SIXMONTH = "5";
            public static readonly string SIXMONTH_W = "6ヶ月";
        }

        /// <summary>
        /// 承認分類
        /// </summary>
        public static class APPROVALLEVEL
        {
            // 承認分類（中分類）
            public static readonly string MIDDLE = "1";
            // 承認分類（大分類）
            public static readonly string MAJORE = "2";
            // 承認分類（施設）
            public static readonly string FACILITY = "3";
        }

        /// <summary>
        /// 初回ログイン時、管理者不在モード
        /// </summary>
        public static class USERNAME
        {
            // 初回ログイン 更新ユーザーID
            public static readonly string FIRSTLOGIN_UPDID = "FIRST";
            // 管理者不在モード 更新ユーザーID
            public static readonly string NOMANAGER_UPDID = "NOMNG";
        }

        /// <summary>
        /// 帳票テンプレート共通データ
        /// </summary>
        public static class ReportTemplateCommonData
        {
            // 店舗ID
            public static readonly string SHOPID = "00000";
        }

        /// <summary>
        /// 初期ログイン時データ
        /// </summary>
        public static class FIRSTLOGIN 
        {
            // 作業者ID
            public static readonly string WORKER_ID = "99999";
            // 作業者名
            public static readonly string WORKER_NAME = "初期作業者";
        }

        /// <summary>
        /// 回答種類マスタ_回答種類区分
        /// </summary>
        public static class AnsweTypeKbn
        {
            // ボタン
            public static readonly string BUTTON = "01";
            // テキスト
            public static readonly string TEXT = "02";
            // テキストエリア
            public static readonly string TEXTAREA = "03";
            // 温度計
            public static readonly string ONDOKEI = "04";
            // 体温計
            public static readonly string TAIONKEI = "05";
            // 日付
            public static readonly string DATE = "06";
            // 時間
            public static readonly string TIME = "07";
            // 数値（整数）
            public static readonly string NUMERIC = "08";
            // 数値（小数点）
            public static readonly string DECIMAL = "09";
            // カメラ撮影
            public static readonly string CAMERA = "10";
            // 動画撮影
            public static readonly string VIDEO = "11";
            // 仕入れ先マスタ
            public static readonly string SHIIRESAKI_MST = "12";
            // 機器マスタ
            public static readonly string KIKI_MST = "13";
            // 食材マスタ
            public static readonly string SHOKUZAI_MST = "14";
            // 料理マスタ
            public static readonly string RYORI_MST = "15";
            // 半製品マスタ
            public static readonly string HANSEIHIN_MST = "16";
            // 作業者マスタ
            public static readonly string SAGYOSHA_MST = "17";
            // 4分岐
            public static readonly string BRANCH_4 = "18";
            // ユーザーマスタ
            public static readonly string USER_MST = "19";
        }

        /// <summary>
        /// 正常データ基準
        /// </summary>
        public static class NormalDataReference
        {
            // 指定なし
            public static readonly string UNSPECIFIED = "0";
            // 次の値の間
            public static readonly string BETWEEN = "1";
            // 次の値の間以外
            public static readonly string NO_BETWEEN = "2";
            // 次の値に等しい
            public static readonly string ISEQUALTO = "3";
            // 次の値に等しくない
            public static readonly string NOTEQUALTO = "4";
            // 次の値より大きい
            public static readonly string GREATER_THAN = "5";
            // 次の値より小さい
            public static readonly string LESS_THAN = "6";
            // 次の値以上
            public static readonly string GREATER_THAN_EQUAL = "7";
            // 次の値以下
            public static readonly string LESS_THAN_EQUAL = "8";
            // 上限下限温度の間
            public static readonly string UPPER_LOWER_TEMPERATURE = "9";
        }

        /// <summary>
        /// 汎用マスタキー値
        /// </summary>
        public static class EnvironmentKey
        {
            // ホスト名
            public static readonly string KEY_HOSTNAME = "HOSTNAME";
            public static readonly string KEY_APPVERSION = "APPVERSION";
        }

        /// <summary>
        /// オプションリスト表示用基準月
        /// </summary>
        public static readonly Dictionary<string, string> OPTIONLIST_BASEMONTH = new Dictionary<string, string>()
        {
            // 基準月（値）, 基準月（表示値）
            {"1", "1"},
            {"2", "2"},
            {"3", "3"},
            {"4", "4"},
            {"5", "5"},
            {"6", "6"},
            {"7", "7"},
            {"8", "8"},
            {"9", "9"},
            {"10", "10"},
            {"11", "11"},
            {"12", "12"},
        };

        /// <summary>
        /// オプションリスト表示用基準日（曜日）周期：1週間
        /// </summary>
        public static readonly Dictionary<string, string> OPTIONLIST_REFERENCEDATE_ONEWEEK = new Dictionary<string, string>()
        {
            {"Sun", "日曜日"},
            {"Mon", "月曜日"},
            {"Tue", "火曜日"},
            {"Wed", "水曜日"},
            {"Thu", "木曜日"},
            {"Fri", "金曜日"},
            {"Sat", "土曜日"},
        };

        /// <summary>
        /// オプションリスト基準日（曜日）周期：1週間
        /// </summary>
        public static readonly Dictionary<string, int> OPTIONLIST_REFERENCEDATE_DAYOFWEEK = new Dictionary<string, int>()
        {
            {"Sun", 0},
            {"Mon", 1},
            {"Tue", 2},
            {"Wed", 3},
            {"Thu", 4},
            {"Fri", 5},
            {"Sat", 6},
        };

        /// <summary>
        /// オプションリスト表示用基準日（曜日）周期：1ヶ月
        /// </summary>
        public static readonly Dictionary<string, string> OPTIONLIST_REFERENCEDATE_ONEMONTH = new Dictionary<string, string>()
        {
            // 基準日（値）, 基準日（表示値）
            {"0", "月末"},
            {"1", "1"},
            {"2", "2"},
            {"3", "3"},
            {"4", "4"},
            {"5", "5"},
            {"6", "6"},
            {"7", "7"},
            {"8", "8"},
            {"9", "9"},
            {"10", "10"},
            {"11", "11"},
            {"12", "12"},
            {"13", "13"},
            {"14", "14"},
            {"15", "15"},
            {"16", "16"},
            {"17", "17"},
            {"18", "18"},
            {"19", "19"},
            {"20", "20"},
            {"21", "21"},
            {"22", "22"},
            {"23", "23"},
            {"24", "24"},
            {"25", "25"},
            {"26", "26"},
            {"27", "27"},
            {"28", "28"},
            {"29", "29"},
            {"30", "30"},
            {"31", "31"},
        };

        /// <summary>
        /// オプションリスト表示用基準日（曜日）周期：数ヶ月
        /// </summary>
        public static readonly Dictionary<string, string> OPTIONLIST_REFERENCEDATE_SEVERALMONTH = new Dictionary<string, string>()
        {
            // 基準日（値）, 基準日（表示値）
            {"1", "1"},
            {"2", "2"},
            {"3", "3"},
            {"4", "4"},
            {"5", "5"},
            {"6", "6"},
            {"7", "7"},
            {"8", "8"},
            {"9", "9"},
            {"10", "10"},
            {"11", "11"},
            {"12", "12"},
            {"13", "13"},
            {"14", "14"},
            {"15", "15"},
            {"16", "16"},
            {"17", "17"},
            {"18", "18"},
            {"19", "19"},
            {"20", "20"},
            {"21", "21"},
            {"22", "22"},
            {"23", "23"},
            {"24", "24"},
            {"25", "25"},
            {"26", "26"},
            {"27", "27"},
            {"28", "28"},
            {"29", "29"},
            {"30", "30"},
            {"31", "31"},
        };

        /// <summary>
        /// オプションリスト表示用捺印数
        /// </summary>
        public static readonly Dictionary<string, string> OPTIONLIST_STAMPFIELD = new Dictionary<string, string>()
        {
            // 捺印数（値）, 捺印数（表示値）
            {"1", "1"},
            {"2", "2"},
            {"3", "3"},
        };

        /// <summary>
        /// CSV管理
        /// </summary>
        public static class CsvManage
        {
            public static readonly List<CsvDef> DEFS = new List<CsvDef>()
            {
                new CsvDef() {
                    ManagementId = "01",
                    FileName = GetAppSet.GetAppSetValue("CSVFileName", "management_01"),
                    Columns = new List<CsvColumn>()
                    {
                        new CsvColumn() { Pos = 1, Name = "MANAGENO", Title = "仕入先番号", },
                        new CsvColumn() { Pos = 2, Name = "MANAGENAME", Title = "仕入先名", },
                    }
                },
                new CsvDef() {
                    ManagementId = "02",
                    FileName = GetAppSet.GetAppSetValue("CSVFileName", "management_02"),
                    Columns = new List<CsvColumn>()
                    {
                        new CsvColumn() { Pos = 1, Name = "MANAGENO", Title = "食材番号", },
                        new CsvColumn() { Pos = 2, Name = "MANAGENAME", Title = "食材名", },
                        new CsvColumn() { Pos = 3, Name = "UNIT", Title = "単位", },
                        new CsvColumn() { Pos = 4, Name = "UPPERLIMIT", Title = "上限温度", },
                        new CsvColumn() { Pos = 5, Name = "LOWERLIMIT", Title = "下限温度", },
                    }
                },
                new CsvDef() {
                    ManagementId = "03",
                    FileName = GetAppSet.GetAppSetValue("CSVFileName", "management_03"),
                    Columns = new List<CsvColumn>()
                    {
                        new CsvColumn() { Pos = 1, Name = "MANAGENO", Title = "料理番号", },
                        new CsvColumn() { Pos = 2, Name = "MANAGENAME", Title = "料理名", },
                        new CsvColumn() { Pos = 3, Name = "UNIT", Title = "単位", },
                        new CsvColumn() { Pos = 4, Name = "UPPERLIMIT", Title = "上限温度", },
                        new CsvColumn() { Pos = 5, Name = "LOWERLIMIT", Title = "下限温度", },
                        new CsvColumn() { Pos = 6, Name = "LOCATIONID", Title = "中分類ID", },
                    }
                },
                new CsvDef() {
                    ManagementId = "04",
                    FileName = GetAppSet.GetAppSetValue("CSVFileName", "management_04"),
                    Columns = new List<CsvColumn>()
                    {
                        new CsvColumn() { Pos = 1, Name = "MANAGENO", Title = "半製品番号", },
                        new CsvColumn() { Pos = 2, Name = "MANAGENAME", Title = "半製品名", },
                        new CsvColumn() { Pos = 3, Name = "UNIT", Title = "単位", },
                        new CsvColumn() { Pos = 4, Name = "UPPERLIMIT", Title = "上限温度", },
                        new CsvColumn() { Pos = 5, Name = "LOWERLIMIT", Title = "下限温度", },
                        new CsvColumn() { Pos = 6, Name = "LOCATIONID", Title = "中分類ID", },
                    }
                },
                new CsvDef() {
                    ManagementId = "05",
                    FileName = GetAppSet.GetAppSetValue("CSVFileName", "management_05"),
                    Columns = new List<CsvColumn>()
                    {
                        new CsvColumn() { Pos = 1, Name = "MANAGENO", Title = "ユーザマスタ番号", },
                        new CsvColumn() { Pos = 2, Name = "MANAGENAME", Title = "ユーザマスタ名", },
                        new CsvColumn() { Pos = 3, Name = "UNIT", Title = "単位", },
                        new CsvColumn() { Pos = 4, Name = "UPPERLIMIT", Title = "上限温度", },
                        new CsvColumn() { Pos = 5, Name = "LOWERLIMIT", Title = "下限温度", },
                        new CsvColumn() { Pos = 6, Name = "LOCATIONID", Title = "中分類ID", },
                    }
                },
            };
            public static readonly List<CsvDef> DEFS_WORKER = new List<CsvDef>()
            {
                new CsvDef() {
                    ManagementId = "",
                    FileName = "",
                    Columns = new List<CsvColumn>()
                    {
                        new CsvColumn() { Pos = 1, Name = "WORKERNAME", Title = "作業者名", },
                    }
                },
            };
        }
        public class CsvColumn
        {
            public int Pos { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
        }
        public class CsvDef
        {
            public string ManagementId { get; set; }
            public string FileName { get; set; }
            public List<CsvColumn> Columns { get; set; }
        }
    }

}