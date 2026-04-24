using Blocks.Domain.Exceptions;
using Lending.Domain.Loans;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.UnitTests.Domain.Loans.ValueObjects;

public class InterestRateTests
{
    // ===== Factory methods =====
 
    [Fact]
    public void Annual_ShouldCreate_WithCorrectPeriod()
    {
        var rate = InterestRate.Annual(0.12m);
 
        Assert.Equal(0.12m, rate.Rate);
        Assert.Equal(InterestRatePeriod.Annual, rate.Period);
    }
 
    [Fact]
    public void Monthly_ShouldCreate_WithCorrectPeriod()
    {
        var rate = InterestRate.Monthly(0.05m);
 
        Assert.Equal(0.05m, rate.Rate);
        Assert.Equal(InterestRatePeriod.Monthly, rate.Period);
    }
 
    [Fact]
    public void Weekly_ShouldCreate_WithCorrectPeriod()
    {
        var rate = InterestRate.Weekly(0.02m);
 
        Assert.Equal(0.02m, rate.Rate);
        Assert.Equal(InterestRatePeriod.Weekly, rate.Period);
    }
 
    [Fact]
    public void ZeroRate_ShouldBeAllowed_ForNormalRate()
    {
        var rate = InterestRate.Annual(0m);
 
        Assert.Equal(0m, rate.Rate);
    }
 
    [Fact]
    public void RateGreaterThanOne_ShouldThrow()
    {
        Assert.Throws<InvalidDomainValueException>(() => InterestRate.Annual(1.01m));
    }
 
    [Fact]
    public void RateEqualToOne_ShouldThrow()
    {
        Assert.Throws<InvalidDomainValueException>(() => InterestRate.Annual(1m));
    }
 
    [Fact]
    public void NegativeRate_ShouldThrow()
    {
        Assert.Throws<InvalidDomainValueException>(() => InterestRate.Annual(-0.01m));
    }
    
    // ===== ForPenalty =====
 
    [Fact]
    public void ForPenalty_ZeroRate_ShouldThrow()
    {
        Assert.Throws<InvalidDomainValueException>(() =>
            InterestRate.ForPenalty(0m, InterestRatePeriod.Annual));
    }
 
    [Fact]
    public void ForPenalty_ValidRate_ShouldCreate()
    {
        var rate = InterestRate.ForPenalty(0.30m, InterestRatePeriod.Annual);
 
        Assert.Equal(0.30m, rate.Rate);
    }
    
    // ===== ToDaily =====
 
    [Fact]
    public void ToDaily_Annual_ShouldDivideBy360()
    {
        var rate = InterestRate.Annual(0.36m);
 
        Assert.Equal(0.001m, rate.ToDaily());
    }
 
    [Fact]
    public void ToDaily_Monthly_ShouldDivideBy30()
    {
        var rate = InterestRate.Monthly(0.03m);
 
        Assert.Equal(0.001m, rate.ToDaily());
    }
 
    [Fact]
    public void ToDaily_Weekly_ShouldDivideBy7()
    {
        var rate = InterestRate.Weekly(0.07m);
 
        var expected = Math.Round(0.07m / 7m, 10);
        Assert.Equal(expected, Math.Round(rate.ToDaily(), 10));
    }
    
    // ===== ToPeriod =====
 
    public static IEnumerable<object[]> Data =>
    [
        [PaymentFrequency.Weekly,   0.07m, InterestRatePeriod.Weekly,  0.07m],
        [PaymentFrequency.Monthly,  0.03m, InterestRatePeriod.Monthly, 0.03m],
        [PaymentFrequency.Daily,    0.36m, InterestRatePeriod.Annual,  0.001m],
        [PaymentFrequency.Weekly,   0.36m, InterestRatePeriod.Annual,  0.007m],
        [PaymentFrequency.BiWeekly, 0.36m, InterestRatePeriod.Annual,  0.014m],
        [PaymentFrequency.Monthly,  0.36m, InterestRatePeriod.Annual,  0.030m]
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public void ToPeriod_ShouldConvertCorrectly(
        PaymentFrequency frequency,
        decimal inputRate,
        InterestRatePeriod period,
        decimal expectedRate)
    {
        var rate = period switch
        {
            InterestRatePeriod.Annual  => InterestRate.Annual(inputRate),
            InterestRatePeriod.Monthly => InterestRate.Monthly(inputRate),
            InterestRatePeriod.Weekly  => InterestRate.Weekly(inputRate),
            _ => throw new ArgumentOutOfRangeException()
        };
 
        var result = Math.Round(rate.ToPeriod(frequency), 6);
 
        Assert.Equal(expectedRate, result);
    }
}