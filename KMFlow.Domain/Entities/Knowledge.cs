using System.ComponentModel.DataAnnotations.Schema;

namespace KMFlow.Domain.Entities;

public class Knowledge : AuditableEntity
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public Guid OwnerDept { get; set; }

    public int Status { get; set; }

    public Guid SubmittedBy { get; set; }

    public DateTime SubmittedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    [ForeignKey(nameof(OwnerDept))]
    public virtual Department? Department { get; set; }

    [ForeignKey(nameof(SubmittedBy))]
    public virtual User? User { get; set; }

    public virtual ICollection<KnowledgeHistory>? History { get; set; }
}
