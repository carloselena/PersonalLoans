using Blocks.Domain.Enums;
using Lending.Domain.Loans;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.Application.Features.Loans.Dtos;

public record LoanDto(
    Guid Id,
    Guid LenderId,
    Guid ClientId,
    
    decimal Principal,
    Currency Currency,
    
    decimal InterestRate,
    InterestRatePeriod InterestRatePeriod,
    
    decimal PenaltyInterestRate,
    InterestRatePeriod PenaltyInterestRatePeriod,
    
    int Term,
    PaymentFrequency PaymentFrequency,
    LoanStatus Status,
    
    decimal? OneTimeInterest,
    
    DateTimeOffset CreatedAt,
    DateTimeOffset? DisbursedAt,
    
    IReadOnlyCollection<InstallmentDto> Installments,
    IReadOnlyCollection<PenaltyDto> Penalties);