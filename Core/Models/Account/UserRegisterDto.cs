namespace Refundeo.Core.Models.Account
{
    public class UserRegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string[] Scopes { get; set; }
    }
}
