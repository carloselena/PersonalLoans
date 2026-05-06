using Blocks.Domain.Enums;
using Lending.Domain.Loans;
using Lending.Domain.Loans.Enums;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.Application.Features.Loans.Queries.GetAllLoans;

public record LoanListDto(
    Guid Id,
    Guid ClientId,
    Guid LenderId,
    
    PaymentFrequency PaymentFrequency,
    
    decimal Principal,
    Currency Currency,
    
    decimal InterestRate,
    InterestRatePeriod InterestRatePeriod,
    
    decimal PenaltyInterestRate,
    InterestRatePeriod PenaltyInterestRatePeriod,
    
    int Term,
    
    DateOnly? FirstDueDate,
    DateOnly? LastDueDate,
    
    int PaidInstallmentsCount,
    int PendingInstallmentsCount,

    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalOwed,
    
    decimal TotalPenaltyAmount,
    decimal TotalPenaltyOwed,
    
    bool IsDisbursed,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DisbursedAt
);