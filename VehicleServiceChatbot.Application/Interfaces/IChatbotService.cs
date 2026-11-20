using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.Chatbot;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IChatbotService
{
    Task<ChatbotResponseDto> AskChatbotAsync(ChatbotRequestDto request);
}
