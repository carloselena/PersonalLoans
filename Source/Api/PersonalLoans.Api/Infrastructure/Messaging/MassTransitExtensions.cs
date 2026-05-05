using System.Reflection;
using System.Security.Authentication;
using Blocks.MassTransit;
using MassTransit;

namespace PersonalLoans.Api.Infrastructure.Messaging;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitModules(this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var modules = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IMassTransitModule).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IMassTransitModule>();
        
        services.AddMassTransit(x =>
        {
            foreach (var module in modules)
                module.AddMassTransit(x);
            
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMQ");
                
                cfg.Host(rabbit["Host"], rabbit["VHost"],h =>
                {
                    h.Username(rabbit["Username"]!);
                    h.Password(rabbit["Password"]!);
                    h.UseSsl(s => s.Protocol = SslProtocols.Tls12);
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}