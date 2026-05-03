using Blocks.Domain.Abstractions;
using Blocks.Domain.ValueObjects;
using Lending.Application.Common.Mappings;
using Lending.Domain;
using Lending.Domain.Loans;
using Lending.Domain.Loans.ValueObjects;
using MediatR;

namespace Lending.Application.Features.Loans.Commands.CreateNormalLoan;

public class CreateNormalLoanCommandHandler : IRequestHandler<CreateNormalLoanCommand, Guid>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILendingUnitOfWork _unitOfWork;

    public CreateNormalLoanCommandHandler(ILoanRepository loanRepository, ILendingUnitOfWork unitOfWork)
    {
        _loanRepository = loanRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Guid> Handle(CreateNormalLoanCommand command, CancellationToken cancellationToken)
    {
        // todo - validate ClientId exists
        
        var principal = new Money(command.Principal, command.Currency);

        var interestRate = InterestRateMapper.ToNormalRate(command.InterestRate, command.InterestRatePeriod);

        var penaltyInterestRate = InterestRate.ForPenalty(command.PenaltyInterestRate, command.PenaltyInterestRatePeriod);

        var loan = Loan.CreateNormal(
            command.ClientId,
            principal,
            interestRate,
            penaltyInterestRate,
            Term.Of(command.Term),
            command.PaymentFrequency,
            DateTimeOffset.UtcNow);

        await _loanRepository.AddAsync(loan, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        
        return loan.Id;
    }
}