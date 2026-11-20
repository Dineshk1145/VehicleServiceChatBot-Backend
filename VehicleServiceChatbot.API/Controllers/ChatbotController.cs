using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VehicleServiceChatbot.Application.DTOs.Chatbot;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;

    public ChatbotController(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("ask")]
    [EnableRateLimiting("ChatbotLimiter")]
    public async Task<IActionResult> Ask([FromBody] ChatbotRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Security: Always bind UserId from the authenticated JWT claims to prevent spoofing
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int currentUserId))
        {
            request.UserId = currentUserId;
        }

        var result = await _chatbotService.AskChatbotAsync(request);
        return Ok(result);
    }
}
