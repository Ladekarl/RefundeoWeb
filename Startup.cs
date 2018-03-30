using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Refundeo.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace Refundeo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            }

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            services.AddDbContext<RefundeoDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("RefundeoDevDB")));      

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

            services.AddAuthentication(options => {
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
                    ValidIssuer = "refundeo.dk",

                    ValidateAudience = true,
                    ValidAudience = "refundeo.dk",

                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, UserManager<RefundeoUser> userManager)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            var options = new RewriteOptions()
            .AddRedirectToHttps();

            app.UseRewriter(options);

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Refundeo API V1");
            });

            app.UseStaticFiles();

            app.UseAuthentication();

            DbInitializer.Initialize(userManager);
            
            app.UseMvc();
        }
    }
}
