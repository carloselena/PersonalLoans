namespace Blocks.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid LenderId { get; }
    bool IsAuthenticated { get; }
}