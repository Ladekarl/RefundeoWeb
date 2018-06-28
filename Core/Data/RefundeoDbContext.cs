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

            builder.Entity<MerchantInformationTag>()
                .HasKey(m => new {m.MerhantInformationId, m.TagId});

            builder.Entity<MerchantInformationTag>()
                .HasOne(m => m.MerchantInformation)
                .WithMany(m => m.MerchantInformationTags)
                .HasForeignKey(m => m.MerhantInformationId);

            builder.Entity<MerchantInformationTag>()
                .HasOne(m => m.Tag)
                .WithMany(m => m.MerchantInformationTags)
                .HasForeignKey(m => m.TagId);
        }

        public DbSet<CustomerInformation> CustomerInformations { get; set; }
        public DbSet<MerchantInformation> MerchantInformations { get; set; }
        public DbSet<MerchantInformationTag> MerchantInformationTags { get; set; }
        public DbSet<RefundCase> RefundCases { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<OpeningHours> OpeningHours { get; set; }
    }
}
