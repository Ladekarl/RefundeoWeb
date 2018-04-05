using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Refundeo.Data.Models;

namespace Refundeo.Data
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

            builder.Entity<RefundCase>()
                .HasOne(c => c.Customer)
                .WithMany(u => u.MerchantRefundCases)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RefundCase>()
                .HasOne(c => c.Merchant)
                .WithMany(u => u.CustomerRefundCases)
                .HasForeignKey(c => c.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<RefundCase> RefundCases { get; set; }
        public DbSet<Documentation> Documentations { get; set; }
        public DbSet<QRCode> QRCodes { get; set; }
    }
}