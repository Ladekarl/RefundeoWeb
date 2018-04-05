using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Refundeo.Data.Models;

namespace Refundeo.Controllers
{
    public abstract class AuthenticationController : RefundeoController
    {
        public IConfiguration Configuration { get; set; }
        protected readonly SignInManager<RefundeoUser> signManager;
        public AuthenticationController(IConfiguration config, UserManager<RefundeoUser> userManager, SignInManager<RefundeoUser> signManager) : base(userManager)
        {
            Configuration = config;
            this.signManager = signManager;
        }

        protected async Task<IdentityResult> UpdateRolesAsync(RefundeoUser user, ICollection<string> roles)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            var rolesAdded = roles.Except(currentRoles);
            var rolesRemoved = currentRoles.Except(roles);

            if (rolesAdded.Count() > 0)
            {
                var result = await userManager.AddToRolesAsync(user, rolesAdded);
                if (!result.Succeeded)
                {
                    return result;
                }
            }

            if (rolesRemoved.Count() > 0)
            {
                var result = await userManager.RemoveFromRolesAsync(user, rolesRemoved);
                if (!result.Succeeded)
                {
                    return result;
                }
            }

            return IdentityResult.Success;
        }

        protected ObjectResult GenerateBadRequestObjectResult(params string[] errors)
        {
            return GenerateBadRequestObjectResult(errors.ToList());
        }

        protected ObjectResult GenerateBadRequestObjectResult(IEnumerable errors)
        {
            return new BadRequestObjectResult(new
            {
                errors = errors
            });
        }

        protected async Task<ObjectResult> GenerateTokenResultAsync(RefundeoUser user)
        {
            var token = await GenerateTokenAsync(user);

            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                id = user.Id,
                username = user.UserName,
                roles = await userManager.GetRolesAsync(user)
            });
        }

        protected async Task<SignInResult> IsValidUserAndPasswordCombinationAsync(string username, string password)
        {
            if (String.IsNullOrEmpty(username))
            {
                return new SignInResult.NoUsername();
            }
            if (String.IsNullOrEmpty(password))
            {
                return new SignInResult.NoPassword();
            }

            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                return new SignInResult.UserDoesNotExist();
            }

            var isValid = await signManager.UserManager.CheckPasswordAsync(user, password);

            if (!isValid)
            {
                return new SignInResult.WrongPassword();
            }
            return new SignInResult.Success();
        }

        protected async Task<JwtSecurityToken> GenerateTokenAsync(RefundeoUser user)
        {
            var claims = GenerateClaims(user);

            var roles = await userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"])),
                        SecurityAlgorithms.HmacSha256)),
                        new JwtPayload(claims));
        }

        protected List<Claim> GenerateClaims(RefundeoUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, Configuration["ValidAudience"]),
                new Claim(JwtRegisteredClaimNames.Iss, Configuration["ValidIssuer"])
            };
            return claims;
        }
    }

    #region Helper Classes

    public abstract class SignInResult
    {
        public abstract SignInId Id { get; }
        public abstract string Desc { get; }

        public class WrongPassword : SignInResult
        {
            public override SignInId Id { get { return SignInId.WRONG_PASSWORD; } }
            public override string Desc { get { return "Wrong password"; } }
        }

        public class UserDoesNotExist : SignInResult
        {
            public override SignInId Id { get { return SignInId.USER_DOES_NOT_EXIST; } }
            public override string Desc { get { return "User does not exist"; } }
        }

        public class NoPassword : SignInResult
        {
            public override SignInId Id { get { return SignInId.NO_PASSWORD; } }
            public override string Desc { get { return "No password provided"; } }
        }

        public class NoUsername : SignInResult
        {
            public override SignInId Id { get { return SignInId.NO_USERNAME; } }
            public override string Desc { get { return "No username provided"; } }
        }

        public class Success : SignInResult
        {
            public override SignInId Id { get { return SignInId.SUCCESS; } }
            public override string Desc { get { return "Success"; } }
        }
    }

    public enum SignInId
    {
        SUCCESS = 1,
        WRONG_PASSWORD = -1,
        USER_DOES_NOT_EXIST = -2,
        NO_PASSWORD = -3,
        NO_USERNAME = -4
    }

    #endregion
}