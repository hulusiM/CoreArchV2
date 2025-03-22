using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.Note;
using CoreArchV2.Core.Entity.NoticeVehicle.Notice;
using CoreArchV2.Core.Entity.NoticeVehicle.NoticeUnit_;
using CoreArchV2.Core.Entity.Tender;
using CoreArchV2.Core.Entity.Track;
using CoreArchV2.Core.Entity.TripVehicle;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CoreArchV2.Data
{
    public class CoreArchDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CoreArchDbContext(DbContextOptions<CoreArchDbContext> options) : base(options) { }
        public CoreArchDbContext(DbContextOptions<CoreArchDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Common
            modelBuilder.Entity<User>().ToTable("User", "dbo");
            modelBuilder.Entity<UserRole>().ToTable("UserRole", "dbo");
            modelBuilder.Entity<Role>().ToTable("Role", "dbo");
            modelBuilder.Entity<Authorization>().ToTable("Authorization", "dbo");
            modelBuilder.Entity<RoleAuthorization>().ToTable("RoleAuthorization", "dbo");
            modelBuilder.Entity<City>().ToTable("City", "dbo");
            modelBuilder.Entity<Color>().ToTable("Color", "dbo");
            modelBuilder.Entity<Entity>().ToTable("Entity", "dbo");
            modelBuilder.Entity<LoginLog>().ToTable("LoginLog", "dbo");
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessage", "dbo");
            modelBuilder.Entity<Message>().ToTable("Message", "dbo");
            modelBuilder.Entity<Unit>().ToTable("Unit", "dbo");
            modelBuilder.Entity<LookUpList>().ToTable("LookUpList", "dbo");
            modelBuilder.Entity<ActiveUserForSignalR>().ToTable("ActiveUserForSignalR", "dbo");
            modelBuilder.Entity<Device>().ToTable("Device", "dbo");
            modelBuilder.Entity<MessageLog>().ToTable("MessageLog", "dbo");
            modelBuilder.Entity<TaskScheduler_>().ToTable("TaskScheduler_", "dbo");
            modelBuilder.Entity<Parameter>().ToTable("Parameter", "dbo");
            #endregion

            #region Logistics
            modelBuilder.Entity<Vehicle>().ToTable("Vehicle", "dbo");
            modelBuilder.Entity<VehicleBrandModel>().ToTable("VehicleBrandModel", "dbo");
            modelBuilder.Entity<Maintenance>().ToTable("Maintenance", "dbo");
            modelBuilder.Entity<MaintenanceType>().ToTable("MaintenanceType", "dbo");
            modelBuilder.Entity<MaintenanceFile>().ToTable("MaintenanceFile", "dbo");
            modelBuilder.Entity<FileUpload>().ToTable("FileUpload", "dbo");
            modelBuilder.Entity<VehicleCity>().ToTable("VehicleCity", "dbo");
            modelBuilder.Entity<FuelLog>().ToTable("FuelLog", "dbo");
            modelBuilder.Entity<CriminalLog>().ToTable("CriminalLog", "dbo");
            modelBuilder.Entity<CriminalFile>().ToTable("CriminalFile", "dbo");
            modelBuilder.Entity<Tire>().ToTable("Tire", "dbo");
            modelBuilder.Entity<TireDebit>().ToTable("TireDebit", "dbo");
            modelBuilder.Entity<TireFile>().ToTable("TireFile", "dbo");
            modelBuilder.Entity<VehicleTransferLog>().ToTable("VehicleTransferLog", "dbo");
            modelBuilder.Entity<VehicleRequest>().ToTable("VehicleRequest", "dbo");
            modelBuilder.Entity<VehicleExaminationDate>().ToTable("VehicleExaminationDate", "dbo");
            modelBuilder.Entity<VehicleDebit>().ToTable("VehicleDebit", "dbo");
            modelBuilder.Entity<VehicleRent>().ToTable("VehicleRent", "dbo");
            modelBuilder.Entity<VehicleAmount>().ToTable("VehicleAmount", "dbo");
            modelBuilder.Entity<VehicleContract>().ToTable("VehicleContract", "dbo");
            modelBuilder.Entity<VehicleFile>().ToTable("VehicleFile", "dbo");
            modelBuilder.Entity<VehicleTransferFile>().ToTable("VehicleTransferFile", "dbo");
            modelBuilder.Entity<VehicleMaterial>().ToTable("VehicleMaterial", "dbo");
            modelBuilder.Entity<VehicleCoordinate>().ToTable("VehicleCoordinate", "dbo");
            modelBuilder.Entity<VehiclePhysicalImage>().ToTable("VehiclePhysicalImage", "dbo");
            modelBuilder.Entity<VehiclePhysicalImageFile>().ToTable("VehiclePhysicalImageFile", "dbo");
            modelBuilder.Entity<VehicleOperatingReport>().ToTable("VehicleOperatingReport", "dbo");
            modelBuilder.Entity<VehicleOperatingReportParam>().ToTable("VehicleOperatingReportParam", "dbo");
            #endregion

            #region OneNote
            modelBuilder.Entity<OneNote>().ToTable("OneNote", "note");
            #endregion

            #region Tender
            modelBuilder.Entity<Tender>().ToTable("Tender", "tender");
            modelBuilder.Entity<TenderContact>().ToTable("TenderContact", "tender");
            modelBuilder.Entity<TenderDetail>().ToTable("TenderDetail", "tender");
            modelBuilder.Entity<TenderFile>().ToTable("TenderFile", "tender");
            modelBuilder.Entity<TenderDetailFile>().ToTable("TenderDetailFile", "tender");
            modelBuilder.Entity<Institution>().ToTable("Institution", "tender");
            modelBuilder.Entity<TenderHistory>().ToTable("TenderHistory", "tender");
            modelBuilder.Entity<TenderDetailPriceHistory>().ToTable("TenderDetailPriceHistory", "tender");
            #endregion

            #region Notice
            modelBuilder.Entity<Notice>().ToTable("Notice", "notice");
            modelBuilder.Entity<NoticeUnit>().ToTable("NoticeUnit", "notice");
            modelBuilder.Entity<NoticeUnitHistory>().ToTable("NoticeUnitHistory", "notice");
            modelBuilder.Entity<NoticeUnitFile>().ToTable("NoticeUnitFile", "notice");
            modelBuilder.Entity<NoticeSendUnit>().ToTable("NoticeSendUnit", "notice");
            modelBuilder.Entity<NoticePunishment>().ToTable("NoticePunishment", "notice");
            #endregion

            #region Trip

            modelBuilder.Entity<Trip>().ToTable("Trip", "trip");
            modelBuilder.Entity<TripLog>().ToTable("TripLog", "trip");


            #endregion

            #region Track
            modelBuilder.Entity<Coordinate>().ToTable("Coordinate", "track");

            #endregion

            base.OnModelCreating(modelBuilder);
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
                //property.Relational().ColumnType = "decimal(18,2)";
                property.SetColumnType("decimal(18,2)");

            modelBuilder.Entity<FuelLog>().Property(x => x.DiscountPercent).HasPrecision(18, 5);
        }

        #region DbSets

        #region Common
        public virtual DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<Authorization> Authorization { get; set; }
        public DbSet<RoleAuthorization> RoleAuthorization { get; set; }
        public DbSet<Entity> Entity { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Color> Color { get; set; }
        public DbSet<Unit> Unit { get; set; }
        public DbSet<LoginLog> LoginLog { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<ChatMessage> ChatMessage { get; set; }
        public DbSet<LookUpList> LookUpList { get; set; }
        public DbSet<ActiveUserForSignalR> ActiveUserForSignalR { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<MessageLog> MessageLog { get; set; }
        public DbSet<TaskScheduler_> TaskScheduler_ { get; set; }
        public DbSet<Parameter> Parameter { get; set; }

        #endregion

        #region Logistics
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<VehicleBrandModel> VehicleBrandModel { get; set; }
        public DbSet<Maintenance> Maintenance { get; set; }
        public DbSet<MaintenanceType> MaintenanceType { get; set; }
        public DbSet<MaintenanceFile> MaintenanceFile { get; set; }
        public DbSet<FileUpload> FileUpload { get; set; }
        public DbSet<VehicleCity> VehicleCity { get; set; }
        public DbSet<FuelLog> FuelLog { get; set; }
        public DbSet<CriminalLog> CriminalLog { get; set; }
        public DbSet<CriminalFile> CriminalFile { get; set; }
        public DbSet<Tire> Tire { get; set; }
        public DbSet<TireDebit> TireDebit { get; set; }
        public DbSet<TireFile> TireFile { get; set; }
        public DbSet<VehicleTransferLog> VehicleTransferLog { get; set; }
        public DbSet<VehicleRequest> VehicleRequest { get; set; }
        public DbSet<VehicleExaminationDate> VehicleExaminationDate { get; set; }
        public DbSet<VehicleDebit> VehicleDebit { get; set; }
        public DbSet<VehicleRent> VehicleRent { get; set; }
        public DbSet<VehicleFile> VehicleFile { get; set; }
        public DbSet<VehicleAmount> VehicleAmount { get; set; }
        public DbSet<VehicleContract> VehicleContract { get; set; }
        public DbSet<VehicleTransferFile> VehicleTransferFile { get; set; }
        public DbSet<VehicleMaterial> VehicleMaterial { get; set; }
        public DbSet<VehicleCoordinate> VehicleCoordinate { get; set; }
        public DbSet<VehiclePhysicalImageFile> VehiclePhysicalImageFile { get; set; }
        public DbSet<VehiclePhysicalImage> VehiclePhysicalImage { get; set; }
        public DbSet<VehicleOperatingReport> VehicleOperatingReport { get; set; }
        public DbSet<VehicleOperatingReportParam> VehicleOperatingReportParam { get; set; }

        #endregion

        #region OneNote
        public DbSet<OneNote> OneNote { get; set; }
        #endregion

        #region Tender
        public DbSet<Tender> Tender { get; set; }
        public DbSet<TenderFile> TenderFile { get; set; }
        public DbSet<TenderDetail> TenderDetail { get; set; }
        public DbSet<TenderDetailFile> TenderDetailFile { get; set; }
        public DbSet<TenderHistory> TenderHistory { get; set; }
        public DbSet<Institution> Institution { get; set; }
        public DbSet<TenderContact> TenderContact { get; set; }
        public DbSet<TenderDetailPriceHistory> TenderDetailPriceHistory { get; set; }
        #endregion

        #region Notice
        public virtual DbSet<Notice> Notice { get; set; }
        public virtual DbSet<NoticeUnit> NoticeUnit { get; set; }
        public virtual DbSet<NoticeUnitHistory> NoticeUnitHistory { get; set; }
        public virtual DbSet<NoticeUnitFile> NoticeUnitFile { get; set; }
        public virtual DbSet<NoticeSendUnit> NoticeSendUnit { get; set; }
        public virtual DbSet<NoticePunishment> NoticePunishment { get; set; }
        #endregion

        #region Trip
        public virtual DbSet<Trip> Trip { get; set; }
        public virtual DbSet<TripLog> TripLog { get; set; }
        #endregion

        #region Track
        public virtual DbSet<Coordinate> Coordinate { get; set; }
        #endregion

        #endregion
    }
}