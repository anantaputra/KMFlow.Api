using System.ComponentModel.DataAnnotations;

namespace KMFlow.Application.DTOs.Users;

public class CreateUserDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Guid RoleId { get; set; }

    [Required]
    public Guid DeptId { get; set; }
}
