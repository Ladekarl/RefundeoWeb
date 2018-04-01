using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme { In = "header", Description = "Please enter JWT with Bearer into field", Name = "Authorization", Type = "apiKey" });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
                    { "Bearer", Enumerable.Empty<string>() },
                });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(Configuration["SwaggerEndpoint"], "Refundeo API V1");
            });

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
}
