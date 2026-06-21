using Microsoft.Extensions.Logging;
using HabitContract.Domain.Entities;
using HabitContract.Domain.Enums;
using HabitContract.Domain.Interfaces;

namespace HabitContract.Infrastructure.Services;

public class InAppNotificationSender : INotificationSender
{
    private readonly ILogger<InAppNotificationSender> _logger;

    public ReminderChannel Channel => ReminderChannel.InApp;

    public InAppNotificationSender(ILogger<InAppNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(User user, string title, string content)
    {
        try
        {
            _logger.LogInformation(
                "发送站内信通知给用户 {UserId}({Username}): {Title}",
                user.Id,
                user.Username,
                title);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送站内信通知失败: {UserId}", user.Id);
            return Task.FromResult(false);
        }
    }
}
