using BT.Application.Behaviours;
using BT.Application.Contracts.Implementations.Common;
using BT.Application.Contracts.Interfaces.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BT.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            RegisterApplicationServices(services);

            return services;
        }
        catch (Exception)
        {
            throw;
        }

    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));

        });

        services.AddScoped<ICustomerNumberGenerator, CustomerNumberGenerator>();
        services.AddScoped<IServiceManager, ServiceManager>();

    }




}
