using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Domain.Entities;
using KMFlow.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KMFlow.Infrastucture.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
    private const string Schema = "dbo";

    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserWithLoginDetailByEmail(string email)
    {
        var normalized = (email ?? string.Empty).Trim();
        var sql = $"EXEC {Schema}.sp_User_GetLoginByEmail @Email";
        var emailParam = new SqlParameter("@Email", normalized);
        var rows = await _context.Database
            .SqlQueryRaw<LoginUserRow>(sql, emailParam)
            .ToListAsync();

        var row = rows.FirstOrDefault();
        return row == null ? null : MapUserWithLoginDetail(row);
    }

    public async Task<User?> GetUserWithLoginDetailById(Guid id)
    {
        var sql = $"EXEC {Schema}.sp_User_GetLoginById @Id";
        var idParam = new SqlParameter("@Id", id);
        var rows = await _context.Database
            .SqlQueryRaw<LoginUserRow>(sql, idParam)
            .ToListAsync();

        var row = rows.FirstOrDefault();
        return row == null ? null : MapUserWithLoginDetail(row);
    }

    private static User MapUserWithLoginDetail(LoginUserRow r)
    {
        var user = new User
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Email = r.Email ?? string.Empty,
            PasswordHash = r.PasswordHash ?? string.Empty,
            DeptId = r.DeptId,
            RoleId = r.RoleId,
            IsActive = r.IsActive,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            UpdatedBy = r.UpdatedBy,
            UpdatedAt = r.UpdatedAt
        };

        if (r.DeptId != null)
        {
            user.Department = new Department
            {
                Id = r.DeptId.Value,
                DeptName = r.DeptName ?? string.Empty,
                DeptSlug = r.DeptSlug ?? string.Empty
            };
        }

        if (r.RoleId != null)
        {
            user.Role = new Role
            {
                Id = r.RoleId.Value,
                RoleName = r.RoleName ?? string.Empty
            };
        }

        return user;
    }

    private sealed class LoginUserRow
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public Guid? DeptId { get; set; }
        public Guid? RoleId { get; set; }
        public bool IsActive { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? DeptName { get; set; }
        public string? DeptSlug { get; set; }
        public string? RoleName { get; set; }
    }
}
