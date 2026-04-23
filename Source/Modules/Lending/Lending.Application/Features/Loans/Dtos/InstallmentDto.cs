using Blocks.Domain.Enums;
using Lending.Domain.Loans.Installments;

namespace Lending.Application.Features.Loans.Dtos;

public record InstallmentDto(
    int Number,
    DateOnly DueDate,
    
    decimal Amount,
    decimal Principal,
    decimal Interest,
    
    decimal AmountPaid,
    decimal AmountOwed,
    
    Currency Currency,
    InstallmentStatus Status);