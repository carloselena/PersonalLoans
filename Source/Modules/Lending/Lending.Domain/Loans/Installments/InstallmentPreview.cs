using Blocks.Domain.ValueObjects;

namespace Lending.Domain.Loans.Installments;

public record InstallmentPreview(
    int Number,
    DateOnly DueDate,
    Money Amount,
    Money Principal,
    Money Interest
);