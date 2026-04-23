using Blocks.Domain.Enums;

namespace Lending.Application.Features.Loans.Dtos;

public record PenaltyDto(
    Guid Id,
    int InstallmentNumber,
    
    decimal Amount,
    decimal AmountPaid,
    decimal AmountOwed,
    
    Currency Currency,
    
    DateOnly AppliedAt,
    DateOnly LastAppliedAt,
    
    int DaysAccrued,
    bool IsPaid);