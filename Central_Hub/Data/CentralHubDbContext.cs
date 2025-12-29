using Central_Hub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Central_Hub.Data
{
    public class Central_HubDbContext : IdentityDbContext<IdentityUser>
    {
        public Central_HubDbContext(DbContextOptions<Central_HubDbContext> options)
            : base(options)
        {
        }
        public DbSet<Users> CentralUser { get; set; }
        public DbSet<ClientCompany> ClientCompanies { get; set; }
        public DbSet<CompanyAdministrator> CompanyAdministrators { get; set; }
        public DbSet<DemoRequest> DemoRequests { get; set; }
        public DbSet<CreditBatch> CreditBatches { get; set; }
        public DbSet<CreditRequest> CreditRequests { get; set; }
        public DbSet<CreditTransaction> CreditTransactions { get; set; }
        public DbSet<LicenseRenewal> LicenseRenewals { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<ClientCompany>()
                .HasOne(c => c.Administrator)
                .WithOne(a => a.Company)
                .HasForeignKey<CompanyAdministrator>(a => a.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClientCompany>()
                .HasMany(c => c.CreditTransactions)
                .WithOne(t => t.Company)
                .HasForeignKey(t => t.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClientCompany>()
                .HasMany(c => c.LicenseRenewals)
                .WithOne(r => r.Company)
                .HasForeignKey(r => r.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<ClientCompany>()
                .HasIndex(c => c.LicenseKey)
                .IsUnique();

            modelBuilder.Entity<ClientCompany>()
                .HasIndex(c => c.EmailDomain);

            modelBuilder.Entity<CompanyAdministrator>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<DemoRequest>()
                .HasIndex(d => d.Email);

            modelBuilder.Entity<DemoRequest>()
                .HasIndex(d => d.Status);

            // On CreditBatch
            modelBuilder.Entity<CreditBatch>()
                .HasIndex(b => b.ExpiryDate);
        }
    }
}