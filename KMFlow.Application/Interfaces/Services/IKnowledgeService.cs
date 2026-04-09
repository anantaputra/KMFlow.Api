using KMFlow.Application.DTOs.Knowledges;

namespace KMFlow.Application.Interfaces.Services;

public interface IKnowledgeService
{
    Task<Response<KnowledgeResponseDto>> AddKnowledgeAsync(Guid submittedBy, AddKnowledgeWithFileRequestDto request);
    Task<ResponseList<KnowledgeResponseDto>> GetAllKnowledgeAsync();
    Task<Response<KnowledgeStatsResponseDto>> GetKnowledgeStatsAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> SearchKnowledgeAsync(string? query, string? department);
    Task<ResponseList<KnowledgeResponseDto>> GetAllPendingReviewKnowledgeAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> GetAllInReviewKnowledgeAsync(Guid userId);
    Task<Response<KnowledgeResponseDto>> ReviewKnowledgeAsync(Guid actionBy, Guid knowledgeId);
    Task<ResponseList<KnowledgeResponseDto>> GetAllRejectKnowledgeAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> GetAllApproveKnowledgeAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> GetAllPublishKnowledgeAsync(Guid userId);
    Task<ResponseList<KnowledgeResponseDto>> GetRecentlyAddedKnowledgeAsync();
    Task<Response<KnowledgeResponseDto>> ApproveKnowledgeAsync(Guid actionBy, Guid knowledgeId);
    Task<Response<KnowledgeResponseDto>> PublishKnowledgeAsync(Guid actionBy, Guid knowledgeId);
    Task<Response<KnowledgeResponseDto>> RejectKnowledgeAsync(Guid actionBy, Guid knowledgeId);
    Task<ResponseList<KnowledgeResponseDto>> GetKnowledgeByUserAsync(Guid userId);
    Task<Response<KnowledgeResponseDto>> GetDetailKnowledgeAsync(Guid knowledgeId);
    Task<Response<KnowledgeResponseDto>> UpdateDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId, UpdateDraftKnowledgeRequestDto request);
    Task<Response<KnowledgeResponseDto>> SubmitDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId);
    Task<ResponseList<string>> GetKnowledgeStatusesAsync();
    Task<BaseResponse> DeleteDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId);
}
