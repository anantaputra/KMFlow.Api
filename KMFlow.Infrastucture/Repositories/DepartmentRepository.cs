using KMFlow.Application.DTOs.Departments;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KMFlow.Infrastucture.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;
    private const string Schema = "dbo";

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResponseList<DepartmentResponseDto>> GetAllDepartmentAsync()
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Department_GetAll";
            var departments = await _context.Database
                .SqlQueryRaw<DepartmentResponseDto>(sql)
                .ToListAsync();

            return new ResponseList<DepartmentResponseDto>(departments);
        }
        catch (Exception ex)
        {
            return new ResponseList<DepartmentResponseDto>(
                false,
                $"Error retrieving departments: {ex.Message}",
                new List<DepartmentResponseDto>()
            );
        }
    }

    public async Task<Response<DepartmentResponseDto>> GetDepartmentByIdAsync(Guid id)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Department_GetById @Id=@Id";
            var idParam = new SqlParameter("@Id", id);
            var rows = await _context.Database
                .SqlQueryRaw<DepartmentResponseDto>(sql, idParam)
                .ToListAsync();

            var department = rows.FirstOrDefault();

            if (department == null)
            {
                return new Response<DepartmentResponseDto>(
                    false,
                    $"Department with ID {id} not found",
                    null
                );
            }

            return new Response<DepartmentResponseDto>(department);
        }
        catch (Exception ex)
        {
            return new Response<DepartmentResponseDto>(
                false,
                $"Error retrieving department: {ex.Message}",
                null
            );
        }
    }

    public async Task<Response<DepartmentResponseDto>> AddDepartmentAsync(Guid createdBy, CreateDepartmentDto dto)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Department_Create @CreatedBy=@CreatedBy, @Name=@Name, @Slug=@Slug";
            var createdByParam = new SqlParameter("@CreatedBy", createdBy);
            var nameParam = new SqlParameter("@Name", dto.Name?.Trim() ?? string.Empty);
            var slugParam = new SqlParameter("@Slug", dto.Slug?.Trim() ?? string.Empty);
            var rows = await _context.Database
                .SqlQueryRaw<DepartmentResponseDto>(sql, createdByParam, nameParam, slugParam)
                .ToListAsync();

            var created = rows.FirstOrDefault();

            if (created == null)
            {
                return new Response<DepartmentResponseDto>(false, "Gagal membuat department", null);
            }

            return new Response<DepartmentResponseDto>(true, "Department created successfully", created);
        }
        catch (Exception ex)
        {
            return new Response<DepartmentResponseDto>(
                false,
                $"Error creating department: {ex.Message}",
                null
            );
        }
    }

    public async Task<Response<DepartmentResponseDto>> UpdateDepartmentAsync(Guid updatedBy, Guid id, UpdateDepartmentDto dto)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Department_Update @UpdatedBy=@UpdatedBy, @Id=@Id, @Name=@Name, @Slug=@Slug";
            var updatedByParam = new SqlParameter("@UpdatedBy", updatedBy);
            var idParam = new SqlParameter("@Id", id);
            var nameParam = new SqlParameter("@Name", dto.Name?.Trim() ?? string.Empty);
            var slugParam = new SqlParameter("@Slug", dto.Slug?.Trim() ?? string.Empty);
            var rows = await _context.Database
                .SqlQueryRaw<DepartmentResponseDto>(sql, updatedByParam, idParam, nameParam, slugParam)
                .ToListAsync();

            var updated = rows.FirstOrDefault();

            if (updated == null)
            {
                return new Response<DepartmentResponseDto>(false, $"Department with ID {id} not found", null);
            }

            return new Response<DepartmentResponseDto>(true, "Department updated successfully", updated);
        }
        catch (Exception ex)
        {
            return new Response<DepartmentResponseDto>(
                false,
                $"Error updating department: {ex.Message}",
                null
            );
        }
    }

    public async Task<BaseResponse> DeleteDepartmentAsync(Guid id)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Department_Delete @Id=@Id";
            var idParam = new SqlParameter("@Id", id);
            var rows = await _context.Database
                .SqlQueryRaw<DeleteResultRow>(sql, idParam)
                .ToListAsync();

            var deleted = rows.FirstOrDefault();

            if (deleted == null)
            {
                return new BaseResponse(false, $"Department with ID {id} not found");
            }

            return new BaseResponse(deleted.IsSuccess, deleted.Message);
        }
        catch (Exception ex)
        {
            return new BaseResponse(false, $"Error deleting department: {ex.Message}");
        }
    }

    public async Task<Response<DepartmentResponseDto>> GetByNameAsync(string name)
    {
        var normalized = (name ?? string.Empty).Trim();
        var sql = $"EXEC {Schema}.sp_Department_GetByName @Name=@Name";
        var nameParam = new SqlParameter("@Name", normalized);
        var rows = await _context.Database
            .SqlQueryRaw<DepartmentResponseDto>(sql, nameParam)
            .ToListAsync();
        var department = rows.FirstOrDefault();

        if (department == null)
        {
            return new Response<DepartmentResponseDto>(false, $"Department '{normalized}' tidak ditemukan", null);
        }

        return new Response<DepartmentResponseDto>(department);
    }

    private sealed class DeleteResultRow
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
