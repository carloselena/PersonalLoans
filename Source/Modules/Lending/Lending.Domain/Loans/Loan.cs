using Blocks.Domain.Abstractions;
using Blocks.Domain.Exceptions;
using Blocks.Domain.Guards;
using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans.Installments;
using Lending.Domain.Loans.Penalties;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.Domain.Loans;

public partial class Loan : AggregateRoot
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
}