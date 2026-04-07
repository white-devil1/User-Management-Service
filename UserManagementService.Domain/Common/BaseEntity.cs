namespace UserManagementService.Domain.Common;

public abstract class BaseEntity<TKey> : IEntity<TKey>
{
    public TKey Id { get; set; } = default!;

    // Audit Fields - Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Audit Fields - User Tracking
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public abstract class BaseEntity : BaseEntity<Guid>, IEntity { }