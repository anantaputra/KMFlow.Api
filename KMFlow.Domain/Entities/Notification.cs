using KMFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace KMFlow.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }

    public Guid UserId { get; set; }

    public Guid? KnowledgeId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public Guid? RelatedActionBy { get; set; }

    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [ForeignKey(nameof(KnowledgeId))]
    public virtual Knowledge? Knowledge { get; set; }

    [ForeignKey(nameof(RelatedActionBy))]
    public virtual User? RelatedActionUser { get; set; }
}
