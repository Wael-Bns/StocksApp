using System.ComponentModel.DataAnnotations;

namespace StocksApp.Core.DTO.AuthenticationDTO
{
    public class TokenModel
    {
        [Required]
        public string? Token { get; set; }
        [Required]
        public string? RefreshToken { get; set; }
    }
}
