using MassTransit;

namespace Blocks.MassTransit;

public interface IMassTransitModule
{
    void AddMassTransit(IBusRegistrationConfigurator x);
}