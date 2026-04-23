using Microsoft.EntityFrameworkCore;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ApplicationUser?> GetUserByUserId(Guid userId)
        {
            return await _dbContext.ApplicationUsers.FirstOrDefaultAsync(user => user.UserId == userId);
        }
        public async Task<bool> UpdateUser(ApplicationUser user)
        {
            _dbContext.ApplicationUsers.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
