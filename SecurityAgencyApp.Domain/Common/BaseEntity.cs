namespace SecurityAgencyApp.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    /// <summary>User who created the record (enterprise audit).</summary>
    public Guid? CreatedBy { get; set; }
    /// <summary>User who last modified the record (enterprise audit).</summary>
    public Guid? ModifiedBy { get; set; }
}
