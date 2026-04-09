using System.ComponentModel.DataAnnotations;

namespace KMFlow.Application.DTOs.Knowledges;

public class UpdateKnowledgeDto
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int Status { get; set; }
}
