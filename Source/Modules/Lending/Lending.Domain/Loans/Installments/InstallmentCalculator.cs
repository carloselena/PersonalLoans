using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans.Enums;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.Domain.Loans.Installments;

internal static class InstallmentCalculator
{
    internal static IReadOnlyList<InstallmentPreview> Calculate(
        Money principal,
        InterestRate interestRate,
        Term term,
        PaymentFrequency paymentFrequency,
        DateOnly disbursementDate)
    {
        var list = new List<InstallmentPreview>();
        
        var ratePerPeriod = interestRate.ToPeriod(paymentFrequency);

        var totalInterest =
            Math.Round(principal.Amount * ratePerPeriod * term.Value, 2);

        var principalPerInstallment =
            Math.Round(principal.Amount / term.Value, 2);

        var interestPerInstallment =
            Math.Round(totalInterest / term.Value, 2);

        var currentDate = disbursementDate;

        for (var i = 1; i <= term.Value; i++)
        {
            var principalAmount = i == term.Value
                ? principal.Amount - (principalPerInstallment * (term.Value - 1))
                : principalPerInstallment;

            var interestAmount = i == term.Value
                ? totalInterest - (interestPerInstallment * (term.Value - 1))
                : interestPerInstallment;

            var dueDate = AddPeriod(currentDate, paymentFrequency);

            var installment = new InstallmentPreview(
                i,
                AdjustIfSunday(dueDate),
                new Money(principalAmount + interestAmount, principal.Currency),
                new Money(principalAmount, principal.Currency),
                new Money(interestAmount, principal.Currency)
                );
            
            list.Add(installment);
            currentDate = dueDate;
        }

        return list;
    }
    private static DateOnly AddPeriod(DateOnly date, PaymentFrequency frequency)
    {
        return frequency switch
        {
            PaymentFrequency.Daily => date.AddDays(1),
            PaymentFrequency.Weekly => date.AddDays(7),
            PaymentFrequency.BiWeekly => date.AddDays(14),
            PaymentFrequency.Monthly => date.AddMonths(1),
            _ => throw new NotSupportedException("Frecuencia de pago no soportada")
        };
    }

    internal static DateOnly AdjustIfSunday(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Sunday
            ? date.AddDays(1)
            : date;
    }
}