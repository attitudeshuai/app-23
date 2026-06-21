using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HabitContract.Domain.Interfaces;
using HabitContract.Infrastructure.Data;
using HabitContract.Infrastructure.Repositories;
using HabitContract.Infrastructure.Services;

namespace HabitContract.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 从环境变量读取数据库连接信息，支持容器化部署
        var dbHost = configuration["DB_HOST"];
        var dbPort = configuration["DB_PORT"];
        var dbName = configuration["DB_NAME"];
        var dbUser = configuration["DB_USER"];
        var dbPassword = configuration["DB_PASSWORD"];

        string connectionString;

        if (!string.IsNullOrEmpty(dbHost))
        {
            connectionString = $"Server={dbHost};Port={dbPort ?? "3306"};Database={dbName ?? "habitcontract"};User={dbUser ?? "app_user"};Password={dbPassword ?? "app_pass"};";
        }
        else
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Port=3306;Database=habitcontract;User=root;Password=123456;";
        }

        // 注册MySQL数据库上下文（使用Pomelo驱动）
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
        services.AddDbContext<HabitContractDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        // 注册所有仓储和服务
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IContractPartnerRepository, ContractPartnerRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<IContractViolationRepository, ContractViolationRepository>();
        services.AddScoped<IHabitTemplateCategoryRepository, HabitTemplateCategoryRepository>();
        services.AddScoped<IHabitTemplateRepository, HabitTemplateRepository>();
        services.AddScoped<IHabitTemplateVersionRepository, HabitTemplateVersionRepository>();
        services.AddScoped<IContractReminderSettingRepository, ContractReminderSettingRepository>();
        services.AddScoped<IReminderRecordRepository, ReminderRecordRepository>();
        services.AddScoped<IReminderTemplateRepository, ReminderTemplateRepository>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<INotificationSender, InAppNotificationSender>();

        return services;
    }
}
