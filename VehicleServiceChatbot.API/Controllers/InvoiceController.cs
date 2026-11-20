using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceChatbot.Application.Interfaces;

namespace VehicleServiceChatbot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
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

    [HttpGet("booking/{bookingId}/download")]
    public async Task<IActionResult> DownloadInvoicePdf(int bookingId)
    {
        int userId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        try
        {
            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(userId, isAdmin, bookingId);
            string fileName = $"Invoice-Booking-{bookingId}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("booking/{bookingId}/data")]
    public async Task<IActionResult> GetInvoiceData(int bookingId)
    {
        int userId = GetCurrentUserId();
        bool isAdmin = IsAdmin();

        try
        {
            var data = await _invoiceService.GetInvoiceDataAsync(userId, isAdmin, bookingId);
            if (data == null) return NotFound($"Invoice for booking #{bookingId} not found.");
            return Ok(data);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
