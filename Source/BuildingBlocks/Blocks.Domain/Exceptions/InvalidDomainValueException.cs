namespace Blocks.Domain.Exceptions;

public sealed class InvalidDomainValueException(string message) : DomainException(message);