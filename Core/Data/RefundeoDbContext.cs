using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Data.Models;

namespace Refundeo.Core.Data
{
    public class RefundeoDbContext : IdentityDbContext<RefundeoUser>
    {
        public RefundeoDbContext(DbContextOptions<RefundeoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CustomerInformation>()
                .HasOne(i => i.Customer)
                .WithOne(c => c.CustomerInformation)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MerchantInformation>()
                .HasOne(i => i.Merchant)
                .WithOne(c => c.MerchantInformation);
        }

        public DbSet<CustomerInformation> CustomerInformations { get; set; }
        public DbSet<MerchantInformation> MerchantInformations { get; set; }
        public DbSet<RefundCase> RefundCases { get; set; }
        public DbSet<Documentation> Documentations { get; set; }
        public DbSet<QRCode> QRCodes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Location> Locations { get; set; }
    }
}
