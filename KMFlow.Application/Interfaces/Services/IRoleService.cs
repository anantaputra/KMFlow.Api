using KMFlow.Application.DTOs.Roles;

namespace KMFlow.Application.Interfaces.Services;

public interface IRoleService
{
    Task<ResponseList<RoleResponseDto>> GetAllRoleAsync();
}
