using System;

namespace HACCPExtender.Models.Bussiness
{
    public class BWorkerM
    {
        // 0:未更新、1:更新、2:登録、3:削除
        public int EditMode { get; set; }
        public Boolean DelFlg { get; set; }
        public string ShopId { get; set; }
        public string WorkerId { get; set; }
        public string WorkerName { get; set; }
        public Boolean ManagerKbn { get; set; }
        public Boolean CategoryKbn1 { get; set; }
        public Boolean CategoryKbn2 { get; set; }
        public Boolean CategoryKbn3 { get; set; }
        public Boolean CategoryKbn4 { get; set; }
        public Boolean CategoryKbn5 { get; set; }
        public Boolean CategoryKbn6 { get; set; }
        public Boolean CategoryKbn7 { get; set; }
        public Boolean CategoryKbn8 { get; set; }
        public Boolean CategoryKbn9 { get; set; }
        public Boolean CategoryKbn10 { get; set; }
        public string AppId { get; set; }
        public string AppPass { get; set; }
        public string MailAddressPc { get; set; }
        public string MailAddressFeature { get; set; }
        public string TransMissionTime1 { get; set; }
        public string TransMissionTime2 { get; set; }
        public Boolean TransMissionTime1Flg { get; set; }
        public Boolean TransMissionTime2Flg { get; set; }
        public Boolean NoDisplayKbn { get; set; }
        public Int16 DisplayNo { get; set; }
        public string InsUserId { get; set; }
        public string UpdUserId { get; set; }
        public string UpdDate { get; set; }
        public int No { get; set; }
    }
}