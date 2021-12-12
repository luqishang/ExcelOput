using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Text.RegularExpressions;

namespace HACCPExtender.Controllers.Common
{
    /// <summary>
    /// 入力チェック
    /// author : PTJ.Cheng
    /// Create Date : 2020/10/02
    /// </summary>
    public class CheckFunction
    {

        /// <summary>
		/// 数字化どうかチェックする
		/// </summary>
		/// <param name="item"></param>
		/// <remarks></remarks>
        public static bool CheckInteger(string item)
        {
            bool ret = Regex.IsMatch(item, "^[-]?[0-9]+$");
            return ret;
        }

        /// <summary>
		/// 数字の大小比較
		/// </summary>
		/// <param name="item1"></param>
        /// <param name="item2"></param>
		/// <remarks></remarks>
        public static bool CheckBigness(string item1, string item2)
        {
            try
            {
                int num1 = int.Parse(item1);
                int num2 = int.Parse(item2);
                if (num1 > num2)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Default.WriteError(ex.Message, ex);
                return false;
            }


        }

        /// <summary>
        /// 上限温度、下限温度がSmallIntの範囲を超えていないかチェック
        /// </summary>
        /// <param name="limit">温度</param>
        /// <returns>SmallIntの範囲を超えていた場合false,超えていない場合true</returns>
        public static bool CheckOverflow(string limit)
        {
            int limitInt = int.Parse(limit);

            //温度がSMALLINTの範囲を超えた際に、false
            if (limitInt > 32767 || limitInt < -32767)
            {
                return false;
            }

            return true;
        }
    }
}