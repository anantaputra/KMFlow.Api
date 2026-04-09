using KMFlow.Application.DTOs.Users;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Domain.Entities;
using KMFlow.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KMFlow.Infrastucture.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _hasher;
    private const string Schema = "dbo";

    public UserRepository(AppDbContext context)
    {
        _context = context;
        _hasher = new PasswordHasher<User>();
    }

    public async Task<Response<UserResponseDto>> AddUserAsync(Guid createdBy, CreateUserDto dto)
    {
        try
        {
            var email = (dto.Email ?? string.Empty).Trim();

            var userForHash = new User { Id = Guid.NewGuid(), Email = email, Name = dto.Name ?? string.Empty };
            var passwordHash = _hasher.HashPassword(userForHash, dto.Password ?? string.Empty);

            var sql =
                $"EXEC {Schema}.sp_User_Create @CreatedBy=@CreatedBy, @Id=@Id, @Name=@Name, @Email=@Email, @DeptId=@DeptId, @RoleId=@RoleId, @PasswordHash=@PasswordHash";
            var createdByParam = new SqlParameter("@CreatedBy", createdBy);
            var idParam = new SqlParameter("@Id", userForHash.Id);
            var nameParam = new SqlParameter("@Name", dto.Name?.Trim() ?? string.Empty);
            var emailParam = new SqlParameter("@Email", email);
            var deptIdParam = new SqlParameter("@DeptId", dto.DeptId);
            var roleIdParam = new SqlParameter("@RoleId", dto.RoleId);
            var passwordHashParam = new SqlParameter("@PasswordHash", passwordHash);
            var rows = await _context.Database
                .SqlQueryRaw<UserOperationRow>(
                    sql,
                    createdByParam,
                    idParam,
                    nameParam,
                    emailParam,
                    deptIdParam,
                    roleIdParam,
                    passwordHashParam
                )
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<UserResponseDto>(false, "Error creating user", null);
            }

            var data = new UserResponseDto
            {
                Id = result.Id,
                Name = result.Name ?? string.Empty,
                Email = result.Email ?? string.Empty,
                RoleName = result.RoleName ?? string.Empty,
                DeptName = result.DeptName ?? string.Empty,
                CreatedAt = result.CreatedAt
            };

            return new Response<UserResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<UserResponseDto>(
                false,
                $"Error creating user: {ex.Message}",
                null
            );
        }
    }

    public async Task<BaseResponse> DeleteUserAsync(Guid id)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_User_SoftDelete @Id=@Id";
            var idParam = new SqlParameter("@Id", id);
            var rows = await _context.Database
                .SqlQueryRaw<OperationResultRow>(sql, idParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new BaseResponse(false, $"User with ID {id} not found");
            }

            return new BaseResponse(result.IsSuccess, result.Message);
        }
        catch (Exception ex)
        {
            return new BaseResponse(false, $"Error deleting user: {ex.Message}");
        }
    }

    public async Task<ResponseList<UserResponseDto>> GetUsersByRoleAsync(string roleName)
    {
        try
        {
            var normalizedRoleName = (roleName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                return new ResponseList<UserResponseDto>(
                    false,
                    "roleName wajib diisi",
                    new List<UserResponseDto>()
                );
            }

            var users = await _context.Database
                .SqlQueryRaw<UserResponseDto>(
                    $"EXEC {Schema}.sp_User_GetByRole @RoleName=@RoleName",
                    new SqlParameter("@RoleName", normalizedRoleName)
                )
                .ToListAsync();

            return new ResponseList<UserResponseDto>(users);
        }
        catch (Exception ex)
        {
            return new ResponseList<UserResponseDto>(
                false,
                $"Error retrieving users by role: {ex.Message}",
                new List<UserResponseDto>()
            );
        }
    }

    public async Task<Response<UserResponseDto>> SetUserRoleAsync(Guid updatedBy, Guid targetUserId, string roleName)
    {
        try
        {
            var normalizedRoleName = (roleName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                return new Response<UserResponseDto>(false, "roleName wajib diisi", null);
            }

            var sql =
                $"EXEC {Schema}.sp_User_SetRole @UpdatedBy=@UpdatedBy, @TargetUserId=@TargetUserId, @RoleName=@RoleName";
            var updatedByParam = new SqlParameter("@UpdatedBy", updatedBy);
            var targetUserIdParam = new SqlParameter("@TargetUserId", targetUserId);
            var roleNameParam = new SqlParameter("@RoleName", normalizedRoleName);
            var rows = await _context.Database
                .SqlQueryRaw<UserOperationRow>(sql, updatedByParam, targetUserIdParam, roleNameParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<UserResponseDto>(false, $"User dengan ID {targetUserId} tidak ditemukan", null);
            }

            var data = new UserResponseDto
            {
                Id = result.Id,
                Name = result.Name ?? string.Empty,
                Email = result.Email ?? string.Empty,
                RoleName = result.RoleName ?? string.Empty,
                DeptName = result.DeptName ?? string.Empty,
                CreatedAt = result.CreatedAt
            };

            return new Response<UserResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<UserResponseDto>(false, $"Error set user role: {ex.Message}", null);
        }
    }

    public async Task<ResponseList<UserResponseDto>> GetAllUserAsync()
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_User_GetAllActive";
            var users = await _context.Database
                .SqlQueryRaw<UserResponseDto>(sql)
                .ToListAsync();

            return new ResponseList<UserResponseDto>(users);
        }
        catch (Exception ex)
        {
            return new ResponseList<UserResponseDto>(
                false,
                $"Error retrieving users: {ex.Message}",
                new List<UserResponseDto>()
            );
        }
    }

    public async Task<Response<UserResponseDto>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_User_GetByIdActive @Id=@Id";
            var idParam = new SqlParameter("@Id", id);
            var rows = await _context.Database
                .SqlQueryRaw<UserResponseDto>(sql, idParam)
                .ToListAsync();
            var user = rows.FirstOrDefault();

            if (user == null)
            {
                return new Response<UserResponseDto>(
                    false,
                    $"User with ID {id} not found",
                    null
                );
            }

            return new Response<UserResponseDto>(user);
        }
        catch (Exception ex)
        {
            return new Response<UserResponseDto>(
                false,
                $"Error retrieving user: {ex.Message}",
                null
            );
        }
    }

    public async Task<Response<UserResponseDto>> UpdateUserAsync(Guid updatedBy, UpdateUserDto dto)
    {
        try
        {
            var email = (dto.Email ?? string.Empty).Trim();
            string? passwordHash = null;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var userForHash = new User { Id = dto.Id, Email = email, Name = dto.Name ?? string.Empty };
                passwordHash = _hasher.HashPassword(userForHash, dto.Password);
            }

            var sql =
                $"EXEC {Schema}.sp_User_Update @UpdatedBy=@UpdatedBy, @Id=@Id, @Name=@Name, @Email=@Email, @DeptId=@DeptId, @RoleId=@RoleId, @PasswordHash=@PasswordHash";
            var updatedByParam = new SqlParameter("@UpdatedBy", updatedBy);
            var idParam = new SqlParameter("@Id", dto.Id);
            var nameParam = new SqlParameter("@Name", dto.Name?.Trim() ?? string.Empty);
            var emailParam = new SqlParameter("@Email", email);
            var deptIdParam = new SqlParameter("@DeptId", dto.DeptId);
            var roleIdParam = new SqlParameter("@RoleId", dto.RoleId);
            var passwordHashParam = new SqlParameter("@PasswordHash", (object?)passwordHash ?? DBNull.Value);
            var rows = await _context.Database
                .SqlQueryRaw<UserOperationRow>(
                    sql,
                    updatedByParam,
                    idParam,
                    nameParam,
                    emailParam,
                    deptIdParam,
                    roleIdParam,
                    passwordHashParam
                )
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<UserResponseDto>(false, $"User with ID {dto.Id} not found", null);
            }

            var data = new UserResponseDto
            {
                Id = result.Id,
                Name = result.Name ?? string.Empty,
                Email = result.Email ?? string.Empty,
                RoleName = result.RoleName ?? string.Empty,
                DeptName = result.DeptName ?? string.Empty,
                CreatedAt = result.CreatedAt
            };

            return new Response<UserResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<UserResponseDto>(
                false,
                $"Error updating user: {ex.Message}",
                null
            );
        }
    }

    private sealed class UserOperationRow
    {
        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public string? DeptName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed class OperationResultRow
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
