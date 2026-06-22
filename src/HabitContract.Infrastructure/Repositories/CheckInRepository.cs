using Microsoft.EntityFrameworkCore;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;

namespace HabitContract.Infrastructure.Repositories;

public class CheckInRepository : Repository<CheckIn, int>, ICheckInRepository
{
    public CheckInRepository(HabitContractDbContext context) : base(context)
    {
    }

    public async Task<List<CheckIn>> GetByContractIdAsync(int contractId)
    {
        return await _dbSet
            .Include(ci => ci.User)
            .Where(ci => ci.ContractId == contractId)
            .OrderByDescending(ci => ci.CheckInDate)
            .ToListAsync();
    }

    public async Task<List<CheckIn>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(ci => ci.Contract)
            .Where(ci => ci.UserId == userId)
            .OrderByDescending(ci => ci.CheckInDate)
            .ToListAsync();
    }

    public async Task<List<CheckIn>> GetByContractAndUserIdAsync(int contractId, int userId)
    {
        return await _dbSet
            .Where(ci => ci.ContractId == contractId && ci.UserId == userId)
            .OrderBy(ci => ci.CheckInDate)
            .ToListAsync();
    }

    public async Task<CheckIn?> GetByDateAsync(int contractId, int userId, DateTime checkInDate)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ci => ci.ContractId == contractId && ci.UserId == userId && ci.CheckInDate == checkInDate.Date);
    }

    public async Task<List<CheckIn>> GetPendingCheckInsAsync()
    {
        return await _dbSet
            .Include(ci => ci.Contract)
            .Where(ci => ci.Status == CheckInStatus.Pending)
            .ToListAsync();
    }

    public async Task<int> GetConsecutiveDaysAsync(int contractId, int userId, DateTime checkInDate)
    {
        var checkIns = await _dbSet
            .Where(ci => ci.ContractId == contractId
                && ci.UserId == userId
                && ci.CheckInDate <= checkInDate.Date
                && ci.Status != CheckInStatus.Pending
                && ci.Status != CheckInStatus.Missed)
            .OrderByDescending(ci => ci.CheckInDate)
            .Select(ci => ci.CheckInDate.Date)
            .Distinct()
            .ToListAsync();

        if (!checkIns.Any() || checkIns[0] != checkInDate.Date)
        {
            return 0;
        }

        int consecutive = 0;
        DateTime currentDate = checkInDate.Date;

        foreach (var date in checkIns)
        {
            if (date == currentDate)
            {
                consecutive++;
                currentDate = currentDate.AddDays(-1);
            }
            else if (date < currentDate)
            {
                break;
            }
        }

        return consecutive;
    }

    public async Task<(int CurrentStreak, int LongestStreak)> GetStreaksAsync(int contractId, int userId)
    {
        var checkIns = await _dbSet
            .Where(ci => ci.ContractId == contractId
                && ci.UserId == userId
                && ci.Status != CheckInStatus.Pending
                && ci.Status != CheckInStatus.Missed)
            .OrderBy(ci => ci.CheckInDate)
            .Select(ci => ci.CheckInDate.Date)
            .Distinct()
            .ToListAsync();

        if (!checkIns.Any())
        {
            return (0, 0);
        }

        int currentStreak = 0;
        int longestStreak = 0;
        int tempStreak = 0;
        DateTime? previousDate = null;

        for (int i = checkIns.Count - 1; i >= 0; i--)
        {
            var date = checkIns[i];

            if (previousDate == null)
            {
                if (date == DateTime.UtcNow.Date || date == DateTime.UtcNow.Date.AddDays(-1))
                {
                    currentStreak = 1;
                    tempStreak = 1;
                }
                else
                {
                    currentStreak = 0;
                    tempStreak = 1;
                }
                previousDate = date;
                continue;
            }

            if (previousDate.Value.AddDays(-1) == date)
            {
                tempStreak++;
                if (currentStreak > 0)
                {
                    currentStreak++;
                }
            }
            else
            {
                if (tempStreak > longestStreak)
                {
                    longestStreak = tempStreak;
                }
                tempStreak = 1;
                currentStreak = 0;
            }

            previousDate = date;
        }

        if (tempStreak > longestStreak)
        {
            longestStreak = tempStreak;
        }

        return (currentStreak, longestStreak);
    }

    public async Task UpdateStatusForMissedDeadlinesAsync()
    {
        var now = DateTime.UtcNow;
        var pendingCheckIns = await GetPendingCheckInsAsync();

        var checkInsByUserContract = new Dictionary<(int ContractId, int UserId), List<CheckIn>>();

        foreach (var checkIn in pendingCheckIns)
        {
            var contract = checkIn.Contract;
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(contract.TimeZone);
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);
            var makeUpDeadline = checkIn.CheckInDate.Date.AddDays(contract.MakeUpDeadlineDays);

            if (nowLocal.Date > makeUpDeadline)
            {
                checkIn.Status = CheckInStatus.Missed;
                checkIn.StatusChangedAt = now;
                checkIn.ConsecutiveDays = 0;

                var key = (checkIn.ContractId, checkIn.UserId);
                if (!checkInsByUserContract.ContainsKey(key))
                {
                    checkInsByUserContract[key] = new List<CheckIn>();
                }
                checkInsByUserContract[key].Add(checkIn);
            }
        }

        foreach (var group in checkInsByUserContract)
        {
            var (contractId, userId) = group.Key;
            var missedCheckIns = group.Value.OrderBy(ci => ci.CheckInDate).ToList();
            var allCheckIns = await GetByContractAndUserIdAsync(contractId, userId);

            foreach (var missedCheckIn in missedCheckIns)
            {
                var subsequentCheckIns = allCheckIns
                    .Where(ci => ci.CheckInDate > missedCheckIn.CheckInDate && ci.Status != CheckInStatus.Missed)
                    .OrderBy(ci => ci.CheckInDate)
                    .ToList();

                foreach (var checkIn in subsequentCheckIns)
                {
                    checkIn.ConsecutiveDays = await GetConsecutiveDaysAsync(contractId, userId, checkIn.CheckInDate);
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    protected override IQueryable<CheckIn> ApplySearch(IQueryable<CheckIn> query, string searchTerm)
    {
        return query.Where(ci =>
            ci.ProofText != null && ci.ProofText.Contains(searchTerm));
    }

    protected override IQueryable<CheckIn> ApplySorting(IQueryable<CheckIn> query, string sortBy, bool descending)
    {
        return sortBy.ToLower() switch
        {
            "checkindate" => descending ? query.OrderByDescending(ci => ci.CheckInDate) : query.OrderBy(ci => ci.CheckInDate),
            "createdat" => descending ? query.OrderByDescending(ci => ci.CreatedAt) : query.OrderBy(ci => ci.CreatedAt),
            "status" => descending ? query.OrderByDescending(ci => ci.Status) : query.OrderBy(ci => ci.Status),
            _ => query.OrderByDescending(ci => ci.CheckInDate)
        };
    }
}
