using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleServiceChatbot.Application.DTOs.ServiceBooking;

public class ServiceBookingRequestDto
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "VehicleId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid VehicleId is required")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "ServiceType is required")]
    [StringLength(100, ErrorMessage = "ServiceType cannot exceed 100 characters")]
    public string ServiceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "BookingDate is required")]
    public DateTime BookingDate { get; set; }

    [Range(0, 100000, ErrorMessage = "CustomPrice must be between 0 and 100,000")]
    public decimal? CustomPrice { get; set; }

    [Range(0, 50000, ErrorMessage = "Discount must be between 0 and 50,000")]
    public decimal? Discount { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string? Status { get; set; }
}

public class UpdateBookingStatusDto
{
    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = string.Empty;
}
