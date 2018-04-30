namespace Refundeo.Core.Models.Account
{
    public class LoginFacebookDTO
    {
        public string AccessToken {get; set;}
        public string[] Scopes { get; set; }
    }
}
