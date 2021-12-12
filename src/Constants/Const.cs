namespace HACCPExtender.Constants
{
    public class Const
    {

        public static readonly string RecordTime = "記録時間";

        public static readonly string WorkerName = "記録者";

        public static readonly string RecognizerName = "承認者";

        // 周期 1：1日
        public static readonly string PeriodDay = "1";

        // 周期 1：2：1週間
        public static readonly string PeriodWeek = "2";

        // 周期 1：3：1ヶ月
        public static readonly string PeriodMonth = "3";

        // 周期 1：4：3ヶ月
        public static readonly string PeriodThreeMonth = "4";

        // 周期 1：5：6ヶ月
        public static readonly string PeriodSixMonth = "5";

        // Excel Sheet Name
        public static readonly string SheetName = "sheet1";

        /// <summary>
        /// 管理者ログイン編集モード
        /// </summary>
        /// 
        public static class ManagerLoginMode
        {
            // 管理者未ログイン（編集不可）
            public static readonly string LOGIN_NONE = "0";
            // 管理者ログイン済（編集可能）
            public static readonly string LOGIN_ALREADY = "1";
            // 管理者不在（編集可）
            public static readonly string NO_MANAGER = "2";
            // 初回ログイン（編集可）
            public static readonly string FIRST_LOGIN = "3";
        }

        /// <summary>
        /// 画面名称
        /// </summary>
        public enum GamenName
        {   // 衛生管理画面

            // 温度管理マスタ
            Temperature,
            // 機器マスタ
            MachineM,
            // 仕入先マスタ
            Supplier,
            // 食材マスタ
            Foodstuff,
            // 料理マスタ
            Cuisine,
            // 半製品マスタ
            Semifinishedproduct,
            // ユーザーマスタ
            Usermst,
            // マスタ設定
            Mstsettei
        }

        /// <summary>
        /// 管理対象区分
        /// </summary>
        public static class ManagementID
        {
            // 仕入先
            public static readonly string SHIIRESAKI = "01";
            // 食材
            public static readonly string SHOKUZAI = "02";
            // 料理
            public static readonly string RYORI = "03";
            // 半製品
            public static readonly string HANSEIHIN = "04";
            // ユーザー
            public static readonly string USERMST = "05";
        }

        /// <summary>
        /// 承認ステータス
        /// </summary>
        public static class ApprovalStatus
        {
            // 承認待ち
            public static readonly string PENDING = "0";
            // 承認済
            public static readonly string APPROVAL = "1";
            // 差戻
            public static readonly string REMAND = "2";
        }

        /// <summary>
        /// チェック項目の種類
        /// </summary>
        public enum CheckItemType
        {   
            // 中分類名称
            MiddleName,
            // 記録時間
            RecordTime,
            // 記録者
            WorkerName,
            // 設問
            Question
        }

        /// <summary>
        /// 色のRGB
        /// </summary>
        public static class BlueRgb
        {
            // 承認待ち
            public static readonly int R = 221;
            // 承認済み
            public static readonly int G = 235;
            // 差戻し
            public static readonly int B = 247;
        }
    }
}