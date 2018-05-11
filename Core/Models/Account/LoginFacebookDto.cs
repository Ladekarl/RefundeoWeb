namespace Refundeo.Core.Models.Account
{
    public class LoginFacebookDto
    {
        public string AccessToken { get; set; }
        public string[] Scopes { get; set; }
    }
}
