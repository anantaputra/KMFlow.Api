using KMFlow.Application.DTOs.Notifications;
using KMFlow.Domain.Enums;

namespace KMFlow.Application.Interfaces.Services;

public interface INotificationService
{
    Task<Response<NotificationResponseDto>> CreateAsync(
        Guid recipientUserId,
        NotificationType type,
        string title,
        string message,
        Guid? knowledgeId = null,
        Guid? relatedActionBy = null
    );

    Task<ResponseList<NotificationResponseDto>> GetByUserAsync(Guid userId, bool? isRead = null);

    Task<Response<int>> GetUnreadCountAsync(Guid userId);

    Task<BaseResponse> MarkAsReadAsync(Guid userId, int notificationId);

    Task<BaseResponse> MarkAllAsReadAsync(Guid userId);
}
