using System;

namespace VehicleServiceChatbot.Domain.Entities;

public class ServiceBooking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int VehicleId { get; set; }

    public string ServiceType { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }

    public string Status { get; set; } = "Pending";

    public decimal Price { get; set; }

    public decimal Tax { get; set; }

    public decimal Discount { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Vehicle? Vehicle { get; set; }
}

