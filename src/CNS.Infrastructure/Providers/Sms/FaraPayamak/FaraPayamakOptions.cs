using System.Reflection.PortableExecutable;

namespace CNS.Infrastructure.Providers.Sms.FaraPayamak
{
    public class FaraPayamakOptions
    {
        public string ProviderName { get; set; } = "";
        public string Url { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string To { get; set; } = "";
        public string From { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
