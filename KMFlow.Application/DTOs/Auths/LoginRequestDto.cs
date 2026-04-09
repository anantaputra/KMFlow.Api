using System.ComponentModel.DataAnnotations;

namespace KMFlow.Application.DTOs.Auths;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [StringLength(100, ErrorMessage = "Email length must be less than or equal to 100 characters.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
