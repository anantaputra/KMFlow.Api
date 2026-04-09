using System.ComponentModel.DataAnnotations.Schema;

namespace KMFlow.Domain.Entities;

public class KnowledgeHistory
{
    public Guid Id { get; set; }

    public Guid KnowledgeId { get; set; }

    public int Action { get; set; }

    public int FromStatus { get; set; }

    public int ToStatus { get; set; }

    public Guid ActionBy { get; set; }

    public DateTime ActionAt { get; set; }

    [ForeignKey(nameof(KnowledgeId))]
    public virtual Knowledge? Knowledge { get; set; }

    public virtual User? User { get; set; }
}
