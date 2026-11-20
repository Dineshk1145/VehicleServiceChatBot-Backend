using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceChatbot.Application.DTOs;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
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

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto request)
    {
        int userId = GetCurrentUserId();
        try
        {
            var result = await _paymentService.ProcessPaymentAsync(userId, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetPaymentByBookingId(int bookingId)
    {
        int userId = GetCurrentUserId();
        bool isAdmin = IsAdmin();
        try
        {
            var result = await _paymentService.GetPaymentByBookingIdAsync(userId, isAdmin, bookingId);
            if (result == null) return NotFound($"Payment for booking #{bookingId} not found.");
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
