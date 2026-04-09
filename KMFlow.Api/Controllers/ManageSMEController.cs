using KMFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KMFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ManageSMEController : Controller
{
    private readonly IUserService _userService;

    public ManageSMEController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSme()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _userService.GetAllSmeAsync(userId);
        if (!response.Status)
        {
            if (response.Message.Contains("admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new BaseResponse(false, response.Message));

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{userId:guid}/enable")]
    public async Task<IActionResult> PromoteToSme(Guid userId)
    {
        if (!TryGetUserId(out var requestedBy))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _userService.PromoteToSmeAsync(requestedBy, userId);
        if (!response.Status)
        {
            if (response.Message.Contains("admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new BaseResponse(false, response.Message));

            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{userId:guid}/promote")]
    public async Task<IActionResult> PromoteSme(Guid userId)
    {
        if (!TryGetUserId(out var requestedBy))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _userService.PromoteToSmeAsync(requestedBy, userId);
        if (!response.Status)
        {
            if (response.Message.Contains("admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new BaseResponse(false, response.Message));

            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{userId:guid}/disable")]
    public async Task<IActionResult> DisableSme(Guid userId)
    {
        if (!TryGetUserId(out var requestedBy))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _userService.DemoteFromSmeAsync(requestedBy, userId);
        if (!response.Status)
        {
            if (response.Message.Contains("admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new BaseResponse(false, response.Message));

            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{userId:guid}/demote")]
    public async Task<IActionResult> DemoteSme(Guid userId)
    {
        if (!TryGetUserId(out var requestedBy))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _userService.DemoteFromSmeAsync(requestedBy, userId);
        if (!response.Status)
        {
            if (response.Message.Contains("admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new BaseResponse(false, response.Message));

            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out userId);
    }
}
