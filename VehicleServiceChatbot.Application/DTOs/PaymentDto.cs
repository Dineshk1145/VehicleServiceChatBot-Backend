using System;

namespace VehicleServiceChatbot.Application.DTOs;

public class ProcessPaymentDto
{
    public int BookingId { get; set; }
    public string PaymentMethod { get; set; } = "Credit/Debit Card"; // Card, UPI, NetBanking
    public bool SimulateFailure { get; set; } = false; // For testing failed payment flows
}

public class PaymentResponseDto
{
    public int Id { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
