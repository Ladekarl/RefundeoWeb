using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Helpers;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IAuthenticationService
    {
         Task<IdentityResult> UpdateRolesAsync(RefundeoUser user, ICollection<string> roles);
         Task<ObjectResult> GenerateTokenResultAsync(RefundeoUser user, string refreshToken);
         Task<Helpers.SignInResult> IsValidUserAndPasswordCombinationAsync(string username, string password);
         Task<JwtSecurityToken> GenerateTokenAsync(RefundeoUser user);
         ICollection<Claim> GenerateClaims(RefundeoUser user);

    }
}
