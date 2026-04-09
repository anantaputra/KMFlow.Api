using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KMFlow.Application.DTOs.Knowledges;

public class UpdateDraftKnowledgeRequestDto
{
    [Required]
    public string NamaFile { get; set; } = string.Empty;

    public IFormFile? Attachment { get; set; }

    public string? OwnerDepartment { get; set; }

    public string? Status { get; set; }
}
