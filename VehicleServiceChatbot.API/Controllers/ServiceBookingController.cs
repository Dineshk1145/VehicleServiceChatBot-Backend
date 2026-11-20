using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceChatbot.Application.DTOs.Common;
using VehicleServiceChatbot.Application.DTOs.ServiceBooking;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ServiceBookingController : ControllerBase
{
    private readonly IServiceBookingService _bookingService;

    public ServiceBookingController(IServiceBookingService bookingService)
    {
        _bookingService = bookingService;
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
    public async Task<IActionResult> AddBooking([FromBody] ServiceBookingRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int userId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        var result = await _bookingService.AddBookingAsync(request, userId, isAdmin);
        return CreatedAtAction(nameof(GetBookingById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBookings([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        int userId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        // Support pagination
        if (page.HasValue || pageSize.HasValue)
        {
            var pagination = new PaginationParams
            {
                PageNumber = page ?? 1,
                PageSize = pageSize ?? 10
            };

            var pagedResult = await _bookingService.GetBookingsPagedAsync(userId, isAdmin, pagination);
            Response.Headers.Append("X-Total-Count", pagedResult.TotalCount.ToString());
            Response.Headers.Append("X-Page-Number", pagedResult.PageNumber.ToString());
            Response.Headers.Append("X-Page-Size", pagedResult.PageSize.ToString());
            Response.Headers.Append("X-Total-Pages", pagedResult.TotalPages.ToString());
            return Ok(pagedResult);
        }

        var result = await _bookingService.GetAllBookingsAsync(userId, isAdmin);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        int userId = GetCurrentUserId();
        bool isAdmin = IsAdmin();
        try
        {
            var result = await _bookingService.GetBookingByIdAsync(id, userId, isAdmin);
            if (result == null) return NotFound($"Booking with ID {id} not found.");
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBooking(int id, [FromBody] ServiceBookingRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int userId = GetCurrentUserId();
        bool isAdmin = IsAdmin();
        try
        {
            var result = await _bookingService.UpdateBookingAsync(id, request, userId, isAdmin);
            if (result == null) return NotFound($"Booking with ID {id} not found.");
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _bookingService.UpdateBookingStatusAsync(id, request.Status);
            if (result == null) return NotFound($"Booking with ID {id} not found.");
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        int userId = GetCurrentUserId();
        bool isAdmin = IsAdmin();
        try
        {
            var result = await _bookingService.DeleteBookingAsync(id, userId, isAdmin);
            if (!result) return NotFound($"Booking with ID {id} not found.");
            return Ok(new { message = "Booking deleted successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
