using System;

namespace VehicleServiceChatbot.Domain.Entities
{
    public class ChatSession
    {
        public string SessionId { get; set; } = string.Empty;
        public string CurrentState { get; set; } = "IDLE";
        public string BookingDataJson { get; set; } = "{}"; // JSON serialized temp booking data
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
