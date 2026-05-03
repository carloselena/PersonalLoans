using Blocks.Domain.Exceptions;
using Blocks.Domain.ValueObjects;

namespace Blocks.Domain.Guards;

public static class Guard
{
    public static void AgainstNullOrWhiteSpace(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDomainValueException($"{propertyName} debe tener al menos un carácter");
    }
    
    public static void AgainstEmptyGuid(Guid value, string propertyName)
    {
        if (value == Guid.Empty)
            throw new InvalidDomainValueException($"{propertyName} no puede ser un Guid vacío");
    }
    public static void AgainstNegativeOrZeroInt(int value, string propertyName)
    {
        if (value <= 0)
            throw new InvalidDomainValueException($"{propertyName} debe ser mayor que cero");
    }
    public static void AgainstNegativeDecimal(decimal value, string propertyName)
    {
        if (value < 0)
            throw new InvalidDomainValueException($"{propertyName} no puede ser negativo");
    }

    public static void AgainstMoreThanTwoDecimals(decimal value, string propertyName)
    {
        if (value * 100 != decimal.Truncate(value * 100))
            throw new InvalidDomainValueException($"{propertyName} no puede tener más de dos decimales");
    }
    
    public static void AgainstDifferentCurrencies(Money amount1, Money amount2)
    {
        if (amount1.Currency != amount2.Currency)
            throw new BusinessRuleViolationException("Las monedas deben ser iguales");
    }
}