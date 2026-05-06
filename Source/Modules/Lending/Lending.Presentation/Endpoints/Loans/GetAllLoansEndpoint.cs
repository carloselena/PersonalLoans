using Lending.Application.Features.Loans.Queries.GetAllLoans;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lending.Presentation.Endpoints.Loans;

public static class GetAllLoansEndpoint
{
    public static IEndpointRouteBuilder MapGetAllLoansEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("loans/", async (
            [AsParameters] GetAllLoansQuery request,
            ISender sender,
            CancellationToken cancellationToken
            ) =>
        {
            var result = await sender.Send(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAllLoans")
        .WithTags("Loans")
        .Produces<List<LoanListDto>>()
        .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        return app;
    }
}