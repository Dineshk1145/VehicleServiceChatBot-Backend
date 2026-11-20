using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VehicleServiceChatbot.Application.DTOs.ChatMessage;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatMessageController : ControllerBase
{
    private readonly IChatMessageService _chatMessageService;

    public ChatMessageController(IChatMessageService chatMessageService)
    {
        _chatMessageService = chatMessageService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    [HttpPost]
    [EnableRateLimiting("ChatbotLimiter")]
    public async Task<IActionResult> SaveMessage([FromBody] ChatMessageRequestDto request, [FromQuery] string reply)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int currentUserId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        // Security: Bind UserId to authenticated user unless Admin
        if (!isAdmin || request.UserId <= 0)
        {
            request.UserId = currentUserId;
        }

        var result = await _chatMessageService.SaveChatMessageAsync(request, reply ?? string.Empty);
        return Ok(result);
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetChatHistory(int userId)
    {
        int currentUserId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        // Security: Non-admin users cannot access other users' chat logs
        if (!isAdmin && userId != currentUserId)
        {
            return Forbid("Access denied. You can only view your own chat history.");
        }

        var result = await _chatMessageService.GetChatHistoryAsync(userId);
        return Ok(result);
    }
}
