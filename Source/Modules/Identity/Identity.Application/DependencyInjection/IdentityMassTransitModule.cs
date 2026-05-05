using Blocks.MassTransit;
using MassTransit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application.DependencyInjection;

public class IdentityMassTransitModule : IMassTransitModule
{
    public void AddMassTransit(IBusRegistrationConfigurator x)
    {
        // todo - add consumers
            
        x.AddEntityFrameworkOutbox<IdentityDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    }
}