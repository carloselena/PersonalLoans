using Lending.Presentation.Endpoints.Loans;
using Microsoft.AspNetCore.Routing;

namespace Lending.Presentation;

public static class EndpointsRegistration
{
    public static IEndpointRouteBuilder MapAllLendingEndpoints(this IEndpointRouteBuilder app)
    {
        #region Loans
        app
            .MapCreateNormalLoanEndpoint()
            .MapCreateOneTimePaymentLoanEndpoint()
            .MapPreviewNormalLoanEndpoint()
            .MapGetAllLoansEndpoint()
            .MapGetLoanByIdEndpoint()
            ;
        #endregion
        
        return app;
    }
}