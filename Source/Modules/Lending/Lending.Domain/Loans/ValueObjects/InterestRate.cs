using Blocks.Domain.Exceptions;
using Blocks.Domain.Guards;

namespace Lending.Domain.Loans.ValueObjects;

public enum InterestRatePeriod
{
    Weekly,
    Monthly,
    Annual
}

public sealed record InterestRate
{
    public decimal Rate { get; }
    public InterestRatePeriod Period { get; }

    private InterestRate(decimal rate, InterestRatePeriod period)
    {
        Guard.AgainstNegativeDecimal(rate, "tasa");
        
        // todo - considerar moverlo a un guard
        if (rate >= 1)
            throw new InvalidDomainValueException("La tasa de interés debe ser menor a 1 (ejemplo: 0.20 para 20%");

        Rate = rate;
        Period = period;
    }

    public static InterestRate Annual(decimal rate)
        => new(rate, InterestRatePeriod.Annual);
    
    public static InterestRate Monthly(decimal rate)
        => new(rate, InterestRatePeriod.Monthly);
    
    public static InterestRate Weekly(decimal rate)
        => new(rate, InterestRatePeriod.Weekly);
    
    public static InterestRate ForPenalty(decimal rate, InterestRatePeriod period)
    {
        return rate switch
        {
            0 => throw new InvalidDomainValueException("La tasa de interés debe ser mayor a 0"),
            > 1 => throw new InvalidDomainValueException("La tasa de interés debe ser menor a 1 (ejemplo: 0.20 para 20%)"),
            _ => new InterestRate(rate, period)
        };
    }
    
    public decimal ToPeriod(PaymentFrequency frequency)
    {
        if (
            (Period == InterestRatePeriod.Weekly && frequency == PaymentFrequency.Weekly) ||
            (Period == InterestRatePeriod.Monthly && frequency == PaymentFrequency.Monthly)
        ) return Rate;

        decimal dailyRate = ToDaily();

        return frequency switch
        {
            PaymentFrequency.Daily => dailyRate,
            PaymentFrequency.Weekly => dailyRate * 7,
            PaymentFrequency.BiWeekly => dailyRate * 14,
            PaymentFrequency.Monthly => dailyRate * 30,
            PaymentFrequency.OneTime => dailyRate, // todo - pendiente
            _ => throw new InvalidDomainValueException("Frecuencia de pago no soportada")
        };
    }

    public decimal ToDaily()
    {
        return Period switch
        {
            InterestRatePeriod.Annual => Rate / 360,
            InterestRatePeriod.Monthly => Rate / 30,
            InterestRatePeriod.Weekly => Rate / 7,
            _ => throw new InvalidDomainValueException("Período de tasa de interés no soportado")
        };
    }
}