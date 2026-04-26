using Blocks.Domain.Exceptions;
using Blocks.Domain.Time;
using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans.Installments;
using Lending.Domain.Loans.Penalties;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.Domain.Loans;

public partial class Loan
{
    public void Disburse(DateTimeOffset disbursedAt)
    {
        if (DisbursedAt.HasValue)
            throw new BusinessRuleViolationException("El préstamo ya se desembolsó");

        DisbursedAt = disbursedAt;

        var disbursementDate = DateProvider.ToLocalDate(disbursedAt);
        GenerateInstallments(disbursementDate);
        
        UpdateStatus();
    }

    public static IReadOnlyCollection<InstallmentPreview> PreviewInstallments(
        Money principal,
        InterestRate interestRate,
        Term term,
        PaymentFrequency paymentFrequency,
        DateOnly disbursementDate)
    {
        if (paymentFrequency == PaymentFrequency.OneTime)
            throw new BusinessRuleViolationException("Use PreviewOneTimeInstallment para préstamos de un solo pago");

        return InstallmentCalculator.Calculate(
            principal,
            interestRate,
            term,
            paymentFrequency,
            disbursementDate);
    }

    public void ApplyPayment(Money payment)
    {
        if (DisbursedAt is null)
            throw new BusinessRuleViolationException("No se puede pagar un préstamo que no ha sido desembolsado");
        
        if (Status is LoanStatus.PaidOff)
            throw new BusinessRuleViolationException("No se puede pagar un préstamo que ya fue saldado");
        
        if (payment <= Money.Zero(Principal.Currency))
            throw new InvalidDomainValueException("El pago debe ser mayor a cero");

        if (payment > TotalAmountOwed)
            throw new BusinessRuleViolationException("El pago no puede exceder el saldo pendiente");

        var remaining = payment;

        foreach (var penalty in _penalties
                     .Where(p => !p.IsPaid)
                    .OrderBy(p => p.LastAppliedAt)
                     .ThenBy(p => p.InstallmentNumber))
        {
            if (remaining <= Money.Zero(remaining.Currency))
                return;

            remaining = penalty.ApplyPayment(remaining);
        }

        foreach (var installement in _installments
                     .Where(i => i.Status != InstallmentStatus.Paid)
                     .OrderBy(i => i.Number))
        {
            if (remaining <= Money.Zero(remaining.Currency))
                return;
            
            remaining = installement.ApplyPayment(remaining);
        }
        
        UpdateStatus();
    }
    public bool AccrueDailyPenalties(DateOnly today)
    {
        var changed = false;
        var dailyRate = PenaltyInterestRate.ToDaily();

        foreach (var installment in _installments)
        {
            if (!installment.IsOverdue(today))
                continue;

            var penalty = GetPenaltyForInstallment(installment.Number);

            var startDate = penalty?.LastAppliedAt.AddDays(1)
                            ?? installment.DueDate.AddDays(1);

            for (var date = startDate; date <= today; date = date.AddDays(1))
            {
                var overdueAmount = PaymentFrequency == PaymentFrequency.OneTime
                    ? installment.Principal - (installment.AmountPaid - installment.InterestPaid)
                    : installment.AmountOwed;

                if (overdueAmount <= Money.Zero(overdueAmount.Currency))
                    break;

                var penaltyAmount = CalculateDailyPenalty(overdueAmount, dailyRate);

                if (penalty is null)
                {
                    var newPenalty = Penalty.Create(
                        installment.Number,
                        penaltyAmount,
                        date);
                
                    _penalties.Add(newPenalty);
                    penalty = newPenalty;
                }
                else
                {
                    penalty.Accrue(penaltyAmount, date);
                }
                
                changed = true;
            }
        }
        
        if (changed)
            UpdateStatus();

        return changed;
    }
    
    #region Private Methods
    private void GenerateInstallments(DateOnly disbursementDate)
    {
        if (!DisbursedAt.HasValue)
            throw new BusinessRuleViolationException("El préstamo debe ser desembolsado primero para generar las cuotas");
        
        _installments.Clear();

        if (PaymentFrequency == PaymentFrequency.OneTime)
        {
            if (!OneTimeDueDate.HasValue)
                throw new BusinessRuleViolationException("Debe proveerse la fecha de pago para préstamos de un solo pago");

            var installment = new Installment(
                number: 1,
                dueDate: InstallmentCalculator.AdjustIfSunday(OneTimeDueDate.Value),
                principal: Principal,
                interest: OneTimeInterest ?? new Money(0, Principal.Currency)
            );
            
            _installments.Add(installment);
            return;
        }

        var calculatedInstallments = InstallmentCalculator.Calculate(
            principal: Principal,
            interestRate: InterestRate,
            term: Term,
            paymentFrequency: PaymentFrequency,
            disbursementDate: disbursementDate
        );

        foreach (var calculatedInstallment in calculatedInstallments)
        {
            var installment = new Installment(
                number: calculatedInstallment.Number,
                dueDate: calculatedInstallment.DueDate,
                principal: calculatedInstallment.Principal,
                interest: calculatedInstallment.Interest
            );
            
            _installments.Add(installment);
        }
    }

    private void UpdateStatus()
    {
        if (DisbursedAt is null)
        {
            Status = LoanStatus.Draft;
            return;
        }

        var totalOwed = TotalInstallmentsOwed + TotalPenaltiesOwed;
        
        Status = totalOwed.Amount == 0
            ? LoanStatus.PaidOff
            : LoanStatus.Active;
    }
    
    private Penalty? GetPenaltyForInstallment(int installmentNumber)
        => _penalties.FirstOrDefault(p => p.InstallmentNumber == installmentNumber);

    private static Money CalculateDailyPenalty(Money overdueAmount, decimal dailyRate)
    {
        var amount = Math.Round((decimal)overdueAmount * dailyRate, 2, MidpointRounding.AwayFromZero);
        const decimal minimum = 1.00m;
        return new Money(Math.Max(amount, minimum), overdueAmount.Currency);
    }
    
    #endregion
}