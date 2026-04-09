using KMFlow.Application.DTOs.Roles;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Application.Interfaces.Services;

namespace KMFlow.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<ResponseList<RoleResponseDto>> GetAllRoleAsync()
    {
        return await _roleRepository.GetAllRoleAsync();
    }
}
