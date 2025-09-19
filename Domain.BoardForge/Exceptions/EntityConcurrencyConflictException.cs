using DevStack.Domain.BoardForge.Entities;

namespace DevStack.Domain.BoardForge.Exceptions;

public class EntityConcurrencyConflictException<TEntity> : Exception where TEntity : BaseEntity
{
    private static readonly string DefaultMessage = $"Concurrency conflict occurred for entity {typeof(TEntity).Name}. The entity may have been modified or deleted by another process.";
    TEntity? Entity { get; }

    public EntityConcurrencyConflictException() : base(DefaultMessage)
    {
    }

    public EntityConcurrencyConflictException(string message) : base(message)
    {
    }

    public EntityConcurrencyConflictException(string message, TEntity entity) : base(message)
    {
        Entity = entity;
    }

    public EntityConcurrencyConflictException(TEntity entity) : base(DefaultMessage)
    {
        Entity = entity;
    }
}

public class EntityConcurrencyConflictException : Exception
{
    private static readonly string DefaultMessage = "Concurrency conflict occurred. The entity may have been modified or deleted by another process.";

    public EntityConcurrencyConflictException() : base(DefaultMessage)
    {
    }

    public EntityConcurrencyConflictException(string message) : base(message)
    {
    }
}