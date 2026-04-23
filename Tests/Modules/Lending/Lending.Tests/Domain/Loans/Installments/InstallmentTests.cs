using Blocks.Domain.Enums;
using Blocks.Domain.Exceptions;
using Blocks.Domain.ValueObjects;
using Lending.Domain.Loans.Installments;

namespace Lending.Tests.Domain.Loans.Installments;

public class InstallmentTests
{

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange
        var principal = new Money(1000);
        var interest = new Money(200);
        var dueDate = new DateOnly(2026, 1, 1);

        // Act
        var installment = new Installment(1, dueDate, principal, interest);

        // Assert
        Assert.Equal(1, installment.Number);
        Assert.Equal(dueDate, installment.DueDate);
        Assert.Equal(1000, installment.Principal.Amount);
        Assert.Equal(200, installment.Interest.Amount);
        Assert.Equal(1200, installment.Amount.Amount);
        Assert.Equal(0, installment.AmountPaid.Amount);
        Assert.Equal(1200, installment.AmountOwed.Amount);
        Assert.Equal(InstallmentStatus.Pending, installment.Status);
    }

    [Fact]
    public void Status_ShouldBePending_WhenNotFullyPaid()
    {
        // Arrange
        var installment = CreateInstallment();

        // Act
        installment.ApplyPayment(new Money(500));

        // Assert
        Assert.Equal(InstallmentStatus.Pending, installment.Status);
    }

    [Fact]
    public void Status_ShouldBePaid_WhenFullyPaid()
    {
        // Arrange
        var installment = CreateInstallment();

        // Act
        installment.ApplyPayment(new Money(1200));

        // Assert
        Assert.Equal(InstallmentStatus.Paid, installment.Status);
    }

    [Fact]
    public void Status_ShouldBePaid_WhenOverpaid()
    {
        // Arrange
        var installment = CreateInstallment();

        // Act
        installment.ApplyPayment(new Money(1500));

        // Assert
        Assert.Equal(InstallmentStatus.Paid, installment.Status);
    }

    [Fact]
    public void AmountOwed_ShouldDecrease_WhenPaymentApplied()
    {
        // Arrange
        var installment = CreateInstallment();

        // Act
        installment.ApplyPayment(new Money(300));

        // Assert
        Assert.Equal(900, installment.AmountOwed.Amount);
    }

    [Fact]
    public void AmountOwed_ShouldBeZero_WhenFullyPaid()
    {
        // Arrange
        var installment = CreateInstallment();

        // Act
        installment.ApplyPayment(new Money(1200));

        // Assert
        Assert.Equal(0, installment.AmountOwed.Amount);
    }

    [Fact]
    public void IsOverdue_ShouldReturnTrue_WhenPendingAndPastDueDate()
    {
        // Arrange
        var installment = CreateInstallment(dueDate: new DateOnly(2026, 1, 1));
        var today = new DateOnly(2026, 1, 2);

        // Act
        var result = installment.IsOverdue(today);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOverdue_ShouldReturnFalse_WhenPaid()
    {
        // Arrange
        var installment = CreateInstallment(dueDate: new DateOnly(2026, 1, 1));
        installment.ApplyPayment(new Money(1200));
        var today = new DateOnly(2026, 1, 2);

        // Act
        var result = installment.IsOverdue(today);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOverdue_ShouldReturnFalse_WhenNotDueYet()
    {
        // Arrange
        var installment = CreateInstallment(dueDate: new DateOnly(2026, 1, 2));
        var today = new DateOnly(2026, 1, 1);

        // Act
        var result = installment.IsOverdue(today);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ApplyPayment_ShouldIncreaseAmountPaid()
    {
        // Arrange
        var installment = CreateInstallment();

        // Act
        installment.ApplyPayment(new Money(300));

        // Assert
        Assert.Equal(300, installment.AmountPaid.Amount);
    }

    [Fact]
    public void ApplyPayment_ShouldAccumulatePayments()
    {
        // Arrange
        var installment = CreateInstallment();

        // Act
        installment.ApplyPayment(new Money(300));
        installment.ApplyPayment(new Money(200));

        // Assert
        Assert.Equal(500, installment.AmountPaid.Amount);
    }

    [Fact]
    public void ApplyPayment_ShouldThrow_WhenDifferentCurrency()
    {
        // Arrange
        var installment = CreateInstallment();

        // Act & Assert
        Assert.ThrowsAny<DomainException>(() =>
            installment.ApplyPayment(new Money(100, Currency.USD)));
    }

    private static Installment CreateInstallment(DateOnly? dueDate = null)
    {
        return new Installment(
            1,
            dueDate ?? new DateOnly(2026, 1, 1),
            new Money(1000),
            new Money(200));
    }
}
