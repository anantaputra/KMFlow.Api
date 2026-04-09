using KMFlow.Application.DTOs.Auths;
using KMFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(
                new LoginResponseDto
                {
                    Status = false,
                    Message = "Login Failed"
                }
            );

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshAsync(request);

        if (result == null)
            return Unauthorized(
                new LoginResponseDto
                {
                    Status = false,
                    Message = "Refresh Failed"
                }
            );

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var me = await _authService.GetMeAsync(userId);
        if (me == null)
            return Unauthorized();

        return Ok(me);
    }
}
