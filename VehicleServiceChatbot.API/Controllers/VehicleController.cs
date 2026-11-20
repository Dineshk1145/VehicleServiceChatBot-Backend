using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceChatbot.Application.DTOs.Common;
using VehicleServiceChatbot.Application.DTOs.Vehicle;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VehicleController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehicleController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
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
    public async Task<IActionResult> AddVehicle([FromBody] VehicleRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int currentUserId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        var result = await _vehicleService.AddVehicleAsync(request, currentUserId, isAdmin);
        return CreatedAtAction(nameof(GetVehicleById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllVehicles([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        int currentUserId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        // Support pagination
        if (page.HasValue || pageSize.HasValue)
        {
            var pagination = new PaginationParams
            {
                PageNumber = page ?? 1,
                PageSize = pageSize ?? 10
            };

            var pagedResult = await _vehicleService.GetVehiclesPagedAsync(currentUserId, isAdmin, pagination);
            Response.Headers.Append("X-Total-Count", pagedResult.TotalCount.ToString());
            Response.Headers.Append("X-Page-Number", pagedResult.PageNumber.ToString());
            Response.Headers.Append("X-Page-Size", pagedResult.PageSize.ToString());
            Response.Headers.Append("X-Total-Pages", pagedResult.TotalPages.ToString());
            return Ok(pagedResult);
        }

        var result = await _vehicleService.GetAllVehiclesAsync(currentUserId, isAdmin);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleById(int id)
    {
        int currentUserId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        try
        {
            var result = await _vehicleService.GetVehicleByIdAsync(id, currentUserId, isAdmin);
            if (result == null) return NotFound($"Vehicle with ID {id} not found.");
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int currentUserId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        try
        {
            var result = await _vehicleService.UpdateVehicleAsync(id, request, currentUserId, isAdmin);
            if (result == null) return NotFound($"Vehicle with ID {id} not found.");
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        int currentUserId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        try
        {
            var result = await _vehicleService.DeleteVehicleAsync(id, currentUserId, isAdmin);
            if (!result) return NotFound($"Vehicle with ID {id} not found.");
            return Ok(new { message = "Vehicle deleted successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
