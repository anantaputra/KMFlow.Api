using KMFlow.Application.DTOs.Notifications;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Domain.Enums;
using KMFlow.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KMFlow.Infrastucture.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;
    private const string Schema = "dbo";

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Response<NotificationResponseDto>> CreateAsync(
        Guid recipientUserId,
        NotificationType type,
        string title,
        string message,
        Guid? knowledgeId,
        Guid? relatedActionBy
    )
    {
        try
        {
            var sql =
                $"EXEC {Schema}.sp_Notification_Create @RecipientUserId=@RecipientUserId, @Type=@Type, @Title=@Title, @Message=@Message, @KnowledgeId=@KnowledgeId, @RelatedActionBy=@RelatedActionBy";
            var recipientParam = new SqlParameter("@RecipientUserId", recipientUserId);
            var typeParam = new SqlParameter("@Type", type.ToString());
            var titleParam = new SqlParameter("@Title", title?.Trim() ?? string.Empty);
            var messageParam = new SqlParameter("@Message", message?.Trim() ?? string.Empty);
            var knowledgeIdParam = new SqlParameter("@KnowledgeId", (object?)knowledgeId ?? DBNull.Value);
            var relatedActionByParam = new SqlParameter("@RelatedActionBy", (object?)relatedActionBy ?? DBNull.Value);
            var rows = await _context.Database
                .SqlQueryRaw<CreateResultRow>(
                    sql,
                    recipientParam,
                    typeParam,
                    titleParam,
                    messageParam,
                    knowledgeIdParam,
                    relatedActionByParam
                )
                .ToListAsync();

            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new Response<NotificationResponseDto>(false, "Error creating notification", null);
            }

            var dto = new NotificationResponseDto
            {
                NotificationId = result.NotificationId,
                UserId = result.UserId,
                KnowledgeId = result.KnowledgeId,
                Type = result.Type,
                Title = result.Title,
                Message = result.Message,
                IsRead = result.IsRead,
                RelatedActionBy = result.RelatedActionBy,
                CreatedDate = result.CreatedDate
            };

            return new Response<NotificationResponseDto>(result.IsSuccess, result.ResponseMessage, result.IsSuccess ? dto : null);
        }
        catch (Exception ex)
        {
            return new Response<NotificationResponseDto>(false, $"Error creating notification: {ex.Message}", null);
        }
    }

    public async Task<ResponseList<NotificationResponseDto>> GetByUserAsync(Guid userId, bool? isRead)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Notification_GetByUser @UserId=@UserId, @IsRead=@IsRead";
            var userIdParam = new SqlParameter("@UserId", userId);
            var isReadParam = new SqlParameter("@IsRead", (object?)isRead ?? DBNull.Value);
            var data = await _context.Database
                .SqlQueryRaw<NotificationResponseDto>(sql, userIdParam, isReadParam)
                .ToListAsync();

            return new ResponseList<NotificationResponseDto>(data);
        }
        catch (Exception ex)
        {
            return new ResponseList<NotificationResponseDto>(
                false,
                $"Error retrieving notifications: {ex.Message}",
                new List<NotificationResponseDto>()
            );
        }
    }

    public async Task<Response<int>> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Notification_GetUnreadCount @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var rows = await _context.Database
                .SqlQueryRaw<UnreadCountRow>(sql, userIdParam)
                .ToListAsync();
            var row = rows.FirstOrDefault();

            return new Response<int>(row?.Count ?? 0);
        }
        catch (Exception ex)
        {
            return new Response<int>(false, $"Error getting unread notifications: {ex.Message}", 0);
        }
    }

    public async Task<BaseResponse> MarkAsReadAsync(Guid userId, int notificationId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Notification_MarkAsRead @UserId=@UserId, @NotificationId=@NotificationId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var notificationIdParam = new SqlParameter("@NotificationId", notificationId);
            var rows = await _context.Database
                .SqlQueryRaw<OperationResultRow>(sql, userIdParam, notificationIdParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new BaseResponse(false, $"Notification dengan ID {notificationId} tidak ditemukan");
            }

            return new BaseResponse(result.IsSuccess, result.Message);
        }
        catch (Exception ex)
        {
            return new BaseResponse(false, $"Error mark notification as read: {ex.Message}");
        }
    }

    public async Task<BaseResponse> MarkAllAsReadAsync(Guid userId)
    {
        try
        {
            var sql = $"EXEC {Schema}.sp_Notification_MarkAllAsRead @UserId=@UserId";
            var userIdParam = new SqlParameter("@UserId", userId);
            var rows = await _context.Database
                .SqlQueryRaw<OperationResultRow>(sql, userIdParam)
                .ToListAsync();
            var result = rows.FirstOrDefault();

            if (result == null)
            {
                return new BaseResponse(true, "Tidak ada notification yang belum dibaca");
            }

            return new BaseResponse(result.IsSuccess, result.Message);
        }
        catch (Exception ex)
        {
            return new BaseResponse(false, $"Error mark all notifications as read: {ex.Message}");
        }
    }

    private sealed class CreateResultRow
    {
        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
        public int NotificationId { get; set; }
        public Guid UserId { get; set; }
        public Guid? KnowledgeId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public Guid? RelatedActionBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    private sealed class UnreadCountRow
    {
        public int Count { get; set; }
    }

    private sealed class OperationResultRow
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
