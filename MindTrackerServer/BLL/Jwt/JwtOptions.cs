using Microsoft.IdentityModel.Tokens;

namespace BLL.Jwt
{
    //1 (1)
    public class JwtOptions
    {
        public string? Issuer { get; set; } 
        public string? Audience { get; set; }
        public string? Key { get; set; } 
        public SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new(System.Text.Encoding.UTF8.GetBytes(Key!));
    }
}
