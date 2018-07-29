namespace Refundeo.Core.Models.Account
{
    public class ResetPasswordDto
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
    }
}
