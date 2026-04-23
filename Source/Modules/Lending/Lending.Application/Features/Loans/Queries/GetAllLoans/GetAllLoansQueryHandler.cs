using Blocks.Application.Abstractions;
using MediatR;

namespace Lending.Application.Features.Loans.Queries.GetAllLoans;

public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, List<LoanListDto>>
{
    private readonly ILoanQueries _queries;
    private readonly ICurrentUser _currentUser;

    public GetAllLoansQueryHandler(ILoanQueries queries, ICurrentUser currentUser)
    {
        _queries = queries;
        _currentUser = currentUser;
    }
    
    public async Task<List<LoanListDto>> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
    {
        return await _queries.GetAllAsync(_currentUser.LenderId, request, cancellationToken);
    }
}