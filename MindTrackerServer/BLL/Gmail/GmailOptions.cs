namespace BLL.Gmail
{
    public class GmailOptions
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? SMTP { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
    }
}
