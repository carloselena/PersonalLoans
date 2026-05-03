using Identity.Application;
using Identity.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation;

public static class ServiceRegistration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityApplicationServices(configuration)
            .AddIdentityPersistenceServices(configuration);

        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<IdentityDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
            
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);
                });
                
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}