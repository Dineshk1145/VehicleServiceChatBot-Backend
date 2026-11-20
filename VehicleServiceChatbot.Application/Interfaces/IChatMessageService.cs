using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.ChatMessage;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IChatMessageService
{
    Task<ChatMessageResponseDto> SaveChatMessageAsync(ChatMessageRequestDto request, string botResponse);
    Task<IEnumerable<ChatMessageResponseDto>> GetChatHistoryAsync(int userId);
}
