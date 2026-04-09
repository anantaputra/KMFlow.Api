using KMFlow.Application.DTOs.Departments;
using KMFlow.Application.DTOs.Knowledges;

namespace KMFlow.Application.Interfaces.Repositories;

public interface IKnowledgeRepository
{
    Task<Response<DepartmentResponseDto>> GetUserDepartmentAsync(Guid userId);
    Task<Response<DepartmentResponseDto>> GetDepartmentByNameAsync(string name);
    Task<Response<KnowledgeResponseDto>> AddKnowledgeAsync(Guid submittedBy, Guid ownerDeptId, string fileName, string filePath, int status);
    Task<ResponseList<KnowledgeResponseDto>> GetAllKnowledgeAsync();
    Task<Response<KnowledgeStatsResponseDto>> GetKnowledgeStatsAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> SearchKnowledgeAsync(string? query, string? department);
    Task<ResponseList<KnowledgeResponseDto>> GetAllPendingReviewKnowledgeAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> GetAllInReviewKnowledgeAsync(Guid userId);
    Task<Response<KnowledgeResponseDto>> ReviewKnowledgeAsync(Guid actionBy, Guid knowledgeId);
    Task<BaseResponse> ValidateSmeCanBeDisabledAsync(Guid smeUserId);
    Task<ResponseList<KnowledgeResponseDto>> GetAllRejectKnowledgeAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> GetAllApproveKnowledgeAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> GetAllPublishKnowledgeAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> GetRecentlyAddedKnowledgeAsync();
    Task<Response<KnowledgeResponseDto>> ApproveKnowledgeAsync(Guid actionBy, Guid knowledgeId);
    Task<Response<KnowledgeResponseDto>> PublishKnowledgeAsync(Guid actionBy, Guid knowledgeId);
    Task<Response<KnowledgeResponseDto>> RejectKnowledgeAsync(Guid actionBy, Guid knowledgeId);
    Task<ResponseList<KnowledgeResponseDto>> GetKnowledgeByUserAsync(Guid userId);
    Task<Response<KnowledgeResponseDto>> UpdateDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId, Guid ownerDeptId, string fileName, string filePath, int status);
    Task<Response<KnowledgeResponseDto>> GetKnowledgeAsync(Guid knowledgeId);
    Task<Response<KnowledgeResponseDto>> SubmitDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId);
    Task<BaseResponse> DeleteDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId);
}
