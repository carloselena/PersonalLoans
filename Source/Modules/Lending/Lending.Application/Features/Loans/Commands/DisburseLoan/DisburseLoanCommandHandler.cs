using Blocks.Application.Abstractions;
using Blocks.Application.Exceptions;
using Blocks.Domain.Abstractions;
using Blocks.Domain.Time;
using Lending.Application.Features.Loans.Dtos;
using Lending.Application.Features.Loans.Queries;
using Lending.Domain.Loans;
using MediatR;

namespace Lending.Application.Features.Loans.Commands.DisburseLoan;

public class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, LoanDto>
{
    private readonly ILoanRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DisburseLoanCommandHandler(ILoanRepository repository, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }
    public async Task<LoanDto> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await _repository.GetByIdAsync(request.LoanId, _currentUser.LenderId, cancellationToken);
        if (loan is null)
            throw new NotFoundException("Préstamo no encontrado");

        loan.Disburse(DateProvider.UtcNow());

        await _unitOfWork.CommitAsync(cancellationToken);
        return loan.ToDto();
    }
}