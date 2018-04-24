namespace Refundeo.Core.Models.Account
{
    public class UserLoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string GrantType { get; set; }
        public string RefreshToken { get; set; }
        public string[] Scopes { get; set; }
    }
}
