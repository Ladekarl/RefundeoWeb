using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<IdentityResult> UpdateRolesAsync(RefundeoUser user, ICollection<string> roles);
        Task<ObjectResult> GenerateTokenResultAsync(RefundeoUser user, string refreshToken = null);
        Task<Helpers.SignInResult> IsValidUserAndPasswordCombinationAsync(string username, string password);
        Task<JwtSecurityToken> GenerateTokenAsync(RefundeoUser user);
        ICollection<Claim> GenerateClaims(RefundeoUser user);
        string GenerateRandomPassword(PasswordOptions opts = null);

        Task<ObjectResult> RegisterUserAsync(RefundeoUser user, string password,
            CustomerInformation customerInformation, bool shouldCreateRefreshToken = false);

        Task<string> CreateAndSaveRefreshTokenAsync(RefundeoUser user);
    }
}
