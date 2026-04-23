using Blocks.Domain.Enums;
using Lending.Domain.Loans;

namespace Lending.Application.Features.Loans.Queries.PreviewNormalLoan;

public record LoanPreviewDto(
    decimal Principal,
    decimal TotalInterest,
    decimal TotalAmount,
    int Term,
    PaymentFrequency PaymentFrequency,
    Currency Currency,
    IReadOnlyCollection<InstallmentPreviewDto> Installments);