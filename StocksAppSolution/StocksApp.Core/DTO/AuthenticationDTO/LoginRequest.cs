using System.ComponentModel.DataAnnotations;

namespace StocksApp.Core.DTO.AuthenticationDTO
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
