using System;
using VehicleServiceChatbot.Domain.Enums;

namespace VehicleServiceChatbot.Domain.Entities
{
    public class Booking
    {
        public string Id { get; set; } = string.Empty; // e.g. BK-128491
        public string Vehicle { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD
        public string Time { get; set; } = string.Empty; // e.g. 10:00 AM
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? Notes { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
