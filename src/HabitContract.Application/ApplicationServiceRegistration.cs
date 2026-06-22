using FluentValidation;
using HabitContract.Application.Interfaces;
using HabitContract.Application.Mappings;
using HabitContract.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HabitContract.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IContractPartnerService, ContractPartnerService>();
        services.AddScoped<ICheckInService, CheckInService>();
        services.AddScoped<IContractViolationService, ContractViolationService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<IReminderTemplateService, ReminderTemplateService>();
        services.AddScoped<IFrequencyParser, FrequencyParser>();
        services.AddScoped<IFrequencyRuleCache, FrequencyRuleCache>();

        services.AddHostedService<ReminderBackgroundService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
