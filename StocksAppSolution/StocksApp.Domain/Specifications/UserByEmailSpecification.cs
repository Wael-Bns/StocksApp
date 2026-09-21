using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Specifications
{
    public class UserByEmailSpecification : BaseSpecification<User>
    {
        public UserByEmailSpecification(string email) : base(u => u.Email == email) { }
    }
}