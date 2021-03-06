// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Refundeo.Core.Data;
using System;

namespace Refundeo.Migrations
{
    [DbContext(typeof(RefundeoDbContext))]
    [Migration("20180629140457_Language")]
    partial class Language
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.Address", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City");

                    b.Property<string>("Country");

                    b.Property<string>("PostalCode");

                    b.Property<string>("StreetName");

                    b.Property<string>("StreetNumber");

                    b.HasKey("Id");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.CustomerInformation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AcceptedPrivacyPolicy");

                    b.Property<bool>("AcceptedTermsOfService");

                    b.Property<string>("AccountNumber");

                    b.Property<long?>("AddressId");

                    b.Property<string>("Country");

                    b.Property<string>("CustomerId");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<bool>("IsOauth");

                    b.Property<string>("LastName");

                    b.Property<string>("Passport");

                    b.Property<string>("Phone");

                    b.Property<string>("PrivacyPolicy");

                    b.Property<string>("QRCode");

                    b.Property<string>("Swift");

                    b.Property<string>("TermsOfService");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.HasIndex("CustomerId")
                        .IsUnique()
                        .HasFilter("[CustomerId] IS NOT NULL");

                    b.ToTable("CustomerInformations");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.Language", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Key");

                    b.Property<string>("RefundCreatedText");

                    b.Property<string>("RefundCreatedTitle");

                    b.Property<string>("RefundUpdateText");

                    b.Property<string>("RefundUpdateTitle");

                    b.HasKey("Id");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.Location", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.HasKey("Id");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.MerchantInformation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("AddressId");

                    b.Property<string>("Banner");

                    b.Property<string>("CVRNumber");

                    b.Property<string>("CompanyName");

                    b.Property<string>("ContactEmail");

                    b.Property<string>("ContactPhone");

                    b.Property<string>("Currency");

                    b.Property<string>("Description");

                    b.Property<long?>("LocationId");

                    b.Property<string>("Logo");

                    b.Property<double>("RefundPercentage");

                    b.Property<string>("VATNumber");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.HasIndex("LocationId");

                    b.ToTable("MerchantInformations");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.MerchantInformationTag", b =>
                {
                    b.Property<long>("MerhantInformationId");

                    b.Property<long>("TagId");

                    b.HasKey("MerhantInformationId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("MerchantInformationTags");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.OpeningHours", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Close");

                    b.Property<int>("Day");

                    b.Property<long?>("MerchantInformationId");

                    b.Property<string>("Open");

                    b.HasKey("Id");

                    b.HasIndex("MerchantInformationId");

                    b.ToTable("OpeningHours");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.RefundCase", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Amount");

                    b.Property<long?>("CustomerInformationId");

                    b.Property<string>("CustomerSignature");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateRequested");

                    b.Property<bool>("IsAccepted");

                    b.Property<bool>("IsRejected");

                    b.Property<bool>("IsRequested");

                    b.Property<long?>("MerchantInformationId");

                    b.Property<string>("MerchantSignature");

                    b.Property<string>("QRCode");

                    b.Property<string>("ReceiptImage");

                    b.Property<string>("ReceiptNumber");

                    b.Property<double>("RefundAmount");

                    b.Property<string>("VATFormImage");

                    b.HasKey("Id");

                    b.HasIndex("CustomerInformationId");

                    b.HasIndex("MerchantInformationId");

                    b.ToTable("RefundCases");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.RefundeoUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<long?>("MerchantInformationId");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("RefreshToken");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("MerchantInformationId")
                        .IsUnique()
                        .HasFilter("[MerchantInformationId] IS NOT NULL");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.Tag", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Key");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.RefundeoUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.RefundeoUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Refundeo.Core.Data.Models.RefundeoUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.RefundeoUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.CustomerInformation", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.Address", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId");

                    b.HasOne("Refundeo.Core.Data.Models.RefundeoUser", "Customer")
                        .WithOne("CustomerInformation")
                        .HasForeignKey("Refundeo.Core.Data.Models.CustomerInformation", "CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.MerchantInformation", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.Address", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId");

                    b.HasOne("Refundeo.Core.Data.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.MerchantInformationTag", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.MerchantInformation", "MerchantInformation")
                        .WithMany("MerchantInformationTags")
                        .HasForeignKey("MerhantInformationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Refundeo.Core.Data.Models.Tag", "Tag")
                        .WithMany("MerchantInformationTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.OpeningHours", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.MerchantInformation")
                        .WithMany("OpeningHours")
                        .HasForeignKey("MerchantInformationId");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.RefundCase", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.CustomerInformation", "CustomerInformation")
                        .WithMany("RefundCases")
                        .HasForeignKey("CustomerInformationId");

                    b.HasOne("Refundeo.Core.Data.Models.MerchantInformation", "MerchantInformation")
                        .WithMany("RefundCases")
                        .HasForeignKey("MerchantInformationId");
                });

            modelBuilder.Entity("Refundeo.Core.Data.Models.RefundeoUser", b =>
                {
                    b.HasOne("Refundeo.Core.Data.Models.MerchantInformation", "MerchantInformation")
                        .WithOne("Merchant")
                        .HasForeignKey("Refundeo.Core.Data.Models.RefundeoUser", "MerchantInformationId");
                });
#pragma warning restore 612, 618
        }
    }
}
