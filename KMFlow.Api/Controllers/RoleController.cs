using KMFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class RoleController : Controller
{
    private IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _roleService.GetAllRoleAsync();
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }
}
