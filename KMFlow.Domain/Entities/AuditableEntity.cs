namespace KMFlow.Domain.Entities;

public abstract class AuditableEntity
{
    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

}
