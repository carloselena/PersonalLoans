using Blocks.Domain.Exceptions;
using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans.Penalties;

namespace Lending.Tests.Domain.Loans.Penalties;

public class PenaltyTests
{
    // ===== Create =====

    [Fact]
    public void Create_ShouldInitializeCorrectly()
    {
        // Arrange
        var amount = new Money(100);
        var date = new DateOnly(2026, 1, 1);

        // Act
        var penalty = Penalty.Create(1, amount, date);

        // Assert
        Assert.Equal(1, penalty.InstallmentNumber);
        Assert.Equal(100, penalty.Amount.Amount);
        Assert.Equal(0, penalty.AmountPaid.Amount);
        Assert.Equal(date, penalty.AppliedAt);
        Assert.Equal(date, penalty.LastAppliedAt);
        Assert.Equal(100, penalty.AmountOwed.Amount);
        Assert.False(penalty.IsPaid);
        Assert.Equal(1, penalty.DaysAccrued);
        Assert.NotEqual(Guid.Empty, penalty.Id);
    }

    [Fact]
    public void Create_ShouldThrow_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var date = new DateOnly(2026, 1, 1);

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            Penalty.Create(1, new Money(0), date));

        Assert.Throws<ArgumentException>(() =>
            Penalty.Create(1, new Money(-10), date));
    }

    // ===== Accrue =====

    [Fact]
    public void Accrue_ShouldIncreaseAmount_AndUpdateLastAppliedAt()
    {
        // Arrange
        var penalty = CreatePenalty();
        var today = new DateOnly(2026, 1, 2);

        // Act
        penalty.Accrue(new Money(50), today);

        // Assert
        Assert.Equal(150, penalty.Amount.Amount);
        Assert.Equal(today, penalty.LastAppliedAt);
        Assert.Equal(2, penalty.DaysAccrued);
    }

    [Fact]
    public void Accrue_ShouldThrow_WhenDateIsNotGreaterThanLastAppliedAt()
    {
        // Arrange
        var penalty = CreatePenalty();
        var sameDay = new DateOnly(2026, 1, 1);

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            penalty.Accrue(new Money(50), sameDay));
    }

    // ===== ApplyPayment =====

    [Fact]
    public void ApplyPayment_ShouldReduceAmountOwed()
    {
        // Arrange
        var penalty = CreatePenalty();

        // Act
        var remaining = penalty.ApplyPayment(new Money(40));

        // Assert
        Assert.Equal(40, penalty.AmountPaid.Amount);
        Assert.Equal(60, penalty.AmountOwed.Amount);
        Assert.Equal(0, remaining.Amount);
    }

    [Fact]
    public void ApplyPayment_ShouldFullyPay_WhenExactAmount()
    {
        // Arrange
        var penalty = CreatePenalty();

        // Act
        var remaining = penalty.ApplyPayment(new Money(100));

        // Assert
        Assert.True(penalty.IsPaid);
        Assert.Equal(0, penalty.AmountOwed.Amount);
        Assert.Equal(0, remaining.Amount);
    }

    [Fact]
    public void ApplyPayment_ShouldReturnRemaining_WhenOverpaid()
    {
        // Arrange
        var penalty = CreatePenalty();

        // Act
        var remaining = penalty.ApplyPayment(new Money(150));

        // Assert
        Assert.True(penalty.IsPaid);
        Assert.Equal(0, penalty.AmountOwed.Amount);
        Assert.Equal(50, remaining.Amount);
    }

    [Fact]
    public void ApplyPayment_ShouldAccumulatePayments()
    {
        // Arrange
        var penalty = CreatePenalty();

        // Act
        penalty.ApplyPayment(new Money(30));
        penalty.ApplyPayment(new Money(20));

        // Assert
        Assert.Equal(50, penalty.AmountPaid.Amount);
        Assert.Equal(50, penalty.AmountOwed.Amount);
    }

    [Fact]
    public void ApplyPayment_ShouldThrow_WhenAlreadyPaid()
    {
        // Arrange
        var penalty = CreatePenalty();
        penalty.ApplyPayment(new Money(100));

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            penalty.ApplyPayment(new Money(10)));
    }

    // ===== Helpers =====

    private static Penalty CreatePenalty()
    {
        return Penalty.Create(
            1,
            new Money(100),
            new DateOnly(2026, 1, 1));
    }
}