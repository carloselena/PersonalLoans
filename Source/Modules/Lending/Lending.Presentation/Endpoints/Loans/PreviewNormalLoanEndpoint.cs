using Lending.Application.Features.Loans.Queries.PreviewNormalLoan;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Lending.Presentation.Endpoints.Loans;

public static class PreviewNormalLoanEndpoint
{
    public static IEndpointRouteBuilder MapPreviewNormalLoanEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("loans/normal/preview", async (
                PreviewNormalLoanQuery request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(request, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("PreviewNormalLoan")
            .WithTags("Loans")
            .Produces<LoanPreviewDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        return app;
    }
}