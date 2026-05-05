using Blocks.MassTransit;
using MassTransit;

namespace Accounts.Application.DependencyInjection;

public class AccountsMassTransitModule : IMassTransitModule
{
    public void AddMassTransit(IBusRegistrationConfigurator x)
    {
        x.AddConsumers(GetType().Assembly);
    }
}