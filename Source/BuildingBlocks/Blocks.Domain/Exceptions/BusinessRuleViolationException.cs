namespace Blocks.Domain.Exceptions;

public sealed class BusinessRuleViolationException(string message) : DomainException(message);