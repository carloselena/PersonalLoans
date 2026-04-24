using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Lending.Application;

public static class ServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly));
        
        services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);
    }
}