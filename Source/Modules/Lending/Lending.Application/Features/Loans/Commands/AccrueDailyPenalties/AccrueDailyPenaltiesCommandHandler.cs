using Blocks.Domain.Time;
using Lending.Domain;
using Lending.Domain.Loans;
using MediatR;

namespace Lending.Application.Features.Loans.Commands.AccrueDailyPenalties;

public sealed class AccrueDailyPenaltiesCommandHandler : IRequestHandler<AccrueDailyPenaltiesCommand, AccrueDailyPenaltiesResult>
{
    private readonly ILoanRepository _loanRepository;
    private readonly ILendingUnitOfWork _unitOfWork;

    public AccrueDailyPenaltiesCommandHandler(ILoanRepository loanRepository, ILendingUnitOfWork unitOfWork)
    {
        _loanRepository = loanRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<AccrueDailyPenaltiesResult> Handle(AccrueDailyPenaltiesCommand command, CancellationToken cancellationToken)
    {
        var today = DateProvider.Today();

        var loans = await _loanRepository.GetOverdueLoansForPenaltyAccrualAsync(today, cancellationToken);

        var updatedLoans = 0;

        foreach (var loan in loans)
        {
            var changed= loan.AccrueDailyPenalties(today);

            if (changed)
                updatedLoans++;
        }

        if (updatedLoans > 0)
            await _unitOfWork.CommitAsync(cancellationToken);

        return new AccrueDailyPenaltiesResult(loans.Count, updatedLoans);
    }
}