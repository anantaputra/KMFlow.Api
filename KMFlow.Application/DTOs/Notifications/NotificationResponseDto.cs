namespace KMFlow.Application.DTOs.Notifications;

public class NotificationResponseDto
{
    public int NotificationId { get; set; }

    public Guid UserId { get; set; }

    public Guid? KnowledgeId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public Guid? RelatedActionBy { get; set; }

    public DateTime CreatedDate { get; set; }
}
