namespace HACCPExtender.Controllers.API
{
    public class APIConstants
    {
        // WEBAPI 登録・更新ユーザー
        public static readonly string API_USER = "WBAPI";

        public static readonly int CODE_OK = 200;

        public static readonly int CODE_NG = 400;

        public static readonly int CODE_LICENSE_OVER = 401;

        public static readonly int CODE_APIKEY_EXPIRED = 401;

        public static readonly string STATUS_OK = "OK";

        public static readonly string STATUS_NG = "NG";

        public static readonly string APPROVAL_SKIP_COMMENT = "承認スキップ";
    }
}