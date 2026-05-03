namespace Blocks.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException() : base("No autorizado")
    {
        
    }

    public UnauthorizedException(string message) : base(message)
    {
        
    }
}