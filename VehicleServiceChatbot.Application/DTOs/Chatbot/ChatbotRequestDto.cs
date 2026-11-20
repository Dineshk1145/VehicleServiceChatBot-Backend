using System.ComponentModel.DataAnnotations;

namespace VehicleServiceChatbot.Application.DTOs.Chatbot;

public class ChatbotRequestDto
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Message is required")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 1000 characters")]
    public string Message { get; set; } = string.Empty;
}
