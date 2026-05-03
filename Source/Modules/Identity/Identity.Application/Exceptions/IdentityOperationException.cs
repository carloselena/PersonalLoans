namespace Identity.Application.Exceptions;

public class IdentityOperationException : Exception
{
    public IReadOnlyCollection<string> Errors { get; }

    public IdentityOperationException(IEnumerable<string> errors)
        : this(errors.ToArray()) { }
    
    private IdentityOperationException(string[] errors)
        : base(string.Join(", ", errors))
    {
        Errors = errors;
    }
}