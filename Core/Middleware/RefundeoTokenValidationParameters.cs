using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Refundeo.Core.Middleware
{
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