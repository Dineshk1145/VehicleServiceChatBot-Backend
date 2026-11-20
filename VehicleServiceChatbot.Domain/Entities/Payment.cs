 using System;

namespace VehicleServiceChatbot.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public string PaymentId { get; set; } = string.Empty; // e.g. PAY-20260902-83921

    public int BookingId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentStatus { get; set; } = "Completed"; // Completed, Failed

    public string TransactionId { get; set; } = string.Empty; // e.g. TXN-94820192

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public string PaymentMethod { get; set; } = "Credit/Debit Card"; // Card, UPI, NetBanking, etc.

    // Navigation properties
    public ServiceBooking? Booking { get; set; }
    public User? User { get; set; }
}
