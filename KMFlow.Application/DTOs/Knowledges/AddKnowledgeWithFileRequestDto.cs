using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KMFlow.Application.DTOs.Knowledges;

public class AddKnowledgeWithFileRequestDto
{
    [Required]
    public string NamaFile { get; set; } = string.Empty;

    [Required]
    public IFormFile Attachment { get; set; } = default!;

    public string? OwnerDepartment { get; set; }

    public string? Status { get; set; }
}
