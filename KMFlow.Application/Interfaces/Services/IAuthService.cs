using KMFlow.Application.DTOs.Auths;

namespace KMFlow.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);

    Task<LoginResponseDto?> RefreshAsync(RefreshTokenRequestDto dto);

    Task<UserResponseDto?> GetMeAsync(Guid userId);
}
