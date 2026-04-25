using StocksApp.Core.DTO.UsersDTO;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// An abstraction for user-related operations, such as registration and authentication. 
    /// </summary>
    public interface IApplicationUserService
    {
        /// <summary>
        /// A method to register a new user in the system.
        /// </summary>
        /// <param name="applicationUserDTO"></param>
        /// <returns></returns>
        Task<ApplicationUserResponse> RegisterUser(ApplicationUserRegisterDTO applicationUserDTO);
    }
}
