using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Refundeo.Core.Middleware
{
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

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}