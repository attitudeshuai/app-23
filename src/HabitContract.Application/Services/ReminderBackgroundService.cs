using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HabitContract.Application.Interfaces;

namespace HabitContract.Application.Services;

public class ReminderBackgroundService : BackgroundService
{
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(1);

    public ReminderBackgroundService(
        ILogger<ReminderBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("提醒定时扫描服务已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();

                await reminderService.ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行提醒扫描任务时发生错误");
            }

            await Task.Delay(_scanInterval, stoppingToken);
        }

        _logger.LogInformation("提醒定时扫描服务已停止");
    }
}
