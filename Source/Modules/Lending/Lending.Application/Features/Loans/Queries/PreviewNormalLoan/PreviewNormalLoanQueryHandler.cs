using Blocks.Domain.ValueObjects;
using Lending.Application.Common.Mappings;
using Lending.Domain.Loans;
using Lending.Domain.Loans.ValueObjects;
using MediatR;

namespace Lending.Application.Features.Loans.Queries.PreviewNormalLoan;

public class PreviewNormalLoanQueryHandler : IRequestHandler<PreviewNormalLoanQuery, LoanPreviewDto>
{
    public Task<LoanPreviewDto> Handle(PreviewNormalLoanQuery request, CancellationToken cancellationToken)
    {
        var principal = new Money(request.Principal, request.Currency);
        var interestRate = InterestRateMapper.ToNormalRate(request.InterestRate, request.InterestRatePeriod);

        var term = Term.Of(request.Term);
        
        var installmentsPreview = 
            Loan.PreviewInstallments(
                principal,
                interestRate,
                term,
                request.PaymentFrequency,
                request.DisbursementDate);

        var installments = installmentsPreview
            .Select(i => new InstallmentPreviewDto(
                i.Number,
                i.DueDate,
                (decimal)i.Amount,
                (decimal)i.Principal,
                (decimal)i.Interest))
            .ToList();

        var totalInterest = installments.Sum(i => i.Interest);
        var totalAmount = installments.Sum(i => i.Amount);

        var loanPreviewDto = new LoanPreviewDto(
            request.Principal,
            totalInterest,
            totalAmount,
            request.Term,
            request.PaymentFrequency,
            request.Currency,
            installments);

        return Task.FromResult(loanPreviewDto);
    }
}