namespace KMFlow.Application.DTOs.Departments;

public class DepartmentResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;
}
