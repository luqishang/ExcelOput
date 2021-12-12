using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

namespace HACCPExtender.Business
{
    public class GetAppSet
    {

        public static string GetAppSetValue(string tag, string key)
        {

            //xmlファイルを指定する
            var path = HostingEnvironment.MapPath("~/appset.config");
            XElement xml = XElement.Load(path);
            //メンバー情報のタグ内の情報を取得する
            IEnumerable<XElement> infos = from item in xml.Elements(tag) select item;
            //メンバー情報分ループして、コンソールに表示
            if (infos.Count() == 0)
            {
                return null;
            }
            if (infos.First().Element(key) != null)
            {
                return infos.First().Element(key).Value;
            } else
            {
                return null;
            }
        }
    }
}