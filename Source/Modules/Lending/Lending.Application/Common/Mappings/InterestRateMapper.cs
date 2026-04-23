using Lending.Domain.Loans.ValueObjects;

namespace Lending.Application.Common.Mappings;

internal static class InterestRateMapper
{
    public static InterestRate ToNormalRate(decimal rate, InterestRatePeriod period)
        => period switch
        {
            InterestRatePeriod.Annual => InterestRate.Annual(rate),
            InterestRatePeriod.Monthly => InterestRate.Monthly(rate),
            InterestRatePeriod.Weekly => InterestRate.Weekly(rate),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
}