using System.Collections.Generic;

namespace VehicleServiceChatbot.Application.DTOs
{
    public class ChatRequestDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<string> QuickReplies { get; set; } = new();
        public ChatActionDto? Action { get; set; }
        public ChatStateDto State { get; set; } = new();
    }

    public class ChatActionDto
    {
        public string Type { get; set; } = string.Empty; // e.g. "REFRESH_BOOKINGS"
        public string? BookingId { get; set; }
    }

    public class ChatStateDto
    {
        public string CurrentState { get; set; } = "IDLE";
    }
}
