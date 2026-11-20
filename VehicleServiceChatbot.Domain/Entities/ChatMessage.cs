using System;

namespace VehicleServiceChatbot.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Response { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
