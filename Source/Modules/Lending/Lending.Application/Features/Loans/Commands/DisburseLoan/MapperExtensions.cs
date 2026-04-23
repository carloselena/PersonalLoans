using Lending.Application.Features.Loans.Dtos;
using Lending.Domain.Loans;

namespace Lending.Application.Features.Loans.Commands.DisburseLoan;

public static class MapperExtensions
{
    public static LoanDto ToDto(this Loan loan)
    {
        return new LoanDto(
            loan.Id,
            loan.LenderId,
            loan.ClientId,
            (decimal)loan.Principal,
            loan.Principal.Currency,
            loan.InterestRate.Rate,
            loan.InterestRate.Period,
            loan.PenaltyInterestRate.Rate,
            loan.PenaltyInterestRate.Period,
            loan.Term.Value,
            loan.PaymentFrequency,
            loan.Status,
            loan.OneTimeInterest == null ? null : (decimal)loan.OneTimeInterest,
            loan.CreatedAt,
            loan.DisbursedAt,
            loan.Installments.OrderBy(i => i.Number).Select(i => new InstallmentDto(
                i.Number,
                i.DueDate,
                (decimal)i.Amount,
                (decimal)i.Principal,
                (decimal)i.Interest,
                (decimal)i.AmountPaid,
                (decimal)i.AmountOwed,
                i.Principal.Currency,
                i.Status
                )).ToList(),
            []
        );
    }
}