using Central_Hub.Models;
using Microsoft.EntityFrameworkCore;

namespace Central_Hub.Data
{
    public class Central_HubContext : DbContext
    {
     
        public DbSet<ClientInstance> ClientInstances { get; set; }
        public DbSet<DemoRequest> DemoRequests { get; set; }
        public DbSet<CreditTransaction> CreditTransactions { get; set; }
        public DbSet<LicenseRenewal> LicenseRenewals { get; set; }
        public DbSet<SyncLog> SyncLogs { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ClientInstance
            modelBuilder.Entity<ClientInstance>()
                .HasIndex(c => c.LicenseKey)
                .IsUnique();

            modelBuilder.Entity<ClientInstance>()
                .HasIndex(c => c.CompanyEmail);

            // Configure relationships
            modelBuilder.Entity<CreditTransaction>()
                .HasOne(ct => ct.ClientInstance)
                .WithMany(c => c.CreditTransactions)
                .HasForeignKey(ct => ct.ClientInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LicenseRenewal>()
                .HasOne(lr => lr.ClientInstance)
                .WithMany(c => c.LicenseRenewals)
                .HasForeignKey(lr => lr.ClientInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SyncLog>()
                .HasOne(sl => sl.ClientInstance)
                .WithMany(c => c.SyncLogs)
                .HasForeignKey(sl => sl.ClientInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemoRequest>()
                .HasOne(dr => dr.ClientInstance)
                .WithMany()
                .HasForeignKey(dr => dr.ClientInstanceId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public static Central_HubContext Create()
        {
            return new Central_HubContext();
        }
    }
}
