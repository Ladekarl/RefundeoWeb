using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;
using Refundeo.Core.Middleware;
using Refundeo.Core.Services;
using Refundeo.Core.Services.Interfaces;
using Swashbuckle.AspNetCore.Swagger;

namespace Refundeo
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostingEnvironment HostingEnvironment { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                builder.AddUserSecrets<Startup>();
            }
            else
            {
                var config = builder.Build();

                builder.AddAzureKeyVault(
                    $"https://{config["Vault"]}.vault.azure.net/",
                    config["ClientId"],
                    config["ClientSecret"]);
            }

            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddMvc();
            services.AddCors();

            services.AddTransient<IUtilityService, UtilityService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IRefundCaseService, RefundCaseService>();
            services.AddTransient(typeof(IPaginationService<>), typeof(PaginationService<>));

            services.Configure<StorageAccountOptions>(Configuration.GetSection("StorageAccount"));
            services.Configure<EmailAccountOptions>(Configuration.GetSection("EmailAccount"));

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddSingleton<IBlobStorageService, BlobStorageServiceService>();
            services.AddSingleton<IEmailService, EmailService>();

            services.AddDbContext<RefundeoDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("RefundeoConnection")));

            services.AddIdentity<RefundeoUser, IdentityRole>(opts =>
                {
                    opts.Password.RequireDigit = false;
                    opts.Password.RequireLowercase = true;
                    opts.Password.RequireUppercase = false;
                    opts.Password.RequireNonAlphanumeric = false;
                    opts.Password.RequiredLength = 8;
                    opts.Password.RequiredUniqueChars = 4;
                })
                .AddEntityFrameworkStores<RefundeoDbContext>()
                .AddDefaultTokenProviders();

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Refundeo", Version = "v1"}); });

            services.AddSingleton(Configuration);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "JwtBearer";
                    options.DefaultChallengeScheme = "JwtBearer";
                })
                .AddJwtBearer("JwtBearer", jwtBearerOptions =>
                    jwtBearerOptions.TokenValidationParameters = new RefundeoTokenValidationParameters(Configuration)
                        .TokenValidationParameters);

            if (!HostingEnvironment.IsDevelopment())
            {
                services.Configure<MvcOptions>(options => { options.Filters.Add(new RequireHttpsAttribute()); });
            }
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
            UserManager<RefundeoUser> userManager, RoleManager<IdentityRole> roleManager, RefundeoDbContext dbContext)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (HostingEnvironment.IsDevelopment())
            {
                loggerFactory.AddDebug();
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseCors(builder => builder.WithOrigins(Configuration["AngularServer"]));
            }
            else
            {
                app.UseSwaggerAuthorized();
                var options = new RewriteOptions()
                    .AddRedirectToHttps();
                app.UseRewriter(options);
            }

            app.UseSwagger();
            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 &&
                    !Path.HasExtension(context.Request.Path.Value) &&
                    !context.Request.Path.Value.StartsWith("/swagger") &&
                    !context.Request.Path.Value.StartsWith("/Token") &&
                    !context.Request.Path.Value.StartsWith("/api/"))
                {
                    context.Request.Path = new PathString("/index.html");
                    await next();
                }
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
