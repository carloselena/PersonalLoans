using Blocks.Domain.Enums;
using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans;
using Lending.Domain.Loans.Installments;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.UnitTests.Domain.Loans.Installments;

public class InstallmentCalculatorTests
{
    private static readonly Money _principal = new(1200m);
    private static readonly InterestRate _monthlyRate = InterestRate.Monthly(0.03m);
    private static readonly Term _term12 = Term.Of(12);
    private static readonly DateOnly _disbursementDate = new(2026, 1, 15);
 
    [Theory]
    [InlineData(PaymentFrequency.Monthly,  6,  6)]
    [InlineData(PaymentFrequency.Weekly,   4,  4)]
    [InlineData(PaymentFrequency.BiWeekly, 3,  3)]
    [InlineData(PaymentFrequency.Daily,    5,  5)]
    public void Calculate_ShouldReturnCorrectCount(
        PaymentFrequency frequency, int termValue, int expectedCount)
    {
        var result = InstallmentCalculator.Calculate(
            _principal, _monthlyRate, Term.Of(termValue), frequency, _disbursementDate);
 
        Assert.Equal(expectedCount, result.Count);
    }
 
    [Fact]
    public void Calculate_PrincipalSum_ShouldEqualTotalPrincipal()
    {
        var result = InstallmentCalculator.Calculate(
            _principal, _monthlyRate, _term12, PaymentFrequency.Monthly, _disbursementDate);
 
        var sum = result.Sum(i => i.Principal.Amount);
        Assert.Equal(_principal.Amount, sum);
    }
 
    [Fact]
    public void Calculate_LastInstallment_ShouldAbsorbRoundingDifference()
    {
        var principal = new Money(1000m);
        var result = InstallmentCalculator.Calculate(
            principal, _monthlyRate, Term.Of(3), PaymentFrequency.Monthly, _disbursementDate);
 
        var sum = result.Sum(i => i.Principal.Amount);
        Assert.Equal(1000m, sum);
    }
 
    [Fact]
    public void Calculate_Amount_ShouldEqualPrincipalPlusInterest()
    {
        var result = InstallmentCalculator.Calculate(
            _principal, _monthlyRate, _term12, PaymentFrequency.Monthly, _disbursementDate);
 
        foreach (var i in result)
            Assert.Equal(i.Principal.Amount + i.Interest.Amount, i.Amount.Amount);
    }
 
    [Fact]
    public void Calculate_InterestSum_ShouldMatchFormula()
    {
        // total interest = principal * rate_per_period * term
        var ratePerPeriod = _monthlyRate.ToPeriod(PaymentFrequency.Monthly);
        var expected = Math.Round(_principal.Amount * ratePerPeriod * _term12.Value, 2);
 
        var result = InstallmentCalculator.Calculate(
            _principal, _monthlyRate, _term12, PaymentFrequency.Monthly, _disbursementDate);
 
        var totalInterest = result.Sum(i => i.Interest.Amount);
        Assert.Equal(expected, totalInterest);
    }
 
    [Fact]
    public void Calculate_Weekly_DueDates_ShouldBeSevenDaysApart_WhenNoSunday()
    {
        var wednesday = new DateOnly(2026, 1, 7);
        var result = InstallmentCalculator.Calculate(
            _principal, _monthlyRate, Term.Of(3), PaymentFrequency.Weekly, wednesday).ToList();
 
        for (var i = 1; i < result.Count; i++)
        {
            var diff = result[i].DueDate.DayNumber - result[i - 1].DueDate.DayNumber;
            Assert.Equal(7, diff);
        }
    }
 
    [Fact]
    public void AdjustIfSunday_Sunday_ShouldReturnMonday()
    {
        var sunday = new DateOnly(2026, 3, 22);
 
        var adjusted = InstallmentCalculator.AdjustIfSunday(sunday);
 
        Assert.Equal(DayOfWeek.Monday, adjusted.DayOfWeek);
    }
 
    [Fact]
    public void AdjustIfSunday_NonSunday_ShouldReturnSameDate()
    {
        var monday = new DateOnly(2026, 3, 23);
 
        var adjusted = InstallmentCalculator.AdjustIfSunday(monday);
 
        Assert.Equal(monday, adjusted);
    }
 
    [Fact]
    public void Calculate_NoDueDate_ShouldFallOnSunday()
    {
        var thursday = new DateOnly(2026, 1, 1);
        var result = InstallmentCalculator.Calculate(
            _principal, _monthlyRate, Term.Of(50), PaymentFrequency.Daily, thursday);
 
        Assert.All(result, i => Assert.NotEqual(DayOfWeek.Sunday, i.DueDate.DayOfWeek));
    }
 
    [Fact]
    public void Calculate_InstallmentNumbers_ShouldStartAtOneAndBeSequential()
    {
        var result = InstallmentCalculator.Calculate(
            _principal, _monthlyRate, Term.Of(5), PaymentFrequency.Monthly, _disbursementDate).ToList();
 
        for (var i = 0; i < result.Count; i++)
            Assert.Equal(i + 1, result[i].Number);
    }
 
    [Fact]
    public void Calculate_ShouldPreserveCurrency()
    {
        var usdPrincipal = new Money(1000m, Currency.USD);
        var result = InstallmentCalculator.Calculate(
            usdPrincipal, _monthlyRate, Term.Of(3), PaymentFrequency.Monthly, _disbursementDate);
 
        Assert.All(result, i =>
        {
            Assert.Equal(Currency.USD, i.Amount.Currency);
            Assert.Equal(Currency.USD, i.Principal.Currency);
            Assert.Equal(Currency.USD, i.Interest.Currency);
        });
    }
}