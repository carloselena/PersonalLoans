using Lending.Application.Features.Loans.Commands.CreateOneTimePaymentLoan;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lending.Presentation.Endpoints.Loans;

public static class CreateOneTimePaymentLoanEndpoint
{
    public static IEndpointRouteBuilder MapCreateOneTimePaymentLoanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("loans/one-time", async (
            [FromBody] CreateOneTimePaymentLoanCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var loanId = await sender.Send(command, cancellationToken);
            return Results.Created($"/loans/one-time/{loanId}", loanId);
        })
        .WithName("CreateOneTimePaymentLoan")
        .WithTags("Loans")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound) // ClientId
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        return app;
    }
}