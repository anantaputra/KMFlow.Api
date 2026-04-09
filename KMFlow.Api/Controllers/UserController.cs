using KMFlow.Application.DTOs.Users;
using KMFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KMFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : Controller
{
    private readonly IUserService _UserService;

    public UserController(IUserService UserService)
    {
        _UserService = UserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _UserService.GetAllUserAsync();
        if(!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _UserService.GetUserByIdAsync(id);
        if(!response.Status)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateUserDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _UserService.AddUserAsync(userId, dto);
        if(!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromForm] UpdateUserDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _UserService.UpdateUserAsync(userId, dto);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _UserService.DeleteUserAsync(id);
        if(!response.Status)
            return BadRequest(response);

        return Ok(response);
    }
}
