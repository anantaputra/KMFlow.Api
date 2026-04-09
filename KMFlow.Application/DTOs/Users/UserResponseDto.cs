namespace KMFlow.Application.DTOs.Users;

public class UserResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string DeptName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
