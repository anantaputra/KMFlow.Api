namespace KMFlow.Application.DTOs.Auths;

public class LoginResponseDto
{
    public bool Status { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiredAt { get; set; }

    public UserResponseDto User { get; set; }
}

public class UserResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string DeptName { get; set; } = string.Empty;
}
