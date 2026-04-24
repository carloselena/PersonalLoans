using Blocks.Domain.Exceptions;
using Blocks.Domain.Time;
using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.Tests.Domain.Loans;

public class LoanTests
{
    // ===== Arrange =====

    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Money _principal = new(1000m);
    private readonly InterestRate _interestRate = InterestRate.Annual(0.12m);
    private readonly InterestRate _penaltyInterestRate = InterestRate.Annual(0.30m);
    private readonly Term _term = Term.Of(12);
    private readonly PaymentFrequency _monthly = PaymentFrequency.Monthly;
    private readonly PaymentFrequency _weekly = PaymentFrequency.Weekly;
    private readonly PaymentFrequency _oneTime = PaymentFrequency.OneTime;

    private readonly DateTimeOffset _createdAt = DateProvider.UtcNow();
    private readonly DateTimeOffset _disbursedAt = DateProvider.UtcNow().AddHours(1);

    private readonly DateOnly _dueDate = DateProvider.ToLocalDate(DateProvider.UtcNow()).AddDays(30);
    private readonly Money _interest = new(50m);
    private readonly DateOnly _sunday = new(2026, 3, 22);

    // ===== Create =====

    [Fact]
    public void CreateNormal_ShouldCreateLoan()
    {
        // Act
        var loan = Loan.CreateNormal(_clientId, _principal, _interestRate, _penaltyInterestRate, _term, _monthly, _createdAt);

        // Assert
        Assert.Equal(_clientId, loan.ClientId);
        Assert.Equal(_principal, loan.Principal);
        Assert.Equal(_interestRate, loan.InterestRate);
        Assert.Equal(_penaltyInterestRate, loan.PenaltyInterestRate);
        Assert.Equal(_term, loan.Term);
        Assert.Equal(_monthly, loan.PaymentFrequency);
        Assert.Equal(_createdAt, loan.CreatedAt);
        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.Null(loan.DisbursedAt);
        Assert.Empty(loan.Installments);
    }

    [Fact]
    public void CreateNormal_WithOneTimeFrequency_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            Loan.CreateNormal(_clientId, _principal, _interestRate, _penaltyInterestRate, _term, _oneTime, _createdAt));
    }

    [Fact]
    public void CreateOneTime_ShouldCreateLoan()
    {
        // Act
        var loan = Loan.CreateOneTime(_clientId, _principal, _interest, _penaltyInterestRate, _dueDate, _createdAt);

        // Assert
        Assert.Equal(_clientId, loan.ClientId);
        Assert.Equal(_principal, loan.Principal);
        Assert.Equal(_interest, loan.OneTimeInterest);
        Assert.Equal(_dueDate, loan.OneTimeDueDate);
        Assert.Equal(_oneTime, loan.PaymentFrequency);
        Assert.Equal(Term.Of(1), loan.Term);
        Assert.Equal(InterestRate.Monthly(0), loan.InterestRate);
        Assert.NotEqual(Guid.Empty, loan.Id);
    }

    // ===== Disbursement =====

    [Fact]
    public void Disburse_ShouldGenerateInstallments()
    {
        // Arrange
        var loan = CreateLoan();

        // Act
        loan.Disburse(_disbursedAt);

        // Assert
        Assert.Equal(_disbursedAt, loan.DisbursedAt);
        Assert.NotEmpty(loan.Installments);
    }

    [Fact]
    public void Disburse_Twice_ShouldThrow()
    {
        // Arrange
        var loan = CreateLoan();
        loan.Disburse(_disbursedAt);

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() => loan.Disburse(_disbursedAt));
    }

    // ===== Installments =====

    [Fact]
    public void OneTime_ShouldGenerateSingleInstallment()
    {
        // Arrange
        var loan = CreateAndDisburseOneTimeLoan();

        // Assert
        var i = loan.Installments.Single();
        Assert.Equal(1, i.Number);
        Assert.Equal(_principal, i.Principal);
        Assert.Equal(_interest, i.Interest);
        Assert.Equal(new Money(1050m), i.Amount);
    }

    [Fact]
    public void OneTime_SundayDueDate_ShouldMoveToMonday()
    {
        // Arrange
        var loan = Loan.CreateOneTime(_clientId, _principal, _interest, _penaltyInterestRate, _sunday, _createdAt);

        // Act
        loan.Disburse(_disbursedAt);

        // Assert
        Assert.Equal(DayOfWeek.Monday, loan.Installments.First().DueDate.DayOfWeek);
    }

    [Fact]
    public void Monthly_ShouldGenerateCorrectCount()
    {
        // Arrange
        var loan = CreateAndDisburseLoan(_monthly, 12);

        // Assert
        Assert.Equal(12, loan.Installments.Count);
    }

    [Fact]
    public void Weekly_ShouldGenerateCorrectCount()
    {
        // Arrange
        var loan = CreateAndDisburseLoan(_weekly, 6);

        // Assert
        Assert.Equal(6, loan.Installments.Count);
    }

    [Fact]
    public void LastInstallment_ShouldFixRounding()
    {
        // Arrange
        var loan = CreateAndDisburseLoan(_monthly, 6);

        // Act
        var total = loan.Installments.Sum(x => x.Principal.Amount);

        // Assert
        Assert.Equal(_principal.Amount, total);
    }

    [Fact]
    public void DueDates_ShouldBeCorrect()
    {
        // Arrange
        var loan = CreateAndDisburseLoan(_monthly, 3);
        var installments = loan.Installments.ToList();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");
        var local = TimeZoneInfo.ConvertTime(_disbursedAt, tz);
        var baseDate = DateOnly.FromDateTime(local.DateTime);

        DateOnly Adjust(DateOnly d) =>
            d.DayOfWeek == DayOfWeek.Sunday ? d.AddDays(1) : d;

        // Assert
        Assert.Equal(Adjust(baseDate.AddMonths(1)), installments[0].DueDate);
        Assert.Equal(Adjust(baseDate.AddMonths(2)), installments[1].DueDate);
        Assert.Equal(Adjust(baseDate.AddMonths(3)), installments[2].DueDate);
    }

    [Fact]
    public void NoInstallment_ShouldFallOnSunday()
    {
        // Arrange
        var loan = CreateAndDisburseLoan(PaymentFrequency.Daily, 20);

        // Assert
        Assert.All(loan.Installments, i =>
            Assert.NotEqual(DayOfWeek.Sunday, i.DueDate.DayOfWeek));
    }

    // ===== Interest =====

    [Fact]
    public void InterestCalculation_ShouldBeCorrect()
    {
        // Arrange
        var loan = Loan.CreateNormal(
            _clientId,
            new Money(12000m),
            _interestRate,
            _penaltyInterestRate,
            _term,
            _monthly,
            _createdAt);

        loan.Disburse(_disbursedAt);

        // Act
        var totalInterest = loan.Installments.Sum(i => i.Interest.Amount);

        // Assert
        var expected = Math.Round(12000m * (0.12m / 12m) * 12m, 2);
        Assert.Equal(expected, totalInterest);
    }

    // ===== Penalties =====

    [Fact]
    public void Penalty_ShouldNotCreate_WhenNotOverdue()
    {
        // Arrange
        var loan = CreateAndDisburseLoan();

        // Act
        loan.AccrueDailyPenalties(loan.Installments.First().DueDate);

        // Assert
        Assert.Empty(loan.Penalties);
    }

    [Fact]
    public void Penalty_ShouldCreate_WhenOverdue()
    {
        // Arrange
        var loan = CreateAndDisburseLoan();
        var today = loan.Installments.First().DueDate.AddDays(1);

        // Act
        loan.AccrueDailyPenalties(today);

        // Assert
        Assert.Single(loan.Penalties);
    }

    [Fact]
    public void Penalty_ShouldAccumulate()
    {
        // Arrange
        var loan = CreateAndDisburseLoan();
        var due = loan.Installments.First().DueDate;

        // Act
        loan.AccrueDailyPenalties(due.AddDays(1));
        loan.AccrueDailyPenalties(due.AddDays(3));

        // Assert
        Assert.True(loan.Penalties.First().DaysAccrued >= 3);
    }

    [Fact]
    public void Penalty_ShouldNotDuplicateSameDay()
    {
        // Arrange
        var loan = CreateAndDisburseLoan();
        var today = loan.Installments.First().DueDate.AddDays(1);

        // Act
        loan.AccrueDailyPenalties(today);
        var first = loan.Penalties.First().Amount;

        loan.AccrueDailyPenalties(today);
        var second = loan.Penalties.First().Amount;

        // Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Penalty_ShouldNotApply_WhenInstallmentPaid()
    {
        // Arrange
        var loan = CreateAndDisburseLoan();
        var i = loan.Installments.First();
        i.ApplyPayment(i.Amount);

        // Act
        loan.AccrueDailyPenalties(i.DueDate.AddDays(5));

        // Assert
        Assert.Empty(loan.Penalties);
    }

    [Fact]
    public void Penalty_ShouldWork_ForOneTimeLoan()
    {
        // Arrange
        var loan = CreateAndDisburseOneTimeLoan();
        var today = loan.Installments.First().DueDate.AddDays(2);

        // Act
        loan.AccrueDailyPenalties(today);

        // Assert
        Assert.Single(loan.Penalties);
    }

    // ===== Edge =====

    [Fact]
    public void Accrue_ShouldNotFail_WhenNotDisbursed()
    {
        // Arrange
        var loan = CreateLoan();

        // Act
        loan.AccrueDailyPenalties(DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
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