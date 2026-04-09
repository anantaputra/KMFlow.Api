using KMFlow.Application.DTOs.Notifications;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Application.Interfaces.Services;
using KMFlow.Domain.Enums;

namespace KMFlow.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Response<NotificationResponseDto>> CreateAsync(
        Guid recipientUserId,
        NotificationType type,
        string title,
        string message,
        Guid? knowledgeId = null,
        Guid? relatedActionBy = null
    )
    {
        if (recipientUserId == Guid.Empty)
        {
            return new Response<NotificationResponseDto>(false, "recipientUserId tidak valid", null);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return new Response<NotificationResponseDto>(false, "Title wajib diisi", null);
        }

        if (title.Length > 255)
        {
            return new Response<NotificationResponseDto>(false, "Title maksimal 255 karakter", null);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return new Response<NotificationResponseDto>(false, "Message wajib diisi", null);
        }

        return await _notificationRepository.CreateAsync(
            recipientUserId,
            type,
            title.Trim(),
            message.Trim(),
            knowledgeId,
            relatedActionBy
        );
    }

    public async Task<ResponseList<NotificationResponseDto>> GetByUserAsync(Guid userId, bool? isRead = null)
    {
        if (userId == Guid.Empty)
        {
            return new ResponseList<NotificationResponseDto>(
                false,
                "userId tidak valid",
                new List<NotificationResponseDto>()
            );
        }

        return await _notificationRepository.GetByUserAsync(userId, isRead);
    }

    public async Task<Response<int>> GetUnreadCountAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return new Response<int>(false, "userId tidak valid", 0);
        }

        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    public async Task<BaseResponse> MarkAsReadAsync(Guid userId, int notificationId)
    {
        if (userId == Guid.Empty)
        {
            return new BaseResponse(false, "userId tidak valid");
        }

        if (notificationId <= 0)
        {
            return new BaseResponse(false, "notificationId tidak valid");
        }

        return await _notificationRepository.MarkAsReadAsync(userId, notificationId);
    }

    public async Task<BaseResponse> MarkAllAsReadAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return new BaseResponse(false, "userId tidak valid");
        }

        return await _notificationRepository.MarkAllAsReadAsync(userId);
    }
}
