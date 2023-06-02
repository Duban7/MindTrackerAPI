namespace Domain.Models
{
    public class RefreshToken
    {
        //1 (1)
        public string? Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
    }
}
