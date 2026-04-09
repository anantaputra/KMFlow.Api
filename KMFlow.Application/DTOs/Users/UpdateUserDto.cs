using System.ComponentModel.DataAnnotations;

namespace KMFlow.Application.DTOs.Users;

public class UpdateUserDto
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    [Required]
    public Guid DeptId { get; set; }
}
