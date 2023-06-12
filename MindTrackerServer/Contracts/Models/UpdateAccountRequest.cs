namespace Domain.Models
{
    public class UpdateAccountRequest
    {
        public string? OldPassword { get; set; }    
        public string? NewPassword { get; set; }
        public string? NewEmail { get; set; }
    }
}
