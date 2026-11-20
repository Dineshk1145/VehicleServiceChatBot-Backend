using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(int userId, int? bookingId, string message);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
    Task MarkAllAsReadAsync(int userId);
}
