using System.Reflection;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeInvestigationSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}
