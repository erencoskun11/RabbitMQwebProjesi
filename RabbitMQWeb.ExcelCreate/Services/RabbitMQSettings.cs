namespace RabbitMQWeb.ExcelCreate.Services
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public bool SslEnabled { get; set; } = false;
        // Development only: allow invalid/untrusted certs to ease local testing.
        // NEVER set to true in production.
        public bool AllowInvalidCertificates { get; set; } = false;
        // optional: Url (amqps://user:pass@host/vhost) - not used by default but handy
        public string? Url { get; set; }
    }
}
