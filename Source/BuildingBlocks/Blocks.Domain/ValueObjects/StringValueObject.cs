namespace Blocks.Domain.ValueObjects;

public abstract record StringValueObject(string Value)
{
    public sealed override string ToString() => Value;

    public static implicit operator string(StringValueObject obj) => obj.Value;
}