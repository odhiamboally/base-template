using BT.Domain.IAM.Entities;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Contracts.Implementations.Services;
using BT.Application.Contracts.Interfaces.Services;
using BT.Infrastructure.Extensions;
using BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;
using BT.Persistence.IAM.DataContext;
using BT.Persistence.IAM.Extensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BT.Infrastructure.IAM.Extensions;

public static class IamModuleDI
{
    public static IServiceCollection AddIamModule(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddIamPersistence(configuration);

        services.AddIdentity<AppUser, IdentityRole>(DependencyInjection.ConfigureIdentityOptions)
            .AddEntityFrameworkStores<IamDbContext>()
            .AddDefaultTokenProviders();

        DependencyInjection.ConfigureAuthentication(services, configuration);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Login>());

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IClaimsService, ClaimsService>();

        return services;
    }
}
