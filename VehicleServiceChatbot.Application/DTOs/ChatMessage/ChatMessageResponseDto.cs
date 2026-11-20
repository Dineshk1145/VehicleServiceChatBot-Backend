using System;

namespace VehicleServiceChatbot.Application.DTOs.ChatMessage;

public class ChatMessageResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
