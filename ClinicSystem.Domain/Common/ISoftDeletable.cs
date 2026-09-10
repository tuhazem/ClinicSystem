using System;

namespace ClinicSystem.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAtUtc { get; }
    void SoftDelete();
    void UndoDelete();
}
