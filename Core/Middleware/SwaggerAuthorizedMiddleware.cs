using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Refundeo.Core.Middleware
{
    public class SwaggerAuthorizedMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAuthorizedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IConfiguration configuration)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                var token = await context.GetTokenAsync("JwtBearer", "access_token");

                if (token != null)
                {
                    var refundeoParameters = new RefundeoTokenValidationParameters(configuration);
                    var principal = new JwtSecurityTokenHandler().ValidateToken(token,
                        refundeoParameters.TokenValidationParameters, out _);

                    if (principal.IsInRole("Admin") ||
                        principal.IsInRole("Merchant"))
                    {
                        await _next.Invoke(context);
                        return;
                    }
                }

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
