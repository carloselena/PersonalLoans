using Lending.Application.Features.Loans.Commands;
using Lending.Application.Features.Loans.Commands.CreateNormalLoan;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Lending.Presentation.Endpoints.Loans;

public static class CreateNormalLoanEndpoint
{
    public static IEndpointRouteBuilder MapCreateNormalLoanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/loans/normal", async (
            [FromBody] CreateNormalLoanCommand command,
            ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            var loanId = await sender.Send(command, cancellationToken);
            return Results.Created($"/loans/normal/{loanId}", loanId);
        })
        .WithName("CreateNormalLoan")
        .WithTags("Loans")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound) // ClientId
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }
}