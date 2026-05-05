using Accounts.Domain;
using Accounts.Domain.Lenders;
using Blocks.IntegrationEvents;
using MassTransit;

namespace Accounts.Application.Consumers;

public class UserCreatedConsumer(
    ILenderRepository lenderRepository,
    IAccountsUnitOfWork unitOfWork) : IConsumer<UserCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
    {
        var existing = await lenderRepository.GetByUserIdAsync(context.Message.UserId, context.CancellationToken);

        if (existing is not null)
            return;
        
        var lender = Lender.Create(context.Message.UserId);
        await lenderRepository.AddAsync(lender);
        await unitOfWork.CommitAsync();
    }
}