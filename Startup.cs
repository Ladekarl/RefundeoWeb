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
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models;
using Swashbuckle.AspNetCore.Swagger;


namespace Refundeo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

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
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization();

            services.AddMvc();

            services.AddCors();

            services.AddDbContext<RefundeoDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("RefundeoConnection")));

            services.AddIdentity<RefundeoUser, IdentityRole>()
                .AddEntityFrameworkStores<RefundeoDbContext>()
                .AddDefaultTokenProviders();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Refundeo", Version = "v1" });
            });

            var refundeoTokenValidationParameters = new RefundeoTokenValidationParameters(Configuration);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = refundeoTokenValidationParameters.TokenValidationParameters;
            });

            services.AddSingleton<IConfiguration>(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, UserManager<RefundeoUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseCors(builder => builder.WithOrigins(Configuration["AngularServer"]));

            app.UseSwaggerAuthorized();
            app.UseSwagger();

            app.UseAuthentication();

            DbInitializer.InitializeAsync(userManager, roleManager).Wait();

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

    public static class SwaggerAuthorizeExtensions
    {
        public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerAuthorizedMiddleware>();
        }
    }

    public class SwaggerAuthorizedMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAuthorizedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IConfiguration Configuration)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                SecurityToken jwt;
                var token = await context.GetTokenAsync("JwtBearer", "access_token");

                if (token != null)
                {
                    var refundeoParameters = new RefundeoTokenValidationParameters(Configuration);
                    var principal = new JwtSecurityTokenHandler().ValidateToken(token, refundeoParameters.TokenValidationParameters, out jwt);

                    if (principal.IsInRole("Admin") ||
                        principal.IsInRole("Merchant"))
                    {
                        await _next.Invoke(context);
                        return;
                    }
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            
            await _next.Invoke(context);
        }
    }

    public class RefundeoTokenValidationParameters
    {
        public TokenValidationParameters TokenValidationParameters { get; set; }
        public RefundeoTokenValidationParameters(IConfiguration Configuration)
        {
            TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"])),

                ValidateIssuer = true,
                ValidIssuer = Configuration["ValidIssuer"],

                ValidateAudience = true,
                ValidAudience = Configuration["ValidAudience"],

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromMinutes(5)
            };
        }
    }
}
