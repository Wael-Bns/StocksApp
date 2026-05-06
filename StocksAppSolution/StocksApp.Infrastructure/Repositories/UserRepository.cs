using Microsoft.EntityFrameworkCore;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<User> AddUser(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUser(Guid userId)
        {
            int rowsAffected = await _db.Users
                                        .Where(u => u.UserId == userId)
                                        .ExecuteDeleteAsync();
            return rowsAffected > 0;   
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> UpdateUser(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }
    }
}