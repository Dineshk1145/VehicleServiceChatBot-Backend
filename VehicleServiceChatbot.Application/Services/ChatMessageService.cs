using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleServiceChatbot.Application.DTOs.ChatMessage;
using VehicleServiceChatbot.Application.Interfaces;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Services;

public class ChatMessageService : IChatMessageService
{
    private readonly IChatMessageRepository _chatMessageRepository;

    public ChatMessageService(IChatMessageRepository chatMessageRepository)
    {
        _chatMessageRepository = chatMessageRepository;
    }

    public async Task<ChatMessageResponseDto> SaveChatMessageAsync(ChatMessageRequestDto request, string botResponse)
    {
        var msg = new ChatMessage
        {
            UserId = request.UserId,
            Message = request.Message,
            Response = botResponse,
            CreatedAt = DateTime.Now
        };

        await _chatMessageRepository.AddChatMessageAsync(msg);

        return new ChatMessageResponseDto
        {
            Id = msg.Id,
            UserId = msg.UserId,
            Message = msg.Message,
            Response = msg.Response,
            CreatedAt = msg.CreatedAt
        };
    }

    public async Task<IEnumerable<ChatMessageResponseDto>> GetChatHistoryAsync(int userId)
    {
        var msgs = await _chatMessageRepository.GetAllChatMessagesAsync();
        return msgs.Where(m => m.UserId == userId).Select(m => new ChatMessageResponseDto
        {
            Id = m.Id,
            UserId = m.UserId,
            Message = m.Message,
            Response = m.Response,
            CreatedAt = m.CreatedAt
        }).OrderBy(m => m.CreatedAt);
    }
}
