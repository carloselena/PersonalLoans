using Blocks.Domain.Enums;
using Blocks.Domain.Exceptions;
using Blocks.Domain.Time;
using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans;
using Lending.Domain.Loans.Enums;
using Lending.Domain.Loans.Installments;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.UnitTests.Domain.Loans;

public class LoanTests
{
    // ===== Fixture =====

    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Money _principal = new(1000m);
    private readonly InterestRate _interestRate = InterestRate.Annual(0.12m);
    private readonly InterestRate _penaltyInterestRate = InterestRate.Annual(0.30m);
    private readonly Term _term = Term.Of(12);
    
    private readonly PaymentFrequency _monthly = PaymentFrequency.Monthly;
    private readonly PaymentFrequency _weekly = PaymentFrequency.Weekly;
    private readonly PaymentFrequency _oneTime = PaymentFrequency.OneTime;
    
    private readonly LoanStatus _draft = LoanStatus.Draft;
    private readonly LoanStatus _active = LoanStatus.Active;
    private readonly LoanStatus _paidOff = LoanStatus.PaidOff;

    private readonly DateTimeOffset _createdAt = DateProvider.UtcNow();
    private readonly DateTimeOffset _disbursedAt = DateProvider.UtcNow().AddHours(1);

    private readonly DateOnly _dueDate = DateProvider.ToLocalDate(DateProvider.UtcNow()).AddDays(30);
    private readonly Money _interest = new(50m);
    private readonly DateOnly _sunday = new(2026, 3, 22);

    
    // ===== Create =====

    [Fact]
    public void CreateNormal_ShouldCreateLoan()
    {
        var loan = Loan.CreateNormal(_clientId, _principal, _interestRate, _penaltyInterestRate, _term, _monthly, _createdAt);

        Assert.Equal(_clientId, loan.ClientId);
        Assert.Equal(_principal, loan.Principal);
        Assert.Equal(_interestRate, loan.InterestRate);
        Assert.Equal(_penaltyInterestRate, loan.PenaltyInterestRate);
        Assert.Equal(_term, loan.Term);
        Assert.Equal(_monthly, loan.PaymentFrequency);
        Assert.Equal(_draft, loan.Status);
        Assert.Equal(_createdAt, loan.CreatedAt);
        Assert.Null(loan.DisbursedAt);
        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.Empty(loan.Installments);
    }

    [Fact]
    public void CreateNormal_WithOneTimeFrequency_ShouldThrow()
    {
        Assert.Throws<BusinessRuleViolationException>(() =>
            Loan.CreateNormal(_clientId, _principal, _interestRate, _penaltyInterestRate, _term, _oneTime, _createdAt));
    }

    [Fact]
    public void CreateOneTime_ShouldCreateLoan()
    {
        var loan = Loan.CreateOneTime(_clientId, _principal, _interest, _penaltyInterestRate, _dueDate, _createdAt);
        
        Assert.Equal(_clientId, loan.ClientId);
        Assert.Equal(_principal, loan.Principal);
        Assert.Equal(_interest, loan.OneTimeInterest);
        Assert.Equal(_dueDate, loan.OneTimeDueDate);
        Assert.Equal(Term.Of(1), loan.Term);
        Assert.Equal(_oneTime, loan.PaymentFrequency);
        Assert.Equal(_draft, loan.Status);
        Assert.Equal(InterestRate.Monthly(0), loan.InterestRate);
        Assert.Equal(_createdAt, loan.CreatedAt);
        Assert.Null(loan.DisbursedAt);
        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.Empty(loan.Installments);
    }
    
    [Fact]
    public void CreateOneTime_WithMismatchedCurrencies_ShouldThrow()
    {
        var usdInterest = new Money(50m, Currency.USD);
 
        Assert.Throws<BusinessRuleViolationException>(() =>
            Loan.CreateOneTime(_clientId, _principal, usdInterest, _penaltyInterestRate, _dueDate, _createdAt));
    }

    // ===== Disbursement =====

    [Fact]
    public void Disburse_ShouldGenerateInstallments()
    {
        var loan = CreateLoan();
        
        loan.Disburse(_disbursedAt);
        
        Assert.Equal(_disbursedAt, loan.DisbursedAt);
        Assert.NotEmpty(loan.Installments);
    }
    
    [Fact]
    public void Disburse_ShouldSetStatusToActive()
    {
        var loan = CreateLoan();
        
        loan.Disburse(_disbursedAt);
        
        Assert.Equal(_active, loan.Status);
    }

    [Fact]
    public void Disburse_Twice_ShouldThrow()
    {
        var loan = CreateLoan();
        loan.Disburse(_disbursedAt);

        Assert.Throws<BusinessRuleViolationException>(() => loan.Disburse(_disbursedAt));
    }

    // ===== Interest =====

    [Fact]
    public void InterestCalculation_ShouldBeCorrect()
    {
        var loan = CreateAndDisburseLoan(_monthly, 12);
        
        var totalInterest = loan.Installments.Sum(i => i.Interest.Amount);
        
        var expected = Math.Round(_principal.Amount * (0.12m / 12m) * 12m, 2);
        Assert.Equal(expected, totalInterest);
    }
    
    // ===== Installments =====

    [Fact]
    public void OneTime_ShouldGenerateSingleInstallment()
    {
        var loan = CreateAndDisburseOneTimeLoan();
        
        var i = loan.Installments.Single();
        Assert.Equal(1, i.Number);
        Assert.Equal(_principal, i.Principal);
        Assert.Equal(_interest, i.Interest);
        Assert.Equal(new Money(1050m), i.Amount);
    }

    [Fact]
    public void OneTime_SundayDueDate_ShouldMoveToMonday()
    {
        var loan = Loan.CreateOneTime(_clientId, _principal, _interest, _penaltyInterestRate, _sunday, _createdAt);
        
        loan.Disburse(_disbursedAt);
        
        Assert.Equal(DayOfWeek.Monday, loan.Installments.First().DueDate.DayOfWeek);
    }

    [Fact]
    public void Monthly_ShouldGenerateCorrectCount()
    {
        var loan = CreateAndDisburseLoan(_monthly, 12);
        
        Assert.Equal(12, loan.Installments.Count);
    }

    [Fact]
    public void Weekly_ShouldGenerateCorrectCount()
    {
        var loan = CreateAndDisburseLoan(_weekly, 6);
        
        Assert.Equal(6, loan.Installments.Count);
    }

    [Fact]
    public void LastInstallment_ShouldFixRounding()
    {
        var loan = CreateAndDisburseLoan(_monthly, 6);
        
        var total = loan.Installments.Sum(x => x.Principal.Amount);
        
        Assert.Equal(_principal.Amount, total);
    }

    [Fact]
    public void DueDates_ShouldBeCorrect()
    {
        var loan = CreateAndDisburseLoan(_monthly, 3);
        var installments = loan.Installments.ToList();

        var localDate = DateProvider.ToLocalDate(_disbursedAt);

        DateOnly Adjust(DateOnly date) =>
            date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(1) : date;
        
        Assert.Equal(Adjust(localDate.AddMonths(1)), installments[0].DueDate);
        Assert.Equal(Adjust(localDate.AddMonths(2)), installments[1].DueDate);
        Assert.Equal(Adjust(localDate.AddMonths(3)), installments[2].DueDate);
    }

    [Fact]
    public void NoInstallment_ShouldFallOnSunday()
    {
        var loan = CreateAndDisburseLoan(PaymentFrequency.Daily, 20);
        
        Assert.All(loan.Installments, i =>
            Assert.NotEqual(DayOfWeek.Sunday, i.DueDate.DayOfWeek));
    }
    
    // ===== Penalties =====

    [Fact]
    public void Penalty_ShouldNotCreate_WhenNotOverdue()
    {
        var loan = CreateAndDisburseLoan();
        
        loan.AccrueDailyPenalties(loan.Installments.First().DueDate);
        
        Assert.Empty(loan.Penalties);
    }

    [Fact]
    public void Penalty_ShouldCreate_WhenOverdue()
    {
        var loan = CreateAndDisburseLoan();
        var today = loan.Installments.First().DueDate.AddDays(1);
        
        loan.AccrueDailyPenalties(today);
        
        Assert.Single(loan.Penalties);
    }

    [Fact]
    public void Penalty_ShouldAccumulate()
    {
        var loan = CreateAndDisburseLoan();
        var dueDate = loan.Installments.First().DueDate;
        
        loan.AccrueDailyPenalties(dueDate.AddDays(1));
        loan.AccrueDailyPenalties(dueDate.AddDays(3));
        
        Assert.Equal(3, loan.Penalties.First().DaysAccrued);
    }

    [Fact]
    public void Penalty_ShouldNotDuplicateSameDay()
    {
        var loan = CreateAndDisburseLoan();
        var today = loan.Installments.First().DueDate.AddDays(1);
        
        loan.AccrueDailyPenalties(today);
        var firstAmount = loan.Penalties.First().Amount;

        loan.AccrueDailyPenalties(today);
        var secondAmount = loan.Penalties.First().Amount;
        
        Assert.Equal(firstAmount, secondAmount);
    }

    [Fact]
    public void Penalty_ShouldNotApply_WhenInstallmentPaid()
    {
        var loan = CreateAndDisburseLoan();
        var i = loan.Installments.First();
        i.ApplyPayment(i.Amount);
        
        loan.AccrueDailyPenalties(i.DueDate.AddDays(5));
        
        Assert.Empty(loan.Penalties);
    }

    [Fact]
    public void Penalty_ShouldWork_ForOneTimeLoan()
    {
        var loan = CreateAndDisburseOneTimeLoan();
        var today = loan.Installments.First().DueDate.AddDays(2);
        
        loan.AccrueDailyPenalties(today);
        
        Assert.Single(loan.Penalties);
    }
    
    [Fact]
    public void Penalty_MinimumAmount_ShouldBeOneUnit()
    {
        // Even a tiny principal should produce at least 1.00 penalty per day
        var tinyPrincipal = new Money(1m);
        var loan = Loan.CreateNormal(
            _clientId, tinyPrincipal, _interestRate, _penaltyInterestRate,
            Term.Of(1), _monthly, _createdAt);
        loan.Disburse(_disbursedAt);
 
        loan.AccrueDailyPenalties(loan.Installments.First().DueDate.AddDays(1));
 
        Assert.True(loan.Penalties.First().Amount.Amount >= 1m);
    }
    
    [Fact]
    public void Penalty_MultipleOverdueInstallments_ShouldCreateOnePenaltyEach()
    {
        var loan = CreateAndDisburseLoan(_monthly, 3);
        var lastDue = loan.Installments.Max(i => i.DueDate);
 
        loan.AccrueDailyPenalties(lastDue.AddDays(1));
 
        Assert.Equal(3, loan.Penalties.Count);
    }
    
    // ===== Status transitions =====
 
    [Fact]
    public void Status_ShouldBeDraft_BeforeDisbursement()
    {
        var loan = CreateLoan();
 
        Assert.Equal(_draft, loan.Status);
    }
 
    [Fact]
    public void Status_ShouldBeActive_AfterDisbursement()
    {
        var loan = CreateAndDisburseLoan();
 
        Assert.Equal(_active, loan.Status);
    }
 
    [Fact]
    public void Status_ShouldBePaidOff_WhenAllInstallmentsPaid()
    {
        var loan = CreateAndDisburseLoan(_monthly, 1);
        var installment = loan.Installments.Single();
 
        loan.ApplyPayment(installment.Amount);
 
        Assert.Equal(_paidOff, loan.Status);
    }
 
    [Fact]
    public void Status_ShouldRemainActive_WhenPartiallyPaid()
    {
        var loan = CreateAndDisburseLoan(_monthly, 2);
        var first = loan.Installments.OrderBy(i => i.Number).First();
 
        loan.ApplyPayment(first.Amount);
 
        Assert.Equal(_active, loan.Status);
    }
 
    [Fact]
    public void Status_ShouldBePaidOff_AfterPayingAllInstallmentsAndPenalties()
    {
        var loan = CreateAndDisburseLoan(_monthly, 1);
        var installment = loan.Installments.Single();
        var overdue = installment.DueDate.AddDays(3);
 
        loan.AccrueDailyPenalties(overdue);
 
        var total = loan.TotalAmountOwed;
        loan.ApplyPayment(total);
 
        Assert.Equal(_paidOff, loan.Status);
    }
    
    // ===== ApplyPayment =====
 
    [Fact]
    public void ApplyPayment_BeforeDisbursement_ShouldThrow()
    {
        var loan = CreateLoan();
 
        Assert.Throws<BusinessRuleViolationException>(() =>
            loan.ApplyPayment(new Money(100m)));
    }
 
    [Fact]
    public void ApplyPayment_WithZeroAmount_ShouldThrow()
    {
        var loan = CreateAndDisburseLoan();
 
        Assert.Throws<InvalidDomainValueException>(() =>
            loan.ApplyPayment(new Money(0m)));
    }
 
    [Fact]
    public void ApplyPayment_ExceedingTotalOwed_ShouldThrow()
    {
        var loan = CreateAndDisburseLoan(_monthly, 1);
        var owed = loan.TotalAmountOwed;
 
        Assert.Throws<BusinessRuleViolationException>(() =>
            loan.ApplyPayment(owed + new Money(0.01m)));
    }
 
    [Fact]
    public void ApplyPayment_ShouldReduceAmountOwed()
    {
        var loan = CreateAndDisburseLoan(_monthly, 1);
        var installment = loan.Installments.Single();
        var partial = new Money(installment.Amount.Amount / 2);
 
        loan.ApplyPayment(partial);
 
        Assert.True(loan.TotalInstallmentsOwed < installment.Amount);
    }
 
    [Fact]
    public void ApplyPayment_ShouldPayPenaltiesFirst_ThenInstallments()
    {
        var loan = CreateAndDisburseLoan(_monthly, 1);
        var installment = loan.Installments.Single();
        var overdue = installment.DueDate.AddDays(2);
 
        loan.AccrueDailyPenalties(overdue);
 
        var penaltyOwed = loan.TotalPenaltiesOwed;
        
        loan.ApplyPayment(penaltyOwed);
 
        Assert.Equal(0m, loan.TotalPenaltiesOwed.Amount);
        Assert.True(loan.TotalInstallmentsOwed > Money.Zero());
    }
 
    [Fact]
    public void ApplyPayment_OnPaidOffLoan_ShouldThrow()
    {
        var loan = CreateAndDisburseLoan(_monthly, 1);
        loan.ApplyPayment(loan.TotalAmountOwed);
 
        Assert.Throws<BusinessRuleViolationException>(() =>
            loan.ApplyPayment(new Money(1m)));
    }
 
    [Fact]
    public void ApplyPayment_ShouldApplyAcrossMultipleInstallmentsInOrder()
    {
        var loan = CreateAndDisburseLoan(_monthly, 3);
        var installments = loan.Installments.OrderBy(i => i.Number).ToList();
 
        // Pay exactly the first two installments
        var twoInstallmentsAmount = installments[0].Amount + installments[1].Amount;
        loan.ApplyPayment(twoInstallmentsAmount);
 
        Assert.Equal(InstallmentStatus.Paid, installments[0].Status);
        Assert.Equal(InstallmentStatus.Paid, installments[1].Status);
        Assert.Equal(InstallmentStatus.Pending, installments[2].Status);
    }

    // ===== Edge =====

    [Fact]
    public void Accrue_ShouldNotFail_WhenNotDisbursed()
    {
        var loan = CreateLoan();
        
        loan.AccrueDailyPenalties(DateProvider.Today());
        
        Assert.Empty(loan.Penalties);
    }

    // ===== Helpers =====

    private Loan CreateLoan()
    {
        return Loan.CreateNormal(
            _clientId,
            _principal,
            _interestRate,
            _penaltyInterestRate,
            _term,
            _monthly,
            _createdAt);
    }

    private Loan CreateAndDisburseLoan(
        PaymentFrequency frequency = PaymentFrequency.Monthly,
        int termValue = 3)
    {
        var loan = Loan.CreateNormal(
            _clientId,
            _principal,
            _interestRate,
            _penaltyInterestRate,
            Term.Of(termValue),
            frequency,
            _createdAt);

        loan.Disburse(_disbursedAt);
        return loan;
    }

    private Loan CreateAndDisburseOneTimeLoan()
    {
        var loan = Loan.CreateOneTime(
            _clientId,
            _principal,
            _interest,
            _penaltyInterestRate,
            _dueDate,
            _createdAt);

        loan.Disburse(_disbursedAt);
        return loan;
    }
}