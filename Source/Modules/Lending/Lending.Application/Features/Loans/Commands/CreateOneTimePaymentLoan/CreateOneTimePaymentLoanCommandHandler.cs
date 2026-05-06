using Blocks.Domain.ValueObjects;
using Lending.Domain;
using Lending.Domain.Loans;
using Lending.Domain.Loans.ValueObjects;
using MediatR;

namespace Lending.Application.Features.Loans.Commands.CreateOneTimePaymentLoan;

public class CreateOneTimePaymentLoanCommandHandler : IRequestHandler<CreateOneTimePaymentLoanCommand, Guid>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILendingUnitOfWork _unitOfWork;

    public CreateOneTimePaymentLoanCommandHandler(ILoanRepository loanRepository, ILendingUnitOfWork unitOfWork)
    {
        _loanRepository = loanRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Guid> Handle(CreateOneTimePaymentLoanCommand command, CancellationToken cancellationToken)
    {
        // todo - validate ClientId exists

        var principal = new Money(command.Principal, command.Currency);
        var interest = new Money(command.Interest, command.Currency);

        var penaltyInterestRate = InterestRate.ForPenalty(command.PenaltyInterestRate, command.PenaltyInterestRatePeriod);

        var loan = Loan.CreateOneTime(
            clientId: command.ClientId,
            principal: principal,
            interest: interest,
            penaltyInterestRate: penaltyInterestRate,
            dueDate: command.DueDate,
            createdAt: DateTimeOffset.UtcNow);

        await _loanRepository.AddAsync(loan, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return loan.Id;
    }
}