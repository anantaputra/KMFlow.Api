namespace KMFlow.Application.DTOs.Knowledges;

public class KnowledgeResponseDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string OwnerDepartment { get; set; } = string.Empty;

    public string PublishedBy { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? PublishedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class KnowledgeStatsResponseDto
{
    public int TotalKnowledge { get; set; }

    public int MyContribution { get; set; }

    public int PendingReview { get; set; }
}
