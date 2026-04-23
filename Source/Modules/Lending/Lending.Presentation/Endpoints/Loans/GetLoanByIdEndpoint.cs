using Lending.Application.Features.Loans.Dtos;
using Lending.Application.Features.Loans.Queries.GetLoanById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lending.Presentation.Endpoints.Loans;

public static class GetLoanByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetLoanByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("loans/{id:guid}", async (
            GetLoanByIdQuery request,
            ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await sender.Send(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetLoanById")
        .WithTags("Loans")
        .Produces<LoanDto>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        return app;
    }
}