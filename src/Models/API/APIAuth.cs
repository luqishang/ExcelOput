namespace HACCPExtender.Models.API
{
    /// <summary>
    /// WenAPI連携データinput用
    /// </summary>
    public class APIAuth
    {
        public string ShopNo { get; set; }

        public string GUID { get; set; }

        public string LicenseKey { get; set; }
    }
}