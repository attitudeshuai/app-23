using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;

namespace HabitContract.Domain.Interfaces;

public interface INotificationSender
{
    ReminderChannel Channel { get; }

    Task<bool> SendAsync(User user, string title, string content);
}
