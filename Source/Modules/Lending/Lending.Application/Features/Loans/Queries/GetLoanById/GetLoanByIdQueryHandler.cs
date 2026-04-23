using Blocks.Application.Abstractions;
using Blocks.Application.Exceptions;
using Lending.Application.Features.Loans.Dtos;
using MediatR;

namespace Lending.Application.Features.Loans.Queries.GetLoanById;

public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanDto>
{
    private readonly ILoanQueries _queries;
    private readonly ICurrentUser _currentUser;

    public GetLoanByIdQueryHandler(ILoanQueries queries, ICurrentUser currentUser)
    {
        _queries = queries;
        _currentUser = currentUser;
    }
    public async Task<LoanDto> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
    {
        var loan = await _queries.GetByIdAsync(request.LoanId, _currentUser.LenderId, cancellationToken);

        return loan ?? throw new NotFoundException("Préstamo no encontrado");
    }
}