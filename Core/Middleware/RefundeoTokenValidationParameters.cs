using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Refundeo.Core.Middleware
{
    public class RefundeoTokenValidationParameters
    {
        public TokenValidationParameters TokenValidationParameters { get; }

        public RefundeoTokenValidationParameters(IConfiguration configuration)
        {
            TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"])),

                ValidateIssuer = true,
                ValidIssuer = configuration["ValidIssuer"],

                ValidateAudience = true,
                ValidAudience = configuration["ValidAudience"],

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromMinutes(5)
            };
        }
    }
}
