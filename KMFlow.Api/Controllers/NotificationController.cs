using KMFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KMFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMy([FromQuery] bool? isRead)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _notificationService.GetByUserAsync(userId, isRead);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _notificationService.GetUnreadCountAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("{notificationId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _notificationService.MarkAsReadAsync(userId, notificationId);
        if (!response.Status)
        {
            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _notificationService.MarkAllAsReadAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out userId);
    }
}
