namespace KMFlow.Domain.Entities;

public class Department : AuditableEntity
{
    public Guid Id { get; set; }

    public string DeptName { get; set; } = string.Empty;

    public string DeptSlug { get; set; } = string.Empty;

    public virtual ICollection<User>? Users { get; set; }
}
