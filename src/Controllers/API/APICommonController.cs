using HACCPExtender.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HACCPExtender.Controllers.API
{
    public class APICommonController : Controller
    {
        /// <summary>
        /// APIキー有効判定
        /// </summary>
        /// <param name="shopId">店舗ID</param>
        /// <param name="APIKey">連携APIキー</param>
        /// <returns>APIキー有効判定結果</returns>
        public bool ChkAPIKey(MasterContext context, string shopId, string APIKey)
        {
            bool chk = false;

            // APIキーがない場合はエラー
            if (string.IsNullOrEmpty(APIKey))
            {
                return chk;
            }

            // APIKeyを端末番号とKeyに分ける
            string[] key = APIKey.Split('-');
            // 端末番号とAPIキーに分割できない場合はエラー
            if (key.Length != 2)
            {
                return chk;
            }
            int no = int.Parse(key[0]);
            string keyVal = key[1];

            // 端末情報を取得
            var mobileDt = from mob in context.MobileTs
                           where mob.SHOPID == shopId
                           && mob.TERMINALNO == (short)no
                           && mob.APIKEY == keyVal
                           select mob;

            // 該当データが1件以外の場合はエラー
            if (mobileDt == null || mobileDt.Count() != 1)
            {
                return chk;
            }

            string deadTime = mobileDt.FirstOrDefault().EXPIRATION;
            // 期限が取得できない場合はエラー
            if (string.IsNullOrEmpty(deadTime))
            {
                return chk;
            }

            long dateTime = long.Parse(DateTime.Now.ToString("yyyyMMddHHmm"));

            // 現在時刻が期限内ではない場合はエラー
            if (long.Parse(deadTime) < dateTime)
            {
                return chk;
            }

            return true;
        }

    }
}