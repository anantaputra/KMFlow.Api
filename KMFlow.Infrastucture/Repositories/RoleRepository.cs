using KMFlow.Application.DTOs.Roles;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KMFlow.Infrastucture.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;
    private const string Schema = "dbo";

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResponseList<RoleResponseDto>> GetAllRoleAsync()
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Role_GetAll";
            var roles = await _context.Database
                .SqlQueryRaw<RoleResponseDto>(sql)
                .ToListAsync();

            return new ResponseList<RoleResponseDto>(roles);
        }
        catch (Exception ex)
        {
            return new ResponseList<RoleResponseDto>(
                false,
                $"Error retrieving roles: {ex.Message}",
                new List<RoleResponseDto>()
            );
        }
    }
}
