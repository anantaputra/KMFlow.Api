using KMFlow.Application.DTOs.Departments;
using KMFlow.Application.DTOs.Knowledges;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KMFlow.Infrastucture.Repositories;

public class KnowledgeRepository : IKnowledgeRepository
{
    private readonly AppDbContext _context;
    private const string Schema = "dbo";

    public KnowledgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BaseResponse> ValidateSmeCanBeDisabledAsync(Guid smeUserId)
    {
        try
        {
            var hasInReview = await _context.Knowledges
                .AsNoTracking()
                .AnyAsync(k => k.Status == 2 && k.UpdatedBy == smeUserId);

            if (hasInReview)
            {
                return new BaseResponse(false, "Tidak bisa menonaktifkan SME karena user sedang melakukan review minimal 1 request kontribusi knowledge");
            }

            return new BaseResponse(true, "Success");
        }
        catch (Exception ex)
        {
            return new BaseResponse(false, $"Error validasi review knowledge: {ex.Message}");
        }
    }

    public async Task<Response<DepartmentResponseDto>> GetUserDepartmentAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetUserDepartment @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var rows = await _context.Database
                .SqlQueryRaw<DepartmentResponseDto>(sql, userIdParam)
                .ToListAsync();
            var department = rows.FirstOrDefault();

            if (department == null)
            {
                return new Response<DepartmentResponseDto>(false, $"User dengan ID {userId} tidak ditemukan", null);
            }

            return new Response<DepartmentResponseDto>(department);
        }
        catch (Exception ex)
        {
            return new Response<DepartmentResponseDto>(false, $"Error mengambil department user: {ex.Message}", null);
        }
    }

    public async Task<Response<DepartmentResponseDto>> GetDepartmentByNameAsync(string name)
    {
        try
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
        catch (Exception ex)
        {
            return new Response<DepartmentResponseDto>(false, $"Error mengambil department: {ex.Message}", null);
        }
    }

    public async Task<Response<KnowledgeResponseDto>> AddKnowledgeAsync(Guid submittedBy, Guid ownerDeptId, string fileName, string filePath, int status)
    {
        try
        {
            var normalizedFileName = (fileName ?? string.Empty).Trim();
            var normalizedFilePath = (filePath ?? string.Empty).Trim();

            var sql =
                $"EXEC {Schema}.sp_Knowledge_Create @SubmittedBy=@SubmittedBy, @OwnerDeptId=@OwnerDeptId, @FileName=@FileName, @FilePath=@FilePath, @Status=@Status";
            var submittedByParam = new SqlParameter("@SubmittedBy", submittedBy);
            var ownerDeptIdParam = new SqlParameter("@OwnerDeptId", ownerDeptId);
            var fileNameParam = new SqlParameter("@FileName", normalizedFileName);
            var filePathParam = new SqlParameter("@FilePath", normalizedFilePath);
            var statusParam = new SqlParameter("@Status", status);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeOperationRow>(
                    sql,
                    submittedByParam,
                    ownerDeptIdParam,
                    fileNameParam,
                    filePathParam,
                    statusParam
                )
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<KnowledgeResponseDto>(false, "Error creating knowledge", null);
            }

            var data = ToKnowledgeResponseDto(result);
            return new Response<KnowledgeResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error creating knowledge: {ex.Message}", null);
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllKnowledgeAsync()
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetAllPublished";
            var knowledges = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(knowledges);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error mengambil knowledge: {ex.Message}"
            };
        }
    }

    public async Task<Response<KnowledgeStatsResponseDto>> GetKnowledgeStatsAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetStats @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeStatsResponseDto>(sql, userIdParam)
                .ToListAsync();
            var stats = rows.FirstOrDefault();

            if (stats == null)
            {
                return new Response<KnowledgeStatsResponseDto>(false, "Error mengambil statistik knowledge", null);
            }

            return new Response<KnowledgeStatsResponseDto>(true, "Success", stats);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeStatsResponseDto>(false, $"Error mengambil statistik knowledge: {ex.Message}", null);
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> SearchKnowledgeAsync(string? query, string? department)
    {
        try
        {
            var trimmedQuery = query?.Trim();
            var trimmedDepartment = department?.Trim();

            var sql = $"EXEC {Schema}.sp_Knowledge_Search @Query=@Query, @Department=@Department";
            var queryParam = new SqlParameter("@Query", (object?)trimmedQuery ?? DBNull.Value);
            var departmentParam = new SqlParameter("@Department", (object?)trimmedDepartment ?? DBNull.Value);
            var knowledges = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql, queryParam, departmentParam)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(knowledges);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error search knowledge: {ex.Message}"
            };
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllPendingReviewKnowledgeAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetPendingReviewByUser @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var knowledges = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql, userIdParam)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(knowledges);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error mengambil knowledge untuk review: {ex.Message}"
            };
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllInReviewKnowledgeAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetInReviewByUser @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var knowledges = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql, userIdParam)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(knowledges);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error mengambil knowledge in review: {ex.Message}"
            };
        }
    }

    public async Task<Response<KnowledgeResponseDto>> ReviewKnowledgeAsync(Guid actionBy, Guid knowledgeId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_Review @ActionBy=@ActionBy, @KnowledgeId=@KnowledgeId";
            var actionByParam = new SqlParameter("@ActionBy", actionBy);
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", knowledgeId);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeOperationRow>(sql, actionByParam, knowledgeIdParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<KnowledgeResponseDto>(false, $"Knowledge dengan ID {knowledgeId} tidak ditemukan", null);
            }

            var data = ToKnowledgeResponseDto(result);
            return new Response<KnowledgeResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error review knowledge: {ex.Message}", null);
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllRejectKnowledgeAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetRejectedByUser @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var knowledges = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql, userIdParam)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(knowledges);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error mengambil knowledge rejected: {ex.Message}"
            };
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllApproveKnowledgeAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetApprovedByUser @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var knowledges = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql, userIdParam)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(knowledges);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error mengambil knowledge approved: {ex.Message}"
            };
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllPublishKnowledgeAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetPublishedByUser @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var knowledges = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql, userIdParam)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(knowledges);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error mengambil knowledge published: {ex.Message}"
            };
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetRecentlyAddedKnowledgeAsync()
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetRecentlyAdded";
            var knowledges = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(knowledges);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error mengambil knowledge recently added: {ex.Message}"
            };
        }
    }

    public async Task<Response<KnowledgeResponseDto>> ApproveKnowledgeAsync(Guid actionBy, Guid knowledgeId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_Approve @ActionBy=@ActionBy, @KnowledgeId=@KnowledgeId";
            var actionByParam = new SqlParameter("@ActionBy", actionBy);
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", knowledgeId);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeOperationRow>(sql, actionByParam, knowledgeIdParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<KnowledgeResponseDto>(false, $"Knowledge dengan ID {knowledgeId} tidak ditemukan", null);
            }

            var data = ToKnowledgeResponseDto(result);
            return new Response<KnowledgeResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error approve knowledge: {ex.Message}", null);
        }
    }

    public async Task<Response<KnowledgeResponseDto>> PublishKnowledgeAsync(Guid actionBy, Guid knowledgeId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_Publish @ActionBy=@ActionBy, @KnowledgeId=@KnowledgeId";
            var actionByParam = new SqlParameter("@ActionBy", actionBy);
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", knowledgeId);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeOperationRow>(sql, actionByParam, knowledgeIdParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<KnowledgeResponseDto>(false, $"Knowledge dengan ID {knowledgeId} tidak ditemukan", null);
            }

            var data = ToKnowledgeResponseDto(result);
            return new Response<KnowledgeResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error publish knowledge: {ex.Message}", null);
        }
    }

    public async Task<Response<KnowledgeResponseDto>> RejectKnowledgeAsync(Guid actionBy, Guid knowledgeId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_Reject @ActionBy=@ActionBy, @KnowledgeId=@KnowledgeId";
            var actionByParam = new SqlParameter("@ActionBy", actionBy);
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", knowledgeId);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeOperationRow>(sql, actionByParam, knowledgeIdParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<KnowledgeResponseDto>(false, $"Knowledge dengan ID {knowledgeId} tidak ditemukan", null);
            }

            var data = ToKnowledgeResponseDto(result);
            return new Response<KnowledgeResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error reject knowledge: {ex.Message}", null);
        }
    }

    public async Task<Response<KnowledgeResponseDto>> GetKnowledgeAsync(Guid knowledgeId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetById @KnowledgeId=@KnowledgeId";
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", knowledgeId);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql, knowledgeIdParam)
                .ToListAsync();
            var knowledge = rows.FirstOrDefault();

            if (knowledge == null)
            {
                return new Response<KnowledgeResponseDto>(false, $"Knowledge dengan ID {knowledgeId} tidak ditemukan", null);
            }

            return new Response<KnowledgeResponseDto>(true, "Success", knowledge);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error mengambil knowledge: {ex.Message}", null);
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetKnowledgeByUserAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_GetBySubmittedBy @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var response = await _context.Database
                .SqlQueryRaw<KnowledgeResponseDto>(sql, userIdParam)
                .ToListAsync();

            return new ResponseList<KnowledgeResponseDto>(response);
        }
        catch (Exception ex)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = $"Error mengambil kontribusi knowledge: {ex.Message}"
            };
        }
    }

    public async Task<Response<KnowledgeResponseDto>> UpdateDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId, Guid ownerDeptId, string fileName, string filePath, int status)
    {
        try
        {
            var sql =
                $"EXEC {Schema}.sp_Knowledge_UpdateDraft @SubmittedBy=@SubmittedBy, @KnowledgeId=@KnowledgeId, @OwnerDeptId=@OwnerDeptId, @FileName=@FileName, @FilePath=@FilePath, @Status=@Status";
            var submittedByParam = new SqlParameter("@SubmittedBy", submittedBy);
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", knowledgeId);
            var ownerDeptIdParam = new SqlParameter("@OwnerDeptId", ownerDeptId);
            var fileNameParam = new SqlParameter("@FileName", (fileName ?? string.Empty).Trim());
            var filePathParam = new SqlParameter("@FilePath", (filePath ?? string.Empty).Trim());
            var statusParam = new SqlParameter("@Status", status);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeOperationRow>(
                    sql,
                    submittedByParam,
                    knowledgeIdParam,
                    ownerDeptIdParam,
                    fileNameParam,
                    filePathParam,
                    statusParam
                )
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<KnowledgeResponseDto>(false, $"Knowledge dengan ID {knowledgeId} tidak ditemukan", null);
            }

            var data = ToKnowledgeResponseDto(result);
            return new Response<KnowledgeResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error updating knowledge: {ex.Message}", null);
        }
    }

    public async Task<Response<KnowledgeResponseDto>> SubmitDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_SubmitDraft @SubmittedBy=@SubmittedBy, @KnowledgeId=@KnowledgeId";
            var submittedByParam = new SqlParameter("@SubmittedBy", submittedBy);
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", knowledgeId);
            var rows = await _context.Database
                .SqlQueryRaw<KnowledgeOperationRow>(sql, submittedByParam, knowledgeIdParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<KnowledgeResponseDto>(false, $"Knowledge dengan ID {knowledgeId} tidak ditemukan", null);
            }

            var data = ToKnowledgeResponseDto(result);
            return new Response<KnowledgeResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? data : null);
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error submit knowledge: {ex.Message}", null);
        }
    }

    public async Task<BaseResponse> DeleteDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Knowledge_DeleteDraft @SubmittedBy=@SubmittedBy, @KnowledgeId=@KnowledgeId";
            var submittedByParam = new SqlParameter("@SubmittedBy", submittedBy);
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", knowledgeId);
            var rows = await _context.Database
                .SqlQueryRaw<DeleteResultRow>(sql, submittedByParam, knowledgeIdParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new BaseResponse(false, $"Knowledge dengan ID {knowledgeId} tidak ditemukan");
            }

            return new BaseResponse(result.IsSuccess, result.Message);
        }
        catch (Exception ex)
        {
            return new BaseResponse(false, $"Error deleting knowledge: {ex.Message}");
        }
    }

    private static KnowledgeResponseDto ToKnowledgeResponseDto(KnowledgeOperationRow r)
    {
        return new KnowledgeResponseDto
        {
            Id = r.Id,
            FileName = r.FileName ?? string.Empty,
            FilePath = r.FilePath ?? string.Empty,
            OwnerDepartment = r.OwnerDepartment ?? string.Empty,
            PublishedBy = r.PublishedBy ?? string.Empty,
            Status = r.Status ?? string.Empty,
            PublishedAt = r.PublishedAt,
            UpdatedAt = r.UpdatedAt
        };
    }

    private sealed class KnowledgeOperationRow
    {
        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? OwnerDepartment { get; set; }
        public string? PublishedBy { get; set; }
        public string? Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private sealed class DeleteResultRow
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
