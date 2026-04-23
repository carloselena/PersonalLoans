namespace Blocks.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() : base("Recurso no encontrado")
    {
        
    }

    public NotFoundException(string message) : base(message)
    {
        
    }
}