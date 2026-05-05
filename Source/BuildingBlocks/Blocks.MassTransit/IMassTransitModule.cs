using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Blocks.MassTransit;

public interface IMassTransitModule
{
    void AddMassTransit(IBusRegistrationConfigurator x);
}