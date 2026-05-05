using Blocks.Application.Exceptions;
using Blocks.Domain.Abstractions;

namespace Blocks.EntityFramework;

public static class RepositoryExtensions
{
    public static async Task<TEntity> FindByIdOrThrowAsync<TEntity>(
        this IGenericRepository<TEntity> repository, Guid id, string propertyName,
        CancellationToken cancellationToken = default)
        where TEntity : class, IAggregateRoot
    {
        var entity = await repository.FindByIdAsync(id, cancellationToken);
        return entity ?? throw new NotFoundException($"No se encontró {propertyName} con id {id}");
    }
}