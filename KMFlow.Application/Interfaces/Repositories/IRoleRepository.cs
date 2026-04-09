using KMFlow.Application.DTOs.Roles;

namespace KMFlow.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<ResponseList<RoleResponseDto>> GetAllRoleAsync();
}
