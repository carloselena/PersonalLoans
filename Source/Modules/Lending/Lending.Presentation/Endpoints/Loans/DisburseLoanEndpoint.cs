using Lending.Application.Features.Loans.Commands.DisburseLoan;
using Lending.Application.Features.Loans.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lending.Presentation.Endpoints.Loans;

public static class DisburseLoanEndpoint
{
    public static IEndpointRouteBuilder MapDisburseLoanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("loans/{id:guid}/disburse", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var result = await sender.Send(new DisburseLoanCommand(id), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("DisburseLoan")
        .WithTags("Loans")
        .Produces<LoanDto>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        return app;
    }
}