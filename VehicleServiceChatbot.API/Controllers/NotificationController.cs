using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        int userId = GetCurrentUserId();
        var result = await _notificationService.GetUserNotificationsAsync(userId);

        // Support pagination
        if (page.HasValue || pageSize.HasValue)
        {
            int p = page ?? 1;
            int ps = pageSize ?? 10;
            int total = result.Count();
            var items = result.Skip((p - 1) * ps).Take(ps).ToList();

            Response.Headers.Append("X-Total-Count", total.ToString());
            Response.Headers.Append("X-Page-Number", p.ToString());
            Response.Headers.Append("X-Page-Size", ps.ToString());
            return Ok(new { items, totalCount = total, pageNumber = p, pageSize = ps });
        }

        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        int userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { unreadCount = count });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        int userId = GetCurrentUserId();
        var success = await _notificationService.MarkAsReadAsync(userId, id);
        if (!success) return NotFound("Notification not found or access denied.");
        return Ok(new { message = "Notification marked as read" });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        int userId = GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(new { message = "All notifications marked as read" });
    }
}
