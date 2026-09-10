using System;
using System.Collections.Generic;

namespace ClinicSystem.Domain.Common;

public abstract class BaseEntity : ISoftDeletable
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAtUtc { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void UpdateModifiedTime()
    {
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdateModifiedTime();
    }

    public void UndoDelete()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        UpdateModifiedTime();
    }
}

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
