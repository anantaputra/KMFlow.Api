using System.ComponentModel.DataAnnotations.Schema;

namespace KMFlow.Domain.Entities;

public class User : AuditableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Guid? DeptId { get; set; }

    public Guid? RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(DeptId))]
    public virtual Department? Department { get; set; }

    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }
}
