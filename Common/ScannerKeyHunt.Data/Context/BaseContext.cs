using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ScannerKeyHunt.Data.Entities;

namespace ScannerKeyHunt.Data.Context
{
    public class BaseContext : IdentityDbContext<User>
    {
        private readonly IConfiguration _configuration;

        public BaseContext() { }

        public BaseContext(DbContextOptions<BaseContext> options) : base(options) { }

        public DbSet<TokenAuth>? TokenAuths { get; set; }

        protected virtual void InitalizeContext()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            Database.SetCommandTimeout(360);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>(entity =>
            //{
            //    entity.ToTable("User");
            //    entity.HasKey(e => e.Id);
            //    entity.Property(e => e.Username).IsRequired();
            //    entity.Property(e => e.Email).IsRequired();
            //    entity.Property(e => e.Password).IsRequired();
            //});

            base.OnModelCreating(modelBuilder);
        }
    }
}
