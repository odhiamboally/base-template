using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using System.Text;

namespace BT.SharedKernel.Validation.Extensions; 

public static class DependencyInjection
{
    public static IServiceCollection AddSharedValidationServices(this IServiceCollection services)
    {
        try
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
        catch (Exception)
        {
            throw;
        }

    }

}
