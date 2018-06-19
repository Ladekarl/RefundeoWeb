using System;

namespace Refundeo.Core.Helpers
{
    public class EmailAccountOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
