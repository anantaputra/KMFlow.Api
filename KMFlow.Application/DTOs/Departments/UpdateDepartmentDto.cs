using System.ComponentModel.DataAnnotations;

namespace KMFlow.Application.DTOs.Departments;

public class UpdateDepartmentDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Slug { get; set; } = string.Empty;
}
