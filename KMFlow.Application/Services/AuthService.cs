using KMFlow.Application.DTOs.Auths;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Application.Interfaces.Services;
using KMFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KMFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly PasswordHasher<User> _hasher;
    private readonly IConfiguration _config;

    public AuthService(
        IAuthRepository authRepository,
        IConfiguration config
    )
    {
        _authRepository = authRepository;
        _hasher = new PasswordHasher<User>();
        _config = config;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var user = await _authRepository.GetUserWithLoginDetailByEmail(dto.Email);
        if (user == null) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result != PasswordVerificationResult.Success) return null;

        var (token, expiresAt) = GenerateJwtToken(user);
        return BuildLoginResponse(user, token, expiresAt, "Login Successful");
    }

    public async Task<LoginResponseDto?> RefreshAsync(RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken)) return null;

        var principal = GetPrincipalFromToken(dto.RefreshToken);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return null;

        var user = await _authRepository.GetUserWithLoginDetailById(userId);
        if (user == null) return null;
        if (!user.IsActive) return null;

        var (token, expiresAt) = GenerateJwtToken(user);
        return BuildLoginResponse(user, token, expiresAt, "Refresh Successful");
    }

    public async Task<UserResponseDto?> GetMeAsync(Guid userId)
    {
        var user = await _authRepository.GetUserWithLoginDetailById(userId);
        if (user == null) return null;
        if (!user.IsActive) return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            RoleName = user.Role?.RoleName ?? string.Empty,
            DeptName = user.Department?.DeptName ?? string.Empty,
        };
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyString)) return null;

        var key = Encoding.ASCII.GetBytes(keyString);

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                },
                out var validatedToken
            );

            if (validatedToken is not JwtSecurityToken jwtToken) return null;
            if (!string.Equals(jwtToken.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal)) return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyString)) throw new InvalidOperationException("Jwt:Key is missing");

        var expiresMinutesString = _config["Jwt:ExpiresMinutes"];
        if (!int.TryParse(expiresMinutesString, out var expiresMinutes)) expiresMinutes = 60;

        var key = Encoding.ASCII.GetBytes(keyString);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            }),
            Expires = expiresAt,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        return (jwtToken, expiresAt);
    }

    private static LoginResponseDto BuildLoginResponse(User user, string token, DateTime expiresAt, string message)
    {
        return new LoginResponseDto
        {
            Status = true,
            Message = message,
            Token = token,
            ExpiredAt = expiresAt,
            User = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleName = user.Role?.RoleName ?? string.Empty,
                DeptName = user.Department?.DeptName ?? string.Empty,
            }
        };
    }
}
