using KMFlow.Application.DTOs.Departments;
using KMFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KMFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _departmentService.GetAllDepartmentAsync();
        if(!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _departmentService.GetDepartmentByIdAsync(id);
        if(!response.Status)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateDepartmentDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _departmentService.AddDepartmentAsync(userId, dto);
        if(!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateDepartmentDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));
            
        var response = await _departmentService.UpdateDepartmentAsync(userId, id, dto);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _departmentService.DeleteDepartmentAsync(id);
        if(!response.Status)
            return BadRequest(response);

        return Ok(response);
    }
}
