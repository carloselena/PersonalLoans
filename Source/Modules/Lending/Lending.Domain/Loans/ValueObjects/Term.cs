using Blocks.Domain.Guards;

namespace Lending.Domain.Loans.ValueObjects;

public sealed record Term
{
    public int Value { get; }
    
    private Term(int value)
    {
        Guard.AgainstNegativeOrZeroInt(value, "cuota");
        Value = value;
    }
    
    public static Term Of(int value) => new(value);
}