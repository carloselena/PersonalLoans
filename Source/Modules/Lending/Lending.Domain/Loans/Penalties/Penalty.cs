using Blocks.Domain.Exceptions;
using Blocks.Domain.Guards;
using Blocks.Domain.ValueObjects;

namespace Lending.Domain.Loans.Penalties;

public class Penalty
{
    public Guid Id { get; private set; }
    public int InstallmentNumber { get; private set; }
    public Money Amount { get; private set; }
    public Money AmountPaid { get; private set; }
    public DateOnly AppliedAt { get; private set; }
    public DateOnly LastAppliedAt { get; private set; }
    
    public Money AmountOwed => Amount - AmountPaid;
    public bool IsPaid => AmountOwed.Amount == 0;
    public int DaysAccrued => LastAppliedAt.DayNumber - AppliedAt.DayNumber + 1;
    
    private Penalty() { }

    private Penalty(int installmentNumber, Money amount, DateOnly appliedAt)
    {
        Guard.AgainstNegativeOrZeroInt(installmentNumber, "cuota");

        if (amount <= Money.Zero(amount.Currency))
            throw new InvalidDomainValueException("La mora debe ser mayor a cero");
        
        InstallmentNumber = installmentNumber;
        Amount = amount;
        AmountPaid = Money.Zero(amount.Currency);
        AppliedAt = appliedAt;
        LastAppliedAt = appliedAt;
        Id = Guid.CreateVersion7();
    }

    internal static Penalty Create(int installmentNumber, Money amount, DateOnly appliedAt)
    {
        return new Penalty(installmentNumber, amount, appliedAt);
    }

    internal void Accrue(Money dailyAmount, DateOnly today)
    {
        if (today <= LastAppliedAt)
            throw new BusinessRuleViolationException("No se puede acarrear una mora que ya se acarrió");

        if (dailyAmount <= Money.Zero(Amount.Currency))
            throw new InvalidDomainValueException("La mora debe ser mayor a cero");

        Amount += dailyAmount;
        LastAppliedAt = today;
    }

    internal Money ApplyPayment(Money payment)
    {
        if (IsPaid)
            throw new BusinessRuleViolationException("No se puede pagar una mora que ya está saldada");
        
        if (payment <= Money.Zero(Amount.Currency))
            throw new InvalidDomainValueException("El pago debe ser mayor a cero");
        
        var applied = payment > AmountOwed ? AmountOwed : payment;
        AmountPaid += applied;

        return payment - applied;
    }
}