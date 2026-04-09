using KMFlow.Application.DTOs.Notifications;
using KMFlow.Domain.Enums;

namespace KMFlow.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Response<NotificationResponseDto>> CreateAsync(
        Guid recipientUserId,
        NotificationType type,
        string title,
        string message,
        Guid? knowledgeId,
        Guid? relatedActionBy
    );

    Task<ResponseList<NotificationResponseDto>> GetByUserAsync(Guid userId, bool? isRead);

    Task<Response<int>> GetUnreadCountAsync(Guid userId);

    Task<BaseResponse> MarkAsReadAsync(Guid userId, int notificationId);

    Task<BaseResponse> MarkAllAsReadAsync(Guid userId);
}
