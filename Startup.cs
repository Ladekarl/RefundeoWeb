using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Initializers;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Middleware;
using Refundeo.Core.Models;
using Refundeo.Core.Services;
using Refundeo.Core.Services.Interfaces;
using Swashbuckle.AspNetCore.Swagger;


namespace Refundeo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);
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
            services.AddTransient<Core.Services.Interfaces.IAuthenticationService, Core.Services.AuthenticationService>();
            services.AddTransient<IRefundCaseService, RefundCaseService>();
            services.AddTransient(typeof(IPaginationService<>), typeof(PaginationService<>));

            services.AddDbContext<RefundeoDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("RefundeoConnection")));

            services.AddIdentity<RefundeoUser, IdentityRole>()
                .AddEntityFrameworkStores<RefundeoDbContext>()
                .AddDefaultTokenProviders();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Refundeo", Version = "v1" });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
                jwtBearerOptions.TokenValidationParameters = new RefundeoTokenValidationParameters(Configuration).TokenValidationParameters);

            services.AddSingleton<IConfiguration>(Configuration);

            if (!HostingEnvironment.IsDevelopment())
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                });
            }
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, UserManager<RefundeoUser> userManager, RoleManager<IdentityRole> roleManager, RefundeoDbContext dbContext)
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
                //app.UseCors(builder => builder.WithOrigins(Configuration["AngularServer"]));
            }

            app.UseSwagger();
            app.UseAuthentication();

            DbInitializer.InitializeAsync(userManager, roleManager, dbContext).Wait();

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
