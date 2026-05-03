using Identity.Application;
using Identity.Persistence;
using Identity.Presentation.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation;

public static class ServiceRegistration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityApplicationServices(configuration)
            .AddIdentityPersistenceServices(configuration);
        
        services.AddHostedService<OutboxProcessorJob>();

        return services;
    }
}