using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleServiceChatbot.Domain.Entities;

namespace VehicleServiceChatbot.Application.Interfaces;

public interface IChatMessageRepository
{
    Task AddChatMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetAllChatMessagesAsync();
}
