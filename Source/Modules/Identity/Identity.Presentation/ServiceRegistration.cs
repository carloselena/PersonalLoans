using Identity.Application.DependencyInjection;
using Identity.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation;

public static class ServiceRegistration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityApplicationServices(configuration)
            .AddIdentityPersistenceServices(configuration);
        
        return services;
    }
}