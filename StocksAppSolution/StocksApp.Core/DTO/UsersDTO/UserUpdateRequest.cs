using System.ComponentModel.DataAnnotations;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO.UsersDTO
{
    public class UserUpdateRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        public User ToUser()
        {
            return new User
            {
                UserId = UserId,
                UserName = UserName,
                Email = Email
            };
        }
    }
}