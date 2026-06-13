using System.ComponentModel.DataAnnotations;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO.UsersDTO
{
    public class UserAddRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        public User ToUser()
        {
            return new User
            {
                UserId = Guid.NewGuid(),
                UserName = UserName,
                Email = Email,
                CashBalance = 100000 // Default balance
            };
        }
    }
}