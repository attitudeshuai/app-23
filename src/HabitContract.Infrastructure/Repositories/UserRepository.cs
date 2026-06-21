using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class UserRepository : Repository<User, int>, IUserRepository
{
    public UserRepository(HabitContractDbContext context) : base(context)
    {
    }

    // 根据用户名或邮箱查找用户，用于登录认证
    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _dbSet.FirstOrDefaultAsync(u =>
            u.Username == usernameOrEmail || u.Email == usernameOrEmail);
    }

    protected override IQueryable<User> ApplySearch(IQueryable<User> query, string searchTerm)
    {
        return query.Where(u =>
            u.Username.Contains(searchTerm) ||
            u.Email.Contains(searchTerm));
    }

    protected override IQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "username" => descending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            "email" => descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "createdat" => descending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };
    }
}
