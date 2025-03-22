using CoreArchV2.Core.Entity.Licence.Entity;
using Microsoft.EntityFrameworkCore;

namespace CoreArchV2.Data
{
    public class LicenceDbContext : DbContext
    {
        public LicenceDbContext(DbContextOptions<LicenceDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IpBlackList>().ToTable("IpBlackList", "dbo");
            modelBuilder.Entity<LicenceKey>().ToTable("LicenceKey", "dbo");
            modelBuilder.Entity<RequestLog>().ToTable("RequestLog", "dbo");
            modelBuilder.Entity<WebLog>().ToTable("WebLog", "dbo");
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<IpBlackList> IpBlackList { get; set; }
        public DbSet<LicenceKey> LicenceKey { get; set; }
        public DbSet<RequestLog> RequestLog { get; set; }
        public DbSet<WebLog> WebLog { get; set; }
    }
}
