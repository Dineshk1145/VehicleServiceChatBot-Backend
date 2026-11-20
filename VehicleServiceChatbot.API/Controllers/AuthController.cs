using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using VehicleServiceChatbot.Application.DTOs.Auth;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);
        var result = await _authService.RegisterAsync(request);
        _logger.LogInformation("User registered successfully for email: {Email}", request.Email);

        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Login attempt for email: {Email} from IP: {IpAddress}", request.Email, HttpContext.Connection.RemoteIpAddress);
        var result = await _authService.LoginAsync(request);
        _logger.LogInformation("Successful login for email: {Email}, UserId: {UserId}, Role: {Role}", request.Email, result.UserId, result.Role);

        return Ok(result);
    }

    // Security: Only Admins can view all registered users (RBAC enforced)
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();
        return Ok(result);
    }
}
