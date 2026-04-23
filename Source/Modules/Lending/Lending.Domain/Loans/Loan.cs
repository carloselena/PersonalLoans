using Blocks.Domain.Abstractions;
using Blocks.Domain.Exceptions;
using Blocks.Domain.Guards;
using Blocks.Domain.Time;
using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans.Installments;
using Lending.Domain.Loans.Penalties;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.Domain.Loans;

public class Loan : AggregateRoot
{
    public Guid LenderId { get; private set; }
    public Guid ClientId { get; private set; }
    
    public Money Principal { get; private set; }
    public InterestRate InterestRate { get; private set; }
    public InterestRate PenaltyInterestRate { get; private set; }
    public Term Term { get; private set; }
    public PaymentFrequency PaymentFrequency { get; private set; }
    public LoanStatus Status { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DisbursedAt { get; private set; }
    
    private readonly List<Installment> _installments = [];
    public IReadOnlyCollection<Installment> Installments => _installments;

    private readonly List<Penalty> _penalties = [];
    public IReadOnlyCollection<Penalty> Penalties => _penalties;
    
    // OneTime Loans
    public Money? OneTimeInterest { get; private set; }
    public DateOnly? OneTimeDueDate { get; private set; }
    
    public Money TotalAmountOwed =>
        TotalInstallmentsOwed + TotalPenaltiesOwed;

    public Money TotalInstallmentsOwed =>
        _installments
            .Where(i => i.Status != InstallmentStatus.Paid)
            .Aggregate(
                Money.Zero(Principal.Currency),
                (total, installment) => total + installment.AmountOwed);
    
    public Money TotalPenaltiesOwed =>
        _penalties
            .Where(p => !p.IsPaid)
            .Aggregate(
                Money.Zero(Principal.Currency),
                (total, penalty) => total + penalty.AmountOwed);
        
    
    private Loan() { }

    public static Loan CreateNormal(
        Guid clientId,
        Money principal,
        InterestRate interestRate,
        InterestRate penaltyInterestRate,
        Term term,
        PaymentFrequency paymentFrequency,
        DateTimeOffset createdAt)
    {
        if (paymentFrequency == PaymentFrequency.OneTime)
            throw new BusinessRuleViolationException("Use CreateOneTime para préstamos de un solo pago");

        return new Loan
        {
            ClientId = clientId,
            Principal = principal,
            InterestRate = interestRate,
            PenaltyInterestRate = penaltyInterestRate,
            Term = term,
            PaymentFrequency = paymentFrequency,
            Status = LoanStatus.Draft,
            CreatedAt = createdAt,
            Id = Guid.CreateVersion7()
        };
    }

    public static Loan CreateOneTime(
        Guid clientId,
        Money principal,
        Money interest,
        InterestRate penaltyInterestRate,
        DateOnly dueDate,
        DateTimeOffset createdAt)
    {
        Guard.AgainstDifferentCurrencies(principal, interest);
        
        var loan = new Loan
        {
            ClientId = clientId,
            Principal = principal,
            OneTimeInterest = interest,
            InterestRate = InterestRate.Monthly(0),
            PenaltyInterestRate = penaltyInterestRate,
            Term = Term.Of(1),
            PaymentFrequency = PaymentFrequency.OneTime,
            Status = LoanStatus.Draft,
            OneTimeDueDate = dueDate,
            CreatedAt = createdAt,
            Id = Guid.CreateVersion7()
        };

        return loan;
    }

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
                var overdueAmount = installment.AmountOwed;

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
        => Money.FromDecimal(overdueAmount.Amount * dailyRate);
    
    #endregion
}