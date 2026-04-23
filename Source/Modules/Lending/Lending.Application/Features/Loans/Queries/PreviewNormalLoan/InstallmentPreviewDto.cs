namespace Lending.Application.Features.Loans.Queries.PreviewNormalLoan;

public record InstallmentPreviewDto(
    int Number,
    DateOnly DueDate,
    decimal Amount,
    decimal Principal,
    decimal Interest);