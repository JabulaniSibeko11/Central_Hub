namespace Central_Hub.Infrastructure.Options
{
    public sealed class EmailOptions
    {
        public bool Enabled { get; set; } = false;

        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;

        public bool UseSsl { get; set; } = true;

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public string FromAddress { get; set; } = "no-reply@inspiredit.local";
        public string FromName { get; set; } = "Inspired IT Central Hub";

        // optional: use this to catch-all during testing
        public string? OverrideTo { get; set; }
    }
}
