using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSystem.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

        public DateTime? LastModifiedAtUtc { get;private set; }


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

        protected void UpdateModifiedTime()
        {
            LastModifiedAtUtc = DateTime.UtcNow;
        }

    }


    public interface IDomainEvent
    {
        DateTime OccurredOnUtc { get; }
    }
}
