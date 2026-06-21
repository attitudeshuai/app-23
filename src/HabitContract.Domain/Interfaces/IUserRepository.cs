using HabitContract.Domain.Entities;

namespace HabitContract.Domain.Interfaces;

public interface IUserRepository : IRepository<User, int>
{
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
}
