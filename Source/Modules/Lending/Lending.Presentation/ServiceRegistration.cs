using Lending.Presentation.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lending.Presentation;

public static class ServiceRegistration
{
    public static IServiceCollection AddLendingModule(this IServiceCollection services, IConfiguration configuration)
    {
        Persistence.ServiceRegistration.AddPersistenceServices(services, configuration);
        Application.ServiceRegistration.AddApplicationServices(services);

        services.AddHostedService<AccrueDailyPenaltiesJob>();
        
        return services;
    }
}