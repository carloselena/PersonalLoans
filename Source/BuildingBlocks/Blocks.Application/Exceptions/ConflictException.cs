namespace Blocks.Application.Exceptions;

public class ConflictException : Exception
{
    public ConflictException() : base("Conflicto con el recurso existente") { }
    public ConflictException(string message) : base(message) { }
}