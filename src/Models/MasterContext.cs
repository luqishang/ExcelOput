using HACCPExtender.Models.API;
using System;
using System.Data.Entity;
using System.Linq;
using static HACCPExtender.Models.Common.ModelConstants;

namespace HACCPExtender.Models
{
    /// <summary>
    /// DBマッピングクラス
    /// </summary>
    public class MasterContext : DbContext
    {
        // 業種マスタ
        public DbSet<IndustryM> IndustryMs { get; set; }

        // 店舗マスタ
        public DbSet<ShopM> ShopMs { get; set; }
        
        // 大分類マスタ
        public DbSet<CategoryM> CategoryMs { get; set; }
        
        // 中分類マスタ
        public DbSet<LocationM> LocationMs { get; set; }
        
        // 作業者マスタ
        public DbSet<WorkerM> WorkerMs { get; set; }
        
        // 設問マスタ
        public DbSet<QuestionM> QuestionMs { get; set; }

        // 帳票マスタ
        public DbSet<ReportM> ReportMs { get; set; }

        // 帳票テンプレートマスタ
        public DbSet<ReportTemplateM> ReportTemplateMs { get; set; }

        // 回答種類マスタ
        public DbSet<AnswerTypeM> AnswerTypeMs { get; set; }
        public DbSet<Shop_AnswerTypeM> Shop_AnswerTypeMs { get; set; }

        // 承認経路マスタ
        public DbSet<ApprovalRouteM> ApprovalRouteMs { get; set; }

        // 機器マスタ
        public DbSet<MachineM> MachineMs { get; set; }
        
        // 管理対象マスタ
        public DbSet<ManagementM> ManagementMs { get; set; }

        // CSV履歴
        public DbSet<CsvHistoryT> CsvHistoryTs { get; set; }

        // 手引書マスタ
        public DbSet<ManualM> ManualMs { get; set; }

        // ライセンス情報
        public DbSet<LicenseM> LicenseMs { get; set; }

        // モバイル情報
        public DbSet<MobileT> MobileTs { get; set; }

        // 中分類承認情報
        public DbSet<MiddleApprovalT> MiddleApprovalTs { get; set; }

        // 大分類承認情報
        public DbSet<MajorApprovalT> MajorApprovalTs { get; set; }

        // 施設承認情報
        public DbSet<FacilityApprovalT> FacilityApprovalTs { get; set; }

        // 承認完了情報
        public DbSet<ApprovalCompleteT> ApprovalCompleteTs { get; set; }

        // お知らせ情報
        public DbSet<NotificationT> NotificationTs { get; set; }

        // 温度衛生管理情報
        public DbSet<TemperatureControlT> TemperatureControlTs { get; set; }

        // データ記録連携情報
        public DbSet<DataCooperation> DataCooperations { get; set; }

        // 汎用マスタ
        public DbSet<GeneralPurposeM> GeneralPurposeMs { get; set; }

        /// <summary>
        /// DB更新処理
        /// </summary>
        /// <returns>処理結果</returns>
        public override int SaveChanges()
        {
            var now = DateTime.Now;
            SetUpdateDateTime(now);
            return base.SaveChanges();
        }

        /// <summary>
        /// 更新日時の更新値を設定
        /// </summary>
        /// <param name="now">更新日時</param>
        private void SetUpdateDateTime(DateTime now)
        {
            var entities = this.ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified) && e.CurrentValues.PropertyNames.Contains("UPDDATE"))
                .Select(e => e.Entity);

            foreach (dynamic entity in entities)
            {
                entity.UPDDATE = now;
            }
        }

        /// <summary>
        /// 初期設定追加
        /// </summary>
        /// <param name="modelBuilder">DbModelBuilderオブジェクト</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // スキーマ名の設定
            modelBuilder.HasDefaultSchema(SchemaName.EXTENDER_MAIN);

            // 回答種類マスタ
            modelBuilder.Entity<AnswerTypeM>().ToTable("ANSWERTYPE_M").HasKey(m => new { m.ANSWERTYPEID });
            modelBuilder.Entity<AnswerTypeM>().ToTable("ANSWERTYPE_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 店舗別回答種類マスタ
            modelBuilder.Entity<Shop_AnswerTypeM>().ToTable("SHOP_ANSWERTYPE_M").HasKey(m => new { m.SHOPID, m.ANSWERTYPEID });
            modelBuilder.Entity<Shop_AnswerTypeM>().ToTable("SHOP_ANSWERTYPE_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 承認完了情報
            modelBuilder.Entity<ApprovalCompleteT>().ToTable("APPROVALCOMPLETE_T").HasKey(m => new { m.SHOPID, m.PERIOD, m.PERIODSTART, m.FACILITYAPPGROUPNO });
            modelBuilder.Entity<ApprovalCompleteT>().ToTable("APPROVALCOMPLETE_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 承認経路マスタ
            modelBuilder.Entity<ApprovalRouteM>().ToTable("APPROVALROUTE_M").HasKey(m => new { m.SHOPID, m.CATEGORYID, m.LOCATIONID, m.APPROVALNODEID });
            modelBuilder.Entity<ApprovalRouteM>().ToTable("APPROVALROUTE_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 部門マスタ
            modelBuilder.Entity<CategoryM>().ToTable("CATEGORY_M").HasKey(m => new { m.SHOPID, m.CATEGORYID });
            modelBuilder.Entity<CategoryM>().ToTable("CATEGORY_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // CSV履歴情報
            modelBuilder.Entity<CsvHistoryT>().ToTable("CSVHISTORY_T").HasKey(m => new { m.SHOPID, m.MANAGEMENTID, m.UPLOADDATE });
            modelBuilder.Entity<CsvHistoryT>().ToTable("CSVHISTORY_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 施設承認情報
            modelBuilder.Entity<FacilityApprovalT>().ToTable("FACILITYAPPROVAL_T").HasKey(m => new { m.SHOPID, m.CATEGORYID, m.PERIOD, m.PERIODSTART, m.MAJORGROUPNO });
            modelBuilder.Entity<FacilityApprovalT>().ToTable("FACILITYAPPROVAL_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // ライセンスマスタ
            modelBuilder.Entity<LicenseM>().ToTable("LICENSE_M").HasKey(m => new { m.SHOPID });
            modelBuilder.Entity<LicenseM>().ToTable("LICENSE_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 場所マスタ
            modelBuilder.Entity<LocationM>().ToTable("LOCATION_M").HasKey(m => new { m.SHOPID, m.LOCATIONID });
            modelBuilder.Entity<LocationM>().ToTable("LOCATION_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 機器マスタ
            modelBuilder.Entity<MachineM>().ToTable("MACHINE_M").HasKey(m => new { m.SHOPID, m.LOCATIONID, m.MACHINEID });
            modelBuilder.Entity<MachineM>().ToTable("MACHINE_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 大分類承認情報
            modelBuilder.Entity<MajorApprovalT>().ToTable("MAJORAPPROVAL_T").HasKey(m => new { m.SHOPID, m.CATEGORYID, m.LOCATIONID, m.REPORTID, m.PERIOD, m.PERIODSTART, m.MIDDLEGROUPNO });
            modelBuilder.Entity<MajorApprovalT>().ToTable("MAJORAPPROVAL_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 管理対象マスタ
            modelBuilder.Entity<ManagementM>().ToTable("MANAGEMENT_M").HasKey(m => new { m.SHOPID, m.MANAGEMENTID, m.MANAGEID });
            modelBuilder.Entity<ManagementM>().ToTable("MANAGEMENT_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 手引書マスタ
            modelBuilder.Entity<ManualM>().ToTable("MANUAL_M").HasKey(m => new { m.SHOPID, m.MANUALID });
            modelBuilder.Entity<ManualM>().ToTable("MANUAL_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 中分類承認情報
            modelBuilder.Entity<MiddleApprovalT>().ToTable("MIDDLEAPPROVAL_T").HasKey(m => new { m.SHOPID, m.CATEGORYID, m.LOCATIONID, m.REPORTID, m.APPROVALID });
            modelBuilder.Entity<MiddleApprovalT>().ToTable("MIDDLEAPPROVAL_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // モバイル端末情報
            modelBuilder.Entity<MobileT>().ToTable("MOBILE_T").HasKey(m => new { m.SHOPID, m.TERMINALNO });
            modelBuilder.Entity<MobileT>().ToTable("MOBILE_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // お知らせ情報
            modelBuilder.Entity<NotificationT>().ToTable("NOTIFICATION_T").HasKey(m => new { m.NOTICEID });
            modelBuilder.Entity<NotificationT>().ToTable("NOTIFICATION_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 設問マスタ
            modelBuilder.Entity<QuestionM>().ToTable("QUESTION_M").HasKey(m => new { m.SHOPID, m.REPORTID, m.QUESTIONID });
            modelBuilder.Entity<QuestionM>().ToTable("QUESTION_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 帳票マスタ
            modelBuilder.Entity<ReportM>().ToTable("REPORT_M").HasKey(m => new { m.SHOPID, m.CATEGORYID, m.LOCATIONID, m.REPORTTEMPLATEID });
            modelBuilder.Entity<ReportM>().ToTable("REPORT_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 帳票テンプレートマスタ
            modelBuilder.Entity<ReportTemplateM>().ToTable("REPORTTEMPLATE_M").HasKey(m => new { m.SHOPID, m.TEMPLATEID });
            modelBuilder.Entity<ReportTemplateM>().ToTable("REPORTTEMPLATE_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 店舗マスタ
            modelBuilder.Entity<ShopM>().ToTable("SHOP_M").HasKey(m => new { m.SHOPID });
            modelBuilder.Entity<ShopM>().ToTable("SHOP_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 温度衛生管理情報
            modelBuilder.Entity<TemperatureControlT>().ToTable("TEMPERATURECONTROL_T").HasKey(m => new { m.SHOPID, m.CATEGORYID, m.LOCATIONID, m.REPORTID, m.APPROVALID });
            modelBuilder.Entity<TemperatureControlT>().ToTable("TEMPERATURECONTROL_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 作業者マスタ
            modelBuilder.Entity<WorkerM>().ToTable("WORKER_M").HasKey(m => new { m.SHOPID, m.WORKERID });
            modelBuilder.Entity<WorkerM>().ToTable("WORKER_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // データ記録連携情報
            modelBuilder.Entity<DataCooperation>().ToTable("DATACOOPERATION_T").HasKey(m => new { m.ShopNO, m.APPROVALID });
            modelBuilder.Entity<DataCooperation>().ToTable("DATACOOPERATION_T").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 業種マスタ
            modelBuilder.Entity<IndustryM>().ToTable("INDUSTRY_M").HasKey(m => new { m.INDUSTRYID });
            modelBuilder.Entity<IndustryM>().ToTable("INDUSTRY_M").Property(p => p.UPDDATE).IsConcurrencyToken();
            // 汎用マスタ
            modelBuilder.Entity<GeneralPurposeM>().ToTable("GENERALPURPOSE_M").HasKey(m => new { m.KEY });
        }
    }
}
