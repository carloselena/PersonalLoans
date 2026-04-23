using Blocks.Domain.Enums;
using Blocks.Domain.Guards;

namespace Blocks.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }
    
    private Money() { }

    public Money(decimal amount, Currency currency = Currency.DOP)
    {
        Guard.AgainstNegativeDecimal(amount, "monto");
        Guard.AgainstMoreThanTwoDecimals(amount, "monto");

        Amount = amount;
        Currency = currency;
    }
    
    public static Money FromDecimal(decimal amount)
    {
        var rounded = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        return new Money(rounded);
    }
    
    public static Money Zero(Currency currency = Currency.DOP) 
        => new(0, currency);
    
    public static explicit operator decimal(Money money)
    {
        return money.Amount;
    }

    public static Money operator +(Money a, Money b)
    {
        Guard.AgainstDifferentCurrencies(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }
    
    public static Money operator -(Money a, Money b)
    {
        Guard.AgainstDifferentCurrencies(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money money, decimal factor)
    {
        return new Money(money.Amount * factor, money.Currency);
    }

    public static Money operator /(Money money, decimal divisor)
    {
        return new Money(money.Amount / divisor, money.Currency);
    }

    public static bool operator >(Money a, Money b)
    {
        Guard.AgainstDifferentCurrencies(a, b);
        return a.Amount > b.Amount;
    }
    
    public static bool operator <(Money a, Money b)
    {
        Guard.AgainstDifferentCurrencies(a, b);
        return a.Amount < b.Amount;
    }

    public static bool operator >=(Money a, Money b)
    {
        Guard.AgainstDifferentCurrencies(a, b);
        return a.Amount >= b.Amount;
    }
    
    public static bool operator <=(Money a, Money b)
    {
        Guard.AgainstDifferentCurrencies(a, b);
        return a.Amount <= b.Amount;
    }
}