using Blocks.Domain.Exceptions;
using Blocks.Domain.Guards;
using Blocks.Domain.ValueObjects;

namespace Lending.Domain.Loans.Installments;

public class Installment
{
    public int Number { get; private set; }
    public DateOnly DueDate { get; private set; }
    
    public Money Amount { get; private set; }
    public Money Principal { get; private set; }
    public Money Interest { get; private set; }

    public Money AmountPaid { get; private set; }
    public Money AmountOwed => Amount - AmountPaid;
    
    public InstallmentStatus Status =>
        AmountPaid >= Amount ? InstallmentStatus.Paid : InstallmentStatus.Pending;
    
    private Installment() { }

    internal Installment(
        int number,
        DateOnly dueDate,
        Money principal,
        Money interest)
    {
        Number = number;
        DueDate = dueDate;
        Principal = principal;
        Interest = interest;
        Amount = new Money(principal.Amount + interest.Amount, principal.Currency);
        AmountPaid = Money.Zero(principal.Currency);
    }

    public bool IsOverdue(DateOnly today) =>
        Status == InstallmentStatus.Pending && DueDate < today;

    internal Money ApplyPayment(Money payment)
    {
        if (Status == InstallmentStatus.Paid)
            throw new BusinessRuleViolationException("No se puede pagar una cuota que ya está saldada");
        
        if (payment <= Money.Zero(Amount.Currency))
            throw new InvalidDomainValueException("El pago de la cuota debe ser mayor a cero");

        var applied = payment > AmountOwed ? AmountOwed : payment;
        AmountPaid += applied;
        
        return payment - applied;
    }
}